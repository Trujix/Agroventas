using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Newtonsoft.Json;
using Acr.UserDialogs;
using Xamarin.Forms.Internals;
using AgroVentas.TablasSQlite;

namespace AgroVentas
{
    public partial class MainPage : ContentPage
    {
        // ******************* [ CLASES ] *******************
        // ::::::::::: [ LOGIN ] :::::::::::
        [Preserve(AllMembers = true)]
        public class LoginUsuario
        {
            public string Usuario { get; set; }
            public string Pass { get; set; }
            public string SecurityID { get; set; }
            public string NotificacionID { get; set; }
        }
        public class ParamsUsuario
        {
            public string Respuesta { get; set; }
            public string UsuarioInfo { get; set; }
            public string TipoUsuario { get; set; }
        }
        public MainPage()
        {
            InitializeComponent();
            LogginAutomatico();
        }

        // INICIO DE SESION A TRAVES DEL BOTON
        private async void Iniciar_Sesion(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(UsuarioTxt.Text))
            {
                UsuarioTxt.Focus();
                DependencyService.Get<IAlertas>().MsgCorto("Coloque el Usuario");
            }
            else if (string.IsNullOrEmpty(PasswordTxt.Text))
            {
                PasswordTxt.Focus();
                DependencyService.Get<IAlertas>().MsgCorto("Coloque la Contraseña");
            }
            else
            {
                if (DependencyService.Get<IAgroWS>().VerificarInternetConeccion())
                {
                    using (UserDialogs.Instance.Loading("Verificando Usuario..."))
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(1000));
                        bool Verif = VerificarUsuarioData();
                        string UsuarioLogeado = "NOUSER";
                        bool PermitirLogin = false;
                        LoginUsuario usuarioInfo = new LoginUsuario()
                        {
                            Usuario = UsuarioTxt.Text,
                            Pass = PasswordTxt.Text,
                            NotificacionID = DependencyService.Get<ISQliteParams>().ObtenerNotificacionID()
                        };
                        if (Verif)
                        {
                            var VerifUsuario = DependencyService.Get<ISQliteParams>().ConsultaUsuario("SELECT * FROM Usuario WHERE IdString = 'u5U4r10+2236'");
                            foreach (var usuario in VerifUsuario)
                            {
                                usuarioInfo.SecurityID = usuario.SecurityID;
                                UsuarioLogeado = usuario.UsuarioNombre;
                            }
                        }
                        if (UsuarioTxt.Text == UsuarioLogeado || UsuarioLogeado == "NOUSER")
                        {
                            PermitirLogin = true;
                        }
                        if (PermitirLogin)
                        {
                            try
                            {
                                ParamsUsuario loginMsg = JsonConvert.DeserializeObject<ParamsUsuario>(DependencyService.Get<IAgroWS>().Login(JsonConvert.SerializeObject(usuarioInfo)));
                                if (loginMsg.Respuesta == "true")
                                {
                                    if (loginMsg.TipoUsuario == "agente")
                                    {
                                        if (Verif)
                                        {
                                            object[] parametsA = { true, "u5U4r10+2236" };
                                            string EditarParamsUser = DependencyService.Get<ISQliteParams>().QueryMaestra(1, "UPDATE Usuario SET Logeado=? WHERE IdString=?", parametsA);
                                            await Navigation.PushModalAsync(new Menu());
                                        }
                                        else
                                        {
                                            Usuario usuarioData = JsonConvert.DeserializeObject<Usuario>(loginMsg.UsuarioInfo);
                                            object[] parametsN = { usuarioData.IdString, usuarioData.IdUsuario, usuarioData.UsuarioNombre, usuarioData.SecurityID, usuarioData.SecurityRestablecerID, usuarioData.Correo, usuarioData.Nombre, true, usuarioData.BDSincronizada, true, "agente", 0, 0 };
                                            string GuardarParamsUser = DependencyService.Get<ISQliteParams>().QueryMaestra(1, "INSERT INTO Usuario (IdString,IdUsuario,UsuarioNombre,SecurityID,SecurityRestablecerID,Correo,Nombre,Logeado,BDSincronizada,SinAutomatico,TipoUsuario,DollarCredito,DollarContado) VALUES (?,?,?,?,?,?,?,?,?,?,?,?,?)", parametsN);
                                            if (usuarioData.BDSincronizada)
                                            {
                                                await Navigation.PushModalAsync(new Menu());
                                            }
                                            else
                                            {
                                                await Navigation.PushModalAsync(new PrimerSinc());
                                            }
                                        }
                                    }
                                    else if (loginMsg.TipoUsuario == "repartidor")
                                    {
                                        Usuario usuarioData = JsonConvert.DeserializeObject<Usuario>(loginMsg.UsuarioInfo);
                                        object[] parametsN = { usuarioData.IdString, usuarioData.IdUsuario, usuarioData.UsuarioNombre, "CAMBIOPASS", "NA", usuarioData.Correo, usuarioData.Nombre, true, usuarioData.BDSincronizada, true, "repartidor" };
                                        string GuardarParamsUser = DependencyService.Get<ISQliteParams>().QueryMaestra(1, "INSERT INTO Usuario (IdString,IdUsuario,UsuarioNombre,SecurityID,SecurityRestablecerID,Correo,Nombre,Logeado,BDSincronizada,SinAutomatico,TipoUsuario) VALUES (?,?,?,?,?,?,?,?,?,?,?)", parametsN);
                                        await Navigation.PushModalAsync(new MenuRepartidor());
                                    }
                                }
                                else if (loginMsg.Respuesta == "false")
                                {
                                    UserDialogs.Instance.HideLoading();
                                    await DisplayAlert("Error!", "Usuario y/o contraseña incorrecto(s)...", "Aceptar");
                                }
                                else if (loginMsg.Respuesta == "errUsuario")
                                {
                                    await Navigation.PushModalAsync(new Tranza());
                                }
                                else
                                {
                                    UserDialogs.Instance.HideLoading();
                                    await DisplayAlert("Error!", "Ocurrió un problema al intentar iniciar sesión", "Aceptar");
                                }
                            }
                            catch (Exception err)
                            {
                                await DisplayAlert("Error!", "Ocurrió un problema al iniciar sesión: - " + err.ToString(), "Aceptar");
                            }
                        }
                        else
                        {
                            await DisplayAlert("Atención!", "Se ha detectado un Usuario registrado en la aplicación. Desactive el dispositivo desde el menú 'Configuración / Ajustes' si desea utilizar otro usuario", "Aceptar");
                        }
                    }
                }
                else
                {
                    await DisplayAlert("Error!", "No tiene conección a Internet", "Aceptar");
                }
            }
        }

        // FUNCION QUE HACE LOGGIN DE FORMA AUTOMATICA
        private async void LogginAutomatico()
        {
            if (VerificarUsuarioData())
            {
                bool verificado = false;
                bool sincServidor = false;
                string tipoUsuario = "";
                var VerifUsuario = DependencyService.Get<ISQliteParams>().ConsultaUsuario("SELECT * FROM Usuario WHERE IdString = 'u5U4r10+2236'");
                foreach (var usuario in VerifUsuario)
                {
                    verificado = usuario.Logeado;
                    sincServidor = usuario.BDSincronizada;
                    tipoUsuario = usuario.TipoUsuario;
                }
                if (sincServidor)
                {
                    if (verificado)
                    {
                        if (tipoUsuario == "agente")
                        {
                            await Navigation.PushModalAsync(new Menu());
                        }
                        else if (tipoUsuario == "repartidor")
                        {
                            await Navigation.PushModalAsync(new MenuRepartidor());
                        }
                    }
                }
                else
                {
                    await Navigation.PushModalAsync(new PrimerSinc());
                }
            }
        }

        // FUNCION QUE VERIFICA SI SE ENCUENTRAN LOS DATOS DE LOGGIN
        private bool VerificarUsuarioData()
        {
            bool Verificado = false;
            var VerifUsuario = DependencyService.Get<ISQliteParams>().ConsultaUsuario("SELECT * FROM Usuario");
            if (VerifUsuario.Count > 0)
            {
                Verificado = true;
            }
            return Verificado;
        }

        // BOTON QUE RESTAURA LA CONTRASEÑA DEL USUARIO (OJO: EL USUARIO DEBE HABERSE LOGEADO EN EL SISTEMA)
        private async void Recuperar_Password(object sender, EventArgs e)
        {
            if (DependencyService.Get<IAgroWS>().VerificarInternetConeccion())
            {
                var VerifUsuario = DependencyService.Get<ISQliteParams>().ConsultaUsuario("SELECT * FROM Usuario WHERE IdString = 'u5U4r10+2236'");
                if (VerifUsuario.Count > 0)
                {
                    using (UserDialogs.Instance.Loading("Verificando Usuario..."))
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(1000));
                        string ParamUsuario = "";
                        foreach (var usuario in VerifUsuario)
                        {
                            ParamUsuario = usuario.TipoUsuario + "ê" + usuario.IdUsuario.ToString() + "ê" + usuario.UsuarioNombre;
                        }
                        string ResetPass = DependencyService.Get<IAgroWS>().RecuperarPassword(ParamUsuario);
                        if (ResetPass == "true")
                        {
                            DependencyService.Get<IAlertas>().MsgCorto("Se ha enviado la info a su correo");
                        }
                        else
                        {
                            await DisplayAlert("Error!", "Ocurrió un problema al recuperar la contraseña", "Aceptar");
                        }
                    }
                }
                else
                {
                    await DisplayAlert("Atención!", "No ha iniciado sesión en este dispositivo con ningún usuario. Para reestablecer su contraseña pongase en contacto con el administrador.", "Aceptar");
                }
            }
            else
            {
                await DisplayAlert("Error!", "No tiene conección a Internet", "Aceptar");
            }
        }
    }
}