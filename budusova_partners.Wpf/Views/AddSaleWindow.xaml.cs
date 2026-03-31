using budusova_partners.Lib.Data;
using budusova_partners.Lib.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;

namespace budusova_partners.Wpf.Views
{
    public partial class AddSaleWindow : Window
    {
        private readonly AppDbContext db;
        private readonly Partner partner;

        public AddSaleWindow(Partner selectedPartner, AppDbContext context)
        {
            InitializeComponent();

            partner = selectedPartner;
            db = context;

            LoadData();
        }

        private void LoadData()
        {
            try
            {
                PartnerNameTextBox.Text = partner.Name;

                var products = db.Products
                    .OrderBy(p => p.Name)
                    .ToList();

                ProductsComboBox.ItemsSource = products;

                SaleDatePicker.SelectedDate = DateTime.Today;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ProductsComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите продукт.",
                        "Ошибка ввода",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(QuantityTextBox.Text, out int quantity) || quantity <= 0)
                {
                    MessageBox.Show("Количество должно быть целым числом больше 0.",
                        "Ошибка ввода",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                if (!decimal.TryParse(UnitPriceTextBox.Text, out decimal unitPrice) || unitPrice <= 0)
                {
                    MessageBox.Show("Цена должна быть числом больше 0.",
                        "Ошибка ввода",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                if (SaleDatePicker.SelectedDate == null)
                {
                    MessageBox.Show("Выберите дату продажи.",
                        "Ошибка ввода",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                var selectedProduct = (Product)ProductsComboBox.SelectedItem;

                var sale = new PartnerSale
                {
                    PartnerId = partner.Id,
                    ProductId = selectedProduct.Id,
                    Quantity = quantity,
                    UnitPrice = unitPrice,
                    SaleDate = SaleDatePicker.SelectedDate.Value
                };

                db.PartnerSales.Add(sale);
                db.SaveChanges();

                DialogResult = true;
                Close();
            }
            catch (DbUpdateException ex)
            {
                MessageBox.Show("Ошибка сохранения в базе данных: " + ex.Message,
                    "Ошибка БД",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при добавлении продажи: " + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}