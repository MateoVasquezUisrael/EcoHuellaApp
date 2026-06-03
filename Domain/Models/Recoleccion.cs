using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcoHuellaApp.Domain.Models
{
    public class Recoleccion
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public DateTime Fecha { get; set; }

        public int Cantidad { get; set; }
    }
}
