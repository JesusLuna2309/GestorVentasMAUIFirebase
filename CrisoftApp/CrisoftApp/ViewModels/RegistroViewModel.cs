using System.Text.RegularExpressions;
using CrisoftApp.DataService;
using System.ComponentModel;
using CrisoftApp.Models.Rols;

namespace CrisoftApp.ViewModels
{
    public class RegistroViewModel : INotifyPropertyChanged
    {
        private string _nombre;
        private string _correoElectronico;
        private string _contraseña;
        private string _repetirContraseña;
        private ConexionFirebase _conexionFirebase;
        private LocalDbService _localDbService;

        private string nombreDB = "Joyeria.db";

        public RegistroViewModel()
        {
            _conexionFirebase = new ConexionFirebase();
            _localDbService = new LocalDbService();
            RegistrarCommand = new Command(async () => await RegistrarUsuario());
        }

        public string Nombre
        {
            get => _nombre;
            set
            {
                _nombre = value;
                OnPropertyChanged(nameof(Nombre));
            }
        }

        public string CorreoElectronico
        {
            get => _correoElectronico;
            set
            {
                _correoElectronico = value;
                OnPropertyChanged(nameof(CorreoElectronico));
            }
        }

        public string Contraseña
        {
            get => _contraseña;
            set
            {
                _contraseña = value;
                OnPropertyChanged(nameof(Contraseña));
            }
        }

        public string RepetirContraseña
        {
            get => _repetirContraseña;
            set
            {
                _repetirContraseña = value;
                OnPropertyChanged(nameof(RepetirContraseña));
            }
        }

        public Command RegistrarCommand { get; }

        private async Task RegistrarUsuario()
        {
            if (string.IsNullOrWhiteSpace(Nombre) || string.IsNullOrWhiteSpace(CorreoElectronico) || string.IsNullOrWhiteSpace(Contraseña) || string.IsNullOrWhiteSpace(RepetirContraseña))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Todos los campos son obligatorios", "OK");
                return;
            }

            if (!IsValidEmail(CorreoElectronico))
            {
                await Application.Current.MainPage.DisplayAlert("Correo inválido", "Por favor, ingresa un correo válido.", "OK");
                return;
            }

            if (Contraseña != RepetirContraseña)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Las contraseñas no coinciden", "OK");
                return;
            }

            try
            {
                // Crear un objeto Usuario con los datos del registro
                var nuevoUsuario = new Usuario
                {
                    Gmail = CorreoElectronico,
                    Nombre = Nombre,
                    Contra = Contraseña,
                    Rol = Rol.Admin
                };

                await _conexionFirebase.RegistrarDBGmail(CorreoElectronico, Contraseña);

                // Insertar el nuevo usuario en la base de datos SQLite
                await _localDbService.InsertarUsuario(nombreDB, nuevoUsuario);

                // Crear usuario en la autenticación de Firebase
                var nuevoUsuarioFirebase = await _conexionFirebase.CrearUsuario(CorreoElectronico, Contraseña, Nombre);

                // Guardar en la base de datos de Firebase Realtime Database


                // Mostrar mensaje de éxito o navegar a otra página
                await Application.Current.MainPage.DisplayAlert("Éxito", "Usuario registrado con éxito", "OK");
            }
            catch (Exception ex)
            {
                // Manejar el error (mostrar mensaje de error)
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }





        private bool IsValidEmail(string email)
        {
            string pattern = @"^[^@\s]+@[^@\s]+\.(com|es|net|org)$";
            return Regex.IsMatch(email, pattern);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }
}
