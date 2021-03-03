using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace AgroVentas.TablasSQlite
{
    [Table("AppConfiguracion")]
    public class AppConfiguracion
    {
        public string NotificacionID { get; set; }
    }
}