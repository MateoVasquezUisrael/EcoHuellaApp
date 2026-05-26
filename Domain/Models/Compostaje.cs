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

        public DateTime? FechaSalida { get; set; }

        public double PesoInicialKg { get; set; }

        public double? PesoFinalKg { get; set; }

        public string Biodigestor { get; set; }

        public bool Finalizado { get; set; }
    }
}
