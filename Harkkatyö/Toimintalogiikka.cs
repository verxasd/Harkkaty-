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
        private bool clientStatus = false;

        public void kaynnistaSekvenssi()
        {
            if(!clientStatus)
            {
                prosessi.ClientInit();
                clientStatus = true;
            }
            
            prosessi.muutaOnOff("E100", true);
            
        }
        public void pysaytaSekvenssi() 
        {
            prosessi.muutaOnOff("E100", false);
        }
        public void muutaParametreja(double keittoaika, double keittolampotila, double kyllastysaika, int keittopaine)
        {
            this.keittoaika = keittoaika;
            this.keittolampotila = keittolampotila;
            this.kyllastysaika = kyllastysaika;
            this.keittopaine = keittopaine;
        }

        public double T100pinta;
        public double T200pinta;
        public double T400pinta;
        public double T300paine;
        public double T300lampotila;

        public bool tila;

        private double keittoaika;
        private double keittolampotila;
        private double kyllastysaika;
        private int keittopaine;

        

        

        


    }
}
