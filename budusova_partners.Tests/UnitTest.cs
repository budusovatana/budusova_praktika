using budusova_partners.Lib.Data;
using budusova_partners.Lib.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace budusova_partners.Tests
{
    [TestClass]
    public class UnitTest
    {
        private AppDbContext _context;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            _context = new AppDbContext(options);

            _context.PartnerSales.RemoveRange(_context.PartnerSales);
            _context.Products.RemoveRange(_context.Products);
            _context.Partners.RemoveRange(_context.Partners);
            _context.PartnerTypes.RemoveRange(_context.PartnerTypes);
            _context.SaveChanges();

            _context.PartnerTypes.AddRange(
                new PartnerType { Id = 1, Name = "Дистрибьютор" },
                new PartnerType { Id = 2, Name = "Розничный магазин" }
            );

            _context.Partners.AddRange(
                new Partner { Id = 1, PartnerTypeId = 1, Name = "Test Partner 1", DirectorFullName = "Иванов И.И.", Rating = 10 },
                new Partner { Id = 2, PartnerTypeId = 2, Name = "Test Partner 2", DirectorFullName = "Петров П.П.", Rating = 5 }
            );

            _context.Products.AddRange(
                new Product { Id = 1, ProductTypeId = 1, Name = "Product 1", MinPartnerPrice = 100 },
                new Product { Id = 2, ProductTypeId = 1, Name = "Product 2", MinPartnerPrice = 200 }
            );

            _context.PartnerSales.AddRange(
                new PartnerSale { Id = 1, PartnerId = 1, ProductId = 1, Quantity = 50, UnitPrice = 120, SaleDate = DateTime.Today },
                new PartnerSale { Id = 2, PartnerId = 1, ProductId = 2, Quantity = 20, UnitPrice = 220, SaleDate = DateTime.Today }
            );

            _context.SaveChanges();
        }

        [TestMethod]
        public void AddPartner_ShouldIncreaseCount()
        {
            int before = _context.Partners.Count();

            var newPartner = new Partner { PartnerTypeId = 2, Name = "Test Partner 3", DirectorFullName = "Сидоров С.С.", Rating = 7 };
            _context.Partners.Add(newPartner);
            _context.SaveChanges();

            int after = _context.Partners.Count();
            Assert.AreEqual(before + 1, after);
        }

        [TestMethod]
        public void PartnerTotalSalesQuantity_ShouldReturnCorrectSum()
        {
            var totalQuantity = _context.PartnerSales
                .Where(ps => ps.PartnerId == 1)
                .Sum(ps => ps.Quantity);

            Assert.AreEqual(70, totalQuantity);
        }

        [TestMethod]
        public void PartnerDiscountPercent_ShouldReturnCorrectValue()
        {
            var totalQuantity = _context.PartnerSales
                .Where(ps => ps.PartnerId == 1)
                .Sum(ps => ps.Quantity); // 70

            int discount = 0;
            if (totalQuantity >= 300000)
                discount = 15;
            else if (totalQuantity >= 50000)
                discount = 10;
            else if (totalQuantity >= 10000)
                discount = 5;
            else
                discount = 0;

            Assert.AreEqual(0, discount);
        }

        [TestMethod]
        public void AddPartnerSale_ShouldAddSale()
        {
            int before = _context.PartnerSales.Count();

            var sale = new PartnerSale { PartnerId = 2, ProductId = 1, Quantity = 10, UnitPrice = 150, SaleDate = DateTime.Today };
            _context.PartnerSales.Add(sale);
            _context.SaveChanges();

            int after = _context.PartnerSales.Count();
            Assert.AreEqual(before + 1, after);
        }
        [TestMethod]
        public void AddPartner_ShouldStoreCorrectFields()
        {
            var newPartner = new Partner
            {
                PartnerTypeId = 2,
                Name = "Test Partner 4",
                DirectorFullName = "Козлов К.К.",
                Rating = 8,
                LegalAddress = "ул. Пушкина, 10",
                Phone = "+70000000000",
                Email = "kozlov@example.com"
            };
            _context.Partners.Add(newPartner);
            _context.SaveChanges();

            var added = _context.Partners.Single(p => p.Name == "Test Partner 4");
            Assert.AreEqual(2, added.PartnerTypeId);
            Assert.AreEqual("Козлов К.К.", added.DirectorFullName);
            Assert.AreEqual(8, added.Rating);
            Assert.AreEqual("ул. Пушкина, 10", added.LegalAddress);
            Assert.AreEqual("+70000000000", added.Phone);
            Assert.AreEqual("kozlov@example.com", added.Email);
        }
        [TestMethod]
        public void PartnerTotalRevenue_ShouldReturnCorrectSum()
        {
            var totalRevenue = _context.PartnerSales
                .Where(ps => ps.PartnerId == 1)
                .Sum(ps => ps.Quantity * ps.UnitPrice);

            // Продажи партнёра 1: 50*120 + 20*220 = 6000 + 4400 = 10400
            Assert.AreEqual(10400, totalRevenue);
        }
        [TestMethod]
        public void DeletePartner_ShouldRemoveAssociatedSales()
        {
            var partner = _context.Partners.First(p => p.Id == 1);
            _context.Partners.Remove(partner);
            _context.SaveChanges();
           
            var sales = _context.PartnerSales.Where(ps => ps.PartnerId == 1).ToList();
            Assert.AreEqual(0, sales.Count);
        }
        [TestMethod]
        public void UpdatePartnerRating_ShouldReflectChange()
        {
            var partner = _context.Partners.First(p => p.Id == 2);
            partner.Rating = 9;
            _context.SaveChanges();

            var updated = _context.Partners.First(p => p.Id == 2);
            Assert.AreEqual(9, updated.Rating);
        }
        [TestMethod]
        public void AddPartnerSale_ShouldBeSavedInDatabase()
        {
            var sale = new PartnerSale
            {
                PartnerId = 1,
                ProductId = 1,
                Quantity = 12000,
                UnitPrice = 1200,
                SaleDate = DateTime.Today
            };

            _context.PartnerSales.Add(sale);
            _context.SaveChanges();

            var addedSale = _context.PartnerSales.FirstOrDefault(s => s.PartnerId == 1 && s.ProductId == 1 && s.Quantity == 12000);

            Assert.IsNotNull(addedSale);
            Assert.AreEqual(12000, addedSale.Quantity);
            Assert.AreEqual(1200m, addedSale.UnitPrice);
            Assert.AreEqual(DateTime.Today, addedSale.SaleDate);
        }

    }
}