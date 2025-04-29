using Firebase.Auth;
using Firebase.Auth.Providers;
using Firebase.Database;
using CrisoftApp.Models.Rols;
using System;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database.Query;
using FirebaseAdmin.Auth;

namespace CrisoftApp.DataService
{
    internal class ConexionFirebase
    {
        FirebaseClient cl = new FirebaseClient("https://crisoftapp-1114d-default-rtdb.europe-west1.firebasedatabase.app/");

        public static FirebaseAuthClient Conectar()
        {
            var config = new FirebaseAuthConfig
            {
                ApiKey = "AIzaSyDp_gRx4QcaQl9JejV2aJzUqNIYduBhaIA",
                AuthDomain = "crisoftapp-1114d.firebaseapp.com",
                Providers = new FirebaseAuthProvider[]
                {
                    new GoogleProvider().AddScopes("email"),
                    new EmailProvider()
                }
            };

            var client = new FirebaseAuthClient(config);
            return client;
        }

        //Al pulsar el boton de Iniciar sesion nos vamos a la BD para comprobar si existe el gmail, si existe nos dejara pasar
        public async Task<UserCredential> CargarUsuario(string Email, string Password)
        {
            var cliente = Conectar();
            var userCredential = await cliente.SignInWithEmailAndPasswordAsync(Email, Password);
            return userCredential;
        }

        //Creamos el usuario en firebase con los datos del usuario.
        public async Task<UserCredential> CrearUsuario(string Email, string Password, string Nombre)
        {
            var cliente = Conectar();
            var userCredential = await cliente.CreateUserWithEmailAndPasswordAsync(Email, Password, Nombre);
            return userCredential;
        }

        //Comprobamos el gmail para saber si existe en la BD con un booleano, devolvera true si existe.
        public async Task<bool> ExisteGmailEnBD(string Gmail)
        {
            var usuarios = await cl.Child("Usuarios").OnceAsync<Usuario>();
            return usuarios.Any(u => u.Object.Gmail == Gmail);
        }

        //Recogemos el Gmail y contraseña y lo introducimos en firebase.
        public async Task RegistrarDBGmail(string Gmail, string Contra)
        {
            if (await ExisteGmailEnBD(Gmail))
            {
                throw new Exception("El correo electrónico ya está registrado en la base de datos.");
            }

            // Generar el ID de usuario manualmente
            //var newUser = await cl.Child("Usuarios").PostAsync(new Usuario { Gmail = Gmail, Contra = Contra, Rol = Rol.Usuario });

            // Obtener el ID generado y actualizar el objeto Usuario
            //int userId = newUser.Key;
            //await cl.Child("Usuarios").Child(userId).PutAsync(new Usuario { IdUsuario = userId, Gmail = Gmail, Contra = Contra, Rol = Rol.Usuario });

        }

        //Metodo para comprobar el Rol del usuario del gmail pasado por parametro
        public async Task<Rol> ConsultarRolUsuarioAsync(string email)
        {
            try
            {
                var usuarios = await cl.Child("Usuarios").OnceAsync<Usuario>();
                Rol tmp = new Rol();
                tmp = Rol.Usuario;
                foreach (var usuario in usuarios)
                {
                    if (usuario.Object.Gmail == email)
                    {
                        // Retorna el rol del usuario encontrado
                        tmp = usuario.Object.Rol;
                        //return usuario.Object.Rol;
                    }
                }

                // Si no se encontró el usuario, retorna un valor predeterminado
                //return Rol.Usuario;
                return tmp;
            }
            catch (Exception ex)
            {
                // Maneja cualquier excepción que pueda ocurrir durante la consulta
                Console.WriteLine($"Error al consultar el rol del usuario: {ex.Message}");
                // Retorna un valor predeterminado en caso de error
                return Rol.Usuario;
            }
        }
    }
}
