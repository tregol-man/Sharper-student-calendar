using System.Diagnostics;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Calendar;

public partial class Administration : ContentPage
{
    private GroupData _group;
    private List<SubjectData> _subjects;
    private UserData _user;

    public Administration()
	{
		InitializeComponent();
        UpdateUser();
        UpdateGroupCode();
	}

    private void UpdateGroupCode()
    {
        ClassCodeButton.Text = FunctionsLib.GetGroupCode(_group.group_id);
    }

    private void OnEditGroupClick(object sender, EventArgs e)
    {
        Shell.Current.GoToAsync($"createClassPage?edit={_group.group_id}");
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

    private async void OnCopyCodeClick(object sender, EventArgs e)
    {
        string text = ClassCodeButton.Text;
        await Clipboard.SetTextAsync(text);
        await DisplayAlert("Kopírováno!", "Text byl uložen do schránky.", "OK");
    
    }
}