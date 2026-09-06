using System.Collections.Generic;
using GoogleSheetsDemo.Models;
using GoogleSheetsDemo.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GoogleSheetsDemo.Tests
{
    [TestClass]
    public class ProductPagerTests
    {
        private static IList<Product> MakeRows(int count)
        {
            var rows = new List<Product>();
            for (int i = 1; i <= count; i++)
            {
                rows.Add(new Product { Id = i, Name = "Row " + i, Quantity = i, Price = i });
            }
            return rows;
        }

        // --- TotalPages ---

        [TestMethod]
        public void TotalPages_WithZeroRows_ReturnsOne()
        {
            Assert.AreEqual(1, ProductPager.TotalPages(0, 10));
        }

        [TestMethod]
        public void TotalPages_WithExactDivision_ReturnsExactPages()
        {
            Assert.AreEqual(2, ProductPager.TotalPages(20, 10));
        }

        [TestMethod]
        public void TotalPages_WithRemainder_RoundsUp()
        {
            Assert.AreEqual(3, ProductPager.TotalPages(25, 10));
        }

        [TestMethod]
        public void TotalPages_WithSingleRow_ReturnsOne()
        {
            Assert.AreEqual(1, ProductPager.TotalPages(1, 10));
        }

        [TestMethod]
        public void TotalPages_WithInvalidPageSize_ReturnsOne()
        {
            Assert.AreEqual(1, ProductPager.TotalPages(25, 0));
            Assert.AreEqual(1, ProductPager.TotalPages(25, -5));
        }

        // --- ClampPage ---

        [TestMethod]
        public void ClampPage_BelowOne_ReturnsFirst()
        {
            Assert.AreEqual(1, ProductPager.ClampPage(0, 3));
            Assert.AreEqual(1, ProductPager.ClampPage(-5, 3));
        }

        [TestMethod]
        public void ClampPage_AboveRange_ReturnsLastPage()
        {
            Assert.AreEqual(3, ProductPager.ClampPage(99, 3));
        }

        [TestMethod]
        public void ClampPage_InsideRange_StaysUnchanged()
        {
            Assert.AreEqual(2, ProductPager.ClampPage(2, 3));
        }

        // --- Slice ---

        [TestMethod]
        public void Slice_FirstPage_ReturnsFirstRows()
        {
            IList<Product> slice = ProductPager.Slice(MakeRows(25), 1, 10);

            Assert.AreEqual(10, slice.Count);
            Assert.AreEqual(1, slice[0].Id);
            Assert.AreEqual(10, slice[9].Id);
        }

        [TestMethod]
        public void Slice_MiddlePage_ReturnsCorrectWindow()
        {
            IList<Product> slice = ProductPager.Slice(MakeRows(25), 2, 10);

            Assert.AreEqual(10, slice.Count);
            Assert.AreEqual(11, slice[0].Id);
            Assert.AreEqual(20, slice[9].Id);
        }

        [TestMethod]
        public void Slice_LastPage_ReturnsPartialRows()
        {
            IList<Product> slice = ProductPager.Slice(MakeRows(25), 3, 10);

            Assert.AreEqual(5, slice.Count);
            Assert.AreEqual(21, slice[0].Id);
            Assert.AreEqual(25, slice[4].Id);
        }

        [TestMethod]
        public void Slice_PageBeyondRange_ReturnsEmpty()
        {
            Assert.AreEqual(0, ProductPager.Slice(MakeRows(5), 9, 10).Count);
        }

        [TestMethod]
        public void Slice_WithNullList_ReturnsEmpty()
        {
            Assert.AreEqual(0, ProductPager.Slice(null, 1, 10).Count);
        }

        [TestMethod]
        public void Slice_WithInvalidPageSize_ReturnsEmpty()
        {
            Assert.AreEqual(0, ProductPager.Slice(MakeRows(5), 1, 0).Count);
        }
    }
}
