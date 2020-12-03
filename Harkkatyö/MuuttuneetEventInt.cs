using System;
using System.Collections.Generic;

namespace Harkkatyö
{
    public class MuuttuneetEventInt : EventArgs 
    {
        private Dictionary<string, int> MuuttuneetArvot;

        public MuuttuneetEventInt(Dictionary<string, int> muuttui)
        {
            MuuttuneetArvot = muuttui;
        }
        
        public Dictionary<string, int> PalautaArvot() 
        {
            return MuuttuneetArvot;
        }
    }
}