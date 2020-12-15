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
    ///  Event handler käyttöliittymän palkkien ja lukuarvojen muuttamiseksi.
    /// </summary>
    public delegate void PaivitaTiedot();
    /// <summary>
    /// Event handler käyttöliittymässä näkyvän sekvenssin tilan muuttamiseksi
    /// </summary>
    /// <param name="tila">Sekvenssin tila merkkijonona</param>
    public delegate void TilaVaihtui(string tila);
    /// <summary>
    /// Event handler käyttöliittymässä näkyvän yhteyden tilan muuttamiseksi
    /// </summary>
    /// <param name="yhteydenTila">Yhteyden tila merkkijonona</param>
    public delegate void YhteydenTilaVaihtui(string yhteydenTila);
    
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Yksityiset muuttujat käyttöliittymässä näytettävien palkkien ja lukuarvojen arvoille
        private int T100korkeus;
        private int T200korkeus;
        private int T400korkeus;
        private int T300paine;
        private double T300lampo;

        private MuutaParametreja muutos;

        // Backgroundworker
        private BackgroundWorker bgv1;
        /// <summary>
        /// Käyttöliittymän constructor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // Otetaan sekvenssin käynnistämisen ja keskeyttämisen mahdollistavat painikkeet pois käytöstä, jos prosessiin ei ole muodostettu yhteyttä
            button2.IsEnabled = false;
            button3.IsEnabled = false;

            // Muutetaan mitattavia arvoja kuvaavat progress barit täyttymään alhaalta ylös
            T100PB.Orientation = Orientation.Vertical;
            T200PB.Orientation = Orientation.Vertical;
            T400PB.Orientation = Orientation.Vertical;
            T300PBpaine.Orientation = Orientation.Vertical;
            T300PBlampo.Orientation = Orientation.Vertical;

            // Tilataan eventit mittuaksien muuttumisesta, sekvenssin tilan muuttumisesta, yhteyden tilan muuttumisesta
            // ja määritellään niille event handlerit
            logiikka.paivita += PaivitaMetodi;
            logiikka.tilaVaihtui += PaivitaTila;
            logiikka.yhteydenTilaVaihtui += PaivitaYhteydenTila;

            // Luodaan uusi BackgroundWorker ja määritellään metodi, mitä se käyttää kun DoWork kutsutaan
            bgv1 = new BackgroundWorker();
            bgv1.DoWork += Bgv1_DoWork;
            bgv1.WorkerSupportsCancellation = true;
            bgv1.WorkerReportsProgress = true;
            this.Closing += MainWindow_Closing;
        }
        // Event handler sille, kun pääikkuna suljetaan rastista.
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            // Jos pääikkuna suljetaan rastista, suljetaan myös ikkuna muuta parametreja jos se on auki.
            if (muutaParametrejaAuki)
            {
                muutos.Close();
                muutaParametrejaAuki = false;
            }
            // Jos sekvenssi on käynnissä, pysäytetään se ennen sovelluksen sulkemista
            if (kaynnissa)
            {
                logiikka.PysaytaSekvenssi();
            }
        }

        // Metodi, jolla päivitetään käyttöliittymässä näkyvä yhteyden tila
        private void PaivitaYhteydenTila(string yhteydenUusiTila)
        {
            // Aloitetaan invoke, jolla muutetaan käyttöliittymässä näkyvä yhteyden tila
            Dispatcher.BeginInvoke(new Action(() => yhteydenTila.Text = yhteydenUusiTila));
            // Jos sekvenssi on ei ole käynnissä ja yhteyden uusi tila on "Connected", 
            // otetaan käyttöön painike "Käynnistä sekvenssi" ja poistetaan käytöstä painike "Yhdistä prosessiin"
            if (yhteydenUusiTila == "Connected" && !kaynnissa)
            {
                Dispatcher.BeginInvoke(new Action(() => button2.IsEnabled = true));
                Dispatcher.BeginInvoke(new Action(() => button4.IsEnabled = false));
            }
            // Jos yhteys prosessiin katkeaa, otetaan painike "Käynnistä sekvenssi" pois käytöstä ja otetaan käyttöön painike "Yhdistä prosessiin"
            if (yhteydenUusiTila != "Connected")
            {
                Dispatcher.BeginInvoke(new Action(() => button2.IsEnabled = false));
                Dispatcher.BeginInvoke(new Action(() => button4.IsEnabled = true));
            }
        }
        // Metodi päivittää käyttöliittymässä näkyvän prosessin tilan invoken tapauksessa
        private void PaivitaTila(string tila)
        {
            Dispatcher.BeginInvoke(new Action(() => sekvenssinTila.Text = tila));
        }

        // Metodilla haetaan logiikalta tieto kaikista käyttöliittymässä näkyvien mittauksien arvoista, kun jokin toimintalogiikan seuraama arvo muuttuu
        private void PaivitaMetodi()
        {
            int muutaT100korkeus = logiikka.PalautaInt("LI100");
            int muutaT200korkeus = logiikka.PalautaInt("LI200");
            int muutaT400korkeus = logiikka.PalautaInt("LI400");
            int muutaT300paine = logiikka.PalautaInt("PI300");
            double muutaT300lampo = logiikka.PalautaDouble("TI300");

            // Jos toimintalogiikan palauttama tulos on -1 (haluttua mittaustulosta ei ole toimintalogiikan muistissa), käytetään sen sijasta oletusarvoa
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

        // Metodi, jolla muutetaan käyttöliittymässä näkyvien palkkien arvot.
        private void MuutaPalkit(int T100, int T200, int T400, int T300P, double T300L)
        {
            // Muutetaan palkkien tiedot ja päivitetään arvot
            T100korkeus = T100;
            T200korkeus = T200;
            T400korkeus = T400;
            T300paine = T300P;
            T300lampo = T300L;
            // Muuttamiseen käytetään invokea, koska käyttöliittymä on eri threadissa kuin backgroundworker
            Dispatcher.BeginInvoke(new Action(() => T100PB.Value = T100korkeus));
            Dispatcher.BeginInvoke(new Action(() => T200PB.Value = T200korkeus));
            Dispatcher.BeginInvoke(new Action(() => T400PB.Value = T400korkeus));
            Dispatcher.BeginInvoke(new Action(() => T300PBpaine.Value = T300paine));
            Dispatcher.BeginInvoke(new Action(() => T300PBlampo.Value = T300lampo));
            // Muutetaan myös käyttöliittymässä näkyvä säiliön T300 paine
            Dispatcher.BeginInvoke(new Action(() => T300PaineNum.Text = T300paine.ToString()));
        }
        // Metodi jota kutsutaan kun backgroundworker pistetään tekemään töitä
        private void Bgv1_DoWork(object sender, DoWorkEventArgs e)
        {
            // Muutetaan  parametrit toimintalogiikalle, käynnistetään sekvenssi ja muutetaan palkkien näyttämät arvot oletuksiksi
            logiikka.MuutaParametreja(keittoaika, keittolampotila, kyllastysaika, keittopaine);
            logiikka.KaynnistaSekvenssi();
            MuutaPalkit(216, 90, 90, 0, 20);
        }

        // Luodaan uusi instanssi luokasta toimintalogiikka
        private Toimintalogiikka logiikka = new Toimintalogiikka();

        // Tallennetaan totuusarvomuotoinen tieto siitä, onko sekvenssi käynnissä
        private bool kaynnissa = false;

        /// <summary>
        /// Keittoaika. Muutetaan ikkunan "Muuta parametreja" kautta
        /// </summary>
        public static double keittoaika;
        /// <summary>
        /// Keittolämpötila. Muutetaan ikkunan "Muuta parametreja" kautta
        /// </summary>
        public static double keittolampotila;
        /// <summary>
        /// Keittopaine. Muutetaan ikkunan "Muuta parametreja" kautta
        /// </summary>
        public static int keittopaine;
        /// <summary>
        /// Kyllästysaika. Muutetaan ikkunan muuta parametreja kautta
        /// </summary>
        public static double kyllastysaika;
        /// <summary>
        /// Boolean sille, onko ikkuna "Muuta parametreja" auki
        /// </summary>
        public static bool muutaParametrejaAuki;

        // Event handler sille, kun käyttöliittymän painiketta "Käynnistä sekvenssi" painetaan
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (muutaParametrejaAuki)
            {
                string message = "Sekvenssiä ei voida käynnistää, jos parametrien muuttaminen on kesken!";
                string title = "Virhe";
                MessageBox.Show(message, title);
            }
            // Jos sekvenssi ei ole käynnissä, käynnistetään backgroundworker ja muutetaan totuusarvo "kaynnissa" trueksi
            if (!kaynnissa && !muutaParametrejaAuki) 
            {                
                bgv1.RunWorkerAsync();
                kaynnissa = true;

                // Otetaan pois käytöstä painikkeet "Käynnistä sekvenssi" ja "Muuta parametreja"
                button1.IsEnabled = false;
                button2.IsEnabled = false;
                // Otetaan käyttöön painike "Keskeytä sekvenssi"
                button3.IsEnabled = true;
            }   
        }
        
        // Event handler sille, kun käyttöliittymän painiketta "Muuta parametreja" painetaan
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (muutaParametrejaAuki)
            {
                string message = "Olet jo muuttamassa parametreja!";
                string title = "Virhe";
                MessageBox.Show(message, title);
            }
            if (!kaynnissa && !muutaParametrejaAuki) 
            {
                // Jos sekvenssi ei ole käynnissä ja ikkuna "Muuta parametreja" ei ole auki, muutetaan bool muutaParametrejaAuki todeksi ja avataan ikkuna
                muutos = new MuutaParametreja();
                muutaParametrejaAuki = true;
                muutos.Show();
            }
        }

        // Event handler sille, kun käyttöliittymän painiketta "Pysäytä sekvenssi" painetaan
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            // Painikkeen painamisesta ei tapahdu mitään, jos sekvenssi ei ole käynnissä
            if (kaynnissa) 
            {
                // Pysäytetään sekvenssi, peruutetaan backgroundworkerin toiminta
                logiikka.PysaytaSekvenssi();
                kaynnissa = false;
                bgv1.CancelAsync();

                // Otetaan käyttöön painikkeet "Käynnistä sekvenssi" ja "Muuta parametreja"
                button1.IsEnabled = true;
                button2.IsEnabled = true;
                // Poistetaan käytöstä painike "Pysäytä sekvenssi"
                button3.IsEnabled = false;
            }
        }
        // Event handler sille, kun progress barin arvo muttuu. Määritellään palkin ylä- ja alarajat, muutetaan käyttöliittymässä näkyvä numeerinen arvo
        private void T100PB_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            T100PB.Minimum = 0;
            T100PB.Maximum = 300;
            T100KorkeusNum.Text = T100korkeus.ToString() + " mm";
        }
        // Event handler sille, kun progress barin arvo muttuu. Määritellään palkin ylä- ja alarajat, muutetaan käyttöliittymässä näkyvä numeerinen arvo
        private void T200PB_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            T200PB.Minimum = 0;
            T200PB.Maximum = 400;
            T200KorkeusNum.Text = T200korkeus.ToString() + " mm";
        }
        // Event handler sille, kun progress barin arvo muttuu. Määritellään palkin ylä- ja alarajat, muutetaan käyttöliittymässä näkyvä numeerinen arvo
        private void T400PB_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            T400PB.Minimum = 0;
            T400PB.Maximum = 400;
            T400KorkeusNum.Text = T400korkeus.ToString() + " mm";
        }
        // Event handler sille, kun progress barin arvo muttuu. Määritellään palkin ylä- ja alarajat, muutetaan käyttöliittymässä näkyvä numeerinen arvo
        private void T300PBpaine_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            T300PBpaine.Minimum = 0;
            T300PBpaine.Maximum = 300;
            T300PaineNum.Text = T300paine.ToString() + " hPa";
        }
        // Event handler sille, kun progress barin arvo muttuu. Määritellään palkin ylä- ja alarajat, muutetaan käyttöliittymässä näkyvä numeerinen arvo
        private void T300PBlampo_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            T300PBlampo.Minimum = 0;
            T300PBlampo.Maximum = 60;
            T300LampoNum.Text = T300lampo.ToString("F2") + " °C";
        }
        // Event handler sille, kun käyttöliittymässä painetaan nappia "Yhdistä prosessiin"
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            logiikka.AlustaProsessi();
        }
    }
}
