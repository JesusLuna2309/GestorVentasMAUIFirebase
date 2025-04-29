using CrisoftApp.Models.Entidades;

namespace CrisoftApp.Models.Tablas_Relacionales
{
    public class PALP
    {
        public Articulo articulo { get; set; } = new Articulo();
        public Pedido pedido { get; set; } = new Pedido();
        public LineasPedido lineasPedido { get; set; } = new LineasPedido();
        public Cliente cliente { get; set; }

        public PALP()
        {
        }

        public PALP(Pedido pedido, Articulo articulo, LineasPedido lineasPedido)
        {
            this.pedido = pedido;
            this.articulo = articulo;
            this.lineasPedido = lineasPedido;
        }
    }
}
