using CrisoftApp.DataService;
using CrisoftApp.Models.Tablas_Relacionales;

namespace CrisoftApp.Pages.Botones;

public partial class Pedidos : ContentPage
{

    private List<CP> cpEscogido = new List<CP>();

    private LocalDbService localDbService;

    string nombreDb = "Joyeria.db";

    public Pedidos()
    {
        InitializeComponent();

        localDbService = new LocalDbService();

        MostrarPedidos();
    }

    private void MostrarPedidos()
    {
        try
        {
            // Se elimina los pedidos nulos
            localDbService.EliminarPedidosConTotalCero(nombreDb);
            // Se reoganiza las id vacías
            //localDbService.LimpiarPedidos(nombreDb);
            // Se obtienen los pedidos
            var clientepedido = localDbService.ObtenerPedidos(nombreDb);
            if (clientepedido != null)
            {
                // Se sacan los pedidos para agregarlos a la lista
                foreach (var ped in clientepedido)
                {
                    // Se obtienen los clientes cuyo id coincida con el de la tabla pedidos
                    var client = localDbService.ObtenerClientePorId(nombreDb, ped.idCliente);

                    // Se guardan en la lista ClientePedidos el cliente y el pedido con el que coincide
                    foreach (var cust in client)
                    {
                        cpEscogido.Add(new CP { cliente = cust, pedido = ped });
                    }
                }
            }

            // Se manda a la lista para mostrarlos
            listaPedidos.ItemsSource = cpEscogido;
        }

        catch (Exception ex)
        {
            Console.WriteLine($"Error al cargar pedidos: {ex}");
        }
    }
}