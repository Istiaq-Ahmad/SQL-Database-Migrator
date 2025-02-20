using Microsoft.Win32;
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
using System.IO;
using System.Data.SqlClient;
using System.Data;
using System.Data.OleDb;


namespace Font_Converter
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

        private void DirectoryTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            return;
        }


        private async void OnBrowseTxtFileClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Select a database",
                Filter = "All Files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                long fileSize = new FileInfo(filePath).Length; // Total file size
                long bytesRead = 0; // Track bytes read
                byte[] buffer = new byte[4096]; // Buffer for reading
                StringBuilder contentBuilder = new StringBuilder();

                // Show the loading indicator and progress
                LoadingIndicator.Visibility = Visibility.Visible;
                ProgressText.Visibility = Visibility.Visible;

                await Task.Run(() =>
                {
                    using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        int read;
                        while ((read = fs.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            contentBuilder.Append(Encoding.UTF8.GetString(buffer, 0, read));
                            bytesRead += read;

                            // Update progress on the UI thread
                            Dispatcher.Invoke(() =>
                            {
                                double progress = (double)bytesRead / fileSize * 100;
                                LoadingIndicator.Value = progress;
                                ProgressText.Text = $"{Math.Round(progress, 2)}%";
                            });
                        }
                    }
                });

                // Hide the loading indicator and progress
                LoadingIndicator.Visibility = Visibility.Collapsed;
                ProgressText.Visibility = Visibility.Collapsed;

                // Update the file content and file name
                SelectedFileName.Text = System.IO.Path.GetFileName(filePath);
                DirectoryTextBox.Text = filePath;
                SelectFileText.Text = contentBuilder.ToString();
            }
            else
            {
                DirectoryTextBox.Text = "Enter file path";
                SelectFileText.Text = "Content of the file";
            }
            return;
        }

        private void Btn_TextCopy(object sender, RoutedEventArgs e)
        {
            return;
        }

        private void Btn_InsertText(object sender, RoutedEventArgs e)
        {
            return;
        }

        private void OnConnectDB(object sender, RoutedEventArgs e)
        {
            this.dgv1.ItemsSource = null;
            var conn1 = new SqlConnection("Server=ISTIAQ-TEAM512\\SQL2019ENT;Database=Font_Converter;Integrated Security=True;");
            var sqlcmd1 = new SqlCommand
            {
                Connection = conn1,
                CommandType = CommandType.StoredProcedure,
                CommandText = "[dbo].[retrieve_info]"
            };
            var parm1 = new SqlParameter("@ProcID", "SHOW_ALL_FONTMAPING");
            sqlcmd1.Parameters.Add(parm1);

            var sqladp1 = new SqlDataAdapter(sqlcmd1);
            var ds1 = new DataSet();
            sqladp1.Fill(ds1);
            this.dgv1.ItemsSource = ds1.Tables[0].DefaultView;
            return;
        }

        private void Goto_WelcomeMenu(object sender, RoutedEventArgs e)
        {
            WelcomeMenuGrid.Visibility = Visibility.Visible;

            DatabaseFeaturesGrid.Visibility = Visibility.Collapsed;
            FontInfoFeaturesGrid.Visibility = Visibility.Collapsed;
            TextConverterGrid.Visibility = Visibility.Collapsed;
        }

        private void Goto_DatabaseFeatures(object sender, RoutedEventArgs e)
        {
            DatabaseFeaturesGrid.Visibility = Visibility.Visible;

            WelcomeMenuGrid.Visibility = Visibility.Collapsed;
            FontInfoFeaturesGrid.Visibility = Visibility.Collapsed;
            TextConverterGrid.Visibility = Visibility.Collapsed;
        }

        private void Goto_FontInfoFeatures(object sender, RoutedEventArgs e)
        {
            FontInfoFeaturesGrid.Visibility = Visibility.Visible;

            WelcomeMenuGrid.Visibility = Visibility.Collapsed;
            DatabaseFeaturesGrid.Visibility = Visibility.Collapsed;
            TextConverterGrid.Visibility = Visibility.Collapsed;
        }

        private void Goto_TextConverter(object sender, RoutedEventArgs e)
        {
            TextConverterGrid.Visibility = Visibility.Visible;
                
            WelcomeMenuGrid.Visibility = Visibility.Collapsed;
            DatabaseFeaturesGrid.Visibility = Visibility.Collapsed;
            FontInfoFeaturesGrid.Visibility = Visibility.Collapsed;
        }

        private void OnBrowseAccessExportPath(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Select a database",
                Filter = "All Files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                if (SelectedFileName != null)
                {

                    string filePath = openFileDialog.FileName;
                    AccessDBExportDirectoryTextBox.Text = filePath;

                }
                else
                {
                    DirectoryTextBox.Text = "Enter file path";
                    SelectFileText.Text = "Content of the file";
                }
            }
            return;
        }

        private void ConvertSQLDBtoAccessDB(object sender, RoutedEventArgs e)
        {
            progressBarSQLtoMDB.Visibility = Visibility.Visible;
            string s_sqlServerName = SQLServername.Text.Trim();
            string s_sqlDBName = SQLDatabasename.Text.Trim();
            string d_accessDBPath = AccessDBExportDirectoryTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(s_sqlServerName) || string.IsNullOrWhiteSpace(s_sqlDBName) || string.IsNullOrWhiteSpace(d_accessDBPath))
            {
                MessageBox.Show("Please provide inputs for SQL Server, Database, and Access File.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                string sqlConnectionString = $@"Server={s_sqlServerName};Database={s_sqlDBName};Trusted_Connection=True;";
                using (SqlConnection sqlConnection = new SqlConnection(sqlConnectionString))
                {
                    sqlConnection.Open();
                    DataTable tables = sqlConnection.GetSchema("Tables");

                    using (OleDbConnection accessConnection = new OleDbConnection($@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={d_accessDBPath};"))
                    {
                        accessConnection.Open();

                        foreach (DataRow tableRow in tables.Rows)
                        {
                            string tableName = tableRow["TABLE_NAME"].ToString();
                            if (string.IsNullOrWhiteSpace(tableName)) continue;

                            DataTable sqlTableData = new DataTable();
                            string selectSqlQuery = $"SELECT * FROM [{tableName}]";

                            using (SqlCommand sqlCommand = new SqlCommand(selectSqlQuery, sqlConnection))
                            using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand))
                            {
                                sqlDataAdapter.Fill(sqlTableData);
                            }

                            string createTableQuery = GenerateCreateTableQuery(tableName, sqlTableData);
                            using (OleDbCommand createTableCommand = new OleDbCommand(createTableQuery, accessConnection))
                            {
                                createTableCommand.ExecuteNonQuery();
                            }

                            foreach (DataRow row in sqlTableData.Rows)
                            {
                                string insertQuery = GenerateInsertQuery(tableName, sqlTableData, row);
                                using (OleDbCommand insertCommand = new OleDbCommand(insertQuery, accessConnection))
                                {
                                    insertCommand.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                }
                MessageBox.Show("Database successfully exported to Access file!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            progressBarSQLtoMDB.Visibility = Visibility.Collapsed;
        }

        private string GenerateCreateTableQuery(string tableName, DataTable dataTable)
        {
            string columns = string.Join(", ", dataTable.Columns.OfType<DataColumn>()
                .Select(col => $"[{col.ColumnName}] {GetOleDbDataType(col.DataType)}"));
            return $"CREATE TABLE [{tableName}] ({columns});";
        }

        private string GenerateInsertQuery(string tableName, DataTable dataTable, DataRow row)
        {
            string columns = string.Join(", ", dataTable.Columns.OfType<DataColumn>().Select(col => $"[{col.ColumnName}]"));
            string values = string.Join(", ", dataTable.Columns.OfType<DataColumn>()
                .Select(col => row[col] == DBNull.Value ? "NULL" : $"'{row[col].ToString().Replace("'", "''")}'"));
            return $"INSERT INTO [{tableName}] ({columns}) VALUES ({values});";
        }

        private string GetOleDbDataType(Type type)
        {
            if (type == typeof(string)) return "TEXT";
            if (type == typeof(int)) return "INTEGER";
            if (type == typeof(double)) return "DOUBLE";
            if (type == typeof(DateTime)) return "DATETIME";
            if (type == typeof(bool)) return "YESNO";
            return "TEXT"; // Default to TEXT for unknown types
        }

        private void OnBrowseAccessImportPath(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Select a database",
                Filter = "All Files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                if (SelectedFileName != null)
                {

                    string filePath = openFileDialog.FileName;
                    AccessDBImportDirectoryTextBox.Text = filePath;

                }
                else
                {
                    DirectoryTextBox.Text = "Enter file path";
                    SelectFileText.Text = "Content of the file";
                }
            }
            return;
        }

        private void ConvertAccessDBtoSQLDB(object sender, RoutedEventArgs e)
        {
            progressBarMDBtoSQL.Visibility = Visibility.Visible;
            string d_sqlServerName = D_SQLServername.Text.Trim();
            string d_sqlDBName = D_SQLDatabasename.Text.Trim();
            string s_accessDBPath = AccessDBImportDirectoryTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(d_sqlDBName) || string.IsNullOrWhiteSpace(d_sqlServerName) || string.IsNullOrWhiteSpace(s_accessDBPath))
            {
                MessageBox.Show("Please provide inputs for SQL Server, Database, and Access File.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try
            {
                
                // Connection to the Access database
                string accessConnectionString = $@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={s_accessDBPath};";
                using (OleDbConnection accessConnection = new OleDbConnection(accessConnectionString))
                {
                    accessConnection.Open();

                    // Connection to SQL Server
                    string sqlMasterConnectionString = $@"Server={d_sqlServerName};Trusted_Connection=True;";
                    using (SqlConnection sqlConnection = new SqlConnection(sqlMasterConnectionString))
                    {
                        sqlConnection.Open();

                        // Create database if it does not exist
                        string checkDBQuery = $"IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = '{d_sqlDBName}') CREATE DATABASE [{d_sqlDBName}]";
                        using (SqlCommand createDBCommand = new SqlCommand(checkDBQuery, sqlConnection))
                        {
                            createDBCommand.ExecuteNonQuery();
                        }

                        // Switch to the newly created or existing database
                        sqlConnection.ChangeDatabase(d_sqlDBName);

                        // Retrieve all tables from the Access database
                        DataTable accessTables = accessConnection.GetSchema("Tables");
                        foreach (DataRow tableRow in accessTables.Rows)
                        {
                            string tableName = tableRow["TABLE_NAME"].ToString();

                            // Skip system tables like MSysACEs
                            if (string.IsNullOrWhiteSpace(tableName) || tableName.StartsWith("MSys")) continue;

                            // Fetch data from Access table
                            DataTable accessTableData = new DataTable();
                            string selectAccessQuery = $"SELECT * FROM [{tableName}]";

                            using (OleDbCommand accessCommand = new OleDbCommand(selectAccessQuery, accessConnection))
                            using (OleDbDataAdapter accessAdapter = new OleDbDataAdapter(accessCommand))
                            {
                                accessAdapter.Fill(accessTableData);
                            }

                            // Generate SQL query to create the table
                            string createTableQuery = GenerateCreateSQLTableQuery(tableName, accessTableData);
                            using (SqlCommand createTableCommand = new SqlCommand(createTableQuery, sqlConnection))
                            {
                                createTableCommand.ExecuteNonQuery();
                            }

                            // Insert data into SQL Server table
                            foreach (DataRow row in accessTableData.Rows)
                            {
                                string insertQuery = GenerateInsertSQLQuery(tableName, accessTableData, row);
                                using (SqlCommand insertCommand = new SqlCommand(insertQuery, sqlConnection))
                                {
                                    insertCommand.ExecuteNonQuery();
                                }
                            }
                        }

                    }
                }
                MessageBox.Show("Access database successfully imported to SQL Server!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                
            }
            progressBarMDBtoSQL.Visibility = Visibility.Collapsed;
        }

        private string GenerateCreateSQLTableQuery(string tableName, DataTable dataTable)
        {
            string columns = string.Join(", ", dataTable.Columns.OfType<DataColumn>()
                .Select(col => $"[{col.ColumnName}] {GetSQLDataType(col.DataType)}"));
            return $"CREATE TABLE [{tableName}] ({columns});";
        }

        private string GenerateInsertSQLQuery(string tableName, DataTable dataTable, DataRow row)
        {
            string columns = string.Join(", ", dataTable.Columns.OfType<DataColumn>().Select(col => $"[{col.ColumnName}]"));
            string values = string.Join(", ", dataTable.Columns.OfType<DataColumn>()
                .Select(col => row[col] == DBNull.Value ? "NULL" : $"'{row[col].ToString().Replace("'", "''")}'"));
            return $"INSERT INTO [{tableName}] ({columns}) VALUES ({values});";
        }

        private string GetSQLDataType(Type type)
        {
            if (type == typeof(string)) return "NVARCHAR(MAX)";
            if (type == typeof(int)) return "INT";
            if (type == typeof(double)) return "FLOAT";
            if (type == typeof(DateTime)) return "DATETIME";
            if (type == typeof(bool)) return "BIT";
            return "NVARCHAR(MAX)"; // Default to NVARCHAR(MAX) for unknown types
        }

        private void OnBrowseAccessImportPath02(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Select a database",
                Filter = "All Files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                if (SelectedFileName != null)
                {

                    string filePath = openFileDialog.FileName;
                    AccessDBImportDirectoryTextBox02.Text = filePath;

                }
                else
                {
                    MessageBox.Show("Please provide an Access File.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            return;
        }

        private void ConvertACDBtoUACDB(object sender, RoutedEventArgs e)
        {
            return;
        }
    }
}
