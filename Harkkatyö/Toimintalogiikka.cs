using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnifiedAutomation.UaBase;
using UnifiedAutomation.UaClient;
using Tuni.MppOpcUaClientLib;

namespace Harkkatyö
{
    class Toimintalogiikka
    {
        private Prosessi prosessi = new Prosessi();

        public void kaynnistaSekvenssi()
        {
        //    prosessi.muutaOnOff("E100", false);
            
        }
        public void pysaytaSekvenssi() 
        {
            
        }
        public void muutaParametreja(int keittoaika, int keittolampotila, int kyllastysaika, int kyllastyspaine)
        {
            this.keittoaika = keittoaika;
            this.keittolampotila = keittolampotila;
            this.kyllastysaika = kyllastysaika;
            this.kyllastyspaine = kyllastyspaine;
        }

        public int T100pinta;
        public int T200pinta;
        public int T400pinta;
        public int T300paine;
        public int T300lampotila;

        public bool tila;

        private int keittoaika;
        private int keittolampotila;
        private int kyllastysaika;
        private int kyllastyspaine;

        

        

        


    }
}
