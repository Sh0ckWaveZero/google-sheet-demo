# แก้ปัญหาฉบับเต็ม (สังเกตจากข้อความ error แล้วไล่หาสาเหตุ)

ทุก error ในเอกสารนี้เป็นกรณีที่**เจอจริง**กับโปรเจกต์นี้ จัดเรียงจากที่พบบ่อยสุด
error ของแอปจะโผล่ที่แถบสถานะล่างซ้ายเป็น `Load failed: ...` / `Append failed: ...` / `Save failed: ...`
ตัวหลัง `:` คือข้อความจาก exception — จับคู่กับตารางด้านล่าง

ตรวจเร็วก่อนเริ่ม: รัน `tools\ConnectTest.exe` แล้วเทียบผลกับ [docs/04-connect-test.md](04-connect-test.md)
จะรู้ทันทีว่าปัญหาอยู่ที่ key / API / สิทธิ์ / ชื่อแท็บ ขั้นไหน

---

## 1) `403 ... The caller does not have permission`

**สาเหตุ:** service account ยืนยันตัวสำเร็จแล้ว แต่**ไฟล์ชีตไม่ได้แชร์ให้มัน**
(เป็นสาเหตุเดิมของโปรเจกต์นี้ — ติดมาตั้งแต่แรกจนแก้เมื่อ 6 ก.ย. 2026)

**ตรวจว่าใช่จริง:** ยิง `tools\ConnectTest.exe` ด้วย ID ปลอม (เช่น `AAAA0000BBBB1111CCCC2222DDDD3333EEEE4444`)
- ได้ **404 not found** → auth และ API ปกติทุกอย่าง ปัญหาคือสิทธิ์ไฟล์แน่นอน
- ID จริงให้ 403 แต่ ID ปลอมให้ 404 = ยืนยันว่าเป็นเรื่องแชร์

**วิธีแก้:**
1. เปิดไฟล์ `credentials.json` หาค่า `client_email` (เช่น `msl-sheet-demo@midseelee.iam.gserviceaccount.com`)
2. เปิดชีต → **แชร์** → วางอีเมลนั้น → สิทธิ์ **เอดิเตอร์** → ยืนยัน
3. กด Load ใหม่ทันที (ไม่ต้อง restart แอป เพราะสิทธิ์ฝั่ง Google เช็กทุก request)

**กับดัก:**
- แชร์ผิดอีเมล (พิมพ์ client_id แทน client_email หรือ typo) — ต้องเป็นค่า `client_email` ใน JSON เป๊ะ ๆ
- ให้สิทธิ์ **Viewer** → Load ผ่านแต่ Append/Save ติด 403 ให้เป็น Editor ไปเลย
- แชร์ให้ไฟล์อื่น แต่ App.config ชี้ไฟล์ที่ยังไม่แชร์ — ต้องแชร์ทุกไฟล์ที่ใช้ (หรือแชร์ทั้งโฟลเดอร์ Drive)

## 2) `Unable to parse range: Sheet1!A2:D` (หรือ range อื่น)

**สาเหตุ:** ชื่อแท็บใน config (`SheetName`) ไม่ตรงกับแท็บจริงในชีต
โปรเจกต์นี้เคยเจอเพราะชีตสร้างจาก Google ภาษาไทย แท็บ default ชื่อ **"ชีต1"** แต่ config ตั้ง `"Sheet1"`

**วิธีแก้ (เลือกอย่างใดอย่างหนึ่ง):**
- แก้ `App.config` key `SheetName` ให้ตรงชื่อแท็บจริง (เช่น `ชีต1`) → **restart แอป**
- หรือเปลี่ยนชื่อแท็บในชีต (ดับเบิลคลิกที่แท็บ) เป็น `Sheet1`

**กับดัก:**
- ชื่อแท็บภาษาไทยมีสระ/วรรณยุกต์ — สะกดต่างนิดเดียวก็ไม่ผ่าน คัดลอกชื่อจากชีตมาวางปลอดภัยสุด
- มีช่องว่างในชื่อแท็บ เช่น `My Sheet` → ใน config ใส่ตรง ๆ ได้ แอปจะต่อเป็น `My Sheet!A2:D` ให้เอง
- แก้ App.config แล้วลืม restart — แอปแคชค่าตอนเปิดโปรแกรม

## 3) `403 ... has not been used in project ... or it is disabled` (accessNotConfigured)

**สาเหตุ:** Google Sheets API ยังไม่ถูก Enable ในโปรเจกต์ที่สร้าง service account

**วิธีแก้:** Cloud Console → APIs & Services → Library → ค้น `Google Sheets API` → **Enable**
ตรวจด้วยว่ากำลังดู "โปรเจกต์ถูกตัว" (dropdown มุมบนซ้าย) — key ของโปรเจกต์ A ใช้ API ที่เปิดในโปรเจกต์ B ไม่ได้

## 4) `Google credentials file not found` / `credentials file was not found`

**สาเหตุ:** หาไฟล์ `credentials.json` ไม่เจอตาม 3 ตำแหน่งที่แอปค้น (ดู [ลำดับการค้นหา](02-architecture.md#ลำดับการค้นหาไฟล์-credentials))

**วิธีแก้:** วางไฟล์ที่ `src\GoogleSheetsDemo\credentials.json` (จุดมาตรฐาน — build จะ copy ลง bin\Debug ให้เอง)
หรือใส่ absolute path เต็ม ๆ ใน App.config key `GoogleCredentialsPath`

**กับดัก:** ไฟล์ชื่ออื่นอยู่ (เช่นยังไม่ได้เปลี่ยนจากชื่อยาวที่ดาวน์โหลดมา) — ต้องชื่อตรงกับ config

## 5) `401 ... invalid_grant` หรือ `Invalid JWT Signature`

**สาเหตุที่พบบ่อย (เรียงตามลำดับ):**
1. key ถูกลบ/เพิกถอนไปแล้วใน Cloud Console (ไฟล์ JSON ยังอยู่แต่ไม่มีผล)
2. นาฬิกาเครื่องคลาดจากจริงมาก ๆ (JWT มี timestamp เช็กทั้งสองฝั่ง)
3. ไฟล์ JSON โดนตัดต่อจนเสียหาย (บันทึกทับด้วย encoding อื่น เว้นวรรคใน private key ฯลฯ)

**วิธีแก้:** ลบ key เก่าในแท็บ Keys ของ service account → สร้าง key JSON ใหม่ → ทับไฟล์เดิม
(ห้ามแก้ไฟล์ JSON ด้วยมือ) ถ้าสงสัยนาฬิกา: Settings → Time & language → Sync now

## 6) `Spreadsheet ID is empty` / ยิงไปแล้ว `404 Requested entity was not found` (ตอนใช้ ID จริง)

**สาเหตุ:** ช่อง ID ว่างเปล่า หรือ ID ผิด (คัดลอกไม่ครบ / ติด `/edit` มาด้วย / ชีตถูกลบ / ID ของชีตที่ไม่ได้แชร์กับบัญชีนี้
— กรณีสุดท้าย Google จะตอบ 404 เพื่อไม่เปิดเผยว่าไฟล์มีอยู่)

**วิธีแก้:** คัดลอกใหม่จาก URL ชีตเฉพาะส่วนระหว่าง `/d/` กับ `/edit` วางในช่อง Spreadsheet ID
ของแอป (แก้สดได้เลยไม่ต้อง restart) — ถ้าชีตโดนลบให้สร้างใหม่และแชร์ใหม่ตาม [docs/01](01-google-cloud-setup.md)

## 7) เปิด/แชร์ชีตในเบราว์เซอร์แล้วเจอ `Error 400 (Bad Request)` จาก `docs.google.com/accounts/SetOSID`

**สาเหตุ:** cookie ของ Google ในเบราว์เซอร์เสีย/ชนกับ session อื่น — เป็นปัญหาฝั่งเบราว์เซอร์ ไม่เกี่ยวกับแอป
(เจอจริงในเครื่องนี้ตอน 6 ก.ย. 2026)

**วิธีแก้ที่ได้ผล (เรียงตามง่ายสุด):**
1. พิมพ์ URL ชีตลงแถบที่อยู่ตรง ๆ (`https://docs.google.com/spreadsheets/d/<ID>/edit`) ข้ามหน้า redirect
2. เปิดหน้าต่างไม่ระบุตัวตน (Ctrl+Shift+N) แล้วล็อกอินใหม่
3. ล้าง cookie เฉพาะ `google.com` (ตั้งค่า Chrome → ความเป็นส่วนตัว → Cookie → ดูข้อมูลและสิทธิ์ทั้งหมด)
4. ออกจากบัญชีแล้วล็อกอินกลับ

## 8) ปุ่มกดแล้วเงียบ / `Save failed` เป็นพัก ๆ

- **network ล่ม/VPN/พร็อกซีบล็อก** `sheets.googleapis.com` → ตรวจด้วยการเปิด `https://sheets.googleapis.com`
  ในเบราว์เซอร์ (ต้องขึ้นหน้า 404 ของ Google แปลว่าเน็ตถึง)
- **โดน rate limit** (error มีคำว่า `quota` / `RESOURCE_EXHAUSTED` 429) — demo ยิงถี่ก็ไม่น่าถึง
  ลองใหม่หลังพัก 1 นาที
- แอปเปิดค้างไว้นานมาก → ปิดเปิดใหม่ (token ต่ออายุอัตโนมัติ แต่ network state ค้างได้)

## 9) ข้อมูลหายไปบางแถวหลังกด Load

ไม่ใช่บั๊ก — ตามกฎของ [SheetRowMapper](02-architecture.md#กฎการ-parse-ของ-sheetrowmapper):
แถวที่ **ID ไม่ใช่ตัวเลข** หรือ **ชื่อว่าง** จะถูกกรองทิ้ง ดูแถวหายในชีตตรง ๆ แล้วจะเจอว่าเซลล์ A หรือ B มีปัญหา

## 10) ตัวเลขในชีตโดน Google format จน parse เพี้ยน

Google เก็บค่าที่แสดงจริงในเซลล์ (formatted value) เช่น `1,234.50` — แอป parse ได้เพราะใช้
`NumberStyles.Any` แต่ถ้าเจอ format แปลก ๆ (เว้นวรรคเป็นพัน ฯลฯ) ให้ตั้ง Format → Number → ปกติในชีต

## 11) ปัญหาฝั่ง Build/Test (ไม่ใช่ runtime)

| อาการ | สาเหตุ/วิธีแก้ |
| --- | --- |
| Build ไม่ผ่าน `WindowsBase`/`net461` ไม่พบ | ยังไม่ได้ติดตั้ง workload ".NET desktop development" ของ VS2017 |
| NuGet restore ไม่ได้ / package version error | **ห้ามอัปเดต Google.Apis** เกิน 1.55 (เครื่องนี้ build ไม่ไหว) ใช้ `packages.config` ที่ให้มาตามเดิม |
| Build ติด `...exe is locked / being used` | แอปกำลังรันอยู่ (หรือ debug session เปิดค้างใน VS) — ปิดแอป/หยุด debug ก่อน build |
| Test Explorer ไม่ขึ้นเทส | Build solution ก่อน แล้วกด refresh ใน Test Explorer; หรือรัน vstest.console ตาม README |
| แก้ App.config แล้วแอปยังใช้ค่าเก่า | แอปอ่านค่าตอน start — restart แอป (เทคนิคลัด: ลอก App.config ทับ `bin\Debug\GoogleSheetsDemo.exe.config` แล้วปิดเปิดแอป) |
| เปิดแอปจาก exe ตรง ๆ แล้ว error ที่ VS รันได้ | เทียบ config ใน `bin\Debug\GoogleSheetsDemo.exe.config` กับ `App.config` — อาจไม่ sync กัน (build ใหม่จะลอกให้เอง) |

## หลักคิดตอนเจอ error ใหม่ ๆ ที่ไม่อยู่ในนี้

1. อ่าน**ข้อความหลัง `failed:`** ใน status bar — มันคือ `ex.Message` ของ exception ชั้นนอกสุด
2. รัน `tools\ConnectTest.exe` เพื่อดู exception chain เต็ม ๆ จากคอนโซล (รวม HTTP body ของ Google)
3. เทียบผลกับต้นไม้การวินิจฉัยใน [docs/04-connect-test.md](04-connect-test.md)
4. error ที่มีเลข 403/404/401 = ฝั่ง Google (สิทธิ์/ไฟล์/ตัวตน) — error แบบ `Unable to parse` = ฝั่ง config ของเรา
