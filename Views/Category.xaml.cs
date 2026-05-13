using FinanceApp.Model;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FinanceApp.Views
{
    public partial class CategoryPage : Page
    {
        private Category editingCategory = null;
        private int currentType; // 1 = доход, 2 = расход

        public CategoryPage()
        {
            var window = App.Current.MainWindow;
            window.Height = 800;
            InitializeComponent();
            LoadCategories();
        }

        // ---------------- LOAD ----------------
        private void LoadCategories()
        {
            try
            {
                IncomeList.ItemsSource = App.dB.Category
    .Where(c => c.IDTypeCategory == 1 &&
           (c.IDUser == App.CurrentUser.IDUser || c.System == 1))
    .ToList();

                ExpenseList.ItemsSource = App.dB.Category
                    .Where(c => c.IDTypeCategory == 2 &&
                           (c.IDUser == App.CurrentUser.IDUser || c.System == 1))
                    .ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки категорий: " + ex.Message);
            }
        }

        // ---------------- OPEN POPUP ----------------
        private void AddIncomeCategory(object sender, RoutedEventArgs e)
        {
            currentType = 1;
            OpenPopup(null);
        }

        private void AddExpenseCategory(object sender, RoutedEventArgs e)
        {
            currentType = 2;
            OpenPopup(null);
        }

        private void OpenPopup(Category cat)
        {
            editingCategory = cat;

            if (cat != null)
            {
                bool usedInTransactions = App.dB.Transaction.Any(t =>
                    t.IDCategory == cat.IDCategory);

                btnDeleteCategoryPopup.IsEnabled = !usedInTransactions;
            }

            if (cat != null)
            {
                PopupTitle.Text = "Редактировать категорию";
                txtPopupCategoryName.Text = cat.NameCategory;
                btnDeleteCategoryPopup.Visibility = Visibility.Visible;
            }
            else
            {
                PopupTitle.Text = "Добавить категорию";
                txtPopupCategoryName.Text = "";
                btnDeleteCategoryPopup.Visibility = Visibility.Collapsed;
            }

            CategoryPopupGrid.Visibility = Visibility.Visible;
        }
        private void IncomeItem_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if ((sender as ListBoxItem)?.DataContext is Category cat)
            {
                if (cat.System == 1)
                    return;

                currentType = 1;
                OpenPopup(cat);
            }
        }
        private void ExpenseItem_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if ((sender as ListBoxItem)?.DataContext is Category cat)
            {
                if (cat.System == 2)
                    return;

                currentType = 1;
                OpenPopup(cat);
            }
        }
        private void CloseCategoryPopup(object sender, RoutedEventArgs e)
        {
            CategoryPopupGrid.Visibility = Visibility.Collapsed;
            editingCategory = null;
        }

        // ---------------- SAVE ----------------
        private void SaveCategoryPopup(object sender, RoutedEventArgs e)
        {
            string name = txtPopupCategoryName.Text.Trim();

            if (string.IsNullOrEmpty(name))
                return;

            try
            {
                if (editingCategory != null)
                {
                    // РЕДАКТИРОВАНИЕ
                    var cat = App.dB.Category
                        .FirstOrDefault(c => c.IDCategory == editingCategory.IDCategory);

                    if (cat != null)
                        cat.NameCategory = name;
                }
                else
                {
                    // ДОБАВЛЕНИЕ
                    bool exists = App.dB.Category.Any(c =>
                    c.NameCategory == name &&
                    c.IDUser == App.CurrentUser.IDUser);

                    if (exists)
                    {
                        MessageBox.Show("Категория с таким названием уже существует");
                        return;
                    }
                    var newCategory = new Category
                    {
                        NameCategory = name,
                        IDTypeCategory = currentType,
                        IDUser = App.CurrentUser.IDUser,
                        System = 0
                    };

                    App.dB.Category.Add(newCategory);
                }

                App.dB.SaveChanges();

                CategoryPopupGrid.Visibility = Visibility.Collapsed;
                editingCategory = null;

                LoadCategories();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения: " + ex.Message);
            }
        }

        // ---------------- DELETE ----------------
        private void DeleteCategoryPopup(object sender, RoutedEventArgs e)
        {
            if (editingCategory == null) return;

            if (MessageBox.Show($"Удалить '{editingCategory.NameCategory}'?",
                "Удаление", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;

            try
            {
                var cat = App.dB.Category
                    .FirstOrDefault(c => c.IDCategory == editingCategory.IDCategory);

                if (cat != null)
                    App.dB.Category.Remove(cat);

                App.dB.SaveChanges();

                CategoryPopupGrid.Visibility = Visibility.Collapsed;
                editingCategory = null;

                LoadCategories();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления: " + ex.Message);
            }
        }

        // ---------------- RIGHT CLICK ----------------
        private void IncomeDataGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (IncomeList.SelectedItem is Category cat && cat.System == 0)
            {
                currentType = 1;
                OpenPopup(cat);
            }
        }

        private void ExpenseDataGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (ExpenseList.SelectedItem is Category cat && cat.System == 0)
            {
                currentType = 2;
                OpenPopup(cat);
            }
        }

        // ---------------- NAVIGATION ----------------
        private void Home_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new Home());

        private void Transaction_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new Transactions());

        private void Debt_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new DebtPage());
        private void Stats_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new Stats());
        private void Profile_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new ProfilePage());

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
                MessageBox.Show("Ошибка выхода: " + ex.Message);
            }
        }
    }
}
