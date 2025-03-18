using System.Diagnostics;
using CommunityToolkit.Maui.Core.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Shapes;
using Newtonsoft.Json;

namespace Calendar;

public partial class CreateClass : ContentPage
{
	private GroupData _group;
	private List<SubjectData> _subjects; // List subjectù, který bude zobrazován; Úložní meziprostor
	private UserData _user;
    private bool editing = false;

	public CreateClass()
	{
		InitializeComponent();
        UpdateUser();
	}

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("edit", out var groupId))
        {
            _group = _user.groups.FirstOrDefault(item => item.group_id == (int)groupId);
            _subjects = FunctionsLib.GetGroupSubjects((int)groupId);

            _subjects.ForEach(item => { AddSubjectToList(item.Name, item.Id, item.Hue); });

            editing = true;
        }         
    }

    private void ConfirmChanges(object sender, EventArgs e) // Pro uložení zmìn èi pro vytvoøení nový class
	{
        string _groupName = ClassNameEntry.Text;

        if (editing)
        {
            Console.WriteLine("Wheeee");
        }
        else
        {
            int GroupId = FunctionsLib.CreateGroup(_groupName);

            Console.WriteLine(GroupId);

            Console.WriteLine(FunctionsLib.GetGroupCode(GroupId));

            foreach (var item in _subjects)
            {
                int result = FunctionsLib.CreateSubject(GroupId, item.Name, 100);
                Console.WriteLine("Result of creating subject" + result); // Error - Bad request
            }

            UpdateUser();


            if (_user.groups != null && _user.groups.Count > 0)
            {
                Application.Current.MainPage = new AppShell();
            }
        }
    }

    private void OnClickCreateSubject(object sender, EventArgs e) // Uloží do lokálního listu jméno subjectu
	{
		string subjectName = SubjectNameEntry.Text;

        if (_subjects.Count != 0)
        {
            SubjectList.Add(AddSubjectToList(subjectName, _subjects.Last().Id + 1));
        } else
        {
            SubjectList.Add(AddSubjectToList(subjectName, 0));
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

    private Border AddSubjectToList(string name, int id, int hue = -1) // Vytvoøí vizuál pro daný subject
	{
        string _hue;
        if (hue != -1)
        {
            _hue = $"#{hue}";
        }
        else
        {
            _hue = $"#FF8080";
        }

        _subjects.Add(new SubjectData()
        {
            Name = name,
            Id = id,
            Hue = ColorToInt(Color.FromRgba(_hue))
        });

		Grid _grid = new Grid()
        {
            Padding = new Thickness(30, 20)
        };

		_grid.Add(new Label()
		{
			Text = name,
			VerticalOptions = LayoutOptions.Center,
			FontSize = 20,
			FontAttributes = FontAttributes.Bold,
			TextColor = Colors.White,
			FontFamily = "Inter"
		});

		ImageButton button = new ImageButton()
		{
			Source = "cross_icon.png",
			HorizontalOptions = LayoutOptions.End,
			VerticalOptions = LayoutOptions.Center,
			TranslationX = 20,
			ClassId = id.ToString()
        };

		button.Clicked += DeleteSubject;

        _grid.Add(button);

        Console.WriteLine($"Vytvoøen Subject {id}, {editing}");

        return new Border()
		{
			BackgroundColor = Color.FromArgb(_hue),
			HeightRequest = 90,
			StrokeShape = new RoundRectangle()
			{
				CornerRadius = 20
			},
			Content = _grid,
			ClassId = $"Subject{id}"
		};
    } 

    private void DeleteSubject(object sender, EventArgs e) // Vymaže daný subject z listu
    {
        if (sender is ImageButton button)
        {
            int _id = int.Parse(button.ClassId);
            Console.WriteLine($"Kliknuto na tlaèítko s textem: ");

            foreach (var item in _subjects)
            {
                if (item.Id == _id)
                {
                    _subjects.Remove(item);
                    break;
                }
            }

            Border selectedSubject = null;
            foreach (var item in SubjectList)
            {
                if (item is Border border && border.ClassId == $"Subject{_id}")
                {
                    Console.WriteLine($"Subject {_id} was removed");
                    SubjectList.Remove(item);
                    break;
                }
            }
        }
    }

    private int ColorToInt(Color color)
    {
        int a = (int)(color.Alpha * 255);
        int r = (int)(color.Red * 255);
        int g = (int)(color.Green * 255);
        int b = (int)(color.Blue * 255);

        return (a << 24) | (r << 16) | (g << 8) | b;
    }


}