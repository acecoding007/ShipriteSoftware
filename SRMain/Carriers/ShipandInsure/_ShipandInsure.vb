Imports Newtonsoft.Json.Linq
Imports Newtonsoft.Json
Imports RestSharp

Module _ShipandInsure

    Private Class ShipandInsure_Carrier

        Public Property Code As String
        Public Property ShortName As String
        Public Property LongName As String
        Public Property Shiprite As String

    End Class
    Private ShipandInsure_CarrierList As New List(Of ShipandInsure_Carrier)
    Public Property ShipandInsure_IsTest As Boolean = True ' default
    Private ReadOnly Property ShipandInsure_URL As String
        Get
            If ShipandInsure_IsTest Then
                Return "http://dev.shipandinsure.com"
            Else
                Return "https://www.shipandinsure.com"
            End If
        End Get
    End Property

    Public Function ShipandInsure_GetCarrierID(ServiceName As String) As String
        Try
            If ShipandInsure_CarrierList.Count = 0 Then
                Using fileReader As New System.IO.StreamReader(gDBpath & "\ShipandInsure_Carriers.txt", System.Text.Encoding.[Default])

                    Dim stringReader As String = fileReader.ReadLine()  ' Reads header line first
                    Do Until fileReader.EndOfStream
                        stringReader = fileReader.ReadLine()
                        Dim stringLine() As String = stringReader.Split(vbTab)
                        Dim carrierToAdd As New ShipandInsure_Carrier()
                        carrierToAdd.Code = stringLine(0)
                        carrierToAdd.ShortName = stringLine(1)
                        carrierToAdd.LongName = stringLine(2)
                        carrierToAdd.Shiprite = stringLine(3)
                        ShipandInsure_CarrierList.Add(carrierToAdd)
                    Loop
                    fileReader.Close()
                End Using
            End If

            Dim carrierCode As String = String.Empty
            Dim carrierCodeToFind As ShipandInsure_Carrier = ShipandInsure_CarrierList.Find(Function(x As ShipandInsure_Carrier) x.Shiprite = ServiceName)
            If carrierCodeToFind IsNot Nothing Then
                carrierCode = carrierCodeToFind.Code
            End If
            Return carrierCode

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Failed to Get ShipandInsure Carrier ID...")
            Return String.Empty
        End Try
    End Function

    Public Function ShipandInsure_GetCarrierRates(UserID As String, Password As String) As String

        Dim resStr As String = ""

        Net.ServicePointManager.SecurityProtocol = Net.SecurityProtocolType.Tls12

        Try

            Dim client As New RestClient(ShipandInsure_URL & "/saiwebapi/api/Carriers")
            Dim req As New RestRequest(Method.GET)
            req.AddHeader("username", "21382")
            req.AddHeader("password", "udMhk7qW")
            req.AddHeader("grant_type", "password")
            Dim res As RestResponse = client.Execute(req)
            resStr = res.Content

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Failed to Get Carrier Information Online Server...")
            Debug.Print(ex.ToString)
        End Try

        Return resStr

    End Function
    Private Function ShipandInsure_GetAccessToken(UserID As String, Password As String, ByRef Token As String) As Boolean

        Net.ServicePointManager.SecurityProtocol = Net.SecurityProtocolType.Tls12
        Token = String.Empty ' reset

        Try
            Dim client As New RestClient(ShipandInsure_URL & "/saiwebapi/Token")
            Dim req As New RestRequest(Method.POST)
            req.AddHeader("Content-Type", "application/x-www-form-urlencoded")
            req.AddParameter("grant_type", "password")
            req.AddParameter("username", UserID)
            req.AddParameter("password", Password)

            Dim res As RestResponse = client.Execute(req)
            Dim resStr As String = res.Content
            If Not String.IsNullOrEmpty(res.ErrorMessage) Then
                ' request error
                Throw New Exception(res.ErrorMessage)
            End If

            Dim resJson As JObject = Nothing
            If IsValidJson(resStr) Then
                resStr = resStr.Trim()
                resJson = JObject.Parse(resStr)
                resStr = resJson.ToString()
                If resJson.TryGetValue("access_token", Token) Then
                    ' access token found
                Else
                    ' failure - return error
                    Throw New Exception(resStr)
                End If
            ElseIf Not String.IsNullOrEmpty(resStr) Then
                ' non-json returned - unexpected
                Throw New Exception("Unexpected response." & vbCrLf & resStr)
            Else
                ' blank response
                Throw New Exception("Blank response.")
            End If
        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Failed to Get Access Token from ShipandInsure Online Server...", , False)
            Debug.Print(ex.ToString)
        End Try

        Return (Not Token = String.Empty)

    End Function

    Public Function ShipandInsure_SaveBulkUploadItem(UserID As String, UserName As String, Password As String, CustomerName As String, CustomerID As String, TrackingNumber As String, CarrierNumber As String, DecVal As String, FromZip As String, ToZip As String, Country As String) As String

        Net.ServicePointManager.SecurityProtocol = Net.SecurityProtocolType.Tls12
        Dim token As String = String.Empty
        Dim cost As String = "0"

        Try
            If ShipandInsure_GetAccessToken(UserID, Password, token) Then
                Dim reqJson As New JObject()
                reqJson("ID") = "-1"
                reqJson("CustomerNumber") = UserID
                reqJson("Name") = CustomerName
                reqJson("TrackingNumber") = TrackingNumber
                reqJson("ShipmentDate") = Format(Today, "MM/dd/yy")
                reqJson("ZipFrom") = FromZip
                reqJson("ZipTo") = ToZip
                reqJson("CarrierNumber") = CarrierNumber
                reqJson("Value") = DecVal
                reqJson("Country") = Country
                reqJson("Cost") = "0"
                reqJson("CompanyName") = UserName
                reqJson("RecipientID") = CustomerID
                Dim reqJsonStr As String = reqJson.ToString()

                Dim client As New RestClient(ShipandInsure_URL & "/saiwebapi/api/SaveBulkUploadItem")
                Dim req As New RestRequest(Method.POST)
                req.AddHeader("Authorization", "Bearer " & token)
                req.AddJsonBody(reqJsonStr)
                Dim res As RestResponse = client.Execute(req)
                Dim resStr As String = res.Content
                _Files.WriteFile_ByOneString(reqJsonStr & vbCrLf & resStr & vbCrLf & vbCrLf, gDBpath & "\ShipandInsure_SaveBulkUploadItem.log", True)
                If Not String.IsNullOrEmpty(res.ErrorMessage) Then
                    ' request error
                    Throw New Exception(res.ErrorMessage)
                End If

                Dim resJson As JObject = Nothing
                If IsValidJson(resStr) Then
                    resJson = JObject.Parse(resStr)
                    resStr = resJson.ToString()

                    If resJson.TryGetValue("Message", Nothing) Then
                        ' error info found ("Message" property)
                        Throw New Exception(resStr)
                    Else
                        Dim statusMessage As String = resJson("StatusMessage")
                        Dim processed As Boolean = resJson("Processed")
                        If Not processed Then
                            If Not String.IsNullOrEmpty(statusMessage) Then
                                _MsgBox.WarningMessage(statusMessage, "Status Message returned from ShipandInsure Online Server...")
                            End If
                            cost = "0.00"
                        Else
                            cost = resJson("Cost").ToString()
                        End If
                    End If
                End If
            End If
        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Failed to Save Bulk Upload Item from ShipandInsure Online Server...")
            Debug.Print(ex.ToString)
        End Try

        Return cost

    End Function

    Public Function ShipandInsure_DeleteBulkUploadItem(UserID As String, UserName As String, Password As String, CustomerName As String, CustomerID As String, TrackingNumber As String, CarrierNumber As String, DecVal As String, FromZip As String, ToZip As String, Country As String) As String

        Net.ServicePointManager.SecurityProtocol = Net.SecurityProtocolType.Tls12
        Dim token As String = String.Empty
        Dim cost As String = "0"

        Try
            If ShipandInsure_GetAccessToken(UserID, Password, token) Then
                Dim reqJson As New JObject()
                reqJson("ID") = "-1"
                reqJson("CustomerNumber") = UserID
                reqJson("Name") = CustomerName
                reqJson("TrackingNumber") = TrackingNumber
                reqJson("ShipmentDate") = Format(Today, "MM/dd/yy")
                reqJson("ZipFrom") = FromZip
                reqJson("ZipTo") = ToZip
                reqJson("CarrierNumber") = CarrierNumber
                reqJson("Value") = DecVal
                reqJson("Country") = Country
                reqJson("Cost") = "0"
                reqJson("CompanyName") = UserName
                reqJson("RecipientID") = CustomerID
                Dim reqJsonStr As String = reqJson.ToString()

                Dim client As New RestClient(ShipandInsure_URL & "/saiwebapi/api/DeleteBulkUploadItem")
                Dim req As New RestRequest(Method.POST)
                req.AddHeader("Authorization", "Bearer " & token)
                req.AddJsonBody(reqJsonStr)
                Dim res As RestResponse = client.Execute(req)
                Dim resStr As String = res.Content
                _Files.WriteFile_ByOneString(reqJsonStr & vbCrLf & resStr & vbCrLf & vbCrLf, gDBpath & "\ShipandInsure_DeleteBulkUploadItem.log", True)
                If Not String.IsNullOrEmpty(res.ErrorMessage) Then
                    ' request error
                    Throw New Exception(res.ErrorMessage)
                End If

                Dim resJson As JObject = Nothing
                If IsValidJson(resStr) Then
                    resJson = JObject.Parse(resStr)
                    resStr = resJson.ToString()

                    If resJson.TryGetValue("Message", Nothing) Then
                        ' error info found ("Message" property)
                        Throw New Exception(resStr)
                    Else
                        Dim statusMessage As String = resJson("StatusMessage")
                        Dim processed As Boolean = resJson("Processed")
                        If Not processed Then
                            If Not String.IsNullOrEmpty(statusMessage) Then
                                _MsgBox.WarningMessage(statusMessage, "Status Message returned from ShipandInsure Online Server...")
                            End If
                            cost = "0.00"
                        Else
                            cost = resJson("Cost").ToString()
                        End If
                    End If
                End If
            End If
        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Failed to Save Bulk Upload Item from ShipandInsure Online Server...")
            Debug.Print(ex.ToString)
        End Try

        Return cost

    End Function



    Public Function ShipandInsure_GetShipmentCost(UserID As String, UserName As String, Password As String, CustomerName As String, CustomerID As String, TrackingNumber As String, CarrierNumber As String, DecVal As String, FromZip As String, ToZip As String, Country As String) As String

        Net.ServicePointManager.SecurityProtocol = Net.SecurityProtocolType.Tls12
        Dim token As String = String.Empty
        Dim cost As String = "0"

        Try
            If ShipandInsure_GetAccessToken(UserID, Password, token) Then
                Dim reqJson As New JObject()
                reqJson("ID") = "-1"
                reqJson("CustomerNumber") = UserID
                reqJson("Name") = CustomerName
                reqJson("TrackingNumber") = TrackingNumber
                reqJson("ShipmentDate") = Format(Today, "MM/dd/yy")
                reqJson("ZipFrom") = FromZip
                reqJson("ZipTo") = ToZip
                reqJson("CarrierNumber") = CarrierNumber
                reqJson("Value") = DecVal
                reqJson("Country") = Country
                reqJson("Cost") = "0"
                reqJson("CompanyName") = UserName
                reqJson("RecipientID") = CustomerID
                Dim reqJsonStr As String = reqJson.ToString()

                Dim client As New RestClient(ShipandInsure_URL & "/saiwebapi/api/GetShipmentCost")
                Dim req As New RestRequest(Method.POST)
                req.AddHeader("Authorization", "Bearer " & token)
                req.AddJsonBody(reqJsonStr)
                Dim res As RestResponse = client.Execute(req)
                Dim resStr As String = res.Content
                _Files.WriteFile_ByOneString(reqJsonStr & vbCrLf & resStr & vbCrLf & vbCrLf, gDBpath & "\ShipandInsure_GetShipmentCost.log", True)
                If Not String.IsNullOrEmpty(res.ErrorMessage) Then
                    ' request error
                    Throw New Exception(res.ErrorMessage)
                End If

                Dim resJson As JObject = Nothing
                If IsValidJson(resStr) Then
                    resJson = JObject.Parse(resStr)
                    resStr = resJson.ToString()

                    If resJson.TryGetValue("Message", Nothing) Then
                        ' error info found ("Message" property)
                        Throw New Exception(resStr)
                    Else
                        Dim statusMessage As String = resJson("StatusMessage")
                        Dim processed As Boolean = resJson("Processed")
                        If Not String.IsNullOrEmpty(statusMessage) Then
                            _MsgBox.WarningMessage(statusMessage, "Status Message returned from ShipandInsure Online Server...")
                            cost = "0.00"
                        Else
                            cost = resJson("Cost")
                        End If
                    End If
                End If
            End If
        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Failed to Get Shipment Cost from ShipandInsure Online Server...")
            Debug.Print(ex.ToString)
        End Try

        Return cost

    End Function
    Public Function ShipandInsure_GetShipmentCosts(UserID As String, UserName As String, Password As String, CustomerName As String, CustomerID As String, TrackingNumber As String, CarrierNumber As String, DecVal As String, FromZip As String, ToZip As String, Country As String) As String

        Net.ServicePointManager.SecurityProtocol = Net.SecurityProtocolType.Tls12
        Dim token As String = String.Empty
        Dim cost As String = "0"

        Try
            If ShipandInsure_GetAccessToken(UserID, Password, token) Then
                Dim reqJson As New JObject()
                reqJson("ID") = "-1"
                reqJson("CustomerNumber") = UserID
                reqJson("Name") = CustomerName
                reqJson("TrackingNumber") = TrackingNumber
                reqJson("ShipmentDate") = Format(Today, "MM/dd/yy")
                reqJson("ZipFrom") = FromZip
                reqJson("ZipTo") = ToZip
                reqJson("CarrierNumber") = CarrierNumber
                reqJson("Value") = DecVal
                reqJson("Country") = Country
                reqJson("Cost") = "0"
                reqJson("CompanyName") = UserName
                reqJson("RecipientID") = CustomerID
                reqJson("intAll") = "-1"
                Dim reqJsonStr As String = reqJson.ToString()

                Dim client As New RestClient(ShipandInsure_URL & "/saiwebapi/api/GetShipmentCost")
                Dim req As New RestRequest(Method.POST)
                req.AddHeader("Authorization", "Bearer " & token)
                req.AddJsonBody(reqJsonStr)
                Dim res As RestResponse = client.Execute(req)
                Dim resStr As String = res.Content
                _Files.WriteFile_ByOneString(reqJsonStr & vbCrLf & resStr & vbCrLf & vbCrLf, gDBpath & "\ShipandInsure_GetShipmentCost.log", True)
                If Not String.IsNullOrEmpty(res.ErrorMessage) Then
                    ' request error
                    Throw New Exception(res.ErrorMessage)
                End If
                Dim resJson As JObject = Nothing

                If IsValidJson(resStr) Then

                    resJson = JObject.Parse(resStr)
                    resStr = resJson.ToString()

                    If resJson.TryGetValue("Message", Nothing) Then
                        ' error info found ("Message" property)
                        Throw New Exception(resStr)
                    Else
                        Dim statusMessage As String = resJson("StatusMessage")
                        Dim processed As Boolean = resJson("Processed")
                        If Not String.IsNullOrEmpty(statusMessage) Then
                            _MsgBox.WarningMessage(statusMessage, "Status Message returned from ShipandInsure Online Server...")
                            cost = "0.00"
                        Else
                            cost = resJson("Cost")
                        End If
                    End If
                End If
            End If
        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Failed to Get Shipment Cost from ShipandInsure Online Server...")
            Debug.Print(ex.ToString)
        End Try

        Return cost

    End Function

End Module
