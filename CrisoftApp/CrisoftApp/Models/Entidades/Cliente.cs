using SQLite;

namespace CrisoftApp.Models.Entidades
{
    [Table("Cliente")]
    public class Cliente
    {
        [PrimaryKey]
        [AutoIncrement]
        [Column("Id")]
        public int idCliente { get; set; }

        [Column("Nombre")]
        public string nombre { get; set; }

        [Column("Email")]
        public string email { get; set; }

        [Column("Direccion")]
        public string dirección { get; set; }

        [Column("Localidad")]
        public string localidad { get; set; }

        [Column("Provincia")]
        public string provincia { get; set; }

        [Column("CodigoPostal")]
        public string codigoPostal { get; set; }

        [Column("Pais")]
        public string pais { get; set; }

        [Column("Ruta")]
        public string ruta { get; set; }

        [Column("Ubicacion")]
        public string ubicación { get; set; }

        [Column("Enviado")]
        public bool enviado { get; set; }

        [Column("Rol")]
        public bool rol { get; set; }

        public List<Cliente> Clientes { get; set; }

        public Cliente() { }

        public Cliente(int idCliente, string nombre, string email, string dirección, string localidad, string provincia, string codigoPostal, string pais, string ruta, string ubicación, bool enviado)
        {
            this.idCliente = idCliente;
            this.nombre = nombre;
            this.email = email;
            this.dirección = dirección;
            this.localidad = localidad;
            this.provincia = provincia;
            this.codigoPostal = codigoPostal;
            this.pais = pais;
            this.ruta = ruta;
            this.ubicación = ubicación;
            this.enviado = enviado;
        }

        public Cliente (int IdCliente, string Nombre)
        {
            this.idCliente = IdCliente;
            this.nombre = Nombre;
        }
    }
}
