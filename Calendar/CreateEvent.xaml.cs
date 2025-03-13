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
        public CreateEvent()
        {
            InitializeComponent();
            _subjects = new List<SubjectData>
            {
                new SubjectData { Id = 1, Name = "Math", Hue = 0 },
                new SubjectData { Id = 2, Name = "Science",  Hue = 1 },
                new SubjectData { Id = 3, Name = "History", Hue = 2 }
            };
        }
        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("date", out var dateString))
            {
                Console.WriteLine(dateString);
                if (DateTime.TryParseExact(dateString.ToString(), "MM/dd/yyyy", null, System.Globalization.DateTimeStyles.None, out var date))
                {
                    // Update the DateButton text
                    DateButton.Text = date.ToString("MM/dd/yyyy");
                }
                else
                {
                    DateButton.Text = "Invalid Date";
                }
            }
            else
            {
                DateButton.Text = "Date";
            }
        }
        private void OnSaveButtonClicked(object sender, EventArgs e)
        {
            // Gather data from input fields
            string eventName = EventNameEntry.Text?.Trim();
            string eventDate = DateButton.Text;
            string eventDescription = EventDetailsEditor.Text?.Trim();
            string selectedSubjectName = SubjectsButton.Text?.Trim();
            int subjectId = _subjects.FirstOrDefault(s => s.Name == selectedSubjectName)?.Id ?? 0;

            // Validate required fields
            bool isValid = !string.IsNullOrEmpty(eventName) &&
                           DateTime.TryParseExact(eventDate, "MM/dd/yyyy", null, System.Globalization.DateTimeStyles.None, out _) &&
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
            if (isValid) {
                Console.WriteLine(FunctionsLib.CreateEvent(json, 1));
            }
        }

        private void SubjectsButton_Clicked(object sender, EventArgs e)
        {
            ObservableCollection<SubjectData>  SubjectsCollection = new ObservableCollection<SubjectData>(_subjects);

            var popup = new SubjectsPopup(SubjectsCollection);
            popup.Closed += (s, args) =>
            {
                if (args.Result is SubjectData selectedSubject)
                {
                    SubjectsButton.Text = selectedSubject.Name;
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
                    DateButton.Text = selectedDate.ToString("MM/dd/yyyy");
                }
            };
            Shell.Current.ShowPopup(datePickerPopup);
        }
    }
}