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

    public delegate void OmaEventHandler(object source, MuuttuneetEvent m);
    public delegate void YhteysEventHandler(string yhteydenUusiTila);

    class Toimintalogiikka
    {
        private Prosessi prosessi = new Prosessi();

        // Säätimet lämpötilan ja paineen säätämiseen
        private Saadin E100Saadin = new Saadin(true, 10);
        private Saadin V104Saadin = new Saadin(false, -0.01*0.5);

        // Booleanit joilla varmistetaan, ettei clientiä voi initoida tai prosessia käynnistää, jos ne on jo tehty eikä niitä ole sammutettu
        private string clientStatus;
        private bool kaynnistetty = false;

        // Boolean, johon tallennetaan tieto siitä onko prosessi käynnissä
        private bool sekvenssiKaynnissa = false;

        private double keittoaika;
        private double keittolampotila;
        private double kyllastysaika;
        private int keittopaine;

        private int LI100SafetyLevel = 100;

        // Julkiset metodit, joita voidaan kutsua käyttöliittymästä
        // Alustetaan prosessi, otetaan yhteys simulaattoriin ja käynnistetään thread jota käytetään prosessin tilan seuraamiseen
        public event PaivitaTiedot paivita;
        public event TilaVaihtui tilaVaihtui;
        public event YhteydenTilaVaihtui yhteydenTilaVaihtui;

        public void AlustaProsessi()
        {
            if (clientStatus != "Connected" || clientStatus != "Connecting")
            {
                prosessi.YhteysMuuttui += new YhteysEventHandler(YhteysMuuttui);
                prosessi.YhdistaProsessiin();

                Trace.WriteLine("tila logiikassa yhdistämisen jälkeen: " + clientStatus);

                if (clientStatus == "Connected")
                {
                    PaivitaTiedot handler = paivita;
                    handler?.Invoke();
                    // Pysäytetään sekvenssi yhteyden palatessa, jos sekvenssi on ollut käynnissä yhteyden katketessa
                    if (tila != 1)
                    {
                        kaynnistetty = false;
                        pysaytaSekvenssi();
                    }
                }
                

            }

            if (!kaynnistetty)
            {
                Thread thread2 = new Thread(ProsessiKaynnissa);
                thread2.Start();
                kaynnistetty = true;
            }
        }

        private void YhteysMuuttui(string yhteydenUusiTila)
        {
            Trace.WriteLine("logiikka sai tiedon yhteyden tila vaihtumisesta");
            clientStatus = yhteydenUusiTila;

            if (clientStatus == "Disconnected")
            {
                pysaytaSekvenssi();
            }
            
            YhteydenTilaVaihtui handler = yhteydenTilaVaihtui;
            handler?.Invoke(clientStatus);
        }

        // Käynnistetään sekvenssi
        public void kaynnistaSekvenssi()
        {
            sekvenssiKaynnissa = true;

            // Muutetaan parametrit koekäyttöä varten tämän kautta
            muutaParametreja(20, 25, 15, 250);

            // Tulostetaan muutetut parametrit outputiin testitarkoituksessa
            Trace.WriteLine(keittopaine);
            Trace.WriteLine(keittoaika);
            Trace.WriteLine(keittolampotila);
            Trace.WriteLine(kyllastysaika);

            Thread thread3 = new Thread(Kaynnista);
            thread3.Start();
        }

        internal double PalautaDouble(string v)
        {
            double luku;
            if (mitattavatDouble.ContainsKey(v))
            {
                luku = mitattavatDouble[v];
            }
            else
            {
                luku = -1;
            }
            return luku;
        }

        internal int PalautaInt(string v)
        {
            int luku;
            if (mitattavatInt.ContainsKey(v))
            {
                luku = mitattavatInt[v];
            }
            else
            {
                luku = -1;
            }
            return luku;
        }

        public void pysaytaSekvenssi()
        {
            // Lämmittimen sammuttaminen on tällä hetkellä mukana vain testaustarkoituksessa
            // prosessi.MuutaOnOff("E100", false);

            // Muutetaan sekvenssin tilaksi false ja palautetaan prosessin tila kohtaan 1
            sekvenssiKaynnissa = false;
            tila = 1;

            // Kaikkien venttiilien, pumppujen ja lämmittimien sulkeminen välittömästi
            EM1_OP3();
            EM1_OP4();
            EM2_OP2();
            EM3_OP1();
            EM3_OP6();
            EM3_OP7();            
            EM4_OP2();
            EM5_OP3();
            EM5_OP4();
            U1_OP3();
            U1_OP4();

            // Digesterin paineen poistaminen tässä vaiheessa? Jätän sen komennon vielä kommentteihin
            //EM3_OP8();
            
            TilaVaihtui handler = tilaVaihtui;
            handler?.Invoke("Idle");
        }
        
        // Muutetaan parametrit käyttöliittymästä käsin
        public void muutaParametreja(double keittoaikaA, double keittolampotilaA, double kyllastysaikaA, int keittopaineA)
        {
            this.keittoaika = keittoaikaA;
            this.keittolampotila = keittolampotilaA;
            this.kyllastysaika = kyllastysaikaA;
            this.keittopaine = keittopaineA;
        }

        // Yksityiset metodit alkavat
        
        // Tilataan event prosessin arvojen muuttumisesta
        private void ProsessiKaynnissa()
        {
            prosessi.MikaMuuttui += new OmaEventHandler(PaivitaArvot);
        }

        private void Kaynnista()
        {
            // Normaali toimintasykli
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
            TilaVaihtui handler = tilaVaihtui;
            handler?.Invoke("Impregnation");
            
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

            // EM3_OP1 
            EM3_OP1();
            Trace.WriteLine("Käynnistetään kyllästysajan laskuri");
            Trace.WriteLine(impregnationTime);
            // Pistetään laskuri rullaamaan, kun yläraja on saavutettu            
            while (impregnationTime > 0.0 && sekvenssiKaynnissa == true)
            {                   
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

            if (sekvenssiKaynnissa)
            {
                // Lopuksi muutetaan tilaa isommaksi ja kutsutaan uudestaan sekvenssin käynnistämistä
                tila += 1;
                Kaynnista();
            }

        }
        private void BlackLiquorFill() 
        {
            TilaVaihtui handler = tilaVaihtui;
            handler?.Invoke("Black Liquor Fill");
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

            if (sekvenssiKaynnissa)
            {
                // Lopuksi muutetaan tilaa isommaksi ja kutsutaan uudestaan sekvenssin käynnistämistä
                tila += 1;
                Kaynnista();
            }
        }
        private void WhiteLiquorFill()
        {
            TilaVaihtui handler = tilaVaihtui;
            handler?.Invoke("White Liquor Fill");
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

            if (sekvenssiKaynnissa)
            {
                // Lopuksi muutetaan tilaa isommaksi ja kutsutaan uudestaan sekvenssin käynnistämistä
                tila += 1;
                Kaynnista();
            }
        }
        private void Cooking() 
        {
            TilaVaihtui handler = tilaVaihtui;
            handler?.Invoke("Cooking");
            Trace.WriteLine("Aloitetaan vaihe 4");
            double cookingTime = keittoaika;
            double TI300 = mitattavatDouble["TI300"];
            // EM1_OP1 LI100 safety level = 100 mm

            // EM3_OP4, EM1_OP1
            EM3_OP4();
            EM1_OP1();
            // Nostetaan lämpötilaa ja tarkastellaan turvarajaa, kunnes lämpötila on saavutettu
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
                // Käynnistetään paineen ja lämpötilan säätö. Keittoaika ei kuitenkaan lähde laskemaan, ennen kun keittopaine on saavutettu
                bool saavutettu = U1_OP1();
                U1_OP2();
                if (saavutettu) 
                {
                    cookingTime -= 0.5;
                }                
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

            if (sekvenssiKaynnissa)
            {
                // Lopuksi muutetaan tilaa isommaksi ja kutsutaan uudestaan sekvenssin käynnistämistä
                tila += 1;
                Kaynnista();
            }
        }
        private void Discharge()
        {
            TilaVaihtui handler = tilaVaihtui;
            handler?.Invoke("Discharge");

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
            TilaVaihtui handler1 = tilaVaihtui;
            handler1?.Invoke("Idle");
        }

        // Varsinaiset metodit, joilla prosessin eri vaiheet toteutetaan
        // Operaatiot on toteutettu tehtävänannon mukana tarjotun PFC-kaavion mukaisesti
        // Operaatioissa kutsutaan prosessin metodeita, jotka kutsuvat simulaattorin metodeita

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
            // Pidetään viive paineen poistamiseksi
            double viive = 1;
            while (viive > 0)
            {
                viive -= 0.5;
                Thread.Sleep(500);
            }
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

        // U1 operaatiot, käynnistetään/lopetetaan paineen ja lämpötila säätö. Keskeytetään sekvenssi, jos suure poikkeaa liikaa asetusarvosta

        // Paineen säätö. Palautetaan kutsujalle tieto siitä, onko haluttu paine jo saavutettu
        private bool U1_OP1() 
        {
            // Tallennetaan muuttujaan saavutettu tieto siitä, onko haluttu lämpötila jo saavutettu riittävällä tarkkuudella, oletuksena se on false
            bool saavutettu = false;
            // Luetaan PI300
            int PI300 = mitattavatInt["PI300"];
            // Voidaan laskea venttiilin suuntaa antava asento halutun paineen perusteella, koska venttiili käyttäytyy testien perusteella käytännössä lineaarisesti
            int nollalinja = 100 - Convert.ToInt32(keittopaine)/3;
            // Käytetään ohjauksen laskemiseen Säädin-luokan oliota, jolle annetaan parametreina keittopaine, nykyinen paine ja aiemmin laskettu nollalinja
            int ohjaus = V104Saadin.PalautaOhjaus(Convert.ToDouble(keittopaine), Convert.ToDouble(PI300), nollalinja);
            // Muutetaan halutun venttiilin tilaa prosessin metodin avulla, annetaan parametrina venttiilin nimi ja ohjaus
            prosessi.MuutaVenttiilinOhjaus("V104", ohjaus);

            // Tarkastetaan, onko haluttu paine jo saavutettu
            if (Math.Abs(PI300 - keittopaine) < 10) 
            {
                // Kun haluttu paine on saavutettu, paineen seuranta otetaan käyttöön
                saavutettu = true;

            }
                // Jos paine poikkeaa liikaa asetusarvosta, keskeytetään sekvenssi
            if (Math.Abs(PI300 - keittopaine) > 10 && saavutettu)
            {
                Trace.WriteLine("Paine poikkesi sallitusta");
                Trace.WriteLine("Paine: " + PI300.ToString() + ", virtaus " + mitattavatDouble["FI100"].ToString());
                Trace.WriteLine("Venttiilin uusi ohjaus: " + ohjaus);
                pysaytaSekvenssi();
            }
            return saavutettu;
        }
        // Lämpötilan säätö 
        private void U1_OP2()
        {
            double TI300 = mitattavatDouble["TI300"];
            double TI100 = mitattavatDouble["TI100"];
            // Tälle säädin, jolla katkotaan E100 sopivasti
            int ohjaus = E100Saadin.PalautaOhjaus(keittolampotila, TI300, 0);

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

            PaivitaTiedot handler = paivita;
            handler?.Invoke();

        }

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
