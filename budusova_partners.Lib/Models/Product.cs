using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace budusova_partners.Lib.Models
{
    [Table("products", Schema = "app")]
    public class Product
    {
        public int Id { get; set; }

        [Column("product_type_id")]
        public int ProductTypeId { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("min_partner_price")]
        public decimal MinPartnerPrice { get; set; }

        // Навигационное свойство к типу продукта
        [ForeignKey("ProductTypeId")]
        public virtual ProductType ProductType { get; set; }

        // Коллекция продаж
        public virtual ICollection<PartnerSale> PartnerSales { get; set; }

        public Product()
        {
            PartnerSales = new List<PartnerSale>();
        }
    }
}


