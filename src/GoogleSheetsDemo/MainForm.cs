using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using GoogleSheetsDemo.Controls;
using GoogleSheetsDemo.Models;
using GoogleSheetsDemo.Services;
using GoogleSheetsDemo.Theme;
using GoogleSheetsDemo.Validation;

namespace GoogleSheetsDemo
{
    /// <summary>
    /// Main window: loads the sheet into a grid, lets the user search, edit
    /// cells or the selected row, delete rows, append new ones and write
    /// changes back to Google Sheets.
    /// All input rules live in ProductValidator and search rules in
    /// ProductFilter (no UI types, unit tested); this class only decides how
    /// results are presented: status bar plus a modal dialog with an icon for
    /// validation problems and runtime errors.
    /// </summary>
    public partial class MainForm : Form
    {
        private readonly BindingList<Product> _products = new BindingList<Product>();
        private LoadingSpinner _spinner;
        private Product _editingProduct;
        private Bitmap _editCellIcon;
        private Bitmap _deleteCellIcon;
        private bool _busy;
        private bool _loaded; // true once the sheet has been loaded at least once
        private int _page = 1;
        private int _pageSize = 10;
        private int _totalPages = 1;
        private bool _viewReady; // false while InitializeComponent is still running
        private ThemePalette _palette = AppTheme.Light;
        private string _lastStatus = "Ready.";
        private bool _lastStatusIsError;
        private bool? _themeOverride; // null = follow the Windows theme
        private bool _isDark;

        public MainForm()
        {
            InitializeComponent();

            txtSpreadsheetId.Text = ConfigurationManager.AppSettings["SpreadsheetId"] ?? "";
            dataGridViewProducts.DataSource = _products;
            dataGridViewProducts.SelectionChanged += dataGridViewProducts_SelectionChanged;
            dataGridViewProducts.CellContentClick += dataGridViewProducts_CellContentClick;
            dataGridViewProducts.CellDoubleClick += dataGridViewProducts_CellDoubleClick;
            dataGridViewProducts.CellPainting += dataGridViewProducts_CellPainting;
            dataGridViewProducts.CellMouseEnter += dataGridViewProducts_CellMouseEnter;
            dataGridViewProducts.CellMouseLeave += dataGridViewProducts_CellMouseLeave;

            try
            {
                // the exe embeds App.ico through the ApplicationIcon project property
                Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            }
            catch
            {
                // embedded icon unavailable - keep the default form icon
            }

            ApplyModernStyle();
            UpdateButtonStates();
            _viewReady = true;

            // the edit form is bottom-anchored: whenever the window resizes it
            // moves, and the pager strip + grid must follow without overlapping
            grpNewProduct.LocationChanged += delegate { RelayoutLowerArea(); };
            grpNewProduct.Resize += delegate { RelayoutLowerArea(); };
            RelayoutLowerArea();
        }

        private string SheetName
        {
            get { return ConfigurationManager.AppSettings["SheetName"] ?? "Sheet1"; }
        }

        private async void btnLoad_Click(object sender, EventArgs e)
        {
            ExitEditMode();
            SetBusy(true, "Loading data from Google Sheets...");
            try
            {
                using (GoogleSheetService service = CreateSheetService())
                {
                    var repository = new ProductRepository(service, SheetName);
                    IList<Product> products = await repository.GetAllAsync();
                    ReplaceGridItems(products);
                    _page = 1;
                    ApplyFilter();
                    _loaded = true;
                    SetBusy(false, string.Format("Loaded {0} row(s).", products.Count));
                }
            }
            catch (Exception ex)
            {
                SetBusy(false, null);
                ShowErrorDialog("Load", ex);
            }
        }

        private async void btnAppend_Click(object sender, EventArgs e)
        {
            if (_editingProduct != null)
            {
                await UpdateEditingRowAsync();
                return;
            }

            ProductInputResult input = ProductValidator.ParseAppendInput(
                txtName.Text, txtQuantity.Text, txtPrice.Text);

            if (!input.IsValid)
            {
                ShowValidationDialog(input.Validation, "Cannot append row");
                SetStatus("Append cancelled: fix the fields shown in the dialog.", true);
                return;
            }

            SetBusy(true, "Appending new row...");
            try
            {
                var product = new Product
                {
                    Name = input.Name,
                    Quantity = input.Quantity,
                    Price = input.Price
                };

                using (GoogleSheetService service = CreateSheetService())
                {
                    var repository = new ProductRepository(service, SheetName);
                    int newId = await repository.AddAsync(product);

                    ClearInputFields();
                    ReplaceGridItems(await repository.GetAllAsync());
                    _page = int.MaxValue; // clamp to the last page so the new row is visible
                    ApplyFilter();
                    SetBusy(false, string.Format("Appended row with ID {0}.", newId));
                }
            }
            catch (Exception ex)
            {
                SetBusy(false, null);
                ShowErrorDialog("Append", ex);
            }
        }

        private async void btnSave_Click(object sender, EventArgs e)
        {
            IList<Product> products = _products.ToList();

            ValidationResult validation = ProductValidator.ValidateForSave(products);
            if (!validation.IsValid)
            {
                ShowValidationDialog(validation, "Cannot save changes");
                SetStatus("Save cancelled: fix the rows shown in the dialog.", true);
                return;
            }

            SetBusy(true, "Saving changes...");
            try
            {
                using (GoogleSheetService service = CreateSheetService())
                {
                    var repository = new ProductRepository(service, SheetName);
                    await repository.SaveAllAsync(products);
                    SetBusy(false, string.Format("Saved {0} row(s).", products.Count));
                }
            }
            catch (Exception ex)
            {
                SetBusy(false, null);
                ShowErrorDialog("Save", ex);
            }
        }

        // --- edit row (double click or the per-row pencil icon) ---

        private void dataGridViewProducts_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            // the grid is read-only; double click opens the edit form instead
            if (e.RowIndex < 0)
            {
                return;
            }
            Product selected = SelectedProduct();
            if (selected != null)
            {
                EnterEditMode(selected);
            }
        }

        /// <summary>
        /// Row-level actions: the Edit / Delete icons at the end of every row.
        /// Works from the filtered view too - the row holds the same product
        /// reference as the master list.
        /// </summary>
        private async void dataGridViewProducts_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || _busy)
            {
                return;
            }

            string columnName = dataGridViewProducts.Columns[e.ColumnIndex].Name;
            Product product = dataGridViewProducts.Rows[e.RowIndex].DataBoundItem as Product;
            if (product == null)
            {
                return;
            }

            if (columnName == "colEdit")
            {
                EnterEditMode(product);
            }
            else if (columnName == "colDelete")
            {
                await DeleteProductAsync(product);
            }
        }

        /// <summary>Draws the pencil / trash icons inside the row action cells.</summary>
        private void dataGridViewProducts_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || _editCellIcon == null || _deleteCellIcon == null)
            {
                return;
            }

            string columnName = dataGridViewProducts.Columns[e.ColumnIndex].Name;
            Bitmap icon = columnName == "colEdit" ? _editCellIcon
                : (columnName == "colDelete" ? _deleteCellIcon : null);
            if (icon == null)
            {
                return;
            }

            e.Handled = true;
            e.PaintBackground(e.ClipBounds, true);

            int size = Math.Min(18, Math.Min(e.CellBounds.Width, e.CellBounds.Height) - 8);
            if (size < 4)
            {
                return;
            }
            int x = e.CellBounds.Left + (e.CellBounds.Width - size) / 2;
            int y = e.CellBounds.Top + (e.CellBounds.Height - size) / 2;
            e.Graphics.DrawImage(icon, x, y, size, size);
        }

        private void dataGridViewProducts_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                string columnName = dataGridViewProducts.Columns[e.ColumnIndex].Name;
                if (columnName == "colEdit" || columnName == "colDelete")
                {
                    dataGridViewProducts.Cursor = Cursors.Hand;
                }
            }
        }

        private void dataGridViewProducts_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            dataGridViewProducts.Cursor = DefaultCursor;
        }

        /// <summary>Applies the form to the row being edited, then persists the whole block.</summary>
        private async System.Threading.Tasks.Task UpdateEditingRowAsync()
        {
            ProductInputResult input = ProductValidator.ParseAppendInput(
                txtName.Text, txtQuantity.Text, txtPrice.Text);

            if (!input.IsValid)
            {
                ShowValidationDialog(input.Validation, "Cannot update row");
                SetStatus("Update cancelled: fix the fields shown in the dialog.", true);
                return;
            }

            SetBusy(true, "Updating row...");
            try
            {
                _editingProduct.Name = input.Name;
                _editingProduct.Quantity = input.Quantity;
                _editingProduct.Price = input.Price;
                int id = _editingProduct.Id;

                int saved = await SaveAllProductsAsync("Update");
                if (saved < 0)
                {
                    SetBusy(false, null);
                    return; // validation dialog was shown
                }
                RefreshGridAfterMutation();
                ExitEditMode();
                SetBusy(false, string.Format("Updated ID {0} and saved {1} row(s).", id, saved));
            }
            catch (Exception ex)
            {
                SetBusy(false, null);
                ShowErrorDialog("Update", ex);
            }
        }

        private void EnterEditMode(Product product)
        {
            _editingProduct = product;
            txtName.Text = product.Name;
            txtQuantity.Text = product.Quantity.ToString(CultureInfo.InvariantCulture);
            txtPrice.Text = product.Price.ToString(CultureInfo.InvariantCulture);

            grpNewProduct.Text = "Edit product (ID " + product.Id + ")";
            btnAppend.Text = "Update Row";
            SetButtonIcon(btnAppend, ButtonIcons.Create(ButtonIconKind.Pencil, _palette.AppendFore));
            btnCancel.Visible = true;
            txtName.Focus();
            SetStatus("Editing ID " + product.Id + " - change the values and press Update Row.");
        }

        private void ExitEditMode()
        {
            _editingProduct = null;
            ClearInputFields();
            grpNewProduct.Text = "Add new product (appends a row to the sheet)";
            btnAppend.Text = "Append Row";
            SetButtonIcon(btnAppend, ButtonIcons.Create(ButtonIconKind.Plus, _palette.AppendFore));
            btnCancel.Visible = false;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            ExitEditMode();
            SetStatus("Edit cancelled.");
        }

        // --- delete row (the per-row trash icon) ---

        /// <summary>Confirm dialog, then removes the row and writes the change to the sheet.</summary>
        private async System.Threading.Tasks.Task DeleteProductAsync(Product selected)
        {
            DialogResult answer = MessageBox.Show(this,
                "Delete \"" + selected.Name + "\" (ID " + selected.Id + ")?\n\n" +
                "The row is removed from the grid and the change is written to Google Sheets immediately.",
                "Confirm delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (answer != DialogResult.Yes)
            {
                SetStatus("Delete cancelled.");
                return;
            }

            SetBusy(true, "Deleting row...");
            try
            {
                if (_editingProduct == selected)
                {
                    ExitEditMode();
                }

                _products.Remove(selected);
                int saved = await SaveAllProductsAsync("Delete");
                if (saved < 0)
                {
                    SetBusy(false, null);
                    return; // validation dialog was shown
                }
                ApplyFilter();
                SetBusy(false, string.Format("Deleted \"{0}\" and saved {1} row(s).", selected.Name, saved));
            }
            catch (Exception ex)
            {
                SetBusy(false, null);
                ShowErrorDialog("Delete", ex);
            }
        }

        /// <summary>
        /// Writes the whole product list to the sheet. Returns the number of
        /// rows saved, or -1 when validation blocked the write (dialog shown).
        /// Callers are responsible for SetBusy around it.
        /// </summary>
        private async System.Threading.Tasks.Task<int> SaveAllProductsAsync(string action)
        {
            IList<Product> products = _products.ToList();

            ValidationResult validation = ProductValidator.ValidateForSave(products);
            if (!validation.IsValid)
            {
                ShowValidationDialog(validation, "Cannot " + action.ToLowerInvariant() + " changes");
                SetStatus(action + " cancelled: fix the rows shown in the dialog.", true);
                return -1;
            }

            using (GoogleSheetService service = CreateSheetService())
            {
                var repository = new ProductRepository(service, SheetName);
                await repository.SaveAllAsync(products);
                return products.Count;
            }
        }

        // --- search / filter ---

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            _page = 1; // a new search always starts from the first page
            ApplyFilter();
        }

        private void btnPrev_Click(object sender, EventArgs e)
        {
            _page--;
            ApplyFilter();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            _page++;
            ApplyFilter();
        }

        private void cmbPageSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            int size;
            if (int.TryParse(Convert.ToString(cmbPageSize.SelectedItem, CultureInfo.InvariantCulture),
                NumberStyles.Integer, CultureInfo.InvariantCulture, out size) && size > 0)
            {
                _pageSize = size;
            }
            _page = 1;
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            if (!_viewReady)
            {
                return; // controls are still being built
            }

            string query = txtSearch.Text;
            bool filtering = query.Trim().Length > 0;

            IList<Product> source = ProductFilter.Apply(_products, query);
            _totalPages = ProductPager.TotalPages(source.Count, _pageSize);
            _page = ProductPager.ClampPage(_page, _totalPages);

            var view = new BindingList<Product>();
            foreach (Product product in ProductPager.Slice(source, _page, _pageSize))
            {
                view.Add(product); // same object references as the master list
            }

            dataGridViewProducts.DataSource = view;

            lblPageInfo.Text = string.Format(CultureInfo.InvariantCulture,
                "Page {0} of {1} - {2} row(s)", _page, _totalPages, source.Count);

            if (filtering)
            {
                SetStatus(string.Format("Filtering: {0} of {1} row(s) match; showing page {2} of {3}.",
                    source.Count, _products.Count, _page, _totalPages));
            }
            UpdateButtonStates();
        }

        // --- helpers ---

        private Product SelectedProduct()
        {
            if (dataGridViewProducts.CurrentRow == null)
            {
                return null;
            }
            return dataGridViewProducts.CurrentRow.DataBoundItem as Product;
        }

        private void RefreshGridAfterMutation()
        {
            ApplyFilter(); // keeps the current page (clamped) and re-renders from the master list
        }

        private void ClearInputFields()
        {
            txtName.Text = "";
            txtQuantity.Text = "";
            txtPrice.Text = "";
        }

        /// <summary>Runtime failure (Google API, missing file, ...): status bar plus an error dialog.</summary>
        private void ShowErrorDialog(string action, Exception ex)
        {
            SetStatus(action + " failed: " + ex.Message, true);
            MessageBox.Show(this,
                action + " failed.\n\n" + ex.Message,
                action + " Failed",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        /// <summary>Invalid user input: warning dialog listing every failure at once.</summary>
        private void ShowValidationDialog(ValidationResult validation, string title)
        {
            MessageBox.Show(this,
                validation.ToDialogText(),
                title,
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        private GoogleSheetService CreateSheetService()
        {
            string configuredPath = ConfigurationManager.AppSettings["GoogleCredentialsPath"] ?? "credentials.json";
            string credentialsPath = ResolveCredentialsPath(configuredPath);
            if (!File.Exists(credentialsPath))
            {
                throw new FileNotFoundException(
                    "Google credentials file not found ('" + configuredPath + "'). " +
                    "Follow the setup steps in README.md to create credentials.json.",
                    credentialsPath);
            }

            string spreadsheetId = txtSpreadsheetId.Text.Trim();
            if (spreadsheetId.Length == 0)
            {
                throw new InvalidOperationException(
                    "Spreadsheet ID is empty. Paste the ID from the sheet URL into the box (or App.config).");
            }

            return new GoogleSheetService(spreadsheetId, credentialsPath, "Google Sheets Demo");
        }

        private static string ResolveCredentialsPath(string configuredPath)
        {
            if (Path.IsPathRooted(configuredPath))
            {
                return configuredPath;
            }

            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // 1) next to the exe (bin\Debug), 2) current folder,
            // 3) the project folder (bin\Debug\..\..) so the file works
            //    straight from the source tree without copying it manually.
            string[] candidates =
            {
                Path.Combine(baseDirectory, configuredPath),
                Path.Combine(Environment.CurrentDirectory, configuredPath),
                Path.GetFullPath(Path.Combine(baseDirectory, "..", "..", configuredPath))
            };

            foreach (string candidate in candidates)
            {
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }

            return candidates[0];
        }

        private void ReplaceGridItems(IEnumerable<Product> products)
        {
            _products.Clear();
            foreach (Product product in products)
            {
                _products.Add(product);
            }
        }

        private void dataGridViewProducts_SelectionChanged(object sender, EventArgs e)
        {
            UpdateButtonStates();
        }

        /// <summary>
        /// Single place that decides which controls are enabled:
        /// - nothing is clickable while a request is in flight (busy);
        /// - Edit/Delete need a selected row (greyed out otherwise);
        /// - Save needs data that came from the sheet or was appended, so an
        ///   untouched empty grid cannot wipe the sheet by accident.
        /// </summary>
        private void UpdateButtonStates()
        {
            btnLoad.Enabled = !_busy;
            txtSearch.Enabled = !_busy;
            btnAppend.Enabled = !_busy;
            btnCancel.Enabled = !_busy;
            btnSave.Enabled = !_busy && (_loaded || _products.Count > 0);
            btnPrev.Enabled = !_busy && _page > 1;
            btnNext.Enabled = !_busy && _page < _totalPages;
            btnTheme.Enabled = !_busy;
            cmbPageSize.Enabled = !_busy;
        }

        private void SetStatus(string message, bool isError = false)
        {
            _lastStatus = message;
            _lastStatusIsError = isError;
            RenderStatus();
        }

        private void RenderStatus()
        {
            lblStatus.ForeColor = _lastStatusIsError ? _palette.StatusError : _palette.StatusText;
            lblStatus.Text = _lastStatus;
        }

        /// <summary>
        /// Turns on the loading state: spinner over the grid, busy flag set and
        /// every control disabled through UpdateButtonStates so a request
        /// cannot be fired twice. Pass null as the message to keep the current
        /// status text.
        /// </summary>
        private void SetBusy(bool busy, string message)
        {
            _busy = busy;

            _spinner.Visible = busy;
            if (busy)
            {
                _spinner.Start();
            }
            else
            {
                _spinner.Stop();
            }

            UpdateButtonStates();

            if (message != null)
            {
                SetStatus(message);
            }
        }

        private void CenterSpinnerOnGrid()
        {
            if (_spinner == null)
            {
                return;
            }

            _spinner.Location = new Point(
                dataGridViewProducts.Left + (dataGridViewProducts.Width - _spinner.Width) / 2,
                dataGridViewProducts.Top + (dataGridViewProducts.Height - _spinner.Height) / 2);
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_SETTINGCHANGE = 0x001A;
            if (m.Msg == WM_SETTINGCHANGE)
            {
                // Windows broadcasts this when personalization changes;
                // "ImmersiveColorSet" identifies a light/dark theme switch.
                string section = Marshal.PtrToStringAuto(m.LParam);
                if (section == "ImmersiveColorSet")
                {
                    ApplyTheme(_themeOverride ?? SystemTheme.IsDarkMode());
                }
            }
            base.WndProc(ref m);
        }

        private void btnTheme_Click(object sender, EventArgs e)
        {
            _themeOverride = !_isDark;
            ApplyTheme(_themeOverride.Value);
            PersistThemeOverride(_themeOverride.Value);
            SetStatus("Switched to the " + (_isDark ? "dark" : "light") + " theme.");
        }

        /// <summary>
        /// null = follow the Windows theme; true/false = manual choice.
        /// The choice lives in a UTF-8 sidecar file next to the exe - never in
        /// the .config, because ConfigurationManager.Save() rewrites the whole
        /// config with the machine's ANSI codepage and corrupts non-ASCII
        /// values (it turned the Thai sheet name into "????1" mojibake).
        /// </summary>
        private static bool? LoadThemeOverride()
        {
            string value = ReadThemeSidecar();
            if (value == null)
            {
                value = ConfigurationManager.AppSettings["Theme"]; // choice made before the sidecar existed
            }
            if (string.Equals(value, "dark", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            if (string.Equals(value, "light", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            return null;
        }

        private static void PersistThemeOverride(bool dark)
        {
            try
            {
                // never write the .config from here: its Thai SheetName would
                // be mangled by the ANSI round-trip of ConfigurationManager
                File.WriteAllText(ThemeSidecarPath(), dark ? "dark" : "light", System.Text.Encoding.UTF8);
            }
            catch
            {
                // file not writable - the toggle still works for this session
            }
        }

        private static string ThemeSidecarPath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GoogleSheetsDemo.theme");
        }

        private static string ReadThemeSidecar()
        {
            try
            {
                string path = ThemeSidecarPath();
                if (!File.Exists(path))
                {
                    return null;
                }
                return File.ReadAllText(path, System.Text.Encoding.UTF8).Trim();
            }
            catch
            {
                return null; // unreadable sidecar = follow the Windows theme
            }
        }

        #region Rounded modern styling

        private void ApplyModernStyle()
        {
            Font = new Font("Segoe UI", 9F);

            SetupInputField(txtSpreadsheetId);
            SetupInputField(txtName);
            SetupInputField(txtQuantity);
            SetupInputField(txtPrice);
            SetupInputField(txtSearch);

            dataGridViewProducts.BorderStyle = BorderStyle.None;
            dataGridViewProducts.EnableHeadersVisualStyles = false;
            dataGridViewProducts.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridViewProducts.ColumnHeadersHeight = 34;
            dataGridViewProducts.RowHeadersVisible = false;
            dataGridViewProducts.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;

            Round(dataGridViewProducts, 12);
            Round(pnlId, 6);
            Round(pnlName, 6);
            Round(pnlQuantity, 6);
            Round(pnlPrice, 6);
            Round(pnlSearch, 6);
            Round(btnLoad, 10);
            Round(btnAppend, 10);
            Round(btnSave, 10);
            Round(btnCancel, 10);
            Round(btnPrev, 10);
            Round(btnNext, 10);
            Round(btnTheme, 10);

            // loading spinner floats over the middle of the grid while a
            // request is in flight
            _spinner = new LoadingSpinner
            {
                Size = new Size(28, 28),
                Visible = false
            };
            Controls.Add(_spinner);
            _spinner.BringToFront();
            CenterSpinnerOnGrid();

            // controls anchored to the window edge change size together with it
            dataGridViewProducts.Resize += delegate
            {
                Round(dataGridViewProducts, 12);
                CenterSpinnerOnGrid();
            };
            pnlId.Resize += delegate
            {
                Round(pnlId, 6);
                CenterInputInField(txtSpreadsheetId);
            };
            pnlSearch.Resize += delegate
            {
                Round(pnlSearch, 6);
                CenterInputInField(txtSearch);
            };

            TryRoundWindowCorners();

            // colors come last so every themed control gets the palette.
            // A manually chosen theme (sun/moon button) wins over the system.
            _themeOverride = LoadThemeOverride();
            ApplyTheme(_themeOverride ?? SystemTheme.IsDarkMode());

            // designed label positions assume design-time font metrics; the
            // runtime font makes auto-sized labels drift off the input's
            // vertical center, so snap every caption to its input afterwards
            AlignLabelsWithInputs();
        }

        /// <summary>Vertically centers each caption with the input next to it.</summary>
        /// <summary>
        /// One baseline pass: every caption, input and button that shares a row
        /// is centered on the same vertical middle, so text lines up even when
        /// control heights differ (25px boxes vs 34px buttons vs 24px combo).
        /// </summary>
        private void AlignLabelsWithInputs()
        {
            // captions centered on their inputs (the panel forms the visible
            // field, so baselines must match the panel, not the inner edit)
            CenterLabelVertically(lblName, pnlName);
            CenterLabelVertically(lblQuantity, pnlQuantity);
            CenterLabelVertically(lblPrice, pnlPrice);
            CenterLabelVertically(lblSearch, pnlSearch);
            CenterLabelVertically(lblPageSize, cmbPageSize);
            CenterLabelVertically(lblPageInfo, cmbPageSize);

            // buttons centered on the row they belong to
            CenterLabelVertically(btnLoad, pnlId);
            CenterLabelVertically(btnTheme, pnlId);
            CenterLabelVertically(btnAppend, pnlPrice);
            CenterLabelVertically(btnCancel, pnlPrice);
            CenterLabelVertically(btnPrev, cmbPageSize);
            CenterLabelVertically(btnNext, cmbPageSize);
        }

        /// <summary>
        /// Keeps the pager strip between the grid and the bottom-anchored edit
        /// form: the strip sits just above the form and the grid ends above the
        /// strip, so resizing the window never stacks them on top of each other.
        /// </summary>
        private void RelayoutLowerArea()
        {
            if (!_viewReady)
            {
                return;
            }

            int pagerTop = grpNewProduct.Top - 44;
            btnPrev.Top = pagerTop;
            btnNext.Top = pagerTop;
            // the combo is shorter than the buttons - center it on their middle,
            // then AlignLabelsWithInputs pulls the two labels onto the same line
            cmbPageSize.Top = pagerTop + (btnPrev.Height - cmbPageSize.Height) / 2;

            dataGridViewProducts.Height = pagerTop - 6 - dataGridViewProducts.Top;

            Round(dataGridViewProducts, 12);
            CenterSpinnerOnGrid();
            AlignLabelsWithInputs();
        }

        private static void CenterLabelVertically(Control label, Control input)
        {
            label.Top = input.Top + (input.Height - label.Height) / 2;
        }

        private static void StyleButton(Button button, Color backColor, Color foreColor,
            Color hoverColor, Color pressedColor)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = hoverColor;
            button.FlatAppearance.MouseDownBackColor = pressedColor;
            button.UseVisualStyleBackColor = false;
            button.BackColor = backColor;
            button.ForeColor = foreColor;
            button.Cursor = Cursors.Hand;
        }

        /// <summary>Colors the per-row Edit / Delete button columns to match the theme.</summary>
        private static void StyleActionColumn(DataGridViewButtonColumn column, Color back, Color fore)
        {
            column.FlatStyle = FlatStyle.Flat;
            column.DefaultCellStyle.BackColor = back;
            column.DefaultCellStyle.ForeColor = fore;
            column.DefaultCellStyle.SelectionBackColor = back;
            column.DefaultCellStyle.SelectionForeColor = fore;
        }

        private void StyleTextBox(TextBox textBox)
        {
            textBox.BackColor = _palette.TextBoxBack;
            textBox.ForeColor = _palette.TextBoxText;
            textBox.Parent.BackColor = _palette.TextBoxBack; // the visible field
        }

        /// <summary>
        /// A single-line borderless edit always paints its text at the very
        /// top of its client area, so a tall edit box can never look
        /// vertically centered. The visible field is therefore the rounded
        /// panel behind the box, and the edit itself is made exactly one
        /// text line tall, centered inside that panel - with no extra space
        /// above the text, nothing can float to the top edge.
        /// </summary>
        private static void SetupInputField(TextBox textBox)
        {
            textBox.AutoSize = false;
            textBox.BorderStyle = BorderStyle.None;
            Control field = textBox.Parent; // rounded panel behind the box
            field.Click += delegate { textBox.Focus(); };
            CenterInputInField(textBox);
        }

        /// <summary>Sizes and places the edit inside its field panel.</summary>
        private static void CenterInputInField(TextBox textBox)
        {
            Control field = textBox.Parent;
            textBox.Height = textBox.Font.Height + 2;
            textBox.Top = (field.Height - textBox.Height) / 2;
            textBox.Left = 2;
            textBox.Width = field.Width - 4;
        }

        private static void Round(Control control, int radius)
        {
            Rectangle bounds = new Rectangle(0, 0, control.Width, control.Height);
            if (bounds.Width < radius * 2 || bounds.Height < radius * 2)
            {
                return;
            }

            var path = new GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, radius, radius, 180, 90);
            path.AddArc(bounds.Right - radius - 1, bounds.Y, radius, radius, 270, 90);
            path.AddArc(bounds.Right - radius - 1, bounds.Bottom - radius - 1, radius, radius, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - radius - 1, radius, radius, 90, 90);
            path.CloseFigure();
            control.Region = new Region(path);
        }

        private void TryRoundWindowCorners()
        {
            try
            {
                // 33 = DWMWA_WINDOW_CORNER_PREFERENCE, 2 = DWMWCP_ROUND.
                // Windows 11 rounds the window with anti-aliasing; older
                // Windows just ignores the attribute.
                int preference = 2;
                DwmSetWindowAttribute(Handle, 33, ref preference, 4);
            }
            catch (DllNotFoundException)
            {
                // dwmapi.dll not available - nothing to do.
            }
        }

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

        /// <summary>
        /// Regenerates the icon in front of every button label from the
        /// active palette. While a row is being edited the append button
        /// shows a pencil instead of a plus.
        /// </summary>
        private void ApplyButtonIcons()
        {
            SetButtonIcon(btnLoad, ButtonIcons.Create(ButtonIconKind.Refresh, _palette.SecondaryFore));
            SetButtonIcon(btnAppend, ButtonIcons.Create(
                _editingProduct != null ? ButtonIconKind.Pencil : ButtonIconKind.Plus,
                _palette.AppendFore));
            SetButtonIcon(btnSave, ButtonIcons.Create(ButtonIconKind.Save, _palette.SaveFore));
            SetButtonIcon(btnCancel, ButtonIcons.Create(ButtonIconKind.Cross, _palette.SecondaryFore));
            // the pager buttons are text-only: their < and > characters ARE
            // the direction marks, an icon in front would duplicate them
            SetButtonIcon(btnTheme, ButtonIcons.Create(
                _isDark ? ButtonIconKind.Sun : ButtonIconKind.Moon,
                _palette.SecondaryFore));

            // same icons, drawn into the per-row Edit / Delete cells
            ReplaceCellIcon(ref _editCellIcon, ButtonIcons.Create(ButtonIconKind.Pencil, _palette.SecondaryFore));
            ReplaceCellIcon(ref _deleteCellIcon, ButtonIcons.Create(ButtonIconKind.Trash, _palette.DeleteFore));
            dataGridViewProducts.Invalidate();
        }

        private static void ReplaceCellIcon(ref Bitmap field, Bitmap icon)
        {
            Bitmap old = field;
            field = icon;
            if (old != null)
            {
                old.Dispose();
            }
        }

        private static void SetButtonIcon(Button button, Bitmap icon)
        {
            Image old = button.Image;
            button.Image = icon;
            button.TextImageRelation = TextImageRelation.ImageBeforeText;
            if (old != null)
            {
                old.Dispose();
            }
        }

        /// <summary>Applies the light or dark palette to every themed control.</summary>
        private void ApplyTheme(bool dark)
        {
            _isDark = dark;
            _palette = dark ? AppTheme.Dark : AppTheme.Light;

            BackColor = _palette.FormBack;

            StyleButton(btnLoad, _palette.SecondaryBack, _palette.SecondaryFore,
                _palette.SecondaryHover, _palette.SecondaryPressed);
            StyleButton(btnCancel, _palette.SecondaryBack, _palette.SecondaryFore,
                _palette.SecondaryHover, _palette.SecondaryPressed);
            StyleButton(btnAppend, _palette.AppendBack, _palette.AppendFore,
                _palette.AppendHover, _palette.AppendPressed);
            StyleButton(btnSave, _palette.SaveBack, _palette.SaveFore,
                _palette.SaveHover, _palette.SavePressed);
            StyleButton(btnPrev, _palette.SecondaryBack, _palette.SecondaryFore,
                _palette.SecondaryHover, _palette.SecondaryPressed);
            StyleButton(btnNext, _palette.SecondaryBack, _palette.SecondaryFore,
                _palette.SecondaryHover, _palette.SecondaryPressed);
            StyleButton(btnTheme, _palette.SecondaryBack, _palette.SecondaryFore,
                _palette.SecondaryHover, _palette.SecondaryPressed);
            StyleActionColumn(colEdit, _palette.SecondaryBack, _palette.SecondaryFore);
            StyleActionColumn(colDelete, _palette.DeleteBack, _palette.DeleteFore);

            ApplyButtonIcons();

            StyleTextBox(txtSpreadsheetId);
            StyleTextBox(txtName);
            StyleTextBox(txtQuantity);
            StyleTextBox(txtPrice);
            StyleTextBox(txtSearch);

            dataGridViewProducts.BackgroundColor = _palette.GridBack;
            dataGridViewProducts.GridColor = _palette.GridLine;
            dataGridViewProducts.ColumnHeadersDefaultCellStyle.BackColor = _palette.HeaderBack;
            dataGridViewProducts.ColumnHeadersDefaultCellStyle.ForeColor = _palette.HeaderText;
            dataGridViewProducts.ColumnHeadersDefaultCellStyle.SelectionBackColor = _palette.HeaderBack;
            dataGridViewProducts.ColumnHeadersDefaultCellStyle.SelectionForeColor = _palette.HeaderText;
            dataGridViewProducts.DefaultCellStyle.BackColor = _palette.CellBack;
            dataGridViewProducts.DefaultCellStyle.ForeColor = _palette.CellText;
            dataGridViewProducts.DefaultCellStyle.SelectionBackColor = _palette.SelectionBack;
            dataGridViewProducts.DefaultCellStyle.SelectionForeColor = _palette.SelectionText;
            dataGridViewProducts.AlternatingRowsDefaultCellStyle.BackColor = _palette.AlternatingRow;
            dataGridViewProducts.RowTemplate.DefaultCellStyle.BackColor = _palette.CellBack;
            dataGridViewProducts.RowTemplate.DefaultCellStyle.ForeColor = _palette.CellText;

            lblSpreadsheetId.ForeColor = _palette.LabelText;
            lblName.ForeColor = _palette.LabelText;
            lblQuantity.ForeColor = _palette.LabelText;
            lblPrice.ForeColor = _palette.LabelText;
            lblSearch.ForeColor = _palette.LabelText;
            lblPageInfo.ForeColor = _palette.LabelText;
            lblPageSize.ForeColor = _palette.LabelText;
            grpNewProduct.ForeColor = _palette.PrimaryText;

            cmbPageSize.BackColor = _palette.TextBoxBack;
            cmbPageSize.ForeColor = _palette.TextBoxText;

            _spinner.SetColors(_palette.SpinnerDisc, _palette.SpinnerTrack, _palette.SpinnerArc);

            TryImmersiveTitleBar(dark);
            RenderStatus();
        }

        private void TryImmersiveTitleBar(bool dark)
        {
            try
            {
                // 20 = DWMWA_USE_IMMERSIVE_DARK_MODE (attribute 19 on early
                // Windows 10 builds) - colors the title bar with the theme.
                int value = dark ? 1 : 0;
                if (DwmSetWindowAttribute(Handle, 20, ref value, 4) != 0)
                {
                    DwmSetWindowAttribute(Handle, 19, ref value, 4);
                }
            }
            catch (DllNotFoundException)
            {
                // dwmapi.dll not available - nothing to do.
            }
        }

        #endregion
    }
}
