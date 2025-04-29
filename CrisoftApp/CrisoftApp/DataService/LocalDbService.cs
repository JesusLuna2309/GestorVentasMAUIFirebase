using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CrisoftApp.Models.Entidades;
using CrisoftApp.Models.Rols;
using CrisoftApp.Models.Tablas_Relacionales;
using Firebase.Database;
using Firebase.Database.Query;
using LiteDB;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using SQLite;

namespace CrisoftApp.DataService
{
    public partial class LocalDbService : ObservableObject
    {

        FirebaseClient client = new FirebaseClient("https://crisoftapp-1114d-default-rtdb.europe-west1.firebasedatabase.app/");

        static SQLiteAsyncConnection aConexion;

        public async Task BorrarTodosLosClientesFirebase()
        {
            try
            {
                await client
                    .Child("Clientes")
                    .DeleteAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al borrar todos los clientes de Firebase: {ex.Message}");
            }
        }

        public async Task BorrarTodosLosArticulosFirebase()
        {
            try
            {
                await client
                    .Child("Articulos")
                    .DeleteAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al borrar todos los artículos de Firebase: {ex.Message}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="databaseName"></param>
        /// <param name="jsonUrl"></param>
        /// <returns></returns>
        public async Task DownloadAndInsertClientesAsync(string databaseName, string jsonUrl)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(jsonUrl);
                    response.EnsureSuccessStatusCode();

                    string json = await response.Content.ReadAsStringAsync();
                    Listas lis = JsonConvert.DeserializeObject<Listas>(json);
                    InsertClientes(databaseName, lis.clientes);

                }
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Error al descargar el JSON: {ex.Message}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="databaseName"></param>
        /// <param name="clientes"></param>
        public async Task InsertClientes(string databaseName, Cliente[] clientes)
        {
            if (clientes == null)
            {
                Debug.WriteLine("La lista de clientes es nula.");
                return;
            }

            try
            {
                using (SqliteConnection connection = ConnectToDatabase(databaseName))
                {
                    connection.Open();

                    string createTableQuery = @"
                    CREATE TABLE IF NOT EXISTS Clientes(
                        IdCliente INTEGER PRIMARY KEY AUTOINCREMENT,
                        Nombre TEXT,
                        Email TEXT,
                        Dirección TEXT,
                        Localidad TEXT,
                        Provincia TEXT,
                        CodigoPostal TEXT,
                        Pais TEXT,
                        Ruta INTEGER,
                        Ubicación TEXT,
                        Enviado INTEGER
                    )";

                    using (var command = new SqliteCommand(createTableQuery, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    foreach (Cliente cliente in clientes)
                    {
                        string insertQuery = @"
                        INSERT INTO Clientes (
                            IdCliente,
                            Nombre,
                            Email,
                            Dirección,
                            Localidad,
                            Provincia,
                            CodigoPostal,
                            Pais,
                            Ruta,
                            Ubicación,
                            Enviado
                        ) 
                        SELECT @IdCliente, @Nombre, @Email, @Dirección, @Localidad, @Provincia, @CodigoPostal, @Pais, @Ruta, @Ubicación, @Enviado
                        WHERE NOT EXISTS (
                            SELECT 1 FROM Clientes WHERE IdCliente = @IdCliente
                        )";

                        using (var insertCommand = new SqliteCommand(insertQuery, connection))
                        {
                            insertCommand.Parameters.AddWithValue("@IdCliente", cliente.idCliente);
                            insertCommand.Parameters.AddWithValue("@Nombre", cliente.nombre);
                            insertCommand.Parameters.AddWithValue("@Email", cliente.email);
                            insertCommand.Parameters.AddWithValue("@Dirección", cliente.dirección);
                            insertCommand.Parameters.AddWithValue("@Localidad", cliente.localidad);
                            insertCommand.Parameters.AddWithValue("@Provincia", cliente.provincia);
                            insertCommand.Parameters.AddWithValue("@CodigoPostal", cliente.codigoPostal);
                            insertCommand.Parameters.AddWithValue("@Pais", cliente.pais);
                            insertCommand.Parameters.AddWithValue("@Ruta", cliente.ruta);
                            insertCommand.Parameters.AddWithValue("@Ubicación", cliente.ubicación);
                            insertCommand.Parameters.AddWithValue("@Enviado", cliente.enviado);

                            insertCommand.ExecuteNonQuery();
                        }

                        // Verificar si el cliente ya existe en Firebase
                        var clientesExistentes = await client.Child("Clientes")
                                                             .OrderByKey()
                                                             .EqualTo(cliente.idCliente.ToString())
                                                             .OnceAsync<Cliente>();

                        if (!clientesExistentes.Any())
                        {
                            // Crear objeto cliente para Firebase
                            // Guardar en Firebase
                            await client.Child("Clientes").Child(cliente.idCliente.ToString()).PutAsync(new Cliente
                            {
                                idCliente = cliente.idCliente,
                                nombre = cliente.nombre,
                                email = cliente.email,
                                dirección = cliente.dirección,
                                localidad = cliente.localidad,
                                provincia = cliente.provincia,
                                codigoPostal = cliente.codigoPostal,
                                pais = cliente.pais,
                                ruta = cliente.ruta,
                                ubicación = cliente.ubicación,
                                enviado = cliente.enviado
                            });
                        }
                        else
                        {
                            Debug.WriteLine($"Cliente con Id {cliente.idCliente} ya existe en Firebase.");
                        }
                    }

                    Debug.WriteLine("Clientes insertados correctamente en la base de datos y en Firebase.");
                }
            }
            catch (SqliteException e)
            {
                Debug.WriteLine($"Error al insertar clientes en la base de datos: {e.Message}");
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error al insertar clientes en Firebase: {e.Message}");
            }
        }




        public async Task InsertarLineasPedidos(string databaseName, params LineasPedido[] lineasPedidos)
        {
            if (lineasPedidos == null || lineasPedidos.Length == 0)
            {
                Debug.WriteLine("La lista de líneas de pedido es nula o está vacía.");
                return;
            }

            try
            {
                using (SqliteConnection connection = ConnectToDatabase(databaseName))
                {
                    connection.Open();

                    // No es necesario crear la tabla aquí si ya está creada en la base de datos SQLite

                    foreach (LineasPedido lineaPedido in lineasPedidos)
                    {
                        string insertQuery = @"
                        INSERT INTO LineasPedidos (
                            IdPedido,
                            IdArticulo,
                            Referencia,
                            Coste,
                            Venta,
                            Unidades,
                            Total
                        ) 
                        VALUES (
                            @IdPedido,
                            @IdArticulo,
                            @Referencia,
                            @Coste,
                            @Venta,
                            @Unidades,
                            @Total
                        )";

                        using (var insertCommand = new SqliteCommand(insertQuery, connection))
                        {
                            insertCommand.Parameters.AddWithValue("@IdPedido", lineaPedido.idPedido);
                            insertCommand.Parameters.AddWithValue("@IdArticulo", lineaPedido.idArticulo);
                            insertCommand.Parameters.AddWithValue("@Referencia", lineaPedido.referencia ?? "");
                            insertCommand.Parameters.AddWithValue("@Coste", lineaPedido.coste);
                            insertCommand.Parameters.AddWithValue("@Venta", lineaPedido.venta);
                            insertCommand.Parameters.AddWithValue("@Unidades", lineaPedido.unidades);
                            insertCommand.Parameters.AddWithValue("@Total", lineaPedido.total);

                            insertCommand.ExecuteNonQuery();
                        }

                        // Crear objeto línea de pedido para Firebase y guardar en Firebase
                        await client.Child("LineasPedidos").PostAsync(lineaPedido);
                    }

                    Debug.WriteLine("Líneas de pedido insertadas correctamente en la base de datos SQLite y en Firebase.");
                }
            }
            catch (SqliteException e)
            {
                Debug.WriteLine($"Error al insertar líneas de pedido en la base de datos SQLite: {e.Message}");
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error al insertar líneas de pedido: {e.Message}");
            }
        }






        /// <summary>
        /// 
        /// </summary>
        /// <param name="databaseName"></param>
        /// <param name="cliente"></param>
        public async Task AñadirCliente(string databaseName, Cliente cliente)
        {
            try
            {
                int newIdCliente;
                using (SqliteConnection connection = ConnectToDatabase(databaseName))
                {
                    connection.Open();
                    string createTableQuery = @"
            CREATE TABLE IF NOT EXISTS Clientes(
                IdCliente INTEGER PRIMARY KEY AUTOINCREMENT,
                Nombre TEXT,
                Email TEXT,
                Dirección TEXT,
                Localidad TEXT,
                Provincia TEXT,
                CodigoPostal TEXT,
                Pais TEXT,
                Ruta INTEGER,
                Ubicación TEXT,
                Enviado INTEGER
            )";

                    using (var command = new SqliteCommand(createTableQuery, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    string insertQuery = @"
            INSERT INTO Clientes (
                Nombre,
                Email,
                Dirección,
                Localidad,
                Provincia,
                CodigoPostal,
                Pais,
                Ruta,
                Ubicación,
                Enviado
            ) 
            VALUES (
                @Nombre, @Email, @Dirección, @Localidad, @Provincia, 
                @CodigoPostal, @Pais, @Ruta, @Ubicación, @Enviado
            );
            SELECT last_insert_rowid();";

                    using (var insertCommand = new SqliteCommand(insertQuery, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@Nombre", cliente.nombre);
                        insertCommand.Parameters.AddWithValue("@Email", cliente.email);
                        insertCommand.Parameters.AddWithValue("@Dirección", cliente.dirección);
                        insertCommand.Parameters.AddWithValue("@Localidad", cliente.localidad);
                        insertCommand.Parameters.AddWithValue("@Provincia", cliente.provincia);
                        insertCommand.Parameters.AddWithValue("@CodigoPostal", cliente.codigoPostal);
                        insertCommand.Parameters.AddWithValue("@Pais", cliente.pais);
                        insertCommand.Parameters.AddWithValue("@Ruta", cliente.ruta);
                        insertCommand.Parameters.AddWithValue("@Ubicación", cliente.ubicación);
                        insertCommand.Parameters.AddWithValue("@Enviado", cliente.enviado ? 1 : 0);

                        // Obtener el nuevo IdCliente generado
                        newIdCliente = Convert.ToInt32(insertCommand.ExecuteScalar());
                    }

                    Debug.WriteLine("Cliente insertado correctamente en la base de datos.");
                }

                // Añadir cliente a Firebase
                cliente.idCliente = newIdCliente; // Asegurarse de que el IdCliente es el mismo que en SQLite

                await client.Child("Clientes").Child(cliente.idCliente.ToString()).PutAsync(new Cliente
                {
                    idCliente = cliente.idCliente,
                    nombre = cliente.nombre,
                    email = cliente.email,
                    dirección = cliente.dirección,
                    localidad = cliente.localidad,
                    provincia = cliente.provincia,
                    codigoPostal = cliente.codigoPostal,
                    pais = cliente.pais,
                    ruta = cliente.ruta,
                    ubicación = cliente.ubicación,
                    enviado = cliente.enviado
                });

                Debug.WriteLine("Cliente añadido correctamente a Firebase.");
            }
            catch (SqliteException e)
            {
                Debug.WriteLine($"Error al insertar clientes en la base de datos: {e.Message}");
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error al añadir clientes en Firebase: {e.Message}");
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="databaseName"></param>
        /// <param name="cliente"></param>
        public async Task UpdateClientes(string databaseName, Cliente cliente)
        {
            try
            {
                using (SqliteConnection connection = ConnectToDatabase(databaseName))
                {
                    connection.Open();
                    string updateQuery = @"UPDATE Clientes 
                                   SET Nombre = @Nombre, 
                                       Email = @Email, 
                                       Dirección = @Dirección, 
                                       Localidad = @Localidad, 
                                       Provincia = @Provincia, 
                                       Ruta = @Ruta 
                                   WHERE IdCliente = @IdCliente";

                    using (SqliteCommand command = new SqliteCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Nombre", cliente.nombre);
                        command.Parameters.AddWithValue("@Email", cliente.email);
                        command.Parameters.AddWithValue("@Dirección", cliente.dirección);
                        command.Parameters.AddWithValue("@Localidad", cliente.localidad);
                        command.Parameters.AddWithValue("@Provincia", cliente.provincia);
                        command.Parameters.AddWithValue("@Ruta", cliente.ruta);
                        command.Parameters.AddWithValue("@IdCliente", cliente.idCliente);

                        int rowsUpdated = command.ExecuteNonQuery();

                        if (rowsUpdated > 0)
                        {
                            Debug.WriteLine("Cliente actualizado correctamente en SQLite.");
                        }
                        else
                        {
                            Debug.WriteLine("No se encontró el cliente para actualizar en SQLite.");
                        }
                    }
                }

                // Actualizar el cliente en Firebase
                await client.Child("Clientes").Child(cliente.idCliente.ToString()).PutAsync(new Cliente
                {
                    idCliente = cliente.idCliente,
                    nombre = cliente.nombre,
                    email = cliente.email,
                    dirección = cliente.dirección,
                    localidad = cliente.localidad,
                    provincia = cliente.provincia,
                    codigoPostal = cliente.codigoPostal,
                    pais = cliente.pais,
                    ruta = cliente.ruta,
                    ubicación = cliente.ubicación,
                    enviado = cliente.enviado
                });

                Debug.WriteLine("Cliente actualizado correctamente en Firebase.");
            }
            catch (SqliteException e)
            {
                Debug.WriteLine($"Error al actualizar clientes en la base de datos SQLite: {e.Message}");
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error al actualizar clientes en Firebase: {e.Message}");
            }
        }

        public async Task UpdateUsuario(string databaseName, CrisoftApp.Models.Rols.Usuario usuario)
        {
            try
            {
                using (SqliteConnection connection = ConnectToDatabase(databaseName))
                {
                    connection.Open();
                    string updateQuery = @"UPDATE Usuarios 
                           SET Nombre = @Nombre, 
                               Gmail = @Gmail, 
                               Contra = @Contra, 
                               Rol = @Rol 
                           WHERE IdUsuario = @IdUsuario";

                    using (SqliteCommand command = new SqliteCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Nombre", usuario.Nombre);
                        command.Parameters.AddWithValue("@Gmail", usuario.Gmail);
                        command.Parameters.AddWithValue("@Contra", usuario.Contra);
                        command.Parameters.AddWithValue("@Rol", usuario.Rol.ToString());
                        command.Parameters.AddWithValue("@IdUsuario", usuario.IdUsuario);

                        int rowsUpdated = command.ExecuteNonQuery();

                        if (rowsUpdated > 0)
                        {
                            Debug.WriteLine("Usuario actualizado correctamente en SQLite.");
                        }
                        else
                        {
                            Debug.WriteLine("No se encontró el usuario para actualizar en SQLite.");
                        }
                    }
                }

                // Convertir el rol a un valor numérico
                int rolValue = usuario.Rol == CrisoftApp.Models.Rols.Rol.Admin ? 0 : 1;

                // Actualizar el usuario en Firebase
                await client.Child("Usuarios").Child(usuario.IdUsuario.ToString()).PutAsync(new
                {
                    IdUsuario = usuario.IdUsuario,
                    Nombre = usuario.Nombre,
                    Gmail = usuario.Gmail,
                    Contra = usuario.Contra,
                    Rol = rolValue,
                    FechaRegistro = usuario.FechaRegistro.ToString("yyyy-MM-dd HH:mm:ss")
                });

                Debug.WriteLine("Usuario actualizado correctamente en Firebase.");
            }
            catch (SqliteException e)
            {
                Debug.WriteLine($"Error al actualizar usuarios en la base de datos SQLite: {e.Message}");
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error al actualizar usuarios en Firebase: {e.Message}");
            }
        }


        public async Task EliminarCliente(string databaseName, int idCliente)
        {
            try
            {
                // Eliminar cliente de SQLite
                using (SqliteConnection connection = ConnectToDatabase(databaseName))
                {
                    connection.Open();
                    string deleteQuery = @"DELETE FROM Clientes WHERE IdCliente = @IdCliente";

                    using (SqliteCommand command = new SqliteCommand(deleteQuery, connection))
                    {
                        command.Parameters.AddWithValue("@IdCliente", idCliente);

                        int rowsDeleted = command.ExecuteNonQuery();

                        if (rowsDeleted > 0)
                        {
                            Debug.WriteLine("Cliente eliminado correctamente de SQLite.");
                        }
                        else
                        {
                            Debug.WriteLine("No se encontró el cliente para eliminar en SQLite.");
                        }
                    }
                }

                // Eliminar cliente de Firebase
                await client.Child("Clientes").Child(idCliente.ToString()).DeleteAsync();

                Debug.WriteLine("Cliente eliminado correctamente de Firebase.");
            }
            catch (SqliteException e)
            {
                Debug.WriteLine($"Error al eliminar cliente de la base de datos SQLite: {e.Message}");
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error al eliminar cliente de Firebase: {e.Message}");
            }
        }


        public async Task DownloadAndInsertMarcasAsync(string databaseName, string jsonUrl)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(jsonUrl);
                    response.EnsureSuccessStatusCode();

                    string json = await response.Content.ReadAsStringAsync();
                    Listas rootObject = JsonConvert.DeserializeObject<Listas>(json);
                    AñadirMarcas(databaseName, rootObject.marca);
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Error al descargar el JSON: {ex.Message}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="databaseName"></param>
        /// <param name="marcas"></param>
        public async Task AñadirMarcas(string databaseName, Marca[] marcas)
        {
            if (marcas == null)
            {
                Debug.WriteLine("La lista de marcas es nula.");
                return;
            }

            try
            {
                using (SqliteConnection connection = ConnectToDatabase(databaseName))
                {
                    await connection.OpenAsync();

                    string createTableQuery = @"
                        CREATE TABLE IF NOT EXISTS Marcas(
                            IdMarca TEXT PRIMARY KEY,
                            Descripcion TEXT,
                            UrlImagen TEXT
                        )";

                    using (var command = new SqliteCommand(createTableQuery, connection))
                    {
                        await command.ExecuteNonQueryAsync();
                    }

                    foreach (Marca marca in marcas)
                    {
                        string insertQuery = @"
                            INSERT INTO Marcas (
                                IdMarca,
                                Descripcion,
                                UrlImagen
                            ) 
                            SELECT @IdMarca, @Descripcion, @UrlImagen
                            WHERE NOT EXISTS (
                                SELECT 1 FROM Marcas WHERE IdMarca = @IdMarca
                            )";

                        using (var insertCommand = new SqliteCommand(insertQuery, connection))
                        {
                            insertCommand.Parameters.AddWithValue("@IdMarca", marca.idMarca);
                            insertCommand.Parameters.AddWithValue("@Descripcion", marca.descripcion);
                            insertCommand.Parameters.AddWithValue("@UrlImagen", marca.urlImagen);

                            await insertCommand.ExecuteNonQueryAsync();
                        }

                        var existingMarcas = await client.Child("Marcas")
                                                         .OnceAsync<Marca>();
                        if (!existingMarcas.Any(m => m.Object.idMarca == marca.idMarca))
                        {
                            await client.Child("Marcas").PostAsync(new Marca
                            {
                                idMarca = marca.idMarca,
                                descripcion = marca.descripcion,
                                urlImagen = marca.urlImagen
                            });
                        }
                    }

                    Debug.WriteLine("Marcas insertadas correctamente en la base de datos y en Firebase.");
                }
            }
            catch (SqliteException e)
            {
                Debug.WriteLine($"Error al insertar marcas en la base de datos: {e.Message}");
            }
        }


        public async Task DownloadAndInsertArticulosAsync(string databaseName, string jsonUrl)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(jsonUrl);
                    response.EnsureSuccessStatusCode();

                    string json = await response.Content.ReadAsStringAsync();
                    Listas lis = JsonConvert.DeserializeObject<Listas>(json);
                    AñadirArticulos(databaseName, lis.articulos);
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Error al descargar el JSON de artículos: {ex.Message}");
            }
        }

        public async Task AñadirArticulos(string databaseName, Articulo[] articulos)
        {
            if (articulos == null)
            {
                Debug.WriteLine("La lista de artículos es nula.");
                return;
            }

            try
            {
                using (SqliteConnection connection = ConnectToDatabase(databaseName))
                {
                    await connection.OpenAsync();

                    string createTableQuery = @"
            CREATE TABLE IF NOT EXISTS Articulos(
                IdArticulo INTEGER PRIMARY KEY AUTOINCREMENT,
                Referencia TEXT,
                Descripcion TEXT,
                IdCategoria TEXT,
                IdMarca TEXT,
                CodidoBarras TEXT,
                Coste DECIMAL(18, 2),
                Venta DECIMAL(18, 2),
                VentaOferta DECIMAL(18, 2),
                FechaAlta TEXT,
                Existencias INTEGER,
                UrlImagen1 TEXT,
                UrlImagen2 TEXT,
                UrlImagen3 TEXT,
                UrlImagen4 TEXT,
                UrlImagen5 TEXT,
                UrlImagen6 TEXT,
                UrlImagen7 TEXT,
                FOREIGN KEY (IdMarca) REFERENCES Marcas(IdMarca),
                FOREIGN KEY (IdCategoria) REFERENCES Categorias(IdCategoria)
            )";

                    using (var command = new SqliteCommand(createTableQuery, connection))
                    {
                        await command.ExecuteNonQueryAsync();
                    }

                    foreach (Articulo articulo in articulos)
                    {
                        string insertQuery = @"
                INSERT INTO Articulos (
                    Referencia,
                    Descripcion,
                    IdCategoria,
                    IdMarca,
                    CodidoBarras,
                    Coste,
                    Venta,
                    VentaOferta,
                    FechaAlta,
                    Existencias,
                    UrlImagen1,
                    UrlImagen2,
                    UrlImagen3,
                    UrlImagen4,
                    UrlImagen5,
                    UrlImagen6,
                    UrlImagen7
                ) SELECT 
                    @Referencia,
                    @Descripcion,
                    @IdCategoria,
                    @IdMarca,
                    @CodidoBarras,
                    @Coste,
                    @Venta,
                    @VentaOferta,
                    @FechaAlta,
                    @Existencias,
                    @UrlImagen1,
                    @UrlImagen2,
                    @UrlImagen3,
                    @UrlImagen4,
                    @UrlImagen5,
                    @UrlImagen6,
                    @UrlImagen7
                WHERE NOT EXISTS (
                    SELECT 1 FROM Articulos WHERE Referencia = @Referencia
                )";

                        using (var insertCommand = new SqliteCommand(insertQuery, connection))
                        {
                            insertCommand.Parameters.AddWithValue("@Referencia", articulo.referencia);
                            insertCommand.Parameters.AddWithValue("@Descripcion", articulo.descripcion);
                            insertCommand.Parameters.AddWithValue("@IdCategoria", articulo.idCategoria);
                            insertCommand.Parameters.AddWithValue("@IdMarca", articulo.idMarca);
                            insertCommand.Parameters.AddWithValue("@CodidoBarras", articulo.codidoBarras);
                            insertCommand.Parameters.AddWithValue("@Coste", articulo.coste);
                            insertCommand.Parameters.AddWithValue("@Venta", articulo.venta);
                            insertCommand.Parameters.AddWithValue("@VentaOferta", articulo.ventaOferta);
                            insertCommand.Parameters.AddWithValue("@FechaAlta", articulo.fechaAlta);
                            insertCommand.Parameters.AddWithValue("@Existencias", articulo.existencias);
                            insertCommand.Parameters.AddWithValue("@UrlImagen1", articulo.urlImagen1);
                            insertCommand.Parameters.AddWithValue("@UrlImagen2", articulo.urlImagen2 ?? "");
                            insertCommand.Parameters.AddWithValue("@UrlImagen3", articulo.urlImagen3 ?? "");
                            insertCommand.Parameters.AddWithValue("@UrlImagen4", articulo.urlImagen4 ?? "");
                            insertCommand.Parameters.AddWithValue("@UrlImagen5", articulo.urlImagen5 ?? "");
                            insertCommand.Parameters.AddWithValue("@UrlImagen6", articulo.urlImagen6 ?? "");
                            insertCommand.Parameters.AddWithValue("@UrlImagen7", articulo.urlImagen7 ?? "");

                            await insertCommand.ExecuteNonQueryAsync();
                        }

                        var existingArticulos = await client.Child("Articulos")
                                                            .OnceAsync<Articulo>();
                        if (!existingArticulos.Any(a => a.Object.referencia == articulo.referencia))
                        {
                            await client.Child("Articulos").PostAsync(articulo);
                        }
                    }

                    Debug.WriteLine("Artículos insertados correctamente en la base de datos y en Firebase.");
                }
            }
            catch (SqliteException e)
            {
                Debug.WriteLine($"Error al insertar artículos en la base de datos: {e.Message}");
            }
        }


        public async Task DownloadAndInsertCategoriasAsync(string databaseName, string jsonUrl)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(jsonUrl);
                    response.EnsureSuccessStatusCode();

                    string json = await response.Content.ReadAsStringAsync();
                    Listas lis = JsonConvert.DeserializeObject<Listas>(json);
                    AñadirCategorias(databaseName, lis.categorias);
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Error al descargar el JSON de categorías: {ex.Message}");
            }
        }

        public async void AñadirCategorias(string databaseName, Categoria[] categorias)
        {
            if (categorias == null)
            {
                Debug.WriteLine("La lista de categorías es nula.");
                return;
            }

            try
            {
                using (SqliteConnection connection = ConnectToDatabase(databaseName))
                {
                    connection.Open();
                    string createTableQuery = @"
                        CREATE TABLE IF NOT EXISTS Categorias (
                            IdCategoria TEXT PRIMARY KEY,
                            Descripcion TEXT,
                            UrlImagen TEXT
                        )";

                    using (var command = new SqliteCommand(createTableQuery, connection))
                    {
                        command.ExecuteNonQuery();
                        Debug.WriteLine("Tabla 'Categorias' creada o ya existente.");
                    }

                    foreach (Categoria categoria in categorias)
                    {
                        string insertQuery = @"
                            INSERT INTO Categorias (
                                IdCategoria,
                                Descripcion,
                                UrlImagen
                            )
                            SELECT @IdCategoria, @Descripcion, @UrlImagen
                            WHERE NOT EXISTS (
                                SELECT 1 FROM Categorias WHERE IdCategoria = @IdCategoria
                            )";

                        using (var insertCommand = new SqliteCommand(insertQuery, connection))
                        {
                            insertCommand.Parameters.AddWithValue("@IdCategoria", categoria.idCategoria);
                            insertCommand.Parameters.AddWithValue("@Descripcion", categoria.descripcion);
                            insertCommand.Parameters.AddWithValue("@UrlImagen", categoria.urlImagen);

                            insertCommand.ExecuteNonQuery();
                        }

                        await client.Child("Categorias").PostAsync(categoria);
                    }

                    Debug.WriteLine("Categorías insertadas correctamente en la base de datos y en Firebase.");
                }
            }
            catch (SqliteException e)
            {
                Debug.WriteLine($"Error al insertar categorías en la base de datos: {e.Message}");
            }
        }






        public List<PALP> ObtenerDatosPedido(string databaseName, int idPedido)
        {
            List<PALP> datosPedido = new List<PALP>();

            try
            {
                using (SqliteConnection connection = ConnectToDatabase(databaseName))
                {
                    connection.Open();

                    // Consulta para obtener los datos de las tablas una a una
                    string selectQuery = @"
                    SELECT 
                        lp.idLineaPedido, 
                        lp.idPedido, 
                        lp.idArticulo, 
                        lp.referencia, 
                        lp.coste, 
                        lp.venta, 
                        lp.unidades, 
                        lp.total,
                        p.fecha, 
                        p.idCliente, 
                        p.total, 
                        p.enviado,
                        a.descripcion AS descripcionArticulo, 
                        a.idCategoria, 
                        a.idMarca, 
                        a.codidoBarras, 
                        a.ventaOferta, 
                        a.fechaAlta, 
                        a.existencias,
                        a.urlImagen1, 
                        a.urlImagen2, 
                        a.urlImagen3, 
                        a.urlImagen4, 
                        a.urlImagen5, 
                        a.urlImagen6, 
                        a.urlImagen7,
                        c.descripcion AS descripcionCategoria,
                        m.descripcion AS descripcionMarca
                    FROM Pedidos p
                    LEFT JOIN LineasPedidos lp ON p.idPedido = lp.idPedido
                    LEFT JOIN Articulos a ON lp.idArticulo = a.idArticulo
                    LEFT JOIN Categorias c ON a.idCategoria = c.idCategoria
                    LEFT JOIN Marcas m ON a.idMarca = m.idMarca
                    WHERE p.idPedido = @IdPedido;";

                    using (SqliteCommand command = new SqliteCommand(selectQuery, connection))
                    {
                        command.Parameters.AddWithValue("@IdPedido", idPedido);

                        using (SqliteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                LineasPedido lineasPedido = new LineasPedido
                                {
                                    idLineaPedido = reader.IsDBNull(reader.GetOrdinal("idLineaPedido")) ? 0 : Convert.ToInt32(reader["idLineaPedido"]),
                                    idPedido = reader.IsDBNull(reader.GetOrdinal("idPedido")) ? 0 : Convert.ToInt32(reader["idPedido"]),
                                    idArticulo = reader.IsDBNull(reader.GetOrdinal("idArticulo")) ? 0 : Convert.ToInt32(reader["idArticulo"]),
                                    referencia = reader.IsDBNull(reader.GetOrdinal("referencia")) ? null : reader.GetString(reader.GetOrdinal("referencia")),
                                    coste = reader.IsDBNull(reader.GetOrdinal("coste")) ? 0 : Convert.ToInt32(reader["coste"]),
                                    venta = reader.IsDBNull(reader.GetOrdinal("venta")) ? 0 : Convert.ToInt32(reader["venta"]),
                                    unidades = reader.IsDBNull(reader.GetOrdinal("unidades")) ? 0 : Convert.ToInt32(reader["unidades"]),
                                    total = reader.IsDBNull(reader.GetOrdinal("total")) ? 0 : Convert.ToSingle(reader["total"])
                                };

                                Pedido pedido = new Pedido
                                {
                                    fecha = string.IsNullOrEmpty(reader["fecha"].ToString()) ? null : DateTime.Parse(reader["fecha"].ToString()),
                                    idCliente = reader.IsDBNull(reader.GetOrdinal("idCliente")) ? 0 : Convert.ToInt32(reader["idCliente"]),
                                    total = reader.IsDBNull(reader.GetOrdinal("total")) ? 0 : Convert.ToSingle(reader["total"]),
                                    enviado = reader.IsDBNull(reader.GetOrdinal("enviado")) ? 0 : Convert.ToInt32(reader["enviado"])
                                };

                                Articulo articulo = new Articulo
                                {
                                    descripcion = reader.IsDBNull(reader.GetOrdinal("descripcionArticulo")) ? null : reader.GetString(reader.GetOrdinal("descripcionArticulo")),
                                    codidoBarras = reader.IsDBNull(reader.GetOrdinal("codidoBarras")) ? null : reader.GetString(reader.GetOrdinal("codidoBarras")),
                                    ventaOferta = reader.IsDBNull(reader.GetOrdinal("ventaOferta")) ? 0 : Convert.ToInt32(reader["ventaOferta"]),
                                    fechaAlta = string.IsNullOrEmpty(reader["fechaAlta"].ToString()) ? null : DateTime.Parse(reader["fechaAlta"].ToString()),
                                    existencias = reader.IsDBNull(reader.GetOrdinal("existencias")) ? 0 : Convert.ToInt32(reader["existencias"]),
                                    urlImagen1 = reader.IsDBNull(reader.GetOrdinal("urlImagen1")) ? null : reader.GetString(reader.GetOrdinal("urlImagen1")),
                                    urlImagen2 = reader.IsDBNull(reader.GetOrdinal("urlImagen2")) ? null : reader.GetString(reader.GetOrdinal("urlImagen2")),
                                    urlImagen3 = reader.IsDBNull(reader.GetOrdinal("urlImagen3")) ? null : reader.GetString(reader.GetOrdinal("urlImagen3")),
                                    urlImagen4 = reader.IsDBNull(reader.GetOrdinal("urlImagen4")) ? null : reader.GetString(reader.GetOrdinal("urlImagen4")),
                                    urlImagen5 = reader.IsDBNull(reader.GetOrdinal("urlImagen5")) ? null : reader.GetString(reader.GetOrdinal("urlImagen5")),
                                    urlImagen6 = reader.IsDBNull(reader.GetOrdinal("urlImagen6")) ? null : reader.GetString(reader.GetOrdinal("urlImagen6")),
                                    urlImagen7 = reader.IsDBNull(reader.GetOrdinal("urlImagen7")) ? null : reader.GetString(reader.GetOrdinal("urlImagen7")),
                                    idCategoria = reader.IsDBNull(reader.GetOrdinal("idCategoria")) ? null : reader.GetString(reader.GetOrdinal("idCategoria")),
                                    idMarca = reader.IsDBNull(reader.GetOrdinal("idMarca")) ? null : reader.GetString(reader.GetOrdinal("idMarca")),
                                };

                                PALP datos = new PALP
                                {
                                    pedido = pedido,
                                    articulo = articulo,
                                    lineasPedido = lineasPedido
                                };

                                datosPedido.Add(datos);

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al obtener datos del pedido desde la base de datos: {ex.Message}");
            }

            return datosPedido;
        }

        public List<Cliente> ObtenerClientes(string databaseName)
        {
            List<Cliente> clientes = new List<Cliente>();

            try
            {
                using (SqliteConnection connection = ConnectToDatabase(databaseName))
                {
                    connection.Open();

                    string selectQuery = @"
                    SELECT IdCliente, Nombre, Email, Dirección, Localidad, Provincia, CodigoPostal, Pais, Ruta, Ubicación, Enviado
                    FROM Clientes
                    ORDER BY IdCliente";

                    using (SqliteCommand command = new SqliteCommand(selectQuery, connection))
                    {
                        using (SqliteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Cliente cliente = new Cliente
                                {
                                    idCliente = Convert.ToInt32(reader["IdCliente"]),
                                    nombre = reader["Nombre"].ToString(),
                                    email = reader["Email"].ToString(),
                                    dirección = reader["Dirección"].ToString(),
                                    localidad = reader["Localidad"].ToString(),
                                    provincia = reader["Provincia"].ToString(),
                                    codigoPostal = reader["CodigoPostal"].ToString(),
                                    pais = reader["Pais"].ToString(),
                                    ruta = reader["Ruta"].ToString(), //Convert.ToInt32(reader["Ruta"]),
                                    ubicación = reader["Ubicación"].ToString(),
                                    enviado = Convert.ToBoolean(reader["Enviado"])
                                };

                                clientes.Add(cliente);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al obtener clientes desde la base de datos: {ex.Message}");
            }

            return clientes;
        }

        public List<Agenda> ObtenerAgendas(string databaseName)
        {
            List<Agenda> agendas = new List<Agenda>();

            try
            {
                using (SqliteConnection connection = ConnectToDatabase(databaseName))
                {
                    connection.Open();

                    string selectQuery = @"
                        SELECT idEvento, idUsuario, fecha, hora, notas
                        FROM Agenda
                        ORDER BY idEvento";

                    using (SqliteCommand command = new SqliteCommand(selectQuery, connection))
                    {
                        using (SqliteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Agenda agenda = new Agenda
                                {
                                    idEvento = Convert.ToInt32(reader["idEvento"]),
                                    idUsuario = Convert.ToInt32(reader["idUsuario"]),
                                    fecha = reader["fecha"] != DBNull.Value ? Convert.ToDateTime(reader["fecha"]) : DateTime.MinValue,
                                    hora = reader["hora"] != DBNull.Value ? TimeSpan.Parse(reader["hora"].ToString()) : TimeSpan.MinValue,
                                    notas = reader["notas"] != DBNull.Value ? reader["notas"].ToString() : null
                                };

                                agendas.Add(agenda);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al obtener agendas desde la base de datos: {ex.Message}");
            }

            return agendas;
        }




        public List<Categoria> ObtenerCategorias(string databaseName)
        {
            List<Categoria> categorias = new List<Categoria>();

            try
            {
                using (SqliteConnection connection = ConnectToDatabase(databaseName))
                {
                    connection.Open();

                    string selectQuery = @"
                        SELECT idCategoria, descripcion, urlImagen
                        FROM Categorias
                        ORDER BY idCategoria";

                    using (SqliteCommand command = new SqliteCommand(selectQuery, connection))
                    {
                        using (SqliteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Categoria categoria = new Categoria
                                {
                                    idCategoria = reader["idCategoria"].ToString(),
                                    descripcion = reader["descripcion"].ToString(),
                                    urlImagen = reader["urlImagen"].ToString()
                                };

                                categorias.Add(categoria);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al obtener categorías desde la base de datos: {ex.Message}");
            }

            return categorias;
        }

        public List<Marca> ObtenerMarcas(string databaseName)
        {
            List<Marca> marcas = new List<Marca>();

            try
            {
                using (SqliteConnection connection = ConnectToDatabase(databaseName))
                {
                    connection.Open();

                    string selectQuery = @"
                        SELECT idMarca, descripcion, urlImagen
                        FROM Marcas
                        ORDER BY idMarca";

                    using (SqliteCommand command = new SqliteCommand(selectQuery, connection))
                    {
                        using (SqliteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Marca marca = new Marca
                                {
                                    idMarca = reader["idMarca"].ToString(),
                                    descripcion = reader["descripcion"].ToString(),
                                    urlImagen = reader["urlImagen"].ToString()
                                };

                                marcas.Add(marca);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al obtener marcas desde la base de datos: {ex.Message}");
            }

            return marcas;
        }

        public List<Categoria> ObtenerCategoriasPorDescripcion(string descripcion, string databaseName)
        {
            List<Categoria> categorias = new List<Categoria>();

            try
            {
                using (SqliteConnection connection = ConnectToDatabase(databaseName))
                {
                    connection.Open();
                    Debug.WriteLine("Conexión a la base de datos abierta.");

                    string selectQuery = @"
                        SELECT IdCategoria, Descripcion, UrlImagen
                        FROM Categorias
                        WHERE Descripcion = @Descripcion";

                    using (SqliteCommand command = new SqliteCommand(selectQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Descripcion", descripcion);

                        using (SqliteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Categoria categoria = new Categoria
                                {
                                    idCategoria = reader["IdCategoria"].ToString(),
                                    descripcion = reader["Descripcion"].ToString(),
                                    urlImagen = reader["UrlImagen"].ToString()
                                };

                                categorias.Add(categoria);
                            }
                        }
                    }

                    Debug.WriteLine("Categorías obtenidas correctamente desde la base de datos.");
                }
            }
            catch (SqliteException ex)
            {
                Debug.WriteLine($"Error al obtener categorías por descripción desde la base de datos: {ex.Message}");
            }

            return categorias;
        }


        public List<Articulo> ObtenerArticulosPorCategoria(string databaseName, string idCategoria) //Corregir
        {
            List<Articulo> articulos = new List<Articulo>();

            try
            {
                using (SqliteConnection connection = ConnectToDatabase(databaseName))
                {
                    connection.Open();

                    string selectQuery = @"
                        SELECT idArticulo, referencia, descripcion, idCategoria, idMarca, codidoBarras, coste, venta, ventaOferta, fechaAlta, existencias, urlImagen1, urlImagen2, urlImagen3, urlImagen4, urlImagen5, urlImagen6, urlImagen7
                        FROM Articulos
                        WHERE idCategoria = @idCategoria
                        ORDER BY idArticulo";

                    using (SqliteCommand command = new SqliteCommand(selectQuery, connection))
                    {
                        command.Parameters.AddWithValue("@idCategoria", idCategoria);

                        using (SqliteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Articulo articulo = new Articulo
                                {
                                    idArticulo = Convert.ToInt32(reader["idArticulo"]),
                                    referencia = reader["referencia"].ToString(),
                                    descripcion = reader["descripcion"].ToString(),
                                    idCategoria = reader["idCategoria"].ToString(),
                                    idMarca = reader["idMarca"].ToString(),
                                    codidoBarras = reader["codidoBarras"].ToString(),
                                    coste = Convert.ToInt32(reader["coste"]),
                                    venta = reader["venta"].ToString(),
                                    ventaOferta = Convert.ToInt32(reader["ventaOferta"]),
                                    fechaAlta = reader["fechaAlta"] as DateTime?,
                                    existencias = Convert.ToInt32(reader["existencias"]),
                                    urlImagen1 = reader["urlImagen1"].ToString(),
                                    urlImagen2 = reader["urlImagen2"].ToString(),
                                    urlImagen3 = reader["urlImagen3"].ToString(),
                                    urlImagen4 = reader["urlImagen4"].ToString(),
                                    urlImagen5 = reader["urlImagen5"].ToString(),
                                    urlImagen6 = reader["urlImagen6"].ToString(),
                                    urlImagen7 = reader["urlImagen7"].ToString()
                                };

                                articulos.Add(articulo);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al obtener artículos por categoría desde la base de datos: {ex.Message}");
            }

            return articulos;
        }

        public List<Marca> ObtenerMarcasPorDescripcion(string databaseName, string descripcion)
        {
            List<Marca> marcas = new List<Marca>();

            try
            {
                using (SqliteConnection connection = ConnectToDatabase(databaseName))
                {
                    connection.Open();

                    string selectQuery = @"
                        SELECT idMarca, descripcion, urlImagen
                        FROM Marcas
                        WHERE descripcion = @Descripcion";

                    using (SqliteCommand command = new SqliteCommand(selectQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Descripcion", descripcion);

                        using (SqliteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Marca marca = new Marca
                                {
                                    idMarca = reader["idMarca"].ToString(),
                                    descripcion = reader["descripcion"].ToString(),
                                    urlImagen = reader["urlImagen"].ToString()
                                };

                                marcas.Add(marca);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al obtener marcas desde la base de datos: {ex.Message}");
            }

            return marcas;
        }




        public List<Articulo> ObtenerArticulosPorIdMarca(string databaseName, string idMarca)
        {
            List<Articulo> articulos = new List<Articulo>();

            try
            {
                using (SqliteConnection connection = ConnectToDatabase(databaseName))
                {
                    connection.Open();

                    string selectQuery = @"
                        SELECT idArticulo, referencia, descripcion, idCategoria, idMarca, codidoBarras, coste, venta, ventaOferta, fechaAlta, existencias, urlImagen1, urlImagen2, urlImagen3, urlImagen4, urlImagen5, urlImagen6, urlImagen7
                        FROM Articulos
                        WHERE idMarca = @IdMarca";

                    using (SqliteCommand command = new SqliteCommand(selectQuery, connection))
                    {
                        command.Parameters.AddWithValue("@IdMarca", idMarca);

                        using (SqliteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Articulo articulo = new Articulo
                                {
                                    idArticulo = Convert.ToInt32(reader["idArticulo"]),
                                    referencia = reader["referencia"].ToString(),
                                    descripcion = reader["descripcion"].ToString(),
                                    idCategoria = reader["idCategoria"].ToString(),
                                    idMarca = reader["idMarca"].ToString(),
                                    codidoBarras = reader["codidoBarras"].ToString(),
                                    coste = Convert.ToInt32(reader["coste"]),
                                    venta = reader["venta"].ToString(),
                                    ventaOferta = Convert.ToInt32(reader["ventaOferta"]),
                                    fechaAlta = reader["fechaAlta"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["fechaAlta"]) : null,
                                    existencias = Convert.ToInt32(reader["existencias"]),
                                    urlImagen1 = reader["urlImagen1"].ToString(),
                                    urlImagen2 = reader["urlImagen2"].ToString(),
                                    urlImagen3 = reader["urlImagen3"].ToString(),
                                    urlImagen4 = reader["urlImagen4"].ToString(),
                                    urlImagen5 = reader["urlImagen5"].ToString(),
                                    urlImagen6 = reader["urlImagen6"].ToString(),
                                    urlImagen7 = reader["urlImagen7"].ToString()
                                };

                                articulos.Add(articulo);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al obtener artículos desde la base de datos: {ex.Message}");
            }

            return articulos;
        }




        public List<Articulo> ObtenerArticulosPorVentaOfertaDescendente(string databaseName)
        {
            List<Articulo> articulos = new List<Articulo>();

            try
            {
                using (SqliteConnection connection = ConnectToDatabase(databaseName))
                {
                    connection.Open();

                    string selectQuery = @"
                        SELECT idArticulo, referencia, descripcion, idCategoria, idMarca, codidoBarras, coste, venta, ventaOferta, fechaAlta, existencias, urlImagen1, urlImagen2, urlImagen3, urlImagen4, urlImagen5, urlImagen6, urlImagen7
                        FROM Articulos
                        ORDER BY ventaOferta DESC";

                    using (SqliteCommand command = new SqliteCommand(selectQuery, connection))
                    {
                        using (SqliteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Articulo articulo = new Articulo
                                {
                                    idArticulo = Convert.ToInt32(reader["idArticulo"]),
                                    referencia = reader["referencia"].ToString(),
                                    descripcion = reader["descripcion"].ToString(), 
                                    idCategoria = reader["idCategoria"].ToString(),
                                    idMarca = reader["idMarca"].ToString(),
                                    codidoBarras = reader["codidoBarras"].ToString(),
                                    coste = Convert.ToInt32(reader["coste"]),
                                    venta = reader["venta"].ToString(),
                                    ventaOferta = Convert.ToInt32(reader["ventaOferta"]),
                                    fechaAlta = reader["fechaAlta"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["fechaAlta"]) : null,
                                    existencias = Convert.ToInt32(reader["existencias"]),
                                    urlImagen1 = reader["urlImagen1"].ToString(),
                                    urlImagen2 = reader["urlImagen2"].ToString(),
                                    urlImagen3 = reader["urlImagen3"].ToString(),
                                    urlImagen4 = reader["urlImagen4"].ToString(),
                                    urlImagen5 = reader["urlImagen5"].ToString(),
                                    urlImagen6 = reader["urlImagen6"].ToString(),
                                    urlImagen7 = reader["urlImagen7"].ToString()
                                };

                                articulos.Add(articulo);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al obtener artículos desde la base de datos: {ex.Message}");
            }

            return articulos;
        }




        public List<Articulo> ObtenerArticulosPorVentaOfertaAscendente(string databaseName)
        {
            List<Articulo> articulos = new List<Articulo>();

            try
            {
                using (SqliteConnection connection = ConnectToDatabase(databaseName))
                {
                    connection.Open();

                    string selectQuery = @"
                        SELECT idArticulo, referencia, descripcion, idCategoria, idMarca, codidoBarras, coste, venta, ventaOferta, fechaAlta, existencias, urlImagen1, urlImagen2, urlImagen3, urlImagen4, urlImagen5, urlImagen6, urlImagen7
                        FROM Articulos
                        ORDER BY ventaOferta ASC";

                    using (SqliteCommand command = new SqliteCommand(selectQuery, connection))
                    {
                        using (SqliteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Articulo articulo = new Articulo
                                {
                                    idArticulo = Convert.ToInt32(reader["idArticulo"]),
                                    referencia = reader["referencia"].ToString(),
                                    descripcion = reader["descripcion"].ToString(),
                                    idCategoria = reader["idCategoria"].ToString(),
                                    idMarca = reader["idMarca"].ToString(),
                                    codidoBarras = reader["codidoBarras"].ToString(),
                                    coste = Convert.ToInt32(reader["coste"]),
                                    venta = reader["venta"].ToString(),
                                    ventaOferta = Convert.ToInt32(reader["ventaOferta"]),
                                    fechaAlta = reader["fechaAlta"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["fechaAlta"]) : null,
                                    existencias = Convert.ToInt32(reader["existencias"]),
                                    urlImagen1 = reader["urlImagen1"].ToString(),
                                    urlImagen2 = reader["urlImagen2"].ToString(),
                                    urlImagen3 = reader["urlImagen3"].ToString(),
                                    urlImagen4 = reader["urlImagen4"].ToString(),
                                    urlImagen5 = reader["urlImagen5"].ToString(),
                                    urlImagen6 = reader["urlImagen6"].ToString(),
                                    urlImagen7 = reader["urlImagen7"].ToString()
                                };

                                articulos.Add(articulo);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al obtener artículos desde la base de datos: {ex.Message}");
            }

            return articulos;
        }


        public bool ExisteClientePorId(string databaseName, int id)
        {
            try
            {
                using (SqliteConnection connection = ConnectToDatabase(databaseName))
                {
                    connection.Open();

                    string selectQuery = "SELECT COUNT(*) FROM Clientes WHERE idCliente = @id";

                    using (SqliteCommand command = new SqliteCommand(selectQuery, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);

                        int count = Convert.ToInt32(command.ExecuteScalar());

                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al verificar si existe el cliente con ID {id} en la base de datos: {ex.Message}");
                return false;
            }
        }


        public int SacarIdPorNombre(string databaseName, string nombre)
        {
            try
            {
                using (SqliteConnection connection = ConnectToDatabase(databaseName))
                {
                    connection.Open();

                    string selectQuery = "SELECT idCliente FROM Clientes WHERE nombre = @nombre";

                    using (SqliteCommand command = new SqliteCommand(selectQuery, connection))
                    {
                        command.Parameters.AddWithValue("@nombre", nombre);

                        object result = command.ExecuteScalar();

                        if (result != null)
                        {
                            return Convert.ToInt32(result);
                        }
                        else
                        {
                            Debug.WriteLine($"No se encontró ningún cliente con nombre '{nombre}' en la base de datos.");
                            return 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al buscar el ID del cliente '{nombre}' en la base de datos: {ex.Message}");
                return 0;
            }
        }

        

        public void CrearTablaPedidos(string databaseName)
        {
            try
            {
                using (SqliteConnection connection = ConnectToDatabase(databaseName))
                {
                    connection.Open();
                    string createTableQuery = @"
                    CREATE TABLE IF NOT EXISTS Pedidos(
                        IdPedido INTEGER PRIMARY KEY AUTOINCREMENT,
                        Fecha TEXT,
                        IdCliente INTEGER,
                        Total INTEGER,
                        Enviado INTEGER
                    )";

                    using (var command = new SqliteCommand(createTableQuery, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    Debug.WriteLine("Tabla de Pedidos creada correctamente en la base de datos.");
                }
            }
            catch (SqliteException e)
            {
                Debug.WriteLine($"Error al crear tabla de Pedidos en la base de datos: {e.Message}");
            }
        }

        public async Task<int> InsertarPedidos(string databaseName, Pedido pedido)
        {
            if (pedido == null)
            {
                Debug.WriteLine("El pedido es nulo.");
                return -1;
            }

            try
            {
                using (SqliteConnection connection = ConnectToDatabase(databaseName))
                {
                    await connection.OpenAsync();

                    string insertQuery = @"
                        INSERT INTO Pedidos (
                            Fecha,
                            IdCliente,
                            Total,
                            Enviado
                        ) 
                        VALUES (
                            @Fecha,
                            @IdCliente,
                            @Total,
                            @Enviado
                        )";

                    using (var insertCommand = new SqliteCommand(insertQuery, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@Fecha", pedido.fecha);
                        insertCommand.Parameters.AddWithValue("@IdCliente", pedido.idCliente);
                        insertCommand.Parameters.AddWithValue("@Total", pedido.total);
                        insertCommand.Parameters.AddWithValue("@Enviado", pedido.enviado);

                        await insertCommand.ExecuteNonQueryAsync();
                    }

                    // Obtener el ID del pedido recién insertado
                    string selectQuery = "SELECT last_insert_rowid()";
                    using (var selectCommand = new SqliteCommand(selectQuery, connection))
                    {
                        var idPedido = (long)await selectCommand.ExecuteScalarAsync();
                        pedido.idPedido = (int)idPedido;
                    }

                    // Crear objeto pedido para Firebase y guardar en Firebase
                    await client.Child("Pedidos").PostAsync(pedido);

                    Debug.WriteLine("Pedido insertado correctamente en la base de datos SQLite y en Firebase.");
                    return pedido.idPedido;
                    return pedido.idPedido;
                }
            }
            catch (SqliteException e)
            {
                Debug.WriteLine($"Error al insertar pedido en la base de datos SQLite: {e.Message}");
                return -1;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error al insertar pedido: {e.Message}");
                return -1;
            }
        }


        public void ActualizarTotalPedido(string databaseName, int idPedido, decimal nuevoTotal)
        {
            try
            {
                using (SqliteConnection connection = ConnectToDatabase(databaseName))
                {
                    connection.Open();
                    string updateQuery = @"
                        UPDATE Pedidos
                        SET Total = @NuevoTotal
                        WHERE IdPedido = @IdPedido";

                    using (var command = new SqliteCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@NuevoTotal", nuevoTotal);
                        command.Parameters.AddWithValue("@IdPedido", idPedido);

                        command.ExecuteNonQuery();
                    }

                    Debug.WriteLine("Total del pedido actualizado correctamente en la base de datos.");
                }
            }
            catch (SqliteException e)
            {
                Debug.WriteLine($"Error al actualizar el total del pedido en la base de datos: {e.Message}");
            }
        }


        public void ActualizarLineaPedido(string databaseName, int idLineaPedido, int nuevasUnidades, decimal nuevoPrecioTotal)
        {
            try
            {
                using (SqliteConnection connection = ConnectToDatabase(databaseName))
                {
                    connection.Open();
                    string updateQuery = @"
                        UPDATE LineasPedidos 
                        SET Unidades = @NuevasUnidades,
                            Total = @NuevoPrecioTotal
                        WHERE IdLineaPedido = @IdLineaPedido";

                    using (var command = new SqliteCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@NuevasUnidades", nuevasUnidades);
                        command.Parameters.AddWithValue("@NuevoPrecioTotal", nuevoPrecioTotal);
                        command.Parameters.AddWithValue("@IdLineaPedido", idLineaPedido);

                        command.ExecuteNonQuery();
                    }

                    Debug.WriteLine("Línea de pedido actualizada correctamente en la base de datos.");
                }
            }
            catch (SqliteException e)
            {
                Debug.WriteLine($"Error al actualizar la línea de pedido en la base de datos: {e.Message}");
            }
        }



        public LineasPedido ObtenerLineaPedido(string databaseName, int idPedido, int idArticulo)
        {
            LineasPedido lineaPedido = null;

            try
            {
                using (SqliteConnection connection = ConnectToDatabase(databaseName))
                {
                    connection.Open();
                    string selectQuery = @"
                SELECT * FROM LineasPedidos 
                WHERE IdPedido = @IdPedido AND IdArticulo = @IdArticulo";

                    using (var command = new SqliteCommand(selectQuery, connection))
                    {
                        command.Parameters.AddWithValue("@IdPedido", idPedido);
                        command.Parameters.AddWithValue("@IdArticulo", idArticulo);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                lineaPedido = new LineasPedido
                                {
                                    idLineaPedido = reader.GetInt32(reader.GetOrdinal("IdLineaPedido")),
                                    idPedido = reader.GetInt32(reader.GetOrdinal("IdPedido")),
                                    idArticulo = reader.GetInt32(reader.GetOrdinal("IdArticulo")),
                                    referencia = reader.IsDBNull(reader.GetOrdinal("Referencia")) ? null : reader.GetString(reader.GetOrdinal("Referencia")),
                                    coste = reader.GetInt32(reader.GetOrdinal("Coste")),
                                    venta = reader.GetInt32(reader.GetOrdinal("Venta")),
                                    unidades = reader.GetInt32(reader.GetOrdinal("Unidades")),
                                    total = reader.GetInt32(reader.GetOrdinal("Total"))
                                };
                            }
                        }
                    }
                }
            }
            catch (SqliteException e)
            {
                Debug.WriteLine($"Error al obtener la línea de pedido de la base de datos: {e.Message}");
            }

            return lineaPedido;
        }


        


        public void CrearTablaLineasPedidos(string databaseName)
        {
            try
            {
                using (SqliteConnection connection = ConnectToDatabase(databaseName))
                {
                    connection.Open();
                    string createTableQuery = @"
                    CREATE TABLE IF NOT EXISTS LineasPedidos(
                        IdLineaPedido INTEGER PRIMARY KEY AUTOINCREMENT,
                        IdPedido INTEGER,
                        IdArticulo INTEGER,
                        Referencia TEXT,
                        Coste DECIMAL,
                        Venta DECIMAL,
                        Unidades INTEGER,
                        Total INTEGER
                    )";

                    using (var command = new SqliteCommand(createTableQuery, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    Debug.WriteLine("Tabla de LineasPedidos creada correctamente en la base de datos.");
                }
            }
            catch (SqliteException e)
            {
                Debug.WriteLine($"Error al crear tabla de LineasPedidos en la base de datos: {e.Message}");
            }
        }


        public void CrearTablaAgenda(string databaseName)
        {
            try
            {
                using (SqliteConnection connection = ConnectToDatabase(databaseName))
                {
                    connection.Open();
                    string createTableQuery = @"
                        CREATE TABLE IF NOT EXISTS Agenda(
                            IdEvento INTEGER PRIMARY KEY AUTOINCREMENT,
                            IdUsuario INTEGER,
                            Fecha TEXT,
                            Hora TEXT,
                            Notas TEXT
                        )";

                    using (var command = new SqliteCommand(createTableQuery, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    Debug.WriteLine("Tabla de Agenda creada correctamente en la base de datos.");
                }
            }
            catch (SqliteException e)
            {
                Debug.WriteLine($"Error al crear tabla de Agenda en la base de datos: {e.Message}");
            }
        }



        public async Task InsertarAgenda(string databaseName, Agenda agenda)
        {
            try
            {
                using (SqliteConnection connection = ConnectToDatabase(databaseName))
                {
                    connection.Open();

                    // No es necesario crear la tabla aquí si ya está creada en la base de datos SQLite

                    string insertQuery = @"
                        INSERT INTO Agenda (
                            IdUsuario,  
                            Fecha,
                            Hora,
                            Notas
                        ) 
                        VALUES (
                            @IdUsuario,
                            @Fecha,
                            @Hora,
                            @Notas
                        )";

                    using (var insertCommand = new SqliteCommand(insertQuery, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@IdUsuario", agenda.idUsuario);

                        // Convertir Fecha a formato de texto adecuado o DBNull.Value si es nulo
                        insertCommand.Parameters.AddWithValue("@Fecha", agenda.fecha.ToString("yyyy-MM-dd"));

                        // Convertir Hora a formato de texto adecuado o DBNull.Value si es nulo
                        insertCommand.Parameters.AddWithValue("@Hora", agenda.hora.ToString(@"hh\:mm\:ss"));

                        insertCommand.Parameters.AddWithValue("@Notas", agenda.notas ?? ""); // Manejo de valores nulos para notas

                        insertCommand.ExecuteNonQuery();
                    }

                    // Crear objeto agenda para Firebase y guardar en Firebase
                    await client.Child("Agenda").PostAsync(agenda);

                    Debug.WriteLine("Cita de agenda insertada correctamente en la base de datos SQLite y en Firebase.");
                }
            }
            catch (FormatException ex)
            {
                Debug.WriteLine($"Error de formato al insertar cita de agenda: {ex.Message}");
                throw; // Lanzar la excepción para manejarla más arriba si es necesario
            }
            catch (SqliteException ex)
            {
                Debug.WriteLine($"Error al insertar cita de agenda en la base de datos SQLite: {ex.Message}");
                throw; // Lanzar la excepción para manejarla más arriba si es necesario
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al insertar cita de agenda: {ex.Message}");
                throw; // Lanzar la excepción para manejarla más arriba si es necesario
            }
        }





        public async Task InsertarUsuario(string databaseName, Usuario usuario)
        {
            try
            {
                using (SqliteConnection connection = ConnectToDatabase(databaseName))
                {
                    connection.Open();

                    string insertQuery = @"
                        INSERT INTO Usuarios (
                            Nombre,
                            Gmail,
                            Contra,
                            Rol,
                            FechaRegistro
                        ) VALUES (
                            @Nombre,
                            @Gmail,
                            @Contra,
                            @Rol,
                            @FechaRegistro
                        )";

                    using (var insertCommand = new SqliteCommand(insertQuery, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@Gmail", usuario.Gmail);
                        insertCommand.Parameters.AddWithValue("@Nombre", usuario.Nombre);
                        insertCommand.Parameters.AddWithValue("@Contra", usuario.Contra);
                        insertCommand.Parameters.AddWithValue("@Rol", (int)usuario.Rol);
                        insertCommand.Parameters.AddWithValue("@FechaRegistro", usuario.FechaRegistro.ToString("yyyy-MM-dd HH:mm:ss"));

                        insertCommand.ExecuteNonQuery();
                    }

                    Debug.WriteLine("Usuario insertado correctamente en la base de datos SQLite.");
                }

                // Obtener el ID del usuario insertado en SQLite (asumiendo que existe un método para esto)
                int idUsuario = ObtenerIdUsuario(databaseName, usuario.Gmail, usuario.Contra);

                // Insertar en Firebase Realtime Database
                await client.Child("Usuarios").PostAsync(new
                {
                    Id = idUsuario,
                    Gmail = usuario.Gmail,
                    Nombre = usuario.Nombre,
                    Contra = usuario.Contra,
                    Rol = (int)usuario.Rol,
                    FechaRegistro = usuario.FechaRegistro.ToString("yyyy-MM-dd HH:mm:ss")
                });

                Debug.WriteLine("Usuario insertado correctamente en Firebase Realtime Database.");
            }
            catch (SqliteException e)
            {
                Debug.WriteLine($"Error al insertar usuario en la base de datos SQLite: {e.Message}");
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error al insertar usuario en Firebase: {e.Message}");
            }
        }





        public Usuario ObtenerUsuario(string databaseName, int idUsuario)
        {
            Usuario usuario = null;

            try
            {
                using (SqliteConnection connection = ConnectToDatabase(databaseName))
                {
                    connection.Open();

                    string selectQuery = @"
                    SELECT IdUsuario, Gmail, Contra, Rol, FechaRegistro
                    FROM Usuarios
                    WHERE IdUsuario = @idUsuario";

                    using (SqliteCommand command = new SqliteCommand(selectQuery, connection))
                    {
                        // Parámetro para evitar inyecciones SQL
                        command.Parameters.AddWithValue("@idUsuario", idUsuario);

                        using (SqliteDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                usuario = new Usuario
                                {
                                    IdUsuario = Convert.ToInt32(reader["IdUsuario"]),
                                    Gmail = reader["Gmail"]?.ToString(),
                                    Contra = reader["Contra"]?.ToString(),
                                    Rol = (Rol) Convert.ToInt32(reader["Rol"]),
                                    FechaRegistro = Convert.ToDateTime(reader["FechaRegistro"])
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al obtener el usuario desde la base de datos: {ex.Message}");
            }

            return usuario;
        }

        public int ObtenerIdUsuario(string databaseName, string gmail, string contra)
        {
            int userId = -1;

            try
            {
                using (SqliteConnection connection = ConnectToDatabase(databaseName))
                {
                    connection.Open();

                    string selectQuery = @"
                    SELECT IdUsuario 
                    FROM Usuarios 
                    WHERE Gmail = @Gmail AND Contra = @Contra";

                    using (var selectCommand = new SqliteCommand(selectQuery, connection))
                    {
                        selectCommand.Parameters.AddWithValue("@Gmail", gmail);
                        selectCommand.Parameters.AddWithValue("@Contra", contra);

                        var result = selectCommand.ExecuteScalar();
                        if (result != null)
                        {
                            userId = Convert.ToInt32(result);
                        }
                    }
                }
            }
            catch (SqliteException e)
            {
                Debug.WriteLine($"Error al obtener IdUsuario de la base de datos: {e.Message}");
            }

            return userId;
        }



        public void CrearTablaUsuarios(string databaseName)
        {
            try
            {
                using (SqliteConnection connection = ConnectToDatabase(databaseName))
                {
                    connection.Open();
                    string createTableQuery = @"
                        CREATE TABLE IF NOT EXISTS Usuarios(
                            IdUsuario INTEGER PRIMARY KEY AUTOINCREMENT,
                            Nombre TEXT,
                            Gmail TEXT,
                            Contra TEXT,
                            Rol INTEGER,
                            FechaRegistro TEXT
                        )";

                    using (var command = new SqliteCommand(createTableQuery, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    Debug.WriteLine("Tabla de Usuarios creada correctamente en la base de datos.");
                }
            }
            catch (SqliteException e)
            {
                Debug.WriteLine($"Error al crear tabla de Usuarios en la base de datos: {e.Message}");
            }
        }




        public List<Cliente> ObtenerIdYNombreClientes(string databaseName)
        {
            List<Cliente> listaClientes = new List<Cliente>();

            try
            {
                using (SqliteConnection connection = ConnectToDatabase(databaseName))
                {
                    connection.Open();

                    string selectQuery = @"
                        SELECT IdCliente, Nombre
                        FROM Clientes";

                    using (SqliteCommand command = new SqliteCommand(selectQuery, connection))
                    {
                        using (SqliteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Cliente cliente = new Cliente
                                {
                                    idCliente = Convert.ToInt32(reader["IdCliente"]),
                                    nombre = reader["Nombre"].ToString()
                                };

                                listaClientes.Add(cliente);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al obtener clientes desde la base de datos: {ex.Message}");
            }

            return listaClientes;
        }

        public List<Usuario> ObtenerIdYNombreUsuarios(string databaseName)
        {
            List<Usuario> listaUsuarios = new List<Usuario>();

            try
            {
                using (SqliteConnection connection = ConnectToDatabase(databaseName))
                {
                    connection.Open();

                    string selectQuery = @"
                        SELECT IdUsuario, Nombre
                        FROM Usuarios";

                    using (SqliteCommand command = new SqliteCommand(selectQuery, connection))
                    {
                        using (SqliteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Usuario usuario = new Usuario
                                {
                                    IdUsuario = Convert.ToInt32(reader["IdUsuario"]),
                                    Nombre = reader["Nombre"].ToString()
                                };

                                listaUsuarios.Add(usuario);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al obtener usuarios desde la base de datos: {ex.Message}");
            }

            return listaUsuarios;
        }



        public List<Usuario> ObtenerUsuarioPorId(string databaseName, int idUsuario)
        {
            List<Usuario> usuarios = new List<Usuario>();

            try
            {
                using (SqliteConnection connection = ConnectToDatabase(databaseName))
                {
                    connection.Open();

                    string selectQuery = @"
                        SELECT IdUsuario, Gmail, Contra, Rol, FechaRegistro, Nombre
                        FROM Usuarios
                        WHERE IdUsuario = @IdUsuario";

                    using (SqliteCommand command = new SqliteCommand(selectQuery, connection))
                    {
                        command.Parameters.AddWithValue("@IdUsuario", idUsuario);

                        using (SqliteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Usuario usuario = new Usuario
                                {
                                    IdUsuario = Convert.ToInt32(reader["IdUsuario"]),
                                    Nombre = reader["Nombre"].ToString(),
                                    Gmail = reader["Gmail"].ToString(),
                                    Contra = reader["Contra"].ToString(),
                                    Rol = (Rol)Enum.Parse(typeof(Rol), reader["Rol"].ToString()),
                                    FechaRegistro = Convert.ToDateTime(reader["FechaRegistro"])
                                };

                                usuarios.Add(usuario);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener el usuario desde la base de datos: {ex.Message}");
            }

            return usuarios;
        }

        public List<MCA> ObtenerArticulosCategoriasMarcas(string databaseName)
        {
            List<MCA> listaMCA = new List<MCA>();

            try
            {
                using (SqliteConnection connection = ConnectToDatabase(databaseName))
                {
                    connection.Open();

                    string selectQuery = @"
                    SELECT 
                        a.idArticulo,
                        a.referencia AS referenciaArticulo,
                        a.descripcion AS descripcionArticulo,
                        a.codidoBarras AS codidoBarrasArticulo,
                        a.coste AS costeArticulo,
                        a.venta AS ventaArticulo,
                        a.ventaOferta AS ventaOfertaArticulo,
                        a.fechaAlta AS fechaAltaArticulo,
                        a.existencias AS existenciasArticulo,
                        a.urlImagen1 AS urlImagen1Articulo,
                        a.urlImagen2 AS urlImagen2Articulo,
                        a.urlImagen3 AS urlImagen3Articulo,
                        a.urlImagen4 AS urlImagen4Articulo,
                        a.urlImagen5 AS urlImagen5Articulo,
                        a.urlImagen6 AS urlImagen6Articulo,
                        a.urlImagen7 AS urlImagen7Articulo,
                        c.idCategoria,
                        c.descripcion AS descripcionCategoria,
                        c.urlImagen AS urlImagenCategoria,
                        m.idMarca,
                        m.descripcion AS descripcionMarca,
                        m.urlImagen AS urlImagenMarca
                    FROM Articulos a
                    LEFT JOIN Categorias c ON a.idCategoria = c.idCategoria
                    LEFT JOIN Marcas m ON a.idMarca = m.idMarca";


                    using (SqliteCommand command = new SqliteCommand(selectQuery, connection))
                    {
                        using (SqliteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Articulo articulo = new Articulo
                                {
                                    idArticulo = Convert.ToInt32(reader["idArticulo"]),
                                    referencia = reader["referenciaArticulo"].ToString(),
                                    descripcion = reader["descripcionArticulo"].ToString(),
                                    codidoBarras = reader["codidoBarrasArticulo"].ToString(),
                                    coste = Convert.ToInt32(reader["costeArticulo"]),
                                    venta = reader["ventaArticulo"].ToString(),
                                    ventaOferta = Convert.ToInt32(reader["ventaOfertaArticulo"]),
                                    fechaAlta = reader.IsDBNull(reader.GetOrdinal("fechaAltaArticulo")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("fechaAltaArticulo")),
                                    existencias = Convert.ToInt32(reader["existenciasArticulo"]),
                                    urlImagen1 = reader["urlImagen1Articulo"].ToString(),
                                    urlImagen2 = reader["urlImagen2Articulo"].ToString(),
                                    urlImagen3 = reader["urlImagen3Articulo"].ToString(),
                                    urlImagen4 = reader["urlImagen4Articulo"].ToString(),
                                    urlImagen5 = reader["urlImagen5Articulo"].ToString(),
                                    urlImagen6 = reader["urlImagen6Articulo"].ToString(),
                                    urlImagen7 = reader["urlImagen7Articulo"].ToString(),
                                    idCategoria = reader["idCategoria"].ToString(),
                                    idMarca = reader["idMarca"].ToString()
                                };

                                Categoria categoria = new Categoria
                                {
                                    idCategoria = reader["idCategoria"].ToString(),
                                    descripcion = reader["descripcionCategoria"].ToString(),
                                    urlImagen = reader["urlImagenCategoria"].ToString()
                                };

                                Marca marca = new Marca
                                {
                                    idMarca = reader["idMarca"].ToString(),
                                    descripcion = reader["descripcionMarca"].ToString(),
                                    urlImagen = reader["urlImagenMarca"].ToString()
                                };

                                MCA articuloCategoriaMarca = new(marca, categoria, articulo);

                                listaMCA.Add(articuloCategoriaMarca);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al obtener artículos, categorías y marcas desde la base de datos: {ex.Message}");
            }

            return listaMCA;
        }



        public static SqliteConnection ConnectToDatabase(string databaseName)
        {
            try
            {
                string connectionString = $"Data Source={databaseName}";
                SqliteConnection connection = new SqliteConnection(connectionString);
                connection.Open();
                return connection;
            }
            catch (SqliteException ex)
            {
                Debug.WriteLine("Error al conectar a la base de datos: " + ex.Message);
                return null;
            }
        }

        public bool DatabaseExists(string databaseName)
        {
            try
            {
                Debug.WriteLine(File.Exists(databaseName));
                return File.Exists(databaseName);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al verificar la existencia de la base de datos: {ex.Message}");
                return false;
            }
        }

        public List<Articulo> ObtenerArticulos(string databaseName)
        {
            List<Articulo> articulos = new List<Articulo>();

            try
            {
                using (SqliteConnection connection = ConnectToDatabase(databaseName))
                {
                    connection.Open();

                    string selectQuery = @"
                        SELECT 
                            idArticulo, 
                            referencia, 
                            descripcion, 
                            idCategoria, 
                            idMarca, 
                            codidoBarras, 
                            coste, 
                            venta, 
                            ventaOferta, 
                            fechaAlta, 
                            existencias,
                            urlImagen1, 
                            urlImagen2, 
                            urlImagen3, 
                            urlImagen4, 
                            urlImagen5, 
                            urlImagen6, 
                            urlImagen7
                            FROM Articulos;";

                    using (SqliteCommand command = new SqliteCommand(selectQuery, connection))
                    {
                        using (SqliteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Articulo articulo = new Articulo
                                {
                                    idArticulo = reader.GetInt32(0),
                                    referencia = reader.IsDBNull(1) ? null : reader.GetString(1),
                                    descripcion = reader.IsDBNull(2) ? null : reader.GetString(2),
                                    idCategoria = reader.IsDBNull(3) ? null : reader.GetString(3),
                                    idMarca = reader.IsDBNull(4) ? null : reader.GetString(4),
                                    codidoBarras = reader.IsDBNull(5) ? null : reader.GetString(5),
                                    coste = reader.GetInt32(6),
                                    venta = reader.IsDBNull(7) ? null : reader.GetString(7),
                                    ventaOferta = reader.GetInt32(8),
                                    fechaAlta = reader.IsDBNull(9) ? (DateTime?)null : reader.GetDateTime(9),
                                    existencias = reader.GetInt32(10),
                                    urlImagen1 = reader.IsDBNull(11) ? null : reader.GetString(11),
                                    urlImagen2 = reader.IsDBNull(12) ? null : reader.GetString(12),
                                    urlImagen3 = reader.IsDBNull(13) ? null : reader.GetString(13),
                                    urlImagen4 = reader.IsDBNull(14) ? null : reader.GetString(14),
                                    urlImagen5 = reader.IsDBNull(15) ? null : reader.GetString(15),
                                    urlImagen6 = reader.IsDBNull(16) ? null : reader.GetString(16),
                                    urlImagen7 = reader.IsDBNull(17) ? null : reader.GetString(17),
                                    //urlImagen8 = reader.IsDBNull(17) ? null : reader.GetString(18),
                                    //urlImagen9 = reader.IsDBNull(17) ? null : reader.GetString(19),
                                    //urlImagen10 = reader.IsDBNull(17) ? null : reader.GetString(20),
                                    //urlImagen11 = reader.IsDBNull(17) ? null : reader.GetString(21),
                                    //urlImagen12 = reader.IsDBNull(17) ? null : reader.GetString(22),
                                    //urlImagen13 = reader.IsDBNull(17) ? null : reader.GetString(23)
                                };

                                articulos.Add(articulo);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al obtener artículos desde la base de datos: {ex.Message}");
            }

            return articulos;
        }

        public static async Task<bool> CheckArticuloEnPedido(string databaseName, int idPedido, int idArticulo)
        {
            try
            {
                using (SqliteConnection connection = ConnectToDatabase(databaseName))
                {
                    await connection.OpenAsync();

                    string query = @"
                        SELECT 1 
                        FROM LineasPedidos 
                        WHERE idPedido = @idPedido AND idArticulo = @idArticulo
                        LIMIT 1";

                    using (var command = new SqliteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@idPedido", idPedido);
                        command.Parameters.AddWithValue("@idArticulo", idArticulo);

                        var result = await command.ExecuteScalarAsync();

                        // Devuelve true si se encuentra una fila, de lo contrario false
                        return result != null;
                    }
                }
            }
            catch (SqliteException e)
            {
                Debug.WriteLine($"Error al verificar el artículo en el pedido en la base de datos: {e.Message}");
                return false;
            }
        }

        internal static async Task UpdateLineaPedidosunidades(int id, object idArticulo, int v, object totalf)
        {
            throw new NotImplementedException();
        }

        internal static async Task AñadirLP(int id, int idArticulo, object referencia, object coste, double v, object cantidad, object totalf)
        {
            throw new NotImplementedException();
        }

        public List<LineasPedido> ObtenerLineasPedido(string databaseName, int idPedido)
        {
            List<LineasPedido> lineasPedido = new List<LineasPedido>();

            try
            {
                using (SqliteConnection connection = ConnectToDatabase(databaseName))
                {
                    connection.Open();

                    string selectQuery = @"
                    SELECT IdLineaPedido, IdPedido, IdArticulo, Referencia, Coste, Venta, Unidades, Total
                    FROM LineasPedidos
                    WHERE IdPedido = @IdPedido
                    ORDER BY IdLineaPedido";

                    using (SqliteCommand command = new SqliteCommand(selectQuery, connection))
                    {
                        command.Parameters.AddWithValue("@IdPedido", idPedido);

                        using (SqliteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                LineasPedido lineaPedido = new LineasPedido
                                {
                                    idLineaPedido = Convert.ToInt32(reader["IdLineaPedido"]),
                                    idPedido = Convert.ToInt32(reader["IdPedido"]),
                                    idArticulo = Convert.ToInt32(reader["IdArticulo"]),
                                    referencia = reader["Referencia"].ToString(),
                                    coste = Convert.ToInt32(reader["Coste"]),
                                    venta = Convert.ToInt32(reader["Venta"]),
                                    unidades = Convert.ToInt32(reader["Unidades"]),
                                    total = Convert.ToSingle(reader["Total"])
                                };

                                lineasPedido.Add(lineaPedido);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al obtener líneas de pedido desde la base de datos: {ex.Message}");
            }

            return lineasPedido;
        }


        public List<Articulo> ObtenerArticuloCompleto(string databaseName, int idArticulo)
        {
            List<Articulo> articulos = new List<Articulo>();

            try
            {
                using (SqliteConnection connection = ConnectToDatabase(databaseName))
                {
                    connection.Open();

                    string selectQuery = @"
                    SELECT IdArticulo, Referencia, Descripcion, IdCategoria, IdMarca, CodidoBarras, Coste, Venta, VentaOferta, FechaAlta, Existencias, UrlImagen1, UrlImagen2, UrlImagen3, UrlImagen4, UrlImagen5, UrlImagen6, UrlImagen7
                    FROM Articulos
                    WHERE IdArticulo = @idArticulo";

                    using (SqliteCommand command = new SqliteCommand(selectQuery, connection))
                    {
                        // Parámetro para evitar inyecciones SQL
                        command.Parameters.AddWithValue("@idArticulo", idArticulo);

                        using (SqliteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Articulo articulo = new Articulo
                                {
                                    idArticulo = Convert.ToInt32(reader["IdArticulo"]),
                                    referencia = reader["Referencia"]?.ToString(),
                                    descripcion = reader["Descripcion"]?.ToString(),
                                    idCategoria = reader["IdCategoria"]?.ToString(),
                                    idMarca = reader["IdMarca"]?.ToString(),
                                    codidoBarras = reader["CodidoBarras"]?.ToString(),
                                    coste = Convert.ToInt32(reader["Coste"]),
                                    venta = reader["Venta"]?.ToString(),
                                    ventaOferta = Convert.ToInt32(reader["VentaOferta"]),
                                    fechaAlta = ParseDateTime(reader["FechaAlta"]),
                                    existencias = Convert.ToInt32(reader["Existencias"]),
                                    urlImagen1 = reader["UrlImagen1"]?.ToString(),
                                    urlImagen2 = reader["UrlImagen2"]?.ToString(),
                                    urlImagen3 = reader["UrlImagen3"]?.ToString(),
                                    urlImagen4 = reader["UrlImagen4"]?.ToString(),
                                    urlImagen5 = reader["UrlImagen5"]?.ToString(),
                                    urlImagen6 = reader["UrlImagen6"]?.ToString(),
                                    urlImagen7 = reader["UrlImagen7"]?.ToString()
                                };

                                articulos.Add(articulo);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al obtener el resumen del pedido desde la base de datos: {ex.Message}");
            }

            return articulos;
        }

        private DateTime? ParseDateTime(object dateTimeObj)
        {
            if (dateTimeObj != DBNull.Value)
            {
                string dateTimeStr = dateTimeObj.ToString();
                DateTime dateTime;
                string[] formats = { "MM/dd/yyyy", "dd/MM/yyyy" }; // Agregar más formatos si es necesario

                if (DateTime.TryParseExact(dateTimeStr, formats, null, System.Globalization.DateTimeStyles.None, out dateTime))
                {
                    return dateTime;
                }
            }

            return null;
        }


        public void ActualizarPedidos(string databaseName, int idPedido, double ntotal)
        {
            try
            {
                using (SqliteConnection connection = ConnectToDatabase(databaseName))
                {
                    connection.Open();
                    string updateQuery = @"
                        UPDATE Pedidos 
                        SET Fecha = @Fecha, 
                            Total = @Total
                        WHERE IdPedido = @IdPedido";

                    using (SqliteCommand command = new SqliteCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Fecha", DateTime.Now);
                        command.Parameters.AddWithValue("@Total", ntotal);
                        command.Parameters.AddWithValue("@IdPedido", idPedido);

                        int rowsUpdated = command.ExecuteNonQuery();

                        if (rowsUpdated > 0)
                        {
                            Debug.WriteLine("Pedido actualizado correctamente.");
                        }
                        else
                        {
                            Debug.WriteLine("No se encontró el pedido para actualizar.");
                        }
                    }
                }
            }
            catch (SqliteException e)
            {
                Debug.WriteLine($"Error al actualizar pedidos en la base de datos: {e.Message}");
            }
        }

        public async Task ActualizarExistenciasT(string databaseName, int idArticulo, int cantidad)
        {
            try
            {
                using (SqliteConnection connection = ConnectToDatabase(databaseName))
                {
                    connection.Open();
                    string updateQuery = @"
                    UPDATE Articulos 
                    SET Existencias = Existencias + @Cantidad 
                    WHERE idArticulo = @IdArticulo";

                    using (SqliteCommand command = new SqliteCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Cantidad", cantidad);
                        command.Parameters.AddWithValue("@IdArticulo", idArticulo);

                        int rowsUpdated = command.ExecuteNonQuery();

                        if (rowsUpdated > 0)
                        {
                            Debug.WriteLine("Existencias actualizadas correctamente en SQLite.");

                            // Obtener las existencias actualizadas para enviar a Firebase
                            string selectQuery = @"
                            SELECT Existencias FROM Articulos 
                            WHERE idArticulo = @IdArticulo";

                            using (SqliteCommand selectCommand = new SqliteCommand(selectQuery, connection))
                            {
                                selectCommand.Parameters.AddWithValue("@IdArticulo", idArticulo);
                                int existenciasActualizadas = Convert.ToInt32(selectCommand.ExecuteScalar());

                                // Actualizar solo las existencias en Firebase
                                var updateData = new { existencias = existenciasActualizadas };
                                await client.Child("Articulos").Child(idArticulo.ToString()).PatchAsync(updateData);

                                Debug.WriteLine("Existencias actualizadas correctamente en Firebase.");
                            }
                        }
                        else
                        {
                            Debug.WriteLine("No se encontró el artículo para actualizar en SQLite.");
                        }
                    }
                }
            }
            catch (SqliteException e)
            {
                Debug.WriteLine($"Error al actualizar existencias en la base de datos SQLite: {e.Message}");
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error al actualizar existencias en Firebase: {e.Message}");
            }
        }


        public async Task EliminarLineaPedido(string databaseName, int idLineaPedido)
        {
            try
            {
                using (SqliteConnection connection = ConnectToDatabase(databaseName))
                {
                    connection.Open();
                    string deleteQuery = @"
            DELETE FROM Lineaspedidos 
            WHERE iDLineaPedido = @IdLineaPedido";

                    using (SqliteCommand command = new SqliteCommand(deleteQuery, connection))
                    {
                        command.Parameters.AddWithValue("@IdLineaPedido", idLineaPedido);

                        int rowsDeleted = command.ExecuteNonQuery();

                        if (rowsDeleted > 0)
                        {
                            Debug.WriteLine("Línea de pedido eliminada correctamente en SQLite.");

                            // Eliminar en Firebase
                            await client.Child("LineasPedidos").Child(idLineaPedido.ToString()).DeleteAsync();
                            Debug.WriteLine("Línea de pedido eliminada correctamente en Firebase.");
                        }
                        else
                        {
                            Debug.WriteLine("No se encontró la línea de pedido para eliminar en SQLite.");
                        }
                    }
                }
            }
            catch (SqliteException e)
            {
                Debug.WriteLine($"Error al eliminar la línea de pedido en la base de datos SQLite: {e.Message}");
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error al eliminar la línea de pedido en Firebase: {e.Message}");
            }
        }


        public int ObtenerExistencias(string databaseName, int idArticulo)
        {
            int existencias = 0;
            try
            {
                using (SqliteConnection connection = ConnectToDatabase(databaseName))
                {
                    connection.Open();
                    string query = @"
                    SELECT existencias 
                    FROM Articulos
                    WHERE idArticulo = @IdArticulo";

                    using (SqliteCommand command = new SqliteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IdArticulo", idArticulo);

                        using (SqliteDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                existencias = reader.GetInt32(0);
                            }
                        }
                    }
                }
            }
            catch (SqliteException e)
            {
                Debug.WriteLine($"Error al obtener las existencias del artículo en la base de datos: {e.Message}");
            }
            return existencias;
        }

        public void ActualizarExistenciasR(string databaseName, int idArticulo)
        {
            try
            {
                using (SqliteConnection connection = ConnectToDatabase(databaseName))
                {
                    connection.Open();
                    string query = @"
                    UPDATE Articulos
                    SET existencias = existencias - 1
                    WHERE idArticulo = @IdArticulo";

                    using (SqliteCommand command = new SqliteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IdArticulo", idArticulo);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (SqliteException e)
            {
                Debug.WriteLine($"Error al actualizar las existencias del artículo en la base de datos: {e.Message}");
            }
        }


        public void ActualizarExistencias(string databaseName, int idArticulo)
        {
            try
            {
                using (SqliteConnection connection = ConnectToDatabase(databaseName))
                {
                    connection.Open();
                    string query = @"
                    UPDATE Articulos
                    SET existencias = existencias + 1
                    WHERE idArticulo = @IdArticulo";

                    using (SqliteCommand command = new SqliteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IdArticulo", idArticulo);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (SqliteException e)
            {
                Debug.WriteLine($"Error al actualizar las existencias del artículo en la base de datos: {e.Message}");
            }
        }


        public async Task ActualizarLineasPedidos(string databaseName, int idLineaPedido, int nuevasUnidades, float nuevoTotal)
        {
            try
            {
                using (SqliteConnection connection = ConnectToDatabase(databaseName))
                {
                    connection.Open();
                    string query = @"
                    UPDATE LineasPedidos
                    SET unidades = @NuevasUnidades,
                        total = @NuevoTotal
                    WHERE idLineaPedido = @IdLineaPedido";

                    using (SqliteCommand command = new SqliteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@NuevasUnidades", nuevasUnidades);
                        command.Parameters.AddWithValue("@NuevoTotal", nuevoTotal);
                        command.Parameters.AddWithValue("@IdLineaPedido", idLineaPedido);

                        int rowsUpdated = command.ExecuteNonQuery();

                        if (rowsUpdated > 0)
                        {
                            Debug.WriteLine("Línea de pedido actualizada correctamente en SQLite.");

                            // Actualizar solo las propiedades necesarias en Firebase
                            var updateData = new
                            {
                                unidades = nuevasUnidades,
                                total = nuevoTotal
                            };
                            await client.Child("LineasPedidos").Child(idLineaPedido.ToString()).PatchAsync(updateData);

                            Debug.WriteLine("Línea de pedido actualizada correctamente en Firebase.");
                        }
                        else
                        {
                            Debug.WriteLine("No se encontró la línea de pedido para actualizar en SQLite.");
                        }
                    }
                }
            }
            catch (SqliteException e)
            {
                Debug.WriteLine($"Error al actualizar la línea de pedido en la base de datos SQLite: {e.Message}");
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error al actualizar la línea de pedido en Firebase: {e.Message}");
            }
        }


        public static bool ComprobarArticuloEnPedido(string databaseName, int idPedido, int idArticulo)
        {
            try
            {
                using (SqliteConnection connection = ConnectToDatabase(databaseName))
                {
                    connection.Open();

                    string query = @"
                    SELECT COUNT(*) FROM LineasPedidos
                    WHERE IdPedido = @IdPedido AND IdArticulo = @IdArticulo";

                    using (var command = new SqliteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IdPedido", idPedido);
                        command.Parameters.AddWithValue("@IdArticulo", idArticulo);

                        int count = Convert.ToInt32(command.ExecuteScalar());

                        return count > 0;
                    }
                }
            }
            catch (SqliteException e)
            {
                Debug.WriteLine($"Error al comprobar artículo en pedido: {e.Message}");
                return false;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error inesperado al comprobar artículo en pedido: {e.Message}");
                return false;
            }
        }


        public async Task ActualizarUnidadesPedidos(string databaseName, int idPedido, int idArticulo, int nuevasUnidades, double nuevoTotal)
        {
            try
            {
                using (SqliteConnection connection = ConnectToDatabase(databaseName))
                {
                    connection.Open();

                    string query = @"
                    UPDATE LineasPedidos 
                    SET Unidades = Unidades + @NuevasUnidades, 
                        Total = @NuevoTotal 
                    WHERE IdPedido = @IdPedido AND IdArticulo = @IdArticulo";

                    using (var command = new SqliteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@NuevasUnidades", nuevasUnidades);
                        command.Parameters.AddWithValue("@NuevoTotal", nuevoTotal);
                        command.Parameters.AddWithValue("@IdPedido", idPedido);
                        command.Parameters.AddWithValue("@IdArticulo", idArticulo);

                        int rowsUpdated = await command.ExecuteNonQueryAsync();

                        if (rowsUpdated > 0)
                        {
                            Debug.WriteLine("Unidades de pedido actualizadas correctamente en la base de datos SQLite.");

                            // Obtener la línea de pedido actualizada para enviar a Firebase
                            string selectQuery = @"
                            SELECT idLineaPedido FROM LineasPedidos 
                            WHERE IdPedido = @IdPedido AND IdArticulo = @IdArticulo";

                            using (SqliteCommand selectCommand = new SqliteCommand(selectQuery, connection))
                            {
                                selectCommand.Parameters.AddWithValue("@IdPedido", idPedido);
                                selectCommand.Parameters.AddWithValue("@IdArticulo", idArticulo);

                                int idLineaPedido = Convert.ToInt32(await selectCommand.ExecuteScalarAsync());

                                // Actualizar solo las propiedades necesarias en Firebase
                                var updateData = new
                                {
                                    unidades = nuevasUnidades,
                                    total = nuevoTotal
                                };
                                await client.Child("LineasPedidos").Child(idLineaPedido.ToString()).PatchAsync(updateData);

                                Debug.WriteLine("Unidades de pedido actualizadas correctamente en Firebase.");
                            }
                        }
                        else
                        {
                            Debug.WriteLine("No se encontró la línea de pedido para actualizar en SQLite.");
                        }
                    }
                }
            }
            catch (SqliteException e)
            {
                Debug.WriteLine($"Error al actualizar unidades de pedido en la base de datos SQLite: {e.Message}");
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error al actualizar unidades de pedido en Firebase: {e.Message}");
            }
        }


        public List<Pedido> ObtenerPedidos(string databaseName)
        {
            List<Pedido> pedidos = new List<Pedido>();

            try
            {
                using (SqliteConnection connection = ConnectToDatabase(databaseName))
                {
                    connection.Open();

                    string selectQuery = @"
                    SELECT IdPedido, Fecha, IdCliente, Total, Enviado
                    FROM Pedidos
                    ORDER BY IdPedido";

                    using (SqliteCommand command = new SqliteCommand(selectQuery, connection))
                    {
                        using (SqliteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Pedido pedido = new Pedido
                                {
                                    idPedido = Convert.ToInt32(reader["IdPedido"]),
                                    fecha = reader["Fecha"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["Fecha"]) : null,
                                    idCliente = Convert.ToInt32(reader["IdCliente"]),
                                    total = Convert.ToSingle(reader["Total"]),
                                    enviado = Convert.ToInt32(reader["Enviado"])
                                };

                                pedidos.Add(pedido);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al obtener pedidos desde la base de datos: {ex.Message}");
            }

            return pedidos;
        }

        public List<Cliente> ObtenerClientePorId(string databaseName, int idCliente)
        {
            List<Cliente> clientes = new List<Cliente>();

            try
            {
                using (SqliteConnection connection = ConnectToDatabase(databaseName))
                {
                    connection.Open();

                    string selectQuery = @"
                        SELECT IdCliente, Nombre, Email, Dirección, Localidad, Provincia, CodigoPostal, Pais, Ruta, Ubicación, Enviado
                        FROM Clientes
                        WHERE IdCliente = @IdCliente";

                    using (SqliteCommand command = new SqliteCommand(selectQuery, connection))
                    {
                        command.Parameters.AddWithValue("@IdCliente", idCliente);

                        using (SqliteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Cliente cliente = new Cliente
                                {
                                    idCliente = Convert.ToInt32(reader["IdCliente"]),
                                    nombre = reader["Nombre"].ToString(),
                                    email = reader["Email"].ToString(),
                                    dirección = reader["Dirección"].ToString(),
                                    localidad = reader["Localidad"].ToString(),
                                    provincia = reader["Provincia"].ToString(),
                                    codigoPostal = reader["CodigoPostal"].ToString(),
                                    pais = reader["Pais"].ToString(),
                                    ruta = reader["Ruta"].ToString(),
                                    ubicación = reader["Ubicación"].ToString(),
                                    enviado = Convert.ToBoolean(reader["Enviado"])
                                };

                                clientes.Add(cliente);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al obtener el cliente desde la base de datos: {ex.Message}");
            }

            return clientes;
        }

        public async Task EliminarPedidosConTotalCero(string databaseName)
        {
            try
            {
                using (var connection = ConnectToDatabase(databaseName))
                {
                    connection.Open();

                    // Obtener los IdPedido de los pedidos con total = 0
                    const string selectQuery = @"
                    SELECT idPedido FROM Pedidos
                    WHERE total = 0";

                    List<int> pedidosAEliminar = new List<int>();

                    using (var selectCommand = new SqliteCommand(selectQuery, connection))
                    using (var reader = selectCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            pedidosAEliminar.Add(reader.GetInt32(0));
                        }
                    }

                    // Consulta SQL para eliminar pedidos con total = 0
                    const string deleteQuery = @"
                    DELETE FROM Pedidos
                    WHERE total = 0";

                    using (var command = new SqliteCommand(deleteQuery, connection))
                    {
                        int rowsDeleted = command.ExecuteNonQuery();

                        if (rowsDeleted > 0)
                        {
                            Console.WriteLine("Pedidos con total 0 eliminados correctamente en SQLite.");

                            // Eliminar los pedidos correspondientes en Firebase
                            foreach (var idPedido in pedidosAEliminar)
                            {
                                await client.Child("Pedidos").Child(idPedido.ToString()).DeleteAsync();
                            }

                            Console.WriteLine("Pedidos con total 0 eliminados correctamente en Firebase.");
                        }
                        else
                        {
                            Console.WriteLine("No se encontraron pedidos con total 0 para eliminar en SQLite.");
                        }
                    }
                }
            }
            catch (SqliteException e)
            {
                Console.WriteLine($"Error al eliminar pedidos con total 0 en la base de datos SQLite: {e.Message}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error al eliminar pedidos con total 0 en Firebase: {e.Message}");
            }
        }


        public void LimpiarPedidos(string databaseName)
        {
            try
            {
                using (var connection = ConnectToDatabase(databaseName))
                {
                    connection.Open();

                    // Eliminar pedidos con total = 0
                    const string deleteQuery = @"
                    DELETE FROM Pedidos
                    WHERE total = 0";
                    using (var deleteCommand = new SqliteCommand(deleteQuery, connection))
                    {
                        deleteCommand.ExecuteNonQuery();
                    }

                    // Obtener pedidos ordenados por idPedido
                    const string selectQuery = @"
                    SELECT * FROM Pedidos
                    ORDER BY idPedido";
                    using (var selectCommand = new SqliteCommand(selectQuery, connection))
                    {
                        using (var reader = selectCommand.ExecuteReader())
                        {
                            var tempPedidos = new List<Pedido>();
                            int id = 1;

                            while (reader.Read())
                            {
                                var fecha = reader.GetDateTime(reader.GetOrdinal("fecha"));
                                var idCliente = reader.GetInt32(reader.GetOrdinal("idCliente"));
                                var total = reader.GetFloat(reader.GetOrdinal("total"));

                                tempPedidos.Add(new Pedido
                                {
                                    idPedido = id,
                                    fecha = fecha,
                                    idCliente = idCliente,
                                    total = total
                                });

                                id++;
                            }

                            // Eliminar tabla original
                            const string dropTableQuery = @"
                            DROP TABLE IF EXISTS Pedidos";
                            using (var dropCommand = new SqliteCommand(dropTableQuery, connection))
                            {
                                dropCommand.ExecuteNonQuery();
                            }

                            // Crear nueva tabla
                            const string createTableQuery = @"
                            CREATE TABLE Pedidos (
                                idPedido INTEGER PRIMARY KEY AUTOINCREMENT,
                                fecha DATETIME NOT NULL,
                                idCliente INTEGER NOT NULL,
                                total REAL NOT NULL
                            )";
                            using (var createCommand = new SqliteCommand(createTableQuery, connection))
                            {
                                createCommand.ExecuteNonQuery();
                            }

                            // Insertar pedidos de la tabla temporal
                            foreach (var pedido in tempPedidos)
                            {
                                const string insertQuery = @"
                                INSERT INTO Pedidos (idPedido, fecha, idCliente, total)
                                VALUES (@IdPedido, @Fecha, @IdCliente, @Total)";
                                using (var insertCommand = new SqliteCommand(insertQuery, connection))
                                {
                                    insertCommand.Parameters.AddWithValue("@IdPedido", pedido.idPedido);
                                    insertCommand.Parameters.AddWithValue("@Fecha", pedido.fecha);
                                    insertCommand.Parameters.AddWithValue("@IdCliente", pedido.idCliente);
                                    insertCommand.Parameters.AddWithValue("@Total", pedido.total);

                                    insertCommand.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                }
            }
            catch (SqliteException e)
            {
                Console.WriteLine($"Error al limpiar los pedidos: {e.Message}");
            }
        }

        public List<Usuario> ObtenerUsuarios(string databaseName)
        {
            List<Usuario> usuarios = new List<Usuario>();

            try
            {
                using (SqliteConnection connection = ConnectToDatabase(databaseName))
                {
                    connection.Open();

                    string selectQuery = @"
                        SELECT IdUsuario, Nombre, Gmail, Contra, Rol, FechaRegistro
                        FROM Usuarios
                        ORDER BY IdUsuario";

                    using (SqliteCommand command = new SqliteCommand(selectQuery, connection))
                    {
                        using (SqliteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Usuario usuario = new Usuario
                                {
                                    IdUsuario = Convert.ToInt32(reader["IdUsuario"]),
                                    Nombre = reader["Nombre"].ToString(),
                                    Gmail = reader["Gmail"].ToString(),
                                    Contra = reader["Contra"].ToString(),
                                    Rol = (Rol)Enum.Parse(typeof(Rol), reader["Rol"].ToString()),
                                    FechaRegistro = Convert.ToDateTime(reader["FechaRegistro"])
                                };

                                usuarios.Add(usuario);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al obtener usuarios desde la base de datos: {ex.Message}");
            }

            return usuarios;
        }


        //public List<Usuario> ObtenerUsuarioPorId(string databaseName, int idUsuario)
        //{
        //    List<Usuario> usuarios = new List<Usuario>();

        //    try
        //    {
        //        using (SqliteConnection connection = ConnectToDatabase(databaseName))
        //        {
        //            connection.Open();

        //            string selectQuery = @"
        //                SELECT IdUsuario, Gmail, Contra, Rol, FechaRegistro
        //                FROM Usuarios
        //                WHERE IdUsuario = @IdUsuario";

        //            using (SqliteCommand command = new SqliteCommand(selectQuery, connection))
        //            {
        //                command.Parameters.AddWithValue("@IdUsuario", idUsuario);

        //                using (SqliteDataReader reader = command.ExecuteReader())
        //                {
        //                    while (reader.Read())
        //                    {
        //                        Usuario usuario = new Usuario
        //                        {
        //                            IdUsuario = Convert.ToInt32(reader["IdUsuario"]),
        //                            Gmail = reader["Gmail"].ToString(),
        //                            Contra = reader["Contra"].ToString(),
        //                            Rol = (Rol)Enum.Parse(typeof(Rol), reader["Rol"].ToString()),
        //                            FechaRegistro = Convert.ToDateTime(reader["FechaRegistro"])
        //                        };

        //                        usuarios.Add(usuario);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine($"Error al obtener el usuario desde la base de datos: {ex.Message}");
        //    }

        //    return usuarios;
        //}

        public void CrearTablaUP(string databaseName)
        {
            try
            {
                using (SqliteConnection connection = ConnectToDatabase(databaseName))
                {
                    connection.Open();
                    string createTableQuery = @"
                        CREATE TABLE IF NOT EXISTS UP (
                            IdUP INTEGER PRIMARY KEY AUTOINCREMENT,
                            IdUsuario INTEGER,
                            IdPedido INTEGER,
                            FOREIGN KEY(IdUsuario) REFERENCES Usuarios(IdUsuario),
                            FOREIGN KEY(IdPedido) REFERENCES Pedidos(IdPedido)
                        )";

                    using (var command = new SqliteCommand(createTableQuery, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    Debug.WriteLine("Tabla UP creada correctamente en la base de datos.");
                }
            }
            catch (SqliteException e)
            {
                Debug.WriteLine($"Error al crear tabla UP en la base de datos: {e.Message}");
            }
        }


        public void InsertarUP(string databaseName, Pedido pedido, int idUsuario)
        {
            try
            {
                using (SqliteConnection connection = ConnectToDatabase(databaseName))
                {
                    connection.Open();
                    string insertQuery = @"
                        INSERT INTO Pedidos (Fecha, IdCliente, Total, Enviado)
                        VALUES (@Fecha, @IdCliente, @Total, @Enviado);
                        SELECT last_insert_rowid();"; // Obtener el ID del pedido insertado

                    using (var command = new SqliteCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Fecha", pedido.fecha);
                        command.Parameters.AddWithValue("@IdCliente", pedido.idCliente);
                        command.Parameters.AddWithValue("@Total", pedido.total);
                        command.Parameters.AddWithValue("@Enviado", pedido.enviado);

                        int idPedido = Convert.ToInt32(command.ExecuteScalar()); // Obtener el ID del pedido insertado

                        // Insertar en la tabla UP con el ID del usuario
                        string insertUPQuery = @"
                            INSERT INTO UP (IdUsuario, IdPedido)
                            VALUES (@IdUsuario, @IdPedido);";

                        using (var insertUPCommand = new SqliteCommand(insertUPQuery, connection))
                        {
                            insertUPCommand.Parameters.AddWithValue("@IdUsuario", idUsuario);
                            insertUPCommand.Parameters.AddWithValue("@IdPedido", idPedido);

                            insertUPCommand.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al insertar pedido en la base de datos: {ex.Message}");
            }
        }

        public List<Pedido> ObtenerPedidosPorUsuario(string databaseName, int idUsuario)
        {
            List<Pedido> pedidos = new List<Pedido>();

            try
            {
                using (SqliteConnection connection = ConnectToDatabase(databaseName))
                {
                    connection.Open();

                    string selectQuery = @"
                        SELECT Pedidos.IdPedido, Pedidos.Fecha, Pedidos.IdCliente, Pedidos.Total, Pedidos.Enviado
                        FROM Pedidos
                        INNER JOIN UP ON Pedidos.IdPedido = UP.IdPedido
                        WHERE UP.IdUsuario = @IdUsuario";

                    using (SqliteCommand command = new SqliteCommand(selectQuery, connection))
                    {
                        command.Parameters.AddWithValue("@IdUsuario", idUsuario);

                        using (SqliteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Pedido pedido = new Pedido
                                {
                                    idPedido = Convert.ToInt32(reader["IdPedido"]),
                                    fecha = reader["Fecha"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["Fecha"]) : null,
                                    idCliente = Convert.ToInt32(reader["IdCliente"]),
                                    total = Convert.ToSingle(reader["Total"]),
                                    enviado = Convert.ToInt32(reader["Enviado"])
                                };

                                pedidos.Add(pedido);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al obtener pedidos por usuario desde la base de datos: {ex.Message}");
            }

            return pedidos;
        }


        internal async Task EliminarUsuario(string db, object idUsuario)
        {
            throw new NotImplementedException();
        }
    }

}
