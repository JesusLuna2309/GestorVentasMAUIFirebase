using CrisoftApp.Models.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrisoftApp.Models.Tablas_Relacionales
{
    public class PA
    {
        public Articulo Articulo { get; set; }
        public LineasPedido LineasPedidos { get; set; }
    }
}
