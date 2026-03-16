using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace budusova_partners.Lib.Models
{
    [Table("partner_sales", Schema = "app")]
    public class PartnerSale
    {
        public int Id { get; set; }

        [Column("partner_id")]
        public int PartnerId { get; set; }

        [Column("product_id")]
        public int ProductId { get; set; }

        [Column("quantity")]
        public int Quantity { get; set; }

        [Column("unit_price")]
        public decimal UnitPrice { get; set; }

        [Column("sale_date")]
        public DateTime SaleDate { get; set; }

        // Навигационные свойства
        [ForeignKey("PartnerId")]
        public virtual Partner Partner { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
    }
}