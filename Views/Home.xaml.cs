using FinanceApp.Model;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace FinanceApp.Views
{
    public partial class Home : Page
    {
        private int currentType;

        public Home()
        {
            InitializeComponent();

            if (App.CurrentUser == null)
            {
                MessageBox.Show("User is null");
                return;
            }

            var window = App.Current.MainWindow;
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            window.WindowState = WindowState.Maximized;
            window.WindowStyle = WindowStyle.SingleBorderWindow;

            LoadGreeting();
            LoadBalance();
            UpdateMonthlyStats();
        }
        private void LoadGreeting()
        {
            string[] months =
            {
        "января", "февраля", "марта", "апреля",
        "мая", "июня", "июля", "августа",
        "сентября", "октября", "ноября", "декабря"
    };

            DateTime now = DateTime.Now;

            string date =
                $"{now.Day} {months[now.Month - 1]}";

            GreetingText.Text =
                $"Добро пожаловать, {App.CurrentUser.Username}! Сегодня {date}.";
        }
        // ---------------- BALANCE ----------------
        private void LoadBalance()
        {
            try
            {
                var user = App.dB.Users
                    .FirstOrDefault(u => u.IDUser == App.CurrentUser.IDUser);

                blnc.Text = user != null
                    ? $"{user.Balance:0.##} ₽"
                    : "0 ₽";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки баланса: " + ex.Message);
            }
        }

        // ---------------- STATS ----------------
        private void UpdateMonthlyStats()
        {
            try
            {
                var now = DateTime.Now;

                var transactions = App.dB.Transaction
                    .Where(t => t.IDUser == App.CurrentUser.IDUser &&
                                t.DateTransaction.Month == now.Month &&
                                t.DateTransaction.Year == now.Year)
                    .ToList();

                decimal income = transactions
                    .Where(t => t.Category.IDTypeCategory == 1)
                    .Sum(t => (decimal?)t.AmountTransaction) ?? 0;

                decimal expense = transactions
                    .Where(t => t.Category.IDTypeCategory == 2)
                    .Sum(t => (decimal?)t.AmountTransaction) ?? 0;

                IncomeText.Text = $"+{income:0.##} ₽";
                ExpenseText.Text = $"-{expense:0.##} ₽";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка статистики: " + ex.Message);
            }
        }

        // ---------------- CATEGORIES ----------------
        private void LoadCategoriesToComboBox(int type)
        {
            var categories = App.dB.Category
                .Where(c => c.IDTypeCategory == type)
                .ToList();
            cbCategory.ItemsSource = categories;
        }

        // ---------------- LOGOUT ----------------
        private void Close(object sender, RoutedEventArgs e)
        {
            try
            {
                string file = "token.txt";

                if (System.IO.File.Exists(file))
                {
                    string token = System.IO.File.ReadAllText(file);

                    var user = App.dB.Users
                        .FirstOrDefault(u => u.RememberToken == token);

                    if (user != null)
                    {
                        user.RememberToken = null;
                        App.dB.SaveChanges();
                    }

                    System.IO.File.Delete(file);
                }

                App.CurrentUser = null;

                NavigationService.Navigate(new Auth());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Logout error: " + ex.Message);
            }
        }

        // ---------------- POPUPS ----------------
        private void OpenIncomePopup(object sender, RoutedEventArgs e)
        {
            currentType = 1;
            PopupTitle.Text = "Добавить доход";
            cbCategory.SelectedIndex = 0;
            LoadCategoriesToComboBox(currentType);

            AddIncomePopup.Visibility = Visibility.Visible;
        }

        private void OpenExpensePopup(object sender, RoutedEventArgs e)
        {
            currentType = 2;
            PopupTitle.Text = "Добавить расход";
            cbCategory.SelectedIndex = 1;
            LoadCategoriesToComboBox(currentType);

            AddIncomePopup.Visibility = Visibility.Visible;
        }

        private void ClosePopup(object sender, RoutedEventArgs e)
        {
            AddIncomePopup.Visibility = Visibility.Collapsed;
        }

        // ---------------- ADD TRANSACTION ----------------
        private void AddTranaction(object sender, RoutedEventArgs e)
        {
            if (cbCategory.SelectedItem is Category selectedCategory &&
                decimal.TryParse(txtAmount.Text, out decimal amount))
            {
                try
                {
                    var user = App.dB.Users
                        .FirstOrDefault(u => u.IDUser == App.CurrentUser.IDUser);

                    var transaction = new Model.Transaction
                    {
                        AmountTransaction = amount,
                        NoteTransaction = txtDescription.Text,
                        Category = selectedCategory,
                        IDUser = App.CurrentUser.IDUser,
                        DateTransaction = DateTime.Now
                    };

                    App.dB.Transaction.Add(transaction);

                    if (currentType == 1)
                        user.Balance += amount;
                    else
                        user.Balance -= amount;

                    App.dB.SaveChanges();

                    blnc.Text = $"{user.Balance:0.##} ₽";

                    AddIncomePopup.Visibility = Visibility.Collapsed;

                    txtAmount.Text = "";
                    txtDescription.Text = "";
                    cbCategory.SelectedItem = null;

                    LoadBalance();
                    UpdateMonthlyStats();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка добавления: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Введите сумму и выберите категорию");
            }
        }

        private void CategoriesPage_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new CategoryPage());
        private void Debt_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new DebtPage());
        private void Transaction_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new Transactions());
        private void Stats_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new Stats());
        private void Profile_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new ProfilePage());
    }
}
