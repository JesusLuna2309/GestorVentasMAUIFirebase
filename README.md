
# 📱 Gestor de Ventas - CrisoftApp

Aplicación de gestión de ventas desarrollada con .NET MAUI, con integración en Firebase para autenticación y uso de base de datos local para funcionamiento offline.

## 🚀 Características principales

- Inicio de sesión con validación por correo y contraseña.
- Gestión de usuarios con roles (Administrador y Usuario).
- Acceso diferenciado según el rol.
- Base de datos local SQLite que se sincroniza con Firebase al inicio (clientes, artículos, marcas y categorías).
- Navegación entre páginas con `NavigationPage`.
- Interfaz moderna y responsive gracias a MAUI.

## 🏗️ Tecnologías utilizadas

- [.NET MAUI](https://learn.microsoft.com/es-es/dotnet/maui/)
- [Firebase Authentication](https://firebase.google.com/products/auth)
- [Firebase Realtime Database / Firestore](https://firebase.google.com/products/firestore)
- SQLite (con almacenamiento local)
- C#
- XAML

## 📂 Estructura del proyecto

```
CrisoftApp/
│
├── Models/                # Modelos de datos (Usuario, Rol, Artículo...)
├── Pages/                 # Páginas de la aplicación (Login, Registro, Inicio...)
├── DataService/           # Servicios de conexión y gestión de datos locales
├── App.xaml, App.xaml.cs  # Configuración general de la app
├── AppShell.xaml          # Shell de navegación
├── MainPage.xaml          # Página de login
└── MauiProgram.cs         # Configuración inicial del proyecto
```

## 🔐 Roles de usuario

- **Administrador**: Accede a todos los apartados de gestión y control.
- **Usuario**: Accede solo a su perfil y funcionalidades básicas.

Para asignar un rol a un usuario desde Firebase, puedes crear un nodo personalizado en la base de datos como:

```json
"roles": {
  "usuario1@example.com": "Admin",
  "usuario2@example.com": "Usuario"
}
```

## 🔧 Requisitos

- Visual Studio 2022 o superior con soporte para MAUI.
- Cuenta de Firebase configurada con autenticación por email/contraseña.
- Dispositivo o emulador Android/iOS para pruebas.

## 🛠️ Futuras mejoras

Estas son algunas ideas para seguir desarrollando y mejorando la aplicación:

- 🔐 **Sistema de recuperación de contraseñas** mediante correo electrónico.
- 📝 **Historial de pedidos por usuario**, con detalles descargables en PDF.
- 📊 **Dashboard de estadísticas de ventas** para administradores.
- 🌐 **Internacionalización** (multiidioma).
- 📦 **Gestión de stock** avanzada con alertas por bajo inventario.
- 🔔 **Notificaciones push** para nuevos pedidos o cambios de estado.
- 🖼️ **Carga de imágenes para productos** desde la app.
- 🧪 **Tests unitarios** y pruebas automatizadas.
- 🧠 **Soporte offline mejorado**, sincronización en segundo plano.
- 🧩 **Sistema de roles y permisos más granular** para distintos tipos de usuario (ej. Vendedor, Supervisor).
