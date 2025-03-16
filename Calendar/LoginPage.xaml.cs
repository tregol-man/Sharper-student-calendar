namespace Calendar;

using Google.Apis.Oauth2.v2;
using Google.Apis.Oauth2.v2.Data;
using Google.Apis.Services;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2.Flows;
using System.Diagnostics;
using Newtonsoft.Json;

public partial class LoginPage : ContentPage
{
	private string clientId = "973820326914-6nbldai65vlm0gr636l177b47p7ots1e";
    private GroupData _group;
    private List<SubjectData> _subjects;
    private UserData _user;

    public LoginPage()
	{
        InitializeComponent();
    }

	private void OnGoogleLoginClick(object sender, EventArgs e)
	{
		string userName = "brumbal";
		string email = "brumbal@gmail.com";
        if (FunctionsLib.RegisterUser(userName, clientId, email) != -1)
        {
            UpdateUser();
            if (_user == null)
            {
                // Handle the case where user or groups are null
                Console.WriteLine("User data is null.");
                return;
            }

            if (_user.groups.Count > 0 && _user.groups != null)
            {
                Application.Current.MainPage = new AppShell();
            }
            else
            {
                Application.Current.MainPage = new JoinCreatePage();   
            }

        }

	}

    public async void GetGoogleAccount() // Prozatím nefunkèní
	{
		try
		{
            var flow = new ClientSecrets // Pro spojení s OAuth
			{
				ClientId = clientId
			};

			var credentails = await GoogleWebAuthorizationBroker.AuthorizeAsync( //Pro získání google credencials, ve kterých je i access token
				flow,
				new[] { "email", "profile" },
				"user",
				CancellationToken.None
			);

			var service = new Oauth2Service(new BaseClientService.Initializer()
			{
				HttpClientInitializer = credentails,
				ApplicationName = "Sharper Student Calendar"
			});
			
			var request = service.Userinfo.Get().ExecuteAsync().Result;
			await DisplayAlert("Pøihlášení úspìšné", $"Vítej, {request.Name}!", "OK");
			
			/*
            var authUrl = "https://accounts.google.com/o/oauth2/v2/auth" +
                      $"?client_id={clientId}.apps.googleusercontent.com" +
                      $"&redirect_uri=com.googleusercontent.apps.{clientId}:/oauth2redirect" +
                      "&response_type=code" +
                      "&scope=email%20profile" +
                      "&access_type=offline" +
                      "&code_challenge_method=S256";

            var authResult = await WebAuthenticator.AuthenticateAsync(
                new Uri(authUrl),
                new Uri($"com.googleusercontent.apps.{clientId}:/oauth2redirect"));

            string authCode = authResult.Properties["code"];

            await DisplayAlert("Úspìch", $"Authorization Code: {authCode}", "OK");*/

        }
		catch (Exception ex)
		{
            await DisplayAlert("Chyba", ex.Message, "OK");
        }
	}
    private void UpdateUser()
    {
        _user = FunctionsLib.GetUserData();
        if (_user == null)
        {
            Console.WriteLine("Failed to fetch user data.");
            return;
        }
        Debug.WriteLine($"User data: {JsonConvert.SerializeObject(_user, Formatting.Indented)}");
        if (_user.groups != null && _user.groups.Count > 0)
        {
            _group = _user.groups[0];
            if (_group.group_id != -1)
            {
                Console.WriteLine("Group set properly: " + _group.group_id);
                _subjects = FunctionsLib.GetGroupSubjects(_group.group_id);
            }
            else
            {
                Console.WriteLine("Group is invalid");
            }
        }
        else
        {
            Console.WriteLine("User has no groups.");
            _group = null;
            _subjects = new List<SubjectData>();
        }
    }
}