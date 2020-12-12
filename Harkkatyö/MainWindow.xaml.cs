using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Harkkatyö
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public delegate void PaivitaTiedot();
    public delegate void TilaVaihtui(string tila);
    public delegate void YhteydenTilaVaihtui(string yhteydenTila);
    public partial class MainWindow : Window
    {
        
        public PaivitaTiedot paivitaDelegaatti;
        public TilaVaihtui tilaDelegaatti;
        
        private int T100korkeus;
        private int T200korkeus;
        private int T400korkeus;
        private int T300paine;
        private double T300lampo;

        private BackgroundWorker bgv1;

        public MainWindow()
        {
            InitializeComponent();
            button2.IsEnabled = false;
            button3.IsEnabled = false;


            // Muutetaan progress barit täyttymään alhaalta ylös
            T100PB.Orientation = Orientation.Vertical;
            T200PB.Orientation = Orientation.Vertical;
            T400PB.Orientation = Orientation.Vertical;
            T300PBpaine.Orientation = Orientation.Vertical;
            T300PBlampo.Orientation = Orientation.Vertical;
            

            logiikka.paivita += PaivitaMetodi;
            logiikka.tilaVaihtui += PaivitaTila;
            logiikka.yhteydenTilaVaihtui += PaivitaYhteydenTila;

            // Luodaan uusi BackgroundWorker
            bgv1 = new BackgroundWorker();
            bgv1.DoWork += Bgv1_DoWork;
            bgv1.ProgressChanged += Bgv1_ProgressChanged;
            bgv1.WorkerReportsProgress = true;
            bgv1.WorkerSupportsCancellation = true;

            
        }

        private void PaivitaYhteydenTila(string yhteydenUusiTila)
        {
            Trace.WriteLine("yhteyden tila muuttui");
            Dispatcher.BeginInvoke(new Action(() => yhteydenTila.Text = yhteydenUusiTila));
            if (yhteydenUusiTila == "Connected" && !kaynnissa)
            {
                Dispatcher.BeginInvoke(new Action(() => button2.IsEnabled = true));
                Dispatcher.BeginInvoke(new Action(() => button4.IsEnabled = false));
            }
            if (yhteydenUusiTila != "Connected")
            {
                Dispatcher.BeginInvoke(new Action(() => button2.IsEnabled = false));
                Dispatcher.BeginInvoke(new Action(() => button4.IsEnabled = true));
            }
        }

        private void PaivitaTila(string tila)
        {
            Dispatcher.BeginInvoke(new Action(() => sekvenssinTila.Text = tila));
        }

        private void PaivitaMetodi()
        {
            int muutaT100korkeus = logiikka.PalautaInt("LI100");
            int muutaT200korkeus = logiikka.PalautaInt("LI200");
            int muutaT400korkeus = logiikka.PalautaInt("LI400");
            int muutaT300paine = logiikka.PalautaInt("PI300");
            double muutaT300lampo = logiikka.PalautaDouble("TI300");

            Trace.WriteLine(muutaT100korkeus);

            if (muutaT100korkeus == -1)
            {
                muutaT100korkeus = T100korkeus;
            }
            if (muutaT200korkeus == -1)
            {
                muutaT200korkeus = T200korkeus;
            }
            if (muutaT400korkeus == -1)
            {
                muutaT400korkeus = T400korkeus;
            }
            if (muutaT300paine == -1)
            {
                muutaT300paine = T300paine;
            }
            if (muutaT300lampo == -1)
            {
                muutaT300lampo = T300lampo;
            }
            MuutaPalkit(muutaT100korkeus, muutaT200korkeus, muutaT400korkeus, muutaT300paine, muutaT300lampo);
        }

        private void MuutaPalkit(int T100, int T200, int T400, int T300P, double T300L)
        {
            // Muutetaan palkkien tiedot ja päivitetään arvot
            T100korkeus = T100;
            T200korkeus = T200;
            T400korkeus = T400;
            T300paine = T300P;
            T300lampo = T300L;
            Dispatcher.BeginInvoke(new Action(() => T100PB.Value = T100korkeus));
            Dispatcher.BeginInvoke(new Action(() => T200PB.Value = T200korkeus));
            Dispatcher.BeginInvoke(new Action(() => T400PB.Value = T400korkeus));
            Dispatcher.BeginInvoke(new Action(() => T300PBpaine.Value = T300paine));
            Dispatcher.BeginInvoke(new Action(() => T300PBlampo.Value = T300lampo));
            Dispatcher.BeginInvoke(new Action(() => T300PaineNum.Text = T300paine.ToString()));
        }

        private void Bgv1_DoWork(object sender, DoWorkEventArgs e)
        {
            logiikka.muutaParametreja(keittoaika, keittolampotila, kyllastysaika, keittopaine);
            logiikka.kaynnistaSekvenssi();
            MuutaPalkit(216, 90, 90, 0, 20);
        }

        private void Bgv1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private Toimintalogiikka logiikka = new Toimintalogiikka();

        private bool kaynnissa = false;

        public static double keittoaika;
        public static double keittolampotila;
        public static int keittopaine;
        public static double kyllastysaika;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!kaynnissa) 
            {                
                bgv1.RunWorkerAsync();
                kaynnissa = true;
                button1.IsEnabled = false;
                button2.IsEnabled = false;

                button3.IsEnabled = true;
            }
            
        }
        
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (!kaynnissa) 
            {
                MuutaParametreja muutos = new MuutaParametreja();
                muutos.Show();
            }
            
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (kaynnissa) 
            {
                logiikka.pysaytaSekvenssi();
                kaynnissa = false;
                bgv1.CancelAsync();
                button1.IsEnabled = true;
                button2.IsEnabled = true;

                button3.IsEnabled = false;
            }
            
        }

        private void T100PB_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            T100PB.Minimum = 0;
            T100PB.Maximum = 300;
            // T100PB.Value = T100korkeus;
            T100KorkeusNum.Text = T100korkeus.ToString() + " mm";
        }

        private void T200PB_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            T200PB.Minimum = 0;
            T200PB.Maximum = 400;
            // T200PB.Value = T200korkeus;
            T200KorkeusNum.Text = T200korkeus.ToString() + " mm";
        }

        private void T400PB_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            T400PB.Minimum = 0;
            T400PB.Maximum = 400;
            // T400PB.Value = T400korkeus;
            T400KorkeusNum.Text = T400korkeus.ToString() + " mm";
        }

        private void T300PBpaine_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            T300PBpaine.Minimum = 0;
            T300PBpaine.Maximum = 300;
            // T300PBpaine.Value = T300paine;
            T300PaineNum.Text = T300paine.ToString() + " hPa";
        }

        private void T300PBlampo_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            T300PBlampo.Minimum = 0;
            T300PBlampo.Maximum = 60;
            T300PBlampo.Value = T300lampo;
            T300LampoNum.Text = T300lampo.ToString("F2") + " °C";
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            logiikka.AlustaProsessi();
        }
    }
}
