using System;
using System.Collections.Generic;
using System.Globalization;
using GoogleSheetsDemo.Models;

namespace GoogleSheetsDemo.Services
{
    /// <summary>
    /// Converts between raw sheet rows (IList of IList of object) and Product
    /// objects. Rows are skipped when the ID cannot be parsed or the name is
    /// blank; missing cells are treated as empty/zero.
    /// </summary>
    public static class SheetRowMapper
    {
        public static IList<Product> ToProducts(IList<IList<object>> rows)
        {
            var products = new List<Product>();
            if (rows == null)
            {
                return products;
            }

            foreach (IList<object> row in rows)
            {
                Product product = ToProduct(row);
                if (product != null)
                {
                    products.Add(product);
                }
            }
            return products;
        }

        /// <summary>Maps a single sheet row, or returns null when the row must be skipped.</summary>
        public static Product ToProduct(IList<object> row)
        {
            if (row == null || IsEmpty(row))
            {
                return null;
            }

            IList<object> cells = PadRow(row, 4);

            int id;
            if (!TryParseInt(cells[0], out id))
            {
                return null;
            }

            string name = (cells[1] ?? "").ToString().Trim();
            if (name.Length == 0)
            {
                return null;
            }

            int quantity;
            TryParseInt(cells[2], out quantity); // defaults to 0 when not a number

            decimal price;
            TryParseDecimal(cells[3], out price); // defaults to 0 when not a number

            return new Product { Id = id, Name = name, Quantity = quantity, Price = price };
        }

        /// <summary>Builds the sheet row for a product (invariant culture so the sheet always sees "1234.5").</summary>
        public static IList<object> ToRow(Product product)
        {
            if (product == null)
            {
                throw new ArgumentNullException("product");
            }

            return new List<object>
            {
                product.Id.ToString(CultureInfo.InvariantCulture),
                product.Name ?? "",
                product.Quantity.ToString(CultureInfo.InvariantCulture),
                product.Price.ToString(CultureInfo.InvariantCulture)
            };
        }

        private static bool IsEmpty(IList<object> row)
        {
            foreach (object cell in row)
            {
                if (cell != null && cell.ToString().Trim().Length > 0)
                {
                    return false;
                }
            }
            return true;
        }

        private static IList<object> PadRow(IList<object> row, int minimumCells)
        {
            var cells = new List<object>(row);
            while (cells.Count < minimumCells)
            {
                cells.Add(null);
            }
            return cells;
        }

        private static bool TryParseInt(object cell, out int value)
        {
            string text = (cell ?? "").ToString().Trim();
            value = 0;
            if (text.Length == 0)
            {
                return false;
            }
            if (int.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
            {
                return true;
            }
            return int.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out value);
        }

        private static bool TryParseDecimal(object cell, out decimal value)
        {
            string text = (cell ?? "").ToString().Trim();
            value = 0;
            if (text.Length == 0)
            {
                return false;
            }
            if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
            {
                return true;
            }
            return decimal.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out value);
        }
    }
}
