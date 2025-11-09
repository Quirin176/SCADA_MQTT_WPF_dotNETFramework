using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
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

namespace MyWPFApp.LoginWindow
{
    /// <summary>
    /// Interaction logic for Window_Login.xaml
    /// </summary>
    public partial class Window_Login : Window
    {
        //private Window_Main window_Main;
        private string connectionString = @"Data Source=DESKTOP\SQLEXPRESS;Initial Catalog=LVTN;Integrated Security=True;Encrypt=True;TrustServerCertificate=True";
        private int privilege;
        private string role;

        public Window_Login()
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string username = txt_Username.Text;
            string password = pwd_Password.Visibility == Visibility.Visible ? pwd_Password.Password : txt_Password.Text;

            if (AuthenticateUser(username, password))
            {
                Window_Main mainWindow = new Window_Main();
                mainWindow.UserName = username;
                mainWindow.Role = role;
                mainWindow.UserPrivilege = privilege;
                mainWindow.Show();

                LogLoginActivity(username, privilege, role);

                this.Close();
            }
            else
            {
                MessageBox.Show("Invalid username or password!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool AuthenticateUser(string username, string password)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT PasswordHash, Privilege, Role FROM [User] WHERE Username = @Username";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);
                        SqlDataReader reader = cmd.ExecuteReader();

                        if (reader.Read())
                        {
                            privilege = Convert.ToInt32(reader["Privilege"]);
                            role = Convert.ToString(reader["Role"]);
                            string storedHash = reader["PasswordHash"].ToString();

                            // Verify the password
                            string hashedInput = ComputeSHA256(password);

                            return storedHash.Equals(hashedInput, StringComparison.OrdinalIgnoreCase);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database connection error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return false;
        }

        private void LogLoginActivity(string username, int privilege, string role)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "INSERT INTO LoginDiary (Username, Privilege, Role, DateTime) VALUES (@Username, @Privilege, @Role, @DateTime)";
                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);
                        cmd.Parameters.AddWithValue("@Privilege", privilege);
                        cmd.Parameters.AddWithValue("@Role", role);
                        cmd.Parameters.AddWithValue("@DateTime", DateTime.Now);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while logging the login activity: " + ex.Message);
            }
        }

        private string ComputeSHA256(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private void pwdPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (txt_Password.Visibility == Visibility.Visible)
            {
                txt_Password.Text = pwd_Password.Password;
            }
        }

        // Show/Hide Password Logic
        private void ShowPassword_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)((CheckBox)sender).IsChecked)
            {
                txt_Password.Text = pwd_Password.Password;
                txt_Password.Visibility = Visibility.Visible;
                pwd_Password.Visibility = Visibility.Collapsed;
            }
            else
            {
                pwd_Password.Password = txt_Password.Text;
                pwd_Password.Visibility = Visibility.Visible;
                txt_Password.Visibility = Visibility.Collapsed;
            }
        }

        private void Input_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Login_Click(sender, e);
            }
        }
    }
}
