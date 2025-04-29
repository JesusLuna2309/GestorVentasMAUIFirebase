using CrisoftApp.DataService;
using CrisoftApp.Models.Entidades;
using CrisoftApp.Pages.Botones;

namespace CrisoftApp.Pages.Secundarios
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

            // Cargar clientes al iniciar la página
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

        private async void OnAñadirClienteClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AñadirClientes());
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

        private async void listaCl_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            var clienteSeleccionado = (Cliente)e.Item;
            await Navigation.PushAsync(new Catalogo(clienteSeleccionado));
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
            // Lógica para manejar la pulsación del botón de búsqueda
        }
    }

}
