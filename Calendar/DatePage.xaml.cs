using System.Drawing;
using System.Net;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Calendar;

public partial class DatePage : ContentPage, IQueryAttributable
{
    private List<EventInfo> _events;
    private List<SubjectData> _subjects;
    private GroupData _group;
    private UserData _user;
    private DateTime? _selectedDate;
    public DatePage()
    {
        InitializeComponent();
        UpdateUser();
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
                    _selectedDate = date;
                    try
                    {
                        _events = FunctionsLib.LoadDateEvents(date, 1) ?? new List<EventInfo>();
                    }
                    catch (Exception ex)
                    {
                        _events = new List<EventInfo>(); // Fallback to an empty list
                    }
                    // Display the selected date
                    DateLabel.Text = $"Selected Date: {date.ToString("MM/dd/yyyy")}";

                    // Find events tied to this date
                    var eventsForDate = _events.Where(e => e.event_date.Date == date.Date).ToList();

                    // Clear previous events
                    EventsStackLayout.Children.Clear();

                    if (eventsForDate.Any())
                    {
                        var sortedEvents = eventsForDate.OrderBy(e => e.event_date).ToList();
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
                            lastEventDate = eventInfo.event_date.Date;
                            useBlueBackground = !useBlueBackground;

                            var detailsGrid = new Grid
                            {
                                BackgroundColor = Colors.Transparent /*useBlueBackground ? Colors.LightBlue : Colors.LightGreen*/,
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
                                Command = new Command(() => FunctionsLib.OnEventTapped(eventInfo.event_id))
                            };
                            detailsGrid.GestureRecognizers.Add(tapGestureEvent);

                            // Event name
                            var nameLabel = new Label
                            {
                                Text = eventInfo.event_name,
                                FontAttributes = FontAttributes.Bold,
                                Margin = new Thickness(10, 5, 0, 5),
                                FontFamily = "Inter",
                                FontSize = 18,
                                TextColor = Colors.White
                            };
                            Grid.SetRow(nameLabel, 0);
                            Grid.SetColumn(nameLabel, 0);
                            detailsGrid.Children.Add(nameLabel);

                            // Event border, for the edges and the gradient

                            Border borderGrid = new Border
                            {
                                StrokeThickness = 15,
                                Stroke = Colors.Transparent,
                                StrokeShape = new RoundRectangle
                                {
                                    CornerRadius = 15
                                },
                                Background = new LinearGradientBrush
                                {
                                    EndPoint = new Microsoft.Maui.Graphics.Point(1, 0),
                                    GradientStops = new GradientStopCollection
                                    {
                                        new GradientStop { Color = Microsoft.Maui.Graphics.Color.FromArgb("#5E52A0"), Offset = 0},
                                        new GradientStop { Color = Microsoft.Maui.Graphics.Color.FromArgb("#8E80DE"), Offset = 1}
                                    }
                                }
                            };

                            // Event subject
                            var subjectName = _subjects.FirstOrDefault(s => s.Id == eventInfo.subject_id)?.Name ?? "Unknown Subject";
                            var subjectLabel = new Label
                            {
                                Text = subjectName,
                                FontAttributes = FontAttributes.Italic,
                                Margin = new Thickness(10, 0, 0, 10),
                                FontFamily = "Inter",
                                TextColor = Colors.White
                            };
                            Grid.SetRow(subjectLabel, 1);
                            Grid.SetColumn(subjectLabel, 0);
                            detailsGrid.Children.Add(subjectLabel);

                            // Add details grid to the main event grid
                            borderGrid.Content = detailsGrid;

                            eventGrid.Children.Add(borderGrid);

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
    private void CreateEventButton_Clicked(object sender, EventArgs e)
    {
        // Check if the date is set, use it for navigation
        var selectedDate = _selectedDate?.ToString("yyyy-MM-dd") ?? DateTime.Now.ToString("yyyy-MM-dd");

        Console.WriteLine($"Navigating with selected date: {selectedDate}");

        // Navigate to the createevent page with the selected date
        Shell.Current.GoToAsync($"createevent?date={selectedDate}");
    }
}
