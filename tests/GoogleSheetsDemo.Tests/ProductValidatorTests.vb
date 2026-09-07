Imports System.Linq
Imports GoogleSheetsDemo.Models
Imports GoogleSheetsDemo.Validation
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class ProductValidatorTests

    ' --- ParseAppendInput: the append form ---

    <TestMethod>
    Public Sub ParseAppendInput_WithValidInput_ReturnsParsedValues()
        Dim input As ProductInputResult = ProductValidator.ParseAppendInput("Keyboard", "10", "199.5")

        Assert.IsTrue(input.IsValid)
        Assert.AreEqual(0, input.Validation.Failures.Count)
        Assert.AreEqual("Keyboard", input.Name)
        Assert.AreEqual(10, input.Quantity)
        Assert.AreEqual(199.5D, input.Price)
    End Sub

    <TestMethod>
    Public Sub ParseAppendInput_TrimsWhitespaceAroundName()
        Dim input As ProductInputResult = ProductValidator.ParseAppendInput("  Keyboard  ", "1", "1")

        Assert.IsTrue(input.IsValid)
        Assert.AreEqual("Keyboard", input.Name)
    End Sub

    <TestMethod>
    Public Sub ParseAppendInput_WithBlankName_FailsWithRequiredMessage()
        Dim input As ProductInputResult = ProductValidator.ParseAppendInput("   ", "1", "1")

        Assert.IsFalse(input.IsValid)
        Assert.AreEqual("Name", input.Validation.Failures.Single().Field)
        Assert.IsTrue(input.Validation.Failures.Single().Message.Contains("required"))
    End Sub

    <TestMethod>
    Public Sub ParseAppendInput_WithNameLongerThanMax_FailsWithMaxLengthMessage()
        Dim longName As String = New String("x"c, ProductValidator.NameMaxLength + 1)

        Dim input As ProductInputResult = ProductValidator.ParseAppendInput(longName, "1", "1")

        Assert.IsFalse(input.IsValid)
        Assert.AreEqual("Name", input.Validation.Failures.Single().Field)
        Assert.IsTrue(input.Validation.Failures.Single().Message.Contains(ProductValidator.NameMaxLength.ToString()))
    End Sub

    <TestMethod>
    Public Sub ParseAppendInput_WithEmptyQuantity_FailsAsRequired()
        Dim input As ProductInputResult = ProductValidator.ParseAppendInput("Keyboard", "", "1")

        Assert.IsFalse(input.IsValid)
        Assert.AreEqual("Quantity", input.Validation.Failures.Single().Field)
    End Sub

    <TestMethod>
    Public Sub ParseAppendInput_WithNonNumericQuantity_Fails()
        Dim input As ProductInputResult = ProductValidator.ParseAppendInput("Keyboard", "ten", "1")

        Assert.IsFalse(input.IsValid)
        Assert.AreEqual("Quantity", input.Validation.Failures.Single().Field)
    End Sub

    <TestMethod>
    Public Sub ParseAppendInput_WithNegativeQuantity_FailsWithRangeMessage()
        Dim input As ProductInputResult = ProductValidator.ParseAppendInput("Keyboard", "-3", "1")

        Assert.IsFalse(input.IsValid)
        Assert.AreEqual("Quantity", input.Validation.Failures.Single().Field)
        Assert.IsTrue(input.Validation.Failures.Single().Message.Contains("between"))
    End Sub

    <TestMethod>
    Public Sub ParseAppendInput_WithThousandsSeparator_ParsesNumber()
        Dim input As ProductInputResult = ProductValidator.ParseAppendInput("Keyboard", "1,500", "1")

        Assert.IsTrue(input.IsValid)
        Assert.AreEqual(1500, input.Quantity)
    End Sub

    <TestMethod>
    Public Sub ParseAppendInput_WithEmptyPrice_FailsAsRequired()
        Dim input As ProductInputResult = ProductValidator.ParseAppendInput("Keyboard", "1", "  ")

        Assert.IsFalse(input.IsValid)
        Assert.AreEqual("Price", input.Validation.Failures.Single().Field)
    End Sub

    <TestMethod>
    Public Sub ParseAppendInput_WithNegativePrice_FailsWithRangeMessage()
        Dim input As ProductInputResult = ProductValidator.ParseAppendInput("Keyboard", "1", "-0.5")

        Assert.IsFalse(input.IsValid)
        Assert.AreEqual("Price", input.Validation.Failures.Single().Field)
        Assert.IsTrue(input.Validation.Failures.Single().Message.Contains("between"))
    End Sub

    <TestMethod>
    Public Sub ParseAppendInput_WithEveryFieldBroken_ReportsAllFailuresAtOnce()
        Dim input As ProductInputResult = ProductValidator.ParseAppendInput("", "abc", "abc")

        Assert.IsFalse(input.IsValid)
        Assert.AreEqual(3, input.Validation.Failures.Count)
        CollectionAssert.AreEquivalent(
            New String() {"Name", "Quantity", "Price"},
            input.Validation.Failures.Select(Function(f) f.Field).ToArray())
    End Sub

    ' --- ValidateForSave: the grid before writing back ---

    <TestMethod>
    Public Sub ValidateForSave_WithValidRows_ReturnsValid()
        Dim products = New Product() _
        {
            New Product With {.Id = 1, .Name = "A", .Quantity = 1, .Price = 1D},
            New Product With {.Id = 2, .Name = "B", .Quantity = 0, .Price = 0D}
        }

        Dim result As ValidationResult = ProductValidator.ValidateForSave(products)

        Assert.IsTrue(result.IsValid)
    End Sub

    <TestMethod>
    Public Sub ValidateForSave_WithEmptyList_ReturnsValid()
        Dim result As ValidationResult = ProductValidator.ValidateForSave(New Product() {})

        Assert.IsTrue(result.IsValid)
    End Sub

    <TestMethod>
    Public Sub ValidateForSave_WithBlankName_ReportsGridRowNumber()
        Dim products = New Product() _
        {
            New Product With {.Id = 1, .Name = "A", .Quantity = 1, .Price = 1D},
            New Product With {.Id = 2, .Name = "   ", .Quantity = 1, .Price = 1D}
        }

        Dim result As ValidationResult = ProductValidator.ValidateForSave(products)

        Assert.IsFalse(result.IsValid)
        Dim failure As ValidationFailure = result.Failures.Single()
        Assert.AreEqual("Row 2", failure.Field)
        Assert.IsTrue(failure.Message.Contains("required"))
    End Sub

    <TestMethod>
    Public Sub ValidateForSave_WithInvalidId_ReportsGridRowNumber()
        Dim products = New Product() _
        {
            New Product With {.Id = 0, .Name = "A", .Quantity = 1, .Price = 1D}
        }

        Dim result As ValidationResult = ProductValidator.ValidateForSave(products)

        Assert.IsFalse(result.IsValid)
        Assert.AreEqual("Row 1", result.Failures.Single().Field)
        Assert.IsTrue(result.Failures.Single().Message.Contains("ID"))
    End Sub

    <TestMethod>
    Public Sub ValidateForSave_WithOutOfRangePrice_ReportsRange()
        Dim products = New Product() _
        {
            New Product With {.Id = 1, .Name = "A", .Quantity = 1, .Price = ProductValidator.PriceMaxValue + 1D}
        }

        Dim result As ValidationResult = ProductValidator.ValidateForSave(products)

        Assert.IsFalse(result.IsValid)
        Assert.AreEqual("Row 1", result.Failures.Single().Field)
        Assert.IsTrue(result.Failures.Single().Message.Contains("between"))
    End Sub

    <TestMethod>
    Public Sub ValidateForSave_WithSeveralBrokenRows_ListsEachRow()
        Dim products = New Product() _
        {
            New Product With {.Id = 0, .Name = "", .Quantity = -1, .Price = -1D},
            New Product With {.Id = 5, .Name = "OK", .Quantity = 1, .Price = 1D},
            New Product With {.Id = 6, .Name = "  ", .Quantity = 1, .Price = 1D}
        }

        Dim result As ValidationResult = ProductValidator.ValidateForSave(products)

        Assert.IsFalse(result.IsValid)
        Assert.AreEqual(5, result.Failures.Count)
        CollectionAssert.AreEquivalent(
            New String() {"Row 1", "Row 1", "Row 1", "Row 1", "Row 3"},
            result.Failures.Select(Function(f) f.Field).ToArray())
    End Sub

End Class
