using System;
using System.Collections.Generic;

namespace Harkkatyö
{
    /// <summary>
    /// Luokka kuvaa event argumentien muodon omalle eventille, joka nostetaan kun prosessi-luokan muistiin tallennettavat arvot muuttuvat
    /// </summary>
    public class MuuttuneetEvent : EventArgs
    {
        private Dictionary<string, int> MuuttuneetArvot;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="muuttui">Dictionary, joka sisältää kaikkien muuttuneiden arvojen nimet ja tiedon niiden tyypistä</param>
        public MuuttuneetEvent(Dictionary<string, int> muuttui)
        {
            MuuttuneetArvot = muuttui;
        }
        /// <summary>
        /// Metodi palauttaa dictionaryn, mikä sisältää muuttuneet arvot ja niiden tyypin
        /// </summary>
        /// <returns>Dictionary, joka sisältää muuttuneiden arvojen nimet ja tiedon niiden tyypeistä</returns>
        public Dictionary<string, int> PalautaArvot()
        {
            return MuuttuneetArvot;
        }
    }
}