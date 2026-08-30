using System;
using System.Collections.Generic;
using System.Text;
using EcoHuellaApp.Helpers;

namespace EcoHuellaApp.Helpers
{
    // TODO: validar que los parámetros (cantidadesBaldes, MasaCarbono) no sean negativos en los métodos públicos de esta clase.
    public class  MatematicaVerde
    {
        public double CalcularMasa(int cantidadesBaldes)
        {

            /// <summary>
            /// La ecuación nos devuelve la masa de residuos orgánicos asentados. Se hace multiplicando la cantidad de baldes de una recolección por el volumen constante usado
            /// por Aldea Las Nubes por la densidad calculada dada por la Secrertaria de Ambiente de Quito.
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
            /// <summary>
            /// Método encargado de calcular el metano dentro 
            /// del compostaje anaeróbico en base a https://www.ipcc-nggip.iges.or.jp/public/2006gl/pdf/5_Volume5/V5_4_Ch4_Bio_Treat.pdf
            /// </summary>

            double calculo = MasaCarbono * ConstantesMatematicaVerde.ConstanteFactorEmisionMetano;

            return calculo;
        }

        public double CalcularMetanoEvitado(double MasaCarbono)
        {
            ///<summary>
            ///Calcula el metano evitado basado en el metano que se genera en el compost y el potencial de metano en vertedero.
            ///</summary>
            
            double calculo = CalcularMetanoEnVertadero(MasaCarbono) - CalcularMetanoEnCompost(MasaCarbono);

            return calculo;
        }

        public double ConversionMetanoCarbono(double MasaCarbono)
        {
            double calculo = CalcularMetanoEvitado(MasaCarbono) * ConstantesMatematicaVerde.PotencialCalentamientoMetano;

            return calculo;
        }

        public double PerdidaMasa(double MasaCarbono)
        {
            double calculo = MasaCarbono * ConstantesMatematicaVerde.PerdidaVolumen;

            return calculo;
        }
    }
}
