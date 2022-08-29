namespace HoradotTV.Pages;

public partial class StartupPage : ContentPage
{
	public StartupPage(StartupViewModel vm)
	{
		InitializeComponent();

		BindingContext = vm;
	}
}