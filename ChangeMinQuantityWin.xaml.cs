using MySql.Data.MySqlClient;
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

namespace AnotherDraft
{
    /// <summary>
    /// Логика взаимодействия для ChangeMinQuantityWin.xaml
    /// </summary>
    public partial class ChangeMinQuantityWin : Window
    {
        MySqlConnection conn;
        List<int> ids;
        public ChangeMinQuantityWin(MySqlConnection mySqlConn, List<int> materialIds)
        {
            InitializeComponent();
            conn = mySqlConn;
            ids = materialIds;
        }

        private void enterMinQuantityTb_TextInput(object sender, TextCompositionEventArgs e)
        {
            if (Char.IsDigit(e.Text, 0))
            {
                e.Handled = true;
            }
        }

        private void changeBtn_Click(object sender, RoutedEventArgs e)
        {
            conn.Open();

            string sql = "Update material set MinQuantity = " + Convert.ToInt64(enterMinQuantityTb.Text) + " Where ID = ";

            foreach (int id in ids)
            {
                MySqlCommand cmd = new MySqlCommand(sql + id, conn);
                cmd.ExecuteNonQuery();
            }
            conn.Close();

            DialogResult = true;
        }
    }
}
