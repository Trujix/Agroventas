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
using Xamarin.Forms;

[assembly: Dependency(typeof(AgroVentas.Droid.Alertas))]
namespace AgroVentas.Droid
{
    public class Alertas : IAlertas
    {
        // MENSAJE CORTO
        public void MsgCorto(string msg)
        {
            Toast.MakeText(Android.App.Application.Context, msg, ToastLength.Short).Show();
        }
        // MENSAJE LARGO
        public void MsgLargo(string msg)
        {
            Toast.MakeText(Android.App.Application.Context, msg, ToastLength.Long).Show();
        }
        // CREAR UNA CADENA ALEATORIA
        public string crearTokenAleatorio()
        {
            string token = ""; Int64 cadFecha = 0; string cadFechaTXT = ""; Random num = new Random();
            var f = new DateTime(1970, 1, 1);
            TimeSpan t = (DateTime.Now.ToUniversalTime() - f);
            cadFecha = (Int64)(t.TotalMilliseconds + 0.5);
            cadFechaTXT = cadFecha.ToString();
            foreach (char c in cadFechaTXT)
            {
                int l = num.Next(0, 26);
                char let = (char)('a' + l);
                token += c + "" + let;
            }
            return token;
        }

        // DEVUELDE FORMATOS DE FECHAESPECIAL TIPO GETTIME() DE JAVASCRIPT EN MILISEGUNDOS
        public Int64 FechaEspecial(int[] fechapers)
        {
            Int64 Milisegundos = 0;
            DateTime Fecha = new DateTime(2019, 1, 1);
            if (fechapers.Length > 0)
            {
                DateTime FechaPers = new DateTime(fechapers[0], fechapers[1], fechapers[2]);
                TimeSpan Tiempo = (FechaPers - Fecha);
                Milisegundos = (Int64)(Tiempo.TotalMilliseconds + 0.5);
                return Milisegundos;
            }
            else
            {
                TimeSpan Tiempo = (DateTime.Now.ToUniversalTime() - Fecha);
                Milisegundos = (Int64)(Tiempo.TotalMilliseconds + 0.5);
                return Milisegundos;
            }
        }
    }
}