using System;

namespace budusova_partners.Lib.Services
{
    public class DiscountService
    {
        public static int CalculateDiscount(int totalQuantity)
        {
            if (totalQuantity < 0)
                throw new ArgumentException("Количество проданной продукции не может быть отрицательным");

            if (totalQuantity < 10000) return 0;
            if (totalQuantity < 50000) return 5;
            if (totalQuantity < 300000) return 10;
            return 15;
        }

        public static string GetDiscountDescription(int discountPercent)
        {
            return discountPercent > 0 ? $"{discountPercent}%" : "нет";
        }
    }
}