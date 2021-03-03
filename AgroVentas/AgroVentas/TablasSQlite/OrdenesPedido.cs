using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace AgroVentas.TablasSQlite
{
    [Table("OrdenesPedido")]
    public class OrdenesPedido
    {
        [PrimaryKey]
        public int Id { get; set; }
        public int NumeroOrden { get; set; }
        public int IdTipoDocumento { get; set; }
        public string TipoDocumento { get; set; }
        public int IdTipoCliente { get; set; }
        public string TipoCliente { get; set; }
        public int IdCliente { get; set; }
        public string Cliente { get; set; }
        public string Campo { get; set; }
        public string Ubicacion { get; set; }
        public double Total { get; set; }
        public int Estatus { get; set; }
        public DateTime FechaEntrega { get; set; }
    }
}