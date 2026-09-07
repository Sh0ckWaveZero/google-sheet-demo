' Standalone connectivity probe: calls the real Google Sheets API with the
' same credentials + spreadsheet id the app uses, and prints the full error
' chain so runtime failures can be diagnosed from the console.
Imports System
Imports System.Reflection

Namespace ConnectTest

    Module Program

        Function Main(args As String()) As Integer
            Dim credentialsPath As String = If(args.Length > 0, args(0),
                "C:\lisa\Demo_Spread_Sheet\src\GoogleSheetsDemo\bin\Debug\credentials.json")
            Dim spreadsheetId As String = If(args.Length > 1, args(1), "1SbOWOfgu2R4cg_PmNtFQA7RxoryUUI_UFsj5fZGIFbs")
            Dim range As String = If(args.Length > 2, args(2), ChrW(&HE0A) & ChrW(&HE35) & ChrW(&HE15) & "1!A1:D5") ' "Sheet1" in Thai

            Console.WriteLine("Credentials : " + credentialsPath)
            Console.WriteLine("Spreadsheet : " + spreadsheetId)
            Console.WriteLine("Range       : " + range)
            Console.WriteLine()

            Try
                Dim asm = Assembly.LoadFrom(
                    "C:\lisa\Demo_Spread_Sheet\src\GoogleSheetsDemo\bin\Debug\GoogleSheetsDemo.exe")
                Dim svcType = asm.GetType("GoogleSheetsDemo.Services.GoogleSheetService", True)
                Dim svc = Activator.CreateInstance(svcType,
                    spreadsheetId, credentialsPath, "GoogleSheetsDemo-ConnectTest")

                Dim method = svcType.GetMethod("GetValuesAsync")
                Dim task = DirectCast(method.Invoke(svc, New Object() {range}),
                    System.Threading.Tasks.Task(Of System.Collections.Generic.IList(Of System.Collections.Generic.IList(Of Object))))

                Dim rows = task.GetAwaiter().GetResult()
                Console.WriteLine("SUCCESS - got " + rows.Count.ToString() + " row(s):")
                For Each row In rows
                    Console.WriteLine("  [" + String.Join(" | ", row) + "]")
                Next
                Return 0
            Catch ex As Exception
                Dim depth As Integer = 0
                Dim cur As Exception = ex
                Do While cur IsNot Nothing
                    Console.WriteLine(If(depth = 0, "EXCEPTION", "  INNER " + depth.ToString()) + ": [" +
                        cur.GetType().FullName + "] " + cur.Message)
                    ' GoogleApiException carries the raw HTTP response body
                    Dim curType As Type = cur.GetType()
                    Dim respProp As PropertyInfo = curType.GetProperty("HttpResponse")
                    If respProp Is Nothing AndAlso curType.BaseType IsNot Nothing Then
                        respProp = curType.BaseType.GetProperty("HttpResponse")
                    End If
                    If respProp IsNot Nothing Then
                        Dim resp = respProp.GetValue(cur, Nothing)
                        Console.WriteLine("  HttpResponse: " + resp.ToString())
                    End If
                    depth += 1
                    cur = cur.InnerException
                Loop
                Console.WriteLine()
                Console.WriteLine(ex.StackTrace)
                Return 1
            End Try
        End Function

    End Module

End Namespace
