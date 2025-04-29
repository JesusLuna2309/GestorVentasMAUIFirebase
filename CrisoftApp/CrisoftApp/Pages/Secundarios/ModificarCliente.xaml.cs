using CrisoftApp.DataService;
using CrisoftApp.Models.Entidades;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace CrisoftApp.Pages.Secundarios;

public partial class ModificarCliente : ContentPage
{
	private Cliente cl;
    public LocalDbService localDbService;


	public ModificarCliente(Cliente clien)
	{
		InitializeComponent();

        localDbService = new LocalDbService();

		cl = clien;

        ClienteCargado();
	}

    private async void OnAñadidoTapped(object sender, EventArgs e)
    {
        try
        {
            // Se almacenan los datos nuevos
            string nombreA = NombreEntry.Text;
            string direccionA = DireccionEntry.Text;
            string localidadA = LocalidadEntry.Text;
            string provinciaA = ProvinciaEntry.Text;
            string ubicacionA = UbicacionEntry.Text;
            string rutaA = RutaEntry.Text;
            string cPostalA = CPostalEntry.Text;
            string emailA = EmailEntry.Text;

            // Crear un objeto Cliente con los nuevos datos
            Cliente updatedCliente = new Cliente
            {
                idCliente = cl.idCliente,
                nombre = nombreA,
                dirección = direccionA,
                localidad = localidadA,
                provincia = provinciaA,
                ubicación = ubicacionA,
                ruta = rutaA,
                codigoPostal = cPostalA,
                email = emailA
            };

            // Se llama al método para actualizar los datos de la tabla cliente
            localDbService.UpdateClientes("Joyeria.db", updatedCliente);

            // Navegar de regreso a la lista de clientes
            await Navigation.PopModalAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Ocurrió un error al editar el cliente: {ex.Message}", "OK");
        }
    }


    private async void OnVolverClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    public async void ClienteCargado()
    {
        NombreEntry.Text = cl.nombre;
        DireccionEntry.Text = cl.dirección;
        ProvinciaEntry.Text = cl.provincia;
        LocalidadEntry.Text = cl.localidad;
        CPostalEntry.Text = cl.codigoPostal;
        EmailEntry.Text = cl.email;
        RutaEntry.Text = cl.ruta;
        UbicacionEntry.Text = cl.ubicación;
    }
}