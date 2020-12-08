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

        private int ohjausNolla;

        private bool tyyppi;
        private double Kp;

        public Saadin(bool onOff, double KpAsetus) 
        {
            tyyppi = onOff;
            asetusArvo = 0;
            mitattuArvo = 0;
            Kp = KpAsetus;
        }
        public int PalautaOhjaus(double asetusarvo, double mittaus, int nollalinja)
        {
            ohjausNolla = nollalinja;
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
            int ohjausP;
           
            double erosuure = asetusArvo - mitattuArvo;

            // Muutetaan lasketut haarakohtaiset ohjaukset integereiksi ja summataan ne
            ohjausP = Convert.ToInt32(erosuure * Kp);

            ohjaus = ohjausP;

            // Suodatetaan ohjaus sallitulle välille ja palautetaan se
            if (ohjaus + ohjausNolla > 100)
            {
                ohjaus = 100 - ohjausNolla;
            }
            if (ohjaus + ohjausNolla < 0)
            {
                ohjaus = 0 - ohjausNolla;
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
            return ohjaus + ohjausNolla;
        }

    }
}
