using FinanceApp.Model;
using FinanceApp.Views;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FinanceApp.Views
{
    public partial class Transactions : Page

    {
        public Transactions()
        {
            InitializeComponent();
            LoadTransactions();
        }

        // ---------------- LOAD ----------------
        private void LoadTransactions()
        {
            try
            {
                if (App.CurrentUser == null)
                {
                    MessageBox.Show("Пользователь не найден");
                    return;
                }

                var list = App.dB.Transaction
                    .Where(t => t.IDUser == App.CurrentUser.IDUser)
                    .OrderByDescending(t => t.DateTransaction)
                    .AsEnumerable() // LINQ to Objects
                    .Select(t => new TransactionModel
                    {
                        Date = t.DateTransaction,
                        Category = t.Category.NameCategory,
                        Note = t.NoteTransaction,
                        Amount = t.AmountTransaction,

                        Type = t.Category.IDTypeCategory == 1 ? "Доход" : "Расход",

                        AmountText = t.Category.IDTypeCategory == 1
        ? $"+{t.AmountTransaction:0.##} ₽"
        : $"-{t.AmountTransaction:0.##} ₽",

                        AmountColor = t.Category.IDTypeCategory == 1
        ? Brushes.LightGreen
        : Brushes.IndianRed
                    })
                    .ToList();

                dgTransactions.ItemsSource = list;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки транзакций: " + ex.Message);
            }
        }

        // ---------------- NAVIGATION ----------------
        private void Home_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new Home());

        private void CategoriesPage_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new CategoryPage());

        private void Debt_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new DebtPage());

        private void Stats_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new Stats());
        private void Profile_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new ProfilePage());

        // ---------------- LOGOUT ----------------
        private void Close(object sender, RoutedEventArgs e)
        {
            try
            {
                if (App.CurrentUser != null)
                {
                    var user = App.dB.Users
                        .FirstOrDefault(u => u.IDUser == App.CurrentUser.IDUser);

                    if (user != null)
                    {
                        user.RememberToken = null;
                        App.dB.SaveChanges();
                    }
                }

                System.IO.File.Delete("token.txt");

                NavigationService.Navigate(new Auth());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка выхода: " + ex.Message);
            }
        }
        public class TransactionModel
        {
            public DateTime Date { get; set; }
            public string Category { get; set; }
            public string Type { get; set; }
            public decimal Amount { get; set; }
            public string Note { get; set; }
            public string AmountText { get; set; }
            public Brush AmountColor { get; set; }
        }
    }
}
