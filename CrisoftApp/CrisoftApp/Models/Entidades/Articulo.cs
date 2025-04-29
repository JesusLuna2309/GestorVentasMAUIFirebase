using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrisoftApp.Models.Entidades
{
    public class Articulo
    {
        [PrimaryKey, AutoIncrement]
        public int idArticulo { get; set; }
        public string? referencia { get; set; }
        public string? descripcion { get; set; }
        public string? idCategoria { get; set; }
        public string? idMarca { get; set; }
        public string? codidoBarras { get; set; }
        public int coste { get; set; }
        public string venta { get; set; }
        public int ventaOferta { get; set; }
        public DateTime? fechaAlta { get; set; }
        public int existencias { get; set; }
        public string? urlImagen1 { get; set; }
        public string? urlImagen2 { get; set; }
        public string? urlImagen3 { get; set; }
        public string? urlImagen4 { get; set; }
        public string? urlImagen5 { get; set; }
        public string? urlImagen6 { get; set; }
        public string? urlImagen7 { get; set; }

    }
}
