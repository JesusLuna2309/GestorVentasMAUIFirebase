using CrisoftApp.Models.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrisoftApp.Models.Tablas_Relacionales
{
    public class ArticuloCartel
    {
        public Articulo articulo { get; set; }
        // Variables para los carteles
        public string cartelOferta { get; set; }
        public string cartelNovedad { get; set; }
        public string colorFondoCartelOferta { get; set; }
        public string colorFondoCartelNovedad { get; set; }
    }
}
