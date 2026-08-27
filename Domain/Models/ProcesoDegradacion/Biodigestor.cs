using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EcoHuellaApp.Domain.Models.ProcesoDegradacion
{
    public class Biodigestor
    {
        [PrimaryKey, AutoIncrement, Indexed]
        public int Id { get; set; }
        [Indexed]
        public string UsuarioUid { get; set; } = string.Empty;
        public double CapacidadMaxima {  get; set; } //la señora Lorena dice que siempre tienden a ser 400Kg
        public bool Estado {  get; set; } //true es activo, false es inactivo

        [OneToMany(inverseForeignKey: nameof(ProcesoBiodigestor.BiodigestorId), inverseProperty: nameof(ProcesoBiodigestor.Biodigestor))]
        public List<ProcesoBiodigestor>? Procesos { get; set; }
    }
}
