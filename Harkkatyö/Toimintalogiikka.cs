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
    // Event handlerit, jotka ovat vanhan toteutuksen peruja. Poistettava nykytiedon valossa viimeistään ennen lopullista palautusta.
    /*
    public delegate void OmaEventHandlerInt(object source, MuuttuneetEventInt m);
    public delegate void OmaEventHandlerDouble(object source, MuuttuneetEventDouble m);
    public delegate void OmaEventHandlerBool(object source, MuuttuneetEventBool m);
    */

    public delegate void OmaEventHandler(object source, MuuttuneetEvent m);

    class Toimintalogiikka
    {
        private Prosessi prosessi = new Prosessi();


        // Säätimet lämpötilan ja paineen säätämiseen. Paineen säätämiseen toteutettava PI- tai PID-säädin, pelkällä P ei saavuteta riittävää tarkkuutta?
        private Saadin V104Saadin = new Saadin(false, -0.5);
        private Saadin E100Saadin = new Saadin(true, 10);

        private bool clientStatus = false;
        private bool kaynnistetty = false;

        private bool sekvenssiKaynnissa = false;

        private double keittoaika;
        private double keittolampotila;
        private double kyllastysaika;
        private int keittopaine;

        private int LI100SafetyLevel = 100;

        // Julkiset metodit, joita voidaan kutsua käyttöliittymästä
        // Alustetaan prosessi, otetaan yhteys simulaattoriin ja käynnistetään thread jota käytetään prosessin tilan seuraamiseen
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

            // Muutetaan parametrit koekäyttöä varten tämän kautta
            muutaParametreja(20, 30, 15, 200);

            // Tulostetaan muutetut parametrit outputiin testitarkoituksessa
            Trace.WriteLine(keittopaine);
            Trace.WriteLine(keittoaika);
            Trace.WriteLine(keittolampotila);
            Trace.WriteLine(kyllastysaika);

        }

        // Käynnistetään sekvenssi
        public void kaynnistaSekvenssi()
        {
            // Prosessin alustaminen tällä hetkellä sekvenssin käynnistämisessä vain testaustarkoituksessa, oikeasti se alustetaan jo käyttöliittymän pääikkunan avautumisen yhteydesssä
            AlustaProsessi();
            sekvenssiKaynnissa = true;
            Thread thread3 = new Thread(Kaynnista);
            thread3.Start();

            /*
            // Muutetaan parametrit koekäyttöä varten tämän kautta
            muutaParametreja(20, 30, 15, 200);

            // Tulostetaan muutetut parametrit outputiin testitarkoituksessa
            Trace.WriteLine(keittopaine);
            Trace.WriteLine(keittoaika);
            Trace.WriteLine(keittolampotila);
            Trace.WriteLine(kyllastysaika);*/

            // Lämmittimen käynnistäminen on tällä hetkellä mukana vain testaustarkoituksessa
            // prosessi.MuutaOnOff("E100", true);
            /*
            if (tila == 1)
            {
                Impregnation();
            }
            if (tila == 2)
            {
                BlackLiquorFill();
            }
            if (tila == 3)
            {
                WhiteLiquorFill();
            }
            if (tila == 4)
            {
                Cooking();
            }
            if (tila == 5)
            {
                Discharge();
            }
            */



        }

        public void pysaytaSekvenssi()
        {
            // Lämmittimen sammuttaminen on tällä hetkellä mukana vain testaustarkoituksessa
            // prosessi.MuutaOnOff("E100", false);
            sekvenssiKaynnissa = false;
            // Kaikkien venttiilien, pumppujen ja lämmittimien sulkeminen välittömästi
            EM1_OP3();
            EM1_OP4();
            EM2_OP2();
            EM3_OP1();
            EM3_OP6();
            EM3_OP7();
            // Digesterin paineen poistaminen tässä vaiheessa? Jätän sen komennon vielä kommentteihin
            //EM3_OP8();
            EM4_OP2();
            EM5_OP3();
            EM5_OP4();
            U1_OP3();
            U1_OP4();

            // Palautetaan prosessin tila kohtaan 1
            tila = 1;
        }
        
        // Muutetaan parametrit käyttöliittymästä käsin
        public void muutaParametreja(double keittoaika, double keittolampotila, double kyllastysaika, int keittopaine)
        {
            this.keittoaika = keittoaika;
            this.keittolampotila = keittolampotila;
            this.kyllastysaika = kyllastysaika;
            this.keittopaine = keittopaine;
        }

        // Yksityiset metodit alkavat
        
        // Tilataan event prosessin arvojen muuttumisesta
        private void ProsessiKaynnissa()
        {
            prosessi.MikaMuuttui += new OmaEventHandler(PaivitaArvot);
        }

        private void Kaynnista() 
        {
            if (tila == 1 && sekvenssiKaynnissa == true)
            {
                Impregnation();
            }
            if (tila == 2 && sekvenssiKaynnissa == true)
            {
                BlackLiquorFill();
            }
            if (tila == 3 && sekvenssiKaynnissa == true)
            {
                WhiteLiquorFill();
            }
            if (tila == 4 && sekvenssiKaynnissa == true)
            {
                Cooking();
            }
            if (tila == 5 && sekvenssiKaynnissa == true)
            {
                Discharge();
            }
        }

        // Sekvenssin runko, operaatiot kuitenkin omiin metodeihin
        // Sekvenssin runkometodeille ehkä kuitenkin jokin palautusarvo prosessin tilan seuraamiseksi? Esim bool siitä onko tilanne valmis vai ei?
        private void Impregnation() 
        {
            Trace.WriteLine("Aloitetaan vaihe 1");
            double impregnationTime = kyllastysaika;
            bool LS300P = mitattavatBool["LS+300"];

            // EM2_OP1, EM5_OP1, EM3_OP2
            EM2_OP1();
            EM5_OP1();
            EM3_OP2();

            if (!LS300P) 
            {
                
                while (!LS300P && sekvenssiKaynnissa == true)
                {                    
                    Trace.WriteLine("LS300 ei ole valmis");
                    LS300P = mitattavatBool["LS+300"];                    
                    Thread.Sleep(500);
                }
            }
            EM3_OP1();
            Trace.WriteLine("Käynnistetään kyllästysajan laskuri");
            // Pistetään laskuri rullaamaan, kun yläraja on saavutettu            
            while (impregnationTime > 0.0 && sekvenssiKaynnissa == true)
            {
                // LS+300 aktivoituu
                
                // EM3_OP1                    
                impregnationTime -= 0.5;
                Trace.WriteLine("Kyllästysaikaa jäljellä " + impregnationTime.ToString());
                
                Thread.Sleep(500);

            }
            // Time Ti tulee täyteen
            // EM2_OP2, EM5_OP3, EM3_OP6
            EM2_OP2();
            EM5_OP3();
            EM3_OP6();
            
            // Pidetään threadia 0,5 s levossa ennen paineen poistamista
            Thread.Sleep(500);
            
            // EM3_OP8
            EM3_OP8();
            
            // Lopuksi muutetaan tilaa isommaksi ja kutsutaan uudestaan sekvenssin käynnistämistä
            tila += 1;
            Kaynnista();
        }
        private void BlackLiquorFill() 
        {
            Trace.WriteLine("Aloitetaan vaihe 2");
            int LI400 = mitattavatInt["LI400"];
            // LI400 rajana on LI400 < 35 mm 

            // EM3_OP2, EM5_OP1, EM4_OP1
            EM3_OP2();
            EM5_OP1();
            EM4_OP1();

            while (LI400 > 35 && sekvenssiKaynnissa == true) 
            {
                LI400 = mitattavatInt["LI400"];
                Thread.Sleep(500);
            }
            // LI400

            // EM3_OP6, EM5_OP3, EM4_OP2
            EM3_OP6();
            EM5_OP3();
            EM4_OP2();

            // Lopuksi muutetaan tilaa isommaksi ja kutsutaan uudestaan sekvenssin käynnistämistä
            tila += 1;
            Kaynnista();
        }
        private void WhiteLiquorFill()
        {
            Trace.WriteLine("Aloitetaan vaihe 3");
            int LI400 = mitattavatInt["LI400"];
            // LI400 rajana on LI400 > 80 mm 
            // EM1_OP2 LI100 safety level = 100 mm

            // EM3_OP3, EM1_OP2
            EM3_OP3();
            EM1_OP2();

            // Tarkastetaan syklin välein nestepintojen taso, pysäytetään kaikki jos LI100 pinta laskee liian alas
            while (LI400 <= 80 && sekvenssiKaynnissa == true)
            {
                LI100Safety();
                LI400 = mitattavatInt["LI400"];
                Thread.Sleep(500);
            }
            // LI400

            // EM3_OP6, EM1_OP4
            EM3_OP6();
            EM1_OP4();

            // Lopuksi muutetaan tilaa isommaksi ja kutsutaan uudestaan sekvenssin käynnistämistä
            tila += 1;
            Kaynnista();
        }
        private void Cooking() 
        {
            Trace.WriteLine("Aloitetaan vaihe 4");
            double cookingTime = keittoaika;
            double TI300 = mitattavatDouble["TI300"];
            // EM1_OP1 LI100 safety level = 100 mm

            // EM3_OP4, EM1_OP1
            EM3_OP4();
            EM1_OP1();

            while (TI300 < keittolampotila && sekvenssiKaynnissa == true) 
            {
                LI100Safety();
                TI300 = mitattavatDouble["TI300"];
                Thread.Sleep(500);
            }
            // TI300 lämpenee tarpeeksi
            // EM3_OP1, EM1_OP2
            EM3_OP1();
            EM1_OP2();
            
            // U1_OP1, U1_OP2
            while (cookingTime > 0  && sekvenssiKaynnissa == true)
            {
                U1_OP1();
                U1_OP2();
                cookingTime -= 0.5;
                Thread.Sleep(500);
            }
            // Time tc tulee täyteen
            // U1_OP3, U1_OP4
            U1_OP3();
            U1_OP4();
            // EM3_OP6, EM1_OP4
            EM3_OP6();
            EM1_OP4();

            // Pidetään threadia levossa 0,5 s ennen paineen poistamista
            Thread.Sleep(500);
            // EM3_OP8
            EM3_OP8();

            // Lopuksi muutetaan tilaa isommaksi ja kutsutaan uudestaan sekvenssin käynnistämistä
            tila += 1;
            Kaynnista();
        }
        private void Discharge()
        {
            Trace.WriteLine("Aloitetaan vaihe 5");
            bool LS300N = mitattavatBool["LS-300"];
            // EM5_OP2, EM3_OP5
            EM5_OP2();
            EM3_OP5();

            // Seurataan LS-300 tilaa, jatketaan tyhjäystä niin kauan kun tavaraa riittää
            while (LS300N && sekvenssiKaynnissa == true)
            {
                LS300N = mitattavatBool["LS-300"];
                Thread.Sleep(500);
            }
            // LS-300 deaktivoituu
            // EM5_OP4, EM3_OP7
            EM5_OP4();
            EM3_OP7();

            // Muutetaan prosessin tila takaisin arvoon 1
            tila = 1;
        }

        // Varsinaiset metodit, joilla prosessin eri vaiheet toteutetaam

        // EM1 operaatiot
        private void EM1_OP1() 
        {            
            prosessi.MuutaVenttiilinOhjaus("V102", 100);
            prosessi.MuutaOnOff("V304", true);
            prosessi.MuutaOnOff("P100_P200_PRESET", true);            
            prosessi.MuutaPumpunOhjaus("P100", 100);
            prosessi.MuutaOnOff("E100", true);
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
            double viive = 1;
            while (viive > 0)
            {
                viive -= 0.5;
                Thread.Sleep(500);
            }
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
            prosessi.MuutaOnOff("P100_P200_PRESET", true);
            prosessi.MuutaPumpunOhjaus("P200", 100);
        }
        private void EM5_OP2()
        {
            prosessi.MuutaOnOff("V103", true);
            prosessi.MuutaOnOff("V303", true);
            prosessi.MuutaOnOff("P100_P200_PRESET", true);
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
            int PI300 = mitattavatInt["PI300"];
            // Säädin, jolla säädetään V104 tilaa tarpeen mukaan
            int ohjaus = V104Saadin.PalautaOhjaus(Convert.ToDouble(keittopaine), Convert.ToDouble(PI300));
            prosessi.MuutaVenttiilinOhjaus("V104", ohjaus);

            // Jos paine poikkeaa liikaa asetusarvosta, keskeytetään sekvenssi
            if (Math.Abs(PI300 - keittopaine) > 10)
            {
                Trace.WriteLine("Paine poikkesi sallitusta");
                Trace.WriteLine(PI300);
                // pysaytaSekvenssi();
            }
        }
        private void U1_OP2()
        {
            double TI300 = mitattavatDouble["TI300"];
            double TI100 = mitattavatDouble["TI100"];
            // Tälle säädin, jolla katkotaan E100 sopivasti
            int ohjaus = E100Saadin.PalautaOhjaus(keittolampotila, TI300);

            bool ohjausBool;

            
            if (ohjaus == 100)
            {
                ohjausBool = true;
            }
            else
            {
                ohjausBool = false;
            }
            prosessi.MuutaOnOff("E100", ohjausBool);

            if (Math.Abs(TI300 - keittolampotila) > 0.3)
            {
                Trace.WriteLine("Lämpötila poikkesi sallitusta");
                Trace.WriteLine(TI300);
                // pysaytaSekvenssi();
            }
        }
        private void U1_OP3() 
        {
            prosessi.MuutaVenttiilinOhjaus("V104", 0);
        }
        private void U1_OP4()
        {
            prosessi.MuutaOnOff("E100", false);
        }

        // Metodi LI100 pinnankorkeuden varmistamiseen. Kutsutaan joka loopilla, kun pumppu P100 on käynnissä
        private void LI100Safety() 
        {
            if (mitattavatInt["LI100"] < LI100SafetyLevel) 
            {
                pysaytaSekvenssi();
            }
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
                    //Trace.WriteLine(avain.ToString() + " " + mitattavatBool[avain].ToString());
                }
                if (muuttuneet[avain] == 1)
                {
                    mitattavatDouble[avain] = prosessi.PalautaDouble(avain);
                    //Trace.WriteLine(avain.ToString() + " " + mitattavatDouble[avain].ToString());
                }
                if (muuttuneet[avain] == 2)
                {
                    mitattavatInt[avain] = prosessi.PalautaInt(avain);
                    //Trace.WriteLine(avain.ToString() + " " + mitattavatInt[avain].ToString());
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


        // Tilan muuttaminen privateksi, ehkä myös eri muotoon? Int? Tai dict<string, bool>?        
        
        // Prosessin tila. 1 = Impregnation, 2 = BlackLiquorFill, 3 = WhiteLiquorFill, 4 = Cooking, 5 = Discharge
        private int tila = 1;
        // public bool tila;

        // Mitattavat suureet omissa dictionaryissaan, jokaiselle tyypille oma dictionary
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

        
    }
}
