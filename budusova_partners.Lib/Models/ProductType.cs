using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace budusova_partners.Lib.Models
{
    [Table("product_types", Schema = "app")]
    public class ProductType
    {
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        // Коллекция продуктов
        public virtual ICollection<Product> Products { get; set; }

        public ProductType()
        {
            Products = new List<Product>();
        }
    }
}
