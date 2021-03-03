using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Acr.UserDialogs;

namespace AgroVentas
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PrimerSinc : ContentPage
    {
        public PrimerSinc()
        {
            InitializeComponent();
        }

        // BOTON QUE EJECUTA LA ACTUALIZACION "OBLIGATORIA" DE LA APP CON LA INFO DEL WS
        private async void Primer_SincronizarAPP(object sender, EventArgs e)
        {
            using (UserDialogs.Instance.Loading("Sincronizando App..."))
            {
                await Task.Delay(TimeSpan.FromMilliseconds(500));
                if (DependencyService.Get<IAgroWS>().VerificarInternetConeccion())
                {
                    string actualizarApp = DependencyService.Get<IAgroWS>().RecibirDataWS();
                    if (actualizarApp == "true")
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(1000));
                        await Navigation.PushModalAsync(new Menu());
                    }
                    else
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(1000));
                        await DisplayAlert("Error!", "Ocurrió un problema: " + actualizarApp, "Aceptar");
                    }
                }
                else
                {
                    DependencyService.Get<IAlertas>().MsgCorto("Se requiere conección a Internet");
                }
            }
        }
    }
}