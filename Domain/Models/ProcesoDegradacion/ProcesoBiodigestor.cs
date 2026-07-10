using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;

using System.Diagnostics.CodeAnalysis;
using System.Text;


namespace EcoHuellaApp.Domain.Models.ProcesoDegradacion
{
    public class ProcesoBiodigestor
    {
        [PrimaryKey, AutoIncrement, Indexed]
        public int Id { get; set; }
        public DateTime? FechaInicio { get; set; }
        [AllowNull]
        public DateTime? FechaCierre { get; set; }
        [AllowNull]
        public double MetanoEvitado { get; set; } // estos valores se calculan si y solo si se acabó un proceso
        [AllowNull]
        public double CarbonoEvitado { get; set; } // lo mismo de arriba
        public bool Estado { get; set; } = true; //true es iniciado/en proceso y false es finalizado
        [ForeignKey(typeof(Biodigestor))]
        public int BiodigestorId { get; set; }
        [ManyToOne]
        public Biodigestor? Biodigestor
        {
            get; set;
        }

        [OneToMany(inverseForeignKey: nameof(EntradasProcesoBiodigestor.ProcesoBiodigestorId),
            inverseProperty: nameof(EntradasProcesoBiodigestor.Proceso))]
        public List<EntradasProcesoBiodigestor>? Entradas {  get; set; }
    }
}
