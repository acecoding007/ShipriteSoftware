
Public Module FedEx_Data2XML

    Private m_Path_SOAPEnvelope As String
    Private m_Path_SaveDocXML As String

    Private m_Connect2ServerURL As String
    ''
    Private m_CspCredential_Key As String
    Private m_CspCredential_Pass As String
    Private m_UserCredential_Key As String
    Private m_UserCredential_Pass As String
    ''
    Private m_CspSolutionId As String
    Private m_CspType As String
    Private m_CspAccountNumber As String
    ''
    Private m_AccountNumber As String
    Private m_MeterNumber As String
    Private m_ClientProductId As String
    Private m_ClientProductVersion As String
    ''
    Private m_ApplicationId As String
    ''
    Private m_OriginLocationId As String
    ''
    Private m_PaymentType As String
    ''
    Private m_DropoffType As String
    ''
    Private m_LabelImageType As String
    Private m_LabelFormatType As String
    Private m_LabelStockType As String
    ''
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

    Private Sub error_DebugPint(ByVal routineName As String, ByVal errorDesc As String)
        MsgBox(String.Format("FedEx_Data2XML.{0}(): {1}", routineName, errorDesc), MsgBoxStyle.Exclamation)
        _Debug.Print_(String.Format("FedEx_Data2XML.{0}(): {1}", routineName, errorDesc))
    End Sub

#Region "Test"
    Public Function Test_LoadCredentials() As Boolean
        ''
        m_Path_SOAPEnvelope = "C:\ShipRite\Fedex"
        m_Path_SaveDocXML = "C:\ShipRite\Fedex\Test"
        ''
        m_Connect2ServerURL = "https://ws.fedex.com:443/web-services" '' Product URL
        ''
        m_CspCredential_Key = "QFdKk9EIx1jJ5Tdc" '' CSP credential Key
        m_CspCredential_Pass = "yYbSG24IEfUD8ALLUYcg3Uurb" '' CSP credential Password
        ''
        '* Register user returned values:
        m_UserCredential_Key = "UZkngbZK92lKIZMN" '*
        m_UserCredential_Pass = "ov01FpFZCmuRCq25ZFPLitfOk" '*
        m_MeterNumber = "104654558" '*
        ''
        m_CspSolutionId = "059" '' Solution Type: 059
        m_CspType = "CERTIFIED_SOLUTION_PROVIDER"
        m_CspAccountNumber = "280062448" ' Canada "602289028" '' Test Account Number
        ''
        m_AccountNumber = "280062448" ' Canada "602289028" ' Client Account#
        m_ClientProductId = "SRGC" '' Client Product Id
        m_ClientProductVersion = "7232" '' Client Product version
        ''
        '' Version Capture Code: TSPH5652
        ''
        m_OriginLocationId = "SNAP2" ' our FedEx HAL Location ID
        m_ApplicationId = "5202506" ' our FedEx HAL Agent ID
        ''
        m_DropoffType = "REGULAR_PICKUP"
        m_PaymentType = "SENDER"
        m_LabelImageType = "Laser"
        m_LabelFormatType = "COMMON2D" '' the other option: "LABEL_DATA_ONLY"
        m_LabelStockType = "Thermal 4x6"
        ''
        Test_LoadCredentials = True
        ''
    End Function
    Public Function Test_Load_TestCredentials() As Boolean
        ''
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

        m_Path_SOAPEnvelope = "C:\ShipRite\Fedex"
        m_Path_SaveDocXML = "C:\ShipRite\Fedex\Test"
        ''
        m_Connect2ServerURL = "https://wsbeta.fedex.com:443/web-services" '' Test URL
        ''
        m_CspCredential_Key = "2U39xPqXhhm3DdiA" '' CSP credential Key
        m_CspCredential_Pass = "h3ZXnSQZ5Np7AxKzPaQUurRv6" '' CSP credential Password
        ''
        '* Register user returned values:
        m_UserCredential_Key = "cPcId3RsuJq4dAoR" '*
        m_UserCredential_Pass = "W2hvH4iMct6eHhr3y24AlulUz" '*
        m_MeterNumber = "100332406" '*
        ''
        m_CspSolutionId = "059" '' Solution Type: 059
        m_CspType = "CERTIFIED_SOLUTION_PROVIDER"
        m_CspAccountNumber = "609152567" ' Canada "609167505" '' Test Account Number
        ''
        m_AccountNumber = "609152567" ' Client Account#
        m_ClientProductId = "SRGC" '' Client Product Id
        m_ClientProductVersion = "7000" '' Client Product version
        ''
        m_OriginLocationId = "SNAP2" ' our FedEx HAL Location ID
        m_ApplicationId = "5202506" ' our FedEx HAL Agent ID
        ''
        m_DropoffType = "REGULAR_PICKUP"
        m_PaymentType = "SENDER"
        m_LabelImageType = "Laser"
        m_LabelFormatType = "COMMON2D" '' the other option: "LABEL_DATA_ONLY"
        m_LabelStockType = "Thermal 4x6"
        ''
        Test_Load_TestCredentials = (Not 0 = m_MeterNumber.Length)
        ''
    End Function

#End Region

    Public Function Load_Credentials_FromDatabase(ByVal gDBpath As String) As Boolean
        '
        ''
        m_Path_SOAPEnvelope = gDBpath & "\FedEx"
        m_Path_SaveDocXML = gDBpath & "\FedEx\InOut"
        ''
        m_Connect2ServerURL = "https://ws.fedex.com:443/web-services" '' Product URL
        ''
        m_CspCredential_Key = "QFdKk9EIx1jJ5Tdc" '' CSP credential Key
        m_CspCredential_Pass = "yYbSG24IEfUD8ALLUYcg3Uurb" '' CSP credential Password
        ''
        m_ClientProductId = "SRGC" '' Client Product Id
        m_ClientProductVersion = "6000" '"7232" '' Client Product version
        ''
        m_CspSolutionId = "059" '' Solution Type: 059
        m_CspType = "CERTIFIED_SOLUTION_PROVIDER"
        ' m_CspAccountNumber = _Convert.Null2DefaultValue(dreader("FedExAccountNumber"))
        m_CspAccountNumber = GetPolicyData(gShipriteDB, "FedExAccountNumber")
        ''
        '* Register user returned values:
        ' m_UserCredential_Key = _Convert.Null2DefaultValue(dreader("FedExIOPort"))
        ' m_UserCredential_Pass = _Convert.Null2DefaultValue(dreader("FedExPassword"))
        ' m_MeterNumber = _Convert.Null2DefaultValue(dreader("FedExMeter"))
        m_UserCredential_Key = GetPolicyData(gShipriteDB, "FedExIOPort")
        m_UserCredential_Pass = GetPolicyData(gShipriteDB, "FedExPassword")
        m_MeterNumber = GetPolicyData(gShipriteDB, "FedExMeter")
        ''
        m_AccountNumber = m_CspAccountNumber
        ''
        m_OriginLocationId = GetPolicyData(gShipriteDB, "FedExHAL_LocationID")
        m_ApplicationId = GetPolicyData(gShipriteDB, "FedExHAL_AgentID")
        'ShipRiteDb.Setup2_GetFedExHAL_IDs(m_OriginLocationId, m_ApplicationId)
        'm_OriginLocationId = "UCAA" ' our FedEx HAL Location ID
        'm_ApplicationId = "5202506" ' our FedEx HAL Agent ID
        ''
        m_DropoffType = "REGULAR_PICKUP"
        m_PaymentType = "SENDER"
        m_LabelFormatType = "COMMON2D" '' the other option: "LABEL_DATA_ONLY"
        ''
        If Not ReportsDb.Get_FedExLabelType = "Laser" Then
            m_LabelImageType = "Eltron Thermal"
            m_LabelStockType = "Thermal 4x6"
        Else
            m_LabelImageType = "PDF Image"
            m_LabelStockType = "" '' optional
        End If
        ''
        Load_Credentials_FromDatabase = (Not String.IsNullOrEmpty(m_UserCredential_Key.Length) And Not String.IsNullOrEmpty(m_UserCredential_Pass.Length) And Not String.IsNullOrEmpty(m_MeterNumber.Length))
    End Function

    Private Function getLabel_StockType(ByVal labelStockType As String) As String
        ''
        ''	For thermal printer labels this indicates the size of the label and 
        ''  the location of the doc tab if present.
        ''
        getLabel_StockType = String.Empty '' assume.
        'Try
        Dim tmp As String = String.Empty
        Select Case labelStockType
            Case "Laser 4x6" : tmp = "PAPER_4X6"
            Case "Laser 7x4.75" : tmp = "PAPER_7X4.75"
            Case "Thermal 4x6" : tmp = "STOCK_4X6"
            Case "Thermal 4x6.75" : tmp = "STOCK_4X6.75_LEADING_DOC_TAB"
            Case "Thermal 4x8" : tmp = "STOCK_4X8"
            Case "Thermal 4x9" : tmp = "STOCK_4X9_LEADING_DOC_TAB"
            Case Else : tmp = "STOCK_4X6"
        End Select
        getLabel_StockType = tmp
        ''
        'Catch ex As Exception : error_DebugPint("GetServiceType", ex.Message)
        'End Try
        ''
    End Function
    Private Function getLabel_ImageType(ByVal labelImageType As String) As String
        ''
        '' The type of image or printer commands the label is to be formatted in.
        '					  DPL = Unimark thermal printer language
        '					  EPL2 = Eltron thermal printer language
        '					  PDF = a label returned as a pdf image
        '					  PNG = a label returned as a png image
        '					  ZPLII = Zebra thermal printer language
        getLabel_ImageType = String.Empty '' assume.
        Select Case labelImageType
            Case "Zebra Thermal" : getLabel_ImageType = "ZPLII"
            Case "Eltron Thermal" : getLabel_ImageType = "EPL2"
            Case "Unimark Thermal" : getLabel_ImageType = "DPL"
            Case "PDF Image", "Laser" : getLabel_ImageType = "PDF"
            Case "PNG Image" : getLabel_ImageType = "PNG"
            Case Else : getLabel_ImageType = "EPL2"
        End Select
        ''
    End Function
    Public Function GetLabel_FileExtension(ByVal labelImageType As String) As String
        GetLabel_FileExtension = "txt" ' assume Thermal
        If Not _Controls.Contains(labelImageType, "Thermal") Then
            GetLabel_FileExtension = "pdf"
        End If
    End Function

    Private Sub createXMLFolders()
        _Files.Create_Folder(_FedExWeb.objFedEx_Setup.Path_SOAPEnvelope, True)
        _Files.Create_Folder(_FedExWeb.objFedEx_Setup.Path_SaveDocXML, True)
    End Sub

    Public Function GetShipDate_IntlFormat(ByVal shipDate As Date) As String
        GetShipDate_IntlFormat = shipDate.ToString("yyyy-MM-dd")
    End Function
    Public Function GetShipTimeStamp(ByVal shipDate As Date) As String
        ''
        '' e.g 2006-06-26T17:00:00-0400 is defined form June 26, 2006 5:00 pm Eastern Time
        GetShipTimeStamp = shipDate.ToString("yyyy-MM-ddTHH:mm:ss")
        ''
        Dim utcDiff As System.TimeSpan
        If _Date.UTC_TimeSpan(shipDate, utcDiff) Then
            '' UTC offset component indicating the number of hours/mainutes from UTC
            Dim hrs As String = utcDiff.Hours.ToString
            Dim min As String = utcDiff.Minutes.ToString
            If 1 = hrs.Length Then hrs = String.Format("0{0}", hrs)
            If 1 = min.Length Then min = String.Format("0{0}", min)
            GetShipTimeStamp = String.Format("{0}-{1}:{2}", GetShipTimeStamp, hrs, min)
        End If
        ''
    End Function

#Region "Types Required"
    Public Function GetPackagingType(ByVal srPackagingType As String) As FedEx_ShipService.PackagingType
        ''
        'GetPackagingType = FedEx_ShipService.PackagingType.YOUR_PACKAGING '' assume.
        ''Try
        'If _Controls.Contains(srPackagingType, "Letter") Or _Controls.Contains(srPackagingType, "Env") Then
        '    GetPackagingType = FedEx_ShipService.PackagingType.FEDEX_ENVELOPE
        'ElseIf _Controls.Contains(srPackagingType, "Box") And _Controls.Contains(srPackagingType, "10kg") Then
        '    GetPackagingType = FedEx_ShipService.PackagingType.FEDEX_10KG_BOX
        'ElseIf _Controls.Contains(srPackagingType, "Box") And _Controls.Contains(srPackagingType, "25kg") Then
        '    GetPackagingType = FedEx_ShipService.PackagingType.FEDEX_25KG_BOX
        'ElseIf _Controls.Contains(srPackagingType, "Box") Then
        '    GetPackagingType = FedEx_ShipService.PackagingType.FEDEX_BOX
        'ElseIf _Controls.Contains(srPackagingType, "Pak") Then
        '    GetPackagingType = FedEx_ShipService.PackagingType.FEDEX_PAK
        'ElseIf _Controls.Contains(srPackagingType, "Tube") Then
        '    GetPackagingType = FedEx_ShipService.PackagingType.FEDEX_TUBE
        'End If
        '''

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
    End Function
    Public Function GetServiceType(ByVal serviceABBR As String) As FedEx_ShipService.ServiceType
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
                                ''ol#1.2.69(5/8)... FedEx Freight Box services were added.
            Case "FEDEX-FRP" : GetServiceType = FedEx_ShipService.ServiceType.FEDEX_FREIGHT_PRIORITY
            Case "FEDEX-FRE" : GetServiceType = FedEx_ShipService.ServiceType.FEDEX_FREIGHT_ECONOMY
        End Select
        ''
    End Function
    Public Function GetSignatureOptionType(ByVal signatureOption As String) As FedEx_ShipService.SignatureOptionType
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
    Public Function GetCarrierCodeType(ByVal serviceABBR As String) As FedEx_ShipService.CarrierCodeType
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
    Public Function GetCodAddTransportationChargesType(ByVal optionType As Integer) As FedEx_ShipService.CodAddTransportationChargeBasisType
        GetCodAddTransportationChargesType = FedEx_ShipService.CodAddTransportationChargeBasisType.COD_SURCHARGE ' assume.
        Select Case optionType
            Case CODChargeType_ADD_ACCOUNT_NET_CHARGE : GetCodAddTransportationChargesType = FedEx_ShipService.CodAddTransportationChargeBasisType.NET_CHARGE
            Case CODChargeType_ADD_ACCOUNT_NET_FREIGHT : GetCodAddTransportationChargesType = FedEx_ShipService.CodAddTransportationChargeBasisType.NET_FREIGHT
            Case CODChargeType_ADD_ACCOUNT_TOTAL_CUSTOMER_CHARGE : GetCodAddTransportationChargesType = FedEx_ShipService.CodAddTransportationChargeBasisType.TOTAL_CUSTOMER_CHARGE
        End Select
    End Function
    Public Function GetCodCollectionType(ByVal optionType As Integer) As FedEx_ShipService.CodCollectionType
        GetCodCollectionType = FedEx_ShipService.CodCollectionType.GUARANTEED_FUNDS ' assume.
        Select Case optionType
            Case CODPaymentType_ANY : GetCodCollectionType = FedEx_ShipService.CodCollectionType.ANY
            Case CODPaymentType_CASH : GetCodCollectionType = FedEx_ShipService.CodCollectionType.CASH
        End Select
    End Function
    Public Function GetHomeDeliveryPremiumType(ByVal optionType As String) As FedEx_ShipService.HomeDeliveryPremiumType
        GetHomeDeliveryPremiumType = FedEx_ShipService.HomeDeliveryPremiumType.EVENING
        If _Controls.Contains(optionType, "APPOINTMENT") Then
            GetHomeDeliveryPremiumType = FedEx_ShipService.HomeDeliveryPremiumType.APPOINTMENT
        ElseIf _Controls.Contains(optionType, "DATE") AndAlso _Controls.Contains(optionType, "CERTAIN") Then
            GetHomeDeliveryPremiumType = FedEx_ShipService.HomeDeliveryPremiumType.DATE_CERTAIN
        End If
    End Function
    Public Function GetPaymentType(ByVal type As String, ByVal shipment As _baseShipment, ByRef payor As _baseContact) As FedEx_ShipService.PaymentType
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
                Case Else
                    ''ol#1.1.77(10/13)... ShippingChargesPayment - Payor is required to have full address specified - account# is not enough now.
                    payor = shipment.ShipperContact
            End Select
        Else
            payor = shipment.ShipperContact
        End If
    End Function
    Public Function GetPaymentType_Freight(ByVal type As String, ByVal shipment As _baseShipment, ByRef payor As _baseContact) As FedEx_ShipService.PaymentType
        ''ol#1.2.69(5/8)... FedEx Freight Box services were added.
        GetPaymentType_Freight = FedEx_ShipService.PaymentType.SENDER
        Select Case type
            Case "ACCOUNT" : GetPaymentType_Freight = FedEx_ShipService.PaymentType.ACCOUNT
                payor = shipment.ShipperContact
            Case "COLLECT" : GetPaymentType_Freight = FedEx_ShipService.PaymentType.COLLECT
                payor = shipment.ShipToContact
            Case "RECIPIENT" : GetPaymentType_Freight = FedEx_ShipService.PaymentType.RECIPIENT
                payor = shipment.ShipToContact
            Case "RECIPIENT-TEST-ONLY" : GetPaymentType_Freight = FedEx_ShipService.PaymentType.RECIPIENT
                payor = shipment.ShipToContact
            Case "THIRD_PARTY" : GetPaymentType_Freight = FedEx_ShipService.PaymentType.THIRD_PARTY
                payor = shipment.ShipperContact
            Case Else
                ''ol#1.1.77(10/13)... ShippingChargesPayment - Payor is required to have full address specified - account# is not enough now.
                payor = shipment.ShipperContact
        End Select
    End Function

    Public Function GetInternationalDocumentContentType(ByVal isShippingDocumentsOnly As Boolean) As FedEx_ShipService.InternationalDocumentContentType
        ''
        If isShippingDocumentsOnly Then
            GetInternationalDocumentContentType = FedEx_ShipService.InternationalDocumentContentType.DOCUMENTS_ONLY
        Else
            GetInternationalDocumentContentType = FedEx_ShipService.InternationalDocumentContentType.NON_DOCUMENTS
        End If
        ''
    End Function
    Public Function GetPurposeOfShipmentType(ByVal type As String) As FedEx_ShipService.PurposeOfShipmentType
        GetPurposeOfShipmentType = FedEx_ShipService.PurposeOfShipmentType.NOT_SOLD
        If _Controls.Contains(type, "Sample") Then
            GetPurposeOfShipmentType = FedEx_ShipService.PurposeOfShipmentType.SAMPLE
        ElseIf _Controls.Contains(type, "Return") AndAlso _Controls.Contains(type, "Goods") Then
            GetPurposeOfShipmentType = FedEx_ShipService.PurposeOfShipmentType.REPAIR_AND_RETURN
        ElseIf _Controls.Contains(type, "Other") Then
            GetPurposeOfShipmentType = FedEx_ShipService.PurposeOfShipmentType.PERSONAL_EFFECTS
        ElseIf _Controls.Contains(type, "Gift") Then
            GetPurposeOfShipmentType = FedEx_ShipService.PurposeOfShipmentType.GIFT
        End If
    End Function
#End Region
#Region "Types Optional"
    Public Function GetShipmentSpecialServiceType(ByVal optionType As String, ByRef type As FedEx_ShipService.ShipmentSpecialServiceType) As Boolean
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
    Public Function GetPackageSpecialServiceType(ByVal optionType As String, ByRef type As FedEx_ShipService.PackageSpecialServiceType) As Boolean
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
    Public Function GetB13AFilingOptionType(ByVal optionType As String, ByRef type As FedEx_ShipService.B13AFilingOptionType) As Boolean
        GetB13AFilingOptionType = (Not 0 = optionType.Length) ' assume required
        Select Case optionType
            Case "FILED_ELECTRONICALLY" : type = FedEx_ShipService.B13AFilingOptionType.FILED_ELECTRONICALLY
            Case "MANUALLY_ATTACHED" : type = FedEx_ShipService.B13AFilingOptionType.MANUALLY_ATTACHED
            Case "SUMMARY_REPORTING" : type = FedEx_ShipService.B13AFilingOptionType.SUMMARY_REPORTING
            Case Else : type = FedEx_ShipService.B13AFilingOptionType.NOT_REQUIRED
        End Select
    End Function
#End Region
#Region "Types Un-used"
    Public Function GetCodReturnReferenceIndicatorType(ByVal optionType As String) As String
        ''
        Dim tmp As String = String.Empty
        Select Case optionType
            Case "x" : tmp = "INVOICE"
            Case "x" : tmp = "PO"
            Case "x" : tmp = "REFERENCE"
            Case "x" : tmp = "TRACKING"
        End Select
        ''
        GetCodReturnReferenceIndicatorType = tmp
        ''
    End Function
    Public Function GetTermsOfSaleType(ByVal optionType As Integer) As String
        ''
        ' Required for dutiable international express or ground shipment. 
        ' This field is not applicable to an international PIB (document) or a non-document 
        ' which does not require a commercial invoice express shipment.
        '							  CFR_OR_CPT (Cost and Freight/Carriage Paid TO)
        '							  CIF_OR_CIP (Cost Insurance and Freight/Carraige Insurance Paid)
        '							  DDP (Delivered Duty Paid)
        '							  DDU (Delivered Duty Unpaid)
        '							  EXW (Ex Works)
        '							  FOB_OR_FCA (Free On Board/Free Carrier)
        Dim tmp As String = String.Empty
        Select Case optionType
            Case IntTermsOfSale_CFR_OR_CPT : tmp = "CFR_OR_CPT"
            Case IntTermsOfSale_CIF_OR_CIP : tmp = "CIF_OR_CIP"
            Case IntTermsOfSale_DDP : tmp = "DDP"
            Case IntTermsOfSale_DDU : tmp = "DDU"
            Case IntTermsOfSale_EXW : tmp = "EXW"
            Case IntTermsOfSale_FOB_OR_FCA : tmp = "FOB_OR_FCA"
        End Select
        GetTermsOfSaleType = tmp
        ''
    End Function
#End Region

    Public Property OriginLocationId() As String
        Get
            Return m_OriginLocationId
        End Get
        Set(ByVal value As String)
        End Set
    End Property

    Public Property ApplicationId() As String
        Get
            Return m_ApplicationId
        End Get
        Set(ByVal value As String)
        End Set
    End Property

End Module

