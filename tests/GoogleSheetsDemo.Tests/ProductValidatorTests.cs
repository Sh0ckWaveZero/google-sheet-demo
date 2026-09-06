using System.Linq;
using GoogleSheetsDemo.Models;
using GoogleSheetsDemo.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GoogleSheetsDemo.Tests
{
    [TestClass]
    public class ProductValidatorTests
    {
        // --- ParseAppendInput: the append form ---

        [TestMethod]
        public void ParseAppendInput_WithValidInput_ReturnsParsedValues()
        {
            ProductInputResult input = ProductValidator.ParseAppendInput("Keyboard", "10", "199.5");

            Assert.IsTrue(input.IsValid);
            Assert.AreEqual(0, input.Validation.Failures.Count);
            Assert.AreEqual("Keyboard", input.Name);
            Assert.AreEqual(10, input.Quantity);
            Assert.AreEqual(199.5m, input.Price);
        }

        [TestMethod]
        public void ParseAppendInput_TrimsWhitespaceAroundName()
        {
            ProductInputResult input = ProductValidator.ParseAppendInput("  Keyboard  ", "1", "1");

            Assert.IsTrue(input.IsValid);
            Assert.AreEqual("Keyboard", input.Name);
        }

        [TestMethod]
        public void ParseAppendInput_WithBlankName_FailsWithRequiredMessage()
        {
            ProductInputResult input = ProductValidator.ParseAppendInput("   ", "1", "1");

            Assert.IsFalse(input.IsValid);
            Assert.AreEqual("Name", input.Validation.Failures.Single().Field);
            Assert.IsTrue(input.Validation.Failures.Single().Message.Contains("required"));
        }

        [TestMethod]
        public void ParseAppendInput_WithNameLongerThanMax_FailsWithMaxLengthMessage()
        {
            string longName = new string('x', ProductValidator.NameMaxLength + 1);

            ProductInputResult input = ProductValidator.ParseAppendInput(longName, "1", "1");

            Assert.IsFalse(input.IsValid);
            Assert.AreEqual("Name", input.Validation.Failures.Single().Field);
            Assert.IsTrue(input.Validation.Failures.Single().Message.Contains(ProductValidator.NameMaxLength.ToString()));
        }

        [TestMethod]
        public void ParseAppendInput_WithEmptyQuantity_FailsAsRequired()
        {
            ProductInputResult input = ProductValidator.ParseAppendInput("Keyboard", "", "1");

            Assert.IsFalse(input.IsValid);
            Assert.AreEqual("Quantity", input.Validation.Failures.Single().Field);
        }

        [TestMethod]
        public void ParseAppendInput_WithNonNumericQuantity_Fails()
        {
            ProductInputResult input = ProductValidator.ParseAppendInput("Keyboard", "ten", "1");

            Assert.IsFalse(input.IsValid);
            Assert.AreEqual("Quantity", input.Validation.Failures.Single().Field);
        }

        [TestMethod]
        public void ParseAppendInput_WithNegativeQuantity_FailsWithRangeMessage()
        {
            ProductInputResult input = ProductValidator.ParseAppendInput("Keyboard", "-3", "1");

            Assert.IsFalse(input.IsValid);
            Assert.AreEqual("Quantity", input.Validation.Failures.Single().Field);
            Assert.IsTrue(input.Validation.Failures.Single().Message.Contains("between"));
        }

        [TestMethod]
        public void ParseAppendInput_WithThousandsSeparator_ParsesNumber()
        {
            ProductInputResult input = ProductValidator.ParseAppendInput("Keyboard", "1,500", "1");

            Assert.IsTrue(input.IsValid);
            Assert.AreEqual(1500, input.Quantity);
        }

        [TestMethod]
        public void ParseAppendInput_WithEmptyPrice_FailsAsRequired()
        {
            ProductInputResult input = ProductValidator.ParseAppendInput("Keyboard", "1", "  ");

            Assert.IsFalse(input.IsValid);
            Assert.AreEqual("Price", input.Validation.Failures.Single().Field);
        }

        [TestMethod]
        public void ParseAppendInput_WithNegativePrice_FailsWithRangeMessage()
        {
            ProductInputResult input = ProductValidator.ParseAppendInput("Keyboard", "1", "-0.5");

            Assert.IsFalse(input.IsValid);
            Assert.AreEqual("Price", input.Validation.Failures.Single().Field);
            Assert.IsTrue(input.Validation.Failures.Single().Message.Contains("between"));
        }

        [TestMethod]
        public void ParseAppendInput_WithEveryFieldBroken_ReportsAllFailuresAtOnce()
        {
            ProductInputResult input = ProductValidator.ParseAppendInput("", "abc", "abc");

            Assert.IsFalse(input.IsValid);
            Assert.AreEqual(3, input.Validation.Failures.Count);
            CollectionAssert.AreEquivalent(
                new[] { "Name", "Quantity", "Price" },
                input.Validation.Failures.Select(f => f.Field).ToArray());
        }

        // --- ValidateForSave: the grid before writing back ---

        [TestMethod]
        public void ValidateForSave_WithValidRows_ReturnsValid()
        {
            var products = new[]
            {
                new Product { Id = 1, Name = "A", Quantity = 1, Price = 1m },
                new Product { Id = 2, Name = "B", Quantity = 0, Price = 0m }
            };

            ValidationResult result = ProductValidator.ValidateForSave(products);

            Assert.IsTrue(result.IsValid);
        }

        [TestMethod]
        public void ValidateForSave_WithEmptyList_ReturnsValid()
        {
            ValidationResult result = ProductValidator.ValidateForSave(new Product[0]);

            Assert.IsTrue(result.IsValid);
        }

        [TestMethod]
        public void ValidateForSave_WithBlankName_ReportsGridRowNumber()
        {
            var products = new[]
            {
                new Product { Id = 1, Name = "A", Quantity = 1, Price = 1m },
                new Product { Id = 2, Name = "   ", Quantity = 1, Price = 1m }
            };

            ValidationResult result = ProductValidator.ValidateForSave(products);

            Assert.IsFalse(result.IsValid);
            ValidationFailure failure = result.Failures.Single();
            Assert.AreEqual("Row 2", failure.Field);
            Assert.IsTrue(failure.Message.Contains("required"));
        }

        [TestMethod]
        public void ValidateForSave_WithInvalidId_ReportsGridRowNumber()
        {
            var products = new[]
            {
                new Product { Id = 0, Name = "A", Quantity = 1, Price = 1m }
            };

            ValidationResult result = ProductValidator.ValidateForSave(products);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual("Row 1", result.Failures.Single().Field);
            Assert.IsTrue(result.Failures.Single().Message.Contains("ID"));
        }

        [TestMethod]
        public void ValidateForSave_WithOutOfRangePrice_ReportsRange()
        {
            var products = new[]
            {
                new Product { Id = 1, Name = "A", Quantity = 1, Price = ProductValidator.PriceMaxValue + 1m }
            };

            ValidationResult result = ProductValidator.ValidateForSave(products);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual("Row 1", result.Failures.Single().Field);
            Assert.IsTrue(result.Failures.Single().Message.Contains("between"));
        }

        [TestMethod]
        public void ValidateForSave_WithSeveralBrokenRows_ListsEachRow()
        {
            var products = new[]
            {
                new Product { Id = 0, Name = "", Quantity = -1, Price = -1m },
                new Product { Id = 5, Name = "OK", Quantity = 1, Price = 1m },
                new Product { Id = 6, Name = "  ", Quantity = 1, Price = 1m }
            };

            ValidationResult result = ProductValidator.ValidateForSave(products);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(5, result.Failures.Count);
            CollectionAssert.AreEquivalent(
                new[] { "Row 1", "Row 1", "Row 1", "Row 1", "Row 3" },
                result.Failures.Select(f => f.Field).ToArray());
        }
    }
}
