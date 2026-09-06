# สถาปัตยกรรมและการทำงานภายใน (แบบละเอียด)

เอกสารนี้อธิบายทุกชั้นของแอป การไหลของข้อมูลทีละปุ่ม ช่วง range ที่ยิงจริง กฎการ parse
และเหตุผลที่ unit test ทำงานได้โดยไม่ต้องต่อ Google — อ้างอิงไฟล์/บรรทัดจริงใน repo

---

## ภาพรวมชั้นต่อชั้น

```
┌─────────────────────────────────────────────────────────────────┐
│ MainForm.cs (UI Layer)                                          │
│  - อ่าน App.config มา prefill ช่อง Spreadsheet ID                │
│  - ส่ง input ให้ ProductValidator ตรวจก่อนยิง API ทุกครั้ง         │
│  - error: status bar + dialog พร้อม icon                           │
│    (Warning = validation, Error = runtime เช่น 403/404)         │
│  - ปุ่มทุกปุ่มเป็น async void + try/catch                          │
│  - สร้าง GoogleSheetService ใหม่ทุกครั้งที่กดปุ่ม (using → Dispose)│
└──────────────┬──────────────────────────────────────────────────┘
               │ เรียกผ่าน
┌──────────────▼──────────────────────────────────────────────────┐
│ ProductRepository.cs (Business Logic)                           │
│  - GetAllAsync / AddAsync / SaveAllAsync                         │
│  - รัน ID, เขียนหัวตารางเมื่อชีตว่าง, ตัดสินเคลียร์หรือเขียนทับ        │
│  - ไม่รู้จัก Google เลย — รู้จักแค่ ISheetService                 │
└──────────────┬──────────────────────────────────────────────────┘
               │ เรียกผ่าน
┌──────────────▼──────────────────────────────────────────────────┐
│ ISheetService.cs (Abstraction — 4 เมธอด)                         │
│  GetValuesAsync / UpdateValuesAsync / AppendValuesAsync /        │
│  ClearValuesAsync                                                │
└───────┬──────────────────────────────────┬──────────────────────┘
        │ ตอนรันจริง                        │ ตอนเทส
┌───────▼──────────────────────┐  ┌───────▼─────────────────────┐
│ GoogleSheetService.cs        │  │ FakeSheetService.cs         │
│  - โหลด JSON key → Google-   │  │  - คืนแถวที่เทสส่งมาให้       │
│    Credential → scoped       │  │  - จด range/value ทุก call  │
│    spreadsheets              │  │    ไว้ใน List สาธารณะ        │
│  - SheetsService (HTTP)      │  │  - เทส assert จาก List นี้   │
└──────────────────────────────┘  └─────────────────────────────┘
```

เหตุผลของการแบ่ง (แนว clean architecture — dependency ชี้เข้าในเท่านั้น):
- **ชั้นในไม่รู้จักชั้นนอก** — `ProductRepository`, `ProductValidator`, `SheetRowMapper`
  (โค้ดที่มี logic มากที่สุด) ไม่มี dependency ต่อ Google SDK หรือ Windows Forms เลย — เทสได้ 100% แบบ in-memory
- **UI เป็นแค่ผู้นำเสนอ** — validator คืน `ValidationResult` เป็นข้อมูลล้วน ส่วน MainForm เป็นคนตัดสิน
  ว่าจะแสดง dialog แบบไหน (⚠/✖) ถ้าอยากเปลี่ยน MessageBox เป็น inline error แก้แค่ไฟล์เดียว
- **อินฟราเปลี่ยนได้** — เปลี่ยนที่เก็บข้อมูลจาก Google Sheets = เขียน `ISheetService` ตัวใหม่ไฟล์เดียว
  โลจิก/เทสไม่ต้องแตะเลย

## ไฟล์และความรับผิดชอบ

| ไฟล์ | ความรับผิดชอบ | จุดที่น่ารู้ |
| --- | --- | --- |
| `Program.cs` | จุดเริ่มแอป | เปิด `Tls12` ก่อนขึ้น UI เพราะ endpoint ของ Google บังคับ TLS 1.2 (แก้ปัญหา Windows เก่า) |
| `MainForm.cs` | หน้าจอ + event ทุกปุ่ม | `_products` เป็น `BindingList<Product>` bind ตรงเข้า DataGridView แก้เซลล์ = แก้ใน list |
| `Services/ISheetService.cs` | interface 4 เมธอด | จุดเยื้อง (seam) ที่ทำให้เทสได้ |
| `Services/GoogleSheetService.cs` | ตัวจริงคุยกับ Google | `IDisposable` — ปิด `SheetsService` ทุกครั้งที่จบปุ่ม |
| `Services/ProductRepository.cs` | โลจิกทั้งหมด | ค่า `HeaderCells = { "ID", "Name", "Quantity", "Price" }` อยู่ที่นี่ |
| `Services/ProductFilter.cs` | กฎค้นหาของกริด | static class: ID ตรงเป๊ะ หรือชื่อมีคำ (OrdinalIgnoreCase) |
| `Services/ProductPager.cs` | คณิตแบ่งหน้าของกริด | static class: `TotalPages` / `ClampPage` / `Slice` — กริดแสดงแค่หน้าเดียว แต่ Save/Edit/Delete ยังใช้ข้อมูลเต็ม |
| `Services/SheetRowMapper.cs` | แปลงแถว ↔ Product | static class ไม่มี state |
| `Validation/ProductValidator.cs` | กฎ input ทั้งหมดของแอป | static class ไม่มี UI type — `ParseAppendInput` (ฟอร์ม) กับ `ValidateForSave` (กริดก่อนเขียน) |
| `Validation/ValidationResult.cs` | กอง failures ของการตรวจหนึ่งรอบ | `ToDialogText()` จัดข้อความรวมเป็นบูลเล็ตพร้อมโชว์ใน dialog |
| `Validation/ValidationFailure.cs` | 1 จุดที่พัง | `Field` = ชื่อช่อง หรือ "Row N" |
| `Validation/ProductInputResult.cs` | ผล parse ฟอร์ม append | มีทั้ง failures และค่าที่ parse ผ่านแล้ว (Name/Quantity/Price) |
| `Models/Product.cs` | โมเดล 1 แถว | `Price` เป็น `decimal` (เหมาะกับเงิน) ที่เหลือ `int`/`string` |
| `App.config` | ค่าตั้ง 3 ตัว | `SpreadsheetId`, `GoogleCredentialsPath`, `SheetName` |

## Data Flow ทีละปุ่ม

### กด Load

```
btnLoad_Click (MainForm.cs:39)
 ├─ CreateSheetService()            เช็กไฟล์ credentials + ช่อง ID ว่างหรือไม่
 ├─ new ProductRepository(service, SheetName)
 └─ repository.GetAllAsync()
     ├─ GET  "ชีต1!A2:D"            <- อ่านตั้งแต่แถว 2 (ข้ามหัวตาราง)
     └─ SheetRowMapper.ToProducts()  กรองแถวเพี้ยนทิ้ง (ดูกฎด้านล่าง)
 └─ ReplaceGridItems()               เคลียร์กริด ใส่ของใหม่ทั้งหมด
 └─ status: "Loaded N row(s)."
```

- error ใด ๆ → status แดง `Load failed: <ข้อความ>` **พร้อม dialog (icon ✖ Error)** แสดงข้อความ exception ชั้นนอกสุด

### กด Append Row

```
btnAppend_Click (MainForm.cs:58)
 ├─ ProductValidator.ParseAppendInput(name, qtyText, priceText)
 │   ├─ Name: บังคับกรอก, trim, ≤ 100 ตัวอักษร
 │   ├─ Quantity: บังคับกรอก, ต้องเป็นเลขจำนวนเต็ม, 0 - 1,000,000,000
 │   ├─ Price: บังคับกรอก, ต้องเป็นตัวเลข, 0 - 99,999,999.99
 │   └─ พังจุดไหนรายงานครบทุกจุดในรอบเดียว (ไม่หยุดที่ failure แรก)
 ├─ ไม่ผ่าน → dialog ⚠ "Cannot append row" + รายการทุกจุด → หยุด (ไม่ยิง API)
 └─ repository.AddAsync(product)  (ProductRepository.cs:43)
     ├─ GetAllAsync()                 อ่านของเดิมมาก่อน "ทุกครั้ง"
     ├─ nextId = แถวว่าง ? 1 : max(ID)+1
     ├─ ถ้าชีตว่าง → UPDATE "ชีต1!A1" ด้วยหัวตาราง ID|Name|Quantity|Price ก่อน 1 แถว
     └─ APPEND "ชีต1!A:D"  1 แถวใหม่ (Google จะวางต่อจากแถวสุดท้ายที่มีข้อมูล)
 └─ เคลียร์ฟอร์ม, status: "Appended row with ID N."
 └─ GetAllAsync() ซ้ำเพื่อรีเฟรชกริดอัตโนมัติ
```

หมายเหตุ: ID ที่ได้คือ **max + 1 ของแถวที่ parse ได้เท่านั้น** — ถ้าชีตมีแถว ID เพี้ยนถูกข้าม
ID เหล่านั้นไม่ถูกนับ อาจได้ ID ซ้ำกับแถวเพี้ยนในชีต (พฤติกรรมยอมรับไว้ของ demo)

### กด Save Changes

```
btnSave_Click (MainForm.cs:97)
 ├─ dataGridView.EndEdit()           ปิดการแก้เซลล์ที่ค้างอยู่ให้ commit ลง list ก่อน
 ├─ snapshot _products เป็น List
 ├─ ProductValidator.ValidateForSave(products)
 │   ├─ ทุกแถว: ID ≥ 1, ชื่อไม่ว่าง (≤100), Quantity/Price ในช่วงเดียวกับฟอร์ม
 │   └─ พัง → dialog ⚠ "Cannot save changes" ระบุ "Row N: เหตุผล" ทุกแถว → หยุด (ไม่ยิง API)
 └─ repository.SaveAllAsync(products)  (ProductRepository.cs:69)
     ├─ รายการว่าง → CLEAR "ชีต1!A2:D"      (ลบข้อมูลแต่เก็บหัวตารางไว้)
     └─ มีข้อมูล  → UPDATE "ชีต1!A2:D"       เขียนทับทั้งบล็อกด้วยแถวจากกริด
 └─ status: "Saved N row(s)."
```

ผลข้างเคียงที่ควรรู้:
- **ลบแถวในกริด = ลบจากชีตจริง** หลังกด Save (เขียนทับทั้งบล็อก)
- แถวที่แก้ในกริดจน ID ไม่ใช่ตัวเลข/ชื่อว่าง → จะโดน validation กันไว้ตอน Save (dialog "Row N")
- Save ไม่แตะคอลัมน์ E ขึ้นไป และไม่แตะหัวตารางแถว 1

### กด Edit (ไอคอน ✏ ท้ายแถว หรือดับเบิลคลิกแถว) → Update Row

```
CellContentClick (colEdit) / CellDoubleClick
 └─ EnterEditMode(product)          เก็บ reference ของแถวไว้ (_editingProduct)
     ├─ ใส่ค่าเดิมลงฟอร์ม, หัวกล่อง → "Edit product (ID N)"
     └─ ปุ่ม → "Update Row" + โผล่ปุ่ม "Cancel" ข้าง ๆ

btnAppend_Click (โหมดแก้ไข) → UpdateEditingRowAsync
 ├─ ProductValidator.ParseAppendInput  เหมือน append (กฎเดียวกัน)
 │   └─ ไม่ผ่าน → dialog ⚠ "Cannot update row" → หยุด
 ├─ แก้ค่าใน master list → SaveAllProductsAsync  เขียนทับทั้งบล็อกลงชีตทันที
 │   └─ validation ทั้งลิสต์ไม่ผ่าน → dialog ⚠ "Row N..." → หยุด
 └─ รีเฟรชกริด, ออกจากโหมดแก้ไข, status: "Updated ID N and saved M row(s)."

btnCancel_Click → ExitEditMode()  คืนฟอร์มเป็นโหมด append ธรรมดา
```

### กด Delete (ไอคอน 🗑 ท้ายแถว)

```
CellContentClick (colDelete) → DeleteProductAsync(product)
 ├─ dialog ⚠ "Confirm delete" (Yes/No) ระบุชื่อ+ID → No = ยกเลิก
 ├─ _products.Remove(selected)      ถ้าแถวนั้นกำลังถูกแก้ → ออกจากโหมดแก้ไขก่อน
 ├─ SaveAllProductsAsync            เขียนบล็อกที่เหลือลงชีตทันที
 └─ ApplyFilter(), status: "Deleted "ชื่อ" and saved M row(s)."
```

### พิมพ์ในช่อง Search (ID or Name)

```
txtSearch_TextChanged → ApplyFilter()
 ├─ ค้นหาว่าง → DataSource = master list, กริดแก้ได้ตามปกติ
 └─ มีคำค้น → DataSource = BindingList ใหม่จาก ProductFilter.Apply()
     (แถวใน view คือ reference เดียวกับ master — ไม่มีข้อมูลซ้ำ)
     └─ กริดกลายเป็น read-only ระหว่างกรอง กันแก้เซลล์ผ่านมุมมองที่ไม่ครบ
        status: "Filtering: showing X of Y row(s)..."
หมายเหตุ: Save/Edit/Delete ทำงานกับ master list เสมอ ไม่ว่าจะกำลังกรองอยู่หรือไม่
```

### Pagination (แถบ pager ใต้ตาราง)

```
ช่องค้นหาเปลี่ยน      → _page = 1
กด Append สำเร็จ      → _page = int.MaxValue (clamp ไปหน้าสุดท้ายให้เห็นแถวใหม่)
กด Load / เปลี่ยน size → _page = 1
กด Next / Prev        → _page++ / _page-- แล้ว clamp เข้าช่วงเสมอ

ApplyFilter() ทุกครั้ง:
  source = ProductFilter.Apply(_products, query)     ข้อมูลเต็มที่ผ่านค้นหา
  _totalPages = ProductPager.TotalPages(source.Count, _pageSize)
  _page = ProductPager.ClampPage(_page, _totalPages)
  view = ProductPager.Slice(source, _page, _pageSize)  ← reference เดียวกับ master
  DataSource = view; ป้าย "Page X of Y - N row(s)"
```

- Save Changes / Update Row / ลบผ่านไอคอน 🗑 ยังทำงานกับ `_products` (ข้อมูลเต็ม) เสมอ —
  การแบ่งหน้าเป็นแค่ "มุมมอง" ทำให้แก้/ลบ/บันทึกข้ามหน้าได้ถูกต้องโดยไม่ต้องเปิดทุกหน้า
- แถวใน view คือ reference เดียวกับ master จึงแก้เซลล์ตรงในหน้าใดก็ได้ ค่าจะไปอยู่ใน master ทันที
- Prev/Next ปิดเองเมื่ออยู่หน้าแรก/สุดท้าย (รวมอยู่ใน UpdateButtonStates)

### สถานะปุ่ม Enable/Disable (UpdateButtonStates)

จุดเดียวกลางที่ทุก event เรียก (SelectionChanged, ApplyFilter, RefreshGridAfterMutation, SetBusy,
โหลดเสร็จ, constructor) — คำนวณจาก 3 ธง:

| ธง | ทำให้ปุ่มไหนปิด |
| --- | --- |
| `_busy` (request กำลังบิน) | ปุ่มทุกปุ่ม + `txtSearch` — กันยิงซ้ำ (spinner หมุนคู่กัน) |
| (ผู้ใช้เลือกแถวไม่ได้ที่กริดอ่านอย่างเดียว) |
| `_loaded == false` **และ** master ว่าง | `btnSave` — เปิดแอปมาใหม่กริดว่าง กด Save ไม่ได้ เพื่อไม่ให้เคลียร์ชีตจริงโดยไม่ตั้งใจ (พอโหลดหรือ append ครั้งแรกปุ่มเปิด และถ้าเจตนาลบจนหมดหลังโหลดแล้ว Save ยังใช้ได้) |

## ตาราง range ที่แอปใช้จริง

| ปฏิบัติการ | Range ที่ยิง | เมธอด API |
| --- | --- | --- |
| อ่านทั้งหมด | `<แท็บ>!A2:D` | `values.get` |
| เขียนหัวตาราง (ชีตว่าง) | `<แท็บ>!A1` | `values.update` |
| เพิ่มแถว | `<แท็บ>!A:D` | `values.append` (+`ValueInputOption=USERENTERED`, `InsertDataOption=INSERTROWS`) |
| เขียนทับทั้งหมด | `<แท็บ>!A2:D` | `values.update` |
| เคลียร์ทั้งหมด | `<แท็บ>!A2:D` | `values.clear` |

`<แท็บ>` = ค่า `SheetName` จาก App.config (ปัจจุบัน `ชีต1`) — ถ้าค่าว่าง/มีแต่ช่องว่าง
`ProductRepository` จะ fallback เป็น `Sheet1` (MainForm ก็ fallback เหมือนกัน)

## กฎการ parse ของ SheetRowMapper

อ่าน (แถวดิบ → Product) — ทำใน `SheetRowMapper.ToProduct`:

| กรณี | ผลลัพธ์ |
| --- | --- |
| แถว `null` หรือทุกเซลล์ว่าง/ช่องว่าง | ข้ามทั้งแถว |
| แถวสั้นกว่า 4 เซลล์ | เติมเซลล์ว่างจนครบ (missing = 0) |
| คอลัมน์ ID parse เป็น int ไม่ได้ (รวมว่าง) | ข้ามทั้งแถว |
| คอลัมน์ Name ว่าง (หลัง trim) | ข้ามทั้งแถว |
| Quantity/Price parse ไม่ได้ | เป็น 0 แต่**ยังเอาแถวมาแสดง** |
| ตัวเลข parse: ลอง **InvariantCulture ก่อน** แล้วค่อย **CurrentCulture** | `199.5` และ `199,5` (locale ไทย/ยุโรป) รอดทั้งคู่ |

เขียน (Product → แถวดิบ) — `SheetRowMapper.ToRow`:

- ทุกตัวเลข format ด้วย **`CultureInfo.InvariantCulture`** เสมอ → ชีตจะเห็น `1234.5` ไม่ว่าเครื่องจะตั้ง locale อะไร
- `Name` เป็น null → เขียนเป็นสตริงว่าง
- ปุ่ม Append ฝั่งฟอร์มใช้กฎ parse เดียวกัน (`ParseIntOrDefault` / `ParseDecimalOrDefault` ใน MainForm.cs:176-202)

## ลำดับการค้นหาไฟล์ credentials

`MainForm.ResolveCredentialsPath` (MainForm.cs:137) — ถ้าค่าใน App.config เป็น relative path (default: `credentials.json`):

1. `bin\Debug\credentials.json` (โฟลเดอร์ exe — csproj set **Copy to Output Directory** ไว้แล้ว)
2. โฟลเดอร์ current working directory
3. `src\GoogleSheetsDemo\credentials.json` (bin\Debug → ขึ้น 2 ชั้น) — ใช้ตรง ๆ จาก source tree ได้เลย

ถ้า path ใน config เป็น absolute ใช้ค่านั้นอย่างเดียว ไม่ค้น 3 ตำแหน่ง

## การยืนยันตัวตน (GoogleSheetService)

```
credentials.json (JSON key ของ service account)
   └─ GoogleCredential.FromStream()          อ่าน private key + client_email
        └─ .CreateScoped(Spreadsheets)        ขอสิทธิ์เฉพาะ https://www.googleapis.com/auth/spreadsheets
             └─ SheetsService(BaseClientService.Initializer)
                  └─ ทุก request แนบ JWT แลก access token อัตโนมัติ (client library ต่ออายุให้เอง)
```

- ไม่มีขั้นตอน login มนุษย์ — ความน่าเชื่อถือมาจาก key ไฟล์เดียว
- สิทธิ์จริง = สิทธิ์บนไฟล์ที่ถูกแชร์ให้อีเมล `client_email` เท่านั้น (การเมืองระดับไฟล์ ไม่ใช่ระดับโดเมน)
- error จาก Google จะถูกโยนเป็น `Google.GoogleApiException` — ข้อความสั้นที่เห็นใน status bar คือ `ex.Message`
  ถ้าอยากเห็น HTTP body จริง ให้ใช้ `tools\ConnectTest.exe` ([docs/04-connect-test.md](04-connect-test.md))

## Unit Test (58 เคส)

โครงสร้างการเทส: เทสสร้าง `FakeSheetService` (ใส่แถวจำลองได้) → ป้อนให้ `ProductRepository` /
เรียก `SheetRowMapper`, `ProductValidator`, `ProductFilter` หรือ `ProductPager` ตรง ๆ → assert จาก List
ที่ fake จดไว้ (`GetRanges`, `UpdateRanges`, `UpdatedValues`, `AppendRanges`, `AppendedValues`,
`ClearedRanges`) หรือจาก `ValidationResult` / ผลกรอง / ผลแบ่งหน้าที่ได้กลับมา

### ProductRepositoryTests (6 เคส) — โลจิกและ range

| เคส | พิสูจน์ว่า |
| --- | --- |
| `GetAllAsync_ReadsDataRangeAndMapsRows` | อ่านจาก `Sheet1!A2:D` และ map แถวถูกลำดับ |
| `GetAllAsync_WhenSheetHasNoValues_ReturnsEmptyList` | ชีตว่าง (API คืน null) ไม่ crash |
| `AddAsync_WithExistingRows_AppendsRowWithNextId` | ID = max+1 (มี ID 3 → ใหม่ได้ 4) append ที่ `A:D` เซลล์ตรง format |
| `AddAsync_OnEmptySheet_WritesHeaderThenFirstRow` | ชีตว่าง → เขียนหัวตารางที่ `A1` ก่อน แล้วค่อย append ID 1 |
| `SaveAllAsync_WithProducts_UpdatesDataRange` | เขียนทับ `A2:D` เลขเป็น invariant (`2.25`) |
| `SaveAllAsync_WithNoProducts_ClearsDataRange` | กริดว่าง → `values.clear` ที่ `A2:D` ไม่ยิง update |

### SheetRowMapperTests (10 เคส) — กฎ parsing

| เคส | พิสูจน์ว่า |
| --- | --- |
| `ToProducts_WithNullRows_ReturnsEmptyList` | API คืน null → ได้ list ว่าง ไม่ crash |
| `ToProducts_WithValidRow_ParsesAllFields` | 4 ฟิลด์ parse ถูก type |
| `ToProducts_WithUnparseableId_SkipsRow` | ID `abc` → ข้ามทั้งแถว |
| `ToProducts_WithBlankName_SkipsRow` | ชื่อเป็นช่องว่าง → ข้ามทั้งแถว |
| `ToProducts_WithBlankRow_SkipsRow` | แถว null ทั้งแถว → ข้าม |
| `ToProducts_WithShortRow_TreatsMissingCellsAsZero` | แถวมี 2 เซลล์ → Quantity/Price = 0 |
| `ToProducts_WithNonNumericQuantity_DefaultsToZero` | Quantity `n/a` → 0 แต่แถวยังอยู่ |
| `ToProducts_TrimsWhitespaceAroundName` | `"  Mouse  "` → `"Mouse"` |
| `ToRow_WritesCellsInInvariantCulture` | `1234.5` เขียนเป็น `"1234.5"` เสมอ |
| `ToRow_ThenToProduct_RoundTripsAllFields` | เขียนแล้วอ่านกลับ = ค่าเดิมทุกฟิลด์ |

### ProductValidatorTests (17 เคส) — กฎ input ทั้งฟอร์มและตาราง

| เคส | พิสูจน์ว่า |
| --- | --- |
| `ParseAppendInput_WithValidInput_ReturnsParsedValues` | ของถูกผ่านครบ ค่า parse ตรง |
| `ParseAppendInput_TrimsWhitespaceAroundName` | เว้นวรรคหัวท้ายถูกตัด |
| `ParseAppendInput_WithBlankName_FailsWithRequiredMessage` | ชื่อว่าง → failure ช่อง Name บอกว่า required |
| `ParseAppendInput_WithNameLongerThanMax_FailsWithMaxLengthMessage` | ชื่อยากเกิน 100 → ถูกปฏิเสธ |
| `ParseAppendInput_WithEmptyQuantity_FailsAsRequired` | จำนวนว่าง = ผิด (ไม่เดาเป็น 0 อีกแล้ว) |
| `ParseAppendInput_WithNonNumericQuantity_Fails` | `ten` → ผิด |
| `ParseAppendInput_WithNegativeQuantity_FailsWithRangeMessage` | ติดลบ → ผิด พร้อมข้อความช่วง |
| `ParseAppendInput_WithThousandsSeparator_ParsesNumber` | `1,500` → 1500 |
| `ParseAppendInput_WithEmptyPrice_FailsAsRequired` | ราคาว่าง = ผิด |
| `ParseAppendInput_WithNegativePrice_FailsWithRangeMessage` | ราคาติดลบ → ผิด |
| `ParseAppendInput_WithEveryFieldBroken_ReportsAllFailuresAtOnce` | พัง 3 ช่อง → รายงานครบ 3 ในรอบเดียว |
| `ValidateForSave_WithValidRows_ReturnsValid` | แถวปกติผ่าน |
| `ValidateForSave_WithEmptyList_ReturnsValid` | กริดว่าง (จะ clear) = ผ่าน |
| `ValidateForSave_WithBlankName_ReportsGridRowNumber` | รายงานเป็น "Row 2" ชี้แถวตรง ๆ |
| `ValidateForSave_WithInvalidId_ReportsGridRowNumber` | ID 0 → "Row 1: ID must be 1 or greater." |
| `ValidateForSave_WithOutOfRangePrice_ReportsRange` | ราคาเกินเพดาน → ผิด |
| `ValidateForSave_WithSeveralBrokenRows_ListsEachRow` | หลายแถวพัง → รายงานครบทุกจุด ทุกแถว |

### ProductFilterTests (11 เคส) — กฎค้นหา

| เคส | พิสูจน์ว่า |
| --- | --- |
| `Matches_WithEmptyQuery_ReturnsTrue` / `WithWhitespaceQuery` | ช่องค้นหาว่าง = แสดงทุกแถว |
| `Matches_ByName_IsCaseInsensitive` | ค้นชื่อไม่สนตัวพิมพ์ (`KEY` เจอ `Keyboard`) |
| `Matches_ByNameSubstring_NotFound_ReturnsFalse` | ไม่มีคำในชื่อ = ไม่ตรง |
| `Matches_ByIdExact_ReturnsTrue` | พิมพ์เลข = จับคู่ ID ตรงเป๊ะ |
| `Matches_ByIdPartialDigit_DoesNotMatchOtherIds` | `2` ไม่จับ ID 20 (กันผลค้นหาเพี้ยน) |
| `Matches_TrimsQueryBeforeComparing` | เว้นวรรคหน้าหลังถูกตัด |
| `Matches_WithNullProduct_ReturnsFalse` | แถว null ไม่หลุดเข้าผลลัพธ์ |
| `Apply_KeepsOnlyMatches_PreservesOrder` | กรองแล้วยังเรียงตามลำดับเดิม |
| `Apply_WithBlankQuery_ReturnsEverything` / `WithNullList` | ขอบกรณีทั้งสอง |

### ProductPagerTests (14 เคส) — คณิตแบ่งหน้า

| เคส | พิสูจน์ว่า |
| --- | --- |
| `TotalPages` (5 เคส) | 0 แถว = 1 หน้า, หารลงตัว = 2, เศษ 25/10 = 3, แถวเดียว = 1, pageSize ไม่ถูกต้อง = 1 |
| `ClampPage` (3 เคส) | ต่ำกว่า 1 → 1, เกินช่วง → หน้าสุดท้าย, อยู่ในช่วง → คงเดิม |
| `Slice` (6 เคส) | หน้าแรก/หน้ากลางครบช่วง, หน้าสุดท้ายเศษบางส่วน, เกินช่วง = ว่าง, list null / pageSize ไม่ถูกต้อง = ว่าง |

### ข้อจำกัดของเทสชุดนี้

- `MainForm` ไม่มีเทส (UI layer) — ตามดีไซน์ โค้ดที่มี logic ถูกดันลง Repository/Mapper หมดแล้ว
- ไม่มีเทสต่อ `GoogleSheetService` ตัวจริง (ต้อง network) — ใช้ `tools\ConnectTest.exe` ทดสอบแทนแบบ manual

## ทำไมต้อง pin Google.Apis 1.55

เครื่องพัฒนาเครื่องนี้มีแค่ Visual Studio 2017 Community และ .NET targeting pack ถึงแค่ 4.6.1
Google.Apis เวอร์ชันใหม่กว่าต้องการ build tools/targeting pack ที่เครื่องไม่มี จึงล็อกที่ 1.55.0
(ทั้ง `Google.Apis`, `.Auth`, `.Core`, `.Sheets.v4`) — ดูรายละเอียดเครื่องใน [README → สิ่งที่ต้องมี](../README.md#สิ่งที่ต้องมี-prerequisites)
