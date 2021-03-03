using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace AgroVentas.TablasSQlite
{
    [Table("BDPendientes")]
    public class BDPendientes
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Data { get; set; }
        public string Accion { get; set; }
        public string Tabla { get; set; }
    }
}
