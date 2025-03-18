namespace Calendar;

public partial class JoinGroup : ContentPage
{
	public JoinGroup()
	{
		InitializeComponent();
	}

	private void OnJoinGroupClick(object sender, EventArgs e)
	{
		string groupCode = GroupCodeEntry.Text;

		string groupId = FunctionsLib.GetGroupByCode(groupCode);

        Console.WriteLine(FunctionsLib.JoinGroup(groupCode));

        Application.Current.MainPage = new AppShell();
        
    }
}