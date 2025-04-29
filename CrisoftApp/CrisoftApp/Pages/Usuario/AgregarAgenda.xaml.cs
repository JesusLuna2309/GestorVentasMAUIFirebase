using CrisoftApp.DataService;
using CrisoftApp.Models.Rols;
using CrisoftApp.Models.Entidades;
using System.Diagnostics;
namespace CrisoftApp.Pages.Usuario
{
    public partial class AgregarAgenda : ContentPage
    {
        public LocalDbService dbService;

        private string nombreDb = "Joyeria.db";

        public AgregarAgenda(int idUsu)
        {
            InitializeComponent();
            dbService = new LocalDbService();

            List<Models.Rols.Usuario> usuarios = dbService.ObtenerIdYNombreUsuarios("Joyeria.db");

            foreach (var usuario in usuarios)
            {
                clientePicker.Items.Add($"{usuario.IdUsuario}: {usuario.Nombre}");
            }
        }

        private async void Guardar_Clicked(object sender, EventArgs e)
        {
            try
            {
                // Verificar si se ha seleccionado un cliente en el Picker
                if (clientePicker.SelectedIndex == -1)
                {
                    await DisplayAlert("Error", "Por favor, seleccione un cliente.", "Aceptar");
                    return;
                }

                // Obtener el ID del cliente seleccionado del Picker
                string clienteSeleccionado = clientePicker.SelectedItem.ToString();
                int idCliente = int.Parse(clienteSeleccionado.Split(':')[0].Trim());

                // Obtener los valores de los otros controles
                DateTime fecha = fechaPicker.Date;
                TimeSpan hora = horaPicker.Time;
                string notas = notasEntry.Text;

                // Verificar valores antes de crear la Agenda
                Debug.WriteLine($"Fecha: {fecha}, Hora: {hora}, Notas: {notas}");

                // Crear un nuevo objeto Agenda
                Agenda agenda = new Agenda(idCliente, fecha, hora, notas);

                // Insertar la agenda en la base de datos
                await dbService.InsertarAgenda("Joyeria.db", agenda);

                // Mostrar mensaje de éxito
                await DisplayAlert("Éxito", "Los datos se han guardado correctamente.", "Aceptar");
            }
            catch (Exception ex)
            {
                // Mostrar mensaje de error si ocurre alguna excepción
                await DisplayAlert("Error", $"Ha ocurrido un error: {ex.Message}", "Aceptar");
            }
        }
    }
}
