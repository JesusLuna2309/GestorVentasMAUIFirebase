namespace CrisoftApp.Models.Rols
{
    public enum Rol
    {
        Admin,
        Usuario
    }

    public class Usuario
    {
        public int IdUsuario { get; set; }
        public string Nombre { get; set; }
        public string Gmail { get; set; }
        public string Contra { get; set; }
        public Rol Rol { get; set; }

        public DateTime FechaRegistro { get; set; }

        // Constructor sin parámetros
        public Usuario()
        {
            FechaRegistro = DateTime.Now; // Establecer la fecha de registro al momento de crear el objeto
        }

        // Constructor con parámetros para Gmail, Contraseña y Nombre
        public Usuario(string nombre, string gmail, string contra)
        {
            Nombre = nombre;
            Gmail = gmail;
            Contra = contra;
            Rol = Rol.Usuario; // Valor por defecto
            FechaRegistro = DateTime.Now;
        }

        // Constructor con parámetros para Gmail, Contraseña, Rol y Nombre
        public Usuario(string nombre, string gmail, string contra, Rol rol)
        {
            Nombre = nombre;
            Gmail = gmail;
            Contra = contra;
            Rol = rol;
            FechaRegistro = DateTime.Now;
        }

        // Constructor con parámetros para IdUsuario, Gmail, Contraseña, Rol y Nombre
        public Usuario(int idUsuario, string nombre, string gmail, string contra, Rol rol)
        {
            IdUsuario = idUsuario;
            Nombre = nombre;
            Gmail = gmail;
            Contra = contra;
            Rol = rol;
            FechaRegistro = DateTime.Now;
        }
    }
}
