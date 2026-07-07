using System;
using System.Collections.Generic;
using System.Text;
using EcoHuellaApp.Domain.Models.Recoleccion;
using SQLite;
using SQLiteNetExtensions;
using SQLiteNetExtensions.Attributes;



namespace EcoHuellaApp.Domain.Models.Recoleccion
{
//Entidad asociada al proceso de recolección; se llama casa, pero puede ser más grande como una urbanización
//el estado de esta entidad es true = activo, false = inactivo
    public class Casa
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [NotNull]
        public string NombreResponsable { get; set; }
        [NotNull]
        public string Direccion { get; set; }

        public string? Sector { get; set; }

        public bool Estado { get; set; }

        public double Longitud { get; set; }

        public double Latitud { get; set; }
        
        [OneToMany]
        public List<Recoleccion>? Recolecciones
        {
            get; set;
        }
    }
}
