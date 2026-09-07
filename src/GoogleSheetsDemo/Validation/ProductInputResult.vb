
Namespace Validation

    ''' <summary>
    ''' Result of parsing and validating the append form: the failures to show
    ''' (if any) plus the parsed values, which are only meaningful when valid.
    ''' </summary>
    Public Class ProductInputResult

        Public Sub New()
            Me.Validation = New ValidationResult()
            Me.Name = ""
        End Sub

        Public ReadOnly Property Validation As ValidationResult

        Public ReadOnly Property IsValid As Boolean
            Get
                Return Validation.IsValid
            End Get
        End Property

        Public Property Name As String
        Public Property Quantity As Integer
        Public Property Price As Decimal

    End Class


End Namespace
