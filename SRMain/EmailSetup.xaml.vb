Imports System.Data
Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports System.Windows.Forms

Public Class EmailSetup
    Inherits CommonWindow

    Dim TemplatePosition As Integer = 0
    Dim EmailTemplates(6) As EmailTemplate
    Dim EmailContentUnsaved As Boolean = False

#Region "Setup"
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
    Private Sub EmailSetup_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        SMTP_Help_Border.Visibility = Visibility.Hidden
        Load_Notification_ComboBox()
        Load_CurrentSettings()
    End Sub
    Private Sub Load_Notification_ComboBox()
        ' Build Email array
        Dim Types() As String = {   ' To add the availability of a new template type, add it here. Don't forget to increase size of array EmailTemplates.
            "Email - New Mail in Mailbox",
            "SMS - New Mail in Mailbox",
            "Email - New Package in Mailbox",
            "SMS - New Package in Mailbox",
            "Email - POS Receipt",
            "Email - AR Statement",
            "Email - Mailbox Statement"
        }
        ' These are the internal identifiers
        For i = 0 To Types.GetUpperBound(0)
            ' For each in Types
            EmailTemplates(i) = New EmailTemplate
            EmailTemplates(i).Type = Types(i)
            ' Pull from Policy
            EmailTemplates(i).Subject = GetPolicyData(gShipriteDB, "Notify_" & Regex.Replace(Types(i), "[ ]", String.Empty) & "Subject", "")
            EmailTemplates(i).Content = GetPolicyData(gShipriteDB, "Notify_" & Regex.Replace(Types(i), "[ ]", String.Empty) & "Content", "")
            NotificationType_ComboBox.Items.Add(Types(i))
        Next
        DisableContentSave()
    End Sub
    Private Sub Load_CurrentSettings()
        ' load policy into cache
        My.Settings.Notify_Email = _Convert.Base64ToString(GetPolicyData(gShipriteDB, "Notify_Email"))
        My.Settings.Notify_Password = _Convert.Base64ToString(GetPolicyData(gShipriteDB, "Notify_Password"))
        My.Settings.Notify_SendCopy = Convert.ToBoolean(GetPolicyData(gShipriteDB, "Notify_SendCopy", "False"))
        My.Settings.Notify_KeepLog = Convert.ToBoolean(GetPolicyData(gShipriteDB, "Notify_KeepLog", "False"))
        My.Settings.Notify_SmtpServer = _Convert.Base64ToString(GetPolicyData(gShipriteDB, "Notify_SmtpServer"))
        My.Settings.Notify_SmtpPort = _Convert.Base64ToString(GetPolicyData(gShipriteDB, "Notify_SmtpPort", _Convert.StringToBase64("587")))
        My.Settings.Notify_SmtpEncrypted = Convert.ToBoolean(GetPolicyData(gShipriteDB, "Notify_SmtpEncrypted", "False"))
        ' load from cache

        ' Account Credentials
        Email_TextBox.Text = My.Settings.Notify_Email
        If Not String.IsNullOrEmpty(My.Settings.Notify_Password) Then
            Password_TextBox.Password = My.Settings.Notify_Password
        End If
        Check_KeepLog.IsChecked = My.Settings.Notify_KeepLog
        Check_SendCopy.IsChecked = My.Settings.Notify_SendCopy
        ' Outgoing SMTP Settings
        SmtpServer.Text = My.Settings.Notify_SmtpServer
        If My.Settings.Notify_SmtpPort Is Nothing Then
            SmtpPort.Text = "587"
        Else
            SmtpPort.Text = My.Settings.Notify_SmtpPort
        End If
        SmtpEncrypted.IsChecked = My.Settings.Notify_SmtpEncrypted
    End Sub
#End Region

#Region "Event Handlers"
    Private Sub Help_Button_Click(sender As Object, e As RoutedEventArgs) Handles Help_Button.Click
        If SMTP_Help_Border.Visibility = Visibility.Hidden Then
            SMTP_Help_Border.Visibility = Visibility.Visible
        Else
            SMTP_Help_Border.Visibility = Visibility.Hidden
        End If
    End Sub
    Private Sub EmailChanged(sender As Object, e As RoutedEventArgs) Handles Email_TextBox.TextChanged
        My.Settings.Notify_Email = Email_TextBox.Text
        'If Email_TextBox.Text = "SuperSecret" Then
        '    Console.WriteLine("Resetting Email Settings...")
        '    My.Settings.Notify_Email = Nothing
        '    My.Settings.Notify_Password = Nothing
        '    My.Settings.Notify_SmtpServer = Nothing
        '    My.Settings.Notify_SmtpPort = Nothing
        '    My.Settings.Notify_SmtpEncrypted = Nothing
        '    My.Settings.Notify_KeepLog = Nothing
        '    My.Settings.Notify_SendCopy = Nothing
        '    My.Settings.Save()
        'Else
        '    Console.WriteLine(Email_TextBox.Text)
        'End If
    End Sub
    Private Sub PasswordChanged(sender As Object, e As RoutedEventArgs) Handles Password_TextBox.PasswordChanged
        My.Settings.Notify_Password = Password_TextBox.Password
    End Sub
    Private Sub SmtpServerChanged(sender As Object, e As RoutedEventArgs) Handles SmtpServer.TextChanged
        My.Settings.Notify_SmtpServer = SmtpServer.Text
    End Sub
    Private Sub SmtpPortChanged(sender As Object, e As RoutedEventArgs) Handles SmtpPort.TextChanged
        If IsNumeric(SmtpPort.Text) Or SmtpPort.Text = "" Then
            My.Settings.Notify_SmtpPort = SmtpPort.Text
        Else
            SmtpPort.Text = ""
            System.Windows.MessageBox.Show("Please enter a valid TCP Port.", "Email Setup", MessageBoxButton.OK, MessageBoxImage.Exclamation)
        End If
    End Sub
    Private Sub SendCopyChanged(sender As Object, e As RoutedEventArgs) Handles Check_SendCopy.Click
        My.Settings.Notify_SendCopy = Check_SendCopy.IsChecked
    End Sub
    Private Sub KeepLogChanged(sender As Object, e As RoutedEventArgs) Handles Check_KeepLog.Click
        My.Settings.Notify_KeepLog = Check_KeepLog.IsChecked
    End Sub
    Private Sub EncryptionChanged(sender As Object, e As RoutedEventArgs) Handles SmtpEncrypted.Click
        My.Settings.Notify_SmtpEncrypted = SmtpEncrypted.IsChecked
    End Sub
    Private Sub SaveButton_Click(sender As Object, e As RoutedEventArgs) Handles SaveButton.Click
        If EmailContentUnsaved Then
            Dim result As DialogResult = MessageBox.Show("You have unsaved changes in the email template, would you like to save them?", "Email Setup", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question)
            Select Case result
                Case System.Windows.Forms.DialogResult.Yes
                    ' Save before changing ttemplate type
                    SaveEmail(sender, e)
                    MasterSave()
                Case System.Windows.Forms.DialogResult.No
                    MasterSave()
                Case System.Windows.Forms.DialogResult.Cancel
                    ' Cancel changing the template type
            End Select
        Else
            'SaveTemplates() ' This circumvented the EmailContentUnsaved feature
            MasterSave()
        End If
    End Sub
    Private Sub TypeChanged(sender As Object, e As RoutedEventArgs) Handles NotificationType_ComboBox.SelectionChanged
        If EmailContentUnsaved And Not NotificationType_ComboBox.Text = "Select Email Notification" Then
            ' Alert user asking if they'd like to save the changed email content
            Dim result As DialogResult = MessageBox.Show("Would you like to save the email template?", "Email Setup", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question)
            Select Case result
                Case System.Windows.Forms.DialogResult.Yes
                    ' Save before changing ttemplate type
                    SaveEmail(sender, e)
                    changeTemplate()
                Case System.Windows.Forms.DialogResult.No
                    ' Change template type without saving the current email
                    changeTemplate()
                Case System.Windows.Forms.DialogResult.Cancel
                    ' Cancel changing the template type
                    NotificationType_ComboBox.Text = EmailTemplates(TemplatePosition).Type ' Set selection back to what it was
            End Select
        Else
            changeTemplate()
        End If
    End Sub
    Private Sub SubjectChanged(sender As Object, e As RoutedEventArgs) Handles Subject.TextChanged
        If Not IsNothing(EmailTemplates) AndAlso Not IsNothing(EmailTemplates(TemplatePosition)) Then
            Dim currentSubject As String = GetPolicyData(gShipriteDB, "Notify_" & Regex.Replace(EmailTemplates(TemplatePosition).Type, "[ ]", String.Empty) & "Subject", Nothing)
            Dim newSubject As String = Subject.Text
            If String.IsNullOrEmpty(currentSubject) Then
                ' Original is blank
                If Not String.IsNullOrEmpty(newSubject) Then
                    EnableContentSave()
                End If
            Else
                If currentSubject <> newSubject Then
                    EnableContentSave()
                End If
            End If
        End If
    End Sub
    Private Sub ContentChanged(sender As Object, e As RoutedEventArgs) Handles EmailContent.TextChanged
        If Not IsNothing(EmailTemplates) AndAlso Not IsNothing(EmailTemplates(TemplatePosition)) Then
            Dim currentContent As String = GetPolicyData(gShipriteDB, "Notify_" & Regex.Replace(EmailTemplates(TemplatePosition).Type, "[ ]", String.Empty) & "Content", Nothing)
            If String.IsNullOrEmpty(currentContent) Then
                ' Original is blank
                Dim CurrentPlainContent As String = String.Empty
                Dim textRange As New TextRange(EmailContent.Document.ContentStart, EmailContent.Document.ContentEnd)
                CurrentPlainContent = textRange.Text
                If Not String.IsNullOrEmpty(CurrentPlainContent) Then
                    ' New content!
                    EnableContentSave()
                End If
            Else
                Dim DBRTF64 As String = currentContent
                Dim URRTF64 As String = RichBoxToString(EmailContent)
                Debug.Print("Compare RTF Data")
                If DBRTF64 <> URRTF64 Then
                    EnableContentSave()
                End If
            End If
        End If
    End Sub
    Private Sub EnableContentSave()
        EmailContentUnsaved = True
        If EmailSave IsNot Nothing Then
            EmailSave.Opacity = 1.0
        End If
    End Sub
    Private Sub DisableContentSave()
        EmailContentUnsaved = False
        If EmailSave IsNot Nothing Then
            EmailSave.Opacity = 0.25
        End If
    End Sub
    Private Sub SaveEmail(sender As Object, e As RoutedEventArgs) Handles EmailSave.Click
        If EmailContentUnsaved Then
            ' Save email subject
            EmailTemplates(TemplatePosition).Subject = Subject.Text
            ' Save email content
            If EmailTemplates(TemplatePosition).Type.StartsWith("SMS") Then
                Dim textRange As New TextRange(EmailContent.Document.ContentStart, EmailContent.Document.ContentEnd)
                EmailTemplates(TemplatePosition).Content = _Convert.StringToBase64(textRange.Text)
            Else
                EmailTemplates(TemplatePosition).Content = RichBoxToString(EmailContent)
            End If
            SaveTemplates()
            DisableContentSave()
        End If
    End Sub
    Public Shared Function RichBoxToString(box As Controls.RichTextBox) As String
        Dim tempFile As String = My.Computer.FileSystem.GetTempFileName & Guid.NewGuid().ToString
        '  Save email content to temporary file
        Dim tr As TextRange = New TextRange(box.Document.ContentStart, box.Document.ContentEnd)
        Dim stream As FileStream = New FileStream(tempFile, FileMode.Create)
        tr.Save(stream, DataFormats.Rtf)
        stream.Close()
        '  Read temporary file to object
        Dim content As String = File.ReadAllText(tempFile)
        content = _Convert.StringToBase64(content)
        '  Delete the temporary file
        My.Computer.FileSystem.DeleteFile(tempFile)
        Return content
    End Function
    Private Sub Test_Button_Click(sender As Object, e As RoutedEventArgs) Handles Test_Button.Click
        ' Send test email to self
        If ApiRequest.sendTestEmail(Email_TextBox.Text, Password_TextBox.Password, SmtpServer.Text, SmtpPort.Text, SmtpEncrypted.IsChecked) Then
            ' Success
            MessageBox.Show("Email was sent successfully", "Email Setup", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Else
            ' Failure
            MessageBox.Show("Email has failed to send", "Email Setup", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End If
    End Sub
#End Region

#Region "Supporting Functions"
    Private Sub changeTemplate()
        Dim selectedTemplate As EmailTemplate = Nothing
        ' Find Email Template
        If EmailTemplates IsNot Nothing Then
            For x = 0 To EmailTemplates.GetUpperBound(0)
                If EmailTemplates(x) IsNot Nothing AndAlso EmailTemplates(x).Type = NotificationType_ComboBox.SelectedItem.ToString Then
                    ' Found the Template!
                    selectedTemplate = EmailTemplates(x)
                    TemplatePosition = x
                End If
            Next
            If selectedTemplate IsNot Nothing Then
                ' Load Template
                Subject.Text = selectedTemplate.Subject
                ' Prepare new email content
                If Not String.IsNullOrEmpty(EmailTemplates(TemplatePosition).Content) Then
                    ' There is content to be had
                    '  Create a temporary file
                    Dim tempFile As String = My.Computer.FileSystem.GetTempFileName & Guid.NewGuid().ToString
                    '  Put content of object into temporary file
                    Dim writer As New System.IO.StreamWriter(tempFile)
                    Dim content As String = _Convert.Base64ToString(EmailTemplates(TemplatePosition).Content)
                    writer.Write(content)
                    writer.Close()
                    '  Load temporary file to RichTextBox
                    Dim tr As TextRange = New TextRange(EmailContent.Document.ContentStart, EmailContent.Document.ContentEnd)
                    Dim stream As FileStream = New FileStream(tempFile, FileMode.Open)
                    If EmailTemplates(TemplatePosition).Type.StartsWith("SMS") Then
                        tr.Text = content
                    Else
                        tr.Load(stream, DataFormats.Rtf)
                    End If
                    stream.Close()
                    '  Delete the temporary file
                    My.Computer.FileSystem.DeleteFile(tempFile)
                Else
                    EmailContent.Document.Blocks.Clear()
                End If
                ' Reset save button
                DisableContentSave()
            End If
        End If
    End Sub
    Private Sub MasterSave()
        ' Email Settings
        UpdatePolicy(gShipriteDB, "Notify_Email", _Convert.StringToBase64(My.Settings.Notify_Email))
        UpdatePolicy(gShipriteDB, "Notify_Password", _Convert.StringToBase64(My.Settings.Notify_Password)) ' TODO: Replace this with some form of encryption)
        UpdatePolicy(gShipriteDB, "Notify_SendCopy", Convert.ToString(My.Settings.Notify_SendCopy))
        UpdatePolicy(gShipriteDB, "Notify_KeepLog", Convert.ToString(My.Settings.Notify_KeepLog))
        UpdatePolicy(gShipriteDB, "Notify_SmtpServer", _Convert.StringToBase64(My.Settings.Notify_SmtpServer))
        UpdatePolicy(gShipriteDB, "Notify_SmtpPort", _Convert.StringToBase64(My.Settings.Notify_SmtpPort))
        UpdatePolicy(gShipriteDB, "Notify_SmtpEncrypted", Convert.ToString(My.Settings.Notify_SmtpEncrypted))
        ' Popup confirming save
        _MsgBox.SavedSuccessfully("Email Settings")
    End Sub
    Private Sub SaveTemplates()
        ' Email Templates
        For z = 0 To EmailTemplates.GetUpperBound(0)
            UpdatePolicy(gShipriteDB, "Notify_" & Regex.Replace(EmailTemplates(z).Type, "[ ]", String.Empty) & "Subject", _Controls.Replace(EmailTemplates(z).Subject, "'", "''"))
            UpdatePolicy(gShipriteDB, "Notify_" & Regex.Replace(EmailTemplates(z).Type, "[ ]", String.Empty) & "Content", EmailTemplates(z).Content)
        Next
    End Sub

    Private Sub ShowPassword_Btn_Click(sender As Object, e As RoutedEventArgs) Handles ShowPassword_Btn.Click
        MsgBox(Password_TextBox.Password)
    End Sub



#End Region
End Class


