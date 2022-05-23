using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
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

namespace AnotherDraft
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MySqlConnection conn;
        MySqlDataAdapter adapter;
        DataTable table;
        int page;
        int height;
        int rowCount;

        public MainWindow()
        {
            InitializeComponent();
            Init();
            FillPage(0, false, 0, "");
        }

        private void Init()
        {
            string connStr = "server=localhost;database=draftsheme;user=root;password=1234;";
            conn = new MySqlConnection(connStr);

            adapter = new MySqlDataAdapter();
            table = new DataTable();

            page = 0;
            height = 15;

            dataDg.DataContext = table;

            conn.Open();
            string sql = "select Count(*) from material;";
            MySqlCommand cmd = new MySqlCommand(sql, conn);

            MySqlDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                rowCount = reader.GetInt32(0);
            }
            reader.Close();

            sortMaterialParameterCmb.Items.Add("Наименование");
            sortMaterialParameterCmb.Items.Add("Остаток на складе");
            sortMaterialParameterCmb.Items.Add("Стоимость");

            sql = "select Name From material_type;";
            cmd = new MySqlCommand(sql, conn);
            reader = cmd.ExecuteReader();

            filterMaterialTypeCmb.Items.Add("Все типы");
            while (reader.Read())
            {
                filterMaterialTypeCmb.Items.Add(reader.GetString(0));
            }
            conn.Close();

            rowCountLB.Content = "Общее количество записей - " + rowCount;
        }

        private void FillPage(int sortParamInd, bool isDesc, int filterParamInd, string searchNameSubstr)
        {
            try
            {
                string sortParam = getSortParam(sortParamInd);
                string filterParamWhere = "";
                string searchNameWhere = "";
                if (filterParamInd != 0)
                {
                    filterParamWhere = " AND (material.Type = " + filterParamInd + ") ";
                }

                if (searchNameSubstr != "")
                {
                    searchNameWhere = " AND (material.Name LIKE '%" + searchNameSubstr + "%') ";
                }

                conn.Open();
                string sql = @"select material.ID, material.Name as 'Name', material_type.Name as 'Type', 
                           material.ImagePath, material.Price, material.Quantity, 
                           material.MinQuantity, material.QuantityInPack, 
                           unit_of_measurement.Name as 'Unit of measurement' 
                           FROM material, material_type, unit_of_measurement
                           Where (material.Type = material_type.ID) AND 
                           (material.UnitOfMeasurementType = unit_of_measurement.ID) " +
                           filterParamWhere + searchNameWhere + " Order by " +
                           sortParam + " " + (isDesc ? " DESC " : "") + " LIMIT " + page * height + "," + height;
                MySqlCommand cmd = new MySqlCommand(sql, conn);

                adapter.SelectCommand = cmd;

                table.Clear();
                adapter.Fill(table);
                conn.Close();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        private string getSortParam(int ind)
        {
            if (ind == 0)
            {
                return "material.Name";
            }
            else if (ind == 1)
            {
                return "material.Quantity";
            }
            else if (ind == 2)
            {
                return "material.Price";
            }
            return "";
        }



        private void DefaultFillPage()
        {
            FillPage(sortMaterialParameterCmb.SelectedIndex, (bool)DescSortCB.IsChecked, filterMaterialTypeCmb.SelectedIndex, searchNameTB.Text);
        }

        private void prevPageBtn_Click(object sender, RoutedEventArgs e)
        {
            if (page > 0)
            {
                --page;
                DefaultFillPage();
            }
        }

        private void nextPageBtn_Click(object sender, RoutedEventArgs e)
        {
            if ((page + 1) * height < rowCount)
            {
                ++page;
                DefaultFillPage();
            }
        }

        private void sortMaterialParameterCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (conn.State == ConnectionState.Closed)
            {
                DefaultFillPage();
            }
        }

        private void filterMaterialTypeCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (conn.State == ConnectionState.Closed)
            {
                DefaultFillPage();
            }
        }

        private void DescSortCB_Checked(object sender, RoutedEventArgs e)
        {
            if (conn.State == ConnectionState.Closed)
            {
                DefaultFillPage();
            }
        }

        private void DescSortCB_Unchecked(object sender, RoutedEventArgs e)
        {
            if (conn.State == ConnectionState.Closed)
            {
                DefaultFillPage();
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (conn.State == ConnectionState.Closed)
            {
                DefaultFillPage();
            }
        }

        private void dataDg_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dataDg.SelectedItems.Count > 0)
            {
                changeMinQuantityBtn.Visibility = Visibility.Visible;
            }
            else
            {
                changeMinQuantityBtn.Visibility = Visibility.Hidden;
            }
        }

        private void changeMinQuantityBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<int> selectedIds = new List<int>();
                foreach (var row in dataDg.SelectedItems)
                {
                    selectedIds.Add((int)((DataRowView)row).Row.ItemArray[0]);
                }

                ChangeMinQuantityWin win = new ChangeMinQuantityWin(conn, selectedIds);

                if (win.ShowDialog() == true)
                {
                    DefaultFillPage();
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        private void dataDg_AutoGeneratedColumns(object sender, EventArgs e)
        {
            dataDg.Columns[0].Visibility = Visibility.Hidden;
        }
    }
}
