using Newtonsoft.Json;
using System.Diagnostics;

namespace Calendar;

public partial class JoinCreatePage : ContentPage
{
    private GroupData _group;
    private List<SubjectData> _subjects;
    private UserData _user;
    public JoinCreatePage()
	{
		InitializeComponent();
	}
    private void OnCreateGroupClick(object sender, EventArgs e)
    {
        UpdateUser();
        string groupName = "Test Group"; // Change this to dynamically enter a name if needed
        int GroupId = FunctionsLib.CreateGroup(groupName);
        Console.WriteLine(GroupId);
        UpdateUser();
        if (_user.groups != null && _user.groups.Count > 0)
        {
            Application.Current.MainPage = new AppShell();
        }
    }

    private void OnJoinGroupClick(object sender, EventArgs e)
    {
        UpdateUser();
        string groupCode = FunctionsLib.GetGroupCode(1);
        Console.WriteLine(FunctionsLib.JoinGroup(groupCode));
        UpdateUser();
        if (_user.groups != null && _user.groups.Count > 0)
        {
            Application.Current.MainPage = new AppShell();
        }

    }
    private void OnLogOutClick(object sender, EventArgs e)
    {
        UpdateUser();
        FunctionsLib.LogoutUser();
        UpdateUser();
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