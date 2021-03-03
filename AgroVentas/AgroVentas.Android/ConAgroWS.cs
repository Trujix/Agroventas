using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Newtonsoft.Json;
using AgroVentas.Droid.AgroVentasWebServ;
using Xamarin.Forms;
using System.Net;
using AgroVentas.TablasSQlite;
using AgroVentas.Vistas;
using Acr.UserDialogs;
using System.Threading.Tasks;

[assembly: Dependency(typeof(AgroVentas.Droid.ConAgroWS))]
namespace AgroVentas.Droid
{
    public class ConAgroWS : IAgroWS
    {
        // VARIABLES GLOBALES
        private AgroVentasWS conWS = new AgroVentasWS();

        // LLAMADO DEL WEB SERVICE
        // :::::::::::::::::::: [ LOGIN ] ::::::::::::::::::::
        public string Login(string logininfo)
        {
            this.conWS = new AgroVentasWS();
            return conWS.LoginAccion(logininfo);
        }

        // :::::::::::::::::::: [ ACCIONES DE DATOS ] ::::::::::::::::::::

        // ------------ FUNCION QUE RECIBE TODA LA INFO DEL WS PARA RESTAURAR/SINCRONIZAR LA APP
        public string RecibirDataWS()
        {
            try
            {
                int IdUsuario = 0;
                var VerifUsuario = DependencyService.Get<ISQliteParams>().ConsultaUsuario("SELECT * FROM Usuario WHERE IdString = 'u5U4r10+2236'");
                foreach (var usuario in VerifUsuario)
                {
                    IdUsuario = usuario.IdUsuario;
                }
                string DataWS = conWS.UsuarioDataBD(IdUsuario);
                return DependencyService.Get<ISQliteParams>().RestaurarSQliteWS(DataWS);
            }
            catch (Exception err)
            {
                return err.ToString();
            }
        }

        // FUNCION QUE RECIBE LA LISTA DE ORDENES PEDIDOS (MLTIUSOS - AGENTES Y REPARTIDORES)
        public string ObtenerOrdenesPedido()
        {
            try
            {
                string UsuarioParam = "";
                var VerifUsuario = DependencyService.Get<ISQliteParams>().ConsultaUsuario("SELECT * FROM Usuario WHERE IdString = 'u5U4r10+2236'");
                foreach (var usuario in VerifUsuario)
                {
                    if (usuario.TipoUsuario == "agente")
                    {
                        UsuarioParam = usuario.IdUsuario.ToString();
                    }
                    else
                    {
                        UsuarioParam = usuario.UsuarioNombre;
                    }
                }
                return conWS.ObtenerPedidosLista(UsuarioParam);
            }
            catch (Exception err)
            {
                return err.ToString();
            }
        }

        // ------------- FUNCION QUE ENVIA INFO AL WS PARA SER PROCESADA Y ALMACENADA
        public string EnviarDataWS(string dataws)
        {
            int IdUsuario = 0;
            BDPendientes Pendiente = JsonConvert.DeserializeObject<BDPendientes>(dataws);
            object[] DataPendiente = { Pendiente.Data, Pendiente.Accion, Pendiente.Tabla };
            var VerifUsuario = DependencyService.Get<ISQliteParams>().ConsultaUsuario("SELECT * FROM Usuario WHERE IdString = 'u5U4r10+2236'");
            foreach (var usuario in VerifUsuario)
            {
                IdUsuario = usuario.IdUsuario;
            }
            if (VerificarInternetConnecionLocal())
            {
                var PendientesWS = DependencyService.Get<ISQliteParams>().ConsultaBDPendientes("SELECT * FROM BDPendientes");
                if (PendientesWS.Count > 0)
                {
                    int idPendiente = 0;
                    var IDPendientesLista = DependencyService.Get<ISQliteParams>().ConsultaBDPendientes("SELECT MAX(Id) AS Id FROM BDPendientes");
                    foreach (var idElem in IDPendientesLista)
                    {
                        idPendiente = idElem.Id + 1;
                    }
                    Pendiente.Id = idPendiente;
                    PendientesWS.Add(Pendiente);
                    string WS = conWS.ProcesarData(JsonConvert.SerializeObject(PendientesWS), IdUsuario);
                    if (WS == "true")
                    {
                        object[] dataLimpiarPendiente = { "l1Mp14Rd474" };
                        string limpiarPendientes = DependencyService.Get<ISQliteParams>().QueryMaestra(0, "DELETE FROM BDPendientes WHERE Accion <> ?", dataLimpiarPendiente);
                        return "true";
                    }
                    else
                    {
                        return DependencyService.Get<ISQliteParams>().QueryMaestra(0, "INSERT INTO BDPendientes (Data,Accion,Tabla) VALUES (?,?,?)", DataPendiente);
                    }
                }
                else
                {
                    List<BDPendientes> PendientesListaWS = new List<BDPendientes>() { Pendiente };
                    string WS = conWS.ProcesarData(JsonConvert.SerializeObject(PendientesListaWS), IdUsuario);
                    if (WS == "true")
                    {
                        return "true";
                    }
                    else
                    {
                        return DependencyService.Get<ISQliteParams>().QueryMaestra(0, "INSERT INTO BDPendientes (Data,Accion,Tabla) VALUES (?,?,?)", DataPendiente);
                    }
                }
            }
            else
            {
                return DependencyService.Get<ISQliteParams>().QueryMaestra(0, "INSERT INTO BDPendientes (Data,Accion,Tabla) VALUES (?,?,?)", DataPendiente);
            }
        }


        // :::::::::::::::::::: [ RECUPERACION DE CONTRASEÑA ] ::::::::::::::::::::
        public string RecuperarPassword(string usuarioparams)
        {
            return conWS.RecuperarPassword(usuarioparams);
        }

        // :::::::::::::::::::: [ DOCUMENTOS PDF ] ::::::::::::::::::::
        // FUNCION PARA GENERAR LA FICHA DEL CLIENTE
        public string GenerarFichaCliente(string clienteinfo, string correos, int idusuario)
        {
            this.conWS = new AgroVentasWS();
            return conWS.CrearClienteFichaPDF(clienteinfo, correos, idusuario);
        }

        // FUNCION PARA GENERAR EL REPORTE DE ORDEN PEDIDO
        public string GenerarOrdenPedido(string pedidoinfo)
        {
            int IdUsuario = 0;
            var Usuario = DependencyService.Get<ISQliteParams>().ConsultaUsuario("SELECT * FROM Usuario WHERE IdString = 'u5U4r10+2236'");
            foreach (var usuario in Usuario)
            {
                IdUsuario = usuario.IdUsuario;
            }
            this.conWS = new AgroVentasWS();
            return conWS.CrearOrdenPedidoPDF(pedidoinfo, IdUsuario);
        }
        // FUNCION QUE OBTIENE UNA ORDEN DE PEDIDO ESPECIFICA
        public string ObtenerOrdenPedidoDoc(int idusuario, string tipodocumento, string folioadicional)
        {
            this.conWS = new AgroVentasWS();
            return conWS.ObtenerOrdenPedidoDoc(idusuario, tipodocumento, folioadicional);
        }
        // FUNCION QUE OBTIENE UNA COTIZACION
        public string GenerarCotizacion(string cotizaciondata)
        {
            int IdUsuario = 0;
            var Usuario = DependencyService.Get<ISQliteParams>().ConsultaUsuario("SELECT * FROM Usuario WHERE IdString = 'u5U4r10+2236'");
            foreach (var usuario in Usuario)
            {
                IdUsuario = usuario.IdUsuario;
            }
            this.conWS = new AgroVentasWS();
            return conWS.CrearCotizacionPDF(IdUsuario, cotizaciondata);
        }

        // FUNCION QUE OBTIENE LA URL PARA ABRIR EL MANUAL DE USUARIO
        public string ObtenerManualUrl()
        {
            this.conWS = new AgroVentasWS();
            return conWS.ObtenerManualesUrl();
        }


        // ::::::::::::::::::::::::: [ SINCRONIZAR WEB SERVICE ] :::::::::::::::::::::::::
        // FUNCION QUE SINCRONIZA LA INFORMACION LOCAL CON LA BD
        public async void SincronizarWebService()
        {
            if (VerificarInternetConnecionLocal())
            {
                var VerifUsuario = DependencyService.Get<ISQliteParams>().ConsultaUsuario("SELECT * FROM Usuario WHERE IdString = 'u5U4r10+2236'");
                if (VerifUsuario.Count > 0)
                {
                    int IdUsuario = 0;
                    bool SincAutomatico = false;
                    foreach (var usuario in VerifUsuario)
                    {
                        IdUsuario = usuario.IdUsuario;
                        SincAutomatico = usuario.SinAutomatico;
                    }
                    if (SincAutomatico)
                    {
                        using (UserDialogs.Instance.Loading("Actualizando Servidor..."))
                        {
                            await Task.Delay(TimeSpan.FromMilliseconds(500));
                            var PendientesWS = DependencyService.Get<ISQliteParams>().ConsultaBDPendientes("SELECT * FROM BDPendientes");
                            if (PendientesWS.Count > 0)
                            {
                                string UpdateWS = conWS.ProcesarData(JsonConvert.SerializeObject(PendientesWS), IdUsuario);
                                if (UpdateWS == "true")
                                {
                                    object[] dataLimpiarPendiente = { "l1Mp14Rd474" };
                                    string LimpiarPendientes = DependencyService.Get<ISQliteParams>().QueryMaestra(0, "DELETE FROM BDPendientes WHERE Accion <> ?", dataLimpiarPendiente);
                                }
                            }
                        }
                    }
                }
            }
        }


        // :::::::::::::::::::: [ ACCIONES CON ORDENES DE PEDIDOS ] ::::::::::::::::::::
        // FUNCION QUE ASIGNA UNA ORDEN DE PEDIDO A UN REPARTIDOR
        public string AsignarOPRepartidor(int idusuario, int idordenpedido, string repartidor)
        {
            this.conWS = new AgroVentasWS();
            return conWS.AsignarOPRepartidor(idusuario, idordenpedido, repartidor);
        }

        // FUNCION QUE CAMBIA EL ESTATUS A UNA ORDEN DE PEDIDO
        public string CambiarEstatusOP(int idusuario, int idordenpedido, int estatus)
        {
            this.conWS = new AgroVentasWS();
            return conWS.CambiarEstatusOP(idusuario, idordenpedido, estatus);
        }

        //FUNCION QUE DEVUELVE EL ID DEL REPARTIDOR DE UNA ORDEN DE PEDIDO (PARA AGENTE)
        public int ObtenerIdRepartidorOP(int idordenpedido)
        {
            int IdUsuario = 0;
            var VerifUsuario = DependencyService.Get<ISQliteParams>().ConsultaUsuario("SELECT * FROM Usuario WHERE IdString = 'u5U4r10+2236'");
            foreach (var Usuario in VerifUsuario)
            {
                IdUsuario = Usuario.IdUsuario;
            }
            this.conWS = new AgroVentasWS();
            return conWS.ObtenerIDRepartidorOP(IdUsuario, idordenpedido);
        }

        // :::::::::::::::::::: [ REPORTES ONLINE ] ::::::::::::::::::::
        // FUNCION QUE DEVUELVE LOS REPORTES ONLINE
        public string ReportesOnline(string dataestructura, int reporte)
        {
            int IdUsuario = 0;
            var VerifUsuario = DependencyService.Get<ISQliteParams>().ConsultaUsuario("SELECT * FROM Usuario WHERE IdString = 'u5U4r10+2236'");
            foreach (var Usuario in VerifUsuario)
            {
                IdUsuario = Usuario.IdUsuario;
            }
            this.conWS = new AgroVentasWS();
            return conWS.EntregarReporte(IdUsuario, dataestructura, reporte);
        }


        // :::::::::::::::::::: [ AUXILIARES ] ::::::::::::::::::::
        // FUNCION QUE SIRVE PARA VERIFICAR LA CONECCION A INTERNET (LE DAMOS SALIDA A LA FUNCION PRIVADA)
        public bool VerificarInternetConeccion()
        {
            return VerificarInternetConnecionLocal();
        }

        // FUNCION PRIVADA QUE SIRVE PARA VERIFICAR LA CONECCION A INTERNET
        private bool VerificarInternetConnecionLocal()
        {
            try
            {
                HttpWebRequest iNetRequest = (HttpWebRequest)WebRequest.Create("https://dns.google");
                iNetRequest.Timeout = 5000;
                WebResponse iNetResponse = iNetRequest.GetResponse();
                iNetResponse.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}