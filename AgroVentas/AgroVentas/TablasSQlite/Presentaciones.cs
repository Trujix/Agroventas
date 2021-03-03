using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace AgroVentas.TablasSQlite
{
    [Table("Presentaciones")]
    public class Presentaciones
    {
        public int IdProducto { get; set; }
        public string NombrePresentacion { get; set; }
        public double Precio { get; set; }
        public int IVA { get; set; }
        public int IEPS { get; set; }
        public int Moneda { get; set; }
    }
}