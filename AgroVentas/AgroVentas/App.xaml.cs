using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Plugin.Connectivity;

using Microsoft.AppCenter;
using Microsoft.AppCenter.Push;
using AgroVentas.Vistas;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace AgroVentas
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new MainPage();
            DependencyService.Get<ISQliteParams>().IniciarSQlite();
            CrossConnectivity.Current.ConnectivityChanged += Current_ConnectivityChanged;
        }

        protected override void OnStart()
        {
            CrossConnectivity.Current.ConnectivityChanged += Current_ConnectivityChanged;
            if (!AppCenter.Configured)
            {
                Push.PushNotificationReceived += (sender, e) =>
                {
                    if (DependencyService.Get<ISQliteParams>().ObtenerTipoUsuario() == "agente")
                    {
                        MainPage = new Entregas();
                    }
                    else
                    {
                        MainPage = new MenuRepartidor();
                    }
                };
            }
            AppCenter.Start("10df90d6-e0ae-4f4f-8118-e2edb64ac09c", typeof(Push));
            string NotifID = DependencyService.Get<ISQliteParams>().ObtenerNotificacionID();
            AppCenter.SetUserId(NotifID);
        }

        protected override void OnSleep()
        {
            CrossConnectivity.Current.ConnectivityChanged += Current_ConnectivityChanged;
        }

        protected override void OnResume()
        {
            CrossConnectivity.Current.ConnectivityChanged += Current_ConnectivityChanged;
        }

        // FUNCION QUE CONTROLA LA ACCION DE DEVOLUCION DE INTERNET - SI VUELVE LA CONECCION INTENTA SINCRONIZAR WEB SERVICE
        private void Current_ConnectivityChanged(object sender, Plugin.Connectivity.Abstractions.ConnectivityChangedEventArgs e)
        {
            if (e.IsConnected)
            {
                DependencyService.Get<IAgroWS>().SincronizarWebService();
            }
        }
    }
}
