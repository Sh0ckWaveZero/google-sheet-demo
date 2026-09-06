using System;
using System.Collections.Generic;
using System.Globalization;
using GoogleSheetsDemo.Models;

namespace GoogleSheetsDemo.Validation
{
    /// <summary>
    /// All input rules of the app in one place, kept free of any UI types so
    /// the rules can be unit tested without Windows Forms. The form (append
    /// fields) and the grid (rows to save) both go through this class; the
    /// caller only decides how to display the returned failures.
    /// </summary>
    public static class ProductValidator
    {
        public const int NameMaxLength = 100;
        public const int QuantityMaxValue = 1000000000;
        public const decimal PriceMaxValue = 99999999.99m;

        /// <summary>
        /// Parses and validates the append form. Every broken rule is reported
        /// at once; parsed values are only set for fields that passed.
        /// </summary>
        public static ProductInputResult ParseAppendInput(string nameText, string quantityText, string priceText)
        {
            var input = new ProductInputResult();

            string name = (nameText ?? "").Trim();
            if (name.Length == 0)
            {
                input.Validation.AddFailure("Name", "Name is required.");
            }
            else if (name.Length > NameMaxLength)
            {
                input.Validation.AddFailure("Name",
                    string.Format("Name must be {0} characters or fewer (currently {1}).", NameMaxLength, name.Length));
            }
            else
            {
                input.Name = name;
            }

            int quantity;
            if (!TryParseInt(quantityText, out quantity))
            {
                input.Validation.AddFailure("Quantity", "Quantity is required and must be a whole number.");
            }
            else if (!IsQuantityInRange(quantity))
            {
                input.Validation.AddFailure("Quantity", RangeMessage("Quantity", QuantityMaxValue.ToString(CultureInfo.InvariantCulture)));
            }
            else
            {
                input.Quantity = quantity;
            }

            decimal price;
            if (!TryParseDecimal(priceText, out price))
            {
                input.Validation.AddFailure("Price", "Price is required and must be a number.");
            }
            else if (!IsPriceInRange(price))
            {
                input.Validation.AddFailure("Price", RangeMessage("Price", PriceMaxValue.ToString(CultureInfo.InvariantCulture)));
            }
            else
            {
                input.Price = price;
            }

            return input;
        }

        /// <summary>
        /// Validates every row that is about to be written back to the sheet.
        /// Failures carry the grid row number so the dialog can point at the
        /// exact row to fix. A null/empty list is valid (it means "clear").
        /// </summary>
        public static ValidationResult ValidateForSave(IEnumerable<Product> products)
        {
            var result = new ValidationResult();
            if (products == null)
            {
                return result;
            }

            int rowNumber = 1;
            foreach (Product product in products)
            {
                string row = "Row " + rowNumber.ToString(CultureInfo.InvariantCulture);

                if (product == null)
                {
                    result.AddFailure(row, "Row is empty.");
                }
                else
                {
                    if (product.Id < 1)
                    {
                        result.AddFailure(row, "ID must be 1 or greater.");
                    }

                    ValidateNameField(product.Name, row, result);

                    if (!IsQuantityInRange(product.Quantity))
                    {
                        result.AddFailure(row, RangeMessage("Quantity", QuantityMaxValue.ToString(CultureInfo.InvariantCulture)));
                    }

                    if (!IsPriceInRange(product.Price))
                    {
                        result.AddFailure(row, RangeMessage("Price", PriceMaxValue.ToString(CultureInfo.InvariantCulture)));
                    }
                }

                rowNumber++;
            }
            return result;
        }

        private static void ValidateNameField(string name, string label, ValidationResult result)
        {
            string trimmed = (name ?? "").Trim();
            if (trimmed.Length == 0)
            {
                result.AddFailure(label, "Name is required.");
            }
            else if (trimmed.Length > NameMaxLength)
            {
                result.AddFailure(label,
                    string.Format("Name must be {0} characters or fewer (currently {1}).", NameMaxLength, trimmed.Length));
            }
        }

        private static string RangeMessage(string field, string maxText)
        {
            return string.Format("{0} must be between 0 and {1}.", field, maxText);
        }

        private static bool IsQuantityInRange(int quantity)
        {
            return quantity >= 0 && quantity <= QuantityMaxValue;
        }

        private static bool IsPriceInRange(decimal price)
        {
            return price >= 0 && price <= PriceMaxValue;
        }

        private static bool TryParseInt(string text, out int value)
        {
            string trimmed = (text ?? "").Trim();
            value = 0;
            if (trimmed.Length == 0)
            {
                return false;
            }
            if (int.TryParse(trimmed, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
            {
                return true;
            }
            return int.TryParse(trimmed, NumberStyles.Any, CultureInfo.CurrentCulture, out value);
        }

        private static bool TryParseDecimal(string text, out decimal value)
        {
            string trimmed = (text ?? "").Trim();
            value = 0m;
            if (trimmed.Length == 0)
            {
                return false;
            }
            if (decimal.TryParse(trimmed, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
            {
                return true;
            }
            return decimal.TryParse(trimmed, NumberStyles.Any, CultureInfo.CurrentCulture, out value);
        }
    }
}
