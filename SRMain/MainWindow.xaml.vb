Imports System.Drawing
Imports System.Drawing.Printing
Imports System.IO
Imports System.Threading
Imports System.Web
Imports System.Net
Imports RestSharp
Imports Newtonsoft.Json.Linq
Imports Newtonsoft.Json

Class MainWindow
    Inherits CommonWindow

    Private UpdaterModule As New SRN_Updater()

    Public Sub New()

        MyBase.New()
        Set_Window_StartingSize()

        ' This call is required by the designer.
        InitializeComponent()

    End Sub

    Public Sub New(ByVal callingWindow As Window)

        MyBase.New(callingWindow)
        Set_Window_StartingSize()

        ' This call is required by the designer.
        InitializeComponent()

    End Sub
    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Dim buf As String
        Dim SQL As String
        Dim SRN_v As String

        If gIsProgramSecurityEnabled Then

            Dim win As New UserLogIn()
            win.ShowDialog()

            If UserLogIn.isAllowed = False Then
                Application.Current.Shutdown()
                Exit Sub
            End If

            UserName_Display.Visibility = Visibility.Visible
            UserName_TxtBx.Text = gCurrentUser

        Else

            UserName_Display.Visibility = Visibility.Hidden

        End If
        If Not String.IsNullOrWhiteSpace(gCurrentUser) AndAlso gUserSegment = "" Then

            SQL = "SELECT * FROM Users WHERE [DisplayName] = '" & gCurrentUser & "'"
            gUserSegment = IO_GetSegmentSet(gShipriteDB, SQL)

        End If

        Startup_Check_Registration()
        VersionNo_Lbl.Content = GetPolicyData(gShipriteDB, "Name", "") & "   v" & My.Application.Info.Version.ToString

        buf = Get_DefaultTaxCounty()
        SQL = "SELECT * FROM CountyTaxes WHERE County = '" & buf & "'"
        gPOSDefaultTaxSegment = IO_GetSegmentSet(gShipriteDB, SQL)


        If CheckTickler() > 0 And gRegistrationExpired = False Then
            'When first opening software, open Tickler windows automatically if there are tasks to do
            Dim win As New Tickler(Me)
            win.ShowDialog(Me)
        End If

        Open_CustomerDisplayScreen()

        UpdaterModule = New SRN_Updater
        UpdaterModule.ThreadStart_Silent()

        SRN_v = My.Application.Info.Version.ToString

        SQL = "UPDATE Setup2 SET ShipriteVersion = '" & SRN_v & "'"
        buf = IO_UpdateSQLProcessor(gReportWriter, SQL)
        UpdatePolicy(gShipriteDB, "ShipriteVersion", SRN_v)


    End Sub

    Private Sub Window_KeyDown(sender As Object, e As KeyEventArgs) Handles Me.KeyDown

        ShortcutKeyHandlers.KeyDown(sender, e, Me)

    End Sub

    Private Sub Me_Activated(sender As Object, e As EventArgs) Handles Me.Activated

        UserName_TxtBx.Text = gCurrentUser


        If gRegistrationExpired = False Then

            CheckTickler()

        End If


        If gIsCustomerDisplayEnabled And Not IsNothing(gCustomerDisplay) Then
            gCustomerDisplay.ChangeTab(0)
        End If
    End Sub

    Private Function Get_DefaultTaxCounty()
        Dim county As String = GetPolicyData(gShipriteDB, "DefaultCounty", "")

        Dim buf = IO_GetSegmentSet(gShipriteDB, "SELECT * From CountyTaxes WHERE County='" & county & "'")

        If buf = "" Then
            'if default county does not exist in table, get first county from list
            buf = IO_GetSegmentSet(gShipriteDB, "SELECT First(county) as FirstCounty From CountyTaxes")
            county = ExtractElementFromSegment("FirstCounty", buf, "")

            If county = "" Then
                MsgBox("Tax Counties not setup. Please go into SETUP > POS Setup > Tax Counties and setup sales tax!", vbExclamation)
                Return "none"

            Else
                Return county
            End If

        Else
            Return county
        End If

    End Function

    Private Sub Set_Window_StartingSize()
        If My.Settings.Window_IsMaximized Then
            Me.WindowState = WindowState.Maximized
        Else
            Me.Height = My.Settings.Window_Height
            Me.Width = My.Settings.Window_Width
        End If

        Me.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight
        Me.MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth

    End Sub

    Private Sub SearchBox_GotFocus(sender As Object, e As RoutedEventArgs) Handles SearchBox.GotFocus

        Try
            SearchBox.Text = ""
            SearchBox.Foreground = Me.FindResource("Black_Color")

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try

    End Sub

    Private Sub Open_CustomerDisplayScreen()

        If gIsCustomerDisplayEnabled Then

            Dim secondaryScreenLeft = SystemParameters.PrimaryScreenWidth
            gCustomerDisplay = New CustomerDisplay
            gCustomerDisplay.WindowStartupLocation = WindowStartupLocation.Manual
            gCustomerDisplay.Left = secondaryScreenLeft
            gCustomerDisplay.Top = 0
            gCustomerDisplay.Show()

        End If

    End Sub

    Private Function CheckTickler() As Integer

        Dim TicklerCount As Integer = 0
        TicklerCount = Tickler.Get_Open_Tickler_Count()

        Tickler_Count_Lbl.Content = TicklerCount

        If TicklerCount > 0 Then
            Tickler_Btn_Lbl.Content = "NEW TASKS TO DO"
        Else
            Tickler_Btn_Lbl.Content = "TICKLER"
        End If

        Return TicklerCount
    End Function

    Private Sub SearchBox_LostFocus(sender As Object, e As RoutedEventArgs) Handles SearchBox.LostFocus

        Try
            SearchBox.Text = "Type here to search"
            SearchBox.Foreground = Me.FindResource("Faded_Color")

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try

    End Sub

    Private Sub POSButton_Click(sender As Object, e As RoutedEventArgs) Handles POSButton.Click

        Dim ret As Boolean = False
        Dim PassCode As Integer

        PassCode = ValidateAccess(gCurrentUser, "A", "POS", "Point Of Sale")
        If gRegistrationExpired = True Or PassCode = 1 Then
            Exit Sub
        End If

        Try

            Dim win As New POSManager(Me)
            win.ShowDialog(Me)

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub

    Private Sub ShipButton_Click(sender As Object, e As RoutedEventArgs) Handles ShipButton.Click

        If GetPolicyData(gShipriteDB, "ForcePOSShipping") Then
            MsgBox("SHIP access is only allowed through POS!", vbInformation)
            Exit Sub
        End If

        Dim PassCode As Integer
        PassCode = ValidateAccess(gCurrentUser, "F", "SHIPPING", "Ship Master")
        If gRegistrationExpired = True Or PassCode = 1 Then

            Exit Sub

        End If
        Try


            gCallingSKU = ""

            Dim win As New ShipManager(Me)
            win.ShowDialog(Me)

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub

    Private Sub MBXButton_Click(sender As Object, e As RoutedEventArgs) Handles MBXButton.Click

        Dim PassCode As Integer
        PassCode = ValidateAccess(gCurrentUser, "K", "Setup_Mailbox", "Mailbox Master")
        If gRegistrationExpired = True Or PassCode = 1 Then

            Exit Sub

        End If
        If gRegistrationExpired = True Then

            Exit Sub

        End If
        Try



            Dim win As New MailboxManager(Me)
            win.ShowDialog(Me)


        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub

    Private Sub ReportsButton_Click(sender As Object, e As RoutedEventArgs) Handles ReportsButton.Click

        Dim PassCode As Integer
        PassCode = ValidateAccess(gCurrentUser, "", "Reports_IncomeProduction", "Viewing Reports")
        If gRegistrationExpired = True Or PassCode = 1 Then

            Exit Sub

        End If
        Try

            Dim win As New ReportsManager(Me)
            win.ShowDialog(Me)

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub

    Private Sub SetupOptions_Click(sender As Object, e As RoutedEventArgs) Handles SetupOptions.Click

        Dim PassCode As Integer
        PassCode = ValidateAccess(gCurrentUser, "", "SETUP", "System Setup")
        If gRegistrationExpired = True Or PassCode = 1 Then

            Exit Sub

        End If

        Try


            If OpenUserLogin(Me, "SETUP", gIsSetupSecurityEnabled) = False Then
                Exit Sub
            End If

            Dim win As New SetupManager(Me)
            win.ShowDialog(Me)
            gItemSetIntialized = False  ' Intialize Packmaster Structure Just in case Values were changed
            ReDim gItemSet(0)

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try

    End Sub

    Private Sub Utilities_Click(sender As Object, e As RoutedEventArgs) Handles Utilities.Click

        Dim PassCode As Integer
        PassCode = ValidateAccess(gCurrentUser, "", "SETUP", "Utilities")
        If gRegistrationExpired = True Or PassCode = 1 Then

            Exit Sub

        End If
        Try


            Dim win As New UtilitiesManager(Me)
            win.ShowDialog(Me)

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub

    Private Sub DropOffManagerButton_Click(sender As Object, e As RoutedEventArgs) Handles DropOffManagerButton.Click

        Dim PassCode As Integer
        PassCode = ValidateAccess(gCurrentUser, "I", "", "Dropoff Manager")
        If gRegistrationExpired = True Or PassCode = 1 Then

            Exit Sub

        End If
        Try


            Call _DropOff.Open_DropOffManager(Me, gCurrentUser, Nothing)

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub

    Private Sub PackageValetButton_Click(sender As Object, e As RoutedEventArgs) Handles PackageValetButton.Click

        If gRegistrationExpired = True Then

            Exit Sub

        End If
        Try


            If _MailboxPackage.Open_PackageProcessingCenter(gShipriteDB, gMailboxDB, gReportsDB, gCurrentUser) Then
                Dim win As New PackageValet(Me)
                win.ShowDialog(Me)
            Else
                _MsgBox.ErrorMessage("Could not read DefaultShipFrom id value to retrieve Store Owner address object!", "Failed to get Store Owner address!", "Package Valet Dashboard")
            End If
        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try

    End Sub

    Private Sub ContactsButton_Click(sender As Object, e As RoutedEventArgs) Handles ContactsButton.Click

        Dim PassCode As Integer
        PassCode = ValidateAccess(gCurrentUser, "B", "", "Contacts Manager")
        If gRegistrationExpired = True Or PassCode = 1 Then

            Exit Sub

        End If
        Try


            gAutoExitFromContacts = False
            Dim win As New ContactManager(Me)
            win.ShowDialog(Me)
            gAutoExitFromContacts = True

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub

    Private Sub ShipHistoryButton_Click(sender As Object, e As RoutedEventArgs) Handles ShipHistoryButton.Click

        Dim PassCode As Integer
        PassCode = ValidateAccess(gCurrentUser, "F", "SHIPPING", "Shipping History")
        If gRegistrationExpired = True Or PassCode = 1 Then

            Exit Sub

        End If
        Try


            Dim win As New ShipmentHistory(Me)
            win.ShowDialog(Me)

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub

    Private Sub Me_Initialized(sender As Object, e As EventArgs) Handles Me.Initialized

        If gRegistrationExpired = True Then

            Exit Sub

        End If
        Try
            If Me.winListPointer = 0 Then
                Dim Window As New SplashScreen()
                Window.Show()
            End If

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try

    End Sub

    Private Overloads Sub RefreshButton_Click(sender As Object, e As RoutedEventArgs)

        If gRegistrationExpired = True Then

            Exit Sub

        End If
        Try

            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location, "-r")
            Application.Current.Shutdown()

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub

    Private Sub Clear_Window_History_Click(sender As Object, e As RoutedEventArgs) Handles Clear_Window_History.Click

        If gRegistrationExpired = True Then

            Exit Sub

        End If
        Try
            CommonWindowStack.ClearWindowList()
            MessageBox.Show("Window History Cleared!", "SHIPRITE", Forms.MessageBoxButtons.OK, Forms.MessageBoxIcon.Information)

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub

    Private Sub TicketImage_Button_Click(sender As Object, e As RoutedEventArgs) Handles TicketImage_Button.Click

        Try

            Process.Start("http://support.shiprite.net/")

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub

    Private Sub SupportImage_Button_Click(sender As Object, e As RoutedEventArgs) Handles SupportImage_Button.Click

        If gRegistrationExpired = True Then

            Exit Sub

        End If
        Try

            Process.Start("https://www.fastsupport.com/")

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub

    Private Sub ShopImage_Button_Click(sender As Object, e As RoutedEventArgs) Handles ShopImage_Button.Click

        If gRegistrationExpired = True Then

            Exit Sub

        End If
        Try

            Process.Start("https://shipritenext.com/shop/")

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub

    Private Sub FacebookImage_Button_Click(sender As Object, e As RoutedEventArgs) Handles FacebookImage_Button.Click

        If gRegistrationExpired = True Then

            Exit Sub

        End If
        Try

            Process.Start("https://www.facebook.com/ShipRiteSoftware")

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub

    Private Sub TwitterImage_Button_Click(sender As Object, e As RoutedEventArgs) Handles TwitterImage_Button.Click

        If gRegistrationExpired = True Then

            Exit Sub

        End If
        Try

            Process.Start("https://twitter.com/ShipRiteUpdates")

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub

    Private Sub ARButton_Click(sender As Object, e As RoutedEventArgs) Handles EODButton.Click

        If gRegistrationExpired = True Then

            Exit Sub

        End If
        Try


            Dim win As New EOD_Manifest(Me)
            win.ShowDialog(Me)

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try

    End Sub

    Private Sub TimeClock_Btn_Click(sender As Object, e As RoutedEventArgs) Handles TimeClock_Btn.Click

        If gRegistrationExpired = True Then

            Exit Sub

        End If
        Try



            Dim wind As New UserLogIn(Me, "")
            wind.ShowDialog()
            If UserLogIn.isAllowed = False Then
                Exit Sub
            End If

            Dim win As New TimeClock(Me)
            win.ShowDialog(Me)

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub

    Private Sub Tickler_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Tickler_Btn.Click

        If gRegistrationExpired = True Then

            Exit Sub

        End If
        Try


            If gCurrentUser = "" And (gIsProgramSecurityEnabled Or gIsPOSSecurityEnabled Or gIsSetupSecurityEnabled) Then
                Dim wind As New UserLogIn(Me, "")
                wind.ShowDialog()

                If UserLogIn.isAllowed = False Then
                    Exit Sub
                End If

            End If

            Dim win As New Tickler(Me)
            win.ShowDialog(Me)

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try

    End Sub

    Private Sub Inventory_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Inventory_Btn.Click

        Dim PassCode As Integer
        PassCode = ValidateAccess(gCurrentUser, "", "Inventory", "Inventory Manager")
        If gRegistrationExpired = True Or PassCode = 1 Then

            Exit Sub

        End If
        Try


            Dim win As New InventoryManager(Me)
            win.ShowDialog(Me)

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub

    Private Sub Startup_Check_Registration()
        Dim MyFile As String
        Dim buf As String = String.Empty
        Dim buf2 As String
        Dim iloc As Integer
        Dim iloc2 As Integer
        Dim eName As String
        Dim eValue As String

        MyFile = Dir(gDBpath & "\*.KEY")

        If Not MyFile = "" Then

            Dim b As Boolean
            Dim ret As Integer

            b = ReadFile_ToEnd(gDBpath & "\" & MyFile, True, buf)

            If b = True Then

                buf = FlushOut(buf, vbCrLf, "|")

                Do Until buf = ""

                    iloc = InStr(1, buf, "|")
                    buf2 = Mid(buf, 0, iloc - 1)
                    buf = Mid(buf, iloc)
                    iloc2 = InStr(1, buf2, ",")
                    eName = Trim(Mid(buf2, 0, iloc2 - 1))
                    If String.Compare(eName, "phone", True) = 0 Then
                        eName = "Phone1"
                    ElseIf String.Compare(eName, "fax", True) = 0 Then
                        eName = "Phone2"
                    End If
                    eValue = Trim(Mid(buf2, iloc2))
                    ret = UpdatePolicy(gShipriteDB, eName, eValue)

                Loop
                MsgBox("ATTENTION...NEW Registration Key File Imported!", vbInformation, gProgramName)
                IO.File.Delete(gDBpath & "\" & MyFile)

            End If

        End If

        Dim RegString As String
        Dim RegCode As String
        Dim ExpireDate As Date
        Dim SupportDate As Date
        Dim i As Integer

        RegString = ""
        RegString += GetPolicyData(gShipriteDB, "Name")
        RegString += GetPolicyData(gShipriteDB, "FName")
        RegString += GetPolicyData(gShipriteDB, "LName")
        RegString += GetPolicyData(gShipriteDB, "Addr1")
        RegString += GetPolicyData(gShipriteDB, "Addr2")
        RegString += GetPolicyData(gShipriteDB, "City")
        RegString += GetPolicyData(gShipriteDB, "State")
        RegString += GetPolicyData(gShipriteDB, "Zip")
        RegString += GetPolicyData(gShipriteDB, "Phone1")
        RegString += GetPolicyData(gShipriteDB, "Phone2")

        RegCode = GetPolicyData(gShipriteDB, "RegistrationNumber")

        i = CheckRegistration(RegString, RegCode, "keisha", ExpireDate, SupportDate, gAccessCodes)

        Dim input As String = "0"
        Do While gRegistrationExpired And input <> ""
            input = InputBox("Current Key is expired/invalid" & vbCrLf & "Please Enter new Registration key", "New Reg Key Entry", "")

            CheckRegistration(RegString, input, "keisha", ExpireDate, SupportDate, gAccessCodes)

            If gRegistrationExpired = False Then
                UpdatePolicy(gShipriteDB, "RegistrationNumber", input)
            End If
        Loop

    End Sub

#Region "Gary Testing"
    Private Sub SearchBox_KeyDown(sender As Object, e As KeyEventArgs) Handles SearchBox.KeyDown

        If e.Key = Key.Return Then

            Select Case UCase(SearchBox.Text)

                ' Gary's sandbox functions...

                Case "GFORD-T1"  'This is used to do adhoc testing of functions and .Net capabilities...basically, this is where I try things out.

                    SearchBox.Text = ""

                    Dim SQL As String
                    Dim MyRecordSet As RecordSetDefinition
                    Dim RecordID As Long = 0
                    Dim FNum As Integer = 0
                    Dim Segment As String = ""
                    Dim ret As Long

                    SQL = InputBox("Enter SQL Statement", "")
                    If SQL = "" Then

                        Exit Sub

                    End If
                    ret = IO_GetSegmentSetInToStructure(gShipriteDB, SQL, MyRecordSet)
                    ret = DumpRecordSetToFile(MyRecordSet, "C:\ShipriteNext\dump.txt", ";")
                    MsgBox("Records Read " & ret.ToString)

                    RecordID = GetRecordID(MyRecordSet, "Name", "Ford, Gary")
                    Segment = MakeSegmentFromRecord(MyRecordSet, RecordID, True)
                    MsgBox(Segment)
                    FNum = GetFieldNumber(MyRecordSet, "City")
                    MsgBox("Field Number is " & FNum.ToString)

                    SearchBox.Focus()

                Case "GFORD-T2" ' This routine is used by Gary to test out snipets of code

                    Dim printer As PrintDocument = New PrintDocument ' prints to default printer
                    AddHandler printer.PrintPage,
                        Sub(ByVal sender2 As Object, ByVal e2 As PrintPageEventArgs)
                            Dim img As Image = Image.FromFile("C:\ShipriteNext\LOGO_Receipt.jpg")
                            e2.Graphics.DrawImage(img, 0, 0)
                        End Sub
                    printer.Print()

                Case "GFORD-T3"

                    ShipandInsure_IsTest = False
                    Dim CarrierCode As String = ShipandInsure_GetCarrierID("COM-GND")
                    Dim t As String = ShipandInsure_GetShipmentCost(GetPolicyData(gShipriteDB, "ShipAndInsureUserID"), "Shiprite", GetPolicyData(gShipriteDB, "ShipAndInsurePassword"), "Gary Ford", "10293", "1Z1140380394508780", CarrierCode, "500", "13403", "22104", "US")

                    MsgBox(t)

                    Dim r As String = ShipandInsure_SaveBulkUploadItem("21382", "Shiprite", "udMhk7qW", "Gary Ford", "10293", "1Z1140380311111240", CarrierCode, "500", "13403", "22104", "US")
                    MsgBox(r)

                    Dim y As String = ShipandInsure_DeleteBulkUploadItem("21382", "Shiprite", "udMhk7qW", "Gary Ford", "10293", "1Z1140380311111240", CarrierCode, "500", "13403", "22104", "US")
                    MsgBox(y)

                Case "GFORD-T4A"

                    Dim reqJson As New JObject()
                    reqJson("ID") = "-1"
                    reqJson("CustomerNumber") = "GFORD"
                    reqJson("Name") = "Gary Ford"
                    reqJson("TrackingNumber") = "1234838383"
                    reqJson("ShipmentDate") = Format(Today, "MM/dd/yy")
                    reqJson("ZipFrom") = "13502"
                    reqJson("ZipTo") = "13403"
                    reqJson("CarrierNumber") = "-1"
                    Dim reqJsonStr As String = reqJson.ToString()
                    MsgBox(reqJsonStr)

                    Dim parsejson As JObject = JObject.Parse(reqJsonStr)
                    Dim TheName = parsejson.SelectToken("Name").ToString()
                    MsgBox(TheName)







                    Dim json As Linq.JObject = Linq.JObject.Parse(reqJsonStr)
                    Dim CName As String = json.SelectToken("Name")
                    Dim TrackingNumber As String = json.SelectToken("ShipmentDate")
                    MsgBox(CName)
                    MsgBox(TrackingNumber)

                Case "GFORD-T4"

                    ShipandInsure_IsTest = False
                    Dim CarrierCode As String = ShipandInsure_GetCarrierID("FEDEX-STD")
                    Dim t As String = ShipandInsure_GetShipmentCost(GetPolicyData(gShipriteDB, "ShipAndInsureUserID"), "Shiprite", GetPolicyData(gShipriteDB, "ShipAndInsurePassword"), "Gary Ford", "10293", "1Z1140380394508780", CarrierCode, "5000", "13403", "22104", "US")
                    MsgBox(t)

                Case "GFORD-T5"

                    Dim buf As String
                    buf = GetPolicyData(gShipriteDB, "UPSOauthToken")

                    Dim clientId As String = "sYmHwIqvGxywW6XS1XiJMbDOt4caS5u77FtKXWFamyoq313I"
                    Dim clientSecret As String = "8Zo36B0c2JnyZ4Z0ZQRnVjXV41tqEqsy2XehpKOJVWl8PqXdXnxS0dgsTlCOucka"
                    Dim urlCallback As String = "http://localhost:5000/callback/"
                    Dim urlAuthClient As String = "https://wwwcie.ups.com/security/v1/oauth/token"
                    Dim urlAuthClientLogin As String = "https://wwwcie.ups.com/security/v1/oauth/authorize?client_id=" & clientId & "&redirect_uri=" & urlCallback & "&response_type=code&state=&scope=read"
                    Dim shipperAcctNum As String = "9Q7Z5J"
                    Dim code As String = ""
                    Dim ret As Long

                    If Not buf = "" Then

                        Dim ExpirationDate As Date = GetPolicyData(gShipriteDB, "")

                    End If
                    ret = MsgBox("ATTENTION...Your UPS OAUTH Token Needs to be Refreshed" & vbCrLf & vbCrLf & "You will be sent To a UPS Login Page to Authenticate.  You" & vbCrLf & "will be required to enter your UPS UserID and Password.", vbOK, "ShipriteNext")
                    If ret = vbCancel Then

                        Exit Sub

                    End If

                    ' Authorize Client request
                    ' open auth url in default browser for user to login
                    Process.Start(urlAuthClientLogin)

                    ' start listening on localhost callback port 5000
                    Using listener As New HttpListener
                        listener.Prefixes.Add(urlCallback)
                        listener.Start()

                        ' wait for response callback on port 5000 while user logs into their UPS account in the default browser
                        Dim context As HttpListenerContext = listener.GetContext()

                        ' callback received with 'code' to be used for Generating Token
                        Dim request As HttpListenerRequest = context.Request
                        code = request.QueryString.Get("code")
                        context.Response.Close()
                        listener.Stop()
                    End Using

                    If Not String.IsNullOrEmpty(code) Then
                        ' GenerateToken request
                        Net.ServicePointManager.SecurityProtocol = Net.SecurityProtocolType.Tls12
                        Dim client As New RestClient(urlAuthClient)
                        client.Authenticator = New Authenticators.HttpBasicAuthenticator(clientId, clientSecret)
                        Dim request As New RestRequest(Method.POST)
                        request.AddHeader("Content-Type", "application/x-www-form-urlencoded")
                        request.AddParameter("grant_type", "authorization_code")
                        request.AddParameter("code", code)
                        request.AddParameter("redirect_uri", urlCallback)

                        Dim response As IRestResponse = client.Execute(request)
                        If response.StatusCode = HttpStatusCode.OK Then

                            Dim result As JObject = JObject.Parse(response.Content)
                            Dim accessToken As String = result.SelectToken("access_token")
                            Dim accessTokenExpiresIn As String = result.SelectToken("expires_in")
                            Dim refreshToken As String = result.SelectToken("refresh_token")
                            Dim refreshTokenExpiresIn As String = result.SelectToken("refresh_token_expires_in")
                            Dim accessTokenExpiresInDate As Date = Now + TimeSpan.FromSeconds(accessTokenExpiresIn)
                            Dim refreshTokenExpiresInDate As Date = Now + TimeSpan.FromSeconds(refreshTokenExpiresIn)
                            ret = UpdatePolicy(gShipriteDB, "UPSOauthToken", accessToken)
                            ret = UpdatePolicy(gShipriteDB, "UPSOauthTokenExpires", accessTokenExpiresInDate)
                            ret = UpdatePolicy(gShipriteDB, "UPSOauthRefreshToken", refreshToken)
                            ret = UpdatePolicy(gShipriteDB, "UPSOauthRefreshTokenExpires", refreshTokenExpiresInDate)
                            MsgBox("ATTENTION...Token Refreshed, you may continue working.", vbInformation, "ShipriteNext")

                        End If

                    End If

                Case "GFORD-T6"

                    Dim accessToken As String = GetPolicyData(gShipriteDB, "UPSOauthToken")
                    If accessToken = "" Then

                        Dim ans As Integer = MsgBox("ATTENTION...UPS Requires a Periodic Login Authentication " & vbCrLf & "before communicating with their server.  This requires" & vbCrLf & "YOUR UPS UserID and Password." & vbCrLf & vbCrLf & "CONTINUE???", vbQuestion + vbYesNo, "SHIPRITE NEXT UPS INTEGRATION")
                        If ans = vbNo Then

                            Exit Sub

                        End If

                    End If
                    Dim accessTokenExpires As Date = GetPolicyData(gShipriteDB, "UPSOauthTokenExpires")
                    Dim refreshToken As String = GetPolicyData(gShipriteDB, "UPSOauthRefreshToken")
                    Dim refreshTokenExpires As Date = GetPolicyData(gShipriteDB, "UPSOauthRefreshTokenExpires")
                    Dim clientId As String = "sYmHwIqvGxywW6XS1XiJMbDOt4caS5u77FtKXWFamyoq313I"
                    Dim clientSecret As String = "8Zo36B0c2JnyZ4Z0ZQRnVjXV41tqEqsy2XehpKOJVWl8PqXdXnxS0dgsTlCOucka"
                    'Dim shipperAcctNum As String = "9Q7Z5J"
                    Dim shipperAcctNum As String = "114038"

                    ' refresh token?
                    If accessToken = "" OrElse accessTokenExpires = "1/1/0001" OrElse accessTokenExpires.AddSeconds(-10) <= Date.Now Then
                        ' access token expired
                        If refreshToken = "" OrElse refreshTokenExpires = "1/1/0001" OrElse refreshTokenExpires.AddSeconds(-10) <= Date.Now Then
                            ' refresh token expired
                            ' can't refresh - need to do full authentication with login
                        Else
                            ' access token expired, refresh token not expired
                            ' send refresh token request
                            Net.ServicePointManager.SecurityProtocol = Net.SecurityProtocolType.Tls12
                            Dim urlRefreshToken As String = "https://wwwcie.ups.com/security/v1/oauth/refresh"
                            Dim rClient As New RestClient(urlRefreshToken)
                            rClient.Authenticator = New Authenticators.HttpBasicAuthenticator(clientId, clientSecret)
                            Dim rRequest As New RestRequest(Method.POST)
                            rRequest.AddHeader("Content-Type", "application/x-www-form-urlencoded")
                            rRequest.AddParameter("grant_type", "refresh_token")
                            rRequest.AddParameter("refresh_token", refreshToken)

                            Dim rResponse As IRestResponse = rClient.Execute(rRequest)
                            If rResponse.StatusCode = HttpStatusCode.OK Then
                                Dim result As JObject = JObject.Parse(rResponse.Content)
                                accessToken = result.SelectToken("access_token")
                                Dim accessTokenExpiresIn As String = result.SelectToken("expires_in")
                                refreshToken = result.SelectToken("refresh_token")
                                Dim refreshTokenExpiresIn As String = result.SelectToken("refresh_token_expires_in")
                                accessTokenExpires = Now + TimeSpan.FromSeconds(accessTokenExpiresIn)
                                refreshTokenExpires = Now + TimeSpan.FromSeconds(refreshTokenExpiresIn)
                                UpdatePolicy(gShipriteDB, "UPSOauthToken", accessToken)
                                UpdatePolicy(gShipriteDB, "UPSOauthTokenExpires", accessTokenExpires)
                                UpdatePolicy(gShipriteDB, "UPSOauthRefreshToken", refreshToken)
                                UpdatePolicy(gShipriteDB, "UPSOauthRefreshTokenExpires", refreshTokenExpires)
                            End If
                        End If
                    End If

                    Dim fileName As String = "Test.txt"
                    Dim fileFormat As String = "txt"
                    Dim fileBuffer As String = "Now Is the time for all good men To come to the aid of their country."
                    Dim encFileBuffer As String = _Convert.StringToBase64(fileBuffer)
                    Dim formType As String = "013"
                    Dim urlUploadPaperlessDoc As String = "https://wwwcie.ups.com/api/paperlessdocuments/v2/upload"

                    Dim jsonReq As JObject = JObject.Parse(
                    "{
                        UploadRequest: {
                            Request: {
                                TransactionReference: {
                                    CustomerContext: ''
                                }
                            },
                            UserCreatedForm: {
                                UserCreatedFormFileName: '" & fileName & "',
                                UserCreatedFormFileFormat: '" & fileFormat & "',
                                UserCreatedFormDocumentType: '" & formType & "',
                                UserCreatedFormFile: '" & encFileBuffer & "'
                            }
                        }
                    }")

                    Dim jsonReqStr As String = jsonReq.ToString()
                    WriteFile_ToEnd(jsonReqStr, "c:\shipritenext\JSON_Payload.txt")

                    Net.ServicePointManager.SecurityProtocol = Net.SecurityProtocolType.Tls12
                    Dim client As New RestClient(urlUploadPaperlessDoc)
                    Dim request As New RestRequest(Method.POST)
                    request.AddHeader("Authorization", "Bearer " & accessToken)
                    request.AddHeader("Content-Type", "application/json")
                    request.AddHeader("ShipperNumber", shipperAcctNum)
                    request.AddHeader("transactionSrc", "testing")
                    request.AddParameter("application/json", jsonReqStr, ParameterType.RequestBody)

                    'MsgBox(request.Body.ToString)
                    Dim response As IRestResponse = client.Execute(request)

                    If response.StatusCode = HttpStatusCode.OK Then

                        Dim result As JObject = JObject.Parse(response.Content)
                        'Dim accessToken As String = result.SelectToken("access_token")

                    Else

                        Dim jsonResp As JObject = JObject.Parse(response.Content)
                        Dim jsonRespStr = jsonResp.ToString
                        WriteFile_ToEnd(jsonRespStr, "c:\shipritenext\JSON_Payload-Response.txt")
                        MsgBox(jsonRespStr)

                    End If

                Case "GFORD-T7"

                    Dim FileBuffer As String = IO.File.ReadAllText("c:\shipritenext\COMMERCIAL INVOICE TEMPLATE.rtf")
                    Dim iloc = InStr(1, FileBuffer, "SHIPPERNAME1.....................35")
                    Dim buf = GetPolicyData(gShipriteDB, "Name")
                    buf = String.Format("{0,-35}", buf)
                    FileBuffer = FlushOut(FileBuffer, "SHIPPERNAME1.....................35", buf)
                    FileBuffer = FlushOut(FileBuffer, "DDDDDDDDDDDDDDD", Format(Today, "dd-MMM-yyyy"))
                    WriteFile_ToEnd(FileBuffer, "c:\shipritenext\test.rtf")

                Case "H-T1"

                    Dim SQL As String = "SELECT * FROM PreShip ORDER BY ID"
                    Dim SegmentSet As String = IO_GetSegmentSet(gShipriteDB, SQL)
                    Dim Segment As String

                    Do Until SegmentSet = ""

                        Segment = GetNextSegmentFromSet(SegmentSet)
                        MsgBox(Segment)

                    Loop

                Case "GFORD-T8"

                    Dim SQL As String
                    Dim SegmentSet As String
                    SQL = "SELECT AcctNum, AcctName FROM Payments GROUP BY AcctNum, AcctName"
                    SegmentSet = IO_GetSegmentSet("c:\shiprite\shiprite.mdb", SQL)
                    MsgBox(SegmentSet)

            End Select

        End If
    End Sub
#End Region

    Private Sub User_Btn_Click(sender As Object, e As RoutedEventArgs) Handles User_Btn.Click
        Dim win As New UserLogIn()
        win.ShowDialog()
    End Sub

    Private Sub Updater_Check_Click(sender As Object, e As RoutedEventArgs) Handles Updater_Check.Click
        UpdaterModule.Start_CheckNow()
    End Sub

    Private Sub Updater_Configure_Click(sender As Object, e As RoutedEventArgs) Handles Updater_Configure.Click
        UpdaterModule.Start_Configure()
    End Sub



    Private Class SRN_Updater
        Private Const ModuleName As String = "SRN Updater"
        Private Const ModuleFileName As String = "SRN_Updater.exe"
        Private ModulePath As String

        Public Sub New()
            ModulePath = Path.Combine(Path.GetDirectoryName(Reflection.Assembly.GetExecutingAssembly().Location), ModuleFileName)
        End Sub

        Public Sub ThreadStart_Silent()
            Dim updaterThread As New Thread(New ThreadStart(AddressOf Start_Silent))
            updaterThread.Start()
        End Sub

        Public Sub Start_CheckNow()
            Start("/checknow")
        End Sub
        Public Sub Start_Configure()
            Start("/configure")
        End Sub
        Public Sub Start_Silent()
            Thread.Sleep(10000) ' wait 10 secs before starting
            Start("/silent", True)
        End Sub

        Public Function Start(opts As String, Optional isSilent As Boolean = False) As Integer?
            Dim procRet As Integer? = Nothing
            Dim maxWaitms As Integer = 0
            '
            If Not isSilent Then
                maxWaitms = 60000 ' wait 1 min max
            End If
            '
            If gRegistrationExpired Then
                If Not isSilent Then _MsgBox.InformationMessage("Registration Expired!", "Updater not started...", ModuleName)
            ElseIf Not _Files.IsFileExist(ModulePath, False) Then
                If Not isSilent Then _MsgBox.WarningMessage("Updater Missing!", "Failed to start Updater...", ModuleName)
            Else
                Try
                    Dim proc As Process = Process.Start(ModulePath, opts)
                    If Not isSilent Then
                        proc.WaitForExit(maxWaitms)
                        If proc.HasExited Then
                            procRet = proc.ExitCode()
                        End If
                    End If
                    proc.Close()
                Catch ex As Exception
                    _MsgBox.ErrorMessage(ex, "Failed to start Updater...", ModuleName)
                End Try
            End If
            '
            Return procRet
        End Function
    End Class

End Class

