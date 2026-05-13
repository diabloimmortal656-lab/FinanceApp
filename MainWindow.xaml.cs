using FinanceApp.Views;
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

namespace FinanceApp
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            MainFrame.Navigate(new Auth());

                if (!System.IO.File.Exists("token.txt"))
                    return;

                string token = System.IO.File.ReadAllText("token.txt");

                var user = App.dB.Users
                    .FirstOrDefault(u => u.RememberToken == token);

                if (user == null)
                    return;

                App.CurrentUser = user;

                Dispatcher.Invoke(() =>
                {
                    MainFrame.Navigate(new Home());
                });


        }
    }
}
