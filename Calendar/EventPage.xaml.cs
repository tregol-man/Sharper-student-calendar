using Microsoft.Maui.Controls;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Calendar;

public partial class EventPage : ContentPage, IQueryAttributable
{
    public EventPage()
    {
        InitializeComponent();
    }
    private List<SubjectData> _subjects = new List<SubjectData>();

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("eventId", out var id))
        {
            try
            {
                // Convert eventId to an integer
                int eventId = Convert.ToInt32(id);
                // Fetch the event from the loaded events list
                var eventInfo = FunctionsLib.LoadSingleEvent(1, eventId);
                if (eventInfo == null)
                {
                    EventNameLabel.Text = "Event not found";
                    EventDetailsLabel.Text = "No details available for this event.";
                    SubjectLabel.Text = string.Empty;
                    return;
                }

                // Fetch the subject name
                var subjectName = _subjects.FirstOrDefault(s => s.Id == eventInfo.SubjectId)?.Name ?? "Unknown Subject";

                // Update the labels with event details
                EventNameLabel.Text = eventInfo.Name;
                DateLabel.Text = $"Due date: {eventInfo.DueDateDatetime().ToShortDateString()}";
                SubjectLabel.Text = subjectName;
                EventDetailsLabel.Text = eventInfo.Description;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching event data: {ex.Message}");
                EventNameLabel.Text = "Error loading event";
                EventDetailsLabel.Text = "Could not load event details.";
            }
        }
    }
}