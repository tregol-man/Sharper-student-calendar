using System.Text.Json;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Calendar
{
    public partial class CreateEvent : ContentPage, IQueryAttributable
    {

        public CreateEvent()
        {
            InitializeComponent();
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
                DateButton.Text = "Select Date";
            }
        }
    }
}