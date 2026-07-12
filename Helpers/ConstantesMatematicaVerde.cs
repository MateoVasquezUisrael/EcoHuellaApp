using System;
using System.Collections.Generic;
using System.Text;

namespace EcoHuellaApp.Helpers;
public static class ConstantesMatematicaVerde
{
    //--------------- necesario para el cálculo de masa de residuos orgánicos ----------------------------------
    public const double DensidadResiduosOrganicos = 0.30; //contenidos presurtizados de basura orgánica necesarios en el cálculo de masa; se mide en kg/L.
    public const int VolumenBaldes = 20; //se mide el litros

    //--------------- necesario para el cálculo de metano en vertedero y en compost en base al IPCC
    public const double CarbonoOrganicoDegradable = 0.15;
    public const double CarbonoOrganicoDegradableFraccionario = 0.5;
    public const double FraccionMetanoEnBiogas = 0.5;
    public const double ConstanteConversionCarbonoMetano = 1.333; //constante de conversión de Caarbono a Metano
    public const double FactorCorreccionVertedero = 1.0;
    public const double ConstanteFactorEmisionMetano = 0.004; //equivalente a 4g de Metano por Kg en base a https://www.ipcc-nggip.iges.or.jp/public/2006gl/pdf/5_Volume5/V5_4_Ch4_Bio_Treat.pdf
    //-------------- necesario para el cálculo 
    public const double PotencialCalentamientoMetano = 27.9; //constante usada para convertir metano en CO2 de la IPCC.
}
