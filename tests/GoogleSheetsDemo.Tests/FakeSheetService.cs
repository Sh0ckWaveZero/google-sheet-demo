using System.Collections.Generic;
using System.Threading.Tasks;
using GoogleSheetsDemo.Services;

namespace GoogleSheetsDemo.Tests
{
    /// <summary>
    /// Test double for ISheetService. Returns canned rows for Get and records
    /// every write call so tests can assert ranges and values without any
    /// network access.
    /// </summary>
    public class FakeSheetService : ISheetService
    {
        public FakeSheetService(IList<IList<object>> rowsToReturn = null)
        {
            RowsToReturn = rowsToReturn;
        }

        /// <summary>Rows returned by GetValuesAsync (may be null to simulate an empty response).</summary>
        public IList<IList<object>> RowsToReturn { get; set; }

        public List<string> GetRanges { get; } = new List<string>();
        public List<string> UpdateRanges { get; } = new List<string>();
        public List<IList<IList<object>>> UpdatedValues { get; } = new List<IList<IList<object>>>();
        public List<string> AppendRanges { get; } = new List<string>();
        public List<IList<IList<object>>> AppendedValues { get; } = new List<IList<IList<object>>>();
        public List<string> ClearedRanges { get; } = new List<string>();

        /// <summary>Every call in the exact order it happened: Get/Update/Append/Clear.</summary>
        public List<string> CallLog { get; } = new List<string>();

        public Task<IList<IList<object>>> GetValuesAsync(string range)
        {
            GetRanges.Add(range);
            CallLog.Add("Get");
            return Task.FromResult(RowsToReturn);
        }

        public Task UpdateValuesAsync(string range, IList<IList<object>> values)
        {
            UpdateRanges.Add(range);
            UpdatedValues.Add(values);
            CallLog.Add("Update");
            return Task.CompletedTask;
        }

        public Task AppendValuesAsync(string range, IList<IList<object>> values)
        {
            AppendRanges.Add(range);
            AppendedValues.Add(values);
            CallLog.Add("Append");
            return Task.CompletedTask;
        }

        public Task ClearValuesAsync(string range)
        {
            ClearedRanges.Add(range);
            CallLog.Add("Clear");
            return Task.CompletedTask;
        }
    }
}
