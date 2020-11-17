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

        private bool kaynnissa = false;

        public static double keittoaika;
        public static double keittolampotila;
        public static int keittopaine;
        public static double kyllastysaika;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!kaynnissa) 
            {
                logiikka.muutaParametreja(keittoaika, keittolampotila, kyllastysaika, keittopaine);
                logiikka.kaynnistaSekvenssi();
                kaynnissa = true;
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
            }
            
        }
    }
}
