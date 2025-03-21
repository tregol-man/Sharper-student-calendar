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
using Microsoft.Maui.Controls.PlatformConfiguration;
#if ANDROID
using Android.Content;
using Android.App;
using Android.Content.PM;
#endif
public partial class LoginPage : ContentPage
{
    //983820326914-6nbldai65vlm0gr636l177b47p7ots1e.apps.googleusercontent.com<- Client ID pro Android Auth
    //983820326914-edhneat20eagehms4vj6vq2448vbet7c.apps.googleusercontent.com <- Client ID pro Web Auth
    private string clientId = "983820326914-edhneat20eagehms4vj6vq2448vbet7c.apps.googleusercontent.com";
    private GroupData _group;
    private List<SubjectData> _subjects;
    private UserData _user;

    public LoginPage()
	{
        InitializeComponent();
    }

#if ANDROID

    [Activity(NoHistory = true, LaunchMode = LaunchMode.SingleTop, Exported = true)]
    [IntentFilter(new[] { Intent.ActionView },
              Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
              DataScheme = CALLBACK_SCHEME)]
    public class WebAuthenticationCallbackActivity : Microsoft.Maui.Authentication.WebAuthenticatorCallbackActivity
    {
        const string CALLBACK_SCHEME = "myapp";

    }
#endif
    private void OnGoogleLoginClick(object sender, EventArgs e)
	{

        GetGoogleAccount();
		/*string userName = "brumbal";
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

        }*/
	}

    public async void GetGoogleAccount() // Prozatím nefunkèní
	{
		try
		{
            /*var flow = new ClientSecrets // Pro spojení s OAuth
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
			await DisplayAlert("Pøihlášení úspìšné", $"Vítej, {request.Name}!", "OK");*/
			
			
            var authUrl = "https://accounts.google.com/o/oauth2/auth?client_id={clientId}&redirect_uri=com.googleusercontent.apps.{clientId}:/oauth2redirect&response_type=code&scope=email";

            var authResult = await WebAuthenticator.AuthenticateAsync(
                new Uri(authUrl),
                new Uri($"myapp://"));

            string authCode = authResult.Properties["code"];

            await DisplayAlert("Úspìch", $"Authorization Code: {authCode}", "OK");

        }
		catch (Exception ex)
		{
            await DisplayAlert("Chyba", ex.Message, "OK");
        }
	}
    private void UpdateUser(int groupId = 0)
    {
        _user = FunctionsLib.GetUserData();
        if (_user == null)
        {
            Console.WriteLine("Failed to fetch user data.");
            return;
        }
        Console.WriteLine($"User data: {JsonConvert.SerializeObject(_user, Formatting.Indented)}");
        if (_user.groups != null && _user.groups.Count > 0)
        {
            _group = _user.groups[groupId];
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