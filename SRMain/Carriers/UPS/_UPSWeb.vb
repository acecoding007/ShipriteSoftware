Imports System.Collections.Generic
Imports SHIPRITE.UPS_TinTWebReference
Imports SHIPRITE.UPS_ShipWebReference
Imports SHIPRITE.UPS_VoidWebReference
Imports SHIPRITE.UPS_RegWebReference
Imports System.Web.Services
Imports System.Xml.Serialization
Imports System.IO
Imports System.Text
Imports System.Net

Public Class UPSSetupData

    Public ShipRite_AccessLicenseNumber As String '= "6C9A3E22CD9306E8"
    Public ShipRite_DeveloperLicenseNumber As String '= "2CB2702381CB5253"
    Public ShipRite_AccessLicensingOnlyNumber As String '= "ECB270290C495E56"
    Public ShipRite_Username As String '= "ticketrite"
    Public ShipRite_Password As String '= "Shiprite1312"
    Public ShipRite_ShipperNumber As String '= "114038"
    Public ShipRite_PaymentType As String '= "01" '01 = Transportation is required; 02 = Duties and Taxes is not required

    Public LabelImageType As String '= "ELP"
    Public LabelStockSize_Height As String '= "6"
    Public LabelStockSize_Width As String '= "4"
    Public LabelFilePath As String '= "C:\test\label.txt"

    Public Sub New()
        '
        ShipRite_AccessLicenseNumber = General.GetPolicyData(gShipriteDB, _ReusedField.fldUPSWeb_UserLicense) '"6C9A3E22CD9306E8" '"ECBD9A8B38193EA5" '"7CBA65AA0C30F7B6" '
        ShipRite_AccessLicensingOnlyNumber = "ECB270290C495E56"
        ShipRite_DeveloperLicenseNumber = "2CB2702381CB5253"
        '
        ShipRite_Username = General.GetPolicyData(gShipriteDB, _ReusedField.fldUPSWeb_UserID)
        ShipRite_Password = General.GetPolicyData(gShipriteDB, _ReusedField.fldUPSWeb_UserPassword)
        ShipRite_ShipperNumber = General.GetPolicyData(gShipriteDB, _ReusedField.fldUPSWeb_UPSAccount) ' "114038" 
        '
        ShipRite_PaymentType = "01" '01 = Transportation is required; 02 = Duties and Taxes is not required

        ' IP address will be called and returned in the code:
        'Init_UPSSetupData.StoreOwner_IPAddress = _XML.Get_PublicIPAddress '"208.125.58.214"
        'Init_UPSSetupData.StoreOwner_AccessLicenseNumber = "7CBA65AA0C30F7B6" ' returned in AccessLicenseRequest.xml
        '
        LabelImageType = "Eltron Thermal" '"Eltron Thermal" 
        LabelStockSize_Height = "6"
        LabelStockSize_Width = "4"
        LabelFilePath = String.Format("{0}\UPS\InOut", gDBpath)
        '
    End Sub
End Class
Public Class StoreOwnerUPSInvoice
    ' UPS Invoice to AIA authentication:
    Public InvoiceNumber As String
    Public InvoiceDate As Date ' yyyyMMdd
    Public CurrencyCode As String
    Public InvoiceAmount As Double
    Public ControlID As String
End Class

Public Module _UPSWeb

    Public Const UPSReady As String = "UPS Ready™"
    Public objUPS_Setup As UPSSetupData
    Public IsEmail_UPS_ShipNotification As Boolean
    Public Print_ShipriteLabel As Boolean


#Region "ShipRite Credentials"
    Private Function get_UPSSecurity(ByVal upsSRSetup As Object, ByRef upss As UPS_RegWebReference.UPSSecurity) As Boolean
        Dim upssSvcAccessToken As New UPS_RegWebReference.UPSSecurityServiceAccessToken
        upssSvcAccessToken.AccessLicenseNumber = upsSRSetup.ShipRite_AccessLicenseNumber
        upss.ServiceAccessToken = upssSvcAccessToken
        Dim upssUsrNameToken As New UPS_RegWebReference.UPSSecurityUsernameToken
        upssUsrNameToken.Username = upsSRSetup.ShipRite_Username
        upssUsrNameToken.Password = upsSRSetup.ShipRite_Password
        upss.UsernameToken = upssUsrNameToken
        get_UPSSecurity = Not IsNothing(upss)
    End Function
    Private Function get_UPSSecurity(ByVal upsSRSetup As Object, ByRef upss As UPS_ShipWebReference.UPSSecurity) As Boolean
        Dim upssSvcAccessToken As New UPS_ShipWebReference.UPSSecurityServiceAccessToken
        upssSvcAccessToken.AccessLicenseNumber = upsSRSetup.ShipRite_AccessLicenseNumber
        upss.ServiceAccessToken = upssSvcAccessToken
        Dim upssUsrNameToken As New UPS_ShipWebReference.UPSSecurityUsernameToken
        upssUsrNameToken.Username = upsSRSetup.ShipRite_Username
        upssUsrNameToken.Password = upsSRSetup.ShipRite_Password
        upss.UsernameToken = upssUsrNameToken
        get_UPSSecurity = Not IsNothing(upss)
    End Function
    Private Function set_UPSAuthentication(ByVal upsSRSetup As Object, ByRef upsService As UPS_ShipWebReference.ShipService) As Boolean
        If UPS_Rest.Api.Authentication.AuthenticationService.IsEnabled Then
            upsService.Url = My.Settings.UPS_Steppingstone_Ship_Url
            Dim authToken As New UPS_Rest.Api.Authentication.OAuth.GenerateTokenResponse
            If UPS_Rest.Api.Authentication.AuthenticationService.IsGetAccessToken(authToken) Then
                upsService.AccessToken = authToken.AccessToken
                Return True
            End If
        Else
            Dim upss As New UPS_ShipWebReference.UPSSecurity
            If get_UPSSecurity(upsSRSetup, upss) Then
                upsService.UPSSecurityValue = upss
                Return True
            End If
        End If
        Return False
    End Function
    Private Function get_UPSSecurity(ByVal upsSRSetup As Object, ByRef upss As UPS_VoidWebReference.UPSSecurity) As Boolean
        Dim upssSvcAccessToken As New UPS_VoidWebReference.UPSSecurityServiceAccessToken
        upssSvcAccessToken.AccessLicenseNumber = upsSRSetup.ShipRite_AccessLicenseNumber
        upss.ServiceAccessToken = upssSvcAccessToken
        Dim upssUsrNameToken As New UPS_VoidWebReference.UPSSecurityUsernameToken
        upssUsrNameToken.Username = upsSRSetup.ShipRite_Username
        upssUsrNameToken.Password = upsSRSetup.ShipRite_Password
        upss.UsernameToken = upssUsrNameToken
        get_UPSSecurity = Not IsNothing(upss)
    End Function
    Private Function set_UPSAuthentication(ByVal upsSRSetup As Object, ByRef upsService As UPS_VoidWebReference.VoidService) As Boolean
        If UPS_Rest.Api.Authentication.AuthenticationService.IsEnabled Then
            upsService.Url = My.Settings.UPS_Steppingstone_Void_Url
            Dim authToken As New UPS_Rest.Api.Authentication.OAuth.GenerateTokenResponse
            If UPS_Rest.Api.Authentication.AuthenticationService.IsGetAccessToken(authToken) Then
                upsService.AccessToken = authToken.AccessToken
                Return True
            End If
        Else
            Dim upss As New UPS_VoidWebReference.UPSSecurity
            If get_UPSSecurity(upsSRSetup, upss) Then
                upsService.UPSSecurityValue = upss
                Return True
            End If
        End If
        Return False
    End Function
    Private Function get_UPSSecurity(ByVal upsSRSetup As Object, ByRef upss As UPS_TinTWebReference.UPSSecurity) As Boolean
        Dim upssSvcAccessToken As New UPS_TinTWebReference.UPSSecurityServiceAccessToken
        upssSvcAccessToken.AccessLicenseNumber = upsSRSetup.ShipRite_AccessLicenseNumber
        upss.ServiceAccessToken = upssSvcAccessToken
        Dim upssUsrNameToken As New UPS_TinTWebReference.UPSSecurityUsernameToken
        upssUsrNameToken.Username = upsSRSetup.ShipRite_Username 'StoreOwner_Username '
        upssUsrNameToken.Password = upsSRSetup.ShipRite_Password 'StoreOwner_Password '
        upss.UsernameToken = upssUsrNameToken
        get_UPSSecurity = Not IsNothing(upss)
    End Function
    Private Function set_UPSAuthentication(ByVal upsSRSetup As Object, ByRef upsService As UPS_TinTWebReference.TimeInTransitService) As Boolean
        If UPS_Rest.Api.Authentication.AuthenticationService.IsEnabled Then
            upsService.Url = My.Settings.UPS_Steppingstone_TinT_Url
            Dim authToken As New UPS_Rest.Api.Authentication.OAuth.GenerateTokenResponse
            If UPS_Rest.Api.Authentication.AuthenticationService.IsGetAccessToken(authToken) Then
                upsService.AccessToken = authToken.AccessToken
                Return True
            End If
        Else
            Dim upss As New UPS_TinTWebReference.UPSSecurity
            If get_UPSSecurity(upsSRSetup, upss) Then
                upsService.UPSSecurityValue = upss
                Return True
            End If
        End If
        Return False
    End Function
    Private Function get_ShipperNumber(ByVal ShipRite_ShipperNumber As String, ByRef shipment As ShipmentType) As Boolean
        Dim shipper As New ShipperType
        shipper.ShipperNumber = ShipRite_ShipperNumber
        shipment.Shipper = shipper
        get_ShipperNumber = (Not 0 = shipment.Shipper.ShipperNumber.Length)
    End Function
#End Region
#Region "Convert ShipRite/UPS"
    Private Function ups2sr_ServiceCode(ByVal serviceABBR As String, ByVal shipFromCoutryCode As String, ByVal shipToCoutryCode As String, ByVal isSaturdayDelivery As Boolean) As String
        Dim tmp As String = String.Empty
        Select Case serviceABBR
            Case "1DM"
                If Not isSaturdayDelivery Then tmp = "1DAYEAM" ' 1DM - UPS Next Day Air Early A.M.;
            Case "1DMS"
                If isSaturdayDelivery Then tmp = "1DAYEAM" ' 1DMS - UPS next Day Air Early A.M. (Saturday Delivery);
            Case "1DA"
                If Not isSaturdayDelivery Then tmp = "1DAY" ' 1DA - UPS Next Day Air;
            Case "1DAS"
                If isSaturdayDelivery Then tmp = "1DAY" ' 1DAS - UPS Next Day Air (Saturday Delivery);
            Case "1DP"
                tmp = "1DAYSVR" ' 1DP - UPS Next Day Air Saver;
            Case "2DM"
                tmp = "2DAYAM" ' 2DM - UPS 2nd Day Air A.M.;
            Case "2DA"
                tmp = "2DAY" ' 2DA - UPS 2nd Day Air;
            Case "3DS", "33"
                If "CA" = shipFromCoutryCode And "US" = shipToCoutryCode Then
                    tmp = "USA-3DAYSEL"     '  UPS 3 Day Select
                Else
                    tmp = "3DAYSEL" ' 3DS, 33 - UPS 3 Day Select;
                End If
            Case "GND", "G"
                tmp = "COM-GND" ' GND - UPS Ground.

                ' International:
            Case "01", "06"
                If "PR" = shipFromCoutryCode Or "PR" = shipToCoutryCode Then
                    tmp = "1DAY" ' 01 - UPS Next Day Air* (For Puerto Rico Only);
                ElseIf "CA" = shipFromCoutryCode And "US" = shipToCoutryCode Then
                    tmp = "USA-XPRES"       '  UPS Worldwide Express
                ElseIf "US" = shipFromCoutryCode And "CA" = shipToCoutryCode Then
                    tmp = "CAN-XPRES"
                Else
                    tmp = "WWXPRES" ' 01, 06 - UPS Worldwide Express;
                End If
            Case "02"
                If "PR" = shipFromCoutryCode Or "PR" = shipToCoutryCode Then
                    tmp = "2DAY" ' UPS 2nd Day Air* (For Puerto Rico Only);
                Else
                    If "CA" = shipToCoutryCode Then tmp = "CAN-XPRES" Else tmp = "WWXPRES" ' UPS Worldwide Express;
                End If
            Case "03", "08", "25", "68"
                If "US" = shipFromCoutryCode And "CA" = shipToCoutryCode Then
                    tmp = "CAN-STD" ' UPS Standard;
                ElseIf "CA" = shipFromCoutryCode And "CA" = shipToCoutryCode Then
                    tmp = "STD"     '  UPS Standard
                ElseIf "CA" = shipFromCoutryCode And "US" = shipToCoutryCode Then
                    tmp = "USA-STD" '  UPS Standard
                End If
            Case "05", "19"
                If "US" = shipFromCoutryCode And "CA" = shipToCoutryCode Then
                    tmp = "CAN-XPED"
                ElseIf "CA" = shipFromCoutryCode And "CA" = shipToCoutryCode Then
                    tmp = "XPED"    '  UPS Expedited
                ElseIf "CA" = shipFromCoutryCode And "US" = shipToCoutryCode Then
                    tmp = "USA-XPED"        '  UPS Worldwide Expedited
                Else
                    tmp = "WWXPED" ' 05, 19 - UPS Worldwide Expedited;
                End If
            Case "18", "20", "26", "28"
                If "US" = shipFromCoutryCode And "CA" = shipToCoutryCode Then
                    tmp = "CAN-XSVR"
                ElseIf "CA" = shipFromCoutryCode And "CA" = shipToCoutryCode Then
                    tmp = "SVR"     '  UPS Express Saver
                ElseIf "CA" = shipFromCoutryCode And "US" = shipToCoutryCode Then
                    tmp = "USA-XSVR" '  UPS Express Saver
                Else
                    tmp = "WWXSVR" ' 18, 20, 26 - UPS Saver;
                End If
            Case "21"
                If "PR" = shipFromCoutryCode Then
                    tmp = "1DAYEAM" ' 21 - UPS Next Day Air Early A.M. *( For Puerto Rico Only);
                ElseIf "CA" = shipFromCoutryCode And "US" = shipToCoutryCode Then
                    tmp = "USA-1DAYEAM"     '  UPS Express Early
                Else
                    tmp = "" ' 21 - UPS Worldwide Express Plus;
                End If

                ''ol#1.2.65(10/25)... Canada origin Service Codes were added.
                ' Canada origin Domestic
            Case "23" : tmp = "1DAYEAM" '  UPS Express Early
            Case "24" : tmp = "XPRES"   '  UPS Express

        End Select
        ups2sr_ServiceCode = tmp
    End Function
    Private Function sr2ups_ServiceCode(ByVal serviceABBR As String) As String
        'Values are: 82 = UPS Today Standard, 83 = UPS Today Dedicated Courier, 84 = UPS Today Intercity, 85 = UPS Today Express, 86 = UPS Today Express Saver. Note: Only service code 03 is used for Ground Freight Pricing shipments
        Dim tmp As String = String.Empty
        Select Case serviceABBR
            Case "1DAYEAM"
                tmp = "1DM" ' Next Day Air Early AM
            Case "1DAY"
                tmp = "1DA" ' Next Day Air
            Case "1DAYSVR"
                tmp = "1DP" ' Next Day Air Saver
            Case "2DAYAM"
                tmp = "2DM" ' 2nd Day Air A.M.
            Case "2DAY"
                tmp = "2DA" ' 2nd Day Air
            Case "3DAYSEL"
                tmp = "3DS" ' 3 Day Select
            Case "COM-GND"
                tmp = "GND" ' Ground
            Case "x"
                tmp = "EP"  ' Express Plus
            Case "CAN-XPRES", "WWXPRES"
                tmp = "ES"  ' Express
            Case "CAN-XSVR", "WWXSVR"
                tmp = "SV" ' Saver
            Case "CAN-XPED", "WWXPED"
                tmp = "EX" ' Expedited
            Case "CAN-STD"
                tmp = "ST" ' Standard

                ''ol#1.2.65(10/25)... Canada origin Service Codes were added.
                ' Canada origin Domestic
            Case "STD" : tmp = "25"     '  UPS Standard
            Case "1DAYEAM" : tmp = "23" '  UPS Express Early
            Case "XPED" : tmp = "19"    '  UPS Expedited
            Case "SVR" : tmp = "20"     '  UPS Express Saver
            Case "XPRES" : tmp = "24"   '  UPS Express
                ' Canada origin to USA
            Case "USA-1DAYEAM" : tmp = "21"     '  UPS Express Early
            Case "USA-XSVR" : tmp = "28"        '  UPS Express Saver
            Case "USA-3DAYSEL" : tmp = "33"     '  UPS 3 Day Select
            Case "USA-STD" : tmp = "03"         '  UPS Standard
            Case "USA-XPRES" : tmp = "01"       '  UPS Worldwide Express
            Case "USA-XPED" : tmp = "05"        '  UPS Worldwide Expedited

            Case Else
                tmp = serviceABBR ' error
        End Select
        sr2ups_ServiceCode = tmp
    End Function
    Private Function sr2ups_Service(ByVal serviceABBR As String) As String
        'Values are: 82 = UPS Today Standard, 83 = UPS Today Dedicated Courier, 84 = UPS Today Intercity, 85 = UPS Today Express, 86 = UPS Today Express Saver. Note: Only service code 03 is used for Ground Freight Pricing shipments
        ''AP(10/13/2017) - HI Origin: Shipping Oahu Ground returns service code error.
        ''AP(10/13/2017) - Can Origin: Shipping Domestic, USA returns service code error.
        Dim tmp As String = String.Empty
        Select Case serviceABBR
            Case "1DAY", "XPRES"
                tmp = "01" ' Next Day Air
            Case "2DAY", "XPED"
                tmp = "02" ' 2nd Day Air
            Case "COM-GND", "OAHU-GND"
                tmp = "03" ' Ground
            Case "CAN-XPRES", "WWXPRES", "USA-XPRES"
                tmp = "07" ' Express
            Case "CAN-XPED", "WWXPED", "USA-XPED"
                tmp = "08" ' Expedited
            Case "CAN-STD", "STD", "USA-STD"
                tmp = "11" ' UPS Standard
            Case "3DAYSEL", "USA-3DAYSEL"
                tmp = "12" ' 3 Day Select
            Case "1DAYSVR", "SVR"
                tmp = "13" ' Next Day Air Saver
            Case "1DAYEAM"
                tmp = "14" ' Next Day Air Early AM
            Case "x", "USA-1DAYEAM"
                tmp = "54" ' Express Plus
            Case "2DAYAM"
                tmp = "59" ' 2nd Day Air A.M.
            Case "CAN-XSVR", "WWXSVR", "USA-XSVR"
                tmp = "65" ' UPS Saver
            Case Else
                tmp = serviceABBR ' error
        End Select
        sr2ups_Service = tmp
    End Function
    Private Function sr2ups_PackagingType(ByVal packagingType As String) As String
        Dim tmp As String = "02" ' Customer Supplied Package
        '
        ' Note: Only packaging type code 02 is applicable to Ground Freight Pricing
        '
        If _Controls.Contains(packagingType, "Letter") Then
            tmp = "01" ' UPS Letter
        ElseIf _Controls.Contains(packagingType, "Pak") Then
            tmp = "04" ' PAK
        ElseIf _Controls.Contains(packagingType, "Tube") Then
            tmp = "03" ' Tube
        ElseIf _Controls.Contains(packagingType, "Box") And _Controls.Contains(packagingType, "10kg") Then
            tmp = "25" ' UPS 10KG Box
        ElseIf _Controls.Contains(packagingType, "Box") And _Controls.Contains(packagingType, "25kg") Then
            tmp = "24" ' UPS 25KG Box
        ElseIf _Controls.Contains(packagingType, "Box") And _Controls.Contains(packagingType, "Small") And _Controls.Contains(packagingType, "Exp") Then
            tmp = "2a" ' Small Express Box
        ElseIf _Controls.Contains(packagingType, "Box") And _Controls.Contains(packagingType, "Medium") And _Controls.Contains(packagingType, "Exp") Then
            tmp = "2b" ' Medium Express Box
        ElseIf _Controls.Contains(packagingType, "Box") And _Controls.Contains(packagingType, "Large") And _Controls.Contains(packagingType, "Exp") Then
            tmp = "2c" ' Large Express Box
        ElseIf _Controls.Contains(packagingType, "Box") And _Controls.Contains(packagingType, "Exp") Then
            tmp = "21" ' UPS Express Box
        ElseIf _Controls.Contains(packagingType, "Pallet") Then
            tmp = "30" ' Pallet
        End If
        sr2ups_PackagingType = tmp
    End Function
    Private Function sr2ups_LabelStockType(ByVal labelStockType As String) As String
        ''
        ''	For thermal printer labels this indicates the size of the label and 
        ''  the location of the doc tab if present.
        ''
        sr2ups_LabelStockType = String.Empty '' assume.
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
        sr2ups_LabelStockType = tmp
        ''
        'Catch ex As Exception : error_DebugPint("GetServiceType", ex.Message)
        'End Try
        ''
    End Function
    Public Function sr2ups_LabelImageType(ByVal labelImageType As String) As String
        ''
        'Label print method code that the Labels are to be generated for 
        'EPL2 formatted Labels use EPL, 
        'SPL formatted Labels use SPL, 
        'ZPL formatted Labels use ZPL 
        'image formats use GIF, 
        'Star Printer format formatted Labels use STARPL.
        Dim tmp As String = labelImageType
        Select Case labelImageType
            Case "Zebra Thermal" : tmp = "ZPL"
            Case "Eltron Thermal" : tmp = "EPL"
            Case "SPL" : tmp = "SPL"
            Case "Star Printer" : tmp = "STARPL"
            Case "GIF Image" : tmp = "GIF"
            Case "PDF Image" : tmp = "PDF"
            Case "PNG Image" : tmp = "PNG"
        End Select
        sr2ups_LabelImageType = tmp
    End Function

#End Region
#Region "Shipment"
    Private Function get_Service(ByVal obj As _baseCarrierService, ByRef shipment As ShipmentType) As Boolean
        Dim service As New ServiceType()
        service.Code = sr2ups_Service(obj.ServiceABBR)
        service.Description = obj.ServiceABBR
        shipment.Service = service
        get_Service = Not (0 = service.Code.Length)
    End Function
    Private Function get_ServiceSurcharges(ByVal obj As _baseShipment, ByRef shipment As ShipmentType) As Boolean
        Dim serviceopt As New ShipmentTypeShipmentServiceOptions
        If get_IsServiceOptionsSurcharges(obj, serviceopt) Then
            shipment.ShipmentServiceOptions = serviceopt
            ' Puerto Rico or Canada only:
            If Not obj.CarrierService.IsDomestic Then
                ''ol#1.1.79(10/22)... For Puerto Rico to Puerto Rico shipments 'InvoiceTotal' node is not allowed.
                ''  If obj.ShipToContact.CountryCode = "CA" Or obj.ShipToContact.CountryCode = "PR" Then
                If (Not obj.ShipperContact.CountryCode = obj.ShipToContact.CountryCode) And (obj.ShipToContact.CountryCode = "CA" Or obj.ShipToContact.CountryCode = "PR") Then
                    ''ol#9.166(5/7)... 'InvoiceLineTotal' is not allowed for UPS letter to Puerto Rico or Canada.
                    Dim package As _baseShipmentPackage = obj.Packages(0)
                    If Not package.IsLetter Then ''ol#9.166(5/7).
                        If obj.CommInvoice IsNot Nothing Then
                            shipment.InvoiceLineTotal = create_CurrencyMonetaryType(obj.CommInvoice.CurrencyType, obj.CommInvoice.CommoditiesTotalValue)
                        End If
                    End If
                End If
            End If
        End If
        get_ServiceSurcharges = True
    End Function
    Private Function get_IsServiceOptionsSurcharges(ByVal obj As _baseShipment, ByRef serviceopt As ShipmentTypeShipmentServiceOptions) As Boolean
        get_IsServiceOptionsSurcharges = False ' assume.
        For i As Integer = 0 To obj.CarrierService.ServiceSurcharges.Count - 1
            Dim sur As _baseServiceSurcharge = obj.CarrierService.ServiceSurcharges(i)
            Select Case sur.Name
                Case "EMAIL_NOTIFICATION"
                    If Not 0 = sur.Description.Length Then
                        If Not 0 = obj.ShipFromContact.Email.Length Or Not 0 = obj.ShipToContact.Email.Length Then
                            Dim email As New NotificationType
                            email.NotificationCode = sur.Description ' "6" - QV Ship Notification
                            Dim edetails As New EmailDetailsType
                            Dim emails(1) As String
                            If Not 0 = obj.ShipFromContact.Email.Length Then
                                emails(0) = obj.ShipFromContact.Email
                            End If
                            If Not 0 = obj.ShipToContact.Email.Length Then
                                If IsNothing(emails(0)) Then
                                    emails(0) = obj.ShipToContact.Email
                                Else
                                    emails(1) = obj.ShipToContact.Email
                                End If
                            End If
                            edetails.EMailAddress = emails
                            email.EMail = edetails
                            serviceopt.Notification = {email}
                            get_IsServiceOptionsSurcharges = True
                        End If
                    End If
                Case "SATURDAY_DELIVERY"
                    'The presence indicates Saturday delivery; the absence indicates not Saturday delivery.
                    serviceopt.SaturdayDeliveryIndicator = String.Empty
                    get_IsServiceOptionsSurcharges = True
            End Select
        Next i
        '
        ' Iternational
        If Not obj.CarrierService.IsDomestic Then
            serviceopt.InternationalForms = create_InternationalFormType(obj.CommInvoice)
            With serviceopt.InternationalForms
                .Contacts = New UPS_ShipWebReference.ContactType
                .Contacts.SoldTo = create_SoldToType(obj.ShipToContact)
            End With
            get_IsServiceOptionsSurcharges = True
        End If
    End Function
    Private Function get_IsServiceOptionsSurcharges(ByVal obj As _baseShipment, ByRef request As TimeInTransitRequest) As Boolean
        get_IsServiceOptionsSurcharges = False ' assume.
        For i As Integer = 0 To obj.CarrierService.ServiceSurcharges.Count - 1
            Dim sur As _baseServiceSurcharge = obj.CarrierService.ServiceSurcharges(i)
            Select Case sur.Name
                Case "SATURDAY_DELIVERY"
                    'The presence indicates Saturday delivery; the absence indicates not Saturday delivery.
                    request.SaturdayDeliveryInfoRequestIndicator = String.Empty
                    get_IsServiceOptionsSurcharges = True
            End Select
        Next
    End Function

    Private Function get_PackageOptionSurcharges(ByVal obj As _baseShipmentPackage, ByRef packageopt As PackageServiceOptionsType) As Boolean
        If Not IsNothing(obj.DeclaredValue) Then
            If Not 0 = obj.DeclaredValue Then
                packageopt.DeclaredValue = New PackageDeclaredValueType
                packageopt.DeclaredValue.CurrencyCode = obj.Currency_Type
                packageopt.DeclaredValue.MonetaryValue = obj.DeclaredValue.ToString
            End If
        End If
        If Not IsNothing(obj.DeliveryConfirmation) Then
            If Not 0 = obj.DeliveryConfirmation.Length Then
                Dim conf As New DeliveryConfirmationType
                conf.DCISType = obj.DeliveryConfirmation ' 1 - Delivery Confirmation 2 - Delivery Confirmation Signature Required 3 - Delivery Confirmation Adult Signature Required.
                packageopt.DeliveryConfirmation = conf
            End If
        End If
        If Not IsNothing(obj.COD) Then
            If Not 0 = obj.COD.Amount Then
                Dim cod As New PSOCODType
                Dim currency As New CurrencyMonetaryType
                currency.CurrencyCode = obj.COD.CurrencyType
                currency.MonetaryValue = obj.COD.Amount.ToString
                cod.CODAmount = currency
                cod.CODFundsCode = obj.COD.PaymentType.ToString '0 = cash; 8 = check, cashiers check or money order - no cash allowed.
                packageopt.COD = cod
            End If
        End If
        If Not IsNothing(obj.DryIce) Then
            If Not 0 = obj.DryIce.Weight Then
                Dim dryice As New DryIceType
                dryice.RegulationSet = "CFR"
                Dim dryweight As New DryIceWeightType
                dryweight.UnitOfMeasurement = New ShipUnitOfMeasurementType
                dryweight.UnitOfMeasurement.Code = obj.DryIce.WeightUnits
                'dryweight.UnitOfMeasurement.Description = obj.DryIce.WeightUnits
                dryweight.Weight = obj.DryIce.Weight.ToString
                dryice.DryIceWeight = dryweight
                packageopt.DryIce = dryice
            End If
        End If
        get_PackageOptionSurcharges = True
    End Function
    Private Function get_ShipmentWeight(ByVal obj As _baseShipment, ByRef request As TimeInTransitRequest) As Boolean
        get_ShipmentWeight = False ' assume.
        Dim weight As Long = 0
        request.TotalPackagesInShipment = obj.Packages.Count.ToString
        For i As Integer = 0 To obj.Packages.Count - 1
            Dim srpackage As _baseShipmentPackage = obj.Packages(i)
            weight = weight + srpackage.Weight_LBs
        Next
        Dim shipmentWeight As New ShipmentWeightType
        shipmentWeight.Weight = weight.ToString
        Dim unitOfMeasurement As New UPS_TinTWebReference.CodeDescriptionType
        Dim unit As _baseShipmentPackage = obj.Packages(0)
        unitOfMeasurement.Code = unit.Weight_Units
        If unitOfMeasurement.Code = "LB" Or unitOfMeasurement.Code = "KG" Then
            unitOfMeasurement.Code = unitOfMeasurement.Code & "S"
        End If
        shipmentWeight.UnitOfMeasurement = unitOfMeasurement
        request.ShipmentWeight = shipmentWeight

        get_ShipmentWeight = (Not 0 = request.ShipmentWeight.Weight.Length)
    End Function

    Private Function get_ShipmentCommInvoiceTotal(ByVal obj As _baseShipment, ByRef request As TimeInTransitRequest) As Boolean
        get_ShipmentCommInvoiceTotal = False ' assume.
        If obj.CommInvoice IsNot Nothing Then
            Dim comminv As _baseCommInvoice = obj.CommInvoice
            Dim invoiceLineTotal As New InvoiceLineTotalType()
            invoiceLineTotal.CurrencyCode = comminv.CurrencyType
            invoiceLineTotal.MonetaryValue = comminv.CommoditiesTotalValue.ToString
            request.InvoiceLineTotal = invoiceLineTotal
            request.MaximumListSize = "1"
        End If
        get_ShipmentCommInvoiceTotal = (Not 0 = request.ShipmentWeight.Weight.Length)
    End Function
    Private Function get_Packages(ByVal obj As _baseShipment, ByRef shipment As ShipmentType) As Boolean
        get_Packages = False ' assume.
        Dim pkgArray(obj.Packages.Count - 1) As PackageType
        For i As Integer = 0 To obj.Packages.Count - 1
            Dim package As New PackageType
            Dim srpackage As _baseShipmentPackage = obj.Packages(i)
            If get_Package(srpackage, package) Then
                '
                Dim packageopt As New PackageServiceOptionsType
                If get_PackageOptionSurcharges(obj.Packages(i), packageopt) Then
                    package.PackageServiceOptions = packageopt
                End If
                '
                If obj.CarrierService.IsDomestic Then
                    Dim pakid As New ReferenceNumberType
                    'pakid.BarCodeIndicator = String.Empty ' slips to the second label
                    pakid.Value = obj.Packages(i).PackageID
                    package.ReferenceNumber = {pakid}
                End If
                '
                'shipment.Package = {package, package}
                pkgArray(i) = package
                '
            End If
        Next
        shipment.Package = pkgArray
        get_Packages = (Not 0 = shipment.Package.Length)
    End Function
    Private Function get_Package(ByVal obj As _baseShipmentPackage, ByRef package As PackageType) As Boolean
        Dim packageWeight As New PackageWeightType
        packageWeight.Weight = obj.Weight_LBs.ToString
        Dim uom As New ShipUnitOfMeasurementType
        uom.Code = obj.Weight_Units
        packageWeight.UnitOfMeasurement = uom
        package.PackageWeight = packageWeight
        Dim packType As New PackagingType
        packType.Code = sr2ups_PackagingType(obj.PackagingType)
        packType.Description = obj.PackagingType
        package.Packaging = packType
        Dim packDims As New DimensionsType
        packDims.Length = obj.Dim_Length.ToString
        packDims.Width = obj.Dim_Width.ToString
        packDims.Height = obj.Dim_Height
        packDims.UnitOfMeasurement = New ShipUnitOfMeasurementType()
        packDims.UnitOfMeasurement.Code = obj.Dim_Units ' IN = Inches, CM = Centimeters, 00 = Metric Units Of Measurement, 01 = English Units of Measurement
        'packDims.UnitOfMeasurement.Description = "Inches"
        package.Dimensions = packDims
        '
        If obj.IsAdditionalHandling Then
            package.AdditionalHandlingIndicator = String.Empty ' just need an empty node
        End If
        If obj.IsLargePackage Then
            package.LargePackageIndicator = String.Empty ' just need an empty node
        End If
        '
        get_Package = True
    End Function
    Private Function get_Label(ByVal upsSRSetup As Object, ByRef shipRequest As ShipmentRequest) As Boolean
        Dim labelSpec As New LabelSpecificationType()
        Dim labelStockSize As New LabelStockSizeType()
        labelStockSize.Height = upsSRSetup.LabelStockSize_Height
        labelStockSize.Width = upsSRSetup.LabelStockSize_Width
        labelSpec.LabelStockSize = labelStockSize
        Dim labelImageFormat As New LabelImageFormatType()
        labelImageFormat.Code = sr2ups_LabelImageType(upsSRSetup.LabelImageType)
        labelSpec.LabelImageFormat = labelImageFormat
        shipRequest.LabelSpecification = labelSpec
        get_Label = Not (0 = shipRequest.LabelSpecification.LabelImageFormat.Code.Length)
    End Function

    Private Function create_ShipperAccountType(ByVal storeowner As Object) As UPS_RegWebReference.ShipperAccountType
        create_ShipperAccountType = New UPS_RegWebReference.ShipperAccountType
        With create_ShipperAccountType
            .AccountName = storeowner.CompanyName
            .AccountNumber = storeowner.AccountNumber
            .PostalCode = storeowner.Zip
            .CountryCode = storeowner.CountryCode
            .InvoiceInfo = create_InvoiceInfoType()
        End With
    End Function
    Private Function create_InvoiceInfoType() As UPS_RegWebReference.InvoiceInfoType
        create_InvoiceInfoType = New UPS_RegWebReference.InvoiceInfoType
        'UserInvoice.ShowDialog ' ' Test only before the UserAgreement form is build !!!!!
        If _UPSWeb.UPSInvoice IsNot Nothing Then
            If _UPSWeb.UPSInvoice.InvoiceNumber IsNot Nothing Then
                If Not String.IsNullOrEmpty(_UPSWeb.UPSInvoice.InvoiceNumber) Then
                    With create_InvoiceInfoType
                        .InvoiceNumber = _UPSWeb.UPSInvoice.InvoiceNumber
                        .InvoiceDate = String.Format("{0:yyyyMMdd}", _UPSWeb.UPSInvoice.InvoiceDate) ' yyyyMMdd
                        .CurrencyCode = _UPSWeb.UPSInvoice.CurrencyCode
                        .InvoiceAmount = _UPSWeb.UPSInvoice.InvoiceAmount.ToString
                        .ControlID = _UPSWeb.UPSInvoice.ControlID
                    End With
                Else
                    create_InvoiceInfoType = Nothing
                End If
            Else
                create_InvoiceInfoType = Nothing
            End If
        Else
            create_InvoiceInfoType = Nothing
        End If
    End Function
    'Private Function create_InvoiceInfoType(ByVal invoice As StoreOwnerUPSInvoice) As UPS_RegWebReference.InvoiceInfoType
    '    create_InvoiceInfoType = New UPS_RegWebReference.InvoiceInfoType
    '    UserInvoice.ShowDialog()
    '    If _UPSWeb.UPSInvoice IsNot Nothing Then
    '        If _UPSWeb.UPSInvoice.InvoiceNumber IsNot Nothing Then
    '            If Not String.IsNullOrEmpty(_UPSWeb.UPSInvoice.InvoiceNumber) Then
    '                With create_InvoiceInfoType
    '                    .InvoiceNumber = invoice.InvoiceNumber
    '                    .InvoiceDate = String.Format("{0:yyyyMMdd}", invoice.InvoiceDate) ' yyyyMMdd
    '                    .CurrencyCode = invoice.CurrencyCode
    '                    .InvoiceAmount = invoice.InvoiceAmount.ToString
    '                    .ControlID = invoice.ControlID
    '                End With
    '            Else
    '                create_InvoiceInfoType = Nothing
    '            End If
    '        Else
    '            create_InvoiceInfoType = Nothing
    '        End If
    '    Else
    '        create_InvoiceInfoType = Nothing
    '    End If
    'End Function
#End Region
#Region "Shipment Objects - International"
    Private Function create_InternationalFormType(ByVal obj As _baseCommInvoice) As InternationalFormType
        create_InternationalFormType = New InternationalFormType
        With create_InternationalFormType
            .FormType = {"01"} '01 - Invoice; 02 - SED03 - CO; 04 - NAFTA CO; 05 - Partial Invoice.
            .Product = create_ProductType(obj)
            .InvoiceNumber = obj.InvoiceNo
            .InvoiceDate = String.Format("{0:yyyyMMdd}", DateTime.Today)
            If Not IsNothing(obj.TermsOfSale) AndAlso Not 0 = obj.TermsOfSale.Length Then
                .TermsOfShipment = obj.TermsOfSale
            End If
            .ReasonForExport = obj.TypeOfContents
            If Not IsNothing(obj.Comments) AndAlso Not 0 = obj.Comments.Length Then
                .Comments = obj.Comments
            End If
            If Not IsNothing(obj.FreightCharge) AndAlso Not 0 = obj.FreightCharge Then
                .FreightCharges = New UPS_ShipWebReference.IFChargesType
                .FreightCharges.MonetaryValue = obj.FreightCharge
            End If
            If Not IsNothing(obj.InsuranceCharge) AndAlso Not 0 = obj.InsuranceCharge Then
                .InsuranceCharges = New UPS_ShipWebReference.IFChargesType
                .InsuranceCharges.MonetaryValue = obj.InsuranceCharge
            End If
            If Not IsNothing(obj.TaxesOrMiscCharge) AndAlso Not 0 = obj.TaxesOrMiscCharge Then
                .OtherCharges = New UPS_ShipWebReference.OtherChargesType
                .OtherCharges.MonetaryValue = obj.TaxesOrMiscCharge
                .OtherCharges.Description = "TaxesOrMiscCharge"
            End If
            .CurrencyCode = obj.CurrencyType
        End With
    End Function
    Private Function create_SoldToType(ByVal obj As _baseContact) As UPS_ShipWebReference.SoldToType
        create_SoldToType = New UPS_ShipWebReference.SoldToType
        With create_SoldToType
            .Name = obj.CompanyName
            .AttentionName = obj.FNameLName
            .Phone = create_PhoneType(obj.Tel)
            .EMailAddress = obj.Email
            .Address = create_AddressType(obj)
        End With
    End Function
    Private Function create_AddressType(ByVal obj As _baseContact) As UPS_ShipWebReference.AddressType
        create_AddressType = New UPS_ShipWebReference.AddressType
        With create_AddressType
            .AddressLine = {obj.Addr1, obj.Addr2}
            .City = obj.City
            .StateProvinceCode = obj.State
            .PostalCode = obj.Zip
            .CountryCode = obj.CountryCode
        End With
    End Function
    Private Function create_PhoneType(ByVal phone As String) As UPS_ShipWebReference.PhoneType
        create_PhoneType = New UPS_ShipWebReference.PhoneType
        With create_PhoneType
            .Number = phone
        End With
    End Function
    Private Function create_ProductType(ByVal obj As _baseCommInvoice) As UPS_ShipWebReference.ProductType()
        create_ProductType = Nothing
        If obj.CommoditiesList IsNot Nothing AndAlso 0 < obj.CommoditiesList.Count Then
            Dim array_ProductType(obj.CommoditiesList.Count - 1) As UPS_ShipWebReference.ProductType
            For i As Integer = 0 To obj.CommoditiesList.Count - 1
                Dim product As New UPS_ShipWebReference.ProductType
                Dim commodity As _baseCommodities = obj.CommoditiesList(i)
                With product
                    .Description = {commodity.Item_Description}
                    .Unit = create_UnitType(commodity)
                    .CommodityCode = commodity.Item_Code
                    .OriginCountryCode = commodity.Item_CountryOfOrigin
                    .ProductWeight = create_ProductWeightType(commodity.Item_Weight, commodity.Item_WeightUnits)
                End With
                array_ProductType(i) = product
            Next i
            create_ProductType = array_ProductType
        End If
    End Function
    Private Function create_ProductWeightType(ByVal weight As Double, ByVal measurecode As String) As UPS_ShipWebReference.ProductWeightType
        create_ProductWeightType = New UPS_ShipWebReference.ProductWeightType
        With create_ProductWeightType
            .Weight = weight.ToString
            .UnitOfMeasurement = create_UnitOfMeasurementType(measurecode, String.Empty)
        End With
    End Function
    Private Function create_UnitType(ByVal obj As _baseCommodities) As UPS_ShipWebReference.UnitType
        create_UnitType = New UPS_ShipWebReference.UnitType
        With create_UnitType
            .Number = obj.Item_Quantity.ToString
            .UnitOfMeasurement = create_UnitOfMeasurementType(obj.Item_UnitsOfMeasure, String.Empty)
            .Value = obj.Item_CustomsValue.ToString
        End With
    End Function
    Private Function create_UnitOfMeasurementType(ByVal code As String, ByVal description As String) As UPS_ShipWebReference.UnitOfMeasurementType
        create_UnitOfMeasurementType = New UPS_ShipWebReference.UnitOfMeasurementType
        With create_UnitOfMeasurementType
            .Code = code
            If Not 0 = description.Length Then
                .Description = description
            End If
        End With
    End Function
    Private Function create_CurrencyMonetaryType(ByVal code As String, ByVal value As Double) As UPS_ShipWebReference.CurrencyMonetaryType
        create_CurrencyMonetaryType = New UPS_ShipWebReference.CurrencyMonetaryType
        With create_CurrencyMonetaryType
            .CurrencyCode = code
            .MonetaryValue = value.ToString
        End With
    End Function
#End Region
#Region "Ship To/From"
    Private Function get_ShipperAddress(ByVal obj As _baseContact, ByRef shipment As ShipmentType) As Boolean
        Dim shipperAddress As New ShipAddressType()
        Dim addressLine As String() = {obj.Addr1, obj.Addr2}
        shipperAddress.AddressLine = addressLine
        shipperAddress.City = obj.City
        shipperAddress.PostalCode = obj.Zip
        shipperAddress.StateProvinceCode = obj.State
        shipperAddress.CountryCode = obj.CountryCode
        shipment.Shipper.Address = shipperAddress
        shipment.Shipper.Name = obj.CompanyName
        shipment.Shipper.AttentionName = obj.FNameLName
        Dim shipperPhone As New ShipPhoneType()
        shipperPhone.Number = obj.Tel
        shipment.Shipper.Phone = shipperPhone
        get_ShipperAddress = Not (0 = shipment.Shipper.Address.AddressLine(0).Length)
    End Function
    Private Function get_ShipperAddress(ByVal obj As Object) As UPS_RegWebReference.AddressType
        get_ShipperAddress = New UPS_RegWebReference.AddressType
        With get_ShipperAddress
            Dim addressLine As String()
            If String.IsNullOrEmpty(obj.Addr2) Then
                addressLine = {obj.Addr1}
            Else
                addressLine = {obj.Addr1, obj.Addr2}
            End If
            .AddressLine = addressLine
            .City = obj.City
            .PostalCode = obj.Zip
            .StateProvinceCode = obj.State
            .CountryCode = obj.CountryCode
        End With
    End Function
    Private Function get_ShipFromAddress(ByVal obj As _baseContact, ByRef shipment As ShipmentType) As Boolean
        Dim shipFrom As New ShipFromType()
        Dim shipFromAddress As New ShipAddressType()
        Dim shipFromAddressLine As String() = {obj.Addr1, obj.Addr2}
        shipFromAddress.AddressLine = shipFromAddressLine
        shipFromAddress.City = obj.City
        shipFromAddress.PostalCode = obj.Zip
        shipFromAddress.StateProvinceCode = obj.State
        shipFromAddress.CountryCode = obj.CountryCode
        shipFrom.Address = shipFromAddress
        shipFrom.AttentionName = obj.FNameLName
        shipFrom.Name = obj.CompanyName
        Dim phone As New ShipPhoneType()
        phone.Number = obj.Tel
        shipFrom.Phone = phone
        shipment.ShipFrom = shipFrom
        get_ShipFromAddress = Not (0 = shipment.ShipFrom.Address.AddressLine(0).Length)
    End Function
    Private Function get_ShipFromAddress(ByVal obj As _baseContact, ByRef request As TimeInTransitRequest) As Boolean
        Dim shipFrom As New RequestShipFromType()
        Dim shipFromAddress As New RequestShipFromAddressType()
        shipFromAddress.City = obj.City
        shipFromAddress.PostalCode = obj.Zip
        shipFromAddress.StateProvinceCode = obj.State
        shipFromAddress.CountryCode = obj.CountryCode
        shipFrom.Address = shipFromAddress
        request.ShipFrom = shipFrom
        get_ShipFromAddress = Not (0 = request.ShipFrom.Address.PostalCode.Length)
    End Function
    Private Function get_ShipToAddress(ByVal obj As _baseContact, ByRef shipment As ShipmentType) As Boolean
        Dim shipTo As New ShipToType()
        Dim shipToAddress As New ShipToAddressType()
        Dim shipToAddressLine As String() = {obj.Addr1, obj.Addr2}
        shipToAddress.AddressLine = shipToAddressLine
        shipToAddress.City = obj.City
        shipToAddress.PostalCode = obj.Zip
        shipToAddress.StateProvinceCode = obj.State
        shipToAddress.CountryCode = obj.CountryCode
        If obj.Residential Then
            shipToAddress.ResidentialAddressIndicator = String.Empty
        End If
        shipTo.Address = shipToAddress
        shipTo.AttentionName = obj.FNameLName
        If Not 0 = obj.CompanyName.Length Then
            shipTo.Name = obj.CompanyName
        Else
            shipTo.Name = obj.FNameLName
        End If
        Dim shipToPhone As New ShipPhoneType()
        shipToPhone.Number = obj.Tel
        shipTo.Phone = shipToPhone
        shipment.ShipTo = shipTo
        get_ShipToAddress = Not (0 = shipment.ShipTo.Address.AddressLine(0).Length)
    End Function
    Private Function get_ShipToAddress(ByVal obj As _baseContact, ByRef request As TimeInTransitRequest) As Boolean
        Dim shipTo As New RequestShipToType()
        Dim shipToAddress As New RequestShipToAddressType()
        shipToAddress.City = obj.City
        shipToAddress.PostalCode = obj.Zip
        shipToAddress.StateProvinceCode = obj.State
        shipToAddress.CountryCode = obj.CountryCode
        If obj.Residential Then
            shipToAddress.ResidentialAddressIndicator = String.Empty
        End If
        shipTo.Address = shipToAddress
        request.ShipTo = shipTo
        get_ShipToAddress = Not (0 = request.ShipTo.Address.PostalCode.Length)
    End Function
#End Region
#Region "Payment"
    Private Function get_PaymentInfo(ByVal upsSRSetup As Object, ByVal obj As _baseShipment, ByRef shipment As ShipmentType) As Boolean
        Dim paymentInfo As New PaymentInfoType
        Dim shpmentCharge1 As New ShipmentChargeType
        'A shipment charge type of 01 = Transportation. 
        'A shipment charge type of 02 = Duties and Taxes.
        '
        'On a UPS Internl shipment, Frt charges are billed to Sender directly (not as a third party).   
        'Duty and taxes to receiver with NO account number.
        '
        'If Duty and Tax charges are  applicable to a shipment and a payer is not specified, 
        'the default payer of Duty and Tax charges is Bill to Receiver.
        '
        shpmentCharge1.Type = "01"
        shpmentCharge1.BillShipper = create_BillShipperType(upsSRSetup.ShipRite_ShipperNumber)
        'shpmentCharge2.Type = "02"
        'shpmentCharge2.BillReceiver = create_BillReceiverType(obj.ShipToContact)
        'Dim shpmentChargeArray As ShipmentChargeType() = {shpmentCharge1, shpmentCharge2}
        Dim shpmentChargeArray As ShipmentChargeType() = {shpmentCharge1}
        paymentInfo.ShipmentCharge = shpmentChargeArray
        shipment.PaymentInformation = paymentInfo
        get_PaymentInfo = True
    End Function
    Private Function create_BillShipperType(ByVal accountNo As String) As UPS_ShipWebReference.BillShipperType
        create_BillShipperType = New UPS_ShipWebReference.BillShipperType
        With create_BillShipperType
            .AccountNumber = accountNo
        End With
    End Function
    Private Function create_BillReceiverType(ByVal obj As _baseContact) As UPS_ShipWebReference.BillReceiverType
        create_BillReceiverType = New UPS_ShipWebReference.BillReceiverType
        With create_BillReceiverType
            If Not IsNothing(obj.AccountNumber) AndAlso Not 0 = obj.AccountNumber.Length Then
                .AccountNumber = obj.AccountNumber
            End If
            .Address = create_BillReceiverAddressType(obj)
        End With
    End Function
    Private Function create_BillThirdPartyType(ByVal obj As _baseContact) As UPS_ShipWebReference.BillThirdPartyChargeType
        create_BillThirdPartyType = New UPS_ShipWebReference.BillThirdPartyChargeType
        With create_BillThirdPartyType
            If Not IsNothing(obj.AccountNumber) AndAlso Not 0 = obj.AccountNumber.Length Then
                .AccountNumber = obj.AccountNumber
            End If
            .Address = create_AccountAddressType(obj)
        End With
    End Function
    Private Function create_AccountAddressType(ByVal obj As _baseContact) As UPS_ShipWebReference.AccountAddressType
        create_AccountAddressType = New UPS_ShipWebReference.AccountAddressType
        With create_AccountAddressType
            .CountryCode = obj.CountryCode
            .PostalCode = obj.Zip
        End With
    End Function
    Private Function create_BillReceiverAddressType(ByVal obj As _baseContact) As UPS_ShipWebReference.BillReceiverAddressType
        create_BillReceiverAddressType = New UPS_ShipWebReference.BillReceiverAddressType
        With create_BillReceiverAddressType
            .PostalCode = obj.Zip
        End With
    End Function
#End Region
#Region "Ship A Package"
    Public Function Process_ShipAPackage(ByVal upsSRSetup As Object, ByVal obj As _baseShipment, ByRef vb_response As Object) As Boolean
        Process_ShipAPackage = False ' assume.
        Try
            Dim shipService As New ShipService
            '
            If set_UPSAuthentication(objUPS_Setup, shipService) Then
                '
                'Dim webReq As WebRequest = shipService.WebRequest()
                '_Files.WriteFile_ByOneString("URL: """ & webReq.RequestUri.AbsoluteUri & """" & Environment.NewLine, upsSRSetup.LabelFilePath & "\" & obj.Packages(0).PackageID & "_ship_request-headers.txt", False)
                'For Each webReqHead As String In webReq.Headers.AllKeys
                '    _Files.WriteFile_ByOneString(webReqHead & ": """ & webReq.Headers.Item(webReqHead) & """" & Environment.NewLine, upsSRSetup.LabelFilePath & "\" & obj.Packages(0).PackageID & "_ship_request-headers.txt", True)
                'Next
                '
                Dim shipRequest As New ShipmentRequest
                If create_RequestObject(upsSRSetup, obj, shipRequest) Then
                    '
                    If Not _Files.IsFileExist(String.Format("{0}\no_xml.txt", upsSRSetup.LabelFilePath), False) Then
                        Dim xdoc As New Xml.XmlDocument
                        xdoc.LoadXml(serializeShipRequest2string(shipRequest))
                        xdoc.Save(upsSRSetup.LabelFilePath & "\" & obj.Packages(0).PackageID & "_ship_request.xml") ' shipment ID
                    End If
                    '
                    Process_ShipAPackage = getResponse_ShipAPackage(upsSRSetup, shipService, shipRequest, vb_response)
                    '
                End If
                '
            End If
        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to create a Ship-A-Package request...")
        End Try
    End Function
    Private Function create_RequestObject(ByVal upsSRSetup As Object, ByVal obj As _baseShipment, ByRef shipRequest As ShipmentRequest) As Boolean
        create_RequestObject = False ' assume.
        '
        Dim request As New UPS_ShipWebReference.RequestType
        Dim requestOption() As String = {"nonvalidate"} 'Optional Processing. nonvalidate = No address validation. validate = Fail on failed address validation. Defaults to validate. Note: Full address validation is not performed.
        request.RequestOption = requestOption
        shipRequest.Request = request
        '
        Dim shipment As New ShipmentType
        shipment.Description = obj.Comments
        If Not obj.CarrierService.IsDomestic AndAlso String.IsNullOrEmpty(shipment.Description) Then
            shipment.Description = obj.CommInvoice.TypeOfContents
        End If
        ' don't know where is the Shipment ID in UPS object yet!!! I need it to void by a package, not by shipment...
        'Dim shipmentId As New ReferenceNumberType
        'shipmentId.Value = obj.TrackingNo
        'shipment.ReferenceNumber() = {shipmentId}
        If get_ShipperNumber(upsSRSetup.ShipRite_ShipperNumber, shipment) Then
            If get_PaymentInfo(upsSRSetup, obj, shipment) Then
                If get_ShipperAddress(obj.ShipperContact, shipment) Then
                    If get_ShipFromAddress(obj.ShipFromContact, shipment) Then
                        If get_ShipToAddress(obj.ShipToContact, shipment) Then
                            If get_Service(obj.CarrierService, shipment) Then
                                If get_ServiceSurcharges(obj, shipment) Then
                                    If get_Packages(obj, shipment) Then
                                        If get_Label(upsSRSetup, shipRequest) Then
                                            '
                                            shipRequest.Shipment = shipment
                                            create_RequestObject = True
                                            '
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    End If
                End If
            End If
        End If
    End Function
    Private Function getResponse_ShipAPackage(ByVal upsSRSetup As Object, ByVal shipService As ShipService, ByVal shipRequest As ShipmentRequest, ByRef vb_response As Object) As Boolean
        getResponse_ShipAPackage = False ' assume.
        Try
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12

            Dim shipResponse As UPS_ShipWebReference.ShipmentResponse = shipService.ProcessShipment(shipRequest)
            If Not _Files.IsFileExist(String.Format("{0}\no_xml.txt", upsSRSetup.LabelFilePath), False) Then
                Dim xdoc As New Xml.XmlDocument
                xdoc.LoadXml(serializeShipResponse2string(shipResponse))
                xdoc.Save(upsSRSetup.LabelFilePath & "\" & vb_response.Packages(0).PackageID & "_ship_response.xml")
            End If
            'Dim xreader As Xml.XmlTextReader = Nothing


            '_Debug.Print_(("The transaction was a " + shipResponse.Response.ResponseStatus.Description))
            '_Debug.Print_(("The 1Z number of the new shipment is " + shipResponse.ShipmentResults.ShipmentIdentificationNumber))
            Dim packageResult As PackageResultsType() = shipResponse.ShipmentResults.PackageResults
            For i As Integer = 0 To packageResult.GetLength(0) - 1
                Dim labelBase64 As String = packageResult(i).ShippingLabel.GraphicImage
                Dim labelString As String = String.Empty
                Dim labelFileExt As String = sr2ups_LabelImageType(upsSRSetup.LabelImageType)
                Dim labelFile As String = upsSRSetup.LabelFilePath & "\" & vb_response.Packages(i).PackageID & "_label." & labelFileExt
                '
                If _Files.WriteFile_ToEnd(_Convert.Base64String2Byte(labelBase64), labelFile) Then
                    If _Files.ReadFile_ToEnd(labelFile, False, labelString) Then
                        vb_response.Packages(i).LabelImage = labelString
                    End If
                End If
                vb_response.Packages(i).TrackingNo = packageResult(i).TrackingNumber
            Next
            '
            ' High Value Report:
            Dim highvalreport As UPS_ShipWebReference.ImageType() = shipResponse.ShipmentResults.ControlLogReceipt
            If Not IsNothing(highvalreport) Then
                For i As Integer = 0 To highvalreport.GetLength(0) - 1
                    Dim highvalBase64 As String = highvalreport(i).GraphicImage
                    Dim highvalFileExt As String = highvalreport(i).ImageFormat.Code
                    Dim highvalFile As String = upsSRSetup.LabelFilePath & "\" & vb_response.Packages(i).PackageID & "_HighValueReport." & highvalFileExt
                    Dim highvalString As String = String.Empty
                    '
                    If _Files.WriteFile_ToEnd(_Convert.Base64String2Byte(highvalBase64), highvalFile) Then
                        If _Files.ReadFile_ToEnd(highvalFile, False, highvalString) Then
                            vb_response.AdditionalInfo = highvalString
                        End If
                    End If
                Next
            End If
            '
            Dim packageAlert As UPS_ShipWebReference.CodeDescriptionType() = shipResponse.Response.Alert
            If Not IsNothing(packageAlert) Then
                For i As Integer = 0 To packageAlert.GetLength(0) - 1
                    '_Debug.Print_(packageAlert(i).Description)
                    '
                    ' Ignore "Invalid Date" warning alert 121943 - "Invalid Date. Changed To today's date"
                    If Not packageAlert(i).Code = "121943" Then
                        vb_response.ShipmentAlerts.Add(packageAlert(i).Description)
                    End If
                Next
            End If
            '
            vb_response.ShipmentID = shipResponse.ShipmentResults.ShipmentIdentificationNumber
            '
            Dim shipmentResult As ShipmentChargesType = shipResponse.ShipmentResults.ShipmentCharges
            '_Debug.Print_(shipmentResult.RateChart)
            Dim shipmentCharge As ShipChargeType = shipmentResult.TotalCharges
            vb_response.TotalCharges = Val(shipmentCharge.MonetaryValue)
            shipmentCharge = shipmentResult.ServiceOptionsCharges
            vb_response.ServiceOptionsCharges = Val(shipmentCharge.MonetaryValue)
            shipmentCharge = shipmentResult.TransportationCharges
            vb_response.TransportationCharges = Val(shipmentCharge.MonetaryValue)
            '
            getResponse_ShipAPackage = ("1" = shipResponse.Response.ResponseStatus.Code) ' 1 = Success.
        Catch ex As System.Web.Services.Protocols.SoapException
            _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message)
            ' write soap error xml to response file
            If Not _Files.IsFileExist(String.Format("{0}\no_xml.txt", upsSRSetup.LabelFilePath), False) Then
                Dim xdoc As New Xml.XmlDocument
                xdoc.LoadXml(ex.Detail.InnerXml)
                xdoc.Save(upsSRSetup.LabelFilePath & "\" & vb_response.Packages(0).PackageID & "_ship_response.xml")
            End If
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to get a response for Ship-A-Package request...")
        End Try
    End Function

    Private Function serializeShipRequest2string(obj As UPS_ShipWebReference.ShipmentRequest) As String
        Dim xml_serializer As New XmlSerializer(GetType(UPS_ShipWebReference.ShipmentRequest))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeShipRequest2string = string_writer.ToString()
        string_writer.Close()
    End Function
    Private Function serializeShipResponse2string(obj As UPS_ShipWebReference.ShipmentResponse) As String
        Dim xml_serializer As New XmlSerializer(GetType(UPS_ShipWebReference.ShipmentResponse))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeShipResponse2string = string_writer.ToString()
        string_writer.Close()
    End Function
    Private Function deserializeShipResponse2object(xmlsting As String) As UPS_ShipWebReference.ShipmentResponse
        Dim xml_serializer As New XmlSerializer(GetType(UPS_ShipWebReference.ShipmentResponse))
        Dim string_reader As New StringReader(xmlsting)
        deserializeShipResponse2object = DirectCast(xml_serializer.Deserialize(string_reader), UPS_ShipWebReference.ShipmentResponse)
        string_reader.Close()
    End Function
    Private Function serializeUPSSecurity2string(obj As UPS_ShipWebReference.UPSSecurity) As String
        ' for test purposes only:
        Dim xml_serializer As New XmlSerializer(GetType(UPS_ShipWebReference.UPSSecurity))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeUPSSecurity2string = string_writer.ToString()
        string_writer.Close()
    End Function

#End Region
#Region "Void A Package"
    Public Function Process_VoidAPackage(ByVal upsSRSetup As Object, ByVal packageID As String, ByVal trackingNo As String) As Boolean
        Process_VoidAPackage = False ' assume.
        Try
            Dim voidService As New VoidService
            '
            If set_UPSAuthentication(upsSRSetup, voidService) Then
                '
                'Dim webReq As WebRequest = voidService.WebRequest()
                '_Files.WriteFile_ByOneString("URL: """ & webReq.RequestUri.AbsoluteUri & """" & Environment.NewLine, upsSRSetup.LabelFilePath & "\" & packageID & "_void_request-headers.txt", False)
                'For Each webReqHead As String In webReq.Headers.AllKeys
                '    _Files.WriteFile_ByOneString(webReqHead & ": """ & webReq.Headers.Item(webReqHead) & """" & Environment.NewLine, upsSRSetup.LabelFilePath & "\" & packageID & "_void_request-headers.txt", True)
                'Next
                '
                Dim voidRequest As New VoidShipmentRequest
                If create_RequestObject(upsSRSetup, trackingNo, voidRequest) Then
                    '
                    If Not _Files.IsFileExist(String.Format("{0}\no_xml.txt", upsSRSetup.LabelFilePath), False) Then
                        Dim xdoc As New Xml.XmlDocument
                        xdoc.LoadXml(serializeVoidRequest2string(voidRequest))
                        xdoc.Save(upsSRSetup.LabelFilePath & "\" & packageID & "_void_request.xml") ' shipment ID
                    End If
                    '
                    Process_VoidAPackage = getResponse_VoidAPackage(upsSRSetup, voidService, voidRequest, packageID)
                    '
                End If
                '
            End If
        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to create a Void-A-Package request...")
        End Try
    End Function
    Private Function create_RequestObject(ByVal upsSRSetup As Object, ByVal trackingNo As String, ByRef voidRequest As VoidShipmentRequest) As Boolean
        create_RequestObject = False ' assume.
        '
        Dim request As New UPS_VoidWebReference.RequestType
        Dim requestOption() As String = {"1"}
        request.RequestOption = requestOption
        voidRequest.Request = request
        '
        Dim voidShipment As New VoidShipmentRequestVoidShipment
        voidShipment.ShipmentIdentificationNumber = trackingNo
        voidRequest.VoidShipment = voidShipment
        '
        create_RequestObject = Not (0 = voidRequest.VoidShipment.ShipmentIdentificationNumber.Length)
    End Function
    Private Function getResponse_VoidAPackage(ByVal upsSRSetup As Object, ByVal voidService As VoidService, ByVal voidRequest As VoidShipmentRequest, ByVal packageID As String) As Boolean
        getResponse_VoidAPackage = False ' assume.
        Try
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12

            Dim voidResponse As UPS_VoidWebReference.VoidShipmentResponse = voidService.ProcessVoid(voidRequest)
            If Not _Files.IsFileExist(String.Format("{0}\no_xml.txt", upsSRSetup.LabelFilePath), False) Then
                Dim xdoc As New Xml.XmlDocument
                xdoc.LoadXml(serializeVoidResponse2string(voidResponse))
                xdoc.Save(upsSRSetup.LabelFilePath & "\" & packageID & "_void_response.xml")
            End If
            '
            getResponse_VoidAPackage = ("1" = voidResponse.Response.ResponseStatus.Code) ' 1 = Success.
        Catch ex As System.Web.Services.Protocols.SoapException
            _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message)
            ' write soap error xml to response file
            If Not _Files.IsFileExist(String.Format("{0}\no_xml.txt", upsSRSetup.LabelFilePath), False) Then
                Dim xdoc As New Xml.XmlDocument
                xdoc.LoadXml(ex.Detail.InnerXml)
                xdoc.Save(upsSRSetup.LabelFilePath & "\" & packageID & "_void_response.xml")
            End If
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to get a response for Void-A-Package request...")
        End Try
    End Function

    Private Function serializeVoidRequest2string(obj As UPS_VoidWebReference.VoidShipmentRequest) As String
        Dim xml_serializer As New XmlSerializer(GetType(UPS_VoidWebReference.VoidShipmentRequest))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeVoidRequest2string = string_writer.ToString()
        string_writer.Close()
    End Function
    Private Function serializeVoidResponse2string(obj As UPS_VoidWebReference.VoidShipmentResponse) As String
        Dim xml_serializer As New XmlSerializer(GetType(UPS_VoidWebReference.VoidShipmentResponse))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeVoidResponse2string = string_writer.ToString()
        string_writer.Close()
    End Function
#End Region
#Region "Time In Transit"
    Public Function Process_TimeInTransit(ByVal obj As _baseShipment, ByRef vb_response As Object) As Boolean
        If _UPSWeb.objUPS_Setup Is Nothing Then
            _UPSWeb.objUPS_Setup = New UPSSetupData()
        End If

        Process_TimeInTransit = False ' assume.
        Dim service As New TimeInTransitService
        Try
            '
            If set_UPSAuthentication(objUPS_Setup, service) Then
                '
                'Dim webReq As WebRequest = service.WebRequest()
                '_Files.WriteFile_ByOneString("URL: """ & webReq.RequestUri.AbsoluteUri & """" & Environment.NewLine, objUPS_Setup.LabelFilePath & "\TinT_request-headers.txt", False)
                'For Each webReqHead As String In webReq.Headers.AllKeys
                '    _Files.WriteFile_ByOneString(webReqHead & ": """ & webReq.Headers.Item(webReqHead) & """" & Environment.NewLine, objUPS_Setup.LabelFilePath & "\TinT_request-headers.txt", True)
                'Next
                '
                Dim request As New TimeInTransitRequest
                If create_RequestObject(objUPS_Setup, obj, request) Then
                    '
                    If Not _Files.IsFileExist(String.Format("{0}\no_xml.txt", objUPS_Setup.LabelFilePath), False) Then
                        Dim xdoc As New Xml.XmlDocument
                        xdoc.LoadXml(serializeTinTRequest2string(request))
                        xdoc.Save(objUPS_Setup.LabelFilePath & "\TinT_request.xml") ' shipment ID
                    End If
                    '
                    Process_TimeInTransit = getResponse_TimeInTransit(objUPS_Setup, service, request, vb_response)
                    '
                End If
                '
            End If
        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to create a Time-In-Transit request...")
        Finally : service.Dispose()
        End Try
    End Function
    Private Function create_RequestObject(ByVal upsSRSetup As Object, ByVal obj As _baseShipment, ByRef request As TimeInTransitRequest) As Boolean
        create_RequestObject = False ' assume.
        '
        Dim requesttype As New UPS_TinTWebReference.RequestType
        Dim requestOption() As String = {"TNT"}
        requesttype.RequestOption = requestOption
        request.Request = requesttype
        '
        Dim pickup As New PickupType
        pickup.Date = String.Format("{0:yyyyMMdd}", obj.CarrierService.ShipDate) ' YYYYMMDD
        request.Pickup = pickup
        '
        Call get_IsServiceOptionsSurcharges(obj, request)
        '
        If get_ShipFromAddress(obj.ShipFromContact, request) Then
            If get_ShipToAddress(obj.ShipToContact, request) Then
                If get_ShipmentWeight(obj, request) Then
                    If obj.CarrierService.IsDomestic Then
                        create_RequestObject = True
                    Else
                        ' international only:
                        If obj.IsDocumentsOnly Then
                            create_RequestObject = True
                        Else
                            create_RequestObject = get_ShipmentCommInvoiceTotal(obj, request)
                            'Dim requestrefrence As New UPS_TinTWebReference.TransactionReferenceType
                            'requestrefrence.CustomerContext = obj.CommInvoice.TypeOfContents
                            'request.Request.TransactionReference = requestrefrence
                        End If
                    End If
                End If
            End If
        End If
    End Function
    Private Function getResponse_TimeInTransit(ByVal upsSRSetup As Object, ByVal service As TimeInTransitService, ByVal request As TimeInTransitRequest, ByRef vb_response As Object) As Boolean
        getResponse_TimeInTransit = False ' assume.
        Try

            'System.Net.ServicePointManager.CertificatePolicy = New TrustAllCertificatePolicy
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12
            '
            Dim response As UPS_TinTWebReference.TimeInTransitResponse = service.ProcessTimeInTransit(request)
            '
            If Not _Files.IsFileExist(String.Format("{0}\no_xml.txt", upsSRSetup.LabelFilePath), False) Then
                Dim xdoc As New Xml.XmlDocument
                xdoc.LoadXml(serializeTinTResponse2string(response))
                xdoc.Save(upsSRSetup.LabelFilePath & "\TinT_response.xml")
            End If
            '
            Dim alerts As UPS_TinTWebReference.CodeDescriptionType1() = response.Response.Alert
            If Not IsNothing(alerts) Then
                For i As Integer = 0 To alerts.GetLength(0) - 1
                    '_Debug.Print_(alerts(i).Description)
                    vb_response.TimeInTransitAlerts.Add(alerts(i).Description)
                Next
            End If
            '
            Dim isSaturdayDeliveryRequested As Boolean = (request.SaturdayDeliveryInfoRequestIndicator IsNot Nothing)
            '
            Dim tntresponse As UPS_TinTWebReference.TransitResponseType = response.Item
            Dim shipfrom As UPS_TinTWebReference.ResponseShipFromType = tntresponse.ShipFrom
            Dim shipto As UPS_TinTWebReference.ResponseShipToType = tntresponse.ShipTo
            Dim tntsummary As UPS_TinTWebReference.ServiceSummaryType() = tntresponse.ServiceSummary

            If tntsummary IsNot Nothing Then
                For i As Integer = 0 To tntsummary.GetLength(0) - 1
                    Dim arrival As UPS_TinTWebReference.EstimatedArrivalType = tntsummary(i).EstimatedArrival
                    Dim arriving As UPS_TinTWebReference.PickupType = arrival.Arrival
                    Dim upsservice As UPS_TinTWebReference.CodeDescriptionType = tntsummary(i).Service
                    For v As Integer = 0 To vb_response.AvailableServices.Count - 1
                        Dim vb_service As Object = vb_response.AvailableServices(v)
                        If vb_service.ServiceCode = ups2sr_ServiceCode(upsservice.Code, shipfrom.Address.CountryCode, shipto.Address.CountryCode, isSaturdayDeliveryRequested) Then
                            If tntsummary(i).SaturdayDelivery = "0" And isSaturdayDeliveryRequested Then
                                ' we need only Saturday Delivery ones:
                                vb_service.IsServiceAvailable = False
                                Exit For
                            End If
                            ' leave ShipRite code: vb_service.ServiceCode = upsservice.Code
                            vb_service.ServiceDesc = upsservice.Description
                            vb_service.IsOnlyArrivalTransitTime = Not (8 = arriving.Date.Length)
                            If vb_service.IsOnlyArrivalTransitTime Then
                                If arrival.BusinessDaysInTransit = "1" Then
                                    vb_service.ArrivalTransitTime = "1 day"
                                Else
                                    vb_service.ArrivalTransitTime = String.Format("{0} days", arrival.BusinessDaysInTransit)
                                End If
                            Else
                                If 6 = arriving.Time.Length Then
                                    Dim datestr As String = String.Format("{0}/{1}/{2}", _Controls.Mid(arriving.Date, 4, 2), _Controls.Right(arriving.Date, 2), _Controls.Left(arriving.Date, 4))
                                    Dim timestr As String = String.Format("{0}:{1}:{2}", _Controls.Left(arriving.Time, 2), _Controls.Mid(arriving.Time, 2, 2), _Controls.Right(arriving.Time, 2))
                                    vb_service.ArrivalDate = _Convert.String2DateTime(datestr, timestr)
                                End If
                            End If
                            vb_service.ArrivalDayOfWeek = arrival.DayOfWeek
                            vb_service.IsServiceAvailable = True
                            Exit For ' found, go to the next available returned service
                        End If
                    Next v
                Next i
            End If
            '
            getResponse_TimeInTransit = ("1" = response.Response.ResponseStatus.Code) ' 1 = Success.
        Catch ex As System.Web.Services.Protocols.SoapException
            _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message)
            ' write soap error xml to response file
            If Not _Files.IsFileExist(String.Format("{0}\no_xml.txt", upsSRSetup.LabelFilePath), False) Then
                Dim xdoc As New Xml.XmlDocument
                xdoc.LoadXml(ex.Detail.InnerXml)
                xdoc.Save(upsSRSetup.LabelFilePath & "\TinT_response.xml")
            End If
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to get a response for Time-In-Transit request...")
        End Try
    End Function

    Private Function serializeTinTRequest2string(obj As UPS_TinTWebReference.TimeInTransitRequest) As String
        Dim xml_serializer As New XmlSerializer(GetType(UPS_TinTWebReference.TimeInTransitRequest))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeTinTRequest2string = string_writer.ToString()
        string_writer.Close()
    End Function
    Private Function serializeTinTResponse2string(obj As UPS_TinTWebReference.TimeInTransitResponse) As String
        Dim xml_serializer As New XmlSerializer(GetType(UPS_TinTWebReference.TimeInTransitResponse))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeTinTResponse2string = string_writer.ToString()
        string_writer.Close()
    End Function
#End Region
#Region "License & Registration"

    Public UPSInvoice As StoreOwnerUPSInvoice
    Public IsUserAgreementAccepted As Boolean

    Public Function Get_LicenseAgreementText(ByRef userAgreementText As String) As Boolean
        Get_LicenseAgreementText = False
        Try
            userAgreementText = String.Empty ' assume.
            '
            Dim xdoc As New Xml.XmlDocument
            Dim xmlcall As String = "https://onlinetools.ups.com/ups.app/xml/License" '"https://wwwcie.ups.com/ups.app/xml/License"
            '
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12
            '
            xdoc.LoadXml(My.Resources.AccessLicenseAgreement_request)
            Dim response As String = String.Empty
            If _XML.Send_HttpWebRequest(xdoc, xmlcall, response) Then
                xdoc.LoadXml(response)
                Get_LicenseAgreementText = read_WebResponse(xdoc, "AccessLicenseText", userAgreementText)
            End If
            '
        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to get a UPS License Agreement text...")
        End Try
    End Function
    Public Function Agree_ToGetLicenseNumber(ByVal storeowner As _baseContact, ByVal userAgreementText As String, ByRef userLicenseKey As String) As Boolean
        Agree_ToGetLicenseNumber = False
        Try
            userLicenseKey = String.Empty ' assume.
            '
            Dim xdoc As New Xml.XmlDocument
            Dim xmlcall As String = "https://onlinetools.ups.com/ups.app/xml/License" '"https://wwwcie.ups.com/ups.app/xml/License"
            '
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12
            '
            xdoc.LoadXml(My.Resources.AccessLicense_request)
            Dim response As String = String.Empty
            If create_StoreOwnerLicenseRequest(storeowner, xdoc) Then
                If _XML.Node_AssignValue("/AccessLicenseRequest/AccessLicenseProfile/AccessLicenseText", userAgreementText, xdoc) Then
                    If _XML.Send_HttpWebRequest(xdoc, xmlcall, response) Then
                        xdoc.LoadXml(response)
                        Agree_ToGetLicenseNumber = read_WebResponse(xdoc, "AccessLicenseNumber", userLicenseKey)
                    End If
                End If
            End If
            '
        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to get a UPS License Number...")
        End Try
    End Function

    Private Function read_WebResponse(ByVal xdoc As Xml.XmlDocument, ByVal nodeName As String, ByRef nodeText As String) As Boolean
        ''
        read_WebResponse = False ' assume.
        nodeText = String.Empty ' assume.
        Dim responseStatus As String = String.Empty
        Dim errorDescription As String = String.Empty
        '
        If xdoc IsNot Nothing Then
            '
            If _XML.NodeReader_GetValueByNodeName(xdoc, "ResponseStatusCode", nodeText) Then
                If "1" = nodeText Then
                    ' Success:
                    If _XML.NodeReader_GetValueByNodeName(xdoc, nodeName, nodeText) Then
                        read_WebResponse = True ''ol#1.1.50(4/30)... UPS Ready <Success> node value is "1", otherwise we should stop the registration process.
                    End If
                    '
                Else
                    ' Error:
                    ''ol#1.1.50(4/30)... UPS Web <Failure> node has the error description that we should display.
                    ''If _XML.NodeReader_GetValueByNodeName(xdoc, "ResponseStatusDescription", nodeText) Then
                    ''    _MsgBox.ErrorMessage(nodeText, "Failed to read UPS Web Server response...")
                    ''End If
                    If _XML.NodeReader_GetValueByNodeName(xdoc, "ResponseStatusDescription", responseStatus) Then
                        Call _XML.NodeReader_GetValueByNodeName(xdoc, "ErrorDescription", errorDescription)
                        _MsgBox.ErrorMessage(errorDescription, "Request status: " & responseStatus, UPSReady)
                    End If
                    '
                End If
                '
            Else
                Call _XML.NodeReader_GetValueByNodeName(xdoc, "err:Description", errorDescription)
                _MsgBox.ErrorMessage(errorDescription, "", UPSReady)
            End If
            '
        End If
        ''ol#1.1.50(4/30)... UPS Ready <Success> node value is "1", otherwise we should stop the registration process.
        ''  read_WebResponse = (Not 0 = nodeText.Length)
    End Function
    Private Function create_StoreOwnerLicenseRequest(ByVal storeowner As Object, ByRef xdoc As Xml.XmlDocument) As Boolean
        With storeowner
            Call _XML.Node_AssignValue("/AccessLicenseRequest/CompanyName", .CompanyName, xdoc)
            Call _XML.Node_AssignValue("/AccessLicenseRequest/AddressArtifactFormat/StreetNumberLow", .Addr1, xdoc)
            Call _XML.Node_AssignValue("/AccessLicenseRequest/AddressArtifactFormat/StreetName", .Addr1, xdoc)
            Call _XML.Node_AssignValue("/AccessLicenseRequest/AddressArtifactFormat/PoliticalDivision2", .City, xdoc)
            Call _XML.Node_AssignValue("/AccessLicenseRequest/AddressArtifactFormat/PoliticalDivision1", .State, xdoc)
            Call _XML.Node_AssignValue("/AccessLicenseRequest/AddressArtifactFormat/CountryCode", .CountryCode, xdoc)
            Call _XML.Node_AssignValue("/AccessLicenseRequest/AddressArtifactFormat/PostcodePrimaryLow", .Zip, xdoc)
            Call _XML.Node_AssignValue("/AccessLicenseRequest/PrimaryContact/Name", .FNameLName, xdoc)
            Call _XML.Node_AssignValue("/AccessLicenseRequest/PrimaryContact/EMailAddress", .Email, xdoc)
            Call _XML.Node_AssignValue("/AccessLicenseRequest/PrimaryContact/PhoneNumber", .Tel.Replace("-", ""), xdoc)
            Call _XML.Node_AssignValue("/AccessLicenseRequest/PrimaryContact/FaxNumber", .Fax.Replace("-", ""), xdoc)
            Call _XML.Node_AssignValue("/AccessLicenseRequest/ShipperNumber", .AccountNumber, xdoc)
        End With
        create_StoreOwnerLicenseRequest = True
    End Function

    Public Function Finish_Registration(ByRef upsSRSetup As Object, ByVal storeowner As _baseContact) As Boolean
        Finish_Registration = False
        Try
            Dim regService As New UPS_RegWebReference.RegisterMgrAcctService
            Dim upss As New UPS_RegWebReference.UPSSecurity
            If get_UPSSecurity(upsSRSetup, upss) Then
                '
                regService.UPSSecurityValue = upss
                '
                Dim regRequest As New UPS_RegWebReference.RegisterRequest
                If create_RequestObject(upsSRSetup, storeowner, regRequest) Then
                    If _UPSWeb.IsUserAgreementAccepted Then
                        Finish_Registration = getResponse_Registration(regService, regRequest)
                    End If
                End If
            End If
            '
        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to create a Registration request...")
        End Try
    End Function

    Private Function create_RequestObject(ByRef upsSRSetup As Object, ByVal storeowner As Object, ByRef regRequest As UPS_RegWebReference.RegisterRequest) As Boolean
        create_RequestObject = False ' assume.
        '
        regRequest.EndUserIPAddress = _XML.Get_PublicIPAddress
        ''ol#9.175(8/29)... We are rolling back to UPS Registration with actual user's MyUPS.com user name and password and ShipRite Access Key.
        ''upsSRSetup.ShipRite_Username = String.Format("User{0}", regRequest.EndUserIPAddress.Replace(".", ""))
        ''upsSRSetup.ShipRite_Password = String.Format("Pass{0}", regRequest.EndUserIPAddress.Replace(".", ""))
        regRequest.Username = upsSRSetup.ShipRite_Username
        regRequest.Password = upsSRSetup.ShipRite_Password
        '
        regRequest.CompanyName = storeowner.CompanyName
        regRequest.CustomerName = storeowner.FNameLName
        regRequest.Address = get_ShipperAddress(storeowner)
        regRequest.PhoneNumber = storeowner.Tel.Replace("-", "")
        regRequest.EmailAddress = storeowner.Email
        '
        regRequest.NotificationCode = "01" ' = Notify by email if (username and password) is about to expire
        regRequest.ShipperAccount = create_ShipperAccountType(storeowner)
        '
        regRequest.SuggestUsernameIndicator = "Y" ' = Please Suggest If the username provided is not unique
        create_RequestObject = True
    End Function
    Private Function getResponse_Registration(ByVal regService As RegisterMgrAcctService, ByVal regRequest As RegisterRequest) As Boolean
        getResponse_Registration = False ' assume.
        Try
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12

            Dim regResponse As UPS_RegWebReference.RegisterResponse = regService.ProcessRegister(regRequest)
            'Dim xdoc As New Xml.XmlDocument
            'xdoc.LoadXml(serializeRegResponse2string(regResponse))
            'xdoc.Save("c:\test\Register_response_test.xml")
            '
            Dim alerts As UPS_RegWebReference.RegCodeDescriptionType() = regResponse.ShipperAccountStatus
            Dim usermsg As String = String.Empty
            If Not IsNothing(alerts) Then
                If alerts.GetLength(0) > 0 Then
                    For i As Integer = 0 To alerts.GetLength(0) - 1
                        '_Debug.Print_(alerts(i).Description)
                        usermsg += alerts(i).Description & _Controls.vbCr_ & _Controls.vbCr_
                    Next i
                    _MsgBox.WarningMessage(usermsg, , "Your UPS Account Status:")
                End If
            End If
            '
            getResponse_Registration = ("1" = regResponse.Response.ResponseStatus.Code) ' 1 = Success.
        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to get a response for Registration request...")
        End Try
    End Function

    Private Function serializeRegRequest2string(obj As UPS_RegWebReference.RegisterRequest) As String
        Dim xml_serializer As New XmlSerializer(GetType(UPS_RegWebReference.RegisterRequest))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeRegRequest2string = string_writer.ToString()
        string_writer.Close()
    End Function
    Private Function serializeRegResponse2string(obj As UPS_RegWebReference.RegisterResponse) As String
        Dim xml_serializer As New XmlSerializer(GetType(UPS_RegWebReference.RegisterResponse))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeRegResponse2string = string_writer.ToString()
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
                _Debug.Print_(objShipment.Packages.Count)
                If _UPSWeb.objUPS_Setup IsNot Nothing Then
                    ''
                    '' Double check ShipFrom and ShipTo contacts to avoid shipping from/to previously used contacts.
                    If Not _Contact.ShipFromContact.ContactID = Val(ExtractElementFromSegment("SID", SegmentSet)) Then
                        Call _Contact.Load_ContactFromDb(Val(ExtractElementFromSegment("SID", SegmentSet)), _Contact.ShipFromContact)
                    End If
                    If Not _Contact.ShipToContact.ContactID = Val(ExtractElementFromSegment("CID", SegmentSet)) Then
                        Call _Contact.Load_ContactFromDb(Val(ExtractElementFromSegment("CID", SegmentSet)), _Contact.ShipToContact)
                    End If
                    objShipment.ShipperContact = _Contact.ShipperContact
                    objShipment.ShipToContact = _Contact.ShipToContact
                    objShipment.ShipmentNo = ExtractElementFromSegment("ShipmentID", SegmentSet)

                    gShip.Domestic = Not ("I" = ExtractElementFromSegment("InternationalIndicator", SegmentSet))
                    objShipment.CarrierService.IsDomestic = gShip.Domestic

                    ''AP(07/17/2017) - UPS Intl: Label should show sender as shipper's addr, not store addr.
                    If gShip.Domestic Then
                        objShipment.ShipperContact = _Contact.ChangeShipFromAs_co_StoreAddress(True) ''ol#9.277(2/13).
                        objShipment.ShipFromContact = _Contact.ChangeShipFromAs_co_StoreAddress(True)
                    Else
                        objShipment.ShipperContact = _Contact.ShipFromContact
                        objShipment.ShipFromContact = _Contact.ShipFromContact
                    End If
                    objShipment.ShipperContact.AccountNumber = _UPSWeb.objUPS_Setup.ShipRite_ShipperNumber '' baseContact class has AccountNumber property now associated with a address.
                    ''
                    '' 'EMAIL - Shipping Notifications' option was added to Carrier Setup tab where you can disable/enable email notifications.
                    If Not _UPSWeb.IsEmail_UPS_ShipNotification Then
                        objShipment.ShipFromContact.Email = ""
                        objShipment.ShipToContact.Email = ""
                    End If
                    ''
                    gShip.Country = objShipment.ShipToContact.Country
                    If ExtractElementFromSegment("RES", SegmentSet, "") = "X" Then
                        objShipment.ShipToContact.Residential = True
                    Else
                        objShipment.ShipToContact.Residential = False
                    End If

                    gShip.Residential = objShipment.ShipToContact.Residential
                    objShipment.CarrierService.ServiceABBR = ExtractElementFromSegment("P1", SegmentSet)
                    gShip.ServiceABBR = objShipment.CarrierService.ServiceABBR
                    '' Express shipments to Puerto Rico should be sent by FedEx International Services.
                    If objShipment.ShipToContact.CountryCode = "PR" Then
                        If objShipment.ShipFromContact.CountryCode = "US" Then
                            '
                            gShip.Domestic = False
                            objShipment.CarrierService.IsDomestic = False
                            '
                        End If
                    End If

                    '' for hawaii customers the country code has to be 'US'
                    '' If the UPS shipper country code cannot be found, default it to 'US'.
                    If objShipment.ShipFromContact.CountryCode = "HI" Or objShipment.ShipFromContact.CountryCode = "" Then
                        objShipment.ShipFromContact.CountryCode = "US"
                    End If
                    ''
                    If objShipment.ShipToContact.Tel.Length = 0 Then
                        objShipment.ShipToContact.Tel = InputBox("Recipient is missing a phone number!" & vbCr & vbCr & vbCr & vbCr & vbCr &
                                                        "Please enter the Recipient's phone number here:", "UPS")
                        If Not 0 = Len(objShipment.ShipToContact.Tel) Then
                            '' To Do
                            ''_Contact.Update_PhoneNo objShipment.ShipToContact.ContactID, objShipment.ShipToContact.Tel
                        End If
                    End If
                    ''
                    ''
                    objShipment.Comments = ExtractElementFromSegment("Contents", SegmentSet) '"Comments go here"
                    objShipment.RateRequestType = "ACCOUNT"
                    objShipment.CarrierService.CarrierName = "UPS"

                    ' UPS Web Services time-stamp should include the time the package was shipped.
                    objShipment.CarrierService.ShipDate = DateTime.Now
                    objShipment.ShipmentNo = ExtractElementFromSegment("ShipmentID", SegmentSet)
                    '
                    '
                    ' Surcharges:
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
                        Call _FedExWeb.Prepare_InternationalData(objShipment, False)
                        '
                    End If
                    '
                    Prepare_ShipmentFromDb = True
                    '
                End If
            End If
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to read shipment info from database...")
        End Try

    End Function
    Private Function add_ServiceSurcharges(ByVal SegmentSet As String, ByRef objShipment As _baseShipment) As Boolean
        add_ServiceSurcharges = False ' assume.
        '
        '
        Dim holdID As Long = Val(ExtractElementFromSegment("ABHoldAtAirport", SegmentSet))
        If Not 0 = holdID Then
            '
            objShipment.CarrierService.ServiceSurcharges.Add(New _baseServiceSurcharge(0, "HOLD_AT_LOCATION", "HOLD_AT_LOCATION", True))
            '
            If FedExCERT.IsFedExTestAccount Then
                '
                objShipment.HoldAtLocation = _Contact.HoldAtContact
                '_Contact.HoldAtContact.AccountNumber = "OLVAD"
                _Debug.Print_("Hold at Location coutry code: " & objShipment.HoldAtLocation.CountryCode)
                '
            Else
                Call _Contact.Load_ContactFromDb(holdID, objShipment.HoldAtLocation)
                '
            End If
            '
        End If
        '
        If Not objShipment.Comments = "TinT Request" Then
            If Not 0 = Len(objShipment.ShipFromContact.Email) Or Not 0 = Len(objShipment.ShipToContact.Email) Then
                objShipment.CarrierService.ServiceSurcharges.Add(New _baseServiceSurcharge(0, "EMAIL_NOTIFICATION", "6", True))
            End If
        End If
        '
        If DateTime.Today < objShipment.CarrierService.ShipDate Then
            objShipment.CarrierService.ServiceSurcharges.Add(New _baseServiceSurcharge(0, "FUTURE_DAY_SHIPMENT", "FUTURE_DAY_SHIPMENT", True))
        End If
        '
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
        If Not 0 = Val(ExtractElementFromSegment("actSATPU", SegmentSet)) Then
            objShipment.CarrierService.ServiceSurcharges.Add(New _baseServiceSurcharge(0, "SATURDAY_PICKUP", "SATURDAY_PICKUP", True))
        End If
        '
    End Function
    Private Function add_ServiceSurcharges_Package(ByVal SegmentSet As String, ByRef objPack As _baseShipmentPackage) As Boolean
        '
        add_ServiceSurcharges_Package = False ' assume.
        '
        objPack.COD.Amount = Val(ExtractElementFromSegment("CODAMT", SegmentSet))
        If objPack.COD.Amount > 0 Then
            objPack.COD.ChargeType = String.Empty
            If CBool(ExtractElementFromSegment("CashiersCheck", SegmentSet, "False")) Then
                objPack.COD.PaymentType = "8" ' check
            Else
                objPack.COD.PaymentType = "0" ' cash
            End If
            objPack.COD.CurrencyType = _IDs.CurrencyType
            objPack.COD.AddCOD2Total = (objPack.COD.Amount < Val(ExtractElementFromSegment("CODAMTwSHIP", SegmentSet)))
            If objPack.COD.AddCOD2Total Then
                objPack.COD.Amount = Val(ExtractElementFromSegment("CODAMTwSHIP", SegmentSet))
            End If
        End If
        '
        Dim dryice As String = ExtractElementFromSegment("ABHazMat", SegmentSet).ToUpper
        If Not String.IsNullOrEmpty(dryice) Then
            objPack.DryIce.WeightUnits = _Controls.Right(dryice, 2)
            objPack.DryIce.Weight = Val(_Controls.Replace(dryice, objPack.DryIce.WeightUnits, "").Trim)
        End If
        '
        ' Re-use "Fx_SigType" field for signature type of all carriers:
        If _SignatureType.Adult_Signature = Val(ExtractElementFromSegment("Fx_SigType", SegmentSet, "-1")) Then
            objPack.DeliveryConfirmation = "3" ' Adult
        ElseIf _SignatureType.Direct_Signature = Val(ExtractElementFromSegment("Fx_SigType", SegmentSet, "-1")) Then
            objPack.DeliveryConfirmation = "2" ' Signature required
        ElseIf _SignatureType.Indirect_Signature = Val(ExtractElementFromSegment("Fx_SigType", SegmentSet, "-1")) Then
            objPack.DeliveryConfirmation = "1" ' Delivery confirmation
        End If
        '
        If "X" = ExtractElementFromSegment("AH", SegmentSet) Then
            objPack.IsAdditionalHandling = True
        End If
        If 0 < Val(ExtractElementFromSegment("AHPlus", SegmentSet)) Then
            objPack.IsLargePackage = True
        End If
        '
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
                    If objShipment.CarrierService.IsDomestic Then
                        Pack.DeclaredValue = IIf(Val(ExtractElementFromSegment("DECVAL", SegmentSet)) < 100, 100, Val(ExtractElementFromSegment("DECVAL", SegmentSet)))
                    Else
                        Pack.DeclaredValue = Val(ExtractElementFromSegment("DECVAL", SegmentSet))
                    End If
                End If

                If "X" = ExtractElementFromSegment("LTR", SegmentSet) Then
                    ' A letter
                    Pack.Weight_LBs = Val(ExtractElementFromSegment("BillableWeight", SegmentSet))
                    Pack.IsLetter = True
                Else
                    ' Not a letter
                    Pack.Weight_LBs = Val(ExtractElementFromSegment("LBS", SegmentSet))
                    Pack.Dim_Height = Val(ExtractElementFromSegment("Height", SegmentSet))
                    Pack.Dim_Length = Val(ExtractElementFromSegment("LENGTH", SegmentSet))
                    Pack.Dim_Width = Val(ExtractElementFromSegment("Width", SegmentSet))
                    Pack.IsLetter = False
                End If
                gShip.actualWeight = Pack.Weight_LBs '' gShip.ActualWeight must be set to determine if the residential shipment is Home Delivery or Not while reading from database.
                Pack.PackagingType = ExtractElementFromSegment("Packaging", SegmentSet)
                Pack.Currency_Type = _IDs.CurrencyType '' CurrencyType variable was added to manipulate between CAD and USD.
                Pack.Weight_Units = Pack.Weight_Units & "S" '' FedEx has 'LB'/'KG' as units of measure vs. UPS has 'LBS'/'KGS'.
                '
                add_ServiceSurcharges_Package(SegmentSet, Pack)
                '
                objShipment.Packages.Add(Pack)
                Prepare_PackageFromDb = True
            End If
            '
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to read Package data from Manifest database table...")
        End Try
    End Function
    Public Function Upload_Shipment(ByVal objShipment As _baseShipment, Optional NoDelete As Boolean = False, Optional showConfirmMsg As Boolean = True) As Boolean
        Upload_Shipment = True ' assume.
        ''
        Dim sql2exe As String
        Dim p%
        ''
        Dim objResponse As New baseWebResponse_Shipment
        Dim Pack As baseWebResponse_Package
        ''
        Try

            If _UPSWeb.IsUPSWebServicesEnabled Then
                If Not objShipment Is Nothing Then
                    '
                    If objShipment.Packages.Count > 0 Then
                        '
                        objResponse.ShipmentID = ""
                        objResponse.AdditionalInfo = ""
                        For p% = 0 To objShipment.Packages.Count - 1
                            ' add number if response packages
                            Pack = New baseWebResponse_Package
                            Pack.PackageID = objShipment.Packages(p%).PackageID
                            Pack.TrackingNo = ""
                            Pack.LabelImage = ""
                            objResponse.Packages.Add(Pack)
                        Next p%
                        '
                        If _UPSWeb.Process_ShipAPackage(_UPSWeb.objUPS_Setup, objShipment, objResponse) Then
                            '
                            For p% = 0 To objResponse.Packages.Count - 1
                                '
                                Dim retpack As New baseWebResponse_Package
                                retpack = objResponse.Packages(p%)
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
                                        Call sql2cmd.Qry_UPDATE("Exported", "Exported", sql2cmd.TXT_, True, False, "Manifest", "PackageID = '" & retpack.PackageID & "'")
                                        '
                                    End If
                                    Call sql2cmd.Qry_UPDATE("ReferralSource", "XML", sql2cmd.TXT_)
                                    Call sql2cmd.Qry_UPDATE("Date", String.Format("{0:MM/dd/yyyy}", objShipment.CarrierService.ShipDate), sql2cmd.DTE_)
                                    sql2exe = sql2cmd.Qry_UPDATE("TRACKING#", retpack.TrackingNo, sql2cmd.TXT_, False, True)
                                    _Debug.Print_(sql2exe)
                                    If -1 = IO_UpdateSQLProcessor(gShipriteDB, sql2exe) Then
                                        MsgBox("Failed to update Manifest with UPS tracking number...", MsgBoxStyle.Critical)
                                        Upload_Shipment = False
                                    End If
                                    '
                                End If
                                '
                            Next p%
                            '
                            If objResponse.ShipmentAlerts.Count > 0 Then
                                '
                                Dim alerts As String = String.Empty
                                For p% = 0 To objResponse.ShipmentAlerts.Count - 1
                                    alerts = alerts & vbCr & objResponse.ShipmentAlerts(p%)
                                Next p%
                                '
                                MsgBox("There were some UPS alerts in the response: " & vbCr & alerts, vbExclamation, "UPS Alerts!")
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
                    _UPSWeb.Print_ShipriteLabel = Not _Debug.IsINHOUSE
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
                            ElseIf _UPSWeb.Print_ShipriteLabel Then
                                '
                                Call _UPSWeb.Print_Label(Pack.PackageID, Pack.LabelImage)
                                If Convert.ToBoolean(General.GetPolicyData(gShipriteDB, "DuplicateLabel", "False")) Then
                                    Call _UPSWeb.Print_Label(Pack.PackageID, Pack.LabelImage)
                                End If
                                '
                            End If
                        Next p%
                    End If
                    '
                    If Not 0 = Len(objResponse.AdditionalInfo) Then
                        If _UPSWeb.Print_ShipriteLabel Then
                            ' High Value Report - print 2 copies:
                            Call _UPSWeb.Print_Label("DummyPID", objResponse.AdditionalInfo)
                            Call _UPSWeb.Print_Label("DummyPID", objResponse.AdditionalInfo)
                        End If
                    End If
                    '
                End If
            End If

        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to upload Package to FedEx...")
        End Try
    End Function


#End Region

#Region "UPS Functions"
    Public Function IsUPSWebServicesEnabled() As Boolean
        IsUPSWebServicesEnabled = False ' assume
        If Not _UPSWeb.objUPS_Setup Is Nothing Then
            If Not 0 = Len(_UPSWeb.objUPS_Setup.ShipRite_ShipperNumber) Then
                If UPS_Rest.Api.Authentication.AuthenticationService.IsEnabled Then ' UPS REST API - OAuth Token
                    If UPS_Rest.Api.Authentication.AuthenticationService.IsAccessTokenSaved() Then
                        IsUPSWebServicesEnabled = True
                    End If
                Else ' UPS Web Services - AccessKey
                    If Not 0 = Len(_UPSWeb.objUPS_Setup.ShipRite_AccessLicenseNumber) Then
                        If Not 0 = Len(_UPSWeb.objUPS_Setup.ShipRite_Password) Then
                            If Not 0 = Len(_UPSWeb.objUPS_Setup.ShipRite_Username) Then
                                IsUPSWebServicesEnabled = True
                            End If
                        End If
                    End If
                End If
            End If
        End If
    End Function
#End Region

#Region "Printing Labels"

    Public Function Print_Label(ByVal PackageID As String, Optional ByVal LabelImage As String = "") As Boolean
        Print_Label = False
        If _UPSWeb.IsUPSWebServicesEnabled Then
            '
            If Not 0 = Len(LabelImage) Then
                Print_Label = print_LabelFromImage(LabelImage)
            Else
                Print_Label = print_LabelFromFile(_UPSWeb.objUPS_Setup.LabelImageType, PackageID, _UPSWeb.objUPS_Setup.LabelFilePath)
            End If
            '
        End If
    End Function

    Private Function print_LabelFromImage(ByVal LabelImage As String) As Boolean
        print_LabelFromImage = False
        '
        Try
            '
            print_LabelFromImage = RawPrinterHelper.SendStringToPrinter(GetPolicyData(gReportsDB, "LabelPrinter"), LabelImage)
            '
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to print label...")
        End Try
    End Function

    Private Function print_LabelFromFile(ByVal LabelImageType As String, ByVal flePackageID As String, ByVal dir2DocXML As String) As Boolean
        print_LabelFromFile = False
        ''
        Dim imageFile As String
        Dim fileExt As String
        ''
        Dim PrinterName As String = GetPolicyData(gReportsDB, "LabelPrinter")
        ''
        Try
            ''
            fileExt = sr2ups_LabelImageType(LabelImageType)
            ''
            If _Controls.Contains(LabelImageType, "Thermal") Then
                '
                imageFile = dir2DocXML & "\" & flePackageID & "_label." & fileExt
                _Debug.Print_(imageFile)
                If _Files.IsFileExist(imageFile, False) Then
                    '
                    print_LabelFromFile = RawPrinterHelper.SendFileToPrinter(PrinterName, imageFile)
                    ' High Value Report:
                    imageFile = dir2DocXML & "\" & flePackageID & "_HighValueReport." & fileExt
                    If _Files.IsFileExist(imageFile, False) Then
                        ' shipper copy
                        print_LabelFromFile = RawPrinterHelper.SendFileToPrinter(PrinterName, imageFile)
                        ' UPS driver copy
                        print_LabelFromFile = RawPrinterHelper.SendFileToPrinter(PrinterName, imageFile)
                    End If
                    '
                End If
                '
            Else
                '
                ' To Do
                ''imageFile = dir2DocXML & "\" & flePackageID & "_label." & fileExt
                ''If _Files.IsFileExist(imageFile, False) Then
                ''    print_LabelFromFile = Printing.Print_FilePDF(&O0, imageFile)
                ''End If
                ''
                '''' High Value Report:
                ''imageFile = dir2DocXML & "\" & flePackageID & "_HighValueReport." & fileExt
                ''_Debug.Print_(imageFile)
                ''If _Files.IsFileExist(imageFile, False) Then
                ''    print_LabelFromFile = Printing_.Print_FilePDF(&O0, imageFile) ' shipper copy
                ''    print_LabelFromFile = Printing_.Print_FilePDF(&O0, imageFile) ' UPS driver copy
                ''End If
                '
            End If
            ''
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to print label...")
        End Try
    End Function

#End Region
End Module
