Imports System.IO

Public Class POS_Payment
    Inherits CommonWindow
    Private IsGiftPayment As Boolean
    Private isBulkPayment As Boolean
    Private POSInvoiceStatus As String
    Private CreditList As List(Of CreditPaymentItem)
    Private UsingCardOnFile As Boolean

    Public Sub New(ByVal callingWindow As Window, Optional InvoiceStatus As String = "")

        MyBase.New(callingWindow)

        ' This call is required by the designer.
        InitializeComponent()
        POSInvoiceStatus = InvoiceStatus

    End Sub

    Public Sub New()

        MyBase.New()

        ' This call is required by the designer.
        InitializeComponent()

    End Sub

    Private Sub POS_Window_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Try


            Dim SQL As String = ""
            Dim Segment As String = ""
            gReceiptCCEndBlock = ""
            BalanceDue.Content = gGrandTotal.ToString("$ 0.00")
            InvoiceBalance_Lbl.Content = BalanceDue.Content
            Payments_LB.ItemsSource = gPM.NewPayments

            PostingDate.Text = Today.ToString("MM/dd/yyyy")
            Payment_Entry.Focus()

            Email_TxtBox.Text = ExtractElementFromSegment("Email", gCustomerSegment, "")
            If Email_TxtBox.Text <> "" Then EmailReceipt_Button.RaiseEvent(New RoutedEventArgs(Button.ClickEvent))

            If gResult = "GIFT CARD IN EFFECT" Then
                IsGiftPayment = True
            Else
                IsGiftPayment = False
            End If

            If isARAccount() = True Then

                ARCharge_TxtBx.Visibility = Visibility.Visible
                Load_AR_Credits()
                If CreditList.Count > 0 Then
                    ApplyCredit_Button.Visibility = Visibility.Visible
                Else
                    ApplyCredit_Button.Visibility = Visibility.Hidden
                End If
                CardOnFileBTN.Visibility = Visibility.Visible
                SQL = "SELECT VaultToken FROM AR WHERE AcctNum = '" & ExtractElementFromSegment("AR", gCustomerSegment) & "'"
                Segment = IO_GetSegmentSet(gShipriteDB, SQL)
                If ExtractElementFromSegment("VaultToken", Segment, "") = "" Then

                    CardOnFileBTN.Visibility = Visibility.Hidden
                    CardOnFileBTN.IsEnabled = False
                    UseCardOnFile.Visibility = Visibility.Hidden
                    UseCardOnFile.IsEnabled = False

                End If

            Else

                ARCharge_TxtBx.Visibility = Visibility.Collapsed
                ApplyCredit_Button.Visibility = Visibility.Hidden
                CardOnFileBTN.Visibility = Visibility.Hidden
                CardOnFileBTN.IsEnabled = False
                UseCardOnFile.Visibility = Visibility.Hidden
                UseCardOnFile.IsEnabled = False

            End If

            If isARAccount() And gGrandTotal = 0 And gROAinEffect = False Then
                isBulkPayment = True
                AR_Invoices_GroupBox.Visibility = Visibility.Visible

                Load_AR_Invoices()
                Payment_Entry.IsEnabled = False
                CheckAmount.IsEnabled = False
                CC_PaymentAmount.IsEnabled = False
            Else
                isBulkPayment = False
                AR_Invoices_GroupBox.Visibility = Visibility.Hidden

            End If

            If isBulkPayment Then
                QuickCashEntry_GroupBox.Visibility = Visibility.Hidden
            End If



            Check_SmartSwiperSetup()

            InvoiceBalance_Lbl.Visibility = Visibility.Hidden
            RemainingBalance_Lbl.Visibility = Visibility.Hidden

            If gROAinEffect = True Then

                ExactCash.Visibility = Visibility.Hidden
                '            OtherPayment_Button.Visibility = Visibility.Hidden
                GiftCard_Button.Visibility = Visibility.Hidden
                ApplyCredit_Button.Visibility = Visibility.Hidden

            End If

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error.")
        End Try
    End Sub

    Private Sub Check_SmartSwiperSetup()
        Try
            If GetPolicyData(gShipriteDB, "MerchantWare") = False Then Exit Sub

            Dim SegmentSet As String = ""

            gSSDBpath = GetWinINI("SmartSwiper", "C:\windows\SmartSwiper.ini", "", "DataPath")
            gSSAppPath = GetWinINI("SmartSwiper", "C:\windows\SmartSwiper.ini", "", "ApplicationPath")


            If gSSDBpath <> "" Then

                '    Cannot do this without screwing up Current Shiprite
                '               IO_UpdateSQLProcessor(gSSAppPath & "\Reports.mdb", "UPDATE SETUP SET EnableRemoteControl = True")

                SegmentSet = ExtractElementFromSegment("UserID", IO_GetSegmentSet(gSSDBpath & "\SmartSwiper.mdb", "SELECT UserID From Users WHERE UserID='SRN'"), "")
                If SegmentSet = "" Then
                    IO_UpdateSQLProcessor(gSSDBpath & "\SmartSwiper.mdb", "INSERT INTO [Users] ([UserID], [Password], [FullName]) VALUES ('SRN', '222', 'ShipRiteNext')")
                End If
            End If


        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error.")
        End Try

    End Sub

    Private Sub Load_AR_Invoices()
        Try
            Dim SegmentSet As String = ""
            Dim Segment As String = ""
            Dim BulkPayList As List(Of AR_BulkPaymentItem)
            Dim item As AR_BulkPaymentItem

            Dim SQL = "SELECT FIRST([Payments.Date]) AS [Date], InvNum, SUM(IIf([Type] = 'Sale' or [Type]='ADJUST', Charge, 0)) AS InvAmt,
        SUM(Payment) - sum(IIF( [Type] = 'Change', Charge, 0)) AS InvPayment,
        (InvAmt-InvPayment) AS Balance
        FROM Payments
        WHERE Balance > 0 AND Status='Ok' AND AcctNum = '" & ExtractElementFromSegment("AR", gCustomerSegment, "") & "'
        GROUP BY InvNum"

            SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
            BulkPayList = New List(Of AR_BulkPaymentItem)

            Do Until SegmentSet = ""
                item = New AR_BulkPaymentItem

                Segment = GetNextSegmentFromSet(SegmentSet)

                item.InvDate = ExtractElementFromSegment("Date", Segment, "")
                item.InvoiceNo = ExtractElementFromSegment("InvNum", Segment, "")
                item.InvoiceAmount = ExtractElementFromSegment("InvAmt", Segment, "0")
                item.Balance = ExtractElementFromSegment("Balance", Segment, "0")
                item.isPay = False

                BulkPayList.Add(item)
            Loop

            BulkPay_LV.ItemsSource = BulkPayList
            BulkPay_LV.Items.Refresh()

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Loading AR Invoices.")
        End Try
    End Sub

    Private Sub BulkPay_LV_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles BulkPay_LV.SelectionChanged
        Try
            Dim Sum_Balance As Double = 0
            For Each item As AR_BulkPaymentItem In BulkPay_LV.SelectedItems
                Sum_Balance += item.Balance
            Next

            BalanceDue.Content = FormatCurrency(Sum_Balance)
            Payment_Entry.Text = Format(Sum_Balance, "N2")

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error.")
        End Try
    End Sub

    Private Function isARAccount() As Boolean
        If ExtractElementFromSegment("AR", gCustomerSegment, "") = "" Or ExtractElementFromSegment("AR", gCustomerSegment, "") = "CASH" Then
            Return False
        Else
            Return True
        End If
    End Function

    Private Sub Display_NonPaymentErrorMessage()
        MsgBox("No payment applied!" & vbCrLf & "Please enter a payment then complete sale!", vbExclamation, "Cannot Complete Sale!")
    End Sub

    Private Function CompleteSale() As Integer
        Try
            Dim EMail_DisplayName As String
            Dim amt As Double
            amt = ValFix(InvoiceBalance_Lbl.Content)

            If isARAccount() Then
                If isBulkPayment Then
                    'payment on account, check for payment.
                    If gPM.NewPayments.Count = 0 Then
                        Display_NonPaymentErrorMessage()
                        Return 0
                    Else
                        gPM.isBulkPayment = True
                    End If

                ElseIf Not POSManager.Is_NewSale_Quote_Hold(POSInvoiceStatus) Then
                    'payment on recovered invoice
                    If gPM.NewPayments.Count = 0 Then
                        Display_NonPaymentErrorMessage()
                        Return 0
                    End If

                ElseIf gROAinEffect Then
                    If gPM.NewPayments.Count = 0 Then
                        Display_NonPaymentErrorMessage()
                        Return 0
                    End If
                End If
            Else
                'no AR
                If gPM.NewPayments.Count = 0 Then

                    If Payment_Entry.Text <> "" Then
                        'Handles pressing "Complete Sale" while having an amount entered in the payment entry.
                        CashEnterPressClick()
                        Return 0
                    Else
                        Display_NonPaymentErrorMessage()
                        Return 0
                    End If


                ElseIf amt > 0 Then
                    MsgBox("Invoice Underpaid!" & vbCrLf & "Please enter additional payment then complete sale!", vbExclamation, "Cannot Complete Sale!")
                    Return 0
                End If
            End If


            If amt < 0 And IsGiftPayment = False And gROAinEffect = False Then

                gChangeDue = amt * -1

                Dim item As PaymentDefinition = New PaymentDefinition
                item.PostDate = PostingDate.Text
                item.Desc = "Change Paid"
                item.Type = "Change"
                item.Charge = gChangeDue
                gPM.NewPayments.Add(item)

            Else

                gChangeDue = 0

            End If


            If NOReceipt_Button.Tag = "YES" Then
                gPOS_IsPrintReceipt = False 'don't print receipt
            Else
                gPOS_IsPrintReceipt = True 'print receipt
            End If


            If PrintInvoice_Button.Tag = "YES" Then
                gPOS_IsPrintFullSheetInvoice = True 'print 8.5x11 invoice
            Else
                gPOS_IsPrintFullSheetInvoice = False ' don't print invoice
            End If

            If EmailInvoice_Button.Tag = "YES" Then
                gPOS_FullSheetInvoice_Email = Email_TxtBox.Text
            Else
                gPOS_FullSheetInvoice_Email = ""
            End If


            gPOS_EmailReceipt = New Email_POSReceipt
            If EmailReceipt_Button.Tag = "YES" And Email_TxtBox.Text <> "" Then
                gPOS_EmailReceipt.isEmail = True
                gPOS_EmailReceipt.EmailAddress = Email_TxtBox.Text

                EMail_DisplayName = ExtractElementFromSegment("Email", gCustomerSegment, "")
                If EMail_DisplayName = "" Then EMail_DisplayName = "Customer"
                gPOS_EmailReceipt.EmailTemplate = getEmailTemplate("Notify_Email-POSReceipt", EMail_DisplayName)

            Else
                gPOS_EmailReceipt.isEmail = False
            End If


            gPaymentsCompleted = True
            Me.Close()
            Return 0

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error.")
            Return -1
        End Try
    End Function

    Private Function CalculateBalanceAfterPayment() As Double
        Try
            Dim BalanceAfterPayment As Double = 0

            For Each item In gPM.NewPayments
                BalanceAfterPayment += item.Payment
            Next

            TotalPaid.Text = BalanceAfterPayment.ToString("$ 0.00")

            BalanceAfterPayment = Val(FlushOut(BalanceDue.Content, "$", "")) - Round(BalanceAfterPayment, 2)
            InvoiceBalance_Lbl.Content = BalanceAfterPayment.ToString("$ 0.00")

            Payment_Entry.Focus()
            If BalanceAfterPayment <= 0 And IsGiftPayment = False Then
                CompleteSale()
            Else
                InvoiceBalance_Lbl.Visibility = Visibility.Visible
                RemainingBalance_Lbl.Visibility = Visibility.Visible
            End If

            Payment_Entry.Text = ""
            Payment_Entry.Focus()

            Payments_LB.Items.Refresh()

            Return BalanceAfterPayment

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Calculating Invoice Balance.")
            Return 0
        End Try
    End Function

    Private Sub CompleteSale_Button_Click(sender As Object, e As RoutedEventArgs) Handles CompleteSale_Button.Click
        Try

            Dim ret As Integer = 0
             
            ret = CompleteSale()

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub


    Private Sub Receipt_Options_Buttons(sender As Object, e As RoutedEventArgs) Handles NOReceipt_Button.Click, PrintInvoice_Button.Click, EmailReceipt_Button.Click, EmailInvoice_Button.Click, EmailCustomerReview_Button.Click
        Dim currentButton As Button = DirectCast(sender, Button)

        'Tag determines if button is selected.
        If currentButton.Tag = "NO" Then
            currentButton.Tag = "YES"
            'selected button sets lighter background color
            currentButton.Background = New System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(190, 217, 244))            '

        Else
            currentButton.Tag = "NO"
            currentButton.Background = New System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(104, 117, 171))           '

        End If
    End Sub

    Private Sub PrintButton_Click(sender As Object, e As RoutedEventArgs) Handles PrintButton.Click
        Try

            Dim win As New PrinterSetup(Me)
            win.ShowDialog(Me)

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error.")
        End Try
    End Sub

    Private Sub Payment_Entry_PreviewTextInput(sender As Object, e As TextCompositionEventArgs) Handles Payment_Entry.PreviewTextInput
        Try
            If Not Char.IsDigit(CChar(e.Text)) Then e.Handled = True
            If Not Char.IsSeparator(CChar(e.Text)) And Payment_Entry.Text.IndexOf(".") = -1 Then e.Handled = False

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error.")
        End Try
    End Sub

    Private Sub Payments_LB_KeyDown(sender As Object, e As KeyEventArgs) Handles Payments_LB.KeyDown
        Try
            If e.Key = Key.Delete Then

                If Payments_LB.SelectedIndex = -1 Then Exit Sub

                Dim payment As PaymentDefinition = Payments_LB.SelectedItem

                If vbYes = MsgBox("Do you want to remove the selected payment?", vbQuestion + vbYesNo, payment.Desc) Then

                    If payment.Type = "CHARGE" And payment.CC_AuthorizationCode <> "noswipe" And Not String.IsNullOrEmpty(payment.CC_AuthorizationCode) Then
                        MsgBox("Credit Card charge is already approved." & vbCrLf & "Payment cannot be deleted!", vbExclamation)
                        Exit Sub
                    End If

                    gPM.NewPayments.Remove(payment)
                    CalculateBalanceAfterPayment()

                End If

            End If
        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error.")
        End Try

    End Sub


    Private Sub PostingDate_KeyDown(sender As Object, e As Input.KeyEventArgs) Handles PostingDate.KeyDown
        Try
            If e.Key = Key.Return Then

                PostingDate.Text = ReformatDate(PostingDate.Text)
                Payment_Entry.Focus()

            End If

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error.")
        End Try
    End Sub

    Private Sub PostingDate_LostFocus(sender As Object, e As RoutedEventArgs) Handles PostingDate.LostFocus

        PostingDate.Text = ReformatDate(PostingDate.Text)

    End Sub

    Private Sub Keypad_Click(sender As Object, e As RoutedEventArgs) Handles Keypad_1.Click, Keypad_2.Click, Keypad_3.Click, Keypad_4.Click, Keypad_5.Click, Keypad_6.Click, Keypad_7.Click, Keypad_8.Click, Keypad_9.Click, Keypad_0.Click, Keypad_DEL.Click, Keypad_DOT.Click, Keypad_ENTER.Click, Keypad_00.Click
        Try
            If sender.content = "ENTER" Then
                CashEnterPressClick()

            ElseIf sender.content = "DEL" Then
                If Payment_Entry.Text.Length <> 0 Then
                    Payment_Entry.Text = Payment_Entry.Text.Substring(0, Payment_Entry.Text.Length - 1)
                End If
            Else

                Payment_Entry.Text = Payment_Entry.Text & sender.content
            End If

            'Payment_Entry.CaretIndex = Payment_Entry.Text.Length
            'Payment_Entry.Focus()

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error.")
        End Try
    End Sub

#Region "Cash Payments"

    Private Sub QuickCashEntry_Buttons_Click(sender As Object, e As RoutedEventArgs) Handles OneDollar.Click, TwoDollar.Click, FiveDollar.Click, TenDollar.Click, TwentyDollar.Click, FiftyDollar.Click, HundredDollar.Click, ExactCash.Click
        Try
            If sender.tag = "EXACT" Then
                SaveCashPayment(Val(FlushOut(BalanceDue.Content, "$", "")))
            Else

                'if cash is already applied to sale, then find the amount and add to it
                Dim i = gPM.NewPayments.FindIndex(Function(x As PaymentDefinition) x.Type = "CASH")
                Dim amt As Double = 0

                If i <> -1 Then
                    amt = gPM.NewPayments(i).Payment
                End If

                SaveCashPayment(Val(sender.tag) + amt)
            End If

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error.")
        End Try
    End Sub

    Private Sub Payment_Entry_KeyDown(sender As Object, e As Input.KeyEventArgs) Handles Payment_Entry.KeyDown
        Try
            If e.Key = Key.Return Then
                CashEnterPressClick()
            End If

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error.")
        End Try
    End Sub

    Private Sub Cash_Button_Click(sender As Object, e As RoutedEventArgs) Handles Cash_Button.Click
        CashEnterPressClick()
    End Sub

    Private Sub CashEnterPressClick()
        Try
            'CashPaid.Content = ""
            FormatPaymentEntry()
            SaveCashPayment(Val(Payment_Entry.Text))
            Payment_Entry.Text = ""

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error.")
        End Try
    End Sub

    Private Sub SaveCashPayment(amt As Double)
        Try
            Dim i As Integer
            Dim Balance As Double
            Dim BulkInvoiceCount As Integer

            Balance = ValFix(BalanceDue.Content)
            If Balance = 0 And IsGiftPayment = False And gROAinEffect = False Then
                Exit Sub
            End If
            If amt = 0 Then Exit Sub

            If isBulkPayment Then
                'BULK PAYMENT
                BulkInvoiceCount = BulkPay_LV.SelectedItems.Count

                For i = 0 To BulkInvoiceCount - 1
                    Dim item As PaymentDefinition = New PaymentDefinition
                    item.PostDate = PostingDate.Text
                    item.Desc = "Bulk Cash Payment"
                    item.Type = "CASH"
                    item.PaymentDisplay = "CASH Payment"
                    item.InvNum = BulkPay_LV.SelectedItems(i).InvoiceNo
                    item.Payment = BulkPay_LV.SelectedItems(i).Balance
                    gPM.NewPayments.Add(item)

                Next


            Else
                i = gPM.NewPayments.FindIndex(Function(x As PaymentDefinition) x.Type = "CASH")

                If i <> -1 Then
                    'cash exists already in list
                    gPM.NewPayments(i).Payment = amt
                Else
                    'add new
                    Dim item As PaymentDefinition = New PaymentDefinition
                    item.PostDate = PostingDate.Text
                    item.Desc = "Cash Payment"
                    item.Type = "CASH"
                    item.PaymentDisplay = "CASH Payment"
                    item.Payment = amt
                    gPM.NewPayments.Add(item)
                End If
            End If


            ' Must be last
            CalculateBalanceAfterPayment()

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Saving Cash Payment.")
        End Try
    End Sub

#End Region


#Region "Check Payment"
    Private Sub Check_Button_Click(sender As Object, e As RoutedEventArgs) Handles Check_Button.Click

        Dim Balance As Double

        FormatPaymentEntry()
        Balance = ValFix(BalanceDue.Content)
        If Balance = 0 And IsGiftPayment = False And gROAinEffect = False Then

            Exit Sub

        End If

        Try

            Dim CK_BalanceDue As Double = 0
            Dim CK_InvoiceBalance As Double = 0
            Dim CK_PaymentAmount As Double = 0

            CK_BalanceDue = ValFix(BalanceDue.Content)
            CK_InvoiceBalance = ValFix(InvoiceBalance_Lbl.Content)
            CK_PaymentAmount = ValFix(Payment_Entry.Text)

            If CK_PaymentAmount = 0 Then

                CK_PaymentAmount = CK_InvoiceBalance

            End If
            CheckAmount.Text = Format$(CK_PaymentAmount, "$ 0.00")



            CheckPayment_Popup.IsOpen = True
            Payment_Check.Foreground = New System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.DarkRed)

            If ExtractElementFromSegment("Name", gCustomerSegment, "") <> "" And ExtractElementFromSegment("Name", gCustomerSegment, "") <> "Cash, Check, Charge" Then
                NameOnCheck.Text = ExtractElementFromSegment("Name", gCustomerSegment, "")
                CheckNumber.Focus()
            End If

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error.")
        End Try
    End Sub

    Private Sub Check_Cancel_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Check_Cancel_Btn.Click

        Clear_Check_Screen()
    End Sub
    Private Sub Clear_Check_Screen()

        CheckAmount.Text = ""
        NameOnCheck.Text = ""
        CheckNumber.Text = ""
        NameOfBank.Text = ""
        StateOfBank.Text = ""

        CheckPayment_Popup.IsOpen = False
        Payment_Check.Foreground = New System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black)

    End Sub

    Private Sub SaveCheckPayment(sender As Object, e As RoutedEventArgs) Handles SaveCheck_Button.Click
        Try
            Dim CK_Amount As Double = 0
            Dim CK_Number As String = ""
            Dim CK_NameOnCheck As String = ""
            Dim CK_NameOfBank As String = ""
            Dim CK_StateOfBank As String = ""
            Dim BulkInvoiceCount As Integer

            CK_Amount = ValFix(CheckAmount.Text)
            CK_Number = CheckNumber.Text
            CK_NameOnCheck = NameOnCheck.Text
            CK_NameOfBank = NameOfBank.Text
            CK_StateOfBank = StateOfBank.Text

            If isBulkPayment Then
                BulkInvoiceCount = BulkPay_LV.SelectedItems.Count
            Else
                BulkInvoiceCount = 1
            End If

            For i = 0 To BulkInvoiceCount - 1
                Dim item As PaymentDefinition = New PaymentDefinition
                item.PostDate = PostingDate.Text
                item.Type = "CHECK"
                item.Check_Name = CK_NameOnCheck
                item.Check_Number = CK_Number
                item.Check_NameOfBank = CK_NameOfBank
                item.Check_StateOfBank = CK_StateOfBank

                If isBulkPayment Then
                    item.Desc = "Bulk Check: #" & CK_Number
                    item.PaymentDisplay = "Bulk Check: #" & CK_Number
                    item.InvNum = BulkPay_LV.SelectedItems(i).InvoiceNo
                    item.Payment = BulkPay_LV.SelectedItems(i).Balance
                Else
                    item.Desc = "Check Payment"
                    item.PaymentDisplay = "Check: #" & CK_Number
                    item.Payment = CK_Amount
                End If
                gPM.NewPayments.Add(item)
            Next
            Clear_Check_Screen()

            CalculateBalanceAfterPayment()

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error saving Check payment.")
        End Try
    End Sub

#End Region


#Region "Gift Cards"
    Private Sub GiftCard_Button_Click(sender As Object, e As RoutedEventArgs) Handles GiftCard_Button.Click

        NewGiftCard_Popup.IsOpen = True
        GiftCard_Lbl.Foreground = New System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.DarkRed)
        GiftCardNumber.Focus()

    End Sub

    Private Sub NewGiftCard_CloseScreen_Btn_Click(sender As Object, e As RoutedEventArgs) Handles NewGiftCard_CloseScreen_Btn.Click
        NewGiftCard_Popup.IsOpen = False
        GiftCard_Lbl.Foreground = New System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black)
    End Sub

    Private Sub GiftCardNumber_KeyDown(sender As Object, e As KeyEventArgs) Handles GiftCardNumber.KeyDown
        Try
            Dim SQL As String = ""
            Dim SegmentSet As String = ""
            Dim CardBalance As Double = 0
            Dim Deposits As Double = 0
            Dim Disbursements As Double = 0
            Dim InvNum As Long = 0
            Dim PaymentAmount As Double = 0
            Dim BalanceOnInvoice As Double = 0
            Dim amt As Double = 0
            Dim GiftIDNum As String = ""

            If e.Key = Key.Return Then

                If sender.text = "" Then

                    Payment_Entry.Focus()
                    Exit Sub

                End If
                GiftIDNum = sender.text
                NewGiftCard_Popup.IsOpen = False
                GiftCard_Lbl.Foreground = New System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black)

                SQL = "SELECT * FROM GiftRegistry WHERE GiftIDNumber = '" & GiftIDNum & "' AND Status = 'OPEN'"
                SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
                If SegmentSet = "" Then

                    MsgBox("ATTENTION...Card Not Found Or Card is Expired" & vbCrLf & "REPLENISH CARD", vbCritical)
                    GiftCardNumber.Text = ""
                    Payment_Entry.Focus()
                    Exit Sub

                End If
                InvNum = Val(ExtractElementFromSegment("InvNum", SegmentSet))
                SQL = "SELECT SUM(Payment) as Deposits, SUM(Charge) AS Disbursements FROM Payments WHERE InvNum = '" & InvNum & "'"
                SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
                Deposits = Val(ExtractElementFromSegment("Deposits", SegmentSet))
                Disbursements = Val(ExtractElementFromSegment("Disbursements", SegmentSet))
                CardBalance = Round(Deposits - Disbursements, 2)
                If CardBalance = 0 Then

                    MsgBox("ATTENTION...Card Balance is Zero!!!" & vbCrLf & "REPLENISH CARD", vbCritical)
                    GiftCardNumber.Text = ""
                    Payment_Entry.Focus()
                    Exit Sub

                End If
                BalanceOnInvoice = ValFix(InvoiceBalance_Lbl.Content)
                If CardBalance >= BalanceOnInvoice Then

                    PaymentAmount = BalanceOnInvoice

                Else

                    PaymentAmount = CardBalance

                End If
                If Val(gInvoiceNumber) = 0 Then

                    gInvoiceNumber = GetNextInvoiceNumber().ToString

                End If

                Dim item As PaymentDefinition = New PaymentDefinition
                item.PostDate = PostingDate.Text
                item.Desc = "Gift Card Payment"
                item.Type = "GIFT"
                item.PaymentDisplay = "Gift Card"
                item.Payment = PaymentAmount
                gPM.NewPayments.Add(item)

                item = New PaymentDefinition
                item.PostDate = PostingDate.Text
                item.Desc = "Disbursement " & gInvoiceNumber
                item.Type = "GIFT"
                item.Charge = PaymentAmount
                item.AdjustmentInvoiceNumber = InvNum

                gPM.NewPayments.Add(item)

                ' Must be last

                'amt = 0
                'For Each PayItem As PaymentDefinition In gPM.NewPayments

                '    If PayItem.Type = "CHARGE" Then

                '        amt = amt + PayItem.Payment

                '    End If

                'Next
                'If amt = 0 Then

                '    amt = PaymentAmount

                'End If
                'GiftCardPaid.Content = Format(amt, "$ 0.00")

                CalculateBalanceAfterPayment()


            End If

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error processing Gift Card.")
        End Try
    End Sub

#End Region

#Region "Credit Card Payments"

    Private Sub Credit_Button_Click(sender As Object, e As RoutedEventArgs) Handles Credit_Button.Click, CardOnFile_Button.Click

        Dim fnum As Integer = 0
        Dim CCactionComplete As Boolean = False
        Dim ReturnSegment As String = ""
        Dim buf As String = ""
        Dim CC_Payment As Double = 0
        Dim CC_BalanceDue As Double = 0.00
        Dim CC_InvoiceBalance As Double = 0.00
        Dim BulkInvoiceCount As Integer
        'Dim amt As Double = 0
        Dim ret As Long = 0

        ' Dim PaySegment As String = ""
        'Dim i As Integer = 0
        Dim Balance As Double
        gCreditCardSegment = ""

        buf = sender.name
        If Not buf = "CardOnFile_Button" Then

            UsingCardOnFile = False

        Else

            UsingCardOnFile = True
            If UseCardOnFile.IsEnabled = False Then

                Exit Sub

            End If

        End If

        Balance = ValFix(BalanceDue.Content)
        If Balance = 0 And IsGiftPayment = False And gROAinEffect = False Then

            Exit Sub

        End If
        FormatPaymentEntry()

        buf = Dir(gSSAppPath & "\ExternalControlMailBox")
        If buf = "" Then

            Directory.CreateDirectory(gSSAppPath & "\ExternalControlMailBox")

        End If

        CC_BalanceDue = ValFix(BalanceDue.Content)
        CC_InvoiceBalance = ValFix(InvoiceBalance_Lbl.Content)
        CC_Payment = ValFix(Payment_Entry.Text)

        If CC_Payment = 0 Then

            CC_Payment = CC_InvoiceBalance

        End If
        CC_PaymentAmount.Text = Format$(CC_Payment, "$ 0.00")

        CC_Name.Text = ""
        CC_Last4.Text = ""
        CC_ExpireDate.Text = ""
        CC_CardType_LB.SelectedIndex = 4

        If Val(gInvoiceNumber) = 0 Then

            gInvoiceNumber = GetNextInvoiceNumber().ToString

        End If

        Try

            buf = GetPolicyData(gShipriteDB, "MerchantWare")
            If buf = "True" And gSSDBpath <> "" Then
                'SMARTSWIPER ENABLED

                If UsingCardOnFile = False Then

                    fnum = FreeFile()
                    buf = Dir(gSSAppPath & "\ExternalControlMailBox\" & gInvoiceNumber & ".*")
                    Do Until buf = ""

                        FileSystem.Kill(gSSAppPath & "\ExternalControlMailBox\" & buf)
                        buf = Dir()

                    Loop
                    buf = Dir(gSSAppPath & "\ExternalControlMailBox\" & gInvoiceNumber & ".Req")
                    If Not buf = "" Then

                        FileSystem.Kill(gSSAppPath & "\ExternalControlMailBox\" & gInvoiceNumber & ".Req")

                    End If
                    FileOpen(fnum, gSSAppPath & "\ExternalControlMailBox\" & gInvoiceNumber & ".tmp", OpenMode.Output)
                    FileSystem.Print(fnum, "Type Payment" & vbCrLf)
                    FileSystem.Print(fnum, "CurrentUser SRN" & vbCrLf)
                    FileSystem.Print(fnum, "AdminPassword 222" & vbCrLf)
                    FileSystem.Print(fnum, "InvoiceNumber " & gInvoiceNumber & vbCrLf)
                    FileSystem.Print(fnum, "SaleAmount " & Format(CC_Payment, "0.00") & vbCrLf)
                    FileSystem.FileClose(fnum)
                    FileSystem.Rename(gSSAppPath & "\ExternalControlMailBox\" & gInvoiceNumber & ".tmp", gSSAppPath & "\ExternalControlMailBox\" & gInvoiceNumber & ".REQ")

                    If IsProcessRunning("SmartSwiperExpress") = False Then

                        Dim p As New ProcessStartInfo
                        p.FileName = gSSAppPath & "\SmartSwiperExpress.exe"
                        p.WorkingDirectory = System.IO.Path.GetDirectoryName(p.FileName)
                        p.Arguments = "IntegratedSmartSwiper"
                        p.WindowStyle = ProcessWindowStyle.Minimized
                        Process.Start(p)

                    End If

                    PaymentBorder.IsEnabled = False

                    Do Until CCactionComplete = True Or IsProcessRunning("SmartSwiperExpress") = False

                        buf = Dir(gSSAppPath & "\ExternalControlMailBox\" & gInvoiceNumber & ".RES")
                        If Not buf = "" Then

                            CCactionComplete = True

                        End If
                        System.Windows.Forms.Application.DoEvents()

                    Loop

                    PaymentBorder.IsEnabled = True

                    CCactionComplete = False
                    fnum = FreeFile()
                    System.Windows.Forms.Application.DoEvents()
                    Threading.Thread.Sleep(500)
                    Dim FILE_NAME As String = gSSAppPath & "\ExternalControlMailBox\" & gInvoiceNumber & ".RES"
                    If Dir(FILE_NAME) = "" Then

                        MsgBox("CREDIT CARD PROCESSING..." & vbCrLf & vbCrLf & "Cancelled by User", vbInformation, gProgramName)
                        Exit Sub

                    End If
                    Dim objReader As New System.IO.StreamReader(FILE_NAME)

                    ReturnSegment = ""
                    Do While objReader.Peek() <> -1

                        buf = objReader.ReadLine()
                        If InStr(1, buf, "[") = 0 Then

                            ReturnSegment = ReturnSegment & Chr(171) & buf & Chr(187)

                        End If

                    Loop
                    gCreditCardSegment = ReturnSegment
                    'MsgBox(ReturnSegment)
                    objReader.Close()

                Else

                    buf = GENIUS_ProcessVaultPayment(Val(Format(CC_Payment, "0.00")), Val(gSalesTax), gInvoiceNumber, "1001")

                End If

                buf = ExtractElementFromSegment("Result", gCreditCardSegment)
                Dim AuthCode As String = ExtractElementFromSegment("AuthCode", gCreditCardSegment, "")
                'SmartSwiper will on some occasions return a APPROVED result for declined transactions.
                'In that case all other paramaters returned are blank.

                If (buf = "APPROVED" Or buf = "SUCCESS") And AuthCode <> "" Then

                    If isBulkPayment Then
                        BulkInvoiceCount = BulkPay_LV.SelectedItems.Count
                    Else
                        BulkInvoiceCount = 1
                    End If


                    For i = 0 To BulkInvoiceCount - 1

                        Dim item As PaymentDefinition = New PaymentDefinition
                        item.PostDate = PostingDate.Text

                        If ReturnSegment = "" Then

                            ReturnSegment = gCreditCardSegment

                        End If
                        item.Type = "CHARGE"
                        item.CC_Last4 = ExtractElementFromSegment("CardNumber", ReturnSegment)

                        item.CC_AuthorizationCode = ExtractElementFromSegment("AuthCode", ReturnSegment) & "/" & ExtractElementFromSegment("ReferenceID", ReturnSegment)
                        item.CC_ExpDate = ExtractElementFromSegment("ExpirationDate", ReturnSegment)
                        item.CC_CardName = ExtractElementFromSegment("CardHolder", ReturnSegment)
                        item.CC_TypeOfCard = ExtractElementFromSegment("Provider", ReturnSegment)

                        'MsgBox(gCreditCardSegment)
                        If isBulkPayment Then
                            item.Payment = BulkPay_LV.SelectedItems(i).Balance
                            item.Desc = "Bulk Credit Card Payment"
                            item.PaymentDisplay = "Bulk Credit Card  ****" & item.CC_Last4
                            item.InvNum = BulkPay_LV.SelectedItems(i).InvoiceNo
                        Else
                            item.Payment = Val(ExtractElementFromSegment("AuthorizedAmount", ReturnSegment))
                            item.Desc = "Credit Card Payment"
                            item.PaymentDisplay = "Credit Card  ****" & item.CC_Last4
                        End If


                        gPM.NewPayments.Add(item)

                        gReceiptCCEndBlock = gReceiptCCEndBlock & "Card Holder:  " & item.CC_CardName & vbCrLf
                        gReceiptCCEndBlock = gReceiptCCEndBlock & "Card Number:  " & item.CC_Last4 & vbCrLf
                        gReceiptCCEndBlock = gReceiptCCEndBlock & "Trans Type:  " & ExtractElementFromSegment("TransactionType", ReturnSegment) & vbCrLf
                        gReceiptCCEndBlock = gReceiptCCEndBlock & "Auth Code:  " & item.CC_AuthorizationCode.Substring(0, item.CC_AuthorizationCode.IndexOf("/")) & vbCrLf
                        gReceiptCCEndBlock = gReceiptCCEndBlock & "Reference ID:  " & ExtractElementFromSegment("ReferenceID", ReturnSegment) & vbCrLf
                        gReceiptCCEndBlock = gReceiptCCEndBlock & "AID:  " & ExtractElementFromSegment("AID", ReturnSegment) & vbCrLf
                        gReceiptCCEndBlock = gReceiptCCEndBlock & "TSI:  " & ExtractElementFromSegment("TSI", ReturnSegment) & vbCrLf
                        gReceiptCCEndBlock = gReceiptCCEndBlock & "TVR:  " & ExtractElementFromSegment("TVR", ReturnSegment) & vbCrLf
                        gReceiptCCEndBlock = gReceiptCCEndBlock & "APP:  " & ExtractElementFromSegment("APP", ReturnSegment) & vbCrLf
                        gReceiptCCEndBlock = gReceiptCCEndBlock & "IAD:  " & ExtractElementFromSegment("IAD", ReturnSegment) & vbCrLf
                        gReceiptCCEndBlock = gReceiptCCEndBlock & "TID:  " & ExtractElementFromSegment("TID", ReturnSegment) & vbCrLf & vbCrLf & vbCrLf

                    Next

                    CalculateBalanceAfterPayment()

                ElseIf buf = "" Then
                    MsgBox("CREDIT CARD PROCESSING..." & vbCrLf & vbCrLf & "Cancelled by User", vbInformation, gProgramName)
                    Exit Sub

                Else

                    MsgBox("ATTENTION...Credit Card Declined" & vbCrLf & vbCrLf & "Please try another form of payment" & vbCrLf & vbCrLf & ExtractElementFromSegment("DeclinedReason", ReturnSegment), vbExclamation + vbOKOnly)
                    Exit Sub

                End If

                Payment_Entry.Text = ""
                Payment_Entry.Focus()

            Else

                CreditCardPayment_Popup.IsOpen = True

                If ExtractElementFromSegment("FName", gCustomerSegment, "") <> "" Then
                    CC_Name.Text = ExtractElementFromSegment("FName", gCustomerSegment, "") & " " & ExtractElementFromSegment("LName", gCustomerSegment, "")
                End If
                CC_lbl.Foreground = New System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.DarkRed)

                If CC_Name.Text = "" Then
                    CC_Name.Focus()
                Else
                    CC_Last4.Focus()
                End If

            End If

            Payments_LB.Items.Refresh()

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error processing Credit Card.")
        End Try
    End Sub

    Private Sub CreditCardSave(sender As Object, e As RoutedEventArgs) Handles SaveCreditCard_Button.Click
        'SmartSwiper disabled
        Try
            Dim CC_Amount As Double = 0
            Dim CC_Number As String = ""
            Dim CC_NameOnCard As String = ""
            Dim CC_ExpirationDate As String = ""
            Dim CC_TypeOfCard As String = ""
            Dim PaySegment As String = ""
            Dim Ret As Integer
            Dim amt As Double = 0.0
            Dim i As Integer = 0
            Dim BulkInvoiceCount As Integer

            CC_Amount = ValFix(CC_PaymentAmount.Text)
            If CC_Last4.Text.Length > 4 Then
                CC_Number = Strings.Right(CC_Last4.Text, 4)
            Else
                CC_Number = CC_Last4.Text
            End If


            CC_NameOnCard = CC_Name.Text
            CC_ExpirationDate = CC_ExpireDate.Text
            CC_TypeOfCard = CC_CardType_LB.SelectedItem.Content



            If isBulkPayment Then
                BulkInvoiceCount = BulkPay_LV.SelectedItems.Count
            Else
                BulkInvoiceCount = 1
            End If

            For i = 0 To BulkInvoiceCount - 1
                Dim item As PaymentDefinition = New PaymentDefinition
                item.PostDate = PostingDate.Text
                item.Type = "CHARGE"
                item.CC_AuthorizationCode = "noswipe"
                item.CC_ExpDate = CC_ExpirationDate
                item.CC_Last4 = CC_Number
                item.CC_CardName = CC_NameOnCard

                If CC_Number <> "" Then
                    item.PaymentDisplay = "CreditCard  *" & CC_Number
                Else
                    item.PaymentDisplay = "Credit Card"
                End If


                If isBulkPayment Then
                    item.Desc = "Bulk Credit Card Payment"
                    item.InvNum = BulkPay_LV.SelectedItems(i).InvoiceNo
                    item.Payment = BulkPay_LV.SelectedItems(i).Balance
                    item.PaymentDisplay = "Bulk " & item.PaymentDisplay
                Else
                    item.Desc = "Credit Card Payment"
                    item.Payment = CC_Amount
                End If


                gPM.NewPayments.Add(item)
            Next


            ' Must be last

            amt = CalculateBalanceAfterPayment()
            Payment_Entry.Focus()
            If amt <= 0 And IsGiftPayment = False Then

                Ret = CompleteSale()

            End If
            CreditCardPayment_Popup.IsOpen = False
            CC_lbl.Foreground = New System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black)
            Payment_Entry.Text = ""
            Payment_Entry.Focus()

            Payments_LB.Items.Refresh()

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error saving Credit Card payment.")
        End Try
    End Sub

    Private Sub CC_Cancel_Btn_Click(sender As Object, e As RoutedEventArgs) Handles CC_Cancel_Btn.Click
        CreditCardPayment_Popup.IsOpen = False
        CC_lbl.Foreground = New System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black)
    End Sub

#End Region



    Private Sub OtherPayment_Button_Click(sender As Object, e As RoutedEventArgs) Handles OtherPayment_Button.Click
        Try
            Dim Balance As Double
            Dim OtherPayment_Description As String
            Dim OT_BalanceDue As Double = 0
            Dim OT_InvoiceBalance As Double = 0
            Dim OT_PaymentAmount As Double = 0
            Dim BulkInvoiceCount As Integer

            Balance = ValFix(BalanceDue.Content)
            If Balance = 0 And IsGiftPayment = False And gROAinEffect = False Then
                Exit Sub
            End If

            FormatPaymentEntry()

            OT_BalanceDue = ValFix(BalanceDue.Content)
            OT_InvoiceBalance = ValFix(InvoiceBalance_Lbl.Content)
            OT_PaymentAmount = ValFix(Payment_Entry.Text)

            If OT_PaymentAmount = 0 Then

                OT_PaymentAmount = OT_InvoiceBalance

            End If

            Other_Lbl.Foreground = New System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.DarkRed)
            OtherPayment_Description = InputBox("Please enter type of payment!", "Other Payment", "Other")
            Other_Lbl.Foreground = New System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black)

            If OtherPayment_Description = "" Then Exit Sub

            If isBulkPayment Then
                BulkInvoiceCount = BulkPay_LV.SelectedItems.Count
            Else
                BulkInvoiceCount = 1
            End If


            For i = 0 To BulkInvoiceCount - 1
                Dim item As PaymentDefinition = New PaymentDefinition
                item.PostDate = PostingDate.Text
                item.Desc = "Payment: " & OtherPayment_Description
                item.Type = "Other"
                item.PaymentDisplay = "OTHER: " & OtherPayment_Description
                item.OtherText = OtherPayment_Description
                item.Payment = OT_PaymentAmount

                If isBulkPayment Then
                    item.Desc = "Bulk " & item.Desc
                    item.PaymentDisplay = "Bulk " & item.PaymentDisplay
                    item.InvNum = BulkPay_LV.SelectedItems(i).InvoiceNo
                    item.Payment = BulkPay_LV.SelectedItems(i).Balance
                End If

                gPM.NewPayments.Add(item)

            Next

            CalculateBalanceAfterPayment()

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error processing Other payment.")
        End Try
    End Sub

    Private Sub Payment_Entry_LostFocus(sender As Object, e As RoutedEventArgs) Handles Payment_Entry.LostFocus
        FormatPaymentEntry()
    End Sub

    Private Sub FormatPaymentEntry()
        If Payment_Entry.Text = "" Then Exit Sub

        If Payment_Entry.Text.Contains(".") Then
            Exit Sub
        Else
            Payment_Entry.Text = Format(CDbl(Payment_Entry.Text) / 100, "N2")
        End If
    End Sub

#Region "Apply Credit Payment"
    Private Sub Load_AR_Credits()
        Try
            Dim SegmentSet As String
            Dim Segment As String
            Dim Charged As Double = 0
            Dim Paid As Double = 0
            Dim Balance As Double
            Dim Item As CreditPaymentItem
            '       Dim SQL = "SELECT InvNum, SUM(Charge) - SUM(Payment) AS [Balance] FROM Payments Where AcctNum='" & ExtractElementFromSegment("AR", gCustomerSegment, "") & "' and TYPE<>'ADJUST' GROUP BY InvNum HAVING (SUM(Charge) - SUM(Payment)) > 0"

            Dim SQL = "SELECT InvNum, SUM(Charge) AS Charged, SUM(Payment) AS Paid FROM Payments Where AcctNum='" & ExtractElementFromSegment("AR", gCustomerSegment, "") & "'  GROUP BY InvNum"

            SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
            CreditList = New List(Of CreditPaymentItem)

            Do Until SegmentSet = ""

                Segment = GetNextSegmentFromSet(SegmentSet)
                Charged = Val(ExtractElementFromSegment("Charged", Segment))
                Paid = Val(ExtractElementFromSegment("Paid", Segment))
                Balance = Round(Charged - Paid, 2)
                If Balance < 0 Then

                    Item = New CreditPaymentItem

                    Item.InvoiceNo = ExtractElementFromSegment("InvNum", Segment, "")
                    Item.Balance = Balance * -1

                    CreditList.Add(Item)

                End If
            Loop

            Credit_LV.ItemsSource = CreditList
            Credit_LV.Items.Refresh()

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error loading AR credits.")
        End Try
    End Sub

    Private Sub ApplyCredit_Button_Click(sender As Object, e As RoutedEventArgs) Handles ApplyCredit_Button.Click

        Dim Balance As Double = 0
        Balance = ValFix(BalanceDue.Content)
        If Balance = 0 Then Exit Sub

        ApplyCredit_Popup.IsOpen = True
        Credit_LV.Focus()

    End Sub

    Private Sub Credit_Select_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Credit_Select_Btn.Click
        Try
            Dim i As Integer
            Dim Balance As Double
            Dim amt As Double = 0
            Dim InvoiceNumber As String = ""

            ApplyCredit_Popup.IsOpen = False

            Balance = ValFix(BalanceDue.Content)
            If Balance = 0 And IsGiftPayment = False And gROAinEffect = False Then
                Exit Sub
            End If
            If Balance = 0 Then Exit Sub

            'add new
            Dim item As PaymentDefinition = New PaymentDefinition

            If Credit_LV.SelectedItems.Count = 0 Then

                MsgBox("ATTENTION...Not Credit Selected", vbInformation, gProgramName)
                Exit Sub

            End If
            amt = Val(Credit_LV.SelectedItems(0).Balance)
            If amt > Balance Then

                amt = Balance

            End If
            InvoiceNumber = Credit_LV.SelectedItems(0).InvoiceNo
            item.PostDate = PostingDate.Text
            item.Desc = "CR FROM:" & InvoiceNumber
            item.Type = "ADJUST"
            item.PaymentDisplay = "CR Applied: " & InvoiceNumber
            item.Payment = amt
            gPM.NewPayments.Add(item)

            ' Must be last

            CalculateBalanceAfterPayment()

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error processing AR transaction.")
        End Try
    End Sub

    Private Sub Credit_Cancel_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Credit_Cancel_Btn.Click
        ApplyCredit_Popup.IsOpen = False
    End Sub

    Private Sub Credit_LV_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles Credit_LV.MouseDoubleClick
        Credit_Select_Btn_Click(Nothing, Nothing)
    End Sub




#End Region


End Class
