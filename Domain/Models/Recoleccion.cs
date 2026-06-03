using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace EcoHuellaApp.Domain.Models
{
    public class Recoleccion
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public DateTime Fecha { get; set; }

        public int CantidadCubetas { get; set; }
        [AllowNull]
        public string? CoordenadasGPS { get; set; }
    }
}
