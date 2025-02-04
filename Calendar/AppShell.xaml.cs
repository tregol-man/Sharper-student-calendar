using Microsoft.Maui.Controls;

namespace Calendar
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            this.BindingContext = this;
            Routing.RegisterRoute("eventpage", typeof(EventPage));
            Routing.RegisterRoute("datepage", typeof(DatePage));
            Routing.RegisterRoute("createevent", typeof(CreateEvent));
        }
    }
}