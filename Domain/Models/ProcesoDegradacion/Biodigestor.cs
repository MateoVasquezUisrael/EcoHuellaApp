using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcoHuellaApp.Domain.Models.ProcesoDegradacion
{
    public class Biodigestor
    {
        [PrimaryKey, AutoIncrement, Indexed]
        public int Id { get; set; }
        public double CapacidadMaxima {  get; set; } //la señora Lorena dice que siempre tienden a ser 400Kg
        public bool Estado {  get; set; } //1 es activo, 0 es inactivo

        [OneToMany]
        public List<ProcesoBiodigestor>? Procesos { get; set; }
    }
}
