using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace AgroVentas.TablasSQlite
{
    [Table("Productos")]
    public class Productos
    {
        [PrimaryKey]
        public int Id { get; set; }
        public string NombreProducto { get; set; }
        public int Estatus { get; set; }
    }
}
