using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnifiedAutomation.UaBase;
using UnifiedAutomation.UaClient;
using Tuni.MppOpcUaClientLib;
using System.Diagnostics;
using System.Threading;

namespace Harkkatyö
{
    public delegate void OmaEventHandlerInt(object source, MuuttuneetEventInt m);
    public delegate void OmaEventHandlerDouble(object source, MuuttuneetEventDouble m);
    public delegate void OmaEventHandlerBool(object source, MuuttuneetEventBool m);

    public delegate void OmaEventHandler(object source, MuuttuneetEvent m);

    class Toimintalogiikka
    {
        private Prosessi prosessi = new Prosessi();
        private bool clientStatus = false;

        private bool kaynnistetty = false;

        // Julkiset metodit, joita voidaan kutsua käyttöliittymästä
        public void AlustaProsessi()
        {
            if (!clientStatus)
            {
                prosessi.ClientInit();
                clientStatus = true;
            }

            if (!kaynnistetty)
            {
                Thread thread2 = new Thread(ProsessiKaynnissa);
                thread2.Start();
                kaynnistetty = true;
            }
        }

        public void kaynnistaSekvenssi()
        {                       
            prosessi.MuutaOnOff("E100", true);          
        }

        public void pysaytaSekvenssi()
        {
            prosessi.MuutaOnOff("E100", false);
        }
        public void muutaParametreja(double keittoaika, double keittolampotila, double kyllastysaika, int keittopaine)
        {
            this.keittoaika = keittoaika;
            this.keittolampotila = keittolampotila;
            this.kyllastysaika = kyllastysaika;
            this.keittopaine = keittopaine;
        }

        // Yksityiset metodit alkavat
        private void ProsessiKaynnissa()
        {
            prosessi.MikaMuuttui += new OmaEventHandler(PaivitaArvot);
        }


        // Sekvenssin runko, operaatiot kuitenkin omiin metodeihin?
        private void Impregnation() 
        {
            // EM2_OP1, EM5_OP1, EM3_OP2
            // LS+300 aktivoituu
            // EM3_OP1
            // Time Ti tulee täyteen
            // EM2_OP2, EM5_OP3, EM3_OP6
            // EM3_OP8
        }
        private void BlackLiquorFill() 
        {
            // LI400 rajana on LI400 < 35 mm 
            
            // EM3_OP2, EM5_OP1, EM4_OP1
            // LI400
            // EM3_OP6, EM5_OP3, EM4_OP2
        }
        private void WhiteLiquorFill()
        {
            // LI400 rajana on LI400 > 80 mm 
            // EM1_OP2 LI100 safety level = 100 mm


            // EM3_OP3, EM1_OP2
            // LI400
            // EM3_OP6, EM1_OP4
        }
        private void Cooking() 
        {
            // EM1_OP1 LI100 safety level = 100 mm
            
            // EM3_OP4, EM1_OP1
            // TI300 lämpenee tarpeeksi
            // EM3_OP1, EM1_OP2
            // U1_OP1, U1_OP2
            // Time tc tulee täyteen
            // U1_OP3, U1_OP4
            // EM3_OP6, EM1_OP4
            // EM3_OP8
        }
        private void Discharge()
        {
            // EM5_OP2, EM3_OP5
            // LS-300 deaktivoituu
            // EM5_OP4, EM3_OP7
        }

        // Varsinaiset metodit, joilla prosessin eri vaiheet toteutetaam

        // EM1 operaatiot
        private void EM1_OP1() 
        {            
            prosessi.MuutaVenttiilinOhjaus("V102", 100);
            prosessi.MuutaOnOff("V304", true);
            prosessi.MuutaOnOff("P100_P200_PRESET", true);
            prosessi.MuutaOnOff("E100", true);
            prosessi.MuutaPumpunOhjaus("P100", 100);

        }
        private void EM1_OP2() 
        {
            prosessi.MuutaVenttiilinOhjaus("V102", 100);
            prosessi.MuutaOnOff("V304", true);
            prosessi.MuutaOnOff("P100_P200_PRESET", true);
            prosessi.MuutaPumpunOhjaus("P100", 100);
        }
        private void EM1_OP3()
        {
            prosessi.MuutaVenttiilinOhjaus("V102", 0);
            prosessi.MuutaOnOff("V304", false);
            prosessi.MuutaOnOff("E100", false);
            prosessi.MuutaPumpunOhjaus("P100", 0);

        }
        private void EM1_OP4()
        {
            prosessi.MuutaVenttiilinOhjaus("V102", 0);
            prosessi.MuutaOnOff("V304", false);
            prosessi.MuutaPumpunOhjaus("P100", 0);
        }

        // EM2 operaatiot
        private void EM2_OP1()
        {
            prosessi.MuutaOnOff("V201", true);
        }
        private void EM2_OP2()
        {
            prosessi.MuutaOnOff("V201", false);
        }

        // EM3 operaatiot
        private void EM3_OP1()
        {
            prosessi.MuutaVenttiilinOhjaus("V104", 0);
            prosessi.MuutaOnOff("V204", false);
            prosessi.MuutaOnOff("V401", false);

        }
        private void EM3_OP2()
        {
            prosessi.MuutaOnOff("V204", true);
            prosessi.MuutaOnOff("V301", true);
        }
        private void EM3_OP3()
        {
            prosessi.MuutaOnOff("V301", true);
            prosessi.MuutaOnOff("V401", true);
        }
        private void EM3_OP4()
        {
            prosessi.MuutaVenttiilinOhjaus("V104", 100);
            prosessi.MuutaOnOff("V301", true);
        }
        private void EM3_OP5()
        {
            prosessi.MuutaOnOff("V204", true);
            prosessi.MuutaOnOff("V302", true);
        }
        private void EM3_OP6() 
        {
            prosessi.MuutaVenttiilinOhjaus("V104", 0);
            prosessi.MuutaOnOff("V204", false);
            prosessi.MuutaOnOff("V301", false);
            prosessi.MuutaOnOff("V401", false);
        }
        private void EM3_OP7()
        {
            prosessi.MuutaOnOff("V302", false);
            prosessi.MuutaOnOff("V204", false);
        }
        private void EM3_OP8()
        {
            prosessi.MuutaOnOff("V204", true);
            // tähän väliin toteutettava 1 s viive myöhemmin
            prosessi.MuutaOnOff("V204", false);
        }

        // EM4 operaatiot
        private void EM4_OP1()
        {
            prosessi.MuutaOnOff("V404", true);
        }
        private void EM4_OP2() 
        {
            prosessi.MuutaOnOff("V404", false);
        }

        //EM5 operaatiot
        private void EM5_OP1()
        {
            prosessi.MuutaOnOff("V303", true);
            prosessi.MuutaPumpunOhjaus("P200", 100);
        }
        private void EM5_OP2()
        {
            prosessi.MuutaOnOff("V103", true);
            prosessi.MuutaOnOff("V303", true);
            prosessi.MuutaPumpunOhjaus("P200", 100);
        }
        private void EM5_OP3()
        {
            prosessi.MuutaOnOff("V303", false);
            prosessi.MuutaPumpunOhjaus("P200", 0);
        }
        private void EM5_OP4()
        {
            prosessi.MuutaOnOff("V103", false);
            prosessi.MuutaOnOff("V303", false);
            prosessi.MuutaPumpunOhjaus("P200", 0);
        }

        // U1 operaatiot, käynnistetään/lopetetaan parametrien säätö
        private void U1_OP1() 
        {
            // Säädin, jolla säädetään V104 tilaa tarpeen mukaan
        }
        private void U1_OP2()
        {
            // Tälle säädin, jolla katkotaan E100 sopivasti
        }
        private void U1_OP3() 
        {
            prosessi.MuutaVenttiilinOhjaus("V104", 0);
        }
        private void U1_OP4()
        {
            prosessi.MuutaOnOff("E100", false);
        }

        // Metodi, jota kutsutaan event handlerilla eventin noustessa. Metodilla päivitetään päivitetyt arvot toimintalogiikan tietoon
        private void PaivitaArvot(object source, MuuttuneetEvent m)
        {
            DateTime time = DateTime.Now;
            Trace.WriteLine("aika: " + time.ToString("HH: mm:ss.fff") + " toimintalogiikka sai tiedon muutoksista");
            Dictionary<string, int> muuttuneet = m.PalautaArvot();
            foreach (string avain in muuttuneet.Keys)
            {
                if (muuttuneet[avain] == 0) 
                {
                    mitattavatBool[avain] = prosessi.PalautaBool(avain);
                    Trace.WriteLine(avain.ToString() + " " + mitattavatBool[avain].ToString());
                }
                if (muuttuneet[avain] == 1)
                {
                    mitattavatDouble[avain] = prosessi.PalautaDouble(avain);
                    Trace.WriteLine(avain.ToString() + " " + mitattavatDouble[avain].ToString());
                }
                if (muuttuneet[avain] == 2)
                {
                    mitattavatInt[avain] = prosessi.PalautaInt(avain);
                    Trace.WriteLine(avain.ToString() + " " + mitattavatInt[avain].ToString());
                }
            }
        }

        /*
        private void PaivitaArvotDouble(object source, MuuttuneetEventDouble m)
        {
            DateTime time = DateTime.Now;
            Trace.WriteLine("aika: " + time.ToString("HH: mm:ss.fff") + " toimintalogiikka sai tiedon muutoksista");
            Dictionary<string, double> muuttuneet = m.PalautaArvot();
            foreach (string avain in muuttuneet.Keys) 
            {
                Trace.WriteLine(avain.ToString() + " " + muuttuneet[avain].ToString());
            }            
            // Thread.Sleep(500);
            //throw new NotImplementedException();
        }

        static void PaivitaArvotBool(object source, MuuttuneetEventBool m)
        {
            Trace.WriteLine("päivitetään boolit eventistä");
            Dictionary<string, bool> muuttuneet = m.PalautaArvot();
            foreach (string avain in muuttuneet.Keys)
            {
                Trace.WriteLine(avain.ToString() + " " + muuttuneet[avain].ToString());
            }
            //throw new NotImplementedException();
        }

        static void PaivitaArvotInt(object source, MuuttuneetEventInt m)
        {
            Trace.WriteLine("päivitetään intit eventistä");
            //throw new NotImplementedException();
        }
        */

        
                
        public bool tila;

        private Dictionary<string, bool> mitattavatBool = new Dictionary<string, bool>() {
            { "LA+100", true },
            { "LS-200", true },
            { "LS-300", true },
            { "LS+300", false }
        };

        private Dictionary<string, int> mitattavatInt = new Dictionary<string, int>() {
            { "LI100", 216 },
            { "LI200", 90 },
            { "LI400", 90 },
            { "PI300", 0 }
        };
     
        private Dictionary<string, double> mitattavatDouble = new Dictionary<string, double>() {
            { "TI100", 20 },
            { "TI300", 20 },
            { "FI100", 0 }
        };

        private double keittoaika;
        private double keittolampotila;
        private double kyllastysaika;
        private int keittopaine;
    }
}
