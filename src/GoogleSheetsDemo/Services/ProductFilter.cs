using System;
using System.Collections.Generic;
using System.Globalization;
using GoogleSheetsDemo.Models;

namespace GoogleSheetsDemo.Services
{
    /// <summary>
    /// Search rules for the product grid: a query matches a product when it
    /// equals the product's ID or appears inside its name (case-insensitive).
    /// Pure logic, no UI types, so the rules are unit testable.
    /// </summary>
    public static class ProductFilter
    {
        /// <summary>True when the product should be shown for the given query.</summary>
        public static bool Matches(Product product, string query)
        {
            if (product == null)
            {
                return false;
            }

            string term = (query ?? "").Trim();
            if (term.Length == 0)
            {
                return true;
            }

            int id;
            if (int.TryParse(term, NumberStyles.Integer, CultureInfo.InvariantCulture, out id) && product.Id == id)
            {
                return true;
            }

            return (product.Name ?? "").IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>Filters the list preserving its order. Empty/whitespace query returns everything.</summary>
        public static IList<Product> Apply(IEnumerable<Product> products, string query)
        {
            var result = new List<Product>();
            if (products == null)
            {
                return result;
            }

            foreach (Product product in products)
            {
                if (Matches(product, query))
                {
                    result.Add(product);
                }
            }
            return result;
        }
    }
}
