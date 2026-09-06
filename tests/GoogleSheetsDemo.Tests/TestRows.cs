using System.Collections.Generic;

namespace GoogleSheetsDemo.Tests
{
    /// <summary>Helper for building sheet rows in tests.</summary>
    public static class TestRows
    {
        public static IList<IList<object>> Make(params object[][] rows)
        {
            var result = new List<IList<object>>();
            foreach (object[] row in rows)
            {
                result.Add(new List<object>(row));
            }
            return result;
        }
    }
}
