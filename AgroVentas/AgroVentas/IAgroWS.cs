using System;
using System.Collections.Generic;
using System.Text;

namespace AgroVentas
{
    public interface IAgroWS
    {
        // :::::::::::::::::::: [ LOGIN ] ::::::::::::::::::::
        string Login(string logininfo);

        // :::::::::::::::::::: [ ACCIONES DE DATOS ] ::::::::::::::::::::
        // ----- ENVIO DATA APP -> WS
        string EnviarDataWS(string dataws);
        // ----- RECIBIR DATA WS -> APP
        string RecibirDataWS();
        string ObtenerOrdenesPedido();


        // ::::::::::::::::::::::::: [ SINCRONIZAR WEB SERVICE ] :::::::::::::::::::::::::
        void SincronizarWebService();


        // :::::::::::::::::::: [ RECUPERACION DE CONTRASEÑA ] ::::::::::::::::::::
        string RecuperarPassword(string usuarioparams);


        // :::::::::::::::::::: [ DOCUMENTOS PDF ] ::::::::::::::::::::
        string GenerarFichaCliente(string clienteinfo, string correos, int idusuario);
        string GenerarOrdenPedido(string pedidoinfo);
        string ObtenerOrdenPedidoDoc(int idusuario, string tipodocumento, string folioadicional);
        string GenerarCotizacion(string cotizaciondata);
        string ObtenerManualUrl();


        // :::::::::::::::::::: [ ACCIONES CON ORDENES DE PEDIDOS ] ::::::::::::::::::::
        string AsignarOPRepartidor(int idusuario, int idordenpedido, string repartidor);
        string CambiarEstatusOP(int idusuario, int idordenpedido, int estatus);
        int ObtenerIdRepartidorOP(int idordenpedido);

        // :::::::::::::::::::: [ REPORTES ONLINE ] ::::::::::::::::::::
        string ReportesOnline(string dataestructura, int reporte);

        // :::::::::::::::::::: [ AUXILIARES ] ::::::::::::::::::::
        bool VerificarInternetConeccion();
    }
}