using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace AgroVentas.TablasSQlite
{
    [Table("CorreosClientes")]
    public class CorreosClientes
    {
        public int IdCliente { get; set; }
        public string Correo { get; set; }
    }
}