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

        private void ProsessiKaynnissa() 
        {
            /*
            prosessi.MikaMuuttuiInt += new OmaEventHandlerInt(PaivitaArvotInt);
            prosessi.MikaMuuttuiDouble += new OmaEventHandlerDouble(PaivitaArvotDouble);
            prosessi.MikaMuuttuiBool += new OmaEventHandlerBool(PaivitaArvotBool);
            */

            prosessi.MikaMuuttui += new OmaEventHandler(PaivitaArvot);

            /*while (true) 
            {             
                prosessi.OmaEventDouble();
                prosessi.OmaEventInt();
                prosessi.OmaEventBool();

                Thread.Sleep(500);
            }*/

        }

        private void PaivitaArvot(object source, MuuttuneetEvent m)
        {
            DateTime time = DateTime.Now;
            Trace.WriteLine("aika: " + time.ToString("HH: mm:ss.fff") + " toimintalogiikka sai tiedon muutoksista");
            /*
            Trace.WriteLine(prosessi.PalautaDouble("TI100"));
            //throw new NotImplementedException();
            */
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
