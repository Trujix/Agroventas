using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace AgroVentas.TablasSQlite
{
    [Table("Campos")]
    public class Campos
    {
        public int IdCliente { get; set; }
        public string NombreCampo { get; set; }
        public string Ubicacion { get; set; }
    }
}