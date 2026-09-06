
Namespace Validation

    ''' <summary>
    ''' A single input rule violation: which field failed and why.
    ''' Pure data - the UI decides how to present it.
    ''' </summary>
    Public Class ValidationFailure

        Public Sub New(field As String, message As String)
            Me.Field = field
            Me.Message = message
        End Sub

        ''' <summary>Field or row label the failure belongs to (e.g. "Name", "Row 3").</summary>
        Public ReadOnly Property Field As String

        ''' <summary>Human readable description of the rule that was broken.</summary>
        Public ReadOnly Property Message As String

    End Class


End Namespace
