using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcoHuellaApp.Domain.Models.ProcesoComposteraArtesanal
{
    public class ComposteraArtesanal
    {
        [PrimaryKey, Indexed, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public string UsuarioUid { get; set; } = string.Empty;

        public bool Estado { get; set; } //true es activo, false es inactivo

        public double? PesoMaximo { get; set; }

        [OneToMany]
        public List<AccionCompostera>? Acciones { get; set; }
    }
}
