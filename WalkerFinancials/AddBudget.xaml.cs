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
    /// Interaction logic for AddBudget.xaml
    /// </summary>
    public partial class AddBudget : Window
    {
        string ipHost;
        string ipUser;
        string ippw;
        string ipID;
        int bmo;
        int byr;

        public string IpHost { get => ipHost; set => ipHost = value; }
        public string IpUser { get => ipUser; set => ipUser = value; }
        public string Ippw { get => ippw; set => ippw = value; }
        public string IpID { get => ipID; set => ipID = value; }
        public int Bmo { get => bmo; set => bmo = value; }
        public int Byr { get => byr; set => byr = value; }

        public AddBudget(string a, string b, string c, string d)
        {
            //Create Window, set DbConnection parameters, set month and year, create the list to hold category tuples and add the first row
            InitializeComponent();
            IpHost = a;
            IpUser = b;
            Ippw = c;
            IpID = d;

            Bmo = DateTime.Now.Month + 1;
            Byr = DateTime.Now.Year;
            if (Bmo == 13)
            {
                Bmo = 1;
                Byr++;
            }
            MoBudg.Text = Convert.ToString(Bmo);
            YrBudg.Text = Convert.ToString(Byr);

            List<Tuple<ListBox, TextBox>> budgetList = new List<Tuple<ListBox, TextBox>>();
            AddCat(ref budgetList);
        }

        public Tuple<ListBox, TextBox> AddCat(ref List<Tuple<ListBox, TextBox>> bList)
        {
            //adjust the height of the window, the gridrow, and the scrollviewer that each cat row goes in if there are less than three cat rows in bList
            if (bList.Count < 3)
            {
                this.Height += 50;
                abGrid.RowDefinitions[2].Height = new GridLength(abGrid.RowDefinitions[2].Height.Value + 50);
                CatViewer.Height += 50;
            }

            ListBox catList = new ListBox()
            {
                Height = 25,
                Width = 100,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                ItemsSource = Cats(),
            };

            TextBox catAmt = new TextBox()
            {
                Height = 25,
                Width = 100,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            Tuple<ListBox, TextBox> catRow = new Tuple<ListBox, TextBox>(catList, catAmt);

            return catRow;
        }

        private List<string> Cats()
        {
            MySqlConnection conn = Query.ConnToDB(IpHost, IpUser, Ippw, IpID);
            List<string> cats = Query.GetCategories(conn);
            conn.Close();
            return cats;
        }
    }
}
