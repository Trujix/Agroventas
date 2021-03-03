using System;
using System.Collections.Generic;
using System.Text;
using AgroVentas.TablasSQlite;

namespace AgroVentas
{
    public interface ISQliteParams
    {
        // INICIAR SQLITE DB
        void IniciarSQlite();


        // -------- LIMPIAR SQLITE TABLAS --------
        string RestaurarSQliteWS(string dataws);

        // --------------- DESACTIVAR DISPOSITIVO ---------------
        string DesactivarSQliteWS();


        // --------- CONSULTAS ---------
        // **************** [ PARAMETROS ESPECIALES ] ****************
        // PARAMETROS DE APPCONFIGURACION
        string ObtenerNotificacionID();
        // TIPO DE USUARIO
        string ObtenerTipoUsuario();
        // FOLIO DE ORDEN PEDIDO
        string ObtenerFolioOrdenPedido();
        // ID USUARIO (AGENTE) EN REPARTIDOR
        int ObtenerIDUsuarioReparto();
        // ***********************************************************

        // USUARIOS
        List<Usuario> ConsultaUsuario(string consulta);
        // BD SERVIDOR
        List<BDPendientes> ConsultaBDPendientes(string consulta);
        // CLIENTES
        List<Clientes> ConsultaClientes(string consulta);
        // CORREOS
        List<CorreosClientes> ConsultaCorreos(string consulta);
        // CAMPOS
        List<Campos> ConsultaCampos(string consulta);
        // REPARTIDORES
        List<Repartidores> ConsultaRepartidores(string consulta);
        // PRODUCTOS
        List<Productos> ConsultaProductos(string consulta);
        // PRESENTACIONES
        List<Presentaciones> ConsultaPresentaciones(string consulta);
        // CLIENTES PRECIOS
        List<ClientesPrecios> ConsultaClientesPrecios(string consulta);
        // ORDENES PEDIDOS
        List<OrdenesPedido> ConsultaOrdenesPedido(string consulta);
        // ORDENES PEDIDOS PRODUCTOS
        List<OrdenesPedidoProductos> ConsultaOrdenesPedidoProductos(string consulta);
        // COTIZACIONES
        List<Cotizaciones> ConsultaCotizacion(string consulta);
        // COTIZACIONES PRODUCTOS
        List<CotizacionProductos> ConsultaCotizacionProductos(string consulta);
        // ORDENES PEDIDO PENDIENTES
        List<OrdenesPedidoPendientes> ConsultaOrdenPedidoPendiente(string consulta);


        // ***********************************************
        // -------------- QUERYS ESPECIALES --------------
        // ORDENES PEDIDOS
        /// <param name="accion">[1 - NUEVA] [2 - EDITAR]</param>
        string GuardarOrdenesPedido(int accion, List<OrdenesPedido> opdata, List<OrdenesPedidoProductos> opproductodata);
        // ORDENES PEDIDO PENDIENTES
        string GuardarOrdenesPedidoPendientes(int accion, List<OrdenesPedidoPendientes> opdatapendiente);
        // COTIZACIONES
        /// <param name="accion">[1 - NUEVA] [2 - EDITAR]</param>
        string GuardarCotizacion(int accion, List<Cotizaciones> cotizaciondata, List<CotizacionProductos> cotizacionproductosdata);

        // --------- MULTI QUERY PARA USOS DE INSERT, UPDATE Y DELETE ---------
        /// <param name="tabla">[0 - BDSERVIDOR] [1 - USUARIO] [2 - CLIENTES] [3 - CAMPOS] [4 - CORREOS] [5 - REPARTIDORES] [6 - PRODUCTOS] [7 - PRESENTACIONES] [8 - ORDEN PEDIDO] [9 - ORDEN PEDIDO PRODUCTOS] [10 - CLIENTES PRECIOS] [11 - COTIZACION] [12 - COTIZACION PRODUCTOS] [13 - ORDENES PEDIDO PENDIENTES]</param>
        string QueryMaestra(int tabla, string consulta, object[] parametros);
    }
}