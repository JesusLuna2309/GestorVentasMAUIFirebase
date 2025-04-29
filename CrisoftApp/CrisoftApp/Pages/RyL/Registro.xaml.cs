namespace CrisoftApp.Pages.RyL;

public partial class Registro : ContentPage
{
	public Registro()
	{
		InitializeComponent();
	}

    private void OnRegisterClicked(object sender, EventArgs e)
    {

    }

    private async void OnSignInLabelTapped(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new MainPage());
    }


}