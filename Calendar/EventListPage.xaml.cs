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
        public EventListPage()
        {
            InitializeComponent();
            LoadEvents();
            PopulateEventGrid();
        }
        private List<EventInfo> _events;
        private List<SubjectData> _subjects;
        private List<GroupData> _groups;

        private void LoadEvents()
        {
            (_events, _subjects, _groups) = FunctionsLib.LoadEvents();
        }
        private void PopulateEventGrid(bool sortByDate = true)
        {
            DynamicContent.Children.Clear();
            IEnumerable<EventInfo> sortedEvents;

            if (sortByDate)
            {
                // Sort events by date
                sortedEvents = _events.OrderBy(e => e.DueDate);
            }
            else
            {
                // Sort events by subject name
                sortedEvents = _events
                    .OrderBy(e => _subjects.FirstOrDefault(s => s.Id == e.SubjectId)?.Name ?? "Unknown Subject")
                    .ThenBy(e => e.Name);
            }
            DateTime? lastEventDate = null;
            string lastHeader = null;
            bool useBlueBackground = true;

            foreach (var eventInfo in sortedEvents)
            {
                string currentHeader = sortByDate
                ? eventInfo.DueDate.ToString("MMMM yyyy") // Month and year for date sorting
                : _subjects.FirstOrDefault(s => s.Id == eventInfo.SubjectId)?.Name ?? "Unknown Subject";

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
                    Text = eventInfo.DueDate.ToString("MM/dd/yyyy"),
                    VerticalOptions = LayoutOptions.Center,
                    FontAttributes = FontAttributes.Bold,
                    FontFamily = "Inter",
                    TextColor = Color.FromRgba("#4F5171")
                };
                TapGestureRecognizer tapGestureDate = new TapGestureRecognizer
                {
                    Command = new Command(() => FunctionsLib.OnDayTapped(eventInfo.DueDate.Date))
                };
                dateLabel.GestureRecognizers.Add(tapGestureDate);
                Grid.SetRow(dateLabel, 0);
                Grid.SetColumn(dateLabel, 0);
                eventGrid.Children.Add(dateLabel);

                lastEventDate = eventInfo.DueDate.Date;
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
                    Command = new Command(() => FunctionsLib.OnEventTapped(eventInfo.Id))
                };
                detailsGrid.GestureRecognizers.Add(tapGestureEvent);
                //event name
                var nameLabel = new Label
                {
                    Text = eventInfo.Name,
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
                var subjectName = _subjects.FirstOrDefault(s => s.Id == eventInfo.SubjectId)?.Name ?? "Unknown Subject";
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
    }
}
