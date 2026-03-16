using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace budusova_partners.Lib.Models
{
    [Table("partner_types", Schema = "app")]
    public class PartnerType
    {
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        // Коллекция партнеров
        public virtual ICollection<Partner> Partners { get; set; }

        public PartnerType()
        {
            Partners = new List<Partner>();
        }
    }
}

