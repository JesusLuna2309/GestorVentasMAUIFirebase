using CrisoftApp.DataService;
using CrisoftApp.Models.Tablas_Relacionales;

namespace CrisoftApp.Pages.Admin;

public partial class ListadoPedidosAdminUsuarios : ContentPage
{

	private List<UP> upEscogido = new List<UP>();

    private LocalDbService localDbService;

    string nombreDb = "Joyeria.db";

    public ListadoPedidosAdminUsuarios()
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
                    var client = localDbService.ObtenerUsuarioPorId(nombreDb, ped.idCliente);

                    // Se guardan en la lista ClientePedidos el cliente y el pedido con el que coincide
                    foreach (var cust in client)
                    {
                        upEscogido.Add(new UP { usuario = cust, pedido = ped });
                    }
                }
            }

            // Se manda a la lista para mostrarlos
            listaPedidosUsuarios.ItemsSource = upEscogido;
        }

        catch (Exception ex)
        {
            Console.WriteLine($"Error al cargar pedidos: {ex}");
        }
    }
}