Imports System.Xml.Serialization
Imports System.IO


Public Module _FedEx_OpenShip

#Region "Create Shipment"
    Public Function Process_CreateOpenShipment_Request(ByVal srSetup As Object, ByVal obj As Object, ByRef vb_response As Object) As Boolean
        Process_CreateOpenShipment_Request = False ' assume.
        Try
            Dim webservice As New FedEx_OpenShipService.OpenShipService
            webservice.Url = _FedExWeb.objFedEx_Setup.Connect2ServerURL '"https://ws.fedex.com:443/web-services"
            Dim shipService As New FedEx_OpenShipService.CreateOpenShipmentRequest

            Dim webauth As New FedEx_OpenShipService.WebAuthenticationDetail
            Dim webauth_csp As New FedEx_OpenShipService.WebAuthenticationCredential
            Dim webauth_user As New FedEx_OpenShipService.WebAuthenticationCredential
            If create_WebAuthenticationCredential(_FedExWeb.objFedEx_Setup.Web_CspCredential_Key, _FedExWeb.objFedEx_Setup.Web_CspCredential_Pass, webauth_csp) Then
                If create_WebAuthenticationCredential(_FedExWeb.objFedEx_Setup.Web_UserCredential_Key, _FedExWeb.objFedEx_Setup.Web_UserCredential_Pass, webauth_user) Then
                    webauth.ParentCredential = webauth_csp
                    webauth.UserCredential = webauth_user
                    shipService.WebAuthenticationDetail = webauth
                    '
                    Dim client As New FedEx_OpenShipService.ClientDetail
                    If create_ClientDetail(client) Then
                        shipService.ClientDetail = client
                        '
                        shipService.Index = obj.TrackingNo
                        shipService.Actions = {FedEx_OpenShipService.CreateOpenShipmentActionType.CREATE_PACKAGE, FedEx_OpenShipService.CreateOpenShipmentActionType.STRONG_VALIDATION}
                        '
                        Dim version As New FedEx_OpenShipService.VersionId
                        If create_Version("ship", 9, 0, 0, version) Then
                            shipService.Version = version
                            '
                            Dim shipRequest As New FedEx_OpenShipService.RequestedShipment
                            If create_RequestObject(srSetup, obj, shipRequest) Then
                                shipService.RequestedShipment = shipRequest
                                With (shipService.RequestedShipment)
                                    .PackageCount = obj.Packages.Count.ToString
                                    .TotalWeight = create_TotalWeight(obj)
                                    .TotalInsuredValue = create_TotalInsuredValue(obj)
                                    '
                                    For i As Integer = 0 To obj.Packages.Count - 1
                                        If i > 1 Then
                                            ' only 1st package's Tracking# in MPS shipment is the MasterTracking#
                                            '.MasterTrackingId = create_MasterTrackingID(vb_response.Packages(0).TrackingNo)
                                        End If
                                        .RequestedPackageLineItems = {create_RequestedPackageLineItem(obj, i)}
                                        '
                                        Dim trans As New FedEx_OpenShipService.TransactionDetail
                                        Dim srpack As Object = obj.Packages(i)
                                        trans.CustomerTransactionId = srpack.PackageID
                                        shipService.TransactionDetail = trans
                                        '
                                        If Not _Files.IsFileExist(String.Format("{0}\no_xml.txt", _FedExWeb.objFedEx_Setup.Path_SaveDocXML), False) Then
                                            Dim xdoc As New Xml.XmlDocument
                                            xdoc.LoadXml(serializeShipRequest2string(shipService))
                                            xdoc.Save(_FedExWeb.objFedEx_Setup.Path_SaveDocXML & "\" & obj.TrackingNo & "_RequestShipment.xml") ' shipment ID
                                        End If
                                        '
                                        Process_CreateOpenShipment_Request = process_CreateOpenShipment_Response(srSetup, shipService, webservice, i, vb_response)
                                        '
                                    Next i
                                End With
                                '
                            End If
                        End If
                    End If
                End If
            End If
            '
        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to create a CreateOpenShipment request...")
        End Try
    End Function
    Private Function process_CreateOpenShipment_Response(ByVal srSetup As Object, ByVal shipService As FedEx_OpenShipService.CreateOpenShipmentRequest, ByVal webservice As FedEx_OpenShipService.OpenShipService, ByVal pack_sequence As Integer, ByRef vb_response As Object) As Boolean
        process_CreateOpenShipment_Response = False ' assume.
        Try
            Dim shipResponse As FedEx_OpenShipService.CreateOpenShipmentReply = webservice.createOpenShipment(shipService)

            If Not _Files.IsFileExist(String.Format("{0}\no_xml.txt", _FedExWeb.objFedEx_Setup.Path_SaveDocXML), False) Then
                Dim xdoc As New Xml.XmlDocument
                xdoc.LoadXml(serializeShipResponse2string(shipResponse))
                xdoc.Save(_FedExWeb.objFedEx_Setup.Path_SaveDocXML & "\" & vb_response.ShipmentID & "_ReplyShipment.xml")
            End If
            process_CreateOpenShipment_Response = True ' got the response!

            ' Result
            If shipResponse.Notifications IsNot Nothing Then
                For n As Integer = 0 To shipResponse.Notifications.Length - 1
                    Dim notify As FedEx_OpenShipService.Notification = shipResponse.Notifications(n)
                    If Not notify.Code = "0000" Then
                        ' Error:
                        vb_response.ShipmentAlerts.Add(notify.Message)
                    End If
                Next n
            End If
            '
            If shipResponse.CompletedShipmentDetail IsNot Nothing Then
                Dim shipment As FedEx_OpenShipService.CompletedShipmentDetail = shipResponse.CompletedShipmentDetail
                '
                'Dim masterTrackID As TrackingId = shipment.MasterTrackingId
                'If masterTrackID IsNot Nothing Then
                '    ' not used for now...
                'End If
                Dim packages As FedEx_OpenShipService.CompletedPackageDetail() = shipment.CompletedPackageDetails
                If packages IsNot Nothing Then
                    For p As Integer = 0 To packages.Length - 1
                        Dim package As FedEx_OpenShipService.CompletedPackageDetail = packages(p)
                        ' Tracking Numbers:
                        If package.TrackingIds IsNot Nothing Then
                            For t As Integer = 0 To package.TrackingIds.Length - 1
                                vb_response.Packages(t + pack_sequence).TrackingNo = package.TrackingIds(t).TrackingNumber
                            Next t
                        End If
                    Next p
                End If
            End If
        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message) : _Debug.Print_(ex.Detail.LastChild.InnerText)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to get a response for CreateOpenShipment_Response request...")
        End Try
    End Function

    Private Function serializeShipRequest2string(obj As FedEx_OpenShipService.CreateOpenShipmentRequest) As String
        Dim xml_serializer As New XmlSerializer(GetType(FedEx_OpenShipService.CreateOpenShipmentRequest))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeShipRequest2string = string_writer.ToString()
        string_writer.Close()
    End Function
    Private Function serializeShipResponse2string(obj As FedEx_OpenShipService.CreateOpenShipmentReply) As String
        Dim xml_serializer As New XmlSerializer(GetType(FedEx_OpenShipService.CreateOpenShipmentReply))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeShipResponse2string = string_writer.ToString()
        string_writer.Close()
    End Function
    Private Function deserializeShipResponse2object(xmlsting As String) As FedEx_OpenShipService.CreateOpenShipmentReply
        Dim xml_serializer As New XmlSerializer(GetType(FedEx_OpenShipService.CreateOpenShipmentReply))
        Dim string_reader As New StringReader(xmlsting)
        deserializeShipResponse2object = DirectCast(xml_serializer.Deserialize(string_reader), FedEx_OpenShipService.CreateOpenShipmentReply)
        string_reader.Close()
    End Function
#End Region
#Region "Delete Shipment"
    Public Function Process_DeleteOpenShipment_Request(ByVal srSetup As Object, ByVal obj As Object, ByRef vb_response As Object) As Boolean
        Process_DeleteOpenShipment_Request = False ' assume.
        Try
            Dim webservice As New FedEx_OpenShipService.OpenShipService
            webservice.Url = _FedExWeb.objFedEx_Setup.Connect2ServerURL '"https://ws.fedex.com:443/web-services"
            Dim shipService As New FedEx_OpenShipService.DeleteOpenShipmentRequest

            Dim webauth As New FedEx_OpenShipService.WebAuthenticationDetail
            Dim webauth_csp As New FedEx_OpenShipService.WebAuthenticationCredential
            Dim webauth_user As New FedEx_OpenShipService.WebAuthenticationCredential
            If create_WebAuthenticationCredential(_FedExWeb.objFedEx_Setup.Web_CspCredential_Key, _FedExWeb.objFedEx_Setup.Web_CspCredential_Pass, webauth_csp) Then
                If create_WebAuthenticationCredential(_FedExWeb.objFedEx_Setup.Web_UserCredential_Key, _FedExWeb.objFedEx_Setup.Web_UserCredential_Pass, webauth_user) Then
                    webauth.ParentCredential = webauth_csp
                    webauth.UserCredential = webauth_user
                    shipService.WebAuthenticationDetail = webauth
                    '
                    Dim client As New FedEx_OpenShipService.ClientDetail
                    If create_ClientDetail(client) Then
                        shipService.ClientDetail = client
                        '
                        shipService.Index = obj.TrackingNo
                        '
                        Dim version As New FedEx_OpenShipService.VersionId
                        If create_Version("ship", 9, 0, 0, version) Then
                            shipService.Version = version
                            '
                            If Not _Files.IsFileExist(String.Format("{0}\no_xml.txt", _FedExWeb.objFedEx_Setup.Path_SaveDocXML), False) Then
                                Dim xdoc As New Xml.XmlDocument
                                xdoc.LoadXml(serializeDeleteShipmentRequest2string(shipService))
                                xdoc.Save(_FedExWeb.objFedEx_Setup.Path_SaveDocXML & "\" & obj.TrackingNo & "_RequestShipment.xml") ' shipment ID
                            End If
                            '
                            Process_DeleteOpenShipment_Request = process_DeleteOpenShipment_Response(srSetup, shipService, webservice, vb_response)
                            '
                        End If
                    End If
                End If
            End If
            '
        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to create a DeleteOpenShipment request...")
        End Try
    End Function
    Private Function process_DeleteOpenShipment_Response(ByVal srSetup As Object, ByVal shipService As FedEx_OpenShipService.DeleteOpenShipmentRequest, ByVal webservice As FedEx_OpenShipService.OpenShipService, ByRef vb_response As Object) As Boolean
        process_DeleteOpenShipment_Response = False ' assume.
        Try
            Dim shipResponse As FedEx_OpenShipService.DeleteOpenShipmentReply = webservice.deleteOpenShipment(shipService)

            If Not _Files.IsFileExist(String.Format("{0}\no_xml.txt", _FedExWeb.objFedEx_Setup.Path_SaveDocXML), False) Then
                Dim xdoc As New Xml.XmlDocument
                xdoc.LoadXml(serializeDeleteShipmentResponse2string(shipResponse))
                xdoc.Save(_FedExWeb.objFedEx_Setup.Path_SaveDocXML & "\" & vb_response.ShipmentID & "_ReplyShipment.xml")
            End If
            process_DeleteOpenShipment_Response = True ' got the response!

            ' Result
            If shipResponse.Notifications IsNot Nothing Then
                For n As Integer = 0 To shipResponse.Notifications.Length - 1
                    Dim notify As FedEx_OpenShipService.Notification = shipResponse.Notifications(n)
                    If Not notify.Code = "0000" Then
                        ' Error:
                        vb_response.ShipmentAlerts.Add(notify.Message)
                    End If
                Next n
            End If
            '
        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message) : _Debug.Print_(ex.Detail.LastChild.InnerText)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to get a response for DeleteOpenShipment request...")
        End Try
    End Function

    Private Function serializeDeleteShipmentRequest2string(obj As FedEx_OpenShipService.DeleteOpenShipmentRequest) As String
        Dim xml_serializer As New XmlSerializer(GetType(FedEx_OpenShipService.DeleteOpenShipmentRequest))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeDeleteShipmentRequest2string = string_writer.ToString()
        string_writer.Close()
    End Function
    Private Function serializeDeleteShipmentResponse2string(obj As FedEx_OpenShipService.DeleteOpenShipmentReply) As String
        Dim xml_serializer As New XmlSerializer(GetType(FedEx_OpenShipService.DeleteOpenShipmentReply))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeDeleteShipmentResponse2string = string_writer.ToString()
        string_writer.Close()
    End Function
#End Region
#Region "Confirm Shipment"
    Public Function Process_ConfirmOpenShipment_Request(ByVal srSetup As Object, ByVal obj As Object, ByRef vb_response As Object) As Boolean
        Process_ConfirmOpenShipment_Request = False ' assume.
        Try
            Dim webservice As New FedEx_OpenShipService.OpenShipService
            webservice.Url = _FedExWeb.objFedEx_Setup.Connect2ServerURL '"https://ws.fedex.com:443/web-services"
            Dim shipService As New FedEx_OpenShipService.ConfirmOpenShipmentRequest

            Dim webauth As New FedEx_OpenShipService.WebAuthenticationDetail
            Dim webauth_csp As New FedEx_OpenShipService.WebAuthenticationCredential
            Dim webauth_user As New FedEx_OpenShipService.WebAuthenticationCredential
            If create_WebAuthenticationCredential(_FedExWeb.objFedEx_Setup.Web_CspCredential_Key, _FedExWeb.objFedEx_Setup.Web_CspCredential_Pass, webauth_csp) Then
                If create_WebAuthenticationCredential(_FedExWeb.objFedEx_Setup.Web_UserCredential_Key, _FedExWeb.objFedEx_Setup.Web_UserCredential_Pass, webauth_user) Then
                    webauth.ParentCredential = webauth_csp
                    webauth.UserCredential = webauth_user
                    shipService.WebAuthenticationDetail = webauth
                    '
                    Dim client As New FedEx_OpenShipService.ClientDetail
                    If create_ClientDetail(client) Then
                        shipService.ClientDetail = client
                        '
                        shipService.Index = obj.TrackingNo
                        '
                        Dim version As New FedEx_OpenShipService.VersionId
                        If create_Version("ship", 9, 0, 0, version) Then
                            shipService.Version = version
                            '
                            'shipService.LabelSpecification = create_LabelSpecification()
                            'shipService.RateRequestTypes = {RateRequestType.ACCOUNT}
                            '
                            If Not _Files.IsFileExist(String.Format("{0}\no_xml.txt", _FedExWeb.objFedEx_Setup.Path_SaveDocXML), False) Then
                                Dim xdoc As New Xml.XmlDocument
                                xdoc.LoadXml(serializeShipRequest2string(shipService))
                                xdoc.Save(_FedExWeb.objFedEx_Setup.Path_SaveDocXML & "\" & obj.TrackingNo & "_RequestShipment.xml") ' shipment ID
                            End If
                            '
                            Process_ConfirmOpenShipment_Request = process_ConfirmOpenShipment_Response(srSetup, shipService, webservice, vb_response)
                            '
                        End If
                    End If
                End If
            End If
            '
        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to create a ConfirmOpenShipment request...")
        End Try
    End Function
    Private Function process_ConfirmOpenShipment_Response(ByVal srSetup As Object, ByVal shipService As FedEx_OpenShipService.ConfirmOpenShipmentRequest, ByVal webservice As FedEx_OpenShipService.OpenShipService, ByRef vb_response As Object) As Boolean
        process_ConfirmOpenShipment_Response = False ' assume.
        Try
            Dim shipResponse As FedEx_OpenShipService.ConfirmOpenShipmentReply = webservice.confirmOpenShipment(shipService)
            Dim pack_sequence As Integer = 1 ' 1 based collection

            If Not _Files.IsFileExist(String.Format("{0}\no_xml.txt", _FedExWeb.objFedEx_Setup.Path_SaveDocXML), False) Then
                Dim xdoc As New Xml.XmlDocument
                xdoc.LoadXml(serializeShipResponse2string(shipResponse))
                xdoc.Save(_FedExWeb.objFedEx_Setup.Path_SaveDocXML & "\" & vb_response.ShipmentID & "_ReplyShipment.xml")
            End If
            process_ConfirmOpenShipment_Response = True ' got the response!

            ' Result
            If shipResponse.Notifications IsNot Nothing Then
                For n As Integer = 0 To shipResponse.Notifications.Length - 1
                    Dim notify As FedEx_OpenShipService.Notification = shipResponse.Notifications(n)
                    If Not notify.Code = "0000" Then
                        ' Error:
                        vb_response.ShipmentAlerts.Add(notify.Message)
                    End If
                Next n
            End If
            '
            If shipResponse.CompletedShipmentDetail IsNot Nothing Then
                Dim shipment As FedEx_OpenShipService.CompletedShipmentDetail = shipResponse.CompletedShipmentDetail
                If shipment.OperationalDetail IsNot Nothing Then
                    ' Delivery dates:
                    Dim dates As FedEx_OpenShipService.ShipmentOperationalDetail = shipment.OperationalDetail
                    If Not IsNothing(dates.DeliveryDate) AndAlso dates.DeliveryDate > DateTime.Now.Date Then
                        vb_response.DeliveryDate = dates.DeliveryDate
                        vb_response.DeliveryDay = dates.DeliveryDay.ToString
                    ElseIf Not IsNothing(dates.TransitTime) Then
                        vb_response.DeliveryDay = dates.TransitTime.ToString
                    End If
                End If
                If Not IsNothing(shipment.AssociatedShipments) Then
                    ' COD lables at Shipment level:
                    For s As Integer = 0 To shipment.AssociatedShipments.Length - 1
                        Dim cod As FedEx_OpenShipService.AssociatedShipmentDetail = shipment.AssociatedShipments(s)
                        Dim codString As String = String.Empty
                        Dim codFileExt As String = FedEx_Data2XML.GetLabel_FileExtension(_FedExWeb.objFedEx_Setup.LabelImageType)
                        Dim codFile As String = _FedExWeb.objFedEx_Setup.Path_SaveDocXML & "\" & vb_response.Packages(s + 1).PackageID & "_labelCOD." & codFileExt
                        '
                        If Not IsNothing(cod.Label) Then
                            If _Files.WriteFile_ToEnd(cod.Label.Parts(s).Image, codFile) Then
                                If _Files.ReadFile_ToEnd(codFile, False, codString) Then
                                    vb_response.Packages(s + 1).LabelCODImage = codString
                                End If
                            End If
                        End If
                    Next s
                End If
                '
                Dim packages As FedEx_OpenShipService.CompletedPackageDetail() = shipment.CompletedPackageDetails
                If Not IsNothing(packages) Then
                    For p As Integer = 0 To packages.Length - 1
                        Dim package As FedEx_OpenShipService.CompletedPackageDetail = shipResponse.CompletedShipmentDetail.CompletedPackageDetails(p)
                        If package.TrackingIds IsNot Nothing Then
                            ' Tracking Number:
                            For t As Integer = 0 To package.TrackingIds.Length - 1
                                vb_response.Packages(t + p + 1).TrackingNo = package.TrackingIds(t).TrackingNumber
                            Next t
                            ' Shipping labels:
                            If package.Label IsNot Nothing AndAlso package.Label.Parts IsNot Nothing Then
                                For i As Integer = 0 To package.Label.Parts.Length - 1
                                    Dim labelString As String = String.Empty
                                    Dim labelFileExt As String = FedEx_Data2XML.GetLabel_FileExtension(_FedExWeb.objFedEx_Setup.LabelImageType)
                                    Dim labelFile As String = _FedExWeb.objFedEx_Setup.Path_SaveDocXML & "\" & vb_response.Packages(i + p + 1).PackageID & "_label." & labelFileExt
                                    '
                                    If _Files.WriteFile_ToEnd(package.Label.Parts(i).Image, labelFile) Then
                                        If _Files.ReadFile_ToEnd(labelFile, False, labelString) Then
                                            vb_response.Packages(i + p + 1).LabelImage = labelString
                                        End If
                                    End If
                                    If Not IsNothing(package.CodReturnDetail) Then
                                        If Not IsNothing(package.CodReturnDetail.Label) Then
                                            Dim cod As FedEx_OpenShipService.ShippingDocument = package.CodReturnDetail.Label
                                            If cod.Parts IsNot Nothing Then
                                                Dim codString As String = String.Empty
                                                Dim codFileExt As String = FedEx_Data2XML.GetLabel_FileExtension(_FedExWeb.objFedEx_Setup.LabelImageType)
                                                Dim codFile As String = _FedExWeb.objFedEx_Setup.Path_SaveDocXML & "\" & vb_response.Packages(i + p + 1).PackageID & "_labelCOD." & codFileExt
                                                If _Files.WriteFile_ToEnd(cod.Parts(i).Image, codFile) Then
                                                    If _Files.ReadFile_ToEnd(codFile, False, codString) Then
                                                        vb_response.Packages(i + p + 1).LabelCODImage = codString
                                                    End If
                                                End If
                                            End If
                                        End If
                                    End If
                                Next i
                            End If
                        End If
                    Next p
                End If
            End If
        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message) : _Debug.Print_(ex.Detail.LastChild.InnerText)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to get a response for ConfirmOpenShipment_Response request...")
        End Try
    End Function

    Private Function serializeShipRequest2string(obj As FedEx_OpenShipService.ConfirmOpenShipmentRequest) As String
        Dim xml_serializer As New XmlSerializer(GetType(FedEx_OpenShipService.ConfirmOpenShipmentRequest))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeShipRequest2string = string_writer.ToString()
        string_writer.Close()
    End Function
    Private Function serializeShipResponse2string(obj As FedEx_OpenShipService.ConfirmOpenShipmentReply) As String
        Dim xml_serializer As New XmlSerializer(GetType(FedEx_OpenShipService.ConfirmOpenShipmentReply))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeShipResponse2string = string_writer.ToString()
        string_writer.Close()
    End Function
#End Region

#Region "Add Package"
    Public Function Process_AddPackagesToOpenShipment(ByVal srSetup As Object, ByVal obj As Object, ByRef vb_response As Object) As Boolean
        Process_AddPackagesToOpenShipment = False ' assume.
        Try
            Dim webservice As New FedEx_OpenShipService.OpenShipService
            webservice.Url = _FedExWeb.objFedEx_Setup.Connect2ServerURL '"https://ws.fedex.com:443/web-services"
            Dim shipService As New FedEx_OpenShipService.AddPackagesToOpenShipmentRequest

            Dim webauth As New FedEx_OpenShipService.WebAuthenticationDetail
            Dim webauth_csp As New FedEx_OpenShipService.WebAuthenticationCredential
            Dim webauth_user As New FedEx_OpenShipService.WebAuthenticationCredential
            If create_WebAuthenticationCredential(_FedExWeb.objFedEx_Setup.Web_CspCredential_Key, _FedExWeb.objFedEx_Setup.Web_CspCredential_Pass, webauth_csp) Then
                If create_WebAuthenticationCredential(_FedExWeb.objFedEx_Setup.Web_UserCredential_Key, _FedExWeb.objFedEx_Setup.Web_UserCredential_Pass, webauth_user) Then
                    webauth.ParentCredential = webauth_csp
                    webauth.UserCredential = webauth_user
                    shipService.WebAuthenticationDetail = webauth
                    '
                    Dim client As New FedEx_OpenShipService.ClientDetail
                    If create_ClientDetail(client) Then
                        shipService.ClientDetail = client
                        '
                        shipService.Index = obj.TrackingNo
                        shipService.Actions = {FedEx_OpenShipService.AddPackagesToOpenShipmentActionType.STRONG_VALIDATION}
                        '
                        Dim version As New FedEx_OpenShipService.VersionId
                        If create_Version("ship", 9, 0, 0, version) Then
                            shipService.Version = version
                            '
                            With shipService
                                For i As Integer = 0 To obj.Packages.Count - 1
                                    If i > 1 Then
                                        ' only 1st package's Tracking# in MPS shipment is the MasterTracking#
                                        '.MasterTrackingId = create_MasterTrackingID(vb_response.Packages(0).TrackingNo)
                                    End If
                                    .RequestedPackageLineItems = {create_RequestedPackageLineItem(obj, i)}
                                    '
                                    Dim trans As New FedEx_OpenShipService.TransactionDetail
                                    Dim srpack As Object = obj.Packages(i)
                                    trans.CustomerTransactionId = srpack.PackageID
                                    shipService.TransactionDetail = trans
                                    '
                                    If Not _Files.IsFileExist(String.Format("{0}\no_xml.txt", _FedExWeb.objFedEx_Setup.Path_SaveDocXML), False) Then
                                        Dim xdoc As New Xml.XmlDocument
                                        xdoc.LoadXml(serializeShipRequest2string(shipService))
                                        xdoc.Save(_FedExWeb.objFedEx_Setup.Path_SaveDocXML & "\" & obj.TrackingNo & "_RequestShipment.xml") ' shipment ID
                                    End If
                                    '
                                    Process_AddPackagesToOpenShipment = process_AddPackagesToOpenShipment_Response(srSetup, shipService, webservice, i, vb_response)
                                    '
                                Next i
                            End With
                        End If
                    End If
                End If
            End If
            '
        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to process AddPackagesToOpenShipment request...")
        End Try
    End Function
    Private Function process_AddPackagesToOpenShipment_Response(ByVal srSetup As Object, ByVal shipService As FedEx_OpenShipService.AddPackagesToOpenShipmentRequest, ByVal webservice As FedEx_OpenShipService.OpenShipService, ByVal pack_sequence As Integer, ByRef vb_response As Object) As Boolean
        process_AddPackagesToOpenShipment_Response = False ' assume.
        Try
            Dim shipResponse As FedEx_OpenShipService.AddPackagesToOpenShipmentReply = webservice.addPackagesToOpenShipment(shipService)

            If Not _Files.IsFileExist(String.Format("{0}\no_xml.txt", _FedExWeb.objFedEx_Setup.Path_SaveDocXML), False) Then
                Dim xdoc As New Xml.XmlDocument
                xdoc.LoadXml(serializeShipResponse2string(shipResponse))
                xdoc.Save(_FedExWeb.objFedEx_Setup.Path_SaveDocXML & "\" & vb_response.ShipmentID & "_ReplyShipment.xml")
            End If
            process_AddPackagesToOpenShipment_Response = True ' got the response!

            ' Result
            If shipResponse.Notifications IsNot Nothing Then
                For n As Integer = 0 To shipResponse.Notifications.Length - 1
                    Dim notify As FedEx_OpenShipService.Notification = shipResponse.Notifications(n)
                    If Not notify.Code = "0000" Then
                        ' Error:
                        vb_response.ShipmentAlerts.Add(notify.Message)
                    End If
                Next n
            End If
            '
            If shipResponse.CompletedShipmentDetail IsNot Nothing Then
                Dim shipment As FedEx_OpenShipService.CompletedShipmentDetail = shipResponse.CompletedShipmentDetail
                Dim packages As FedEx_OpenShipService.CompletedPackageDetail() = shipment.CompletedPackageDetails
                If packages IsNot Nothing Then
                    For p As Integer = 0 To packages.Length - 1
                        Dim package As FedEx_OpenShipService.CompletedPackageDetail = shipResponse.CompletedShipmentDetail.CompletedPackageDetails(p)
                        ' Tracking Numbers:
                        If package.TrackingIds IsNot Nothing Then
                            For t As Integer = 0 To package.TrackingIds.Length - 1
                                vb_response.Packages(t + pack_sequence).TrackingNo = package.TrackingIds(t).TrackingNumber
                            Next t
                        End If
                    Next p
                End If
            End If
        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message) : _Debug.Print_(ex.Detail.LastChild.InnerText)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to process AddPackagesToOpenShipment response request...")
        End Try
    End Function

    Private Function serializeShipRequest2string(obj As FedEx_OpenShipService.AddPackagesToOpenShipmentRequest) As String
        Dim xml_serializer As New XmlSerializer(GetType(FedEx_OpenShipService.AddPackagesToOpenShipmentRequest))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeShipRequest2string = string_writer.ToString()
        string_writer.Close()
    End Function
    Private Function serializeShipResponse2string(obj As FedEx_OpenShipService.AddPackagesToOpenShipmentReply) As String
        Dim xml_serializer As New XmlSerializer(GetType(FedEx_OpenShipService.AddPackagesToOpenShipmentReply))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeShipResponse2string = string_writer.ToString()
        string_writer.Close()
    End Function
#End Region
#Region "Edit Package"
    Public Function Process_EditPackageInOpenShipment(ByVal srSetup As Object, ByVal obj As Object, ByRef vb_response As Object) As Boolean
        Process_EditPackageInOpenShipment = False ' assume.
        Try
            Dim webservice As New FedEx_OpenShipService.OpenShipService
            webservice.Url = _FedExWeb.objFedEx_Setup.Connect2ServerURL '"https://ws.fedex.com:443/web-services"
            Dim shipService As New FedEx_OpenShipService.ModifyPackageInOpenShipmentRequest

            Dim webauth As New FedEx_OpenShipService.WebAuthenticationDetail
            Dim webauth_csp As New FedEx_OpenShipService.WebAuthenticationCredential
            Dim webauth_user As New FedEx_OpenShipService.WebAuthenticationCredential
            If create_WebAuthenticationCredential(_FedExWeb.objFedEx_Setup.Web_CspCredential_Key, _FedExWeb.objFedEx_Setup.Web_CspCredential_Pass, webauth_csp) Then
                If create_WebAuthenticationCredential(_FedExWeb.objFedEx_Setup.Web_UserCredential_Key, _FedExWeb.objFedEx_Setup.Web_UserCredential_Pass, webauth_user) Then
                    webauth.ParentCredential = webauth_csp
                    webauth.UserCredential = webauth_user
                    shipService.WebAuthenticationDetail = webauth
                    '
                    Dim client As New FedEx_OpenShipService.ClientDetail
                    If create_ClientDetail(client) Then
                        shipService.ClientDetail = client
                        '
                        shipService.Index = obj.TrackingNo
                        shipService.Actions = {FedEx_OpenShipService.ModifyPackageInOpenShipmentActionType.STRONG_VALIDATION}
                        '
                        Dim version As New FedEx_OpenShipService.VersionId
                        If create_Version("ship", 9, 0, 0, version) Then
                            shipService.Version = version
                            '
                            With shipService
                                For i As Integer = 0 To obj.Packages.Count - 1
                                    ' Only one Package at a time:
                                    .RequestedPackageLineItem = create_RequestedPackageLineItem(obj, i)
                                    '
                                    Dim srpack As Object = obj.Packages(i)
                                    '
                                    Dim trackid As New FedEx_OpenShipService.TrackingId
                                    trackid.TrackingIdType = FedEx_OpenShipService.TrackingIdType.FEDEX
                                    trackid.TrackingIdTypeSpecified = True
                                    trackid.TrackingNumber = srpack.TrackingNo
                                    .TrackingId = trackid
                                    '
                                    Dim trans As New FedEx_OpenShipService.TransactionDetail
                                    trans.CustomerTransactionId = srpack.PackageID
                                    .TransactionDetail = trans
                                    '
                                    If Not _Files.IsFileExist(String.Format("{0}\no_xml.txt", _FedExWeb.objFedEx_Setup.Path_SaveDocXML), False) Then
                                        Dim xdoc As New Xml.XmlDocument
                                        xdoc.LoadXml(serializeShipRequest2string(shipService))
                                        xdoc.Save(_FedExWeb.objFedEx_Setup.Path_SaveDocXML & "\" & obj.TrackingNo & "_RequestShipment.xml") ' shipment ID
                                    End If
                                    '
                                    Process_EditPackageInOpenShipment = process_EditPackageInOpenShipment_Response(srSetup, shipService, webservice, i, vb_response)
                                    '
                                Next i
                            End With
                        End If
                    End If
                End If
            End If
            '
        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to process ModifyPackageInOpenShipment request...")
        End Try
    End Function
    Private Function process_EditPackageInOpenShipment_Response(ByVal srSetup As Object, ByVal shipService As FedEx_OpenShipService.ModifyPackageInOpenShipmentRequest, ByVal webservice As FedEx_OpenShipService.OpenShipService, ByVal pack_sequence As Integer, ByRef vb_response As Object) As Boolean
        process_EditPackageInOpenShipment_Response = False ' assume.
        Try
            Dim shipResponse As FedEx_OpenShipService.ModifyPackageInOpenShipmentReply = webservice.modifyPackageInOpenShipment(shipService)

            If Not _Files.IsFileExist(String.Format("{0}\no_xml.txt", _FedExWeb.objFedEx_Setup.Path_SaveDocXML), False) Then
                Dim xdoc As New Xml.XmlDocument
                xdoc.LoadXml(serializeShipResponse2string(shipResponse))
                xdoc.Save(_FedExWeb.objFedEx_Setup.Path_SaveDocXML & "\" & vb_response.ShipmentID & "_ReplyShipment.xml")
            End If
            process_EditPackageInOpenShipment_Response = True ' got the response!

            ' Result
            If shipResponse.Notifications IsNot Nothing Then
                For n As Integer = 0 To shipResponse.Notifications.Length - 1
                    Dim notify As FedEx_OpenShipService.Notification = shipResponse.Notifications(n)
                    If Not notify.Code = "0000" Then
                        ' Error:
                        vb_response.ShipmentAlerts.Add(notify.Message)
                    End If
                Next n
            End If
            '
            If shipResponse.CompletedShipmentDetail IsNot Nothing Then
                Dim shipment As FedEx_OpenShipService.CompletedShipmentDetail = shipResponse.CompletedShipmentDetail
                Dim packages As FedEx_OpenShipService.CompletedPackageDetail() = shipment.CompletedPackageDetails
                If packages IsNot Nothing Then
                    For p As Integer = 0 To packages.Length - 1
                        Dim package As FedEx_OpenShipService.CompletedPackageDetail = shipResponse.CompletedShipmentDetail.CompletedPackageDetails(p)
                        ' Tracking Numbers:
                        If package.TrackingIds IsNot Nothing Then
                            For t As Integer = 0 To package.TrackingIds.Length - 1
                                vb_response.Packages(t + pack_sequence).TrackingNo = package.TrackingIds(t).TrackingNumber
                            Next t
                        End If
                    Next p
                End If
            End If
        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message) : _Debug.Print_(ex.Detail.LastChild.InnerText)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to process ModifyPackageInOpenShipment response request...")
        End Try
    End Function

    Private Function serializeShipRequest2string(obj As FedEx_OpenShipService.ModifyPackageInOpenShipmentRequest) As String
        Dim xml_serializer As New XmlSerializer(GetType(FedEx_OpenShipService.ModifyPackageInOpenShipmentRequest))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeShipRequest2string = string_writer.ToString()
        string_writer.Close()
    End Function
    Private Function serializeShipResponse2string(obj As FedEx_OpenShipService.ModifyPackageInOpenShipmentReply) As String
        Dim xml_serializer As New XmlSerializer(GetType(FedEx_OpenShipService.ModifyPackageInOpenShipmentReply))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeShipResponse2string = string_writer.ToString()
        string_writer.Close()
    End Function
#End Region
#Region "Delete Pakage"
    Public Function Process_DeletePackagesFromOpenShipment(ByVal srSetup As Object, ByVal obj As Object, ByRef vb_response As Object) As Boolean
        Process_DeletePackagesFromOpenShipment = False ' assume.
        Try
            Dim webservice As New FedEx_OpenShipService.OpenShipService
            webservice.Url = _FedExWeb.objFedEx_Setup.Connect2ServerURL '"https://ws.fedex.com:443/web-services"
            Dim shipService As New FedEx_OpenShipService.DeletePackagesFromOpenShipmentRequest

            Dim webauth As New FedEx_OpenShipService.WebAuthenticationDetail
            Dim webauth_csp As New FedEx_OpenShipService.WebAuthenticationCredential
            Dim webauth_user As New FedEx_OpenShipService.WebAuthenticationCredential
            If create_WebAuthenticationCredential(_FedExWeb.objFedEx_Setup.Web_CspCredential_Key, _FedExWeb.objFedEx_Setup.Web_CspCredential_Pass, webauth_csp) Then
                If create_WebAuthenticationCredential(_FedExWeb.objFedEx_Setup.Web_UserCredential_Key, _FedExWeb.objFedEx_Setup.Web_UserCredential_Pass, webauth_user) Then
                    webauth.ParentCredential = webauth_csp
                    webauth.UserCredential = webauth_user
                    shipService.WebAuthenticationDetail = webauth
                    '
                    Dim client As New FedEx_OpenShipService.ClientDetail
                    If create_ClientDetail(client) Then
                        shipService.ClientDetail = client
                        '
                        shipService.Index = obj.TrackingNo
                        '
                        Dim version As New FedEx_OpenShipService.VersionId
                        If create_Version("ship", 9, 0, 0, version) Then
                            shipService.Version = version
                            '
                            With (shipService)
                                For i As Integer = 0 To obj.Packages.Count - 1
                                    Dim srpack As Object = obj.Packages(i)
                                    '
                                    Dim trackid As New FedEx_OpenShipService.TrackingId
                                    trackid.TrackingIdType = FedEx_OpenShipService.TrackingIdType.FEDEX
                                    trackid.TrackingIdTypeSpecified = True
                                    trackid.TrackingNumber = srpack.TrackingNo
                                    .TrackingIds = {trackid}
                                    '
                                    Dim trans As New FedEx_OpenShipService.TransactionDetail
                                    trans.CustomerTransactionId = srpack.PackageID
                                    .TransactionDetail = trans
                                    '

                                    If Not _Files.IsFileExist(String.Format("{0}\no_xml.txt", _FedExWeb.objFedEx_Setup.Path_SaveDocXML), False) Then
                                        Dim xdoc As New Xml.XmlDocument
                                        xdoc.LoadXml(serializeShipRequest2string(shipService))
                                        xdoc.Save(_FedExWeb.objFedEx_Setup.Path_SaveDocXML & "\" & obj.TrackingNo & "_RequestShipment.xml") ' shipment ID
                                    End If
                                    '
                                    Process_DeletePackagesFromOpenShipment = process_EditPackageInOpenShipment_Response(srSetup, shipService, webservice, i, vb_response)
                                    '
                                Next i
                            End With
                        End If
                    End If
                End If
            End If
            '
        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to process DeletePackagesFromOpenShipment request...")
        End Try
    End Function
    Private Function process_EditPackageInOpenShipment_Response(ByVal srSetup As Object, ByVal shipService As FedEx_OpenShipService.DeletePackagesFromOpenShipmentRequest, ByVal webservice As FedEx_OpenShipService.OpenShipService, ByVal pack_sequence As Integer, ByRef vb_response As Object) As Boolean
        process_EditPackageInOpenShipment_Response = False ' assume.
        Try
            Dim shipResponse As FedEx_OpenShipService.DeletePackagesFromOpenShipmentReply = webservice.deletePackagesFromOpenShipment(shipService)

            If Not _Files.IsFileExist(String.Format("{0}\no_xml.txt", _FedExWeb.objFedEx_Setup.Path_SaveDocXML), False) Then
                Dim xdoc As New Xml.XmlDocument
                xdoc.LoadXml(serializeShipResponse2string(shipResponse))
                xdoc.Save(_FedExWeb.objFedEx_Setup.Path_SaveDocXML & "\" & vb_response.ShipmentID & "_ReplyShipment.xml")
            End If
            process_EditPackageInOpenShipment_Response = True ' got the response!

            ' Result
            If shipResponse.Notifications IsNot Nothing Then
                For n As Integer = 0 To shipResponse.Notifications.Length - 1
                    Dim notify As FedEx_OpenShipService.Notification = shipResponse.Notifications(n)
                    If Not notify.Code = "0000" Then
                        ' Error:
                        vb_response.ShipmentAlerts.Add(notify.Message)
                    End If
                Next n
            End If
            '
        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message) : _Debug.Print_(ex.Detail.LastChild.InnerText)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to process DeletePackagesFromOpenShipment response request...")
        End Try
    End Function

    Private Function serializeShipRequest2string(obj As FedEx_OpenShipService.DeletePackagesFromOpenShipmentRequest) As String
        Dim xml_serializer As New XmlSerializer(GetType(FedEx_OpenShipService.DeletePackagesFromOpenShipmentRequest))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeShipRequest2string = string_writer.ToString()
        string_writer.Close()
    End Function
    Private Function serializeShipResponse2string(obj As FedEx_OpenShipService.DeletePackagesFromOpenShipmentReply) As String
        Dim xml_serializer As New XmlSerializer(GetType(FedEx_OpenShipService.DeletePackagesFromOpenShipmentReply))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeShipResponse2string = string_writer.ToString()
        string_writer.Close()
    End Function
#End Region

#Region "Create Objects"
    Private Function create_RequestObject(ByVal srSetup As Object, ByVal obj As _baseShipment, ByRef shipRequest As FedEx_OpenShipService.RequestedShipment) As Boolean
        create_RequestObject = False ' assume.
        With shipRequest
            .ShipTimestamp = obj.CarrierService.ShipDate
            .ShipTimestampSpecified = True
            .DropoffType = FedEx_OpenShipService.DropoffType.REGULAR_PICKUP
            .DropoffTypeSpecified = True
            .ServiceType = GetServiceType(obj.CarrierService.ServiceABBR)
            .ServiceTypeSpecified = True
            Dim package As _baseShipmentPackage = obj.Packages(0)
            .PackagingType = GetPackagingType(package.PackagingType)
            .PackagingTypeSpecified = True
            .PreferredCurrency = package.Currency_Type

            .Shipper = create_ContactParty_ShipService(obj.ShipFromContact, _FedExWeb.objFedEx_Setup.Csp_AccountNumber)
            .Recipient = create_ContactParty_ShipService(obj.ShipToContact, String.Empty)
            .ShippingChargesPayment = create_Payment(_FedExWeb.objFedEx_Setup.PaymentType, obj)
            .SpecialServicesRequested = create_ShipmentSpecialServices(obj)
            .LabelSpecification = create_LabelSpecification()
            .RateRequestTypes = {FedEx_OpenShipService.RateRequestType.LIST}
            '
            'International:
            If Not obj.CarrierService.IsDomestic Then
                .CustomsClearanceDetail = create_CustomsClearanceDetail(obj)
            End If
            '
            create_RequestObject = True
        End With
    End Function

    Private Function create_ContactParty_ShipService(ByVal obj As _baseContact, ByVal accountNo As String) As FedEx_OpenShipService.Party
        create_ContactParty_ShipService = New FedEx_OpenShipService.Party ' assume
        Dim address As New FedEx_OpenShipService.Address
        Dim contact As New FedEx_OpenShipService.Contact
        With create_ContactParty_ShipService
            If Not accountNo = String.Empty Then
                .AccountNumber = accountNo
            End If
            If create_Address(obj, address) Then
                .Address = address
            End If
            If create_Contact(obj, contact) Then
                .Contact = contact
            End If
            '.Shipper.Tins
        End With
    End Function
    Private Function create_Payment(ByVal type As String, ByVal obj As Object) As FedEx_OpenShipService.Payment
        create_Payment = New FedEx_OpenShipService.Payment
        With create_Payment
            Dim contact As New _baseContact
            .PaymentType = GetPaymentType(type, obj, contact)
            .PaymentTypeSpecified = True
            Dim accountNo As String = String.Empty
            If .PaymentType = FedEx_OpenShipService.PaymentType.SENDER Or .PaymentType = FedEx_OpenShipService.PaymentType.THIRD_PARTY Or type = "RECIPIENT-TEST-ONLY" Then
                accountNo = _FedExWeb.objFedEx_Setup.Client_AccountNumber
                .Payor = create_Payor(contact, accountNo)
            End If
        End With
    End Function
    Private Function create_Payor(ByVal obj As _baseContact, ByVal accountNo As String) As FedEx_OpenShipService.Payor
        create_Payor = New FedEx_OpenShipService.Payor
        With create_Payor
            .ResponsibleParty = create_ContactParty_ShipService(obj, accountNo)
        End With
    End Function
    Private Function create_ShipmentSpecialServices(ByVal obj As Object) As FedEx_OpenShipService.ShipmentSpecialServicesRequested
        create_ShipmentSpecialServices = New FedEx_OpenShipService.ShipmentSpecialServicesRequested
        With create_ShipmentSpecialServices
            If obj.CarrierService.ServiceSurcharges IsNot Nothing AndAlso 0 < obj.CarrierService.ServiceSurcharges.Count Then
                Dim type(obj.CarrierService.ServiceSurcharges.Count - 1) As FedEx_OpenShipService.ShipmentSpecialServiceType
                For i As Integer = 0 To obj.CarrierService.ServiceSurcharges.Count - 1
                    Dim objServiceSurcharge As _baseServiceSurcharge = obj.CarrierService.ServiceSurcharges(i)
                    If objServiceSurcharge IsNot Nothing Then
                        If objServiceSurcharge.IsToShow Then
                            If GetShipmentSpecialServiceType(objServiceSurcharge.Name, type(i)) Then
                                Select Case type(i)
                                    Case FedEx_OpenShipService.ShipmentSpecialServiceType.EMAIL_NOTIFICATION
                                        .EMailNotificationDetail = create_EMailNotificationDetail(obj)
                                    Case FedEx_OpenShipService.ShipmentSpecialServiceType.COD
                                        Dim cod As _baseServiceSurchargeCOD = obj.CarrierService.ServiceSurchargeCOD
                                        .CodDetail = create_CodDetail(cod, obj.ShipperContact)
                                    Case FedEx_OpenShipService.ShipmentSpecialServiceType.HOLD_AT_LOCATION
                                        .HoldAtLocationDetail = create_HoldAtLocationDetail(obj.HoldAtLocation)
                                    Case FedEx_OpenShipService.ShipmentSpecialServiceType.HOME_DELIVERY_PREMIUM
                                        .HomeDeliveryPremiumDetail = create_HomeDeliveryPremiumDetail(obj, GetHomeDeliveryPremiumType(objServiceSurcharge.Description))
                                    Case FedEx_OpenShipService.ShipmentSpecialServiceType.DRY_ICE
                                    ' dry ice is at the Package level

                                    ' don't require details:
                                    'Case ShipmentSpecialServiceType.FEDEX_ONE_RATE
                                    Case FedEx_OpenShipService.ShipmentSpecialServiceType.FUTURE_DAY_SHIPMENT
                                    Case FedEx_OpenShipService.ShipmentSpecialServiceType.SATURDAY_DELIVERY
                                    Case FedEx_OpenShipService.ShipmentSpecialServiceType.SATURDAY_PICKUP
                                    Case FedEx_OpenShipService.ShipmentSpecialServiceType.INSIDE_DELIVERY
                                    Case FedEx_OpenShipService.ShipmentSpecialServiceType.INSIDE_PICKUP
                                End Select
                            End If
                        End If
                    End If
                Next i
                .SpecialServiceTypes = type
            End If
        End With
    End Function

    Private Function create_CodDetail(ByVal cod As _baseServiceSurchargeCOD, ByVal contact As _baseContact) As FedEx_OpenShipService.CodDetail
        create_CodDetail = New FedEx_OpenShipService.CodDetail
        With create_CodDetail
            If cod.AddCOD2Total Then
                .AddTransportationChargesDetail = create_CodAddTransportationChargesDetail(cod)
            End If
            .CollectionType = GetCodCollectionType(cod.PaymentType)
            .CollectionTypeSpecified = True
            .CodCollectionAmount = create_Money(cod.Amount, cod.CurrencyType)
            .CodRecipient = create_ContactParty_ShipService(contact, String.Empty)
        End With
    End Function
    Private Function create_CodAddTransportationChargesDetail(ByVal cod As _baseServiceSurchargeCOD) As FedEx_OpenShipService.CodAddTransportationChargesDetail
        create_CodAddTransportationChargesDetail = New FedEx_OpenShipService.CodAddTransportationChargesDetail
        With create_CodAddTransportationChargesDetail
            .ChargeBasisSpecified = True
            .ChargeBasis = GetCodAddTransportationChargesType(cod.ChargeType)
            .ChargeBasisLevelSpecified = True
            .ChargeBasisLevel = FedEx_OpenShipService.ChargeBasisLevelType.CURRENT_PACKAGE
            .RateTypeBasisSpecified = True
            .RateTypeBasis = FedEx_OpenShipService.RateTypeBasisType.ACCOUNT
        End With
    End Function

    Private Function create_Money(ByVal amount As Decimal, currencytype As String) As FedEx_OpenShipService.Money
        create_Money = New FedEx_OpenShipService.Money
        With create_Money
            .Amount = amount
            .Currency = currencytype
            .AmountSpecified = True
        End With
    End Function
    Private Function create_EMailNotificationDetail(ByVal obj As Object) As FedEx_OpenShipService.EMailNotificationDetail
        ' The descriptive data required for FedEx to provide email notification to the customer regarding the shipment.
        create_EMailNotificationDetail = New FedEx_OpenShipService.EMailNotificationDetail
        With create_EMailNotificationDetail
            .PersonalMessage = _Controls.Left(obj.Comments, 120)
            If Not 0 = obj.ShipFromContact.Email.Length And Not 0 = obj.ShipToContact.Email.Length Then
                .Recipients = {create_EMailNotificationRecipient(obj.ShipFromContact, FedEx_OpenShipService.EMailNotificationRecipientType.SHIPPER), create_EMailNotificationRecipient(obj.ShipToContact, FedEx_OpenShipService.EMailNotificationRecipientType.RECIPIENT)}
            ElseIf Not 0 = obj.ShipFromContact.Email.Length Then
                .Recipients = {create_EMailNotificationRecipient(obj.ShipFromContact, FedEx_OpenShipService.EMailNotificationRecipientType.SHIPPER)}
            ElseIf Not 0 = obj.ShipToContact.Email.Length Then
                .Recipients = {create_EMailNotificationRecipient(obj.ShipToContact, FedEx_OpenShipService.EMailNotificationRecipientType.RECIPIENT)}
            End If
        End With
    End Function
    Private Function create_EMailNotificationRecipient(ByVal obj As _baseContact, ByVal type As FedEx_OpenShipService.EMailNotificationRecipientType) As FedEx_OpenShipService.EMailNotificationRecipient
        create_EMailNotificationRecipient = New FedEx_OpenShipService.EMailNotificationRecipient
        With create_EMailNotificationRecipient
            .EMailAddress = obj.Email
            .EMailNotificationRecipientType = type
            .EMailNotificationRecipientTypeSpecified = True
            .Format = FedEx_OpenShipService.EMailNotificationFormatType.TEXT
            .FormatSpecified = True
            .NotificationEventsRequested = {FedEx_OpenShipService.EMailNotificationEventType.ON_DELIVERY, FedEx_OpenShipService.EMailNotificationEventType.ON_EXCEPTION, FedEx_OpenShipService.EMailNotificationEventType.ON_SHIPMENT}
            '.NotificationEventsRequested = {EMailNotificationEventType.ON_EXCEPTION}
            '.NotificationEventsRequested = {EMailNotificationEventType.ON_SHIPMENT}
            .Localization = New FedEx_OpenShipService.Localization
            .Localization.LanguageCode = "EN"
        End With
    End Function
    Private Function create_HomeDeliveryPremiumDetail(ByVal obj As Object, ByVal type As FedEx_OpenShipService.HomeDeliveryPremiumType) As FedEx_OpenShipService.HomeDeliveryPremiumDetail
        create_HomeDeliveryPremiumDetail = New FedEx_OpenShipService.HomeDeliveryPremiumDetail
        With create_HomeDeliveryPremiumDetail
            .Date = obj.CarrierService.DeliveryDate
            .DateSpecified = True
            .PhoneNumber = obj.ShipToContact.Tel
            .HomeDeliveryPremiumType = type
            .HomeDeliveryPremiumTypeSpecified = True
        End With
    End Function
    Private Function create_LabelSpecification() As FedEx_OpenShipService.LabelSpecification
        create_LabelSpecification = New FedEx_OpenShipService.LabelSpecification
        With create_LabelSpecification
            .LabelFormatType = FedEx_OpenShipService.LabelFormatType.COMMON2D 'FedEx_Data2XML.LabelFormatType
            .LabelFormatTypeSpecified = True
            .ImageTypeSpecified = True
            If _Controls.Contains(_FedExWeb.objFedEx_Setup.LabelImageType, "Thermal") Then
                .ImageType = FedEx_OpenShipService.ShippingDocumentImageType.EPL2
                .LabelStockType = FedEx_OpenShipService.LabelStockType.STOCK_4X6
                .LabelStockTypeSpecified = True
                .LabelPrintingOrientation = FedEx_OpenShipService.LabelPrintingOrientationType.BOTTOM_EDGE_OF_TEXT_FIRST
                .LabelPrintingOrientationSpecified = True
            Else
                .ImageType = FedEx_OpenShipService.ShippingDocumentImageType.PDF
            End If
            .CustomerSpecifiedDetail = create_CustomerSpecifiedLabelDetail()
        End With
    End Function
    Private Function create_CustomerSpecifiedLabelDetail() As FedEx_OpenShipService.CustomerSpecifiedLabelDetail
        create_CustomerSpecifiedLabelDetail = New FedEx_OpenShipService.CustomerSpecifiedLabelDetail
        With create_CustomerSpecifiedLabelDetail
            .MaskedData = {FedEx_OpenShipService.LabelMaskableDataType.SHIPPER_ACCOUNT_NUMBER}
        End With
    End Function
    Private Function create_HoldAtLocationDetail(ByVal obj As _baseContact) As FedEx_OpenShipService.HoldAtLocationDetail
        Dim address As New FedEx_OpenShipService.Address
        Dim contact As New FedEx_OpenShipService.Contact
        create_HoldAtLocationDetail = New FedEx_OpenShipService.HoldAtLocationDetail
        With create_HoldAtLocationDetail
            .LocationContactAndAddress = New FedEx_OpenShipService.ContactAndAddress
            If create_Contact(obj, contact) Then
                .LocationContactAndAddress.Contact = contact
            End If
            If create_Address(obj, address) Then
                .LocationContactAndAddress.Address = address
            End If
            .PhoneNumber = obj.Tel
            '' ''ol#1.2.33(3/28)... FedEx Cert: One Rate HAL is missing Location Type tag.
            ''.LocationType = FedExLocationType.FEDEX_EXPRESS_STATION
            ''.LocationTypeSpecified = True
        End With
    End Function

    Private Function create_MasterTrackingID(ByVal masterTrackingNo As String) As FedEx_OpenShipService.TrackingId
        create_MasterTrackingID = New FedEx_OpenShipService.TrackingId
        With create_MasterTrackingID
            .TrackingIdType = FedEx_OpenShipService.TrackingIdType.FEDEX
            .TrackingIdTypeSpecified = True
            '.FormId = String.Empty
            .TrackingNumber = masterTrackingNo
        End With
    End Function
    Private Function create_RequestedPackageLineItem(ByVal shipment As Object, ByVal siquenceno As Integer) As FedEx_OpenShipService.RequestedPackageLineItem
        create_RequestedPackageLineItem = New FedEx_OpenShipService.RequestedPackageLineItem
        Dim package As _baseShipmentPackage = shipment.Packages(siquenceno)
        With create_RequestedPackageLineItem
            .InsuredValue = create_Money(package.DeclaredValue, package.Currency_Type)
            .SequenceNumber = package.SequenceNo.ToString
            .Weight = create_Weight(package.Weight_LBs, package.Weight_Units)
            .Dimensions = create_Dimensions(package)
            .CustomerReferences = {create_CustomerReference(package)}
            .SpecialServicesRequested = create_PackageSpecialServices(package, shipment.ShipperContact)
        End With
    End Function
    Private Function create_PackageSpecialServices(ByVal obj As _baseShipmentPackage, ByVal codContact As _baseContact) As FedEx_OpenShipService.PackageSpecialServicesRequested
        create_PackageSpecialServices = New FedEx_OpenShipService.PackageSpecialServicesRequested
        With create_PackageSpecialServices
            If obj.ServiceSurcharges IsNot Nothing AndAlso 0 < obj.ServiceSurcharges.Count Then
                Dim type(obj.ServiceSurcharges.Count - 1) As FedEx_OpenShipService.PackageSpecialServiceType
                For i As Integer = 0 To obj.ServiceSurcharges.Count - 1
                    Dim objServiceSurcharge As _baseServiceSurcharge = obj.ServiceSurcharges(i)
                    If objServiceSurcharge IsNot Nothing Then
                        If objServiceSurcharge.IsToShow Then
                            If GetPackageSpecialServiceType(objServiceSurcharge.Name, type(i)) Then
                                Select Case type(i)
                                    Case FedEx_OpenShipService.PackageSpecialServiceType.SIGNATURE_OPTION
                                        .SignatureOptionDetail = create_SignatureOptionDetail(objServiceSurcharge)
                                    Case FedEx_OpenShipService.PackageSpecialServiceType.DRY_ICE
                                        .DryIceWeight = create_Weight(obj.DryIce.Weight, obj.DryIce.WeightUnits)
                                    Case FedEx_OpenShipService.PackageSpecialServiceType.NON_STANDARD_CONTAINER
                                    ' nothing required - just a flag
                                    Case FedEx_OpenShipService.PackageSpecialServiceType.COD
                                        .CodDetail = create_CodDetail(obj.COD, codContact)
                                End Select
                            End If
                        End If
                    End If
                Next i
                .SpecialServiceTypes = type
            End If
        End With
    End Function

    Private Function create_TotalInsuredValue(ByVal shipment As Object) As FedEx_OpenShipService.Money
        create_TotalInsuredValue = New FedEx_OpenShipService.Money
        With create_TotalInsuredValue
            .Currency = shipment.Packages(0).Currency_Type
            For i As Integer = 0 To shipment.Packages.Count - 1
                Dim package As _baseShipmentPackage = shipment.Packages(i)
                .Amount += package.DeclaredValue
            Next i
            .AmountSpecified = True
        End With
    End Function
    Private Function create_TotalWeight(ByVal shipment As Object) As FedEx_OpenShipService.Weight
        create_TotalWeight = New FedEx_OpenShipService.Weight
        If shipment.Packages.Count > 0 Then
            With create_TotalWeight
                If "KG" = shipment.Packages(0).Weight_Units.ToUpper Then
                    .Units = FedEx_OpenShipService.WeightUnits.KG
                Else
                    .Units = FedEx_OpenShipService.WeightUnits.LB
                End If
                .UnitsSpecified = True
                For i As Integer = 0 To shipment.Packages.Count - 1
                    Dim package As _baseShipmentPackage = shipment.Packages(i)
                    .Value += package.Weight_LBs
                Next i
                .ValueSpecified = True
            End With
        End If
    End Function
    Private Function create_Weight(ByVal weight As Double, ByVal units As String) As FedEx_OpenShipService.Weight
        create_Weight = New FedEx_OpenShipService.Weight
        With create_Weight
            If "KG" = units.ToUpper Then
                .Units = FedEx_OpenShipService.WeightUnits.KG
            Else
                .Units = FedEx_OpenShipService.WeightUnits.LB
            End If
            .Value = weight
            .UnitsSpecified = True
            .ValueSpecified = True
        End With
    End Function
    Private Function create_Dimensions(ByVal package As _baseShipmentPackage) As FedEx_OpenShipService.Dimensions
        create_Dimensions = New FedEx_OpenShipService.Dimensions
        With create_Dimensions
            .Length = package.Dim_Length.ToString
            .Width = package.Dim_Width.ToString
            .Height = package.Dim_Height.ToString
            If "CM" = package.Dim_Units.ToUpper Then
                .Units = FedEx_OpenShipService.LinearUnits.CM
            Else
                .Units = FedEx_OpenShipService.LinearUnits.IN
            End If
            .UnitsSpecified = True
        End With
    End Function
    Private Function create_CustomerReference(ByVal package As _baseShipmentPackage) As FedEx_OpenShipService.CustomerReference
        create_CustomerReference = New FedEx_OpenShipService.CustomerReference
        With create_CustomerReference
            .CustomerReferenceType = FedEx_OpenShipService.CustomerReferenceType.CUSTOMER_REFERENCE
            .CustomerReferenceTypeSpecified = True
            .Value = package.PackageID
        End With
    End Function
    Private Function create_SignatureOptionDetail(ByVal obj As _baseServiceSurcharge) As FedEx_OpenShipService.SignatureOptionDetail
        create_SignatureOptionDetail = New FedEx_OpenShipService.SignatureOptionDetail
        With create_SignatureOptionDetail
            .OptionType = GetSignatureOptionType(obj.Description)
            .OptionTypeSpecified = True
            '.SignatureReleaseNumber
        End With
    End Function


#Region "International"
    Private Function create_CustomsClearanceDetail(ByVal obj As Object) As FedEx_OpenShipService.CustomsClearanceDetail
        create_CustomsClearanceDetail = New FedEx_OpenShipService.CustomsClearanceDetail
        If obj.CommInvoice IsNot Nothing Then
            Dim comminv As _baseCommInvoice = obj.CommInvoice
            With create_CustomsClearanceDetail
                .DutiesPayment = create_Payment(comminv.DutiesPaymentType, obj)
                .DocumentContent = GetInternationalDocumentContentType(_Controls.Contains(comminv.TypeOfContents, "documents"))
                .CustomsValue = create_Money(comminv.CustomsValue, comminv.CurrencyType)
                .InsuranceCharges = create_Money(comminv.InsuranceCharge, comminv.CurrencyType)
                .CommercialInvoice = create_CommercialInvoice(obj.CommInvoice)
                .ExportDetail = create_ExportDetail(comminv)
                If obj.CommInvoice.CommoditiesList IsNot Nothing AndAlso 0 < obj.CommInvoice.CommoditiesList.Count Then
                    Dim commoditis(obj.CommInvoice.CommoditiesList.Count - 1) As FedEx_OpenShipService.Commodity
                    For i As Integer = 0 To obj.CommInvoice.CommoditiesList.Count - 1
                        Dim commodity As _baseCommodities = obj.CommInvoice.CommoditiesList(i)
                        commoditis(i) = create_Commodity(commodity, comminv.CurrencyType)
                    Next i
                    .Commodities = commoditis
                End If
            End With
        End If
    End Function
    Private Function create_ExportDetail(ByVal obj As _baseCommInvoice) As FedEx_OpenShipService.ExportDetail
        create_ExportDetail = New FedEx_OpenShipService.ExportDetail
        With create_ExportDetail
            Dim typeB13A As New FedEx_OpenShipService.B13AFilingOptionType
            If GetB13AFilingOptionType(obj.B13AFilingOption, typeB13A) Then
                .B13AFilingOptionSpecified = True
                .B13AFilingOption = typeB13A
                If .B13AFilingOption = FedEx_OpenShipService.B13AFilingOptionType.FILED_ELECTRONICALLY Then
                    .ExportComplianceStatement = "V121245451XCVXCBNBV1253" ' test only
                ElseIf .B13AFilingOption = FedEx_OpenShipService.B13AFilingOptionType.SUMMARY_REPORTING Then
                    .ExportComplianceStatement = "DSGFH12" ' test only
                ElseIf .B13AFilingOption = FedEx_OpenShipService.B13AFilingOptionType.NOT_REQUIRED Then
                    .ExportComplianceStatement = "NO EEI 30.37(f)"
                End If
            End If
        End With
    End Function
    Private Function create_CommercialInvoice(ByVal obj As _baseCommInvoice) As FedEx_OpenShipService.CommercialInvoice
        create_CommercialInvoice = New FedEx_OpenShipService.CommercialInvoice
        With create_CommercialInvoice
            .Comments = {obj.Comments}
            .FreightCharge = create_Money(obj.FreightCharge, obj.CurrencyType)
            .TaxesOrMiscellaneousCharge = create_Money(obj.TaxesOrMiscCharge, obj.CurrencyType)
            .Purpose = GetPurposeOfShipmentType(obj.TypeOfContents)
        End With
    End Function
    Private Function create_Commodity(ByVal obj As _baseCommodities, ByVal currencytype As String) As FedEx_OpenShipService.Commodity
        create_Commodity = New FedEx_OpenShipService.Commodity
        With create_Commodity
            .NumberOfPieces = "1"
            .Description = obj.Item_Description
            .CountryOfManufacture = obj.Item_CountryOfOrigin
            .Weight = create_Weight(obj.Item_Weight, obj.Item_WeightUnits)
            .Quantity = obj.Item_Quantity.ToString
            .QuantityUnits = obj.Item_UnitsOfMeasure
            .UnitPrice = create_Money(obj.Item_UnitPrice, currencytype)
            .CustomsValue = create_Money(obj.Item_CustomsValue, currencytype)
        End With
    End Function
#End Region

#End Region
#Region "Get Types"
#Region "Types Required"
    Private Function GetPackagingType(ByVal srPackagingType As String) As FedEx_OpenShipService.PackagingType
        ''
        GetPackagingType = FedEx_OpenShipService.PackagingType.YOUR_PACKAGING '' assume.
        'Try
        If _Controls.Contains(srPackagingType, "Letter") Or _Controls.Contains(srPackagingType, "Env") Then
            GetPackagingType = FedEx_OpenShipService.PackagingType.FEDEX_ENVELOPE
        ElseIf _Controls.Contains(srPackagingType, "Box") And _Controls.Contains(srPackagingType, "10kg") Then
            GetPackagingType = FedEx_OpenShipService.PackagingType.FEDEX_10KG_BOX
        ElseIf _Controls.Contains(srPackagingType, "Box") And _Controls.Contains(srPackagingType, "25kg") Then
            GetPackagingType = FedEx_OpenShipService.PackagingType.FEDEX_25KG_BOX

            'ElseIf _Controls.Contains(srPackagingType, "Box") AndAlso _Controls.Contains(srPackagingType, "Extra") AndAlso _Controls.Contains(srPackagingType, "Large") Then
            '    GetPackagingType = PackagingType.FEDEX_EXTRA_LARGE_BOX
            'ElseIf _Controls.Contains(srPackagingType, "Box") AndAlso _Controls.Contains(srPackagingType, "Large") Then
            '    GetPackagingType = PackagingType.FEDEX_LARGE_BOX
            'ElseIf _Controls.Contains(srPackagingType, "Box") AndAlso _Controls.Contains(srPackagingType, "Medium") Then
            '    GetPackagingType = PackagingType.FEDEX_MEDIUM_BOX
            'ElseIf _Controls.Contains(srPackagingType, "Box") AndAlso _Controls.Contains(srPackagingType, "Small") Then
            '    GetPackagingType = PackagingType.FEDEX_SMALL_BOX

        ElseIf _Controls.Contains(srPackagingType, "Box") Then
            GetPackagingType = FedEx_OpenShipService.PackagingType.FEDEX_BOX
        ElseIf _Controls.Contains(srPackagingType, "Pak") Then
            GetPackagingType = FedEx_OpenShipService.PackagingType.FEDEX_PAK
        ElseIf _Controls.Contains(srPackagingType, "Tube") Then
            GetPackagingType = FedEx_OpenShipService.PackagingType.FEDEX_TUBE
        End If
        ''
    End Function
    Private Function GetServiceType(ByVal serviceABBR As String) As FedEx_OpenShipService.ServiceType
        ''
        GetServiceType = FedEx_OpenShipService.ServiceType.GROUND_HOME_DELIVERY '' assume.
        ''
        Select Case serviceABBR
            Case FedEx.Ground, FedEx.CanadaGround : GetServiceType = FedEx_OpenShipService.ServiceType.FEDEX_GROUND
            Case FedEx.FirstOvernight : GetServiceType = FedEx_OpenShipService.ServiceType.FIRST_OVERNIGHT
            Case FedEx.SecondDay : GetServiceType = FedEx_OpenShipService.ServiceType.FEDEX_2_DAY
            Case FedEx.SecondDayAM : GetServiceType = FedEx_OpenShipService.ServiceType.FEDEX_2_DAY_AM
            Case FedEx.Priority : GetServiceType = FedEx_OpenShipService.ServiceType.PRIORITY_OVERNIGHT
            Case FedEx.Standard : GetServiceType = FedEx_OpenShipService.ServiceType.STANDARD_OVERNIGHT
            Case FedEx.Saver : GetServiceType = FedEx_OpenShipService.ServiceType.FEDEX_EXPRESS_SAVER
            Case FedEx.Intl_First : GetServiceType = FedEx_OpenShipService.ServiceType.INTERNATIONAL_FIRST ''EUROPE_FIRST_INTERNATIONAL_PRIORITY
            Case FedEx.Intl_Priority : GetServiceType = FedEx_OpenShipService.ServiceType.INTERNATIONAL_PRIORITY ''INTERNATIONAL_PRIORITY_FREIGHT
            Case FedEx.Intl_Economy : GetServiceType = FedEx_OpenShipService.ServiceType.INTERNATIONAL_ECONOMY ''INTERNATIONAL_ECONOMY_FREIGHT
            Case FedEx.Freight_1Day : GetServiceType = FedEx_OpenShipService.ServiceType.FEDEX_1_DAY_FREIGHT
            Case FedEx.Freight_2Day : GetServiceType = FedEx_OpenShipService.ServiceType.FEDEX_2_DAY_FREIGHT
            Case FedEx.Freight_3Day : GetServiceType = FedEx_OpenShipService.ServiceType.FEDEX_3_DAY_FREIGHT
        End Select
        ''
    End Function
    Private Function GetSignatureOptionType(ByVal signatureOption As String) As FedEx_OpenShipService.SignatureOptionType
        ''
        GetSignatureOptionType = FedEx_OpenShipService.SignatureOptionType.NO_SIGNATURE_REQUIRED '' assume.
        Select Case signatureOption
            Case "Adult Signature" : GetSignatureOptionType = FedEx_OpenShipService.SignatureOptionType.ADULT
            Case "Direct Signature", "Direct Sig. - No Charge" : GetSignatureOptionType = FedEx_OpenShipService.SignatureOptionType.DIRECT
            Case "Indirect Signature" : GetSignatureOptionType = FedEx_OpenShipService.SignatureOptionType.INDIRECT
            Case "Del.Conf at No Charge" : GetSignatureOptionType = FedEx_OpenShipService.SignatureOptionType.SERVICE_DEFAULT
        End Select
        ''
    End Function
    Private Function GetCarrierCodeType(ByVal serviceABBR As String) As FedEx_OpenShipService.CarrierCodeType
        ''
        GetCarrierCodeType = FedEx_OpenShipService.CarrierCodeType.FDXG '' assume ground.
        ''
        Select Case serviceABBR
            Case FedEx.Priority, FedEx.FirstOvernight, FedEx.SecondDay, FedEx.SecondDayAM, FedEx.Standard, FedEx.Saver
                GetCarrierCodeType = FedEx_OpenShipService.CarrierCodeType.FDXE  ' FedEx Express
            Case FedEx.Intl_First, FedEx.Intl_Priority, FedEx.Intl_Economy
                GetCarrierCodeType = FedEx_OpenShipService.CarrierCodeType.FDXE ' FedEx Express
            Case FedEx.Ground, FedEx.CanadaGround
                GetCarrierCodeType = FedEx_OpenShipService.CarrierCodeType.FDXG ' FedEx Ground
            Case FedEx.Freight_1Day, FedEx.Freight_2Day, FedEx.Freight_3Day
                GetCarrierCodeType = FedEx_OpenShipService.CarrierCodeType.FXFR  ' FedEx Freight
        End Select
        ' If the CarrierCode is left blank, Express and Ground (if applicable/available) are returned in the reply.
        ''
    End Function
    Private Function GetCodAddTransportationChargesType(ByVal optionType As Integer) As FedEx_OpenShipService.CodAddTransportationChargeBasisType
        GetCodAddTransportationChargesType = FedEx_OpenShipService.CodAddTransportationChargeBasisType.COD_SURCHARGE ' assume.
        Select Case optionType
            Case CODChargeType_ADD_ACCOUNT_NET_CHARGE : GetCodAddTransportationChargesType = FedEx_OpenShipService.CodAddTransportationChargeBasisType.NET_CHARGE
            Case CODChargeType_ADD_ACCOUNT_NET_FREIGHT : GetCodAddTransportationChargesType = FedEx_OpenShipService.CodAddTransportationChargeBasisType.NET_FREIGHT
            Case CODChargeType_ADD_ACCOUNT_TOTAL_CUSTOMER_CHARGE : GetCodAddTransportationChargesType = FedEx_OpenShipService.CodAddTransportationChargeBasisType.TOTAL_CUSTOMER_CHARGE
        End Select
    End Function
    Private Function GetCodCollectionType(ByVal optionType As Integer) As FedEx_OpenShipService.CodCollectionType
        GetCodCollectionType = FedEx_OpenShipService.CodCollectionType.GUARANTEED_FUNDS ' assume.
        Select Case optionType
            Case CODPaymentType_ANY : GetCodCollectionType = FedEx_OpenShipService.CodCollectionType.ANY
            Case CODPaymentType_CASH : GetCodCollectionType = FedEx_OpenShipService.CodCollectionType.CASH
        End Select
    End Function
    Private Function GetHomeDeliveryPremiumType(ByVal optionType As String) As FedEx_OpenShipService.HomeDeliveryPremiumType
        GetHomeDeliveryPremiumType = FedEx_OpenShipService.HomeDeliveryPremiumType.EVENING
        If _Controls.Contains(optionType, "APPOINTMENT") Then
            GetHomeDeliveryPremiumType = FedEx_OpenShipService.HomeDeliveryPremiumType.APPOINTMENT
        ElseIf _Controls.Contains(optionType, "DATE") AndAlso _Controls.Contains(optionType, "CERTAIN") Then
            GetHomeDeliveryPremiumType = FedEx_OpenShipService.HomeDeliveryPremiumType.DATE_CERTAIN
        End If
    End Function
    Private Function GetPaymentType(ByVal type As String, ByVal shipment As Object, ByRef payor As _baseContact) As FedEx_OpenShipService.PaymentType
        GetPaymentType = FedEx_OpenShipService.PaymentType.SENDER
        ''ol#1.1.42(10/23)... FedEx International Shipping Charges must be assigned to Third-Party and Duties & Taxes must be assigned to Receiver party.
        ''  ''ol#1.1.34(8/13)... Force FedEx payment type as 'Sender' if Shipper and ShipTo contact country codes are 'US'.
        ''  If Not shipment.ShipToContact.CountryCode = "US" And Not shipment.ShipperContact.CountryCode = "US" Then ' Sender by Default in US
        If Not (shipment.ShipToContact.CountryCode = "US" And shipment.ShipperContact.CountryCode = "US") Then
            Select Case type
                Case "ACCOUNT" : GetPaymentType = FedEx_OpenShipService.PaymentType.ACCOUNT
                    payor = shipment.ShipperContact
                Case "COLLECT" : GetPaymentType = FedEx_OpenShipService.PaymentType.COLLECT
                    payor = shipment.ShipToContact
                Case "RECIPIENT" : GetPaymentType = FedEx_OpenShipService.PaymentType.RECIPIENT
                    payor = shipment.ShipToContact
                Case "RECIPIENT-TEST-ONLY" : GetPaymentType = FedEx_OpenShipService.PaymentType.RECIPIENT
                    payor = shipment.ShipToContact
                Case "THIRD_PARTY" : GetPaymentType = FedEx_OpenShipService.PaymentType.THIRD_PARTY
                    payor = shipment.ShipperContact
            End Select
        Else
            payor = shipment.ShipperContact
        End If
    End Function
    Private Function GetInternationalDocumentContentType(ByVal isShippingDocumentsOnly As Boolean) As FedEx_OpenShipService.InternationalDocumentContentType
        ''
        If isShippingDocumentsOnly Then
            GetInternationalDocumentContentType = FedEx_OpenShipService.InternationalDocumentContentType.DOCUMENTS_ONLY
        Else
            GetInternationalDocumentContentType = FedEx_OpenShipService.InternationalDocumentContentType.NON_DOCUMENTS
        End If
        ''
    End Function
    Private Function GetPurposeOfShipmentType(ByVal type As String) As FedEx_OpenShipService.PurposeOfShipmentType
        GetPurposeOfShipmentType = FedEx_OpenShipService.PurposeOfShipmentType.GIFT
        Select Case type
            Case _Controls.Contains(type, "Sample")
                GetPurposeOfShipmentType = FedEx_OpenShipService.PurposeOfShipmentType.SAMPLE
            Case _Controls.Contains(type, "Return") AndAlso _Controls.Contains(type, "Goods")
                GetPurposeOfShipmentType = FedEx_OpenShipService.PurposeOfShipmentType.REPAIR_AND_RETURN
            Case _Controls.Contains(type, "Other")
                GetPurposeOfShipmentType = FedEx_OpenShipService.PurposeOfShipmentType.PERSONAL_EFFECTS
            Case _Controls.Contains(type, "Documents")
                GetPurposeOfShipmentType = FedEx_OpenShipService.PurposeOfShipmentType.NOT_SOLD
        End Select
    End Function
#End Region
#Region "Types Optional"
    Private Function GetShipmentSpecialServiceType(ByVal optionType As String, ByRef type As FedEx_OpenShipService.ShipmentSpecialServiceType) As Boolean
        ''
        GetShipmentSpecialServiceType = True ' assume required
        If _Controls.Contains(optionType, "Rate") AndAlso _Controls.Contains(optionType, "One") Then
            'type = ShipmentSpecialServiceType.FEDEX_ONE_RATE
        ElseIf _Controls.Contains(optionType, "COD", True) Then
            type = FedEx_OpenShipService.ShipmentSpecialServiceType.COD
        ElseIf _Controls.Contains(optionType, "Dry") AndAlso _Controls.Contains(optionType, "Ice") Then
            type = FedEx_OpenShipService.ShipmentSpecialServiceType.DRY_ICE
        ElseIf _Controls.Contains(optionType, "EMAIL") Then
            type = FedEx_OpenShipService.ShipmentSpecialServiceType.EMAIL_NOTIFICATION
        ElseIf _Controls.Contains(optionType, "Home") AndAlso _Controls.Contains(optionType, "Delivery") Then
            type = FedEx_OpenShipService.ShipmentSpecialServiceType.HOME_DELIVERY_PREMIUM
        ElseIf _Controls.Contains(optionType, "Saturday") AndAlso _Controls.Contains(optionType, "Delivery") Then
            type = FedEx_OpenShipService.ShipmentSpecialServiceType.SATURDAY_DELIVERY
        ElseIf _Controls.Contains(optionType, "Saturday") AndAlso _Controls.Contains(optionType, "Pickup") Then
            type = FedEx_OpenShipService.ShipmentSpecialServiceType.SATURDAY_PICKUP
            'ElseIf _Controls.Contains(optionType, "Weekday") AndAlso _Controls.Contains(optionType, "Delivery") Then
            '    type = "WEEKDAY_DELIVERY"
        ElseIf _Controls.Contains(optionType, "HOLD") AndAlso _Controls.Contains(optionType, "LOCATION") Then
            type = FedEx_OpenShipService.ShipmentSpecialServiceType.HOLD_AT_LOCATION
        ElseIf _Controls.Contains(optionType, "Future") AndAlso _Controls.Contains(optionType, "Day") Then
            type = FedEx_OpenShipService.ShipmentSpecialServiceType.FUTURE_DAY_SHIPMENT
        ElseIf _Controls.Contains(optionType, "INSIDE") AndAlso _Controls.Contains(optionType, "DELIVERY") Then
            type = FedEx_OpenShipService.ShipmentSpecialServiceType.INSIDE_DELIVERY
        ElseIf _Controls.Contains(optionType, "INSIDE") AndAlso _Controls.Contains(optionType, "PICKUP") Then
            type = FedEx_OpenShipService.ShipmentSpecialServiceType.INSIDE_PICKUP
        Else
            GetShipmentSpecialServiceType = False ' optional
        End If
        'Case "x" : tmp = "THIRD_PARTY_CONSIGNEE"
        'Case "x" : tmp = "RETURN_SHIPMENT"
        'Case "x" : tmp = "HOLD_SATURDAY"
        'Case "x" : tmp = "BROKER_SELECT_OPTION"
    End Function
    Private Function GetPackageSpecialServiceType(ByVal optionType As String, ByRef type As FedEx_OpenShipService.PackageSpecialServiceType) As Boolean
        ''
        GetPackageSpecialServiceType = True ' assume required
        If _Controls.Contains(optionType, "COD", True) Then
            type = FedEx_OpenShipService.PackageSpecialServiceType.COD
        ElseIf _Controls.Contains(optionType, "Dry") AndAlso _Controls.Contains(optionType, "Ice") Then
            type = FedEx_OpenShipService.PackageSpecialServiceType.DRY_ICE
        ElseIf _Controls.Contains(optionType, "NON_STANDARD_CONTAINER") Then
            type = FedEx_OpenShipService.PackageSpecialServiceType.NON_STANDARD_CONTAINER
        ElseIf _Controls.Contains(optionType, "SIGNATURE") Then
            type = FedEx_OpenShipService.PackageSpecialServiceType.SIGNATURE_OPTION
        Else
            GetPackageSpecialServiceType = False ' optional
        End If
    End Function
    Private Function GetB13AFilingOptionType(ByVal optionType As String, ByRef type As FedEx_OpenShipService.B13AFilingOptionType) As Boolean
        GetB13AFilingOptionType = (Not 0 = optionType.Length) ' assume required
        Select Case optionType
            Case "FILED_ELECTRONICALLY" : type = FedEx_OpenShipService.B13AFilingOptionType.FILED_ELECTRONICALLY
            Case "MANUALLY_ATTACHED" : type = FedEx_OpenShipService.B13AFilingOptionType.MANUALLY_ATTACHED
            Case "SUMMARY_REPORTING" : type = FedEx_OpenShipService.B13AFilingOptionType.SUMMARY_REPORTING
            Case Else : type = FedEx_OpenShipService.B13AFilingOptionType.NOT_REQUIRED
        End Select
    End Function
#End Region
#End Region

End Module
