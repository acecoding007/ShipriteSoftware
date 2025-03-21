'Imports System.Text
'Imports CommonCode
'Imports CommonShip
'Imports DbCode
'Imports EmailNotification
'Imports Microsoft.VisualBasic

'Public Module Receipt_HTML

'    Public ReceiptOnOffOptions As String
'    Public ReceiptIsShipAddrOn As Boolean
'    Public Const PREFIX_NAME As String = "to: "
'    Public Const PREFIX_ADDR As String = "st: "
'    Public Const PREFIX_CITY As String = "ct: "
'    Public qq As Char = Microsoft.VisualBasic.ChrW(34) ' double quote

'#Region "POS Receipt"
'    Public Function Email_POS_HTMLReceipt_FromObject(ByVal receipt As Object, ByVal emailTo As Object) As Boolean
'        Email_POS_HTMLReceipt_FromObject = False ' assume.
'        Dim sb As New StringBuilder
'        Call Load_ReceiptOnOffOptions()
'        Call build_HTML_Receipt(receipt, sb)
'        '_Debug.Print_(sb.ToString)
'        Dim EmailPackages As New Collection
'        For i As Integer = 1 To emailTo.Count
'            Dim epack As New _EmailPackage
'            epack.EmailTo = emailTo(i)
'            epack.HTMLBody = sb.ToString
'            EmailPackages.Add(epack)
'        Next
'        Email_POS_HTMLReceipt_FromObject = _EmailSetup.Send_HTMLEmail(EmailPackages, "Receipt")
'    End Function
'    Public Function Email_POS_HTMLReceipt_FromDb(ByVal invoiceNo As Long, ByVal emailTo As Object) As Boolean
'        Email_POS_HTMLReceipt_FromDb = False ' assume.
'        Dim sb As New StringBuilder
'        Dim receipt As New _basePOS_Receipt(invoiceNo)
'        Call Load_ReceiptOnOffOptions()
'        Call Read_Receipt_FromDb(receipt)
'        Call build_HTML_Receipt(receipt, sb)
'        '_Debug.Print_(sb.ToString)
'        Dim EmailPackages As New Collection
'        For i As Integer = 1 To emailTo.Count
'            Dim epack As New _EmailPackage
'            epack.EmailTo = emailTo(i)
'            epack.HTMLBody = sb.ToString
'            EmailPackages.Add(epack)
'        Next
'        Email_POS_HTMLReceipt_FromDb = Send_HTMLEmail(EmailPackages, "Receipt")
'    End Function

'    Public Sub Read_Receipt_FromDb(ByRef receipt As _basePOS_Receipt)
'        Call read_ReceiptHeader_FromDB(receipt)
'        Call read_ReceiptItems_FromDB(receipt)
'        Call read_ReceiptBottom(receipt)
'    End Sub
'    Private Sub build_HTML_Receipt(ByRef receipt As _basePOS_Receipt, ByRef sb As StringBuilder)
'        Call build_Header(sb)
'        Call build_StoreOwner(sb)
'        Call build_ReceiptHeader(receipt, sb)
'        Call build_ReceiptItems(receipt, sb)
'        Call build_ReceiptTotals(receipt, sb)
'        Call build_PaymentType(receipt, sb)
'        Call build_End(receipt, sb)
'    End Sub
'    Private Sub build_Header(ByRef sb As StringBuilder)
'        sb.AppendLine("<html><head>")
'        sb.AppendLine(String.Format("<meta http-equiv={0}Content-Type{0} content={0}text/html; charset=us-ascii{0}/>", qq))
'        sb.AppendLine(String.Format("<style type={0}text/css{0}>", qq))
'        sb.AppendLine(" .style1 {height: 17px;} </style></head><body>")
'        'sb.AppendLine("<img src={0}cid:MyLogo{0}/>")
'        sb.AppendLine(String.Format("<table style={0}border:1px solid #94a6b5; font-size: 8pt; font-family:Verdana,arial,helvetica,sans-serif;{0}>", qq))
'    End Sub
'    Private Sub build_StoreOwner(ByRef sb As StringBuilder)
'        Dim StoreOwner As New _baseContact
'        Call ShipRiteDb.Setup_GetAddress_StoreOwner(StoreOwner)
'        sb.AppendLine(String.Format("<tr><th class={0}style1{0}></th><td colspan={0}4{0}><font color={0}#003366{0} size={0}2{0}><b>{1}</b></font></td></tr>", qq, StoreOwner.CompanyName))
'        sb.AppendLine(String.Format("<tr><th class={0}style1{0}></th><td colspan={0}4{0}><font color={0}#003366{0} size={0}2{0}><b>{1}</b></font></td></tr>", qq, StoreOwner.Addr1))
'        If Not String.IsNullOrEmpty(StoreOwner.Addr2) Then
'            sb.AppendLine(String.Format("<tr><th class={0}style1{0}></th><td colspan={0}4{0}><font color={0}#003366{0} size={0}2{0}><b>{1}</b></font></td></tr>", qq, StoreOwner.Addr2))
'        End If
'        sb.AppendLine(String.Format("<tr><th class={0}style1{0}></th><td colspan={0}4{0}><font color={0}#003366{0} size={0}2{0}><b>{1}</b></font></td></tr>", qq, StoreOwner.CityStateZip))
'        sb.AppendLine(String.Format("<tr><th class={0}style1{0}></th><td colspan={0}4{0}><font color={0}#003366{0} size={0}2{0}><b>{1}</b></font></td></tr>", qq, StoreOwner.Tel))

'        Call draw_Line_Size4(sb)
'    End Sub
'    Private Sub build_ReceiptHeader(ByVal receipt As _basePOS_Receipt, ByRef sb As StringBuilder)
'        With receipt
'            sb.AppendLine(String.Format("<tr><th class={0}style1{0}><font color={0}#003366{0}>Invoice#</font></th><td colspan={0}4{0}>{1}</td></tr>", qq, .InvoiceNumb.ToString))
'            sb.AppendLine(String.Format("<tr><th class={0}style1{0}><font color={0}#003366{0}>Clerk</font></th><td colspan={0}4{0}>{1}</td></tr>", qq, .Clerk))
'            sb.AppendLine(String.Format("<tr><th class={0}style1{0}><font color={0}#003366{0}>Date</font></th><td colspan={0}4{0}>{1}</td></tr>", qq, .InvoiceDate.ToString("MM/dd/yyyy hh:mm tt")))
'            If "Cash" = .AccountNumb Then
'                sb.AppendLine(String.Format("<tr><th class={0}style1{0}><font color={0}#003366{0}>{1}</font></th><td colspan={0}4{0}>{2}</td></tr>", qq, String.Empty, .AccountName))
'            Else
'                sb.AppendLine(String.Format("<tr><th class={0}style1{0}><font color={0}#003366{0}>{1}</font></th><td colspan={0}4{0}>{2}<br/>{3}</td></tr>", qq, "Account", .AccountNumb, .AccountName))
'            End If
'        End With
'        Call draw_Line_Size4(sb)
'        sb.AppendLine(String.Format("<tr><th><font color={0}#003366{0}>Description<br/>SKU#</font></th><th><font color={0}#003366{0}>Price</font></th><th><font color={0}#003366{0}>Qty</font></th><th><font color={0}#003366{0}>Ext. Price</font></th></tr>", qq))
'        Call draw_Line_Size3(sb)
'    End Sub
'    Private Sub build_ReceiptItems(ByVal receipt As _basePOS_Receipt, ByRef sb As StringBuilder)
'        For i As Integer = 1 To receipt.Items.Count
'            Dim item As _basePOS_ReceiptItem = receipt.Items(i)
'            With item
'                If "NOTE" = .ItemSKU Then
'                    If print_ReceiptOnOffOptions(.ItemDesc) Then
'                        sb.AppendLine("<tr>")
'                        sb.AppendLine(String.Format("<td width={0}200{0} align={0}center{0} class={0}style1{0}>{1}</td>", qq, .ItemDesc))
'                        sb.AppendLine(String.Format("<td width={0}80{0} align={0}center{0} class={0}style1{0}></td>", qq))
'                        sb.AppendLine(String.Format("<td width={0}80{0} align={0}center{0} class={0}style1{0}></td>", qq))
'                        sb.AppendLine(String.Format("<td width={0}100{0} align={0}center{0} class={0}style1{0}></td>", qq))
'                        sb.AppendLine("</tr>")
'                    End If
'                ElseIf "MEMO" = .ItemSKU.ToUpper.Trim Then ''AP(04/11/2019) - Updated POS emailed receipt to only print memo text without "MEMO" SKU text.
'                    sb.AppendLine("<tr>")
'                    sb.AppendLine(String.Format("<td width={0}200{0} align={0}center{0} class={0}style1{0}>{1}</td>", qq, .ItemDesc))
'                    sb.AppendLine(String.Format("<td width={0}80{0} align={0}center{0} class={0}style1{0}></td>", qq))
'                    sb.AppendLine(String.Format("<td width={0}80{0} align={0}center{0} class={0}style1{0}></td>", qq))
'                    sb.AppendLine(String.Format("<td width={0}100{0} align={0}center{0} class={0}style1{0}></td>", qq))
'                    sb.AppendLine("</tr>")
'                Else
'                    sb.AppendLine("<tr>")
'                    sb.AppendLine(String.Format("<td width={0}200{0} align={0}center{0} class={0}style1{0}>{1}<br/>{2}</td>", qq, .ItemDesc, .ItemSKU))
'                    sb.AppendLine(String.Format("<td width={0}80{0} align={0}center{0} class={0}style1{0}>{1}</td>", qq, .ItemPrice.ToString("F")))
'                    sb.AppendLine(String.Format("<td width={0}80{0} align={0}center{0} class={0}style1{0}>{1}</td>", qq, .ItemQty))
'                    If .ItemIsTaxable Then
'                        sb.AppendLine(String.Format("<td width={0}100{0} align={0}center{0} class={0}style1{0}>{1} t</td>", qq, .ItemExtPrice.ToString("F")))
'                    Else
'                        sb.AppendLine(String.Format("<td width={0}100{0} align={0}center{0} class={0}style1{0}>{1}</td>", qq, .ItemExtPrice.ToString("F")))
'                    End If
'                    sb.AppendLine("</tr>")
'                End If
'            End With
'        Next

'        Call draw_Line_Size4(sb)
'    End Sub
'    Private Sub build_ReceiptTotals(ByVal receipt As _basePOS_Receipt, ByRef sb As StringBuilder)
'        ''AP(04/10/2018) - US Origin: Emailed POS receipt shows "HST Tax" instead of "Sale Tax".
'        Dim StoreOwner As New _baseContact
'        Call ShipRiteDb.Setup_GetAddress_StoreOwner(StoreOwner)
'        With receipt
'            Call build_ReceiptTotal("Sub-Total:", .SubTotal.ToString("C"), sb)
'            ''AP(04/10/2018) - US Origin: Emailed POS receipt shows "HST Tax" instead of "Sale Tax".
'            ''  ''ol#1.2.54(6/28)... Canada receipts will have HST, PST, GST sales tax break-down.
'            ''  ''  Call build_ReceiptTotal("Sale Tax:", .SalesTax.ToString("C"), sb)
'            ''  If .SalesTax1 > 0 Or .SalesTax2 > 0 Or .SalesTax3 > 0 Then
'            If StoreOwner.Country.ToUpper = "CANADA" Or StoreOwner.Country.ToUpper = "CA" Or StoreOwner.CountryCode.ToUpper = "CA" Then
'                If .SalesTax1 > 0 Then Call build_ReceiptTotal("HST Tax:", .SalesTax1.ToString("C"), sb)
'                If .SalesTax2 > 0 Then Call build_ReceiptTotal("PST Tax:", .SalesTax2.ToString("C"), sb)
'                If .SalesTax3 > 0 Then Call build_ReceiptTotal("GST Tax:", .SalesTax3.ToString("C"), sb)
'            Else
'                Call build_ReceiptTotal("Sale Tax:", .SalesTax.ToString("C"), sb)
'            End If
'            If .ServiceTax > 0 Then
'                Call build_ReceiptTotal("Service Tax:", .ServiceTax.ToString("C"), sb)
'            End If
'            Call draw_TotalLine(sb)
'            Call build_ReceiptTotal("Total:", .Total.ToString("C"), sb)
'        End With
'    End Sub

'    Private Sub draw_TotalLine(ByRef sb As StringBuilder)
'        sb.AppendLine("<tr>")
'        sb.AppendLine(String.Format("<td width={0}150{0} align={0}center{0} class={0}style1{0}></td>", qq))
'        sb.AppendLine(String.Format("<td width={0}80{0} align={0}center{0} class={0}style1{0}></td>", qq))
'        sb.AppendLine(String.Format("<td width={0}90{0} align={0}right{0} class={0}style1{0}></td>", qq))
'        sb.AppendLine(String.Format("<td width={0}100{0} align={0}center{0} class={0}style2{0}><font color={0}#003366{0} size={0}3{0}>-------</font></td>", qq))
'        sb.AppendLine("</tr>")
'    End Sub
'    Private Sub draw_Line_Size3(ByRef sb As StringBuilder)
'        sb.AppendLine(String.Format("<tr><th colspan={0}4{0}><font color={0}#003366{0} size={0}3{0}>------------------------------------------------------</font></th></tr>", qq))
'    End Sub
'    Private Sub draw_Line_Size4(ByRef sb As StringBuilder)
'        sb.AppendLine(String.Format("<tr><th colspan={0}4{0}><font color={0}#003366{0} size={0}4{0}>------------------------------------------------</font></th></tr>", qq))
'    End Sub
'    Private Sub build_OneColumn_Row(ByVal message As String, ByRef sb As StringBuilder)
'        sb.AppendLine(String.Format("<tr><td align={0}center{0} class={0}style1{0} colspan={0}4{0}>{1}</td></tr>", qq, message))
'    End Sub

'    Private Sub build_ReceiptTotal(ByVal totalType As String, ByVal totalValue As String, ByRef sb As StringBuilder)
'        sb.AppendLine("<tr>")
'        sb.AppendLine(String.Format("<td width={0}150{0} align={0}center{0} class={0}style1{0}></td>", qq))
'        sb.AppendLine(String.Format("<td width={0}80{0} align={0}center{0} class={0}style1{0}></td>", qq))
'        sb.AppendLine(String.Format("<td width={0}90{0} align={0}center{0} class={0}style2{0}><font color={0}#003366{0}><b>{1}</b></font></td>", qq, totalType))
'        sb.AppendLine(String.Format("<td width={0}100{0} align={0}center{0} class={0}style1{0}>{1}</td>", qq, totalValue))
'        sb.AppendLine("</tr>")
'    End Sub
'    Private Sub build_PaymentType(ByVal receipt As _basePOS_Receipt, ByRef sb As StringBuilder)
'        ''AP(08/22/2019) - Updated POS emailed receipt to show multiple payments applied to invoice.
'        Dim pPayment As New _basePOS_PaymentItem
'        Dim isPaymentAdded As Boolean = False
'        '
'        If receipt.IsCash Then
'            pPayment = receipt.Payments.Find(Function(p) p.IsCash = True)
'            If pPayment IsNot Nothing Then
'                Call draw_Line_Size3(sb)
'                Call build_ReceiptTotal("Cash Tendered:", pPayment.ItemPaymentAmt.ToString("C"), sb)
'                isPaymentAdded = True
'            End If
'        End If
'        If receipt.IsCheck Then
'            pPayment = receipt.Payments.Find(Function(p) p.IsCheck = True)
'            If pPayment IsNot Nothing Then
'                If Not isPaymentAdded Then Call draw_Line_Size3(sb)
'                Call build_ReceiptTotal("Check Tendered:", pPayment.ItemPaymentAmt.ToString("C"), sb)
'                'Call draw_Line_Size4(sb)
'                Call build_OneColumn_Row("Check#: " & pPayment.CheckOrCCard_Numb, sb)
'                Call build_OneColumn_Row(pPayment.CheckOrCCard_Name, sb)
'                isPaymentAdded = True
'            End If
'        End If
'        If receipt.IsCredit Then
'            pPayment = receipt.Payments.Find(Function(p) p.IsCredit = True)
'            If pPayment IsNot Nothing Then
'                If Not isPaymentAdded Then Call draw_Line_Size3(sb)
'                Call build_ReceiptTotal("CCard Charge:", pPayment.ItemPaymentAmt.ToString("C"), sb)
'                'Call draw_Line_Size4(sb)
'                Call build_OneColumn_Row(pPayment.CheckOrCCard_Numb, sb)
'                Call build_OneColumn_Row(pPayment.CheckOrCCard_Name, sb)
'                Call build_OneColumn_Row("I agree to pay the above amount according to the card user agreement.<br/> (merchant agreement if credit voucher)", sb)
'                isPaymentAdded = True
'            End If
'        End If
'        If receipt.IsOther Then
'            If Not isPaymentAdded Then Call draw_Line_Size3(sb)
'            pPayment = receipt.Payments.Find(Function(p) p.IsOther = True)
'            If pPayment IsNot Nothing Then
'                Call build_ReceiptTotal("Other:", pPayment.ItemPaymentAmt.ToString("C"), sb)
'                isPaymentAdded = True
'            End If
'        End If
'        If receipt.IsRefund Then
'            If Not isPaymentAdded Then Call draw_Line_Size3(sb)
'            pPayment = receipt.Payments.Find(Function(p) p.IsRefund = True)
'            If pPayment IsNot Nothing Then
'                Call build_ReceiptTotal("Refund:", pPayment.ItemPaymentAmt.ToString("C"), sb)
'            End If
'        End If
'        If receipt.IsChange Then
'            Call draw_Line_Size3(sb)
'            pPayment = receipt.Payments.Find(Function(p) p.IsChange = True)
'            If pPayment IsNot Nothing Then
'                Call build_ReceiptTotal("Change Due:", pPayment.ItemPaymentAmt.ToString("C"), sb)
'            End If
'        End If
'        'If receipt.IsCash Then
'        '    Call draw_Line_Size3(sb)
'        '    Call build_ReceiptTotal("Cash Tendered:", receipt.CustomerPaid.ToString("C"), sb)
'        '    ''AP(09/12/2016) - NET: Emailed receipt for Payment on Account shows Change Due when it shouldn't.
'        '    If (receipt.Total - receipt.CustomerPaid) > 0 Then
'        '        Call build_ReceiptTotal("Change Due:", (receipt.Total - receipt.CustomerPaid).ToString("C"), sb)
'        '    End If
'        'ElseIf receipt.IsCheck Then
'        '    Call draw_Line_Size3(sb)
'        '    Call build_ReceiptTotal("Check Tendered:", receipt.CustomerPaid.ToString("C"), sb)
'        '    Call draw_Line_Size4(sb)
'        '    Call build_OneColumn_Row("Check#: " & receipt.CheckOrCCard_Numb, sb)
'        '    Call build_OneColumn_Row(receipt.CheckOrCCard_Name, sb)
'        'ElseIf receipt.IsCredit Then
'        '    Call draw_Line_Size3(sb)
'        '    Call build_ReceiptTotal("CCard Charge:", receipt.CustomerPaid.ToString("C"), sb)
'        '    Call draw_Line_Size4(sb)
'        '    Call build_OneColumn_Row(receipt.CheckOrCCard_Numb, sb)
'        '    Call build_OneColumn_Row(receipt.CheckOrCCard_Name, sb)
'        '    Call build_OneColumn_Row("I agree to pay the above amount according to the card user agreement.<br/> (merchant agreement if credit voucher)", sb)
'        'ElseIf receipt.IsRefund Then
'        '    Call build_ReceiptTotal("Refund:", receipt.CustomerPaid.ToString("C"), sb)
'        'ElseIf receipt.IsOther Then
'        '    Call build_ReceiptTotal("Other:", receipt.CustomerPaid.ToString("C"), sb)
'        'End If
'    End Sub

'#Region "Read From Db"
'    Private Sub read_ReceiptHeader_FromDB(ByRef receipt As _basePOS_Receipt)
'        Dim dreader As OleDb.OleDbDataReader = Nothing
'        Dim rec As Integer = 1
'        If ShipRiteDb.Payments_GetInvoiceData(receipt.InvoiceNumb, dreader) Then ' ordered by ID
'            Do While dreader.Read
'                With receipt
'                    If 1 = rec Then ' typically Type = 'Sale' record which holds single value in Paid field. If user paid by multiple
'                        If IsDate(_Convert.Null2DefaultValue(dreader("Date")) & " " & _Convert.Null2DefaultValue(dreader("Time"))) Then
'                            .InvoiceDate = CDate(_Convert.Null2DefaultValue(dreader("Date")) & " " & _Convert.Null2DefaultValue(dreader("Time")))
'                        Else
'                            .InvoiceDate = Date.Now
'                        End If
'                        .AccountNumb = _Convert.Null2DefaultValue(dreader("AcctNum"))
'                        .AccountName = _Convert.Null2DefaultValue(dreader("AcctName"))
'                        .Clerk = _Convert.Null2DefaultValue(dreader("SalesRep"))
'                        .PaymentType = _Convert.Null2DefaultValue(dreader("Paid"))
'                        .IsCash = ("Cash" = .PaymentType)
'                        .IsCheck = ("Check" = .PaymentType)
'                        .IsCredit = ("CreditCard" = .PaymentType)
'                        .IsRefund = ("Refund" = .PaymentType)
'                        .IsOther = ("Other" = .PaymentType)
'                    ElseIf rec >= 2 Then '2 = rec Then ' 1st payment record after sale ' if rec >= 2 then get payment record(s) by checking Type field and Payment field <> 0
'                        ''AP(08/22/2019) - Updated POS emailed receipt to show multiple payments applied to invoice.
'                        Dim pType As String = _Convert.Null2DefaultValue(dreader("Type"))
'                        Dim pPayment As Double = _Convert.Null2DefaultValue(dreader("Payment"), 0)
'                        If pPayment <> 0 Then
'                            Select Case pType
'                                Case "Cash"
'                                    .IsCash = True
'                                    .Payments.Add(New _basePOS_PaymentItem With {
'                                                  .ItemPaymentType = _basePOS_PaymentItem.PaymentType.Cash,
'                                                  .ItemPaymentAmt = pPayment
'                                                  })
'                                Case "Check"
'                                    .IsCheck = True
'                                    .Payments.Add(New _basePOS_PaymentItem With {
'                                                  .ItemPaymentType = _basePOS_PaymentItem.PaymentType.Check,
'                                                  .ItemPaymentAmt = pPayment,
'                                                  .CheckOrCCard_Name = _Convert.Null2DefaultValue(dreader("NameOnCheck")),
'                                                  .CheckOrCCard_Numb = _Convert.Null2DefaultValue(dreader("CheckNum"))
'                                                  })
'                                Case "Charge"
'                                    .IsCredit = True
'                                    .Payments.Add(New _basePOS_PaymentItem With {
'                                                  .ItemPaymentType = _basePOS_PaymentItem.PaymentType.Charge,
'                                                  .ItemPaymentAmt = pPayment,
'                                                  .CheckOrCCard_Name = _Convert.Null2DefaultValue(dreader("CardName")),
'                                                  .CheckOrCCard_Numb = _Convert.Null2DefaultValue(dreader("CCNum"))
'                                                  })
'                                Case "Refund"
'                                    .IsRefund = True
'                                    .Payments.Add(New _basePOS_PaymentItem With {
'                                                  .ItemPaymentType = _basePOS_PaymentItem.PaymentType.Refund,
'                                                  .ItemPaymentAmt = pPayment
'                                                  })
'                                Case "Other"
'                                    .IsOther = True
'                                    .Payments.Add(New _basePOS_PaymentItem With {
'                                                  .ItemPaymentType = _basePOS_PaymentItem.PaymentType.Other,
'                                                  .ItemPaymentAmt = pPayment
'                                                  })
'                            End Select
'                            .CustomerPaid += pPayment
'                        ElseIf pType = "Change" Then
'                            .IsChange = True
'                            .Payments.Add(New _basePOS_PaymentItem With {
'                                          .ItemPaymentType = _basePOS_PaymentItem.PaymentType.Change,
'                                          .ItemPaymentAmt = _Convert.Null2DefaultValue(dreader("Charge"), 0)
'                                          })
'                        End If
'                        '
'                        '.CustomerPaid = _Convert.Null2DefaultValue(dreader("Payment"), 0)
'                        'If .IsCredit Then
'                        '    .CheckOrCCard_Name = _Convert.Null2DefaultValue(dreader("CardName"))
'                        '    .CheckOrCCard_Numb = _Convert.Null2DefaultValue(dreader("CCNum"))
'                        'End If
'                        'If .IsCheck Then
'                        '    .CheckOrCCard_Name = _Convert.Null2DefaultValue(dreader("NameOnCheck"))
'                        '    .CheckOrCCard_Numb = _Convert.Null2DefaultValue(dreader("CheckNum"))
'                        'End If
'                    End If
'                End With
'                rec += 1
'            Loop
'        End If
'        ShipRiteDb.Close_dreader(dreader)
'    End Sub
'    Private Sub read_ReceiptItems_FromDB(ByRef receipt As _basePOS_Receipt)
'        Dim dreader As OleDb.OleDbDataReader = Nothing
'        If ShipRiteDb.Transactions_GetInvoiceData(receipt.InvoiceNumb, dreader) Then
'            Do While dreader.Read
'                Dim item As New _basePOS_ReceiptItem
'                With item
'                    .ItemSKU = _Convert.Null2DefaultValue(dreader("SKU"))
'                    .ItemDesc = _Convert.Null2DefaultValue(dreader("Desc"))
'                    If Not receipt.IsPrintDisclaimer Then
'                        receipt.IsPrintDisclaimer = _Controls.Contains(.ItemDesc, "Package ID#:")
'                    End If
'                    .ItemPrice = _Convert.Null2DefaultValue(dreader("UnitPrice"), 0)
'                    .ItemQty = _Convert.Null2DefaultValue(dreader("Qty"), 1)
'                    .ItemExtPrice = _Convert.Null2DefaultValue(dreader("ExtPrice"), 0)
'                    .ItemSalesTax = _Convert.Null2DefaultValue(dreader("STax"), 0)
'                    '.ItemServiceTax = _Convert.Null2DefaultValue(dreader("ExtPrice"))
'                    ''ol#1.2.54(6/28)... Canada receipts will have HST, PST, GST sales tax break-down.
'                    .ItemSalesTax1 = _Convert.Null2DefaultValue(dreader("STax1"), 0)
'                    .ItemSalesTax2 = _Convert.Null2DefaultValue(dreader("STax2"), 0)
'                    .ItemSalesTax3 = _Convert.Null2DefaultValue(dreader("STax3"), 0)
'                    .ItemIsTaxable = (.ItemSalesTax > 0)
'                End With
'                receipt.Items.Add(item)
'            Loop
'        End If
'        ShipRiteDb.Close_dreader(dreader)
'    End Sub
'    Private Sub read_ReceiptBottom(ByRef receipt As _basePOS_Receipt)
'        If receipt.IsPrintDisclaimer Then
'            receipt.Disclaimer = ShipRiteDb.Setup_GetInvoiceDisclaimer
'        End If
'        receipt.Cupons = ShipRiteDb.Setup_GetInvoiceCupons
'    End Sub

'#End Region

'#Region "Receipt On/Off Options"
'    Public Sub Load_ReceiptOnOffOptions()
'        ''ol#9.289(3/23)... Five new on/off options for 'Printed Receipt' were added to Global Features.
'        Receipt_HTML.ReceiptOnOffOptions = ShipRiteDb.Setup2_GetReceiptOnOffOptions()
'        Receipt_HTML.ReceiptIsShipAddrOn = ShipRiteDb.Setup_GetShipAddressOnOffOptions()
'        Call Receipt_HTML.Read_ReceiptOnOffOptions(ReceiptIsShipAddrOn, Receipt_HTML.ReceiptOnOffOptions) ''ol#9.289(3/23).
'    End Sub
'    Public Sub Read_ReceiptOnOffOptions(ByVal isShipAddrOn As Boolean, ByRef onoffopt As String)
'        ''ol#9.289(3/23)... Five new on/off options for 'Printed Receipt' were added to Global Features.
'        If 0 = Len(onoffopt) Then
'            If isShipAddrOn Then
'                onoffopt = "11111"
'            Else
'                onoffopt = "10011"
'            End If
'        End If
'    End Sub
'    Public ReadOnly Property IsPrint_Consignee_Name() As Boolean
'        ''ol#9.289(3/23)... Five new on/off options for 'Printed Receipt' were added to Global Features.
'        Get
'            Return (0 < InStr(1, Receipt_HTML.ReceiptOnOffOptions, "1"))
'        End Get
'    End Property
'    Public ReadOnly Property IsPrint_Consignee_Addr1() As Boolean
'        ''ol#9.289(3/23)... Five new on/off options for 'Printed Receipt' were added to Global Features.
'        Get
'            Return (0 < InStr(2, Receipt_HTML.ReceiptOnOffOptions, "1"))
'        End Get
'    End Property
'    Public ReadOnly Property IsPrint_Consignee_CityStateZip() As Boolean
'        ''ol#9.289(3/23)... Five new on/off options for 'Printed Receipt' were added to Global Features.
'        Get
'            Return (0 < InStr(3, Receipt_HTML.ReceiptOnOffOptions, "1"))
'        End Get
'    End Property
'    Public ReadOnly Property IsPrint_Package_Dims() As Boolean
'        ''ol#9.289(3/23)... Five new on/off options for 'Printed Receipt' were added to Global Features.
'        Get
'            Return (0 < InStr(4, Receipt_HTML.ReceiptOnOffOptions, "1"))
'        End Get
'    End Property
'    Public ReadOnly Property IsPrint_Package_Weight() As Boolean
'        ''ol#9.289(3/23)... Five new on/off options for 'Printed Receipt' were added to Global Features.
'        Get
'            Return (0 < InStr(5, Receipt_HTML.ReceiptOnOffOptions, "1"))
'        End Get
'    End Property

'    Private Function print_ReceiptOnOffOptions(ByRef Desc As String) As Boolean
'        ''ol#9.289(3/23)... Five new on/off options for 'Printed Receipt' were added to Global Features.
'        If 0 < InStr(1, Desc, Receipt_HTML.PREFIX_NAME) Then
'            print_ReceiptOnOffOptions = Receipt_HTML.IsPrint_Consignee_Name
'            Desc = _Controls.Replace(Desc, Receipt_HTML.PREFIX_NAME, "")
'        ElseIf 0 < InStr(1, Desc, Receipt_HTML.PREFIX_ADDR) Then
'            If Receipt_HTML.ReceiptIsShipAddrOn Then
'                print_ReceiptOnOffOptions = Receipt_HTML.IsPrint_Consignee_Addr1
'                Desc = _Controls.Replace(Desc, Receipt_HTML.PREFIX_ADDR, "")
'            Else
'                print_ReceiptOnOffOptions = False
'            End If
'        ElseIf 0 < InStr(1, Desc, Receipt_HTML.PREFIX_CITY) Then
'            If Receipt_HTML.ReceiptIsShipAddrOn Then
'                print_ReceiptOnOffOptions = Receipt_HTML.IsPrint_Consignee_CityStateZip
'                Desc = _Controls.Replace(Desc, Receipt_HTML.PREFIX_CITY, "")
'            Else
'                print_ReceiptOnOffOptions = False
'            End If
'        ElseIf 0 < InStr(1, Desc, "Dimensions:") Then
'            print_ReceiptOnOffOptions = Receipt_HTML.IsPrint_Package_Dims
'        ElseIf 0 < InStr(1, Desc, "Scale Display:") Or 0 < InStr(1, Desc, "Weight:") Then
'            print_ReceiptOnOffOptions = Receipt_HTML.IsPrint_Package_Weight
'        Else
'            print_ReceiptOnOffOptions = True
'        End If
'    End Function

'#End Region
'    Private Sub build_End(ByVal receipt As _basePOS_Receipt, ByRef sb As StringBuilder)
'        sb.AppendLine("</table>")
'        If 0 < receipt.Disclaimer.Length Then
'            sb.AppendLine(String.Format("<font size={0}2{0} face={0}Verdana{0} color={0}#003366{0}><br/><br/>", qq))
'            sb.AppendLine(receipt.Disclaimer.Replace(vbNewLine, "<br/>"))
'            sb.AppendLine("</font>")
'        End If
'        If 0 < receipt.Cupons.Length Then
'            sb.AppendLine(String.Format("<font size={0}2{0} face={0}Verdana{0} color={0}maroon{0}><b><br/><br/>", qq))
'            sb.AppendLine(receipt.Cupons.Replace(vbNewLine, "<br/>"))
'            sb.AppendLine("</b></font>")
'        End If
'        sb.AppendLine("</body></html>")
'    End Sub
'#End Region

'End Module
