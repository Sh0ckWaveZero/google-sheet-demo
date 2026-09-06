using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GoogleSheetsDemo.Models;
using GoogleSheetsDemo.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GoogleSheetsDemo.Tests
{
    [TestClass]
    public class ProductRepositoryTests
    {
        private const string SheetName = "Sheet1";
        private static readonly string[] Header = { "ID", "Name", "Quantity", "Price" };

        [TestMethod]
        public async Task GetAllAsync_ReadsDataRangeAndMapsRows()
        {
            var fake = new FakeSheetService(TestRows.Make(
                new object[] { "1", "Keyboard", "10", "199.5" },
                new object[] { "2", "Mouse", "25", "8.9" }));
            var repository = new ProductRepository(fake, SheetName);

            IList<Product> products = await repository.GetAllAsync();

            Assert.AreEqual(SheetName + "!A2:D", fake.GetRanges.Single());
            Assert.AreEqual(2, products.Count);
            Assert.AreEqual("Keyboard", products[0].Name);
            Assert.AreEqual(8.9m, products[1].Price);
        }

        [TestMethod]
        public async Task GetAllAsync_WhenSheetHasNoValues_ReturnsEmptyList()
        {
            var fake = new FakeSheetService(null);
            var repository = new ProductRepository(fake, SheetName);

            IList<Product> products = await repository.GetAllAsync();

            Assert.AreEqual(0, products.Count);
        }

        [TestMethod]
        public async Task AddAsync_WithExistingRows_AppendsRowWithNextId()
        {
            var fake = new FakeSheetService(TestRows.Make(
                new object[] { "3", "Existing", "1", "1" }));
            var repository = new ProductRepository(fake, SheetName);

            int newId = await repository.AddAsync(new Product { Name = "New item", Quantity = 2, Price = 3.5m });

            Assert.AreEqual(4, newId);
            Assert.AreEqual(SheetName + "!A:D", fake.AppendRanges.Single());

            IList<object> appendedRow = fake.AppendedValues.Single().Single();
            CollectionAssert.AreEqual(new[] { "4", "New item", "2", "3.5" }, appendedRow.ToArray());
        }

        [TestMethod]
        public async Task AddAsync_OnEmptySheet_WritesHeaderThenFirstRow()
        {
            var fake = new FakeSheetService(null);
            var repository = new ProductRepository(fake, SheetName);

            int newId = await repository.AddAsync(new Product { Name = "First", Quantity = 1, Price = 1 });

            Assert.AreEqual(1, newId);
            Assert.AreEqual(SheetName + "!A1", fake.UpdateRanges.Single());
            CollectionAssert.AreEqual(Header, fake.UpdatedValues.Single().Single().ToArray());
            Assert.AreEqual(1, fake.AppendedValues.Single().Count);
        }

        [TestMethod]
        public async Task SaveAllAsync_WithProducts_UpdatesDataRange()
        {
            var fake = new FakeSheetService();
            var repository = new ProductRepository(fake, SheetName);

            await repository.SaveAllAsync(new[]
            {
                new Product { Id = 1, Name = "A", Quantity = 1, Price = 1m },
                new Product { Id = 2, Name = "B", Quantity = 2, Price = 2.25m }
            });

            Assert.AreEqual(SheetName + "!A2:D", fake.UpdateRanges.Single());

            IList<IList<object>> savedRows = fake.UpdatedValues.Single();
            Assert.AreEqual(2, savedRows.Count);
            Assert.AreEqual("2.25", savedRows[1][3]);
        }

        [TestMethod]
        public async Task SaveAllAsync_AfterDeletingRows_ClearsBeforeRewriting()
        {
            var fake = new FakeSheetService();
            var repository = new ProductRepository(fake, SheetName);

            // deleting shrinks the list, but the sheet can still hold more
            // rows than the rewrite supplies - without the clear the last
            // old row survives and comes back on the next load
            await repository.SaveAllAsync(new[]
            {
                new Product { Id = 1, Name = "A", Quantity = 1, Price = 1m }
            });

            CollectionAssert.AreEqual(new[] { "Clear", "Update" }, fake.CallLog);
            Assert.AreEqual(SheetName + "!A2:D", fake.ClearedRanges.Single());
            Assert.AreEqual(SheetName + "!A2:D", fake.UpdateRanges.Single());
        }

        [TestMethod]
        public async Task SaveAllAsync_WithNoProducts_ClearsDataRange()
        {
            var fake = new FakeSheetService();
            var repository = new ProductRepository(fake, SheetName);

            await repository.SaveAllAsync(new Product[0]);

            Assert.AreEqual(SheetName + "!A2:D", fake.ClearedRanges.Single());
            Assert.AreEqual(0, fake.UpdateRanges.Count);
            CollectionAssert.AreEqual(new[] { "Clear" }, fake.CallLog);
        }
    }
}
