 using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using SQLiteNetExtensions;
using SQLiteNetExtensions.Attributes;


namespace EcoHuellaApp.Domain.Models.Recoleccion
{
    public class Recoleccion
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [NotNull]
        public DateTime? Fecha { get; set; }

        public int CantidadCubetas {  get; set; }
        public double LitrosEstimados { get; set; }
        public double MasaEstimada { get; set; }
        public bool Estado{  get; set; }
        [ForeignKey(typeof(Casa))]
        public int CasaId {  get; set; }
        [ManyToOne]
        public Casa Casa { get; set; }
        [ForeignKey(typeof(PuntoRecoleccion))]
        public int PuntoRecoleccionId {  get; set; }
        [ManyToOne]
        public PuntoRecoleccion PuntoRecoleccion { get; set; }
    }
}
