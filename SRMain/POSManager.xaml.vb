Imports System.Text.RegularExpressions
Imports System.Drawing
Imports System.Drawing.Printing
Public Class POSManager
    Inherits CommonWindow

    Dim MySelectedLine As Integer = 0

    Dim FirstTime As Boolean
    Public Shared DefaultLineCT As Integer = 38
    Dim RecoveredInvoiceNumber As Long
    Dim ReceiptOptionsSegment As String
    Dim isCOGSview As Boolean
    Dim Last_Column_Sorted As String
    Dim Last_Sort_Ascending As Boolean
    Public Property RecoveredInvoicePayments As List(Of PaymentDefinition)

    Public Shared InvoiceHistoryList As List(Of InvoiceHistoryItem)
    Public Refund_Line_List As List(Of Refund_LineItem)


#Region "Page Management"
    Public Sub New()

        MyBase.New()

        ' This call is required by the designer.
        InitializeComponent()

    End Sub

    Public Sub New(ByVal callingWindow As Window)

        MyBase.New(callingWindow)

        ' This call is required by the designer.
        FirstTime = True
        InitializeComponent()

    End Sub

    Private Sub ExitPOS(sender As Object, e As RoutedEventArgs)
        If ContactDropDown_Popup.IsOpen Then
            ContactDropDown_Popup.IsOpen = False
        End If

        CloseLineEditPopup()
        gCustomerSegment = ""

        Select Case sender.Name
            Case "HomeButton"
                HomeButton_Click(sender, e)
            Case "BackButton"
                BackButton_Click(sender, e)
            Case "CloseButton"
                CloseButton_Click(sender, e)
            Case Else
                Debug.Print(sender.Name)
                Debug.Print("Break Here")
        End Select
    End Sub

    Private Sub HandleExit(sender As Object, e As RoutedEventArgs)

        '' TODO: replace with RefundLineList
        If Receipt_LB.Items.Count > 0 And InvoiceType.Content <> "Recovered Invoice" Then
            If _MsgBox.QuestionMessage("You have a pending invoice, leaving will clear it. Are you sure you want to leave?", "Pending Invoice") Then
                ' Clear invoice
                ClearPOS()
                ' Go Home
                ExitPOS(sender, e)
            End If
        Else
            ClearPOS()
            ExitPOS(sender, e)
        End If
    End Sub

    Private Overloads Sub RefreshButton_Click(sender As Object, e As RoutedEventArgs)

        If Receipt_LB.Items.Count > 0 And InvoiceType.Content <> "Recovered Invoice" Then
            If Not _MsgBox.QuestionMessage("You have a pending invoice, are you sure you want to clear it?", "Pending Invoice") Then
                Exit Sub
            End If
        End If

        Dim ret As Integer
        ret = NewSale()

    End Sub

    Private Sub POSManager_Activated(sender As Object, e As EventArgs) Handles Me.Activated
        If gIsCustomerDisplayEnabled Then
            gCustomerDisplay.ChangeTab(1)
        End If
    End Sub

    Private Sub POS_Window_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded

        Dim SQL As String
        Dim Segment As String
        Dim SegmentSet As String
        Dim buf As String

        SaleOptions_Btn.Visibility = Visibility.Hidden
        TrackPackage_Btn.Visibility = Visibility.Hidden
        'FirstTime = True
        Segment = ""
        SegmentSet = ""
        buf = ""
        Dim ret As Integer

        ' This fixes POSManager opening in the background
        Application.Current.MainWindow.Activate()

        StoreName_Lbl.Content = GetPolicyData(gShipriteDB, "Name", "")

        Try

            'CHECK IF DRAWER NEEDs to be Opened
            buf = IO_GetSegmentSet(gShipriteDB, "SELECT ID FROM OpenClose WHERE [DrawerID] = '" & gDrawerID & "' AND [DrawerIsOpen] = True")
            If buf = "" Then

                gResult = "Open"
                '
                Dim win As New POS_OpenClose(Me)
                win.ShowDialog()
                If Not gResult = "Success" Then
                    Me.Close()
                    Exit Sub
                End If

            End If


            If FirstTime Then
                SQL = "SELECT [SKU] FROM POSButtons WHERE Type='Group'"
                SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
                Do Until SegmentSet = ""

                    Segment = GetNextSegmentFromSet(SegmentSet)
                    buf = ExtractElementFromSegment("SKU", Segment)
                    If Not buf = "" Then

                        CurrentGroup.Items.Add(buf)

                    End If

                Loop
                CurrentGroup.Text = "MAIN"
                ret = LoadPosButtons()

                FirstTime = False
            End If

            ret = NewSale()

            'Opening Recovered Invoice From AR screen.
            If Not IsNothing(gResult3) AndAlso gResult3.Contains("InvNum") Then
                RecoverInvoice(gResult3.Substring(7))
            End If

            '
            InvoiceHistoryList = New List(Of InvoiceHistoryItem)
            InvoiceHistoryView.ItemsSource = InvoiceHistoryList


            ReceiptOptionsSegment = GetReceiptOptions()

            If gIsCustomerDisplayEnabled Then
                gCustomerDisplay.ChangeTab(1)
            End If

            gResult = ""

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Changing Quantity.")
        End Try
    End Sub

#End Region

#Region "General POS Functions"

    Private Function ChangeQuantity() As Integer
        Try
            If DInput.Text = "" Then

                OverrideQuantity.Content = ""
                DInput.Focus()
                Return 0
                Exit Function

            End If
            If Not IsNumeric(DInput.Text) Then

                MsgBox("WARNING...Input is Not Numeric")
                DInput.Focus()
                Return 0
                Exit Function

            End If
            OverrideQuantity.Content = "Qty: " & DInput.Text
            DInput.Text = ""
            DInput.Focus()


            Return 0


        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Changing Quantity.")
        End Try

    End Function

    Private Function RecoverInvoice(InvNum As String) As Integer
        Try
            Dim SQL As String = ""
            Dim Segment As String = ""
            Dim SegmentSet As String = ""
            Dim ret As Long = 0
            Dim buf As String = ""
            Dim InvBalance As Double = 0.0
            Dim PaymentItem As PaymentDefinition

            SQL = "SELECT * FROM Transactions WHERE InvNum = '" & InvNum & "'"
            SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
            If SegmentSet = "" Then

                MsgBox("ATTENTION...Invoice [" & InvNum & "] Transactions information NOT FOUND" & vbCrLf & vbCrLf & "TRY AGAIN", vbCritical)
                Return 1
                Exit Function

            End If

            POSLines = New ObjectModel.ObservableCollection(Of POSLine) ' reset
            Receipt_LB.ItemsSource = POSLines


            gInvoiceNumber = InvNum
            buf = ExtractElementFromSegment("SoldTo", SegmentSet)
            If Val(buf) > 0 Then
                SQL = "SELECT * FROM Contacts WHERE ID = " & buf
                gCustomerSegment = IO_GetSegmentSet(gShipriteDB, SQL)
                If gCustomerSegment <> "" Then
                    LoadInForm_Contact(gCustomerSegment)
                End If
            End If
            buf = ExtractElementFromSegment("Date", SegmentSet)
            ResetReceiptHeader(buf)
            gReceiptCCEndBlock = ""
            Do Until SegmentSet = ""

                Segment = GetNextSegmentFromSet(SegmentSet)
                Dim LineItem As New POSLine()
                LineItem.ID = ExtractElementFromSegment("ID", Segment)                       ' New POS Line 0 = new sale, non 0 is recovered invoice do update
                LineItem.SKU = ExtractElementFromSegment("SKU", Segment)
                LineItem.ModelNumber = ExtractElementFromSegment("ModelNumber", Segment)
                LineItem.Description = ExtractElementFromSegment("Desc", Segment)
                LineItem.UnitPrice = Val(ExtractElementFromSegment("UnitPrice", Segment))
                LineItem.Quantity = Val(ExtractElementFromSegment("Qty", Segment))
                LineItem.ExtPrice = Val(ExtractElementFromSegment("ExtPrice", Segment))
                LineItem.STax = Val(ExtractElementFromSegment("STax", Segment))
                LineItem.LTotal = Val(ExtractElementFromSegment("LTotal", Segment))
                LineItem.TaxCounty = ExtractElementFromSegment("TCounty", Segment)
                LineItem.TRate = Val(ExtractElementFromSegment("TRate", Segment))
                LineItem.BrandName = ExtractElementFromSegment("Brand", Segment)
                LineItem.Category = ExtractElementFromSegment("Category", Segment)
                LineItem.AcctName = ExtractElementFromSegment("AcctName", Segment)
                LineItem.AcctNum = ExtractElementFromSegment("AcctNum", Segment)
                LineItem.SoldToID = Val(ExtractElementFromSegment("SoldTo", Segment))
                LineItem.ShipToID = Val(ExtractElementFromSegment("ShipTo", Segment))
                LineItem.COGS = Val(ExtractElementFromSegment("COGS", Segment))
                LineItem.UnitCost = Val(ExtractElementFromSegment("UnitCost", Segment, "0"))
                LineItem.PackageID = ExtractElementFromSegment("PackageID", Segment)
                POSLines.Add(LineItem)


            Loop
            RecoveredInvoiceNumber = Val(InvNum)
            gCustomerSegment = AddElementToSegment(gCustomerSegment, "RecoveredInvoiceNumber", InvNum)
            ret = CalculateInvoice()

            gReceiptCCEndBlock = ""

            SQL = "SELECT [Date], [Desc], [Type], Charge, Payment, CCEndBlock, SalesRep FROM Payments WHERE InvNum = '" & InvNum & "' AND NOT [Type] = 'Sale'"
            SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)

            Clerk_Lbl.Content = ExtractElementFromSegment("SalesRep", SegmentSet, "")

            RecoveredInvoicePayments = New List(Of PaymentDefinition)

            Do Until SegmentSet = ""
                Segment = GetNextSegmentFromSet(SegmentSet)
                PaymentItem = New PaymentDefinition

                PaymentItem.PostDate = ExtractElementFromSegment("Date", Segment)
                PaymentItem.Desc = ExtractElementFromSegment("Desc", Segment)
                PaymentItem.Type = ExtractElementFromSegment("Type", Segment)
                PaymentItem.Charge = Val(ExtractElementFromSegment("Charge", Segment))
                PaymentItem.Payment = Val(ExtractElementFromSegment("Payment", Segment))
                PaymentItem.RecoveredPayment = True

                'gPM.NewPayments.Add(PaymentItem)
                RecoveredInvoicePayments.Add(PaymentItem)
                buf = ExtractElementFromSegment("CCEndBlock", Segment)
                If Not buf = "" Then

                    gReceiptCCEndBlock = buf

                End If

            Loop

            InvoiceType.Content = "Recovered Invoice"

            SQL = "SELECT SUM(Charge) AS Charged, SUM(Payment) AS Paid FROM Payments WHERE InvNum = '" & RecoveredInvoiceNumber & "'"
            Segment = IO_GetSegmentSet(gShipriteDB, SQL)
            InvBalance = Val(ExtractElementFromSegment("Charged", Segment)) - Val(ExtractElementFromSegment("Paid", Segment))
            InvoiceBalance.Content = "Invoice Balance: " & Format(InvBalance, "$ 0.00")
            SaleOptions_Btn.Visibility = Visibility.Visible

            Receipt_LB.Items.Refresh()

            Return 0

        Catch ex As Exception

            _MsgBox.ErrorMessage(ex, "Error Recovering Invoice.")

        End Try
        Return 0

    End Function

    Private Function PostGiftCard(Amount As Double, GIDNumber As String) As Integer
        Try
            Dim SQL As String = ""
            Dim Segment As String = ""
            Dim ID As Long = 0
            Dim CID As Long = 0
            Dim SoldToBlock As String = ""
            Dim IsUpdate As Boolean = False
            Dim ret As Long = 0

            SQL = "SELECT ID FROM GiftRegistry WHERE GiftIDNumber = '" & GIDNumber & "'"
            Segment = IO_GetSegmentSet(gShipriteDB, SQL)
            If Not Segment = "" Then

                ID = Val(ExtractElementFromSegment("ID", Segment))
                IsUpdate = True

            End If

            If IsUpdate = False Then

                SQL = "SELECT MAX(ID) AS MaxID FROM GiftRegistry"
                Segment = IO_GetSegmentSet(gShipriteDB, SQL)
                ID = Val(ExtractElementFromSegment("MaxID", Segment))
                If ID = 0 Then

                    ID = 1000

                Else

                    ID = ID + 1

                End If

            End If
            CID = Val(ExtractElementFromSegment("ID", gCustomerSegment))
            If Not CID = 0 Then

                SoldToBlock = ExtractElementFromSegment("Name", gCustomerSegment) & vbCrLf & ExtractElementFromSegment("Addr1", gCustomerSegment) & vbCrLf & ExtractElementFromSegment("City", gCustomerSegment) & ", " & ExtractElementFromSegment("State", gCustomerSegment) & " " & ExtractElementFromSegment("Zip", gCustomerSegment) & vbCrLf & ExtractElementFromSegment("Phone", gCustomerSegment)

            Else

                SoldToBlock = "Cash, Check, Charge"

            End If
            If IsUpdate = False Then

                SQL = "INSERT INTO GiftRegistry (ID, InvNum, CID, SoldToBlock, Amount, Completed, GiftiDNumber, Status) Values (" & ID & ", " & gInvoiceNumber & ", " & CID & ", " &
                    "'" & SoldToBlock & "', " & Amount & ", False, '" & GIDNumber & "', 'OPEN')"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

            End If
            Return 0

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Posting Gift Card.")
        End Try

    End Function

    Private Function PostPayments(Optional Status As String = "") As Integer

        Try
            Dim SQL As String
            Dim Segment As String
            Dim SegmentSet As String
            Dim ID As Long
            Dim AR As String
            Dim ret As Integer
            Dim SaleAmount As Double
            Dim PaidAmount As Double
            Dim Balance As Double
            Dim amt As Double
            Dim GiftCardInEffect As Boolean = False
            Dim HoldSegment As String = ""
            Dim InvNum As Long = 0
            Dim OldInvNum As Long = 0

            If Status = "" Then

                Status = "Ok"

            End If

            If gResult = "GIFT CARD IN EFFECT" Then

                gResult = ""
                GiftCardInEffect = True

            End If

            ret = 0
            SQL = ""
            Segment = ""
            SegmentSet = ""
            AR = ""
            ID = 0

            Dim NewSaleInEffect As Boolean

            NewSaleInEffect = False
            If gInvoiceNumber = "NewSale" Then

                NewSaleInEffect = True

            End If
            If gInvoiceNumber = "NewSale" Or gInvoiceNumber = "" Or Val(gInvoiceNumber) = 0 Then

                gInvoiceNumber = GetNextInvoiceNumber().ToString

            End If

            SQL = "SELECT MAX(ID) AS MaxID from Payments"
            SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
            ID = Val(ExtractElementFromSegment("MaxID", SegmentSet)) + 1
            Segment = AddElementToSegment(Segment, "DrawerID", gDrawerID)
            If Is_NewSale_Quote_Hold(InvoiceType.Content) And gGrandTotal <> 0 Then

                Segment = ""
                Segment = AddElementToSegment(Segment, "ID", ID.ToString())
                Segment = AddElementToSegment(Segment, "InvNum", gInvoiceNumber.ToString())
                Segment = AddElementToSegment(Segment, "NumericInvoiceNumber", gInvoiceNumber.ToString())
                Segment = AddElementToSegment(Segment, "AcctNum", ExtractElementFromSegment("AR", gCustomerSegment))
                Segment = AddElementToSegment(Segment, "AcctName", ExtractElementFromSegment("Name", gCustomerSegment))
                Segment = AddElementToSegment(Segment, "Date", Today.ToString("MM/dd/yyyy"))
                Segment = AddElementToSegment(Segment, "Time", Now.ToString("HH:mm:ss"))

                If Not gGrandTotal < 0 Then
                    'Sale
                    Segment = AddElementToSegment(Segment, "Charge", gGrandTotal.ToString())
                    Segment = AddElementToSegment(Segment, "Desc", "Sales")
                    Segment = AddElementToSegment(Segment, "Payment", "0")
                    Segment = AddElementToSegment(Segment, "Type", "Sale")

                    If gPM.NewPayments.Count > 0 Then
                        If gPM.NewPayments.First.Type = "CHARGE" Then
                            Segment = AddElementToSegment(Segment, "Paid", "CreditCard")
                        Else
                            Segment = AddElementToSegment(Segment, "Paid", gPM.NewPayments.First.Type)
                        End If

                    End If
                Else
                    'Refund
                    amt = gGrandTotal * -1
                    Segment = AddElementToSegment(Segment, "Payment", amt.ToString())
                    Segment = AddElementToSegment(Segment, "Desc", "Sales Refund")
                    Segment = AddElementToSegment(Segment, "Type", "Refund")
                    Segment = AddElementToSegment(Segment, "Paid", "REFUND")
                End If
                Segment = AddElementToSegment(Segment, "SalesRep", gCurrentUser)
                'Segment = AddElementToSegment(Segment, "Type", "Sale")
                Segment = AddElementToSegment(Segment, "Status", Status)
                Segment = AddElementToSegment(Segment, "DrawerID", gDrawerID)
                Segment = AddElementToSegment(Segment, "DrawerStatus", "Open")
                Segment = AddElementToSegment(Segment, "SoldTo", ExtractElementFromSegment("ID", gCustomerSegment))
                Segment = AddElementToSegment(Segment, "ShipTo", ExtractElementFromSegment("ID", gCustomerSegment))

                SQL = MakeInsertSQLFromSchema("Payments", Segment, gdbSchema_Payments, True)
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                ID += 1

            End If

            If gPM.NewPayments.Count = 0 Then

                SQL = "SELECT Charge FROM Payments WHERE InvNum = '" & gInvoiceNumber.ToString & "' AND [Type] = 'Sale'"
                Segment = IO_GetSegmentSet(gShipriteDB, SQL)
                SaleAmount = Val(ExtractElementFromSegment("Charge", Segment))
                SQL = "SELECT SUM(Charge) AS Charged, SUM(Payment) AS Paid FROM Payments WHERE InvNum = '" & gInvoiceNumber.ToString & "' AND NOT [Type] = 'Sale'"
                Segment = IO_GetSegmentSet(gShipriteDB, SQL)
                PaidAmount = Val(ExtractElementFromSegment("Paid", Segment)) - Val(ExtractElementFromSegment("Charged", Segment))
                SQL = "UPDATE PAYMENTS Set SaleAmount = " & SaleAmount & ", Balance = " & SaleAmount - PaidAmount & " WHERE  InvNum = '" & gInvoiceNumber.ToString & "'"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                Return 0
                Exit Function

            End If

            For Each item As PaymentDefinition In gPM.NewPayments

                If item.RecoveredPayment = False Then

                    If item.InvNum = 0 Then
                        item.InvNum = gInvoiceNumber.ToString
                    End If

                    Segment = ""
                    Segment = AddElementToSegment(Segment, "ID", ID.ToString())
                    If item.AdjustmentInvoiceNumber = 0 Then

                        Segment = AddElementToSegment(Segment, "InvNum", item.InvNum)
                        Segment = AddElementToSegment(Segment, "NumericInvoiceNumber", item.InvNum)

                    Else

                        Segment = AddElementToSegment(Segment, "InvNum", item.AdjustmentInvoiceNumber)
                        Segment = AddElementToSegment(Segment, "NumericInvoiceNumber", item.AdjustmentInvoiceNumber)

                    End If
                    item.AdjustmentInvoiceNumber = 0
                    Segment = AddElementToSegment(Segment, "AcctNum", ExtractElementFromSegment("AR", gCustomerSegment))
                    Segment = AddElementToSegment(Segment, "AcctName", ExtractElementFromSegment("Name", gCustomerSegment))
                    Segment = AddElementToSegment(Segment, "Date", item.PostDate)
                    Segment = AddElementToSegment(Segment, "Time", Now.ToString("HH:mm:ss"))
                    Segment = AddElementToSegment(Segment, "Desc", item.Desc)
                    Segment = AddElementToSegment(Segment, "Charge", item.Charge)
                    Segment = AddElementToSegment(Segment, "Payment", item.Payment)
                    Segment = AddElementToSegment(Segment, "SalesRep", gCurrentUser)
                    Segment = AddElementToSegment(Segment, "Type", item.Type)
                    Segment = AddElementToSegment(Segment, "Status", Status)
                    Segment = AddElementToSegment(Segment, "DrawerID", gDrawerID)
                    Segment = AddElementToSegment(Segment, "DrawerStatus", "Open")
                    Segment = AddElementToSegment(Segment, "SoldTo", ExtractElementFromSegment("ID", gCustomerSegment))
                    Segment = AddElementToSegment(Segment, "ShipTo", ExtractElementFromSegment("ID", gCustomerSegment))
                    Segment = AddElementToSegment(Segment, "CCEndBlock", gReceiptCCEndBlock)

                    If Not String.IsNullOrWhiteSpace(item.Type) Then
                        If item.Type.ToUpper = "CHECK" Then

                            Segment = AddElementToSegment(Segment, "NameOnCheck", item.Check_Name)
                            Segment = AddElementToSegment(Segment, "CheckNum", item.Check_Number)
                            Segment = AddElementToSegment(Segment, "BankNum", item.Check_NameOfBank)
                            Segment = AddElementToSegment(Segment, "State", item.Check_StateOfBank)

                        ElseIf item.Type.ToUpper = "CHARGE" Then

                            Segment = AddElementToSegment(Segment, "ApprovalNum", item.CC_AuthorizationCode)
                            Segment = AddElementToSegment(Segment, "ExpDate", item.CC_ExpDate)
                            Segment = AddElementToSegment(Segment, "CCNum", "************" & item.CC_Last4)
                            Segment = AddElementToSegment(Segment, "CardName", item.CC_CardName)
                            Segment = AddElementToSegment(Segment, "BankNum", item.CC_TypeOfCard)

                        ElseIf item.Type.ToUpper = "OTHER" Then

                            Segment = AddElementToSegment(Segment, "OtherText", item.OtherText)

                        End If
                    End If
                    HoldSegment = Segment

                    SQL = MakeInsertSQLFromSchema("Payments", Segment, gdbSchema_Payments, True)
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                    If Not InStr(1, item.Desc, "CR FROM:") = 0 Then

                        InvNum = Val(Mid(item.Desc, 8))
                        Segment = HoldSegment
                        OldInvNum = Val(ExtractElementFromSegment("InvNum", Segment))
                        ID += 1
                        Segment = AddElementToSegment(Segment, "ID", ID.ToString)
                        Segment = AddElementToSegment(Segment, "InvNum", InvNum.ToString)
                        Segment = AddElementToSegment(Segment, "Desc", "CR TO:" & OldInvNum.ToString)
                        Segment = AddElementToSegment(Segment, "Charge", ExtractElementFromSegment("Payment", Segment))
                        Segment = RemoveElementFromSegment("Payment", Segment)

                        SQL = MakeInsertSQLFromSchema("Payments", Segment, gdbSchema_Payments, True)
                        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)


                        Update_Invoice_Balance(InvNum)
                    End If


                End If
                ID += 1

            Next

            Dim UniqueInvoiceNumbers As List(Of PaymentDefinition) = gPM.NewPayments.GroupBy(Function(x) x.InvNum).Select(Function(x) x.First).ToList

            For Each payment As PaymentDefinition In UniqueInvoiceNumbers
                Balance = Update_Invoice_Balance(payment.InvNum.ToString)
            Next



            If GiftCardInEffect = True Then

                gResult = Val(Balance * -1).ToString

            End If
            Return 0

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Posting Payments.")
        End Try
    End Function


    Private Function PostTransactions(Optional Status As String = "") As Integer
        Try
            Dim i As Integer
            Dim SQL As String
            Dim Segment As String
            Dim SegmentSet As String
            Dim ret As Integer
            Dim ID As Long

            If Status = "" Then

                Status = "Sold"

            End If
            SQL = "SELECT MAX(ID) AS MaxID FROM Transactions"
            SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
            ID = Val(ExtractElementFromSegment("MaxID", SegmentSet)) + 1
            ret = 0
            For i = 0 To POSLines.Count - 1

                Segment = ""
                Segment = AddElementToSegment(Segment, "ID", ID.ToString())
                Segment = AddElementToSegment(Segment, "InvNum", gInvoiceNumber.ToString())
                Segment = AddElementToSegment(Segment, "NumericInvoiceNumber", gInvoiceNumber.ToString())
                Segment = AddElementToSegment(Segment, "AcctNum", ExtractElementFromSegment("AR", gCustomerSegment))
                Segment = AddElementToSegment(Segment, "AcctName", ExtractElementFromSegment("Name", gCustomerSegment))
                Segment = AddElementToSegment(Segment, "Date", Today.ToString("MM/dd/yyyy"))
                Segment = AddElementToSegment(Segment, "Time", Now.ToString("HH:mm:ss"))

                Segment = AddElementToSegment(Segment, "SKU", POSLines(i).SKU)
                Segment = AddElementToSegment(Segment, "Desc", POSLines(i).Description)
                Segment = AddElementToSegment(Segment, "Dept", POSLines(i).Department)
                Segment = AddElementToSegment(Segment, "Qty", POSLines(i).Quantity.ToString())
                Segment = AddElementToSegment(Segment, "UnitPrice", POSLines(i).UnitPrice.ToString("0.00"))
                Segment = AddElementToSegment(Segment, "Disc", POSLines(i).Discount.ToString())
                Segment = AddElementToSegment(Segment, "ExtPrice", POSLines(i).ExtPrice.ToString("0.00"))
                Segment = AddElementToSegment(Segment, "TRate", POSLines(i).TRate.ToString())
                Segment = AddElementToSegment(Segment, "STax", POSLines(i).STax.ToString())
                Segment = AddElementToSegment(Segment, "TCounty", POSLines(i).TaxCounty)
                Segment = AddElementToSegment(Segment, "LTotal", Math.Round(POSLines(i).LTotal, 6))
                Segment = AddElementToSegment(Segment, "COGS", POSLines(i).COGS)
                Segment = AddElementToSegment(Segment, "UnitCost", POSLines(i).UnitCost)
                Segment = AddElementToSegment(Segment, "PackageID", POSLines(i).PackageID)
                If Not POSLines(i).STax.ToString() = 0 Then

                    Segment = AddElementToSegment(Segment, "TaxableSales", POSLines(i).ExtPrice.ToString())

                Else

                    Segment = AddElementToSegment(Segment, "NonTaxableSales", POSLines(i).ExtPrice.ToString())

                End If
                Segment = AddElementToSegment(Segment, "InvoiceTotal", gGrandTotal)

                Segment = AddElementToSegment(Segment, "SalesRep", gCurrentUser)
                Segment = AddElementToSegment(Segment, "ReportClass", "PRODUCTION")
                Segment = AddElementToSegment(Segment, "Status", Status)
                Segment = AddElementToSegment(Segment, "DrawerID", gDrawerID)
                Segment = AddElementToSegment(Segment, "DrawerStatus", "Open")
                Segment = AddElementToSegment(Segment, "SoldTo", ExtractElementFromSegment("ID", gCustomerSegment))
                Segment = AddElementToSegment(Segment, "ShipTo", ExtractElementFromSegment("ID", gCustomerSegment))
                If Not InvoiceType.Content = "Recovered Invoice" Then

                    SQL = MakeInsertSQLFromSchema("Transactions", Segment, gdbSchema_Transactions, True)
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                    ID += 1

                End If

            Next

            Return 0

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Posting Transactions.")
        End Try
    End Function

    Private Function ClearPOS() As Integer
        Try
            Dim ret As Integer
            Dim buf As String
            Dim tbuf As String

            gReceiptCCEndBlock = ""
            SaleOptions_Btn.Visibility = Visibility.Hidden
            ret = 0
            POSLines = New ObjectModel.ObservableCollection(Of POSLine) ' reset
            Receipt_LB.ItemsSource = POSLines
            Receipt_LB.Items.Refresh()
            gPOSCurrentTaxSegment = gPOSDefaultTaxSegment
            gRefundSegment = ""
            gChangeDue = 0
            ResetReceiptHeader()
            AccountNo_TxtBox.Content = ""
            Balance_TxtBox.Content = ""
            PoleDisplay_Total.Text = "$ 0.00"
            buf = FillData("SubTotal:" & Format(0, "0.00"), 23, "L")
            tbuf = FillData("Sales Tax: " & Format(0, "0.00"), 23, "R")
            PoleDisplay_Subtotal.Text = buf & tbuf
            RecoveredInvoiceNumber = 0
            InvoiceBalance.Content = ""
            DInput.Text = ""
            DInput.Focus()
            InvoiceType.Content = "New Sale"
            CashOut_TxtBx.Text = "   CASH" & vbCrLf & "    OUT"
            gInvoiceNumber = 0
            gGrandTotal = 0
            gSubTotal = 0
            gSalesTax = 0
            InvoiceNote.Text = ""
            RecoveredInvoicePayments = Nothing

            If gIsCustomerDisplayEnabled Then
                gCustomerDisplay.UpdatePOS(POSLines)
            End If
        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Clearing POS.")
        End Try
        Return 0

    End Function

    Private Sub ContactSearch(SKU As String)
        Try
            'Dim Segment As String
            Dim SegmentSet As String
            Dim SearchField = "Name"
            Dim Sql = "SELECT Name, FullAddress, Phone, ID FROM Contacts WHERE Name LIKE '<<SEED>>%'"
            If Regex.Match(SKU, "^[+]*[(]{0,1}[0-9]{1,4}[)]{0,1}[-\s\./0-9]*$").Success Then
                ' Phone number found
                Debug.Print("Search by phone: " & SKU)
                SearchField = "Phone"
                Sql = "SELECT Name, FullAddress, Phone, ID FROM Contacts WHERE Phone LIKE '<<SEED>>%' OR HPhone LIKE '<<SEED>>%' OR CellPhone LIKE '<<SEED>>%' OR Phone2 LIKE '<<SEED>>%'"
            End If
            Dim buf = ""
            buf = SearchList(Me, buf, "Customers", SearchField, "Customer Search", Sql, SKU)
            If Not gResult = "" Then

                Lookup_Contact()
                DInput.Focus()

                DInput.Text = ""
                gResult = ""
                Exit Sub

            End If
            If buf = "" Then

                MsgBox("ATTENTION..." & vbCrLf & "Contact '" & SKU & "' NOT FOUND", vbCritical)
                DInput.Text = ""
                Exit Sub

            End If
            Sql = "SELECT * FROM Contacts WHERE ID = " & buf
            SegmentSet = IO_GetSegmentSet(gShipriteDB, Sql)
            gCustomerSegment = GetNextSegmentFromSet(SegmentSet)
            LoadInForm_Contact(gCustomerSegment)
            DInput.Text = ""

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error in POS Contact Search.")
        End Try
    End Sub

    Function ProcessInput() As Integer

        Dim SKU As String
        Dim SQL As String
        Dim Segment As String
        Dim SegmentSet As String
        Dim InventorySegmentSet As String
        Dim ret As Integer
        Dim oQuantity As Double = 0
        Dim oPrice As Double = 0
        Dim oTax As Double = 0
        Dim oDisc As Double = 0
        Dim buf As String = ""
        Dim i As Integer
        Dim NoteID As String = ""
        Dim Count As Integer
        Dim CQDMultiple As Boolean = False

        Try

            If Not Is_NewSale_Quote_Hold(InvoiceType.Content) Then
                Return 0
            End If
            SQL = ""
            Segment = ""
            SegmentSet = ""
            ret = 0
            SKU = DInput.Text.Trim


            If SKU.Trim.Count = 0 Then
                Debug.Print("SKU Empty")
                ' Open SKU search window
                SQL = "SELECT SKU, Desc, Cost, Quantity FROM Inventory WHERE SKU LIKE '<<SEED>>%'"
                buf = SearchList(Me, buf, "SKU", "SKU", "SKU Search", SQL, DInput.Text)
                If buf = "" Then
                    ' no results found, not sure this check is needed
                    Return 1
                End If
                SKU = buf
            End If


            If Not Check_Static_SKUs(SKU) Then
                'Check for barcode match first.
                SQL = "SELECT * FROM Inventory WHERE UCASE(Barcode) = '" & SKU.ToUpper & "' OR SKU='" & SKU & "' OR Desc = '" & SKU & "' ORDER BY SKU='" & SKU & "'"
                SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)


                If SegmentSet = "" And InStr(1, SKU, ",") = 0 And InStr(1, SKU, " ") = 0 Then
                    ' Check for partial SKU
                    SQL = "SELECT SKU, Desc, Sell FROM Inventory WHERE SKU LIKE '" & SKU & "%' OR Desc LIKE '%" & SKU & "%'"
                    InventorySegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)

                    If InventorySegmentSet = "" Then
                        ' No SKUs found, run name search
                        SQL = "SELECT Count(*) As [count] FROM Contacts WHERE Name LIKE '" & SKU & "%' OR Phone LIKE '" & SKU & "%' OR HPhone LIKE '" & SKU & "%' OR CellPhone LIKE '" & SKU & "%' OR Phone2 LIKE '" & SKU & "%'"
                        SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
                        Segment = GetNextSegmentFromSet(SegmentSet)
                        If ExtractElementFromSegment("count", Segment, "0") > 0 Then
                            ContactSearch(SKU)
                        Else
                            'check for phone number without dashes.
                            If Regex.IsMatch(SKU, "^[0-9 ]+$") And SKU.Length = 10 Then
                                SKU = SKU.Insert(3, "-")
                                SKU = SKU.Insert(7, "-")
                                ContactSearch(SKU)

                            Else

                                MsgBox("ATTENTION...SKU [" & SKU & "] NOT FOUND" & vbCrLf & vbCrLf & "TRY AGAIN", vbCritical)
                            End If
                            ' No results, error out

                        End If
                        DInput.Text = ""
                        Return 0


                    Else
                        Count = countString(InventorySegmentSet, "<SET>")

                        If Count > 1 Then
                            'Multiple results, show hotsearch.
                            SKU_Search(SKU, InventorySegmentSet)
                            Return 0
                        ElseIf Count = 1 Then
                            ' Only one SKU result, use it
                            Segment = GetNextSegmentFromSet(InventorySegmentSet)
                            SKU = ExtractElementFromSegment("SKU", Segment)
                        End If


                        SQL = "SELECT * FROM Inventory WHERE SKU = '" & SKU & "'"
                        SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)

                    End If
                    DInput.Text = ""
                End If


                If SegmentSet = "" And (InStr(1, SKU, ",") <> 0 Or InStr(1, SKU, " ") <> 0) Then
                    ' Search for contact
                    ContactSearch(SKU)
                    Return 0
                End If
                Segment = GetNextSegmentFromSet(SegmentSet)
                SKU = ExtractElementFromSegment("SKU", Segment, "")

                If Not OverrideQuantity.Content = "" Then
                    buf = OverrideQuantity.Content
                    i = InStr(1, buf, ":")
                    If Not i = 0 Then

                        buf = Mid(buf, i + 1)

                    End If
                    oQuantity = Val(buf)
                    OverrideQuantity.Content = ""

                Else
                    oQuantity = 1
                End If


                oPrice = GetSellingPrice(Segment, oQuantity)

                AddPosLineToSet(0, SKU, Segment, oPrice, oQuantity, Val(OverrideDisc.Content))
                If Not OverridePrice.Content = "" Then
                    POSLines.Last.isPriceOverride = True
                Else
                    POSLines.Last.isPriceOverride = False
                End If

                'Price override needs to be cleared after being used.
                OverridePrice.Content = ""

                If ExtractElementFromSegment("Includes", Segment, "") <> "" Then
                    NoteLineToPOS(ExtractElementFromSegment("Includes", Segment, ""))
                End If

                If ExtractElementFromSegment("PopupMessage", Segment, "") <> "" Then
                    MsgBox(ExtractElementFromSegment("PopupMessage", Segment, ""), vbInformation)
                End If
            End If



            ret = CalculateInvoice()

            Receipt_LB.Items.Refresh()

            If Receipt_LB.Items.Count > 0 Then
                Receipt_LB.ScrollIntoView(Receipt_LB.Items(Receipt_LB.Items.Count - 1))
            End If

            If ExtractElementFromSegment("LinkedSKU", Segment, "") <> "" Then
                DInput.Text = ExtractElementFromSegment("LinkedSKU", Segment, "")
                DInput.Focus()
                DInput.CaretIndex = DInput.Text.Length
            Else
                DInput.Text = ""
                DInput.Focus()
            End If

            Return 0

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Processing Input.")
        End Try
        Return 0

    End Function

    Public Function Check_Static_SKUs(SKU As String) As Boolean
        Try

            Dim SQL As String
            Dim Segment As String
            Dim SegmentSet As String
            Dim NoteID As String = ""

            If Val(OverrideQuantity.Content) < 0 Then
                'Refund item, ignore static SKUs
                Return False
            End If

            If Mid(SKU, 0, 3) = "CQD" Then
                gResult = SKU
                SKU = Mid(SKU, 0, 3)
            End If

            If SKU.Contains("CustomNote") Then
                NoteID = Integer.Parse(Regex.Replace(SKU, "[^\d]", "")).ToString
                SKU = "CustomNote"
            End If

            Select Case UCase(SKU)

                Case "CUSTOMNOTE"
                    SQL = "SELECT Desc FROM POSButtons WHERE SKU='" & SKU & NoteID & "'"
                    SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
                    NoteLineToPOS(ExtractElementFromSegment("Desc", SegmentSet, ""))

                Case "GIFT"

                    GiftCardNumber.Text = ""
                    GiftCardNumber2.Text = ""
                    gInvoiceNumber = 0
                    NewGiftCard_Popup.IsOpen = True
                    GiftCardNumber.Focus()
                    Return 0

                Case "PACKMASTER"

                    gCallingSKU = "PackMaster"
                    Dim win As New Packmaster(Me)
                    win.ShowDialog(Me)
                    For i = 0 To POSLines.Count - 1
                        Dim LineItem As POSLine = POSLines(i)
                        If LineItem.PackMaster = True Then

                            LineItem.PackMaster = False
                            POSLines(i) = LineItem

                        End If

                    Next
                    win.Close() ' close window object so user can't press forward button and go back into ShipManager
                    gCallingSKU = ""

                Case "CQD"

                    gCallingSKU = "SHIP1"

                    SQL = "SELECT * FROM Preship WHERE TransID = " & Val(Mid(gResult, 3))
                    SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
                    Do Until SegmentSet = ""

                        Dim win As New ShipManager(Me)
                        Segment = GetNextSegmentFromSet(SegmentSet)
                        gResult2 = Segment
                        win.ShowDialog(Me)

                        For i = 0 To POSLines.Count - 1
                            Dim LineItem As POSLine = POSLines(i)
                            If POSLines(i).PackMaster = True Then

                                LineItem.PackMaster = False
                                POSLines(i) = LineItem

                            End If

                        Next
                        win.Close() ' close window object so user can't press forward button and go back into ShipManager

                    Loop
                    LoadInForm_Contact(gCustomerSegment)

                Case "SHIP1"

                    gCallingSKU = "SHIP1"
                    Dim win As New ShipManager(Me)
                    win.ShowDialog(Me)

                    For i = 0 To POSLines.Count - 1
                        Dim LineItem As POSLine = POSLines(i)
                        If POSLines(i).PackMaster = True Then

                            LineItem.PackMaster = False
                            POSLines(i) = LineItem

                        End If

                    Next
                    win.Close() ' close window object so user can't press forward button and go back into ShipManager

                Case "SHIPM"

                    gCallingSKU = "SHIPM"
                    Dim win As New ShipManager(Me)
                    win.ShowDialog(Me)
                    win.Close() ' close window object so user can't press forward button and go back into ShipManager

                Case "SHIPL"

                    gCallingSKU = "SHIPL"
                    Dim win As New ShipManager(Me)
                    win.ShowDialog(Me)
                    win.Close() ' close window object so user can't press forward button and go back into ShipManager

                Case "MBX"

                    If gCustomerSegment = "" Or ExtractElementFromSegment("Name", gCustomerSegment) = "Cash, Check, Charge" Then
                        MsgBox("Cannot Rent Mailbox, Please click on 'Customer Lookup' and pull up a customer first!", vbExclamation + vbOKOnly, "Error!")
                    Else

                        Dim win As New MailboxManager(Me, "MBX")
                        win.ShowDialog(Me)

                    End If


                Case "MBXR"

                    If gCustomerSegment = "" Or ExtractElementFromSegment("Name", gCustomerSegment) = "Cash, Check, Charge" Then
                        MsgBox("Cannot Rent Mailbox, Please click on 'Customer Lookup' and pull up a customer first!", vbExclamation + vbOKOnly, "Error!")
                    Else

                        Dim win As New MailboxManager(Me, "MBXR")
                        win.ShowDialog(Me)
                    End If

                Case "MBXM"

                    Dim win As New MailboxManager(Me)
                    win.ShowDialog(Me)

                Case "MAILMASTER"

                    Dim win As New MailMaster(Me)
                    win.ShowDialog(Me)

                Case "DOM"
                    Call _DropOff.Open_DropOffManager(Me, gCurrentUser, Nothing)

                Case Else
                    Return False

            End Select

            Return True

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Processing Input.")
            Return False
        End Try
    End Function

    Public Function countString(ByVal inputString As String, ByVal stringToBeSearchedInsideTheInputString As String) As Integer
        Return Regex.Split(inputString, stringToBeSearchedInsideTheInputString).Length - 1
    End Function

#Region "SKU Search popup"

    Private Sub ColumnHeader_Click(sender As Object, e As RoutedEventArgs)
        Try
            'Sorts ListView by clicked Column Header
            Sort_LV_byColumn(sender, TryCast(e.OriginalSource, GridViewColumnHeader), Last_Sort_Ascending, Last_Column_Sorted)

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error sorting Column Header.")
        End Try
    End Sub

    Private Sub DInput_TextChanged(sender As Object, e As TextChangedEventArgs) Handles DInput.TextChanged
        If IsNothing(SKUSearch_Popup) Then Exit Sub
        SKUSearch_Popup.IsOpen = False
    End Sub


    Private Sub SKU_Search(SKU As String, InventorySegmentSet As String)
        Try
            Dim Segment As String = ""
            Dim SearchList As List(Of SKUSearchItem)
            Dim item As SKUSearchItem

            'SQL = "SELECT SKU, Desc, Sell FROM Inventory WHERE SKU LIKE '" & SKU & "%' OR Desc LIKE '" & SKU & "%'"
            'SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)

            If InventorySegmentSet <> "" Then
                SearchList = New List(Of SKUSearchItem)

                Do Until InventorySegmentSet = ""
                    item = New SKUSearchItem
                    Segment = GetNextSegmentFromSet(InventorySegmentSet)

                    item.SKU = ExtractElementFromSegment("SKU", Segment)
                    item.Description = ExtractElementFromSegment("Desc", Segment)
                    item.Price = ExtractElementFromSegment("Sell", Segment, "0")

                    SearchList.Add(item)
                Loop
                SKUSearch_Popup.IsOpen = True
                SKUSearch_LV.Focus()
                SKUSearch_LV.ItemsSource = SearchList
                SKUSearch_LV.Items.Refresh()
                SKUSearch_LV.SelectedIndex = 0
            End If

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error in POS SKU Search.")
        End Try
    End Sub

    Private Sub SKUSearch_Select_Btn_Click(sender As Object, e As RoutedEventArgs) Handles SKUSearch_Select_Btn.Click
        If SKUSearch_LV.SelectedIndex = -1 Then Exit Sub
        SKUSearch_SelectSKU()
    End Sub

    Private Sub SKUSearch_Cancel_Btn_Click(sender As Object, e As RoutedEventArgs) Handles SKUSearch_Cancel_Btn.Click
        SKUSearch_Popup.IsOpen = False
    End Sub

    Private Sub SKUSearch_LV_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles SKUSearch_LV.MouseDoubleClick
        If SKUSearch_LV.SelectedIndex = -1 Then Exit Sub
        SKUSearch_SelectSKU()
    End Sub

    Private Sub SKUSearch_LV_KeyDown(sender As Object, e As Input.KeyEventArgs) Handles SKUSearch_LV.KeyDown
        If SKUSearch_LV.SelectedIndex = -1 Then Exit Sub

        If e.Key = Key.Return Then
            SKUSearch_SelectSKU()
        End If
    End Sub

    Private Sub SKUSearch_SelectSKU()
        Try
            DInput.Text = SKUSearch_LV.SelectedItem.SKU
            SKUSearch_LV.ItemsSource = Nothing
            SKUSearch_LV.Items.Refresh()
            SKUSearch_Popup.IsOpen = False
            ProcessInput()

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error in POS SKU Search.")
        End Try
    End Sub

#End Region

    Private Function GetSellingPrice(Segment As String, Qty As Double) As Double
        Try

            Dim LevelPrice As Double

            'Price Override
            If Not OverridePrice.Content = "" Then
                Return Val(FlushOut(OverridePrice.Content, "$", ""))
            End If


            'Level Pricing
            LevelPrice = GetLevelPricing(Segment, Qty)
            If LevelPrice <> 0 Then
                Return LevelPrice
            End If


            'Regular Pricing
            Return Val(ExtractElementFromSegment("Sell", Segment))

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error obtaining Selling Price.")
        End Try
    End Function

    Private Function GetLevelPricing(ByRef Segment As String, Qty As Double) As Double
        Try
            Dim QL1 As Long
            Dim QL2 As Long
            Dim QL3 As Long
            Dim QL4 As Long
            Dim QL5 As Long
            Dim QL6 As Long
            Dim QL7 As Long
            Dim ColID As String = ""
            Dim SellLevel As String = ""
            Dim DBField As String = ""
            Dim SegmentSet As String

            If AccountNo_TxtBox.Content <> "" And AccountNo_TxtBox.Content <> "CASH" Then
                'Check if AR account has level setup
                SegmentSet = IO_GetSegmentSet(gShipriteDB, "SELECT PLevel FROM AR WHERE AcctNum = '" & AccountNo_TxtBox.Content & "'")
                SellLevel = ExtractElementFromSegment("Plevel", SegmentSet, "")
            End If

            If SellLevel = "" Then
                SellLevel = ExtractElementFromSegment("SellingLevel", Segment, "")
            End If

            If SellLevel.Contains("Level") Then
                QL1 = ExtractElementFromSegment("QL1", Segment, "0")
                QL2 = ExtractElementFromSegment("QL2", Segment, "0")
                QL3 = ExtractElementFromSegment("QL3", Segment, "0")
                QL4 = ExtractElementFromSegment("QL4", Segment, "0")
                QL5 = ExtractElementFromSegment("QL5", Segment, "0")
                QL6 = ExtractElementFromSegment("QL6", Segment, "0")
                QL7 = ExtractElementFromSegment("QL7", Segment, "0")

                'Check if any Level quantities are setup
                If QL1 + QL2 + QL3 + QL4 + QL5 + QL6 + QL7 <> 0 Then

                    If Qty >= QL1 Then 'if quantity less then QL1, revert to regular pricing.

                        Select Case Qty
                            Case QL1 To IIf(QL2 = 0, Qty, QL2 - 1) 'If pricing quantity levels stop short with trailing 0 quantity levels, then use the last non-zero quantity level.
                                ColID = "A"

                            Case QL2 To IIf(QL3 = 0, Qty, QL3 - 1)
                                ColID = "B"

                            Case QL3 To IIf(QL4 = 0, Qty, QL4 - 1)
                                ColID = "C"

                            Case QL4 To IIf(QL5 = 0, Qty, QL5 - 1)
                                ColID = "D"

                            Case QL5 To IIf(QL6 = 0, Qty, QL6 - 1)
                                ColID = "E"

                            Case QL6 To IIf(QL7 = 0, Qty, QL7 - 1)
                                ColID = "F"

                            Case Is >= QL7
                                ColID = "G"

                        End Select

                        DBField = SellLevel.Replace(" ", "") & ColID
                        Return ExtractElementFromSegment(DBField, Segment, "0")
                    End If
                End If

            End If

            Return 0

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Loading Level Pricing from Inventory.")
        End Try
    End Function


    Function LoadPosButtons() As Integer

        Dim SQL As String
        Dim Segment As String
        Dim SegmentSet As String

        Dim BN As Integer
        Dim low As Integer
        Dim high As Integer
        Dim i As Integer

        Dim R_Color As Long
        Dim G_Color As Long
        Dim B_Color As Long

        Dim ForeColor As String
        Dim BackColor As String

        Try

            low = 0
            high = 19

            For i = low To high

                Dim D As Button = Me.FindName("BT" & i.ToString)

                D.Content = ""
                D.Tag = ""
                D.Background = New System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(221, 221, 221))


            Next i
            SQL = "SELECT PosButtons.* FROM PosButtons WHERE [Group] = '" & CurrentGroup.Text & "'"
            SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)

            Do Until SegmentSet = ""

                Segment = GetNextSegmentFromSet(SegmentSet)
                BN = Val(ExtractElementFromSegment("BN", Segment))
                If BN >= 43 Then
                    BN -= 43
                End If
                If BN >= low And BN <= high Then

                    Dim D As Button = Me.FindName("BT" & BN.ToString)

                    D.Content = ExtractElementFromSegment("ButtonDesc", Segment)
                    'D.Tag = ExtractElementFromSegment("ID", Segment) & ":" & ExtractElementFromSegment("Type", Segment) & ":" & ExtractElementFromSegment("SKU", Segment) & ":" & ExtractElementFromSegment("Qty", Segment)
                    D.Tag = Segment

                    'Set background color
                    BackColor = ExtractElementFromSegment("BackColor", Segment)
                    If Not IsNumeric(BackColor) Then
                        BackColor = 0
                    End If
                    Color_to_RGB(BackColor, R_Color, G_Color, B_Color)
                    D.Background = New System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(R_Color, G_Color, B_Color))

                    'Set Foreground color
                    ForeColor = ExtractElementFromSegment("ForeColor", Segment)
                    If Not IsNumeric(ForeColor) Then
                        ForeColor = 255
                    End If
                    Color_to_RGB(ForeColor, R_Color, G_Color, B_Color)
                    D.Foreground = New System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(R_Color, G_Color, B_Color))

                End If

            Loop
            Return 0

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Loading POS Button.")
            Return 0
        End Try

    End Function

    Function Color_to_RGB(Color As Long, Optional ByRef R As Long = 0, Optional ByRef G As Long = 0, Optional ByRef B As Long = 0)
        Try

            If Color = -2147483640 Then Color = 0

            R = Math.Abs(Color Mod 256)
            G = Math.Abs((Color \ 256) Mod 256)
            B = Math.Abs((Color \ 256 \ 256) Mod 256)

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error.")
        End Try
        Return 0

    End Function

    Public Shared Function Brush_to_LongColor(cSolidBrush As System.Windows.Media.SolidColorBrush) As Long

        Try

            Dim R As Long = cSolidBrush.Color.R
            Dim G As Long = cSolidBrush.Color.G
            Dim B As Long = cSolidBrush.Color.B
            Dim retColor As Long = (256 * 256 * B) + (256 * G) + R
            Return retColor

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error.")
            Return Nothing
        End Try

    End Function

    Private Function User_LogIn() As Integer

        If gIsPOSSecurityEnabled Then

            Dim win As New UserLogIn(Me, "POS")
            win.ShowDialog()
            'UserLogIn.isAllowed = True
            ' If the login was cancelled, return to previous screen
            If UserLogIn.isAllowed = False Then

                BackButton_Click(Nothing, Nothing)
                Return -1

            End If

        End If
        Return 0

    End Function

    Private Sub Keypad_Click(sender As Object, e As RoutedEventArgs) Handles Keypad_00.Click, Keypad_0.Click, Keypad_1.Click, Keypad_2.Click, Keypad_3.Click, Keypad_4.Click, Keypad_5.Click, Keypad_6.Click, Keypad_7.Click, Keypad_8.Click, Keypad_9.Click, Keypad_DEL.Click, Keypad_MINUS.Click, Keypad_DOT.Click, Keypad_ENTER.Click

        Dim Selection As String
        Dim iloc As Integer
        Dim buf As String

        Try



            Selection = sender.content
            Select Case Selection

                Case "ENTER"
                    DInput.Tag = DInput.Text
                    ProcessInput()
                Case "DEL"
                    ' Check focus before continuing
                    ' DInput.IsFocused cannot be used because the simple act of clicking an input button temporarily removed focus from the textbox
                    'If DInput.IsFocused Then
                    iloc = Len(DInput.Text)
                    iloc -= 1
                    If iloc < 0 Then
                        ' Input box is empty, so check on selected item(s) in Receipt
                        HandleReceiptDelete(sender, e)
                    ElseIf iloc = 0 Then
                        DInput.Text = ""
                    Else
                        buf = Mid(DInput.Text, 0, iloc)
                        DInput.Text = buf
                    End If
                Case Else
                    DInput.Text = DInput.Text + sender.content
            End Select
            DInput.Select(99, 0)
            DInput.Focus()

        Catch ex As Exception

            _MsgBox.ErrorMessage(ex, "Error.")

        End Try
    End Sub

    Private Sub HoldInvoice_Button_Click(sender As Object, e As RoutedEventArgs) Handles HoldInvoice_Button.Click
        Try


            Lookup_Quote()
            DInput.Focus()

        Catch ex As Exception

            _MsgBox.ErrorMessage(ex, "Error.")

        End Try
    End Sub

    Private Sub InvoiceLookup_Button_Click(sender As Object, e As RoutedEventArgs) Handles InvoiceLookup_Button.Click
        Try



            Lookup_Invoice()

            CheckInvoiceNotes()

        Catch ex As Exception

            _MsgBox.ErrorMessage(ex, "Error.")

        End Try
    End Sub

    Private Sub CheckInvoiceNotes()
        'Invoice Notes
        Dim sql As String
        Dim segmentSet As String

        Try
            If gInvoiceNumber = "NewSale" Or gInvoiceNumber = "0" Then

                Exit Sub

            End If
            sql = "SELECT * FROM InvoiceNotes WHERE InvNum = " & gInvoiceNumber
            segmentSet = IO_GetSegmentSet(gShipriteDB, sql)
            If Not segmentSet = "" Then

                InvoiceNote.Text = ExtractElementFromSegment("Note", segmentSet)
                If InvoiceNote.Text <> "" Then
                    POSOptions_Popup.IsOpen = True
                End If

            End If

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error checking invoice notes.")
        End Try

    End Sub

    Private Sub Contacts_Button_Click(sender As Object, e As RoutedEventArgs) Handles Contacts_Button.Click
        Try



            Lookup_Contact()
            DInput.Focus()

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error.")

        End Try

    End Sub

    Private Sub ProcessTransaction_Button_Click(sender As Object, e As RoutedEventArgs) Handles ProcessTransaction_Button.Click

        Dim ret As Integer
        Dim amt As Double
        Dim temp As Double
        Dim SQL As String = ""
        Dim SegmentSet As String = ""
        Dim ID As String = ""
        Dim buf As String = ""
        Dim iloc As Integer = 0

        Dim Charge As Double = 0
        Dim Paid As Double = 0
        Dim Balance As Double = 0
        Dim CorrectRefundAmount As Double = 0
        Dim AdjustmentAmount As Double = 0
        Dim AcctBalance As Double = 0

        CloseLineEditPopup()

        If Balance_TxtBox.Content <> "" Then
            AcctBalance = CDbl(Balance_TxtBox.Content.ToString)
        End If

        ret = 0

        If Receipt_LB.Items.Count = 0 And AcctBalance = 0 And Not gROAinEffect Then

            MsgBox("ATTENTION..Invoice Total is $0.00", vbInformation, gProgramName)
            Exit Sub

        End If

        If (InStr(1, InvoiceBalance.Content, "$ 0.00") Or InvoiceBalance.Content = "") And 1 = 2 Then

            Exit Sub 'recovered invoice, no balance

        End If

        Try

            gPaymentsCompleted = False
            'gPOS_IsPrintReceipt = True ' default to print receipt

            If Not InvoiceType.Content = "Recovered Invoice" Then
                'new sale
                gPM.Balance = gGrandTotal

            Else
                'recovered invoice
                temp = gGrandTotal
                buf = InvoiceBalance.Content
                iloc = InStr(1, buf, "$")
                buf = Trim(Mid(buf, iloc + 1))
                gGrandTotal = Val(buf)
                gPM.Balance = Val(buf)

            End If


            If gGrandTotal > 0 Or gROAinEffect Then
                'Payment for current sale
                Dim win As New POS_Payment(Me, InvoiceType.Content)
                win.ShowDialog(Me)

            ElseIf (ValFix(InvoiceBalance.Content) > 0 Or InvoiceBalance.Content = "") And AccountNo_TxtBox.Content <> "" Then
                'payment on account
                Dim win As New POS_Payment(Me, InvoiceType.Content)
                win.ShowDialog(Me)

            ElseIf gGrandTotal < 0 Then
                'REFUND

                Dim tempUser As String = gCurrentUser

                If gIsPOSSecurityEnabled Then
                    If Not Check_Current_User_Permission("POS_Refunds", True) Then
                        If MsgBox("User " & gCurrentUser & " does Not have the permission To refund sales." & vbCrLf & vbCrLf & "Refund needs To be approved by a authorized person!", vbExclamation + vbOKCancel) = vbCancel Then
                            Exit Sub
                        Else

                            If OpenUserLogin(Me, "POS_Refunds") = False Then
                                gCurrentUser = tempUser
                                Exit Sub
                            End If
                        End If
                    End If
                End If

                If ExtractElementFromSegment("InvNum", gRefundSegment, "") <> "" Then
                    'Quick Refund from original invoice
                    SQL = "SELECT SUM(Charge) AS Charge,  SUM(Payment) AS Paid FROM Payments WHERE InvNum = '" & ExtractElementFromSegment("ReturnInvoiceNumber", gRefundSegment) & "'"
                    SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
                    Charge = Val(ExtractElementFromSegment("Charge", SegmentSet))
                    Paid = Val(ExtractElementFromSegment("Paid", SegmentSet))
                    Balance = Round(Charge - Paid, 2)
                    CorrectRefundAmount = Round(Paid - (Charge + gGrandTotal), 2)

                    If Not Round(CorrectRefundAmount, 2) = 0 Or Paid = 0 Then

                        AdjustmentAmount = Round((gGrandTotal * -1) - CorrectRefundAmount, 2)
                        gRefundSegment = AddElementToSegment(gRefundSegment, "AdjustOldInvoice", AdjustmentAmount.ToString)

                    End If
                Else
                    'refund with no old invoice, new sale with negative total
                    CorrectRefundAmount = gGrandTotal
                End If



                gRefundSegment = AddElementToSegment(gRefundSegment, "RefundAmount", (CorrectRefundAmount * -1).ToString)
                Dim win As New POS_Refund(Me)
                win.ShowDialog(Me)

                gCurrentUser = tempUser

            ElseIf InvoiceType.Content = "Recovered Invoice" And ValFix(InvoiceBalance.Content) = 0 Then

                gGrandTotal = temp
                ret = PrintReceiptWithLoop()
                NewSale()
                Exit Sub

            End If

            If gPaymentsCompleted = True Then
                ' post payments then transactions

                gResult = ""
                ret = PostPayments()
                ret = PostTransactions()
                Save_Invoice_Notes()

                If InvoiceType.Content = "Quote" Or InvoiceType.Content = "Hold" Then
                    'If a quote/hold sale is completed, the quote/hold entries need to be cleared out.
                    SQL = "DELETE * FROM Payments WHERE InvNum = '" & gInvoiceNumber.ToString & "' and Status='" & InvoiceType.Content & "'"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                    SQL = "DELETE * FROM Transactions WHERE InvNum = '" & gInvoiceNumber.ToString & "' and Status='" & InvoiceType.Content & "'"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                    Update_Invoice_Balance(gInvoiceNumber.ToString)
                End If

                UpdateInventory()
                UpdateManifestTable()

                If gPOS_IsPrintReceipt Then
                    ret = PrintReceiptWithLoop()
                End If

                If gPOS_IsPrintFullSheetInvoice Then
                    Print_FullSheetInvoice(gInvoiceNumber)
                End If

                If gPOS_FullSheetInvoice_Email <> "" Then
                    Dim report As New SHIPRITE.ShipRiteReports._ReportObject()

                    Generate_InvoiceReport(report, gInvoiceNumber)
                    Email_Invoice(report, gInvoiceNumber, gPOS_FullSheetInvoice_Email, False)
                End If

                Email_Receipt(False)


                'Change due should display after receipt printed and the drawer is opened.
                amt = gChangeDue
                If Not amt = 0 And gROAinEffect = False Then
                    Dim frm As New POS_ChangeDue
                    frm.ShowDialog()

                End If
                If gROAinEffect = True Then
                    gChangeDue = 0

                End If



                If gGrandTotal < 0 Then

                    'ret = PrintReceiptWithLoop()

                    buf = ExtractElementFromSegment("IDStack", gRefundSegment)
                    If buf <> "" Then
                        Dim IDList As List(Of String) = buf.Split(",").ToList
                        For Each item In IDList
                            SQL = "UPDATE Transactions SET ReturnedQty = " & item.Split("-")(1) & " WHERE ID=" & item.Split("-")(0)
                            ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                        Next
                    End If

                End If

                ret = NewSale()
            End If

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error processing sale.")

        End Try

    End Sub

    Private Sub Email_Receipt(displayConfirmation As Boolean)
        Try

            'Dim RTB As RichTextBox = New RichTextBox
            'RTB.Document.Blocks.Add(New Paragraph(New Run(gPOS_EmailReceipt.EmailBody)))
            'RTB.FontFamily = New System.Windows.Media.FontFamily("Courier New")
            ' gPOS_EmailReceipt.EmailTemplate.Content = RtfPipe.Rtf.ToHtml(EmailSetup.RichBoxToString(RTB))

            If gPOS_EmailReceipt.isEmail Then ' if false, then gPOS_EmailReceipt.EmailBody not set
                PrintReceipt(, True) 'Needed to set Email Body

                gPOS_EmailReceipt.EmailTemplate.Content = gPOS_EmailReceipt.EmailBody
                If sendEmail(gPOS_EmailReceipt.EmailAddress, gPOS_EmailReceipt.EmailTemplate, False) Then
                    If displayConfirmation Then
                        MsgBox("Email Sent!", vbInformation)
                    End If
                End If
            End If

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Emailing Receipt.")

        End Try
    End Sub


    Public Shared Sub Print_FullSheetInvoice(InvNum As String)
        Try

            Dim report As New SHIPRITE.ShipRiteReports._ReportObject()
            Generate_InvoiceReport(report, InvNum)

            Dim reportPrev As New ReportPreview(report)

            reportPrev.ShowDialog()

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error printing Invoice.")
        End Try
    End Sub

    Private Sub UpdateManifestTable()
        Try
            'Check for any shipments on the receipt and update the shipment record in manifest table.

            Dim PackageIDList As List(Of String)
            PackageIDList = POSLines.GroupBy(Function(x) x.PackageID).Select(Function(x) x.First.PackageID).ToList

            For Each ID As String In PackageIDList
                If Not IsNothing(ID) Then
                    IO_UpdateSQLProcessor(gShipriteDB, "UPDATE Manifest SET [InvoiceNumber]='" & gInvoiceNumber & "', [SalesClerk]='" & gCurrentUser & "', [DrawerID]='" & gDrawerID & "' WHERE [PackageID]='" & ID & "'")
                End If
            Next

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error saving invoice data to manifest.")
        End Try
    End Sub

    Private Sub UpdateInventory()
        Try
            Dim i As Integer
            Dim buf As String
            Dim NewQuantity As Double
            Dim OldQuantity As Double

            For i = 0 To POSLines.Count - 1

                buf = IO_GetSegmentSet(gShipriteDB, "SELECT Quantity, WarningQty, Zero, WarningSent FROM Inventory WHERE SKU='" & POSLines(i).SKU & "'")

                If buf <> "" Then

                    If ExtractElementFromSegment("Zero", buf) = "True" Then
                        OldQuantity = ExtractElementFromSegment("Quantity", buf, "0")
                        NewQuantity = OldQuantity - POSLines(i).Quantity

                        IO_UpdateSQLProcessor(gShipriteDB, "UPDATE Inventory SET Quantity=" & NewQuantity & " WHERE SKU='" & POSLines(i).SKU & "'")


                        If CDbl(ExtractElementFromSegment("WarningQty", buf)) >= NewQuantity And ExtractElementFromSegment("WarningSent", buf) = False Then
                            'Setup Tickler Notice
                            Tickler.CreateInventoryTickler(POSLines(i).SKU, POSLines(i).Description)

                        ElseIf CDbl(ExtractElementFromSegment("WarningQty", buf)) < NewQuantity And ExtractElementFromSegment("WarningSent", buf) = True Then
                            'in case of returns that push quantity above warning level, remove tickler flag from inventory.
                            IO_UpdateSQLProcessor(gShipriteDB, "Update Inventory SET WarningSent=False WHERE SKU='" & POSLines(i).SKU & "'")
                        End If
                    End If

                End If
            Next

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error updating invenotry quantity.")
        End Try
    End Sub

    Private Function NewSale() As Integer

        Dim ret As Integer

        Try
            Tickler_Count_Lbl.Content = Tickler.Get_Open_Tickler_Count()
            gPOSHeader = gBlankPosHeader
            'gPOSHeader.TaxCounty = GetPolicyData(gShipriteDB, "DefaultCounty")
            gPOSHeader.TaxCounty = ExtractElementFromSegment("County", gPOSDefaultTaxSegment, "")

            If gPOSDefaultTaxSegment = "" Then
                gPOSHeader.TaxRate = 0
            Else
                gPOSHeader.TaxRate = ExtractElementFromSegment("TaxRate", gPOSDefaultTaxSegment)
            End If


            gPOS_EmailReceipt = New Email_POSReceipt
            InvoiceNote.Text = ""
            CustomerName.Content = "Cash, Check, Charge"
            gCustomerSegment = ""
            gShipToCustomerSegment = ""
            gRefundSegment = ""
            gInvoiceNumber = "NewSale"
            gChangeDue = 0
            RecoveredInvoiceNumber = 0
            gCallingSKU = ""
            ret = ClearPOS()
            CurrentGroup.Text = "MAIN"
            ret = LoadPosButtons()
            gPM = New PayMaster

            gPM.NewPayments = New List(Of PaymentDefinition)
            gCustomerSegment = AddElementToSegment(gCustomerSegment, "AR", "CASH")
            gCustomerSegment = AddElementToSegment(gCustomerSegment, "Name", "Cash, Check, Charge")
            Mbx_ExpDate_TxtBx.Content = ""
            MBX_No_TxtBx.Content = ""
            CustomerName.Foreground = New System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White)

            InvoiceType.Content = "New Sale"
            CashOut_TxtBx.Text = "   CASH" & vbCrLf & "    OUT"
            OverrideDisc.Content = ""
            OverridePrice.Content = ""
            OverrideQuantity.Content = ""
            ContactDropDown_Popup.IsOpen = False
            CloseLineEditPopup()
            isCOGSview = True
            Set_COGS_View()
            Receipt_LB.Items.Refresh()
            DrawerID_Lbl.Content = gDrawerID

            ret = User_LogIn()

            If gIsCustomerDisplayEnabled Then
                gCustomerDisplay.ClearPOS()
            End If

            If Not ret = -1 Then

                Clerk_Lbl.Content = gCurrentUser

            End If
            Return ret

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error starting new sale.")
            Return ret
        End Try


    End Function



    Private Sub ResetReceiptHeader(Optional strDate As String = "")
        Try
            If gInvoiceNumber = "NewSale" Or Val(gInvoiceNumber) = 0 Then
                Rctp_InvoiceNum_TB.Text = "-----"
            Else
                Rctp_InvoiceNum_TB.Text = gInvoiceNumber
            End If

            If strDate.Trim = "" And Not IsDate(strDate) Then
                Rctp_Date_TB.Text = Now.ToShortDateString
            Else
                Rctp_Date_TB.Text = DateTime.Parse(strDate).ToShortDateString()
            End If

            Rctp_TaxCounty_TB.Text = ExtractElementFromSegment("County", gPOSCurrentTaxSegment)
            Rctp_TaxRate_TB.Text = ExtractElementFromSegment("TaxRate", gPOSCurrentTaxSegment) & "%"

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error setting Receipt Header.")
        End Try
    End Sub

    Public Shared Function FillData(buf As String, Size As Integer, Code As String) As String

        Dim lct As Integer
        Try
            lct = Len(buf)

            'prevent crashing
            If lct > Size Then
                lct = Size
            End If

            Select Case Code

                Case "L"

                    buf &= Space(Size - lct)

                Case "R"

                    buf = Space(Size - lct) & buf

                Case "C"

                    buf = Space((Size - lct) / 2) & buf

            End Select
            Return buf

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error.")
            Return buf
        End Try
    End Function

    Private Sub DInput_KeyDown(sender As Object, e As KeyEventArgs) Handles DInput.KeyDown

        Dim ret As Integer
        If e.Key = Key.Return Then
            DInput.Tag = DInput.Text
            ret = ProcessInput()
        End If
    End Sub

    Private Function CalculateInvoice() As Integer

        Dim SubTotal As Double
        Dim Taxes As Double
        Dim TotalSale As Double
        Dim i As Integer
        Dim buf As String
        Dim tbuf As String
        Dim SQL As String = ""
        Dim Segment As String = ""
        Dim InvBalance As Double = 0.0
        Try
            SubTotal = 0
            Taxes = 0
            TotalSale = 0
            For i = 0 To POSLines.Count - 1

                SubTotal += POSLines(i).ExtPrice
                Taxes += POSLines(i).STax
                TotalSale += POSLines(i).LTotal

            Next
            buf = FillData("SubTotal:" & Format(SubTotal, "0.00"), 23, "L")

            ' SRN-444 Fix Tax display glitch
            Dim TaxAdjLen = 23
            'TaxAdjLen -= (Math.Floor(Taxes).ToString.Length - 1) * 2

            tbuf = FillData("Sales Tax: " & Format(Taxes, "0.00"), TaxAdjLen, "R")
            PoleDisplay_Subtotal.Text = buf & tbuf
            PoleDisplay_Total.Text = Format(TotalSale, "$ 0.00")

            gSubTotal = Round(SubTotal, 2)
            gSalesTax = Round(Taxes, 2)
            gGrandTotal = Round(TotalSale, 2)

            If gIsCustomerDisplayEnabled Then
                gCustomerDisplay.UpdatePOS(POSLines)
            End If

            Return 0


        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Calculating Invoice Totals.")
            Return 0
        End Try

    End Function

    Private Sub Memo_Button_Click(sender As Object, e As RoutedEventArgs) Handles Memo_Button.Click
        Try
            ' If SKU Search has content, use as memo... otherwise uses popup
            If DInput.Text = DInput.ToolTip Or DInput.Text = "" Then
                Memo_Popup.IsOpen = True
                memoBox.Focus()
            Else
                ' Use DInput as memo
                AddMemo(DInput.Text)
                DInput.Text = ""
            End If


        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error adding Memo.")
        End Try
    End Sub

    Private Sub ApplyMemo_Click(sender As Object, e As RoutedEventArgs) Handles ApplyMemo.Click
        Try
            AddMemo(New TextRange(memoBox.Document.ContentStart, memoBox.Document.ContentEnd).Text)
            CancelMemo_Click(sender, e)

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error adding Memo.")
        End Try
    End Sub

    Private Sub CancelMemo_Click(sender As Object, e As RoutedEventArgs) Handles CancelMemo.Click
        Try
            memoBox.Document.Blocks.Clear()
            Memo_Popup.IsOpen = False

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error adding Memo.")
        End Try
    End Sub

    Private Sub AddMemo(memo As String)
        Try
            memo = GetWrappedText(memo)

            Dim MemoLines As List(Of String) = memo.Split({Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).ToList

            ' Dump memo to receipt
            For Each line As String In MemoLines
                NoteLineToPOS(line, True)
                Receipt_LB.Items.Refresh()
            Next

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error adding Memo.")
        End Try
    End Sub


    Private Sub CurrentGroup_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles CurrentGroup.SelectionChanged
        Try
            Dim ret As Integer
            If FirstTime = True Then

                Exit Sub

            End If
            'MsgBox(CurrentGroup.SelectedItem)
            If CurrentGroup.SelectedIndex <= 1 Then
                CurrentGroup.Text = "MAIN"
            Else
                CurrentGroup.Text = CurrentGroup.SelectedItem
            End If
            ret = LoadPosButtons()

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error setting POS Button group.")
        End Try
    End Sub

    Private Sub BT_Click(sender As Object, e As RoutedEventArgs) Handles BT0.Click, BT1.Click, BT2.Click, BT3.Click, BT4.Click, BT5.Click, BT6.Click, BT7.Click, BT8.Click, BT9.Click, BT10.Click, BT11.Click, BT12.Click, BT13.Click, BT14.Click, BT15.Click, BT16.Click, BT17.Click, BT18.Click, BT19.Click

        Dim ID As Long = 0
        Dim Type As String = ""
        Dim SKU As String = ""
        Dim ret As Integer
        Dim Qty As String = ""


        Try

            ID = Val(ExtractElementFromSegment("ID", sender.tag))
            Type = ExtractElementFromSegment("Type", sender.tag)
            SKU = ExtractElementFromSegment("SKU", sender.tag)
            Qty = ExtractElementFromSegment("Qty", sender.tag)

            If SKU = "%DISC%" Then
                OverrideDisc.Content = Qty & "%"
                Exit Sub
            End If

            If Not DInput.Text = "" Then

                If IsNumeric(DInput.Text) Then

                    If Val(DInput.Text) < 1000 Then
                        ret = ChangeQuantity()
                    End If
                End If

            End If

            Select Case UCase(Type)

                Case "SKU"

                    If UCase(SKU) = "MAIN" Then

                        CurrentGroup.Text = "Main"
                        ret = LoadPosButtons()
                        DInput.Text = ""
                        DInput.Focus()

                    Else

                        DInput.Text = SKU
                        Check_POSBUtton_Qty(Qty)
                        ret = ProcessInput()

                    End If

                Case "GROUP"

                    CurrentGroup.Text = SKU
                    ret = LoadPosButtons()
                    DInput.Text = ""
                    DInput.Focus()

            End Select

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error processing POS Button selection.")
        End Try
    End Sub

    Private Sub Check_POSBUtton_Qty(ByVal ButtonQty As String)

        If OverrideQuantity.Content = "" Then

            Try
                If ButtonQty = "?" Then
                    Dim response As String
                    response = InputBox("Please Enter Quantiy:")

                    If IsNumeric(response) Then
                        OverrideQuantity.Content = response
                    End If

                ElseIf ButtonQty <> "" Then
                    If IsNumeric(ButtonQty) Then
                        OverrideQuantity.Content = ButtonQty
                    End If
                End If

            Catch ex As Exception
                _MsgBox.ErrorMessage(ex, "Error checking POS Button Quantity.")
            End Try

        End If

    End Sub

    Public Sub EditButton_Click(sender As Object, e As RoutedEventArgs)
        Try

            If gIsPOSSecurityEnabled AndAlso Not Check_Current_User_Permission("Edit_POS_Buttons") Then Exit Sub

            Dim current_button As Button = Get_POS_Button(sender)
            Dim current_group As String = CurrentGroup.Text

            Dim win As New POS_ButtonMaker(Me, current_button, current_group)
            win.ShowDialog(Me)
            If win.IsPosButtonSaved Then
                LoadPosButtons()
            End If

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error editing POS Button.")
        End Try
    End Sub

    Private Sub DeleteButton_Click(sender As Object, e As RoutedEventArgs)
        Try
            If gIsPOSSecurityEnabled AndAlso Not Check_Current_User_Permission("Edit_POS_Buttons") Then Exit Sub

            Dim current_button As Button = Get_POS_Button(sender)

            If Not _MsgBox.QuestionMessage("Are you sure you want to delete the button?", "Deleting Button") Then
                Exit Sub
            End If

            Dim currentButtonID As Long = Val(ExtractElementFromSegment("ID", current_button.Tag))

            If currentButtonID > 0 Then
                Dim SQL As String = "DELETE * FROM PosButtons WHERE [ID] = " & currentButtonID.ToString
                Dim ret As Long = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                If ret > 0 Then
                    LoadPosButtons()
                End If
            End If

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error deleting POS Button.")
        End Try
    End Sub

    Private Sub EditInventoryButton_Click(sender As Object, e As RoutedEventArgs)
        Try
            Dim current_button As Button = Get_POS_Button(sender)
            'MsgBox(ExtractElementFromSegment("SKU", current_button.Tag))

            Dim win As New InventoryDetail(Me,,, ExtractElementFromSegment("SKU", current_button.Tag))
            win.ShowDialog(Me)

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error editing POS Button.")
        End Try
    End Sub

    Private Function Get_POS_Button(sender As Object) As Button
        Try
            'returns POS button that was right clicked on.
            Dim MenuItem As MenuItem = DirectCast(sender, MenuItem)
            Dim parentContextMenu As ContextMenu = DirectCast(MenuItem.CommandParameter, ContextMenu)

            Return parentContextMenu.PlacementTarget

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error with POS Button.")
        End Try
    End Function

    Private Sub SetupOptions_Click(sender As Object, e As RoutedEventArgs) Handles SetupOptions.Click

        POSOptions_Popup.IsOpen = True

    End Sub

    Private Sub Inventory_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Inventory_Btn.Click
        Try
            Dim win As New InventoryManager(Me)
            win.ShowDialog(Me)

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error.")
        End Try
    End Sub

    Private Sub POSReports_Btn_Click(sender As Object, e As RoutedEventArgs) Handles POSReports_Btn.Click
        Try
            Dim win As New ReportsManager(Me)
            win.ShowDialog(Me)

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error.")
        End Try
    End Sub

    Private Sub DOM_Btn_Click(sender As Object, e As RoutedEventArgs) Handles DOM_Btn.Click
        Try

            Call _DropOff.Open_DropOffManager(Me, gCurrentUser, Nothing)

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error.")
        End Try
    End Sub

    Private Sub PackValet_Btn_Click(sender As Object, e As RoutedEventArgs) Handles PackValet_Btn.Click
        Try
            Dim win As New PackageValet(Me)
            win.ShowDialog(Me)

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error.")
        End Try
    End Sub

    Private Sub Lookup_Invoice()


        ' open search form
        Dim SQL As String = ""
        Dim SegmentSet As String = ""
        Dim Segment As String = ""
        Dim ret As Integer = 0
        Dim buf As String = ""
        Dim SKU As String = ""
        Dim sText As String = ""
        Dim sDate As String = ""
        Dim retInvoice As String = ""
        Dim InvBalance As Double = 0.0
        Dim searchstartDate As String
        Dim searchendDate As String

        Dim DaysToShow As Integer

        Try

            DaysToShow = My.Settings.POS_InvoiceHistory_DaysToShow

            If DaysToShow > 9999 Then DaysToShow = 9999

            If DaysToShow < 1 Then DaysToShow = 90

            searchstartDate = Date.Today.AddDays(-DaysToShow).ToString("MM/dd/yyyy")
            searchendDate = Date.Today.ToString("MM/dd/yyyy")

            If InStr(1, gShipriteDB, "$") = 0 Then
                sDate = "[Date] >= #" & searchstartDate & "# AND [Date] <= #" & searchendDate & "#"
            Else
                sDate = "[Date] >= '" & searchstartDate & "' AND [Date] <= '" & searchendDate & "'"
            End If

            buf = ExtractElementFromSegment("ID", gCustomerSegment)
            If buf = "" Then ' no contact loaded
                'SQL = "SELECT InvNum, Format([Date], 'mm/dd/yyyy') as [Date], Format([Time], 'hh:mm AM/PM') as [Time], AcctName, SalesRep, Charge, Balance, Paid FROM Payments WHERE (" & sDate & " AND [Status] = 'Ok' AND (([Type] = 'Sale' AND ([Charge] <> 0 OR [Payment] <> 0)) Or [Type] = 'Refund')) ORDER BY ID DESC"
                SQL = "SELECT InvNum, Format([Date], 'mm/dd/yyyy') as [Date], Format([Time], 'hh:mm AM/PM') as [Time], AcctName, SalesRep, iif([Type] = 'Refund', [SaleAmount], [Charge]) as Charge, Balance, Paid FROM Payments WHERE " & sDate & " AND [Status] = 'Ok' AND (([Charge] <> 0 OR [Payment] <> 0) AND ([Type] = 'Sale' OR [Type] = 'Refund')) <<SEED>> ORDER BY ID DESC"
            Else ' contact loaded
                SQL = "SELECT InvNum, Format([Date], 'mm/dd/yyyy') as [Date], Format([Time], 'hh:mm AM/PM') as [Time], AcctName, SalesRep, iif([Type] = 'Refund', [SaleAmount], [Charge]) as Charge, Balance, Paid FROM Payments WHERE " & sDate & " AND [Status] = 'Ok' AND (([Charge] <> 0 OR [Payment] <> 0) AND ([Type] = 'Sale' OR[Type] = 'Refund')) AND SoldTo = " & buf & " ORDER BY ID DESC"
            End If
            retInvoice = SearchList(Me, sText, "Invoices", "InvNum", "Invoice Search", SQL, "")
            If retInvoice = "" Then

                Exit Sub

            End If
            ret = RecoverInvoice(retInvoice)

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error opening Invoice Lookup.")
        End Try
    End Sub

    Private Sub Lookup_Quote(Optional isQuote = False)

        Dim SQL As String = ""
        Dim SegmentSet As String = ""
        Dim Segment As String = ""
        Dim ret As Long
        Dim buf As String = ""
        Dim SKU As String = ""
        Dim sText As String = ""
        Dim retInvoice As String = ""
        Dim searchDate As String = Date.Today.AddDays(-90).ToString("MM/dd/yyyy") ' show last 90 days of quotes for now

        Try
            If Not POSLines.Count = 0 Then
                'Put Sale on Hold

                If gGrandTotal = 0 Then
                    MsgBox("Sale cannot be $0.00", vbExclamation)
                    Exit Sub
                End If

                If InvoiceType.Content = "Quote" Or InvoiceType.Content = "Hold" Then
                    SQL = "DELETE * FROM Payments WHERE InvNum = '" & gInvoiceNumber.ToString & "'"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                    SQL = "DELETE * FROM Transactions WHERE InvNum = '" & gInvoiceNumber.ToString & "'"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                    InvoiceType.Content = "New Sale"
                ElseIf InvoiceType.Content = "New Sale" Then
                    gInvoiceNumber = GetNextInvoiceNumber().ToString
                End If

                If InvoiceType.Content = "New Sale" Then
                    ' Action from hold button, clear screen
                    ret = PostPayments(IIf(isQuote, "Quote", "Hold"))
                    ret = PostTransactions(IIf(isQuote, "Quote", "Hold"))
                    Save_Invoice_Notes()
                    'ret = PrintReceiptWithLoop() ' Commented out for SRN-122
                    If isQuote Then
                        ' Print out quote
                        Dim rep As New ShipRiteReports._ReportObject
                        rep.ReportName = "Quote.rpt"
                        rep.ReportFormula = "{Transactions.InvNum}='" & gInvoiceNumber & "'"
                        rep.ReportParameters.Add(CreateDisplayBlock(gCustomerSegment, False))
                        Dim reportPrev As New ReportPreview(rep)
                        Cursor = Cursors.Arrow
                        reportPrev.ShowDialog()
                    End If
                    ret = NewSale()
                    Exit Sub
                End If

            Else
                'Display Hold List

                buf = ExtractElementFromSegment("ID", gCustomerSegment)
                If buf = "" Then ' no contact loaded
                    SQL = "SELECT InvNum, Format([Date], 'MM/dd/yyyy') as [Date], Format([Time], 'hh:mm AM/PM') as [Time], AcctName, SalesRep, Charge, Balance FROM Payments WHERE [Date] >= #" & searchDate & "# AND [Status] = '" & IIf(isQuote, "Quote", "Hold") & "' AND (([Type] = 'Sale' AND ([Charge] <> 0 OR [Payment] <> 0)) Or [Type] = 'Refund') ORDER BY ID DESC"
                    ' SQL to POSHold table
                    'SQL = "SELECT InvNum, Format([Date], 'MM/dd/yyyy') as [Date], Format([Time], 'hh:mm AM/PM') as [Time], AcctName, SalesRep, SUM([LTotal]) as NetCharges, [AcctNum] as Paid FROM POSHold WHERE [Date] >= #" & searchDate & "# AND [Status] = '" & IIf(isQuote, "Quote", "Hold") & "' GROUP BY [Date], [Time], [InvNum], [AcctName], [SalesRep], [AcctNum] Order By [Date] DESC, [InvNum] DESC"
                Else ' contact loaded
                    SQL = "SELECT InvNum, Format([Date], 'MM/dd/yyyy') as [Date], Format([Time], 'hh:mm AM/PM') as [Time], AcctName, SalesRep, Charge, Balance FROM Payments WHERE SoldTo = " & buf & " AND [Date] >= #" & searchDate & "# AND [Status] = '" & IIf(isQuote, "Quote", "Hold") & "' AND (([Type] = 'Sale' AND ([Charge] <> 0 OR [Payment] <> 0)) Or [Type] = 'Refund') ORDER BY ID DESC"
                    'SQL = "SELECT InvNum, Format([Date], 'MM/dd/yyyy') as [Date], Format([Time], 'hh:mm AM/PM') as [Time], AcctName, SalesRep, Format([Charge] - [Payment], '$ 0.00') as NetCharges, Paid FROM Payments WHERE [Date] >= #" & searchDate & "# AND [Status] = '" & IIf(isQuote, "Quote", "Hold") & "' Or 'Hold' AND (([Type] = 'Sale' AND ([Charge] <> 0 OR [Payment] <> 0)) Or [Type] = 'Refund') AND SoldTo = " & buf & " ORDER BY ID DESC"
                    ' SQL to POSHold table
                    'SQL = "SELECT InvNum, Format([Date], 'MM/dd/yyyy') as [Date], Format([Time], 'hh:mm AM/PM') as [Time], AcctName, SalesRep, SUM([LTotal]) as NetCharges, [AcctNum] as Paid FROM POSHold WHERE [Date] >= #" & searchDate & "# AND [Status] = '" & IIf(isQuote, "Quote", "Hold") & "' AND SoldTo = " & buf & " GROUP BY [Date], [Time], [InvNum], [AcctName], [SalesRep], [AcctNum] Order By [Date] DESC, [InvNum] DESC"
                End If
                retInvoice = SearchList(Me, sText, "Invoices", "InvNum", "Invoice Search", SQL, "")
            End If
            If retInvoice = "" Then
                Exit Sub
            End If


            ' show in POS
            SQL = ""
            Segment = ""
            SegmentSet = ""
            ret = 0
            SQL = "SELECT * FROM Transactions WHERE InvNum = '" & retInvoice & "'"
            SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
            If SegmentSet = "" Then
                MsgBox("ATTENTION...Invoice [" & retInvoice & "] Transactions information NOT FOUND" & vbCrLf & vbCrLf & "TRY AGAIN", vbCritical)
                Exit Sub
            End If

            POSLines = New ObjectModel.ObservableCollection(Of POSLine) ' reset
            Receipt_LB.ItemsSource = POSLines


            gInvoiceNumber = retInvoice
            buf = ExtractElementFromSegment("SoldTo", SegmentSet)
            If Val(buf) > 0 Then
                SQL = "SELECT * FROM Contacts WHERE ID = " & buf
                gCustomerSegment = IO_GetSegmentSet(gShipriteDB, SQL)
                LoadInForm_Contact(gCustomerSegment)
            End If
            buf = ExtractElementFromSegment("Date", SegmentSet)
            ResetReceiptHeader(buf)
            Do Until SegmentSet = ""
                Segment = GetNextSegmentFromSet(SegmentSet)
                Dim LineItem As New POSLine()
                LineItem.ID = ExtractElementFromSegment("ID", Segment)                       ' New POS Line 0 = new sale, non 0 is recovered invoice do update
                LineItem.SKU = ExtractElementFromSegment("SKU", Segment)
                LineItem.ModelNumber = ExtractElementFromSegment("ModelNumber", Segment)
                LineItem.Description = ExtractElementFromSegment("Desc", Segment)
                LineItem.Department = ExtractElementFromSegment("Dept", Segment)
                LineItem.UnitPrice = Val(ExtractElementFromSegment("UnitPrice", Segment))
                LineItem.Quantity = Val(ExtractElementFromSegment("Qty", Segment))
                LineItem.ExtPrice = Val(ExtractElementFromSegment("ExtPrice", Segment)) ' LineItem.UnitPrice * LineItem.Quantity
                LineItem.COGS = Val(ExtractElementFromSegment("COGS", Segment))
                LineItem.STax = Val(ExtractElementFromSegment("STax", Segment)) ' (gPOSHeader.TaxRate / 100) * LineItem.ExtPrice
                LineItem.LTotal = Val(ExtractElementFromSegment("LTotal", Segment)) ' LineItem.ExtPrice + LineItem.STax
                LineItem.TaxCounty = ExtractElementFromSegment("TCounty", Segment) ' gPOSHeader.TaxCounty
                LineItem.TRate = Val(ExtractElementFromSegment("TRate", Segment)) ' gPOSHeader.TaxRate
                LineItem.BrandName = ExtractElementFromSegment("Brand", Segment)
                LineItem.Category = ExtractElementFromSegment("Category", Segment)
                LineItem.AcctName = ExtractElementFromSegment("AcctName", Segment)
                LineItem.AcctNum = ExtractElementFromSegment("AcctNum", Segment)
                LineItem.SoldToID = Val(ExtractElementFromSegment("SoldTo", Segment)) '  Val(ExtractElementFromSegment("ID", gCustomerSegment))
                LineItem.ShipToID = Val(ExtractElementFromSegment("ShipTo", Segment)) ' Val(ExtractElementFromSegment("ID", gShipToCustomerSegment))
                LineItem.PackageID = ExtractElementFromSegment("PackageID", Segment)
                POSLines.Add(LineItem)
            Loop

            ret = CalculateInvoice()
            Receipt_LB.Items.Refresh()
            CheckInvoiceNotes()

            SaleOptions_Btn.Visibility = Visibility.Visible



        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error looking up Quote.")
        End Try

        InvoiceType.Content = IIf(isQuote, "Quote", "Hold")
    End Sub

    Private Sub Lookup_Contact()
        Try

            If Not Is_NewSale_Quote_Hold(InvoiceType.Content) Then
                If ExtractElementFromSegment("ID", gCustomerSegment, "") = "" Then
                    'for recovered invoice, don't allow adding of customer, but it should allow editing of customer.
                    Exit Sub
                End If
            End If

            gResult = ""
            gAutoExitFromContacts = True


            If ExtractElementFromSegment("ID", gCustomerSegment, "") = "" Then
                Dim win As New ContactManager(Me)
                win.ShowDialog(Me)
            Else
                Dim win As New ContactManager(Me, ExtractElementFromSegment("ID", gCustomerSegment, "0"))
                win.ShowDialog(Me)
            End If

            If gContactManagerSegment <> "" Then
                gCustomerSegment = gContactManagerSegment
                LoadInForm_Contact(gCustomerSegment)
            End If

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error looking up Quote.")
        End Try

    End Sub

    Private Sub Lookup_Account()

        Dim SQL As String = ""
        Dim AR As String = ""
        Dim ret As Long = 0

        Try
            gResult = ExtractElementFromSegment("AR", gCustomerSegment)
            If gResult = "" Then

                AR = MakeCustomerAccountFromContact(gCustomerSegment)
                AccountNo_TxtBox.Content = AR

            End If
            gResult2 = "AUTOEXIT"
            gResult3 = ""
            Try

                Dim win As New AccountManager(Me)
                win.ShowDialog(Me)

                'If Not gResult = "" Then

                '    gCustomerSegment = gContactManagerSegment
                '    LoadInForm_Contact(gCustomerSegment)

                'End If

            Catch ex As Exception

                MessageBox.Show(Err.Description)

            End Try
            gResult = ""
            gResult2 = ""
            If Not gResult3 = "" Then

                ret = RecoverInvoice(gResult3)
                gResult3 = ""

            End If
            ContactDropDown_Popup.IsOpen = False

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error looking up Account.")
        End Try
    End Sub

    Private Sub LoadInForm_Contact(ByVal CustomerSegment As String)

        Dim SQL As String = ""
        Dim SegmentSet As String = ""
        Dim Balance As Double = 0
        Dim AR_TaxCounty As String = ""
        Dim TaxRate As String = ""
        Dim Discount As Double = 0

        Try
            If String.IsNullOrWhiteSpace(CustomerSegment) Then Exit Sub

            CustomerName.Content = ExtractElementFromSegment("Name", CustomerSegment)
            AccountNo_TxtBox.Content = ExtractElementFromSegment("AR", CustomerSegment)
            Balance_TxtBox.Content = ""

            'check if customer is AR account
            If AccountNo_TxtBox.Content <> "" And AccountNo_TxtBox.Content <> "CASH" Then
                SQL = "SELECT SUM(Charge) AS Charges, SUM(Payment) AS PAID FROM Payments WHERE AcctNum = '" & AccountNo_TxtBox.Content & "' AND Status = 'Ok'"
                SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
                Balance = Val(ExtractElementFromSegment("Charges", SegmentSet)) - Val(ExtractElementFromSegment("PAID", SegmentSet))
                Balance_TxtBox.Content = Format(Balance, "$ 0.00")

                SQL = "SELECT TaxCountyDB, Discount FROM AR WHERE AcctNum = '" & AccountNo_TxtBox.Content & "'"
                SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)

                AR_TaxCounty = ExtractElementFromSegment("TaxCountyDB", SegmentSet, "")

                If AR_TaxCounty <> "" And AR_TaxCounty <> gDefaultTaxCounty Then

                    TaxRate = ExtractElementFromSegment("TaxRate", IO_GetSegmentSet(gShipriteDB, "Select TaxRate FROM CountyTaxes WHERE County='" & AR_TaxCounty & "'"), "")

                    If TaxRate <> "" Then
                        gPOSHeader.TaxRate = TaxRate
                        gPOSHeader.TaxCounty = AR_TaxCounty

                        Rctp_TaxCounty_TB.Text = gPOSHeader.TaxCounty
                        Rctp_TaxRate_TB.Text = gPOSHeader.TaxRate
                    End If
                End If

                Discount = ExtractElementFromSegment("Discount", SegmentSet, "0")
                If Discount <> 0 Then
                    OverrideDisc.Content = Discount.ToString & "%"
                End If

            End If


            'display mailbox info
            If ExtractElementFromSegment("MBX", CustomerSegment, "False") = True Then
                SQL = "SELECT MailBox.MailboxNumber, MailBox.EndDate
From MBXNamesList INNER Join MailBox On MBXNamesList.MBX = MailBox.MailboxNumber
Where MBXNamesList.CID = " & ExtractElementFromSegment("ID", CustomerSegment)
                SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)

                If SegmentSet <> "" Then
                    Mbx_ExpDate_TxtBx.Content = FormatDateTime(ExtractElementFromSegment("EndDate", SegmentSet), DateFormat.ShortDate)
                    MBX_No_TxtBx.Content = ExtractElementFromSegment("MailboxNumber", SegmentSet)
                End If
            Else
                Mbx_ExpDate_TxtBx.Content = ""
                MBX_No_TxtBx.Content = ""
            End If

            'CustomerAddressBlock.Content = ExtractElementFromSegment("Name", CustomerSegment) & vbCrLf & ExtractElementFromSegment("Addr1", CustomerSegment) & vbCrLf & ExtractElementFromSegment("Addr2", CustomerSegment) & vbCrLf & ExtractElementFromSegment("City", CustomerSegment) & ", " & ExtractElementFromSegment("State", CustomerSegment) & "  " & ExtractElementFromSegment("ZipCode", CustomerSegment) & vbCrLf & ExtractElementFromSegment("Phone", CustomerSegment)
            CustomerAddressBlock.Content = CreateDisplayBlock(CustomerSegment, True)

            If ContactManager.Display_Customer_Notes(ExtractElementFromSegment("ID", CustomerSegment), CustomerNote_TxtBx) Then
                CustomerName.Foreground = New System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Yellow)
                'ContactDropDown_Popup.IsOpen = True
            Else
                CustomerName.Foreground = New System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White)
            End If

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error loading Customer Info.")
        End Try
    End Sub

    Private Function PrintReceiptDetermineNoOfCopies() As Integer

        Dim NetCopies As Integer = 1
        Dim Copies As Integer = 0

        Try

            For Each item As PaymentDefinition In gPM.NewPayments

                Select Case UCase(item.Type)

                    Case "CASH"

                        Copies = Val(ExtractElementFromSegment("PrintTotalCash", ReceiptOptionsSegment))

                    Case "CHARGE"

                        Copies = Val(ExtractElementFromSegment("PrintTotalCC", ReceiptOptionsSegment))

                    Case "CHECK"

                        Copies = Val(ExtractElementFromSegment("PrintTotalCheck", ReceiptOptionsSegment))

                    Case "OTHER"

                        Copies = Val(ExtractElementFromSegment("PrintTotalOther", ReceiptOptionsSegment))

                End Select

                If Copies > NetCopies Then
                    NetCopies = Copies
                End If

            Next

            If gGrandTotal > 0 And gPM.NewPayments.Count = 0 Then
                'invoice on account
                Copies = Val(ExtractElementFromSegment("PrintTotalAccount", ReceiptOptionsSegment))

                If Copies > NetCopies Then
                    NetCopies = Copies
                End If

            End If

            If PrintShippingDisclaimer_On_2nd_Receipt() Then
                NetCopies = 2
            End If

            Return NetCopies

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error determining number of receipt copies to print.")
            Return 1
        End Try
    End Function

    Private Function PrintShippingDisclaimer_On_2nd_Receipt() As Boolean
        If ExtractElementFromSegment("EnableShippingDisclaimer", ReceiptOptionsSegment, "True") = True Then
            If ExtractElementFromSegment("ShippingDisclaimer_2ndReceipt", ReceiptOptionsSegment, "True") = True Then
                For Each line As POSLine In POSLines
                    If Not String.IsNullOrEmpty(line.PackageID) Then
                        Return True
                    End If
                Next
            End If
        End If

        Return False

    End Function


    Private Function PrintReceiptWithLoop() As Integer

        Dim i As Integer
        Dim ret As Integer
        Dim LoopCT As Integer
        Try
            LoopCT = PrintReceiptDetermineNoOfCopies()
            For i = 1 To LoopCT

                If i = 1 Then
                    ret = PrintReceipt()
                Else
                    '2nd duplicate receipt
                    ret = PrintReceipt(True)
                End If


            Next

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error.")
        End Try


        Return 0

    End Function

    Private Function PrintCashPaidOutReceipt() As Integer

        Dim buf As String = ""
        Dim receiptToPrint As String = ""
        Dim bufPayments As String = ""
        Dim bufCredits As String = ""
        Dim bufChange As String = ""
        Dim lineCT As Integer = DefaultLineCT
        Dim ThereAreShipments As Boolean = False
        Dim ReceiptSignatureText As String = ""
        Dim ShippingDisclaimer As String = ""
        Try
            PrepareReceiptHeader(receiptToPrint, True)
            lineCT = DefaultLineCT

            receiptToPrint &= FillData("    Time: " & Now.ToShortTimeString, lineCT, "L") & vbCrLf & vbCrLf


            receiptToPrint &= "======================================" & vbCrLf
            receiptToPrint &= "======================================" & vbCrLf & vbCrLf
            receiptToPrint &= "****CASH PAID OUT***CASH PAID OUT**** " & vbCrLf & vbCrLf
            receiptToPrint &= "======================================" & vbCrLf
            receiptToPrint &= "======================================" & vbCrLf
            receiptToPrint &= " " & vbCrLf
            receiptToPrint &= " " & vbCrLf
            receiptToPrint &= CashPaidOut_Purpose_TxtBx.Text & vbCrLf & vbCrLf & vbCrLf
            receiptToPrint &= "x_____________________________________" & vbCrLf
            receiptToPrint &= gCurrentUser & vbCrLf
            receiptToPrint &= " " & vbCrLf
            receiptToPrint &= " " & vbCrLf
            buf = Format(Val(CashPaidOut_Amt_TxtBx.Text), "$ 0.00")
            buf = FillData(buf, lineCT, "C")
            receiptToPrint &= buf

            If receiptToPrint.Trim.Length > 0 Then

                Dim pName As String = GetPolicyData(gReportsDB, "InvoicePrinter")
                If pName = "" Then
                    pName = _Printers.Get_DefaultPrinter()
                End If
                Dim pSettings As New PrintHelper
                pSettings.PrintFontFamilyName = GetPolicyData(gReportsDB, "InvoiceFont")
                pSettings.PrintFontSize = GetPolicyData(gReportsDB, "FontSize")
                pSettings.PrintJobName = "ShipRite Receipt - " & gInvoiceNumber
                pSettings.PrintFontStyle = Drawing.FontStyle.Bold
                pSettings.FireDrawerCode = GetPolicyData(gReportsDB, "InvoiceDrawer")

                _PrintReceipt.Print_FromText(receiptToPrint, pName, pSettings)

            End If

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Printing Cash Paid Out Receipt.")
        End Try
        Return 0

    End Function

    Private Function PrintReceipt(Optional isDuplicateReceipt As Boolean = False, Optional DontPrint As Boolean = False) As Integer

        Try

            Dim buf As String = ""
            Dim receiptToPrint As String = ""
            Dim bufPayments As String = ""
            Dim bufCredits As String = ""
            Dim bufChange As String = ""
            Dim lineCT As Integer = DefaultLineCT
            Dim ThereAreShipments As Boolean = False
            Dim ReceiptSignatureText As String = ""
            Dim ShippingDisclaimer As String = ""
            Dim skuLineCT As Integer = Fix(DefaultLineCT * 0.4)
            Dim pricingLineCT As Integer = DefaultLineCT - skuLineCT
            Dim FoundShippingService As Boolean = False
            Dim i As Integer = 0
            Dim j As Integer = 0
            Dim ReceiptLogo As BitmapImage = Nothing


            PrepareReceiptHeader(receiptToPrint)

            For Each line As POSLine In POSLines
                receiptToPrint &= FillData(line.Description, lineCT, "L") & vbCrLf
                Debug.Print(line.PackageID)

                If Not isNoteORMemo(line.SKU) Then
                    lineCT = Fix(pricingLineCT / 3)
                    receiptToPrint &= FillData(Mid(line.SKU, 0, skuLineCT - 1), skuLineCT, "L")
                    receiptToPrint &= FillData(line.UnitPrice.ToString("0.00"), lineCT + 1, "R")
                    receiptToPrint &= FillData(Math.Round(line.Quantity, 3), lineCT - 1, "R")
                    receiptToPrint &= FillData(line.ExtPrice.ToString("0.00"), lineCT + 2, "R") & vbCrLf

                    lineCT = DefaultLineCT

                    If FoundShippingService = False And Not String.IsNullOrEmpty(line.PackageID) Then
                        FoundShippingService = True
                    End If

                End If

            Next

            lineCT = DefaultLineCT

            If Not gResult = "GIFT CARD IN EFFECT" And Not gPM.isBulkPayment Then
                receiptToPrint &= "______________________________________" & vbCrLf
                receiptToPrint &= "______________________________________" & vbCrLf
                receiptToPrint &= FillData("SubTotal: " & Format(gSubTotal, "$0.00"), lineCT, "R") & vbCrLf
                receiptToPrint &= FillData("Sales Tax: " & Format(gSalesTax, "$0.00"), lineCT, "R") & vbCrLf
                receiptToPrint &= FillData("----------", lineCT, "R") & vbCrLf
                receiptToPrint &= FillData("Total: " & Format(gGrandTotal, "$0.00"), lineCT, "R") & vbCrLf
            End If


            receiptToPrint &= vbCrLf & vbCrLf

            If gGrandTotal >= 0 Then
                For Each item As PaymentDefinition In gPM.NewPayments
                    If item.Type IsNot Nothing Then
                        If Not item.Type.ToLower = "sale" Then
                            If item.Payment = 0 Then
                                If item.Type.ToLower = "change" Then
                                    bufChange = FillData("Change Due: " & Format(item.Charge, "$0.00"), lineCT, "R") & vbCrLf
                                Else
                                    bufCredits &= FillData("Credits: " & Format(item.Charge, "$0.00"), lineCT, "R") & vbCrLf
                                End If
                            ElseIf item.Payment > 0 Then
                                If Not gPM.isBulkPayment Then
                                    bufPayments &= FillData(item.Type & " Tendered: " & Format(item.Payment, "$0.00"), lineCT, "R") & vbCrLf
                                Else
                                    bufPayments &= FillData(" Invoice#: " & item.InvNum, lineCT / 2, "L")
                                    bufPayments &= FillData(Format(item.Payment, "$0.00"), lineCT / 2, "R") & vbCrLf
                                End If
                            End If
                        End If
                    End If
                Next
            Else
                'REFUND
                'if refund on AR account, then there are no gPm.NewPayments entries.
                If gPM.NewPayments.Count <> 0 Then
                    bufPayments &= FillData(gPM.NewPayments(0).Desc & "  " & FormatCurrency(gGrandTotal), lineCT, "C") & vbCrLf
                End If
            End If


            If bufPayments.Trim.Length > 0 Then
                receiptToPrint &= bufPayments
            End If

            If gPM.isBulkPayment Then
                receiptToPrint &= vbCrLf & vbCrLf & FillData(gPM.NewPayments(0).PaymentDisplay, lineCT, "R")
                receiptToPrint &= vbCrLf & FillData("Total: " & FormatCurrency(Get_BulkPayment_Total()), lineCT, "R") & vbCrLf
            End If

            If bufCredits.Trim.Length > 0 Then
                receiptToPrint &= bufCredits
            End If

            If bufChange.Trim.Length > 0 Then
                receiptToPrint &= bufChange
            End If
            receiptToPrint &= vbCrLf

            If Not gReceiptCCEndBlock = "" And Not gResult = "GIFT CARD IN EFFECT" Then
                receiptToPrint &= gReceiptCCEndBlock

            End If


            'GIFT CARD
            If gResult = "GIFT CARD IN EFFECT" Then

                receiptToPrint &= vbCrLf & vbCrLf & "* * * *    GIFT CARD PURCHASE   * * * *"
                receiptToPrint &= vbCrLf & vbCrLf & "* * * *    GIFT CARD PURCHASE   * * * *"
                receiptToPrint &= vbCrLf & vbCrLf & "* * * *    GIFT CARD PURCHASE   * * * *" & vbCrLf & vbCrLf

            End If

            'SHIPPING DISCLAIMER
            buf = ExtractElementFromSegment("EnableShippingDisclaimer", ReceiptOptionsSegment)
            If buf = "True" And Not gResult = "GIFT CARD In EFFECT" And FoundShippingService = True Then

                ShippingDisclaimer = ExtractElementFromSegment("ShippingDisclaimer", ReceiptOptionsSegment)

                If Not ShippingDisclaimer = "" Then

                    ShippingDisclaimer = GetWrappedText(ShippingDisclaimer)

                    If ExtractElementFromSegment("ShippingDisclaimer_2ndReceipt", ReceiptOptionsSegment, "True") Then
                        If isDuplicateReceipt = True Then
                            receiptToPrint &= vbCrLf & vbCrLf & ShippingDisclaimer
                            'Add_ShippingDisclaimer_ToReceipt(receiptToPrint, ShippingDisclaimer)
                        End If

                    Else
                        receiptToPrint &= vbCrLf & vbCrLf & ShippingDisclaimer
                        'Add_ShippingDisclaimer_ToReceipt(receiptToPrint, ShippingDisclaimer)
                    End If
                End If
            End If


            'RECEIPT SIGNATURE
            ReceiptSignatureText = ExtractElementFromSegment("ReceiptSignatureText", ReceiptOptionsSegment)
            If Not ReceiptSignatureText = "" And Not gResult = "GIFT CARD In EFFECT" Then

                receiptToPrint &= vbCrLf & GetWrappedText(ReceiptSignatureText)

            End If


            If GetPolicyData(gShipriteDB, "ReceiptLink1", "") <> "" Then
                receiptToPrint &= vbCrLf & vbCrLf & "Find us on Facebook:" & vbCrLf & GetPolicyData(gShipriteDB, "ReceiptLink1", "")
            End If

            If GetPolicyData(gShipriteDB, "ReceiptLink2", "") <> "" Then
                receiptToPrint &= vbCrLf & vbCrLf & "Find us on X:" & vbCrLf & GetPolicyData(gShipriteDB, "ReceiptLink2", "")
            End If

            If GetPolicyData(gShipriteDB, "ReceiptLink3", "") <> "" Then
                receiptToPrint &= vbCrLf & vbCrLf & "Take our survey:" & vbCrLf & GetPolicyData(gShipriteDB, "ReceiptLink3", "")
            End If


            If receiptToPrint.Trim.Length > 0 Then

                'set printer settings.-----------------------------------------------------------------
                Dim pName As String = GetPolicyData(gReportsDB, "InvoicePrinter")
                If pName = "" Then
                    pName = _Printers.Get_DefaultPrinter()
                End If
                Dim pSettings As New PrintHelper
                pSettings.PrintFontFamilyName = GetPolicyData(gReportsDB, "InvoiceFont", "Consolas")
                pSettings.PrintFontSize = GetPolicyData(gReportsDB, "FontSize", "9")
                pSettings.PrintJobName = "ShipRite Receipt - " & gInvoiceNumber
                pSettings.PrintFontStyle = Drawing.FontStyle.Bold
                pSettings.FireDrawerCode = GetPolicyData(gReportsDB, "InvoiceDrawer")
                '---------------------------------------------------------------------------------------

                If Not DontPrint Then

                    Dim LogoPath As String = GetReceiptLogo()

                    If LogoPath <> "" Then
                        _PrintReceipt.Print_FromTextAndImage(receiptToPrint, pName, LogoPath, pSettings)
                    Else
                        _PrintReceipt.Print_FromText(receiptToPrint, pName, pSettings)

                    End If


                End If


                If gPOS_EmailReceipt.isEmail Then
                    gPOS_EmailReceipt.EmailBody = receiptToPrint
                End If

            End If
            Return 0

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Printing Receipt.")
            Return 1
        End Try

    End Function

    Public Function GetWrappedText(textToWrap As String) As String
        Dim wrappedText As New System.Text.StringBuilder()
        Dim words As String() = textToWrap.Split(" "c)
        Dim line As String = ""
        Dim fontName As String = GetPolicyData(gReportsDB, "InvoiceFont", "Consolas")
        Dim fontSize As Single = GetPolicyData(gReportsDB, "FontSize", "9")
        Dim paperWidth As Integer = 275

        Dim graphics As Graphics = Graphics.FromImage(New Bitmap(1, 1))
        Dim wrapFont As New Font(fontName, fontSize)

        For Each word In words
            Dim testLine As String = If(line.Length > 0, line & " " & word, word)
            Dim size As SizeF = graphics.MeasureString(testLine, wrapFont)

            If size.Width <= paperWidth Then
                line = testLine
            Else
                wrappedText.AppendLine(line)
                line = word
            End If
        Next

        If line.Length > 0 Then
            wrappedText.AppendLine(line)
        End If

        Return wrappedText.ToString()
    End Function



    Private Function GetReceiptLogo() As String
        Create_Folder(gDBpath & "\Ads\ReceiptLogo", False)
        Dim FileList = System.IO.Directory.GetFiles(gDBpath & "\Ads\ReceiptLogo").ToList

        If FileList.Count <> 0 Then
            Return FileList(0)
        End If

        Return ""
    End Function

    Private Sub PrepareReceiptHeader(ByRef ReceiptToPrint As String, Optional HideInvoiceHeader As Boolean = False)
        Try
            Dim LineCT As Integer
            Dim buf As String

            LineCT = DefaultLineCT


            ReceiptToPrint &= FillData(GetPolicyData(gShipriteDB, "Name"), LineCT, "C") & vbCrLf
            ReceiptToPrint &= FillData(GetPolicyData(gShipriteDB, "Addr1"), LineCT, "C") & vbCrLf

            If GetPolicyData(gShipriteDB, "Addr2", "") <> "" Then
                ReceiptToPrint &= FillData(GetPolicyData(gShipriteDB, "Addr2"), LineCT, "C") & vbCrLf
            End If

            ReceiptToPrint &= FillData(GetPolicyData(gShipriteDB, "City") & ", " & GetPolicyData(gShipriteDB, "State") & "   " & GetPolicyData(gShipriteDB, "Zip"), LineCT, "C") & vbCrLf
            ReceiptToPrint &= FillData(GetPolicyData(gShipriteDB, "Phone1"), LineCT, "C") & vbCrLf
            ReceiptToPrint &= FillData(ExtractElementFromSegment("County", gPOSCurrentTaxSegment) & "     TaxRate " & ExtractElementFromSegment("TaxRate", gPOSCurrentTaxSegment) & "%", LineCT, "C") & vbCrLf
            ReceiptToPrint &= vbCrLf

            LineCT = Fix(DefaultLineCT / 2)

            If Not gPM.isBulkPayment Then
                If HideInvoiceHeader = False Then
                    If InvoiceType.Content = "Recovered Invoice" Then
                        ReceiptToPrint &= FillData("Invoice#(R) " & gInvoiceNumber, LineCT, "L")
                    Else
                        ReceiptToPrint &= FillData("Invoice# " & gInvoiceNumber, LineCT, "L")
                    End If
                End If
            End If

            If Clerk_Lbl.Content <> "" Then
                ReceiptToPrint &= FillData("Clerk: " & Clerk_Lbl.Content, LineCT, "R")
            End If
            ReceiptToPrint &= vbCrLf

            ReceiptToPrint &= FillData("Date: " & Rctp_Date_TB.Text, LineCT, "L") & vbCrLf

            If gCustomerSegment <> "" And ExtractElementFromSegment("AR", gCustomerSegment) <> "CASH" Then
                ReceiptToPrint &= vbCrLf & FillData(ExtractElementFromSegment("Name", gCustomerSegment, ""), LineCT, "L") & vbCrLf
                If ExtractElementFromSegment("AR", gCustomerSegment, "") <> "" And ExtractElementFromSegment("AR", gCustomerSegment, "") <> "CASH" Then
                    ReceiptToPrint &= "Acct# " & FillData(ExtractElementFromSegment("AR", gCustomerSegment, ""), LineCT, "L") & vbCrLf
                End If
            End If

            If gGrandTotal < 0 Then
                ReceiptToPrint &= vbCrLf & vbCrLf & FillData("* * *    REFUND    * * *", LineCT * 2, "C") & vbCrLf & vbCrLf
            End If

            If HideInvoiceHeader = False Then
                LineCT = DefaultLineCT
                ReceiptToPrint &= (Strings.StrDup(LineCT, "_")) & vbCrLf

                If Not gPM.isBulkPayment Then
                    ReceiptToPrint &= (FillData("Description", LineCT, "L")) & vbCrLf

                    Dim skuLineCT As Integer = Fix(DefaultLineCT * 0.4)
                    Dim pricingLineCT As Integer = DefaultLineCT - skuLineCT
                    LineCT = Fix(pricingLineCT / 3)
                    Dim lineEndAdj As Integer = Math.Abs(pricingLineCT - (LineCT * 3))
                    buf = FillData("SKU#", skuLineCT, "L") & FillData("Price", LineCT, "R") & FillData("Qty", LineCT, "R") & FillData("ExtPrice", LineCT + lineEndAdj, "R") & vbCrLf
                    ReceiptToPrint &= buf

                    LineCT = DefaultLineCT
                    ReceiptToPrint &= Strings.StrDup(LineCT, "_") & vbCrLf
                End If
            End If


            If gPM.isBulkPayment Then
                ReceiptToPrint &= vbCrLf & FillData("Account Payment", LineCT, "C") & vbCrLf
                ReceiptToPrint &= Strings.StrDup(LineCT, "_") & vbCrLf & vbCrLf
                ReceiptToPrint &= (FillData("Acct#" & ExtractElementFromSegment("AR", gCustomerSegment, ""), LineCT, "L")) & vbCrLf
                ReceiptToPrint &= (FillData("Name" & ExtractElementFromSegment("Name", gCustomerSegment, ""), LineCT, "L")) & vbCrLf
            End If

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Setting Receipt Header.")
        End Try

    End Sub

    Public Function Get_BulkPayment_Total() As Double

        Dim total As Double = 0

        Try
            For Each item In gPM.NewPayments
                total += item.Payment
            Next


        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error.")
        End Try

        Return total
    End Function

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub

    Private Sub ChangePrice_Button_Click(sender As Object, e As RoutedEventArgs) Handles ChangePrice_Button.Click
        Try
            If DInput.Text = "" Then
                If Receipt_LB.SelectedIndex >= 0 AndAlso Not Receipt_LB.SelectedItem Is Nothing AndAlso Not isNoteORMemo(Receipt_LB.SelectedItem.SKU) Then
                    OpenLineEditor()
                Else
                    OverridePrice.Content = ""
                End If
                DInput.Focus()
            ElseIf Not IsNumeric(DInput.Text) Then
                MsgBox("WARNING... Input Is Not Numeric")
                DInput.Focus()
            Else
                OverridePrice.Content = Format(Val(DInput.Text), "$ 0.00")
                DInput.Text = ""
                DInput.Focus()
                Try

                Catch ex As Exception
                    MessageBox.Show(Err.Description)
                End Try
            End If

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Changing Price.")
        End Try
    End Sub

    Private Sub ItemDiscount_Button_Click(sender As Object, e As RoutedEventArgs) Handles ItemDiscount_Button.Click
        Try

            If DInput.Text = "" Then

                OverrideDisc.Content = ""
                DInput.Focus()
                Exit Sub

            End If
            If Not IsNumeric(DInput.Text) Then

                MsgBox("WARNING... Input Is Not Numeric")
                DInput.Focus()
                Exit Sub

            End If
            OverrideDisc.Content = DInput.Text & "%"
            DInput.Text = ""
            DInput.Focus()

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Discounting Sales Item.")
        End Try
    End Sub

    Private Sub ChangeTax_Button_Click(sender As Object, e As RoutedEventArgs) Handles ChangeTax_Button.Click

        Dim buf As String
        Dim ret As Integer
        Dim County As String
        Dim TaxRate As Double

        County = ""
        TaxRate = 0
        ret = 0
        Try


            buf = SearchList(Me, "", "Taxes", "InvNum", "Lookup Tax County", "Select County, State, TaxRate, ID FROM CountyTaxes", "")
            If Not buf = "" Then

                gPOSHeader.TaxCounty = GetRunTimePolicy(gSEARCH, "SECONDRESULT")
                gPOSHeader.TaxRate = GetRunTimePolicy(gSEARCH, "THIRDRESULT")

                Rctp_TaxCounty_TB.Text = gPOSHeader.TaxCounty
                Rctp_TaxRate_TB.Text = gPOSHeader.TaxRate

            End If

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Changing Tax County.")
        End Try
    End Sub

    Private Sub Quantity_Button_Click(sender As Object, e As RoutedEventArgs) Handles Quantity_Button.Click

        Dim ret As Integer
        ret = ChangeQuantity()

    End Sub

    Private Sub ContactOptions_Click(sender As Object, e As RoutedEventArgs) Handles ContactOptions.Click
        Try
            Dim ret As Long = 0
            If ExtractElementFromSegment("AR", gCustomerSegment) = "CASH" Then

                Call Lookup_Contact()
                Exit Sub

            End If
            If ContactDropDown_Popup.IsOpen Then
                ContactDropDown_Popup.IsOpen = False
            Else
                ContactDropDown_Popup.IsOpen = True
            End If

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error.")
        End Try
    End Sub

    Private Sub ContactAddressUpdate(sender As Object, e As RoutedEventArgs) Handles AddressUpdate_Btn.Click

        Call Lookup_Contact()
        ContactDropDown_Popup.IsOpen = False

    End Sub

    Private Sub AR_EditorUpdate(sender As Object, e As RoutedEventArgs) Handles AR_Edit_Btn.Click
        Try
            ContactDropDown_Popup.IsOpen = False

            If (gIsProgramSecurityEnabled Or gIsPOSSecurityEnabled) AndAlso Not Check_Current_User_Permission("AR_CreateAccounts") Then
                Exit Sub
            End If

            If AccountNo_TxtBox.Content = "" Then
                If MsgBox("Are you sure you wish To create an AR account For this customer?", 4, "POS Manager") = MsgBoxResult.Yes Then
                    Lookup_Account()
                End If

            Else
                Lookup_Account()
            End If

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error.")
        End Try

    End Sub

    Private Sub CloseDrawer_Btn_Click(sender As Object, e As RoutedEventArgs) Handles CloseDrawer_Btn.Click
        Try

            If gIsPOSSecurityEnabled AndAlso Not Check_Current_User_Permission("POSManager") Then Exit Sub

            gResult = "Close"


            Dim win As New POS_OpenClose(Me)
            win.ShowDialog(Me)
            If gResult = "CLOSED" Then

                gResult = ""
                Me.Close()
                Exit Sub

            End If
            gResult = ""


        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Closing Drawer.")
        End Try
    End Sub

    Private Sub AddTickler_Btn_Click(sender As Object, e As RoutedEventArgs) Handles AddTickler_Btn.Click
        Try


            If ExtractElementFromSegment("Name", gCustomerSegment) = "Cash, Check, Charge" Then
                Dim win As New Tickler(Me)
                win.ShowDialog(Me)
            Else
                Dim win As New Tickler(Me, gCustomerSegment)
                win.ShowDialog(Me)
            End If


        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error.")
        End Try
        ContactDropDown_Popup.IsOpen = False

    End Sub

    Private Sub SaveInvNumNotes(sender As Object, e As RoutedEventArgs) Handles SaveNote_Btn.Click
        Save_Invoice_Notes()
    End Sub

    Private Sub DeleteTheNote(sender As Object, e As RoutedEventArgs) Handles DeleteNote_Btn.Click
        Try
            Dim SQL As String = ""
            Dim ret As Integer = 0
            InvoiceNote.Text = ""
            SQL = "DELETE * FROM InvoiceNotes WHERE InvNum = " & gInvoiceNumber
            ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
            ContactDropDown_Popup.IsOpen = False

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Deleting Note.")
        End Try
    End Sub

    Private Sub Save_Invoice_Notes()
        Try
            Dim SQL As String = ""
            Dim Segment As String = ""
            Dim SegmentSet As String = ""
            Dim ID As String = ""
            Dim ret As Long
            Dim buf As String

            If gInvoiceNumber = "" Or gInvoiceNumber = "NewSale" Then
                POSOptions_Popup.IsOpen = False
                Exit Sub
            End If

            If Not InvoiceNote.Text = "" Then

                buf = InvoiceNote.Text
                buf = FlushOut(buf, "'", "~")
                buf = FlushOut(buf, "~", "''")
                SQL = "SELECT ID FROM InvoiceNotes WHERE InvNum = " & gInvoiceNumber
                SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)

                If Not SegmentSet = "" Then

                    ID = ExtractElementFromSegment("ID", SegmentSet)
                    SQL = "UPDATE InvoiceNotes SET [Note] = '" & buf & "' WHERE ID=" & ID
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                Else

                    SQL = "SELECT MAX(ID) AS MaxID FROM InvoiceNotes"
                    SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
                    ID = Val(ExtractElementFromSegment("MaxID", SegmentSet)) + 1
                    SQL = "INSERT INTO InvoiceNotes ([ID], [InvNum], [Note]) VALUES (" & ID & ", '" & gInvoiceNumber & "', '" & buf & "')"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                End If
                ContactDropDown_Popup.IsOpen = False
                If ret = 0 Then

                    MsgBox("ATTENTION...Notes Update Failed!", vbCritical, gProgramName)
                Else
                    'MsgBox("Invoice Notes for Invoice " & gInvoiceNumber & " updated!", vbInformation)

                End If
            End If

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Saving Invoice Notes.")
        End Try

        POSOptions_Popup.IsOpen = False
    End Sub

    Private Function isNoteORMemo(SKU As String) As Boolean
        If SKU = "MEMO" Or SKU = "NOTE" Then
            Return True
        Else
            Return False
        End If
    End Function

    Private Sub SaleOptions_Btn_Click(sender As Object, e As RoutedEventArgs) Handles SaleOptions_Btn.Click
        Try
            Dim SQL As String = ""
            Dim Segment As String = ""
            Dim SegmentSet As String = ""
            Dim TDate As Date
            Dim Description As String = ""
            Dim Charge As Double = 0
            Dim Payment As Double = 0
            Dim Balance As Double = 0
            Dim CCApprovalNum As String = ""

            If IsNothing(InvoiceHistoryList) Then
                InvoiceHistoryList = New List(Of InvoiceHistoryItem)
            Else
                InvoiceHistoryList.Clear()
                InvoiceHistoryView.Items.Refresh()
            End If



            If Not RecoveredInvoiceNumber = 0 Then

                Balance = 0

                SQL = "SELECT [Date], [Desc], Charge, Payment, ApprovalNum FROM Payments WHERE InvNum = '" & RecoveredInvoiceNumber & "' ORDER BY ID"
                SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
                Do Until SegmentSet = ""

                    Segment = GetNextSegmentFromSet(SegmentSet)
                    TDate = ExtractElementFromSegment("Date", Segment)
                    Description = ExtractElementFromSegment("Desc", Segment)
                    Charge = Val(ExtractElementFromSegment("Charge", Segment))
                    Payment = Val(ExtractElementFromSegment("Payment", Segment))
                    Balance += (Charge - Payment)

                    If ExtractElementFromSegment("ApprovalNum", Segment, "") <> "" Then
                        CCApprovalNum = ExtractElementFromSegment("ApprovalNum", Segment, "")
                    End If

                    Dim item As InvoiceHistoryItem = New InvoiceHistoryItem
                    item.InvDate = ExtractElementFromSegment("Date", Segment)
                    item.Memo = ExtractElementFromSegment("Desc", Segment)
                    item.ChargePayment = Charge - Payment
                    item.ChargePayment = Format(item.ChargePayment, "0.00")

                    item.Balance = Balance

                    InvoiceHistoryList.Add(item)

                Loop
                InvoiceHistoryView.ItemsSource = InvoiceHistoryList
                InvoiceHistoryView.Items.Refresh()


                If CCApprovalNum <> "" Then

                    Dim iloc = InStr(1, CCApprovalNum, "/")
                    If iloc <> 0 Then
                        RecoveredInvoice_CC_Grid.Visibility = Visibility.Visible
                        Approval_TxtBx.Text = CCApprovalNum.Substring(0, iloc - 1)
                        ReferenceID_TxtBx.Text = Mid(CCApprovalNum, iloc)
                    Else
                        RecoveredInvoice_CC_Grid.Visibility = Visibility.Hidden
                    End If

                Else
                    RecoveredInvoice_CC_Grid.Visibility = Visibility.Hidden
                End If

            End If

            If InvoiceType.Content = "Hold" Then
                QuickRefund_Btn.Visibility = Visibility.Hidden
                ReprintReceipt_Btn.Visibility = Visibility.Hidden
                ReprintInvoice_Btn.Visibility = Visibility.Hidden
            Else
                QuickRefund_Btn.Visibility = Visibility.Visible
                ReprintReceipt_Btn.Visibility = Visibility.Visible
                ReprintInvoice_Btn.Visibility = Visibility.Visible
            End If

            If gCustomerSegment <> "" Then
                Email_TxtBx.Text = ExtractElementFromSegment("Email", gCustomerSegment, "")
            End If

            SaleOptions_Popup.IsOpen = True

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Displaying Sale Options.")
        End Try
    End Sub

    Private Sub Tickler_Btn_Click(sender As Object, e As RoutedEventArgs) Handles TicklerButton.Click
        Try


            Dim win As New Tickler(Me)
            win.ShowDialog(Me)

            Tickler_Count_Lbl.Content = Tickler.Get_Open_Tickler_Count()

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error.")
        End Try

    End Sub

    Private Sub QuoteInvoice_Button_Click(sender As Object, e As RoutedEventArgs) Handles QuoteInvoice_Button.Click
        ' SRN-12

        Lookup_Quote(True)
    End Sub

#End Region


#Region "Cash Paid Out"
    Private Class CashPaidOutItem
        Public Property DateRecorded As Date
        Public Property TimeRecorded As String
        Public Property Detail As String
        Public Property Amount As Double
        Public Property Clerk As String
    End Class
    Private Sub CashPaidOut_Btn_Click(sender As Object, e As RoutedEventArgs) Handles CashPaidOut_Btn.Click
        Try
            If GetPolicyData(gShipriteDB, "POSSecurity") Then
                If Check_Current_User_Permission("POSManager") Then
                    CashPaidOut_Popup.IsOpen = True
                    POSOptions_Popup.IsOpen = False
                End If
            Else
                CashPaidOut_Popup.IsOpen = True
                POSOptions_Popup.IsOpen = False
            End If
            If CashPaidOut_Popup.IsOpen = True Then

                CashPaidOut_Amt_TxtBx.Focus()

            End If


        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error.")
        End Try
    End Sub

    Private Sub CashPaidOut_CloseScreen_Btn_Click(sender As Object, e As RoutedEventArgs) Handles CashPaidOut_CloseScreen_Btn.Click
        CashPaidOut_Popup.IsOpen = False
    End Sub

    Private Sub CashPaidOut_TabCtrl_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles CashPaidOut_TabCtrl.SelectionChanged
        If CashPaidOut_TabCtrl.SelectedIndex = 1 Then
            Populate_CashPaidOut_Log()
        End If
    End Sub

    Private Sub Populate_CashPaidOut_Log()
        Try
            Dim Time As DateTime
            Dim item As CashPaidOutItem
            Dim CashPaidOut_ItemList As List(Of CashPaidOutItem) = New List(Of CashPaidOutItem)

            Dim current_Segment As String
            Dim SegmentSet As String = IO_GetSegmentSet(gShipriteDB, "Select [Date], [Time], [Payment], [SalesRep], [PaidOutReason] From Payments WHERE [Type]='Paid-Out' Order By [Date] DESC")

            Do Until SegmentSet = ""
                current_Segment = GetNextSegmentFromSet(SegmentSet)
                item = New CashPaidOutItem

                item.DateRecorded = ExtractElementFromSegment("Date", current_Segment)
                Time = ExtractElementFromSegment("Time", current_Segment)
                item.TimeRecorded = Format(Time, "hh:mm tt")

                item.Detail = ExtractElementFromSegment("PaidOutReason", current_Segment)
                item.Amount = ExtractElementFromSegment("Payment", current_Segment)
                item.Clerk = ExtractElementFromSegment("SalesRep", current_Segment)


                CashPaidOut_ItemList.Add(item)
            Loop

            CashPaidOut_LV.ItemsSource = CashPaidOut_ItemList
            CashPaidOut_LV.Items.Refresh()

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Populating Cash Paid Out Log.")
        End Try
    End Sub

    Private Sub CashPaidOut_Save_Btn_Click(sender As Object, e As RoutedEventArgs) Handles CashPaidOut_Save_Btn.Click
        Try
            Dim Segment As String = ""
            Dim CashPaidOut_Amount As Double
            Dim SQL As String
            Dim ret As Long

            CashPaidOut_Popup.IsOpen = False
            If CashPaidOut_Amt_TxtBx.Text = "" Then
                CashPaidOut_Amount = 0
            Else
                CashPaidOut_Amount = CDbl(CashPaidOut_Amt_TxtBx.Text)
            End If

            If CashPaidOut_Amount = "0" Then
                MsgBox("Please enter a valid Amount.", vbExclamation + vbOKOnly)
                Exit Sub
            End If

            gInvoiceNumber = GetNextInvoiceNumber().ToString

            Segment = AddElementToSegment(Segment, "InvNum", gInvoiceNumber)
            Segment = AddElementToSegment(Segment, "NumericInvoiceNumber", gInvoiceNumber)
            Segment = AddElementToSegment(Segment, "AcctNum", "ADMIN")
            Segment = AddElementToSegment(Segment, "AcctName", "Administration")
            Segment = AddElementToSegment(Segment, "Date", Now.Date.ToString)
            Segment = AddElementToSegment(Segment, "Time", Now.TimeOfDay.ToString("hh\:mm\:ss"))
            Segment = AddElementToSegment(Segment, "Desc", "Removed, Cash")
            Segment = AddElementToSegment(Segment, "Charge", CashPaidOut_Amount)
            Segment = AddElementToSegment(Segment, "Payment", "0")
            Segment = AddElementToSegment(Segment, "SalesRep", gCurrentUser)
            Segment = AddElementToSegment(Segment, "Type", "Cash")
            Segment = AddElementToSegment(Segment, "Status", "Ok")
            Segment = AddElementToSegment(Segment, "DrawerID", gDrawerID)
            Segment = AddElementToSegment(Segment, "DrawerStatus", "Open")
            Segment = AddElementToSegment(Segment, "OtherText", "Paid Out")

            SQL = MakeInsertSQLFromSchema("Payments", Segment, gdbSchema_Payments, True)
            ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

            Segment = ""
            Segment = AddElementToSegment(Segment, "InvNum", gInvoiceNumber)
            Segment = AddElementToSegment(Segment, "NumericInvoiceNumber", gInvoiceNumber)
            Segment = AddElementToSegment(Segment, "AcctNum", "ADMIN")
            Segment = AddElementToSegment(Segment, "AcctName", "Administration")
            Segment = AddElementToSegment(Segment, "Date", Now.Date.ToString)
            Segment = AddElementToSegment(Segment, "Time", Now.TimeOfDay.ToString("hh\:mm\:ss"))
            Segment = AddElementToSegment(Segment, "Desc", "Cash Paid Out")
            Segment = AddElementToSegment(Segment, "Charge", "0")
            Segment = AddElementToSegment(Segment, "Payment", CashPaidOut_Amount)
            Segment = AddElementToSegment(Segment, "SalesRep", gCurrentUser)
            Segment = AddElementToSegment(Segment, "Type", "Paid-Out")
            Segment = AddElementToSegment(Segment, "Status", "Ok")
            Segment = AddElementToSegment(Segment, "DrawerID", gDrawerID)
            Segment = AddElementToSegment(Segment, "DrawerStatus", "Open")
            Segment = AddElementToSegment(Segment, "OtherText", "Cash Paid Out")
            Segment = AddElementToSegment(Segment, "PaidOutReason", CashPaidOut_Purpose_TxtBx.Text)
            'Segment = AddElementToSegment(Segment, "DID", gCurrentCloseID)

            SQL = MakeInsertSQLFromSchema("Payments", Segment, gdbSchema_Payments, True)
            ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
            ret = PrintCashPaidOutReceipt()
            CashPaidOut_Purpose_TxtBx.Text = ""
            CashPaidOut_Amt_TxtBx.Text = "0.00"
            CashPaidOut_Popup.IsOpen = False

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Saving Cash Paid Out.")
        End Try
    End Sub


    Private Sub SaveCustomerNote_Btn_Click(sender As Object, e As RoutedEventArgs) Handles SaveCustomerNote_Btn.Click
        Try
            ContactManager.Save_Customer_Notes(ExtractElementFromSegment("ID", gCustomerSegment, ""), CustomerNote_TxtBx.Text)

            If CustomerNote_TxtBx.Text <> "" Then
                CustomerName.Foreground = New System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Yellow)
            Else
                CustomerName.Foreground = New System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White)
            End If
            ContactDropDown_Popup.IsOpen = False

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error.")
        End Try
    End Sub

    Private Sub DeleteCustomerNotes_Btn_Click(sender As Object, e As RoutedEventArgs) Handles DeleteCustomerNotes_Btn.Click
        Try
            ContactManager.Delete_Customer_Notes(ExtractElementFromSegment("ID", gCustomerSegment, ""), CustomerNote_TxtBx)
            CustomerName.Foreground = New System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White)
            ContactDropDown_Popup.IsOpen = False

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error.")
        End Try
    End Sub

#End Region

#Region "POSLine Editor"
    ' SKU line:
    ' SKU(all chars)    UnitPrice(+/- dec #)    Quantity(+/- #, can be dec)    ExtPrice(+/- dec #)
    Dim SKULineRegex = "^.+ +-?[0-9]+\.[0-9]{2,} +-?[0-9]+(\.[0-9]{2,})? +-?[0-9]+\.[0-9]{2,}$"
    Dim ShippingLineRegex = "^\.\.\..+:.*$"
    Dim TaxCountiesList As New List(Of Tuple(Of String, String, Double))

    Private Sub Receipt_LB_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs)
        OpenLineEditor()
    End Sub

    Private Sub OpenLineEditor()
        Try
            If Is_NewSale_Quote_Hold(InvoiceType.Content) Then

                If IsNothing(Receipt_LB.SelectedItem) Then Exit Sub

                If Receipt_LB.SelectedItem.SKU = "NOTE" And (Receipt_LB.SelectedItem.Description.StartsWith("Expiration Date: ") Or Receipt_LB.SelectedItem.Description.StartsWith("Mailbox# ")) Then
                    'do not allow user to edit Mailbox Notes. 
                    Exit Sub
                End If

                If gIsPOSSecurityEnabled AndAlso Not Check_Current_User_Permission("POS_Discounts") Then Exit Sub

                Dim MySelectedLine As POSLine = Receipt_LB.SelectedItem

                If isItem_NoDiscount(MySelectedLine) Then
                    PopupDiscount.Visibility = Visibility.Hidden
                    PopupSell.Visibility = Visibility.Hidden
                Else
                    PopupDiscount.Visibility = Visibility.Visible
                    PopupSell.Visibility = Visibility.Visible
                End If


                Receipt_LB.IsEnabled = False

                PopName.Content = MySelectedLine.Description
                PopupSell.Text = MySelectedLine.UnitPrice.ToString("0.00")
                PopupDiscount.Text = MySelectedLine.Discount
                PopupQuantity.Text = MySelectedLine.Quantity
                PopupDescription.Text = MySelectedLine.Description

                ' Tax County
                TaxCountiesList = New List(Of Tuple(Of String, String, Double)) ' reset
                Dim SQL As String
                Dim SegmentSet As String
                Dim Segment As String
                SQL = "SELECT County, TaxRate, ID FROM CountyTaxes"
                SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
                Do Until SegmentSet = ""
                    Segment = GetNextSegmentFromSet(SegmentSet)
                    Dim County As String = ExtractElementFromSegment("County", Segment)
                    Dim Rate As Double = Val(ExtractElementFromSegment("TaxRate", Segment))
                    TaxCountiesList.Add(New Tuple(Of String, String, Double)(County & " - " & Rate.ToString, County, Rate))
                Loop

                PopupTaxCounty.ItemsSource = TaxCountiesList.Select(Function(x) x.Item1)
                Dim tindex As Integer = TaxCountiesList.FindIndex(Function(x) x.Item2 = MySelectedLine.TaxCounty)
                If tindex >= 0 Then
                    PopupTaxCounty.SelectedIndex = tindex
                Else
                    PopupTaxCounty.SelectedIndex = TaxCountiesList.FindIndex(Function(x) x.Item2 = gPOSHeader.TaxCounty)
                End If


                ' Open Editor
                LineEdit_Popup.IsOpen = True
                PopupSell.Focus()

            Else
                Set_COGS_View()

            End If

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Opening Line Editor.")
        End Try
    End Sub


    Private Sub LineEditPopup_Save()
        Try
            Dim ret As String = ""

            Dim LineItem = Receipt_LB.SelectedItem

            ' Set Item's Properties
            LineItem.Discount = Val(PopupDiscount.Text)
            LineItem.Quantity = Val(PopupQuantity.Text)

            If PopupDescription.Text <> "" Then
                LineItem.Description = PopupDescription.Text
            End If

            Dim selectedTaxCounty As Tuple(Of String, String, Double) = TaxCountiesList.Find(Function(x) x.Item1 = PopupTaxCounty.SelectedItem)


            If LineItem.UnitPrice = Val(PopupSell.Text) Then
                Update_POS_LineItemTotal(LineItem, selectedTaxCounty)
            Else
                'unit price changed
                LineItem.isPriceOverride = True
                Update_POS_LineItemTotal(LineItem, selectedTaxCounty, Val(PopupSell.Text))
            End If



            LineEdit_Popup.IsOpen = False
            Receipt_LB.IsEnabled = True

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Saving POS Line Edit.")
        End Try
    End Sub

    Private Sub Update_POS_LineItemTotal(ByRef Lineitem As POSLine, Optional selectedTaxCounty As Tuple(Of String, String, Double) = Nothing, Optional NewUnitPrice As Double = -12345.67)

        Try
            Dim levelprice As Double

            Dim SegmentSet = IO_GetSegmentSet(gShipriteDB, "SELECT * FROM Inventory WHERE SKU = '" & Lineitem.SKU & "'")
            levelprice = GetLevelPricing(SegmentSet, Lineitem.Quantity)


            If NewUnitPrice = -12345.67 Then
                'no new unit price in line editor

                'if user set a price override, use it.
                If Lineitem.isPriceOverride = False Then

                    If levelprice <> 0 Then
                        Lineitem.UnitPrice = levelprice
                    Else
                        If String.IsNullOrEmpty(Lineitem.PackageID) Then 'don't use inventory price for shipments
                            Lineitem.UnitPrice = ExtractElementFromSegment("Sell", SegmentSet, "0")
                        End If

                    End If
                End If

            Else
                Lineitem.UnitPrice = NewUnitPrice
            End If

            If Lineitem.Discount = 0 Then
                Lineitem.ExtPrice = Lineitem.UnitPrice * Lineitem.Quantity
            Else
                CalculateDiscountPrice(Lineitem)
            End If

            If selectedTaxCounty IsNot Nothing Then
                Lineitem.TRate = selectedTaxCounty.Item3
                Lineitem.TaxCounty = selectedTaxCounty.Item2
            Else
                Lineitem.TRate = gPOSHeader.TaxRate
                Lineitem.TaxCounty = gPOSHeader.TaxCounty
            End If
            Lineitem.STax = Calculate_SalesTax(Lineitem.SKU, Lineitem.Department, Lineitem.ExtPrice, Lineitem.TRate)

            'Lineitem.STax = Lineitem.TRate / 100 * Lineitem.ExtPrice

            Lineitem.LTotal = Lineitem.ExtPrice + Lineitem.STax
            Lineitem.COGS = Lineitem.UnitCost * Lineitem.Quantity

            Receipt_LB.Items.Refresh()
            CalculateInvoice()

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error updating POS Pricing.")
        End Try
    End Sub



    Private Sub LineEditCancel_Btn_Click(sender As Object, e As RoutedEventArgs) Handles LineEditCancel_Btn.Click

        CloseLineEditPopup()

    End Sub

    Private Sub CloseLineEditPopup()
        LineEdit_Popup.IsOpen = False
        Receipt_LB.IsEnabled = True
    End Sub

    Private Sub LineEdit_TxtBx_KeyUp(sender As Object, e As KeyEventArgs) Handles PopupSell.KeyUp, PopupQuantity.KeyUp, PopupDiscount.KeyUp
        If (e.Key = Key.Return) Then
            LineEditPopup_Save()
        End If
    End Sub

    Private Sub LineEditSave_Btn_Click(sender As Object, e As RoutedEventArgs) Handles LineEditSave_Btn.Click
        LineEditPopup_Save()
    End Sub

    Private Sub PopupDiscount_GotFocus(sender As Object, e As RoutedEventArgs) Handles PopupDiscount.GotFocus
        If PopupDiscount.Text = "0" Then PopupDiscount.Text = ""
    End Sub

    Private Sub PopupQunatity_GotFocus(sender As Object, e As RoutedEventArgs) Handles PopupQuantity.GotFocus
        If PopupQuantity.Text = "1" Then PopupQuantity.Text = ""
    End Sub

    Private Sub PopupQunatity_LostFocus(sender As Object, e As RoutedEventArgs) Handles PopupQuantity.LostFocus
        If PopupQuantity.Text = "" Then PopupQuantity.Text = POSLines(MySelectedLine).Quantity
    End Sub

#End Region

#Region "Refunds"
    Private Sub QuickRefund_Btn_Click(sender As Object, e As RoutedEventArgs) Handles QuickRefund_Btn.Click
        Try
            Dim ret As Integer
            SaleOptions_Popup.IsOpen = False
            Refund_Popup.IsOpen = True
            ret = Load_Invoice_for_Refund()

            If ret = 1 Then
                Refund_Popup.IsOpen = False
            End If

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error.")
        End Try
    End Sub

    Private Function Load_Invoice_for_Refund() As Integer
        Try

            Dim Segment As String
            Dim LineItem As Refund_LineItem
            Refund_Line_List = New List(Of Refund_LineItem)

            Dim SegmentSet As String = IO_GetSegmentSet(gShipriteDB, "SELECT ID, SKU, Desc, Dept, UnitPrice, Qty, ExtPrice, TRate, STax, COGS, ReturnedQty From Transactions WHERE InvNum='" & RecoveredInvoiceNumber & "'")

            If SegmentSet = "" Then

                Refund_Popup.IsOpen = False
                MsgBox("ATTENTION...Refund Processing" & vbCrLf & vbCrLf & "Nothing available to refund. All lines already refunded.", vbCritical, "ShipriteNext")
                Return 1
                Exit Function

            End If
            Do Until SegmentSet = ""

                Segment = GetNextSegmentFromSet(SegmentSet)
                LineItem = New Refund_LineItem

                LineItem.ID = ExtractElementFromSegment("ID", Segment)
                LineItem.SKU = ExtractElementFromSegment("SKU", Segment)
                LineItem.Desc = ExtractElementFromSegment("Desc", Segment)
                LineItem.UnitPrice = ExtractElementFromSegment("UnitPrice", Segment)
                LineItem.Qty = ExtractElementFromSegment("Qty", Segment)
                LineItem.LineTotal = ExtractElementFromSegment("ExtPrice", Segment)
                LineItem.STax = ExtractElementFromSegment("STax", Segment)
                LineItem.TRate = ExtractElementFromSegment("TRate", Segment)
                LineItem.PreviouslyRefundedQty = ExtractElementFromSegment("ReturnedQty", Segment, "0")
                LineItem.Department = ExtractElementFromSegment("Dept", Segment, "0")
                LineItem.COGS = ExtractElementFromSegment("COGS", Segment, "0")

                If LineItem.Qty = LineItem.PreviouslyRefundedQty Then
                    LineItem.isRefundable = False
                Else
                    LineItem.isRefundable = True
                End If

                Refund_Line_List.Add(LineItem)

            Loop

            Refund_LV.ItemsSource = Refund_Line_List
            Refund_LV.Items.Refresh()


        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Loading Invoice For Refund.")
        End Try

        Return 0
    End Function

    Private Sub CancelRefund_Btn_Click(sender As Object, e As RoutedEventArgs) Handles CancelRefund_Btn.Click
        Refund_Popup.IsOpen = False
    End Sub

    Private Sub Calculate_Total_refund()
        Try
            Dim subtotal As Double = 0
            Dim TaxSubTotal As Double = 0

            For Each item As Refund_LineItem In Refund_Line_List
                If item.isRefunded Then
                    item.Refund_Tax = item.Refund_Amt * (item.TRate / 100)
                    If item.Refund_Tax > item.STax Then item.Refund_Tax = item.STax

                    subtotal += item.Refund_Amt
                    TaxSubTotal += item.Refund_Tax
                End If
            Next

            Refund_SubTotal_Lbl.Content = Strings.FormatCurrency(subtotal)
            Refund_SalesTax_Lbl.Content = Strings.FormatCurrency(TaxSubTotal)
            Refund_Total_Lbl.Content = Strings.FormatCurrency(subtotal + TaxSubTotal)

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Calculating Refund.")
        End Try
    End Sub

    Private Sub Refund_Amt_TxtBx_TextChanged()
        Calculate_Total_refund()
    End Sub

    Private Sub Refund_TextBox_LostFocus(sender As Object, e As RoutedEventArgs)
        Try
            'refund quantity changed, recalculate refund line total
            If Refund_LV.SelectedIndex = -1 Then Exit Sub
            Dim item As Refund_LineItem = Refund_LV.SelectedItem

            If item.Refund_Qty > (item.Qty - item.PreviouslyRefundedQty) Then
                item.Refund_Qty = item.Qty - item.PreviouslyRefundedQty
            End If

            item.Refund_Amt = item.Refund_Qty * item.UnitPrice

            'If item.Refund_Amt > item.LineTotal Then
            'item.Refund_Amt = item.LineTotal
            'End If

            Refund_LV.Items.Refresh()
            Calculate_Total_refund()

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error.")
        End Try
    End Sub

    Protected Sub SelectCurrentItem(ByVal sender As Object, ByVal e As KeyboardFocusChangedEventArgs)
        Try
            'When clicking inside a textbox, select the listiew item that the textbox belongs to.
            Dim item As ListViewItem = CType(sender, ListViewItem)

            item.IsSelected = True

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Loading Invoice For Refund.")
        End Try
    End Sub

    Private Sub Refund_CheckBox_Checked()
        Try
            If Refund_LV.SelectedIndex = -1 Then Exit Sub
            Dim item As Refund_LineItem = Refund_LV.SelectedItem

            item.Refund_Qty = item.Qty - item.PreviouslyRefundedQty
            item.Refund_Amt = item.LineTotal


            Refund_LV.Items.Refresh()
            Calculate_Total_refund()

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error.")
        End Try
    End Sub

    Private Sub Refund_CheckBox_Unchecked()
        Try
            If Refund_LV.SelectedIndex = -1 Then Exit Sub
            Dim item As Refund_LineItem = Refund_LV.SelectedItem

            item.Refund_Qty = 0
            item.Refund_Amt = 0

            Refund_LV.Items.Refresh()
            Calculate_Total_refund()

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Loading Invoice For Refund.")
        End Try
    End Sub

    Private Sub Continue_Click(sender As Object, e As RoutedEventArgs) Handles [Continue].Click
        Try

            Dim LineItem As Refund_LineItem
            Dim buf As String = ""
            Dim LineCT As Integer = 0
            Dim ID As Long = 0
            Dim IDstack As String = ""
            Dim RefundInvoice = ""
            Dim Segment As String = ""

            If Refund_Line_List.FindAll((Function(x) x.isRefunded)).Count = 0 Then
                Exit Sub
                'msgbox is hidden behind refund setup and cannot be clicked.
                'MsgBox("ATTENTION...Nothing Selected to Refund.", vbIgnore, "ShipriteNext")
            End If


            Refund_Popup.IsOpen = False
            RefundInvoice = RecoveredInvoiceNumber
            ClearPOS()
            gRefundSegment = AddElementToSegment(gRefundSegment, "ReturnInvoiceNumber", RefundInvoice)

            ' assign by ref
            Refund_Line_List = New List(Of Refund_LineItem)
            Refund_Line_List = Refund_LV.ItemsSource

            For Each LineItem In Refund_Line_List
                If LineItem.isRefunded = True Then

                    ID = LineItem.ID
                    If Not IDstack = "" Then

                        IDstack = IDstack & ","

                    End If
                    IDstack &= ID.ToString & "-" & LineItem.Refund_Qty + LineItem.PreviouslyRefundedQty

                    Segment = IO_GetSegmentSet(gShipriteDB, "SELECT * FROM Inventory WHERE SKU = '" & LineItem.SKU & "'")

                    If Segment = "" Then
                        'If invenotry item is not availble, then use the data from the original transaction.
                        Segment = AddElementToSegment(Segment, "Desc", LineItem.Desc)
                        Segment = AddElementToSegment(Segment, "Department", LineItem.Department)
                        Segment = AddElementToSegment(Segment, "Cost", LineItem.COGS)
                    End If


                    AddPosLineToSet(0, LineItem.SKU, Segment, LineItem.UnitPrice, LineItem.Refund_Qty * -1, 0)

                End If
            Next
            gRefundSegment = AddElementToSegment(gRefundSegment, "IDStack", IDstack)

            buf = "Invoice#: -----"
            LineCT = Fix(DefaultLineCT / 2)
            buf = FillData(buf, LineCT, "L")
            buf &= FillData("Date: " & Now.Month & "/" & Now.Day & "/" & Now.Year, LineCT, "R")

            CalculateInvoice()
            Receipt_LB.Items.Refresh()

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Processing Refund Selection.")
        End Try
    End Sub

    Private Sub GiftCard_CloseScreen_Btn_Click(sender As Object, e As RoutedEventArgs) Handles GiftCard_CloseScreen_Btn.Click

        GiftCard_Popup.IsOpen = False
        DInput.Text = ""
        DInput.Focus()

    End Sub

    Private Sub GiftCard_Btn_Click(sender As Object, e As RoutedEventArgs) Handles GiftCard_Btn.Click

        Dim ret As Integer
        POSOptions_Popup.IsOpen = False
        DInput.Text = "GIFT"
        DInput.Tag = DInput.Text
        ret = ProcessInput()

    End Sub

    Private Sub NewGiftCard_CloseScreen_Btn_Click(sender As Object, e As RoutedEventArgs) Handles NewGiftCard_CloseScreen_Btn.Click

        NewGiftCard_Popup.IsOpen = False
        DInput.Text = ""
        DInput.Focus()

    End Sub

    Private Sub CashPaidOut_Amt_TxtBx_GotFocus(sender As Object, e As RoutedEventArgs) Handles CashPaidOut_Amt_TxtBx.GotFocus

        If Val(sender.text) = 0 Then

            sender.text = ""

        End If
    End Sub

    Private Sub CashPaidOut_Amt_TxtBx_LostFocus(sender As Object, e As RoutedEventArgs) Handles CashPaidOut_Amt_TxtBx.LostFocus

        If sender.text = "" Then

            sender.text = "0.00"

        End If

    End Sub

    Private Sub GiftCardNumber_KeyDown(sender As Object, e As KeyEventArgs) Handles GiftCardNumber.KeyDown
        Try
            If Not e.Key = Key.Return Then

                Exit Sub

            End If

            Dim SQL As String = ""
            Dim SegmentSet As String = ""
            Dim ret As Integer = 0
            Dim Charges As Double = 0
            Dim Payments As Double = 0
            Dim Balance As Double = 0
            Dim Deposit As Double = 0
            Dim Disbursed As Double = 0

            NewGiftCard_Popup.IsOpen = False
            If sender.text = "" Then

                Exit Sub

            End If
            sender.text = UCase(sender.text)
            SQL = "SELECT * FROM GiftRegistry WHERE GiftIDNumber = '" & sender.text & "'"
            SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
            If SegmentSet = "" Then

                Balance = ValFix(PoleDisplay_Total.Text)
                If Not Balance = 0 Then

                    MsgBox("ATTENTION...You cannot add a gift card to an invoice" & vbCrLf & "with other items.  Gift card purchases can only be sold" & vbCrLf & "on a separate invoice.", vbInformation)
                    DInput.Text = ""
                    DInput.Focus()
                    Exit Sub

                End If
                gResult = "GIFT CARD IN EFFECT"

                Dim win As New POS_Payment(Me)
                win.ShowDialog(Me)

                If gPaymentsCompleted = True Then

                    gResult = "GIFT CARD IN EFFECT"
                    ret = PostPayments()
                    ret = PostGiftCard(Val(gResult), GiftCardNumber.Text)
                    gResult = "GIFT CARD IN EFFECT"
                    ret = PrintReceipt()
                    gResult = ""

                End If

            Else

                Balance = ValFix(PoleDisplay_Total.Text)
                If Not Balance = 0 Then

                    AddFundsToGiftCardButton.Visibility = Visibility.Hidden

                Else

                    AddFundsToGiftCardButton.Visibility = Visibility.Visible

                End If

                GiftCardNumber2.Text = GiftCardNumber.Text
                SQL = "SELECT InvNum FROM GiftRegistry WHERE GiftIDNumber = '" & GiftCardNumber2.Text & "'"
                SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
                gInvoiceNumber = ExtractElementFromSegment("InvNum", SegmentSet)
                SQL = "SELECT SUM(Charge) AS Charges, SUM(Payment) AS Paid FROM Payments WHERE InvNum = '" & gInvoiceNumber & "'"
                SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
                Charges = Val(ExtractElementFromSegment("Charges", SegmentSet))
                Payments = Val(ExtractElementFromSegment("Paid", SegmentSet))
                Balance = Payments - Charges
                GiftCardBalance.Text = Format(Balance, "$ 0.00")

                SQL = "SELECT InvNum FROM GiftRegistry WHERE GiftIDNumber = '" & GiftCardNumber2.Text & "'"
                SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)

                SQL = "SELECT [Date], InvNum, [Desc], Payment AS Deposit, Charge AS Disbursed, 0 AS Balance FROM Payments WHERE InvNum = '" & ExtractElementFromSegment("InvNum", SegmentSet) & "' AND NOT [Type] = 'Sale' ORDER BY ID"


                BindingOperations.ClearAllBindings(GiftCard_LV) ' clear binding on ListView
                GiftCard_LV.DataContext = Nothing ' remove any rows already in ListView
                Dim DT As New System.Data.DataTable ' datatable to use to populate ListView
                Dim currentGridView As GridView = GiftCard_LV.View ' variable to reference current GridView in Users_ListView to set up columns.

                DT.Columns.Add("Date", GetType(Date))
                DT.Columns.Add("InvNum")
                DT.Columns.Add("Desc")
                DT.Columns.Add("Deposit", GetType(Double))
                DT.Columns.Add("Disbursed", GetType(Double))
                DT.Columns.Add("Balance", GetType(Double))
                ret = IO_LoadListView(GiftCard_LV, DT, gShipriteDB, SQL, 6, "")

                Balance = 0
                For i = 0 To ret - 1

                    Deposit = Val(DT.Rows.Item(i).Item(3))
                    Disbursed = Val(DT.Rows.Item(i).Item(4))
                    Balance = Balance + Deposit - Disbursed
                    DT.Rows.Item(i).Item(5) = Balance

                Next i

                GiftCard_Popup.IsOpen = True
                AddFundsToGiftCardButton.Focus()
                Exit Sub

            End If
            GiftCardNumber.Text = ""
            ret = NewSale()

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Processing new Gift Card.")
        End Try
    End Sub

    Private Sub AddFundsToGiftCardButton_Click(sender As Object, e As RoutedEventArgs) Handles AddFundsToGiftCardButton.Click
        Try
            Dim ret As Long

            gResult = "GIFT CARD IN EFFECT"

            Dim win As New POS_Payment(Me)
            win.ShowDialog(Me)
            ret = PostPayments()
            GiftCard_Popup.IsOpen = False
            gResult = "GIFT CARD IN EFFECT"
            ret = PrintReceipt()
            gResult = ""
            DInput.Text = ""
            DInput.Focus()

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Adding Funds to Gift Card.")
        End Try
    End Sub

#End Region

#Region "ShortcutKeyHandler"
    Private Sub Window_KeyDown(sender As Object, e As KeyEventArgs)
        If e.Key = Key.Delete Then
            HandleReceiptDelete(sender, e)

        ElseIf e.Key = Key.N AndAlso (e.KeyboardDevice IsNot Nothing AndAlso e.KeyboardDevice.Modifiers = ModifierKeys.Control) Then
            'Ctrl + N ... open No Sale
            NoSale_Btn_Click(Nothing, Nothing)
        Else
            ShortcutKeyHandlers.KeyDown(sender, e, Me)
        End If
    End Sub


#End Region

    Private Sub HandleReceiptDelete(sender As Object, e As RoutedEventArgs)
        Try
            Dim i As Integer = Receipt_LB.SelectedIndex

            If i = -1 Or Not Is_NewSale_Quote_Hold(InvoiceType.Content) Then Exit Sub
            Dim PackageId As String = Receipt_LB.SelectedItem.PackageID


            If gIsPOSSecurityEnabled AndAlso Not Check_Current_User_Permission("POS_DeleteLine") Then Exit Sub

            If Not String.IsNullOrEmpty(PackageId) Then
                'Deleting Shipment
                If ShipmentHistory.Void_Shipment(PackageId) Then
                    For Each line As POSLine In POSLines.ToList
                        If line.PackageID = PackageId Then POSLines.Remove(line)
                    Next

                Else
                    If MsgBox("Shipment could not be voided, would you still like to delete it from Receipt?", vbQuestion + vbYesNo) = MsgBoxResult.Yes Then
                        For Each line As POSLine In POSLines.ToList
                            If line.PackageID = PackageId Then POSLines.Remove(line)
                        Next
                    End If

                End If


            ElseIf Receipt_LB.SelectedItem.SKU = "MBX" Then
                'Deleting Mailbox Entry

                POSLines.Remove(Receipt_LB.SelectedItem)

                'Delete notes related to Mailbox rental
                If POSLines.ElementAt(i).Description.StartsWith("Mailbox") Then
                    POSLines.RemoveAt(i)
                End If

                If POSLines.ElementAt(i).Description.StartsWith("Expiration") Then
                    POSLines.RemoveAt(i)
                End If

            ElseIf Receipt_LB.SelectedItem.SKU = "NOTE" And (POSLines.ElementAt(i).Description.StartsWith("Expiration Date: ") Or POSLines.ElementAt(i).Description.StartsWith("Mailbox# ")) Then
                'do not allow user to delete Mailbox Notes. 
                Exit Sub
            Else

                POSLines.Remove(Receipt_LB.SelectedItem)
            End If


            Receipt_LB.Items.Refresh()

            CalculateInvoice()
            DInput.Text = ""
            DInput.Focus()

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Deleting Receipt Line Item.")
        End Try
    End Sub

    Private Sub ReprintReceipt_Btn_Click(sender As Object, e As RoutedEventArgs) Handles ReprintReceipt_Btn.Click
        If InvoiceType.Content = "Recovered Invoice" Then
            gPM.NewPayments = RecoveredInvoicePayments
            PrintReceipt()
            NewSale()
            Exit Sub
        End If
    End Sub

    Private Sub ReprintInvoice_Btn_Click(sender As Object, e As RoutedEventArgs) Handles ReprintInvoice_Btn.Click
        If InvoiceType.Content = "Recovered Invoice" Then
            Print_FullSheetInvoice(Rctp_InvoiceNum_TB.Text)
            NewSale()
        End If


    End Sub

    Private Sub EmailReceipt_Btn_Click(sender As Object, e As RoutedEventArgs) Handles EmailReceipt_Btn.Click
        If Email_TxtBx.Text = "" Then Exit Sub

        gPOS_EmailReceipt = New Email_POSReceipt

        gPOS_EmailReceipt.isEmail = True
        gPOS_EmailReceipt.EmailAddress = Email_TxtBx.Text
        gPOS_EmailReceipt.EmailTemplate = getEmailTemplate("Notify_Email-POSReceipt", "Mersad")

        Email_Receipt(True)
    End Sub

    Private Sub EmailInvoice_Btn_Click(sender As Object, e As RoutedEventArgs) Handles EmailInvoice_Btn.Click
        Dim report As New SHIPRITE.ShipRiteReports._ReportObject()

        Generate_InvoiceReport(report, Rctp_InvoiceNum_TB.Text)
        Email_Invoice(report, Rctp_InvoiceNum_TB.Text, Email_TxtBx.Text, True)

    End Sub

    Public Shared Sub Generate_InvoiceReport(ByRef report As ShipRiteReports._ReportObject, InvNum As String)
        Try
            report.ReportName = "Invoice.rpt"
            report.ReportFormula = "{Transactions.InvNum} = '" & InvNum & "'"

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error generating Invoice.")
        End Try
    End Sub

    Private Sub Email_Invoice(ByRef report As ShipRiteReports._ReportObject, InvNum As String, Email As String, displayConfirmation As Boolean)
        Try
            Dim InvoiceFilePath As String = gAppPath & "/Reports/Invoice.pdf"
            report.ReportSaveAsPath = InvoiceFilePath


            ShipRiteReports._LocalReport.Execute_ODBC_ToPDF(report)

            If _Files.IsFileExist(InvoiceFilePath, True) Then
                Dim template_Email As EmailTemplate = getEmailTemplate("Notify_Email-POSReceipt", "")


                Dim invoice_pdf As New System.Net.Mail.Attachment(InvoiceFilePath)

                Dim success As Boolean
                success = sendEmailWithAttachment(Email, template_Email.Subject, template_Email.Content, invoice_pdf)

                If success Then
                    If displayConfirmation Then
                        MsgBox("Email Sent Successfully!", vbInformation)
                    End If
                End If
            End If

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Emailing Invoice.")
        End Try
    End Sub


    Private Sub COGS_Btn_Click(sender As Object, e As RoutedEventArgs) Handles COGS_Btn.Click
        Set_COGS_View()
    End Sub

    Private Sub Set_COGS_View()
        Try

            If isCOGSview = False Then
                isCOGSview = True

                Description_TxtBlock.Text = " *COGS* *COGS* *COGS* *COGS* *COGS* *COGS*"
                Description_TxtBlock.Foreground = Media.Brushes.DarkRed
                COGS_Btn.Content = "Exit COGS View"

            Else
                COGS_Btn.Content = "View Cost Of Goods Sold"
                Description_TxtBlock.Text = " Description"
                Description_TxtBlock.Foreground = Media.Brushes.Black
                isCOGSview = False

            End If

            SaleOptions_Popup.IsOpen = False

            For Each item In POSLines
                item.isCOGSview = isCOGSview
            Next
            Receipt_LB.Items.Refresh()

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error setting COGS view.")
        End Try
    End Sub

    Public Shared Function Is_NewSale_Quote_Hold(InvoiceType As String) As Boolean
        If InvoiceType = "New Sale" Or InvoiceType = "Quote" Or InvoiceType = "Hold" Then
            Return True
        Else
            Return False
        End If
    End Function

#Region "Void Sale"

    Private Sub Void_Cancel_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Void_Cancel_Btn.Click
        Void_Popup.IsOpen = False
    End Sub

    Private Sub Void_Save_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Void_Save_Btn.Click
        Try
            Dim ans As Integer = 0
            Dim SQL As String = ""
            Dim ret As Integer = 0

            'ans = MsgBox("ATTENTION...VOIDING INVOICE #:" & RecoveredInvoiceNumber & vbCrLf & vbCrLf & "Continue???", vbQuestion + vbYesNo)
            'If ans = vbNo Then
            'Exit Sub
            'End If

            SQL = "UPDATE Transactions SET Status = 'VOIDED' WHERE InvNum = '" & RecoveredInvoiceNumber & "'"
            ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
            SQL = "UPDATE Payments SET Status = 'VOIDED' WHERE InvNum = '" & RecoveredInvoiceNumber & "'"
            ret += IO_UpdateSQLProcessor(gShipriteDB, SQL)
            'MsgBox(ret & " - ROWS AFFECTED", vbInformation, gProgramName)


            SQL = "INSERT INTO Void (InvNum, TotalSale, [Date], OriginalDate, OriginalClerk, VoidingClerk, Reason, AuthorizingClerk) " &
            "VALUES ('" & RecoveredInvoiceNumber & "', " & CDbl(Void_SaleTotal_Lbl.Content) & ", #" & DateTime.Now & "#, #" & Rctp_Date_TB.Text & "#, '" & void_OrigClerk.Text & "', '" & Void_VoidingClerk.Text & "', '" & Void_Reason.Text & "', '" & Void_ApprovedBy.Text & "')"
            IO_UpdateSQLProcessor(gShipriteDB, SQL)

            Clear_Void()

            Void_Popup.IsOpen = False

            NewSale()

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Voiding Invoice.")
        End Try
    End Sub

    Private Sub Clear_Void()
        Void_SaleTotal_Lbl.Content = ""
        void_OrigClerk.Text = ""
        Void_VoidingClerk.Text = ""
        Void_Reason.Text = ""
        Void_ApprovedBy.Text = ""
    End Sub

    Private Sub VoidSale_Btn_Click(sender As Object, e As RoutedEventArgs) Handles VoidSale_Btn.Click
        Try
            Dim tempUser As String = ""

            If InvoiceType.Content <> "Hold" And InvoiceType.Content <> "Quote" Then
                If Not isInvoiceDrawerOpen(RecoveredInvoiceNumber) Then
                    MsgBox("This invoice is included in a drawer that has been closed and cannot be voided. You must REFUND this invoice instead!", vbExclamation, "Cannot Void Invoice")
                    Exit Sub
                End If

            Else
                'Voiding Hold Sale
                RecoveredInvoiceNumber = gInvoiceNumber
            End If

            If gIsPOSSecurityEnabled Then
                If Not Check_Current_User_Permission("POS_VoidSale", True) Then
                    If MsgBox("User " & gCurrentUser & " does Not have the permission To void sales." & vbCrLf & vbCrLf & "Void needs To be approved by a authorized person!", vbExclamation + vbOKCancel) = vbCancel Then
                        Exit Sub
                    Else
                        tempUser = gCurrentUser 'opening the userlogin will overwrite the currentUser
                        If OpenUserLogin(Me, "POS_VoidSale") = False Then
                            gCurrentUser = tempUser
                            Exit Sub
                        Else
                            Void_ApprovedBy.Text = gCurrentUser
                            gCurrentUser = tempUser
                        End If
                    End If
                End If
            End If

            Void_Popup.IsOpen = True

            Dim SegmentSet As String = IO_GetSegmentSet(gShipriteDB, "Select Charge, SalesRep From Payments WHERE InvNum='" & RecoveredInvoiceNumber & "' AND Desc='Sales'")
            Void_SaleTotal_Lbl.Content = FormatCurrency(ExtractElementFromSegment("Charge", SegmentSet, ""))
            void_OrigClerk.Text = ExtractElementFromSegment("SalesRep", SegmentSet, "")
            Void_VoidingClerk.Text = Clerk_Lbl.Content

            If Void_ApprovedBy.Text = "" Then Void_ApprovedBy.Text = Clerk_Lbl.Content

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error.")
        End Try
    End Sub

    Private Function isInvoiceDrawerOpen(invNum As String) As Boolean
        If IO_GetSegmentSet(gShipriteDB, "Select DrawerStatus from Payments WHERE InvNum='" & invNum & "' AND DrawerStatus='Open'") = "" Then
            Return False
        Else
            Return True
        End If
    End Function

    Private Sub Void_Reason_Checked(sender As Object, e As RoutedEventArgs) Handles Void_CustomerChangedMind.Checked, Void_ClerkError.Checked, Void_Training.Checked, Void_Testing.Checked, Void_Other.Checked
        Try
            If Void_CustomerChangedMind.IsChecked Then
                Void_Reason.Text = Void_CustomerChangedMind.Content & " - "

            ElseIf Void_ClerkError.IsChecked Then
                Void_Reason.Text = Void_ClerkError.Content & " - "

            ElseIf Void_Training.IsChecked Then
                Void_Reason.Text = Void_Training.Content & " - "

            ElseIf Void_Testing.IsChecked Then
                Void_Reason.Text = Void_Testing.Content & " - "

            ElseIf Void_Other.IsChecked Then
                Void_Reason.Text = Void_Other.Content & " - "

            End If

            Void_Reason.Focus()

            Void_Reason.CaretIndex = Void_Reason.Text.Length

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error.")
        End Try
    End Sub




#End Region


    Protected Sub SelectCurrent_LB_Item(ByVal sender As Object, ByVal e As KeyboardFocusChangedEventArgs)
        'When clicking inside a textbox, select the listiew item that the textbox belongs to.
        Dim item As ListBoxItem = CType(sender, ListBoxItem)

        item.IsSelected = True
    End Sub

    Private Sub Receipt_Qty_TxtBx_LostKeyboardFocus(sender As Object, e As KeyboardFocusChangedEventArgs)
        Receipt_LB.Items.Refresh()

    End Sub

    Private Sub Receipt_Qty_TxtBx_KeyDown(sender As Object, e As KeyEventArgs)
        If e.Key = Key.Return Or e.Key = Key.Tab Then
            Dim LineItem As POSLine = Receipt_LB.SelectedItem
            If (sender.text = "" Or sender.text = "0") Then sender.text = "1"
            LineItem.Quantity = sender.text
            Update_POS_LineItemTotal(LineItem)
        End If
    End Sub

    Private Sub ROA_Button_Click(sender As Object, e As RoutedEventArgs) Handles ROA_Button.Click
        ' to receive money on account
        CloseLineEditPopup()

        If ExtractElementFromSegment("AR", gCustomerSegment, "") = "" Or ExtractElementFromSegment("AR", gCustomerSegment, "") = "CASH" Then

            MsgBox("Cannot Apply ROA payment!" & vbCrLf & vbCrLf & "You must select an AR customer account to use 'ROA'", vbCritical)
            DInput.Text = ""
            DInput.Focus()
            Exit Sub

        End If

        gROAinEffect = True
        Call ProcessTransaction_Button_Click(Nothing, Nothing)
        gROAinEffect = False
    End Sub



    Private Sub Receipt_LB_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles Receipt_LB.SelectionChanged
        If Receipt_LB.SelectedIndex = -1 Or InvoiceType.Content = "New Sale" Then
            TrackPackage_Btn.Visibility = Visibility.Hidden
            Exit Sub
        End If
        Dim line As POSLine = Receipt_LB.SelectedItem


        If Not String.IsNullOrEmpty(Receipt_LB.SelectedItem.PackageID) Then
            TrackPackage_Btn.Visibility = Visibility.Visible
        Else
            TrackPackage_Btn.Visibility = Visibility.Hidden
        End If

    End Sub

    Private Sub TrackPackage_Btn_Click(sender As Object, e As RoutedEventArgs) Handles TrackPackage_Btn.Click
        Dim segment As String
        Dim Carrier As String
        Dim TrackingNo As String

        If String.IsNullOrEmpty(Receipt_LB.SelectedItem.PackageID) Then Exit Sub


        segment = IO_GetSegmentSet(gShipriteDB, "SELECT Carrier, [Tracking#] from Manifest WHERE [PackageID]='" & Receipt_LB.SelectedItem.packageID & "'")
        Carrier = SegmentFunctions.ExtractElementFromSegment("Carrier", segment)
        TrackingNo = SegmentFunctions.ExtractElementFromSegment("Tracking#", segment)

        ShipmentHistory.TRACK_Package(Carrier, TrackingNo)
    End Sub


#Region "Recover Previous Packages"
    Private Sub RecoverPackages_Btn_Click(sender As Object, e As RoutedEventArgs) Handles RecoverPackages_Btn.Click
        RecoverPackages_Popup.IsOpen = True
        POSOptions_Popup.IsOpen = False
        Dim QueryDays = 30



        Dim sql = "SELECT PackageID, [Date], P1, ShipToName, [Tracking#], LBS, Z1, [T1] FROM Manifest WHERE (Manifest.InvoiceNumber) Is Null"
        sql = sql & " And Manifest.[Date] > #" & Today.AddDays(-QueryDays) & "# AND Manifest.[Exported] <> 'Deleted' ORDER BY Manifest.[Date] DESC"

        BindingOperations.ClearAllBindings(RecoverPackages_LV) ' clear binding on ListView
        RecoverPackages_LV.DataContext = Nothing ' remove any rows already in ListView
        Dim DT As New System.Data.DataTable ' datatable to use to populate ListView
        Dim currentGridView As GridView = RecoverPackages_LV.View ' variable to reference current GridView in Users_ListView to set up columns.

        DT.Columns.Add("PackageID")
        DT.Columns.Add("Date", GetType(Date))
        DT.Columns.Add("P1")
        DT.Columns.Add("ShipToName")
        DT.Columns.Add("Tracking#")
        DT.Columns.Add("LBS", GetType(Double))
        DT.Columns.Add("Z1")
        DT.Columns.Add("T1", GetType(Double))
        IO_LoadListView(RecoverPackages_LV, DT, gShipriteDB, sql, 8, "")

        Debug.Print(sql)

    End Sub

    Private Sub Close_RecoverPackagesPopup_Click(sender As Object, e As RoutedEventArgs) Handles Close_RecoverPackagesPopup.Click
        RecoverPackages_Popup.IsOpen = False
    End Sub

    Private Sub RecoverPackage_Btn_Click(sender As Object, e As RoutedEventArgs) Handles RecoverPackage_Btn.Click
        RecoverPackage()

    End Sub

    Private Sub RecoverPackages_LV_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles RecoverPackages_LV.MouseDoubleClick
        RecoverPackage()
    End Sub

    Private Sub RecoverPackage()
        Try
            If RecoverPackages_LV.SelectedIndex = -1 Then Exit Sub

            Dim packageID As String
            packageID = RecoverPackages_LV.SelectedItem(0)

            Write_Shipment_To_POS(packageID)
            CalculateInvoice()
            RecoverPackages_Popup.IsOpen = False

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error.")
            RecoverPackages_Popup.IsOpen = False
        End Try
    End Sub

    Private Sub NoSale_Btn_Click(sender As Object, e As RoutedEventArgs) Handles NoSale_Btn.Click
        Try
            FireDrawer()
            RecoverPackages_Popup.IsOpen = False

            Dim SQL = "INSERT INTO Void (InvNum, TotalSale, [Date], OriginalDate, OriginalClerk, VoidingClerk, Reason, AuthorizingClerk) " &
"VALUES ('New Sale', 0, #" & Today.Date & "#, #" & Today.Date & "#, '" & Clerk_Lbl.Content & "', '" & Clerk_Lbl.Content & "', 'No Sale', '" & Clerk_Lbl.Content & "')"

            IO_UpdateSQLProcessor(gShipriteDB, SQL)

            NewSale()

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error recording No Sale event.")
            RecoverPackages_Popup.IsOpen = False
        End Try
    End Sub


    Public Shared Sub FireDrawer()
        Try
            Dim pName As String = GetPolicyData(gReportsDB, "InvoicePrinter")
            If pName = "" Then
                pName = _Printers.Get_DefaultPrinter()
            End If
            Dim pSettings As New PrintHelper
            pSettings.PrintJobName = "ShipRite No Sale"
            pSettings.FireDrawerCode = GetPolicyData(gReportsDB, "InvoiceDrawer")

            _PrintReceipt.Print_FromText("", pName, pSettings)

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Opening Drawer.")

        End Try
    End Sub

#End Region
End Class
