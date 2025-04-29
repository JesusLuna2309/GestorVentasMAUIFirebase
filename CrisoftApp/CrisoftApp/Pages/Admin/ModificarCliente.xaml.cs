using CrisoftApp.DataService;
using CrisoftApp.Models.Entidades;
using System.Text.RegularExpressions;

namespace CrisoftApp.Pages.Admin;

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

    private async void OnModificarTapped(object sender, EventArgs e)
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

            // Validar que no hay campos vacíos
            if (string.IsNullOrEmpty(nombreA) ||
                string.IsNullOrEmpty(direccionA) ||
                string.IsNullOrEmpty(provinciaA) ||
                string.IsNullOrEmpty(localidadA) ||
                string.IsNullOrEmpty(cPostalA) ||
                string.IsNullOrEmpty(emailA) ||
                string.IsNullOrEmpty(rutaA) ||
                string.IsNullOrEmpty(ubicacionA))
            {
                await DisplayAlert("Error", "Todos los campos deben ser completados", "OK");
                return;
            }

            // Validar formato de email
            if (!Regex.IsMatch(emailA, @"^[^@\s]+@gmail\.com$"))
            {
                await DisplayAlert("Error", "El email debe tener un formato válido (ejemplo@gmail.com)", "OK");
                return;
            }

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
