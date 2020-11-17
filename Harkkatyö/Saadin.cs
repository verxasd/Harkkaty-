using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harkkatyö
{
    class Saadin
    {
        private double asetusArvo;
        private double mitattuArvo;

        private int ohjaus;

        private string nimi;
        private bool tyyppi;
        private double gain;

        public Saadin(string name, bool onOff, double gain) 
        {
            nimi = name;
            tyyppi = onOff;
            asetusArvo = 0;
            mitattuArvo = 0;    
        }

        public void muutaAsetusarvo(double asetusarvo) { }

        public void muutaMitattuArvo(double mitattuArvo) { }

        public void muutaOhjaus() 
        {
            double erosuure = asetusArvo - mitattuArvo;
            ohjaus = Convert.ToInt32(erosuure * gain);

            if (ohjaus > 100)
            {
                ohjaus = 100;
            }

            if (ohjaus < 0)
            {
                ohjaus = 0;
            }
            if (tyyppi)
            {
                if (ohjaus > 50)
                {
                    ohjaus = 100;
                }
                else
                {
                    ohjaus = 0;
                }
            }
        }

    }
}
