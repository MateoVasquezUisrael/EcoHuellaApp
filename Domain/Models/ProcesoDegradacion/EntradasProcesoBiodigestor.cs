using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using SQLiteNetExtensions.Attributes;


namespace EcoHuellaApp.Domain.Models.ProcesoDegradacion
{
    public class EntradasProcesoBiodigestor
    {
        [PrimaryKey, AutoIncrement, Indexed]
        public int Id { get; set; }
        [NotNull]
        public DateTime FechaIngreso { get; set; }
        [NotNull]
        public bool Estado { get; set; } = true;


        [ForeignKey(typeof(ProcesoBiodigestor))]
        public int ProcesoBiodigestorId { get; set; }


        [ManyToOne(foreignKey:nameof(ProcesoBiodigestorId), inverseProperty: nameof(ProcesoBiodigestor.Entradas))]
        public ProcesoBiodigestor? Proceso { get; set; }
    }
}
