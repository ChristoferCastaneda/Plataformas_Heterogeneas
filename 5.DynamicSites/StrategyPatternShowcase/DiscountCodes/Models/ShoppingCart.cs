namespace DiscountCodes.Models
{
    public class ShoppingCart
    {
        private List<Product> items = new List<Product>();
        private string currentDiscountCode = "";

        public IReadOnlyList<Product> Items => items.AsReadOnly();

        public void AddItem(Product product)
        {
            items.Add(product);
        }

        /// <summary>
        /// Returns the total price of all items in the cart, after applying any discounts.
        /// </summary>
        /// <returns></returns>
        public decimal GetTotal()
        {
            if (string.IsNullOrEmpty(currentDiscountCode))
            {
                return items.Sum(item => item.Price);
            }

            return ApplyDiscountToTotal();
        }

        /// <summary>
        /// Applies a discount code to the shopping cart.
        /// If the code is invalid or empty, no discount is applied.
        /// Only one discount code can be applied at a time, applying a new code replaces the previous one.
        /// </summary>
        /// <param name="code"></param>
        public void ApplyDiscount(string code)
        {
            currentDiscountCode = code;
        }

        private decimal ApplyDiscountToTotal()
        {
            switch (currentDiscountCode)
            {
                case "BOGOFREE":
                    return ApplyBogoFree();
                case "BRAND2DISCOUNT":
                    return ApplyBrand2Discount();
                case "10PERCENTOFF":
                    return Apply10PercentOff();
                case "5USDOFF":
                    return Apply5UsdOff();
                default:
                    return items.Sum(item => item.Price);
            }
        }


        // La verdad profe hay algo que no funcionaba y copilot me ayudo a arreglarlo
        private decimal ApplyBogoFree()
        {
            var groups = items.GroupBy(p => new { p.Brand, p.Name, p.Price });
            decimal total = 0;

            foreach (var group in groups)
            {
                int count = group.Count();
                int paidItems = (count + 1) / 2;
                total += paidItems * group.Key.Price;
            }

            return total;
        }

        private decimal ApplyBrand2Discount()
        {
            decimal total = 0;
            foreach (var item in items)
            {
                if (item.Brand == "Brand2")
                {
                    total += item.Price * 0.9m;
                }
                else
                {
                    total += item.Price;
                }
            }
            return total;
        }

        private decimal Apply10PercentOff()
        {
            return items.Sum(item => item.Price) * 0.9m;
        }

        private decimal Apply5UsdOff()
        {
            decimal originalTotal = items.Sum(item => item.Price);
            return Math.Max(0, originalTotal - 5.00m);
        }
    }
}