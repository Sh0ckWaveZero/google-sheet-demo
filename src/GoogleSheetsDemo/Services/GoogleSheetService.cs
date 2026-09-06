using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;

namespace GoogleSheetsDemo.Services
{
    /// <summary>
    /// Real implementation of ISheetService backed by the official
    /// Google.Apis.Sheets.v4 client, authenticated with a service account key.
    /// </summary>
    public class GoogleSheetService : ISheetService, IDisposable
    {
        private readonly SheetsService _sheetsService;
        private readonly string _spreadsheetId;

        public GoogleSheetService(string spreadsheetId, string credentialsPath, string applicationName)
        {
            if (string.IsNullOrWhiteSpace(spreadsheetId))
                throw new ArgumentException("Spreadsheet ID is required.", "spreadsheetId");
            if (string.IsNullOrWhiteSpace(credentialsPath))
                throw new ArgumentException("Path to the Google credentials file is required.", "credentialsPath");
            if (!File.Exists(credentialsPath))
                throw new FileNotFoundException(
                    "Google credentials file was not found. Follow the setup steps in README.md to create credentials.json.",
                    credentialsPath);

            _spreadsheetId = spreadsheetId;

            GoogleCredential credential;
            using (FileStream stream = File.OpenRead(credentialsPath))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(SheetsService.Scope.Spreadsheets);
            }

            _sheetsService = new SheetsService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = applicationName ?? "GoogleSheetsDemo"
            });
        }

        public async Task<IList<IList<object>>> GetValuesAsync(string range)
        {
            SpreadsheetsResource.ValuesResource.GetRequest request =
                _sheetsService.Spreadsheets.Values.Get(_spreadsheetId, range);

            ValueRange response = await request.ExecuteAsync().ConfigureAwait(false);
            return response.Values ?? new List<IList<object>>();
        }

        public async Task UpdateValuesAsync(string range, IList<IList<object>> values)
        {
            var body = new ValueRange { Values = values };

            SpreadsheetsResource.ValuesResource.UpdateRequest request =
                _sheetsService.Spreadsheets.Values.Update(body, _spreadsheetId, range);
            request.ValueInputOption =
                SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;

            await request.ExecuteAsync().ConfigureAwait(false);
        }

        public async Task AppendValuesAsync(string range, IList<IList<object>> values)
        {
            var body = new ValueRange { Values = values };

            SpreadsheetsResource.ValuesResource.AppendRequest request =
                _sheetsService.Spreadsheets.Values.Append(body, _spreadsheetId, range);
            request.ValueInputOption =
                SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            request.InsertDataOption =
                SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;

            await request.ExecuteAsync().ConfigureAwait(false);
        }

        public async Task ClearValuesAsync(string range)
        {
            SpreadsheetsResource.ValuesResource.ClearRequest request =
                _sheetsService.Spreadsheets.Values.Clear(new ClearValuesRequest(), _spreadsheetId, range);

            await request.ExecuteAsync().ConfigureAwait(false);
        }

        public void Dispose()
        {
            if (_sheetsService != null)
            {
                _sheetsService.Dispose();
            }
        }
    }
}
