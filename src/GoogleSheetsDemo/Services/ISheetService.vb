Imports System.Collections.Generic
Imports System.Threading.Tasks

Namespace Services

    ''' <summary>
    ''' Small abstraction over the Google Sheets API so the rest of the app
    ''' (and the unit tests) does not depend on the real HTTP service.
    ''' </summary>
    Public Interface ISheetService

        ''' <summary>Reads cell values from the given A1 range, e.g. "Sheet1!A2:D".</summary>
        Function GetValuesAsync(range As String) As Task(Of IList(Of IList(Of Object)))

        ''' <summary>Overwrites the given A1 range with the supplied rows.</summary>
        Function UpdateValuesAsync(range As String, values As IList(Of IList(Of Object))) As Task

        ''' <summary>Appends the supplied rows after the last row with data.</summary>
        Function AppendValuesAsync(range As String, values As IList(Of IList(Of Object))) As Task

        ''' <summary>Clears all values inside the given A1 range.</summary>
        Function ClearValuesAsync(range As String) As Task

    End Interface


End Namespace
