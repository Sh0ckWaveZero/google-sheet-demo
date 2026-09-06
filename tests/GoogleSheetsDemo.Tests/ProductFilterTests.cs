using System.Collections.Generic;
using System.Linq;
using GoogleSheetsDemo.Models;
using GoogleSheetsDemo.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GoogleSheetsDemo.Tests
{
    [TestClass]
    public class ProductFilterTests
    {
        private static Product Make(int id, string name)
        {
            return new Product { Id = id, Name = name, Quantity = 1, Price = 1m };
        }

        [TestMethod]
        public void Matches_WithEmptyQuery_ReturnsTrue()
        {
            Assert.IsTrue(ProductFilter.Matches(Make(1, "Keyboard"), ""));
        }

        [TestMethod]
        public void Matches_WithWhitespaceQuery_ReturnsTrue()
        {
            Assert.IsTrue(ProductFilter.Matches(Make(1, "Keyboard"), "   "));
        }

        [TestMethod]
        public void Matches_ByName_IsCaseInsensitive()
        {
            Assert.IsTrue(ProductFilter.Matches(Make(1, "Keyboard"), "KEY"));
            Assert.IsTrue(ProductFilter.Matches(Make(1, "Keyboard"), "board"));
        }

        [TestMethod]
        public void Matches_ByNameSubstring_NotFound_ReturnsFalse()
        {
            Assert.IsFalse(ProductFilter.Matches(Make(1, "Keyboard"), "mouse"));
        }

        [TestMethod]
        public void Matches_ByIdExact_ReturnsTrue()
        {
            Assert.IsTrue(ProductFilter.Matches(Make(7, "Keyboard"), "7"));
        }

        [TestMethod]
        public void Matches_ByIdPartialDigit_DoesNotMatchOtherIds()
        {
            Assert.IsFalse(ProductFilter.Matches(Make(20, "Keyboard"), "2"));
        }

        [TestMethod]
        public void Matches_TrimsQueryBeforeComparing()
        {
            Assert.IsTrue(ProductFilter.Matches(Make(7, "Keyboard"), "  7  "));
            Assert.IsTrue(ProductFilter.Matches(Make(7, "Keyboard"), " key "));
        }

        [TestMethod]
        public void Matches_WithNullProduct_ReturnsFalse()
        {
            Assert.IsFalse(ProductFilter.Matches(null, "keyboard"));
        }

        [TestMethod]
        public void Apply_KeepsOnlyMatches_PreservesOrder()
        {
            var products = new[]
            {
                Make(1, "Keyboard"),
                Make(2, "Mouse"),
                Make(12, "Webcam"),
                Make(3, "keyboard tray")
            };

            IList<Product> result = ProductFilter.Apply(products, "key");

            CollectionAssert.AreEquivalent(new[] { 1, 3 }, result.Select(p => p.Id).ToArray());
            Assert.AreEqual(1, result[0].Id); // original order preserved
            Assert.AreEqual(3, result[1].Id);
        }

        [TestMethod]
        public void Apply_WithBlankQuery_ReturnsEverything()
        {
            var products = new[] { Make(1, "A"), Make(2, "B") };

            Assert.AreEqual(2, ProductFilter.Apply(products, "").Count);
        }

        [TestMethod]
        public void Apply_WithNullList_ReturnsEmptyList()
        {
            Assert.AreEqual(0, ProductFilter.Apply(null, "x").Count);
        }
    }
}
