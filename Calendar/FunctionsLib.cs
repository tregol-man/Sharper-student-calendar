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
public static class FunctionsLib

{
    public static List<Calendar.EventInfo> LoadMonthEvents(DateTime first, DateTime last, int id)
    {
        using (HttpClient client = new HttpClient())
        {
            int firstDaysSinceEpoch = (int)(first - new DateTime(1970, 1, 1)).TotalDays;
            int lastDaysSinceEpoch = (int)(last - new DateTime(1970, 1, 1)).TotalDays;
            string firstStr = firstDaysSinceEpoch.ToString();
            string lastStr = lastDaysSinceEpoch.ToString();
            Console.WriteLine("TEST" + firstStr + " " + lastStr);
            string url = "https://sharperserver.onrender.com/groups/" + id + "/calendar/" + firstStr + "/" + lastStr;
            HttpResponseMessage response = client.GetAsync(url).Result;
            string responseContent = response.Content.ReadAsStringAsync().Result;
            if (responseContent != "missing valid token")
            {
                var responseData = JsonConvert.DeserializeObject<List<Calendar.EventInfo>>(responseContent);
                return responseData;
            }
            else
            {
                return new List<Calendar.EventInfo>();
            }
        }
    }
    public static List<Calendar.EventInfo> LoadDateEvents(DateTime date, int id)
    {
        using (HttpClient client = new HttpClient())
        {
            int dateSinceEpoch = (int)(date - new DateTime(1970, 1, 1)).TotalDays;
            string datetStr = dateSinceEpoch.ToString("yyyy-MM-dd");
            string url = "https://sharperserver.onrender.com/groups/" + id + "/calendar/" + datetStr;
            HttpResponseMessage response = client.GetAsync(url).Result;
            string responseContent = response.Content.ReadAsStringAsync().Result;
            if (responseContent != "missing valid token")
            {
                var responseData = JsonConvert.DeserializeObject<List<Calendar.EventInfo>>(responseContent);
                return responseData;
            }
            else
            {
                return new List<Calendar.EventInfo>();
            }
        }
    }
    public static List<Calendar.EventInfo> LoadAllEvents(int id)
    {
        try
        {
            using (HttpClient client = new HttpClient())
            {
                string url = "https://sharperserver.onrender.com/groups/" + id + "/events";
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

                return JsonConvert.DeserializeObject<List<Calendar.EventInfo>>(responseContent) ?? new List<Calendar.EventInfo>();
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
        try
        {
            using (HttpClient client = new HttpClient())
            {
                string url = $"https://sharperserver.onrender.com/groups/{groupId}/events/{eventId}";
                HttpResponseMessage response = client.GetAsync(url).Result;

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to fetch event: {response.ReasonPhrase}");
                }

                string responseContent = response.Content.ReadAsStringAsync().Result;

                if (responseContent == "missing valid token")
                {
                    return null; // Return null to indicate no event was loaded
                }

                return JsonConvert.DeserializeObject<Calendar.EventInfo>(responseContent);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading event: {ex.Message}");
            return null; // Fallback to null for error handling
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
}
