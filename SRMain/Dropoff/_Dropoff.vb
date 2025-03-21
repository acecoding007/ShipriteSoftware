'Imports CommonCode
'Imports CommonShip
'Imports DbCode
'Imports System.Text
'Imports ReceiptPrinting
'Imports System.Net.Mail
'Imports ThermalPrinting
'Imports ShipRiteReports
'Imports System.Data
Imports System.IO
Imports System.Net
Imports System.Net.Mail
Imports Newtonsoft.Json.Linq

Public Module _DropOff

    Public StoreOwner As New _baseContact
    Public CustomerObject As _baseContact
    Public Clerk As String
    Public ReceiptFontName As String
    Private DHLDropOff_FilesPath As String
    Private UPSDropOff_FilesPath As String
    Private Const UPSDropOff_SourceId As String = "ASO_SHIPRITE"

    Private ReadOnly Property UPSDropOff_IsTest As Boolean
        Get
            Return My.Settings.UPS_DropOff_IsTest
        End Get
    End Property
    Private ReadOnly Property UPSDropOff_URL As String
        Get
            If UPSDropOff_IsTest Then
                Return My.Settings.UPS_DropOff_Test_Url ' "https://b2c.ams1907.com"
            Else
                Return My.Settings.UPS_DropOff_Production_Url ' "https://b2c.ups.com"
            End If
        End Get
    End Property
    Private ReadOnly Property UPSDropOff_ClientId As String
        Get
            If UPSDropOff_IsTest Then
                Return "72e49067cb14c0398d18d9e5b5ee0ce3"
            Else
                Return "573a1555599b53c4e6d0f8c071cf33f9" '10/14/2023 - 04/30/2024 '"8ffcba50c25774b815acbd1991c0789f" '06/14/2023 - 10/31/2023 '"82a186bdb469c57dc767b8c11cb89091" 'expires 06/30/2023
            End If
        End Get
    End Property
    Private ReadOnly Property UPSDropOff_ClientSecret As String
        Get
            If UPSDropOff_IsTest Then
                Return "a42cdc4cd91ed3265add41aceb320e18"
            Else
                Return "31a5e19db93ccd8d6042865c8400efee" '10/14/2023 - 04/30/2024 '"e0eb00956a98c1f8990662261d0d3898" 06/14/2023 - 10/31/2023 '"c84b801fdefe787434ebb0b8703c85a4" 'expires 06/30/2023
            End If
        End Get
    End Property

#Region "Old-Code"
    Public Function Open_DropOffManager(callingWindow As Window, clerkid As String, objCustomer As _baseContact) As Boolean
        '    ''ol#1.2.40(6/2)... If customer selected in POS or ShipMaster then DropOff will pick it up automatically if called from these screens.
        _DropOff.CustomerObject = objCustomer
        _DropOff.Clerk = clerkid
        _DropOff.ReceiptFontName = String.Empty ' assume.
        '    Call ReportsDb.Get_ReceiptPrinterFontName(_DropOff.ReceiptFontName)
        '    If _PrinterSetup.NONE = My.Settings.Job_ReceiptPrinter_Name Then
        '        Call ReportsDb.Get_ReportPrinterName(ReportsDb.fldReceiptSlipPrinter, My.Settings.Job_ReceiptPrinter_Name)
        '    End If

        DHLDropOff_FilesPath = String.Format("{0}\DHL\DropOff", gDBpath)
        _Files.Create_Folder(DHLDropOff_FilesPath, False)
        UPSDropOff_FilesPath = String.Format("{0}\UPS\DropOff", gDBpath)
        _Files.Create_Folder(UPSDropOff_FilesPath, False)

        If ShipRiteDb.Setup_GetAddress_StoreOwner(StoreOwner) Then
            Dim win As New DropOffManager(callingWindow)
            win.ShowDialog(callingWindow)
            Return True
        Else
            _MsgBox.ErrorMessage("Could not read DefaultShipFrom id value to retrieve Store Owner address object!", "Failed to get Store Owner address!", "Drop Off Manager!")
            Return False
        End If

    End Function
#End Region

    Public Sub Print_DropOffReceipt(ByVal lvListView As ListView, ByVal customerName As String, Optional Copies As Integer = 1)


        ''ol#1.2.40(6/20)... Print Receipt will be utilizing a more reliable and much faster .Net Graphics.DrawString() in PrintHandlerEvent.
        'If 0 < My.Settings.JobPrintNotice_ReceiptPrinter_Copies Then 'No Copies setting found in SR Next

        Dim receipt As New System.Text.StringBuilder
        Dim count As Integer = lvListView.Items.Count
        Dim index As Integer = 0
        Dim objSingleDOI As DropOffInformation
        Dim pSettings As New PrintHelper
        Dim pName As String = GetPolicyData(gReportsDB, "InvoicePrinter")

        Call create_DropOff_Receipt_Header(receipt)
        Call create_DropOff_Receipt_EmptyLines(receipt, 2)
        '
        If Not String.Empty = customerName Then
            receipt.AppendLine(String.Format("Name:  {0}", customerName))
            Call create_DropOff_Receipt_EmptyLines(receipt, 3)
        End If
        '

        If count > 0 Then
            Do While index < count

                objSingleDOI = CType(lvListView.Items(index), DropOffInformation)

                receipt.AppendLine(String.Format("Carrier: {0}", objSingleDOI.CarrierName))
                receipt.AppendLine(String.Format("TRA: {0}", objSingleDOI.trackingNumber))
                If 0 < objSingleDOI.DropOffNotes.Length Then
                    receipt.AppendLine(String.Format("Notes: {0}", objSingleDOI.DropOffNotes))
                End If
                If Val(objSingleDOI.PackagingFee) > 0 Then
                    Dim fee As Double
                    Double.TryParse(objSingleDOI.PackagingFee.ToString, fee)
                    receipt.AppendLine(String.Format("Packaging Fee: {0}", fee.ToString("C2")))
                End If
                Call create_DropOff_Receipt_EmptyLines(receipt, 2)

                index = index + 1
            Loop
        End If

        '
        receipt.AppendLine(String.Format("Drop Off Date: {0}", Date.Now.ToString))
        Call create_DropOff_Receipt_EmptyLines(receipt, 2)

        ' Disclaimer
        Dim disclaimer As String = String.Empty
        If _Files.ReadFile_ToEnd(gTemplatesPath & "\DropOff_Disclaimer.txt", True, disclaimer) Then
            receipt.AppendLine(disclaimer)
        End If
        '
        ''ol#1.2.42(9/21)... When set to print 2 receipts, it prints one receipt with liability release, then a second receipt that has the liability release wording on it twice.
        'For i As Integer = 1 To My.Settings.JobPrintNotice_ReceiptPrinter_Copies 'No Copies setting found in SR Next
        '
        '_Receipt.Print_FromText(receipt.ToString, My.Settings.Job_ReceiptPrinter_Name) 'NetModule Code

        If receipt.ToString.Length > 0 Then
            If pName = "" Then
                pName = _Printers.Get_DefaultPrinter()
            End If

            pSettings.PrintFontFamilyName = GetPolicyData(gReportsDB, "InvoiceFont", "Consolas")
            pSettings.PrintFontSize = Val(GetPolicyData(gReportsDB, "FontSize", "9"))
            pSettings.PrintJobName = "ShipRite Dropff Receipt"
            'pSettings.FireDrawerCode = GetPolicyData(gReportsDB, "InvoiceDrawer")

            For i = 1 To Copies
                _PrintReceipt.Print_FromText(receipt.ToString, pName, pSettings)
            Next

        End If
        ' 
        'Next i 'No Copies setting found in SR Next
        receipt = Nothing
        '
        'End If 'No Copies setting found in SR Next


    End Sub
    Private Sub create_DropOff_Receipt_Header(ByRef receipt As System.Text.StringBuilder)
        receipt.AppendLine("        " & StoreOwner.CompanyName)
        receipt.AppendLine("        " & StoreOwner.Addr1)
        receipt.AppendLine("        " & StoreOwner.City)
        receipt.AppendLine("        " & StoreOwner.State)
        receipt.AppendLine("        " & StoreOwner.Zip)
        receipt.AppendLine("        " & StoreOwner.Tel)
    End Sub
    Private Sub create_DropOff_Receipt_Body(ByRef receipt As System.Text.StringBuilder)

    End Sub
    Private Sub create_DropOff_Receipt_EmptyLines(ByRef receipt As System.Text.StringBuilder, ByVal linesNumber As Integer)
        For i As Integer = 1 To linesNumber
            receipt.AppendLine()
        Next
    End Sub

#Region "Email Receipt"
    Public Function FlipOver_PersonName(ByVal name As String) As String
        FlipOver_PersonName = name ' assume.
        If Not 0 = name.Length Then
            Dim pos As Integer = InStr(name, ",")
            If Not 0 = pos Then
                FlipOver_PersonName = _Controls.Mid(name, pos + 1).Trim & " " & _Controls.Left(name, pos - 1).Trim
            End If
        End If
    End Function
    Public Function ToTutor_NewAppointment(ByVal tutorName As String, ByVal studentName As String, ByVal course As String, ByVal location As String, ByVal timeFrom As Date, ByVal timeTo As Date, ByRef subject As String, ByRef body As System.Text.StringBuilder) As Boolean
        ToTutor_NewAppointment = False ' assume.
        subject = "Tutoring Appointment Notification!"
        body.Append("Dear ") : body.Append(tutorName) : body.Append(",")
        body.AppendLine()
        body.AppendLine()
        body.AppendLine("An appointment was just made with you by:")
        body.AppendLine()
        body.Append("Student: ") : body.AppendLine(studentName)
        body.Append("Course: ") : body.AppendLine(course)
        body.Append("Date: ") : body.AppendLine(timeFrom.ToString("ddd, dd MMM yyyy"))
        body.Append("Time: ") : body.Append(timeFrom.ToString("t")) : body.Append(" - ") : body.AppendLine(timeTo.ToString("t"))
        body.Append("Location: ") : body.Append(location)
        body.AppendLine()
        body.AppendLine()
        body.Append("To view or cancel this appointment, please go to: ") : body.AppendLine("https://tutoring.mvcc.edu:8443/TutorAppointments.aspx")
        '_Debug.Print_(body.ToString)
        ToTutor_NewAppointment = True
    End Function

    Public Function Create_HtmlReceipt(ByVal tutorName As String, ByVal studentName As String, ByVal course As String, ByVal location As String, ByVal timeFrom As Date, ByVal timeTo As Date, ByRef subject As String, ByRef body As System.Text.StringBuilder) As Boolean
        Create_HtmlReceipt = False ' assume.
        subject = "Your Drop Off Receipt"
        body.Remove(0, body.Length) ' clear
        body.AppendLine("<html><body>")
        body.AppendLine("<font size=" & Chr(34) & "2" & Chr(34) & " face=" & Chr(34) & "Verdana" & Chr(34) & " color=" & Chr(34) & "#003366" & Chr(34) & ">")
        body.AppendLine("Tutoring Appointment was made by " & studentName & " on " & DateTime.Now.ToString & " for:")
        Call create_AppointmentHtmlCard(tutorName, studentName, course, location, timeFrom, timeTo, body)
        body.AppendLine("You have an appointment setup with a student. Your appointment time is listed above. <br> Please report to the Learning Center (" & location & ") at the scheduled time.")
        body.AppendLine("<br><br>")
        body.AppendLine("This email address is unmonitored, so please do not reply to it as your message will not reach anyone.")
        body.AppendLine("</font>")
        body.AppendLine("</body></html>")
        Create_HtmlReceipt = True
    End Function

    Private Function create_AppointmentHtmlCard(ByVal tutorName As String, ByVal studentName As String, ByVal course As String, ByVal location As String, ByVal timeFrom As Date, ByVal timeTo As Date, ByRef body As System.Text.StringBuilder) As Boolean
        create_AppointmentHtmlCard = False ' assume.
        body.AppendLine("<br><br>")
        body.AppendLine("<table style=" & Chr(34) & "bgcolor: #F0F8FF; font-size: 8pt; font-color:#003366; font-family:Verdana,arial,helvetica,sans-serif; border:1px solid #94a6b5;" & Chr(34) & ">")
        body.AppendLine("<tr><th><font color=" & Chr(34) & "#003366" & Chr(34) & ">Tutor</font></th><td colspan=4>" & tutorName & "</td></tr>")
        body.AppendLine("<tr><th><font color=" & Chr(34) & "#003366" & Chr(34) & ">Course</font></th><td colspan=4>" & course & "</td></tr>")
        body.AppendLine("<tr><th><font color=" & Chr(34) & "#003366" & Chr(34) & ">Student</font></th><td colspan=4>" & studentName & "</td></tr>")
        body.AppendLine("<tr><th><font color=" & Chr(34) & "#003366" & Chr(34) & ">Date</font></th><th><font color=" & Chr(34) & "#003366" & Chr(34) & ">From</font></th><th><font color=" & Chr(34) & "#003366" & Chr(34) & ">To</font></th><th><font color=" & Chr(34) & "#003366" & Chr(34) & ">Room</font></th></tr>")
        body.AppendLine("<td width=" & Chr(34) & "120" & Chr(34) & " align=" & Chr(34) & "center" & Chr(34) & ">" & timeFrom.ToString("ddd, dd MMM yyyy") & "</td>")
        body.AppendLine("<td width=" & Chr(34) & "80" & Chr(34) & " align=" & Chr(34) & "center" & Chr(34) & ">" & timeFrom.ToString("t") & "</td>")
        body.AppendLine("<td width=" & Chr(34) & "80" & Chr(34) & " align=" & Chr(34) & "center" & Chr(34) & ">" & timeTo.ToString("t") & "</td>")
        body.AppendLine("<td width=" & Chr(34) & "100" & Chr(34) & " align=" & Chr(34) & "center" & Chr(34) & ">" & location & "</td>")
        body.AppendLine("</table>")
        body.AppendLine("<br><br>")
        create_AppointmentHtmlCard = True
    End Function

    Public Function EmailNow(ByVal emailsTo As String, ByVal subject As String, ByVal body As String) As Boolean
        Dim emailFrom As New MailAddress("LearningCenter@mvcc.edu", "Learning Center Online")
        Dim emailTo As New MailAddressCollection()
        Dim splitEmails() As String = emailsTo.Split(";")
        EmailNow = False ' assume.
        For i As Integer = 0 To splitEmails.Length - 1
            emailTo.Add(splitEmails(i).Trim)
        Next
        If emailTo.Count > 0 Then
            EmailNow = _Email.Send_Notification(emailFrom, emailTo, subject, body)
            emailTo.Clear()
        End If
        emailFrom = Nothing
        emailTo = Nothing
    End Function

#End Region

#Region "SOAP request"
    Public Function Send_DHL_SOAPRequest(ByVal DHLAccount As String, ByVal TrackingNo As String) As Boolean
        Send_DHL_SOAPRequest = False ' assume.
        Try
            System.Net.ServicePointManager.SecurityProtocol = Net.SecurityProtocolType.Tls12 ''AP(06/07/2018) - Updated DHL Drop Off request to use TLS 1.2 security protocol.

            Dim manualWebClient As New System.Net.WebClient()

            manualWebClient.Headers.Add("Content-Type", "application/soap+xml;  charset=utf-8")

            ' Note: don't put the <?xml... tag in--otherwise it will blow up with a 500 internal error message!
            Dim bytArguments As Byte() = System.Text.Encoding.ASCII.GetBytes(
                "<SOAP-ENV:Envelope xmlns:SOAP-ENV=""http://www.w3.org/2003/05/soap-envelope"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">" & System.Environment.NewLine &
                "<SOAP-ENV:Header>" & System.Environment.NewLine &
                "   <NS1:IWebAuthendicationHeader xmlns:NS1=""http://www.w3.org/2003/05/soap-envelope"">" & System.Environment.NewLine &
                "       <PortalID xmlns=""http://webservices.DotNetNuke.com/"">" & "13" & "</PortalID>" & System.Environment.NewLine &
                "       <UserID xmlns=""http://webservices.DotNetNuke.com/"">" & "0" & "</UserID>" & System.Environment.NewLine &
                "       <Username xmlns=""http://webservices.DotNetNuke.com/"">" & DHLAccount & "</Username>" & System.Environment.NewLine &
                "       <Password xmlns=""http://webservices.DotNetNuke.com/"">" & DHLAccount & "</Password>" & System.Environment.NewLine &
                "       <Encrypted xmlns=""http://webservices.DotNetNuke.com/"">" & "False" & "</Encrypted>" & System.Environment.NewLine &
                "       <WebPageCall xmlns=""http://webservices.DotNetNuke.com/"">" & "False" & "</WebPageCall>" & System.Environment.NewLine &
                "       <ModuleId xmlns=""http://webservices.DotNetNuke.com/"">" & "0" & "</ModuleId>" & System.Environment.NewLine &
                "   </NS1:IWebAuthendicationHeader>" & System.Environment.NewLine &
                "</SOAP-ENV:Header>" & System.Environment.NewLine &
                "  <SOAP-ENV:Body>" & System.Environment.NewLine &
                "    <RsaAddDropOff xmlns=""http://webservices.DotNetNuke.com/"">" & System.Environment.NewLine &
                "       <RsaID>" & "ShipRite" & "</RsaID>" & System.Environment.NewLine &
                "       <AccountNo>" & DHLAccount & "</AccountNo>" & System.Environment.NewLine &
                "       <WaybillNo>" & TrackingNo & "</WaybillNo>" & System.Environment.NewLine &
                "       <Pieces>" & "1" & "</Pieces>" & System.Environment.NewLine &
                "    </RsaAddDropOff>" & System.Environment.NewLine &
                "  </SOAP-ENV:Body>" & System.Environment.NewLine &
                "</SOAP-ENV:Envelope>")

            '_Debug.Print_(System.Text.Encoding.ASCII.GetString(bytArguments))
            _Files.WriteFile_ToEnd(bytArguments, String.Format("{0}\{1}_request.xml", DHLDropOff_FilesPath, TrackingNo))

            Dim bytRetData As Byte() = manualWebClient.UploadData("https://us-central1-brain-cloud-services.cloudfunctions.net/retail-hip-dropoff-prod/retail", "POST", bytArguments)

            '_Debug.Print_(System.Text.Encoding.ASCII.GetString(bytRetData))
            _Files.WriteFile_ToEnd(bytRetData, String.Format("{0}\{1}_reply.xml", DHLDropOff_FilesPath, TrackingNo))

            Return True
        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to upload Tracking# to DHL Online Server...")
        End Try
    End Function

    Public Function Send_UPS_ScanRequest(UPSAccessPointID As String, TrackingNo As String) As Boolean
        Try
            Net.ServicePointManager.SecurityProtocol = Net.SecurityProtocolType.Tls12

            Using webClient As New Net.WebClient()

                webClient.Headers("Accept") = "application/json"
                webClient.Headers("Content-Type") = "application/json"
                webClient.Headers("x-ibm-client-id") = UPSDropOff_ClientId
                webClient.Headers("x-ibm-client-secret") = UPSDropOff_ClientSecret
                webClient.Headers("transactionSrc") = UPSDropOff_SourceId
                webClient.Headers("transId") = Guid.NewGuid.ToString '"1099"

                Dim reqJson As New JObject()
                reqJson("accessPointID") = UPSAccessPointID
                reqJson("trackingNumber") = TrackingNo
                reqJson("scanDateTime") = Date.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
                reqJson("event") = "SHIPPER_DROP_OFF"

                Dim reqString As String = reqJson.ToString()

                '_Debug.Print_(reqString)
                _Files.WriteFile_ToEnd(reqString, String.Format("{0}\ShipperDropOff_{1}_request.json", UPSDropOff_FilesPath, TrackingNo))

                Dim resErrMsg As String = String.Empty
                Dim resString As String = UPSDropOff_PostString(webClient, UPSDropOff_URL & "/rmsi/scan/api/v1/sendscanevent", reqString, resErrMsg)

                '_Debug.Print_(resString)
                Dim resJson As JObject = Nothing
                If IsValidJson(resString) Then
                    resJson = JObject.Parse(resString)
                    resString = resJson.ToString()
                End If

                Dim resBuf As String = String.Empty
                If Not String.IsNullOrEmpty(resString) Then
                    resBuf = resString
                ElseIf Not String.IsNullOrEmpty(resErrMsg) Then
                    resBuf = resErrMsg
                End If
                If Not String.IsNullOrEmpty(resBuf) Then
                    _Files.WriteFile_ToEnd(resBuf, String.Format("{0}\ShipperDropOff_{1}_response.json", UPSDropOff_FilesPath, TrackingNo))
                End If

                If Not String.IsNullOrEmpty(resErrMsg) Then
                    'errored
                    Dim resErrors As String = String.Empty
                    If resJson IsNot Nothing Then
                        If Not String.IsNullOrEmpty(resErrMsg) Then
                            Try
                                If resJson("errors") Is Nothing Then
                                    ' errored and json returned and errors element not returned - return json for more descriptive return status
                                    resErrors = resJson.ToString()
                                Else
                                    Dim resErrorJsonArray As JArray = resJson("errors")
                                    resErrors = resErrorJsonArray.ToString()
                                End If
                            Catch ex As Exception
                            End Try
                        End If
                    End If
                    If Not String.IsNullOrEmpty(resErrors) Then
                        Throw New Exception(resErrors)
                    ElseIf Not String.IsNullOrEmpty(resErrMsg) Then
                        Throw New Exception(resErrMsg)
                    End If
                Else
                    'success
                    ' no response data captured on success
                End If

            End Using

            Return True
        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to upload Tracking# to UPS Online Server...", , False)
        End Try
        Return False
    End Function
    Public Function Send_UPS_CommInvoiceRequest(UPSAccessPointID As String, TrackingNo As String, ByRef commInvoice As Boolean) As Boolean
        Try
            Net.ServicePointManager.SecurityProtocol = Net.SecurityProtocolType.Tls12
            commInvoice = False ' assume

            Using webClient As New Net.WebClient()

                webClient.Headers("Accept") = "application/json"
                webClient.Headers("Content-Type") = "application/json"
                webClient.Headers("x-ibm-client-id") = UPSDropOff_ClientId
                webClient.Headers("x-ibm-client-secret") = UPSDropOff_ClientSecret
                webClient.Headers("transactionSrc") = UPSDropOff_SourceId
                webClient.Headers("transId") = Guid.NewGuid.ToString '"1099"

                Dim reqJson As New JObject()
                reqJson("accessPointID") = UPSAccessPointID
                reqJson("trackingNumber") = TrackingNo

                Dim reqString As String = reqJson.ToString()

                '_Debug.Print_(reqString)
                _Files.WriteFile_ToEnd(reqString, String.Format("{0}\CommercialInvoice_{1}_request.json", UPSDropOff_FilesPath, TrackingNo))

                Dim resErrMsg As String = String.Empty
                Dim resString As String = UPSDropOff_PostString(webClient, UPSDropOff_URL & "/rmsi/shipper/api/v1/commercialinvoice", reqString, resErrMsg)

                '_Debug.Print_(resString)
                Dim resJson As JObject = Nothing
                If IsValidJson(resString) Then
                    resJson = JObject.Parse(resString)
                    resString = resJson.ToString()
                End If

                Dim resBuf As String = String.Empty
                If Not String.IsNullOrEmpty(resString) Then
                    resBuf = resString
                ElseIf Not String.IsNullOrEmpty(resErrMsg) Then
                    resBuf = resErrMsg
                End If
                If Not String.IsNullOrEmpty(resBuf) Then
                    _Files.WriteFile_ToEnd(resBuf, String.Format("{0}\CommercialInvoice_{1}_response.json", UPSDropOff_FilesPath, TrackingNo))
                End If

                If Not String.IsNullOrEmpty(resErrMsg) Then
                    'errored
                    Dim resErrors As String = String.Empty
                    If resJson IsNot Nothing Then
                        If Not String.IsNullOrEmpty(resErrMsg) Then
                            Try
                                Dim isSkipError As Boolean = False
                                If resJson("errors") Is Nothing Then
                                    ' errored and json returned and errors element not returned - return json for more descriptive return status
                                    resErrors = resJson.ToString()
                                Else
                                    Dim resErrorJsonArray As JArray = resJson("errors")
                                    For i As Integer = 0 To resErrorJsonArray.Count - 1
                                        Dim resErrorCode As String = resErrorJsonArray(i)("code")
                                        If resErrorCode = "80027" Then
                                            isSkipError = True
                                            Exit For
                                        End If
                                    Next
                                    If isSkipError Then
                                        resErrMsg = String.Empty
                                        resErrors = String.Empty
                                    Else
                                        resErrors = resErrorJsonArray.ToString()
                                    End If
                                End If
                            Catch ex As Exception
                            End Try
                        End If
                    End If
                    If Not String.IsNullOrEmpty(resErrors) Then
                        Throw New Exception(resErrors)
                    ElseIf Not String.IsNullOrEmpty(resErrMsg) Then
                        Throw New Exception(resErrMsg)
                    End If
                Else
                    'success
                    If resJson IsNot Nothing Then
                        Try
                            commInvoice = resJson("response")("commercialInvoiceIndication")
                        Catch ex As Exception
                        End Try
                    End If
                End If

            End Using

            Return True
        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to check Commercial Invoice from UPS Online Server...", , False)
        End Try
        Return False
    End Function

    Private Function UPSDropOff_PostString(webClient As WebClient, address As String, data As String, ByRef resErrMsg As String) As String
        Dim resString As String = String.Empty
        resErrMsg = String.Empty
        Try
            resString = webClient.UploadString(address, "POST", data)
        Catch ex As WebException
            resErrMsg = ex.Message
            Try
                Using webResponse As WebResponse = ex.Response
                    Using dataStream As Stream = webResponse.GetResponseStream()
                        Using reader As New StreamReader(dataStream)
                            resString = reader.ReadToEnd()
                        End Using
                    End Using
                End Using
            Catch ex2 As Exception
            End Try
        End Try
        Return resString
    End Function
#End Region

End Module

Public Class _DropOff_Receipt
    Public Name As String
    Public DropOffDate As Date
    Public Items As New List(Of _DropOff_ReceiptItem)
    Public Disclaimer As String
End Class
Public Class _DropOff_ReceiptItem
    Public Carrer As String
    Public TrackingNumb As String
    Public Notes As String
    Public PackagingFee As Double
End Class
