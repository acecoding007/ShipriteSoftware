Imports System.Drawing
Imports System.Drawing.Printing
Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Text

Public Module _Printers

    Private Sub error_DebugPrint(ByVal routineName As String, ByVal errorDesc As String)
        _Debug.PrintError_(String.Format("_Printers.{0}(): {1}", routineName, errorDesc))
    End Sub

    Public Function Is_DefaultPrinter(ByVal printername As String) As Boolean
        Is_DefaultPrinter = False
        Try
            Dim printer As New Printing.PrinterSettings
            printer.PrinterName = printername
            Is_DefaultPrinter = printer.IsDefaultPrinter
        Catch ex As Exception : error_DebugPrint("Is_DefaultPrinter", ex.Message)
        End Try
    End Function
    Public Function IsValid_PrinterName(ByVal printerName As String) As Boolean
        IsValid_PrinterName = False ' assume.
        Try
            For i = 0 To PrinterSettings.InstalledPrinters.Count - 1
                If printerName = PrinterSettings.InstalledPrinters.Item(i) Then
                    IsValid_PrinterName = True
                    Exit For
                End If
            Next i
        Catch ex As Exception : error_DebugPrint("IsValid_PrinterName", ex.Message)
        End Try
    End Function

    Public Function Set_DefaultPrinter(ByVal printername As String) As Boolean
        Set_DefaultPrinter = False
        Try
            Interaction.Shell(String.Format("rundll32 printui.dll,PrintUIEntry /y /n ""{0}""", printername))
            Set_DefaultPrinter = Is_DefaultPrinter(printername)
        Catch ex As Exception : error_DebugPrint("Set_DefaultPrinter", ex.Message)
        End Try
    End Function
    Public Function Get_DefaultPrinter(ByRef printername As String) As Boolean
        Get_DefaultPrinter = False
        printername = String.Empty ' assume.
        Try
            For Each printer As String In Printing.PrinterSettings.InstalledPrinters
                Get_DefaultPrinter = Is_DefaultPrinter(printer)
                If Get_DefaultPrinter Then
                    printername = printer
                    Exit For
                End If
            Next
        Catch ex As Exception : error_DebugPrint("Get_DefaultPrinter", ex.Message)
        End Try
    End Function
    Public Function Get_DefaultPrinter() As String
        Get_DefaultPrinter = String.Empty ' assume.
        Try
            For Each printer As String In Printing.PrinterSettings.InstalledPrinters
                If Is_DefaultPrinter(printer) Then
                    Get_DefaultPrinter = printer
                    Exit For
                End If
            Next
        Catch ex As Exception : error_DebugPrint("Get_DefaultPrinter", ex.Message)
        End Try
    End Function

End Module

Public Class _PrinterForAJob
    Public JobName As String

    Public LaserPrinter_Enabled As Boolean
    Public LabelPrinter_Enabled As Boolean
    Public ReceiptPrinter_Enabled As Boolean

    Public LaserPrinter_Name As String
    Public LabelPrinter_Name As String
    Public ReceiptPrinter_Name As String

    Public LaserPrinter_Copies As Integer
    Public LabelPrinter_Copies As Integer
    Public ReceiptPrinter_Copies As Integer

    Public IsApplySettings As Boolean
End Class

Public Class PrintHelper
    Friend TextToBePrinted As List(Of String)
    Friend PrinterName As String
    Friend PrintJobName As String
    Friend PrintFont As Font
    Friend PrintFontFamilyName As String = "Consolas"
    Friend PrintFontSize As Single = 9
    Friend PrintFontStyle As FontStyle = FontStyle.Regular
    Private m_IsFireDrawer As Boolean = False
    Friend ReadOnly Property IsFireDrawer As Boolean
        Get
            Return m_IsFireDrawer
        End Get
    End Property
    Private m_FireDrawerCode As String = ""
    Private m_FireDrawerCharCode As String = ""
    Friend Property FireDrawerCode As String
        Get
            Return m_FireDrawerCode.ToString
        End Get
        Set(value As String)
            m_IsFireDrawer = False
            If value IsNot Nothing AndAlso value.Length > 0 Then
                Dim codeArr() As String = value.Split(",")
                Dim code As String = ""
                For i = 0 To codeArr.GetUpperBound(0)
                    If Val(codeArr(i)) >= 0 And Val(codeArr(i)) <= 255 Then
                        code &= Chr(Val(codeArr(i)))
                    Else
                        code &= codeArr(i)
                    End If
                Next
                If code.Length > 0 Then
                    m_FireDrawerCode = value
                    m_FireDrawerCharCode = code
                    m_IsFireDrawer = True
                End If
            End If
        End Set
    End Property
    Friend ReadOnly Property FireDrawerCharCode As String
        Get
            Return m_FireDrawerCharCode
        End Get
    End Property

    Private WithEvents PrintDoc As Printing.PrintDocument
    Private Delegate Sub PrintDoc2(ByVal sender As Object, ByVal args As Printing.PrintPageEventArgs)
    Private PrintArgs As Printing.PrintPageEventArgs

    Public Function prt(ByVal text As List(Of String), ByVal printer As String) As Integer

        Dim PrtStatus As Integer = 0

        TextToBePrinted = text
        '
        PrintFont = New Font(PrintFontFamilyName, PrintFontSize, PrintFontStyle)
        '
        PrintDoc = New Printing.PrintDocument

        Using (PrintDoc)

            If PrintJobName IsNot Nothing AndAlso PrintJobName.Length > 0 Then
                PrintDoc.DocumentName = PrintJobName
            End If

            Try

                PrintDoc.PrinterSettings.PrinterName = printer
                PrintDoc.Print()

            Catch ex As Exception

                MsgBox(Err.Number & "-" & Err.Description)
                PrtStatus = 1

            End Try

        End Using
        Return PrtStatus

    End Function

    Private Sub PrintPageHandler(ByVal sender As Object, ByVal args As Printing.PrintPageEventArgs) Handles PrintDoc.PrintPage
        'Dim reportfont As String = String.Empty
        'Dim reportsize As Single = 0

        'Call ReportsDb.Get_ReceiptPrinterFontName(reportfont)
        'Call ReportsDb.Get_ReceiptPrinterFontSize(reportsize)

        PrintFont = New Font(PrintFontFamilyName, PrintFontSize, PrintFontStyle)
        Dim yPos As Single = 5

        For i As Integer = 0 To TextToBePrinted.Count - 1
            args.Graphics.DrawString(TextToBePrinted(i), PrintFont, Brushes.Black, 5, yPos)
            yPos += PrintFontSize
        Next

        'args.Graphics.DrawString(TextToBePrinted, PrintFont, Brushes.Black, 5, 5)
        'args.Graphics.DrawString("Test In Handler", PrintFont, Brushes.Black, 5, 50)

    End Sub

End Class

Public Module _PrintReceipt

    'Public Sub PrintPreview_FromFile(ByVal filename As String, ByVal printerName As String, ByVal is2preview As Boolean, ByVal msg2show As Boolean)
    '    If is2preview Then
    '        preview_FromFile(filename, printerName, msg2show)
    '        ReceiptPrinting.Preview.ShowDialog()
    '    Else
    '        print_FromFile(filename, printerName, msg2show)
    '    End If
    'End Sub

    'Private Sub preview_FromFile(ByVal filename As String, ByVal printerName As String, ByVal msg2show As Boolean)
    '    Dim file2str As String = String.Empty
    '    If _Files.ReadFile_ToEnd(filename, msg2show, file2str) Then
    '        ReceiptPrinting.Preview.txtPreview.Text = file2str
    '        ReceiptPrinting.Preview.txtPreview.Tag = printerName
    '    End If
    'End Sub

    Public Sub Print_FromFile(ByVal filename As String, ByVal printerName As String, ByVal msg2show As Boolean)
        Dim originalDefaultPrinterName As String = _Printers.Get_DefaultPrinter()
        If Not originalDefaultPrinterName = printerName Then
            ' set as a default printer to re-set back to original one later.
            _Printers.Set_DefaultPrinter(printerName)
        End If
        ''
        Dim prter As New PrintHelper
        Dim file2str As String = String.Empty
        Dim text2print As New List(Of String)
        If _Files.ReadFile_ToEnd(filename, msg2show, file2str) Then
            text2print.Add(file2str)
            prter.prt(text2print, printerName)
        End If
        ''
        If Not originalDefaultPrinterName = printerName Then
            ' set default back to original default printer.
            _Printers.Set_DefaultPrinter(originalDefaultPrinterName)
        End If
    End Sub

    Public Function Print_FromTextAndImage(ByVal text2print As String, ByVal printerName As String, ByVal imagePath As String, Optional ByVal pSettings As PrintHelper = Nothing) As Integer
        Dim originalDefaultPrinterName As String = _Printers.Get_DefaultPrinter()
        Dim result As Integer = 0

        ' Set the specified printer as the default if it's different from the original default printer
        If Not originalDefaultPrinterName = printerName Then
            _Printers.Set_DefaultPrinter(printerName)
        End If

        Dim prter As PrintHelper = If(pSettings Is Nothing, New PrintHelper(), pSettings)

        If prter.IsFireDrawer Then
            RawPrinterHelper.SendStringToPrinter(printerName, prter.FireDrawerCharCode)
        End If

        ' Create a PrintDocument object
        Dim printDoc As New PrintDocument()
        printDoc.PrinterSettings.PrinterName = printerName
        printDoc.DocumentName = prter.PrintJobName

        ' Define event handler for PrintPage event
        AddHandler printDoc.PrintPage, Sub(sender As Object, e As PrintPageEventArgs)
                                           ' Use settings from pSettings if available
                                           Dim fontFamilyName As String = If(String.IsNullOrEmpty(prter.PrintFontFamilyName), "Consolas", prter.PrintFontFamilyName)
                                           Dim fontSize As Single = If(prter.PrintFontSize <= 0, 9, prter.PrintFontSize)
                                           Dim fontStyle As FontStyle = If(prter.PrintFontStyle = 0, FontStyle.Regular, prter.PrintFontStyle)
                                           Dim printFont As New Font(fontFamilyName, fontSize, fontStyle)

                                           ' Print image
                                           Dim img As Image = Image.FromFile(imagePath)
                                           Dim imagePosition As New PointF(3, 10)
                                           e.Graphics.DrawImage(img, imagePosition)

                                           ' Calculate position for the text below the image
                                           Dim imageSize As SizeF = New SizeF(img.Width, img.Height)
                                           Dim textPosition As New PointF(3, imagePosition.Y + imageSize.Height + 20)

                                           ' Print text
                                           e.Graphics.DrawString(text2print, printFont, Brushes.Black, textPosition)
                                       End Sub

        ' Print the document
        Try
            printDoc.Print()
            result = 1
        Catch ex As Exception
            result = 0
        End Try

        ' Restore the original default printer if the specified printer was changed
        If Not originalDefaultPrinterName = printerName Then
            _Printers.Set_DefaultPrinter(originalDefaultPrinterName)
        End If

        Return result
    End Function

    Public Function Print_FromText(ByVal text2print As String, ByVal printerName As String, Optional ByVal pSettings As PrintHelper = Nothing) As Integer

        Dim originalDefaultPrinterName As String = _Printers.Get_DefaultPrinter()
        Dim result As Integer = 0
        If Not originalDefaultPrinterName = printerName Then
            ' set as a default printer to re-set back to original one later.
            _Printers.Set_DefaultPrinter(printerName)
        End If
        ''
        Dim prter As PrintHelper
        If pSettings Is Nothing Then
            prter = New PrintHelper
        Else
            prter = pSettings
        End If

        If prter.IsFireDrawer Then
            RawPrinterHelper.SendStringToPrinter(printerName, prter.FireDrawerCharCode)
        End If

        Dim text2printList As New List(Of String)
        text2printList.Add(text2print)
        result = prter.prt(text2printList, printerName)
        If result = 1 Then

            Return 1
            Exit Function

        End If

        ''
        If Not originalDefaultPrinterName = printerName Then
            ' set default back to original default printer.
            _Printers.Set_DefaultPrinter(originalDefaultPrinterName)
        End If
        Return 0

    End Function
    Public Sub Print_FromText(ByVal text2print As List(Of String), ByVal printerName As String, Optional ByVal pSettings As PrintHelper = Nothing)
        Dim originalDefaultPrinterName As String = _Printers.Get_DefaultPrinter()
        If Not originalDefaultPrinterName = printerName Then
            ' set as a default printer to re-set back to original one later.
            _Printers.Set_DefaultPrinter(printerName)
        End If
        ''
        Dim prter As PrintHelper
        If pSettings Is Nothing Then
            prter = New PrintHelper
        Else
            prter = pSettings
        End If

        prter.prt(text2print, printerName)
        If prter.IsFireDrawer Then
            RawPrinterHelper.SendStringToPrinter(printerName, prter.FireDrawerCharCode)
        End If
        ''
        If Not originalDefaultPrinterName = printerName Then
            ' set default back to original default printer.
            _Printers.Set_DefaultPrinter(originalDefaultPrinterName)
        End If
    End Sub

End Module

Public Class RawPrinterHelper
    ' Structure and API declarions:
    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)>
    Structure DOCINFOW
        <MarshalAs(UnmanagedType.LPWStr)> Public pDocName As String
        <MarshalAs(UnmanagedType.LPWStr)> Public pOutputFile As String
        <MarshalAs(UnmanagedType.LPWStr)> Public pDataType As String
    End Structure

    <DllImport("winspool.Drv", EntryPoint:="OpenPrinterW",
       SetLastError:=True, CharSet:=CharSet.Unicode,
       ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Public Shared Function OpenPrinter(ByVal src As String, ByRef hPrinter As IntPtr, ByVal pd As Int32) As Boolean
    End Function
    <DllImport("winspool.Drv", EntryPoint:="ClosePrinter",
       SetLastError:=True, CharSet:=CharSet.Unicode,
       ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Public Shared Function ClosePrinter(ByVal hPrinter As IntPtr) As Boolean
    End Function
    <DllImport("winspool.Drv", EntryPoint:="StartDocPrinterW",
       SetLastError:=True, CharSet:=CharSet.Unicode,
       ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Public Shared Function StartDocPrinter(ByVal hPrinter As IntPtr, ByVal level As Int32, ByRef pDI As DOCINFOW) As Boolean
    End Function
    <DllImport("winspool.Drv", EntryPoint:="EndDocPrinter",
       SetLastError:=True, CharSet:=CharSet.Unicode,
       ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Public Shared Function EndDocPrinter(ByVal hPrinter As IntPtr) As Boolean
    End Function
    <DllImport("winspool.Drv", EntryPoint:="StartPagePrinter",
       SetLastError:=True, CharSet:=CharSet.Unicode,
       ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Public Shared Function StartPagePrinter(ByVal hPrinter As IntPtr) As Boolean
    End Function
    <DllImport("winspool.Drv", EntryPoint:="EndPagePrinter",
       SetLastError:=True, CharSet:=CharSet.Unicode,
       ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Public Shared Function EndPagePrinter(ByVal hPrinter As IntPtr) As Boolean
    End Function
    <DllImport("winspool.Drv", EntryPoint:="WritePrinter",
       SetLastError:=True, CharSet:=CharSet.Unicode,
       ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Public Shared Function WritePrinter(ByVal hPrinter As IntPtr, ByVal pBytes As IntPtr, ByVal dwCount As Int32, ByRef dwWritten As Int32) As Boolean
    End Function

    ' SendBytesToPrinter()
    ' When the function is given a printer name and an unmanaged array of  
    ' bytes, the function sends those bytes to the print queue.
    ' Returns True on success or False on failure.
    Public Shared Function SendBytesToPrinter(ByVal szPrinterName As String, ByVal pBytes As IntPtr, ByVal dwCount As Int32) As Boolean
        Dim hPrinter As IntPtr      ' The printer handle.
        Dim dwError As Int32        ' Last error - in case there was trouble.
        Dim di As DOCINFOW          ' Describes your document (name, port, data type).
        Dim dwWritten As Int32      ' The number of bytes written by WritePrinter().
        Dim bSuccess As Boolean     ' Your success code.

        ' Set up the DOCINFO structure.
        di = Nothing ' assume.
        With di
            .pDocName = "My Visual Basic .NET RAW Document"
            .pDataType = "RAW"
        End With
        ' Assume failure unless you specifically succeed.
        bSuccess = False
        If OpenPrinter(szPrinterName, hPrinter, 0) Then
            If StartDocPrinter(hPrinter, 1, di) Then
                If StartPagePrinter(hPrinter) Then
                    ' Write your printer-specific bytes to the printer.
                    bSuccess = WritePrinter(hPrinter, pBytes, dwCount, dwWritten)
                    EndPagePrinter(hPrinter)
                End If
                EndDocPrinter(hPrinter)
            End If
            ClosePrinter(hPrinter)
        End If
        ' If you did not succeed, GetLastError may give more information
        ' about why not.
        If bSuccess = False Then
            dwError = Marshal.GetLastWin32Error()
        End If
        Return bSuccess
    End Function ' SendBytesToPrinter()

    ' SendFileToPrinter()
    ' When the function is given a file name and a printer name, 
    ' the function reads the contents of the file and sends the
    ' contents to the printer.
    Public Shared Function SendFileToPrinter(ByVal szPrinterName As String, ByVal szFileName As String) As Boolean
        Dim sb As String ' New StringBuilder()
        Using sr = New StreamReader(szFileName, Encoding.[Default])
            ' Set the correct encoding
            sb = sr.ReadToEnd
            'While Not sr.EndOfStream
            '    ' This will automatically fix the last line
            '    sb.AppendLine(sr.ReadLine())
            'End While
        End Using
        Return RawPrinterHelper.SendStringToPrinter(szPrinterName, sb)
    End Function

    ' When the function is given a string and a printer name,
    ' the function sends the string to the printer as raw bytes.
    Public Shared Function SendStringToPrinter(ByVal szPrinterName As String, ByVal szString As String) As Boolean
        Dim pBytes As IntPtr
        Dim dwCount As Int32
        Dim bSuccess As Boolean

        ' How many characters are in the string?
        dwCount = szString.Length()
        ' Assume that the printer is expecting ANSI text, and then convert
        ' the string to ANSI text.
        pBytes = Marshal.StringToCoTaskMemAnsi(szString)
        ' Send the converted ANSI string to the printer.
        bSuccess = SendBytesToPrinter(szPrinterName, pBytes, dwCount)
        Marshal.FreeCoTaskMem(pBytes)
        Return bSuccess
    End Function
End Class