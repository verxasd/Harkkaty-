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
        private double tempKeittoaika;
        private double tempKeittolampotila;
        private double tempKyllastysaika;
        private int tempKeittopaine;

        // Muuttujat, joihin kenttien arvot luetaan
        private double keittoaikaTulos;
        private double kyllastysAikaTulos;
        private double keittoLampoTulos;
        private int keittoPaineTulos;
        public MuutaParametreja()
        {
            InitializeComponent();
            tempKeittoaika = MainWindow.keittoaika;
            tempKeittolampotila = MainWindow.keittolampotila;
            tempKyllastysaika = MainWindow.kyllastysaika;
            tempKeittopaine = MainWindow.keittopaine;
    }

        
        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            tempKeittoaika = keittoaikaTulos;

        }
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            tempKyllastysaika = kyllastysAikaTulos;
        }
        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            tempKeittolampotila = keittoLampoTulos;
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

        private void TextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            string luku1Str = KeittoAikaUusi.Text;
            keittoaikaTulos = double.Parse(luku1Str);
        }

        private void TextBox_TextChanged_2(object sender, TextChangedEventArgs e)
        {
            string luku2Str = KeittoPaineUusi.Text;
            keittoPaineTulos = int.Parse(luku2Str);
        }

        private void TextBox_TextChanged_3(object sender, TextChangedEventArgs e)
        {
            string luku3Str = KeittoLampoUusi.Text;
            keittoLampoTulos = double.Parse(luku3Str);
        }

        private void TextBox_TextChanged_4(object sender, TextChangedEventArgs e)
        {
            string luku4Str = KyllastysaikaUusi.Text;
            kyllastysAikaTulos = double.Parse(luku4Str);
        }

        private string GetDebuggerDisplay()
        {
            return ToString();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
