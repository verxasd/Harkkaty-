using System;
using System.Collections.Generic;

namespace Harkkatyö
{
    public class MuuttuneetEventBool : EventArgs
    {
        private Dictionary<string, bool> MuuttuneetArvot;

        public MuuttuneetEventBool(Dictionary<string, bool> muuttui)
        {
            MuuttuneetArvot = muuttui;
        }

        public Dictionary<string, bool> PalautaArvot()
        {
            return MuuttuneetArvot;
        }
    }
}