Imports System.Collections.Generic
Imports System.Linq
Imports GoogleSheetsDemo.Models
Imports GoogleSheetsDemo.Services
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class SheetRowMapperTests

    <TestMethod>
    Public Sub ToProducts_WithNullRows_ReturnsEmptyList()
        Dim products As IList(Of Product) = SheetRowMapper.ToProducts(Nothing)

        Assert.IsNotNull(products)
        Assert.AreEqual(0, products.Count)
    End Sub

    <TestMethod>
    Public Sub ToProducts_WithValidRow_ParsesAllFields()
        Dim rows As IList(Of IList(Of Object)) = TestRows.Make(
            New Object() {"1", "Keyboard", "10", "199.5"})

        Dim products As IList(Of Product) = SheetRowMapper.ToProducts(rows)

        Assert.AreEqual(1, products.Count)
        Assert.AreEqual(1, products(0).Id)
        Assert.AreEqual("Keyboard", products(0).Name)
        Assert.AreEqual(10, products(0).Quantity)
        Assert.AreEqual(199.5D, products(0).Price)
    End Sub

    <TestMethod>
    Public Sub ToProducts_WithUnparseableId_SkipsRow()
        Dim rows As IList(Of IList(Of Object)) = TestRows.Make(
            New Object() {"abc", "Keyboard", "10", "199.5"})

        Assert.AreEqual(0, SheetRowMapper.ToProducts(rows).Count)
    End Sub

    <TestMethod>
    Public Sub ToProducts_WithBlankName_SkipsRow()
        Dim rows As IList(Of IList(Of Object)) = TestRows.Make(
            New Object() {"1", "   ", "10", "199.5"})

        Assert.AreEqual(0, SheetRowMapper.ToProducts(rows).Count)
    End Sub

    <TestMethod>
    Public Sub ToProducts_WithBlankRow_SkipsRow()
        Dim rows As IList(Of IList(Of Object)) = TestRows.Make(
            New Object() {Nothing, Nothing, Nothing, Nothing})

        Assert.AreEqual(0, SheetRowMapper.ToProducts(rows).Count)
    End Sub

    <TestMethod>
    Public Sub ToProducts_WithShortRow_TreatsMissingCellsAsZero()
        Dim rows As IList(Of IList(Of Object)) = TestRows.Make(
            New Object() {"7", "Mouse"})

        Dim products As IList(Of Product) = SheetRowMapper.ToProducts(rows)

        Assert.AreEqual(1, products.Count)
        Assert.AreEqual(7, products(0).Id)
        Assert.AreEqual("Mouse", products(0).Name)
        Assert.AreEqual(0, products(0).Quantity)
        Assert.AreEqual(0D, products(0).Price)
    End Sub

    <TestMethod>
    Public Sub ToProducts_WithNonNumericQuantity_DefaultsToZero()
        Dim rows As IList(Of IList(Of Object)) = TestRows.Make(
            New Object() {"7", "Mouse", "n/a", "5"})

        Dim product As Product = SheetRowMapper.ToProducts(rows)(0)

        Assert.AreEqual(0, product.Quantity)
        Assert.AreEqual(5D, product.Price)
    End Sub

    <TestMethod>
    Public Sub ToProducts_TrimsWhitespaceAroundName()
        Dim rows As IList(Of IList(Of Object)) = TestRows.Make(
            New Object() {"7", "  Mouse  ", "1", "1"})

        Assert.AreEqual("Mouse", SheetRowMapper.ToProducts(rows)(0).Name)
    End Sub

    <TestMethod>
    Public Sub ToRow_WritesCellsInInvariantCulture()
        Dim product = New Product With {.Id = 3, .Name = "Monitor", .Quantity = 4, .Price = 1234.5D}

        Dim row As IList(Of Object) = SheetRowMapper.ToRow(product)

        CollectionAssert.AreEqual(
            New String() {"3", "Monitor", "4", "1234.5"},
            row.ToArray())
    End Sub

    <TestMethod>
    Public Sub ToRow_ThenToProduct_RoundTripsAllFields()
        Dim product = New Product With {.Id = 9, .Name = "Webcam", .Quantity = 2, .Price = 88.8D}

        Dim restored As Product = SheetRowMapper.ToProduct(SheetRowMapper.ToRow(product))

        Assert.IsNotNull(restored)
        Assert.AreEqual(product.Id, restored.Id)
        Assert.AreEqual(product.Name, restored.Name)
        Assert.AreEqual(product.Quantity, restored.Quantity)
        Assert.AreEqual(product.Price, restored.Price)
    End Sub

End Class
