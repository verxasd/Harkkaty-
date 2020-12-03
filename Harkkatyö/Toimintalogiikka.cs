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


    class Toimintalogiikka
    {
        private Prosessi prosessi = new Prosessi();
        private bool clientStatus = false;

        private bool kaynnistetty = false;

        public void kaynnistaSekvenssi()
        {
            if(!clientStatus)
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
            
            
            prosessi.MuutaOnOff("E100", true);          
        }

        private void ProsessiKaynnissa() 
        {
            prosessi.MikaMuuttuiInt += new OmaEventHandlerInt(PaivitaArvotInt);
            prosessi.MikaMuuttuiDouble += new OmaEventHandlerDouble(PaivitaArvotDouble);
            prosessi.MikaMuuttuiBool += new OmaEventHandlerBool(PaivitaArvotBool);

            while (true) 
            {             
                prosessi.OmaEventDouble = new Dictionary<string, double>() { };
                prosessi.OmaEventInt = new Dictionary<string, int>() { };
                prosessi.OmaEventBool = new Dictionary<string, bool>() { };

                Thread.Sleep(500);
            }
            
        }

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
            //throw new NotImplementedException();
        }

        static void PaivitaArvotInt(object source, MuuttuneetEventInt m)
        {
            Trace.WriteLine("päivitetään intit eventistä");
            //throw new NotImplementedException();
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

        public double T100pinta;
        public double T200pinta;
        public double T400pinta;
        public double T300paine;
        public double T300lampotila;

        public bool tila;

        private double keittoaika;
        private double keittolampotila;
        private double kyllastysaika;
        private int keittopaine;
    }
}
