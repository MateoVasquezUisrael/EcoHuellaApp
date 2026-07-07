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

        public bool Estado {  get; set; } //1 es iniciado/en proceso y 0 es finalizado
        [ForeignKey(typeof(Biodigestor))]
        public int BiodigestorId { get; set; }
        [ManyToOne]
        public Biodigestor Biodigestor
        {
            get; set;
        }

        [OneToMany]
        public List<EntradasProcesoBiodigestor>? Entradas {  get; set; }
    }
}
