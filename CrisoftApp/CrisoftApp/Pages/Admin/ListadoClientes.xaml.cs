using CrisoftApp.DataService;
using CrisoftApp.Models.Entidades;
using CrisoftApp.Pages.Secundarios;

namespace CrisoftApp.Pages.Admin
{
    public partial class ListadoClientes : ContentPage
    {
        private LocalDbService localDbService;
        string db = "Joyeria.db";

        private List<Cliente> clientes = new List<Cliente>();

        public ListadoClientes()
        {
            InitializeComponent();
            localDbService = new LocalDbService();
            BindingContext = new Cliente();
            //bBusqueda.TextChanged += SearchBar_TextChanged;

            // Cargar clientes al iniciar la p�gina
            CargarClientes();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            CargarClientes();
        }

        private void CargarClientes()
        {
            clientes = localDbService.ObtenerClientes(db).ToList();
            listaCl.ItemsSource = clientes;
        }

        private async void OnA�adirClienteClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new A�adirClientes());
        }

        private void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (e.NewTextValue != null)
            {
                var filtro = e.NewTextValue.ToLower();
                if (string.IsNullOrEmpty(filtro))
                {
                    listaCl.ItemsSource = clientes;
                }
                else
                {
                    listaCl.ItemsSource = clientes
                        .Where(c => c.idCliente.ToString().ToLower().Contains(filtro) ||
                                    c.nombre.ToLower().Contains(filtro) ||
                                    c.localidad.ToLower().Contains(filtro) ||
                                    c.provincia.ToLower().Contains(filtro) ||
                                    c.ruta.ToLower().Contains(filtro)).ToList();
                }
            }
        }

        private async void btn_ModificarClicked(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            var clienteSele = (Cliente)btn.BindingContext;
            ModificarCliente mc = new ModificarCliente(clienteSele);
            await Navigation.PushModalAsync(mc);
        }

        private void SearchButtonPressed(object sender, EventArgs e)
        {
            // L�gica para manejar la pulsaci�n del bot�n de b�squeda
        }

        private async void listaCl_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            var clienteSeleccionado = (Cliente)e.Item;
            await Navigation.PushAsync(new CatalogoAdmin(clienteSeleccionado));
        }

        private async void btn_EliminarClicked(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            var clienteSele = (Cliente)btn.BindingContext;

            // Confirmaci�n de eliminaci�n (opcional)
            var result = await DisplayAlert("Confirmar", $"�Est�s seguro de que deseas eliminar al cliente {clienteSele.nombre}?", "S�", "No");
            if (result)
            {
                await localDbService.EliminarCliente(db, clienteSele.idCliente);

                // Recargar la lista de clientes
                CargarClientes();
            }
        }

    }

}
