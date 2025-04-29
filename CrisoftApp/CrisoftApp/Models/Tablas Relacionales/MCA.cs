using CrisoftApp.Models.Entidades;

namespace CrisoftApp.Models.Tablas_Relacionales
{
    public class MCA
    {
        public Articulo articulo { get; set; }
        public Categoria categoria { get; set; }
        public Marca marca { get; set; }
        public bool oferta { get; set; }
        public string precio { get; set; }

        public MCA()
        {

        }

        public MCA(Marca marca, Categoria categoria, Articulo articulo)
        {
            this.marca = marca;
            this.categoria = categoria;
            this.articulo = articulo;
        }

        public MCA(Marca marca, Categoria categoria, Articulo articulo, bool oferta, string precio)
        {
            this.marca = marca;
            this.categoria = categoria;
            this.articulo = articulo;
            this.oferta = oferta;
            this.precio = precio;
        }
    }
}
