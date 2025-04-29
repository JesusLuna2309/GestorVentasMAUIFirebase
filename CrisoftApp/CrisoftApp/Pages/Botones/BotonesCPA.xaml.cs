using CrisoftApp.Pages.Secundarios;
using CrisoftApp.Pages.Botones;
using CrisoftApp.Pages.Admin;

namespace CrisoftApp.Pages;

public partial class BotonesCPA : ContentPage
{
    public BotonesCPA()
    {
        InitializeComponent();
    }

    private async void OnListaClientesTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Admin.ListadoClientes());
    }

    private async void OnPedidosTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Pedidos());
    }

    private async void OnAgendaTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Agenda());
    }

    private async void OnUsuariosTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ListadoPersonal());
    }

    private async void OnPedidosUsuariosTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ListadoPedidosAdminUsuarios());
    }
}