using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using Calendar;

namespace Calendar
{
    public partial class DatePickerPopup : Popup
    {
        public DatePickerPopup()
        {
            InitializeComponent();
        }
        private void OnDateSelected(object sender, EventArgs e)
        {
            Close(datePicker.Date);
        }
    }
}