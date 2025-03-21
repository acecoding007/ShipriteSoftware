Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Net.Mail
Imports System.Windows.Media.Brushes
Imports System.Data


Public Class DropOffManager
    Inherits CommonWindow

    Public Property DropOffInfo As ObservableCollection(Of DropOffInformation)
    Private Property userWantsToSelectCarrier As Boolean
    Private Property compensations As New List(Of DropOffCompensationObject)

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

    Private Sub Read_CustomerNames()
        Dim SegmentSet As String
        Dim Segment As String
        Dim sql2exe As String = "Select Distinct CustomerName " &
                                "From DropOff_Packages " &
                                "Order by CustomerName"
        SegmentSet = IO_GetSegmentSet(gDropOffDB, sql2exe)
        Do Until SegmentSet = ""
            Segment = GetNextSegmentFromSet(SegmentSet)
            cmbCustomerName.Items.Add(ExtractElementFromSegment("CustomerName", Segment))
        Loop


    End Sub

    Private Overloads Sub RefreshButton_Click(sender As Object, e As RoutedEventArgs)
        clear_Form(True, True)
    End Sub

    Private Sub clear_Form(ByVal isCustomerNameAlso As Boolean, ByVal isPackagingFeeAlso As Boolean)
        If isCustomerNameAlso Then
            txtCustomerName.Text = String.Empty
            gContactManagerSegment = ""
        End If
        Me.txtDesc.Text = String.Empty
        If isPackagingFeeAlso Then
            Me.txtPackagingFee.Text = "0.00"
            Me.chkPackagingFee.IsChecked = False
        End If
        Me.txtPackageTrackingNo.Text = String.Empty
        Me.lblEmailSendStatus.Content = String.Empty
        'Me.lblSMSSendStatus.Content = String.Empty
        Me.lblUPSUploadStatus.Content = String.Empty
        userWantsToSelectCarrier = False

    End Sub

    Private Sub clear_List()
        lvPackages.ItemsSource = Nothing
    End Sub

    Private Sub DropOffManager_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Try
            Load_Carrier_Options()
            DropOffInfo = New ObservableCollection(Of DropOffInformation)() 'temp location, need to apply elsewhere too

            DropOffPackagesDB.path2db = gDropOffDB

            Call clear_Form(True, True)
            Call clear_List()
            Call read_MySettings()
            Call load_CompensationValues()

            If "Canada" = _DropOff.StoreOwner.Country Then
                'Code for Canada post goes here      
            End If

            'Insert code for other carrier name in compensation setup form

            If Not String.IsNullOrEmpty(gCustomerSegment) Then
                LoadAddressFromSegment(gCustomerSegment)
            End If

            If 0 = Me.txtCustomerName.Text.Length AndAlso _DropOff.CustomerObject IsNot Nothing AndAlso Not _DropOff.CustomerObject.ContactID = 0 Then
                Me.txtCustomerName.Text = _DropOff.CustomerObject.CompanyName
                Me.txtCustomerName.Tag = _DropOff.CustomerObject
                Me.txtPackageTrackingNo.Focus()
            Else
                CheckFocus()
            End If

            printCopyCount.Text = GetPolicyData(gShipriteDB, "DropOffCopyCount", 1)

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Failed to load Drop Off Manager")
        End Try
    End Sub

    Private Sub CheckFocus()
        If My.Settings.DropOff_FocusTracking = True Then
            txtPackageTrackingNo.Focus()
        Else
            txtCustomerName.Focus()
        End If
    End Sub

    Private Sub read_MySettings()
        ''AP(05/30/2017) - Reload previously saved form settings after updating to new version.

        'Insert upgrade code here for WPF

        'If My.Settings.UpgradeRequired Then
        '    My.Settings.Upgrade()
        '    My.Settings.UpgradeRequired = False
        'End If
        'Insert upgrade code here for WPF

        Me.chkPrintReceipt.IsChecked = My.Settings.DropOff_chkPrintReceipt
        Me.chkSendEmails.IsChecked = My.Settings.DropOff_chkSendEmails
        'Me.chkAutoDetect.IsChecked = My.Settings.DropOff_chkAutoDetect
        'Me.chkSendSMS.IsChecked = My.Settings.DropOff_chkSendSMS

    End Sub

    Private Sub load_CompensationValues()
        compensations.Clear()
        Dim SQL As String = "Select * from DropOff_Compensation"
        Dim SegmentSet As String = IO_GetSegmentSet(gDropOffDB, SQL)
        Dim Segment As String
        While SegmentSet.Length > 0
            Segment = SegmentFunctions.GetNextSegmentFromSet(SegmentSet)
            Dim comp As New DropOffCompensationObject
            comp.CarrierName = _Convert.Null2DefaultValue(ExtractElementFromSegment("CarrierName", Segment))
            comp.Air_Value = _Convert.Null2DefaultValue(ExtractElementFromSegment("AirCompensation", Segment), 0)
            comp.Ground_Value = _Convert.Null2DefaultValue(ExtractElementFromSegment("GroundCompensation", Segment), 0)
            compensations.Add(comp)
        End While

        ' Add USPS if doesn't exist
        Dim isUSPSExist As Boolean = Not String.IsNullOrEmpty(compensations.Select(Function(x) x.CarrierName).FirstOrDefault(Function(x) x.ToLower = "USPS".ToLower))
        If Not isUSPSExist Then
            Dim otherComp As DropOffCompensationObject = compensations.FirstOrDefault(Function(x) x.CarrierName.ToLower = "other")
            If otherComp IsNot Nothing Then
                SQL = "INSERT INTO DropOff_Compensation (CarrierName, AirCompensation, GroundCompensation) VALUES ('USPS', " + otherComp.Air_Value.ToString() + ", " + otherComp.Ground_Value.ToString() + ")"
                IO_UpdateSQLProcessor(gDropOffDB, SQL)
            End If
        End If
    End Sub

    Private Function get_Compensation(ByVal carriername As String, ByVal isGround As Boolean) As Double
        get_Compensation = 0 ' assume.

        'To handle UPS and FedEx Ground/Express carrier names
        If _Controls.Contains(carriername, "Ground") Then
            carriername = carriername.Remove((carriername.Length - " Ground".Length), " Ground".Length)
            isGround = True
        ElseIf _Controls.Contains(carriername, "Express") Then
            carriername = carriername.Remove((carriername.Length - " Express".Length), " Express".Length)
            isGround = False
        End If
        'To handle UPS and FedEx Ground/Express carrier names

        For Each comp As DropOffCompensationObject In compensations
            If carriername = comp.CarrierName Then
                If isGround Then
                    Return comp.Ground_Value
                Else
                    Return comp.Air_Value
                End If
            End If
        Next
    End Function

    Private Function get_SelectedCarrier() As String
        Try
            Dim carrierFound As CarrierIcon = Carrier_ListBox.SelectedItem

            If carrierFound IsNot Nothing Then
                Return carrierFound.CarrierName
            End If
        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
        Return String.Empty
    End Function

    Private Sub set_SelectedCarrier(ByVal carrier As String)
        ''ol#1.2.11(9/3)... Ground vs Air packages distinguishing.
        Try
            Dim CarrierList As List(Of CarrierIcon) = New List(Of CarrierIcon)
            Dim CarrierFound As CarrierIcon
            Dim isGround As Boolean = False

            If _Controls.Contains(carrier, "Ground") Then
                carrier = carrier.Remove((carrier.Length - " Ground".Length), " Ground".Length)
                isGround = True
            ElseIf _Controls.Contains(carrier, "Express") Then
                carrier = carrier.Remove((carrier.Length - " Express".Length), " Express".Length)
            End If

            ' Liz: fix FedEx and DHL
            Select Case carrier
                Case "Federal Express"
                    carrier = "FedEx"
                Case "AIRBORNE"
                    carrier = "DHL"
                Case Else
            End Select
            CarrierList = Carrier_ListBox.ItemsSource

            If _Controls.Contains(carrier, "FedEx") Then
                ' get either for Express or Ground
                If isGround Then
                    CarrierFound = CarrierList.Find(Function(p) _Controls.Contains(p.CarrierName, carrier) AndAlso _Controls.Contains(p.CarrierName, "ground"))
                Else
                    CarrierFound = CarrierList.Find(Function(p) _Controls.Contains(p.CarrierName, carrier) AndAlso _Controls.Contains(p.CarrierName, "express"))
                End If
            Else
                CarrierFound = CarrierList.Find(Function(p) p.CarrierName = carrier)
            End If

            If Not IsNothing(CarrierFound) Then
                Carrier_ListBox.SelectedIndex = CarrierFound.Index
            Else
                Carrier_ListBox.SelectedIndex = CarrierList.Find(Function(p) p.CarrierName = "Other").Index
            End If

        Catch ex As Exception
            Throw ex
        End Try

    End Sub

    Private Sub DropOff_BackButton_Click(sender As Object, e As RoutedEventArgs)
        If DropOffHasData() Then
            If _MsgBox.QuestionMessage("Leaving the Drop Off Manager will lose the current data. Are you sure you want to leave?", "Drop Off Manager") Then
                BackButton_Click(sender, e)
            End If
        Else
            BackButton_Click(sender, e)
        End If
    End Sub

    Private Sub DropOff_HomeButton_Click(sender As Object, e As RoutedEventArgs)
        If DropOffHasData() Then
            If _MsgBox.QuestionMessage("Leaving the Drop Off Manager will lose the current data. Are you sure you want to leave?", "Drop Off Manager") Then
                HomeButton_Click(sender, e)
            End If
        Else
            HomeButton_Click(sender, e)
        End If
    End Sub

    Private Function guess_Carrier(ByRef trackingno As String, ByRef isFound As Boolean) As String
        guess_Carrier = String.Empty ' assume.
        Try
            ''ol#1.2.30(1/5)... 'Auto Detect' check box in Carriers selection was added for visual carrier auto vs manual mode.
            ''  If Not userWantsToSelectCarrier Then

            Dim CarrierList As List(Of CarrierIcon) = New List(Of CarrierIcon)
            CarrierList = Carrier_ListBox.ItemsSource

            If Me.chkAutoDetect.IsChecked Then
                guess_Carrier = BarCode.ShippingCo(trackingno)
                Call set_SelectedCarrier(guess_Carrier)
                '                           
                If Carrier_ListBox.SelectedIndex = CarrierList.Find(Function(p) p.CarrierName = "Other").Index Then
                    _MsgBox.InformationMessage("Select the Carrier Yourself!", "Cannot identify carrier!")
                    ''ol#1.2.22(10/29)... If a carrier cannot be detected automatic then don't add it to the list, let user select the carrier.                    
                    Carrier_ListBox.SelectedIndex = -1
                    isFound = False
                Else
                    isFound = True
                End If

                chkAutoDetect.IsChecked = True
            Else
                guess_Carrier = get_SelectedCarrier()
                If guess_Carrier = String.Empty Then
                    isFound = False
                    _MsgBox.WarningMessage("Select carrier!")
                Else
                    isFound = True
                End If
            End If

        Catch ex As Exception : _Debug.Print_(ex.Message)
            _MsgBox.WarningMessage("Select the Carrier Yourself!", "Cannot identify carrier!")
            Carrier_ListBox.SelectedIndex = -1
        End Try
    End Function

    Private Function isExist_ListItem(ByVal trackingno As String) As Boolean
        Try
            isExist_ListItem = False

            Dim count As Integer = lvPackages.Items.Count
            Dim index As Integer = 0

            If count > 0 Then
                Do While index < count
                    If trackingno = CType(lvPackages.Items(index), DropOffInformation).trackingNumber Then
                        _MsgBox.WarningMessage("This Tracking# is already in the list! If you need to edit this entry then double-click on the list entry.", "Already Scanned!")
                        txtPackageTrackingNo.Text = ""
                        txtPackageTrackingNo.Focus()
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
        isExist_DbItem = DropOffPackagesDB.IsExist_TrackingNo(trackingno)
        If isExist_DbItem Then
            ''ol#1.2.21(10/21)... By scanning an already existed tracking# you will have an option to delete this package from database.            
            If _MsgBox.QuestionMessage("This Tracking# is already in the database! Do you want to Delete it?", "Delete?") Then
                ' delete package from database
                Dim sql As String = "Delete * From DropOff_Packages Where [TrackingNo]='" & trackingno & "'"
                If DropOffPackagesDB.Execute(sql) Then
                    _MsgBox.InformationMessage("Successfully!", "Deleted!")
                    Me.txtPackageTrackingNo.Text = String.Empty
                End If
            End If
        End If
    End Function

    Private Function add_ListItem(ByVal carrier As String) As Boolean
        Try
            add_ListItem = False ' assume.

            'Need to write code for selected icon from Carrier_ListBox here                           
            'Need to write code to determine ground logic

            Dim packFee As Double
            Dim groundCheck As Boolean

            Dim CarrierList As List(Of CarrierIcon) = New List(Of CarrierIcon)
            'Dim CarrierFound As CarrierIcon
            CarrierList = Carrier_ListBox.ItemsSource

            If carrier = "" Then
                If Carrier_ListBox.SelectedIndex > -1 And Carrier_ListBox.SelectedIndex < CarrierList.Count - 1 Then
                    carrier = CarrierList(Carrier_ListBox.SelectedIndex).CarrierName
                ElseIf Carrier_ListBox.SelectedIndex = CarrierList.Count - 1 Then
                    carrier = My.Settings.DropOff_CarrierOther
                Else
                    _MsgBox.WarningMessage("Select carrier!")
                    Return False
                End If
            End If

            'CarrierFound = CarrierList.Find(Function(p) p.CarrierName = carrier)

            groundCheck = _Controls.Contains(carrier, "Ground") Or "SP" = _Controls.Left(Me.txtPackageTrackingNo.Text, 2)

            If _Controls.Contains(carrier, "Ground") Then
                carrier = carrier.Replace(" Ground", "")
                carrier = carrier.Replace("Ground", "")
            End If
            If _Controls.Contains(carrier, "Express") Then
                carrier = carrier.Replace(" Express", "")
                carrier = carrier.Replace("Express", "")
            End If
            If Me.chkPackagingFee.IsChecked Then
                If Not Double.TryParse(txtPackagingFee.Text.Replace("$", ""), packFee) Then
                    ' Liz: Error out here, as the packing fee is not a valid number
                    _MsgBox.WarningMessage("Please enter a valid number as the packing fee!")
                    ' TODO: error out
                End If
            Else
                packFee = 0.00
            End If

            DropOffInfo.Add(New DropOffInformation() With {
                            .trackingNumber = txtPackageTrackingNo.Text,
                            .CarrierName = carrier,
                            .isChecked = groundCheck,
                            .DropOffNotes = txtDesc.Text,
                            .PackagingFee = packFee
                         })

            My.Computer.Clipboard.SetText(Me.txtPackageTrackingNo.Text) ' copy tracking number to the clipboard
            lvPackages.ItemsSource = DropOffInfo
            lblPackageCount.Content = lvPackages.Items.Count
            'lvItem.Tag = Me.txtCustomerName.Tag  'Need to find if there is an equivalent of this and verify if it's necessary here in WPF/Will work once Address Entry is in place
            Return True
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Private Function add_Package() As Boolean
        Try
            Me.lblEmailSendStatus.Content = String.Empty
            'Me.lblSMSSendStatus.Content = String.Empty
            Me.lblUPSUploadStatus.Content = String.Empty
            add_Package = False ' assume.
            If 0 < Me.txtPackageTrackingNo.Text.Length Then
                ' check if this tracking# is already scanned: 
                ''ol#1.2.21(10/23)... DOM needs to truncate FedEx tracking# (to last 12 digits) before looking for duplicates.
                ''ol#1.2.22(10/29)... If a carrier cannot be detected automatic then don't add it to the list, let user select the carrier.
                Dim isFound As Boolean = False ' assume.
                Dim carrier As String = guess_Carrier(Me.txtPackageTrackingNo.Text, isFound)
                If isFound Then
                    If Not isExist_ListItem(Me.txtPackageTrackingNo.Text) Then
                        ''ol#1.1.91(2/26)... Check for duplicate tracking# in database as well as in the check in list.
                        If Not isExist_DbItem(Me.txtPackageTrackingNo.Text) Then
                            If _Controls.Left(carrier.ToUpper, 3) = "UPS" AndAlso _Controls.Left(Me.txtPackageTrackingNo.Text.ToUpper, 2) = "1Z" Then
                                Dim accessPointId As String = ShipRiteDb.Setup_Get_UPS_AccessPointId
                                If Not String.IsNullOrEmpty(accessPointId) Then
                                    Dim commInvoice As Boolean
                                    If _DropOff.Send_UPS_CommInvoiceRequest(accessPointId, Me.txtPackageTrackingNo.Text, commInvoice) Then
                                        If commInvoice Then
                                            MessageBox.Show("Commercial Invoice is Required." & vbCrLf & vbCrLf & "There will be delays in Shipping w/o an Invoice.", "Drop Off", MessageBoxButton.OK, MessageBoxImage.Warning)
                                        End If
                                    End If
                                End If
                            End If
                            If add_ListItem(carrier) Then
                                add_Package = True
                                If add_Package Then
                                    Me.txtPackageTrackingNo.Text = String.Empty
                                    Call clear_Form(False, False)
                                End If
                            End If
                        End If
                    End If
                End If
            Else
                _MsgBox.InformationMessage("Tracking number is required to add a package to the list!", "Tracking# Required!")
            End If

            txtPackageTrackingNo.Focus()
        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Failed to add Package...")
            Return False
        End Try
    End Function

    Private Function create_PackageObject_FromScreen() As DropOffPackageObject
        create_PackageObject_FromScreen = New DropOffPackageObject
        Dim obj As _baseContact = Me.txtCustomerName.Tag 'Needs reverification
        With create_PackageObject_FromScreen
            If obj IsNot Nothing Then
                .CustomerID = obj.ContactID
            End If
            .CustomerName = Me.txtCustomerName.Text
            .CarrierName = get_SelectedCarrier()
            .TrackingNo = Me.txtPackageTrackingNo.Text
            .DropOffDate = Date.Now
            .ManifestDate = Nothing
            .DropOffNotes = Me.txtDesc.Text
            If Me.chkPackagingFee.IsChecked Then
                .PackagingFee = Val(Me.txtPackagingFee.Text)
            End If
        End With
    End Function

    Private Sub set_EmailSendStatus(ByVal isok As Boolean)
        If isok Then
            Me.lblEmailSendStatus.Foreground = Green
            Me.lblEmailSendStatus.Content = "Email Successful !!!"
        Else
            Me.lblEmailSendStatus.Foreground = Red
            Me.lblEmailSendStatus.Content = "Failed to Email ..."
        End If
    End Sub

    Private Sub set_UPSUploadStatus(ByVal isok As Boolean)
        If isok Then
            Me.lblUPSUploadStatus.Foreground = Green
            Me.lblUPSUploadStatus.Content = "UPS Upload Successful !!!"
        Else
            Me.lblUPSUploadStatus.Foreground = Red
            Me.lblUPSUploadStatus.Content = "Failed UPS Upload ..."
        End If
    End Sub

    Private Function process_Packages() As Boolean
        Try
            Me.lblEmailSendStatus.Content = String.Empty
            'Me.lblSMSSendStatus.Content = String.Empty
            Me.lblUPSUploadStatus.Content = String.Empty
            If Me.chkPrintReceipt.IsChecked Then
                'Need to write printer code
                Call _DropOff.Print_DropOffReceipt(Me.lvPackages, Me.txtCustomerName.Text, GetPolicyData(gShipriteDB, "DropOffCopyCount", 1))
            End If

            If Me.chkSendEmails.IsChecked Then
                Dim obj As _baseContact = Me.txtCustomerName.Tag 'Need verification
                Dim emailContent As String = ApiRequest.createDropoffEmail(DropOffInfo, Me.txtCustomerName.Text, DateTime.Now.ToString("MM/dd/yyyy hh:mm tt"))
                If obj IsNot Nothing AndAlso Not String.IsNullOrEmpty(obj.Email) Then
                    Dim emailSent As Boolean = False
                    Dim address() As String = {obj.Email}
                    Call set_EmailSendStatus(ApiRequest.sendEmail(address, "Drop Off Receipt", emailContent))
                Else
                    Dim result As MessageBoxResult = MessageBox.Show("Contact's email was not found." & Environment.NewLine & "Would you like to provide an email to use for now?", "Drop Off Receipt", MessageBoxButton.YesNo, MessageBoxImage.Question)
                    If result = MessageBoxResult.Yes Then
                        badEmail(emailContent)
                    End If
                End If
            End If

            ' Liz: TODO: Implement SMS sending
            'If Me.chkSendSMS.IsChecked Then
            '    Dim apiResult As String

            '    ' Build list of tracking numbers
            'Dim assembledPackages As String = ""
            'Dim objSingleDOI As DropOffInformation
            'For Each package As ListViewItem In Me.lvPackages.Items
            '    objSingleDOI = CType(package.Content, DropOffInformation)
            '    assembledPackages = assembledPackages & "." & objSingleDOI.trackingNumber & ","
            'Next

            '    ' Retrieve customer's phone number and service provider
            '    Dim customerPhone As String = ""
            '    Dim customerCarrier As String = ""

            '    ' Build payload for API (There's probably a better way to do this, but I was unable to find it)
            '    Dim payload As String = "{'key':'"
            '    payload = payload & ApiRequest.apiKey & "','phone':'" & "CUSTOMERPHONE" & "','carrier':'" & "" & "','type':'"
            '    payload = payload & "" & "','store':'" & "" & "','storephone':'" & "" & "','time':'" & ""
            '    payload = payload & "','packages':'" & assembledPackages & "'}"

            '    ' apiResult = ApiRequest.liminal("sms", payload)
            'End If

            Call save_Packages()
            Return True
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Private Sub DropOffManager_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        Try
            My.Settings.DropOff_chkPrintReceipt = chkPrintReceipt.IsChecked
            My.Settings.DropOff_chkSendEmails = chkSendEmails.IsChecked
            'My.Settings.DropOff_chkAutoDetect = chkAutoDetect.IsChecked
            'My.Settings.DropOff_chkSendSMS = chkSendSMS.IsChecked
            My.Settings.Save()
        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Failed to close Drop off manager.")
        End Try
    End Sub

    Private Sub Load_Carrier_Options()
        'Dim Carrier_List As List(Of String)
        'Carrier_List = New List(Of String)

        'Carrier_List.Add("Resources/FedEx_Logo.png")
        'Carrier_List.Add("Resources/UPS_Logo.png")
        'Carrier_List.Add("Resources/DHL_Logo.png")
        'Carrier_List.Add("Resources/USPS_Logo.png")
        'Carrier_List.Add("Resources/Other_Logo.png")
        'Carrier_ListBox.ItemsSource = Carrier_List
        'Carrier_ListBox.Items.Refresh()


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
                If fieldValue = "FedEx" Then
                    'we need ground and express
                    current_Carrier.CarrierImage = "Resources/FedEx_Ground.png"
                    current_Carrier.Index = index
                    current_Carrier.CarrierName = "FedEx Ground"
                    CarrierList.Add(current_Carrier)
                    index = index + 1

                    current_Carrier = New CarrierIcon
                    current_Carrier.CarrierImage = "Resources/FedEx_Express.png"
                    current_Carrier.Index = index
                    current_Carrier.CarrierName = "FedEx Express"
                    CarrierList.Add(current_Carrier)
                    index = index + 1
                Else
                    current_Carrier.CarrierImage = "Resources/" & fieldValue & "_Logo.png"
                    current_Carrier.Index = index
                    current_Carrier.CarrierName = fieldValue
                    CarrierList.Add(current_Carrier)

                    index = index + 1
                End If
            Loop

            current_Carrier = New CarrierIcon

            current_Carrier.Index = index
            current_Carrier.CarrierName = "Other"
            current_Carrier.CarrierImage = "Resources/Other_Logo.png"

            CarrierList.Add(current_Carrier)

            Carrier_ListBox.ItemsSource = CarrierList
            Carrier_ListBox.Items.Refresh()

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try


    End Sub

    Private Sub PrintButton_Click(sender As Object, e As RoutedEventArgs) Handles PrintButton.Click

        If Print_Popup.IsOpen = True Then
            Print_Popup.IsOpen = False
        Else
            ' Set Default dates SRN-191
            dtpFrom.SelectedDate = Today.AddMonths(-1)
            dtpTo.SelectedDate = Today
            Print_Popup.IsOpen = True
        End If

    End Sub

    Private Function save_Packages() As Boolean
        Try
            save_Packages = False ' assume.
            Me.lblUPSUploadStatus.Content = String.Empty
            ' This compiles a list of packages that have failed to be saved
            Dim FailedPackages As Collection(Of DropOffInformation) = New Collection(Of DropOffInformation)
            ' This compiles a list of packages that are NOT DHL packages for the User to upload the list to their respective courier sites
            Dim NonDHLPackages As Collection(Of DropOffInformation) = New Collection(Of DropOffInformation)
            Dim UPSFailedPackageCount As Integer = -1
            For Each package As DropOffInformation In DropOffInfo
                If save_Package(package) Then
                    Dim FailedPackage As Boolean = True
                    If "DHL" = package.CarrierName Then
                        ''ol#1.2.34(4/14)... DHL-INT account number should be read from Setup -> ABTPID3 re-used field.
                        Dim accntNo As String = ShipRiteDb.Setup_Get_DHL_ShipperNo
                        ''ol#1.2.42(10/5)... Warning message will pop-up if DHL is missing account#.
                        If String.IsNullOrEmpty(accntNo) Then
                            MessageBox.Show("DHL account number is missing!" & Environment.NewLine & "Check Setup Settings!", "Drop Off", MessageBoxButton.OK, MessageBoxImage.Exclamation)
                        Else
                            ''AP(11/28/2016) - Don't show DHL Account# when saving DHL Drop Offs.
                            If System.Windows.MessageBoxResult.Yes = MessageBox.Show(String.Format("Tracking#: {0}{1}", package.trackingNumber, Environment.NewLine) & "" & Environment.NewLine & "Continue DHL Upload?", "Drop Off", MessageBoxButton.YesNo, MessageBoxImage.Question) Then
                                If _DropOff.Send_DHL_SOAPRequest(accntNo, package.trackingNumber) Then
                                    FailedPackage = False
                                End If
                            End If
                        End If
                    ElseIf _Controls.Left(package.CarrierName.ToUpper, 3) = "UPS" Then
                        Dim accessPointId As String = ShipRiteDb.Setup_Get_UPS_AccessPointId
                        FailedPackage = Not String.IsNullOrEmpty(accessPointId) ' False ' non UPS AP locations - just save to database
                        If FailedPackage Then
                            If UPSFailedPackageCount = -1 Then UPSFailedPackageCount = 0
                            If _DropOff.Send_UPS_ScanRequest(accessPointId, package.trackingNumber) Then
                                FailedPackage = False
                            Else
                                UPSFailedPackageCount += 1
                            End If
                        End If
                    Else
                        NonDHLPackages.Add(package)
                        FailedPackage = False
                    End If

                    If FailedPackage Then
                        FailedPackages.Add(package)
                    End If
                Else
                    _MsgBox.WarningMessage("Error inserting dropoff records into the database", "Failed to Process!")
                End If
            Next
            If UPSFailedPackageCount > -1 Then
                set_UPSUploadStatus(UPSFailedPackageCount = 0)
            End If
            Return True
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Private Function save_Package(obj As DropOffInformation) As Integer
        Try
            Dim sql2cmd As New sqlINSERT
            Dim sql2exe As String = String.Empty
            Dim Segment As String = ""
            Dim SQL As String = ""
            Dim CustID As Integer = 0
            If Not IsNothing(txtCustomerName.Tag) Then
                Dim cust As _baseContact = txtCustomerName.Tag
                CustID = cust.ContactID
            End If
            Segment = AddElementToSegment(Segment, "CustomerId", CustID)
            Segment = AddElementToSegment(Segment, "CustomerName", txtCustomerName.Text)
            Segment = AddElementToSegment(Segment, "TrackingNo", obj.trackingNumber)
            'Need to insert Customer details here, once the customer module is in place            
            Segment = AddElementToSegment(Segment, "CarrierName", obj.CarrierName)
            Segment = AddElementToSegment(Segment, "IsGround", obj.isChecked.ToString)
            Segment = AddElementToSegment(Segment, "DropOffDate", Date.Now.ToString)
            Segment = AddElementToSegment(Segment, "DropOffNotes", obj.DropOffNotes)
            Segment = AddElementToSegment(Segment, "PackagingFee", obj.PackagingFee.ToString)
            Segment = AddElementToSegment(Segment, "Compensation", get_Compensation(obj.CarrierName, obj.isChecked))
            Segment = AddElementToSegment(Segment, "Clerk", gCurrentUser)

            SQL = MakeInsertSQLFromSchema("DropOff_Packages", Segment, gDropOffSchema, True)

            Return IO_UpdateSQLProcessor(gDropOffDB, SQL)
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Private Sub txtPackageTrackingNo_TextChanged(sender As Object, e As TextChangedEventArgs) Handles txtPackageTrackingNo.TextChanged
        Try
            ''ol#1.2.02(6/2)... If Mailbox or Tracking# text changed then success/failed labels should be cleared.
            If Not 0 = Me.txtPackageTrackingNo.Text.Length Then
                Me.lblEmailSendStatus.Content = String.Empty
                'Me.lblSMSSendStatus.Content = String.Empty
                Me.lblUPSUploadStatus.Content = String.Empty
            End If
        Catch ex As Exception

        End Try
    End Sub

    Private Sub txtPackageTrackingNo_KeyDown(ByVal sender As Object, ByVal e As Input.KeyEventArgs) Handles txtPackageTrackingNo.KeyDown
        Try
            If e.Key = Key.Return Then
                txtPackageTrackingNo.Text = StrConv(txtPackageTrackingNo.Text, vbUpperCase)
                Cursor = Input.Cursors.Wait
                If add_Package() Then
                End If
            End If
        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to add Package to the list...")
        Finally : Cursor = Input.Cursors.Arrow
        End Try
    End Sub

    Private Sub txtPackageTrackingNo_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtPackageTrackingNo.LostFocus
        ''ol#1.2.03(6/3)... You can Tab from Tracking# to Notes then Location and back to Tracking# if Mbox is selected.
        'If 0 = Me.txtDesc.Text.Length Then
        '    Me.txtDesc.Select()
        'ElseIf Me.chkPackagingFee.Checked Then
        '    Me.txtPackagingFee.Select()
        'End If
    End Sub

    Private Sub txtCustomerName_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtCustomerName.LostFocus
        Me.txtPackageTrackingNo.Focus()
    End Sub

    Private Sub txtCustomerName_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtCustomerName.TextChanged
        Try
            If 1 = Me.txtCustomerName.Text.Length Then
                Me.txtCustomerName.Text = Me.txtCustomerName.Text.ToUpper
                Me.txtCustomerName.SelectionStart = 1
            ElseIf 1 < Me.txtCustomerName.Text.Length Then
                _Controls.ToProperCase(Me.txtCustomerName)
            End If
            Me.txtCustomerName.Tag = Nothing
        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to change the name to Proper Case...")
        End Try
    End Sub

    Private Sub txtCustomerName_KeyDown(sender As Object, e As Input.KeyEventArgs) Handles txtCustomerName.KeyDown
        Try
            If e.Key = Key.Return Then
                Call open_AddressEntry()
            End If
        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to open Contact...")
        End Try
    End Sub

    Private Sub txtCustomerName_MouseDoubleClick(sender As Object, e As RoutedEventArgs) Handles txtCustomerName.MouseDoubleClick, CustomerLookupTrigger.Click
        Try
            Call open_AddressEntry()
        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to open Contact...")
        End Try
    End Sub

    Private Sub open_AddressEntry()
        Try
            gAutoExitFromContacts = True
            gContactManagerSegment = ""
            gResult = ""
            Dim win As New ContactManager(Me,, txtCustomerName.Text)
            win.ShowDialog(Me)

            LoadAddressFromSegment(gContactManagerSegment)

            'Dim frmprops As New _FormProperties(True, True, True, True, _DropOff.StoreOwner.Country)
            'frmprops.IsShow_AddressMatches = True
            ''ol#1.2.21(10/23)... 'Net Address Entry' will be able to add/edit contacts now when opened from Drop Off Manager.
            '_AddressEntry.AddressEntry_ShowForm(frmprops, ShipRiteDb.path2db, ZipCodesDb.path2db, _DropOff.CustomerObject, True)
            Me.txtCustomerName.Text = _DropOff.CustomerObject.CompanyName
            Me.txtCustomerName.Tag = _DropOff.CustomerObject
            txtPackageTrackingNo.Focus()


        Catch ex As Exception
            Throw ex
        End Try
    End Sub

    Private Sub LoadAddressFromSegment(ByRef Segment As String)
        _DropOff.CustomerObject = New _baseContact

        _DropOff.CustomerObject.CompanyName = ExtractElementFromSegment("Name", Segment)
        _DropOff.CustomerObject.ContactID = Val(ExtractElementFromSegment("ID", Segment, 0))
        _DropOff.CustomerObject.AccountNumber = ExtractElementFromSegment("AcctNum", Segment, "")
        _DropOff.CustomerObject.FName = ExtractElementFromSegment("FName", Segment, "")
        _DropOff.CustomerObject.LName = ExtractElementFromSegment("LName", Segment, "")
        _DropOff.CustomerObject.Addr1 = ExtractElementFromSegment("Addr1", Segment, "")
        _DropOff.CustomerObject.Addr2 = ExtractElementFromSegment("Addr2", Segment, "")
        _DropOff.CustomerObject.City = ExtractElementFromSegment("City", Segment, "")
        _DropOff.CustomerObject.State = ExtractElementFromSegment("State", Segment, "")
        _DropOff.CustomerObject.Zip = ExtractElementFromSegment("Zip", Segment, "")
        _DropOff.CustomerObject.Tel = ExtractElementFromSegment("Phone", Segment, "")
        _DropOff.CustomerObject.Fax = ExtractElementFromSegment("Phone2", Segment, "")
        _DropOff.CustomerObject.CellCarrier = ExtractElementFromSegment("CellCarrier", Segment, "")
        _DropOff.CustomerObject.CellPhone = ExtractElementFromSegment("CellPhone", Segment, "")
        _DropOff.CustomerObject.Email = ExtractElementFromSegment("EMail", Segment, "")
    End Sub

    Private Sub txtPackagingFee_LostFocus(sender As Object, e As RoutedEventArgs) Handles txtPackagingFee.LostFocus
        Try
            txtPackagingFee.Text = Format(Val(txtPackagingFee), "0.00")
            Me.txtPackageTrackingNo.Focus()
        Catch ex As Exception

        End Try
    End Sub

    Private Sub lvPackages_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles lvPackages.MouseDoubleClick
        Try

            Dim lvPackage_Selected As New DropOffInformation()

            If Me.lvPackages.SelectedItems IsNot Nothing AndAlso 0 < Me.lvPackages.Items.Count Then
                lvPackage_Selected = CType(lvPackages.SelectedItem, DropOffInformation)
                Call set_SelectedCarrier(lvPackage_Selected.CarrierName)
                Me.txtPackageTrackingNo.Text = lvPackage_Selected.trackingNumber
                Me.txtDesc.Text = lvPackage_Selected.DropOffNotes
                Me.txtPackagingFee.Text = lvPackage_Selected.PackagingFee

                'Will work after address entry module is in place
                '    If lvItem.Tag IsNot Nothing Then
                '        Dim obj As CommonShip._baseContact = lvItem.Tag
                '        Me.txtCustomerName.Tag = obj
                '        Me.txtCustomerName.Text = obj.CompanyName
                '    End If
                'Will work after address entry module is in place

                If RemoveFromList(lvPackages.SelectedIndex) Then
                    lvPackages.ItemsSource = DropOffInfo
                    lblPackageCount.Content = lvPackages.Items.Count
                End If

            End If

        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to bring out Package entry...")
        End Try
    End Sub

    Private Function RemoveFromList(ByVal itemIndex As Integer) As Boolean
        Try
            DropOffInfo.RemoveAt(itemIndex)
            Return True
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Private Sub cmdNextTracking_Click(sender As Object, e As RoutedEventArgs) Handles cmdNextTracking.Click
        Try
            Cursor = Input.Cursors.Wait
            If add_Package() Then
            End If
        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Failed to add Package to the list...")
        Finally : Cursor = Input.Cursors.Arrow
        End Try
    End Sub

    Private Sub cmdProcess_Click(sender As Object, e As RoutedEventArgs) Handles cmdProcess.Click
        Try
            If lvPackages.Items.Count = 0 Then Exit Sub

            If process_Packages() Then
                _MsgBox.InformationMessage("All packages were saved and processed successfully!", "Saved and Processed!")
                Call clear_Form(True, True) ''AP(11/10/2016) - Reenable clearing DropOff form after selecting Process & Save button.
                lvPackages.ItemsSource = Nothing
                lblPackageCount.Content = 0
                DropOffInfo.Clear()
            Else
                _MsgBox.WarningMessage("Some of the packages were not processed and not saved!" & Environment.NewLine & "Uncheck some processing options and try again...", "Failed to Process!")
            End If
        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to process Packages...")

        End Try
        txtPackageTrackingNo.Focus()

    End Sub

    Private Sub cmdClearCurrent_Click(sender As Object, e As RoutedEventArgs) Handles cmdClearCurrent.Click
        ' TODO: Need to remove all selected packages here, maybe use a MessageBox to verify the user really wants to remove said package(s)
        Dim indexesToKill As Collection(Of Integer) = New Collection(Of Integer)
        'For i = 0 To DropOffInfo.Count - 1
        '    If DropOffInfo.Item(i).isChecked Then
        '        ' Remove package
        '        'DropOffInfo.RemoveAt(i) ' Caused issue with for loop
        '        indexesToKill.Add(i)
        '    End If
        'Next
        For Each Item In lvPackages.SelectedItems
            Dim trackingNumberToRemove As String = CType(Item, DropOffInformation).trackingNumber
            Dim Handled As Boolean = False
            For i = 0 To DropOffInfo.Count
                If Not Handled AndAlso DropOffInfo(i).trackingNumber = trackingNumberToRemove Then
                    indexesToKill.Add(i)
                    Handled = True
                End If
            Next
        Next
        ' Remove packages from collection
        For i = indexesToKill.Count To 1 Step -1
            DropOffInfo.RemoveAt(indexesToKill(i - 1))
        Next
        ' Update list with collection
        lvPackages.ItemsSource = DropOffInfo
        lblPackageCount.Content = lvPackages.Items.Count
        ' Old Code:
        'Changed from Me.lvPackages.SelectedItems IsNot Nothing to Me.lvPackages.SelectedIndex > -1 because of WPF
        'If Me.lvPackages.SelectedIndex > -1 And 0 < Me.lvPackages.Items.Count Then
        '    If RemoveFromList(lvPackages.SelectedIndex) Then
        '        lvPackages.ItemsSource = DropOffInfo
        '    Else
        '        Debug.WriteLine("Failed removing from list")
        '    End If
        'Else
        '    Debug.WriteLine("Selected package not within bounds")
        'End If
    End Sub

    Private Sub SetupOptions_Click(sender As Object, e As RoutedEventArgs) Handles SetupOptions.Click

        lblOther.Content = My.Settings.DropOff_CarrierOther
        Call load_CompensationValues_ForPopup()

        If Setup_Popup.IsOpen = False Then
            Setup_Popup.IsOpen = True
        End If

        If My.Settings.DropOff_FocusTracking = True Then
            Focus_Tracking_RdoBtn.IsChecked = True
        Else
            Focus_Customer_RdoBtn.IsChecked = True
        End If

    End Sub

    Private Sub Focus_Tracking_RdoBtn_Checked(sender As Object, e As RoutedEventArgs) Handles Focus_Tracking_RdoBtn.Checked, Focus_Customer_RdoBtn.Checked
        If Focus_Tracking_RdoBtn.IsChecked = True Then
            My.Settings.DropOff_FocusTracking = True
        Else
            My.Settings.DropOff_FocusTracking = False
        End If
    End Sub

#Region "FedEx Returns"
    Private Sub FedExReturns_Btn_Click(sender As Object, e As RoutedEventArgs) Handles FedExReturns_Btn.Click
        If FedExReturns_Popup.IsOpen Then
            FedExReturns_Popup.IsOpen = False
        Else
            FedExReturns_Popup.IsOpen = True
        End If
    End Sub

    Private Sub FedExReturns_PRINT_Btn_Click(sender As Object, e As RoutedEventArgs) Handles FedExReturns_PRINT_Btn.Click
        Dim FXR_TrackingNo As String = ""

        If FedExReturns_RMA_TxtBx.Text = "" Then Exit Sub

        If gFedExReturnsSETUP.LocationID = "" Then
            MsgBox("FedEx Return Technology is not setup. Please go to Shipping Setup and enter in your Location ID.", vbExclamation, "Error!")
            Exit Sub
        End If

        If FedExRETURNS.FXReturns_SendRequest(FedExReturns_RMA_TxtBx.Text, FedExReturns_RequestPackingSlip_ChkBx.IsChecked, FedExReturns_ImageType_CmbBx.SelectedIndex, FXR_TrackingNo) Then
            FedExReturns_RMA_TxtBx.Text = ""
            FedExReturns_Popup.IsOpen = False
            txtPackageTrackingNo.Text = FXR_TrackingNo
            txtPackageTrackingNo.Focus()
        Else
            FedExReturns_Popup.IsOpen = True
        End If

    End Sub

    Private Sub FedExReturns_RMA_TxtBx_KeyDown(sender As Object, e As KeyEventArgs) Handles FedExReturns_RMA_TxtBx.KeyDown

        If e.Key = Key.Enter Then
            FedExReturns_PRINT_Btn_Click(Nothing, Nothing)
        End If

    End Sub
#End Region

#Region "Compensation Setup"
    Private Sub load_CompensationValues_ForPopup()
        If compensations Is Nothing OrElse compensations.Count = 0 Then
            load_CompensationValues()
        End If
        For Each comp In compensations
            Select Case comp.CarrierName
                Case "UPS"
                    Me.txtUPSAirComp.Text = comp.Air_Value.ToString("C")
                    Me.txtUPSGroundComp.Text = comp.Ground_Value.ToString("C")
                Case "FedEx"
                    Me.txtFedExAirComp.Text = comp.Air_Value.ToString("C")
                    Me.txtFedExGroundComp.Text = comp.Ground_Value.ToString("C")
                Case "DHL"
                    Me.txtDHLAirComp.Text = comp.Air_Value.ToString("C")
                Case "Other"
                    Me.txtOtherAirComp.Text = comp.Air_Value.ToString("C")
                    Me.txtOtherGroundComp.Text = comp.Ground_Value.ToString("C")
            End Select
        Next
    End Sub

    Private Sub txtUPSAirComp_LostFocus(sender As Object, e As RoutedEventArgs) Handles txtUPSAirComp.LostFocus, txtUPSGroundComp.LostFocus, txtFedExAirComp.LostFocus, txtFedExGroundComp.LostFocus, txtDHLAirComp.LostFocus, txtOtherAirComp.LostFocus, txtOtherGroundComp.LostFocus, txtPackagingFee.LostFocus
        Try
            Dim txt As TextBox = CType(sender, TextBox)
            If txt IsNot Nothing Then
                Dim m As Double = Val(txt.Text.Replace("$", ""))
                txt.Text = m.ToString("C")
            End If
        Catch ex As Exception : _Debug.Print_(ex.Message)

        End Try
    End Sub

    Private Function save_CompensationValues(ByVal carriername As String, ByVal aircomp As String, ByVal groundcomp As String) As Boolean
        Dim SQL As String = ""
        Dim ret As Long

        SQL = "UPDATE DropOff_Compensation SET AirCompensation = " + aircomp.Replace("$", "") + ",  GroundCompensation = " + groundcomp.Replace("$", "") + " WHERE CarrierName = '" + carriername + "'"

        ret = IO_UpdateSQLProcessor(gDropOffDB, SQL)

        If ret > 0 Then
            Return True
        Else
            Return False
        End If

    End Function

    Private Function save_CarrierCompensation() As Boolean
        save_CarrierCompensation = False ' assume.
        If save_CompensationValues("UPS", Me.txtUPSAirComp.Text, Me.txtUPSGroundComp.Text) Then
            If save_CompensationValues("FedEx", Me.txtFedExAirComp.Text, Me.txtFedExGroundComp.Text) Then
                If save_CompensationValues("DHL", Me.txtDHLAirComp.Text, "0") Then
                    If save_CompensationValues("Other", Me.txtOtherAirComp.Text, Me.txtOtherGroundComp.Text) Then
                        If save_CompensationValues("USPS", Me.txtOtherAirComp.Text, Me.txtOtherGroundComp.Text) Then
                            save_CarrierCompensation = True
                        End IF
                    End If
                End If
            End If
        End If
        load_CompensationValues() ' reload
    End Function

    Private Function update_AirCompensationValues_inPackages(carriername As String, aircomp As String) As Boolean
        ''ol#1.2.34(3/30)... By clicking 'Update' in Compensation Setup it will recalculate all the previous compensations in the Compensation report.
        Dim SQL As String = ""
        Dim ret As Long
        ' Update past comp values
        SQL = "UPDATE DropOff_Packages SET Compensation = " + aircomp.Replace("$", "") + " WHERE CarrierName = '" + carriername + "'"
        ret = IO_UpdateSQLProcessor(gDropOffDB, SQL)
        If ret >= 0 Then
            Return True
        Else
            Return False
        End If
    End Function

    Private Function update_GroundCompensationValues_inPackages(carriername As String, groundcomp As String) As Boolean
        ''ol#1.2.34(3/30)... By clicking 'Update' in Compensation Setup it will recalculate all the previous compensations in the Compensation report.
        Dim SQL As String = ""
        Dim ret As Long
        ' Update past comp values
        SQL = "UPDATE DropOff_Packages SET Compensation = " + groundcomp.Replace("$", "") + " WHERE CarrierName = '" + carriername + "'"
        ret = IO_UpdateSQLProcessor(gDropOffDB, SQL)
        If ret >= 0 Then
            Return True
        Else
            Return False
        End If
    End Function

    Private Function update_CarrierCompensation() As Boolean
        ''ol#1.2.34(3/30)... By clicking 'Update' in Compensation Setup it will recalculate all the previous compensations in the Compensation report.
        Dim result As Boolean = True
        result = result And update_AirCompensationValues_inPackages("UPS", Me.txtUPSAirComp.Text)
        result = result And update_GroundCompensationValues_inPackages("UPS", Me.txtUPSGroundComp.Text)
        result = result And update_AirCompensationValues_inPackages("FedEx", Me.txtFedExAirComp.Text)
        result = result And update_GroundCompensationValues_inPackages("FedEx", Me.txtFedExGroundComp.Text)
        result = result And update_AirCompensationValues_inPackages("DHL", Me.txtDHLAirComp.Text)
        result = result And update_AirCompensationValues_inPackages("Other", Me.txtOtherAirComp.Text)
        result = result And update_GroundCompensationValues_inPackages("Other", Me.txtOtherGroundComp.Text)
        Return result
    End Function

    Private Sub cmdSave_Click(sender As System.Object, e As System.EventArgs) Handles cmdSave.Click
        Try
            If save_CarrierCompensation() Then
                _MsgBox.InformationMessage("Successful!", "Saved!")
            End If
        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to save Compensation values...")
        End Try
    End Sub

    Private Sub cmdUpdate_Click(sender As System.Object, e As System.EventArgs) Handles cmdUpdate.Click
        Try
            If _MsgBox.QuestionMessage("All Compensations for the Previous Packages will be Updated. Continue?", "Update?") Then
                If update_CarrierCompensation() Then
                    _MsgBox.InformationMessage("Previous Package Compensations were updated Successfully!", "Updated!")
                End If
            End If
        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to update Package Compensation values...")
        End Try
    End Sub

    Private Sub cmdChangeCarrier_Click(sender As System.Object, e As System.EventArgs) Handles lblOther.MouseLeftButtonUp
        Try
            ''ol#1.2.40(6/1)... 'Other' carrier name could be changed now to a different particular carrier name. In Compensation Setup form.
            Dim carrier As String = InputBox("Type new Carrier Name below:", "Change Carrier", "Other")
            If carrier IsNot Nothing AndAlso Not carrier = String.Empty Then
                Me.lblOther.Content = carrier
            End If
            'DropOffManager.optOther.Text = Me.lblOther.Text
            My.Settings.DropOff_CarrierOther = Me.lblOther.Content
            My.Settings.Save()
        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to Change Carrier name...")
        End Try
    End Sub

    Private Sub CopyCtChanged(sender As Object, e As RoutedEventArgs) Handles printCopyCount.TextChanged
        Dim copyCount As Integer
        If Integer.TryParse(printCopyCount.Text, copyCount) Then
            UpdatePolicy(gShipriteDB, "DropOffCopyCount", copyCount)
        End If
    End Sub
#End Region

#Region "Reports"
    Private Sub cmdRunManifest_Click(sender As Object, e As RoutedEventArgs) Handles cmdRunManifest.Click
        ' Print Driver Manifest
        ' TODO: Skip date range????????
        PrintReport(CType(sender, System.Windows.Controls.Button).Name, False)
    End Sub

    'Private Sub dtpFrom_CalendarClosed(sender As Object, e As RoutedEventArgs) Handles dtpFrom.CalendarClosed
    '    ' on selection of date, auto enable checkbox
    '    If String.IsNullOrEmpty(dtpFrom.Text) Then
    '        ReportFromCheck.IsChecked = True
    '    End If
    'End Sub

    'Private Sub dtpTo_CalendarClosed(sender As Object, e As RoutedEventArgs) Handles dtpTo.CalendarClosed
    '    ' on selection of date, auto enable checkbox
    '    If String.IsNullOrEmpty(dtpTo.Text) Then
    '        ReportToCheck.IsChecked = True
    '    End If
    'End Sub

    Private Sub Report_Click(sender As Object, e As RoutedEventArgs) Handles lklDropOff.Click, lklCustomerProdReport.Click, lklDropOffCompensations.Click, lklFascCompensation.Click, lklManifest.Click
        PrintReport(CType(sender, System.Windows.Controls.Button).Name)
    End Sub

#End Region

#Region "Helper Functions"
    Private Sub badEmail(content As String, Optional ask As Boolean = True)
        ' Ask for email to use
        Dim email As String = Interaction.InputBox("Enter the email to send a receipt to:", "Drop Off Receipt", "johnnyappleseed@apple.com")
        ' Verify email
        Try
            Dim address As MailAddress = New MailAddress(email)
            ' send mail
            Dim addresses As String() = {address.Address}
            Call set_EmailSendStatus(ApiRequest.sendEmail(addresses, "Drop Off Receipt", content))
        Catch ex As Exception
            Dim result As MessageBoxResult = MessageBox.Show("You have entered an invalid email address. Would you like to retry?", "Drop Off Receipt", MessageBoxButton.YesNo, MessageBoxImage.Question)
            If result = MessageBoxResult.Yes Then
                badEmail(content)
            End If
        End Try
    End Sub

    Private Sub PrintReport(button As String, Optional useDateRange As Boolean = True)
        Try
            ' Prepare variables
            Dim dateFrom As Date?
            Dim dateTo As Date?
            Dim rep As ShipRiteReports._ReportObject = New ShipRiteReports._ReportObject
            Dim store As New _baseContact
            ShipRiteDb.Setup_GetAddress_StoreOwner(store)
            ' Prepare printer stuff
            rep.PrinterName = GetPolicyData(gShipriteDB, "ReportPrinter", "")
            rep.ReportParameters.Add(store.CompanyName)
            rep.ReportParameters.Add(store.Addr1)
            rep.ReportParameters.Add(store.CityStateZip)
            ' Set date range
            If useDateRange Then
                Dim daterange As String = String.Empty
                ' If ReportFromCheck.IsChecked And ReportToCheck.IsChecked Then
                dateFrom = IIf(dtpFrom.SelectedDate IsNot Nothing, dtpFrom.SelectedDate, #11/1/2014#)
                dateTo = IIf(dtpTo.SelectedDate IsNot Nothing, dtpTo.SelectedDate, Date.Today)
                daterange = String.Format("from {0} to {1}", dateFrom.Value.ToString("d"), dateTo.Value.ToString("d"))
                'ElseIf ReportFromCheck.IsChecked Then
                '    dateFrom = IIf(dtpFrom.SelectedDate IsNot Nothing, dtpFrom.SelectedDate, #11/1/2014#)
                '    dateTo = Date.Today
                '    daterange = String.Format("from {0} to {1}", dateFrom.Value.ToString("d"), dateTo.Value.ToString("d"))
                'ElseIf ReportToCheck.IsChecked Then
                '    dateFrom = #11/1/2014#
                '    dateTo = IIf(dtpTo.SelectedDate IsNot Nothing, dtpTo.SelectedDate, Date.Today)
                '    daterange = String.Format("from the beginning to {0}", dateTo.Value.ToString("d"))
                'Else
                '    daterange = "with no date restrictions"
                'End If
                If dateFrom IsNot Nothing AndAlso dateTo IsNot Nothing Then
                    rep.ReportFormula = String.Format("{{DropOff_Packages.DropOffDate}} in DateTime ({0}, {1}, {2}, 0, 0, 0) to DateTime ({3}, {4}, {5}, 23, 59, 59)",
                                        dateFrom.Value.Year, dateFrom.Value.Month, dateFrom.Value.Day,
                                        dateTo.Value.Year, dateTo.Value.Month, dateTo.Value.Day)
                Else
                    rep.ReportFormula = String.Empty
                End If
                rep.ReportParameters.Add(daterange)
            End If


            ' Process individual reports
            Select Case button
                Case "lklDropOff" ' Print Drop Off Report
                    rep.ReportName = "DropOff.rpt"

                Case "lklDropOffCompensations" ' Print Drop Off Compensation Report
                    rep.ReportName = "DropOff_Compensation.rpt"

                Case "lklFascCompensation" ' Print Drop Off Fasc Compensation Report
                    Dim packagelist As String = ""
                    rep.ReportName = "DropOff_FASC_Compensation.rpt"

                    Dim dtable As New DataTable
                    dtable.Columns.Add("PackageID", GetType(Integer))
                    dtable.Columns.Add("CarrierName", GetType(String))
                    dtable.Columns.Add("IsGround", GetType(Boolean))
                    dtable.Columns.Add("TrackingNo", GetType(String))
                    dtable.Columns.Add("DropOffDate", GetType(Date))
                    '
                    ' FedEx DropOffs
                    Dim Segment As String
                    Dim carriername As String

                    carriername = cmbCarrier_fasc.Text
                    If DropOffPackagesDB.Read_Packages(carriername, packagelist, dateFrom, dateTo) Then

                        Do Until packagelist = ""

                            Segment = GetNextSegmentFromSet(packagelist)

                            Dim drow As DataRow = dtable.NewRow
                            drow("PackageID") = ExtractElementFromSegment("PackageID", Segment)
                            drow("CarrierName") = ExtractElementFromSegment("CarrierName", Segment)
                            drow("IsGround") = ExtractElementFromSegment("IsGround", Segment)
                            drow("TrackingNo") = ExtractElementFromSegment("TrackingNo", Segment)
                            drow("DropOffDate") = ExtractElementFromSegment("DropOffDate", Segment)

                            dtable.Rows.Add(drow)
                        Loop
                    End If

                    ' FedEx HAL
                    If carriername = "FedEx" Then
                        If _MailboxPackagesDB.Read_Packages("FedEx", _MailboxPackage.FEDEX_HAL, packagelist, dateFrom, dateTo) Then

                            Do Until packagelist = ""

                                Segment = GetNextSegmentFromSet(packagelist)

                                Dim drow As DataRow = dtable.NewRow
                                drow("PackageID") = ExtractElementFromSegment("PackageID", Segment)
                                drow("CarrierName") = ExtractElementFromSegment("CarrierName", Segment) & " HAL"
                                drow("IsGround") = ExtractElementFromSegment("IsGround", Segment)
                                drow("TrackingNo") = ExtractElementFromSegment("TrackingNo", Segment)
                                drow("DropOffDate") = ExtractElementFromSegment("ReceivedDate", Segment)

                                dtable.Rows.Add(drow)
                            Loop
                        End If
                    ElseIf carriername = "UPS" Then
                        If _MailboxPackagesDB.Read_Packages("UPS", _MailboxPackage.UPS_AP, packagelist, dateFrom, dateTo) Then

                            Do Until packagelist = ""

                                Segment = GetNextSegmentFromSet(packagelist)

                                Dim drow As DataRow = dtable.NewRow
                                drow("PackageID") = ExtractElementFromSegment("PackageID", Segment)
                                drow("CarrierName") = ExtractElementFromSegment("CarrierName", Segment) & " AP"
                                drow("IsGround") = ExtractElementFromSegment("IsGround", Segment)
                                drow("TrackingNo") = ExtractElementFromSegment("TrackingNo", Segment)
                                drow("DropOffDate") = ExtractElementFromSegment("ReceivedDate", Segment)

                                dtable.Rows.Add(drow)
                            Loop

                        End If

                    End If


                    If 0 < dtable.Rows.Count Then
                        rep.DatabaseTables.Add(dtable)
                    Else
                        MsgBox("No records returned.", vbExclamation, "Nothing to Report!")
                        dtable.Dispose()
                        Exit Sub
                    End If
                    '
                    dtable.Dispose()


                Case "lklManifest" ' Reprint Old Manifests
                    rep.ReportName = "DropOff_Manifest.rpt"
                    ' The following 23 lines have been copy/pasted with "reportobject" being changed to "rep"
                    If Not String.IsNullOrEmpty(rep.ReportFormula) AndAlso 0 = rep.ReportFormula.Length Then
                        If Not 0 = Me.cmbCarrier.Text.Length And Not "Select Carrier" = Me.cmbCarrier.Text Then
                            ''ol#1.2.16(10/2)... Manifest report should be separated by Air and Ground for FedEx and UPS.
                            If _Controls.Contains(Me.cmbCarrier.Text, "Ground") Then
                                rep.ReportFormula = "{DropOff_Packages.CarrierName} = '" & _Controls.Replace(Me.cmbCarrier.Text, " Ground", "") & "' And {DropOff_Packages.IsGround}"
                            ElseIf _Controls.Contains(Me.cmbCarrier.Text, "Air") Then
                                rep.ReportFormula = "{DropOff_Packages.CarrierName} = '" & _Controls.Replace(Me.cmbCarrier.Text, " Air", "") & "' And Not {DropOff_Packages.IsGround}"
                            Else
                                rep.ReportFormula = "{DropOff_Packages.CarrierName} = '" & Me.cmbCarrier.Text & "'"
                            End If
                        End If
                    Else
                        If Not 0 = Me.cmbCarrier.Text.Length And Not "Select Carrier" = Me.cmbCarrier.Text Then
                            ''ol#1.2.16(10/2)... Manifest report should be separated by Air and Ground for FedEx and UPS.
                            If _Controls.Contains(Me.cmbCarrier.Text, "Ground") Then
                                rep.ReportFormula = String.Format("{{DropOff_Packages.CarrierName}} = '{0}' and {{DropOff_Packages.IsGround}} and {1}", _Controls.Replace(Me.cmbCarrier.Text, " Ground", ""), rep.ReportFormula)
                            ElseIf _Controls.Contains(Me.cmbCarrier.Text, "Air") Then
                                rep.ReportFormula = String.Format("{{DropOff_Packages.CarrierName}} = '{0}' and Not {{DropOff_Packages.IsGround}} and {1}", _Controls.Replace(Me.cmbCarrier.Text, " Air", ""), rep.ReportFormula)
                            Else
                                rep.ReportFormula = String.Format("{{DropOff_Packages.CarrierName}} = '{0}' and {1}", Me.cmbCarrier.Text, rep.ReportFormula)
                            End If
                        End If
                    End If


                ' I THINK there should be more here??
                Case "lklCustomerProdReport" ' Production Report by Customer
                    rep.ReportName = "DropOff_byCustomer.rpt"
                    If Not String.IsNullOrEmpty(rep.ReportFormula) AndAlso 0 = rep.ReportFormula.Length Then
                        If Not 0 = Me.cmbCustomerName.Text.Length And Not "Select Customer Name" = Me.cmbCustomerName.Text Then
                            rep.ReportFormula = "{DropOff_Packages.CustomerName} = '" & Me.cmbCustomerName.Text & "'"
                        ElseIf 0 = Me.cmbCustomerName.Text.Length Then
                            rep.ReportFormula = "isnull({DropOff_Packages.CustomerName})"
                        End If
                    Else
                        If Not 0 = Me.cmbCustomerName.Text.Length And Not "Select Customer Name" = Me.cmbCustomerName.Text Then
                            rep.ReportFormula = String.Format("{{DropOff_Packages.CustomerName}} = '{0}' and {1}", Me.cmbCustomerName.Text, rep.ReportFormula)
                        ElseIf 0 = Me.cmbCustomerName.Text.Length Then
                            rep.ReportFormula = String.Format("isnull({{DropOff_Packages.CustomerName}}) and {0}", rep.ReportFormula)
                        End If
                    End If

                Case "cmdRunManifest" ' Get Manifest by carrier
                    rep.ReportName = "DropOff_Manifest.rpt"
                    Dim readyToRun As Boolean = False
                    rep.ReportParameters.Add(Today.ToString("d"))
                    ' Get carrier name to use
                    Dim carriername As String = get_Selected_Manifest_Carrier()
                    If _Controls.Contains(carriername, "FedEx") Then
                        'Dim isGround As Boolean = Windows.MessageBoxResult.Yes = _MyMsgBox.Questional("Is it Ground Manifest?", "Ground?")
                        Dim isGround As Boolean = ("FedEx Ground" = carriername)
                        ''ol#1.2.39(5/24)... If 'Run Driver Manifest' is run then Manifest is closed for that date and its date advanced to the next one.
                        If DropOffPackagesDB.IsManifest_ClosedForTheDay(Today, "FedEx", isGround) Then
                            ''ol#1.2.40(6/6)... User will be able to add packages to the already closed driver manifest if needed.
                            ''AP(08/10/2016) - Change wording of already closed driver manifest msgbox.
                            Dim Result As MessageBoxResult = MessageBox.Show("Manifest is already Closed!" & Environment.NewLine &
                                                                           "Driver has already picked up today. Do you want to Re-Open the old manifest?", "Add to Today's Manifest?", MessageBoxButton.YesNo, MessageBoxImage.Question)
                            If System.Windows.MessageBoxResult.Yes = Result Then
                                If update_ManifestDate("FedEx", isGround) Then
                                    If isGround Then
                                        rep.ReportFormula = String.Format("{{DropOff_Packages.CarrierName}} = '{0}' and {{DropOff_Packages.IsGround}} and {{DropOff_Packages.ManifestDate}} = DateTime ({1}, {2}, {3}, 0, 0, 0)", _Controls.Replace(carriername, " Ground", ""), Today.Year, Today.Month, Today.Day)
                                        readyToRun = True
                                    Else
                                        rep.ReportFormula = String.Format("{{DropOff_Packages.CarrierName}} = '{0}' and not {{DropOff_Packages.IsGround}} and {{DropOff_Packages.ManifestDate}} = DateTime ({1}, {2}, {3}, 0, 0, 0)", _Controls.Replace(carriername, " Air", ""), Today.Year, Today.Month, Today.Day)
                                        readyToRun = True
                                    End If
                                End If
                            End If
                        Else
                            Dim Result As MessageBoxResult = MessageBox.Show("Do you want to Close this Manifest for the day?", "Close Manifest?", MessageBoxButton.YesNo, MessageBoxImage.Question)
                            If System.Windows.MessageBoxResult.Yes = Result Then
                                If update_ManifestDate("FedEx", isGround) Then
                                    If isGround Then
                                        rep.ReportFormula = String.Format("{{DropOff_Packages.CarrierName}} = '{0}' and {{DropOff_Packages.IsGround}} and {{DropOff_Packages.ManifestDate}} = DateTime ({1}, {2}, {3}, 0, 0, 0)", _Controls.Replace(carriername, " Ground", ""), Today.Year, Today.Month, Today.Day)
                                        readyToRun = True
                                    Else
                                        rep.ReportFormula = String.Format("{{DropOff_Packages.CarrierName}} = '{0}' and not {{DropOff_Packages.IsGround}} and {{DropOff_Packages.ManifestDate}} = DateTime ({1}, {2}, {3}, 0, 0, 0)", _Controls.Replace(carriername, " Air", ""), Today.Year, Today.Month, Today.Day)
                                        readyToRun = True
                                    End If
                                End If
                            End If
                        End If
                    Else
                        If IsManifest_ClosedForTheDay(Today, carriername) Then
                            ''AP(08/10/2016) - Change wording of already closed driver manifest msgbox.
                            Dim Result As MessageBoxResult = MessageBox.Show("Manifest is already Closed!" & Environment.NewLine &
                                                                           "Driver has already picked up today. Do you want to Re-Open the old manifest?", "Add to Today's Manifest?",
                                                                            MessageBoxButton.YesNo, MessageBoxImage.Question)
                            If System.Windows.MessageBoxResult.Yes = Result Then
                                If update_ManifestDate(carriername) Then
                                    rep.ReportFormula = String.Format("{{DropOff_Packages.CarrierName}} = '{0}' and {{DropOff_Packages.ManifestDate}} = DateTime ({1}, {2}, {3}, 0, 0, 0)", carriername, Today.Year, Today.Month, Today.Day)
                                    readyToRun = True
                                End If
                            End If
                        Else
                            Dim Result As MessageBoxResult = MessageBox.Show("Do you want to Close this Manifest for the day?", "Close Manifest?", MessageBoxButton.YesNo, MessageBoxImage.Question)
                            If Result = System.Windows.MessageBoxResult.Yes Then
                                If update_ManifestDate(carriername) Then
                                    rep.ReportFormula = String.Format("{{DropOff_Packages.CarrierName}} = '{0}' and {{DropOff_Packages.ManifestDate}} = DateTime ({1}, {2}, {3}, 0, 0, 0)", carriername, Today.Year, Today.Month, Today.Day)
                                    readyToRun = True
                                End If
                            End If
                        End If
                    End If
                    If Not readyToRun Then
                        Debug.WriteLine("Not ready to run driver manifest")
                        Exit Sub
                    End If

            End Select
            Dim reportPrev As New ReportPreview(rep)
            reportPrev.ShowDialog()
            ' Run the report
            'If Not String.IsNullOrEmpty(rep.ReportName) Then
            'For i As Integer = 1 To GetPolicyData(gShipriteDB, "DropOffCopyCount", 1) Step 1
            'Call ShipRiteReports.Execute_ODBC(rep)
            'Next
            'End If
        Catch ex As Exception
            Debug.Print(ex.StackTrace)
        End Try
    End Sub

    Private Function get_Selected_Manifest_Carrier() As String
        Try

            Dim Carrier As String = ""
            If optUPS.IsChecked = True Then

                Carrier = "UPS"

            ElseIf optFedExAir.IsChecked = True Then

                Carrier = "FedEx Air"

            ElseIf optFedExGround.IsChecked = True Then

                Carrier = "FedEx Ground"

            ElseIf optDHL.IsChecked = True Then

                Carrier = "DHL"

            ElseIf optUSPS.IsChecked = True Then

                Carrier = "USPS"

            End If
            Return Carrier

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
        Return String.Empty
    End Function


    Private Function update_ManifestDate(carriername As String, Optional isGround As Integer = 2) As Boolean
        Dim sql2cmd As New sqlUpdate
        Dim Ground As String = ""
        If isGround <> 2 Then
            Ground = " And IsGround = " & CBool(isGround).ToString
        End If
        Dim sql2exe As String = sql2cmd.Qry_UPDATE("ManifestDate", Today.ToString("d"), sql2cmd.DTE_, True, True, "DropOff_Packages", "ManifestDate Is Null And CarrierName = '" & carriername & "'" & Ground)
        If Not DropOffPackagesDB.Execute(sql2exe) Then
            _MsgBox.ErrorMessage("Failed to update Manifest...")
            Return False
        End If
        Return True
    End Function

    Private Sub cmdEditDisclaimer_Click(sender As Object, e As RoutedEventArgs) Handles cmdEditDisclaimer.Click
        ' Open disclaimer message for editing
        Process.Start("notepad.exe", gTemplatesPath & "\DropOff_Disclaimer.txt")
    End Sub

    Private Sub cmbCustomerName_DropDownOpened(sender As Object, e As EventArgs)
        Read_CustomerNames()

    End Sub





    Private Function DropOffHasData()
        ' Check if the drop off manager has entered data (so we can warn about leaving)
        Dim hasChanged = False
        ' Package Info
        hasChanged = hasChanged Or (Not txtPackageTrackingNo.Text = "")
        hasChanged = hasChanged Or (Not txtDesc.Text = "")
        hasChanged = hasChanged Or (Not txtCustomerName.Text = "")
        ' Package List
        hasChanged = hasChanged Or (Not lvPackages.Items.Count = 0)
        Return hasChanged
    End Function

    Private Sub Carrier_ListBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles Carrier_ListBox.SelectionChanged
        chkAutoDetect.IsChecked = False
    End Sub



#End Region

End Class

Public Class DropOffInformation
    Public Property trackingNumber As String
    Public Property CustomerID As String
    Public Property CustomerName As String
    Public Property CarrierName As String
    Public Property isChecked As Boolean
    Public Property DropOffDate As String
    Public Property DropOffNotes As String
    Public Property PackagingFee As Double
End Class


Public Class DropOffPackageObject
    Public CustomerID As Long
    Public CustomerName As String
    Public CarrierName As String
    Public TrackingNo As String
    Public DropOffDate As Date
    Public ManifestDate As Date
    Public DropOffNotes As String
    Public PackagingFee As Double
End Class

Public Class DropOffCompensationObject
    Public CarrierName As String
    Public Air_Value As Double
    Public Ground_Value As Double
End Class

Public Class CarrierIcon
    Public Property Index As Integer
    Public Property CarrierName As String
    Public Property CarrierImage As String
End Class