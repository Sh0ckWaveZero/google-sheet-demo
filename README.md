# Google Sheets Demo (C# WinForms + Visual Studio Community 2017)

แอป Windows Forms ตัวอย่างสำหรับ **อ่าน / เขียน / แก้ไข / ลบ / ค้นหา Google Sheets** ผ่าน Google Sheets API v4
รันบน .NET Framework 4.6.1 พร้อม unit test (MSTest V2) 58 เคส ที่รันได้จาก Test Explorer ของ VS2017
โดย**ไม่ต้องต่ออินเทอร์เน็ตและไม่ต้องมี credentials** — input ทุกอย่างผ่านชั้น Validation แยกจาก UI
(แนว clean architecture) และ error ทุกประเภทแสดงเป็น dialog พร้อม icon

> สถานะล่าสุด (6 ก.ย. 2026): ทดสอบ end-to-end ผ่านครบทั้ง **Load / Append Row / Save Changes**
> ชีตจริงชื่อ "GoogleSheetsDemo" แชร์กับ service account เป็น Editor เรียบร้อย — ดูบันทึกการแก้ไขท้ายไฟล์

---

## สารบัญ

1. [ความสามารถของแอป](#ความสามารถของแอป)
2. [สถาปัตยกรรม](#สถาปัตยกรรม)
3. [โครงสร้างโปรเจกต์](#โครงสร้างโปรเจกต์)
4. [สิ่งที่ต้องมี (Prerequisites)](#สิ่งที่ต้องมี-prerequisites)
5. [เตรียม Google Cloud ให้ครบทุกขั้น](#เตรียม-google-cloud-ให้ครบทุกขั้น)
6. [ตั้งค่าแอป (App.config)](#ตั้งค่าแอป-appconfig)
7. [Build และรัน](#build-และรัน)
8. [วิธีใช้งานแอปทีละปุ่ม](#วิธีใช้งานแอปทีละปุ่ม)
9. [รูปแบบข้อมูลในชีต](#รูปแบบข้อมูลในชีต)
10. [รัน Unit Test](#รัน-unit-test)
11. [เครื่องมือวินิจฉัยการเชื่อมต่อ](#เครื่องมือวินิจฉัยการเชื่อมต่อ)
12. [แก้ปัญหาที่พบบ่อย](#แก้ปัญหาที่พบบ่อย)
13. [ความปลอดภัย](#ความปลอดภัย)
14. [เอกสารเพิ่มเติมใน docs/](#เอกสารเพิ่มเติม)

---

## ความสามารถของแอป

| ฟีเจอร์ | รายละเอียด |
| --- | --- |
| **Load** | อ่านข้อมูลทั้งหมดจากชีต (ช่วง `<แท็บ>!A2:D`) มาแสดงในตาราง |
| **Append Row** | เพิ่มแถวใหม่ต่อท้ายชีตด้วย `values.append` ระบบรัน ID ให้เอง = max ID + 1 ถ้าชีตว่างจะเขียนหัวตารางให้ก่อน |
| **Save Changes** | เขียนทับบล็อกข้อมูลทั้งหมด (`<แท็บ>!A2:D`) ตามสิ่งที่แสดงในตาราง — แก้เซลล์ / ลบแถวแล้วกดปุ่มนี้ |
| **Validation** | ฟอร์มเพิ่มสินค้าและแถวที่จะ save ถูกตรวจก่อนยิง API เสมอ — ผิดกฎขึ้น **dialog พร้อม icon** แจ้งทุกจุดที่พังในครั้งเดียว (กฎอยู่ใน `ProductValidator` แยกจาก UI เทสได้เต็มรูปแบบ) |
| **Edit** | คลิกไอคอน ✏ ท้ายแถว (หรือดับเบิลคลิกแถว) → ค่าเดิมถูกใส่ลงฟอร์มให้แก้ → กด **Update Row** ตรวจด้วย validator แล้วเขียนลงชีตทันที |
| **Delete** | คลิกไอคอน 🗑 ท้ายแถว → dialog ยืนยัน (icon ⚠) → ลบจากชีตทันที |
| **Search** | ช่องค้นหาบน toolbar กรองสดตามพิมพ์: ตรง ID หรือมีคำใน Name (ไม่สนตัวพิมพ์) — ระหว่างกรองกริดเป็น read-only กันแก้ข้อมูลผ่านมุมมองที่ไม่ครบ |
| **Pagination** | แถบใต้ตาราง: ปุ่ม `< Prev` / `Next >` (ปุ่มตัวอักษรล้วน ไม่มี icon ซ้อน) + ป้าย "Page X of Y - N row(s)" + เลือกจำนวนแถวต่อหน้า (2/5/10/25/50/100) — คณิตแบ่งหน้าอยู่ใน `ProductPager` (เทสได้) ส่วน Save/Edit/Delete ยังทำงานกับ**ข้อมูลเต็ม**เสมอ ไม่ว่าจะดูหน้าไหนอยู่ |
| เปลี่ยนชีตได้ทันที | ช่อง Spreadsheet ID มุมบนซ้ายแก้ได้ตอนรัน ไม่ต้องแก้ App.config ใหม่ |
| ทนต่อข้อมูลเพี้ยน | แถวที่ ID ไม่ใช่ตัวเลขหรือชื่อว่างจะ**ข้าม** แถวสั้น/เซลล์เป็นข้อความจะถือเป็น 0 |

## สถาปัตยกรรม

แบ่งชั้นเพื่อให้ "โลจิก" ทดสอบได้โดยไม่แตะเน็ต — และ input ผ่านชั้น Validation ที่ไม่รู้จัก UI:

```
MainForm (UI, async) ──> ProductValidator (กฎ input)      ── คืน ValidationResult
        │
        └─────────────> ProductRepository (โลจิก) ──> ISheetService (interface)
                                                           │
                                               ┌───────────┴───────────┐
                                               │                       │
                                    GoogleSheetService          FakeSheetService
                                    (เรียก Google จริง)          (ตัวปลอมสำหรับเทส)
```

- `ISheetService` — ห่อ 4 ปฏิบัติการของ Sheets API: Get / Update / Append / Clear
- `GoogleSheetService` — ตัวจริง ยืนยันตัวด้วย service account key (JWT → OAuth2 token) ขอบเขต `spreadsheets`
- `ProductRepository` — โลจิก: อ่านทั้งหมด / รัน ID / เขียนทับ / เคลียร์ — เทสด้วย fake ล้วน ๆ
- `ProductValidator` — กฎตรวจ input ทั้งหมด (ฟอร์ม + แถวก่อน save) ไม่มี UI type เลย — เทสได้ 100%
- `SheetRowMapper` — แปลงแถวดิบ `IList<IList<object>>` ↔ `Product` พร้อมกฎกันข้อมูลเพี้ยน

อ่านต่อแบบละเอียด (data flow ทีละปุ่ม, ช่วง range ที่ใช้, กฎ parsing, กฎ validation) ที่ [docs/02-architecture.md](docs/02-architecture.md)

## โครงสร้างโปรเจกต์

```
GoogleSheetsDemo.sln
├── src/GoogleSheetsDemo/            แอป WinForms (.NET Framework 4.6.1)
│   ├── Program.cs                   จุดเริ่ม — เปิด TLS 1.2 ก่อนขึ้น UI
│   ├── MainForm.cs / .Designer.cs   หน้าจอหลัก (ช่อง ID + ตาราง + ปุ่ม 3 ปุ่ม + status bar)
│   ├── Services/
│   │   ├── ISheetService.cs         interface ห่อ API (จุดต่อสำหรับเทส)
│   │   ├── GoogleSheetService.cs    ตัวจริง (service account + Sheets API v4)
│   │   ├── ProductRepository.cs     โลจิกอ่าน/เพิ่ม/เขียนทับ (รัน ID ที่นี่)
│   │   ├── ProductFilter.cs         กฎค้นหา (ID ตรง หรือชื่อมีคำ — เทสได้)
│   │   ├── ProductPager.cs          คณิตแบ่งหน้า (TotalPages/ClampPage/Slice — เทสได้)
│   │   └── SheetRowMapper.cs        แปลงแถวชีต ↔ object
│   ├── Validation/                  กฎ input ทั้งหมด — แยกจาก UI (เทสได้)
│   │   ├── ProductValidator.cs      ตรวจฟอร์ม append + แถวก่อน save
│   │   ├── ValidationResult.cs      กอง failures + จัดข้อความสำหรับ dialog
│   │   ├── ValidationFailure.cs     1 จุดที่พัง (field/row + เหตุผล)
│   │   └── ProductInputResult.cs    ผล parse ฟอร์ม (ค่าที่ parse ได้ + failures)
│   ├── Models/Product.cs            1 แถวของชีต: Id, Name, Quantity, Price(decimal)
│   ├── App.config                   SpreadsheetId / GoogleCredentialsPath / SheetName
│   └── credentials.json             (ไม่ commit — สร้างเองตาม docs/01, ดูโครงสร้างใน docs นั้น)
├── tests/GoogleSheetsDemo.Tests/    MSTest V2 — 59 เคส
│   ├── FakeSheetService.cs          test double: จำ range/value ทุก call ที่ถูกเรียก
│   ├── ProductRepositoryTests.cs    6 เคส (โลจิกรัน ID, หัวตาราง, range ที่เขียน)
│   ├── SheetRowMapperTests.cs       10 เคส (parsing, ข้ามแถวเพี้ยน, invariant culture)
│   ├── ProductValidatorTests.cs     17 เคส (กฎ input ฟอร์ม + กฎแถวก่อน save)
│   ├── ProductFilterTests.cs        11 เคส (กฎค้นหา: ID/ชื่อ/ตัวพิมพ์/ลำดับ)
│   ├── ProductPagerTests.cs         14 เคส (จำนวนหน้า, clamp, slice ทุกขอบ)
│   └── TestRows.cs                  helper สร้างแถวดิบในเทส
├── tools/ConnectTest.cs (+ .exe)    โปรแกรมคอนโซลยิง API จริงเพื่อดู error เต็ม ๆ
├── docs/                            เอกสารลึกรายหัวข้อ
└── packages/                        NuGet (pin Google.Apis 1.55 — ดูหมายเหตุด้านล่าง)
```

## สิ่งที่ต้องมี

- **Visual Studio Community 2017** ติดตั้ง workload **".NET desktop development"**
- **.NET Framework 4.6.1 Targeting Pack** (มีมากับ VS2017 อยู่แล้ว)
- บัญชี Google สำหรับสร้างโปรเจกต์บน Google Cloud Console

> **หมายเหตุการ pin เวอร์ชัน:** โปรเจกต์นี้ล็อก `Google.Apis.*` ไว้ที่ **1.55.0** เพราะเครื่องงานนี้
> มีแค่ VS2017 + targeting pack ถึงแค่ 4.6.1 เวอร์ชันใหม่กว่านั้นต้องการ build tool ที่เครื่องไม่มี
> **อย่าอัปเดตแพ็กเกจถ้าไม่จำเป็น** (`Newtonsoft.Json` 13.0.1 ก็ล็อกเช่นกัน)

## เตรียม Google Cloud

คู่มือฉบับเดินตามมือ (พร้อมภาพปัญหาจริงที่เจอ) อยู่ที่ **[docs/01-google-cloud-setup.md](docs/01-google-cloud-setup.md)**
สรุปสั้น ๆ:

1. สร้าง Project ใน [Google Cloud Console](https://console.cloud.google.com/)
2. **APIs & Services → Library** → เปิดใช้ **Google Sheets API**
3. **Credentials → Create Credentials → Service account** (ข้าม role ได้)
4. แท็บ **Keys → Add key → JSON** → ดาวน์โหลด → เปลี่ยนชื่อเป็น `credentials.json`
   วางที่ `src/GoogleSheetsDemo/` (แอปหาไฟล์ใน 3 ตำแหน่ง — ดู [ตารางลำดับค้นหา](docs/02-architecture.md#ลำดับการค้นหาไฟล์-credentials))
5. สร้าง Google Sheet → **Share** → ใส่อีเมล service account (ค่า `client_email` ใน credentials.json)
   เป็น **Editor** ← *ขั้นนี้คือสาเหตุอันดับหนึ่งของ error 403 ห้ามข้าม*
6. คัดลอก **Spreadsheet ID** จาก URL มาใส่ App.config
7. เช็กชื่อแท็บชีตให้ตรงกับ `SheetName` ใน App.config
   *(ชีตที่สร้างจาก Google ภาษาไทย แท็บ default ชื่อ **"ชีต1"** ไม่ใช่ "Sheet1" — โปรเจกต์นี้ตั้งค่าเป็น "ชีต1" ไว้แล้ว)*

> ไม่ต้องสร้างหัวตารางเองก็ได้ — ชีตว่างแล้วกด **Append Row** แอปเขียนหัว `ID | Name | Quantity | Price` ให้เอง

## ตั้งค่าแอป

ทั้งหมดอยู่ใน `src/GoogleSheetsDemo/App.config`:

| key | ความหมาย | ค่าปัจจุบันของโปรเจกต์นี้ |
| --- | --- | --- |
| `SpreadsheetId` | ID จาก URL ชีต `.../d/`**`ส่วนนี้`**`/edit` | `1SbOWOfgu2R4cg_PmNtFQA7RxoryUUI_UFsj5fZGIFbs` |
| `GoogleCredentialsPath` | path ไฟล์ key (relative = เทียบกับโฟลเดอร์ exe) | `credentials.json` |
| `SheetName` | ชื่อแท็บในชีต — **ต้องสะกดตรงเป๊ะ** | `ชีต1` |

หมายเหตุ: ถ้า key ไหนหายไป แอปมีค่า default ในโค้ดคือ `credentials.json` / `Sheet1`

## Build และรัน

**ผ่าน Visual Studio:** เปิด `GoogleSheetsDemo.sln` → Build Solution (`Ctrl+Shift+B`, ครั้งแรก VS จะ restore NuGet เอง) → `F5`

**ผ่านคอมมานด์ไลน์ (MSBuild):**

```bat
"C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe" GoogleSheetsDemo.sln /p:Configuration=Debug /v:minimal
```

**ผ่านคอมมานด์ไลน์ (restore ก่อนถ้ายังไม่เคย build):** `nuget restore GoogleSheetsDemo.sln` (ต้องมี nuget.exe) หรือแค่เปิด VS แล้วสั่ง build ครั้งเดียว

> แก้ App.config แล้วต้อง **restart แอป** (และถ้ารันผ่าน VS ให้หยุด debug ก่อน build ไม่งั้นติด lock ของ exe)
> เทคนิค: ลอก App.config ทับ `bin\Debug\GoogleSheetsDemo.exe.config` แล้วปิด-เปิดแอปใหม่ ก็ใช้ค่าใหม่ได้โดยไม่ต้อง build

## วิธีใช้งานแอป

หน้าจอ: ช่อง **Spreadsheet ID** (บน) → **toolbar ค้นหา + Edit/Delete** → ตารางข้อมูล (กลาง) → กล่อง "Add new product" (Name/Quantity/Price) → ปุ่ม **Save Changes** + แถบสถานะ

| ปุ่ม / ช่อง | ทำอะไร | รายละเอียดเบื้องหลัง |
| --- | --- | --- |
| **Load** | อ่านชีตมาแสดง | GET `<แท็บ>!A2:D` แถวที่ ID ไม่ใช่เลข/ชื่อว่างจะไม่ถูกแสดง |
| **Append Row** | เพิ่มสินค้าจากฟอร์มล่าง | ผ่าน `ProductValidator` ก่อน: ชื่อต้องมี (≤100 ตัวอักษร), Quantity ต้องเป็นเลขจำนวนเต็ม 0–1,000,000,000, Price ต้องเป็นตัวเลข 0–99,999,999.99 — ผิดกฎขึ้น **dialog (icon ⚠) รวมทุกจุดที่พัง** ID = max+1 แล้วโหลดซ้ำอัตโนมัติ |
| **Edit** — ดับเบิลคลิกแถว หรือไอคอน **✏** ท้ายแถว | แก้ไขแถวนั้น | ค่าเดิมถูกใส่ลงฟอร์ม หัวกล่องเปลี่ยนเป็น "Edit product (ID N)" ปุ่มกลายเป็น **Update Row** โดยมี **Cancel** อยู่ข้าง ๆ สำหรับยกเลิก กดแล้วผ่าน validator → เขียนทับทั้งบล็อกลงชีตทันที |
| **Delete** — ไอคอน **🗑** ท้ายแถว | ลบแถวนั้น | dialog ยืนยัน (icon ⚠, Yes/No) → ลบจากชีตทันที |
| **Search (ID or Name)** | ค้นหา/กรองตาราง | กรองสดขณะพิมพ์: เลข = ID ตรงเป๊ะ, ข้อความ = ชื่อมีคำนั้น (ไม่สนตัวพิมพ์) — ระหว่างกรอง **กริด read-only** และ status บอกจำนวนแถวที่เจอ เคลียร์ช่อง = กลับไปดูทั้งหมด |
| **Save Changes** | เขียนทับตามตาราง | ตรวจทุกแถวในกริดก่อน (ID ≥ 1, ชื่อไม่ว่าง, ค่าในช่วง) ถ้าพังขึ้น **dialog ระบุ "Row N"** ที่มีปัญหา ลบแถวในกริดแล้วกดปุ่มนี้ = ลบจากชีตจริง ถ้าตารางว่าง = เคลียร์ A2:D |
| แก้ช่อง Spreadsheet ID | ใช้ชีตอื่นทันที | ใช้ได้เลยไม่ต้อง restart แต่จะไม่ถูกบันทึกกลับลง App.config |

หมายเหตุ: การแก้เซลล์ตรงในตาราง (นอกโหมด Edit) ยังอยู่แค่ในหน่วยความจำจนกว่าจะกด **Save Changes** — ส่วน Update Row / ลบผ่านไอคอน 🗑 จะเขียนลงชีตให้ทันที

**การจัดตำแหน่ง (baseline)** — caption/ปุ่ม/ช่องที่อยู่แถวเดียวกันถูกจัดกึ่งกลางแนวตั้งบนเส้นเดียวกันโดย `AlignLabelsWithInputs()` (ทำงานหลังตั้งฟอนต์/ธีม/resize): ปุ่ม Load/ธีมกับช่อง ID, ช่องค้นหากับ label, ปุ่ม Prev/Next/combo กับ label ของ pager, ช่อง Name/Quantity/Price กับปุ่ม Append/Cancel — ไม่ต้องกะพิกัดใน Designer เอง

**ตัวอักษรในช่อง input กึ่งกลางแนวตั้ง** — textbox ไร้ขอบ (`BorderStyle.None`) ปกติวาดตัวอักษรชิดขอบบนเสมอ แก้โดยให้แผงโค้งมนด้านหลัง (`pnlId`, `pnlName`, `pnlQuantity`, `pnlPrice`, `pnlSearch`) เป็น "ช่อง" ที่มองเห็น ส่วน textbox เองถูกย่อให้สูงเท่าบรรทัดตัวอักษรพอดี (ฟอนต์ + 2px) แล้วจัดกึ่งกลางในแผงด้วย `CenterInputInField()` — คลิกที่แผงจะโฟกัสเข้าช่องให้เอง สีช่องถูกอัปเดตพร้อมธีมผ่าน `StyleTextBox()`

**สถานะปุ่ม (Enable/Disable)** — ปุ่มทุกปุ่มถูกคุมจากจุดเดียวกัน (`UpdateButtonStates`):

| สถานการณ์ | ปุ่มที่ปิด (เทา) |
| --- | --- |
| เปิดแอปใหม่ กริดยังว่าง | **Save Changes** (กันกดเคลียร์ชีตโดยไม่ตั้งใจ) |
| ระหว่างยิง API (โหลด/เพิ่ม/บันทึก/แก้/ลบ — spinner หมุน) | **ปุ่มทุกปุ่ม + ช่องค้นหา** กันกดซ้ำ |
| ค้นหาแล้วไม่เจอแถว | ไม่มีปุ่มไหนปิดเพิ่ม — Save ยังเปิดเพราะข้อมูลหลักยังอยู่ (แก้/ลบทำผ่านไอคอนรายแถว) |
| มีแถวถูกเลือก + ไม่ busy | เปิดครบทุกปุ่ม |

**การแสดง error ทุกประเภท** (แนวนี้ใช้ทั้งแอป):

| ประเภท | dialog | icon |
| --- | --- | --- |
| กรอกข้อมูลผิดกฎ | รวมทุก failure เป็นบูลเล็ตเดียว (`- Name: Name is required.` ฯลฯ) | ⚠ Warning |
| ยืนยันการลบแถว | "Confirm delete" ระบุชื่อ+ID ที่จะลบ (Yes/No) | ⚠ Warning |
| กด Edit/Delete โดยยังไม่เลือกแถว | "No row selected" | ⚠ Warning |
| runtime (API/ไฟล์/เน็ต เช่น 403, 404) | หัวข้อ "Load/Append/Save/Update/Delete Failed" + ข้อความ exception | ✖ Error |
| ทำสำเร็จ | ไม่ขึ้น dialog (แจ้งในแถบสถานะพอ) | — |

## รูปแบบข้อมูลในชีต

```
    A        B         C           D
1   ID       Name      Quantity    Price      <- หัวตาราง (เขียนอัตโนมัติเมื่อ append บนชีตว่าง)
2   1        Keyboard  10          199.5
3   2        Mouse     25          8.9
```

- เขียนเลขแบบ **invariant culture** เสมอ (`1234.5` ไม่ใช่ `1.234,5` หรือ `1,234.50`)
- อ่าน: เลข parse ไม่ได้ → 0, ID parse ไม่ได้ → **ข้ามทั้งแถว**, ชื่อว่าง → **ข้ามทั้งแถว**
- ตัวเลขในชีตที่ Google จัด format ไว้ (มี comma เป็นพัน ฯลฯ) จะถูกอ่านเป็นข้อความแล้ว parse ให้ ถ้าอยากได้ชีตสะอาดแนะนำ Format → Number ปกติ

## รัน Unit Test

**ผ่าน VS2017:** เมนู **Test → Windows → Test Explorer** → **Run All** (`Ctrl+R, A`)

**ผ่านคอมมานด์ไลน์ (ยืนยันแล้วว่าผ่าน 16/16):**

```bat
"C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" tests\GoogleSheetsDemo.Tests\bin\Debug\GoogleSheetsDemo.Tests.dll
```

เทสทั้ง 58 เคสใช้ `FakeSheetService` และ validator/filter/pager ล้วน ๆ — เทสตรวจได้ทั้ง "เรียกถูกตำแหน่ง"
"เขียนถูก format" และ "กฎ input/ค้นหา/แบ่งหน้า ถูกต้อง" โดยไม่ยิงเน็ต รายชื่อเทสและสิ่งที่แต่ละเคสพิสูจน์ อยู่ที่ [docs/02-architecture.md](docs/02-architecture.md#unit-test-58-เคส)

## เครื่องมือวินิจฉัยการเชื่อมต่อ

เจอ error ในแถบสถานะแล้วไม่รู้สาเหตุ? ใช้ `tools\ConnectTest.exe` (คอนโซล) ยิง API จริงด้วย credentials/ID เดียวกับแอป แล้วดู exception ทั้งก้อน:

```bat
tools\ConnectTest.exe                          :: ใช้ค่า default ของโปรเจกต์
tools\ConnectTest.exe <credentials.json> <spreadsheetId> <range>
```

ตีความผลลัพธ์ (ต้นไม้การวินิจฉัยแบบละเอียดที่ [docs/04-connect-test.md](docs/04-connect-test.md)):

| ผลที่ได้ | แปลว่า |
| --- | --- |
| `SUCCESS - got N row(s)` | ทุกอย่างถูกต้อง — ปัญหาอยู่ที่อื่น |
| `403 The caller does not have permission` | ยังไม่ได้แชร์ชีตให้ service account (สาเหตุเดิมของโปรเจกต์นี้) |
| `404 Requested entity was not found` ตอนยิง **ID จริง** | ID ผิดหรือชีตถูกลบ — *แต่ถ้าได้ 404 ตอนยิง ID ปลอม = auth และ API ปกติทุกอย่าง* |
| `403 ... has not been used in project / accessNotConfigured` | ยังไม่ได้ Enable Google Sheets API |
| `Unable to parse range: Sheet1!...` | ชื่อแท็บไม่ตรง `SheetName` (เช่น ชีตภาษาไทยใช้ "ชีต1") |
| `credentials file not found` | ไฟล์ key ไม่อยู่ในตำแหน่งที่แอปค้น |

## แก้ปัญหาที่พบบ่อย

ตารางสรุป — ฉบับเต็มพร้อมวิธีแก้ทีละขั้นที่ [docs/03-troubleshooting.md](docs/03-troubleshooting.md)

| อาการในแถบสถานะ | สาเหตุ | วิธีแก้ |
| --- | --- | --- |
| `Load failed: ... 403 ... The caller does not have permission` | ไม่ได้แชร์ชีตให้ `client_email` | Share ชีต → ใส่อีเมล service account เป็น **Editor** |
| `Load failed: ... 403 ... accessNotConfigured` | ยังไม่ Enable Sheets API | เปิดใน Library ของโปรเจกต์เดียวกับ key |
| `Load failed: Unable to parse range: Sheet1!...` | ชื่อแท็บ ≠ `SheetName` | แก้ App.config (เช่น `ชีต1`) แล้ว restart |
| `Unable to parse range:` แต่ค่าในข้อความเป็น**อักขระมั่ว** (เช่น `à¸¢à¸µ...1`) | ไฟล์ `exe.config` ถูกเขียนใหม่ด้วย encoding ของเครื่อง (จาก `config.Save()` เวอร์ชันเก่า) ทำให้ค่าภาษาไทยพัง | ลอก `App.config` ทับ `bin\Debug\GoogleSheetsDemo.exe.config` แล้ว restart — เวอร์ชันใหม่เก็บธีมในไฟล์ `GoogleSheetsDemo.theme` แยก ไม่แตะ .config อีกต่อไป |
| `Load failed: Google credentials file not found ...` | ไม่เจอ `credentials.json` | วางที่ `src/GoogleSheetsDemo/` หรือ `bin\Debug\` |
| ขึ้น dialog ⚠ "Cannot append row / Cannot save changes" | input ผิดกฎ validation (ชื่อว่าง, เลขผิดรูป, เกินช่วง) | แก้ตามรายการใน dialog แล้วกดปุ่มซ้ำ |
| `Save failed: ...` ตอนกด Save | ดู error หลังเครื่องหมาย `:` | ส่วนใหญ่เป็นเรื่องสิทธิ์/เน็ต เหมือนแถว 403/401 |
| 401 `invalid_grant` | key ถูกลบ/หมดอายุ หรือนาฬิกาเครื่องคลาด | สร้าง key ใหม่ / sync เวลาเครื่อง |

## ความปลอดภัย

- `credentials.json` คือกุญแจเข้า Google ของโปรเจกต์ (แบบไม่ต้อง password) — **ห้าม commit** (`.gitignore` กันไว้แล้ว) ห้ามส่งในแชท/อีเมล
- ถ้า key หลั่งไหลออกไป: **Google Cloud Console → IAM & Admin → Service Accounts → แท็บ Keys → ลบ key ทิ้งทันที** แล้วสร้างใหม่
- สิทธิ์ของ service account คือสิทธิ์บนไฟล์ที่ถูกแชร์ให้เท่านั้น (ไม่ใช่ทั้ง Drive ของเจ้าของ) — ถ้าจะเลิกใช้ แค่เอาออกจากผู้แชร์ของชีต
- โปรเจกต์นี้ใช้ขอบเขต `https://www.googleapis.com/auth/spreadsheets` (อ่าน+เขียนเฉพาะชีตที่แชร์ให้)

## เอกสารเพิ่มเติม

| ไฟล์ | เนื้อหา |
| --- | --- |
| [docs/01-google-cloud-setup.md](docs/01-google-cloud-setup.md) | เดินตามทีละคลิก: สร้าง project / enable API / service account / แชร์ชีต (รวมกรณีเบราว์เซอร์ Error 400) |
| [docs/02-architecture.md](docs/02-architecture.md) | สถาปัตยกรรมลึก: data flow ทีละปุ่ม (รวม edit/delete/search/pagination), range ที่ใช้, กฎ parsing + validation + ค้นหา + แบ่งหน้า, ตารางเทส 58 เคส |
| [docs/03-troubleshooting.md](docs/03-troubleshooting.md) | แก้ปัญหาฉบับเต็ม: ทุก error ที่เคยเจอจริง + วิธีวินิจฉัยทีละขั้น |
| [docs/04-connect-test.md](docs/04-connect-test.md) | วิธีใช้และตีความผล `tools\ConnectTest.exe` |

## บันทึกการแก้ไข

**6 ก.ย. 2026** — แก้ runtime error ที่ค้างมาตั้งแต่ต้น (ทดสอบผ่านครบท้ายวัน):
1. แชร์ชีต (ID ข้างบน) ให้ `msl-sheet-demo@midseelee.iam.gserviceaccount.com` เป็น Editor → แก้ 403
2. เปลี่ยน `SheetName` ใน App.config `Sheet1` → `ชีต1` (แท็บจริงของชีต) → แก้ Unable to parse range
3. ตั้งชื่อเอกสารชีตเป็น "GoogleSheetsDemo" (Google บังคับตอนกดแชร์เองก็ต้องตั้ง)
4. เพิ่ม `tools/ConnectTest.cs` เครื่องมือวินิจฉัย + เอกสารครบชุดใน `docs/`

**6 ก.ย. 2026 (รอบสอง)** — เพิ่ม validation + ปรับ UI:
1. ชั้น `Validation/` ใหม่ (`ProductValidator` + `ValidationResult`) แยกกฎ input ออกจาก UI — เทสได้เต็มรูปแบบ (+17 เทส รวมเป็น 33)
2. ฟอร์ม Append ตรวจเข้ม: ชื่อต้องมี (≤100 ตัวอักษร), Quantity จำนวนเต็ม 0–1e9, Price 0–99,999,999.99 — ก่อนหน้านี้เลขเพี้ยนเงียบ ๆ กลายเป็น 0
3. Save Changes ตรวจทุกแถวก่อนเขียน (รายงานเป็น "Row N")
4. error ทุกประเภทขึ้น dialog พร้อม icon (⚠ warning สำหรับ validation, ✖ error สำหรับ runtime)
5. หน้าตาใหม่: ฟอนต์ Segoe UI, ปุ่มสีตามบทบาท (Load/Append/Save), หัวตารางน้ำเงิน + แถวสลับสี, ปรับขนาดหน้าต่างได้

**6 ก.ย. 2026 (รอบสาม)** — เพิ่ม edit / delete / search:
1. ชั้น `Services/ProductFilter.cs` กฎค้นหา (ID ตรงเป๊ะ หรือชื่อมีคำ ไม่สนตัวพิมพ์) + เทส 11 เคส (รวมเป็น 44)
2. **Edit** — คลิกไอคอน ✏ ท้ายแถว หรือดับเบิลคลิกแถว: โหลดค่าเดิมลงฟอร์ม ปุ่มกลายเป็น Update Row (+ Cancel Edit) ผ่าน validator แล้วเขียนลงชีตทันที
3. **Delete** — คลิกไอคอน 🗑 ท้ายแถว: dialog ยืนยัน (icon ⚠) แล้วเขียนลงชีตทันที
4. **Search**: กรองสดขณะพิมพ์ ระหว่างกรองกริด read-only กันแก้ข้อมูลผ่านมุมมองที่ไม่ครบ
5. รวมงานธีมใหม่ (ขอบมน, spinner, ไอคอนแอป) กับฟีเจอร์ทั้งหมดโดยไม่มีอะไรตกหล่น

**6 ก.ย. 2026 (รอบสี่)** — ปุ่ม enable/disable ครบ + Pagination:
1. `UpdateButtonStates()` จุดคุมสถานะเดียว: ปิดทุกปุ่มตอน busy, Edit/Delete ปิดเมื่อไม่มีแถวเลือก, Save ปิดจนกว่าจะมีข้อมูลโหลด/เพิ่มแล้ว (กันเคลียร์ชีตเปล่าโดยไม่ตั้งใจ)
2. ชั้น `Services/ProductPager.cs` + เทส 14 เคส (รวมเป็น 58) — TotalPages / ClampPage / Slice
3. แถบ pager ใต้ตาราง: Prev/Next (ปิดเองเมื่ออยู่หน้าแรก/สุดท้าย), ป้าย "Page X of Y - N row(s)", เลือกแถวต่อหน้า 2/5/10/25/50/100
4. เพจเลขใหม่ตอนกด Append (เลื่อนไปหน้าสุดท้ายให้เห็นแถวใหม่), ค้นหาใหม่เริ่มหน้า 1, Save/Edit/Delete ทำงานกับข้อมูลเต็มเสมอข้ามทุกหน้า

**6 ก.ย. 2026 (รอบห้า)** — แก้อักขระมั่ว + layout ทับกัน + ปุ่มรายแถว:
1. ซ่อม "ชีต1" ใน exe.config ที่พังเป็นอักขระมั่ว (สาเหตุ: `config.Save()` เขียนไฟล์ด้วย encoding เครื่อง)
2. ธีมย้ายไปเก็บไฟล์ `GoogleSheetsDemo.theme` (UTF-8) — ไม่เขียน .config อีก ภาษาไทยปลอดภัย
3. `RelayoutLowerArea()`: pager/กริด/กล่องแก้ไขเรียงกันไม่ทับกันทุกขนาดหน้าต่าง + MinimumSize 916
4. ปุ่ม **Edit / Delete รายแถว** ทางขวาของตาราง (นอกจากปุ่ม toolbar เดิม) + ปุ่มสลับธีมมุมมนสไตล์เดียวกับปุ่มอื่น
