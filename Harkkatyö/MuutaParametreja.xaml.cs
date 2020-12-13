using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Harkkatyö
{
    /// <summary>
    /// Interaction logic for MuutaParametreja.xaml
    /// </summary>
    public partial class MuutaParametreja : Window
    {
        private double tempKeittoaika;
        private double tempKeittolampotila;
        private double tempKyllastysaika;
        private int tempKeittopaine;

        //  Muuttujat, joihin kenttien arvot luetaan
        private double keittoaikaTulos;
        private double kyllastysAikaTulos;
        private double keittoLampoTulos;
        private int keittoPaineTulos;
        /// <summary>
        /// Constructor luokalle MuutaParametreja.
        /// </summary>
        public MuutaParametreja()
        {
            InitializeComponent();
            // Ikkunan initialisoinnin jälkeen luetaan väliaikaisiksi parametrien arvoiksi arvot mainwindowin muuttujista
            tempKeittoaika = MainWindow.keittoaika;
            tempKeittolampotila = MainWindow.keittolampotila;
            tempKyllastysaika = MainWindow.kyllastysaika;
            tempKeittopaine = MainWindow.keittopaine;
            KeittoPaineNykyinen.Text = tempKeittopaine.ToString();
            KeittoaikaNykyinen.Text = tempKeittoaika.ToString();
            KyllastysaikaNykyinen.Text = tempKyllastysaika.ToString();
            KeittoLampoNykyinen.Text = tempKeittolampotila.ToString();
        }

        // Metodi, jolla koitetaan parsia stringistä double. Jos se ei onnistu, palautetaan Nan
        private double LueDouble(string syote)
        {
            double luku;
            try
            {
                luku = double.Parse(syote);
            }
            catch 
            {
                luku = double.NaN;
            }
            return luku;
        }

        // "Käytä"-painikkeet. Asetetut parametrit otetaan käyttöön vain siinä tapauksessa, että ne ovat nollaa suurempia ja ovat numeroita.
        // Suurimmat sallitut asetusarvot parametreille:
        // Keittoaika 120 s
        // Kyllästysaika 120 s
        // Keittopaine 290 hPa
        // Keittolämpötila 60 C

        // Painike, jolla otetaan uusi keittopaine käyttöön
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (keittoPaineTulos > 0 && keittoPaineTulos < 290)
            {
                tempKeittopaine = keittoPaineTulos;
            }
            KeittoPaineNykyinen.Text = tempKeittopaine.ToString();
        }
        // Painike, jolla otetaan uusi keittoaika käyttöön
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (!double.IsNaN(keittoaikaTulos))
            {
                if (keittoaikaTulos > 0 && keittoaikaTulos < 120)
                {
                    tempKeittoaika = keittoaikaTulos;
                }
            }
            KeittoaikaNykyinen.Text = tempKeittoaika.ToString();
        }
        // Painike, jolla otetaan uusi kyllästysaika käyttöön
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            if (!double.IsNaN(kyllastysAikaTulos))
            {
                if (kyllastysAikaTulos > 0 && kyllastysAikaTulos < 120) 
                {
                    tempKyllastysaika = kyllastysAikaTulos;
                } 
                
            }
            KyllastysaikaNykyinen.Text = tempKyllastysaika.ToString();
        }
        // Painike, jolla otetaan uusi keittolämpötila käyttöön
        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            if (!double.IsNaN(keittoLampoTulos))
            {
                if (keittoLampoTulos > 0 && keittoLampoTulos < 60)
                {
                    tempKeittolampotila = keittoLampoTulos;
                }
                
            }
            KeittoLampoNykyinen.Text = tempKeittolampotila.ToString();
        }

        // "OK"-painike
        // Avataan messagebox ja varmistetaan, että muutetut parametrit otetaan käyttöön
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            string message = "Otetaanko muutetut parametrit käyttöön ja suljetaan ikkuna?";
            string title = "Ota parametrit käyttöön";
            MessageBoxButtons buttons = MessageBoxButtons.YesNo;
            DialogResult result = System.Windows.Forms.MessageBox.Show(message, title, buttons);
            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                MainWindow.keittoaika = tempKeittoaika;
                MainWindow.keittolampotila = tempKeittolampotila;
                MainWindow.kyllastysaika = tempKyllastysaika;
                MainWindow.keittopaine = tempKeittopaine;
                Close();
            }
        }

        // "Peruuta"-painike
        // Avataan messagebox ja varmistetaan, peruutetaanko tehdyt muutokset ja suljetaan ikkuna
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {

            string message = "Peruutetaanko muutokset ja suljetaan ikkuna?";
            string title = "Peruuta";
            MessageBoxButtons buttons = MessageBoxButtons.YesNo;
            DialogResult result = System.Windows.Forms.MessageBox.Show(message, title, buttons);
            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                Close();
            }
        }

        // Textboxit, joihin syötetään manuaalisesti uudet arvot parametreille
        // String parsitaan erillisellä metodilla automaattisesti kun kentän sisältö múuttuu
        private void TextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            string luku1Str = KeittoAikaUusi.Text;
            keittoaikaTulos = LueDouble(luku1Str);
        
            // keittoaikaTulos = double.Parse(luku1Str);
        }

        private void TextBox_TextChanged_2(object sender, TextChangedEventArgs e)
        {
            string luku2Str = KeittoPaineUusi.Text;
            // Kokeillaan, tuleeko parsimiseen käytetyltä funktiolta järkevä luku vain NaN
            try 
            {
                keittoPaineTulos = Convert.ToInt32(LueDouble(luku2Str));
            }
            // Jos tulokseksi saadaan NaN jota ei voi muuttaa intiksi, otetaan koppi exceptionista
            catch { }
        }

        private void TextBox_TextChanged_3(object sender, TextChangedEventArgs e)
        {
            string luku3Str = KeittoLampoUusi.Text;
            keittoLampoTulos = LueDouble(luku3Str); ;
        }

        private void TextBox_TextChanged_4(object sender, TextChangedEventArgs e)
        {
            string luku4Str = KyllastysaikaUusi.Text;
            kyllastysAikaTulos = LueDouble(luku4Str); ;
        }
    }
}
