
Imports System.Xml.Serialization
Imports System.IO
Imports System.Drawing

Public Module _EndiciaWeb

    Public objEndiciaCredentials As _EndiciaSetup

    Public Const DialAZip As String = "Dial-A-ZIP"
    Public Const EndiciaLavelServer As String = "Endicia Label Server™"
    Public Const ShipRite As String = "ShipRite Software Inc."
    Public hold_XMLdirpath As String
    Public IsVerifyAllinCAPS As Boolean

    Public Sub Save_XMLfile(ByVal xdoc As Xml.XmlDocument, ByVal filename As String)
        Try
            hold_XMLdirpath = _EndiciaWeb.objEndiciaCredentials.LabelFilePath
            '
            If hold_XMLdirpath IsNot Nothing AndAlso Not 0 = hold_XMLdirpath.Length Then
                If _Files.Create_Folder(hold_XMLdirpath, False) Then
                    xdoc.Save(hold_XMLdirpath & "\" & filename)
                End If
            End If
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to save XML file...")
        End Try
    End Sub

#Region "Endicia Setup"

    Public ReadOnly Property EndiciaWeb_IsEnabled() As Boolean
        Get
            If objEndiciaCredentials Is Nothing Then
                objEndiciaCredentials = New _EndiciaSetup
            End If
            If objEndiciaCredentials IsNot Nothing Then
                Return objEndiciaCredentials.IsEnabled
            Else
                Return False
            End If
        End Get
    End Property

    Private Function get_SecuritySettings(ByRef labelRequest As Endicia_LabelService.LabelRequest) As Boolean
        Return get_SecuritySettings(objEndiciaCredentials, labelRequest)
    End Function
    Private Function get_SecuritySettings(ByVal SRSetup As _EndiciaSetup, ByRef labelRequest As Endicia_LabelService.LabelRequest) As Boolean
        labelRequest.RequesterID = SRSetup.RequesterID
        labelRequest.AccountID = SRSetup.AccountID
        labelRequest.PassPhrase = SRSetup.PassPhrase
        labelRequest.PartnerCustomerID = SRSetup.PartnerCustomerID
        labelRequest.PartnerTransactionID = SRSetup.PartnerTransactionID
        Return True
    End Function
    Private Function get_LabelSettings(ByRef labelRequest As Endicia_LabelService.LabelRequest) As Boolean
        Return get_LabelSettings(objEndiciaCredentials, labelRequest)
    End Function
    Private Function get_LabelSettings(ByVal SRSetup As _EndiciaSetup, ByRef labelRequest As Endicia_LabelService.LabelRequest) As Boolean
        labelRequest.Test = SRSetup.Test
        labelRequest.ImageFormat = sr2endicia_LabelImageType(SRSetup.LabelImageType)
        labelRequest.LabelType = SRSetup.LabelType
        labelRequest.LabelSubtype = SRSetup.LabelSubtype
        labelRequest.LabelSize = SRSetup.LabelSize
        Return True
    End Function
#End Region

#Region "International ?"
    Public Function isCountryCode_TreatedAsDomestic(ByVal CountryCode As String) As Boolean
        ' 'Endicia Label Server' integration
        isCountryCode_TreatedAsDomestic = True ' assume.
        Select Case CountryCode
            Case "US"
            Case "AS", "GU", "MP", "PR", "VI", "MH", "FM", "PW"
            Case "Wake Atoll", "Wake Island"
            Case "AA", "AE", "AP" ' Military State Codes
                ' 'United Arab Emirates' were treated as domestic because country code 'AE' was matching USPS Military Codes.
                If "United Arab Emirates" = _Contact.ShipToContact.Country Then
                    isCountryCode_TreatedAsDomestic = False
                End If
            Case Else : isCountryCode_TreatedAsDomestic = False
        End Select
    End Function
    Public Function isStateCode_MilitaryState(ByVal StateCode As String) As Boolean
        ' According to the USPS, all mail going to an APO/FPO containing merchandise should include Customs Forms 2976-A
        isStateCode_MilitaryState = True ' assume.
        Select Case StateCode
            Case "AA", "AE", "AP" ' Military State Codes
            Case Else : isStateCode_MilitaryState = False
        End Select
    End Function
    Private Function isZipCode_InCustomsFormList(ByVal ZipCode As String) As Boolean
        ' 'Endicia Label Server' integration
        isZipCode_InCustomsFormList = True ' assume.
        Select Case ZipCode
        ' AMERICAN SAMOA
            Case "96799"
            ' GUAM
            Case "96910", "96912", "96913", "96915", "96916", "96917", "96919", "96921", "96923", "96928", "96929", "96931", "96932"
            ' PALAU
            Case "96939", "96940"
            ' FEDERATED STATES OF MICRONESIA
            Case "96941", "96942", "96943", "96944"
            ' COMMONWEALTH OF THE NORTHERN MARIANA ISLANDS
            Case "96950", "96951", "96952"
            ' REPUBLIC OF THE MARSHALL ISLANDS
            Case "96960", "96970"
            Case Else : isZipCode_InCustomsFormList = False
        End Select
    End Function
    Public Function IsPackage_NeedsCustomsForm(ByVal objShipment As _baseShipment) As Boolean
        Return IsPackage_NeedsCustomsForm(objShipment, objEndiciaCredentials)
    End Function
    Public Function IsPackage_NeedsCustomsForm(ByVal objShipment As _baseShipment, ByRef upsSetup As _EndiciaSetup) As Boolean
        '
        ' 'Endicia Label Server' integration
        IsPackage_NeedsCustomsForm = False ' assume.
        upsSetup.LabelType = "Default" ' assume.
        upsSetup.LabelSubtype = "None" ' assume.
        '
        With objShipment
            ' Some US Territories zipcodes are required to have Customs form for a shipment over 1lb.
            ' could be Considered as Domestic
            If isCountryCode_TreatedAsDomestic(.ShipperContact.CountryCode) And isCountryCode_TreatedAsDomestic(.ShipToContact.CountryCode) Then
                ''ol#9.233(7/29)... According to the USPS, all mail going to an APO/FPO containing merchandise should include Customs Forms 2976-A
                If isStateCode_MilitaryState(.ShipToContact.State) Then
                    IsPackage_NeedsCustomsForm = True
                    upsSetup.LabelType = "Domestic"
                    upsSetup.LabelSubtype = "Integrated"
                    ' is 16oz (1lb) or more ?
                ElseIf Not 1 > objShipment.Packages(0).Weight_LBs Then
                    If isZipCode_InCustomsFormList(.ShipToContact.Zip) Then
                        IsPackage_NeedsCustomsForm = True
                        upsSetup.LabelType = "Domestic"
                        upsSetup.LabelSubtype = "Integrated"
                    End If
                End If
            Else
                ' clear International:
                IsPackage_NeedsCustomsForm = True
                upsSetup.LabelType = "International"
                upsSetup.LabelSubtype = "Integrated"
            End If
        End With
    End Function
    Private Function create_IntegratedFormType(ByRef obj As _baseShipment) As Boolean
        ''ol#9.218(5/30)... 'Endicia Label Server' integration
        Dim CustomsForm As New _baseServiceSurcharge
        CustomsForm.Name = "IntegratedFormType"
        CustomsForm.Description = "Form2976" ' assume. short version
        If "International" = _EndiciaWeb.objEndiciaCredentials.LabelType Then
            Dim Pack As _baseShipmentPackage
            Pack = obj.Packages(0)
            '
            If "FirstClassMailInternational" = _EndiciaWeb.sr2endicia_MailClass(obj.CarrierService.ServiceABBR) Then
                CustomsForm.Description = "Form2976" ' short version
                ' First Class Mail International will use First Class Package International Service for 'Other' package type.
            ElseIf "FirstClassPackageInternationalService" = _EndiciaWeb.sr2endicia_MailClass(obj.CarrierService.ServiceABBR) Then
                CustomsForm.Description = "Form2976" ' short version ''ol#9.257(9/26).
            ElseIf "PriorityMailInternational" = _EndiciaWeb.sr2endicia_MailClass(obj.CarrierService.ServiceABBR) Then
                ''AP(01/10/2017){DRN = 1196} - Effective 1/22/2017, USPS flat rate envelopes and small boxes will use 2976-A customs form.
                CustomsForm.Description = "Form2976A" ' long version
                objEndiciaCredentials.LabelImageType = "GIF Image"
            Else
                CustomsForm.Description = "Form2976A" ' long version
                objEndiciaCredentials.LabelImageType = "GIF Image"
            End If
            '
        Else
            ' According to the USPS, all mail going to an APO/FPO containing merchandise should include Customs Forms 2976-A
            If isStateCode_MilitaryState(obj.ShipToContact.State) Then
                If obj.IsDocumentsOnly Or obj.Packages(0).IsLetter Then
                    CustomsForm.Description = "Form2976" ' short version
                Else
                    CustomsForm.Description = "Form2976A" ' long version
                    objEndiciaCredentials.LabelImageType = "GIF Image"
                End If
            End If
        End If
        '
        obj.CarrierService.ServiceSurcharges.Add(CustomsForm)
        create_IntegratedFormType = True
    End Function


#End Region
#Region "Convert ShipRite to Endicia"
    Private Function sr2endicia_LabelImageType(ByVal v As String) As String
        ''
        'Label print method code that the Labels are to be generated for 
        Dim tmp As String = v
        Select Case v
            Case "Zebra Thermal" : tmp = "ZPLII"
            Case "Eltron Thermal" : tmp = "EPL2"
            Case "JPEG Image" : tmp = "JPEG"
            Case "GIF Image" : tmp = "GIF"
            Case "PDF Image" : tmp = "PDF"
            Case "PNG Image" : tmp = "PNG"
        End Select
        sr2endicia_LabelImageType = tmp
    End Function
    Public Function sr2endicia_MailClass(ByVal v As String) As String
        Select Case v
            Case "FirstClass" : v = "First"
            Case "USPS-PRI", "USPS-PRI_CubicRate" : v = "Priority" ''ol#1.2.43(1/19)... 'Soft Pak' was added to USPS Cubic rate logic
            Case "USPS-RG" : v = "StandardPost"
            Case "USPS-PS" : v = "ParcelSelect"
            Case "USPS-GND-ADV" : v = "GroundAdvantage"
            Case "USPS-MEDIA" : v = "MediaMail"
            Case "USPS-PRT-MTR", "USPS-PRT-MTR_Flats" : v = "LibraryMail"
            Case "USPS-EXPR" : v = "PriorityExpress"
                ' Case "x":      v = "CriticalMail" 
                ' International:
            Case "USPS-INTL-FCMI_Letter", "USPS-INTL-FCMI_Flats" : v = "FirstClassMailInternational"
                ''ol#1.1.71(9/19)... First Class Mail International will use First Class Package International Service for Parcels.
            Case "USPS-INTL-FCMI" : v = "FirstClassPackageInternationalService"
            Case "USPS-INTL-EMI" : v = "PriorityMailExpressInternational"
            Case "USPS-INTL-PMI" : v = "PriorityMailInternational"
            Case "USPS-INTL-GXG" : v = "GXG"
            Case Else : v = "" '' Do not print postage
        End Select
        Return v
    End Function
    Public Function sr2endicia_PackageType(sVal As String) As String
        If _Controls.Contains(sVal, "Flat") Then
            '' Flat Rate:
            If _Controls.Contains(sVal, "Env") Then
                ' Flat Env:
                If _Controls.Contains(sVal, "Legal") Then
                    Return "FlatRateLegalEnvelope" '' Flat Rate Legal Envelope–Priority Mail and Express Mail (Domestic only)
                ElseIf _Controls.Contains(sVal, "Padded") Then
                    Return "FlatRatePaddedEnvelope"
                ElseIf _Controls.Contains(sVal, "GiftCard") Then
                    Return "FlatRateGiftCardEnvelope"
                ElseIf _Controls.Contains(sVal, "Window") Then
                    Return "FlatRateWindowEnvelope"
                ElseIf _Controls.Contains(sVal, "Small") Then
                    Return "SmallFlatRateEnvelope"
                Else
                    Return "FlatRateEnvelope" '' Flat Rate Envelope – Priority and Express Mail
                End If
            ElseIf _Controls.Contains(sVal, "Box") Then
                ' Flat Box:
                If (_Controls.Contains(sVal, "Large") Or _Controls.Contains(sVal, "Military")) Then
                    Return "LargeFlatRateBox" '' Flat Rate Box Large – Priority Mail
                ElseIf (_Controls.Contains(sVal, "Small")) Then
                    Return "SmallFlatRateBox" '' Flat Rate Box Small – Priority Mail 
                Else
                    Return "MediumFlatRateBox" '' Flat Rate Box – Priority Mail
                End If
            End If
            ''
        ElseIf _Controls.Contains(sVal, "Regnl") Then
            '' Regional:
            If _Controls.Contains(sVal, "Box") Then
                ' Regional Box:
                If _Controls.Contains(sVal, "RegnlA") Then
                    Return "RegionalRateBoxA" '' Regional Rate Box A–Priority Mail (Domestic only)
                ElseIf _Controls.Contains(sVal, "RegnlB") Then
                    Return "RegionalRateBoxB" '' Regional Rate Box B–Priority Mail (Domestic only)
                Else
                    Return "RegionalRateBoxC" '' Regional Rate Box C–Priority Mail (Domestic only)
                End If
            End If
            ''
        ElseIf _Controls.Contains(sVal, "Letter") Then
            Return "Flat"
        End If
        ''
        '' anything else Other:
        Return "Parcel"
    End Function
#End Region
#Region "Shipment Object"
    Private Function get_Shipment(ByVal obj As _baseShipment, ByRef labelRequest As Endicia_LabelService.LabelRequest) As Boolean
        With labelRequest
            .MailClass = sr2endicia_MailClass(obj.CarrierService.ServiceABBR)
            .DateAdvance = CInt(_Date.Diff_Dates(obj.CarrierService.ShipDate, DateTime.Today))
            .Description = obj.Comments
            .ShipDate = String.Format("{0:MM/dd/yyyy}", obj.CarrierService.ShipDate)
            .ShipTime = String.Format("{0:hh:mm tt}", obj.CarrierService.ShipDate)
            ''ol#1.1.58(7/16)... 'Endicia Label Server' request requires <SortType> and <EntryFacility> tags for USPS Parcel Select service.
            If "ParcelSelect" = .MailClass Then
                ''ol#1.2.31(1/18)... Starting January 2016 for Parcel Select the <SortType> needs to be set to Nonpresorted when the <EntryFacility> is set to Other.
                ' .SortType = "Presorted"
                .SortType = "Nonpresorted"
                .EntryFacility = "Other"
            End If
            ''ol#1.1.64(8/1)... USPS 'Mail from ZIP' should always be store owner zip code, which is used for determining the zone and calculating the postage price.
            .POZipCode = obj.ShipperContact.Zip
        End With
        '
        Call get_SpecialServices(obj, labelRequest)
        '
        Dim package As _baseShipmentPackage = CType(obj.Packages(0), _baseShipmentPackage)
        Return get_Package(package, labelRequest)
    End Function
    Private Function get_Package(ByVal obj As _baseShipmentPackage, ByRef labelRequest As Endicia_LabelService.LabelRequest) As Boolean
        With labelRequest
            .WeightOz = _Convert.Pounds2Ounces(obj.Weight_LBs, 1) ''ol#1.1.62(7/23)... Endicia Label Server <WeightOz> element will only support one decimal.
            .MailpieceShape = sr2endicia_PackageType(obj.PackagingType)
            If "OTHER" = obj.PackagingType.ToUpper Then
                .MailpieceDimensions = New Endicia_LabelService.Dimensions
                .MailpieceDimensions.Length = obj.Dim_Length
                .MailpieceDimensions.Height = obj.Dim_Height
                .MailpieceDimensions.Width = obj.Dim_Width
            ElseIf _Controls.Contains(obj.PackagingType, "Soft") Then ''ol#1.2.43(1/19)... 'Soft Pak' was added to USPS Cubic rate logic
                .PackageTypeIndicator = "Softpack"
                .MailpieceDimensions = New Endicia_LabelService.Dimensions
                .MailpieceDimensions.Length = obj.Dim_Length
                If obj.Dim_Width > 1 Then
                    .MailpieceDimensions.Width = obj.Dim_Width
                Else
                    .MailpieceDimensions.Width = obj.Dim_Height
                End If
                .MailpieceDimensions.Height = 1 ' Height (or thickness) of the mailpiece. Set to “1” when PackageTypeIndicator is SoftPack.
            End If
            .Value = CSng(obj.DeclaredValue)
        End With

        Return True
    End Function
    Private Function get_SpecialServices(ByVal obj As _baseShipment, ByRef labelRequest As Endicia_LabelService.LabelRequest) As Boolean
        If Not IsNothing(obj.CarrierService.ServiceSurcharges) Then
            If obj.CarrierService.ServiceSurcharges.Count > 0 Then
                Dim spec As New Endicia_LabelService.SpecialServices
                For i As Integer = 0 To obj.CarrierService.ServiceSurcharges.Count - 1
                    Dim ss As _baseServiceSurcharge = CType(obj.CarrierService.ServiceSurcharges(i), _baseServiceSurcharge)
                    With labelRequest
                        Select Case ss.Name
                            Case "Machinable" : .Machinable = ss.Description
                            Case "IncludePostage" : .IncludePostage = ss.Description 'Print postage on the label.(Default)
                            Case "ReplyPostage" : .ReplyPostage = ss.Description 'Do not print reply postage.(Default)
                            Case "ShowReturnAddress" : .ShowReturnAddress = ss.Description 'Print sender’s address on the label. (Default)
                            Case "Stealth" : .Stealth = ss.Description
                            Case "PrintConsolidatorLabel" : .PrintConsolidatorLabel = ss.Description
                                .IncludePostage = CStr(False)

                                ' The following 3 elements are for Priority Mail Express only:
                            Case "SignatureWaiver" : .SignatureWaiver = ss.Description
                            Case "NoWeekendDelivery" : .NoWeekendDelivery = ss.Description
                            Case "SundayHolidayDelivery" : .SundayHolidayDelivery = ss.Description

                                ' Special Services ON/OFF:
                            Case "DeliveryConfirmation" : spec.DeliveryConfirmation = ss.Description
                            Case "SignatureConfirmation" : spec.SignatureConfirmation = ss.Description
                                ' PS Form 3800 required
                            Case "CertifiedMail" : spec.CertifiedMail = ss.Description
                            Case "RestrictedDelivery" : spec.RestrictedDelivery = ss.Description
                            Case "ReturnReceipt" : spec.ReturnReceipt = ss.Description
                            Case "ElectronicReturnReceipt" : spec.ElectronicReturnReceipt = ss.Description
                            Case "HoldForPickup" : spec.HoldForPickup = ss.Description
                            Case "OpenAndDistribute" : spec.OpenAndDistribute = ss.Description
                            Case "AdultSignature" : spec.AdultSignature = ss.Description
                            Case "AdultSignatureRestrictedDelivery" : spec.AdultSignatureRestrictedDelivery = ss.Description
                            Case "AMDelivery" : spec.AMDelivery = ss.Description

                                ' Special Services With Currency:
                                ' Must affix a completed COD Form 3816 to the mailpiece and take it to the retail USPS counter
                            Case "COD" : spec.COD = ss.Description : .CODAmount = ss.BaseCost
                                ' Must affix a completed Return Receipt Form 3811 to the mailpiece and take it to the retail USPS counter
                            Case "InsuredMail" : spec.InsuredMail = ss.Description : .InsuredValue = CStr(ss.BaseCost)
                            Case "RegisteredMail" : spec.RegisteredMail = ss.Description : .RegisteredMailValue = ss.BaseCost

                                ' Currency:
                            Case "CostCenter" : .CostCenter = CInt(ss.BaseCost)

                                ' International:
                            Case "IntegratedFormType" : .IntegratedFormType = ss.Description
                                If Not IsNothing(obj.CommInvoice) Then
                                    .CustomsInfo = create_CustomsInfor(obj.CommInvoice)
                                    ''ol#1.1.70(8/22)... 'Returned Goods' is not available for labels using Form 2976
                                    If "RETURNEDGOODS" = .CustomsInfo.ContentsType.ToUpper And ss.Description = "Form2976" Then
                                        .CustomsInfo.ContentsType = "Other"
                                        .CustomsInfo.ContentsExplanation = "Returned Goods"
                                    End If
                                End If
                        End Select
                    End With
                Next i
                labelRequest.Services = spec
            End If
        End If
        Return True
    End Function
    Private Function isTrue(v As String) As Boolean
        isTrue = (v.ToUpper = "ON") Or (v.ToUpper = "TRUE")
    End Function
#Region "International"
    Private Function create_CustomsInfor(ByVal obj As _baseCommInvoice) As Endicia_LabelService.CustomsInfo
        create_CustomsInfor = New Endicia_LabelService.CustomsInfo
        With create_CustomsInfor
            Dim custitems(obj.CommoditiesList.Count - 1) As Endicia_LabelService.CustomsItem
            For i As Integer = 0 To obj.CommoditiesList.Count - 1
                Dim comm As _baseCommodities = CType(obj.CommoditiesList(i), _baseCommodities)
                custitems(i) = New Endicia_LabelService.CustomsItem
                custitems(i).CountryOfOrigin = comm.Item_CountryOfOrigin
                custitems(i).Description = comm.Item_Description
                custitems(i).Quantity = comm.Item_Quantity
                custitems(i).Value = CDec(comm.Item_CustomsValue)
                custitems(i).Weight = CDec(_Convert.Pounds2Ounces(comm.Item_Weight, 1))
                custitems(i).HSTariffNumber = comm.Item_Code
            Next i
            .CustomsItems = custitems
            .ContentsType = obj.TypeOfContents
            If "OTHER" = .ContentsType.ToUpper Then
                ''ol#1.1.63(7/30)... Explanation of the customs items is required if <ContentsType> is 'Other'.
                If get_ContentsExplanation(obj) Then
                    .ContentsType = obj.TypeOfContents
                    .ContentsExplanation = obj.Comments
                End If
            End If
        End With
    End Function
    Private Function get_ContentsExplanation(ByRef obj As _baseCommInvoice) As Boolean
        ' To Do: create ContentsExplanation form
        get_ContentsExplanation = False ' assume.
        ''ContentsExplanation.ContentsType = obj.TypeOfContents
        ''ContentsExplanation.ShowDialog()
        ''If ContentsExplanation.txtExplanation.Tag IsNot Nothing Then
        ''    If CBool(ContentsExplanation.txtExplanation.Tag) Then
        ''        obj.TypeOfContents = ContentsExplanation.ContentsType
        ''        obj.Comments = ContentsExplanation.txtExplanation.Text
        ''        get_ContentsExplanation = True
        ''    End If
        ''End If
        ''ContentsExplanation.Dispose()
    End Function
#End Region
#End Region
#Region "Ship To/From"
    Private Function get_ShipFrom_Address(ByVal obj As _baseContact, ByRef labelRequest As Endicia_LabelService.LabelRequest) As Boolean
        With labelRequest
            .FromCompany = obj.CompanyName
            .FromName = obj.FNameLName
            .ReturnAddress1 = obj.Addr1
            .ReturnAddress2 = obj.Addr2
            .FromCity = obj.City
            .FromState = obj.State
            .FromPostalCode = obj.Zip
            .FromCountry = obj.Country
            .FromEMail = obj.Email
            .FromPhone = obj.Tel.Replace("-", "")
        End With
        Return True
    End Function
    Private Function get_ShipTo_Address(ByVal obj As _baseContact, ByRef labelRequest As Endicia_LabelService.LabelRequest) As Boolean
        With labelRequest
            .ToCompany = obj.CompanyName
            .ToName = obj.FNameLName
            .ToAddress1 = obj.Addr1
            .ToAddress2 = obj.Addr2
            .ToCity = obj.City
            .ToState = obj.State
            .ToPostalCode = obj.Zip
            '.ToCountry = obj.Country
            .ToCountryCode = obj.CountryCode
            .ToEMail = obj.Email
            .ToPhone = obj.Tel.Replace("-", "")
        End With
        Return True
    End Function
#End Region

#Region "Request: Account Status"
    Public Function Request_AccountStatus(ByRef response As Endicia_LabelService.AccountStatusResponse, Optional isDYMO As Boolean = False) As Boolean
        Request_AccountStatus = False ' assume.
        Try
            Dim request As New Endicia_LabelService.AccountStatusRequest
            request.RequestID = DateTime.Now 'Request ID that uniquely identifies this request.
            If objEndiciaCredentials IsNot Nothing Then
                '
                request.CertifiedIntermediary = New Endicia_LabelService.CertifiedIntermediary
                If isDYMO Then
                    request.CertifiedIntermediary.AccountID = GetPolicyData(gShipriteDB, "Endicia_AccountID2", "False")
                    request.CertifiedIntermediary.PassPhrase = GetPolicyData(gShipriteDB, "Endicia_PassPhrase2", "False")
                Else
                    request.CertifiedIntermediary.AccountID = objEndiciaCredentials.AccountID
                    request.CertifiedIntermediary.PassPhrase = objEndiciaCredentials.PassPhrase
                End If

                request.RequesterID = objEndiciaCredentials.RequesterID
                '
                Dim xdoc As New Xml.XmlDocument
                xdoc.LoadXml(serializeShipRequest2string(request))
                _EndiciaWeb.Save_XMLfile(xdoc, "AccountStatus_request.xml")
                '
                Request_AccountStatus = response_AccountStatus(request, response)
            Else
                _MsgBox.WarningMessage("Failed to load Endicia Account credentials!", "Can't Login!")
                Return False
            End If
            '
        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to create a Account Status request...")
        End Try
    End Function
    Private Function response_AccountStatus(ByVal request As Endicia_LabelService.AccountStatusRequest, ByRef response As Endicia_LabelService.AccountStatusResponse) As Boolean
        response_AccountStatus = False ' assume.
        Try
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12

            Dim ewsLabelService As Endicia_LabelService.EwsLabelService
            '
            ewsLabelService = New Endicia_LabelService.EwsLabelService
            response = ewsLabelService.GetAccountStatus(request)
            '
            Dim xdoc As New Xml.XmlDocument
            xdoc.LoadXml(serializeShipRequest2string(response))
            _EndiciaWeb.Save_XMLfile(xdoc, "AccountStatus_response.xml")
            '
            If Not IsNothing(response.ErrorMessage) Then
                ' Error
                _MsgBox.WarningMessage(response.ErrorMessage, EndiciaLavelServer)
            End If
            '
            response_AccountStatus = (0 = response.Status)
        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to get a response to Account Status request...")
        End Try
    End Function

    Private Function serializeShipRequest2string(obj As Endicia_LabelService.AccountStatusRequest) As String
        Dim xml_serializer As New XmlSerializer(GetType(Endicia_LabelService.AccountStatusRequest))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeShipRequest2string = string_writer.ToString()
        string_writer.Close()
    End Function
    Private Function serializeShipRequest2string(obj As Endicia_LabelService.AccountStatusResponse) As String
        Dim xml_serializer As New XmlSerializer(GetType(Endicia_LabelService.AccountStatusResponse))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeShipRequest2string = string_writer.ToString()
        string_writer.Close()
    End Function


#End Region

#Region "Request: Carrier Pickup"
    Public _PickupLocations As New List(Of _PickupLocation)
    Public Class _PickupLocation
        Public LocationID As Long
        Public LocationName As String
        Public LocationAbbr As String

        Public Overrides Function ToString() As String
            Return LocationName
        End Function
        Protected Overrides Sub Finalize()
            MyBase.Finalize()
        End Sub
    End Class
    Public Sub Load_PickupLocationList()
        _PickupLocations.Add(create_PickupLocation("ot", "Other", _PickupLocations.Count))
        _PickupLocations.Add(create_PickupLocation("sd", "Side Door", _PickupLocations.Count))
        _PickupLocations.Add(create_PickupLocation("kd", "Knock on Door/Ring Bell", _PickupLocations.Count))
        _PickupLocations.Add(create_PickupLocation("mr", "Mail Room", _PickupLocations.Count))
        _PickupLocations.Add(create_PickupLocation("of", "Office", _PickupLocations.Count))
        _PickupLocations.Add(create_PickupLocation("rc", "Reception", _PickupLocations.Count))
        _PickupLocations.Add(create_PickupLocation("im", "In/At Mailbox", _PickupLocations.Count))
        _PickupLocations.Add(create_PickupLocation("fd", "Front Door", _PickupLocations.Count))
        _PickupLocations.Add(create_PickupLocation("bd", "Back Door", _PickupLocations.Count))
    End Sub
    Private Function create_PickupLocation(ByVal abbr As String, ByVal name As String, ByVal id As Long) As _PickupLocation
        create_PickupLocation = New _PickupLocation
        With create_PickupLocation
            .LocationID = id
            .LocationName = name
            .LocationAbbr = abbr
        End With
    End Function
    Public Function get_PickupLocationAbbr(ByVal pickuplocation As String) As String
        get_PickupLocationAbbr = String.Empty
        For i As Integer = 0 To _PickupLocations.Count - 1
            Dim pickupitem As _PickupLocation = _PickupLocations.Item(i)
            If pickuplocation = pickupitem.LocationName Then
                Return pickupitem.LocationAbbr
            End If
        Next i
    End Function

    Private Function count_MailServices(ByVal serviceCode As String, ByVal vb_packages As Object) As Integer
        Dim count As Integer = 0
        For i As Integer = 0 To vb_packages.Packages.Count - 1
            Dim pack As Object = vb_packages.Packages(i)
            If pack IsNot Nothing AndAlso Not String.IsNullOrEmpty(pack.ServiceCode) Then
                If _Controls.Contains(pack.ServiceCode, serviceCode) Then
                    count += 1
                End If
            End If
        Next
        Return count
    End Function

    Public Function Request_SchedulePickup(ByVal SRSetup As Object, ByRef vb_response As Object) As Boolean
        Request_SchedulePickup = False ' assume.
        Try
            ''ol#1.2.32(2/11)... Endicia account info moved from ShipriteSetup_Integration.mdb to ShipRite.mdb (Setup table).
            ''  ShipriteSetup_InvegrationsDb.path2db = SRSetup.DbPath
            'ShipRiteDb.path2db = SRSetup.DbPath
            '
            Dim request As New Endicia_LabelService.PackagePickupRequest

            request.Test = SRSetup.Test
            If load_setupdata(request) Then
                '
                request.ExpressMailCount = count_MailServices("USPS-EXPR", vb_response)
                request.PriorityMailCount = count_MailServices("USPS-PRI", vb_response)
                request.InternationalCount = count_MailServices("INTL", vb_response)
                request.OtherPackagesCount = vb_response.Packages.Count - (request.ExpressMailCount + request.PriorityMailCount + request.InternationalCount)
                request.EstimatedWeightLb = vb_response.TotalWeight
                request.UseAddressOnFile = "Yes"

                request.RequestID = DateTime.Now
                '
                If Not _Files.IsFileExist(String.Format("{0}\no_xml.txt", _EndiciaWeb.hold_XMLdirpath), False) Then
                    If "Yes" = SRSetup.Test Then
                        Dim xdoc As New Xml.XmlDocument
                        xdoc.LoadXml(serializeShipRequest2string(request))
                        xdoc.Save(SRSetup.LabelFilePath & "\SchedulePickup_request.xml") ' shipment ID
                    End If
                End If
                '
                Request_SchedulePickup = response_SchedulePickup(SRSetup, request, vb_response)
                If Request_SchedulePickup Then
                    MessageBox.Show(String.Format("Scheduled pickup: {0}, {1}", vb_response.DeliveryDay, vb_response.DeliveryDate) & Environment.NewLine &
                                            String.Format("Confirmation#: {0}", vb_response.ShipmentID), EndiciaLavelServer, MessageBoxButton.OK, MessageBoxImage.Information)
                End If
                '
            End If
            '
        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to create a Schedule Pickup Form request...")
        End Try
    End Function
    Private Function response_SchedulePickup(ByVal SrSetup As Object, ByVal request As Endicia_LabelService.PackagePickupRequest, ByRef vb_response As Object) As Boolean
        response_SchedulePickup = False ' assume.
        Try
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12

            Dim response As New Endicia_LabelService.PackagePickupResponse
            Dim ewsLabelService As Endicia_LabelService.EwsLabelService
            '
            ewsLabelService = New Endicia_LabelService.EwsLabelService
            response = ewsLabelService.GetPackagePickup(request)
            '
            If Not _Files.IsFileExist(String.Format("{0}\no_xml.txt", _EndiciaWeb.hold_XMLdirpath), False) Then
                If "Yes" = SrSetup.Test Then
                    Dim xdoc As New Xml.XmlDocument
                    xdoc.LoadXml(serializeShipRequest2string(response))
                    xdoc.Save(SrSetup.LabelFilePath & "\SchedulePickup_response.xml")
                End If
            End If
            '
            If response.PackagePickup IsNot Nothing Then
                ' Needed to change or cancel pickup
                vb_response.ShipmentID = response.ConfirmationNumber
                vb_response.DeliveryDate = response.PackagePickup.Date
                vb_response.DeliveryDay = response.PackagePickup.DayOfWeek
                '
            End If
            '
            If response.ErrorMessage IsNot Nothing Then
                ' Error
                _MsgBox.ErrorMessage(response.ErrorMessage, EndiciaLavelServer)
            End If
            '
            response_SchedulePickup = (0 = response.Status)
            '
        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to get a response to Schedule Pickup request...")
        End Try
    End Function

    Public Function Request_CancelPickup(ByVal SRSetup As Object, ByRef vb_response As Object) As Boolean
        Request_CancelPickup = False ' assume.
        Try
            ''ol#1.2.32(2/11)... Endicia account info moved from ShipriteSetup_Integration.mdb to ShipRite.mdb (Setup table).
            ''  ShipriteSetup_InvegrationsDb.path2db = SRSetup.DbPath
            'ShipRiteDb.path2db = SRSetup.DbPath
            '
            Dim request As New Endicia_LabelService.PackagePickupCancelRequest
            request.Test = SRSetup.Test
            If load_setupdata(request) Then
                '
                request.RequestID = DateTime.Now
                request.ConfirmationNumber = vb_response.ShipmentID
                request.UseAddressOnFile = "Yes"
                '
                If Not _Files.IsFileExist(String.Format("{0}\no_xml.txt", _EndiciaWeb.hold_XMLdirpath), False) Then
                    If "Yes" = SRSetup.Test Then
                        Dim xdoc As New Xml.XmlDocument
                        xdoc.LoadXml(serializeShipRequest2string(request))
                        xdoc.Save(SRSetup.LabelFilePath & "\CancelPickup_request.xml") ' shipment ID
                    End If
                End If
                '                '
                Request_CancelPickup = response_CancelPickup(SRSetup, request, vb_response)
                If Request_CancelPickup Then
                    MessageBox.Show(vb_response.AdditionalInfo, EndiciaLavelServer, MessageBoxButton.OK, MessageBoxImage.Information)
                End If
                '
            End If
            '
        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to create a Schedule Pickup Form request...")
        End Try
    End Function
    Private Function response_CancelPickup(ByVal SrSetup As Object, ByVal request As Endicia_LabelService.PackagePickupCancelRequest, ByRef vb_response As Object) As Boolean
        response_CancelPickup = True ' assume.
        Try
            Dim response As New Endicia_LabelService.PackagePickupCancelResponse
            Dim ewsLabelService As Endicia_LabelService.EwsLabelService
            '
            ewsLabelService = New Endicia_LabelService.EwsLabelService
            response = ewsLabelService.GetPackagePickupCancel(request)
            '
            If Not _Files.IsFileExist(String.Format("{0}\no_xml.txt", _EndiciaWeb.hold_XMLdirpath), False) Then
                If "Yes" = SrSetup.Test Then
                    Dim xdoc As New Xml.XmlDocument
                    xdoc.LoadXml(serializeShipRequest2string(response))
                    xdoc.Save(SrSetup.LabelFilePath & "\CancelPickup_response.xml")
                End If
            End If
            '
            If response.ErrorMessage IsNot Nothing Then
                ' Error
                _MsgBox.ErrorMessage(response.ErrorMessage, EndiciaLavelServer)
            Else
                vb_response.AdditionalInfo = "Pickup was cancelled on " & DateTime.Now.ToString
            End If
            '
            response_CancelPickup = (0 = response.Status)
            '
        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to get a response to Cancel Pickup request...")
        End Try
    End Function


    Private Function serializeShipRequest2string(obj As Endicia_LabelService.PackagePickupRequest) As String
        Dim xml_serializer As New XmlSerializer(GetType(Endicia_LabelService.PackagePickupRequest))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeShipRequest2string = string_writer.ToString()
        string_writer.Close()
    End Function
    Private Function serializeShipRequest2string(obj As Endicia_LabelService.PackagePickupResponse) As String
        Dim xml_serializer As New XmlSerializer(GetType(Endicia_LabelService.PackagePickupResponse))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeShipRequest2string = string_writer.ToString()
        string_writer.Close()
    End Function
    Private Function serializeShipRequest2string(obj As Endicia_LabelService.PackagePickupCancelRequest) As String
        Dim xml_serializer As New XmlSerializer(GetType(Endicia_LabelService.PackagePickupCancelRequest))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeShipRequest2string = string_writer.ToString()
        string_writer.Close()
    End Function
    Private Function serializeShipRequest2string(obj As Endicia_LabelService.PackagePickupCancelResponse) As String
        Dim xml_serializer As New XmlSerializer(GetType(Endicia_LabelService.PackagePickupCancelResponse))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeShipRequest2string = string_writer.ToString()
        string_writer.Close()
    End Function

    Private Function load_setupdata(ByRef request As Endicia_LabelService.PackagePickupRequest) As Boolean
        If objEndiciaCredentials IsNot Nothing Then
            request.CertifiedIntermediary = New Endicia_LabelService.CertifiedIntermediary
            request.CertifiedIntermediary.AccountID = objEndiciaCredentials.AccountID
            request.CertifiedIntermediary.PassPhrase = objEndiciaCredentials.PassPhrase
            request.PackageLocation = GetPolicyData(gShipriteDB, “ABButton1”)
            request.SpecialInstructions = GetPolicyData(gShipriteDB, “ABButton2”)
            request.RequesterID = objEndiciaCredentials.RequesterID
            Return True
        Else
            _MsgBox.WarningMessage("Failed to load Endicia Account credentials!", "Can't Login!")
            Return False
        End If
    End Function
    Private Function load_setupdata(ByRef request As Endicia_LabelService.PackagePickupCancelRequest) As Boolean
        If objEndiciaCredentials IsNot Nothing Then
            request.CertifiedIntermediary = New Endicia_LabelService.CertifiedIntermediary
            request.CertifiedIntermediary.AccountID = objEndiciaCredentials.AccountID
            request.CertifiedIntermediary.PassPhrase = objEndiciaCredentials.PassPhrase
            request.RequesterID = objEndiciaCredentials.RequesterID
            Return True
        Else
            _MsgBox.WarningMessage("Failed to load Endicia Account credentials!", "Can't Login!")
            Return False
        End If
    End Function

#End Region

#Region "Request: Buying Postage"

    Public Function Request_Recredit(ByRef request As Endicia_LabelService.RecreditRequest, ByRef response As Endicia_LabelService.RecreditRequestResponse, Optional isDYMO As Boolean = False) As Boolean
        Request_Recredit = False ' assume.
        Try
            '
            request.RequestID = DateTime.Now 'Request ID that uniquely identifies this request.
            If objEndiciaCredentials IsNot Nothing Then
                '
                Dim cert As New Endicia_LabelService.CertifiedIntermediary

                If isDYMO Then
                    cert.AccountID = GetPolicyData(gShipriteDB, "Endicia_AccountID2", "False")
                    cert.PassPhrase = GetPolicyData(gShipriteDB, "Endicia_PassPhrase2", "False")
                Else
                    cert.AccountID = objEndiciaCredentials.AccountID
                    cert.PassPhrase = objEndiciaCredentials.PassPhrase
                End If

                request.CertifiedIntermediary = cert
                request.RequesterID = objEndiciaCredentials.RequesterID
                '
                Dim xdoc As New Xml.XmlDocument
                xdoc.LoadXml(serializeShipRequest2string(request))
                _EndiciaWeb.Save_XMLfile(xdoc, "BuyPostage_request.xml")
                '
                Request_Recredit = response_Recredit(request, response)
            Else
                _MsgBox.WarningMessage("Failed to load Endicia Account credentials!", "Can't Login!")
                Return False
            End If
            '
        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to create a Buy Postage request...")
        End Try
    End Function
    Private Function response_Recredit(ByVal request As Endicia_LabelService.RecreditRequest, ByRef response As Endicia_LabelService.RecreditRequestResponse) As Boolean
        response_Recredit = False ' assume.
        Try
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12

            Dim ewsLabelService As Endicia_LabelService.EwsLabelService
            '
            ewsLabelService = New Endicia_LabelService.EwsLabelService
            response = ewsLabelService.BuyPostage(request)
            '
            Dim xdoc As New Xml.XmlDocument
            xdoc.LoadXml(serializeShipRequest2string(response))
            _EndiciaWeb.Save_XMLfile(xdoc, "BuyPostage_response.xml")
            '
            If Not IsNothing(response.ErrorMessage) Then
                ' Error
                _MsgBox.WarningMessage(response.ErrorMessage, EndiciaLavelServer)
            End If
            response_Recredit = (0 = response.Status)
        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to get a response to Buy Postage request...")
        End Try
    End Function

    Private Function serializeShipRequest2string(obj As Endicia_LabelService.RecreditRequest) As String
        Dim xml_serializer As New XmlSerializer(GetType(Endicia_LabelService.RecreditRequest))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeShipRequest2string = string_writer.ToString()
        string_writer.Close()
    End Function
    Private Function serializeShipRequest2string(obj As Endicia_LabelService.RecreditRequestResponse) As String
        Dim xml_serializer As New XmlSerializer(GetType(Endicia_LabelService.RecreditRequestResponse))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeShipRequest2string = string_writer.ToString()
        string_writer.Close()
    End Function

#End Region

#Region "Request: Change Pass Phrase"
    Public passPraseRequest As New Endicia_LabelService.ChangePassPhraseRequest

    Public Function Request_ChangePassPhrase(ByRef request As Endicia_LabelService.ChangePassPhraseRequest) As Boolean
        Request_ChangePassPhrase = False ' assume.
        Try
            '
            request.RequestID = DateTime.Now 'Request ID that uniquely identifies this request.
            If objEndiciaCredentials IsNot Nothing Then
                '
                request.CertifiedIntermediary.AccountID = objEndiciaCredentials.AccountID
                request.RequesterID = objEndiciaCredentials.RequesterID
                '
                Dim xdoc As New Xml.XmlDocument
                xdoc.LoadXml(serializeShipRequest2string(request))
                _EndiciaWeb.Save_XMLfile(xdoc, "ChangePassPhrase_request.xml")
                '
                Request_ChangePassPhrase = response_ChangePassPhrase(request)
            Else
                _MsgBox.WarningMessage("Failed to load Endicia Account credentials!", "Can't Login!")
                Return False
            End If
            '
        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to create a Change Pass Phrase request...")
        End Try
    End Function
    Private Function response_ChangePassPhrase(ByVal request As Endicia_LabelService.ChangePassPhraseRequest) As Boolean
        response_ChangePassPhrase = False ' assume.
        Try
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12

            Dim response As New Endicia_LabelService.ChangePassPhraseRequestResponse
            Dim ewsLabelService As Endicia_LabelService.EwsLabelService
            '
            ewsLabelService = New Endicia_LabelService.EwsLabelService
            response = ewsLabelService.ChangePassPhrase(request)
            '
            Dim xdoc As New Xml.XmlDocument
            xdoc.LoadXml(serializeShipRequest2string(response))
            _EndiciaWeb.Save_XMLfile(xdoc, "ChangePassPhrase_response.xml")
            '
            If Not IsNothing(response.ErrorMessage) Then
                ' Error
                _MsgBox.WarningMessage(response.ErrorMessage, EndiciaLavelServer)
            End If
            response_ChangePassPhrase = (0 = response.Status)
        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to get a response to Change Pass Phrase request...")
        End Try
    End Function

    Private Function serializeShipRequest2string(obj As Endicia_LabelService.ChangePassPhraseRequest) As String
        Dim xml_serializer As New XmlSerializer(GetType(Endicia_LabelService.ChangePassPhraseRequest))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeShipRequest2string = string_writer.ToString()
        string_writer.Close()
    End Function
    Private Function serializeShipRequest2string(obj As Endicia_LabelService.ChangePassPhraseRequestResponse) As String
        Dim xml_serializer As New XmlSerializer(GetType(Endicia_LabelService.ChangePassPhraseRequestResponse))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeShipRequest2string = string_writer.ToString()
        string_writer.Close()
    End Function

#End Region

#Region "Request: Shipping Label"
    Public Function Request_ShippingLabel(ByVal obj As _baseShipment, ByRef vb_response As baseWebResponse_Shipment) As Boolean
        Return Request_ShippingLabel(objEndiciaCredentials, obj, vb_response)
    End Function
    Public Function Request_ShippingLabel(ByVal SRSetup As _EndiciaSetup, ByVal obj As _baseShipment, ByRef vb_response As baseWebResponse_Shipment) As Boolean
        Request_ShippingLabel = False ' assume.
        Try
            Dim labelRequest As Endicia_LabelService.LabelRequest
            '
            labelRequest = New Endicia_LabelService.LabelRequest
            If get_SecuritySettings(SRSetup, labelRequest) Then
                If get_LabelSettings(SRSetup, labelRequest) Then
                    If get_ShipFrom_Address(obj.ShipFromContact, labelRequest) Then
                        If get_ShipTo_Address(obj.ShipToContact, labelRequest) Then
                            If get_Shipment(obj, labelRequest) Then
                                '
                                If "GXG" = labelRequest.MailClass Then
                                    ' GXG international shipment requires GXG FedEx Tracking Number to be entered
                                    ' To Do:
                                    ''ScanTrackingNo.ShowDialog()
                                    ''If "Exit" = ScanTrackingNo.cmdExit.Text Then
                                    ''    ScanTrackingNo.Dispose()
                                    ''    Return False
                                    ''End If
                                    ''labelRequest.GXGFedexTrackingNumber = ScanTrackingNo.txtFedExTackingNo.Text
                                    ''labelRequest.GXGUSPSTrackingNumber = ScanTrackingNo.txtUSPSTrackingNo.Text
                                    ''ScanTrackingNo.Dispose()
                                End If
                                '
                                ' Exit if file with the same PackageID already exists.
                                'If Not _Files.IsFileExist(String.Format("{0}\{1}_ship_request.xml", SRSetup.LabelFilePath, obj.Packages(0).PackageID), False) Then
                                '
                                If Not _Files.IsFileExist(String.Format("{0}\no_xml.txt", SRSetup.LabelFilePath), False) Then
                                    Dim xdoc As New Xml.XmlDocument
                                    xdoc.LoadXml(serializeShipRequest2string(labelRequest))
                                    hold_XMLdirpath = SRSetup.LabelFilePath
                                    _EndiciaWeb.Save_XMLfile(xdoc, obj.Packages(0).PackageID & "_ship_request.xml")
                                End If
                                '
                                Request_ShippingLabel = getResponse_ShippingLabel(SRSetup, labelRequest, vb_response)
                                '
                                'End If
                                '
                            End If
                        End If
                    End If
                End If
            End If
            '
        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to create a Shipping Label request...")
        End Try
    End Function
    Private Function getResponse_ShippingLabel(ByVal labelRequest As Endicia_LabelService.LabelRequest, ByRef vb_response As baseWebResponse_Shipment) As Boolean
        Return getResponse_ShippingLabel(objEndiciaCredentials, labelRequest, vb_response)
    End Function
    Private Function getResponse_ShippingLabel(ByVal SRSetup As _EndiciaSetup, ByVal labelRequest As Endicia_LabelService.LabelRequest, ByRef vb_response As baseWebResponse_Shipment) As Boolean
        getResponse_ShippingLabel = False ' assume.
        Try
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12

            Dim ewsLabelService As Endicia_LabelService.EwsLabelService
            Dim labelRequestResponse As Endicia_LabelService.LabelRequestResponse
            '
            ewsLabelService = New Endicia_LabelService.EwsLabelService
            labelRequestResponse = ewsLabelService.GetPostageLabel(labelRequest)
            '
            If Not _Files.IsFileExist(String.Format("{0}\no_xml.txt", _EndiciaWeb.hold_XMLdirpath), False) Then
                Dim xdoc As New Xml.XmlDocument
                xdoc.LoadXml(serializeShipResponse2string(labelRequestResponse))
                _EndiciaWeb.Save_XMLfile(xdoc, vb_response.Packages(0).PackageID & "_ship_response.xml")
            End If
            '
            If (0 = labelRequestResponse.Status) Then
                ' 
                Dim i As Integer = 0 ' only one package for now
                Dim labelString As String = String.Empty
                Dim labelFileExt As String = sr2endicia_LabelImageType(SRSetup.LabelImageType)
                Dim labelFile As String = String.Empty
                Dim labelBase64 As String = String.Empty
                '
                ' Label:
                If Not IsNothing(labelRequestResponse.Label) Then
                    '
                    Dim imgdata() As Endicia_LabelService.ImageData = labelRequestResponse.Label.Image
                    For p As Integer = 0 To imgdata.Length - 1
                        labelBase64 = imgdata(p).Value
                        labelFile = SRSetup.LabelFilePath & "\" & vb_response.Packages(i).PackageID & "_label" & (p + 1).ToString & "." & labelFileExt
                        '
                        If _Files.WriteFile_ToEnd(_Convert.Base64String2Byte(labelBase64), labelFile) Then
                            If _Files.ReadFile_ToEnd(labelFile, False, labelString) Then
                                ' could be multiple label parts
                                vb_response.Packages(i).LabelImage = vb_response.Packages(i).LabelImage & labelString
                            End If
                        End If
                    Next p
                    '
                Else
                    labelBase64 = labelRequestResponse.Base64LabelImage
                    labelFile = SRSetup.LabelFilePath & "\" & vb_response.Packages(i).PackageID & "_label1." & labelFileExt
                    '
                    If _Files.WriteFile_ToEnd(_Convert.Base64String2Byte(labelBase64), labelFile) Then
                        If _Files.ReadFile_ToEnd(labelFile, False, labelString) Then
                            ' could be multiple label parts
                            vb_response.Packages(i).LabelImage = vb_response.Packages(i).LabelImage & labelString
                        End If
                    End If
                End If
                '
                ' CustomsForm:
                If Not IsNothing(labelRequestResponse.CustomsForm) Then
                    Dim imgdata() As Endicia_LabelService.ImageData = labelRequestResponse.CustomsForm.Image
                    For p As Integer = 0 To imgdata.Length - 1
                        labelBase64 = imgdata(p).Value
                        labelFile = SRSetup.LabelFilePath & "\" & vb_response.Packages(i).PackageID & "_customs" & (p + 1).ToString & "." & labelFileExt
                        '
                        If _Files.WriteFile_ToEnd(_Convert.Base64String2Byte(labelBase64), labelFile) Then
                            If _Files.ReadFile_ToEnd(labelFile, False, labelString) Then
                                ' could be multiple label parts
                                vb_response.Packages(i).LabelCustomsImage = vb_response.Packages(i).LabelCustomsImage & labelString
                            End If
                        End If
                    Next p
                End If
                '
                vb_response.Packages(i).TrackingNo = labelRequestResponse.TrackingNumber
                '
            Else
                '
                '_Debug.Print_("Status = " & labelRequestResponse.Status.ToString(), labelRequestResponse.ErrorMessage)
                vb_response.ShipmentAlerts.Add(labelRequestResponse.ErrorMessage)
                '
            End If
            '
            ' re-used: for Remaining postage balance afterlabel is generated, in dollars and cents
            vb_response.TotalCharges = labelRequestResponse.PostageBalance
            vb_response.ShipmentID = labelRequestResponse.TransactionID
            '
            getResponse_ShippingLabel = True
        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message) : _Debug.Print_(ex.Detail.LastChild.InnerText)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to get a response for Shipping Label request...")
        End Try
    End Function

    Private Function serializeShipRequest2string(obj As Endicia_LabelService.LabelRequest) As String
        Dim xml_serializer As New XmlSerializer(GetType(Endicia_LabelService.LabelRequest))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeShipRequest2string = string_writer.ToString()
        string_writer.Close()
    End Function
    Private Function serializeShipResponse2string(obj As Endicia_LabelService.LabelRequestResponse) As String
        Dim xml_serializer As New XmlSerializer(GetType(Endicia_LabelService.LabelRequestResponse))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeShipResponse2string = string_writer.ToString()
        string_writer.Close()
    End Function
    Private Function deserializeShipResponse2object(xmlsting As String) As Endicia_LabelService.LabelRequestResponse
        Dim xml_serializer As New XmlSerializer(GetType(Endicia_LabelService.LabelRequestResponse))
        Dim string_reader As New StringReader(xmlsting)
        deserializeShipResponse2object = DirectCast(xml_serializer.Deserialize(string_reader), Endicia_LabelService.LabelRequestResponse)
        string_reader.Close()
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
                If _EndiciaWeb.objEndiciaCredentials IsNot Nothing Then
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
                        'checkbox to enable/disable store address for USPS
                        If gShip.Use_Store_Address Then
                            objShipment.ShipperContact = _Contact.ChangeShipFromAs_co_StoreAddress(True) ''ol#9.277(2/13).
                            objShipment.ShipFromContact = _Contact.ChangeShipFromAs_co_StoreAddress(True)
                        Else
                            objShipment.ShipperContact = _Contact.ShipFromContact
                            objShipment.ShipFromContact = _Contact.ShipFromContact
                        End If

                    Else
                        'international
                        objShipment.ShipperContact = _Contact.ShipFromContact
                        objShipment.ShipFromContact = _Contact.ShipFromContact
                    End If

                    ' 'Endicia Label Server' cannot handle Company Name with '&' sign, so temp-solution is to remove it.
                    objShipment.ShipperContact.CompanyName = objShipment.ShipperContact.CompanyName.Replace("&", "")
                    objShipment.ShipFromContact.CompanyName = objShipment.ShipFromContact.CompanyName.Replace("&", "")
                    objShipment.ShipToContact.CompanyName = objShipment.ShipToContact.CompanyName.Replace("&", "")
                    '
                    objShipment.ShipperContact.AccountNumber = _EndiciaWeb.objEndiciaCredentials.AccountID '' baseContact class has AccountNumber property now associated with an address.
                    '
                    gShip.Country = objShipment.ShipToContact.Country
                    If ExtractElementFromSegment("RES", SegmentSet, "") = "X" Then
                        objShipment.ShipToContact.Residential = True
                    Else
                        objShipment.ShipToContact.Residential = False
                    End If
                    gShip.Residential = objShipment.ShipToContact.Residential
                    objShipment.CarrierService.ServiceABBR = ExtractElementFromSegment("P1", SegmentSet)
                    gShip.ServiceABBR = objShipment.CarrierService.ServiceABBR

                    If objShipment.CarrierService.ServiceABBR = "USPS-INTL-FCMI" And objShipment.Packages(0).IsLetter Then
                        objShipment.CarrierService.ServiceABBR = "USPS-INTL-FCMI_Flats"
                    End If

                    '' for hawaii customers the country code has to be 'US'
                    If objShipment.ShipFromContact.CountryCode = "HI" Or objShipment.ShipFromContact.CountryCode = "" Then
                        objShipment.ShipFromContact.CountryCode = "US"
                    End If
                    ''
                    If objShipment.ShipToContact.Tel.Length = 0 Then
                        objShipment.ShipToContact.Tel = InputBox("Recipient is missing a phone number!" & vbCr & vbCr & vbCr & vbCr & vbCr &
                                                        "Please enter the Recipient's phone number here:", "USPS")
                        If Not 0 = Len(objShipment.ShipToContact.Tel) Then
                            '' To Do
                            ''_Contact.Update_PhoneNo objShipment.ShipToContact.ContactID, objShipment.ShipToContact.Tel
                        End If
                    End If
                    ''
                    ''
                    objShipment.Comments = ExtractElementFromSegment("Contents", SegmentSet, "Package") '"Comments go here"
                    objShipment.RateRequestType = "ACCOUNT"
                    objShipment.CarrierService.CarrierName = "USPS"

                    objShipment.CarrierService.ShipDate = DateTime.Now
                    objShipment.ShipmentNo = ExtractElementFromSegment("ShipmentID", SegmentSet)
                    '
                    '
                    ' Surcharges:
                    '
                    objShipment.CarrierService.ServiceSurcharges.Add(add_ServiceSurcharge(0, "Machinable", _Convert.Boolean2TrueFalse(_USPS.StandardPost = objShipment.CarrierService.ServiceABBR), True))
                    ' Stealth Postage cannot be used with COD, USPS Insurance, Registered Mail, Automation rate, LabelSize of EnvelopeSize10 and Card shape mailpieces
                    objShipment.CarrierService.ServiceSurcharges.Add(add_ServiceSurcharge(0, "Stealth", _Convert.Boolean2TrueFalse(Not CBool(ExtractElementFromSegment("showHidePostage", SegmentSet, "False"))), True))
                    '
                    If gShip.IsCertifiedMail Then
                        objShipment.CarrierService.ServiceSurcharges.Add(add_ServiceSurcharge(0, "CertifiedMail", "ON", True))
                    End If
                    If gShip.IsReturnReceipt Then
                        objShipment.CarrierService.ServiceSurcharges.Add(add_ServiceSurcharge(0, "ReturnReceipt", "ON", True))
                    End If
                    '
                    If Not 0 = Val(ExtractElementFromSegment("actSAT", SegmentSet)) Then
                        objShipment.CarrierService.ServiceSurcharges.Add(add_ServiceSurcharge(0, "NoWeekendDelivery", "FALSE", True))
                    End If

                    '
                    If IsPackage_NeedsCustomsForm(objShipment) AndAlso Not String.IsNullOrEmpty(ExtractElementFromSegment("CustomsTypeOfContents", SegmentSet)) Then
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
                        ' re-use FedEx procedure:
                        Call _FedExWeb.Prepare_InternationalData(objShipment, False)
                        '
                        Call _EndiciaWeb.IsPackage_NeedsCustomsForm(objShipment)
                        Call create_IntegratedFormType(objShipment)
                        '
                        ' <ContentsType> was adjusted to exactly match the wording of Endicia Label Server.
                        If "Commercial Sample" = objShipment.CommInvoice.TypeOfContents Then
                            objShipment.CommInvoice.TypeOfContents = "Sample"
                        ElseIf "Returned Goods" = objShipment.CommInvoice.TypeOfContents Then
                            objShipment.CommInvoice.TypeOfContents = "ReturnedGoods"
                        End If
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
    Private Function add_ServiceSurcharge(ByVal servID As Integer, ByVal servName As String, ByVal servDesc As String, ByVal servIsToShow As Boolean, Optional servBaseCost As Double = 0, Optional servSellPrice As Double = 0, Optional servDiscount As Double = 0, Optional servIDNote As String = "") As _baseServiceSurcharge
        ''
        Dim objSurcharge As New _baseServiceSurcharge
        ''
        With objSurcharge
            .ID = servID
            .Name = servName
            .Description = servDesc
            .SellPrice = servSellPrice
            .BaseCost = servBaseCost
            .Discount = servDiscount
            .IDNote = servIDNote
            .IsToShow = servIsToShow
        End With
        ''
        add_ServiceSurcharge = objSurcharge
        ''
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

                Pack.DeclaredValue = Val(ExtractElementFromSegment("DECVAL", SegmentSet))

                If "X" = ExtractElementFromSegment("LTR", SegmentSet) Then
                    Pack.Weight_LBs = Val(ExtractElementFromSegment("BillableWeight", SegmentSet))
                    Pack.IsLetter = True
                Else
                    Pack.Weight_LBs = Val(ExtractElementFromSegment("ScaleReading", SegmentSet))
                    Pack.Dim_Height = Val(ExtractElementFromSegment("Height", SegmentSet))
                    Pack.Dim_Length = Val(ExtractElementFromSegment("LENGTH", SegmentSet))
                    Pack.Dim_Width = Val(ExtractElementFromSegment("Width", SegmentSet))
                    Pack.IsLetter = False
                End If
                gShip.actualWeight = Pack.Weight_LBs '' gShip.ActualWeight must be set to determine if the residential shipment is Home Delivery or Not while reading from database.
                Pack.PackagingType = ExtractElementFromSegment("Packaging", SegmentSet)
                Pack.Currency_Type = _IDs.CurrencyType '' CurrencyType variable was added to manipulate between CAD and USD.
                '
                '
                Pack.COD.Amount = Val(ExtractElementFromSegment("CODAMT", SegmentSet))
                If Pack.COD.Amount > 0 Then
                    Pack.COD.ChargeType = String.Empty
                    If CBool(ExtractElementFromSegment("CashiersCheck", SegmentSet, "False")) Then
                        Pack.COD.PaymentType = "8" ' check
                    Else
                        Pack.COD.PaymentType = "0" ' cash
                    End If
                    Pack.COD.CurrencyType = _IDs.CurrencyType
                    Pack.COD.AddCOD2Total = (Pack.COD.Amount < Val(ExtractElementFromSegment("CODAMTwSHIP", SegmentSet)))
                    If Pack.COD.AddCOD2Total Then
                        Pack.COD.Amount = Val(ExtractElementFromSegment("CODAMTwSHIP", SegmentSet))
                    End If
                End If
                '
                ' Re-use "Fx_SigType" field for signature type of all carriers:
                If _SignatureType.Adult_Signature = Val(ExtractElementFromSegment("Fx_SigType", SegmentSet, "-1")) Then
                    Pack.DeliveryConfirmation = "3" ' Adult
                ElseIf _SignatureType.Direct_Signature = Val(ExtractElementFromSegment("Fx_SigType", SegmentSet, "-1")) Then
                    Pack.DeliveryConfirmation = "2" ' Signature Confirmation
                Else
                    Pack.DeliveryConfirmation = String.Empty
                End If
                '
                If "X" = ExtractElementFromSegment("AH", SegmentSet) Then
                    Pack.IsAdditionalHandling = True
                End If
                If 0 < Val(ExtractElementFromSegment("AHPlus", SegmentSet)) Then
                    Pack.IsLargePackage = True
                End If
                '
                ' USPS Delivery Confirmation and Signature are only for Parcels packaging type of First Class.
                ' To Do: USMail.SignatureConfirmation = USMail.IsAvailable_SignatureConfirmation(objShipment.CarrierService.ServiceABBR)
                ''ol#9.236(8/8)... 'Adult Signature' option in addition to 'Signature Required' could be enabled from at ShipMaster screen
                If _SignatureType.Adult_Signature = Val(ExtractElementFromSegment("Fx_SigType", SegmentSet, "-1")) Then
                    objShipment.CarrierService.ServiceSurcharges.Add(add_ServiceSurcharge(0, "AdultSignature", _Convert.Boolean2OnOff(Pack.DeliveryConfirmation = "3"), True))
                    objShipment.CarrierService.ServiceSurcharges.Add(add_ServiceSurcharge(0, "SignatureWaiver", "FALSE", True))
                ElseIf _SignatureType.Direct_Signature = Val(ExtractElementFromSegment("Fx_SigType", SegmentSet, "-1")) Then
                    objShipment.CarrierService.ServiceSurcharges.Add(add_ServiceSurcharge(0, "SignatureConfirmation", _Convert.Boolean2OnOff(Pack.DeliveryConfirmation = "2"), True))
                    objShipment.CarrierService.ServiceSurcharges.Add(add_ServiceSurcharge(0, "SignatureWaiver", "FALSE", True))
                End If
                '
                Dim InsuredMail As String
                Dim is3rdInsOn As Boolean
                is3rdInsOn = ShipriteStartup.IsOn_gThirdPartyInsurance(objShipment.ShipToContact.Country, "USPS", objShipment.CarrierService.ServiceABBR, Val(gShip.DecVal))
                If is3rdInsOn Or (0 = Val(gShip.DecVal) And 0 = Pack.COD.Amount) Then
                    InsuredMail = "OFF"
                Else
                    InsuredMail = "ENDICIA"
                End If
                If 0 = Len(InsuredMail) Then
                    InsuredMail = "UspsOnline"
                End If
                objShipment.CarrierService.ServiceSurcharges.Add(add_ServiceSurcharge(0, "InsuredMail", InsuredMail, True, Pack.DeclaredValue))
                '
                ' USPS COD in Endicia has priority over Declared Value - COD amount will overwrite Insurance one.
                If Not "OFF" = UCase(InsuredMail) Then
                    '
                    If 0 < Pack.COD.Amount Then
                        '
                        ' If user has chosen to add COD amount to the shipping charges then the receipt should show COD Amount as the added up total of COD + Shipping Cost.
                        objShipment.CarrierService.ServiceSurcharges.Add(add_ServiceSurcharge(0, "COD", _Convert.Boolean2OnOff(0 < Pack.COD.Amount), True, Pack.COD.Amount))
                        objShipment.CarrierService.ServiceSurcharges.Add(add_ServiceSurcharge(0, "CostCenter", "", True, Pack.COD.Amount))
                    Else
                        'add_ServiceSurcharge Shipment, 0, "InsuredMail", InsuredMail, True, Val(gShip.DecVal)
                        objShipment.CarrierService.ServiceSurcharges.Add(add_ServiceSurcharge(0, "CostCenter", "", True, Pack.DeclaredValue))
                        '
                    End If
                    ' Signature cannot be waived for Express Mail shipments when Adult Signature, COD, USPS insurance, or Hold For Pickup has been selected.
                    objShipment.CarrierService.ServiceSurcharges.Add(add_ServiceSurcharge(0, "SignatureWaiver", "FALSE", True))
                    '
                ElseIf 0 < Pack.COD.Amount Then
                    '
                    ' USPS COD amount should be passed to DAZzle even if the 3rd Party Insurance is enabled in Ship Master.
                    objShipment.CarrierService.ServiceSurcharges.Add(add_ServiceSurcharge(0, "COD", _Convert.Boolean2OnOff(0 < Pack.COD.Amount), True, Pack.COD.Amount))
                    objShipment.CarrierService.ServiceSurcharges.Add(add_ServiceSurcharge(0, "CostCenter", "", True, Pack.COD.Amount))
                    ' Signature cannot be waived for Express Mail shipments when Adult Signature, COD, USPS insurance, or Hold For Pickup has been selected.
                    objShipment.CarrierService.ServiceSurcharges.Add(add_ServiceSurcharge(0, "SignatureWaiver", "FALSE", True))
                    '
                Else
                    '
                    objShipment.CarrierService.ServiceSurcharges.Add(add_ServiceSurcharge(0, "CostCenter", "", True, Pack.DeclaredValue))
                    '
                End If
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

            If _EndiciaWeb.objEndiciaCredentials IsNot Nothing Then
                If objShipment IsNot Nothing Then
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
                        If _Debug.IsINHOUSE Then
                            _EndiciaWeb.objEndiciaCredentials.Test = "YES"
                        Else
                            _EndiciaWeb.objEndiciaCredentials.Test = _Convert.Boolean2String_YesNo(gShip.TestShipment).ToUpper
                        End If

                        '
                        If _EndiciaWeb.Request_ShippingLabel(objShipment, objResponse) Then
                            '
                            For p% = 0 To objResponse.Packages.Count - 1
                                '
                                Dim retpack As New baseWebResponse_Package
                                retpack = objResponse.Packages(p%)
                                '
                                If Not 0 = Len(retpack.TrackingNo) Or Not 0 = Len(retpack.LabelImage) Then ' Some First Class Intl packages can return with no tracking number but still valid label.
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
                                    Call sql2cmd.Qry_UPDATE("Exported", EOD.PickupWaitingStatus, sql2cmd.TXT_, True, False, "Manifest", "PackageID = '" & retpack.PackageID & "'")
                                    Call sql2cmd.Qry_UPDATE("ReferralSource", "XML", sql2cmd.TXT_)
                                    If Not (objShipment.CarrierService.ServiceABBR = _USPS.FirstClassMail AndAlso objShipment.Packages(0).IsLetter AndAlso Not objShipment.CarrierService.ServiceSurcharges.Exists(Function(test) test.Name = "CertifiedMail" And test.Description = "ON")) Then
                                        Call sql2cmd.Qry_UPDATE("TRACKING#", retpack.TrackingNo, sql2cmd.TXT_)
                                    End If
                                    sql2exe = sql2cmd.Qry_UPDATE("Date", String.Format("{0:MM/dd/yyyy}", objShipment.CarrierService.ShipDate), sql2cmd.DTE_, False, True)
                                    _Debug.Print_(sql2exe)
                                    If -1 = IO_UpdateSQLProcessor(gShipriteDB, sql2exe) Then
                                        MsgBox("Failed to update Manifest with USPS tracking number...", MsgBoxStyle.Critical)
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
                                MsgBox("There were some alerts in the response: " & vbCr & alerts, vbExclamation, "USPS Alerts!")
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
                            If 0 = Len(Pack.TrackingNo) And 0 = Len(Pack.LabelImage) Then ' Some First Class Intl packages can return with no tracking number but still valid label.
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
                                Call _EndiciaWeb.Print_Label(Pack.PackageID, Pack.LabelImage)
                                If Convert.ToBoolean(General.GetPolicyData(gShipriteDB, "DuplicateLabel", "False")) Then
                                    Call _EndiciaWeb.Print_Label(Pack.PackageID, Pack.LabelImage)
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

#End Region

#Region "Printing Labels"
    Public Function Print_Label(ByVal PackageID As String, Optional ByVal LabelImage As String = "", Optional ByVal ServiceABBR As String = "") As Boolean
        Print_Label = False ' assume.
        _Debug.Stop_(PackageID & " - Prining Endicia Shipping Label...")
        If Not _Debug.IsINHOUSE Then
            If Not 0 = Len(LabelImage) And _Controls.Contains(_EndiciaWeb.objEndiciaCredentials.LabelImageType, "Thermal") Then
                Print_Label = print_LabelFromImage(LabelImage)
            Else
                Print_Label = print_LabelFromFile(PackageID, ServiceABBR)
            End If
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
    Private Function print_LabelFromFile(ByVal PackageID As String, Optional ByVal ServiceABBR As String = "") As Boolean
        print_LabelFromFile = False ' assume.

        Dim FileExtensionFilters As New List(Of String)
        FileExtensionFilters.AddRange(New String() {"ZPLII", "EPL2", "PDF", "GIF", "BMP", "JPG"})
        '
        Dim imageFile As String
        Dim fileExt As String
        Dim i As Integer : i = 1
        Dim labelfile As New _Labels
        Dim printerName As String = GetPolicyData(gReportsDB, "LabelPrinter")
        '
        'imageFile = _EndiciaWeb.objEndiciaCredentials.LabelFilePath & "\" & PackageID & "_label" & i & ".*"
        'wildcard doesn't work for file extensions...

        For c As Integer = 0 To FileExtensionFilters.Count - 1

            imageFile = _EndiciaWeb.objEndiciaCredentials.LabelFilePath & "\" & PackageID & "_label" & i & "." & FileExtensionFilters(c)

            Do While _Files.IsFileExist(imageFile, False)

                ' get file extention:
                fileExt = _Files.Get_FileExtension(imageFile)
                If Not String.IsNullOrEmpty(fileExt) Then
                    '
                    'imageFile = _EndiciaWeb.objEndiciaCredentials.LabelFilePath & "\" & imageFile
                    '_Debug.Print_(imageFile)
                    '
                    Select Case UCase(fileExt)
                        Case ".ZPLII", ".EPL2" ' Thermal
                            '
                            RawPrinterHelper.SendFileToPrinter(printerName, imageFile)
                        '
                        Case ".PDF"
                            '
                            ' To Do:
                            'If Printing_.Set_SystemPrinter2PrinterType(fldReportPrinter) Then
                            'print_LabelFromFile = Print_FilePDF(&O0, imageFile)
                            'End If
                            printerName = GetPolicyData(gReportsDB, "ReportPrinter")
                            RawPrinterHelper.SendFileToPrinter(printerName, imageFile)
                  '
                        Case ".GIF", ".BMP", ".JPG"
                            '
                            ' 'Endicia Label Server': Printing 2 International labes on 1 page.
                            labelfile.LabelPath.Add(imageFile)
                            '
                    End Select
                    '
                End If
                i = i + 1
                imageFile = _EndiciaWeb.objEndiciaCredentials.LabelFilePath & "\" & PackageID & "_label" & i & "." & FileExtensionFilters(c)
                'imageFile = _EndiciaWeb.objEndiciaCredentials.LabelFilePath & "\" & PackageID & "_label" & i & ".*" ' reset
            Loop
            i = 1
        Next
        '
        ' 'Endicia Label Server': Printing 2 International labes on 1 page.
        If 0 < labelfile.LabelPath.Count Then
            '
            labelfile.PrinterName = GetPolicyData(gReportsDB, "ReportPrinter")
            If Not String.IsNullOrEmpty(labelfile.PrinterName) Then

                If "GXG" = sr2endicia_MailClass(ServiceABBR) Then
                    ' one image per page:
                    Call Print_LabelsFromImage2On1Page(labelfile, False)
                Else
                    ' two images per page:
                    Call Print_LabelsFromImage2On1Page(labelfile, True)
                End If
            End If
            labelfile = Nothing
            '
        End If
        ''
        ' To Do:
        ''Set_SystemPrinterBack2Default
    End Function

#Region "Print Image Labels"
    Public Class _Labels
        Public PrinterName As String
        Public LabelPath As New List(Of String)
    End Class

    Private image1 As String
    Private image2 As String
    Private image3 As Bitmap
    Public Function Print_LabelsFromImage2On1Page(ByVal labels As Object, ByVal isFit2into1page As Boolean) As Boolean
        Print_LabelsFromImage2On1Page = False ' assume.
        Try
            Dim originalDefaultPrinterName As String = _Printers.Get_DefaultPrinter()
            Dim prter As New Drawing.Printing.PrinterSettings
            If _Printers.IsValid_PrinterName(labels.PrinterName) Then
                prter.PrinterName = labels.PrinterName
                _Debug.Print_(originalDefaultPrinterName, "->", prter.PrinterName)
                If Not originalDefaultPrinterName = prter.PrinterName Then
                    ' set as a default printer to re-set back to original one later.
                    _Printers.Set_DefaultPrinter(prter.PrinterName)
                    _Debug.Print_(prter.PrinterName, "IsDefaultPrinter: " & prter.IsDefaultPrinter)
                End If
                '
                If isFit2into1page Then
                    ' Printing 2 International labes on 1 page at a time:
                    For i As Integer = 0 To labels.LabelPath.Count - 1
                        If Not _Controls.IsOddNumber(i) Then
                            image1 = labels.LabelPath(i)
                        Else
                            image2 = labels.LabelPath(i)
                        End If

                        If _Controls.IsOddNumber(i) OrElse i = labels.LabelPath.Count - 1 Then
                            Call print_2LabelsOn1Page(labels.PrinterName)
                        End If
                    Next i
                    '
                Else
                    '
                    For i As Integer = 0 To labels.LabelPath.Count - 1
                        image1 = labels.LabelPath(i)
                        print_1LabelOn1Page(labels.PrinterName)
                    Next i
                    '
                End If
            End If
            '
            If Not originalDefaultPrinterName = prter.PrinterName Then
                ' set default back to original default printer.
                prter.PrinterName = originalDefaultPrinterName
                _Printers.Set_DefaultPrinter(prter.PrinterName)
                _Debug.Print_(prter.PrinterName, "IsDefaultPrinter: " & prter.IsDefaultPrinter)
            End If
            '
            prter = Nothing
            '
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to print a Shipping Label...")
        End Try
    End Function
    Private Sub print_1LabelOn1Page(ByVal printerName As String)
        Dim PrintDocument1 As Printing.PrintDocument = New Printing.PrintDocument
        AddHandler PrintDocument1.PrintPage, AddressOf _EndiciaWeb.PrintDocument1_PrintPage
        PrintDocument1.Print()
    End Sub
    Private Sub PrintDocument1_PrintPage(ByVal sender As System.Object, ByVal e As System.Drawing.Printing.PrintPageEventArgs)
        If _Files.IsFileExist(image1, False) Then
            Dim newImage1 As Image = Image.FromFile(image1)
            e.Graphics.DrawImage(newImage1, 5, 10, 790, 1050)
            image1 = String.Empty ' reset
        End If
    End Sub
    Private Sub print_2LabelsOn1Page(ByVal printerName As String)
        Dim PrintDocument2 As Printing.PrintDocument = New Printing.PrintDocument
        AddHandler PrintDocument2.PrintPage, AddressOf _EndiciaWeb.PrintDocument2_PrintPage
        PrintDocument2.Print()
    End Sub
    Private Sub PrintDocument2_PrintPage(ByVal sender As System.Object, ByVal e As System.Drawing.Printing.PrintPageEventArgs)
        If _Files.IsFileExist(image1, False) Then
            Dim newImage1 As Image = Image.FromFile(image1)
            e.Graphics.DrawImage(newImage1, 5, 10, 790, 530)
            image1 = String.Empty ' reset
        End If
        If _Files.IsFileExist(image2, False) Then
            Dim newImage2 As Image = Image.FromFile(image2)
            e.Graphics.DrawImage(newImage2, 5, 542, 790, 530)
            image2 = String.Empty ' reset
        End If
    End Sub



    ''AP(03/13/2020) - Added Endicia Stamps web service request to print DYMO/NetStamp stamps.
    Public Function Print_StampFromImage(ByVal printerName As String, ByVal imageData As String, Optional ByVal isImageBase64 As Boolean = True) As Boolean
        Print_StampFromImage = False ' assume.
        Try
            '
            Dim prter As New Drawing.Printing.PrinterSettings
            If _Printers.IsValid_PrinterName(printerName) AndAlso imageData IsNot Nothing AndAlso Not String.IsNullOrEmpty(imageData) Then
                '
                prter.PrinterName = printerName
                prter.DefaultPageSettings.PaperSize = New Printing.PaperSize("Dymo", 163, 132) ' 1 5/8 in. x 1 5/16 in. ' DYMO 30915 = 1 5/8 in. x 1 1/4 in. ; NetStamps = 1 5/16 in. x 1 5/16 in.
                'prter.DefaultPageSettings.Margins = New Printing.Margins(0, 0, 500, 0)
                'prter.DefaultPageSettings.PaperSource = New Printing.PaperSource() With {.SourceName = "Left Roll"} '.SourceName = "Automatically Select" ' "Left Roll"
                '
                If isImageBase64 Then
                    Dim labelBase64 As String = imageData ' base64 image string
                    image3 = _Convert.Base64String2Bitmap(labelBase64)
                    print_StampImageOnPage(prter)
                Else
                    Dim labelFilePath As String = imageData ' label path
                    If _Files.IsFileExist(labelFilePath, False) Then
                        image3 = Image.FromFile(labelFilePath)
                        print_StampImageOnPage(prter)
                    End If
                End If
                '
                Print_StampFromImage = True
                '
            End If
            '
            prter = Nothing
            '
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to print Stamp image...")
        End Try
    End Function
    Public Function Print_StampsFromImages(ByVal labels As _Labels, Optional ByVal isImageBase64 As Boolean = True) As Boolean
        Print_StampsFromImages = False ' assume.
        Try
            '
            If labels IsNot Nothing AndAlso labels.PrinterName IsNot Nothing AndAlso Not String.IsNullOrEmpty(labels.PrinterName) Then
                For i As Integer = 1 To labels.LabelPath.Count
                    Print_StampsFromImages = Print_StampFromImage(labels.PrinterName, labels.LabelPath(i), isImageBase64)
                    If Not Print_StampsFromImages Then Exit For
                Next i
            End If
            '
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to print Stamps from images...")
        End Try
    End Function
    Private Sub print_StampImageOnPage(ByVal pSettings As Printing.PrinterSettings)
        Dim PrintDocument3 As Printing.PrintDocument = New Printing.PrintDocument
        PrintDocument3.PrinterSettings = pSettings
        AddHandler PrintDocument3.PrintPage, AddressOf _EndiciaWeb.PrintDocument4_PrintPage
        PrintDocument3.Print()
    End Sub
    Private Sub PrintDocument4_PrintPage(ByVal sender As System.Object, ByVal e As System.Drawing.Printing.PrintPageEventArgs)
        If image3 IsNot Nothing Then
            'Dim newImage3 As Image = TryCast(image3, Image)
            e.Graphics.DrawImage(image3, 0, 0)
            image3 = Nothing ' reset
        End If
    End Sub



#End Region

#End Region


#Region "Test Objects"
    Public Function set_SRSetupObject() As _EndiciaSetup
        set_SRSetupObject = New _EndiciaSetup
        With set_SRSetupObject
            .RequesterID = "lsrp"
            .AccountID = "634409"
            .PassPhrase = "ShipriteGizmo"
            .PartnerCustomerID = "shiprite"
            .PartnerTransactionID = "6789EFGH"
            '
            .Test = "Yes" '"No" '
            '
            .LabelFilePath = "C:\Shiprite\Endicia\Test"
            .LabelImageType = "Eltron Thermal" '"Eltron Thermal" '"GIF Image"
            .LabelType = "Default" 'Default, CertifiedMail, DestinationConfirm, Domestic, International
            ' will be set to 'Integrated' when Customs form is assigned to <IntegratedFormType>:
            .LabelSubtype = "None" ' None or Integrated ' Required when label type is Domestic or International
            .LabelSize = "4X6"
            '
            .DbPath = "c:\ShipRite\Shiprite.mdb"
            '.DbPath = "C:\Documents and Settings\odonchuk\My Documents\My Projects\ShipRite\ShipRite Customers\ShipriteSetup_Integrations_Postal_Authority\ShipriteSetup_Integrations.mdb"
        End With
    End Function
    Public Function set_SRSetup_TEST_DYMO() As _EndiciaSetup
        set_SRSetup_TEST_DYMO = New _EndiciaSetup
        With set_SRSetup_TEST_DYMO
            .RequesterID = "lsrp"
            .AccountID = "946942"
            .PassPhrase = "Shiprite1312"
            .PartnerCustomerID = "shiprite"
            .PartnerTransactionID = "6789EFGH"
            '
            .Test = "Yes" '"No" '
            '
            .LabelFilePath = "C:\Shiprite\Endicia\Test"
            .LabelImageType = "PNG" '"PDF" '"PNG" 
            .LabelType = "30915" 'DYMO Roll; 356-2 - DYMO Sheets;
            ' Activation codes are available on the backings of every DYMO Stamps® Postage Label Roll. These activation codes are alphanumeric and can be up to 50 characters long.
            .LabelSubtype = "5848758385734ADRFGR" ' Activation Code validates that genuine DYMO labels are used to print stamps and ensures that no postage is rejected by USPS due to printing stamps on counterfeit labels.
            .LabelSize = "DYMO Roll"
            '
            .DbPath = "c:\ShipRite\Shiprite.mdb"
            '.DbPath = "C:\Documents and Settings\odonchuk\My Documents\My Projects\ShipRite\ShipRite Customers\ShipriteSetup_Integrations_Postal_Authority\ShipriteSetup_Integrations.mdb"
        End With
    End Function

#End Region

#Region "Request: Dial-A-ZIP"
    Public original As _baseContact
    Public verified As _baseContact
    Public verifiedcodes As List(Of String)
    Public isSaveVerifiedAddress As Boolean
    Public Function Submit_DialAZip(ByVal path2save As String, ByRef contact As Object) As Boolean
        isSaveVerifiedAddress = False ' assume.
        If Request_DialAZip(path2save, contact) Then
            original = New _baseContact
            If copy_OriginalAddress(original, contact) Then
                'DialAZipForm.path2save = path2save
                'DialAZipForm.ShowDialog()
                'DialAZipForm.Dispose()
                isSaveVerifiedAddress = True
            End If
        End If
        Submit_DialAZip = isSaveVerifiedAddress
    End Function
    Public Function Request_DialAZip(ByVal path2save As String, ByRef obj As Object) As Boolean
        Request_DialAZip = False ' assume.
        Try
            ' Properly formatted XML requests are sent to the URL using the following calling convention:
            ' http://www.dial-a-zip.com/XML-Dial-A-ZIP/DAZService.asmx/MethodZIPValidate?input=<VERIFYADDRESS>...</VERIFYADDRESS>
            '
            Dim xdoc As New Xml.XmlDocument
            Dim xmlcall As String = "http://www.dial-a-zip.com/XML-Dial-A-ZIP/DAZService.asmx/MethodZIPValidate?input="
            Dim request As String = String.Empty
            Dim response As String = String.Empty
            '
            request = String.Format("<VERIFYADDRESS>" &
                                   "<COMMAND>ZIP1</COMMAND><SERIALNO>946942</SERIALNO><PASSWORD>Gizmo1312</PASSWORD><USER>946942</USER>" &
                                   "<ADDRESS0>{0}</ADDRESS0>" &
                                   "<ADDRESS1>{1}</ADDRESS1>" &
                                   "<ADDRESS2>{2}</ADDRESS2>" &
                                   "<ADDRESS3>{3}, {4}, {5}</ADDRESS3>" &
                                   "</VERIFYADDRESS>",
                                   String.Empty,
                                   obj.Addr1,
                                   obj.Addr2,
                                   obj.City, obj.State, obj.Zip).Replace(" ", "%20")

            ' send it wiht GET method:
            If _XML.Send_HttpWebRequest(String.Format("{0}{1}", xmlcall, request), response) Then
                xdoc.LoadXml(response)
                If Not String.Empty = path2save Then
                    xdoc.Save(path2save & "\DialAZip_response.xml")
                End If
                If read_DialAZip(xdoc) Then
                    Request_DialAZip = True
                End If
            Else
                _MsgBox.ErrorMessage(response, "Failed to create a Dial-A-ZIP request...", DialAZip)
            End If

        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to create a Dial-A-ZIP request...")
        End Try
    End Function
    Private Function read_DialAZip(ByVal xdoc As Xml.XmlDocument) As Boolean
        Dim nreader As New Xml.XmlNodeReader(xdoc)
        Dim readnode As String = String.Empty
        verifiedcodes = New List(Of String)
        verified = New _baseContact
        If _XML.NodeReader_GetValueByNodeName(nreader, "ReturnCode", readnode) Then
            verifiedcodes.Add(String.Format("ReturnCode: {0}", describe_ReturnCode(readnode)))
        End If
        If _XML.NodeReader_GetValueByNodeName(nreader, "ZIP5", readnode) Then
            verified.Zip = readnode
        End If
        If _XML.NodeReader_GetValueByNodeName(nreader, "AddrLine1", readnode) Then
            verified.Addr1 = readnode
        End If
        If _XML.NodeReader_GetValueByNodeName(nreader, "AddrLine2", readnode) Then
            verified.Addr2 = readnode
        End If
        If _XML.NodeReader_GetValueByNodeName(nreader, "City", readnode) Then
            verified.City = readnode
        End If
        If _XML.NodeReader_GetValueByNodeName(nreader, "State", readnode) Then
            verified.State = readnode
        End If
        If _XML.NodeReader_GetValueByNodeName(nreader, "AddrExists", readnode) Then
            verifiedcodes.Add(String.Format("AddrExists: {0}", readnode))
        End If
        If _XML.NodeReader_GetValueByNodeName(nreader, "RecType", readnode) Then
            verifiedcodes.Add(String.Format("RecType: {0}", describe_RecType(readnode)))
        End If
        If _XML.NodeReader_GetValueByNodeName(nreader, "HSA", readnode) Then
            If "Y" = readnode Then
                verifiedcodes.Add(String.Format("Delivery Point Match Found: Yes"))
            Else
                verifiedcodes.Add(String.Format("Delivery Point Match Found: No"))
            End If
        End If
        If _XML.NodeReader_GetValueByNodeName(nreader, "DPVFootNote", readnode) Then
            If 4 = readnode.Length Then
                Dim note1 As String = describe_DPVFootNote(readnode.Substring(0, 2))
                Dim note2 As String = describe_DPVFootNote(readnode.Substring(2, 2))
                verifiedcodes.Add(String.Format("Delivery Point Validation: {0}. {1}.", note1, note2))
            End If
        End If
        If _XML.NodeReader_GetValueByNodeName(nreader, "RDI", readnode) Then
            verified.Residential = (Not "B" = readnode)
        Else
            ''ol#1.1.70(8/21)... If Dial-A-Zip returns 'RDI' code as null then Residential defaults to TRUE.
            verified.Residential = True ' by default
        End If
        Return True
    End Function

    Public Function copy_OriginalAddress(ByRef obj1 As Object, ByVal obj2 As Object) As Boolean
        obj1.Addr1 = obj2.Addr1
        obj1.Addr2 = obj2.Addr2
        obj1.City = obj2.City
        obj1.State = obj2.State
        obj1.Zip = obj2.Zip
        obj1.Residential = obj2.Residential
        Return True
    End Function
    Private Function describe_ReturnCode(ByVal code As String) As String
        Select Case Val(code)
            Case 10 : Return "Invalid Dual Address – May indicate that more than one delivery address is detected"
            Case 11 : Return "Invalid City/State/ZIP Code - ZIP Code could not be found because neither a valid City, State, nor valid 5-digit ZIP Code was present"
            Case 12 : Return "Invalid State - The State code in the address is invalid. Note that only US State and U.S. Territories and possession abbreviations are valid."
            Case 13 : Return "Invalid City – The City in the address submitted is invalid. Remember, city names cannot begin with numbers."
            Case 21 : Return "Address Not Found – The address as submitted could not be found. Check for excessive abbreviations in the street address line or in the City name."
            Case 22 : Return "Multiple Responses – More than one ZIP+4 was found. Check for missing address elements, or run ZIPM request for a list of possible valid addresses."
            Case 25 : Return "City, State and ZIP Code are valid, but street address is not a match."
            Case 31 : Return "Exact Match – No corrective action required"
            Case 32 : Return "Default Match – More information, such as an apartment or suite number, may give a more specific address."
            Case Else : Return String.Empty
        End Select
    End Function
    Private Function describe_RecType(ByVal code As String) As String
        Select Case code
            Case "S" : Return "Street record"
            Case "P" : Return "Post Office Box"
            Case "R" : Return "Rural Route or Highway Contract"
            Case "F" : Return "Firm Match"
            Case "G" : Return "General Delivery"
            Case "H" : Return "Building or Apartment"
            Case Else : Return String.Empty
        End Select
    End Function
    Private Function describe_DPVFootNote(ByVal code As String) As String
        Select Case code
            Case "AA" : Return "ZIP4 matched"
            Case "A1" : Return "ZIP4 did not match"
            Case "BB" : Return "HSA_DPV confirmed entire address"
            Case "CC" : Return "HSA_DPV confirmed address by dropping secondary information"
            Case "F1" : Return "Matched ZIP4 military record"
            Case "G1" : Return "Matched ZIP4 General Delivery record"
            Case "M1" : Return "Primary number missing"
            Case "M3" : Return "Primary number invalid"
            Case "N1" : Return "HSA_DPV confirmed a hi-rise address without secondary information"
            Case "P1" : Return "Box number missing"
            Case "P3" : Return "Box number invalid"
            Case "RR" : Return "HSC_DPV confirmed address with PMB information"
            Case "R1" : Return "HSC_DPV confirmed address without PMB information"
            Case "U1" : Return "Matched ZIP4 unique ZIP Code record"
            Case Else : Return String.Empty
        End Select
    End Function
#End Region

#Region "Verify Address"
    Public Function Request_ValidateAddress(ByRef obj As Object) As Boolean
        Return Request_ValidateAddress(objEndiciaCredentials, obj)
    End Function
    Public Function Request_ValidateAddress(ByVal SRSetup As _EndiciaSetup, ByRef obj As Object) As Boolean
        Request_ValidateAddress = False ' assume.
        Try
            ' Dim validateRequest As New EwsLabelService.ValidateAddressInfoRequest
            Dim validateRequest As New Endicia_LabelService.ValidateAddressInfoRequest
            '
            If get_SecuritySettings(SRSetup, validateRequest) Then
                If get_AddressToValidate(obj, validateRequest.Address) Then
                    If Not _Files.IsFileExist(String.Format("{0}\no_xml.txt", SRSetup.LabelFilePath), False) Then

                        Dim xdoc As New Xml.XmlDocument
                        xdoc.LoadXml(serializeValidateRequest2string(validateRequest))
                        hold_XMLdirpath = SRSetup.LabelFilePath
                        _EndiciaWeb.Save_XMLfile(xdoc, "ValidateAddress_request.xml")
                    End If
                    '
                    Request_ValidateAddress = getResponse_ValidateRequest(SRSetup, validateRequest, verified)
                    '
                End If
            End If
            '
        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to create a Validate Address request...")
        End Try
    End Function

    Private Function getResponse_ValidateRequest(ByVal SRSetup As Object, ByVal validateRequest As Endicia_LabelService.ValidateAddressInfoRequest, ByRef verified_response As Object) As Boolean
        getResponse_ValidateRequest = False ' assume.
        Try
            'System.Net.ServicePointManager.CertificatePolicy = New TrustAllCertificatePolicy
            System.Net.ServicePointManager.SecurityProtocol = Net.SecurityProtocolType.Tls12
            Dim ewsLabelService As New Endicia_LabelService.EwsLabelService
            Dim validateResponse As Endicia_LabelService.ValidateAddressInfoResponse
            '
            validateResponse = ewsLabelService.ValidateAddress(validateRequest)
            '
            If Not _Files.IsFileExist(String.Format("{0}\no_xml.txt", _EndiciaWeb.hold_XMLdirpath), False) Then
                Dim xdoc As New Xml.XmlDocument
                xdoc.LoadXml(serializeValidateResponse2string(validateResponse))
                _EndiciaWeb.Save_XMLfile(xdoc, "ValidateAddress_response.xml")
            End If
            '
            verifiedcodes = New List(Of String)
            verified_response = New _baseContact
            '
            If validateResponse IsNot Nothing Then
                If describe_ReturnCode(validateResponse.Status.ToString) = String.Empty And Not String.IsNullOrEmpty(validateResponse.ErrorMessage) Then
                    verifiedcodes.Add(String.Format("ReturnCode: {0}", validateResponse.Status.ToString))
                    verifiedcodes.Add(String.Format("Error: {0}", validateResponse.ErrorMessage))
                Else
                    verifiedcodes.Add(String.Format("ReturnCode: {0}", describe_ReturnCode(validateResponse.Status.ToString)))
                    If validateResponse.Address IsNot Nothing Then
                        If Not String.IsNullOrEmpty(validateResponse.Address.PostalCode) Then
                            verified_response.Zip = validateResponse.Address.PostalCode
                        End If
                        If Not String.IsNullOrEmpty(validateResponse.Address.Address1) Then
                            verified_response.Addr1 = validateResponse.Address.Address1
                        End If
                        If Not String.IsNullOrEmpty(validateResponse.Address.Address2) Then
                            verified_response.Addr2 = validateResponse.Address.Address2
                        End If
                        If Not String.IsNullOrEmpty(validateResponse.Address.City) Then
                            verified_response.City = validateResponse.Address.City
                        End If
                        If Not String.IsNullOrEmpty(validateResponse.Address.State) Then
                            verified_response.State = validateResponse.Address.State
                        End If
                        If Not String.IsNullOrEmpty(validateResponse.Address.CountryCode) Then
                            verified_response.CountryCode = validateResponse.Address.CountryCode
                        End If
                    End If
                    verified_response.Residential = validateResponse.ResidentialDeliveryIndicator
                    '
                    verifiedcodes.Add(String.Format("AddrExists: {0}", validateResponse.AddressMatch))
                    If validateResponse.StatusCodes IsNot Nothing Then
                        If validateResponse.StatusCodes.dpvFootnotes IsNot Nothing Then
                            Dim dpvFootnotes As String = ""
                            For Each footnote As Endicia_LabelService.DpvFootnote In validateResponse.StatusCodes.dpvFootnotes
                                Dim dpvnote As String = describe_DPVFootNote(footnote.Value)
                                If Not String.IsNullOrEmpty(dpvnote) Then
                                    dpvFootnotes &= dpvnote & ". "
                                End If
                            Next
                            If dpvFootnotes.Length > 0 Then
                                verifiedcodes.Add(String.Format("Delivery Point Validation: {0}", dpvFootnotes.Trim))
                            End If
                        End If
                    End If
                    ' nothing matches in new ELS response
                    'If _XML.NodeReader_GetValueByNodeName(nreader, "RecType", readnode) Then
                    '    verifiedcodes.Add(String.Format("RecType: {0}", describe_RecType(readnode)))
                    'End If
                    'If _XML.NodeReader_GetValueByNodeName(nreader, "HSA", readnode) Then
                    '    If "Y" = readnode Then
                    '        verifiedcodes.Add(String.Format("Delivery Point Match Found: Yes"))
                    '    Else
                    '        verifiedcodes.Add(String.Format("Delivery Point Match Found: No"))
                    '    End If
                    'End If
                End If
            End If
            '
            getResponse_ValidateRequest = True
            '
        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message) : _Debug.Print_(ex.Detail.LastChild.InnerText)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to get a response for Validate Address request...")
        End Try

    End Function

    Private Function get_SecuritySettings(ByVal SRSetup As Object, ByRef validateRequest As Endicia_LabelService.ValidateAddressInfoRequest) As Boolean
        validateRequest.CertifiedIntermediary = New Endicia_LabelService.CertifiedIntermediary
        validateRequest.CertifiedIntermediary.AccountID = SRSetup.AccountID
        validateRequest.CertifiedIntermediary.PassPhrase = SRSetup.PassPhrase
        validateRequest.RequesterID = SRSetup.RequesterID
        Return True
    End Function

    Private Function get_AddressToValidate(ByVal obj As Object, ByRef validateRequestAddress As Endicia_LabelService.ValidateAddressInfo) As Boolean
        validateRequestAddress = New Endicia_LabelService.ValidateAddressInfo
        If obj.Name IsNot Nothing AndAlso Not String.IsNullOrEmpty(obj.Name) Then
            validateRequestAddress.Name = obj.Name
        Else
            validateRequestAddress.Name = "Name"
        End If
        If obj.CompanyName IsNot Nothing AndAlso Not String.IsNullOrEmpty(obj.CompanyName) Then
            validateRequestAddress.Company = obj.CompanyName
        End If
        validateRequestAddress.Address1 = obj.Addr1
        validateRequestAddress.Address2 = obj.Addr2
        validateRequestAddress.City = obj.City
        validateRequestAddress.State = obj.State
        validateRequestAddress.PostalCode = obj.Zip
        Return True
    End Function
    Private Function serializeValidateRequest2string(obj As Endicia_LabelService.ValidateAddressInfoRequest) As String
        Dim xml_serializer As New XmlSerializer(GetType(Endicia_LabelService.ValidateAddressInfoRequest))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeValidateRequest2string = string_writer.ToString()
        string_writer.Close()
    End Function
    Private Function serializeValidateResponse2string(obj As Endicia_LabelService.ValidateAddressInfoResponse) As String
        Dim xml_serializer As New XmlSerializer(GetType(Endicia_LabelService.ValidateAddressInfoResponse))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeValidateResponse2string = string_writer.ToString()
        string_writer.Close()
    End Function
#End Region

#Region "Request: SCAN Form"
    Public Function Request_SCANform(ByRef vb_response As baseWebResponse_Shipment) As Boolean
        Return Request_SCANform(objEndiciaCredentials, vb_response)
    End Function
    Public Function Request_SCANform(ByVal SRSetup As _EndiciaSetup, ByRef vb_response As baseWebResponse_Shipment) As Boolean
        Request_SCANform = False ' assume.
        Try
            '
            If vb_response.Packages.Count > 0 Then
                If _MsgBox.QuestionMessage("Important!" & _Controls.vbCr_ & "Tracking numbers included in a SCAN Form cannot be refunded.", "Continue SCAN ?") Then
                    Dim request As New Endicia_LabelService.SCANRequest
                    If load_setupdata(request) Then
                        '
                        Dim scanlist(vb_response.Packages.Count - 1) As String
                        For i As Integer = 0 To vb_response.Packages.Count - 1
                            scanlist(i) = vb_response.Packages(i).TrackingNo
                        Next i
                        '
                        request.PicNumbers = scanlist
                        request.GetSCANRequestParameters = New Endicia_LabelService.GetSCANParameters
                        request.GetSCANRequestParameters.ImageFormat = "PDF"
                        request.RequestID = DateTime.Now
                        '
                        Dim xdoc As New Xml.XmlDocument
                        xdoc.LoadXml(serializeShipRequest2string(request))
                        hold_XMLdirpath = SRSetup.LabelFilePath
                        _EndiciaWeb.Save_XMLfile(xdoc, "SCANform_request.xml")
                        '
                        Request_SCANform = response_SCANform(SRSetup, request, vb_response)
                        '
                    End If
                End If
            End If
            '
        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to create a SCAN Form request...")
        End Try
    End Function
    Private Function response_SCANform(ByVal SrSetup As Object, ByVal request As Endicia_LabelService.SCANRequest, ByRef vb_response As Object) As Boolean
        response_SCANform = False ' assume.
        Try
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12

            Dim response As New Endicia_LabelService.SCANResponse
            Dim ewsLabelService As Endicia_LabelService.EwsLabelService
            '
            ewsLabelService = New Endicia_LabelService.EwsLabelService
            response = ewsLabelService.GetSCAN(request)
            '
            Dim xdoc As New Xml.XmlDocument
            xdoc.LoadXml(serializeShipResponse2string(response))
            _EndiciaWeb.Save_XMLfile(xdoc, "SCANform_response.xml")
            '
            If response.SubmissionID IsNot Nothing Then
                ' used for re-printing SCAN form
                vb_response.ShipmentID = response.SubmissionID
            End If
            '
            response_SCANform = (response.SCANForm IsNot Nothing)
            If response_SCANform Then
                '
                Dim labelString As String = String.Empty
                Dim labelFileExt As String = "pdf"
                Dim labelFile As String = SrSetup.LabelFilePath & "\SCANform_" & vb_response.ShipmentID & "." & labelFileExt
                Dim labelBase64 As String = response.SCANForm
                '
                If _Files.WriteFile_ToEnd(_Convert.Base64String2Byte(labelBase64), labelFile) Then
                    If _Files.ReadFile_ToEnd(labelFile, False, labelString) Then
                        vb_response.AdditionalInfo = labelString
                    End If
                End If
                '
            End If
            '
            If response.ErrorMessage IsNot Nothing Then
                ' Error
                _MsgBox.WarningMessage(response.ErrorMessage, EndiciaLavelServer)
            End If
            '
        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to get a response to SCAN Form request...")
        End Try
    End Function

    Private Function serializeShipRequest2string(obj As Endicia_LabelService.SCANRequest) As String
        Dim xml_serializer As New XmlSerializer(GetType(Endicia_LabelService.SCANRequest))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeShipRequest2string = string_writer.ToString()
        string_writer.Close()
    End Function
    Private Function serializeShipResponse2string(obj As Endicia_LabelService.SCANResponse) As String
        Dim xml_serializer As New XmlSerializer(GetType(Endicia_LabelService.SCANResponse))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeShipResponse2string = string_writer.ToString()
        string_writer.Close()
    End Function

    Private Function load_setupdata(ByRef request As Endicia_LabelService.SCANRequest) As Boolean
        If objEndiciaCredentials IsNot Nothing Then
            request.CertifiedIntermediary = New Endicia_LabelService.CertifiedIntermediary
            request.CertifiedIntermediary.AccountID = objEndiciaCredentials.AccountID
            request.CertifiedIntermediary.PassPhrase = objEndiciaCredentials.PassPhrase
            request.RequesterID = objEndiciaCredentials.RequesterID
            Return True
        Else
            _MsgBox.WarningMessage("Failed to load Endicia Account credentials!", "Can't Login!")
            Return False
        End If
    End Function


#End Region

#Region "Request: Postage Refund"
    Public Function Request_PackageRefund(ByRef vb_response As Object) As Boolean
        Return Request_PackageRefund(objEndiciaCredentials, vb_response)
    End Function
    Public Function Request_PackageRefund(ByVal SRSetup As _EndiciaSetup, ByRef vb_response As Object) As Boolean
        Request_PackageRefund = False ' assume.
        Try
            '
            Dim request As New Endicia_LabelService.RefundRequest
            If vb_response.Packages.Count > 0 Then
                If load_setupdata(request) Then
                    '
                    Dim scanlist(vb_response.Packages.Count - 1) As String
                    For i As Integer = 0 To vb_response.Packages.Count - 1
                        scanlist(i) = vb_response.Packages(i).TrackingNo
                    Next i
                    '
                    request.PicNumbers = scanlist
                    request.RequestID = DateTime.Now
                    '
                    Dim xdoc As New Xml.XmlDocument
                    xdoc.LoadXml(serializeShipRequest2string(request))
                    hold_XMLdirpath = SRSetup.LabelFilePath
                    _EndiciaWeb.Save_XMLfile(xdoc, "PostageRefund_request.xml")
                    '
                    Request_PackageRefund = response_PackageRefund(SRSetup, request, vb_response)
                    '
                End If
            End If
            '
        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to create a Postage Refund request...")
        End Try

    End Function
    Private Function response_PackageRefund(ByVal SrSetup As Object, ByVal request As Endicia_LabelService.RefundRequest, ByRef vb_response As Object) As Boolean
        response_PackageRefund = False ' assume.
        Try
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12

            Dim response As New Endicia_LabelService.RefundResponse
            Dim ewsLabelService As Endicia_LabelService.EwsLabelService
            '
            ewsLabelService = New Endicia_LabelService.EwsLabelService
            response = ewsLabelService.GetRefund(request)
            '
            Dim xdoc As New Xml.XmlDocument
            xdoc.LoadXml(serializeShipResponse2string(response))
            _EndiciaWeb.Save_XMLfile(xdoc, "PostageRefund_response.xml")
            '
            If response.Refund IsNot Nothing Then
                For vb As Integer = 0 To vb_response.Packages.Count - 1
                    For i As Integer = 0 To response.Refund.Length - 1
                        '
                        Dim lblresponse As Endicia_LabelService.LabelResponse = response.Refund(i)
                        If lblresponse IsNot Nothing Then
                            Dim trackNo As String = lblresponse.PicNumber
                            If trackNo = vb_response.Packages(vb).TrackingNo Then
                                If lblresponse.RefundStatus = Endicia_LabelService.RefundStatus.Approved Then
                                    vb_response.Packages(vb).LabelImage = "YES"
                                    vb_response.Packages(vb).LabelCustomsImage = lblresponse.RefundStatusMessage
                                    MessageBox.Show("Refund Approved!", EndiciaLavelServer, MessageBoxButton.OK, MessageBoxImage.Information)
                                Else
                                    vb_response.Packages(vb).LabelImage = "NO"
                                    vb_response.Packages(vb).LabelCustomsImage = lblresponse.RefundStatusMessage
                                    MessageBox.Show("Refund Not Approved..." & Environment.NewLine &
                                           String.Format("Note: {0}", vb_response.Packages(vb).LabelCustomsImage), EndiciaLavelServer, MessageBoxButton.OK, MessageBoxImage.Information)
                                End If
                                Exit For
                            End If
                        End If
                        '
                    Next i
                Next vb
            End If
            '
            If response.ErrorMessage IsNot Nothing Then
                ' Error
                _MsgBox.WarningMessage(response.ErrorMessage, EndiciaLavelServer)
            Else
                response_PackageRefund = True
            End If
            '
        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to get a response to Postage Refund request...")
        End Try
    End Function

    Private Function serializeShipRequest2string(obj As Endicia_LabelService.RefundRequest) As String
        Dim xml_serializer As New XmlSerializer(GetType(Endicia_LabelService.RefundRequest))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeShipRequest2string = string_writer.ToString()
        string_writer.Close()
    End Function
    Private Function serializeShipResponse2string(obj As Endicia_LabelService.RefundResponse) As String
        Dim xml_serializer As New XmlSerializer(GetType(Endicia_LabelService.RefundResponse))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeShipResponse2string = string_writer.ToString()
        string_writer.Close()
    End Function

    Private Function load_setupdata(ByRef request As Endicia_LabelService.RefundRequest) As Boolean
        If objEndiciaCredentials IsNot Nothing Then
            request.CertifiedIntermediary = New Endicia_LabelService.CertifiedIntermediary
            request.CertifiedIntermediary.AccountID = objEndiciaCredentials.AccountID
            request.CertifiedIntermediary.PassPhrase = objEndiciaCredentials.PassPhrase
            request.RequesterID = objEndiciaCredentials.RequesterID
            Return True
        Else
            _MsgBox.WarningMessage("Failed to load Endicia Account credentials!", "Can't Login!")
            Return False
        End If
    End Function

#End Region

#Region "Request: Package Status"

    Public Class PICNumber
        Public TrackingNo As String
        Public Status As String
        Public StatusBreakdown As New List(Of String)
        Public Overrides Function ToString() As String
            Return TrackingNo ' default object string
        End Function
    End Class
    Public tracks As List(Of PICNumber)

    Public Function Request_PackageStatus(ByVal SRSetup As Object, ByRef vb_response As Object) As Boolean
        Request_PackageStatus = False
        Try
            tracks = New List(Of PICNumber)

            Dim request As New Endicia_LabelService.PackageStatusRequest
            If vb_response.Packages.Count > 0 Then
                If load_setupdata(request) Then
                    '
                    Dim scanlist(vb_response.Packages.Count - 1) As String
                    For i As Integer = 0 To vb_response.Packages.Count - 1
                        scanlist(i) = vb_response.Packages(i).TrackingNo
                    Next i
                    '
                    request.PicNumbers = scanlist
                    request.RequestID = DateTime.Now
                    Dim reqoptions As New Endicia_LabelService.PackageStatusRequestOptions
                    reqoptions.PackageStatus = Endicia_LabelService.GetPackageStatus.COMPLETE
                    request.RequestOptions = reqoptions
                    '
                    Dim xdoc As New Xml.XmlDocument
                    xdoc.LoadXml(serializeShipRequest2string(request))
                    hold_XMLdirpath = SRSetup.LabelFilePath
                    _EndiciaWeb.Save_XMLfile(xdoc, "PackageStatus_request.xml")
                    '
                    Request_PackageStatus = response_PackageStatus(SRSetup, request, tracks)
                    If Request_PackageStatus Then
                        ' display
                        ' To Do:
                        'PackageStatus.txtAccount.Text = request.CertifiedIntermediary.AccountID
                        'PackageStatus.ShowDialog()
                        'PackageStatus.Dispose()
                    End If
                    '
                End If
            End If

        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to create a Package Status request...")
        End Try
    End Function
    Private Function response_PackageStatus(ByVal SrSetup As Object, ByVal request As Endicia_LabelService.PackageStatusRequest, ByRef tracks As List(Of PICNumber)) As Boolean
        response_PackageStatus = False
        Try
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12

            Dim response As New Endicia_LabelService.PackageStatusResponse
            Dim ewsLabelService As Endicia_LabelService.EwsLabelService
            '
            ewsLabelService = New Endicia_LabelService.EwsLabelService
            response = ewsLabelService.StatusRequest(request)
            '
            Dim xdoc As New Xml.XmlDocument
            xdoc.LoadXml(serializeShipResponse2string(response))
            _EndiciaWeb.Save_XMLfile(xdoc, "PackageStatus_response.xml")
            '
            If response.PackageStatus IsNot Nothing Then
                For i As Integer = 0 To response.PackageStatus.Length - 1
                    Dim packstat As Endicia_LabelService.StatusResponse = response.PackageStatus(i)
                    Dim track As New _EndiciaWeb.PICNumber
                    track.TrackingNo = packstat.PicNumber
                    If packstat.PackageStatusEventList IsNot Nothing Then
                        For p As Integer = 0 To packstat.PackageStatusEventList.Length - 1
                            Dim eventitem As Endicia_LabelService.StatusEventList = packstat.PackageStatusEventList(p)
                            track.Status = eventitem.StatusDescription
                            If eventitem.TrackingResults IsNot Nothing Then
                                For t As Integer = 0 To eventitem.TrackingResults.Length - 1
                                    Dim trackresult As Endicia_LabelService.TrackingResult = eventitem.TrackingResults(t)
                                    track.StatusBreakdown.Add(trackresult.Status)
                                Next t
                            End If

                        Next p
                    End If
                    tracks.Add(track)
                Next i
            End If
            '
            If response.ErrorMessage IsNot Nothing Then
                ' Error
                _MsgBox.WarningMessage(response.ErrorMessage, EndiciaLavelServer)
            Else
                response_PackageStatus = True
            End If
            '
        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to get a response to Package Status request...")
        End Try
    End Function

    Private Function serializeShipRequest2string(obj As Endicia_LabelService.PackageStatusRequest) As String
        Dim xml_serializer As New XmlSerializer(GetType(Endicia_LabelService.PackageStatusRequest))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeShipRequest2string = string_writer.ToString()
        string_writer.Close()
    End Function
    Private Function serializeShipResponse2string(obj As Endicia_LabelService.PackageStatusResponse) As String
        Dim xml_serializer As New XmlSerializer(GetType(Endicia_LabelService.PackageStatusResponse))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeShipResponse2string = string_writer.ToString()
        string_writer.Close()
    End Function

    Private Function load_setupdata(ByRef request As Endicia_LabelService.PackageStatusRequest) As Boolean
        If objEndiciaCredentials IsNot Nothing Then
            request.CertifiedIntermediary = New Endicia_LabelService.CertifiedIntermediary
            request.CertifiedIntermediary.AccountID = objEndiciaCredentials.AccountID
            request.CertifiedIntermediary.PassPhrase = objEndiciaCredentials.PassPhrase
            request.RequesterID = objEndiciaCredentials.RequesterID
            Return True
        Else
            _MsgBox.WarningMessage("Failed to load Endicia Account credentials!", "Can't Login!")
            Return False
        End If
    End Function

#End Region



#Region "Request: Print DYMO Stamp"
    Public Function Request_Stamp(ByVal obj As _EndiciaStamps, ByRef vb_response As baseWebResponse_Shipment) As Boolean
        Return Request_Stamp(objEndiciaCredentials, obj, vb_response)
    End Function
    Public Function Request_Stamp(ByVal SRSetup As _EndiciaSetup, ByVal obj As _EndiciaStamps, ByRef vb_response As baseWebResponse_Shipment) As Boolean
        Request_Stamp = False ' assume.
        Try
            Dim requests As New Endicia_DYMOStampService.StampsRequest
            With requests
                .RequesterID = SRSetup.RequesterID
                .AccountID = SRSetup.AccountID
                .PassPhrase = SRSetup.PassPhrase
                '
                If obj.TestPrint Then
                    .Test = "YES"
                Else
                    .Test = "NO"
                End If

                .ImageFormat = SRSetup.LabelImageType
                .MediaType = SRSetup.LabelType ' "30915" - DYMO Stamps® Postage Label Roll
                .ActivationCode = SRSetup.LabelSubtype ' Activation codes are available on the backings of every DYMO Stamps® Postage Label Roll. These activation codes are alphanumeric and can be up to 50 characters long.
                '
                .DateAdvance = CInt(_Date.Diff_Dates(obj.ShipDate, DateTime.Today))
                .ShipDate = String.Format("{0:MM/dd/yyyy}", obj.ShipDate)
                .ShipTime = String.Format("{0:hh:mm tt}", obj.ShipDate)
                '
                Dim request As New Endicia_DYMOStampService.StampRequest
                With request
                    .Count = obj.Quantity
                    .MailClass = sr2endicia_MailClass(obj.MailClass)
                    .WeightOz = obj.WeightOz
                    .MailpieceShape = obj.MailpieceShape
                    .UseUserRate = obj.UseUserRate.ToString
                    .UserRate = obj.UserRate ' Prints ‘zero dollar’ postage stamp
                    .ToCountryCode = obj.ToCountryCode
                End With

                .StampRequests = {request}
            End With

            Dim xdoc As New Xml.XmlDocument
            xdoc.LoadXml(serializeShipRequest2string(requests))
            hold_XMLdirpath = SRSetup.LabelFilePath
            _EndiciaWeb.Save_XMLfile(xdoc, "Stamp_request.xml")

            Request_Stamp = getResponse_Stamp(SRSetup, requests, vb_response)

        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to create Print a Stamp request...")
        End Try
    End Function
    Private Function getResponse_Stamp(ByVal SRSetup As Object, ByVal requests As Endicia_DYMOStampService.StampsRequest, ByRef vb_response As Object) As Boolean
        getResponse_Stamp = False
        Try
            System.Net.ServicePointManager.SecurityProtocol = Net.SecurityProtocolType.Tls12
            Dim webService As New Endicia_DYMOStampService.DYMOStampsService
            Dim responses As Endicia_DYMOStampService.StampsResponse = webService.GetStamps(requests)
            '
            Dim xdoc As New Xml.XmlDocument
            xdoc.LoadXml(serializeShipResponse2string(responses))
            _EndiciaWeb.Save_XMLfile(xdoc, "Stamp_response.xml")
            '
            getResponse_Stamp = (0 = responses.StatusPostage)
            If getResponse_Stamp Then
                ' success:
                ''AP(03/13/2020) - Added Endicia Stamps web service request to print DYMO/NetStamp stamps.
                Dim labelType As String = requests.ImageFormat
                Dim labelBase64 As String = String.Empty
                Dim labelPrinterName As String = SRSetup.LabelPrinterName ' printer checked if exists in vb6
                '
                ' Stamps:
                If responses.Base64LabelImage IsNot Nothing AndAlso Not String.IsNullOrEmpty(responses.Base64LabelImage) Then
                    '
                    Select Case labelType.ToUpper
                        '
                        Case "PNG"
                            '
                            If responses.ImageData IsNot Nothing AndAlso responses.ImageData.Length > 0 Then
                                For i As Integer = 0 To responses.ImageData.Length - 1
                                    labelBase64 = responses.ImageData(i) ' current base64 string in array
                                    If labelBase64 IsNot Nothing AndAlso Not String.IsNullOrEmpty(labelBase64) Then
                                        Dim respack As New baseWebResponse_Package
                                        respack.LabelImage = labelBase64 ' 
                                        vb_response.Packages.Add(respack) ' send image base64 strings back in response
                                        ' send to printer
                                        getResponse_Stamp = Print_StampFromImage(labelPrinterName, labelBase64)
                                    End If
                                Next
                            Else
                                labelBase64 = responses.Base64LabelImage
                                Dim respack As New baseWebResponse_Package
                                respack.LabelImage = labelBase64 ' 
                                vb_response.Packages.Add(respack) ' send image base64 string back in response
                                ' send to printer
                                getResponse_Stamp = Print_StampFromImage(labelPrinterName, labelBase64)
                            End If

                            '
                        Case "PDF"
                            '
                            ' only 1 pdf image returned
                            labelBase64 = responses.Base64LabelImage
                            Dim labelFile As String = SRSetup.LabelFilePath & "\" & "Stamp.pdf"
                            '
                            If _Files.WriteFile_ToEnd(_Convert.Base64String2Byte(labelBase64), labelFile) Then ' write to file
                                Dim labelString As String = String.Empty ' just a buffer, not used
                                If _Files.ReadFile_ToEnd(labelFile, False, labelString) Then ' read back to return
                                    Dim respack As New baseWebResponse_Package
                                    respack.LabelImage = labelString
                                    vb_response.Packages.Add(respack)
                                    ' open to print
                                    getResponse_Stamp = _Files.Run_File(labelFile, True)
                                End If
                            End If
                            '
                    End Select
                    '
                End If
                '
                vb_response.TotalCharges = responses.PostageBalance
                '
            Else
                '
                vb_response.ShipmentAlerts.Add(responses.ErrorMessagePostage)
                '
            End If
            '
        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to get Print a Stamp response...")
        End Try
    End Function

    Private Function serializeShipRequest2string(obj As Endicia_DYMOStampService.StampsRequest) As String
        Dim xml_serializer As New XmlSerializer(GetType(Endicia_DYMOStampService.StampsRequest))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeShipRequest2string = string_writer.ToString()
        string_writer.Close()
    End Function
    Private Function serializeShipResponse2string(obj As Endicia_DYMOStampService.StampsResponse) As String
        Dim xml_serializer As New XmlSerializer(GetType(Endicia_DYMOStampService.StampsResponse))
        Dim string_writer As New StringWriter
        xml_serializer.Serialize(string_writer, obj)
        serializeShipResponse2string = string_writer.ToString()
        string_writer.Close()
    End Function
    Public Sub set_SRSetupDYMO(ByRef SRSetupDYMO As _EndiciaSetup)
        SRSetupDYMO = New _EndiciaSetup
        With SRSetupDYMO

            'NetStamps and Labelserver need only to have one account. Hide option for separate NetStamp Account.
            '.AccountID = GetPolicyData(gShipriteDB, "Endicia_AccountID2", "False")
            '.PassPhrase = GetPolicyData(gShipriteDB, "Endicia_PassPhrase2", "False")

            'use same as labelserver
            .AccountID = GetPolicyData(gShipriteDB, _ReusedField.fldAccountID)
            .PassPhrase = GetPolicyData(gShipriteDB, _ReusedField.fldPassPhrase)
            .PartnerCustomerID = "shiprite"


            .LabelImageType = "PNG" '"PDF" '"PNG" 
            .LabelType = "30915" 'DYMO Roll; 356-2 - DYMO Sheets;

            ' Activation codes are available on the backings of every DYMO Stamps® Postage Label Roll. These activation codes are alphanumeric and can be up to 50 characters long.
            .LabelSubtype = GetPolicyData(gReportsDB, "DYMO_ActivationCode", "") ' Activation Code validates that genuine DYMO labels are used to print stamps and ensures that no postage is rejected by USPS due to printing stamps on counterfeit labels.

            .LabelSize = "DYMO Roll"
            .LabelPrinterName = GetPolicyData(gReportsDB, "DYMOLabelPrinter", "")

        End With
    End Sub


#End Region





End Module

Public Class _EndiciaSetup
    Public AccountID As String '  "123456"
    Public PassPhrase As String '  "samplePassPhrase"
    Public PartnerCustomerID As String '  "12345ABCD"
    Public PartnerTransactionID As String '  "6789EFGH"
    Public RequesterID As String ' "abcd"
    '
    Public Test As String ' "YES"
    Public LabelType As String ' "Domestic", "International"
    Public LabelSubtype As String ' "Integrated", "None"
    Public LabelSize As String ' "4X6"
    '
    Public LabelFilePath As String
    Public LabelImageType As String
    '
    Public DbPath As String ' c:\ShipRite\Shiprite.mdb
    Public LabelPrinterName As String ''AP(03/13/2020) - Added Endicia Stamps web service request to print DYMO/NetStamp stamps.

    Public Sub New()
        AccountID = GetPolicyData(gShipriteDB, _ReusedField.fldAccountID)
        PassPhrase = GetPolicyData(gShipriteDB, _ReusedField.fldPassPhrase)
        PartnerCustomerID = GetPolicyData(gShipriteDB, “Name”)

        PartnerTransactionID = "6789EFGH"
        RequesterID = "lsrp"
        '
        LabelImageType = GetPolicyData(gReportsDB, "EndiciaLabelType", "Zebra Thermal")
        'LabelImageType = "Zebra Thermal"

        LabelFilePath = String.Format("{0}\Endicia\InOut", gDBpath)
    End Sub

    Public ReadOnly Property IsEnabled
        Get
            Return Not String.IsNullOrEmpty(AccountID) AndAlso Not String.IsNullOrEmpty(PassPhrase)
        End Get
    End Property
End Class
Public Class _EndiciaStamps
    Public MailClass As String
    Public MailpieceShape As String
    Public Quantity As Integer
    Public TestPrint As Boolean
    Public WeightOz As Double
    Public ShipDate As Date
    Public ToCountryCode As String
    Public UseUserRate As Boolean
    Public UserRate As Double
End Class

