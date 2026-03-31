using budusova_partners.Lib.Data;
using budusova_partners.Lib.Models;
using budusova_partners.Lib.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
using System.Windows.Shapes;

namespace budusova_partners.Wpf.Views
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AppDbContext db;
        private ObservableCollection<Partner> partnersCollection;

        public MainWindow()
        {
            InitializeComponent();
            db = new AppDbContext();
            LoadPartners();
        }

        private void LoadPartners()
        {
            try
            {
                var partners = db.Partners
                    .Include(p => p.PartnerType)
                    .Include(p => p.PartnerSales)
                    .ThenInclude(ps => ps.Product)
                    .ToList();

                foreach (var partner in partners)
                {
                    int totalQuantity = partner.PartnerSales.Sum(ps => ps.Quantity);
                    int discount = DiscountService.CalculateDiscount(totalQuantity);
                    partner.DiscountText = DiscountService.GetDiscountDescription(discount);
                }

                partnersCollection = new ObservableCollection<Partner>(partners);
                PartnersList.ItemsSource = partnersCollection;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка загрузки партнеров",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PartnersList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PartnersList.SelectedItem is Partner selectedPartner)
            {
                SalesGrid.ItemsSource = selectedPartner.PartnerSales.ToList();
            }
            else
            {
                SalesGrid.ItemsSource = null;
            }
        }

        private void AddPartner_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddEditPartnerWindow(null, db);
            bool? result = window.ShowDialog();
            if (result == true)
            {
                LoadPartners();
                MessageBox.Show("Данные сохранены.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void EditPartner_Click(object sender, RoutedEventArgs e)
        {
            if (PartnersList.SelectedItem == null)
            {
                MessageBox.Show("Выберите партнера для редактирования",
                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedPartner = (Partner)PartnersList.SelectedItem;
            var editWindow = new AddEditPartnerWindow(selectedPartner, db);
            bool? result = editWindow.ShowDialog();

            if (result == true)
            {
                LoadPartners();
                MessageBox.Show("Данные сохранены.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeletePartner_Click(object sender, RoutedEventArgs e)
        {
            if (PartnersList.SelectedItem == null)
            {
                MessageBox.Show("Сначала выберите партнера для удаления.",
                    "Партнер не выбран", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedPartner = (Partner)PartnersList.SelectedItem;

            var result = MessageBox.Show(
                $"Вы действительно хотите удалить партнера \"{selectedPartner.Name}\"?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    db.PartnerSales.RemoveRange(selectedPartner.PartnerSales);
                    db.Partners.Remove(selectedPartner);
                    db.SaveChanges();
                    LoadPartners();
                    MessageBox.Show("Партнер успешно удален.", "Удаление выполнено", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка удаления", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CreateReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var partners = db.Partners
                    .Include(p => p.PartnerType)
                    .Include(p => p.PartnerSales)
                    .ThenInclude(s => s.Product)
                    .ToList();

                StringBuilder report = new StringBuilder();
                report.AppendLine("ОТЧЕТ О ПАРТНЕРАХ И ПРОДАЖАХ");
                report.AppendLine("Дата формирования: " + DateTime.Now);
                report.AppendLine("--------------------------------------------------");

                foreach (var partner in partners)
                {
                    report.AppendLine($"Партнер: {partner.Name}");
                    report.AppendLine($"Тип партнера: {partner.PartnerType?.Name}");
                    report.AppendLine($"Директор: {partner.DirectorFullName}");
                    report.AppendLine($"Телефон: {partner.Phone}");
                    report.AppendLine($"Email: {partner.Email}");
                    report.AppendLine($"Рейтинг: {partner.Rating}");
                    report.AppendLine($"Скидка: {partner.DiscountText}");
                    report.AppendLine("История продаж:");

                    if (partner.PartnerSales.Any())
                    {
                        foreach (var sale in partner.PartnerSales)
                        {
                            report.AppendLine($"   Товар: {sale.Product?.Name} | Количество: {sale.Quantity} | Цена: {sale.UnitPrice} | Дата: {sale.SaleDate:d}");
                        }
                    }
                    else
                    {
                        report.AppendLine("   Продаж нет");
                    }

                    report.AppendLine("--------------------------------------------------");
                }

                string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "partners_report.txt");
                File.WriteAllText(path, report.ToString(), Encoding.UTF8);

                MessageBox.Show($"Отчет успешно создан!\nФайл сохранен: {path}", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при создании отчета: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void AddSale_Click(object sender, RoutedEventArgs e)
        {
            if (PartnersList.SelectedItem == null)
            {
                MessageBox.Show("Сначала выберите партнера, для которого нужно добавить продажу.",
                    "Партнер не выбран",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var selectedPartner = (Partner)PartnersList.SelectedItem;

            var addSaleWindow = new AddSaleWindow(selectedPartner, db);
            bool? result = addSaleWindow.ShowDialog();

            if (result == true)
            {
                int selectedPartnerId = selectedPartner.Id;

                LoadPartners();

                var updatedPartner = partnersCollection.FirstOrDefault(p => p.Id == selectedPartnerId);
                if (updatedPartner != null)
                {
                    PartnersList.SelectedItem = updatedPartner;
                    SalesGrid.ItemsSource = updatedPartner.PartnerSales.ToList();
                }

                MessageBox.Show("Продажа успешно добавлена.",
                    "Успех",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
        private void EditSale_Click(object sender, RoutedEventArgs e)
        {
            if (PartnersList.SelectedItem == null)
            {
                MessageBox.Show("Сначала выберите партнера.",
                    "Партнер не выбран",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (SalesGrid.SelectedItem == null)
            {
                MessageBox.Show("Сначала выберите продажу для редактирования.",
                    "Продажа не выбрана",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var selectedPartner = (Partner)PartnersList.SelectedItem;
            var selectedSale = (PartnerSale)SalesGrid.SelectedItem;

            var editSaleWindow = new AddSaleWindow(selectedPartner, selectedSale, db);
            bool? result = editSaleWindow.ShowDialog();

            if (result == true)
            {
                int selectedPartnerId = selectedPartner.Id;

                LoadPartners();

                var updatedPartner = partnersCollection.FirstOrDefault(p => p.Id == selectedPartnerId);
                if (updatedPartner != null)
                {
                    PartnersList.SelectedItem = updatedPartner;
                    SalesGrid.ItemsSource = updatedPartner.PartnerSales.ToList();
                }

                MessageBox.Show("Продажа успешно изменена.",
                    "Успех",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void DeleteSale_Click(object sender, RoutedEventArgs e)
        {
            if (PartnersList.SelectedItem == null)
            {
                MessageBox.Show("Сначала выберите партнера.",
                    "Партнер не выбран",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (SalesGrid.SelectedItem == null)
            {
                MessageBox.Show("Сначала выберите продажу для удаления.",
                    "Продажа не выбрана",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var selectedPartner = (Partner)PartnersList.SelectedItem;
            var selectedSale = (PartnerSale)SalesGrid.SelectedItem;

            var result = MessageBox.Show(
                "Вы действительно хотите удалить выбранную продажу?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                db.PartnerSales.Remove(selectedSale);
                db.SaveChanges();

                int selectedPartnerId = selectedPartner.Id;

                LoadPartners();

                var updatedPartner = partnersCollection.FirstOrDefault(p => p.Id == selectedPartnerId);
                if (updatedPartner != null)
                {
                    PartnersList.SelectedItem = updatedPartner;
                    SalesGrid.ItemsSource = updatedPartner.PartnerSales.ToList();
                }

                MessageBox.Show("Продажа успешно удалена.",
                    "Удаление выполнено",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления продажи: " + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}