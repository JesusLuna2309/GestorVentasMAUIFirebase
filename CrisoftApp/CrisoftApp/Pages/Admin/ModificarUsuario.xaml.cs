using CrisoftApp.DataService;
using CrisoftApp.Models.Rols;
using System;
using System.Text.RegularExpressions;

namespace CrisoftApp.Pages.Admin
{
    public partial class ModificarUsuario : ContentPage
    {
        private CrisoftApp.Models.Rols.Usuario usuario;  // Usar la ruta completa
        private LocalDbService localDbService;

        private string nombreDb = "Joyeria.db";

        public ModificarUsuario(CrisoftApp.Models.Rols.Usuario user)  // Usar la ruta completa
        {
            InitializeComponent();

            localDbService = new LocalDbService();

            usuario = user;

            UsuarioCargado();
        }

        private async void OnModificarTapped(object sender, EventArgs e)
        {
            try
            {
                // Se almacenan los datos nuevos
                string nombreA = NombreEntry.Text;
                string correoA = CorreoElectronicoEntry.Text;
                string contraseñaA = ContraseñaEntry.Text;
                //string repetirContraseñaA = RepetirContraseñaEntry.Text;

                // Validar que no hay campos vacíos
                if (string.IsNullOrEmpty(nombreA) ||
                    string.IsNullOrEmpty(correoA) ||
                    string.IsNullOrEmpty(contraseñaA))
                    //string.IsNullOrEmpty(repetirContraseñaA))
                {
                    await DisplayAlert("Error", "Todos los campos deben ser completados", "OK");
                    return;
                }

                // Validar formato de email
                if (!IsValidEmail(correoA))
                {
                    await DisplayAlert("Error", "El email debe tener un formato válido (ejemplo@gmail.com)", "OK");
                    return;
                }

                // Validar que las contraseñas coincidan
                //if (contraseñaA != repetirContraseñaA)
                //{
                //    await DisplayAlert("Error", "Las contraseñas no coinciden", "OK");
                //    return;
                //}

                // Crear un objeto Usuario con los nuevos datos
                var updatedUsuario = new CrisoftApp.Models.Rols.Usuario  // Usar la ruta completa
                {
                    IdUsuario = usuario.IdUsuario,
                    Nombre = nombreA,
                    Gmail = correoA,
                    Contra = contraseñaA,
                    Rol = (Rol)RolPicker.SelectedIndex, // Asignar el rol seleccionado
                    FechaRegistro = usuario.FechaRegistro // Conservar la fecha original de registro
                };

                // Actualizar el usuario en la base de datos local SQLite
                await localDbService.UpdateUsuario(nombreDb, updatedUsuario);

                // Navegar de regreso a la página anterior
                await Navigation.PopModalAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Ocurrió un error al editar el usuario: {ex.Message}", "OK");
            }
        }

        private async void OnVolverClicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }

        public void UsuarioCargado()
        {
            NombreEntry.Text = usuario.Nombre;
            CorreoElectronicoEntry.Text = usuario.Gmail;
            ContraseñaEntry.Text = usuario.Contra;
            //RepetirContraseñaEntry.Text = usuario.Contra; // Por defecto repetir contraseña es igual a contraseña
            RolPicker.SelectedIndex = (int)usuario.Rol; // Seleccionar el rol actual del usuario
        }

        private bool IsValidEmail(string email)
        {
            string pattern = @"^[^@\s]+@gmail\.com$";
            return Regex.IsMatch(email, pattern);
        }
    }
}
 