using CrisoftApp.DataService;
using CrisoftApp.Models.Rols;
using CrisoftApp.Pages;
using CrisoftApp.Pages.Usuario;
using System.Text.RegularExpressions;

namespace CrisoftApp
{
    public partial class MainPage : ContentPage
    {
        // C:\Users\USER\AppData\Local\Packages\com.companyname.crisoftapp_9zz4h110yvjzm\LocalState

        // Casa: C:\Users\Usuario\AppData\Local\Packages\com.companyname.crisoftapp_9zz4h110yvjzm\LocalState

        // PC: C:\Users\PC GAMING\AppData\Local\Packages\com.companyname.crisoftapp_9zz4h110yvjzm\LocalState

        LocalDbService services;
        ConexionFirebase cf = new ConexionFirebase();
        string nombreDB = "Joyeria.db";
        string urlClientes = "https://crisoft.es/appCrijoya/clientes.json";
        string urlMarcas = "https://crisoft.es/appCrijoya/marcas.json";
        string urlArticulos = "https://crisoft.es/appCrijoya/articulos.json";
        string urlCategorias = "https://crisoft.es/appCrijoya/categorias.json";

        public MainPage()
        {
            InitializeComponent();
            services = new LocalDbService();
        }

        private async void OnCounterClicked(object sender, EventArgs e)
        {
            string email = GmailEntry.Text;
            string password = PasswordEntry.Text;

            if (!IsValidEmail(email))
            {
                await DisplayAlert("Correo inválido", "Por favor, ingresa un correo válido.", "OK");
                return;
            }

            try
            {
                var userCredential = await cf.CargarUsuario(email, password);
                await DisplayAlert("Login Exitoso", "Bienvenido", "OK");

                // Verificar si la base de datos local existe
                if (await CheckDatabaseExistence())
                {
                    // Obtener el IdUsuario
                    int idUsuario = services.ObtenerIdUsuario(nombreDB, email, password);

                    // Redirigir a la vista correspondiente según el rol del usuario
                    Rol rolUsuario = await cf.ConsultarRolUsuarioAsync(email);

                    if (rolUsuario == Rol.Admin)
                    {
                        await Navigation.PushAsync(new BotonesCPA());
                    }
                    else if (rolUsuario == Rol.Usuario)
                    {
                        await Navigation.PushAsync(new BotonesCPAUsuario(idUsuario));
                    }
                    else
                    {
                        // El rol del usuario no está definido correctamente
                        await DisplayAlert("Error", "El rol del usuario no está definido correctamente", "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Sin conexión", "No puedes acceder por primera vez sin conexión", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error de autenticación", "Correo o contraseña incorrectos", "OK");
                //Debug.WriteLine($"Error de autenticación: {ex.Message}");
            }
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return false;
            }

            string pattern = @"^[^@\s]+@[^@\s]+\.(com|es|net|org)$";
            return Regex.IsMatch(email, pattern);
        }

        private async Task<bool> CheckDatabaseExistence()
        {
            if (!services.DatabaseExists(nombreDB))
            {
                var current = Connectivity.NetworkAccess;

                if (current == NetworkAccess.Internet)
                {
                    DescargarEInsertarClientes();
                    services.CrearTablaAgenda(nombreDB);
                    DescargarEInsertarMarcas();
                    DescargarEInsertarCategorias();
                    DescargarEInsertarArticulos();
                    services.CrearTablaPedidos(nombreDB);
                    services.CrearTablaLineasPedidos(nombreDB);
                    services.CrearTablaUsuarios(nombreDB);
                    services.CrearTablaUP(nombreDB);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        private async void DescargarEInsertarClientes()
        {
            await services.DownloadAndInsertClientesAsync(nombreDB, urlClientes);
        }

        private async void DescargarEInsertarMarcas()
        {
            await services.DownloadAndInsertMarcasAsync(nombreDB, urlMarcas);
        }

        private async void DescargarEInsertarCategorias()
        {
            await services.DownloadAndInsertCategoriasAsync(nombreDB, urlCategorias);
        }

        private async void DescargarEInsertarArticulos()
        {
            await services.DownloadAndInsertArticulosAsync(nombreDB, urlArticulos);
        }

        private async void btnRegistrarClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Pages.RyL.Registro());
        }
    }
}
