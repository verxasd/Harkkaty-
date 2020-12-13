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
    /// <summary>
    /// Tätä event handleria käytetään omien eventien käsittelyyn silloin, kun prosessin muistissa olevat mitatut arvot muuttuvat
    /// </summary>
    /// <param name="source">Source</param>
    /// <param name="m">Event argumentit omalle eventille. Dictionary, missä avaimena on string ja arvona int</param>
    public delegate void OmaEventHandler(object source, MuuttuneetEvent m);
    /// <summary>
    /// Tätä event handleria käytetään yhteyden tilasta kertovan eventin käsittelyyn silloin, kun prosessin tiedossa oleva yhteyden tila muuttuu
    /// </summary>
    /// <param name="yhteydenUusiTila">Yhteyden uusi tila merkkijonona, FullStatusString muotoisena OPC UA mukaan</param>
    public delegate void YhteysEventHandler(string yhteydenUusiTila);

    /// <summary>
    /// Tällä luokalla ohjataan prosessin toimintaa sekvenssin eri vaiheiden mukaan.
    /// </summary>
    class Toimintalogiikka
    {
        // Luodaan uusi instanssi Prosessi-luokasta, jolla hallitaan sovelluksen ja simulaattorin välistä yhteyttä
        private Prosessi prosessi = new Prosessi();

        // Säätimet lämpötilan ja paineen säätämiseen
        private Saadin E100Saadin = new Saadin(true, 10);
        private Saadin V104Saadin = new Saadin(false, -0.01*0.5);

        // String, johon tallennetaan yhteyden tila merkkijonona
        private string clientStatus;

        // Boolean jolla varmistetaan, ettei prosessia käynnistää, jos ne on jo tehty eikä sitä ole sammutettu
        private bool kaynnistetty = false;

        // Boolean, johon tallennetaan tieto siitä onko prosessi käynnissä
        private bool sekvenssiKaynnissa = false;
        
        // Sekvenssin parametrit, jotka asetetaan julkisella metodilla MuutaParametreja
        private double keittoaika;
        private double keittolampotila;
        private double kyllastysaika;
        private int keittopaine;

        private int LI100SafetyLevel = 100;

        /// <summary>
        /// Event, joka nostetaan kun toimintalogiikan muistissa oleviin prosessin mitattuihin arvoihin tehdään muutoksia
        /// </summary>
        public event PaivitaTiedot paivita;
        /// <summary>
        /// Event, joka nostetaan sekvenssin vaiheen muuttuessa
        /// </summary>
        public event TilaVaihtui tilaVaihtui;
        /// <summary>
        /// Event, joka nostetaan yhteyden tilan muuttuessa
        /// </summary>
        public event YhteydenTilaVaihtui yhteydenTilaVaihtui;

        // Julkiset metodit, joita voidaan kutsua käyttöliittymästä
        
        /// <summary>
        /// Tällä metodilla käsketään prosessia ottamaan yhteys simulaattoriin, jos yhteyttä ei vielä ole.
        /// Jos sekvenssin tila yhteyttä muodostaessa on jokin muu kuin Impregnation, pysäytetään sekvenssi.
        /// </summary>
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
                        PysaytaSekvenssi();
                    }
                }
            }
            if (!kaynnistetty)
            {
                // Tilataan event prosessin mittauksien muuttumisesta ja määritellään sille event handler
                prosessi.MikaMuuttui += new OmaEventHandler(PaivitaArvot);
                kaynnistetty = true;
            }
        }

        

        // Käynnistetään sekvenssi
        /// <summary>
        /// Tällä metodilla luodaan oma thread sekvenssille ja käynnistetään sekvenssi
        /// </summary>
        public void KaynnistaSekvenssi()
        {
            sekvenssiKaynnissa = true;

            // Muutetaan parametrit koekäyttöä varten tämän kautta
            MuutaParametreja(20, 25, 15, 250);

            // Tulostetaan muutetut parametrit outputiin testitarkoituksessa
            Trace.WriteLine(keittopaine);
            Trace.WriteLine(keittoaika);
            Trace.WriteLine(keittolampotila);
            Trace.WriteLine(kyllastysaika);

            Thread thread3 = new Thread(Kaynnista);
            thread3.Start();
        }
        /// <summary>
        /// Metodi palauttaa toimintalogiikan muistista halutun parametrin arvon. Jos parametria ei ole olemassa muistissa, funktio palauttaa -1.
        /// </summary>
        /// <param name="v">Mitattavan arvon nimi merkkijonona</param>
        /// <returns>Mitattava arvo doublena, jos se on olemassa. Jos sitä ei ole olemassa, palautetaan -1</returns>
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
        /// <summary>
        /// Metodi palauttaa toimintalogiikan muistista halutun parametrin arvon. Jos parametria ei ole olemassa muistissa, funktio palauttaa -1.
        /// </summary>
        /// <param name="v">Mitattavan arvon nimi merkkijonona</param>
        /// <returns>Mitattava arvo kokonaislukuna, jos se on olemassa. Jos sitä ei ole olemassa, palautetaan -1</returns>
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
        /// <summary>
        /// Metodi sulkee kaikki venttiilit, pysäyttää pumput ja sammuttaa lämmitysvastuksen. Prosessin tila palautetaan alkutilaan.
        /// Lopuksi ilmoitetaan käyttöliittymälle invokella prosessin tilan muuttumisesta.
        /// </summary>
        public void PysaytaSekvenssi()
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
        
        /// <summary>
        /// Metodilla muutetaan sekvenssin parametrin toimintalogiikan muistiin
        /// </summary>
        /// <param name="keittoaikaA">Asetettava keittoaika doublena</param>
        /// <param name="keittolampotilaA">Asetettava keittolämpötila doublena</param>
        /// <param name="kyllastysaikaA">Asetettava kyllästysaika doublena</param>
        /// <param name="keittopaineA">Asetettava keittopaine doublena</param>
        public void MuutaParametreja(double keittoaikaA, double keittolampotilaA, double kyllastysaikaA, int keittopaineA)
        {
            this.keittoaika = keittoaikaA;
            this.keittolampotila = keittolampotilaA;
            this.kyllastysaika = kyllastysaikaA;
            this.keittopaine = keittopaineA;
        }

        // Yksityiset metodit alkavat

        private void YhteysMuuttui(string yhteydenUusiTila)
        {
            clientStatus = yhteydenUusiTila;

            if (clientStatus == "Disconnected")
            {
                PysaytaSekvenssi();
            }

            YhteydenTilaVaihtui handler = yhteydenTilaVaihtui;
            handler?.Invoke(clientStatus);
        }

        // Metodi, jolla kutsutaan prosessin seuraavaa vaihetta nykyisen tilan perusteella
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

        // Sekvenssin runko, operaatiot ovat omissa metodeissaan

        // Sekvenssin eri vaiheet on toteutettu tehtävänannon kanssa annetun PFC-kaavion mukaisesti, käyttäen PFC-kaavion mukaisia operaatioita
        // Sekvenssin vaiheiden nimet ja eri operaatioiden nimet noudattavat PFC-kaaviota

        // Jokaisen sekvenssin vaiheen aluksi nostetaan event sekvenssin tilan muuttumisesta ja ilmoitetaan siitä invokella käyttöliittymälle

        // Metodilla toteutetaan Impregration PFC-kaavion mukaisesti
        private void Impregnation() 
        {
            TilaVaihtui handler = tilaVaihtui;
            handler?.Invoke("Impregnation");
            
            // Luetaan kyllästysaika metodin sisäiseen muuttujaan
            double impregnationTime = kyllastysaika;
            bool LS300P = mitattavatBool["LS+300"];

            // EM2_OP1, EM5_OP1, EM3_OP2
            EM2_OP1();
            EM5_OP1();
            EM3_OP2();
            // 
            if (!LS300P) 
            {                
                while (!LS300P && sekvenssiKaynnissa == true)
                {                    
                    LS300P = mitattavatBool["LS+300"];
                    // Pidetään yhden jaksonajan viive vain siinä tapauksessa, että ylärajaa ei ole vielä saavutettu
                    if (!LS300P) 
                    {
                        Thread.Sleep(500);
                    }
                }
            }

            // EM3_OP1 
            EM3_OP1();
            Trace.WriteLine("Käynnistetään kyllästysajan laskuri");
            Trace.WriteLine(impregnationTime);
            // Pistetään laskuri rullaamaan, kun yläraja on saavutettu            
            while (impregnationTime >= 0.5 && sekvenssiKaynnissa == true)
            {                   
                impregnationTime -= 0.5;
                Thread.Sleep(500);

            }
            // Time Ti tulee täyteen
            // EM2_OP2, EM5_OP3, EM3_OP6
            EM2_OP2();
            EM5_OP3();
            EM3_OP6();

            // EM3_OP8
            EM3_OP8();
            
            // Muutetaan sekvenssin tila seuraavaan vaiheeseen vain siinä tapauksessa, että sekvenssiä ei ole keskeytetty
            if (sekvenssiKaynnissa)
            {
                // Lopuksi muutetaan tilaa isommaksi ja kutsutaan uudestaan sekvenssin käynnistämistä
                tila += 1;
                Kaynnista();
            }
        }
        // Metodilla toteutetaan Black Liquor Fill PFC-kaavion mukaisesti.
        private void BlackLiquorFill() 
        {
            TilaVaihtui handler = tilaVaihtui;
            handler?.Invoke("Black Liquor Fill");

            int LI400 = mitattavatInt["LI400"];
            // LI400 rajana on LI400 < 35 mm 

            // EM3_OP2, EM5_OP1, EM4_OP1
            EM3_OP2();
            EM5_OP1();
            EM4_OP1();

            // Jatketaan pinnankorkeuden seuraamista ja edeltäviä operaatioita, kunnes LI400 on halutussa arvossa tai sekvenssi keskeytetään
            while (LI400 > 35 && sekvenssiKaynnissa == true) 
            {
                LI400 = mitattavatInt["LI400"];
                // Pidetään yhden syklin tauko vain siinä tapauksessa, että alarajaa ei ole vielä saavutetta
                if (LI400 > 35)
                {
                    Thread.Sleep(500);
                }
            }

            // EM3_OP6, EM5_OP3, EM4_OP2
            EM3_OP6();
            EM5_OP3();
            EM4_OP2();

            // Muutetaan sekvenssin tila seuraavaan vaiheeseen vain siinä tapauksessa, että sekvenssiä ei ole keskeytetty
            if (sekvenssiKaynnissa)
            {
                // Lopuksi muutetaan tilaa isommaksi ja kutsutaan uudestaan sekvenssin käynnistämistä
                tila += 1;
                Kaynnista();
            }
        }
        // Metodilla toteutetaan White Liquor Fill PFC-kaavion mukaisesti
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
            while (LI400 < 80 && sekvenssiKaynnissa == true)
            {
                LI100Safety();
                LI400 = mitattavatInt["LI400"];
                if (LI400 < 80)
                {
                    Thread.Sleep(500);
                }
            }
            // LI400

            // EM3_OP6, EM1_OP4
            EM3_OP6();
            EM1_OP4();

            // Muutetaan sekvenssin tila seuraavaan vaiheeseen vain siinä tapauksessa, että sekvenssiä ei ole keskeytetty
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
                // Pidetään viive vain siinä tapauksessa, että keittolämpötilaa ei ole vielä saavutettu
                if (TI300 < keittolampotila)
                {
                    Thread.Sleep(500);
                }                
            }
            // TI300 lämpenee tarpeeksi
            // EM3_OP1, EM1_OP2
            EM3_OP1();
            EM1_OP2();
            
            // U1_OP1, U1_OP2
            while (cookingTime >= 0.5  && sekvenssiKaynnissa == true)
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

            // EM3_OP8
            EM3_OP8();

            // Muutetaan sekvenssin tila seuraavaan vaiheeseen vain siinä tapauksessa, että sekvenssiä ei ole keskeytetty
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
                // Pidetään viive vain jos alarajaa ei ole saavutettu
                if (LS300N)
                {
                    Thread.Sleep(500);
                }                
            }
            // LS-300 deaktivoituu
            // EM5_OP4, EM3_OP7
            EM5_OP4();
            EM3_OP7();

            // Muutetaan prosessin tila takaisin arvoon 1
            tila = 1;
            // Nostetaan event ja ilmoitetaan käyttöliittymälle invokella siitä, että prosessi on jälleen idlessä
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
                PysaytaSekvenssi();
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
                PysaytaSekvenssi();
            }
        }

        // Metodi, jota kutsutaan event handlerilla eventin noustessa. Metodilla päivitetään päivitetyt arvot toimintalogiikan tietoon
        private void PaivitaArvot(object source, MuuttuneetEvent m)
        {
            Dictionary<string, int> muuttuneet = m.PalautaArvot();
            foreach (string avain in muuttuneet.Keys)
            {
                if (muuttuneet[avain] == 0) 
                {
                    mitattavatBool[avain] = prosessi.PalautaBool(avain);
                }
                if (muuttuneet[avain] == 1)
                {
                    mitattavatDouble[avain] = prosessi.PalautaDouble(avain);
                }
                if (muuttuneet[avain] == 2)
                {
                    mitattavatInt[avain] = prosessi.PalautaInt(avain);
                }
            }
            // Lopuksi nostetaan event ja ilmoitetaan invokella käyttöliittymälle muuttuneista mittauksista
            PaivitaTiedot handler = paivita;
            handler?.Invoke();
        }

        // Prosessin tila. 1 = Impregnation, 2 = BlackLiquorFill, 3 = WhiteLiquorFill, 4 = Cooking, 5 = Discharge
        private int tila = 1;

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
