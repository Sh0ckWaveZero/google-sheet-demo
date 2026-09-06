Imports System.Collections.Generic
Imports System.Linq
Imports GoogleSheetsDemo.Models
Imports GoogleSheetsDemo.Services
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class ProductFilterTests

    Private Shared Function Make(id As Integer, name As String) As Product
        Return New Product With {.Id = id, .Name = name, .Quantity = 1, .Price = 1D}
    End Function

    <TestMethod>
    Public Sub Matches_WithEmptyQuery_ReturnsTrue()
        Assert.IsTrue(ProductFilter.Matches(Make(1, "Keyboard"), ""))
    End Sub

    <TestMethod>
    Public Sub Matches_WithWhitespaceQuery_ReturnsTrue()
        Assert.IsTrue(ProductFilter.Matches(Make(1, "Keyboard"), "   "))
    End Sub

    <TestMethod>
    Public Sub Matches_ByName_IsCaseInsensitive()
        Assert.IsTrue(ProductFilter.Matches(Make(1, "Keyboard"), "KEY"))
        Assert.IsTrue(ProductFilter.Matches(Make(1, "Keyboard"), "board"))
    End Sub

    <TestMethod>
    Public Sub Matches_ByNameSubstring_NotFound_ReturnsFalse()
        Assert.IsFalse(ProductFilter.Matches(Make(1, "Keyboard"), "mouse"))
    End Sub

    <TestMethod>
    Public Sub Matches_ByIdExact_ReturnsTrue()
        Assert.IsTrue(ProductFilter.Matches(Make(7, "Keyboard"), "7"))
    End Sub

    <TestMethod>
    Public Sub Matches_ByIdPartialDigit_DoesNotMatchOtherIds()
        Assert.IsFalse(ProductFilter.Matches(Make(20, "Keyboard"), "2"))
    End Sub

    <TestMethod>
    Public Sub Matches_TrimsQueryBeforeComparing()
        Assert.IsTrue(ProductFilter.Matches(Make(7, "Keyboard"), "  7  "))
        Assert.IsTrue(ProductFilter.Matches(Make(7, "Keyboard"), " key "))
    End Sub

    <TestMethod>
    Public Sub Matches_WithNullProduct_ReturnsFalse()
        Assert.IsFalse(ProductFilter.Matches(Nothing, "keyboard"))
    End Sub

    <TestMethod>
    Public Sub Apply_KeepsOnlyMatches_PreservesOrder()
        Dim products = New Product() _
        {
            Make(1, "Keyboard"),
            Make(2, "Mouse"),
            Make(12, "Webcam"),
            Make(3, "keyboard tray")
        }

        Dim result As IList(Of Product) = ProductFilter.Apply(products, "key")

        CollectionAssert.AreEquivalent(New Integer() {1, 3}, result.Select(Function(p) p.Id).ToArray())
        Assert.AreEqual(1, result(0).Id) ' original order preserved
        Assert.AreEqual(3, result(1).Id)
    End Sub

    <TestMethod>
    Public Sub Apply_WithBlankQuery_ReturnsEverything()
        Dim products = New Product() {Make(1, "A"), Make(2, "B")}

        Assert.AreEqual(2, ProductFilter.Apply(products, "").Count)
    End Sub

    <TestMethod>
    Public Sub Apply_WithNullList_ReturnsEmptyList()
        Assert.AreEqual(0, ProductFilter.Apply(Nothing, "x").Count)
    End Sub

End Class
