using SQLite;
using SQLiteNetExtensions;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcoHuellaApp.Domain.Models
{
    public class PuntoRecoleccion
    {
        //punto de recolección, la unión  de este y Casa dan una Recolección
        //TODO: se necesita conseguir la latitud y longitud de la dirección
        //el estado de esta entidad es true = activo, false = inactivo
        [PrimaryKey, AutoIncrement]
        public int Id {  get; set; }
        [NotNull]
        public string Direccion { get; set; }
        public double Latitud {  get; set; }
        public double Longitud { get; set; }
        public bool Estado { get; set; }

        [OneToMany]
        public List<Recoleccion>? Recolecciones { get; set; } 

    }
}
