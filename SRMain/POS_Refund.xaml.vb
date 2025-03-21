Imports wgssSTU

Public Class POS_Refund

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

    Public Sub POS_Refund_Loaded() Handles POS_Refund_Window.Loaded
        Dim AR As String = ""
        Dim SQL As String

        Try
            If ExtractElementFromSegment("Name", gCustomerSegment) <> "Cash, Check, Charge" Then
                NameAndAddress.Text = CreateDisplayBlock(gCustomerSegment, True)
                CustomerPhone.Text = ExtractElementFromSegment("Phone", gCustomerSegment)
            End If

            AR = ExtractElementFromSegment("AR", gCustomerSegment, "")

            If AR <> "" And AR <> "CASH" Then
                AccountNo_TxtBx.Text = AR

                SQL = "SELECT AcctName FROM AR WHERE AcctNum = '" & AR & "'"
                AccountName_TxtBx.Text = ExtractElementFromSegment("AcctName", IO_GetSegmentSet(gShipriteDB, SQL), "")
            Else
                AccountName_TxtBx.Text = ""
                AccountNo_TxtBx.Text = ""
                AccountName_TxtBx.Visibility = Visibility.Hidden
                AccountNo_TxtBx.Visibility = Visibility.Hidden
            End If


            Manager_TxtBx.Text = gCurrentUser
            DrawerID_TxtBx.Text = gDrawerID
            OriginalInvoiceNumber.Text = ExtractElementFromSegment("RecoveredInvoiceNumber", gCustomerSegment)
            AmountToRefund.Text = FormatCurrency(ExtractElementFromSegment("RefundAmount", gRefundSegment))
            PostingDate.Text = Format(Today, "MM/dd/yyyy")
            TodaysDate_Lbl.Content = Today.ToString("dddd MMMM dd, yyyy")
            ExplanationForReturn.Focus()

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Loading Refund Info")
        End Try
    End Sub

    Private Sub Refund_Cash_PreviewTextInput(sender As Object, e As TextCompositionEventArgs) Handles Refund_Cash.PreviewTextInput, Refund_Check.PreviewTextInput, Refund_CreditCard.PreviewTextInput, Refund_CreditAccount.PreviewTextInput

        Try
            Dim allowedchars As String = "0123456789.-"
            If allowedchars.IndexOf(CChar(e.Text)) = -1 Then e.Handled = True

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error")
        End Try

    End Sub

    Private Sub Continue_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Continue_Btn.Click

        Dim ramt As Double
        Dim r_cash As Double
        Dim r_check As Double
        Dim r_creditcard As Double
        Dim r_creditaccount As Double
        Dim AdjustOldInvoice As Double
        Dim RefundPaymentItem As PaymentDefinition

        Try

            ramt = ValFix(AmountToRefund.Text)
            r_cash = ValFix(Refund_Cash.Text)
            r_check = ValFix(Refund_Check.Text)
            r_creditcard = ValFix(Refund_CreditCard.Text)
            r_creditaccount = ValFix(Refund_CreditAccount.Text)

            If Not ramt = r_cash + r_check + r_creditcard + r_creditaccount Then

                MsgBox("ATTENTION...Total Allocated to refund must equal refund requested amount." & vbCrLf & vbCrLf & "TRY AGAIN...", vbCritical, gProgramName)
                Exit Sub

            End If

            If Not r_creditaccount = 0 Then
                If AccountNo_TxtBx.Text = "" Then
                    'No AR Account, need to create one
                    If gIsPOSSecurityEnabled AndAlso Check_Current_User_Permission("AR_CreateAccounts") Then
                        If MsgBox("Are you sure you wish to create an AR account for this customer?", 4, "POS Manager") = MsgBoxResult.Yes Then
                            MakeCustomerAccountFromContact(gCustomerSegment)
                            ''-------------> apply account payment
                        Else
                            MsgBox("User " & gCurrentUser & " does not have the permission to process Accounts Receivable transactions!", vbExclamation)
                            Exit Sub
                        End If
                    End If
                Else
                    'Existing AR Account.
                    If gIsPOSSecurityEnabled AndAlso Check_Current_User_Permission("AccountsReceivable", True) Then


                        ''-------------> apply account payment

                    Else
                        MsgBox("User " & gCurrentUser & " does not have the permission to process Accounts Receivable transactions!", vbExclamation)
                        Exit Sub

                    End If
                End If
            End If

            If Not r_creditcard = 0 Then

                Dim returnsegment As String = Apply_CreditCard(OriginalInvoiceNumber.Text, r_creditcard)

                Dim buf As String = ExtractElementFromSegment("AuthCode", returnsegment)
                If Not buf = "" Then

                    RefundPaymentItem = New PaymentDefinition
                    RefundPaymentItem.PostDate = PostingDate.Text
                    RefundPaymentItem.Desc = "Charge Card Refund"
                    RefundPaymentItem.Type = "CHARGE"
                    RefundPaymentItem.Charge = r_creditcard
                    RefundPaymentItem.Payment = 0
                    RefundPaymentItem.CC_Last4 = ExtractElementFromSegment("RightFour", returnsegment)
                    RefundPaymentItem.CC_AuthorizationCode = ExtractElementFromSegment("AuthCode", returnsegment) & "/" & ExtractElementFromSegment("ReferenceID", returnsegment)
                    RefundPaymentItem.CC_CardName = ExtractElementFromSegment("NameOnCard", returnsegment)
                    RefundPaymentItem.CC_TypeOfCard = ExtractElementFromSegment("Provider", returnsegment)
                    gPM.NewPayments.Add(RefundPaymentItem)

                Else

                    buf = buf

                End If

            End If

            If Not r_cash = 0 Then

                RefundPaymentItem = New PaymentDefinition
                RefundPaymentItem.PostDate = PostingDate.Text
                RefundPaymentItem.Desc = "Cash Refund"
                RefundPaymentItem.Type = "CASH"
                RefundPaymentItem.Charge = r_cash
                RefundPaymentItem.Payment = 0
                gPM.NewPayments.Add(RefundPaymentItem)

            End If


            If Not r_check = 0 Then

                RefundPaymentItem = New PaymentDefinition
                RefundPaymentItem.PostDate = PostingDate.Text
                RefundPaymentItem.Desc = "Check Refund"
                RefundPaymentItem.Type = "CHECK"
                RefundPaymentItem.Charge = r_check
                RefundPaymentItem.Payment = 0
                gPM.NewPayments.Add(RefundPaymentItem)

            End If


            AdjustOldInvoice = Val(ExtractElementFromSegment("AdjustOldInvoice", gRefundSegment))
            If Not AdjustOldInvoice = 0 Then

                RefundPaymentItem = New PaymentDefinition

                RefundPaymentItem.PostDate = PostingDate.Text
                RefundPaymentItem.Desc = "Refund Adjustment"
                RefundPaymentItem.Type = "ADJUST"
                RefundPaymentItem.Charge = AdjustOldInvoice
                RefundPaymentItem.Payment = 0
                gPM.NewPayments.Add(RefundPaymentItem)


                RefundPaymentItem = New PaymentDefinition
                RefundPaymentItem.PostDate = PostingDate.Text
                RefundPaymentItem.Desc = "Refund Adjustment"
                RefundPaymentItem.Type = "ADJUST"
                RefundPaymentItem.Charge = 0
                RefundPaymentItem.Payment = AdjustOldInvoice
                RefundPaymentItem.InvNum = ExtractElementFromSegment("ReturnInvoiceNumber", gRefundSegment)
                gPM.NewPayments.Add(RefundPaymentItem)

            End If
            gPaymentsCompleted = True
            gPOS_IsPrintReceipt = True
            Me.Close()

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Processing Refund")
        End Try
    End Sub

    Private Function Apply_CreditCard(InvNum As String, r_creditcard As Double) As String

        Dim buf As String
        Dim RefundPaymentItem As PaymentDefinition

        Try

            buf = GetPolicyData(gShipriteDB, "MerchantWare")
            If buf = "True" Then

                Return GENIUS_processRETURN(InvNum, r_creditcard)

            Else

                RefundPaymentItem = New PaymentDefinition
                RefundPaymentItem.PostDate = PostingDate.Text
                RefundPaymentItem.Desc = "Charge Refund"
                RefundPaymentItem.Type = "CHARGE"
                RefundPaymentItem.Charge = r_creditcard
                RefundPaymentItem.Payment = 0

                gPM.NewPayments.Add(RefundPaymentItem)

                gReceiptCCEndBlock = ""

                Return True

            End If

        Catch ex As Exception

            _MsgBox.ErrorMessage(ex, "Error processing Credit Card Refund.")
            Return False

        End Try

    End Function

    'Private Function Refund_SmartSwiper(r_creditcard As Double) As Boolean

    '    Dim Org_Sale As Double = 0
    '    Dim Org_Payments As Double = 0
    '    Dim Org_Balance As Double = 0

    '    Dim ReturnSegment As String = ""
    '    Dim buf As String = ""
    '    Dim CC_Amount As Double = 0
    '    Dim CC_Number As String = ""
    '    Dim CC_NameOnCard = ""
    '    Dim CC_ExpirationDate As String = ""
    '    Dim CC_TypeOfCard As String = ""
    '    Dim CC_Payment As Double = 0
    '    Dim fnum As Integer = 0
    '    Dim CCactionComplete As Boolean = False
    '    Dim RefundPaymentItem As PaymentDefinition


    '    gSSAppPath = GetWinINI("SmartSwiper", "C:\windows\SmartSwiper.ini", "c:\SmartSwiper", "ApplicationPath")

    '    fnum = FreeFile()
    '    buf = Dir(gSSAppPath & "\ExternalControlMailBox\" & gInvoiceNumber & ".*")
    '    Do Until buf = ""

    '        FileSystem.Kill(gSSAppPath & "\ExternalControlMailBox\" & buf)
    '        buf = Dir()

    '    Loop
    '    buf = Dir(gSSAppPath & "\ExternalControlMailBox\" & gInvoiceNumber & ".Req")
    '    If Not buf = "" Then

    '        FileSystem.Kill(gSSAppPath & "\ExternalControlMailBox\" & gInvoiceNumber & ".Req")

    '    End If
    '    If Val(gInvoiceNumber) = 0 Then

    '        gInvoiceNumber = GetNextInvoiceNumber().ToString

    '    End If
    '    FileOpen(fnum, gSSAppPath & "\ExternalControlMailBox\" & gInvoiceNumber & ".tmp", OpenMode.Output)
    '    FileSystem.Print(fnum, "Type Refund" & vbCrLf)
    '    FileSystem.Print(fnum, "CurrentUser SRN" & vbCrLf)
    '    FileSystem.Print(fnum, "AdminPassword 222" & vbCrLf)
    '    FileSystem.Print(fnum, "InvoiceNumber " & gInvoiceNumber & vbCrLf)
    '    FileSystem.Print(fnum, "OriginalInvoice " & ExtractElementFromSegment("ReturnInvoiceNumber", gRefundSegment) & vbCrLf)
    '    FileSystem.Print(fnum, "RefundAmount " & Format(r_creditcard, "0.00") & vbCrLf)
    '    FileSystem.FileClose(fnum)
    '    FileSystem.Rename(gSSAppPath & "\ExternalControlMailBox\" & gInvoiceNumber & ".tmp", gSSAppPath & "\ExternalControlMailBox\" & gInvoiceNumber & ".REQ")

    '    buf = GetPolicyData(gShipriteDB, "MerchantWare")
    '    If buf = "True" Then

    '        If IsProcessRunning("SmartSwiperExpress") = False Then

    '            Dim p As New ProcessStartInfo
    '            p.FileName = gSSAppPath & "\SmartSwiperExpress.exe"
    '            p.WorkingDirectory = System.IO.Path.GetDirectoryName(p.FileName)
    '            p.Arguments = "IntegratedSmartSwiper"
    '            p.WindowStyle = ProcessWindowStyle.Minimized
    '            Process.Start(p)

    '        End If

    '    End If

    '    Do Until CCactionComplete = True Or IsProcessRunning("SmartSwiperExpress") = False

    '        buf = Dir(gSSAppPath & "\ExternalControlMailBox\" & gInvoiceNumber & ".RES")
    '        If Not buf = "" Then

    '            CCactionComplete = True

    '        End If
    '        System.Windows.Forms.Application.DoEvents()

    '    Loop
    '    CCactionComplete = False
    '    fnum = FreeFile()
    '    System.Windows.Forms.Application.DoEvents()
    '    Threading.Thread.Sleep(500)
    '    Dim FILE_NAME As String = gSSAppPath & "\ExternalControlMailBox\" & gInvoiceNumber & ".RES"
    '    If Dir(FILE_NAME) = "" Then

    '        MsgBox("CREDIT CARD PROCESSING..." & vbCrLf & vbCrLf & "Cancelled by User", vbInformation, gProgramName)
    '        Return False

    '    End If
    '    Dim objReader As New System.IO.StreamReader(FILE_NAME)

    '    ReturnSegment = ""
    '    Do While objReader.Peek() <> -1

    '        buf = objReader.ReadLine()
    '        If InStr(1, buf, "[") = 0 Then

    '            ReturnSegment = ReturnSegment & Chr(171) & buf & Chr(187)

    '        End If

    '    Loop
    '    objReader.Close()
    '    buf = ExtractElementFromSegment("Result", ReturnSegment)

    '    If buf = "APPROVED" Then

    '        CC_Amount = Val(ExtractElementFromSegment("AuthorizedAmount", ReturnSegment))
    '        'CC_Number = CC_Last4.Text
    '        'CC_NameOnCard = CC_Name.Text
    '        'CC_ExpirationDate = CC_ExpireDate.Text
    '        'CC_TypeOfCard = CC_CardType.Text

    '        RefundPaymentItem = New PaymentDefinition
    '        RefundPaymentItem.PostDate = PostingDate.Text
    '        RefundPaymentItem.Desc = "Charge Refund"
    '        RefundPaymentItem.Type = "CHARGE"
    '        RefundPaymentItem.Charge = r_creditcard
    '        RefundPaymentItem.Payment = 0

    '        gPM.NewPayments.Add(RefundPaymentItem)

    '        gReceiptCCEndBlock = ""

    '        gReceiptCCEndBlock = gReceiptCCEndBlock & "Card Holder:  " & ExtractElementFromSegment("CardHolder", ReturnSegment) & vbCrLf
    '        gReceiptCCEndBlock = gReceiptCCEndBlock & "Card Number:  " & ExtractElementFromSegment("CardNumber", ReturnSegment) & vbCrLf
    '        gReceiptCCEndBlock = gReceiptCCEndBlock & "Trans Type:  " & ExtractElementFromSegment("TransactionType", ReturnSegment) & vbCrLf
    '        gReceiptCCEndBlock = gReceiptCCEndBlock & "Auth Code:  " & ExtractElementFromSegment("AuthCode", ReturnSegment) & vbCrLf
    '        gReceiptCCEndBlock = gReceiptCCEndBlock & "Reference ID:  " & ExtractElementFromSegment("ReferenceID", ReturnSegment) & vbCrLf

    '        Return True

    '    Else
    '        Return False
    '    End If
    'End Function

    Private Sub Refund_TextBox_GotFocus(sender As Object, e As RoutedEventArgs) Handles Refund_Cash.GotFocus, Refund_Check.GotFocus, Refund_CreditAccount.GotFocus, Refund_CreditCard.GotFocus
        If Refund_Cash.Text = "" And Refund_Check.Text = "" And Refund_CreditCard.Text = "" And Refund_CreditAccount.Text = "" Then
            sender.text = AmountToRefund.Text
        End If

    End Sub

End Class
