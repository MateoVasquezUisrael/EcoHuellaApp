# Módulo Login — EcoHuellaApp

Documentación técnica completa del módulo de autenticación y autorización.  
**Stack:** .NET MAUI 10 · Firebase Authentication · Cloud Firestore · MVVM · C# 13

---

## Tabla de contenidos

1. [Descripción general](#1-descripción-general)
2. [Arquitectura](#2-arquitectura)
3. [Estructura de archivos](#3-estructura-de-archivos)
4. [Dominio](#4-dominio)
5. [Flujos de autenticación](#5-flujos-de-autenticación)
6. [Implementaciones por plataforma](#6-implementaciones-por-plataforma)
7. [Firestore — autorización y estructura de datos](#7-firestore--autorización-y-estructura-de-datos)
8. [Configuración del proyecto Firebase](#8-configuración-del-proyecto-firebase)
9. [Paquetes NuGet](#9-paquetes-nuget)
10. [Inyección de dependencias](#10-inyección-de-dependencias)
11. [Roles y desactivación de usuarios](#11-roles-y-desactivación-de-usuarios)
12. [Seguridad](#12-seguridad)
13. [Guía de configuración paso a paso](#13-guía-de-configuración-paso-a-paso)
14. [Pruebas sin Firebase (FakeAuthService)](#14-pruebas-sin-firebase-fakeauthservice)
15. [Advertencias conocidas](#15-advertencias-conocidas)

---

## 1. Descripción general

El módulo Login implementa un sistema de autenticación y autorización de dos capas:

| Capa | Servicio | Responsabilidad |
|---|---|---|
| **Autenticación** | Firebase Authentication | Verificar identidad (`¿quién eres?`) |
| **Autorización** | Cloud Firestore | Verificar permisos (`¿puedes entrar?`) |

### Funcionalidades implementadas

- Inicio de sesión con **correo electrónico y contraseña**
- Inicio de sesión con **cuenta Google** (OAuth 2.0)
- **Cambio de contraseña obligatorio** en el primer inicio de sesión
- **Recuperación de contraseña** por correo electrónico
- **Control de acceso por Firestore**: solo usuarios registrados en la base de datos pueden ingresar
- **Desactivación de usuarios** sin eliminarlos
- **Sistema de roles** extensible (`Usuario`, `Supervisor`, `Administrador`)
- Soporte multiplataforma: **Android**, **iOS** y **Windows**

### Principios de diseño aplicados

- **MVVM estricto**: los ViewModels nunca referencian clases de plataforma
- **SRP (Single Responsibility Principle)**: autenticación e i autorización en servicios separados
- **Dependency Inversion**: los ViewModels dependen de interfaces, no de implementaciones
- **Patrón Result Object**: todas las operaciones retornan `AuthResult` en lugar de lanzar excepciones como flujo de control

---

## 2. Arquitectura

```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer                        │
│                                                             │
│  LoginPage ──► LoginViewModel                               │
│  ChangePasswordPage ──► ChangePasswordViewModel             │
│  MainPage ──► MainViewModel                                 │
│                                                             │
│  (ViewModels solo conocen interfaces del dominio)           │
└──────────────────────┬──────────────────────────────────────┘
                       │ depende de
┌──────────────────────▼──────────────────────────────────────┐
│                     Domain Layer                             │
│                                                             │
│  IAuthService        IUserRepository    IUserSessionService  │
│  INavigationService                                         │
│                                                             │
│  AppUser  AuthResult  UsuarioSistema  RolSistema            │
└──────────┬────────────────────┬────────────────────────────┘
           │ implementado por   │ implementado por
┌──────────▼──────┐   ┌─────────▼──────────────────────────┐
│  Infrastructure │   │  Platforms                          │
│                 │   │                                     │
│  Firestore      │   │  Android: FirebaseAuthService       │
│  UserRepository │   │           GoogleSignInService       │
│                 │   │  iOS:     FirebaseAuthService       │
│  UserSession    │   │  Windows: FirebaseRestAuthService   │
│  Service        │   │  Other:   FakeAuthService           │
│                 │   │                                     │
│  MauiNavigation │   └─────────────────────────────────────┘
│  Service        │
└─────────────────┘
```

### Separación de responsabilidades

```
IAuthService        → ¿Quién eres? (Firebase Authentication)
IUserRepository     → ¿Tienes permiso? (Cloud Firestore)
IUserSessionService → Estado de la sesión activa (en memoria)
INavigationService  → Navegación post-login (sin referencias de UI en VM)
```

---

## 3. Estructura de archivos

```
EcoHuellaApp/
│
├── Domain/
│   ├── Interfaces/
│   │   ├── IAuthService.cs           # Contrato de autenticación
│   │   ├── IUserRepository.cs        # Contrato de autorización (Firestore)
│   │   ├── IUserSessionService.cs    # Contrato de estado de sesión
│   │   └── INavigationService.cs     # Contrato de navegación
│   └── Models/
│       ├── AppUser.cs                # Usuario autenticado (Firebase)
│       ├── AuthResult.cs             # Resultado tipado de operaciones auth
│       ├── UsuarioSistema.cs         # Perfil de usuario en Firestore
│       └── RolSistema.cs             # Enum: Usuario, Supervisor, Administrador
│
├── Infrastructure/
│   ├── Repositories/
│   │   └── FirestoreUserRepository.cs  # Consulta Firestore vía REST API
│   └── Services/
│       ├── UserSessionService.cs       # Estado de sesión en memoria
│       ├── MauiNavigationService.cs    # Navegación con MAUI Shell
│       └── FakeAuthService.cs          # Mock para desarrollo/pruebas
│
├── Platforms/
│   ├── Android/
│   │   ├── FirebaseAuthService.Android.cs   # Auth nativa Android
│   │   ├── GoogleSignInService.Android.cs   # Google Sign-In (Credential Manager)
│   │   ├── MainActivity.cs                  # OnActivityResult para OAuth
│   │   └── google-services.json             # Configuración Firebase Android
│   ├── iOS/
│   │   ├── FirebaseAuthService.iOS.cs       # Auth nativa iOS (GIDSignIn)
│   │   ├── AppDelegate.cs                   # Firebase.Core.App.Configure()
│   │   ├── GoogleService-Info.plist         # Configuración Firebase iOS
│   │   └── Info.plist                       # REVERSED_CLIENT_ID registrado
│   └── Windows/
│       ├── App.xaml.cs                      # Entry point Windows
│       └── Package.appxmanifest             # Manifiesto de la app
│
├── Presentation/
│   ├── ViewModels/
│   │   ├── BaseViewModel.cs              # IsBusy, ErrorMessage, ExecuteAsync
│   │   ├── LoginViewModel.cs             # Lógica de login + autorización
│   │   ├── ChangePasswordViewModel.cs    # Cambio de contraseña obligatorio
│   │   └── MainViewModel.cs              # Pantalla principal post-login
│   ├── Views/
│   │   ├── LoginPage.xaml / .cs
│   │   ├── ChangePasswordPage.xaml / .cs
│   │   └── MainPage.xaml / .cs
│   └── Converters/
│       ├── InverseBoolConverter.cs
│       ├── BoolToStringConverter.cs
│       └── StringNotEmptyConverter.cs
│
├── App.xaml.cs       # Punto de entrada → siempre muestra LoginPage
└── MauiProgram.cs    # Registro de DI por plataforma
```

---

## 4. Dominio

### `AppUser` — usuario autenticado por Firebase

```csharp
public sealed record AppUser
{
    public string Uid                    { get; init; }  // UID único de Firebase
    public string Email                  { get; init; }
    public string DisplayName            { get; init; }
    public bool   IsEmailVerified        { get; init; }
    public bool   RequiresPasswordChange { get; init; }  // true en el primer login
    public IReadOnlyList<string> LinkedProviders { get; init; }
    // Valores: "password", "google.com"
}
```

### `UsuarioSistema` — perfil en Firestore

```csharp
public sealed record UsuarioSistema
{
    public string     Uid    { get; init; }  // Coincide con AppUser.Uid
    public string     Email  { get; init; }
    public string     Nombre { get; init; }
    public RolSistema Rol    { get; init; }  // Usuario | Supervisor | Administrador
    public bool       Activo { get; init; }  // false = cuenta desactivada
}
```

### `AuthResult` — resultado tipado

```csharp
// Todas las operaciones de auth retornan AuthResult — nunca lanzan excepciones
var result = await _authService.SignInWithEmailPasswordAsync(email, password);

if (result.IsSuccess)
    // result.User contiene el AppUser
else
    // result.ErrorMessage  → mensaje legible para el usuario
    // result.ErrorCode     → AuthErrorCode enum para lógica condicional
```

### `AuthErrorCode` — códigos de error

| Código | Descripción |
|---|---|
| `InvalidCredentials` | Email o contraseña incorrectos |
| `UserNotFound` | No existe cuenta con ese email |
| `UserDisabled` | Cuenta deshabilitada en Firebase |
| `WeakPassword` | Contraseña no cumple requisitos |
| `RequiresRecentLogin` | Firebase exige re-autenticación |
| `NetworkError` | Sin conexión a internet |
| `TooManyRequests` | Bloqueo temporal por intentos fallidos |
| `Cancelled` | Usuario canceló el selector de cuentas |
| `UserNotAuthorized` | Autenticado en Firebase pero sin registro en Firestore |
| `UserDeactivated` | Existe en Firestore pero `activo = false` |

---

## 5. Flujos de autenticación

### Flujo 1 — Login con email / contraseña

```
Usuario ingresa email + contraseña
          │
          ▼
IAuthService.SignInWithEmailPasswordAsync()
  │  Firebase verifica credenciales
  │  ✗ Error → SetError() y detener
  │  ✓ OK    → AppUser con IdToken
          │
          ▼
IAuthService.GetFreshTokenAsync()
  → Obtiene token JWT válido
          │
          ▼
IUserRepository.GetByUidAsync(uid, token)
  → GET Firestore /usuarios/{uid}
  │  ✗ null    → SignOut + "No tienes permiso"
  │  ✗ !activo → SignOut + "Cuenta desactivada"
  │  ✓ OK      → UsuarioSistema con Rol y Nombre
          │
          ▼
IUserSessionService.SetSession(authUser, sistemaUser)
          │
          ├── RequiresPasswordChange = true → ChangePasswordPage
          └── RequiresPasswordChange = false → MainApp (AppShell)
```

### Flujo 2 — Login con Google

Idéntico al Flujo 1 después del paso de Firebase. La diferencia está en el primer paso:

```
Android  → GoogleSignInService (Credential Manager API)
           → AuthCredential → Firebase.SignInWithCredential()

iOS      → GIDSignIn.SharedInstance.SignIn()
           → idToken + accessToken → Firebase.SignInWithCredential()

Windows  → HttpListener en puerto dinámico de 127.0.0.1
           → OAuth 2.0 + PKCE (RFC 7636) + client_secret
           → Código de autorización → token endpoint de Google
           → id_token → Firebase.signInWithIdp (REST)
```

> **Nota:** Los usuarios que ingresan con Google **nunca** pasan por `ChangePasswordPage` porque `RequiresPasswordChange` es `false` cuando el proveedor es `google.com`.

### Flujo 3 — Recuperación de contraseña

```
Usuario ingresa email en DisplayPromptAsync
          │
          ▼
IAuthService.SendPasswordResetEmailAsync(email)
  → Firebase envía correo con enlace de restablecimiento
  → El enlace expira en 1 hora
  ✓ OK → SuccessMessage en LoginPage
  ✗ Error → ErrorMessage en LoginPage

NOTA: El correo puede llegar a la carpeta de spam.
```

### Flujo 4 — Cambio de contraseña (primer login)

```
Usuario llega a ChangePasswordPage
  (RequiresPasswordChange = true)
          │
          ▼
Usuario ingresa nueva contraseña + confirmación
          │
          ▼
IAuthService.UpdatePasswordAsync(newPassword)
  Android/iOS → Firebase.UpdatePassword()
  Windows     → REST :update endpoint
          │
          ▼
Preferences.Set("first_login_{uid}", false)
  → Marca localmente que el primer login se completó
          │
          ▼
INavigationService.GoToMainApp()
  → Reemplaza toda la pila de navegación con AppShell
```

### Flujo 5 — Logout

```
Usuario pulsa "Cerrar sesión" en MainPage
          │
          ▼
IUserSessionService.ClearSession()
  → Limpia AuthUser y SistemaUser de memoria
          │
          ▼
IAuthService.SignOutAsync()
  Android  → FirebaseAuth.SignOut() + GoogleSignIn.SignOut()
  iOS      → Auth.SignOut() + GIDSignIn.SharedInstance.SignOut()
  Windows  → Limpia _currentUser e _idToken en memoria
          │
          ▼
INavigationService.GoToLoginAsync()
  → Reemplaza AppShell con NavigationPage(LoginPage)
```

---

## 6. Implementaciones por plataforma

### Android — `FirebaseAuthService.Android.cs`

- **SDK:** `Xamarin.Firebase.Auth` v124.0.1
- **Google Sign-In:** `GoogleSignInService.Android.cs` usando `Xamarin.GooglePlayServices.Auth`
- **Método Google:** `GoogleSignInClient.SignInIntent` → `OnActivityResult` en `MainActivity`
- **Requisito SHA-1:** el fingerprint del certificado debe estar registrado en Firebase Console

```
Firebase Console → Project Settings → tu app Android → Add fingerprint
```

### iOS — `FirebaseAuthService.iOS.cs`

- **SDK Auth:** `AdamE.Firebase.iOS.Auth` v12.10.0
- **SDK Google:** `AdamE.Google.iOS.SignIn` v9.0.0 (`GIDSignIn`)
- **Método Google:** `GIDSignIn.SharedInstance.SignIn()` abre `ASWebAuthenticationSession`
- **Requisito:** `REVERSED_CLIENT_ID` registrado en `Info.plist` como URL scheme

```xml
<!-- Platforms/iOS/Info.plist -->
<key>CFBundleURLSchemes</key>
<array>
  <string>com.googleusercontent.apps.1063838909055-cbho8glot83btdjrq0l4j54f3ac04bq6</string>
</array>
```

- **Inicialización:** `Firebase.Core.App.Configure()` en `AppDelegate.FinishedLaunching()`

### Windows — `FirebaseRestAuthService.cs`

- **Protocolo:** Firebase Identity Toolkit REST API v1
- **Google Sign-In:** OAuth 2.0 Authorization Code + PKCE + `client_secret`
- **Callback:** `HttpListener` en puerto dinámico de `127.0.0.1`
- **Timeout:** 3 minutos para que el usuario complete el OAuth en el navegador
- **Puerto:** dinámico en rango efímero (49152-65535), sin conflictos

```
Puerto → GetAvailablePort() → TcpListener(0) → obtiene puerto libre → libera → usa
```

> **Importante:** Google Auth Platform trata los clientes de escritorio como
> confidenciales. El `client_secret` es necesario en el intercambio del código,
> incluso cuando se usa PKCE.

### MacCatalyst — `FakeAuthService.cs`

Implementación simulada. No requiere Firebase configurado. Útil para desarrollo.

---

## 7. Firestore — autorización y estructura de datos

### Colección: `usuarios`

```
firestore/
└── usuarios/                    ← colección
    └── {uid}/                   ← documento (ID = UID de Firebase Auth)
        ├── uid:          string  ← igual al UID del documento
        ├── email:        string
        ├── nombre:       string  ← nombre completo para mostrar en la app
        ├── rol:          string  ← "Usuario" | "Supervisor" | "Administrador"
        ├── activo:       boolean ← false = desactivado (no puede ingresar)
        └── fechaCreacion: timestamp
```

### Ejemplo de documento

```json
{
  "uid":          "abc123def456",
  "email":        "carlos@empresa.com",
  "nombre":       "Carlos Rodríguez",
  "rol":          "Administrador",
  "activo":       true,
  "fechaCreacion": "2025-01-15T10:30:00Z"
}
```

### Reglas de seguridad (Firestore Rules)

```javascript
rules_version = '2';
service cloud.firestore {
  match /databases/{database}/documents {
    match /usuarios/{userId} {
      // Cada usuario solo puede leer su propio documento
      allow read: if request.auth != null
                  && request.auth.uid == userId;
      // Solo el Admin SDK (backend/consola) puede escribir
      allow write: if false;
    }
  }
}
```

### Cómo crear un usuario en Firestore

1. Ir a **Firebase Console → Authentication → Users → Add user**
2. Crear el usuario con email y contraseña temporal
3. Copiar el **UID** generado
4. Ir a **Firestore Database → usuarios → Add document**
5. Usar el UID como Document ID
6. Agregar los campos según la estructura de arriba

> **Primer login:** la app detecta automáticamente que es el primer ingreso
> (`first_login_{uid}` no existe en `Preferences`) y redirige al usuario
> a `ChangePasswordPage` antes de permitir el acceso.

---

## 8. Configuración del proyecto Firebase

### Proyecto Firebase

- **Nombre:** `login-ecohuella`
- **Project ID:** `login-ecohuella`
- **Package Android:** `com.companyname.ecohuellaapp`
- **Bundle iOS:** `com.companyname.ecohuellaapp`

### Proveedores habilitados

| Proveedor | Estado |
|---|---|
| Email/Password | ✅ Habilitado |
| Google | ✅ Habilitado |

### Archivos de configuración

| Archivo | Plataforma | Ubicación |
|---|---|---|
| `google-services.json` | Android | `Platforms/Android/` |
| `GoogleService-Info.plist` | iOS | `Platforms/iOS/` |

### Clientes OAuth 2.0 en Google Cloud Console

| Cliente | Tipo | Usado por |
|---|---|---|
| Web client (auto created) | Aplicación web | Android WebClientId |
| EcoHuellaApp Desktop | Aplicación de escritorio | Windows DesktopClientId |

---

## 9. Paquetes NuGet

### Todos los targets

| Paquete | Versión | Propósito |
|---|---|---|
| `Microsoft.Maui.Controls` | 10.0.20 | Framework base |
| `CommunityToolkit.Mvvm` | 8.4.0 | `[ObservableProperty]`, `[RelayCommand]`, `ObservableObject` |
| `Microsoft.Extensions.Logging.Debug` | 10.0.8 | Logging en Debug |
| `sqlite-net-pcl` | 1.9.172 | Base de datos local (módulo existente) |

### Android

| Paquete | Versión | Propósito |
|---|---|---|
| `Xamarin.Firebase.Auth` | 124.0.1 | Firebase Authentication nativo |
| `Xamarin.GooglePlayServices.Auth` | 121.4.0.2 | Google Sign-In |

### iOS

| Paquete | Versión | Propósito |
|---|---|---|
| `AdamE.Firebase.iOS.Auth` | 12.10.0 | Firebase Authentication nativo iOS |
| `AdamE.Firebase.iOS.Installations` | 12.10.0 | Dependencia de Firebase iOS |
| `AdamE.Google.iOS.SignIn` | 9.0.0 | Google Sign-In (`GIDSignIn`) |

### Windows

No requiere paquetes adicionales. Usa `System.Net.HttpListener` y `HttpClient` del runtime de .NET.

---

## 10. Inyección de dependencias

Toda la configuración DI del módulo está encapsulada en `MauiProgram.RegisterLoginModule()`:

```csharp
private static void RegisterLoginModule(IServiceCollection services)
{
    // Singleton: estado compartido entre ViewModels
    services.AddSingleton<IUserSessionService, UserSessionService>();
    services.AddSingleton<INavigationService,  MauiNavigationService>();

    // IAuthService — implementación nativa por plataforma
#if ANDROID
    services.AddSingleton<IAuthService,
        EcoHuellaApp.Platforms.Android.FirebaseAuthService>();
#elif IOS
    services.AddSingleton<IAuthService,
        EcoHuellaApp.Platforms.iOS.FirebaseAuthService>();
#elif WINDOWS
    services.AddSingleton<IAuthService, FirebaseRestAuthService>();
#else
    services.AddSingleton<IAuthService, FakeAuthService>(); // MacCatalyst
#endif

    // IUserRepository — Firestore REST (todas las plataformas reales)
#if MACCATALYST
    services.AddSingleton<IUserRepository, FakeUserRepository>();
#else
    services.AddSingleton<IUserRepository, FirestoreUserRepository>();
#endif

    // Transient: nueva instancia por cada página
    services.AddTransient<LoginViewModel>();
    services.AddTransient<ChangePasswordViewModel>();
    services.AddTransient<MainViewModel>();
    services.AddTransient<LoginPage>();
    services.AddTransient<ChangePasswordPage>();
    services.AddTransient<MainPage>();
}
```

### Ciclos de vida

| Tipo | Clases | Razón |
|---|---|---|
| `Singleton` | `IAuthService`, `IUserSessionService`, `INavigationService`, `IUserRepository` | Estado compartido y conexiones que deben reutilizarse |
| `Transient` | ViewModels y Pages | Evita estado obsoleto entre navegaciones |

---

## 11. Roles y desactivación de usuarios

### Roles disponibles

| Rol | Valor numérico | Descripción |
|---|---|---|
| `Usuario` | 0 | Acceso básico a la aplicación |
| `Supervisor` | 1 | Supervisión y reportes |
| `Administrador` | 2 | Acceso completo |

El valor numérico permite comparaciones de nivel de acceso:

```csharp
// Verificar si el usuario tiene nivel mínimo requerido
if (usuario.Rol >= RolSistema.Supervisor)
    // permitir acceso a función restringida
```

Para mostrar el rol en la UI se accede via `IUserSessionService`:

```csharp
// En cualquier ViewModel que reciba IUserSessionService
var rol    = _session.SistemaUser?.Rol;
var nombre = _session.SistemaUser?.Nombre;
```

### Desactivar un usuario

Cambiar `activo` a `false` en Firestore. El usuario no puede ingresar pero su cuenta se conserva. Para reactivarlo, cambiar a `true`.

```
Firebase Console → Firestore → usuarios → {uid} → activo → false
```

El mensaje que verá el usuario al intentar ingresar:

```
"Tu cuenta está desactivada. Contacta al administrador."
```

---

## 12. Seguridad

### Qué protege la arquitectura actual

| Amenaza | Protección |
|---|---|
| Cualquier persona con cuenta Google puede entrar | Firestore valida que el UID existe en `/usuarios` |
| Cuenta comprometida activa | Desactivar `activo = false` en Firestore |
| Intercepción del código OAuth (Windows) | PKCE: `code_challenge` + `code_verifier` (SHA-256) |
| State forgery en OAuth | Parámetro `state` validado en el callback |
| Token Firebase expirado | `GetFreshTokenAsync()` siempre entrega un token válido |
| Acceso a datos de otros usuarios en Firestore | Reglas: `request.auth.uid == userId` |
| Contraseña temporal del admin expuesta | Cambio obligatorio en el primer login |

### Qué NO protege (alcance del módulo)

- Rotación automática de tokens Firebase (los tokens duran 1 hora; para apps de larga sesión se requiere implementar refresh explícito)
- Protección del `client_secret` de Windows a nivel binario (mitigación: usar un proxy backend para el intercambio de código)

---

## 13. Guía de configuración paso a paso

### Requisitos previos

- Visual Studio 2022 v17.12+ con workload MAUI instalado
- Cuenta de Google con acceso a Firebase Console
- Para Android: dispositivo físico o emulador con API 23+ y Google Play Services
- Para iOS: Mac con Xcode compatible con iOS 26+ (o iOS Remote Simulator desde Windows)
- Para Windows: Windows 10 1803+ (Build 17763+)

### Paso 1 — Clonar y restaurar paquetes

```bash
git clone <repo>
cd EcoHuellaApp
dotnet restore
```

### Paso 2 — Crear proyecto en Firebase Console

1. Ir a [console.firebase.google.com](https://console.firebase.google.com)
2. Crear proyecto → nombre: `login-ecohuella`
3. **Authentication → Get Started → Sign-in method:**
   - Habilitar **Email/Password**
   - Habilitar **Google** (agregar correo de soporte)
4. **Firestore Database → Create database → Production mode**

### Paso 3 — Configurar app Android

1. Firebase Console → Project Settings → Add app → Android
2. Package name: `com.companyname.ecohuellaapp`
3. Descargar `google-services.json` → copiar a `Platforms/Android/`
4. Agregar SHA-1 del certificado de debug:

```bash
keytool -list -v ^
  -keystore "%USERPROFILE%\.android\debug.keystore" ^
  -alias androiddebugkey -storepass android -keypass android
```

5. Firebase Console → Project Settings → tu app Android → **Add fingerprint** → pegar SHA-1 → Save
6. Descargar el nuevo `google-services.json` y reemplazar

### Paso 4 — Configurar app iOS

1. Firebase Console → Project Settings → Add app → iOS
2. Bundle ID: `com.companyname.ecohuellaapp`
3. Descargar `GoogleService-Info.plist` → copiar a `Platforms/iOS/`
4. Verificar que `Info.plist` tiene el `REVERSED_CLIENT_ID` registrado como URL scheme

### Paso 5 — Configurar Windows (Google Sign-In Desktop)

1. [console.cloud.google.com](https://console.cloud.google.com) → APIs y servicios → Credenciales
2. Crear credencial → **ID de cliente OAuth 2.0 → Aplicación de escritorio**
3. Nombre: `EcoHuellaApp Desktop`
4. Copiar el **Client ID** y el **Client Secret** generados
5. En `Infrastructure/Services/FirebaseRestAuthService.cs`:

```csharp
private const string DesktopClientId     = "TU_DESKTOP_CLIENT_ID.apps.googleusercontent.com";
private const string DesktopClientSecret = "GOCSPX-TU_CLIENT_SECRET";
```

### Paso 6 — Crear reglas de Firestore

Firebase Console → Firestore Database → Rules → reemplazar con:

```javascript
rules_version = '2';
service cloud.firestore {
  match /databases/{database}/documents {
    match /usuarios/{userId} {
      allow read: if request.auth != null && request.auth.uid == userId;
      allow write: if false;
    }
  }
}
```

Clic en **Publish**.

### Paso 7 — Crear el primer usuario

1. Firebase Console → Authentication → Users → **Add user**
2. Email: `admin@tudominio.com` | Password: contraseña temporal
3. Copiar el **UID** generado
4. Firestore Database → usuarios → **Add document**
5. Document ID: `{UID copiado}`
6. Campos:

```
uid:          string  → {UID}
email:        string  → admin@tudominio.com
nombre:       string  → Nombre Completo
rol:          string  → Administrador
activo:       boolean → true
```

---

## 14. Pruebas sin Firebase (FakeAuthService)

Activo automáticamente en **MacCatalyst**. Para activarlo en otras plataformas durante desarrollo, modificar temporalmente `MauiProgram.cs`:

```csharp
// Reemplazar el bloque #if ANDROID/#elif IOS/... por:
services.AddSingleton<IAuthService, FakeAuthService>();
services.AddSingleton<IUserRepository, FakeUserRepository>();
```

### Usuarios de prueba disponibles

| Email | Contraseña | Rol | Resultado |
|---|---|---|---|
| `admin@ecohuellaapp.com` | `Admin123!` | Administrador | Primer login → `ChangePasswordPage` |
| `operador@ecohuellaapp.com` | `Oper123!` | Usuario | Login directo → `MainPage` |
| Cualquier otro | cualquiera | — | "No existe una cuenta con ese correo" |
| Google (fake) | — | — | "No tienes permiso" (UID no existe en FakeUserRepository) |

---

## 15. Advertencias conocidas

### `AlertManager: Window already had an alert manager subscription`

**Severidad:** Informativa — no afecta el funcionamiento  
**Aparece en:** Modo Debug únicamente  
**Causa:** MAUI 10 registra múltiples suscripciones de `AlertManager` al reemplazar la página raíz de la ventana (de `NavigationPage` a `AppShell`) durante el flujo de login  
**Acción:** Ninguna. Desaparece en Release.

---

## Créditos

**Módulo desarrollado por:** [Tu nombre]  
**Proyecto:** EcoHuellaApp — Fundación Aldea Las Nubes  
**Tecnologías:** .NET MAUI 10 · C# 13 · Firebase · Cloud Firestore · CommunityToolkit.Mvvm
