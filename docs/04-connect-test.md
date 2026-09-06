# เครื่องมือวินิจฉัยการเชื่อมต่อ (tools\ConnectTest.exe)

โปรแกรมคอนโซลตัวเล็ก ทำหน้าที่เดียว: **เรียก Google Sheets API จริง** ด้วย credentials และ
Spreadsheet ID ชุดเดียวกับที่แอปใช้ (ผ่าน `GoogleSheetService` ตัวเดียวกันเป๊ะ)
แล้วพิมพ์ผลออกหน้าจอ — ถ้า error จะพิมพ์ **exception chain ทั้งก้อน**

ประโยชน์: แถบสถานะของแอปโชว์แค่ `ex.Message` ชั้นนอกสุด (สั้น) แต่ตัวนี้เห็นสาเหตุลึก ๆ
ทั้ง HTTP body ที่ Google ตอบกลับ ทำให้แยกโรคได้ใน 1 คำสั่ง โดยไม่ต้องเปิด debugger

---

## วิธีใช้

```bat
cd C:\lisa\Demo_Spread_Sheet

:: ใช้ค่า default ของโปรเจกต์ (credentials ใน bin\Debug + ID ใน App.config + range ชีต1!A1:D5)
tools\ConnectTest.exe

:: หรือระบุเองทั้ง 3 ค่า (ลำดับ: ไฟล์ key, spreadsheet ID, range)
tools\ConnectTest.exe C:\path\to\credentials.json 1SbOWOfgu2R4cg_PmNtFQA7RxoryUUI_UFsj5fZGIFbs Sheet1!A1:D5
```

ออก code `0` เมื่ออ่านสำเร็จ, `1` เมื่อ error (เอาไปใช้ใน script ได้)

## ตีความผลลัพธ์ (ต้นไม้การวินิจฉัย)

### ✅ `SUCCESS - got N row(s)`

ทุกอย่างถูกต้อง: key ใช้ได้, API เปิด, แชร์ชีตแล้ว, range ถูก
→ ถ้าแอปยัง error แสดงว่าปัญหาอยู่ที่ config ของแอป (เช่น exe.config เก่ากว่า App.config ให้ restart/build ใหม่)

### ❌ `403 The caller does not have permission` (reason: forbidden)

ยังไม่ได้แชร์ชีตให้ service account → แก้ตาม [docs/03 ข้อ 1](03-troubleshooting.md#1-403--the-caller-does-not-have-permission)

**เทคนิคพิสูจน์ตัวเอง:** ยิงซ้ำด้วย **ID ปลอม** เช่น
```bat
tools\ConnectTest.exe "" AAAA0000BBBB1111CCCC2222DDDD3333EEEE4444 Sheet1!A1:D5
```
(ส่ง path ค่าว่าง ๆ ให้ตัวโปรแกรมใช้ default ต่อ)

| ผลกับ ID ปลอม | แปลว่า |
| --- | --- |
| **404 Requested entity was not found** | auth ผ่าน + API เปิดแล้ว — ปัญหาแน่นอนคือ "ไม่ได้แชร์ชีตจริง" ← เคสของโปรเจกต์นี้ |
| **403 accessNotConfigured / has not been used** | ยังไม่ได้ Enable Google Sheets API |

### ❌ `404 Requested entity was not found` (ตอนใช้ **ID จริง**)

ID ผิด / ชีตถูกลบ / ชีตไม่ได้แชร์ให้ service account นี้เลย (Google ตอบ 404 แทน 403 เพื่อไม่เปิดเผยว่าไฟล์มีอยู่)

### ❌ `Unable to parse range: Sheet1!...`

ชื่อแท็บไม่ตรง — range ที่ส่งใช้แท็บชื่ออื่นกว่าที่ชีตมีจริง (ภาษาไทยระวัง "ชีต1") → [docs/03 ข้อ 2](03-troubleshooting.md#2-unable-to-parse-range-sheet1a2d-หรือ-range-อื่น)

### ❌ `credentials file was not found`

path ไฟล์ key ผิด — ตัวโปรแกรมนี้**ไม่**ค้นหา 3 ตำแหน่งเหมือนแอป ต้องระบุ path เต็มถ้าไฟล์ไม่ได้อยู่ที่ `bin\Debug\credentials.json`

### ❌ อื่น ๆ (network / TLS / 401)

ดู [docs/03-troubleshooting.md](03-troubleshooting.md) หัวข้อ 5, 8 — exception chain ที่พิมพ์ออกมาจะบอกชั้นในสุด เช่น
`INNER 1: [System.Net.Sockets.SocketException] ...` = ปัญหา network ไม่ใช่ Google

## ตัวอย่างผลจริงจากโปรเจกต์นี้

**ตอนยังไม่ได้แชร์ชีต (ปัญหาเดิม):**
```
EXCEPTION: [Google.GoogleApiException] Google.Apis.Requests.RequestError
The caller does not have permission [403]
Errors [
	Message[The caller does not have permission] Location[ - ] Reason[forbidden] Domain[global]
]
```

**หลังแชร์แล้ว (และแก้ชื่อแท็บถูก):**
```
SUCCESS - got 0 row(s):
```

**ยิง ID ปลอมเพื่อพิสูจน์ว่า auth/API ปกติ:**
```
EXCEPTION: [Google.GoogleApiException] Google.Apis.Requests.RequestError
Requested entity was not found. [404]
```

## หากต้องแก้/คอมไพล์ใหม่

ซอร์สอยู่ที่ `tools\ConnectTest.cs` (โหลด `GoogleSheetService` จาก `bin\Debug\GoogleSheetsDemo.exe`
ผ่าน reflection — แก้ logic ของแอปแล้วตัวนี้ใช้ตัวใหม่ทันทีหลัง build)

คอมไพล์ด้วย C# compiler ที่มีในเครื่องทุกเครื่องที่ลง .NET Framework:

```bat
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe -nologo -out:tools\ConnectTest.exe ^
  -r:src\GoogleSheetsDemo\bin\Debug\Google.Apis.dll ^
  -r:src\GoogleSheetsDemo\bin\Debug\Google.Apis.Auth.dll ^
  -r:src\GoogleSheetsDemo\bin\Debug\Google.Apis.Core.dll ^
  -r:src\GoogleSheetsDemo\bin\Debug\Google.Apis.Sheets.v4.dll ^
  -r:src\GoogleSheetsDemo\bin\Debug\Newtonsoft.Json.dll ^
  tools\ConnectTest.cs
```

> คอมไพเลอร์ตัวนี้รองรับแค่ C# 5 — ห้ามใช้ syntax ใหม่ เช่น `?.`, string interpolation `$""`
> (ถ้าจะแก้ไฟล์นี้ ดูตัวอย่างการเขียนแบบ C# 5 ในไฟล์เดิมได้เลย)
