Imports System.Collections.Generic

''' <summary>Helper for building sheet rows in tests.</summary>
Public Module TestRows

    Public Function Make(ParamArray rows As Object()()) As IList(Of IList(Of Object))
        Dim result As New List(Of IList(Of Object))()
        For Each row As Object() In rows
            result.Add(New List(Of Object)(row))
        Next
        Return result
    End Function

End Module
