Imports System.IO.Compression

Public Class SupportUtilities
    Inherits CommonWindow

    Private Class SQL_ViewItem
        Public Property LineNo As String
        Public Property Segment As String
    End Class

    Private Class DBRecord_ViewItem
        Public Property FieldName As String
        Public Property Content As String
    End Class

    Dim SQL_View_List As List(Of SQL_ViewItem)

    Public Sub New()

        MyBase.New()

        ' This call is required by the designer.
        InitializeComponent()

    End Sub

    Public Sub New(ByVal callingWindow As Window)

        MyBase.New(callingWindow)

        ' This call is required by the designer.
        InitializeComponent()

    End Sub
    Private Function SetBalancesInPayments(DoOnlyAccount As String) As Integer

        Dim BalanceRecordSet As RecordSetDefinition
        Dim RecordCT As Long
        Dim SQL As String
        Dim Balance As Double
        Dim i As Long
        Dim InvNum As String
        Dim ret As Long

        Mouse.OverrideCursor = Cursors.Wait

        If DoOnlyAccount = "" Then

            SQL = "UPDATE Payments SET Balance = 0"

        Else

            SQL = "UPDATE Payments SET Balance = 0 WHERE AcctNum = '" & DoOnlyAccount & "'"

        End If
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

        If DoOnlyAccount = "" Then

            SQL = "SELECT InvNum, SUM(Charge - Payment) AS Balance FROM Payments GROUP BY InvNum"

        Else

            SQL = "SELECT InvNum, SUM(Charge - Payment) AS Balance FROM Payments WHERE AcctNum = '" & DoOnlyAccount & "' GROUP BY InvNum"

        End If
        RecordCT = IO_GetSegmentSetInToStructure(gShipriteDB, SQL, BalanceRecordSet)

        For i = 0 To RecordCT - 1

            InvNum = BalanceRecordSet.RecordSet(i).Field(0).FValue
            Balance = Round(Val(BalanceRecordSet.RecordSet(i).Field(1).FValue), 2)
            If Not Balance = 0 Then

                SQL = "UPDATE Payments SET Balance = " & Balance & " WHERE InvNum = '" & InvNum & "'"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

            End If

        Next
        Mouse.OverrideCursor = Cursors.Arrow

    End Function
    Private Sub SupportUtilities_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded

        If (gIsProgramSecurityEnabled Or gIsSetupSecurityEnabled) AndAlso Not Check_Current_User_Permission("SETUP") Then
            Me.Close()
        End If

        ConnectionString.Text = gShipriteDB
        SQL_Statement.Text = gLastSQL
        SQL_Results_LV.Visibility = Visibility.Hidden
        DBRecord_LV.Visibility = Visibility.Hidden
    End Sub

    Private Sub Process_UpdateACCDB_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Process_UpdateACCDB_Btn.Click
        Try
            Dim fromDbPath As String = gShipriteDB
            Dim toDbPath As String = gDBpath & "\Update.accdb"
            Dim toDbProcessPath As String = gDBpath & "\Update-Process.accdb"

            If _Files.IsFileExist(fromDbPath, True) AndAlso _Files.IsFileExist(toDbPath, True) Then
                Dim bAns As Boolean = _MsgBox.QuestionMessage("Close Shiprite Next on all other workstations before proceeding.", "Process Update.accdb", "Continue?")
                If bAns Then
                    ' make process copy of db for use.
                    If _Files.CopyFile_ToNewFolder(toDbPath, toDbProcessPath, True) Then
                        ' replace extension with backup datetime and .zip
                        Dim fromDbBackupPath As String = fromDbPath.Substring(0, fromDbPath.LastIndexOf(".")) & "-Backup-" & Now.ToString("yyyyMMddHHmmss") & ".zip"
                        ' make backup of srn db
                        Using zip As ZipArchive = ZipFile.Open(fromDbBackupPath, ZipArchiveMode.Create)
                            zip.CreateEntryFromFile(gShipriteDB, "ShipriteNext.accdb")
                        End Using
                        ' convert
                        ShipriteStartup.ConvertUtility_Run(fromDbPath, toDbProcessPath, "ShipriteNext")
                        _MsgBox.InformationMessage("Operation Complete")
                    End If
                End If
            End If
        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Creating Update.accdb Database")
        End Try
    End Sub

    Private Sub Process_SQL_Click(sender As Object, e As RoutedEventArgs) Handles Process_SQL.Click

        Dim SQLDBPath As String
        Dim SQL As String
        Dim Segment As String
        Dim SegmentSet As String
        Dim ret As Long
        Dim buf As String
        Dim xbuf As String
        Dim iloc As Integer
        Dim OtherDB As String
        Dim LineCT As Long
        Dim ExportFilePath As String = ""
        Dim fnum As Integer
        Dim eName As String = ""
        Dim eValue As String = ""
        Dim Header As String = ""
        Dim i As Integer = 0
        Dim k As Integer = 0

        If Not ExportToFile.Text = "" Then

            ExportFilePath = gAppPath & "\" & ExportToFile.Text & ".csv"
            buf = Dir(ExportFilePath)
            If Not buf = "" Then

                FileSystem.Kill(ExportFilePath)

            End If

        End If
        gAzureConnectionString = ConnectionString.Text
        If InStr(1, UCase(SQL_Statement.Text), "SELECT") = 0 Then

            ret = MsgBox("ATTENTION...These Changes will be irreversable!!!" & vbCrLf & vbCrLf & "Do you have a backup????", vbQuestion + vbYesNo, gProgramName)
            If ret = vbNo Then

                Exit Sub

            End If
            ret = UpdateRunTimePolicy(gGLOBALpolicy, "HoldSQL", ExtractElementFromSegment("SQLStatement", CallingStack.result))

        Else

            ret = UpdateRunTimePolicy(gGLOBALpolicy, "HoldSQL", ExtractElementFromSegment("SQLStatement", CallingStack.result))

        End If
        buf = Trim$(GetRunTimePolicy(gGLOBALpolicy, "HoldSQL"))
        If Mid$(buf, 1, 1) = "[" Then

            iloc = InStr(1, buf, "]")
            If Not iloc = 0 Then

                OtherDB = Trim$(Mid$(buf, 2, iloc - 2))
                SQL = Trim$(Mid$(buf, iloc + 1))
                SQLDBPath = ""

            Else

                MsgBox("ATTENTION...SQL format Error with '['" & vbCrLf & vbCrLf &
                  "A valid database path must be encapsulated between" & vbCrLf &
                  " a set of brackets [~~~~]" & vbCrLf & vbCrLf &
                  GetRunTimePolicy(gGLOBALpolicy, "HoldSQL"), vbInformation, gProgramName)
                Exit Sub

            End If

        Else

            OtherDB = ""
            SQLDBPath = ConnectionString.Text
            SQL = Trim(SQL_Statement.Text) ' Make sure to trim any excess whitespace.

        End If

        If UCase(Mid$(SQL, 1, 3)) = "SP_" Then

            'ret = NEW_Process_Stored_Procedures(gDBpath & "\" & SQL & ".sp")

        Else

            If InStr(UCase(SQL), "SELECT") = 0 Then ' If first word of sql statement is select then identify as select statement. NOT if select is contained somewhere in sql statement.

                ret = IO_UpdateSQLProcessor(SQLDBPath, SQL, ConnectionString.Text)
                MsgBox("***ROWS AFFECTED, " & ret & "***", vbInformation, gProgramName)

            Else

                If Not ExportFilePath = "" Then

                    fnum = FreeFile()
                    FileOpen(fnum, ExportFilePath, OpenMode.Append)

                End If

                SegmentSet = IO_GetSegmentSet(SQLDBPath, SQL, "", ConnectionString.Text)

                On Error Resume Next
                On Error GoTo 0

                If Not Err.Number = 0 Then

                    Exit Sub

                End If
                If Not SegmentSet = "" Then
                    SQL_View_List = New List(Of SQL_ViewItem)
                    Dim Item As SQL_ViewItem

                    LineCT = 1
                    i = 0
                    Do Until SegmentSet = ""

                        Item = New SQL_ViewItem
                        Segment = GetNextSegmentFromSet(SegmentSet)
                        If Not ExportFilePath = "" Then

                            xbuf = Segment
                            xbuf = xbuf.Replace(",", "^")  'We're making a CSV file so replace any comma's in the data with a wierd special character that can be replaced after the file is converted to an Excel Worksheet
                            buf = ""
                            Header = ""
                            k = 0
                            Do Until xbuf = ""

                                If Not k = 0 Then

                                    buf &= ","
                                    Header &= ","

                                End If
                                k += 1
                                xbuf = ExtractNextElementFromSegment(eName, eValue, xbuf)
                                buf &= eValue
                                If i = 0 Then

                                    Header &= eName

                                End If

                            Loop

                        End If
                        If Not ExportFilePath = "" Then

                            If i = 0 Then

                                FileSystem.Print(fnum, Header & vbCrLf)

                            End If
                            FileSystem.Print(fnum, buf & vbCrLf)
                            i += 1

                        End If
                        Segment = Segment.Replace(vbCrLf, " ")
                        Item.LineNo = LineCT
                        Item.Segment = Segment
                        LineCT = LineCT + 1
                        SQL_View_List.Add(Item)

                    Loop
                    FileSystem.FileClose(fnum)

                    SQL_Results_LV.Visibility = Visibility.Visible
                    DBRecord_LV.Visibility = Visibility.Visible
                    SQL_Results_LV.ItemsSource = SQL_View_List
                    SQL_Results_LV.Items.Refresh()

                Else

                    MsgBox("ATTENTION...No Results from Query", vbInformation, gProgramName)

                End If

            End If

        End If

        gLastSQL = SQL_Statement.Text
    End Sub

    Private Sub SQL_Results_LV_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles SQL_Results_LV.SelectionChanged
        If SQL_Results_LV.SelectedIndex = -1 Then
            Exit Sub
        End If

        Dim Segment As String = SQL_Results_LV.SelectedItem.Segment
        Dim item As DBRecord_ViewItem
        Dim DBRecord_List As List(Of DBRecord_ViewItem) = New List(Of DBRecord_ViewItem)

        Do Until Segment = ""
            item = New DBRecord_ViewItem

            Segment = ExtractNextElementFromSegment(item.FieldName, item.Content, Segment)

            DBRecord_List.Add(item)
        Loop

        DBRecord_LV.ItemsSource = DBRecord_List
        DBRecord_LV.Items.Refresh()

    End Sub

    Private Sub NormalizeInvoices_Btn_Click(sender As Object, e As RoutedEventArgs) Handles NormalizeInvoices_Btn.Click

        Dim ret As Integer
        Dim SQL As String
        Dim Segment As String
        Dim SegmentSet As String
        Dim AcctSegment As String
        Dim CurrentCredit As Double = 0.0
        Dim CurrentCreditInvNum As String = ""
        Dim CRSegmentSet As String
        Dim InvNum As String
        Dim InvNum2 As String
        Dim AcctNum As String
        Dim TDate As Date
        Dim amt As Double
        Dim DoOnlyAccount As String = ""

        Dim InvoiceRecordSet As RecordSetDefinition
        Dim RecordCT As Long = 0
        Dim i As Long = 0
        Dim j As Long = 0
        Dim Credit As Double = 0.0
        Dim InvBal As Double = 0.0


        Dim OriginalAR As Double
        Dim NewAR As Double

        ret = MsgBox("ATTENTION...This utility will apply the balance of all Invoices" & vbCrLf & "with credit balances to all invoices that do not" & vbCrLf & "balance to zero." & vbCrLf & vbCrLf & "CONTINUE???", vbQuestion + vbYesNo, "Shiprite NEXT")
        If ret = vbNo Then

            Exit Sub

        End If
        DoOnlyAccount = InputBox("Enter an Account Number or Blank for all...", "Normalizing Accounts", " ")
        If String.IsNullOrEmpty(DoOnlyAccount) Then

            Exit Sub

        End If
        DoOnlyAccount = Trim(DoOnlyAccount)

        Mouse.OverrideCursor = Cursors.Wait

        ' Get Rid of 'Deleted' status records

        SQL = "DELETE * FROM Payments WHERE Status = 'DELETED'"
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

        ' Fixing blank invoice numbers in payments

        SQL = "SELECT * FROM Payments WHERE (InvNum = '' OR ISNULL(InvNum)) AND [Type] = 'ADJUST' ORDER BY [Date], ID"
        SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
        ret = 0
        Do Until SegmentSet = ""

            Segment = GetNextSegmentFromSet(SegmentSet)
            Segment = RemoveBlankElementsFromSegment(Segment)
            Segment = AddElementToSegment(Segment, "InvNum", GetNextInvoiceNumber().ToString)
            SQL = MakeUpdateSQLFromSchema("Payments", Segment, PaymentsSchema)
            ret += IO_UpdateSQLProcessor(gShipriteDB, SQL)

        Loop
        SQL = "SELECT AcctNum, [Date] FROM Payments WHERE InvNum = '' OR ISNULL(InvNum) GROUP BY AcctNum, [Date]"
        SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
        ret = 0
        Do Until SegmentSet = ""

            Segment = GetNextSegmentFromSet(SegmentSet)
            AcctNum = ExtractElementFromSegment("AcctNum", Segment)
            TDate = ExtractElementFromSegment("Date", Segment)
            InvNum = GetNextInvoiceNumber()
            SQL = "UPDATE Payments SET InvNum = '" & InvNum & "', NumericInvoiceNumber = " & InvNum & " WHERE AcctNum = '" & AcctNum & "' AND (InvNum = '' OR ISNULL(InvNum)) AND Date = #" & Format(TDate, "MM/dd/yyyy") & "#"
            ret += IO_UpdateSQLProcessor(gShipriteDB, SQL)

        Loop

        ' Fixing Blank NumericInvoiceNumber

        SQL = "UPDATE Payments Set NumericInvoiceNumber = InvNum WHERE NumericInvoiceNumber = 0 or ISNULL(NumericInvoiceNumber)"
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

        ' Fixing Blank ShipTo

        SQL = "UPDATE Payments Set ShipTo = SoldTo WHERE ShipTo = 0 or ISNULL(ShipTo)"
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

        If DoOnlyAccount = "" Then

            SQL = "DELETE * FROM Payments WHERE [Desc] LIKE 'Balance X%'"
            ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
            SQL = "SELECT AcctNum FROM AR ORDER BY AcctNum"

        Else

            SQL = "DELETE * FROM Payments WHERE AcctNum = '" & DoOnlyAccount & "' AND [Desc] LIKE 'Balance X%'"
            ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
            SQL = "SELECT AcctNum FROM AR WHERE AcctNum = '" & DoOnlyAccount & "'"

        End If

        AcctSegment = IO_GetSegmentSet(gShipriteDB, SQL)

        ' Get Total of Store AR to match after utility completes

        If DoOnlyAccount = "" Then

            SQL = "SELECT  SUM(Charge) - SUM(Payment) as Balance FROM Payments WHERE NOT Status = 'VOIDED' AND NOT Status = 'DELETED'"

        Else

            SQL = "SELECT TOP 1 ID FROM AR WHERE AcctNum = '" & DoOnlyAccount & "'"
            SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
            If SegmentSet = "" Then

                MsgBox("ATTENTION...Unable to locate Account Number - " & DoOnlyAccount & vbCrLf & vbCrLf & "TRY AGAIN!", vbCritical)
                Exit Sub

            End If
            SQL = "SELECT  SUM(Charge) - SUM(Payment) as Balance FROM Payments WHERE AcctNum = '" & DoOnlyAccount & "' AND NOT Status = 'VOIDED' AND NOT Status = 'DELETED'"

        End If
        SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)

        OriginalAR = Round(Val(ExtractElementFromSegment("Balance", SegmentSet)), 2)

        Do Until AcctSegment = ""

            Segment = GetNextSegmentFromSet(AcctSegment)
            AcctNum = ExtractElementFromSegment("AcctNum", Segment)
            CRSegmentSet = ""
            CurrentCredit = 0
            SQL = "SELECT NumericInvoiceNumber, SUM(Charge - Payment) as Balance FROM Payments WHERE AcctNum = '" & AcctNum & "' AND NOT Status = 'VOIDED' AND NOT Status = 'DELETED' GROUP BY NumericInvoiceNumber ORDER BY NumericInvoiceNumber"
            RecordCT = IO_GetSegmentSetInToStructure(gShipriteDB, SQL, InvoiceRecordSet)

            For i = 0 To RecordCT - 1

                Credit = Val(InvoiceRecordSet.RecordSet(i).Field(1).FValue)
                InvNum2 = InvoiceRecordSet.RecordSet(i).Field(0).FValue
                Credit = Round(Credit, 2)
                If Credit < 0 Then

                    Credit *= -1
                    j = 0
                    Do Until j = RecordCT Or Credit = 0

                        InvBal = Val(InvoiceRecordSet.RecordSet(j).Field(1).FValue)
                        InvBal = Round(InvBal, 2)
                        InvNum = InvoiceRecordSet.RecordSet(j).Field(0).FValue

                        ExportToFile.Text = i.ToString & " of " & RecordCT.ToString & " | " & AcctNum & " - " & InvNum
                        '                        System.Windows.Forms.Application.DoEvents()
                        If Not InvBal = 0 And Not InvNum = InvNum2 Then

                            If Credit > InvBal Then

                                Credit -= InvBal
                                amt = InvBal
                                InvoiceRecordSet.RecordSet(j).Field(1).FValue = 0

                            Else

                                Credit = Round(Credit, 2)
                                amt = Credit
                                Credit = 0
                                InvoiceRecordSet.RecordSet(j).Field(1).FValue = Val(InvoiceRecordSet.RecordSet(j).Field(1).FValue) - amt

                            End If
                            SQL = "SELECT TOP 1 * FROM Payments where InvNum = '" & InvNum & "' ORDER BY ID desc"
                            SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
                            Segment = GetNextSegmentFromSet(SegmentSet)
                            Segment = RemoveBlankElementsFromSegment(Segment)
                            Segment = AddElementToSegment(Segment, "ID", GetNextIDNumber(gShipriteDB, "Payments"))
                            Segment = AddElementToSegment(Segment, "Type", "ADJUST")
                            Segment = AddElementToSegment(Segment, "FOLIO", "NORMALIZE BALANCE")
                            Segment = AddElementToSegment(Segment, "Desc", "Balance XFER FR " & InvoiceRecordSet.RecordSet(i).Field(0).FValue)
                            Segment = AddElementToSegment(Segment, "Charge", "0")
                            Segment = AddElementToSegment(Segment, "Payment", Format(amt, "0.00"))

                            SQL = MakeInsertSQLFromSchema("Payments", Segment, gdbSchema_Payments, True)
                            ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                            SQL = "SELECT TOP 1 * FROM Payments where InvNum = '" & InvoiceRecordSet.RecordSet(i).Field(0).FValue & "'"
                            SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
                            Segment = GetNextSegmentFromSet(SegmentSet)
                            Segment = RemoveBlankElementsFromSegment(Segment)
                            Segment = AddElementToSegment(Segment, "ID", GetNextIDNumber(gShipriteDB, "Payments"))
                            Segment = AddElementToSegment(Segment, "Type", "ADJUST")
                            Segment = AddElementToSegment(Segment, "FOLIO", "NORMALIZE BALANCE")
                            Segment = AddElementToSegment(Segment, "Desc", "Balance XFER TO " & InvoiceRecordSet.RecordSet(j).Field(0).FValue)
                            Segment = AddElementToSegment(Segment, "Charge", Format(amt, "0.00"))
                            Segment = AddElementToSegment(Segment, "Payment", "0")

                            SQL = MakeInsertSQLFromSchema("Payments", Segment, gdbSchema_Payments, True)
                            ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                        End If
                        j += 1

                    Loop
                    If j = RecordCT And Credit < 0 Then  ' no more invoices with balances

                        Exit For

                    End If
                    InvoiceRecordSet.RecordSet(i).Field(1).FValue = Credit

                End If

            Next i

        Loop
        ' Get Total of Store AR to match after utility completes

        SQL = "SELECT  SUM(Charge - Payment) as Balance FROM Payments WHERE"
        If Not DoOnlyAccount = "" Then

            SQL &= " AcctNum = '" & DoOnlyAccount & "' AND"

        End If
        SQL &= " NOT Status = 'VOIDED' AND NOT Status = 'DELETED'"
        SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)

        NewAR = Round(Val(ExtractElementFromSegment("Balance", SegmentSet)), 2)


        ret = SetBalancesInPayments(DoOnlyAccount)

        Mouse.OverrideCursor = Cursors.Arrow
        MsgBox("OPERATION COMPLTED" & vbCrLf & vbCrLf & "Original AR Balance = " & Format(OriginalAR, "$ 0.00") & vbCrLf & Format(NewAR, "$ 0.00"), vbInformation, "ShipriteNext")

    End Sub

    Private Sub SetBalances_Click(sender As Object, e As RoutedEventArgs) Handles SetBalances.Click

        Dim ret
        ret = SetBalancesInPayments("")
        MsgBox("OPERATION COMPLETE")

    End Sub

    Private Sub ClearCashBalances_Click(sender As Object, e As RoutedEventArgs) Handles ClearCashBalances.Click

        Dim ans As Integer
        Dim InvoiceRC As RecordSetDefinition
        Dim RecordCT As Long = 0
        Dim RowsAffected As Long = 0
        Dim SQL As String = ""
        Dim i As Long = 0
        Dim AcctNum As String = ""
        Dim InvNum As String = ""
        Dim Balance As Double = 0
        Dim TotBalance As Double = 0
        Dim Segment As String = ""
        Dim SegmentSet As String = ""
        Dim ret As Integer = 0

        ans = MsgBox("ATTENTION...Clearing Balances on ALL CASH account invoices." & vbCrLf & vbCrLf & "CONTINUE???", vbQuestion + vbYesNo)
        If ans = vbNo Then

            Exit Sub

        End If

        Mouse.OverrideCursor = Cursors.Wait
        SQL = "SELECT AcctNum, InvNum, SUM(Charge - Payment) AS Balance FROM Payments GROUP BY AcctNum, InvNum"
        RecordCT = IO_GetSegmentSetInToStructure(gShipriteDB, SQL, InvoiceRC)
        RowsAffected = 0
        For i = 0 To RecordCT - 1

            AcctNum = InvoiceRC.RecordSet(i).Field(0).FValue
            If AcctNum = "CASH" Then

                InvNum = InvoiceRC.RecordSet(i).Field(1).FValue
                Balance = Round(Val(InvoiceRC.RecordSet(i).Field(2).FValue), 2)
                If Not Balance = 0 Then

                    RowsAffected += 1
                    SQL = "SELECT TOP 1 * FROM Payments where InvNum = '" & InvNum & "' ORDER BY ID desc"
                    SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
                    Segment = GetNextSegmentFromSet(SegmentSet)
                    Segment = RemoveBlankElementsFromSegment(Segment)
                    Segment = AddElementToSegment(Segment, "ID", GetNextIDNumber(gShipriteDB, "Payments"))
                    Segment = AddElementToSegment(Segment, "Type", "ADJUST")
                    Segment = AddElementToSegment(Segment, "FOLIO", "NORMALIZE BALANCE")
                    Segment = AddElementToSegment(Segment, "Desc", "Write Off Cash Acct")

                    If Balance > 0 Then

                        Segment = AddElementToSegment(Segment, "Charge", "0")
                        Segment = AddElementToSegment(Segment, "Payment", Format(Balance, "0.00"))

                    Else

                        Segment = AddElementToSegment(Segment, "Payment", "0")
                        Segment = AddElementToSegment(Segment, "Charge", Format(Balance, "0.00"))

                    End If

                    SQL = MakeInsertSQLFromSchema("Payments", Segment, gdbSchema_Payments, True)
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                    TotBalance += Balance

                End If

            End If

        Next i
        Mouse.OverrideCursor = Cursors.Arrow
        MsgBox("OPERATION COMPLETE" & vbCrLf & vbCrLf & "Rows Affected - " & RowsAffected & vbCrLf & vbCrLf & "Written Off Invoices..." & Format(TotBalance, "0.00"), vbInformation)

    End Sub

    Private Sub RecoverAdjustments_Click(sender As Object, e As RoutedEventArgs) Handles RecoverAdjustments.Click

        Dim buf As String = ""
        Dim dPath As String = ""
        Dim SQL As String = ""
        Dim Segment As String = ""
        Dim SegmentSet As String = ""
        Dim ret As Long = 0

        dPath = Dir("z:\", FileAttribute.Directory)
        If dPath = "" Then

            dPath = Dir("c:\shiprite", FileAttribute.Directory)
            If Not dPath = "" Then

                dPath = "c:\shiprite\"

            End If

        Else

            dPath = "z:\"

        End If
        If dPath = "" Then

            dPath = "c:\shipritenext\"

        End If
        If dPath = "" Then

            InputBox("ATTENTION...Enter Complete Path of Shiprite Pro Database...", "Recovering Adjustments")
            If buf = "" Then

                Exit Sub

            End If

        Else

            dPath = dPath & "shiprite.mdb"

        End If
        buf = Dir(dPath, FileAttribute.Normal)
        If buf = "" Then

            MsgBox("ATTENTION...Unable to locate Shiprite.MDB file at..." & dPath & vbCrLf & vbCrLf & "CANCELLED!", vbIgnore, "Recovering Adjustments")
            Exit Sub

        End If
        Mouse.OverrideCursor = Cursors.Wait
        SQL = "SELECT * FROM Payments WHERE [Type] = 'ADJUST' ORDER BY ID"
        SegmentSet = IO_GetSegmentSet(dPath, SQL)
        ret = 0
        Do Until SegmentSet = ""

            Segment = GetNextSegmentFromSet(SegmentSet)
            Segment = RemoveBlankElementsFromSegment(Segment)
            buf = GetNextInvoiceNumber()
            Segment = AddElementToSegment(Segment, "InvNum", buf)
            Segment = AddElementToSegment(Segment, "NumericInvoiceNumber", buf)
            SQL = MakeInsertSQLFromSchema("Payments", Segment, PaymentsSchema)
            ret += IO_UpdateSQLProcessor(gShipriteDB, SQL,, True)

        Loop
        Mouse.OverrideCursor = Cursors.Arrow
        MsgBox("ATTENTION...Operation completed...Added back " & ret.ToString & " Records.", vbInformation)

    End Sub
End Class
