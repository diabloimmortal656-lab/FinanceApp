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
using System.IO;

namespace FinanceApp.Views
{
    /// <summary>
    /// Логика взаимодействия для Auth.xaml
    /// </summary>
    public partial class Auth : Page
    {
        public Auth()
        {
            InitializeComponent();
            var window = App.Current.MainWindow;
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            window.WindowState = WindowState.Normal;
            window.WindowStyle = WindowStyle.None;
            window.Height = 500;
            window.Width = 500;
            try
            {
                App.dB = new FinanceDB();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Нет подключения к БД");
            }
            
        }
        // Автовход
        


        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = tbUser.Text;
            string password = tbPassword.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Введите логин и пароль");
                return;
            }

            var user = App.dB.Users
                .FirstOrDefault(u => u.Username == username && u.Password == password);

            if (user != null)
            {
                App.CurrentUser = user;

                // 🔹 Запомнить меня
                if (RememberMeCheckBox.IsChecked == true)
                {
                    string token = Guid.NewGuid().ToString();

                    user.RememberToken = token;
                    App.dB.SaveChanges();

                    File.WriteAllText("token.txt", token);
                }

                NavigationService.Navigate(new Home());
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль");
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Reg());
        }
    }
}
