using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;

using System.Text;

namespace EcoHuellaApp.Domain.Models.ProcesoComposteraArtesanal
{
    /// <summary>
    /// Se añaden tipos de datos locales solo necesario aquí. Referente a si hubo ingreso o salida de compuestos. Aunqu el LIXIVIADO SOLO SE PUEDE SACAR.
    /// </summary>
    public class Acciones
    {
        public const string INSERTAR = "Insertar";
        public const string EXTRAER = "Extraer";
    }

    public class Elementos
    {
        public const string LIXIVIADO = "Lixiviado";
        public const string COMPOST = "Compost";
        public const string FORRAJEVERDE = "Forraje Verde";
    }

    public class AccionCompostera
    {
        [PrimaryKey, Indexed, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public string UsuarioUid { get; set; } = string.Empty;

        public string? TipoAccion { get; set; }

        public DateTime? FechaAccion { get; set;  }

        public string? TipoElemento { get; set; }

        [ForeignKey(typeof(ComposteraArtesanal))]
        public int ComposteraArtesanalId { get; set; }

        [ManyToOne]
        public ComposteraArtesanal? Compostera {  get; set; }
    }
}
