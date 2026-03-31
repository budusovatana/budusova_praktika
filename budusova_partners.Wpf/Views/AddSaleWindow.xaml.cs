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
        private readonly PartnerSale editingSale;

        // Режим добавления
        public AddSaleWindow(Partner selectedPartner, AppDbContext context)
        {
            InitializeComponent();

            partner = selectedPartner;
            db = context;
            editingSale = null;

            Title = "Добавление продажи";
            LoadData();
        }

        // Режим редактирования
        public AddSaleWindow(Partner selectedPartner, PartnerSale selectedSale, AppDbContext context)
        {
            InitializeComponent();

            partner = selectedPartner;
            db = context;
            editingSale = selectedSale;

            Title = "Редактирование продажи";
            LoadData();
            FillSaleData();
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

                if (editingSale == null)
                {
                    SaleDatePicker.SelectedDate = DateTime.Today;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void FillSaleData()
        {
            try
            {
                if (editingSale == null)
                    return;

                ProductsComboBox.SelectedValue = editingSale.ProductId;
                QuantityTextBox.Text = editingSale.Quantity.ToString();
                UnitPriceTextBox.Text = editingSale.UnitPrice.ToString();
                SaleDatePicker.SelectedDate = editingSale.SaleDate;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки продажи: " + ex.Message,
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

                if (editingSale == null)
                {
                    var sale = new PartnerSale
                    {
                        PartnerId = partner.Id,
                        ProductId = selectedProduct.Id,
                        Quantity = quantity,
                        UnitPrice = unitPrice,
                        SaleDate = SaleDatePicker.SelectedDate.Value
                    };

                    db.PartnerSales.Add(sale);
                }
                else
                {
                    editingSale.ProductId = selectedProduct.Id;
                    editingSale.Quantity = quantity;
                    editingSale.UnitPrice = unitPrice;
                    editingSale.SaleDate = SaleDatePicker.SelectedDate.Value;
                }

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
                MessageBox.Show("Ошибка при сохранении продажи: " + ex.Message,
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