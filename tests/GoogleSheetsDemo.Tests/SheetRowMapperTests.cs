using System.Collections.Generic;
using System.Linq;
using GoogleSheetsDemo.Models;
using GoogleSheetsDemo.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GoogleSheetsDemo.Tests
{
    [TestClass]
    public class SheetRowMapperTests
    {
        [TestMethod]
        public void ToProducts_WithNullRows_ReturnsEmptyList()
        {
            IList<Product> products = SheetRowMapper.ToProducts(null);

            Assert.IsNotNull(products);
            Assert.AreEqual(0, products.Count);
        }

        [TestMethod]
        public void ToProducts_WithValidRow_ParsesAllFields()
        {
            IList<IList<object>> rows = TestRows.Make(
                new object[] { "1", "Keyboard", "10", "199.5" });

            IList<Product> products = SheetRowMapper.ToProducts(rows);

            Assert.AreEqual(1, products.Count);
            Assert.AreEqual(1, products[0].Id);
            Assert.AreEqual("Keyboard", products[0].Name);
            Assert.AreEqual(10, products[0].Quantity);
            Assert.AreEqual(199.5m, products[0].Price);
        }

        [TestMethod]
        public void ToProducts_WithUnparseableId_SkipsRow()
        {
            IList<IList<object>> rows = TestRows.Make(
                new object[] { "abc", "Keyboard", "10", "199.5" });

            Assert.AreEqual(0, SheetRowMapper.ToProducts(rows).Count);
        }

        [TestMethod]
        public void ToProducts_WithBlankName_SkipsRow()
        {
            IList<IList<object>> rows = TestRows.Make(
                new object[] { "1", "   ", "10", "199.5" });

            Assert.AreEqual(0, SheetRowMapper.ToProducts(rows).Count);
        }

        [TestMethod]
        public void ToProducts_WithBlankRow_SkipsRow()
        {
            IList<IList<object>> rows = TestRows.Make(
                new object[] { null, null, null, null });

            Assert.AreEqual(0, SheetRowMapper.ToProducts(rows).Count);
        }

        [TestMethod]
        public void ToProducts_WithShortRow_TreatsMissingCellsAsZero()
        {
            IList<IList<object>> rows = TestRows.Make(
                new object[] { "7", "Mouse" });

            IList<Product> products = SheetRowMapper.ToProducts(rows);

            Assert.AreEqual(1, products.Count);
            Assert.AreEqual(7, products[0].Id);
            Assert.AreEqual("Mouse", products[0].Name);
            Assert.AreEqual(0, products[0].Quantity);
            Assert.AreEqual(0m, products[0].Price);
        }

        [TestMethod]
        public void ToProducts_WithNonNumericQuantity_DefaultsToZero()
        {
            IList<IList<object>> rows = TestRows.Make(
                new object[] { "7", "Mouse", "n/a", "5" });

            Product product = SheetRowMapper.ToProducts(rows)[0];

            Assert.AreEqual(0, product.Quantity);
            Assert.AreEqual(5m, product.Price);
        }

        [TestMethod]
        public void ToProducts_TrimsWhitespaceAroundName()
        {
            IList<IList<object>> rows = TestRows.Make(
                new object[] { "7", "  Mouse  ", "1", "1" });

            Assert.AreEqual("Mouse", SheetRowMapper.ToProducts(rows)[0].Name);
        }

        [TestMethod]
        public void ToRow_WritesCellsInInvariantCulture()
        {
            var product = new Product { Id = 3, Name = "Monitor", Quantity = 4, Price = 1234.5m };

            var row = SheetRowMapper.ToRow(product);

            CollectionAssert.AreEqual(
                new[] { "3", "Monitor", "4", "1234.5" },
                row.ToArray());
        }

        [TestMethod]
        public void ToRow_ThenToProduct_RoundTripsAllFields()
        {
            var product = new Product { Id = 9, Name = "Webcam", Quantity = 2, Price = 88.8m };

            Product restored = SheetRowMapper.ToProduct(SheetRowMapper.ToRow(product));

            Assert.IsNotNull(restored);
            Assert.AreEqual(product.Id, restored.Id);
            Assert.AreEqual(product.Name, restored.Name);
            Assert.AreEqual(product.Quantity, restored.Quantity);
            Assert.AreEqual(product.Price, restored.Price);
        }
    }
}
