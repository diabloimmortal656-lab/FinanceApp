using FinanceApp.Model;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data.Entity;

namespace FinanceApp.Views
{
    public partial class DebtPage : Page
    {
        private int addingType;
        private Debt selectedDebt;

        public DebtPage()
        {
            InitializeComponent();

            LoadDebts();
        }

        // ---------------- LOAD ----------------
        private void LoadDebts()
        {
            try
            {
                if (App.CurrentUser == null) return;

                DebtList.ItemsSource = null;

                DebtList.ItemsSource = App.dB.Debt
                    .Include(d => d.TypeDebt)
                    .Where(d => d.IDUser == App.CurrentUser.IDUser)
                    .OrderByDescending(d => d.DateDebt)
                    .ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // ---------------- SELECT DEBT ----------------
        private void DebtItem_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if ((sender as ListBoxItem)?.DataContext is Debt debt)
            {
                selectedDebt = debt;

                PayPopupTitle.Text = $"Оплата долга: {debt.NoteDebt}";
                txtPayAmount.Text = "";

                PayDebtPopup.Visibility = Visibility.Visible;
            }
        }

        // ---------------- CLOSE PAY POPUP ----------------
        private void ClosePayPopup(object sender, RoutedEventArgs e)
        {
            PayDebtPopup.Visibility = Visibility.Collapsed;
            DebtList.SelectedItem = null;
            selectedDebt = null;
        }

        // ---------------- APPLY PAYMENT ----------------
        private void ApplyPayment(object sender, RoutedEventArgs e)
        {
            if (selectedDebt == null)
                return;

            if (!decimal.TryParse(txtPayAmount.Text, out decimal pay) || pay <= 0)
            {
                MessageBox.Show("Введите корректную сумму");
                return;
            }

            try
            {
                var debt = App.dB.Debt.First(d => d.IDDebt == selectedDebt.IDDebt);

                decimal remaining = debt.AmountDebt - debt.PaidAmountDebt;

                if (pay > remaining)
                {
                    MessageBox.Show($"Остаток долга: {remaining}");
                    return;
                }

                // 1. обновляем долг
                debt.PaidAmountDebt += pay;

                // 2. определяем категорию закрытия
                string categoryName = debt.IDTypeDebt == 1
                    ? "Отдача долга"
                    : "Взято в долг";

                var category = App.dB.Category.FirstOrDefault(c =>
                    c.NameCategory == categoryName &&
                    c.System == 1);

                if (category == null)
                {
                    MessageBox.Show("Категория закрытия долга не найдена");
                    return;
                }

                // 3. СОЗДАЁМ ТРАНЗАКЦИЮ (ВАЖНО)
                var transaction = new FinanceApp.Model.Transaction
                {
                    AmountTransaction = pay,
                    DateTransaction = DateTime.Now,
                    NoteTransaction = "Закрытие долга: " + debt.NoteDebt,
                    IDUser = App.CurrentUser.IDUser,
                    IDCategory = category.IDCategory
                };

                App.dB.Transaction.Add(transaction);

                // 4. БАЛАНС
                if (debt.IDTypeDebt == 1)
                {
                    // ты возвращаешь долг → минус баланс
                    App.CurrentUser.Balance -= pay;
                }
                else
                {
                    // тебе возвращают долг → плюс баланс
                    App.CurrentUser.Balance += pay;
                }

                // 5. сохраняем
                App.dB.SaveChanges();

                PayDebtPopup.Visibility = Visibility.Collapsed;
                selectedDebt = null;
                LoadDebts();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка оплаты: " + ex.Message);
            }
        }

        // ---------------- OPEN ADD POPUP ----------------
        private void OpenAddDebtPopup(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                addingType = btn.Content.ToString() == "Беру в долг" ? 1 : 2;
                PopupDebtTitle.Text = btn.Content.ToString();
            }

            txtDebtAmount.Text = "";
            txtDebtNote.Text = "";

            AddDebtPopup.Visibility = Visibility.Visible;
        }

        // ---------------- CLOSE ADD POPUP ----------------
        private void CloseAddDebtPopup(object sender, RoutedEventArgs e)
        {
            AddDebtPopup.Visibility = Visibility.Collapsed;
        }

        // ---------------- ADD DEBT ----------------
        private void AddDebt(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(txtDebtAmount.Text, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Некорректная сумма");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtDebtNote.Text))
            {
                MessageBox.Show("Введите описание");
                return;
            }

            try
            {
                var debt = new Debt
                {
                    AmountDebt = amount,
                    PaidAmountDebt = 0,
                    NoteDebt = txtDebtNote.Text,
                    IDTypeDebt = addingType,
                    DateDebt = DateTime.Now,
                    IDUser = App.CurrentUser.IDUser
                };

                App.dB.Debt.Add(debt);

                string categoryName = addingType == 1
                    ? "Взято в долг"
                    : "Отдача долга";

                var category = App.dB.Category.FirstOrDefault(c =>
                    c.NameCategory == categoryName &&
                    c.System == 1);

                if (category == null)
                {
                    MessageBox.Show($"Категория '{categoryName}' не найдена");
                    return;
                }

                var transaction = new FinanceApp.Model.Transaction
                {
                    AmountTransaction = amount,
                    DateTransaction = DateTime.Now,
                    NoteTransaction = txtDebtNote.Text,
                    IDUser = App.CurrentUser.IDUser,
                    IDCategory = category.IDCategory
                };

                App.dB.Transaction.Add(transaction);

                // БАЛАНС ПРИ СОЗДАНИИ
                if (addingType == 1)
                    App.CurrentUser.Balance += amount;
                else
                    App.CurrentUser.Balance -= amount;

                App.dB.SaveChanges();

                AddDebtPopup.Visibility = Visibility.Collapsed;
                LoadDebts();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // ---------------- NAVIGATION ----------------
        private void Home_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new Home());

        private void Transaction_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new Transactions());

        private void Stats_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new Stats());

        private void CategoriesPage_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new CategoryPage());

        private void Profile_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new ProfilePage());

        private void Close(object sender, RoutedEventArgs e)
        {
            try
            {
                var file = "token.txt";

                if (System.IO.File.Exists(file))
                {
                    var token = System.IO.File.ReadAllText(file);

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
                MessageBox.Show(ex.Message);
            }
        }
    }
}