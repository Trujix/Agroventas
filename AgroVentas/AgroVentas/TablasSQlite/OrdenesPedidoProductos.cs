using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace AgroVentas.TablasSQlite
{
    [Table("OrdenesPedidoProductos")]
    public class OrdenesPedidoProductos
    {
        public int IdOrdenPedido { get; set; }
        public int IdProducto { get; set; }
        public string Descripcion { get; set; }
        public string Presentacion { get; set; }
        public float Cantidad { get; set; }
        public double PrecioUnitario { get; set; }
        public double PrecioUnitarioDesc { get; set; }
        public int DescuentoPorc { get; set; }
        public double Descuento { get; set; }
        public int IVA { get; set; }
        public double IVAMonto { get; set; }
        public int IEPS { get; set; }
        public double IEPSMonto { get; set; }
        public double Importe { get; set; }
    }
}