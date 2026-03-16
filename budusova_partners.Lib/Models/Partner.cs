using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace budusova_partners.Lib.Models
    {
    [Table("partners", Schema = "app")]
    public class Partner
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("partner_type_id")]
        public int PartnerTypeId { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("legal_address")]
        public string LegalAddress { get; set; }

        [Column("director_full_name")]
        public string DirectorFullName { get; set; }

        [Column("phone")]
        public string Phone { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("rating")]
        public int Rating { get; set; }

        [NotMapped]
        public string DiscountText { get; set; }

        // Навигационное свойство к типу партнера с ForeignKey
        [ForeignKey("PartnerTypeId")]
        public virtual PartnerType PartnerType { get; set; }

        // Коллекция продаж
        public virtual ICollection<PartnerSale> PartnerSales { get; set; }

        public Partner()
        {
            PartnerSales = new List<PartnerSale>();
        }
    }
}
