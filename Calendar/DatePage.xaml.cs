using Microsoft.Maui.Controls;
using Newtonsoft.Json;

namespace Calendar;

public partial class DatePage : ContentPage, IQueryAttributable
{
    public DatePage()
    {
        InitializeComponent();
        (_events, _subjects, _groups) = FunctionsLib.LoadEvents();
    }

    private List<EventInfo> _events;
    private List<SubjectData> _subjects;
    private List<GroupData> _groups;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("date", out var dateString))
        {
            Console.WriteLine($"Received date string: {dateString}");
            try
            {
                // Parse the date from the query parameter
                if (DateTime.TryParseExact(dateString.ToString(), "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var date))
                {
                    // Display the selected date
                    DateLabel.Text = $"Selected Date: {date.ToString("MM/dd/yyyy")}";

                    // Find events tied to this date
                    var eventsForDate = _events.Where(e => e.DueDate.Date == date.Date).ToList();

                    // Clear previous events
                    EventsStackLayout.Children.Clear();

                    if (eventsForDate.Any())
                    {
                        var sortedEvents = eventsForDate.OrderBy(e => e.DueDate).ToList();
                        DateTime? lastEventDate = null;
                        bool useBlueBackground = true;

                        foreach (var eventInfo in sortedEvents)
                        {
                            // Create a new grid for the event
                            var eventGrid = new Grid
                            {
                                ColumnDefinitions =
                                {
                                    new ColumnDefinition { Width = new GridLength(100, GridUnitType.Star) }
                                },
                                Margin = new Thickness(10, 5),
                            };

                            // Date label
                            // Alternate background colors
                            lastEventDate = eventInfo.DueDate.Date;
                            useBlueBackground = !useBlueBackground;

                            var detailsGrid = new Grid
                            {
                                BackgroundColor = useBlueBackground ? Colors.LightBlue : Colors.LightGreen,
                                RowDefinitions =
                                {
                                    new RowDefinition { Height = GridLength.Auto },
                                    new RowDefinition { Height = GridLength.Auto }
                                },
                                Margin = new Thickness(10, 5),
                                Padding = new Thickness(10, 0, 0, 0)
                            };

                            // Add tap gesture to the details grid
                            TapGestureRecognizer tapGestureEvent = new TapGestureRecognizer
                            {
                                Command = new Command(() => FunctionsLib.OnEventTapped(eventInfo.Id))
                            };
                            detailsGrid.GestureRecognizers.Add(tapGestureEvent);

                            // Event name
                            var nameLabel = new Label
                            {
                                Text = eventInfo.Name,
                                FontAttributes = FontAttributes.Bold,
                                Margin = new Thickness(0, 0, 0, 2)
                            };
                            Grid.SetRow(nameLabel, 0);
                            Grid.SetColumn(nameLabel, 0);
                            detailsGrid.Children.Add(nameLabel);

                            // Event subject
                            var subjectName = _subjects.FirstOrDefault(s => s.Id == eventInfo.SubjectId)?.Name ?? "Unknown Subject";
                            var subjectLabel = new Label
                            {
                                Text = subjectName,
                                FontAttributes = FontAttributes.Italic,
                                TextColor = Colors.Gray
                            };
                            Grid.SetRow(subjectLabel, 1);
                            Grid.SetColumn(subjectLabel, 0);
                            detailsGrid.Children.Add(subjectLabel);

                            // Add details grid to the main event grid
                            eventGrid.Children.Add(detailsGrid);

                            // Add the event grid to the main layout
                            EventsStackLayout.Children.Add(eventGrid);
                        }
                    }
                    else
                    {
                        // Display no events message
                        EventsStackLayout.Children.Add(new Label
                        {
                            Text = "No events for this date.",
                            FontSize = 18,
                            Margin = new Thickness(0, 5),
                            HorizontalOptions = LayoutOptions.Center
                        });
                    }
                }
                else
                {
                    DateLabel.Text = "Invalid date format.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching event data: {ex.Message}");
                DateLabel.Text = "Error loading events.";
            }
        }
    }
}
