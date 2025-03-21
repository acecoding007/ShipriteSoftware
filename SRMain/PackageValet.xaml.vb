Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Data
Imports System.Linq
Imports SHIPRITE.ReportPreview
Imports SHIPRITE.ShipRiteReports
Imports Newtonsoft.Json.Linq

Public Class PackageValet
    Inherits CommonWindow

    Dim isAdult As Boolean
    Dim retToDriver As Boolean
    Dim customerRefused As Boolean
    Dim Last_Column_Sorted As String
    Dim Last_Sort_Ascending As Boolean


    Public Property MailBoxInfo As ObservableCollection(Of MailboxPackageObjectObservable)
    Public Property PackageCheckOutInfo As ObservableCollection(Of MailboxPackageObjectObservable)
    Public Property PackageInventoryInfo As ObservableCollection(Of MailboxPackageObjectObservable)

    Private Property emailStatusFailed As New StatusLabel With {.text = "email - Failed", .FColor = Media.Brushes.Red}
    Private Property emailStatusSuccess As New StatusLabel With {.text = "email - Success", .FColor = Media.Brushes.Green}
    Private Property smsStatusFailed As New StatusLabel With {.text = "sms - Failed", .FColor = Media.Brushes.Red}
    Private Property smsStatusSuccess As New StatusLabel With {.text = "sms - Success", .FColor = Media.Brushes.Green}
    Private Property statusInvisible As New StatusLabel With {.text = "", .FColor = Media.Brushes.Black}


    Private _emailStatus As StatusLabel = statusInvisible
    Public Property emailStatus As StatusLabel
        Get
            Return _emailStatus
        End Get
        Set(value As StatusLabel)
            _emailStatus = value
            NotifyPropertyChanged()
        End Set
    End Property
    Private _smsStatus As StatusLabel = statusInvisible
    Public Property smsStatus As StatusLabel
        Get
            Return _smsStatus
        End Get
        Set(value As StatusLabel)
            _smsStatus = value
            NotifyPropertyChanged()
        End Set
    End Property

    Enum NoticePrinter As Byte
        Both = 0
        Receipt = 1
        Label = 2
    End Enum

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

    Private Sub PackageValet_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Me.DataContext = Me

        MailBoxInfo = New ObservableCollection(Of MailboxPackageObjectObservable)
        PackageCheckOutInfo = New ObservableCollection(Of MailboxPackageObjectObservable)
        PackageInventoryInfo = New ObservableCollection(Of MailboxPackageObjectObservable)

        dtpFrom.SelectedDate = Date.Today
        dtpTo.SelectedDate = Date.Today
        cmbPackageClass.SelectedIndex = 0

        'makes tab headers not visible in run time. 
        For Each currentTab As TabItem In PackageValet_TabControl.Items
            currentTab.Visibility = Visibility.Collapsed
        Next

        PackageValet_TabControl.Visibility = Visibility.Hidden

        'Selection_ListBox.SelectedIndex = 0
        Load_Carrier_Options()

        chkPrintNotices.IsChecked = My.Settings.PackageValet_CheckIn_PrintNotice
        chkPackageLabel.IsChecked = My.Settings.PackageValet_CheckIn_PrintLabel

        chkSignatureSheet.IsChecked = My.Settings.PackageValet_CheckIn_PrintSignatureSheet
        chkSendSMS.IsChecked = My.Settings.PackageValet_CheckIn_SendSMS
        chkSendEmails.IsChecked = My.Settings.PackageValet_CheckIn_SendEmail

        Select Case My.Settings.PackageValet_CheckIn_NoticePrintOption
            Case NoticePrinter.Both
                PrintNoticeBoth.IsChecked = True
            Case NoticePrinter.Receipt
                PrintNoticeReceipt.IsChecked = True
            Case NoticePrinter.Label
                PrintNoticeLabel.IsChecked = True
        End Select

    End Sub

    Private Sub HandleExit(sender As Object, e As RoutedEventArgs)
        If lvPackages.Items.Count > 0 Then
            If vbNo = MsgBox("You have checked in packages that have not been processed and saved yet. Do you stil wish to exit?", vbYesNo + vbExclamation, "Check In - Unprocessed Packages") Then
                Selection_ListBox.SelectedIndex = 0
                Exit Sub
            End If
        End If

        Select Case sender.Name
            Case "HomeButton"
                HomeButton_Click(sender, e)
            Case "BackButton"
                BackButton_Click(sender, e)
            Case "CloseButton"
                CloseButton_Click(sender, e)
            Case "ForwardButton"
                ForwardButton_Click(sender, e)
        End Select

    End Sub

    Private Sub Selection_ListBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles Selection_ListBox.SelectionChanged
        Try
            PackageValet_TabControl.Visibility = Visibility.Visible
            PackageValet_TabControl.SelectedIndex = Selection_ListBox.SelectedIndex

            If Selection_ListBox.SelectedIndex = 0 Then
                Header_Label.Content = "PACKAGE VALET - CHECK IN"
                txtMailboxNo.Focus()
            ElseIf Selection_ListBox.SelectedIndex = 1 Then
                Header_Label.Content = "PACKAGE VALET - CHECK OUT"
                CheckOut_TabItem_Loaded()
            Else
                Header_Label.Content = "PACKAGE VALET - INVENTORY"
                PackageInventory_TabItem_Loaded()
            End If

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error.")

        End Try
    End Sub

    Private Sub Load_Carrier_Options()

        Try

            Dim SegmentSet As String = ""
            Dim current_segment As String = ""
            Dim buf As String = ""
            Dim fieldName As String = ""
            Dim fieldValue As String = ""
            Dim CarrierList As List(Of CarrierIcon) = New List(Of CarrierIcon)
            Dim current_Carrier As CarrierIcon
            Dim index As Integer = 0

            buf = IO_GetSegmentSet(gShipriteDB, "SELECT DISTINCT Carrier from Master")

            Do Until buf = ""
                current_segment = GetNextSegmentFromSet(buf)
                current_segment = ExtractNextElementFromSegment(fieldName, fieldValue, current_segment)

                If fieldValue Is Nothing Then fieldValue = ""
                current_Carrier = New CarrierIcon

                current_Carrier.Index = index
                current_Carrier.CarrierName = fieldValue
                current_Carrier.CarrierImage = "Resources/" & fieldValue & "_Logo.png"

                CarrierList.Add(current_Carrier)

                index += 1
            Loop

            current_Carrier = New CarrierIcon
            current_Carrier.Index = index
            current_Carrier.CarrierName = "Amazon"
            current_Carrier.CarrierImage = "Resources/Amazon_Logo.png"
            CarrierList.Add(current_Carrier)

            index += 1

            current_Carrier = New CarrierIcon
            current_Carrier.Index = index
            current_Carrier.CarrierName = "Other"
            current_Carrier.CarrierImage = "Resources/Other_Logo.png"
            CarrierList.Add(current_Carrier)

            Carrier_ListBox.ItemsSource = CarrierList
            Carrier_ListBox.Items.Refresh()

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error.")
        End Try

    End Sub


#Region "Customer and Mailbox Lookup"

    Private Sub TxtMailboxNo_KeyDown(sender As Object, e As KeyEventArgs) Handles txtMailboxNo.KeyDown, txtMailboxNo1.KeyDown
        Try
            If e.Key = Key.Return Or e.Key = Key.Tab Then

                If sender.Text.Length = 0 Then
                    sender.Focus()
                Else
                    If "0" = sender.Text Then
                        Me.cmbCustomerName.Focus()
                    Else
                        If Selection_ListBox.SelectedIndex = 0 Then
                            'checkIn
                            load_Mailbox(txtMailboxNo, txtExpDate, txtMailboxName)
                        Else
                            'checkout
                            load_Mailbox(txtMailboxNo1, txtExpDate1, txtMailboxName1)
                        End If
                    End If
                End If
            End If
        Catch ex As Exception
        End Try
    End Sub

    Private Sub txtMailboxNo_LostFocus(sender As Object, e As RoutedEventArgs) Handles txtMailboxNo.LostFocus
        Try
            If 0 = Me.txtMailboxNo.Text.Length Then
                Me.txtMailboxNo.Text = "0"
            End If
            'Call select_Mailbox()
            'Call chkHoldForPickup_VisibleOnOff()
        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Failed to load Mailbox name...")
        End Try
    End Sub

    Private Function DisplayCustomerSegment() As Long
        Try
            Dim FName As String
            Dim buf As String

            FName = ""

            Dim customer As New _baseContact

            FName = txtMailboxName.Uid
            buf = ExtractElementFromSegment(FName, gContactManagerSegment)

            If Not buf = "" Then
                txtMailboxName.Text = buf
            End If

            buf = ExtractElementFromSegment("ID", gContactManagerSegment)

            If Not buf = "" Then
                customer.UniqueID = buf
            End If

            'add info from segment to customer variable
            customer.CompanyName = txtMailboxName.Text
            'add info from segment to customer variable

            txtMailboxName.Tag = customer


            Return 0

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Displaying Customer.")
            Return -1
        End Try
    End Function

    Private Sub txtMailboxName_KeyDown(sender As Object, e As KeyEventArgs) Handles txtMailboxName.KeyDown

        Try
            If e.Key = Key.Return Then
                OpenContactManager(txtMailboxName, txtExpDate, txtMailboxNo)
            End If
        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Failed to load Address Entry form...")
            Debug.Print(ex.StackTrace)
        End Try
    End Sub

    Private Function OpenContactManager(ByRef nameBox As ComboBox, ByRef expDate As Label, ByRef numBox As TextBox) As Long
        Try
            gAutoExitFromContacts = True

            gContactManagerSegment = ""
            Dim win As New ContactManager(Me,, nameBox.Text)
            win.ShowDialog(Me)

            If Not String.IsNullOrEmpty(gContactManagerSegment) Then
                nameBox.Text = ExtractElementFromSegment("Name", gContactManagerSegment)
                Dim CID = ExtractElementFromSegment("ID", gContactManagerSegment)
                Dim SQL As String = "SELECT MBX from MBXNamesList WHERE CID = " & CID
                Dim mboxSegment As String = GetNextSegmentFromSet(IO_GetSegmentSet(gShipriteDB, SQL))
                If SegmentFunctions.IsElementInSegment("MBX", mboxSegment) Then
                    numBox.Text = ExtractElementFromSegment("MBX", mboxSegment)
                Else
                    numBox.Text = "0"
                End If

                Dim customer As New _baseContact
                customer.CompanyName = nameBox.Text

                load_Mailbox(numBox, expDate, nameBox)

                txtPackageTrackingNo.Focus()
            End If

            Return DisplayCustomerSegment()


        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Processing Package.")
            Return False
        End Try

    End Function

    Private Sub btn_ContactSearch_Click(sender As Object, e As RoutedEventArgs) Handles btn_ContactSearch.Click
        OpenContactManager(txtMailboxName, txtExpDate, txtMailboxNo)
    End Sub

#End Region


    Private Sub PrintButton_Click(sender As Object, e As RoutedEventArgs) Handles PrintButton.Click

        If Print_Popup.IsOpen = True Then
            Print_Popup.IsOpen = False
        Else
            Print_Popup.IsOpen = True
        End If

    End Sub

    Private Sub txtPackageTrackingNo_KeyDown(sender As Object, e As KeyEventArgs) Handles txtPackageTrackingNo.KeyDown
        Try
            If e.Key = Key.Return Then
                txtPackageTrackingNo.Text = StrConv(txtPackageTrackingNo.Text, vbUpperCase)
                addPackageIfPossible("Checkin Tracking Number")
            End If

        Catch ex As Exception
            Cursor = Cursors.Arrow
            _MsgBox.ErrorMessage(ex, "Failed to add Package to the list...")
        Finally : Cursor = Cursors.Arrow
        End Try
    End Sub

    Private Function add_Package() As Boolean
        Try
            add_Package = False ' assume.
            If 0 < Me.txtMailboxNo.Text.Length Then
                If 0 < Me.txtPackageTrackingNo.Text.Length Then
                    Me.txtPackageTrackingNo.Tag = Me.txtPackageTrackingNo.Text ' Barcode Scan as is.
                    ' check if this tracking# is already scanned:
                    ''ol#1.2.22(10/29)... If a carrier cannot be detected automatic then don't add it to the list, let user select the carrier.
                    ''ol#1.2.23(11/3)... We need to truncate FedEx tracking# (to last 12 digits) before looking for duplicates.
                    Dim isFound As Boolean = False ' assume.
                    Call guess_Carrier(Me.txtPackageTrackingNo.Text, isFound)
                    If Not isExist_ListItem(Me.txtPackageTrackingNo.Text) Then
                        ''ol#1.1.91(2/26)... Check for duplicate tracking# in database as well as in the check in list.
                        If Not isExist_DbItem(Me.txtPackageTrackingNo.Text) Then
                            If isFound Then
                                Dim package As MailboxPackageObject = create_PackageObject_FromScreen()
                                If Not package.CarrierName = "" Then
                                    If add_ListItem(package) Then
                                        add_Package = checkin_Package(package)
                                        If add_Package Then
                                            Call clear_Form(False) ''AP(11/10/2016){DRN = 1128} - Package CheckIn form clears after checking in package.
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    End If
                Else
                    _MsgBox.InformationMessage("Tracking number is required to add a package to the list!", "Tracking# Required!")
                End If
            Else
                _MsgBox.InformationMessage("Mailbox number is required to add a package to the list!", "Mailbox Required!")
            End If
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Private Sub clear_Form(Optional ByVal clearCustInfo As Boolean = False)
        ''AP(11/10/2016){DRN = 1128} - Package CheckIn form clears after checking in package.
        If clearCustInfo Then
            If Not Me.chkFedEx_HAL.IsChecked And Not Me.chkUPS_AP.IsChecked Then
                Me.txtMailboxNo.Text = String.Empty
                Me.txtMailboxNo.Tag = String.Empty
            End If
            Me.txtMailboxName.Text = String.Empty
            Me.txtMailboxName.Tag = Nothing
            Me.txtExpDate.Content = String.Empty
        End If
        Me.txtDesc.Text = String.Empty
        Me.txtLocation.Text = String.Empty
        Me.txtPackageTrackingNo.Text = String.Empty
        Me.txtPackageTrackingNo.Tag = String.Empty
        Me.txtPickedupBy.Text = String.Empty
        Me.txtCheckOutNotes.Text = String.Empty

        'Me.lblEmailSendStatus.Content = String.Empty
        'Me.lblSMSSendStatus.Text = String.Empty        
        If Me.txtMailboxNo.Text = "0" Then
            Me.txtMailboxName.Focus()
        Else
            Me.txtMailboxNo.Focus()
        End If
    End Sub

    Private Function create_PackageObject_FromScreen() As MailboxPackageObject
        Try
            create_PackageObject_FromScreen = New MailboxPackageObject
            With create_PackageObject_FromScreen
                .MailboxNo = Val(Me.txtMailboxNo.Text)
                .MailboxName = Me.txtMailboxName.Text
                .CarrierName = get_SelectedCarrier()
                .TrackingNo = Me.txtPackageTrackingNo.Text
                .ReceivedDate = Date.Now
                .PickedupBy = String.Empty
                .PickedupDate = Nothing
                ''ol#1.2.03(6/3)... Notes and Location fields are added to Print Notice body.
                .Notes = Me.txtDesc.Text
                .Location = Me.txtLocation.Text
                ''ol#1.2.41(7/11)... 'Hold for Pickup' will be integrated with Mailbox 'Package Check In/Out'.
                .BarCodeScan = Me.txtPackageTrackingNo.Tag

                Dim customer As New _baseContact
                Dim mbxSQL As String = ""
                If .MailboxNo <> 0 Then
                    mbxSQL = " AND mbx.MBX = " & .MailboxNo
                End If
                Dim SQL As String = "SELECT TOP 1 c.EMail, c.ID, c.CellPhone, c.CellCarrier " &
                                    "FROM Contacts AS c LEFT JOIN MBXNamesList AS mbx ON c.ID = mbx.CID " &
                                    "WHERE c.Name = '" & .MailboxName & "'" & mbxSQL
                Dim segment As String = IO_GetSegmentSet(gShipriteDB, SQL)
                If segment <> "" Then
                    .Email = SegmentFunctions.ExtractElementFromSegment("EMail", segment)
                    .CustomerID = Val(SegmentFunctions.ExtractElementFromSegment("ID", segment))
                    .SMS = SegmentFunctions.ExtractElementFromSegment("CellPhone", segment)
                    .CellCarrier = SegmentFunctions.ExtractElementFromSegment("CellCarrier", segment)
                End If

                If Me.chkFedEx_HAL.IsChecked Then
                    .PackageClass = _MailboxPackage.FEDEX_HAL
                ElseIf Me.chkUPS_AP.IsChecked Then
                    .PackageClass = _MailboxPackage.UPS_AP
                ElseIf .MailboxNo > 0 Then
                    .PackageClass = _MailboxPackage.MBOX
                Else
                    .PackageClass = _MailboxPackage.HOLD
                End If
            End With

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Creating Package Object.")
            Return Nothing
        End Try
    End Function

    Private Function get_SelectedCarrier() As String
        Try
            If Carrier_ListBox.SelectedItem IsNot Nothing Then
                Return CType(Carrier_ListBox.SelectedItem, CarrierIcon).CarrierName
            Else
                _MsgBox.InformationMessage("Select the Carrier Yourself!", "Carrier not selected")
            End If
        Catch ex As Exception
            Throw ex
        End Try
        Return ""

    End Function

    Private Function checkin_Package(ByVal package As MailboxPackageObject) As Boolean
        Try
            checkin_Package = False ' assume.

            If Me.chkPrintNotices.IsChecked Then
                Dim noticeSetting = My.Settings.PackageValet_CheckIn_NoticePrintOption
                'Insert printer code here
                If noticeSetting = NoticePrinter.Receipt OrElse noticeSetting = NoticePrinter.Both Then
                    Call _MailboxPackage.Print_PackageNotice(package)
                End If
                If noticeSetting = NoticePrinter.Label OrElse noticeSetting = NoticePrinter.Both Then
                    Call _MailboxPackage.Print_PackageNotice_Label(package)
                End If
            End If

            If Me.chkPackageLabel.IsChecked Then
                'Insert printer code here
                Call _MailboxPackage.Print_PackageLabel_v1(package)
            End If

            package = Nothing
            checkin_Package = True
        Catch ex As Exception
            checkin_Package = False
            _MsgBox.ErrorMessage(ex, "Error in printing..")
        End Try
    End Function

    Private Function add_ListItem(ByVal package As MailboxPackageObject) As Boolean
        Try
            add_ListItem = False ' assume.

            MailBoxInfo.Add(New MailboxPackageObjectObservable() With {
                            .MailboxNo = package.MailboxNo,
                            .MailboxName = package.MailboxName,
                            .SMS = package.SMS,
                            .smsCarrier = package.CellCarrier,
                            .CarrierName = package.CarrierName,
                            .TrackingNo = package.TrackingNo,
                            .CheckInNotes = package.Notes,
                            .Location = package.Location,
                            .IsGround = package.IsGround,
                            .PackageClass = package.PackageClass,
                            .BarCodeScan = package.BarCodeScan,
                            .CustomerID = package.CustomerID,
                            .ReceivedDate = package.ReceivedDate,
                            .Email = package.Email
                            })

            lvPackages.ItemsSource = MailBoxInfo
            lblPackageCount.Content = lvPackages.Items.Count
            Return True
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Private Function guess_Carrier(ByRef trackingno As String, ByRef isFound As Boolean) As String
        guess_Carrier = String.Empty ' assume.
        Try
            ''ol#1.2.30(1/5)... 'Auto Detect' check box in Carriers selection was added for visual carrier auto vs manual mode.
            ''  If Not userWantsToSelectCarrier Then
            If Me.chkAutoDetect.IsChecked Then
                guess_Carrier = BarCode.ShippingCo(trackingno)
                Call set_SelectedCarrier(guess_Carrier)

                If CType(Carrier_ListBox.SelectedItem, CarrierIcon).CarrierName.ToUpper() = "OTHER" Then
                    _MsgBox.InformationMessage("Select the Carrier Yourself!", "Cannot identify carrier!")
                    Carrier_ListBox.SelectedItem = -1
                    isFound = False
                Else
                    isFound = True
                End If

                chkAutoDetect.IsChecked = True
            Else
                ''ol#1.2.32(3/3)... If 'Auto Detect' is unchecked then don't do any automations, let user to do everything.
                '' ''ol#1.2.30(1/8)... Even if 'Auto Detect' is off FedEx long tracking# will be truncated to 12 digits.
                ''If "FedEx" = get_SelectedCarrier() Then
                ''    If 12 < trackingno.Length Then
                ''        trackingno = _Controls.Right(trackingno, 12)
                ''    End If
                ''End If
                isFound = True
            End If

        Catch ex As Exception : _Debug.Print_(ex.Message)
            _MsgBox.InformationMessage("Select the Carrier Yourself!", "Cannot identify carrier!")
            Carrier_ListBox.SelectedItem = -1
        End Try
    End Function

    Private Sub set_SelectedCarrier(ByVal carrier As String)

        Try
            Dim CarrierList As List(Of CarrierIcon) = New List(Of CarrierIcon)
            Dim CarrierFound As CarrierIcon

            If _Controls.Contains(carrier, "Ground") Then
                carrier = carrier.Remove((carrier.Length - " Ground".Length), " Ground".Length)
            ElseIf _Controls.Contains(carrier, "Express") Then
                carrier = carrier.Remove((carrier.Length - " Express".Length), " Express".Length)
            End If

            CarrierList = Carrier_ListBox.ItemsSource

            CarrierFound = CarrierList.Find(Function(p) p.CarrierName = carrier)

            If _Controls.Contains(carrier, "UPS") Then
                Carrier_ListBox.SelectedIndex = CarrierFound.Index
            ElseIf _Controls.Contains(carrier, "FedEx") Then
                Carrier_ListBox.SelectedIndex = CarrierFound.Index
            ElseIf _Controls.Contains(carrier, "USPS") Then
                Carrier_ListBox.SelectedIndex = CarrierFound.Index
            ElseIf _Controls.Contains(carrier, "DHL") Then
                Carrier_ListBox.SelectedIndex = CarrierFound.Index
            Else
                Carrier_ListBox.SelectedIndex = CarrierFound.Index
            End If

        Catch ex As Exception
            Throw ex
        End Try

    End Sub

    Private Function isExist_ListItem(ByVal trackingno As String) As Boolean
        Try
            isExist_ListItem = False

            Dim count As Integer = lvPackages.Items.Count
            Dim index As Integer = 0

            If count > 0 Then
                Do While index < count
                    If trackingno = CType(lvPackages.Items(index), MailboxPackageObjectObservable).TrackingNo Then
                        _MsgBox.WarningMessage("This Tracking# is already in the list! If you need to edit this entry then double-click on the list entry.", "Already Scanned!")
                        Return True
                    End If
                    index = index + 1
                Loop
            End If
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Private Function isExist_DbItem(ByVal trackingno As String) As Boolean
        Dim ret As Long
        isExist_DbItem = _MailboxPackagesDB.IsExist_TrackingNo(trackingno)
        If isExist_DbItem Then
            ''ol#1.2.21(10/21)... By scanning an already existed tracking# you will have an option to delete this package from database.            
            If MessageBoxResult.Yes = MsgBox("This Tracking# is already in the database! Do you want to Delete it?", vbYesNo + vbQuestion, "Delete?") Then
                ' delete package from database
                Dim sql2exe As String = "Delete * From Mailbox_Packages Where [TrackingNo]='" & trackingno & "'"

                ret = IO_UpdateSQLProcessor(gMailboxDB, sql2exe)

                If ret > 0 Then
                    _MsgBox.InformationMessage("Successfully!", "Deleted!")
                    Me.txtPackageTrackingNo.Text = String.Empty
                End If

            End If
        End If
    End Function

    Private Sub cmdProcess_Click(sender As Object, e As RoutedEventArgs) Handles cmdProcess.Click
        Try
            Cursor = Cursors.Wait
            If process_Packages() Then
                Call clear_Form(True)
                lvPackages.ItemsSource = Nothing
                MailBoxInfo.Clear()
            Else
                _MsgBox.InformationMessage("Some of the packages were not processed and not saved!" & Environment.NewLine & "Uncheck some processing options and try again...", "Failed to Process!")
            End If
        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to process currently selected Package...")
        Finally : Cursor = Cursors.Arrow
        End Try
    End Sub

    Private Function process_Packages() As Boolean
        Try
            emailStatus = statusInvisible
            smsStatus = statusInvisible

            ' Printing a signature sheet
            If Me.chkSignatureSheet.IsChecked Then
                Call _MailboxPackage.Print_SignatureSheet_Report(Me.lvPackages)
            End If

            ' Sending emails
            If Me.chkSendEmails.IsChecked AndAlso getEmailTemplate("Notify_Email-NewPackageinMailbox", "") IsNot Nothing Then
                sendEmails(lvPackages.Items)
            End If

            ' Sending texts (sms)
            If Me.chkSendSMS.IsChecked Then
                Dim msgs = groupPackages(True, lvPackages.Items)
                Dim failedMsgs As String = ""
                For Each msg In msgs
                    ' Bear in mind msg.SMS referes to the phone number
                    If Not String.IsNullOrEmpty(msg.SMS) Then
                        Dim template As EmailTemplate = getEmailTemplate("Notify_SMS-NewPackageinMailbox", msg.CustomerName)
                        'template.Content = template.Content.Replace(vbNullChar, "")
                        template.Content = template.Content.Replace(vbCrLf, "")
                        'template.Content = template.Content.Replace("%Customer%", msg.CustomerName)
                        Dim packageNum As String = msg.PackageItems.Count & " package"
                        If msg.PackageItems.Count > 1 Then
                            packageNum = packageNum & "s"
                        End If
                        template.Content = template.Content.Replace("%Packages#%", packageNum)
                        Dim trackingNums As String = ""
                        For index = 0 To msg.PackageItems.Count - 1
                            Dim pkg = msg.PackageItems(index)
                            trackingNums = trackingNums & pkg.Carrier & ": " & pkg.TrackingNo
                            If index < msg.PackageItems.Count Then
                                trackingNums = trackingNums & vbCrLf
                            End If
                        Next
                        template.Content = template.Content.Replace("%Carrier: Tracking#%", trackingNums)
                        'template.Content = template.Content.Replace("%StoreOwnerName%", _StoreOwner.StoreOwner.FNameLName)
                        'template.Content = template.Content.Replace("%StoreName%", _StoreOwner.StoreOwner.CompanyName)
                        'template.Content = template.Content.Replace("%StoreAddress%", _StoreOwner.StoreOwner.Address)
                        'template.Content = template.Content.Replace("%StorePhone%", _StoreOwner.StoreOwner.CellPhone)

                        Dim apiParams As New Dictionary(Of String, String)
                        apiParams.Add("key", ApiRequest.apiKey)
                        apiParams.Add("type", "custom")
                        apiParams.Add("phone", msg.SMS)
                        apiParams.Add("carrier", msg.smsCarrier)
                        apiParams.Add("content", _Convert.StringToBase64(template.Content))
                        Dim apiResponse As Object = JObject.Parse(ApiRequest.liminal("sms", apiParams))
                        If Not apiResponse("status") Then
                            Debug.Print(apiResponse("reason"))
                            smsStatus = smsStatusFailed
                            failedMsgs &= vbCrLf & msg.CustomerName & " at " & msg.SMS
                        Else
                            If smsStatus IsNot smsStatusFailed Then
                                smsStatus = smsStatusSuccess
                            End If
                        End If
                    Else
                        smsStatus = smsStatusFailed
                        failedMsgs &= vbCrLf & msg.CustomerName & ": no SMS number available "
                    End If
                Next
                If failedMsgs.Length > 0 Then
                    MessageBox.Show("There were problems sending SMS to these customers:" & failedMsgs, "Problem sending SMS")
                End If

                'Modify code once email module is in place

                '_EmailSetup.StoreOwner = StoreOwner ' copy store owner contact
                ''ol#1.2.24(11/18)... Don't use 'Do While' loop to avoid mass-duplicated emails.
                ''  Dim SMSPackages As New Collection
                'Dim SMSPackages As New List(Of _EmailMboxPackageItems)
                'If create_EmailPackageObjects(SMSPackages, True) Then
                '    Call set_SMSSendStatus(EmailNotification.Send_NotificationEmail(SMSPackages, _EmailSetup.file_YouHaveAPackageInMbox_SMS, gDBpath))
                'End If
            End If

            Return save_Packages()

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Processing Package.")
            Return False
        End Try
    End Function
    ''' <summary>
    ''' Groups packages in lvPackages by their recipient. Optionally uses SMS based 
    ''' on phone number and name.
    ''' </summary>
    ''' <param name="isSMS"></param>
    ''' <returns></returns>
    Private Function groupPackages(isSMS As Boolean, pkgs As ICollection) As List(Of _EmailMboxPackageItems)

        Dim emails As New List(Of _EmailMboxPackageItems)

        Try
            ' Assign each package to an email
            For Each pkg As MailboxPackageObjectObservable In pkgs
                ' Find the email (if any) that this package belongs with
                Dim targetEmail As Integer
                If isSMS Then
                    targetEmail = emails.FindIndex(
                        Function(test As _EmailMboxPackageItems)
                            Return test.SMS = pkg.SMS.Replace("-", "") And test.CustomerName = pkg.MailboxName
                        End Function)
                Else
                    targetEmail = emails.FindIndex(
                        Function(test As _EmailMboxPackageItems)
                            Return test.EmailTo = pkg.Email And test.CustomerName = pkg.MailboxName
                        End Function)
                End If

                ' There wasn't an email/text for this consignee already, make one
                If targetEmail = -1 Then
                    Dim newEmail As New _EmailMboxPackageItems
                    newEmail.CustomerName = pkg.MailboxName
                    If isSMS Then
                        If pkg.SMS IsNot Nothing Then
                            newEmail.SMS = pkg.SMS.Replace("-", "")
                            newEmail.smsCarrier = pkg.smsCarrier
                            targetEmail = emails.Count
                            emails.Add(newEmail)
                        End If
                    Else
                        newEmail.EmailTo = pkg.Email
                        targetEmail = emails.Count
                        emails.Add(newEmail)
                    End If
                End If

                ' Add the package to the appropriate email
                If targetEmail > -1 Then
                    Dim package As New _EmailMboxPackageItem(pkg, CheckInOut.CheckIn)
                    emails(targetEmail).PackageItems.Add(package)
                End If
            Next

            Return emails

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Processing Package.")
            Return emails
        End Try
    End Function
    Private Function sendEmails(pkgs As ICollection)
        Dim emails = groupPackages(False, pkgs)

        ' Fill out templates and send the emails
        Dim failedEmails As String = ""
        For Each email In emails
            If Not String.IsNullOrEmpty(email.EmailTo) Then
                Dim template As EmailTemplate = getEmailTemplate("Notify_Email-NewPackageinMailbox", email.CustomerName)
                'template.Content = template.Content.Replace(vbNullChar, "")
                'template.Content = template.Content.Replace("%Customer%", email.CustomerName)
                Dim packageNum As String = email.PackageItems.Count & " package"
                If email.PackageItems.Count > 1 Then
                    packageNum = packageNum & "s"
                End If
                template.Content = template.Content.Replace("%Packages#%", packageNum)
                Dim trackingNums As String = ""
                For index = 0 To email.PackageItems.Count - 1
                    Dim pkg = email.PackageItems(index)
                    trackingNums = trackingNums & pkg.Carrier & ": " & pkg.TrackingNo
                    If index < email.PackageItems.Count Then
                        trackingNums = trackingNums & "<br>"
                    End If
                Next
                template.Content = template.Content.Replace("%Carrier: Tracking#%", trackingNums)
                ' template.Content = template.Content.Replace("%StoreOwnerName%", _StoreOwner.StoreOwner.FNameLName)
                ' template.Content = template.Content.Replace("%StoreName%", _StoreOwner.StoreOwner.CompanyName)
                ' template.Content = template.Content.Replace("%StoreAddress%", _StoreOwner.StoreOwner.Address)
                ' template.Content = template.Content.Replace("%StorePhone%", _StoreOwner.StoreOwner.CellPhone)

                If sendEmail(email.EmailTo, template) Then
                    If emailStatus IsNot emailStatusFailed Then
                        emailStatus = emailStatusSuccess
                    End If
                Else
                    emailStatus = emailStatusFailed
                    ' Notification per email about failure
                    ' Dim msgboxText As String = "Failed to send email to " & email.CustomerName & " at " & email.EmailTo
                    ' MessageBox.Show(msgboxText, "Problem sending email")
                    failedEmails &= vbCrLf & email.CustomerName & " at " & email.EmailTo
                End If
            Else
                emailStatus = emailStatusFailed
                ' Notification per email about failure
                ' Dim msgboxText As String = "Failed to send email to " & email.CustomerName & ": no email available "
                ' MessageBox.Show(msgboxText, "Problem sending email")
                failedEmails &= vbCrLf & email.CustomerName & ": no email available "
            End If
        Next

        ' Give the user a summary of the email problems
        If failedEmails.Length > 0 Then
            MessageBox.Show("There were problems sending emails to these customers:" & failedEmails, "Problem sending emails")
        End If
    End Function
    Private Function save_Packages() As Boolean

        Try
            save_Packages = True ' assume.

            'Old code 
            'For Each lvItem As ListViewItem In Me.lvPackages.Items
            '    If save_Package(lvItem) Then
            '        Me.lvPackages.Items(lvItem.Index).Remove()
            '    End If
            'Next
            ''_SigPlusPad.Signature_FileName = String.Empty
            'save_Packages = (0 = Me.lvPackages.Items.Count)
            'Old code 

            Dim count As Integer = lvPackages.Items.Count
            Dim index As Integer = 0

            If count > 0 Then
                Do While index < count
                    If save_Package(index) > 0 Then
                    Else
                        save_Packages = False
                        _MsgBox.WarningMessage("Error inserting check-in records into the database", "Failed to Process!")
                    End If
                    index += 1
                Loop
            End If

            _SigPlusPad.Signature_FileName = String.Empty

            'Enable code once signature module is in place
            ''_SigPlusPad.Signature_FileName = String.Empty
        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Saving Package.")
            Return False
        End Try
    End Function

    ' Returns -1 if error, or the number of database rows affected
    Private Function save_Package(ByVal index As Integer) As Integer
        Try
            Dim obj As MailboxPackageObjectObservable = CType(lvPackages.Items(index), MailboxPackageObjectObservable)

            Dim Segment As String = ""
            Dim SQL As String = ""

            Segment = AddElementToSegment(Segment, "MailboxNo", obj.MailboxNo.ToString)
            Segment = AddElementToSegment(Segment, "MailboxName", obj.MailboxName)
            Segment = AddElementToSegment(Segment, "CarrierName", obj.CarrierName)

            'IsGround check needs to come here after FedEx-Web module is in place
            If obj.CarrierName.Contains("FedEx") Then
                If _MailboxPackage.IsFedExWebEnabled And _MailboxPackage.IsFedExHALEnabled Then
                    Dim isGround As Boolean = False
                    If upload_FedExPackage(obj, "Delivered", isGround) Then
                        Segment = AddElementToSegment(Segment, "IsGround", isGround.ToString)
                    Else
                        obj.FColor = System.Windows.Media.Brushes.Red
                        lvPackages.Items.Refresh()
                        Return -1
                    End If
                End If
            End If
            'IsGround check needs to come here after FedEx-Web module is in place

            Segment = AddElementToSegment(Segment, "TrackingNo", obj.TrackingNo)
            Segment = AddElementToSegment(Segment, "PackageClass", obj.PackageClass)
            Segment = AddElementToSegment(Segment, "BarCodeScan", obj.BarCodeScan)

            If Not CType(obj.CustomerID, String) = "" Then
                Segment = AddElementToSegment(Segment, "CustomerID", obj.CustomerID.ToString)
            End If

            Segment = AddElementToSegment(Segment, "ReceivedDate", obj.ReceivedDate.ToString)
            Segment = AddElementToSegment(Segment, "CheckInNotes", obj.CheckInNotes)
            Segment = AddElementToSegment(Segment, "Location", obj.Location)
            Segment = AddElementToSegment(Segment, "Clerk", gCurrentUser)

            SQL = MakeInsertSQLFromSchema("Mailbox_Packages", Segment, gMailBoxSchema, True)

            Return IO_UpdateSQLProcessor(gMailboxDB, SQL)
        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Saving Package.")
            Return -1
        End Try
    End Function

    Private Function RemoveFromList(ByVal itemIndex As Integer) As Boolean
        Try
            MailBoxInfo.RemoveAt(itemIndex)
            Return True
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Private Sub cmdClearCurrent_Click(sender As Object, e As RoutedEventArgs) Handles cmdClearCurrent.Click
        Try
            'Changed from Me.lvPackages.SelectedItems IsNot Nothing to Me.lvPackages.SelectedIndex > -1 because of WPF
            While Me.lvPackages.SelectedIndex > -1 And 0 < Me.lvPackages.Items.Count
                If RemoveFromList(lvPackages.SelectedIndex) Then
                    lvPackages.ItemsSource = MailBoxInfo
                    lblPackageCount.Content = lvPackages.Items.Count
                End If
            End While
        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Failed to delete selected Package...")
        End Try
    End Sub

    Private Sub lvPackages_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles lvPackages.MouseDoubleClick
        Try
            Dim lvPackage_Selected As New MailboxPackageObjectObservable()

            If Me.lvPackages.SelectedItems IsNot Nothing AndAlso 0 < Me.lvPackages.Items.Count Then
                lvPackage_Selected = CType(lvPackages.SelectedItem, MailboxPackageObjectObservable)
                Call set_SelectedCarrier(lvPackage_Selected.CarrierName)
                Me.txtPackageTrackingNo.Text = lvPackage_Selected.TrackingNo
                Me.txtDesc.Text = lvPackage_Selected.CheckInNotes
                Me.txtLocation.Text = lvPackage_Selected.Location
                Me.txtMailboxNo.Text = lvPackage_Selected.MailboxNo

                load_Mailbox(txtMailboxNo, txtExpDate, txtMailboxName)

                'load_mailbox will selecte default mailbox name
                Me.txtMailboxName.Text = lvPackage_Selected.MailboxName

                If RemoveFromList(lvPackages.SelectedIndex) Then
                    lvPackages.ItemsSource = MailBoxInfo
                End If

            End If

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Failed to bring out Package entry...")
        End Try
    End Sub

    Private Sub txtMailboxNo1_KeyDown(sender As Object, e As KeyEventArgs) Handles txtMailboxNo1.KeyDown
        Try
            If e.Key = Key.Return Then
                If 0 < Me.txtMailboxNo1.Text.Length Then
                    Call clear_List()
                    Call load_Mailbox(txtMailboxNo1, txtExpDate1, txtMailboxName1)
                    Call load_Packages(Me.txtMailboxNo1.Text)
                Else
                    _MsgBox.InformationMessage("Enter valid mailbox number!", "Mailbox# is Missing!")
                End If
            End If
        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Failed to load Address Entry form...")
        End Try
    End Sub

    Private Sub clear_List()
        lvPackages1.ItemsSource = Nothing
        PackageCheckOutInfo.Clear()
    End Sub

    Private Sub clear_Form1()
        Try
            If 0 < Val(Me.txtMailboxNo.Text) Then
                Me.txtMailboxNo.Text = String.Empty
            End If
            Me.txtMailboxName1.Text = String.Empty
            Me.txtMailboxName1.Items.Clear()
            Me.txtExpDate.Content = String.Empty
            Me.txtPackageTrackingNo1.Text = String.Empty
            Me.txtPickedupBy.Text = String.Empty
            Me.txtPickedupBy.Items.Clear()
            Me.txtCheckOutNotes.Text = String.Empty
            Me.lblCarrierName.Content = String.Empty
            Me.lblDateReceived.Content = String.Empty
            Me.txtPackageTrackingNo1.Focus()
        Catch ex As Exception
            Throw ex
        End Try
    End Sub



    Private Function load_Mailbox(ByRef MBX_No_TxtBx As TextBox, ByRef ExpDate_Lbl As Label, ByRef Name_TxtBx As ComboBox) As Boolean
        load_Mailbox = False ' assume.
        Dim current_segment = ""
        If IsNumeric(MBX_No_TxtBx.Text) Then
            Dim mboxNumber As Integer = Val(MBX_No_TxtBx.Text)
            Dim expdate As String = ""
            Dim isRented As Boolean
            'If mboxNumber = 0 Then
            '    ExpDate_Lbl.Content = ""
            'End If
            If Mailbox_GetExpirationDate(mboxNumber, expdate, isRented) Then
                If isRented Then
                    If Not expdate = "" Then
                        Dim dateString = Format(CDate(expdate), "d")
                        If CDate(expdate) < Date.Today Then
                            ' Expired
                            If ExpDate_Lbl.Width > 105 Then
                                ExpDate_Lbl.Content = String.Format("Expired on " & "{0}", dateString)
                            Else
                                ExpDate_Lbl.Content = String.Format("Expired on" & vbCrLf & "{0}", dateString)
                            End If

                            ExpDate_Lbl.Foreground = System.Windows.Media.Brushes.Red
                        Else
                            ExpDate_Lbl.Content = dateString
                            ExpDate_Lbl.Foreground = System.Windows.Media.Brushes.Navy
                        End If
                    End If
                    '
                    Me.txtPickedupBy.Items.Clear()
                    Me.txtPickedupBy.Text = String.Empty

                    Dim mailBoxList As String = ""
                    Name_TxtBx.Items.Clear()
                    If Mailbox_GetNameList(mboxNumber, mailBoxList) Then

                        Do Until mailBoxList = ""
                            current_segment = GetNextSegmentFromSet(mailBoxList)
                            Name_TxtBx.Items.Add(_Convert.Null2DefaultValue(ExtractElementFromSegment("MboxName", current_segment)))
                            txtPickedupBy.Items.Add(_Convert.Null2DefaultValue(ExtractElementFromSegment("MboxName", current_segment)))
                        Loop
                        Name_TxtBx.SelectedItem = Get_MailboxName(mboxNumber)
                        txtPickedupBy.SelectedItem = Get_MailboxName(mboxNumber)
                    End If
                    Name_TxtBx.Text = Mailbox_GetName(mboxNumber)
                    txtPickedupBy.Text = Mailbox_GetName(mboxNumber)

                    ''ol#1.2.02(6/2)... Pre-select 'Signed By' on Check Out to the same name as the Mailbox holder.
                    'Me.txtPickedupBy.Text = Name_TxtBx.Text

                    load_Mailbox = True
                Else
                    ''ol#1.1.82(12/15)... If a mailbox exists but not rented the program should state so instead of the 'Exp Date' showing today's date.
                    ExpDate_Lbl.Content = "Exists but Not Rented!"
                    Name_TxtBx.Text = ""
                    ExpDate_Lbl.Foreground = System.Windows.Media.Brushes.Red
                End If
            ElseIf mboxNumber = 0 Then
                ExpDate_Lbl.Content = String.Empty
            Else
                ''ol#1.1.82(12/15)... If a mailbox doesn't exist the program should state so instead of the 'Exp Date' showing Year 1.
                ExpDate_Lbl.Content = "Doesn't Exist!"
                Name_TxtBx.Text = ""
                ExpDate_Lbl.Foreground = System.Windows.Media.Brushes.Red
            End If
        End If
        Return True
    End Function


    Private Function Get_MailboxName(mbx_no As Double) As String
        Return ExtractElementFromSegment("Name", IO_GetSegmentSet(gShipriteDB, "Select Name From Mailbox Where MailboxNumber=" & mbx_no), "")
    End Function

    Private Function RemoveFromList1(ByVal itemIndex As Integer) As Boolean
        Try
            PackageCheckOutInfo.RemoveAt(itemIndex)
            Return True
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Private Function RemoveSelectedFromList(ByVal trackingNumber As String) As Boolean
        Try
            RemoveSelectedFromList = False 'assume

            Dim dataTable = New DataTable()
            Dim rowCount As Integer = 0

            dataTable.Columns.Add("Index")
            dataTable.Columns.Add("TrackingNo")

            For Each package In PackageCheckOutInfo
                Dim newRow = dataTable.NewRow()

                newRow("Index") = rowCount
                newRow("TrackingNo") = package.TrackingNo

                dataTable.Rows.Add(newRow)

                rowCount = rowCount + 1
            Next

            Dim keyRow As DataRow
            keyRow = dataTable.Select("TrackingNo = '" + trackingNumber + "'")(0)

            Dim itemIndex As Integer

            If Not keyRow("Index") = "" Then
                itemIndex = CType(keyRow("Index"), Integer)
                PackageCheckOutInfo.RemoveAt(itemIndex)
                Return True
            End If

        Catch ex As Exception
            Throw ex
        End Try
    End Function



    Private Function load_Packages(ByVal mailbox As String) As Boolean
        load_Packages = False ' assume.

        Dim packagesList As String = ""
        Dim current_segment As String = ""

        If _MailboxPackagesDB.Read_Packages(mailbox, packagesList) Then
            Do Until packagesList = ""
                current_segment = GetNextSegmentFromSet(packagesList)
                Call add_ListItem1(current_segment, True)
            Loop
        End If
        Call chkHoldForPickup_VisibleOnOff()
        Return True
    End Function

    Private Sub chkHoldForPickup_VisibleOnOff()
        Try
            If 0 < Val(Me.txtMailboxNo1.Text) Then
                'Me.lblExpDate.Visibility = Visibility.Visible
                Me.txtExpDate.Visibility = Visibility.Visible
            Else
                'Me.lblExpDate.Visibility = Visibility.Hidden
                Me.txtExpDate.Visibility = Visibility.Hidden
            End If
        Catch ex As Exception
            Throw ex
        End Try
    End Sub

    Private Function add_ListItem1(ByVal current_segment As String, Optional selectAdded As Boolean = False) As Boolean
        Try
            add_ListItem1 = False ' assume.
            '

            Dim lvObject As New _ListItemWithObject
            lvObject.ItemID = ExtractElementFromSegment("PackageID", current_segment) ' PackageID in Db
            lvObject.ItemIndex = _Convert.Null2DefaultValue(ExtractElementFromSegment("MailboxNo", current_segment))
            lvObject.ItemText = _Convert.Null2DefaultValue(ExtractElementFromSegment("MailboxName", current_segment))

            ''ol#1.2.42(10/4)... FedEx HAL upload was added to Package Check In/Out forms.
            Dim mobj As New MailboxPackageObject
            mobj.BarCodeScan = _Convert.Null2DefaultValue(ExtractElementFromSegment("BarCodeScan", current_segment))
            mobj.CarrierName = _Convert.Null2DefaultValue(ExtractElementFromSegment("CarrierName", current_segment))
            mobj.TrackingNo = _Convert.Null2DefaultValue(ExtractElementFromSegment("TrackingNo", current_segment))
            mobj.PackageClass = _Convert.Null2DefaultValue(ExtractElementFromSegment("PackageClass", current_segment))
            mobj.SignatureFile = _Convert.Null2DefaultValue(ExtractElementFromSegment("Signature", current_segment))
            mobj.IsGround = CType(_Convert.Null2DefaultValue(ExtractElementFromSegment("IsGround", current_segment), False), Boolean)
            lvObject.ItemObject = mobj

            Dim foundInList As Integer = -1
            For Each search As MailboxPackageObjectObservable In PackageCheckOutInfo
                If search.TrackingNo = mobj.TrackingNo Then
                    foundInList = PackageCheckOutInfo.IndexOf(search)
                End If
                If foundInList > -1 Then
                    Exit For
                End If
            Next

            ' If the package is not already in the list, add it
            Dim sql As String = "SELECT EMail FROM contacts WHERE ID=" & _Convert.Null2DefaultValue(ExtractElementFromSegment("CustomerID", current_segment, "0"))
            Dim contact As String = SegmentFunctions.GetNextSegmentFromSet(IO_GetSegmentSet(gShipriteDB, sql))
            If foundInList = -1 Then
                Dim newId = PackageCheckOutInfo.Count
                PackageCheckOutInfo.Add(New MailboxPackageObjectObservable() With {
                    .CarrierName = _Convert.Null2DefaultValue(ExtractElementFromSegment("CarrierName", current_segment)),
                    .TrackingNo = _Convert.Null2DefaultValue(ExtractElementFromSegment("TrackingNo", current_segment)),
                    .ReceivedDate = _Convert.Null2DefaultValue(ExtractElementFromSegment("ReceivedDate", current_segment)),
                    .Location = _Convert.Null2DefaultValue(ExtractElementFromSegment("Location", current_segment)),
                    .CheckInNotes = _Convert.Null2DefaultValue(ExtractElementFromSegment("CheckInNotes", current_segment)),
                    .PickedupBy = _Convert.Null2DefaultValue(ExtractElementFromSegment("PickedupBy", current_segment)),
                    .CheckOutNotes = _Convert.Null2DefaultValue(ExtractElementFromSegment("CheckOutNotes", current_segment)),
                    .ItemID = _Convert.Null2DefaultValue(ExtractElementFromSegment("PackageID", current_segment)),
                    .MailboxName = _Convert.Null2DefaultValue(ExtractElementFromSegment("MailboxName", current_segment)),
                    .MailboxNo = _Convert.Null2DefaultValue(ExtractElementFromSegment("MailboxNo", current_segment)),
                    .Email = _Convert.Null2DefaultValue(ExtractElementFromSegment("EMail", contact))
                })

                lvPackages1.ItemsSource = PackageCheckOutInfo

                ' Select the newly added package
                If selectAdded Then
                    lvPackages1.SelectedItems.Add(lvPackages1.Items(newId))
                End If
            Else
                ' Item was already in the list, select it.
                lvPackages1.SelectedItems.Add(lvPackages1.Items(foundInList))
            End If

            lblPackageCount1.Content = lvPackages1.Items.Count

            Return True
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Private Sub CheckOut_TabItem_Loaded()
        Try
            Call clear_Form1()
            Call clear_List()
            Call Load_Packages_OverDaysOld(-1)
        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Failed to load Check Out tab..")
        End Try
    End Sub

    Private Sub PackageInventory_TabItem_Loaded()
        Try
            Call clear_Form_PI()
            Call clear_List_PI()


        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error in Package Inventory Tab Item Load")
        End Try
    End Sub

    Private Sub Load_Packages_OverDaysOld(ByVal overDaysOld As Integer)
        ''ol#1.2.45(2/8)... List all the packages then over 7 days on Check Out for driver.
        Dim current_segment As String
        Dim receivedDate As String

        Dim SQL As String = "SELECT * FROM Mailbox_Packages WHERE PickedupBy IS NULL " &
            "AND DateDiff('d', ReceivedDate,  Now()) > " & overDaysOld &
            " ORDER BY MailboxName, ReceivedDate"

        Dim packageList As String = IO_GetSegmentSet(gMailboxDB, SQL)
        If packageList.Length > 0 Then
            While packageList.Length > 0
                current_segment = GetNextSegmentFromSet(packageList)
                receivedDate = _Convert.Null2DefaultValue(ExtractElementFromSegment("ReceivedDate", current_segment))
                If receivedDate <> "" Then
                    Call add_ListItem1(current_segment)
                End If
            End While
        End If
    End Sub

    Private Function load_Packages(ByVal mailbox As String, ByVal mailboxname As String) As Boolean
        load_Packages = False ' assume.
        Dim packageList As String = ""
        Dim current_segment As String = ""
        If mailbox Is Nothing Then
            If _MailboxPackagesDB.Read_Packages_By_Name(mailboxname, packageList) Then
                Do Until packageList = ""
                    current_segment = GetNextSegmentFromSet(packageList)
                    Call add_ListItem1(current_segment, True)
                Loop
            End If
        ElseIf mailboxname Is Nothing Then
            If _MailboxPackagesDB.Read_Packages_By_Number(Val(mailbox), packageList) Then
                Do Until packageList = ""
                    current_segment = GetNextSegmentFromSet(packageList)
                    Call add_ListItem1(current_segment, True)
                Loop
            End If
        Else
            If _MailboxPackagesDB.Read_Packages(mailbox, mailboxname, packageList) Then
                Do Until packageList = ""
                    current_segment = GetNextSegmentFromSet(packageList)
                    Call add_ListItem1(current_segment, True)
                Loop
            End If
        End If
        Return True
    End Function

    Private Sub txtPackageTrackingNo1_KeyDown(sender As Object, e As KeyEventArgs) Handles txtPackageTrackingNo1.KeyDown
        Try
            If e.Key = Key.Return Then
                If Me.txtPackageTrackingNo1.Text.Length > 0 Then
                    If load_Package(Me.txtPackageTrackingNo1.Text) Then
                        If 0 < Val(Me.txtMailboxNo1.Text) Then
                            Me.txtPickedupBy.Focus()
                        End If
                    End If
                End If
            End If
        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Failed to find the package by tracking#...")
        End Try
    End Sub

    ''' <summary>
    ''' Pulls package by trakcing number in CheckIn
    ''' </summary>
    ''' <param name="trackingNo"></param>
    ''' <returns></returns>
    Private Function load_Package(ByVal trackingNo As String) As Boolean
        load_Package = False ' assume.
        ''ol#1.2.24(11/16)... In Check Out, truncate FedEx tracking# to 12 digits before looking for it in database.

        Call BarCode.ShippingCo(trackingNo)

        Dim packagesList As String = ""
        Dim current_segment As String = ""

        Dim packagesListTwo As String = ""
        Dim current_segmentTwo As String = ""
        If _MailboxPackagesDB.Read_Package(trackingNo, packagesList) Then
            current_segment = GetNextSegmentFromSet(packagesList)
            If current_segment.Length > 0 Then
                If String.IsNullOrEmpty(ExtractElementFromSegment("PickedUpDate", current_segment)) Then
                    PackageCheckOutInfo.Clear()
                    If current_segment.Length > 0 Then
                        add_ListItem1(current_segment, True)
                    End If
                    txtMailboxNo1.Text = ExtractElementFromSegment("MailboxNo", current_segment)
                    txtMailboxName1.Text = ExtractElementFromSegment("MailboxName", current_segment)
                    load_Mailbox(txtMailboxNo1, txtExpDate1, txtMailboxName1)

                    lblCarrierName.Content = ExtractElementFromSegment("CarrierName", current_segment)
                    lblDateReceived.Content = Format(CDate(ExtractElementFromSegment("ReceivedDate", current_segment)), "d")

                    txtPickedupBy.Text = txtMailboxName1.Text
                Else
                    _MsgBox.InformationMessage("This package was already picked up by " & _Convert.Null2DefaultValue(ExtractElementFromSegment("PickedupBy", current_segment)) & " on " & _Convert.Null2DefaultValue(ExtractElementFromSegment("PickedupDate", current_segment)), "Already Checked Out!")
                End If
            End If

        End If
        If 0 = Val(Me.txtMailboxNo1.Text) Then
            Me.txtPackageTrackingNo1.Text = String.Empty
        End If
        Call chkHoldForPickup_VisibleOnOff()
    End Function

    Private Sub txtMailboxNo1_LostFocus(sender As Object, e As RoutedEventArgs) Handles txtMailboxNo1.LostFocus
        Try
            Call chkHoldForPickup_VisibleOnOff()
        Catch ex As Exception

        End Try
    End Sub

    Private Sub txtMailboxName1_KeyDown(sender As Object, e As KeyEventArgs) Handles txtMailboxName1.KeyDown
        Try
            If e.Key = Key.Return Then
                If 0 < txtMailboxName1.Text.Length Then
                    clear_List()
                    load_Packages(Nothing, txtMailboxName1.Text)

                    txtPickedupBy.Text = txtMailboxName1.Text
                    'txtMailboxNo1.Text = _MailboxPackagesDB.Mailbox_GetNumber(txtMailboxName1.Text)
                Else
                    _MsgBox.InformationMessage("Enter valid Customer name!", "Name is Missing!")
                End If
            End If
        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Failed to display all the packages for selected customer...")
        End Try
    End Sub

    Private Sub txtMailboxName1_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles txtMailboxName1.SelectionChanged
        Try
            If 0 = Val(Me.txtMailboxNo1.Text) Then
                If Me.txtMailboxName1.SelectedItem IsNot Nothing Then
                    Call clear_List()
                    Call load_Packages(Nothing, Me.txtMailboxName1.Text)
                    Me.txtPickedupBy.Text = Me.txtMailboxName1.Text
                End If
            End If
        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Failed to display all the packages for selected customer...")
        End Try
    End Sub

    Private Sub lvPackages1_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles lvPackages1.MouseDoubleClick
        Try
            Dim lvPackage1_Selected As MailboxPackageObjectObservable = CType(e.OriginalSource, FrameworkElement).DataContext


            'If Me.lvPackages1.SelectedIndex > -1 AndAlso 0 < Me.lvPackages1.Items.Count Then
            If lvPackage1_Selected IsNot Nothing Then
                Me.txtPackageTrackingNo1.Text = lvPackage1_Selected.TrackingNo
                Me.lblDateReceived.Content = Format(lvPackage1_Selected.ReceivedDate, "d")
                Me.txtMailboxNo1.Text = lvPackage1_Selected.MailboxNo
                Me.txtMailboxName1.Text = lvPackage1_Selected.MailboxName
                Me.lblCarrierName.Content = lvPackage1_Selected.CarrierName

                Call load_Mailbox(txtMailboxNo1, txtExpDate1, txtMailboxName1)
            End If


        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Failed to display the selected package...")
        End Try
    End Sub

    Private Function create_PackageObject(ByRef package As MailboxPackageObject) As Boolean

        package = New MailboxPackageObject
        With package
            .CarrierName = Me.lblCarrierName.Content
            .MailboxName = Me.txtMailboxName1.Text
            .MailboxNo = Val(Me.txtMailboxNo.Text)
            .TrackingNo = Me.txtPackageTrackingNo1.Text
            If _Date.IsDate_(Me.lblDateReceived.Content) Then
                .ReceivedDate = CDate(Me.lblDateReceived.Content)
            Else
                .ReceivedDate = Date.Today
            End If
            .PickedupBy = Me.txtPickedupBy.Text
            .PickedupDate = Date.Now
        End With
        Return True
    End Function

    Private Sub cmdPrintNotice_Click(sender As Object, e As RoutedEventArgs) Handles cmdPrintNotice.Click
        Try
            Cursor = Cursors.Wait

            If Me.lvPackages1.SelectedIndex > -1 And 0 < Me.lvPackages1.Items.Count Then
                Dim package As MailboxPackageObject = Nothing
                If create_PackageObject(package) Then
                    _MailboxPackage.Print_PackageNotices(lvPackages1, package)
                End If
                package = Nothing
            Else
                _MsgBox.InformationMessage("At least one package should be checked!", "Check a Package!")
            End If
        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Failed to print Notice...")
        Finally
            Cursor = Cursors.Arrow
        End Try
    End Sub

    Private Sub cmdPrintPickTicket_Click(sender As Object, e As RoutedEventArgs) Handles cmdPrintPickTicket.Click
        Try
            If Me.lvPackages1.SelectedIndex > -1 And 0 < Me.lvPackages1.Items.Count Then
                Call _MailboxPackage.Print_PICK_Ticket_Report(Me.lvPackages1)
            Else
                _MsgBox.InformationMessage("At least one package should be checked!", "Check a Package!")
            End If
        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Failed to print PICK ticket...")
        End Try
    End Sub

    Private Sub cmdShowAll_Click(sender As Object, e As RoutedEventArgs) Handles cmdShowAll.Click
        Try
            Call clear_List()

            If (Not String.IsNullOrEmpty(txtMailboxNo1.Text)) And Not (String.IsNullOrEmpty(txtMailboxName1.Text)) Then
                ' Both name and # filled
                load_Mailbox(txtMailboxNo1, txtExpDate1, txtMailboxName1)
                load_Packages(txtMailboxNo1.Text, txtMailboxName1.Text)
            ElseIf Not String.IsNullOrEmpty(txtMailboxNo1.Text) Then
                ' # is filled, name is empty
                load_Mailbox(txtMailboxNo1, txtExpDate1, txtMailboxName1)
                load_Packages(txtMailboxNo1.Text)
            ElseIf Not String.IsNullOrEmpty(txtMailboxName1.Text) Then
                ' Name is filled, # is empty
                load_Packages("0", txtMailboxName1.Text)
            Else
                ' both empty
                load_Packages("0")
            End If

            'If String.IsNullOrEmpty(txtMailboxNo1.Text) Then
            '    Call load_Mailbox(txtMailboxNo1, txtExpDate1, txtMailboxName1)
            '    Call load_Packages(Me.txtMailboxNo1.Text)
            'ElseIf String.IsNullOrEmpty(txtMailboxName1.Text) Then
            '    Call load_Packages(Nothing, txtMailboxNo1.Text)
            'Else
            '    Call load_Packages("0")
            'End If
        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Failed to display all the packages for selected mailbox...")
        End Try
    End Sub

    Private Sub cmdPickedup_Click(sender As Object, e As RoutedEventArgs) Handles cmdPickedup.Click
        Try
            If isAdultSignatureRequired() Then
                isAdult = True
                Disclaimer_Popup.IsOpen = True
            Else
                isAdult = False
                Call pickup_And_sign("Delivered")
                txtMailboxNo1.Text = ""
                txtMailboxName1.Text = ""
                txtPackageTrackingNo1.Text = ""
                txtPickedupBy.Text = ""
                txtCheckOutNotes.Text = ""
            End If

            lblPackageCount1.Content = lvPackages1.Items.Count

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Failed to save pickup...")
        End Try
    End Sub

    Private Sub pickup_And_sign(ByVal reason As String)
        Try
            Dim obj As New MailboxPackageObjectObservable()

            If Not 0 = Me.txtPickedupBy.Text.Length Then
                If Me.lvPackages1.SelectedIndex > -1 Then
                    Dim pkg As MailboxPackageObjectObservable
                    For i = lvPackages1.SelectedItems.Count - 1 To 0 Step -1
                        pkg = lvPackages1.SelectedItems(i)
                        If save_Package1(pkg, reason) AndAlso RemoveSelectedFromList(pkg.TrackingNo) Then
                            lvPackages1.ItemsSource = PackageCheckOutInfo
                        End If
                    Next

                    If Me.lvPackages1.SelectedItems.Count = 0 Then
                        Call clear_Form(True)
                        Me.txtPackageTrackingNo.Focus()
                        _MsgBox.InformationMessage(String.Format("{0} was saved successfully!", reason), "Saved!")
                    Else
                        _MsgBox.InformationMessage("Some of the packages were not processed and not saved!", "Failed to Process!")
                    End If
                Else
                    _MsgBox.InformationMessage("At least one package should be checked!", "Check a Package!")
                End If
                _SigPlusPad.Signature_FileName = String.Empty
            Else
                _MsgBox.InformationMessage("Select or type in Customer name!", "Signed By is a Required Field!")
            End If
        Catch ex As Exception
            Throw ex
        End Try
    End Sub

    Private Function save_Package1(ByVal obj As MailboxPackageObjectObservable, ByVal reason As String) As Boolean
        Dim sql2cmd As New sqlUpdate
        Dim sql2exe As String = String.Empty

        ''ol#1.2.42(10/4)... FedEx HAL upload was added to Package Check In/Out forms.
        If "FedEx" = obj.CarrierName Then
            If _MailboxPackage.IsFedExWebEnabled AndAlso _MailboxPackage.IsFedExHALEnabled Then
                ''ol#1.2.61(9/18)... 'Refused Package' should have 'Refused' tag in ProofOfDelivery call.
                If Not upload_FedExPackage(obj, reason, Nothing) Then
                    'insert code to color non uploaded package in list to red
                    Return False ' exit and don't save
                End If
            End If
        End If
        ''
        ''ol#1.2.61(9/18)... 'Refused Package' should have 'Refused' tag in ProofOfDelivery call.
        Call sql2cmd.Qry_UPDATE("PickedupBy", Me.txtPickedupBy.Text, sql2cmd.TXT_, True, False, "Mailbox_Packages", "PackageID = " & obj.ItemID)
        If Not "Refused" = reason Then
            Call sql2cmd.Qry_UPDATE("PickedupDate", Date.Now.ToString, sql2cmd.TXT_)
        End If


        If Not String.IsNullOrEmpty(_SigPlusPad.Signature_FileName) Then
            Call sql2cmd.Qry_UPDATE("Signature", _SigPlusPad.Signature_FileName, sql2cmd.TXT_)
        End If


        sql2exe = sql2cmd.Qry_UPDATE("CheckOutNotes", Me.txtCheckOutNotes.Text, sql2cmd.TXT_, False, True)

        save_Package1 = DatabaseFunctions.IO_UpdateSQLProcessor(gMailboxDB, sql2exe)
        sql2cmd = Nothing
    End Function

    ' Reason is either Refused or Delivered. This refers to what is happening with the package.
    ' Returns false if and only if there were problems with the actual upload process.
    ' Returns true otherwise, including if the package was NOT FedEx, not HAL, etc.
    Private Function upload_FedExPackage(ByVal obj As MailboxPackageObjectObservable, ByVal reason As String, ByRef isGround As Boolean) As Boolean
        upload_FedExPackage = True ' assume.
        ''ol#1.2.42(10/4)... FedEx HAL upload was added to Package Check In/Out forms.
        If "FedEx" = obj.CarrierName Then
            If _MailboxPackage.IsFedExWebEnabled AndAlso _MailboxPackage.IsFedExHALEnabled Then
                ''ol#1.2.59(9/12)... 'Proof of Delivery' call is required for NONE MAILBOX HOLDERS as well.
                If obj.PackageClass IsNot Nothing AndAlso obj.PackageClass.Contains(_MailboxPackage.FEDEX_HAL) Then
                    Dim ourShipmentResponse As New baseWebResponse_Shipment
                    Dim resPack As New baseWebResponse_Package
                    resPack.PackageID = obj.BarCodeScan ' scanned barcode. Note: The ampersand before and after. Its required.
                    resPack.TrackingNo = obj.TrackingNo ' we need it only to create unique xml file name
                    resPack.Recipient = _MailboxPackage.StoreOwner
                    ''ol#1.2.61(9/18)... 'Refused Package' should have 'Refused' tag in ProofOfDelivery call.
                    resPack.LabelCODImage = reason
                    resPack.LabelCustomsImage = Me.txtCheckOutNotes.Text
                    ourShipmentResponse.Packages.Add(resPack)
                    _FedExWeb.objFedEx_Setup = _FedExWeb.objFedEx_Regular_Setup
                    If _FedExHAL.Process_TransferAcceptanceEvent_Request(ourShipmentResponse) Then
                        isGround = _Controls.Contains(resPack.ServiceCode, "Ground")

                        ' Upload the package data to FedEx's server
                        _FedExWeb.objFedEx_Setup = _FedExWeb.objFedEx_Regular_Setup
                        If _Files.Create_Folder(SignatureFolder, True) Then
                            If signed_bySigPlusPad(True) Then
                                If _FedExHAL.Process_PublishDeliveryEvent_Request(_SigPlusPad.Signature_FileName, ourShipmentResponse) Then
                                    upload_FedExPackage = (ourShipmentResponse.ShipmentAlerts.Count = 0)
                                    If Not upload_FedExPackage Then
                                        MessageBox.Show(resPack.TrackingNo & vbCr_ & ourShipmentResponse.ShipmentAlerts(0).ToString, "FedEx Web Server", MessageBoxButton.OK, MessageBoxImage.Warning)
                                    End If
                                End If
                            End If
                        End If
                    End If
                End If
            End If
        End If
    End Function

    Private Function isAdultSignatureRequired() As Boolean
        ''ol#1.2.44(1/30)... Adult Signature disclaimer needed.
        isAdultSignatureRequired = False ' assume.

        're-write once Fedex_Data2XML modules are fully in place ?
        _FedExWeb.objFedEx_Setup = _FedExWeb.objFedEx_Regular_Setup
        For Each obj As MailboxPackageObjectObservable In lvPackages1.SelectedItems
            If "FedEx" = obj.CarrierName Then
                If _MailboxPackage.IsFedExWebEnabled AndAlso _MailboxPackage.IsFedExHALEnabled Then
                    Dim filename As String = String.Format("{0}\TransferAcceptance_{1}_Reply.xml", _FedExWeb.objFedEx_Setup.Path_SaveDocXML, obj.TrackingNo)
                    If _Files.IsFileExist(filename, False) Then
                        ' read if Adult Signature is required
                        Dim xdoc As New Xml.XmlDocument
                        Dim value As String = String.Empty
                        xdoc.Load(filename)
                        Dim nreader As New Xml.XmlNodeReader(xdoc)
                        ' Note! There were 2 layers of this If, both doing GetValueByNodeName for BarcodeHandlings. 
                        '   If this doesn't seem to be working, try adding it back.
                        If _XML.NodeReader_GetValueByNodeName(nreader, "BarcodeHandlings", value) Then
                            If "ADULT_SIGNATURE_REQUIRED" = value Then
                                ' show the disclaimer
                                Return True
                            End If
                        End If
                    End If
                End If
            End If
        Next

        're-write once Fedex_Data2XML modules are fully in place ?
    End Function

    Private Function signed_bySigPlusPad(Optional overridePickupName As Boolean = False) As Boolean
        signed_bySigPlusPad = False '' assume.

        Try
            _SigPlusPad.Signature_FileName = String.Empty ' assume.
            '
            If Not 0 = Me.txtPickedupBy.Text.Length Or overridePickupName Then
                If Me.PackageValet_TabControl.SelectedIndex = 0 OrElse (Me.lvPackages1.SelectedItems IsNot Nothing AndAlso 0 < Me.lvPackages1.Items.Count) Then
                    _SigPlusPad.Signature_FileName = String.Format("{0}\{1}.png", SignatureFolder, Date.Now.ToString("yyyyMMddhhmmssfff"))
                    _SigPlusPad.Signature_ImageType = System.Drawing.Imaging.ImageFormat.Png


                    _SigPlusPad.SigPlusPad_ShowForm()

                    If _SigPlusPad.Signature_IsSaved Then
                        signed_bySigPlusPad = True
                    Else
                        ' Canceled
                        _SigPlusPad.Signature_FileName = String.Empty
                        Me.txtCheckOutNotes.Text = String.Empty
                    End If
                    '
                Else
                    _MsgBox.InformationMessage("At least one package should be checked!", "Check a Package!")
                End If
            Else
                _MsgBox.InformationMessage("Select or type in Customer name!", "Pickedup By is Required Field!")
                Me.txtPickedupBy.Focus()
            End If

        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to load SigPlusPad form...")
        End Try
    End Function

    Private Function save_ReturnedPackage(ByVal obj As MailboxPackageObjectObservable) As Boolean
        Try
            Dim sql2cmd As New sqlUpdate
            Dim sql2exe As String = String.Empty


            ''ol#1.2.49(2/17)... 'Return to Driver' will use no calls for now, FedEx driver should scan only.
            '' ''ol#1.2.42(10/4)... FedEx HAL upload was added to Package Check In/Out forms.
            ''If "FedEx" = lvItem.Text Then
            ''    If _MailboxPackage.IsFedExWebEnabled AndAlso _MailboxPackage.IsFedExHALEnabled Then
            ''        If Not return_FedExPackage(lvItem) Then
            ''            lvItem.ForeColor = Color.Red
            ''            Return False ' exit and don't save
            ''        End If
            ''    End If
            ''End If

            If String.IsNullOrEmpty(Me.txtPickedupBy.Text) Or _Controls.Contains(Me.txtPickedupBy.Text, "driver") Then
                Call sql2cmd.Qry_UPDATE("PickedupBy", obj.CarrierName & " driver", sql2cmd.TXT_, True, False, "Mailbox_Packages", "PackageID = " & obj.ItemID)
            Else
                Call sql2cmd.Qry_UPDATE("PickedupBy", Me.txtPickedupBy.Text, sql2cmd.TXT_, True, False, "Mailbox_Packages", "PackageID = " & obj.ItemID)
            End If
            If String.IsNullOrEmpty(Me.txtCheckOutNotes.Text) Then
                Me.txtCheckOutNotes.Text = "Return to driver"
            End If
            Call sql2cmd.Qry_UPDATE("CheckOutNotes", Me.txtCheckOutNotes.Text, sql2cmd.TXT_)
            sql2exe = sql2cmd.Qry_UPDATE("PickedupDate", Date.Now.ToString, sql2cmd.TXT_, False, True)
            save_ReturnedPackage = DatabaseFunctions.IO_UpdateSQLProcessor(gMailboxDB, sql2exe)
            sql2cmd = Nothing
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Private Sub cmdAdult_Click(sender As Object, e As RoutedEventArgs) Handles cmdAdult.Click
        Try
            If isAdult Then
                If Not signed_bySigPlusPad() Then
                    txtRemarks.Text = ""
                    Disclaimer_Popup.IsOpen = False
                    Exit Sub
                End If
            End If

            txtCheckOutNotes.Text = txtRemarks.Text
            txtRemarks.Text = ""
            Disclaimer_Popup.IsOpen = False

            Call pickup_And_sign("Delivered")
        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error in Adult Disclaimer button click")
        End Try
    End Sub

    Private Sub cmdNonAdult_Click(sender As Object, e As RoutedEventArgs) Handles cmdNonAdult.Click
        Try
            isAdult = False
            txtRemarks.Text = ""
            Disclaimer_Popup.IsOpen = False
            _MsgBox.InformationMessage("You must have an adult signature to release this package!", "Adult Signature Required!")
        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error in Adult Disclaimer button click")
        End Try
    End Sub

    Private Sub cmdSigPad_Click(sender As Object, e As RoutedEventArgs) Handles cmdSigPad.Click
        Try
            HandlePackagePickupAndSign("Delivered")
        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error in Signature Pad button click")
        End Try
    End Sub

    Private Sub cmdReturnToDriver_Click(sender As Object, e As RoutedEventArgs) Handles cmdReturnToDriver.Click
        Try

            retToDriver = False

            If Me.lvPackages1.SelectedIndex > -1 AndAlso 0 < Me.lvPackages1.SelectedItems.Count Then
                retToDriver = True
                ReturnToDriver_Popup.IsOpen = True
            Else
                _MsgBox.InformationMessage("At least one package should be checked!", "Check a Package!")
            End If
        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Failed to process Package Return...")
        End Try
    End Sub

    Private Sub cmdProcessRet_Click(sender As Object, e As RoutedEventArgs) Handles cmdProcessRet.Click
        Try
            Dim count As Integer
            Dim obj As New MailboxPackageObjectObservable()

            If retToDriver Then
                count = lvPackages1.SelectedItems.Count

                If Me.lvPackages1.SelectedIndex > -1 And 0 < count Then

                    Do Until lvPackages1.SelectedItems.Count = 0
                        obj = CType(lvPackages1.SelectedItem, MailboxPackageObjectObservable)

                        txtPickedupBy.Text = "Driver"
                        txtCheckOutNotes.Text = get_SelectedReturnReason()

                        If save_ReturnedPackage(obj) Then
                            If RemoveSelectedFromList(obj.TrackingNo) Then
                                lvPackages1.ItemsSource = PackageCheckOutInfo
                            End If
                        End If
                    Loop
                End If

                If Me.lvPackages1.SelectedIndex < 0 AndAlso Me.lvPackages1.SelectedItems.Count = 0 Then
                    Call clear_Form1()
                    Me.txtPackageTrackingNo1.Focus()
                    _MsgBox.InformationMessage("Return was processed successfully!", "Saved!")
                Else
                    _MsgBox.InformationMessage("Some of the packages were not processed and not saved!", "Failed to Process!")
                End If
            End If

            retToDriver = False
            ReturnToDriver_Popup.IsOpen = False

            lblPackageCount1.Content = lvPackages1.Items.Count

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error in Return to Driver button click")
        End Try
    End Sub

    Private Function get_SelectedReturnReason() As String
        get_SelectedReturnReason = String.Empty

        If optDropOff.IsChecked Then
            Return optDropOff.Content
        ElseIf Me.optRefusedDamaged.IsChecked Then
            Return Me.optRefusedDamaged.Content
        ElseIf Me.optRefusedOther.IsChecked Then
            Return Me.optRefusedOther.Content
        ElseIf Me.optReturnToHUB.IsChecked Then
            Return Me.optReturnToHUB.Content
        End If

    End Function

    Private Sub cmdCancel_Click(sender As Object, e As RoutedEventArgs) Handles cmdCancel.Click
        Try
            txtCheckOutNotes.Text = String.Empty
            retToDriver = False
            ReturnToDriver_Popup.IsOpen = False
        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error in Return to Driver button click")
        End Try
    End Sub

    Private Sub cmdRefused_Click(sender As Object, e As RoutedEventArgs) Handles cmdRefused.Click
        Try
            If Not String.IsNullOrEmpty(Me.txtPickedupBy.Text) Then
                CustomerRefused_Popup.IsOpen = True
            Else
                _MsgBox.InformationMessage("Select or type in Customer name!", "Signed By is a Required Field!")
                Me.txtPickedupBy.Focus()
            End If
        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Failed to open Customer Refused form...")
        End Try
    End Sub

    Private Function HandlePackagePickupAndSign(reason As String) As Boolean
        ' refused and need to sign
        If _Files.Create_Folder(SignatureFolder, True) Then
            If signed_bySigPlusPad() Then
                Call pickup_And_sign(reason)
                Return True
            End If
        End If
        Return False
    End Function

    Private Sub cmdSubmit_Click(sender As Object, e As RoutedEventArgs) Handles cmdSubmit.Click
        Try

            If Not String.IsNullOrEmpty(txtReasonDesc.Text) Then
                txtCheckOutNotes.Text = String.Format("Refused: {0}", Me.txtReasonDesc.Text)
                CustomerRefused_Popup.IsOpen = False
                If Not String.IsNullOrEmpty(txtCheckOutNotes.Text) Then
                    HandlePackagePickupAndSign("Refused")
                End If
            Else
                _MsgBox.InformationMessage("Reason Description is the required field!", "Required Field!")
            End If

            lblPackageCount1.Content = lvPackages1.Items.Count
        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error in Submit button")
            Debug.Print(ex.StackTrace)
        End Try
    End Sub

    Private Sub cmdCancelRefused_Click(sender As Object, e As RoutedEventArgs) Handles cmdCancelRefused.Click
        Try
            txtCheckOutNotes.Text = String.Empty
            CustomerRefused_Popup.IsOpen = False
        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error in Cancel button")
        End Try
    End Sub

    Private Sub CustomerRefused_Checked(sender As Object, e As RoutedEventArgs) Handles optDamaged.Checked, optOrderCanceled.Checked, optLateDelivery.Checked, optOrderDuplicated.Checked, optOther.Checked, optOrderIncorrect.Checked, optWrongItem.Checked, optNotOrdered.Checked, optUnwanted.Checked, optShipperRequested.Checked
        Try
            Dim opt As RadioButton = CType(sender, RadioButton)

            If opt.IsChecked Then
                txtReasonDesc.Text = opt.Content
            End If
        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error in Cancel button")
        End Try
    End Sub

    Private Sub cmdNextTracking_Click(sender As Object, e As RoutedEventArgs) Handles cmdNextTracking.Click
        addPackageIfPossible("Add Package Button")
    End Sub

    Private Sub addPackageIfPossible(Optional calledFrom As String = "Unspecified")
        Try
            Cursor = Cursors.Wait

            If txtMailboxNo.Text.Length = 0 Then
                txtMailboxNo.Focus()
            ElseIf txtPackageTrackingNo.Text.Length = 0 Then
                txtPackageTrackingNo.Focus()
            Else
                add_Package()
                txtPackageTrackingNo.Focus()
            End If
        Catch ex As Exception
            Cursor = Cursors.Arrow
            _MsgBox.ErrorMessage(ex, String.Format("Error adding package!\nAttempt from \'{0}\'", calledFrom))
            Debug.Print(ex.StackTrace)
        Finally : Cursor = Cursors.Arrow
        End Try
    End Sub

    Private Sub cmdSearchWTracking_Click(sender As Object, e As RoutedEventArgs) Handles cmdSearchWTracking.Click
        Try
            If Not 0 = Me.txtPackageTrackingNo1.Text.Length Then
                If load_Package(Me.txtPackageTrackingNo1.Text) Then
                    If 0 < Val(Me.txtMailboxNo1.Text) Then
                        Me.txtPickedupBy.Focus()
                    End If
                End If
            End If
        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error in search with tracking number button")
        End Try
    End Sub

    Private Sub clear_Form_PI()

        Me.txtMailboxNo2.IsEnabled = False
        Me.txtMailboxNo2.Text = String.Empty
        Me.txtMailboxNo2.Tag = String.Empty
        Me.txtName2.Text = String.Empty
        Me.optMbox.IsChecked = False

        Me.optAll.IsChecked = False
        Me.optFedExHAL.IsChecked = False
        Me.optNonMbox.IsChecked = False
        Me.optUPSAP.IsChecked = False

        Me.txtPackageTrackingNo2.Text = String.Empty
        Me.txtPackageTrackingNo2.Tag = String.Empty

        Me.txtPackageTrackingNo2.Focus()
    End Sub
    Private Sub clear_List_PI()
        'Me.lvPackages2.Items.Clear() 'Clear observable info too here

        lvPackages2.ItemsSource = Nothing
        PackageInventoryInfo.Clear()


        Me.optShowBoth.IsChecked = False
        Me.optShowBoth.IsEnabled = False
        Me.optShowPresent.IsChecked = False
        Me.optShowPresent.IsEnabled = False
        Me.optShowMissing.IsChecked = False
        Me.optShowMissing.IsEnabled = False
    End Sub

    ''' <summary>
    ''' Inventory Only. Enables missing/present/both radio buttons, and selects both.
    ''' </summary>
    Private Sub init_ShowOptions()
        Me.optShowBoth.IsEnabled = False
        Me.optShowBoth.IsChecked = True

        Me.optShowBoth.IsEnabled = True
        Me.optShowPresent.IsEnabled = True
        Me.optShowMissing.IsEnabled = True

    End Sub

    Private Sub optName_Checked(sender As Object, e As RoutedEventArgs) Handles optName.Checked
        txtName2.Focus()
    End Sub

    Private Sub optName_Unchecked(sender As Object, e As RoutedEventArgs) Handles optName.Unchecked
        txtName2.Text = ""
    End Sub

    Private Sub txtName2_LostFocus(sender As Object, e As RoutedEventArgs) Handles txtName2.LostFocus
        Load_Packages_ByName()
    End Sub

    Private Sub txtName2_GotFocus(sender As Object, e As RoutedEventArgs) Handles txtName2.GotFocus
        If optName.IsChecked = False Then
            optName.IsChecked = True
        End If
    End Sub

    Private Sub txtName2_KeyDown(sender As Object, e As KeyEventArgs) Handles txtName2.KeyDown

        If e.Key = Key.Return Then
            Load_Packages_ByName()
        End If

    End Sub


    Private Sub optMbox_Checked(sender As Object, e As RoutedEventArgs) Handles optMbox.Checked, optMbox.Unchecked
        Try
            If Me.optMbox.IsChecked Then
                Me.txtMailboxNo2.IsEnabled = True
                Call clear_List_PI()
                Me.txtMailboxNo2.Focus()
                load_Packages_byMbox("")
            Else
                Me.txtMailboxNo2.IsEnabled = False
                Me.txtMailboxNo2.Text = String.Empty
                Me.txtMailboxNo2.Tag = String.Empty
            End If
        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error in selecting Mailbox Packages option")
        End Try
    End Sub

    Private Sub optFedExHAL_Checked(sender As Object, e As RoutedEventArgs) Handles optFedExHAL.Checked, optFedExHAL.Unchecked
        Try
            If Me.optFedExHAL.IsChecked Then
                If load_Packages_byClass(_MailboxPackage.FEDEX_HAL) Then
                    Call init_ShowOptions()
                End If
                Me.txtPackageTrackingNo2.Focus()
            End If
        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error in selecting FedEx HAL option")
        End Try
    End Sub

    Private Sub optUPSAP_Checked(sender As Object, e As RoutedEventArgs) Handles optUPSAP.Checked, optUPSAP.Unchecked
        Try
            If Me.optUPSAP.IsChecked Then
                If load_Packages_byClass(_MailboxPackage.UPS_AP) Then
                    Call init_ShowOptions()
                End If
                Me.txtPackageTrackingNo2.Focus()
            End If
        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error in selecting FedEx HAL option")
        End Try
    End Sub

    Private Sub optNonMbox_Checked(sender As Object, e As RoutedEventArgs) Handles optNonMbox.Checked, optNonMbox.Unchecked
        Try
            If Me.optNonMbox.IsChecked Then
                If load_Packages_byClass(_MailboxPackage.HOLD) Then
                    Call init_ShowOptions()
                End If
                Me.txtPackageTrackingNo2.Focus()
            End If
        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error in selecting Non-Mailbox Packages option")
        End Try
    End Sub

    Private Sub optAll_Checked(sender As Object, e As RoutedEventArgs) Handles optAll.Checked, optAll.Unchecked
        Try
            If Me.optAll.IsChecked Then
                If load_Packages_byClass(String.Empty) Then
                    Call init_ShowOptions()
                End If
                Me.txtPackageTrackingNo2.Focus()
            End If
        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error in selecting All Packages option")
        End Try
    End Sub

    'Private Sub optShowBoth_Checked(sender As Object, e As RoutedEventArgs) Handles optShowBoth.Checked, optShowBoth.Unchecked
    Private Sub optShowBoth_Checked(sender As Object, e As RoutedEventArgs)
        Try
            If Me.optShowBoth.IsChecked AndAlso optShowBoth.IsFocused Then
                lvPackages2.Items.Filter = Nothing
            End If
        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error in selecting All Packages option")
        End Try
    End Sub

    Private Sub optShowPresent_Checked(sender As Object, e As RoutedEventArgs) Handles optShowPresent.Checked, optShowPresent.Unchecked
        Try
            If Me.optShowPresent.IsChecked Then
                lvPackages2.Items.Filter = New Predicate(Of Object)(
                    Function(t)
                        Return t.Checked
                    End Function
                )
            End If
        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error in selecting All Packages option")
        End Try
    End Sub

    Private Sub optShowMissing_Checked(sender As Object, e As RoutedEventArgs) Handles optShowMissing.Checked, optShowMissing.Unchecked
        Try
            If Me.optShowMissing.IsChecked AndAlso optShowMissing.IsFocused Then
                lvPackages2.Items.Filter = New Predicate(Of Object)(
                    Function(t)
                        Return Not t.Checked
                    End Function
                )
            End If
        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error in selecting All Packages option")
        End Try
    End Sub

    Private Sub Load_Packages_ByName()
        Try
            Call clear_List_PI()
            Dim packagesList As String = ""
            Dim current_segment As String = ""
            If Read_Packages_By_Name(txtName2.Text, packagesList) Then

                Do Until packagesList = ""
                    current_segment = GetNextSegmentFromSet(packagesList)
                    add_ListItem2(current_segment)
                Loop
            End If
        Catch ex As Exception
            Throw ex
        End Try
    End Sub

    Private Function load_Packages_byClass(ByVal packclass As String) As Boolean
        load_Packages_byClass = False ' assume.
        Try
            Call clear_List_PI()
            Dim packagesList As String = ""
            Dim current_segment As String = ""
            If _MailboxPackagesDB.Read_Packages_ByClass(packclass, packagesList) Then

                Do Until packagesList = ""
                    current_segment = GetNextSegmentFromSet(packagesList)
                    add_ListItem2(current_segment)
                Loop

                Return True
            End If
        Catch ex As Exception
            Throw ex
        End Try
    End Function


    Private Function add_ListItem2(ByVal current_segment As String) As Boolean
        Try
            add_ListItem2 = False ' assume.
            '
            PackageInventoryInfo.Add(New MailboxPackageObjectObservable() With {
                                        .MailboxName = _Convert.Null2DefaultValue(ExtractElementFromSegment("MailboxName", current_segment)),
                                        .CarrierName = _Convert.Null2DefaultValue(ExtractElementFromSegment("CarrierName", current_segment)),
                                        .MailboxNo = _Convert.Null2DefaultValue(ExtractElementFromSegment("MailboxNo", current_segment)),
                                        .TrackingNo = _Convert.Null2DefaultValue(ExtractElementFromSegment("TrackingNo", current_segment)),
                                        .ReceivedDate = _Convert.Null2DefaultValue(ExtractElementFromSegment("ReceivedDate", current_segment)),
                                        .Location = _Convert.Null2DefaultValue(ExtractElementFromSegment("Location", current_segment)),
                                        .CheckInNotes = _Convert.Null2DefaultValue(ExtractElementFromSegment("CheckInNotes", current_segment))
                                        })

            PackageInventoryInfo = New ObservableCollection(Of MailboxPackageObjectObservable)(PackageInventoryInfo.OrderBy(Function(pkg) pkg.ReceivedDate))
            lvPackages2.ItemsSource = PackageInventoryInfo

            Return True
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Private Sub txtMailboxNo2_KeyDown(sender As Object, e As KeyEventArgs) Handles txtMailboxNo2.KeyDown
        Try
            If e.Key = Key.Return Then
                If 0 < Me.txtMailboxNo2.Text.Length Then
                    If load_Packages_byMbox(Me.txtMailboxNo2.Text) Then
                        Call init_ShowOptions()
                    End If
                Else
                    Call clear_List_PI()
                    load_Packages_byMbox("")
                End If
            End If
        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error in Mailbox number search")
        End Try
    End Sub

    Private Sub txtMailboxNo2_LostFocus(sender As Object, e As RoutedEventArgs) Handles txtMailboxNo2.LostFocus
        Try
            If load_Packages_byMbox(Me.txtMailboxNo2.Text) Then
                Call init_ShowOptions()
            End If
        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error in Mailbox number search")
        End Try
    End Sub

    Private Sub txtPackageTrackingNo2_KeyDown(sender As Object, e As KeyEventArgs) Handles txtPackageTrackingNo2.KeyDown
        Try
            If e.Key = Key.Return Then
                ' If the list of packages is empty, assume mailbox packages
                If PackageInventoryInfo.Count = 0 Then
                    Call get_PackageClass_byTrackingNo(Me.txtPackageTrackingNo2.Text)
                End If
                For Each pkg As MailboxPackageObjectObservable In PackageInventoryInfo
                    ' Invert the pkg being checked if the tracking number matches
                    pkg.Checked = pkg.Checked Xor (pkg.TrackingNo = txtPackageTrackingNo2.Text)
                Next
                ' Reset the filter, lvPackages2.Items.Refresh() wasn't doing it
                lvPackages2.Items.Filter = lvPackages2.Items.Filter
                '
                'rewrite after Fedex web modules are in place
                'Dim lvItem As ListViewItem = Nothing
                'If find_TrackingNo_inListItems(Me.txtPackageTrackingNo.Text, lvItem) Then
                '    If lvItem IsNot Nothing Then
                '        '
                '        lvItem.ForeColor = Color.Green
                '        lvItem.Checked = True
                '        lvItem.EnsureVisible()
                '        '
                '        ''ol#1.2.42(10/4)... FedEx HAL upload was added to Package Check In/Out forms.
                '        If "FedEx" = lvItem.Text Then
                '            If _MailboxPackage.IsFedExWebEnabled AndAlso _MailboxPackage.IsFedExHALEnabled Then
                '                Dim ourShipmentResponse As New baseWebResponse_Shipment
                '                Dim respack As New baseWebResponse_Package
                '                respack.PackageID = lvItem.Tag ' scanned barcode. Note: The ampersand before and after. Its required.
                '                respack.TrackingNo = lvItem.SubItems(1).Text ' we need it only to create unique xml file name
                '                ourShipmentResponse.Packages.Add(respack)
                '                If _FedExHAL.Process_TransferAcceptanceEvent_Request(ourShipmentResponse) Then
                '                    If ourShipmentResponse.ShipmentAlerts.Count > 0 Then
                '                        MessageBox.Show(respack.TrackingNo & vbCr_ & ourShipmentResponse.ShipmentAlerts(1).ToString, "FedEx Web Server", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                '                    Else
                '                        Me.lblUploadStatus.Text = "FedEx Upload - Successful !!!"
                '                        Me.lblUploadStatus.ForeColor = Color.Green
                '                    End If
                '                Else
                '                    Me.lblUploadStatus.Text = "FedEx Upload - Failed ..."
                '                    Me.lblUploadStatus.ForeColor = Color.Red
                '                End If
                '            End If
                '        End If
                '        '
                '    End If
                'End If
                'rewrite after Fedex web modules are in place

                ' Clear the tracking number field
                Me.txtPackageTrackingNo2.Text = String.Empty
            End If
        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error in tracking number search")
        End Try
    End Sub

    Private Function get_PackageClass_byTrackingNo(ByVal trackingno As String) As Boolean
        Try
            get_PackageClass_byTrackingNo = False ' assume.

            Dim packagesList As String = ""
            Dim current_segment As String = ""

            If _MailboxPackagesDB.Read_Package_ByBarcodeScan(trackingno, packagesList) OrElse _MailboxPackagesDB.Read_Package(trackingno, packagesList) Then

                'Do Until packagesList = ""
                current_segment = GetNextSegmentFromSet(packagesList)

                Dim pclass As String = _Convert.Null2DefaultValue(ExtractElementFromSegment("PackageClass", current_segment))

                Select Case pclass
                    Case _MailboxPackage.FEDEX_HAL : Me.optFedExHAL.IsChecked = True
                    Case _MailboxPackage.HOLD : Me.optNonMbox.IsChecked = True
                    Case _MailboxPackage.MBOX
                        Me.optMbox.IsChecked = True
                        Me.txtMailboxNo2.Text = _Convert.Null2DefaultValue(ExtractElementFromSegment("MailboxNo", current_segment), 0)
                        If load_Packages_byMbox(Me.txtMailboxNo.Text) Then
                            Call init_ShowOptions()
                        End If
                    Case _MailboxPackage.UPS_AP : Me.optUPSAP.IsChecked = True
                    Case Else : Me.optAll.IsChecked = True
                End Select

                'Loop

                If packagesList = "" Then
                    Return True
                End If

            End If
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    'rewrite after Fedex web modules are in place
    'Private Function find_TrackingNo_inListItems(ByVal tracknumber As String, ByRef listItem As ListViewItem) As Boolean
    '    find_TrackingNo_inListItems = False ' assume.
    '    '
    '    For Each lvItem As ListViewItem In Me.lvPackages.Items
    '        If tracknumber = lvItem.SubItems(1).Text Then
    '            listItem = lvItem
    '            Return True
    '        ElseIf tracknumber = lvItem.Tag Then
    '            listItem = lvItem
    '            Return True
    '        End If
    '    Next
    'End Function
    'rewrite after Fedex web modules are in place

    ''' <summary>
    ''' Inventory Only
    ''' </summary>
    ''' <param name="mbox"></param>
    ''' <returns></returns>
    Private Function load_Packages_byMbox(ByVal mbox As String) As Boolean
        Try
            load_Packages_byMbox = False
            Call clear_List_PI()
            '
            Dim packagesList As String = ""
            Dim current_segment As String = ""
            If _MailboxPackagesDB.Read_Packages(mbox, packagesList) Then


                Do Until packagesList = ""
                    current_segment = GetNextSegmentFromSet(packagesList)
                    load_Packages_byMbox = add_ListItem2(current_segment)
                Loop
            End If

        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Private Sub cmdClearForm_Click(sender As Object, e As RoutedEventArgs) Handles cmdClearForm.Click
        Try
            Call clear_Form_PI()
            Call clear_List_PI()
        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error in clear form button click")
        End Try
    End Sub

    Private Sub Carrier_ListBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles Carrier_ListBox.SelectionChanged
        chkAutoDetect.IsChecked = False
        If Carrier_ListBox.SelectedItem.CarrierName <> "FedEx" Then
            Me.chkFedEx_HAL.IsChecked = False
        End If

        If Carrier_ListBox.SelectedItem.CarrierName <> "UPS" Then
            Me.chkUPS_AP.IsChecked = False
        End If
    End Sub

    Private Sub chkFedEx_HAL_Click(sender As Object, e As RoutedEventArgs)
        Me.chkUPS_AP.IsChecked = False
        selectCarrier("FedEx")
    End Sub

    Private Sub chkUPS_AP_Click(sender As Object, e As RoutedEventArgs)
        Me.chkFedEx_HAL.IsChecked = False
        selectCarrier("UPS")
    End Sub

    Private Sub selectCarrier(ByVal carrier As String)
        Dim searchingCarrier As Boolean = True
        Dim i = 0
        While searchingCarrier AndAlso i < Carrier_ListBox.Items.Count
            If Carrier_ListBox.Items.Item(i).CarrierName = carrier Then
                searchingCarrier = False
                Carrier_ListBox.SelectedIndex = i
            End If
            i += 1
        End While
    End Sub

    Private Sub txtMailboxName_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs)
        OpenContactManager(txtMailboxName, txtExpDate, txtMailboxNo)
    End Sub


    'Private Function get_SelectedRefusedReason() As String
    '    get_SelectedRefusedReason = String.Empty

    '    If optDamaged.IsChecked Then
    '        Return optDamaged.Content
    '    ElseIf Me.optOrderCanceled.IsChecked Then
    '        Return Me.optOrderCanceled.Content
    '    ElseIf Me.optShipperRequested.IsChecked Then
    '        Return Me.optShipperRequested.Content
    '    ElseIf Me.optLateDelivery.IsChecked Then
    '        Return Me.optLateDelivery.Content
    '    ElseIf Me.optOrderDuplicated.IsChecked Then
    '        Return Me.optOrderDuplicated.Content
    '    ElseIf Me.optUnwanted.IsChecked Then
    '        Return Me.optUnwanted.Content
    '    ElseIf Me.optNotOrdered.IsChecked Then
    '        Return Me.optNotOrdered.Content
    '    ElseIf Me.optOrderIncorrect.IsChecked Then
    '        Return Me.optOrderIncorrect.Content
    '    ElseIf Me.optWrongItem.IsChecked Then
    '        Return Me.optWrongItem.Content
    '    ElseIf Me.optOther.IsChecked Then
    '        Return Me.optOther.Content
    '    End If
    'End Function


    Public Enum CheckInOut
        CheckIn
        CheckOut
    End Enum

    Private Sub lklAllPackagesButSigned_Click(sender As Object, e As RoutedEventArgs) Handles lklAllPackagesButSigned.Click
        Try
            Cursor = Cursors.Wait
            Dim report As New _ReportObject()
            With report
                If set_ReportHeader_ByClass(.ReportParameters, .ReportFormula) Then
                    If cmbPackageClass.SelectedItem.ToString() = _MailboxPackage.MBOX Then
                        .ReportName = "PackagesOnHand_NotSigned.rpt"
                    Else
                        .ReportName = "Packages_OnHand.rpt"
                    End If

                    If .ReportFormula.Length = 0 Then
                        .ReportFormula = "isnull({Mailbox_Packages.PickedupBy})"
                    Else
                        .ReportFormula = "isnull({Mailbox_Packages.PickedupBy}) and " & .ReportFormula
                    End If

                End If
            End With
            Dim reportPrev As New ReportPreview(report)
            Cursor = Cursors.Arrow
            ' Debug.Print(Cursor.ToString)
            reportPrev.ShowDialog()

        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to report [Packages On Hand]...")
        Finally : Cursor = Cursors.Arrow
        End Try
    End Sub

    Private Function set_ReportHeader_ByClass(ByRef params As Collection, ByRef formula As String) As Boolean
        params.Add(_MailboxPackage.StoreOwner.CompanyName)
        params.Add(_MailboxPackage.StoreOwner.Addr1)
        params.Add(_MailboxPackage.StoreOwner.CityStateZip)
        '
        ''ol#1.2.26(12/3)... Adding one day at the end of the month causing error in Mailbox and DropOff reports.
        Dim daterange As String = String.Empty
        If _MailboxPackage.MBOX = Me.cmbPackageClass.Text Then
            '
            If Me.chkDtpFrom.IsChecked And Me.chkDtpTo.IsChecked Then
                daterange = String.Format("from {0} to {1}", Me.dtpFrom.SelectedDate.Value.ToString("d"), Me.dtpTo.SelectedDate.Value.ToString("d"))
                formula = String.Format("{{Mailbox_Packages.MailboxNo}} > 0 and {{Mailbox_Packages.ReceivedDate}} in DateTime ({0}, {1}, {2}, 0, 0, 0) to DateTime ({3}, {4}, {5}, 23, 59, 59)", Me.dtpFrom.SelectedDate.Value.Year, Me.dtpFrom.SelectedDate.Value.Month, Me.dtpFrom.SelectedDate.Value.Day, Me.dtpTo.SelectedDate.Value.Year, Me.dtpTo.SelectedDate.Value.Month, Me.dtpTo.SelectedDate.Value.Day) ''ol#1.1.88(2/5)... Report from/to dates when set to the same date will not return any results.
            ElseIf Me.chkDtpFrom.IsChecked Then
                daterange = String.Format("from {0} to {1}", Me.dtpFrom.SelectedDate.Value.ToString("d"), Date.Today.Date.ToString("d"))
                formula = String.Format("{{Mailbox_Packages.MailboxNo}} > 0 and {{Mailbox_Packages.ReceivedDate}} in DateTime ({0}, {1}, {2}, 0, 0, 0) to DateTime ({3}, {4}, {5}, 23, 59, 59)", Me.dtpFrom.SelectedDate.Value.Year, Me.dtpFrom.SelectedDate.Value.Month, Me.dtpFrom.SelectedDate.Value.Day, Date.Today.Year, Date.Today.Month, Date.Today.Day)
            ElseIf Me.chkDtpTo.IsChecked Then
                daterange = String.Format("from the beginning To {0}", Me.dtpTo.SelectedDate.Value.ToString("d"))
                formula = String.Format("{{Mailbox_Packages.MailboxNo}} > 0 And {{Mailbox_Packages.ReceivedDate}} In DateTime (2014, 11, 01, 0, 0, 0) To DateTime ({0}, {1}, {2}, 23, 59, 59)", Me.dtpTo.SelectedDate.Value.Year, Me.dtpTo.SelectedDate.Value.Month, Me.dtpTo.SelectedDate.Value.Day) ''ol#1.1.88(2/5)... Report from/to dates when set to the same date will not return any results.
            Else
                daterange = "With no Date restrictions"
                formula = "{Mailbox_Packages.MailboxNo} > 0"
            End If
            '
        Else
            '
            Dim allclass As String = "Select Package Class"
            If Me.chkDtpFrom.IsChecked And Me.chkDtpTo.IsChecked Then
                daterange = String.Format("from {0} To {1}", Me.dtpFrom.SelectedDate.Value.ToString("d"), Me.dtpTo.SelectedDate.Value.ToString("d"))
                If allclass = Me.cmbPackageClass.Text Then
                    formula = String.Format("{{Mailbox_Packages.MailboxNo}} = 0 And {{Mailbox_Packages.ReceivedDate}} In DateTime ({0}, {1}, {2}, 0, 0, 0) To DateTime ({3}, {4}, {5}, 23, 59, 59)", Me.dtpFrom.SelectedDate.Value.Year, Me.dtpFrom.SelectedDate.Value.Month, Me.dtpFrom.SelectedDate.Value.Day, Me.dtpTo.SelectedDate.Value.Year, Me.dtpTo.SelectedDate.Value.Month, Me.dtpTo.SelectedDate.Value.Day, Me.cmbPackageClass.Text) ''ol#1.1.88(2/5)... Report from/to dates when set to the same date will not return any results.
                Else
                    formula = String.Format("{{Mailbox_Packages.MailboxNo}} = 0 And {{Mailbox_Packages.PackageClass}} = '{6}' and {{Mailbox_Packages.ReceivedDate}} in DateTime ({0}, {1}, {2}, 0, 0, 0) to DateTime ({3}, {4}, {5}, 23, 59, 59)", Me.dtpFrom.SelectedDate.Value.Year, Me.dtpFrom.SelectedDate.Value.Month, Me.dtpFrom.SelectedDate.Value.Day, Me.dtpTo.SelectedDate.Value.Year, Me.dtpTo.SelectedDate.Value.Month, Me.dtpTo.SelectedDate.Value.Day, Me.cmbPackageClass.Text) ''ol#1.1.88(2/5)... Report from/to dates when set to the same date will not return any results.
                End If
            Else
                If Me.chkDtpFrom.IsChecked Then
                    daterange = String.Format("from {0} to {1}", Me.dtpFrom.SelectedDate.Value.ToString("d"), Date.Today.Date.ToString("d"))
                    If allclass = Me.cmbPackageClass.Text Then
                        formula = String.Format("{{Mailbox_Packages.MailboxNo}} = 0 and {{Mailbox_Packages.ReceivedDate}} in DateTime ({0}, {1}, {2}, 0, 0, 0) to DateTime ({3}, {4}, {5}, 23, 59, 59)", Me.dtpFrom.SelectedDate.Value.Year, Me.dtpFrom.SelectedDate.Value.Month, Me.dtpFrom.SelectedDate.Value.Day, Date.Today.Year, Date.Today.Month, Date.Today.Day, Me.cmbPackageClass.Text)
                    Else
                        formula = String.Format("{{Mailbox_Packages.MailboxNo}} = 0 and {{Mailbox_Packages.PackageClass}} = '{6}' and {{Mailbox_Packages.ReceivedDate}} in DateTime ({0}, {1}, {2}, 0, 0, 0) to DateTime ({3}, {4}, {5}, 23, 59, 59)", Me.dtpFrom.SelectedDate.Value.Year, Me.dtpFrom.SelectedDate.Value.Month, Me.dtpFrom.SelectedDate.Value.Day, Date.Today.Year, Date.Today.Month, Date.Today.Day, Me.cmbPackageClass.Text)
                    End If
                ElseIf Me.chkDtpTo.IsChecked Then
                    daterange = String.Format("from the beginning to {0}", Me.dtpTo.SelectedDate.Value.ToString("d"))
                    If allclass = Me.cmbPackageClass.Text Then
                        formula = String.Format("{{Mailbox_Packages.MailboxNo}} = 0 and {{Mailbox_Packages.ReceivedDate}} in DateTime (2014, 11, 01, 0, 0, 0) to DateTime ({0}, {1}, {2}, 23, 59, 59)", Me.dtpTo.SelectedDate.Value.Year, Me.dtpTo.SelectedDate.Value.Month, Me.dtpTo.SelectedDate.Value.Day, Me.cmbPackageClass.Text) ''ol#1.1.88(2/5)... Report from/to dates when set to the same date will not return any results.
                    Else
                        formula = String.Format("{{Mailbox_Packages.MailboxNo}} = 0 and {{Mailbox_Packages.PackageClass}} = '{3}' and {{Mailbox_Packages.ReceivedDate}} in DateTime (2014, 11, 01, 0, 0, 0) to DateTime ({0}, {1}, {2}, 23, 59, 59)", Me.dtpTo.SelectedDate.Value.Year, Me.dtpTo.SelectedDate.Value.Month, Me.dtpTo.SelectedDate.Value.Day, Me.cmbPackageClass.Text) ''ol#1.1.88(2/5)... Report from/to dates when set to the same date will not return any results.
                    End If
                Else
                    daterange = "with no date restrictions"
                    If allclass = Me.cmbPackageClass.Text Then
                        formula = "{Mailbox_Packages.MailboxNo} = 0"
                    Else
                        formula = "{Mailbox_Packages.MailboxNo} = 0 and {Mailbox_Packages.PackageClass} = '" & Me.cmbPackageClass.Text & "'"
                    End If
                End If
            End If
            '
        End If
        params.Add(daterange)
        '
        Return True
    End Function

    Private Sub lklAllPackagesSigned_Click(sender As Object, e As RoutedEventArgs) Handles lklAllPackagesSigned.Click
        Try
            Cursor = Cursors.Wait
            Dim report As New _ReportObject()
            With report
                If set_ReportHeader_ByClass_CheckOut(.ReportParameters, .ReportFormula) Then
                    If cmbPackageClass.SelectedItem.ToString() = _MailboxPackage.MBOX Then
                        .ReportName = "PackagesOnHand_Signed.rpt"
                    Else
                        .ReportName = "Packages_CheckOut.rpt"
                    End If

                    If .ReportFormula.Length = 0 Then
                        .ReportFormula = "not isnull({Mailbox_Packages.PickedupBy})"
                    Else
                        .ReportFormula = "not isnull({Mailbox_Packages.PickedupBy}) and " & .ReportFormula
                    End If

                End If
            End With
            Dim reportPrev As New ReportPreview(report)
            Cursor = Cursors.Arrow
            reportPrev.ShowDialog()
        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to report [Checked Out]...")
        Finally : Cursor = Cursors.Arrow
        End Try
    End Sub
    Private Function set_ReportHeader_ByClass_CheckOut(ByRef params As Collection, ByRef formula As String) As Boolean
        params.Add(_MailboxPackage.StoreOwner.CompanyName)
        params.Add(_MailboxPackage.StoreOwner.Addr1)
        params.Add(_MailboxPackage.StoreOwner.CityStateZip)
        '
        ''ol#1.2.26(12/3)... Adding one day at the end of the month causing error in Mailbox and DropOff reports.
        Dim daterange As String = String.Empty
        If _MailboxPackage.MBOX = Me.cmbPackageClass.Text Then
            '
            If Me.chkDtpFrom.IsChecked And Me.chkDtpTo.IsChecked Then
                daterange = String.Format("from {0} to {1}", Me.dtpFrom.SelectedDate.Value.Date.ToString("d"), Me.dtpTo.SelectedDate.Value.Date.ToString("d"))
                formula = String.Format("{{Mailbox_Packages.MailboxNo}} > 0 and {{Mailbox_Packages.PickedupDate}} in DateTime ({0}, {1}, {2}, 0, 0, 0) to DateTime ({3}, {4}, {5}, 23, 59, 59)", Me.dtpFrom.SelectedDate.Value.Year, Me.dtpFrom.SelectedDate.Value.Month, Me.dtpFrom.SelectedDate.Value.Day, Me.dtpTo.SelectedDate.Value.Year, Me.dtpTo.SelectedDate.Value.Month, Me.dtpTo.SelectedDate.Value.Day) ''ol#1.1.88(2/5)... Report from/to dates when set to the same date will not return any results.
            Else
                If Me.chkDtpFrom.IsChecked Then
                    daterange = String.Format("from {0} to {1}", Me.dtpFrom.SelectedDate.Value.Date.ToString("d"), Date.Today.Date.ToString("d"))
                    formula = String.Format("{{Mailbox_Packages.MailboxNo}} > 0 and {{Mailbox_Packages.PickedupDate}} in DateTime ({0}, {1}, {2}, 0, 0, 0) to DateTime ({3}, {4}, {5}, 23, 59, 59)", Me.dtpFrom.SelectedDate.Value.Year, Me.dtpFrom.SelectedDate.Value.Month, Me.dtpFrom.SelectedDate.Value.Day, Date.Today.Year, Date.Today.Month, Date.Today.Day)
                ElseIf Me.chkDtpTo.IsChecked Then
                    daterange = String.Format("from the beginning to {0}", Me.dtpTo.SelectedDate.Value.Date.ToString("d"))
                    formula = String.Format("{{Mailbox_Packages.MailboxNo}} > 0 and {{Mailbox_Packages.PickedupDate}} in DateTime (2014, 11, 01, 0, 0, 0) to DateTime ({0}, {1}, {2}, 23, 59, 59)", Me.dtpTo.SelectedDate.Value.Year, Me.dtpTo.SelectedDate.Value.Month, Me.dtpTo.SelectedDate.Value.Day) ''ol#1.1.88(2/5)... Report from/to dates when set to the same date will not return any results.
                Else
                    daterange = "with no date restrictions"
                    formula = "{Mailbox_Packages.MailboxNo} > 0"
                End If
            End If
            '
        Else
            '
            Dim allclass As String = "Select Package Class"
            If Me.chkDtpFrom.IsChecked And Me.chkDtpTo.IsChecked Then
                daterange = String.Format("from {0} to {1}", Me.dtpFrom.SelectedDate.Value.Date.ToString("d"), Me.dtpTo.SelectedDate.Value.Date.ToString("d"))
                If allclass = Me.cmbPackageClass.Text Then
                    formula = String.Format("{{Mailbox_Packages.MailboxNo}} = 0 and {{Mailbox_Packages.PickedupDate}} in DateTime ({0}, {1}, {2}, 0, 0, 0) to DateTime ({3}, {4}, {5}, 23, 59, 59)", Me.dtpFrom.SelectedDate.Value.Year, Me.dtpFrom.SelectedDate.Value.Month, Me.dtpFrom.SelectedDate.Value.Day, Me.dtpTo.SelectedDate.Value.Year, Me.dtpTo.SelectedDate.Value.Month, Me.dtpTo.SelectedDate.Value.Day, Me.cmbPackageClass.Text) ''ol#1.1.88(2/5)... Report from/to dates when set to the same date will not return any results.
                Else
                    formula = String.Format("{{Mailbox_Packages.MailboxNo}} = 0 and {{Mailbox_Packages.PackageClass}} = '{6}' and {{Mailbox_Packages.PickedupDate}} in DateTime ({0}, {1}, {2}, 0, 0, 0) to DateTime ({3}, {4}, {5}, 23, 59, 59)", Me.dtpFrom.SelectedDate.Value.Year, Me.dtpFrom.SelectedDate.Value.Month, Me.dtpFrom.SelectedDate.Value.Day, Me.dtpTo.SelectedDate.Value.Year, Me.dtpTo.SelectedDate.Value.Month, Me.dtpTo.SelectedDate.Value.Day, Me.cmbPackageClass.Text) ''ol#1.1.88(2/5)... Report from/to dates when set to the same date will not return any results.
                End If
            Else
                If Me.chkDtpFrom.IsChecked Then
                    daterange = String.Format("from {0} to {1}", Me.dtpFrom.SelectedDate.Value.Date.ToString("d"), Date.Today.Date.ToString("d"))
                    If allclass = Me.cmbPackageClass.Text Then
                        formula = String.Format("{{Mailbox_Packages.MailboxNo}} = 0 and {{Mailbox_Packages.PickedupDate}} in DateTime ({0}, {1}, {2}, 0, 0, 0) to DateTime ({3}, {4}, {5}, 23, 59, 59)", Me.dtpFrom.SelectedDate.Value.Year, Me.dtpFrom.SelectedDate.Value.Month, Me.dtpFrom.SelectedDate.Value.Day, Date.Today.Year, Date.Today.Month, Date.Today.Day, Me.cmbPackageClass.Text)
                    Else
                        formula = String.Format("{{Mailbox_Packages.MailboxNo}} = 0 and {{Mailbox_Packages.PackageClass}} = '{6}' and {{Mailbox_Packages.PickedupDate}} in DateTime ({0}, {1}, {2}, 0, 0, 0) to DateTime ({3}, {4}, {5}, 23, 59, 59)", Me.dtpFrom.SelectedDate.Value.Year, Me.dtpFrom.SelectedDate.Value.Month, Me.dtpFrom.SelectedDate.Value.Day, Date.Today.Year, Date.Today.Month, Date.Today.Day, Me.cmbPackageClass.Text)
                    End If
                ElseIf Me.chkDtpTo.IsChecked Then
                    daterange = String.Format("from the beginning to {0}", Me.dtpTo.SelectedDate.Value.Date.ToString("d"))
                    If allclass = Me.cmbPackageClass.Text Then
                        formula = String.Format("{{Mailbox_Packages.MailboxNo}} = 0 and {{Mailbox_Packages.PickedupDate}} in DateTime (2014, 11, 01, 0, 0, 0) to DateTime ({0}, {1}, {2}, 23, 59, 59)", Me.dtpTo.SelectedDate.Value.Year, Me.dtpTo.SelectedDate.Value.Month, Me.dtpTo.SelectedDate.Value.Day, Me.cmbPackageClass.Text) ''ol#1.1.88(2/5)... Report from/to dates when set to the same date will not return any results.
                    Else
                        formula = String.Format("{{Mailbox_Packages.MailboxNo}} = 0 and {{Mailbox_Packages.PackageClass}} = '{3}' and {{Mailbox_Packages.PickedupDate}} in DateTime (2014, 11, 01, 0, 0, 0) to DateTime ({0}, {1}, {2}, 23, 59, 59)", Me.dtpTo.SelectedDate.Value.Year, Me.dtpTo.SelectedDate.Value.Month, Me.dtpTo.SelectedDate.Value.Day, Me.cmbPackageClass.Text) ''ol#1.1.88(2/5)... Report from/to dates when set to the same date will not return any results.
                    End If
                Else
                    daterange = "with no date restrictions"
                    If allclass = Me.cmbPackageClass.Text Then
                        formula = "{Mailbox_Packages.MailboxNo} = 0"
                    Else
                        formula = "{Mailbox_Packages.MailboxNo} = 0 and {Mailbox_Packages.PackageClass} = '" & Me.cmbPackageClass.Text & "'"
                    End If
                End If
            End If
            '
        End If
        params.Add(daterange)
        '
        Return True
    End Function

    Private Sub lklMailboxPackageHistory_Click(sender As Object, e As RoutedEventArgs) Handles lklMailboxPackageHistory.Click
        Try
            Cursor = Cursors.Wait
            Dim report As New _ReportObject()
            With report
                If set_ReportHeader_ByMBox(.ReportParameters, .ReportFormula) Then
                    .ReportName = "PackagesHistory.rpt"
                    Dim mBoxParam As String = "for all mailboxes"
                    If Me.txtMailboxNo3.Text.Length > 0 Then
                        mBoxParam = "for mailbox #" & txtMailboxNo3.Text
                        If .ReportFormula.Length = 0 Then
                            .ReportFormula = "{Mailbox_Packages.MailboxNo} = " & txtMailboxNo3.Text
                        Else
                            .ReportFormula = "{Mailbox_Packages.MailboxNo} = " & txtMailboxNo3.Text & " and " & .ReportFormula
                        End If
                    End If
                    .ReportParameters.Add(mBoxParam)
                End If
            End With
            Dim reportPrev As New ReportPreview(report)
            Cursor = Cursors.Arrow
            reportPrev.ShowDialog()
        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to report [Checked Out]...")
        Finally : Cursor = Cursors.Arrow
        End Try
    End Sub

    Private Function set_ReportHeader_ByMBox(ByRef params As Collection, ByRef formula As String) As Boolean
        params.Add(_MailboxPackage.StoreOwner.CompanyName)
        params.Add(_MailboxPackage.StoreOwner.Addr1)
        params.Add(_MailboxPackage.StoreOwner.CityStateZip)
        '
        ''ol#1.2.26(12/3)... Adding one day at the end of the month causing error in Mailbox and DropOff reports.
        Dim daterange As String = String.Empty
        If Me.chkDtpFrom.IsChecked And Me.chkDtpTo.IsChecked Then
            daterange = String.Format("from {0} to {1}", Me.dtpFrom.SelectedDate.Value.Date.ToString("d"), Me.dtpTo.SelectedDate.Value.Date.ToString("d"))
            formula = String.Format("{{Mailbox_Packages.MailboxNo}} > 0 and {{Mailbox_Packages.ReceivedDate}} in DateTime ({0}, {1}, {2}, 0, 0, 0) to DateTime ({3}, {4}, {5}, 23, 59, 59)", Me.dtpFrom.SelectedDate.Value.Year, Me.dtpFrom.SelectedDate.Value.Month, Me.dtpFrom.SelectedDate.Value.Day, Me.dtpTo.SelectedDate.Value.Year, Me.dtpTo.SelectedDate.Value.Month, Me.dtpTo.SelectedDate.Value.Day) ''ol#1.1.88(2/5)... Report from/to dates when set to the same date will not return any results.
        Else
            If Me.chkDtpFrom.IsChecked Then
                daterange = String.Format("from {0} to {1}", Me.dtpFrom.SelectedDate.Value.Date.ToString("d"), Date.Today.Date.ToString("d"))
                formula = String.Format("{{Mailbox_Packages.MailboxNo}} > 0 and {{Mailbox_Packages.ReceivedDate}} in DateTime ({0}, {1}, {2}, 0, 0, 0) to DateTime ({3}, {4}, {5}, 23, 59, 59)", Me.dtpFrom.SelectedDate.Value.Year, Me.dtpFrom.SelectedDate.Value.Month, Me.dtpFrom.SelectedDate.Value.Day, Date.Today.Year, Date.Today.Month, Date.Today.Day)
            ElseIf Me.chkDtpTo.IsChecked Then
                daterange = String.Format("from the beginning to {0}", Me.dtpTo.SelectedDate.Value.Date.ToString("d"))
                formula = String.Format("{{Mailbox_Packages.MailboxNo}} > 0 and {{Mailbox_Packages.ReceivedDate}} in DateTime (2014, 11, 01, 0, 0, 0) to DateTime ({0}, {1}, {2}, 23, 59, 59)", Me.dtpTo.SelectedDate.Value.Year, Me.dtpTo.SelectedDate.Value.Month, Me.dtpTo.SelectedDate.Value.Day) ''ol#1.1.88(2/5)... Report from/to dates when set to the same date will not return any results.
            Else
                daterange = "with no date restrictions"
                formula = "{Mailbox_Packages.MailboxNo} > 0"
            End If
        End If
        params.Add(daterange)
        '
        Return True
    End Function

    Private Sub cmdPkgRep_LoadNames_Click(sender As Object, e As RoutedEventArgs) Handles cmdPkgRep_LoadNames.Click
        Cursor = Cursors.Wait

        Dim namesSegment As String = ""
        cmbCustomerName.Items.Clear()
        If _MailboxPackagesDB.Read_MailboxNames_All(namesSegment) Then
            Dim name As String
            While namesSegment.Length > 0
                name = SegmentFunctions.GetNextSegmentFromSet(namesSegment)
                name = SegmentFunctions.ExtractElementFromSegment("MailboxName", name)
                If name.Length > 0 Then
                    cmbCustomerName.Items.Add(name)
                End If
            End While
        End If

        Cursor = Cursors.Arrow
    End Sub
    Private Sub lklMailboxPackageHistoryByCustomer_Click(sender As Object, e As RoutedEventArgs) Handles lklMailboxPackageHistoryByCustomer.Click
        Try
            Cursor = Cursors.Wait
            Dim report As New _ReportObject()
            With report
                If set_ReportHeader(.ReportParameters, .ReportFormula) Then
                    .ReportName = "PackagesHistory_ByCustomer.rpt"
                    Dim mBoxParam As String = ""
                    If Me.cmbCustomerName.Text <> "" Then
                        'If .ReportFormula.Length = 0 Then
                        mBoxParam = "for customer: " & cmbCustomerName.Text
                            If .ReportFormula.Length = 0 Then
                                .ReportFormula = "{Mailbox_Packages.MailboxName} = '" & cmbCustomerName.Text & "'"
                            Else
                                .ReportFormula = String.Format("{{Mailbox_Packages.MailboxName}} = '{0}' and {1}", cmbCustomerName.Text, .ReportFormula)
                            End If
                        'End If
                    End If
                    .ReportParameters.Add(mBoxParam)
                End If
            End With
            Dim reportPrev As New ReportPreview(report)
            Cursor = Cursors.Arrow
            reportPrev.ShowDialog()
        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to report [Checked Out]...")
        Finally : Cursor = Cursors.Arrow
        End Try
    End Sub
    Private Function set_ReportHeader(ByRef params As Collection, ByRef formula As String) As Boolean
        params.Add(_MailboxPackage.StoreOwner.CompanyName)
        params.Add(_MailboxPackage.StoreOwner.Addr1)
        params.Add(_MailboxPackage.StoreOwner.CityStateZip)
        '
        ''ol#1.2.26(12/3)... Adding one day at the end of the month causing error in Mailbox and DropOff reports.
        Dim daterange As String = String.Empty
        If Me.chkDtpFrom.IsChecked And Me.chkDtpTo.IsChecked Then
            daterange = String.Format("from {0} to {1}", Me.dtpFrom.SelectedDate.Value.Date.ToString("d"), Me.dtpTo.SelectedDate.Value.Date.ToString("d"))
            formula = String.Format("{{Mailbox_Packages.ReceivedDate}} in DateTime ({0}, {1}, {2}, 0, 0, 0) to DateTime ({3}, {4}, {5}, 23, 59, 59)", Me.dtpFrom.SelectedDate.Value.Year, Me.dtpFrom.SelectedDate.Value.Month, Me.dtpFrom.SelectedDate.Value.Day, Me.dtpTo.SelectedDate.Value.Year, Me.dtpTo.SelectedDate.Value.Month, Me.dtpTo.SelectedDate.Value.Day) ''ol#1.1.88(2/5)... Report from/to dates when set to the same date will not return any results.
        Else
            If Me.chkDtpFrom.IsChecked Then
                daterange = String.Format("from {0} to {1}", Me.dtpFrom.SelectedDate.Value.Date.ToString("d"), Date.Today.Date.ToString("d"))
                formula = String.Format("{{Mailbox_Packages.ReceivedDate}} in DateTime ({0}, {1}, {2}, 0, 0, 0) to DateTime ({3}, {4}, {5}, 23, 59, 59)", Me.dtpFrom.SelectedDate.Value.Year, Me.dtpFrom.SelectedDate.Value.Month, Me.dtpFrom.SelectedDate.Value.Day, Date.Today.Year, Date.Today.Month, Date.Today.Day)
            ElseIf Me.chkDtpTo.IsChecked Then
                daterange = String.Format("from the beginning to {0}", Me.dtpTo.SelectedDate.Value.Date.ToString("d"))
                formula = String.Format("{{Mailbox_Packages.ReceivedDate}} in DateTime (2014, 11, 01, 0, 0, 0) to DateTime ({0}, {1}, {2}, 23, 59, 59)", Me.dtpTo.SelectedDate.Value.Year, Me.dtpTo.SelectedDate.Value.Month, Me.dtpTo.SelectedDate.Value.Day) ''ol#1.1.88(2/5)... Report from/to dates when set to the same date will not return any results.
            Else
                daterange = "with no date restrictions"
                formula = ""
            End If
        End If
        params.Add(daterange)
        '
        Return True
    End Function

    Private Sub lklPOD_byCarrier_Click(sender As Object, e As RoutedEventArgs) Handles lklPOD_byCarrier.Click
        Try
            Cursor = Cursors.Wait
            Dim report As New _ReportObject()


            With report
                If set_ReportHeader_ByPOD(.ReportParameters, .ReportFormula) Then
                    .ReportName = "ProofOfPickup_SignatureSheet.rpt"

                    If cmdCarrierName.Text.Contains("FedEx") Then
                        'FedEx
                        Dim isGround As Boolean = ("FedEx Ground" = cmdCarrierName.Text)
                        Dim CarrierName As String = "FedEx"

                        If .ReportFormula = "" Then
                            .ReportFormula = "{Mailbox_Packages.PickedupBy} <> '' and {Mailbox_Packages.CarrierName} = '" & CarrierName & "' and {Mailbox_Packages.IsGround}=" & isGround
                        Else
                            .ReportFormula = String.Format("{{Mailbox_Packages.PickedupBy}} <> '' and {0} and {{Mailbox_Packages.CarrierName}} = '{1}' and {{Mailbox_Packages.IsGround}}={2}", .ReportFormula, CarrierName, isGround)
                        End If


                    Else
                        'All Other Carriers
                        If .ReportFormula = "" Then
                            .ReportFormula = "{Mailbox_Packages.PickedupBy} <> '' and {Mailbox_Packages.CarrierName} = '" & cmdCarrierName.Text & "'"
                        Else
                            .ReportFormula = String.Format("{{Mailbox_Packages.PickedupBy}} <> '' and {0} and {{Mailbox_Packages.CarrierName}} = '{1}'", .ReportFormula, cmdCarrierName.Text)
                        End If

                    End If


                End If
            End With
            Dim reportPrev As New ReportPreview(report)
            Cursor = Cursors.Arrow
            reportPrev.ShowDialog()
        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to report [Checked Out]...")
        Finally : Cursor = Cursors.Arrow
        End Try
    End Sub
    Private Sub lklPOD_byDateRange_Click(sender As Object, e As RoutedEventArgs) Handles lklPOD_byDateRange.Click
        Try
            Cursor = Cursors.Wait
            Dim report As New _ReportObject()
            With report
                If set_ReportHeader_ByPOD(.ReportParameters, .ReportFormula) Then
                    .ReportName = "ProofOfPickup_SignatureSheet.rpt"

                    If .ReportFormula = "" Then
                        .ReportFormula = "{Mailbox_Packages.PickedupBy} <> ''"
                    Else
                        .ReportFormula = "{Mailbox_Packages.PickedupBy} <> ''  and " & .ReportFormula
                    End If
                End If
            End With
            Dim reportPrev As New ReportPreview(report)
            Cursor = Cursors.Arrow
            reportPrev.ShowDialog()
        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to report [Checked Out]...")
        Finally : Cursor = Cursors.Arrow
        End Try
    End Sub
    Private Sub lklPOD_byTrackingNo_Click(sender As Object, e As RoutedEventArgs) Handles lklPOD_byTrackingNo.Click
        Try
            Cursor = Cursors.Wait
            Dim report As New _ReportObject()
            With report
                If set_ReportHeader_ByPOD(.ReportParameters, .ReportFormula) Then
                    .ReportName = "ProofOfPickup_SignatureSheet.rpt"

                    If .ReportFormula = "" Then
                        .ReportFormula = String.Format("{{Mailbox_Packages.PickedupBy}} <> '' and ({{Mailbox_Packages.TrackingNo}} = '{0}' or {{Mailbox_Packages.BarCodeScan}} = '{0}')", txtPackageTrackingNo3.Text)
                    Else
                        .ReportFormula = String.Format("{{Mailbox_Packages.PickedupBy}} <> '' and {0} and ({{Mailbox_Packages.TrackingNo}} = '{1}' or {{Mailbox_Packages.BarCodeScan}} = '{1}')", .ReportFormula, txtPackageTrackingNo3.Text)
                    End If
                End If
            End With
            Dim reportPrev As New ReportPreview(report)
            Cursor = Cursors.Arrow
            reportPrev.ShowDialog()
        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to report [Checked Out]...")
        Finally : Cursor = Cursors.Arrow
        End Try
    End Sub

    Private Sub cmdPOD_LoadNames_Click(sender As Object, e As RoutedEventArgs) Handles cmdPOD_LoadNames.Click
        Cursor = Cursors.Wait

        Dim namesSegment As String = ""
        cmbCustomerName.Items.Clear()
        If _MailboxPackagesDB.Read_PickupNames_All(namesSegment) Then
            Dim name As String
            While namesSegment.Length > 0
                name = SegmentFunctions.GetNextSegmentFromSet(namesSegment)
                name = SegmentFunctions.ExtractElementFromSegment("PickedupBy", name)
                If name.Length > 0 Then
                    cmbPOD_byCustomer.Items.Add(name)
                End If
            End While
        End If

        Cursor = Cursors.Arrow
    End Sub
    Private Sub lklPOD_byPickupPerson_Click(sender As Object, e As RoutedEventArgs) Handles lklPOD_byPickupPerson.Click
        Try
            Cursor = Cursors.Wait
            Dim report As New _ReportObject()
            Debug.Print(cmbPOD_byCustomer.Text)
            With report
                If set_ReportHeader_ByPOD(.ReportParameters, .ReportFormula) Then
                    .ReportName = "ProofOfPickup_SignatureSheet.rpt"

                    If .ReportFormula = "" Then
                        .ReportFormula = "{Mailbox_Packages.PickedupBy} = '" & cmbPOD_byCustomer.Text.Replace("'", "''") & "'"
                    Else
                        .ReportFormula = String.Format("{{Mailbox_Packages.PickedupBy}} = '{1}' and {0}", .ReportFormula, cmbPOD_byCustomer.Text.Replace("'", "''"))
                    End If
                End If
            End With
            Dim reportPrev As New ReportPreview(report)
            Cursor = Cursors.Arrow
            reportPrev.ShowDialog()
        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to report [POD By Pickup Person]...")
        Finally : Cursor = Cursors.Arrow
        End Try
    End Sub

    Private Function set_ReportHeader_ByPOD(ByRef params As Collection, ByRef formula As String) As Boolean
        params.Add(_MailboxPackage.StoreOwner.CompanyName)
        params.Add(_MailboxPackage.StoreOwner.Addr1)
        params.Add(_MailboxPackage.StoreOwner.CityStateZip)
        '
        ''ol#1.2.26(12/3)... Adding one day at the end of the month causing error in Mailbox and DropOff reports.
        Dim daterange As String = String.Empty
        If Me.chkDtpFrom.IsChecked And Me.chkDtpTo.IsChecked Then
            daterange = String.Format("from {0} to {1}", Me.dtpFrom.SelectedDate.Value.Date.ToString("d"), Me.dtpTo.SelectedDate.Value.Date.ToString("d"))
            formula = String.Format("{{Mailbox_Packages.PickedupDate}} in DateTime ({0}, {1}, {2}, 0, 0, 0) to DateTime ({3}, {4}, {5}, 23, 59, 59)", Me.dtpFrom.SelectedDate.Value.Year, Me.dtpFrom.SelectedDate.Value.Month, Me.dtpFrom.SelectedDate.Value.Day, Me.dtpTo.SelectedDate.Value.Year, Me.dtpTo.SelectedDate.Value.Month, Me.dtpTo.SelectedDate.Value.Day) ''ol#1.1.88(2/5)... Report from/to dates when set to the same date will not return any results.
        Else
            If Me.chkDtpFrom.IsChecked Then
                daterange = String.Format("from {0} to {1}", Me.dtpFrom.SelectedDate.Value.Date.ToString("d"), Date.Today.Date.ToString("d"))
                formula = String.Format("{{Mailbox_Packages.PickedupDate}} in DateTime ({0}, {1}, {2}, 0, 0, 0) to DateTime ({3}, {4}, {5}, 23, 59, 59)", Me.dtpFrom.SelectedDate.Value.Year, Me.dtpFrom.SelectedDate.Value.Month, Me.dtpFrom.SelectedDate.Value.Day, Date.Today.Year, Date.Today.Month, Date.Today.Day)
            ElseIf Me.chkDtpTo.IsChecked Then
                daterange = String.Format("from the beginning to {0}", Me.dtpTo.SelectedDate.Value.Date.ToString("d"))
                formula = String.Format("{{Mailbox_Packages.PickedupDate}} in DateTime (2014, 11, 01, 0, 0, 0) to DateTime ({0}, {1}, {2}, 23, 59, 59)", Me.dtpTo.SelectedDate.Value.Year, Me.dtpTo.SelectedDate.Value.Month, Me.dtpTo.SelectedDate.Value.Day) ''ol#1.1.88(2/5)... Report from/to dates when set to the same date will not return any results.
            Else
                daterange = "with no date restrictions"
                formula = ""
            End If
        End If
        params.Add(daterange)
        params.Add(String.Format("{0}\", _MailboxPackage.SignatureFolder)) ''ol#1.2.60(9/14)... 'Proof of Delivery' report signature location fix.
        '
        Return True
    End Function

    Private Sub lvPackages2_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles lvPackages2.SelectionChanged
        Dim pkg = lvPackages2.SelectedItem
        If pkg IsNot Nothing Then
            pkg.Checked = Not pkg.Checked
            lvPackages2.SelectedItems.Clear()
            lvPackages2.Items.Filter = lvPackages2.Items.Filter
        End If
    End Sub

    Private Sub PackageValet_Window_Closing(sender As Object, e As CancelEventArgs) Handles PackageValet_Window.Closing
        My.Settings.PackageValet_CheckIn_PrintNotice = chkPrintNotices.IsChecked
        My.Settings.PackageValet_CheckIn_PrintLabel = chkPackageLabel.IsChecked

        My.Settings.PackageValet_CheckIn_PrintSignatureSheet = chkSignatureSheet.IsChecked
        My.Settings.PackageValet_CheckIn_SendSMS = chkSendSMS.IsChecked
        My.Settings.PackageValet_CheckIn_SendEmail = chkSendEmails.IsChecked

        My.Settings.Save()
    End Sub

    Private Sub SetupOptions_Click(sender As Object, e As RoutedEventArgs) Handles SetupOptions.Click
        PrinterOptions.IsOpen = True
    End Sub

    Private Sub PrintNoticeReceipt_Checked(sender As Object, e As RoutedEventArgs) Handles PrintNoticeReceipt.Checked
        My.Settings.PackageValet_CheckIn_NoticePrintOption = NoticePrinter.Receipt
    End Sub

    Private Sub PrintNoticeLabel_Checked(sender As Object, e As RoutedEventArgs) Handles PrintNoticeLabel.Checked
        My.Settings.PackageValet_CheckIn_NoticePrintOption = NoticePrinter.Label
    End Sub

    Private Sub PrintNoticeBoth_Checked(sender As Object, e As RoutedEventArgs) Handles PrintNoticeBoth.Checked
        My.Settings.PackageValet_CheckIn_NoticePrintOption = NoticePrinter.Both
    End Sub

    Private Sub cmdPrintNotice_Copy_Click(sender As Object, e As RoutedEventArgs) Handles cmdPrintNotice_Copy.Click
        sendEmails(lvPackages1.SelectedItems)
    End Sub

    Private Sub ColumnHeader_Click(sender As Object, e As RoutedEventArgs)
        'Sorts ListView by clicked Column Header
        'Dim columnHeader As GridViewColumnHeader = TryCast(e.OriginalSource, GridViewColumnHeader)
        Sort_LV_byColumn(sender, TryCast(e.OriginalSource, GridViewColumnHeader), Last_Sort_Ascending, Last_Column_Sorted)

    End Sub

End Class

Public Class MailboxPackageObjectObservable
    Public Property MailboxNo As Long
    Public Property MailboxName As String
    Public Property CarrierName As String
    Public Property TrackingNo As String
    Public Property ReceivedDate As DateTime
    Public Property PickedupBy As String
    Public Property PickedupDate As Date
    ''ol#1.2.03(6/3)... Notes and Location fields are added to Print Notice body.
    Public Property CheckInNotes As String
    Public Property Location As String
    ''ol#1.2.41(7/11)... 'Hold for Pickup' will be integrated with Mailbox 'Package Check In/Out'.
    Public Property BarCodeScan As String
    Public Property CustomerID As Long
    Public Property PackageClass As String
    Public Property Email As String
    Public Property SMS As String
    Public Property smsCarrier As String
    Public Property SignatureFile As String
    Public Property IsGround As Boolean
    Public Property CheckOutNotes As String
    Public Property ItemID As String
    Public Property ItemIndex As String
    Public Property ItemText As String
    Public Property FColor As System.Windows.Media.SolidColorBrush = Media.Brushes.Black
    Public Property Checked As Boolean
End Class

Public Class StatusLabel
    Public Property text As String = "Initial Text"
    Public Property FColor As System.Windows.Media.SolidColorBrush = Media.Brushes.Black
End Class