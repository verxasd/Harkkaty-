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
        public MuutaParametreja()
        {
            InitializeComponent();
        }

        private double tempKeittoaika = MainWindow.keittoaika;
        private double tempKeittolampotila = MainWindow.keittolampotila;
        private double tempKyllastysaika = MainWindow.kyllastysaika;
        private int tempKeittopaine = MainWindow.keittopaine;
        private void Button_Click(object sender, RoutedEventArgs e)
        {

            // Lähetä tulos käyttöliittymälle
            //double tulos = luku1Dbl * luku2Dbl;
            //String tulosStr = tulos.ToString();
            //tulosTextBox.Text = tulosStr;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //tempKeittoaika

        }
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            //tempKyllastysaika
        }
        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            //tempKeittolampotila
        }


        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            MainWindow.keittoaika = tempKeittoaika;
            MainWindow.keittolampotila = tempKeittolampotila;
            MainWindow.kyllastysaika = tempKyllastysaika;
            MainWindow.keittopaine = tempKeittopaine;
            this.Close();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            

            this.Close();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "<Pending>")]
        private void TextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            string keittoaika = null;
            //String luku1Str = luku1TextBox.Text;
            //string keittoaika = tempKeittoaika;
            string luku1TextBox = double.Parse(keittoaika);
        }

        private void TextBox_TextChanged_2(object sender, TextChangedEventArgs e)
        {
            //String luku2Str = luku2TextBox.Text;
            //int keittopaine = tempKeittopaine;
            string luku2TextBox = int.Parse(keittopaine);
        }

        private void TextBox_TextChanged_3(object sender, TextChangedEventArgs e)
        {
            //String luku3Str = luku3TextBox.Text;
            //double keittolampotila = tempKeittolampotila;
            string luku3TextBox = double.Parse(keittolampotila);
        }

        private void TextBox_TextChanged_4(object sender, TextChangedEventArgs e)
        {
            //String luku4Str = luku4TextBox.Text;
            //double kyllastysaika = tempKyllastysaika;
            string luku4TextBox = double.Parse(kyllastysaika);
        }

        private string GetDebuggerDisplay()
        {
            return ToString();
        }
    }
}
