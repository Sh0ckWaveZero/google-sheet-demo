Imports System
Imports System.Collections.Generic

Namespace Services

    ''' <summary>
    ''' Page math for the product grid: how many pages a row count needs, how
    ''' to keep the current page inside range, and which rows belong to a page.
    ''' Pure logic, no UI types, so the rules are unit testable. The full list
    ''' always stays in the caller's hands - the pager only ever returns a view.
    ''' </summary>
    Public Module ProductPager

        ''' <summary>Number of pages needed; an empty list still counts as one (empty) page.</summary>
        Public Function TotalPages(totalRows As Integer, pageSize As Integer) As Integer
            If totalRows <= 0 OrElse pageSize <= 0 Then
                Return 1
            End If
            Return (totalRows + pageSize - 1) \ pageSize
        End Function

        ''' <summary>Keeps the page number inside 1..totalPages.</summary>
        Public Function ClampPage(page As Integer, totalPages As Integer) As Integer
            If page < 1 Then
                Return 1
            End If
            If page > totalPages Then
                Return totalPages
            End If
            Return page
        End Function

        ''' <summary>The rows of one page. A page past the end yields an empty list.</summary>
        Public Function Slice(products As IList(Of Product), page As Integer, pageSize As Integer) As IList(Of Product)
            Dim result As New List(Of Product)()
            If products Is Nothing OrElse pageSize <= 0 OrElse page < 1 Then
                Return result
            End If

            Dim firstIndex As Integer = (page - 1) * pageSize
            Dim lastIndex As Integer = Math.Min(firstIndex + pageSize, products.Count)
            For i As Integer = firstIndex To lastIndex - 1
                result.Add(products(i))
            Next
            Return result
        End Function

    End Module


End Namespace
