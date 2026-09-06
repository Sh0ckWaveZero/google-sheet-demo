// Standalone connectivity probe: calls the real Google Sheets API with the
// same credentials + spreadsheet id the app uses, and prints the full error
// chain so runtime failures can be diagnosed from the console.
using System;
using System.Reflection;

namespace ConnectTest
{
    class Program
    {
        static int Main(string[] args)
        {
            string credentialsPath = args.Length > 0 ? args[0] :
                @"C:\lisa\Demo_Spread_Sheet\src\GoogleSheetsDemo\bin\Debug\credentials.json";
            string spreadsheetId = args.Length > 1 ? args[1] : "1SbOWOfgu2R4cg_PmNtFQA7RxoryUUI_UFsj5fZGIFbs";
            string range = args.Length > 2 ? args[2] : "\u0E0A\u0E35\u0E151!A1:D5"; // "Sheet1" in Thai

            Console.WriteLine("Credentials : " + credentialsPath);
            Console.WriteLine("Spreadsheet : " + spreadsheetId);
            Console.WriteLine("Range       : " + range);
            Console.WriteLine();

            try
            {
                var asm = Assembly.LoadFrom(
                    @"C:\lisa\Demo_Spread_Sheet\src\GoogleSheetsDemo\bin\Debug\GoogleSheetsDemo.exe");
                var svcType = asm.GetType("GoogleSheetsDemo.Services.GoogleSheetService", true);
                var svc = Activator.CreateInstance(svcType,
                    spreadsheetId, credentialsPath, "GoogleSheetsDemo-ConnectTest");

                var method = svcType.GetMethod("GetValuesAsync");
                var task = (System.Threading.Tasks.Task<System.Collections.Generic.IList<System.Collections.Generic.IList<object>>>)
                    method.Invoke(svc, new object[] { range });

                var rows = task.GetAwaiter().GetResult();
                Console.WriteLine("SUCCESS - got " + rows.Count + " row(s):");
                foreach (var row in rows)
                    Console.WriteLine("  [" + string.Join(" | ", row) + "]");
                return 0;
            }
            catch (Exception ex)
            {
                int depth = 0;
                for (Exception cur = ex; cur != null; cur = cur.InnerException)
                {
                    Console.WriteLine((depth == 0 ? "EXCEPTION" : "  INNER " + depth) + ": [" +
                        cur.GetType().FullName + "] " + cur.Message);
                    // GoogleApiException carries the raw HTTP response body
                    Type curType = cur.GetType();
                    PropertyInfo respProp = curType.GetProperty("HttpResponse");
                    if (respProp == null && curType.BaseType != null)
                        respProp = curType.BaseType.GetProperty("HttpResponse");
                    if (respProp != null)
                    {
                        var resp = respProp.GetValue(cur);
                        Console.WriteLine("  HttpResponse: " + resp);
                    }
                    depth++;
                }
                Console.WriteLine();
                Console.WriteLine(ex.StackTrace);
                return 1;
            }
        }
    }
}
