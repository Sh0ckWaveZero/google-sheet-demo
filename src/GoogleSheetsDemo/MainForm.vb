Imports System
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Configuration
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Runtime.InteropServices
Imports System.Windows.Forms
Imports GoogleSheetsDemo.Controls
Imports GoogleSheetsDemo.Models
Imports GoogleSheetsDemo.Services
Imports GoogleSheetsDemo.Theme
Imports GoogleSheetsDemo.Validation

''' <summary>
''' Main window: loads the sheet into a grid, lets the user search, edit
''' cells or the selected row, delete rows, append new ones and write
''' changes back to Google Sheets.
''' All input rules live in ProductValidator and search rules in
''' ProductFilter (no UI types, unit tested); this class only decides how
''' results are presented: status bar plus a modal dialog with an icon for
''' validation problems and runtime errors.
''' </summary>
Partial Public Class MainForm
    Inherits Form

    Private ReadOnly _products As New BindingList(Of Product)()
    Private _spinner As LoadingSpinner
    Private _editingProduct As Product
    Private _editCellIcon As Bitmap
    Private _deleteCellIcon As Bitmap
    Private _busy As Boolean
    Private _loaded As Boolean ' true once the sheet has been loaded at least once
    Private _page As Integer = 1
    Private _pageSize As Integer = 10
    Private _totalPages As Integer = 1
    Private _viewReady As Boolean ' false while InitializeComponent is still running
    Private _palette As ThemePalette = AppTheme.Light
    Private _lastStatus As String = "Ready."
    Private _lastStatusIsError As Boolean
    Private _themeOverride As Boolean? ' Nothing = follow the Windows theme
    Private _isDark As Boolean

    Public Sub New()
        InitializeComponent()

        txtSpreadsheetId.Text = If(ConfigurationManager.AppSettings("SpreadsheetId"), "")
        dataGridViewProducts.DataSource = _products

        Try
            ' the exe embeds App.ico through the ApplicationIcon project property
            Me.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath)
        Catch
            ' embedded icon unavailable - keep the default form icon
        End Try

        ApplyModernStyle()
        UpdateButtonStates()
        _viewReady = True

        ' the edit form is bottom-anchored: whenever the window resizes it
        ' moves, and the pager strip + grid must follow without overlapping
        AddHandler grpNewProduct.LocationChanged, Sub() RelayoutLowerArea()
        AddHandler grpNewProduct.Resize, Sub() RelayoutLowerArea()
        RelayoutLowerArea()
    End Sub

    Private ReadOnly Property SheetName As String
        Get
            Return If(ConfigurationManager.AppSettings("SheetName"), "Sheet1")
        End Get
    End Property

    Private Async Sub btnLoad_Click(sender As Object, e As EventArgs) Handles btnLoad.Click
        ExitEditMode()
        SetBusy(True, "Loading data from Google Sheets...")
        Try
            Using service As GoogleSheetService = CreateSheetService()
                Dim repository As New ProductRepository(service, SheetName)
                Dim products As IList(Of Product) = Await repository.GetAllAsync()
                ReplaceGridItems(products)
                _page = 1
                ApplyFilter()
                _loaded = True
                SetBusy(False, String.Format("Loaded {0} row(s).", products.Count))
            End Using
        Catch ex As Exception
            SetBusy(False, Nothing)
            ShowErrorDialog("Load", ex)
        End Try
    End Sub

    Private Async Sub btnAppend_Click(sender As Object, e As EventArgs) Handles btnAppend.Click
        If _editingProduct IsNot Nothing Then
            Await UpdateEditingRowAsync()
            Return
        End If

        Dim input As ProductInputResult = ProductValidator.ParseAppendInput(
            txtName.Text, txtQuantity.Text, txtPrice.Text)

        If Not input.IsValid Then
            ShowValidationDialog(input.Validation, "Cannot append row")
            SetStatus("Append cancelled: fix the fields shown in the dialog.", True)
            Return
        End If

        SetBusy(True, "Appending new row...")
        Try
            Dim product As New Product With {
                .Name = input.Name,
                .Quantity = input.Quantity,
                .Price = input.Price
            }

            Using service As GoogleSheetService = CreateSheetService()
                Dim repository As New ProductRepository(service, SheetName)
                Dim newId As Integer = Await repository.AddAsync(product)

                ClearInputFields()
                ReplaceGridItems(Await repository.GetAllAsync())
                _page = Integer.MaxValue ' clamp to the last page so the new row is visible
                ApplyFilter()
                SetBusy(False, String.Format("Appended row with ID {0}.", newId))
            End Using
        Catch ex As Exception
            SetBusy(False, Nothing)
            ShowErrorDialog("Append", ex)
        End Try
    End Sub

    Private Async Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        Dim products As IList(Of Product) = _products.ToList()

        Dim validation As ValidationResult = ProductValidator.ValidateForSave(products)
        If Not validation.IsValid Then
            ShowValidationDialog(validation, "Cannot save changes")
            SetStatus("Save cancelled: fix the rows shown in the dialog.", True)
            Return
        End If

        SetBusy(True, "Saving changes...")
        Try
            Using service As GoogleSheetService = CreateSheetService()
                Dim repository As New ProductRepository(service, SheetName)
                Await repository.SaveAllAsync(products)
                SetBusy(False, String.Format("Saved {0} row(s).", products.Count))
            End Using
        Catch ex As Exception
            SetBusy(False, Nothing)
            ShowErrorDialog("Save", ex)
        End Try
    End Sub

    ' --- edit row (double click or the per-row pencil icon) ---

    Private Sub dataGridViewProducts_CellDoubleClick(sender As Object, e As DataGridViewCellEventArgs) Handles dataGridViewProducts.CellDoubleClick
        ' the grid is read-only; double click opens the edit form instead
        If e.RowIndex < 0 Then
            Return
        End If
        Dim selected As Product = SelectedProduct()
        If selected IsNot Nothing Then
            EnterEditMode(selected)
        End If
    End Sub

    ''' <summary>
    ''' Row-level actions: the Edit / Delete icons at the end of every row.
    ''' Works from the filtered view too - the row holds the same product
    ''' reference as the master list.
    ''' </summary>
    Private Async Sub dataGridViewProducts_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles dataGridViewProducts.CellContentClick
        If e.RowIndex < 0 OrElse _busy Then
            Return
        End If

        Dim columnName As String = dataGridViewProducts.Columns(e.ColumnIndex).Name
        Dim product As Product = TryCast(dataGridViewProducts.Rows(e.RowIndex).DataBoundItem, Product)
        If product Is Nothing Then
            Return
        End If

        If columnName = "colEdit" Then
            EnterEditMode(product)
        ElseIf columnName = "colDelete" Then
            Await DeleteProductAsync(product)
        End If
    End Sub

    ''' <summary>Draws the pencil / trash icons inside the row action cells.</summary>
    Private Sub dataGridViewProducts_CellPainting(sender As Object, e As DataGridViewCellPaintingEventArgs) Handles dataGridViewProducts.CellPainting
        If e.RowIndex < 0 OrElse _editCellIcon Is Nothing OrElse _deleteCellIcon Is Nothing Then
            Return
        End If

        Dim columnName As String = dataGridViewProducts.Columns(e.ColumnIndex).Name
        Dim icon As Bitmap = If(columnName = "colEdit", _editCellIcon,
            If(columnName = "colDelete", _deleteCellIcon, CType(Nothing, Bitmap)))
        If icon Is Nothing Then
            Return
        End If

        e.Handled = True
        e.PaintBackground(e.ClipBounds, True)

        Dim size As Integer = Math.Min(18, Math.Min(e.CellBounds.Width, e.CellBounds.Height) - 8)
        If size < 4 Then
            Return
        End If
        Dim x As Integer = e.CellBounds.Left + (e.CellBounds.Width - size) \ 2
        Dim y As Integer = e.CellBounds.Top + (e.CellBounds.Height - size) \ 2
        e.Graphics.DrawImage(icon, x, y, size, size)
    End Sub

    Private Sub dataGridViewProducts_CellMouseEnter(sender As Object, e As DataGridViewCellEventArgs) Handles dataGridViewProducts.CellMouseEnter
        If e.RowIndex >= 0 Then
            Dim columnName As String = dataGridViewProducts.Columns(e.ColumnIndex).Name
            If columnName = "colEdit" OrElse columnName = "colDelete" Then
                dataGridViewProducts.Cursor = Cursors.Hand
            End If
        End If
    End Sub

    Private Sub dataGridViewProducts_CellMouseLeave(sender As Object, e As DataGridViewCellEventArgs) Handles dataGridViewProducts.CellMouseLeave
        dataGridViewProducts.Cursor = Me.DefaultCursor
    End Sub

    ''' <summary>Applies the form to the row being edited, then persists the whole block.</summary>
    Private Async Function UpdateEditingRowAsync() As Threading.Tasks.Task
        Dim input As ProductInputResult = ProductValidator.ParseAppendInput(
            txtName.Text, txtQuantity.Text, txtPrice.Text)

        If Not input.IsValid Then
            ShowValidationDialog(input.Validation, "Cannot update row")
            SetStatus("Update cancelled: fix the fields shown in the dialog.", True)
            Return
        End If

        SetBusy(True, "Updating row...")
        Try
            _editingProduct.Name = input.Name
            _editingProduct.Quantity = input.Quantity
            _editingProduct.Price = input.Price
            Dim id As Integer = _editingProduct.Id

            Dim saved As Integer = Await SaveAllProductsAsync("Update")
            If saved < 0 Then
                SetBusy(False, Nothing)
                Return ' validation dialog was shown
            End If
            RefreshGridAfterMutation()
            ExitEditMode()
            SetBusy(False, String.Format("Updated ID {0} and saved {1} row(s).", id, saved))
        Catch ex As Exception
            SetBusy(False, Nothing)
            ShowErrorDialog("Update", ex)
        End Try
    End Function

    Private Sub EnterEditMode(product As Product)
        _editingProduct = product
        txtName.Text = product.Name
        txtQuantity.Text = product.Quantity.ToString(CultureInfo.InvariantCulture)
        txtPrice.Text = product.Price.ToString(CultureInfo.InvariantCulture)

        grpNewProduct.Text = "Edit product (ID " & product.Id & ")"
        btnAppend.Text = "Update Row"
        SetButtonIcon(btnAppend, ButtonIcons.Create(ButtonIconKind.Pencil, _palette.AppendFore))
        btnCancel.Visible = True
        txtName.Focus()
        SetStatus("Editing ID " & product.Id & " - change the values and press Update Row.")
    End Sub

    Private Sub ExitEditMode()
        _editingProduct = Nothing
        ClearInputFields()
        grpNewProduct.Text = "Add new product (appends a row to the sheet)"
        btnAppend.Text = "Append Row"
        SetButtonIcon(btnAppend, ButtonIcons.Create(ButtonIconKind.Plus, _palette.AppendFore))
        btnCancel.Visible = False
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        ExitEditMode()
        SetStatus("Edit cancelled.")
    End Sub

    ' --- delete row (the per-row trash icon) ---

    ''' <summary>Confirm dialog, then removes the row and writes the change to the sheet.</summary>
    Private Async Function DeleteProductAsync(selected As Product) As Threading.Tasks.Task
        Dim answer As DialogResult = MessageBox.Show(Me,
            "Delete """ & selected.Name & """ (ID " & selected.Id & ")?" & vbLf & vbLf &
            "The row is removed from the grid and the change is written to Google Sheets immediately.",
            "Confirm delete",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning)

        If answer <> DialogResult.Yes Then
            SetStatus("Delete cancelled.")
            Return
        End If

        SetBusy(True, "Deleting row...")
        Try
            If _editingProduct Is selected Then
                ExitEditMode()
            End If

            _products.Remove(selected)
            Dim saved As Integer = Await SaveAllProductsAsync("Delete")
            If saved < 0 Then
                SetBusy(False, Nothing)
                Return ' validation dialog was shown
            End If
            ApplyFilter()
            SetBusy(False, String.Format("Deleted ""{0}"" and saved {1} row(s).", selected.Name, saved))
        Catch ex As Exception
            SetBusy(False, Nothing)
            ShowErrorDialog("Delete", ex)
        End Try
    End Function

    ''' <summary>
    ''' Writes the whole product list to the sheet. Returns the number of
    ''' rows saved, or -1 when validation blocked the write (dialog shown).
    ''' Callers are responsible for SetBusy around it.
    ''' </summary>
    Private Async Function SaveAllProductsAsync(action As String) As Threading.Tasks.Task(Of Integer)
        Dim products As IList(Of Product) = _products.ToList()

        Dim validation As ValidationResult = ProductValidator.ValidateForSave(products)
        If Not validation.IsValid Then
            ShowValidationDialog(validation, "Cannot " + action.ToLowerInvariant() + " changes")
            SetStatus(action + " cancelled: fix the rows shown in the dialog.", True)
            Return -1
        End If

        Using service As GoogleSheetService = CreateSheetService()
            Dim repository As New ProductRepository(service, SheetName)
            Await repository.SaveAllAsync(products)
            Return products.Count
        End Using
    End Function

    ' --- search / filter ---

    Private Sub txtSearch_TextChanged(sender As Object, e As EventArgs) Handles txtSearch.TextChanged
        _page = 1 ' a new search always starts from the first page
        ApplyFilter()
    End Sub

    Private Sub btnPrev_Click(sender As Object, e As EventArgs) Handles btnPrev.Click
        _page -= 1
        ApplyFilter()
    End Sub

    Private Sub btnNext_Click(sender As Object, e As EventArgs) Handles btnNext.Click
        _page += 1
        ApplyFilter()
    End Sub

    Private Sub cmbPageSize_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbPageSize.SelectedIndexChanged
        Dim size As Integer
        If Integer.TryParse(Convert.ToString(cmbPageSize.SelectedItem, CultureInfo.InvariantCulture),
            NumberStyles.Integer, CultureInfo.InvariantCulture, size) AndAlso size > 0 Then
            _pageSize = size
        End If
        _page = 1
        ApplyFilter()
    End Sub

    Private Sub ApplyFilter()
        If Not _viewReady Then
            Return ' controls are still being built
        End If

        Dim query As String = txtSearch.Text
        Dim filtering As Boolean = query.Trim().Length > 0

        Dim source As IList(Of Product) = ProductFilter.Apply(_products, query)
        _totalPages = ProductPager.TotalPages(source.Count, _pageSize)
        _page = ProductPager.ClampPage(_page, _totalPages)

        Dim view As New BindingList(Of Product)()
        For Each product As Product In ProductPager.Slice(source, _page, _pageSize)
            view.Add(product) ' same object references as the master list
        Next

        dataGridViewProducts.DataSource = view

        lblPageInfo.Text = String.Format(CultureInfo.InvariantCulture,
            "Page {0} of {1} - {2} row(s)", _page, _totalPages, source.Count)

        If filtering Then
            SetStatus(String.Format("Filtering: {0} of {1} row(s) match; showing page {2} of {3}.",
                source.Count, _products.Count, _page, _totalPages))
        End If
        UpdateButtonStates()
    End Sub

    ' --- helpers ---

    Private Function SelectedProduct() As Product
        If dataGridViewProducts.CurrentRow Is Nothing Then
            Return Nothing
        End If
        Return TryCast(dataGridViewProducts.CurrentRow.DataBoundItem, Product)
    End Function

    Private Sub RefreshGridAfterMutation()
        ApplyFilter() ' keeps the current page (clamped) and re-renders from the master list
    End Sub

    Private Sub ClearInputFields()
        txtName.Text = ""
        txtQuantity.Text = ""
        txtPrice.Text = ""
    End Sub

    ''' <summary>Runtime failure (Google API, missing file, ...): status bar plus an error dialog.</summary>
    Private Sub ShowErrorDialog(action As String, ex As Exception)
        SetStatus(action + " failed: " + ex.Message, True)
        MessageBox.Show(Me,
            action + " failed." & vbLf & vbLf & ex.Message,
            action + " Failed",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error)
    End Sub

    ''' <summary>Invalid user input: warning dialog listing every failure at once.</summary>
    Private Sub ShowValidationDialog(validation As ValidationResult, title As String)
        MessageBox.Show(Me,
            validation.ToDialogText(),
            title,
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning)
    End Sub

    Private Function CreateSheetService() As GoogleSheetService
        Dim configuredPath As String = If(ConfigurationManager.AppSettings("GoogleCredentialsPath"), "credentials.json")
        Dim credentialsPath As String = ResolveCredentialsPath(configuredPath)
        If Not File.Exists(credentialsPath) Then
            Throw New FileNotFoundException(
                "Google credentials file not found ('" & configuredPath & "'). " &
                "Follow the setup steps in README.md to create credentials.json.",
                credentialsPath)
        End If

        Dim spreadsheetId As String = txtSpreadsheetId.Text.Trim()
        If spreadsheetId.Length = 0 Then
            Throw New InvalidOperationException(
                "Spreadsheet ID is empty. Paste the ID from the sheet URL into the box (or App.config).")
        End If

        Return New GoogleSheetService(spreadsheetId, credentialsPath, "Google Sheets Demo")
    End Function

    Private Shared Function ResolveCredentialsPath(configuredPath As String) As String
        If Path.IsPathRooted(configuredPath) Then
            Return configuredPath
        End If

        Dim baseDirectory As String = AppDomain.CurrentDomain.BaseDirectory

        ' 1) next to the exe (bin\Debug), 2) current folder,
        ' 3) the project folder (bin\Debug\..\..) so the file works
        '    straight from the source tree without copying it manually.
        Dim candidates() As String = {
            Path.Combine(baseDirectory, configuredPath),
            Path.Combine(Environment.CurrentDirectory, configuredPath),
            Path.GetFullPath(Path.Combine(baseDirectory, "..", "..", configuredPath))
        }

        For Each candidate As String In candidates
            If File.Exists(candidate) Then
                Return candidate
            End If
        Next

        Return candidates(0)
    End Function

    Private Sub ReplaceGridItems(products As IEnumerable(Of Product))
        _products.Clear()
        For Each product As Product In products
            _products.Add(product)
        Next
    End Sub

    Private Sub dataGridViewProducts_SelectionChanged(sender As Object, e As EventArgs) Handles dataGridViewProducts.SelectionChanged
        UpdateButtonStates()
    End Sub

    ''' <summary>
    ''' Single place that decides which controls are enabled:
    ''' - nothing is clickable while a request is in flight (busy);
    ''' - Edit/Delete need a selected row (greyed out otherwise);
    ''' - Save needs data that came from the sheet or was appended, so an
    '''   untouched empty grid cannot wipe the sheet by accident.
    ''' </summary>
    Private Sub UpdateButtonStates()
        btnLoad.Enabled = Not _busy
        txtSearch.Enabled = Not _busy
        btnAppend.Enabled = Not _busy
        btnCancel.Enabled = Not _busy
        btnSave.Enabled = Not _busy AndAlso (_loaded OrElse _products.Count > 0)
        btnPrev.Enabled = Not _busy AndAlso _page > 1
        btnNext.Enabled = Not _busy AndAlso _page < _totalPages
        btnTheme.Enabled = Not _busy
        cmbPageSize.Enabled = Not _busy
    End Sub

    Private Sub SetStatus(message As String, Optional isError As Boolean = False)
        _lastStatus = message
        _lastStatusIsError = isError
        RenderStatus()
    End Sub

    Private Sub RenderStatus()
        lblStatus.ForeColor = If(_lastStatusIsError, _palette.StatusError, _palette.StatusText)
        lblStatus.Text = _lastStatus
    End Sub

    ''' <summary>
    ''' Turns on the loading state: spinner over the grid, busy flag set and
    ''' every control disabled through UpdateButtonStates so a request
    ''' cannot be fired twice. Pass null as the message to keep the current
    ''' status text.
    ''' </summary>
    Private Sub SetBusy(busy As Boolean, message As String)
        _busy = busy

        _spinner.Visible = busy
        If busy Then
            _spinner.Start()
        Else
            _spinner.Stop()
        End If

        UpdateButtonStates()

        If message IsNot Nothing Then
            SetStatus(message)
        End If
    End Sub

    Private Sub CenterSpinnerOnGrid()
        If _spinner Is Nothing Then
            Return
        End If

        _spinner.Location = New Point(
            dataGridViewProducts.Left + (dataGridViewProducts.Width - _spinner.Width) \ 2,
            dataGridViewProducts.Top + (dataGridViewProducts.Height - _spinner.Height) \ 2)
    End Sub

    Protected Overrides Sub WndProc(ByRef m As Message)
        Const WM_SETTINGCHANGE As Integer = &H1A
        If m.Msg = WM_SETTINGCHANGE Then
            ' Windows broadcasts this when personalization changes;
            ' "ImmersiveColorSet" identifies a light/dark theme switch.
            Dim section As String = Marshal.PtrToStringAuto(m.LParam)
            If section = "ImmersiveColorSet" Then
                ApplyTheme(If(_themeOverride, SystemTheme.IsDarkMode()))
            End If
        End If
        MyBase.WndProc(m)
    End Sub

    Private Sub btnTheme_Click(sender As Object, e As EventArgs) Handles btnTheme.Click
        _themeOverride = Not _isDark
        ApplyTheme(_themeOverride.Value)
        PersistThemeOverride(_themeOverride.Value)
        SetStatus("Switched to the " & If(_isDark, "dark", "light") & " theme.")
    End Sub

    ''' <summary>
    ''' Nothing = follow the Windows theme; True/False = manual choice.
    ''' The choice lives in a UTF-8 sidecar file next to the exe - never in
    ''' the .config, because ConfigurationManager.Save() rewrites the whole
    ''' config with the machine's ANSI codepage and corrupts non-ASCII
    ''' values (it turned the Thai sheet name into "????1" mojibake).
    ''' </summary>
    Private Shared Function LoadThemeOverride() As Boolean?
        Dim value As String = ReadThemeSidecar()
        If value Is Nothing Then
            value = ConfigurationManager.AppSettings("Theme") ' choice made before the sidecar existed
        End If
        If String.Equals(value, "dark", StringComparison.OrdinalIgnoreCase) Then
            Return True
        End If
        If String.Equals(value, "light", StringComparison.OrdinalIgnoreCase) Then
            Return False
        End If
        Return Nothing
    End Function

    Private Shared Sub PersistThemeOverride(dark As Boolean)
        Try
            ' never write the .config from here: its Thai SheetName would
            ' be mangled by the ANSI round-trip of ConfigurationManager
            File.WriteAllText(ThemeSidecarPath(), If(dark, "dark", "light"), System.Text.Encoding.UTF8)
        Catch
            ' file not writable - the toggle still works for this session
        End Try
    End Sub

    Private Shared Function ThemeSidecarPath() As String
        Return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GoogleSheetsDemo.theme")
    End Function

    Private Shared Function ReadThemeSidecar() As String
        Try
            Dim path As String = ThemeSidecarPath()
            If Not File.Exists(path) Then
                Return Nothing
            End If
            Return File.ReadAllText(path, System.Text.Encoding.UTF8).Trim()
        Catch
            Return Nothing ' unreadable sidecar = follow the Windows theme
        End Try
    End Function

#Region "Rounded modern styling"

    Private Sub ApplyModernStyle()
        Me.Font = New Font("Segoe UI", 9.0F)

        SetupInputField(txtSpreadsheetId)
        SetupInputField(txtName)
        SetupInputField(txtQuantity)
        SetupInputField(txtPrice)
        SetupInputField(txtSearch)

        dataGridViewProducts.BorderStyle = BorderStyle.None
        dataGridViewProducts.EnableHeadersVisualStyles = False
        dataGridViewProducts.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing
        dataGridViewProducts.ColumnHeadersHeight = 34
        dataGridViewProducts.RowHeadersVisible = False
        dataGridViewProducts.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal

        Round(dataGridViewProducts, 12)
        Round(pnlId, 6)
        Round(pnlName, 6)
        Round(pnlQuantity, 6)
        Round(pnlPrice, 6)
        Round(pnlSearch, 6)
        Round(btnLoad, 10)
        Round(btnAppend, 10)
        Round(btnSave, 10)
        Round(btnCancel, 10)
        Round(btnPrev, 10)
        Round(btnNext, 10)
        Round(btnTheme, 10)

        ' loading spinner floats over the middle of the grid while a
        ' request is in flight
        _spinner = New LoadingSpinner With {
            .Size = New Size(28, 28),
            .Visible = False
        }
        Controls.Add(_spinner)
        _spinner.BringToFront()
        CenterSpinnerOnGrid()

        ' controls anchored to the window edge change size together with it
        AddHandler dataGridViewProducts.Resize,
            Sub()
                Round(dataGridViewProducts, 12)
                CenterSpinnerOnGrid()
            End Sub
        AddHandler pnlId.Resize,
            Sub()
                Round(pnlId, 6)
                CenterInputInField(txtSpreadsheetId)
            End Sub
        AddHandler pnlSearch.Resize,
            Sub()
                Round(pnlSearch, 6)
                CenterInputInField(txtSearch)
            End Sub

        TryRoundWindowCorners()

        ' colors come last so every themed control gets the palette.
        ' A manually chosen theme (sun/moon button) wins over the system.
        _themeOverride = LoadThemeOverride()
        ApplyTheme(If(_themeOverride, SystemTheme.IsDarkMode()))

        ' designed label positions assume design-time font metrics; the
        ' runtime font makes auto-sized labels drift off the input's
        ' vertical center, so snap every caption to its input afterwards
        AlignLabelsWithInputs()
    End Sub

    ''' <summary>
    ''' One baseline pass: every caption, input and button that shares a row
    ''' is centered on the same vertical middle, so text lines up even when
    ''' control heights differ (25px boxes vs 34px buttons vs 24px combo).
    ''' </summary>
    Private Sub AlignLabelsWithInputs()
        ' captions centered on their inputs (the panel forms the visible
        ' field, so baselines must match the panel, not the inner edit)
        CenterLabelVertically(lblName, pnlName)
        CenterLabelVertically(lblQuantity, pnlQuantity)
        CenterLabelVertically(lblPrice, pnlPrice)
        CenterLabelVertically(lblSearch, pnlSearch)
        CenterLabelVertically(lblPageSize, cmbPageSize)
        CenterLabelVertically(lblPageInfo, cmbPageSize)

        ' buttons centered on the row they belong to
        CenterLabelVertically(btnLoad, pnlId)
        CenterLabelVertically(btnTheme, pnlId)
        CenterLabelVertically(btnAppend, pnlPrice)
        CenterLabelVertically(btnCancel, pnlPrice)
        CenterLabelVertically(btnPrev, cmbPageSize)
        CenterLabelVertically(btnNext, cmbPageSize)
    End Sub

    ''' <summary>
    ''' Keeps the pager strip between the grid and the bottom-anchored edit
    ''' form: the strip sits just above the form and the grid ends above the
    ''' strip, so resizing the window never stacks them on top of each other.
    ''' </summary>
    Private Sub RelayoutLowerArea()
        If Not _viewReady Then
            Return
        End If

        Dim pagerTop As Integer = grpNewProduct.Top - 44
        btnPrev.Top = pagerTop
        btnNext.Top = pagerTop
        ' the combo is shorter than the buttons - center it on their middle,
        ' then AlignLabelsWithInputs pulls the two labels onto the same line
        cmbPageSize.Top = pagerTop + (btnPrev.Height - cmbPageSize.Height) \ 2

        dataGridViewProducts.Height = pagerTop - 6 - dataGridViewProducts.Top

        Round(dataGridViewProducts, 12)
        CenterSpinnerOnGrid()
        AlignLabelsWithInputs()
    End Sub

    Private Shared Sub CenterLabelVertically(label As Control, input As Control)
        label.Top = input.Top + (input.Height - label.Height) \ 2
    End Sub

    Private Shared Sub StyleButton(button As Button, backColor As Color, foreColor As Color,
                                   hoverColor As Color, pressedColor As Color)
        button.FlatStyle = FlatStyle.Flat
        button.FlatAppearance.BorderSize = 0
        button.FlatAppearance.MouseOverBackColor = hoverColor
        button.FlatAppearance.MouseDownBackColor = pressedColor
        button.UseVisualStyleBackColor = False
        button.BackColor = backColor
        button.ForeColor = foreColor
        button.Cursor = Cursors.Hand
    End Sub

    ''' <summary>Colors the per-row Edit / Delete button columns to match the theme.</summary>
    Private Shared Sub StyleActionColumn(column As DataGridViewButtonColumn, back As Color, fore As Color)
        column.FlatStyle = FlatStyle.Flat
        column.DefaultCellStyle.BackColor = back
        column.DefaultCellStyle.ForeColor = fore
        column.DefaultCellStyle.SelectionBackColor = back
        column.DefaultCellStyle.SelectionForeColor = fore
    End Sub

    Private Sub StyleTextBox(textBox As TextBox)
        textBox.BackColor = _palette.TextBoxBack
        textBox.ForeColor = _palette.TextBoxText
        textBox.Parent.BackColor = _palette.TextBoxBack ' the visible field
    End Sub

    ''' <summary>
    ''' A single-line borderless edit always paints its text at the very
    ''' top of its client area, so a tall edit box can never look
    ''' vertically centered. The visible field is therefore the rounded
    ''' panel behind the box, and the edit itself is made exactly one
    ''' text line tall, centered inside that panel - with no extra space
    ''' above the text, nothing can float to the top edge.
    ''' </summary>
    Private Shared Sub SetupInputField(textBox As TextBox)
        textBox.AutoSize = False
        textBox.BorderStyle = BorderStyle.None
        Dim field As Control = textBox.Parent ' rounded panel behind the box
        AddHandler field.Click, Sub() textBox.Focus()
        CenterInputInField(textBox)
    End Sub

    ''' <summary>Sizes and places the edit inside its field panel.</summary>
    Private Shared Sub CenterInputInField(textBox As TextBox)
        Dim field As Control = textBox.Parent
        textBox.Height = textBox.Font.Height + 2
        textBox.Top = (field.Height - textBox.Height) \ 2
        textBox.Left = 2
        textBox.Width = field.Width - 4
    End Sub

    Private Shared Sub Round(control As Control, radius As Integer)
        Dim bounds As New Rectangle(0, 0, control.Width, control.Height)
        If bounds.Width < radius * 2 OrElse bounds.Height < radius * 2 Then
            Return
        End If

        Dim path As New GraphicsPath()
        path.AddArc(bounds.X, bounds.Y, radius, radius, 180, 90)
        path.AddArc(bounds.Right - radius - 1, bounds.Y, radius, radius, 270, 90)
        path.AddArc(bounds.Right - radius - 1, bounds.Bottom - radius - 1, radius, radius, 0, 90)
        path.AddArc(bounds.X, bounds.Bottom - radius - 1, radius, radius, 90, 90)
        path.CloseFigure()
        control.Region = New Region(path)
    End Sub

    Private Sub TryRoundWindowCorners()
        Try
            ' 33 = DWMWA_WINDOW_CORNER_PREFERENCE, 2 = DWMWCP_ROUND.
            ' Windows 11 rounds the window with anti-aliasing; older
            ' Windows just ignores the attribute.
            Dim preference As Integer = 2
            DwmSetWindowAttribute(Me.Handle, 33, preference, 4)
        Catch ex As DllNotFoundException
            ' dwmapi.dll not available - nothing to do.
        End Try
    End Sub

    <DllImport("dwmapi.dll")>
    Private Shared Function DwmSetWindowAttribute(hwnd As IntPtr, attribute As Integer, ByRef value As Integer, size As Integer) As Integer
    End Function

    ''' <summary>
    ''' Regenerates the icon in front of every button label from the
    ''' active palette. While a row is being edited the append button
    ''' shows a pencil instead of a plus.
    ''' </summary>
    Private Sub ApplyButtonIcons()
        SetButtonIcon(btnLoad, ButtonIcons.Create(ButtonIconKind.Refresh, _palette.SecondaryFore))
        SetButtonIcon(btnAppend, ButtonIcons.Create(
            If(_editingProduct IsNot Nothing, ButtonIconKind.Pencil, ButtonIconKind.Plus),
            _palette.AppendFore))
        SetButtonIcon(btnSave, ButtonIcons.Create(ButtonIconKind.Save, _palette.SaveFore))
        SetButtonIcon(btnCancel, ButtonIcons.Create(ButtonIconKind.Cross, _palette.SecondaryFore))
        ' the pager buttons are text-only: their < and > characters ARE
        ' the direction marks, an icon in front would duplicate them
        SetButtonIcon(btnTheme, ButtonIcons.Create(
            If(_isDark, ButtonIconKind.Sun, ButtonIconKind.Moon),
            _palette.SecondaryFore))

        ' same icons, drawn into the per-row Edit / Delete cells
        ReplaceCellIcon(_editCellIcon, ButtonIcons.Create(ButtonIconKind.Pencil, _palette.SecondaryFore))
        ReplaceCellIcon(_deleteCellIcon, ButtonIcons.Create(ButtonIconKind.Trash, _palette.DeleteFore))
        dataGridViewProducts.Invalidate()
    End Sub

    Private Shared Sub ReplaceCellIcon(ByRef field As Bitmap, icon As Bitmap)
        Dim old As Bitmap = field
        field = icon
        If old IsNot Nothing Then
            old.Dispose()
        End If
    End Sub

    Private Shared Sub SetButtonIcon(button As Button, icon As Bitmap)
        Dim old As Image = button.Image
        button.Image = icon
        button.TextImageRelation = TextImageRelation.ImageBeforeText
        If old IsNot Nothing Then
            old.Dispose()
        End If
    End Sub

    ''' <summary>Applies the light or dark palette to every themed control.</summary>
    Private Sub ApplyTheme(dark As Boolean)
        _isDark = dark
        _palette = If(dark, AppTheme.Dark, AppTheme.Light)

        BackColor = _palette.FormBack

        StyleButton(btnLoad, _palette.SecondaryBack, _palette.SecondaryFore,
            _palette.SecondaryHover, _palette.SecondaryPressed)
        StyleButton(btnCancel, _palette.SecondaryBack, _palette.SecondaryFore,
            _palette.SecondaryHover, _palette.SecondaryPressed)
        StyleButton(btnAppend, _palette.AppendBack, _palette.AppendFore,
            _palette.AppendHover, _palette.AppendPressed)
        StyleButton(btnSave, _palette.SaveBack, _palette.SaveFore,
            _palette.SaveHover, _palette.SavePressed)
        StyleButton(btnPrev, _palette.SecondaryBack, _palette.SecondaryFore,
            _palette.SecondaryHover, _palette.SecondaryPressed)
        StyleButton(btnNext, _palette.SecondaryBack, _palette.SecondaryFore,
            _palette.SecondaryHover, _palette.SecondaryPressed)
        StyleButton(btnTheme, _palette.SecondaryBack, _palette.SecondaryFore,
            _palette.SecondaryHover, _palette.SecondaryPressed)
        StyleActionColumn(colEdit, _palette.SecondaryBack, _palette.SecondaryFore)
        StyleActionColumn(colDelete, _palette.DeleteBack, _palette.DeleteFore)

        ApplyButtonIcons()

        StyleTextBox(txtSpreadsheetId)
        StyleTextBox(txtName)
        StyleTextBox(txtQuantity)
        StyleTextBox(txtPrice)
        StyleTextBox(txtSearch)

        dataGridViewProducts.BackgroundColor = _palette.GridBack
        dataGridViewProducts.GridColor = _palette.GridLine
        dataGridViewProducts.ColumnHeadersDefaultCellStyle.BackColor = _palette.HeaderBack
        dataGridViewProducts.ColumnHeadersDefaultCellStyle.ForeColor = _palette.HeaderText
        dataGridViewProducts.ColumnHeadersDefaultCellStyle.SelectionBackColor = _palette.HeaderBack
        dataGridViewProducts.ColumnHeadersDefaultCellStyle.SelectionForeColor = _palette.HeaderText
        dataGridViewProducts.DefaultCellStyle.BackColor = _palette.CellBack
        dataGridViewProducts.DefaultCellStyle.ForeColor = _palette.CellText
        dataGridViewProducts.DefaultCellStyle.SelectionBackColor = _palette.SelectionBack
        dataGridViewProducts.DefaultCellStyle.SelectionForeColor = _palette.SelectionText
        dataGridViewProducts.AlternatingRowsDefaultCellStyle.BackColor = _palette.AlternatingRow
        dataGridViewProducts.RowTemplate.DefaultCellStyle.BackColor = _palette.CellBack
        dataGridViewProducts.RowTemplate.DefaultCellStyle.ForeColor = _palette.CellText

        lblSpreadsheetId.ForeColor = _palette.LabelText
        lblName.ForeColor = _palette.LabelText
        lblQuantity.ForeColor = _palette.LabelText
        lblPrice.ForeColor = _palette.LabelText
        lblSearch.ForeColor = _palette.LabelText
        lblPageInfo.ForeColor = _palette.LabelText
        lblPageSize.ForeColor = _palette.LabelText
        grpNewProduct.ForeColor = _palette.PrimaryText

        cmbPageSize.BackColor = _palette.TextBoxBack
        cmbPageSize.ForeColor = _palette.TextBoxText

        _spinner.SetColors(_palette.SpinnerDisc, _palette.SpinnerTrack, _palette.SpinnerArc)

        TryImmersiveTitleBar(dark)
        RenderStatus()
    End Sub

    Private Sub TryImmersiveTitleBar(dark As Boolean)
        Try
            ' 20 = DWMWA_USE_IMMERSIVE_DARK_MODE (attribute 19 on early
            ' Windows 10 builds) - colors the title bar with the theme.
            Dim value As Integer = If(dark, 1, 0)
            If DwmSetWindowAttribute(Me.Handle, 20, value, 4) <> 0 Then
                DwmSetWindowAttribute(Me.Handle, 19, value, 4)
            End If
        Catch ex As DllNotFoundException
            ' dwmapi.dll not available - nothing to do.
        End Try
    End Sub

#End Region

End Class
