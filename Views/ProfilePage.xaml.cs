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
using System.Globalization;
using FinanceApp.Model;

namespace FinanceApp.Views
{
    /// <summary>
    /// Логика взаимодействия для ProfilePage.xaml
    /// </summary>
    public partial class ProfilePage : Page
    {
        private Goal currentGoal;
        public ProfilePage()
        {
            InitializeComponent();
            LoadGoal();
            LoadLastTransactions();
            LoadWindow();
            LoadForecast();
            LoadFinancialLevel();
        }

        private void UpdateWindow()
        {
            LoadGoal();
            LoadLastTransactions();
            LoadWindow();
            LoadFinancialLevel();
            LoadForecast();
        }
        private FinancialLevelResult CalculateFinancialLevel()
        {
            int userId = App.CurrentUser.IDUser;

            var transactions = App.dB.Transaction
                .Where(t => t.IDUser == userId)
                .ToList();

            decimal income = transactions
                .Where(t => t.Category.IDTypeCategory == 1)
                .Sum(t => t.AmountTransaction);

            decimal expense = transactions
                .Where(t => t.Category.IDTypeCategory == 2)
                .Sum(t => t.AmountTransaction);

            decimal balance = income - expense;

            // защита от деления на 0
            if (income <= 0)
                income = 1;

            // 1. Savings Rate (0–100)
            double savingsRate = (double)(balance / income);
            double savingsScore = Clamp(savingsRate * 100, 0, 100);

            // 2. Средние месячные расходы (по количеству месяцев активности)
            int monthsActive = GetMonthsActive();
            decimal avgMonthlyExpense = monthsActive == 0 ? expense : expense / monthsActive;

            // 3. Финансовая подушка (в месяцах жизни без дохода)
            double bufferMonths = avgMonthlyExpense == 0
                ? 0
                : (double)(balance / avgMonthlyExpense);

            double bufferScore = Clamp(bufferMonths * 20, 0, 100);

            double expenseRatio = income <= 0
                ? 1.5
                : (double)(expense / income);

            double stabilityScore;

            if (expenseRatio <= 0.5)
                stabilityScore = 100;
            else if (expenseRatio <= 0.7)
                stabilityScore = 85;
            else if (expenseRatio <= 0.9)
                stabilityScore = 60;
            else if (expenseRatio <= 1.0)
                stabilityScore = 30;
            else
                stabilityScore = 0;
            // 5. Финальный score
            double score =
                savingsScore * 0.45 +
                bufferScore * 0.35 +
                stabilityScore * 0.20;

            // 6. Уровень
            string level = GetFinancialLevel(score);

            return new FinancialLevelResult
            {
                Score = score,
                Level = level,

                CashFlowScore = stabilityScore,
                SavingsScore = savingsScore,
                BufferScore = bufferScore,

                Income = income,
                Expense = expense,
                Balance = balance,
                BufferMonths = (decimal)bufferMonths
            };
        }
        private int GetMonthsActive()
        {
            var transactions = App.dB.Transaction
                .Where(t => t.IDUser == App.CurrentUser.IDUser)
                .ToList();
            if (!transactions.Any())
                return 1;

            var first = transactions.Min(t => t.DateTransaction);
            var last = transactions.Max(t => t.DateTransaction);

            int months =
                ((last.Year - first.Year) * 12) +
                last.Month - first.Month + 1;

            return months <= 0 ? 1 : months;
        }

        private double Clamp(double value, double min, double max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
        private void LoadLastTransactions()
        {
            var data = App.dB.Transaction
    .Where(t => t.IDUser == App.CurrentUser.IDUser)
    .OrderByDescending(t => t.DateTransaction)
    .Take(5)
    .ToList()
    .Select(t => new TransactionVM
    {
        Title = t.Category.NameCategory,

        AmountText =
            (t.Category.IDTypeCategory == 1 ? "+" : "-")
            + t.AmountTransaction.ToString("0.##") + " ₽",

        AmountColor = t.Category.IDTypeCategory == 1
            ? Brushes.PaleGreen
            : Brushes.IndianRed
    })
    .ToList();

            this.DataContext = new
            {
                LastTransactions = data
            };
        }
        private void LoadWindow()
        {
            var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1);
            decimal income = App.dB.Transaction
                .Where(t =>
                    t.Category.IDTypeCategory == 1 && t.IDUser == App.CurrentUser.IDUser)
                .Sum(t => (decimal?)t.AmountTransaction) ?? 0;

            decimal expense = App.dB.Transaction
                .Where(t =>
                    t.Category.IDTypeCategory == 2 && t.IDUser == App.CurrentUser.IDUser)
                .Sum(t => (decimal?)t.AmountTransaction) ?? 0;
            decimal totalBalance = income - expense;
            txtUserName.Text = App.CurrentUser.Username;
            txtOperations.Text = App.dB.Transaction.Count(t => t.IDUser == App.CurrentUser.IDUser).ToString();
            txtBalance.Text = $"{totalBalance:0.##}";
            txtIncome.Text = $"+{income:0.##} ₽";
            txtExpense.Text = $"-{expense:0.##} ₽";
            decimal expensemounth = App.dB.Transaction
                .Where(t =>
                    t.Category.IDTypeCategory == 2 && t.DateTransaction >= startOfMonth && t.DateTransaction < endOfMonth)
                .Sum(t => (decimal?)t.AmountTransaction) ?? 0;
            decimal incomemounth = App.dB.Transaction
                .Where(t =>
                    t.Category.IDTypeCategory == 1 && t.DateTransaction >= startOfMonth && t.DateTransaction < endOfMonth)
                .Sum(t => (decimal?)t.AmountTransaction) ?? 0;
            txtMonthExpense.Text = $"-{expensemounth:0.##}";
            txtMonthIncome.Text = $"+{incomemounth:0.##}";
            decimal totalBalancemounth = incomemounth - expensemounth;
            if (totalBalancemounth != 0)
            {
                if (totalBalancemounth > 0)
                {
                    txtMonthResult.Text = "+" + $"{totalBalancemounth:0.##}";
                }
                else
                {
                    txtMonthResult.Text = "-" + $"{totalBalancemounth:0.##}";
                }
            }
            else
            {
                txtMonthResult.Text = $"{totalBalancemounth:0.##}";
            }

            var expenses = App.dB.Transaction
                .Where(t => t.IDUser == App.CurrentUser.IDUser
                        && t.Category.IDTypeCategory == 2)
                .ToList();
            var topCategory = expenses
                .GroupBy(t => t.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    Total = g.Sum(x => x.AmountTransaction)
                })
                .OrderByDescending(x => x.Total)
                .FirstOrDefault();
            decimal totalExpenses = expenses.Sum(x => x.AmountTransaction);

            if (topCategory != null)
            {
                txtMainCategory.Text = topCategory.Category.NameCategory;

                decimal percent = totalExpenses == 0
                    ? 0
                    : (topCategory.Total / totalExpenses) * 100;

                txtMainCategoryAmount.Text =
                    $"{topCategory.Total:0.##} ₽ • {percent:F0}%";
            }
            else
            {
                txtMainCategory.Text = "Нет данных";
                txtMainCategoryAmount.Text = "0 ₽ • 0%";
            }
        }

        private void LoadFinancialLevel()
        {
            var result = CalculateFinancialLevel();

            txtFinancialLevel.Text = result.Level;

        }
        private void LoadForecast()
        {
            if (App.CurrentUser == null)
                return;

            try
            {
                var now = DateTime.Now;

                var startMonth = new DateTime(now.Year, now.Month, 1);
                var endMonth = startMonth.AddMonths(1);

                var transactions = App.dB.Transaction
                    .Where(t => t.IDUser == App.CurrentUser.IDUser
                                && t.DateTransaction >= startMonth
                                && t.DateTransaction < endMonth)
                    .ToList();

                // доходы / расходы за текущий месяц
                decimal income = transactions
                    .Where(t => t.Category.IDTypeCategory == 1)
                    .Sum(t => (decimal?)t.AmountTransaction) ?? 0;

                decimal expense = transactions
                    .Where(t => t.Category.IDTypeCategory == 2)
                    .Sum(t => (decimal?)t.AmountTransaction) ?? 0;

                int daysPassed = (now - startMonth).Days + 1;
                int daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
                int daysLeft = daysInMonth - daysPassed;

                // защита от деления на 0
                if (daysPassed <= 0) daysPassed = 1;

                // средний расход в день
                decimal avgDailyExpense = expense / daysPassed;

                // прогноз расходов до конца месяца
                decimal forecastExpense = expense + (avgDailyExpense * daysLeft);

                // итог
                decimal result = income - forecastExpense;

                txtForecast.Text = $"{result:N0} ₽";

                // статус
                if (result >= 0)
                {
                    txtForecast.Foreground =
                        new System.Windows.Media.SolidColorBrush(
                            System.Windows.Media.Color.FromRgb(156, 255, 177));

                    txtForecastStatus.Text = "Финансы под контролем";
                }
                else
                {
                    txtForecast.Foreground =
                        new System.Windows.Media.SolidColorBrush(
                            System.Windows.Media.Color.FromRgb(255, 120, 120));

                    txtForecastStatus.Text = "Риск перерасхода";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка прогноза: " + ex.Message);
            }
        }
        private string GetFinancialLevel(double score)
        {
            if (score < 30)
                return "КРИТИЧЕСКИЙ";

            if (score < 50)
                return "НИЗКИЙ";

            if (score < 70)
                return "СТАБИЛЬНЫЙ";

            if (score < 85)
                return "ХОРОШИЙ";

            return "ОТЛИЧНЫЙ";
        }
        private void OpenFinanceInfo_Click(object sender, RoutedEventArgs e)
        {
            var result = CalculateFinancialLevel();

            // 1. Финальный score
            txtFinanceScore.Text = $"{result.Score:F0} / 100";

            // 2. Подписи в блоках процентов
            txtCashFlow.Text = $"💰 Прибыльность — ({result.CashFlowScore:F0}/100)";
            txtSavingsRate.Text = $"📈 Экономность — ({result.SavingsScore:F0}/100)";
            txtFinancialBuffer.Text = $"🛡 Подушка безопасности — ({result.BufferScore:F0}/100)";

            // 3. Можно дополнительно подсветить уровень (если хочешь)
            // например изменить цвет финального score
            txtFinanceScore.Foreground = GetLevelBrush(result.Score);
            FinanceInfoOverlay.Visibility = Visibility.Visible;
            FinanceInfoOverlayScroll.Visibility = Visibility.Visible;
        }
        private Brush GetLevelBrush(double score)
        {
            if (score < 30)
                return new SolidColorBrush(Color.FromRgb(255, 179, 179)); // red
            if (score < 50)
                return new SolidColorBrush(Color.FromRgb(255, 213, 158)); // orange
            if (score < 70)
                return new SolidColorBrush(Color.FromRgb(255, 241, 168)); // yellow
            if (score < 85)
                return new SolidColorBrush(Color.FromRgb(183, 255, 183)); // green
            return new SolidColorBrush(Color.FromRgb(156, 255, 177));     // bright green
        }
        private void OpenForecastInfo_Click(object sender, RoutedEventArgs e)
        {
            ForecastInfoOverlay.Visibility = Visibility.Visible;
        }

        private void CloseForecastInfo_Click(object sender, RoutedEventArgs e)
        {
            ForecastInfoOverlay.Visibility = Visibility.Collapsed;
        }
        private void CloseFinanceInfo_Click(object sender, RoutedEventArgs e)
        {
            FinanceInfoOverlay.Visibility = Visibility.Collapsed;
            FinanceInfoOverlayScroll.Visibility= Visibility.Collapsed;
        }
        private void OpenGoalPopup_Click(object sender, RoutedEventArgs e)
        {
            CreateGoalOverlay.Visibility = Visibility.Visible;
        }

        private void CloseGoalPopup_Click(object sender, RoutedEventArgs e)
        {
            CreateGoalOverlay.Visibility = Visibility.Collapsed;
        }
        private void OpenGoalEditPopup_Click(object sender, RoutedEventArgs e)
        {
            GoalOverlay.Visibility = Visibility.Visible;
        }

        private void CloseGoalEditPopup_Click(object sender, RoutedEventArgs e)
        {
            GoalOverlay.Visibility = Visibility.Collapsed;
        }

        private void LoadGoal()
        {
            currentGoal = App.dB.Goal.Where(g => g.IDUser == App.CurrentUser.IDUser).FirstOrDefault();

            if (currentGoal == null)
            {
                GoalEmptyState.Visibility = Visibility.Visible;
                GoalContent.Visibility = Visibility.Collapsed;

                return;
            }

            GoalEmptyState.Visibility = Visibility.Collapsed;
            GoalContent.Visibility = Visibility.Visible;

            GoalTitleText.Text = currentGoal.NameGoal;
            GoalTitleBox.Text = currentGoal.NameGoal;
            GoalPaidText.Text = $"{currentGoal.PaidAmountGoal:N0} ₽";
            GoalAmountBox.Text = currentGoal.AmountGoal.ToString("N0");
            GoalAmountText.Text = $"{currentGoal.PaidAmountGoal:N0}" + " / " + currentGoal.AmountGoal.ToString("N0") + " ₽";

            double percent =
                (double)(currentGoal.PaidAmountGoal / currentGoal.AmountGoal* 100);

            GoalOverlayProgress.Value = percent;

            GoalOverlayProgressPopup.Value = percent;
        }

        private bool isEditing = false;

        private void ToggleEditMode_Click(object sender, RoutedEventArgs e)
        {
            if (currentGoal == null) return;

            isEditing = !isEditing;

            if (isEditing)
            {
                EnableEditing(GoalTitleBox);
                EnableEditing(GoalAmountBox);
            }
            else
            {
                DisableEditing(GoalTitleBox);
                DisableEditing(GoalAmountBox);

                SaveGoalInline();
            }
        }
        private void EnableEditing(TextBox box)
        {
            btnEditGoal.Content = "✏ Сорхранить цель";
            GoalTitleBox.IsReadOnly = false;
            GoalTitleBox.BorderThickness = new Thickness(0, 0, 0, 2);
            GoalTitleBox.BorderBrush = Brushes.White;
            GoalAmountBox.IsReadOnly = false;
            GoalAmountBox.BorderThickness = new Thickness(0, 0, 0, 2);
            GoalAmountBox.BorderBrush = Brushes.White;
        }
        private void DisableEditing(TextBox box)
        {
            btnEditGoal.Content = "✏ Изменить цель";
            box.IsReadOnly = true;
            box.BorderThickness = new Thickness(0);
            box.Background = Brushes.Transparent;
        }
        private void SaveGoalInline()
        {
            if (!decimal.TryParse(
    GoalAmountBox.Text.Replace(" ", ""),
    out decimal amount))
            {
                MessageBox.Show("Введите сумму");
                return;
            }

            currentGoal.NameGoal = GoalTitleBox.Text;
            currentGoal.AmountGoal = amount;

            App.dB.SaveChanges(); ;
            LoadGoal();
        }

        private void CreateGoalConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtGoalName.Text))
            {
                MessageBox.Show("Введите название цели");

                return;
            }

            if (!decimal.TryParse(txtGoalAmount.Text, out decimal amount))
            {
                MessageBox.Show("Введите сумму");

                return;
            }

            Goal goal = new Goal()
            {
                NameGoal = txtGoalName.Text,
                AmountGoal = amount,
                PaidAmountGoal = 0,
                IDUser = App.CurrentUser.IDUser
            };

            App.dB.Goal.Add(goal);

            App.dB.SaveChanges();

            txtGoalName.Clear();

            txtGoalAmount.Clear();

            CreateGoalOverlay.Visibility = Visibility.Collapsed;

            LoadGoal();
        }
        private void AddMoneyGoal_Click(object sender, RoutedEventArgs e)
        {
            if (currentGoal == null)
            {
                MessageBox.Show("Цель не найдена");
                return;
            }

            // проверка ввода
            if (!decimal.TryParse(
                txtGoalAddAmount.Text,
                out decimal add))
            {
                MessageBox.Show("Введите корректную сумму");
                return;
            }

            // защита от уже закрытой цели
            if (currentGoal.PaidAmountGoal >= currentGoal.AmountGoal)
            {
                MessageBox.Show("Цель достигнута", "Цель достигнута", MessageBoxButton.OK, MessageBoxImage.Information); ;
                return;
            }

            // защита от некорректных значений
            if (add <= 0)
            {
                MessageBox.Show("Сумма пополнения должна быть больше 0");
                return;
            }

            // проверка баланса
            if (App.CurrentUser.Balance < add)
            {
                MessageBox.Show("Недостаточно средств на балансе");
                return;
            }

            // защита от переполнения цели
            decimal remaining = currentGoal.AmountGoal - currentGoal.PaidAmountGoal;

            if (add > remaining)
            {
                var result = MessageBox.Show(
                    $"Вы добавляете больше чем осталось ({remaining:0.##} ₽). Добавить только оставшуюся сумму?",
                    "Подтверждение",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    MessageBox.Show("Отлично цель достигнута, отличная работа!", "Поздравление", MessageBoxButton.OK, MessageBoxImage.Information);
                    currentGoal.PaidAmountGoal = currentGoal.AmountGoal;
                    // 1. списываем с баланса
                    App.CurrentUser.Balance -= currentGoal.AmountGoal;
                    var transaction = new Model.Transaction
                    {
                        AmountTransaction = currentGoal.AmountGoal,
                        NoteTransaction = "В копилку на цель",
                        Category = App.dB.Category.FirstOrDefault(c => c.NameCategory == "Цель"),
                        IDUser = App.CurrentUser.IDUser,
                        DateTransaction = DateTime.Now
                    }; App.dB.Transaction.Add(transaction);
                }
                else
                {
                    return;
                }
            }
            else
            {
                // 1. списываем с баланса
                App.CurrentUser.Balance -= add;

                // 2. добавляем в цель
                currentGoal.PaidAmountGoal += add;

                var transaction = new Model.Transaction
                {
                    AmountTransaction = add,
                    NoteTransaction = "В копилку на цель",
                    Category = App.dB.Category.FirstOrDefault(c => c.NameCategory == "Цель"),
                    IDUser = App.CurrentUser.IDUser,
                    DateTransaction = DateTime.Now
                }; App.dB.Transaction.Add(transaction);

                remaining = currentGoal.AmountGoal - currentGoal.PaidAmountGoal;

                if (remaining == 0)
                {
                    MessageBox.Show("Отлично цель достигнута, отличная работа!", "Поздравление", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }

            try
            {
                App.dB.SaveChanges();
                UpdateWindow();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения: " + ex.Message);
            }
        }
        private void DeleteGoal_Click(object sender, RoutedEventArgs e)
        {
            if (currentGoal == null)
                return;

            App.dB.Goal.Remove(currentGoal);
            App.dB.SaveChanges();

            currentGoal = null;
            GoalOverlay.Visibility = Visibility.Collapsed;
            UpdateWindow();
        }

        private void CategoriesPage_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new CategoryPage());
        private void Debt_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new DebtPage());
        private void Transaction_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new Transactions());
        private void Stats_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new Stats());
        private void Home_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new Home());
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
                MessageBox.Show("Ошибка выхода: " + ex.Message);
            }
        }
    }
    public class TransactionVM
    {
        public string Title { get; set; }
        public string AmountText { get; set; }
        public Brush AmountColor { get; set; }
        public decimal Amount { get; set; }
        public bool IsIncome { get; set; }
    }
    public class FinancialLevelResult
    {
        public double Score { get; set; }

        public string Level { get; set; }

        public double CashFlowScore { get; set; }

        public double SavingsScore { get; set; }

        public double BufferScore { get; set; }

        public decimal Income { get; set; }

        public decimal Expense { get; set; }

        public decimal Balance { get; set; }

        public decimal BufferMonths { get; set; }
    }
}

