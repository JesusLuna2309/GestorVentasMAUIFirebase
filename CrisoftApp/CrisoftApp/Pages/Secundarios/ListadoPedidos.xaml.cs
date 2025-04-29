using CrisoftApp.DataService;
using CrisoftApp.Models.Tablas_Relacionales;
using CrisoftApp.Models.Entidades;

namespace CrisoftApp.Pages.Secundarios
{
    public partial class ListadoPedidos : ContentPage
    {
        private LocalDbService localDbService;
        private List<PA> _lineaarticulo = new List<PA>();
        public string nombreDb = "Joyeria.db";
        private int idPedido;
        private float tDinero;

        public ListadoPedidos(int idPedido, float total)
        {
            InitializeComponent();
            localDbService = new LocalDbService();
            this.idPedido = idPedido;
            this.tDinero = total;
            MostrarResumenPedidos();
            Totalentry.Text = total.ToString();
        }

        private void MostrarResumenPedidos()
        {
            try
            {
                var lineaspedidos = localDbService.ObtenerLineasPedido(nombreDb, idPedido);
                if (lineaspedidos != null)
                {
                    foreach (var linea in lineaspedidos)
                    {
                        var articulo = localDbService.ObtenerArticuloCompleto(nombreDb, linea.idArticulo);
                        foreach (var arti in articulo)
                        {
                            if (arti.venta == arti.ventaOferta.ToString())
                            {
                                linea.total = linea.unidades * linea.venta;
                                arti.coste = 0;
                                _lineaarticulo.Add(new PA { Articulo = arti, LineasPedidos = linea });
                            }
                            else
                            {
                                linea.total = linea.unidades * linea.venta;
                                arti.coste = (int)(100 - ((arti.ventaOferta * 100) / Convert.ToDouble(arti.venta)));
                                _lineaarticulo.Add(new PA { Articulo = arti, LineasPedidos = linea });
                            }
                        }
                    }
                }
                listaresumen.ItemsSource = _lineaarticulo;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar clientes: {ex}");
            }
        }

        private async void Clicked_btnfin(object sender, EventArgs e)
        {
            if (tDinero == 0)
            {
                await DisplayAlert("Error", "No se ha agregado ningún producto al carrito", "OK");
                await Navigation.PushAsync(new BotonesCPA());
            }
            else
            {
                localDbService.ActualizarPedidos(nombreDb, idPedido, tDinero);
                await DisplayAlert("Compra finalizada", "Su compra ha sido registrada", "OK");
                await Navigation.PushAsync(new BotonesCPA());
            }
        }

        private async void Clicked_btntrash(object sender, EventArgs e)
        {
            try
            {
                Button btn = (Button)sender;
                PA pedidoArticulo = (PA)btn.BindingContext;
                int idLineaPedido = pedidoArticulo.LineasPedidos.idLineaPedido;
                localDbService.ActualizarExistenciasT(nombreDb, pedidoArticulo.Articulo.idArticulo, pedidoArticulo.LineasPedidos.unidades);
                float totalart = pedidoArticulo.LineasPedidos.total;
                tDinero -= totalart;
                Totalentry.Text = tDinero.ToString();
                localDbService.EliminarLineaPedido(nombreDb, idLineaPedido);
                _lineaarticulo.Clear();
                listaresumen.ItemsSource = null;
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
                PA pedidoArticulo = (PA)btn.BindingContext;
                int exi = localDbService.ObtenerExistencias(nombreDb, pedidoArticulo.Articulo.idArticulo);

                if (exi > 0)
                {
                    localDbService.ActualizarExistenciasR(nombreDb, pedidoArticulo.Articulo.idArticulo);
                    int idLineaPedido = pedidoArticulo.LineasPedidos.idLineaPedido;
                    int unidadesActuales = pedidoArticulo.LineasPedidos.unidades;
                    int nuevasUnidades = unidadesActuales + 1;
                    float nuevoTotal = pedidoArticulo.Articulo.ventaOferta * nuevasUnidades;
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

        private async void RestarCantidadClicked(object sender, EventArgs e)
        {
            try
            {
                Button btn = (Button)sender;
                PA pedidoArticulo = (PA)btn.BindingContext;
                int idLineaPedido = pedidoArticulo.LineasPedidos.idLineaPedido;
                localDbService.ActualizarExistencias(nombreDb, pedidoArticulo.Articulo.idArticulo);
                int unidadesActuales = pedidoArticulo.LineasPedidos.unidades;

                if (unidadesActuales > 1)
                {
                    int nuevasUnidades = unidadesActuales - 1;
                    float nuevoTotal = pedidoArticulo.Articulo.ventaOferta * nuevasUnidades;
                    await UpdateLineaPedido(idLineaPedido, nuevasUnidades, nuevoTotal);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al restar la cantidad: {ex}");
            }
        }

        private async Task UpdateLineaPedido(int idLineaPedido, int nuevasUnidades, float nuevoTotal)
        {
            try
            {
                var lineaPedido = _lineaarticulo.FirstOrDefault(linea => linea.LineasPedidos.idLineaPedido == idLineaPedido);
                if (lineaPedido != null)
                {
                    int oldunidades = lineaPedido.LineasPedidos.unidades;
                    lineaPedido.LineasPedidos.unidades = nuevasUnidades;
                    lineaPedido.LineasPedidos.total = nuevoTotal;
                    localDbService.ActualizarLineasPedidos(nombreDb, idLineaPedido, nuevasUnidades, nuevoTotal);
                    listaresumen.ItemsSource = null;
                    listaresumen.ItemsSource = _lineaarticulo;

                    if (nuevasUnidades > oldunidades)
                    {
                        tDinero = tDinero + lineaPedido.Articulo.ventaOferta;
                        Totalentry.Text = tDinero.ToString();
                    }
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
}
