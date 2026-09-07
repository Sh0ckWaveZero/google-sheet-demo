Imports System.Collections.Generic
Imports System.Text

Namespace Validation

    ''' <summary>
    ''' Outcome of validating one input or a batch of rows: zero failures means
    ''' valid. Carries every failure at once so the user can fix them in a
    ''' single pass instead of one dialog per field.
    ''' </summary>
    Public Class ValidationResult

        Private ReadOnly _failures As New List(Of ValidationFailure)()

        Public ReadOnly Property IsValid As Boolean
            Get
                Return _failures.Count = 0
            End Get
        End Property

        Public ReadOnly Property Failures As IList(Of ValidationFailure)
            Get
                Return _failures
            End Get
        End Property

        Public Sub AddFailure(field As String, message As String)
            _failures.Add(New ValidationFailure(field, message))
        End Sub

        ''' <summary>
        ''' Builds the text for a message box: a single failure shows its message
        ''' alone, multiple failures are listed one per line as "- Field: message".
        ''' </summary>
        Public Function ToDialogText() As String
            If _failures.Count = 0 Then
                Return ""
            End If

            If _failures.Count = 1 Then
                Return _failures(0).Message
            End If

            Dim builder As New StringBuilder()
            For i As Integer = 0 To _failures.Count - 1
                If i > 0 Then
                    builder.AppendLine()
                End If
                builder.Append("- ")
                builder.Append(_failures(i).Field)
                builder.Append(": ")
                builder.Append(_failures(i).Message)
            Next
            Return builder.ToString()
        End Function

    End Class


End Namespace
