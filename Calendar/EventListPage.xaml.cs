using System.Text.Json;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;
using Newtonsoft.Json;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.Maui.Controls.Shapes;

namespace Calendar
{
    public partial class EventListPage : ContentPage
    {
        private List<EventInfo> _events;
        private List<SubjectData> _subjects = new List<SubjectData>();
        private GroupData _group;
        private UserData _user;
        public EventListPage()
        {
            InitializeComponent();
            LoadEvents();
            PopulateEventGrid();
            UpdateUser();
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Reload user data and calendar when the page is displayed again
            UpdateUser();
            LoadEvents();
            PopulateEventGrid();
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

        private void LoadEvents()
        {
            try
            {
                _events = FunctionsLib.LoadAllEvents(1) ?? new List<EventInfo>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading events: {ex.Message}");
                _events = new List<EventInfo>(); // Fallback to an empty list
            }
        }
        private void PopulateEventGrid(bool sortByDate = true)
        {
            DynamicContent.Children.Clear();
            IEnumerable<EventInfo> sortedEvents;

            if (sortByDate)
            {
                // Sort events by date
                sortedEvents = _events.OrderBy(e => e.event_date);
            }
            else
            {
                // Sort events by subject name
                sortedEvents = _events
                    .OrderBy(e => _subjects.FirstOrDefault(s => s.Id == e.subject_id)?.Name ?? "Unknown Subject")
                    .ThenBy(e => e.event_name);
            }
            DateTime? lastEventDate = null;
            string lastHeader = null;
            bool useBlueBackground = true;

            foreach (var eventInfo in sortedEvents)
            {
                string currentHeader = sortByDate
                ? eventInfo.event_date.ToString("MMMM yyyy") // Month and year for date sorting
                : _subjects.FirstOrDefault(s => s.Id == eventInfo.subject_id)?.Name ?? "Unknown Subject";

                if (currentHeader != lastHeader)
                {
                    lastHeader = currentHeader;

                    Border headerBorder = new Border
                    {
                        BackgroundColor = Colors.White,
                        StrokeShape = new RoundRectangle
                        {
                            CornerRadius = 15
                        }
                    };

                    var headerLabel = new Label
                    {
                        Text = currentHeader,
                        FontSize = 20,
                        FontAttributes = FontAttributes.Bold,
                        HorizontalOptions = LayoutOptions.Center,
                        TextColor = Color.FromRgba("#4F5171")
                    };

                    headerBorder.Content = headerLabel;

                    DynamicContent.Children.Add(headerBorder);
                }

                var eventGrid = new Grid
                {
                    ColumnDefinitions =
                    {
                        new ColumnDefinition { Width = new GridLength(30, GridUnitType.Star) }, // 30% width for date
                        new ColumnDefinition { Width = new GridLength(70, GridUnitType.Star) }
                    },
                    Margin = new Thickness(10, 5),
                };
                var dateLabel = new Label
                {
                    Text = eventInfo.event_date.ToString("MM/dd/yyyy"),
                    VerticalOptions = LayoutOptions.Center,
                    FontAttributes = FontAttributes.Bold,
                    FontFamily = "Inter",
                    TextColor = Color.FromRgba("#4F5171")
                };
                TapGestureRecognizer tapGestureDate = new TapGestureRecognizer
                {
                    Command = new Command(() => FunctionsLib.OnDayTapped(eventInfo.event_date.Date))
                };
                dateLabel.GestureRecognizers.Add(tapGestureDate);
                Grid.SetRow(dateLabel, 0);
                Grid.SetColumn(dateLabel, 0);
                eventGrid.Children.Add(dateLabel);

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
                TapGestureRecognizer tapGestureEvent = new TapGestureRecognizer
                {
                    Command = new Command(() => FunctionsLib.OnEventTapped(eventInfo.event_id))
                };
                detailsGrid.GestureRecognizers.Add(tapGestureEvent);
                //event name
                var nameLabel = new Label
                {
                    Text = eventInfo.event_name,
                    FontAttributes = FontAttributes.Bold,
                    Margin = new Thickness(0, 0, 0, 2),
                    FontFamily = "Inter",
                    FontSize = 18,
                    TextColor = Colors.White
                };
                Grid.SetRow(nameLabel, 0);
                Grid.SetColumn(nameLabel, 0);
                detailsGrid.Children.Add(nameLabel);

                //description
                var subjectName = _subjects.FirstOrDefault(s => s.Id == eventInfo.subject_id)?.Name ?? "Unknown Subject";
                var subjectLabel = new Label
                {
                    Text = subjectName,
                    FontAttributes = FontAttributes.Italic,
                    Margin = new Thickness(5, 0, 0, 10),
                    FontFamily = "Inter",
                    TextColor = Colors.White
                };
                Grid.SetRow(subjectLabel, 1);
                Grid.SetColumn(subjectLabel, 0);
                detailsGrid.Children.Add(subjectLabel);

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

                // Add the details grid to the main event grid
                borderGrid.Content = detailsGrid;

                Grid.SetRow(borderGrid, 0);
                Grid.SetColumn(borderGrid, 1);

                eventGrid.Children.Add(borderGrid);

                // Add the event grid to the main layout
                DynamicContent.Children.Add(eventGrid);
            }
        }
        private void OnDateSortClicked(object sender, EventArgs e)
        {
            PopulateEventGrid(sortByDate: true); // Sort by date
        }

        private void OnEventSortClicked(object sender, EventArgs e)
        {
            PopulateEventGrid(sortByDate: false); // Sort by subject
        }

        private void CreateEventButton_Clicked(object sender, EventArgs e)
        {
            Shell.Current.GoToAsync("createevent");
        }
    }
}
