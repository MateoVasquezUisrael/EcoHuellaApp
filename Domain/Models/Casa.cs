using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace EcoHuellaApp.Models
{
    public class Casa
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string NombreResponsable { get; set; }

        public string Direccion { get; set; }

        public string Sector { get; set; }

        public bool Activa { get; set; }
    }
}
