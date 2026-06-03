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

        public DateTime? Fecha { get; set; }

        public double Latitud { get; set; }

        public double Longitud { get; set; }

        public bool Estado{  get; set; }
    }
}
