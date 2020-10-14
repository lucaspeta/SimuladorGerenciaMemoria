using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace SimuladorGerenciaMemoria.Classes
{
    class Utils
    {
        public static string ReadInputFile(string path)
        {
            try
            {
                return File.ReadAllText(path);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void AddTimeProcessTaked(Stopwatch stopwatch, string name, string outputPath = "../../../Outputs/timeTaked")
        {
            var line = name + " = " + stopwatch.ElapsedMilliseconds.ToString() + " milliseconds.";
            File.WriteAllText(@outputPath + name + ".txt", line);
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
