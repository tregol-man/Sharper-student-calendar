﻿using System.Text.Json;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;
using Newtonsoft.Json;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

                    var headerLabel = new Label
                    {
                        Text = currentHeader,
                        FontSize = 20,
                        FontAttributes = FontAttributes.Bold,
                        Margin = new Thickness(10, 10, 10, 5),
                        TextColor = Colors.DarkBlue
                    };

                    DynamicContent.Children.Add(headerLabel);
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
                    FontAttributes = FontAttributes.Bold
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
                    BackgroundColor = useBlueBackground ? Colors.LightBlue : Colors.LightGreen,
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
                    Margin = new Thickness(0, 0, 0, 2)
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
                    TextColor = Colors.Gray
                };
                Grid.SetRow(subjectLabel, 1);
                Grid.SetColumn(subjectLabel, 0);
                detailsGrid.Children.Add(subjectLabel);
                Grid.SetRow(detailsGrid, 0);
                Grid.SetColumn(detailsGrid, 1);
                // Add the details grid to the main event grid
                eventGrid.Children.Add(detailsGrid);

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