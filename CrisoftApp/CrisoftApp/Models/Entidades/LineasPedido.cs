namespace CrisoftApp.Models.Entidades
{
    public class LineasPedido
    {
        public int idLineaPedido { get; set; }
        public int idPedido { get; set; }
        public int idArticulo { get; set; }
        public string? referencia { get; set; }
        public int coste { get; set; }
        public int venta { get; set; }
        public int unidades { get; set; }
        public float total { get; set; }


        public LineasPedido()
        {

        }

        public LineasPedido(int idPedido, int idArticulo, string? referencia, int coste, int venta, int unidades, float total)
        {
            this.idPedido = idPedido;
            this.idArticulo = idArticulo;
            this.referencia = referencia;
            this.coste = coste;
            this.venta = venta;
            this.unidades = unidades;
            this.total = total;
        }
    }
}
