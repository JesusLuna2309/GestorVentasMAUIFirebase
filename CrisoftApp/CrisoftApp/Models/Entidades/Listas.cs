using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrisoftApp.Models.Entidades
{
    public class Listas
    {
        public Marca[] marca { get; set; }
        public Cliente[] clientes { get; set; }
        public Articulo[] articulos { get; set; }
        public Categoria[] categorias { get; set; }
    }
}
