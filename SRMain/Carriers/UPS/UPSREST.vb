Imports System.Threading
Imports System.Net
Imports RestSharp
Imports Newtonsoft.Json.Linq
Imports Newtonsoft.Json

Namespace UPS_Rest.Api

    Public Class UPSJsonResponse

        Public Function ToJsonString() As String
            Return JsonConvert.SerializeObject(Me)
        End Function
        Public Function ToJsonString(formatting As Formatting) As String
            Return JsonConvert.SerializeObject(Me, formatting)
        End Function

    End Class

    Namespace Exceptions

        Public Class UPSApiException
            Inherits Exception

            Public ReadOnly Property Detail As Errors.ErrorResponse
            Public ReadOnly Property DetailString As String
                Get
                    Dim dStr As String = String.Empty

                    If Detail IsNot Nothing AndAlso Detail.Response IsNot Nothing AndAlso Detail.Response.Errors IsNot Nothing AndAlso Detail.Response.Errors.Count > 0 Then
                        For Each errDetail As Errors.ErrorDetail In Detail.Response.Errors
                            If Not String.IsNullOrEmpty(dStr) Then
                                dStr &= Environment.NewLine
                            End If
                            dStr &= errDetail.Code & " : " & errDetail.Message
                        Next
                    End If

                    If Not String.IsNullOrWhiteSpace(dStr) Then
                        Return dStr
                    Else
                        Return Message
                    End If
                End Get
            End Property

            Public Sub New(errorResponse As Errors.ErrorResponse)

                MyBase.New(errorResponse.ToJsonString(Formatting.Indented))

                Me.Detail = errorResponse

            End Sub

            Public Sub New(errorResponseStr As String)

                MyBase.New(errorResponseStr)

                Try
                    Me.Detail = JsonConvert.DeserializeObject(Of Errors.ErrorResponse)(errorResponseStr)
                Catch ex As Exception
                    Me.Detail = Nothing
                End Try

            End Sub

        End Class

    End Namespace

    Namespace Configuration

        Public Class ApiConfig

            Private Const BaseTestingUrl As String = "https://wwwcie.ups.com"
            Private Const BaseProductionUrl As String = "https://onlinetools.ups.com"
            Private Const DefaultCallbackUrlTimeout As Integer = 60

            Public Property ClientId As String = "sYmHwIqvGxywW6XS1XiJMbDOt4caS5u77FtKXWFamyoq313I"
            Public Property ClientSecret As String = "8Zo36B0c2JnyZ4Z0ZQRnVjXV41tqEqsy2XehpKOJVWl8PqXdXnxS0dgsTlCOucka"
            Public Property CallbackUrl As String = "http://localhost:5000/callback/"
            Public Property CallbackUrlTimeout As Integer
            Public ReadOnly Property BaseUrl As String
                Get
                    If IsTest Then
                        Return BaseTestingUrl
                    Else
                        Return BaseProductionUrl
                    End If
                End Get
            End Property

            Public Property IsTest As Boolean
            Public Property ShipperAccountNumber ' "9Q7Z5J"

            Public Property LabelImageType As String '= "ELP"
            Public Property LabelStockSize_Height As String '= "6"
            Public Property LabelStockSize_Width As String '= "4"
            Public Property LabelFilePath As String

            Public Shared ReadOnly Property IsEnabled As Boolean
                Get
                    Dim result As Boolean
                    Boolean.TryParse(GetPolicyData(gShipriteDB, "UPSREST_Enabled", "False"), result)
                    Return result
                End Get
            End Property

            Public Sub New()
                Me.New(False, DefaultCallbackUrlTimeout)
            End Sub
            Public Sub New(isTest As Boolean)
                Me.New(isTest, DefaultCallbackUrlTimeout)
            End Sub
            Public Sub New(isTest As Boolean, callbackUrlTimeout As Integer)
                Me.IsTest = isTest
                Me.CallbackUrlTimeout = callbackUrlTimeout

                LabelImageType = ""
                LabelStockSize_Height = 6
                LabelStockSize_Width = 4
                LabelFilePath = String.Format("{0}\UPS\InOut", gDBpath)
            End Sub

        End Class

    End Namespace

    Namespace Errors

        Public Class ErrorDetail
            <JsonProperty(PropertyName:="code", NullValueHandling:=NullValueHandling.Ignore)>
            Public Property Code As String
            <JsonProperty(PropertyName:="message", NullValueHandling:=NullValueHandling.Ignore)>
            Public Property Message As String
        End Class

        Public Class ErrorResponseDetail
            <JsonProperty(PropertyName:="errors", NullValueHandling:=NullValueHandling.Ignore)>
            Public Property Errors As List(Of ErrorDetail)
        End Class

        Public Class ErrorResponse
            Inherits UPSJsonResponse

            <JsonProperty(PropertyName:="response", NullValueHandling:=NullValueHandling.Ignore)>
            Public Property Response As ErrorResponseDetail
        End Class

    End Namespace

    Namespace Authentication

        Namespace OAuth

            Public Enum AuthorizeClientResponseStatus
                None
                Success
                Failed
                Timeout
            End Enum

            Public Class AuthorizeClientResponse

                Public Property Status As AuthorizeClientResponseStatus
                Public Property ErrorException As Exception

                Public Property Code As String
                Public Sub New()
                    Code = ""
                    Status = AuthorizeClientResponseStatus.None
                End Sub

            End Class

            Public Class GenerateTokenResponse
                Inherits UPSJsonResponse

                <JsonProperty(PropertyName:="token_type", NullValueHandling:=NullValueHandling.Ignore, Order:=1)>
                Public Property TokenType As String
                <JsonProperty(PropertyName:="client_id", NullValueHandling:=NullValueHandling.Ignore, Order:=2)>
                Public Property ClientId As String
                <JsonProperty(PropertyName:="scope", NullValueHandling:=NullValueHandling.Ignore, Order:=3)>
                Public Property Scope As String
                <JsonProperty(PropertyName:="refresh_count", NullValueHandling:=NullValueHandling.Ignore, Order:=4)>
                Public Property RefreshCount As String

                <JsonProperty(PropertyName:="access_token", NullValueHandling:=NullValueHandling.Ignore, Order:=5)>
                Public Property AccessToken As String '' Token to be used in API requests.
                <JsonProperty(PropertyName:="issued_at", NullValueHandling:=NullValueHandling.Ignore, Order:=6)>
                Public Property AccessTokenIssuedAt As String '' Issue time of requested token in milliseconds.
                <JsonProperty(PropertyName:="expires_in", NullValueHandling:=NullValueHandling.Ignore, Order:=7)>
                Public Property AccessTokenExpiresIn As String = "14399" '' Expire time for requested token in seconds.
                <JsonProperty(PropertyName:="status", NullValueHandling:=NullValueHandling.Ignore, Order:=8)>
                Public Property AccessTokenStatus As String

                <JsonProperty(PropertyName:="refresh_token", NullValueHandling:=NullValueHandling.Ignore, Order:=9)>
                Public Property RefreshToken As String '' Refresh token to be used in refresh requests when obtaining new access token.
                <JsonProperty(PropertyName:="refresh_token_issued_at", NullValueHandling:=NullValueHandling.Ignore, Order:=10)>
                Public Property RefreshTokenIssuedAt As String '' Time that refresh token was issued in milliseconds.
                <JsonProperty(PropertyName:="refresh_token_expires_in", NullValueHandling:=NullValueHandling.Ignore, Order:=11)>
                Public Property RefreshTokenExpiresIn As String = "5183999" '' Expiration time for requested refresh token in seconds.
                <JsonProperty(PropertyName:="refresh_token_status", NullValueHandling:=NullValueHandling.Ignore, Order:=12)>
                Public Property RefreshTokenStatus As String

                <JsonIgnore>
                Public ReadOnly Property AccessTokenIssuedAtDate As Date
                    Get
                        If AccessTokenIssuedAt Is Nothing OrElse AccessTokenIssuedAt.Length = 0 Then
                            Return Date.MinValue
                        End If

                        Return New Date(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(AccessTokenIssuedAt)
                    End Get
                End Property
                Private _accessTokenExpiresValue As Long
                <JsonIgnore>
                Public Property AccessTokenExpiresValue As Long
                    Get
                        If AccessTokenIssuedAt Is Nothing OrElse AccessTokenExpiresIn Is Nothing OrElse AccessTokenIssuedAt.Length = 0 OrElse AccessTokenExpiresIn.Length = 0 Then
                            If _accessTokenExpiresValue > 0 Then
                                Return _accessTokenExpiresValue
                            End If
                            Return 0
                        End If

                        Return AccessTokenIssuedAt + (AccessTokenExpiresIn * 1000) ' milliseconds
                    End Get
                    Set(value As Long)
                        _accessTokenExpiresValue = value
                    End Set
                End Property
                <JsonIgnore>
                Public ReadOnly Property AccessTokenExpiresDate As Date
                    Get
                        If AccessTokenExpiresValue = 0 Then
                            Return Date.MinValue
                        End If

                        Return New Date(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(AccessTokenExpiresValue)
                    End Get
                End Property

                <JsonIgnore>
                Public ReadOnly Property RefreshTokenIssuedAtDate As Date
                    Get
                        If RefreshTokenIssuedAt Is Nothing OrElse RefreshTokenIssuedAt.Length = 0 Then
                            Return Date.MinValue
                        End If

                        Return New Date(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(RefreshTokenIssuedAt)
                    End Get
                End Property
                Private _refreshTokenExpiresValue As Long
                <JsonIgnore>
                Public Property RefreshTokenExpiresValue As Long
                    Get
                        If RefreshTokenIssuedAt Is Nothing OrElse RefreshTokenExpiresIn Is Nothing OrElse RefreshTokenIssuedAt.Length = 0 OrElse RefreshTokenExpiresIn.Length = 0 Then
                            If _refreshTokenExpiresValue > 0 Then
                                Return _refreshTokenExpiresValue
                            End If
                            Return 0
                        End If

                        Return RefreshTokenIssuedAt + (RefreshTokenExpiresIn * 1000) ' milliseconds
                    End Get
                    Set(value As Long)
                        _refreshTokenExpiresValue = value
                    End Set
                End Property
                <JsonIgnore>
                Public ReadOnly Property RefreshTokenExpiresDate As Date
                    Get
                        If RefreshTokenExpiresValue = 0 Then
                            Return Date.MinValue
                        End If

                        Return New Date(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(RefreshTokenExpiresValue)
                    End Get
                End Property

                <JsonIgnore>
                Public ReadOnly Property IsAccessTokenValid As Boolean
                    Get
                        Return Not String.IsNullOrWhiteSpace(AccessToken)
                    End Get
                End Property

                <JsonIgnore>
                Public ReadOnly Property IsAccessTokenExpired As Boolean
                    Get
                        If AccessTokenExpiresDate <= Date.UtcNow.AddHours(1) Then
                            Return True
                        Else
                            Return False
                        End If
                    End Get
                End Property

                <JsonIgnore>
                Public ReadOnly Property IsRefreshTokenExpired As Boolean
                    Get
                        If RefreshTokenExpiresDate <= Date.UtcNow.AddHours(1) Then
                            Return True
                        Else
                            Return False
                        End If
                    End Get
                End Property

                Public Sub New()
                    ' defaults
                    AccessTokenExpiresIn = "14399"
                    RefreshTokenExpiresIn = "5183999"
                End Sub

                Public Sub SetIssuedAtNow()
                    Dim currentEpochTimeSpan As TimeSpan = Date.UtcNow - New Date(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                    Dim currentEpochTimeValue As Long = Math.Round(currentEpochTimeSpan.TotalMilliseconds, 0)
                    AccessTokenIssuedAt = currentEpochTimeValue.ToString()
                    RefreshTokenIssuedAt = AccessTokenIssuedAt ' copy - usually the same
                End Sub
            End Class

            Public Class AuthorizeClientRequest
                Inherits RestRequest

                Private Const EndpointPath As String = "/security/v1/oauth/authorize"
                Private ReadOnly _responseType As String = "code"
                Private ReadOnly _baseUrl As String

                Public ReadOnly Property AuthorizationUrl() As String
                    Get
                        Dim authClient As New RestClient(_baseUrl)
                        Return authClient.BuildUri(Me).AbsoluteUri
                    End Get
                End Property

                Public Sub New(clientId As String, redirectUri As String, baseUrl As String)
                    MyBase.New(EndpointPath, Method.GET)

                    _baseUrl = baseUrl
                    Me.AddQueryParameter("client_id", clientId)
                    Me.AddQueryParameter("redirect_uri", redirectUri)
                    Me.AddQueryParameter("response_type", _responseType)
                End Sub
            End Class

            Public Class AuthorizeClientService

                Private Shared _listener As HttpListener
                Private Shared _listenerTokenSource As CancellationTokenSource

                Private Shared Sub Listener_Stop()
                    If _listener IsNot Nothing Then
                        If _listener.IsListening Then
                            _listener.Stop()
                        End If
                    End If
                End Sub
                Private Shared Sub Listener_Close()
                    If _listener IsNot Nothing Then
                        _listener.Close()
                        _listener = Nothing
                    End If
                End Sub

                Public Shared Sub CancelAuthorizationRequest()
                    If _listenerTokenSource IsNot Nothing AndAlso Not _listenerTokenSource.IsCancellationRequested Then
                        _listenerTokenSource.Cancel()
                    End If
                End Sub

                Private Shared Async Function Listener_GetContextAsync(taskTimeout As Integer) As Task(Of HttpListenerContext)

                    _listenerTokenSource = New CancellationTokenSource() ' CancellationTokenSource.CreateLinkedTokenSource(timerToken) 'New CancellationTokenSource()
                    Dim token As CancellationToken = _listenerTokenSource.Token
                    token.Register(Sub() Listener_Stop())

                    Dim timerTokenSource As CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token)
                    Dim timerToken As CancellationToken = timerTokenSource.Token
                    timerToken.Register(Sub() Listener_Stop())
                    timerTokenSource.CancelAfter(TimeSpan.FromSeconds(taskTimeout))

                    Try
                        Dim listenerContext As HttpListenerContext = Await Task.Run(
                        Async Function()
                            Return Await _listener.GetContextAsync()
                        End Function, timerToken)

                        Return listenerContext

                    Catch ex As Exception
                        If token.IsCancellationRequested Then
                            ' canceled
                            Debug.WriteLine("Canceled...")
                        ElseIf timerToken.IsCancellationRequested Then
                            ' timeout
                            Throw New TimeoutException("The operation has timed out")
                        Else
                            ' error
                            Throw
                        End If
                    Finally
                        ' no longer needed
                        If timerTokenSource IsNot Nothing Then
                            timerTokenSource.Dispose()
                            timerTokenSource = Nothing
                        End If
                        If _listenerTokenSource IsNot Nothing Then
                            _listenerTokenSource.Dispose()
                            _listenerTokenSource = Nothing
                        End If
                    End Try

                    Return Nothing
                End Function

                Public Shared Async Function ProcessAuthorizeAsync(config As Configuration.ApiConfig) As Task(Of AuthorizeClientResponse)

                    Dim authResponse As New AuthorizeClientResponse()

                    If _listener IsNot Nothing AndAlso _listener.IsListening Then
                        Return Nothing
                    End If

                    Try

                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12

                        Dim authClientRequest As New AuthorizeClientRequest(config.ClientId, config.CallbackUrl, config.BaseUrl)

                        ' Authorize Client request
                        ' open auth url in default browser for user to login
                        Process.Start(authClientRequest.AuthorizationUrl)

                        ' start listening on callback url
                        _listener = New HttpListener
                        _listener.Prefixes.Add(config.CallbackUrl)
                        _listener.Start()

                        ' wait for response callback on port 5000 while user logs into their UPS account in the default browser
                        Dim context As HttpListenerContext = Nothing

                        Try
                            context = Await Listener_GetContextAsync(config.CallbackUrlTimeout)
                        Catch ex As TimeoutException
                            ' timeout
                            authResponse = New AuthorizeClientResponse() With {.Status = AuthorizeClientResponseStatus.Timeout}
                        End Try

                        ' callback received with 'code' to be used for Generating Token
                        If context IsNot Nothing Then
                            If context.Request IsNot Nothing Then
                                Dim request As HttpListenerRequest = context.Request

                                authResponse = New AuthorizeClientResponse() With {
                                    .Status = AuthorizeClientResponseStatus.Success,
                                    .Code = request.QueryString.Get("code")
                                }
                            End If
                            If context.Response IsNot Nothing Then
                                If authResponse IsNot Nothing AndAlso authResponse.Status = AuthorizeClientResponseStatus.Success Then
                                    context.Response.Redirect("https://shipritenext.com/ups/oauth/allowed/")
                                Else
                                    context.Response.Redirect("https://shipritenext.com/ups/oauth/denied/")
                                End If
                                context.Response.Close()
                                Thread.Sleep(200) ' pause to allow redirect to start/complete before closing httplistener
                            End If
                        End If

                    Catch ex As Exception
                        authResponse = New AuthorizeClientResponse With {
                            .Status = AuthorizeClientResponseStatus.Failed,
                            .ErrorException = ex
                        }
                    Finally
                        Listener_Close()
                    End Try

                    Return authResponse

                End Function

            End Class

            Public Class GenerateTokenRequest
                Inherits RestRequest

                Private Const EndpointPath As String = "/security/v1/oauth/token"

                Public Sub New(code As String, redirectUri As String)
                    MyBase.New(EndpointPath, Method.POST)
                    Me.AddHeader("Content-Type", "application/x-www-form-urlencoded")
                    Me.AddParameter("grant_type", "authorization_code")
                    Me.AddParameter("code", code)
                    Me.AddParameter("redirect_uri", redirectUri)
                End Sub

            End Class

            Public Class GenerateTokenClient
                Inherits RestClient

                Public Sub New(baseUrl As String, clientId As String, clientSecret As String)
                    MyBase.New(baseUrl)
                    Me.Authenticator = New Authenticators.HttpBasicAuthenticator(clientId, clientSecret)
                End Sub

                Public Shadows Function Execute(genTokenRequest As GenerateTokenRequest) As GenerateTokenResponse
                    Dim response As IRestResponse = MyBase.Execute(genTokenRequest)

                    If String.IsNullOrWhiteSpace(response.Content) Then
                        Throw New Exception("Invalid Empty response returned from server.")
                    End If

                    If response.StatusCode = HttpStatusCode.OK Then
                        Return JsonConvert.DeserializeObject(Of GenerateTokenResponse)(response.Content)
                    Else
                        Throw New Exceptions.UPSApiException(response.Content)
                    End If
                End Function
            End Class

            Public Class GenerateTokenService

                Private Const MsgBoxTitle As String = "UPS REST API Authentication"

                Public Shared Function ProcessGenerateToken(config As Configuration.ApiConfig, code As String) As GenerateTokenResponse

                    Dim tokenResponse As New GenerateTokenResponse()
                    Dim saveFilePath As String = config.LabelFilePath & "\authtoken_response.json"

                    Try
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12

                        If String.IsNullOrEmpty(code) Then
                            Throw New Exception("Invalid empty authorization code provided.")
                        End If

                        Dim genTokenRequest As New GenerateTokenRequest(code, config.CallbackUrl)
                        Dim genTokenClient As New GenerateTokenClient(config.BaseUrl, config.ClientId, config.ClientSecret)
                        tokenResponse = genTokenClient.Execute(genTokenRequest)

                        _Files.WriteFile_ToEnd(tokenResponse.ToJsonString(), saveFilePath)

                        If Not tokenResponse.IsAccessTokenValid Then
                            Throw New Exception("Invalid Access Token Returned." & Environment.NewLine & tokenResponse.ToJsonString(Formatting.Indented))
                        End If

                    Catch ex As Exceptions.UPSApiException
                        Dim errorSaveFile As String = ex.Message
                        Dim errorMsgBox As String = ex.Message

                        If ex.Detail IsNot Nothing Then
                            errorSaveFile = ex.Detail.ToJsonString(Formatting.Indented)
                            errorMsgBox = ex.DetailString
                        End If

                        _Files.WriteFile_ToEnd(errorSaveFile, saveFilePath)
                        _MsgBox.ErrorMessage(errorMsgBox, "Failed to Generate Token from UPS REST API Server...", MsgBoxTitle)
                    Catch ex As Exception
                        _MsgBox.ErrorMessage(ex, "Failed to Generate Token from UPS REST API Server...", MsgBoxTitle)
                    End Try
                    '
                    Return tokenResponse
                    '
                End Function

            End Class

            Public Class RefreshTokenRequest
                Inherits RestRequest

                Private Const EndpointPath As String = "/security/v1/oauth/refresh"

                Public Sub New(rToken As String)
                    MyBase.New(EndpointPath, Method.POST)
                    Me.AddHeader("Content-Type", "application/x-www-form-urlencoded")
                    Me.AddParameter("grant_type", "refresh_token")
                    Me.AddParameter("refresh_token", rToken)
                End Sub
            End Class

            Public Class RefreshTokenClient
                Inherits RestClient

                Public Sub New(baseUrl As String, clientId As String, clientSecret As String)
                    MyBase.New(baseUrl)
                    Me.Authenticator = New Authenticators.HttpBasicAuthenticator(clientId, clientSecret)
                End Sub

                Public Shadows Function Execute(refTokenRequest As RefreshTokenRequest) As GenerateTokenResponse
                    Dim response As IRestResponse = MyBase.Execute(refTokenRequest)

                    If String.IsNullOrWhiteSpace(response.Content) Then
                        Throw New Exception("Invalid Empty response returned from server.")
                    End If

                    If response.StatusCode = HttpStatusCode.OK Then
                        Return JsonConvert.DeserializeObject(Of GenerateTokenResponse)(response.Content)
                    Else
                        Throw New Exceptions.UPSApiException(response.Content)
                    End If
                End Function

            End Class

            Public Class RefreshTokenService

                Private Const MsgBoxTitle As String = "UPS REST API Authentication"

                Public Shared Function ProcessRefreshToken(config As Configuration.ApiConfig, rToken As String) As GenerateTokenResponse

                    Dim tokenResponse As New GenerateTokenResponse()
                    Dim saveFilePath As String = config.LabelFilePath & "\authtoken_response.json"

                    Try
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12

                        If String.IsNullOrEmpty(rToken) Then
                            Throw New Exception("Invalid empty refresh token provided.")
                        End If

                        Dim refTokenRequest As New RefreshTokenRequest(rToken)
                        Dim refTokenClient As New RefreshTokenClient(config.BaseUrl, config.ClientId, config.ClientSecret)
                        tokenResponse = refTokenClient.Execute(refTokenRequest)

                        _Files.WriteFile_ToEnd(tokenResponse.ToJsonString(), saveFilePath)

                        If Not tokenResponse.IsAccessTokenValid Then
                            Throw New Exception("Invalid Access Token Returned." & Environment.NewLine & tokenResponse.ToJsonString(Formatting.Indented))
                        End If

                    Catch ex As Exceptions.UPSApiException
                        Dim errorSaveFile As String = ex.Message
                        Dim errorMsgBox As String = ex.Message

                        If ex.Detail IsNot Nothing Then
                            errorSaveFile = ex.Detail.ToJsonString(Formatting.Indented)
                            errorMsgBox = ex.DetailString
                        End If

                        _Files.WriteFile_ToEnd(errorSaveFile, saveFilePath)
                        _MsgBox.ErrorMessage(errorMsgBox, "Failed to Refresh Token from UPS REST API Server...", MsgBoxTitle)
                    Catch ex As Exception
                        _MsgBox.ErrorMessage(ex, "Failed to Refresh Token from UPS REST API Server...", MsgBoxTitle)
                    End Try
                    '
                    Return tokenResponse
                    '
                End Function

            End Class

        End Namespace

        Public Class AuthenticationService

            Private Const MsgBoxTitle As String = "UPS REST API Authentication"
            Private Const AuthTokenResponseDbField As String = "UPSREST_OAuthTokenResponse"

            Public Shared ReadOnly Property IsEnabled As Boolean
                Get
                    ' enabled by default - UPS deprecating Access Key authentication effective 10/07/2024
                    Return True
                End Get
            End Property

            Public Shared Sub CancelAuthorizationRequest()
                OAuth.AuthorizeClientService.CancelAuthorizationRequest()
            End Sub

            Public Shared Async Function RequestAuthorizationAsync() As Task(Of Boolean)
                Dim config As New Configuration.ApiConfig()

                Dim authResponse As OAuth.AuthorizeClientResponse = Await OAuth.AuthorizeClientService.ProcessAuthorizeAsync(config)

                If authResponse.Status = OAuth.AuthorizeClientResponseStatus.Timeout Then
                    _MsgBox.InformationMessage("Request to Authorize Client timed out...", , MsgBoxTitle)
                ElseIf authResponse.Status = OAuth.AuthorizeClientResponseStatus.Failed Then
                    _MsgBox.ErrorMessage(authResponse.ErrorException, "Failed to Authorize Client from UPS REST API Server...", MsgBoxTitle)
                ElseIf authResponse.Status = OAuth.AuthorizeClientResponseStatus.Success Then
                    Dim tokenResponse As OAuth.GenerateTokenResponse = OAuth.GenerateTokenService.ProcessGenerateToken(config, authResponse.Code)
                    If tokenResponse.IsAccessTokenValid Then
                        Return UpdateTokenInDatabase(tokenResponse)
                    Else
                        _MsgBox.ErrorMessage("Invalid Access Token Returned.", "Failed to Authorize Client from UPS REST API Server...", MsgBoxTitle)
                    End If
                End If

                Return False
            End Function

            Public Shared Function IsAccessTokenSaved() As Boolean

                Dim currentTokenResponse As OAuth.GenerateTokenResponse = LoadTokenFromDatabase()

                Return currentTokenResponse IsNot Nothing AndAlso currentTokenResponse.IsAccessTokenValid

            End Function

            Public Shared Function IsGetAccessToken(ByRef authToken As OAuth.GenerateTokenResponse) As Boolean
                authToken = GetAccessToken()
                Return authToken IsNot Nothing
            End Function

            Public Shared Function GetAccessToken() As OAuth.GenerateTokenResponse
                Dim config As New Configuration.ApiConfig()
                Dim currentTokenResponse As OAuth.GenerateTokenResponse = LoadTokenFromDatabase()
                '
                If currentTokenResponse IsNot Nothing AndAlso currentTokenResponse.IsAccessTokenValid Then
                    ' valid
                    If currentTokenResponse.IsAccessTokenExpired Then
                        ' access token expired
                        If Not currentTokenResponse.IsRefreshTokenExpired Then
                            ' refresh not expired - use to get new access token
                            Dim newTokenResponse As OAuth.GenerateTokenResponse = OAuth.RefreshTokenService.ProcessRefreshToken(config, currentTokenResponse.RefreshToken)
                            If newTokenResponse.IsAccessTokenValid Then
                                UpdateTokenInDatabase(newTokenResponse)
                                Return newTokenResponse
                            End If
                        End If
                    Else
                        ' access token not expired - good to go
                        Return currentTokenResponse
                    End If
                End If
                ' both tokens expired or no access token found
                ' need to login to Carrier Setup
                _MsgBox.WarningMessage("Please navigate to UPS Carrier Setup to Authorize your UPS account for use in this application.", "Failed to Get UPS REST API Access Token...", MsgBoxTitle)
                Return Nothing
                '
            End Function

            Public Shared Function ClearTokenInDatabase() As Boolean
                Dim ret As Integer = 0
                '
                Try
                    ret = UpdatePolicy(gShipriteDB, AuthTokenResponseDbField, "")
                Catch ex As Exception
                    _MsgBox.ErrorMessage(ex, "Failed to Clear Authentication Info from Database...", MsgBoxTitle)
                End Try
                '
                Return ret = 1
                '
            End Function

            Private Shared Function UpdateTokenInDatabase(tokenResponse As OAuth.GenerateTokenResponse) As Boolean
                Dim ret As Integer = 0
                '
                Try
                    Dim json As String = tokenResponse.ToJsonString()
                    ret = UpdatePolicy(gShipriteDB, AuthTokenResponseDbField, json)
                Catch ex As Exception
                    _MsgBox.ErrorMessage(ex, "Failed to Update Authentication Info in Database...", MsgBoxTitle)
                End Try
                '
                Return ret = 1
                '
            End Function

            Private Shared Function LoadTokenFromDatabase() As OAuth.GenerateTokenResponse
                Dim tokenResponse As New OAuth.GenerateTokenResponse
                '
                Try
                    Dim json As String = GetPolicyData(gShipriteDB, AuthTokenResponseDbField)
                    If Not String.IsNullOrWhiteSpace(json) Then
                        tokenResponse = JsonConvert.DeserializeObject(Of OAuth.GenerateTokenResponse)(json)
                    End If
                Catch ex As Exception
                    _MsgBox.ErrorMessage(ex, "Failed to Load Authentication Info from Database...", MsgBoxTitle)
                End Try
                '
                Return tokenResponse
                '
            End Function

        End Class

    End Namespace

End Namespace


Module UPSREST

    Public Function UPS_CommercialInvoiceUpload(FileBuffer As String, PackageID As String) As Integer

        Dim accessToken As String = GetPolicyData(gShipriteDB, "UPSOauthToken")
        If accessToken = "" Then

            Dim ans As Integer = MsgBox("ATTENTION...UPS Requires a Periodic Login Authentication " & vbCrLf & "before communicating with their server.  This requires" & vbCrLf & "YOUR UPS UserID and Password." & vbCrLf & vbCrLf & "CONNECT NOW FOR UPS AUTHENTICATION???", vbQuestion + vbYesNo, "SHIPRITE NEXT UPS INTEGRATION")
            If ans = vbNo Then

                Return 1
                Exit Function

            Else

                Dim ret As Integer = UPS_GetAccessToken()
                If ret = 1 Then

                    Return 1
                    Exit Function

                End If

            End If

        End If
        Dim accessTokenExpires As Date = GetPolicyData(gShipriteDB, "UPSOauthTokenExpires")
        Dim refreshToken As String = GetPolicyData(gShipriteDB, "UPSOauthRefreshToken")
        Dim refreshTokenExpires As Date = GetPolicyData(gShipriteDB, "UPSOauthRefreshTokenExpires")
        Dim clientId As String = "sYmHwIqvGxywW6XS1XiJMbDOt4caS5u77FtKXWFamyoq313I"
        Dim clientSecret As String = "8Zo36B0c2JnyZ4Z0ZQRnVjXV41tqEqsy2XehpKOJVWl8PqXdXnxS0dgsTlCOucka"
        Dim ManifestSegment As String = IO_GetSegmentSet(gShipriteDB, "SELECT * FROM Manifest WHERE PackageID = '" & PackageID & "'")
        Dim shipperAcctNum As String = ExtractElementFromSegment("SHIPPER#", ManifestSegment)

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
        Dim fileName As String = ExtractElementFromSegment("TRACKING#", ManifestSegment) & ".rtf"
        Dim fileFormat As String = "rtf"
        Dim encFileBuffer As String = _Convert.StringToBase64(FileBuffer)
        Dim formType As String = "002"
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
                                UserCreatedFormFile: '" & encFileBuffer & "',
                                TrackingNumber: '" & ExtractElementFromSegment("TRACKINGING#", ManifestSegment) & "'
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

        Dim response As IRestResponse = client.Execute(request)

        If response.StatusCode = HttpStatusCode.OK Then

            Dim result As JObject = JObject.Parse(response.Content)
            Return 0

        Else

            Dim jsonResp As JObject = JObject.Parse(response.Content)
            Dim jsonRespStr = jsonResp.ToString
            WriteFile_ToEnd(jsonRespStr, "c:\shipritenext\JSON_Payload-Response.txt")
            MsgBox(jsonRespStr)
            Return 1

        End If

    End Function
    Public Function UPS_GetAccessToken() As Integer

        Dim buf As String
        buf = GetPolicyData(gShipriteDB, "UPSOauthToken")
        If Not buf = "" Then

            Dim ExpirationDate As Date = GetPolicyData(gShipriteDB, "")

        End If

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

            Return 1
            Exit Function

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
        Return 0

    End Function

    Public Function CreateUPSCommercialInvoiceFile(PackageID As String) As String

        Dim ManifestSegment As String = IO_GetSegmentSet(gShipriteDB, "SELECT * FROM Manifest WHERE PackageID = '" & PackageID & "'")
        Dim ItemsSegment As String = IO_GetSegmentSet(gShipriteDB, "SELECT * FROM CustomsItems WHERE PackageID = '" & PackageID & "'")
        Dim ConsigneeSegment As String = IO_GetSegmentSet(gShipriteDB, "SELECT * FROM Contacts WHERE ID = " & ExtractElementFromSegment("CID", ManifestSegment))
        Dim ShipperSegment As String = IO_GetSegmentSet(gShipriteDB, "SELECT * FROM Contacts WHERE ID = " & ExtractElementFromSegment("SID", ManifestSegment))
        Dim i As Integer = 0
        Dim ItemsCT As Integer
        Dim ItemsTot As Double
        Dim ItemsLBS As Double
        Dim Segment As String = ""
        Dim SegmentSet As String = IO_GetSegmentSet(gShipriteDB, "SELECT COUNT(*) AS Tally, SUM(ItemValue) AS TotalInvoice, SUM(Weight) AS TotalWeight FROM CustomsItems WHERE PackageID = '" & PackageID & "'")
        ItemsCT = Val(ExtractElementFromSegment("Tally", SegmentSet))
        ItemsTot = Val(ExtractElementFromSegment("TotalInvoice", SegmentSet))
        ItemsLBS = Val(ExtractElementFromSegment("TotalWeight", SegmentSet))

        ' Read Commercial Invoice File

        Dim FileBuffer As String = IO.File.ReadAllText("c:\shipritenext\COMMERCIAL INVOICE TEMPLATE.rtf")

        Dim buf = GetPolicyData(gShipriteDB, "Name")

        buf = String.Format("{0,-35}", buf)
        FileBuffer = FlushOut(FileBuffer, "DDDDDDDDDDDDDDD", Format(Today, "dd-MMM-yyyy"))
        FileBuffer = FlushOut(FileBuffer, "SHIPPERNAME1.....................35", String.Format("{0,-35}", ExtractElementFromSegment("Name", ShipperSegment)))
        FileBuffer = FlushOut(FileBuffer, "SHIPPERNAME2.....................35", String.Format("{0,-35}", ExtractElementFromSegment("LName", ShipperSegment) & ", " & ExtractElementFromSegment("FName", ShipperSegment)))
        FileBuffer = FlushOut(FileBuffer, "SHIPPERADDRESS1..................35", String.Format("{0,-35}", ExtractElementFromSegment("Addr1", ShipperSegment)))
        FileBuffer = FlushOut(FileBuffer, "SHIPPERADDRESS2..................35", String.Format("{0,-35}", ExtractElementFromSegment("Addr2", ShipperSegment)))
        FileBuffer = FlushOut(FileBuffer, "SHIPPERCSZ.......................35", String.Format("{0,-35}", ExtractElementFromSegment("City", ShipperSegment) & ", " & ExtractElementFromSegment("State", ShipperSegment) & "   " & ExtractElementFromSegment("Zip", ShipperSegment)))
        FileBuffer = FlushOut(FileBuffer, "SHIPPERCOUNTRY...................35", String.Format("{0,-35}", ExtractElementFromSegment("Country", ShipperSegment)))
        FileBuffer = FlushOut(FileBuffer, "SHIPPERPHONE.....................35", String.Format("{0,-35}", ExtractElementFromSegment("Phone", ShipperSegment)))

        FileBuffer = FlushOut(FileBuffer, "CONSIGNEENAME1...................35", String.Format("{0,-35}", ExtractElementFromSegment("Name", ConsigneeSegment)))
        FileBuffer = FlushOut(FileBuffer, "CONSIGNEENAME2...................35", String.Format("{0,-35}", ExtractElementFromSegment("LName", ConsigneeSegment) & ", " & ExtractElementFromSegment("FName", ConsigneeSegment)))
        FileBuffer = FlushOut(FileBuffer, "CONSIGNEEADDRESS1................35", String.Format("{0,-35}", ExtractElementFromSegment("Addr1", ConsigneeSegment)))
        FileBuffer = FlushOut(FileBuffer, "CONSIGNEEADDRESS2................35", String.Format("{0,-35}", ExtractElementFromSegment("Addr2", ConsigneeSegment)))
        FileBuffer = FlushOut(FileBuffer, "CONSIGNEECSZ.....................35", String.Format("{0,-35}", ExtractElementFromSegment("City", ConsigneeSegment) & ", " & ExtractElementFromSegment("State", ConsigneeSegment) & "   " & ExtractElementFromSegment("Zip", ConsigneeSegment)))
        FileBuffer = FlushOut(FileBuffer, "CONSIGNEECOUNTRY.................35", String.Format("{0,-35}", ExtractElementFromSegment("Country", ConsigneeSegment)))
        FileBuffer = FlushOut(FileBuffer, "CONSIGNEECOUNTRY....22", String.Format("{0,-35}", ExtractElementFromSegment("Country", ConsigneeSegment)))
        FileBuffer = FlushOut(FileBuffer, "CONSIGNEEPHONE...................35", String.Format("{0,-35}", ExtractElementFromSegment("Phone", ConsigneeSegment)))
        FileBuffer = FlushOut(FileBuffer, "TRACKINGNUMBER...................35", String.Format("{0,-35}", ExtractElementFromSegment("TRACKING#", ManifestSegment)))
        FileBuffer = FlushOut(FileBuffer, "SHIPREF.10", String.Format("{0,-11}", ExtractElementFromSegment("SHIPPER#", ManifestSegment)))
        FileBuffer = FlushOut(FileBuffer, "TTTT5", String.Format("{0,-5}", "1"))
        FileBuffer = FlushOut(FileBuffer, "WWWWWWWW10", String.Format("{0,-10}", ItemsLBS.ToString))
        FileBuffer = FlushOut(FileBuffer, "CONTENTS..........20", String.Format("{0,-20}", "Gift"))
        FileBuffer = FlushOut(FileBuffer, "TTTTTTTTTTTTT15", String.Format("{0,-20}", Format(ItemsTot, "$ 0.00")))

        For i = 1 To 12

            Segment = GetNextSegmentFromSet(ItemsSegment)
            FileBuffer = FlushOut(FileBuffer, "QQ" & Format(i, "00"), String.Format("{0,-4}", ExtractElementFromSegment("Quantity", Segment)))
            FileBuffer = FlushOut(FileBuffer, "DESCRIPTION...." & Format(i, "00"), String.Format("{0,-17}", ExtractElementFromSegment("Description", Segment)))
            FileBuffer = FlushOut(FileBuffer, "WWWW" & Format(i, "00"), String.Format("{0,-6}", ExtractElementFromSegment("Weight", Segment)))
            FileBuffer = FlushOut(FileBuffer, "VVVVVV" & Format(i, "00"), String.Format("{0,8}", Format(Val(ExtractElementFromSegment("ItemValue", Segment)), "0.00")))
            FileBuffer = FlushOut(FileBuffer, "CCCCCCCCCCCCCCCCCC" & Format(i, "00"), String.Format("{0,-20}", ExtractElementFromSegment("OriginCountry", Segment)))
            FileBuffer = FlushOut(FileBuffer, "HHHHHHHH" & Format(i, "00"), String.Format("{0,-10}", ExtractElementFromSegment("HarmonizedCode", Segment)))

            If i = ItemsCT Then

                Exit For

            End If

        Next
        For i = i + 1 To ItemsTot

            FileBuffer = FlushOut(FileBuffer, "QQ" & Format(i, "00"), String.Format("{0,-4}", ""))
            FileBuffer = FlushOut(FileBuffer, "DESCRIPTION...." & Format(i, "00"), String.Format("{0,-17}", ""))
            FileBuffer = FlushOut(FileBuffer, "WWWW" & Format(i, "00"), String.Format("{0,-6}", ""))
            FileBuffer = FlushOut(FileBuffer, "VVVVVV" & Format(i, "00"), String.Format("{0,8}", ""))
            FileBuffer = FlushOut(FileBuffer, "CCCCCCCCCCCCCCCCCC" & Format(i, "00"), String.Format("{0,-20}", ""))
            FileBuffer = FlushOut(FileBuffer, "HHHHHHHH" & Format(i, "00"), String.Format("{0,-10}", ""))

        Next
        ' Write Commercial Invoice File

        buf = Dir("C:\shipritenext\UPSCommercialInvoice", FileAttribute.Directory)
        If buf = "" Then

            MkDir("C:\shipritenext\UPSCommercialInvoice")

        End If
        buf = "C:\shipritenext\UPSCommercialInvoice" & "\" & ExtractElementFromSegment("TRACKING#", ManifestSegment) & ".rtf"
        WriteFile_ToEnd(FileBuffer, buf)
        Return FileBuffer

    End Function

End Module
