Imports System.Xml.Serialization
Imports System.IO
Imports System.Net.NetworkInformation
Imports System.Drawing
Imports SHIPRITE.FedEx_ScanPostingService

Public Module _FedExHAL

    Public StoreOwner As New _baseContact
    Public Clerk As String

    Public Function Get_MacAddress() As String
        Try
            Dim adapters As NetworkInterface() = NetworkInterface.GetAllNetworkInterfaces()
            Dim adapter As NetworkInterface
            Dim myMac As String = String.Empty

            For Each adapter In adapters
                '_Debug.Print_(adapter.Name, adapter.Description)
                Select Case adapter.NetworkInterfaceType
                    'Exclude Tunnels, Loopbacks and PPP
                    Case NetworkInterfaceType.Tunnel, NetworkInterfaceType.Loopback, NetworkInterfaceType.Ppp
                    Case Else
                        If Not adapter.GetPhysicalAddress.ToString = String.Empty And Not adapter.GetPhysicalAddress.ToString = "00000000000000E0" Then
                            myMac = adapter.GetPhysicalAddress.ToString
                            Exit For ' Got a mac so exit for
                        End If

                End Select
            Next adapter

            Return String.Format("R{0}", _Controls.Right(myMac, 11))
        Catch ex As Exception
            Return String.Empty
        End Try
    End Function

#Region "FedEx Web HAL Calls"

    Public Function Process_TransferAcceptanceEvent_Request(ByRef vb_response As baseWebResponse_Shipment) As Boolean
        Process_TransferAcceptanceEvent_Request = False ' assume.
        Try
            Dim webService As New FedEx_ScanPostingService.ScanPostingService
            webService.Url = _FedExWeb.objFedEx_Setup.Connect2ServerURL & "/scanposting" '"https://ws.fedex.com:443/web-services"

            Dim webRequest As New FedEx_ScanPostingService.PublishTransferAcceptanceEventRequest
            With webRequest
                Dim webauth As New FedEx_ScanPostingService.WebAuthenticationDetail
                Dim webauth_csp As New FedEx_ScanPostingService.WebAuthenticationCredential
                Dim webauth_user As New FedEx_ScanPostingService.WebAuthenticationCredential
                If create_WebAuthenticationCredential(_FedExWeb.objFedEx_Setup.Web_CspCredential_Key, _FedExWeb.objFedEx_Setup.Web_CspCredential_Pass, webauth_csp) Then
                    If create_WebAuthenticationCredential(_FedExWeb.objFedEx_Setup.Web_UserCredential_Key, _FedExWeb.objFedEx_Setup.Web_UserCredential_Pass, webauth_user) Then
                        webauth.ParentCredential = webauth_csp
                        webauth.UserCredential = webauth_user
                        .WebAuthenticationDetail = webauth
                        '
                        Dim client As New FedEx_ScanPostingService.ClientDetail
                        If create_ClientDetail(client) Then
                            .ClientDetail = client
                            '
                            .TransactionDetail = New FedEx_ScanPostingService.TransactionDetail
                            .TransactionDetail.CustomerTransactionId = "Publish Transfer/Acceptance Event Request"
                            '
                            .Version = New FedEx_ScanPostingService.VersionId
                            If create_Version("spst", 3, 0, 0, .Version) Then
                                '
                                Dim pkgArray(vb_response.Packages.Count - 1) As FedEx_ScanPostingService.TransferAcceptanceEventPackageDetail
                                For i As Integer = 0 To vb_response.Packages.Count - 1
                                    '
                                    Dim pack As baseWebResponse_Package = vb_response.Packages(i)
                                    '
                                    Dim evntpackdetail As New FedEx_ScanPostingService.TransferAcceptanceEventPackageDetail
                                    Dim packdetail As New FedEx_ScanPostingService.PackageEventDetail

                                    Dim barcode As New FedEx_ScanPostingService.Barcode
                                    barcode.Format = FedEx_ScanPostingService.BarcodeFormatType.STRING
                                    barcode.FormatSpecified = True
                                    barcode.Symbology = FedEx_ScanPostingService.BarcodeSymbologyType.CODE128
                                    barcode.SymbologySpecified = True
                                    Dim barcodestring As New FedEx_ScanPostingService.StringBarcode

                                    packdetail.BarcodeEntryTypeSpecified = True
                                    If (pack.PackageID.Length > 12) Then
                                        packdetail.BarcodeEntryType = FedEx_ScanPostingService.BarcodeEntryType.SCAN
                                        barcodestring.Value = String.Format("&{0}&", pack.PackageID)

                                        'evntpackdetail.Barcode = String.Format("&{0}&", pack.PackageID) ' scanned barcode. Note: The ampersand before and after. Its required.
                                    Else
                                        packdetail.BarcodeEntryType = FedEx_ScanPostingService.BarcodeEntryType.MANUAL_ENTRY
                                        barcodestring.Value = pack.PackageID
                                        'evntpackdetail.Barcode = pack.PackageID
                                    End If
                                    barcode.StringBarcode = barcodestring
                                    packdetail.Barcodes = {barcode}

                                    ' this data is NOT needed if barcode is passed
                                    'If (evntpackdetail.BarcodeEntryType = BarcodeEntryType.MANUAL_ENTRY) Then
                                    '    Dim bcodedata As New PackageEventBarcodeData
                                    '    bcodedata.OperatingCompany = OperatingCompanyType.FEDEX_EXPRESS ' optional
                                    '    bcodedata.OperatingCompanySpecified = True
                                    '    bcodedata.TrackingNumber = pack.TrackingNo
                                    '    evntpackdetail.BarcodeData = bcodedata
                                    'End If
                                    '
                                    evntpackdetail.PackageEventDetail = packdetail
                                    pkgArray(i) = evntpackdetail
                                    '
                                Next i
                                .Event = pkgArray
                                '
                                Dim capturedetail As New FedEx_ScanPostingService.EventCaptureDetail
                                capturedetail.LocationId = _FedExWeb.objFedEx_Setup.OriginLocationId ' FedEx location id assigned to this location like HKAA, OLVA etc…. For UTICA, NY, the location id is UCAA
                                capturedetail.LocationType = FedEx_ScanPostingService.FedExLocationType.FEDEX_AUTHORIZED_SHIP_CENTER
                                capturedetail.LocationTypeSpecified = True
                                capturedetail.Timestamp = Date.Now
                                capturedetail.TimestampSpecified = True
                                capturedetail.DeviceId = _FedExHAL.Get_MacAddress '"R00118688916" ' Should always be starting with “R” followed by Mac address encoded to 11 char string.
                                '
                                .EventCaptureDetail = capturedetail
                                '
                                Dim captureAgent As New FedEx_ScanPostingService.EventCapturingAgent
                                captureAgent.Id = _FedExWeb.objFedEx_Setup.ApplicationId ' emp# should be associated with location (FedEx should setup and provide this info)
                                captureAgent.Type = FedEx_ScanPostingService.EventCapturingAgentType.AUTHORIZED_AGENT
                                captureAgent.TypeSpecified = True
                                '
                                .EventCaptureDetail.Agent = captureAgent
                                '
                                ' option set to VALIDATE_ONLY, scans will NOT posted. It will just validate.
                                'Dim processoptns As New FedEx_ScanPostingService.PublishTenderedPackageEventProcessingOptionType
                                'processoptns = PublishTenderedPackageEventProcessingOptionType.VALIDATE_ONLY
                                ''
                                '.ProcessingOptions = {processoptns}
                                '' 
                                Dim xdoc As New Xml.XmlDocument
                                xdoc.LoadXml(serializeShipRequest2string(webRequest))
                                Dim pack2 As baseWebResponse_Package = vb_response.Packages(0)
                                xdoc.Save(String.Format("{0}\TransferAcceptance_{1}_Request.xml", _FedExWeb.objFedEx_Setup.Path_SaveDocXML, pack2.TrackingNo))
                                ''
                                Process_TransferAcceptanceEvent_Request = process_PublishTransferAcceptanceEvent_Reply(webService, webRequest, vb_response)
                                '
                            End If
                            '
                        End If
                        '
                    End If
                    '
                End If
                '
            End With

        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to create a 'Transfer/Acceptance Package' request...")
        End Try
    End Function
    Private Function process_PublishTransferAcceptanceEvent_Reply(ByVal webService As FedEx_ScanPostingService.ScanPostingService, ByVal webRequest As FedEx_ScanPostingService.PublishTransferAcceptanceEventRequest, ByRef vb_response As baseWebResponse_Shipment) As Boolean
        process_PublishTransferAcceptanceEvent_Reply = False ' assume.
        Try
            System.Net.ServicePointManager.SecurityProtocol = Net.SecurityProtocolType.Tls12
            Dim webReply As FedEx_ScanPostingService.PublishTransferAcceptanceEventReply = webService.publishTransferAcceptanceEvent(webRequest)
            '
            Dim xdoc As New Xml.XmlDocument
            xdoc.LoadXml(serializeShipResponse2string(webReply))
            Dim pack2 As baseWebResponse_Package = vb_response.Packages(0)
            xdoc.Save(String.Format("{0}\TransferAcceptance_{1}_Reply.xml", _FedExWeb.objFedEx_Setup.Path_SaveDocXML, pack2.TrackingNo))
            '
            ' Result
            If webReply.Notifications IsNot Nothing Then
                Dim notify As FedEx_ScanPostingService.Notification = webReply.Notifications(0)
                ''ol#1.2.49(2/17)... FedEx HAL don't show WARNING, only ERROR
                '' If Not "0000" = notify.Code Then
                If notify.SeveritySpecified AndAlso (notify.Severity = FedEx_ScanPostingService.NotificationSeverityType.ERROR Or notify.Severity = FedEx_ScanPostingService.NotificationSeverityType.FAILURE) Then
                    vb_response.ShipmentAlerts.Add(notify.Message)
                ElseIf Not notify.SeveritySpecified AndAlso Not "0000" = notify.Code Then
                    vb_response.ShipmentAlerts.Add(notify.Message)
                End If
            End If
            '
            If webReply.Results IsNot Nothing Then
                For i As Integer = 0 To webReply.Results.Length - 1
                    Dim packresult As FedEx_ScanPostingService.PackageEventResult = webReply.Results(i)
                    Dim pack As baseWebResponse_Package = vb_response.Packages(i)
                    If packresult.TrackingNumber IsNot Nothing Then
                        pack.TrackingNo = packresult.TrackingNumber
                    End If
                    If packresult.OperatingCompany.ToString IsNot Nothing Then
                        pack.ServiceCode = packresult.OperatingCompany.ToString
                    End If
                    If packresult.BarcodeHandlings IsNot Nothing Then
                        pack.LabelImage = packresult.BarcodeHandlings(0).ToString
                    End If
                Next
            End If
            '
            process_PublishTransferAcceptanceEvent_Reply = True
            '
        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message) : _Debug.Print_(ex.Detail.LastChild.InnerText)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to get a response for 'Transfer/Acceptance Package' request...")
        End Try
    End Function
    Private Function serializeShipRequest2string(obj As FedEx_ScanPostingService.PublishTransferAcceptanceEventRequest) As String
        Dim xml_serializer As New XmlSerializer(GetType(FedEx_ScanPostingService.PublishTransferAcceptanceEventRequest))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeShipRequest2string = string_writer.ToString()
        string_writer.Close()
    End Function
    Private Function serializeShipResponse2string(obj As FedEx_ScanPostingService.PublishTransferAcceptanceEventReply) As String
        Dim xml_serializer As New XmlSerializer(GetType(FedEx_ScanPostingService.PublishTransferAcceptanceEventReply))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeShipResponse2string = string_writer.ToString()
        string_writer.Close()
    End Function


    Public Function Process_PublishDeliveryEvent_Request(ByVal signaturefile As String, ByRef vb_response As baseWebResponse_Shipment) As Boolean
        Process_PublishDeliveryEvent_Request = False ' assume.
        Try
            Dim webService As New FedEx_ScanPostingService.ScanPostingService
            webService.Url = _FedExWeb.objFedEx_Setup.Connect2ServerURL & "/scanposting" '"https://ws.fedex.com:443/web-services"

            Dim webRequest As New FedEx_ScanPostingService.PublishDeliveryEventRequest
            With webRequest
                Dim webauth As New FedEx_ScanPostingService.WebAuthenticationDetail
                Dim webauth_csp As New FedEx_ScanPostingService.WebAuthenticationCredential
                Dim webauth_user As New FedEx_ScanPostingService.WebAuthenticationCredential
                If create_WebAuthenticationCredential(_FedExWeb.objFedEx_Setup.Web_CspCredential_Key, _FedExWeb.objFedEx_Setup.Web_CspCredential_Pass, webauth_csp) Then
                    If create_WebAuthenticationCredential(_FedExWeb.objFedEx_Setup.Web_UserCredential_Key, _FedExWeb.objFedEx_Setup.Web_UserCredential_Pass, webauth_user) Then
                        webauth.ParentCredential = webauth_csp
                        webauth.UserCredential = webauth_user
                        .WebAuthenticationDetail = webauth
                        '
                        Dim client As New FedEx_ScanPostingService.ClientDetail
                        If create_ClientDetail(client) Then
                            .ClientDetail = client

                            .TransactionDetail = New FedEx_ScanPostingService.TransactionDetail
                            .TransactionDetail.CustomerTransactionId = "Publish Delivery Event Request"

                            .Version = New FedEx_ScanPostingService.VersionId
                            If create_Version("spst", 3, 0, 0, .Version) Then
                                .Event = New FedEx_ScanPostingService.DeliveredPackageEvent

                                Dim pkgArray(vb_response.Packages.Count - 1) As FedEx_ScanPostingService.DeliveredPackageEventDetail
                                For i As Integer = 0 To vb_response.Packages.Count - 1
                                    Dim pack As baseWebResponse_Package = vb_response.Packages(i)

                                    Dim evntpackdetail As New FedEx_ScanPostingService.DeliveredPackageEventDetail
                                    Dim packdetail As New FedEx_ScanPostingService.PackageEventDetail

                                    Dim barcode As New FedEx_ScanPostingService.Barcode
                                    barcode.Format = FedEx_ScanPostingService.BarcodeFormatType.STRING
                                    barcode.FormatSpecified = True
                                    barcode.Symbology = FedEx_ScanPostingService.BarcodeSymbologyType.CODE128
                                    barcode.SymbologySpecified = True

                                    Dim barcodestring As New FedEx_ScanPostingService.StringBarcode

                                    ''evntpackdetail.Barcode = String.Format("&{0}&", pack.PackageID)
                                    packdetail.BarcodeEntryTypeSpecified = True
                                    If pack.PackageID > 12 Then
                                        packdetail.BarcodeEntryType = FedEx_ScanPostingService.BarcodeEntryType.SCAN
                                        barcodestring.Type = StringBarcodeType.ASTRA
                                        barcodestring.TypeSpecified = True
                                        barcodestring.Value = "&" & pack.PackageID & "&"
                                    Else
                                        packdetail.BarcodeEntryType = FedEx_ScanPostingService.BarcodeEntryType.MANUAL_ENTRY
                                        barcodestring.Value = pack.PackageID
                                    End If
                                    barcode.StringBarcode = barcodestring
                                    packdetail.Barcodes = {barcode}

                                    ''ol#1.2.61(9/18)... 'Refused Package' should be have 'Refused' tag in ProofOfDelivery call.
                                    If "Refused" = pack.LabelCODImage Then
                                        Dim refuse As New FedEx_ScanPostingService.RefusedPackageDeliveryDetail
                                        evntpackdetail.Status = FedEx_ScanPostingService.PackageDeliveryEventStatusType.REFUSED
                                        refuse.Reason = FedEx_ScanPostingService.RefusedPackageDeliveryReasonType.OTHER
                                        refuse.ReasonSpecified = True
                                        refuse.Description = pack.LabelCustomsImage
                                        evntpackdetail.RefusalDetail = refuse
                                    Else
                                        evntpackdetail.Status = FedEx_ScanPostingService.PackageDeliveryEventStatusType.DELIVERED
                                    End If
                                    evntpackdetail.StatusSpecified = True
                                    evntpackdetail.PackageEventDetail = packdetail
                                    '
                                    pkgArray(i) = evntpackdetail
                                    '
                                Next i
                                .Event.Packages = pkgArray
                                '
                                Dim proofdelivery As New FedEx_ScanPostingService.DeliveredPackageProofOfDeliveryDetail
                                Dim signaturedetail As New FedEx_ScanPostingService.DeliveredPackageSignatureDetail

                                Dim converter As New ImageConverter
                                ' convert .png to .tiff for upload - SignatureDetail/Image requires Base64 encoded, TIFF image
                                Dim signatureBytes As Byte() = Nothing
                                Using ms As New MemoryStream
                                    Dim signature As Image = Image.FromFile(signaturefile)
                                    signature.Save(ms, Imaging.ImageFormat.Tiff)
                                    ms.Position = 0 ' beginning
                                    Dim sigTiff As Image = Image.FromStream(ms)
                                    signatureBytes = converter.ConvertTo(sigTiff, GetType(Byte()))
                                End Using
                                signaturedetail.Image = signatureBytes ' converter.ConvertTo(signature, GetType(Byte()))
                                signaturedetail.Type = FedEx_ScanPostingService.SignatureImageType.TIFF
                                signaturedetail.TypeSpecified = True
                                proofdelivery.SignatureDetail = signaturedetail
                                Dim recipient As New FedEx_ScanPostingService.Contact
                                Dim package As baseWebResponse_Package = vb_response.Packages(0)
                                If create_Contact(package.Recipient, recipient) Then
                                    proofdelivery.ActualRecipient = recipient
                                End If
                                '
                                .Event.ProofOfDeliveryDetail = proofdelivery
                                '
                                Dim capturedetail As New FedEx_ScanPostingService.EventCaptureDetail
                                capturedetail.LocationId = _FedExWeb.objFedEx_Setup.OriginLocationId ' FedEx location id assigned to this location like HKAA, OLVA etc…. For UTICA, NY, the location id is UCAA
                                capturedetail.LocationType = FedEx_ScanPostingService.FedExLocationType.FEDEX_AUTHORIZED_SHIP_CENTER
                                capturedetail.LocationTypeSpecified = True
                                capturedetail.Timestamp = Date.Now
                                capturedetail.TimestampSpecified = True
                                capturedetail.DeviceId = _FedExHAL.Get_MacAddress '"R00118688916" ' Should always be starting with “R” followed by Mac address encoded to 11 char string.
                                '
                                .EventCaptureDetail = capturedetail
                                '
                                Dim captureAgent As New FedEx_ScanPostingService.EventCapturingAgent
                                captureAgent.Id = _FedExWeb.objFedEx_Setup.ApplicationId ' emp# should be associated with location (FedEx should setup and provide this info)
                                captureAgent.Type = FedEx_ScanPostingService.EventCapturingAgentType.AUTHORIZED_AGENT
                                captureAgent.TypeSpecified = True
                                '
                                .EventCaptureDetail.Agent = captureAgent
                                ' 
                                Dim xdoc As New Xml.XmlDocument
                                xdoc.LoadXml(serializeShipRequest2string(webRequest))

                                Dim pack2 As baseWebResponse_Package = vb_response.Packages(0)

                                xdoc.Save(String.Format("{0}\DeliveryProof_{1}_Request.xml", _FedExWeb.objFedEx_Setup.Path_SaveDocXML, pack2.TrackingNo))
                                ''
                                Process_PublishDeliveryEvent_Request = process_PublishDeliveryEvent_Reply(webService, webRequest, vb_response)
                                '
                            End If
                        End If
                    End If
                End If
            End With

        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to create a 'Delivery Proof' request...")
        End Try
    End Function
    Private Function process_PublishDeliveryEvent_Reply(ByVal webService As FedEx_ScanPostingService.ScanPostingService, ByVal webRequest As FedEx_ScanPostingService.PublishDeliveryEventRequest, ByRef vb_response As baseWebResponse_Shipment) As Boolean
        process_PublishDeliveryEvent_Reply = False ' assume.
        Try
            System.Net.ServicePointManager.SecurityProtocol = Net.SecurityProtocolType.Tls12
            Dim webReply As FedEx_ScanPostingService.PublishDeliveryEventReply = webService.publishDeliveryEvent(webRequest)
            '
            Dim xdoc As New Xml.XmlDocument
            xdoc.LoadXml(serializeShipResponse2string(webReply))
            Dim pack2 As baseWebResponse_Package = vb_response.Packages(0)
            xdoc.Save(String.Format("{0}\DeliveryProof_{1}_Reply.xml", _FedExWeb.objFedEx_Setup.Path_SaveDocXML, pack2.TrackingNo))
            '
            ' Result
            If webReply.Notifications IsNot Nothing Then
                Dim notify As FedEx_ScanPostingService.Notification = webReply.Notifications(0)
                ''ol#1.2.49(2/17)... FedEx HAL don't show WARNING, only ERROR
                '' If Not "0000" = notify.Code Then
                If notify.SeveritySpecified AndAlso (notify.Severity = FedEx_ScanPostingService.NotificationSeverityType.ERROR Or notify.Severity = FedEx_ScanPostingService.NotificationSeverityType.FAILURE) Then
                    vb_response.ShipmentAlerts.Add(notify.Message)
                ElseIf Not notify.SeveritySpecified AndAlso Not "0000" = notify.Code Then
                    vb_response.ShipmentAlerts.Add(notify.Message)
                End If
            End If
            '
            If webReply.Results IsNot Nothing Then
                For i As Integer = 0 To webReply.Results.Length - 1
                    Dim packresult As FedEx_ScanPostingService.PackageEventResult = webReply.Results(i)
                    Dim pack As baseWebResponse_Package = vb_response.Packages(i)
                    If packresult.TrackingNumber IsNot Nothing Then
                        pack.TrackingNo = packresult.TrackingNumber
                    End If
                    If packresult.OperatingCompany.ToString IsNot Nothing Then
                        pack.ServiceCode = packresult.OperatingCompany.ToString
                    End If
                    If packresult.BarcodeHandlings IsNot Nothing Then
                        pack.LabelImage = packresult.BarcodeHandlings(0).ToString
                    End If
                Next
            End If

            process_PublishDeliveryEvent_Reply = True

        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message) : _Debug.Print_(ex.Detail.LastChild.InnerText)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to get a response for 'Delivery Proof' request...")
        End Try
    End Function
    Private Function serializeShipRequest2string(obj As FedEx_ScanPostingService.PublishDeliveryEventRequest) As String
        Dim xml_serializer As New XmlSerializer(GetType(FedEx_ScanPostingService.PublishDeliveryEventRequest))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeShipRequest2string = string_writer.ToString()
        string_writer.Close()
    End Function
    Private Function serializeShipResponse2string(obj As FedEx_ScanPostingService.PublishDeliveryEventReply) As String
        Dim xml_serializer As New XmlSerializer(GetType(FedEx_ScanPostingService.PublishDeliveryEventReply))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeShipResponse2string = string_writer.ToString()
        string_writer.Close()
    End Function


    Public Function Process_PublishTenderedPackageEvent_Request(ByRef vb_response As Object) As Boolean
        Process_PublishTenderedPackageEvent_Request = False ' assume.
        Try
            Dim webService As New FedEx_ScanPostingService.ScanPostingService
            webService.Url = _FedExWeb.objFedEx_Setup.Connect2ServerURL & "/scanposting" '"https://ws.fedex.com:443/web-services"

            Dim webRequest As New FedEx_ScanPostingService.PublishTenderedPackageEventRequest
            With webRequest
                Dim webauth As New FedEx_ScanPostingService.WebAuthenticationDetail
                Dim webauth_csp As New FedEx_ScanPostingService.WebAuthenticationCredential
                Dim webauth_user As New FedEx_ScanPostingService.WebAuthenticationCredential
                If create_WebAuthenticationCredential(_FedExWeb.objFedEx_Setup.Web_CspCredential_Key, _FedExWeb.objFedEx_Setup.Web_CspCredential_Pass, webauth_csp) Then
                    If create_WebAuthenticationCredential(_FedExWeb.objFedEx_Setup.Web_UserCredential_Key, _FedExWeb.objFedEx_Setup.Web_UserCredential_Pass, webauth_user) Then
                        webauth.ParentCredential = webauth_csp
                        webauth.UserCredential = webauth_user
                        .WebAuthenticationDetail = webauth
                        '
                        Dim client As New FedEx_ScanPostingService.ClientDetail
                        If create_ClientDetail(client) Then
                            .ClientDetail = client
                            '
                            .TransactionDetail = New FedEx_ScanPostingService.TransactionDetail
                            .TransactionDetail.CustomerTransactionId = "Publish Tendered Package Event Request"
                            '
                            .Version = New FedEx_ScanPostingService.VersionId
                            If create_Version("spst", 3, 0, 0, .Version) Then
                                '
                                Dim pkgArray(vb_response.Packages.Count - 1) As FedEx_ScanPostingService.TenderedPackageEventPackageDetail
                                For i As Integer = 0 To vb_response.Packages.Count - 1
                                    '
                                    Dim pack As baseWebResponse_Package = vb_response.Packages(i)
                                    '
                                    Dim evntpackdetail As New FedEx_ScanPostingService.TenderedPackageEventPackageDetail
                                    Dim packdetail As New FedEx_ScanPostingService.PackageEventDetail

                                    Dim barcode As New FedEx_ScanPostingService.Barcode
                                    barcode.Format = FedEx_ScanPostingService.BarcodeFormatType.STRING
                                    barcode.FormatSpecified = True
                                    barcode.Symbology = FedEx_ScanPostingService.BarcodeSymbologyType.CODE128
                                    barcode.SymbologySpecified = True
                                    Dim barcodestring As New FedEx_ScanPostingService.StringBarcode

                                    packdetail.BarcodeEntryTypeSpecified = True
                                    If (pack.PackageID.Length > 12) Then
                                        packdetail.BarcodeEntryType = FedEx_ScanPostingService.BarcodeEntryType.SCAN
                                        barcodestring.Value = String.Format("&{0}&", pack.PackageID)
                                        ''evntpackdetail.Barcode = String.Format("&{0}&", pack.PackageID) ' scanned barcode. Note: The ampersand before and after. Its required.
                                    Else
                                        packdetail.BarcodeEntryType = FedEx_ScanPostingService.BarcodeEntryType.MANUAL_ENTRY
                                        barcodestring.Value = pack.PackageID
                                        ''evntpackdetail.Barcode = pack.PackageID
                                    End If
                                    barcode.StringBarcode = barcodestring
                                    packdetail.Barcodes = {barcode}
                                    '
                                    ' this data is NOT needed if barcode is passed
                                    'Dim bcodedata As New PackageEventBarcodeData
                                    'bcodedata.OperatingCompany = OperatingCompanyType.FEDEX_EXPRESS
                                    'bcodedata.OperatingCompanySpecified = True
                                    'bcodedata.TrackingNumber = String.Empty
                                    'evntpackdetail.BarcodeData = bcodedata
                                    '
                                    '
                                    evntpackdetail.PackageEventDetail = packdetail
                                    pkgArray(i) = evntpackdetail
                                    '
                                Next i
                                .Event = pkgArray
                                '
                                Dim capturedetail As New FedEx_ScanPostingService.EventCaptureDetail
                                capturedetail.LocationId = _FedExWeb.objFedEx_Setup.OriginLocationId ' FedEx location id assigned to this location like HKAA, OLVA etc…. For UTICA, NY, the location id is UCAA
                                capturedetail.LocationType = FedEx_ScanPostingService.FedExLocationType.FEDEX_AUTHORIZED_SHIP_CENTER
                                capturedetail.LocationTypeSpecified = True
                                capturedetail.Timestamp = Date.Now
                                capturedetail.TimestampSpecified = True
                                capturedetail.DeviceId = _FedExHAL.Get_MacAddress '"R00118688916" ' Should always be starting with “R” followed by Mac address encoded to 11 char string.
                                '
                                .EventCaptureDetail = capturedetail
                                '
                                Dim captureAgent As New FedEx_ScanPostingService.EventCapturingAgent
                                captureAgent.Id = _FedExWeb.objFedEx_Setup.ApplicationId ' emp# should be associated with location (FedEx should setup and provide this info)
                                captureAgent.Type = FedEx_ScanPostingService.EventCapturingAgentType.AUTHORIZED_AGENT
                                captureAgent.TypeSpecified = True
                                '
                                .EventCaptureDetail.Agent = captureAgent
                                '
                                ' option set to VALIDATE_ONLY, scans will NOT posted. It will just validate.
                                Dim processoptns As New FedEx_ScanPostingService.PublishTenderedPackageEventProcessingOptionType
                                processoptns = FedEx_ScanPostingService.PublishTenderedPackageEventProcessingOptionType.VALIDATE_ONLY
                                ''
                                .ProcessingOptions = {processoptns}
                                '' 
                                Dim xdoc As New Xml.XmlDocument
                                xdoc.LoadXml(serializeShipRequest2string(webRequest))
                                Dim pack2 As baseWebResponse_Package = vb_response.Packages(0)
                                xdoc.Save(String.Format("{0}\TenderedPackage_{1}_Request.xml", _FedExWeb.objFedEx_Setup.Path_SaveDocXML, pack2.TrackingNo))
                                ''
                                Process_PublishTenderedPackageEvent_Request = process_PublishTenderedPackageEvent_Reply(webService, webRequest, vb_response)
                                '
                            End If
                        End If
                    End If
                End If
                '
            End With

        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to create a 'Tendered Package' request...")
        End Try
    End Function
    Private Function process_PublishTenderedPackageEvent_Reply(ByVal webService As FedEx_ScanPostingService.ScanPostingService, ByVal webRequest As FedEx_ScanPostingService.PublishTenderedPackageEventRequest, ByRef vb_response As baseWebResponse_Shipment) As Boolean
        process_PublishTenderedPackageEvent_Reply = False ' assume.
        Try
            System.Net.ServicePointManager.SecurityProtocol = Net.SecurityProtocolType.Tls12
            Dim webReply As FedEx_ScanPostingService.PublishTenderedPackageEventReply = webService.publishTenderedPackageEvent(webRequest)
            '
            Dim xdoc As New Xml.XmlDocument
            xdoc.LoadXml(serializeShipResponse2string(webReply))
            Dim pack2 As baseWebResponse_Package = vb_response.Packages(0)
            xdoc.Save(String.Format("{0}\TenderedPackage_{1}_Reply.xml", _FedExWeb.objFedEx_Setup.Path_SaveDocXML, pack2.TrackingNo))
            '
            ' Result
            If webReply.Notifications IsNot Nothing Then
                Dim notify As FedEx_ScanPostingService.Notification = webReply.Notifications(0)
                ''ol#1.2.49(2/17)... FedEx HAL don't show WARNING, only ERROR
                '' If Not "0000" = notify.Code Then
                If notify.SeveritySpecified AndAlso (notify.Severity = FedEx_ScanPostingService.NotificationSeverityType.ERROR Or notify.Severity = FedEx_ScanPostingService.NotificationSeverityType.FAILURE) Then
                    vb_response.ShipmentAlerts.Add(notify.Message)
                ElseIf Not notify.SeveritySpecified AndAlso Not "0000" = notify.Code Then
                    vb_response.ShipmentAlerts.Add(notify.Message)
                End If
            End If
            '
            If webReply.Results IsNot Nothing Then
                For i As Integer = 0 To webReply.Results.Length - 1
                    Dim packresult As FedEx_ScanPostingService.PackageEventResult = webReply.Results(i)
                    Dim pack As baseWebResponse_Package = vb_response.Packages(i + 1)
                    If packresult.TrackingNumber IsNot Nothing Then
                        pack.TrackingNo = packresult.TrackingNumber
                    End If
                    If packresult.OperatingCompany.ToString IsNot Nothing Then
                        pack.ServiceCode = packresult.OperatingCompany.ToString
                    End If
                    If packresult.BarcodeHandlings IsNot Nothing Then
                        pack.LabelImage = packresult.BarcodeHandlings(0).ToString
                    End If
                Next
            End If
            '
            process_PublishTenderedPackageEvent_Reply = True
            '
        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message) : _Debug.Print_(ex.Detail.LastChild.InnerText)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to get a response for 'Tendered Package' request...")
        End Try
    End Function
    Private Function serializeShipRequest2string(obj As FedEx_ScanPostingService.PublishTenderedPackageEventRequest) As String
        Dim xml_serializer As New XmlSerializer(GetType(FedEx_ScanPostingService.PublishTenderedPackageEventRequest))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeShipRequest2string = string_writer.ToString()
        string_writer.Close()
    End Function
    Private Function serializeShipResponse2string(obj As FedEx_ScanPostingService.PublishTenderedPackageEventReply) As String
        Dim xml_serializer As New XmlSerializer(GetType(FedEx_ScanPostingService.PublishTenderedPackageEventReply))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeShipResponse2string = string_writer.ToString()
        string_writer.Close()
    End Function


    Public Function Process_PublishInventoryEvent_Request(ByRef vb_response As baseWebResponse_Shipment) As Boolean
        Process_PublishInventoryEvent_Request = False ' assume.
        Try
            Dim webService As New FedEx_ScanPostingService.ScanPostingService
            webService.Url = _FedExWeb.objFedEx_Setup.Connect2ServerURL & "/scanposting" '"https://ws.fedex.com:443/web-services"

            Dim webRequest As New FedEx_ScanPostingService.PublishInventoryEventRequest
            With webRequest
                Dim webauth As New FedEx_ScanPostingService.WebAuthenticationDetail
                Dim webauth_csp As New FedEx_ScanPostingService.WebAuthenticationCredential
                Dim webauth_user As New FedEx_ScanPostingService.WebAuthenticationCredential
                If create_WebAuthenticationCredential(_FedExWeb.objFedEx_Setup.Web_CspCredential_Key, _FedExWeb.objFedEx_Setup.Web_CspCredential_Pass, webauth_csp) Then
                    If create_WebAuthenticationCredential(_FedExWeb.objFedEx_Setup.Web_UserCredential_Key, _FedExWeb.objFedEx_Setup.Web_UserCredential_Pass, webauth_user) Then
                        webauth.ParentCredential = webauth_csp
                        webauth.UserCredential = webauth_user
                        .WebAuthenticationDetail = webauth
                        '
                        Dim client As New FedEx_ScanPostingService.ClientDetail
                        If create_ClientDetail(client) Then
                            .ClientDetail = client
                            '
                            .TransactionDetail = New FedEx_ScanPostingService.TransactionDetail
                            .TransactionDetail.CustomerTransactionId = "Publish Inventory Event Request"
                            '
                            .Version = New FedEx_ScanPostingService.VersionId
                            If create_Version("spst", 3, 0, 0, .Version) Then
                                '
                                Dim pkgArray(vb_response.Packages.Count - 1) As FedEx_ScanPostingService.InventoryEventPackageDetail
                                For i As Integer = 0 To vb_response.Packages.Count - 1
                                    '
                                    Dim pack As baseWebResponse_Package = vb_response.Packages(i)
                                    '
                                    Dim evntpackdetail As New FedEx_ScanPostingService.InventoryEventPackageDetail
                                    Dim packdetail As New FedEx_ScanPostingService.PackageEventDetail

                                    Dim barcode As New FedEx_ScanPostingService.Barcode
                                    barcode.Format = FedEx_ScanPostingService.BarcodeFormatType.STRING
                                    barcode.FormatSpecified = True
                                    barcode.Symbology = FedEx_ScanPostingService.BarcodeSymbologyType.CODE128
                                    barcode.SymbologySpecified = True
                                    Dim barcodestring As New FedEx_ScanPostingService.StringBarcode

                                    packdetail.BarcodeEntryTypeSpecified = True
                                    If (pack.PackageID.Length > 12) Then
                                        packdetail.BarcodeEntryType = FedEx_ScanPostingService.BarcodeEntryType.SCAN
                                        barcodestring.Type = FedEx_ScanPostingService.StringBarcodeType.ASTRA
                                        barcodestring.TypeSpecified = True
                                        barcodestring.Value = String.Format("&{0}&", pack.PackageID)

                                        'evntpackdetail.Barcode = String.Format("&{0}&", pack.PackageID) ' scanned barcode. Note: The ampersand before and after. Its required.
                                    Else
                                        packdetail.BarcodeEntryType = FedEx_ScanPostingService.BarcodeEntryType.MANUAL_ENTRY
                                        barcodestring.Value = pack.PackageID
                                        'evntpackdetail.Barcode = pack.PackageID
                                    End If
                                    barcode.StringBarcode = barcodestring
                                    packdetail.Barcodes = {barcode}

                                    ' this data is NOT needed if barcode is passed
                                    'If (evntpackdetail.BarcodeEntryType = BarcodeEntryType.MANUAL_ENTRY) Then
                                    '    Dim bcodedata As New PackageEventBarcodeData
                                    '    bcodedata.OperatingCompany = OperatingCompanyType.FEDEX_EXPRESS ' optional
                                    '    bcodedata.OperatingCompanySpecified = True
                                    '    bcodedata.TrackingNumber = pack.TrackingNo
                                    '    evntpackdetail.BarcodeData = bcodedata
                                    'End If
                                    '
                                    evntpackdetail.PackageEventDetail = packdetail
                                    '
                                    pkgArray(i) = evntpackdetail
                                    '
                                Next i
                                .Event = pkgArray
                                '
                                Dim capturedetail As New FedEx_ScanPostingService.EventCaptureDetail
                                capturedetail.LocationId = _FedExWeb.objFedEx_Setup.OriginLocationId ' FedEx location id assigned to this location like HKAA, OLVA etc…. For UTICA, NY, the location id is UCAA
                                capturedetail.LocationType = FedEx_ScanPostingService.FedExLocationType.FEDEX_AUTHORIZED_SHIP_CENTER
                                capturedetail.LocationTypeSpecified = True
                                capturedetail.Timestamp = Date.Now
                                capturedetail.TimestampSpecified = True
                                capturedetail.DeviceId = _FedExHAL.Get_MacAddress '"R00118688916" ' Should always be starting with “R” followed by Mac address encoded to 11 char string.
                                '
                                .EventCaptureDetail = capturedetail
                                '
                                Dim captureAgent As New FedEx_ScanPostingService.EventCapturingAgent
                                captureAgent.Id = _FedExWeb.objFedEx_Setup.ApplicationId ' emp# should be associated with location (FedEx should setup and provide this info)
                                captureAgent.Type = FedEx_ScanPostingService.EventCapturingAgentType.AUTHORIZED_AGENT
                                captureAgent.TypeSpecified = True
                                '
                                .EventCaptureDetail.Agent = captureAgent
                                '
                                Dim xdoc As New Xml.XmlDocument
                                xdoc.LoadXml(serializeShipRequest2string(webRequest))
                                Dim pack2 As baseWebResponse_Package = vb_response.Packages(0)
                                xdoc.Save(String.Format("{0}\PublishInventory_{1}_Request.xml", _FedExWeb.objFedEx_Setup.Path_SaveDocXML, pack2.TrackingNo))
                                '
                                Process_PublishInventoryEvent_Request = process_PublishInventoryEvent_Reply(webService, webRequest, vb_response)
                                '
                            End If
                            '
                        End If
                        '
                    End If
                    '
                End If
                '
            End With

        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to create a 'Publish Inventory' request...")
        End Try
    End Function
    Private Function process_PublishInventoryEvent_Reply(ByVal webService As FedEx_ScanPostingService.ScanPostingService, ByVal webRequest As FedEx_ScanPostingService.PublishInventoryEventRequest, ByRef vb_response As baseWebResponse_Shipment) As Boolean
        process_PublishInventoryEvent_Reply = False ' assume.
        Try
            System.Net.ServicePointManager.SecurityProtocol = Net.SecurityProtocolType.Tls12
            Dim webReply As FedEx_ScanPostingService.PublishInventoryEventReply = webService.publishInventoryEvent(webRequest)
            '
            Dim xdoc As New Xml.XmlDocument
            xdoc.LoadXml(serializeShipResponse2string(webReply))
            Dim pack2 As baseWebResponse_Package = vb_response.Packages(0)
            xdoc.Save(String.Format("{0}\PublishInventory_{1}_Reply.xml", _FedExWeb.objFedEx_Setup.Path_SaveDocXML, pack2.TrackingNo))
            '
            ' Result
            If webReply.Notifications IsNot Nothing Then
                Dim notify As FedEx_ScanPostingService.Notification = webReply.Notifications(0)
                ''ol#1.2.49(2/17)... FedEx HAL don't show WARNING, only ERROR
                '' If Not "0000" = notify.Code Then
                If notify.SeveritySpecified AndAlso (notify.Severity = FedEx_ScanPostingService.NotificationSeverityType.ERROR Or notify.Severity = FedEx_ScanPostingService.NotificationSeverityType.FAILURE) Then
                    vb_response.ShipmentAlerts.Add(notify.Message)
                ElseIf Not notify.SeveritySpecified AndAlso Not "0000" = notify.Code Then
                    vb_response.ShipmentAlerts.Add(notify.Message)
                End If
            End If
            '
            If webReply.Results IsNot Nothing Then
                For i As Integer = 0 To webReply.Results.Length - 1
                    Dim packresult As FedEx_ScanPostingService.PackageEventResult = webReply.Results(i)
                    Dim pack As baseWebResponse_Package = vb_response.Packages(i + 1)
                    If packresult.TrackingNumber IsNot Nothing Then
                        pack.TrackingNo = packresult.TrackingNumber
                    End If
                    If packresult.OperatingCompany.ToString IsNot Nothing Then
                        pack.ServiceCode = packresult.OperatingCompany.ToString
                    End If
                    If packresult.BarcodeHandlings IsNot Nothing Then
                        pack.LabelImage = packresult.BarcodeHandlings(0).ToString
                    End If
                Next
            End If
            '
            process_PublishInventoryEvent_Reply = True
            '
        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message) : _Debug.Print_(ex.Detail.LastChild.InnerText)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to get a response for 'Publish Inventory' request...")
        End Try
    End Function
    Private Function serializeShipRequest2string(obj As FedEx_ScanPostingService.PublishInventoryEventRequest) As String
        Dim xml_serializer As New XmlSerializer(GetType(FedEx_ScanPostingService.PublishInventoryEventRequest))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeShipRequest2string = string_writer.ToString()
        string_writer.Close()
    End Function
    Private Function serializeShipResponse2string(obj As FedEx_ScanPostingService.PublishInventoryEventReply) As String
        Dim xml_serializer As New XmlSerializer(GetType(FedEx_ScanPostingService.PublishInventoryEventReply))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeShipResponse2string = string_writer.ToString()
        string_writer.Close()
    End Function


#End Region


End Module
