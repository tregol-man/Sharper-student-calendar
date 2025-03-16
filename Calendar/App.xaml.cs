namespace Calendar
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            if (FunctionsLib.AutoLogin())
            {
                var user = FunctionsLib.GetUserData();
                if (user != null && user.groups != null && user.groups.Count > 0)
                {
                    MainPage = new AppShell(); // If the user has groups, show the AppShell
                }
                else
                {
                    MainPage = new JoinCreatePage(); // If the user has no groups, show the JoinCreatePage
                }
            }
            else
            {
                // If not logged in, redirect to the LoginPage
                MainPage = new LoginPage(); // Directly set LoginPage as the main page
            }
        }
    }
}
