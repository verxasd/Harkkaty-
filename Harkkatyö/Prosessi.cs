using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnifiedAutomation.UaBase;
using UnifiedAutomation.UaClient;
using Tuni.MppOpcUaClientLib;
using System.Windows.Documents;
using System.Windows;
using System.Diagnostics;
using static Tuni.MppOpcUaClientLib.MppClient;
using Harkkatyö;
using System.Threading;

namespace Harkkatyö
{
    /// <summary>
    /// Tällä luokalla otetaan yhteys simulaattoriin, luetaan simulaattorilla muuttuneet arvot luokan omaan muistiin ja ilmoitetaan toimintalogiikalle,
    /// jos yhteyden tilassa tai mitattavissa arvoissa tapahtuu muutoksia
    /// </summary>
    class Prosessi : Mittaus
    {
        // Muuttuja, johon yhteysparametrit tallennetaan
        static private ConnectionParamsHolder parametrit = new ConnectionParamsHolder("opc.tcp://127.0.0.1:8087");

        // Alustetaan asiakas-client
        private MppClient asiakas;

        /// <summary>
        /// Event, joka nostetaan kun tämän luokan muistissa olevat mitattavat suureet muuttuvat
        /// </summary>
        public event OmaEventHandler MikaMuuttui;
        /// <summary>
        /// Event, joka nostetaan yhteyden tilan muuttuessa
        /// </summary>
        public event YhteysEventHandler YhteysMuuttui;

        // Merkkijono, mihin tallennetaan yhteyden tila
        private string yhteydenTila = String.Empty;

        // Boolean, mihin tallennetaan tieto siitä, onko client init kutsuttu jo
        private bool initKutsuttu;

        // Metodi, jota kutsutaan kun yhteyden tila muuttuu. Lopuksi nostetaan event, jolla ilmoitetaan tässä tapauksessa yhteyden tilan muuttumisesta toimintalogiikalle
        private void YhteydenTilaMuuttui(object source, ConnectionStatusEventArgs args) 
        {
            ConnectionStatusInfo muuttunutYhteydenTila = args.StatusInfo;
            yhteydenTila = muuttunutYhteydenTila.FullStatusString;
            if (yhteydenTila == null)
            {
                yhteydenTila = "Unknown";
            }
            // Jos yhteys katkeaa tai simulaattori suljetaan ohjelman ollessa käynnissä, poistetaan asiakas-client ja muutetaan initKutsuttu alkuarvoonsa
            if (yhteydenTila == "ConnectionErrorClientReconnect")
            {
                asiakas.Dispose();
                initKutsuttu = false;
            }
            YhteysMuuttui(yhteydenTila);
        }
        /// <summary>
        /// Tällä metodilla otetaan yhteys prosessiin, jos yhteyttä ei vielä ole olemassa
        /// </summary>
        public void YhdistaProsessiin()
        {
            // Jos yhteys katkesi eikä siihen ole ehditty vielä reagoimaan, poistetaan asiakas-client ja muutetaan initKutsuttu alkuarvoonsa
            if (yhteydenTila == "ConnectionErrorClientReconnect")
            {
                asiakas.Dispose();
                initKutsuttu = false;
            }
            else
            {
                // Jos yhteyttä ei ole tai sitä ei olle muodostamassa, luodaan uusi asiakas, tilataan eventit sen tilan muutoksista ja kutsutaan init
                if (yhteydenTila != "Connected" || yhteydenTila != "Connecting")
                {
                    asiakas = new MppClient(parametrit);
                    asiakas.ConnectionStatus += new ConnectionStatusEventHandler(YhteydenTilaMuuttui);

                    if (!initKutsuttu)
                    {
                        ClientInit();
                        initKutsuttu = true;
                    }


                }
            }           
        }

        // Metodit, joilla ohjataan prosessin eri toimilaitteita. Metodit eivät tee kutsuttaessa mitään, jos prosessiin ei ole yhteyttä.
        /// <summary>
        /// Metodi muuttaa totuusarvolla ohjattavan toimilaitteen tilan halutusti
        /// </summary>
        /// <param name="nimi">Toimilaitteen nimi merkkijonona</param>
        /// <param name="totuus">Toimilaitteen ohjaus totuusarvona</param>
        public void MuutaOnOff(string nimi, bool totuus)
        {
            if (yhteydenTila == "Connected")
            {
                asiakas.SetOnOffItem(nimi, totuus);
            }
        }
        /// <summary>
        /// Metodi muuttaa pumpun ohjauksen
        /// </summary>
        /// <param name="nimi">Pumpun nimi merkkijonona</param>
        /// <param name="ohjaus">Ohjaus kokonaislukuna, sallittu ohjaus kuuluu välille 0-100</param>
        public void MuutaPumpunOhjaus(string nimi, int ohjaus)
        {
            if (yhteydenTila == "Connected")
            {
                asiakas.SetPumpControl(nimi, ohjaus);
            }
                
        }
        /// <summary>
        /// Metodi muuttaa venttiilin ohjauksen
        /// </summary>
        /// <param name="nimi">Venttiilin nimi merkkijonona</param>
        /// <param name="ohjaus">Ohjaus kokonaislukuna, sallittu ohjaus kuuluu välille 0-100</param>
        public void MuutaVenttiilinOhjaus(string nimi, int ohjaus)
        {
            if (yhteydenTila == "Connected") 
            {
                asiakas.SetValveOpening(nimi, ohjaus);
            }
                
        }
        /// <summary>
        /// Metodi initoi clientin
        /// </summary>
        public void ClientInit()
        {
            // Initoidaan client, jonka jälkeen lisätään seurattavat arvot serverin tietoon
            try
            {
                asiakas.Init();
                AddSubscriptions();
                // Tilataan event seurattavien arvojen muutoksesta ja määritellään event handler
                asiakas.ProcessItemsChanged += new ProcessItemsChangedEventHandler(MuutaMuuttuneet);
            }
            catch (Exception)
            {
                // Jos yhdistämisessä on ongelmia, tulostetaan siitä ilmoitus debug-outputiin
                Trace.WriteLine("häikkää yhdistämisessä");
            }
        }
        
        /// <summary>
        /// Metodi palauttaa halutun kokonaislukumuotoisen mittaustuloksen
        /// </summary>
        /// <param name="nimi">Mittauksen nimi merkkijonona</param>
        /// <returns>Mittauksen arvo kokonaislukuna</returns>
        public int PalautaInt(string nimi)
        {
            int arvo = mitattavatInt[nimi];
            return arvo;
        }
        /// <summary>
        /// Metodi palauttaa halutun totuusarvomuotoisen mittaustuloksen
        /// </summary>
        /// <param name="nimi">Mittauksen nimi merkkijonona</param>
        /// <returns>Mittauksen totuusarvo</returns>
        public bool PalautaBool(string nimi)
        {
            bool arvo = mitattavatBool[nimi];
            return arvo;
        }
        /// <summary>
        /// Metodi palauttaa doublemuotoisen mittaustuloksen
        /// </summary>
        /// <param name="nimi">Mittuaksen nimi merkkijonona</param>
        /// <returns>Mittauksen arvo doublena</returns>
        public double PalautaDouble(string nimi)
        {
            double arvo = mitattavatDouble[nimi];
            return arvo;
        }
        /// <summary>
        /// Metodi, jota kutsutaan kun prosessin muuttuneet arvot on muutettu tämän luokan muistiin. Tarkastetaan kaikki muuttuneet arvot,
        /// jonka jälkeen tallennetaan ne dictionaryyn, missä avain on mittauksen nimi ja arvo int, joka kuvaa mittauksen muotoa. Int = 0, double = 1 ja bool = 2.
        /// </summary>
        public void OmaEvent()
        {
            Dictionary<string, int> Muuttuneet = new Dictionary<string, int> { };
            foreach (string avain in mitattavatBool.Keys)
            {
                if (mitattavatBoolVanha.ContainsKey(avain))
                {
                    if (!mitattavatBool[avain].Equals(mitattavatBoolVanha[avain]))
                    {
                        Muuttuneet.Add(avain, 0);
                    }
                }
                else
                {
                    Muuttuneet.Add(avain, 0);
                }
            }
            foreach (string avain in mitattavatDouble.Keys)
            {
                if (mitattavatDoubleVanha.ContainsKey(avain))
                {
                    if (!mitattavatDouble[avain].Equals(mitattavatDoubleVanha[avain]))
                    {
                        Muuttuneet.Add(avain, 1);
                    }
                }
                else
                {
                    Muuttuneet.Add(avain, 1);
                }
            }
            foreach (string avain in mitattavatInt.Keys)
            {
                if (mitattavatIntVanha.ContainsKey(avain))
                {
                    if (!mitattavatInt[avain].Equals(mitattavatIntVanha[avain]))
                    {
                        Muuttuneet.Add(avain, 2);
                    }
                }
                else
                {
                    Muuttuneet.Add(avain, 2);
                }
            }
            // Jos muuttuneet arvot sisältävä dictionary ei ole tyhjä, nostetaan event MikaMuuttui ja annetaan sille event argumenteina 
            // dictionary muuttuneet, jonka jälkeen tyhjennetään dictionary muuttuneet
            if (Muuttuneet.Count > 0)
            {
                MikaMuuttui(this, new MuuttuneetEvent(Muuttuneet));
                Muuttuneet.Clear();
            }
        }
        // Metodilla tilataan halutut prosessin mitattavat arvot
        private void AddSubscriptions()
        {
            // Lisätään mitattavien asioiden subscriptionit
            
            // Mitattavat doublet
            asiakas.AddToSubscription("TI100");
            asiakas.AddToSubscription("TI300");
            asiakas.AddToSubscription("FI100");

            // Mitattavat booleanit
            asiakas.AddToSubscription("LA+100");
            asiakas.AddToSubscription("LS-200");
            asiakas.AddToSubscription("LS-300");
            asiakas.AddToSubscription("LS+300");

            // Mitattavat intit
            asiakas.AddToSubscription("LI100");
            asiakas.AddToSubscription("LI200");
            asiakas.AddToSubscription("LI400");
            asiakas.AddToSubscription("PI300");
        }

        // Metodilla muutetaan prosessin muuttuneet mittaustulokset tämän luokan omaan muistiin, jonka lisäksi tallennetaan vanhat arvot toiseen dictionaryyn.
        // Lopuksi nostetaan event siitä, että prosessissa on tapahtunut muutoksia.
        // Metodia kutsutaan event handlerilla, kun prosessin mitattavissa kohteissa tapahtuu muutoksia
        private void MuutaMuuttuneet(object source, ProcessItemChangedEventArgs args)
        {
            // Metodi, jota event handler kutsuu kun event nousee
            // Metodilla muutetaan dicteihin päivitetyt arvot
            Dictionary<string, MppValue> kaikkiMuuttuneet = args.ChangedItems;

            foreach (string itemName in kaikkiMuuttuneet.Keys)
            {
                MppValue item = kaikkiMuuttuneet[itemName];

                if (item.ValueType == MppValue.ValueTypeType.Bool)
                {
                    bool arvo = (bool)item.GetValue();
                    if (mitattavatBoolVanha.ContainsKey(itemName))
                    {
                        mitattavatBoolVanha[itemName] = mitattavatBool[itemName];
                    }
                    else 
                    {
                        mitattavatBoolVanha.Add(itemName, mitattavatBool[itemName]);
                    }

                    mitattavatBool[itemName] = arvo;
                }
                if (item.ValueType == MppValue.ValueTypeType.Double)
                {
                    double arvo = (double)item.GetValue();

                    if (mitattavatDoubleVanha.ContainsKey(itemName))
                    {
                        mitattavatDoubleVanha[itemName] = mitattavatDouble[itemName];
                    }
                    else
                    {
                        mitattavatDoubleVanha.Add(itemName, mitattavatDouble[itemName]);
                    }


                    mitattavatDouble[itemName] = arvo;

                }
                if (item.ValueType == MppValue.ValueTypeType.Int)
                {
                    int arvo = (int)item.GetValue();

                    if (mitattavatIntVanha.ContainsKey(itemName))
                    {
                        mitattavatIntVanha[itemName] = mitattavatInt[itemName];
                    }
                    else
                    {
                        mitattavatIntVanha.Add(itemName, mitattavatInt[itemName]);
                    }

                    // mitattavatIntVanha = mitattavatInt;
                    mitattavatInt[itemName] = arvo;
                }
            }
            OmaEvent();

            Thread.Sleep(500);
        }

        // Dictit, joissa on eri tyyppiset mitattavat arvot ja dictit, joissa on mitattavien arvojen edelliset arvot

        private Dictionary<string, bool> mitattavatBoolVanha = new Dictionary<string, bool>() { };

        private Dictionary<string, bool> mitattavatBool = new Dictionary<string, bool>() {
            { "LA+100", true },
            { "LS-200", true },
            { "LS-300", true },
            { "LS+300", false }
        };

        private Dictionary<string, int> mitattavatIntVanha = new Dictionary<string, int>() { };

        private Dictionary<string, int> mitattavatInt = new Dictionary<string, int>() {
            { "LI100", 216 },
            { "LI200", 90 },
            { "LI400", 90 },
            { "PI300", 0 }
        };

        private Dictionary<string, double> mitattavatDoubleVanha = new Dictionary<string, double>() { };

        private Dictionary<string, double> mitattavatDouble = new Dictionary<string, double>() {
            { "TI100", 20 },
            { "TI300", 20 },
            { "FI100", 0 }
        };
    }
}
