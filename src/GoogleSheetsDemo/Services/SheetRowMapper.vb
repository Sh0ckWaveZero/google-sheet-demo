Imports System
Imports System.Collections.Generic
Imports System.Globalization

Namespace Services

    ''' <summary>
    ''' Converts between raw sheet rows (IList of IList of object) and Product
    ''' objects. Rows are skipped when the ID cannot be parsed or the name is
    ''' blank; missing cells are treated as empty/zero.
    ''' </summary>
    Public Module SheetRowMapper

        Public Function ToProducts(rows As IList(Of IList(Of Object))) As IList(Of Product)
            Dim products As New List(Of Product)()
            If rows Is Nothing Then
                Return products
            End If

            For Each row As IList(Of Object) In rows
                Dim product As Product = ToProduct(row)
                If product IsNot Nothing Then
                    products.Add(product)
                End If
            Next
            Return products
        End Function

        ''' <summary>Maps a single sheet row, or returns null when the row must be skipped.</summary>
        Public Function ToProduct(row As IList(Of Object)) As Product
            If row Is Nothing OrElse IsEmpty(row) Then
                Return Nothing
            End If

            Dim cells As IList(Of Object) = PadRow(row, 4)

            Dim id As Integer
            If Not TryParseInt(cells(0), id) Then
                Return Nothing
            End If

            Dim name As String = If(cells(1), "").ToString().Trim()
            If name.Length = 0 Then
                Return Nothing
            End If

            Dim quantity As Integer
            TryParseInt(cells(2), quantity) ' defaults to 0 when not a number

            Dim price As Decimal
            TryParseDecimal(cells(3), price) ' defaults to 0 when not a number

            Return New Product With {.Id = id, .Name = name, .Quantity = quantity, .Price = price}
        End Function

        ''' <summary>Builds the sheet row for a product (invariant culture so the sheet always sees "1234.5").</summary>
        Public Function ToRow(product As Product) As IList(Of Object)
            If product Is Nothing Then
                Throw New ArgumentNullException("product")
            End If

            Return New List(Of Object) From {
                product.Id.ToString(CultureInfo.InvariantCulture),
                If(product.Name, ""),
                product.Quantity.ToString(CultureInfo.InvariantCulture),
                product.Price.ToString(CultureInfo.InvariantCulture)
            }
        End Function

        Private Function IsEmpty(row As IList(Of Object)) As Boolean
            For Each cell As Object In row
                If cell IsNot Nothing AndAlso cell.ToString().Trim().Length > 0 Then
                    Return False
                End If
            Next
            Return True
        End Function

        Private Function PadRow(row As IList(Of Object), minimumCells As Integer) As IList(Of Object)
            Dim cells As New List(Of Object)(row)
            Do While cells.Count < minimumCells
                cells.Add(Nothing)
            Loop
            Return cells
        End Function

        Private Function TryParseInt(cell As Object, ByRef value As Integer) As Boolean
            Dim text As String = If(cell, "").ToString().Trim()
            value = 0
            If text.Length = 0 Then
                Return False
            End If
            If Integer.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, value) Then
                Return True
            End If
            Return Integer.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, value)
        End Function

        Private Function TryParseDecimal(cell As Object, ByRef value As Decimal) As Boolean
            Dim text As String = If(cell, "").ToString().Trim()
            value = 0D
            If text.Length = 0 Then
                Return False
            End If
            If Decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, value) Then
                Return True
            End If
            Return Decimal.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, value)
        End Function

    End Module


End Namespace
