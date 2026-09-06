using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoogleSheetsDemo.Services
{
    /// <summary>
    /// Small abstraction over the Google Sheets API so the rest of the app
    /// (and the unit tests) does not depend on the real HTTP service.
    /// </summary>
    public interface ISheetService
    {
        /// <summary>Reads cell values from the given A1 range, e.g. "Sheet1!A2:D".</summary>
        Task<IList<IList<object>>> GetValuesAsync(string range);

        /// <summary>Overwrites the given A1 range with the supplied rows.</summary>
        Task UpdateValuesAsync(string range, IList<IList<object>> values);

        /// <summary>Appends the supplied rows after the last row with data.</summary>
        Task AppendValuesAsync(string range, IList<IList<object>> values);

        /// <summary>Clears all values inside the given A1 range.</summary>
        Task ClearValuesAsync(string range);
    }
}
