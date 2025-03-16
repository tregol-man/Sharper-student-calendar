using Microsoft.Maui.Controls;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Calendar;

public partial class EventPage : ContentPage, IQueryAttributable
{
    private List<SubjectData> _subjects = new List<SubjectData>();
    private GroupData _group;
    private UserData _user;
    private EventInfo _currentEvent;
    public EventPage()
    {
        InitializeComponent();
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
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("eventId", out var id))
        {
            try
            {
                // Convert eventId to an integer
                int eventId = Convert.ToInt32(id);
                // Fetch the event from the loaded events list
                _currentEvent = FunctionsLib.LoadSingleEvent(1, eventId);
                Debug.WriteLine($"User data: {JsonConvert.SerializeObject(_currentEvent, Formatting.Indented)}");
                if (_currentEvent == null)
                {
                    EventNameLabel.Text = "Event not found";
                    EventDetailsLabel.Text = "No details available for this event.";
                    SubjectLabel.Text = string.Empty;
                    return;
                }

                // Fetch the subject name
                var subjectName = _subjects.FirstOrDefault(s => s.Id == _currentEvent.subject_id)?.Name ?? "Unknown Subject";

                // Update the labels with event details
                EventNameLabel.Text = _currentEvent.event_name;
                DateLabel.Text = $"Due date: {_currentEvent.event_date.ToShortDateString()}";
                SubjectLabel.Text = subjectName;
                EventDetailsLabel.Text = _currentEvent.event_description;
                if (_currentEvent.creator_id == _user.user_id || _group.level > 1)
                {
                    EditButton.IsEnabled = true;
                    EditButton.IsVisible = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching event data: {ex.Message}");
                EventNameLabel.Text = "Error loading event";
                EventDetailsLabel.Text = "Could not load event details.";
            }
        }
    }

    private void EditButton_Clicked(object sender, EventArgs e)
    {
        Console.WriteLine("edit");
        if (_currentEvent == null) return;

        string navigationParams = $"createevent?eventId={_currentEvent.event_id}" +
                                  $"&name={Uri.EscapeDataString(_currentEvent.event_name)}" +
                                  $"&date={_currentEvent.event_date:yyyy-MM-dd}" +
                                  $"&description={Uri.EscapeDataString(_currentEvent.event_description)}" +
                                  $"&subjectId={_currentEvent.subject_id}";

        Shell.Current.GoToAsync(navigationParams);
    }
}