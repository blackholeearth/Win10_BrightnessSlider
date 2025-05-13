using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1
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


        private void bt1_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Test");
        }


        private void bt3_Click(object sender, RoutedEventArgs e)
        {
            var win1 = new Window1();

            win1.Show();
        }





    }
}