using Mapsui.Providers.Wfs.Utilities;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcoHuellaApp.Domain.Models.Ventas
{
    public class MotivosSaco
    {
        public const string VENTA = "Venta";

        public const string CONSUMO = "Consumo";
    }

    public class SacosCompost
    {
        [PrimaryKey, Indexed, AutoIncrement]
        public int Id { get; set; }
        public DateTime? Fecha { get; set; } // fecha de la acciòn del uso o venta del saco

        public bool Estado { get; set; } //true es que está guardado y false que ya fue vendido o usado.
        public string? Motivo { get; set; } //Si el saco fue usado o vendido
        public string? ClienteVenta { get; set; } // puede insertarse solo si el motivo es venta
    }
}
