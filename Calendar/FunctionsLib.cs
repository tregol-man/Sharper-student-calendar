using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Calendar;
using Microsoft.Maui.Controls;
using System.Reflection;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Diagnostics;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
public static class FunctionsLib
{
    private const string baseURL = "https://sharperserver.onrender.com/";
    private static string filePath = Path.Combine(FileSystem.AppDataDirectory, "auth.json");
    public class AuthData
    {
        public string Token { get; set; } = "";
        public int UserId { get; set; } = -1;
        public override string ToString()
        {
            return $"Token: {Token}, UserId: {UserId}";
        }
    }
    public static AuthData LoadToken()
    {
        try
        {
            if (!File.Exists(filePath)) { Debug.WriteLine($"Error"); return new AuthData(); }
            string json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<AuthData>(json) ?? new AuthData();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading token: {ex.Message}");
            return new AuthData();
        }
    }
    public static void ClearToken()
    {
        try
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
            Debug.WriteLine("Token cleared.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error clearing token: {ex.Message}");
        }
    }
    public static void SaveToken(string token, int userId)
    {
        try
        {
            var authData = new AuthData { Token = token, UserId = userId };
            string json = JsonConvert.SerializeObject(authData);
            File.WriteAllText(filePath, json);

            Debug.WriteLine("Token saved successfully at"+ filePath);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error saving token: {ex.Message}");
        }
    }
    public static bool RefreshToken()
    {
        using (HttpClient client = CreateAuthorizedClient())
        {
            string url = $"{baseURL}user/login/token";

            HttpResponseMessage response = client.PostAsync(url, new StringContent("")).Result;
            string responseContent = response.Content.ReadAsStringAsync().Result;

            if (!response.IsSuccessStatusCode)
            {
                ClearToken();
                return false;
            }

            var responseData = JsonConvert.DeserializeObject<dynamic>(responseContent);
            string newToken = responseData?.data?.token ?? "";

            if (!string.IsNullOrEmpty(newToken))
            {
                var authData = LoadToken();
                Console.WriteLine("saving token");
                SaveToken(newToken, authData.UserId);
                return true;
            }

            ClearToken();
            return false;
        }
    }
    private static HttpClient CreateAuthorizedClient()
    {
        var authData = LoadToken();
        if (string.IsNullOrEmpty(authData.Token))
        {
            Debug.WriteLine("Token is empty. Authorization failed.");
            RedirectToLogin();
            return null; // Handle accordingly
        }

        HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Add("Auth", authData.Token);

        return client;
    }
    public static int RegisterUser(string userName, string googleId, string email)
    {
        using (HttpClient client = new HttpClient())
        {
            string url = $"{baseURL}user/register";

            var requestData = new
            {
                user_name = userName,
                google_id = googleId,
                email = email
            };

            string json = JsonConvert.SerializeObject(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            Debug.WriteLine($"Request URL: {url}");
            Debug.WriteLine($"Request JSON: {json}");
            HttpResponseMessage response = client.PostAsync(url, content).Result;
            string responseContent = response.Content.ReadAsStringAsync().Result;
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                {
                    Debug.WriteLine("User already exists. Attempting login...");
                    return LoginUser(googleId);
                }
                Debug.WriteLine($"Error registering user: {response.ReasonPhrase}");
                return -1; // Return -1 to indicate failure
            }

            var responseData = JsonConvert.DeserializeObject<dynamic>(responseContent);
            LoginUser(googleId);
            return responseData?.data ?? -1; // Return user ID if successful, otherwise -1
        }
    }
    public static int LoginUser(string googleId)
    {
        using (HttpClient client = new HttpClient())
        {
            string url = $"{baseURL}user/login";

            var requestData = new
            {
                google_id = googleId
            };

            string json = JsonConvert.SerializeObject(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = client.PostAsync(url, content).Result;
            string responseContent = response.Content.ReadAsStringAsync().Result;

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"Error logging in: {response.StatusCode} - {response.ReasonPhrase}");
                Debug.WriteLine($"Response: {responseContent}");
                return -1; // Return -1 to indicate failure
            }

            var responseData = JsonConvert.DeserializeObject<dynamic>(responseContent);
            int userId = responseData?.data?.user_id ?? -1;
            string token = responseData?.data?.token ?? "";
            if (userId != -1 && !string.IsNullOrEmpty(token))
                Console.WriteLine("trying to save "+ token);
                SaveToken(token, userId);

            return userId;
        }
    }
    public static void LogoutUser()
    {
        ClearToken();
        Debug.WriteLine("User logged out successfully.");
        RedirectToLogin(); 
    }
    public static bool AutoLogin()
    {
        var authData = LoadToken();
        if (authData.UserId == -1 || string.IsNullOrEmpty(authData.Token))
            return false;

        return RefreshToken();
    }
    public static int CreateGroup(string groupName)
    {
        using (HttpClient client = CreateAuthorizedClient())
        {
            if (client == null)
            {
                Debug.WriteLine("Authorization failed. Redirecting to login...");
                return -1; // User is already being redirected
            }
            string url = $"{baseURL}group/";

            var requestData = new { group_name = groupName };
            string json = JsonConvert.SerializeObject(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = client.PostAsync(url, content).Result;
            string responseContent = response.Content.ReadAsStringAsync().Result;

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"Error creating group: {response.StatusCode} - {response.ReasonPhrase}");
                return -1; // Failure
            }

            var responseData = JsonConvert.DeserializeObject<dynamic>(responseContent);
            Console.WriteLine(responseData);
            return responseData?.data ?? -1; // Return the group ID or -1 if failed
        }
    }

    public static string GetGroupByCode(string groupCode)
    {
        using (HttpClient client = CreateAuthorizedClient())
        {
            if (client == null)
            {
                Debug.WriteLine("Authorization failed. Redirecting to login...");
                return null; // User is already being redirected
            }
            string url = $"{baseURL}group/code/{groupCode}";

            HttpResponseMessage response = client.GetAsync(url).Result;
            string responseContent = response.Content.ReadAsStringAsync().Result;

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"Error fetching group by code: {response.StatusCode} - {response.ReasonPhrase}");
                return ""; // Failure
            }

            var responseData = JsonConvert.DeserializeObject<dynamic>(responseContent);
            return responseData?.data?.group_id ?? ""; // Return group ID as string or empty
        }
    }

    public static string GetGroupCode(int groupId)
    {
        using (HttpClient client = CreateAuthorizedClient())
        {
            if (client == null)
            {
                Debug.WriteLine("Authorization failed. Redirecting to login...");
                return null; // User is already being redirected
            }
            string url = $"{baseURL}group/{groupId}/code";

            HttpResponseMessage response = client.GetAsync(url).Result;
            string responseContent = response.Content.ReadAsStringAsync().Result;

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"Error fetching group code: {response.StatusCode} - {response.ReasonPhrase}");
                return ""; // Failure
            }

            var responseData = JsonConvert.DeserializeObject<dynamic>(responseContent);
            return responseData?.data ?? ""; // Return the group code or empty
        }
    }
    public static int JoinGroup(string groupCode)
    {
        using (HttpClient client = CreateAuthorizedClient())
        {
            if (client == null)
            {
                Debug.WriteLine("Authorization failed. Redirecting to login...");
                return -1; // User is already being redirected
            }

            string url = $"{baseURL}group/code/{groupCode}";

            // Create a POST request with an empty body
            HttpContent content = new StringContent("{}", Encoding.UTF8, "application/json");
            HttpResponseMessage response = client.PostAsync(url, content).Result;
            string responseContent = response.Content.ReadAsStringAsync().Result;

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"Error joining group: {response.StatusCode} - {response.ReasonPhrase}");
                return -1; // Failure
            }

            var responseData = JsonConvert.DeserializeObject<dynamic>(responseContent);
            return responseData?.data ?? -1; // Return group ID or -1 if failed
        }
    }
    public static int DeleteGroup(int groupId)
    {
        using (HttpClient client = CreateAuthorizedClient())
        {
            if (client == null)
            {
                Debug.WriteLine("Authorization failed. Redirecting to login...");
                return -1; // User is already being redirected
            }
            string url = $"{baseURL}group/{groupId}";

            var request = new HttpRequestMessage(HttpMethod.Delete, url);
            HttpResponseMessage response = client.SendAsync(request).Result;
            string responseContent = response.Content.ReadAsStringAsync().Result;

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"Error deleting group: {response.StatusCode} - {response.ReasonPhrase}");
                return -1; // Return -1 if deletion fails
            }

            var responseData = JsonConvert.DeserializeObject<dynamic>(responseContent);
            return responseData?.data ?? -1; // Return deleted group ID, or -1 if failed
        }
    }
    public static int LeaveGroup(int groupId)
    {
        using (HttpClient client = CreateAuthorizedClient())
        {
            if (client == null)
            {
                Debug.WriteLine("Authorization failed. Redirecting to login...");
                return -1; // User is already being redirected
            }
            string url = $"{baseURL}group/{groupId}/members";

            HttpResponseMessage response = client.DeleteAsync(url).Result;
            string responseContent = response.Content.ReadAsStringAsync().Result;

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"Error leaving group: {response.StatusCode} - {response.ReasonPhrase}");
                return -1; // Failure
            }

            var responseData = JsonConvert.DeserializeObject<dynamic>(responseContent);
            return responseData?.data ?? -1; // Return group ID or -1 if failed
        }
    }
    public static UserData GetUserData()
    {
        using (HttpClient client = CreateAuthorizedClient())
        {
            if (client == null)
            {
                Debug.WriteLine("Authorization failed. Redirecting to login...");
                return null; // User is already being redirected
            }
            string url = $"{baseURL}user/me";

            HttpResponseMessage response = client.GetAsync(url).Result;
            string responseContent = response.Content.ReadAsStringAsync().Result;
            Console.WriteLine(responseContent);

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"Error fetching user data: {response.StatusCode} - {response.ReasonPhrase}");
                return null;
            }

            var responseObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseContent);

            // Get the 'data' object and deserialize it into UserData
            var userDataJson = responseObject["data"].ToString();
            var responseData = JsonConvert.DeserializeObject<UserData>(userDataJson);
            return responseData;
        }
    }
    public static int CreateEvent(string json, int groupId)
    {
        using (HttpClient client = CreateAuthorizedClient())
        {
            if (client == null)
            {
                Debug.WriteLine("Authorization failed. Redirecting to login...");
                return -1; // User is already being redirected
            }
            string url = $"{baseURL}group/{groupId}/event";

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = client.PostAsync(url, content).Result;
            string responseContent = response.Content.ReadAsStringAsync().Result;

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"Error creating event in: {response.StatusCode} - {response.ReasonPhrase}");
                Debug.WriteLine($"Response: {responseContent}");
                return -1; // Return -1 to indicate failure
            }

            var responseData = JsonConvert.DeserializeObject<dynamic>(responseContent);
            return responseData?.data ?? -1; // Return user ID if successful, otherwise -1
        }
    }
    public static int UpdateEvent(string json, int groupId, int eventId)
    {
        using (HttpClient client = CreateAuthorizedClient())
        {
            if (client == null)
            {
                Debug.WriteLine("Authorization failed. Redirecting to login...");
                return -1; // User is already being redirected
            }
            string url = $"{baseURL}group/{groupId}/event/{eventId}";

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(new HttpMethod("PATCH"), url) { Content = content };
            HttpResponseMessage response = client.SendAsync(request).Result;
            string responseContent = response.Content.ReadAsStringAsync().Result;

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"Error updating event: {response.StatusCode} - {response.ReasonPhrase}");
                Debug.WriteLine($"Response: {responseContent}");
                return -1; // Return -1 to indicate failure
            }

            var responseData = JsonConvert.DeserializeObject<dynamic>(responseContent);
            return responseData?.data ?? -1; // Return event ID if successful, otherwise -1
        }
    }
    public static List<Calendar.EventInfo> LoadMonthEvents(DateTime first, DateTime last, int id)
    {
        using (HttpClient client = CreateAuthorizedClient())
        {
            if (client == null)
            {
                Debug.WriteLine("Authorization failed. Redirecting to login...");
                return null; // User is already being redirected
            }
            string firstStr = first.ToString("yyyy-MM-dd");
            string lastStr = last.ToString("yyyy-MM-dd");
            string url = $"{baseURL}group/{id}/event/range/{firstStr}/{lastStr}";

            HttpResponseMessage response = client.GetAsync(url).Result;
            string responseContent = response.Content.ReadAsStringAsync().Result;
            var jsonObject = JsonConvert.DeserializeObject<JObject>(responseContent);
            Console.WriteLine(jsonObject.ToString());

            var eventList = jsonObject["data"]?.ToObject<List<Calendar.EventInfo>>() ?? new List<Calendar.EventInfo>();
            return eventList;
        }
    }

    public static List<Calendar.EventInfo> LoadDateEvents(DateTime date, int id)
    {
        Console.WriteLine(date + " " + id);
        using (HttpClient client = CreateAuthorizedClient())
        {
            if (client == null)
            {
                Debug.WriteLine("Authorization failed. Redirecting to login...");
                return null; // User is already being redirected
            }
            string dateStr = date.ToString("yyyy-MM-dd");
            string url = $"{baseURL}group/{id}/event/day/{dateStr}";

            HttpResponseMessage response = client.GetAsync(url).Result;
            string responseContent = response.Content.ReadAsStringAsync().Result;
            var jsonObject = JsonConvert.DeserializeObject<JObject>(responseContent);
            Console.WriteLine(jsonObject.ToString());

            var eventList = jsonObject["data"]?.ToObject<List<Calendar.EventInfo>>() ?? new List<Calendar.EventInfo>();
            return eventList;
        }
    }

    public static List<Calendar.EventInfo> LoadAllEvents(int id)
    {
        Console.WriteLine(id + " all events");
        try
        {
            using (HttpClient client = CreateAuthorizedClient())
            {
                string url = $"{baseURL}group/{id}/event";
                HttpResponseMessage response = client.GetAsync(url).Result;

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to fetch events: {response.ReasonPhrase}");
                }

                string responseContent = response.Content.ReadAsStringAsync().Result;

                if (responseContent == "missing valid token")
                {
                    return new List<Calendar.EventInfo>();
                }

                var jsonObject = JsonConvert.DeserializeObject<JObject>(responseContent);
                Console.WriteLine(jsonObject.ToString());

                var eventList = jsonObject["data"]?.ToObject<List<Calendar.EventInfo>>() ?? new List<Calendar.EventInfo>();
                Console.WriteLine(eventList[0].event_date);
                return eventList;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading events: {ex.Message}");
            return new List<Calendar.EventInfo>(); // Fallback to an empty list
        }
    }

    public static Calendar.EventInfo? LoadSingleEvent(int groupId, int eventId)
    {
        Console.WriteLine(groupId + " " + eventId);
        try
        {
            using (HttpClient client = CreateAuthorizedClient())
            {
                if (client == null)
                {
                    Debug.WriteLine("Authorization failed. Redirecting to login...");
                    return null; // User is already being redirected
                }
                string url = $"{baseURL}group/{groupId}/event/{eventId}";
                HttpResponseMessage response = client.GetAsync(url).Result;

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to fetch event: {response.ReasonPhrase}");
                }

                string responseContent = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine(responseContent);
                if (responseContent == "missing valid token")
                {
                    return null; // Return null to indicate no event was loaded
                }
                var jsonObject = JsonConvert.DeserializeObject<JObject>(responseContent);

                // Extract the "data" part of the response
                var eventInfo = jsonObject["data"]?.ToObject<Calendar.EventInfo>();

                if (eventInfo != null)
                {
                    // Return the deserialized EventInfo object
                    return eventInfo;
                }

                return null;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading event: {ex.Message}");
            return null; // Fallback to null for error handling
        }
    }
    public static int CreateSubject(int groupId, string subjectName, int subjectHue)
    {
        using (HttpClient client = CreateAuthorizedClient())
        {
            if (client == null)
            {
                Debug.WriteLine("Authorization failed. Redirecting to login...");
                return -1; // User is already being redirected
            }
            string url = $"{baseURL}group/{groupId}/subject";
            var requestBody = new
            {
                subject_name = subjectName,
                subject_hue = subjectHue
            };

            StringContent content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

            HttpResponseMessage response = client.PostAsync(url, content).Result;
            string responseContent = response.Content.ReadAsStringAsync().Result;

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"Error creating subject: {response.StatusCode} - {response.ReasonPhrase}");
                return -1; // Return -1 if subject creation fails
            }

            var responseData = JsonConvert.DeserializeObject<dynamic>(responseContent);
            return responseData?.data ?? -1; // Return new subject ID
        }
    }
    public static List<SubjectData> GetGroupSubjects(int groupId)
    {
        using (HttpClient client = CreateAuthorizedClient())
        {
            if (client == null)
            {
                Debug.WriteLine("Authorization failed. Redirecting to login...");
                return null; // User is already being redirected
            }
            string url = $"{baseURL}group/{groupId}/subject";

            HttpResponseMessage response = client.GetAsync(url).Result;
            string responseContent = response.Content.ReadAsStringAsync().Result;

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"Error fetching subjects: {response.StatusCode} - {response.ReasonPhrase}");
                return new List<SubjectData>(); // Return empty list on failure
            }

            var responseData = JsonConvert.DeserializeObject<dynamic>(responseContent);

            if (responseData?.data == null || !(responseData.data is JArray))
            {
                return new List<SubjectData>(); // Ensure data is an array
            }

            List<SubjectData> subjects = new List<SubjectData>();
            foreach (var subject in responseData.data)
            {
                subjects.Add(new SubjectData
                {
                    Id = subject.subject_id,
                    Name = subject.subject_name,
                    Hue = subject.subject_hue
                });
            }

            return subjects;
        }
    }
    public static int DeleteSubject(int groupId, int subjectId)
    {
        using (HttpClient client = CreateAuthorizedClient())
        {
            if (client == null)
            {
                Debug.WriteLine("Authorization failed. Redirecting to login...");
                return -1; // User is already being redirected
            }
            string url = $"{baseURL}group/{groupId}/subject/{subjectId}";

            HttpResponseMessage response = client.DeleteAsync(url).Result;
            string responseContent = response.Content.ReadAsStringAsync().Result;

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"Error deleting subject: {response.StatusCode} - {response.ReasonPhrase}");
                return -1; // Return -1 if deletion fails
            }

            var responseData = JsonConvert.DeserializeObject<dynamic>(responseContent);
            return responseData?.data ?? -1; // Return deleted subject ID
        }
    }
    public static void OnDayTapped(DateTime date)
    {
        var formattedDate = date.ToString("yyyy-MM-dd");
        Shell.Current.GoToAsync($"datepage?date={formattedDate}");
    }
    public static void OnEventTapped(int eventId)
    {
        Shell.Current.GoToAsync($"eventpage?eventId={eventId}");
    }
    public static void RedirectToLogin()
    {
        Debug.WriteLine("Redirecting user to login...");
        Application.Current.MainPage = new LoginPage();
    }
}
