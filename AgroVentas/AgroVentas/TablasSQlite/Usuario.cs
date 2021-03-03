using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace AgroVentas.TablasSQlite
{
    [Table("Usuario")]
    public class Usuario
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string IdString { get; set; }
        public int IdUsuario { get; set; }
        public string UsuarioNombre { get; set; }
        public string SecurityID { get; set; }
        public string SecurityRestablecerID { get; set; }
        public string Correo { get; set; }
        public string Nombre { get; set; }
        public bool Logeado { get; set; }
        public bool BDSincronizada { get; set; }
        public bool SinAutomatico { get; set; }
        public string TipoUsuario { get; set; }
        public double DollarCredito { get; set; }
        public double DollarContado { get; set; }
    }
}