Imports System.Net
Imports System.Reflection
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports Newtonsoft.Json.Serialization
Imports RestSharp

Module FedExRETURNS

    Public Class FedExRETURNS_SETUP
        Property OAuthToken As String = ""  'ShipRite Token
        Property OAuthExpires As DateTime

        Property URL As String = " https://api.test.office.fedex.com" 'testing url
        'Property URL As String = " https://api.office.fedex.com"

        Property ClientID As String = "l70ec239d113c644bc9b79904b0a0b52dd" 'ShipRite Credentials
        Property ClientSecret As String = "f524583e177847dea4fecfc2ffc7f327" 'ShipRite Credentials

        Property LocationID As String = GetPolicyData(gShipriteDB, "FedExRETURN_LocationID", "")
        Property Path_Save_InOut_File As String = String.Format("{0}\FedEx\InOut\Returns", gDBpath)

    End Class

    Public Class FXReturns_GetFormattedLabelRequest
        Property rmaId As String
        Property clientId As String
        Property originAppId As String = "SHPRTE"
        Property locId As String = gFedExReturnsSETUP.LocationID
        Property labelFormatType As String = "COMMON2D"
        Property featureLabelTypeNeeded As New List(Of String)
        Property imageType As String
        Property labelStockType As String
        Property labelPrintingOrientation As String
        Property labelRotation As String

    End Class

    Public Class FXReturns_GetFormattedLabelResponse
        Property requestIdentifier As String
        Property transactionDate As String
        Property highestSeverity As String
        Property notifications As List(Of FXReturns_NotificationData)
        Property labelFormatType As String
        Property imageType As String
        Property labelStockType As String
        Property labelPrintingOrientation As String
        Property labelRotation As String
        Property featureLabelType As String
        Property label As List(Of FXReturns_FeatureLabelData)
    End Class

    Public Class FXReturns_NotificationData
        Property severity As String
        Property source As String
        Property code As String
        Property message As String
    End Class

    Public Class FXReturns_FeatureLabelData
        Property trackingNumber As String
        Property trackingBarcode As String
        Property labelContent As String
        Property packingSlipContent As String
    End Class

    Public Function FXReturns_SendRequest(RMA_No As String, RequirePackingSlip As Boolean, imageType As Integer, ByRef trackingNo As String)
        If Not FXReturns_Get_OAuth_Token() Then
            Return False
        End If

        System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12
        Dim client = New RestClient(gFedExReturnsSETUP.URL & "/returns/v1/label/formatted")
        Dim request = New RestRequest(Method.POST)

        request.AddHeader("Authorization", "Bearer " & gFedExReturnsSETUP.OAuthToken)
        request.AddHeader("User-Agent", "insomnia/8.4.2")
        request.AddHeader("Content-Type", "application/json")

        Dim GetFormattedLabelRequest As New FXReturns_GetFormattedLabelRequest

        GetFormattedLabelRequest.rmaId = RMA_No
        GetFormattedLabelRequest.clientId = gFedExReturnsSETUP.ClientID


        If RequirePackingSlip Then
            GetFormattedLabelRequest.featureLabelTypeNeeded.Add("PKG_SLIP")
        Else
            GetFormattedLabelRequest.featureLabelTypeNeeded.Add("LABEL")
        End If

        If imageType = 0 Then
            GetFormattedLabelRequest.imageType = "ZPLII"
        Else
            GetFormattedLabelRequest.imageType = "PDF"
        End If

        GetFormattedLabelRequest.labelStockType = "STOCK_4X6"
        GetFormattedLabelRequest.labelPrintingOrientation = "TOP_EDGE_OF_TEXT_FIRST"
        GetFormattedLabelRequest.labelRotation = "LEFT"


        Dim jsonPayload As String = JsonConvert.SerializeObject(GetFormattedLabelRequest, Formatting.Indented, New JsonSerializerSettings With {
       .NullValueHandling = NullValueHandling.Ignore,
       .DefaultValueHandling = DefaultValueHandling.Ignore,
       .ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
       .ContractResolver = ShouldSerializeContractResolver.Instance})


        request.AddParameter("application/x-www-form-urlencoded", jsonPayload, ParameterType.RequestBody)

        Dim response As IRestResponse = client.Execute(request)

        Debug.Print(jsonPayload)

        System.IO.File.WriteAllText(gFedExReturnsSETUP.Path_Save_InOut_File & "/" & RMA_No & "_Request.txt", JObject.FromObject(GetFormattedLabelRequest).ToString)
        System.IO.File.WriteAllText(gFedExReturnsSETUP.Path_Save_InOut_File & "/" & RMA_No & "_Response.txt", JObject.Parse(response.Content).ToString)

        If response.StatusCode = HttpStatusCode.OK Then
            If FXReturns_ProcessResponse(response, GetFormattedLabelRequest.rmaId, trackingNo) Then
                Return True
            Else
                Return False
            End If

        Else
            MsgBox("Failed to create return request." & vbCrLf & vbCrLf & response.Content, vbExclamation)
            Debug.Print(response.Content)
            Return False
        End If

    End Function

    Private Function FXReturns_ProcessResponse(response As IRestResponse, rmaID As String, ByRef trackingNo As String) As Boolean
        Dim ResponseObj As FXReturns_GetFormattedLabelResponse
        Dim Notifications As String = ""
        Dim labelFilePath As String
        Dim SuccessMessage As String = ""
        Dim imageType As String
        Dim packingSlipPath As String = ""
        Dim count As Integer = 0
        Dim countStr As String = ""


        ResponseObj = JsonConvert.DeserializeObject(Of FXReturns_GetFormattedLabelResponse)(response.Content)

        If ResponseObj.highestSeverity = "ERROR" Or ResponseObj.highestSeverity = "FAILURE" Then
            Notifications = "FedEx server returned the following Errors: " & vbCrLf & vbCrLf
            For Each e In ResponseObj.notifications
                Notifications &= e.severity & "- " & e.message & vbCrLf
            Next

            MsgBox(Notifications, vbExclamation, "Error!")
            Return False
        End If


        'Assume Success
        If ResponseObj.label.Count > 1 Then
            'multiple tracking numbers returned
            SuccessMessage = "Return labels generated successfully!" & vbCrLf & vbCrLf & "Multiple labels returned!" & vbCrLf & vbCrLf & "Tracking#: " & vbCrLf
            For Each label As FXReturns_FeatureLabelData In ResponseObj.label
                SuccessMessage &= label.trackingNumber & vbCrLf
            Next

        Else
            '1 label returned
            SuccessMessage = "Return label generated successfully!" & vbCrLf & vbCrLf & "Tracking#: " & ResponseObj.label(0).trackingNumber
        End If

        'return tracking number
        trackingNo = ResponseObj.label(0).trackingNumber

        'Display Notifications
        For Each e In ResponseObj.notifications
            If e.severity <> "SUCCESS" Then
                Notifications &= e.severity & "- " & e.message & vbCrLf
            End If
        Next

        If Notifications <> "" Then
            SuccessMessage &= vbCrLf & vbCrLf & Notifications
        End If



        For Each lbl As FXReturns_FeatureLabelData In ResponseObj.label

            If ResponseObj.label.Count <> 1 Then
                'multi label
                countStr = "_" & count + 1
            End If


            'LABEL
            If ResponseObj.imageType = "PDF" Then
                labelFilePath = gFedExReturnsSETUP.Path_Save_InOut_File & "\" & rmaID & "_label" & countStr & ".pdf"
                imageType = "PDF"
            Else
                labelFilePath = gFedExReturnsSETUP.Path_Save_InOut_File & "\" & rmaID & "_label" & countStr & ".txt"
                imageType = "Thermal"
            End If

            _Files.WriteFile_ToEnd(_Convert.Base64String2Byte(lbl.labelContent), labelFilePath)
            _FedExWeb.print_LabelFromFile(imageType, rmaID, gFedExReturnsSETUP.Path_Save_InOut_File, countStr)


            'PACKING SLIP
            If ResponseObj.featureLabelType = "PKG_SLIP" Then

                If ResponseObj.imageType = "PDF" Then

                    'PDF packing slip
                    packingSlipPath = gFedExReturnsSETUP.Path_Save_InOut_File & "\" & rmaID & "_PackingSlip" & countStr & ".pdf"
                    _Files.WriteFile_ToEnd(_Convert.Base64String2Byte(lbl.packingSlipContent), packingSlipPath)

                    Process.Start(packingSlipPath)
                Else

                    '4x6 packing slip
                    packingSlipPath = gFedExReturnsSETUP.Path_Save_InOut_File & "\" & rmaID & "_PackingSlip" & countStr & ".txt"
                    _Files.WriteFile_ToEnd(_Convert.Base64String2Byte(lbl.packingSlipContent), packingSlipPath)

                    RawPrinterHelper.SendFileToPrinter(GetPolicyData(gReportsDB, "LabelPrinter"), packingSlipPath)
                End If
            End If

            count += 1
        Next


        MsgBox(SuccessMessage, vbInformation, "Success")

        Return True

    End Function

    Public Function FXReturns_Get_OAuth_Token() As Boolean
        Try
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12

            Dim client = New RestClient(gFedExReturnsSETUP.URL & "/auth/oauth/v2/token")
            Dim request = New RestRequest(Method.POST)
            Dim result As JObject
            Dim ExpiresIn As TimeSpan

            If Not Is_Current_OAuthToken_Expired() Then
                'existing token still valid, exit sub
                Return True
            End If


            request.AddHeader("Content-Type", "application/x-www-form-urlencoded")

            request.AddParameter("grant_type", "client_credentials")
            request.AddParameter("client_id", gFedExReturnsSETUP.ClientID)
            request.AddParameter("client_secret", gFedExReturnsSETUP.ClientSecret)

            Dim response As IRestResponse = client.Execute(request)


            'Saving Files with tokens should be for FedEx certification / bug fixing purposes only.
            _Files.IsFolderExist_CreateIfNot(gFedExReturnsSETUP.Path_Save_InOut_File, False)
            System.IO.File.WriteAllText(gFedExReturnsSETUP.Path_Save_InOut_File & "/GetToken_Request.txt", JObject.FromObject(request).ToString)
            System.IO.File.WriteAllText(gFedExReturnsSETUP.Path_Save_InOut_File & "/GetToken_Parent_Response.txt", JObject.Parse(response.Content).ToString)




            If response.StatusCode = HttpStatusCode.OK Then
                result = JObject.Parse(response.Content)

                gFedExReturnsSETUP.OAuthToken = result.SelectToken("access_token")
                ExpiresIn = TimeSpan.FromSeconds(result.SelectToken("expires_in"))
                gFedExReturnsSETUP.OAuthExpires = Now + ExpiresIn

                UpdatePolicy(gShipriteDB, "FedExRETURN_OAuth_Token", gFedExReturnsSETUP.OAuthToken)
                UpdatePolicy(gShipriteDB, "FedExRETURN_OAuth_Expires", gFedExReturnsSETUP.OAuthExpires)

                Return True
            Else
                MsgBox("Failed to obtain OAuth token." & vbCrLf & vbCrLf & response.Content, vbExclamation)
                Return False
            End If

        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Error obtaining OAuth token...")
            Return False
        End Try
    End Function

    Private Function Is_Current_OAuthToken_Expired() As Boolean
        Try
            gFedExReturnsSETUP.OAuthToken = GetPolicyData(gShipriteDB, "FedExRETURN_OAuth_Token", "")

            If GetPolicyData(gShipriteDB, "FedExRETURN_OAuth_Expires") = "" Then
                Return True
            Else
                gFedExReturnsSETUP.OAuthExpires = GetPolicyData(gShipriteDB, "FedExRETURN_OAuth_Expires")
            End If

            If gFedExReturnsSETUP.OAuthToken = "" Then
                Return True
            End If

            If gFedExReturnsSETUP.OAuthExpires = "1/1/0001" Then
                Return True
            End If

            If gFedExReturnsSETUP.OAuthExpires.AddSeconds(-10) > DateTime.Now Then
                Return False
            Else
                Return True
            End If

        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Erro checking OAuth token...")
            Return False
        End Try
    End Function

End Module
