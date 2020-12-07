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

        private bool tyyppi;
        private double gain;

        public Saadin(bool onOff, double gainAsetus) 
        {
            tyyppi = onOff;
            asetusArvo = 0;
            mitattuArvo = 0;
            gain = gainAsetus;
        }
        public int PalautaOhjaus(double asetusarvo, double mittaus)
        {
            MuutaMitattuArvo(mittaus);
            MuutaAsetusarvo(asetusarvo);
            int ohjaus = LaskeOhjaus();
            return ohjaus;
        }
        private void MuutaMitattuArvo(double mittaus)
        {
            mitattuArvo = mittaus;
        }
        private void MuutaAsetusarvo(double asetusarvo)
        {
            asetusArvo = asetusarvo;
        }
        
        // Lasketaan ohjaus yksinkertaisella P-säätimellä. Jos ohjattava laite hyväksyy ohjauksena vain 0 tai 100, muutetaan ohjaus hyväksyttyyn muotoon
        private int LaskeOhjaus()
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
                if (ohjaus > 0)
                {
                    ohjaus = 100;
                }
                else
                {
                    ohjaus = 0;
                }
            }
            return ohjaus;
        }

    }
}
