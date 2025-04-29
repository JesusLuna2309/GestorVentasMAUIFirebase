using CrisoftApp.Models.Entidades;
using CrisoftApp.DataService;
using System.Linq;

namespace CrisoftApp.Pages.Admin
{
    public partial class Agenda : ContentPage
    {
        private LocalDbService localDbService;
        string db = "Joyeria.db";

        private List<CrisoftApp.Models.Entidades.Agenda> agendaList = new List<CrisoftApp.Models.Entidades.Agenda>();

        public Agenda()
        {
            InitializeComponent();
            localDbService = new LocalDbService();
            BindingContext = new CrisoftApp.Models.Entidades.Agenda();

            // Cargar eventos de la agenda al iniciar la página
            CargarAgenda();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            CargarAgenda();
        }

        private void CargarAgenda()
        {
            agendaList = localDbService.ObtenerAgendas(db).ToList();
            listaAgenda.ItemsSource = agendaList;
        }

        //private void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    if (e.NewTextValue != null)
        //    {
        //        var filtro = e.NewTextValue.ToLower();
        //        if (string.IsNullOrEmpty(filtro))
        //        {
        //            listaAgenda.ItemsSource = agendaList;
        //        }
        //        else
        //        {
        //            listaAgenda.ItemsSource = agendaList
        //                .Where(a => a.idEvento.ToString().ToLower().Contains(filtro) ||
        //                            a.idCliente.ToString().ToLower().Contains(filtro) ||
        //                            (a.fecha.HasValue && a.fecha.Value.ToString("dd/MM/yyyy").ToLower().Contains(filtro)) ||
        //                            (a.hora.HasValue && a.hora.Value.ToString("hh\\:mm").ToLower().Contains(filtro)) ||
        //                            (a.notas != null && a.notas.ToLower().Contains(filtro))).ToList();
        //        }
        //    }
        //}
    }
}
