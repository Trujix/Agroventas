using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace AgroVentas.TablasSQlite
{
    [Table("ClientesPrecios")]
    public class ClientesPrecios
    {
        [PrimaryKey]
        public int Id { get; set; }
        public int IdOrdenPedido { get; set; }
        public int IdCliente { get; set; }
        public int IdProducto { get; set; }
        public string Presentacion { get; set; }
        public double Precio { get; set; }
        public double PrecioOriginal { get; set; }
        public string Impuestos { get; set; }
        public int Descuento { get; set; }
        public double FechaVenta { get; set; }
        public DateTime FechaCreado { get; set; }
    }
}