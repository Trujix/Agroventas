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

using SQLite;
using System.IO;
using Xamarin.Forms;
using AgroVentas.TablasSQlite;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

[assembly: Dependency(typeof(AgroVentas.Droid.SQliteParams))]
namespace AgroVentas.Droid
{
    public class SQliteParams : ISQliteParams
    {
        // ------- VARIABLES INICIALES DE USO GLOBAL -------
        private SQLiteConnection Db;
        private static SQliteParams instanciaConeccion;
        public static SQliteParams InstanciaConeccion { get { return instanciaConeccion; } }

        public class UsuarioSyncBD
        {
            public string Clientes { get; set; }
            public string Campos { get; set; }
            public string CorreosClientes { get; set; }
            public string Repartidores { get; set; }
            public string Productos { get; set; }
            public string Presentaciones { get; set; }
            public string OrdenesPedido { get; set; }
            public string OrdenesPedidoProductos { get; set; }
            public string ClientesPrecios { get; set; }
        }
        // ------- INICIAR BASE DE DATOS SQLITE (SE MANDA LLAMAR DESDE EL MAINACTIVITY) -------
        public void IniciarSQlite()
        {
            if (InstanciaConeccion != null)
            {
                instanciaConeccion.Db.Close();
            }
            instanciaConeccion = new SQliteParams();
        }

        // ------- CREACION DE LOS ELEMENTOS DE BD (BASE DE DATOS Y TABLAS) -------
        public SQliteParams()
        {
            Db = new SQLiteConnection(Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "AgroVentas.db3"));
            // CREACION DE TABLAS EN BD DE SQLITE
            Db.CreateTable<Usuario>();
            Db.CreateTable<BDPendientes>();
            Db.CreateTable<Clientes>();
            Db.CreateTable<Campos>();
            Db.CreateTable<CorreosClientes>();
            Db.CreateTable<Repartidores>();
            Db.CreateTable<Productos>();
            Db.CreateTable<Presentaciones>();
            Db.CreateTable<OrdenesPedido>();
            Db.CreateTable<OrdenesPedidoProductos>();
            Db.CreateTable<OrdenesPedidoPendientes>();
            Db.CreateTable<ClientesPrecios>();
            Db.CreateTable<Cotizaciones>();
            Db.CreateTable<CotizacionProductos>();

            // PARAMETROS DE LA APP [NOTIFICACION]
            Db.CreateTable<AppConfiguracion>();
            var VerifAppConfig = Db.Query<AppConfiguracion>("SELECT * FROM AppConfiguracion");
            if (VerifAppConfig.Count == 0)
            {
                string NotifToken = DependencyService.Get<IAlertas>().crearTokenAleatorio();
                try
                {
                    Db.BeginTransaction();
                    TableMapping TablaNotif = Db.GetMapping<AppConfiguracion>();
                    object[] NotifParams = { NotifToken };
                    var ejecutar = Db.Query(TablaNotif, "INSERT INTO AppConfiguracion (NotificacionID) VALUES (?)", NotifParams);
                    Db.Commit();
                }
                catch
                {
                    Db.Rollback();
                }
            }
        }

        // -----------******************* FUNCIONES SQLITE *******************-----------

        // -------------- LIMPIAR TODAS LAS TABLAS --------
        public string RestaurarSQliteWS(string dataws)
        {
            try
            {
                UsuarioSyncBD UsuarioWS = JsonConvert.DeserializeObject<UsuarioSyncBD>(dataws);
                List<Clientes> clienteWS = JsonConvert.DeserializeObject<List<Clientes>>(UsuarioWS.Clientes);
                List<Campos> camposWS = JsonConvert.DeserializeObject<List<Campos>>(UsuarioWS.Campos);
                List<CorreosClientes> correosClientesWS = JsonConvert.DeserializeObject<List<CorreosClientes>>(UsuarioWS.CorreosClientes);
                List<Repartidores> repartidoresWS = JsonConvert.DeserializeObject<List<Repartidores>>(UsuarioWS.Repartidores);
                List<Productos> productosWS = JsonConvert.DeserializeObject<List<Productos>>(UsuarioWS.Productos);
                List<Presentaciones> presentacionesWS = JsonConvert.DeserializeObject<List<Presentaciones>>(UsuarioWS.Presentaciones);
                List<OrdenesPedido> ordenespedidosWS = JsonConvert.DeserializeObject<List<OrdenesPedido>>(UsuarioWS.OrdenesPedido);
                List<OrdenesPedidoProductos> ordenespedidosproductosWS = JsonConvert.DeserializeObject<List<OrdenesPedidoProductos>>(UsuarioWS.OrdenesPedidoProductos);
                List<ClientesPrecios> clientespreciosWS = JsonConvert.DeserializeObject<List<ClientesPrecios>>(UsuarioWS.ClientesPrecios);

                Db.BeginTransaction();
                TableMapping tabla = Db.GetMapping<BDPendientes>();
                Db.DeleteAll(tabla);
                tabla = Db.GetMapping<Clientes>();
                Db.DeleteAll(tabla);
                tabla = Db.GetMapping<Campos>();
                Db.DeleteAll(tabla);
                tabla = Db.GetMapping<CorreosClientes>();
                Db.DeleteAll(tabla);
                tabla = Db.GetMapping<Repartidores>();
                Db.DeleteAll(tabla);
                tabla = Db.GetMapping<Productos>();
                Db.DeleteAll(tabla);
                tabla = Db.GetMapping<Presentaciones>();
                Db.DeleteAll(tabla);
                tabla = Db.GetMapping<OrdenesPedido>();
                Db.DeleteAll(tabla);
                tabla = Db.GetMapping<OrdenesPedidoProductos>();
                Db.DeleteAll(tabla);
                tabla = Db.GetMapping<OrdenesPedidoPendientes>();
                Db.DeleteAll(tabla);
                tabla = Db.GetMapping<ClientesPrecios>();
                Db.DeleteAll(tabla);

                Db.InsertAll(clienteWS, true);
                Db.InsertAll(camposWS, true);
                Db.InsertAll(correosClientesWS, true);
                Db.InsertAll(repartidoresWS, true);
                Db.InsertAll(productosWS, true);
                Db.InsertAll(presentacionesWS, true);
                Db.InsertAll(ordenespedidosWS, true);
                Db.InsertAll(ordenespedidosproductosWS, true);
                Db.InsertAll(clientespreciosWS, true);

                tabla = Db.GetMapping<Usuario>();
                object[] DBSyncData = { true, "u5U4r10+2236" };
                var ejecutarDBSync = Db.Query(tabla, "UPDATE Usuario SET BDSincronizada=? WHERE IdString=?", DBSyncData);
                Db.Commit();
                return "true";
            }
            catch (Exception err)
            {
                Db.Rollback();
                return err.ToString();
            }
        }

        // --------------- DESACTIVAR DISPOSITIVO ---------------
        public string DesactivarSQliteWS()
        {
            try
            {
                Db.BeginTransaction();
                TableMapping tabla = Db.GetMapping<Usuario>();
                Db.DeleteAll(tabla);
                tabla = Db.GetMapping<BDPendientes>();
                Db.DeleteAll(tabla);
                tabla = Db.GetMapping<Clientes>();
                Db.DeleteAll(tabla);
                tabla = Db.GetMapping<Campos>();
                Db.DeleteAll(tabla);
                tabla = Db.GetMapping<CorreosClientes>();
                Db.DeleteAll(tabla);
                tabla = Db.GetMapping<Productos>();
                Db.DeleteAll(tabla);
                tabla = Db.GetMapping<Presentaciones>();
                Db.DeleteAll(tabla);
                tabla = Db.GetMapping<OrdenesPedido>();
                Db.DeleteAll(tabla);
                tabla = Db.GetMapping<OrdenesPedidoProductos>();
                Db.DeleteAll(tabla);
                tabla = Db.GetMapping<OrdenesPedidoPendientes>();
                Db.DeleteAll(tabla);
                tabla = Db.GetMapping<ClientesPrecios>();
                Db.DeleteAll(tabla);
                tabla = Db.GetMapping<Cotizaciones>();
                Db.DeleteAll(tabla);
                tabla = Db.GetMapping<CotizacionProductos>();
                Db.DeleteAll(tabla);

                Db.Commit();
                return "true";
            }
            catch (Exception err)
            {
                Db.Rollback();
                return err.ToString();
            }
        }

        // -------------- CONSULTAS--------------

        // **************** [ PARAMETROS ESPECIALES ] ****************
        // PARAMETROS DE APPCONFIGURACION
        public string ObtenerNotificacionID()
        {
            string NotifID = "";
            var VerifNotifID = Db.Query<AppConfiguracion>("SELECT * FROM AppConfiguracion");
            foreach (var Notif in VerifNotifID)
            {
                NotifID = Notif.NotificacionID;
            }
            return NotifID;
        }

        // TIPO DE USUARIO
        public string ObtenerTipoUsuario()
        {
            string TipoUsuario = "";
            var VerifTipoUsuario = Db.Query<Usuario>("SELECT * FROM Usuario WHERE IdString = 'u5U4r10+2236'");
            foreach (var Tipo in VerifTipoUsuario)
            {
                TipoUsuario = Tipo.TipoUsuario;
            }
            return TipoUsuario;
        }

        // FOLIO DE ORDEN PEDIDO
        public string ObtenerFolioOrdenPedido()
        {
            int folio = 0;
            var ContarOrdenesPedido = Db.Query<OrdenesPedido>("SELECT MAX(Id) AS Id FROM OrdenesPedido");
            foreach (var orden in ContarOrdenesPedido)
            {
                folio = orden.Id;
            }
            if (folio == 0)
            {
                folio = 1;
            }
            else
            {
                folio++;
            }

            string folioTxt = "";
            if (folio < 10)
            {
                folioTxt = "00";
            }
            else if (folio < 100)
            {
                folioTxt = "0";
            }

            return folioTxt + folio.ToString();
        }
        // FUNCION QUE DEVUELVE EL ID DE USUARIO (AGENTE) EN REPARTIDORES
        public int ObtenerIDUsuarioReparto()
        {
            string usuarioparam = "";
            var VerifTipoUsuario = Db.Query<Usuario>("SELECT * FROM Usuario WHERE IdString = 'u5U4r10+2236'");
            foreach (var Tipo in VerifTipoUsuario)
            {
                usuarioparam = Tipo.UsuarioNombre;
            }
            int IdUsuario = 0;
            bool verif = int.TryParse(usuarioparam.Substring(0, usuarioparam.Length - 1).Substring(0, usuarioparam.Length - 2).Substring(0, usuarioparam.Length - 3).Replace("reparto", ""), out IdUsuario);
            return IdUsuario;
        }
        // ***********************************************************

        // ******** USUARIO
        public List<Usuario> ConsultaUsuario(string consulta)
        {
            return Db.Query<Usuario>(consulta);
        }

        // ******** BD SERVIDOR
        public List<BDPendientes> ConsultaBDPendientes(string consulta)
        {
            return Db.Query<BDPendientes>(consulta);
        }

        // ********* CLIENTES
        public List<Clientes> ConsultaClientes(string consulta)
        {
            return Db.Query<Clientes>(consulta);
        }
        // ******** CORREOS
        public List<CorreosClientes> ConsultaCorreos(string consulta)
        {
            return Db.Query<CorreosClientes>(consulta);
        }
        // ******** CAMPOS
        public List<Campos> ConsultaCampos(string consulta)
        {
            return Db.Query<Campos>(consulta);
        }
        // ******** REPARTIDORES
        public List<Repartidores> ConsultaRepartidores(string consulta)
        {
            return Db.Query<Repartidores>(consulta);
        }
        // ******** PRODUCTOS
        public List<Productos> ConsultaProductos(string consulta)
        {
            return Db.Query<Productos>(consulta);
        }
        // ******** PRESENTACIONES
        public List<Presentaciones> ConsultaPresentaciones(string consulta)
        {
            return Db.Query<Presentaciones>(consulta);
        }
        // CLIENTES PRECIOS
        public List<ClientesPrecios> ConsultaClientesPrecios(string consulta)
        {
            return Db.Query<ClientesPrecios>(consulta);
        }
        // ORDENES PEDIDOS
        public List<OrdenesPedido> ConsultaOrdenesPedido(string consulta)
        {
            return Db.Query<OrdenesPedido>(consulta);
        }
        // ORDENES PEDIDOS PRODUCTOS
        public List<OrdenesPedidoProductos> ConsultaOrdenesPedidoProductos(string consulta)
        {
            return Db.Query<OrdenesPedidoProductos>(consulta);
        }
        // COTIZACIONES
        public List<Cotizaciones> ConsultaCotizacion(string consulta) {
            return Db.Query<Cotizaciones>(consulta);
        }
        // COTIZACIONES PRODUCTOS
        public List<CotizacionProductos> ConsultaCotizacionProductos(string consulta) {
            return Db.Query<CotizacionProductos>(consulta);
        }
        // ORDENES PEDIDO PENDIENTES
        public List<OrdenesPedidoPendientes> ConsultaOrdenPedidoPendiente(string consulta)
        {
            return Db.Query<OrdenesPedidoPendientes>(consulta);
        }

        // ***********************************************
        // -------------- QUERYS ESPECIALES --------------
        // ORDENES PEDIDOS
        public string GuardarOrdenesPedido(int accion, List<OrdenesPedido> opdata, List<OrdenesPedidoProductos> opproductodata)
        {
            try
            {
                Db.BeginTransaction();
                if (accion == 1)
                {
                    Db.InsertAll(opdata, true);
                    Db.InsertAll(opproductodata, true);
                }
                else if (accion == 2)
                {
                    int IdOP = 0;
                    foreach(var OP in opdata)
                    {
                        IdOP = OP.Id;
                    }
                    TableMapping mapa = Db.GetMapping<OrdenesPedidoProductos>();
                    Db.Query(mapa, "DELETE FROM OrdenesPedidoProductos WHERE IdOrdenPedido = ?", new object[] { IdOP });
                    Db.UpdateAll(opdata, true);
                    Db.InsertAll(opproductodata, true);
                }
                Db.Commit();
                return "true";
            }
            catch (Exception err)
            {
                Db.Rollback();
                return err.ToString();
            }
        }

        // ORDENES PEDIDO PENDIENTES
        public string GuardarOrdenesPedidoPendientes(int accion, List<OrdenesPedidoPendientes> opdatapendiente)
        {
            try
            {
                Db.BeginTransaction();
                if (accion == 1)
                {
                    Db.InsertAll(opdatapendiente, true);
                }
                Db.Commit();
                return "true";
            }
            catch (Exception err)
            {
                Db.Rollback();
                return err.ToString();
            }
        }

        // COTIZACIONES
        public string GuardarCotizacion(int accion, List<Cotizaciones> cotizaciondata, List<CotizacionProductos> cotizacionproductosdata)
        {
            try
            {
                Db.BeginTransaction();
                if (accion == 1)
                {
                    Db.InsertAll(cotizaciondata, true);
                    Db.InsertAll(cotizacionproductosdata, true);
                }
                Db.Commit();
                return "true";
            }
            catch (Exception err)
            {
                Db.Rollback();
                return err.ToString();
            }
        }

        // CLIENTES PEDIDOS
        public string GuardarClientePrecio(List<ClientesPrecios> clientespreciosdata)
        {
            try
            {
                Db.BeginTransaction();
                Db.InsertAll(clientespreciosdata, true);
                Db.Commit();
                return "true";
            }
            catch (Exception err)
            {
                Db.Rollback();
                return err.ToString();
            }
        }

        // --------- MULTI QUERY PARA USOS DE INSERT, UPDATE Y DELETE ---------
        public string QueryMaestra(int tabla, string consulta, object[] parametros)
        {
            TableMapping mapa = Db.GetMapping<BDPendientes>();
            if (tabla == 1)
            {
                mapa = Db.GetMapping<Usuario>();
            }
            else if (tabla == 2)
            {
                mapa = Db.GetMapping<Clientes>();
            }
            else if (tabla == 3)
            {
                mapa = Db.GetMapping<Campos>();
            }
            else if (tabla == 4)
            {
                mapa = Db.GetMapping<CorreosClientes>();
            }
            else if (tabla == 5)
            {
                mapa = Db.GetMapping<Repartidores>();
            }
            else if (tabla == 6)
            {
                mapa = Db.GetMapping<Productos>();
            }
            else if (tabla == 7)
            {
                mapa = Db.GetMapping<Presentaciones>();
            }
            else if (tabla == 8)
            {
                mapa = Db.GetMapping<OrdenesPedido>();
            }
            else if (tabla == 9)
            {
                mapa = Db.GetMapping<OrdenesPedidoProductos>();
            }
            else if (tabla == 10)
            {
                mapa = Db.GetMapping<ClientesPrecios>();
            }
            else if (tabla == 11)
            {
                mapa = Db.GetMapping<Cotizaciones>();
            }
            else if (tabla == 12)
            {
                mapa = Db.GetMapping<CotizacionProductos>();
            }
            else if (tabla == 13)
            {
                mapa = Db.GetMapping<OrdenesPedidoPendientes>();
            }
            try
            {
                Db.BeginTransaction();
                var ejecutar = Db.Query(mapa, consulta, parametros);
                Db.Commit();
                return "true";
            }
            catch (Exception e)
            {
                Db.Rollback();
                return e.ToString();
            }

        }
    }
}