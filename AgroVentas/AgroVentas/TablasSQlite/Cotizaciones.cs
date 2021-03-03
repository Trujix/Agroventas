using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace AgroVentas.TablasSQlite
{
    [Table("Cotizaciones")]
    public class Cotizaciones
    {
        [PrimaryKey]
        public int Id { get; set; }
        public string TipoCliente { get; set; }
        public int IdCliente { get; set; }
        public string Cliente { get; set; }
        public string CantidadArticulos { get; set; }
        public string Subtotal { get; set; }
        public string Descuentps { get; set; }
        public string IEPS { get; set; }
        public string IVA { get; set; }
        public string Total { get; set; }
        public DateTime FechaCreada { get; set; }
    }
}