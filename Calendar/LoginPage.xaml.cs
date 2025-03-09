namespace Calendar;

using Google.Apis.Oauth2.v2;
using Google.Apis.Oauth2.v2.Data;
using Google.Apis.Services;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2.Flows;

public partial class LoginPage : ContentPage
{
	private string clientId = "983820326914-6nbldai65vlm0gr636l177b47p7ots1e";

	public LoginPage()
	{
        InitializeComponent();
    }

    private void OnGoogleLoginClick(object sender, EventArgs e)
	{
		GetGoogleAccount();
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
}