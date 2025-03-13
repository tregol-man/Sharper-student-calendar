using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;
using System.Text.Json;
using Microsoft.Maui.Controls;

namespace Calendar
{
    public partial class SubjectsPopup : Popup
    {
        public ObservableCollection<SubjectData> Subjects { get; set; }
        public SubjectData SelectedSubject { get; private set; }
        public SubjectsPopup(ObservableCollection<SubjectData> subjects)
        {
            InitializeComponent();
            Subjects = subjects;
            BindingContext = this;
        }
        private void SubjectSelected(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is SubjectData subject)
            {
                SelectedSubject = subject;
                Close(subject);  // Pass the selected subject back
            }
        }
    }
}
