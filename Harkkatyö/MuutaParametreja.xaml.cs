using System;
using System.Collections.Generic;
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

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

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

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
