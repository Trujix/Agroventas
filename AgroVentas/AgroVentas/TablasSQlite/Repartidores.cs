using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace AgroVentas.TablasSQlite
{
    [Table("Repartidores")]
    public class Repartidores
    {
        [PrimaryKey]
        public int Id { get; set; }
        public string NombreRepartidor { get; set; }
        public string Correo { get; set; }
        public int Estatus { get; set; }
    }
}