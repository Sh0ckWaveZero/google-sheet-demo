Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Threading.Tasks
Imports Google.Apis.Auth.OAuth2
Imports Google.Apis.Services
Imports Google.Apis.Sheets.v4
Imports Google.Apis.Sheets.v4.Data

Namespace Services

    ''' <summary>
    ''' Real implementation of ISheetService backed by the official
    ''' Google.Apis.Sheets.v4 client, authenticated with a service account key.
    ''' </summary>
    Public Class GoogleSheetService
        Implements ISheetService, IDisposable

        Private ReadOnly _sheetsService As SheetsService
        Private ReadOnly _spreadsheetId As String

        Public Sub New(spreadsheetId As String, credentialsPath As String, applicationName As String)
            If String.IsNullOrWhiteSpace(spreadsheetId) Then
                Throw New ArgumentException("Spreadsheet ID is required.", "spreadsheetId")
            End If
            If String.IsNullOrWhiteSpace(credentialsPath) Then
                Throw New ArgumentException("Path to the Google credentials file is required.", "credentialsPath")
            End If
            If Not File.Exists(credentialsPath) Then
                Throw New FileNotFoundException(
                    "Google credentials file was not found. Follow the setup steps in README.md to create credentials.json.",
                    credentialsPath)
            End If

            _spreadsheetId = spreadsheetId

            Dim credential As GoogleCredential
            Using stream As FileStream = File.OpenRead(credentialsPath)
                credential = GoogleCredential.FromStream(stream) _
                    .CreateScoped(SheetsService.Scope.Spreadsheets)
            End Using

            _sheetsService = New SheetsService(New BaseClientService.Initializer With {
                .HttpClientInitializer = credential,
                .ApplicationName = If(applicationName, "GoogleSheetsDemo")
            })
        End Sub

        Public Async Function GetValuesAsync(range As String) As Task(Of IList(Of IList(Of Object))) Implements ISheetService.GetValuesAsync
            Dim request As SpreadsheetsResource.ValuesResource.GetRequest =
                _sheetsService.Spreadsheets.Values.Get(_spreadsheetId, range)

            Dim response As ValueRange = Await request.ExecuteAsync().ConfigureAwait(False)
            Return If(response.Values, New List(Of IList(Of Object))())
        End Function

        Public Async Function UpdateValuesAsync(range As String, values As IList(Of IList(Of Object))) As Task Implements ISheetService.UpdateValuesAsync
            Dim body As New ValueRange With {.Values = values}

            Dim request As SpreadsheetsResource.ValuesResource.UpdateRequest =
                _sheetsService.Spreadsheets.Values.Update(body, _spreadsheetId, range)
            request.ValueInputOption =
                SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED

            Await request.ExecuteAsync().ConfigureAwait(False)
        End Function

        Public Async Function AppendValuesAsync(range As String, values As IList(Of IList(Of Object))) As Task Implements ISheetService.AppendValuesAsync
            Dim body As New ValueRange With {.Values = values}

            Dim request As SpreadsheetsResource.ValuesResource.AppendRequest =
                _sheetsService.Spreadsheets.Values.Append(body, _spreadsheetId, range)
            request.ValueInputOption =
                SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED
            request.InsertDataOption =
                SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS

            Await request.ExecuteAsync().ConfigureAwait(False)
        End Function

        Public Async Function ClearValuesAsync(range As String) As Task Implements ISheetService.ClearValuesAsync
            Dim request As SpreadsheetsResource.ValuesResource.ClearRequest =
                _sheetsService.Spreadsheets.Values.Clear(New ClearValuesRequest(), _spreadsheetId, range)

            Await request.ExecuteAsync().ConfigureAwait(False)
        End Function

        Public Sub Dispose() Implements IDisposable.Dispose
            If _sheetsService IsNot Nothing Then
                _sheetsService.Dispose()
            End If
        End Sub

    End Class


End Namespace
