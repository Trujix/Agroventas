using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Acr.UserDialogs;
using AgroVentas.TablasSQlite;

namespace AgroVentas.Vistas
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Configuracion : TabbedPage
    {
        public const string IconCircAlerta = "\uf071";
        // VARIABLE LOCAL QUE IMPIDE EJECUTAR EL GUARDADO DE CONFS AL PRIMER USO (SWITCH)
        private bool ConfPrimerUso = true;
        public Configuracion()
        {
            InitializeComponent();
            LLenarMenusConfiguracion();
        }

        // ------------------ [ ********* ] ------------------
        // VARIABLES GLOBALES
        private class UsuarioLogin
        {
            public string Usuario { get; set; }
            public string Pass { get; set; }
            public string SecurityID { get; set; }
        }
        private class ParamsUsuarioLogin
        {
            public string Respuesta { get; set; }
            public string UsuarioInfo { get; set; }
            public string TipoUsuario { get; set; }
        }

        // FUNCION DE ARRANQUE LLENA LOS MENUS DEL MENU CONFIGURACION
        private async void LLenarMenusConfiguracion()
        {
            bool SincAutomatico = false;
            string NombreAgente = "", Email = "";
            double DllCredito = 0, DllContado = 0;
            var VerifUsuario = DependencyService.Get<ISQliteParams>().ConsultaUsuario("SELECT * FROM Usuario WHERE IdString = 'u5U4r10+2236'");
            foreach (var usuario in VerifUsuario)
            {
                SincAutomatico = usuario.SinAutomatico;
                NombreAgente = usuario.Nombre;
                Email = usuario.Correo;
                DllCredito = usuario.DollarCredito;
                DllContado = usuario.DollarContado;
            }

            ConfigNomAgente.Text = NombreAgente;
            ConfigEmailAgente.Text = Email;

            ConfigSincAuto.IsToggled = SincAutomatico;

            var PendientesWS = DependencyService.Get<ISQliteParams>().ConsultaBDPendientes("SELECT * FROM BDPendientes");
            if (PendientesWS.Count > 0)
            {
                ConfigSincManual.BackgroundColor = Color.FromRgb(220, 53, 69);
                ConfigSincManual.Text = "\uf071 Sincronizar con el servidor";
                ConfigSincManual.AutomationId = "1";
            }
            else
            {
                ConfigSincManual.BackgroundColor = Color.FromRgb(40, 167, 69);
                ConfigSincManual.Text = "\uf058 Sincronizado con el servidor";
                ConfigSincManual.AutomationId = "0";
            }
            ConfPrimerUso = false;

            ConfigCreditoDLL.Text = DllCredito.ToString("F");
            ConfigContadoDLL.Text = DllContado.ToString("F");

            await Task.Delay(TimeSpan.FromMilliseconds(500));
            UserDialogs.Instance.HideLoading();
        }

        // -------------------------------------------------------------------------------------------
        // ********************************* [ CONFIG MENU - DATOS ] *********************************

        // BOTON QUE CIERRA LA SESIÓN DEL USUARIO
        private async void Cerrar_Sesion(object sender, EventArgs e)
        {
            var borrar = await DisplayAlert("Atencion!", "¿Desea cerrar sesión?", "Si", "Cancelar");
            if (borrar)
            {
                object[] CerrarSesionData = { false, "u5U4r10+2236" };
                string CerrarSesion = DependencyService.Get<ISQliteParams>().QueryMaestra(1, "UPDATE Usuario SET Logeado=? WHERE IdString=?", CerrarSesionData);
                await Navigation.PushModalAsync(new MainPage());
            }
        }

        // FUNCION QUE CONTROLA CUANDO EL SWITCH DE SINCRONIZAR AUTO CON EL SERVIDOR SE ENCIENDE O APAGA
        private async void ConfigSincAuto_Toggled(object sender, ToggledEventArgs e)
        {
            if (!ConfPrimerUso)
            {
                using (UserDialogs.Instance.Loading("Guardando Cambios..."))
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(500));
                    object[] ActSinAutomatica = { ConfigSincAuto.IsToggled, "u5U4r10+2236" };
                    string CerrarSesion = DependencyService.Get<ISQliteParams>().QueryMaestra(1, "UPDATE Usuario SET SinAutomatico=? WHERE IdString=?", ActSinAutomatica);

                    if (ConfigSincAuto.IsToggled)
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(500));
                        DependencyService.Get<IAgroWS>().SincronizarWebService();
                    }
                }
            }
        }

        // FUNCION QUE CONTROLA LA ACTUALIZACION MANUAL CON EL SERVIDOR
        private void Sincronizar_Manual(object sender, EventArgs e)
        {
            int Accion = Convert.ToInt32(ConfigSincManual.AutomationId);
            if (Accion > 0)
            {
                if (DependencyService.Get<IAgroWS>().VerificarInternetConeccion())
                {
                    DependencyService.Get<IAgroWS>().SincronizarWebService();
                    var PendientesWS = DependencyService.Get<ISQliteParams>().ConsultaBDPendientes("SELECT * FROM BDPendientes");
                    if (PendientesWS.Count == 0)
                    {
                        ConfigSincManual.BackgroundColor = Color.FromRgb(40, 167, 69);
                        ConfigSincManual.Text = "\uf058 Sincronizado con el servidor";
                        ConfigSincManual.AutomationId = "0";
                    }
                }
            }
        }

        // FUNCION QUE DESCARGA LA INFORMACION DEL SERVIDOR Y ACTUALIZA LA APP
        private async void DescargarServidor_Data(object sender, EventArgs e)
        {
            bool sincronizarAPP = await DisplayAlert("Atencion!", "TODA LA INFORMACIÓN SE REESTABLECERÁ\n¿Desea continuar?", "Si", "Cancelar");
            if (sincronizarAPP)
            {
                if (DependencyService.Get<IAgroWS>().VerificarInternetConeccion())
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

        // BOTON QUE DESACTIVA EL DISPOSITIVO Y RESTABLECE EL USUARIO
        private async void Desactivar_Dispositivo(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(ConfigPassDesactivar.Text))
            {
                ConfigPassDesactivar.Focus();
                DependencyService.Get<IAlertas>().MsgCorto("Coloque Contraseña");
            }
            else
            {
                if (DependencyService.Get<IAgroWS>().VerificarInternetConeccion())
                {
                    using (UserDialogs.Instance.Loading("Desactivando dispositivo..."))
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(500));
                        var VerifUsuario = DependencyService.Get<ISQliteParams>().ConsultaUsuario("SELECT * FROM Usuario WHERE IdString = 'u5U4r10+2236'");
                        string user = "", pass = "", securityid = "", antiguoSecurityID = "";
                        foreach (var usuario in VerifUsuario)
                        {
                            user = usuario.UsuarioNombre;
                            pass = ConfigPassDesactivar.Text;
                            securityid = usuario.SecurityID;
                            antiguoSecurityID = usuario.SecurityRestablecerID;
                        }
                        UsuarioLogin LoginVerif = new UsuarioLogin()
                        {
                            Usuario = user,
                            Pass = pass,
                            SecurityID = securityid
                        };
                        ParamsUsuarioLogin loginMsg = JsonConvert.DeserializeObject<ParamsUsuarioLogin>(DependencyService.Get<IAgroWS>().Login(JsonConvert.SerializeObject(LoginVerif)));
                        if (loginMsg.Respuesta == "true")
                        {
                            Usuario UsuarioData = new Usuario()
                            {
                                SecurityRestablecerID = antiguoSecurityID
                            };
                            BDPendientes DesactivarDisp = new BDPendientes()
                            {
                                Accion = "Desactivar",
                                Tabla = "Usuarios",
                                Data = JsonConvert.SerializeObject(UsuarioData)
                            };
                            string WS = DependencyService.Get<IAgroWS>().EnviarDataWS(JsonConvert.SerializeObject(DesactivarDisp));
                            if (WS == "true")
                            {
                                string limpiarAPP = DependencyService.Get<ISQliteParams>().DesactivarSQliteWS();
                                if (limpiarAPP == "true")
                                {
                                    await Navigation.PushModalAsync(new MainPage());
                                }
                                else
                                {
                                    await DisplayAlert("Error!", "Ocurrió un problema al desactivar dispositivo", "Aceptar");
                                }
                            }
                            else
                            {
                                await DisplayAlert("Error!", "Ocurrió un problema al verificar usuario", "Aceptar");
                            }
                        }
                        else
                        {
                            await DisplayAlert("Error!", "Contraseña incorrecta", "Aceptar");
                        }
                    }
                }
                else
                {
                    await DisplayAlert("Atención!", "Requiere conección a Internet para esa acción", "Aceptar");
                }
            }
        }


        // --------------------------------------------------------------------------------------------
        // ********************************* [ CONFIG MENU - AGENTE ] *********************************

        // BOTON QUE CONTROLA EL GUARDADO DE LOS CAMBIOS DE AGENTE
        private async void Guardar_ConfInfoAgente(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(ConfigNomAgente.Text))
            {
                ConfigNomAgente.Focus();
                DependencyService.Get<IAlertas>().MsgCorto("Coloque Nombre Agente");
            }
            else if (string.IsNullOrEmpty(ConfigEmailAgente.Text))
            {
                ConfigEmailAgente.Focus();
                DependencyService.Get<IAlertas>().MsgCorto("Coloque Correo Electrónico");
            }
            else
            {
                using (UserDialogs.Instance.Loading("Guardando Cambios..."))
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(500));
                    Usuario UsuarioData = new Usuario()
                    {
                        Nombre = ConfigNomAgente.Text,
                        Correo = ConfigEmailAgente.Text
                    };
                    BDPendientes CambiosAgente = new BDPendientes()
                    {
                        Accion = "Modificar",
                        Tabla = "Usuarios",
                        Data = JsonConvert.SerializeObject(UsuarioData)
                    };
                    string WS = DependencyService.Get<IAgroWS>().EnviarDataWS(JsonConvert.SerializeObject(CambiosAgente));
                    object[] cambioData = { ConfigNomAgente.Text, ConfigEmailAgente.Text, "u5U4r10+2236" };
                    string RestaurarAccion = DependencyService.Get<ISQliteParams>().QueryMaestra(1, "UPDATE Usuario SET Nombre=?,Correo=? WHERE IdString=?", cambioData);
                    if (RestaurarAccion == "true")
                    {
                        await DisplayAlert("Éxito!", "Cambios Guardados correctamente", "Aceptar");
                    }
                    else
                    {
                        await DisplayAlert("Error!", RestaurarAccion, "Aceptar");
                    }
                }
            }
        }

        // BOTON QUE GUARDA EL CAMBIO DE CONTRASEÑA DEL USUARIO
        private async void Cambiar_Password(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(ConfigAntiguaPass.Text))
            {
                ConfigAntiguaPass.Focus();
                DependencyService.Get<IAlertas>().MsgCorto("Coloque Antigua Contraseña");
            }
            else if (string.IsNullOrEmpty(ConfigNuevaPass.Text))
            {
                ConfigNuevaPass.Focus();
                DependencyService.Get<IAlertas>().MsgCorto("Coloque Nueva Contraseña");
            }
            else if (string.IsNullOrEmpty(ConfigConfirmarPass.Text))
            {
                ConfigConfirmarPass.Focus();
                DependencyService.Get<IAlertas>().MsgCorto("Coloque Confirmar Contraseña");
            }
            else if (ConfigNuevaPass.Text != ConfigConfirmarPass.Text)
            {
                ConfigNuevaPass.Focus();
                await DisplayAlert("Atención!", "Las contraseñas no coinciden", "Aceptar");
            }
            else
            {
                if (DependencyService.Get<IAgroWS>().VerificarInternetConeccion())
                {
                    using (UserDialogs.Instance.Loading("Guardando Cambios..."))
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(500));
                        var VerifUsuario = DependencyService.Get<ISQliteParams>().ConsultaUsuario("SELECT * FROM Usuario WHERE IdString = 'u5U4r10+2236'");
                        string user = "", pass = "", securityid = "";
                        foreach (var usuario in VerifUsuario)
                        {
                            user = usuario.UsuarioNombre;
                            pass = ConfigAntiguaPass.Text;
                            securityid = usuario.SecurityID;
                        }
                        UsuarioLogin LoginVerif = new UsuarioLogin()
                        {
                            Usuario = user,
                            Pass = pass,
                            SecurityID = securityid
                        };
                        ParamsUsuarioLogin loginMsg = JsonConvert.DeserializeObject<ParamsUsuarioLogin>(DependencyService.Get<IAgroWS>().Login(JsonConvert.SerializeObject(LoginVerif)));
                        if (loginMsg.Respuesta == "true")
                        {
                            Usuario UsuarioData = new Usuario()
                            {
                                SecurityID = ConfigNuevaPass.Text
                            };
                            BDPendientes CambioPass = new BDPendientes()
                            {
                                Accion = "Password",
                                Tabla = "Usuarios",
                                Data = JsonConvert.SerializeObject(UsuarioData)
                            };
                            string WS = DependencyService.Get<IAgroWS>().EnviarDataWS(JsonConvert.SerializeObject(CambioPass));
                            if (WS == "true")
                            {
                                await DisplayAlert("Éxito!", "La Contraseña ha sido modificada", "Aceptar");
                                ConfigAntiguaPass.Text = "";
                                ConfigNuevaPass.Text = "";
                                ConfigConfirmarPass.Text = "";
                            }
                            else
                            {
                                await DisplayAlert("Error!", "Ocurrió un problema al actualizar contraseña", "Aceptar");
                            }
                        }
                        else
                        {
                            await DisplayAlert("Error!", "Contraseña incorrecta", "Aceptar");
                        }
                    }
                }
                else
                {
                    await DisplayAlert("Atención!", "Requiere conección a Internet para esa acción", "Aceptar");
                }
            }
        }

        // BOTON QUE ABRE EL MANUAL DE USUARIO
        private async void Abrir_ManualUsuario(object sender, EventArgs e)
        {
            using (UserDialogs.Instance.Loading("Abriendo Manual..."))
            {
                await Task.Delay(TimeSpan.FromMilliseconds(1000));
                if (DependencyService.Get<IAgroWS>().VerificarInternetConeccion())
                {
                    try
                    {
                        string Manual = "";
                        var VerifUsuario = DependencyService.Get<ISQliteParams>().ConsultaUsuario("SELECT * FROM Usuario WHERE IdString = 'u5U4r10+2236'");
                        foreach(var Usuario in VerifUsuario)
                        {
                            Manual = Usuario.TipoUsuario;
                        }
                        var UrlManuales = DependencyService.Get<IAgroWS>().ObtenerManualUrl();
                        Device.OpenUri(new Uri(UrlManuales + Manual + ".pdf"));
                    }
                    catch (Exception err)
                    {
                        await DisplayAlert("Error!", err.ToString(), "Aceptar");
                    }
                }
                else
                {
                    await DisplayAlert("Atención!", "Necesita conección a Internet para esta acción", "Aceptar");
                }
            }
        }

        // --------------------------------------------------------------------------------------------
        // ********************************* [ CONFIG MENU - AGENTE ] *********************************

        // BOTON QUE ALMACENA LOS VALORES DE OTROS PARAMETROS
        private async void Guardar_OtrosParams(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(ConfigCreditoDLL.Text))
            {
                ConfigCreditoDLL.Focus();
                DependencyService.Get<IAlertas>().MsgCorto("Coloque DLL Credito");
            }
            else if (string.IsNullOrEmpty(ConfigContadoDLL.Text))
            {
                ConfigContadoDLL.Focus();
                DependencyService.Get<IAlertas>().MsgCorto("Coloque DLL Contado");
            }
            else
            {
                using (UserDialogs.Instance.Loading("Guardando Cambios..."))
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(500));
                    object[] cambioData = { Convert.ToDouble(ConfigContadoDLL.Text), Convert.ToDouble(ConfigCreditoDLL.Text), "u5U4r10+2236" };
                    string RestaurarAccion = DependencyService.Get<ISQliteParams>().QueryMaestra(1, "UPDATE Usuario SET DollarContado=?,DollarCredito=? WHERE IdString=?", cambioData);
                    if (RestaurarAccion != "true")
                    {
                        await DisplayAlert("Error!", "Ocurrió un problema al guardar los valores: - " + RestaurarAccion, "Aceptar");
                    }
                }
            }
        }
    }
}