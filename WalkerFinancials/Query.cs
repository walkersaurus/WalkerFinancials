using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Data;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace WalkerFinancials
{
    //This class contains static methods that run custom-made queries of the WFdb.
    class Query
    {

        public static MySqlConnection ConnToDB(string server, string userid, string dbPassword, string db, bool pwPrompt = false)
        {
            string cs = $"server={server};userid={userid};password={dbPassword};database={db}";

            MySqlConnection conn = null;

            try
            {
                conn = new MySqlConnection(cs);
                conn.Open();
                //dbServer.Text = $"Server: {conn.DataSource}";
                //dbName.Text = $"Database: {conn.Database}";

                //MessageBox.Show($"Now connected to: {conn.Database} database. Yay!");
                return conn;
            }
            catch (MySqlException ex)
            {
                if (!pwPrompt)
                {
                    MessageBox.Show($"Error: {ex.Message}");
                }
                else
                {
                    MessageBox.Show("Invalid Password");
                }
                return null;
            }
        }

        public static void AddToMonthlyBudget(MySqlConnection conn, int bMo, int bYr, string cat, decimal bAmt)
        {
            //Create insert command string
            DateTime bDate = new DateTime(bYr, bMo, 1);
            string sDate = bDate.ToString("yyyy-MM-dd");
            int catNum = GetCatNumber(conn, cat);
            string qry = $"insert into monthly_budget(b_month, b_Category, b_Amount) values('{sDate}', {catNum}, {bAmt})";

            //Prep MySQL command and insert into budget
            MySqlCommand cmd = new MySqlCommand(qry, conn);
            cmd.ExecuteNonQuery();

            //At end of method, dispose of MySQL objects
            cmd.Dispose();
        }

        //AddNewTransaction inserts a new record into the Cash_Flows table of WFdb
        public static void AddNewTransaction(MySqlConnection conn, DateTime dateTrans, decimal amountTrans, string category, string details)
        {
            //Initialize command object & connect to WFdb
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;


            //Write query to command object


            //Upload transaction to WFdb
            cmd.ExecuteNonQuery();
        }

        //ShowMonthlyBudget queries the monthly_budget table and returns the budget records for a specific month
        public static void ShowMonthlyBudget(MySqlConnection conn, int perMo, int perYr, ref DataGrid myGrid)
        {
            //Initialize string to hold query
            #region ConstructQuery
            string qry = 
                " Select " +
                $" {perMo} as Month, {perYr} as Year, " +
                " cat_name, b_Amount, " +
                $" ifnull((select sum(t_Amount) from cash_flows where month(t_date) = {perMo} and year(t_date) = {perYr} and t_category = b_Category), 0) as cat_total, " +
                $"(b_Amount - ifnull((select sum(t_Amount) from cash_flows where month(t_date) = {perMo} and year(t_date) = {perYr} and t_category = b_Category), 0)) as current_balance, " +
                "b_Notes" +
                " from monthly_budget " +
                " join budget_categories on b_Category = budget_categories.cat_id" +
                $" where month(b_month) = {perMo} and year(b_month) = {perYr}";
            #endregion

            //Create MySql Command w/ query string
            MySqlCommand cmd = new MySqlCommand(qry, conn);
            cmd.Prepare();

            //Set in-query variables
            //cmd.Parameters.AddWithValue("@Mo", perMo);
            //cmd.Parameters.AddWithValue("@Yr", perYr);

            //Run query and assign to a dataset
            MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
            DataSet ds = new DataSet();

            adapter.Fill(ds, "hudData");

            //Rebind myGrid, rebuild columns
            myGrid.DataContext = ds;
            myGrid.Columns.Clear();

            DataGridTextColumn bMonth = new DataGridTextColumn
            {
                Binding = new Binding("Month"),
                Header = "Month", 
                Width = 50,
                IsReadOnly = true,
                CanUserResize = false
            };
            myGrid.Columns.Add(bMonth);

            DataGridTextColumn bYear = new DataGridTextColumn
            {
                Binding = new Binding("Year"),
                Header = "Year",
                Width = 50,
                IsReadOnly = true,
                CanUserResize = false
            };
            myGrid.Columns.Add(bYear);

            DataGridTextColumn catName = new DataGridTextColumn
            {
                Binding = new Binding("cat_name"),
                Header = "Category",
                Width = 100,
                IsReadOnly = true,
                CanUserResize = false
            };
            myGrid.Columns.Add(catName);

            DataGridTextColumn bAmount = new DataGridTextColumn
            {
                Binding = new Binding("b_Amount"),
                Header = "Budget",
                Width = 50,
                IsReadOnly = true,
                CanUserResize = false
            };
            myGrid.Columns.Add(bAmount);

            DataGridTextColumn catTotal = new DataGridTextColumn
            {
                Binding = new Binding("cat_total"),
                Header = "Spent",
                Width = 50,
                IsReadOnly = true,
                CanUserResize = false
            };
            myGrid.Columns.Add(catTotal);

            DataGridTextColumn currBalance = new DataGridTextColumn
            {
                Binding = new Binding("current_balance"),
                Header = "Balance",
                Width = 50,
                IsReadOnly = true,
                CanUserResize = false
            };
            myGrid.Columns.Add(currBalance);

            DataGridTextColumn bNotes = new DataGridTextColumn
            {
                Binding = new Binding("b_Notes"),
                Header = "Notes",
                Width = 192,
                IsReadOnly = true,
                CanUserResize = false
            };
            myGrid.Columns.Add(bNotes);


        }

        //ShowTransactions shows all transactions that occurred in a month
        public static void ShowTransactions(MySqlConnection conn, int perMo, int perYr, ref DataGrid myGrid)
        {
            //Initialize string to hold query
            string qry =
                "select " +
                    "t_date, " +
	                "(select cat_name from budget_categories where cat_id = cash_flows.t_Category) AS category, " +
                    "t_amount, " +
                    "t_details " +
                "from cash_flows "+
                $"where month(t_date) = {perMo} and year(t_date) = {perYr} "+
                "Order by t_date desc"
            ;


            //Create MySql Command w/ query string
            MySqlCommand cmd = new MySqlCommand(qry, conn);
            cmd.Prepare();

            //Run query and assign to a dataset
            MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
            DataSet ds = new DataSet();

            adapter.Fill(ds, "hudData");

            //Rebind myGrid, rebuild columns
            myGrid.DataContext = ds;
            myGrid.Columns.Clear();

            DataGridTextColumn bDate = new DataGridTextColumn
            {
                Binding = new Binding("t_date"),
                Header = "Date",
                Width = 60,
                IsReadOnly = true,
                CanUserResize = false
            };
            myGrid.Columns.Add(bDate);

            DataGridTextColumn cat = new DataGridTextColumn
            {
                Binding = new Binding("category"),
                Header = "Category",
                Width = 100,
                IsReadOnly = true,
                CanUserResize = false
            };
            myGrid.Columns.Add(cat);

            DataGridTextColumn amt = new DataGridTextColumn
            {
                Binding = new Binding("t_amount"),
                Header = "Amount",
                Width = 50,
                IsReadOnly = true,
                CanUserResize = false
            };
            myGrid.Columns.Add(amt);

            DataGridTextColumn deets = new DataGridTextColumn
            {
                Binding = new Binding("t_details"),
                Header = "Details",
                Width = 332,
                IsReadOnly = true,
                CanUserResize = false
            };
            myGrid.Columns.Add(deets);
        }

        public static List<string> GetCategories(MySqlConnection conn)
        {
            //Initialize string to hold query
            string qry = "Select cat_name from budget_categories";

            //Create MySql Command w/ query string
            MySqlCommand cmd = new MySqlCommand(qry, conn);
            cmd.Prepare();

            //Run query and assign to a list
            MySqlDataReader reader = cmd.ExecuteReader();
            List<string> cats = new List<string>();

            while (reader.Read())
            {
                cats.Add(reader.GetString(0));
            }

            return cats;
        }

        public static int GetLastTransNumber(MySqlConnection conn)
        {
            string qry =
                "Select transaction_number from cash_flows " +
                "order by transaction_number desc " +
                "limit 1";

            MySqlCommand cmd = new MySqlCommand(qry, conn);
            cmd.Prepare();
            MySqlDataReader rdr = cmd.ExecuteReader();

            rdr.Read();
            int tNum = rdr.GetInt32(0) + 1;
            cmd.Dispose();
            rdr.Dispose();
            return tNum;
        }

        public static int GetCatNumber(MySqlConnection conn, string cat)
        {
            string qry = $"select cat_id from budget_categories where cat_name = @cat";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = qry;
            cmd.Prepare();
            cmd.Parameters.AddWithValue("@cat", cat);

            MySqlDataReader rdr = cmd.ExecuteReader();
            rdr.Read();
            int catID = rdr.GetInt32(0);

            cmd.Dispose();
            rdr.Dispose();
            return catID;
        }

        public static void UploadNewTransaction(MySqlConnection conn, int tNum, double amt, int catID, string det, string strDate)
        {
            string qry =
                            "insert into cash_flows " +
                            "Values( " +
                                $"{tNum}, {amt}, " +
                                //Replace the next part with catID integer
                                $"{catID}, " +
                                $"'{det}', '{strDate}')";

            //MessageBox.Show(qry);

            MySqlCommand cmd = new MySqlCommand(qry, conn);
            cmd.Prepare();
            int affRow = cmd.ExecuteNonQuery();
            //MessageBox.Show(Convert.ToString(affRow) + "Added");

            cmd.Dispose();
        }

        public static decimal GetMonthlyIncome(MySqlConnection conn, int bMo, int bYr)
        {
            /***This method queries the MySQL db to get the sum of all transactions in the income category and returns that value as a decimal***/

            //Create query and the decimal variable to return
            string qry = "SELECT sum(t_amount) FROM cash_flows WHERE t_Category = 11 AND month(t_date) = @Mo AND year(t_date) = @Yr";
            decimal inc = 0;

            //Run the query and assign to a decimal variable
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = qry;
            cmd.Prepare();
            cmd.Parameters.AddWithValue("@Mo", bMo);
            cmd.Parameters.AddWithValue("@Yr", bYr);

            MySqlDataReader rdr = cmd.ExecuteReader();
            rdr.Read();
            inc = rdr.GetDecimal(0);

            //Dispose cmd and rdr, then return inc
            cmd.Dispose();
            rdr.Dispose();
            return inc;
        }

        public static decimal GetMonthlyExpense(MySqlConnection conn, int bMo, int bYr)
        {
            /***This method queries the MySQL db to get the sum of all transactions not in the income category and returns that value as a decimal***/
            
            //Create query and the decimal variable to return
            string qry = "SELECT sum(t_amount) FROM cash_flows WHERE t_Category != 11 AND month(t_date) = @Mo AND year(t_date) = @Yr";
            decimal inc = 0;

            //Run the query and assign to a decimal variable
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = qry;
            cmd.Prepare();
            cmd.Parameters.AddWithValue("@Mo", bMo);
            cmd.Parameters.AddWithValue("@Yr", bYr);

            MySqlDataReader rdr = cmd.ExecuteReader();
            rdr.Read();
            inc = rdr.GetDecimal(0);

            //Dispose cmd and rdr, then return inc
            cmd.Dispose();
            rdr.Dispose();
            return inc;
        }
    }
}
