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
using MySql.Data;
using MySql.Data.MySqlClient;

namespace WalkerFinancials
{
    /// <summary>
    /// Interaction logic for InputTransaction.xaml
    /// </summary>
    public partial class InputTransaction : Window
    {
        string ipHost;
        string ipUser;
        string ippw;
        string ipID;

        public string IpHost { get => ipHost; set => ipHost = value; }
        public string IpUser { get => ipUser; set => ipUser = value; }
        public string Ippw { get => ippw; set => ippw = value; }
        public string IpID { get => ipID; set => ipID = value; }

        public InputTransaction(string a, string b, string c, string d)
        {
            InitializeComponent();
            IpHost = a;
            IpUser = b;
            Ippw = c;
            IpID = d;
            tCat.ItemsSource = Cats();
            pDate.SelectedDate = DateTime.Now.Date;
            tAmt.Focus();
        }

        private List<string> Cats()
        {
            MySqlConnection conn = Query.ConnToDB(IpHost, IpUser, Ippw, IpID);
            List<string> cats = Query.GetCategories(conn);
            conn.Close();
            return cats;
            }

        private void UpdateCF()
        {
            //Initialize connection object by calling connToDB, which connects to WFdb.
            MySqlConnection conn = Query.ConnToDB(IpHost, IpUser, Ippw, IpID);
            if (conn == null)
            {
                MessageBox.Show("Unable to connect to database");
                return;
            }

            //Validate and localize inputs: amount, category, transDate, & details
            double amount = Math.Round(Convert.ToDouble(tAmt.Text), 2);
            int tNum = Query.GetLastTransNumber(conn);
            int catID = Query.GetCatNumber(conn, tCat.Text);
            DateTime tDate = (DateTime)pDate.SelectedDate;
            string strDate = tDate.Year.ToString() + "-" + tDate.Month.ToString() + "-" + tDate.Day.ToString();
            string det = tDet.Text;

            //Upload new transaction record to db
            Query.UploadNewTransaction(conn, tNum, amount, catID, det, strDate);

            //Refresh HUD? Not sure 
            

            //Close connection at end of method
            if (conn != null)
            {
                conn.Dispose();
            }
        }

        private void updateCashFlows(object sender, RoutedEventArgs e)
        {
            UpdateCF();
            this.Close();
        }

        private void BtnCancel(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
