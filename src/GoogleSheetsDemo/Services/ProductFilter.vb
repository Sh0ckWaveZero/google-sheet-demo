Imports System
Imports System.Collections.Generic
Imports System.Globalization

Namespace Services

    ''' <summary>
    ''' Search rules for the product grid: a query matches a product when it
    ''' equals the product's ID or appears inside its name (case-insensitive).
    ''' Pure logic, no UI types, so the rules are unit testable.
    ''' </summary>
    Public Module ProductFilter

        ''' <summary>True when the product should be shown for the given query.</summary>
        Public Function Matches(product As Product, query As String) As Boolean
            If product Is Nothing Then
                Return False
            End If

            Dim term As String = If(query, "").Trim()
            If term.Length = 0 Then
                Return True
            End If

            Dim id As Integer
            If Integer.TryParse(term, NumberStyles.Integer, CultureInfo.InvariantCulture, id) AndAlso product.Id = id Then
                Return True
            End If

            Return If(product.Name, "").IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0
        End Function

        ''' <summary>Filters the list preserving its order. Empty/whitespace query returns everything.</summary>
        Public Function Apply(products As IEnumerable(Of Product), query As String) As IList(Of Product)
            Dim result As New List(Of Product)()
            If products Is Nothing Then
                Return result
            End If

            For Each product As Product In products
                If Matches(product, query) Then
                    result.Add(product)
                End If
            Next
            Return result
        End Function

    End Module


End Namespace
