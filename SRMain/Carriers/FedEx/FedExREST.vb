Imports System.Net
Imports System.Reflection
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports Newtonsoft.Json.Serialization
Imports RestSharp

Module FedEx_REST
#Region "Class Definitions"
    Public Class ShouldSerializeContractResolver
        Inherits DefaultContractResolver

        Public Shared ReadOnly Instance As ShouldSerializeContractResolver = New ShouldSerializeContractResolver()

        Protected Overrides Function CreateProperty(ByVal member As MemberInfo, ByVal memberSerialization As MemberSerialization) As JsonProperty
            Dim [property] As JsonProperty = MyBase.CreateProperty(member, memberSerialization)

            If [property].PropertyType <> GetType(String) Then
                If [property].PropertyType.GetInterface(NameOf(IEnumerable)) IsNot Nothing Then
                    [property].ShouldSerialize = Function(instance) (TryCast(instance?.[GetType]().GetProperty([property].PropertyName).GetValue(instance), IEnumerable(Of Object)))?.Count() > 0
                End If
            End If

            Return [property]
        End Function
    End Class

    Public Class FedExREST_SETUP
        Property OAuthToken As String = ""  'ShipRite Token
        Property OAuthExpires As DateTime
        Property AccountAuthorizationToken As String = "" 'Customer account token
        'Property URL As String = "https://apis-sandbox.fedex.com/" 'testing url
        Property URL As String = "https://apis.fedex.com/"
        'Property ClientID As String = "l7ad4d56237b5c4ed2b56e24813194d433" 'ShipRite Test Credentials
        Property ClientID As String = "l7db5b037015224dfba61604c700397d50" 'ShipRite Credentials
        Property ClientSecret As String = "ae3ea21c-420e-435b-b5cc-295babfef628" 'ShipRite Credentials
        Property Customer_Key As String
        Property Customer_SecretKey As String

        Property AccountNumber As String '= "700257037" 'FedEx Testing Account Number

        Property invoiceDetail As invoiceDetail
        Property CustomerName As String
        Property CustomerAddress As FXR_address
        Property Path_Save_InOut_File As String = String.Format("{0}\FedEx\InOut", gDBpath)

    End Class

    Public Class FedExREST_Reg_Request
        Property address As FXR_address
        Property accountNumber As FXR_accountNumber
        Property customerName As String
    End Class

    Public Class FXR_PIN_Request
        Property [option] As String
    End Class

    Public Class FXR_PIN_Validation_Request
        Property secureCodePin As String
    End Class

    Public Class FXR_address
        Property city As String
        Property countryCode As String
        Property postalCode As String
        Property streetLines As List(Of String) = New List(Of String)
        Property stateOrProvinceCode As String
        Property residential As Boolean
    End Class

    Public Class FXR_accountNumber
        Property value As String
    End Class


    Public Class InvoiceValidation_Request
        Property invoiceDetail As invoiceDetail
        'Property customerName As String
    End Class

    Public Class invoiceDetail
        Property number As Integer
        Property currency As String
        Property [date] As String
        Property amount As Double
    End Class

    Public Class FXR_CreateShipment
        Property labelResponseOptions As String
        Property requestedShipment As FXR_requestedShipment

        Property accountNumber As New FXR_accountNumber
    End Class



    '------------------------------------------------------------------------------------
    Public Class FXR_requestedShipment
        Property shipper As New FXR_ShipperRecipient
        Property recipients As List(Of FXR_ShipperRecipient) = New List(Of FXR_ShipperRecipient)
        Property shipDatestamp As String
        Property pickupType As String
        Property serviceType As String

        Property packagingType As String
        Property shippingChargesPayment As New FXR_shippingChargesPayment

        Property labelSpecification As New FXR_labelSpecification

        Property requestedPackageLineItems As List(Of FXR_RequestedPackageLineItem) = New List(Of FXR_RequestedPackageLineItem)

        Property labelResponseOptions As String

        Property shipmentSpecialServices As FXR_shipmentSpecialServices

        Property customsClearanceDetail As FXR_customsClearanceDetail

        Property emailNotificationDetail As FXR_emailNotificationDetail

    End Class

    '--------------------------------------------------------------------------
    Public Class FXR_CancelShipment
        Property accountNumber As New FXR_accountNumber
        Property trackingNumber As String
    End Class


    Public Class FXR_RequestedPackageLineItem
        Property weight As FXR_weight
        Property dimensions As FXR_dimensions
        Property declaredValue As FXR_declaredValue
        Property packageSpecialServices As FXR_packageSpecialServices
        Property customerReferences As List(Of FXR_customerReferences)
    End Class

    Public Class FXR_customerReferences
        Property customerReferenceType As String
        Property value As String
    End Class

    Public Class FXR_weight
        Property value As Double
        Property units As String

    End Class

    Public Class FXR_declaredValue
        Property amount As Double
        Property currency As String
    End Class

    Public Class FXR_dimensions
        Property length As Integer
        Property width As Integer
        Property height As Integer
        Property units As String

    End Class

    Public Class FXR_packageSpecialServices
        Property specialServiceTypes As List(Of String)
        Property signatureOptionType As String
        Property dryIceWeight As FXR_dryIceWeight
    End Class

    Public Class FXR_dryIceWeight
        Property units As String
        Property value As Double
    End Class

    Public Class FXR_shipmentSpecialServices
        Property specialServiceTypes As List(Of String)
        Property homeDeliveryPremiumDetail As FXR_homeDeliveryPremiumDetail
        Property holdAtLocationDetail As FXR_holdAtLocationDetail
    End Class

    Public Class FXR_holdAtLocationDetail
        Property locationId As String
    End Class

    Public Class FXR_homeDeliveryPremiumDetail
        Property phoneNumber As FXR_phoneNumber
        Property deliveryDate As String
        Property homedeliveryPremiumType As String
    End Class

    Public Class FXR_phoneNumber
        Property areaCode As String
        Property localNumber As String
    End Class


    Public Class FXR_labelSpecification
        Property labelFormatType As String
        Property labelStockType As String

        Property imageType As String
    End Class


    Public Class FXR_shippingChargesPayment
        Property paymentType As String
    End Class

    Public Class FXR_ShipperRecipient
        Property contact As New FXR_contact
        Property address As New FXR_address

    End Class

    Public Class FXR_contact
        Property personName As String
        Property phoneNumber As String
        Property emailAddress As String
        Property companyName As String
    End Class
    Public Class FXR_customsClearanceDetail
        Property commodities As List(Of FXR_commodities)
        Property dutiesPayment As FXR_dutiesPayment
        Property isDocumentOnly As Boolean

        Property exportDetail As FXR_exportDetail
        'Property totalCustomsValue As FXR_customsValue
    End Class

    Public Class FXR_exportDetail
        Property b13AFilingOption As String
        Property exportComplianceStatement As String

    End Class

    Public Class FXR_dutiesPayment
        Property paymentType As String
    End Class

    Public Class FXR_commodities
        Property description As String
        Property numberOfPieces As Integer
        Property weight As FXR_weight
        Property countryOfManufacture As String
        Property harmonizedCode As String
        Property unitPrice As FXR_unitPrice
        Property quantity As Integer
        Property quantityUnits As String
        Property customsValue As FXR_customsValue
    End Class

    Public Class FXR_alerts
        Property code As String
        Property alertType As String
        Property message As String
    End Class

    Public Class FXR_unitPrice
        Property amount As Double
        Property currency As String
    End Class

    Public Class FXR_customsValue
        Property amount As Double
        Property currency As String
    End Class

    Public Class FXR_emailNotificationDetail
        Property emailNotificationRecipients As List(Of FXR_emailNotificationRecipients)
    End Class

    Public Class FXR_emailNotificationRecipients
        Property name As String
        Property emailNotificationRecipientType As String
        Property emailAddress As String
        Property notificationFormatType As String
        Property notificationType As String
        Property locale As String
        Property notificationEventType As List(Of String)
    End Class


    '----- LOCATION SEARCH API ----------------------------------
    'used for Hold At Location address lookup
    Public Class FXR_HAL_LocationRequest
        Property locationsSummaryRequestControlParameters As FXR_locationsSummaryRequestControlParameters
        Property locationSearchCriterion As String
        Property location As FXR_Location
        Property locationCapabilities As List(Of FXR_locationCapabilities)
    End Class
    Public Class FXR_locationsSummaryRequestControlParameters
        Property distance As FXR_Distance

    End Class

    Public Class FXR_Distance
        Property units As String
        Property value As Double
    End Class

    Public Class FXR_Location
        Property address As FXR_address
    End Class

    Public Class FXR_locationCapabilities
        'Property serviceType As String
        Property transferOfPossessionType As String

    End Class


    'HAL Location search result:
    Public Class FXR_LocationDetailList
        Property distance As FXR_Distance
        Property contactAndAddress As FXR_ShipperRecipient

        Property locationId As String

    End Class
    '-------------------------------------------------------------

    Public Class FXR_addressesToValidate
        Property address As FXR_address
    End Class

    Public Class FXR_addressesToValidateList
        Property addressesToValidate As List(Of FXR_addressesToValidate)
    End Class


    Public Class FXR_resolvedAddressesList
        Property resolvedAddresses As List(Of FXR_resolvedAddress)
    End Class

    Public Class FXR_resolvedAddress
        Property streetLinesToken As List(Of String)
        Property city As String
        Property stateOrProvinceCode As String
        Property postalCode As String
        Property countryCode As String
        Property customerMessages As List(Of String)
        Property classification As String


    End Class



    '---------------------------------------------------------------------------
    'TIME IN TRANSIT
    Public Class FXR_TimeInTransitRequest
        Property accountNumber As New FXR_accountNumber
        Property carrierCodes As List(Of String)
        Property rateRequestControlParameters As FXR_rateRequestControlParameters
        Property requestedShipment As New FXR_TinT_requestedShipment
    End Class

    Public Class FXR_TinT_requestedShipment
        Property shipper As New FXR_ShipperRecipient
        Property recipient As New FXR_ShipperRecipient
        Property rateRequestType As New List(Of String)
        Property pickupType As String
        Property requestedPackageLineItems As New List(Of FXR_RequestedPackageLineItem)

    End Class

    Public Class FXR_rateRequestControlParameters
        Property returnTransitTimes As Boolean
    End Class

    'Time In Transit RESPONSE
    Public Class FXR_rateReplyDetails
        Property serviceType As String
        Property dateDetail As New FXR_dateDetail
    End Class

    Public Class FXR_dateDetail
        Property dayOfWeek As String
        Property dayFormat As DateTime
    End Class

    Public Class FXR_EODGroundRequest
        Property closeReqType As String
        Property groundServiceCategory As String
        Property accountNumber As New FXR_accountNumber
        Property closeDate As String
        Property closeDocumentSpecification As New FXR_closeDocumentSpecification

    End Class

    Public Class FXR_closeDocumentSpecification
        Property closeDocumentTypes As New List(Of String)
    End Class

#End Region

    Public Function FXR_Process_Shipment(ByVal obj As _baseShipment, ByRef web_response As baseWebResponse_Shipment) As Boolean
        Try

            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12
            Dim client = New RestClient(gFedExSETUP.URL & "ship/v1/shipments")
            Dim request = New RestRequest(Method.POST)
            Dim FXR As FXR_CreateShipment = New FXR_CreateShipment


            Get_OAuth_Token(False)

            request.AddHeader("Authorization", "Bearer " & gFedExSETUP.OAuthToken)
            request.AddHeader("X-locale", "en_US")
            request.AddHeader("Content-Type", "application/json")

            'FXR.accountNumber.value = _FedExWeb.objFedEx_Regular_Setup.Csp_AccountNumber
            FXR.accountNumber.value = gFedExSETUP.AccountNumber
            FXR.labelResponseOptions = "LABEL"

            FXR.requestedShipment = Create_RequestedShipment(obj)


            Dim jsonPayload As String = JsonConvert.SerializeObject(FXR, Formatting.Indented, New JsonSerializerSettings With {
        .NullValueHandling = NullValueHandling.Ignore,
        .DefaultValueHandling = DefaultValueHandling.Ignore,
        .ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        .ContractResolver = ShouldSerializeContractResolver.Instance})


            request.AddParameter("application/x-www-form-urlencoded", jsonPayload, ParameterType.RequestBody)

            Dim response As IRestResponse = client.Execute(request)
            Debug.Print(jsonPayload)
            System.IO.File.WriteAllText(gFedExSETUP.Path_Save_InOut_File & "/" & obj.Packages(0).PackageID & "_Request.txt", JObject.FromObject(FXR).ToString)
            System.IO.File.WriteAllText(gFedExSETUP.Path_Save_InOut_File & "/" & obj.Packages(0).PackageID & "_Response.txt", JObject.Parse(response.Content).ToString)

            If response.StatusCode = HttpStatusCode.OK Then
                process_Response(response, web_response)

                Return True
            Else
                MsgBox("Failed to create Shipment." & vbCrLf & vbCrLf & response.Content, vbExclamation)
                Debug.Print(response.Content)
                Return False
            End If

        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to create FedEx shipment...")
            Return False
        End Try
    End Function

    Private Sub process_Response(response As IRestResponse, ByRef web_response As baseWebResponse_Shipment)
        Try

            Dim image As String
            Dim labelString As String = String.Empty
            Dim labelFilePath As String
            Dim result As JObject
            Dim alerts As List(Of JToken)

            result = JObject.Parse(response.Content)
            Debug.Print(result.ToString)

            web_response.Packages(0).TrackingNo = result.SelectToken("output.transactionShipments[0].masterTrackingNumber")

            Dim alert_token As JToken = (result.SelectToken("output.transactionShipments[0].alerts"))

            If Not IsNothing(alert_token) Then
                alerts = alert_token.ToList

                For Each alert As JToken In alerts
                    web_response.ShipmentAlerts.Add(alert.ToString)
                Next

            End If


            image = result.SelectToken("output.transactionShipments[0].pieceResponses[0].packageDocuments[0].encodedLabel")
            If GetPolicyData(gReportsDB, "FedExLabelType") = "Laser" Then
                labelFilePath = gFedExSETUP.Path_Save_InOut_File & "\" & web_response.Packages(0).PackageID & "_label.pdf"
            Else
                labelFilePath = gFedExSETUP.Path_Save_InOut_File & "\" & web_response.Packages(0).PackageID & "_label.txt"
            End If


            If (_Files.WriteFile_ToEnd(_Convert.Base64String2Byte(image), labelFilePath)) Then
                If (_Files.ReadFile_ToEnd(labelFilePath, False, labelString)) Then
                    web_response.Packages(0).LabelImage = labelString
                End If
            End If

        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to process shipment response...")
        End Try

    End Sub

    Private Function Create_RequestedShipment(ByVal obj As _baseShipment) As FXR_requestedShipment

        Try
            Dim requestedShipment As New FXR_requestedShipment

            requestedShipment.labelResponseOptions = "LABEL"
            'requestedShipment.shipDatestamp = DateTime.Now.ToString("yyyy-MM-dd")
            requestedShipment.shipDatestamp = obj.CarrierService.ShipDate.ToString("yyyy-MM-dd")

            Populate_Contact(requestedShipment.shipper, obj.ShipFromContact, True)
            requestedShipment.recipients.Add(New FXR_ShipperRecipient)
            Populate_Contact(requestedShipment.recipients(0), obj.ShipToContact, False)


            requestedShipment.serviceType = FXR_GetServiceType(obj.CarrierService.ServiceABBR)
            requestedShipment.packagingType = FXR_GetPackagingType(obj.Packages(0).PackagingType)
            requestedShipment.pickupType = "USE_SCHEDULED_PICKUP"
            requestedShipment.shippingChargesPayment.paymentType = "SENDER"

            If GetPolicyData(gReportsDB, "FedExLabelType") = "Laser" Then
                requestedShipment.labelSpecification.imageType = "PDF"
                requestedShipment.labelSpecification.labelStockType = "PAPER_LETTER"

            Else
                requestedShipment.labelSpecification.labelStockType = "STOCK_4X6"
                requestedShipment.labelSpecification.imageType = "ZPLII"

            End If


            Add_ShipmentSpecialServices(requestedShipment, obj)


            Dim item As New FXR_RequestedPackageLineItem

            'Weight
            item.weight = New FXR_weight With {.units = obj.Packages(0).Weight_Units, .value = obj.Packages(0).Weight_LBs}

            'Dimensions
            item.dimensions = New FXR_dimensions With {
                .height = Math.Round(obj.Packages(0).Dim_Height, 0, MidpointRounding.AwayFromZero),
                .length = Math.Round(obj.Packages(0).Dim_Length, 0, MidpointRounding.AwayFromZero),
                .width = Math.Round(obj.Packages(0).Dim_Width, 0, MidpointRounding.AwayFromZero),
                .units = "IN"}

            'Declared Value
            If obj.Packages(0).DeclaredValue > 0 Then
                item.declaredValue = New FXR_declaredValue With {.amount = obj.Packages(0).DeclaredValue, .currency = "USD"}
            End If
            Add_PackageSpecialServices(item, obj)



            'Add packageID as customer reference
            item.customerReferences = New List(Of FXR_customerReferences)
            Dim reference As New FXR_customerReferences
            reference.customerReferenceType = "CUSTOMER_REFERENCE"
            reference.value = obj.Packages(0).PackageID
            item.customerReferences.Add(reference)


            'Check Email Notification
            Add_EmailNotifications(requestedShipment)

            requestedShipment.requestedPackageLineItems.Add(item)

            If obj.CommInvoice.CommoditiesList.Count > 0 Then
                Add_customsInfo(requestedShipment, obj)
            End If

            Return requestedShipment

        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Error creating FedEx shipment...")
            Return Nothing
        End Try
    End Function

    Private Sub Add_EmailNotifications(ByRef requestedShipment As FXR_requestedShipment)
        Try
            If GetPolicyData(gShipriteDB, "Disable_FedEx_EmailShipNotifications", "True") Then
                'carrier notifications disabled in carrier setup
                Exit Sub
            End If

            If String.IsNullOrEmpty(requestedShipment.shipper.contact.emailAddress) And String.IsNullOrEmpty(requestedShipment.recipients(0).contact.emailAddress) And String.IsNullOrEmpty(gShip.FedEx_EmailNotification_Email) Then
                'no email addresses entered.
                Exit Sub
            End If

            Dim EmailDetail As New FXR_emailNotificationDetail
            Dim recipientsList As New List(Of FXR_emailNotificationRecipients)
            Dim recipient As FXR_emailNotificationRecipients

            'Add shipper email
            If Not String.IsNullOrEmpty(requestedShipment.shipper.contact.emailAddress) Then
                recipient = New FXR_emailNotificationRecipients

                recipient.emailAddress = requestedShipment.shipper.contact.emailAddress
                recipient.name = requestedShipment.shipper.contact.personName
                recipient.emailNotificationRecipientType = "SHIPPER"

                recipientsList.Add(recipient)
            End If

            'add consignee email
            If Not String.IsNullOrEmpty(requestedShipment.recipients(0).contact.emailAddress) Then
                recipient = New FXR_emailNotificationRecipients

                recipient.emailAddress = requestedShipment.recipients(0).contact.emailAddress
                recipient.name = requestedShipment.recipients(0).contact.personName
                recipient.emailNotificationRecipientType = "RECIPIENT"

                recipientsList.Add(recipient)
            End If

            'Add other email
            If Not String.IsNullOrEmpty(gShip.FedEx_EmailNotification_Email) Then
                recipient = New FXR_emailNotificationRecipients

                recipient.emailAddress = gShip.FedEx_EmailNotification_Email
                'recipient.name = requestedShipment.shipper.contact.personName
                recipient.emailNotificationRecipientType = "OTHER"

                recipientsList.Add(recipient)
            End If

            For Each rcpt As FXR_emailNotificationRecipients In recipientsList
                rcpt.locale = "en_US"
                rcpt.notificationType = "EMAIL"
                rcpt.notificationFormatType = "HTML"
                rcpt.notificationEventType = New List(Of String) From {"ON_DELIVERY", "ON_EXCEPTION", "ON_SHIPMENT"}
            Next

            EmailDetail.emailNotificationRecipients = recipientsList
            requestedShipment.emailNotificationDetail = EmailDetail

        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Error with FedEx email notification request...")
        End Try
    End Sub

    Private Sub Add_customsInfo(ByRef requestedShipment As FXR_requestedShipment, ByRef obj As _baseShipment)
        Try
            Dim CCD As New FXR_customsClearanceDetail
            Dim commodity As FXR_commodities
            CCD.commodities = New List(Of FXR_commodities)

            For Each item As _baseCommodities In obj.CommInvoice.CommoditiesList
                commodity = New FXR_commodities

                commodity.countryOfManufacture = item.Item_CountryOfOrigin

                commodity.customsValue = New FXR_customsValue
                commodity.customsValue.amount = item.Item_CustomsValue
                commodity.customsValue.currency = "USD"

                commodity.description = item.Item_Description
                commodity.harmonizedCode = item.Item_Code
                commodity.numberOfPieces = 1
                commodity.quantity = item.Item_Quantity
                commodity.quantityUnits = "PCS"

                commodity.unitPrice = New FXR_unitPrice
                commodity.unitPrice.amount = item.Item_UnitPrice
                commodity.unitPrice.currency = "USD"

                commodity.weight = New FXR_weight
                commodity.weight.value = item.Item_Weight
                commodity.weight.units = item.Item_WeightUnits

                CCD.commodities.Add(commodity)
            Next

            CCD.dutiesPayment = New FXR_dutiesPayment
            CCD.dutiesPayment.paymentType = obj.CommInvoice.DutiesPaymentType

            If obj.CommInvoice.B13AFilingOption <> "" Then
                CCD.exportDetail = New FXR_exportDetail

                CCD.exportDetail.b13AFilingOption = "NOT_REQUIRED"
                CCD.exportDetail.exportComplianceStatement = "NO EEI 30.37(f)"
            End If

            If obj.CommInvoice.TypeOfContents = "Documents" Then CCD.isDocumentOnly = True

            requestedShipment.customsClearanceDetail = CCD



        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Error processing Customs info request...")
        End Try
    End Sub


    Private Sub Add_ShipmentSpecialServices(ByRef requestedShipment As FXR_requestedShipment, ByRef obj As _baseShipment)
        Try
            If obj.CarrierService.ServiceSurcharges.Count > 0 Then
                Dim shipmentSpecialServices = New FXR_shipmentSpecialServices
                shipmentSpecialServices.specialServiceTypes = New List(Of String)

                For Each x As _baseServiceSurcharge In obj.CarrierService.ServiceSurcharges
                    If x.Name = "HOME_DELIVERY_PREMIUM" Then
                        shipmentSpecialServices.specialServiceTypes.Add("HOME_DELIVERY_PREMIUM")
                        shipmentSpecialServices.homeDeliveryPremiumDetail = New FXR_homeDeliveryPremiumDetail
                        shipmentSpecialServices.homeDeliveryPremiumDetail.homedeliveryPremiumType = x.Description


                        Dim phone As New FXR_phoneNumber
                        phone.localNumber = Text.RegularExpressions.Regex.Replace(requestedShipment.recipients(0).contact.phoneNumber, "[^\d]", "")
                        shipmentSpecialServices.homeDeliveryPremiumDetail.phoneNumber = phone


                        If x.Description = "DATE_CERTAIN" Then
                            Dim DDate As Date = Convert.ToDateTime(gShip.HOMEFedEXDeliveryDate)
                            shipmentSpecialServices.homeDeliveryPremiumDetail.deliveryDate = DDate.ToString("yyyy-MM-dd")
                        End If

                    ElseIf x.Name = "SATURDAY_DELIVERY" Then
                        shipmentSpecialServices.specialServiceTypes.Add("SATURDAY_DELIVERY")

                    ElseIf x.Name = "SATURDAY_PICKUP" Then
                        shipmentSpecialServices.specialServiceTypes.Add("SATURDAY_PICKUP")

                    ElseIf x.Name = "FEDEX_ONE_RATE" Then
                        shipmentSpecialServices.specialServiceTypes.Add("FEDEX_ONE_RATE")

                    ElseIf x.Name = "HOLD_AT_LOCATION" Then
                        shipmentSpecialServices.specialServiceTypes.Add("HOLD_AT_LOCATION")
                        shipmentSpecialServices.holdAtLocationDetail = New FXR_holdAtLocationDetail
                        shipmentSpecialServices.holdAtLocationDetail.locationId = gShip.HoldAtLocationID

                    ElseIf x.Name = "INSIDE_PICKUP" Then
                        shipmentSpecialServices.specialServiceTypes.Add("INSIDE_PICKUP")

                    ElseIf x.Name = "INSIDE_DELIVERY" Then
                        shipmentSpecialServices.specialServiceTypes.Add("INSIDE_DELIVERY")

                    End If


                Next

                requestedShipment.shipmentSpecialServices = shipmentSpecialServices
            End If

        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Error processing FedEx shipment special services request...")
        End Try
    End Sub

    Private Sub Add_PackageSpecialServices(ByRef item As FXR_RequestedPackageLineItem, ByRef obj As _baseShipment)
        Try
            If obj.Packages(0).ServiceSurcharges.Count > 0 Then
                item.packageSpecialServices = New FXR_packageSpecialServices
                item.packageSpecialServices.specialServiceTypes = New List(Of String)

                For Each charge As _baseServiceSurcharge In obj.Packages(0).ServiceSurcharges
                    If charge.Name = "SIGNATURE_OPTION" Then
                        item.packageSpecialServices.specialServiceTypes.Add("SIGNATURE_OPTION")
                        item.packageSpecialServices.signatureOptionType = FXR_GetSignatureOptionType(charge.Description)

                    ElseIf charge.Name = "NON_STANDARD_CONTAINER" Then
                        item.packageSpecialServices.specialServiceTypes.Add("NON_STANDARD_CONTAINER")

                    ElseIf charge.Name = "DRY_ICE" Then
                        item.packageSpecialServices.specialServiceTypes.Add("DRY_ICE")
                        item.packageSpecialServices.dryIceWeight = New FXR_dryIceWeight With {
                        .units = obj.Packages(0).DryIce.WeightUnits, .value = obj.Packages(0).DryIce.Weight}

                    End If
                Next
            End If

        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to Add special services to request...")
        End Try
    End Sub

    Private Sub Populate_Contact(ByRef ShipContact As FXR_ShipperRecipient, ByVal obj As _baseContact, isShipper As Boolean)
        Try
            Dim contact As New FXR_contact

            contact.companyName = obj.CompanyName

            If isShipper Then
                'shipper
                contact.personName = obj.FNameLName
            Else
                'consignee
                If obj.CompanyName <> obj.LNameFName And obj.LNameFName <> "," Then
                    contact.personName = obj.LNameFName
                End If
            End If

            contact.emailAddress = obj.Email
            contact.phoneNumber = obj.Tel


            Dim add As New FXR_address
            add.streetLines.Add(obj.Addr1)

            If obj.Addr2 <> "" Then
                add.streetLines.Add(obj.Addr2)

                If obj.Addr3 <> "" Then
                    add.streetLines.Add(obj.Addr3)
                End If

            End If

            add.city = obj.City
            add.stateOrProvinceCode = obj.State
            add.postalCode = obj.Zip
            add.countryCode = obj.CountryCode
            add.residential = obj.Residential

            ShipContact.address = add
            ShipContact.contact = contact

        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to populate contact information...")
        End Try
    End Sub

    Public Function FXR_GetServiceType(ByVal serviceABBR As String) As String
        Try

            FXR_GetServiceType = "GROUND_HOME_DELIVERY" '' assume.

            ''
            Select Case serviceABBR
                Case FedEx.Ground, FedEx.CanadaGround : FXR_GetServiceType = "FEDEX_GROUND"
                Case FedEx.FirstOvernight : FXR_GetServiceType = "FIRST_OVERNIGHT"
                Case FedEx.SecondDay : FXR_GetServiceType = "FEDEX_2_DAY"
                Case FedEx.SecondDayAM : FXR_GetServiceType = "FEDEX_2_DAY_AM"
                Case FedEx.Priority : FXR_GetServiceType = "PRIORITY_OVERNIGHT"
                Case FedEx.Standard : FXR_GetServiceType = "STANDARD_OVERNIGHT"
                Case FedEx.Saver : FXR_GetServiceType = "FEDEX_EXPRESS_SAVER"
                Case FedEx.Intl_First : FXR_GetServiceType = "INTERNATIONAL_FIRST" ''EUROPE_FIRST_INTERNATIONAL_PRIORITY
                Case FedEx.Intl_Priority : FXR_GetServiceType = "INTERNATIONAL_PRIORITY" ''INTERNATIONAL_PRIORITY_FREIGHT
                Case FedEx.Intl_Economy : FXR_GetServiceType = "INTERNATIONAL_ECONOMY"''INTERNATIONAL_ECONOMY_FREIGHT
                Case FedEx.Freight_1Day : FXR_GetServiceType = "FEDEX_1_DAY_FREIGHT"
                Case FedEx.Freight_2Day : FXR_GetServiceType = "FEDEX_2_DAY_FREIGHT"
                Case FedEx.Freight_3Day : FXR_GetServiceType = "FEDEX_3_DAY_FREIGHT"
                                ''ol#1.2.69(5/8)... FedEx Freight Box services were added.
                Case "FEDEX-FRP" : FXR_GetServiceType = "FEDEX_FREIGHT_PRIORITY"
                Case "FEDEX-FRE" : FXR_GetServiceType = "FEDEX_FREIGHT_ECONOMY"
            End Select
            ''
        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to obtain Service Type...")
            Return ""
        End Try
    End Function

    Private Function FXR_GetPackagingType(ByVal srPackagingType As String) As String
        Try

            FXR_GetPackagingType = "YOUR_PACKAGING" '' assume.
            'Try
            If _Controls.Contains(srPackagingType, "Letter") Or _Controls.Contains(srPackagingType, "Env") Then
                FXR_GetPackagingType = "FEDEX_ENVELOPE"
            ElseIf _Controls.Contains(srPackagingType, "Box") And _Controls.Contains(srPackagingType, "10kg") Then
                FXR_GetPackagingType = "FEDEX_10KG_BOX"
            ElseIf _Controls.Contains(srPackagingType, "Box") And _Controls.Contains(srPackagingType, "25kg") Then
                FXR_GetPackagingType = "FEDEX_25KG_BOX"

            ElseIf _Controls.Contains(srPackagingType, "Box") AndAlso _Controls.Contains(srPackagingType, "Extra") AndAlso _Controls.Contains(srPackagingType, "Large") Then
                FXR_GetPackagingType = "FEDEX_EXTRA_LARGE_BOX"
            ElseIf _Controls.Contains(srPackagingType, "Box") AndAlso _Controls.Contains(srPackagingType, "Large") Then
                FXR_GetPackagingType = "FEDEX_LARGE_BOX"
            ElseIf _Controls.Contains(srPackagingType, "Box") AndAlso _Controls.Contains(srPackagingType, "Medium") Then
                FXR_GetPackagingType = "FEDEX_MEDIUM_BOX"
            ElseIf _Controls.Contains(srPackagingType, "Box") AndAlso _Controls.Contains(srPackagingType, "Small") Then
                FXR_GetPackagingType = "FEDEX_SMALL_BOX"

            ElseIf _Controls.Contains(srPackagingType, "Box") Then
                FXR_GetPackagingType = "FEDEX_BOX"
            ElseIf _Controls.Contains(srPackagingType, "Pak") Then
                FXR_GetPackagingType = "FEDEX_PAK"
            ElseIf _Controls.Contains(srPackagingType, "Tube") Then
                FXR_GetPackagingType = "FEDEX_TUBE"
            End If

        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to load packaging type...")
            Return ""
        End Try
    End Function

    Public Function FXR_GetSignatureOptionType(ByVal signatureOption As String) As String
        Try
            ''
            FXR_GetSignatureOptionType = "SERVICE_DEFAULT"  '' assume.
            Select Case signatureOption
                Case "Adult Signature" : FXR_GetSignatureOptionType = "ADULT"
                Case "Direct Signature", "Direct Sig. - No Charge" : FXR_GetSignatureOptionType = "DIRECT"
                Case "Indirect Signature" : FXR_GetSignatureOptionType = "INDIRECT"
                Case "Del.Conf at No Charge" : FXR_GetSignatureOptionType = "NO_SIGNATURE_REQUIRED"
                Case "No Signature Required" : FXR_GetSignatureOptionType = "NO_SIGNATURE_REQUIRED"
            End Select
            ''
        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to obtain signature option...")
            Return "SERVICE_DEFAULT"
        End Try
    End Function

    Public Function FXR_VoidShipment(ByVal TrackingNum As String) As Boolean
        Try
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12
            Dim client = New RestClient(gFedExSETUP.URL & "ship/v1/shipments/cancel")
            Dim request = New RestRequest(Method.PUT)
            Dim FXR As FXR_CancelShipment = New FXR_CancelShipment
            Dim result As JObject


            Get_OAuth_Token(False)

            request.AddHeader("Authorization", "Bearer " & gFedExSETUP.OAuthToken)
            request.AddHeader("X-locale", "en_US")
            request.AddHeader("Content-Type", "application/json")

            'FXR.accountNumber.value = _FedExWeb.objFedEx_Regular_Setup.Csp_AccountNumber
            FXR.accountNumber.value = gFedExSETUP.AccountNumber
            FXR.trackingNumber = TrackingNum


            Dim jsonPayload As String = JsonConvert.SerializeObject(FXR, Formatting.Indented)


            request.AddParameter("application/x-www-form-urlencoded", jsonPayload, ParameterType.RequestBody)

            Dim response As IRestResponse = client.Execute(request)
            Debug.Print(jsonPayload)
            System.IO.File.WriteAllText(gFedExSETUP.Path_Save_InOut_File & "/VoidShipment_Request.txt", JObject.FromObject(FXR).ToString)
            System.IO.File.WriteAllText(gFedExSETUP.Path_Save_InOut_File & "/VoidShipment_Response.txt", JObject.Parse(response.Content).ToString)

            If response.StatusCode = HttpStatusCode.OK Then
                result = JObject.Parse(response.Content)

                Debug.Print(result.ToString)

                Return True
            Else
                MsgBox("Failed to void Shipment." & vbCrLf & vbCrLf & response.Content, vbExclamation)
                Debug.Print(response.Content)
                Return False
            End If

        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to create FedEx shipment...")
            Return False
        End Try
    End Function


#Region "Registration"
    Public Function Get_OAuth_Token(isRegistration As Boolean) As Boolean
        Try
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12

            If Not isRegistration Then
                'For registration do not check existing token, obtain a a new one.
                If Not Is_Current_OAuthToken_Expired(gFedExSETUP) Then
                    'existing token still valid, exit sub
                    Return True
                End If
            End If

            Dim client = New RestClient(gFedExSETUP.URL & "oauth/token")
            Dim request = New RestRequest(Method.POST)
            Dim result As JObject
            Dim ExpiresIn As TimeSpan

            request.AddHeader("Content-Type", "application/x-www-form-urlencoded")

            If isRegistration Then
                request.AddParameter("grant_type", "client_credentials")
            Else
                request.AddParameter("grant_type", "csp_credentials")
                request.AddParameter("child_Key", gFedExSETUP.Customer_Key)
                request.AddParameter("child_Secret", gFedExSETUP.Customer_SecretKey)
            End If

            request.AddParameter("client_id", gFedExSETUP.ClientID)
            request.AddParameter("client_secret", gFedExSETUP.ClientSecret)

            Dim response As IRestResponse = client.Execute(request)

            If isRegistration Then
                'Saving Files with tokens should be for FedEx certification / bug fixing purposes only.
                System.IO.File.WriteAllText(gFedExSETUP.Path_Save_InOut_File & "/GetToken_Parent_Request.txt", JObject.FromObject(request).ToString)
                System.IO.File.WriteAllText(gFedExSETUP.Path_Save_InOut_File & "/GetToken_Parent_Response.txt", JObject.Parse(response.Content).ToString)

            Else
                System.IO.File.WriteAllText(gFedExSETUP.Path_Save_InOut_File & "/GetToken_Child_Request.txt", JObject.FromObject(request).ToString)
                System.IO.File.WriteAllText(gFedExSETUP.Path_Save_InOut_File & "/GetToken_Child_Response.txt", JObject.Parse(response.Content).ToString)

            End If


            If response.StatusCode = HttpStatusCode.OK Then
                result = JObject.Parse(response.Content)

                gFedExSETUP.OAuthToken = result.SelectToken("access_token")
                ExpiresIn = TimeSpan.FromSeconds(result.SelectToken("expires_in"))
                gFedExSETUP.OAuthExpires = Now + ExpiresIn

                If Not isRegistration Then
                    ' registration OAuth token does not need to be saved since it's only used for the registration
                    UpdatePolicy(gShipriteDB, "FedExREST_OAuth_Token", gFedExSETUP.OAuthToken)
                    UpdatePolicy(gShipriteDB, "FedExREST_OAuth_Expires", gFedExSETUP.OAuthExpires)
                End If

                Return True
            Else
                MsgBox("Failed to obtain OAuth token." & vbCrLf & vbCrLf & response.Content, vbExclamation)
                Return False
            End If

        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Error obtaining OAuth token...")
            Return False
        End Try
    End Function

    Private Function Is_Current_OAuthToken_Expired(FedExSetup As FedExREST_SETUP) As Boolean
        Try
            FedExSetup.OAuthToken = GetPolicyData(gShipriteDB, "FedExREST_OAuth_Token", "")

            If GetPolicyData(gShipriteDB, "FedExREST_OAuth_Expires") = "" Then
                Return True
            Else
                FedExSetup.OAuthExpires = GetPolicyData(gShipriteDB, "FedExREST_OAuth_Expires")
            End If

            If FedExSetup.OAuthToken = "" Then
                Return True
            End If

            If FedExSetup.OAuthExpires = "1/1/0001" Then
                Return True
            End If

            If FedExSetup.OAuthExpires.AddSeconds(-10) > DateTime.Now Then
                Return False
            Else
                Return True
            End If

        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Erro checking OAuth token...")
            Return False
        End Try
    End Function


    Public Function Get_Customer_Key(Optional IsTechSupportRegistration As Boolean = False) As Boolean
        Try
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12
            Dim client = New RestClient(gFedExSETUP.URL & "registration/v2/address/keysgeneration")
            Dim request = New RestRequest(Method.POST)
            Dim result As JObject

            request.AddHeader("Authorization", "Bearer " & gFedExSETUP.OAuthToken)
            request.AddHeader("X-locale", "en_US")
            request.AddHeader("Content-Type", "application/json")

            Dim FXR As FedExREST_Reg_Request = New FedExREST_Reg_Request

            Dim address As New FXR_address

            'Testing Address
            'With address
            '    .city = "New York"
            '    .countryCode = "US"
            '    .postalCode = "100114624"
            '    .streetLines.Add("15 W 18TH ST FL 7")
            '    .stateOrProvinceCode = "NY"
            'End With

            With address
                .city = _StoreOwner.StoreOwner.City
                .countryCode = _StoreOwner.StoreOwner.CountryCode
                .postalCode = _StoreOwner.StoreOwner.Zip
                .streetLines.Add(_StoreOwner.StoreOwner.Addr1)

                If _StoreOwner.StoreOwner.Addr2 <> "" Then
                    .streetLines.Add(_StoreOwner.StoreOwner.Addr2)
                End If

                .stateOrProvinceCode = _StoreOwner.StoreOwner.State
            End With

            Dim AcctNum As New FXR_accountNumber With {.value = gFedExSETUP.AccountNumber}

            FXR.address = address
            FXR.accountNumber = AcctNum
            FXR.customerName = gFedExSETUP.CustomerName '"Shiprite Software - CSP"
            ' FXR.customerName = "Shiprite Software - CSP110"

            request.AddParameter("application/x-www-form-urlencoded", JObject.FromObject(FXR), ParameterType.RequestBody)
            Debug.Print(JObject.FromObject(FXR).ToString)

            Dim response As IRestResponse = client.Execute(request)

            'Saving Files with customer key should be for FedEx certification purposes only.
            System.IO.File.WriteAllText(gFedExSETUP.Path_Save_InOut_File & "/GetCustomerKey_Request.txt", JObject.FromObject(FXR).ToString)
            System.IO.File.WriteAllText(gFedExSETUP.Path_Save_InOut_File & "/GetCustomerKey_Response.txt", JObject.Parse(response.Content).ToString)


            If response.StatusCode = HttpStatusCode.OK Then
                result = JObject.Parse(response.Content)

                If IsTechSupportRegistration Then
                    'if verifying account via FedEx Tech support call, then the child key and secret key will be returned.
                    If IsNothing(result.SelectToken("output").SelectToken("child_Key")) Then
                        Return False
                    End If

                    gFedExSETUP.Customer_Key = result.SelectToken("output").SelectToken("child_Key")
                    gFedExSETUP.Customer_SecretKey = result.SelectToken("output").SelectToken("child_secret")

                    UpdatePolicy(gShipriteDB, "FedExREST_CustomerKey", gFedExSETUP.Customer_Key)
                    UpdatePolicy(gShipriteDB, "FedExREST_CustomerSecret", gFedExSETUP.Customer_SecretKey)
                    Return True
                Else

                    gFedExSETUP.AccountAuthorizationToken = result.SelectToken("output.mfaOptions[0]").SelectToken("accountAuthToken")
                    Return True
                End If

            Else
                MsgBox("Failed to verify customer address." & vbCrLf & vbCrLf & response.Content, vbExclamation)
                Return False
            End If

        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Error obtaining FedEx Customer Key..")
            Return False
        End Try
    End Function

    Public Function VerifyInvoice()
        Try
            Dim client = New RestClient(gFedExSETUP.URL & "registration/v2/invoice/keysgeneration")
            Dim request = New RestRequest(Method.POST)
            Dim result As JObject

            request.AddHeader("Authorization", "Bearer " & gFedExSETUP.OAuthToken)
            request.AddHeader("accountAuthToken", gFedExSETUP.AccountAuthorizationToken)
            request.AddHeader("X-locale", "en_US")
            request.AddHeader("Content-Type", "application/json")


            'Dim InvoiceValidation As New InvoiceValidation_Request With {
            '.invoiceDetail = gFedExSETUP.invoiceDetail,
            '.customerName = gFedExSETUP.CustomerName' "Shiprite Software - CSP6"
            '}

            Dim InvoiceValidation As New InvoiceValidation_Request
            InvoiceValidation.invoiceDetail = gFedExSETUP.invoiceDetail


            request.AddParameter("application/x-www-form-urlencoded", JObject.FromObject(InvoiceValidation), ParameterType.RequestBody)
            Dim response As IRestResponse = client.Execute(request)

            System.IO.File.WriteAllText(gFedExSETUP.Path_Save_InOut_File & "/VerifyInvoice_Request.txt", JObject.FromObject(InvoiceValidation).ToString)
            System.IO.File.WriteAllText(gFedExSETUP.Path_Save_InOut_File & "/VerifyInvoice_Response.txt", JObject.Parse(response.Content).ToString)


            If response.StatusCode = HttpStatusCode.OK Then
                result = JObject.Parse(response.Content)
                gFedExSETUP.Customer_Key = result.SelectToken("output").SelectToken("child_Key")
                gFedExSETUP.Customer_SecretKey = result.SelectToken("output").SelectToken("child_secret")

                UpdatePolicy(gShipriteDB, "FedExREST_CustomerKey", gFedExSETUP.Customer_Key)
                UpdatePolicy(gShipriteDB, "FedExREST_CustomerSecret", gFedExSETUP.Customer_SecretKey)

                MsgBox("FedEx account successfully registered!", vbInformation, "Success!")
                Return True
            Else
                MsgBox("Failed to verify Invoice information." & vbCrLf & vbCrLf & response.Content, vbExclamation)
                Return False
            End If

        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Error verifying invoice information...")
            Return False
        End Try
    End Function

    Public Function Request_PIN(Type As String) As Boolean
        System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12
        Dim client = New RestClient(gFedExSETUP.URL & "registration/v2/customerkeys/pingeneration")
        Dim request = New RestRequest(Method.POST)
        Dim result As JObject


        request.AddHeader("Authorization", "Bearer " & gFedExSETUP.OAuthToken)
        request.AddHeader("accountAuthToken", gFedExSETUP.AccountAuthorizationToken)
        request.AddHeader("X-locale", "en_US")
        request.AddHeader("Content-Type", "application/json")

        Dim Payload As FXR_PIN_Request = New FXR_PIN_Request
        Payload.option = Type

        request.AddParameter("application/x-www-form-urlencoded", JObject.FromObject(Payload), ParameterType.RequestBody)

        Dim response As IRestResponse = client.Execute(request)

        System.IO.File.WriteAllText(gFedExSETUP.Path_Save_InOut_File & "/PIN_Request.txt", JObject.FromObject(Payload).ToString)
        System.IO.File.WriteAllText(gFedExSETUP.Path_Save_InOut_File & "/PIN_Response.txt", JObject.Parse(response.Content).ToString)

        Debug.Print(JObject.FromObject(Payload).ToString)

        If response.StatusCode = HttpStatusCode.OK Then
            result = JObject.Parse(response.Content)


            MsgBox("PIN sent successfully!", vbInformation, "Success!")

            Return True
        Else
            MsgBox("Failed to verify PIN." & vbCrLf & vbCrLf & response.Content, vbExclamation)
            Return False
        End If

    End Function

    Public Function Register_PIN(PIN As String) As Boolean
        System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12
        Dim client = New RestClient(gFedExSETUP.URL & "registration/v2/pin/keysgeneration")
        Dim request = New RestRequest(Method.POST)
        Dim result As JObject


        request.AddHeader("Authorization", "Bearer " & gFedExSETUP.OAuthToken)
        request.AddHeader("accountAuthToken", gFedExSETUP.AccountAuthorizationToken)
        request.AddHeader("X-locale", "en_US")
        request.AddHeader("Content-Type", "application/json")

        Dim Payload As FXR_PIN_Validation_Request = New FXR_PIN_Validation_Request
        Payload.secureCodePin = PIN

        request.AddParameter("application/x-www-form-urlencoded", JObject.FromObject(Payload), ParameterType.RequestBody)
        Debug.Print(JObject.FromObject(Payload).ToString)
        Dim response As IRestResponse = client.Execute(request)

        System.IO.File.WriteAllText(gFedExSETUP.Path_Save_InOut_File & "/Register_PIN_Request.txt", JObject.FromObject(Payload).ToString)
        System.IO.File.WriteAllText(gFedExSETUP.Path_Save_InOut_File & "/Register_PIN_Response.txt", JObject.Parse(response.Content).ToString)


        If response.StatusCode = HttpStatusCode.OK Then
            result = JObject.Parse(response.Content)

            gFedExSETUP.Customer_Key = result.SelectToken("output").SelectToken("child_Key")
            gFedExSETUP.Customer_SecretKey = result.SelectToken("output").SelectToken("child_secret")

            UpdatePolicy(gShipriteDB, "FedExREST_CustomerKey", gFedExSETUP.Customer_Key)
            UpdatePolicy(gShipriteDB, "FedExREST_CustomerSecret", gFedExSETUP.Customer_SecretKey)

            MsgBox("FedEx account successfully registered!", vbInformation, "Success!")

            Return True
        Else
            MsgBox("Failed to verify PIN." & vbCrLf & vbCrLf & response.Content, vbExclamation)
            Return False
        End If

    End Function

#End Region


#Region "Hold At Location"
    Public Function FXR_GetListOf_HAL_Locations(recipient As _baseContact) As List(Of FXR_LocationDetailList)
        Try
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12
            Dim client = New RestClient(gFedExSETUP.URL & "location/v1/locations")
            Dim request = New RestRequest(Method.POST)
            Dim result As JObject

            Get_OAuth_Token(False)
            request.AddHeader("Authorization", "Bearer " & gFedExSETUP.OAuthToken)
            request.AddHeader("X-locale", "en_US")
            request.AddHeader("Content-Type", "application/json")

            Dim hal_request As FXR_HAL_LocationRequest
            hal_request = Create_HAL_Location_Request(recipient)


            Dim jsonPayload As String = JsonConvert.SerializeObject(hal_request, Formatting.Indented)
            Debug.Print(jsonPayload)

            request.AddParameter("application/x-www-form-urlencoded", jsonPayload, ParameterType.RequestBody)

            Dim response As IRestResponse = client.Execute(request)

            _FedExWeb.objFedEx_Setup = _FedExWeb.objFedEx_Regular_Setup
            System.IO.File.WriteAllText(gFedExSETUP.Path_Save_InOut_File & "/" & "HAL_Location_Request.txt", jsonPayload.ToString)
            System.IO.File.WriteAllText(gFedExSETUP.Path_Save_InOut_File & "/" & "HAL_Location_Response.txt", JObject.Parse(response.Content).ToString)

            If response.StatusCode = HttpStatusCode.OK Then
                result = JObject.Parse(response.Content)
                Dim ResultList As List(Of FXR_LocationDetailList) = New List(Of FXR_LocationDetailList)
                ResultList = result.SelectToken("output.locationDetailList").ToObject(Of List(Of FXR_LocationDetailList))
                'Debug.Print(result.ToString)
                Return ResultList

            Else

                MsgBox("Failed to obtain FedEx HAL Location List." & vbCrLf & vbCrLf & response.Content, vbExclamation)
                Return Nothing

            End If

        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Error obtaining FedEx HAL Location List.")
            Return Nothing
        End Try
    End Function



    Private Function Create_HAL_Location_Request(recipient As _baseContact) As FXR_HAL_LocationRequest
        Try
            Dim hal_request As New FXR_HAL_LocationRequest

            Dim LSRP As New FXR_locationsSummaryRequestControlParameters
            Dim distance As New FXR_Distance
            distance.units = "MI"
            distance.value = 15
            LSRP.distance = distance
            hal_request.locationsSummaryRequestControlParameters = LSRP


            hal_request.locationSearchCriterion = "ADDRESS"
            hal_request.location = New FXR_Location


            Dim contact As New FXR_ShipperRecipient
            Populate_Contact(contact, recipient, True)
            hal_request.location.address = contact.address



            hal_request.locationCapabilities = New List(Of FXR_locationCapabilities)

            Dim locCap As New FXR_locationCapabilities
            locCap.transferOfPossessionType = "HOLD_AT_LOCATION"
            hal_request.locationCapabilities.Add(locCap)

            Return hal_request

        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Error creating HAL request.")
            Return Nothing
        End Try

    End Function

#End Region

#Region "Address Verification"

    Public Function FXR_Submit_Address_For_Validation(ByRef contact As _baseContact, ByRef verifiedContact As _baseContact) As Boolean
        Try
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12
            Dim result As JObject
            Dim JsonPayload As String

            Dim client = New RestClient(gFedExSETUP.URL & "address/v1/addresses/resolve")
            Dim request = New RestRequest(Method.POST)


            Get_OAuth_Token(False)
            request.AddHeader("Authorization", "Bearer " & gFedExSETUP.OAuthToken)
            request.AddHeader("X-locale", "en_US")
            request.AddHeader("Content-Type", "application/json")

            'JsonPayload = JsonConvert.SerializeObject(FXR_Create_AddressValidation_Payload(contact), Formatting.Indented)
            JsonPayload = JsonConvert.SerializeObject(FXR_Create_AddressValidation_Payload(contact), Formatting.Indented, New JsonSerializerSettings With {
        .NullValueHandling = NullValueHandling.Ignore,
        .DefaultValueHandling = DefaultValueHandling.Ignore,
        .ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        .ContractResolver = ShouldSerializeContractResolver.Instance})

            request.AddParameter("application/x-www-form-urlencoded", JsonPayload, ParameterType.RequestBody)

            Dim response As IRestResponse = client.Execute(request)
            Debug.Print(JsonPayload.ToString)

            If response.StatusCode = HttpStatusCode.OK Then

                result = JObject.Parse(response.Content)

                Debug.Print(result.ToString)

                Dim resolvedAdd As New FXR_resolvedAddress
                resolvedAdd = result.SelectToken("output.resolvedAddresses(0)").ToObject(Of FXR_resolvedAddress)

                Load_VerifiedContact(verifiedContact, resolvedAdd)

                Return True

            Else

                MsgBox("Failed to verify FedEx address!" & vbCrLf & vbCrLf & response.Content, vbExclamation)
                Return False

            End If

        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Error verifying address with FedEx.")
            Return Nothing
        End Try

    End Function

    Private Sub Load_VerifiedContact(ByRef verifiedContact As _baseContact, ByRef resolvedAdd As FXR_resolvedAddress)
        Try

            With verifiedContact

                Select Case resolvedAdd.streetLinesToken.Count
                    Case 1
                        .Addr1 = resolvedAdd.streetLinesToken(0)
                    Case 2
                        .Addr1 = resolvedAdd.streetLinesToken(0)
                        .Addr2 = resolvedAdd.streetLinesToken(1)
                    Case 3
                        .Addr1 = resolvedAdd.streetLinesToken(0)
                        .Addr2 = resolvedAdd.streetLinesToken(1)
                        .Addr3 = resolvedAdd.streetLinesToken(2)
                End Select

                .City = resolvedAdd.city
                .State = resolvedAdd.stateOrProvinceCode
                .Zip = resolvedAdd.postalCode
                .CountryCode = resolvedAdd.countryCode

                If resolvedAdd.classification = "RESIDENTIAL" Then
                    .Residential = True
                Else
                    .Residential = False
                End If

            End With

        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Error loading verification address.")

        End Try
    End Sub

    Private Function FXR_Create_AddressValidation_Payload(ByRef contact As _baseContact) As FXR_addressesToValidateList
        Try
            Dim addressesToValidateList As New FXR_addressesToValidateList
            Dim addressesToValidate As New FXR_addressesToValidate
            Dim address As New FXR_address

            address.streetLines.Add(contact.Addr1)
            If Not String.IsNullOrEmpty(contact.Addr2) Then address.streetLines.Add(contact.Addr2)

            address.city = contact.City
            address.stateOrProvinceCode = contact.State
            address.postalCode = contact.Zip
            address.countryCode = "US"


            addressesToValidate.address = address

            addressesToValidateList.addressesToValidate = New List(Of FXR_addressesToValidate)
            addressesToValidateList.addressesToValidate.Add(addressesToValidate)

            Return addressesToValidateList

        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Error loading verification address...")
            Return Nothing
        End Try
    End Function


#End Region

#Region "Time In Transit"

    Public Sub Process_TinT_Request(ByVal obj As _baseShipment, ByRef shipments As baseWebResponse_TinT_Services)
        Try

            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12
            Dim client = New RestClient(gFedExSETUP.URL & "rate/v1/comprehensiverates/quotes")
            Dim request = New RestRequest(Method.POST)
            Dim result As JObject


            Get_OAuth_Token(False)

            request.AddHeader("Authorization", "Bearer " & gFedExSETUP.OAuthToken)
            request.AddHeader("X-locale", "en_US")
            request.AddHeader("Content-Type", "application/json")


            Dim TinT As FXR_TimeInTransitRequest = Create_TinT_Request(obj)
            'Dim TinT As FXR_TimeInTransitRequest = TESTING_Create_TinT_Request()

            Dim jsonPayload As String ' = JsonConvert.SerializeObject(TinT, Formatting.Indented)


            jsonPayload = JsonConvert.SerializeObject(TinT, Formatting.Indented, New JsonSerializerSettings With {
        .NullValueHandling = NullValueHandling.Ignore,
        .DefaultValueHandling = DefaultValueHandling.Ignore,
        .ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        .ContractResolver = ShouldSerializeContractResolver.Instance})


            request.AddParameter("application/x-www-form-urlencoded", jsonPayload, ParameterType.RequestBody)
            Dim response As IRestResponse = client.Execute(request)
            Debug.Print(jsonPayload.ToString)

            _FedExWeb.objFedEx_Setup = _FedExWeb.objFedEx_Regular_Setup
            System.IO.File.WriteAllText(gFedExSETUP.Path_Save_InOut_File & "/" & "TimeInTransit_Request.txt", jsonPayload.ToString)
            System.IO.File.WriteAllText(gFedExSETUP.Path_Save_InOut_File & "/" & "TimeInTransit_Response.txt", JObject.Parse(response.Content).ToString)


            If response.StatusCode = HttpStatusCode.OK Then

                result = JObject.Parse(response.Content)

                Debug.Print(result.ToString)


                Dim resolvedTinT_List As New List(Of FXR_rateReplyDetails)
                Dim resolvedDetail As FXR_rateReplyDetails
                Dim FoundItem As FXR_rateReplyDetails

                For Each DateDetail In result.SelectTokens("output.rateReplyDetails[*]")

                    resolvedDetail = New FXR_rateReplyDetails
                    resolvedDetail.serviceType = DateDetail.SelectToken("serviceType")
                    resolvedDetail.dateDetail.dayOfWeek = DateDetail.SelectToken("commit.dateDetail.dayOfWeek")
                    resolvedDetail.dateDetail.dayFormat = DateDetail.SelectToken("commit.dateDetail.dayFormat")

                    resolvedTinT_List.Add(resolvedDetail)

                Next

                For Each service As baseWebResponse_TinT_Service In shipments.AvailableServices
                    'If FXR_GetServiceType(item.ServiceCode) = 

                    FoundItem = resolvedTinT_List.Find(Function(x) x.serviceType = FXR_GetServiceType(service.ServiceCode))

                    If Not IsNothing(FoundItem) Then
                        service.ArrivalDate = FoundItem.dateDetail.dayFormat
                        service.ArrivalTransitTime = FoundItem.dateDetail.dayFormat
                        service.ArrivalDayOfWeek = FoundItem.dateDetail.dayOfWeek
                    End If

                Next
            Else
                MsgBox("Failed to process Time In Transit request!" & vbCrLf & vbCrLf & response.Content, vbExclamation)

            End If

        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Error processing FedEx Time in Transit request...")

        End Try
    End Sub

    Private Function Create_TinT_Request(ByRef obj As _baseShipment) As FXR_TimeInTransitRequest
        Try
            Dim TinT As FXR_TimeInTransitRequest = New FXR_TimeInTransitRequest


            TinT.accountNumber.value = gFedExSETUP.AccountNumber

            TinT.carrierCodes = New List(Of String)
            TinT.carrierCodes.Add("FDXE")
            TinT.carrierCodes.Add("FDXG")

            TinT.rateRequestControlParameters = New FXR_rateRequestControlParameters With {.returnTransitTimes = True}

            Populate_Contact(TinT.requestedShipment.shipper, obj.ShipFromContact, True)
            Populate_Contact(TinT.requestedShipment.recipient, obj.ShipToContact, False)
            TinT.requestedShipment.pickupType = "USE_SCHEDULED_PICKUP"
            TinT.requestedShipment.rateRequestType.Add("LIST")
            TinT.requestedShipment.rateRequestType.Add("ACCOUNT")

            Dim reqestLineItem = New FXR_RequestedPackageLineItem

            reqestLineItem.weight = New FXR_weight With {.units = obj.Packages(0).Weight_Units, .value = obj.Packages(0).Weight_LBs}

            'reqestLineItem.dimensions = New FXR_dimensions With {
            '    .height = obj.Packages(0).Dim_Height,
            '    .length = obj.Packages(0).Dim_Length,
            '    .width = obj.Packages(0).Dim_Width,
            '    .units = "IN"}

            reqestLineItem.customerReferences = New List(Of FXR_customerReferences)

            TinT.requestedShipment.requestedPackageLineItems.Add(reqestLineItem)

            Return TinT


        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Error preparing FedEx Time In Transit request.")
            Return Nothing
        End Try

    End Function

    Private Function TESTING_Create_TinT_Request() As FXR_TimeInTransitRequest
        Dim TinT As FXR_TimeInTransitRequest = New FXR_TimeInTransitRequest


        'TinT.accountNumber.value = gFedExSETUP.AccountNumber
        TinT.accountNumber.value = "604007038"


        TinT.carrierCodes = New List(Of String)
        TinT.carrierCodes.Add("FDXE")

        TinT.rateRequestControlParameters = New FXR_rateRequestControlParameters With {.returnTransitTimes = True}

        TinT.requestedShipment = New FXR_TinT_requestedShipment
        TinT.requestedShipment.shipper = New FXR_ShipperRecipient
        TinT.requestedShipment.shipper.address = New FXR_address With {.postalCode = 44202, .countryCode = "US"}

        TinT.requestedShipment.recipient = New FXR_ShipperRecipient
        TinT.requestedShipment.recipient.address = New FXR_address With {.postalCode = 99501, .countryCode = "US"}


        TinT.requestedShipment.pickupType = "USE_SCHEDULED_PICKUP"
        TinT.requestedShipment.rateRequestType.Add("LIST")
        TinT.requestedShipment.rateRequestType.Add("ACCOUNT")


        Dim reqestLineItem = New FXR_RequestedPackageLineItem

        reqestLineItem.weight = New FXR_weight With {.units = "LB", .value = 50}

        reqestLineItem.dimensions = New FXR_dimensions With {
            .height = 10,
            .length = 10,
            .width = 10,
            .units = "IN"}
        reqestLineItem.customerReferences = New List(Of FXR_customerReferences)

        TinT.requestedShipment.requestedPackageLineItems.Add(reqestLineItem)


        Return TinT
    End Function

#End Region


#Region "Ground Close"
    Public Sub FXR_EOD_CloseGround()

        Try
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12
            Dim client = New RestClient(gFedExSETUP.URL & "ship/v1/endofday/")
            Dim request = New RestRequest(Method.PUT)
            Dim result As JObject
            Dim labelFilePath As String
            Dim image As String

            Get_OAuth_Token(False)

            request.AddHeader("Authorization", "Bearer " & gFedExSETUP.OAuthToken)
            request.AddHeader("X-locale", "en_US")
            request.AddHeader("Content-Type", "application/json")


            Dim fxr As FXR_EODGroundRequest = New FXR_EODGroundRequest
            fxr.accountNumber.value = gFedExSETUP.AccountNumber
            fxr.closeDate = DateTime.Now.ToString("yyyy-MM-dd")
            fxr.closeReqType = "GCDR"
            fxr.groundServiceCategory = "GROUND"
            fxr.closeDocumentSpecification.closeDocumentTypes.Add("MANIFEST")


            Dim jsonPayload As String = JsonConvert.SerializeObject(fxr, Formatting.Indented)

            request.AddParameter("application/x-www-form-urlencoded", jsonPayload, ParameterType.RequestBody)
            Dim response As IRestResponse = client.Execute(request)
            Debug.Print(jsonPayload.ToString)

            System.IO.File.WriteAllText(gFedExSETUP.Path_Save_InOut_File & "/GroundClose_Request.txt", JObject.FromObject(fxr).ToString)
            System.IO.File.WriteAllText(gFedExSETUP.Path_Save_InOut_File & "/GroundClose_Response.txt", JObject.Parse(response.Content).ToString)


            If response.StatusCode = HttpStatusCode.OK Then

                result = JObject.Parse(response.Content)

                Debug.Print(result.ToString)

                image = result.SelectToken("output.closeDocuments[0].parts[0].image")

                _FedExWeb.objFedEx_Setup = _FedExWeb.objFedEx_Regular_Setup
                labelFilePath = gFedExSETUP.Path_Save_InOut_File & "\GroundEOD_Manifest.txt"

                If (_Files.WriteFile_ToEnd(_Convert.Base64String2Byte(image), labelFilePath)) Then
                    If (_Files.ReadFile_ToEnd(labelFilePath, False, image)) Then
                        'web_response.Packages(0).LabelImage = imgage
                    End If
                End If



                Dim pName As String = GetPolicyData(gReportsDB, "ReportPrinter")
                If pName = "" Then
                    pName = _Printers.Get_DefaultPrinter()
                End If

                _PrintReceipt.Print_FromFile(labelFilePath, pName, False)


            Else
                MsgBox("Failed to process EOD Ground Request!" & vbCrLf & vbCrLf & response.Content, vbExclamation)
            End If


        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Error preparing FedEx EOD Ground Close request.")

        End Try
    End Sub

#End Region
End Module


