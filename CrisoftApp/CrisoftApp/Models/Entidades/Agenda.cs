using SQLite;

namespace CrisoftApp.Models.Entidades
{
    public class Agenda
    {
        [PrimaryKey, AutoIncrement]
        public int idEvento { get; set; }
        public int idUsuario { get; set; }
        public DateTime fecha { get; set; }
        public TimeSpan hora { get; set; }
        public string? notas { get; set; }

        public Agenda() { }

        public Agenda(int idUsuario, DateTime fecha, TimeSpan hora, string? notas)
        {
            this.idUsuario = idUsuario;
            this.fecha = fecha;
            this.hora = hora;
            this.notas = notas;
        }
    }
}
