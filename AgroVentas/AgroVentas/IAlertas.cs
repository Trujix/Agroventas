using System;
using System.Collections.Generic;
using System.Text;

namespace AgroVentas
{
    // INTERFACE - PARA LLAMAR TOASTS GENERICOS DE ANDROID PARA XAMARIN
    public interface IAlertas
    {
        // MUESTRA UN ALERT CORTO
        void MsgCorto(string msg);
        // MUESTRA UN ALERT LARGO
        void MsgLargo(string msg);
        // CREA UNA CADENA ALEATORIA (MULTIUSOS)
        string crearTokenAleatorio();
        // DEVUELDE FORMATOS DE FECHAESPECIAL TIPO GETTIME() DE JAVASCRIPT EN MILISEGUNDOS
        Int64 FechaEspecial(int[] fechapers);
    }
}