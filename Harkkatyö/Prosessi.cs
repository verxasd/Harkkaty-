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

// public delegate void ProcessItemsChangedEventHandler(object source, ProcessItemChangedEventArgs args);
//public delegate void OmaEventHandlerInt(object source, MuuttuneetEventInt m);
//public delegate void OmaEventHandlerDouble(object source, MuuttuneetEventDouble m);
// public delegate void OmaEventHandlerBool(object source, MuuttuneetEventBool m);

namespace Harkkatyö
{

    class Prosessi : Mittaus
    {
        static private ConnectionParamsHolder parametrit = new ConnectionParamsHolder("opc.tcp://127.0.0.1:8087");

        private MppClient asiakas = new MppClient(parametrit);

        // public event ProcessItemsChangedEventHandler JokinMuuttui;
        /*
        // Lisätään omat event handlerit eri tietotyypeille
        public event OmaEventHandlerInt MikaMuuttuiInt;
        public event OmaEventHandlerDouble MikaMuuttuiDouble;
        public event OmaEventHandlerBool MikaMuuttuiBool;
        */
        public event OmaEventHandler MikaMuuttui;


        public void MuutaOnOff(string nimi, bool totuus)
        {
            asiakas.SetOnOffItem(nimi, totuus);
        }
        public void MuutaPumpunOhjaus(string nimi, int ohjaus)
        {
            asiakas.SetPumpControl(nimi, ohjaus);
        }
        public void MuutaVenttiilinOhjaus(string nimi, int ohjaus)
        {
            asiakas.SetValveOpening(nimi, ohjaus);
        }

        public void ClientInit()
        {
            // Initoidaan client, jonka jälkeen lisätään seurattavat arvot serverin tietoon ja tilataan event ja määritellään event handler
            asiakas.Init();
            AddSubscriptions();
            Thread thread1 = new Thread(MuutaThread);
            thread1.Start();

        }
        private void MuutaThread()
        {
            asiakas.ProcessItemsChanged += new ProcessItemsChangedEventHandler(MuutaMuuttuneet);
            // Thread.Sleep(500);
        }
        
        // Metodit Intien, boolien ja doublejen palauttamiseen
        public int PalautaInt(string nimi)
        {
            int arvo = mitattavatInt[nimi];
            return arvo;
        }
        public bool PalautaBool(string nimi)
        {
            bool arvo = mitattavatBool[nimi];
            return arvo;
        }
        public double PalautaDouble(string nimi)
        {
            double arvo = mitattavatDouble[nimi];
            return arvo;
        }
        

        // Event sille, jos intit muuttuvat
        /*public void OmaEventInt()
        {
            
            Dictionary<string, int> MuuttuneetInt = new Dictionary<string, int> { };
            foreach (string avain in mitattavatInt.Keys) 
            {
                if (mitattavatIntVanha.ContainsKey(avain))
                {
                    if (!mitattavatInt[avain].Equals(mitattavatIntVanha[avain]))
                    {
                        MuuttuneetInt.Add(avain, mitattavatInt[avain]);
                    }
                }
                else
                {
                    MuuttuneetInt.Add(avain, mitattavatInt[avain]);
                }
            }
            if (MuuttuneetInt.Count > 0) 
            {
                MikaMuuttuiInt(this, new MuuttuneetEventInt(MuuttuneetInt));
            }
            
            
        }
        // Event sille, jos doublet muuttuvat
        
        public void OmaEventDouble()
        {            
                          
            Dictionary<string, double> MuuttuneetDouble = new Dictionary<string, double> { };
            foreach (string avain in mitattavatDouble.Keys)
            {
                if (mitattavatDoubleVanha.ContainsKey(avain))
                {
                    if (!mitattavatDouble[avain].Equals(mitattavatDoubleVanha[avain]))
                    {
                        MuuttuneetDouble.Add(avain, mitattavatDouble[avain]);
                    }
                }
                else 
                {
                    MuuttuneetDouble.Add(avain, mitattavatDouble[avain]);
                }                 
            }
            if (MuuttuneetDouble.Count > 0)
            {
                MikaMuuttuiDouble(this, new MuuttuneetEventDouble(MuuttuneetDouble));
            }
            

        }
        // Event sille, jos boolit muuttuvat
        public void OmaEventBool()
        {
            
            Dictionary<string, bool> MuuttuneetBool = new Dictionary<string, bool> { };
            foreach (string avain in mitattavatBool.Keys)
            {
                if (mitattavatBoolVanha.ContainsKey(avain))
                {
                    if (!mitattavatBool[avain].Equals(mitattavatBoolVanha[avain]))
                    {
                        MuuttuneetBool.Add(avain, mitattavatBool[avain]);
                    }
                }
                else
                {
                    MuuttuneetBool.Add(avain, mitattavatBool[avain]);
                }
            }
            if (MuuttuneetBool.Count > 0)
            {
                MikaMuuttuiBool(this, new MuuttuneetEventBool(MuuttuneetBool));
            }
            

        }*/

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
            if (Muuttuneet.Count > 0)
            {
                MikaMuuttui(this, new MuuttuneetEvent(Muuttuneet));
                Muuttuneet.Clear();
            }
        }



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

            // Tilataan event käytettävälle event handlerille
            // asiakas.ProcessItemsChanged += JokinMuuttui;

        }

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
                    // mitattavatBoolVanha = mitattavatBool;
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

                    // mitattavatDoubleVanha = mitattavatDouble;
                    mitattavatDouble[itemName] = arvo;

                    // Kirjotoitetaan double outputiin, tällä hetkellä mukana vain testaustarkoituksessa
                    // Trace.WriteLine( "uusi " + mitattavatDouble["TI100"].ToString() + " vanha " + mitattavatDoubleVanha["TI100"].ToString());
                    
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
            /*
            OmaEventBool();
            OmaEventDouble();
            OmaEventInt();
            */

            OmaEvent();

            Thread.Sleep(500);
        }
        /*
        // Säiliöiden pinnankorkeudet
        private int LI100;
        private int LI200;
        private int LI400;

        // Pintavahdit
        private bool LS200N;
        private bool LS300N;
        private bool LS300P;

        // Pumppujen esivalinta
        private bool P100_P200_PRESET;

        // Pumppujen tilat
        private int P100;
        private int P200;

        // Mitattavat lämpötilat
        private int TI100;
        private int TI300;

        // Mitattavat paineet
        private int PI300;

        // Virtaus ennen P100
        private double FI100;

        // T100 pintahälytys
        private bool LA100;
        */

        // Dictit, joissa on eri tyyppiset mitattavat arvot

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

        /*
        // Sulkuventtiilien tilat
        private bool V101;
        private bool V103;

        private bool V201;
        private bool V202;
        private bool V203;
        private bool V204;

        private bool V301;
        private bool V302;
        private bool V303;
        private bool V304;

        private bool V401;
        private bool V402;
        private bool V403;
        private bool V404;

        // Säätöventtiilien tilat
        private int V102;
        private int V104;
        */

    }
}
