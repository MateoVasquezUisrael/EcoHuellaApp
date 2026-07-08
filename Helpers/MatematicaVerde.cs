using System;
using System.Collections.Generic;
using System.Text;
using EcoHuellaApp.Helpers;
using KotlinX.Android.Extensions;

namespace EcoHuellaApp.Helpers
{
    public class MatematicaVerde
    {
        public double CalcularMasa(int cantidadesBaldes)
        {

            /// <summary>
            /// La ecuación nos devuelve la masa de residuos orgánicos asentados. Se hace multiplicando la cantidad de baldes de una recolección por el volumen cosntante usado
            /// por Aldea Las Nubes por la densidad calculada dada por la Secrertaria de AMbiente de Quito.
            /// </summary>
            double calculo = cantidadesBaldes * ConstantesMatematicaVerde.VolumenBaldes * ConstantesMatematicaVerde.DensidadResiduosOrganicos;

            return calculo;
        }

        public double CalcularMetanoEnVertadero(double MasaCarbono)
        {
            ///<summary>
            ///Método simplificado para calcular el potencial de generación de metano de la IPCC
            ///en https://www.ipcc-nggip.iges.or.jp/public/gp/spanish/5_Waste_ES.pdf
            /// </summary>
            double calculo = MasaCarbono * ConstantesMatematicaVerde.CarbonoOrganicoDegradable * ConstantesMatematicaVerde.CarbonoOrganicoDegradableFraccionario
                * ConstantesMatematicaVerde.FraccionMetanoEnBiogas * ConstantesMatematicaVerde.ConstanteConversionCarbonoMetano * ConstantesMatematicaVerde.FactorCorreccionVertedero;

            return calculo;
        }

        public double CalcularMetanoEnCompost(double MasaCarbono)
        {
            double calculo = MasaCarbono * ConstantesMatematicaVerde.ConstanteFactorEmisionMetano;

            return calculo;
        }
    }
}
