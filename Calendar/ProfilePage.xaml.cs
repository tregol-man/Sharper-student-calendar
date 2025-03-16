using Newtonsoft.Json;
using System.Diagnostics;


namespace Calendar
{
    public partial class ProfilePage : ContentPage
    {
        private GroupData _group;
        private List<SubjectData> _subjects;
        private UserData _user;
        public ProfilePage()
        {
            InitializeComponent();
            UpdateUser();
            if (_group.level > 1)
            {
                CreateSubjectButton.IsVisible = true;
                DeleteGroupButton.IsVisible = true;
                CreateSubjectButton.IsEnabled = true;
                DeleteGroupButton.IsEnabled = true;
            }
            else
            {
                CreateSubjectButton.IsVisible = false;
                DeleteGroupButton.IsVisible = false;
                CreateSubjectButton.IsEnabled = false;
                DeleteGroupButton.IsEnabled = false;
            }
        }
        private void OnLogOutClick(object sender, EventArgs e)
        {
            UpdateUser();
            FunctionsLib.LogoutUser();
            UpdateUser();
        }
        private void OnCreateSubjectClick(object sender, EventArgs e)
        {
            UpdateUser();
            Console.WriteLine(FunctionsLib.CreateSubject(_group.group_id, "math", 100));
            UpdateUser();
        }
        private void OnLeaveGroupClick(object sender, EventArgs e)
        {
            UpdateUser();
            Console.WriteLine(FunctionsLib.LeaveGroup(_group.group_id));
            UpdateUser();
            Application.Current.MainPage = new JoinCreatePage();
        }
        private void OnDeleteGroupClick(object sender, EventArgs e)
        {
            UpdateUser();
            Console.WriteLine(FunctionsLib.DeleteGroup(_group.group_id));
            UpdateUser();
            Application.Current.MainPage = new JoinCreatePage();
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
}