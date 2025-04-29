namespace CrisoftApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new NavigationPage(new MainPage()); // Aquí envolvemos MainPage en un NavigationPage
        }
    }
}
//namespace CrisoftApp
//{
//    public partial class App : Application
//    {
//        public App()
//        {
//            InitializeComponent();

//            MainPage = new AppShell();
//        }
//    }
//}
