Imports System.Xml.Serialization
Imports System.IO

Public Class FedEx_Setup
    '
    Public FreightBox_AccountNumber As String
    Public FreightBox_UserName As String
    Public FreightBox_Password As String
    Public FreightBox_MeterNumber As String
    Public Regular_AccountNumber As String
    Public Regular_UserName As String
    Public Regular_Password As String
    Public Regular_MeterNumber As String

    Public Path_SOAPEnvelope As String
    Public Path_SaveDocXML As String
    Public Web_CspCredential_Key As String
    Public Web_CspCredential_Pass As String
    Public Web_UserCredential_Key As String
    Public Web_UserCredential_Pass As String
    Public CSP_SolutionId As String
    Public CSP_Type As String
    Public Client_AccountNumber As String
    Public Client_MeterNumber As String
    Public Client_ProductId As String
    Public Client_ProductVersion As String
    Public ApplicationId As String
    Public OriginLocationId As String
    Public DropoffType As String
    Public PaymentType As String
    Public LabelImageType As String
    Public LabelFormatType As String
    Public LabelStockType As String

    Public Connect2ServerURL As String
    Public Csp_AccountNumber As String
    Public ClientCountryCode As String
    '
    Public Const CODPaymentType_FUNDS As Integer = 0
    Public Const CODPaymentType_CASH As Integer = 1
    Public Const CODPaymentType_ANY As Integer = 2
    Public Const CODChargeType_ADD_ACCOUNT_COD_SURCHARGE As Integer = 0
    Public Const CODChargeType_ADD_ACCOUNT_NET_CHARGE As Integer = 1
    Public Const CODChargeType_ADD_ACCOUNT_NET_FREIGHT As Integer = 2
    Public Const CODChargeType_ADD_ACCOUNT_TOTAL_CUSTOMER_CHARGE As Integer = 3
    ''
    Public Const IntTermsOfSale_CFR_OR_CPT As Integer = 1
    Public Const IntTermsOfSale_CIF_OR_CIP As Integer = 2
    Public Const IntTermsOfSale_DDP As Integer = 3
    Public Const IntTermsOfSale_DDU As Integer = 4
    Public Const IntTermsOfSale_EXW As Integer = 5
    Public Const IntTermsOfSale_FOB_OR_FCA As Integer = 6
    '
    Sub New(ByVal isFreightBox As Boolean)
        '
        ' 2017 FedEx Test Credentials:
        '
        '•	Parent Credential Key: 2U39xPqXhhm3DdiA
        '•	Parent Credential Password: h3ZXnSQZ5Np7AxKzPaQUurRv6
        '•	US Test Account #: 609152567 (Address: 7901 Brewerton Road, Cicero, NY, 13039, US)
        '•	CA Test Account #: 609167505 (Address: 5985 EXPLORER DR, MISSISSAUGA, ON, L4W5K6, CA)
        '•	ECOD Account #: 222326460 (Address: 500 THORNHILL LN, AURORA, OH, 44202, US)
        '•	US Freight Account #: 630081440 (Address: 1202 Chalet Lane, Harrison, AR, 72601, US)
        '•	CA Freight Account #: 602091147 (Address: 7075 ORDAN DR, MISSISSAUGA, ON,L5T1K6, CA)
        '•	Freight Third Party (Bill-To) Account #: 510051408 (Address: 2000 ARKANSAS 7, Harrison, AR, 72602, US) 
        '•	Version Capture Code: SRGC7000
        '•	CspSolutionId: 059
        '•	Test URL: https://wsbeta.fedex.com:443/web-services
        '
        '  Test Credentials
        'm_Connect2ServerURL = "https://wsbeta.fedex.com:443/web-services"
        'm_CspCredential_Key = "2U39xPqXhhm3DdiA" '' CSP credential Key
        'm_CspCredential_Pass = "h3ZXnSQZ5Np7AxKzPaQUurRv6" '' CSP credential Password
        'm_Path_SOAPEnvelope = gDBpath & "\FedEx"
        'm_Path_SaveDocXML = "C:\Shiprite\Test\FedEx"
        '
        '  Production Credentials
        Connect2ServerURL = "https://ws.fedex.com:443/web-services" '' Product URL
        Web_CspCredential_Key = "QFdKk9EIx1jJ5Tdc" '' CSP credential Key
        Web_CspCredential_Pass = "yYbSG24IEfUD8ALLUYcg3Uurb" '' CSP credential Password
        Path_SOAPEnvelope = gDBpath & "\FedEx"
        Path_SaveDocXML = String.Format("{0}\FedEx\InOut", gDBpath)
        ''
        Client_ProductId = "SRGC" '' Client Product Id
        Client_ProductVersion = "6000" '"7232" '' Client Product version
        ''
        CSP_SolutionId = "059" '' Solution Type: 059
        CSP_Type = "CERTIFIED_SOLUTION_PROVIDER"
        ''
        If _Debug.IsINHOUSE Then
            '
            Connect2ServerURL = "https://wsbeta.fedex.com:443/web-services"
            Web_CspCredential_Key = "M4stSU9Qm5zUbQrW" '' CSP credential Key
            Web_CspCredential_Pass = "o8WH3w4GGjn2TuM2jY5MyxapN" '' CSP credential Password
            '
            If isFreightBox Then
                '
                Csp_AccountNumber = "929819136"
                '
                '* Register user returned values:
                Web_UserCredential_Key = "fHun2ijP7QPUMcSk"
                Web_UserCredential_Pass = "83NyBOx0IPK2OWErYSwDY17Jt"
                Client_MeterNumber = "113075164"
                '
            Else
                '
                Csp_AccountNumber = "630081440"
                '
                '* Register user returned values:
                Web_UserCredential_Key = "1ewA9MMSeca7JsIp"
                Web_UserCredential_Pass = "bb3njwrd0SPXRFF2k7vEZ22Xa"
                Client_MeterNumber = "119039368"
                '
            End If
            '
        Else
            '
            If isFreightBox Then
                '
                Csp_AccountNumber = General.GetPolicyData(gShipriteDB, "FedExFreightBox_AccountNumber")
                '
                '* Register user returned values:
                Web_UserCredential_Key = General.GetPolicyData(gShipriteDB, "FedExFreightBox_UserName")
                Web_UserCredential_Pass = General.GetPolicyData(gShipriteDB, "FedExFreightBox_Password")
                Client_MeterNumber = General.GetPolicyData(gShipriteDB, "FedExFreightBox_MeterNumber")
                '
            Else
                '
                Csp_AccountNumber = General.GetPolicyData(gShipriteDB, "FedExAccountNumber")
                '
                '* Register user returned values:
                Web_UserCredential_Key = General.GetPolicyData(gShipriteDB, "FedExIOPort")
                Web_UserCredential_Pass = General.GetPolicyData(gShipriteDB, "FedExPassword")
                Client_MeterNumber = General.GetPolicyData(gShipriteDB, "FedExMeter")
                '
            End If
            '
        End If
        ''
        DropoffType = "REGULAR_PICKUP"
        If isFreightBox Then
            '
            PaymentType = "RECIPIENT"
            LabelImageType = "Laser"
            LabelFormatType = "FEDEX_FREIGHT_STRAIGHT_BILL_OF_LADING" '' the other option: "LABEL_DATA_ONLY"
            LabelStockType = "Thermal 4x6"
            '
        Else
            PaymentType = "SENDER"
            LabelFormatType = "COMMON2D" '' the other option: "LABEL_DATA_ONLY"
            ''
            If "Laser" = General.GetPolicyData(gReportsDB, "FedExLabelType") Then
                LabelImageType = "PDF Image"
                LabelStockType = "" '' optional
            Else
                LabelImageType = "Eltron Thermal"
                LabelStockType = "Thermal 4x6"
            End If
            '
        End If
        ''
        Client_AccountNumber = Csp_AccountNumber
        ''
        OriginLocationId = General.GetPolicyData(gShipriteDB, "FedExHAL_LocationID") ' "SNAP2" ' our FedEx HAL Location ID
        ApplicationId = General.GetPolicyData(gShipriteDB, "FedExHAL_AgentID") ' "5202506" ' our FedEx HAL Agent ID
        ''
    End Sub
End Class

Public Module _FedExWeb

    Public objFedEx_Setup As FedEx_Setup
    Public objFedEx_Regular_Setup As FedEx_Setup
    Public objFedEx_Freight_Setup As FedEx_Setup
    Public Const WebServTitle As String = "FedEx Web Services" '"FedEx® Web Services"
    Public IsLabelPrintedSuccessfully As Boolean
    Public IsEnabled_OneRate As Boolean

#Region "Create Web Authentication Objects"
    Public Function create_WebAuthenticationCredential(ByVal akey As String, ByVal pass As String, ByRef obj As Object) As Boolean
        With obj
            .Key = akey
            .Password = pass
            create_WebAuthenticationCredential = (.Key = akey)
        End With
    End Function
    Public Function create_ClientDetail(ByRef obj As Object) As Boolean
        With obj
            .AccountNumber = _FedExWeb.objFedEx_Setup.Csp_AccountNumber
            '.ClientProductId = _FedExWeb.objFedEx_Setup.Client_ProductId
            '.ClientProductVersion = _FedExWeb.objFedEx_Setup.Client_ProductVersion
            .MeterNumber = _FedExWeb.objFedEx_Setup.Client_MeterNumber
            ''ol#1.2.53(7/6)... FedEx removed '<IntegratorId>' tag from all services.
            ''.IntegratorId = _FedExWeb.objFedEx_Setup.Client_ProductId & _FedExWeb.objFedEx_Setup.Client_ProductVersion
            '.Localization

            create_ClientDetail = (.AccountNumber = _FedExWeb.objFedEx_Setup.Csp_AccountNumber)
        End With
    End Function
    Public Function create_Version(ByVal aserviceId As String, ByVal amajor As Integer, ByVal aintermediate As Integer, ByVal aminor As Integer, ByRef obj As Object) As Boolean
        With obj
            .ServiceId = aserviceId
            .Major = amajor
            .Intermediate = aintermediate
            .Minor = aminor
            create_Version = (.ServiceId = aserviceId)
        End With
    End Function
#End Region
#Region "Create Common Objects"
    Public Function create_Contact(ByVal obj As _baseContact, ByRef contact As Object) As Boolean
        With contact
            .CompanyName = obj.CompanyName
            '.ContactId = obj.ContactID.ToString
            .EMailAddress = obj.Email
            .FaxNumber = obj.Fax
            .PersonName = obj.FNameLName
            .PhoneNumber = obj.Tel
            create_Contact = True
        End With
    End Function
    Public Function create_Address(ByVal obj As _baseContact, ByRef address As Object) As Boolean
        With address
            .City = obj.City
            .CountryCode = obj.CountryCode
            .CountryName = obj.Country
            .PostalCode = obj.Zip
            .Residential = obj.Residential
            .ResidentialSpecified = True
            .StateOrProvinceCode = obj.State
            .StreetLines = {obj.Addr1, obj.Addr2}
            create_Address = True
        End With
    End Function

#End Region
#Region "Create Registration Objects"
    Private Function create_ContactParty_RegistrationService(ByVal obj As _baseContact, ByVal accountNo As String) As FedEx_RegistrationService.Party
        create_ContactParty_RegistrationService = New FedEx_RegistrationService.Party ' assume
        Dim address As New FedEx_RegistrationService.Address
        Dim contact As New FedEx_RegistrationService.Contact
        With create_ContactParty_RegistrationService
            If Not accountNo = String.Empty Then
                .AccountNumber = accountNo
            End If
            If create_RegAddress(obj, address) Then
                .Address = address
            End If
            If create_Contact(obj, contact) Then
                .Contact = contact
            End If
            '.Shipper.Tins
        End With
    End Function
    Public Function create_RegAddress(ByVal obj As _baseContact, ByRef address As Object) As Boolean
        With address
            .City = obj.City
            .CountryCode = obj.CountryCode
            .CountryName = obj.Country
            .PostalCode = obj.Zip
            ''ol#1.2.35(4/18)... Registration calls don't use Residential flag.
            '.Residential = obj.Residential
            '.ResidentialSpecified = True
            .StateOrProvinceCode = obj.State
            .StreetLines = {obj.Addr1, obj.Addr2}
            create_RegAddress = True
        End With
    End Function

#End Region
#Region "Create Shipment Objects"
    Private Function create_RequestObject(ByVal obj As _baseShipment, ByRef shipRequest As FedEx_ShipService.RequestedShipment, Optional ByVal isFreight As Boolean = False) As Boolean
        create_RequestObject = False ' assume.
        With shipRequest


            If obj.CarrierService.ShipDate < Today Then
                .ShipTimestamp = Today
            Else
                .ShipTimestamp = obj.CarrierService.ShipDate
            End If

            .DropoffType = FedEx_ShipService.DropoffType.REGULAR_PICKUP
            .ServiceType = FedEx_Data2XML.GetServiceType(obj.CarrierService.ServiceABBR)
            Dim package As _baseShipmentPackage = obj.Packages(0)
            '
            If isFreight Then
                .PackagingType = FedEx_ShipService.PackagingType.YOUR_PACKAGING
            Else
                .PackagingType = GetPackagingType(package.PackagingType)
            End If
            '
            .Shipper = create_ContactParty_ShipService(obj.ShipFromContact, _FedExWeb.objFedEx_Setup.Csp_AccountNumber)
            .Recipient = create_ContactParty_ShipService(obj.ShipToContact, String.Empty)
            .ShippingChargesPayment = create_Payment(_FedExWeb.objFedEx_Setup.PaymentType, obj)
            .SpecialServicesRequested = create_ShipmentSpecialServices(obj)
            .LabelSpecification = create_LabelSpecification()
            ''ol#1.1.76(10/9)... In request 'RateRequestTypes' must be equal 'LIST'.
            If "PREFERRED" = obj.RateRequestType Then
                .RateRequestTypes = {FedEx_ShipService.RateRequestType.PREFERRED}
            Else
                .RateRequestTypes = {FedEx_ShipService.RateRequestType.LIST}
            End If
            '
            'International:
            If Not obj.CarrierService.IsDomestic Then
                .CustomsClearanceDetail = create_CustomsClearanceDetail(obj)
            End If
            '
            create_RequestObject = True
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
            Dim contact As _baseContact = Nothing
            .PaymentType = FedEx_Data2XML.GetPaymentType(type, obj, contact)
            Dim accountNo As String = String.Empty
            If .PaymentType = FedEx_ShipService.PaymentType.SENDER Or .PaymentType = FedEx_ShipService.PaymentType.THIRD_PARTY Or type = "RECIPIENT-TEST-ONLY" Then
                accountNo = _FedExWeb.objFedEx_Setup.Client_AccountNumber
                If contact IsNot Nothing Then
                    .Payor = create_Payor(contact, accountNo)
                End If
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
                            If FedEx_Data2XML.GetShipmentSpecialServiceType(objServiceSurcharge.Name, type(i)) Then
                                Select Case type(i)
                                ''ol#1.2.53(7/6)... FedEx removed EMAIL_NOTIFICATION service in Ship_Service_v21.
                                ''Case FedEx_ShipService.ShipmentSpecialServiceType.EMAIL_NOTIFICATION
                                ''   .EMailNotificationDetail = create_EMailNotificationDetail(obj)
                                    Case FedEx_ShipService.ShipmentSpecialServiceType.COD
                                        Dim cod As _baseServiceSurchargeCOD = obj.CarrierService.ServiceSurchargeCOD
                                        .CodDetail = create_CodDetail(cod, obj.ShipperContact)
                                    Case FedEx_ShipService.ShipmentSpecialServiceType.HOLD_AT_LOCATION
                                        .HoldAtLocationDetail = create_HoldAtLocationDetail(obj.HoldAtLocation)
                                    Case FedEx_ShipService.ShipmentSpecialServiceType.HOME_DELIVERY_PREMIUM
                                        .HomeDeliveryPremiumDetail = create_HomeDeliveryPremiumDetail(obj, FedEx_Data2XML.GetHomeDeliveryPremiumType(objServiceSurcharge.Description))
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

    Private Function create_DangerousGoodsDetail(ByVal dgoods As _baseDangerousGoods) As FedEx_ShipService.DangerousGoodsDetail
        ''ol#1.2.43(12/13)... Dangerous Goods module was added.
        create_DangerousGoodsDetail = New FedEx_ShipService.DangerousGoodsDetail
        With create_DangerousGoodsDetail
            If dgoods.IsAccessible Then
                .Accessibility = FedEx_ShipService.DangerousGoodsAccessibilityType.ACCESSIBLE
            Else
                .Accessibility = FedEx_ShipService.DangerousGoodsAccessibilityType.INACCESSIBLE
            End If
            .AccessibilitySpecified = True
            .CargoAircraftOnly = dgoods.CargoAircraftOnly
            .CargoAircraftOnlySpecified = True
            Dim contn As New FedEx_ShipService.DangerousGoodsContainer
            contn.ContainerType = dgoods.ContainerType
            contn.NumberOfContainers = dgoods.NumberOfContainers.ToString
            'Dim hz As New FedEx_ShipService.HazardousCommodityContent

            .Containers = {contn}
        End With
    End Function
    Private Function create_CodDetail(ByVal cod As _baseServiceSurchargeCOD, ByVal contact As _baseContact) As FedEx_ShipService.CodDetail
        create_CodDetail = New FedEx_ShipService.CodDetail
        With create_CodDetail
            If cod.AddCOD2Total Then
                .AddTransportationChargesDetail = create_CodAddTransportationChargesDetail(cod)
            End If
            .CollectionType = FedEx_Data2XML.GetCodCollectionType(cod.PaymentType)
            .CodCollectionAmount = create_Money(cod.Amount, cod.CurrencyType)
            .CodRecipient = create_ContactParty_ShipService(contact, String.Empty)
        End With
    End Function
    Private Function create_CodAddTransportationChargesDetail(ByVal cod As _baseServiceSurchargeCOD) As FedEx_ShipService.CodAddTransportationChargesDetail
        create_CodAddTransportationChargesDetail = New FedEx_ShipService.CodAddTransportationChargesDetail
        With create_CodAddTransportationChargesDetail
            .ChargeBasisSpecified = True
            .ChargeBasis = FedEx_Data2XML.GetCodAddTransportationChargesType(cod.ChargeType)
            .ChargeBasisLevelSpecified = True
            .ChargeBasisLevel = FedEx_ShipService.ChargeBasisLevelType.CURRENT_PACKAGE
            .RateTypeBasisSpecified = True
            .RateTypeBasis = FedEx_ShipService.RateTypeBasisType.ACCOUNT
        End With
    End Function

    Private Function create_Money(ByVal amount As Double, currencytype As String) As FedEx_ShipService.Money
        create_Money = New FedEx_ShipService.Money
        With create_Money
            .Amount = amount
            .Currency = currencytype
        End With
    End Function
    Private Function create_TotalInsuredValue(ByVal shipment As _baseShipment) As FedEx_ShipService.Money
        ''ol#1.2.69(5/8)... FedEx Freight Box services were added.
        create_TotalInsuredValue = New FedEx_ShipService.Money
        With create_TotalInsuredValue
            .Currency = shipment.Packages(1).Currency_Type
            For i As Integer = 1 To shipment.Packages.Count
                Dim pack As _baseShipmentPackage = shipment.Packages(i)
                .Amount += pack.DeclaredValue
            Next i
        End With
    End Function
    Private Function create_Payment_Freight(ByVal type As String, ByVal obj As _baseShipment) As FedEx_ShipService.Payment
        ''ol#1.2.69(5/8)... FedEx Freight Box services were added.
        create_Payment_Freight = New FedEx_ShipService.Payment
        With create_Payment_Freight
            Dim contact As _baseContact = Nothing
            .PaymentType = FedEx_Data2XML.GetPaymentType_Freight(type, obj, contact)
            Dim accountNo As String = String.Empty
            If "SENDER" = type Then
                accountNo = _FedExWeb.objFedEx_Freight_Setup.Client_AccountNumber
            Else
                accountNo = contact.AccountNumber
            End If
            If contact IsNot Nothing Then
                .Payor = create_Payor(contact, accountNo)
            End If
        End With
    End Function
    Private Function create_TotalInsuredValue_Rate(ByVal shipment As _baseShipment) As FedEx_RateService.Money
        ''ol#1.2.69(5/8)... FedEx Freight Box services were added.
        create_TotalInsuredValue_Rate = New FedEx_RateService.Money
        With create_TotalInsuredValue_Rate
            .Currency = shipment.Packages(0).Currency_Type
            For i As Integer = 0 To shipment.Packages.Count - 1
                Dim pack As _baseShipmentPackage = shipment.Packages(i)
                .Amount += pack.DeclaredValue
            Next i
            .AmountSpecified = True
        End With
    End Function


    ''ol#1.2.53(7/6)... FedEx removed EMAIL_NOTIFICATION service in Ship_Service_v21.
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
            ''ol#1.2.69(5/8)... FedEx Freight Box services were added.
            If _Controls.Contains(_FedExWeb.objFedEx_Setup.LabelFormatType, "Freight") Then
                .LabelFormatType = FedEx_ShipService.LabelFormatType.FEDEX_FREIGHT_STRAIGHT_BILL_OF_LADING
            Else
                .LabelFormatType = FedEx_ShipService.LabelFormatType.COMMON2D
            End If
            .ImageTypeSpecified = True
            If _Controls.Contains(_FedExWeb.objFedEx_Setup.LabelImageType, "Thermal") Then
                .ImageType = FedEx_ShipService.ShippingDocumentImageType.EPL2
                .LabelStockType = FedEx_ShipService.LabelStockType.STOCK_4X6
                .LabelStockTypeSpecified = True
                .LabelPrintingOrientation = FedEx_ShipService.LabelPrintingOrientationType.BOTTOM_EDGE_OF_TEXT_FIRST
                .LabelPrintingOrientationSpecified = True
            Else
                .ImageType = FedEx_ShipService.ShippingDocumentImageType.PDF
                ''ol#1.2.69(5/8)... FedEx Freight Box services were added.
                .LabelStockType = FedEx_ShipService.LabelStockType.PAPER_LETTER
                .LabelStockTypeSpecified = True
                .LabelPrintingOrientation = FedEx_ShipService.LabelPrintingOrientationType.BOTTOM_EDGE_OF_TEXT_FIRST
                .LabelPrintingOrientationSpecified = True
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
            '' ''ol#1.2.33(3/28)... FedEx Cert: One Rate HAL is missing Location Type tag.
            ''.LocationType = FedEx_ShipService.FedExLocationType.FEDEX_EXPRESS_STATION
            ''.LocationTypeSpecified = True
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
    Private Function create_RequestedPackageLineItem(ByVal shipment As _baseShipment, ByVal siquenceno As Integer) As FedEx_ShipService.RequestedPackageLineItem
        create_RequestedPackageLineItem = New FedEx_ShipService.RequestedPackageLineItem
        Dim package As _baseShipmentPackage = shipment.Packages(siquenceno)
        With create_RequestedPackageLineItem
            .InsuredValue = create_Money(package.DeclaredValue, package.Currency_Type)
            .SequenceNumber = (siquenceno + 1).ToString
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
                            If FedEx_Data2XML.GetPackageSpecialServiceType(objServiceSurcharge.Name, type(i)) Then
                                Select Case type(i)
                                    Case FedEx_ShipService.PackageSpecialServiceType.SIGNATURE_OPTION
                                        .SignatureOptionDetail = create_SignatureOptionDetail(objServiceSurcharge)
                                    Case FedEx_ShipService.PackageSpecialServiceType.DRY_ICE
                                        .DryIceWeight = create_Weight(obj.DryIce.Weight, obj.DryIce.WeightUnits)
                                    Case FedEx_ShipService.PackageSpecialServiceType.NON_STANDARD_CONTAINER
                                    ' nothing required - just a flag
                                    Case FedEx_ShipService.PackageSpecialServiceType.COD
                                        .CodDetail = create_CodDetail(obj.COD, codContact)
                                    Case FedEx_ShipService.PackageSpecialServiceType.DANGEROUS_GOODS ''ol#1.2.43(12/13)... Dangerous Goods module was added.
                                        .DangerousGoodsDetail = create_DangerousGoodsDetail(obj.DangerousGoods)
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
            .OptionType = FedEx_Data2XML.GetSignatureOptionType(obj.Description)
            '.SignatureReleaseNumber
        End With
    End Function

    Private Function create_ContactAndAddress(ByVal obj As _baseContact) As FedEx_ShipService.ContactAndAddress
        ''ol#1.2.69(5/8)... FedEx Freight Box services were added.
        Dim address As New FedEx_ShipService.Address
        Dim contact As New FedEx_ShipService.Contact
        create_ContactAndAddress = New FedEx_ShipService.ContactAndAddress
        With create_ContactAndAddress
            If create_Contact(obj, contact) Then
                .Contact = contact
            End If
            If create_Address(obj, address) Then
                .Address = address
            End If
        End With
    End Function

    Private Function create_RequestedFreightLineItem(ByVal shipment As _baseShipment, ByVal siquenceno As Integer) As FedEx_ShipService.FreightShipmentLineItem
        ''ol#1.2.69(5/8)... FedEx Freight Box services were added.
        create_RequestedFreightLineItem = New FedEx_ShipService.FreightShipmentLineItem
        Dim package As _baseShipmentPackage = shipment.Packages(siquenceno)
        Dim objFreight As _baseFreight = package.Freight
        With create_RequestedFreightLineItem
            .FreightClass = _FedExWeb.get_FreightClassType(objFreight)
            .FreightClassSpecified = True
            .Packaging = get_Freight_PhysicalPackagingType(package)
            .PackagingSpecified = True
            .Description = objFreight.LTL_Freight_Description
            .Pieces = "1"
            .HandlingUnits = "1"
            .Weight = create_Weight(package.Weight_LBs, package.Weight_Units)
            .Dimensions = create_Dimensions(package)
        End With
    End Function
    Private Function get_Freight_PhysicalPackagingType(ByVal obj As _baseShipmentPackage) As FedEx_ShipService.PhysicalPackagingType
        ''ol#1.2.69(5/8)... FedEx Freight Box services were added.
        get_Freight_PhysicalPackagingType = FedEx_ShipService.PhysicalPackagingType.BOX ' assume.
        If _Controls.Contains(obj.PackagingType, "BAG") Then
            get_Freight_PhysicalPackagingType = FedEx_ShipService.PhysicalPackagingType.BAG
        ElseIf _Controls.Contains(obj.PackagingType, "BARREL") Then
            get_Freight_PhysicalPackagingType = FedEx_ShipService.PhysicalPackagingType.BARREL
        ElseIf _Controls.Contains(obj.PackagingType, "BASKET") Then
            get_Freight_PhysicalPackagingType = FedEx_ShipService.PhysicalPackagingType.BASKET
        ElseIf _Controls.Contains(obj.PackagingType, "BOX") Then
            get_Freight_PhysicalPackagingType = FedEx_ShipService.PhysicalPackagingType.BOX
        ElseIf _Controls.Contains(obj.PackagingType, "BUCKET") Then
            get_Freight_PhysicalPackagingType = FedEx_ShipService.PhysicalPackagingType.BUCKET
        ElseIf _Controls.Contains(obj.PackagingType, "BUNDLE") Then
            get_Freight_PhysicalPackagingType = FedEx_ShipService.PhysicalPackagingType.BUNDLE
        ElseIf _Controls.Contains(obj.PackagingType, "CARTON") Then
            get_Freight_PhysicalPackagingType = FedEx_ShipService.PhysicalPackagingType.CARTON
        ElseIf _Controls.Contains(obj.PackagingType, "CASE") Then
            get_Freight_PhysicalPackagingType = FedEx_ShipService.PhysicalPackagingType.CASE
        ElseIf _Controls.Contains(obj.PackagingType, "CONTAINER") Then
            get_Freight_PhysicalPackagingType = FedEx_ShipService.PhysicalPackagingType.CONTAINER
        ElseIf _Controls.Contains(obj.PackagingType, "CRATE") Then
            get_Freight_PhysicalPackagingType = FedEx_ShipService.PhysicalPackagingType.CRATE
        ElseIf _Controls.Contains(obj.PackagingType, "CYLINDER") Then
            get_Freight_PhysicalPackagingType = FedEx_ShipService.PhysicalPackagingType.CYLINDER
        ElseIf _Controls.Contains(obj.PackagingType, "DRUM") Then
            get_Freight_PhysicalPackagingType = FedEx_ShipService.PhysicalPackagingType.DRUM
        ElseIf _Controls.Contains(obj.PackagingType, "HAMPER") Then
            get_Freight_PhysicalPackagingType = FedEx_ShipService.PhysicalPackagingType.HAMPER
        ElseIf _Controls.Contains(obj.PackagingType, "PAIL") Then
            get_Freight_PhysicalPackagingType = FedEx_ShipService.PhysicalPackagingType.PAIL
        ElseIf _Controls.Contains(obj.PackagingType, "PALLET") Then
            get_Freight_PhysicalPackagingType = FedEx_ShipService.PhysicalPackagingType.PALLET
        ElseIf _Controls.Contains(obj.PackagingType, "PIECE") Then
            get_Freight_PhysicalPackagingType = FedEx_ShipService.PhysicalPackagingType.PIECE
        ElseIf _Controls.Contains(obj.PackagingType, "REEL") Then
            get_Freight_PhysicalPackagingType = FedEx_ShipService.PhysicalPackagingType.REEL
        ElseIf _Controls.Contains(obj.PackagingType, "ROLL") Then
            get_Freight_PhysicalPackagingType = FedEx_ShipService.PhysicalPackagingType.ROLL
        ElseIf _Controls.Contains(obj.PackagingType, "SKID") Then
            get_Freight_PhysicalPackagingType = FedEx_ShipService.PhysicalPackagingType.SKID
        ElseIf _Controls.Contains(obj.PackagingType, "TANK") Then
            get_Freight_PhysicalPackagingType = FedEx_ShipService.PhysicalPackagingType.TANK
        ElseIf _Controls.Contains(obj.PackagingType, "TUBE") Then
            get_Freight_PhysicalPackagingType = FedEx_ShipService.PhysicalPackagingType.TUBE
        End If
    End Function
    Private Function get_FreightClassType(ByVal obj As _baseFreight) As FedEx_ShipService.FreightClassType
        ''ol#1.2.69(5/8)... FedEx Freight Box services were added.
        get_FreightClassType = FedEx_ShipService.FreightClassType.CLASS_100 ' assume
        If _Controls.Contains(obj.LTL_Freight_Class, "050") Then
            get_FreightClassType = FedEx_ShipService.FreightClassType.CLASS_050
        ElseIf _Controls.Contains(obj.LTL_Freight_Class, "055") Then
            get_FreightClassType = FedEx_ShipService.FreightClassType.CLASS_055
        ElseIf _Controls.Contains(obj.LTL_Freight_Class, "060") Then
            get_FreightClassType = FedEx_ShipService.FreightClassType.CLASS_060
        ElseIf _Controls.Contains(obj.LTL_Freight_Class, "065") Then
            get_FreightClassType = FedEx_ShipService.FreightClassType.CLASS_065
        ElseIf _Controls.Contains(obj.LTL_Freight_Class, "070") Then
            get_FreightClassType = FedEx_ShipService.FreightClassType.CLASS_070
        ElseIf _Controls.Contains(obj.LTL_Freight_Class, "077") Then
            get_FreightClassType = FedEx_ShipService.FreightClassType.CLASS_077_5
        ElseIf _Controls.Contains(obj.LTL_Freight_Class, "085") Then
            get_FreightClassType = FedEx_ShipService.FreightClassType.CLASS_085
        ElseIf _Controls.Contains(obj.LTL_Freight_Class, "092") Then
            get_FreightClassType = FedEx_ShipService.FreightClassType.CLASS_092_5
        ElseIf _Controls.Contains(obj.LTL_Freight_Class, "100") Then
            get_FreightClassType = FedEx_ShipService.FreightClassType.CLASS_100
        ElseIf _Controls.Contains(obj.LTL_Freight_Class, "110") Then
            get_FreightClassType = FedEx_ShipService.FreightClassType.CLASS_110
        ElseIf _Controls.Contains(obj.LTL_Freight_Class, "125") Then
            get_FreightClassType = FedEx_ShipService.FreightClassType.CLASS_125
        ElseIf _Controls.Contains(obj.LTL_Freight_Class, "150") Then
            get_FreightClassType = FedEx_ShipService.FreightClassType.CLASS_150
        ElseIf _Controls.Contains(obj.LTL_Freight_Class, "175") Then
            get_FreightClassType = FedEx_ShipService.FreightClassType.CLASS_175
        ElseIf _Controls.Contains(obj.LTL_Freight_Class, "200") Then
            get_FreightClassType = FedEx_ShipService.FreightClassType.CLASS_200
        ElseIf _Controls.Contains(obj.LTL_Freight_Class, "250") Then
            get_FreightClassType = FedEx_ShipService.FreightClassType.CLASS_250
        ElseIf _Controls.Contains(obj.LTL_Freight_Class, "300") Then
            get_FreightClassType = FedEx_ShipService.FreightClassType.CLASS_300
        ElseIf _Controls.Contains(obj.LTL_Freight_Class, "400") Then
            get_FreightClassType = FedEx_ShipService.FreightClassType.CLASS_400
        ElseIf _Controls.Contains(obj.LTL_Freight_Class, "500") Then
            get_FreightClassType = FedEx_ShipService.FreightClassType.CLASS_500
        End If
    End Function
    Private Function create_RequestedShippingDocumentType(ByVal shipment As _baseShipment, ByVal siquenceno As Integer) As FedEx_ShipService.RequestedShippingDocumentType
        ''ol#1.2.69(5/8)... FedEx Freight Box services were added.
        create_RequestedShippingDocumentType = New FedEx_ShipService.RequestedShippingDocumentType
        create_RequestedShippingDocumentType = FedEx_ShipService.RequestedShippingDocumentType.FREIGHT_ADDRESS_LABEL
    End Function

#Region "International"
    Private Function create_CustomsClearanceDetail(ByVal obj As _baseShipment) As FedEx_ShipService.CustomsClearanceDetail
        create_CustomsClearanceDetail = New FedEx_ShipService.CustomsClearanceDetail
        If obj.CommInvoice IsNot Nothing Then
            Dim comminv As _baseCommInvoice = obj.CommInvoice
            With create_CustomsClearanceDetail
                .DutiesPayment = create_Payment(comminv.DutiesPaymentType, obj)
                If _Controls.Contains(comminv.TypeOfContents, "documents") Then
                    .DocumentContent = FedEx_ShipService.InternationalDocumentContentType.DOCUMENTS_ONLY
                Else
                    .DocumentContent = FedEx_ShipService.InternationalDocumentContentType.NON_DOCUMENTS
                End If
                .DocumentContentSpecified = True
                .CustomsValue = create_Money(comminv.CustomsValue, comminv.CurrencyType)
                .InsuranceCharges = create_Money(comminv.InsuranceCharge, comminv.CurrencyType)
                .CommercialInvoice = create_CommercialInvoice(obj.CommInvoice)
                .ExportDetail = create_ExportDetail(comminv)
                '.ImporterOfRecord.
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
            If FedEx_Data2XML.GetB13AFilingOptionType(obj.B13AFilingOption, typeB13A) Then
                .B13AFilingOptionSpecified = True
                .B13AFilingOption = typeB13A
                If .B13AFilingOption = FedEx_ShipService.B13AFilingOptionType.FILED_ELECTRONICALLY Then
                    .ExportComplianceStatement = "V121245451XCVXCBNBV1253" ' test only
                ElseIf .B13AFilingOption = FedEx_ShipService.B13AFilingOptionType.SUMMARY_REPORTING Then
                    .ExportComplianceStatement = "DSGFH12" ' test only
                ElseIf .B13AFilingOption = FedEx_ShipService.B13AFilingOptionType.NOT_REQUIRED Then
                    .ExportComplianceStatement = "NO EEI 30.37(f)"
                End If
            Else
                Return Nothing
            End If
        End With
    End Function
    Private Function create_CommercialInvoice(ByVal obj As _baseCommInvoice) As FedEx_ShipService.CommercialInvoice
        create_CommercialInvoice = New FedEx_ShipService.CommercialInvoice
        With create_CommercialInvoice
            .Comments = {obj.Comments}
            .FreightCharge = create_Money(obj.FreightCharge, obj.CurrencyType)
            .TaxesOrMiscellaneousCharge = create_Money(obj.TaxesOrMiscCharge, obj.CurrencyType)
            If Not FedEx_Data2XML.GetPurposeOfShipmentType(obj.TypeOfContents) = FedEx_ShipService.PurposeOfShipmentType.NOT_SOLD Then
                .Purpose = FedEx_Data2XML.GetPurposeOfShipmentType(obj.TypeOfContents)
                .PurposeSpecified = True
            End If
            If Not String.Empty = obj.TermsOfSale Then
                .TermsOfSale = obj.TermsOfSale
            End If
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
            .QuantitySpecified = True ''ol#1.1.77(10/13)... 'Insufficient information for commodity 1 to complete shipment' error fix.
            .QuantityUnits = obj.Item_UnitsOfMeasure
            .UnitPrice = create_Money(obj.Item_UnitPrice, currencytype)
            .CustomsValue = create_Money(obj.Item_CustomsValue, currencytype)
            .HarmonizedCode = obj.Item_Code
        End With
    End Function
#End Region

#End Region
#Region "Create Rate & TinT Objects"
    Private Function create_RateRequestObject(ByVal obj As _baseShipment, ByRef shipRequest As FedEx_RateService.RequestedShipment) As Boolean
        create_RateRequestObject = False ' assume.
        With shipRequest
            .ShipTimestamp = obj.CarrierService.ShipDate
            .ShipTimestampSpecified = True
            .DropoffType = FedEx_RateService.DropoffType.REGULAR_PICKUP
            .DropoffTypeSpecified = True
            ' need all of them: ServiceType = FedEx_Data2XML.GetServiceType(obj.CarrierService.ServiceABBR)
            Dim package As _baseShipmentPackage = obj.Packages(0)
            .PackagingType = getPackagingType_RateService(package.PackagingType)
            .Shipper = create_ContactParty_RateService(obj.ShipFromContact, _FedExWeb.objFedEx_Setup.Csp_AccountNumber)
            .Recipient = create_ContactParty_RateService(obj.ShipToContact, String.Empty)
            '.ShippingChargesPayment = create_Payment(_FedExWeb.objFedEx_Setup.PaymentType, obj)
            '.SpecialServicesRequested = create_ShipmentSpecialServices_RateService(obj)
            '.LabelSpecification = create_LabelSpecification_RateService()
            If "PREFERRED" = obj.RateRequestType Then
                .RateRequestTypes = {FedEx_RateService.RateRequestType.PREFERRED}
            Else
                .RateRequestTypes = {FedEx_RateService.RateRequestType.LIST}
            End If
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
            If create_Address(obj, address) Then
                .Address = address
            End If
            If create_Contact(obj, contact) Then
                .Contact = contact
            End If
            '.Shipper.Tins
        End With
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
                                    ''ol#1.2.53(7/6)... FedEx removed EMAIL_NOTIFICATION service in Ship_Service_v21.
                                    ''  .EMailNotificationDetail = create_EMailNotificationDetail(obj)
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
    Private Function create_LabelSpecification_RateService() As FedEx_RateService.LabelSpecification
        create_LabelSpecification_RateService = New FedEx_RateService.LabelSpecification
        With create_LabelSpecification()
            .LabelFormatType = FedEx_RateService.LabelFormatType.COMMON2D 'FedEx_Data2XML.LabelFormatType
            .ImageTypeSpecified = True
            If _Controls.Contains(_FedExWeb.objFedEx_Setup.LabelImageType, "Thermal") Then
                .ImageType = FedEx_RateService.ShippingDocumentImageType.EPL2
                .LabelStockType = FedEx_RateService.LabelStockType.STOCK_4X6
                .LabelStockTypeSpecified = True
                .LabelPrintingOrientation = FedEx_RateService.LabelPrintingOrientationType.BOTTOM_EDGE_OF_TEXT_FIRST
                .LabelPrintingOrientationSpecified = True
            Else
                .ImageType = FedEx_RateService.ShippingDocumentImageType.PDF
            End If
            '.CustomerSpecifiedDetail = create_CustomerSpecifiedLabelDetail()
        End With
    End Function
    Private Function create_Payment_RateService(ByVal type As String, ByVal obj As _baseShipment) As FedEx_RateService.Payment
        create_Payment_RateService = New FedEx_RateService.Payment
        With create_Payment_RateService
            Dim contact As New _baseContact
            .PaymentType = CType(FedEx_Data2XML.GetPaymentType(type, obj, contact), FedEx_RateService.PaymentType)
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
            .AmountSpecified = True
            .Currency = currencytype
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
            .UnitsSpecified = True
            .Value = weight
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

    Private Function create_RequestedPackageLineItem_RateService(ByVal shipment As _baseShipment, ByVal siquenceno As Integer) As FedEx_RateService.RequestedPackageLineItem
        create_RequestedPackageLineItem_RateService = New FedEx_RateService.RequestedPackageLineItem
        Dim package As _baseShipmentPackage = shipment.Packages(siquenceno)
        With create_RequestedPackageLineItem_RateService
            .InsuredValue = create_Money_RateService(package.DeclaredValue, package.Currency_Type)
            .SequenceNumber = (siquenceno + 1).ToString
            .GroupPackageCount = (siquenceno + 1).ToString
            .Weight = create_Weight_RateService(package.Weight_LBs, package.Weight_Units)
            .Dimensions = create_Dimensions_RateService(package)
            .CustomerReferences = {create_CustomerReference_RateService(package)}
            .SpecialServicesRequested = create_PackageSpecialServices_RateService(package, shipment.ShipperContact)
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
            '.SignatureReleaseNumber
            .OptionTypeSpecified = True
        End With
    End Function

    Private Function getShipmentSpecialServiceType_RateService(ByVal optionType As String, ByRef type As String) As Boolean
        ''
        getShipmentSpecialServiceType_RateService = True ' assume required
        If _Controls.Contains(optionType, "COD", True) Then
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
        ElseIf _Controls.Contains(optionType, "EXTREME") AndAlso _Controls.Contains(optionType, "LENGTH") Then
            ''ol#1.2.69(5/8)... FedEx Freight Box services were added.
            type = "EXTREME_LENGTH"
        Else
            getShipmentSpecialServiceType_RateService = False ' optional
        End If
        'Case "x" : tmp = "THIRD_PARTY_CONSIGNEE"
        'Case "x" : tmp = "RETURN_SHIPMENT"
        'Case "x" : tmp = "HOLD_SATURDAY"
        'Case "x" : tmp = "BROKER_SELECT_OPTION"
    End Function
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
        ElseIf _Controls.Contains(srPackagingType, "Box") Then
            getPackagingType_RateService = "FEDEX_BOX"
        ElseIf _Controls.Contains(srPackagingType, "Pak") Then
            getPackagingType_RateService = "FEDEX_PAK"
        ElseIf _Controls.Contains(srPackagingType, "Tube") Then
            getPackagingType_RateService = "FEDEX_TUBE"
        End If
        ''
    End Function
    Private Function getServiceTypeCode_RateService(ByVal serviceABBR As String) As String
        Dim typeCode As String = "GROUND_HOME_DELIVERY" ' assume.
        Select Case serviceABBR
            Case FedEx.Ground, FedEx.CanadaGround : typeCode = "FEDEX_GROUND"
            Case FedEx.FirstOvernight : typeCode = "FIRST_OVERNIGHT"
            Case FedEx.SecondDay : typeCode = "FEDEX_2_DAY"
            Case FedEx.SecondDayAM : typeCode = "FEDEX_2_DAY_AM"
            Case FedEx.Priority : typeCode = "PRIORITY_OVERNIGHT"
            Case FedEx.Standard : typeCode = "STANDARD_OVERNIGHT"
            Case FedEx.Saver : typeCode = "FEDEX_EXPRESS_SAVER"
            Case FedEx.Intl_First : typeCode = "INTERNATIONAL_FIRST" ''EUROPE_FIRST_INTERNATIONAL_PRIORITY
            Case FedEx.Intl_Priority : typeCode = "INTERNATIONAL_PRIORITY" ''INTERNATIONAL_PRIORITY_FREIGHT
            Case FedEx.Intl_Economy : typeCode = "INTERNATIONAL_ECONOMY" ''INTERNATIONAL_ECONOMY_FREIGHT
            Case FedEx.Freight_1Day : typeCode = "FEDEX_1_DAY_FREIGHT"
            Case FedEx.Freight_2Day : typeCode = "FEDEX_2_DAY_FREIGHT"
            Case FedEx.Freight_3Day : typeCode = "FEDEX_3_DAY_FREIGHT"
        End Select
        getServiceTypeCode_RateService = typeCode
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
             ''ol#1.2.69(5/8)... FedEx Freight Box services were added.
            Case "FEDEX-FRP" : Return "FEDEX_FREIGHT_PRIORITY"
            Case "FEDEX_FRE" : Return "FEDEX_FREIGHT_ECONOMY"
        End Select
    End Function

    Private Function create_ContactAndAddress_RateService(ByVal obj As _baseContact) As FedEx_RateService.ContactAndAddress
        ''ol#1.2.69(5/8)... FedEx Freight Box services were added.
        Dim address As New FedEx_RateService.Address
        Dim contact As New FedEx_RateService.Contact
        create_ContactAndAddress_RateService = New FedEx_RateService.ContactAndAddress
        With create_ContactAndAddress_RateService
            If create_Contact(obj, contact) Then
                .Contact = contact
            End If
            If create_Address(obj, address) Then
                .Address = address
            End If
        End With
    End Function
    Private Function create_RequestedFreightLineItem_RateService(ByVal shipment As _baseShipment, ByVal siquenceno As Integer) As FedEx_RateService.FreightShipmentLineItem
        ''ol#1.2.69(5/8)... FedEx Freight Box services were added.
        create_RequestedFreightLineItem_RateService = New FedEx_RateService.FreightShipmentLineItem
        Dim package As _baseShipmentPackage = shipment.Packages(siquenceno)
        Dim objFreight As _baseFreight = package.Freight
        With create_RequestedFreightLineItem_RateService
            .FreightClass = get_FreightClassType_RateService(objFreight)
            .FreightClassSpecified = True
            .Packaging = get_Freight_PhysicalPackagingType_RateService(package)
            .PackagingSpecified = True
            .Description = objFreight.LTL_Freight_Description
            .Weight = create_Weight_RateService(package.Weight_LBs, package.Weight_Units)
            .Dimensions = create_Dimensions_RateService(package)
        End With
    End Function

    Private Function get_Freight_PhysicalPackagingType_RateService(ByVal obj As _baseShipmentPackage) As FedEx_RateService.PhysicalPackagingType
        ''ol#1.2.69(5/8)... FedEx Freight Box services were added.
        get_Freight_PhysicalPackagingType_RateService = FedEx_RateService.PhysicalPackagingType.BOX ' assume.
        If _Controls.Contains(obj.PackagingType, "BAG") Then
            get_Freight_PhysicalPackagingType_RateService = FedEx_RateService.PhysicalPackagingType.BAG
        ElseIf _Controls.Contains(obj.PackagingType, "BARREL") Then
            get_Freight_PhysicalPackagingType_RateService = FedEx_RateService.PhysicalPackagingType.BARREL
        ElseIf _Controls.Contains(obj.PackagingType, "BASKET") Then
            get_Freight_PhysicalPackagingType_RateService = FedEx_RateService.PhysicalPackagingType.BASKET
        ElseIf _Controls.Contains(obj.PackagingType, "BOX") Then
            get_Freight_PhysicalPackagingType_RateService = FedEx_RateService.PhysicalPackagingType.BOX
        ElseIf _Controls.Contains(obj.PackagingType, "BUCKET") Then
            get_Freight_PhysicalPackagingType_RateService = FedEx_RateService.PhysicalPackagingType.BUCKET
        ElseIf _Controls.Contains(obj.PackagingType, "BUNDLE") Then
            get_Freight_PhysicalPackagingType_RateService = FedEx_RateService.PhysicalPackagingType.BUNDLE
        ElseIf _Controls.Contains(obj.PackagingType, "CARTON") Then
            get_Freight_PhysicalPackagingType_RateService = FedEx_RateService.PhysicalPackagingType.CARTON
        ElseIf _Controls.Contains(obj.PackagingType, "CASE") Then
            get_Freight_PhysicalPackagingType_RateService = FedEx_RateService.PhysicalPackagingType.CASE
        ElseIf _Controls.Contains(obj.PackagingType, "CONTAINER") Then
            get_Freight_PhysicalPackagingType_RateService = FedEx_RateService.PhysicalPackagingType.CONTAINER
        ElseIf _Controls.Contains(obj.PackagingType, "CRATE") Then
            get_Freight_PhysicalPackagingType_RateService = FedEx_RateService.PhysicalPackagingType.CRATE
        ElseIf _Controls.Contains(obj.PackagingType, "CYLINDER") Then
            get_Freight_PhysicalPackagingType_RateService = FedEx_RateService.PhysicalPackagingType.CYLINDER
        ElseIf _Controls.Contains(obj.PackagingType, "DRUM") Then
            get_Freight_PhysicalPackagingType_RateService = FedEx_RateService.PhysicalPackagingType.DRUM
        ElseIf _Controls.Contains(obj.PackagingType, "HAMPER") Then
            get_Freight_PhysicalPackagingType_RateService = FedEx_RateService.PhysicalPackagingType.HAMPER
        ElseIf _Controls.Contains(obj.PackagingType, "PAIL") Then
            get_Freight_PhysicalPackagingType_RateService = FedEx_RateService.PhysicalPackagingType.PAIL
        ElseIf _Controls.Contains(obj.PackagingType, "PALLET") Then
            get_Freight_PhysicalPackagingType_RateService = FedEx_RateService.PhysicalPackagingType.PALLET
        ElseIf _Controls.Contains(obj.PackagingType, "PIECE") Then
            get_Freight_PhysicalPackagingType_RateService = FedEx_RateService.PhysicalPackagingType.PIECE
        ElseIf _Controls.Contains(obj.PackagingType, "REEL") Then
            get_Freight_PhysicalPackagingType_RateService = FedEx_RateService.PhysicalPackagingType.REEL
        ElseIf _Controls.Contains(obj.PackagingType, "ROLL") Then
            get_Freight_PhysicalPackagingType_RateService = FedEx_RateService.PhysicalPackagingType.ROLL
        ElseIf _Controls.Contains(obj.PackagingType, "SKID") Then
            get_Freight_PhysicalPackagingType_RateService = FedEx_RateService.PhysicalPackagingType.SKID
        ElseIf _Controls.Contains(obj.PackagingType, "TANK") Then
            get_Freight_PhysicalPackagingType_RateService = FedEx_RateService.PhysicalPackagingType.TANK
        ElseIf _Controls.Contains(obj.PackagingType, "TUBE") Then
            get_Freight_PhysicalPackagingType_RateService = FedEx_RateService.PhysicalPackagingType.TUBE
        End If
    End Function
    Private Function get_FreightClassType_RateService(ByVal obj As _baseFreight) As FedEx_RateService.FreightClassType
        ''ol#1.2.69(5/8)... FedEx Freight Box services were added.
        get_FreightClassType_RateService = FedEx_RateService.FreightClassType.CLASS_100 ' assume
        If _Controls.Contains(obj.LTL_Freight_Class, "050") Then
            get_FreightClassType_RateService = FedEx_RateService.FreightClassType.CLASS_050
        ElseIf _Controls.Contains(obj.LTL_Freight_Class, "055") Then
            get_FreightClassType_RateService = FedEx_RateService.FreightClassType.CLASS_055
        ElseIf _Controls.Contains(obj.LTL_Freight_Class, "060") Then
            get_FreightClassType_RateService = FedEx_RateService.FreightClassType.CLASS_060
        ElseIf _Controls.Contains(obj.LTL_Freight_Class, "065") Then
            get_FreightClassType_RateService = FedEx_RateService.FreightClassType.CLASS_065
        ElseIf _Controls.Contains(obj.LTL_Freight_Class, "070") Then
            get_FreightClassType_RateService = FedEx_RateService.FreightClassType.CLASS_070
        ElseIf _Controls.Contains(obj.LTL_Freight_Class, "077") Then
            get_FreightClassType_RateService = FedEx_RateService.FreightClassType.CLASS_077_5
        ElseIf _Controls.Contains(obj.LTL_Freight_Class, "085") Then
            get_FreightClassType_RateService = FedEx_RateService.FreightClassType.CLASS_085
        ElseIf _Controls.Contains(obj.LTL_Freight_Class, "092") Then
            get_FreightClassType_RateService = FedEx_RateService.FreightClassType.CLASS_092_5
        ElseIf _Controls.Contains(obj.LTL_Freight_Class, "100") Then
            get_FreightClassType_RateService = FedEx_RateService.FreightClassType.CLASS_100
        ElseIf _Controls.Contains(obj.LTL_Freight_Class, "110") Then
            get_FreightClassType_RateService = FedEx_RateService.FreightClassType.CLASS_110
        ElseIf _Controls.Contains(obj.LTL_Freight_Class, "125") Then
            get_FreightClassType_RateService = FedEx_RateService.FreightClassType.CLASS_125
        ElseIf _Controls.Contains(obj.LTL_Freight_Class, "150") Then
            get_FreightClassType_RateService = FedEx_RateService.FreightClassType.CLASS_150
        ElseIf _Controls.Contains(obj.LTL_Freight_Class, "175") Then
            get_FreightClassType_RateService = FedEx_RateService.FreightClassType.CLASS_175
        ElseIf _Controls.Contains(obj.LTL_Freight_Class, "200") Then
            get_FreightClassType_RateService = FedEx_RateService.FreightClassType.CLASS_200
        ElseIf _Controls.Contains(obj.LTL_Freight_Class, "250") Then
            get_FreightClassType_RateService = FedEx_RateService.FreightClassType.CLASS_250
        ElseIf _Controls.Contains(obj.LTL_Freight_Class, "300") Then
            get_FreightClassType_RateService = FedEx_RateService.FreightClassType.CLASS_300
        ElseIf _Controls.Contains(obj.LTL_Freight_Class, "400") Then
            get_FreightClassType_RateService = FedEx_RateService.FreightClassType.CLASS_400
        ElseIf _Controls.Contains(obj.LTL_Freight_Class, "500") Then
            get_FreightClassType_RateService = FedEx_RateService.FreightClassType.CLASS_500
        End If
    End Function

#Region "International"
    Private Function create_CustomsClearanceDetail_RateService(ByVal obj As _baseShipment) As FedEx_RateService.CustomsClearanceDetail
        create_CustomsClearanceDetail_RateService = New FedEx_RateService.CustomsClearanceDetail
        If obj.CommInvoice IsNot Nothing Then
            Dim comminv As _baseCommInvoice = obj.CommInvoice
            With create_CustomsClearanceDetail_RateService
                .DutiesPayment = create_Payment_RateService(comminv.DutiesPaymentType, obj)
                If _Controls.Contains(comminv.TypeOfContents, "documents") Then
                    .DocumentContent = FedEx_RateService.InternationalDocumentContentType.DOCUMENTS_ONLY
                Else
                    .DocumentContent = FedEx_RateService.InternationalDocumentContentType.NON_DOCUMENTS
                End If
                .DocumentContentSpecified = True
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
            .PurposeSpecified = True
        End With
    End Function
    Private Function create_ExportDetail_RateService(ByVal obj As _baseCommInvoice) As FedEx_RateService.ExportDetail
        create_ExportDetail_RateService = New FedEx_RateService.ExportDetail
        With create_ExportDetail_RateService
            Dim typeB13A As New FedEx_ShipService.B13AFilingOptionType
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


#Region "Registration"
    Public Function Process_RegisterCSPUser_Request(ByVal shipper As _baseContact, ByRef userKey As String, ByRef userPass As String, ByRef response As String) As Boolean
        '
        'Use the Register CSP User Request to obtain a CSP end user specific key and password. These
        'end user credentials will be obtained during integration testing and also when a customer
        'application moves to production. They must be included with all subsequent requests along with
        'the Provider key and password.
        '
        Process_RegisterCSPUser_Request = False ' assume.
        Try
            Dim webService As New FedEx_RegistrationService.RegistrationService
            webService.Url = _FedExWeb.objFedEx_Setup.Connect2ServerURL ' "https://ws.fedex.com:443/web-services"

            Dim webRequest As New FedEx_RegistrationService.RegisterWebUserRequest
            With webRequest
                .WebAuthenticationDetail = New FedEx_RegistrationService.WebAuthenticationDetail
                .WebAuthenticationDetail.ParentCredential = New FedEx_RegistrationService.WebAuthenticationCredential
                If create_WebAuthenticationCredential(_FedExWeb.objFedEx_Setup.Web_CspCredential_Key, _FedExWeb.objFedEx_Setup.Web_CspCredential_Pass, .WebAuthenticationDetail.ParentCredential) Then
                    '
                    .ClientDetail = New FedEx_RegistrationService.ClientDetail

                    'account number should be read from the textbox, not the database.
                    .ClientDetail.AccountNumber = _FedExWeb.objFedEx_Setup.Csp_AccountNumber
                    '.ClientDetail.AccountNumber = shipper.AccountNumber

                    ''ol#1.2.53(7/6)... FedEx removed '<IntegratorId>' tag from all services.
                    '.ClientDetail.IntegratorId = FedEx_Data2XML.Client_ProductId & FedEx_Data2XML.Client_ProductVersion
                    '
                    .TransactionDetail = New FedEx_RegistrationService.TransactionDetail
                    .TransactionDetail.CustomerTransactionId = "Register CSP User Service Request"
                    '
                    .Version = New FedEx_RegistrationService.VersionId
                    If create_Version("fcas", 7, 0, 0, .Version) Then
                        '
                        .ShippingAddress = New FedEx_RegistrationService.Address
                        If create_RegAddress(shipper, .ShippingAddress) Then
                            '
                            .UserContactAndAddress = New FedEx_RegistrationService.ParsedContactAndAddress
                            .UserContactAndAddress.Address = New FedEx_RegistrationService.Address
                            .UserContactAndAddress.Contact = New FedEx_RegistrationService.ParsedContact
                            If create_RegAddress(shipper, .UserContactAndAddress.Address) Then
                                .UserContactAndAddress.Contact.PersonName = New FedEx_RegistrationService.ParsedPersonName
                                .UserContactAndAddress.Contact.PersonName.FirstName = shipper.FName
                                .UserContactAndAddress.Contact.PersonName.LastName = shipper.LName
                                .UserContactAndAddress.Contact.CompanyName = shipper.CompanyName
                                .UserContactAndAddress.Contact.EMailAddress = shipper.Email
                                .UserContactAndAddress.Contact.FaxNumber = shipper.Fax
                                .UserContactAndAddress.Contact.PhoneNumber = shipper.Tel
                            End If
                        End If
                        ' 
                        Dim xdoc As New Xml.XmlDocument
                        xdoc.LoadXml(serializeShipRequest2string(webRequest))
                        xdoc.Save(_FedExWeb.objFedEx_Setup.Path_SaveDocXML & "\RegisterUser_Request.xml")
                        '
                        Process_RegisterCSPUser_Request = process_RegisterCSPUser_Reply(webService, webRequest, userKey, userPass, response)
                        '
                    End If

                End If
                '
            End With

        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to create a 'Register Web CSP User' request...")
        End Try
    End Function
    Private Function process_RegisterCSPUser_Reply(ByVal webService As FedEx_RegistrationService.RegistrationService, ByVal webRequest As FedEx_RegistrationService.RegisterWebUserRequest, ByRef userKey As String, ByRef userPass As String, ByRef response As String) As Boolean
        process_RegisterCSPUser_Reply = False ' assume.
        userKey = String.Empty
        userPass = String.Empty
        Try
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12
            Dim webReply As FedEx_RegistrationService.RegisterWebUserReply = webService.registerWebUser(webRequest)
            '
            Dim xdoc As New Xml.XmlDocument
            xdoc.LoadXml(serializeShipResponse2string(webReply))
            xdoc.Save(_FedExWeb.objFedEx_Setup.Path_SaveDocXML & "\RegisterUser_Reply.xml")
            ' Result
            If webReply.Notifications IsNot Nothing Then
                Dim notify As FedEx_RegistrationService.Notification = webReply.Notifications(0)
                response = notify.Message
            End If
            '
            If webReply.UserCredential IsNot Nothing Then
                If webReply.UserCredential.Key IsNot Nothing AndAlso webReply.UserCredential.Password IsNot Nothing Then
                    userKey = webReply.UserCredential.Key
                    userPass = webReply.UserCredential.Password
                End If
            End If
            '
            process_RegisterCSPUser_Reply = (Not 0 = userKey.Length AndAlso Not 0 = userPass.Length)
            '
        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message) : _Debug.Print_(ex.Detail.LastChild.InnerText)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to get a response for 'Register Web CSP User' request...")
        End Try
    End Function

    Private Function serializeShipRequest2string(obj As FedEx_RegistrationService.RegisterWebUserRequest) As String
        Dim xml_serializer As New XmlSerializer(GetType(FedEx_RegistrationService.RegisterWebUserRequest))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeShipRequest2string = string_writer.ToString()
        string_writer.Close()
    End Function
    Private Function serializeShipResponse2string(obj As FedEx_RegistrationService.RegisterWebUserReply) As String
        Dim xml_serializer As New XmlSerializer(GetType(FedEx_RegistrationService.RegisterWebUserReply))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeShipResponse2string = string_writer.ToString()
        string_writer.Close()
    End Function

    Public Function Process_Subscription_Request(ByVal shipper As _baseContact, ByRef userMeterNumber As String, ByRef response As String) As Boolean
        '
        ' Use the SubscriptionRequest to obtain the unique meter number specific to the FedEx customer’s account number for integration testing 
        ' and when a customer application moves to production. This is a one-time only request to register the customer's FedEx account.
        ' A unique meter number specific to the customer's FedEx account number will be returned to the client. The meter number should be used 
        ' in all subsequent requests sent to FedEx web services.
        '
        Process_Subscription_Request = False ' assume.
        Try
            Dim webService As New FedEx_RegistrationService.RegistrationService
            webService.Url = _FedExWeb.objFedEx_Setup.Connect2ServerURL '"https://ws.fedex.com:443/web-services"

            Dim webRequest As New FedEx_RegistrationService.SubscriptionRequest
            With webRequest
                .WebAuthenticationDetail = New FedEx_RegistrationService.WebAuthenticationDetail
                .WebAuthenticationDetail.ParentCredential = New FedEx_RegistrationService.WebAuthenticationCredential
                .WebAuthenticationDetail.UserCredential = New FedEx_RegistrationService.WebAuthenticationCredential
                If create_WebAuthenticationCredential(_FedExWeb.objFedEx_Setup.Web_CspCredential_Key, _FedExWeb.objFedEx_Setup.Web_CspCredential_Pass, .WebAuthenticationDetail.ParentCredential) Then
                    If create_WebAuthenticationCredential(_FedExWeb.objFedEx_Setup.Web_UserCredential_Key, _FedExWeb.objFedEx_Setup.Web_UserCredential_Pass, .WebAuthenticationDetail.UserCredential) Then
                        '
                        .ClientDetail = New FedEx_RegistrationService.ClientDetail
                        .ClientDetail.AccountNumber = _FedExWeb.objFedEx_Setup.Csp_AccountNumber
                        ''ol#1.2.53(7/6)... FedEx removed '<IntegratorId>' tag from all services.
                        '.ClientDetail.IntegratorId = FedEx_Data2XML.Client_ProductId & FedEx_Data2XML.Client_ProductVersion

                        .TransactionDetail = New FedEx_RegistrationService.TransactionDetail
                        .TransactionDetail.CustomerTransactionId = "Subscription Request"
                        '
                        .Version = New FedEx_RegistrationService.VersionId
                        If create_Version("fcas", 7, 0, 0, .Version) Then
                            '
                            .CspSolutionId = _FedExWeb.objFedEx_Setup.CSP_SolutionId
                            .CspType = FedEx_RegistrationService.CspType.CERTIFIED_SOLUTION_PROVIDER
                            .CspTypeSpecified = True
                            .Subscriber = create_ContactParty_RegistrationService(shipper, _FedExWeb.objFedEx_Setup.Csp_AccountNumber)
                            .AccountShippingAddress = New FedEx_RegistrationService.Address
                            If create_RegAddress(shipper, .AccountShippingAddress) Then
                                '
                            End If
                            '
                            If Not _Files.IsFileExist(String.Format("{0}\no_xml.txt", _FedExWeb.objFedEx_Setup.Path_SaveDocXML), False) Then
                                Dim xdoc As New Xml.XmlDocument
                                xdoc.LoadXml(serializeShipRequest2string(webRequest))
                                xdoc.Save(_FedExWeb.objFedEx_Setup.Path_SaveDocXML & "\Subscription_Request.xml")
                            End If
                            '
                            Process_Subscription_Request = process_Subscription_Reply(webService, webRequest, userMeterNumber, response)
                            '
                        End If
                        '
                    End If
                End If
                '
            End With

        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to create a 'Subscription' request...")
        End Try
    End Function
    Private Function process_Subscription_Reply(ByVal webService As FedEx_RegistrationService.RegistrationService, ByVal webRequest As FedEx_RegistrationService.SubscriptionRequest, ByRef userMeterNumber As String, ByRef response As String) As Boolean
        process_Subscription_Reply = False ' assume.
        userMeterNumber = String.Empty
        Try
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12
            Dim webReply As FedEx_RegistrationService.SubscriptionReply = webService.subscription(webRequest)
            '
            If Not _Files.IsFileExist(String.Format("{0}\no_xml.txt", _FedExWeb.objFedEx_Setup.Path_SaveDocXML), False) Then
                Dim xdoc As New Xml.XmlDocument
                xdoc.LoadXml(serializeShipResponse2string(webReply))
                xdoc.Save(_FedExWeb.objFedEx_Setup.Path_SaveDocXML & "\Subscription_Reply.xml")
            End If
            ' Result
            If webReply.Notifications IsNot Nothing Then
                Dim notify As FedEx_RegistrationService.Notification = webReply.Notifications(0)
                response = notify.Message
            End If
            '
            If webReply.MeterDetail IsNot Nothing Then
                If webReply.MeterDetail.MeterNumber IsNot Nothing Then
                    userMeterNumber = webReply.MeterDetail.MeterNumber
                End If
            End If
            '
            process_Subscription_Reply = (Not 0 = userMeterNumber.Length)
            '
        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message) : _Debug.Print_(ex.Detail.LastChild.InnerText)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to get a response for 'Subscription' request...")
        End Try
    End Function

    Private Function serializeShipRequest2string(obj As FedEx_RegistrationService.SubscriptionRequest) As String
        Dim xml_serializer As New XmlSerializer(GetType(FedEx_RegistrationService.SubscriptionRequest))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeShipRequest2string = string_writer.ToString()
        string_writer.Close()
    End Function
    Private Function serializeShipResponse2string(obj As FedEx_RegistrationService.SubscriptionReply) As String
        Dim xml_serializer As New XmlSerializer(GetType(FedEx_RegistrationService.SubscriptionReply))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeShipResponse2string = string_writer.ToString()
        string_writer.Close()
    End Function

    Public Function Process_VersionCapture_Request(ByVal userPostalCode As String, ByRef response As String) As Boolean
        '
        ' The VersionCaptureRequest is required to be run at least once for every meter number that is created.
        ' This transaction uploads your FedEx Compatible software product and version information to FedEx.
        '
        Process_VersionCapture_Request = False ' assume.
        Try
            Dim webService As New FedEx_RegistrationService.RegistrationService
            webService.Url = _FedExWeb.objFedEx_Setup.Connect2ServerURL '"https://ws.fedex.com:443/web-services"

            Dim webRequest As New FedEx_RegistrationService.VersionCaptureRequest
            With webRequest
                .WebAuthenticationDetail = New FedEx_RegistrationService.WebAuthenticationDetail
                .WebAuthenticationDetail.ParentCredential = New FedEx_RegistrationService.WebAuthenticationCredential
                .WebAuthenticationDetail.UserCredential = New FedEx_RegistrationService.WebAuthenticationCredential
                If create_WebAuthenticationCredential(_FedExWeb.objFedEx_Setup.Web_CspCredential_Key, _FedExWeb.objFedEx_Setup.Web_CspCredential_Pass, .WebAuthenticationDetail.ParentCredential) Then
                    If create_WebAuthenticationCredential(_FedExWeb.objFedEx_Setup.Web_UserCredential_Key, _FedExWeb.objFedEx_Setup.Web_UserCredential_Pass, .WebAuthenticationDetail.UserCredential) Then
                        '
                        .ClientDetail = New FedEx_RegistrationService.ClientDetail
                        .ClientDetail.AccountNumber = _FedExWeb.objFedEx_Setup.Csp_AccountNumber
                        ''ol#1.2.53(7/6)... FedEx removed '<IntegratorId>' tag from all services.
                        '.ClientDetail.IntegratorId = FedEx_Data2XML.Client_ProductId & FedEx_Data2XML.Client_ProductVersion
                        .ClientDetail.MeterNumber = _FedExWeb.objFedEx_Setup.Client_MeterNumber
                        '
                        .TransactionDetail = New FedEx_RegistrationService.TransactionDetail
                        .TransactionDetail.CustomerTransactionId = "Version Capture Postal Code Request"
                        '
                        .Version = New FedEx_RegistrationService.VersionId
                        If create_Version("fcas", 7, 0, 0, .Version) Then
                            '
                            .OriginLocationId = userPostalCode
                            .VendorProductPlatform = "Windows"
                            '
                            If Not _Files.IsFileExist(String.Format("{0}\no_xml.txt", _FedExWeb.objFedEx_Setup.Path_SaveDocXML), False) Then
                                Dim xdoc As New Xml.XmlDocument
                                xdoc.LoadXml(serializeShipRequest2string(webRequest))
                                xdoc.Save(_FedExWeb.objFedEx_Setup.Path_SaveDocXML & "\VersionCapture_Request.xml")
                            End If
                            '
                            Process_VersionCapture_Request = process_VersionCapture_Reply(webService, webRequest, userPostalCode, response)
                            '
                        End If
                        '
                    End If
                End If
                '
            End With

        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to create a 'Version Capture' request...")
        End Try
    End Function
    Private Function process_VersionCapture_Reply(ByVal webService As FedEx_RegistrationService.RegistrationService, ByVal webRequest As FedEx_RegistrationService.VersionCaptureRequest, ByRef userPostalCode As String, ByRef response As String) As Boolean
        process_VersionCapture_Reply = False ' assume.
        Try
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12
            Dim webReply As FedEx_RegistrationService.VersionCaptureReply = webService.versionCapture(webRequest)
            '
            If Not _Files.IsFileExist(String.Format("{0}\no_xml.txt", _FedExWeb.objFedEx_Setup.Path_SaveDocXML), False) Then
                Dim xdoc As New Xml.XmlDocument
                xdoc.LoadXml(serializeShipResponse2string(webReply))
                xdoc.Save(_FedExWeb.objFedEx_Setup.Path_SaveDocXML & "\VersionCapture_Reply.xml")
            End If

            ' Result
            If webReply.Notifications IsNot Nothing Then
                Dim notify As FedEx_RegistrationService.Notification = webReply.Notifications(0)
                process_VersionCapture_Reply = (notify.Severity = FedEx_RegistrationService.NotificationSeverityType.SUCCESS)
                response = notify.Message
                If response Is Nothing Then
                    If notify.Severity.ToString IsNot Nothing Then
                        response = notify.Severity.ToString
                    End If
                End If
            End If
            '
        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message) : _Debug.Print_(ex.Detail.LastChild.InnerText)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to get a response for 'Version Capture' request...")
        End Try
    End Function

    Private Function serializeShipRequest2string(obj As FedEx_RegistrationService.VersionCaptureRequest) As String
        Dim xml_serializer As New XmlSerializer(GetType(FedEx_RegistrationService.VersionCaptureRequest))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeShipRequest2string = string_writer.ToString()
        string_writer.Close()
    End Function
    Private Function serializeShipResponse2string(obj As FedEx_RegistrationService.VersionCaptureReply) As String
        Dim xml_serializer As New XmlSerializer(GetType(FedEx_RegistrationService.VersionCaptureReply))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeShipResponse2string = string_writer.ToString()
        string_writer.Close()
    End Function

#End Region

#Region "Shipping"

    Public Function Process_ShipAPackage(ByVal obj As _baseShipment, ByRef vb_response As Object) As Boolean
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
                        'Dim trans As New FedEx_ShipService.TransactionDetail
                        'Dim srpack As _baseShipmentPackage = obj.Packages(0)
                        'trans.CustomerTransactionId = srpack.PackageID
                        'shipService.TransactionDetail = trans
                        '
                        Dim version As New FedEx_ShipService.VersionId
                        If create_Version("ship", 21, 0, 0, version) Then
                            shipService.Version = version
                            '
                            Dim shipRequest As New FedEx_ShipService.RequestedShipment
                            If create_RequestObject(obj, shipRequest) Then
                                shipService.RequestedShipment = shipRequest
                                With shipService.RequestedShipment
                                    .PackageCount = obj.Packages.Count.ToString
                                    .TotalWeight = create_TotalWeight(obj)
                                    '
                                    For i As Integer = 0 To obj.Packages.Count - 1
                                        If i > 0 Then
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
                                        If FedEx_Freight.IsFreightBoxPackaging(srpack.PackagingType) Then
                                            .DeliveryInstructions = "Freight Box"
                                            .PickupDetail = New FedEx_ShipService.PickupDetail
                                            .PickupDetail.CourierInstructions = "Freight Box"
                                        End If
                                        '
                                        If Not _Files.IsFileExist(String.Format("{0}\no_xml.txt", _FedExWeb.objFedEx_Setup.Path_SaveDocXML), False) Then
                                            Dim xdoc As New Xml.XmlDocument
                                            xdoc.LoadXml(serializeShipRequest2string(shipService))
                                            xdoc.Save(_FedExWeb.objFedEx_Setup.Path_SaveDocXML & "\" & srpack.PackageID & "_RequestShipment.xml") ' shipment ID
                                        End If
                                        '
                                        ''ol#1.1.53(6/19)... FedEx Web should fail back without clearing ShipMaster shipping info if there are network-connection issues.
                                        Process_ShipAPackage = process_ShipAPackage_Response(shipService, webservice, i, vb_response)
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
    Private Function process_ShipAPackage_Response(ByVal shipService As FedEx_ShipService.ProcessShipmentRequest, ByVal webservice As FedEx_ShipService.ShipService, ByVal pack_sequence As Integer, ByRef vb_response As Object) As Boolean
        process_ShipAPackage_Response = False ' assume.
        Try
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12
            Dim shipResponse As FedEx_ShipService.ProcessShipmentReply = webservice.processShipment(shipService)

            If Not _Files.IsFileExist(String.Format("{0}\no_xml.txt", _FedExWeb.objFedEx_Setup.Path_SaveDocXML), False) Then
                Dim xdoc As New Xml.XmlDocument
                xdoc.LoadXml(serializeShipResponse2string(shipResponse))
                xdoc.Save(_FedExWeb.objFedEx_Setup.Path_SaveDocXML & "\" & vb_response.Packages(pack_sequence).PackageID & "_ReplyShipment.xml")
            End If
            '
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
#Region "Shipping Freight"

    Public Function Process_Ship_Freight(ByVal obj As _baseShipment, ByRef vb_response As Object) As Boolean
        Process_Ship_Freight = False ' assume. ''ol#1.2.69(5/8)... FedEx Freight Box services were added.
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
                            create_RequestObject(obj, shipRequest, True)
                            shipService.RequestedShipment = shipRequest
                            With shipService.RequestedShipment
                                .PackageCount = obj.Packages.Count.ToString
                                .TotalWeight = create_TotalWeight(obj)
                                .TotalInsuredValue = create_TotalInsuredValue(obj)
                                '
                                .ServiceType = FedEx_Data2XML.GetServiceType(obj.CarrierService.ServiceABBR)
                                .ShippingChargesPayment = create_Payment_Freight(_FedExWeb.objFedEx_Setup.PaymentType, obj)
                                '
                                .DeliveryInstructions = "Freight Box"
                                .PickupDetail = New FedEx_ShipService.PickupDetail
                                .PickupDetail.CourierInstructions = "Freight Box"
                                '
                                Dim freightDetail As New FedEx_ShipService.FreightShipmentDetail
                                freightDetail.FedExFreightAccountNumber = obj.ShipperContact.AccountNumber
                                Dim contactAndAddress = New FedEx_ShipService.ContactAndAddress
                                freightDetail.FedExFreightBillingContactAndAddress = create_ContactAndAddress(obj.ShipperContact)
                                freightDetail.RoleSpecified = True
                                freightDetail.Role = FedEx_ShipService.FreightShipmentRoleType.SHIPPER
                                freightDetail.CollectTermsType = FedEx_ShipService.FreightCollectTermsType.NON_RECOURSE_SHIPPER_SIGNED
                                freightDetail.CollectTermsTypeSpecified = True
                                freightDetail.TotalHandlingUnits = obj.Packages.Count.ToString
                                '
                                '
                                '.MasterTrackingId = create_MasterTrackingID(vb_response.Packages(1).TrackingNo)
                                If obj.Packages.Count = 1 Then
                                    freightDetail.LineItems = {create_RequestedFreightLineItem(obj, 1)}
                                ElseIf obj.Packages.Count = 2 Then
                                    freightDetail.LineItems = {create_RequestedFreightLineItem(obj, 1), create_RequestedFreightLineItem(obj, 2)}
                                ElseIf obj.Packages.Count = 3 Then
                                    freightDetail.LineItems = {create_RequestedFreightLineItem(obj, 1), create_RequestedFreightLineItem(obj, 2), create_RequestedFreightLineItem(obj, 3)}
                                ElseIf obj.Packages.Count = 4 Then
                                    freightDetail.LineItems = {create_RequestedFreightLineItem(obj, 1), create_RequestedFreightLineItem(obj, 2), create_RequestedFreightLineItem(obj, 3), create_RequestedFreightLineItem(obj, 4)}
                                ElseIf obj.Packages.Count > 4 Then
                                    freightDetail.LineItems = {create_RequestedFreightLineItem(obj, 1), create_RequestedFreightLineItem(obj, 2), create_RequestedFreightLineItem(obj, 3), create_RequestedFreightLineItem(obj, 4), create_RequestedFreightLineItem(obj, 5)}
                                End If
                                .FreightShipmentDetail = freightDetail
                                '
                                Dim shipdocs As New FedEx_ShipService.ShippingDocumentSpecification
                                shipdocs.ShippingDocumentTypes = {FedEx_ShipService.RequestedShippingDocumentType.FREIGHT_ADDRESS_LABEL}
                                Dim feightAddrDetail As New FedEx_ShipService.FreightAddressLabelDetail
                                feightAddrDetail.Format = New FedEx_ShipService.ShippingDocumentFormat
                                With feightAddrDetail.Format
                                    .ImageType = FedEx_ShipService.ShippingDocumentImageType.PNG
                                    .ImageTypeSpecified = True
                                    .StockType = FedEx_ShipService.ShippingDocumentStockType.PAPER_4X6
                                    .StockTypeSpecified = True
                                    .ProvideInstructions = True
                                    .ProvideInstructionsSpecified = True
                                End With
                                shipdocs.FreightAddressLabelDetail = feightAddrDetail
                                .ShippingDocumentSpecification = shipdocs
                                '
                                Dim trans As New FedEx_ShipService.TransactionDetail
                                Dim srpack As _baseShipmentPackage = obj.Packages(1)
                                trans.CustomerTransactionId = srpack.PackageID
                                shipService.TransactionDetail = trans
                                '
                                Dim packageitem As New FedEx_ShipService.RequestedPackageLineItem
                                packageitem.SpecialServicesRequested = New FedEx_ShipService.PackageSpecialServicesRequested
                                packageitem.SpecialServicesRequested = create_PackageSpecialServices(srpack, obj.ShipperContact)
                                .RequestedPackageLineItems = {packageitem}
                                '
                                Dim xdoc As New Xml.XmlDocument
                                xdoc.LoadXml(serializeShipRequest2string(shipService))
                                xdoc.Save(_FedExWeb.objFedEx_Setup.Path_SaveDocXML & "\" & srpack.PackageID & "_RequestFreight.xml") ' shipment ID
                                '
                                ''ol#1.1.53(6/19)... FedEx Web should fail back without clearing ShipMaster shipping info if there are network-connection issues.
                                Process_Ship_Freight = process_Freight_Response(shipService, webservice, 1, vb_response)
                                '
                            End With
                            '
                        End If
                    End If
                End If
            End If
            '
        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to create a 'Ship Freight' request...")
        End Try
    End Function
    Private Function process_Freight_Response(ByVal shipService As FedEx_ShipService.ProcessShipmentRequest, ByVal webservice As FedEx_ShipService.ShipService, ByVal pack_sequence As Integer, ByRef vb_response As Object) As Boolean
        process_Freight_Response = False ' assume. ''ol#1.2.69(5/8)... FedEx Freight Box services were added.
        Try
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12
            Dim shipResponse As FedEx_ShipService.ProcessShipmentReply = webservice.processShipment(shipService)

            Dim xdoc As New Xml.XmlDocument
            xdoc.LoadXml(serializeShipResponse2string(shipResponse))
            xdoc.Save(_FedExWeb.objFedEx_Setup.Path_SaveDocXML & "\" & vb_response.Packages(pack_sequence).PackageID & "_ReplyFreight.xml")
            '
            process_Freight_Response = True ' got the response!

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
                        Dim codFile As String = _FedExWeb.objFedEx_Setup.Path_SaveDocXML & "\" & vb_response.Packages(s + pack_sequence).PackageID & "_FreightlabelCOD." & codFileExt
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
                If shipment.MasterTrackingId IsNot Nothing Then
                    ' Tracking Number:
                    vb_response.Packages(pack_sequence).TrackingNo = shipment.MasterTrackingId.TrackingNumber
                End If
                Dim packages As FedEx_ShipService.ShippingDocument() = shipment.ShipmentDocuments
                If Not IsNothing(packages) Then
                    For p As Integer = 0 To packages.Length - 1
                        Dim package As FedEx_ShipService.ShippingDocument = shipResponse.CompletedShipmentDetail.ShipmentDocuments(p)
                        ' Shipping labels:
                        For i As Integer = 0 To package.Parts.Length - 1
                            Dim labelString As String = String.Empty
                            Dim labelFileExt As String = FedEx_Data2XML.GetLabel_FileExtension(_FedExWeb.objFedEx_Setup.LabelImageType)
                            ''Dim labelFile As String = FedEx_Data2XML.Path_SaveDocXML & "\" & vb_response.Packages(i + pack_sequence).PackageID & "_Freightlabel." & labelFileExt
                            Dim labelFile As String = _FedExWeb.objFedEx_Setup.Path_SaveDocXML & "\" & vb_response.Packages(i + pack_sequence).PackageID & "_" & package.Type.ToString & "." & package.ImageType.ToString
                            '
                            If Not IsNothing(package.Parts) Then
                                If _Files.WriteFile_ToEnd(package.Parts(i).Image, labelFile) Then
                                    If _Files.ReadFile_ToEnd(labelFile, False, labelString) Then
                                        vb_response.Packages(i + pack_sequence).LabelImage = labelString
                                    End If
                                End If
                            End If
                            ''If Not IsNothing(package.CodReturnDetail) Then
                            ''    If Not IsNothing(package.CodReturnDetail.Label) Then
                            ''        Dim cod As ShippingDocument = package.CodReturnDetail.Label
                            ''        Dim codString As String = String.Empty
                            ''        Dim codFileExt As String = FedEx_Data2XML.GetLabel_FileExtension(_FedExWeb.objFedEx_Setup.LabelImageType)
                            ''        Dim codFile As String = _FedExWeb.objFedEx_Setup.Path_SaveDocXML & "\" & vb_response.Packages(i + pack_sequence).PackageID & "_labelCOD." & codFileExt
                            ''        If _Files.WriteFile_ToEnd(cod.Parts(i).Image, codFile) Then
                            ''            If _Files.ReadFile_ToEnd(codFile, False, codString) Then
                            ''                vb_response.Packages(i + pack_sequence).LabelCODImage = codString
                            ''            End If
                            ''        End If
                            ''    End If
                            ''End If
                        Next i
                    Next p
                End If
            End If
        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message) : _Debug.Print_(ex.Detail.LastChild.InnerText)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to get a response for Ship Freight request...")
        End Try
    End Function

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
                            Dim trans As New FedEx_ShipService.TransactionDetail
                            trans.CustomerTransactionId = packageID
                            shipService.TransactionDetail = trans
                            '
                            Dim trackid As New FedEx_ShipService.TrackingId
                            trackid.TrackingIdType = FedEx_ShipService.TrackingIdType.FEDEX
                            trackid.TrackingIdTypeSpecified = True
                            trackid.TrackingNumber = trackingNo
                            shipService.TrackingId = trackid
                            shipService.DeletionControl = FedEx_ShipService.DeletionControlType.DELETE_ONE_PACKAGE
                            '
                            If Not _Files.IsFileExist(String.Format("{0}\no_xml.txt", _FedExWeb.objFedEx_Setup.Path_SaveDocXML), False) Then
                                Dim xdoc As New Xml.XmlDocument
                                xdoc.LoadXml(serializeDeleteRequest2string(shipService))
                                xdoc.Save(_FedExWeb.objFedEx_Setup.Path_SaveDocXML & "\" & packageID & "_RequestDeleteShipment.xml") ' shipment ID
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
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12
            Dim shipResponse As FedEx_ShipService.ShipmentReply = webservice.deleteShipment(shipService)

            If Not _Files.IsFileExist(String.Format("{0}\no_xml.txt", _FedExWeb.objFedEx_Setup.Path_SaveDocXML), False) Then
                Dim xdoc As New Xml.XmlDocument
                xdoc.LoadXml(serializeDeleteResponse2string(shipResponse))
                xdoc.Save(_FedExWeb.objFedEx_Setup.Path_SaveDocXML & "\" & packageID & "_ReplyDeleteShipment.xml")
            End If
            '
            process_DeleteAPackage_Response = True ' got the response!

            ' Result
            If shipResponse.Notifications IsNot Nothing Then
                For n As Integer = 0 To shipResponse.Notifications.Length - 1
                    Dim notify As FedEx_ShipService.Notification = shipResponse.Notifications(n)
                    If Not notify.Severity = FedEx_ShipService.NotificationSeverityType.SUCCESS Then
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

#Region "Rate & TimeInTransit"

    Public Function Process_Rate_Freight(ByVal path2RateDb As String, ByVal obj As _baseShipment, ByRef vb_response As Object) As Boolean
        Process_Rate_Freight = False ' assume.
        ''ol#1.2.69(5/8)... FedEx Freight Box services were added.
        Try
            Dim webservice As New FedEx_RateService.RateService
            webservice.Url = _FedExWeb.objFedEx_Setup.Connect2ServerURL '"https://ws.fedex.com:443/web-services"
            Dim shipService As New FedEx_RateService.RateRequest

            Dim webauth As New FedEx_RateService.WebAuthenticationDetail
            Dim webauth_csp As New FedEx_RateService.WebAuthenticationCredential
            Dim webauth_user As New FedEx_RateService.WebAuthenticationCredential
            If create_WebAuthenticationCredential(_FedExWeb.objFedEx_Setup.Web_CspCredential_Key, _FedExWeb.objFedEx_Setup.Web_CspCredential_Pass, webauth_csp) Then
                If create_WebAuthenticationCredential(_FedExWeb.objFedEx_Setup.Web_UserCredential_Key, _FedExWeb.objFedEx_Setup.Web_UserCredential_Pass, webauth_user) Then
                    webauth.ParentCredential = webauth_csp
                    webauth.UserCredential = webauth_user
                    shipService.WebAuthenticationDetail = webauth
                    '
                    Dim client As New FedEx_RateService.ClientDetail
                    If create_ClientDetail(client) Then
                        shipService.ClientDetail = client
                        '
                        Dim version As New FedEx_RateService.VersionId
                        If create_Version("crs", 26, 0, 0, version) Then
                            shipService.Version = version
                            '
                            ' Time In Transit enabled here:
                            shipService.ReturnTransitAndCommitSpecified = True
                            shipService.ReturnTransitAndCommit = True
                            '
                            Dim shipRequest As New FedEx_RateService.RequestedShipment
                            If create_RateRequestObject(obj, shipRequest) Then
                                shipService.RequestedShipment = shipRequest
                                With shipService.RequestedShipment
                                    .PackageCount = obj.Packages.Count.ToString
                                    .TotalWeight = create_TotalWeight_RateService(obj)
                                    .TotalInsuredValue = create_TotalInsuredValue_Rate(obj)
                                    '
                                    .ServiceType = FedEx_Data2XML.GetServiceType(obj.CarrierService.ServiceABBR)
                                    .ShippingChargesPayment = create_Payment_RateService(_FedExWeb.objFedEx_Setup.PaymentType, obj)
                                    '
                                    .DeliveryInstructions = "Freight Box"
                                    '
                                    Dim srpack As _baseShipmentPackage = obj.Packages(0)
                                    Dim freightDetail As New FedEx_RateService.FreightShipmentDetail
                                    freightDetail.FedExFreightAccountNumber = obj.ShipperContact.AccountNumber
                                    Dim contactAndAddress = New FedEx_RateService.ContactAndAddress
                                    freightDetail.FedExFreightBillingContactAndAddress = create_ContactAndAddress_RateService(obj.ShipperContact)
                                    freightDetail.Role = FedEx_RateService.FreightShipmentRoleType.SHIPPER
                                    freightDetail.RoleSpecified = True
                                    Dim objfreight As _baseFreight = srpack.Freight
                                    freightDetail.TotalHandlingUnits = objfreight.LTL_Freight_TotalHandlingUnits.ToString
                                    '
                                    If obj.Packages.Count = 1 Then
                                        freightDetail.LineItems = {create_RequestedFreightLineItem_RateService(obj, 0)}
                                    ElseIf obj.Packages.Count = 2 Then
                                        freightDetail.LineItems = {create_RequestedFreightLineItem_RateService(obj, 0), create_RequestedFreightLineItem_RateService(obj, 1)}
                                    ElseIf obj.Packages.Count > 2 Then
                                        freightDetail.LineItems = {create_RequestedFreightLineItem_RateService(obj, 0), create_RequestedFreightLineItem_RateService(obj, 1), create_RequestedFreightLineItem_RateService(obj, 2)}
                                    End If
                                    .FreightShipmentDetail = freightDetail
                                    '
                                    Dim trans As New FedEx_RateService.TransactionDetail
                                    trans.CustomerTransactionId = srpack.PackageID
                                    shipService.TransactionDetail = trans
                                    '
                                    Dim packageitem As New FedEx_RateService.RequestedPackageLineItem
                                    packageitem.SpecialServicesRequested = New FedEx_RateService.PackageSpecialServicesRequested
                                    packageitem.SpecialServicesRequested = create_PackageSpecialServices_RateService(srpack, obj.ShipperContact)
                                    .RequestedPackageLineItems = {packageitem}
                                    '
                                    Dim xdoc As New Xml.XmlDocument
                                    xdoc.LoadXml(serializeRateRequest2string(shipService))
                                    xdoc.Save(_FedExWeb.objFedEx_Setup.Path_SaveDocXML & "\RateFreight_Request.xml") ' shipment ID
                                    '
                                    Process_Rate_Freight = process_RateFreight_Response(shipService, webservice, vb_response)
                                    If Process_Rate_Freight Then
                                        ' save TotalCharge and TotalSurcharge in Shiprite_Rates.mdb
                                        If _Files.IsFileExist(path2RateDb, True) Then
                                            If vb_response.AvailableServices(0).TotalBaseCharge > 0 Then
                                                Call save_FreightRates(path2RateDb, obj, vb_response)
                                            End If
                                        End If
                                    End If
                                    '
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
    Private Function process_RateFreight_Response(ByVal shipService As FedEx_RateService.RateRequest, ByVal webservice As FedEx_RateService.RateService, ByRef vb_response As Object) As Boolean
        process_RateFreight_Response = False ' assume.
        ''ol#1.2.69(5/8)... FedEx Freight Box services were added.
        Try
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12
            Dim shipResponse As FedEx_RateService.RateReply = webservice.getRates(shipService)

            Dim xdoc As New Xml.XmlDocument
            xdoc.LoadXml(serializeRateResponse2string(shipResponse))
            xdoc.Save(_FedExWeb.objFedEx_Setup.Path_SaveDocXML & "\RateFreight_Reply.xml")
            '
            process_RateFreight_Response = True ' got the response!

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
                Dim rate As FedEx_RateService.RateReplyDetail = shipResponse.RateReplyDetails(0)
                Dim vb_service As baseWebResponse_TinT_Service = vb_response.AvailableServices(0)
                If rate.CommitDetails IsNot Nothing Then
                    ' Delivery dates:
                    Dim commit As FedEx_RateService.CommitDetail = rate.CommitDetails(0)
                    If getServiceType_RateService(vb_service.ServiceCode, String.Empty, String.Empty) = commit.ServiceType Then
                        _Debug.Print_("Serivce: " & vb_service.ServiceCode)
                        vb_service.IsOnlyArrivalTransitTime = commit.TransitTimeSpecified
                        If commit.TransitTimeSpecified Then
                            If commit.TransitTime = FedEx_RateService.TransitTimeType.ONE_DAY Then
                                vb_service.ArrivalTransitTime = "1 day"
                            Else
                                vb_service.ArrivalTransitTime = String.Format("{0} days", getTimeInTransitTypeNumber_RateService(commit.TransitTime).ToString)
                            End If
                            _Debug.Print_(String.Format("Transit: in {0}", vb_service.ArrivalTransitTime))
                        End If
                        If commit.CommitTimestampSpecified Then
                            vb_service.ArrivalDate = commit.CommitTimestamp
                            _Debug.Print_(String.Format("Arrival: {0}", vb_service.ArrivalDate.ToString))
                        End If
                        If commit.DayOfWeekSpecified Then
                            vb_service.ArrivalDayOfWeek = commit.DayOfWeek.ToString
                        End If
                        vb_service.IsServiceAvailable = True
                    End If
                End If
                '
                If rate.RatedShipmentDetails IsNot Nothing Then
                    Dim rated As FedEx_RateService.RatedShipmentDetail = rate.RatedShipmentDetails(0)
                    If rated.ShipmentRateDetail IsNot Nothing Then
                        Dim ratedetail As FedEx_RateService.ShipmentRateDetail = rated.ShipmentRateDetail
                        If ratedetail.TotalBaseCharge.AmountSpecified Then
                            vb_service.TotalBaseCharge = ratedetail.TotalBaseCharge.Amount
                        End If
                        If ratedetail.TotalSurcharges.AmountSpecified Then
                            vb_service.TotalSurcharges = ratedetail.TotalSurcharges.Amount
                        End If
                    End If
                End If
                '
            End If
            '
        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message) : _Debug.Print_(ex.Detail.LastChild.InnerText)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to get a response for Rate Freight request...")
        End Try
    End Function
    Private Function save_FreightRates(ByVal path2db As String, ByVal obj As _baseShipment, ByVal vb_response As Object) As Boolean
        save_FreightRates = False ''ol#1.2.69(5/8)... FedEx Freight Box services were added.
        '
        Dim sql2exe As String = String.Empty
        Dim errorDesc As String = String.Empty
        Dim sql2cmd As New sqlINSERT
        Dim package As _baseShipmentPackage = obj.Packages(1)
        Dim freight As _baseFreight = package.Freight
        '
        sql2cmd.Qry_INSERT("RateDate", DateTime.Today, sql2cmd.DTE_, True, False, "Freight_Rates")
        sql2cmd.Qry_INSERT("Service", obj.CarrierService.ServiceABBR, sql2cmd.TXT_)
        sql2cmd.Qry_INSERT("Zip", obj.ShipToContact.Zip, sql2cmd.TXT_)
        sql2cmd.Qry_INSERT("Weight", create_TotalWeight_RateService(obj).Value.ToString, sql2cmd.NUM_)
        sql2cmd.Qry_INSERT("DecVal", create_TotalInsuredValue_Rate(obj).Amount.ToString, sql2cmd.NUM_)
        sql2cmd.Qry_INSERT("DimL", package.Dim_Length.ToString, sql2cmd.NUM_)
        sql2cmd.Qry_INSERT("DimH", package.Dim_Height.ToString, sql2cmd.NUM_)
        sql2cmd.Qry_INSERT("DimW", package.Dim_Width.ToString, sql2cmd.NUM_)
        sql2cmd.Qry_INSERT("Packaging", freight.LTL_Freight_Packaging.ToUpper, sql2cmd.TXT_)
        sql2cmd.Qry_INSERT("Class", freight.LTL_Freight_Class.ToUpper, sql2cmd.TXT_)
        sql2cmd.Qry_INSERT("Charge", vb_response.AvailableServices(1).TotalBaseCharge, sql2cmd.NUM_)
        sql2exe = sql2cmd.Qry_INSERT("Surcharge", vb_response.AvailableServices(1).TotalSurcharges, sql2cmd.NUM_, False, True)
        '
        save_FreightRates = Not (-1 = IO_UpdateSQLProcessor(gShipriteDB, sql2exe))
        If Not save_FreightRates Then
            _MsgBox.ErrorMessage(errorDesc, "Failed to save a Freght Rates...")
        End If
    End Function



    Public Function Process_Rate_TimeInTransit(ByVal obj As _baseShipment, ByRef vb_response As Object) As Boolean
        Process_Rate_TimeInTransit = False ' assume.
        Try
            Dim webservice As New FedEx_RateService.RateService
            If _FedExWeb.objFedEx_Setup Is Nothing Then
                objFedEx_Setup = objFedEx_Regular_Setup
            End If
            webservice.Url = _FedExWeb.objFedEx_Setup.Connect2ServerURL '"https://ws.fedex.com:443/web-services"
            'Dim shipService As New FedEx_RateService.ProcessShipmentRequest
            Dim shipService As New FedEx_RateService.RateRequest

            Dim webauth As New FedEx_RateService.WebAuthenticationDetail
            Dim webauth_csp As New FedEx_RateService.WebAuthenticationCredential
            Dim webauth_user As New FedEx_RateService.WebAuthenticationCredential
            If create_WebAuthenticationCredential(_FedExWeb.objFedEx_Setup.Web_CspCredential_Key, _FedExWeb.objFedEx_Setup.Web_CspCredential_Pass, webauth_csp) Then
                If create_WebAuthenticationCredential(_FedExWeb.objFedEx_Setup.Web_UserCredential_Key, _FedExWeb.objFedEx_Setup.Web_UserCredential_Pass, webauth_user) Then
                    webauth.ParentCredential = webauth_csp
                    webauth.UserCredential = webauth_user
                    shipService.WebAuthenticationDetail = webauth
                    '
                    Dim client As New FedEx_RateService.ClientDetail
                    If create_ClientDetail(client) Then
                        shipService.ClientDetail = client
                        '
                        Dim version As New FedEx_RateService.VersionId
                        If create_Version("crs", 26, 0, 0, version) Then
                            shipService.Version = version
                            '
                            ' Time In Transit enabled here:
                            shipService.ReturnTransitAndCommitSpecified = True
                            shipService.ReturnTransitAndCommit = True
                            '
                            Dim shipRequest As New FedEx_RateService.RequestedShipment
                            If create_RateRequestObject(obj, shipRequest) Then
                                shipService.RequestedShipment = shipRequest
                                With shipService.RequestedShipment
                                    .PackageCount = obj.Packages.Count.ToString
                                    .TotalWeight = create_TotalWeight_RateService(obj)
                                    '
                                    For i As Integer = 0 To obj.Packages.Count - 1
                                        'If i > 1 Then
                                        '    ' only 1st package's Tracking# in MPS shipment is the MasterTracking#
                                        '    .MasterTrackingId = create_MasterTrackingID(vb_response.Packages(0).TrackingNo)
                                        'End If
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
                                        Process_Rate_TimeInTransit = process_Rate_Response(shipService, webservice, i, vb_response)
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
    Private Function process_Rate_Response(ByVal shipService As FedEx_RateService.RateRequest, ByVal webservice As FedEx_RateService.RateService, ByVal pack_sequence As Integer, ByRef vb_response As Object) As Boolean
        process_Rate_Response = False ' assume.
        Try
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12
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
                                        vb_service.ArrivalDate = commit.CommitTimestamp
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

#Region "Request: Address Validation"
    Public original As _baseContact
    Public verified As _baseContact
    Public verifiedcodes As List(Of String)
    Public isSaveVerifiedAddress As Boolean
    Public Function Submit_AddressValidation(ByRef contact As Object) As Boolean
        Submit_AddressValidation = False ' assume.
        isSaveVerifiedAddress = False ' assume.
        original = New _baseContact
        If copy_OriginalAddress(original, contact) Then
            If Request_AddressValidation(contact) Then
                'AddressValidation.path2save = _FedExWeb.objFedEx_Setup.Path_SaveDocXML
                'AddressValidation.ShowDialog()
                'AddressValidation.Dispose()
                Submit_AddressValidation = True
            End If
        End If
        'Submit_AddressValidation = isSaveVerifiedAddress
    End Function
    Public Function Request_AddressValidation(ByRef obj As Object) As Boolean
        Request_AddressValidation = False ' assume.
        Try
            Dim webservice As New FedEx_AddressValidationService.AddressValidationService
            webservice.Url = _FedExWeb.objFedEx_Setup.Connect2ServerURL '"https://ws.fedex.com:443/web-services"
            Dim shipService As New FedEx_AddressValidationService.AddressValidationRequest

            Dim webauth As New FedEx_AddressValidationService.WebAuthenticationDetail
            Dim webauth_csp As New FedEx_AddressValidationService.WebAuthenticationCredential
            Dim webauth_user As New FedEx_AddressValidationService.WebAuthenticationCredential
            If create_WebAuthenticationCredential(_FedExWeb.objFedEx_Setup.Web_CspCredential_Key, _FedExWeb.objFedEx_Setup.Web_CspCredential_Pass, webauth_csp) Then
                If create_WebAuthenticationCredential(_FedExWeb.objFedEx_Setup.Web_UserCredential_Key, _FedExWeb.objFedEx_Setup.Web_UserCredential_Pass, webauth_user) Then
                    webauth.ParentCredential = webauth_csp
                    webauth.UserCredential = webauth_user
                    shipService.WebAuthenticationDetail = webauth
                    '
                    Dim client As New FedEx_AddressValidationService.ClientDetail
                    If create_ClientDetail(client) Then
                        shipService.ClientDetail = client
                        '
                        Dim version As New FedEx_AddressValidationService.VersionId
                        If create_Version("aval", 4, 0, 0, version) Then
                            shipService.Version = version
                            '
                            Dim address As New FedEx_AddressValidationService.AddressToValidate
                            If create_AddressToValidateObject(obj, address) Then
                                shipService.AddressesToValidate = {address}
                                '
                                Dim trans As New FedEx_AddressValidationService.TransactionDetail
                                trans.CustomerTransactionId = Date.Now.ToString
                                shipService.TransactionDetail = trans
                                '
                                shipService.InEffectAsOfTimestamp = Date.UtcNow
                                shipService.InEffectAsOfTimestampSpecified = True
                                '
                                If Not _Files.IsFileExist(String.Format("{0}\no_xml.txt", _FedExWeb.objFedEx_Setup.Path_SaveDocXML), False) Then
                                    Dim xdoc As New Xml.XmlDocument
                                    xdoc.LoadXml(serializeRateRequest2string(shipService))
                                    If Not String.Empty = _FedExWeb.objFedEx_Setup.Path_SaveDocXML Then
                                        xdoc.Save(_FedExWeb.objFedEx_Setup.Path_SaveDocXML & "\AddressValidation_Request.xml") ' shipment ID
                                    End If
                                End If
                                '
                                Request_AddressValidation = read_AddressValidation(shipService, webservice)
                                '
                            End If
                        End If
                    End If
                End If
            End If
        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to create a Address Validation request...")
        End Try
    End Function
    Private Function read_AddressValidation(ByVal request As FedEx_AddressValidationService.AddressValidationRequest, ByVal webservice As FedEx_AddressValidationService.AddressValidationService) As Boolean
        read_AddressValidation = False ' assume.
        verified = New _baseContact
        verifiedcodes = New List(Of String)
        '
        System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12
        Dim reply As FedEx_AddressValidationService.AddressValidationReply = webservice.addressValidation(request)
        read_AddressValidation = reply IsNot Nothing
        '
        If read_AddressValidation Then
            '
            Dim xdoc As New Xml.XmlDocument
            xdoc.LoadXml(serializeRateResponse2string(reply))
            If Not String.Empty = _FedExWeb.objFedEx_Setup.Path_SaveDocXML Then
                xdoc.Save(_FedExWeb.objFedEx_Setup.Path_SaveDocXML & "\AddressValidation_Reply.xml") ' shipment ID
            End If
            '
            If reply.Notifications IsNot Nothing Then
                For n As Integer = 0 To reply.Notifications.Length - 1
                    Dim notify As FedEx_AddressValidationService.Notification = reply.Notifications(n)
                    If Not FedEx_AddressValidationService.NotificationSeverityType.SUCCESS = notify.Severity Then
                        Dim severityDesc As String = "NOTE"
                        If notify.Message IsNot Nothing Then
                            ' Error:
                            If FedEx_AddressValidationService.NotificationSeverityType.ERROR = notify.Severity Then
                                severityDesc = "ERROR"
                            ElseIf FedEx_AddressValidationService.NotificationSeverityType.FAILURE = notify.Severity Then
                                severityDesc = "FAILURE"
                            ElseIf FedEx_AddressValidationService.NotificationSeverityType.WARNING = notify.Severity Then
                                severityDesc = "WARNING"
                            End If
                            verifiedcodes.Add(String.Format("{0}: {1}", severityDesc, notify.Message))
                        End If
                    End If
                Next n
            End If
            '
            If reply.AddressResults IsNot Nothing Then
                For i As Integer = 0 To reply.AddressResults.Length - 1
                    '
                    Dim isCountrySupported As Boolean = False ' assume.
                    Dim isAdrrVerified As Boolean = False ' assume.
                    Dim result As FedEx_AddressValidationService.AddressValidationResult = reply.AddressResults(i)
                    ''ol#1.2.29(12/17)... FedEx Address Verify reply has new <Resolved>=True/False tag, which indicates if Address was verified or not.
                    If result.Attributes IsNot Nothing Then
                        For j As Integer = 0 To result.Attributes.Length - 1
                            '
                            Dim attribute As FedEx_AddressValidationService.AddressAttribute = result.Attributes(j)
                            If "COUNTRYSUPPORTED" = attribute.Name.ToUpper Then
                                isCountrySupported = ("TRUE" = attribute.Value.ToUpper)
                                If isCountrySupported Then
                                    verifiedcodes.Add("Country Supported: Yes")
                                    'Exit For
                                Else
                                    verifiedcodes.Add("Country is Not Supported!")
                                    'Exit For
                                End If
                            ElseIf "RESOLVED" = attribute.Name.ToUpper Then
                                isAdrrVerified = ("TRUE" = attribute.Value.ToUpper)
                                If isAdrrVerified Then
                                    verifiedcodes.Add("Address Verified: Yes")
                                Else
                                    verifiedcodes.Add("Address Verified: No")
                                End If
                            End If
                            '
                        Next j
                    End If
                    '
                    If isCountrySupported Then
                        ' collect address info
                        If result.ClassificationSpecified Then
                            If isAdrrVerified Then
                                If result.Classification = FedEx_AddressValidationService.FedExAddressClassificationType.RESIDENTIAL Or result.Classification = FedEx_AddressValidationService.FedExAddressClassificationType.BUSINESS Then
                                    verified.Residential = (result.Classification = FedEx_AddressValidationService.FedExAddressClassificationType.RESIDENTIAL)
                                    If verified.Residential Then
                                        verifiedcodes.Add("Residential Confirmed: Yes")
                                    Else
                                        verifiedcodes.Add("Business Confirmed: Yes")
                                    End If
                                Else
                                    verifiedcodes.Add("Residential or Business type is Unknown")
                                    ''ol#1.1.70(8/21)... If FedEx Verify returns 'Residential or Business type is Unknown' code then Residential defaults to TRUE.
                                    verified.Residential = True ' make a default.
                                End If
                            End If
                            If result.EffectiveAddress IsNot Nothing Then
                                Dim addr As FedEx_AddressValidationService.Address = result.EffectiveAddress
                                If addr.City IsNot Nothing Then
                                    verified.City = addr.City
                                End If
                                If addr.PostalCode IsNot Nothing Then
                                    If isAdrrVerified Then verifiedcodes.Add(String.Format("Postal Code: {0}", addr.PostalCode))
                                End If
                                If addr.CountryCode IsNot Nothing Then
                                    verified.CountryCode = addr.CountryCode
                                    If isAdrrVerified Then verifiedcodes.Add(String.Format("Country Code: {0}", verified.CountryCode))
                                End If
                                If addr.StateOrProvinceCode IsNot Nothing Then
                                    verified.State = addr.StateOrProvinceCode
                                End If
                                If addr.StreetLines IsNot Nothing Then
                                    If 1 = addr.StreetLines.Length Then
                                        verified.Addr1 = addr.StreetLines(0)
                                        verified.Addr2 = original.Addr2
                                    ElseIf 1 < addr.StreetLines.Length Then
                                        verified.Addr1 = addr.StreetLines(0)
                                        verified.Addr2 = addr.StreetLines(1)
                                    End If
                                End If
                                If result.ParsedAddressPartsDetail IsNot Nothing Then
                                    If result.ParsedAddressPartsDetail.ParsedStreetLine IsNot Nothing AndAlso result.ParsedAddressPartsDetail.ParsedStreetLine.Organization IsNot Nothing Then
                                        Dim org As FedEx_AddressValidationService.ParsedStreetLineDetail = result.ParsedAddressPartsDetail.ParsedStreetLine
                                        If org.Organization IsNot Nothing Then
                                            String.Format("Org: {0}", org.Organization)
                                        End If
                                    End If
                                    If result.ParsedAddressPartsDetail.ParsedPostalCode IsNot Nothing Then
                                        Dim zip As FedEx_AddressValidationService.ParsedPostalCodeDetail = result.ParsedAddressPartsDetail.ParsedPostalCode
                                        If zip.Base IsNot Nothing Then
                                            verified.Zip = zip.Base
                                        End If
                                    End If
                                End If
                            End If
                        End If
                        '
                    End If
                    '
                Next i
            End If
            '
        End If
        '
    End Function

    Private Function serializeRateRequest2string(obj As FedEx_AddressValidationService.AddressValidationRequest) As String
        Dim xml_serializer As New XmlSerializer(GetType(FedEx_AddressValidationService.AddressValidationRequest))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeRateRequest2string = string_writer.ToString()
        string_writer.Close()
    End Function
    Private Function serializeRateResponse2string(obj As FedEx_AddressValidationService.AddressValidationReply) As String
        Dim xml_serializer As New XmlSerializer(GetType(FedEx_AddressValidationService.AddressValidationReply))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeRateResponse2string = string_writer.ToString()
        string_writer.Close()
    End Function

    Private Function create_AddressToValidateObject(ByVal obj As _baseContact, ByRef address As FedEx_AddressValidationService.AddressToValidate) As Boolean
        create_AddressToValidateObject = False ' assume.
        With address
            .Address = New FedEx_AddressValidationService.Address
            With .Address
                .City = obj.City
                .CountryCode = obj.CountryCode
                .CountryName = obj.Country
                .PostalCode = obj.Zip
                .Residential = obj.Residential
                .ResidentialSpecified = True
                .StateOrProvinceCode = obj.State
                .StreetLines = {obj.Addr1, obj.Addr2}
            End With
        End With
        create_AddressToValidateObject = True
    End Function
    'Private Function create_AddressValidationOptionsObject(ByRef addrvaidation As FedEx_AddressValidatioinService.AddressValidationOptions) As Boolean
    '    create_AddressValidationOptionsObject = False ' assume.
    '    With addrvaidation
    '        .CheckResidentialStatus = True
    '        .CheckResidentialStatusSpecified = True
    '        .DirectionalAccuracy = FedEx_AddressValidatioinService.AddressValidationAccuracyType.MEDIUM
    '        .DirectionalAccuracySpecified = True
    '        .MaximumNumberOfMatches = "1"
    '        .RecognizeAlternateCityNames = True
    '        .RecognizeAlternateCityNamesSpecified = True
    '        .StreetAccuracy = FedEx_AddressValidatioinService.AddressValidationAccuracyType.MEDIUM
    '        .StreetAccuracySpecified = True
    '        .VerifyAddresses = True
    '        .VerifyAddressesSpecified = True
    '        .ReturnParsedElements = True
    '        .ReturnParsedElementsSpecified = True
    '    End With
    '    Return True
    'End Function

    Public Function copy_OriginalAddress(ByRef obj1 As Object, ByVal obj2 As Object) As Boolean
        obj1.ContactID = obj2.ContactID
        obj1.CompanyName = obj2.CompanyName
        obj1.Addr1 = obj2.Addr1
        obj1.Addr2 = obj2.Addr2
        obj1.City = obj2.City
        obj1.State = obj2.State
        obj1.Zip = obj2.Zip
        obj1.CountryCode = obj2.CountryCode
        obj1.Residential = obj2.Residential
        Return True
    End Function
#End Region

#Region "Close Ground Manifest"

    Public Function Close_Request(ByVal srSetup As Object, ByRef vb_response As Object) As Boolean
        Close_Request = False
        Try
            Dim webservice As New FedEx_CloseService.CloseService
            webservice.Url = _FedExWeb.objFedEx_Setup.Connect2ServerURL '"https://ws.fedex.com:443/web-services"
            Dim webrequest As New FedEx_CloseService.GroundCloseWithDocumentsRequest
            '
            Dim webauth As New FedEx_CloseService.WebAuthenticationDetail
            Dim webauth_csp As New FedEx_CloseService.WebAuthenticationCredential
            Dim webauth_user As New FedEx_CloseService.WebAuthenticationCredential
            If create_WebAuthenticationCredential(_FedExWeb.objFedEx_Setup.Web_CspCredential_Key, _FedExWeb.objFedEx_Setup.Web_CspCredential_Pass, webauth_csp) Then
                If create_WebAuthenticationCredential(_FedExWeb.objFedEx_Setup.Web_UserCredential_Key, _FedExWeb.objFedEx_Setup.Web_UserCredential_Pass, webauth_user) Then
                    webauth.ParentCredential = webauth_csp
                    webauth.UserCredential = webauth_user
                    webrequest.WebAuthenticationDetail = webauth
                    '
                    Dim client As New FedEx_CloseService.ClientDetail
                    If create_ClientDetail(client) Then
                        webrequest.ClientDetail = client
                        '
                        Dim trans As New FedEx_CloseService.TransactionDetail
                        trans.CustomerTransactionId = "Close Shipment (Ground Manifest)"
                        webrequest.TransactionDetail = trans
                        '
                        Dim version As New FedEx_CloseService.VersionId
                        If create_Version("clos", 5, 0, 0, version) Then
                            webrequest.Version = version
                            '
                            webrequest.CloseDate = DateTime.Today
                            webrequest.CloseDateSpecified = True
                            '
                            Dim closedocspec As New FedEx_CloseService.CloseDocumentSpecification
                            Dim manifest As FedEx_CloseService.CloseDocumentType = FedEx_CloseService.CloseDocumentType.MANIFEST
                            Dim op90 As FedEx_CloseService.CloseDocumentType = FedEx_CloseService.CloseDocumentType.OP_950
                            Dim codrep As FedEx_CloseService.CloseDocumentType = FedEx_CloseService.CloseDocumentType.COD_REPORT
                            closedocspec.CloseDocumentTypes = {manifest, op90, codrep}
                            '
                            Dim manifestdetl As New FedEx_CloseService.ManifestDetail
                            Dim closedocformat As New FedEx_CloseService.CloseDocumentFormat
                            closedocformat.ImageType = FedEx_CloseService.ShippingDocumentImageType.TEXT
                            closedocformat.ImageTypeSpecified = True
                            manifestdetl.Format = closedocformat
                            closedocspec.ManifestDetail = manifestdetl
                            '
                            Dim op90detail As New FedEx_CloseService.Op950Detail
                            Dim op90detailFormat As New FedEx_CloseService.CloseDocumentFormat
                            op90detailFormat.ImageType = FedEx_CloseService.ShippingDocumentImageType.PDF
                            op90detailFormat.ImageTypeSpecified = True
                            op90detailFormat.StockType = FedEx_CloseService.ShippingDocumentStockType.OP_950
                            op90detailFormat.StockTypeSpecified = True
                            op90detail.Format = op90detailFormat
                            closedocspec.Op950Detail = op90detail
                            '
                            webrequest.CloseDocumentSpecification = closedocspec
                            '
                            If Not _Files.IsFileExist(String.Format("{0}\no_xml.txt", _FedExWeb.objFedEx_Setup.Path_SaveDocXML), False) Then
                                Dim xdoc As New Xml.XmlDocument
                                xdoc.LoadXml(serializeRequest2string(webrequest))
                                xdoc.Save(_FedExWeb.objFedEx_Setup.Path_SaveDocXML & "\GroundManifest_Request.xml") ' shipment ID
                            End If
                            '
                            Close_Request = process_Close_Response(webrequest, webservice, vb_response)
                            '
                        End If
                    End If
                End If
            End If
        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to close 'Ground Manifest' request...")
        End Try
    End Function
    Private Function process_Close_Response(ByVal webrequest As FedEx_CloseService.GroundCloseWithDocumentsRequest, ByVal webservice As FedEx_CloseService.CloseService, ByRef vb_response As baseWebResponse_Shipment) As Boolean
        process_Close_Response = False ' assume.
        Try
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12
            Dim webResponse As FedEx_CloseService.GroundCloseDocumentsReply = webservice.groundCloseWithDocuments(webrequest)

            If Not _Files.IsFileExist(String.Format("{0}\no_xml.txt", _FedExWeb.objFedEx_Setup.Path_SaveDocXML), False) Then
                Dim xdoc As New Xml.XmlDocument
                xdoc.LoadXml(serializeResponse2string(webResponse))
                xdoc.Save(_FedExWeb.objFedEx_Setup.Path_SaveDocXML & "\GroundManifest_Reply.xml")
            End If
            '
            process_Close_Response = True ' got the response!
            '
            ' Result
            If webResponse.Notifications IsNot Nothing Then
                For n As Integer = 0 To webResponse.Notifications.Length - 1
                    Dim notify As FedEx_CloseService.Notification = webResponse.Notifications(n)
                    If Not notify.Severity = FedEx_CloseService.NotificationSeverityType.SUCCESS Then
                        ' Error:
                        vb_response.ShipmentAlerts.Add(notify.Message)
                    End If
                Next n
            End If
            '
            If webResponse.CloseDocuments IsNot Nothing Then
                For d As Integer = 0 To webResponse.CloseDocuments.Length - 1
                    Dim doc As FedEx_CloseService.CloseDocument = webResponse.CloseDocuments(d)
                    If doc.Parts IsNot Nothing Then
                        For p As Integer = 0 To doc.Parts.Length - 1
                            Dim part As FedEx_CloseService.ShippingDocumentPart = doc.Parts(p)
                            If part.Image IsNot Nothing Then
                                If _Files.WriteFile_ToEnd(part.Image, _FedExWeb.objFedEx_Setup.Path_SaveDocXML & "\Ground_Manifest.txt") Then
                                    vb_response.AdditionalInfo = "SUCCESS"
                                End If
                            End If
                        Next p
                    End If
                Next d
            End If
            '
        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message) : _Debug.Print_(ex.Detail.LastChild.InnerText)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to get a response for 'Ground Manifest' request...")
        End Try
    End Function

    Private Function serializeRequest2string(obj As FedEx_CloseService.GroundCloseWithDocumentsRequest) As String
        Dim xml_serializer As New XmlSerializer(GetType(FedEx_CloseService.GroundCloseWithDocumentsRequest))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeRequest2string = string_writer.ToString()
        string_writer.Close()
    End Function
    Private Function serializeResponse2string(obj As FedEx_CloseService.GroundCloseDocumentsReply) As String
        Dim xml_serializer As New XmlSerializer(GetType(FedEx_CloseService.GroundCloseDocumentsReply))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeResponse2string = string_writer.ToString()
        string_writer.Close()
    End Function

#End Region

#Region "Prepare Shipment Info from Manifest and Upload"
    Public Function Prepare_ShipmentFromDb(ByVal dbPackageID As String, ByRef objShipment As _baseShipment) As Boolean
        Prepare_ShipmentFromDb = False ' assume
        ''
        Dim sql2exe As String = String.Empty
        Dim SegmentSet As String = String.Empty
        ''
        Try
            ''
            sql2exe = "Select * From Manifest Where PackageID = '" & dbPackageID & "'"
            SegmentSet = DatabaseFunctions.IO_GetSegmentSet(gShipriteDB, sql2exe)
            _Debug.Print_(SegmentSet)
            If Not String.IsNullOrEmpty(SegmentSet) Then
                '' FedEx Freight Box services were added.
                _Debug.Print_(objShipment.Packages.Count)
                If FedEx_Freight.IsFreightBoxPackaging(objShipment.Packages(0).PackagingType) Then
                    _FedExWeb.objFedEx_Setup = _FedExWeb.objFedEx_Freight_Setup
                Else
                    _FedExWeb.objFedEx_Setup = _FedExWeb.objFedEx_Regular_Setup
                End If
                '
                If _FedExWeb.objFedEx_Setup IsNot Nothing Then

                    If GetPolicyData(gShipriteDB, "FedExREST_Enabled", "False") Then

                    Else
                        If 0 = Len(_FedExWeb.objFedEx_Setup.Web_UserCredential_Key) Or 0 = Len(_FedExWeb.objFedEx_Setup.Web_UserCredential_Pass) Or 0 = Len(_FedExWeb.objFedEx_Setup.Client_MeterNumber) Then
                            Return False
                        End If
                    End If

                    ''
                    '' Double check ShipFrom and ShipTo contacts to avoid shipping from/to previously used contacts.
                    If Not _Contact.ShipFromContact.ContactID = Val(ExtractElementFromSegment("SID", SegmentSet)) Then
                        Call _Contact.Load_ContactFromDb(Val(ExtractElementFromSegment("SID", SegmentSet)), _Contact.ShipFromContact)
                    End If
                    _Debug.Print_(ExtractElementFromSegment("CID", SegmentSet))


                    If IsNothing(_Contact.ShipToContact) OrElse Not _Contact.ShipToContact.ContactID = Val(ExtractElementFromSegment("CID", SegmentSet)) Then
                        Call _Contact.Load_ContactFromDb(Val(ExtractElementFromSegment("CID", SegmentSet)), _Contact.ShipToContact)
                    End If
                    objShipment.ShipperContact = _Contact.ShipperContact
                    objShipment.ShipToContact = _Contact.ShipToContact

                    gShip.Domestic = Not ("I" = ExtractElementFromSegment("InternationalIndicator", SegmentSet))
                    objShipment.CarrierService.IsDomestic = gShip.Domestic

                    ''AP(07/13/2017) - FedEx Intl: Label should show sender as shipper's addr, not store addr.
                    If gShip.Domestic Then
                        objShipment.ShipFromContact = _Contact.ChangeShipFromAs_co_StoreAddress(True)
                    Else
                        objShipment.ShipFromContact = _Contact.ShipFromContact
                    End If

                    gShip.Country = objShipment.ShipToContact.Country
                    'objShipment.ShipToContact.Residential = (0 < Val(ExtractElementFromSegment("chgRES", SegmentSet, "0")))

                    If ExtractElementFromSegment("RES", SegmentSet, "") = "X" Then
                        objShipment.ShipToContact.Residential = True
                    Else
                        objShipment.ShipToContact.Residential = False
                    End If

                    gShip.Residential = objShipment.ShipToContact.Residential
                    ''
                    '' FedEx Ground vs. HomeDelivery (if residential and not more the 70 lbs for actual weight then HomeDelivery)
                    objShipment.CarrierService.ServiceABBR = ExtractElementFromSegment("P1", SegmentSet)
                    If FedEx.IsGroundHomeDelivery(objShipment.CarrierService.ServiceABBR) And objShipment.CarrierService.IsDomestic Then
                        objShipment.CarrierService.ServiceABBR = "FEDEX-GNDHOME" '' home delivery
                    End If
                    ''

                    '' 'EMAIL - Shipping Notifications' option was added to Carrier Setup tab where you can disable/enable email notifications.
                    If Not FedEx.IsEmail_FedEx_ShipNotification Then
                        objShipment.ShipFromContact.Email = ""
                        objShipment.ShipToContact.Email = ""
                    End If

                    '' for hawaii customers the country code has to be 'US'
                    '' If the FedEx shipper country code cannot be found, default it to 'US'.
                    If objShipment.ShipFromContact.CountryCode = "HI" Or objShipment.ShipFromContact.CountryCode = "" Then
                        objShipment.ShipFromContact.CountryCode = "US"
                    End If
                    ''
                    If objShipment.ShipToContact.Tel.Length = 0 Then
                        objShipment.ShipToContact.Tel = InputBox("Recipient is missing a phone number!" & vbCr & vbCr & vbCr & vbCr & vbCr &
                                                               "Please enter the Recipient's phone number here:", WebServTitle)
                        If Not 0 = Len(objShipment.ShipToContact.Tel) Then
                            '' To Do
                            _Contact.Update_PhoneNo(objShipment.ShipToContact.ContactID, objShipment.ShipToContact.Tel)
                        End If
                    End If
                    ''
                    ''
                    objShipment.Comments = ExtractElementFromSegment("Contents", SegmentSet) '"Comments go here"
                    objShipment.RateRequestType = "ACCOUNT"
                    objShipment.CarrierService.CarrierName = "FEDEX"

                    '' FedEx Web Services time-stamp should include the time the package was shipped.
                    Dim PickupDate As Date = _Convert.String2Date(ExtractElementFromSegment("PickupDate", SegmentSet))
                    If PickupDate > Today Then
                        objShipment.CarrierService.ShipDate = PickupDate
                    Else
                        objShipment.CarrierService.ShipDate = Today
                    End If
                    ' To Do: future date
                    objShipment.ShipmentNo = ExtractElementFromSegment("ShipmentID", SegmentSet)
                    ''
                    '' To Do
                    If (objShipment.ShipFromContact.CountryCode = "US" And objShipment.ShipToContact.CountryCode = "PR") Or (objShipment.ShipFromContact.CountryCode = "PR" And objShipment.ShipToContact.CountryCode = "PR") Or (objShipment.ShipFromContact.CountryCode = "PR" And objShipment.ShipToContact.CountryCode = "US") Then
                        '
                        ' Residential Ground shipments to Puerto Rico should be sent by regular FedEx Ground.
                        If _FedExWeb.IsGroundHomeDelivery(objShipment.CarrierService.ServiceABBR) Then
                            objShipment.CarrierService.ServiceABBR = "FEDEX-GND"
                        ElseIf objShipment.CarrierService.ServiceABBR = "FEDEX-PRI" Then
                            objShipment.CarrierService.ServiceABBR = "FEDEX-INTP"
                        ElseIf objShipment.CarrierService.ServiceABBR = "FEDEX-SVR" Then
                            objShipment.CarrierService.ServiceABBR = "FEDEX-INTE"
                        End If
                        '
                    End If
                    '
                    ' Surcharges:
                    '
                    objFedEx_Setup.PaymentType = "SENDER" ' RECIPIENT, COLLECT, THIRD_PARTY 
                    '
                    Call add_ServiceSurcharges(SegmentSet, objShipment)
                    '
                    If Not objShipment.CarrierService.IsDomestic AndAlso Not String.IsNullOrEmpty(ExtractElementFromSegment("CustomsTypeOfContents", SegmentSet)) Then
                        '  
                        Customs.CustomsList.Clear()
                        Customs.Customs_Contents_Type = ExtractElementFromSegment("CustomsTypeOfContents", SegmentSet)
                        _Debug.Print_("Customs Content Type: " & Customs.Customs_Contents_Type)
                        '
                        sql2exe = "Select * From CustomsItems Where PackageID = '" & dbPackageID & "'"
                        SegmentSet = DatabaseFunctions.IO_GetSegmentSet(gShipriteDB, sql2exe)
                        _Debug.Print_(SegmentSet)
                        If Not String.IsNullOrEmpty(SegmentSet) Then
                            '
                            Do Until String.IsNullOrEmpty(SegmentSet)
                                '
                                Dim Segment As String = GetNextSegmentFromSet(SegmentSet)
                                Dim item As New Customs.CustomsItem
                                '
                                item.Qty = Val(ExtractElementFromSegment("Quantity", Segment))
                                item.Description = ExtractElementFromSegment("Description", Segment)
                                item.Weight = Val(ExtractElementFromSegment("Weight", Segment))
                                item.Value = Val(ExtractElementFromSegment("ItemValue", Segment))
                                item.OriginCountry = ExtractElementFromSegment("OriginCountry", Segment)
                                item.HarmonizedCode = ExtractElementFromSegment("HarmonizedCode", Segment)
                                _Debug.Print_(item.Description & " " & item.Value & " " & item.Weight)
                                Customs.CustomsList.Add(item)
                                '
                            Loop
                            '
                        End If
                        '
                        Call _FedExWeb.Prepare_InternationalData(objShipment)
                        If Not objShipment.ShipperContact.CountryCode = objShipment.ShipToContact.CountryCode Then
                            _FedExWeb.objFedEx_Setup.PaymentType = "THIRD_PARTY" ' FedEx International shipments will be billed to a 'Third Party'.
                        End If
                        '
                    End If
                    '
                    ' FedEx Freight Box services were added.
                    If FedEx_Freight.IsFreightLTLService(objShipment.CarrierService.ServiceABBR) Then
                        '
                        If FedEx_Freight.IsFreightBoxPackaging(objShipment.Packages(0).PackagingType) Then
                            '
                            objShipment.Packages(0).PackagingType = "Freight Box"
                            '
                            LTL_Freight = New _baseFreight
                            LTL_Freight.FreightFormPaymentType = "SENDER" 'Prepaid
                            Dim FreightItem As New FreightFormItem
                            FreightItem.HandlingUnits = 1
                            FreightItem.PackagingType = "PALLET"
                            FreightItem.PiecesNo = 1
                            FreightItem.Description = "Freight Box"
                            FreightItem.Weight = gShip.actualWeight
                            FreightItem.InsuredValue = gShip.DecVal
                            FreightItem.PackageClass = "CLASS 100"
                            '
                        End If
                        '
                        _FedExWeb.objFedEx_Setup.PaymentType = LTL_Freight.FreightFormPaymentType
                        Call FedEx_Freight.Create_FreightItemsObject(objShipment)
                        '
                    End If

                    Prepare_ShipmentFromDb = True
                    ''
                End If
            End If
            ''
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to read shipment info from database...")
        End Try

    End Function

    Public Function Prepare_PackageFromDb(ByVal PackageID As String, ByRef objShipment As _baseShipment) As Boolean
        Prepare_PackageFromDb = False 'assume.
        ''
        Dim sql2exe As String = String.Empty
        Dim SegmentSet As String = String.Empty
        ''
        Try
            ''
            sql2exe = "Select * From Manifest Where PackageID = '" & PackageID & "'"
            SegmentSet = DatabaseFunctions.IO_GetSegmentSet(gShipriteDB, sql2exe)
            If Not String.IsNullOrEmpty(SegmentSet) Then
                Dim Pack As New _baseShipmentPackage
                Pack.PackageID = PackageID

                If Not ShipriteStartup.IsOn_gThirdPartyInsurance(objShipment.ShipToContact.Country, objShipment.CarrierService.CarrierName, objShipment.CarrierService.ServiceABBR, Val(gShip.DecVal)) Then
                    ' FedEx Web Services Declared Value for International should Not be $100 if user left it as $0.
                    If objShipment.CarrierService.IsDomestic Then
                        Pack.DeclaredValue = IIf(Val(ExtractElementFromSegment("DECVAL", SegmentSet)) < 100, 100, Val(ExtractElementFromSegment("DECVAL", SegmentSet)))
                    Else
                        Pack.DeclaredValue = Val(ExtractElementFromSegment("DECVAL", SegmentSet))
                    End If
                End If

                If "X" = ExtractElementFromSegment("LTR", SegmentSet) Then
                    Pack.Weight_LBs = Val(ExtractElementFromSegment("BillableWeight", SegmentSet))
                    Pack.IsLetter = True
                Else
                    Pack.Weight_LBs = Val(ExtractElementFromSegment("LBS", SegmentSet))
                    Pack.Dim_Height = Val(ExtractElementFromSegment("Height", SegmentSet))
                    Pack.Dim_Length = Val(ExtractElementFromSegment("LENGTH", SegmentSet))
                    Pack.Dim_Width = Val(ExtractElementFromSegment("Width", SegmentSet))
                    Pack.IsLetter = False
                End If
                gShip.actualWeight = Pack.Weight_LBs ''ol#8.55(7/17)... gShip.ActualWeight must be set to determine if the residential shipment is Home Delivery or Not while reading from database.
                Pack.PackagingType = ExtractElementFromSegment("Packaging", SegmentSet)
                Pack.Currency_Type = _IDs.CurrencyType ''ol#9.151(2/5)... CurrencyType variable was added to manipulate between CAD and USD.
                '
                If "X" = ExtractElementFromSegment("AH", SegmentSet) Then
                    Pack.IsAdditionalHandling = True
                End If
                If 0 < Val(ExtractElementFromSegment("AHPlus", SegmentSet)) Then
                    Pack.IsLargePackage = True
                End If
                '
                Call add_ServiceSurcharges_Package(SegmentSet, objShipment, Pack)
                '
                objShipment.Packages.Add(Pack)
                Prepare_PackageFromDb = True
            End If
            ''
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to read Package data from Manifest database table...")
        End Try
    End Function

    Public Function Prepare_InternationalData(ByRef objShipment As _baseShipment, Optional isFedEx As Boolean = True) As Boolean
        '
        Dim objCommodity As _baseCommodities
        Dim Pack As _baseShipmentPackage = objShipment.Packages(0)
        '
        If 0 < Customs.CustomsList.Count Then
            '
            For Each item As Customs.CustomsItem In Customs.CustomsList
                If 0 <= item.Value Then
                    objCommodity = New _baseCommodities
                    objCommodity.Item_CustomsValue = item.Value
                    objCommodity.Item_Description = item.Description
                    objCommodity.Item_Code = item.HarmonizedCode
                    objCommodity.Item_Weight = item.Weight
                    objCommodity.Item_WeightUnits = Pack.Weight_Units
                    objCommodity.Item_Quantity = item.Qty
                    objCommodity.Item_UnitsOfMeasure = "EA"
                    objCommodity.Item_UnitPrice = item.Value
                    Dim cntry As _CountryDB = Nothing
                    If Shipping.Find_CountryObject_byName(item.OriginCountry, cntry) Then
                        objCommodity.Item_CountryOfOrigin = cntry.CountryCode
                    End If
                    objShipment.CommInvoice.CurrencyType = Pack.Currency_Type
                    objShipment.CommInvoice.CommoditiesList.Add(objCommodity)
                End If
            Next item
            '
            objShipment.CommInvoice.TypeOfContents = Customs.Customs_Contents_Type

            If Not isFedEx Then
                '
                'objShipment.CommInvoice.TypeOfContents = Customs.Customs_Contents_Type
                objShipment.CommInvoice.InvoiceNo = objShipment.Packages(0).PackageID
                '
                '' There are 30 countries that are not required Commercial Invoice to be XML(ed) to FedEx Web Services.
            ElseIf _FedExWeb.IsCommercialInvoice_Allowed(objShipment.ShipToContact.Country) Then
                '
                ' objShipment.CommInvoice.TypeOfContents = Customs.Customs_Contents_Type
                objShipment.CommInvoice.InvoiceNo = objShipment.Packages(0).PackageID
                '
            End If
            '
        End If

        objShipment.CommInvoice.IDTinType = "" 'EIN
        objShipment.CommInvoice.IDTinNo = "" 'Convert.Null2DefaultValue(rs.Fields("TinNumber"))
        objShipment.CommInvoice.DutiesPaymentType = "RECIPIENT" 'Convert.Null2DefaultValue(rs.Fields("DutiesPaymentType"))
        objShipment.CommInvoice.Comments = objShipment.Comments ' Convert.Null2DefaultValue(rs.Fields("CommercialInvoiceComments"))

        ''If FedExCERT.IsFedExTestAccount Then
        ''    objShipment.CommInvoice.FreightCharge = FedExCERT.FreightCharge
        ''    objShipment.CommInvoice.InsuranceCharge = FedExCERT.InsuranceCharge
        ''    objShipment.CommInvoice.TaxesOrMiscCharge = FedExCERT.TaxesOrMiscCharge
        ''Else
        objShipment.CommInvoice.FreightCharge = 0 '100
        objShipment.CommInvoice.InsuranceCharge = 0 '50
        objShipment.CommInvoice.TaxesOrMiscCharge = 0 '25
        ''End If 

        '' 'TermsOfSale' of International shipment was not indicated (if applicable).
        If _IDs.IsIt_CanadaShipper And (Not "CA" = objShipment.ShipToContact.CountryCode) Then
            objShipment.CommInvoice.TermsOfSale = "FOB"
        Else
            objShipment.CommInvoice.TermsOfSale = String.Empty
        End If
        '
        objShipment.CommInvoice.CustomsValue = objShipment.CommInvoice.CommoditiesTotalValue
        objShipment.CommInvoice.CurrencyType = Pack.Currency_Type
        '
        '' B13AFilingOption cannot be empty for Canada Origin to USA.
        If (_IDs.IsIt_USAShipper Or _IDs.IsIt_PuertoRicoShipper Or _IDs.IsIt_VirginIslandShipper) _
        And (objShipment.ShipToContact.CountryCode = "CN" Or objShipment.ShipToContact.CountryCode = "HK" _
            Or objShipment.ShipToContact.CountryCode = "RU" Or objShipment.ShipToContact.CountryCode = "VE") Then
            objShipment.CommInvoice.B13AFilingOption = "NOT_REQUIRED"
            ''ol#16.05(3/4)... B13AFilingOption cannot be empty for Canada Origin to USA.
        ElseIf _IDs.IsIt_USAShipper And "CA" = objShipment.ShipToContact.CountryCode Then
            objShipment.CommInvoice.B13AFilingOption = "NOT_REQUIRED"
        ElseIf _IDs.IsIt_CanadaShipper And (Not "CA" = objShipment.ShipToContact.CountryCode) Then
            objShipment.CommInvoice.B13AFilingOption = "NOT_REQUIRED"  ' FILED_ELECTRONICALLY, NOT_REQUIRED, SUMMARY_REPORTING, MANUALLY_ATTACHED
        Else
            objShipment.CommInvoice.B13AFilingOption = ""
        End If
        '
        ''If FedExCERT.IsFedExTestAccount And Not 0 = Len(FedExCERT.B13AFilingOption) Then
        ''    objShipment.CommInvoice.B13AFilingOption = FedExCERT.B13AFilingOption
        ''End If
        '
        Return True
    End Function
    Public Function IsCommercialInvoice_Allowed(ByVal CountryName As String) As Boolean
        IsCommercialInvoice_Allowed = False ' assume.
        '
        '' FedEx\NoCommInvoiceNeeded.txt country list was created to add quickly countries for which FedEx doesn't need Commercial Invoice.
        '
        If _Files.IsFileExist(_FedExWeb.NoCommInvoiceNeeded_FilePath, False) Then
            '
            IsCommercialInvoice_Allowed = True
            '
            Dim readLn As String = String.Empty
            If _Files.ReadFile_ToEnd(_FedExWeb.NoCommInvoiceNeeded_FilePath, False, readLn) Then
                '
                If _Controls.Contains(readLn, CountryName) Then
                    Return False '' found!
                End If
                '
            Else '' the old way without the file.
                Select Case CountryName
                    Case "Afghanistan", "Algeria", "Aruba", "Bahamas", "Bermuda", "Bonaire"
                    Case "Bosnia & Herzegovina", "Bosnia", "Herzegovina"
                    Case "Bulgaria", "China", "Curacao", "Grenada", "Guadeloupe", "Hungary", "Iceland", "Israel"
                    Case "Kuwait", "Libya", "Nepal"
                    Case "New Caledonia", "Caledonia"
                    Case "Norway", "Peru", "Poland", "Portugal", "Reunion", "Romania"
                    Case "St. Lucia", "St Lucia"
                    Case "Republic of South Africa", "South Africa Republic", "South Africa"
                    Case "Spain", "Swaziland"
                    Case "Tumon-Leste", "Tumon Leste", "Tumon", "Leste"
                    Case Else : Return True
                End Select
            End If
            '
        End If
    End Function
    Public ReadOnly Property NoCommInvoiceNeeded_FilePath() As String
        Get
            Return String.Format("{0}\FedEx\NoCommInvoiceNeeded.txt", gDBpath)
        End Get
    End Property
    Public Function IsGroundHomeDelivery(ByVal serviceAbbr As String) As Boolean
        IsGroundHomeDelivery = False ' assume.
        If "FEDEX-GND" = serviceAbbr Then
            Return gShip.Residential And (Not gShip.actualWeight > 70)
        End If
    End Function
    Public Function IsGroundService(ByVal serviceAbbr As String) As Boolean
        Return ("FEDEX-CAN" = serviceAbbr) Or ("FEDEX-GND" = serviceAbbr)
    End Function
    Private Function add_ServiceSurcharges(ByVal SegmentSet As String, ByRef objShipment As _baseShipment) As Boolean
        add_ServiceSurcharges = False ' assume.
        '
        '
        Dim objCOD As New _baseServiceSurchargeCOD
        objCOD.Amount = Val(ExtractElementFromSegment("CODAMT", SegmentSet))
        If objCOD.Amount > 0 And Not _FedExWeb.IsGroundService(objShipment.CarrierService.ServiceABBR) Then
            '
            _Debug.Stop_("COD Shipment Level = " & objCOD.Amount)
            objShipment.CarrierService.ServiceSurcharges.Add(New _baseServiceSurcharge(0, "COD", "COD", True))
            '
            With objCOD
                If _IDs.IsIt_CanadaShipper And objShipment.CarrierService.IsDomestic Then
                    .CurrencyType = "CAD"
                Else
                    .CurrencyType = "USD"
                End If
                .ChargeType = "" 'COD Recipient AccountNumber
                If CBool(ExtractElementFromSegment("CashiersCheck", SegmentSet, "False")) Then
                    .PaymentType = "0" '' GUARANTEED_FUNDS
                Else
                    ' COD with "certified check or money order only" unchecked should have 'ANY' payment type submitted to FedEx Web Server.
                    ' .PaymentType = "1" ' CASH
                    .PaymentType = "2" '' ANY 
                End If
                ' FedEx COD amount should stay as entered by the user since all the calculations FedEx Web Services will do automatically.
                .Amount = Val(ExtractElementFromSegment("CODAMTwSHIP", SegmentSet))
            End With
            '
            objShipment.CarrierService.ServiceSurchargeCOD = objCOD
        End If
        '
        Dim holdID As String = ExtractElementFromSegment("ABHoldAtAirport", SegmentSet, "")
        If Not String.IsNullOrEmpty(holdID) Then
            objShipment.CarrierService.ServiceSurcharges.Add(New _baseServiceSurcharge(0, "HOLD_AT_LOCATION", "HOLD_AT_LOCATION", True))

        End If
        '
        ' 'FedEx One Rate' (flat rate for certain FedEx packaging) was added to the Buttons'Panel in ShipMaster.
        If _FedExWeb.IsEnabled_OneRate Then
            _Debug.Stop_("FEDEX_ONE_RATE")
            objShipment.CarrierService.ServiceSurcharges.Add(New _baseServiceSurcharge(0, "FEDEX_ONE_RATE", "FEDEX_ONE_RATE", True))
        End If
        '
        '' TODO: Enable with EVENT_NOTIFICATION tag after updated FedEx Web Services.
        '' EMAIL_NOTIFICATION changed to EVENT_NOTIFICATION in FedEx_Data2XML.GetShipmentSpecialServiceType()
        ''    If Not objShipment.Comments = "TinT Request" Then
        ''      If Not 0 = Len(objShipment.ShipFromContact.EMail) Or Not 0 = Len(objShipment.ShipToContact.EMail) Then
        ''          objShipment.CarrierService.ServiceSurcharges.Add( add_ServiceSurcharge(0, "EMAIL_NOTIFICATION", "EMAIL_NOTIFICATION", True))
        ''      End If
        ''    End If
        '
        If DateTime.Today < objShipment.CarrierService.ShipDate Then
            If GetPolicyData(gShipriteDB, "FedExREST_Enabled", "False") = False Then
                'FedEx REST does not have a futuer day shipment tag anymore. Only the shipdate is set forward.
                objShipment.CarrierService.ServiceSurcharges.Add(New _baseServiceSurcharge(0, "FUTURE_DAY_SHIPMENT", "FUTURE_DAY_SHIPMENT", True))
            End If

        End If
        '
        If Not 0 = Val(ExtractElementFromSegment("costFedEXHDCertain", SegmentSet)) Then
            objShipment.CarrierService.ServiceSurcharges.Add(New _baseServiceSurcharge(0, "HOME_DELIVERY_PREMIUM", "DATE_CERTAIN", True))
            ' User defined Delivery Date has to be transferred to FedEx Web Services in case of HomeDelivery Certain.
            If _Date.IsDate_(ExtractElementFromSegment("FEDEXDeliveryDate", SegmentSet)) Then
                objShipment.CarrierService.DeliveryDate = _Convert.String2Date(ExtractElementFromSegment("FEDEXDeliveryDate", SegmentSet))
            End If
        End If
        '
        If Not 0 = Val(ExtractElementFromSegment("costFedEXHDEvening", SegmentSet)) Then
            objShipment.CarrierService.ServiceSurcharges.Add(New _baseServiceSurcharge(0, "HOME_DELIVERY_PREMIUM", "EVENING", True))
        End If
        If Not 0 = Val(ExtractElementFromSegment("costFedEXHDAppt", SegmentSet)) Then
            objShipment.CarrierService.ServiceSurcharges.Add(New _baseServiceSurcharge(0, "HOME_DELIVERY_PREMIUM", "APPOINTMENT", True))
        End If
        If "Y" = UCase(ExtractElementFromSegment("InsideDelivery", SegmentSet)) Then
            objShipment.CarrierService.ServiceSurcharges.Add(New _baseServiceSurcharge(0, "INSIDE_DELIVERY", "INSIDE_DELIVERY", True))
        End If
        If "Y" = UCase(ExtractElementFromSegment("InsidePickup", SegmentSet)) Then
            objShipment.CarrierService.ServiceSurcharges.Add(New _baseServiceSurcharge(0, "INSIDE_PICKUP", "INSIDE_PICKUP", True))
        End If
        '
        If Not 0 = Val(ExtractElementFromSegment("actSAT", SegmentSet)) Then
            objShipment.CarrierService.ServiceSurcharges.Add(New _baseServiceSurcharge(0, "SATURDAY_DELIVERY", "SATURDAY_DELIVERY", True))
        End If
        If Not 0 = Val(ExtractElementFromSegment("actSATPU", SegmentSet)) Or gShip.SaturdayPickUp Then 'saturday pickup can have a $0.00 charge
            objShipment.CarrierService.ServiceSurcharges.Add(New _baseServiceSurcharge(0, "SATURDAY_PICKUP", "SATURDAY_PICKUP", True))
        End If
        '
        '' NOTE: "Rate Shipment" in Comments was old method used in FedEx DLL.
        '' These are set at the Package Level as indicated in previous comment below.
        '' FedEx Rate Shipment has no Package Level Surcharges - everything is at the Shipment Level.
        'If "Rate Shipment" = objShipment.Comments Then
        '    '
        '    Dim dryice As String = ExtractElementFromSegment("ABHazMat", SegmentSet).ToUpper
        '    If Not String.IsNullOrEmpty(dryice) Then
        '        objShipment.CarrierService.ServiceSurcharges.Add(add_ServiceSurcharge(0, "DRY_ICE", "DRY_ICE", True))
        '        objShipment.DryIce.WeightUnits = _Controls.Right(dryice, 2)
        '        objShipment.DryIce.Weight = Val(_Controls.Replace(dryice, objShipment.DryIce.WeightUnits, "").Trim)
        '    End If
        '    '
        '    If gShip.NonStandardContainer Then
        '        objShipment.CarrierService.ServiceSurcharges.Add(add_ServiceSurcharge(0, "NON_STANDARD_CONTAINER", "NON_STANDARD_CONTAINER", True))
        '    End If
        '    '
        '    Dim signaturetype As String = String.Empty
        '    If _SignatureType.Adult_Signature = Val(ExtractElementFromSegment("Fx_SigType", SegmentSet, "-1")) Then
        '        signaturetype = "Adult Signature"
        '    ElseIf _SignatureType.Direct_Signature = Val(ExtractElementFromSegment("Fx_SigType", SegmentSet, "-1")) Then
        '        signaturetype = "Direct Signature" ' Direct
        '    ElseIf _SignatureType.Indirect_Signature = Val(ExtractElementFromSegment("Fx_SigType", SegmentSet, "-1")) Then
        '        signaturetype = "Indirect Signature" ' Indirect
        '    ElseIf _SignatureType.No_Signature_Required = Val(ExtractElementFromSegment("Fx_SigType", SegmentSet, "-1")) Then
        '        signaturetype = "No Signature Required" ' No Signature Required
        '    End If
        '    If Not String.IsNullOrEmpty(signaturetype) Then
        '        objShipment.CarrierService.ServiceSurcharges.Add(add_ServiceSurcharge(0, "SIGNATURE_OPTION", signaturetype, True))
        '    End If
        '    '
        'End If
        '
    End Function
    Private Function add_ServiceSurcharges_Package(ByVal SegmentSet As String, ByRef objShipment As _baseShipment, ByRef objPack As _baseShipmentPackage) As Boolean
        add_ServiceSurcharges_Package = False ' assume.
        '
        Dim objCOD As New _baseServiceSurchargeCOD
        objCOD.Amount = Val(ExtractElementFromSegment("CODAMT", SegmentSet))
        If objCOD.Amount > 0 And _FedExWeb.IsGroundService(objShipment.CarrierService.ServiceABBR) Then
            '
            _Debug.Stop_("COD Package Level = " & objCOD.Amount)
            objPack.ServiceSurcharges.Add(New _baseServiceSurcharge(0, "COD", "COD", True))
            '
            With objCOD
                If _IDs.IsIt_CanadaShipper And objShipment.CarrierService.IsDomestic Then
                    .CurrencyType = "CAD"
                Else
                    .CurrencyType = "USD"
                End If
                .ChargeType = "" 'COD Recipient AccountNumber
                If CBool(ExtractElementFromSegment("CashiersCheck", SegmentSet, "False")) Then
                    .PaymentType = "0" '' GUARANTEED_FUNDS
                Else
                    ' COD with "certified check or money order only" unchecked should have 'ANY' payment type submitted to FedEx Web Server.
                    ' .PaymentType = "1" ' CASH
                    .PaymentType = "2" '' ANY 
                End If
                ' FedEx COD amount should stay as entered by the user since all the calculations FedEx Web Services will do automatically.
                .Amount = Val(ExtractElementFromSegment("CODAMTwSHIP", SegmentSet))
            End With
            '
            objPack.COD = objCOD ' FedEx COD for Ground shipment should be added at Package level only.
            '
        Else
            '
            objPack.COD = Nothing
            '
        End If
        '
        Dim dryice As String = ExtractElementFromSegment("ABHazMat", SegmentSet).ToUpper
        If Not String.IsNullOrEmpty(dryice) Then
            objPack.ServiceSurcharges.Add(New _baseServiceSurcharge(0, "DRY_ICE", "DRY_ICE", True))
            objPack.DryIce.WeightUnits = _Controls.Right(dryice, 2)
            objPack.DryIce.Weight = Val(_Controls.Replace(dryice, objPack.DryIce.WeightUnits, "").Trim)
        End If
        '
        If gShip.NonStandardContainer Then
            objPack.ServiceSurcharges.Add(New _baseServiceSurcharge(0, "NON_STANDARD_CONTAINER", "NON_STANDARD_CONTAINER", True))
        End If
        '
        Dim signaturetype As String = String.Empty
        If _SignatureType.Adult_Signature = Val(ExtractElementFromSegment("Fx_SigType", SegmentSet, "-1")) Then
            signaturetype = "Adult Signature"
        ElseIf _SignatureType.Direct_Signature = Val(ExtractElementFromSegment("Fx_SigType", SegmentSet, "-1")) Then
            signaturetype = "Direct Signature" ' Direct
        ElseIf _SignatureType.Indirect_Signature = Val(ExtractElementFromSegment("Fx_SigType", SegmentSet, "-1")) Then
            signaturetype = "Indirect Signature" ' Indirect
        ElseIf _SignatureType.No_Signature_Required = Val(ExtractElementFromSegment("Fx_SigType", SegmentSet, "-1")) Then
            signaturetype = "No Signature Required" ' No Signature Required
        End If
        If Not String.IsNullOrEmpty(signaturetype) Then
            objPack.ServiceSurcharges.Add(New _baseServiceSurcharge(0, "SIGNATURE_OPTION", signaturetype, True))
        End If
        '
    End Function

    Public Function Upload_Shipment(ByVal objShipment As _baseShipment, Optional NoDelete As Boolean = False, Optional showConfirmMsg As Boolean = True) As Boolean
        ''
        Upload_Shipment = True ' assume.
        ''
        Dim sql2exe As String = String.Empty
        Dim p%
        Dim Success As String = String.Empty
        Dim fileCount As String = String.Empty
        ''
        Dim objResponse As New baseWebResponse_Shipment
        Dim Pack As baseWebResponse_Package
        Dim objPackage As _baseShipmentPackage
        ''
        Try

            If FedEx.IsWebServicesReady Or GetPolicyData(gShipriteDB, "FedExREST_Enabled", "False") Then
                If Not objShipment Is Nothing Then
                    '
                    If objShipment.Packages.Count > 0 Then
                        '
                        objResponse.ShipmentID = "" ' string is an object in .Net and could not be Nothing
                        objResponse.AdditionalInfo = ""
                        objResponse.DeliveryDate = DateTime.Today
                        objResponse.DeliveryDay = ""
                        For p% = 0 To objShipment.Packages.Count - 1
                            ' add number if response packages
                            Pack = New baseWebResponse_Package
                            objPackage = objShipment.Packages(p%)
                            Pack.PackageID = objPackage.PackageID
                            '' 'FedEx Open Ship' shipment ID is the first Package ID of the whole shipment.
                            If p% = 0 Then
                                objShipment.ShipmentNo = Pack.PackageID
                                objResponse.ShipmentID = Pack.PackageID
                            End If ''ol#9.195(1/24).
                            Pack.TrackingNo = ""
                            Pack.LabelImage = ""
                            objShipment.Packages(p%).SequenceNo = p% + 1
                            Pack.SequenceNo = objPackage.SequenceNo '' 'SequenceNo' property was added to the 'baseWebResponse_Package' class for Multi-Ship mode.
                            objResponse.Packages.Add(Pack)
                        Next p%
                        '
                        If FedExWeb_ShipIt_GoOnlineNow(objShipment, objResponse) Then
                            '
                            '
                            Pack = objResponse.Packages(0)
                            If objResponse.ShipmentAlerts.Count > 0 And 0 = Len(Pack.TrackingNo) Then
                                '
                                MsgBox("FedEx Error:" & vbCr & objResponse.ShipmentAlerts(0), vbCritical, _FedExWeb.WebServTitle)
                                If "Commercial Invoice not allowed for origin destination" = objResponse.ShipmentAlerts(0) Then
                                    If FedEx.NoCommInvoiceNeeded_AddCountryToFile(objShipment.ShipToContact.Country) Then
                                        MsgBox("[" & objShipment.ShipToContact.Country & "] has been added to the FedEx list of countries that don't need Commercial Invoice!" & vbCr & vbCr &
                                       "Please try again to process this shipment...", vbInformation)
                                    End If
                                End If
                                gResult = "Cancel"
                                gShip.PackageID = String.Empty ''  don't record the package that failed.
                                Upload_Shipment = False
                                '
                            Else
                                '
                                If objResponse.ShipmentAlerts.Count > 0 Then
                                    '
                                    Dim alerts As String = String.Empty
                                    For p% = 0 To objResponse.ShipmentAlerts.Count - 1
                                        alerts = alerts & vbCr & objResponse.ShipmentAlerts(p%)
                                    Next p%
                                    '
                                    MsgBox("There were some FedEx alerts in the response: " & vbCr & alerts, vbExclamation, "FedEx Alerts!")
                                    '
                                End If
                                '
                                For p% = 0 To objResponse.Packages.Count - 1
                                    '
                                    Dim retpack As New baseWebResponse_Package
                                    retpack = objResponse.Packages(p%)
                                    _Debug.Print_("PACK#: " & retpack.PackageID)
                                    _Debug.Print_("FEDEX #: " & retpack.TrackingNo)
                                    '
                                    If Not 0 = Len(retpack.TrackingNo) Then
                                        '
                                        '' To Do
                                        ''For M% = 0 To UBound(gManifest) - 1
                                        ''    ' find the right package in Manifest array
                                        ''    If gManifest(M%).PID = retpack.PackageID Then
                                        ''        gManifest(M%).TrackingNumber = retpack.TrackingNo
                                        ''        Exit For
                                        ''    End If
                                        ''Next M%
                                        '
                                        Dim sql2cmd As New sqlUpdate
                                        If Not NoDelete Then
                                            '
                                            Call sql2cmd.Qry_UPDATE("Exported", EOD.PickupWaitingStatus, sql2cmd.TXT_, True, False, "Manifest", "PackageID = '" & retpack.PackageID & "'")
                                            '
                                        Else
                                            Call sql2cmd.Qry_UPDATE("Exported", EOD.ExportedStatus, sql2cmd.TXT_, True, False, "Manifest", "PackageID = '" & retpack.PackageID & "'")
                                            '
                                        End If
                                        Call sql2cmd.Qry_UPDATE("ReferralSource", "XML", sql2cmd.TXT_)
                                        Call sql2cmd.Qry_UPDATE("Date", String.Format("{0:MM/dd/yyyy}", objShipment.CarrierService.ShipDate), sql2cmd.DTE_)
                                        sql2exe = sql2cmd.Qry_UPDATE("TRACKING#", retpack.TrackingNo, sql2cmd.TXT_, False, True)
                                        _Debug.Print_(sql2exe)
                                        If -1 = IO_UpdateSQLProcessor(gShipriteDB, sql2exe) Then
                                            MsgBox("Failed to update Manifest with " & _FedExWeb.WebServTitle & " tracking number...", MsgBoxStyle.Critical)
                                            Upload_Shipment = False
                                        End If
                                        '
                                    End If
                                    '
                                Next p%
                                '
                                If Not 0 = Len(Success) Then
                                    MsgBox(_FedExWeb.WebServTitle & " Success" & Success, vbInformation)
                                End If
                                '
                            End If
                            '
                        Else
                            '
                            Upload_Shipment = False
                            '
                        End If
                        '
                    Else
                        '
                        Upload_Shipment = False
                        '
                    End If
                    ''
                    If Not NoDelete Then
                        For p% = objResponse.Packages.Count - 1 To 0 Step -1
                            Pack = New baseWebResponse_Package
                            Pack = objResponse.Packages(p%)
                            If 0 = Len(Pack.TrackingNo) Then
                                '
                                '' To Do
                                '' Call Shipping.gManifest_RemovePackageID(Pack.PackageID) '' 'Ship Multi' doesn't work with shipments going to different addresses.
                                '
                                sql2exe = "Delete * From Manifest Where PackageID = '" & Pack.PackageID & "'"
                                _Debug.Print_(sql2exe)
                                Call IO_UpdateSQLProcessor(gShipriteDB, sql2exe)
                                sql2exe = "Delete * From CustomsItems Where PackageID = '" & Pack.PackageID & "'"
                                Call IO_UpdateSQLProcessor(gShipriteDB, sql2exe)
                                '
                                Call objResponse.Packages.Remove(Pack)
                                Upload_Shipment = False
                                '
                            Else
                                '
                                If Not _Debug.IsINHOUSE Then
                                    If Convert.ToBoolean(General.GetPolicyData(gShipriteDB, "DuplicateLabel", "False")) Then
                                        Call _FedExWeb.print_LabelFromFile(_FedExWeb.objFedEx_Setup.LabelImageType, Pack.PackageID, _FedExWeb.objFedEx_Setup.Path_SaveDocXML, fileCount)
                                        Call _FedExWeb.print_LabelFromFile(_FedExWeb.objFedEx_Setup.LabelImageType, Pack.PackageID, _FedExWeb.objFedEx_Setup.Path_SaveDocXML, "COD" & fileCount)
                                        Call _FedExWeb.print_LabelFromFile(_FedExWeb.objFedEx_Setup.LabelImageType, Pack.PackageID, _FedExWeb.objFedEx_Setup.Path_SaveDocXML, fileCount)
                                        Call _FedExWeb.print_LabelFromFile(_FedExWeb.objFedEx_Setup.LabelImageType, Pack.PackageID, _FedExWeb.objFedEx_Setup.Path_SaveDocXML, "COD" & fileCount)
                                        '
                                        Call _FedExWeb.print_LabelFromFile("Laser", Pack.PackageID, _FedExWeb.objFedEx_Setup.Path_SaveDocXML, "FreightBOL" & fileCount)
                                        Call _FedExWeb.print_LabelFromFile(_FedExWeb.objFedEx_Setup.LabelImageType, Pack.PackageID, _FedExWeb.objFedEx_Setup.Path_SaveDocXML, "Freight" & fileCount)
                                        Call _FedExWeb.print_LabelFromFile("Laser", Pack.PackageID, _FedExWeb.objFedEx_Setup.Path_SaveDocXML, "FreightBOL" & fileCount)
                                        Call _FedExWeb.print_LabelFromFile(_FedExWeb.objFedEx_Setup.LabelImageType, Pack.PackageID, _FedExWeb.objFedEx_Setup.Path_SaveDocXML, "Freight" & fileCount)
                                    Else
                                        Call _FedExWeb.print_LabelFromFile(_FedExWeb.objFedEx_Setup.LabelImageType, Pack.PackageID, _FedExWeb.objFedEx_Setup.Path_SaveDocXML, fileCount)
                                        Call _FedExWeb.print_LabelFromFile(_FedExWeb.objFedEx_Setup.LabelImageType, Pack.PackageID, _FedExWeb.objFedEx_Setup.Path_SaveDocXML, "COD" & fileCount)
                                        '
                                        Call _FedExWeb.print_LabelFromFile("Laser", Pack.PackageID, _FedExWeb.objFedEx_Setup.Path_SaveDocXML, "FreightBOL" & fileCount)
                                        Call _FedExWeb.print_LabelFromFile(_FedExWeb.objFedEx_Setup.LabelImageType, Pack.PackageID, _FedExWeb.objFedEx_Setup.Path_SaveDocXML, "Freight" & fileCount)
                                    End If
                                End If
                                '
                            End If
                        Next p%
                    End If
                    '
                End If
            End If

        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to upload Package to FedEx...")
        End Try
    End Function

    Public Function FedExWeb_ShipIt_GoOnlineNow(ByVal fedexShipment As _baseShipment, ByRef webResponse As baseWebResponse_Shipment) As Boolean
        FedExWeb_ShipIt_GoOnlineNow = False ' assume.
        _Debug.Print_(objFedEx_Setup.Csp_AccountNumber)

        If (fedexShipment.CarrierService.ServiceABBR = "FEDEX-FRP" Or fedexShipment.CarrierService.ServiceABBR = "FEDEX-FRE") And Not FedEx_Freight.IsFreightBoxPackaging(fedexShipment.Packages(0).PackagingType) Then
            '
            Return _FedExWeb.Process_Ship_Freight(fedexShipment, webResponse)
        Else
            If GetPolicyData(gShipriteDB, "FedExREST_Enabled", "False") Then
                Return FedEx_REST.FXR_Process_Shipment(fedexShipment, webResponse)
            Else
                Return _FedExWeb.Process_ShipAPackage(fedexShipment, webResponse)
            End If

        End If
    End Function
#End Region

#Region "Printing Label"

    Public Function print_LabelFromFile(ByVal LabelImageType As String, ByVal flePackageID As String, ByVal dir2DocXML As String, Optional fileCount As String = "") As Boolean
        print_LabelFromFile = False
        ''
        Dim imageFile As String = String.Empty
        Dim PrinterName As String
        ''
        Try
            ''
            _FedExWeb.IsLabelPrintedSuccessfully = False '' assume.
            ''
            If _Controls.Contains(LabelImageType, "Thermal") Then
                PrinterName = GetPolicyData(gReportsDB, "LabelPrinter")
                '
                imageFile = dir2DocXML & "\" & flePackageID & "_Label" & fileCount & ".txt"
                _Debug.Print_(imageFile)
                If _Files.IsFileExist(imageFile, False) Then
                    '
                    print_LabelFromFile = RawPrinterHelper.SendFileToPrinter(PrinterName, imageFile)
                    '
                End If
                '
            Else
                '
                '' To Do
                PrinterName = GetPolicyData(gReportsDB, "ReportPrinter")
                imageFile = dir2DocXML & "\" & flePackageID & "_Label" & fileCount & ".pdf"
                ''_Debug.Print_(imageFile)
                If _Files.IsFileExist(imageFile, False) Then
                    'print_LabelFromFile = Printing_.Print_FilePDF(&O0, imageFile)
                    'print_LabelFromFile = RawPrinterHelper.SendFileToPrinter(PrinterName, imageFile)
                    Process.Start(imageFile)
                End If
                '
            End If
                ''
                _FedExWeb.IsLabelPrintedSuccessfully = print_LabelFromFile
            ''
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to print label...")
        End Try

    End Function


#End Region
End Module