using Microsoft.Maui.Controls.Shapes;

namespace Calendar;

public partial class CreateClass : ContentPage
{
	List<string> _subjects; // List subjectù, který bude zobrazován; Úložní meziprostor

	public CreateClass()
	{
		InitializeComponent();
	}

	private void ConfirmChanges(object sender, EventArgs e) // Pro uložení zmìn èi pro vytvoøení nový class
	{
		DisplayAlert("Warning", "Vše klape", "Super");
	}

    private void OnClickCreateSubject(object sender, EventArgs e) // Uloží do lokálního listu jméno subjectu
	{
		string subjectName = SubjectNameEntry.Text;
	}

	private void UpdateSubjectList() // Updatne vizualní stránku _subjects
	{

	}

	private Border CreateSubject(string Name)
	{
		Grid _grid = new Grid();

		_grid.Add(new Label()
		{
			Text = Name,
			VerticalOptions = LayoutOptions.Center,
			FontSize = 20,
			FontAttributes = FontAttributes.Bold,
			TextColor = Colors.White,
			FontFamily = "Inter"
		});

		ImageButton button = new ImageButton()
		{
			Source = "cross_icon.png",
			HorizontalOptions = LayoutOptions.End,
			VerticalOptions = LayoutOptions.Center,
			TranslationX = 20,

		};

		button.Clicked += (sender, e) => DeleteSubject();

        _grid.Add(button);

		return new Border()
		{
			BackgroundColor = Color.FromArgb("#FF8080"),
			HeightRequest = 90,
			StrokeShape = new RoundRectangle()
			{
				CornerRadius = 20
			},
			Content = _grid
		};
	} // Vytvoøí vizuál pro daný subject

	private void DeleteSubject() // Vymaže daný subject z listu
	{
		
	}
}