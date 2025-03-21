Public Class MailboxNotifications
    Inherits CommonWindow

    Public Names_List As List(Of AdditionalName_Item)
    Public NoticeLog As List(Of Notice_Log_Item)
    Public isLoading As Boolean
    Public Class Notice_Log_Item
        Public Property MBX_No As Integer
        Public Property Name As String
        Public Property Email As String
        Public Property SMS As String
        Public Property ErrorMessage As String
    End Class

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

    Private Sub MailboxNotices_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        isLoading = True
        EmailNotifcation_ChkBx.IsChecked = My.Settings.MBXDailyNotice_chkEmail
        SMSNotifcation_ChkBx.IsChecked = My.Settings.MBXDailyNotice_chkSMS

        If EmailNotifcation_ChkBx.IsChecked = False And SMSNotifcation_ChkBx.IsChecked = False Then
            EmailNotifcation_ChkBx.IsChecked = True
        End If

        isLoading = False

        NoticeLog = New List(Of Notice_Log_Item)
        DailyNotice_MbxNo.Focus()
    End Sub



    Private Sub DailyNotice_MbxNo_LostFocus(sender As Object, e As RoutedEventArgs) Handles DailyNotice_MbxNo.LostFocus
        If DailyNotice_MbxNo.Text = "" Then Exit Sub

        Get_MailboxExpirationDate()
        DailyNotice_Load_MailboxNames()
    End Sub

    Private Sub DailyNotice_Load_MailboxNames()
        Try
            Dim current_segment As String
            Dim AddName As AdditionalName_Item
            Dim MBX_No As String = DailyNotice_MbxNo.Text
            Dim buf As String

            buf = IO_GetSegmentSet(gShipriteDB, "SELECT MbxNamesList.CID, MBXNamesList.Name, Contacts.Email, Contacts.CellPhone, Contacts.CellCarrier 
FROM MbxNamesList LEFT JOIN Contacts On MbxNamesList.CID=Contacts.ID 
WHERE MbxNamesList.MBX=" & MBX_No)


            If buf = "" Then
                DailyNotice_MBXNames_LV.Items.Refresh()
                Exit Sub
            End If

            Names_List = New List(Of AdditionalName_Item)

            Do Until buf = ""
                AddName = New AdditionalName_Item
                current_segment = GetNextSegmentFromSet(buf)

                AddName.DisplayName = ExtractElementFromSegment("Name", current_segment, "")
                AddName.CID = ExtractElementFromSegment("CID", current_segment, "")
                AddName.Email = ExtractElementFromSegment("Email", current_segment, "")
                AddName.CellPhone = ExtractElementFromSegment("CellPhone", current_segment, "")
                AddName.CellCarrier = ExtractElementFromSegment("CellCarrier", current_segment, "")

                If Names_List.Count = 0 Then AddName.isSelected = True

                Names_List.Add(AddName)


            Loop

            DailyNotice_MBXNames_LV.ItemsSource = Names_List
            DailyNotice_MBXNames_LV.Items.Refresh()


        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Loading Mailbox Names")
        End Try
    End Sub

    Private Sub Get_MailboxExpirationDate()
        Try
            DailyNotice_ExpDate.Foreground = Media.Brushes.Black

            Dim SQL As String = "Select [EndDate] From Mailbox Where [MailboxNumber] = " & DailyNotice_MbxNo.Text
            Dim Segment As String
            Dim ExpDate As Date

            Segment = IO_GetSegmentSet(gShipriteDB, SQL)
            If Segment = "" Then
                DailyNotice_ExpDate.Text = "Box not rented!"
                DailyNotice_ExpDate.Foreground = Media.Brushes.Red
                Exit Sub
            End If

            ExpDate = ExtractElementFromSegment("EndDate", Segment, "1/1/1900")
            If ExpDate < Today Then
                DailyNotice_ExpDate.Text = "Expired on " & ExpDate.ToString("d")
                DailyNotice_ExpDate.Foreground = Media.Brushes.Red
            Else
                DailyNotice_ExpDate.Text = ExpDate.ToString("d")
            End If

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error obtaining Mailbox Expiration Date.")
        End Try
    End Sub

    Private Sub DailyNotice_ClearForm()
        Try
            DailyNotice_MbxNo.Text = ""
            DailyNotice_ExpDate.Text = ""
            DailyNotice_ExpDate.Foreground = Media.Brushes.Black

            DailyNotice_MBXNames_LV.ItemsSource = Nothing
            DailyNotice_MBXNames_LV.Items.Refresh()

            DailyNotice_MbxNo.Focus()

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error clearing Daily Notice Form.")
        End Try
    End Sub

    Private Sub DailyNotice_Clear_Btn_Click(sender As Object, e As RoutedEventArgs) Handles DailyNotice_Clear_Btn.Click
        DailyNotice_ClearForm()
    End Sub

    Private Sub DailyNotice_Send_Btn_Click(sender As Object, e As RoutedEventArgs) Handles DailyNotice_Send_Btn.Click

        DailyNotice_SEND_Notice()

    End Sub

    Private Sub DailyNotice_MbxNo_KeyDown(sender As Object, e As KeyEventArgs) Handles DailyNotice_MbxNo.KeyDown
        If e.Key = Key.Return Then
            DailyNotice_Load_MailboxNames()
            DailyNotice_SEND_Notice()
            DailyNotice_ClearForm()
        End If
    End Sub

    Private Sub DailyNotice_SEND_Notice()
        Try
            Dim success As Boolean
            Dim LogItem As Notice_Log_Item
            Dim apiParams As New Dictionary(Of String, String)

            If Not EmailNotifcation_ChkBx.IsChecked And Not SMSNotifcation_ChkBx.IsChecked Then
                MsgBox("No notification option selected. Please check EMAIL or SMS optoins to send notification!", vbExclamation)
                Exit Sub
            End If

            If DailyNotice_MbxNo.Text = "" Or IsNothing(Names_List) Then
                Exit Sub
            End If

            For Each item As AdditionalName_Item In Names_List

                If item.isSelected And (item.Email <> "" Or item.CellPhone <> "") Then

                    LogItem = New Notice_Log_Item
                    LogItem.MBX_No = DailyNotice_MbxNo.Text
                    LogItem.Name = item.DisplayName

                    'Email
                    If EmailNotifcation_ChkBx.IsChecked And item.Email <> "" Then
                        Dim template_Email As EmailTemplate = getEmailTemplate("Notify_Email-NewMailinMailbox", item.DisplayName)
                        success = sendEmail(item.Email, template_Email)


                        If success = True Then
                            LogItem.Email = "Sent"
                        Else
                            LogItem.Email = "Fail"
                            LogItem.ErrorMessage = ""
                        End If

                    End If

                    'SMS
                    If SMSNotifcation_ChkBx.IsChecked And item.CellPhone <> "" Then
                        Dim template_SMS As EmailTemplate = getEmailTemplate("Notify_SMS-NewMailInMailbox", item.DisplayName)

                        apiParams = New Dictionary(Of String, String)
                        apiParams.Add("key", ApiRequest.apiKey)
                        apiParams.Add("type", "custom")
                        apiParams.Add("phone", item.CellPhone)
                        apiParams.Add("carrier", item.CellCarrier)
                        apiParams.Add("content", _Convert.StringToBase64(template_SMS.Content))
                        Dim apiResponse As Object = Newtonsoft.Json.Linq.JObject.Parse(ApiRequest.liminal("sms", apiParams))
                        If Not apiResponse("status") Then
                            Debug.Print(apiResponse("reason"))
                            'failed
                            LogItem.SMS = "Fail"
                            LogItem.ErrorMessage = apiResponse("reason")
                        Else
                            'success
                            LogItem.SMS = "Sent"
                        End If
                    End If

                    If LogItem.SMS <> Nothing Or LogItem.Email <> Nothing Then
                        NoticeLog.Add(LogItem)
                    End If


                End If
            Next

            NotificationLog_LV.ItemsSource = NoticeLog
            NotificationLog_LV.Items.Refresh()

            DailyNotice_MbxNo.Text = ""
            DailyNotice_MbxNo.Focus()


        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error sending Daily Notice.")
        End Try

    End Sub

    Private Sub Notifcation_ChkBx_Checked(sender As Object, e As RoutedEventArgs) Handles EmailNotifcation_ChkBx.Checked, SMSNotifcation_ChkBx.Checked, EmailNotifcation_ChkBx.Unchecked, SMSNotifcation_ChkBx.Unchecked

        If isLoading Then Exit Sub

        My.Settings.MBXDailyNotice_chkEmail = EmailNotifcation_ChkBx.IsChecked
        My.Settings.MBXDailyNotice_chkSMS = SMSNotifcation_ChkBx.IsChecked
    End Sub
End Class
