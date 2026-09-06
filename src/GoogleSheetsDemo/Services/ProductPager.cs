using System;
using System.Collections.Generic;
using GoogleSheetsDemo.Models;

namespace GoogleSheetsDemo.Services
{
    /// <summary>
    /// Page math for the product grid: how many pages a row count needs, how
    /// to keep the current page inside range, and which rows belong to a page.
    /// Pure logic, no UI types, so the rules are unit testable. The full list
    /// always stays in the caller's hands - the pager only ever returns a view.
    /// </summary>
    public static class ProductPager
    {
        /// <summary>Number of pages needed; an empty list still counts as one (empty) page.</summary>
        public static int TotalPages(int totalRows, int pageSize)
        {
            if (totalRows <= 0 || pageSize <= 0)
            {
                return 1;
            }
            return (totalRows + pageSize - 1) / pageSize;
        }

        /// <summary>Keeps the page number inside 1..totalPages.</summary>
        public static int ClampPage(int page, int totalPages)
        {
            if (page < 1)
            {
                return 1;
            }
            if (page > totalPages)
            {
                return totalPages;
            }
            return page;
        }

        /// <summary>The rows of one page. A page past the end yields an empty list.</summary>
        public static IList<Product> Slice(IList<Product> products, int page, int pageSize)
        {
            var result = new List<Product>();
            if (products == null || pageSize <= 0 || page < 1)
            {
                return result;
            }

            int firstIndex = (page - 1) * pageSize;
            int lastIndex = Math.Min(firstIndex + pageSize, products.Count);
            for (int i = firstIndex; i < lastIndex; i++)
            {
                result.Add(products[i]);
            }
            return result;
        }
    }
}
