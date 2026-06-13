using Android.App;
using Android.Runtime;
using Firebase;                         // ← AGREGAR using

namespace EcoHuellaApp
{
    [Application]
    public class MainApplication : MauiApplication
    {
        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership) { }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        public override void OnCreate()
        {
            base.OnCreate();
            // Inicializar Firebase desde google-services.json
            // Este método es idempotente — si ya fue inicializado, no hace nada.
            FirebaseApp.InitializeApp(this);    // ← AGREGAR
        }
    }
}