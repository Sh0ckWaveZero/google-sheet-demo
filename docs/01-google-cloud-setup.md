# ขั้นตอนเตรียม Google Cloud ฉบับละเอียด (ทำครั้งเดียวต่อเครื่อง/ต่อโปรเจกต์)

เอกสารนี้เดินตามจริงมาแล้วทั้งเส้นทางเมื่อ 6 ก.ย. 2026 รวมถึง**กับดักที่เจอจริง** (Error 403, ชื่อแท็บภาษาไทย,
เบราว์เซอร์ขึ้น Error 400 ตอนจะแชร์ชีต) ทำตามลำดับห้ามข้ามขั้น

ทั้งหมดใช้แนวทาง **Service Account** — เหมาะกับแอปที่รันเงียบ ๆ ไม่ต้องขึ้นหน้า login ให้คนกดทุกครั้ง

---

## ทำไมต้องทำครบทั้ง 3 เรื่อง

แอปจะทำงานได้ต้องผ่าน 3 ด่านพร้อมกัน ขาดด่านใดด่านหนึ่งจะ error ทันที:

| ด่าน | คืออะไร | ถ้าขาดจะเจอ |
| --- | --- | --- |
| 1. Service account + key ไฟล์ | "ตัวตน" ของแอป (อีเมลปลอมแบบหุ่นยนต์) กับกุญแจ JSON | `credentials file not found` / 401 |
| 2. Enable Google Sheets API | สวิตช์เปิดใช้ API ของโปรเจกต์บนคลาวด์ | 403 `accessNotConfigured` |
| 3. แชร์ชีตให้ service account | สิทธิ์เข้าถึง**ไฟล์** (Google เช็กแยกต่อไฟล์) | **403 `The caller does not have permission`** |

สำคัญ: ด่าน 2 กับด่าน 3 คนละเรื่องกัน — เปิด API แล้ว**ไม่ได้**แปลว่าเข้าถึงชีตได้
(โปรเจกต์นี้ติดด่าน 3 มาตั้งแต่ต้น ทั้งที่ API เปิดแล้ว)

---

## ขั้นที่ 1 — สร้าง Google Cloud Project

1. เข้า [console.cloud.google.com](https://console.cloud.google.com/) ล็อกอินด้วยบัญชี Google ของคุณ
2. แถบด้านบน ข้างโลโก้ มี dropdown เลือกโปรเจกต์ → กด → **New Project** (โปรเจกต์ใหม่)
3. ตั้งชื่ออะไรก็ได้ (ตัวอย่างของโปรเจกต์นี้ใช้ชื่อ `midseelee`) → **Create**
4. รอสักครู่ แล้วเลือกโปรเจกต์นี้ใน dropdown ให้เรียบร้อย (มุมบนซ้ายต้องแสดงชื่อที่เพิ่งสร้าง)

## ขั้นที่ 2 — Enable Google Sheets API

1. เมนูแฮมเบอร์เกอร์ (☰) → **APIs & Services → Library**
2. ช่องค้นหา พิมพ์ `Google Sheets API` → เลือกรายการ **Google Sheets API**
3. กดปุ่ม **Enable**
4. เมื่อเปิดแล้วหน้าจอจะเปลี่ยนเป็นหน้า overview ของ API (มีปุ่ม Disable อยู่แทน) — เท่านี้คือเสร็จ

> เช็กว่าเปิดถูกโปรเจกต์: APIs & Services → Enabled APIs & services → ต้องเห็น Google Sheets API
> อยู่ในรายการของ**โปรเจกต์เดียวกับ**ที่จะสร้าง service account ในขั้นถัดไป

## ขั้นที่ 3 — สร้าง Service Account และดาวน์โหลด Key

1. **APIs & Services → Credentials** → ปุ่มบนสุด **+ Create Credentials → Service account**
2. ตั้งชื่อ เช่น `msl-sheet-demo` (ชื่อจะกลายเป็นส่วนหน้าของอีเมล) → **Create and Continue**
3. หน้า Role — **ข้ามได้** (กด Continue ผ่าน) เพราะเราจะให้สิทธิ์ระดับไฟล์ผ่านการ Share แทน
4. หน้า Grant users — ข้ามได้ → **Done**
5. กลับหน้า Credentials จะเห็น service account ใหม่ในลิสต์ → คลิกเข้าไป
6. แท็บ **Keys** → **Add key → Create new key** → เลือก **JSON** → **Create**
   ไฟล์ `.json` จะดาวน์โหลดอัตโนมัติ (ชื่อยาว ๆ ประมาณ `project-name-xxxxx.json`)
7. เปลี่ยนชื่อไฟล์เป็น **`credentials.json`** แล้วย้ายไปวางที่:
   ```
   C:\lisa\Demo_Spread_Sheet\src\GoogleSheetsDemo\credentials.json
   ```
   (แอปค้นหาไฟล์ 3 ตำแหน่ง — ดู [ลำดับการค้นหา](02-architecture.md#ลำดับการค้นหาไฟล์-credentials) — แต่ที่นี่คือจุดมาตรฐาน)
8. เปิดไฟล์ด้วย notepad แล้วหาค่า **`client_email`** — จะได้อีเมลประมาณ
   `msl-sheet-demo@midseelee.iam.gserviceaccount.com` **จดไว้** ขั้นถัดไปต้องใช้

> ไฟล์ key นี้ = รหัสผ่าน ห้าม commit / ห้ามส่งให้ใคร ถ้าหลุดให้ลบ key ทันที (แท็บ Keys → ลบ) แล้วสร้างใหม่

## ขั้นที่ 4 — สร้างชีตและแชร์ให้ service account

1. สร้าง Google Sheet ใหม่ที่ [sheets.new](https://sheets.new) (หรือจาก Google Drive)
2. คลิกปุ่ม **แชร์ (Share)** มุมขวาบน
   - ถ้าชีตยังไม่มีชื่อ Google จะถาม "ตั้งชื่อเอกสารก่อนแชร์" — ตั้งชื่อได้เลย (โปรเจกต์นี้ใช้ `GoogleSheetsDemo`)
     หรือกด **ข้าม** ก็ได้ ชื่อไม่มีผลต่อการเชื่อมต่อ
3. ในช่อง "เพิ่มผู้คน กลุ่ม..." **วางอีเมล service account** จากขั้นที่ 3.8
   ระบบจะขึ้นรายการแนะนำ → คลิกเลือก
4. **เปลี่ยนสิทธิ์เป็น "เอดิเตอร์" (Editor)** ← สำคัญ ถ้าเป็น Viewer จะอ่านได้อย่างเดียว
5. ถ้าไม่อยากให้ส่งอีเมลแจ้ง (service account ไม่อ่านเมลอยู่แล้ว) ให้**ยกเลิกติ๊ก "แจ้งเตือนพวกเขา"**
   ปุ่มจะเปลี่ยนเป็น **แชร์** → กดยืนยัน
6. กลับไปที่ชีต จะเห็นไอคอน 🔒 ข้างปุ่มแชร์เปลี่ยนเป็นรูปคน 2 คน — แปลว่าแชร์สำเร็จ

> **เจอ Error 400 (Bad Request) จาก `docs.google.com/accounts/SetOSID` ตอนจะเปิด/แชร์ชีต?**
> เป็นปัญหา cookie ของ Google ในเบราว์เซอร์ ไม่เกี่ยวกับแอป — วิธีแก้ที่ได้ผลในเครื่องนี้:
> พิมพ์ URL ของชีตลงแถบที่อยู่ตรง ๆ (`https://docs.google.com/spreadsheets/d/.../edit`) เพื่อข้าม
> หน้า redirect ถ้ายังไม่หาย ให้ล้าง cookie ของ `google.com` หรือเปิดหน้าต่างไม่ระบุตัวตนแล้วล็อกอินใหม่

## ขั้นที่ 5 — เอา Spreadsheet ID มาใส่แอป

1. ดู URL ของชีตในเบราว์เซอร์:
   ```
   https://docs.google.com/spreadsheets/d/1SbOWOfgu2R4cg_PmNtFQA7RxoryUUI_UFsj5fZGIFbs/edit
                                         └──────────── ส่วนนี้คือ ID ────────────┘
   ```
2. คัดลอกส่วนระหว่าง `/d/` กับ `/edit` (สตริงยาว 40+ ตัวอักษร ไม่มี `/`)
3. เปิด `src/GoogleSheetsDemo/App.config` ใส่ใน key `SpreadsheetId`

## ขั้นที่ 6 — เช็กชื่อแท็บ (กับดักภาษาไทย!)

1. ดูมุมล่างซ้ายของชีต — ชื่อแท็บแรก ถ้า Google ของคุณเป็นภาษาไทยจะชื่อ **"ชีต1"** (ไม่ใช่ "Sheet1")
2. ชื่อนี้ต้องตรงกับ key `SheetName` ใน App.config **เป๊ะ ๆ** (สะกด/วรรณยุกต์ต่างกันไม่ได้)
3. มี 2 ทางเลือก:
   - แก้ App.config ให้ตรงกับชีต ← **โปรเจกต์นี้เลือกทางนี้ ตั้ง `SheetName` = `ชีต1` ไว้แล้ว**
   - หรือดับเบิลคลิกชื่อแท็บในชีต เปลี่ยนเป็น `Sheet1` (อังกฤษ) แล้วแก้ App.config กลับ
4. แก้ App.config แล้วต้อง **restart แอป** ถึงจะมีผล

## ขั้นที่ 7 — ทดสอบว่าทุกอย่างผูกกันถูกต้อง

ก่อนเปิดแอปจริง ยิงทดสอบด้วยเครื่องมือวินิจฉัยของโปรเจกต์ (ดูรายละเอียด [docs/04-connect-test.md](04-connect-test.md)):

```bat
cd C:\lisa\Demo_Spread_Sheet
tools\ConnectTest.exe
```

- เห็น `SUCCESS - got N row(s)` → เสร็จสมบูรณ์ เปิดแอปได้เลย
- เห็น `403 The caller does not have permission` → กลับไปขั้นที่ 4 (แชร์ชีต)
- เห็น `Unable to parse range` → กลับไปขั้นที่ 6 (ชื่อแท็บ)
- เห็น `accessNotConfigured` → กลับไปขั้นที่ 2 (Enable API)

---

## Checklist ย่อ (พิมพ์ติดจอ)

```
□ สร้าง Project บน Cloud Console แล้วเลือกให้ถูก
□ Enable Google Sheets API ในโปรเจกต์นั้น
□ สร้าง Service account + ดาวน์โหลด JSON key
□ เปลี่ยนชื่อเป็น credentials.json วางที่ src/GoogleSheetsDemo/
□ Share ชีตให้ client_email เป็น EDITOR (ไม่ใช่ Viewer)
□ คัดลอก Spreadsheet ID ใส่ App.config
□ ชื่อแท็บชีต = SheetName ใน App.config (ระวัง "ชีต1" vs "Sheet1")
□ tools\ConnectTest.exe ขึ้น SUCCESS
```
