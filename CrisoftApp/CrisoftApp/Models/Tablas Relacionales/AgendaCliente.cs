using CrisoftApp.Models.Entidades;

namespace CrisoftApp.Models.Tablas_Relacionales
{
    public class AgendaCliente
    {
        public Agenda agenda { get; set; }
        public Cliente cliente { get; set; }
    }
}
