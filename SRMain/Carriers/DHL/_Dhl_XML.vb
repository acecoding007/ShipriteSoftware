Imports System.Xml.Serialization
Imports System.IO
Imports System.Text
Imports System.Net
'
Public Class DHL_Setup

    Public ShipperID As String
    Public SiteID As String
    Public Password As String
    Public SoftwareName As String
    Public SoftwareVersion As String
    '
    Public TaxIds As List(Of DHL_RegistrationNumber)
    '
    Public ShipperAccountNumber As String
    Public ShippingPaymentType As Integer
    Public DutyPaymentType As Integer
    Public Const Sender As Integer = 0
    Public Const Recipient As Integer = 1
    Public Const ThirdParty As Integer = 2
    '
    Public TermsOfTrade As Integer
    Public GlobalProductCode As String
    Public LabelImageFormat As String
    Public Path_SaveDocXML As String
    Public URL As String
    Public Envelope_Weight_Limit_Lbs As Double

    Sub New()

        If My.Settings.DHL_XML_IsTest Then
            ' Test:
            SiteID = "v62_1WIR6POsx3"
            Password = "9MwqMZIBPL"
            URL = My.Settings.DHL_XML_Test_Url '"https://xmlpitest-ea.dhl.com/XMLShippingServlet"
        Else
            ' Production:
            SiteID = "ShipRite"
            Password = "9Klpp09t55E3e"
            URL = My.Settings.DHL_XML_Production_Url '"https://xmlpi-ea.dhl.com/XMLShippingServlet"
        End If
        '
        SoftwareName = "ShipriteNext"
        SoftwareVersion = "1.0.0"
        Envelope_Weight_Limit_Lbs = 0.625
        ShipperAccountNumber = General.GetPolicyData(gShipriteDB, _ReusedField.fldDHL_ShipperID)
        ShipperID = ShipperAccountNumber
        ShippingPaymentType = ThirdParty ' Sender
        DutyPaymentType = Recipient
        TermsOfTrade = DHL_ShipReq.TermsOfTrade.DDU
        GlobalProductCode = "P" ' default
        LabelImageFormat = "EPL2" ' "EPL2" ' "PDF" ' "ZPL2"
        Path_SaveDocXML = String.Format("{0}\DHL\InOut", gDBpath)

        'TaxIds = New List(Of DHL_RegistrationNumber) From {
        '    New DHL_RegistrationNumber With {.Number = "1234567890", .NumberIssuerCountryCode = "US", .NumberTypeCode = DHL_NumberTypeCode.FED},
        '    New DHL_RegistrationNumber With {.Number = "0987654321", .NumberIssuerCountryCode = "US", .NumberTypeCode = DHL_NumberTypeCode.STA}
        '}
    End Sub

End Class

Public Class DHL_RegistrationNumber
    Public Property Number As String
    Public Property NumberIssuerCountryCode As String
    Public Property NumberTypeCode As DHL_NumberTypeCode
End Class

Public Enum DHL_NumberTypeCode

    '''<remarks/>
    SDT

    '''<remarks/>
    VAT

    '''<remarks/>
    FTZ

    '''<remarks/>
    DAN

    '''<remarks/>
    TAN

    '''<remarks/>
    DTF

    '''<remarks/>
    CNP

    '''<remarks/>
    DUN

    '''<remarks/>
    EIN

    '''<remarks/>
    EOR

    '''<remarks/>
    SSN

    '''<remarks/>
    FED

    '''<remarks/>
    STA

    '''<remarks/>
    RGP

    '''<remarks/>
    DLI

    '''<remarks/>
    NID

    '''<remarks/>
    PAS

    '''<remarks/>
    MID
End Enum

Public Module _Dhl_XML

    Public objDHL_Setup As DHL_Setup
    Public IsEmail_DHL_ShipNotification As Boolean

#Region "Get Available Service"
    Public Function GetAvailableService_Request(ByVal setup As DHL_Setup, ByVal shipment As _baseShipment, ByRef vb_response As baseWebResponse_TinT_Services) As Boolean
        GetAvailableService_Request = False ' assume.
        Try
            Dim shiprequest As New DHL_DCTReq.DCTRequest
            Dim getCapability As New DHL_DCTReq.DCTRequestGetCapability
            Dim request As New DHL_DCTReq.Request
            '
            With request
                Dim serviceheader As New DHL_DCTReq.ServiceHeader With {
                    .MessageTime = DateTime.Now,
                    .MessageReference = DateTime.Now.ToString("yyyyMMddHHmmssfyyyyMMddHHmmssf"),
                    .SiteID = setup.SiteID,
                    .Password = setup.Password
                }
                .ServiceHeader = serviceheader
            End With
            getCapability.Request = request
            '
            Dim dtcFrom As New DHL_DCTReq.DCTFrom
            Dim shipper As _baseContact = shipment.ShipperContact
            With dtcFrom
                .City = shipper.City
                .Postalcode = shipper.Zip
                .CountryCode = shipper.CountryCode
            End With
            getCapability.From = dtcFrom
            '
            Dim dtcTo As New DHL_DCTReq.DCTTo
            Dim shipTo As _baseContact = shipment.ShipToContact
            With dtcTo
                .City = shipTo.City
                .Postalcode = shipTo.Zip
                .CountryCode = shipTo.CountryCode
            End With
            getCapability.To = dtcTo
            '
            Dim package As _baseShipmentPackage = shipment.Packages(0)
            Dim dutiable As New DHL_DCTReq.DCTDutiable
            With dutiable
                .DeclaredValue = package.DeclaredValue
                .DeclaredValueSpecified = True
                .DeclaredCurrency = package.Currency_Type
            End With
            getCapability.Dutiable = dutiable
            '
            Dim service As _baseCarrierService = shipment.CarrierService
            Dim bkg As New DHL_DCTReq.BkgDetailsType
            With bkg
                .PaymentCountryCode = shipper.CountryCode
                .PaymentAccountNumber = setup.ShipperAccountNumber
                .Date = service.ShipDate
                .ReadyTime = String.Format("PT{0:hh}H{0:mm}M", DateTime.Now)
                If package.Dim_Units = "IN" Then
                    .DimensionUnit = DHL_DCTReq.BkgDetailsTypeDimensionUnit.IN
                Else
                    .DimensionUnit = DHL_DCTReq.BkgDetailsTypeDimensionUnit.CM
                End If
                If package.Weight_Units = "LB" Then
                    .WeightUnit = DHL_DCTReq.BkgDetailsTypeWeightUnit.LB
                Else
                    .WeightUnit = DHL_DCTReq.BkgDetailsTypeWeightUnit.KG
                End If
                .NetworkTypeCode = DHL_DCTReq.BkgDetailsTypeNetworkTypeCode.AL
                .NetworkTypeCodeSpecified = True
                If (package.IsLetter And package.Weight_LBs < setup.Envelope_Weight_Limit_Lbs) Or shipment.CarrierService.ServiceABBR = "DHL-INT-DOC" Or shipment.CommInvoice.TypeOfContents.ToUpper = "DOCUMENTS" Then
                    .IsDutiable = DHL_DCTReq.BkgDetailsTypeIsDutiable.N
                Else
                    .IsDutiable = DHL_DCTReq.BkgDetailsTypeIsDutiable.Y
                End If
                .IsDutiableSpecified = True
                .NumberOfPieces = shipment.Packages.Count
                Dim pieces(shipment.Packages.Count - 1) As DHL_DCTReq.PieceType
                For p As Integer = 0 To shipment.Packages.Count - 1
                    Dim pack As _baseShipmentPackage = shipment.Packages(p)
                    Dim piece As New DHL_DCTReq.PieceType
                    With piece
                        .PieceID = p + 1
                        If Not pack.IsLetter Then ' letter dims = 0
                            .Depth = Math.Round(pack.Dim_Length, 3)
                            .DepthSpecified = True
                            .Height = Math.Round(pack.Dim_Height, 3)
                            .HeightSpecified = True
                            .Width = Math.Round(pack.Dim_Width, 3)
                            .WidthSpecified = True
                        End If
                        .Weight = Math.Round(pack.Weight_LBs, 3)
                    End With
                    pieces(p) = piece
                Next p
                .Pieces = pieces
            End With
            getCapability.BkgDetails = bkg
            shiprequest.Item = getCapability
            '
            Dim xdoc As New Xml.XmlDocument
            xdoc.LoadXml(serializeDCTRequest2string(shiprequest))
            xdoc.Save(setup.Path_SaveDocXML & "\DCTRequest.xml") ' shipment ID
            Dim text As String = String.Empty
            If _Files.ReadFile_ToEnd(setup.Path_SaveDocXML & "\DCTRequest.xml", False, text) Then
                '
                Dim byteArray As Byte() = Encoding.UTF8.GetBytes(text)
                '
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12
                '
                Dim xml_request As WebRequest = WebRequest.Create(setup.URL)
                With xml_request
                    .Method = "POST"
                    .ContentType = "application/xml"
                    .ContentLength = byteArray.Length
                End With
                Using dataStream As Stream = xml_request.GetRequestStream()
                    dataStream.Write(byteArray, 0, byteArray.Length)
                End Using
                '
                If process_GetAvailableService_Response(setup, xml_request, vb_response) Then
                    Return True
                End If
            End If
            '
        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to create a 'Get Available Service' request...")
        End Try
    End Function
    Private Function process_GetAvailableService_Response(ByVal setup As DHL_Setup, ByVal xml_request As WebRequest, ByRef vb_response As baseWebResponse_TinT_Services) As Boolean
        process_GetAvailableService_Response = False ' assume.
        Try
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12
            '
            Dim text As String = String.Empty
            '
            Using xml_response As WebResponse = xml_request.GetResponse()
                Using dataStream As Stream = xml_response.GetResponseStream()
                    Using reader As New StreamReader(dataStream)
                        text = reader.ReadToEnd()
                    End Using
                End Using
            End Using
            '
            _Files.WriteFile_ByOneString(text, setup.Path_SaveDocXML & "\DCTResponse.xml", False)
            '_Debug.Print_(text)
            '
            If _Controls.Contains(text, "ErrorResponse") Then
                ' Error:
                Dim errorres As DHL_ErrorRes.ErrorResponse = deserializeDCTResponseError2object(text)
                If errorres.Response IsNot Nothing AndAlso errorres.Response.Status IsNot Nothing AndAlso errorres.Response.Status.Condition IsNot Nothing Then
                    For r As Integer = 0 To errorres.Response.Status.Condition.Length - 1
                        Dim cond As DHL_ErrorRes.Condition = errorres.Response.Status.Condition(r)
                        vb_response.TimeInTransitAlerts.Add(cond.ConditionData)
                    Next r
                End If
                '
            ElseIf _Controls.Contains(text, "DCTResponse") Then
                ' Success:
                Dim response As DHL_DCTRes.DCTResponse = deserializeDCTResponse2object(text)
                If response IsNot Nothing AndAlso response.Item IsNot Nothing Then
                    Dim getCapability As DHL_DCTRes.DCTResponseGetCapabilityResponse = response.Item
                    If getCapability IsNot Nothing Then
                        If getCapability.BkgDetails IsNot Nothing Then
                            For i As Integer = 0 To getCapability.BkgDetails.Length - 1
                                Dim bkg As DHL_DCTRes.QtdShpType = getCapability.BkgDetails(i)
                                Dim pack As New baseWebResponse_TinT_Service
                                With bkg
                                    pack.ServiceCode = .GlobalProductCode
                                    pack.ServiceDesc = .ProductShortName
                                    Dim deliverdate As DateTime
                                    If _Date.Add_Day(.TotalTransitDays, .PickupDate, deliverdate) Then
                                        pack.ArrivalDate = deliverdate
                                    End If
                                    pack.ArrivalTransitTime = .NetworkTypeCode
                                    pack.ArrivalDayOfWeek = .DestinationDayOfWeekNum
                                    pack.TotalBaseCharge = .ShippingCharge
                                    pack.IsServiceAvailable = True
                                End With
                                vb_response.AvailableServices.Add(pack)
                            Next i
                        End If
                    End If
                End If
                '
            End If
            '
            Return True
        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message) : _Debug.Print_(ex.Detail.LastChild.InnerText)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to get a response for Get Available Service request...")
        End Try
    End Function


    Private Function serializeDCTRequest2string(obj As DHL_DCTReq.DCTRequest) As String
        Dim xml_serializer As New XmlSerializer(GetType(DHL_DCTReq.DCTRequest))
        Dim string_writer As New Utf8StringWriter ' use UTF-8 instead of default UTF-16
        xml_serializer.Serialize(string_writer, obj)
        serializeDCTRequest2string = string_writer.ToString()
        string_writer.Close()
    End Function
    Private Function deserializeDCTResponseError2object(xmlsting As String) As DHL_ErrorRes.ErrorResponse
        Dim xml_serializer As New XmlSerializer(GetType(DHL_ErrorRes.ErrorResponse))
        Dim string_reader As New StringReader(xmlsting)
        deserializeDCTResponseError2object = DirectCast(xml_serializer.Deserialize(string_reader), DHL_ErrorRes.ErrorResponse)
        string_reader.Close()
    End Function
    Private Function deserializeDCTResponse2object(xmlsting As String) As DHL_DCTRes.DCTResponse
        Dim xml_serializer As New XmlSerializer(GetType(DHL_DCTRes.DCTResponse))
        Dim string_reader As New StringReader(xmlsting)
        deserializeDCTResponse2object = DirectCast(xml_serializer.Deserialize(string_reader), DHL_DCTRes.DCTResponse)
        string_reader.Close()
    End Function

#End Region

#Region "Ship Package"
    Public Function ShipAPackage(ByVal setup As DHL_Setup, ByVal shipment As _baseShipment, ByRef vb_response As baseWebResponse_Shipment) As Boolean
        ShipAPackage = False ' assume.
        Try

            Dim shiprequest As New DHL_ShipReq.ShipmentRequest
            Dim request As New DHL_ShipReq.Request
            '
            With request
                Dim serviceheader As New DHL_ShipReq.ServiceHeader With {
                    .MessageTime = DateTime.Now, 'DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:sszzz")
                    .MessageReference = DateTime.Now.ToString("yyyyMMddHHmmssfyyyyMMddHHmmssf"),
                    .SiteID = setup.SiteID,
                    .Password = setup.Password
                }
                .ServiceHeader = serviceheader
                '
                Dim metadata As New DHL_ShipReq.MetaData With {
                    .SoftwareName = setup.SoftwareName,
                    .SoftwareVersion = setup.SoftwareVersion
                }
                .MetaData = metadata
            End With
            shiprequest.Request = request
            '
            shiprequest.RequestedPickupTime = DHL_ShipReq.YesNo.Y
            shiprequest.LanguageCode = "en"
            '
            Dim billing As New DHL_ShipReq.Billing
            With billing
                .ShipperAccountNumber = setup.ShipperAccountNumber
                .ShippingPaymentType = setup.ShippingPaymentType
                'If Not setup.ShippingPaymentType = DHL_Setup.Sender Then ' required if ShippingPaymentType is other than sender 'S'
                '    .BillingAccountNumber = setup.ShipperAccountNumber
                'End If
                .BillingAccountNumber = setup.ShipperAccountNumber
            End With
            shiprequest.Billing = billing
            '
            Dim shipper As New DHL_ShipReq.Shipper
            Dim shipFrom As _baseContact = shipment.ShipFromContact
            With shipper
                .ShipperID = setup.ShipperID
                .RegisteredAccount = setup.ShipperAccountNumber
                .CompanyName = shipFrom.CompanyName
                .AddressLine1 = shipFrom.Addr1
                If Not String.IsNullOrWhiteSpace(shipFrom.Addr2) Then
                    .AddressLine2 = shipFrom.Addr2
                End If
                .City = shipFrom.City
                .PostalCode = shipFrom.Zip
                .DivisionCode = shipFrom.State
                .CountryCode = shipFrom.CountryCode
                .CountryName = shipFrom.Country
                Dim contact_sh As New DHL_ShipReq.Contact
                With contact_sh
                    .PersonName = shipFrom.FNameLName
                    .PhoneNumber = shipFrom.Tel
                    If Not String.IsNullOrWhiteSpace(shipFrom.Email) Then
                        .Email = shipFrom.Email
                    End If
                End With
                .Contact = contact_sh
                '
                If setup.TaxIds IsNot Nothing AndAlso setup.TaxIds.Count > 0 Then
                    Dim registrationNums(setup.TaxIds.Count - 1) As DHL_ShipReq.RegistrationNumber
                    For i As Integer = 0 To setup.TaxIds.Count - 1
                        registrationNums(i) = New DHL_ShipReq.RegistrationNumber With {
                            .Number = setup.TaxIds(i).Number,
                            .NumberIssuerCountryCode = setup.TaxIds(i).NumberIssuerCountryCode,
                            .NumberTypeCode = Convert_TaxNumberTypeCode2NumberTypeCode(setup.TaxIds(i).NumberTypeCode)
                        }
                    Next
                    .RegistrationNumbers = registrationNums
                End If
            End With
            shiprequest.Shipper = shipper
            '
            Dim consignee As New DHL_ShipReq.Consignee
            Dim shipTo As _baseContact = shipment.ShipToContact
            With consignee
                .CompanyName = shipTo.CompanyName
                .AddressLine1 = shipTo.Addr1
                If Not String.IsNullOrWhiteSpace(shipTo.Addr2) Then
                    .AddressLine2 = shipTo.Addr2
                End If

                If Not String.IsNullOrWhiteSpace(shipTo.Addr3) Then
                    .AddressLine3 = shipTo.Addr3
                End If

                .City = shipTo.City
                .PostalCode = shipTo.Zip
                .DivisionCode = shipTo.State
                .CountryCode = shipTo.CountryCode
                .CountryName = shipTo.Country
                Dim contact_co As New DHL_ShipReq.Contact
                With contact_co
                    .PersonName = shipTo.FNameLName
                    .PhoneNumber = shipTo.Tel
                    If Not String.IsNullOrWhiteSpace(shipTo.Email) Then
                        .Email = shipTo.Email
                    End If
                End With
                .Contact = contact_co
            End With
            shiprequest.Consignee = consignee
            '
            Dim emailNotification As New DHL_ShipReq.Notification()
            If Not String.IsNullOrWhiteSpace(shiprequest.Shipper.Contact.Email) Then
                emailNotification.EmailAddress = shiprequest.Shipper.Contact.Email
            End If
            If Not String.IsNullOrWhiteSpace(shiprequest.Consignee.Contact.Email) Then
                If Not String.IsNullOrWhiteSpace(emailNotification.EmailAddress) Then
                    emailNotification.EmailAddress &= ";" ' For multiple email address, it can be separated by semicolon ‘;’
                End If
                emailNotification.EmailAddress &= shiprequest.Consignee.Contact.Email
            End If
            If Not String.IsNullOrWhiteSpace(emailNotification.EmailAddress) Then
                shiprequest.Notification = emailNotification
            End If
            '
            Dim package As _baseShipmentPackage = shipment.Packages(0)
            Dim reference As New DHL_ShipReq.Reference With {
                .ReferenceID = package.PackageID
            }
            shiprequest.Reference = {reference}
            '
            If shipment.CommInvoice IsNot Nothing Then
                '
                Dim comminvoice As _baseCommInvoice = shipment.CommInvoice
                'Dim commodities(comminvoice.CommoditiesList.Count - 1) As DHL_ShipReq.Commodity
                'For i As Integer = 0 To comminvoice.CommoditiesList.Count - 1
                '    Dim commodity As New DHL_ShipReq.Commodity
                '    Dim commitem As _baseCommodities = comminvoice.CommoditiesList(i)
                '    If String.IsNullOrEmpty(commitem.Item_Code) Then
                '        commodity.CommodityCode = (i + 1).ToString
                '    Else
                '        commodity.CommodityCode = commitem.Item_Code
                '    End If
                '    commodity.CommodityName = commitem.Item_Description
                '    commodities(i) = commodity
                'Next i
                'shiprequest.Commodity = commodities
                '
                If 0 < comminvoice.CommoditiesList.Count Then
                    '
                    Dim exportDeclare As New DHL_ShipReq.ExportDeclaration
                    '
                    Dim exportLineItems(comminvoice.CommoditiesList.Count - 1) As DHL_ShipReq.ExportLineItem
                    For i As Integer = 0 To comminvoice.CommoditiesList.Count - 1
                        Dim exportLineItem As New DHL_ShipReq.ExportLineItem
                        Dim commItem As _baseCommodities = comminvoice.CommoditiesList(i)
                        exportLineItem.LineNumber = (i + 1).ToString
                        exportLineItem.Quantity = commItem.Item_Quantity.ToString
                        exportLineItem.QuantityUnit = DHL_ShipReq.QuantityUnit.PCS
                        exportLineItem.Description = commItem.Item_Description
                        exportLineItem.Value = Math.Round(commItem.Item_UnitPrice, 3)
                        If Not String.IsNullOrEmpty(commItem.Item_Code) Then
                            exportLineItem.CommodityCode = commItem.Item_Code
                        End If
                        Dim exportLineItemWeight As New DHL_ShipReq.ExportLineItemWeight
                        Dim exportLineItemGrossWeight As New DHL_ShipReq.ExportLineItemGrossWeight
                        exportLineItemWeight.Weight = Math.Round(commItem.Item_Weight, 3)
                        exportLineItemGrossWeight.Weight = Math.Round(commItem.Item_Weight, 3)
                        If package.Weight_Units = "LB" Then
                            exportLineItemWeight.WeightUnit = DHL_ShipReq.WeightUnit.L
                            exportLineItemGrossWeight.WeightUnit = DHL_ShipReq.WeightUnit.L
                        Else
                            exportLineItemWeight.WeightUnit = DHL_ShipReq.WeightUnit.K
                            exportLineItemGrossWeight.WeightUnit = DHL_ShipReq.WeightUnit.K
                        End If
                        exportLineItem.Weight = exportLineItemWeight
                        exportLineItem.GrossWeight = exportLineItemGrossWeight
                        exportLineItem.ManufactureCountryCode = commItem.Item_CountryOfOrigin ' should be countrycode
                        exportLineItems(i) = exportLineItem
                    Next
                    exportDeclare.ExportLineItem = exportLineItems
                    '
                    If shipment.CommInvoice.InvoiceNo IsNot Nothing AndAlso shipment.CommInvoice.InvoiceNo.Length > 0 Then
                        exportDeclare.InvoiceNumber = shipment.CommInvoice.InvoiceNo
                    Else
                        exportDeclare.InvoiceNumber = reference.ReferenceID ' packageid
                    End If
                    exportDeclare.InvoiceDate = shipment.CarrierService.ShipDate
                    shiprequest.ExportDeclaration = exportDeclare
                    '
                    Dim duty As New DHL_ShipReq.Dutiable
                    With duty
                        .DeclaredValue = comminvoice.CommoditiesTotalValue
                        .DeclaredValueSpecified = True
                        .DeclaredCurrency = comminvoice.CurrencyType
                        .TermsOfTrade = setup.TermsOfTrade
                        Dim dutyFiling As New DHL_ShipReq.Filing
                        If shiprequest.Shipper.CountryCode = "US" And shiprequest.Consignee.CountryCode = "CA" Then
                            dutyFiling.FilingType = DHL_ShipReq.FilingType.FTR
                            dutyFiling.FilingTypeSpecified = True
                            dutyFiling.FTSR = DHL_ShipReq.FTSR.Item3036
                            dutyFiling.FTSRSpecified = True
                        Else
                            dutyFiling.FilingType = DHL_ShipReq.FilingType.FTR
                            dutyFiling.FilingTypeSpecified = True
                            dutyFiling.FTSR = DHL_ShipReq.FTSR.Item3037a
                            dutyFiling.FTSRSpecified = True
                        End If
                        .Filing = dutyFiling
                    End With
                    shiprequest.Dutiable = duty
                    '
                End If
                '
                If String.IsNullOrEmpty(shipment.Comments) Then
                    shipment.Comments = comminvoice.TypeOfContents
                End If
            End If
            '
            Dim shipdetails As New DHL_ShipReq.ShipmentDetails
            With shipdetails
                Dim pieces(shipment.Packages.Count - 1) As DHL_ShipReq.Piece
                For p As Integer = 0 To shipment.Packages.Count - 1
                    Dim pack As _baseShipmentPackage = shipment.Packages(p)
                    Dim piece As New DHL_ShipReq.Piece
                    With piece
                        .PieceID = pack.PackageID
                        If pack.IsLetter Then
                            ' v10.0.4: Letter/Envelope: Dims cannot exceed 0.5 in. but Dims elements require integers.
                            ' v10.0.4: Workaround: Set units to metric because 1 cm. = 0.393701 in. which is < 0.5 in.
                            ' v10.0.6: The Dims elements now allow decimals including 0 for the dims fields. Don't need above workaround anymore.
                            .Depth = 0.1
                            .Height = 0.1
                            .Width = 0.1
                            .Weight = Math.Round(pack.Weight_LBs, 3) '_Convert.Lb2Kg(pack.Weight_LBs, 3)
                            'package.Weight_Units = "KG"
                        Else
                            .Depth = Math.Round(pack.Dim_Length, 3)
                            .Height = Math.Round(pack.Dim_Height, 3)
                            .Width = Math.Round(pack.Dim_Width, 3)
                            .Weight = Math.Round(pack.Weight_LBs, 3)
                        End If
                    End With
                    pieces(p) = piece
                Next p
                .Pieces = pieces
                .GlobalProductCode = setup.GlobalProductCode
                .Date = shipment.CarrierService.ShipDate
                .Contents = shipment.Comments
                Dim weightUnit As New DHL_ShipReq.WeightUnit
                If package.Weight_Units = "LB" Then
                    .WeightUnit = DHL_ShipReq.WeightUnit.L
                    .DimensionUnit = DHL_ShipReq.DimensionUnit.I
                Else
                    .WeightUnit = DHL_ShipReq.WeightUnit.K          ' KG
                    .DimensionUnit = DHL_ShipReq.DimensionUnit.C    ' CM
                End If
                .DimensionUnitSpecified = True
                .CurrencyCode = package.Currency_Type
                '
                If .GlobalProductCode = "X" Then
                    .PackageType = DHL_ShipReq.PackageType.EE
                    .IsDutiable = DHL_ShipReq.YesNo.N
                ElseIf .GlobalProductCode = "D" Then
                    .PackageType = DHL_ShipReq.PackageType.CP
                    .IsDutiable = DHL_ShipReq.YesNo.N
                Else ' .GlobalProductCode = "P"
                    .PackageType = DHL_ShipReq.PackageType.CP
                    .IsDutiable = DHL_ShipReq.YesNo.Y
                End If
                .PackageTypeSpecified = True
                .IsDutiableSpecified = True
                '
            End With
            shiprequest.ShipmentDetails = shipdetails
            '
            If shipdetails.IsDutiable = DHL_ShipReq.YesNo.Y And package.DeclaredValue > 0 Then
                shipment.CarrierService.ServiceSurcharges.Add(New _baseServiceSurcharge(0, "SHIPMENT_INSURANCE", package.Currency_Type, True, package.DeclaredValue))
            End If
            '
            shiprequest.SpecialService = Create_SpecialServices(shipment)
            '
            'shiprequest.UseDHLInvoice = DHL_ShipReq.YesNo.N ' optional
            '
            If setup.LabelImageFormat = "PDF" Then ' setup.LabelImageFormatType = DHL_LabelType.PDF
                shiprequest.LabelImageFormat = DHL_ShipReq.LabelImageFormat.PDF
                shiprequest.Label = New DHL_ShipReq.Label With {
                    .LabelTemplate = DHL_ShipReq.LabelTemplate.Item6X4_PDF,
                    .LabelTemplateSpecified = True
                }
            ElseIf setup.LabelImageFormat = "ZPL2" Then ' setup.LabelImageFormatType = DHL_LabelType.ZPL2
                shiprequest.LabelImageFormat = DHL_ShipReq.LabelImageFormat.ZPL2
                shiprequest.Label = New DHL_ShipReq.Label With {
                    .LabelTemplate = DHL_ShipReq.LabelTemplate.Item6X4_thermal,
                    .LabelTemplateSpecified = True
                }
            Else ' EPL2
                shiprequest.LabelImageFormat = DHL_ShipReq.LabelImageFormat.EPL2
                shiprequest.Label = New DHL_ShipReq.Label With {
                    .LabelTemplate = DHL_ShipReq.LabelTemplate.Item6X4_thermal,
                    .LabelTemplateSpecified = True
                }
            End If
            shiprequest.LabelImageFormatSpecified = True
            shiprequest.EProcShip = DHL_ShipReq.YesNo.N ' Default 'N'
            shiprequest.EProcShipSpecified = True
            '
            Dim xdoc As New Xml.XmlDocument
            xdoc.LoadXml(serializeShipRequest2string(shiprequest))
            xdoc.Save(setup.Path_SaveDocXML & "\" & package.PackageID & "_RequestShipment.xml") ' shipment ID
            Dim text As String = String.Empty
            If _Files.ReadFile_ToEnd(setup.Path_SaveDocXML & "\" & package.PackageID & "_RequestShipment.xml", False, text) Then
                '
                Dim byteArray As Byte() = Encoding.UTF8.GetBytes(text)
                '
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12
                '
                Dim xml_request As WebRequest = WebRequest.Create(setup.URL)
                With xml_request
                    .Method = "POST"
                    .ContentType = "application/xml"
                    .ContentLength = byteArray.Length
                End With
                Using dataStream As Stream = xml_request.GetRequestStream()
                    dataStream.Write(byteArray, 0, byteArray.Length)
                End Using
                '
                If process_ShipAPackage_Response(setup, xml_request, vb_response) Then
                    Return True
                End If
            End If

        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to create a 'Ship A Package' request...")
        End Try
    End Function
    Private Function process_ShipAPackage_Response(ByVal setup As DHL_Setup, ByVal xml_request As WebRequest, ByRef vb_response As baseWebResponse_Shipment) As Boolean
        process_ShipAPackage_Response = False ' assume.
        Try
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12
            '
            Dim text As String = String.Empty
            '
            Using xml_response As WebResponse = xml_request.GetResponse()
                Using dataStream As Stream = xml_response.GetResponseStream()
                    Using reader As New StreamReader(dataStream)
                        text = reader.ReadToEnd()
                    End Using
                End Using
            End Using
            '
            _Files.WriteFile_ByOneString(text, setup.Path_SaveDocXML & "\" & vb_response.Packages(0).PackageID & "_ReplyShipment.xml", False)
            '_Debug.Print_(text)
            '
            If _Controls.Contains(text, "ShipmentValidateErrorResponse") Then
                ' Error:
                Dim errorres As DHL_ShipErrorRes.ShipmentValidateErrorResponse = deserializeShipResponseError2object(text)
                If errorres.Response IsNot Nothing AndAlso errorres.Response.Status IsNot Nothing AndAlso errorres.Response.Status.Condition IsNot Nothing Then
                    For r As Integer = 0 To errorres.Response.Status.Condition.Length - 1
                        Dim cond As DHL_ShipErrorRes.Condition = errorres.Response.Status.Condition(r)
                        vb_response.ShipmentAlerts.Add(cond.ConditionData)
                    Next r
                End If
                '
            ElseIf _Controls.Contains(text, "ErrorResponse") Then
                ' Error:
                Dim errorres As DHL_ErrorRes.ErrorResponse = deserializeDCTResponseError2object(text)
                If errorres.Response IsNot Nothing AndAlso errorres.Response.Status IsNot Nothing AndAlso errorres.Response.Status.Condition IsNot Nothing Then
                    For r As Integer = 0 To errorres.Response.Status.Condition.Length - 1
                        Dim cond As DHL_ErrorRes.Condition = errorres.Response.Status.Condition(r)
                        vb_response.ShipmentAlerts.Add(cond.ConditionData)
                    Next r
                End If
                '
            ElseIf _Controls.Contains(text, "ShipmentResponse") Then
                ' Success:
                Dim response As DHL_ShipRes.ShipmentResponse = deserializeShipResponse2object(text)
                If response IsNot Nothing Then
                    '
                    If response.Note IsNot Nothing AndAlso response.Note.Condition IsNot Nothing Then
                        For r As Integer = 0 To response.Note.Condition.Length - 1
                            Dim cond As DHL_ShipRes.Condition = response.Note.Condition(r)
                            vb_response.ShipmentAlerts.Add(cond.ConditionData)
                        Next r
                    End If
                    '
                    vb_response.TotalCharges = response.ShippingCharge ' total shipping charge of shipment including surcharges
                    vb_response.TransportationCharges = response.PackageCharge ' charge for package delivery
                    '
                    Dim pack As baseWebResponse_Package = vb_response.Packages(0)
                    pack.TrackingNo = response.AirwayBillNumber
                    '
                    If response.LabelImage IsNot Nothing Then
                        For i As Integer = 0 To response.LabelImage.Length - 1
                            Dim label As DHL_ShipRes.LabelImage = response.LabelImage(i)
                            If label IsNot Nothing Then
                                '
                                Dim labelString As String = String.Empty
                                Dim labelFileExt As String = setup.LabelImageFormat
                                Dim labelFile As String = setup.Path_SaveDocXML & "\" & vb_response.Packages(0).PackageID & "_label." & labelFileExt
                                '
                                If Not IsNothing(label.OutputImage) Then
                                    If _Files.WriteFile_ToEnd(label.OutputImage, labelFile) Then
                                        If _Files.ReadFile_ToEnd(labelFile, False, labelString) Then
                                            vb_response.Packages(i).LabelImage = labelString
                                        End If
                                    End If
                                End If
                                '
                            End If
                        Next i
                    End If
                    '
                End If
                '
            End If
            '
            Return True
        Catch ex As System.Web.Services.Protocols.SoapException : _MsgBox.ErrorMessage(ex.Detail.LastChild.InnerText, ex.Message) : _Debug.Print_(ex.Detail.LastChild.InnerText)
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to get a response for Ship-A-Package request...")
        End Try
    End Function


    Private Function serializeShipRequest2string(obj As DHL_ShipReq.ShipmentRequest) As String
        Dim xml_serializer As New XmlSerializer(GetType(DHL_ShipReq.ShipmentRequest))
        Dim string_writer As New Utf8StringWriter ' use UTF-8 instead of default UTF-16
        xml_serializer.Serialize(string_writer, obj)
        serializeShipRequest2string = string_writer.ToString()
        string_writer.Close()
    End Function
    Private Function serializeShipResponse2string(obj As DHL_ShipRes.ShipmentResponse) As String
        Dim xml_serializer As New XmlSerializer(GetType(DHL_ShipRes.ShipmentResponse))
        Dim string_writer As New Utf8StringWriter ' use UTF-8 instead of default UTF-16
        xml_serializer.Serialize(string_writer, obj)
        serializeShipResponse2string = string_writer.ToString()
        string_writer.Close()
    End Function
    Private Function deserializeShipResponse2object(xmlsting As String) As DHL_ShipRes.ShipmentResponse
        Dim xml_serializer As New XmlSerializer(GetType(DHL_ShipRes.ShipmentResponse))
        Dim string_reader As New StringReader(xmlsting)
        deserializeShipResponse2object = DirectCast(xml_serializer.Deserialize(string_reader), DHL_ShipRes.ShipmentResponse)
        string_reader.Close()
    End Function
    Private Function deserializeShipResponseError2object(xmlsting As String) As DHL_ShipErrorRes.ShipmentValidateErrorResponse
        Dim xml_serializer As New XmlSerializer(GetType(DHL_ShipErrorRes.ShipmentValidateErrorResponse))
        Dim string_reader As New StringReader(xmlsting)
        deserializeShipResponseError2object = DirectCast(xml_serializer.Deserialize(string_reader), DHL_ShipErrorRes.ShipmentValidateErrorResponse)
        string_reader.Close()
    End Function

    Private Function Convert_TaxNumberTypeCode2NumberTypeCode(taxRegNum As DHL_NumberTypeCode) As DHL_ShipReq.NumberTypeCode
        Select Case taxRegNum
            Case DHL_NumberTypeCode.CNP : Return DHL_ShipReq.NumberTypeCode.CNP
            Case DHL_NumberTypeCode.DAN : Return DHL_ShipReq.NumberTypeCode.DAN
            Case DHL_NumberTypeCode.DLI : Return DHL_ShipReq.NumberTypeCode.DLI
            Case DHL_NumberTypeCode.DTF : Return DHL_ShipReq.NumberTypeCode.DTF
            Case DHL_NumberTypeCode.DUN : Return DHL_ShipReq.NumberTypeCode.DUN
            Case DHL_NumberTypeCode.EIN : Return DHL_ShipReq.NumberTypeCode.EIN
            Case DHL_NumberTypeCode.EOR : Return DHL_ShipReq.NumberTypeCode.EOR
            Case DHL_NumberTypeCode.FED : Return DHL_ShipReq.NumberTypeCode.FED
            Case DHL_NumberTypeCode.FTZ : Return DHL_ShipReq.NumberTypeCode.FTZ
            Case DHL_NumberTypeCode.MID : Return DHL_ShipReq.NumberTypeCode.MID
            Case DHL_NumberTypeCode.NID : Return DHL_ShipReq.NumberTypeCode.NID
            Case DHL_NumberTypeCode.PAS : Return DHL_ShipReq.NumberTypeCode.PAS
            Case DHL_NumberTypeCode.RGP : Return DHL_ShipReq.NumberTypeCode.RGP
            Case DHL_NumberTypeCode.SDT : Return DHL_ShipReq.NumberTypeCode.SDT
            Case DHL_NumberTypeCode.SSN : Return DHL_ShipReq.NumberTypeCode.SSN
            Case DHL_NumberTypeCode.STA : Return DHL_ShipReq.NumberTypeCode.STA
            Case DHL_NumberTypeCode.TAN : Return DHL_ShipReq.NumberTypeCode.TAN
            Case DHL_NumberTypeCode.VAT : Return DHL_ShipReq.NumberTypeCode.VAT
            Case Else
                Return DHL_ShipReq.NumberTypeCode.SDT ' default
        End Select
    End Function
    Private Function Create_SpecialServices(obj As _baseShipment) As DHL_ShipReq.SpecialService()
        ''
        Dim specialServices As New List(Of DHL_ShipReq.SpecialService)
        If obj.CarrierService IsNot Nothing AndAlso obj.CarrierService.ServiceSurcharges IsNot Nothing AndAlso obj.CarrierService.ServiceSurcharges.Count > 0 Then
            For Each objServiceSurcharge As _baseServiceSurcharge In obj.CarrierService.ServiceSurcharges
                Select Case objServiceSurcharge.Name
                    Case "RESIDENTIAL" ' YB	Oversize Piece (dimension)	Y - Non Standard Shipments	OSP	SCH - Auto/Surcharges
                        specialServices.Add(New DHL_ShipReq.SpecialService() With {.SpecialServiceType = "YB"})
                    Case "SATURDAY_DELIVERY" ' AA	Saturday Delivery	A - Weekends & Holidays	SAT	XCH - Service Option
                        specialServices.Add(New DHL_ShipReq.SpecialService() With {.SpecialServiceType = "AA"})
                    Case "SATURDAY_PICKUP" ' AB	Saturday Pickup	A - Weekends & Holidays	SPU	XCH - Service Option
                        specialServices.Add(New DHL_ShipReq.SpecialService() With {.SpecialServiceType = "AB"})
                    Case "OVERSIZE_PIECE" ' YB	Oversize Piece (dimension)	Y - Non Standard Shipments	OSP	SCH - Auto/Surcharges
                        specialServices.Add(New DHL_ShipReq.SpecialService() With {.SpecialServiceType = "YB"})
                    Case "OVERWEIGHT_PIECE" ' YY	Oversize Piece (weight)	Y - Non Standard Shipments	OWP	SCH - Auto/Surcharges
                        specialServices.Add(New DHL_ShipReq.SpecialService() With {.SpecialServiceType = "YY"})
                    Case "ELEVATED_RISK" ' CA	Elevated Risk	C - Security Services	RSK	SCH - Auto/Surcharges
                        specialServices.Add(New DHL_ShipReq.SpecialService() With {.SpecialServiceType = "CA"})
                    Case "RESTRICTED_DESTINATION" ' CB	Restricted Destination	C - Security Services	RDC	SCH - Auto/Surcharges
                        specialServices.Add(New DHL_ShipReq.SpecialService() With {.SpecialServiceType = "CB"})
                    Case "EXPORTER_VALIDATION" ' WP	Exporter Validation	W - Customs Services	VAL	SCH - Auto/Surcharges
                        specialServices.Add(New DHL_ShipReq.SpecialService() With {.SpecialServiceType = "WP"})
                    Case "SHIPMENT_INSURANCE" ' II	Shipment Insurance	I - Insurance Services	INS	XCH - Service Option
                        specialServices.Add(
                            New DHL_ShipReq.SpecialService() With {
                                .SpecialServiceType = "II",
                                .ChargeValue = objServiceSurcharge.BaseCost,
                                .ChargeValueSpecified = True,
                                .CurrencyCode = objServiceSurcharge.Description
                            }
                        )
                End Select
            Next
            If specialServices.Count > 0 Then
                Return specialServices.ToArray()
            End If
        End If
        ''
        Return Nothing
        ''
    End Function
#End Region

#Region "Prepare Shipment Info from Manifest and Upload"

    Public Function Prepare_ShipmentFromDb(ByVal dbPackageID As String, ByRef objShipment As _baseShipment) As Boolean
        Prepare_ShipmentFromDb = False ' assume
        ''
        Dim objShipperInfo As New _baseContact
        Dim objShipFromInfo As New _baseContact
        Dim objShipToInfo As New _baseContact
        Dim objCarrierSurchargeCOD As New _baseServiceSurchargeCOD
        Dim objCarrierSurcharge As New _baseServiceSurcharge

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
                If _Dhl_XML.objDHL_Setup IsNot Nothing Then
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

                    ''DHL Labels should use the store address as the From address. [SRN-1014]
                    'If gShip.Domestic Then
                    objShipment.ShipperContact = _Contact.ChangeShipFromAs_co_StoreAddress(True) ''ol#9.277(2/13).
                        objShipment.ShipFromContact = _Contact.ChangeShipFromAs_co_StoreAddress(True)
                    'Else
                    'objShipment.ShipperContact = _Contact.ShipFromContact
                    'objShipment.ShipFromContact = _Contact.ShipFromContact
                    'End If
                    objShipment.ShipperContact.AccountNumber = _UPSWeb.objUPS_Setup.ShipRite_ShipperNumber '' baseContact class has AccountNumber property now associated with a address.
                    '
                    ' 'EMAIL - Shipping Notifications' option was added to Carrier Setup tab where you can disable/enable email notifications.
                    If Not _Dhl_XML.IsEmail_DHL_ShipNotification Then
                        objShipment.ShipFromContact.Email = ""
                        objShipment.ShipToContact.Email = ""
                    End If
                    '
                    gShip.Country = objShipment.ShipToContact.Country
                    objShipment.ShipToContact.Residential = ("X" = ExtractElementFromSegment("RES", SegmentSet))
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

                    ' for hawaii customers the country code has to be 'US'
                    ' If the UPS shipper country code cannot be found, default it to 'US'.
                    If objShipment.ShipFromContact.CountryCode = "HI" Or objShipment.ShipFromContact.CountryCode = "" Then
                        objShipment.ShipFromContact.CountryCode = "US"
                    End If
                    ''
                    If objShipment.ShipToContact.Tel.Length = 0 Then
                        objShipment.ShipToContact.Tel = InputBox("Recipient is missing a phone number!" & vbCr & vbCr & vbCr & vbCr & vbCr &
                                                        "Please enter the Recipient's phone number here:", "DHL")
                        If Not 0 = Len(objShipment.ShipToContact.Tel) Then
                            '' To Do
                            ''_Contact.Update_PhoneNo objShipment.ShipToContact.ContactID, objShipment.ShipToContact.Tel
                        End If
                    End If
                    '
                    '
                    objShipment.Comments = ExtractElementFromSegment("Contents", SegmentSet) '"Comments go here"
                    objShipment.RateRequestType = "ACCOUNT"
                    objShipment.CarrierService.CarrierName = "DHL"
                    '
                    'objShipment.CarrierService.ShipDate = DateTime.Now
                    Dim PickupDate As Date = _Convert.String2Date(ExtractElementFromSegment("PickupDate", SegmentSet, Today.ToShortDateString))
                    If PickupDate > Today Then
                        objShipment.CarrierService.ShipDate = PickupDate
                    Else
                        objShipment.CarrierService.ShipDate = Today
                    End If


                    objShipment.ShipmentNo = ExtractElementFromSegment("ShipmentID", SegmentSet)
                    '
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
                                _Debug.Print_(item.Description & " " & item.Value & " " & item.Weight & item.HarmonizedCode)
                                Customs.CustomsList.Add(item)
                                '
                            Loop
                            '
                        End If
                        '
                        ' re-use FedEx international setup of the Commercial Invoice
                        Call _FedExWeb.Prepare_InternationalData(objShipment, False)
                        _Debug.Print_("Packages Count = " & objShipment.Packages.Count)
                        ''
                    End If
                    '
                    Prepare_ShipmentFromDb = True
                    '
                End If
            End If
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
                gShip.actualWeight = Pack.Weight_LBs '' gShip.ActualWeight must be set to determine if the residential shipment is Home Delivery or Not while reading from database.
                Pack.PackagingType = ExtractElementFromSegment("Packaging", SegmentSet)
                Pack.Currency_Type = _IDs.CurrencyType '' CurrencyType variable was added to manipulate between CAD and USD.

                If "X" = ExtractElementFromSegment("AH", SegmentSet) Then
                    Pack.IsAdditionalHandling = True
                End If
                If 0 < Val(ExtractElementFromSegment("AHPlus", SegmentSet)) Then
                    Pack.IsLargePackage = True
                End If
                '
                objShipment.Packages.Add(Pack)
                Prepare_PackageFromDb = True
            End If
            ''
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to read Package data from Manifest database table...")
        End Try
    End Function

    Public Function Upload_Shipment(ByVal objShipment As _baseShipment, Optional NoDelete As Boolean = False, Optional showConfirmMsg As Boolean = True) As Boolean
        Upload_Shipment = True ' assume.
        ''
        Dim sql2exe As String = String.Empty
        Dim p%
        ''
        Dim objResponse As New baseWebResponse_Shipment
        Dim Pack As baseWebResponse_Package
        ''
        Try

            If _Dhl_XML.objDHL_Setup IsNot Nothing Then
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
                        If _Dhl_XML.Process_ShipAPackage(_Dhl_XML.objDHL_Setup, objShipment, objResponse) Then
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
                                        Call sql2cmd.Qry_UPDATE("Exported", EOD.ExportedStatus, sql2cmd.TXT_, True, False, "Manifest", "PackageID = '" & retpack.PackageID & "'")
                                        '
                                    End If
                                    Call sql2cmd.Qry_UPDATE("ReferralSource", "XML", sql2cmd.TXT_)
                                    Call sql2cmd.Qry_UPDATE("Date", String.Format("{0:MM/dd/yyyy}", objShipment.CarrierService.ShipDate), sql2cmd.DTE_)
                                    sql2exe = sql2cmd.Qry_UPDATE("TRACKING#", retpack.TrackingNo, sql2cmd.TXT_, False, True)
                                    _Debug.Print_(sql2exe)
                                    If -1 = IO_UpdateSQLProcessor(gShipriteDB, sql2exe) Then
                                        MsgBox("Failed to update Manifest with DHL tracking number...", MsgBoxStyle.Critical)
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
                                MsgBox("There were some DHL alerts in the response: " & vbCr & alerts, vbExclamation, "DHL Alerts!")
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
                                '' To Do:
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
                            ElseIf Not _Debug.IsINHOUSE Then
                                '
                                Call _Dhl_XML.Print_Label(Pack.PackageID, Pack.LabelImage)
                                If Convert.ToBoolean(General.GetPolicyData(gShipriteDB, "DuplicateLabel", "False")) Then
                                    Call _Dhl_XML.Print_Label(Pack.PackageID, Pack.LabelImage)
                                End If
                                '
                            End If
                        Next p%
                    End If
                    '
                End If
            End If

        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to upload Package to DHL...")
        End Try
    End Function

    Private Function Process_ShipAPackage(ByVal dhlSetup As DHL_Setup, ByVal shipment As _baseShipment, ByRef shipResponse As baseWebResponse_Shipment) As Boolean
        Process_ShipAPackage = False ' assume.
        '
        '
        ' 1. Get available services 
        '
        Dim tntResponse As New baseWebResponse_TinT_Services
        '
        If _Dhl_XML.GetAvailableService_Request(dhlSetup, shipment, tntResponse) Then
            If tntResponse.TimeInTransitAlerts.Count > 0 Then
                MessageBox.Show(tntResponse.TimeInTransitAlerts(0).ToString, "Alerts!", MessageBoxButton.OK, MessageBoxImage.Exclamation)
            Else
                '
                ' 2. Add Available service to Shipment
                '
                Dim shppack As _baseShipmentPackage = shipment.Packages(0)
                For i As Integer = 0 To tntResponse.AvailableServices.Count - 1
                    Dim tntpack As baseWebResponse_TinT_Service = tntResponse.AvailableServices(i)
                    If shppack.IsLetter AndAlso shppack.Weight_LBs <= dhlSetup.Envelope_Weight_Limit_Lbs Then
                        If "X" = tntpack.ServiceCode Then ' X: Express Envelope - XPD - Doc
                            dhlSetup.GlobalProductCode = tntpack.ServiceCode
                            Exit For
                        End If
                    Else
                        ' D: Express WW - DOX - Doc
                        If "D" = tntpack.ServiceCode AndAlso (shipment.CarrierService.ServiceABBR.ToUpper = "DHL-INT-DOC" Or shipment.CommInvoice.TypeOfContents.ToUpper = "DOCUMENTS") Then
                            dhlSetup.GlobalProductCode = tntpack.ServiceCode
                            Exit For
                        ElseIf "P" = tntpack.ServiceCode Then ' P: Express WW - WPX - Non Doc
                            dhlSetup.GlobalProductCode = tntpack.ServiceCode
                            Exit For
                        End If
                    End If
                Next i
                '
                ' 3. Ship Package
                '
                Process_ShipAPackage = _Dhl_XML.ShipAPackage(dhlSetup, shipment, shipResponse)
                '
            End If
        End If
        '

    End Function

    Public Class Utf8StringWriter
        Inherits StringWriter

        Public Overrides ReadOnly Property Encoding As Encoding
            Get
                Return Encoding.UTF8
            End Get
        End Property

    End Class

#End Region

#Region "Printing Labels"

    Public Function Print_Label(ByVal PackageID As String, Optional ByVal LabelImage As String = "") As Boolean
        Print_Label = False
        If Not 0 = Len(LabelImage) Then
            Print_Label = print_LabelFromImage(_Dhl_XML.objDHL_Setup.LabelImageFormat, LabelImage)
        Else
            Print_Label = print_LabelFromFile(_Dhl_XML.objDHL_Setup.LabelImageFormat, PackageID, _Dhl_XML.objDHL_Setup.Path_SaveDocXML)
        End If
    End Function

    Private Function print_LabelFromImage(LabelImageType As String, ByVal LabelImage As String) As Boolean
        print_LabelFromImage = False
        '
        Try
            '
            If LabelImageType = "PDF" Then
                print_LabelFromImage = RawPrinterHelper.SendStringToPrinter(GetPolicyData(gReportsDB, "ReportPrinter"), LabelImage)
            Else
                print_LabelFromImage = RawPrinterHelper.SendStringToPrinter(GetPolicyData(gReportsDB, "LabelPrinter"), LabelImage)
            End If
            '
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to print label...")
        End Try
    End Function

    Private Function print_LabelFromFile(ByVal LabelImageType As String, ByVal flePackageID As String, ByVal dir2DocXML As String) As Boolean
        print_LabelFromFile = False
        '
        Dim PrinterName As String
        Dim imageFile As String = dir2DocXML & "\" & flePackageID & "_Label." & LabelImageType
        '
        Try
            ''
            If LabelImageType = "PDF" Then
                '
                PrinterName = GetPolicyData(gReportsDB, "ReportPrinter")
                '_Debug.Print_(imageFile)
                If _Files.IsFileExist(imageFile, False) Then
                    print_LabelFromFile = RawPrinterHelper.SendFileToPrinter(PrinterName, imageFile)
                    'Process.Start(imageFile)
                End If
                '
            Else ' EPL2, ZPL2
                '
                PrinterName = GetPolicyData(gReportsDB, "LabelPrinter")
                '_Debug.Print_(imageFile)
                If _Files.IsFileExist(imageFile, False) Then
                    '
                    print_LabelFromFile = RawPrinterHelper.SendFileToPrinter(PrinterName, imageFile)
                    '
                End If
                '
            End If
            ''
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to print label...")
        End Try
    End Function

#End Region

End Module
