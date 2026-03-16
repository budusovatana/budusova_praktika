using budusova_partners.Lib.Data;
using budusova_partners.Lib.Models;
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
using System.Windows.Shapes;

namespace budusova_partners.Wpf.Views
{
    /// <summary>
    /// Логика взаимодействия для AddEditPartnerWindow.xaml
    /// </summary>
    public partial class AddEditPartnerWindow : Window
    {
        private AppDbContext db;
        private Partner currentPartner;

        public AddEditPartnerWindow(Partner partner = null, AppDbContext context = null)
        {
            InitializeComponent();

            db = context ?? new AppDbContext();
            LoadPartnerTypes();

            if (partner != null)
            {
                Title = "Редактирование партнера";
                currentPartner = db.Partners.First(p => p.Id == partner.Id);
                FillFields();
            }
            else
            {
                Title = "Добавление партнера";
            }
        }

        private void LoadPartnerTypes()
        {
            TypeBox.ItemsSource = db.PartnerTypes.ToList();
            TypeBox.DisplayMemberPath = "Name";
            TypeBox.SelectedValuePath = "Id";
        }

        private void FillFields()
        {
            NameBox.Text = currentPartner.Name;
            AddressBox.Text = currentPartner.LegalAddress;
            DirectorBox.Text = currentPartner.DirectorFullName;
            PhoneBox.Text = currentPartner.Phone;
            EmailBox.Text = currentPartner.Email;
            RatingBox.Text = currentPartner.Rating.ToString();
            TypeBox.SelectedValue = currentPartner.PartnerTypeId;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (TypeBox.SelectedValue == null)
            {
                MessageBox.Show("Выберите тип партнера", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(RatingBox.Text, out int rating) || rating < 0 || rating > 10)
            {
                MessageBox.Show("Рейтинг должен быть целым числом от 0 до 10", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (currentPartner == null)
                {
                    currentPartner = new Partner();
                    db.Partners.Add(currentPartner);
                }

                currentPartner.Name = NameBox.Text;
                currentPartner.LegalAddress = AddressBox.Text;
                currentPartner.DirectorFullName = DirectorBox.Text;
                currentPartner.Phone = PhoneBox.Text;
                currentPartner.Email = EmailBox.Text;
                currentPartner.Rating = rating;
                currentPartner.PartnerTypeId = (int)TypeBox.SelectedValue;

                db.SaveChanges();

                // Сохраняем успешный результат
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}

