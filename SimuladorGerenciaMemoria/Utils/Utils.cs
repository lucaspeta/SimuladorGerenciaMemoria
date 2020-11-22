using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SimuladorGerenciaMemoria.Utils
{
    public class Utils
    {
        public static int RetornaPorcentagem(int i, long total)
        {
            return (int)((i * 100) / total);
        }

        public static int RetornaIndex(double porc)
        {
            int index = 0;

            if (porc <= 10) index = 0; if (porc > 10 && porc <= 20) index = 1;
            if (porc > 20 && porc <= 30) index = 2; if (porc > 30 && porc <= 40) index = 3;
            if (porc > 40 && porc <= 50) index = 4; if (porc > 50 && porc <= 60) index = 5;
            if (porc > 60 && porc <= 70) index = 6; if (porc > 70 && porc <= 80) index = 7;
            if (porc > 80 && porc <= 90) index = 8; if (porc > 90) index = 9;

            return index;
        }

        public static int IntPow(int x, uint pow)
        {
            int ret = 1;

            while (pow != 0)
            {
                if ((pow & 1) == 1)
                    ret *= x;
                x *= x;
                pow >>= 1;
            }

            return ret;
        }
    }
}
