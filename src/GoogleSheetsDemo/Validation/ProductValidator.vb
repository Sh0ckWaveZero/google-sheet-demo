Imports System
Imports System.Collections.Generic
Imports System.Globalization

Namespace Validation

    ''' <summary>
    ''' All input rules of the app in one place, kept free of any UI types so
    ''' the rules can be unit tested without Windows Forms. The form (append
    ''' fields) and the grid (rows to save) both go through this class; the
    ''' caller only decides how to display the returned failures.
    ''' </summary>
    Public Module ProductValidator

        Public Const NameMaxLength As Integer = 100
        Public Const QuantityMaxValue As Integer = 1000000000
        Public Const PriceMaxValue As Decimal = 99999999.99D

        ''' <summary>
        ''' Parses and validates the append form. Every broken rule is reported
        ''' at once; parsed values are only set for fields that passed.
        ''' </summary>
        Public Function ParseAppendInput(nameText As String, quantityText As String, priceText As String) As ProductInputResult
            Dim input As New ProductInputResult()

            Dim name As String = If(nameText, "").Trim()
            If name.Length = 0 Then
                input.Validation.AddFailure("Name", "Name is required.")
            ElseIf name.Length > NameMaxLength Then
                input.Validation.AddFailure("Name",
                    String.Format("Name must be {0} characters or fewer (currently {1}).", NameMaxLength, name.Length))
            Else
                input.Name = name
            End If

            Dim quantity As Integer
            If Not TryParseInt(quantityText, quantity) Then
                input.Validation.AddFailure("Quantity", "Quantity is required and must be a whole number.")
            ElseIf Not IsQuantityInRange(quantity) Then
                input.Validation.AddFailure("Quantity", RangeMessage("Quantity", QuantityMaxValue.ToString(CultureInfo.InvariantCulture)))
            Else
                input.Quantity = quantity
            End If

            Dim price As Decimal
            If Not TryParseDecimal(priceText, price) Then
                input.Validation.AddFailure("Price", "Price is required and must be a number.")
            ElseIf Not IsPriceInRange(price) Then
                input.Validation.AddFailure("Price", RangeMessage("Price", PriceMaxValue.ToString(CultureInfo.InvariantCulture)))
            Else
                input.Price = price
            End If

            Return input
        End Function

        ''' <summary>
        ''' Validates every row that is about to be written back to the sheet.
        ''' Failures carry the grid row number so the dialog can point at the
        ''' exact row to fix. A null/empty list is valid (it means "clear").
        ''' </summary>
        Public Function ValidateForSave(products As IEnumerable(Of Product)) As ValidationResult
            Dim result As New ValidationResult()
            If products Is Nothing Then
                Return result
            End If

            Dim rowNumber As Integer = 1
            For Each product As Product In products
                Dim row As String = "Row " + rowNumber.ToString(CultureInfo.InvariantCulture)

                If product Is Nothing Then
                    result.AddFailure(row, "Row is empty.")
                Else
                    If product.Id < 1 Then
                        result.AddFailure(row, "ID must be 1 or greater.")
                    End If

                    ValidateNameField(product.Name, row, result)

                    If Not IsQuantityInRange(product.Quantity) Then
                        result.AddFailure(row, RangeMessage("Quantity", QuantityMaxValue.ToString(CultureInfo.InvariantCulture)))
                    End If

                    If Not IsPriceInRange(product.Price) Then
                        result.AddFailure(row, RangeMessage("Price", PriceMaxValue.ToString(CultureInfo.InvariantCulture)))
                    End If
                End If

                rowNumber += 1
            Next
            Return result
        End Function

        Private Sub ValidateNameField(name As String, label As String, result As ValidationResult)
            Dim trimmed As String = If(name, "").Trim()
            If trimmed.Length = 0 Then
                result.AddFailure(label, "Name is required.")
            ElseIf trimmed.Length > NameMaxLength Then
                result.AddFailure(label,
                    String.Format("Name must be {0} characters or fewer (currently {1}).", NameMaxLength, trimmed.Length))
            End If
        End Sub

        Private Function RangeMessage(field As String, maxText As String) As String
            Return String.Format("{0} must be between 0 and {1}.", field, maxText)
        End Function

        Private Function IsQuantityInRange(quantity As Integer) As Boolean
            Return quantity >= 0 AndAlso quantity <= QuantityMaxValue
        End Function

        Private Function IsPriceInRange(price As Decimal) As Boolean
            Return price >= 0 AndAlso price <= PriceMaxValue
        End Function

        Private Function TryParseInt(text As String, ByRef value As Integer) As Boolean
            Dim trimmed As String = If(text, "").Trim()
            value = 0
            If trimmed.Length = 0 Then
                Return False
            End If
            If Integer.TryParse(trimmed, NumberStyles.Any, CultureInfo.InvariantCulture, value) Then
                Return True
            End If
            Return Integer.TryParse(trimmed, NumberStyles.Any, CultureInfo.CurrentCulture, value)
        End Function

        Private Function TryParseDecimal(text As String, ByRef value As Decimal) As Boolean
            Dim trimmed As String = If(text, "").Trim()
            value = 0D
            If trimmed.Length = 0 Then
                Return False
            End If
            If Decimal.TryParse(trimmed, NumberStyles.Any, CultureInfo.InvariantCulture, value) Then
                Return True
            End If
            Return Decimal.TryParse(trimmed, NumberStyles.Any, CultureInfo.CurrentCulture, value)
        End Function

    End Module


End Namespace
