using FinanceApp.Model;
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

namespace FinanceApp.Views
{
    /// <summary>
    /// Логика взаимодействия для Page1.xaml
    /// </summary>
    public partial class Reg : Page
    {
        public Reg()
        {
            InitializeComponent();
            var window = App.Current.MainWindow;
            window.Height = 650;
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string email = EmailTextBox.Text.Trim();
            string password = PasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;

            // ---------------- VALIDATION ----------------
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                MessageBox.Show("Заполните все поля");
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Пароли не совпадают");
                return;
            }

            try
            {
                // ---------------- CHECK EXIST ----------------
                bool userExists = App.dB.Users
                    .Any(u => u.Username == username || u.Email == email);

                if (userExists)
                {
                    MessageBox.Show("Логин или email уже используется");
                    return;
                }

                // ---------------- CREATE USER ----------------
                var newUser = new Users
                {
                    Username = username,
                    Email = email,
                    Password = password, // потом можно заменить на hash
                    Balance = 0,
                    RememberToken = null
                };

                App.dB.Users.Add(newUser);
                App.dB.SaveChanges();

                MessageBox.Show("Регистрация успешна");

                // переход на логин
                NavigationService.Navigate(new Auth());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка регистрации: " + ex.Message);
            }
        }
        private void BackToLoginButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Auth());
        }
    }
}
