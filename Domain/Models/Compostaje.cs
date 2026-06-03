using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcoHuellaApp.Domain.Models
{
    public class Compostaje
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public DateTime FechaIngreso { get; set; }

        public DateTime? FechaCosecha { get; set; }

        public double PesoKg { get; set; }

        public double? PesoKgFinal { get; set;}

        public string Observaciones { get; set; }

        public bool Estado { get; set; }
    }
}
