using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace budusova_partners.Lib.Services
{
    public class DiscountService
    {
        public static int CalculateDiscount(decimal totalSales)
        {
            if (totalSales < 0)
                throw new ArgumentException("Сумма продаж не может быть отрицательной");

            if (totalSales < 10000m) return 0;
            if (totalSales < 50000m) return 5;
            if (totalSales < 300000m) return 10;
            return 15;
        }

        public static string GetDiscountDescription(int discountPercent)
        {
            return discountPercent > 0 ? $"{discountPercent}%" : "нет";
        }
    }
    }

