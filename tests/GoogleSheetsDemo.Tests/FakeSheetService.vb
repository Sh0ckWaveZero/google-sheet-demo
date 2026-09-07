Imports System.Collections.Generic
Imports System.Threading.Tasks
Imports GoogleSheetsDemo.Services

''' <summary>
''' Test double for ISheetService. Returns canned rows for Get and records
''' every write call so tests can assert ranges and values without any
''' network access.
''' </summary>
Public Class FakeSheetService
    Implements ISheetService

    Public Sub New(Optional rowsToReturn As IList(Of IList(Of Object)) = Nothing)
        Me.RowsToReturn = rowsToReturn
    End Sub

    ''' <summary>Rows returned by GetValuesAsync (may be null to simulate an empty response).</summary>
    Public Property RowsToReturn As IList(Of IList(Of Object))

    Public ReadOnly Property GetRanges As New List(Of String)()
    Public ReadOnly Property UpdateRanges As New List(Of String)()
    Public ReadOnly Property UpdatedValues As New List(Of IList(Of IList(Of Object)))()
    Public ReadOnly Property AppendRanges As New List(Of String)()
    Public ReadOnly Property AppendedValues As New List(Of IList(Of IList(Of Object)))()
    Public ReadOnly Property ClearedRanges As New List(Of String)()

    ''' <summary>Every call in the exact order it happened: Get/Update/Append/Clear.</summary>
    Public ReadOnly Property CallLog As New List(Of String)()

    Public Function GetValuesAsync(range As String) As Task(Of IList(Of IList(Of Object))) Implements ISheetService.GetValuesAsync
        GetRanges.Add(range)
        CallLog.Add("Get")
        Return Task.FromResult(RowsToReturn)
    End Function

    Public Function UpdateValuesAsync(range As String, values As IList(Of IList(Of Object))) As Task Implements ISheetService.UpdateValuesAsync
        UpdateRanges.Add(range)
        UpdatedValues.Add(values)
        CallLog.Add("Update")
        Return Task.CompletedTask
    End Function

    Public Function AppendValuesAsync(range As String, values As IList(Of IList(Of Object))) As Task Implements ISheetService.AppendValuesAsync
        AppendRanges.Add(range)
        AppendedValues.Add(values)
        CallLog.Add("Append")
        Return Task.CompletedTask
    End Function

    Public Function ClearValuesAsync(range As String) As Task Implements ISheetService.ClearValuesAsync
        ClearedRanges.Add(range)
        CallLog.Add("Clear")
        Return Task.CompletedTask
    End Function

End Class
