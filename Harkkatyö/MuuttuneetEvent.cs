using System;
using System.Collections.Generic;

namespace Harkkatyö
{
    public class MuuttuneetEvent : EventArgs
    {
        private Dictionary<string, int> MuuttuneetArvot;

        public MuuttuneetEvent(Dictionary<string, int> muuttui)
        {
            MuuttuneetArvot = muuttui;
        }

        public Dictionary<string, int> PalautaArvot()
        {
            return MuuttuneetArvot;
        }
    }
}