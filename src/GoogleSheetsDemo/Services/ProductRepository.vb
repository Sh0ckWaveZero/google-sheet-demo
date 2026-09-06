Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Threading.Tasks

Namespace Services

    ''' <summary>
    ''' Business logic on top of ISheetService: loads products, appends new
    ''' rows with an auto-incremented ID, and rewrites the whole data block.
    ''' This class is fully unit tested with a fake ISheetService.
    ''' </summary>
    Public Class ProductRepository

        Public Shared ReadOnly HeaderCells As String() = {"ID", "Name", "Quantity", "Price"}

        Private ReadOnly _sheets As ISheetService
        Private ReadOnly _sheetName As String

        Public Sub New(sheets As ISheetService, sheetName As String)
            If sheets Is Nothing Then
                Throw New ArgumentNullException("sheets")
            End If
            _sheets = sheets
            _sheetName = If(String.IsNullOrWhiteSpace(sheetName), "Sheet1", sheetName)
        End Sub

        ''' <summary>Reads every product below the header row (A2:D).</summary>
        Public Async Function GetAllAsync() As Task(Of IList(Of Product))
            Dim rows As IList(Of IList(Of Object)) =
                Await _sheets.GetValuesAsync(Range("A2:D")).ConfigureAwait(False)
            Return SheetRowMapper.ToProducts(rows)
        End Function

        ''' <summary>
        ''' Appends a new row at the end of the sheet. The ID is assigned here
        ''' (max existing ID + 1). When the sheet is still empty the header row
        ''' is written first.
        ''' </summary>
        Public Async Function AddAsync(product As Product) As Task(Of Integer)
            If product Is Nothing Then
                Throw New ArgumentNullException("product")
            End If

            Dim existing As IList(Of Product) = Await GetAllAsync().ConfigureAwait(False)
            Dim nextId As Integer = If(existing.Count = 0, 1, existing.Max(Function(p) p.Id) + 1)
            product.Id = nextId

            If existing.Count = 0 Then
                Dim header As New List(Of IList(Of Object)) From {New List(Of Object)(HeaderCells)}
                Await _sheets.UpdateValuesAsync(Range("A1"), header).ConfigureAwait(False)
            End If

            Dim rows As New List(Of IList(Of Object)) From {SheetRowMapper.ToRow(product)}
            Await _sheets.AppendValuesAsync(Range("A:D"), rows).ConfigureAwait(False)
            Return nextId
        End Function

        ''' <summary>
        ''' Rewrites the whole data block (A2:D) with the given products.
        ''' Passing an empty list leaves the data block cleared.
        ''' The block is always cleared first: values.update only overwrites
        ''' as many rows as it receives, so deleting a row without the clear
        ''' would leave the last old row in the sheet and it would come back
        ''' on the next load.
        ''' </summary>
        Public Async Function SaveAllAsync(products As IList(Of Product)) As Task
            Await _sheets.ClearValuesAsync(Range("A2:D")).ConfigureAwait(False)

            If products Is Nothing OrElse products.Count = 0 Then
                Return
            End If

            Dim rows As New List(Of IList(Of Object))(products.Count)
            For Each product As Product In products
                rows.Add(SheetRowMapper.ToRow(product))
            Next
            Await _sheets.UpdateValuesAsync(Range("A2:D"), rows).ConfigureAwait(False)
        End Function

        Private Function Range(localRange As String) As String
            Return _sheetName + "!" + localRange
        End Function

    End Class


End Namespace
