<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Public Class MainForm
    Inherits System.Windows.Forms.Form

    ''' <summary>
    ''' Clean up any resources being used.
    ''' </summary>
    ''' <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim dataGridViewCellStyle5 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim dataGridViewCellStyle6 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Me.lblSpreadsheetId = New System.Windows.Forms.Label()
        Me.pnlId = New System.Windows.Forms.Panel()
        Me.txtSpreadsheetId = New System.Windows.Forms.TextBox()
        Me.btnLoad = New System.Windows.Forms.Button()
        Me.btnTheme = New System.Windows.Forms.Button()
        Me.dataGridViewProducts = New System.Windows.Forms.DataGridView()
        Me.colId = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colName = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colQuantity = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colPrice = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colEdit = New System.Windows.Forms.DataGridViewButtonColumn()
        Me.colDelete = New System.Windows.Forms.DataGridViewButtonColumn()
        Me.grpNewProduct = New System.Windows.Forms.GroupBox()
        Me.pnlName = New System.Windows.Forms.Panel()
        Me.lblName = New System.Windows.Forms.Label()
        Me.txtName = New System.Windows.Forms.TextBox()
        Me.pnlQuantity = New System.Windows.Forms.Panel()
        Me.lblQuantity = New System.Windows.Forms.Label()
        Me.txtQuantity = New System.Windows.Forms.TextBox()
        Me.pnlPrice = New System.Windows.Forms.Panel()
        Me.lblPrice = New System.Windows.Forms.Label()
        Me.txtPrice = New System.Windows.Forms.TextBox()
        Me.btnAppend = New System.Windows.Forms.Button()
        Me.btnCancel = New System.Windows.Forms.Button()
        Me.btnSave = New System.Windows.Forms.Button()
        Me.lblStatus = New System.Windows.Forms.Label()
        Me.lblSearch = New System.Windows.Forms.Label()
        Me.pnlSearch = New System.Windows.Forms.Panel()
        Me.txtSearch = New System.Windows.Forms.TextBox()
        Me.btnPrev = New System.Windows.Forms.Button()
        Me.btnNext = New System.Windows.Forms.Button()
        Me.lblPageInfo = New System.Windows.Forms.Label()
        Me.lblPageSize = New System.Windows.Forms.Label()
        Me.cmbPageSize = New System.Windows.Forms.ComboBox()
        CType(Me.dataGridViewProducts, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpNewProduct.SuspendLayout()
        Me.SuspendLayout()
        '
        'lblSpreadsheetId
        '
        Me.lblSpreadsheetId.AutoSize = True
        Me.lblSpreadsheetId.ForeColor = System.Drawing.Color.FromArgb(86, 96, 110)
        Me.lblSpreadsheetId.Location = New System.Drawing.Point(14, 10)
        Me.lblSpreadsheetId.Name = "lblSpreadsheetId"
        Me.lblSpreadsheetId.Size = New System.Drawing.Size(81, 13)
        Me.lblSpreadsheetId.TabIndex = 8
        Me.lblSpreadsheetId.Text = "Spreadsheet ID"
        '
        'pnlId (the visible input field; the edit box inside is one
        'text line tall so its text cannot float to the top edge)
        '
        Me.pnlId.Controls.Add(Me.txtSpreadsheetId)
        Me.pnlId.Location = New System.Drawing.Point(14, 26)
        Me.pnlId.Name = "pnlId"
        Me.pnlId.Size = New System.Drawing.Size(604, 28)
        Me.pnlId.TabIndex = 0
        '
        'txtSpreadsheetId
        '
        Me.txtSpreadsheetId.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right, System.Windows.Forms.AnchorStyles)
        Me.txtSpreadsheetId.Location = New System.Drawing.Point(2, 1)
        Me.txtSpreadsheetId.Name = "txtSpreadsheetId"
        Me.txtSpreadsheetId.Size = New System.Drawing.Size(600, 18)
        Me.txtSpreadsheetId.TabIndex = 0
        '
        'btnLoad
        '
        Me.btnLoad.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnLoad.BackColor = System.Drawing.Color.White
        Me.btnLoad.Cursor = System.Windows.Forms.Cursors.Hand
        Me.btnLoad.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(203, 213, 225)
        Me.btnLoad.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(233, 240, 250)
        Me.btnLoad.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnLoad.Font = New System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold)
        Me.btnLoad.ForeColor = System.Drawing.Color.FromArgb(37, 64, 97)
        Me.btnLoad.Location = New System.Drawing.Point(660, 24)
        Me.btnLoad.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.btnLoad.Name = "btnLoad"
        Me.btnLoad.Size = New System.Drawing.Size(98, 28)
        Me.btnLoad.TabIndex = 1
        Me.btnLoad.Text = "Load"
        Me.btnLoad.UseVisualStyleBackColor = False
        '
        'btnTheme
        '
        Me.btnTheme.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnTheme.Cursor = System.Windows.Forms.Cursors.Hand
        Me.btnTheme.FlatAppearance.BorderSize = 0
        Me.btnTheme.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnTheme.Font = New System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold)
        Me.btnTheme.Location = New System.Drawing.Point(624, 24)
        Me.btnTheme.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.btnTheme.Name = "btnTheme"
        Me.btnTheme.Size = New System.Drawing.Size(29, 28)
        Me.btnTheme.TabIndex = 17
        Me.btnTheme.UseVisualStyleBackColor = False
        '
        'dataGridViewProducts
        '
        Me.dataGridViewProducts.AllowUserToAddRows = False
        Me.dataGridViewProducts.AllowUserToResizeRows = False
        Me.dataGridViewProducts.AutoGenerateColumns = False
        Me.dataGridViewProducts.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.dataGridViewProducts.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.dataGridViewProducts.BackgroundColor = System.Drawing.Color.White
        Me.dataGridViewProducts.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.dataGridViewProducts.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal
        Me.dataGridViewProducts.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single
        dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        dataGridViewCellStyle5.BackColor = System.Drawing.Color.FromArgb(0, 120, 215)
        dataGridViewCellStyle5.Font = New System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold)
        dataGridViewCellStyle5.ForeColor = System.Drawing.Color.White
        dataGridViewCellStyle5.Padding = New System.Windows.Forms.Padding(6, 0, 6, 0)
        dataGridViewCellStyle5.SelectionBackColor = System.Drawing.Color.FromArgb(0, 120, 215)
        dataGridViewCellStyle5.SelectionForeColor = System.Drawing.Color.White
        dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.[False]
        Me.dataGridViewProducts.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle5
        Me.dataGridViewProducts.ColumnHeadersHeight = 38
        Me.dataGridViewProducts.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing
        Me.dataGridViewProducts.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colId, Me.colName, Me.colQuantity, Me.colPrice, Me.colEdit, Me.colDelete})
        dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        dataGridViewCellStyle6.BackColor = System.Drawing.Color.White
        dataGridViewCellStyle6.Font = New System.Drawing.Font("Segoe UI", 9.5F)
        dataGridViewCellStyle6.ForeColor = System.Drawing.Color.FromArgb(40, 44, 52)
        dataGridViewCellStyle6.Padding = New System.Windows.Forms.Padding(3, 4, 3, 4)
        dataGridViewCellStyle6.SelectionBackColor = System.Drawing.Color.FromArgb(205, 228, 255)
        dataGridViewCellStyle6.SelectionForeColor = System.Drawing.Color.FromArgb(10, 25, 40)
        dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.[False]
        Me.dataGridViewProducts.DefaultCellStyle = dataGridViewCellStyle6
        Me.dataGridViewProducts.EnableHeadersVisualStyles = False
        Me.dataGridViewProducts.GridColor = System.Drawing.Color.FromArgb(228, 234, 242)
        Me.dataGridViewProducts.Location = New System.Drawing.Point(14, 94)
        Me.dataGridViewProducts.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.dataGridViewProducts.MultiSelect = False
        Me.dataGridViewProducts.Name = "dataGridViewProducts"
        Me.dataGridViewProducts.ReadOnly = True
        Me.dataGridViewProducts.RowHeadersVisible = False
        Me.dataGridViewProducts.RowTemplate.DefaultCellStyle.BackColor = System.Drawing.Color.White
        Me.dataGridViewProducts.RowTemplate.Height = 30
        Me.dataGridViewProducts.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.[False]
        Me.dataGridViewProducts.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.dataGridViewProducts.Size = New System.Drawing.Size(744, 223)
        Me.dataGridViewProducts.TabIndex = 2
        '
        'colId
        '
        Me.colId.DataPropertyName = "Id"
        Me.colId.FillWeight = 12.0F
        Me.colId.HeaderText = "ID"
        Me.colId.Name = "colId"
        Me.colId.ReadOnly = True
        '
        'colName
        '
        Me.colName.DataPropertyName = "Name"
        Me.colName.FillWeight = 46.0F
        Me.colName.HeaderText = "Name"
        Me.colName.Name = "colName"
        Me.colName.ReadOnly = True
        '
        'colQuantity
        '
        Me.colQuantity.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight
        Me.colQuantity.FillWeight = 20.0F
        Me.colQuantity.HeaderText = "Quantity"
        Me.colQuantity.Name = "colQuantity"
        Me.colQuantity.ReadOnly = True
        '
        'colPrice
        '
        Me.colPrice.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight
        Me.colPrice.DefaultCellStyle.Format = "N2"
        Me.colPrice.FillWeight = 22.0F
        Me.colPrice.HeaderText = "Price"
        Me.colPrice.Name = "colPrice"
        Me.colPrice.ReadOnly = True
        '
        'colEdit
        '
        Me.colEdit.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.colEdit.FillWeight = 11.0F
        Me.colEdit.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.colEdit.HeaderText = "Edit"
        Me.colEdit.MinimumWidth = 48
        Me.colEdit.Name = "colEdit"
        Me.colEdit.ReadOnly = True
        Me.colEdit.ToolTipText = "Edit this row"
        Me.colEdit.UseColumnTextForButtonValue = True
        '
        'colDelete
        '
        Me.colDelete.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.colDelete.FillWeight = 11.0F
        Me.colDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.colDelete.HeaderText = "Delete"
        Me.colDelete.MinimumWidth = 48
        Me.colDelete.Name = "colDelete"
        Me.colDelete.ReadOnly = True
        Me.colDelete.ToolTipText = "Delete this row"
        Me.colDelete.UseColumnTextForButtonValue = True
        '
        'grpNewProduct
        '
        Me.grpNewProduct.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right, System.Windows.Forms.AnchorStyles)
        Me.grpNewProduct.Controls.Add(Me.lblName)
        Me.grpNewProduct.Controls.Add(Me.pnlName)
        Me.grpNewProduct.Controls.Add(Me.lblQuantity)
        Me.grpNewProduct.Controls.Add(Me.pnlQuantity)
        Me.grpNewProduct.Controls.Add(Me.lblPrice)
        Me.grpNewProduct.Controls.Add(Me.pnlPrice)
        Me.grpNewProduct.Controls.Add(Me.btnAppend)
        Me.grpNewProduct.Controls.Add(Me.btnCancel)
        Me.grpNewProduct.Font = New System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold)
        Me.grpNewProduct.ForeColor = System.Drawing.Color.FromArgb(50, 60, 75)
        Me.grpNewProduct.Location = New System.Drawing.Point(14, 359)
        Me.grpNewProduct.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.grpNewProduct.Name = "grpNewProduct"
        Me.grpNewProduct.Padding = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.grpNewProduct.Size = New System.Drawing.Size(744, 84)
        Me.grpNewProduct.TabIndex = 3
        Me.grpNewProduct.TabStop = False
        Me.grpNewProduct.Text = "Add new product (appends a row to the sheet)"
        '
        'lblName
        '
        Me.lblName.AutoSize = True
        Me.lblName.Font = New System.Drawing.Font("Segoe UI", 9.5F)
        Me.lblName.ForeColor = System.Drawing.Color.FromArgb(86, 96, 110)
        Me.lblName.Location = New System.Drawing.Point(12, 39)
        Me.lblName.Name = "lblName"
        Me.lblName.Size = New System.Drawing.Size(43, 17)
        Me.lblName.TabIndex = 0
        Me.lblName.Text = "Name"
        '
        'pnlName (visible field, see pnlId)
        '
        Me.pnlName.Controls.Add(Me.txtName)
        Me.pnlName.Location = New System.Drawing.Point(53, 37)
        Me.pnlName.Name = "pnlName"
        Me.pnlName.Size = New System.Drawing.Size(198, 28)
        Me.pnlName.TabIndex = 3
        '
        'txtName
        '
        Me.txtName.Font = New System.Drawing.Font("Segoe UI", 9.5F)
        Me.txtName.Location = New System.Drawing.Point(2, 2)
        Me.txtName.Name = "txtName"
        Me.txtName.Size = New System.Drawing.Size(194, 19)
        Me.txtName.TabIndex = 3
        '
        'lblQuantity
        '
        Me.lblQuantity.AutoSize = True
        Me.lblQuantity.Font = New System.Drawing.Font("Segoe UI", 9.5F)
        Me.lblQuantity.ForeColor = System.Drawing.Color.FromArgb(86, 96, 110)
        Me.lblQuantity.Location = New System.Drawing.Point(267, 39)
        Me.lblQuantity.Name = "lblQuantity"
        Me.lblQuantity.Size = New System.Drawing.Size(56, 17)
        Me.lblQuantity.TabIndex = 1
        Me.lblQuantity.Text = "Quantity"
        '
        'pnlQuantity (visible field, see pnlId)
        '
        Me.pnlQuantity.Controls.Add(Me.txtQuantity)
        Me.pnlQuantity.Location = New System.Drawing.Point(326, 37)
        Me.pnlQuantity.Name = "pnlQuantity"
        Me.pnlQuantity.Size = New System.Drawing.Size(73, 28)
        Me.pnlQuantity.TabIndex = 4
        '
        'txtQuantity
        '
        Me.txtQuantity.Font = New System.Drawing.Font("Segoe UI", 9.5F)
        Me.txtQuantity.Location = New System.Drawing.Point(2, 2)
        Me.txtQuantity.Name = "txtQuantity"
        Me.txtQuantity.Size = New System.Drawing.Size(69, 19)
        Me.txtQuantity.TabIndex = 4
        '
        'lblPrice
        '
        Me.lblPrice.AutoSize = True
        Me.lblPrice.Font = New System.Drawing.Font("Segoe UI", 9.5F)
        Me.lblPrice.ForeColor = System.Drawing.Color.FromArgb(86, 96, 110)
        Me.lblPrice.Location = New System.Drawing.Point(416, 39)
        Me.lblPrice.Name = "lblPrice"
        Me.lblPrice.Size = New System.Drawing.Size(36, 17)
        Me.lblPrice.TabIndex = 2
        Me.lblPrice.Text = "Price"
        '
        'pnlPrice (visible field, see pnlId)
        '
        Me.pnlPrice.Controls.Add(Me.txtPrice)
        Me.pnlPrice.Location = New System.Drawing.Point(454, 37)
        Me.pnlPrice.Name = "pnlPrice"
        Me.pnlPrice.Size = New System.Drawing.Size(86, 28)
        Me.pnlPrice.TabIndex = 5
        '
        'txtPrice
        '
        Me.txtPrice.Font = New System.Drawing.Font("Segoe UI", 9.5F)
        Me.txtPrice.Location = New System.Drawing.Point(2, 2)
        Me.txtPrice.Name = "txtPrice"
        Me.txtPrice.Size = New System.Drawing.Size(82, 19)
        Me.txtPrice.TabIndex = 5
        '
        'btnAppend
        '
        Me.btnAppend.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnAppend.BackColor = System.Drawing.Color.FromArgb(0, 120, 215)
        Me.btnAppend.Cursor = System.Windows.Forms.Cursors.Hand
        Me.btnAppend.FlatAppearance.BorderSize = 0
        Me.btnAppend.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(28, 140, 240)
        Me.btnAppend.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnAppend.Font = New System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold)
        Me.btnAppend.ForeColor = System.Drawing.Color.White
        Me.btnAppend.Location = New System.Drawing.Point(629, 33)
        Me.btnAppend.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.btnAppend.Name = "btnAppend"
        Me.btnAppend.Size = New System.Drawing.Size(103, 28)
        Me.btnAppend.TabIndex = 6
        Me.btnAppend.Text = "Append Row"
        Me.btnAppend.UseVisualStyleBackColor = False
        '
        'btnCancel
        '
        Me.btnCancel.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnCancel.Location = New System.Drawing.Point(549, 33)
        Me.btnCancel.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.Size = New System.Drawing.Size(72, 28)
        Me.btnCancel.TabIndex = 10
        Me.btnCancel.Text = "Cancel"
        Me.btnCancel.Visible = False
        '
        'btnSave
        '
        Me.btnSave.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnSave.BackColor = System.Drawing.Color.FromArgb(16, 124, 65)
        Me.btnSave.Cursor = System.Windows.Forms.Cursors.Hand
        Me.btnSave.FlatAppearance.BorderSize = 0
        Me.btnSave.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(21, 146, 82)
        Me.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnSave.Font = New System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold)
        Me.btnSave.ForeColor = System.Drawing.Color.White
        Me.btnSave.Location = New System.Drawing.Point(629, 457)
        Me.btnSave.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.btnSave.Name = "btnSave"
        Me.btnSave.Size = New System.Drawing.Size(129, 31)
        Me.btnSave.TabIndex = 7
        Me.btnSave.Text = "Save Changes"
        Me.btnSave.UseVisualStyleBackColor = False
        '
        'lblStatus
        '
        Me.lblStatus.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right, System.Windows.Forms.AnchorStyles)
        Me.lblStatus.AutoEllipsis = True
        Me.lblStatus.ForeColor = System.Drawing.Color.FromArgb(86, 96, 110)
        Me.lblStatus.Location = New System.Drawing.Point(14, 466)
        Me.lblStatus.Name = "lblStatus"
        Me.lblStatus.Size = New System.Drawing.Size(605, 14)
        Me.lblStatus.TabIndex = 9
        Me.lblStatus.Text = "Ready."
        '
        'lblSearch
        '
        Me.lblSearch.AutoSize = True
        Me.lblSearch.ForeColor = System.Drawing.Color.FromArgb(86, 96, 110)
        Me.lblSearch.Location = New System.Drawing.Point(14, 63)
        Me.lblSearch.Name = "lblSearch"
        Me.lblSearch.Size = New System.Drawing.Size(104, 13)
        Me.lblSearch.TabIndex = 11
        Me.lblSearch.Text = "Search (ID or Name)"
        '
        'pnlSearch (visible field, see pnlId)
        '
        Me.pnlSearch.Controls.Add(Me.txtSearch)
        Me.pnlSearch.Location = New System.Drawing.Point(117, 60)
        Me.pnlSearch.Name = "pnlSearch"
        Me.pnlSearch.Size = New System.Drawing.Size(215, 28)
        Me.pnlSearch.TabIndex = 2
        '
        'txtSearch
        '
        Me.txtSearch.Location = New System.Drawing.Point(2, 1)
        Me.txtSearch.Name = "txtSearch"
        Me.txtSearch.Size = New System.Drawing.Size(211, 18)
        Me.txtSearch.TabIndex = 2
        '
        'btnPrev
        '
        Me.btnPrev.Location = New System.Drawing.Point(14, 320)
        Me.btnPrev.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.btnPrev.Name = "btnPrev"
        Me.btnPrev.Size = New System.Drawing.Size(74, 24)
        Me.btnPrev.TabIndex = 12
        Me.btnPrev.Text = "< Prev"
        '
        'btnNext
        '
        Me.btnNext.Location = New System.Drawing.Point(91, 320)
        Me.btnNext.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.btnNext.Name = "btnNext"
        Me.btnNext.Size = New System.Drawing.Size(74, 24)
        Me.btnNext.TabIndex = 13
        Me.btnNext.Text = "Next >"
        '
        'lblPageInfo
        '
        Me.lblPageInfo.AutoSize = True
        Me.lblPageInfo.Location = New System.Drawing.Point(171, 327)
        Me.lblPageInfo.Name = "lblPageInfo"
        Me.lblPageInfo.Size = New System.Drawing.Size(108, 13)
        Me.lblPageInfo.TabIndex = 14
        Me.lblPageInfo.Text = "Page 1 of 1 - 0 row(s)"
        '
        'lblPageSize
        '
        Me.lblPageSize.AutoSize = True
        Me.lblPageSize.Location = New System.Drawing.Point(369, 327)
        Me.lblPageSize.Name = "lblPageSize"
        Me.lblPageSize.Size = New System.Drawing.Size(79, 13)
        Me.lblPageSize.TabIndex = 15
        Me.lblPageSize.Text = "Rows per page"
        '
        'cmbPageSize
        '
        Me.cmbPageSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbPageSize.Items.AddRange(New Object() {"2", "5", "10", "25", "50", "100"})
        Me.cmbPageSize.Location = New System.Drawing.Point(454, 320)
        Me.cmbPageSize.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.cmbPageSize.Name = "cmbPageSize"
        Me.cmbPageSize.Size = New System.Drawing.Size(61, 21)
        Me.cmbPageSize.TabIndex = 16
        '
        'MainForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0F, 13.0F)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.White
        Me.ClientSize = New System.Drawing.Size(771, 504)
        Me.Controls.Add(Me.lblStatus)
        Me.Controls.Add(Me.btnSave)
        Me.Controls.Add(Me.grpNewProduct)
        Me.Controls.Add(Me.dataGridViewProducts)
        Me.Controls.Add(Me.pnlSearch)
        Me.Controls.Add(Me.lblSearch)
        Me.Controls.Add(Me.cmbPageSize)
        Me.Controls.Add(Me.lblPageSize)
        Me.Controls.Add(Me.btnTheme)
        Me.Controls.Add(Me.lblPageInfo)
        Me.Controls.Add(Me.btnNext)
        Me.Controls.Add(Me.btnPrev)
        Me.Controls.Add(Me.btnLoad)
        Me.Controls.Add(Me.pnlId)
        Me.Controls.Add(Me.lblSpreadsheetId)
        Me.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.MinimumSize = New System.Drawing.Size(787, 479)
        Me.Name = "MainForm"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Google Sheets Demo"
        CType(Me.dataGridViewProducts, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpNewProduct.ResumeLayout(False)
        Me.grpNewProduct.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents lblSpreadsheetId As System.Windows.Forms.Label
    Friend WithEvents pnlId As System.Windows.Forms.Panel
    Friend WithEvents txtSpreadsheetId As System.Windows.Forms.TextBox
    Friend WithEvents btnLoad As System.Windows.Forms.Button
    Friend WithEvents dataGridViewProducts As System.Windows.Forms.DataGridView
    Friend WithEvents colId As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colName As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colQuantity As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colPrice As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colEdit As System.Windows.Forms.DataGridViewButtonColumn
    Friend WithEvents colDelete As System.Windows.Forms.DataGridViewButtonColumn
    Friend WithEvents grpNewProduct As System.Windows.Forms.GroupBox
    Friend WithEvents pnlName As System.Windows.Forms.Panel
    Friend WithEvents lblName As System.Windows.Forms.Label
    Friend WithEvents txtName As System.Windows.Forms.TextBox
    Friend WithEvents pnlQuantity As System.Windows.Forms.Panel
    Friend WithEvents lblQuantity As System.Windows.Forms.Label
    Friend WithEvents txtQuantity As System.Windows.Forms.TextBox
    Friend WithEvents pnlPrice As System.Windows.Forms.Panel
    Friend WithEvents lblPrice As System.Windows.Forms.Label
    Friend WithEvents txtPrice As System.Windows.Forms.TextBox
    Friend WithEvents btnAppend As System.Windows.Forms.Button
    Friend WithEvents btnSave As System.Windows.Forms.Button
    Friend WithEvents lblStatus As System.Windows.Forms.Label
    Friend WithEvents lblSearch As System.Windows.Forms.Label
    Friend WithEvents pnlSearch As System.Windows.Forms.Panel
    Friend WithEvents txtSearch As System.Windows.Forms.TextBox
    Friend WithEvents btnCancel As System.Windows.Forms.Button
    Friend WithEvents btnPrev As System.Windows.Forms.Button
    Friend WithEvents btnNext As System.Windows.Forms.Button
    Friend WithEvents lblPageInfo As System.Windows.Forms.Label
    Friend WithEvents btnTheme As System.Windows.Forms.Button
    Friend WithEvents lblPageSize As System.Windows.Forms.Label
    Friend WithEvents cmbPageSize As System.Windows.Forms.ComboBox

End Class
