using CrisoftApp.Models.Rols;
using CrisoftApp.Pages.Botones;
using CrisoftApp.DataService;
using System.Linq;

namespace CrisoftApp.Pages.Admin
{
    public partial class ListadoPersonal : ContentPage
    {
        private LocalDbService localDbService;
        string db = "Joyeria.db";

        private List<Models.Rols.Usuario> usuarios = new List<Models.Rols.Usuario>();

        public ListadoPersonal()
        {
            InitializeComponent();
            localDbService = new LocalDbService();
            BindingContext = new Models.Rols.Usuario();

            // Cargar usuarios al iniciar la página
            CargarUsuarios();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            CargarUsuarios();
        }

        private void CargarUsuarios()
        {
            usuarios = localDbService.ObtenerUsuarios(db).ToList();
            listaUsuarios.ItemsSource = usuarios;
        }

        private void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (e.NewTextValue != null)
            {
                var filtro = e.NewTextValue.ToLower();
                if (string.IsNullOrEmpty(filtro))
                {
                    listaUsuarios.ItemsSource = usuarios;
                }
                else
                {
                    listaUsuarios.ItemsSource = usuarios
                        .Where(u => u.IdUsuario.ToString().ToLower().Contains(filtro) ||
                                    u.Gmail.ToLower().Contains(filtro) ||
                                    u.Rol.ToString().ToLower().Contains(filtro)).ToList();
                }
            }
        }

        private async void btn_ModificarClicked(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            var usuarioSele = (Models.Rols.Usuario)btn.BindingContext;
            ModificarUsuario mu = new ModificarUsuario(usuarioSele);
            await Navigation.PushModalAsync(mu);
        }

        private async void btn_EliminarClicked(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            var usuarioSele = (Models.Rols.Usuario)btn.BindingContext;

            // Confirmación de eliminación (opcional)
            var result = await DisplayAlert("Confirmar", $"¿Estás seguro de que deseas eliminar al usuario {usuarioSele.Gmail}?", "Sí", "No");
            if (result)
            {
                await localDbService.EliminarUsuario(db, usuarioSele.IdUsuario);

                // Recargar la lista de usuarios
                CargarUsuarios();
            }
        }

        private async void OnVolverClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
