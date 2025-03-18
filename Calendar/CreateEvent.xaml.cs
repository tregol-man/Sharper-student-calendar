using System.Text.Json;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Views;

namespace Calendar
{
    public partial class CreateEvent : ContentPage, IQueryAttributable
    {
        private List<SubjectData> _subjects;
        private GroupData _group;
        private UserData _user;
        private int _eventId = -1;
        public CreateEvent()
        {
            InitializeComponent();
            UpdateUser();
        }
        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("date", out var dateString))
                DateButton.Text = dateString.ToString();

            if (query.TryGetValue("eventId", out var id))
            {
                _eventId = Convert.ToInt32(id);
                EventNameLabel.Text = "Edit event";

                if (query.TryGetValue("name", out var name))
                    EventNameEntry.Text = Uri.UnescapeDataString(name.ToString());

                if (query.TryGetValue("description", out var description))
                    EventDetailsEditor.Text = Uri.UnescapeDataString(description.ToString());

                if (query.TryGetValue("subjectId", out var subjectIdStr) && int.TryParse(subjectIdStr.ToString(), out int subjectId))
                {
                    var subject = _subjects.FirstOrDefault(s => s.Id == subjectId);
                    SubjectsButton.Text = subject?.Name ?? "Unknown Subject";

                    // Set the color of the SubjectsButton based on the selected subject
                    if (subject != null)
                    {
                        var subjectColor = FunctionsLib.GetColorFromSubject(subject.Hue);
                        SubjectsButton.BackgroundColor = subjectColor;
                    }
                }
            }
        }

        private void OnSaveButtonClicked(object sender, EventArgs e)
        {
            string eventName = EventNameEntry.Text?.Trim();
            string eventDate = DateButton.Text?.Trim();
            string eventDescription = EventDetailsEditor.Text?.Trim();
            string selectedSubjectName = SubjectsButton.Text?.Trim();
            int subjectId = 0;
            try
            {
                subjectId = _subjects.FirstOrDefault(s => s.Name == selectedSubjectName).Id;
            }catch (Exception ex) { }

            // Validate required fields
            bool isValid = !string.IsNullOrEmpty(eventName) &&
                           DateTime.TryParseExact(eventDate, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out _) &&
                           !string.IsNullOrEmpty(eventDescription);

            // Create JSON object
            var eventData = new
            {
                event_name = eventName,
                event_date = eventDate,
                event_description = eventDescription,
                subject_id = subjectId
            };

            string json = JsonConvert.SerializeObject(eventData, Formatting.Indented);
            Debug.WriteLine(json);

            // Print validation result
            Debug.WriteLine(isValid ? "OK" : "ERROR");
            if (isValid)
            {
                if (_eventId == -1)
                {
                    Console.WriteLine(FunctionsLib.CreateEvent(json, 1));
                }
                else
                {
                    Console.WriteLine(FunctionsLib.UpdateEvent(json, 1, _eventId));
                }
                Shell.Current.GoToAsync("..");
            }
            else
            {
                DisplayAlert("Error", "Please fill in all the required fields correctly.", "OK");
            }
        }

        private void SubjectsButton_Clicked(object sender, EventArgs e)
        {
            var subjectsWithUntagged = new List<SubjectData>
            {
                new SubjectData { Name = "Untagged", Id = -1 } // Add the extra object
            };

            // Append the existing subjects to the new list
             if (_subjects != null)
             {
                subjectsWithUntagged.AddRange(_subjects); // Add the subjects to the list if not null
             }

            // Convert the updated list to an ObservableCollection
            ObservableCollection<SubjectData> SubjectsCollection = new ObservableCollection<SubjectData>(subjectsWithUntagged);

            var popup = new SubjectsPopup(SubjectsCollection);
            popup.Closed += (s, args) =>
            {
                if (args.Result is SubjectData selectedSubject)
                {
                    SubjectsButton.Text = selectedSubject.Name;
                    var subjectColor = FunctionsLib.GetColorFromSubject(selectedSubject.Hue);
                    SubjectsButton.BackgroundColor = subjectColor;
                }
            };

            Shell.Current.ShowPopup(popup);
        }

        private void DateButton_Clicked(object sender, EventArgs e)
        {
            var datePickerPopup = new DatePickerPopup();
            datePickerPopup.Closed += (s, args) =>
            {
                if (args.Result is DateTime selectedDate)
                {
                    // Update the DateButton text with the selected date
                    DateButton.Text = selectedDate.ToString("yyyy-MM-dd");
                }
            };
            Shell.Current.ShowPopup(datePickerPopup);
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
}