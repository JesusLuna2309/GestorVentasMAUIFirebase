using CrisoftApp.DataService;
using CrisoftApp.Models.Entidades;

namespace CrisoftApp.Pages.Secundarios;

public partial class AñadirClientes : ContentPage
{
	public AñadirClientes()	
	{
		InitializeComponent();
	}

    private async void OnClienteTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AñadirClientes());
    }

    private async void OnAñadidoTapped(object sender, EventArgs e)
    {
        // Recoger los datos de los Entry
        var nombre = ((Entry)this.FindByName("NombreEntry")).Text;
        var direccion = ((Entry)this.FindByName("DireccionEntry")).Text;
        var provincia = ((Entry)this.FindByName("ProvinciaEntry")).Text;
        var localidad = ((Entry)this.FindByName("LocalidadEntry")).Text;
        var codigoPostal = ((Entry)this.FindByName("CPostalEntry")).Text;
        var email = ((Entry)this.FindByName("EmailEntry")).Text;
        var pais = ((Entry)this.FindByName("PaisEntry")).Text;
        var rutaText = ((Entry)this.FindByName("RutaEntry")).Text;
        var ubicacion = ((Entry)this.FindByName("UbicacionEntry")).Text;
        var enviadoText = ((Entry)this.FindByName("EnviadoEntry")).Text;

        // Validar y convertir el valor de Ruta y Enviado
        if (!int.TryParse(rutaText, out int ruta))
        {
            DisplayAlert("Error", "Ruta debe ser un número entero", "OK");
            return;
        }

        if (!int.TryParse(enviadoText, out int enviado))
        {
            DisplayAlert("Error", "Enviado debe ser un número entero", "OK");
            return;
        }

        // Crear un objeto Cliente
        var cliente = new Cliente
        {
            nombre = nombre,
            dirección = direccion,
            provincia = provincia,
            localidad = localidad,
            codigoPostal = codigoPostal,
            email = email,
            pais = pais,
            ruta = rutaText,
            ubicación = ubicacion,
            enviado = enviado == 1 // Convertir enviado a booleano
        };

        // Guardar el cliente en la base de datos
        var databaseName = "Joyeria.db";
        var localDbService = new LocalDbService();
        localDbService.AñadirCliente(databaseName, cliente);

        // Mostrar un mensaje de éxito
        DisplayAlert("Éxito", "Cliente añadido correctamente", "OK");

        await Navigation.PushModalAsync(new ListadoClientes());
    }

    //private async void Clicked_btncancelar(object sender, EventArgs e)
    //{
    //    await DisplayAlert("Cancelado", "Se ha cancelado el registro del cliente", "OK");
    //    await Navigation.PopModalAsync();
    //}
}