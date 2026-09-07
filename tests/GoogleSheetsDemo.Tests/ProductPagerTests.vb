Imports System.Collections.Generic
Imports GoogleSheetsDemo.Models
Imports GoogleSheetsDemo.Services
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class ProductPagerTests

    Private Shared Function MakeRows(count As Integer) As IList(Of Product)
        Dim rows As New List(Of Product)()
        For i As Integer = 1 To count
            rows.Add(New Product With {.Id = i, .Name = "Row " & i, .Quantity = i, .Price = i})
        Next
        Return rows
    End Function

    ' --- TotalPages ---

    <TestMethod>
    Public Sub TotalPages_WithZeroRows_ReturnsOne()
        Assert.AreEqual(1, ProductPager.TotalPages(0, 10))
    End Sub

    <TestMethod>
    Public Sub TotalPages_WithExactDivision_ReturnsExactPages()
        Assert.AreEqual(2, ProductPager.TotalPages(20, 10))
    End Sub

    <TestMethod>
    Public Sub TotalPages_WithRemainder_RoundsUp()
        Assert.AreEqual(3, ProductPager.TotalPages(25, 10))
    End Sub

    <TestMethod>
    Public Sub TotalPages_WithSingleRow_ReturnsOne()
        Assert.AreEqual(1, ProductPager.TotalPages(1, 10))
    End Sub

    <TestMethod>
    Public Sub TotalPages_WithInvalidPageSize_ReturnsOne()
        Assert.AreEqual(1, ProductPager.TotalPages(25, 0))
        Assert.AreEqual(1, ProductPager.TotalPages(25, -5))
    End Sub

    ' --- ClampPage ---

    <TestMethod>
    Public Sub ClampPage_BelowOne_ReturnsFirst()
        Assert.AreEqual(1, ProductPager.ClampPage(0, 3))
        Assert.AreEqual(1, ProductPager.ClampPage(-5, 3))
    End Sub

    <TestMethod>
    Public Sub ClampPage_AboveRange_ReturnsLastPage()
        Assert.AreEqual(3, ProductPager.ClampPage(99, 3))
    End Sub

    <TestMethod>
    Public Sub ClampPage_InsideRange_StaysUnchanged()
        Assert.AreEqual(2, ProductPager.ClampPage(2, 3))
    End Sub

    ' --- Slice ---

    <TestMethod>
    Public Sub Slice_FirstPage_ReturnsFirstRows()
        Dim slice As IList(Of Product) = ProductPager.Slice(MakeRows(25), 1, 10)

        Assert.AreEqual(10, slice.Count)
        Assert.AreEqual(1, slice(0).Id)
        Assert.AreEqual(10, slice(9).Id)
    End Sub

    <TestMethod>
    Public Sub Slice_MiddlePage_ReturnsCorrectWindow()
        Dim slice As IList(Of Product) = ProductPager.Slice(MakeRows(25), 2, 10)

        Assert.AreEqual(10, slice.Count)
        Assert.AreEqual(11, slice(0).Id)
        Assert.AreEqual(20, slice(9).Id)
    End Sub

    <TestMethod>
    Public Sub Slice_LastPage_ReturnsPartialRows()
        Dim slice As IList(Of Product) = ProductPager.Slice(MakeRows(25), 3, 10)

        Assert.AreEqual(5, slice.Count)
        Assert.AreEqual(21, slice(0).Id)
        Assert.AreEqual(25, slice(4).Id)
    End Sub

    <TestMethod>
    Public Sub Slice_PageBeyondRange_ReturnsEmpty()
        Assert.AreEqual(0, ProductPager.Slice(MakeRows(5), 9, 10).Count)
    End Sub

    <TestMethod>
    Public Sub Slice_WithNullList_ReturnsEmpty()
        Assert.AreEqual(0, ProductPager.Slice(Nothing, 1, 10).Count)
    End Sub

    <TestMethod>
    Public Sub Slice_WithInvalidPageSize_ReturnsEmpty()
        Assert.AreEqual(0, ProductPager.Slice(MakeRows(5), 1, 0).Count)
    End Sub

End Class
