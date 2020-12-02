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

namespace Harkkatyö
{
    delegate void ProcessItemsChangedEventHandler(object source, ProcessItemChangedEventArgs args);
    class Prosessi
    {

        static private ConnectionParamsHolder parametrit = new ConnectionParamsHolder("opc.tcp://127.0.0.1:8087");

        private MppClient asiakas = new MppClient(parametrit);

        public event ProcessItemsChangedEventHandler jokinMuuttui;

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
            asiakas.Init();
            AddSubscriptions();
        }
        private void AddSubscriptions()
        {
            // Vaihtoehdot subscriptioneiden lisäämiseen, kumpi onkaan oikea vaihtoehto

            //sasiakas.AddToSubscription("jokinMuuttui");
            
            /*
            foreach (string element in mitattavatBool.Keys)
            {
                asiakas.AddToSubscription(element);
            }
            foreach (string element in mitattavatDouble.Keys)
            {
                asiakas.AddToSubscription(element);
            }
            foreach (string element in mitattavatInt.Keys)
            {
                asiakas.AddToSubscription(element);
            }*/
        }
        private void MuutaMuuttuneet(object source, ProcessItemChangedEventArgs args)
        {
            Dictionary<string, MppValue> kaikkiMuuttuneet = args.ChangedItems;
            foreach (string itemName in kaikkiMuuttuneet.Keys)
            {
                MppValue item = kaikkiMuuttuneet[itemName];

                if ((int)item.ValueType == 0)
                {
                    bool arvo = (bool)item.GetValue();
                }
                if ((int)item.ValueType == 1)
                {
                    double arvo = (double)item.GetValue();
                }
                if ((int)item.ValueType == 2)
                {
                    int arvo = (int)item.GetValue();
                }
            }
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
        private Dictionary<string, bool> mitattavatBool = new Dictionary<string, bool>() {
            { "LS-200", false },
            { "LS-300", false },
            { "LS+400", false}
        };

        private Dictionary<string, int> mitattavatInt = new Dictionary<string, int>() {
            { "LI100", 0},
            { "LI200", 0},
            { "LI400", 0},
            { "TI100", 0},
            { "TI300", 0},
            { "PI300", 0}
        };

        private Dictionary<string, double> mitattavatDouble = new Dictionary<string, double>() {
            { "FI100", 0}
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
