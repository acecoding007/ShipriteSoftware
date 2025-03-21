Imports System.Windows.Forms
Imports System.Windows.Media
Imports System.IO
Imports Newtonsoft.Json.Linq
Imports System.Collections.ObjectModel
Imports System.Windows.Threading
Imports System.Text.RegularExpressions

Public Class SearchItem
    Property ID As Integer
    Property Name As String
    Property FName As String
    Property LName As String
    Property FullAddress As String
    Property Phone As String
    Property AR As String
    Property MBX As String

End Class

Public Class ContactManager
#Region "Classes"
    Inherits CommonWindow

    Private Shadows isLoaded As Boolean
    Private Property ShipperID As Long = 0
    'Private Property ProfileImage As Byte()
    'Private Property ProfileImageFile As String
#Region "Address Autocomplete"
    Public isShipTo As Boolean 'Autocomplete should only be enabled for ShipTo addresses
    Private AutoComplete_Timeout As UInt16 = 1
    Private AutoComplete_Timer As System.Threading.Timer
    Private _AutoComplete_SuggestList As ObservableCollection(Of AddressSuggestion)
    Public Property AutoComplete_SuggestList As ObservableCollection(Of AddressSuggestion)
        Get
            Return _AutoComplete_SuggestList
        End Get
        Set(value As ObservableCollection(Of AddressSuggestion))
            _AutoComplete_SuggestList = value
            NotifyPropertyChanged()
        End Set
    End Property
    Public Property AutoComplete_Enabled As Boolean
        Get
            Return GetPolicyData(gShipriteDB, "ContactManager_Autocomplete_Enable", True)
        End Get
        Set(value As Boolean)
            UpdatePolicy(gShipriteDB, "ContactManager_Autocomplete_Enable", value)
            NotifyPropertyChanged()
        End Set
    End Property
#End Region

    Private Class HotSearchItem
        Property Name As String
        Property Mailbox As String
        Property ID As Integer
        Property AR As String
        Property Addr1 As String
        Property City As String
        Property State As String
        Property Zip As String

    End Class

    Public Class AddressSuggestion
        Property DisplayText As String
        Property HouseNumber As String
        Property Street As String
        Property City As String
        Property State As String
        Property PostalCode As String
        Property CountryCode As String
    End Class
#End Region
    Public Sub New()

        MyBase.New()

        ' This call is required by the designer.
        InitializeComponent()
        Console.WriteLine("INITIALIZED")
        Me.DataContext = Me
    End Sub
    Public Sub New(ByVal callingWindow As Window, Optional ByVal CID As Long = 0, Optional ByVal SearchTerm As String = "", Optional ByVal Is_ShowShipToAddresses_For_Shipper As Boolean = False, Optional isShipToAddress As Boolean = False)

        MyBase.New(callingWindow)
        isLoaded = False
        ' This call is required by the designer.
        InitializeComponent()

        ' TEST
        Me.DataContext = Me

        isShipTo = isShipToAddress

        load_Countries()
        CountrySelection.Text = "United States"

        If CID <> 0 And Is_ShowShipToAddresses_For_Shipper = False Then
            Pull_Up_Search_CustomerByID(CID)


        ElseIf CID <> 0 And Is_ShowShipToAddresses_For_Shipper Then
            ShipperID = CID
            Search_LoadList("", Search_LB, , , Is_ShowShipToAddresses_For_Shipper, ShipperID)
            SearchResults_GroupBox.Header = "ShipTo addresses for " & ExtractElementFromSegment("Name", IO_GetSegmentSet(gShipriteDB, "Select Name from Contacts WHERE ID=" & ShipperID)) & " - " & Search_LB.Items.Count


        ElseIf SearchTerm <> "" Then
            D0.Text = SearchTerm
            Search_LoadList(SearchTerm, Search_LB)
            SearchResults_GroupBox.Header = "Search Results - " & Search_LB.Items.Count
        End If

    End Sub
    Private Sub ContactManager_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded

        Dim i As Integer = 0
        D0.Focus()

        D0.SelectionStart = D0.Text.Length
        If Not gResult = "" Then
            If Not IsNumeric(gResult) Then 'sometimes GResult can be the ContactID
                D0.Text = gResult
                i = InStr(1, gResult, ",")
                If Not i = 0 Then

                    D1.Text = Trim(Mid(gResult, i))
                    D2.Text = Trim(Mid(gResult, 1, i - 1))

                End If
            End If
        End If

        'ProfileImageFile = My.Computer.FileSystem.GetTempFileName & Guid.NewGuid().ToString

        'AutoComplete_Timeout = GetPolicyData(gShipriteDB, "ContactManager_AutoComplete_Timeout", 1) ' load variable value from db
        Select_SearchType()

        Task.Run(Sub() PostCodeLoadDB())
        isLoaded = True
    End Sub
    Private Sub load_Countries()
        For Each ctry As _CountryDB In gCountry
            CountrySelection.Items.Add(ctry)
        Next
    End Sub

    Private Sub Select_SearchType()
        Select Case My.Settings.Contacts_SearchType
            Case 1
                Contains_RadioBtn.IsChecked = True
            Case 2
                EndsWith_RadioBtn.IsChecked = True
            Case Else
                StartsWith_RadioBtn.IsChecked = True

        End Select

    End Sub

    Private Sub SearchType_SelectionChanged() Handles Contains_RadioBtn.Checked, EndsWith_RadioBtn.Checked, StartsWith_RadioBtn.Checked

        If Contains_RadioBtn.IsChecked Then
            My.Settings.Contacts_SearchType = 1
        ElseIf EndsWith_RadioBtn.IsChecked Then
            My.Settings.Contacts_SearchType = 2
        Else
            My.Settings.Contacts_SearchType = 0
        End If

    End Sub

#Region "Contact Address Controls"
    ' Load contact from DB
    Private Function DisplayCustomerSegment() As Long

        Dim i As Integer
        Dim FName As String
        Dim buf As String
        Dim segment As String
        FName = ""
        For i = 0 To 15

            Dim D As Object = Me.FindName("D" & i.ToString)
            FName = D.uid
            buf = ExtractElementFromSegment(FName, gContactManagerSegment)
            If Not buf = "" Then

                D.text = buf
                ' Change non-placeholder text black
                D.Foreground = Application.Current.MainWindow.FindResource("Black_Color")

            Else

                D.text = D.tooltip

            End If

        Next
        buf = ExtractElementFromSegment("Residential", gContactManagerSegment)
        If buf = "True" Then

            Commercial_Checkbox.IsChecked = False
            Residential_Checkbox.IsChecked = True

        Else

            Commercial_Checkbox.IsChecked = True
            Residential_Checkbox.IsChecked = False

        End If
        buf = ExtractElementFromSegment("Notes", gContactManagerSegment)
        If Not buf = "" Then

            CustomerNotes.Text = buf
        Else
            CustomerNotes.Text = ""
        End If
        IDNumber.Content = ExtractElementFromSegment("ID", gContactManagerSegment)

        CountrySelection.Text = ExtractElementFromSegment("Country", gContactManagerSegment, "United States")

        ' Pull cell carrier
        buf = ExtractElementFromSegment("CellCarrier", gContactManagerSegment)
        If Not buf = "" Then
            ' Place cell carrier
            CellCarrier_ComboBox.SelectedValue = buf
        Else
            CellCarrier_ComboBox.SelectedIndex = 0
        End If


        'Display Mailbox Number
        If ExtractElementFromSegment("MBX", gContactManagerSegment, "") = True Then
            D15.Text = ""
            buf = IO_GetSegmentSet(gShipriteDB, "Select distinct MBX from MBXNamesList Where CID=" & IDNumber.Content)

            'Same contact can have multiple mailboxes
            Do Until buf = ""
                segment = GetNextSegmentFromSet(buf)
                D15.Text = D15.Text & ExtractElementFromSegment("MBX", segment, "") & ", "
            Loop

            D15.Text = D15.Text.Trim.TrimEnd(CChar(","))

        End If

        ' Pull contact image
        'Dim image As Byte() = GetBytesFromDb(gShipriteDB, "Contacts", "ProfileImage", "ID=" & IDNumber.Content)
        'If Not image Is Nothing Then
        '    File.WriteAllBytes(ProfileImageFile, image)
        '    Profile_Image.Source = New ImageSourceConverter().ConvertFromString(ProfileImageFile)
        'End If

        Return 0

    End Function
    Private Sub Contact_Textbox_GotFocus(sender As Object, e As RoutedEventArgs) Handles D0.GotFocus, D1.GotFocus, D2.GotFocus, D3.GotFocus, D4.GotFocus, D5.GotFocus, D6.GotFocus, D7.GotFocus, D8.GotFocus, D9.GotFocus, D10.GotFocus, D11.GotFocus, D12.GotFocus, D13.GotFocus, D14.GotFocus, D15.GotFocus
        Try
            If sender.Text = sender.ToolTip Then
                sender.text = ""
                sender.Foreground = Application.Current.MainWindow.FindResource("Black_Color")
            End If

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub
    Private Sub Commercial_Checkbox_Checked(sender As Object, e As RoutedEventArgs) Handles Commercial_Checkbox.Checked
        Try
            Residential_Checkbox.IsChecked = False
            Residential_Icon.Opacity = 0.25
            Residential_Label.Opacity = 0.25
            Commercial_Icon.Opacity = 1
            Commercial_Label.Opacity = 1
        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub
    Private Sub Residential_Checkbox_Checked(sender As Object, e As RoutedEventArgs) Handles Residential_Checkbox.Checked
        Try
            Commercial_Checkbox.IsChecked = False
            Commercial_Icon.Opacity = 0.25
            Commercial_Label.Opacity = 0.25
            Residential_Icon.Opacity = 1
            Residential_Label.Opacity = 1
        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub
    Private Sub Commercial_Checkbox_Unchecked(sender As Object, e As RoutedEventArgs) Handles Commercial_Checkbox.Unchecked
        Try
            Commercial_Icon.Opacity = 0.25
            Commercial_Label.Opacity = 0.25
        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub
    Private Sub Residential_Checkbox_Unchecked(sender As Object, e As RoutedEventArgs) Handles Residential_Checkbox.Unchecked
        Try
            Residential_Icon.Opacity = 0.25
            Residential_Label.Opacity = 0.25
        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub
    'Private Sub Profile_Image_ImageFailed(sender As Object, e As ExceptionRoutedEventArgs) Handles Profile_Image.ImageFailed
    '    Try
    '        Profile_Image.Source = New BitmapImage(New Uri("Resources/Profile_Default.png", UriKind.Relative))
    '    Catch ex As Exception

    '        MessageBox.Show(Err.Description)

    '    End Try
    'End Sub
    'Private Sub Profile_Image_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles Profile_Image.MouseLeftButtonUp
    '    Try

    '        Dim selectContactImage As OpenFileDialog = New OpenFileDialog()

    '        selectContactImage.Title = "Select Image"
    '        ' Liz: replaced with the more system-correct method of pointing to the user's picture folder
    '        selectContactImage.InitialDirectory = Environment.GetEnvironmentVariable("CSIDL_MYPICTURES")
    '        selectContactImage.Filter = "Image files (*.jpg, *.jpeg, *.png, *.bmp) | *.jpg; *.jpeg; *.png; *.bmp"
    '        selectContactImage.ShowDialog()

    '        If selectContactImage.FileName = Nothing Then
    '            Exit Sub
    '        Else
    '            Profile_Image.Source = New ImageSourceConverter().ConvertFromString(selectContactImage.FileName.ToString())
    '            ProfileImage = File.ReadAllBytes(selectContactImage.FileName.ToString())
    '        End If

    '    Catch ex As Exception

    '        MessageBox.Show(Err.Description)

    '    End Try
    'End Sub
    Private Sub CountrySelection_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles CountrySelection.SelectionChanged
        If IsNothing(Address3_Row) Then Exit Sub

        Dim isUsaSelected As Boolean = True ' assume

        If CountrySelection.SelectedItem IsNot Nothing Then
            Dim country As _CountryDB = CountrySelection.SelectedItem

            If country.CountryName <> "United States" Then
                isUsaSelected = False
            End If
        End If

        If isUsaSelected Then
            Address3_Row.Height = New GridLength(0)
            D8.MaxLength = 5 ' zip code
            D7.MaxLength = 2 'state
            D14.IsEnabled = False ' Addr3 hidden - disable and skip focus
        Else
            Address3_Row.Height = New GridLength(1, GridUnitType.Star)
            D8.MaxLength = 0 ' zip code - no limit
            D7.MaxLength = 35 'state/province
            D14.IsEnabled = True ' Addr3 showing - enable
        End If
    End Sub
    Private Sub AddContact_Button_Click(sender As Object, e As RoutedEventArgs) Handles AddContact_Button.Click
        Try
             

            ' save new contact

            Dim retMsgBox As MsgBoxResult
            Dim ID As Long = CLng(IDNumber.Content)

            If ID = 0 Then ' no contact loaded
                retMsgBox = MessageBox.Show("Are you sure you want to add a new contact?", "Contact Manager", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
            Else ' contact loaded

                ' retMsgBox = MessageBox.Show("A contact is already loaded. Are you sure you want to add a new contact with the entered information?", "Contact Manager", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                retMsgBox = MessageBox.Show("This contact already exists, would you like to create a duplicate?", "Contact Manager", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
            End If

            If retMsgBox = MsgBoxResult.Yes Then
                gAutoExitFromContacts = False 'don't close contact screen if duplicating
                ' Save_Contact(Not ID = 0) ' Original version
                gContactManagerSegment = "" ' SRN-262: Fix duplication of contacts
                Save_Contact(True) ' Tryue to create new contact
                gAutoExitFromContacts = True
            End If

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub
    Private Sub RemoveContact_Button_Click(sender As Object, e As RoutedEventArgs) Handles RemoveContact_Button.Click
        Try

            If vbYes = MsgBox("Are you sure you want to delete contact?", vbQuestion + vbYesNo) Then

                Dim ID As Long = CLng(IDNumber.Content)

                If ID = 0 Then
                    MessageBox.Show("No contact loaded to remove.", "Contact Manager", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Else
                    If Remove_Contact() Then
                        Clear_Screen()
                    End If
                End If
            End If

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub
    Private Sub GoogleMaps_Button_Click(sender As Object, e As RoutedEventArgs) Handles GoogleMaps_Button.Click
        Try
            Dim searchterm As String

            'addr1
            searchterm = D4.Text

            'addr2
            If D5.Text <> "" And D5.Text <> D5.ToolTip Then
                searchterm = searchterm & " " & D5.Text
            End If

            'city
            searchterm = searchterm & " " & D6.Text

            'state
            If D7.Text <> "" And D7.Text <> D7.ToolTip Then
                searchterm = searchterm & ", " & D7.Text
            End If

            'zip
            If D8.Text <> "" And D8.Text <> D8.ToolTip Then
                searchterm = searchterm & " " & D8.Text
            End If

            'country
            searchterm = searchterm & CountrySelection.Text

            ' Use address input for Google Maps search
            Process.Start("https://www.google.com/maps/search/" & searchterm)
        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub
    Private Sub Marketing_Tools_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Marketing_Tools_Btn.Click
        If Marketing_Tools_Popup.IsOpen = True Then
            Marketing_Tools_Popup.IsOpen = False
        Else
            Marketing_Tools_Popup.IsOpen = True
        End If
    End Sub
    Private Sub CloseMarketingTools(sender As Object, e As RoutedEventArgs) Handles MarketingTools_ListBox.MouseDoubleClick
        If Marketing_Tools_Popup.IsOpen Then
            Marketing_Tools_Popup.IsOpen = False
        End If
    End Sub
    Private Sub Contact_Textbox_LostFocus(sender As Object, e As RoutedEventArgs) Handles D0.LostFocus, D1.LostFocus, D2.LostFocus, D3.LostFocus, D4.LostFocus, D5.LostFocus, D6.LostFocus, D7.LostFocus, D8.LostFocus, D9.LostFocus, D10.LostFocus, D11.LostFocus, D12.LostFocus, D13.LostFocus, D14.LostFocus, D15.LostFocus

        Dim FNumber As Integer
        Dim iloc As Integer = 0
        Dim SQL As String = ""
        Dim SegmentSet As String = ""


        FNumber = CInt(Strings.Mid(sender.name, 2))
        Try
            If sender.Text = "" Then
                sender.Text = sender.tooltip
                sender.Foreground = Application.Current.MainWindow.FindResource("Faded_Color")
            End If
            Select Case FNumber

                Case 0

                    If D0.Text = "" Then

                        Exit Sub

                    End If
                    If D1.Text = D1.ToolTip And D2.Text = D2.ToolTip And InStr(1, D0.Text, ", ") > 0 Then
                        ' Name = LName, FName -> Assume Residential
                        Residential_Checkbox.IsChecked = True
                        Commercial_Checkbox.IsChecked = False
                        iloc = InStr(1, D0.Text, ", ")
                        D2.Text = Trim(Mid(D0.Text, 0, iloc - 1))
                        D1.Text = Trim(Mid(D0.Text, iloc + 1))

                    ElseIf Residential_Checkbox.IsChecked = False And Commercial_Checkbox.IsChecked = False Then
                        ' new contact entry with no preselected commercial/residential status
                        ' Name <> LName, FName -> Assume Commercial
                        Residential_Checkbox.IsChecked = False
                        Commercial_Checkbox.IsChecked = True

                    ElseIf D0.Text = D2.Text & ", " & D1.Text Then
                        'existing contact -> Name = LName, FName
                        Residential_Checkbox.IsChecked = True
                        Commercial_Checkbox.IsChecked = False

                    End If

                Case 8

                    If Not D8.Text = "" And D6.Text.Length >= 5 Then

                        'SQL = "SELECT City, ST FROM ZipCodes WHERE Zip = '" & D8.Text & "'"
                        'SegmentSet = IO_GetSegmentSet(gZipCodeDB, SQL)
                        'D6.Text = ExtractElementFromSegment("City", SegmentSet)
                        'D7.Text = ExtractElementFromSegment("ST", SegmentSet)
                        If CountrySelection.SelectedItem IsNot Nothing AndAlso CountrySelection.SelectedItem.CountryName = "United States" Then

                            Dim postalcode As String = D8.Text
                            Dim countryName As String = CountrySelection.SelectedItem.CountryName
                            Task.Run(Sub() PostCodeControl(postalcode, countryName)) ' starts function on separate thread - continues execution
                        End If
                        'D6.Text = ApiRequest.PostCodeToCity(D8.Text)
                        'D7.Text = ApiRequest.PostCodeToState(D8.Text)
                        'D6.Foreground = Application.Current.MainWindow.FindResource("Black_Color")
                        'D7.Foreground = Application.Current.MainWindow.FindResource("Black_Color")
                        D9.Focus()

                    End If

                Case 9, 10, 11, 12

                    If Not sender.text = sender.tooltip Then

                        If CountrySelection.SelectedItem IsNot Nothing AndAlso CountrySelection.SelectedItem.CountryName = "United States" Then
                            sender.text = ReformatPhone(gShipriteDB, sender.text)
                        End If
                    End If

                        Case 1, 2

                    Dim BuiltName As String = ""
                    If Not D1.Text = D1.ToolTip And Not String.IsNullOrEmpty(D1.Text) Then
                        BuiltName = D1.Text
                    End If
                    If Not D2.Text = D2.ToolTip And Not String.IsNullOrEmpty(D2.Text) Then
                        If Not BuiltName = "" Then
                            BuiltName = D2.Text & ", " & BuiltName
                        Else
                            BuiltName = D2.Text
                        End If
                    End If
                    If D0.Text = D0.ToolTip Or D0.Text = "" Or D0.Text = D1.Text Or D0.Text = D2.Text Or D0.Text = D2.Text & ", " & D1.Text Then
                        D0.Text = BuiltName
                    End If

                Case 4 ' Address Line 1

                    If AutoComplete_Timer IsNot Nothing Then
                        AutoComplete_Timer.Dispose()
                    End If

            End Select

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try

    End Sub
    Private Sub D_KeyDown_Processor(sender As Object, e As Input.KeyEventArgs) Handles D0.PreviewKeyDown, D1.PreviewKeyDown, D2.PreviewKeyDown, D3.PreviewKeyDown, D4.PreviewKeyDown,
        D5.PreviewKeyDown, D6.PreviewKeyDown, D7.PreviewKeyDown, D8.PreviewKeyDown, D9.PreviewKeyDown, D10.PreviewKeyDown, D11.PreviewKeyDown, D12.PreviewKeyDown, D13.PreviewKeyDown,
        D14.PreviewKeyDown, D15.PreviewKeyDown

        Dim sel_start As Integer
        Dim sel_length As Integer
        Dim DBField As String
        Dim FNumber As Integer
        Dim SearchTerm As String

        Select Case sender.name
            Case "D0"
                If e.Key = Key.Down Then
                    'down arrow sets focus on the HotSearch ListView
                    If HotSearch_LV.Items.Count > 0 Then
                        HotSearch_LV.SelectedIndex = 0
                        HotSearch_LV.Focus()
                    End If

                ElseIf e.Key = Key.Tab Then
                    'Tabbing to the next textbox should close the HotSearch popup
                    HotSearch_Popup.IsOpen = False
                End If
            Case "D4"
                If e.Key = Key.Down Then
                    'down arrow sets focus on the Address Autocomplete ListView
                    If Address_List.Items.Count > 0 Then
                        Address_List.SelectedIndex = 0
                        Address_List.Focus()
                    End If

                ElseIf e.Key = Key.Tab Then
                    'Tabbing to the next textbox should close the Address Autocomplete popup
                    AddressAutocomplete_Popup.IsOpen = False
                End If
        End Select

        If e.Key = Key.Return Then
            'Enter key will search the database and populate the Search Listbox
            SearchTerm = sender.Text
            DBField = sender.uid

            If SearchTerm <> "" Then

                If ShipperID <> 0 Then
                    'searching list of ShipTo Addresses for selected Shipper.
                    Search_LoadList(GetSearchTypeSeed(SearchTerm, False), Search_LB, , DBField, True, ShipperID)
                    SearchResults_GroupBox.Header = "ShipTo addresses for " & ExtractElementFromSegment("Name", IO_GetSegmentSet(gShipriteDB, "Select Name from Contacts WHERE ID=" & ShipperID)) & Search_LB.Items.Count
                Else

                    Search_LoadList(GetSearchTypeSeed(SearchTerm, False), Search_LB, , DBField)
                    SearchResults_GroupBox.Header = "Search Results - " & Search_LB.Items.Count
                End If

                HotSearch_Popup.IsOpen = False
                Search_LB.Focus()

            End If
        End If

        If e.Key = Key.Right Then
            'Right arrow sets focus on the Search ListBox
            If Search_LB.Items.Count > 0 Then
                Search_LB.SelectedIndex = 0
                Search_LB.Focus()
            End If
        End If

        ' Setup formatting
        Dim tb As Controls.TextBox = Nothing
        If TypeOf sender Is Controls.AutoCompleteBox Then
            '
            Dim ac As Controls.AutoCompleteBox = TryCast(sender, Controls.AutoCompleteBox)
            tb = TryCast(VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(ac, 0), 0), Controls.TextBox)
            '
        ElseIf TypeOf sender Is Controls.TextBox Then
            '
            tb = TryCast(sender, Controls.TextBox)
            '
        End If
        '
        If tb IsNot Nothing AndAlso TypeOf tb Is Controls.TextBox Then
            '
            sel_start = tb.SelectionStart
            sel_length = tb.SelectionLength
            FNumber = CInt(Strings.Mid(sender.name, 2))

            Select Case FNumber
                Case 0, 1, 2, 4, 5, 6, 14

                    'break up user entry into individual words
                    Dim TextArray() As String = tb.Text.Split(" ")
                    For i = 0 To UBound(TextArray)

                        'if user typed any captialized characters, then do not proper case that word. This will leave the user's capitalization in tact.
                        If Not TextArray(i).Any(AddressOf Char.IsUpper) Then
                            TextArray(i) = StrConv(TextArray(i), VbStrConv.ProperCase)
                        End If

                    Next i
                    tb.Text = String.Join(" ", TextArray)

                    'tb.Text = StrConv(tb.Text, VbStrConv.ProperCase)

                Case 3, 7, 8

                    tb.Text = StrConv(tb.Text, VbStrConv.Uppercase)

            End Select
            tb.SelectionStart = sel_start
            tb.SelectionLength = sel_length
            '
        End If

    End Sub
    Public Sub AddressAutocompleteHandler(sender As Object, e As Input.KeyEventArgs) Handles D4.KeyUp
        ' Address Autocomplete on address line 1
        If isShipTo AndAlso AutoComplete_Enabled AndAlso CountrySelection.SelectedItem IsNot Nothing AndAlso sender.name = "D4" And isLoaded = True And Not D4.Text = D4.ToolTip And D4.Text.Length > 3 Then
            Dim AutoComplete_Input As String = D4.Text
            AddressAutoComplete(AutoComplete_Input, CountrySelection.SelectedItem.CountryName)
        End If
    End Sub
    Public Sub ZipCitySuggestion(sender As Object, e As RoutedEventArgs) Handles D8.KeyUp
        If CountrySelection.SelectedItem IsNot Nothing AndAlso CountrySelection.SelectedItem.CountryName = "United States" Then
            ' Zip --> City
            Dim postalcode As String = D8.Text
            Dim countryName As String = CountrySelection.SelectedItem.CountryName
            If postalcode.Length = 5 Then
                Debug.Print("Trigger Post code thread")
                Task.Run(Sub() PostCodeControl(postalcode, countryName)) ' starts function on separate thread - continues execution
            End If
        End If
    End Sub
    Public Shared Sub Search_LoadList(ByVal SearchTerm As String, ByRef Search_LB As Controls.ListBox, Optional ByVal DisplayCount As Integer = 0, Optional ByVal DBField As String = "Name", Optional ByVal Is_ShowShipToAddresses_For_Shipper As Boolean = False, Optional ShowShipToAddresses_For_Shipper_SID As String = "")
        Dim SearchList As List(Of SearchItem)
        Dim Item As SearchItem
        Dim SQL As String
        Dim SegmentSet As String
        Dim Segment As String
        Dim TopNumberSQL As String

        If Not SearchTerm.Contains("%") And Is_ShowShipToAddresses_For_Shipper = False Then
            'if wildcard is not included, then add default
            SearchTerm = SearchTerm & "%"
        End If

        SearchTerm = SearchTerm.Replace("'", "''")


        If DisplayCount = 0 Then
            TopNumberSQL = ""
        Else
            'Limits number of results displayed
            TopNumberSQL = " TOP " & DisplayCount
        End If

        Dim AmIAPhone As Boolean = IsStringPhoneNumber(SearchTerm)
        ' Add Phone number support
        If Is_ShowShipToAddresses_For_Shipper Then
            'showing shipTo addresses for specific shipper
            SQL = "SELECT DISTINCT Manifest.SID, Contacts.ID, Contacts.Name, Contacts.MBX, Contacts.AR, Contacts.FullAddress, Contacts.Phone " &
                              "FROM Manifest INNER JOIN Contacts ON Manifest.CID = Contacts.ID " &
                              "Where Manifest.SID = " & ShowShipToAddresses_For_Shipper_SID & " "

            If SearchTerm <> "" Then
                If DBField = "Name" And AmIAPhone Then
                    SearchTerm = PrepareSearchPhone(SearchTerm)
                    SQL = SQL & "AND Contacts.CellPhone LIKE " & SearchTerm & " OR Contacts.Phone LIKE " & SearchTerm & " OR Contacts.Phone2 LIKE " & SearchTerm
                Else
                    SQL = SQL & "AND Contacts." & DBField & " LIKE '" & SearchTerm & "' "
                End If
            End If
            SQL = SQL & "ORDER BY Contacts.Name"


        Else
            If DBField = "Name" And AmIAPhone Then
                SearchTerm = PrepareSearchPhone(SearchTerm)
                SQL = "SELECT" & TopNumberSQL & " Contacts.ID, Contacts.Name, Contacts.MBX, Contacts.AR, Contacts.FullAddress, Contacts.Phone, Mailbox.MailboxNumber " &
                    "FROM Contacts LEFT JOIN Mailbox ON Contacts.ID = Mailbox.CID WHERE Contacts.Phone LIKE " & SearchTerm & " OR Contacts.CellPhone LIKE " & SearchTerm &
                    " OR Contacts.Phone2 LIKE " & SearchTerm & " ORDER BY Contacts.Name"

            ElseIf DBField = "MailboxNo" Then
                SearchTerm = SearchTerm.TrimEnd(CChar("%")).TrimStart(CChar("%"))
                SQL = "SELECT" & TopNumberSQL & " Contacts.ID, Contacts.Name, Contacts.MBX, Contacts.AR, Contacts.FullAddress, Contacts.Phone, MBXNamesList.MBX " &
                "FROM Contacts LEFT JOIN MBXNamesList ON Contacts.ID = MBXNamesList.CID WHERE MBXNamesList.MBX = " & SearchTerm & " ORDER BY Contacts.Name"
            Else
                SQL = "SELECT" & TopNumberSQL & " Contacts.ID, Contacts.Name, Contacts.MBX, Contacts.AR, Contacts.FullAddress, Contacts.Phone, Mailbox.MailboxNumber " &
                "FROM Contacts LEFT JOIN Mailbox ON Contacts.ID = Mailbox.CID WHERE Contacts." & DBField & " LIKE '" & SearchTerm & "' ORDER BY Contacts.Name"
            End If
        End If
        SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
        SearchList = New List(Of SearchItem)

        If SegmentSet <> "" Then

            Do Until SegmentSet = ""
                Item = New SearchItem
                Segment = GetNextSegmentFromSet(SegmentSet)

                Item.ID = ExtractElementFromSegment("ID", Segment)
                Item.Name = ExtractElementFromSegment("Name", Segment)
                Item.FullAddress = ExtractElementFromSegment("FullAddress", Segment)
                Item.Phone = ExtractElementFromSegment("Phone", Segment)


                Item.AR = ExtractElementFromSegment("AR", Segment, "")
                If Item.AR <> "" Then Item.AR = "AR:     " & Item.AR

                Item.MBX = ExtractElementFromSegment("MailboxNumber", Segment, "")
                If Item.MBX <> "" Then Item.MBX = "MBX: " & Item.MBX

                SearchList.Add(Item)
            Loop

        End If

        Search_LB.ItemsSource = SearchList
        Search_LB.Items.Refresh()

    End Sub
    Private Sub D0_TextChanged(sender As Object, e As TextChangedEventArgs) Handles D0.TextChanged
        'Displays HotSearch drop down
        If D0.Text <> "" And D0.Text <> D0.ToolTip And isLoaded = True Then
            HotSearch_LoadList(GetSearchTypeSeed(D0.Text.Replace("'", "''")), HotSearch_LV)

            If HotSearch_LV.Items.Count > 0 Then
                HotSearch_Popup.IsOpen = True
            Else
                HotSearch_Popup.IsOpen = False
            End If

            If InStr(1, D0.Text, ", ") > 0 Then
                'first name last name entered, tab straight to Add1 line.
                D1.IsTabStop = False
                D2.IsTabStop = False
            Else
                'company name entered, tab to first name field.
                D1.IsTabStop = True
                D2.IsTabStop = True
            End If

        End If

    End Sub
    Private Sub HotSearch_LoadList(ByRef SearchText As String, ByRef HotSearch_LV As Controls.ListView)

        Dim SQL As String
        Dim Segment As String
        Dim SegmentSet As String
        Dim HotSearchList As List(Of HotSearchItem)
        Dim Item As HotSearchItem
        Dim IsPhoneNumber As Boolean = IsStringPhoneNumber(SearchText)

        If ShipperID <> 0 Then
            'showing shipTo addresses for specific shipper

            SQL = "SELECT DISTINCT TOP 35 Manifest.SID, Contacts.ID, Contacts.Name, Contacts.MBX, Contacts.AR " &
                             "FROM Manifest INNER JOIN Contacts ON Manifest.CID = Contacts.ID " &
                             "Where Manifest.SID = " & ShipperID & " AND Contacts.Name LIKE " & SearchText & " ORDER BY Contacts.Name"
            If IsPhoneNumber Then
                SearchText = PrepareSearchPhone(SearchText)
                ' check "Phone" or "CellPhone"
                SQL = "SELECT DISTINCT TOP 35 Manifest.SID, Contacts.ID, Contacts.Name, Contacts.MBX, Contacts.AR " &
                             "FROM Manifest INNER JOIN Contacts ON Manifest.CID = Contacts.ID " &
                             "Where Manifest.SID = " & ShipperID & " AND Contacts.Phone LIKE " & SearchText & "OR Contacts.CellPhone LIKE " & SearchText & "OR Contacts.Phone2 LIKE " & SearchText & " ORDER BY Contacts.Name"
            End If

        Else
            'SQL = "SELECT TOP 35 Contacts.ID, Contacts.Name, Contacts.MBX, Contacts.AR, Contacts.Addr1, Contacts.City, Contacts.State, Contacts.Zip, Mailbox.MailboxNumber " &
            '   "From Contacts LEFT Join Mailbox On Contacts.ID = Mailbox.CID Where Contacts.Name LIKE " & SearchText & " Order By Contacts.Name"

            SQL = "SELECT TOP 35 Contacts.ID, Contacts.Name, Contacts.MBX, Contacts.AR, Contacts.Addr1, Contacts.City, Contacts.State, Contacts.Zip, MBXNamesList.MBX as MailboxNumber " &
                "From Contacts LEFT Join MBXNamesList On Contacts.ID = MBXNamesList.CID Where Contacts.Name LIKE " & SearchText & " Order By Contacts.Name"


            If IsPhoneNumber Then
                SearchText = PrepareSearchPhone(SearchText)
                SQL = "SELECT TOP 35 Contacts.ID, Contacts.Name, Contacts.MBX, Contacts.AR, Contacts.Addr1, Contacts.City, Contacts.State, Contacts.Zip, MBXNamesList.MBX as MailboxNumber " &
                    "From Contacts LEFT Join MBXNamesList On Contacts.ID = MBXNamesList.CID Where Contacts.Phone LIKE " & SearchText & "OR Contacts.CellPhone LIKE " & SearchText & "OR Contacts.Phone2 LIKE " & SearchText & " Order By Contacts.Name"
            End If
        End If
        Debug.Print(SQL)

        SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)

        If SegmentSet <> "" Then
            HotSearchList = New List(Of HotSearchItem)

            Do Until SegmentSet = ""
                Item = New HotSearchItem
                Segment = GetNextSegmentFromSet(SegmentSet)

                Item.ID = ExtractElementFromSegment("ID", Segment)
                Item.Name = ExtractElementFromSegment("Name", Segment)
                Item.Mailbox = ExtractElementFromSegment("MailboxNumber", Segment)
                Item.AR = ExtractElementFromSegment("AR", Segment)

                'Item.Addr1 = ExtractElementFromSegment("Addr1", Segment)
                'Item.City = ExtractElementFromSegment("City", Segment)
                'Item.State = ExtractElementFromSegment("State", Segment)
                'Item.Zip = ExtractElementFromSegment("Zip", Segment)

                HotSearchList.Add(Item)
            Loop

            HotSearch_LV.ItemsSource = HotSearchList
            HotSearch_LV.Items.Refresh()


        Else
            HotSearch_LV.ItemsSource = Nothing
            HotSearch_LV.Items.Refresh()
        End If


    End Sub
    Private Shared Function PrepareSearchPhone(Input As String)
        ' add % between each char
        Debug.Print(Input)
        Input = Regex.Replace(Input, "[^0-9]", "")
        Input = Input.InsertEveryNthChar("%", 1)
        Return "'" & Input & "'"
    End Function
    Public Shared Function IsStringPhoneNumber(input As String) As Boolean
        input = Regex.Replace(input, "[^0-9]", "")
        If input.Length <= 16 And IsNumeric(input) And input.Length >= 7 Then
            Return True
        Else
            Return False
        End If
    End Function
    Private Function GetSearchTypeSeed(ByVal searchTerm As String, Optional ByVal isIncludeQuotes As Boolean = True) As String

        Const sqlWildCard As String = "%"
        Dim retSeed As String : retSeed = searchTerm & sqlWildCard ' default

        If IsNothing(StartsWith_RadioBtn) Then
            Return ""
        End If

        If StartsWith_RadioBtn.IsChecked = True Then
            retSeed = searchTerm & sqlWildCard
        ElseIf Contains_RadioBtn.IsChecked = True Then
            retSeed = sqlWildCard & searchTerm & sqlWildCard
        ElseIf EndsWith_RadioBtn.IsChecked = True Then
            retSeed = sqlWildCard & searchTerm
        End If


        If isIncludeQuotes Then retSeed = "'" & retSeed & "'"

        GetSearchTypeSeed = retSeed

    End Function
    Private Sub Pull_Up_Search_CustomerByID(ByVal CID As String)
        Dim SQL As String

        SQL = "SELECT * FROM Contacts WHERE ID = " & CID
        gContactManagerSegment = IO_GetSegmentSet(gShipriteDB, SQL)
        DisplayCustomerSegment()
        'SaveButton.Visibility = Visibility.Visible
        Submit.Focus()

        HotSearch_Popup.IsOpen = False
    End Sub
    Private Sub Search_LB_KeyDown(sender As Object, e As Input.KeyEventArgs) Handles Search_LB.KeyDown
        'pull up
        If Search_LB.SelectedIndex = -1 Then Exit Sub

        If e.Key = Key.Return Then
            Submit.RaiseEvent(New RoutedEventArgs(Primitives.ButtonBase.ClickEvent))
        End If
    End Sub
    Private Sub Search_LB_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles Search_LB.MouseDoubleClick
        Submit.RaiseEvent(New RoutedEventArgs(Primitives.ButtonBase.ClickEvent))
    End Sub
    Private Sub AddressAutocomplete_Click(sender As Object, e As MouseButtonEventArgs) Handles Address_List.MouseDoubleClick
        If Address_List.SelectedIndex > -1 Then
            AddressAutocomplete_PopulateData(Address_List.SelectedItem)
        End If
    End Sub
    Private Sub AddressAutocomplete_Return(sender As Object, e As Input.KeyEventArgs) Handles Address_List.KeyDown
        If e.Key = Key.Return And Address_List.SelectedIndex > -1 Then
            AddressAutocomplete_PopulateData(Address_List.SelectedItem)
        ElseIf e.Key = Key.Escape Then
            ' Close popup
            AddressAutocomplete_Popup.IsOpen = False
        End If
    End Sub
    Private Sub Search_LB_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles Search_LB.SelectionChanged
        If Search_LB.SelectedIndex = -1 Then Exit Sub

        Dim item As SearchItem = Search_LB.SelectedItem
        Pull_Up_Search_CustomerByID(item.ID)
    End Sub
    Private Sub HotSearch_LV_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles HotSearch_LV.MouseDoubleClick
        If HotSearch_LV.SelectedIndex = -1 Then Exit Sub

        Dim item As HotSearchItem = HotSearch_LV.SelectedItem
        Pull_Up_Search_CustomerByID(item.ID)
    End Sub
    Private Sub HotSearch_LV_KeyDown(sender As Object, e As Input.KeyEventArgs) Handles HotSearch_LV.KeyDown
        If e.Key = Key.Return Then

            Dim item As HotSearchItem = HotSearch_LV.SelectedItem
            Pull_Up_Search_CustomerByID(item.ID)
        End If
    End Sub
    Private Overloads Sub RefreshButton_Click(sender As Object, e As RoutedEventArgs)
        Clear_Screen()
    End Sub

    Private Sub Clear_Screen()
        'clears screen
        Dim i As Integer
        For i = 0 To 15

            Dim D As Object = Me.FindName("D" & i.ToString)
            D.text = D.tooltip
            ' TODO: Set all back to gray
            D.Foreground = Application.Current.MainWindow.FindResource("Faded_Color") ' "#FF78777F"
        Next
        Commercial_Checkbox.IsChecked = False
        Residential_Checkbox.IsChecked = False
        IDNumber.Content = "00000"
        CountrySelection.Text = "United States"
        'SaveButton.Visibility = Visibility.Hidden
        Search_LB.ItemsSource = Nothing
        Search_LB.Items.Refresh()

        CustomerNotes.Text = ""
        CellCarrier_ComboBox.SelectedIndex = 0
        gContactManagerSegment = ""

    End Sub
    Private Sub SearchCountry_Btn_Click(sender As Object, e As RoutedEventArgs) Handles SearchCountry_Btn.Click
        If CountrySelection.Text <> "" Then
            Search_LoadList(CountrySelection.Text, Search_LB, 250, "Country")
            HotSearch_Popup.IsOpen = False
            SearchResults_GroupBox.Header = "Search Results - " & Search_LB.Items.Count
            Search_LB.Focus()
        End If
    End Sub
    ' Save Contact to DB
    Private Sub Save_Contact(Optional ByVal isAddNewContact As Boolean = False)
        'Save changes to current contact.
        Dim UpdateOnly As Boolean
        Dim SQL As String
        Dim Segment As String
        Dim ID As Long
        Dim ret As Long
        Try
            If isAddNewContact Then
                IDNumber.Content = "00000"
            End If
            ID = CLng(IDNumber.Content)
            If ID = 0 Then ' insert
                UpdateOnly = False
                SQL = "SELECT MAX(ID) AS MaxID FROM Contacts"
                Segment = IO_GetSegmentSet(gShipriteDB, SQL)
                ID = CLng(ExtractElementFromSegment("MaxID", Segment)) + 1
                IDNumber.Content = ID.ToString
            Else ' update
                UpdateOnly = True
            End If
            SQL = ""
            Segment = ""
            Segment = Add_ContactInfo_ToSegment()
            If UpdateOnly = False Then
                Segment = AddElementToSegment(Segment, "FirstDate", DateTime.Today.ToShortDateString)
            End If
            If Not Segment = "" Then
                If UpdateOnly Then
                    SQL = MakeUpdateSQLFromSchema("Contacts", Segment, gContactsTableSchema, , True)
                Else
                    SQL = MakeInsertSQLFromSchema("Contacts", Segment, gContactsTableSchema, True)
                End If
                ' Update Profile Image.
                'If Not ProfileImage Is Nothing Then
                '    If Not UpdateBytesToDb(gShipriteDB, ProfileImage, "Contacts", "ProfileImage", "ID=" & ID) Then
                '        Debug.WriteLine("Error adding image")
                '    Else
                '        Debug.WriteLine("Image upload success")
                '    End If
                'Else
                '    Debug.WriteLine("No new image to upload")
                'End If
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                If ret > 0 Then
                    If UpdateOnly Then
                        If Not gAutoExitFromContacts Or ThereIsSomethingToSave() Then
                            MessageBox.Show("Contact updated successfully!", "Contact Manager", MessageBoxButtons.OK, MessageBoxIcon.Information)
                        End If
                        ' SRN-381 commented out because Mark doesn't like it
                        'Else
                        '    MessageBox.Show("Contact added successfully!", "Contact Manager", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    End If
                    IDNumber.Content = ID.ToString
                    If Not D3.Text = "" And Not D3.Text = D3.ToolTip Then
                        SQL = "SELECT ID FROM AR WHERE AcctNum = '" & D3.Text & "'"
                        Segment = IO_GetSegmentSet(gShipriteDB, SQL)
                        If Segment = "" Then
                            SQL = "SELECT MAX(ID) AS MaxID FROM AR"
                            Segment = IO_GetSegmentSet(gShipriteDB, SQL)
                            ID = Val(ExtractElementFromSegment("MaxID", Segment)) + 1
                            Segment = ""
                            Segment = AddElementToSegment(Segment, "ID", ID.ToString)
                            Segment = AddElementToSegment(Segment, "AcctNum", D3.Text)
                            Segment = AddElementToSegment(Segment, "AcctName", D0.Text)
                            Segment = AddElementToSegment(Segment, "FName", D1.Text)
                            Segment = AddElementToSegment(Segment, "LName", D2.Text)
                            Segment = AddElementToSegment(Segment, "Addr1", D4.Text)
                            Segment = AddElementToSegment(Segment, "Addr2", D5.Text)
                            Segment = AddElementToSegment(Segment, "City", D6.Text)
                            Segment = AddElementToSegment(Segment, "State", D7.Text)
                            Segment = AddElementToSegment(Segment, "ZipCode", D8.Text)
                            Segment = AddElementToSegment(Segment, "Phone", D9.Text) ' Actually "Home"
                            Segment = AddElementToSegment(Segment, "Fax", D11.Text)
                            Segment = AddElementToSegment(Segment, "SName", D0.Text)
                            Segment = AddElementToSegment(Segment, "SAddr1", D4.Text)
                            Segment = AddElementToSegment(Segment, "SCity", D6.Text)
                            Segment = AddElementToSegment(Segment, "SST", D7.Text)
                            Segment = AddElementToSegment(Segment, "SZip", D8.Text)
                            Segment = AddElementToSegment(Segment, "CellCarrier", IIf(Not CellCarrier_ComboBox.Text = CellCarrier_ComboBox.ToolTip, CellCarrier_ComboBox.Text, ""))
                            Segment = AddElementToSegment(Segment, "CellPhone", D12.Text)
                            Segment = AddElementToSegment(Segment, "EMail", D13.Text)
                            Segment = AddElementToSegment(Segment, "Phone2", D10.Text) ' Actually "work"
                            If isShipTo Then
                                Segment = AddElementToSegment(Segment, "Class", "Consignee")
                            Else
                                Segment = AddElementToSegment(Segment, "Class", "Shipper")
                            End If

                            Dim Residential As Boolean = False
                            Residential = (Residential_Checkbox.IsChecked = True)

                            Segment = AddElementToSegment(Segment, "Residential", Residential.ToString)
                            SQL = MakeInsertSQLFromSchema("AR", Segment, gARTableSchema, True)
                            ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                        End If
                    End If
                Else
                    MessageBox.Show("Error adding/updating contact to database.", "Contact Manager", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End If
            Else
                If gAutoExitFromContacts = True Then
                    ret = 1
                Else
                    MessageBox.Show("No contact information found to save.", "Contact Manager", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                End If
            End If
            If Not Val(IDNumber.Content) = 0 Then
                gResult = IDNumber.Content
                SQL = "SELECT * FROM Contacts WHERE ID = " & gResult
                gContactManagerSegment = IO_GetSegmentSet(gShipriteDB, SQL)
            End If
            If ret > 0 And gAutoExitFromContacts Then
                gResult = ""
                gResult2 = ""
                Me.Close()
            End If
        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub
    Private Function Remove_Contact()

        Dim ID As Long = CLng(IDNumber.Content)
        Dim SQL As String
        Dim ret As Long

        If ID = 0 Then ' no contact selected
            MessageBox.Show("Failed to remove contact." & vbCrLf & "No contact loaded.", "Contact Manager", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            Return False
        Else
            SQL = "DELETE * FROM Contacts WHERE ID = " & ID
            ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
            If ret > 0 Then
                MessageBox.Show("Contact removed successfully!", "Contact Manager", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return True
            Else
                MessageBox.Show("Error removing contact from database.", "Contact Manager", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                Return False
            End If
        End If
    End Function
    Private Function Add_ContactInfo_ToSegment()

        Dim Segment As String = ""
        Dim FName As String = ""
        Dim ID As Long
        Dim currentValue As String = ""
        Dim isContactChanged As Boolean = False

        Try

            ID = CLng(IDNumber.Content)
            Segment = AddElementToSegment(Segment, "ID", ID.ToString)   'This makes sure ID will be first

            For i As Integer = 0 To 14

                Dim D As Object = Me.FindName("D" & i.ToString)
                FName = D.uid
                ' this is where you need to fix it for duplication of contacts
                currentValue = ExtractElementFromSegment(FName, gContactManagerSegment)
                If Not D.text = currentValue Then
                    'entry changed

                    If D.text = "" Or D.text = D.tooltip Then
                        Segment = AddElementToSegment(Segment, FName, "")
                    Else
                        Segment = AddElementToSegment(Segment, FName, D.text)
                    End If

                    isContactChanged = True

                End If



            Next

            If isContactChanged = True Then
                'Full Address
                Segment = AddElementToSegment(Segment, "FullAddress", Create_Full_Address_Field())
            End If

            'Country
            currentValue = ExtractElementFromSegment("Country", gContactManagerSegment)
            If Not CountrySelection.Text = currentValue Then

                Segment = AddElementToSegment(Segment, "Country", CountrySelection.Text)
                isContactChanged = True
            End If

            'Residential/Commercial
            If Commercial_Checkbox.IsChecked = True Then
                currentValue = ExtractElementFromSegment("Residential", gContactManagerSegment)
                If Not "False" = currentValue Then

                    Segment = AddElementToSegment(Segment, "Residential", "False")
                    isContactChanged = True
                End If
            Else
                currentValue = ExtractElementFromSegment("Residential", gContactManagerSegment)
                If Not "True" = currentValue Then

                    Segment = AddElementToSegment(Segment, "Residential", "True")
                    isContactChanged = True
                End If
            End If

            'Notes

            currentValue = ExtractElementFromSegment("Notes", gContactManagerSegment, "")
            If Not CustomerNotes.Text = currentValue Then
                Segment = AddElementToSegment(Segment, "Notes", CustomerNotes.Text)
                isContactChanged = True
            End If

            ' Cell Carrier
            currentValue = ExtractElementFromSegment("CellCarrier", gContactManagerSegment, "")
            If Not currentValue = CellCarrier_ComboBox.Text And Not CellCarrier_ComboBox.Text = CellCarrier_ComboBox.ToolTip Then
                Segment = AddElementToSegment(Segment, "CellCarrier", CellCarrier_ComboBox.Text)
                isContactChanged = True
            End If


            'Class
            If isShipTo Then
                Segment = AddElementToSegment(Segment, "Class", "Consignee")
            Else
                Segment = AddElementToSegment(Segment, "Class", "Shipper")
            End If

        Catch ex As Exception

            MessageBox.Show(Err.Description)
            Segment = ""

        End Try

        If Not isContactChanged Then
            Segment = ""
        End If
        Return Segment

    End Function
    Private Function Create_Full_Address_Field() As String
        Dim field As String = ""

        'Addr1
        If Not D4.Text = D4.ToolTip Then
            field = D4.Text & vbCrLf
        End If

        'Addr2
        If D5.Text <> "" And Not D5.Text = D5.ToolTip Then
            field = field & D5.Text & vbCrLf
        End If

        'Addr3
        If D14.Text <> "" And Not D14.Text = D14.ToolTip Then
            field = field & D14.Text & vbCrLf
        End If

        'City
        If Not D6.Text = D6.ToolTip Then
            field = field & D6.Text
        End If

        'State
        If D7.Text <> "" And Not D7.Text = D7.ToolTip Then
            field = field & ", " & D7.Text
        End If

        'Zip
        If Not D8.Text = D8.ToolTip Then
            field = field & " " & D8.Text
        End If

        Return field

    End Function
    Private Sub Options_Button_Click(sender As Object, e As RoutedEventArgs) Handles Options_Button.Click
        SearchOptions_Popup.IsOpen = True
    End Sub
    Private Sub SubmitContact(sender As Object, e As RoutedEventArgs) Handles Submit.Click

        ' SRN-404: fix duplicate contacts issue
        'If Not D0.Text = D0.ToolTip AndAlso Not D0.Text = "" Then
        '    ' Main name field is filled
        '    'If (D1.Text = D1.ToolTip Or D1.Text = "") And (D2.Text = D2.ToolTip Or D2.Text = "") Then ' Commented out because it interferes with "First, Last" listings
        '    If IDNumber.Content = "00000" Then ' ID is default AKA. contact not loaded into form
        '        ' First/Last name fields are blank
        '        If Search_LB.Items.Count = 1 Then
        '            ' Only one result in search view
        '            Pull_Up_Search_CustomerByID(Search_LB.Items.GetItemAt(0).ID)
        '            ' return it.
        '        ElseIf Search_LB.Items.Count > 1 Then
        '            If Search_LB.SelectedIndex = -1 Then
        '                ' Tell user to select a contact in the search results
        '                _MsgBox.InformationMessage("Please select a contact from the search results first", msgboxTitle:="Contact Manager")
        '                Return
        '            Else
        '                Pull_Up_Search_CustomerByID(Search_LB.SelectedItem.ID)
        '            End If
        '        End If
        '    End If
        'End If

        If IsNothing(CountrySelection.SelectedItem) Then
            MsgBox("No Country Selected. Please select a country first!", vbExclamation)
            Exit Sub
        End If


        If IDNumber.Content = "00000" And (D0.Text = D0.ToolTip Or D0.Text = "") And D1.Text = D1.ToolTip And D2.Text = D2.ToolTip Then

            If gAutoExitFromContacts = False Then

                MsgBox("ATTENTION...Nothing to Save.")

            Else

                Me.Close()

            End If

        Else

            If IDNumber.Content = "00000" Then
                'add new contact
                Call Save_Contact(True)
            Else
                If ThereIsSomethingToSave() Then
                    Call Save_Contact()
                End If
            End If


            If gContactManagerSegment <> "" Then
                'Set LastContactDate
                Dim SQL As String = "Update Contacts Set LastContactDate=#" & Today.ToShortDateString & "# WHERE ID=" & IDNumber.Content
                IO_UpdateSQLProcessor(gShipriteDB, SQL)
            End If


            If gAutoExitFromContacts = True Then
                Me.Close()
            End If

        End If

    End Sub
    Private Function ThereIsSomethingToSave() As Boolean
        ' Compare to gContactManagerSegment
        Dim isDifferent As Boolean = False
        Dim i As Integer
        Dim FName As String
        Dim buf As String
        FName = ""
        For i = 0 To 14
            Dim D As Object = Me.FindName("D" & i.ToString)
            FName = D.uid
            buf = ExtractElementFromSegment(FName, gContactManagerSegment)
            If Not buf = "" Then
                If Not D.Text = buf Then
                    isDifferent = True
                End If
            Else
                If Not String.IsNullOrEmpty(D.Text) And Not D.Text = D.ToolTip Then
                    isDifferent = True
                End If
            End If
        Next
        buf = ExtractElementFromSegment("Residential", gContactManagerSegment)
        If buf = "True" Then
            If Not Commercial_Checkbox.IsChecked = False Or Not Residential_Checkbox.IsChecked = True Then
                isDifferent = True
            End If
        Else
            If Not Commercial_Checkbox.IsChecked = True Or Not Residential_Checkbox.IsChecked = False Then
                isDifferent = True
            End If
        End If
        buf = ExtractElementFromSegment("Notes", gContactManagerSegment)
        If Not buf = "" Then
            If Not CustomerNotes.Text = buf Then
                isDifferent = True
            End If
        Else
            If Not String.IsNullOrEmpty(CustomerNotes.Text) Then
                isDifferent = True
            End If
        End If
        If Not IDNumber.Content = ExtractElementFromSegment("ID", gContactManagerSegment) Then
            isDifferent = True
        End If
        If Not CountrySelection.Text = ExtractElementFromSegment("Country", gContactManagerSegment) Then
            isDifferent = True
        End If
        ' Pull cell carrier
        buf = ExtractElementFromSegment("CellCarrier", gContactManagerSegment)
        If Not buf = "" Then
            ' Place cell carrier
            If Not CellCarrier_ComboBox.Text = buf Then
                isDifferent = True
            End If
        Else
            If Not String.IsNullOrEmpty(CellCarrier_ComboBox.Text) And Not CellCarrier_ComboBox.Text = CellCarrier_ComboBox.ToolTip Then
                isDifferent = True
            End If
        End If
        ' Pull contact image, commented out for simplicity
        '   like anyone who uses the software will actually notice
        'Dim image As Byte() = GetBytesFromDb(gShipriteDB, "Contacts", "ProfileImage", "ID=" & IDNumber.Content)
        'If Not image Is Nothing Then
        '    If Not image =
        '    File.WriteAllBytes(ProfileImageFile, image) Then
        '        Profile_Image.Source = New ImageSourceConverter().ConvertFromString(ProfileImageFile)
        '    End If
        Return isDifferent
    End Function
    Private Sub CreateAccountBTN_Click(sender As Object, e As RoutedEventArgs) Handles CreateAccount.Click
        If MsgBox("Create A/R Account?", MsgBoxStyle.YesNo, "Attention") = MsgBoxResult.Yes Then
            If D3.Text = "" Or D3.Text = "Account #" Then
                Dim Tally As Long
                Dim Result As String

                Tally = DateDiff("S", "1/1/2018", Now())
                Result = gDrawerID & Format$(Tally + 1, "0000000000")
                D3.Text = Result
            End If
        End If
    End Sub
#End Region

#Region "Customer_Notes"
    'shared functions for displaying, saving, deleting customer notes in customer drop down windows in Ship1 and POS
    Public Shared Sub Save_Customer_Notes(CID As Long, Notes As String)
        IO_UpdateSQLProcessor(gShipriteDB, "Update Contacts SET Notes='" & Notes & "' WHERE ID=" & CID)

    End Sub
    Public Shared Sub Delete_Customer_Notes(CID As Long, ByRef Notes_txtBox As System.Windows.Controls.TextBox)

        IO_UpdateSQLProcessor(gShipriteDB, "Update Contacts SET Notes='' WHERE ID=" & CID)

        Notes_txtBox.Text = ""

    End Sub
    Public Shared Function Display_Customer_Notes(CID As Long, ByRef Notes_txtBox As System.Windows.Controls.TextBox) As Boolean
        Dim SQL As String

        SQL = "Select Notes from Contacts WHERE ID=" & CID
        Notes_txtBox.Text = ExtractElementFromSegment("Notes", IO_GetSegmentSet(gShipriteDB, SQL), "")

        If Notes_txtBox.Text = "" Then
            Return False
        Else
            Return True
        End If

    End Function
#End Region

#Region "Verify Address"
    Public objContact As New _baseContact
    Public DefaultCountry As String
    Private Verify_TabPressed As String
    Private Sub Verify_Address_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Verify_Address_Btn.Click

        Try
            If Verify_Address_Popup.IsOpen Then
                Verify_Address_Popup.IsOpen = False
            Else
                If addressExist() Then
                    Call clear_OriginalAddress()
                    Call clear_VerifiedAddress()
                    Me.Verification_Codes_TextBox.Text = String.Empty
                    ' Populate Original Address
                    Original_Addr1_TextBox.Text = D4.Text
                    If Not String.IsNullOrEmpty(D5.Text) And Not D5.Text = D5.ToolTip Then
                        Original_Addr2_TextBox.Text = D5.Text
                    End If
                    If Not D6.Text = D6.ToolTip Then
                        Original_City_TextBox.Text = D6.Text
                    End If
                    If Not D7.Text = D7.ToolTip Then
                        Original_State_TextBox.Text = D7.Text
                    End If
                    If Not D8.Text = D8.ToolTip Then
                        Original_ZipCode.Text = D8.Text
                    End If
                    Original_Residential_CheckBox.IsChecked = Residential_Checkbox.IsChecked
                    Verify_Address_Popup.IsOpen = True

                    If GetPolicyData(gShipriteDB, "Address_Verification_Service", "0") = 0 Then
                        FedexWebServices_Button_Click(Nothing, Nothing)
                    Else
                        EndiciaDialAZip_Button_Click(Nothing, Nothing)
                    End If
                End If
            End If
        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to open Verify Address option...")
        End Try
    End Sub
    Private Function addressExist() As Boolean
        If String.IsNullOrEmpty(D4.Text) Or D4.Text = D4.ToolTip Then
            MessageBox.Show("Please enter an address.")
            Return False
        Else
            Return True
        End If
    End Function
    Private Sub clear_OriginalAddress()
        Me.Original_Addr1_TextBox.Text = String.Empty : Me.Original_Addr1_TextBox.Background = System.Windows.Media.Brushes.White
        Me.Original_Addr2_TextBox.Text = String.Empty : Me.Original_Addr2_TextBox.Background = System.Windows.Media.Brushes.White
        Me.Original_City_TextBox.Text = String.Empty : Me.Original_City_TextBox.Background = System.Windows.Media.Brushes.White
        Me.Original_State_TextBox.Text = String.Empty : Me.Original_State_TextBox.Background = System.Windows.Media.Brushes.White
        Me.Original_ZipCode.Text = String.Empty : Me.Original_ZipCode.Background = System.Windows.Media.Brushes.White
        Me.Original_Residential_CheckBox.IsChecked = False : Me.Original_Residential_CheckBox.Background = System.Windows.Media.Brushes.Gray
    End Sub

    Private Sub clear_VerifiedAddress()
        Me.Verified_Addr1_TextBox.Text = String.Empty : Me.Verified_Addr1_TextBox.Background = System.Windows.Media.Brushes.White
        Me.Verified_Addr2_TextBox.Text = String.Empty : Me.Verified_Addr2_TextBox.Background = System.Windows.Media.Brushes.White
        Me.Verified_City_TextBox.Text = String.Empty : Me.Verified_City_TextBox.Background = System.Windows.Media.Brushes.White
        Me.Verified_State_TextBox.Text = String.Empty : Me.Verified_State_TextBox.Background = System.Windows.Media.Brushes.White
        Me.Verified_ZipCode.Text = String.Empty : Me.Verified_ZipCode.Background = System.Windows.Media.Brushes.White
        Me.Verified_Residential_CheckBox.IsChecked = False : Me.Verified_Residential_CheckBox.Background = System.Windows.Media.Brushes.Gray
    End Sub
    Private Function IsOriginalAvailable() As Boolean
        If String.IsNullOrEmpty(Me.Original_Addr1_TextBox.Text) Or
            String.IsNullOrEmpty(Me.Original_City_TextBox.Text) Or 'String.IsNullOrEmpty(Me.Original_State_TextBox.Text) Or
            String.IsNullOrEmpty(Me.Original_ZipCode.Text) Then
            Return False
        Else
            Return True
        End If
    End Function
    Private Function read_OriginalAddress(ByVal obj As _baseContact) As Boolean
        With obj '_FedExWeb.original
            Me.Original_Addr1_TextBox.Text = .Addr1
            If Not String.IsNullOrEmpty(.Addr2) AndAlso Not .Addr2 = D5.ToolTip Then
                Me.Original_Addr2_TextBox.Text = .Addr2
            End If
            Me.Original_City_TextBox.Text = .City
            Me.Original_State_TextBox.Text = .State
            Me.Original_ZipCode.Text = .Zip
            Me.Original_Residential_CheckBox.IsChecked = .Residential
        End With
        Return True
    End Function

    Private Function read_VerifiedAddress(ByVal obj As _baseContact) As Boolean
        With obj '_FedExWeb.verified
            Me.Verified_Addr1_TextBox.Text = StrConv(.Addr1, VbStrConv.ProperCase)
            Dim Addr2 As String = StrConv(.Addr2, VbStrConv.ProperCase)
            If Not String.IsNullOrEmpty(Addr2) AndAlso Not Addr2 = D5.ToolTip Then
                Me.Verified_Addr2_TextBox.Text = .Addr2
            End If
            Me.Verified_City_TextBox.Text = StrConv(.City, VbStrConv.ProperCase)
            Me.Verified_State_TextBox.Text = .State
            Me.Verified_ZipCode.Text = .Zip
            Me.Verified_Residential_CheckBox.IsChecked = .Residential
        End With
        Call compare_Addresses()
        Return True
    End Function

    Private Sub compare_Addresses()
        Dim c As System.Windows.Media.Brush = System.Windows.Media.Brushes.Yellow
        '
        If Not Me.Verified_Addr1_TextBox.Text.ToUpper = Me.Original_Addr1_TextBox.Text.ToUpper Then
            Me.Verified_Addr1_TextBox.Background = c
        End If
        If Not Me.Verified_Addr2_TextBox.Text.ToUpper = Me.Original_Addr2_TextBox.Text.ToUpper Then
            Me.Verified_Addr2_TextBox.Background = c
        End If
        If Not Me.Verified_City_TextBox.Text.ToUpper = Me.Original_City_TextBox.Text.ToUpper Then
            Me.Verified_City_TextBox.Background = c
        End If
        If Not Me.Verified_State_TextBox.Text.ToUpper = Me.Original_State_TextBox.Text.ToUpper Then
            Me.Verified_State_TextBox.Background = c
        End If
        If Not Me.Verified_ZipCode.Text.ToUpper = Me.Original_ZipCode.Text.ToUpper Then
            Me.Verified_ZipCode.Background = c
        End If
        If Not Me.Verified_Residential_CheckBox.IsChecked = Me.Original_Residential_CheckBox.IsChecked Then
            Me.Verified_Residential_CheckBox.Background = c
        End If
    End Sub
    Private Function create_ContactObject(ByRef obj As _baseContact) As Boolean
        obj.Country = Me.CountrySelection.Text
        obj.CountryCode = _Contact.Get_CountryCodeFromCountryName(obj.Country)
        obj.CompanyName = Me.D0.Text
        obj.FName = Me.D1.Text
        obj.LName = Me.D2.Text
        obj.Addr1 = Me.D4.Text
        If D5.Text <> D5.ToolTip Then obj.Addr2 = Me.D5.Text
        If D14.Text <> D14.ToolTip Then obj.Addr3 = Me.D14.Text
        obj.City = Me.D6.Text
        obj.State = Me.D7.Text
        obj.Zip = Me.D8.Text
        obj.Tel = Me.D10.Text
        obj.Fax = Me.D11.Text
        obj.Residential = Me.Residential_Checkbox.IsChecked
        obj.Email = Me.D13.Text
        obj.AccountNumber = Me.D3.Text
        'obj.CLASS_TYPE = Me.txtFirstName.Tag
        ''ol#1.1.93(3/6)... SMS entry was added to collect cell phone & domain for text notifications.
        obj.CellCarrier = Me.CellCarrier_ComboBox.Text
        'obj.CellDomain = SMS.txtCellDomain.Text
        obj.CellPhone = Me.D12.Text
        create_ContactObject = True
    End Function

    Private Function read_ContactObject(ByVal obj As _baseContact, ByVal isitVarified As Boolean) As Boolean
        'Me.cmbCountry.Tag = obj.ContactID
        Me.CountrySelection.Text = obj.Country
        If String.IsNullOrEmpty(CountrySelection.Text) Then
            CountrySelection.Text = DefaultCountry
        End If
        Me.D0.Text = obj.CompanyName
        Me.D1.Text = obj.FName
        Me.D2.Text = obj.LName
        Me.D4.Text = obj.Addr1
        Me.D5.Text = obj.Addr2
        Me.D14.Text = obj.Addr3
        Me.D6.Text = obj.City
        Me.D7.Text = obj.State
        Me.D8.Text = obj.Zip
        Me.D10.Text = obj.Fax
        ''ol#1.2.00(5/20)... If address is verified then don't check for residential based on comma in company name.
        ''ol#1.1.97(4/23)... If Company Name has a comma sign in it then Residential flag will be set.
        If Not isitVarified Then
            ''ol#1.2.00(5/20)... If address is not verified and has comma and INC keyword in company name then it becomes commercial one.
            If (Not 0 < obj.ContactID) AndAlso (_Controls.Contains(obj.CompanyName, ", INC") Or _Controls.Contains(Me.D0.Text, " INC.")) Then
                obj.Residential = False
            ElseIf (Not 0 < obj.ContactID) AndAlso _Controls.Contains(obj.CompanyName, ",") Then
                obj.Residential = True
            End If
        End If
        Me.Residential_Checkbox.IsChecked = obj.Residential
        Me.Commercial_Checkbox.IsChecked = Not Me.Residential_Checkbox.IsChecked
        Me.D13.Text = obj.Email
        Me.D3.Text = obj.AccountNumber
        'Me.txtFirstName.Tag = obj.CLASS_TYPE
        Me.CellCarrier_ComboBox.Text = obj.CellCarrier
        'SMS.txtCellDomain.Text = obj.CellDomain
        Me.D12.Text = obj.CellPhone
        read_ContactObject = True
    End Function
    '#Region "Test"
    '    Private Function create_ShipToObject(ByRef obj As _baseContact, Optional countryCode As String = "US") As Boolean
    '        ' create_ShipToObject = True
    '        If countryCode = "US" Then
    '            With obj
    '                .ContactID = 2
    '                .CompanyName = "Shipping Co."
    '                .FName = "Dr. Kenneth"
    '                .LName = "Beckman"
    '                .Addr1 = "1510 Everette St."
    '                .Addr2 = ""
    '                .City = "Alameda"
    '                .State = "CA"
    '                .Zip = "94501"
    '                .Province = ""
    '                .Country = "United States"
    '                .CountryCode = "US"
    '                .Tel = "315-796-4528"
    '                .Fax = ""
    '                .Email = "oleg@shipritesoftware.com"
    '                .Residential = False
    '            End With
    '        ElseIf countryCode = "MX" Then
    '            With obj
    '                .ContactID = 6
    '                .CompanyName = "Mexico US Embassy"
    '                .FName = ""
    '                .LName = ""
    '                .Addr1 = "Paseo De La Reforma 305"
    '                .Addr2 = ""
    '                .City = "Mexico"
    '                .State = ""
    '                .Zip = "06500"
    '                .Province = ""
    '                .Country = "Mexico"
    '                .CountryCode = "MX"
    '                .Tel = "340-344-7632"
    '                .Fax = ""
    '                .Email = "oleg@shipritesoftware.com"
    '            End With
    '        ElseIf countryCode = "VI" Then
    '            With obj
    '                .ContactID = 6
    '                .CompanyName = "Mail Plus"
    '                .FName = "Alvery"
    '                .LName = "Smith"
    '                .Addr1 = "4605 Tu Tu Park Mall Ste 133"
    '                .Addr2 = ""
    '                .City = "St Thomas"
    '                .State = "VI"
    '                .Zip = "00802"
    '                .Province = ""
    '                .Country = "US Virgin Islands"
    '                .CountryCode = "VI"
    '                .Tel = "340-344-7632"
    '                .Fax = ""
    '                .Email = "oleg@shipritesoftware.com"
    '            End With
    '        ElseIf countryCode = "CA" Then ' Canada
    '            With obj
    '                .ContactID = 4
    '                .CompanyName = "Canada FedEx"
    '                .FName = ""
    '                .LName = ""
    '                .Addr1 = "80 FEDEX PRKWY"
    '                .Addr2 = ""
    '                .City = "WESTMOUNT"
    '                .State = "QC"
    '                .Zip = "H3Z2Y7"
    '                .Province = ""
    '                .Country = "Canada"
    '                .CountryCode = "CA"
    '                .Tel = "901-263-3035"
    '                .Fax = ""
    '                .Email = "oleg@shipritesoftware.com"
    '            End With
    '        ElseIf countryCode = "PR" Then ' Puerto Rico
    '            With obj
    '                .ContactID = 4
    '                .CompanyName = "Puerto Rico FedEx"
    '                .FName = ""
    '                .LName = ""
    '                .Addr1 = "80 FEDEX PRKWY"
    '                .Addr2 = ""
    '                .City = "BAYAMON"
    '                .State = "PR"
    '                .Zip = "00961"
    '                .Province = ""
    '                '.Country = "PUERTO RICO"
    '                .CountryCode = "PR"
    '                .Tel = "901-263-3035"
    '                .Fax = ""
    '                .Email = "oleg@shipritesoftware.com"
    '            End With
    '        ElseIf countryCode = "AS" Then ' AMERICAN SAMOA ' only this zip code required Customs Form
    '            With obj
    '                .ContactID = 5
    '                .CompanyName = "AMERICAN SAMOA FedEx"
    '                .FName = ""
    '                .LName = ""
    '                .Addr1 = "80 FEDEX PRKWY"
    '                .Addr2 = ""
    '                .City = "PAGO PAGO"
    '                .State = "AS"
    '                .Zip = "96799"
    '                .Province = ""
    '                .Country = ""
    '                .CountryCode = "AS"
    '                .Tel = "901-263-3035"
    '                .Fax = ""
    '                .Email = "oleg@shipritesoftware.com"
    '            End With
    '        ElseIf countryCode = "AP" Then ' Military base address
    '            With obj
    '                .ContactID = 8
    '                .CompanyName = ""
    '                .FName = "Scott"
    '                .LName = "Harold"
    '                .Addr1 = "PSC 305 Box 2203"
    '                .Addr2 = ""
    '                .City = "APO"
    '                .State = "AP"
    '                .Zip = "96218"
    '                .Province = ""
    '                .Country = ""
    '                .CountryCode = "AP"
    '                .Tel = "901-263-3035"
    '                .Fax = ""
    '                .Email = "oleg@shipritesoftware.com"
    '            End With
    '        ElseIf countryCode = "AE" Then ' Military base address
    '            With obj
    '                .ContactID = 8
    '                .CompanyName = ""
    '                .FName = "Betsy"
    '                .LName = "Jenkins"
    '                .Addr1 = "Prg 6-6 FOB Fenty"
    '                .Addr2 = ""
    '                .City = "APO"
    '                .State = "AE"
    '                .Zip = "09310"
    '                .Province = ""
    '                .Country = ""
    '                .CountryCode = "AE"
    '                .Tel = "901-263-3035"
    '                .Fax = ""
    '                .Email = "oleg@shipritesoftware.com"
    '            End With
    '        ElseIf countryCode = "UK" Then
    '            With obj
    '                .ContactID = 8
    '                .CompanyName = "U.S. Embassy"
    '                .FName = ""
    '                .LName = ""
    '                .Addr1 = "24 Grosvenor Square"
    '                .Addr2 = ""
    '                .City = "London"
    '                .State = ""
    '                .Zip = "W1K 6AH"
    '                .Province = ""
    '                .Country = "United Kingdom"
    '                .CountryCode = "UK"
    '                .Tel = "20-7499-9000"
    '                .Fax = ""
    '                .Email = "oleg@shipritesoftware.com"
    '            End With
    '        ElseIf countryCode = "FR" Then
    '            With obj
    '                .ContactID = 8
    '                .CompanyName = "U.S. Embassy"
    '                .FName = ""
    '                .LName = ""
    '                .Addr1 = "2 avenue Gabriel"
    '                .Addr2 = ""
    '                .City = "Paris"
    '                .State = ""
    '                .Zip = "75008"
    '                .Province = ""
    '                .Country = "France"
    '                .CountryCode = "FR"
    '                .Tel = "43122222"
    '                .Fax = ""
    '                .Email = "oleg@shipritesoftware.com"
    '            End With
    '        Else
    '            With obj
    '                .ContactID = 3
    '                .CompanyName = "Russian Red-Cross"
    '                .FName = "Russian"
    '                .LName = "Test Officer"
    '                .Addr1 = "Cheryomushkinsky Proezd 5 Rf"
    '                .Addr2 = "4th Floor - Office# 1234"
    '                .City = "Moscow"
    '                .State = ""
    '                .Zip = "117036"
    '                .Province = ""
    '                .Country = "Russia"
    '                .CountryCode = "RU"
    '                .Tel = "111-796-4528"
    '                .Fax = ""
    '                .Email = "oleg@shipritesoftware.com"
    '            End With

    '        End If
    '    End Function

    '#End Region
    Private Sub Original_Apply_Button_Click(sender As Object, e As RoutedEventArgs) Handles Original_Apply_Button.Click
        Verify_Address_Popup.IsOpen = False
    End Sub
    Private Sub Verified_Apply_Button_Click(sender As Object, e As RoutedEventArgs) Handles Verified_Apply_Button.Click
        Try

            _FedExWeb.isSaveVerifiedAddress = True
            Verify_Address_Popup.IsOpen = False
            '
            Call copy_VerifiedAddress(Me.objContact)

            ' display on the form
            Call read_ContactObject(Me.objContact, True)

        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to save...")
        End Try
    End Sub

    Public Function copy_VerifiedAddress(ByRef obj1 As Object) As Boolean
        obj1.Addr1 = Me.Verified_Addr1_TextBox.Text
        obj1.Addr2 = Me.Verified_Addr2_TextBox.Text
        obj1.City = Me.Verified_City_TextBox.Text
        obj1.State = Me.Verified_State_TextBox.Text
        obj1.Zip = Me.Verified_ZipCode.Text
        obj1.CountryCode = Me.Verified_State_TextBox.Text
        obj1.Residential = Me.Verified_Residential_CheckBox.IsChecked
        Return True
    End Function

#Region "FedEx Verify"
    Private Sub FedexWebServices_Button_Click(sender As Object, e As RoutedEventArgs) Handles FedexWebServices_Button.Click
        Try
             
            If IsOriginalAvailable() Then
                Me.Verify_TabPressed = "FedEx"
                '
                Call create_ContactObject(Me.objContact)
                '
                _FedExWeb.objFedEx_Setup = _FedExWeb.objFedEx_Regular_Setup

                If GetPolicyData(gShipriteDB, "FedExREST_Enabled", "False") = True Then
                    'FedEx REST 
                    Dim verifiedContact As New _baseContact
                    If FXR_Submit_Address_For_Validation(Me.objContact, verifiedContact) Then
                        Verify_Address_Popup.IsOpen = True
                        Call read_OriginalAddress(Me.objContact)
                        Call read_VerifiedAddress(verifiedContact)
                    End If



                Else


                    ' "Not Is Nothing" as a quick way to prevent an object not set to an instance... error
                    If Not _FedExWeb.objFedEx_Setup Is Nothing AndAlso Not String.IsNullOrEmpty(_FedExWeb.objFedEx_Setup.Client_MeterNumber) Then
                        '
                        If _FedExWeb.Submit_AddressValidation(Me.objContact) Then

                            Verify_Address_Popup.IsOpen = True
                            '
                            If _FedExWeb.original IsNot Nothing Then
                                Call read_OriginalAddress(_FedExWeb.original)
                            End If
                            If _FedExWeb.verified IsNot Nothing Then
                                Call read_VerifiedAddress(_FedExWeb.verified)
                                '
                                If _FedExWeb.verifiedcodes IsNot Nothing Then
                                    If 0 < _FedExWeb.verifiedcodes.Count Then
                                        Call read_VerifiedCodes_FedEx()
                                    End If
                                End If
                            End If
                            '
                        End If
                    Else
                        Verification_Codes_TextBox.Text = "Unable to verify." & Environment.NewLine & "Please log into FedEx first."
                    End If
                End If


            Else
                _MsgBox.WarningMessage("No original address is available to verify.")
                Verify_Address_Popup.IsOpen = False
            End If
        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to verify with FedEx...")
        End Try
    End Sub
    Private Function read_VerifiedCodes_FedEx() As Boolean
        Me.Verification_Codes_TextBox.Text = String.Empty
        Verification_Codes_TextBox.Text = "FedEx Verification:" & vbCrLf & vbCrLf
        For i As Integer = 0 To _FedExWeb.verifiedcodes.Count - 1
            Me.Verification_Codes_TextBox.Text += String.Format("({0}) {1}{2}{2}", (i + 1), _FedExWeb.verifiedcodes(i), Environment.NewLine)
        Next i
        Return True
    End Function
#End Region

#Region "Endicia Verify"
    Private Sub EndiciaDialAZip_Button_Click(sender As Object, e As RoutedEventArgs) Handles EndiciaDialAZip_Button.Click

        If CountrySelection.SelectedItem.CountryName <> "United States" Then
            MsgBox("Endicia verification does not support international addresses!", vbExclamation)
            Exit Sub
        End If

        If IsOriginalAvailable() Then
            Try
                If _EndiciaWeb.original Is Nothing Then
                    _EndiciaWeb.original = New _baseContact
                End If
                With _EndiciaWeb.original
                    .Addr1 = Original_Addr1_TextBox.Text
                    .Addr2 = Original_Addr2_TextBox.Text
                    .City = Original_City_TextBox.Text
                    .State = Original_State_TextBox.Text
                    .Zip = Original_ZipCode.Text
                    .Residential = Original_Residential_CheckBox.IsChecked
                    .CompanyName = ""
                End With

                Call create_ContactObject(Me.objContact)

                If _EndiciaWeb.Request_ValidateAddress(_EndiciaWeb.original) Then
                    Call read_VerifiedAddress()
                    Call read_VerifiedCodes()
                End If
            Catch ex As Exception
                Debug.Print(ex.ToString)
            End Try
        Else
            _MsgBox.WarningMessage("No original address is available to verify.")
        End If
    End Sub
    Private Function read_VerifiedAddress() As Boolean
        With _EndiciaWeb.verified
            Verified_Addr1_TextBox.Text = StrConv(.Addr1, VbStrConv.ProperCase)
            Verified_Addr2_TextBox.Text = StrConv(.Addr2, VbStrConv.ProperCase)
            Verified_City_TextBox.Text = StrConv(.City, VbStrConv.ProperCase)
            Verified_State_TextBox.Text = .State
            Verified_ZipCode.Text = .Zip
            Verified_Residential_CheckBox.IsChecked = .Residential
        End With
        Call compare_Addresses()
        Return True
    End Function
    Private Function read_VerifiedCodes() As Boolean
        Verification_Codes_TextBox.Text = String.Empty
        Verification_Codes_TextBox.Text = "Endicia Verification:" & vbCrLf & vbCrLf
        For i As Integer = 0 To _EndiciaWeb.verifiedcodes.Count - 1
            Verification_Codes_TextBox.Text += String.Format("({0}) {1}{2}{2}", (i + 1), _EndiciaWeb.verifiedcodes(i), Environment.NewLine)
        Next i
        Return True
    End Function
#End Region

#End Region

#Region "Image Handling"
    '    Public Function Base64ToFile(filename As String, input As String) As Boolean
    '        ' convert the input base64 to an image located at filename and return true if everything worked
    '        Try
    '            File.WriteAllBytes(filename, System.Convert.FromBase64String(input))
    '            Return True
    '        Catch ex As Exception
    '            Debug.WriteLine(ex.ToString)
    '            Return False
    '        End Try
    '    End Function
    '    Public Function FileToBase64(filename As String) As String
    '        Try
    '            Return System.Convert.ToBase64String(File.ReadAllBytes(filename))
    '        Catch ex As Exception
    '            Debug.WriteLine(ex.ToString)
    '            Return Nothing
    '        End Try
    '    End Function
#End Region

#Region "Autocomplete"
    Public Sub AutoCompleteAddress(input As String, ByVal countrycode As String)
        ' If the entered text is over 3 chars long, run autocomplete
        Dim parameters As Dictionary(Of String, String) = New Dictionary(Of String, String)
        parameters.Add("address", System.Web.HttpUtility.UrlEncode(input))
        parameters.Add("compatibility", "cloud")
        parameters.Add("key", ApiRequest.apiKey)
        'parameters.Add("country", countrycode)
        Dim APIResponse As String = ApiRequest.liminal("autocomplete", parameters)
        AutoComplete_SuggestList = New ObservableCollection(Of AddressSuggestion)
        Dim ResponseObject As Object = JObject.Parse(APIResponse)
        If Not ResponseObject("status") Then
            ' API Failed
            Debug.Print(ResponseObject.ToString)
        Else

            For Each address As Object In ResponseObject("data")
                Dim data As AddressSuggestion = New AddressSuggestion
                data.HouseNumber = address("address")("houseNumber")
                data.Street = address("address")("street")
                data.City = address("address")("city")
                data.State = address("address")("state")
                data.PostalCode = address("address")("postalCode")
                'data.CountryCode = address("address")("countryCode")
                data.DisplayText = data.HouseNumber & " " & data.Street & ", " & data.City & ", " & data.State & " " & data.PostalCode
                Me.Dispatcher.Invoke(Sub() AutoComplete_SuggestList.Add(data))
            Next
        End If
        Me.Dispatcher.Invoke(Sub() AddressAutocomplete_Popup.IsOpen = True)

    End Sub
    Private Sub AddressAutocomplete_PopulateData(address As AddressSuggestion)
        D4.Text = address.HouseNumber & " " & address.Street
        D6.Text = address.City
        If address.State.Length > 2 Then
            D7.Text = ApiRequest.Get_StateCodeFromStateName(address.State)
        Else
            D7.Text = address.State
        End If
        D8.Text = address.PostalCode
        D4.Foreground = Application.Current.MainWindow.FindResource("Black_Color")
        D6.Foreground = Application.Current.MainWindow.FindResource("Black_Color")
        D7.Foreground = Application.Current.MainWindow.FindResource("Black_Color")
        D8.Foreground = Application.Current.MainWindow.FindResource("Black_Color")
        D9.Focus() ' home phone
        AddressAutocomplete_Popup.IsOpen = False
    End Sub
    Private Sub AddressAutoComplete(AutoComplete_Input As String, ByVal countrycode As String)
        If AutoComplete_Timer IsNot Nothing Then
            AutoComplete_Timer.Dispose()
        End If
        ' start timer on background thread to call AutoCompleteAddress() function only one time and delay by AutoComplete_Timeout secs
        AutoComplete_Timer = New Threading.Timer(Sub() AutoCompleteAddress(AutoComplete_Input, countrycode), Nothing, AutoComplete_Timeout * 1000, Threading.Timeout.Infinite)
    End Sub
    Private Sub PostCodeControl(PostCodeControlInput As String, CountryName As String)
        If Not String.IsNullOrEmpty(PostCodeControlInput) Then
            If CountryName = "United States" Then
                Dim PostCodeControlOutput As String() = {"", ""}
                Dim Result As PostCodeResult = GetPostCodeData(PostCodeControlInput) 'Await Task.Run(Function() GetPostCodeData(PostCodeControlInput))
                If Not IsNothing(Result) Then
                    If Result.CountryCode = "US" Then
                        PostCodeControlOutput(0) = Result.City
                        PostCodeControlOutput(1) = Result.State
                    End If
                End If
                Debug.Print("Post Code Lookup complete")
                Me.Dispatcher.Invoke(DispatcherPriority.Normal, New Action(Sub() PostCodeControlApply(PostCodeControlOutput)))
            End If
        End If
    End Sub
    Private Sub PostCodeControlApply(PostCodeControlOutput As String())
        If Not String.IsNullOrEmpty(PostCodeControlOutput(0)) Then
            D6.Text = PostCodeControlOutput(0)
            D6.Foreground = Application.Current.MainWindow.FindResource("Black_Color")
        End If
        If Not String.IsNullOrEmpty(PostCodeControlOutput(1)) Then
            D7.Text = PostCodeControlOutput(1)
            D7.Foreground = Application.Current.MainWindow.FindResource("Black_Color")
        End If
    End Sub
    Private Sub PostCodeLoadDB()
        ApiRequest.EnsureCodeDBLoaded()
    End Sub

    Private Sub HotSearch_Popup_Opened(sender As Object, e As EventArgs) Handles HotSearch_Popup.Opened
        'forces the user to double click name in hot search to pull it up. Disabling submit button prevents user of just selecting it and clicking Save.
        Submit.IsEnabled = False
    End Sub

    Private Sub HotSearch_Popup_Closed(sender As Object, e As EventArgs) Handles HotSearch_Popup.Closed
        Submit.IsEnabled = True
    End Sub
#End Region
End Class
