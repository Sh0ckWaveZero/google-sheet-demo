using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GoogleSheetsDemo.Models;

namespace GoogleSheetsDemo.Services
{
    /// <summary>
    /// Business logic on top of ISheetService: loads products, appends new
    /// rows with an auto-incremented ID, and rewrites the whole data block.
    /// This class is fully unit tested with a fake ISheetService.
    /// </summary>
    public class ProductRepository
    {
        public static readonly string[] HeaderCells = { "ID", "Name", "Quantity", "Price" };

        private readonly ISheetService _sheets;
        private readonly string _sheetName;

        public ProductRepository(ISheetService sheets, string sheetName)
        {
            if (sheets == null)
            {
                throw new ArgumentNullException("sheets");
            }
            _sheets = sheets;
            _sheetName = string.IsNullOrWhiteSpace(sheetName) ? "Sheet1" : sheetName;
        }

        /// <summary>Reads every product below the header row (A2:D).</summary>
        public async Task<IList<Product>> GetAllAsync()
        {
            IList<IList<object>> rows = await _sheets.GetValuesAsync(Range("A2:D")).ConfigureAwait(false);
            return SheetRowMapper.ToProducts(rows);
        }

        /// <summary>
        /// Appends a new row at the end of the sheet. The ID is assigned here
        /// (max existing ID + 1). When the sheet is still empty the header row
        /// is written first.
        /// </summary>
        public async Task<int> AddAsync(Product product)
        {
            if (product == null)
            {
                throw new ArgumentNullException("product");
            }

            IList<Product> existing = await GetAllAsync().ConfigureAwait(false);
            int nextId = existing.Count == 0 ? 1 : existing.Max(p => p.Id) + 1;
            product.Id = nextId;

            if (existing.Count == 0)
            {
                var header = new List<IList<object>> { new List<object>(HeaderCells) };
                await _sheets.UpdateValuesAsync(Range("A1"), header).ConfigureAwait(false);
            }

            var rows = new List<IList<object>> { SheetRowMapper.ToRow(product) };
            await _sheets.AppendValuesAsync(Range("A:D"), rows).ConfigureAwait(false);
            return nextId;
        }

        /// <summary>
        /// Rewrites the whole data block (A2:D) with the given products.
        /// Passing an empty list leaves the data block cleared.
        /// The block is always cleared first: values.update only overwrites
        /// as many rows as it receives, so deleting a row without the clear
        /// would leave the last old row in the sheet and it would come back
        /// on the next load.
        /// </summary>
        public async Task SaveAllAsync(IList<Product> products)
        {
            await _sheets.ClearValuesAsync(Range("A2:D")).ConfigureAwait(false);

            if (products == null || products.Count == 0)
            {
                return;
            }

            var rows = new List<IList<object>>(products.Count);
            foreach (Product product in products)
            {
                rows.Add(SheetRowMapper.ToRow(product));
            }
            await _sheets.UpdateValuesAsync(Range("A2:D"), rows).ConfigureAwait(false);
        }

        private string Range(string localRange)
        {
            return _sheetName + "!" + localRange;
        }
    }
}
