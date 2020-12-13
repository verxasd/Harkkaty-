using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harkkatyö
{
    /// <summary>
    /// Luokka, jolla toteutetaan säätimen toiminta
    /// </summary>
    class Saadin
    {
        private double asetusArvo;
        private double mitattuArvo;

        private int ohjaus;

        private int ohjausNolla;

        private bool tyyppi;
        private double Kp;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="onOff">Totuusarvo ohjattavan laitteen tyypistä. True = toimilaitetta ohjataan binääristi, false = toimilaitetta ohjataan 0-100</param>
        /// <param name="KpAsetus">Säätimen P-vahvistus doublena</param>
        public Saadin(bool onOff, double KpAsetus) 
        {
            tyyppi = onOff;
            asetusArvo = 0;
            mitattuArvo = 0;
            Kp = KpAsetus;
        }
        /// <summary>
        /// Metodilla muutetaan säätimelle asetusarvo, mittaus, ohjaus ja nollalinja. Metodi palauttaa ohjauksen
        /// </summary>
        /// <param name="asetusarvo">Ohjattavan suureen asetusarvo doublena</param>
        /// <param name="mittaus">Ohjattavan suureen mittaustulos doublena</param>
        /// <param name="nollalinja">Toimilaitteen nollalinja kokonaislukuna</param>
        /// <returns>Ohjaus kokonaislukuna</returns>
        public int PalautaOhjaus(double asetusarvo, double mittaus, int nollalinja)
        {
            ohjausNolla = nollalinja;
            mitattuArvo = mittaus;
            asetusArvo = asetusarvo;
            int ohjaus = LaskeOhjaus();
            return ohjaus;
        }
        
        // Lasketaan ohjaus yksinkertaisella P-säätimellä. Jos ohjattava laite hyväksyy ohjauksena vain 0 tai 100, muutetaan ohjaus hyväksyttyyn muotoon
        private int LaskeOhjaus()
        {
            int ohjausP;
           
            double erosuure = asetusArvo - mitattuArvo;

            // Muutetaan lasketut haarakohtaiset ohjaukset integereiksi ja summataan ne
            // Lopullisessa versiossa käytettiin vain P-säädintä
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
