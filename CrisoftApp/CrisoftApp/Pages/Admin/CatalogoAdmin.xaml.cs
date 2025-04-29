using CrisoftApp.DataService;
using CrisoftApp.Models.Entidades;
using CrisoftApp.Models.Tablas_Relacionales;
using CrisoftApp.Pages.Botones;

namespace CrisoftApp.Pages.Admin;

public partial class CatalogoAdmin : ContentPage
{

    private LocalDbService localDbService;

    private Cliente clEscogido;

    string nombreDb = "Joyeria.db";

    public string id;
    public float tDinero;
    public int idPedido;

    private List<ArticuloCartel> listaAC;

    public CatalogoAdmin(Cliente cl)
	{
		InitializeComponent();

        localDbService = new LocalDbService();

        this.clEscogido = cl;

        listaAC = new List<ArticuloCartel>();

        totalprecio.Text = ("0" + " €");

        MessagingCenter.Subscribe<DetallesDelPedidoAdmin, float>(this, "UpdateTotal", (sender, total) =>
        {
            this.tDinero = total;
            totalprecio.Text = total.ToString() + " €";
        });

        // Se recibe el tDinero dedsde la página ResumenPedido y se actualiza
        MessagingCenter.Subscribe<ListadoPedidos, float>(this, "UpdateResumen", (sender, total) =>
        {
            this.tDinero = total;
            totalprecio.Text = total.ToString() + " €";
        });

        LeerYGuardarArticulos();
    }
    private async void LeerYGuardarArticulos()
    {
        try
        {
            // Crear el pedido
            var pedido = new Pedido
            {
                fecha = DateTime.Today,
                idCliente = clEscogido.idCliente,
                total = tDinero,
                enviado = 0
            };

            // Insertar el pedido y obtener el ID
            this.idPedido = await localDbService.InsertarPedidos(nombreDb, pedido);

            // Verificar si el pedido se insertó correctamente
            if (idPedido == -1)
            {
                Console.WriteLine("Error al insertar el pedido.");
                return;
            }

            // Continuar con el resto del código para obtener y mostrar artículos
            var art = localDbService.ObtenerArticulos(nombreDb);
            string cartelof = null;
            string cartelno = null;
            string colorof = "";
            string colorno = "";

            if (art != null)
            {
                foreach (var articul in art)
                {
                    if (articul.existencias > 0)
                    {
                        TimeSpan diferencia = (TimeSpan)(DateTime.Now - articul.fechaAlta);

                        if (articul.ventaOferta.ToString() == articul.venta)
                        {
                            articul.venta = null;
                            colorof = "Transparent";
                        }
                        else
                        {
                            articul.venta = articul.venta + " €";
                            cartelof = "OFERTA";
                            colorof = "Red";
                        }

                        if (diferencia.TotalDays < 80)
                        {
                            cartelno = "NOVEDAD";
                            colorno = "Green";
                        }
                        else
                        {
                            cartelno = "";
                            colorno = "Transparent";
                        }

                        listaAC.Add(new ArticuloCartel
                        {
                            articulo = articul,
                            cartelOferta = cartelof,
                            cartelNovedad = cartelno,
                            colorFondoCartelOferta = colorof,
                            colorFondoCartelNovedad = colorno
                        });

                        cartelno = null;
                        cartelof = null;
                        colorof = null;
                        colorno = null;
                    }
                }
            }

            if (listaAC.Count == 0)
            {
                Resultado.Text = "SIN RESULTADOS";
                Resultado.TextColor = Colors.Black;
            }
            else
            {
                Resultado.Text = "";
                Resultado.TextColor = Colors.White;
            }

            listaCat.ItemsSource = listaAC;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al cargar clientes: {ex}");
        }
    }


    private async void OnPedidosTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Pedidos());
    }

    //Este es el boton que aparece en cada uno de los objetos para indicar que lo añadimos a la bolsa. 
    private async void clicked_btncarrito(object sender, EventArgs e)
    {
        var button = sender as Button;
        // Se obtiene el artículo seleccionado de la lista ArticuloCartel
        var articulocartelSeleccionado = (ArticuloCartel)button.BindingContext;
        var articuloSeleccionado = articulocartelSeleccionado.articulo;
        if (button != null)
        {
            int exist = localDbService.ObtenerExistencias(nombreDb, articulocartelSeleccionado.articulo.idArticulo);
            if (exist > 0)
            {
                // Se actualiza el total con el nuevo artículo seleccionado
                tDinero = tDinero + articuloSeleccionado.ventaOferta;
                totalprecio.Text = tDinero.ToString() + " €";

                // Se comprueba que el producto está añadido o no
                bool addp = LocalDbService.ComprobarArticuloEnPedido(nombreDb, idPedido, articuloSeleccionado.idArticulo);
                // Si está, se actualiza las unidades del producto
                if (addp)
                {
                    localDbService.ActualizarUnidadesPedidos(nombreDb, idPedido, articuloSeleccionado.idArticulo, 0, tDinero);
                    localDbService.ActualizarExistenciasR(nombreDb, articuloSeleccionado.idArticulo);
                }
                // Si no está, se añade nuevo
                else
                {
                    // Crear una instancia de LineasPedido
                    var lineaPedido = new LineasPedido
                    {
                        idPedido = idPedido,
                        idArticulo = articuloSeleccionado.idArticulo,
                        referencia = articuloSeleccionado.referencia,
                        coste = articuloSeleccionado.coste,
                        venta = articuloSeleccionado.ventaOferta,
                        unidades = 1, // Solo se añade una unidad
                        total = tDinero
                    };

                    // Llamar al método InsertarLineasPedidos con el objeto LineasPedido
                    await localDbService.InsertarLineasPedidos(nombreDb, lineaPedido);

                    localDbService.ActualizarExistenciasR(nombreDb, articuloSeleccionado.idArticulo);
                }
            }
            else
            {
                await DisplayAlert("SIN STOCK", "El producto seleccionado se ha quedado sin existencias", "OK");
            }
        }
    }

    private async void Clicked_btndetalle(object sender, EventArgs e)
    {
        var button = sender as Button;
        // Se obtiene el articulo seleccionado de la lista ArticuloCartel y se envía a la página ClienteDetalle
        var articulocartelSeleccionado = (ArticuloCartel)button.BindingContext;
        var articuloSeleccionado = articulocartelSeleccionado.articulo;
        Page mapaPage;

        // Verifica la plataforma actual
        //mapaPage = new DetallesDelPedido(articuloSeleccionado, idPedido, tDinero, articulocartelSeleccionado.cartelOferta, articulocartelSeleccionado.cartelNovedad, articulocartelSeleccionado.colorFondoCartelOferta, articulocartelSeleccionado.colorFondoCartelNovedad);

        // Navegar a la página correspondiente
        //await Navigation.PushModalAsync(mapaPage);
    }

    private async void Clicked_btnComprar(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Pages.Secundarios.ListadoPedidos(idPedido, tDinero));
    }

    private async void OnFilterButtonClicked(object sender, EventArgs e)
    {
        try
        {
            // Se muestran las opciones al producto
            string? opcionPrincipalSeleccionada = await DisplayActionSheet("Seleccionar opción", "Cancelar", null, "Novedades", "Ofertas", "Precio", "Categorías", "Marcas", "Eliminar Filtro");
            string? opc = "";

            if (opcionPrincipalSeleccionada != null && opcionPrincipalSeleccionada != "Cancelar")
            {
                // Se crea la lista para almacenar las subopciones
                List<string> subopciones = new List<string>();
                string? subopcionSeleccionada = "";
                if (opcionPrincipalSeleccionada == "Categorías")
                {
                    opc = "categoría";
                    // Si se selecciona Categorías se obtienen todas las categorías de la base de datos y se añaden a la lista
                    var categorias = localDbService.ObtenerCategorias(nombreDb);
                    foreach (var categoria in categorias)
                    {
                        subopciones.Add(categoria.descripcion);
                    }

                    // Finalmente se muestra las categorías para elegir
                    subopcionSeleccionada = await DisplayActionSheet("Seleccionar una " + opc, "Cancelar", null, subopciones.ToArray());
                }

                else if (opcionPrincipalSeleccionada == "Marcas")
                {
                    opc = "marca";
                    // Si se selecciona Marcas se obtienen todas las marcas de la base de datos y se añaden a la lista
                    var marcas = localDbService.ObtenerMarcas(nombreDb);
                    foreach (var marca in marcas)
                    {
                        subopciones.Add(marca.descripcion);
                    }

                    // Finalmente se muestra las marcas para elegir
                    subopcionSeleccionada = await DisplayActionSheet("Seleccionar una " + opc, "Cancelar", null, subopciones.ToArray());
                }

                else if (opcionPrincipalSeleccionada == "Ofertas")
                {
                    // Si se selecciona Ofertas no se añade nada porque no hay subopciones
                    subopciones.Add("");
                }

                else if (opcionPrincipalSeleccionada == "Novedades")
                {
                    // Si se selecciona Novedades no se añade nada porque no hay subopciones
                    subopciones.Add("");
                }

                else if (opcionPrincipalSeleccionada == "Eliminar Filtro")
                {
                    // Si se selecciona Eliminar Filtro no se añade nada porque no hay subopciones
                    subopciones.Add("");
                }

                else if (opcionPrincipalSeleccionada == "Precio")
                {
                    // Si se selecciona Precio, se muestran las opciones de Ascendente o Descendente
                    subopcionSeleccionada = await DisplayActionSheet("Seleccionar un orden", "Cancelar", null, "Ascendente", "Descendente");
                }

                if (subopcionSeleccionada != null && subopcionSeleccionada != "Cancelar")
                {
                    // Si no se ha seleccionado cancelar o una opción vacía se llama al método que filtra los productos
                    await FiltrarProductos(opcionPrincipalSeleccionada, subopcionSeleccionada);
                }
            }
        }

        catch (Exception ex)
        {
            Console.WriteLine($"Error al mostrar menú desplegable: {ex}");
        }
    }

    private async Task FiltrarProductos(string opcionPrincipalSeleccionada, string subopcionSeleccionada)
    {
        if (opcionPrincipalSeleccionada == "Categorías")
        {
            // Si la opción seleccionada es Categorías se obtiene el id de la categoría seleccionada
            var idcate = localDbService.ObtenerCategoriasPorDescripcion(subopcionSeleccionada, nombreDb);

            // Se almacena en la clase el id
            foreach (var idc in idcate)
            {
                this.id = idc.idCategoria;
            }

            // Se obtiene los artículos con dicha categoría
            var articulos = localDbService.ObtenerArticulosPorCategoria(nombreDb, id);

            // Se limpia la lista
            listaAC.Clear();
            foreach (var articulo in articulos)
            {
                string? cartelof = null;
                string? cartelno = null;
                string? colorof = null;
                string? colorno = null;
                if (articulo != null)
                {
                    // Se calcula la diferencia de diferencias para establecer que productos son novedad
                    TimeSpan diferencia = DateTime.Now - (articulo.fechaAlta ?? DateTime.MinValue);
                    // Si no hay descuento se establace a nulo el precio venta para que no se muestre  
                    if (articulo.ventaOferta.ToString() == articulo.venta)
                    {
                        articulo.venta = null;
                        colorof = "Transparent";

                    }
                    // Si hay descuento se crea el cartel de "OFERTA"
                    else
                    {
                        articulo.venta = articulo.venta + " €";
                        cartelof = "OFERTA";
                        colorof = "Red";
                    }

                    // Si el producto tiene una antiguedad inferior a 60 días se crea el cartel de "NOVEDAD"
                    if (diferencia.TotalDays < 60)
                    {
                        cartelno = "NOVEDAD";
                        colorno = "Green";
                    }
                    else
                    {
                        cartelno = "";
                        colorno = "Transparent";
                    }

                    // Se crea la lista ArticuloCartel finalmente con el artículo y los carteles y sus colores
                    listaAC.Add(new ArticuloCartel { articulo = articulo, cartelOferta = cartelof, cartelNovedad = cartelno, colorFondoCartelOferta = colorof, colorFondoCartelNovedad = colorno });

                    // Se reinician las variables del cartel
                    cartelno = null;
                    cartelof = null;
                    colorof = null;
                    colorno = null;
                }
            }

            // Se limpia la lista y se manda para mostrarlos
            listaCat.ItemsSource = null;
            listaCat.ItemsSource = listaAC;
        }

        else if (opcionPrincipalSeleccionada == "Marcas")
        {
            // Si la opción seleccionada es Marcas se obtiene el id de la marca seleccionada
            var idmarca = localDbService.ObtenerMarcasPorDescripcion(nombreDb, subopcionSeleccionada);

            // Se almacena en la clase el id
            foreach (var idm in idmarca)
            {
                this.id = idm.idMarca;
            }

            // Se obtiene los artículos con dicha categoría
            var marcas = localDbService.ObtenerArticulosPorIdMarca(nombreDb, id);

            // Se limpia la lista
            listaAC.Clear();
            foreach (var mar in marcas)
            {
                string? cartelof = null;
                string? cartelno = null;
                string? colorof = null;
                string? colorno = null;
                if (mar != null)
                {
                    // Se calcula la diferencia de diferencias para establecer que productos son novedad
                    TimeSpan diferencia = DateTime.Now - (mar.fechaAlta ?? DateTime.MinValue);
                    // Si no hay descuento se establace a nulo el precio venta para que no se muestre  
                    if (mar.ventaOferta.ToString() == mar.venta)
                    {
                        mar.venta = null;
                        colorof = "Transparent";

                    }
                    // Si hay descuento se crea el cartel de "OFERTA"
                    else
                    {
                        mar.venta = mar.venta + " €";
                        cartelof = "OFERTA";
                        colorof = "Red";
                    }

                    // Si el producto tiene una antiguedad inferior a 60 días se crea el cartel de "NOVEDAD"
                    if (diferencia.TotalDays < 60)
                    {
                        cartelno = "NOVEDAD";
                        colorno = "Green";
                    }
                    else
                    {
                        cartelno = "";
                        colorno = "Transparent";
                    }

                    // Se crea la lista ArticuloCartel finalmente con el artículo y los carteles y sus colores
                    listaAC.Add(new ArticuloCartel { articulo = mar, cartelOferta = cartelof, cartelNovedad = cartelno, colorFondoCartelOferta = colorof, colorFondoCartelNovedad = colorno });

                    // Se reinician las variables del cartel
                    cartelno = null;
                    cartelof = null;
                    colorof = null;
                    colorno = null;
                }
            }

            // Se limpia la lista y se manda para mostrarlos
            listaCat.ItemsSource = null;
            listaCat.ItemsSource = listaAC;
        }

        else if (opcionPrincipalSeleccionada == "Ofertas")
        {
            // Se limpia la lista
            listaAC.Clear();
            // Se obtienen los artículos
            var articulos = localDbService.ObtenerArticulos(nombreDb);
            foreach (var articulo in articulos)
            {
                // Si hay descuento se calcula la diferencia de diferencias para establecer que productos son novedad
                if (articulo.venta != articulo.ventaOferta.ToString())
                {
                    TimeSpan diferencia = DateTime.Now - (articulo.fechaAlta ?? DateTime.MinValue);
                    string? cartelno = null;
                    string? colorno = null;
                    articulo.venta = articulo.venta + " €";

                    // Si el producto tiene una antiguedad inferior a 60 días se crea el cartel de "NOVEDAD"
                    if (diferencia.TotalDays < 60)
                    {
                        cartelno = "NOVEDAD";
                        colorno = "Green";
                    }
                    else
                    {
                        cartelno = "";
                        colorno = "Transparent";
                    }

                    // Se crea la lista ArticuloCartel finalmente con el artículo y los carteles y sus colores
                    listaAC.Add(new ArticuloCartel { articulo = articulo, cartelOferta = "OFERTA", cartelNovedad = cartelno, colorFondoCartelOferta = "Red", colorFondoCartelNovedad = colorno });
                }
            }

            // Se limpia la lista y se manda para mostrarlos
            listaCat.ItemsSource = null;
            listaCat.ItemsSource = listaAC;
        }

        else if (opcionPrincipalSeleccionada == "Novedades")
        {
            // Se limpia la lista
            listaAC.Clear();
            // Se obtienen los artículos
            var articulos = localDbService.ObtenerArticulos(nombreDb);
            foreach (var articulo in articulos)
            {
                // Se calcula la diferencia de diferencias para establecer que productos son novedad
                TimeSpan diferencia = DateTime.Now - (articulo.fechaAlta ?? DateTime.MinValue);
                // Si el producto tiene una antiguedad inferior a 60 días se crea el cartel de "NOVEDAD"
                if (diferencia.TotalDays < 60)
                {
                    string? cartelof = null;
                    string colorof = "";

                    // Si no hay descuento se establace a nulo el precio venta para que no se muestre  
                    if (articulo.ventaOferta.ToString() == articulo.venta)
                    {
                        articulo.venta = null;
                        colorof = "Transparent";

                    }
                    // Si hay descuento se crea el cartel de "OFERTA"
                    else
                    {
                        articulo.venta = articulo.venta + " €";
                        cartelof = "OFERTA";
                        colorof = "Red";
                    }

                    // Se crea la lista ArticuloCartel finalmente con el artículo y los carteles y sus colores
                    listaAC.Add(new ArticuloCartel { articulo = articulo, cartelOferta = cartelof, cartelNovedad = "NOVEDAD", colorFondoCartelOferta = colorof, colorFondoCartelNovedad = "Green" });
                }
            }

            // Se limpia la lista y se manda para mostrarlos
            listaCat.ItemsSource = null;
            listaCat.ItemsSource = listaAC;
        }

        else if (opcionPrincipalSeleccionada == "Eliminar Filtro")
        {
            // Se limpia la lista
            listaAC.Clear();
            // Se obtienen los artículos
            var articulos = localDbService.ObtenerArticulos(nombreDb);
            string? cartelof = null;
            string? cartelno = null;
            string? colorof = null;
            string? colorno = null;
            if (articulos != null)
            {
                foreach (var articulo in articulos) //cliente
                {
                    // Se calcula la diferencia de diferencias para establecer que productos son novedad
                    TimeSpan diferencia = DateTime.Now - (articulo.fechaAlta ?? DateTime.MinValue);
                    // Si no hay descuento se establace a nulo el precio venta para que no se muestre  
                    if (articulo.ventaOferta.ToString() == articulo.venta)
                    {
                        articulo.venta = null;
                        colorof = "Transparent";

                    }
                    // Si hay descuento se crea el cartel de "OFERTA"
                    else
                    {
                        articulo.venta = articulo.venta + " €";
                        cartelof = "OFERTA";
                        colorof = "Red";
                    }

                    // Si el producto tiene una antiguedad inferior a 60 días se crea el cartel de "NOVEDAD"
                    if (diferencia.TotalDays < 60)
                    {
                        cartelno = "NOVEDAD";
                        colorno = "Green";
                    }
                    else
                    {
                        cartelno = "";
                        colorno = "Transparent";
                    }

                    // Se crea la lista ArticuloCartel finalmente con el artículo y los carteles y sus colores
                    listaAC.Add(new ArticuloCartel { articulo = articulo, cartelOferta = cartelof, cartelNovedad = cartelno, colorFondoCartelOferta = colorof, colorFondoCartelNovedad = colorno });

                    // Se reinician las variables del cartel
                    cartelno = null;
                    cartelof = null;
                    colorof = null;
                    colorno = null;
                }

                // Se limpia la lista y se manda para mostrarlos
                listaCat.ItemsSource = null;
                listaCat.ItemsSource = listaAC;
            }
        }

        else if (opcionPrincipalSeleccionada == "Precio")
        {
            // Se limpia la lista
            listaAC.Clear();

            // Si la subopción seleccionada es Descendente se llama al método de la base de datos que devuelve los datos en orden descendente
            if (subopcionSeleccionada == "Descendente")
            {
                var articulos = localDbService.ObtenerArticulosPorVentaOfertaDescendente(nombreDb);
                string? cartelof = null;
                string? cartelno = null;
                string? colorof = null;
                string? colorno = null;
                if (articulos != null)
                {
                    foreach (var art in articulos)
                    {
                        // Se calcula la diferencia de diferencias para establecer que productos son novedad
                        TimeSpan diferencia = DateTime.Now - (art.fechaAlta ?? DateTime.MinValue);
                        // Si no hay descuento se establace a nulo el precio venta para que no se muestre 
                        if (art.ventaOferta.ToString() == art.venta)
                        {
                            art.venta = null;
                            colorof = "Transparent";
                        }
                        // Si hay descuento se crea el cartel de "OFERTA"
                        else
                        {
                            art.venta = art.venta + " €";
                            cartelof = "OFERTA";
                            colorof = "Red";
                        }

                        // Si el producto tiene una antiguedad inferior a 60 días se crea el cartel de "NOVEDAD"
                        if (diferencia.TotalDays < 60)
                        {
                            cartelno = "NOVEDAD";
                            colorno = "Green";
                        }
                        else
                        {
                            cartelno = "";
                            colorno = "Transparent";
                        }

                        // Se crea la lista ArticuloCartel finalmente con el artículo y los carteles y sus colores
                        listaAC.Add(new ArticuloCartel { articulo = art, cartelOferta = cartelof, cartelNovedad = cartelno, colorFondoCartelOferta = colorof, colorFondoCartelNovedad = colorno });

                        // Se reinician las variables del cartel
                        cartelno = null;
                        cartelof = null;
                        colorof = null;
                        colorno = null;
                    }
                }

                // Se ordena la lista en orden descendente por el precio
                listaAC = listaAC.OrderByDescending(item => item.articulo.ventaOferta).ToList();
                // Se limpia la lista y se manda para mostrarlos
                listaCat.ItemsSource = null;
                listaCat.ItemsSource = listaAC;
            }
            // Si la subopción seleccionada es Descendente se llama al método de la base de datos que devuelve los datos en orden descendente
            else
            {
                var articulos = localDbService.ObtenerArticulosPorVentaOfertaAscendente(nombreDb);
                string? cartelof = null;
                string? cartelno = null;
                string? colorof = null;
                string? colorno = null;
                if (articulos != null)
                {
                    foreach (var cliente in articulos)
                    {
                        // Se calcula la diferencia de diferencias para establecer que productos son novedad
                        TimeSpan diferencia = DateTime.Now - (cliente.fechaAlta ?? DateTime.MinValue);
                        // Si no hay descuento se establace a nulo el precio venta para que no se muestre 
                        if (cliente.ventaOferta.ToString() == cliente.venta)
                        {
                            cliente.venta = null;
                            colorof = "White";

                        }
                        // Si hay descuento se crea el cartel de "OFERTA"
                        else
                        {
                            cliente.venta = cliente.venta + " €";
                            cartelof = "OFERTA";
                            colorof = "Red";
                        }

                        // Si el producto tiene una antiguedad inferior a 60 días se crea el cartel de "NOVEDAD"
                        if (diferencia.TotalDays < 60)
                        {
                            cartelno = "NOVEDAD";
                            colorno = "Green";
                        }
                        else
                        {
                            cartelno = "";
                            colorno = "White";
                        }

                        // Se crea la lista ArticuloCartel finalmente con el artículo y los carteles y sus colores
                        listaAC.Add(new ArticuloCartel { articulo = cliente, cartelOferta = cartelof, cartelNovedad = cartelno, colorFondoCartelOferta = colorof, colorFondoCartelNovedad = colorno });

                        // Se reinician las variables del cartel
                        cartelno = null;
                        cartelof = null;
                        colorof = null;
                        colorno = null;
                    }
                }

                // Se ordena la lista en orden ascendente por el precio
                listaAC = listaAC.OrderBy(item => item.articulo.ventaOferta).ToList();
                // Se limpia la lista y se manda para mostrarlos
                listaCat.ItemsSource = null;
                listaCat.ItemsSource = listaAC;
            }
        }
    }
}