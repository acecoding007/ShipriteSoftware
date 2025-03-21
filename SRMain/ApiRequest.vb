Imports System.Collections.ObjectModel
Imports System.Data.OleDb
Imports System.IO
Imports System.Net
Imports System.Net.Mail
Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Web
Imports ICSharpCode.SharpZipLib.BZip2
Imports Newtonsoft.Json.Linq
Imports RestSharp

Module ApiRequest
    Public apiKey As String = "c3UMitpCIGxr8WdaJ4uMBirUlOKMFTXb7E9Vu6dUdH" ' TODO: replace this with more secure method (preferably with cryptlex)
    Private postCodeCacheDB As JObject
    Private codeDatabase As String = gDBpath & "\PostCodes.json"
    Public Function liminal(endpoint As String, payload As Dictionary(Of String, String)) As String
        Dim payloadString As String = ""
        For Each parameter As KeyValuePair(Of String, String) In payload
            payloadString = payloadString & parameter.Key & "=" & parameter.Value.Replace("&", "%26") & "&"
        Next
        payloadString = payloadString.Substring(0, payloadString.Length - 1)
        Return liminal(endpoint, payloadString)
    End Function
    Public Function liminal(endpoint As String, payload As String) As String
        ' Send POST request to intermediate API
        Dim LiminalPort As String = "1337"
        '  Production: 1337
        '  Development: 7284
        endpoint = "http://api.shiprite.net:" & LiminalPort & "/" & endpoint
        Return rest(endpoint, payload, "POST")
    End Function
    Public Function liminalAsObject(endpoint As String, parameters As String) As Object
        Dim apiResult As String = liminal(endpoint, parameters)
        Dim output As Object = Nothing
        If IsValidJson(apiResult) Then
            output = JObject.Parse(apiResult)
        Else
            Debug.Print("NON-JSON result from API:" & Environment.NewLine & apiResult)
        End If
        Return output
    End Function
    Public Function liminalAsObject(endpoint As String, parameters As Dictionary(Of String, String)) As Object
        Dim payloadString As String = ""
        For Each parameter As KeyValuePair(Of String, String) In parameters
            payloadString = payloadString & parameter.Key & "=" & parameter.Value & "&"
        Next
        payloadString = payloadString.Substring(0, payloadString.Length - 1)
        Return liminalAsObject(endpoint, payloadString)
    End Function
    Public Function rest(endpoint As String, payload As Dictionary(Of String, String), methodStr As String) As String
        Dim payloadString As String = ""
        For Each parameter As KeyValuePair(Of String, String) In payload
            payloadString = payloadString & parameter.Key & "=" & parameter.Value & "&"
        Next
        payloadString = payloadString.Substring(0, payloadString.Length - 1)
        Return rest(endpoint, payloadString, methodStr)
    End Function
    Public Function rest(endpoint As String, payloadString As String, methodStr As String) As String
        ' send HTTP request to REST API
        Debug.Print(payloadString)
        Dim response As String = Nothing
        Try
            If methodStr = "GET" Then
                endpoint = endpoint & "?" & payloadString
            End If
            Dim client As RestClient = New RestClient(endpoint)
            Dim RequestMethod As Method
            Select Case methodStr
                Case "POST"
                    RequestMethod = Method.POST
                Case "Get"
                    RequestMethod = Method.GET
            End Select
            Dim req As RestRequest = New RestRequest(RequestMethod)
            If methodStr <> "GET" Then
                req.AddHeader("content-type", "application/x-www-form-urlencoded")
                req.AddParameter("application/x-www-form-urlencoded", payloadString, ParameterType.RequestBody)
            End If
            Dim res As IRestResponse = client.Execute(req)
            response = res.Content
        Catch ex As Exception
            ' MessageBox.Show(ex.ToString, windowTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            Debug.Print(ex.ToString)
        End Try
        Return response
    End Function
    Public Function restAsObject(endpoint As String, payload As Dictionary(Of String, String), methodStr As String) As Object
        Dim payloadString As String = ""
        For Each parameter As KeyValuePair(Of String, String) In payload
            payloadString = payloadString & parameter.Key & "=" & parameter.Value & "&"
        Next
        payloadString = payloadString.Substring(0, payloadString.Length - 1)
        Return restAsObject(endpoint, payloadString, methodStr)
    End Function
    Public Function restAsObject(endpoint As String, payloadString As String, methodStr As String) As Object
        Dim apiResult As String = rest(endpoint, payloadString, methodStr)
        Return JObject.Parse(apiResult)
    End Function
    Public Function restAsByte(endpoint As String, payloadString As String) As Byte()
        Try
            Dim data As Byte()
            Using Client As New WebClient()
                data = Client.DownloadData(endpoint & "?" & payloadString)
            End Using
            Return data
        Catch ex As Exception
            Debug.Print(ex.ToString)
            Return Nothing
        End Try
    End Function
    Public Sub restAsFile(endpoint As String, payloadString As String, fileOutput As String)
        Using client As New WebClient()
            client.DownloadFile(endpoint & "?" & payloadString, fileOutput)
        End Using
    End Sub

#Region "Email"
    ''' <summary>
    ''' Sends an email. Every email address in recipients receives the same email.
    ''' </summary>
    ''' <param name="recipients"></param>
    ''' <param name="subject"></param>
    ''' <param name="content"></param>
    ''' <param name="isHtml"></param>
    ''' <returns></returns>
    Public Function sendEmail(recipients() As String, subject As String, content As String, Optional isHtml As Boolean = True) As Boolean
        Try
            Dim parameters = New Dictionary(Of String, String)
            Dim recipientString As String = ""
            For Each recipient As String In recipients
                recipientString = recipientString & recipient & ","
            Next
            recipientString = recipientString.TrimEnd(CChar(","))
            Dim user As String = GetPolicyData(gShipriteDB, "Notify_Email")
            Dim pass As String = GetPolicyData(gShipriteDB, "Notify_Password")
            Dim smtpserv As String = GetPolicyData(gShipriteDB, "Notify_SmtpServer")
            If String.IsNullOrEmpty(user) Or String.IsNullOrEmpty(pass) Or String.IsNullOrEmpty(smtpserv) Then
                Throw New Exception("Missing email settings.")
                Return False
            End If
            parameters.Add("key", HttpUtility.UrlEncode(apiKey))
            parameters.Add("host", HttpUtility.UrlEncode(_Convert.Base64ToString(smtpserv)))
            parameters.Add("port", HttpUtility.UrlEncode(_Convert.Base64ToString(GetPolicyData(gShipriteDB, "Notify_SmtpPort", _Convert.StringToBase64("587")))))
            parameters.Add("secure", Get_SMTP_Secure_Setting())
            parameters.Add("user", HttpUtility.UrlEncode(user))
            parameters.Add("pass", HttpUtility.UrlEncode(pass))
            parameters.Add("dest", HttpUtility.UrlEncode(_Convert.StringToBase64(recipientString)))
            parameters.Add("subject", HttpUtility.UrlEncode(_Convert.StringToBase64(subject)))
            Dim ProcessedMessage As String = HttpUtility.UrlEncode(_Convert.StringToBase64(content))
            parameters.Add("msg", ProcessedMessage)
            parameters.Add("html", isHtml.ToString())
            Dim res = liminalAsObject("email", parameters)
            If Not res("status") Then
                Dim Reason As String = res("reason")
                If Reason.ToLower().Contains("ssl") Then
                    ' SSL error
                    MessageBox.Show("There was an SSL Error With the SMTP server settings. Please check your settings And Try again.", "Send Email SMTP Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    Return False
                Else
                    Throw New Exception(Reason)
                End If
            End If
            Return res("status")
        Catch ex As Exception
            MessageBox.Show(ex.ToString, "Send Email", MessageBoxButton.OK, MessageBoxImage.Error)
            Return False
        End Try
    End Function

    Public Function sendEmailWithAttachment(recipient As String, subject As String, content As String, file As Attachment, Optional isHtml As Boolean = True, Optional DisplayError As Boolean = True) As Boolean
        Try
            Dim smtpServer As String = _Convert.Base64ToString(GetPolicyData(gShipriteDB, "Notify_SmtpServer"))
            Dim smtpPort As Integer = CInt(_Convert.Base64ToString(GetPolicyData(gShipriteDB, "Notify_SmtpPort")))

            Dim smtpClient As New SmtpClient(smtpServer, smtpPort)
            smtpClient.EnableSsl = GetPolicyData(gShipriteDB, "Notify_SmtpEncrypted")

            Dim user As String = _Convert.Base64ToString(GetPolicyData(gShipriteDB, "Notify_Email"))
            Dim pass As String = _Convert.Base64ToString(GetPolicyData(gShipriteDB, "Notify_Password"))

            smtpClient.Credentials = New System.Net.NetworkCredential(user, pass)

            Dim mailMessage As New MailMessage()
            mailMessage.From = New MailAddress(user)

            mailMessage.To.Add(recipient)

            mailMessage.Subject = subject
            mailMessage.Body = content
            mailMessage.IsBodyHtml = isHtml
            mailMessage.Attachments.Add(file)

            smtpClient.Send(mailMessage)
            Return True
        Catch ex As Exception
            If DisplayError Then
                MessageBox.Show(ex.Message, "Send Email", MessageBoxButton.OK, MessageBoxImage.Error)
            End If

            Return False
        End Try
    End Function
    Private Function Get_SMTP_Secure_Setting() As String
        Dim isSecure As Boolean
        isSecure = GetPolicyData(gShipriteDB, "Notify_SmtpEncrypted", "True")
        Return Not isSecure
    End Function
    Public Function sendEmail(recipient As String, subject As String, content As String, Optional isHtml As Boolean = True) As Boolean
        Dim recipients() As String = {recipient}
        Return sendEmail(recipients, subject, content, isHtml)
    End Function
    Public Function sendTestEmail(account As String, accountPass As String, smtpServer As String, smtpPort As String, smtpEncrypted As Boolean) As Boolean
        Try
            Dim parameters = New Dictionary(Of String, String)
            Dim smtpserv As String = smtpServer
            If String.IsNullOrEmpty(account) Or String.IsNullOrEmpty(accountPass) Or String.IsNullOrEmpty(smtpserv) Then
                Throw New Exception("Missing email settings.")
                Return False
            End If
            parameters.Add("key", HttpUtility.UrlEncode(apiKey))
            parameters.Add("host", HttpUtility.UrlEncode(smtpserv))
            Dim smtpServerPort As String = "587"
            If Not String.IsNullOrEmpty(smtpPort) Then
                smtpServerPort = smtpPort
            End If
            parameters.Add("port", HttpUtility.UrlEncode(smtpServerPort))
            parameters.Add("secure", (Not smtpEncrypted).ToString)
            parameters.Add("user", HttpUtility.UrlEncode(_Convert.StringToBase64(account)))
            parameters.Add("pass", HttpUtility.UrlEncode(_Convert.StringToBase64(accountPass)))
            parameters.Add("dest", HttpUtility.UrlEncode(_Convert.StringToBase64(account)))
            parameters.Add("subject", HttpUtility.UrlEncode(_Convert.StringToBase64("Test Email from " & gProgramName)))
            Dim ProcessedMessage As String = HttpUtility.UrlEncode(_Convert.StringToBase64(File.ReadAllText(gTemplatesPath & "\TestEmail.html")))
            parameters.Add("msg", ProcessedMessage)
            parameters.Add("html", "True")
            Dim res = liminalAsObject("email", parameters)
            If Not IsNothing(res) Then
                If Not res("status") Then
                    Dim Reason As String = res("reason")
                    If Reason.ToLower().Contains("ssl") Then
                        ' SSL error
                        MessageBox.Show("There was an SSL Error With the SMTP server settings. Please check your settings And Try again.", "Send Email SMTP Error", MessageBoxButton.OK, MessageBoxImage.Error)
                        Return False
                    Else
                        Throw New Exception(Reason)
                    End If
                End If
                Return res("status")
            Else
                Return False
            End If
        Catch ex As Exception
            MessageBox.Show(ex.ToString, "Send Email", MessageBoxButton.OK, MessageBoxImage.Error)
            Return False
        End Try
    End Function
    ''' <summary>
    ''' Sends an email. Every email address in recipients receives the same email.
    ''' </summary>
    ''' <param name="recipients"></param>
    ''' <param name="email"></param>
    ''' <param name="isHtml"></param>
    ''' <returns></returns>
    Public Function sendEmail(recipients() As String, email As EmailTemplate, Optional isHtml As Boolean = True) As Boolean
        Return sendEmail(recipients, email.Subject, email.Content, isHtml)
    End Function

    ''' <summary>
    ''' Sends an email. Every email address in recipients receives the same email.
    ''' </summary>
    ''' <param name="recipient"></param>
    ''' <param name="email"></param>
    ''' <param name="isHtml"></param>
    ''' <returns></returns>
    Public Function sendEmail(recipient As String, email As EmailTemplate, Optional isHtml As Boolean = True) As Boolean
        Return sendEmail({recipient}, email.Subject, email.Content, isHtml)
    End Function

    ''' <summary>
    ''' Gets the specified template, content is HTML.
    ''' Template options are in policy and begin with Notify_, but you must not include Content or Subject at the end.
    ''' </summary>
    ''' <param name="type"></param>
    ''' <returns></returns>
    Public Function getEmailTemplate(type As String, customerName As String) As EmailTemplate
        ' gets template as HTML

        Dim template As New EmailTemplate
        template.Type = type
        template.Content = GetPolicyData(gShipriteDB, type & "Content", "")
        template.Subject = GetPolicyData(gShipriteDB, type & "Subject", "")
        If String.IsNullOrEmpty(template.Content) And String.IsNullOrEmpty(template.Subject) Then
            ' empty?! uwu
            Debug.Print("The template data for " & type & "is incomplete.")
            Return Nothing
        Else
            Try
                template.Content = _Convert.Base64ToString(template.Content)
                If type.StartsWith("Notify_Email") Then
                    ' ok, we have the template in RTF, but we need it in HTML
                    template.Content = RtfPipe.Rtf.ToHtml(template.Content)
                End If

                If type.StartsWith("Letter_") Then
                    ' ok, we have the template in RTF, but we need it in HTML
                    template.Content = RtfPipe.Rtf.ToHtml(template.Content)
                End If


                'Store info is same for all notifications and should be preloaded.
                If customerName <> "" Then
                    template.Content = template.Content.Replace("%Customer%", customerName)
                Else
                    template.Content = template.Content.Replace("%Customer%", "Customer")
                End If
                template.Content = template.Content.Replace("%StoreOwnerName%", _StoreOwner.StoreOwner.FNameLName)
                template.Content = template.Content.Replace("%StoreName%", _StoreOwner.StoreOwner.CompanyName)
                template.Content = template.Content.Replace("%StoreAddress%", _StoreOwner.StoreOwner.Address)
                template.Content = template.Content.Replace("%StorePhone%", _StoreOwner.StoreOwner.Tel)

                Return template
            Catch ex As Exception
                MessageBox.Show(ex.ToString, "Get Template Content", MessageBoxButton.OK, MessageBoxImage.Error)
                Debug.Print(ex.StackTrace)
                Return Nothing
            End Try
        End If
        Return Nothing
    End Function

    Public Function getLetterTemplate(Content As String, customerName As String) As String
        ' gets template as HTML



        Try
            'Store info is same for all notifications and should be preloaded.
            If customerName <> "" Then
                Content = Content.Replace("%Customer%", customerName)
            End If
            Content = Content.Replace("%StoreOwnerName%", _StoreOwner.StoreOwner.FNameLName)
            Content = Content.Replace("%StoreName%", _StoreOwner.StoreOwner.CompanyName)
            Content = Content.Replace("%StoreAddress%", _StoreOwner.StoreOwner.Address)
            Content = Content.Replace("%StorePhone%", _StoreOwner.StoreOwner.Tel)

            Return Content
        Catch ex As Exception
            MessageBox.Show(ex.ToString, "Get Template Content", MessageBoxButton.OK, MessageBoxImage.Error)
            Debug.Print(ex.StackTrace)
            Return Nothing
        End Try

        Return Nothing
    End Function

    ''' <summary>
    ''' Generates string of HTML for a dropoff receipt email
    ''' </summary>
    ''' <param name="packages"></param>
    ''' <param name="ContactName"></param>
    ''' <param name="dropDate"></param>
    ''' <returns></returns>
    Public Function createDropoffEmail(packages As ObservableCollection(Of DropOffInformation), ContactName As String, dropDate As String) As String
        Dim receiptFile As String = File.ReadAllText(gTemplatesPath & "\DropOffReceipt.html")
        Dim packageFile As String = File.ReadAllText(gTemplatesPath & "\DropOffReceipt-Package.html")
        Dim addr2File As String = File.ReadAllText(gTemplatesPath & "\DropOffReceipt-Addr2.html")
        Dim disclaimerText As String = File.ReadAllText(gTemplatesPath & "\DropOff_Disclaimer.txt")
        Dim packFeeHtml As String = File.ReadAllText(gTemplatesPath & "\DropOffReceipt-PackFee.html")
        Dim store As New _baseContact
        ShipRiteDb.Setup_GetAddress_StoreOwner(store)
        ' Build store info
        receiptFile = Regex.Replace(receiptFile, "CompanyName", store.CompanyName)
        receiptFile = Regex.Replace(receiptFile, "AddressL1", store.Addr1)
        If Not String.IsNullOrEmpty(store.Addr2) Then
            receiptFile = Regex.Replace(receiptFile, "\<\!\-\-Addr2\-\-\>", addr2File)
            receiptFile = Regex.Replace(receiptFile, "AddressL2", store.Addr2)
        End If
        receiptFile = Regex.Replace(receiptFile, "AddressCityStateZip", store.CityStateZip)
        receiptFile = Regex.Replace(receiptFile, "StorePhone", store.Tel)
        ' Drop Off Date
        receiptFile = Regex.Replace(receiptFile, "DateHere", dropDate)
        ' Add Contact's Name
        receiptFile = Regex.Replace(receiptFile, "ReceiptName", ContactName)
        ' Build Package list
        Dim packageHtml As String = ""
        Dim packfee As String = ""
        For Each pkg As DropOffInformation In packages
            Dim builtPackage As String = Regex.Replace(packageFile, "CarrierName", pkg.CarrierName)
            builtPackage = Regex.Replace(builtPackage, "TrackingNumberHere", pkg.trackingNumber)
            builtPackage = Regex.Replace(builtPackage, "NotesHere", pkg.DropOffNotes)
            packfee = pkg.PackagingFee.ToString
            If Not String.IsNullOrEmpty(packfee) And Not packfee = "0" Then
                ' Format as MONEYEZ
                Dim fee As Double
                Double.TryParse(packfee, fee)
                packfee = Regex.Replace(packFeeHtml, "FeeHERE", fee.ToString("C2"))
            Else
                packfee = ""
            End If
            builtPackage = Regex.Replace(builtPackage, "PACKFEE", packfee)
            packageHtml = packageHtml & builtPackage & Environment.NewLine
        Next
        receiptFile = Regex.Replace(receiptFile, "<PACKAGES>", packageHtml)
        ' Disclaimer Text
        Dim DisclaimerInjection As String = Regex.Replace(disclaimerText, "\n", "<br/>")
        receiptFile = Regex.Replace(receiptFile, "DISCLAIMER", DisclaimerInjection)
        Return receiptFile
    End Function
#End Region

#Region "GeoData"
    ' TODO: This could use some cleaning up, but there's other priorities for now
    ''' <summary>
    '''Converts a postal code (ZIP Code) to the City
    ''' Do not run in main thread, as may take a while when pulling latest data
    ''' </summary>
    ''' <param name="postalcode"></param>
    ''' <returns></returns>
    Public Function PostCodeToCity(postalcode As String) As String
        Dim CurrentDBVersion As Integer = GetPolicyData(gShipriteDB, "PostCodeDBVer", -1)
        If CurrentDBVersion < 1 AndAlso InternetIsAvailable() Then
            ' DB Not initialized
            downloadDatabase()
            ' run query
            Return GetCityFromPostCode(postalcode)
        Else
            If InternetIsAvailable() Then
                ' Check DB Version compared to remote version
                Dim RemoteVersion As Integer = LatestDBVersion()
                If RemoteVersion > 0 Then
                    If RemoteVersion > CurrentDBVersion Then
                        ' get latest DB
                        downloadDatabase()
                    End If
                    Return GetCityFromPostCode(postalcode)
                Else
                    Debug.Print("Cannot retrieve latest DB version")
                    Return GetCityFromPostCode(postalcode)
                End If
            Else
                Return GetCityFromPostCode(postalcode)
            End If
        End If
    End Function
    Public Function PostCodeToState(postalcode As String) As String
        Dim CurrentDBVersion As Integer = GetPolicyData(gShipriteDB, "PostCodeDBVer", -1)
        If CurrentDBVersion < 1 AndAlso InternetIsAvailable() Then
            ' DB Not initialized
            downloadDatabase()
            ' run query
            Return GetStateFromPostCode(postalcode)
        Else
            If InternetIsAvailable() Then
                ' Check DB Version compared to remote version
                Dim RemoteVersion As Integer = LatestDBVersion()
                If RemoteVersion > 0 Then
                    If RemoteVersion > CurrentDBVersion Then
                        ' get latest DB
                        downloadDatabase()
                    End If
                    Return GetStateFromPostCode(postalcode)
                Else
                    Debug.Print("Cannot retrieve latest DB version")
                    Return GetStateFromPostCode(postalcode)
                End If
            Else
                Return GetStateFromPostCode(postalcode)
            End If
        End If
    End Function
    Public Function PostCodeToCountryCode(postalcode As String) As String
        Dim CurrentDBVersion As Integer = GetPolicyData(gShipriteDB, "PostCodeDBVer", -1)
        If CurrentDBVersion < 1 AndAlso InternetIsAvailable() Then
            ' DB Not initialized
            downloadDatabase()
            ' run query
            Return GetCountryCodeFromPostCode(postalcode)
        Else
            If InternetIsAvailable() Then
                ' Check DB Version compared to remote version
                Dim RemoteVersion As Integer = LatestDBVersion()
                If RemoteVersion > 0 Then
                    If RemoteVersion > CurrentDBVersion Then
                        ' get latest DB
                        downloadDatabase()
                    End If
                    Return GetCountryCodeFromPostCode(postalcode)
                Else
                    Debug.Print("Cannot retrieve latest DB version")
                    Return GetCountryCodeFromPostCode(postalcode)
                End If
            Else
                Return GetCountryCodeFromPostCode(postalcode)
            End If
        End If
    End Function
    Private Sub downloadDatabase()
        ' Clear Cached version
        postCodeCacheDB = Nothing
        ' Download archive
        Dim compressedJson As Byte() = restAsByte("http://api.shiprite.net:1337/postdb", "key=" & apiKey)
        ' Extract archive
        Dim decompressedJsonStream As MemoryStream = New MemoryStream
        Dim compressedJsonStream As MemoryStream = New MemoryStream(compressedJson)
        BZip2.Decompress(compressedJsonStream, decompressedJsonStream, False)
        compressedJsonStream.Close()
        File.WriteAllBytes(codeDatabase, decompressedJsonStream.ToArray())
        postCodeCacheDB = JObject.Parse(File.ReadAllText(codeDatabase))
        decompressedJsonStream.Close()
        ' Update stored DB Version
        Dim dbVerResult = liminalAsObject("postalcodes", "key=" & apiKey & "&op=0")
        If dbVerResult("status") Then
            UpdatePolicy(gShipriteDB, "PostCodeDBVer", dbVerResult("data"))
        End If
    End Sub
    Private Function GetCityFromPostCode(postcode As String) As String
        EnsureCodeDBLoaded()
        Try
            Dim city As String = ""
            If postCodeCacheDB.ContainsKey(postcode) Then
                city = postCodeCacheDB(postcode)(1)
            End If
            Return city
        Catch ex As Exception
            Return ""
        End Try
    End Function
    Private Function GetStateFromPostCode(postcode As String) As String
        EnsureCodeDBLoaded()
        Try
            Dim state As String = ""
            If postCodeCacheDB.ContainsKey(postcode) Then
                state = postCodeCacheDB(postcode)(2)
            End If
            Return state
        Catch ex As Exception
            Return ""
        End Try
    End Function
    Private Function GetCountryCodeFromPostCode(postcode As String) As String
        EnsureCodeDBLoaded()
        Try
            Dim country As String = ""
            If postCodeCacheDB.ContainsKey(postcode) Then
                country = postCodeCacheDB(postcode)(0)
            End If
            Return country
        Catch ex As Exception
            Return ""
        End Try
    End Function
    Private Function LatestDBVersion() As Integer
        Dim parameters As String = "key=" & apiKey & "&op=0"
        Dim RawResult As JObject = liminalAsObject("postalcodes", parameters)
        If RawResult("status") Then
            Return RawResult("data")
        Else
            Return -1
        End If
    End Function
    Public Sub EnsureCodeDBLoaded()
        If IsNothing(postCodeCacheDB) Then
            If Not IsFileExist(codeDatabase, False) Then
                downloadDatabase()
            Else
                Try
                    postCodeCacheDB = JObject.Parse(File.ReadAllText(codeDatabase))
                Catch ex As Exception
                    ' Post Code database was unable to download
                    Debug.Print(ex.ToString())
                End Try
            End If
        End If
    End Sub
    Public Function GetPostCodeData(postcode As String) As PostCodeResult
        Dim CurrentDBVersion As Integer = GetPolicyData(gShipriteDB, "PostCodeDBVer", -1)
        If CurrentDBVersion < 1 AndAlso InternetIsAvailable() Then
            ' DB Not initialized
            downloadDatabase()
            ' run query
            Return PostCodeData(postcode)
        Else
            If InternetIsAvailable() Then
                ' Check DB Version compared to remote version
                Dim RemoteVersion As Integer = LatestDBVersion()
                If RemoteVersion > 0 Then
                    If RemoteVersion > CurrentDBVersion Then
                        ' get latest DB
                        downloadDatabase()
                    End If
                    Return PostCodeData(postcode)
                Else
                    Debug.Print("Cannot retrieve latest DB version")
                    Return PostCodeData(postcode)
                End If
            Else
                Return PostCodeData(postcode)
            End If
        End If
    End Function
    Private Function PostCodeData(postcode As String) As PostCodeResult
        EnsureCodeDBLoaded()
        Try
            Dim data As PostCodeResult
            If postCodeCacheDB.ContainsKey(postcode) Then
                data = New PostCodeResult
                data.PostCode = postcode
                data.CountryCode = postCodeCacheDB(postcode)(0)
                data.State = postCodeCacheDB(postcode)(2)
                data.City = postCodeCacheDB(postcode)(1)
            End If
            Return data
        Catch ex As Exception
            Return Nothing
        End Try
    End Function
#End Region

#Region "Country DB"
    Public Function Get_CountryCodeFromCountryName(ByVal countryName As String, Optional ByVal isShowErrMsg As Boolean = True) As String
        If Not String.IsNullOrEmpty(countryName) Then
            Dim SQL = "SELECT [Country_Cd] FROM [Country] WHERE [Country_Name] = '" & countryName.Replace("'", "''") & "'"
            Return ExtractElementFromSegment("Country_Cd", IO_GetSegmentSet(gCountryDB, SQL))

        Else
            Return ""
        End If
    End Function
    Public Function Get_CountryNameFromCountryCode(ByVal countryCode As String) As String
        If Not String.IsNullOrEmpty(countryCode) Then
            Dim SQL = "SELECT [Country_Name] FROM [Country] WHERE [Country_Cd] = '" & countryCode & "'"
            Dim SegmentSet = IO_GetSegmentSet(gCountryDB, SQL)
            Dim output = ExtractElementFromSegment("Country_Name", SegmentSet)
            Return output
        Else
            Return ""
        End If
    End Function
    Public Function Get_StateCodeFromStateName(ByVal StateName As String, Optional ByVal isShowErrMsg As Boolean = True) As String
        If Not String.IsNullOrEmpty(StateName) Then
            Dim SQL As String = "SELECT [ProvState_Cd] FROM [StateProv] WHERE [ProvState_Name] = '" & StateName.Replace("'", "''") & "'"
            Dim SegmentSet = IO_GetSegmentSet(gCountryDB, SQL)
            Dim output = ExtractElementFromSegment("ProvState_Cd", SegmentSet)
            Return output
        Else
            Return ""
        End If
    End Function
    Public Function Get_StateNameFromStateCode(ByVal StateCode As String) As String
        If Not String.IsNullOrEmpty(StateCode) Then
            Dim SQL As String = "SELECT [ProvState_Name] FROM [StateProv] WHERE [ProvState_Cd] = '" & StateCode & "'"
            Dim SegmentSet = IO_GetSegmentSet(gCountryDB, SQL)
            Dim output = ExtractElementFromSegment("ProvState_Name", SegmentSet)
            Return output
        Else
            Return ""
        End If
    End Function
    Public Function Null2DefaultValue(ByVal fld As Object, Optional ByVal defaultValue As Object = "") As Object
        Null2DefaultValue = defaultValue '' assume.
        If Not IsDBNull(fld) Then
            Null2DefaultValue = fld
        End If
    End Function
#End Region

#Region "Extra Random Shit"
    Public Function InternetIsAvailable() As Boolean
        Try
            Using client = New WebClient()
                Using stream = client.OpenRead("http://google.com")
                    Return True
                End Using
            End Using
        Catch
            Return False
        End Try
    End Function
    Public Function IsValidJson(ByVal strInput As String) As Boolean
        If String.IsNullOrWhiteSpace(strInput) Then
            Return False
        End If
        strInput = strInput.Trim()
        If (strInput.StartsWith("{") AndAlso strInput.EndsWith("}")) OrElse (strInput.StartsWith("[") AndAlso strInput.EndsWith("]")) Then
            Try
                Dim obj = JToken.Parse(strInput)
                Return True
            Catch ex As Exception
                Debug.Print(ex.ToString())
                Return False
            End Try
        Else
            Return False
        End If
    End Function

End Module

Module StringExtensions
    <Extension()>
    Public Function InsertEveryNthChar(str As String, inserString As String, nthChar As Int32) As String
        If String.IsNullOrEmpty(str) Then Return str
        Dim builder As New StringBuilder(str)
        Dim startIndex = builder.Length - (builder.Length Mod nthChar)
        For i As Int32 = startIndex To nthChar Step -nthChar
            builder.Insert(i, inserString)
        Next i
        Return builder.ToString()
    End Function
End Module
#End Region

Public Class EmailTemplate
    Public Property Type As String
    Public Property Subject As String
    Public Property Content As String
End Class
Public Class LetterTemplate
    Public Property Type As String

    Public Property Content As String
End Class

Class PostCodeResult
    Public Property PostCode As String
    Public Property CountryCode As String
    Public Property State As String
    Public Property City As String

End Class
