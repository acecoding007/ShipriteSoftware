'Imports System.Text
'Imports CommonCode
'Imports CommonShip
'Imports DbCode
'Imports EmailNotification
'Imports Microsoft.VisualBasic

'Public Module DropOff_Receipt_HTML

'    Public qq As Char = Microsoft.VisualBasic.ChrW(34) ' double quote

'#Region "Drop Off Receipt"
'    Public Function Email_DropOff_HTMLReceipt_FromObject(ByVal receipt As Object, ByVal emailTo As Object) As Boolean
'        Email_DropOff_HTMLReceipt_FromObject = False ' assume.
'        Dim sb As New StringBuilder
'        Call build_HTML_Receipt(receipt, sb)
'        '_Debug.Print_(sb.ToString)
'        Dim EmailPackages As New Collection
'        For i As Integer = 1 To emailTo.Count
'            Dim epack As New _EmailPackage
'            epack.EmailTo = emailTo(i)
'            epack.HTMLBody = sb.ToString
'            EmailPackages.Add(epack)
'        Next
'        Email_DropOff_HTMLReceipt_FromObject = EmailNotification.Send_HTMLEmail(EmailPackages, "Drop Off Receipt")
'    End Function

'    Private Sub build_HTML_Receipt(ByRef receipt As _DropOff_Receipt, ByRef sb As StringBuilder)
'        Call build_Header(sb)
'        Call build_StoreOwner(sb)
'        Call build_ReceiptHeader(receipt, sb)
'        Call build_ReceiptItems(receipt, sb)
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
'        Dim StoreOwner As New CommonCode._baseContact
'        Call ShipRiteDb.Setup_GetAddress_StoreOwner(StoreOwner)
'        sb.AppendLine(String.Format("<tr><th class={0}style2{0} colspan={0}4{0}><font color={0}#003366{0} size={0}2{0}>{1}</font> </th></tr>", qq, StoreOwner.CompanyName))
'        sb.AppendLine(String.Format("<tr><th class={0}style2{0} colspan={0}4{0}><font color={0}#003366{0} size={0}2{0}>{1}</font> </th></tr>", qq, StoreOwner.Addr1))
'        If Not String.IsNullOrEmpty(StoreOwner.Addr2) Then
'            sb.AppendLine(String.Format("<tr><th class={0}style2{0} colspan={0}4{0}><font color={0}#003366{0} size={0}2{0}>{1}</font> </th></tr>", qq, StoreOwner.Addr2))
'        End If
'        sb.AppendLine(String.Format("<tr><th class={0}style2{0} colspan={0}4{0}><font color={0}#003366{0} size={0}2{0}>{1}</font> </th></tr>", qq, StoreOwner.CityStateZip))
'        sb.AppendLine(String.Format("<tr><th class={0}style2{0} colspan={0}4{0}><font color={0}#003366{0} size={0}2{0}>{1}</font> </th></tr>", qq, StoreOwner.Tel))

'        Call draw_Line_Size4(sb)
'    End Sub
'    Private Sub build_ReceiptHeader(ByVal receipt As _DropOff_Receipt, ByRef sb As StringBuilder)
'        With receipt
'            sb.AppendLine(String.Format("<tr><th class={0}style1{0}><font color={0}#003366{0}>Name:</font></th><td colspan={0}2{0}>{1}</td></tr>", qq, .Name))
'        End With
'        Call draw_Line_Size3(sb)
'    End Sub
'    Private Sub build_ReceiptItems(ByVal receipt As _DropOff_Receipt, ByRef sb As StringBuilder)
'        For i As Integer = 0 To receipt.Items.Count - 1
'            Dim item As _DropOff_ReceiptItem = receipt.Items(i)
'            With item
'                sb.AppendLine(String.Format("<tr><th class={0}style1{0}><font color={0}#003366{0}>Carrier:</font></th><td colspan={0}2{0}>{1}</td></tr>", qq, .Carrer))
'                sb.AppendLine(String.Format("<tr><th class={0}style1{0}><font color={0}#003366{0}>Tracking#:</font></th><td colspan={0}2{0}>{1}</td></tr>", qq, .TrackingNumb))
'                sb.AppendLine(String.Format("<tr><th class={0}style1{0}><font color={0}#003366{0}>Notes:</font></th><td colspan={0}2{0}>{1}</td></tr>", qq, .Notes))
'                sb.AppendLine(String.Format("<tr><th class={0}style1{0}><font color={0}#003366{0}>Packaging Fee:</font></th><td colspan={0}2{0}>{1}</td></tr>", qq, .PackagingFee.ToString("C")))
'                Call draw_Line_Size3(sb)
'            End With
'        Next
'        sb.AppendLine(String.Format("<tr><th class={0}style1{0}><font color={0}#003366{0}>Drop Off Date:</font></th><td colspan={0}2{0}>{1}</td></tr>", qq, receipt.DropOffDate))
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

'    Private Sub build_End(ByVal receipt As _DropOff_Receipt, ByRef sb As StringBuilder)
'        sb.AppendLine("</table>")
'        If 0 < receipt.Disclaimer.Length Then
'            sb.AppendLine(String.Format("<font size={0}2{0} face={0}Verdana{0} color={0}#003366{0}><br/><br/>", qq))
'            sb.AppendLine(receipt.Disclaimer.Replace(vbNewLine, "<br/>"))
'            sb.AppendLine("</font>")
'        End If
'        sb.AppendLine("</body></html>")
'    End Sub
'#End Region

'End Module

'Public Class _DropOff_Receipt
'    Public Name As String
'    Public DropOffDate As Date
'    Public Items As New List(Of _DropOff_ReceiptItem)
'    Public Disclaimer As String
'End Class
'Public Class _DropOff_ReceiptItem
'    Public Carrer As String
'    Public TrackingNumb As String
'    Public Notes As String
'    Public PackagingFee As Double
'End Class
