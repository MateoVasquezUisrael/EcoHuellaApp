using System;
using System.Collections.Generic;
using System.Text;
using EcoHuellaApp.Helpers;

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
    }
}
