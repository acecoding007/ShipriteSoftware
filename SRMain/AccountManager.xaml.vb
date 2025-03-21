Imports System.Drawing.Printing
Public Class AccountManager
    Inherits CommonWindow


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
    Private Sub AccountManager_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Try
            Dim ret As Long
            Dim SQL As String = ""

            If (gIsProgramSecurityEnabled Or gIsPOSSecurityEnabled) AndAlso Not Check_Current_User_Permission("AR_CreateAccounts") Then
                Me.Close()
            End If

            For Each currentTab As TabItem In Account_TabControl.Items
                currentTab.Visibility = Visibility.Collapsed
            Next

            GetCountyTaxes()

            If Not gResult = "" Then

                D1.Text = gResult
                SQL = "SELECT * FROM AR WHERE AcctNum = '" & gResult & "'"
                gAccountSegment = IO_GetSegmentSet(gShipriteDB, SQL)
                ret = DisplayARSegment()

            End If

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Loading Account Manager")
        End Try

    End Sub

    Private Sub GetCountyTaxes()
        Try
            'Load Tax Counties into drop down
            Dim SQL As String
            Dim buf As String
            Dim Segment As String
            Dim CountyList As List(Of TaxCounty) = New List(Of TaxCounty)
            Dim county As TaxCounty

            SQL = "SELECT ID, State, County, TaxRate from CountyTaxes"
            buf = IO_GetSegmentSet(gShipriteDB, SQL)

            Do Until buf = ""
                Segment = GetNextSegmentFromSet(buf)
                county = New TaxCounty

                county.ID = ExtractElementFromSegment("ID", Segment, "")
                county.County = ExtractElementFromSegment("County", Segment, "")
                county.State = ExtractElementFromSegment("State", Segment, "")
                county.TaxRate = ExtractElementFromSegment("TaxRate", Segment, "")

                CountyList.Add(county)
            Loop

            D19.ItemsSource = CountyList

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Loading Tax Counties")
        End Try
    End Sub


    Function GetLedger(ANum As String) As Integer
        Try
            Dim AcctNum As String = D1.Text
            Dim SQL As String = "SELECT [Date], [Time], [Desc], InvNum, Charge, Payment, ID FROM Payments WHERE AcctNum = '" & ANum & "' ORDER BY [Date], ID"
            Dim Balance As Double = 0
            Dim amt As Double = 0
            Dim RowCT As Integer = 0
            Dim i As Integer = 0
            Dim Charge As Double = 0
            Dim Payment As Double = 0

            BindingOperations.ClearAllBindings(Ledger_ListView) ' clear binding on ListView
            Ledger_ListView.DataContext = Nothing ' remove any rows already in ListView

            Dim DT As New System.Data.DataTable ' datatable to use to populate ListView

            Dim currentGridView As GridView = Ledger_ListView.View ' variable to reference current GridView in Users_ListView to set up columns.

            ' add same column names to datatable columns
            DT.Columns.Add("Date", GetType(Date))
            DT.Columns.Add("Time")
            DT.Columns.Add("Desc")
            DT.Columns.Add("InvNum")
            DT.Columns.Add("Charge", GetType(Double))
            DT.Columns.Add("Payment", GetType(Double))
            DT.Columns.Add("ID", GetType(Integer))
            DT.Columns.Add("Balance", GetType(Double))

            ' return the # of rows added to ListView
            ' RowCT = IO_LoadListView(Ledger_ListView, DT, gShipriteDB, SQL, currentGridView.Columns.Count)
            RowCT = IO_LoadListView(Ledger_ListView, DT, gShipriteDB, SQL, DT.Columns.Count - 1)

            For i = 0 To RowCT - 1
                Charge = Val(DT.Rows.Item(i).Item(4))
                Payment = Val(DT.Rows.Item(i).Item(5))
                Balance = Balance + Charge - Payment
                DT.Rows.Item(i).Item(7) = Balance

            Next

            Ledger_ListView.SelectedIndex = Ledger_ListView.Items.Count - 1
            Ledger_ListView.ScrollIntoView(Ledger_ListView.SelectedItem)

            Return RowCT


        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Loading Ledger")
            Return 0
        End Try

    End Function
    Function GetHistory(ANum As String) As Integer
        Try
            Dim AcctNum As String = D1.Text
            Dim SQL As String = "SELECT [Date], InvNum, [Desc], UnitPrice, Disc, Qty, STax, LTotal FROM Transactions WHERE AcctNum = '" & ANum & "' ORDER BY [Date], ID"
            Dim Balance As Double = 0
            Dim amt As Double = 0
            Dim RowCT As Integer = 0
            Dim i As Integer = 0
            Dim Charge As Double = 0
            Dim Payment As Double = 0

            BindingOperations.ClearAllBindings(History_ListView) ' clear binding on ListView

            History_ListView.DataContext = Nothing ' remove any rows already in ListView

            Dim DT As New System.Data.DataTable ' datatable to use to populate ListView

            Dim currentGridView As GridView = History_ListView.View ' variable to reference current GridView in Users_ListView to set up columns.


            DT.Columns.Add("Date", GetType(Date))
            DT.Columns.Add("InvNum")
            DT.Columns.Add("Desc")
            DT.Columns.Add("UnitPrice", GetType(Double))
            DT.Columns.Add("Disc")
            DT.Columns.Add("Qty")
            DT.Columns.Add("STax", GetType(Double))
            DT.Columns.Add("LTotal", GetType(Double))


            ' return the # of rows added to ListView
            RowCT = IO_LoadListView(History_ListView, DT, gShipriteDB, SQL, currentGridView.Columns.Count)

            Dim view As CollectionView = CType(CollectionViewSource.GetDefaultView(History_ListView.ItemsSource), CollectionView)
            Dim groupDescription As PropertyGroupDescription = New PropertyGroupDescription("InvNum")
            view.GroupDescriptions.Add(groupDescription)
            Return RowCT


        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Loading Account History")
            Return 0
        End Try
    End Function

    Function GetLedgerByInvoice(ANum As String) As Integer
        Try
            Dim AcctNum As String = D1.Text
            ' Dim SQL As String = "SELECT FIRST([Date]) as [Date], InvNum, FIRST(SalesRep) As SalesRep, SUM(Charge) as InvAmt, SUM(Payment) as InvPayM, SUM(Charge - Payment) As Balance, FIRST([type]) AS AcctBalance FROM Payments WHERE AcctNum = '" & ANum & "' AND NOT [Type] = 'Change' GROUP BY InvNum"
            Dim SQL As String = "SELECT FIRST([Payments.Date]) as [Date], Payments.InvNum, FIRST(Payments.SalesRep) As SalesRep, SUM(Charge) As InvAmt, SUM(Payment) As InvPayM, SUM(Charge - Payment) As Balance, FIRST([type]) As AcctBalance, first(InvoiceNotes.Note) As [Note] 
FROM Payments LEFT JOIN InvoiceNotes On Payments.NumericInvoiceNumber = InvoiceNotes.InvNum 
WHERE (((Payments.[AcctNum]) ='" & ANum & "') AND ((Payments.[Type])<>'Change')) 
GROUP BY Payments.InvNum 
ORDER BY First([Payments.Date])"

            Dim Balance As Double = 0
            Dim InvBalance As Double = 0
            Dim amt As Double = 0
            Dim RowCT As Integer = 0
            Dim i As Integer = 0
            Dim Charge As Double = 0
            Dim Payment As Double = 0
            Dim InvNum As String = ""
            Dim ChangePaid As Double = 0
            Dim buf As String

            BindingOperations.ClearAllBindings(LedgerByInvoice_ListView) ' clear binding on ListView
            LedgerByInvoice_ListView.DataContext = Nothing ' remove any rows already in ListView

            'this.listView1.ColumnFormatStyle.HeaderTextAlign = ContentAlignment.MiddleCenter

            Dim DT As New System.Data.DataTable ' datatable to use to populate ListView

            Dim currentGridView As GridView = LedgerByInvoice_ListView.View ' variable to reference current GridView in Users_ListView to set up columns.

            ' set column bindings to same name as field name used in select SQL
            ' add same column names to datatable columns

            DT.Columns.Clear()
            DT.Columns.Add("Date", GetType(Date))
            DT.Columns.Add("InvNum")
            DT.Columns.Add("SalesRep")
            DT.Columns.Add("InvAmt", GetType(Double))
            DT.Columns.Add("InvPaid", GetType(Double))
            DT.Columns.Add("InvBal", GetType(Double))
            DT.Columns.Add("Balance", GetType(Double))
            DT.Columns.Add("Note")

            ' return the # of rows added to ListView
            RowCT = IO_LoadListView(LedgerByInvoice_ListView, DT, gShipriteDB, SQL, currentGridView.Columns.Count)
            For i = 0 To RowCT - 1

                InvNum = DT.Rows.Item(i).Item(1)
                SQL = "Select SUM(charge) As Change FROM Payments WHERE InvNum = '" & InvNum & "' AND [Type] = 'Change'"
                buf = IO_GetSegmentSet(gShipriteDB, SQL)
                ChangePaid = Val(ExtractElementFromSegment("Change", buf))

                Charge = Val(DT.Rows.Item(i).Item(3))
                Payment = Val(DT.Rows.Item(i).Item(4)) - ChangePaid

                DT.Rows.Item(i).Item(4) = Payment.ToString
                InvBalance = Charge - Payment
                DT.Rows.Item(i).Item(5) = InvBalance.ToString
                InvBalance = Charge - Payment
                Balance = Balance + InvBalance
                DT.Rows.Item(i).Item(6) = Balance.ToString

            Next

            LedgerByInvoice_ListView.SelectedIndex = LedgerByInvoice_ListView.Items.Count - 1
            LedgerByInvoice_ListView.ScrollIntoView(LedgerByInvoice_ListView.SelectedItem)

            Return RowCT


        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Loading Ledger By Invoice")
            Return 0
        End Try
    End Function

    Public Function RefreshUserList() As Integer
        Try


            Dim AcctNum As String = D1.Text
            Dim SQL As String = "SELECT ID, FName & ' ' & LName AS FullName, Name, Addr1, Phone FROM Contacts WHERE AR = '" & AcctNum & "'"

            BindingOperations.ClearAllBindings(Users_ListView) ' clear binding on ListView
            Users_ListView.DataContext = Nothing ' remove any rows already in ListView

            Dim DT As New System.Data.DataTable ' datatable to use to populate ListView

            Dim currentGridView As GridView = Users_ListView.View ' variable to reference current GridView in Users_ListView to set up columns.

            ' set column bindings to same name as field name used in select SQL
            ' add same column names to datatable columns

            DT.Columns.Add("ID")
            DT.Columns.Add("FullName")
            DT.Columns.Add("Name")
            DT.Columns.Add("Addr1")
            DT.Columns.Add("Phone")

            ' return the # of rows added to ListView
            Return IO_LoadListView(Users_ListView, DT, gShipriteDB, SQL, currentGridView.Columns.Count)


        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Loading Account Users.")
            Return 0
        End Try
    End Function


    Private Function DisplayARSegment() As Long
        Try

            Dim i As Integer
            Dim FName As String
            Dim buf As String
            Dim ret As Integer = 0

            Dim ANum As String = ""
            Dim Balance As Double = 0
            Dim Current As Double = 0
            Dim Plus30 As Double = 0
            Dim Plus60 As Double = 0
            Dim Plus90 As Double = 0
            Dim Plus120 As Double = 0

            FName = ""
            For i = 0 To 36

                Dim D As Object = Me.FindName("D" & i.ToString)
                FName = D.uid
                buf = ExtractElementFromSegment(FName, gAccountSegment)
                If Not buf = "" Then

                    If i = 21 Then
                        'Level Pricing ComboBox - expects "" or a number
                        Dim pLevel As String = CStr(buf.Last())
                        Dim pLevelInt As Integer = 0
                        If Not IsNumeric(pLevel) Then
                            pLevelInt = 0
                        Else
                            pLevelInt = CInt(pLevel) + 1
                            Dim pLevelCb As ComboBox = D
                            If pLevelInt < 0 OrElse pLevelInt > pLevelCb.Items.Count - 1 Then
                                pLevelInt = 0
                            End If
                        End If
                        D.SelectedIndex = pLevelInt

                    ElseIf i = 19 Then
                        'Tax County ComboBox
                        Dim Taxlist As List(Of TaxCounty) = D.ItemsSource
                        D.selectedindex = Taxlist.FindIndex(Function(x As TaxCounty) x.County = buf)


                    Else
                        D.text = buf
                    End If

                Else
                    D.text = D.tooltip
                End If

            Next


            ANum = D1.Text
            'TODO: change General -> StatementAndAccounting when that version of function ready
            ret = Account_Aging(gShipriteDB, ANum, Balance, Current, Plus30, Plus60, Plus90, Plus120)
            D13.Text = Format(Balance, "$ 0.00")
            D14.Text = Format(Current, "$ 0.00")
            D15.Text = Format(Plus30, "$ 0.00")
            D16.Text = Format(Plus60, "$ 0.00")
            D17.Text = Format(Plus90, "$ 0.00")
            D18.Text = Format(Plus120, "$ 0.00")

            SendStatements_CheckBox.IsChecked = ExtractElementFromSegment("SendStatement", gAccountSegment)
            FinanceCharges_CheckBox.IsChecked = ExtractElementFromSegment("FinanceCharges", gAccountSegment)
            AutoPay_CheckBox.IsChecked = ExtractElementFromSegment("EnableAutoPay", gAccountSegment)

            Return 0


        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error")
            Return 0
        End Try
    End Function

    Private Sub AccountOptions_ListBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles AccountOptions_ListBox.SelectionChanged
        Try
            Dim ret As Integer
            Select Case AccountOptions_ListBox.SelectedIndex

                Case 0

                Case 1

                    ret = RefreshUserList()

                Case 2

                    ret = GetLedger(D1.Text)

                Case 3

                    ret = GetLedgerByInvoice(D1.Text)

                Case 4

                    ret = GetHistory(D1.Text)

                Case 5

                Case 6

            End Select
            Account_TabControl.SelectedIndex = AccountOptions_ListBox.SelectedIndex


        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error ")
        End Try

    End Sub

    Private Sub Adjustments_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Adjustments_Btn.Click
        If Adjustments_Popup.IsOpen = True Then
            Adjustments_Popup.IsOpen = False
        Else
            If Ledger_ListView.SelectedIndex = -1 Then
                MsgBox("Please select an invoice first!", vbExclamation, "Invoice Adjustment")
                Exit Sub
            End If

            Adj_InvoiceNo_Lbl.Content = Ledger_ListView.SelectedItem(3)
            Adjustments_Popup.IsOpen = True

        End If
    End Sub

    Private Sub PostAdjustment_Btn_Click(sender As Object, e As RoutedEventArgs) Handles PostAdjustment_Btn.Click
        Try
            ' Check for issues with inputs
            Dim emptyFieldText = ""
            If Not IsNumeric(LedgerAdj_Amt.Text) Then
                emptyFieldText &= "- Adjustment amount must be a number." & vbCrLf & vbCrLf
            End If
            If IsNumeric(LedgerAdj_Amt.Text) AndAlso Double.Parse(LedgerAdj_Amt.Text) <= 0 Then
                emptyFieldText &= "- Adjusment amount must be greater than 0." & vbCrLf & vbCrLf
            End If

            If LedgerAdj_Date.SelectedDate Is Nothing Then
                emptyFieldText &= "- You must select a date for the adjustment." & vbCrLf & vbCrLf
            End If

            If String.Compare(LedgerAdj_Desc.Text, "") = 0 Then
                emptyFieldText &= "- Adjustment description cannot be empty." & vbCrLf & vbCrLf
            End If

            If LedgerAdj_BalInc.IsChecked = False And LedgerAdj_BalDec.IsChecked = False Then
                emptyFieldText &= "- You must select either Charge or Payment." & vbCrLf & vbCrLf
            End If

            ' Complain to the user about problems or make the adjustment
            If emptyFieldText.Length > 0 Then
                MsgBox(emptyFieldText, MsgBoxStyle.Exclamation, "There was a problem adjusting the ledger")
                Adjustments_Popup.IsOpen = True
            Else
                Post_Adjustment()
            End If

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Checking Adjustment")
        End Try

    End Sub

    Private Sub Post_Adjustment()
        Try

            Dim SQL As String
            Dim SegmentSet As String
            Dim ID As String
            Dim Segment As String = ""
            Dim ret As Double

            SQL = "SELECT MAX(ID) AS MaxID from Payments"
            SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
            ID = Val(ExtractElementFromSegment("MaxID", SegmentSet)) + 1
            Segment = AddElementToSegment(Segment, "DrawerID", gDrawerID)


            Segment = ""
            Segment = AddElementToSegment(Segment, "ID", ID.ToString())
            Segment = AddElementToSegment(Segment, "InvNum", Adj_InvoiceNo_Lbl.Content)
            Segment = AddElementToSegment(Segment, "NumericInvoiceNumber", Adj_InvoiceNo_Lbl.Content)
            Segment = AddElementToSegment(Segment, "AcctNum", D1.Text)
            Segment = AddElementToSegment(Segment, "AcctName", D0.Text)
            Segment = AddElementToSegment(Segment, "Date", Today.ToString("MM/dd/yyyy"))
            Segment = AddElementToSegment(Segment, "Time", Now.ToString("HH:mm:ss"))
            Segment = AddElementToSegment(Segment, "Desc", LedgerAdj_Desc.Text)

            If LedgerAdj_BalInc.IsChecked Then
                Segment = AddElementToSegment(Segment, "Charge", LedgerAdj_Amt.Text)
                Segment = AddElementToSegment(Segment, "Payment", "0")
            Else
                Segment = AddElementToSegment(Segment, "Charge", "0")
                Segment = AddElementToSegment(Segment, "Payment", LedgerAdj_Amt.Text)
            End If


            Segment = AddElementToSegment(Segment, "SalesRep", "ADMIN")
            Segment = AddElementToSegment(Segment, "Type", "ADJUST")
            Segment = AddElementToSegment(Segment, "Status", "Ok")

            SQL = MakeInsertSQLFromSchema("Payments", Segment, gdbSchema_Payments, True)

            ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

            If ret = 1 Then
                MsgBox("Adjustment applied successfully!", vbInformation)
                Update_Invoice_Balance(Adj_InvoiceNo_Lbl.Content)
                clearAdjustmentScreen()
                GetLedger(D1.Text)
            Else
                MsgBox("Adjustment could not be posted!", vbCritical)
            End If


        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Posting Adjustment")
        End Try
    End Sub

    Private Sub clearAdjustmentScreen()
        LedgerAdj_Amt.Text = ""
        LedgerAdj_Date.Text = ""
        LedgerAdj_Desc.Text = ""
        LedgerAdj_BalDec.IsChecked = True
        Adj_InvoiceNo_Lbl.Content = ""
    End Sub


    Private Sub D_KeyDown(sender As Object, e As KeyEventArgs) Handles D0.KeyDown, D1.KeyDown, D2.KeyDown, D3.KeyDown, D4.KeyDown, D10.KeyDown, D5.KeyDown, D11.KeyDown, D6.KeyDown, D7.KeyDown, D8.KeyDown, D9.KeyDown, D12.KeyDown, D13.KeyDown, D14.KeyDown, D15.KeyDown, D16.KeyDown, D17.KeyDown, D18.KeyDown, D19.KeyDown, D20.KeyDown, D21.KeyDown, D22.KeyDown, D23.KeyDown, D24.KeyDown, D25.KeyDown, D26.KeyDown
        Try
            Dim SQL As String
            Dim Segment As String
            Dim SegmentSet As String
            Dim buf As String
            Dim InputType As Integer
            Dim ret As Long
            Dim sel_start As Integer
            Dim sel_length As Integer

            sel_start = 0
            sel_length = 0
            ret = 0
            buf = ""
            InputType = 0
            Segment = ""
            SegmentSet = ""

            Select Case sender.name
                Case "D0"
                    If e.Key = Key.Return Then
                        buf = sender.Text
                        ' Replace CR, LF, and VT with nothing.
                        ' Why not use buf.Replace()????
                        buf = FlushOut(buf, Chr(13), "")
                        buf = FlushOut(buf, Chr(11), "")
                        buf = FlushOut(buf, Chr(10), "")
                        buf = Trim$(buf)
                        InputType = GetInputType(buf)

                        Select Case InputType
                            Case 0          ' Name
                                SQL = "SELECT AcctName, Addr1+chr(13)+City+', '+State+'  '+ZipCode AS FullAddress, Phone, AcctNum FROM AR WHERE AcctName LIKE '<<SEED>>%' ORDER BY AcctName"
                                buf = SearchList(Me, buf, "AR", "AcctName", "Account Search", SQL, buf)

                            Case 1          ' Phone Number
                                buf = ReformatPhone(gShipriteDB, buf)
                                SQL = "SELECT * FROM AR WHERE Phone = '<<SEED>>' ORDER By Name"
                                buf = SearchList(Me, buf, "ARPhones", "Phone", "Phone Search", SQL, buf)

                            Case 2          ' Address
                                SQL = "SELECT * FROM AR WHERE Addr1 LIKE '<<SEED>>%' ORDER BY Name"
                                buf = SearchList(Me, buf, "ARAddresses", "Addr1", "Address Search", SQL, buf)

                            Case Else       ' Unknown
                                SQL = "SELECT * FROM AR ORDER BY Name"
                                buf = SearchList(Me, buf, "AR", "AcctName", "Customer Search", SQL, "")
                        End Select

                        If Not buf = "" Then
                            SQL = "SELECT * FROM AR WHERE AcctNum = '" & buf & "'"
                            gAccountSegment = IO_GetSegmentSet(gShipriteDB, SQL)
                            ret = DisplayARSegment()
                        End If
                    End If

                Case "D1"
                    If e.Key = Key.Return Then
                        buf = sender.Text
                        If Not String.IsNullOrWhiteSpace(buf) Then
                            buf.Replace("\n", "")
                            buf.Trim()
                        End If
                        SQL = "SELECT * FROM AR WHERE AcctNum = '" & buf & "'"
                        gAccountSegment = IO_GetSegmentSet(gShipriteDB, SQL)
                        ret = DisplayARSegment()
                    End If
            End Select

            sel_start = sender.SelectionStart
            sel_length = sender.SelectionLength

            Select Case sender.name
                Case "D0", "D1", "D2", "D4", "D5", "D6"
                    sender.Text = StrConv(sender.Text, VbStrConv.ProperCase)

                Case "D3", "D7", "D8"
                    sender.Text = StrConv(sender.Text, VbStrConv.Uppercase)
            End Select

            sender.SelectionStart = sel_start
            sender.SelectionLength = sel_length


        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Search Error.")
        End Try

    End Sub


    Private Sub ClearAccount_Click(sender As Object, e As MouseButtonEventArgs) Handles ClearButton.Click
        Try
            Dim i As Integer
            For i = 0 To 36

                Dim D As Object = Me.FindName("D" & i.ToString)
                D.text = D.tooltip

            Next
            IDNumber.Content = "00000"


        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Clearing Screen")
        End Try

    End Sub



    Private Sub SaveButton_Click(sender As Object, e As RoutedEventArgs) Handles SaveButton.Click
        Try
            Dim UpdateOnly As Boolean
            Dim SQL As String
            Dim Segment As String
            Dim i As Integer
            Dim FName As String
            Dim ID As Long
            Dim ret As Long

            ID = CLng(IDNumber.Content)

            If vbNo = MsgBox("Are you sure you want to save changes to Account?", vbYesNo + vbQuestion) Then
                Exit Sub
            End If

            If ID = 0 Then

                UpdateOnly = False
                SQL = "SELECT MAX(ID) AS MaxID FROM AR"
                Segment = IO_GetSegmentSet(gShipriteDB, SQL)
                ID = CLng(ExtractElementFromSegment("MaxID", Segment))

            Else

                ID = CLng(IDNumber.Content)

            End If
            SQL = ""
            Segment = ""
            Segment = AddElementToSegment(Segment, "ID", ID.ToString)   'This makes sure ID will be first
            UpdateOnly = False
            For i = 0 To 36

                Dim D As Object = Me.FindName("D" & i.ToString)
                FName = D.uid


                Select Case D.Name

                    Case "D13", "D14", "D15", "D16", "D17", "D18"

                        Segment = AddElementToSegment(Segment, FName, ValFix(D.text))

                    Case "D19"

                        If Not IsNothing(D.selecteditem) Then
                            Segment = AddElementToSegment(Segment, FName, D.SelectedItem.County)
                        End If


                    Case Else

                        Segment = AddElementToSegment(Segment, FName, D.text)

                End Select

            Next

            Segment = AddElementToSegment(Segment, "SendStatement", SendStatements_CheckBox.IsChecked)
            Segment = AddElementToSegment(Segment, "FinanceCharges", FinanceCharges_CheckBox.IsChecked)
            Segment = AddElementToSegment(Segment, "EnableAutoPay", AutoPay_CheckBox.IsChecked)


            SQL = MakeUpdateSQLFromSchema("AR", Segment, gARTableSchema,, True)
            ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
            If ret > -1 Then
                MsgBox("Changes to Account saved successfully!", vbInformation)
            End If


            If gResult2 = "AUTOEXIT" Then

                Me.Close()

            End If


        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Saving Account Changes")
        End Try

    End Sub
    Private Sub AddUser_Click(sender As Object, e As RoutedEventArgs) Handles AddUser_Btn.Click
        Try
            Dim SQL As String = ""
            Dim ret As Integer = 0
            Dim ID As Long = 0
            Dim AcctNum As String = ""
            AcctNum = D1.Text
            If AcctNum = "" Then

                Exit Sub

            End If
            gAutoExitFromContacts = True
            Dim win As New ContactManager(Me)
            win.ShowDialog(Me)

            If Not gContactManagerSegment = "" Then

                ID = Val(ExtractElementFromSegment("ID", gContactManagerSegment))
                SQL = "UPDATE Contacts SET AR = '" & AcctNum & "' WHERE ID = " & ID
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                If ret >= 0 Then MsgBox("User " & ExtractElementFromSegment("Name", gContactManagerSegment) & " added to account!", vbInformation)
                RefreshUserList()
            End If


        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Adding User")
        End Try
    End Sub

    Private Sub DeleteUser_Btn_Click(sender As Object, e As RoutedEventArgs) Handles DeleteUser_Btn.Click

        Dim ID As Long = 0
        Dim SQL As String = ""
        Dim ret As Long = 0

        Try

            If Users_ListView.SelectedIndex = -1 Then
                MsgBox("ATTENTION...Delete User" & vbCrLf & vbCrLf & "No User Selected. Please select a user first.", vbCritical)
                Exit Sub
            End If

            If vbNo = MsgBox("Are you sure you want to remove the user from the AR account?", vbYesNo + vbQuestion) Then
                Exit Sub
            End If

            ID = Val(Users_ListView.SelectedItem(0).ToString)
            SQL = "UPDATE Contacts SET AR = '' WHERE ID = " & ID.ToString
            ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
            If ret >= 0 Then MsgBox("User removed from account!", vbInformation)
            ret = RefreshUserList()

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Deleting User")
        End Try

    End Sub

    Private Sub EditUser_Btn_Click(sender As Object, e As RoutedEventArgs) Handles EditUser_Btn.Click

        Dim ID As Long = 0
        Dim SQL As String = ""
        Dim Segment As String = ""
        Dim SegmentSet As String = ""

        Try

            If Users_ListView.SelectedIndex = -1 Then Exit Sub

            ID = Val(Users_ListView.SelectedItem(0).ToString)
            gAutoExitFromContacts = True
            Dim win As New ContactManager(Me, ID)
            win.ShowDialog(Me)

            RefreshUserList()

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Editing User")
        End Try

    End Sub

    Private Sub LedgerByInvoice_ListView_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles LedgerByInvoice_ListView.MouseDoubleClick
        Try

            If LedgerByInvoice_ListView.SelectedIndex <> -1 Then Open_Invoice_In_POS(LedgerByInvoice_ListView.SelectedItem(1).ToString)

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error")
        End Try
    End Sub

    Private Sub Ledger_ListView_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles Ledger_ListView.MouseDoubleClick
        Try

            If Ledger_ListView.SelectedIndex <> -1 Then Open_Invoice_In_POS(Ledger_ListView.SelectedItem(3).ToString)

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Adding User")
        End Try
    End Sub

    Private Sub Open_Invoice_In_POS(ByRef InvNum As String)
        Try

            If CommonWindowStack.windowList.FindIndex(Function(x As CommonWindow) x.Name = "POS_Window") = -1 Then
                'POS Not Open
                gResult3 = "InvNum:" & InvNum

                 

                Dim win As New POSManager(Me)
                win.ShowDialog(Me)

                gResult3 = ""
                'After POS is opened the first time, it stays in the window list after being closed. Need to remove it from the list so that it can be opened again.
                CommonWindowStack.windowList(CommonWindowStack.windowList.FindIndex(Function(x As CommonWindow) x.Name = "POS_Window")).Close()

            Else
                'POS already open
                gResult3 = InvNum
                Me.Close()
            End If

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Opening Invoice")
        End Try

    End Sub

    Private Sub DeleteLine_Btn_Click(sender As Object, e As RoutedEventArgs) Handles DeleteLine_Btn.Click
        Try

            Dim desc = Ledger_ListView.SelectedItem.Item("Desc")

            ' Can't delete sales, do a return instead
            If String.Compare(desc, "Sales") = 0 Then
                MsgBox("You cannot delete sale lines. Perform a return instead.", MsgBoxStyle.OkOnly, "Attempting to Delete Sale")
            Else
                Dim cont = MsgBox("Your are about to delete a line in the ledger." & vbCrLf & "Would you like to continue?", MsgBoxStyle.YesNo, "Delete Line")
                If cont = MsgBoxResult.Yes Then
                    Dim id = Ledger_ListView.SelectedItem.Item("ID")
                    Dim SQL = "DELETE * FROM Payments WHERE ID = " & id
                    IO_UpdateSQLProcessor(gShipriteDB, SQL)
                    UpdateInvoiceBalance(Ledger_ListView.SelectedItem.Item("InvNum"))
                    Ledger_ListView.DataContext.Rows.RemoveAt(Ledger_ListView.SelectedIndex)
                    GetLedger(D1.Text)
                End If
            End If

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Deleting Payment")
        End Try
    End Sub

    Private Sub UpdateInvoiceBalance(ByVal InvNum As String)
        Try
            Dim SegmentSet As String
            Dim Balance As Double
            Dim SQL = "SELECT SUM(IIf([Type] = 'Sale', Charge, 0)) AS InvAmt,
        SUM(Payment) - sum(IIF( [Type] = 'Change', Charge, 0)) AS InvPayment,
        (InvAmt-InvPayment) AS Balance
        FROM Payments
        WHERE  Status='Ok' AND InvNum = '" & InvNum & "'"

            SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
            Balance = ExtractElementFromSegment("Balance", SegmentSet, "0")

            IO_UpdateSQLProcessor(gShipriteDB, "Update Payments set Balance=" & Balance & " WHERE InvNum='" & InvNum & "'")

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Updating Invoice Balance")
        End Try

    End Sub

    Private Sub AddLastCCtoVault_Click(sender As Object, e As RoutedEventArgs) Handles AddLastCCtoVault.Click

        Dim ans As Integer
        Dim SQL As String
        Dim Segment As String
        Dim SegmentSet As String
        Dim InvNum As String
        Dim TransactionID As String
        Dim CardType As String
        Dim Last4 As String
        Dim ExpirationDate As String
        Dim Token As String
        Dim iloc As Integer
        Dim Desc As String
        Dim ID As String
        Dim ret As Long

        If D1.Text = "" Then

            Exit Sub

        End If
        ans = MsgBox("ATTENTION...Sending the last CC used to the Genius Vault." & vbCrLf & vbCrLf & "CONTINUE???", vbQuestion + vbYesNo, gProgramName)
        If ans = vbNo Then

            Exit Sub

        End If
        'SQL = "SELECT ID,InvNum, [Desc] FROM Payments WHERE AcctNum = '" & D1.Text & "' AND [Type] = 'Note' AND [Desc] LIKE '%/%' ORDER BY ID Desc"
        SQL = "SELECT ID,InvNum, [ApprovalNum] FROM Payments WHERE AcctNum = '" & D1.Text & "' AND [Type] = 'CHARGE' AND [ApprovalNum] LIKE '%/%' ORDER BY ID Desc"
        SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
        Segment = GetNextSegmentFromSet(SegmentSet)
        ID = ExtractElementFromSegment("ID", Segment)
        InvNum = ExtractElementFromSegment("InvNum", Segment)
        Desc = ExtractElementFromSegment("ApprovalNum", Segment)
        iloc = InStr(1, Desc, "/")
        If iloc = 0 Then

            MsgBox("ATTENTION...Unable to get Vault Token.  No CC payment found.", vbCritical, gProgramName)

            Exit Sub

        End If
        TransactionID = Mid(Desc, iloc)
        SQL = "SELECT CCNum, ExpDate, BankNum FROM Payments WHERE ID = " & ID
        SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
        CardType = ExtractElementFromSegment("BankNum", SegmentSet)
        ExpirationDate = ExtractElementFromSegment("ExpDate", SegmentSet)
        Last4 = ExtractElementFromSegment("CCNum", SegmentSet)
        Last4 = FlushOut(Last4, "*", "")
        gCredentialSegment = GENIUS_GenerateCredentialSegment()

        Segment = GENIUS_Vault_BoardPreviousCard(TransactionID)
        If Segment = "" Then

            Exit Sub

        End If
        Token = Segment
        SQL = "UPDATE AR SET VaultCreditCard = '" & Last4 & "', VaultExpirationDt = '" & ExpirationDate & "', VaultReferenceID = '" & TransactionID & "', VaultToken = '" & Token & "', VaultCardType = '" & CardType & "' WHERE AcctNum = '" & D1.Text & "'"
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
        If ret = 1 And Not Token = "" Then

            D26.Text = TransactionID
            D24.Text = Last4
            D25.Text = ExpirationDate
            MsgBox("ATTENTION...Vaulting SUCCESS" & vbCrLf & vbCrLf & "You will see a new payment option for this customer" & vbCrLf & "on the Payments Form.", vbInformation)

        End If

    End Sub

    Private Sub LedgerByInvoice_MNU_Selected(sender As Object, e As RoutedEventArgs) Handles LedgerByInvoice_MNU.Selected

    End Sub

    Private Sub Statements_Button_Click(sender As Object, e As RoutedEventArgs) Handles Statements_Button.Click

        Try
            Dim win As New ReportsManager(Me, 1)
            win.ShowDialog(Me)

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error")
        End Try

    End Sub

    Private Sub RemoveCardFromVault_Click(sender As Object, e As RoutedEventArgs) Handles RemoveCardFromVault.Click


        '###############################################    Returns BLANK if successful, Error message if not
        '
        '###############################################    Uses VAULT Token, NOT sale token

        Dim SQL As String
        Dim Segment As String
        Dim Ret As Long
        Dim ans As Integer
        Dim VaultToken As String

        If D1.Text = "" Then

            Exit Sub

        End If
        ans = MsgBox("ATTENTION...Removing Token from Genius Vault." & vbCrLf & vbCrLf & "CONTINUE???", vbQuestion + vbYesNo, gProgramName)
        If ans = vbNo Then

            Exit Sub

        End If
        SQL = "SELECT VaultToken FROM AR WHERE AcctNum = '" & D1.Text & "'"
        Segment = IO_GetSegmentSet(gShipriteDB, SQL)
        VaultToken = ExtractElementFromSegment("VaultToken", Segment)

        gCredentialSegment = GENIUS_GenerateCredentialSegment()
        Segment = GENIUS_Vault_RemoveToken(VaultToken)
        If Segment = "" Then

            Exit Sub

        End If
        SQL = "UPDATE AR SET VaultCreditCard = '', VaultExpirationDt = '', VaultReferenceID = '', VaultToken = '', VaultCardType = '' WHERE AcctNum = '" & D1.Text & "'"
        Ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
        If Ret = 1 Then

            D26.Text = ""
            D24.Text = ""
            D25.Text = ""
            MsgBox("ATTENTION...UnVaulting Token SUCCESS" & vbCrLf & vbCrLf & "OPERATION COMPLETE", vbInformation)

        End If

    End Sub

    Private Sub AddToVaultManual_Click(sender As Object, e As RoutedEventArgs) Handles AddToVaultManual.Click

        Dim SQL As String
        Dim Segment As String
        Dim Token As String
        Dim ret As Long

        If D24.Text = "" Or D25.Text = "" Or D26.Text = "" Then

            MsgBox("ATTENTION...ReferenceID, Last 4 of CC, and Expiration Date are Required." & vbCrLf & vbCrLf & "FILL IN REQUIRED INFORMATION AND TRY AGAIN.", vbCritical, "Shiprite Next")
            Exit Sub

        End If
        gCredentialSegment = GENIUS_GenerateCredentialSegment()

        Segment = GENIUS_Vault_BoardPreviousCard(D26.Text)
        If Segment = "" Then

            Exit Sub

        End If
        Token = Segment
        SQL = "UPDATE AR SET VaultCreditCard = '" & D24.Text & "', VaultExpirationDt = '" & D25.Text & "', VaultReferenceID = '" & D26.Text & "', VaultToken = '" & Token & "', VaultCardType = '" & "MANUAL ENTRY" & "' WHERE AcctNum = '" & D1.Text & "'"
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
        If ret = 1 And Not Token = "" Then

            MsgBox("ATTENTION...Vaulting SUCCESS" & vbCrLf & vbCrLf & "You will see a new payment option for this customer" & vbCrLf & "on the Payments Form.", vbInformation)

        End If

    End Sub

    Private Sub PrintInvoice_Btn_Click(sender As Object, e As RoutedEventArgs) Handles PrintInvoice_Btn.Click
        If Ledger_ListView.SelectedIndex < 0 Then Exit Sub

        Dim invnum = Ledger_ListView.SelectedItem(3)

        POSManager.Print_FullSheetInvoice(invnum)
    End Sub

    Private Sub LBI_PrintInvoice_Btn_Click(sender As Object, e As RoutedEventArgs) Handles LBI_PrintInvoice_Btn.Click
        If LedgerByInvoice_ListView.SelectedIndex < 0 Then Exit Sub

        Dim invnum = LedgerByInvoice_ListView.SelectedItem(1)

        POSManager.Print_FullSheetInvoice(invnum)
    End Sub
End Class

