Imports System.Net
Imports System.Net.Mail
Imports System.Text
Imports System.ServiceProcess
Imports System.Text.RegularExpressions
Imports System.Data
Imports SHIPRITE.RTF2HTML

Public Module _Email

    Private m_EmailBody As String
    Private m_EmailBodyHTML As String
    Private m_EmailFrom As String
    Private m_EmailSubject As String
    Public Property EmailSubject() As String
        Get
            Return m_EmailSubject
        End Get
        Set(ByVal value As String)
            m_EmailSubject = value
        End Set
    End Property
    Public Property EmailFrom() As String
        Get
            Return m_EmailFrom
        End Get
        Set(ByVal value As String)
            m_EmailFrom = value
        End Set
    End Property
    Public Property EmailBoby() As String
        Get
            Return m_EmailBody
        End Get
        Set(ByVal value As String)
            m_EmailBody = value
        End Set
    End Property
    Public Property EmailBodyHTML() As String
        Get
            Return m_EmailBodyHTML
        End Get
        Set(ByVal value As String)
            m_EmailBodyHTML = value
        End Set
    End Property

    Public Function Send_Error(ByVal ex As Exception) As Boolean
        Send_Error = False

        If String.IsNullOrEmpty(_Email.EmailFrom) Then
            Return False
        End If

        Dim mailMsg As New MailMessage(_Email.EmailFrom, "mersad@shipritesoftware.com")

        Try
            If isSMTPservice_Installed() Then
                ''
                With mailMsg
                    .Priority = MailPriority.High
                    .Subject = String.Format("[{0}] LCTrack Error: {1}", Environment.MachineName, ex.Message)
                    .Body = String.Format("The following error has occurred: {0}{1}{2}", Environment.NewLine, Environment.NewLine, ex.ToString)
                End With
                ''
                'Dim client As New SmtpClient("mvcc-newexch.mvcc.edu", 25)
                'client.Credentials = CredentialCache.DefaultNetworkCredentials
                Dim client As New SmtpClient
                client.DeliveryMethod = SmtpDeliveryMethod.PickupDirectoryFromIis
                ''
                client.Send(mailMsg)
                Send_Error = True
                ''
            End If

        Catch emailEx As Exception : _Debug.Print_("Email.Send_Error(): " & emailEx.ToString)
        Finally
            ' release the hook:
            mailMsg.Dispose()
        End Try

    End Function
    Public Function Send_TestError(ByVal ex As Exception) As Boolean
        Send_TestError = False

        If String.IsNullOrEmpty(_Email.EmailFrom) Then
            Return False
        End If

        Dim mailMsg As New MailMessage(_Email.EmailFrom, "oleg@shipritesoftware.com")

        Try
            'If isSMTPservice_Installed() Then
            ''
            With mailMsg
                .Priority = MailPriority.High
                .Subject = String.Format("[{0}] LCTrack Error: {1}", Environment.MachineName, ex.Message)
                .Body = String.Format("The following error has occurred: {0}{1}{2}", Environment.NewLine, Environment.NewLine, ex.ToString)
            End With
            ''
            '' Hotmail:
            'Dim client As New SmtpClient("smtp-mail.outlook.com", 587)
            'Dim cr As New System.Net.NetworkCredential("donchuk_vb@hotmail.com", "justsayno")

            'ShipriteSoftware.com:
            Dim client As New SmtpClient("smtp.gmail.com", 587)
            Dim cr As New System.Net.NetworkCredential("oleg@shipritesoftware.com", "justShipRite")

            ''GMail.com:
            'Dim client As New SmtpClient("smtp.googlemail.com", 587)
            'Dim cr As New System.Net.NetworkCredential("odonchuk@gmail.com", "Tamara36Danil10Anton3Polina2")

            client.Credentials = cr
            client.DeliveryMethod = SmtpDeliveryMethod.Network
            client.EnableSsl = True
            ''
            client.Send(mailMsg)
            Send_TestError = True
            ''
            'End If

        Catch emailEx As Exception : _MsgBox.ErrorMessage("Email.Send_Error(): " & emailEx.ToString)
        Finally
            ' release the hook:
            mailMsg.Dispose()
        End Try

    End Function

    Public Function Send_Error(ByVal ex As Exception, ByVal subject As String) As Boolean
        Send_Error = False

        If String.IsNullOrEmpty(_Email.EmailFrom) Then
            Return False
        End If

        Dim mailMsg As New MailMessage(_Email.EmailFrom, "odonchuk@mvcc.edu")

        Try
            If isSMTPservice_Installed() Then
                ''
                With mailMsg
                    .Priority = MailPriority.High
                    .Subject = String.Format("[{0}] {2}: {1}", Environment.MachineName, ex.Message, subject)
                    .Body = String.Format("The following error has occurred: {0}{1}{2}", Environment.NewLine, Environment.NewLine, ex.ToString)
                End With
                ''
                'Dim client As New SmtpClient("mvcc-newexch.mvcc.edu", 25)
                'client.Credentials = CredentialCache.DefaultNetworkCredentials
                Dim client As New SmtpClient
                client.DeliveryMethod = SmtpDeliveryMethod.PickupDirectoryFromIis
                ''
                client.Send(mailMsg)
                Send_Error = True
                ''
            End If

        Catch emailEx As Exception : _Debug.Print_("Email.Send_Error(): " & emailEx.ToString)
        Finally
            ' release the hook:
            mailMsg.Dispose()
        End Try

    End Function
    Public Function Send_Error(ByVal ex As Exception, ByVal subject As String, ByVal sql2exe As String) As Boolean
        Send_Error = False

        If String.IsNullOrEmpty(_Email.EmailFrom) Then
            Return False
        End If

        Dim mailMsg As New MailMessage(_Email.EmailFrom, "odonchuk@mvcc.edu")

        Try
            If isSMTPservice_Installed() Then
                ''
                With mailMsg
                    .Priority = MailPriority.High
                    .Subject = String.Format("[{0}] {2}: {1}", Environment.MachineName, ex.Message, subject)
                    .Body = String.Format("The following [sql2exe] error has occurred: {0}{0}{1}{0}{0}{2}", Environment.NewLine, sql2exe, ex.ToString)
                End With
                ''
                'Dim client As New SmtpClient("mvcc-newexch.mvcc.edu", 25)
                'client.Credentials = CredentialCache.DefaultNetworkCredentials
                Dim client As New SmtpClient
                client.DeliveryMethod = SmtpDeliveryMethod.PickupDirectoryFromIis
                ''
                client.Send(mailMsg)
                Send_Error = True
                ''
            End If

        Catch emailEx As Exception : _Debug.Print_("Email.Send_Error(): " & emailEx.ToString)
        Finally
            ' release the hook:
            mailMsg.Dispose()
        End Try

    End Function
    Public Function Send_Error(ByVal errorStack As String) As Boolean
        Send_Error = False
        Dim mailMsg As New MailMessage '("odonchuk@mvcc.edu", "odonchuk@mvcc.edu")

        Try
            If isSMTPservice_Installed() Then
                ''
                With mailMsg
                    .From = New MailAddress(_Email.EmailFrom, "MVCC Email Merge")
                    .To.Add(New MailAddress("odonchuk@mvcc.edu"))
                    .Priority = MailPriority.High
                    .Subject = String.Format("[{0}] {1}", Environment.MachineName, "MVCC Email Merge Error Notification!")
                    .Body = String.Format("The following error has occurred: {0}{1}{2}", Environment.NewLine, Environment.NewLine, errorStack)
                End With

                'Dim client As New SmtpClient("mvcc-newexch.mvcc.edu", 25)
                'client.Credentials = CredentialCache.DefaultNetworkCredentials
                Dim client As New SmtpClient
                client.DeliveryMethod = SmtpDeliveryMethod.PickupDirectoryFromIis

                client.Send(mailMsg)
                Send_Error = True
                ''
            End If

        Catch emailEx As Exception : _Debug.Print_("Email.Send_Error(): " & emailEx.ToString)
        Finally
            ' release the hook:
            mailMsg.Dispose()
        End Try

    End Function
    Public Function Send_Email(ByVal drow As DataRow, ByVal msg2show As Boolean) As Boolean
        Send_Email = False
        Try

            If isSMTPservice_Installed() Then
                '
                If String.IsNullOrEmpty(drow("emailFrom").ToString) Or String.IsNullOrEmpty(drow("emailTo").ToString) Then
                    '
                    ' make it silent for now: if there is no To/From emails then no email will be sent:
                    'If msg2show Then
                    '    MsgBox("You must enter both To and From email addresses!", MsgBoxStyle.Exclamation)
                    'End If
                    'Task.Activity_PrintMsg(drow, "Missing To/From email address...", msg2show)
                    '
                Else
                    '
                    Send_Email = send(drow)
                    If Send_Email Then
                        If msg2show Then
                            MsgBox("Email has been sent successfully!", MsgBoxStyle.Information)
                        End If
                        'Task.Activity_PrintMsg(drow, "Email-Alert sent successfully!", msg2show)
                    Else
                        If msg2show Then
                            MsgBox("Failed to email!", MsgBoxStyle.Critical)
                        End If
                    End If
                    '
                End If
                '
            Else
                '
                If msg2show Then
                    MsgBox("You do not have Email SMTP Service installed on this machine!", MsgBoxStyle.Exclamation)
                End If
                'Task.Activity_PrintMsg(drow, "SMTP Service was not found on this machine...", msg2show)
                '
            End If

        Catch ex As Exception
            '' catching error to just record the Activity:
            'Task.Activity_PrintMsg(drow, "Failed to email (see ErrStack.log)..." & ControlChars.NewLine & ex.Message, msg2show)
            '' and raising the error again for the caller to catch:
            ''Throw ex
            MsgBox(ex.ToString, MsgBoxStyle.Critical)
        End Try

    End Function
    Public Function Send_Email(ByVal drow As DataRow, ByVal msg2show As Boolean, ByVal isitTest As Boolean) As Boolean
        Send_Email = False
        Try

            If isSMTPservice_Installed() Then
                '
                Send_Email = send(drow, isitTest)
                If Send_Email Then
                    If msg2show Then
                        MsgBox("Email has been sent successfully!", MsgBoxStyle.Information)
                    End If
                Else
                    If msg2show Then
                        MsgBox("Failed to email!", MsgBoxStyle.Critical)
                    End If
                End If
                '
            Else
                '
                If msg2show Then
                    MsgBox("You do not have Email SMTP Service installed on this machine!", MsgBoxStyle.Exclamation)
                End If
                '
            End If

        Catch ex As Exception
            '' catching error to just record the Activity:
            '' and raising the error again for the caller to catch:
            _Debug.Print2File(ex.ToString, String.Empty)
            Throw ex
            ''MsgBox(ex.ToString, MsgBoxStyle.Critical)
        End Try

    End Function
    Public Function Send_Notification(ByVal emailFrom As MailAddress, ByVal emailTo As MailAddressCollection, ByVal subject As String, ByVal body As String, Optional ByVal isBodyHtml As Boolean = True) As Boolean
        Send_Notification = False

        If 0 = emailFrom.Address.Length Then
            Return False
        End If

        Dim mailMsg As New MailMessage()
        mailMsg.From = emailFrom
        For i As Integer = 0 To emailTo.Count - 1
            mailMsg.To.Add(emailTo(i))
        Next i

        Try
            If isSMTPservice_Installed() Then
                ''
                With mailMsg
                    .Priority = MailPriority.High
                    .Subject = subject
                    .Body = body
                    .IsBodyHtml = isBodyHtml
                End With
                ''
                Dim client As New SmtpClient
                client.DeliveryMethod = SmtpDeliveryMethod.PickupDirectoryFromIis
                client.PickupDirectoryLocation = "C:\inetpub\mailroot\Pickup"
                ''
                client.Send(mailMsg)
                Send_Notification = True
                ''
            End If

        Catch emailEx As Exception : _Debug.Print_("Email.Send_Notification(): " & emailEx.ToString)
            'Catch emailEx As Exception : _Debug.Print2File("Email.Send_Notification(): " & emailEx.ToString, "c:\LCTrack_TutorAppointment\ErrLog.txt")
        Finally
            ' release the hook:
            mailMsg.Dispose()
        End Try

    End Function
    Public Function Send_Notification(ByVal smtp As _MySMTP_MailObject) As Boolean
        Send_Notification = False
        Try
            '
            If String.Empty = smtp.EmailSet.From.Address Then
                Return False
            End If
            '
            ' SMTP Server:
            Dim client As New SmtpClient(smtp.Server, smtp.Port)
            Dim creds As New System.Net.NetworkCredential(smtp.UserName, smtp.UserPassword)
            '
            client.Credentials = creds
            client.DeliveryMethod = SmtpDeliveryMethod.Network
            client.EnableSsl = smtp.Encrypted ''ol#1.2.00(5/7)... 'encrypted connection' check box was added, since not all the email providers using it yet.
            '
            client.Send(smtp.EmailSet)
            Call log_Notification(smtp)
            Send_Notification = True
            '
            If smtp.EmailCopy Then
                smtp.EmailSet.To.Clear()
                smtp.EmailSet.To.Add(smtp.UserName)
                smtp.EmailSet.Subject = "Copy: " & smtp.EmailSet.Subject
                client.Send(smtp.EmailSet)
                Call log_Notification(smtp)
            End If
            '
        Catch emailEx As Exception : smtp.ErrorMsg = emailEx.Message : Call log_Notification(smtp) : _Debug.Print_("Email.Send_Notification(): " & smtp.ErrorMsg)
        Finally : smtp.EmailSet.Dispose()
        End Try

    End Function

    Private Sub log_Notification(ByVal smtp As _MySMTP_MailObject)
        ''ol#1.2.14(9/14)... 'Keep a Log of all Notifications' check-box option was added.
        If Not String.IsNullOrEmpty(smtp.NotificationLog) AndAlso _Files.IsFileExist(smtp.NotificationLog, False) Then
            If String.IsNullOrEmpty(smtp.ErrorMsg) Then
                _Files.WriteFile_ByOneString(String.Format("{4}{0},{1},{2},{3}", Date.Now, smtp.EmailSet.To(0), smtp.EmailSet.Subject, "Success", Environment.NewLine), smtp.NotificationLog, True)
            Else
                _Files.WriteFile_ByOneString(String.Format("{4}{0},{1},{2},{3}", Date.Now, smtp.EmailSet.To(0), smtp.EmailSet.Subject, "Error: " & smtp.ErrorMsg, Environment.NewLine), smtp.NotificationLog, True)
            End If
        End If
    End Sub
    Private Function set_EmailPriority(ByVal emailPriority As String) As Net.Mail.MailPriority
        Select Case emailPriority
            Case "high" : set_EmailPriority = MailPriority.High
            Case "low" : set_EmailPriority = MailPriority.Low
            Case Else : set_EmailPriority = MailPriority.Normal
        End Select
    End Function
    Private Function isSMTPservice_Installed() As Boolean
        isSMTPservice_Installed = False

        ' Ensure the SMTP Service is installed.
        Dim services() As ServiceController = ServiceController.GetServices
        Dim service As ServiceController = Nothing

        ' Loop through all the services on the machine and find the SMTP Service.
        For Each service In services
            If service.ServiceName.ToLower = "smtpsvc" Then 'SMTPSVC
                isSMTPservice_Installed = True
                Exit For
            End If
        Next service

        'If Not isSMTPService_Installed Then
        '    If service.CanStop Then
        '        service.Stop()
        '    End If
        'End If

        ' Ensure the SMTP Service is running. If not, start it.
        If Not service.Status = ServiceControllerStatus.Running Then
            service.Start()
        End If

    End Function
    Private Function isAttachmentExist(ByVal drow As DataRow) As Boolean
        isAttachmentExist = False
        Try
            For i As Integer = 0 To drow.Table.Columns.Count - 1
                If drow.Table.Columns(i).ColumnName.Contains("emailAttachment") Then
                    isAttachmentExist = Not String.IsNullOrEmpty(drow("emailAttachment").ToString)
                    Exit For
                End If
            Next i
        Catch ex As Exception
        End Try
    End Function
    Public Function IsValid_EmailFormat(ByVal email As String) As Boolean
        Dim mailPattern = New Regex("@\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*")
        Return mailPattern.IsMatch(email)
    End Function
    Private Function send(ByVal drow As DataRow, Optional ByVal isitTest As Boolean = False) As Boolean

        Dim mailMsg As New MailMessage
        Dim emailAttachment As Net.Mail.Attachment = Nothing

        Try
            With mailMsg
                .From = New MailAddress(_Email.EmailFrom, "MVCC Important Message")
                If isitTest Then
                    ' for test we use EmailFrom address to email to and from:
                    .To.Add(New MailAddress(_Email.EmailFrom, "MVCC TEST Message"))
                Else
                    Dim splitToAddr() As String = drow("emailTo").ToString.Split(",")
                    For i As Short = 0 To splitToAddr.Length - 1
                        .To.Add(New MailAddress(splitToAddr(i).Trim))
                    Next i
                End If
                .Priority = MailPriority.High
                .Subject = _Email.EmailSubject
                '.Body = "this is my test email body.<br><b>this part is in bold</b>"
                '
                If isAttachmentExist(drow) Then
                    If _Files.IsFileExist(drow("emailAttachment").ToString, False) Then
                        emailAttachment = New Net.Mail.Attachment(drow("emailAttachment").ToString)
                        .Attachments.Add(emailAttachment)
                    End If
                End If
                '
                .IsBodyHtml = True
                .Body = _Email.EmailBodyHTML
                '
                Dim client As New SmtpClient
                client.DeliveryMethod = SmtpDeliveryMethod.PickupDirectoryFromIis

                client.Send(mailMsg)
                send = True
                '
            End With

        Catch ex As Exception
            '' catching error to just record the Activity:
            '' and raising the error again for the caller to catch:
            _Debug.Print2File(ex.ToString, String.Empty)
            Throw ex
        Finally
            ' release the hook for attachments:
            ''emailAttachment.Dispose()
            ''mailMsg.Attachments.Clear()
            ''mailMsg.Attachments.Dispose()
            mailMsg.Dispose()
        End Try

    End Function

End Module

Public Class _MySMTP_MailObject
    Public UserName As String
    Public UserPassword As String
    Public Server As String
    Public Port As Integer
    Public Encrypted As Boolean ''ol#1.2.00(5/7)... 'encrypted connection' check box was added, since not all the email providers using it yet.
    Public ErrorMsg As String
    Public EmailSet As New MailMessage()
    Public EmailCopy As Boolean
    Public NotificationLog As String ''ol#1.2.14(9/14)... 'Keep a Log of all Notifications' check-box option was added.
End Class

Public Module _EmailSetup

    Public qq As Char = Microsoft.VisualBasic.ChrW(34) ' double quote

    Public Const file_YouHaveAPackageInMbox_SMS As String = "You have a package in your Mailbox.txt"
    Public Const file_YouHaveALetterInMbox_SMS As String = "You have a letter in your Mailbox.txt"
    Public Const file_YouHaveAPackageInMbox As String = "You have a package in your Mailbox.rtf"
    Public Const file_YouHaveALetterInMbox As String = "You have a letter in your Mailbox.rtf"
    Public Const file_YourReceiptAttached As String = "Your Receipt is Attached.rtf"
    Public Const file_YourARStatementAttached As String = "Your AR Statement is Attached.rtf"
    Public Const file_YourMBXStatementAttached As String = "Your MBX Statement is Attached.rtf"
    Public Const file_ReviewYourCustomerExperience As String = "Review Your Customer Experience.rtf"

    Public StoreOwner As New _baseContact
    Public StoreSMTP As New _MySMTP_MailObject

    Public NotificationsFolder As String
    Public NotificationsBINFolder As String
    Public NotificationLogFile As String

    Public Sub set_Paths(ByVal dbPath As String)
        NotificationsFolder = String.Format("{0}\Notifications\", dbPath)
        NotificationsBINFolder = String.Format("{0}\BIN\Notifications\", dbPath)
        EmailNotificationsDb.Path2db = (NotificationsFolder & "EmailNotifications.mdb")
        NotificationLogFile = String.Format("{0}NotificationLog.csv", NotificationsFolder)
    End Sub
    Public Sub Open_EmailSetupForm(ByVal dbPath As String)
        Call set_Paths(dbPath)
        ''TODO: connect to Email Setup Window?
        'EmailSetup.ShowDialog()
        'EmailSetup.Dispose()
    End Sub

#Region "Send HTML Receipt"
    Public Function Send_HTMLEmail(ByVal EmailPackages As Collection, ByVal emailSubject As String) As Boolean
        Send_HTMLEmail = False ' assume.
        If 0 < EmailPackages.Count Then
            If load_SMTP_Settings() Then
                If StoreOwner Is Nothing Or StoreOwner.ContactID = 0 Then
                    ShipRiteDb.Setup_GetAddress_StoreOwner(StoreOwner)
                End If
                '
                For i As Integer = 1 To EmailPackages.Count
                    Dim epack As _EmailPackage = EmailPackages(i) ' grab the first one in the list
                    If Not String.IsNullOrEmpty(epack.EmailTo) AndAlso _Controls.Contains(epack.EmailTo, "@") Then
                        '
                        '
                        'Dim imageLink As New LinkedResource("C:\Backup\My Pictures\MVCC Logo Email Signature.jpg")
                        'imageLink.ContentId = "MyLogo"
                        'Dim altview As AlternateView = AlternateView.CreateAlternateViewFromString(epack.HTMLBody)
                        'altview.LinkedResources.Add(imageLink)

                        StoreSMTP.EmailSet = New MailMessage
                        StoreSMTP.EmailSet.From = New MailAddress(StoreSMTP.UserName, StoreOwner.CompanyName)
                        StoreSMTP.EmailSet.To.Add(epack.EmailTo.Trim)
                        StoreSMTP.EmailSet.IsBodyHtml = True
                        StoreSMTP.EmailSet.Subject = StoreOwner.CompanyName & " " & emailSubject
                        StoreSMTP.EmailSet.Body = epack.HTMLBody
                        StoreSMTP.EmailSet.Priority = MailPriority.High
                        '
                        'Dim attch As New Attachment("C:\Backup\My Pictures\MVCC Logo Email Signature.jpg")
                        'attch.ContentId = "MyLogo"

                        'StoreSMTP.EmailSet.Attachments.Add(attch)

                        '
                        Send_HTMLEmail = _Email.Send_Notification(StoreSMTP)
                        '
                        StoreSMTP.EmailSet = Nothing
                    End If
                Next
            End If
        End If
    End Function
#End Region
#Region "Send Notification"
    Public Function Send_NotificationEmail(ByVal emailBody As String, ByVal emailSubject As String, ByVal emailTo As Object, ByVal emailAttachment As String) As Boolean
        Send_NotificationEmail = False ' assume.
        If 0 < emailTo.Count Then
            If load_SMTP_Settings() Then
                '
                If StoreOwner Is Nothing Or StoreOwner.ContactID = 0 Then
                    ShipRiteDb.Setup_GetAddress_StoreOwner(StoreOwner)
                End If
                '
                For i As Integer = 1 To emailTo.Count ' Collection
                    Dim emailaddr As String = emailTo(i)
                    If Not String.Empty = emailaddr AndAlso _Controls.Contains(emailaddr, "@") Then
                        '
                        StoreSMTP.EmailSet = New MailMessage
                        StoreSMTP.EmailSet.From = New MailAddress(StoreSMTP.UserName, StoreOwner.CompanyName)
                        StoreSMTP.EmailSet.To.Add(emailaddr.Trim)
                        StoreSMTP.EmailSet.IsBodyHtml = True
                        StoreSMTP.EmailSet.Subject = emailSubject
                        Dim sb As New StringBuilder
                        sb.AppendLine("<html><body>")
                        sb.AppendLine(String.Format("<font size={0}2{0} face={0}Verdana{0} color={0}#003366{0}><br/>", qq))
                        sb.AppendLine(emailBody.Replace(vbNewLine, "<br/>"))
                        sb.AppendLine("</font></body></html>")
                        StoreSMTP.EmailSet.Body = sb.ToString
                        StoreSMTP.EmailSet.Priority = MailPriority.High
                        If Not String.IsNullOrEmpty(emailAttachment) Then
                            If _Files.IsFileExist(emailAttachment, False) Then
                                Dim attch As New Attachment(emailAttachment)
                                StoreSMTP.EmailSet.Attachments.Add(attch)
                            End If
                        End If
                        '
                        Send_NotificationEmail = _Email.Send_Notification(StoreSMTP)
                        '
                        StoreSMTP.EmailSet = Nothing
                        '
                    End If
                Next
            End If
        End If

    End Function
    Public Function Send_NotificationEmail(ByVal EmailPackages As Collection, ByVal notifyFileName As String, ByVal gDBpath As String) As Boolean
        Send_NotificationEmail = False
        Call set_Paths(gDBpath)


        If load_SMTP_Settings() Then
            If StoreOwner Is Nothing Or StoreOwner.ContactID = 0 Then
                ShipRiteDb.Setup_GetAddress_StoreOwner(StoreOwner)
            End If
            Dim item As New _NotificationObject
            ' load notification file:
            If load_FileNotification(notifyFileName, item) Then
                Do While EmailPackages.Count > 0
                    If ".txt" = _Files.Get_FileExtension(notifyFileName).ToLower Then
                        If smtp_SendSMS(EmailPackages, item) Then
                            Send_NotificationEmail = True
                        End If
                    Else
                        If smtp_SendEmail(EmailPackages, item) Then
                            Send_NotificationEmail = True
                        End If
                    End If
                Loop
            End If
            item = Nothing
        End If

    End Function
    Public Function Send_NotificationEmail(ByVal EmailPackages As List(Of _EmailMboxPackageItems), ByVal notifyFileName As String, ByVal gDBpath As String) As Boolean
        Send_NotificationEmail = False
        Call set_Paths(gDBpath)

        If load_SMTP_Settings() Then
            If StoreOwner Is Nothing Or StoreOwner.ContactID = 0 Then
                ShipRiteDb.Setup_GetAddress_StoreOwner(StoreOwner)
            End If
            Dim item As New _NotificationObject
            ' load notification file:
            If load_FileNotification(notifyFileName, item) Then
                For Each epack As _EmailMboxPackageItems In EmailPackages
                    If ".txt" = _Files.Get_FileExtension(notifyFileName).ToLower Then
                        If smtp_SendSMS(epack, item) Then
                            Send_NotificationEmail = True
                        End If
                    Else
                        If smtp_SendEmail(epack, item) Then
                            Send_NotificationEmail = True
                        End If
                    End If
                Next
            End If
            item = Nothing
        End If

    End Function

    Private Function load_SMTP_Settings() As Boolean
        load_SMTP_Settings = False ' assume.
        Dim segment As String = ""
        If ShipRiteDb.Setup2_GetSMTP_Settings(segment) Then
            StoreSMTP.UserName = _Convert.Null2DefaultValue(ExtractElementFromSegment("SMTPUserID", segment)) 'dreader("SMTPUserID"))
            StoreSMTP.UserPassword = _Convert.Null2DefaultValue(ExtractElementFromSegment("SMTPUserPassword", segment)) 'dreader("SMTPUserPassword"))
            StoreSMTP.Server = _Convert.Null2DefaultValue(ExtractElementFromSegment("SMTPServerName", segment)) 'dreader("SMTPServerName"))
            StoreSMTP.Port = _Convert.Null2DefaultValue(ExtractElementFromSegment("SMTPServerPort", segment, "587"), "587") 'dreader("SMTPServerPort"), "587")
            StoreSMTP.Encrypted = _Convert.Null2DefaultValue(ExtractElementFromSegment("SMTPServerEncrypted", segment, "False"), False) 'dreader("SMTPServerEncrypted"), False)
            StoreSMTP.EmailCopy = _Convert.Null2DefaultValue(ExtractElementFromSegment("SMTPUserID", segment, "False"), False) 'dreader("SMTPEmailCopy"), False)
            StoreSMTP.NotificationLog = _Convert.Null2DefaultValue(ExtractElementFromSegment("SMTPUserID", segment)) 'dreader("SMTPNotificationLog"))
        End If
        If String.Empty = StoreSMTP.Server Then
            If _MsgBox.QuestionMessage("Your outgoing email server needs to be configured in Email Setup screen.", "Load Email Setup?") Then
                ''TODO: connect to Email Setup Window?
                'EmailSetup.ShowDialog()
                'EmailSetup.Dispose()
                'Call load_SMTP_Settings()
            End If
        End If
        load_SMTP_Settings = (Not String.Empty = StoreSMTP.Server)
    End Function
    Private Function load_FileNotification(ByVal notifyFileName As String, ByRef item As _NotificationObject) As Boolean
        Dim segment As String = ""
        If EmailNotificationsDb.Read_Notification(notifyFileName, segment) Then
            item.NotificationID = Val(ExtractElementFromSegment("NotificationID", segment)) 'dreader("NotificationID")
            item.FileName = ExtractElementFromSegment("FileName", segment) 'dreader("FileName")
            item.EmailSubject = ExtractElementFromSegment("EmailSubject", segment) 'dreader("EmailSubject")
        End If
        load_FileNotification = (Not 0 = item.NotificationID)
    End Function

    Private Function smtp_SendSMS(ByRef EmailPackages As Collection, ByVal item As _NotificationObject) As Boolean
        smtp_SendSMS = False ' assume.
        Dim rtf2html As String = String.Empty
        If 0 < EmailPackages.Count Then
            Dim epack As _EmailPackage = EmailPackages(1) ' grab the first one in the list
            If Not String.Empty = epack.EmailTo AndAlso _Controls.Contains(epack.EmailTo, "@") Then
                '
                StoreSMTP.EmailSet = New MailMessage
                StoreSMTP.EmailSet.From = New MailAddress(StoreSMTP.UserName, StoreOwner.CompanyName)
                StoreSMTP.EmailSet.To.Add(epack.EmailTo.Trim)
                '
                If _Files.ReadFile_ToEnd(_EmailSetup.NotificationsFolder & item.FileName, False, StoreSMTP.EmailSet.Body) Then
                    Select Case item.FileName
                        Case file_YouHaveALetterInMbox_SMS, file_YouHaveAPackageInMbox_SMS
                            Call replace_YouHaveAPackageInMbox_SMS(EmailPackages, StoreSMTP.EmailSet.Body)
                    End Select
                    StoreSMTP.EmailSet.Body = StoreSMTP.EmailSet.Body.Replace("%Customer%", _Convert.LastFirstName2FirstLastName(epack.CustomerName))
                    StoreSMTP.EmailSet.Body = StoreSMTP.EmailSet.Body.Replace("%StoreOwnerName%", StoreOwner.FNameLName)
                    StoreSMTP.EmailSet.Body = StoreSMTP.EmailSet.Body.Replace("%StoreName%", StoreOwner.CompanyName)
                    Dim addr As New StringBuilder
                    addr.AppendLine(String.Format(" {0} ", StoreOwner.Addr1))
                    If Not String.Empty = StoreOwner.Addr2 Then
                        addr.AppendLine(String.Format("{0} ", StoreOwner.Addr2))
                    End If
                    addr.AppendLine(StoreOwner.CityStateZip)
                    StoreSMTP.EmailSet.Body = StoreSMTP.EmailSet.Body.Replace("%StoreAddress%", addr.ToString)
                    StoreSMTP.EmailSet.Body = StoreSMTP.EmailSet.Body.Replace("%StorePhone%", StoreOwner.Tel)
                    '
                    StoreSMTP.EmailSet.IsBodyHtml = True
                    StoreSMTP.EmailSet.Subject = item.EmailSubject
                    StoreSMTP.EmailSet.Priority = MailPriority.High
                    smtp_SendSMS = _Email.Send_Notification(StoreSMTP)
                    addr = Nothing
                    If Not String.IsNullOrEmpty(StoreSMTP.ErrorMsg) Then
                        Throw New System.Exception(StoreSMTP.ErrorMsg)
                    End If
                End If
                StoreSMTP.EmailSet = Nothing
            Else
                ' delete item without email address
                EmailPackages.Remove(1)
            End If
        End If
    End Function
    Private Function smtp_SendSMS(ByRef epack As _EmailMboxPackageItems, ByVal item As _NotificationObject) As Boolean
        smtp_SendSMS = False ' assume.
        Dim rtf2html As String = String.Empty
        If epack IsNot Nothing Then ''ol#1.2.24(11/18)... Don't use 'Do While' loop to avoid mass-duplicated emails.
            If Not String.IsNullOrEmpty(epack.EmailTo) AndAlso _Controls.Contains(epack.EmailTo, "@") Then
                '
                StoreSMTP.EmailSet = New MailMessage
                StoreSMTP.EmailSet.From = New MailAddress(StoreSMTP.UserName, StoreOwner.CompanyName)
                StoreSMTP.EmailSet.To.Add(epack.EmailTo.Trim)
                '
                If _Files.ReadFile_ToEnd(_EmailSetup.NotificationsFolder & item.FileName, False, StoreSMTP.EmailSet.Body) Then
                    Select Case item.FileName
                        Case file_YouHaveALetterInMbox_SMS, file_YouHaveAPackageInMbox_SMS
                            Call replace_YouHaveAPackageInMbox_SMS(epack, StoreSMTP.EmailSet.Body)
                    End Select
                    StoreSMTP.EmailSet.Body = StoreSMTP.EmailSet.Body.Replace("%Customer%", _Convert.LastFirstName2FirstLastName(epack.CustomerName))
                    StoreSMTP.EmailSet.Body = StoreSMTP.EmailSet.Body.Replace("%StoreOwnerName%", StoreOwner.FNameLName)
                    StoreSMTP.EmailSet.Body = StoreSMTP.EmailSet.Body.Replace("%StoreName%", StoreOwner.CompanyName)
                    Dim addr As New StringBuilder
                    addr.AppendLine(String.Format(" {0} ", StoreOwner.Addr1))
                    If Not String.Empty = StoreOwner.Addr2 Then
                        addr.AppendLine(String.Format("{0} ", StoreOwner.Addr2))
                    End If
                    addr.AppendLine(StoreOwner.CityStateZip)
                    StoreSMTP.EmailSet.Body = StoreSMTP.EmailSet.Body.Replace("%StoreAddress%", addr.ToString)
                    StoreSMTP.EmailSet.Body = StoreSMTP.EmailSet.Body.Replace("%StorePhone%", StoreOwner.Tel)
                    '
                    StoreSMTP.EmailSet.IsBodyHtml = True
                    StoreSMTP.EmailSet.Subject = item.EmailSubject
                    StoreSMTP.EmailSet.Priority = MailPriority.High
                    smtp_SendSMS = _Email.Send_Notification(StoreSMTP)
                    addr = Nothing
                    If Not String.IsNullOrEmpty(StoreSMTP.ErrorMsg) Then
                        Throw New System.Exception(StoreSMTP.ErrorMsg)
                    End If
                End If
                StoreSMTP.EmailSet = Nothing
            End If
        End If
    End Function
    Private Function smtp_SendEmail(ByRef EmailPackages As Collection, ByVal item As _NotificationObject) As Boolean
        smtp_SendEmail = False ' assume.
        Dim rtf2html As String = String.Empty
        If 0 < EmailPackages.Count Then
            Dim epack As _EmailPackage = EmailPackages(1) ' grab the first one in the list
            If Not String.IsNullOrEmpty(epack.EmailTo) AndAlso _Controls.Contains(epack.EmailTo, "@") Then
                '
                StoreSMTP.EmailSet = New MailMessage
                StoreSMTP.EmailSet.From = New MailAddress(StoreSMTP.UserName, StoreOwner.CompanyName)
                StoreSMTP.EmailSet.To.Add(epack.EmailTo.Trim)
                '
                Dim rtbEmailBody As New RichTextBox()
                'rtbEmailBody.LoadFile(_EmailSetup.NotificationsFolder & item.FileName)
                If _Files.IsFileExist(_EmailSetup.NotificationsFolder & item.FileName, False) Then
                    Using fStream As New IO.FileStream(_EmailSetup.NotificationsFolder & item.FileName, IO.FileMode.OpenOrCreate)
                        rtbEmailBody.Selection.Load(fStream, DataFormats.Rtf)
                    End Using
                End If
                If epack.CustomerName IsNot Nothing AndAlso Not String.IsNullOrEmpty(epack.CustomerName) Then
                    'rtbEmailBody.Rtf = rtbEmailBody.Rtf.Replace("%Customer%", _Convert.LastFirstName2FirstLastName(epack.CustomerName))
                    rtbEmailBody.Selection.Text = rtbEmailBody.Selection.Text.Replace("%Customer%", _Convert.LastFirstName2FirstLastName(epack.CustomerName))
                Else
                    'rtbEmailBody.Rtf = rtbEmailBody.Rtf.Replace("%Customer%", "Customer")
                    rtbEmailBody.Selection.Text = rtbEmailBody.Selection.Text.Replace("%Customer%", "Customer")
                End If
                Select Case item.FileName
                    Case file_YouHaveAPackageInMbox, file_YouHaveALetterInMbox
                        Call replace_YouHaveAPackageInMbox(EmailPackages, rtbEmailBody)
                    Case file_YourReceiptAttached
                        If _Files.IsFileExist("C:\ShipRite\Receipts\Invoice.pdf", False) Then
                            Dim att As New Attachment("C:\ShipRite\Receipts\Invoice.pdf")
                            StoreSMTP.EmailSet.Attachments.Add(att)
                            EmailPackages.Remove(1)
                        End If
                    Case file_YourARStatementAttached, file_YourMBXStatementAttached, file_ReviewYourCustomerExperience
                        If epack.Location IsNot Nothing AndAlso Not String.IsNullOrEmpty(epack.Location) Then
                            If _Files.IsFileExist(epack.Location, False) Then
                                Dim attch As New Attachment(epack.Location)
                                StoreSMTP.EmailSet.Attachments.Add(attch)
                            End If
                        End If
                        If item.FileName = file_YourMBXStatementAttached Then
                            If epack.Notes IsNot Nothing AndAlso Not String.IsNullOrEmpty(epack.Notes) Then
                                'rtbEmailBody.Rtf = rtbEmailBody.Rtf.Replace("%MailboxNoticeType%", epack.Notes)
                                rtbEmailBody.Selection.Text = rtbEmailBody.Selection.Text.Replace("%MailboxNoticeType%", epack.Notes)
                            Else
                                'rtbEmailBody.Rtf = rtbEmailBody.Rtf.Replace("%MailboxNoticeType%", "Mailbox")
                                rtbEmailBody.Selection.Text = rtbEmailBody.Selection.Text.Replace("%MailboxNoticeType%", "Mailbox")
                            End If
                        End If
                        EmailPackages.Remove(1)
                End Select
                'rtbEmailBody.Rtf = rtbEmailBody.Rtf.Replace("%StoreOwnerName%", StoreOwner.FNameLName)
                'rtbEmailBody.Rtf = rtbEmailBody.Rtf.Replace("%StoreName%", StoreOwner.CompanyName)
                rtbEmailBody.Selection.Text = rtbEmailBody.Selection.Text.Replace("%StoreOwnerName%", StoreOwner.FNameLName)
                rtbEmailBody.Selection.Text = rtbEmailBody.Selection.Text.Replace("%StoreName%", StoreOwner.CompanyName)
                Dim addr As New StringBuilder
                addr.AppendLine(String.Format("{0}\par", StoreOwner.Addr1))
                If Not String.Empty = StoreOwner.Addr2 Then
                    addr.AppendLine(String.Format("{0}\par", StoreOwner.Addr2))
                End If
                addr.AppendLine(StoreOwner.CityStateZip)
                'rtbEmailBody.Rtf = rtbEmailBody.Rtf.Replace("%StoreAddress%", addr.ToString)
                'rtbEmailBody.Rtf = rtbEmailBody.Rtf.Replace("%StorePhone%", StoreOwner.Tel)
                rtbEmailBody.Selection.Text = rtbEmailBody.Selection.Text.Replace("%StoreAddress%", addr.ToString)
                rtbEmailBody.Selection.Text = rtbEmailBody.Selection.Text.Replace("%StorePhone%", StoreOwner.Tel)
                If _RTF2HTML.Convert_(rtbEmailBody.Selection.Text, rtf2html) Then 'rtbEmailBody.Rtf, rtf2html) Then
                    StoreSMTP.EmailSet.IsBodyHtml = True
                    StoreSMTP.EmailSet.Subject = item.EmailSubject
                    StoreSMTP.EmailSet.Body = rtf2html
                    StoreSMTP.EmailSet.Priority = MailPriority.High
                    smtp_SendEmail = _Email.Send_Notification(StoreSMTP)
                End If
                addr = Nothing
                'rtbEmailBody.Dispose()
                rtbEmailBody = Nothing
                If Not String.IsNullOrEmpty(StoreSMTP.ErrorMsg) Then
                    Throw New System.Exception(StoreSMTP.ErrorMsg)
                End If
                StoreSMTP.EmailSet = Nothing
            Else
                ' delete item without email address
                EmailPackages.Remove(1)
            End If
        End If
    End Function
    Private Function smtp_SendEmail(ByRef epack As _EmailMboxPackageItems, ByVal item As _NotificationObject) As Boolean
        smtp_SendEmail = False ' assume.
        Dim rtf2html As String = String.Empty
        If epack IsNot Nothing Then
            If Not String.IsNullOrEmpty(epack.EmailTo) AndAlso _Controls.Contains(epack.EmailTo, "@") Then
                '
                StoreSMTP.EmailSet = New MailMessage
                StoreSMTP.EmailSet.From = New MailAddress(StoreSMTP.UserName, StoreOwner.CompanyName)
                StoreSMTP.EmailSet.To.Add(epack.EmailTo.Trim)
                '
                Dim rtbEmailBody As New RichTextBox()
                'rtbEmailBody.LoadFile(_EmailSetup.NotificationsFolder & item.FileName)
                If _Files.IsFileExist(_EmailSetup.NotificationsFolder & item.FileName, False) Then
                    Using fStream As New IO.FileStream(_EmailSetup.NotificationsFolder & item.FileName, IO.FileMode.OpenOrCreate)
                        rtbEmailBody.Selection.Load(fStream, DataFormats.Rtf)
                    End Using
                End If
                If epack.CustomerName IsNot Nothing AndAlso Not String.IsNullOrEmpty(epack.CustomerName) Then
                    'rtbEmailBody.Rtf = rtbEmailBody.Rtf.Replace("%Customer%", _Convert.LastFirstName2FirstLastName(epack.CustomerName))
                    rtbEmailBody.Selection.Text = rtbEmailBody.Selection.Text.Replace("%Customer%", _Convert.LastFirstName2FirstLastName(epack.CustomerName))
                Else
                    'rtbEmailBody.Rtf = rtbEmailBody.Rtf.Replace("%Customer%", "Customer")
                    rtbEmailBody.Selection.Text = rtbEmailBody.Selection.Text.Replace("%Customer%", "Customer")
                End If
                Select Case item.FileName
                    Case file_YouHaveAPackageInMbox, file_YouHaveALetterInMbox
                        Call replace_YouHaveAPackageInMbox(epack, rtbEmailBody)
                    Case file_YourReceiptAttached
                        If _Files.IsFileExist("C:\ShipRite\Receipts\Invoice.pdf", False) Then
                            Dim att As New Attachment("C:\ShipRite\Receipts\Invoice.pdf")
                            StoreSMTP.EmailSet.Attachments.Add(att)
                        End If
                End Select
                'rtbEmailBody.Rtf = rtbEmailBody.Rtf.Replace("%StoreOwnerName%", StoreOwner.FNameLName)
                'rtbEmailBody.Rtf = rtbEmailBody.Rtf.Replace("%StoreName%", StoreOwner.CompanyName)
                rtbEmailBody.Selection.Text = rtbEmailBody.Selection.Text.Replace("%StoreOwnerName%", StoreOwner.FNameLName)
                rtbEmailBody.Selection.Text = rtbEmailBody.Selection.Text.Replace("%StoreName%", StoreOwner.CompanyName)
                Dim addr As New StringBuilder
                addr.AppendLine(String.Format("{0}\par", StoreOwner.Addr1))
                If Not String.Empty = StoreOwner.Addr2 Then
                    addr.AppendLine(String.Format("{0}\par", StoreOwner.Addr2))
                End If
                addr.AppendLine(StoreOwner.CityStateZip)
                'rtbEmailBody.Rtf = rtbEmailBody.Rtf.Replace("%StoreAddress%", addr.ToString)
                'rtbEmailBody.Rtf = rtbEmailBody.Rtf.Replace("%StorePhone%", StoreOwner.Tel)
                rtbEmailBody.Selection.Text = rtbEmailBody.Selection.Text.Replace("%StoreAddress%", addr.ToString)
                rtbEmailBody.Selection.Text = rtbEmailBody.Selection.Text.Replace("%StorePhone%", StoreOwner.Tel)
                If _RTF2HTML.Convert_(rtbEmailBody.Selection.Text, rtf2html) Then 'rtbEmailBody.Rtf, rtf2html) Then
                    StoreSMTP.EmailSet.IsBodyHtml = True
                    StoreSMTP.EmailSet.Subject = item.EmailSubject
                    StoreSMTP.EmailSet.Body = rtf2html
                    StoreSMTP.EmailSet.Priority = MailPriority.High
                    smtp_SendEmail = _Email.Send_Notification(StoreSMTP)
                End If
                addr = Nothing
                'rtbEmailBody.Dispose()
                rtbEmailBody = Nothing
                If Not String.IsNullOrEmpty(StoreSMTP.ErrorMsg) Then
                    Throw New System.Exception(StoreSMTP.ErrorMsg)
                End If
                StoreSMTP.EmailSet = Nothing
            End If
        End If
    End Function

    Private Sub replace_YouHaveAPackageInMbox_SMS(ByRef EmailPackages As Collection, ByRef txtEmailBody As String)
        Dim packs As New StringBuilder
        Dim count As Integer = 0
        For i As Integer = EmailPackages.Count To 1 Step -1
            Dim epack As _EmailPackage = EmailPackages(i)
            ' when creating EmailPackages collection make sure no Empty emails are added.
            If StoreSMTP.EmailSet.To(0).Address = epack.EmailTo Then
                count += 1
                packs.AppendLine(String.Format(" {0}. {1}:  tracking number is {2}", count, epack.Carrier, epack.TrackingNo))
                ''ol#1.2.03(6/3)... %Notes% and %Location% fields are added to Email body.
                If Not String.IsNullOrEmpty(epack.Notes) Then
                    packs.AppendLine(String.Format("note: {0}", epack.Notes))
                End If
                If Not String.IsNullOrEmpty(epack.Location) Then
                    packs.AppendLine(String.Format("location: {0}", epack.Location))
                End If
                EmailPackages.Remove(i)
            End If
        Next i
        If 1 = count Then
            txtEmailBody = txtEmailBody.Replace("%Packages#%", "1 package")
        Else
            txtEmailBody = txtEmailBody.Replace("%Packages#%", String.Format("{0} packages", count.ToString))
        End If
        txtEmailBody = txtEmailBody.Replace("%Carrier: Tracking#%", packs.ToString)
        packs = Nothing
    End Sub
    Private Sub replace_YouHaveAPackageInMbox_SMS(ByRef epack As _EmailMboxPackageItems, ByRef txtEmailBody As String)
        Dim packs As New StringBuilder
        Dim count As Integer = 0
        For Each eitem As _EmailMboxPackageItem In epack.PackageItems
            ' when creating EmailPackages collection make sure no Empty emails are added.
            If StoreSMTP.EmailSet.To(0).Address = epack.EmailTo Then
                count += 1
                packs.AppendLine(String.Format(" {0}. {1}:  tracking number is {2}", count, eitem.Carrier, eitem.TrackingNo))
                If Not String.IsNullOrEmpty(eitem.Notes) Then
                    packs.AppendLine(String.Format("note: {0}", eitem.Notes))
                End If
                If Not String.IsNullOrEmpty(eitem.Location) Then
                    packs.AppendLine(String.Format("location: {0}", eitem.Location))
                End If
            End If
        Next
        If 1 = count Then
            txtEmailBody = txtEmailBody.Replace("%Packages#%", "1 package")
        Else
            txtEmailBody = txtEmailBody.Replace("%Packages#%", String.Format("{0} packages", count.ToString))
        End If
        txtEmailBody = txtEmailBody.Replace("%Carrier: Tracking#%", packs.ToString)
        packs = Nothing
    End Sub
    Private Sub replace_YouHaveAPackageInMbox(ByRef EmailPackages As Collection, ByRef rtbEmailBody As RichTextBox)
        Dim packs As New StringBuilder
        Dim count As Integer = 0
        For i As Integer = EmailPackages.Count To 1 Step -1
            Dim epack As _EmailPackage = EmailPackages(i)
            ' when creating EmailPackages collection make sure no Empty emails are added.
            If StoreSMTP.EmailSet.To(0).Address = epack.EmailTo Then
                count += 1
                packs.AppendLine(String.Format("{0}.  {1}:  tracking number is {2}\par", count, epack.Carrier, epack.TrackingNo))
                If Not String.IsNullOrEmpty(epack.Notes) Then
                    packs.AppendLine(String.Format("notes: {0}\par", epack.Notes))
                End If
                If Not String.IsNullOrEmpty(epack.Location) Then
                    packs.AppendLine(String.Format("location: {0}\par", epack.Location))
                End If
                EmailPackages.Remove(i)
            End If
        Next i
        If 1 = count Then
            'rtbEmailBody.Rtf = rtbEmailBody.Rtf.Replace("%Packages#%", "1 package")
            rtbEmailBody.Selection.Text = rtbEmailBody.Selection.Text.Replace("%Packages#%", "1 package")
        Else
            'rtbEmailBody.Rtf = rtbEmailBody.Rtf.Replace("%Packages#%", String.Format("{0} packages", count.ToString))
            rtbEmailBody.Selection.Text = rtbEmailBody.Selection.Text.Replace("%Packages#%", String.Format("{0} packages", count.ToString))
        End If
        'rtbEmailBody.Rtf = rtbEmailBody.Rtf.Replace("%Carrier: Tracking#%", packs.ToString)
        rtbEmailBody.Selection.Text = rtbEmailBody.Selection.Text.Replace("%Carrier: Tracking#%", packs.ToString)
        packs = Nothing
    End Sub
    Private Sub replace_YouHaveAPackageInMbox(ByRef epack As _EmailMboxPackageItems, ByRef rtbEmailBody As RichTextBox)
        Dim packs As New StringBuilder
        Dim count As Integer = 0
        For Each eitem As _EmailMboxPackageItem In epack.PackageItems
            ' when creating EmailPackages collection make sure no Empty emails are added.
            If StoreSMTP.EmailSet.To(0).Address = epack.EmailTo Then
                count += 1
                packs.AppendLine(String.Format("{0}.  {1}:  tracking number is {2}\par", count, eitem.Carrier, eitem.TrackingNo))
                If Not String.IsNullOrEmpty(eitem.Notes) Then
                    packs.AppendLine(String.Format("notes: {0}\par", eitem.Notes))
                End If
                If Not String.IsNullOrEmpty(eitem.Location) Then
                    packs.AppendLine(String.Format("location: {0}\par", eitem.Location))
                End If
            End If
        Next
        If 1 = count Then
            'rtbEmailBody.Rtf = rtbEmailBody.Rtf.Replace("%Packages#%", "1 package")
            rtbEmailBody.Selection.Text = rtbEmailBody.Selection.Text.Replace("%Packages#%", "1 package")
        Else
            'rtbEmailBody.Rtf = rtbEmailBody.Rtf.Replace("%Packages#%", String.Format("{0} packages", count.ToString))
            rtbEmailBody.Selection.Text = rtbEmailBody.Selection.Text.Replace("%Packages#%", String.Format("{0} packages", count.ToString))
        End If
        'rtbEmailBody.Rtf = rtbEmailBody.Rtf.Replace("%Carrier: Tracking#%", packs.ToString)
        rtbEmailBody.Selection.Text = rtbEmailBody.Selection.Text.Replace("%Carrier: Tracking#%", packs.ToString)
        packs = Nothing
    End Sub

#End Region
#Region "Send AR Statement"
    Public Function Email_AR_Statement(ByVal gDBpath As String, ByVal emailTo As Object, ByVal emailAttachment As String, ByVal emailCustomerName As String)
        Email_AR_Statement = False
        Dim EmailPackages As New Collection

        If emailTo.count > 0 Then
            For i As Integer = 1 To emailTo.Count
                Dim epack As New _EmailPackage
                epack.Carrier = String.Empty
                epack.TrackingNo = String.Empty
                epack.CustomerName = emailCustomerName
                epack.EmailTo = emailTo(i)
                epack.Location = emailAttachment
                EmailPackages.Add(epack)
            Next

            Email_AR_Statement = _EmailSetup.Send_NotificationEmail(EmailPackages, _EmailSetup.file_YourARStatementAttached, gDBpath)
        End If

    End Function
#End Region
#Region "Send MBX Statement"
    Public Function Email_MBX_Statement(ByVal gDBpath As String, ByVal emailTo As Object, ByVal emailAttachment As String, ByVal emailCustomerName As String, Optional ByVal mbxNoticeType As String = "")
        Email_MBX_Statement = False
        Dim EmailPackages As New Collection

        If emailTo.count > 0 Then
            For i As Integer = 1 To emailTo.Count
                Dim epack As New _EmailPackage
                epack.Carrier = String.Empty
                epack.TrackingNo = String.Empty
                epack.CustomerName = emailCustomerName
                epack.EmailTo = emailTo(i)
                epack.Location = emailAttachment
                If Not String.IsNullOrEmpty(mbxNoticeType) Then
                    epack.Notes = mbxNoticeType
                End If
                EmailPackages.Add(epack)
            Next

            Email_MBX_Statement = _EmailSetup.Send_NotificationEmail(EmailPackages, _EmailSetup.file_YourMBXStatementAttached, gDBpath)
        End If

    End Function
#End Region
#Region "Send Review Email"
    Public Function Email_CustomerReview(ByVal gDBpath As String, ByVal emailTo As Object, ByVal emailAttachment As String, ByVal emailCustomerName As String)
        Email_CustomerReview = False
        Dim EmailPackages As New Collection

        If emailTo.count > 0 Then
            For i As Integer = 1 To emailTo.Count
                Dim epack As New _EmailPackage
                epack.Carrier = String.Empty
                epack.TrackingNo = String.Empty
                epack.CustomerName = emailCustomerName
                epack.EmailTo = emailTo(i)
                epack.Location = emailAttachment
                EmailPackages.Add(epack)
            Next

            Email_CustomerReview = _EmailSetup.Send_NotificationEmail(EmailPackages, _EmailSetup.file_ReviewYourCustomerExperience, gDBpath)
        End If

    End Function
#End Region

End Module

Public Class _EmailPackage
    Public CustomerName As String
    Public EmailTo As String
    Public Carrier As String
    Public TrackingNo As String
    Public HTMLBody As String
    Public Notes As String
    Public Location As String
End Class
Friend Class _NotificationObject
    Public NotificationID As Long
    Public FileName As String
    Public EmailSubject As String

    Public Overrides Function ToString() As String
        Return EmailSubject
    End Function

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub
End Class

Public Class _EmailMboxPackageItems
    Public CustomerName As String
    Public SMS As String
    Public smsCarrier As String
    Public EmailTo As String
    Public HTMLBody As String
    Public PackageItems As New List(Of _EmailMboxPackageItem)

    Public Overrides Function ToString() As String
        Return EmailTo
    End Function
    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub
End Class
Public Class _EmailMboxPackageItem
    Public Carrier As String
    Public TrackingNo As String
    Public Notes As String
    Public Location As String
    Public Sub New(ByVal pkg As MailboxPackageObjectObservable, ByVal check As PackageValet.CheckInOut)
        Carrier = pkg.CarrierName
        TrackingNo = pkg.TrackingNo
        Select Case check
            Case PackageValet.CheckInOut.CheckIn
                Notes = pkg.CheckInNotes
            Case PackageValet.CheckInOut.CheckOut
                Notes = pkg.CheckOutNotes
        End Select
        Location = pkg.Location
    End Sub

    Public Overrides Function ToString() As String
        Return TrackingNo
    End Function
    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub

End Class

