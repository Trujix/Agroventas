using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace AgroVentas.TablasSQlite
{
    [Table("Clientes")]
    public class Clientes
    {
        [PrimaryKey]
        public int Id { get; set; }
        public string RazonSocial { get; set; }
        public string RFC { get; set; }
        public string Calle { get; set; }
        public string NumInt { get; set; }
        public string NumExt { get; set; }
        public string Colonia { get; set; }
        public string Localidad { get; set; }
        public string Municipio { get; set; }
        public string Estado { get; set; }
        public string Pais { get; set; }
        public int CP { get; set; }
        public string NombreContacto { get; set; }
        public double Telefono { get; set; }
        public string UsoCFDI { get; set; }
        public string MetodoPago { get; set; }
        public string FormaPago { get; set; }
        public int DiasCredito { get; set; }
        public double LineaCredito { get; set; }
        public int Estatus { get; set; }
    }
}