Imports System.Xml.Serialization
Imports System.IO

Public Module _FedEx_OneRate

#Region "RateOne & TimeInTransit"

    Public Function Process_Rate_TimeInTransit(ByVal srSetup As Object, ByVal obj As _baseShipment, ByRef vb_response As Object) As Boolean
        Process_Rate_TimeInTransit = False ' assume.
        Try
            Dim webservice As New FedEx_RateService.RateService
            webservice.Url = _FedExWeb.objFedEx_Setup.Connect2ServerURL '"https://ws.fedex.com:443/web-services"
            Dim shipService As New FedEx_RateService.RateRequest

            Dim webauth As New FedEx_RateService.WebAuthenticationDetail
            Dim webauth_csp As New FedEx_RateService.WebAuthenticationCredential
            Dim webauth_user As New FedEx_RateService.WebAuthenticationCredential
            If _FedExWeb.create_WebAuthenticationCredential(_FedExWeb.objFedEx_Setup.Web_CspCredential_Key, _FedExWeb.objFedEx_Setup.Web_CspCredential_Pass, webauth_csp) Then
                If _FedExWeb.create_WebAuthenticationCredential(_FedExWeb.objFedEx_Setup.Web_UserCredential_Key, _FedExWeb.objFedEx_Setup.Web_UserCredential_Pass, webauth_user) Then
                    webauth.ParentCredential = webauth_csp
                    webauth.UserCredential = webauth_user
                    shipService.WebAuthenticationDetail = webauth
                    '
                    Dim client As New FedEx_RateService.ClientDetail
                    If _FedExWeb.create_ClientDetail(client) Then
                        shipService.ClientDetail = client
                        '
                        Dim version As New FedEx_RateService.VersionId
                        If create_Version("crs", 18, 0, 0, version) Then
                            shipService.Version = version
                            '
                            ' Time In Transit enabled here:
                            shipService.ReturnTransitAndCommitSpecified = True
                            shipService.ReturnTransitAndCommit = True
                            '
                            Dim shipRequest As New FedEx_RateService.RequestedShipment
                            If create_RateRequestObject(srSetup, obj, shipRequest) Then
                                shipService.RequestedShipment = shipRequest
                                With shipService.RequestedShipment
                                    .PackageCount = obj.Packages.Count.ToString
                                    .TotalWeight = create_TotalWeight_RateService(obj)
                                    '
                                    For i As Integer = 0 To obj.Packages.Count - 1
                                        .RequestedPackageLineItems = {create_RequestedPackageLineItem_RateService(obj, i)}
                                        '
                                        Dim trans As New FedEx_RateService.TransactionDetail
                                        Dim srpack As _baseShipmentPackage = obj.Packages(0)
                                        trans.CustomerTransactionId = srpack.PackageID
                                        shipService.TransactionDetail = trans
                                        '
                                        If Not _Files.IsFileExist(String.Format("{0}\no_xml.txt", _FedExWeb.objFedEx_Setup.Path_SaveDocXML), False) Then
                                            Dim xdoc As New Xml.XmlDocument
                                            xdoc.LoadXml(serializeRateRequest2string(shipService))
                                            xdoc.Save(_FedExWeb.objFedEx_Setup.Path_SaveDocXML & "\Rate_Request.xml") ' shipment ID
                                        End If
                                        '
                                        Process_Rate_TimeInTransit = process_Rate_Response(srSetup, shipService, webservice, i, vb_response)
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
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to create a 'Rate & TimeInTransit' request...")
        End Try
    End Function
    Private Function process_Rate_Response(ByVal srSetup As Object, ByVal shipService As FedEx_RateService.RateRequest, ByVal webservice As FedEx_RateService.RateService, ByVal pack_sequence As Integer, ByRef vb_response As Object) As Boolean
        process_Rate_Response = False ' assume.
        Try
            Dim shipResponse As FedEx_RateService.RateReply = webservice.getRates(shipService)

            If Not _Files.IsFileExist(String.Format("{0}\no_xml.txt", _FedExWeb.objFedEx_Setup.Path_SaveDocXML), False) Then
                Dim xdoc As New Xml.XmlDocument
                xdoc.LoadXml(serializeRateResponse2string(shipResponse))
                xdoc.Save(_FedExWeb.objFedEx_Setup.Path_SaveDocXML & "\Rate_Reply.xml")
            End If
            '
            process_Rate_Response = True ' got the response!

            ' Result
            If Not IsNothing(shipResponse.Notifications) Then
                For n As Integer = 0 To shipResponse.Notifications.Length - 1
                    Dim notify As FedEx_RateService.Notification = shipResponse.Notifications(n)
                    If Not Val(notify.Code) = 0 Then
                        ' Error:
                        vb_response.TimeInTransitAlerts.Add(String.Format("{0}: {1}", notify.Severity, notify.Message))
                    End If
                Next n
            End If
            '
            If shipResponse.RateReplyDetails IsNot Nothing Then
                Dim shipFromCountryCode As String = shipService.RequestedShipment.Shipper.Address.CountryCode
                Dim shipToCountryCode As String = shipService.RequestedShipment.Recipient.Address.CountryCode
                For r As Integer = 0 To shipResponse.RateReplyDetails.Length - 1
                    Dim rate As FedEx_RateService.RateReplyDetail = shipResponse.RateReplyDetails(r)
                    If rate.CommitDetails IsNot Nothing Then
                        For c As Integer = 0 To rate.CommitDetails.Length - 1
                            ' Delivery dates:
                            Dim commit As FedEx_RateService.CommitDetail = rate.CommitDetails(c)
                            For v As Integer = 0 To vb_response.AvailableServices.Count - 1
                                Dim vb_service As Object = vb_response.AvailableServices(v)
                                Dim isHomeDelivery As Boolean = (commit.ServiceType = "GROUND_HOME_DELIVERY") And (FedEx.Ground = vb_service.ServiceCode)
                                If getServiceType_RateService(vb_service.ServiceCode, shipFromCountryCode, shipToCountryCode) = commit.ServiceType Or isHomeDelivery Then
                                    _Debug.Print_("Serivce: " & vb_service.ServiceCode)
                                    vb_service.IsOnlyArrivalTransitTime = commit.TransitTimeSpecified
                                    If commit.TransitTimeSpecified Then
                                        If commit.TransitTime = FedEx_RateService.TransitTimeType.ONE_DAY Then
                                            vb_service.ArrivalTransitTime = "1 day"
                                        Else
                                            vb_service.ArrivalTransitTime = String.Format("{0} days", getTimeInTransitTypeNumber_RateService(commit.TransitTime).ToString)
                                        End If
                                        _Debug.Print_(String.Format("Arrival: in {0}", vb_service.ArrivalTransitTime))
                                    Else
                                        If commit.CommitTimestampSpecified Then
                                            vb_service.ArrivalDate = commit.CommitTimestamp
                                            _Debug.Print_(String.Format("Arrival: {0}", vb_service.ArrivalDate.ToString))
                                        End If
                                        If commit.DayOfWeekSpecified Then
                                            vb_service.ArrivalDayOfWeek = commit.DayOfWeek.ToString
                                        End If
                                    End If
                                    vb_service.IsServiceAvailable = True
                                    Exit For ' found, go to the next available returned service
                                End If
                            Next v
                        Next c
                    End If
                Next r
                '
            End If
            '
        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message) : _Debug.Print_(ex.Detail.LastChild.InnerText)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to get a response for Rate & TimeInTransit request...")
        End Try
    End Function

    Private Function serializeRateRequest2string(obj As FedEx_RateService.RateRequest) As String
        Dim xml_serializer As New XmlSerializer(GetType(FedEx_RateService.RateRequest))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeRateRequest2string = string_writer.ToString()
        string_writer.Close()
    End Function
    Private Function serializeRateResponse2string(obj As FedEx_RateService.RateReply) As String
        Dim xml_serializer As New XmlSerializer(GetType(FedEx_RateService.RateReply))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeRateResponse2string = string_writer.ToString()
        string_writer.Close()
    End Function
    Private Function deserializeRateResponse2object(xmlsting As String) As FedEx_RateService.RateReply
        Dim xml_serializer As New XmlSerializer(GetType(FedEx_RateService.RateReply))
        Dim string_reader As New StringReader(xmlsting)
        deserializeRateResponse2object = DirectCast(xml_serializer.Deserialize(string_reader), FedEx_RateService.RateReply)
        string_reader.Close()
    End Function

#End Region
#Region "Create TinT Objects"
    Private Function create_RateRequestObject(ByVal srSetup As Object, ByVal obj As _baseShipment, ByRef shipRequest As FedEx_RateService.RequestedShipment) As Boolean
        create_RateRequestObject = False ' assume.
        With shipRequest
            .ShipTimestamp = obj.CarrierService.ShipDate
            .ShipTimestampSpecified = True
            .DropoffType = FedEx_RateService.DropoffType.REGULAR_PICKUP
            .DropoffTypeSpecified = True
            ' need all of the available services: 
            '.ServiceType = FedEx_RateService.ServiceType.FEDEX_2_DAY 'FedEx_Data2XML.GetServiceType(obj.CarrierService.ServiceABBR)
            Dim package As _baseShipmentPackage = obj.Packages(0)
            .PackagingType = getPackagingType_RateService(package.PackagingType)
            .PreferredCurrency = package.Currency_Type
            .Shipper = create_ContactParty_RateService(obj.ShipFromContact, _FedExWeb.objFedEx_Setup.Csp_AccountNumber)
            .Recipient = create_ContactParty_RateService(obj.ShipToContact, String.Empty)
            .ShippingChargesPayment = create_Payment_RateService(_FedExWeb.objFedEx_Setup.PaymentType, obj)
            ' FEDEX_ONE_RATE
            '.SpecialServicesRequested = New FedEx_RateService.ShipmentSpecialServicesRequested
            '.SpecialServicesRequested.SpecialServiceTypes = {FedEx_RateService.ShipmentSpecialServiceType.FEDEX_ONE_RATE}
            .SpecialServicesRequested = create_ShipmentSpecialServices_RateService(obj)
            .RateRequestTypes = {FedEx_RateService.RateRequestType.NONE}
            '
            'International:
            If Not obj.CarrierService.IsDomestic Then
                If obj.CommInvoice IsNot Nothing Then
                    .CustomsClearanceDetail = create_CustomsClearanceDetail_RateService(obj)
                End If
            End If
            '
            create_RateRequestObject = True
        End With

    End Function

    Private Function create_ContactParty_RateService(ByVal obj As _baseContact, ByVal accountNo As String) As FedEx_RateService.Party
        create_ContactParty_RateService = New FedEx_RateService.Party ' assume
        Dim address As New FedEx_RateService.Address
        Dim contact As New FedEx_RateService.Contact
        With create_ContactParty_RateService
            If Not accountNo = String.Empty Then
                .AccountNumber = accountNo
            End If
            If _FedExWeb.create_Address(obj, address) Then
                .Address = address
            End If
            If _FedExWeb.create_Contact(obj, contact) Then
                .Contact = contact
            End If
            '.Shipper.Tins
        End With
    End Function
    Private Function create_Payment_RateService(ByVal type As String, ByVal obj As _baseShipment) As FedEx_RateService.Payment
        create_Payment_RateService = New FedEx_RateService.Payment
        With create_Payment_RateService
            Dim contact As New _baseContact
            .PaymentType = FedEx_RateService.PaymentType.SENDER
            .PaymentTypeSpecified = True
            Dim accountNo As String = String.Empty
            If .PaymentType = FedEx_RateService.PaymentType.SENDER Or type = "RECIPIENT-TEST-ONLY" Then
                accountNo = _FedExWeb.objFedEx_Setup.Client_AccountNumber
                .Payor = create_Payor_RateSevice(contact, accountNo)
            End If
        End With
    End Function
    Private Function create_Payor_RateSevice(ByVal obj As _baseContact, ByVal accountNo As String) As FedEx_RateService.Payor
        create_Payor_RateSevice = New FedEx_RateService.Payor
        With create_Payor_RateSevice
            .ResponsibleParty = create_ContactParty_RateService(obj, accountNo)
        End With
    End Function
    Private Function create_Money_RateService(ByVal amount As Decimal, ByVal currencytype As String) As FedEx_RateService.Money
        create_Money_RateService = New FedEx_RateService.Money
        With create_Money_RateService
            .Amount = amount
            .Currency = currencytype
            .AmountSpecified = True
        End With
    End Function
    Private Function create_Weight_RateService(ByVal weight As Double, ByVal units As String) As FedEx_RateService.Weight
        create_Weight_RateService = New FedEx_RateService.Weight
        With create_Weight_RateService
            If "KG" = units.ToUpper Then
                .Units = FedEx_RateService.WeightUnits.KG
            Else
                .Units = FedEx_RateService.WeightUnits.LB
            End If
            .Value = weight
            .UnitsSpecified = True
            .ValueSpecified = True
        End With
    End Function
    Private Function create_TotalWeight_RateService(ByVal shipment As _baseShipment) As FedEx_RateService.Weight
        create_TotalWeight_RateService = New FedEx_RateService.Weight
        If shipment.Packages.Count > 0 Then
            With create_TotalWeight_RateService
                If "KG" = shipment.Packages(0).Weight_Units.ToUpper Then
                    .Units = FedEx_RateService.WeightUnits.KG
                Else
                    .Units = FedEx_RateService.WeightUnits.LB
                End If
                .UnitsSpecified = True
                For i As Integer = 0 To shipment.Packages.Count - 1
                    Dim pack As _baseShipmentPackage = shipment.Packages(i)
                    .Value += pack.Weight_LBs
                Next i
                .ValueSpecified = True
            End With
        End If
    End Function

    Private Function create_ShipmentSpecialServices_RateService(ByVal obj As _baseShipment) As FedEx_RateService.ShipmentSpecialServicesRequested
        create_ShipmentSpecialServices_RateService = New FedEx_RateService.ShipmentSpecialServicesRequested
        With create_ShipmentSpecialServices_RateService
            If obj.CarrierService.ServiceSurcharges IsNot Nothing AndAlso 0 < obj.CarrierService.ServiceSurcharges.Count Then
                Dim type(obj.CarrierService.ServiceSurcharges.Count - 1) As String
                For i As Integer = 0 To obj.CarrierService.ServiceSurcharges.Count - 1
                    Dim objServiceSurcharge As _baseServiceSurcharge = obj.CarrierService.ServiceSurcharges(i)
                    If objServiceSurcharge IsNot Nothing Then
                        If objServiceSurcharge.IsToShow Then
                            If getShipmentSpecialServiceType_RateService(objServiceSurcharge.Name, type(i)) Then
                                Select Case type(i)
                                    'Case FedEx_RateService.ShipmentSpecialServiceType.EMAIL_NOTIFICATION
                                    '.EMailNotificationDetail = create_EMailNotificationDetail(obj)
                                    Case "COD"
                                    'Dim cod As _baseServiceSurchargeCOD = obj.CarrierService.ServiceSurchargeCOD
                                    '.CodDetail = create_CodDetail(cod, obj.ShipperContact)
                                    Case "HOLD_AT_LOCATION"
                                    '.HoldAtLocationDetail = create_HoldAtLocationDetail(obj.HoldAtLocation)
                                    Case "HOME_DELIVERY_PREMIUM"
                                        .HomeDeliveryPremiumDetail = create_HomeDeliveryPremiumDetail_RateService(obj, FedEx_Data2XML.GetHomeDeliveryPremiumType(objServiceSurcharge.Description))
                                    Case "DRY_ICE"
                                    ' dry ice is at the Package level

                                    ' don't require details:
                                    Case "FEDEX_ONE_RATE"
                                    Case "FUTURE_DAY_SHIPMENT"
                                    Case "SATURDAY_DELIVERY"
                                    Case "SATURDAY_PICKUP"
                                    Case "INSIDE_DELIVERY"
                                    Case "INSIDE_PICKUP"
                                End Select
                            End If
                        End If
                    End If
                Next i
                .SpecialServiceTypes = type
            End If
        End With
    End Function
    Private Function create_HomeDeliveryPremiumDetail_RateService(ByVal obj As _baseShipment, ByVal type As FedEx_RateService.HomeDeliveryPremiumType) As FedEx_RateService.HomeDeliveryPremiumDetail
        create_HomeDeliveryPremiumDetail_RateService = New FedEx_RateService.HomeDeliveryPremiumDetail
        With create_HomeDeliveryPremiumDetail_RateService
            .Date = obj.CarrierService.DeliveryDate
            .DateSpecified = True
            .PhoneNumber = obj.ShipToContact.Tel
            .HomeDeliveryPremiumType = type
            .HomeDeliveryPremiumTypeSpecified = True
        End With
    End Function

    Private Function create_RequestedPackageLineItem_RateService(ByVal shipment As _baseShipment, ByVal siquenceno As Integer) As FedEx_RateService.RequestedPackageLineItem
        create_RequestedPackageLineItem_RateService = New FedEx_RateService.RequestedPackageLineItem
        Dim package As _baseShipmentPackage = shipment.Packages(siquenceno)
        With create_RequestedPackageLineItem_RateService
            .InsuredValue = create_Money_RateService(package.DeclaredValue, package.Currency_Type)
            .SequenceNumber = siquenceno.ToString
            .GroupPackageCount = "1"
            .GroupNumber = "1"
            .Weight = create_Weight_RateService(package.Weight_LBs, package.Weight_Units)
            .Dimensions = create_Dimensions_RateService(package)
            .CustomerReferences = {create_CustomerReference_RateService(package)}
            .SpecialServicesRequested = create_PackageSpecialServices_RateService(package, shipment.ShipperContact)
        End With
    End Function
    Private Function create_CustomerReference_RateService(ByVal obj As _baseShipmentPackage) As FedEx_RateService.CustomerReference
        create_CustomerReference_RateService = New FedEx_RateService.CustomerReference
        With create_CustomerReference_RateService
            .CustomerReferenceType = FedEx_RateService.CustomerReferenceType.CUSTOMER_REFERENCE
            .CustomerReferenceTypeSpecified = True
            .Value = obj.PackageID
        End With
    End Function
    Private Function create_PackageSpecialServices_RateService(ByVal obj As _baseShipmentPackage, ByVal codContact As _baseContact) As FedEx_RateService.PackageSpecialServicesRequested
        create_PackageSpecialServices_RateService = New FedEx_RateService.PackageSpecialServicesRequested
        With create_PackageSpecialServices_RateService
            If obj.ServiceSurcharges IsNot Nothing AndAlso 0 < obj.ServiceSurcharges.Count Then
                Dim type(obj.ServiceSurcharges.Count - 1) As String
                For i As Integer = 0 To obj.ServiceSurcharges.Count - 1
                    Dim objServiceSurcharge As _baseServiceSurcharge = obj.ServiceSurcharges(i)
                    If objServiceSurcharge IsNot Nothing Then
                        If objServiceSurcharge.IsToShow Then
                            If FedEx_Data2XML.GetPackageSpecialServiceType(objServiceSurcharge.Name, type(i)) Then
                                Select Case type(i)
                                    Case "SIGNATURE_OPTION"
                                        .SignatureOptionDetail = create_SignatureOptionDetail_RateService(objServiceSurcharge)
                                    Case "DRY_ICE"
                                        .DryIceWeight = create_Weight_RateService(obj.DryIce.Weight, obj.DryIce.WeightUnits)
                                    Case "NON_STANDARD_CONTAINER"
                                    ' nothing required - just a flag
                                    Case "COD"
                                        '.CodDetail = create_CodDetail(obj.COD, codContact)
                                End Select
                            End If
                        End If
                    End If
                Next i
                .SpecialServiceTypes = type
            End If
        End With
    End Function
    Private Function create_SignatureOptionDetail_RateService(ByVal obj As _baseServiceSurcharge) As FedEx_RateService.SignatureOptionDetail
        create_SignatureOptionDetail_RateService = New FedEx_RateService.SignatureOptionDetail
        With create_SignatureOptionDetail_RateService
            .OptionType = CType(FedEx_Data2XML.GetSignatureOptionType(obj.Description), FedEx_RateService.SignatureOptionType)
            .OptionTypeSpecified = True
            '.SignatureReleaseNumber
        End With
    End Function
    Private Function create_Dimensions_RateService(ByVal obj As _baseShipmentPackage) As FedEx_RateService.Dimensions
        create_Dimensions_RateService = New FedEx_RateService.Dimensions
        With create_Dimensions_RateService
            .Length = obj.Dim_Length.ToString
            .Width = obj.Dim_Width.ToString
            .Height = obj.Dim_Height.ToString
            If "CM" = obj.Dim_Units.ToUpper Then
                .Units = FedEx_RateService.LinearUnits.CM
            Else
                .Units = FedEx_RateService.LinearUnits.IN
            End If
            .UnitsSpecified = True
        End With
    End Function


#Region "International"
    Private Function create_CustomsClearanceDetail_RateService(ByVal obj As _baseShipment) As FedEx_RateService.CustomsClearanceDetail
        create_CustomsClearanceDetail_RateService = New FedEx_RateService.CustomsClearanceDetail
        If obj.CommInvoice IsNot Nothing Then
            Dim comminv As _baseCommInvoice = obj.CommInvoice
            With create_CustomsClearanceDetail_RateService
                .DutiesPayment = create_Payment_RateService(comminv.DutiesPaymentType, obj)
                .DocumentContent = CType(FedEx_Data2XML.GetInternationalDocumentContentType(_Controls.Contains(comminv.TypeOfContents, "documents")), FedEx_RateService.InternationalDocumentContentType)
                .CustomsValue = create_Money_RateService(comminv.CustomsValue, comminv.CurrencyType)
                .InsuranceCharges = create_Money_RateService(comminv.InsuranceCharge, comminv.CurrencyType)
                .CommercialInvoice = create_CommercialInvoice_RateService(obj.CommInvoice)
                .ExportDetail = create_ExportDetail_RateService(comminv)
                If obj.CommInvoice.CommoditiesList IsNot Nothing AndAlso 0 < obj.CommInvoice.CommoditiesList.Count Then
                    Dim commoditis(obj.CommInvoice.CommoditiesList.Count - 1) As FedEx_RateService.Commodity
                    For i As Integer = 0 To obj.CommInvoice.CommoditiesList.Count - 1
                        Dim commodity As _baseCommodities = obj.CommInvoice.CommoditiesList(i)
                        commoditis(i) = create_Commodity_RateService(commodity, comminv.CurrencyType)
                    Next i
                    .Commodities = commoditis
                End If
            End With
        End If
    End Function
    Private Function create_CommercialInvoice_RateService(ByVal obj As _baseCommInvoice) As FedEx_RateService.CommercialInvoice
        create_CommercialInvoice_RateService = New FedEx_RateService.CommercialInvoice
        With create_CommercialInvoice_RateService
            .Comments = {obj.Comments}
            .FreightCharge = create_Money_RateService(obj.FreightCharge, obj.CurrencyType)
            .TaxesOrMiscellaneousCharge = create_Money_RateService(obj.TaxesOrMiscCharge, obj.CurrencyType)
            .Purpose = CType(FedEx_Data2XML.GetPurposeOfShipmentType(obj.TypeOfContents), FedEx_RateService.PurposeOfShipmentType)
        End With
    End Function
    Private Function create_ExportDetail_RateService(ByVal obj As _baseCommInvoice) As FedEx_RateService.ExportDetail
        create_ExportDetail_RateService = New FedEx_RateService.ExportDetail
        With create_ExportDetail_RateService
            Dim typeB13A As New FedEx_RateService.B13AFilingOptionType
            If FedEx_Data2XML.GetB13AFilingOptionType(obj.B13AFilingOption, typeB13A) Then
                .B13AFilingOptionSpecified = True
                .B13AFilingOption = CType(typeB13A, FedEx_RateService.B13AFilingOptionType)
            End If
        End With
    End Function
    Private Function create_Commodity_RateService(ByVal obj As _baseCommodities, ByVal currencytype As String) As FedEx_RateService.Commodity
        create_Commodity_RateService = New FedEx_RateService.Commodity
        With create_Commodity_RateService
            .NumberOfPieces = "1"
            .Description = obj.Item_Description
            .CountryOfManufacture = obj.Item_CountryOfOrigin
            .Weight = create_Weight_RateService(obj.Item_Weight, obj.Item_WeightUnits)
            .Quantity = obj.Item_Quantity.ToString
            .QuantityUnits = obj.Item_UnitsOfMeasure
            .UnitPrice = create_Money_RateService(obj.Item_UnitPrice, currencytype)
            .CustomsValue = create_Money_RateService(obj.Item_CustomsValue, currencytype)
        End With
    End Function
#End Region

#End Region
#Region "Get TinT Types"
    Private Function getPackagingType_RateService(ByVal srPackagingType As String) As String
        ''
        getPackagingType_RateService = "YOUR_PACKAGING" '' assume.
        'Try
        If _Controls.Contains(srPackagingType, "Letter") Or _Controls.Contains(srPackagingType, "Env") Then
            getPackagingType_RateService = "FEDEX_ENVELOPE"
        ElseIf _Controls.Contains(srPackagingType, "Box") And _Controls.Contains(srPackagingType, "10kg") Then
            getPackagingType_RateService = "FEDEX_10KG_BOX"
        ElseIf _Controls.Contains(srPackagingType, "Box") And _Controls.Contains(srPackagingType, "25kg") Then
            getPackagingType_RateService = "FEDEX_25KG_BOX"

        ElseIf _Controls.Contains(srPackagingType, "Box") AndAlso _Controls.Contains(srPackagingType, "Extra") AndAlso _Controls.Contains(srPackagingType, "Large") Then
            getPackagingType_RateService = "FEDEX_EXTRA_LARGE_BOX"
        ElseIf _Controls.Contains(srPackagingType, "Box") AndAlso _Controls.Contains(srPackagingType, "Large") Then
            getPackagingType_RateService = "FEDEX_LARGE_BOX"
        ElseIf _Controls.Contains(srPackagingType, "Box") AndAlso _Controls.Contains(srPackagingType, "Medium") Then
            getPackagingType_RateService = "FEDEX_MEDIUM_BOX"
        ElseIf _Controls.Contains(srPackagingType, "Box") AndAlso _Controls.Contains(srPackagingType, "Small") Then
            getPackagingType_RateService = "FEDEX_SMALL_BOX"

        ElseIf _Controls.Contains(srPackagingType, "Box") Then
            getPackagingType_RateService = "FEDEX_BOX"
        ElseIf _Controls.Contains(srPackagingType, "Pak") Then
            getPackagingType_RateService = "FEDEX_PAK"
        ElseIf _Controls.Contains(srPackagingType, "Tube") Then
            getPackagingType_RateService = "FEDEX_TUBE"
        End If
        ''
    End Function
    Private Function getServiceType_RateService(ByVal serviceABBR As String, ByVal shipFromCountryCode As String, ByVal shipToCountryCode As String) As String
        getServiceType_RateService = "GROUND_HOME_DELIVERY" '' assume.
        Select Case serviceABBR
            Case FedEx.Ground, FedEx.CanadaGround : Return "FEDEX_GROUND"
            Case FedEx.FirstOvernight : Return "FIRST_OVERNIGHT"
            Case FedEx.SecondDay : Return "FEDEX_2_DAY"
            Case FedEx.SecondDayAM : Return "FEDEX_2_DAY_AM"
            Case FedEx.Priority
                If "PR" = shipToCountryCode Then
                    Return "INTERNATIONAL_PRIORITY"
                Else
                    Return "PRIORITY_OVERNIGHT"
                End If
            Case FedEx.Standard : Return "STANDARD_OVERNIGHT"
            Case FedEx.Saver
                If "PR" = shipToCountryCode Then
                    Return "INTERNATIONAL_ECONOMY"
                Else
                    Return "FEDEX_EXPRESS_SAVER"
                End If
            Case FedEx.Intl_First : Return "INTERNATIONAL_FIRST" ''EUROPE_FIRST_INTERNATIONAL_PRIORITY
            Case FedEx.Intl_Priority : Return "INTERNATIONAL_PRIORITY" ''INTERNATIONAL_PRIORITY_FREIGHT
            Case FedEx.Intl_Economy : Return "INTERNATIONAL_ECONOMY" ''INTERNATIONAL_ECONOMY_FREIGHT
            Case FedEx.Freight_1Day : Return "FEDEX_1_DAY_FREIGHT"
            Case FedEx.Freight_2Day : Return "FEDEX_2_DAY_FREIGHT"
            Case FedEx.Freight_3Day : Return "FEDEX_3_DAY_FREIGHT"
        End Select
    End Function
    Private Function getTimeInTransitTypeNumber_RateService(ByVal type As FedEx_RateService.TransitTimeType) As Long
        getTimeInTransitTypeNumber_RateService = 0 ' assume
        Select Case type
            Case FedEx_RateService.TransitTimeType.ONE_DAY : Return 1
            Case FedEx_RateService.TransitTimeType.TWO_DAYS : Return 2
            Case FedEx_RateService.TransitTimeType.THREE_DAYS : Return 3
            Case FedEx_RateService.TransitTimeType.FOUR_DAYS : Return 4
            Case FedEx_RateService.TransitTimeType.FIVE_DAYS : Return 5
            Case FedEx_RateService.TransitTimeType.SIX_DAYS : Return 6
            Case FedEx_RateService.TransitTimeType.SEVEN_DAYS : Return 7
            Case FedEx_RateService.TransitTimeType.EIGHT_DAYS : Return 8
            Case FedEx_RateService.TransitTimeType.NINE_DAYS : Return 9
            Case FedEx_RateService.TransitTimeType.TEN_DAYS : Return 10
            Case FedEx_RateService.TransitTimeType.ELEVEN_DAYS : Return 11
            Case FedEx_RateService.TransitTimeType.TWELVE_DAYS : Return 12
            Case FedEx_RateService.TransitTimeType.THIRTEEN_DAYS : Return 13
            Case FedEx_RateService.TransitTimeType.FOURTEEN_DAYS : Return 14
            Case FedEx_RateService.TransitTimeType.FIFTEEN_DAYS : Return 15
            Case FedEx_RateService.TransitTimeType.SIXTEEN_DAYS : Return 16
            Case FedEx_RateService.TransitTimeType.SEVENTEEN_DAYS : Return 17
            Case FedEx_RateService.TransitTimeType.EIGHTEEN_DAYS : Return 18
            Case FedEx_RateService.TransitTimeType.NINETEEN_DAYS : Return 19
            Case FedEx_RateService.TransitTimeType.TWENTY_DAYS : Return 20
        End Select
    End Function
    Private Function getShipmentSpecialServiceType_RateService(ByVal optionType As String, ByRef type As String) As Boolean
        ''
        getShipmentSpecialServiceType_RateService = True ' assume required
        If _Controls.Contains(optionType, "Rate") AndAlso _Controls.Contains(optionType, "One") Then
            type = "FEDEX_ONE_RATE"
        ElseIf _Controls.Contains(optionType, "COD", True) Then
            type = "COD"
        ElseIf _Controls.Contains(optionType, "Dry") AndAlso _Controls.Contains(optionType, "Ice") Then
            type = "DRY_ICE"
        ElseIf _Controls.Contains(optionType, "EMAIL") Then
            'type = FedEx_RateService.ShipmentSpecialServiceType.EMAIL_NOTIFICATION
        ElseIf _Controls.Contains(optionType, "Home") AndAlso _Controls.Contains(optionType, "Delivery") Then
            type = "HOME_DELIVERY_PREMIUM"
        ElseIf _Controls.Contains(optionType, "Saturday") AndAlso _Controls.Contains(optionType, "Delivery") Then
            type = "SATURDAY_DELIVERY"
        ElseIf _Controls.Contains(optionType, "Saturday") AndAlso _Controls.Contains(optionType, "Pickup") Then
            type = "SATURDAY_PICKUP"
            'ElseIf _Controls.Contains(optionType, "Weekday") AndAlso _Controls.Contains(optionType, "Delivery") Then
            '    type = "WEEKDAY_DELIVERY"
        ElseIf _Controls.Contains(optionType, "HOLD") AndAlso _Controls.Contains(optionType, "LOCATION") Then
            type = "HOLD_AT_LOCATION"
        ElseIf _Controls.Contains(optionType, "Future") AndAlso _Controls.Contains(optionType, "Day") Then
            type = "FUTURE_DAY_SHIPMENT"
        ElseIf _Controls.Contains(optionType, "INSIDE") AndAlso _Controls.Contains(optionType, "DELIVERY") Then
            type = "INSIDE_DELIVERY"
        ElseIf _Controls.Contains(optionType, "INSIDE") AndAlso _Controls.Contains(optionType, "PICKUP") Then
            type = "INSIDE_PICKUP"
        Else
            getShipmentSpecialServiceType_RateService = False ' optional
        End If
        'Case "x" : tmp = "THIRD_PARTY_CONSIGNEE"
        'Case "x" : tmp = "RETURN_SHIPMENT"
        'Case "x" : tmp = "HOLD_SATURDAY"
        'Case "x" : tmp = "BROKER_SELECT_OPTION"
    End Function

#End Region

#Region "Rate One Shipping"

    Public Function Process_ShipAPackage(ByVal srSetup As Object, ByVal obj As _baseShipment, ByRef vb_response As baseWebResponse_Shipment) As Boolean
        Process_ShipAPackage = False ' assume.
        Try
            Dim webservice As New FedEx_ShipService.ShipService
            webservice.Url = _FedExWeb.objFedEx_Setup.Connect2ServerURL '"https://ws.fedex.com:443/web-services"
            Dim shipService As New FedEx_ShipService.ProcessShipmentRequest

            Dim webauth As New FedEx_ShipService.WebAuthenticationDetail
            Dim webauth_csp As New FedEx_ShipService.WebAuthenticationCredential
            Dim webauth_user As New FedEx_ShipService.WebAuthenticationCredential
            If create_WebAuthenticationCredential(_FedExWeb.objFedEx_Setup.Web_CspCredential_Key, _FedExWeb.objFedEx_Setup.Web_CspCredential_Pass, webauth_csp) Then
                If create_WebAuthenticationCredential(_FedExWeb.objFedEx_Setup.Web_UserCredential_Key, _FedExWeb.objFedEx_Setup.Web_UserCredential_Pass, webauth_user) Then
                    webauth.ParentCredential = webauth_csp
                    webauth.UserCredential = webauth_user
                    shipService.WebAuthenticationDetail = webauth
                    '
                    Dim client As New FedEx_ShipService.ClientDetail
                    If create_ClientDetail(client) Then
                        shipService.ClientDetail = client
                        '
                        Dim version As New FedEx_ShipService.VersionId
                        If create_Version("ship", 21, 0, 0, version) Then
                            shipService.Version = version
                            '
                            Dim shipRequest As New FedEx_ShipService.RequestedShipment
                            If create_RequestObject(srSetup, obj, shipRequest) Then
                                shipService.RequestedShipment = shipRequest
                                With shipService.RequestedShipment
                                    .PackageCount = obj.Packages.Count.ToString
                                    .TotalWeight = create_TotalWeight(obj)
                                    '
                                    For i As Integer = 0 To obj.Packages.Count - 1
                                        If i > 1 Then
                                            ' only 1st package's Tracking# in MPS shipment is the MasterTracking#
                                            .MasterTrackingId = create_MasterTrackingID(vb_response.Packages(0).TrackingNo)
                                        End If
                                        .RequestedPackageLineItems = {create_RequestedPackageLineItem(obj, i)}
                                        '
                                        Dim trans As New FedEx_ShipService.TransactionDetail
                                        Dim srpack As _baseShipmentPackage = obj.Packages(i)
                                        trans.CustomerTransactionId = srpack.PackageID
                                        shipService.TransactionDetail = trans
                                        '
                                        If Not _Files.IsFileExist(String.Format("{0}\no_xml.txt", _FedExWeb.objFedEx_Setup.Path_SaveDocXML), False) Then
                                            Dim xdoc As New Xml.XmlDocument
                                            xdoc.LoadXml(serializeShipRequest2string(shipService))
                                            xdoc.Save(_FedExWeb.objFedEx_Setup.Path_SaveDocXML & "\" & srpack.PackageID & "_RequestShipment.xml") ' shipment ID
                                        End If
                                        '
                                        Process_ShipAPackage = process_ShipAPackage_Response(srSetup, shipService, webservice, i, vb_response)
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
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to create a 'Ship A Package' request...")
        End Try
    End Function
    Private Function process_ShipAPackage_Response(ByVal srSetup As Object, ByVal shipService As FedEx_ShipService.ProcessShipmentRequest, ByVal webservice As FedEx_ShipService.ShipService, ByVal pack_sequence As Integer, ByRef vb_response As Object) As Boolean
        process_ShipAPackage_Response = False ' assume.
        Try
            Dim shipResponse As FedEx_ShipService.ProcessShipmentReply = webservice.processShipment(shipService)

            If Not _Files.IsFileExist(String.Format("{0}\no_xml.txt", _FedExWeb.objFedEx_Setup.Path_SaveDocXML), False) Then
                Dim xdoc As New Xml.XmlDocument
                xdoc.LoadXml(serializeShipResponse2string(shipResponse))
                xdoc.Save(_FedExWeb.objFedEx_Setup.Path_SaveDocXML & "\" & vb_response.Packages(pack_sequence).PackageID & "_ReplyShipment.xml")
            End If
            process_ShipAPackage_Response = True ' got the response!

            ' Result
            If shipResponse.Notifications IsNot Nothing Then
                For n As Integer = 0 To shipResponse.Notifications.Length - 1
                    Dim notify As FedEx_ShipService.Notification = shipResponse.Notifications(n)
                    If Not notify.Code = "0000" Then
                        ' Error:
                        vb_response.ShipmentAlerts.Add(notify.Message)
                    End If
                    '_Debug.Print_(notify.Severity.ToString)
                    '_Debug.Print_(notify.Source)
                    '_Debug.Print_(notify.Code)
                    '_Debug.Print_(notify.Message)
                Next n
            End If
            '
            If shipResponse.CompletedShipmentDetail IsNot Nothing Then
                Dim shipment As FedEx_ShipService.CompletedShipmentDetail = shipResponse.CompletedShipmentDetail
                If shipment.OperationalDetail IsNot Nothing Then
                    ' Delivery dates:
                    Dim dates As FedEx_ShipService.ShipmentOperationalDetail = shipment.OperationalDetail
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
                        Dim cod As FedEx_ShipService.AssociatedShipmentDetail = shipment.AssociatedShipments(s)
                        Dim codString As String = String.Empty
                        Dim codFileExt As String = FedEx_Data2XML.GetLabel_FileExtension(_FedExWeb.objFedEx_Setup.LabelImageType)
                        Dim codFile As String = _FedExWeb.objFedEx_Setup.Path_SaveDocXML & "\" & vb_response.Packages(s + pack_sequence).PackageID & "_labelCOD." & codFileExt
                        '
                        If Not IsNothing(cod.Label) Then
                            If _Files.WriteFile_ToEnd(cod.Label.Parts(s).Image, codFile) Then
                                If _Files.ReadFile_ToEnd(codFile, False, codString) Then
                                    vb_response.Packages(s + pack_sequence).LabelCODImage = codString
                                End If
                            End If
                        End If
                    Next s
                End If
                '
                Dim packages As FedEx_ShipService.CompletedPackageDetail() = shipment.CompletedPackageDetails
                If Not IsNothing(packages) Then
                    For p As Integer = 0 To packages.Length - 1
                        Dim package As FedEx_ShipService.CompletedPackageDetail = shipResponse.CompletedShipmentDetail.CompletedPackageDetails(p)
                        ' Tracking Number:
                        For t As Integer = 0 To package.TrackingIds.Length - 1
                            vb_response.Packages(t + pack_sequence).TrackingNo = package.TrackingIds(t).TrackingNumber
                        Next t
                        ' Shipping labels:
                        For i As Integer = 0 To package.Label.Parts.Length - 1
                            Dim labelString As String = String.Empty
                            Dim labelFileExt As String = FedEx_Data2XML.GetLabel_FileExtension(_FedExWeb.objFedEx_Setup.LabelImageType)
                            Dim labelFile As String = _FedExWeb.objFedEx_Setup.Path_SaveDocXML & "\" & vb_response.Packages(i + pack_sequence).PackageID & "_label." & labelFileExt
                            '
                            If Not IsNothing(package.Label) Then
                                If _Files.WriteFile_ToEnd(package.Label.Parts(i).Image, labelFile) Then
                                    If _Files.ReadFile_ToEnd(labelFile, False, labelString) Then
                                        vb_response.Packages(i + pack_sequence).LabelImage = labelString
                                    End If
                                End If
                            End If
                            If Not IsNothing(package.CodReturnDetail) Then
                                If Not IsNothing(package.CodReturnDetail.Label) Then
                                    Dim cod As FedEx_ShipService.ShippingDocument = package.CodReturnDetail.Label
                                    Dim codString As String = String.Empty
                                    Dim codFileExt As String = FedEx_Data2XML.GetLabel_FileExtension(_FedExWeb.objFedEx_Setup.LabelImageType)
                                    Dim codFile As String = _FedExWeb.objFedEx_Setup.Path_SaveDocXML & "\" & vb_response.Packages(i + pack_sequence).PackageID & "_labelCOD." & codFileExt
                                    If _Files.WriteFile_ToEnd(cod.Parts(i).Image, codFile) Then
                                        If _Files.ReadFile_ToEnd(codFile, False, codString) Then
                                            vb_response.Packages(i + pack_sequence).LabelCODImage = codString
                                        End If
                                    End If
                                End If
                            End If
                        Next i
                    Next p
                End If
            End If
        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message) : _Debug.Print_(ex.Detail.LastChild.InnerText)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to get a response for Ship-A-Package request...")
        End Try
    End Function

    Private Function serializeShipRequest2string(obj As FedEx_ShipService.ProcessShipmentRequest) As String
        Dim xml_serializer As New XmlSerializer(GetType(FedEx_ShipService.ProcessShipmentRequest))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeShipRequest2string = string_writer.ToString()
        string_writer.Close()
    End Function
    Private Function serializeShipResponse2string(obj As FedEx_ShipService.ProcessShipmentReply) As String
        Dim xml_serializer As New XmlSerializer(GetType(FedEx_ShipService.ProcessShipmentReply))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeShipResponse2string = string_writer.ToString()
        string_writer.Close()
    End Function
    Private Function deserializeShipResponse2object(xmlsting As String) As FedEx_ShipService.ProcessShipmentReply
        Dim xml_serializer As New XmlSerializer(GetType(FedEx_ShipService.ProcessShipmentReply))
        Dim string_reader As New StringReader(xmlsting)
        deserializeShipResponse2object = DirectCast(xml_serializer.Deserialize(string_reader), FedEx_ShipService.ProcessShipmentReply)
        string_reader.Close()
    End Function
#End Region
#Region "Create Shipping Objects"
    Private Function create_RequestObject(ByVal srSetup As Object, ByVal obj As _baseShipment, ByRef shipRequest As FedEx_ShipService.RequestedShipment) As Boolean
        create_RequestObject = False ' assume.
        With shipRequest
            .ShipTimestamp = obj.CarrierService.ShipDate
            .DropoffType = FedEx_ShipService.DropoffType.REGULAR_PICKUP
            .ServiceType = GetServiceType(obj.CarrierService.ServiceABBR)
            Dim package As _baseShipmentPackage = obj.Packages(0)
            .PackagingType = GetPackagingType(package.PackagingType)
            .Shipper = create_ContactParty_ShipService(obj.ShipFromContact, _FedExWeb.objFedEx_Setup.Csp_AccountNumber)
            .Recipient = create_ContactParty_ShipService(obj.ShipToContact, String.Empty)
            .ShippingChargesPayment = create_Payment(_FedExWeb.objFedEx_Setup.PaymentType, obj)
            .SpecialServicesRequested = create_ShipmentSpecialServices(obj)
            .LabelSpecification = create_LabelSpecification()
            .RateRequestTypes = {FedEx_ShipService.RateRequestType.NONE}
            '
            'International:
            If Not obj.CarrierService.IsDomestic Then
                .CustomsClearanceDetail = create_CustomsClearanceDetail(obj)
            End If
            '
            create_RequestObject = True
        End With
    End Function
    Private Function create_MasterTrackingID(ByVal masterTrackingNo As String) As FedEx_ShipService.TrackingId
        create_MasterTrackingID = New FedEx_ShipService.TrackingId
        With create_MasterTrackingID
            .TrackingIdType = FedEx_ShipService.TrackingIdType.FEDEX
            .TrackingIdTypeSpecified = True
            '.FormId = String.Empty
            .TrackingNumber = masterTrackingNo
        End With
    End Function
    Private Function create_ContactParty_ShipService(ByVal obj As _baseContact, ByVal accountNo As String) As FedEx_ShipService.Party
        create_ContactParty_ShipService = New FedEx_ShipService.Party ' assume
        Dim address As New FedEx_ShipService.Address
        Dim contact As New FedEx_ShipService.Contact
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
    Private Function create_Payment(ByVal type As String, ByVal obj As _baseShipment) As FedEx_ShipService.Payment
        create_Payment = New FedEx_ShipService.Payment
        With create_Payment
            Dim contact As New _baseContact
            .PaymentType = GetPaymentType(type, obj, contact)
            Dim accountNo As String = String.Empty
            If .PaymentType = FedEx_ShipService.PaymentType.SENDER Or .PaymentType = FedEx_ShipService.PaymentType.THIRD_PARTY Or type = "RECIPIENT-TEST-ONLY" Then
                accountNo = _FedExWeb.objFedEx_Setup.Client_AccountNumber
                .Payor = create_Payor(contact, accountNo)
            End If
        End With
    End Function
    Private Function create_Payor(ByVal obj As _baseContact, ByVal accountNo As String) As FedEx_ShipService.Payor
        create_Payor = New FedEx_ShipService.Payor
        With create_Payor
            .ResponsibleParty = create_ContactParty_ShipService(obj, accountNo)
        End With
    End Function
    Private Function create_ShipmentSpecialServices(ByVal obj As _baseShipment) As FedEx_ShipService.ShipmentSpecialServicesRequested
        create_ShipmentSpecialServices = New FedEx_ShipService.ShipmentSpecialServicesRequested
        With create_ShipmentSpecialServices
            If obj.CarrierService.ServiceSurcharges IsNot Nothing AndAlso 0 < obj.CarrierService.ServiceSurcharges.Count Then
                Dim type(obj.CarrierService.ServiceSurcharges.Count - 1) As FedEx_ShipService.ShipmentSpecialServiceType
                For i As Integer = 0 To obj.CarrierService.ServiceSurcharges.Count - 1
                    Dim objServiceSurcharge As _baseServiceSurcharge = obj.CarrierService.ServiceSurcharges(i)
                    If objServiceSurcharge IsNot Nothing Then
                        If objServiceSurcharge.IsToShow Then
                            If GetShipmentSpecialServiceType(objServiceSurcharge.Name, type(i)) Then
                                Select Case type(i)
                                'Case FedEx_ShipService.ShipmentSpecialServiceType.EMAIL_NOTIFICATION
                                '    .EMailNotificationDetail = create_EMailNotificationDetail(obj)
                                    Case FedEx_ShipService.ShipmentSpecialServiceType.COD
                                        Dim cod As _baseServiceSurchargeCOD = obj.CarrierService.ServiceSurchargeCOD
                                        .CodDetail = create_CodDetail(cod, obj.ShipperContact)
                                    Case FedEx_ShipService.ShipmentSpecialServiceType.HOLD_AT_LOCATION
                                        .HoldAtLocationDetail = create_HoldAtLocationDetail(obj.HoldAtLocation)
                                    Case FedEx_ShipService.ShipmentSpecialServiceType.HOME_DELIVERY_PREMIUM
                                        .HomeDeliveryPremiumDetail = create_HomeDeliveryPremiumDetail(obj, GetHomeDeliveryPremiumType(objServiceSurcharge.Description))
                                    Case FedEx_ShipService.ShipmentSpecialServiceType.DRY_ICE
                                    ' dry ice is at the Package level

                                    ' don't require details:
                                    Case FedEx_ShipService.ShipmentSpecialServiceType.FEDEX_ONE_RATE
                                    Case FedEx_ShipService.ShipmentSpecialServiceType.FUTURE_DAY_SHIPMENT
                                    Case FedEx_ShipService.ShipmentSpecialServiceType.SATURDAY_DELIVERY
                                    Case FedEx_ShipService.ShipmentSpecialServiceType.SATURDAY_PICKUP
                                    Case FedEx_ShipService.ShipmentSpecialServiceType.INSIDE_DELIVERY
                                    Case FedEx_ShipService.ShipmentSpecialServiceType.INSIDE_PICKUP
                                End Select
                            End If
                        End If
                    End If
                Next i
                .SpecialServiceTypes = type
            End If
        End With
    End Function

    Private Function create_CodDetail(ByVal cod As _baseServiceSurchargeCOD, ByVal contact As _baseContact) As FedEx_ShipService.CodDetail
        create_CodDetail = New FedEx_ShipService.CodDetail
        With create_CodDetail
            If cod.AddCOD2Total Then
                .AddTransportationChargesDetail = create_CodAddTransportationChargesDetail(cod)
            End If
            .CollectionType = GetCodCollectionType(cod.PaymentType)
            .CodCollectionAmount = create_Money(cod.Amount, cod.CurrencyType)
            .CodRecipient = create_ContactParty_ShipService(contact, String.Empty)
        End With
    End Function
    Private Function create_CodAddTransportationChargesDetail(ByVal cod As _baseServiceSurchargeCOD) As FedEx_ShipService.CodAddTransportationChargesDetail
        create_CodAddTransportationChargesDetail = New FedEx_ShipService.CodAddTransportationChargesDetail
        With create_CodAddTransportationChargesDetail
            .ChargeBasisSpecified = True
            .ChargeBasis = GetCodAddTransportationChargesType(cod.ChargeType)
            .ChargeBasisLevelSpecified = True
            .ChargeBasisLevel = FedEx_ShipService.ChargeBasisLevelType.CURRENT_PACKAGE
            .RateTypeBasisSpecified = True
            .RateTypeBasis = FedEx_ShipService.RateTypeBasisType.ACCOUNT
        End With
    End Function

    Private Function create_Money(ByVal amount As Decimal, currencytype As String) As FedEx_ShipService.Money
        create_Money = New FedEx_ShipService.Money
        With create_Money
            .Amount = amount
            .Currency = currencytype
        End With
    End Function
    ''Private Function create_EMailNotificationDetail(ByVal obj As _baseShipment) As FedEx_ShipService.EMailNotificationDetail
    ''    ' The descriptive data required for FedEx to provide email notification to the customer regarding the shipment.
    ''    create_EMailNotificationDetail = New FedEx_ShipService.EMailNotificationDetail
    ''    With create_EMailNotificationDetail
    ''        .PersonalMessage = _Controls.Left(obj.Comments, 120)
    ''        If Not 0 = obj.ShipFromContact.Email.Length And Not 0 = obj.ShipToContact.Email.Length Then
    ''            .Recipients = {create_EMailNotificationRecipient(obj.ShipFromContact, FedEx_ShipService.EMailNotificationRecipientType.SHIPPER), create_EMailNotificationRecipient(obj.ShipToContact, FedEx_ShipService.EMailNotificationRecipientType.RECIPIENT)}
    ''        ElseIf Not 0 = obj.ShipFromContact.Email.Length Then
    ''            .Recipients = {create_EMailNotificationRecipient(obj.ShipFromContact, FedEx_ShipService.EMailNotificationRecipientType.SHIPPER)}
    ''        ElseIf Not 0 = obj.ShipToContact.Email.Length Then
    ''            .Recipients = {create_EMailNotificationRecipient(obj.ShipToContact, FedEx_ShipService.EMailNotificationRecipientType.RECIPIENT)}
    ''        End If
    ''    End With
    ''End Function
    ''Private Function create_EMailNotificationRecipient(ByVal obj As _baseContact, ByVal type As FedEx_ShipService.EMailNotificationRecipientType) As FedEx_ShipService.EMailNotificationRecipient
    ''    create_EMailNotificationRecipient = New FedEx_ShipService.EMailNotificationRecipient
    ''    With create_EMailNotificationRecipient
    ''        .EMailAddress = obj.Email
    ''        .EMailNotificationRecipientType = type
    ''        .Format = FedEx_ShipService.EMailNotificationFormatType.TEXT
    ''        .NotificationEventsRequested = {FedEx_ShipService.EMailNotificationEventType.ON_DELIVERY, FedEx_ShipService.EMailNotificationEventType.ON_EXCEPTION, FedEx_ShipService.EMailNotificationEventType.ON_SHIPMENT}
    ''        '.NotificationEventsRequested = {EMailNotificationEventType.ON_EXCEPTION}
    ''        '.NotificationEventsRequested = {EMailNotificationEventType.ON_SHIPMENT}
    ''        .Localization = New FedEx_ShipService.Localization
    ''        .Localization.LanguageCode = "EN"
    ''    End With
    ''End Function
    Private Function create_HomeDeliveryPremiumDetail(ByVal obj As _baseShipment, ByVal type As FedEx_ShipService.HomeDeliveryPremiumType) As FedEx_ShipService.HomeDeliveryPremiumDetail
        create_HomeDeliveryPremiumDetail = New FedEx_ShipService.HomeDeliveryPremiumDetail
        With create_HomeDeliveryPremiumDetail
            .Date = obj.CarrierService.DeliveryDate
            .DateSpecified = True
            .PhoneNumber = obj.ShipToContact.Tel
            .HomeDeliveryPremiumType = type
        End With
    End Function
    Private Function create_LabelSpecification() As FedEx_ShipService.LabelSpecification
        create_LabelSpecification = New FedEx_ShipService.LabelSpecification
        With create_LabelSpecification
            .LabelFormatType = FedEx_ShipService.LabelFormatType.COMMON2D 'FedEx_Data2XML.LabelFormatType
            .ImageTypeSpecified = True
            If _Controls.Contains(_FedExWeb.objFedEx_Setup.LabelImageType, "Thermal") Then
                .ImageType = FedEx_ShipService.ShippingDocumentImageType.EPL2
                .LabelStockType = FedEx_ShipService.LabelStockType.STOCK_4X6
                .LabelStockTypeSpecified = True
                .LabelPrintingOrientation = FedEx_ShipService.LabelPrintingOrientationType.BOTTOM_EDGE_OF_TEXT_FIRST
                .LabelPrintingOrientationSpecified = True
            Else
                .ImageType = FedEx_ShipService.ShippingDocumentImageType.PDF
            End If
            .CustomerSpecifiedDetail = create_CustomerSpecifiedLabelDetail()
        End With
    End Function
    Private Function create_CustomerSpecifiedLabelDetail() As FedEx_ShipService.CustomerSpecifiedLabelDetail
        create_CustomerSpecifiedLabelDetail = New FedEx_ShipService.CustomerSpecifiedLabelDetail
        With create_CustomerSpecifiedLabelDetail
            .MaskedData = {FedEx_ShipService.LabelMaskableDataType.SHIPPER_ACCOUNT_NUMBER}
        End With
    End Function
    Private Function create_HoldAtLocationDetail(ByVal obj As _baseContact) As FedEx_ShipService.HoldAtLocationDetail
        Dim address As New FedEx_ShipService.Address
        Dim contact As New FedEx_ShipService.Contact
        create_HoldAtLocationDetail = New FedEx_ShipService.HoldAtLocationDetail
        With create_HoldAtLocationDetail
            .LocationContactAndAddress = New FedEx_ShipService.ContactAndAddress
            If create_Contact(obj, contact) Then
                .LocationContactAndAddress.Contact = contact
            End If
            If create_Address(obj, address) Then
                .LocationContactAndAddress.Address = address
            End If
            .PhoneNumber = obj.Tel
            ''ol#1.2.33(3/28)... FedEx Cert: One Rate HAL is missing Location Type tag.
            .LocationType = FedEx_ShipService.FedExLocationType.FEDEX_EXPRESS_STATION
            .LocationTypeSpecified = True
        End With
    End Function


    Private Function create_RequestedPackageLineItem(ByVal shipment As _baseShipment, ByVal siquenceno As Integer) As FedEx_ShipService.RequestedPackageLineItem
        create_RequestedPackageLineItem = New FedEx_ShipService.RequestedPackageLineItem
        Dim package As _baseShipmentPackage = shipment.Packages(siquenceno)
        With create_RequestedPackageLineItem
            .InsuredValue = create_Money(package.DeclaredValue, package.Currency_Type)
            .SequenceNumber = siquenceno.ToString
            .Weight = create_Weight(package.Weight_LBs, package.Weight_Units)
            .Dimensions = create_Dimensions(package)
            .CustomerReferences = {create_CustomerReference(package)}
            .SpecialServicesRequested = create_PackageSpecialServices(package, shipment.ShipperContact)
        End With
    End Function
    Private Function create_PackageSpecialServices(ByVal obj As _baseShipmentPackage, ByVal codContact As _baseContact) As FedEx_ShipService.PackageSpecialServicesRequested
        create_PackageSpecialServices = New FedEx_ShipService.PackageSpecialServicesRequested
        With create_PackageSpecialServices
            If obj.ServiceSurcharges IsNot Nothing AndAlso 0 < obj.ServiceSurcharges.Count Then
                Dim type(obj.ServiceSurcharges.Count - 1) As FedEx_ShipService.PackageSpecialServiceType
                For i As Integer = 0 To obj.ServiceSurcharges.Count - 1
                    Dim objServiceSurcharge As _baseServiceSurcharge = obj.ServiceSurcharges(i)
                    If objServiceSurcharge IsNot Nothing Then
                        If objServiceSurcharge.IsToShow Then
                            If GetPackageSpecialServiceType(objServiceSurcharge.Name, type(i)) Then
                                Select Case type(i)
                                    Case FedEx_ShipService.PackageSpecialServiceType.SIGNATURE_OPTION
                                        .SignatureOptionDetail = create_SignatureOptionDetail(objServiceSurcharge)
                                    Case FedEx_ShipService.PackageSpecialServiceType.DRY_ICE
                                        .DryIceWeight = create_Weight(obj.DryIce.Weight, obj.DryIce.WeightUnits)
                                    Case FedEx_ShipService.PackageSpecialServiceType.NON_STANDARD_CONTAINER
                                    ' nothing required - just a flag
                                    Case FedEx_ShipService.PackageSpecialServiceType.COD
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

    Private Function create_TotalWeight(ByVal shipment As _baseShipment) As FedEx_ShipService.Weight
        create_TotalWeight = New FedEx_ShipService.Weight
        If shipment.Packages.Count > 0 Then
            With create_TotalWeight
                If "KG" = shipment.Packages(0).Weight_Units.ToUpper Then
                    .Units = FedEx_ShipService.WeightUnits.KG
                Else
                    .Units = FedEx_ShipService.WeightUnits.LB
                End If
                For i As Integer = 0 To shipment.Packages.Count - 1
                    Dim pack As _baseShipmentPackage = shipment.Packages(i)
                    .Value += pack.Weight_LBs
                Next i
            End With
        End If
    End Function
    Private Function create_Weight(ByVal weight As Double, ByVal units As String) As FedEx_ShipService.Weight
        create_Weight = New FedEx_ShipService.Weight
        With create_Weight
            If "KG" = units.ToUpper Then
                .Units = FedEx_ShipService.WeightUnits.KG
            Else
                .Units = FedEx_ShipService.WeightUnits.LB
            End If
            .Value = weight
        End With
    End Function
    Private Function create_Dimensions(ByVal obj As _baseShipmentPackage) As FedEx_ShipService.Dimensions
        create_Dimensions = New FedEx_ShipService.Dimensions
        With create_Dimensions
            .Length = obj.Dim_Length.ToString
            .Width = obj.Dim_Width.ToString
            .Height = obj.Dim_Height.ToString
            If "CM" = obj.Dim_Units.ToUpper Then
                .Units = FedEx_ShipService.LinearUnits.CM
            Else
                .Units = FedEx_ShipService.LinearUnits.IN
            End If
        End With
    End Function
    Private Function create_CustomerReference(ByVal obj As _baseShipmentPackage) As FedEx_ShipService.CustomerReference
        create_CustomerReference = New FedEx_ShipService.CustomerReference
        With create_CustomerReference
            .CustomerReferenceType = FedEx_ShipService.CustomerReferenceType.CUSTOMER_REFERENCE
            .Value = obj.PackageID
        End With
    End Function
    Private Function create_SignatureOptionDetail(ByVal obj As _baseServiceSurcharge) As FedEx_ShipService.SignatureOptionDetail
        create_SignatureOptionDetail = New FedEx_ShipService.SignatureOptionDetail
        With create_SignatureOptionDetail
            .OptionType = GetSignatureOptionType(obj.Description)
            '.SignatureReleaseNumber
        End With
    End Function


#Region "International"
    Private Function create_CustomsClearanceDetail(ByVal obj As _baseShipment) As FedEx_ShipService.CustomsClearanceDetail
        create_CustomsClearanceDetail = New FedEx_ShipService.CustomsClearanceDetail
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
                    Dim commoditis(obj.CommInvoice.CommoditiesList.Count - 1) As FedEx_ShipService.Commodity
                    For i As Integer = 0 To obj.CommInvoice.CommoditiesList.Count - 1
                        Dim commodity As _baseCommodities = obj.CommInvoice.CommoditiesList(i)
                        commoditis(i) = create_Commodity(commodity, comminv.CurrencyType)
                    Next i
                    .Commodities = commoditis
                End If
            End With
        End If
    End Function
    Private Function create_ExportDetail(ByVal obj As _baseCommInvoice) As FedEx_ShipService.ExportDetail
        create_ExportDetail = New FedEx_ShipService.ExportDetail
        With create_ExportDetail
            Dim typeB13A As New FedEx_ShipService.B13AFilingOptionType
            If GetB13AFilingOptionType(obj.B13AFilingOption, typeB13A) Then
                .B13AFilingOptionSpecified = True
                .B13AFilingOption = typeB13A
                If .B13AFilingOption = FedEx_ShipService.B13AFilingOptionType.FILED_ELECTRONICALLY Then
                    .ExportComplianceStatement = "V121245451XCVXCBNBV1253" ' test only
                ElseIf .B13AFilingOption = FedEx_ShipService.B13AFilingOptionType.SUMMARY_REPORTING Then
                    .ExportComplianceStatement = "DSGFH12" ' test only
                ElseIf .B13AFilingOption = FedEx_ShipService.B13AFilingOptionType.NOT_REQUIRED Then
                    .ExportComplianceStatement = "NO EEI 30.37(f)"
                End If
            End If
        End With
    End Function
    Private Function create_CommercialInvoice(ByVal obj As _baseCommInvoice) As FedEx_ShipService.CommercialInvoice
        create_CommercialInvoice = New FedEx_ShipService.CommercialInvoice
        With create_CommercialInvoice
            .Comments = {obj.Comments}
            .FreightCharge = create_Money(obj.FreightCharge, obj.CurrencyType)
            .TaxesOrMiscellaneousCharge = create_Money(obj.TaxesOrMiscCharge, obj.CurrencyType)
            .Purpose = GetPurposeOfShipmentType(obj.TypeOfContents)
        End With
    End Function
    Private Function create_Commodity(ByVal obj As _baseCommodities, ByVal currencytype As String) As FedEx_ShipService.Commodity
        create_Commodity = New FedEx_ShipService.Commodity
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
#Region "Get Shipping Types"
#Region "Types Required"
    Private Function GetPackagingType(ByVal srPackagingType As String) As FedEx_ShipService.PackagingType
        ''
        GetPackagingType = FedEx_ShipService.PackagingType.YOUR_PACKAGING '' assume.
        'Try
        If _Controls.Contains(srPackagingType, "Letter") Or _Controls.Contains(srPackagingType, "Env") Then
            GetPackagingType = FedEx_ShipService.PackagingType.FEDEX_ENVELOPE
        ElseIf _Controls.Contains(srPackagingType, "Box") And _Controls.Contains(srPackagingType, "10kg") Then
            GetPackagingType = FedEx_ShipService.PackagingType.FEDEX_10KG_BOX
        ElseIf _Controls.Contains(srPackagingType, "Box") And _Controls.Contains(srPackagingType, "25kg") Then
            GetPackagingType = FedEx_ShipService.PackagingType.FEDEX_25KG_BOX

        ElseIf _Controls.Contains(srPackagingType, "Box") AndAlso _Controls.Contains(srPackagingType, "Extra") AndAlso _Controls.Contains(srPackagingType, "Large") Then
            GetPackagingType = FedEx_ShipService.PackagingType.FEDEX_EXTRA_LARGE_BOX
        ElseIf _Controls.Contains(srPackagingType, "Box") AndAlso _Controls.Contains(srPackagingType, "Large") Then
            GetPackagingType = FedEx_ShipService.PackagingType.FEDEX_LARGE_BOX
        ElseIf _Controls.Contains(srPackagingType, "Box") AndAlso _Controls.Contains(srPackagingType, "Medium") Then
            GetPackagingType = FedEx_ShipService.PackagingType.FEDEX_MEDIUM_BOX
        ElseIf _Controls.Contains(srPackagingType, "Box") AndAlso _Controls.Contains(srPackagingType, "Small") Then
            GetPackagingType = FedEx_ShipService.PackagingType.FEDEX_SMALL_BOX

        ElseIf _Controls.Contains(srPackagingType, "Box") Then
            GetPackagingType = FedEx_ShipService.PackagingType.FEDEX_BOX
        ElseIf _Controls.Contains(srPackagingType, "Pak") Then
            GetPackagingType = FedEx_ShipService.PackagingType.FEDEX_PAK
        ElseIf _Controls.Contains(srPackagingType, "Tube") Then
            GetPackagingType = FedEx_ShipService.PackagingType.FEDEX_TUBE
        End If
        ''
    End Function
    Private Function GetServiceType(ByVal serviceABBR As String) As FedEx_ShipService.ServiceType
        ''
        GetServiceType = FedEx_ShipService.ServiceType.GROUND_HOME_DELIVERY '' assume.
        ''
        Select Case serviceABBR
            Case FedEx.Ground, FedEx.CanadaGround : GetServiceType = FedEx_ShipService.ServiceType.FEDEX_GROUND
            Case FedEx.FirstOvernight : GetServiceType = FedEx_ShipService.ServiceType.FIRST_OVERNIGHT
            Case FedEx.SecondDay : GetServiceType = FedEx_ShipService.ServiceType.FEDEX_2_DAY
            Case FedEx.SecondDayAM : GetServiceType = FedEx_ShipService.ServiceType.FEDEX_2_DAY_AM
            Case FedEx.Priority : GetServiceType = FedEx_ShipService.ServiceType.PRIORITY_OVERNIGHT
            Case FedEx.Standard : GetServiceType = FedEx_ShipService.ServiceType.STANDARD_OVERNIGHT
            Case FedEx.Saver : GetServiceType = FedEx_ShipService.ServiceType.FEDEX_EXPRESS_SAVER
            Case FedEx.Intl_First : GetServiceType = FedEx_ShipService.ServiceType.INTERNATIONAL_FIRST ''EUROPE_FIRST_INTERNATIONAL_PRIORITY
            Case FedEx.Intl_Priority : GetServiceType = FedEx_ShipService.ServiceType.INTERNATIONAL_PRIORITY ''INTERNATIONAL_PRIORITY_FREIGHT
            Case FedEx.Intl_Economy : GetServiceType = FedEx_ShipService.ServiceType.INTERNATIONAL_ECONOMY ''INTERNATIONAL_ECONOMY_FREIGHT
            Case FedEx.Freight_1Day : GetServiceType = FedEx_ShipService.ServiceType.FEDEX_1_DAY_FREIGHT
            Case FedEx.Freight_2Day : GetServiceType = FedEx_ShipService.ServiceType.FEDEX_2_DAY_FREIGHT
            Case FedEx.Freight_3Day : GetServiceType = FedEx_ShipService.ServiceType.FEDEX_3_DAY_FREIGHT
        End Select
        ''
    End Function
    Private Function GetSignatureOptionType(ByVal signatureOption As String) As FedEx_ShipService.SignatureOptionType
        ''
        GetSignatureOptionType = FedEx_ShipService.SignatureOptionType.NO_SIGNATURE_REQUIRED '' assume.
        Select Case signatureOption
            Case "Adult Signature" : GetSignatureOptionType = FedEx_ShipService.SignatureOptionType.ADULT
            Case "Direct Signature", "Direct Sig. - No Charge" : GetSignatureOptionType = FedEx_ShipService.SignatureOptionType.DIRECT
            Case "Indirect Signature" : GetSignatureOptionType = FedEx_ShipService.SignatureOptionType.INDIRECT
            Case "Del.Conf at No Charge" : GetSignatureOptionType = FedEx_ShipService.SignatureOptionType.SERVICE_DEFAULT
        End Select
        ''
    End Function
    Private Function GetCarrierCodeType(ByVal serviceABBR As String) As FedEx_ShipService.CarrierCodeType
        ''
        GetCarrierCodeType = FedEx_ShipService.CarrierCodeType.FDXG '' assume ground.
        ''
        Select Case serviceABBR
            Case FedEx.Priority, FedEx.FirstOvernight, FedEx.SecondDay, FedEx.SecondDayAM, FedEx.Standard, FedEx.Saver
                GetCarrierCodeType = FedEx_ShipService.CarrierCodeType.FDXE  ' FedEx Express
            Case FedEx.Intl_First, FedEx.Intl_Priority, FedEx.Intl_Economy
                GetCarrierCodeType = FedEx_ShipService.CarrierCodeType.FDXE ' FedEx Express
            Case FedEx.Ground, FedEx.CanadaGround
                GetCarrierCodeType = FedEx_ShipService.CarrierCodeType.FDXG ' FedEx Ground
            Case FedEx.Freight_1Day, FedEx.Freight_2Day, FedEx.Freight_3Day
                GetCarrierCodeType = FedEx_ShipService.CarrierCodeType.FXFR  ' FedEx Freight
        End Select
        ' If the CarrierCode is left blank, Express and Ground (if applicable/available) are returned in the reply.
        ''
    End Function
    Private Function GetCodAddTransportationChargesType(ByVal optionType As Integer) As FedEx_ShipService.CodAddTransportationChargeBasisType
        GetCodAddTransportationChargesType = FedEx_ShipService.CodAddTransportationChargeBasisType.COD_SURCHARGE ' assume.
        Select Case optionType
            Case CODChargeType_ADD_ACCOUNT_NET_CHARGE : GetCodAddTransportationChargesType = FedEx_ShipService.CodAddTransportationChargeBasisType.NET_CHARGE
            Case CODChargeType_ADD_ACCOUNT_NET_FREIGHT : GetCodAddTransportationChargesType = FedEx_ShipService.CodAddTransportationChargeBasisType.NET_FREIGHT
            Case CODChargeType_ADD_ACCOUNT_TOTAL_CUSTOMER_CHARGE : GetCodAddTransportationChargesType = FedEx_ShipService.CodAddTransportationChargeBasisType.TOTAL_CUSTOMER_CHARGE
        End Select
    End Function
    Private Function GetCodCollectionType(ByVal optionType As Integer) As FedEx_ShipService.CodCollectionType
        GetCodCollectionType = FedEx_ShipService.CodCollectionType.GUARANTEED_FUNDS ' assume.
        Select Case optionType
            Case CODPaymentType_ANY : GetCodCollectionType = FedEx_ShipService.CodCollectionType.ANY
            Case CODPaymentType_CASH : GetCodCollectionType = FedEx_ShipService.CodCollectionType.CASH
        End Select
    End Function
    Private Function GetHomeDeliveryPremiumType(ByVal optionType As String) As FedEx_ShipService.HomeDeliveryPremiumType
        GetHomeDeliveryPremiumType = FedEx_ShipService.HomeDeliveryPremiumType.EVENING
        If _Controls.Contains(optionType, "APPOINTMENT") Then
            GetHomeDeliveryPremiumType = FedEx_ShipService.HomeDeliveryPremiumType.APPOINTMENT
        ElseIf _Controls.Contains(optionType, "DATE") AndAlso _Controls.Contains(optionType, "CERTAIN") Then
            GetHomeDeliveryPremiumType = FedEx_ShipService.HomeDeliveryPremiumType.DATE_CERTAIN
        End If
    End Function
    Private Function GetPaymentType(ByVal type As String, ByVal shipment As _baseShipment, ByRef payor As _baseContact) As FedEx_ShipService.PaymentType
        GetPaymentType = FedEx_ShipService.PaymentType.SENDER
        ''ol#1.1.42(10/23)... FedEx International Shipping Charges must be assigned to Third-Party and Duties & Taxes must be assigned to Receiver party.
        ''  ''ol#1.1.34(8/13)... Force FedEx payment type as 'Sender' if Shipper and ShipTo contact country codes are 'US'.
        ''  If Not shipment.ShipToContact.CountryCode = "US" And Not shipment.ShipperContact.CountryCode = "US" Then ' Sender by Default in US
        If Not (shipment.ShipToContact.CountryCode = "US" And shipment.ShipperContact.CountryCode = "US") Then
            Select Case type
                Case "ACCOUNT" : GetPaymentType = FedEx_ShipService.PaymentType.ACCOUNT
                    payor = shipment.ShipperContact
                Case "COLLECT" : GetPaymentType = FedEx_ShipService.PaymentType.COLLECT
                    payor = shipment.ShipToContact
                Case "RECIPIENT" : GetPaymentType = FedEx_ShipService.PaymentType.RECIPIENT
                    payor = shipment.ShipToContact
                Case "RECIPIENT-TEST-ONLY" : GetPaymentType = FedEx_ShipService.PaymentType.RECIPIENT
                    payor = shipment.ShipToContact
                Case "THIRD_PARTY" : GetPaymentType = FedEx_ShipService.PaymentType.THIRD_PARTY
                    payor = shipment.ShipperContact
            End Select
        Else
            payor = shipment.ShipperContact
        End If
    End Function
    Private Function GetInternationalDocumentContentType(ByVal isShippingDocumentsOnly As Boolean) As FedEx_ShipService.InternationalDocumentContentType
        ''
        If isShippingDocumentsOnly Then
            GetInternationalDocumentContentType = FedEx_ShipService.InternationalDocumentContentType.DOCUMENTS_ONLY
        Else
            GetInternationalDocumentContentType = FedEx_ShipService.InternationalDocumentContentType.NON_DOCUMENTS
        End If
        ''
    End Function
    Private Function GetPurposeOfShipmentType(ByVal type As String) As FedEx_ShipService.PurposeOfShipmentType
        GetPurposeOfShipmentType = FedEx_ShipService.PurposeOfShipmentType.GIFT
        Select Case type
            Case _Controls.Contains(type, "Sample")
                GetPurposeOfShipmentType = FedEx_ShipService.PurposeOfShipmentType.SAMPLE
            Case _Controls.Contains(type, "Return") AndAlso _Controls.Contains(type, "Goods")
                GetPurposeOfShipmentType = FedEx_ShipService.PurposeOfShipmentType.REPAIR_AND_RETURN
            Case _Controls.Contains(type, "Other")
                GetPurposeOfShipmentType = FedEx_ShipService.PurposeOfShipmentType.PERSONAL_EFFECTS
            Case _Controls.Contains(type, "Documents")
                GetPurposeOfShipmentType = FedEx_ShipService.PurposeOfShipmentType.NOT_SOLD
        End Select
    End Function
#End Region
#Region "Types Optional"
    Private Function GetShipmentSpecialServiceType(ByVal optionType As String, ByRef type As FedEx_ShipService.ShipmentSpecialServiceType) As Boolean
        ''
        GetShipmentSpecialServiceType = True ' assume required
        If _Controls.Contains(optionType, "Rate") AndAlso _Controls.Contains(optionType, "One") Then
            type = FedEx_ShipService.ShipmentSpecialServiceType.FEDEX_ONE_RATE
        ElseIf _Controls.Contains(optionType, "COD", True) Then
            type = FedEx_ShipService.ShipmentSpecialServiceType.COD
        ElseIf _Controls.Contains(optionType, "Dry") AndAlso _Controls.Contains(optionType, "Ice") Then
            type = FedEx_ShipService.ShipmentSpecialServiceType.DRY_ICE
        ElseIf _Controls.Contains(optionType, "EMAIL") Then
            'type = ShipmentSpecialServiceType.EMAIL_NOTIFICATION
        ElseIf _Controls.Contains(optionType, "Home") AndAlso _Controls.Contains(optionType, "Delivery") Then
            type = FedEx_ShipService.ShipmentSpecialServiceType.HOME_DELIVERY_PREMIUM
        ElseIf _Controls.Contains(optionType, "Saturday") AndAlso _Controls.Contains(optionType, "Delivery") Then
            type = FedEx_ShipService.ShipmentSpecialServiceType.SATURDAY_DELIVERY
        ElseIf _Controls.Contains(optionType, "Saturday") AndAlso _Controls.Contains(optionType, "Pickup") Then
            type = FedEx_ShipService.ShipmentSpecialServiceType.SATURDAY_PICKUP
            'ElseIf _Controls.Contains(optionType, "Weekday") AndAlso _Controls.Contains(optionType, "Delivery") Then
            '    type = "WEEKDAY_DELIVERY"
        ElseIf _Controls.Contains(optionType, "HOLD") AndAlso _Controls.Contains(optionType, "LOCATION") Then
            type = FedEx_ShipService.ShipmentSpecialServiceType.HOLD_AT_LOCATION
        ElseIf _Controls.Contains(optionType, "Future") AndAlso _Controls.Contains(optionType, "Day") Then
            type = FedEx_ShipService.ShipmentSpecialServiceType.FUTURE_DAY_SHIPMENT
        ElseIf _Controls.Contains(optionType, "INSIDE") AndAlso _Controls.Contains(optionType, "DELIVERY") Then
            type = FedEx_ShipService.ShipmentSpecialServiceType.INSIDE_DELIVERY
        ElseIf _Controls.Contains(optionType, "INSIDE") AndAlso _Controls.Contains(optionType, "PICKUP") Then
            type = FedEx_ShipService.ShipmentSpecialServiceType.INSIDE_PICKUP
        Else
            GetShipmentSpecialServiceType = False ' optional
        End If
        'Case "x" : tmp = "THIRD_PARTY_CONSIGNEE"
        'Case "x" : tmp = "RETURN_SHIPMENT"
        'Case "x" : tmp = "HOLD_SATURDAY"
        'Case "x" : tmp = "BROKER_SELECT_OPTION"
    End Function
    Private Function GetPackageSpecialServiceType(ByVal optionType As String, ByRef type As FedEx_ShipService.PackageSpecialServiceType) As Boolean
        ''
        GetPackageSpecialServiceType = True ' assume required
        If _Controls.Contains(optionType, "COD", True) Then
            type = FedEx_ShipService.PackageSpecialServiceType.COD
        ElseIf _Controls.Contains(optionType, "Dry") AndAlso _Controls.Contains(optionType, "Ice") Then
            type = FedEx_ShipService.PackageSpecialServiceType.DRY_ICE
        ElseIf _Controls.Contains(optionType, "NON_STANDARD_CONTAINER") Then
            type = FedEx_ShipService.PackageSpecialServiceType.NON_STANDARD_CONTAINER
        ElseIf _Controls.Contains(optionType, "SIGNATURE") Then
            type = FedEx_ShipService.PackageSpecialServiceType.SIGNATURE_OPTION
        Else
            GetPackageSpecialServiceType = False ' optional
        End If
    End Function
    Private Function GetB13AFilingOptionType(ByVal optionType As String, ByRef type As FedEx_ShipService.B13AFilingOptionType) As Boolean
        GetB13AFilingOptionType = (Not 0 = optionType.Length) ' assume required
        Select Case optionType
            Case "FILED_ELECTRONICALLY" : type = FedEx_ShipService.B13AFilingOptionType.FILED_ELECTRONICALLY
            Case "MANUALLY_ATTACHED" : type = FedEx_ShipService.B13AFilingOptionType.MANUALLY_ATTACHED
            Case "SUMMARY_REPORTING" : type = FedEx_ShipService.B13AFilingOptionType.SUMMARY_REPORTING
            Case Else : type = FedEx_ShipService.B13AFilingOptionType.NOT_REQUIRED
        End Select
    End Function
#End Region
#End Region

#Region "Delete Package"
    Public Function Delete_Package(ByVal srSetup As Object, ByVal packageID As String, ByVal trackingNo As String, ByRef vb_response As Object) As Boolean
        Delete_Package = False
        Try
            Dim webservice As New FedEx_ShipService.ShipService
            webservice.Url = _FedExWeb.objFedEx_Setup.Connect2ServerURL '"https://ws.fedex.com:443/web-services"
            Dim shipService As New FedEx_ShipService.DeleteShipmentRequest
            '
            Dim webauth As New FedEx_ShipService.WebAuthenticationDetail
            Dim webauth_csp As New FedEx_ShipService.WebAuthenticationCredential
            Dim webauth_user As New FedEx_ShipService.WebAuthenticationCredential
            If create_WebAuthenticationCredential(_FedExWeb.objFedEx_Setup.Web_CspCredential_Key, _FedExWeb.objFedEx_Setup.Web_CspCredential_Pass, webauth_csp) Then
                If create_WebAuthenticationCredential(_FedExWeb.objFedEx_Setup.Web_UserCredential_Key, _FedExWeb.objFedEx_Setup.Web_UserCredential_Pass, webauth_user) Then
                    webauth.ParentCredential = webauth_csp
                    webauth.UserCredential = webauth_user
                    shipService.WebAuthenticationDetail = webauth
                    '
                    Dim client As New FedEx_ShipService.ClientDetail
                    If create_ClientDetail(client) Then
                        shipService.ClientDetail = client
                        '
                        Dim version As New FedEx_ShipService.VersionId
                        If create_Version("ship", 21, 0, 0, version) Then
                            shipService.Version = version
                            '
                            shipService.TrackingId.TrackingIdType = FedEx_ShipService.TrackingIdType.FEDEX
                            shipService.TrackingId.TrackingIdTypeSpecified = True
                            shipService.TrackingId.TrackingNumber = trackingNo
                            shipService.DeletionControl = FedEx_ShipService.DeletionControlType.DELETE_ONE_PACKAGE
                            '
                            If Not _Files.IsFileExist(String.Format("{0}\no_xml.txt", _FedExWeb.objFedEx_Setup.Path_SaveDocXML), False) Then
                                Dim xdoc As New Xml.XmlDocument
                                xdoc.LoadXml(serializeDeleteRequest2string(shipService))
                                xdoc.Save(_FedExWeb.objFedEx_Setup.Path_SaveDocXML & "\" & packageID & "_DeleteRequest.xml") ' shipment ID
                            End If
                            '
                            Delete_Package = process_DeleteAPackage_Response(srSetup, shipService, webservice, packageID, vb_response)
                            '
                        End If
                    End If
                End If
            End If
            '
        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to create a 'Delete A Package' request...")
        End Try
    End Function
    Private Function process_DeleteAPackage_Response(ByVal srSetup As Object, ByVal shipService As FedEx_ShipService.DeleteShipmentRequest, ByVal webservice As FedEx_ShipService.ShipService, ByVal packageID As String, ByRef vb_response As Object) As Boolean
        process_DeleteAPackage_Response = False ' assume.
        Try
            Dim shipResponse As FedEx_ShipService.ShipmentReply = webservice.deleteShipment(shipService)

            If Not _Files.IsFileExist(String.Format("{0}\no_xml.txt", _FedExWeb.objFedEx_Setup.Path_SaveDocXML), False) Then
                Dim xdoc As New Xml.XmlDocument
                xdoc.LoadXml(serializeDeleteResponse2string(shipResponse))
                xdoc.Save(_FedExWeb.objFedEx_Setup.Path_SaveDocXML & "\" & packageID & "_DeleteReply.xml")
            End If
            process_DeleteAPackage_Response = True ' got the response!

            ' Result
            If shipResponse.Notifications IsNot Nothing Then
                For n As Integer = 0 To shipResponse.Notifications.Length - 1
                    Dim notify As FedEx_ShipService.Notification = shipResponse.Notifications(n)
                    If Not notify.Code = "0000" Then
                        ' Error:
                        vb_response.ShipmentAlerts.Add(notify.Message)
                    End If
                    '_Debug.Print_(notify.Severity.ToString)
                    '_Debug.Print_(notify.Source)
                    '_Debug.Print_(notify.Code)
                    '_Debug.Print_(notify.Message)
                Next n
            End If
            '
        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message) : _Debug.Print_(ex.Detail.LastChild.InnerText)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to get a response for Delete-A-Package request...")
        End Try
    End Function

    Private Function serializeDeleteRequest2string(obj As FedEx_ShipService.DeleteShipmentRequest) As String
        Dim xml_serializer As New XmlSerializer(GetType(FedEx_ShipService.DeleteShipmentRequest))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeDeleteRequest2string = string_writer.ToString()
        string_writer.Close()
    End Function
    Private Function serializeDeleteResponse2string(obj As FedEx_ShipService.ShipmentReply) As String
        Dim xml_serializer As New XmlSerializer(GetType(FedEx_ShipService.ShipmentReply))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeDeleteResponse2string = string_writer.ToString()
        string_writer.Close()
    End Function

#End Region

End Module
