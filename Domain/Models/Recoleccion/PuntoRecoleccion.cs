using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using SQLiteNetExtensions;
using SQLiteNetExtensions.Attributes;


namespace EcoHuellaApp.Domain.Models.Recoleccion
{
    public class PuntoRecoleccion
    {
        //punto de recolección, la unión  de este y Casa dan una Recolección
        //la latitud y longitud se obtienen mediante el selector de mapa offline
        //el estado de esta entidad es true = activo, false = inactivo
        [PrimaryKey, AutoIncrement]
        public int Id {  get; set; }
        [NotNull]
        public string Direccion { get; set; }
        public double Latitud {  get; set; }
        public double Longitud { get; set; }
        public bool Estado { get; set; } //true es activo, false es inactivo

        [OneToMany]
        public List<Recoleccion>? Recolecciones { get; set; } 

    }
}
