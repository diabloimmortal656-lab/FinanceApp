using System;
using System.Collections.Generic;
using System.Data.Common;
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
using static System.Collections.Specialized.BitVector32;

namespace FinanceApp.Views
{
    /// <summary>
    /// Логика взаимодействия для Page1.xaml
    /// </summary>
    public partial class Stats : Page
    {
        public Stats()
        {
            InitializeComponent();
            LoadAverageCheck();
            LoadSavedMoney();
            LoadImpulse();
            LoadStability();
            LoadRisk();
            LoadAIInsights();
            LoadMainInsight();
            LoadCategoryStats();
            LoadTrend();
            LoadDangerDay();
            LoadMicroPurchases();
            LoadNightSpending();
            LoadFinanceProfile();
            LoadRecurringExpenses();
            LoadBudgetControl();
            LoadActivity();
            LoadHeatmap();
        }
        private List<string> GenerateAdvice(FinancialState state)
        {
            var rnd = new Random();

            var pool = new List<(string key, string[] texts, int priority)>
    {
        // IMPULSE
        ("IMPULSE_HIGH", new[]
        {
            "Ты слишком часто совершаешь импульсивные покупки — введи правило паузы 24 часа.",
            "Импульсивные траты высоки — ограничь спонтанные расходы.",
            "Финансовые решения часто эмоциональны — стоит замедлить покупки."
        }, 3),

        ("IMPULSE_MEDIUM", new[]
        {
            "Иногда ты тратишь импульсивно — стоит отслеживать триггеры.",
            "Есть потенциал улучшить контроль спонтанных покупок.",
            "Импульсивность умеренная — можно оптимизировать."
        }, 2),

        ("IMPULSE_LOW", new[]
        {
            "Ты хорошо контролируешь импульсивные траты.",
            "Финансовая дисциплина в покупках высокая.",
            "Спонтанные покупки минимальны."
        }, 1),

        // EVENING
        ("EVENING_HIGH", new[]
        {
            "Большая часть расходов приходится на вечер — риск перерасхода.",
            "Вечерние траты доминируют — контроль снижается в это время.",
            "Стоит ограничить покупки вечером."
        }, 3),

        ("EVENING_MEDIUM", new[]
        {
            "Часть расходов смещена на вечер.",
            "Вечерние траты умеренные.",
            "Есть небольшая концентрация расходов вечером."
        }, 2),

        ("EVENING_LOW", new[]
        {
            "Вечерние расходы под контролем.",
            "Ты стабилен в распределении трат по времени.",
            "Нет перегрузки вечерних покупок."
        }, 1),

        // STABILITY
        ("STABILITY_LOW", new[]
        {
            "Финансовые траты нестабильны — есть сильные колебания.",
            "Бюджет сложно прогнозировать.",
            "Стоит выровнять расходы по дням."
        }, 3),

        ("STABILITY_MEDIUM", new[]
        {
            "Стабильность средняя — есть небольшие колебания.",
            "Финансовая структура частично ровная.",
            "Можно улучшить регулярность расходов."
        }, 2),

        ("STABILITY_HIGH", new[]
        {
            "Отличная финансовая стабильность.",
            "Расходы распределены равномерно.",
            "Ты контролируешь финансовый поток."
        }, 1),

        // ACTIVITY
        ("ACTIVITY_HIGH", new[]
        {
            "Слишком высокая частота операций усложняет контроль.",
            "Много транзакций — сложно отслеживать бюджет.",
            "Стоит уменьшить количество мелких операций."
        }, 3),

        ("ACTIVITY_MEDIUM", new[]
        {
            "Умеренная финансовая активность.",
            "Можно немного упростить структуру расходов.",
            "Активность нормальная."
        }, 2),

        ("ACTIVITY_LOW", new[]
        {
            "Низкая активность — легко контролировать финансы.",
            "Мало операций, высокая читаемость бюджета.",
            "Финансовые действия структурированы."
        }, 1),

        // КОМБИНАЦИИ
        ("IMPULSE_HIGH+EVENING_HIGH", new[]
        {
            "Опасное сочетание: вечер + импульсивность усиливают перерасход.",
            "Ты склонен к спонтанным вечерним тратам.",
            "Контроль вечером критически важен."
        }, 4),

        ("ALL_GOOD", new[]
        {
            "Финансовое поведение стабильно и сбалансировано.",
            "Ты демонстрируешь высокий уровень контроля.",
            "Отличная финансовая дисциплина."
        }, 5)
    };

            var result = new List<(string text, int priority)>();

            void Add(string key)
            {
                var items = pool.Where(p => p.key == key).ToList();
                if (!items.Any()) return;

                var item = items.First();
                var text = item.texts[rnd.Next(item.texts.Length)];

                result.Add((text, item.priority));
            }

            // =========================
            // ЛОГИКА ВЫБОРА
            // =========================

            Add("IMPULSE_" + state.Impulse);
            Add("EVENING_" + state.Evening);
            Add("STABILITY_" + state.Stability);
            Add("ACTIVITY_" + state.Activity);

            if (state.Impulse == "HIGH" && state.Evening == "HIGH")
                Add("IMPULSE_HIGH+EVENING_HIGH");

            if (state.Impulse == "LOW" && state.Stability == "HIGH")
                Add("ALL_GOOD");

            return result
                .OrderByDescending(x => x.priority)
                .Take(4)
                .Select(x => x.text)
                .ToList();
        }
        private void LoadAverageCheck()
        {
                var avg = App.dB.Transaction
                    .Where(t => t.IDUser == App.CurrentUser.IDUser
                             && t.Category.IDTypeCategory == 2)
                    .Select(t => (decimal?)t.AmountTransaction)
                    .DefaultIfEmpty(0)
                    .Average();

                txtAverageCheck.Text = $"{Math.Round(avg ?? 0, 0)} ₽";
        }
        private void LoadSavedMoney()
        {
            int userId = App.CurrentUser.IDUser;

            var now = DateTime.Now;

            DateTime startThisMonth = new DateTime(now.Year, now.Month, 1);
            DateTime startLastMonth = startThisMonth.AddMonths(-1);
            DateTime endLastMonth = startThisMonth.AddDays(-1);

            decimal lastMonthExpenses = App.dB.Transaction
                .Where(t =>
                    t.IDUser == userId &&
                    t.Category.IDTypeCategory == 2 &&
                    t.DateTransaction >= startLastMonth &&
                    t.DateTransaction <= endLastMonth)
                .Sum(t => (decimal?)t.AmountTransaction) ?? 0;

            decimal thisMonthExpenses = App.dB.Transaction
                .Where(t =>
                    t.IDUser == userId &&
                    t.Category.IDTypeCategory == 2 &&
                    t.DateTransaction >= startThisMonth)
                .Sum(t => (decimal?)t.AmountTransaction) ?? 0;

            lastMonthExpenses = Math.Abs(lastMonthExpenses);
            thisMonthExpenses = Math.Abs(thisMonthExpenses);

            decimal savedMoney = lastMonthExpenses - thisMonthExpenses;

            if (savedMoney < 0)
                savedMoney = 0;

            txtSavedMoney.Text = $"{savedMoney:N0} ₽";
        }
        private void LoadImpulse()
        {
            int userId = App.CurrentUser.IDUser;

            var now = DateTime.Now;

            DateTime startMonth = new DateTime(now.Year, now.Month, 1);

            // ВСЕ расходы за месяц
            var expenses = App.dB.Transaction
                .Where(t =>
                    t.IDUser == userId &&
                    t.Category.IDTypeCategory == 2 &&
                    t.DateTransaction >= startMonth)
                .ToList();

            // если расходов нет
            if (!expenses.Any())
            {
                txtImpulse.Text = "НЕТ ДАННЫХ";
                return;
            }

            // мелкие покупки до 1000 ₽
            var microExpenses = expenses
                .Where(t => t.AmountTransaction < 1000).Count();

            int totalExpenses = expenses.Count;

            decimal percent = (decimal)microExpenses / totalExpenses * 100;

            // определяем уровень
            if (percent < 25)
            {
                txtImpulse.Text = "НИЗКАЯ";
            }
            else if (percent < 50)
            {
                txtImpulse.Text = "СРЕДНЯЯ";
            }
            else
            {
                txtImpulse.Text = "ВЫСОКАЯ";
            }
        }
        private void LoadStability()
        {
            int userId = App.CurrentUser.IDUser;

            var now = DateTime.Now;

            DateTime startMonth = new DateTime(now.Year, now.Month, 1);

            // расходы за месяц
            var expenses = App.dB.Transaction
                .Where(t =>
                    t.IDUser == userId &&
                    t.Category.IDTypeCategory == 2 &&
                    t.DateTransaction >= startMonth)
                .ToList();

            // если нет данных
            if (!expenses.Any())
            {
                txtStability.Text = "НЕТ ДАННЫХ";
                return;
            }

            // группируем расходы по дням
            var dailyExpenses = expenses
                .GroupBy(t => t.DateTransaction.Date)
                .Select(g => g.Sum(x => x.AmountTransaction))
                .ToList();

            decimal average = dailyExpenses.Average();

            // считаем отклонение
            decimal deviation = dailyExpenses
                .Average(x => Math.Abs(x - average));

            // коэффициент стабильности
            decimal stability = 100;

            if (average > 0)
            {
                stability = 100 - (deviation / average * 100);
            }

            // ограничения
            if (stability < 0)
                stability = 0;

            if (stability > 100)
                stability = 100;

            // определяем уровень
            if (stability < 25)
            {
                txtStability.Text = "НИЗКАЯ";
            }
            else if (stability < 50)
            {
                txtStability.Text = "СРЕДНЯЯ";
            }
            else
            {
                txtStability.Text = "ВЫСОКАЯ";
            }
        }
        private void LoadRisk()
        {
            int userId = App.CurrentUser.IDUser;

            var now = DateTime.Now;

            DateTime startMonth = new DateTime(now.Year, now.Month, 1);

            // доходы
            decimal income = App.dB.Transaction
                .Where(t =>
                    t.IDUser == userId &&
                    t.Category.IDTypeCategory == 1 &&
                    t.DateTransaction >= startMonth)
                .Sum(t => (decimal?)t.AmountTransaction) ?? 0;

            // расходы
            var expenses = App.dB.Transaction
                .Where(t =>
                    t.IDUser == userId &&
                    t.Category.IDTypeCategory == 2 &&
                    t.DateTransaction >= startMonth)
                .ToList();

            decimal totalExpenses = expenses.Sum(t => t.AmountTransaction);

            // если нет данных
            if (income <= 0 || !expenses.Any())
            {
                txtRisk.Text = "НИЗКИЙ";
                return;
            }

            int riskPoints = 0;

            // 1. расходы слишком близки к доходам
            decimal expensePercent = totalExpenses / income * 100;

            if (expensePercent >= 90)
                riskPoints += 3;
            else if (expensePercent >= 70)
                riskPoints += 2;
            else if (expensePercent >= 50)
                riskPoints += 1;

            // 2. много мелких покупок
            int microCount = expenses
                .Count(t => t.AmountTransaction <= 1000);

            decimal microPercent =
                (decimal)microCount / expenses.Count * 100;

            if (microPercent >= 60)
                riskPoints += 2;
            else if (microPercent >= 40)
                riskPoints += 1;

            // 3. нестабильные траты
            var dailyExpenses = expenses
                .GroupBy(t => t.DateTransaction.Date)
                .Select(g => g.Sum(x => x.AmountTransaction))
                .ToList();

            decimal average = dailyExpenses.Average();

            decimal deviation = dailyExpenses
                .Average(x => Math.Abs(x - average));

            decimal stability = 100;

            if (average > 0)
            {
                stability = 100 - (deviation / average * 100);
            }

            if (stability < 40)
                riskPoints += 2;
            else if (stability < 70)
                riskPoints += 1;

            // итог
            if (riskPoints <= 2)
            {
                txtRisk.Text = "НИЗКИЙ";
            }
            else if (riskPoints <= 5)
            {
                txtRisk.Text = "СРЕДНИЙ";
            }
            else
            {
                txtRisk.Text = "ВЫСОКИЙ";
            }
        }
        private void LoadAIInsights()
        {
            int userId = App.CurrentUser.IDUser;

            var now = DateTime.Now;
            var startMonth = new DateTime(now.Year, now.Month, 1);

            var transactions = App.dB.Transaction
                .Where(t => t.IDUser == userId && t.DateTransaction >= startMonth)
                .ToList();

            var expenses = transactions
                .Where(t => t.Category.IDTypeCategory == 2)
                .Select(t => new
                {
                    Amount = Math.Abs(t.AmountTransaction),
                    Hour = t.DateTransaction.Hour,
                    Date = t.DateTransaction.Date
                })
                .ToList();

            if (!expenses.Any())
            {
                SetAISuggestions(new List<string>
        {
            "Недостаточно данных для анализа",
            "Начни совершать операции, чтобы получить рекомендации",
            "Система адаптируется под твоё поведение",
            "Добавь транзакции для персонального анализа"
        });
                return;
            }

            decimal total = expenses.Sum(x => x.Amount);

            decimal evening = expenses
                .Where(x => x.Hour >= 18)
                .Sum(x => x.Amount);

            decimal micro = expenses
                .Where(x => x.Amount < 1000)
                .Sum(x => x.Amount);

            var daily = expenses
                .GroupBy(x => x.Date)
                .Select(g => g.Sum(x => x.Amount))
                .ToList();

            decimal avg = daily.Any() ? daily.Average() : 0;

            decimal deviation = daily.Any()
                ? daily.Average(x => Math.Abs(x - avg))
                : 0;

            // =========================
            // СОСТОЯНИЯ
            // =========================

            var state = new FinancialState
            {
                Impulse = GetLevel(micro / total),
                Evening = GetLevel(evening / total),
                Stability = GetStability(avg, deviation),
                Activity = GetLevel((decimal)expenses.Count / 50)
            };

            var advice = GenerateAdvice(state);

            SetAISuggestions(advice);
        }
        private string GetLevel(decimal ratio)
        {
            if (ratio < 0.2m) return "LOW";
            if (ratio < 0.5m) return "MEDIUM";
            return "HIGH";
        }

        private string GetStability(decimal avg, decimal deviation)
        {
            if (avg == 0) return "LOW";

            decimal stability = 100 - (deviation / avg * 100);

            if (stability < 40) return "LOW";
            if (stability < 70) return "MEDIUM";
            return "HIGH";
        }
        private void SetAISuggestions(List<string> list)
        {
            AIText1.Text = list.ElementAtOrDefault(0) ?? "";
            AIText2.Text = list.ElementAtOrDefault(1) ?? "";
            AIText3.Text = list.ElementAtOrDefault(2) ?? "";
            AIText4.Text = list.ElementAtOrDefault(3) ?? "";
        }
        private void LoadMainInsight()
        {
            int userId = App.CurrentUser.IDUser;

            var now = DateTime.Now;
            var startMonth = new DateTime(now.Year, now.Month, 1);

            var transactions = App.dB.Transaction
                .Where(t => t.IDUser == userId && t.DateTransaction >= startMonth)
                .ToList();

            var expenses = transactions
                .Where(t => t.Category.IDTypeCategory == 2)
                .Select(t => new
                {
                    Amount = Math.Abs(t.AmountTransaction),
                    Hour = t.DateTransaction.Hour
                })
                .ToList();

            if (!expenses.Any())
            {
                txtMainInsight.Text = "Недостаточно данных для анализа поведения";
                return;
            }

            decimal total = expenses.Sum(x => x.Amount);

            decimal evening = expenses
                .Where(x => x.Hour >= 18)
                .Sum(x => x.Amount);

            decimal micro = expenses
                .Where(x => x.Amount < 1000)
                .Sum(x => x.Amount);

            int transactionsCount = expenses.Count;

            var signals = new List<(string text, decimal score)>();

            // =========================
            // 1. импульсивность
            // =========================
            decimal microRatio = micro / total;
            signals.Add((
                "Слишком много мелких и импульсивных покупок",
                microRatio
            ));

            // =========================
            // 2. вечерние траты
            // =========================
            decimal eveningRatio = evening / total;
            signals.Add((
                "Основной перерасход происходит в вечернее время",
                eveningRatio
            ));

            // =========================
            // 3. высокая активность
            // =========================
            decimal activityScore = transactionsCount / 60m;
            signals.Add((
                "У тебя высокая частота финансовых операций",
                activityScore
            ));

            // =========================
            // 4. стабильность (инверсия)
            // =========================
            var daily = expenses
                .GroupBy(x => x.Hour) // грубая модель поведения
                .Select(g => g.Sum(x => x.Amount))
                .ToList();

            decimal avg = daily.Any() ? daily.Average() : 0;
            decimal deviation = daily.Any()
                ? daily.Average(x => Math.Abs(x - avg))
                : 0;

            decimal stability = avg > 0
                ? 1 - (deviation / avg)
                : 0;

            signals.Add((
                "Твое финансовое поведение нестабильно и хаотично",
                1 - stability
            ));

            // =========================
            // ВЫБОР ГЛАВНОГО ИНСАЙТА
            // =========================
            var main = signals
                .OrderByDescending(x => x.score)
                .First();

            txtMainInsight.Text = main.text;
        }

        private void LoadCategoryStats()
        {
            int userId = App.CurrentUser.IDUser;

            var now = DateTime.Now;

            DateTime startMonth =
                new DateTime(now.Year, now.Month, 1);

            var expenses = App.dB.Transaction
                .Where(t =>
                    t.IDUser == userId &&
                    t.Category.IDTypeCategory == 2 &&
                    t.DateTransaction >= startMonth)
                .ToList();

            if (!expenses.Any())
            {
                TopCategories.ItemsSource = null;
                return;
            }

            decimal total = expenses.Sum(t =>
                Math.Abs(t.AmountTransaction));

            var categories = expenses
                .GroupBy(t => t.Category.NameCategory)
                .Select(g => new
                {
                    Name = g.Key,

                    Amount = string.Format(
                        "{0:N0} ₽",
                        g.Sum(x => Math.Abs(x.AmountTransaction))
                    ),

                    Percent = string.Format(
                        "{0}%",
                        Math.Round(
                            g.Sum(x => Math.Abs(x.AmountTransaction))
                            / total * 100
                        )
                    )
                })
                .OrderByDescending(x =>
                    Convert.ToDecimal(
                        x.Amount
                         .Replace(" ₽", "")
                         .Replace(" ", "")
                    ))
                .Take(4)
                .ToList();

            TopCategories.ItemsSource = categories;
        }
        private void LoadTrend()
        {
            int userId = App.CurrentUser.IDUser;

            var now = DateTime.Now;

            // текущий месяц
            DateTime startThisMonth =
                new DateTime(now.Year, now.Month, 1);

            // прошлый месяц
            DateTime startLastMonth =
                startThisMonth.AddMonths(-1);

            DateTime endLastMonth =
                startThisMonth.AddDays(-1);

            // расходы текущего месяца
            decimal thisMonth = App.dB.Transaction
                .Where(t =>
                    t.IDUser == userId &&
                    t.Category.IDTypeCategory == 2 &&
                    t.DateTransaction >= startThisMonth)
                .Sum(t => (decimal?)t.AmountTransaction) ?? 0;

            // расходы прошлого месяца
            decimal lastMonth = App.dB.Transaction
                .Where(t =>
                    t.IDUser == userId &&
                    t.Category.IDTypeCategory == 2 &&
                    t.DateTransaction >= startLastMonth &&
                    t.DateTransaction <= endLastMonth)
                .Sum(t => (decimal?)t.AmountTransaction) ?? 0;

            // делаем положительными
            thisMonth = Math.Abs(thisMonth);
            lastMonth = Math.Abs(lastMonth);

            // если прошлый месяц пустой
            if (lastMonth == 0)
            {
                txtTrend.Text = "0%";
                return;
            }

            // изменение в процентах
            decimal trend =
                ((thisMonth - lastMonth) / lastMonth) * 100;

            // округление
            trend = Math.Round(trend);

            // текст
            if (trend > 0)
            {
                txtTrend.Text = "+" + trend + "%";

                txtTrend.Foreground =
                    new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#FFB3B3"));
            }
            else if (trend < 0)
            {
                txtTrend.Text = trend + "%";

                txtTrend.Foreground =
                    new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#B7FFB7"));
            }
            else
            {
                txtTrend.Text = "0%";

                txtTrend.Foreground =
                    Brushes.White;
            }
        }
        private void LoadDangerDay()
        {
            int userId = App.CurrentUser.IDUser;

            var now = DateTime.Now;

            DateTime startMonth =
                new DateTime(now.Year, now.Month, 1);

            // все расходы за месяц
            var expenses = App.dB.Transaction
                .Where(t =>
                    t.IDUser == userId &&
                    t.Category.IDTypeCategory == 2 &&
                    t.DateTransaction >= startMonth)
                .ToList();

            // если данных нет
            if (!expenses.Any())
            {
                txtDangerDay.Text = "НЕТ ДАННЫХ";
                return;
            }

            // группировка по дням недели
            var dangerDay = expenses
                .GroupBy(t => t.DateTransaction.DayOfWeek)
                .Select(g => new
                {
                    Day = g.Key,
                    Total = g.Sum(x =>
                        Math.Abs(x.AmountTransaction))
                })
                .OrderByDescending(x => x.Total)
                .First();

            string dayName = "";

            switch (dangerDay.Day)
            {
                case DayOfWeek.Monday:
                    dayName = "Понедельник";
                    break;

                case DayOfWeek.Tuesday:
                    dayName = "Вторник";
                    break;

                case DayOfWeek.Wednesday:
                    dayName = "Среда";
                    break;

                case DayOfWeek.Thursday:
                    dayName = "Четверг";
                    break;

                case DayOfWeek.Friday:
                    dayName = "Пятница";
                    break;

                case DayOfWeek.Saturday:
                    dayName = "Суббота";
                    break;

                case DayOfWeek.Sunday:
                    dayName = "Воскресенье";
                    break;
            }

            txtDangerDay.Text = dayName;
        }
        private void LoadMicroPurchases()
        {
            int userId = App.CurrentUser.IDUser;

            var now = DateTime.Now;

            DateTime startMonth =
                new DateTime(now.Year, now.Month, 1);

            // все расходы за месяц
            var expenses = App.dB.Transaction
                .Where(t =>
                    t.IDUser == userId &&
                    t.Category.IDTypeCategory == 2 &&
                    t.DateTransaction >= startMonth)
                .ToList();

            // если нет расходов
            if (!expenses.Any())
            {
                txtMicroPercent.Text = "0%";
                return;
            }

            // мелкие покупки (< 1000)
            var microExpenses = expenses
                .Where(t =>
                    Math.Abs(t.AmountTransaction) < 1000)
                .ToList();

            decimal totalExpenses =
                expenses.Sum(t =>
                    Math.Abs(t.AmountTransaction));

            decimal microTotal =
                microExpenses.Sum(t =>
                    Math.Abs(t.AmountTransaction));

            // защита от деления на 0
            if (totalExpenses <= 0)
            {
                txtMicroPercent.Text = "0%";
                return;
            }

            decimal percent =
                (microTotal / totalExpenses) * 100;

            percent = Math.Round(percent);

            txtMicroPercent.Text = percent + "%";
        }
        private void LoadNightSpending()
        {
            int userId = App.CurrentUser.IDUser;

            var now = DateTime.Now;

            DateTime startMonth =
                new DateTime(now.Year, now.Month, 1);

            // все расходы за месяц
            var expenses = App.dB.Transaction
                .Where(t =>
                    t.IDUser == userId &&
                    t.Category.IDTypeCategory == 2 &&
                    t.DateTransaction >= startMonth)
                .ToList();

            // если данных нет
            if (!expenses.Any())
            {
                txtNightSpend.Text = "0%";
                return;
            }

            // ночные траты после 22:00
            var nightExpenses = expenses
                .Where(t =>
                    t.DateTransaction.Hour >= 22 ||
                    t.DateTransaction.Hour <= 5)
                .ToList();

            decimal totalExpenses =
                expenses.Sum(t =>
                    Math.Abs(t.AmountTransaction));

            decimal nightTotal =
                nightExpenses.Sum(t =>
                    Math.Abs(t.AmountTransaction));

            // защита
            if (totalExpenses <= 0)
            {
                txtNightSpend.Text = "0%";
                return;
            }

            // процент
            decimal percent =
                (nightTotal / totalExpenses) * 100;

            percent = Math.Round(percent);

            txtNightSpend.Text = percent + "%";
        }
        private void LoadFinanceProfile()
        {
            int userId = App.CurrentUser.IDUser;

            var now = DateTime.Now;

            DateTime startMonth =
                new DateTime(now.Year, now.Month, 1);

            var transactions = App.dB.Transaction
                .Where(t =>
                    t.IDUser == userId &&
                    t.DateTransaction >= startMonth)
                .ToList();

            var expenses = transactions
                .Where(t => t.Category.IDTypeCategory == 2)
                .ToList();

            var incomes = transactions
                .Where(t => t.Category.IDTypeCategory == 1)
                .ToList();

            // если данных нет
            if (!transactions.Any())
            {
                txtFinanceProfile.Text = "НЕТ ДАННЫХ";
                return;
            }

            decimal totalExpenses =
                expenses.Sum(t =>
                    Math.Abs(t.AmountTransaction));

            decimal totalIncome =
                incomes.Sum(t =>
                    Math.Abs(t.AmountTransaction));

            // мелкие покупки
            int microCount = expenses
                .Count(t =>
                    Math.Abs(t.AmountTransaction) < 1000);

            decimal microPercent = 0;

            if (expenses.Count > 0)
            {
                microPercent =
                    (decimal)microCount / expenses.Count * 100;
            }

            // ночные траты
            decimal nightTotal = expenses
                .Where(t =>
                    t.DateTransaction.Hour >= 22 ||
                    t.DateTransaction.Hour <= 5)
                .Sum(t =>
                    Math.Abs(t.AmountTransaction));

            decimal nightPercent = 0;

            if (totalExpenses > 0)
            {
                nightPercent =
                    nightTotal / totalExpenses * 100;
            }

            // стабильность
            decimal stability = 100;

            if (expenses.Any())
            {
                var dailyExpenses = expenses
                    .GroupBy(t => t.DateTransaction.Date)
                    .Select(g =>
                        g.Sum(x =>
                            Math.Abs(x.AmountTransaction)))
                    .ToList();

                decimal average =
                    dailyExpenses.Average();

                decimal deviation =
                    dailyExpenses.Average(x =>
                        Math.Abs(x - average));

                if (average > 0)
                {
                    stability =
                        100 - (deviation / average * 100);
                }
            }

            // профиль
            string profile;

            if (microPercent > 60 && nightPercent > 30)
            {
                profile = "Импульсивный";
            }
            else if (stability > 70 && totalIncome > totalExpenses)
            {
                profile = "Рациональный";
            }
            else if (nightPercent > 40)
            {
                profile = "Эмоциональный";
            }
            else if (totalExpenses > totalIncome)
            {
                profile = "Рискованный";
            }
            else if (stability > 60)
            {
                profile = "Стабильный";
            }
            else
            {
                profile = "Смешанный";
            }

            txtFinanceProfile.Text = profile;
            string description = "";

            switch (profile)
            {
                case "Импульсивный":
                    description =
                        "Ты часто совершаешь спонтанные покупки и тратишь деньги под влиянием момента. Основная зона роста — контроль мелких расходов.";
                    break;

                case "Рациональный":
                    description =
                        "Ты грамотно распределяешь деньги, сохраняешь баланс между доходами и расходами и избегаешь лишних трат.";
                    break;

                case "Эмоциональный":
                    description =
                        "Твои расходы сильно зависят от настроения и времени суток. Часть покупок совершается импульсивно, особенно вечером.";
                    break;

                case "Рискованный":
                    description =
                        "Расходы приближаются к доходам или превышают их. Финансовая нагрузка повышена и требует большего контроля.";
                    break;

                case "Стабильный":
                    description =
                        "Ты придерживаешься стабильного финансового поведения и редко совершаешь резкие или хаотичные траты.";
                    break;

                default:
                    description =
                        "Финансовое поведение сочетает несколько разных моделей. Пока сложно выделить один выраженный профиль.";
                    break;
            }

            txtFinanceProfileDescription.Text = description;
        }
        private void LoadRecurringExpenses()
        {
            int userId = App.CurrentUser.IDUser;

            var expenses = App.dB.Transaction
                .Where(t =>
                    t.IDUser == userId &&
                    t.Category.IDTypeCategory == 2)
                .ToList();

            if (!expenses.Any())
            {
                RecurringItems.ItemsSource = null;
                return;
            }

            var recurring = expenses
                .GroupBy(t => new
                {
                    CategoryId = t.IDCategory,
                    Amount = Math.Round(Math.Abs(t.AmountTransaction), 0)
                })
                .Select(g => new
                {
                    Name = g.First().Category.NameCategory,
                    Amount = $"{g.Key.Amount:N0} ₽",
                    Count = g.Count(),
                    Total = g.Sum(x => Math.Abs(x.AmountTransaction))
                })
                .Where(x => x.Count >= 2)
                .OrderByDescending(x => x.Count)
                .ThenByDescending(x => x.Total)
                .Take(5)
                .ToList();

            RecurringItems.ItemsSource = recurring;
        }
        private void LoadBudgetControl()
        {
            int userId = App.CurrentUser.IDUser;

            var now = DateTime.Now;

            DateTime startMonth =
                new DateTime(now.Year, now.Month, 1);

            var expenses = App.dB.Transaction
                .Where(t =>
                    t.IDUser == userId &&
                    t.Category.IDTypeCategory == 2 &&
                    t.DateTransaction >= startMonth)
                .ToList();

            if (!expenses.Any())
            {
                txtBudgetControl.Text = "Недостаточно данных";
                return;
            }

            decimal totalExpenses =
                expenses.Sum(t =>
                    Math.Abs(t.AmountTransaction));

            int daysPassed = now.Day;

            if (daysPassed <= 0)
                daysPassed = 1;

            decimal avgPerDay =
                totalExpenses / daysPassed;

            int daysInMonth =
                DateTime.DaysInMonth(now.Year, now.Month);

            decimal forecastMonth =
                avgPerDay * daysInMonth;

            // ВИРТУАЛЬНЫЙ БЮДЖЕТ (берём прогноз доходов)
            decimal income = App.dB.Transaction
                .Where(t =>
                    t.IDUser == userId &&
                    t.Category.IDTypeCategory == 1 &&
                    t.DateTransaction >= startMonth)
                .Sum(t => (decimal?)t.AmountTransaction) ?? 0;

            if (income <= 0)
                income = forecastMonth * 1.2m; // fallback

            decimal usedPercent =
                (totalExpenses / income) * 100;

            if (usedPercent > 100)
                usedPercent = 100;

            // TEXT 1
            txtBudgetControl.Text =
                $"{Math.Round(usedPercent)}% месячного бюджета использовано";

            // TEXT 2 (прогноз)     
            txtBudgetControl.Text =
                $"Прогноз расходов: {forecastMonth:N0} ₽ / месяц";
        }
        private void LoadActivity()
        {
            int userId = App.CurrentUser.IDUser;

            var now = DateTime.Now;

            DateTime startMonth =
                new DateTime(now.Year, now.Month, 1);

            var expenses = App.dB.Transaction
                .Where(t =>
                    t.IDUser == userId &&
                    t.Category.IDTypeCategory == 2 &&
                    t.DateTransaction >= startMonth)
                .ToList();

            if (!expenses.Any())
            {
                txtBestDay.Text = "-";
                txtWorstDay.Text = "-";
                return;
            }

            // группировка по дням недели
            var grouped = expenses
                .GroupBy(t => t.DateTransaction.DayOfWeek)
                .Select(g => new
                {
                    Day = g.Key,
                    Total = g.Sum(x =>
                        Math.Abs(x.AmountTransaction))
                })
                .ToList();

            var best = grouped
                .OrderBy(x => x.Total)
                .First();

            var worst = grouped
                .OrderByDescending(x => x.Total)
                .First();

            txtBestDay.Text = GetDayName(best.Day);
            txtWorstDay.Text = GetDayName(worst.Day);
        }
        private string GetDayName(DayOfWeek day)
        {
            switch (day)
            {
                case DayOfWeek.Monday: return "Понедельник";
                case DayOfWeek.Tuesday: return "Вторник";
                case DayOfWeek.Wednesday: return "Среда";
                case DayOfWeek.Thursday: return "Четверг";
                case DayOfWeek.Friday: return "Пятница";
                case DayOfWeek.Saturday: return "Суббота";
                case DayOfWeek.Sunday: return "Воскресенье";
                default: return "";
            }
        }
        private void LoadHeatmap()
        {
            int userId = App.CurrentUser.IDUser;

            DateTime endDate = DateTime.Now.Date;
            DateTime startDate = endDate.AddDays(-6);

            var expenses = App.dB.Transaction
                .Where(t =>
                    t.IDUser == userId &&
                    t.Category.IDTypeCategory == 2 &&
                    t.DateTransaction >= startDate &&
                    t.DateTransaction <= endDate)
                .ToList();

            // группировка ПО ДАТАМ (ВАЖНО)
            var daily = expenses
                .GroupBy(t => t.DateTransaction.Date)
                .Select(g => new HeatData
                {
                    Date = g.Key,
                    Total = g.Sum(x => Math.Abs(x.AmountTransaction))
                })
                .ToList();

            decimal max = daily.Any() ? daily.Max(x => x.Total) : 1;

            // 7 дней фиксированно
            for (int i = 0; i < 7; i++)
            {
                DateTime day = startDate.AddDays(i);

                decimal value = daily
                    .FirstOrDefault(x => x.Date == day)?.Total ?? 0;

                double intensity = (double)(value / max);

                SetHeatByIndex(i, intensity);
            }
        }
        private void SetHeatByIndex(int index, double intensity)
        {
            Border border = null;

            switch (index)
            {
                case 0: border = HeatMon; break;
                case 1: border = HeatTue; break;
                case 2: border = HeatWed; break;
                case 3: border = HeatThu; break;
                case 4: border = HeatFri; break;
                case 5: border = HeatSat; break;
                case 6: border = HeatSun; break;
            }

            if (border == null)
                return;

            byte alpha;

            if (intensity < 0.2) alpha = 40;
            else if (intensity < 0.4) alpha = 70;
            else if (intensity < 0.6) alpha = 120;
            else if (intensity < 0.8) alpha = 170;
            else alpha = 220;

            border.Background =
                new SolidColorBrush(
                    Color.FromArgb(alpha, 255, 255, 255));
        }
        private void OpenRiskInfo_Click(object sender, RoutedEventArgs e) => OpenPopup("RiskOverlay");

        private void CloseRiskInfo_Click(object sender, RoutedEventArgs e) => ClosePopup("RiskOverlay");
        // =========================
        // OPEN POPUP (UNIVERSAL)
        // =========================
        private void OpenPopup(string name)
        {
            var overlay = this.FindName(name) as UIElement;
            if (overlay != null)
                overlay.Visibility = Visibility.Visible;
        }

        // =========================
        // CLOSE POPUP (UNIVERSAL)
        // =========================
        private void ClosePopup(string name)
        {
            var overlay = this.FindName(name) as UIElement;
            if (overlay != null)
                overlay.Visibility = Visibility.Collapsed;
        }

        // =========================
        // BUTTON HANDLERS (OPEN)
        // =========================

        private void OpenAvgCheckInfo_Click(object sender, RoutedEventArgs e)
            => OpenPopup("AvgCheckOverlay");

        private void OpenSavedMoneyInfo_Click(object sender, RoutedEventArgs e)
            => OpenPopup("SavedMoneyOverlay");

        private void OpenImpulseInfo_Click(object sender, RoutedEventArgs e)
            => OpenPopup("ImpulseOverlay");

        private void OpenStabilityInfo_Click(object sender, RoutedEventArgs e)
            => OpenPopup("StabilityOverlay");

        private void OpenCategoryInfo_Click(object sender, RoutedEventArgs e)
            => OpenPopup("CategoryOverlay");

        private void OpenTrendInfo_Click(object sender, RoutedEventArgs e)
            => OpenPopup("TrendOverlay");

        private void OpenActivityInfo_Click(object sender, RoutedEventArgs e)
            => OpenPopup("ActivityOverlay");

        private void OpenMicroInfo_Click(object sender, RoutedEventArgs e)
            => OpenPopup("MicroOverlay");

        private void OpenBudgetInfo_Click(object sender, RoutedEventArgs e)
            => OpenPopup("BudgetOverlay");

        private void OpenProfileInfo_Click(object sender, RoutedEventArgs e)
            => OpenPopup("ProfileOverlay");

        private void OpenAIInfo_Click(object sender, RoutedEventArgs e)
            => OpenPopup("AIOverlay");

        // =========================
        // BUTTON HANDLERS (CLOSE)
        // =========================

        private void CloseAvgCheckInfo_Click(object sender, RoutedEventArgs e)
            => ClosePopup("AvgCheckOverlay");

        private void CloseSavedMoneyInfo_Click(object sender, RoutedEventArgs e)
            => ClosePopup("SavedMoneyOverlay");

        private void CloseImpulseInfo_Click(object sender, RoutedEventArgs e)
            => ClosePopup("ImpulseOverlay");

        private void CloseStabilityInfo_Click(object sender, RoutedEventArgs e)
            => ClosePopup("StabilityOverlay");

        private void CloseCategoryInfo_Click(object sender, RoutedEventArgs e)
            => ClosePopup("CategoryOverlay");

        private void CloseTrendInfo_Click(object sender, RoutedEventArgs e)
            => ClosePopup("TrendOverlay");

        private void CloseActivityInfo_Click(object sender, RoutedEventArgs e)
            => ClosePopup("ActivityOverlay");

        private void CloseMicroInfo_Click(object sender, RoutedEventArgs e)
            => ClosePopup("MicroOverlay");

        private void CloseBudgetInfo_Click(object sender, RoutedEventArgs e)
            => ClosePopup("BudgetOverlay");

        private void CloseProfileInfo_Click(object sender, RoutedEventArgs e)
            => ClosePopup("ProfileOverlay");

        private void CloseAIInfo_Click(object sender, RoutedEventArgs e)
            => ClosePopup("AIOverlay");
        private void CategoriesPage_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new CategoryPage());
        private void Debt_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new DebtPage());
        private void Transaction_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new Transactions());
        private void Profile_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new ProfilePage());
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
    public class FinancialState
    {
        public string Impulse;   // HIGH / MEDIUM / LOW
        public string Stability;
        public string Evening;
        public string Activity;
    }
    public class HeatData
    {
        public DateTime Date { get; set; }
        public decimal Total { get; set; }
    }
}
