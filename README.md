
# 📱 Gestor de Ventas - CrisoftApp

Aplicación móvil desarrollada en **.NET MAUI** para la gestión de ventas en una joyería. Utiliza **Firebase** como backend para autenticación de usuarios y sincronización de datos, además de una base de datos local SQLite para funcionamiento offline.

---

## ✨ Características

- 🧑‍💼 Login de usuarios (autenticación con Firebase)
- 🔐 Gestión de roles (admin y usuario)
- 🌐 Sincronización de datos desde Firebase a SQLite
- 📦 Visualización y gestión de clientes, marcas, artículos y categorías
- 📲 Interfaz multiplataforma (Android, iOS, Windows)
- 🛠 Funciona online y offline

---

## 🧱 Tecnologías utilizadas

- [.NET MAUI](https://learn.microsoft.com/dotnet/maui/)
- [Firebase Authentication](https://firebase.google.com/docs/auth)
- [Firebase Realtime Database](https://firebase.google.com/docs/database)
- [SQLite](https://www.sqlite.org/index.html)
- C# y XAML
- MVVM y navegación con `Shell` y `NavigationPage`

---

## ⚙️ Configuración inicial

1. **Clonar el repositorio:**

```bash
git clone https://github.com/tu-usuario/CrisoftApp.git
cd CrisoftApp
```

2. **Configurar Firebase:**

- Crear un proyecto en Firebase: https://console.firebase.google.com
- Habilitar autenticación por correo y contraseña
- Obtener la URL de la base de datos en tiempo real
- Configurar las reglas de acceso para permitir lectura y escritura según el rol
- Si es Android:
  - Añadir el archivo `google-services.json` a la carpeta `Platforms/Android`
- Si es iOS:
  - Añadir el archivo `GoogleService-Info.plist` a `Platforms/iOS`

3. **Configurar URLs en el código:**

Edita las URLs que se encuentran en `MainPage.xaml.cs`:

```csharp
string urlClientes = "https://tu-proyecto.firebaseio.com/clientes.json";
string urlMarcas = "https://tu-proyecto.firebaseio.com/marcas.json";
string urlArticulos = "https://tu-proyecto.firebaseio.com/articulos.json";
string urlCategorias = "https://tu-proyecto.firebaseio.com/categorias.json";
```

4. **Compilar y ejecutar:**

Desde Visual Studio, selecciona la plataforma deseada (Android, Windows, etc.) y ejecuta.

---

## 🧪 Credenciales de prueba

Puedes crear usuarios en Firebase y asignarles roles en la base de datos bajo la rama `/roles`. Ejemplo:

```json
"roles": {
  "usuario@email.com": "Admin",
  "cliente@email.com": "Usuario"
}
```

---

## 📝 Estructura del proyecto

```
/CrisoftApp
│
├── Models/             → Modelos de datos (Usuario, Cliente, Rol, etc.)
├── Pages/              → Vistas de la app (Login, Menús, etc.)
├── DataService/        → Acceso a SQLite y Firebase
├── ViewModels/         → Lógica de interfaz (si aplica)
├── Resources/          → Imágenes, fuentes y estilos
├── App.xaml            → Recursos globales
├── AppShell.xaml       → Estructura de navegación
└── MainPage.xaml       → Página de login
```

---

## 🔐 Roles y seguridad

- **Admin**: Accede a funciones completas (gestión de pedidos, usuarios, etc.)
- **Usuario**: Accede solo a sus propios datos (visualización limitada)

El rol se recupera desde Firebase y se redirige al usuario según corresponda.

---

## 🤝 Autor

**Jesús Luna Romero**  
Desarrollador multiplataforma con experiencia en .NET MAUI, Firebase, Java y más.

---

## 📜 Licencia

Este proyecto está bajo la licencia MIT.
