using CrisoftApp.DataService;
using CrisoftApp.Models.Tablas_Relacionales;

namespace CrisoftApp.Pages.Usuario
{
    public partial class PedidosUsuario : ContentPage
    {

        private List<UP> cuEscogido = new List<UP>();

        private LocalDbService localDbService;

        string nombreDb = "Joyeria.db";

        public PedidosUsuario(int idUsu)
        {
            InitializeComponent();

            localDbService = new LocalDbService();

            MostrarPedidos(idUsu);
        }

        private void MostrarPedidos(int idUsuario)
        {
            try
            {
                // Obtener los pedidos del usuario actual
                var pedidosUsuario = localDbService.ObtenerPedidosPorUsuario(nombreDb, idUsuario);

                // Limpiar la lista de pedidos seleccionados
                cuEscogido.Clear();

                // Agregar los pedidos del usuario actual a la lista
                foreach (var pedidoUsuario in pedidosUsuario)
                {
                    // Obtener los datos del usuario
                    var usuario = localDbService.ObtenerUsuarioPorId(nombreDb, pedidoUsuario.idCliente).FirstOrDefault();

                    if (usuario != null)
                    {
                        // Agregar el usuario y el pedido a la lista
                        cuEscogido.Add(new UP { usuario = usuario, pedido = pedidoUsuario });
                    }
                }

                // Mostrar los pedidos del usuario actual
                listaPedidos.ItemsSource = cuEscogido;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar pedidos: {ex}");
            }
        }

    }
}
