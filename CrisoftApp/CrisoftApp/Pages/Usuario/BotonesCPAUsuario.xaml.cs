using CrisoftApp.DataService;

namespace CrisoftApp.Pages.Usuario
{
    public partial class BotonesCPAUsuario : ContentPage
    {
        public LocalDbService localDbService;
        private int idUsu = 0;
        private float totalGastado;
        public string nombreDb;

        public BotonesCPAUsuario(int idUsuario)
        {
            localDbService = new LocalDbService();
            InitializeComponent();
            this.idUsu = idUsuario;
        }

        private async void OnCatalogoTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new NavigationPage(new CatalogoUsuarios(idUsu)));
        }

        private async void OnMisPedidosTapped(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new NavigationPage(new PedidosUsuario(idUsu)));//Error de Navigation para guardar en documento
        }

        private async void OnPedirCitaTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new NavigationPage(new AgregarAgenda(idUsu)));

        }
    }
}
