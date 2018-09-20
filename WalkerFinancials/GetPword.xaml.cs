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

namespace WalkerFinancials
{

    /// <summary>
    /// Interaction logic for GetPword.xaml
    /// </summary>
    public partial class GetPword : Window
    {

        private bool pass = true;
        public bool Pass { get => pass; set => pass = value; }

        MainWindow main;

        public GetPword(MainWindow HUD)
        {
            InitializeComponent();
            main = HUD;
            pWord.Focus();
        }

        //Need to mask the text box so you can't see the password!

        private void SetPassword(object sender, RoutedEventArgs e)
        {
            //When the ok button is clicked, will attempt to set the db password in App
            main.Dbpw = pWord.Password;
            this.Close();
        }

        private void YouShallNotPass(object sender, RoutedEventArgs e)
        {
            Pass = false;
            this.Close();
        }
    }
}
