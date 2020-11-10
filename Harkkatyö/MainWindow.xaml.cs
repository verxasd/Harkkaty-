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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Harkkatyö
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private Toimintalogiikka logiikka = new Toimintalogiikka();

        private int keittoaika = 0;
        private int keittolampotila = 0;
        private int keittopaine = 0;
        private int kyllastysaika = 0;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            logiikka.kaynnistaSekvenssi();
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            MuutaParametreja muutos = new MuutaParametreja();
            muutos.Show();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            logiikka.pysaytaSekvenssi();
        }
    }
}
