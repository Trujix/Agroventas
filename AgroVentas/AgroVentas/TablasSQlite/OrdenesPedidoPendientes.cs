using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace AgroVentas.TablasSQlite
{
    [Table("OrdenesPedidoPendientes")]
    public class OrdenesPedidoPendientes
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string PedidoCad { get; set; }
        public string IdOrdenPedido { get; set; }
        public DateTime FechaCreado { get; set; }
    }
}
