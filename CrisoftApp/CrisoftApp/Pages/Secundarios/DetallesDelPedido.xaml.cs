using CrisoftApp.DataService;
using CrisoftApp.Models.Entidades;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CrisoftApp.Pages.Secundarios;

public partial class DetallesDelPedido : ContentPage
{
    string db = "Joyeria.db";

    private Articulo arEscogido;

	private int contador = 1;
	private int Id { get; set; }

    private float tDinero;
    private string cartelOferta, cartelNovedad, colorOferta, colorNovedad;

    public DetallesDelPedido(Articulo articulo, int id, float tDinero, string cartelOferta, string cartelNovedad, string colorOferta, string colorNovedad)
	{
		InitializeComponent();

		this.arEscogido = articulo;
		this.Id = id;
		this.tDinero = tDinero;
		this.cartelOferta = cartelOferta;
		this.cartelNovedad = cartelNovedad;
		this.colorOferta = colorOferta;
		this.colorNovedad = colorNovedad;

		RegistroDePedidos();
	}

    private void RegistroDePedidos()
    {
        // Se guardan para mostrar los datos del artículo seleccionado
        Descripcionentry.Text = arEscogido.descripcion;
        Referenciaentry.Text = arEscogido.referencia;
        OfertaCartel.Text = cartelOferta;
        NovedadCartel.Text = cartelNovedad;
        // Si no hay descuento solo se muestra el precio de venta
        if (arEscogido.ventaOferta.ToString() == arEscogido.venta)
        {
            VentaOfertaentry.Text = null;
            Ventaentry.Text = arEscogido.venta;
        }
        // Si lo hay se muestran ambos y el inicial tachado
        else
        {
            VentaOfertaentry.Text = arEscogido.ventaOferta.ToString() + " €";
            Ventaentry.Text = arEscogido.venta;
            Ventaentry.TextDecorations = TextDecorations.Strikethrough;
        }
        // Si se pasa el color verde para novedad se muestra su cartel y con fondo verde
        if (colorNovedad == "Blue")
        {
            NovedadCartel.BackgroundColor = Colors.Green;
            NovedadCartel.TextColor = Colors.White;
        }
        // Si se pasa el color rojo para ofertas se muestra su cartel y con fondo rojo
        if (colorOferta == "Red")
        {
            OfertaCartel.BackgroundColor = Colors.Red;
            OfertaCartel.TextColor = Colors.White;
        }

        // Se crea una lista de URLs de imágenes
        List<string> imagenesUrls = new List<string>{
            //Se almacenan las imágenes
            arEscogido.urlImagen1,
            arEscogido.urlImagen2,
            arEscogido.urlImagen3,
            arEscogido.urlImagen4,
            arEscogido.urlImagen5,
            arEscogido.urlImagen6,
            arEscogido.urlImagen7
            };

        // Se eliminan las URL nulas o vacías
        imagenesUrls.RemoveAll(string.IsNullOrEmpty);

        // Se agregan las imágenes al CarouselView para mostrarlas
        ImagenProductoCollectionView.ItemsSource = imagenesUrls;
    }

    private async void Clicked_btncomprar(object sender, EventArgs e)
    {
        // Se calcula el nuevo tDinero
        this.tDinero = this.tDinero + (contador * arEscogido.ventaOferta);
        arEscogido.venta = arEscogido.ventaOferta.ToString();
        // Se comprueba si el producto ya está agregado al carrito
        bool addp = await LocalDbService.CheckArticuloEnPedido(db,  Id, arEscogido.idArticulo);
        // Si está se actualiza su unidad
        if (addp)
        {
            await LocalDbService.UpdateLineaPedidosunidades(Id, arEscogido.idArticulo, 1, this.tDinero);
        }
        // Si no está se añade 
        else
        {
            await LocalDbService.AñadirLP(Id, arEscogido.idArticulo, arEscogido.referencia, arEscogido.coste, Convert.ToDouble(arEscogido.venta), contador, this.tDinero);
        }
        // Se devuelve el tDinero a la página Catálogo
        MessagingCenter.Send(this, "UpdateTotal", this.tDinero);

        //Se vuelve a la página catálogo
        await Navigation.PopModalAsync();
    }
}