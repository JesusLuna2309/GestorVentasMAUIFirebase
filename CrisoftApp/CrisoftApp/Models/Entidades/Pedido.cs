using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrisoftApp.Models.Entidades
{
    public class Pedido
    {
        public Pedido() { }

        public Pedido(DateTime? fecha, int idCliente, float total, int enviado)
        {
            this.fecha = fecha;
            this.idCliente = idCliente;
            this.total = total;
            this.enviado = enviado;
        }

        public int idPedido { get; set; }
        public DateTime? fecha { get; set; }
        public int idCliente { get; set; }
        public float total { get; set; }
        public int enviado { get; set; }
    }
}
