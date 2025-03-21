Imports System.Drawing.Printing
Imports SHIPRITE.ShipRiteReports

Public Class ReportsManager
    Inherits CommonWindow

    Dim Last_Column_Sorted As String
    Dim Last_Sort_Ascending As Boolean

    Private Class CloseIDItem
        Public Property CloseID As Integer
        Public Property CloseDate As Date
    End Class

    Private Class StatementItem
        Public Property AccountNumber As String
        Public Property SendAddressBlock As String
        Public Property EmailAddress As String
        Public Property TotalBalance As Double
        Public Property SendEmail As Boolean
        Public Property EmailSent As String
    End Class

    Dim PreselectIndex As Integer
    Dim CLoseID_List As List(Of CloseIDItem)


    Public Sub New()

        MyBase.New()

        ' This call is required by the designer.
        InitializeComponent()

    End Sub

    Public Sub New(ByVal callingWindow As Window, Optional ByRef Select_Index As Integer = -1)

        MyBase.New(callingWindow)

        ' This call is required by the designer.
        InitializeComponent()

        PreselectIndex = Select_Index
        ShipRiteReports.ReportsODBC.ShipRiteReports_SetODBC()

    End Sub

    Private Sub ReportsManager_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded

        If (gIsProgramSecurityEnabled Or gIsPOSSecurityEnabled) AndAlso Not Check_Current_User_Permission("Reports") Then

            Me.Close()
        End If

        Dim ret As Long = 0

        'makes tab headers not visible in run time. 
        For Each currentTab As TabItem In Reports_TabControl.Items
            currentTab.Visibility = Visibility.Collapsed
        Next

        'Printer_CmB.ItemsSource = PrinterSettings.InstalledPrinters
        'Printer_CmB.SelectedItem = GetPolicyData(gReportsDB, "ReportPrinter")

        Reports_ListBox.SelectedIndex = PreselectIndex
        Reports_TabControl.SelectedIndex = PreselectIndex

        If PreselectIndex = 1 Then
            'Load AR account info
            If gAccountSegment <> "" Then
                AR_AcctNo_TxtBx.Text = ExtractElementFromSegment("AcctNum", gAccountSegment, "")
                AR_AcctName_TxtBx.Text = ExtractElementFromSegment("AcctName", gAccountSegment, "")
                AR_ForAccount_RdBtn.IsChecked = True
            End If
        End If

        General.Update_ReportWriter_Setup()

        ReportStartDate.SelectedDate = Today
        ReportEndDate.SelectedDate = Today

        gCurrentCloseID = 0
    End Sub

    Public Shared Function MakeDateWizeZReport(StartDate As String, SQL2 As String, CloseIDs As String) As String

        Dim PayM As Double
        Dim chg As Double
        Dim TotalCollected As Double
        Dim ASofAR As Double
        Dim TotalCollect As Double
        Dim RefundAmt As Double
        Dim SQL As String
        Dim zSQL As String
        Dim buf As String

        Dim Segment As String
        Dim SegmentSet As String
        Dim ret As Long

        Dim Cash As Double
        Dim CashOverAndShort As Double
        Dim oTotalCash As Double
        Dim cTotalCash As Double

        Dim ARRecordSet As RecordSetDefinition
        Dim i As Long

        TotalCollected = 0
        SQL = "DELETE * FROM ZReport"
        ret = IO_UpdateSQLProcessor(gReportWriter, SQL)
        SQL = "DELETE * FROM Transactions"
        ret = IO_UpdateSQLProcessor(gReportWriter, SQL)
        SQL = "DELETE * FROM Payments"
        ret = IO_UpdateSQLProcessor(gReportWriter, SQL)

        SQL = "INSERT INTO ZReport (ID, AR) VALUES (1, 0)"
        ret = IO_UpdateSQLProcessor(gReportWriter, SQL)

        TotalCollect = 0
        ' Sales
        SQL = "SELECT Charge, Payment FROM Payments WHERE [Status] = 'Ok' AND [Type] = 'Sale' AND " & SQL2
        SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
        chg = 0
        PayM = 0
        Do Until SegmentSet = ""

            Segment = GetNextSegmentFromSet(SegmentSet)
            chg += Val(ExtractElementFromSegment("Charge", Segment))
            PayM += Val(ExtractElementFromSegment("Payment", Segment))

        Loop
        zSQL = ""
        If Not zSQL = "" Then

            zSQL &= ", "

        End If
        zSQL &= "Sales = " & Round(chg - PayM, 2)

        ' Returns

        '    SQL = "SELECT SUM([Charge]) AS CHG, SUM([Payment]) AS Pay FROM Payments WHERE [Status] = 'Ok' AND [Type] = 'Refund' AND " & SQL2

        buf = FlushOut(SQL2, "Date", "~")
        buf = FlushOut(SQL2, "~", "[Date]")
        SQL = "SELECT SUM([ExtPrice]) AS Returns FROM Transactions WHERE ExtPrice < 0 AND " & buf
        SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
        RefundAmt = Val(ExtractElementFromSegment("Returns", SegmentSet))
        If Not zSQL = "" Then

            zSQL &= ", "

        End If
        zSQL &= "Refunds = " & Math.Abs(RefundAmt)

        ' Cash

        SQL = "SELECT Charge, Payment FROM Payments WHERE [Status] = 'Ok' AND ([Type] = 'Cash' OR [Type] = 'Change') AND " & SQL2
        SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
        chg = 0
        PayM = 0
        Do Until SegmentSet = ""

            Segment = GetNextSegmentFromSet(SegmentSet)
            chg += Val(ExtractElementFromSegment("Charge", Segment))
            PayM += Val(ExtractElementFromSegment("Payment", Segment))

        Loop
        Cash = PayM - chg
        TotalCollected = Round(TotalCollected + (PayM - chg), 2)

        ' Cash Over and Short

        CashOverAndShort = 0
        If Not CloseIDs = "" And Not CloseIDs = "No ID" Then

            SQL = "SELECT * FROM OpenClose WHERE CloseID in (" & CloseIDs & ")"
            SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
            Do Until SegmentSet = ""

                Segment = GetNextSegmentFromSet(SegmentSet)
                oTotalCash = Val(ExtractElementFromSegment("oTotalCash", Segment))
                cTotalCash = Val(ExtractElementFromSegment("cTotalCash", Segment))
                CashOverAndShort += (cTotalCash - oTotalCash)

            Loop
            CashOverAndShort = Round(Cash, 2) - CashOverAndShort
            CashOverAndShort = Round(CashOverAndShort, 2)
            Cash = Round(Cash - CashOverAndShort, 2)                         ' DR #1129 11/11/2016 Cash = Cash O/S Reports Full Cash At Closing Z-report

        End If

        ' Write Cash Here

        If Not zSQL = "" Then

            zSQL &= ", "

        End If

        zSQL &= "Cash = " & Round(Cash, 2)

        ' Paid Out

        SQL = "SELECT Charge, Payment FROM Payments WHERE [Status] = 'Ok' AND [Type] = 'Paid-Out' AND " & SQL2
        SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
        chg = 0
        PayM = 0
        Do Until SegmentSet = ""

            Segment = GetNextSegmentFromSet(SegmentSet)
            chg += Val(ExtractElementFromSegment("Charge", Segment))
            PayM += Val(ExtractElementFromSegment("Payment", Segment))

        Loop

        ' Write CashOverAndShort here

        zSQL &= ", CashOverAndShort = " & Round(CashOverAndShort, 2)

        If Not zSQL = "" Then

            zSQL &= ", "

        End If

        ' Write Paid Out Here

        zSQL &= "PaidOut = " & Round(PayM, 2)

        ' Check

        SQL = "SELECT Charge, Payment FROM Payments WHERE [Status] = 'Ok' AND [Type] = 'Check' AND " & SQL2
        SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
        chg = 0
        PayM = 0
        Do Until SegmentSet = ""

            Segment = GetNextSegmentFromSet(SegmentSet)
            chg += Val(ExtractElementFromSegment("Charge", Segment))
            PayM += Val(ExtractElementFromSegment("Payment", Segment))

        Loop
        If Not zSQL = "" Then

            zSQL &= ", "

        End If
        zSQL &= "[Check] = " & Round(PayM - chg, 2)    '       GF..1014..10/17/2018
        TotalCollected = Round(TotalCollected + (PayM - chg), 2)

        ' Charge

        SQL = "SELECT Charge, Payment FROM Payments WHERE [Status] = 'Ok' AND ([Type] = 'Charge' OR [Type] = 'ChargeSpecial') AND " & SQL2
        SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
        chg = 0
        PayM = 0
        Do Until SegmentSet = ""

            Segment = GetNextSegmentFromSet(SegmentSet)
            chg += Val(ExtractElementFromSegment("Charge", Segment))
            PayM += Val(ExtractElementFromSegment("Payment", Segment))

        Loop
        If Not zSQL = "" Then

            zSQL &= ", "

        End If
        zSQL &= "ChargeCards = " & Round(PayM - chg, 2)
        TotalCollected = Round(TotalCollected + (PayM - chg), 2)

        ' Other

        SQL = "SELECT Charge, Payment FROM Payments WHERE [Status] = 'Ok' AND [Type] = 'Other' AND " & SQL2
        SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
        chg = 0
        PayM = 0
        Do Until SegmentSet = ""

            Segment = GetNextSegmentFromSet(SegmentSet)
            chg += Val(ExtractElementFromSegment("Charge", Segment))
            PayM += Val(ExtractElementFromSegment("Payment", Segment))

        Loop
        If Not zSQL = "" Then

            zSQL &= ", "

        End If
        zSQL &= "Other = " & Round(PayM - chg, 2)
        TotalCollected = Round(TotalCollected + (PayM - chg), 2)

        ' Total A/R

        SQL = "SELECT SUM([Charge]) AS CHG FROM Payments WHERE [Status] = 'Ok' AND " & SQL2
        SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
        chg = Val(ExtractElementFromSegment("CHG", SegmentSet))

        SQL = "SELECT SUM([Payment]) AS Pay FROM Payments WHERE [Status] = 'Ok' AND " & SQL2
        SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
        PayM = Val(ExtractElementFromSegment("Pay", SegmentSet))

        If Not zSQL = "" Then

            zSQL &= ", "

        End If
        zSQL &= "AR = " & Round(chg - PayM, 2)

        ' Current AR
        'commented out because it always returns zero and is not used on Z report. Payments table doesn't have a 'Sold' Status.

        'SQL = "SELECT Charge, Payment, Status FROM Payments WHERE [Date] < " & gDC & StartDate & gDC
        'ret = IO_GetSegmentSetInToStructure(gShipriteDB, SQL, ARRecordSet)
        'chg = 0
        'PayM = 0
        'For i = 0 To ARRecordSet.RecordCount - 1

        '    If ARRecordSet.RecordSet(i).Field(2).FValue = "Sold" Then

        '        chg += Val(ARRecordSet.RecordSet(i).Field(0).FValue)
        '        PayM += Val(ARRecordSet.RecordSet(i).Field(1).FValue)

        '    End If

        'Next i
        'ASofAR = Round(chg - PayM, 2)
        'If Not zSQL = "" Then

        '    zSQL &= ", "

        'End If
        'zSQL &= "CurrentAR = " & Round(chg - PayM, 2)
        If Not zSQL = "" Then

            zSQL &= ", "

        End If
        zSQL &= "TotalCollected = " & Round(TotalCollected, 2)

        SQL = "UPDATE ZReport SET " & zSQL
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
        Return 0

    End Function

    Public Shared Function GetContacts(StartDate As String, EndDate As String) As Integer

        ' This function moves contacts to the contact table based on a date range in the Transactions Table

        Dim SQL As String
        Dim Segment As String
        Dim ContactsRecordSet As RecordSetDefinition
        Dim i As Long
        Dim ret As Long
        Dim dbSchema As String

        ret = IO_UpdateSQLProcessor(gReportWriter, "DELETE * FROM Contacts")
        SQL = "SELECT * FROM Contacts WHERE ID IN (Select DISTINCT SoldTo AS ID FROM Transactions WHERE [Date] BETWEEN " & gDC & StartDate & gDC & " AND " & gDC & EndDate & gDC & ")"
        ret = IO_GetSegmentSetInToStructure(gShipriteDB, SQL, ContactsRecordSet)
        dbSchema = IO_GetFieldsCollection(gReportWriter, "Contacts", "", True, False, True)
        ret = 0
        For i = 0 To ContactsRecordSet.RecordCount - 1

            Segment = MakeSegmentFromRecord(ContactsRecordSet, i, True)
            SQL = MakeInsertSQLFromSchema("Contacts", Segment, dbSchema, True)
            ret += IO_UpdateSQLProcessor(gReportWriter, SQL)

        Next
        Return 0

    End Function

    Public Shared Function Check_Dates(StartDate As Date, EndDate As Date) As Boolean
        If IsNothing(StartDate) Or IsNothing(EndDate) Then

            MsgBox("ATTENTION...Printing Report" & vbCrLf & vbCrLf & "YOU MUST SELECT A COMPLETE DATE RANGE!!! Try Again.", vbCritical, gProgramName)
            Return False

        End If

        If StartDate > EndDate Then

            MsgBox("ATTENTION...Printing Report" & vbCrLf & vbCrLf & "REPORT START DATE CANNOT BE GREATER THAN END DATE!!! Try Again.", vbCritical, gProgramName)
            Return False

        End If

        Return True
    End Function

    Public Shared Function PrintProductionBasedReports(ReportName As String, ReportTitle As String, TableCount As Integer, GetContactsForReport As Boolean, StartDate As Date, EndDate As Date, Optional CloseID_SQL As String = "") As Integer

        'Dim StartDate As Date
        'Dim EndDate As Date

        Dim PrintSegment As String = ""
        Dim ret As Integer

        Dim SQL As String = ""
        Dim Segment As String = ""
        Dim RecordCT As Long = 0
        Dim RecordID As Long = 0
        Dim i As Long = 0
        Dim pStatus As Integer = 0
        Dim dbSchema As String = ""
        Dim Preview As String = ""
        Dim FName As String = ""

        If Not Check_Dates(StartDate, EndDate) Then Return 1

        If GetContactsForReport = True Then

            ret = GetContacts(StartDate.ToShortDateString, EndDate.ToShortDateString)

        End If
        ret = PrepareProductionStyleReport(StartDate.ToShortDateString, EndDate.ToShortDateString, CloseID_SQL)
        If ret = -1 Then Return -1



        Dim report As New _ReportObject()
        report.ReportName = ReportName + ".rpt"
        Dim reportPrev As New ReportPreview(report)
        reportPrev.ShowDialog()

        PrintSegment = AddElementToSegment(PrintSegment, "PrinterDevice", report.PrinterName)
        'PrintSegment = AddElementToSegment(PrintSegment, "PrinterDevice", Printer_CmB.Text)

        If report.IsPreviewReport = True Then
            Preview = "True"
        Else
            Preview = "False"
        End If


        Return 0

    End Function
    Public Shared Function PrepareProductionStyleReport(SDate As String, EDate As String, Optional CloseID_SQL As String = "") As Integer

        Dim TransRecordSet As RecordSetDefinition
        Dim ret As Long = 0
        Dim SQL As String = ""
        Dim Segment As String = ""
        Dim RecordCT As Long = 0
        Dim dbSchema As String = ""
        Dim pStatus As Integer = 0
        Dim pSKU As Integer = 0
        Dim i As Long = 0
        Dim StartDate As String = ""
        Dim EndDate As String = ""

        ret = IO_UpdateSQLProcessor(gReportWriter, "DELETE * FROM Transactions")

        If CloseID_SQL = "" Then
            SQL = "SELECT * FROM Transactions WHERE [Date] BETWEEN " & gDC & SDate & gDC & " AND " & gDC & EDate & gDC
        Else
            SQL = "SELECT * FROM Transactions WHERE " & CloseID_SQL
        End If

        RecordCT = IO_GetSegmentSetInToStructure(gShipriteDB, SQL, TransRecordSet)
        dbSchema = IO_GetFieldsCollection(gReportWriter, "Transactions", "", True, False, True)
        If RecordCT = 0 Then

            MsgBox("ATTENTION...Production Report By Department" & vbCrLf & vbCrLf & "NO RESULTS FROM QUERY.", vbCritical, gProgramName)
            Return -1
            Exit Function

        End If
        pStatus = GetFieldNumber(TransRecordSet, "Status")
        pSKU = GetFieldNumber(TransRecordSet, "SKU")
        ret = 0
        For i = 0 To TransRecordSet.RecordCount - 1

            If TransRecordSet.RecordSet(i).Field(pStatus).FValue = "Sold" And Not TransRecordSet.RecordSet(i).Field(pSKU).FValue = "NOTE" Then

                TransRecordSet.RecordSet(i).SkipRecord = False
                'Segment = MakeSegmentFromRecord(TransRecordSet, i, True)
                'SQL = MakeInsertSQLFromSchema("Transactions", Segment, dbSchema, True)
                'ret += IO_UpdateSQLProcessor(gReportWriter, SQL)

            Else

                TransRecordSet.RecordSet(i).SkipRecord = True

            End If

        Next
        ret = io_DumpRecordsetToLocalTable(gReportWriter, TransRecordSet, "Transactions")

        ret = UpdatePolicy(gReportWriter, "DateRange", SDate & " - " & EDate)
        SQL = "UPDATE PrintInformation SET Dates = '" & SDate & " - " & EDate & "'"

        ret = IO_UpdateSQLProcessor(gReportWriter, SQL)

        Return 0

    End Function

    Public Shared Function UpdateHourlySalesTable(SDate As Date, EDate As Date)
        Dim SQL As String
        Dim ret As Long
        Dim RecordSet As RecordSetDefinition
        Dim CT As Integer = 0
        Dim Segment As String = ""
        IO_UpdateSQLProcessor(gShipriteDB, "DELETE FROM HourlySales;")

        SQL = " SELECT [InvNum], [Time], [SalesRep], [Date], Sum([ExtPrice]) as InvTot FROM Transactions WHERE [Status] <> 'Deleted' And [Hash] = False AND [ExtPrice] <> 0.0 AND ([Date] BETWEEN #" & SDate & "# AND #" & EDate & "#) GROUP BY [InvNum], [SalesRep], [Date], [Time]"
        ret = IO_GetSegmentSetInToStructure(gShipriteDB, SQL, RecordSet)


        For i = 0 To RecordSet.RecordCount - 1
            Segment = ""


            If RecordSet.RecordSet(i).Field(1).FValue <> "" Then
                Segment = AddElementToSegment(Segment, "Hour", Mid(RecordSet.RecordSet(i).Field(1).FValue.ToString(), 0, RecordSet.RecordSet(i).Field(1).FValue.ToString().IndexOf(":")))
            Else
                Segment = AddElementToSegment(Segment, "Hour", "00")
            End If

            Dim TheHDate As DateTime = RecordSet.RecordSet(i).Field(3).FValue

            Segment = AddElementToSegment(Segment, Format$(TheHDate, "dddd"), RecordSet.RecordSet(i).Field(4).FValue)
            Dim total As Double
            If Double.TryParse(RecordSet.RecordSet(i).Field(4).FValue, total) Then
                Segment = AddElementToSegment(Segment, "Total", total)
            Else
                ' Handle the case where the value cannot be parsed as an integer
                ' For example, set a default value or skip adding to Segment
            End If

            Segment = AddElementToSegment(Segment, "Clerk", RecordSet.RecordSet(i).Field(2).FValue)

            Dim HourlySalesDBSchema As String = IO_GetFieldsCollection(gShipriteDB, "HourlySales", "", True, False, True)
            SQL = MakeInsertSQLFromSchema("HourlySales", Segment, HourlySalesDBSchema, True)

            ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

        Next i




        SQL = "SELECT * FROM HourlySales ORDER BY [Hour], [ID]"
        ret = IO_GetSegmentSetInToStructure(gShipriteDB, SQL, RecordSet)

        Dim customercount As Integer = 0
        Dim AccumulatedTotal As Double = 0#
        For i = 0 To RecordSet.RecordCount - 1
            AccumulatedTotal = AccumulatedTotal + RecordSet.RecordSet(i).Field(10).FValue
            customercount = customercount + 1

            RecordSet.RecordSet(i).Field(11).FValue = customercount
            RecordSet.RecordSet(i).Field(12).FValue = AccumulatedTotal
        Next
        IO_UpdateSQLProcessor(gShipriteDB, "DELETE FROM HourlySales;")
        ret = io_DumpRecordsetToLocalTable(gShipriteDB, RecordSet, "HourlySales")

        SQL = "UPDATE PrintInformation SET Dates = '" & SDate & " - " & EDate & "'"

        ret = IO_UpdateSQLProcessor(gReportWriter, SQL)
    End Function
    Private Sub Reports_ListBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles Reports_ListBox.SelectionChanged
        If Reports_ListBox.SelectedIndex = 0 Then
            If (gIsProgramSecurityEnabled Or gIsPOSSecurityEnabled) AndAlso Not Check_Current_User_Permission("Reports_IncomeProduction") Then
                Reports_ListBox.SelectedIndex = Reports_TabControl.SelectedIndex
                Exit Sub
            End If
        End If
        Reports_TabControl.SelectedIndex = Reports_ListBox.SelectedIndex
    End Sub

    Private Sub Report_ProductionByDepartment_Click(sender As Object, e As RoutedEventArgs) Handles Report_ProductionByDepartment.Click

        Me.Cursor = Cursors.Wait

        Dim ret As Integer
        ret = PrintProductionBasedReports("Product", "Production By Department", 2, False, ReportStartDate.SelectedDate, ReportEndDate.SelectedDate)
        Me.Cursor = Cursors.Arrow

    End Sub



    Private Sub Report_ProductionByAccount_Click(sender As Object, e As RoutedEventArgs) Handles Report_ProductionByAccount.Click
        Me.Cursor = Cursors.Wait
        Dim ret As Integer
        ret = PrintProductionBasedReports("ProdAcct", "Production By Account", 2, False, ReportStartDate.SelectedDate, ReportEndDate.SelectedDate)
        Me.Cursor = Cursors.Arrow
    End Sub

    Private Sub Report_ProductionByClerk_Click(sender As Object, e As RoutedEventArgs) Handles Report_ProductionByClerk.Click

        Me.Cursor = Cursors.Wait
        Dim ret As Integer
        ret = PrintProductionBasedReports("ProdClrk", "Production By Sales Clerk", 2, False, ReportStartDate.SelectedDate, ReportEndDate.SelectedDate)
        Me.Cursor = Cursors.Arrow
    End Sub

    Private Sub DepartmentalChargeback_Click(sender As Object, e As RoutedEventArgs) Handles DepartmentalChargeback.Click
        Me.Cursor = Cursors.Wait
        Dim ret As Integer
        ret = PrintProductionBasedReports("DepartmentChargeBack", "Charge Back Report", 2, False, ReportStartDate.SelectedDate, ReportEndDate.SelectedDate)
        Me.Cursor = Cursors.Arrow
    End Sub

    Private Sub Report_SalesJournal_Click(sender As Object, e As RoutedEventArgs) Handles Report_SalesJournal.Click
        Me.Cursor = Cursors.Wait
        Dim ret As Integer
        ret = PrintProductionBasedReports("Sales", "Sales By Department", 2, False, ReportStartDate.SelectedDate, ReportEndDate.SelectedDate)
        Me.Cursor = Cursors.Arrow
    End Sub

    Private Sub Report_SalesByAccount_Click(sender As Object, e As RoutedEventArgs) Handles Report_SalesByAccount.Click
        Me.Cursor = Cursors.Wait
        Dim ret As Integer
        ret = PrintProductionBasedReports("SaleAcct", "Sales By Account", 2, False, ReportStartDate.SelectedDate, ReportEndDate.SelectedDate)
        Me.Cursor = Cursors.Arrow
    End Sub

    Private Sub Report_SalesByClerk_Click(sender As Object, e As RoutedEventArgs) Handles Report_SalesByClerk.Click
        Me.Cursor = Cursors.Wait
        Dim ret As Integer
        ret = PrintProductionBasedReports("SaleClrk", "Sales By Clerk", 2, False, ReportStartDate.SelectedDate, ReportEndDate.SelectedDate)
        Me.Cursor = Cursors.Arrow
    End Sub

    Private Sub Reports_ZReport_Click(sender As Object, e As RoutedEventArgs) Handles Reports_ZReport.Click
        Me.Cursor = Cursors.Wait
        Print_ZReport(ReportStartDate.SelectedDate, ReportEndDate.SelectedDate)
        Me.Cursor = Cursors.Arrow
    End Sub

    Public Shared Sub Print_ZReport(StartDate As Date, EndDate As Date)
        Dim hold As String = ""
        Dim ret As Long = 0
        Dim buf As String
        Dim Title As String
        Dim iloc As Integer
        Dim SQL As String
        Dim Preview As Boolean
        Dim Devline As String
        Dim PrinterDevice As String = ""
        ' Dim StartDate As Date
        ' Dim EndDate As Date
        Dim PrintControlSet As String
        Dim PrintControlSegment As String
        Dim SQL2 As String
        Dim SQL3 As String
        Dim CloseIDs As String
        Dim DrawerIDs As String
        Dim DIDs As String
        Dim date1 As Date
        Dim date2 As Date
        Dim holdCloseIDs As String
        Dim Charged As Double
        Dim Paid As Double
        Dim BALANCE As Double
        Dim Segment As String
        Dim SegmentSet As String
        Dim zSchema As String = ""
        Dim pSchema As String = ""
        Dim tSchema As String = ""
        Dim PaymentsRecordSet As RecordSetDefinition
        Dim TransactionsRecordSet As RecordSetDefinition
        Dim RecordCT As Long = 0
        Dim i As Long = 0
        Dim UsingCloud As Boolean

        If Not InStr(1, gShipriteDB, "$") = 0 Then   ' a '$' sign in the path for the database means its in the cloud

            UsingCloud = False

        Else

            UsingCloud = True

        End If

        If Not Check_Dates(StartDate, EndDate) Then Exit Sub

        zSchema = IO_GetFieldsCollection(gReportWriter, "ZReport", "", True, False, True)
        pSchema = IO_GetFieldsCollection(gReportWriter, "Payments", "", True, False, True)
        tSchema = IO_GetFieldsCollection(gReportWriter, "Transactions", "", True, False, True)


        If StartDate > EndDate Then

            MsgBox("ATTENTION...Printing Report" & vbCrLf & vbCrLf & "REPORT START DATE CANNOT BE GREATER THAN END DATE!!! Try Again.", vbCritical, gProgramName)
            Exit Sub

        End If
        If Not gCurrentCloseID = 0 Then      ' if this is a closing Z report

            SQL = "SELECT TOP 1 [Date] FROM Payments WHERE CloseID = " & gCurrentCloseID
            SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
            If SegmentSet = "" Then

                MsgBox("ATTENTION...Z-Report" & vbCrLf & vbCrLf & "No data to print.  Try different dates.", vbInformation, "Shiprite Report Writer")
                Exit Sub

            End If
            date1 = ExtractElementFromSegment("Date", SegmentSet)
            SQL = "SELECT TOP 1 [Date] FROM Transactions WHERE CloseID = " & gCurrentCloseID
            SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
            date2 = ExtractElementFromSegment("Date", SegmentSet)

            If date1 < date2 Then
                StartDate = date1
            Else
                StartDate = date2
            End If

            SQL = "SELECT TOP 1 [Date] FROM Payments WHERE CloseID = " & gCurrentCloseID & " ORDER BY [Date] Desc"
            SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
            date1 = ExtractElementFromSegment("Date", SegmentSet)

            SQL = "SELECT TOP 1 [Date] FROM Transactions WHERE CloseID = " & gCurrentCloseID & " ORDER BY [Date] Desc"
            SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
            date2 = ExtractElementFromSegment("Date", SegmentSet)

            If date1 < date2 Then
                EndDate = date1
            Else
                EndDate = date2
            End If

        End If

        ret = UpdatePolicy(gShipriteDB, "DateRange", StartDate.ToShortDateString & " - " & EndDate.ToShortDateString)

        ret = ClearReportWriterTables()                                                 ' GF..1060..03/13/2019

        buf = ExtractElementFromSegment("CloseIDs", gResult)

        If buf = "" Then

            Title = "Datewise Z-Report"
            SQL2 = "([Date] >= #" & StartDate.ToShortDateString & "# AND [Date] <= #" & EndDate.ToShortDateString & "#)"
            SQL3 = "([Date] >= #" & StartDate.ToShortDateString & "# AND [Date] <= #" & EndDate.ToShortDateString & "#)"
        Else

            Title = "Consolidated Z-Report"

            If Not gCurrentCloseID = 0 Then
                Title = "Closing Z-Report - " & gCurrentCloseID
            End If

            ret = UpdatePolicy(gShipriteDB, "Data1", Title)
            DrawerIDs = ExtractElementFromSegment("DrawerIDs", gResult)
            If Not gCurrentCloseID = 0 Then
                CloseIDs = "(" & gCurrentCloseID & ")"

            Else

                CloseIDs = ExtractElementFromSegment("CloseIDs", gResult)
                holdCloseIDs = CloseIDs 'FlushOut(CloseIDs, vbCrLf, ", ")

                gResult = ""
                If Not CloseIDs = "" Then
                    buf = "CloseID IN (" & CloseIDs & ")"
                End If

                If buf <> "" Then
                    CloseIDs = buf
                End If
            End If

            buf = ""
            If Not DrawerIDs = "" Then

                buf = ""
                Do Until DrawerIDs = ""

                    iloc = InStr(1, DrawerIDs, vbCrLf)
                    If Not iloc = 0 Then

                        DIDs = Trim$(Mid(DrawerIDs, 1, iloc - 1))
                        DrawerIDs = Trim$(Mid(DrawerIDs, iloc + 2))

                    Else

                        DIDs = DrawerIDs
                        DrawerIDs = ""

                    End If
                    If Not buf = "" Then

                        buf &= " OR "

                    End If
                    buf &= "DrawerID = '" & DIDs & "'"

                Loop
            End If

            If Not buf = "" Then
                DrawerIDs = "(" & buf & ")"
            Else
                DrawerIDs = ""
            End If

            PrintControlSet = ""
            PrintControlSegment = ""
            If Not CloseIDs = "" Then
                SQL2 = CloseIDs
                SQL3 = CloseIDs

            Else
                If InStr(1, gShipriteDB, "$") = 0 Then

                    SQL2 = "([Date] >= #" & StartDate.ToShortDateString & "# AND [Date] <= #" & EndDate.ToShortDateString & "#) AND ISNULL(CloseID)"
                    SQL3 = "([Date] >= #" & StartDate.ToShortDateString & "# AND [Date] <= #" & EndDate.ToShortDateString & "#) AND ISNULL(CloseID)"

                Else

                    SQL2 = "([Date] >= '" & StartDate.ToShortDateString & "' AND [Date] <= '" & EndDate.ToShortDateString & "') AND Status = 'Ok' AND CloseID = NULL"
                    SQL3 = "([Date] >= '" & StartDate.ToShortDateString & "' AND [Date] <= '" & EndDate.ToShortDateString & "') AND Status = 'Ok' AND CloseID = NULL"

                End If
                If Not DrawerIDs = "" Then

                    SQL2 &= " AND DrawerID IN " & DrawerIDs & ""
                    SQL3 &= " AND DrawerID IN " & DrawerIDs & ""

                End If
            End If

            If Not gCurrentCloseID = 0 Then
                SQL2 = "CloseID = " & gCurrentCloseID
                SQL3 = "CloseID = " & gCurrentCloseID
                holdCloseIDs = gCurrentCloseID

            End If
        End If

        ' Make Z-Report Here

        buf = MakeDateWizeZReport(StartDate.ToShortDateString, SQL2, holdCloseIDs)
        Devline = "Default"

        ' Move ZReport

        SQL = "DELETE * FROM ZReport"
        ret = IO_UpdateSQLProcessor(gReportWriter, SQL)
        SQL = "SELECT * FROM ZReport"
        SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
        Segment = GetNextSegmentFromSet(SegmentSet)
        Segment = RemoveBlankElementsFromSegment(Segment)
        SQL = MakeInsertSQLFromSchema("ZReport", Segment, zSchema, True, False)
        ret = IO_UpdateSQLProcessor(gReportWriter, SQL)

        ' Load Transactions

        SQL = "DELETE FROM Transactions"
        ret = IO_UpdateSQLProcessor(gReportWriter, SQL)
        SQL = "SELECT ID, [Date], [Desc], SKU, AcctNum, AcctName, [Status], Dept, QTY, UnitPrice, STax, LTotal FROM Transactions WHERE " & SQL2
        RecordCT = IO_GetSegmentSetInToStructure(gShipriteDB, SQL, TransactionsRecordSet)
        For i = 0 To RecordCT - 1

            If Not TransactionsRecordSet.RecordSet(i).Field(6).FValue = "Sold" Then

                TransactionsRecordSet.RecordSet(i).SkipRecord = True
                'SQL = MakeInsertSQLFromRecordSet("Transactions", i, TransactionsRecordSet, UsingCloud, False, False)
                'ret = IO_UpdateSQLProcessor(gReportWriter, SQL)

            End If

        Next i
        ret = io_DumpRecordsetToLocalTable(gReportWriter, TransactionsRecordSet, "Transactions")

        ' Load Payments

        ret = IO_UpdateSQLProcessor(gReportWriter, "DELETE FROM Payments")
        ret = IO_UpdateSQLProcessor(gReportWriter, "DELETE FROM Cash")
        ret = IO_UpdateSQLProcessor(gReportWriter, "DELETE FROM [Check]")
        ret = IO_UpdateSQLProcessor(gReportWriter, "DELETE FROM [Charge]")
        ret = IO_UpdateSQLProcessor(gReportWriter, "DELETE FROM [Other]")
        ret = IO_UpdateSQLProcessor(gReportWriter, "DELETE FROM [Refund]")

        SQL = "SELECT ID, InvNum, [Date], [Desc], Charge, Payment, AcctNum, AcctName, [Type], [Status], NameOnCheck, CheckNum, BankNum, IIf([Type] = 'Other', OtherText, BankNum) AS OtherText, State, CCNum, ApprovalNum, CloseID, DrawerID, SalesRep FROM Payments WHERE " & SQL2
        '      MsgBox(SQL)
        RecordCT = IO_GetSegmentSetInToStructure(gShipriteDB, SQL, PaymentsRecordSet)


        For i = 0 To RecordCT - 1


            If PaymentsRecordSet.RecordSet(i).Field(9).FValue = "Ok" Then
                SQL = MakeInsertSQLFromRecordSet("Payments", i, PaymentsRecordSet, UsingCloud, False, False)

                ret = IO_UpdateSQLProcessor(gReportWriter, SQL)

                'Cash Table In ReportWriter
                If UCase(PaymentsRecordSet.RecordSet(i).Field(8).FValue) = "CASH" Or UCase(PaymentsRecordSet.RecordSet(i).Field(8).FValue) = "CHANGE" Then

                    ret = IO_UpdateSQLProcessor(gReportWriter, FlushOut(SQL, "Payments", "[Cash]"))

                End If

                'Check Table In ReportWriter
                If UCase(PaymentsRecordSet.RecordSet(i).Field(8).FValue) = "CHECK" Then

                    ret = IO_UpdateSQLProcessor(gReportWriter, FlushOut(SQL, "Payments", "[Check]"))

                End If

                If UCase(PaymentsRecordSet.RecordSet(i).Field(8).FValue) = "REFUND" Then

                    ret = IO_UpdateSQLProcessor(gReportWriter, FlushOut(SQL, "Payments", "[Refund]"))

                End If

                'Charge Table In ReportWriter
                If UCase(PaymentsRecordSet.RecordSet(i).Field(8).FValue) = "CHARGE" Or UCase(PaymentsRecordSet.RecordSet(i).Field(8).FValue) = "CHARGE SPECIAL" Then

                    ret = IO_UpdateSQLProcessor(gReportWriter, FlushOut(SQL, "Payments", "[Charge]"))

                End If

                'Other Table In ReportWriter
                If UCase(PaymentsRecordSet.RecordSet(i).Field(8).FValue) = "OTHER" Then

                    ret = IO_UpdateSQLProcessor(gReportWriter, FlushOut(SQL, "Payments", "[Other]"))


                End If

            End If

        Next i

        ' Changes in A/R

        Dim ARrecordset As RecordSetDefinition
        ret = IO_UpdateSQLProcessor(gReportWriter, "DELETE * FROM Accounts")
        SQL = "SELECT InvNum, AcctName, SUM(Charge) as Charge, SUM(Payment) AS Payment FROM Payments WHERE " & SQL2 & " AND Status = 'Ok' GROUP BY InvNum, AcctName"
        RecordCT = IO_GetSegmentSetInToStructure(gShipriteDB, SQL, ARrecordset)
        ret = 0
        For i = 0 To RecordCT - 1

            Segment = GetNextSegmentFromSet(SegmentSet)
            Charged = Val(ExtractElementFromSegment("Charge", Segment))
            Paid = Val(ExtractElementFromSegment("Payment", Segment))
            BALANCE = Round(Charged - Paid, 2)
            If BALANCE = 0 Then

                ARrecordset.RecordSet(i).SkipRecord = True
                'buf = ""
                'buf = AddElementToSegment(buf, "ID", i)
                'Segment = RemoveBlankElementsFromSegment(Segment)
                'Segment = RemoveElementFromSegment("Charge", Segment)
                'Segment = RemoveElementFromSegment("Payment", Segment)
                'Segment = AddElementToSegment(Segment, "Charge", BALANCE)
                'Segment = buf & Segment
                'hold = Segment
                'SQL = MakeInsertSQLFromSchema("Payments", Segment, pSchema, True, False)
                'SQL = FlushOut(SQL, "Payments", "[Accounts]")
                'ret += IO_UpdateSQLProcessor(gReportWriter, SQL)

            Else

                ret = ret

            End If

        Next i
        'ret = io_DumpRecordsetToLocalTable(gReportWriter, ARrecordset, "Accounts")

        ' List Of Returns

        Dim ReturnsRecordSet As RecordSetDefinition
        ret = IO_UpdateSQLProcessor(gReportWriter, "DELETE * FROM Refund")
        buf = FlushOut(SQL2, "Date", "~")
        buf = FlushOut(SQL2, "~", "[Date]")
        SQL = "SELECT InvNum, AcctName, SalesRep, SUM(Extprice) as Charge FROM Transactions WHERE " & buf & " AND ExtPrice < 0 AND Status = 'Sold' GROUP BY InvNum, AcctName, SalesRep"
        SegmentSet = IO_GetSegmentSetInToStructure(gShipriteDB, SQL, ReturnsRecordSet)
        ret = 0
        'For i = 0 To RecordCT - 1

        '    'buf = ""
        '    'buf = AddElementToSegment(buf, "ID", i)
        '    'Segment = GetNextSegmentFromSet(SegmentSet)
        '    'Segment = RemoveBlankElementsFromSegment(Segment)
        '    'Segment = buf & Segment
        '    'hold = Segment
        '    'SQL = MakeInsertSQLFromSchema("Payments", Segment, pSchema, True, False)
        '    'SQL = FlushOut(SQL, "Payments", "[Returns]")
        '    'ret += IO_UpdateSQLProcessor(gReportWriter, SQL)
        '    'i += 1

        'Next i
        'ret = io_DumpRecordsetToLocalTable(gReportWriter, ARrecordset, "Accounts")

        PrintControlSet = ""

        SQL = "SELECT * FROM Cash"
        PrintControlSegment = ""
        PrintControlSegment = AddElementToSegment(PrintControlSegment, "ReportTitle", "Cash")
        PrintControlSegment = AddElementToSegment(PrintControlSegment, "SQL", SQL)
        PrintControlSegment = AddElementToSegment(PrintControlSegment, "DataFiles", gReportWriter)
        PrintControlSet &= "<SET>" & PrintControlSegment & "</SET>" & vbCrLf

        SQL = "SELECT * FROM [Check]"
        PrintControlSegment = ""
        PrintControlSegment = AddElementToSegment(PrintControlSegment, "ReportTitle", "Check")
        PrintControlSegment = AddElementToSegment(PrintControlSegment, "SQL", SQL)
        PrintControlSegment = AddElementToSegment(PrintControlSegment, "DataFiles", gReportWriter)
        PrintControlSet = PrintControlSet & "<SET>" & PrintControlSegment & "</SET>" & vbCrLf

        SQL = "SELECT * FROM [Charge]"
        PrintControlSegment = ""
        PrintControlSegment = AddElementToSegment(PrintControlSegment, "ReportTitle", "ChargeCardsReceived")
        PrintControlSegment = AddElementToSegment(PrintControlSegment, "SQL", SQL)
        PrintControlSegment = AddElementToSegment(PrintControlSegment, "DataFiles", gReportWriter)
        PrintControlSet = PrintControlSet & "<SET>" & PrintControlSegment & "</SET>" & vbCrLf

        SQL = "SELECT * FROM [Other]"
        PrintControlSegment = ""
        PrintControlSegment = AddElementToSegment(PrintControlSegment, "ReportTitle", "OtherFormsOfPayment")
        PrintControlSegment = AddElementToSegment(PrintControlSegment, "SQL", SQL)
        PrintControlSegment = AddElementToSegment(PrintControlSegment, "DataFiles", gReportWriter)
        PrintControlSet = PrintControlSet & "<SET>" & PrintControlSegment & "</SET>" & vbCrLf

        SQL = "SELECT * FROM [Accounts]"
        PrintControlSegment = ""
        PrintControlSegment = AddElementToSegment(PrintControlSegment, "ReportTitle", "InvoiceBalances")
        PrintControlSegment = AddElementToSegment(PrintControlSegment, "SQL", SQL)
        PrintControlSegment = AddElementToSegment(PrintControlSegment, "DataFiles", gReportWriter)
        PrintControlSet = PrintControlSet & "<SET>" & PrintControlSegment & "</SET>" & vbCrLf

        SQL = "SELECT * FROM [Returns]"
        PrintControlSegment = AddElementToSegment(PrintControlSegment, "ReportTitle", "Returns")
        PrintControlSegment = AddElementToSegment(PrintControlSegment, "SQL", SQL)
        PrintControlSegment = AddElementToSegment(PrintControlSegment, "DataFiles", gReportWriter)
        PrintControlSet = PrintControlSet & "<SET>" & PrintControlSegment & "</SET>" & vbCrLf

        SQL = "SELECT * FROM Payments"
        PrintControlSegment = AddElementToSegment(PrintControlSegment, "ReportTitle", "CloseID's")
        PrintControlSegment = AddElementToSegment(PrintControlSegment, "SQL", SQL)
        PrintControlSegment = AddElementToSegment(PrintControlSegment, "DataFiles", gReportWriter)
        PrintControlSet = PrintControlSet & "<SET>" & PrintControlSegment & "</SET>" & vbCrLf

        SQL = "SELECT * FROM Payments"
        PrintControlSegment = AddElementToSegment(PrintControlSegment, "ReportTitle", "DrawerIDs")
        PrintControlSegment = AddElementToSegment(PrintControlSegment, "SQL", SQL)
        PrintControlSegment = AddElementToSegment(PrintControlSegment, "DataFiles", gReportWriter)
        PrintControlSet = PrintControlSet & "<SET>" & PrintControlSegment & "</SET>" & vbCrLf

        SQL = "SELECT * FROM Transactions"
        ret = PrintProductionBasedReports("ZReport", "Z Report", 2, Preview, StartDate, EndDate, SQL2)

        gCurrentCloseID = 0

    End Sub

    Private Sub Report_SalesInquiry_Click(sender As Object, e As RoutedEventArgs)
        Me.Cursor = Cursors.Wait
        Dim ret As Integer
        ret = PrintProductionBasedReports("SalesInquiry", "Sales Inquiry", 3, True, ReportStartDate.SelectedDate, ReportEndDate.SelectedDate)
        Me.Cursor = Cursors.Arrow
    End Sub

    Private Sub Report_SalesTax_Click(sender As Object, e As RoutedEventArgs)
        Me.Cursor = Cursors.Wait
        Dim ret As Integer
        ret = PrintProductionBasedReports("SalesTax", "Sales Taxes", 2, False, ReportStartDate.SelectedDate, ReportEndDate.SelectedDate)
        Me.Cursor = Cursors.Arrow
    End Sub

    Private Sub Report_SalesTaxByAR_Click(sender As Object, e As RoutedEventArgs)
        Me.Cursor = Cursors.Wait
        Dim ret As Integer
        ret = PrintProductionBasedReports("SalesTax_Accts", "Sales Taxes", 2, False, ReportStartDate.SelectedDate, ReportEndDate.SelectedDate)
        Me.Cursor = Cursors.Arrow
    End Sub

    Private Sub Report_Cash_PaidOut_Click(sender As Object, e As RoutedEventArgs)
        Me.Cursor = Cursors.Wait
        Dim ret As Integer
        ret = PrintProductionBasedReports("PaidOut", "Paid Out", 2, False, ReportStartDate.SelectedDate, ReportEndDate.SelectedDate)
        Me.Cursor = Cursors.Arrow
    End Sub


    Private Sub Report_HourlySales_Click(sender As Object, e As RoutedEventArgs)
        Try

            UpdateHourlySalesTable(ReportStartDate.SelectedDate, ReportEndDate.SelectedDate)
            Cursor = Cursors.Wait
            Dim report As New _ReportObject()
            report.ReportName = "Hourly.rpt"


            Dim reportPrev As New ReportPreview(report)
            Cursor = Cursors.Arrow
            reportPrev.ShowDialog()


        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to report [Inventory Listing]...")
        Finally : Cursor = Cursors.Arrow
        End Try


    End Sub

    Private Sub LoadCloseIDs_Btn_Click(sender As Object, e As RoutedEventArgs) Handles LoadCloseIDs_Btn.Click

        Dim SQL As String
        Dim CloseItem As CloseIDItem
        Dim SegmentSet As String
        Dim Segment As String

        If ReportStartDate.Text = "" Or ReportEndDate.Text = "" Then
            MsgBox("Please enter a start and end date first!", vbExclamation)
            Exit Sub
        End If

        CLoseID_List = New List(Of CloseIDItem)

        SQL = "Select CloseID, CloseDate from OpenClose Where CloseDate >=#" & ReportStartDate.Text & "# AND CloseDate <= #" & ReportEndDate.Text & "#"
        SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)

        If SegmentSet = "" Then
            MsgBox("No Closings found in selected date rage!", vbExclamation)
            Exit Sub
        End If

        Do Until SegmentSet = ""
            CloseItem = New CloseIDItem
            Segment = GetNextSegmentFromSet(SegmentSet)

            CloseItem.CloseDate = ExtractElementFromSegment("CloseDate", Segment)
            CloseItem.CloseID = ExtractElementFromSegment("CloseID", Segment)
            CLoseID_List.Add(CloseItem)
        Loop

        ZReport_CloseID_LV.ItemsSource = CLoseID_List
        ZReport_CloseID_LV.Items.Refresh()

    End Sub

    Private Sub MBX_AlphaListing_Btn_Click(sender As Object, e As RoutedEventArgs) Handles MBX_AlphaListing_Btn.Click
        Try
            Cursor = Cursors.Wait
            Dim report As New _ReportObject()
            report.ReportName = "MBXAlphaListing.rpt"


            Dim reportPrev As New ReportPreview(report)
            Cursor = Cursors.Arrow
            reportPrev.ShowDialog()

        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to report [Alpha Listing]...")
        Finally : Cursor = Cursors.Arrow
        End Try

    End Sub

    Private Sub Reports_ZReport_ByCloseID_Click(sender As Object, e As RoutedEventArgs) Handles Reports_ZReport_ByCloseID.Click
        Dim closeIDs As String = ""

        If ZReport_CloseID_LV.SelectedItems.Count = 0 Then Exit Sub

        Me.Cursor = Cursors.Wait

        For Each item As CloseIDItem In ZReport_CloseID_LV.SelectedItems
            closeIDs = closeIDs & "," & item.CloseID

        Next

        gResult = AddElementToSegment(gResult, "CloseIDs", closeIDs.Substring(1))

        Print_ZReport(ReportStartDate.SelectedDate, ReportEndDate.SelectedDate)

        Me.Cursor = Cursors.Arrow
    End Sub

#Region "Mailboxes"

    Private Sub MBX_Listing_Btn_Click(sender As Object, e As RoutedEventArgs) Handles MBX_Listing_Btn.Click
        Try
            Cursor = Cursors.Wait
            Dim report As New _ReportObject()
            report.ReportName = "MailboxListing.rpt"


            Dim reportPrev As New ReportPreview(report)
            Cursor = Cursors.Arrow
            reportPrev.ShowDialog()

        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to report [Mailbox Listing]...")
        Finally : Cursor = Cursors.Arrow
        End Try
    End Sub

    Private Sub MBX_CancelledBoxes_Btn_Click(sender As Object, e As RoutedEventArgs) Handles MBX_CancelledBoxes_Btn.Click
        Try
            Cursor = Cursors.Wait
            Dim report As New _ReportObject()
            report.ReportName = "PostOffice_CancelledMBoxes.rpt"

            report.ReportFormula = "{MBXHistory.Date} > #" & Today.AddMonths(-6).ToShortDateString & "# and {MBXHistory.Desc}='Cancel Mailbox'"

            Dim reportPrev As New ReportPreview(report)
            Cursor = Cursors.Arrow
            reportPrev.ShowDialog()

        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to report [Cancelled Mailboxes]...")
        Finally : Cursor = Cursors.Arrow
        End Try

    End Sub

    Private Sub Expired_Mailboxes_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Expired_Mailboxes.Click
        Try
            Cursor = Cursors.Wait
            Dim report As New _ReportObject()
            Dim sql As String
            Dim RecordCT As Long = 0
            Dim dbSchema As String = ""
            Dim pEndDate As Integer = 0
            Dim pNewStatus As Integer = 0
            Dim MailboxRecordSet As RecordSetDefinition
            Dim ret As String
            Dim segment As String

            ret = IO_UpdateSQLProcessor(gReportWriter, "DELETE * FROM MailBox")

            sql = "SELECT * FROM Mailbox"  ' Get all mailboxes

            RecordCT = IO_GetSegmentSetInToStructure(gShipriteDB, sql, MailboxRecordSet)
            dbSchema = IO_GetFieldsCollection(gReportWriter, "Mailbox", "", True, False, True)



            pEndDate = GetFieldNumber(MailboxRecordSet, "EndDate")  ' Get EndDate field index
            Dim renewalDays As Integer = GetPolicyData(gShipriteDB, "MBXDaysRenewal")
            ' Copy records to gReportWriter and set Status field dynamically
            For i = 0 To MailboxRecordSet.RecordCount - 1
                Dim endDate As Date = MailboxRecordSet.RecordSet(i).Field(pEndDate).FValue
                Dim status As String
                ' Determine the status based on expiration logic
                If endDate < Date.Today Then
                    status = "Expired"
                ElseIf Date.Today >= DateAdd("d", renewalDays, endDate) Then
                    status = "Renewal Due"
                Else
                    Continue For
                    'status = "Active"
                End If

                ' Copy the record and manually add Status field in gReportWriter

                segment = MakeSegmentFromRecord(MailboxRecordSet, i, True)

                segment = AddElementToSegment(segment, "Status", status) ' Adding Status dynamically
                sql = MakeInsertSQLFromSchema("Mailbox", segment, dbSchema, True)
                ret += IO_UpdateSQLProcessor(gReportWriter, sql)
            Next



            report.ReportName = "Mailboxes_Expired.rpt"

            Dim reportPrev As New ReportPreview(report)
            Cursor = Cursors.Arrow
            reportPrev.ShowDialog()

        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to report [Expired Mailboxes]...")
        Finally : Cursor = Cursors.Arrow
        End Try

    End Sub

    Private Sub MBX_PostOfficeQuarterly_Btn_Click(sender As Object, e As RoutedEventArgs) Handles MBX_PostOfficeQuarterly_Btn.Click
        Try
            Cursor = Cursors.Wait
            Dim report As New _ReportObject()
            report.ReportName = "PostOffice.rpt"


            Dim reportPrev As New ReportPreview(report)
            Cursor = Cursors.Arrow
            reportPrev.ShowDialog()

        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to report [Post Office Quarterly]...")
        Finally : Cursor = Cursors.Arrow
        End Try
    End Sub

#End Region


#Region "SHIP Reports"
    Private Sub SHIP_ByCarrierAdvanced_Btn_Click(sender As Object, e As RoutedEventArgs) Handles SHIP_ByCarrierAdvanced_Btn.Click
        Try
            Cursor = Cursors.Wait
            Dim report As New _ReportObject()
            report.ReportName = "Corder.rpt"

            report.ReportFormula = CreateDateFormula() & Create_CarrierSelectionFormula()
            Dim reportPrev As New ReportPreview(report)
            Cursor = Cursors.Arrow
            reportPrev.ShowDialog()

        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to report [Post Office Quarterly]...")
        Finally : Cursor = Cursors.Arrow
        End Try
    End Sub

    Private Sub SHIP_ByCarrierOrig_Btn_Click(sender As Object, e As RoutedEventArgs) Handles SHIP_ByCarrierOrig_Btn.Click
        Try
            Cursor = Cursors.Wait
            Dim report As New _ReportObject()
            report.ReportName = "Corder_Original.rpt"

            report.ReportFormula = CreateDateFormula() & Create_CarrierSelectionFormula()
            Dim reportPrev As New ReportPreview(report)
            Cursor = Cursors.Arrow
            reportPrev.ShowDialog()

        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to report [Post Office Quarterly]...")
        Finally : Cursor = Cursors.Arrow
        End Try
    End Sub

    Private Sub SHIP_ByCarriersummary_Btn_Click(sender As Object, e As RoutedEventArgs) Handles SHIP_ByCarriersummary_Btn.Click
        Try
            Cursor = Cursors.Wait
            Dim report As New _ReportObject()
            report.ReportName = "Corder_summary.rpt"

            report.ReportFormula = CreateDateFormula() & Create_CarrierSelectionFormula()
            Dim reportPrev As New ReportPreview(report)
            Cursor = Cursors.Arrow
            reportPrev.ShowDialog()

        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to report [Carrier_summary]...")
        Finally : Cursor = Cursors.Arrow
        End Try
    End Sub



    Private Sub SHIP_ByInvoice_Btn_Click(sender As Object, e As RoutedEventArgs) Handles SHIP_ByInvoice_Btn.Click
        Try
            Cursor = Cursors.Wait
            Dim report As New _ReportObject()
            report.ReportName = "Invorder.rpt"

            report.ReportFormula = CreateDateFormula() & Create_CarrierSelectionFormula()
            Dim reportPrev As New ReportPreview(report)
            Cursor = Cursors.Arrow
            reportPrev.ShowDialog()

        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to report [Post Office Quarterly]...")
        Finally : Cursor = Cursors.Arrow
        End Try
    End Sub

    Private Sub SHIP_ByZone_Btn_Click(sender As Object, e As RoutedEventArgs) Handles SHIP_ByZone_Btn.Click
        Try
            Cursor = Cursors.Wait
            Dim report As New _ReportObject()
            report.ReportName = "Zorder.rpt"

            report.ReportFormula = CreateDateFormula() & Create_CarrierSelectionFormula()
            Dim reportPrev As New ReportPreview(report)
            Cursor = Cursors.Arrow
            reportPrev.ShowDialog()

        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to report [Post Office Quarterly]...")
        Finally : Cursor = Cursors.Arrow
        End Try
    End Sub

    Private Sub SHIP_Insurance_Btn_Click(sender As Object, e As RoutedEventArgs) Handles SHIP_Insurance_Btn.Click
        Try
            Cursor = Cursors.Wait
            Dim report As New _ReportObject()
            report.ReportName = "Insurance.rpt"


            report.ReportFormula = CreateDateFormula() & Create_CarrierSelectionFormula()
            report.SubReports.Add(CreateDateFormula() & Create_CarrierSelectionFormula(), "Summary")

            report.ReportParameters.Add(ReportStartDate.Text & " - " & ReportEndDate.Text)
            report.ReportParameters.Add(GetPolicyData(gShipriteDB, "ThirdPartyAddress", ""))

            Dim reportPrev As New ReportPreview(report)
            Cursor = Cursors.Arrow
            reportPrev.ShowDialog()

        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to report [Post Office Quarterly]...")
        Finally : Cursor = Cursors.Arrow
        End Try
    End Sub



    Private Function Create_CarrierSelectionFormula()
        If SHIP_Carrier_Cmb.SelectedIndex <> 0 Then
            Return " AND {Manifest.Carrier} = '" & SHIP_Carrier_Cmb.Text & "' AND {Manifest.Exported} <> 'Deleted'"
        Else
            Return " AND {Manifest.Exported} <> 'Deleted'"
        End If

    End Function

    Private Function CreateDateFormula() As String
        Return "{Manifest.Date} >=#" & ReportStartDate.Text & "# AND {Manifest.Date} <= #" & ReportEndDate.Text & "#"
    End Function

#End Region

    Private Sub INVENTORY_Listing_Btn_Click(sender As Object, e As RoutedEventArgs) Handles INVENTORY_Listing_Btn.Click
        Try
            Cursor = Cursors.Wait
            Dim report As New _ReportObject()
            report.ReportName = "Inventory.rpt"


            Dim reportPrev As New ReportPreview(report)
            Cursor = Cursors.Arrow
            reportPrev.ShowDialog()

        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to report [Inventory Listing]...")
        Finally : Cursor = Cursors.Arrow
        End Try
    End Sub

    Private Sub INVENTORY_Valuation_Btn_Click(sender As Object, e As RoutedEventArgs) Handles INVENTORY_Valuation_Btn.Click
        Try
            Cursor = Cursors.Wait
            Dim report As New _ReportObject()
            report.ReportName = "InventoryValuation.rpt"


            Dim reportPrev As New ReportPreview(report)
            Cursor = Cursors.Arrow
            reportPrev.ShowDialog()

        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to report [Inventory Listing]...")
        Finally : Cursor = Cursors.Arrow
        End Try
    End Sub

    Private Sub INVENTORY_Out_Stock_Click(sender As Object, e As RoutedEventArgs) Handles INVENTORY_Out_Stock.Click
        Try
            Cursor = Cursors.Wait
            Dim report As New _ReportObject()
            report.ReportName = "Inventory_OutStock.rpt"

            report.ReportFormula = "{Inventory.Active} <> False AND {Inventory.Zero} <> False AND {Inventory.Quantity} <= {Inventory.WarningQty}"

            Dim reportPrev As New ReportPreview(report)
            Cursor = Cursors.Arrow
            reportPrev.ShowDialog()

        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to report [Inventory Listing]...")
        Finally : Cursor = Cursors.Arrow
        End Try
    End Sub

    Private Sub INVENTORY_Label_Btn_Click(sender As Object, e As RoutedEventArgs)





        Dim priceLabelWindow As New Price_label()
        If priceLabelWindow.ShowDialog() = True Then
            ' Handle the result from the Price_label modal window
        Else
            ' Handle cancellation or other scenarios
        End If
    End Sub



    Private Sub Report_HourlySalesForWeek_Click(sender As Object, e As RoutedEventArgs)
        Try

            UpdateHourlySalesTable(ReportStartDate.SelectedDate, ReportEndDate.SelectedDate)
            Cursor = Cursors.Wait
            Dim report As New _ReportObject()
            report.ReportName = "HourlyByDay.rpt"


            Dim reportPrev As New ReportPreview(report)
            Cursor = Cursors.Arrow
            reportPrev.ShowDialog()


        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to report [Inventory Listing]...")
        Finally : Cursor = Cursors.Arrow
        End Try
    End Sub



    Private Sub Vault_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Vault_Btn.Click
        Try
            Cursor = Cursors.Wait
            Dim report As New _ReportObject()
            report.ReportName = "Vault.rpt"

            report.ReportFormula = "{Payments.Date} >=#" & ReportStartDate.Text & "# AND {Payments.Date} <= #" & ReportEndDate.Text & "# AND {Payments.SalesRep} LIKE 'VAULT'"
            Dim reportPrev As New ReportPreview(report)
            Cursor = Cursors.Arrow
            reportPrev.ShowDialog()

        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to report [Vault]...")
        Finally : Cursor = Cursors.Arrow
        End Try
    End Sub

    Public Shared Sub PrintCommercialInvoice(PackageID As String)
        Try

            Dim report As New SHIPRITE.ShipRiteReports._ReportObject()
            report.ReportName = "ExportDocument.rpt"
            report.ReportFormula = "{Manifest.PackageID} = '" & PackageID & "'"

            Dim reportPrev As New ReportPreview(report)

            reportPrev.ShowDialog()

        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to report [Commercial Invoice]...")

        End Try
    End Sub

    Private Sub Reports_VoidCancel_Click(sender As Object, e As RoutedEventArgs) Handles Reports_VoidCancel.Click
        Try
            Cursor = Cursors.Wait
            Dim report As New _ReportObject()
            report.ReportName = "Void.rpt"

            report.ReportFormula = "{Void.Date} >=#" & ReportStartDate.Text & "# AND {Void.Date} <= #" & ReportEndDate.Text & "#"
            Dim reportPrev As New ReportPreview(report)
            Cursor = Cursors.Arrow
            reportPrev.ShowDialog()

        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to report [Void/Cancel]...")
        Finally : Cursor = Cursors.Arrow
        End Try
    End Sub


#Region "Accounts Receivable"
    Private Sub AR_Alpha_Listing_Click(sender As Object, e As RoutedEventArgs) Handles AR_Alpha_Listing_Btn.Click
        Try
            Cursor = Cursors.Wait
            Dim report As New _ReportObject()
            report.ReportName = "AlphaList.rpt"


            Dim reportPrev As New ReportPreview(report)
            Cursor = Cursors.Arrow
            reportPrev.ShowDialog()

        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to report [AR Alpha Listing]...")
        Finally : Cursor = Cursors.Arrow
        End Try
    End Sub

    Private Sub AR_Aging_Btn_Click(sender As Object, e As RoutedEventArgs) Handles AR_Aging_Btn.Click

        Dim ret As Integer
        Try
            Mouse.OverrideCursor = Cursors.Wait

            Dim report As New _ReportObject()
            report.ReportName = "Aging.rpt"

            ret = ProcessARAging()

            Dim reportPrev As New ReportPreview(report)

            Mouse.OverrideCursor = Cursors.Arrow
            reportPrev.ShowDialog()

        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to report [AR Aging]...")
        Finally : Cursor = Cursors.Arrow
        End Try
    End Sub

    Private Sub AR_SearchName_Btn_Click(sender As Object, e As RoutedEventArgs) Handles AR_SearchName_Btn.Click
        AR_SearchByName()
    End Sub

    Private Sub AR_AcctName_TxtBx_KeyDown(sender As Object, e As KeyEventArgs) Handles AR_AcctName_TxtBx.KeyDown
        If e.Key = Key.Return Then
            AR_SearchByName()
        End If
    End Sub

    Private Sub AR_SearchByName()
        Dim buf As String = ""
        Dim SQL = "SELECT AcctName, Addr1+chr(13)+City+', '+State+'  '+ZipCode AS FullAddress, Phone, AcctNum FROM AR WHERE AcctName LIKE '<<SEED>>%' ORDER BY AcctName"

        buf = SearchList(Me, AR_AcctName_TxtBx.Text, "AR", "AcctName", "Account Search", SQL, AR_AcctName_TxtBx.Text)

        If Not buf = "" Then
            AR_AcctNo_TxtBx.Text = buf

            AR_AcctName_TxtBx.Text = ExtractElementFromSegment("AcctName", IO_GetSegmentSet(gShipriteDB, "SELECT AcctName FROM AR WHERE AcctNum = '" & buf & "'"))
            AR_ForAccount_RdBtn.IsChecked = True
        End If
    End Sub

    Private Sub AR_SearchAcctNo_Btn_Click(sender As Object, e As RoutedEventArgs) Handles AR_SearchAcctNo_Btn.Click
        AR_SearchByAcctNo()
    End Sub

    Private Sub AR_AcctNo_TxtBx_KeyDown(sender As Object, e As KeyEventArgs) Handles AR_AcctNo_TxtBx.KeyDown
        If e.Key = Key.Return Then
            AR_SearchByAcctNo()
        End If
    End Sub

    Private Sub AR_SearchByAcctNo()
        Dim buf As String = ""
        Dim SQL = "SELECT AcctName, Addr1+chr(13)+City+', '+State+'  '+ZipCode AS FullAddress, Phone, AcctNum FROM AR WHERE AcctNum LIKE '<<SEED>>%' ORDER BY AcctNum"

        buf = SearchList(Me, AR_AcctNo_TxtBx.Text, "AR", "AcctNum", "Account Search", SQL, AR_AcctNo_TxtBx.Text)

        If Not buf = "" Then
            AR_AcctNo_TxtBx.Text = buf
            AR_AcctName_TxtBx.Text = ExtractElementFromSegment("AcctName", IO_GetSegmentSet(gShipriteDB, "SELECT AcctName FROM AR WHERE AcctNum = '" & buf & "'"))
            AR_ForAccount_RdBtn.IsChecked = True
        End If
    End Sub

    Private Sub AR_PrintStatements_Btn_Click(sender As Object, e As RoutedEventArgs) Handles AR_PrintStatements_Btn.Click
        Dim StatementCount As Integer

        StatementCount = ProcessStatements()

        If StatementCount > 0 Then
            PrintStatements()
        End If

    End Sub

    Private Function ProcessStatements() As Integer
        Dim ans As Integer
        Dim ret As Integer
        Dim Balance, Current, Plus30, Plus60, Plus90, Plus120 As Double
        Dim BID As Long
        Dim StDate As String = Format(Today, "MM/01/yyyy")
        Dim SDate As String = Format(Today, "MM/dd/yyyy")
        Dim AsOfDate As String = Format(Today, "MM/dd/yyyy")
        Dim DueDate As String = Format(Today.AddDays(15), "MM/dd/yyyy")
        Dim Args As String
        Dim StatementCT As Integer
        Dim SQL As String
        Dim RecordCT As Long
        Dim i As Long = 0
        Dim j As Long = 0
        Dim ARrecordset As RecordSetDefinition
        Dim AcctNum As String = ""
        Dim SegmentSet As String = ""
        Dim GoPrint As Boolean = False

        SQL = "UPDATE Payments SET AcctNum = 'Cash' WHERE AcctNum = ''"
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

        SQL = "UPDATE Transactions SET AcctNum = 'Cash' WHERE AcctNum = ''"
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

        Args = ""

        If IsNothing(AR_StatementDate_DP.SelectedDate) Then
            AR_StatementDate_DP.SelectedDate = Format(Today, "MM/dd/yyyy")
        End If
        Args = AddElementToSegment(Args, "StatementDate", AR_StatementDate_DP.SelectedDate)


        If IsNothing(AR_DueDate_DP.SelectedDate) Then
            AR_DueDate_DP.SelectedDate = Format(Today.AddDays(15), "MM/dd/yyyy")
        End If
        Args = AddElementToSegment(Args, "DueDate", AR_DueDate_DP.SelectedDate)


        ret = IO_UpdateSQLProcessor(gReportWriter, "DELETE * FROM Statements")
        ret = Update_ReportWriter_Setup()

        If AR_AllAccounts_RdBtn.IsChecked Then
            'Batch Statements for all accounts

            ans = MsgBox("ATTENTION...Create Statement Batch" & vbCrLf & vbCrLf &
                "Creating a statement batch may take some time." & vbCrLf & vbCrLf & "DO YOU WISH TO CONTINUE?", vbQuestion + vbYesNo, gProgramName)

            If ans = vbYes Then
                Mouse.OverrideCursor = Cursors.Wait
                BID = GetNextCounter(gShipriteDB, "BatchID", "Statements")

                ' If this is Grouped by AcctName, then returned records can be multiplied causing the report to take almost 15x longer.
                ' Report results are the same with difference being Accounts sorted by Name instead of Number.
                ' E.g., Sean Harrigan database test went from 600 -> 14000 records causing processing to go from 36 sec to 530 sec (almost 9 min)
                ' If want resulting report to sort by AcctName, need to find a better way.
                ' Reverted change since SRPro doesn't sort by AcctName so it'll work the same in SRN fine for now.
                'SQL = "SELECT AcctNum, sum(charge) - sum(payment) as Balance FROM Payments GROUP BY AcctName, AcctNum"
                SQL = "SELECT AcctNum, sum(charge) - sum(payment) as Balance FROM Payments GROUP BY AcctNum"
                RecordCT = IO_GetSegmentSetInToStructure(gShipriteDB, SQL, ARrecordset)

                StatementCT = 0
                For i = 0 To ARrecordset.RecordCount - 1

                    AcctNum = ARrecordset.RecordSet(i).Field(0).FValue
                    Balance = Round(Val(ARrecordset.RecordSet(i).Field(1).FValue), 2)
                    If AR_AllAccounts_RdBtn.IsChecked And Not AcctNum = "" Then

                        SQL = "SELECT SendStatement FROM AR WHERE AcctNum = '" & AcctNum & "'"
                        SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
                        If ExtractElementFromSegment("SendStatement", SegmentSet) = "False" Then

                            AcctNum = ""

                        End If

                    End If
                    GoPrint = True
                    If UCase(AcctNum) = "CASH" Then

                        GoPrint = False

                    End If
                    If AcctNum = "" Then

                        GoPrint = False

                    End If
                    If OnlyStatementsWithBalance.IsChecked And Balance = 0 Then

                        GoPrint = False

                    End If
                    If PrintBalanceOver.IsChecked And Not Balance > Val(BalancesOverAmount.Text) Then

                        GoPrint = False

                    End If
                    If Balance < 0 Then

                        GoPrint = False

                    End If
                    If GoPrint = True Then

                        StatementCT += ProduceStatementsByInvoice(Args, BID, AcctNum)

                    End If

                Next
                i = i
            End If

        Else

            'statement for single account
            ans = MsgBox("ATTENTION...Creating Statement for " & AR_AcctName_TxtBx.Text & vbCrLf & vbCrLf &
                "Before running you should review the Accounts Receivable" & vbCrLf &
                "Aging Report for accuracy." & vbCrLf & vbCrLf & "DO YOU WISH TO CONTINUE?", vbQuestion + vbYesNo, gProgramName)

            Mouse.OverrideCursor = Cursors.Wait
            BID = GetNextCounter(gShipriteDB, "BatchID", "Statements")
            ret = Account_Aging(gShipriteDB, AR_AcctName_TxtBx.Text, Balance, Current, Plus30, Plus60, Plus90, Plus120)
            StatementCT = ProduceStatementsByInvoice(Args, BID, AR_AcctNo_TxtBx.Text)

            'End If

        End If
        Mouse.OverrideCursor = Cursors.Arrow

        Return StatementCT
    End Function

    Private Function ProcessARAging() As Integer

        Dim SQL As String = ""
        Dim Segment As String = ""
        Dim SegmentSet As String = ""
        Dim ARAgingRecordSet As RecordSetDefinition
        Dim Recordct As Long
        Dim i As Long = 0
        Dim AcctNum As String = ""
        Dim AcctName As String = ""
        Dim Phone As String = ""
        Dim LastPayDate As String = ""
        Dim LastPAmt As String = ""
        Dim Balance As Double = 0.0
        Dim Current As Double = 0.0
        Dim Plus30 As Double = 0.0
        Dim Plus60 As Double = 0.0
        Dim Plus90 As Double = 0.0
        Dim Plus120 As Double = 0.0
        Dim ret As Integer
        Dim ct As Integer = 0
        Dim TheDate As String
        Dim ID As String = ""
        Dim buf As String = ""

        ret = IO_UpdateSQLProcessor(gShipriteDB, "DELETE * FROM ARAging")
        TheDate = Format(Today, "MM/dd/yyyy")
        SQL = "SELECT AcctNum, FIRST(AcctName) AS AcctName, SUM(Charge - Payment) AS Balance FROM Payments WHERE Status = 'Ok' AND AcctNum <> '' AND Ucase(AcctNum) <> 'CASH' GROUP BY AcctNum ORDER BY AcctNum"
        Recordct = IO_GetSegmentSetInToStructure(gShipriteDB, SQL, ARAgingRecordSet)
        For i = 0 To Recordct - 1

            AcctNum = ARAgingRecordSet.RecordSet(i).Field(0).FValue
            AcctName = ARAgingRecordSet.RecordSet(i).Field(1).FValue
            If AcctName = "" Then

                SQL = "SELECT TOP 1 AcctName From Payments WHERE AcctNum = '" & AcctNum & "' AND NOT AcctName = ''"
                buf = IO_GetSegmentSet(gShipriteDB, SQL)
                AcctName = ExtractElementFromSegment("AcctName", buf)
                If AcctName = "" Then

                    AcctName = "UNKNOWN"

                End If

            End If
            Balance = Round(Val(ARAgingRecordSet.RecordSet(i).Field(2).FValue), 2)
            If Balance > 0.01 And Not AcctNum = "CASH" Then

                ct = ct + 1
                ID = GetNextIDNumber(gShipriteDB, "ARAging").ToString
                ret = Account_Aging(gShipriteDB, AcctNum, Balance, Current, Plus30, Plus60, Plus90, Plus120)
                SQL = "SELECT ID, Phone, LastPDate AS LastPayDate, LastPAmt FROM AR WHERE AcctNum = '" & AcctNum & "'"
                SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
                Segment = GetNextSegmentFromSet(SegmentSet)
                Phone = ExtractElementFromSegment("Phone", Segment)
                LastPayDate = ExtractElementFromSegment("LastPayDate", Segment)
                LastPAmt = ExtractElementFromSegment("LastPAmt", Segment)
                Segment = AddElementToSegment(Segment, "ID", ID)
                Segment = AddElementToSegment(Segment, "AcctNum", AcctNum)
                Segment = AddElementToSegment(Segment, "AcctName", AcctName)
                Segment = AddElementToSegment(Segment, "Current", Current)
                Segment = AddElementToSegment(Segment, "Plus30", Plus30)
                Segment = AddElementToSegment(Segment, "Plus60", Plus60)
                Segment = AddElementToSegment(Segment, "Plus90", Plus90)
                Segment = AddElementToSegment(Segment, "Plus120", Plus120)
                Segment = AddElementToSegment(Segment, "Balance", Balance)
                Segment = AddElementToSegment(Segment, "ReportDate", TheDate)
                Segment = AddElementToSegment(Segment, "AsOfDate", TheDate)
                SQL = MakeInsertSQLFromSchema("ARAging", Segment, ARAgingSchema)
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

            End If

        Next
        Return 0

    End Function

    Public Function ProduceStatementsByInvoice(InputSegment As String, BatchID As Long, ARNum As String) As Integer

        Dim MinInvNum As Long
        Dim MaxInvNum As Long

        Dim ct As Integer
        Dim SQL As String
        Dim SegmentSet As String
        Dim ret As Long
        Dim ANum As String
        Dim TotalBalance As Double
        Dim AccountBalance As Double
        Dim Current As Double
        Dim Plus30 As Double
        Dim Plus60 As Double
        Dim Plus90 As Double
        Dim Plus120 As Double
        Dim i As Long
        Dim j As Long
        Dim K As Long
        Dim LASTInvNum As String
        Dim StatementID As Long
        Dim StatementsID As Long
        Dim ARSegment As String
        Dim NewSegment As String
        Dim buf As String
        Dim ID As Long
        Dim InvBalance As Double
        Dim PID, pTransactionDate, pAcctNum, pAcctName, pInvNum, PDesc, PType, pStatus, pCharge, pPayment As Integer
        Dim LastANum As String
        Dim InvCharge As Double
        Dim InvPayment As Double
        Dim Header As String
        Dim BaseInvNum As Long
        Dim Email As String = ""

        Dim StatementMessage As String
        Dim SendBlock As String

        Dim ARB() As ARBrecord
        Dim ARBct As Long

        Dim ARN() As ARNameRecordSet
        Dim ARNct As Long

        Dim AName As String
        Dim LastAName As String

        Dim TDate As Date
        Dim InvoiceStack As String
        Dim ARRecordSet As RecordSetDefinition

        Dim STotal As Double
        Dim RecordID As Long
        Dim FinanceRate As Double
        Dim DueDate As String
        Dim SEQ As Integer
        Dim NumberOfStatements As Integer
        Dim BaseID As Long

        PID = 0
        pTransactionDate = 1
        pAcctNum = 2
        pAcctName = 3
        pInvNum = 4
        PDesc = 5
        PType = 6
        pStatus = 7
        pCharge = 8
        pPayment = 9

        DueDate = ExtractElementFromSegment("DueDate", InputSegment)
        Dim AcctName As String
        Dim MessageCT As Integer

        SQL = "SELECT ID, [Date], AcctNum, AcctName, InvNum, [Desc], [Type], Status, Charge, Payment FROM Payments WHERE AcctNum = '" & ARNum & "' ORDER BY InvNum"
        RecordID = IO_GetSegmentSetInToStructure(gShipriteDB, SQL, ARRecordSet, , , "CommonTableProcessor_General_ProduceStatementsByInvoice")

        If RecordID = 0 Then

            Return 0
            Exit Function

        End If
        BaseID = 0
        Do Until BaseID > 0

            If Val(ARRecordSet.RecordSet(0).Field(pInvNum).FValue) > 0 Then

                Exit Do

            End If
            BaseID += 1

        Loop
        MinInvNum = Val(ARRecordSet.RecordSet(BaseID).Field(pInvNum).FValue)
        MaxInvNum = Val(ARRecordSet.RecordSet(BaseID).Field(pInvNum).FValue)
        For i = BaseID To ARRecordSet.RecordCount - 1

            ID = Val(ARRecordSet.RecordSet(i).Field(pInvNum).FValue)
            If ID < MinInvNum Then

                MinInvNum = ID

            End If
            If ID > MaxInvNum Then

                MaxInvNum = ID

            End If

        Next i
        BaseInvNum = MinInvNum
        ARBct = MaxInvNum - MinInvNum + 1
        ReDim ARB(ARBct + 1)
        ReDim ARN(ARBct + 1)
        For i = 0 To ARBct - 1

            ARB(i).AcctNum = ""
            ARB(i).AcctName = ""
            ARB(i).Charges = 0
            ARB(i).Payments = 0
            ARB(i).InvNum = 0
            ARB(i).PONum = ""
            ARB(i).BALANCE = 0
            ARB(i).FirstDate = Today

        Next i
        For i = 0 To ARRecordSet.RecordCount - 1

            If ARRecordSet.RecordSet(i).Field(pStatus).FValue = "Ok" Then

                ID = Val(ARRecordSet.RecordSet(i).Field(pInvNum).FValue) - BaseInvNum
                ARB(ID).AcctNum = ARRecordSet.RecordSet(i).Field(pAcctNum).FValue
                ARB(ID).AcctName = ARRecordSet.RecordSet(i).Field(pAcctName).FValue
                ARB(ID).Charges = ARB(ID).Charges + Val(ARRecordSet.RecordSet(i).Field(pCharge).FValue)
                ARB(ID).Payments = ARB(ID).Payments + Val(ARRecordSet.RecordSet(i).Field(pPayment).FValue)
                ARB(ID).BALANCE = Round(ARB(ID).Charges - ARB(ID).Payments, 2)
                ARB(ID).InvNum = Val(ARRecordSet.RecordSet(i).Field(pInvNum).FValue)
                buf = ARRecordSet.RecordSet(i).Field(pTransactionDate).FValue
                If Not buf = "" Then

                    TDate = buf

                Else

                    TDate = Today

                End If
                If TDate < ARB(ID).FirstDate Then

                    ARB(ID).FirstDate = TDate

                End If

            End If

        Next i

        ' Sort the Account Names

        AName = ARB(0).AcctName
        LastAName = AName
        ARN(0).AcctName = ARB(0).AcctName
        ARN(0).AcctNum = ARB(0).AcctNum
        ARNct = 1
        For i = 0 To ARBct - 1

            ANum = ARB(i).AcctNum
            AName = ARB(i).AcctName
            If Not AName = "" And Round(ARB(i).BALANCE, 2) > 0 Then

                For j = 0 To ARNct - 1

                    If AName = ARN(j).AcctName And ANum = ARN(j).AcctNum Then

                        Exit For

                    End If
                    If AName < ARN(j).AcctName Then

                        For K = ARNct To j + 1 Step -1

                            ARN(K) = ARN(K - 1)

                        Next K
                        ARN(K).AcctName = AName
                        ARN(K).AcctNum = ANum
                        ARNct = ARNct + 1
                        Exit For

                    End If

                Next j
                If j = ARNct Then

                    ARN(j).AcctName = AName
                    ARN(j).AcctNum = ANum
                    ARNct = ARNct + 1

                End If

            End If

        Next i

        ' Counting Invoice Numbers with Balances and creating invoice number stack

        ct = 0
        InvoiceStack = ""
        For i = 0 To ARBct - 1

            If Not ARB(i).BALANCE = 0 Then

                ct = ct + 1
                If Not InvoiceStack = "" Then

                    InvoiceStack = InvoiceStack & ", "

                End If
                InvoiceStack = InvoiceStack & ARB(i).InvNum

            End If

        Next i

        ct = 0
        LastANum = ""
        LASTInvNum = ""
        InvCharge = 0
        InvPayment = 0
        InvBalance = 0

        ' This is the statement number

        StatementID = GetNextCounter(gShipriteDB, "StatementID", "Statements")
        StatementsID = GetNextIDNumber(gShipriteDB, "Statements")
        NumberOfStatements = 0
        For i = 0 To ARNct - 1

            ANum = ARN(i).AcctNum
            AName = ARN(i).AcctName


            ' The new Aging of Accounts Method

            Current = 0
            Plus30 = 0
            Plus60 = 0
            Plus90 = 0
            Plus120 = 0
            TotalBalance = 0
            For j = 0 To ARBct - 1

                If ARB(j).AcctNum = ANum And Not Round(ARB(j).BALANCE, 2) = 0 Then

                    If ARB(j).BALANCE < 0 Then

                        Current = Current + Round(ARB(j).BALANCE, 2)

                    ElseIf ARB(j).FirstDate >= Today.AddDays(-30) Then   ' Current

                        Current = Current + Round(ARB(j).BALANCE, 2)

                    ElseIf ARB(j).FirstDate >= Today.AddDays(-60) Then  ' Plus 30

                        Plus30 = Plus30 + Round(ARB(j).BALANCE, 2)

                    ElseIf ARB(j).FirstDate >= Today.AddDays(-90) Then  ' Plus 60

                        Plus60 = Plus60 + Round(ARB(j).BALANCE, 2)

                    ElseIf ARB(j).FirstDate >= Today.AddDays(-120) Then ' Plus 90

                        Plus90 = Plus90 + Round(ARB(j).BALANCE, 2)

                    Else                                        ' Plus 120

                        Plus120 = Plus120 + Round(ARB(j).BALANCE, 2)

                    End If
                    TotalBalance = TotalBalance + Round(ARB(j).BALANCE, 2)

                End If

            Next j
            If Not TotalBalance = 0 Then

                SEQ = 1
                NumberOfStatements += 1
                SQL = "SELECT * FROM AR WHERE AcctNum = '" & ANum & "'"
                SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
                If Not SegmentSet = "" Then

                    ARSegment = GetNextSegmentFromSet(SegmentSet)
                    AccountBalance = 0
                    LastANum = ANum
                    AcctName = ExtractElementFromSegment("AcctName", ARSegment)
                    Email = ExtractElementFromSegment("SEmail", ARSegment)

                    MessageCT = Val(ExtractElementFromSegment("MessageCounter", ARSegment))
                    If Not MessageCT = 0 Then

                        StatementMessage = ExtractElementFromSegment("StatementMessage", ARSegment)
                        If Not MessageCT = -1 Then

                            buf = ""
                            buf = AddElementToSegment(buf, "ID", ExtractElementFromSegment("ID", ARSegment))
                            buf = AddElementToSegment(buf, "MessageCounter", MessageCT - 1)
                            SQL = MakeUpdateSQLFromSchema("AR", buf, gARTableSchema)
                            ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                        End If

                    End If
                    SendBlock = ExtractElementFromSegment("AcctName", ARSegment)
                    SendBlock = SendBlock & vbCrLf & ExtractElementFromSegment("Addr1", ARSegment)
                    buf = ExtractElementFromSegment("Addr2", ARSegment)
                    If Not buf = "" Then

                        SendBlock = SendBlock & vbCrLf & buf

                    End If
                    SendBlock = SendBlock & vbCrLf & ExtractElementFromSegment("City", ARSegment)
                    SendBlock = SendBlock & ", " & ExtractElementFromSegment("State", ARSegment)
                    SendBlock = SendBlock & "   " & ExtractElementFromSegment("ZipCode", ARSegment)
                    FinanceRate = Val(ExtractElementFromSegment("FinanceRate", ARSegment))

                Else

                    SQL = "SELECT TOP 1 * FROM Transactions WHERE AcctNum = '" & ANum & "'"
                    ARSegment = IO_GetSegmentSet(gShipriteDB, SQL)
                    AcctName = ExtractElementFromSegment("AcctName", ARSegment)
                    SendBlock = ExtractElementFromSegment("SoldToBlock", ARSegment)
                    i = InStr(1, SendBlock, vbCr)
                    buf = Mid(SendBlock, i)
                    SendBlock = buf
                    'MsgBox(SendBlock)

                End If
                Header = ""
                Header = AddElementToSegment(Header, "ID", 0)
                Header = AddElementToSegment(Header, "AccountNumber", ANum)
                Header = AddElementToSegment(Header, "AccountName", AcctName)
                Header = AddElementToSegment(Header, "TotalBalance", TotalBalance)
                Header = AddElementToSegment(Header, "Description", "Invoice")
                Header = AddElementToSegment(Header, "Current", Current)
                Header = AddElementToSegment(Header, "Plus30", Plus30)
                Header = AddElementToSegment(Header, "Plus60", Plus60)
                Header = AddElementToSegment(Header, "Plus90", Plus90)
                Header = AddElementToSegment(Header, "Plus120", Plus120)
                Header = AddElementToSegment(Header, "DueDate", DueDate)
                Header = AddElementToSegment(Header, "StatementDate", ExtractElementFromSegment("StatementDate", InputSegment))
                Header = AddElementToSegment(Header, "StatementID", StatementID)
                Header = AddElementToSegment(Header, "BatchID", BatchID)
                Header = AddElementToSegment(Header, "SendAddressBlock", SendBlock)
                Header = AddElementToSegment(Header, "FinanceChargeRate", FinanceRate)
                Header = AddElementToSegment(Header, "EmailAddress", Email)
                'Header = AddElementToSegment(Header, "SendEmail", True)
                StatementID += 1
                If Plus120 > 0 Then

                    StatementMessage = GetPolicyData(gShipriteDB, "MessagePlus120")

                ElseIf Plus90 > 0 Then

                    StatementMessage = GetPolicyData(gShipriteDB, "MessagePlus90")

                ElseIf Plus60 > 0 Then

                    StatementMessage = GetPolicyData(gShipriteDB, "MessagePlus60")

                ElseIf Plus30 > 0 Then

                    StatementMessage = GetPolicyData(gShipriteDB, "MessagePlus30")

                Else

                    StatementMessage = GetPolicyData(gShipriteDB, "MessageCurrent")

                End If
                Header = AddElementToSegment(Header, "Message", StatementMessage)
                TotalBalance = 0
                For j = 0 To ARBct - 1

                    If ARB(j).AcctNum = ANum And Not Round(ARB(j).Charges - ARB(j).Payments, 2) = 0 Then

                        If Not STotal = 0 Then

                            ARB(j).Charges = STotal
                            ARB(j).BALANCE = ARB(j).Charges - ARB(j).Payments
                            STotal = 0

                        End If
                        NewSegment = Header
                        SQL = "SELECT COUNT(ID) AS Tally FROM Transactions WHERE InvNum = '" & ARB(j).InvNum & "' AND SKU = 'INTEREST'"
                        buf = IO_GetSegmentSet(gShipriteDB, SQL)
                        If Not Val(ExtractElementFromSegment("Tally", buf)) = 0 Then

                            NewSegment = AddElementToSegment(NewSegment, "Description", "Finance Charge")

                        End If
                        NewSegment = AddElementToSegment(NewSegment, "InvNum", ARB(j).InvNum)
                        NewSegment = AddElementToSegment(NewSegment, "ID", StatementsID)
                        NewSegment = AddElementToSegment(NewSegment, "Charges", ARB(j).Charges)
                        NewSegment = AddElementToSegment(NewSegment, "Payments", ARB(j).Payments)
                        TotalBalance = TotalBalance + ARB(j).BALANCE
                        NewSegment = AddElementToSegment(NewSegment, "Balance", Round(TotalBalance, 2))
                        NewSegment = AddElementToSegment(NewSegment, "Date", ARB(j).FirstDate)
                        NewSegment = AddElementToSegment(NewSegment, "SEQ", SEQ)

                        Dim SSet As String = ""
                        Dim Test As String

                        SQL = "SELECT ID As T FROM Statements WHERE InvNum = '" & ARB(j).InvNum.ToString() & "' AND AccountNumber = '" & ANum & "'"
                        SSet = IO_GetSegmentSet(gReportWriter, SQL)
                        Test = ExtractElementFromSegment("T", SSet)

                        If Test = "" Then
                            SQL = MakeInsertSQLFromSchema("Statements", NewSegment, gStatementsSchema)
                            SEQ += 1
                        Else
                            SQL = ""
                        End If

                        'Dim file As System.IO.StreamWriter
                        'file = My.Computer.FileSystem.OpenTextFileWriter("c:\test.txt", False)
                        'file.WriteLine(NewSegment)
                        'file.WriteLine("")
                        'file.WriteLine(gStatementsSchema)
                        'file.WriteLine("")
                        'file.WriteLine(SQL)
                        'file.Close()

                        If Not SQL = "" Then

                            ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                            'We are using the Statements table in ReportWriter for generating statements.
                            ret = IO_UpdateSQLProcessor(gReportWriter, SQL)
                            StatementsID += 1

                        End If

                    End If

                Next j

            End If

SkipThisRecord:

        Next i
        ReDim ARRecordSet.RecordSet(0)
        Erase ARRecordSet.RecordSet
        ARRecordSet.RecordCount = 0
        ARSegment = ""
        Return NumberOfStatements

    End Function

    Private Sub SelectAll_CloseID_Btn_Click(sender As Object, e As RoutedEventArgs) Handles SelectAll_CloseID_Btn.Click
        ZReport_CloseID_LV.SelectAll()
    End Sub

    Private Sub Contact_Print_Click(sender As Object, e As RoutedEventArgs)
        Dim namefrom As String
        Dim nameto As String
        Dim cityfrom As String
        Dim cityto As String
        Dim statefrom As String
        Dim stateto As String
        Dim zipfrom As String
        Dim zipto As String
        Dim formula As String
        ' Retrieve input values
        namefrom = name_from.Text
        nameto = name_to.Text
        cityfrom = city_from.Text

        statefrom = state_from.Text

        zipfrom = zip_from.Text


        formula = ""

        If namefrom IsNot "" Then
            If formula.Length > 0 Then
                formula &= "And {Contacts.Name} >= '" & namefrom & "'"
            Else
                formula &= " {Contacts.Name} >= '" & namefrom & "'"
            End If
        End If


        If nameto IsNot "" Then

            If formula.Length > 0 Then
                formula &= "And  {Contacts.Name} <= '" & nameto & "'"
            Else
                formula &= "  {Contacts.Name} <= '" & nameto & "'"
            End If


        End If
        If cityfrom <> "" Then
            If formula.Length > 0 Then
                formula &= " AND {Contacts.City} LIKE '*" & cityfrom & "*'"
            Else
                formula &= " {Contacts.City} LIKE '*" & cityfrom & "*'"
            End If
        End If


        If statefrom <> "" Then
            If formula.Length > 0 Then
                formula &= " AND {Contacts.State} LIKE '*" & statefrom & "*'"
            Else
                formula &= " {Contacts.State} LIKE '*" & statefrom & "*'"
            End If
        End If

        If zipfrom <> "" Then
            If formula.Length > 0 Then
                formula &= " AND {Contacts.Zip} LIKE '*" & zipfrom & "*'"
            Else
                formula &= " {Contacts.Zip} LIKE '*" & zipfrom & "*'"
            End If
        End If


        Try
            Cursor = Cursors.Wait
            Dim report As New ShipRiteReports._ReportObject()
            report.ReportName = "Contacts.rpt"

            report.ReportFormula = formula

            Dim reportPrev As New ReportPreview(report)
            Cursor = Cursors.Arrow
            reportPrev.ShowDialog()

        Catch ex As Exception
            ' Display error message if reporting fails
            _MsgBox.ErrorMessage(ex, "Failed to report [Vault]...")
        Finally
            Cursor = Cursors.Arrow
        End Try
    End Sub

    Private Sub AR_EmailStatements_Btn_Click(sender As Object, e As RoutedEventArgs) Handles AR_EmailStatements_Btn.Click

        If AR_AllAccounts_RdBtn.IsChecked Then
            Display_Statements_AllAccounts()
        Else
            Email_Statement_OneAccount()
        End If

    End Sub

    Private Sub Email_Statement_OneAccount()
        Try

            Dim StatementCount As Integer

            StatementCount = ProcessStatements()

            If StatementCount > 0 Then
                Dim Email As String = ""
                Dim AcctName As String = ""
                Get_Statement_Email(AR_AcctNo_TxtBx.Text, AcctName, Email)

                If Email = "" Then
                    MsgBox("Account does not have a statement email setup." & vbCrLf & vbCrLf & "Please go into the account manager statement tab and add an email address to account!", vbExclamation, "Cannot Email Statement!")
                    Exit Sub
                End If

                SEND_StatementEmail(AcctName, AR_AcctNo_TxtBx.Text, Email, True)


            End If

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Emailing Statement.")
        End Try
    End Sub

    Private Sub Get_Statement_Email(AcctNum As String, ByRef AcctName As String, ByRef Email As String)
        Dim sql = "SELECT DISTINCT EmailAddress, SendAddressBlock FROM Statements WHERE AccountNumber='" & AcctNum & "'"
        Dim buf As String = IO_GetSegmentSet(gReportWriter, sql)

        Email = ExtractElementFromSegment("EmailAddress", buf, "")
        AcctName = GetAccountName_FromAddressBlock(ExtractElementFromSegment("SendAddressBlock", buf, ""))

    End Sub

    Private Function GetAccountName_FromAddressBlock(SendToAddressBlock As String) As String

        If SendToAddressBlock <> "" Then
            'Get only the first line (name) from the send to address
            Dim index = SendToAddressBlock.IndexOf(Environment.NewLine)
            If index > -1 Then
                Return SendToAddressBlock.Substring(0, index)
            End If
        End If

        Return ""

    End Function

    Private Sub Display_Statements_AllAccounts()
        Reports_TabControl.SelectedIndex = 6

        ProcessStatements()

        'Display statements in listview
        Dim sql = "SELECT DISTINCT AccountNumber, SendAddressBlock, EmailAddress, TotalBalance FROM Statements"
        Dim buf As String = IO_GetSegmentSet(gReportWriter, sql)
        Dim Segment As String
        Dim Statement As StatementItem
        Dim StatementList As List(Of StatementItem) = New List(Of StatementItem)

        Do Until buf = ""
            Segment = GetNextSegmentFromSet(buf)

            Statement = New StatementItem With {
                .AccountNumber = ExtractElementFromSegment("AccountNumber", Segment, ""),
            .EmailAddress = ExtractElementFromSegment("EmailAddress", Segment, ""),
            .SendAddressBlock = ExtractElementFromSegment("SendAddressBlock", Segment, ""),
            .TotalBalance = ExtractElementFromSegment("TotalBalance", Segment, "")}

            If Statement.EmailAddress = "" Then
                Statement.SendEmail = False
            Else
                Statement.SendEmail = True
            End If


            StatementList.Add(Statement)
        Loop

        StatementList = StatementList.OrderByDescending(Function(x As StatementItem) x.SendEmail).ToList
        Last_Column_Sorted = "SEND"
        Last_Sort_Ascending = False

        EmailStatements_LV.ItemsSource = StatementList
        EmailStatements_LV.Items.Refresh()
    End Sub

    Private Sub EmailStatementsCancel_Btn_Click(sender As Object, e As RoutedEventArgs) Handles EmailStatementsCancel_Btn.Click
        EmailStatements_LV.ItemsSource = Nothing
        EmailStatements_LV.Items.Refresh()

        Reports_TabControl.SelectedIndex = 1
    End Sub

    Private Sub EmailStatements_Btn_Click(sender As Object, e As RoutedEventArgs) Handles EmailStatements_Btn.Click
        Dim StatementList As List(Of StatementItem) = EmailStatements_LV.ItemsSource

        Mouse.OverrideCursor = Cursors.Wait

        For Each item As StatementItem In StatementList

            If item.SendEmail And item.EmailAddress <> "" Then

                If SEND_StatementEmail(GetAccountName_FromAddressBlock(item.SendAddressBlock), item.AccountNumber, item.EmailAddress, False) Then
                    item.EmailSent = "Sent"
                Else
                    item.EmailSent = "Fail"
                End If
                EmailStatements_LV.Items.Refresh()
            End If

        Next

        MsgBox("Emails Sent!", vbInformation)

        Mouse.OverrideCursor = Cursors.Arrow
    End Sub

    Private Function SEND_StatementEmail(AcctName As String, AcctNum As String, Email As String, ShowMessage As Boolean) As Boolean
        Try
            Dim InvoiceFilePath As String = gAppPath & "/Reports/Statement_" & AcctNum & ".pdf"
            Dim report As New _ReportObject()

            report.ReportName = "Statement.rpt"
            report.ReportSaveAsPath = InvoiceFilePath
            ShipRiteReports._LocalReport.Execute_ODBC_ToPDF(report)

            If _Files.IsFileExist(InvoiceFilePath, True) Then
                Dim template_Email As EmailTemplate = getEmailTemplate("Notify_Email-ARStatement", AcctName)


                Dim invoice_pdf As New System.Net.Mail.Attachment(InvoiceFilePath)

                Dim success As Boolean
                success = sendEmailWithAttachment(Email, template_Email.Subject, template_Email.Content, invoice_pdf, , ShowMessage)

                If success And ShowMessage Then
                    MsgBox("Email Sent Successfully!", vbInformation)
                End If

                Return success

            End If

            Return False

        Catch
            Return False
        End Try
    End Function

    Private Sub GridViewColumnHeader_Click(sender As Object, e As RoutedEventArgs)
        Dim ColumnHeader As GridViewColumnHeader = TryCast(sender, GridViewColumnHeader)
        Dim StatementList As List(Of StatementItem) = EmailStatements_LV.ItemsSource

        Select Case ColumnHeader.Content
            Case "SEND"
                If Last_Column_Sorted = ColumnHeader.Content And Last_Sort_Ascending Then
                    StatementList = StatementList.OrderByDescending(Function(x As StatementItem) x.SendEmail).ToList()
                    Last_Sort_Ascending = False
                Else
                    StatementList = StatementList.OrderBy(Function(x As StatementItem) x.SendEmail).ToList()
                    Last_Sort_Ascending = True
                End If

            Case "Account#"
                If Last_Column_Sorted = ColumnHeader.Content And Last_Sort_Ascending Then
                    StatementList = StatementList.OrderByDescending(Function(x As StatementItem) x.AccountNumber).ToList()
                    Last_Sort_Ascending = False
                Else
                    StatementList = StatementList.OrderBy(Function(x As StatementItem) x.AccountNumber).ToList()
                    Last_Sort_Ascending = True
                End If

            Case "Name"
                If Last_Column_Sorted = ColumnHeader.Content And Last_Sort_Ascending Then
                    StatementList = StatementList.OrderByDescending(Function(x As StatementItem) x.SendAddressBlock).ToList()
                    Last_Sort_Ascending = False
                Else
                    StatementList = StatementList.OrderBy(Function(x As StatementItem) x.SendAddressBlock).ToList()
                    Last_Sort_Ascending = True
                End If

            Case "Email"
                If Last_Column_Sorted = ColumnHeader.Content And Last_Sort_Ascending Then
                    StatementList = StatementList.OrderByDescending(Function(x As StatementItem) x.EmailAddress).ToList()
                    Last_Sort_Ascending = False
                Else
                    StatementList = StatementList.OrderBy(Function(x As StatementItem) x.EmailAddress).ToList()
                    Last_Sort_Ascending = True
                End If

            Case "Balance"
                If Last_Column_Sorted = ColumnHeader.Content And Last_Sort_Ascending Then
                    StatementList = StatementList.OrderByDescending(Function(x As StatementItem) x.TotalBalance).ToList()
                    Last_Sort_Ascending = False
                Else
                    StatementList = StatementList.OrderBy(Function(x As StatementItem) x.TotalBalance).ToList()
                    Last_Sort_Ascending = True
                End If

            Case "Sent"
                If Last_Column_Sorted = ColumnHeader.Content And Last_Sort_Ascending Then
                    StatementList = StatementList.OrderByDescending(Function(x As StatementItem) x.EmailSent).ToList()
                    Last_Sort_Ascending = False
                Else
                    StatementList = StatementList.OrderBy(Function(x As StatementItem) x.EmailSent).ToList()
                    Last_Sort_Ascending = True
                End If


        End Select

        Last_Column_Sorted = ColumnHeader.Content
        EmailStatements_LV.ItemsSource = StatementList
        EmailStatements_LV.Items.Refresh()
    End Sub


#End Region


End Class
