using System.Text.Json;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Calendar
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();
            _currentMonth = DateTime.Now; // Set the initial month to the current month
            _selectedMonth = _currentMonth;
            LoadEvents();
            UpdateCalendar(_selectedMonth);
        }
        private DateTime _currentMonth;
        private DateTime _selectedMonth;
        private List<EventInfo> _events;
        private List<SubjectData> _subjects;
        private List<GroupData> _groups;

        private void LoadEvents()
        {
            (_events, _subjects, _groups) = FunctionsLib.LoadEvents();
        }

        // Function to populate calendar days
        private void UpdateCalendar(DateTime month)
        {
            try
            {
                CalendarGrid.Children.Clear();
                MonthLabel.Text = month.ToString("MMMM yyyy");

                // Get the first day of the month and its weekday
                DateTime firstOfMonth = new DateTime(month.Year, month.Month, 1);
                int startDayOfWeek = (int)firstOfMonth.DayOfWeek; // 0 = Sunday, 6 = Saturday
                startDayOfWeek = (startDayOfWeek == 0) ? 6 : startDayOfWeek - 1;

                // Calculate number of days in the current month
                int daysInMonth = DateTime.DaysInMonth(month.Year, month.Month);

                // Calculate previous month
                DateTime previousMonth = month.AddMonths(-1);
                int daysInPreviousMonth = DateTime.DaysInMonth(previousMonth.Year, previousMonth.Month);

                // Calculate the first date to show
                int firstDayToShow = daysInPreviousMonth - startDayOfWeek + 1;
                DateTime firstDate = new DateTime(previousMonth.Year, previousMonth.Month, firstDayToShow);
                int totalDaysToShow = 42;
                DateTime lastDate = firstDate.AddDays(totalDaysToShow - 1);

                Debug.WriteLine($"First date shown: {firstDate.ToShortDateString()}");
                Debug.WriteLine($"Last date shown: {lastDate.ToShortDateString()}");

                // Start filling days from the previous month
                DateTime currentDate = firstDate;
                for (int rowIndex = 0; rowIndex < 6; rowIndex++)
                {
                    for (int colIndex = 0; colIndex < 7; colIndex++)
                    {
                        bool isDifferentMonth = currentDate.Month != month.Month;
                        AddDayToGrid(currentDate.Day, rowIndex, colIndex, isDifferentMonth, currentDate);
                        currentDate = currentDate.AddDays(1);
                    }
                }

                LeftArrowButton.IsVisible = month.Month != DateTime.Now.Month || month.Year != DateTime.Now.Year;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                Debug.WriteLine(ex.StackTrace);
            }
        }


        // Helper method to add a label to the grid

        private void AddDayToGrid(int day, int row, int col, bool isAdjacentMonth, DateTime month)
        {
            try
            {
                DateTime date = new DateTime(month.Year, month.Month, day);
                bool isToday = date.Date == DateTime.Now.Date;

                // Create a new Grid for each day
                Grid dayGrid = new Grid
                {
                    RowDefinitions = {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // Day label
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // Event 1
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // Event 2 or "Plus X more"
                },
                    BackgroundColor = isAdjacentMonth ? Colors.LightGray : Colors.White
                };

                // Create the day label with larger font size
                Label dayLabel = new Label
                {
                    Text = day.ToString(),
                    FontSize = 24, // Larger font size for the day number
                    FontFamily = "Inter",
                    BackgroundColor = isToday ? Colors.LightBlue : Colors.Transparent,
                    TextColor = Colors.White,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center
                };

                // Add the day label to the first row
                Grid.SetRow(dayLabel, 0);
                dayGrid.Children.Add(dayLabel);

                TapGestureRecognizer tapGesture = new TapGestureRecognizer
                {
                    Command = new Command(() => FunctionsLib.OnDayTapped(date))
                };
                dayGrid.GestureRecognizers.Add(tapGesture);
                // Retrieve events for this day
                var eventsForDay = GetEventsForDay(date);

                // Add events to the grid with smaller font and text truncation
                if (eventsForDay.Count > 0)
                {
                    Label eventLabel1 = new Label
                    {
                        Text = eventsForDay[0].Name.Length > 10 ? eventsForDay[0].Name.Substring(0, 10) + "..." : eventsForDay[0].Name,
                        FontSize = 12, // Smaller font size for events
                        FontFamily = "Inter",
                        BackgroundColor = Colors.White,
                        HorizontalTextAlignment = TextAlignment.Center,
                        VerticalTextAlignment = TextAlignment.Center,
                    };
                    Grid.SetRow(eventLabel1, 1);
                    dayGrid.Children.Add(eventLabel1);

                    if (eventsForDay.Count == 2)
                    {
                        Label eventLabel2 = new Label
                        {
                            Text = eventsForDay[1].Name.Length > 10 ? eventsForDay[1].Name.Substring(0, 10) + "..." : eventsForDay[1].Name,
                            FontSize = 12,
                            FontFamily = "Inter",
                            BackgroundColor = Colors.LightBlue,
                            HorizontalTextAlignment = TextAlignment.Center,
                            VerticalTextAlignment = TextAlignment.Center,
                        };
                        Grid.SetRow(eventLabel2, 2);
                        dayGrid.Children.Add(eventLabel2);
                    }
                    else if (eventsForDay.Count > 2)
                    {
                        Label moreLabel = new Label
                        {
                            Text = $"+{eventsForDay.Count - 1} more",
                            FontFamily = "Inter",
                            FontSize = 12,
                            BackgroundColor = Colors.LightPink,
                            HorizontalTextAlignment = TextAlignment.Center,
                            VerticalTextAlignment = TextAlignment.Center,
                        };
                        Grid.SetRow(moreLabel, 2);
                        dayGrid.Children.Add(moreLabel);
                    }
                }
                else
                {
                    // Add placeholders to keep grid space filled
                    Label placeholder1 = new Label
                    {
                        Text = " ", // Empty or minimal placeholder text
                        FontSize = 12,
                        Padding = 10,
                        HorizontalTextAlignment = TextAlignment.Center,
                        VerticalTextAlignment = TextAlignment.Center,
                    };
                    Grid.SetRow(placeholder1, 1);
                    dayGrid.Children.Add(placeholder1);

                    Label placeholder2 = new Label
                    {
                        Text = " ", // Empty or minimal placeholder text
                        FontSize = 12,
                        Padding = 10,
                        HorizontalTextAlignment = TextAlignment.Center,
                        VerticalTextAlignment = TextAlignment.Center,
                    };
                    Grid.SetRow(placeholder2, 2);
                    dayGrid.Children.Add(placeholder2);
                }

                // Add the grid for the day to the calendar
                Grid.SetRow(dayGrid, row);
                Grid.SetColumn(dayGrid, col);
                CalendarGrid.Children.Add(dayGrid);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.InnerException?.Message);
                Debug.WriteLine("!");
            }
        }

        private List<EventInfo> GetEventsForDay(DateTime date)
        {
            if (_events == null)
            {
                Debug.WriteLine("The _events collection is null.");
                return new List<EventInfo>(); // Return an empty list if no events are available
            }
            // Here we filter the events for the selected month and day
            var eventsForDay = _events.Where(e => e.DueDate.Date == date.Date).ToList();
            return eventsForDay;
        }
        // Function to navigate to the next or previous month
        private void LeftArrowButton_Clicked(object sender, EventArgs e)
        {
            if (_selectedMonth.Year == DateTime.Now.Year && _selectedMonth.Month == DateTime.Now.Month)
            {
                return;
            }
            _selectedMonth = _selectedMonth.AddMonths(-1);
            UpdateCalendar(_selectedMonth);
        }

        private void RightArrowButton_Clicked(object sender, EventArgs e)
        {
            _selectedMonth = _selectedMonth.AddMonths(1);
            UpdateCalendar(_selectedMonth);
        }
    }
}
