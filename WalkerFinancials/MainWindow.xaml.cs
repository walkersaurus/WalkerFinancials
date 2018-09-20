using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
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
using MySql.Data;
using MySql.Data.MySqlClient;

namespace WalkerFinancials
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string dbHost = "127.0.0.1";
        string dbUser = "root";
        string dbpw;
        string dbID = "Walker_Financials";

        public string DbHost { get => dbHost; set => dbHost = value; }
        public string DbUser { get => dbUser; set => dbUser = value; }
        public string Dbpw { get => dbpw; set => dbpw = value; }
        public string DbID { get => dbID; set => dbID = value; }

        public MainWindow()
        {
            InitializeComponent();

            //Set default HUD settings
            BudgetHudRadio.IsChecked = true;
            MoHud.Text = Convert.ToString(DateTime.Now.Month);
            YrHud.Text = Convert.ToString(DateTime.Now.Year);
            dbServer.Text = DbHost;
            dbName.Text = DbUser;

            if (LoginToDb())
            {
                // Call BtnUpdateHUD to populate the HUD at startup.
                UpdateHUD(DbHost, DbUser, Dbpw, DbID);
            }
            else
            {
                MessageBox.Show("Not logging in. Buhbye");
                Close();
            }
            ShowTotals();
        }

        private void ShowTotals()
        {
            /***This method connects to MySQL db, queries total income for the month, total expenses for the month, and calculates net cash based on totals***/

            //Connect to db, return error if unable to connect
            MySqlConnection conn = Query.ConnToDB(DbHost, DbUser, Dbpw, DbID, true);
            if (conn == null)
            {
                MessageBox.Show("Unable to show totals due to no MySQL connection");
            }

            //Queries
                //Convert Mo & Yr controls to int variables
            int mo = Convert.ToInt32(MoHud.Text);
            int yr = Convert.ToInt32(YrHud.Text);

                //Query Total Income, passing connection, month, and year arguments
            decimal totInc = Query.GetMonthlyIncome(conn, mo, yr);

                //Query Total Expenses, passing connection, month, and year arguments
            decimal totExp = Query.GetMonthlyExpense(conn, mo, yr);

            //Push query results to controls in Main
            Income.Text = totInc.ToString();
            TotalExp.Text = totExp.ToString();
            NetCash.Text = (Convert.ToDecimal(Income.Text) - Convert.ToDecimal(TotalExp.Text)).ToString();

            //At end of method, dispose of MySQL connection
            if (conn != null)
            {
                conn.Dispose();
            }
        }

        private bool LoginToDb()
        {
            //Prompt user for db password until correct password is entered
            while (true)
            {
                GetPword prompt = new GetPword(this);
                prompt.ShowDialog();

                if (!prompt.Pass)
                {
                    return false;
                }

                MySqlConnection conn = Query.ConnToDB(DbHost, DbUser, Dbpw, DbID, true);

                if (conn != null)
                {
                    conn.Dispose();
                    return true;
                }
            }
        }

        private void BtnUpdateHUD(object sender, RoutedEventArgs e)
        {
            UpdateHUD(DbHost, DbUser, Dbpw, DbID);
            ShowTotals();
        }

        private void UpdateHUD(string host, string user, string pw, string id)
        {
            //Initialize connection object by calling connToDB, which connects to WFdb.
            MySqlConnection conn = Query.ConnToDB(host, user, pw, id);
            if (conn == null)
            {
                MessageBox.Show("Unable to connect to database");
                return;
            }

            //Validate and localize inputs: budget & transaction (bools), periodYr & periodMo (ints).
            #region getInputs
            int periodYr;
            int periodMo;

            try
            {
                periodYr = Convert.ToInt32(YrHud.Text);
                periodMo = Convert.ToInt32(MoHud.Text);

                if (periodMo < 1 || periodMo > 12 || periodYr < 2018 || periodYr > DateTime.Now.Year || periodYr == DateTime.Now.Year && periodMo > DateTime.Now.Month)
                {
                    throw new InvalidOperationException();
                }
            }
            catch
            {
                MessageBox.Show("Must Enter Valid Month & Year");
                return;
            }

            bool budget = (bool)BudgetHudRadio.IsChecked;
            bool transaction = (bool)TransactionsHudRadio.IsChecked;
            #endregion

            //Populate HUD based on budget or transaction query w/ Month & Year values
            if (budget)
            {
                //Change gridHud to budget query
                Query.ShowMonthlyBudget(conn, periodMo, periodYr, ref gridHud);
            }

            if (transaction)
            {
                Query.ShowTransactions(conn, periodMo, periodYr, ref gridHud);
                
            }
            
            //Close db connection
            if(conn != null)
            {
                conn.Close();
                conn.Dispose();
            }
        }

        private void OpenTransaction()
        {
            //Instantiate dialog box
            InputTransaction trans = new InputTransaction(dbHost, DbUser, Dbpw, DbID){Owner = this};
            trans.Show();

            //Open the dialog box modally
        }

        private void GoTo_InTrans(object sender, RoutedEventArgs e)
        {
            OpenTransaction();
        }

        private void GoTo_NewBudget(object sender, RoutedEventArgs e)
        {
            OpenAddBudget();
        }

        private void OpenAddBudget()
        {
            AddBudget abWindow = new AddBudget(dbHost, DbUser, Dbpw, DbID) {Owner = this};
            abWindow.Show();
        }
    }
}
