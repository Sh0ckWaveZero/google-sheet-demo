Imports System.Collections.Generic
Imports System.Linq
Imports System.Threading.Tasks
Imports GoogleSheetsDemo.Models
Imports GoogleSheetsDemo.Services
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class ProductRepositoryTests

    Private Const SheetName As String = "Sheet1"
    Private Shared ReadOnly Header As String() = {"ID", "Name", "Quantity", "Price"}

    <TestMethod>
    Public Async Function GetAllAsync_ReadsDataRangeAndMapsRows() As Task
        Dim fake = New FakeSheetService(TestRows.Make(
            New Object() {"1", "Keyboard", "10", "199.5"},
            New Object() {"2", "Mouse", "25", "8.9"}))
        Dim repository = New ProductRepository(fake, SheetName)

        Dim products As IList(Of Product) = Await repository.GetAllAsync()

        Assert.AreEqual(SheetName & "!A2:D", fake.GetRanges.Single())
        Assert.AreEqual(2, products.Count)
        Assert.AreEqual("Keyboard", products(0).Name)
        Assert.AreEqual(8.9D, products(1).Price)
    End Function

    <TestMethod>
    Public Async Function GetAllAsync_WhenSheetHasNoValues_ReturnsEmptyList() As Task
        Dim fake = New FakeSheetService(Nothing)
        Dim repository = New ProductRepository(fake, SheetName)

        Dim products As IList(Of Product) = Await repository.GetAllAsync()

        Assert.AreEqual(0, products.Count)
    End Function

    <TestMethod>
    Public Async Function AddAsync_WithExistingRows_AppendsRowWithNextId() As Task
        Dim fake = New FakeSheetService(TestRows.Make(
            New Object() {"3", "Existing", "1", "1"}))
        Dim repository = New ProductRepository(fake, SheetName)

        Dim newId As Integer = Await repository.AddAsync(New Product With {.Name = "New item", .Quantity = 2, .Price = 3.5D})

        Assert.AreEqual(4, newId)
        Assert.AreEqual(SheetName & "!A:D", fake.AppendRanges.Single())

        Dim appendedRow As IList(Of Object) = fake.AppendedValues.Single().Single()
        CollectionAssert.AreEqual(New String() {"4", "New item", "2", "3.5"}, appendedRow.ToArray())
    End Function

    <TestMethod>
    Public Async Function AddAsync_OnEmptySheet_WritesHeaderThenFirstRow() As Task
        Dim fake = New FakeSheetService(Nothing)
        Dim repository = New ProductRepository(fake, SheetName)

        Dim newId As Integer = Await repository.AddAsync(New Product With {.Name = "First", .Quantity = 1, .Price = 1})

        Assert.AreEqual(1, newId)
        Assert.AreEqual(SheetName & "!A1", fake.UpdateRanges.Single())
        CollectionAssert.AreEqual(Header, fake.UpdatedValues.Single().Single().ToArray())
        Assert.AreEqual(1, fake.AppendedValues.Single().Count)
    End Function

    <TestMethod>
    Public Async Function SaveAllAsync_WithProducts_UpdatesDataRange() As Task
        Dim fake = New FakeSheetService()
        Dim repository = New ProductRepository(fake, SheetName)

        Await repository.SaveAllAsync(New Product() _
        {
            New Product With {.Id = 1, .Name = "A", .Quantity = 1, .Price = 1D},
            New Product With {.Id = 2, .Name = "B", .Quantity = 2, .Price = 2.25D}
        })

        Assert.AreEqual(SheetName & "!A2:D", fake.UpdateRanges.Single())

        Dim savedRows As IList(Of IList(Of Object)) = fake.UpdatedValues.Single()
        Assert.AreEqual(2, savedRows.Count)
        Assert.AreEqual("2.25", savedRows(1)(3))
    End Function

    <TestMethod>
    Public Async Function SaveAllAsync_AfterDeletingRows_ClearsBeforeRewriting() As Task
        Dim fake = New FakeSheetService()
        Dim repository = New ProductRepository(fake, SheetName)

        ' deleting shrinks the list, but the sheet can still hold more
        ' rows than the rewrite supplies - without the clear the last
        ' old row survives and comes back on the next load
        Await repository.SaveAllAsync(New Product() _
        {
            New Product With {.Id = 1, .Name = "A", .Quantity = 1, .Price = 1D}
        })

        CollectionAssert.AreEqual(New String() {"Clear", "Update"}, fake.CallLog)
        Assert.AreEqual(SheetName & "!A2:D", fake.ClearedRanges.Single())
        Assert.AreEqual(SheetName & "!A2:D", fake.UpdateRanges.Single())
    End Function

    <TestMethod>
    Public Async Function SaveAllAsync_WithNoProducts_ClearsDataRange() As Task
        Dim fake = New FakeSheetService()
        Dim repository = New ProductRepository(fake, SheetName)

        Await repository.SaveAllAsync(New Product() {})

        Assert.AreEqual(SheetName & "!A2:D", fake.ClearedRanges.Single())
        Assert.AreEqual(0, fake.UpdateRanges.Count)
        CollectionAssert.AreEqual(New String() {"Clear"}, fake.CallLog)
    End Function

End Class
