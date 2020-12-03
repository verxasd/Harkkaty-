using System;
using System.Collections.Generic;

namespace Harkkatyö
{
    public class MuuttuneetEventDouble : EventArgs
    {
        private Dictionary<string, double> MuuttuneetArvot;

        public MuuttuneetEventDouble(Dictionary<string, double> muuttui)
        {
            MuuttuneetArvot = muuttui;
        }

        public Dictionary<string, double> PalautaArvot()
        {
            return MuuttuneetArvot;
        }
    }
}