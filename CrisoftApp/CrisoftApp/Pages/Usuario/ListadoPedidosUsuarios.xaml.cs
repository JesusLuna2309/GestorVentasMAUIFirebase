using CrisoftApp.DataService;
using CrisoftApp.Models.Tablas_Relacionales;
using CrisoftApp.Models.Entidades;

namespace CrisoftApp.Pages.Usuario;

public partial class ListadoPedidosUsuarios : ContentPage
{
    private LocalDbService localDbService;

    // Se crea la lista de tipo PedidoArticulo
    private List<PA> _lineaarticulo = new List<PA>();

    public string nombreDb = "Joyeria.db";

    // Se crean las variables para inicializar
    private int idPedido;
    private float tDinero;

    public int idUsu;

    // Constructor para mostrar la página
    public ListadoPedidosUsuarios(int idUsuario, int idPedido, float total)
    {
        // Se crea la pantalla
        InitializeComponent();

        localDbService = new LocalDbService();

        // Se inicializa
        this.idPedido = idPedido; this.tDinero = total;
        this.idUsu = idUsuario;

        // Se muestran los productos del carrito llamando al método MostrarResumenPedidos()
        MostrarResumenPedidos();

        // Se actualiza el valor del tDinero
        Totalentry.Text = total.ToString();
    }

    // Método para mostrar los productos del carrito
    private void MostrarResumenPedidos()
    {
        try
        {
            // Se obtienen las lineasdepedido que coinciden con el id del pedido
            var lineaspedidos = localDbService.ObtenerLineasPedido(nombreDb, idPedido);
            if (lineaspedidos != null)
            {
                // Se selecciona primero la linea del pedido
                foreach (var linea in lineaspedidos)
                {
                    // Se obtienen los articulos que coinciden con el idarticulo de linea de pedidos
                    var articulo = localDbService.ObtenerArticuloCompleto(nombreDb, linea.idArticulo);
                    foreach (var arti in articulo)
                    {
                        // Si no hay descuento se muestra 0 y se guardan la linea del pedido y el articulo en la lista de PedidoArticulo
                        if (arti.venta == arti.ventaOferta.ToString())
                        {
                            linea.total = linea.unidades * linea.venta;
                            arti.coste = 0;
                            _lineaarticulo.Add(new PA { Articulo = arti, LineasPedidos = linea });
                        }
                        // Si  hay descuento se calcula entre el precio de venta inicial y final y se guardan la linea del pedido y el articulo en la lista de PedidoArticulo
                        else
                        {
                            linea.total = linea.unidades * linea.venta;
                            arti.coste = (int)(100 - ((arti.ventaOferta * 100) / Convert.ToDouble(arti.venta)));
                            _lineaarticulo.Add(new PA { Articulo = arti, LineasPedidos = linea, });
                        }
                    }
                }
            }

            // Se manda a la lista para mostrarlos
            listaResumen.ItemsSource = _lineaarticulo;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al cargar clientes: {ex}");
        }
    }

    // Método para finalizar el pedido
    private async void Clicked_btnfin(object sender, EventArgs e)
    {
        // Si no hay ningún producto en el carrito se muestra el mensaje y se vuelve al menú principal
        if (tDinero == 0)
        {
            await DisplayAlert("Error", "No se ha agregado ningún producto al carrito", "OK");
            await Navigation.PushModalAsync(new BotonesCPA());
        }
        // Si hay productos se actualiza el pedido, se muestra la confirmación de compra y se vuelve al menú principal
        else
        {
            // Actualizar el pedido en la base de datos
            localDbService.ActualizarPedidos(nombreDb, idPedido, tDinero);

            // Mostrar la confirmación de compra y volver al menú principal
            await DisplayAlert("Compra finalizada", "Su compra ha sido registrada", "OK");
            await Navigation.PushModalAsync(new BotonesCPAUsuario(idUsu));
        }
    }

    // Método para eliminar un producto del carrito
    private async void Clicked_btntrash(object sender, EventArgs e)
    {
        try
        {
            Button btn = (Button)sender;
            // Se obtiene el id de la lineapedido de la lista PedidoArticulo
            PA pedidoArticulo = (PA)btn.BindingContext;
            int idLineaPedido = pedidoArticulo.LineasPedidos.idLineaPedido;

            // Actualizar existencias del artículo
            localDbService.ActualizarExistenciasT(nombreDb, pedidoArticulo.Articulo.idArticulo, pedidoArticulo.LineasPedidos.unidades);

            // Se actualiza el tDinero
            float totalart = pedidoArticulo.LineasPedidos.total;
            tDinero -= totalart;
            Totalentry.Text = tDinero.ToString();

            // Se elimina la línea de pedido del carrito
            localDbService.EliminarLineaPedido(nombreDb, idLineaPedido);

            // Se vuelve a cargar los artículos en el carrito
            _lineaarticulo.Clear();
            listaResumen.ItemsSource = null;
            MostrarResumenPedidos();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al eliminar el producto del carrito: {ex}");
        }
    }

    private async void SumarCantidadClicked(object sender, EventArgs e)
    {
        try
        {
            Button btn = (Button)sender;
            // Se obtiene el id de la lineapedido de la lista PedidoArticulo
            PA pedidoArticulo = (PA)btn.BindingContext;

            // Obtener existencias del artículo
            int exi = localDbService.ObtenerExistencias(nombreDb, pedidoArticulo.Articulo.idArticulo);

            if (exi > 0)
            {
                localDbService.ActualizarExistenciasR(nombreDb, pedidoArticulo.Articulo.idArticulo);
                int idLineaPedido = pedidoArticulo.LineasPedidos.idLineaPedido;

                // Se obtienen las unidades actuales
                int unidadesActuales = pedidoArticulo.LineasPedidos.unidades;

                // Se aumenta las unidades en 1
                int nuevasUnidades = unidadesActuales + 1;

                // Se calcula el nuevo tDinero
                float nuevoTotal = pedidoArticulo.Articulo.ventaOferta * nuevasUnidades;

                // Se actualiza la línea de pedido
                await UpdateLineaPedido(idLineaPedido, nuevasUnidades, nuevoTotal);
            }
            else
            {
                await DisplayAlert("SIN STOCK", "El producto seleccionado se ha quedado sin existencias", "OK");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al sumar la cantidad: {ex}");
        }
    }

    // Método para restar una unidad del producto
    private async void RestarCantidadClicked(object sender, EventArgs e)
    {
        try
        {
            Button btn = (Button)sender;
            // Se obtiene el id de la lineapedido de la lista PedidoArticulo
            PA pedidoArticulo = (PA)btn.BindingContext;
            int idLineaPedido = pedidoArticulo.LineasPedidos.idLineaPedido;
            localDbService.ActualizarExistencias(nombreDb, pedidoArticulo.Articulo.idArticulo);
            // Se obtienen las unidades actuales
            int unidadesActuales = pedidoArticulo.LineasPedidos.unidades;

            // Se mantiene que como mínimo haya una unidad
            if (unidadesActuales > 1)
            {
                // Se reduce en una unidad y se calcula el nuevo tDinero
                int nuevasUnidades = unidadesActuales - 1;
                float nuevoTotal = pedidoArticulo.Articulo.ventaOferta * nuevasUnidades;

                // Se actualiza la línea de pedido
                await UpdateLineaPedido(idLineaPedido, nuevasUnidades, nuevoTotal);
            }
        }

        catch (Exception ex)
        {
            Console.WriteLine($"Error al restar la cantidad: {ex}");
        }
    }

    // Método para actualizar las unidades y el tDinero del producto
    private async Task UpdateLineaPedido(int idLineaPedido, int nuevasUnidades, float nuevoTotal)
    {
        try
        {
            // Se busca la línea de pedido en la lista
            var lineaPedido = _lineaarticulo.FirstOrDefault(linea => linea.LineasPedidos.idLineaPedido == idLineaPedido);

            if (lineaPedido != null)
            {
                // Se guarda el valor de las unidades antiguo
                int oldunidades = lineaPedido.LineasPedidos.unidades;

                // Se actualizan las unidades y el tDinero en la línea de pedido
                lineaPedido.LineasPedidos.unidades = nuevasUnidades;
                lineaPedido.LineasPedidos.total = nuevoTotal;

                // Actualizamos la línea de pedido en la base de datos
                localDbService.ActualizarLineasPedidos(nombreDb, idLineaPedido, nuevasUnidades, nuevoTotal);

                // Se actualiza la lista de la vista
                listaResumen.ItemsSource = null;
                listaResumen.ItemsSource = _lineaarticulo;

                // Si se aumenta el número de unidades se incrementa el tDinero
                if (nuevasUnidades > oldunidades)
                {
                    tDinero = tDinero + lineaPedido.Articulo.ventaOferta;
                    Totalentry.Text = tDinero.ToString();
                }
                // Si se reduce el número de unidades se disminuye el tDinero
                else
                {
                    tDinero = tDinero - lineaPedido.Articulo.ventaOferta;
                    Totalentry.Text = tDinero.ToString();
                }
            }
            else
            {
                Console.WriteLine("La línea de pedido no fue encontrada en la lista.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al actualizar la línea de pedido: {ex}");
        }
    }
}