using CrisoftApp.DataService;
using CrisoftApp.Models.Entidades;
using CrisoftApp.Models.Tablas_Relacionales;
using CrisoftApp.Pages.Botones;
using CrisoftApp.Pages.Secundarios;

namespace CrisoftApp.Pages.Usuario;

public partial class CatalogoUsuarios : ContentPage
{
    private LocalDbService localDbService;

	private int idUsu;

    private string nombreDb = "Joyeria.db";

    public float tDinero;
    public int idPedido;

    private List<ArticuloCartel> listaAC;

    public CatalogoUsuarios(int idUsuario)
	{
		InitializeComponent();

        localDbService = new LocalDbService();

        this.idUsu = idUsuario;

        listaAC = new List<ArticuloCartel>();

        //totalprecio.Text = ("0" + " �");

        MessagingCenter.Subscribe<DetallesDelPedido, float>(this, "UpdateTotal", (sender, total) =>
        {
            this.tDinero = total;
            //totalprecio.Text = total.ToString() + " �";
        });

        // Se recibe el tDinero dedsde la p�gina ResumenPedido y se actualiza
        MessagingCenter.Subscribe<ListadoPedidos, float>(this, "UpdateResumen", (sender, total) =>
        {
            this.tDinero = total;
            //totalprecio.Text = total.ToString() + " �";
        });

        LeerYGuardarArticulos();
    }

    private async void LeerYGuardarArticulos()
    {
         try
        {
            // Crear el pedido
            var pedido = new Pedido
            {
                fecha = DateTime.Today,
                idCliente = idUsu,
                total = tDinero,
                enviado = 0
            };

            // Insertar el pedido y obtener el ID
            this.idPedido = await localDbService.InsertarPedidos(nombreDb, pedido);

            // Verificar si el pedido se insert� correctamente
            if (idPedido == -1)
            {
                Console.WriteLine("Error al insertar el pedido.");
                return;
            }

            // Continuar con el resto del c�digo para obtener y mostrar art�culos
            var art = localDbService.ObtenerArticulos(nombreDb);
            string cartelof = null;
            string cartelno = null;
            string colorof = "";
            string colorno = "";

            if (art != null)
            {
                foreach (var articul in art)
                {
                    if (articul.existencias > 0)
                    {
                        TimeSpan diferencia = (TimeSpan)(DateTime.Now - articul.fechaAlta);

                        if (articul.ventaOferta.ToString() == articul.venta)
                        {
                            articul.venta = null;
                            colorof = "Transparent";
                        }
                        else
                        {
                            articul.venta = articul.venta + " �";
                            cartelof = "OFERTA";
                            colorof = "Red";
                        }

                        if (diferencia.TotalDays < 80)
                        {
                            cartelno = "NOVEDAD";
                            colorno = "Green";
                        }
                        else
                        {
                            cartelno = "";
                            colorno = "Transparent";
                        }

                        listaAC.Add(new ArticuloCartel
                        {
                            articulo = articul,
                            cartelOferta = cartelof,
                            cartelNovedad = cartelno,
                            colorFondoCartelOferta = colorof,
                            colorFondoCartelNovedad = colorno
                        });

                        cartelno = null;
                        cartelof = null;
                        colorof = null;
                        colorno = null;
                    }
                }
            }

            if (listaAC.Count == 0)
            {
                Resultado.Text = "SIN RESULTADOS";
                Resultado.TextColor = Colors.Black;
            }
            else
            {
                Resultado.Text = "";
                Resultado.TextColor = Colors.White;
            }

            listaCat.ItemsSource = listaAC;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al cargar clientes: {ex}");
        }
    }

    private async void OnPedidosTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Pedidos());
    }

    //Este es el boton que aparece en cada uno de los objetos para indicar que lo a�adimos a la bolsa. 
    private async void clicked_btncarrito(object sender, EventArgs e)
    {
        var button = sender as Button;
        // Se obtiene el art�culo seleccionado de la lista ArticuloCartel
        var articulocartelSeleccionado = (ArticuloCartel)button.BindingContext;
        var articuloSeleccionado = articulocartelSeleccionado.articulo;
        if (button != null)
        {
            int exist = localDbService.ObtenerExistencias(nombreDb, articulocartelSeleccionado.articulo.idArticulo);
            if (exist > 0)
            {
                // Se actualiza el total con el nuevo art�culo seleccionado
                tDinero = tDinero + articuloSeleccionado.ventaOferta;
                totalprecio.Text = tDinero.ToString() + " �";

                // Se comprueba que el producto est� a�adido o no
                bool addp = LocalDbService.ComprobarArticuloEnPedido(nombreDb, idPedido, articuloSeleccionado.idArticulo);
                // Si est�, se actualiza las unidades del producto
                if (addp)
                {
                    localDbService.ActualizarUnidadesPedidos(nombreDb, idPedido, articuloSeleccionado.idArticulo, 0, tDinero);
                    localDbService.ActualizarExistenciasR(nombreDb, articuloSeleccionado.idArticulo);
                }
                // Si no est�, se a�ade nuevo
                else
                {
                    // Crear una instancia de LineasPedido
                    var lineaPedido = new LineasPedido
                    {
                        idPedido = idPedido,
                        idArticulo = articuloSeleccionado.idArticulo,
                        referencia = articuloSeleccionado.referencia,
                        coste = articuloSeleccionado.coste,
                        venta = articuloSeleccionado.ventaOferta,
                        unidades = 1, // Solo se a�ade una unidad
                        total = tDinero
                    };

                    // Llamar al m�todo InsertarLineasPedidos con el objeto LineasPedido
                    await localDbService.InsertarLineasPedidos(nombreDb, lineaPedido);

                    localDbService.ActualizarExistenciasR(nombreDb, articuloSeleccionado.idArticulo);
                }
            }
            else
            {
                await DisplayAlert("SIN STOCK", "El producto seleccionado se ha quedado sin existencias", "OK");
            }
        }
    }

    private async void Clicked_btndetalle(object sender, EventArgs e)
    {
        var button = sender as Button;
        // Se obtiene el articulo seleccionado de la lista ArticuloCartel y se env�a a la p�gina ClienteDetalle
        var articulocartelSeleccionado = (ArticuloCartel)button.BindingContext;
        var articuloSeleccionado = articulocartelSeleccionado.articulo;
        Page mapaPage;

        // Verifica la plataforma actual
        mapaPage = new DetallesDelPedido(articuloSeleccionado, idPedido, tDinero, articulocartelSeleccionado.cartelOferta, articulocartelSeleccionado.cartelNovedad, articulocartelSeleccionado.colorFondoCartelOferta, articulocartelSeleccionado.colorFondoCartelNovedad);

        // Navegar a la p�gina correspondiente
        await Navigation.PushModalAsync(mapaPage);
    }

    private async void Clicked_btnComprar(object sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new ListadoPedidosUsuarios(idUsu, idPedido, tDinero));
    }
}