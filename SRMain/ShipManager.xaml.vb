
Imports wgssSTU

Public Class ShipManager
    Inherits CommonWindow

    Private fDeclaredAsked As Boolean
    Private Display_CarrierList As List(Of Carrier)
    Private Current_Panel_View As String
    Private Shadows isLoaded As Boolean
    Private ReceiptOptionsSegment As String
    Public Shared PrintLabelScreen_Return As String
    Private auto_TinT As Boolean = False

    Public Sub New()

        MyBase.New()

        ' This call is required by the designer.
        InitializeComponent()


    End Sub

    Public Sub New(ByVal callingWindow As Window)

        MyBase.New(callingWindow)

        ' This call is required by the designer.
        InitializeComponent()
        load_ShipOne()

    End Sub

    Private Function load_ShipOne() As Boolean


        If isOpen_PackMaster = True Then
            Length_TxtBx.Text = ExtractElementFromSegment("Length", gShipmentParameters)
            Width_TxtBx.Text = ExtractElementFromSegment("Width", gShipmentParameters)
            Height_TxtBx.Text = ExtractElementFromSegment("Height", gShipmentParameters)
            Packing_Weight.Text = ExtractElementFromSegment("Weight", gShipmentParameters)
            Packing_Charge.Text = ExtractElementFromSegment("Charge", gShipmentParameters)
            Content.Text = ExtractElementFromSegment("Contents", gShipmentParameters)

            Length_TxtBx.Focus()
            Me.Focus()
        End If
    End Function


    Private Sub ShipManager_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded

        Shipper_Discounts_Btn.Visibility = Visibility.Hidden

        If Not InStr(1, gResult, "CQD") = 0 Then

            Dim SQL As String = "SELECT MAX(ID) AS MaxID FROM Contacts"
            Dim buf As String = IO_GetSegmentSet(gShipriteDB, SQL)
            Dim MaxID As Integer = Val(ExtractElementFromSegment("MaxID", buf))
            Dim SID As Integer
            Dim CID As Integer
            Dim ret As Long
            Dim SegmentSet As String
            Dim Segment As String

            SQL = "SELECT * FROM Contacts WHERE Name = '" & ExtractElementFromSegment("S_Name", gResult2) & "' AND Addr1 = '" & ExtractElementFromSegment("S_Addr1", gResult2) & "' AND City = '" & ExtractElementFromSegment("S_City", gResult2) & "' AND Phone = '" & ExtractElementFromSegment("S_Phone", gResult2) & "'"
            SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)

            If SegmentSet = "" Then
                SID = MaxID + 1
                Segment = ""
                Segment = AddElementToSegment(Segment, "ID", SID)
                Segment = AddElementToSegment(Segment, "Name", ExtractElementFromSegment("S_Name", gResult2))
                Segment = AddElementToSegment(Segment, "FName", ExtractElementFromSegment("S_FName", gResult2))
                Segment = AddElementToSegment(Segment, "LName", ExtractElementFromSegment("S_LName", gResult2))
                Segment = AddElementToSegment(Segment, "Addr1", ExtractElementFromSegment("S_Addr1", gResult2))
                Segment = AddElementToSegment(Segment, "Addr2", ExtractElementFromSegment("S_Addr2", gResult2))
                Segment = AddElementToSegment(Segment, "City", ExtractElementFromSegment("S_City", gResult2))
                Segment = AddElementToSegment(Segment, "State", ExtractElementFromSegment("S_State", gResult2))
                Segment = AddElementToSegment(Segment, "Zip", ExtractElementFromSegment("S_Zipcode", gResult2))
                Segment = AddElementToSegment(Segment, "Phone", ExtractElementFromSegment("S_Phone", gResult2))
                Segment = AddElementToSegment(Segment, "Class", "Shipper")
                If ExtractElementFromSegment("S_Name", gResult2) <> ExtractElementFromSegment("S_LName", gResult2) & ", " & ExtractElementFromSegment("S_FName", gResult2) Then
                    Segment = AddElementToSegment(Segment, "Residential", "0")
                Else
                    Segment = AddElementToSegment(Segment, "Residential", "-1")
                End If
                Segment = AddElementToSegment(Segment, "Country", "United States")
                Segment = AddElementToSegment(Segment, "FullAddress", Build_Full_Address("S", gResult2))
                Segment = AddElementToSegment(Segment, "FirstDate", DateTime.Today.ToShortDateString)
                Segment = AddElementToSegment(Segment, "LastDate", DateTime.Today.ToShortDateString)
                SQL = MakeInsertSQLFromSchema("Contacts", Segment, gContactsTableSchema, True)
            Else
                SID = Val(ExtractElementFromSegment("ID", SegmentSet))
                Segment = ""
                Segment = AddElementToSegment(Segment, "ID", SID)
                Segment = AddElementToSegment(Segment, "LastDate", DateTime.Today.ToShortDateString)
                SQL = MakeUpdateSQLFromSchema("Contacts", Segment, gContactsTableSchema, , True)
            End If
            ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

            SQL = "SELECT * FROM Contacts WHERE Name = '" & ExtractElementFromSegment("C_Name", gResult2) & "' AND Addr1 = '" & ExtractElementFromSegment("C_Addr1", gResult2) & "' AND City = '" & ExtractElementFromSegment("C_City", gResult2) & "' AND Phone = '" & ExtractElementFromSegment("C_Phone", gResult2) & "'"
            SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)

            If SegmentSet = "" Then
                If SID = MaxID + 1 Then
                    ' SID inserted
                    CID = SID + 1
                Else
                    ' SID updated
                    CID = MaxID + 1
                End If
                Segment = ""
                Segment = AddElementToSegment(Segment, "ID", CID)
                Segment = AddElementToSegment(Segment, "Name", ExtractElementFromSegment("C_Name", gResult2))
                Segment = AddElementToSegment(Segment, "FName", ExtractElementFromSegment("C_FName", gResult2))
                Segment = AddElementToSegment(Segment, "LName", ExtractElementFromSegment("C_LName", gResult2))
                Segment = AddElementToSegment(Segment, "Addr1", ExtractElementFromSegment("C_Addr1", gResult2))
                Segment = AddElementToSegment(Segment, "Addr2", ExtractElementFromSegment("C_Addr2", gResult2))
                Segment = AddElementToSegment(Segment, "City", ExtractElementFromSegment("C_City", gResult2))
                Segment = AddElementToSegment(Segment, "State", ExtractElementFromSegment("C_State", gResult2))
                Segment = AddElementToSegment(Segment, "Zip", ExtractElementFromSegment("C_Zipcode", gResult2))
                Segment = AddElementToSegment(Segment, "Phone", ExtractElementFromSegment("C_Phone", gResult2))
                Segment = AddElementToSegment(Segment, "Class", "Consignee")
                If ExtractElementFromSegment("Residential", gResult2) = "True" Then
                    Segment = AddElementToSegment(Segment, "Residential", "-1")
                Else
                    Segment = AddElementToSegment(Segment, "Residential", "0")
                End If
                Segment = AddElementToSegment(Segment, "Country", ExtractElementFromSegment("C_Country", gResult2))
                Segment = AddElementToSegment(Segment, "FullAddress", Build_Full_Address("C", gResult2))
                Segment = AddElementToSegment(Segment, "FirstDate", DateTime.Today.ToShortDateString)
                Segment = AddElementToSegment(Segment, "LastDate", DateTime.Today.ToShortDateString)
                SQL = MakeInsertSQLFromSchema("Contacts", Segment, gContactsTableSchema, True)
            Else
                CID = Val(ExtractElementFromSegment("ID", SegmentSet))
                Segment = ""
                Segment = AddElementToSegment(Segment, "ID", CID)
                Segment = AddElementToSegment(Segment, "LastDate", DateTime.Today.ToShortDateString)
                SQL = MakeUpdateSQLFromSchema("Contacts", Segment, gContactsTableSchema, , True)
            End If
            ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

            If Not Val(SID) = 0 Then

                Load_Shipper(SID)
                gCustomerSegment = gShipperSegment ' gShipperSegment loaded in Load_Shipper function

            End If

            If Not Val(CID) = 0 Then

                Load_Consignee(CID)

            End If

            DeclaredValue.Text = Mid(ExtractElementFromSegment("DecVal", gResult2), 2)
            Content.Text = ExtractElementFromSegment("Contents", gResult2)
        Else
            ShipToContact = Nothing
        End If

        isLoaded = False

        gContactManagerSegment = ""

        Display_CarrierList = New List(Of Carrier)

        Packaging_ComboBox.Items.Add("Other")
        Packaging_ComboBox.Items.Add("Letter")
        ' Packaging_ComboBox.Items.Add("FedEx Freight® Small Box")
        ' Packaging_ComboBox.Items.Add("FedEx Freight® Large Box")
        Packaging_ComboBox.SelectedIndex = 0

        Call load_Countries()
        Call Reset_Hidden_Carrier_Settings()

        Current_Panel_View = "Domestic"

        Dim custID As String = ExtractElementFromSegment("ID", gCustomerSegment)
        If custID.Length > 0 Then
            'Display customer that was pulled up in POS as Shipper.
            Load_Shipper(custID)
        End If

        If Customs.CustomsList IsNot Nothing Then
            Customs.CustomsList = Nothing
        End If


        gShip = Nothing
        gShip = New gShip_Class
        gShip.PackagingType = Me.Packaging_ComboBox.Text
        gShipCT = 0

        gManifestSchema = IO_GetFieldsCollection(gShipriteDB, "Manifest", "", True, False, True)

        If String.IsNullOrEmpty(Me.Shipper.Text) AndAlso Not 0 = _Contact.ShipperContact.ContactID Then
            'Default Ship From
            Load_Shipper(_Contact.ShipperContact.ContactID)
        End If
        '
        Me.ThirdPartyInsurance.Text = _Convert.Boolean2OnOff(Definitions.gThirdPartyInsurance).ToUpper
        '
        Country.Text = "United States"

        Consignee.Focus()
        ReceiptOptionsSegment = GetReceiptOptions()
        FedExFlatRate_Button.Visibility = Visibility.Hidden
        _FedExWeb.IsEnabled_OneRate = False

        If gIsCustomerDisplayEnabled Then
            gCustomerDisplay.ChangeTab(2)
        End If

        If gCallingSKU = "SHIPL" Then
            Packaging_ComboBox.SelectedIndex = 1
        End If

        If (Today.DayOfWeek = DayOfWeek.Saturday) Then
            SatDelivery_Btn.Content = "Saturday" & vbCrLf & "  Pickup"
        Else
            SatDelivery_Btn.Content = "Saturday" & vbCrLf & "Delivery"
        End If

        Load_Saved_PackJob()

        ' Scale
        'ConnectedScale.IsWeightKeyed = False ' initialized when ConnectedScale object constructed
        'LoadScale() ' loaded when window activated
        isLoaded = True
    End Sub

    Private Function Load_Saved_PackJob()
        Saved_Packjob.Items.Clear()


        Dim SegmentSet As String = ""
        Dim Segment As String
        SegmentSet = IO_GetSegmentSet(gShipriteDB, "SELECT Contents from Contents")
        Do Until SegmentSet = ""
            Segment = GetNextSegmentFromSet(SegmentSet)
            Saved_Packjob.Items.Add(ExtractElementFromSegment("Contents", Segment))


        Loop

    End Function

    Private Function Build_Full_Address(PersonType As String, Segment As String) As String

        Dim field As String
        Dim prefix As String = PersonType & "_"

        field = ExtractElementFromSegment(prefix & "Addr1", Segment) & vbCrLf
        If ExtractElementFromSegment(prefix & "Addr2", Segment) <> "" Then
            field = field & ExtractElementFromSegment(prefix & "Addr2", Segment) & vbCrLf
        End If
        field = field & ExtractElementFromSegment(prefix & "City", Segment) & ", " & ExtractElementFromSegment(prefix & "State", Segment) & " " & ExtractElementFromSegment(prefix & "Zipcode", Segment)

        Return field

    End Function

    Private Sub ShipManager_Activated(sender As Object, e As EventArgs) Handles Me.Activated
        LoadScale()
        auto_TinT = GetPolicyData(gShipriteDB, "Enable_Auto_TimeInTransit", "False")
    End Sub

    Private Sub load_Countries()
        For Each ctry As _CountryDB In gCountry
            Me.Country.Items.Add(ctry)
        Next
    End Sub

    Private Sub ShipManager_Deactivated(sender As Object, e As EventArgs) Handles Me.Deactivated
        StopScale()
    End Sub

    Private Sub Reset_Hidden_Carrier_Settings()
        Dim current_segment As String


        For Each current_Carrier In gCarrierList
            current_segment = IO_GetSegmentSet(gShipriteDB, "SELECT Carrier, Domestic_Status, Intl_Status, Canada_Status, Freight_Status from Master WHERE Carrier='" & current_Carrier.CarrierName & "'")

            current_Carrier.Status_Domestic = ExtractElementFromSegment("Domestic_Status", current_segment, "0")
            current_Carrier.Status_Intl = ExtractElementFromSegment("Intl_Status", current_segment, "0")
            current_Carrier.Status_Canada = ExtractElementFromSegment("Canada_Status", current_segment, "0")
            current_Carrier.Status_Freight = ExtractElementFromSegment("Freight_Status", current_segment, "0")
            current_Carrier.Selected_Pack_Item = Nothing

        Next

    End Sub


#Region "Shipping_Calculations"

    Public Sub GetRealShippingTimes()
        Dim shipment As _baseShipment
        Dim fedExServices As New baseWebResponse_TinT_Services

        If Country.Text <> "United States" Then Exit Sub

        shipment = New _baseShipment
        shipment.CarrierService.CarrierName = "FEDEX"
        Prepare_ShipmentFromScreen(shipment)
        create_AvailableServicesCollection_FedEx(fedExServices)

        If GetPolicyData(gShipriteDB, "FedExREST_Enabled", "False") Then
            FedEx_REST.Process_TinT_Request(shipment, fedExServices)
        Else
            _FedExWeb.Process_Rate_TimeInTransit(shipment, fedExServices)
        End If


        Dim upsServices As New baseWebResponse_TinT_Services
        shipment = New _baseShipment
        shipment.CarrierService.CarrierName = "UPS"
        Prepare_ShipmentFromScreen(shipment)
        create_AvailableServicesCollection_UPS(upsServices, shipment.ShipperContact.Country)
        If _UPSWeb.IsUPSWebServicesEnabled Then
            _UPSWeb.Process_TimeInTransit(shipment, upsServices)
        End If

        For Each CR As Carrier In Display_CarrierList
            Select Case CR.CarrierName
                Case "FedEx"
                    For Each svc In CR.ServiceList
                        For Each nsvc In fedExServices.AvailableServices
                            If (svc.Service = nsvc.ServiceCode) Then
                                svc.DeliveryDate = nsvc.ArrivalDate
                                If GetPolicyData(gShipriteDB, "FedExREST_Enabled", "False") Then
                                    If svc.DeliveryDate.Year = 1 Then
                                        svc.IsButtonVisible = Visibility.Hidden
                                    End If

                                Else
                                    If svc.DeliveryDate.CompareTo(DateTime.Today) < 0 Then
                                        svc.IsButtonVisible = Visibility.Hidden
                                    End If
                                End If

                                Exit For
                            End If
                        Next
                    Next
                Case "UPS"
                    For Each svc In CR.ServiceList
                        For Each nsvc In upsServices.AvailableServices
                            If (svc.Service = nsvc.ServiceCode) Then
                                svc.DeliveryDate = nsvc.ArrivalDate
                                If svc.DeliveryDate.CompareTo(DateTime.Today) < 0 Then
                                    svc.IsButtonVisible = Visibility.Hidden
                                End If
                                Exit For
                            End If
                        Next
                    Next
            End Select
        Next
        SortShippingCalculations()
        ShippingPanel_IC.ItemsSource = Display_CarrierList
        ShippingPanel_IC.Items.Refresh()
    End Sub

    Private Sub create_AvailableServicesCollection_FedEx(ByRef obj As baseWebResponse_TinT_Services)
        Call create_AvailableServiceObject(FedEx.Ground, obj)
        Call create_AvailableServiceObject(FedEx.CanadaGround, obj)
        Call create_AvailableServiceObject(FedEx.FirstOvernight, obj)
        Call create_AvailableServiceObject(FedEx.SecondDay, obj)
        Call create_AvailableServiceObject(FedEx.SecondDayAM, obj)
        Call create_AvailableServiceObject(FedEx.Priority, obj)
        Call create_AvailableServiceObject(FedEx.Standard, obj)
        Call create_AvailableServiceObject(FedEx.Saver, obj)
        Call create_AvailableServiceObject(FedEx.Intl_First, obj)
        Call create_AvailableServiceObject(FedEx.Intl_Priority, obj)
        Call create_AvailableServiceObject(FedEx.Intl_Economy, obj)
        Call create_AvailableServiceObject(FedEx.Freight_1Day, obj)
        Call create_AvailableServiceObject(FedEx.Freight_2Day, obj)
        Call create_AvailableServiceObject(FedEx.Freight_3Day, obj)
    End Sub

    Private Sub create_AvailableServicesCollection_UPS(ByRef obj As baseWebResponse_TinT_Services, ByVal destCountry As String)
        'Call create_AvailableServiceObject("COM-GND", obj)
        'Call create_AvailableServiceObject("1DAY", obj)
        'Call create_AvailableServiceObject("1DAYSVR", obj)
        'Call create_AvailableServiceObject("2DAYAM", obj)
        'Call create_AvailableServiceObject("2DAY", obj)
        'Call create_AvailableServiceObject("3DAYSEL", obj)
        'Call create_AvailableServiceObject("1DAYEAM", obj)

        Dim destCountCaps = destCountry.ToUpper

        If _IDs.IsIt_CanadaShipper And destCountCaps = "CANADA" Then
            ' Canada Origin Domestic
            Call create_AvailableServiceObject("STD", obj) '  UPS Standard
            Call create_AvailableServiceObject("1DAYEAM", obj) '  UPS Express Early
            Call create_AvailableServiceObject("XPED", obj) '  UPS Expedited
            Call create_AvailableServiceObject("SVR", obj) '  UPS Express Saver
            Call create_AvailableServiceObject("XPRES", obj) '  UPS Express
            '
        ElseIf _IDs.IsIt_CanadaShipper And destCountCaps = "UNITED STATES" Then
            ' Canada Origin to USA
            Call create_AvailableServiceObject("USA-1DAYEAM", obj) '  UPS Express Early
            Call create_AvailableServiceObject("USA-XSVR", obj) '  UPS Express Saver
            Call create_AvailableServiceObject("USA-3DAYSEL", obj) '  UPS 3 Day Select
            Call create_AvailableServiceObject("USA-STD", obj) '  UPS Standard
            Call create_AvailableServiceObject("USA-XPRES", obj) '  UPS Worldwide Express
            Call create_AvailableServiceObject("USA-XPED", obj) '  UPS Worldwide Expedited
            '
        ElseIf _IDs.IsIt_USAShipper And destCountCaps = "CANADA" Then
            ' USA Origin to Canada
            Call create_AvailableServiceObject("CAN-XPRES", obj)
            Call create_AvailableServiceObject("CAN-XSVR", obj)
            Call create_AvailableServiceObject("CAN-XPED", obj)
            Call create_AvailableServiceObject("CAN-STD", obj)
            '
        Else
            ' USA Domestic, Intl. Can Intl.
            Call create_AvailableServiceObject("1DAYEAM", obj)
            Call create_AvailableServiceObject("1DAY", obj)
            Call create_AvailableServiceObject("1DAYSVR", obj)
            Call create_AvailableServiceObject("2DAYAM", obj)
            Call create_AvailableServiceObject("2DAY", obj)
            Call create_AvailableServiceObject("3DAYSEL", obj)
            Call create_AvailableServiceObject("COM-GND", obj)
            '
            Call create_AvailableServiceObject("WWXPRES", obj)
            Call create_AvailableServiceObject("WWXSVR", obj)
            Call create_AvailableServiceObject("WWXPED", obj)
            '
        End If
    End Sub

    Public Shared Function GetShippingSellingPrice(Master As MasterShippingTable, Chg As Double, GetRetailMarkup As Boolean, Weight As Double, Zone As String, isLetter As Boolean) As Double

        Dim SellingPrice As Double
        Dim Level As Integer
        Dim i As Integer
        Dim Markup As Double

        If GetPolicyData(gShipriteDB, "Enable_Pricing_Matrix", "False") Then
            'Pricing Matrix
            Markup = GetMarkupFromMatrix(Master, Weight, Zone, isLetter)

            If Markup = 1.2345678 Then
                'No matching Letter Markup in Matrix, use default
                Return GetLetterMarkup(Master, Chg)
            End If


        ElseIf GetRetailMarkup And isLetter = False Then
            'Retail Markup
            Markup = Master.LevelR


        ElseIf isLetter = False Then
            'Regular Markup
            For i = 0 To 2

                If Chg >= gProfitRange(i).LO And Chg <= gProfitRange(i).HI Then
                    Exit For
                End If
            Next

            Level = i

            Select Case i
                Case 0
                    Markup = Master.Level1
                Case 1
                    Markup = Master.Level2
                Case 2
                    Markup = Master.Level3
            End Select

        Else
            'Letter Markup
            Return GetLetterMarkup(Master, Chg)
        End If


        SellingPrice = Chg * (1 + (Markup / 100))
        SellingPrice = Round(SellingPrice, 2)
        Return SellingPrice

    End Function

    Public Shared Function GetLetterMarkup(Master As MasterShippingTable, Chg As Double) As Double
        Dim SellingPrice As Double = 0
        If Master.LetterFee <> 0 Then
            'Flat Fee markup
            Return Chg + Master.LetterFee

        Else
            'percentage markup
            SellingPrice = Chg * (1 + (Master.LetterPercentage / 100))
            Return Round(SellingPrice, 2)
        End If
    End Function


    Public Shared Function GetMarkupFromMatrix(Master As MasterShippingTable, weight As Double, zone As String, isLetter As Boolean) As Double

        Dim MatrixItem As PricingMatrixItem = Nothing

        If isLetter Then
            MatrixItem = gPricingMatrix.Where(Function(x) (x.Service = Master.ServiceTable And (x.WeightStart = "LETTER" Or x.WeightEnd = "LETTER")) And (x.Zone.Contains("," & zone & ",") Or x.Zone.Contains("ALL"))).FirstOrDefault
        Else

            If weight > 150 Then weight = 150

            For Each item As PricingMatrixItem In gPricingMatrix
                If item.Service = Master.ServiceTable Then
                    If item.WeightStart <> "LETTER" Then
                        If item.WeightStart = "ALL" OrElse (CDbl(item.WeightStart) <= weight And CDbl(item.WeightEnd) >= weight) Then
                            If item.Zone.Contains("," & zone & ",") Or item.Zone.Contains("ALL") Then
                                MatrixItem = item
                                Exit For
                            End If
                        End If
                    End If
                End If
            Next

        End If


        If IsNothing(MatrixItem) Then
            'no entry in pricing matrix, use default//

            If isLetter Then
                Return 1.2345678
            Else
                Return Master.Level1
            End If

            Return 0
        Else

            Return MatrixItem.Markup
        End If

    End Function

    Private Function Get_FirstClass_Flat_Retail(Weight_Oz As Double)
        Return Val(ExtractElementFromSegment("RETAIL-Flat", IO_GetSegmentSet(gPackagingDB, "Select [RETAIL-Flat] From FirstClassRetail WHERE WEIGHT=" & Math.Ceiling(Weight_Oz))))

    End Function

    Public Shared Function Get_FirstClass_ShippingCost(Zone As String, Weight_Oz As Double, Packaging As Integer, svc As String, Optional isComm As Boolean = False) As Double

        If Not String.IsNullOrWhiteSpace(Zone) AndAlso Zone.ToUpper.Replace("ZONE", "").Length > 0 Then
            If svc = "FirstClass" Then
                If Packaging = 0 Then
                    'Other
                    'First Class Package is discontinues and replaced by Retail Ground service.
                    'Return Val(ExtractElementFromSegment("COST-Pack-" & Zone, IO_GetSegmentSet(gUSMailDB_Services, "Select [COST-Pack-" & Zone & "] From FirstClass WHERE WEIGHT=" & Math.Ceiling(Weight_Oz))))
                Else
                    'Letter
                    Return Val(ExtractElementFromSegment("COST-Flat", IO_GetSegmentSet(gUSMailDB_Services, "Select [COST-Flat] From FirstClass WHERE WEIGHT=" & Math.Ceiling(Weight_Oz))))
                End If

            Else
                'First Class International
                If Packaging = 0 Then
                    'Other
                    Dim tblName As String = "USPS-INTL-FCMI"
                    If isComm Then
                        tblName &= "_Commercial"
                    End If
                    Return Val(ExtractElementFromSegment("Cost", IO_GetSegmentSet(gUSMailDB_Services, "Select min(" & Zone & ") as Cost From [" & tblName & "] WHERE OZS >= " & Math.Ceiling(Weight_Oz))))
                Else
                    'Letter
                    Return Val(ExtractElementFromSegment("Cost", IO_GetSegmentSet(gUSMailDB_Services, "Select min(" & Zone & ") as Cost From [USPS-INTL-FCMI_Flats] WHERE OZS >= " & Math.Ceiling(Weight_Oz))))
                End If

            End If
        Else
            Return 0
        End If

    End Function

    Private Function Get_FedEx_FlatRateCost(ByRef svc As ShippingChoiceDefinition)

        If gFedEx_OneRate_Tables.Contains(svc.Service & "-OneRate") Then
            Dim SQL As String = "SELECT " & svc.Zone & " FROM [" & svc.Service & "-OneRate] WHERE PackagingType='" & svc.Packaging.SettingName & "'"
            Return Val(ExtractElementFromSegment(svc.Zone, IO_GetSegmentSet(gFedExServicesDB, SQL), "0"))
        Else
            Return 0
        End If


    End Function

    Public Shared Function GetShippingCost(ServiceTable As String, Zone As String, TheWeight As Double, ByRef DeliveryDate As Date, Optional ServiceDBPath As String = "") As Double

        Dim s As Integer
        Dim i As Integer
        Dim cost As Double
        Dim ColumnNumber As Integer
        Dim DaysInTransit As Integer


        ColumnNumber = 0

        If ServiceDBPath = "" Then
            For s = 0 To gSVCct - 1
                If gServiceTables(s).ServiceName = ServiceTable Then
                    Exit For
                End If
            Next

        Else
            For s = 0 To gSVCct - 1
                If gServiceTables(s).ServiceName = ServiceTable And UCase(gServiceTables(s).dpPath) = UCase(ServiceDBPath) Then
                    Exit For
                End If
            Next
        End If


        If s = gSVCct Then
            'MsgBox("ATTENTION...Service Table [" & ServiceTable & "] NOT FOUND!!!")
            Return 0
        Else
            For ColumnNumber = 0 To gServiceTables(s).cCT - 1
                If gServiceTables(s).ColumnNames(ColumnNumber) = Zone Then
                    Exit For
                End If
            Next
            If ColumnNumber = gServiceTables(s).cCT Then
                Return 0
            Else
                If Not FedEx_Freight.IsFreightBoxPackaging(gShip.PackagingType) Then
                    If FedEx_Freight.IsFreight_123Day_Service(ServiceTable) Then
                        '
                        Dim weightBeak As Int16 = 0
                        If FedEx_Freight.get_FreightWeightBrake(TheWeight, weightBeak) Then
                            '
                            ' FR1, FR2, FR3
                            For i = 0 To gServiceTables(s).RecordCount - 1
                                If weightBeak = gServiceTables(s).Rates(i).Zones(0) Then

                                    cost = gServiceTables(s).Rates(i).Zones(ColumnNumber)
                                    cost = cost * TheWeight
                                    Exit For

                                End If
                            Next
                            '
                        End If
                        '
                    Else
                        '
                        For i = 0 To gServiceTables(s).RecordCount - 1
                            If TheWeight = gServiceTables(s).Rates(i).Zones(0) Then
                                cost = gServiceTables(s).Rates(i).Zones(ColumnNumber)
                                Exit For
                            End If
                        Next
                        '
                    End If
                End If
            End If
            cost = Round(cost, 2)

            If gServiceTables(s).Rates(0).Zones(0) = -1 Then
                DaysInTransit = gServiceTables(s).Rates(0).Zones(ColumnNumber)
            Else
                DaysInTransit = 0
            End If


            If DaysInTransit <> 0 Then

                Dim index = Find_Master_Index(ServiceTable)

                If index <> -1 Then
                    If gMaster(index).PickupDate <> Nothing And gMaster(index).PickupDate.ToShortDateString <> #1/1/0001# And gMaster(index).PickupDate > Today.Date Then

                        DeliveryDate = gMaster(index).PickupDate.AddDays(DaysInTransit)
                    Else
                        DeliveryDate = Today.AddDays(DaysInTransit)
                    End If
                Else
                    DeliveryDate = Today.AddDays(DaysInTransit)
                End If
            Else
                DeliveryDate = Nothing
            End If

            Return cost
        End If

    End Function

    Public Function GetShippingCost_Freight(ByVal ServiceTable As String, ByVal Zone As String, ByVal TheWeight As Integer, ByRef DeliveryDate As Date) As Double
        Dim cost As Double = 0

        Dim weightBeak As Int16 = 0
        If FedEx_Freight.get_FreightWeightBrake(TheWeight, weightBeak) Then
            If FedEx_Freight.IsFreightBoxPackaging(gShip.PackagingType) Then
                '
                ' Freight BOX
                If Not TheWeight > 1200 Then
                    If FedEx_Freight.IsFreightLTLService(ServiceTable) Then
                        If FedEx_Freight.Get_FlatBoxCharge(ServiceTable, Zone, cost) Then
                            ' got the cost.
                        End If
                    End If
                End If
                '
            ElseIf FedEx_Freight.IsFreightLTLService(ServiceTable) Then
                '
                Dim surcharges As Double = 0
                If FedEx_Freight.Get_ChargeAndSurcharge(Me.ZipCode.Text, ServiceTable, cost, surcharges) Then
                    cost += surcharges ' Total Net Freight + Total Surcharges
                Else
                    ' get base rate from Web Server
                    If FedEx_Freight.LTL_Freight IsNot Nothing AndAlso FedEx_Freight.LTL_Freight.FreightFormItems IsNot Nothing Then
                        Dim objResponse As New baseWebResponse_TinT_Services
                        If Execute_Freight_Rate(ServiceTable, objResponse) Then
                            ' read response
                            _Debug.Stop_("Read Freight rate from FedEx Web Server...")
                            Dim FRate As New baseWebResponse_TinT_Service
                            If objResponse.TimeInTransitAlerts.Count > 0 Then
                                _MsgBox.WarningMessage(objResponse.TimeInTransitAlerts(0), "Failed to get " & ServiceTable & " rates...")
                            End If
                            If objResponse.AvailableServices.Count > 0 Then
                                FRate = objResponse.AvailableServices(0)
                                _Debug.Print_(ServiceTable & " Freight charge = " & FRate.TotalBaseCharge)
                                _Debug.Print_(ServiceTable & " Freight surcharge = " & FRate.TotalSurcharges)
                                ' Return total of freight base + surcharges as shipping charge.
                                cost = FRate.TotalBaseCharge + FRate.TotalSurcharges
                            End If
                            '
                        End If
                    End If
                    '
                End If
                '
            End If
        End If

        Return cost

    End Function

    Private Function Execute_Freight_Rate(ByVal ServiceABBR As String, ByRef objResponse As baseWebResponse_TinT_Services) As Boolean
        Execute_Freight_Rate = False ' assume.
        Dim objShipment As New _baseShipment
        objFedEx_Setup = objFedEx_Regular_Setup
        '
        If FedEx.IsWebServicesReady Then
            ' 
            objShipment.CarrierService.CarrierName = "FEDEX"
            objShipment.CarrierService.ServiceABBR = ServiceABBR
            objShipment.CarrierService.IsAir = True
            If Prepare_ShipmentFromScreen(objShipment) Then
                '
                If 0 = Len(_Contact.ShipperContact.AccountNumber) Then
                    _Contact.ShipperContact.AccountNumber = objFedEx_Setup.Client_AccountNumber
                    objShipment.ShipperContact.AccountNumber = objFedEx_Setup.Client_AccountNumber
                End If
                '
                objResponse = New baseWebResponse_TinT_Services
                If create_AvailableServiceObject(objShipment.CarrierService.ServiceABBR, objResponse) Then
                    If _FedExWeb.Process_Rate_Freight(gFedExServicesDB, objShipment, objResponse) Then
                        If objResponse.AvailableServices.Count > 0 Then
                            _Debug.Print_(objResponse.AvailableServices(0).TotalBaseCharge)
                            Execute_Freight_Rate = True
                        End If
                    End If
                End If
                '
            End If
        End If
        '
    End Function

    Private Function create_AvailableServiceObject(ByVal serviceCode As String, ByRef obj As baseWebResponse_TinT_Services) As Boolean
        Dim rescommit As New baseWebResponse_TinT_Service
        rescommit.ServiceCode = serviceCode
        obj.AvailableServices.Add(rescommit)
        Return True
    End Function

    Public Function Prepare_ShipmentFromScreen(ByRef objShipment As _baseShipment) As Boolean
        Prepare_ShipmentFromScreen = False ' assume.
        '
        Dim objShipperInfo As New _baseContact
        Dim objShipFromInfo As New _baseContact
        Dim objShipToInfo As New _baseContact
        Dim objCarrierSurchargeCOD As New _baseServiceSurchargeCOD
        Dim objPackage As _baseShipmentPackage
        Dim result As String = String.Empty
        Dim response As String = String.Empty

        Dim DeliveryDate As String = String.Empty
        Dim DeliveryDay As String = String.Empty
        Dim TrackingNo As String = String.Empty
        Dim LabelImage As String = String.Empty
        Dim daysNo As String = String.Empty
        '
        objShipment.ShipperContact = _Contact.ShipperContact
        objShipment.ShipFromContact = _Contact.ShipperContact
        '

        If Me.Consignee.Tag IsNot Nothing AndAlso Not Me.Consignee.Tag = _Contact.ShipToContact.ContactID Then
            Call _Contact.Load_ContactFromDb(Me.Consignee.Tag, _Contact.ShipToContact)
        End If
        objShipment.ShipToContact = _Contact.ShipToContact

        If IsNothing(objShipment.ShipToContact) Then
            'set zip/country when doing a quote without a shipto address being pulled up.
            objShipment.ShipToContact = New _baseContact
            objShipment.ShipToContact.Country = Country.Text
            objShipment.ShipToContact.Zip = ZipCode.Text
        End If

        objShipment.ShipFromContact.CountryCode = _Contact.Get_CountryCodeFromCountryName(objShipment.ShipFromContact.Country)
        objShipment.ShipToContact.CountryCode = _Contact.Get_CountryCodeFromCountryName(objShipment.ShipToContact.Country)
        If gShip IsNot Nothing Then
            objShipment.ShipToContact.Residential = gShip.Residential
        End If
        '
        With objShipment
            .Comments = "TinT Request" '"Comments go here"
            .RateRequestType = "ACCOUNT" ' List or Account
            '
            .CarrierService.IsDomestic = gShip.Domestic
            .CarrierService.ShipDate = DateAndTime.Today 'gM(MI).ShipDate : ToDo: Ship date could a future date, we need variable
            '
            '  Could be any carrier: set these before entering the function
            '.CarrierService.CarrierName = "FEDEX" 
            '.CarrierService.ServiceABBR = gM(MI).SERVICE
            '.CarrierService.IsAir = gM(MI).AirService
            '
        End With
        '
        objPackage = New _baseShipmentPackage
        If Not ShipriteStartup.IsOn_gThirdPartyInsurance(objShipment.ShipToContact.Country, objShipment.CarrierService.CarrierName, objShipment.CarrierService.ServiceABBR, Val(Me.DeclaredValue.Text)) Then
            objPackage.DeclaredValue = Val(Me.DeclaredValue.Text)
            objPackage.Currency_Type = _IDs.CurrencyType
        End If
        objPackage.Dim_Height = Val(Me.Height_TxtBx.Text)
        objPackage.Dim_Length = Val(Me.Length_TxtBx.Text)
        objPackage.Dim_Width = Val(Me.Width_TxtBx.Text)
        If gShip IsNot Nothing Then
            objPackage.PackageID = gShip.PackageID
        End If
        objPackage.PackagingType = Me.Packaging_ComboBox.Text
        objPackage.Weight_LBs = Val(Me.Weight.Text)
        objPackage.Weight_Units = objPackage.Weight_Units
        '
        Call Prepare_ServiceSurchargesFromScreen_Package(objShipment, objPackage)
        '
        objShipment.Packages.Add(objPackage)
        '
        ' Surcharges:
        Call Prepare_ServiceSurchargesFromScreen(objShipment)
        '
        If Not objShipment.CarrierService.IsDomestic Or "PR" = objShipment.ShipToContact.CountryCode Then
            ' re-use FedEx international setup of the Commercial Invoice
            objShipment.CarrierService.IsDomestic = False
            Dim Pack As New _baseShipmentPackage
            Pack.Weight_Units = objPackage.Weight_Units
            'ToDo: Call FedEx_.Prepare_InternationalData(Nothing, Pack, objShipment)
            _Debug.Print_("Packages Count = " & objShipment.Packages.Count)
        End If

        ' FedEx Freight Box services were added.
        Call FedEx_Freight.Create_FreightItemsObject(objShipment)
        '
        _Debug.Print_("Package Count = " & objShipment.Packages.Count)
        '
        For i As Int16 = 0 To objShipment.CarrierService.ServiceSurcharges.Count - 1
            Dim servcharge As _baseServiceSurcharge = objShipment.CarrierService.ServiceSurcharges(i)
            _Debug.Print_("objCarrierSurcharge.Description = " & servcharge.Description)
        Next i
        ''
        Prepare_ShipmentFromScreen = True
        '
    End Function

    Public Function Prepare_ServiceSurchargesFromScreen(ByRef objShipment As _baseShipment) As Boolean
        '' TODO: Finish converting to using accessorial values from screen where available.
        '
        Prepare_ServiceSurchargesFromScreen = False ' assume.
        '
        '' FedEx - Service Level: COD (Express Services)
        Dim objCOD As New _baseServiceSurchargeCOD
        'objCOD.Amount = Val(ExtractElementFromSegment("CODAMT", SegmentSet))
        objCOD.Amount = Val(COD_Btn.Tag)
        If objCOD.Amount > 0 AndAlso objShipment.CarrierService.CarrierName = "FEDEX" AndAlso Not _FedExWeb.IsGroundService(objShipment.CarrierService.ServiceABBR) Then
            '
            _Debug.Stop_("COD Shipment Level = " & objCOD.Amount)
            objShipment.CarrierService.ServiceSurcharges.Add(New _baseServiceSurcharge(0, "COD", "COD", True))
            With objCOD
                If _IDs.IsIt_CanadaShipper And objShipment.CarrierService.IsDomestic Then
                    .CurrencyType = "CAD"
                Else
                    .CurrencyType = "USD"
                End If
                ' TODO: Once these features are actually written, add this back in.
                '    .ChargeType = "" 'COD Recipient AccountNumber
                '    If CBool(ExtractElementFromSegment("CashiersCheck", SegmentSet, "False")) Then
                '        .PaymentType = "0" '' GUARANTEED_FUNDS
                '    Else
                '        ' COD with "certified check or money order only" unchecked should have 'ANY' payment type submitted to FedEx Web Server.
                '        ' .PaymentType = "1" ' CASH
                '        .PaymentType = "2" '' ANY 
                '    End If
                '    ' FedEx COD amount should stay as entered by the user since all the calculations FedEx Web Services will do automatically.
                '    .Amount = Val(ExtractElementFromSegment("CODAMTwSHIP", SegmentSet))
            End With
            '
            objShipment.CarrierService.ServiceSurchargeCOD = objCOD
        End If
        '
        '' FedEx/UPS - Service Level: Hold At Location
        ' TODO: Can't seem to find the replacement for 'ABHoldAtAirport'
        Dim holdID As Long = 0
        'Dim holdID As Long = Val(ExtractElementFromSegment("ABHoldAtAirport", SegmentSet))
        If Not 0 = holdID Then
            '
            objShipment.CarrierService.ServiceSurcharges.Add(New _baseServiceSurcharge(0, "HOLD_AT_LOCATION", "HOLD_AT_LOCATION", True))
            '
            If FedExCERT.IsFedExTestAccount Then
                '
                objShipment.HoldAtLocation = _Contact.HoldAtContact
                '_Contact.HoldAtContact.AccountNumber = "OLVAD"
                _Debug.Print_("Hold at Location country code: " & objShipment.HoldAtLocation.CountryCode)
                '
            Else
                Call _Contact.Load_ContactFromDb(holdID, objShipment.HoldAtLocation)
                '
            End If
            '
        End If
        '
        '' FedEx - Service Level: One Rate
        ' 'FedEx One Rate' (flat rate for certain FedEx packaging) was added to the Buttons'Panel in ShipMaster.
        If objShipment.CarrierService.CarrierName = "FEDEX" AndAlso _FedExWeb.IsEnabled_OneRate Then
            _Debug.Stop_("FEDEX_ONE_RATE")
            objShipment.CarrierService.ServiceSurcharges.Add(New _baseServiceSurcharge(0, "FEDEX_ONE_RATE", "FEDEX_ONE_RATE", True))
        End If
        '
        '' FedEx/UPS - Service Level: Email Notification - EMAIL_NOTIFICATION changed to EVENT_NOTIFICATION in FedEx_Data2XML.GetShipmentSpecialServiceType()
        If Not objShipment.Comments = "TinT Request" Then
            If Not 0 = Len(objShipment.ShipFromContact.Email) Or Not 0 = Len(objShipment.ShipToContact.Email) Then
                objShipment.CarrierService.ServiceSurcharges.Add(New _baseServiceSurcharge(0, "EMAIL_NOTIFICATION", "6", True))
                'FedEx: objShipment.CarrierService.ServiceSurcharges.Add( add_ServiceSurcharge(0, "EMAIL_NOTIFICATION", "EMAIL_NOTIFICATION", True))
            End If
        End If
        '
        '' FedEx/UPS - Service Level: Future Day Shipment
        If DateTime.Today < objShipment.CarrierService.ShipDate Then
            objShipment.CarrierService.ServiceSurcharges.Add(New _baseServiceSurcharge(0, "FUTURE_DAY_SHIPMENT", "FUTURE_DAY_SHIPMENT", True))
        End If
        '
        ' TODO: These fields in the SegmentSet don't appear to have accessable analogs at this time. When those are written
        '           they need to be added.
        '' FedEx - Service Level: FedEx Home Delivery Options
        'If objShipment.CarrierService.CarrierName.ToUpper = "FEDEX" Then
        '    If Not 0 = Val(ExtractElementFromSegment("costFedEXHDCertain", SegmentSet)) Then
        '        objShipment.CarrierService.ServiceSurcharges.Add(New _baseServiceSurcharge(0, "HOME_DELIVERY_PREMIUM", "DATE_CERTAIN", True))
        '        ' User defined Delivery Date has to be transferred to FedEx Web Services in case of HomeDelivery Certain.
        '        If _Date.IsDate_(ExtractElementFromSegment("FEDEXDeliveryDate", SegmentSet)) Then
        '            objShipment.CarrierService.DeliveryDate = _Convert.String2Date(ExtractElementFromSegment("FEDEXDeliveryDate", SegmentSet))
        '        End If
        '    End If
        '    If Not 0 = Val(ExtractElementFromSegment("costFedEXHDEvening", SegmentSet)) Then
        '        objShipment.CarrierService.ServiceSurcharges.Add(New _baseServiceSurcharge(0, "HOME_DELIVERY_PREMIUM", "EVENING", True))
        '    End If
        '    If Not 0 = Val(ExtractElementFromSegment("costFedEXHDAppt", SegmentSet)) Then
        '        objShipment.CarrierService.ServiceSurcharges.Add(New _baseServiceSurcharge(0, "HOME_DELIVERY_PREMIUM", "APPOINTMENT", True))
        '    End If
        'End If
        ''
        '' FedEx/UPS - Service Level: Inside Delivery/Pickup
        'If "Y" = UCase(ExtractElementFromSegment("InsideDelivery", SegmentSet)) Then
        '    objShipment.CarrierService.ServiceSurcharges.Add(New _baseServiceSurcharge(0, "INSIDE_DELIVERY", "INSIDE_DELIVERY", True))
        'End If
        'If "Y" = UCase(ExtractElementFromSegment("InsidePickup", SegmentSet)) Then
        '    objShipment.CarrierService.ServiceSurcharges.Add(New _baseServiceSurcharge(0, "INSIDE_PICKUP", "INSIDE_PICKUP", True))
        'End If
        '
        '' FedEx/UPS - Service Level: Saturday Delivery/Pickup
        Dim isSat = (Today.DayOfWeek = DayOfWeek.Saturday)
        If SatDelivery_Btn.IsChecked And Not isSat Then
            objShipment.CarrierService.ServiceSurcharges.Add(New _baseServiceSurcharge(0, "SATURDAY_DELIVERY", "SATURDAY_DELIVERY", True))
        End If
        If SatDelivery_Btn.IsChecked And isSat Then
            objShipment.CarrierService.ServiceSurcharges.Add(New _baseServiceSurcharge(0, "SATURDAY_PICKUP", "SATURDAY_PICKUP", True))
        End If
        '
        Prepare_ServiceSurchargesFromScreen = True
        '
    End Function

    Public Function Prepare_ServiceSurchargesFromScreen_Package(ByRef objShipment As _baseShipment, ByRef objPack As _baseShipmentPackage) As Boolean
        '' TODO: Finish converting to using accessorial values from screen where available.
        '
        Prepare_ServiceSurchargesFromScreen_Package = False ' assume.
        '
        '' FedEx - Package Level: COD (Ground Service)
        Dim objCOD As New _baseServiceSurchargeCOD
        'objCOD.Amount = Val(ExtractElementFromSegment("CODAMT", SegmentSet))
        objCOD.Amount = Val(COD_Btn.Tag)
        If objCOD.Amount > 0 AndAlso objShipment.CarrierService.CarrierName = "FEDEX" AndAlso _FedExWeb.IsGroundService(objShipment.CarrierService.ServiceABBR) Then
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
                ' TODO: Once these features are actually written, add this back in.
                '.ChargeType = "" 'COD Recipient AccountNumber
                'If CBool(ExtractElementFromSegment("CashiersCheck", SegmentSet, "False")) Then
                '    .PaymentType = "0" '' GUARANTEED_FUNDS
                'Else
                '    ' COD with "certified check or money order only" unchecked should have 'ANY' payment type submitted to FedEx Web Server.
                '    ' .PaymentType = "1" ' CASH
                '    .PaymentType = "2" '' ANY 
                'End If
                '' FedEx COD amount should stay as entered by the user since all the calculations FedEx Web Services will do automatically.
                '.Amount = Val(ExtractElementFromSegment("CODAMTwSHIP", SegmentSet))
            End With
            '
            objPack.COD = objCOD ' FedEx COD for Ground shipment should be added at Package level only.
            '
        ElseIf objCOD.Amount > 0 AndAlso objShipment.CarrierService.CarrierName = "UPS" Then
            '
            _Debug.Stop_("COD Package Level = " & objCOD.Amount)
            objPack.ServiceSurcharges.Add(New _baseServiceSurcharge(0, "COD", "COD", True))
            '
            objPack.COD.ChargeType = String.Empty
            ' TODO: This doesn't seem to be implemented yet. Add these back when they are.
            'If CBool(ExtractElementFromSegment("CashiersCheck", SegmentSet, "False")) Then
            '    objPack.COD.PaymentType = "8" ' check
            'Else
            '    objPack.COD.PaymentType = "0" ' cash
            'End If
            objPack.COD.CurrencyType = _IDs.CurrencyType
            ' TODO: This doesn't appear to be implemented yet either
            'objPack.COD.AddCOD2Total = (objPack.COD.Amount < Val(ExtractElementFromSegment("CODAMTwSHIP", SegmentSet)))
            'If objPack.COD.AddCOD2Total Then
            '    objPack.COD.Amount = Val(ExtractElementFromSegment("CODAMTwSHIP", SegmentSet))
            'End If
        Else
            '
            objPack.COD = Nothing
            '
        End If
        ' TODO: Decide where to move this to, Dry Ice info is not available at this point.
        '' FedEx/UPS - Package Level: Dry Ice
        'Dim dryice As String = ExtractElementFromSegment("ABHazMat", SegmentSet).ToUpper
        'If Not String.IsNullOrEmpty(dryice) Then
        '    objPack.ServiceSurcharges.Add(New _baseServiceSurcharge(0, "DRY_ICE", "DRY_ICE", True))
        '    objPack.DryIce.WeightUnits = _Controls.Right(dryice, 2)
        '    objPack.DryIce.Weight = Val(_Controls.Replace(dryice, objShipment.DryIce.WeightUnits, "").Trim)
        'End If
        '
        '' FedEx - Package Level: Non-Standard Container
        If gShip.NonStandardContainer Then
            objPack.ServiceSurcharges.Add(New _baseServiceSurcharge(0, "NON_STANDARD_CONTAINER", "NON_STANDARD_CONTAINER", True))
        End If
        '
        '' FedEx - Package Level: Signature Option
        Dim signaturetype As String = String.Empty
        If AdultSig_Btn.IsChecked Then
            signaturetype = "Adult Signature" ' FedEx - Adult
            objPack.DeliveryConfirmation = "3" ' UPS - Adult
        ElseIf SigConfirm_Btn.IsChecked Then
            signaturetype = "Direct Signature" ' FedEx - Direct
            objPack.DeliveryConfirmation = "2" ' UPS - Signature required
        ElseIf DelConf_Btn.IsChecked Then
            signaturetype = "Indirect Signature" ' FedEx - Indirect
            objPack.DeliveryConfirmation = "1" ' UPS - Delivery confirmation
        Else
            ' For TinT, leave blank instead of using this.
            'signaturetype = "No Signature Required" ' FedEx - No Signature Required
        End If
        If Not String.IsNullOrEmpty(signaturetype) Then
            objPack.ServiceSurcharges.Add(New _baseServiceSurcharge(0, "SIGNATURE_OPTION", signaturetype, True))
        End If
        '
        '' FedEx - Package Level: Additional Handling indicator
        '' FedEx - Package Level: Large Package indicator
        If objShipment.CarrierService.CarrierName.ToUpper = "FEDEX" Then
            objPack.IsAdditionalHandling = Shipping_Surcharges.Check_FedEx_IsAdditionalHandling()
            objPack.IsLargePackage = Shipping_Surcharges.Check_FedEx_IsLargePackageSurcharge()
        ElseIf objShipment.CarrierService.CarrierName.ToUpper = "UPS" Then
            objPack.IsAdditionalHandling = Shipping_Surcharges.Check_UPS_IsAdditionalHandling()
            objPack.IsLargePackage = Shipping_Surcharges.Check_UPS_IsLargePackageSurcharge()
        End If
        '
        Prepare_ServiceSurchargesFromScreen_Package = True
        '
    End Function

    Public Shared Function GetShippingZone(ByVal ZoneTable As String, ByVal Zip As String, ByVal shipService As String, Optional ByVal carrier As String = "", Optional ByVal country As String = "") As String

        Dim z As Integer
        Dim i As Integer
        Dim NumericZip As Long
        Dim ZipIsNumeric As Boolean
        Dim TheZone As String = ""

        ZipIsNumeric = IsNumeric(Zip)
        If ZipIsNumeric = True Then

            NumericZip = Val(Zip)

        End If

        If ZoneTable = "USPS" Or ZoneTable = "USPS-EXPR" Then
            If _USPS.USPS_IsZoneMatrixLoaded Then
                Dim zoneToShip As String = ""
                Dim zoneToShipFiller As String = ""
                Dim zoneToShipMailType As String = ""
                Dim zoneToShipEx As String = ""
                If _USPS.USPS_DomesticZoneMatrix_LookupZone(Zip, zoneToShip, zoneToShipFiller) Then
                    If _USPS.USPS_DomesticZoneMatrix_IsException(zoneToShipFiller) Then
                        If _USPS.USPS_DomesticZoneExceptions_LookupZone(Zip, zoneToShipEx, zoneToShipMailType, zoneToShipFiller) Then
                            If _USPS.USPS_DomesticZoneExceptions_IsPriority(zoneToShipMailType) Then
                                If shipService = _USPS.PriorityMail Then
                                    zoneToShip = zoneToShipEx
                                End If
                            Else
                                zoneToShip = zoneToShipEx
                            End If
                        End If
                    End If
                End If
                If Len(zoneToShip) > 0 Then
                    TheZone = "ZONE" & zoneToShip
                End If
            End If
        End If

        If TheZone = "" Then

            If (carrier.ToUpper = "FEDEX" Or carrier.ToUpper = "FEDERAL EXPRESS") AndAlso country.ToUpper = "UNITED STATES" Then
                If Not ZoneTable = "FEDEX-GND" And Not ZoneTable = "FEDEXHI" Then
                    Dim zipCodeFiveDigit As Long = _Convert.ZipCode2FiveDigits(NumericZip, True)
                    '
                    If zipCodeFiveDigit >= 99500 Then ' To AK
                        If shipService = "FEDEX-GND" Then
                            ZoneTable = "FEDEX-GND-AK_HI"
                        ElseIf _IDs.IsIt_HawaiiShipper Then
                            ZoneTable = "FEDEXAK_HI" '' Hawaii Shipper
                        Else
                            ZoneTable = "FEDEXAK"  '' USA Shipper
                        End If
                    ElseIf zipCodeFiveDigit >= 96700 And zipCodeFiveDigit <= 96798 Then ' To HI
                        If shipService = "FEDEX-GND" Then
                            ZoneTable = "FEDEX-GND-AK_HI"
                        ElseIf _IDs.IsIt_HawaiiShipper Then
                            ZoneTable = "FEDEXAK_HI" '' Hawaii Shipper
                        Else
                            ZoneTable = "FEDEXAK"  '' USA Shipper
                        End If

                    ElseIf IsIt_HawaiiShipper() And shipService = "FEDEX-GND" Then
                        'Hawaii to US
                        ZoneTable = "FEDEX-GND"
                    End If
                End If
            End If

            For z = 0 To gZct - 1

                If gZoneTables(z).ZoneName = ZoneTable Then

                    Exit For

                End If

            Next
            If z = gZct Then

                'MsgBox("ATTENTION...Zone Not Found in Zone Table [" & ZoneTable & "] for zip " & Zip)

            Else

                If ZipIsNumeric = True Then

                    For i = 0 To gZoneTables(z).ZoneCount - 1

                        If NumericZip >= gZoneTables(z).Zones(i).Lo And NumericZip <= gZoneTables(z).Zones(i).Hi Then

                            TheZone = gZoneTables(z).Zones(i).Zone
                            Exit For

                        End If

                    Next

                End If

            End If
        End If
        Return TheZone

    End Function

    Public Shared Function GetShippingZone_International(countryName As String, gMasterIndex As Integer) As String
        GetShippingZone_International = String.Empty ' assume.
        '
        Dim z As Integer
        Dim i As Integer
        Dim TheZone As String = String.Empty
        '
        For z = 0 To gZct - 1

            If gZoneTables(z).ZoneName = gMaster(gMasterIndex).ZoneTable Then

                Exit For

            End If

        Next
        If z = gZct Then

            'MsgBox("ATTENTION...Zone Not Found in Zone Table [" & ZoneTable & "] for zip " & Zip)

        Else

            For i = 0 To gZoneTables(z).ZoneCount - 1
                '

                If Not String.IsNullOrEmpty(gZoneTables(z).Zones(i).Country) Then
                    '
                    If countryName.ToUpper = gZoneTables(z).Zones(i).Country.ToUpper Then

                        TheZone = gZoneTables(z).Zones(i).Zone
                        Exit For

                    End If
                    '
                End If
                '
            Next i

        End If
        '
        Return TheZone

    End Function

    Public Shared Function GetShippingZone_USMail_International(Service As String, country As String) As String
        Dim Zone As String = ""
        If Not String.IsNullOrWhiteSpace(country) Then
            Dim SQL As String = "Select * FROM [INTL-ZONE] Where COUNTRY='" & country.Replace("'", "") & "'"
            Dim segment As String = IO_GetSegmentSet(gUSMailDB_Zones, SQL)

            Zone = ExtractElementFromSegment(Service, segment)

            Return "ZONE" & Zone
        Else
            Return String.Empty
        End If

    End Function

    Public Function GetShipping_Zone_UPS_Canada(svc As ShippingChoiceDefinition) As String
        Dim OriginZone As String = ""
        Dim SQL As String = ""
        Dim zip As String = svc.ZipCode

        If zip = "" Then Return ""

        If zip.Length > 3 Then
            zip = zip.Substring(0, 3)
        End If

        If svc.Service = "CAN-STD" Then
            'Canada Ground

            'Get Origin Zone
            SQL = "Select [CAN-STD] From [CAN-USOriginZoneLookup] WHERE [StateCode]='" & GetPolicyData(gShipriteDB, "State") & "'"
            OriginZone = ExtractElementFromSegment("CAN-STD", IO_GetSegmentSet(gUPSZoneDB, SQL), "")

            If OriginZone <> "" Then
                'Get Zone
                SQL = "Select [ZONEORIGIN" & OriginZone & "] FROM [CAN-STD] WHERE '" & zip & "' >= [LOZIP] AND '" & zip & "' <= [HIZIP]"
                Return ExtractElementFromSegment("ZONEORIGIN" & OriginZone, IO_GetSegmentSet(gUPSZoneDB, SQL), "")
            Else
                Return ""
            End If

        Else
            'Canada Express
            SQL = "Select [ZONE] FROM [" & svc.Service & "] WHERE '" & zip & "' >= [LOZIP] AND '" & zip & "' <= [HIZIP]"
            Return ExtractElementFromSegment("Zone", IO_GetSegmentSet(gUPSZoneDB, SQL), "")

        End If

    End Function

    Public Shared Function GetShippingZone_Canada(Zip As String, gMasterIndex As Integer) As String
        GetShippingZone_Canada = String.Empty ' assume.
        '
        Dim z As Integer
        Dim i As Integer
        Dim TheZone As String = String.Empty
        Dim ZoneTable As String = String.Empty



        If Zip = "" Then Return ""
        Zip = Zip.ToUpper

        If Zip.Length > 3 Then
            Zip = Zip.Substring(0, 3)
        End If
        '
        If "FedEx" = gMaster(gMasterIndex).Carrier Then
            ZoneTable = "FEDEX-CAN"
        Else
            ZoneTable = gMaster(gMasterIndex).ZoneTable
        End If
        '
        For z = 0 To gZct - 1

            If gZoneTables(z).ZoneName = ZoneTable Then

                Exit For

            End If

        Next
        If z = gZct Then

            'MsgBox("ATTENTION...Zone Not Found in Zone Table [" & ZoneTable & "] for zip " & Zip)

        Else

            For i = 0 To gZoneTables(z).ZoneCount - 1
                '
                If Not String.IsNullOrEmpty(Zip) AndAlso Not String.IsNullOrEmpty(gZoneTables(z).Zones(i).LoAlpha) Then
                    '
                    If Zip >= gZoneTables(z).Zones(i).LoAlpha And Zip <= gZoneTables(z).Zones(i).HiAlpha Then

                        TheZone = gZoneTables(z).Zones(i).Zone
                        Exit For


                    End If
                    '
                End If
                '
            Next i

        End If
        '
        Return TheZone

    End Function

    Private Sub SortShippingCalculations()
        Select Case SortBy.SelectedIndex
            Case 0, 1
                ' default sort (carrier)
                For Each CR As Carrier In Display_CarrierList
                    CR.ServiceList = CR.ServiceList.OrderBy(Function(svc As ShippingChoiceDefinition) svc.IsButtonVisible).ToList
                Next
            Case 2
                'delivery time
                For Each CR As Carrier In Display_CarrierList
                    CR.ServiceList = CR.ServiceList.OrderBy(Function(svc As ShippingChoiceDefinition) svc.IsButtonVisible).ThenBy(Function(x) x.DeliveryDate).ToList
                Next
            Case 3
                'price
                For Each CR As Carrier In Display_CarrierList
                    CR.ServiceList = CR.ServiceList.OrderBy(Function(svc As ShippingChoiceDefinition) svc.IsButtonVisible).ThenBy(Function(x) x.TotalSell).ToList
                Next

            Case 4
                'profit
                For Each CR As Carrier In Display_CarrierList
                    CR.ServiceList = CR.ServiceList.OrderByDescending(Function(svc As ShippingChoiceDefinition) svc.Profit).ToList
                Next

        End Select


    End Sub

    Private Sub Set_ShippingPanel_View()
        '---set view--------
        If gShip.actualWeight > 150 Then
            Load_Shipping_Panel("Freight")
            Current_Panel_View = "Freight"

        ElseIf Country.Text = "United States" Then

            If IsIt_PuertoRicoShipper() Then
                'PR shipping to US
                Load_Shipping_Panel("Puerto Rico")
                Current_Panel_View = "Intl"
            Else
                'Domestic
                Load_Shipping_Panel("Domestic")
                Current_Panel_View = "Domestic"
            End If



        ElseIf Country.Text = "Canada" Then
            Load_Shipping_Panel("Canada")
            Current_Panel_View = "Canada"

        ElseIf Country.Text = "Puerto Rico" Then
            Load_Shipping_Panel("Puerto Rico")
            Current_Panel_View = "Intl"

        Else
            Load_Shipping_Panel("Intl")
            Current_Panel_View = "Intl"
        End If

        'remove carriers without services AndAlso remove carriers that have status set to Disabled
        'Set up the services to be dumped into ShippingPanel_IC
        Display_CarrierList = gCarrierList.Where(Function(x) x.ServiceList.Count > 0 AndAlso x.Status_Current <> 2).ToList
    End Sub

    Private Function Check_Packaging(svc As ShippingChoiceDefinition, cr As Carrier) As Boolean
        If (cr.Selected_Pack_Item IsNot Nothing AndAlso cr.Selected_Pack_Item.SettingID > 0) Then
            'carrier packaging selected

            If (svc.AirOrExpress Or cr.CarrierName = "USPS") Then
                'Express Service
                Return True
            Else
                'Ground not compatible with carrier packaging
                Return False
            End If



        Else
            'no carrier packaging, check l,w,h
            If (gShip.Length = 0 Or gShip.Width = 0 Or gShip.Height = 0) And Not Packaging_ComboBox.SelectedIndex = 1 Then
                Return False
            Else
                Return True
            End If

        End If
    End Function

    Private Function Check_Maximum_Size_and_Weight_Limits(ByRef svc As ShippingChoiceDefinition) As Boolean
        Select Case svc.Carrier
            Case "FedEx"
                If svc.Service = "FEDEX-GND" Then
                    If gShip.Length > 108 Or gShip.Width > 108 Or gShip.Height > 108 Or Val(LengthGirth.Text) > 165 Then
                        Return False
                    Else
                        Return True
                    End If

                ElseIf isServiceFreight(svc.Service) Then
                    'Freight

                    Return True

                Else
                    If gShip.Length > 119 Or gShip.Width > 119 Or gShip.Height > 119 Or Val(LengthGirth.Text) > 165 Then
                        Return False
                    Else
                        Return True
                    End If

                End If


            Case "UPS"
                If gShip.Length > 108 Or gShip.Width > 108 Or gShip.Height > 108 Or Val(LengthGirth.Text) > 165 Then
                    Return False
                Else
                    Return True
                End If


            Case "DHL"
                'Max Dims 118 x 63 x 48
                Dim dimList As List(Of Double) = New List(Of Double)

                dimList.Add(gShip.Length)
                dimList.Add(gShip.Width)
                dimList.Add(gShip.Height)

                dimList.Sort()

                If dimList(0) > 48 Or dimList(1) > 63 Or dimList(2) > 118 Then
                    Return False
                Else
                    Return True
                End If


            Case "USPS"
                If svc.Service = "USPS-RG" Or svc.Service = "USPS-PS" Then
                    If Val(LengthGirth.Text) > 130 Then
                        Return False
                    Else
                        Return True
                    End If

                Else

                    If Val(LengthGirth.Text) > 108 Then
                        Return False
                    Else
                        Return True
                    End If

                End If

            Case "SPEE-DEE"
                If gShip.Length > 120 Or gShip.Width > 120 Or gShip.Height > 120 Or Val(LengthGirth.Text) > 170 Then
                    Return False
                Else
                    Return True
                End If

            Case Else
                Return True

        End Select


    End Function

    Private Function isServiceDisabled(svc As ShippingChoiceDefinition) As Boolean
        If ExtractElementFromSegment("Disabled", svc.Segment, False) Then
            Return True
        Else
            Return False
        End If
    End Function


    Private Sub ProcessShippingRates()
        'Dim DeliveryDate As Date
        Dim FedEx_DAS As Integer = 0
        Dim UPS_DAS As Integer = 0
        Dim skipService As Boolean
        Dim PerPoundRate As Double

        '
        Call gShip_SetBasics_FromScreen()
        '
        If Country.Text = "" Or gShip.actualWeight = 0 And Packaging_ComboBox.SelectedIndex = 0 Or (Country.Text = "United States" And ZipCode.Text = "") Then
            ClearShipButtons()
            Exit Sub
        End If

        FedEx_DAS = Is_Zip_FedEx_DAS(ZipCode.Text)
        UPS_DAS = Is_Zip_UPS_DAS(ZipCode.Text)

        Set_ShippingPanel_View()

        DAS_Display.IsEnabled = False
        OVS_Display.IsEnabled = False
        AH_Display.IsEnabled = False


        For Each CR As Carrier In Display_CarrierList
            skipService = False

            Check_FedEx_FlatRate_Button(CR)

            For Each svc In CR.ServiceList
                If svc.Service <> "" AndAlso Not isServiceDisabled(svc) AndAlso Check_Maximum_Size_and_Weight_Limits(svc) AndAlso Check_Packaging(svc, CR) Then

                    svc.ZipCode = ZipCode.Text
                    If Not ShipToContact Is Nothing Then
                        svc.ShipTo_State = ShipToContact.State
                    End If

                    svc.ShipTo_Country = Country.Text
                    svc.Weight = gShip.actualWeight
                    svc.Packaging = CR.Selected_Pack_Item

                    Check_Packaging_Dimensions(svc)

                    svc.DeclaredValue = Val(DeclaredValue.Text)
                    svc.isThirdPartyDecVal = gThirdPartyInsurance
                    Definitions_Shipping.Set_Ship_Button_Color(svc)

                    ' Saturday delivery check
                    If gShip.SaturdayDelivery And ExtractElementFromSegment("SaturdayDelivery", svc.Segment) = "False" Then
                        svc.IsButtonVisible = Visibility.Hidden
                        skipService = True
                        Continue For
                    End If

                    If svc.Service = "FEDEX-INTP" And svc.ShipTo_Country = "Puerto Rico" Then
                        'When shipping to Puerto Rico, "FedEx International Priority" needs to be named only "International Priority".
                        svc.ServiceName = svc.ServiceName.Remove(0, 6)
                    End If

                    If svc.Service = "USPS-RG" Then
                        ' USPS retail ground not available in zones 1-4
                        svc.Zone = GetShippingZone(svc.ZoneTable, ZipCode.Text, svc.Service, svc.Carrier, Country.Text)

                        Dim zoneBlackList = {"ZONE1", "ZONE2", "ZONE3", "ZONE4"}
                        For Each zone In zoneBlackList
                            If svc.Zone = zone Then
                                svc.IsButtonVisible = Visibility.Hidden
                                skipService = True
                                Exit For
                            End If
                        Next
                        If skipService Then
                            Continue For
                        End If
                    End If

                    'If ExtractElementFromSegment("Residential", gConsigneeSegment, "False") Or Residential_Btn.IsChecked = True Then
                    If Residential_Btn.IsChecked = True Then
                        svc.IsResidential = True
                    Else
                        svc.IsResidential = False
                    End If


                    If FedEx.IsGroundHomeDelivery(svc.Service) Then
                        svc.IsFedExHomeDelivery = True
                    Else
                        svc.IsFedExHomeDelivery = False
                    End If



                    'NOT LTL FREIGHT
                    If Not FedEx_Freight.IsFreightLTLService(svc.Service) Then
                        If Country.Text = "United States" And Not isServiceInternational(svc.Service) Then
                            'Domestic Zone
                            svc.IsInternational = False
                            svc.Zone = GetShippingZone(svc.ZoneTable, ZipCode.Text, svc.Service, svc.Carrier, Country.Text)

                            If svc.Carrier = "SPEE-DEE" Then svc.Zone = "ZONE" & svc.Zone

                        ElseIf Country.Text = "Puerto Rico" And (CR.CarrierName = "UPS" Or CR.CarrierName = "USPS") Then
                            'UPS and USPS use domestic service zones for Puerto Rico

                            If CR.CarrierName = "UPS" Then svc.IsInternational = True
                            If CR.CarrierName = "USPS" Then svc.IsInternational = False

                            svc.Zone = GetShippingZone(svc.ZoneTable, ZipCode.Text, svc.Service, svc.Carrier, Country.Text)


                        ElseIf isServiceInternational(svc.Service) Or isServiceCanadian(svc.Service) Then
                            ' International Zone:
                            svc.IsInternational = True

                            If "Canada" = Me.Country.Text AndAlso Not "DHL" = svc.Carrier AndAlso Not "USPS" = svc.Carrier Then
                                If svc.Carrier = "UPS" Then
                                    svc.Zone = GetShipping_Zone_UPS_Canada(svc)
                                Else
                                    svc.Zone = GetShippingZone_Canada(Me.ZipCode.Text, Find_Master_Index(svc.Service))
                                End If
                            ElseIf svc.Carrier = "USPS" Then
                                If Not svc.IsFlatRate Then
                                    svc.Zone = GetShippingZone_USMail_International(svc.Service, Country.Text)
                                Else
                                    svc.Zone = GetUSPS_FlatRate_International_Zone(svc)
                                End If

                            ElseIf IsIt_PuertoRicoShipper() And Country.Text = "United States" Then
                                'Shipping from PR to US
                                svc.Zone = "ZONEPR"

                            Else

                                svc.Zone = GetShippingZone_International(Me.Country.Text, Find_Master_Index(svc.Service))

                            End If

                        End If


                        'Get Dimensional Weight, set billable weight
                        If Packaging_ComboBox.SelectedIndex <> 1 Then
                            'Not Letter
                            svc.DIM_Weight = Calculate_DimWeight(svc.Carrier, svc.Service, svc.IsInternational, svc.Weight, svc.Length, svc.Width, svc.Height)
                            '
                            If svc.DIM_Weight <> 0 And svc.DIM_Weight > svc.Weight Then
                                svc.Billable_Weight = svc.DIM_Weight
                            Else
                                svc.Billable_Weight = svc.Weight
                            End If

                            If svc.Service = "USPS-GND-ADV" And svc.Weight <= 0.99375 Then
                                Select Case svc.Weight
                                    Case Is <= 0.25 : svc.Billable_Weight = 0.25
                                    Case Is <= 0.5 : svc.Billable_Weight = 0.5
                                    Case Is <= 0.75 : svc.Billable_Weight = 0.75
                                    Case Is <= 0.99375 : svc.Billable_Weight = 0.99375

                                End Select
                            ElseIf svc.Service = "USPS-PRI_CubicRate" Then

                                IsEligible_CubicRates(svc.Service, svc.Weight, svc.Length, svc.Width, svc.Height, "", svc.Billable_Weight)


                            ElseIf svc.Service <> "FirstClass" And svc.Service <> "USPS-INTL-FCMI" Then
                                svc.Billable_Weight = Math.Ceiling(svc.Billable_Weight)
                            End If

                        Else
                            'Letter
                            Set_Letter_BillableWeight(svc)
                        End If




                        'GET PRICING ------------------------------------------------------------------------------------------------------------------------------------------

                        Get_BaseCost_Pricing(svc)

                        '------------------------------------------------------------------------------------------------------------------------------------------------------

                    ElseIf Country.Text = "United States" And Not isServiceInternational(svc.Service) And FedEx_Freight.IsFreightLTLService(svc.Service) Then
                        'LTL FREIGHT
                        svc.IsInternational = False

                        svc.Zone = GetShippingZone(svc.ZoneTable, ZipCode.Text, svc.Service, svc.Carrier, Country.Text)
                        svc.BaseCost = GetShippingCost_Freight(svc.Service, svc.Zone, gShip.actualWeight, svc.DeliveryDate)

                    End If


                    ' DAS 
                    If svc.Carrier = "FedEx" And svc.IsInternational = False Then
                        svc.IsDAS = FedEx_DAS
                    ElseIf svc.Carrier = "UPS" And svc.IsInternational = False Then
                        svc.IsDAS = UPS_DAS
                    End If


                    ' Calculate price
                    If svc.BaseCost <> 0 Then
                        Shipping_Discounts.Check_Discount_Rules(svc)
                        Check_Surcharge_Rules(svc)

                        If svc.IsBillableWeight_changed And Not FedEx_Freight.IsFreightLTLService(svc.Service) Then
                            'Billable weight changed due to accessorial charges, recalculate price.
                            svc.BaseCost = GetShippingCost(Get_ServiceTable(svc), svc.Zone, svc.Billable_Weight, svc.DeliveryDate, Get_ServiceDB_Path(svc.Carrier))
                            Shipping_Discounts.Check_Discount_Rules(svc)
                        End If


                        If svc.Service = "FirstClass" And Packaging_ComboBox.SelectedIndex = 1 Then
                            'First Class Letter
                            svc.Sell = Get_FirstClass_Flat_Retail(Pounds2Ounces(svc.Billable_Weight, 2))

                        ElseIf svc.IsFlatRate And svc.Carrier = "USPS" Then
                            svc.Sell = Get_USPS_FlatRate_CostRetail(svc, "SellPrice")


                        ElseIf svc.Carrier = "UPS" And GetPolicyData(gShipriteDB, "AlwaysChargeUPSRetail") Then
                            'Always Charge Retail enabled. Get Retail amount and add any Level R markup

                            If svc.Billable_Weight > 150 Then
                                PerPoundRate = GetShippingCost(Get_ServiceTable(svc), svc.Zone, 151, svc.DeliveryDate, gUPSRetailServicesDB)
                                svc.Sell = PerPoundRate * svc.Billable_Weight
                            Else
                                svc.Sell = GetShippingCost(Get_ServiceTable(svc), svc.Zone, svc.Billable_Weight, svc.DeliveryDate, gUPSRetailServicesDB)
                            End If

                            svc.Sell = GetShippingSellingPrice(gMaster(Find_Master_Index(svc.Service)), svc.Sell, True, svc.Billable_Weight, svc.Zone, svc.IsLetter)


                        ElseIf svc.Carrier = "FedEx" And GetPolicyData(gShipriteDB, "AlwaysChargeFedExRetail") Then
                            'Always Charge Retail enabled. Get Retail amount and add any Level R markup

                            If svc.IsFlatRate Then
                                'Retail_services.accdb does not have One Rate pricing. Use BaseCost as base for markup.
                                svc.Sell = svc.BaseCost
                            Else
                                If svc.Billable_Weight > 150 Then

                                    If (svc.Service = "FEDEX-GND" Or svc.Service = "FEDEX-CAN") Then
                                        'FedEx Ground doesn't have 151 per pound rate, need to manually calculate it by dividing 150 lb rate.

                                        PerPoundRate = GetShippingCost(Get_ServiceTable(svc), svc.Zone, 150, svc.DeliveryDate, gFedExRetailServicesDB)
                                        PerPoundRate = PerPoundRate / 150
                                        svc.Sell = PerPoundRate * svc.Billable_Weight
                                    Else
                                        PerPoundRate = GetShippingCost(Get_ServiceTable(svc), svc.Zone, 151, svc.DeliveryDate, gFedExRetailServicesDB)
                                        svc.Sell = PerPoundRate * svc.Billable_Weight
                                    End If

                                Else
                                    svc.Sell = GetShippingCost(Get_ServiceTable(svc), svc.Zone, svc.Billable_Weight, svc.DeliveryDate, gFedExRetailServicesDB)
                                End If
                            End If

                            svc.Sell = GetShippingSellingPrice(gMaster(Find_Master_Index(svc.Service)), svc.Sell, True, svc.Billable_Weight, svc.Zone, svc.IsLetter)


                        ElseIf svc.Carrier = "SPEE-DEE" And Val(LengthGirth.Text) >= 130 Then
                            svc.Sell = Get_SPEEDEE_Oversize(svc, False) 'SpeeDee Flat Rate oversize

                        Else
                            svc.Sell = GetShippingSellingPrice(gMaster(Find_Master_Index(svc.Service)), Markup_From_Base_OR_Discount(svc), False, svc.Billable_Weight, svc.Zone, svc.IsLetter)
                        End If

                        svc.Profit = Round(svc.Sell - svc.BaseCost, 2)

                        Calculate_Total(svc)
                        Check_Rounding_Option(svc)

                        svc.IsButtonVisible = Visibility.Visible
                        Check_ShippingDisplayOptions(svc)

                    Else

                        svc.IsButtonVisible = Visibility.Hidden
                    End If

                    If CR.Status_Current = 1 Then 'Carrier is hidden
                        svc.IsButtonVisible = Visibility.Hidden
                    End If
                Else
                    svc.IsButtonVisible = Visibility.Hidden
                End If
            Next
        Next

        If auto_TinT And gShip.actualWeight <= 150 Then
            If Not Packaging_ComboBox.SelectedIndex = 0 Or Not (gShip.Length = 0 Or gShip.Width = 0 Or gShip.Height = 0) Then
                GetRealShippingTimes()
            End If

        End If


        SortShippingCalculations()

        ShippingPanel_IC.ItemsSource = Display_CarrierList

        If gIsCustomerDisplayEnabled Then
            gCustomerDisplay.UpdateShippingRates(Display_CarrierList, Shipper.Text, Consignee.Text)
        End If

    End Sub

    Private Sub Check_ShippingDisplayOptions(ByRef svc As ShippingChoiceDefinition)
        Dim surchargeslist As List(Of Integer) = New List(Of Integer)

        For Each item In svc.SurchargesList
            surchargeslist.Add(item.ID)
        Next

        If surchargeslist.Contains(1) Or surchargeslist.Contains(51) Then
            'FedEx AH or UPS AH
            AH_Display.IsEnabled = True
        End If

        If surchargeslist.Contains(8) Or surchargeslist.Contains(58) Then
            'FedEx Oversize or UPS Oversize
            OVS_Display.IsEnabled = True
        End If

        If surchargeslist.Any(Function(x) x >= 9 And x <= 18) Or surchargeslist.Any(Function(x) x >= 59 And x <= 65) Then
            'FedEx DAS or UPS DAS
            DAS_Display.IsEnabled = True
        End If


    End Sub

    Private Sub Check_FedEx_FlatRate_Button(ByRef CR As Carrier)
        If CR.CarrierName = "FedEx" Then
            'If FedEx carrier packaging is selected, make option for FedEx Flat Rate visible
            If Not IsNothing(CR.Selected_Pack_Item) AndAlso CR.Selected_Pack_Item.SettingID > 0 And Country.Text = "United States" Then
                FedExFlatRate_Button.Visibility = Visibility.Visible
            Else
                FedExFlatRate_Button.Visibility = Visibility.Hidden
                FedEx_FlatRate_TxtBx.Text = "OFF"
            End If

        End If
    End Sub

    Private Function Markup_From_Base_OR_Discount(ByRef svc As ShippingChoiceDefinition) As Double
        Select Case svc.Carrier
            Case "FedEx"
                If GetPolicyData(gShipriteDB, "IsFedExMarkupDiscount") Then
                    Return svc.DiscountCost
                Else
                    Return svc.BaseCost
                End If

            Case "UPS"
                If GetPolicyData(gShipriteDB, "IsUPSMarkupDiscount") Then
                    Return svc.DiscountCost
                Else
                    Return svc.BaseCost
                End If

            Case "DHL"
                If GetPolicyData(gShipriteDB, "IsDHLMarkupDiscount") Then
                    Return svc.DiscountCost
                Else
                    Return svc.BaseCost
                End If

            Case Else
                Return svc.BaseCost
        End Select

    End Function

    Private Sub Get_BaseCost_Pricing(ByRef svc As ShippingChoiceDefinition)

        If svc.Carrier = "USPS" Then

            If svc.Packaging IsNot Nothing AndAlso svc.Packaging.SettingName <> "" Then
                'Carrier Packaging

                If svc.Packaging.SettingName.Contains("Regnl") Then
                    'Regional Rate Box Selected

                    If svc.Service = "USPS-PRI" Then
                        svc.BaseCost = Get_USPS_RegionalRate_Cost(svc)
                    Else
                        'regional rate is only for Priority Mail
                        svc.BaseCost = 0
                    End If



                ElseIf svc.IsFlatRate Then
                    'Flat Rate selected
                    svc.BaseCost = Get_USPS_FlatRate_CostRetail(svc, "BaseRetail")
                End If


            Else
                'Regular Packaging
                If svc.Service = "FirstClass" Or svc.Service = "USPS-INTL-FCMI" Then
                    svc.BaseCost = Get_FirstClass_ShippingCost(svc.Zone, Pounds2Ounces(svc.Billable_Weight, 2), Packaging_ComboBox.SelectedIndex, svc.Service)
                Else
                    svc.BaseCost = GetShippingCost(Get_ServiceTable(svc), svc.Zone, svc.Billable_Weight, svc.DeliveryDate, Get_ServiceDB_Path(svc.Carrier))
                End If
            End If


        Else
            'All other carriers

            If Check_Is_PO_Box(svc) Then
                'Don't display rates for PO Boxes if not USPS
                svc.BaseCost = 0

            ElseIf isStateCode_MilitaryState(svc.ShipTo_State) Then
                'APO/FPO address, no rates if not USPS
                svc.BaseCost = 0

            ElseIf svc.IsResidential And (svc.Service = "FEDEX-2DY-AM" Or svc.Service = "2DAYAM") Then
                '2 Day AIR AM services are only available to commercial destinations.
                svc.BaseCost = 0


            ElseIf svc.Billable_Weight <= 150 Or IsFreight_123Day_Service(svc.Service) Then
                If svc.Carrier = "FedEx" And svc.IsFlatRate Then
                    svc.BaseCost = Get_FedEx_FlatRateCost(svc)

                ElseIf svc.Carrier = "FedEx" AndAlso isFedEx_10_25KG_Box(svc) Then
                    svc.BaseCost = Get_FedEx_10_25KG_INTL_Cost(svc)

                ElseIf svc.Carrier = "SPEE-DEE" And Val(LengthGirth.Text) >= 130 Then
                    svc.BaseCost = Get_SPEEDEE_Oversize(svc, True) 'SpeeDee Flat Rate oversize
                Else
                    svc.BaseCost = GetShippingCost(Get_ServiceTable(svc), svc.Zone, Check_Weight(svc), svc.DeliveryDate, Get_ServiceDB_Path(svc.Carrier))
                End If


            Else
                '--------Dim Weight over 150lb -----------------
                Dim PerPoundRate As Double

                If svc.Service = "FEDEX-GND" Or svc.Service = "FEDEX-CAN" Then
                    'FedEx Ground doesn't have 151 per pound rate, need to manually calculate it by dividing 150 lb rate.
                    PerPoundRate = GetShippingCost(Get_ServiceTable(svc), svc.Zone, 150, svc.DeliveryDate, Get_ServiceDB_Path(svc.Carrier))
                    PerPoundRate = PerPoundRate / 150

                Else
                    PerPoundRate = GetShippingCost(Get_ServiceTable(svc), svc.Zone, 151, svc.DeliveryDate, Get_ServiceDB_Path(svc.Carrier))
                End If

                svc.BaseCost = PerPoundRate * svc.Billable_Weight

            End If

        End If

    End Sub

    Private Function Check_Weight(svc As ShippingChoiceDefinition) As Double
        If svc.Carrier = "UPS" And svc.IsInternational Then
            'UPS international charts have weight records incremented by 2 - 50 to 100lbs.
            'records are incremented by 5 - 100lbs to 150 lbs.

            Dim weight = svc.Billable_Weight

            If weight >= 50 And weight <= 100 Then
                'increments of 2.
                weight = weight + (weight Mod 2)

            ElseIf weight > 100 Then
                'increments of 5
                weight = Round(weight / 5, 0, True) * 5
            End If

            Return weight

        Else

            Return svc.Billable_Weight
        End If
    End Function

    Private Function Get_SPEEDEE_Oversize(ByRef svc As ShippingChoiceDefinition, GetCost As Boolean) As Double

        Dim field As String

        If GetCost Then
            field = "_Cost"
        Else
            field = "_Charge"
        End If

        Select Case svc.Zone
            Case "ZONE2"
                Return ExtractElementFromSegment("OVS2" & field, svc.Segment, "0")
            Case "ZONE3"
                Return ExtractElementFromSegment("OVS3" & field, svc.Segment, "0")
            Case "ZONE4"
                Return ExtractElementFromSegment("OVS4" & field, svc.Segment, "0")
            Case "ZONE5"
                Return ExtractElementFromSegment("OVS5" & field, svc.Segment, "0")
            Case "ZONE6"
                Return ExtractElementFromSegment("OVS6" & field, svc.Segment, "0")

            Case Else
                Return 0
        End Select

    End Function

    Private Function isFedEx_10_25KG_Box(svc) As Boolean
        If svc.Packaging IsNot Nothing AndAlso svc.Packaging.SettingName <> "" Then
            If (svc.Packaging.SettingName.Contains("10kg") Or svc.Packaging.SettingName.Contains("25kg")) Then
                Return True
            Else
                Return False
            End If
        Else
            Return False
        End If

    End Function

    Private Function Get_FedEx_10_25KG_INTL_Cost(ByRef svc As ShippingChoiceDefinition) As Double
        Dim Box As String

        If svc.Service <> "FEDEX-INTP" Then Return 0

        If svc.Packaging.SettingName.Contains("10kg") Then
            Box = "10"
        ElseIf svc.Packaging.SettingName.Contains("25kg") Then
            Box = "25"
        Else
            Return 0
        End If


        Dim SQL As String = "SELECT " & svc.Zone & " FROM [FEDEX-INTP-REGIONAL] WHERE PackKg=" & Box & " and [Lbs1] <= " & svc.Weight & " AND [Lbs2] >=" & svc.Weight
        Return Val(ExtractElementFromSegment(svc.Zone, IO_GetSegmentSet(gFedExServicesDB, SQL), "0"))

    End Function

    Private Function Check_Is_PO_Box(ByRef svc As ShippingChoiceDefinition) As Boolean
        If ShipToContact Is Nothing Then Return False

        Dim addr1 As String = ShipToContact.Addr1
        addr1 = addr1.ToUpper

        If addr1.Contains("PO BOX") Or addr1.Contains("P.O. BOX") Then
            Return True
        Else
            Return False
        End If
    End Function

    Private Sub Check_Packaging_Dimensions(ByRef svc As ShippingChoiceDefinition)

        If svc.Packaging IsNot Nothing AndAlso svc.Packaging.SettingName <> "" Then
            'carrier packaging

            'Flat Rate flag
            If svc.Carrier = "USPS" Then
                If svc.Packaging.SettingName.Contains("FlatR") Then
                    svc.IsFlatRate = True
                Else
                    svc.IsFlatRate = False
                End If

            ElseIf svc.Carrier = "FedEx" Then
                If FedEx_FlatRate_TxtBx.Text = "ON" Then
                    svc.IsFlatRate = True
                Else
                    svc.IsFlatRate = False
                End If
            End If

            svc.Length = Math.Round(Val(svc.Packaging.Length), 0, MidpointRounding.AwayFromZero)
            svc.Width = Math.Round(Val(svc.Packaging.Width), 0, MidpointRounding.AwayFromZero)
            svc.Height = Math.Round(Val(svc.Packaging.Height), 0, MidpointRounding.AwayFromZero)

            If svc.Packaging.SettingName.Contains("Letter") Or svc.Packaging.SettingName.Contains("Envelope") Or svc.Packaging.SettingName.Contains("ENV") Then
                svc.IsLetter = True
            Else
                svc.IsLetter = False
            End If


        Else
            'Letter
            If Packaging_ComboBox.SelectedIndex = 1 Then
                svc.IsLetter = True
            Else
                svc.IsLetter = False
            End If

            'Other
            If Packaging_ComboBox.SelectedIndex = 0 Then
                svc.Length = Val(Length_TxtBx.Text)
                svc.Width = Val(Width_TxtBx.Text)
                svc.Height = Val(Height_TxtBx.Text)
            End If
        End If

    End Sub

    Private Function GetUSPS_FlatRate_International_Zone(svc As ShippingChoiceDefinition) As Double
        Dim field As String = ""
        Dim SQL As String

        If svc.Service = "USPS-INTL-PMI" And Not svc.Packaging.SettingName.Contains("Exp") Then
            field = "USPS-INTL-PMI-FLATR"

        ElseIf svc.Service = "USPS-INTL-EMI" And svc.Packaging.SettingName.Contains("Exp") Then
            field = "USPS-INTL-EMI-FLATR"

        Else
            Return 0
        End If

        SQL = "Select [" & field & "] from [INTL-ZONE] Where Country='" & svc.ShipTo_Country & "'"

        Return ExtractElementFromSegment(field, IO_GetSegmentSet(gUSMailDB_Zones, SQL), "0")
    End Function

    Public Shared Function Get_USPS_FlatRate_CostRetail(svc As ShippingChoiceDefinition, field As String) As Double

        If svc.Packaging.SettingName.Contains("Exp") And svc.Service = "USPS-EXPR" Then
            'express mail
            Return Get_USPS_FlatRateValue(svc.Packaging.SettingID, field)

        ElseIf Not svc.Packaging.SettingName.Contains("Exp") And svc.Service = "USPS-PRI" Then
            'priority mail
            Return Get_USPS_FlatRateValue(svc.Packaging.SettingID, field)

        ElseIf svc.Service = "USPS-INTL-PMI" And Not svc.Packaging.SettingName.Contains("Exp") Then
            'Priority International
            Return Get_USPS_FlatRateValue(svc.Packaging.SettingID, field, svc.Zone)

        ElseIf svc.Service = "USPS-INTL-EMI" And svc.Packaging.SettingName.Contains("Exp") Then
            'Express International
            Return Get_USPS_FlatRateValue(svc.Packaging.SettingID, field, svc.Zone)

        Else
            Return 0
        End If

        Return 0
    End Function

    Public Shared Function Get_USPS_FlatRateValue(SettingID As Integer, field As String, Optional Intl_Zone As Double = 0) As Double
        Dim SQL As String

        If Intl_Zone <> 0 Then
            SQL = "Select CarrierPackagingFlatRateValues." & field & " from CarrierPackagingFlatRateValues INNER JOIN CarrierServiceTypes ON CarrierServiceTypes.ServiceTypeID=CarrierPackagingFlatRateValues.ServiceTypeID  Where  SettingID=" & SettingID & " and CarrierServiceTypes.ServiceTypeName='Int Price Group " & Intl_Zone & "'"

            If Intl_Zone = 1 Then
                'Zone 1 price group is listed under "canada"
                SQL = Strings.Replace(SQL, "Int Price Group 1", "Canada")
            End If
        Else
            SQL = "Select " & field & " from CarrierPackagingFlatRateValues Where SettingID=" & SettingID
        End If

        Return ExtractElementFromSegment(field, IO_GetSegmentSet(gPackagingDB, SQL), "0")
    End Function

    Private Function Get_USPS_RegionalRate_Cost(svc As ShippingChoiceDefinition) As Double
        Dim SQL As String
        Dim PackagingType As String

        If svc.Packaging.SettingName.Contains("RegnlA") Then
            PackagingType = "A"
        ElseIf svc.Packaging.SettingName.Contains("RegnlB") Then
            PackagingType = "B"
        Else
            Return 0
        End If

        SQL = "SELECT " & svc.Zone & " FROM [USPS-PRI_Regional] WHERE PackagingType='" & PackagingType & "'"

        Return ExtractElementFromSegment(svc.Zone, IO_GetSegmentSet(gUSMailDB_Services, SQL), 0)

    End Function

    Public Shared Function Get_ServiceTable(svc As ShippingChoiceDefinition) As String
        If svc.Service = "USPS-INTL-PMI" And svc.ShipTo_Country = "Canada" Then
            Return "USPS-INTL-PMI-CANADA"
        ElseIf _IDs.IsIt_PuertoRicoShipper And svc.Service = "COM-GND" Then
            Return "COM-GND_PR"
        ElseIf _IDs.IsIt_HawaiiShipper And svc.Service = "COM-GND" Then
            Return "COM-GND_HI"
        Else
            Return svc.Service
        End If

    End Function

    Public Shared Function Get_ServiceDB_Path(Carrier As String) As String
        If Carrier = "FedEx" Then
            If _IDs.IsIt_FedEx_FASC Or _IDs.IsIt_PostNetStore Then
                Return gFedExRetailServicesDB
            Else
                Return gFedExServicesDB
            End If

        ElseIf Carrier = "UPS" Then
            If _IDs.IsIt_UPS_ASO Or _IDs.IsIt_PostNetStore Then
                Return gUPSRetailServicesDB
            Else
                Return gUPSServicesDB
            End If

        Else
            Return ""
        End If

    End Function

    Private Sub Set_Letter_BillableWeight(ByRef svc As ShippingChoiceDefinition)
        If svc.Service = "FirstClass" OrElse svc.Service = "USPS-INTL-FCMI" Then
            svc.Billable_Weight = svc.Weight
        Else
            If svc.Carrier.ToUpper = "DHL" AndAlso svc.Weight > _Dhl_XML.objDHL_Setup.Envelope_Weight_Limit_Lbs Then
                svc.Billable_Weight = Math.Ceiling(svc.Weight)
            Else
                svc.Billable_Weight = 0
            End If
        End If

    End Sub

    Public Shared Sub Calculate_Total(ByRef shipment As ShippingChoiceDefinition)

        shipment.TotalBaseCost = shipment.BaseCost
        shipment.TotalSell = shipment.Sell
        shipment.TotalDiscountCost = shipment.DiscountCost

        If Not IsNothing(shipment.SurchargesList) Then

            For Each item As ShippingSurcharge In shipment.SurchargesList
                shipment.TotalBaseCost += item.BaseCost
                shipment.TotalSell += item.SellPrice
                shipment.TotalDiscountCost += item.DiscountCost
            Next
        End If
    End Sub

    Public Shared Sub Check_Rounding_Option(ByRef shipment As ShippingChoiceDefinition)
        Dim RoundOption As String = GetPolicyData(gShipriteDB, "Rounding", "")
        Dim AddAmount As Double = 0
        Dim RoundAmount As Double = 0

        Select Case RoundOption
            Case "Nickel"
                RoundAmount = RoundNumber(shipment.TotalSell, 0.05)
            Case "Dime"
                RoundAmount = RoundNumber(shipment.TotalSell, 0.1)
            Case "Quarter"
                RoundAmount = RoundNumber(shipment.TotalSell, 0.25)
            Case "Half-Dollar"
                RoundAmount = RoundNumber(shipment.TotalSell, 0.5)
            Case "Dollar"
                RoundAmount = RoundNumber(shipment.TotalSell, 1)
        End Select

        AddAmount = RoundAmount - shipment.TotalSell
        AddAmount = Round(AddAmount, 2)

        If AddAmount > 0 Then
            Dim Scharge As ShippingSurcharge = New ShippingSurcharge

            Scharge.ID = ShippingSurcharge.IDs_Other.RoundOption
            Scharge.Name = "Round Option"
            Scharge.BaseCost = 0
            Scharge.DiscountCost = 0
            Scharge.SellPrice = AddAmount
            Scharge.DBField_Manifest_Sell = "RoundOptionSell"

            shipment.SurchargesList.Add(Scharge)

            shipment.TotalSell += AddAmount
        End If


    End Sub

    Public Shared Function RoundNumber(ByVal OriginalNumber As Double, ByVal RoundTo As Double) As Double

        If (OriginalNumber / RoundTo) * 2 = CInt((OriginalNumber / RoundTo) * 2) Then
            OriginalNumber = OriginalNumber + RoundTo / 10
        End If

        Return Round(OriginalNumber / RoundTo, 0, True) * RoundTo

    End Function

    Private Sub CarrierBorder_MouseDown(sender As Object, e As MouseButtonEventArgs)
        Dim CR As Carrier = sender.tag


        If CR.Status_Current = 1 Then
            'carrier is hidden, set to visible

            Select Case Current_Panel_View
                'set global variable to visible so that the carrier will remain visible until shipping screen is closed
                Case "Domestic"
                    gCarrierList.First(Function(x) x.CarrierName = CR.CarrierName).Status_Domestic = 0

                Case "Canada"
                    gCarrierList.First(Function(x) x.CarrierName = CR.CarrierName).Status_Canada = 0

                Case "Intl"
                    gCarrierList.First(Function(x) x.CarrierName = CR.CarrierName).Status_Intl = 0

                Case "Freight"
                    gCarrierList.First(Function(x) x.CarrierName = CR.CarrierName).Status_Freight = 0

            End Select



            For Each svc In CR.ServiceList
                svc.IsButtonVisible = Visibility.Visible
            Next

            ShippingPanel_IC.Items.Refresh()
        End If

    End Sub

    Private Sub gShip_SetBasics_FromScreen()
        If gShip Is Nothing Then
            gShip = New gShip_Class
        End If
        With gShip
            .actualWeight = Val(Weight.Text)
            .DecVal = Val(DeclaredValue.Text)
            .Height = Val(Height_TxtBx.Text)
            .Length = Val(Length_TxtBx.Text)
            .Width = Val(Width_TxtBx.Text)

            If SatDelivery_Btn.IsChecked Then
                If (Today.DayOfWeek = DayOfWeek.Saturday) Then
                    .SaturdayPickUp = True
                Else
                    .SaturdayDelivery = True
                End If
            End If




            If Not Packaging_ComboBox.SelectedIndex = -1 Then
                .PackagingType = Packaging_ComboBox.Items(Packaging_ComboBox.SelectedIndex)
            End If
            If _IDs.IsIt_USAShipper Then
                .Domestic = ("United States" = Me.Country.Text)
            ElseIf _IDs.IsIt_CanadaShipper Then
                .Domestic = ("Canada" = Me.Country.Text)
            Else
                .Domestic = True
            End If
            If _Contact.ShipToContact IsNot Nothing Then
                .Residential = _Contact.ShipToContact.Residential
            Else
                .Residential = False
            End If
        End With
    End Sub

    Private Sub Packaging_ComboBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles Packaging_ComboBox.SelectionChanged
        Try
            If Packaging_ComboBox.SelectedIndex = 1 Then
                'Letter
                Height_TxtBx.Visibility = Visibility.Hidden
                Width_TxtBx.Visibility = Visibility.Hidden
                Length_TxtBx.Visibility = Visibility.Hidden

                Height_Lbl.Visibility = Visibility.Hidden
                Width_Lbl.Visibility = Visibility.Hidden
                Length_Lbl.Visibility = Visibility.Hidden
            Else
                'Other
                Height_TxtBx.Visibility = Visibility.Visible
                Width_TxtBx.Visibility = Visibility.Visible
                Length_TxtBx.Visibility = Visibility.Visible

                Height_Lbl.Visibility = Visibility.Visible
                Width_Lbl.Visibility = Visibility.Visible
                Length_Lbl.Visibility = Visibility.Visible

            End If
            Call ProcessShippingRates()
            '
        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to select packaging type...")
        End Try
    End Sub

    Public Shared Function Calculate_DimWeight(Carrier As String, Service As String, isInternational As Boolean, Weight As Double, Length As Double, Width As Double, Height As Double) As Integer
        Dim DimW As Integer
        Dim Divisor As Integer = 0

        'Dimensions have to be rounded to full inch before any calculations.
        Dim L As Integer = Math.Round(Length, 0, MidpointRounding.AwayFromZero)
        Dim W As Integer = Math.Round(Width, 0, MidpointRounding.AwayFromZero)
        Dim H As Integer = Math.Round(Height, 0, MidpointRounding.AwayFromZero)

        Select Case Carrier
            Case "FedEx"

                If _IDs.IsIt_FedEx_FASC Or _IDs.IsIt_PostNetStore Then
                    If isInternational = False Then
                        'FASC domestic
                        Divisor = 166
                    Else
                        'FASC international
                        Divisor = 139
                    End If

                Else
                    'non-FASC
                    Divisor = 139
                End If


            Case "UPS"
                If _IDs.IsIt_UPS_ASO Or _IDs.IsIt_PostNetStore Then
                    If isInternational = False Then
                        'ASO domestic
                        Divisor = 166
                    Else
                        'ASO international
                        Divisor = 139
                    End If

                Else
                    'non-ASO
                    Divisor = 139
                End If


            Case "DHL"
                Divisor = 139


            Case "USPS"

                If Service = "USPS-PRI" Or Service = "USPS-RG" Or Service = "USPS-EXPR" Or Service = "USPS-PS" Or Service = "USPS-GND-ADV" Then
                    'Package has to be larger then 1 cubic foot in order for USPS Dim weights to apply
                    If Calculate_Volume(L, W, H) > 1 Then
                        Divisor = 166
                    End If

                End If
        End Select

        If Divisor <> 0 Then
            DimW = Math.Ceiling((L * W * H) / Divisor)
            Return DimW
        Else
            Return 0
        End If

    End Function

    Public Shared Function Calculate_Volume(ByVal L As Double, ByVal W As Double, ByVal H As Double) As Double
        Return Math.Round((L * W * H) / 1728, 2)
    End Function

    Public Shared Function Calculate_Length_Plus_Girth(ByVal L As Double, ByVal W As Double, ByVal H As Double) As Double

        Dim smallest1 As Double
        Dim smallest2 As Double
        Dim largest As Double

        'finds 2 smallest sides
        largest = L
        smallest1 = W
        smallest2 = H

        If W > largest Then
            largest = W
            smallest1 = L
            smallest2 = H
        End If

        If H > largest Then
            largest = H
            smallest1 = W
            smallest2 = L
        End If

        Return ((smallest1 + smallest2) * 2) + largest

    End Function

    Private Sub LWH_Click(sender As Object, e As EventArgs) Handles Length_TxtBx.GotFocus, Width_TxtBx.GotFocus, Height_TxtBx.GotFocus, ZipCode.GotFocus, DeclaredValue.GotFocus, Weight.GotFocus
        Dim CurrentTextbox = DirectCast(sender, TextBox)
        If CurrentTextbox.Name = "Weight" Then
            ConnectedScale.StopScale = True
        End If

        If CurrentTextbox.Text = "0" Then
            CurrentTextbox.Text = ""
        Else
            CurrentTextbox.SelectAll()
        End If
    End Sub

    Private Sub Saved_Packjob_indexchanged(sender As Object, e As EventArgs) Handles Saved_Packjob.SelectionChanged
        Dim Content As String
        Dim Segment As String

        Dim ReturnSegment As String = ""
        Dim L As Integer = 0
        Dim W As Integer = 0
        Dim H As Integer = 0
        Dim buf As String = ""
        Dim i As Integer
        Dim j As Integer
        Dim amt As Double = 0
        Dim amt2 As Double = 0
        Dim TaxRate As Double = 0
        Dim AccumulatedSalesTax As Double = 0
        Dim InnerBox As String = ""
        Dim OuterBox As String = ""
        Dim WrapQty As Double
        Dim FillQty As Double



        Dim wrapHight As Single = 0.5
        Dim wrapLWH As Single
        Dim wrapL As Single
        Dim wrapW As Single
        Dim wrapH As Single
        Dim wrapVol As Single

        Dim outerVol As Integer

        Content = Saved_Packjob.SelectedValue
        Dim sql As String = "SELECT L, W, H FROM Contents WHERE Contents = '" & Content & "'"
        Segment = IO_GetSegmentSet(gShipriteDB, sql)
        Length_TxtBx.Text = ExtractElementFromSegment("L", Segment)
        Width_TxtBx.Text = ExtractElementFromSegment("W", Segment)
        Height_TxtBx.Text = ExtractElementFromSegment("H", Segment)

        Dim itemL As Single : itemL = Val(Length_TxtBx.Text)
        Dim itemW As Single : itemW = Val(Width_TxtBx.Text)
        Dim itemH As Single : itemH = Val(Height_TxtBx.Text)
        Dim itemVol As Single : itemVol = itemL * itemW * itemH

        ProcessShippingRates()

        TaxRate = Val(ExtractElementFromSegment("TaxRate", gPOSDefaultTaxSegment)) / 100

        gResult = ""
        Segment = AddElementToSegment(Segment, "Weight", gShip.actualWeight)
        Segment = AddElementToSegment(Segment, "L", gShip.Length)
        Segment = AddElementToSegment(Segment, "W", gShip.Width)
        Segment = AddElementToSegment(Segment, "H", gShip.Height)
        Segment = AddElementToSegment(Segment, "DecVal", gShip.DecVal)
        Segment = AddElementToSegment(Segment, "NoOfPcs", 1)
        ReturnSegment = FragilityCalculator("FRAGILE", Segment)

        i = 3


        j = GetIndexOfMaterials("Box")

        If Not j = -1 Then

            L = Val(ExtractElementFromSegment("L", gItemSet(j).SKU.InventorySegment(i)))
            W = Val(ExtractElementFromSegment("W", gItemSet(j).SKU.InventorySegment(i)))
            H = Val(ExtractElementFromSegment("H", gItemSet(j).SKU.InventorySegment(i)))
            outerVol = L * W * H
            InnerBox = L.ToString & " x " & W.ToString & " x " & H.ToString
            amt = Val(ExtractElementFromSegment("Sell", gItemSet(j).SKU.InventorySegment(i)))
            If Not ExtractElementFromSegment("Non_Taxable", gItemSet(j).SKU.InventorySegment(i)) = "True" Then

                AccumulatedSalesTax = AccumulatedSalesTax + (amt * TaxRate)

            End If

        End If

        j = GetIndexOfMaterials("DoubleBox")

        If Not j = -1 Then

            L = Val(ExtractElementFromSegment("L", gItemSet(j).SKU.InventorySegment(i)))
            W = Val(ExtractElementFromSegment("W", gItemSet(j).SKU.InventorySegment(i)))
            H = Val(ExtractElementFromSegment("H", gItemSet(j).SKU.InventorySegment(i)))
            OuterBox = L.ToString & " x " & W.ToString & " x " & H.ToString
            If OuterBox = "0 x 0 x 0" Then

                buf = "Box Size: " & InnerBox

            Else

                buf = "Inner Box: " & InnerBox + vbCrLf + "Outer Box: " & OuterBox

            End If

        End If

        amt2 = Val(ExtractElementFromSegment("Sell", gItemSet(j).SKU.InventorySegment(i)))
        If Not ExtractElementFromSegment("Non_Taxable", gItemSet(j).SKU.InventorySegment(i)) = "True" Then

            AccumulatedSalesTax = AccumulatedSalesTax + (amt2 * TaxRate)

        End If



        amt += amt2
        PackPop_Boxes.Content = buf
        PackPop_BoxPrice.Text = Format(amt, "$ 0.00")



        j = GetIndexOfMaterials("Wrap")
        wrapLWH = Val(gItemSet(j).Units.L(i)) * wrapHight
        wrapL = itemL + wrapLWH
        wrapW = itemW + wrapLWH
        wrapH = itemH + wrapLWH
        wrapVol = wrapL * wrapW * wrapH
        If Not j = -1 Then
            WrapQty = _Convert.Round_Double2Decimals((((2 * (itemL * itemW)) + (2 * (itemW * itemH)) + (2 * (itemL * itemH))) / 144) * gItemSet(j).Units.L(i), 1) ''mm#9.83(3/15).

            amt2 = Val(ExtractElementFromSegment("Sell", gItemSet(j).SKU.InventorySegment(i))) * WrapQty
            If Not ExtractElementFromSegment("Non_Taxable", gItemSet(j).SKU.InventorySegment(i)) = "True" Then

                AccumulatedSalesTax = AccumulatedSalesTax + (amt2 * TaxRate)

            End If

        End If

        j = GetIndexOfMaterials("Fill")
        Dim fillHight As Single = Val(gItemSet(j).Units.L(i))
        Dim fillVol As Single = (wrapL + fillHight) * (wrapW + fillHight) * (wrapH + fillHight)


        If 1 = gItemSet(j).Units.L(i) Then

            If 0 < wrapLWH Then
                FillQty = _Convert.Round_Double2Decimals((outerVol - wrapVol) / 1728, 1)
            Else
                FillQty = _Convert.Round_Double2Decimals((outerVol - itemVol) / 1728, 1)

            End If
        Else
            If 0 < wrapLWH Then
                FillQty = _Convert.Round_Double2Decimals((outerVol - wrapVol + fillVol) / 1728, 1)
            Else
                FillQty = _Convert.Round_Double2Decimals((outerVol - wrapVol + fillVol) / 1728, 1)

            End If
        End If
        If Not j = -1 Then


            amt = Val(ExtractElementFromSegment("Sell", gItemSet(j).SKU.InventorySegment(i))) * FillQty
            If Not ExtractElementFromSegment("Non_Taxable", gItemSet(j).SKU.InventorySegment(i)) = "True" Then

                AccumulatedSalesTax += (amt * TaxRate)

            End If

            amt += amt2
            PackPop_PackPrice.Text = Format(amt, "$ 0.00")
        End If


        j = GetIndexOfMaterials("Labor")
        If Not j = -1 Then

            amt = Val(ExtractElementFromSegment("Sell", gItemSet(j).SKU.InventorySegment(i))) * (gItemSet(j).Units.L(i) / 60)
            If Not ExtractElementFromSegment("Non_Taxable", gItemSet(j).SKU.InventorySegment(i)) = "True" Then

                AccumulatedSalesTax = AccumulatedSalesTax + (amt * TaxRate)

            End If
            PackPop_Labor.Text = Format(amt, "$ 0.00")

        End If
        PackPop_Tax.Text = Format(AccumulatedSalesTax, "$ 0.00")
        PackPop_Total.Text = Format(ValFix(PackPop_BoxPrice.Text) + ValFix(PackPop_PackPrice.Text) + ValFix(PackPop_Labor.Text), "$ 0.00")





    End Sub

    Private Sub Edit_SavedPackJob_Click(sender As Object, e As EventArgs) Handles Edit_SavedPackJob.Click
        PackMaster_Popup.IsOpen = False



        gShipmentParameters = AddElementToSegment(gShipmentParameters, "Contents", Saved_Packjob.SelectedItem.ToString)

        isOpen_ShipNew = True
        Dim win As New Packmaster(Me)
        win.ShowDialog(Me)
        isOpen_ShipNew = False
    End Sub

    Private Sub Select_SavedPackJob_Click(sender As Object, e As EventArgs) Handles Select_SavedPackJob.Click

        Dim ret As Long = 0
        Dim j As Integer = 0
        PackMaster_Popup.IsOpen = False
        isOpen_ShipNew = True
        ret = PostPackagingToPOS(3)
        isOpen_ShipNew = False

        j = GetIndexOfMaterials("Box")
        If j > 0 Then

            Length_TxtBx.Text = ExtractElementFromSegment("L", gItemSet(j).SKU.InventorySegment(3))
            Width_TxtBx.Text = ExtractElementFromSegment("W", gItemSet(j).SKU.InventorySegment(3))
            Height_TxtBx.Text = ExtractElementFromSegment("H", gItemSet(j).SKU.InventorySegment(3))

        Else

            j = GetIndexOfMaterials("DoubleBox")
            Length_TxtBx.Text = ExtractElementFromSegment("L", gItemSet(j).SKU.InventorySegment(3))
            Width_TxtBx.Text = ExtractElementFromSegment("W", gItemSet(j).SKU.InventorySegment(3))
            Height_TxtBx.Text = ExtractElementFromSegment("H", gItemSet(j).SKU.InventorySegment(3))

        End If

        Packing_Charge.Text = PackPop_Total.Text
        j = GetIndexOfMaterials("PackagingWeight")
        Packing_Weight.Text = gItemSet(j).PackagingWeight(3)

    End Sub

    Private Sub ClearShipButtons()
        ShippingPanel_IC.ItemsSource = Nothing
        ShippingPanel_IC.Items.Refresh()

        If gIsCustomerDisplayEnabled Then
            gCustomerDisplay.ClearShipScreen()
        End If
    End Sub

    Private Sub ClearShippingForm()
        ConnectedScale.IsWeightKeyed = False ' reset
        ConnectedScale.IsError = False ' reset
        LoadScale()
        Weight.Text = ""
        Length_TxtBx.Text = ""
        Width_TxtBx.Text = ""
        Height_TxtBx.Text = ""
        Consignee.Clear()
        Consignee.Tag = Nothing
        ShipToContact = Nothing
        DeclaredValue.Text = ""
        ZipCode.Text = ""

        LengthGirth.Text = ""
        Volume.Text = ""
        Content.Text = ""
        Country.IsEnabled = True

        DAS_Display.IsEnabled = False
        OVS_Display.IsEnabled = False
        AH_Display.IsEnabled = False


        Dim countryobject As _CountryDB = Nothing
        If Shipping.Find_CountryObject_byName("United States", countryobject) Then
            Country.SelectedItem = countryobject
        End If

        Zip_City_LV.ItemsSource = Nothing
        Zip_City_LV.Items.Refresh()
        Zip_CitySearch_TxtBx.Text = ""

        _FedExWeb.IsEnabled_OneRate = False

        If Customs.CustomsList IsNot Nothing Then
            Customs.CustomsList = Nothing
        End If
        '

        Call ClearShipButtons()
        Call ClearCheckButtons()
        Call CleargShipClass()
        Call Reset_Hidden_Carrier_Settings()
        '
        If Scale_Timer Is Nothing Then
            Weight.Focus() ' if scale disabled, set focus to weight textbox
        End If

    End Sub

    Private Sub Clear_PackingItems()
        Packing_Charge.Text = ""
        Packing_Weight.Text = ""
        gPackItemList = Nothing
    End Sub

    Private Sub ClearCheckButtons()

        DAS_Btn.IsChecked = False
        DelConf_Btn.IsChecked = False
        SigConfirm_Btn.IsChecked = False
        COD_Btn.IsChecked = False
        DAS_Btn.IsChecked = False
        Residential_Btn.IsChecked = False
        AdditionalHandling_Btn.IsChecked = False
        SatDelivery_Btn.IsChecked = False
        AdultSig_Btn.IsChecked = False

    End Sub

    Private Sub CleargShipClass()

        gShip = Nothing
        gShip = New gShip_Class

    End Sub

    Private Function Save_CustomsItems() As Boolean
        '
        If Customs.CustomsList IsNot Nothing Then
            '
            Dim sql2exe As String = String.Empty
            For Each item As Customs.CustomsItem In Customs.CustomsList
                _Debug.Print_(item.Description & " = " & item.Value & " weight: " & item.Weight)
                Dim sql2item As New sqlINSERT
                With sql2item
                    .Qry_INSERT("PackageID", gShip.PackageID, .TXT_, True, False, "CustomsItems")
                    .Qry_INSERT("Quantity", item.Qty.ToString, .NUM_)
                    .Qry_INSERT("Description", item.Description, .TXT_)
                    .Qry_INSERT("Weight", item.Weight.ToString, .NUM_)
                    .Qry_INSERT("ItemValue", item.Value.ToString, .NUM_)

                    If Not String.IsNullOrEmpty(item.HarmonizedCode) Then
                        sql2exe = .Qry_INSERT("HarmonizedCode", item.HarmonizedCode, .TXT_)
                    End If

                    sql2exe = .Qry_INSERT("OriginCountry", item.OriginCountry, .TXT_, False, True)
                End With
                '
                Call IO_UpdateSQLProcessor(gShipriteDB, sql2exe)
            Next
            '
        End If
        '
        Return True
    End Function

    Private Sub Ship_Button_Click(sender As Object, e As RoutedEventArgs)

        Dim CurrentButton As Button = TryCast(sender, Button)
        'Dim ret As Integer
        Dim SelectedIndex As Integer
        ' Dim buf As String
        Dim eName As String = ""
        Dim eValue As String = ""
        Dim amt As Double = 0
        Dim ManifestSegment As String = ""
        'Dim ID As Long = 0
        Dim SQL As String = ""
        Dim Segment As String = ""
        Dim SegmentSet As String = ""
        'Dim PackageID As Long = 0

        Try

             

            SelectedIndex = 0
            gSelectedShipmentChoice = sender.tag
            If gShip Is Nothing Then
                gShip = New gShip_Class
            End If
            With gShip
                .TestShipment = False
                .Contents = Content.Text
                .actualWeight = Val(Weight.Text)
                .DecVal = Val(DeclaredValue.Text)
                .Height = Math.Round(Val(Height_TxtBx.Text), 0, MidpointRounding.AwayFromZero)
                .Length = Math.Round(Val(Length_TxtBx.Text), 0, MidpointRounding.AwayFromZero)
                .Width = Math.Round(Val(Width_TxtBx.Text), 0, MidpointRounding.AwayFromZero)
                .PackagingType = Packaging_ComboBox.Text
                .ServiceABBR = gSelectedShipmentChoice.Service
                .Domestic = Not gSelectedShipmentChoice.IsInternational
                If _Contact.ShipToContact IsNot Nothing Then
                    .Residential = _Contact.ShipToContact.Residential
                End If
                .NonStandardContainer = False ' ToDo:
                .HOMEFedEXDeliveryDate = String.Empty
            End With

            gPackageShipped = False
            PrintLabelScreen_Return = ""

            Dim usTerritories = {"PR", "VI", "GU", "AS", "MP"}
            Dim countryCd = CType(Country.SelectedItem, _CountryDB).CountryCode
            gShip.Country = countryCd
            If Not gShip.Country = "US" AndAlso gThirdPartyInsurance AndAlso GetPolicyData(gShipriteDB, "EnableShipAndInsure") = "True" AndAlso Val(DeclaredValue.Text) > 0 Then

                MsgBox("ATTENTION...ShipAndInsure is configured ONLY for 'US' shipments" & vbCrLf & vbCrLf & "CARRIER BASED INSURANCE IN EFFECT", vbInformation)

            End If

            If countryCd <> "US" AndAlso Not (usTerritories.Contains(countryCd) AndAlso gSelectedShipmentChoice.Carrier = "USPS") Then
                Dim win As New Customs(Me, gSelectedShipmentChoice.Carrier)
                win.ShowDialog() ' save clicked in customs form -> gPackageShipped = True

            ElseIf gSelectedShipmentChoice.Carrier = "USPS" And isStateCode_MilitaryState(gSelectedShipmentChoice.ShipTo_State) Then
                'Military shipment
                Dim win As New Customs(Me, gSelectedShipmentChoice.Carrier)
                win.ShowDialog()

            ElseIf IsIt_PuertoRicoShipper() And countryCd = "US" Then

                'Puerto Rico to US shipping
                Dim win As New Customs(Me, gSelectedShipmentChoice.Carrier)
                win.ShowDialog()

            Else
                gPackageShipped = True
            End If
            '
            If gPackageShipped Then
                gPackageShipped = False ' reset
                Dim win As New Print_Shipping_Label(Me, Me.Shipper.Tag IsNot Nothing, Me.Consignee.Tag IsNot Nothing)
                win.ShowDialog(Me)
            End If
            '
            If gPackageShipped Then

                '   Create Manifest Segment
                Write_Shipment_To_DB()
                '

                gCompletedPackageStack = ""  ' This clears out the completed package stack used for Sending packages to POS
                Dim shipment As New _baseShipment
                shipment.CarrierService.IsDomestic = gShip.Domestic

                If PrintLabelScreen_Return.Contains("ManualLabel") Then
                    IO_UpdateSQLProcessor(gShipriteDB, "Update Manifest set Exported='Pickup Waiting', [Tracking#]='" & PrintLabelScreen_Return.Substring(12) & "' WHERE PackageID='" & gShip.PackageID & "'")

                    Shipment_Process()


                ElseIf PrintLabelScreen_Return.Contains("Batch_Label") Then
                    IO_UpdateSQLProcessor(gShipriteDB, "Update Manifest set Exported='Pending' WHERE PackageID='" & gShip.PackageID & "'")

                    'ToDo -  Print thermal batch label
                    Print_Batch_Label()

                    If Not gCompletedPackageStack = "" Then
                        gCompletedPackageStack = gCompletedPackageStack & ", "
                    End If
                    gCompletedPackageStack = "'" & gCompletedPackageStack & gShip.PackageID & "'"
                    Call ClearShippingForm()
                    If gCallingSKU = "SHIP1" Or gCallingSKU = "SHIPL" Then

                        Me.Close()

                    End If
                Else

                    '
                    If "FedEx" = gSelectedShipmentChoice.Carrier Then
                        '
                        If _FedExWeb.Prepare_PackageFromDb(gShip.PackageID, shipment) Then
                            If _FedExWeb.Prepare_ShipmentFromDb(gShip.PackageID, shipment) Then
                                If _FedExWeb.Upload_Shipment(shipment) Then

                                    Shipment_Process()
                                    '
                                End If
                            End If
                        End If
                        '
                    ElseIf "UPS" = gSelectedShipmentChoice.Carrier Then
                        ' 
                        If _UPSWeb.Prepare_PackageFromDb(gShip.PackageID, shipment) Then
                            If _UPSWeb.Prepare_ShipmentFromDb(gShip.PackageID, shipment) Then
                                If _UPSWeb.Upload_Shipment(shipment) Then

                                    Shipment_Process()
                                    '
                                End If
                            End If
                        End If
                        '
                    ElseIf "DHL" = gSelectedShipmentChoice.Carrier Then
                        '
                        If _Dhl_XML.Prepare_PackageFromDb(gShip.PackageID, shipment) Then
                            If _Dhl_XML.Prepare_ShipmentFromDb(gShip.PackageID, shipment) Then
                                If _Dhl_XML.Upload_Shipment(shipment) Then

                                    Shipment_Process()
                                    '
                                End If
                            End If
                        End If
                        '
                    ElseIf "USPS" = gSelectedShipmentChoice.Carrier Then
                        '
                        objEndiciaCredentials = New _EndiciaSetup
                        If _EndiciaWeb.Prepare_PackageFromDb(gShip.PackageID, shipment) Then
                            If _EndiciaWeb.Prepare_ShipmentFromDb(gShip.PackageID, shipment) Then
                                If _EndiciaWeb.Upload_Shipment(shipment) Then
                                    Shipment_Process()

                                End If
                            End If
                        End If

                    ElseIf "SPEE-DEE" = gSelectedShipmentChoice.Carrier Then
                        IO_UpdateSQLProcessor(gShipriteDB, "Update Manifest set Exported='Pending' WHERE PackageID='" & gShip.PackageID & "'")
                        Shipment_Process()
                    End If
                    '
                End If

                If Not gCompletedPackageStack = "" Then
                    Write_Shipment_To_POS(, Packing_Charge.Text, Packing_Weight.Text)
                    Clear_PackingItems()
                End If

            End If


        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try

    End Sub

    Private Sub Print_Batch_Label()
        'ToDo -  Print thermal batch label
    End Sub

    Private Sub Shipment_Process()
        If Not gCompletedPackageStack = "" Then

            gCompletedPackageStack = gCompletedPackageStack & ", "

        End If
        gCompletedPackageStack = "'" & gCompletedPackageStack & gShip.PackageID & "'"

        '
        If gThirdPartyInsurance = True And gShip.DecVal > 0 Then

            If GetPolicyData(gShipriteDB, "EnableShipsurance") = "True" Then
                Call Go_Online_DSI(gShip.PackageID)
            End If
            If GetPolicyData(gShipriteDB, "EnableShipAndInsure") = "True" Then
                Call Go_Online_ShipAndInsure(gShip.PackageID)
            End If

        End If

        'Print Commercial invoice
        If gSelectedShipmentChoice.IsInternational = True And gSelectedShipmentChoice.Carrier <> "USPS" Then
            Cursor = Cursors.Wait
            ReportsManager.PrintCommercialInvoice(gShip.PackageID)
            Cursor = Cursors.Arrow
        End If

        Update_ShippingTotals_For_Contact()
        '
        Call ClearShippingForm()
        If gCallingSKU = "SHIP1" Or gCallingSKU = "SHIPL" Then

            Me.Close()

        End If
    End Sub

    Private Sub Update_ShippingTotals_For_Contact()
        Dim SID As String
        Dim segment As String
        Dim Volume As Double
        Dim Count As Integer
        SID = ExtractElementFromSegment("ID", gShipperSegment, "")

        If SID <> "" Then
            segment = IO_GetSegmentSet(gShipriteDB, "SELECT SUM([T1]) as ShipVolume FROM Manifest WHERE SID = " & SID)
            Volume = ExtractElementFromSegment("ShipVolume", segment)

            segment = IO_GetSegmentSet(gShipriteDB, "SELECT Count(*) as ShipCount FROM Manifest WHERE [Exported] <> 'Deleted' AND SID = " & SID)
            Count = ExtractElementFromSegment("ShipCount", segment)

            IO_UpdateSQLProcessor(gShipriteDB, "Update Contacts set PackageCount=" & Count & ", ShippingVolume=" & Volume & " WHERE ID=" & SID)


        End If

    End Sub

    Private Sub Write_Shipment_To_DB()
        Dim SQL As String = ""
        Dim SegmentSet As String = ""
        Dim ManifestSegment As String = ""
        Dim ID As Long = 0
        Dim PackageID As Long = 0
        Dim PickupDate As Date

        SQL = "SELECT MAX(ID) AS MaxID FROM Manifest"
        SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
        ID = Val(ExtractElementFromSegment("MaxID", SegmentSet))
        ID = ID + 1

        ManifestSegment = ""
        ManifestSegment = AddElementToSegment(ManifestSegment, "ID", ID.ToString)
        ManifestSegment = AddElementToSegment(ManifestSegment, "Date", String.Format("{0:M/d/yyyy}", DateTime.Today))
        ManifestSegment = AddElementToSegment(ManifestSegment, "Time", String.Format("{0:hh:mm tt}", DateTime.Now))
        ManifestSegment = AddElementToSegment(ManifestSegment, "Carrier", gSelectedShipmentChoice.Carrier)
        ManifestSegment = AddElementToSegment(ManifestSegment, "P1", gSelectedShipmentChoice.Service)
        ManifestSegment = AddElementToSegment(ManifestSegment, "ServiceName", ExtractElementFromSegment("DESCRIPTION", gSelectedShipmentChoice.Segment))
        ManifestSegment = AddElementToSegment(ManifestSegment, "SHIPPER#", ExtractElementFromSegment("Shipper#", gSelectedShipmentChoice.Segment))
        ManifestSegment = AddElementToSegment(ManifestSegment, "DIMWEIGHT", gSelectedShipmentChoice.DIM_Weight)
        ManifestSegment = AddElementToSegment(ManifestSegment, "LENGTH", gSelectedShipmentChoice.Length)
        ManifestSegment = AddElementToSegment(ManifestSegment, "WIDTH", gSelectedShipmentChoice.Width)
        ManifestSegment = AddElementToSegment(ManifestSegment, "HEIGHT", gSelectedShipmentChoice.Height)

        PickupDate = ExtractElementFromSegment("NextPickUpDate", gSelectedShipmentChoice.Segment, Today)
        If PickupDate < Today Then
            PickupDate = Today
        End If
        ManifestSegment = AddElementToSegment(ManifestSegment, "PICKUPDATE", PickupDate.ToShortDateString)

        If Not IsNothing(gSelectedShipmentChoice.Packaging) AndAlso gSelectedShipmentChoice.Packaging.SettingName <> "" Then
            ManifestSegment = AddElementToSegment(ManifestSegment, "Packaging", gSelectedShipmentChoice.Packaging.SettingName)
        Else
            ManifestSegment = AddElementToSegment(ManifestSegment, "Packaging", Packaging_ComboBox.Text)
        End If

        If gSelectedShipmentChoice.IsLetter Then
            ManifestSegment = AddElementToSegment(ManifestSegment, "LTR", "X")
            gSelectedShipmentChoice.Billable_Weight = gSelectedShipmentChoice.Weight
        End If

        If gSelectedShipmentChoice.IsResidential Then
            ManifestSegment = AddElementToSegment(ManifestSegment, "RES", "X")
        End If
        ManifestSegment = AddElementToSegment(ManifestSegment, "LBS", Math.Ceiling(gSelectedShipmentChoice.Weight))
        ManifestSegment = AddElementToSegment(ManifestSegment, "ZIPCODE", ZipCode.Text)
        ManifestSegment = AddElementToSegment(ManifestSegment, "Z1", gSelectedShipmentChoice.Zone)
        ManifestSegment = AddElementToSegment(ManifestSegment, "BillableWeight", gSelectedShipmentChoice.Billable_Weight)
        ManifestSegment = AddElementToSegment(ManifestSegment, "ScaleReading", Weight.Text)

        ManifestSegment = AddElementToSegment(ManifestSegment, "Country", Country.Text)
        ManifestSegment = AddElementToSegment(ManifestSegment, "RID", Environment.MachineName)
        '
        If gSelectedShipmentChoice.IsInternational Then ManifestSegment = AddElementToSegment(ManifestSegment, "InternationalIndicator", "I")
        If IsNumeric(gSelectedShipmentChoice.Zone) Then ManifestSegment = AddElementToSegment(ManifestSegment, "NumericZone", FlushOut(UCase(gSelectedShipmentChoice.Zone), "ZONE", ""))
        If Me.Shipper.Tag IsNot Nothing Then ManifestSegment = AddElementToSegment(ManifestSegment, "SID", Me.Shipper.Tag.ToString)
        If Me.Consignee.Tag IsNot Nothing Then
            ManifestSegment = AddElementToSegment(ManifestSegment, "CID", Me.Consignee.Tag.ToString)
            If Not _Contact.ShipToContact.ContactID = Me.Consignee.Tag Then
                _Contact.Load_ContactFromDb(Me.Consignee.Tag, _Contact.ShipToContact)
            End If
            ManifestSegment = AddElementToSegment(ManifestSegment, "ShipToName", _Contact.ShipToContact.Name)
        End If
        '
        ManifestSegment = AddElementToSegment(ManifestSegment, "DECVAL", DeclaredValue.Text)
        '
        If IsOn_gThirdPartyInsurance(_Contact.ShipToContact.Country, gSelectedShipmentChoice.Carrier, gSelectedShipmentChoice.Service, gShip.DecVal, String.Empty) Then
            '
            If GetPolicyData(gShipriteDB, "EnableShipsurance") = "True" Then

                If (gShip.DecVal > 100) Or (gShip.DecVal > 0 And gSelectedShipmentChoice.Carrier = "USPS") Then
                    '
                    If gDSIis3rdPartyInsurance Then
                        ManifestSegment = AddElementToSegment(ManifestSegment, "DSI_Exported", "Pending")
                    Else
                        ManifestSegment = AddElementToSegment(ManifestSegment, "DSI_Exported", "N/A")
                    End If

                    ' For the DSI Premiere Program members, ShipRite will transmit to DSI server the whole amount of the declared value starting from $0.01, as DSI will charge premium for them.
                ElseIf gShip.DecVal > 0 And gDSIis3rdPartyInsurance Then
                    '
                    If DSI_PremiereProgramMember Then
                        ManifestSegment = AddElementToSegment(ManifestSegment, "DSI_Exported", "Pending")
                    End If
                    '
                End If

            End If
            '
            ' Set Insurance to zero because 3rd party insurance is on.
            '
            ManifestSegment = AddElementToSegment(ManifestSegment, "INS1", "0")
            ManifestSegment = AddElementToSegment(ManifestSegment, "costINS1", "0")
            ' ManifestSegment = AddElementToSegment(ManifestSegment, "THIRDINS1", ExtractElementFromSegment("THIRDINS1", gSelectedShipmentChoice.AncillaryChargesSegment))
            ' ManifestSegment = AddElementToSegment(ManifestSegment, "costTHIRDINS1", ExtractElementFromSegment("costTHIRDINS1", gSelectedShipmentChoice.AncillaryCostsSegment))
            '
        Else
            '
            ' ManifestSegment = AddElementToSegment(ManifestSegment, "INS1", ExtractElementFromSegment("DECVAL", gSelectedShipmentChoice.AncillaryChargesSegment))
            ' ManifestSegment = AddElementToSegment(ManifestSegment, "costINS1", ExtractElementFromSegment("ACTDECVAL", gSelectedShipmentChoice.AncillaryCostsSegment))
            ManifestSegment = AddElementToSegment(ManifestSegment, "THIRDINS1", "0")
            ManifestSegment = AddElementToSegment(ManifestSegment, "costTHIRDINS1", "0")
            ManifestSegment = AddElementToSegment(ManifestSegment, "DSI_Exported", "N/A")
            '
        End If
        '
        ' To Do: need a field for gShip.NonStandardContainer = Boolean

        'ACCESSORIAL SURCHARGES
        For Each item As ShippingSurcharge In gSelectedShipmentChoice.SurchargesList
            ManifestSegment = AddElementToSegment(ManifestSegment, item.DBField_Manifest_Cost, item.DiscountCost)
            ManifestSegment = AddElementToSegment(ManifestSegment, item.DBField_Manifest_Sell, item.SellPrice)

            'Signature Options
            Select Case item.ID
                Case ShippingSurcharge.IDs_FedEx.Sig_Ind, ShippingSurcharge.IDs_UPS.Sig_DelConf
                    ManifestSegment = AddElementToSegment(ManifestSegment, "Fx_SigType", 2)  'indirect
                Case ShippingSurcharge.IDs_FedEx.Sig_Dir, ShippingSurcharge.IDs_UPS.Sig_Req, ShippingSurcharge.IDs_USPS.Sig_Conf
                    ManifestSegment = AddElementToSegment(ManifestSegment, "Fx_SigType", 3)  'signature
                Case ShippingSurcharge.IDs_FedEx.Sig_Adult, ShippingSurcharge.IDs_UPS.Sig_Adult, ShippingSurcharge.IDs_USPS.Sig_Adult
                    ManifestSegment = AddElementToSegment(ManifestSegment, "Fx_SigType", 4)  'adult
            End Select

        Next

        ' if ShipAndInsure set the insurance cost to the third party cost

        If gShip.SignatureType = 0 Then
            'include 'No Signature' tag in request.
            ManifestSegment = AddElementToSegment(ManifestSegment, "Fx_SigType", 0)  'no sig
        End If

        ' FedEx Home Delivery Date Certain
        If Not 0 = Val(ExtractElementFromSegment("costFedEXHDCertain", ManifestSegment)) Then
            If _Date.IsDate_(gShip.HOMEFedEXDeliveryDate) Then
                ManifestSegment = AddElementToSegment(ManifestSegment, "FEDEXDeliveryDate", gShip.HOMEFedEXDeliveryDate)
            End If
        End If

        ' Adjust new total if ShipAndInsure in effect

        If GetPolicyData(gShipriteDB, "EnableShipAndInsure") = "True" And gShip.ShipAndInsureCost > 0 Then

            gSelectedShipmentChoice.TotalDiscountCost = gSelectedShipmentChoice.TotalDiscountCost - Val(ExtractElementFromSegment("costINS1", ManifestSegment)) + gShip.ShipAndInsureCost

        End If

        'Shipment Total cost and total sell price
        ManifestSegment = AddElementToSegment(ManifestSegment, "costT1", gSelectedShipmentChoice.TotalDiscountCost)
        ManifestSegment = AddElementToSegment(ManifestSegment, "T1", gSelectedShipmentChoice.TotalSell)

        'Total of shipping without surcharges
        ManifestSegment = AddElementToSegment(ManifestSegment, "costCH1", gSelectedShipmentChoice.DiscountCost)
        ManifestSegment = AddElementToSegment(ManifestSegment, "CH1", gSelectedShipmentChoice.Sell)


        If gShip.DryIceValue > 0 Then
            ManifestSegment = AddElementToSegment(ManifestSegment, fldDryIce_Cost, ExtractElementFromSegment(fldDryIce_Cost, gSelectedShipmentChoice.Sell))
            ManifestSegment = AddElementToSegment(ManifestSegment, fldDryIce_Charge, ExtractElementFromSegment(fldDryIce_Charge, gSelectedShipmentChoice.BaseCost))
            ManifestSegment = AddElementToSegment(ManifestSegment, "ABHazMat", gShip.DryIceValue.ToString & " KG")
        End If

        If Not String.IsNullOrEmpty(gShip.HoldAtLocationID) Then
            ManifestSegment = AddElementToSegment(ManifestSegment, "ABHoldAtAirport", gShip.HoldAtLocationID)
        End If

        If gShip.InsideDelivery Then
            ManifestSegment = AddElementToSegment(ManifestSegment, "InsideDelivery", "Y")
        End If

        If gShip.InsidePickup Then
            ManifestSegment = AddElementToSegment(ManifestSegment, "InsidePickup", "Y")
        End If

        ManifestSegment = AddElementToSegment(ManifestSegment, "Contents", Me.Content.Text)
        ManifestSegment = AddElementToSegment(ManifestSegment, "CustomsTypeOfContents", Customs.Customs_Contents_Type)

        If GetPolicyData(gShipriteDB, "EnableShipAndInsure") = "True" Then

            If gShip.ShipAndInsureCost > 0 Then

                ManifestSegment = AddElementToSegment(ManifestSegment, "costT1", Format(gSelectedShipmentChoice.TotalDiscountCost, "0.00"))
                ManifestSegment = AddElementToSegment(ManifestSegment, "ShipAndInsure", Format(gShip.ShipAndInsureCost, "0.00"))
                ManifestSegment = AddElementToSegment(ManifestSegment, "costINS1", Format(gShip.ShipAndInsureCost, "0.00"))

            End If
            ' Adjust new total if ShipAndInsure in effect

        End If


        gShip.PackageID = GetPackageID()

        ManifestSegment = AddElementToSegment(ManifestSegment, "PACKAGEID", gShip.PackageID)
        SQL = MakeInsertSQLFromSchema("Manifest", ManifestSegment, gManifestSchema, True, False)
        _Debug.Print_(SQL)
        If -1 = IO_UpdateSQLProcessor(gShipriteDB, SQL) Then
            '
            Exit Sub ' error
            '
        Else
            '
            Call Save_CustomsItems()

        End If
    End Sub



#End Region


#Region "UserInput_And_Events"

    Private Sub Carrier_Packaging_CmbBx_SelectionChanged()

        ProcessShippingRates()
    End Sub

    Private Sub ZipCode_PreviewTextInput(sender As Object, e As TextCompositionEventArgs) Handles ZipCode.PreviewTextInput
        If Country.Text = "United States" Then
            Dim allowedchars As String = "0123456789"
            If allowedchars.IndexOf(CChar(e.Text)) = -1 Then e.Handled = True
        End If

    End Sub

    Private Sub Clear_Button_Click() Handles Clear_Button.Click

        Call ClearShippingForm()

    End Sub

    Private Sub PackMaster_Button_Click(sender As Object, e As RoutedEventArgs) Handles PackMaster_Button.Click

        Dim Segment As String = ""
        Dim ReturnSegment As String = ""
        Dim L As Integer = 0
        Dim W As Integer = 0
        Dim H As Integer = 0
        Dim buf As String = ""
        Dim i As Integer
        Dim j As Integer
        Dim amt As Double = 0
        Dim amt2 As Double = 0
        Dim TaxRate As Double = 0
        Dim AccumulatedSalesTax As Double = 0
        Dim InnerBox As String = ""
        Dim OuterBox As String = ""
        Dim WrapQty As Double
        Dim FillQty As Double

        Dim itemL As Single : itemL = Val(Length_TxtBx.Text)
        Dim itemW As Single : itemW = Val(Width_TxtBx.Text)
        Dim itemH As Single : itemH = Val(Height_TxtBx.Text)

        Dim wrapHight As Single = 0.5
        Dim wrapLWH As Single
        Dim wrapL As Single
        Dim wrapW As Single
        Dim wrapH As Single
        Dim wrapVol As Single
        Dim itemVol As Single : itemVol = itemL * itemW * itemH
        Dim outerVol As Integer


        Saved_Packjob.SelectedIndex = -1

        If itemL <> 0 And itemW <> 0 And itemH <> 0 Then
            TaxRate = Val(ExtractElementFromSegment("TaxRate", gPOSDefaultTaxSegment)) / 100

            gResult = ""
            Segment = AddElementToSegment(Segment, "Weight", gShip.actualWeight)
            Segment = AddElementToSegment(Segment, "L", gShip.Length)
            Segment = AddElementToSegment(Segment, "W", gShip.Width)
            Segment = AddElementToSegment(Segment, "H", gShip.Height)
            Segment = AddElementToSegment(Segment, "DecVal", gShip.DecVal)
            Segment = AddElementToSegment(Segment, "NoOfPcs", 1)
            ReturnSegment = FragilityCalculator("FRAGILE", Segment)

            i = 3


            j = GetIndexOfMaterials("Box")

            If Not j = -1 Then

                L = Val(ExtractElementFromSegment("L", gItemSet(j).SKU.InventorySegment(i)))
                W = Val(ExtractElementFromSegment("W", gItemSet(j).SKU.InventorySegment(i)))
                H = Val(ExtractElementFromSegment("H", gItemSet(j).SKU.InventorySegment(i)))
                outerVol = L * W * H
                InnerBox = L.ToString & " x " & W.ToString & " x " & H.ToString
                amt = Val(ExtractElementFromSegment("Sell", gItemSet(j).SKU.InventorySegment(i)))
                If Not ExtractElementFromSegment("Non_Taxable", gItemSet(j).SKU.InventorySegment(i)) = "True" Then

                    AccumulatedSalesTax = AccumulatedSalesTax + (amt * TaxRate)

                End If

            End If

            j = GetIndexOfMaterials("DoubleBox")

            If Not j = -1 Then

                L = Val(ExtractElementFromSegment("L", gItemSet(j).SKU.InventorySegment(i)))
                W = Val(ExtractElementFromSegment("W", gItemSet(j).SKU.InventorySegment(i)))
                H = Val(ExtractElementFromSegment("H", gItemSet(j).SKU.InventorySegment(i)))
                OuterBox = L.ToString & " x " & W.ToString & " x " & H.ToString
                If OuterBox = "0 x 0 x 0" Then

                    buf = "Box Size: " & InnerBox

                Else

                    buf = "Inner Box: " & InnerBox + vbCrLf + "Outer Box: " & OuterBox

                End If

            End If

            amt2 = Val(ExtractElementFromSegment("Sell", gItemSet(j).SKU.InventorySegment(i)))
            If Not ExtractElementFromSegment("Non_Taxable", gItemSet(j).SKU.InventorySegment(i)) = "True" Then

                AccumulatedSalesTax = AccumulatedSalesTax + (amt2 * TaxRate)

            End If



            amt += amt2
            PackPop_Boxes.Content = buf
            PackPop_BoxPrice.Text = Format(amt, "$ 0.00")



            j = GetIndexOfMaterials("Wrap")
            wrapLWH = Val(gItemSet(j).Units.L(i)) * wrapHight
            wrapL = itemL + wrapLWH
            wrapW = itemW + wrapLWH
            wrapH = itemH + wrapLWH
            wrapVol = wrapL * wrapW * wrapH
            If Not j = -1 Then
                WrapQty = _Convert.Round_Double2Decimals((((2 * (itemL * itemW)) + (2 * (itemW * itemH)) + (2 * (itemL * itemH))) / 144) * gItemSet(j).Units.L(i), 1) ''mm#9.83(3/15).

                amt2 = Val(ExtractElementFromSegment("Sell", gItemSet(j).SKU.InventorySegment(i))) * WrapQty
                If Not ExtractElementFromSegment("Non_Taxable", gItemSet(j).SKU.InventorySegment(i)) = "True" Then

                    AccumulatedSalesTax = AccumulatedSalesTax + (amt2 * TaxRate)

                End If

            End If

            j = GetIndexOfMaterials("Fill")
            Dim fillHight As Single = Val(gItemSet(j).Units.L(i))
            Dim fillVol As Single = (wrapL + fillHight) * (wrapW + fillHight) * (wrapH + fillHight)


            If 1 = gItemSet(j).Units.L(i) Then

                If 0 < wrapLWH Then
                    FillQty = _Convert.Round_Double2Decimals((outerVol - wrapVol) / 1728, 1)
                Else
                    FillQty = _Convert.Round_Double2Decimals((outerVol - itemVol) / 1728, 1)

                End If
            Else
                If 0 < wrapLWH Then
                    FillQty = _Convert.Round_Double2Decimals((outerVol - wrapVol + fillVol) / 1728, 1)
                Else
                    FillQty = _Convert.Round_Double2Decimals((outerVol - wrapVol + fillVol) / 1728, 1)

                End If
            End If
            If Not j = -1 Then


                amt = Val(ExtractElementFromSegment("Sell", gItemSet(j).SKU.InventorySegment(i))) * FillQty
                If Not ExtractElementFromSegment("Non_Taxable", gItemSet(j).SKU.InventorySegment(i)) = "True" Then

                    AccumulatedSalesTax += (amt * TaxRate)

                End If

                amt += amt2
                PackPop_PackPrice.Text = Format(amt, "$ 0.00")
            End If


            j = GetIndexOfMaterials("Labor")
            If Not j = -1 Then

                amt = Val(ExtractElementFromSegment("Sell", gItemSet(j).SKU.InventorySegment(i))) * (gItemSet(j).Units.L(i) / 60)
                If Not ExtractElementFromSegment("Non_Taxable", gItemSet(j).SKU.InventorySegment(i)) = "True" Then

                    AccumulatedSalesTax = AccumulatedSalesTax + (amt * TaxRate)

                End If
                PackPop_Labor.Text = Format(amt, "$ 0.00")

            End If
            PackPop_Tax.Text = Format(AccumulatedSalesTax, "$ 0.00")
            PackPop_Total.Text = Format(ValFix(PackPop_BoxPrice.Text) + ValFix(PackPop_PackPrice.Text) + ValFix(PackPop_Labor.Text), "$ 0.00")

        Else
            PackPop_PackPrice.Text = Format(amt, "$ 0.00")
            PackPop_Labor.Text = Format(amt, "$ 0.00")
            PackPop_PackPrice.Text = Format(amt, "$ 0.00")
            PackPop_BoxPrice.Text = Format(amt, "$ 0.00")
            PackPop_Boxes.Content = "Box Size: "
            PackPop_Total.Text = Format(amt, "$ 0.00")

        End If

        PackMaster_Popup.IsOpen = True



    End Sub

    Private Sub Cancel_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Cancel_Btn.Click
        PackMaster_Popup.IsOpen = False
    End Sub

    Private Sub Modify_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Modify_Btn.Click


        PackMaster_Popup.IsOpen = False



        gShipmentParameters = ""
        gShipmentParameters = AddElementToSegment(gShipmentParameters, "Weight", Weight.Text)
        gShipmentParameters = AddElementToSegment(gShipmentParameters, "Length", Length_TxtBx.Text)
        gShipmentParameters = AddElementToSegment(gShipmentParameters, "Width", Width_TxtBx.Text)
        gShipmentParameters = AddElementToSegment(gShipmentParameters, "Height", Height_TxtBx.Text)
        gShipmentParameters = AddElementToSegment(gShipmentParameters, "DeclaredValue", DeclaredValue.Text)
        gShipmentParameters = AddElementToSegment(gShipmentParameters, "Contents", Content.Text)
        isOpen_ShipNew = True
        Dim win As New Packmaster(Me)
        win.ShowDialog(Me)
        isOpen_ShipNew = False
    End Sub

    Private Sub Weight_KeyDown(sender As Object, e As KeyEventArgs) Handles Weight.KeyDown
        Select Case e.Key
            Case Key.Return
                Length_TxtBx.Focus()
            Case Key.Escape
                ' Re-Enable Scale input
                Length_TxtBx.Focus()
                Weight.Text = "0.00"
                ConnectedScale.IsWeightKeyed = False
                LoadScale()
            Case Key.Tab ' skip
            Case Else
                ' Disable Scale Input
                StopScale()
                ConnectedScale.IsWeightKeyed = True
                ConnectedScale.StopScale = True 'EnableScaleInput = False
        End Select
    End Sub

    Private Sub Width_KeyDown(sender As Object, e As KeyEventArgs) Handles Width_TxtBx.KeyDown

        If e.Key = Key.Return Then

            Height_TxtBx.Focus()

        End If

    End Sub

    Private Sub Height_KeyDown(sender As Object, e As KeyEventArgs) Handles Height_TxtBx.KeyDown

        If e.Key = Key.Return Then

            ZipCode.Focus()

        End If

    End Sub

    Private Sub ZipCode_KeyDown(sender As Object, e As KeyEventArgs) Handles ZipCode.KeyDown

        If e.Key = Key.Return Then

            DeclaredValue.Focus()

        End If

    End Sub

    Private Sub DeclaredValue_KeyDown(sender As Object, e As KeyEventArgs) Handles DeclaredValue.KeyDown

        If e.Key = Key.Return Then

            Shipper.Focus()

        End If

    End Sub

    Private Sub Length_KeyDown(sender As Object, e As KeyEventArgs) Handles Length_TxtBx.KeyDown

        If e.Key = Key.Return Then

            Width_TxtBx.Focus()

        End If

    End Sub

    Private Sub TxtBox_LostFocus(sender As Object, e As RoutedEventArgs) Handles Weight.LostFocus, ZipCode.LostFocus, DeclaredValue.LostFocus, Country.SelectionChanged
        If sender.Name = "Weight" Then
            ConnectedScale.StopScale = False
        End If
        ProcessShippingRates()
    End Sub

    Private Sub LWH_LostFocus(sender As Object, e As RoutedEventArgs) Handles Length_TxtBx.LostFocus, Width_TxtBx.LostFocus, Height_TxtBx.LostFocus

        Dim L As Double
        Dim W As Double
        Dim H As Double

        Try
            L = Val(Length_TxtBx.Text)
            W = Val(Width_TxtBx.Text)
            H = Val(Height_TxtBx.Text)

        Catch
            Exit Sub
        End Try


        LengthGirth.Text = Calculate_Length_Plus_Girth(L, W, H) & " in."
        Volume.Text = Calculate_Volume(L, W, H).ToString("N1") & " cu. ft"

        ProcessShippingRates()

    End Sub

    Private Sub SortBy_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles SortBy.SelectionChanged
        ProcessShippingRates()
    End Sub

    Private Overloads Sub RefreshButton_Click(sender As Object, e As RoutedEventArgs)

        Call ClearShippingForm()

    End Sub

    Private Sub Surcharges_UserInput(sender As Object, e As RoutedEventArgs) Handles SatDelivery_Btn.Click, AdditionalHandling_Btn.Click, Residential_Btn.Click, DAS_Btn.Click, COD_Btn.Click,
                                                                                 DelConf_Btn.Checked, SigConfirm_Btn.Checked, AdultSig_Btn.Checked, DelConf_Btn.Unchecked, SigConfirm_Btn.Unchecked, AdultSig_Btn.Unchecked

        Dim CurrentButton As System.Windows.Controls.Primitives.ToggleButton = TryCast(sender, System.Windows.Controls.Primitives.ToggleButton)

        Select Case CurrentButton.Name

            Case "COD_Btn"
                If CurrentButton.IsChecked = True Then
                    Dim COD_return As String
                    COD_return = InputBox("Enter COD Amount",)

                    If COD_return <> "" And IsNumeric(COD_return) Then
                        COD_Btn.Content = COD_Btn.Content & vbCrLf & "Amt: $" & COD_return
                        COD_Btn.Tag = Val(COD_return)

                        MsgBox("Would you like to add the Shipping Charges to the COD Amount?", vbYesNo + vbQuestion)

                    Else
                        MsgBox("Invalid Entry!", vbInformation)
                        CurrentButton.IsChecked = False
                    End If
                Else
                    COD_Btn.Content = "C.O.D"
                End If



            Case "DelConf_Btn"

                If Me.DelConf_Btn.IsChecked Then
                    Me.SigConfirm_Btn.IsChecked = False
                    Me.AdultSig_Btn.IsChecked = False
                End If


            Case "SigConfirm_Btn"

                If Me.SigConfirm_Btn.IsChecked Then
                    Me.DelConf_Btn.IsChecked = False
                    Me.AdultSig_Btn.IsChecked = False
                End If


            Case "AdultSig_Btn"

                If Me.AdultSig_Btn.IsChecked Then
                    Me.DelConf_Btn.IsChecked = False
                    Me.SigConfirm_Btn.IsChecked = False
                End If


        End Select

        Call ProcessShippingRates()

    End Sub

    Private Function DecVal_LostFocus() As Boolean
        DecVal_LostFocus = False ' assume.
        Dim isDSI As Boolean
        '
        If fDeclaredAsked Then
            '
            gShip.DecVal = Val(Me.DeclaredValue.Text)
            '
            If IsOn_gThirdPartyInsurance(String.Empty) Then
                '
                ' Shiprite could have other than DSI as 3rd Party Insurer, but only one name at a time.
                isDSI = gDSIis3rdPartyInsurance
                '
                ' DSI excluded country list added to block insuring International shipments with DSI that are in this list.
                If isDSI AndAlso Not String.IsNullOrEmpty(_Contact.ShipToContact.Country) AndAlso DSI_IsCountryInExcludedList(_Contact.ShipToContact.Country) Then
                    '
                    isDSI = False ' to elliminate further DSI checks
                    MsgBox(_Contact.ShipToContact.Country & " is in the " & DSI_NewName & " Excluded Country List!" & vbCr &
                       "You won't be able to insure this shipment with " & DSI_NewName & "." & vbCr & vbCr &
                       "This shipment will be insured with the carrier that you will select.", vbExclamation, DSI_NewName & " Insurance Policy !")
                    '
                    ' must inspect the package valued at $500 and over.
                ElseIf Val(Me.DeclaredValue.Text) >= 500 And isDSI Then
                    '
                    If vbNo = MsgBox("Did you inspect the package?" & vbCr & vbCr & "You must inspect the package valued at $500 and over." _
                              , vbYesNo + vbQuestion, "Insurance Requirement !") Then
                        ' cannot change Declared Value in the program, allow the customer to make the decision.
                        If vbNo = MsgBox("WARNING: Selecting <No> means your package will be insured for 'Loss' only; 'Damage' claims are barred." & vbCr & vbCr &
                                 "Click <Yes> to ship this package and insure for 'Loss' only." & vbCr &
                                 "Click <No> to go back and lower the declared value to less than $500.", vbExclamation + vbYesNo, "Insurance Requirement !") Then
                            '
                            Me.DeclaredValue.Focus()
                            Exit Function
                            ''
                        End If
                        '
                    End If
                End If
                '
                ' The code below will force the Delivery with Signature if it is > $1000
                If Val(Me.DeclaredValue.Text) >= Val(gDSISig) And isDSI Then
                    If Not Me.DelConf_Btn.IsChecked Then
                        '
                        ' direct signature required on all packages valued at $1,000.00 and over.
                        If vbNo = MsgBox("ATTENTION... Adult Signature Required?", vbYesNo + vbQuestion, gProgramName) Then
                            Me.DelConf_Btn.IsChecked = True
                        Else
                            Me.AdultSig_Btn.IsChecked = True
                        End If
                        '
                    End If
                End If
                '
                If Val(Me.DeclaredValue.Text) > 15000 And isDSI Then
                    '
                    MsgBox(DSI_NewName & " has a limit of Liability of $15,000 " & vbCr_ &
                        "If you wish to Insure this package for more than $15,000 you have to " &
                        "go back to Setup -> Options, and Disable Third Party Insurance, then Reship " &
                        "Your Declared Value will now be reset to $15,000")

                    Me.DeclaredValue.Text = "15000"
                    '
                ElseIf Val(Me.DeclaredValue.Text) > 10000 Then
                    '
                    MsgBox("WARNING: You are shipping an extremely high value shipment. Different carriers and Third Party Insurance Companies have different " &
                       "limits of liablities depending on what the item is, and different calculations of declared value " &
                       "charges.  You should manually check with the carrier to make sure this item can be insured, and " &
                       "you should double check to see if the declared value charge to your customer is correct.  You " &
                       "should also make sure this item is properly packed.", vbOKOnly)
                End If
                '
            End If
            '
            If Val(Me.DeclaredValue.Text) > 50000 Then
                '
                MsgBox("The maximum declared value allowed by FedEx, UPS, and DHL is $50,000." &
                    vbCr_ & vbCr_ & "Please Enter a VALID Declared Value amount!!!", vbInformation)
                '
                Me.DeclaredValue.Text = ""
                Me.DeclaredValue.Focus()
                '
            End If
            '
            DecVal_LostFocus = True
            '
        End If
        '
        fDeclaredAsked = False ' reset
        '
    End Function

    Private Sub DeclaredValue_TextChanged(sender As Object, e As TextChangedEventArgs) Handles DeclaredValue.TextChanged
        fDeclaredAsked = True
    End Sub

    Private Sub Consignee_LostFocus(sender As Object, e As RoutedEventArgs) Handles Consignee.LostFocus
        Try

            If Consignee.Text = "" Then
                gConsigneeSegment = ""
                Country.IsEnabled = True
            End If

            ZipCode.Text = ExtractElementFromSegment("Zip", gConsigneeSegment)
            If HotSearch_Popup.IsOpen = False Then
                Me.Weight.Focus()
            End If

            ProcessShippingRates()
        Catch ex As Exception
        End Try
    End Sub

    Private Sub ThirdPartyIns_Button_Click(sender As Object, e As RoutedEventArgs) Handles ThirdPartyIns_Button.Click
        Try
            If ThirdPartyInsurance.Text = "ON" Then

                If MsgBox("You are turning OFF Third Party Insurance, the shipment will be insured with the carrier. Do you want to proceed?", MsgBoxStyle.OkCancel + MsgBoxStyle.Question, "WARNING - Third Party Insurance") = MsgBoxResult.Ok Then
                    ThirdPartyInsurance.Text = "OFF"
                    gThirdPartyInsurance = False
                    Log_ThirdPartyInsurance_Switch(False)
                End If

            Else
                If MsgBox("You are turning ON Third Party Insurance, the shipment will NOT be insured with the carrier. Do you want to proceed?", MsgBoxStyle.OkCancel + MsgBoxStyle.Question, "WARNING - Third Party Insurance") = MsgBoxResult.Ok Then
                    ThirdPartyInsurance.Text = "ON"
                    gThirdPartyInsurance = True
                    Log_ThirdPartyInsurance_Switch(True)
                End If
            End If

            ProcessShippingRates()

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Log_ThirdPartyInsurance_Switch(ByRef isTurnOn As Boolean)
        Try

            Dim file As System.IO.StreamWriter = My.Computer.FileSystem.OpenTextFileWriter(gDBpath & "\ThirdPartyInsuranceLog.txt", True)
            Dim WriteString As String

            If isTurnOn Then
                WriteString = "Turned ON: "
            Else
                WriteString = "Turned OFF: "
            End If

            WriteString = WriteString & Now


            If gIsSetupSecurityEnabled Or gIsProgramSecurityEnabled Or gIsPOSSecurityEnabled Then
                Dim win As New UserLogIn(Me, "")
                win.ShowDialog()
                WriteString = WriteString & " - User: " & gCurrentUser
            End If


            file.WriteLine(WriteString)
            file.Close()

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Accept_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Accept_Btn.Click

        Dim ret As Long = 0
        Dim j As Integer = 0
        PackMaster_Popup.IsOpen = False
        isOpen_ShipNew = True
        ret = PostPackagingToPOS(3)
        isOpen_ShipNew = False

        j = GetIndexOfMaterials("Box")
        If j > 0 Then

            Length_TxtBx.Text = ExtractElementFromSegment("L", gItemSet(j).SKU.InventorySegment(3))
            Width_TxtBx.Text = ExtractElementFromSegment("W", gItemSet(j).SKU.InventorySegment(3))
            Height_TxtBx.Text = ExtractElementFromSegment("H", gItemSet(j).SKU.InventorySegment(3))

        Else

            j = GetIndexOfMaterials("DoubleBox")
            Length_TxtBx.Text = ExtractElementFromSegment("L", gItemSet(j).SKU.InventorySegment(3))
            Width_TxtBx.Text = ExtractElementFromSegment("W", gItemSet(j).SKU.InventorySegment(3))
            Height_TxtBx.Text = ExtractElementFromSegment("H", gItemSet(j).SKU.InventorySegment(3))

        End If

        Packing_Charge.Text = PackPop_Total.Text
        j = GetIndexOfMaterials("PackagingWeight")
        Packing_Weight.Text = gItemSet(j).PackagingWeight(3)

    End Sub

    Private Sub ShowPackageDetails_Click(sender As Object, e As RoutedEventArgs)

        Dim MenuItem As MenuItem = DirectCast(sender, MenuItem)
        Dim parentContextMenu As ContextMenu = DirectCast(MenuItem.CommandParameter, ContextMenu)
        Dim current_button As Button = parentContextMenu.PlacementTarget

        Dim shippingChoice As New ShippingChoiceDefinition
        shippingChoice = current_button.Tag

        If gCurrentUser <> "" AndAlso (gIsPOSSecurityEnabled Or gIsProgramSecurityEnabled) Then
            If Not Check_Current_User_Permission("View_Shipping_Costs") Then
                Exit Sub
            End If
        End If

        If shippingChoice.Service IsNot Nothing Then

            DetailsServiceName.Content = shippingChoice.Service
            DetailsZoneName.Content = shippingChoice.Zone

            ShowPackageDetails_Popup.PlacementTarget = Weight
            ShowPackageDetails_Popup.IsOpen = True

            'Display markup percentage
            ShowPackageDetails_Markup_TxtBx.Text = Format(((shippingChoice.Sell - Markup_From_Base_OR_Discount(shippingChoice)) / Markup_From_Base_OR_Discount(shippingChoice)), "0.0 %")

            ShowPackageDetails_Discount_TxtBx.Text = Format(((shippingChoice.BaseCost - shippingChoice.DiscountCost) / shippingChoice.BaseCost), "0.0 %")

            Load_ShowPackageDetails_LV(ShowPackageDetails_LV, shippingChoice)

            If shippingChoice.IsFlatRate = False Then
                ShowPackageDetails_BillWeight_txt.Text = shippingChoice.Billable_Weight & " lb"
            Else
                ShowPackageDetails_BillWeight_txt.Text = "Flat Rate"
            End If
        End If

    End Sub

    Public Shared Sub Load_ShowPackageDetails_LV(ByRef LV As ListView, ByRef shippingChoice As ShippingChoiceDefinition)

        LV.DataContext = Nothing ' make sure clear of binded data
        LV.Items.Clear() ' clear items collection

        'Add Shipping Charge
        Dim shippingDetail As New ShippingDetails With {
            .Service = "Shipping",
            .Sell = Format(Val(shippingChoice.Sell), "$ 0.00"),
            .Cost = Format(Val(shippingChoice.BaseCost), "$ 0.00"),
            .Discount = Format(Val(shippingChoice.DiscountCost), "$ 0.00")
        }
        LV.Items.Add(shippingDetail)


        'Add Surcharges
        For Each item As ShippingSurcharge In shippingChoice.SurchargesList
            shippingDetail = New ShippingDetails With {
                .Service = item.Name,
                .Sell = Format(item.SellPrice, "$ 0.00"),
                .Cost = Format(item.BaseCost, "$ 0.00"),
                .Discount = Format(item.DiscountCost, "$ 0.00")
            }
            LV.Items.Add(shippingDetail)
        Next

        'Add Total Line
        shippingDetail = New ShippingDetails With {
            .Service = "Total",
            .Sell = Format(Val(shippingChoice.TotalSell), "$ 0.00"),
            .Cost = Format(Val(shippingChoice.TotalBaseCost), "$ 0.00"),
            .Discount = Format(Val(shippingChoice.TotalDiscountCost), "$ 0.00")
        }

        LV.Items.Add(shippingDetail)
    End Sub

    Private Sub CloseDetails_Btn_Click(sender As Object, e As RoutedEventArgs) Handles CloseDetails_Btn.Click
        ShowPackageDetails_Popup.IsOpen = False
    End Sub

    Private Sub FROM_Options_Btn_Click(sender As Object, e As RoutedEventArgs) Handles FROM_Options_Btn.Click
        If FROM_Options_Popup.IsOpen = True Then
            FROM_Options_Popup.IsOpen = False
        Else
            FROM_Options_Popup.IsOpen = True
        End If
    End Sub

    Private Sub TO_Options_Btn_Click(sender As Object, e As RoutedEventArgs) Handles TO_Options_Btn.Click
        If TO_Options_Popup.IsOpen = True Then
            TO_Options_Popup.IsOpen = False
        Else
            TO_Options_Popup.IsOpen = True
        End If
    End Sub

    Private Sub DropOff_Btn_Click(sender As Object, e As RoutedEventArgs) Handles DropOff_Btn.Click
        Try

            Call _DropOff.Open_DropOffManager(Me, gCurrentUser, Nothing)

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub

    Private Sub PackageValet_Btn_Click(sender As Object, e As RoutedEventArgs) Handles PackageValet_Btn.Click
        Try

            Dim win As New PackageValet(Me)
            win.ShowDialog(Me)

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub

    Private Sub Shipper_Tickler_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Shipper_Tickler_Btn.Click
        Try


            If ExtractElementFromSegment("Name", gShipperSegment) = "Cash, Check, Charge" Then
                Dim win As New Tickler(Me)
                win.ShowDialog(Me)
            Else
                Dim win As New Tickler(Me, gShipperSegment)
                win.ShowDialog(Me)
            End If

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

#End Region

#Region "Scale"

    Private Scale_Timer As System.Threading.Timer
    Private ConnectedScale As Scale.BaseScale = New Scale.BaseScale()
    Private ScaleCache As String = ""
    Private ScaleUpdateDelay As Int32 = 250
    Private Sub ScaleFillMainThread()
        If Not ConnectedScale.StopScale And Not ConnectedScale.IsWeightKeyed Then
            Weight.Text = ScaleCache
        End If
    End Sub

    Private Sub ScaleFillBGThread()
        ' Pull current weight from the scale and populate Weight.Text
        If ConnectedScale.Model_IsValid AndAlso Not ConnectedScale.IsError Then
            Dim errorCache As String = ""
            ScaleCache = ConnectedScale.Get_Weight(errorCache)
            If ScaleCache = "" Or errorCache.Length > 0 Then
                Me.Dispatcher.Invoke(Sub() StopScale())
            End If
            If errorCache.Length > 0 Then
                If Not ConnectedScale.IsError Then
                    ConnectedScale.IsError = True
                    Me.Dispatcher.Invoke(Sub() _MsgBox.WarningMessage(errorCache, msgboxTitle:="Scale Error"))
                End If
            Else
                Me.Dispatcher.Invoke(Sub() ScaleFillMainThread())
            End If
        End If
    End Sub

    Private Sub LoadScale()
        If Not ConnectedScale.IsWeightKeyed And Not ConnectedScale.IsError Then
            StopScale()
            ' start timer on background thread to call ScaleFill() function only one time and delay by AutoComplete_Timeout secs
            ConnectedScale.Load_ScaleFromPolicy()
            Scale_Timer = New Threading.Timer(Sub() ScaleFillBGThread(), Nothing, 500, ScaleUpdateDelay)
            Debug.Print("Scale Enabled")
            ConnectedScale.StopScale = False 'EnableScaleInput = True
        End If
    End Sub

    Private Sub StopScale()
        If Scale_Timer IsNot Nothing Then
            Scale_Timer.Dispose()
            Scale_Timer = Nothing
            Debug.Print("Scale Disabled")
        End If
    End Sub
#End Region

#Region "From/To Contact selection"

    Private Sub ConsigneeShipper_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles Consignee.MouseDoubleClick, Shipper.MouseDoubleClick
        OpenContactManager(sender)
    End Sub

    Private Sub OpenContactManager(senderTxtBx As TextBox)
        Dim CID As String = "0"
        Dim SearchText As String

        gAutoExitFromContacts = True

        If IsNothing(senderTxtBx.Tag) Or senderTxtBx.LineCount = 1 Then
            'Search
            SearchText = senderTxtBx.Text

            If SearchText.Contains(Environment.NewLine) Then
                SearchText = SearchText.Substring(0, SearchText.IndexOf(Environment.NewLine))
            End If

            If senderTxtBx.Name = "Consignee" Then
                Dim win As New ContactManager(Me, , SearchText,, True)
                win.ShowDialog(Me)
            Else
                Dim win As New ContactManager(Me, , SearchText)
                win.ShowDialog(Me)
            End If


        Else
            'Customer selected, pull up that customer in contact manager
            If senderTxtBx.Name = "Consignee" Then
                Dim win As New ContactManager(Me, senderTxtBx.Tag,,, True)
                win.ShowDialog(Me)
            Else
                Dim win As New ContactManager(Me, senderTxtBx.Tag)
                win.ShowDialog(Me)
            End If
        End If


        CID = ExtractElementFromSegment("ID", gContactManagerSegment, "0")

        If Not CID = 0 Then

            If senderTxtBx.Name = "Consignee" Then
                Load_Consignee(CID)

            Else
                Load_Shipper(CID)

            End If
        End If

        gContactManagerSegment = ""
    End Sub

    Private Sub ConsigneeShipper_TextChanged(sender As Object, e As TextChangedEventArgs) Handles Consignee.TextChanged, Shipper.TextChanged

        If sender.Text <> "" And isLoaded = True And gContactManagerSegment = "" Then
            ContactManager.Search_LoadList(sender.Text, Search_LB, 8)

            HotSearch_Popup.PlacementTarget = sender
            HotSearch_Popup.Tag = sender.name
            HotSearch_Popup.IsOpen = True
        End If
    End Sub

    Private Sub HotSearch_LoadCustomer()
        If Search_LB.SelectedIndex = -1 Then Exit Sub

        Dim item As SearchItem = Search_LB.SelectedItem
        If HotSearch_Popup.Tag = "Consignee" Then
            Load_Consignee(item.ID)
        Else
            Load_Shipper(item.ID)
        End If

        HotSearch_Popup.IsOpen = False
    End Sub

    Private Sub HotSearch_LB_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles Search_LB.MouseLeftButtonUp
        Dim obj As DependencyObject = CType(e.OriginalSource, DependencyObject)


        ' Finds the clicked on ListBoxItem and selects it.
        While obj IsNot Nothing
            If obj.[GetType]() = GetType(ListBoxItem) Then
                Dim LB_Item As ListBoxItem = DirectCast(obj, ListBoxItem)

                Search_LB.SelectedItem = LB_Item.DataContext
                HotSearch_LoadCustomer()
                Exit While
            End If

            obj = System.Windows.Media.VisualTreeHelper.GetParent(obj)
        End While



    End Sub

    Private Sub HotSearch_LB_KeyDown(sender As Object, e As Input.KeyEventArgs) Handles Search_LB.KeyDown

        If e.Key = Key.Return Or e.Key = Key.Tab Then
            HotSearch_LoadCustomer()
        End If
    End Sub

    Private Sub ConsigneeShipper_KeyPreviewDown(sender As Object, e As KeyEventArgs) Handles Consignee.PreviewKeyDown, Shipper.PreviewKeyDown
        Dim CID As String = "0"

        Dim senderTxtBx As TextBox = DirectCast(sender, TextBox)

        senderTxtBx.Foreground = New System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black)

        If e.Key = Key.Return Then

            OpenContactManager(senderTxtBx)


        ElseIf e.Key = Key.Down Then
            'down arrow sets focus on the HotSearch ListView
            If Search_LB.Items.Count > 0 Then
                Search_LB.SelectedIndex = 0
                Search_LB.Focus()
            End If


        ElseIf e.Key = Key.Tab Then
            If Search_LB.HasItems Then
                e.Handled = True
                If Search_LB.SelectedIndex = -1 Then Search_LB.SelectedIndex = 0
                HotSearch_LoadCustomer()

            End If

        ElseIf e.Key = Key.Delete Or e.Key = Key.Back Then
            'pressing delete or backspace on address lines clears out entire textbox.
            If senderTxtBx.GetLineIndexFromCharacterIndex(senderTxtBx.SelectionStart) <> 0 Then

                senderTxtBx.Text = ""

                If senderTxtBx.Name = "Consignee" Then
                    gConsigneeSegment = ""
                Else
                    gShipperSegment = ""
                End If
            End If

        Else
            'only allow the editing of the name on the first line. 
            If senderTxtBx.GetLineIndexFromCharacterIndex(senderTxtBx.SelectionStart) <> 0 Then

                e.Handled = True
                Exit Sub
            Else

                If senderTxtBx.SelectedText = senderTxtBx.Text Then
                    'if all text is selected, then erase it out textbox
                    senderTxtBx.Text = ""

                Else
                    'if editing the first line (name), then delete the address lines below.
                    Dim CharIndex As Integer
                    CharIndex = senderTxtBx.SelectionStart

                    senderTxtBx.Text = senderTxtBx.Text.Split(Environment.NewLine).FirstOrDefault
                    senderTxtBx.SelectionStart = CharIndex
                End If

            End If



        End If

    End Sub

    Private Sub Load_Consignee(ByRef CID As String)
        Dim SQL As String

        SQL = "SELECT * FROM Contacts WHERE ID = " & CID
        gConsigneeSegment = IO_GetSegmentSet(gShipriteDB, SQL)
        Consignee.Text = CreateDisplayBlock(gConsigneeSegment, True)
        Me.Consignee.Tag = CID ' store id
        _Contact.Load_ContactFromDb(CID, _Contact.ShipToContact)

        If ContactManager.Display_Customer_Notes(CID, Consignee_Notes_TxtBx) Then
            Consignee.Foreground = New System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red)
        End If

        If Not 0 = _Contact.ShipToContact.ContactID Then
            Dim countryobject As _CountryDB = Nothing
            If Shipping.Find_CountryObject_byName(_Contact.ShipToContact.Country, countryobject) Then
                Me.Country.SelectedItem = countryobject
                Country.IsEnabled = False
            End If
            Me.ZipCode.Text = _Contact.ShipToContact.Zip
            If _Contact.ShipToContact.Residential = True Then
                Residential_Btn.IsChecked = True
            Else
                Residential_Btn.IsChecked = False
            End If
        End If
        ProcessShippingRates()
        Me.Weight.Focus()
    End Sub

    Private Sub Load_Shipper(ByRef CID As String)
        Dim SQL As String

        SQL = "SELECT * FROM Contacts WHERE ID = " & CID
        gShipperSegment = IO_GetSegmentSet(gShipriteDB, SQL)
        Shipper.Text = CreateDisplayBlock(gShipperSegment, True)

        If ContactManager.Display_Customer_Notes(CID, Shipper_Notes_TxtBx) Then
            Shipper.Foreground = New System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red)
        End If

        Me.Shipper.Tag = CID ' store id
        _Contact.Load_ContactFromDb(CID, _Contact.ShipFromContact)
        Consignee.Focus()

        If gIsCustomerDisplayEnabled Then
            gCustomerDisplay.UpdateShippingRates(Display_CarrierList, Shipper.Text, Consignee.Text)
        End If

    End Sub

    Private Sub Consignee_Edit_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Consignee_Edit_Btn.Click
        If CStr(Consignee.Tag) = "" Then Exit Sub
        Dim CID As String

        Dim win As New ContactManager(Me, CLng(Consignee.Tag))
        win.ShowDialog(Me)

        If gContactManagerSegment <> "" Then
            CID = ExtractElementFromSegment("ID", gContactManagerSegment, "0")

            If Not CID = "0" Then
                Load_Consignee(CID)
            End If

            gContactManagerSegment = ""
        End If


    End Sub

    Private Sub Shipper_Edit_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Shipper_Edit_Btn.Click
        If CStr(Shipper.Tag) = "" Then Exit Sub
        Dim CID As String

        Dim win As New ContactManager(Me, CLng(Shipper.Tag))
        win.ShowDialog(Me)

        If gContactManagerSegment <> "" Then
            CID = ExtractElementFromSegment("ID", gContactManagerSegment, "0")

            If Not CID = "0" Then
                Load_Shipper(CID)
            End If

            gContactManagerSegment = ""
        End If

    End Sub

    Private Sub ShipToSelf_Btn_Click(sender As Object, e As RoutedEventArgs) Handles ShipToSelf_Btn.Click

        If Me.Shipper.Tag IsNot Nothing Then
            Load_Consignee(Me.Shipper.Tag)
            HotSearch_Popup.IsOpen = False
            TO_Options_Popup.IsOpen = False

        End If
    End Sub

    Private Sub SameAsLast_Btn_Click(sender As Object, e As RoutedEventArgs) Handles SameAsLast_Btn.Click
        Dim CID As String
        Dim SQL As String

        'gets customer ID from last shipment in manifest table
        SQL = "SELECT CID from manifest where manifest.id = (select max(id) from manifest where RID = '" & Environment.MachineName & "')"
        CID = ExtractElementFromSegment("CID", IO_GetSegmentSet(gShipriteDB, SQL), "0")

        If Not CID = "0" Then
            Load_Consignee(CID)
            HotSearch_Popup.IsOpen = False
            TO_Options_Popup.IsOpen = False
        End If

    End Sub

    Private Sub Shipper_History_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Shipper_History_Btn.Click
        Try


            If IsNothing(Shipper.Tag) Then
                Dim win As New ShipmentHistory(Me)
                win.ShowDialog(Me)
            Else
                Dim win As New ShipmentHistory(Me, Shipper.Tag)
                win.ShowDialog(Me)
            End If



        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub


    Private Sub Shipper_SetDefault_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Shipper_SetDefault_Btn.Click
        If ExtractElementFromSegment("ID", gShipperSegment, "0") <> 0 Then
            If vbYes = MsgBox("Are you sure you want to make " & ExtractElementFromSegment("Name", gShipperSegment, "0") & " default shipper?", vbYesNo + vbQuestion, "Updating Default Shipper") Then
                UpdatePolicy(gShipriteDB, "DefaultShipFrom", ExtractElementFromSegment("ID", gShipperSegment, "0"))
            End If

        End If


    End Sub

    Private Sub Consignee_History_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Consignee_History_Btn.Click

        Try


            If IsNothing(Consignee.Tag) Then
                Dim win As New ShipmentHistory(Me)
                win.ShowDialog(Me)
            Else
                Dim win As New ShipmentHistory(Me, , Consignee.Tag)
                win.ShowDialog(Me)
            End If

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Consignee_Addresses_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Consignee_Addresses_Btn.Click

        Load_ShipToAddresses_ForShipper()
    End Sub

    Private Sub Load_ShipToAddresses_ForShipper()
        If CStr(Shipper.Tag) = "" Then Exit Sub

        gAutoExitFromContacts = True
        Dim CID As String
        Dim win As New ContactManager(Me, CLng(Shipper.Tag), , True)
        win.ShowDialog(Me)

        If gContactManagerSegment <> "" Then
            CID = ExtractElementFromSegment("ID", gContactManagerSegment, "0")

            If Not CID = "0" Then
                Load_Consignee(CID)
            End If

            gContactManagerSegment = ""
        End If
    End Sub


    Private Sub Consignee_DeleteNote_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Consignee_DeleteNote_Btn.Click
        ContactManager.Delete_Customer_Notes(Consignee.Tag, Consignee_Notes_TxtBx)
        Consignee.Foreground = New System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black)
        TO_Options_Popup.IsOpen = False
    End Sub

    Private Sub Consignee_SaveNote_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Consignee_SaveNote_Btn.Click
        ContactManager.Save_Customer_Notes(Consignee.Tag, Consignee_Notes_TxtBx.Text)

        If Consignee_Notes_TxtBx.Text <> "" Then
            Consignee.Foreground = New System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red)
        Else
            Consignee.Foreground = New System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black)
        End If
        TO_Options_Popup.IsOpen = False
    End Sub

    Private Sub Shipper_DeleteNote_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Shipper_DeleteNote_Btn.Click
        ContactManager.Delete_Customer_Notes(Shipper.Tag, Shipper_Notes_TxtBx)
        Shipper.Foreground = New System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black)
        FROM_Options_Popup.IsOpen = False
    End Sub

    Private Sub Shipper_SaveNote_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Shipper_SaveNote_Btn.Click
        ContactManager.Save_Customer_Notes(Shipper.Tag, Shipper_Notes_TxtBx.Text)

        If Shipper_Notes_TxtBx.Text <> "" Then
            Shipper.Foreground = New System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red)
        Else
            Shipper.Foreground = New System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black)
        End If
        FROM_Options_Popup.IsOpen = False
    End Sub

#End Region

#Region "Freight LTL"

    Private Sub FreightQuote_Button_Click(sender As Object, e As RoutedEventArgs) Handles FreightQuote_Button.Click
        FreigthQuote_Popup_Load()
        FreigthQuote_Popup.IsOpen = True
    End Sub

    Private Sub Close_FreightPopup_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Close_FreightPopup_Btn.Click
        FedEx_Freight.LTL_Freight = Nothing
        Call empty_FreightFormItems()
        FreigthQuote_Popup.IsOpen = False
    End Sub

    Private Sub Get_LTL_FreightRate_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Get_LTL_FreightRate_Btn.Click
        Try
            If collect_FreightFormItems() Then
                Call ProcessShippingRates()
            End If
            FreigthQuote_Popup.IsOpen = False
        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to collect the data...")
        End Try
    End Sub

    Private Sub FreigthQuote_Popup_Load()

        If _IDs.IsMetricSystem Then ' Metric or Imperial switch was added to have KG or LB and CM or IN.
            lblWeight.Text = "KGs"
        Else
            lblWeight.Text = "LBs"
        End If
        '
        Call FreightQuote_Add_PackagingTypes()
        Call FreightQuote_Add_ClassTypes()
        '
        If String.IsNullOrEmpty(Me.Row1_Description_TextBox.Text) Then
            '
            Me.Row1_Description_TextBox.Text = Me.Content.Text
            Me.Row1_InsuredValue_TextBox.Text = Me.DeclaredValue.Text
            Me.Row1_LBs_TextBox.Text = Me.Weight.Text
            '
        ElseIf FedEx_Freight.LTL_Freight IsNot Nothing Then
            '
            Call fillout_FreightFormItems()
            '
        End If
        '
        Call get_Totals()

    End Sub

    Private Sub FreightQuote_Add_PackagingTypes()
        Call add_PackagingIttems(Me.Row1_PackagingType_ComboBox)
        Call add_PackagingIttems(Me.Row2_PackagingType_ComboBox)
        Call add_PackagingIttems(Me.Row3_PackagingType_ComboBox)
        Call add_PackagingIttems(Me.Row4_PackagingType_ComboBox)
        Call add_PackagingIttems(Me.Row5_PackagingType_ComboBox)
    End Sub

    Private Sub add_PackagingIttems(PackagingType_Control As ComboBox)
        If 0 = PackagingType_Control.Items.Count Then
            PackagingType_Control.Items.Add("BAG")
            PackagingType_Control.Items.Add("BARREL")
            PackagingType_Control.Items.Add("BASKET")
            PackagingType_Control.Items.Add("BOX")
            PackagingType_Control.Items.Add("BUCKET")
            PackagingType_Control.Items.Add("BUNDLE")
            PackagingType_Control.Items.Add("CARTON")
            PackagingType_Control.Items.Add("CASE")
            PackagingType_Control.Items.Add("CONTAINER")
            PackagingType_Control.Items.Add("CRATE")
            PackagingType_Control.Items.Add("CYLINDER")
            PackagingType_Control.Items.Add("DRUM")
            PackagingType_Control.Items.Add("HAMPER")
            PackagingType_Control.Items.Add("PAIL")
            PackagingType_Control.Items.Add("PALLET")
            PackagingType_Control.Items.Add("PIECE")
            PackagingType_Control.Items.Add("REEL")
            PackagingType_Control.Items.Add("ROLL")
            PackagingType_Control.Items.Add("SKID")
            PackagingType_Control.Items.Add("TANK")
            PackagingType_Control.Items.Add("TUBE")
        End If
    End Sub

    Private Sub FreightQuote_Add_ClassTypes()
        Call add_ClassItems(Me.Row1_Class_ComboBox)
        Call add_ClassItems(Me.Row2_Class_ComboBox)
        Call add_ClassItems(Me.Row3_Class_ComboBox)
        Call add_ClassItems(Me.Row4_Class_ComboBox)
        Call add_ClassItems(Me.Row5_Class_ComboBox)
    End Sub

    Private Sub add_ClassItems(ClassType_Control As ComboBox)
        If 0 = ClassType_Control.Items.Count Then
            ClassType_Control.Items.Add("CLASS 050")
            ClassType_Control.Items.Add("CLASS 055")
            ClassType_Control.Items.Add("CLASS 060")
            ClassType_Control.Items.Add("CLASS 065")
            ClassType_Control.Items.Add("CLASS 070")
            ClassType_Control.Items.Add("CLASS 077")
            ClassType_Control.Items.Add("CLASS 085")
            ClassType_Control.Items.Add("CLASS 092")
            ClassType_Control.Items.Add("CLASS 100")
            ClassType_Control.Items.Add("CLASS 110")
            ClassType_Control.Items.Add("CLASS 125")
            ClassType_Control.Items.Add("CLASS 150")
            ClassType_Control.Items.Add("CLASS 175")
            ClassType_Control.Items.Add("CLASS 200")
            ClassType_Control.Items.Add("CLASS 250")
            ClassType_Control.Items.Add("CLASS 300")
            ClassType_Control.Items.Add("CLASS 400")
            ClassType_Control.Items.Add("CLASS 500")
        End If
    End Sub

    Private Function fillout_FreightFormItems() As Boolean
        '
        For i As Int16 = 0 To LTL_Freight.FreightFormItems.Count - 1
            '
            Dim FreightItem As FreightFormItem = LTL_Freight.FreightFormItems(i)
            If Not String.IsNullOrEmpty(FreightItem.Description) Then
                '
                If i = 0 Then
                    Me.Row1_HandlingUnits_TextBox.Text = FreightItem.HandlingUnits
                    Me.Row1_PackagingType_ComboBox.Text = FreightItem.PackagingType
                    Me.Row1_Pieces_TextBox.Text = FreightItem.PiecesNo
                    Me.Row1_Description_TextBox.Text = FreightItem.Description
                    Me.Row1_LBs_TextBox.Text = FreightItem.Weight
                    Me.Row1_InsuredValue_TextBox.Text = FreightItem.InsuredValue
                    Me.Row1_Class_ComboBox.Text = FreightItem.PackageClass
                ElseIf i = 1 Then
                    Me.Row2_HandlingUnits_TextBox.Text = FreightItem.HandlingUnits
                    Me.Row2_PackagingType_ComboBox.Text = FreightItem.PackagingType
                    Me.Row2_Pieces_TextBox.Text = FreightItem.PiecesNo
                    Me.Row2_Description_TextBox.Text = FreightItem.Description
                    Me.Row2_LBs_TextBox.Text = FreightItem.Weight
                    Me.Row2_InsuredValue_TextBox.Text = FreightItem.InsuredValue
                    Me.Row2_Class_ComboBox.Text = FreightItem.PackageClass
                ElseIf i = 2 Then
                    Me.Row3_HandlingUnits_TextBox.Text = FreightItem.HandlingUnits
                    Me.Row3_PackagingType_ComboBox.Text = FreightItem.PackagingType
                    Me.Row3_Pieces_TextBox.Text = FreightItem.PiecesNo
                    Me.Row3_Description_TextBox.Text = FreightItem.Description
                    Me.Row3_LBs_TextBox.Text = FreightItem.Weight
                    Me.Row3_InsuredValue_TextBox.Text = FreightItem.InsuredValue
                    Me.Row3_Class_ComboBox.Text = FreightItem.PackageClass
                ElseIf i = 3 Then
                    Me.Row4_HandlingUnits_TextBox.Text = FreightItem.HandlingUnits
                    Me.Row4_PackagingType_ComboBox.Text = FreightItem.PackagingType
                    Me.Row4_Pieces_TextBox.Text = FreightItem.PiecesNo
                    Me.Row4_Description_TextBox.Text = FreightItem.Description
                    Me.Row4_LBs_TextBox.Text = FreightItem.Weight
                    Me.Row4_InsuredValue_TextBox.Text = FreightItem.InsuredValue
                    Me.Row4_Class_ComboBox.Text = FreightItem.PackageClass
                ElseIf i = 4 Then
                    Me.Row5_HandlingUnits_TextBox.Text = FreightItem.HandlingUnits
                    Me.Row5_PackagingType_ComboBox.Text = FreightItem.PackagingType
                    Me.Row5_Pieces_TextBox.Text = FreightItem.PiecesNo
                    Me.Row5_Description_TextBox.Text = FreightItem.Description
                    Me.Row5_LBs_TextBox.Text = FreightItem.Weight
                    Me.Row5_InsuredValue_TextBox.Text = FreightItem.InsuredValue
                    Me.Row5_Class_ComboBox.Text = FreightItem.PackageClass
                End If
                '
            Else
                Exit For
                '
            End If
            '
        Next
        '
        If (FedEx_Freight.LTL_Freight.FreightFormPaymentType = "RECIPIENT") Then ' Collect
            Me.PaymentType_COLLECT_RadioButton.IsChecked = True
        Else
            Me.PaymentType_PREPAID_RadioButton.IsChecked = True
        End If
        '
        fillout_FreightFormItems = (Not 0 = Len(Me.Row1_Description_TextBox.Text))
    End Function

    Private Function collect_FreightFormItems() As Boolean
        collect_FreightFormItems = False ' assume.
        Dim i As Int16 = 0
        '
        FedEx_Freight.LTL_Freight = New _baseFreight
        Dim FreightItem As FreightFormItem
        If Not String.IsNullOrEmpty(Me.Row1_Description_TextBox.Text) Then
            FreightItem = New FreightFormItem
            FreightItem.HandlingUnits = Val(Me.Row1_HandlingUnits_TextBox.Text)
            FreightItem.PackagingType = Me.Row1_PackagingType_ComboBox.Text
            FreightItem.PiecesNo = Val(Me.Row1_Pieces_TextBox.Text)
            FreightItem.Description = Me.Row1_Description_TextBox.Text
            FreightItem.Weight = Val(Me.Row1_LBs_TextBox.Text)
            FreightItem.InsuredValue = Val(Me.Row1_InsuredValue_TextBox.Text)
            FreightItem.PackageClass = Me.Row1_Class_ComboBox.Text
            FedEx_Freight.LTL_Freight.FreightFormItems.Add(FreightItem)
            collect_FreightFormItems = True
        End If
        If Not String.IsNullOrEmpty(Me.Row2_Description_TextBox.Text) Then
            FreightItem = New FreightFormItem
            FreightItem.HandlingUnits = Val(Me.Row2_HandlingUnits_TextBox.Text)
            FreightItem.PackagingType = Me.Row2_PackagingType_ComboBox.Text
            FreightItem.PiecesNo = Val(Me.Row2_Pieces_TextBox.Text)
            FreightItem.Description = Me.Row2_Description_TextBox.Text
            FreightItem.Weight = Val(Me.Row2_LBs_TextBox.Text)
            FreightItem.InsuredValue = Val(Me.Row2_InsuredValue_TextBox.Text)
            FreightItem.PackageClass = Me.Row2_Class_ComboBox.Text
            FedEx_Freight.LTL_Freight.FreightFormItems.Add(FreightItem)
            collect_FreightFormItems = True
        End If
        If Not String.IsNullOrEmpty(Me.Row3_Description_TextBox.Text) Then
            FreightItem = New FreightFormItem
            FreightItem.HandlingUnits = Val(Me.Row3_HandlingUnits_TextBox.Text)
            FreightItem.PackagingType = Me.Row3_PackagingType_ComboBox.Text
            FreightItem.PiecesNo = Val(Me.Row3_Pieces_TextBox.Text)
            FreightItem.Description = Me.Row3_Description_TextBox.Text
            FreightItem.Weight = Val(Me.Row3_LBs_TextBox.Text)
            FreightItem.InsuredValue = Val(Me.Row3_InsuredValue_TextBox.Text)
            FreightItem.PackageClass = Me.Row3_Class_ComboBox.Text
            FedEx_Freight.LTL_Freight.FreightFormItems.Add(FreightItem)
            collect_FreightFormItems = True
        End If
        If Not String.IsNullOrEmpty(Me.Row4_Description_TextBox.Text) Then
            FreightItem = New FreightFormItem
            FreightItem.HandlingUnits = Val(Me.Row4_HandlingUnits_TextBox.Text)
            FreightItem.PackagingType = Me.Row4_PackagingType_ComboBox.Text
            FreightItem.PiecesNo = Val(Me.Row4_Pieces_TextBox.Text)
            FreightItem.Description = Me.Row4_Description_TextBox.Text
            FreightItem.Weight = Val(Me.Row4_LBs_TextBox.Text)
            FreightItem.InsuredValue = Val(Me.Row4_InsuredValue_TextBox.Text)
            FreightItem.PackageClass = Me.Row4_Class_ComboBox.Text
            FedEx_Freight.LTL_Freight.FreightFormItems.Add(FreightItem)
            collect_FreightFormItems = True
        End If
        If Not String.IsNullOrEmpty(Me.Row5_Description_TextBox.Text) Then
            FreightItem = New FreightFormItem
            FreightItem.HandlingUnits = Val(Me.Row5_HandlingUnits_TextBox.Text)
            FreightItem.PackagingType = Me.Row5_PackagingType_ComboBox.Text
            FreightItem.PiecesNo = Val(Me.Row5_Pieces_TextBox.Text)
            FreightItem.Description = Me.Row5_Description_TextBox.Text
            FreightItem.Weight = Val(Me.Row5_LBs_TextBox.Text)
            FreightItem.InsuredValue = Val(Me.Row5_InsuredValue_TextBox.Text)
            FreightItem.PackageClass = Me.Row5_Class_ComboBox.Text
            FedEx_Freight.LTL_Freight.FreightFormItems.Add(FreightItem)
            collect_FreightFormItems = True
        End If
        '
        If Me.PaymentType_COLLECT_RadioButton.IsChecked Then
            FedEx_Freight.LTL_Freight.FreightFormPaymentType = "RECIPIENT"
        Else
            FedEx_Freight.LTL_Freight.FreightFormPaymentType = "SENDER"
        End If
        '
        If Not collect_FreightFormItems Then
            FedEx_Freight.LTL_Freight = Nothing
        End If
    End Function

    Private Sub empty_FreightFormItems()
        Me.Row1_HandlingUnits_TextBox.Text = String.Empty
        Me.Row1_PackagingType_ComboBox.Text = String.Empty
        Me.Row1_Pieces_TextBox.Text = String.Empty
        Me.Row1_Description_TextBox.Text = String.Empty
        Me.Row1_LBs_TextBox.Text = String.Empty
        Me.Row1_InsuredValue_TextBox.Text = String.Empty
        Me.Row1_Class_ComboBox.Text = String.Empty
        Me.Row2_HandlingUnits_TextBox.Text = String.Empty
        Me.Row2_PackagingType_ComboBox.Text = String.Empty
        Me.Row2_Pieces_TextBox.Text = String.Empty
        Me.Row2_Description_TextBox.Text = String.Empty
        Me.Row2_LBs_TextBox.Text = String.Empty
        Me.Row2_InsuredValue_TextBox.Text = String.Empty
        Me.Row2_Class_ComboBox.Text = String.Empty
        Me.Row3_HandlingUnits_TextBox.Text = String.Empty
        Me.Row3_PackagingType_ComboBox.Text = String.Empty
        Me.Row3_Pieces_TextBox.Text = String.Empty
        Me.Row3_Description_TextBox.Text = String.Empty
        Me.Row3_LBs_TextBox.Text = String.Empty
        Me.Row3_InsuredValue_TextBox.Text = String.Empty
        Me.Row3_Class_ComboBox.Text = String.Empty
        Me.Row4_HandlingUnits_TextBox.Text = String.Empty
        Me.Row4_PackagingType_ComboBox.Text = String.Empty
        Me.Row4_Pieces_TextBox.Text = String.Empty
        Me.Row4_Description_TextBox.Text = String.Empty
        Me.Row4_LBs_TextBox.Text = String.Empty
        Me.Row4_InsuredValue_TextBox.Text = String.Empty
        Me.Row4_Class_ComboBox.Text = String.Empty
        Me.Row5_HandlingUnits_TextBox.Text = String.Empty
        Me.Row5_PackagingType_ComboBox.Text = String.Empty
        Me.Row5_Pieces_TextBox.Text = String.Empty
        Me.Row5_Description_TextBox.Text = String.Empty
        Me.Row5_LBs_TextBox.Text = String.Empty
        Me.Row5_InsuredValue_TextBox.Text = String.Empty
        Me.Row5_Class_ComboBox.Text = String.Empty
    End Sub

    Private Function get_ItemsTotalWeight() As Double
        Dim totWt As Double = 0
        '
        If Not String.IsNullOrEmpty(Me.Row1_Description_TextBox.Text) Then
            totWt = Val(Me.Row1_LBs_TextBox.Text)
        End If
        If Not String.IsNullOrEmpty(Me.Row2_Description_TextBox.Text) Then
            totWt += Val(Me.Row2_LBs_TextBox.Text)
        End If
        If Not String.IsNullOrEmpty(Me.Row3_Description_TextBox.Text) Then
            totWt += Val(Me.Row3_LBs_TextBox.Text)
        End If
        If Not String.IsNullOrEmpty(Me.Row4_Description_TextBox.Text) Then
            totWt += Val(Me.Row4_LBs_TextBox.Text)
        End If
        If Not String.IsNullOrEmpty(Me.Row5_Description_TextBox.Text) Then
            totWt += Val(Me.Row5_LBs_TextBox.Text)
        End If
        '
        get_ItemsTotalWeight = totWt
    End Function

    Private Function get_ItemsTotalValue() As Double
        Dim totVal As Double
        '
        If Not String.IsNullOrEmpty(Me.Row1_Description_TextBox.Text) Then
            totVal = Val(Me.Row1_InsuredValue_TextBox.Text)
        End If
        If Not String.IsNullOrEmpty(Me.Row2_Description_TextBox.Text) Then
            totVal += Val(Me.Row2_InsuredValue_TextBox.Text)
        End If
        If Not String.IsNullOrEmpty(Me.Row3_Description_TextBox.Text) Then
            totVal += Val(Me.Row3_InsuredValue_TextBox.Text)
        End If
        If Not String.IsNullOrEmpty(Me.Row4_Description_TextBox.Text) Then
            totVal += Val(Me.Row4_InsuredValue_TextBox.Text)
        End If
        If Not String.IsNullOrEmpty(Me.Row5_Description_TextBox.Text) Then
            totVal += Val(Me.Row5_InsuredValue_TextBox.Text)
        End If
        '
        get_ItemsTotalValue = totVal
    End Function

    Private Function get_ItemsDescTotalCount() As Integer
        Dim totCnt As Integer = 0
        '
        If Not String.IsNullOrEmpty(Me.Row1_Description_TextBox.Text) Then
            totCnt += 1
        End If
        If Not String.IsNullOrEmpty(Me.Row2_Description_TextBox.Text) Then
            totCnt += 1
        End If
        If Not String.IsNullOrEmpty(Me.Row3_Description_TextBox.Text) Then
            totCnt += 1
        End If
        If Not String.IsNullOrEmpty(Me.Row4_Description_TextBox.Text) Then
            totCnt += 1
        End If
        If Not String.IsNullOrEmpty(Me.Row5_Description_TextBox.Text) Then
            totCnt += 1
        End If
        '
        get_ItemsDescTotalCount = totCnt
    End Function

    Private Function distribute_WeightEvenly() As Boolean
        Dim totWt As Double : totWt = get_ItemsTotalWeight()
        Dim cnt As Integer : cnt = get_ItemsDescTotalCount()
        Dim devWt As Double = 0
        Dim DimWieght As Double = Val(Me.Weight.Text)
        '
        If cnt > 0 Then
            '
            devWt = cnt * totWt
            If devWt > DimWieght Then
                '
                devWt = Round(DimWieght / cnt, 2)
                For i As Int16 = 0 To cnt - 1
                    If i = 0 Then
                        Me.Row1_LBs_TextBox.Text = devWt
                    ElseIf i = 1 Then
                        Me.Row2_LBs_TextBox.Text = devWt
                    ElseIf i = 2 Then
                        Me.Row3_LBs_TextBox.Text = devWt
                    ElseIf i = 3 Then
                        Me.Row4_LBs_TextBox.Text = devWt
                    ElseIf i = 4 Then
                        Me.Row5_LBs_TextBox.Text = devWt
                    End If
                Next i
                '
            End If
            ''
        End If
        '
        distribute_WeightEvenly = Not (devWt > DimWieght)
        '
    End Function

    Private Function distribute_ValueEvenly() As Boolean
        Dim totWt As Double : totWt = get_ItemsTotalValue()
        Dim cnt As Integer : cnt = get_ItemsDescTotalCount()
        Dim devWt As Double = 0
        Dim DecVal As Double = Val(Me.DeclaredValue.Text)
        '
        If cnt > 0 Then
            '
            devWt = cnt * totWt
            If devWt > DecVal Then
                '
                devWt = Round(DecVal / cnt, 2)
                For i As Int16 = 0 To cnt - 1
                    If i = 0 Then
                        Me.Row1_InsuredValue_TextBox.Text = devWt
                    ElseIf i = 1 Then
                        Me.Row2_InsuredValue_TextBox.Text = devWt
                    ElseIf i = 2 Then
                        Me.Row3_InsuredValue_TextBox.Text = devWt
                    ElseIf i = 3 Then
                        Me.Row4_InsuredValue_TextBox.Text = devWt
                    ElseIf i = 4 Then
                        Me.Row5_InsuredValue_TextBox.Text = devWt
                    End If
                Next i
                '
            End If
            ''
        End If
        '
        distribute_ValueEvenly = Not (devWt > DecVal)
        '
    End Function

    Private Sub get_Totals()
        If Me.RowTotal_DistributeWeight_CheckBox.IsChecked Then
            Call distribute_WeightEvenly()
            Call distribute_ValueEvenly()
        End If
        Me.RowTotal_LBs_TextBlock.Text = String.Format("{0} lb", get_ItemsTotalWeight)
        Me.RowTotal_InsuredValue_TextBlock.Text = String.Format("${0}", get_ItemsTotalValue)
    End Sub

    Private Sub RowTotal_DistributeWeight_CheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles RowTotal_DistributeWeight_CheckBox.Checked
        Try
            Call get_Totals()
        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to distribute values evenly...")
        End Try
    End Sub

    Private Sub Row1_Description_TextBox_LostFocus(sender As Object, e As RoutedEventArgs) Handles Row1_Description_TextBox.LostFocus, Row2_Description_TextBox.LostFocus, Row3_Description_TextBox.LostFocus, Row4_Description_TextBox.LostFocus, Row5_Description_TextBox.LostFocus
        Try
            Call get_Totals()
        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to distribute values evenly...")
        End Try
    End Sub

    Private Sub TimeInTransit_Button_Click(sender As Object, e As RoutedEventArgs) Handles TimeInTransit_Button.Click
        GetRealShippingTimes()
    End Sub

    Private Sub Edit_MasterShippingTable_Click(sender As Object, e As RoutedEventArgs)
        Try

            If gIsSetupSecurityEnabled Then
                If Not Check_Current_User_Permission("Setup_Carriers") Then
                    Exit Sub
                End If
            End If


            'Get selected button
            Dim MenuItem As MenuItem = DirectCast(sender, MenuItem)
            Dim parentContextMenu As ContextMenu = DirectCast(MenuItem.CommandParameter, ContextMenu)
            Dim current_button As Button = parentContextMenu.PlacementTarget

            If IsNothing(current_button) Then Exit Sub


            Dim shippingChoice As New ShippingChoiceDefinition
            shippingChoice = current_button.Tag


            If shippingChoice.Service IsNot Nothing Then


                Dim win As New ShippingMarkups(Me, shippingChoice)
                win.ShowDialog(Me)
            End If

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub

    Private Sub Setup_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Setup_Btn.Click
        Try

            If gIsSetupSecurityEnabled Then
                Dim wind As New UserLogIn(Me, "Setup_Carriers")
                wind.ShowDialog()

                If UserLogIn.isAllowed = False Then
                    Exit Sub
                End If
            End If


            Dim win As New ShippingSetup(Me)
            win.ShowDialog(Me)

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub

    Private Overloads Sub BackButton_Click(sender As Object, e As RoutedEventArgs)
        ShowPackageDetails_Popup.IsOpen = False



        If CommonWindowStack.WindowListPointerBack Then
            CommonWindowStack.WindowSwitch(False)
        End If
    End Sub

#End Region

#Region "ShortcutKeyHandler"
    Private Sub Window_KeyDown(sender As Object, e As KeyEventArgs)

        If e.Key = Key.F8 Then
            Load_ShipToAddresses_ForShipper()
            Exit Sub
        End If

        ShortcutKeyHandlers.KeyDown(sender, e, Me)
    End Sub



#End Region

    Private Sub FedExFlatRate_Button_Click(sender As Object, e As RoutedEventArgs) Handles FedExFlatRate_Button.Click
        If FedEx_FlatRate_TxtBx.Text = "OFF" Then
            FedEx_FlatRate_TxtBx.Text = "ON"
            _FedExWeb.IsEnabled_OneRate = True
        Else
            FedEx_FlatRate_TxtBx.Text = "OFF"
            _FedExWeb.IsEnabled_OneRate = False
        End If

        ProcessShippingRates()
    End Sub



#Region "Zip Code Lookup Popup"

    Private Sub ZipSearch_Btn_Click(sender As Object, e As RoutedEventArgs) Handles ZipSearch_Btn.Click
        ZipSearch_Popup.IsOpen = True
        Zip_CitySearch_TxtBx.Focus()
    End Sub

    Private Sub Zip_Select_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Zip_Select_Btn.Click
        LoadSelectedZip()
    End Sub

    Private Sub LoadSelectedZip()
        ZipCode.Text = Zip_City_LV.SelectedItem(2)
        ZipSearch_Popup.IsOpen = False
        DeclaredValue.Focus()
        ProcessShippingRates()
    End Sub

    Private Sub Zip_CitySearch_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Zip_CitySearch_Btn.Click
        Load_ZipSearchList()
    End Sub

    Private Sub Zip_CitySearch_TxtBx_PreviewKeyDown(sender As Object, e As KeyEventArgs) Handles Zip_CitySearch_TxtBx.PreviewKeyDown
        If e.Key = Key.Enter Then
            Load_ZipSearchList()

        ElseIf e.Key = Key.Down Then
            If Zip_City_LV.Items.Count > 0 Then
                Zip_City_LV.SelectedIndex = 0
                Zip_City_LV.Focus()
            End If

        End If
    End Sub


    Private Sub Load_ZipSearchList()
        If Zip_CitySearch_TxtBx.Text = "" Then Exit Sub

        Dim SQL As String = "SELECT City, ST, Zip From ZipCodes Where City LIKE '" & Zip_CitySearch_TxtBx.Text & "%'"

        BindingOperations.ClearAllBindings(Zip_City_LV)
        Zip_City_LV.DataContext = Nothing

        Dim DT As New System.Data.DataTable
        Dim currentGridView As GridView = Zip_City_LV.View

        DT.Columns.Add("City")
        DT.Columns.Add("State")
        DT.Columns.Add("Zip")

        IO_LoadListView(Zip_City_LV, DT, gZipCodeDB, SQL, DT.Columns.Count)
    End Sub

    Private Sub Zip_City_LV_KeyDown(sender As Object, e As KeyEventArgs) Handles Zip_City_LV.KeyDown
        If e.Key = Key.Enter Then
            LoadSelectedZip()
        End If
    End Sub

    Private Sub Zip_City_LV_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles Zip_City_LV.MouseDoubleClick
        LoadSelectedZip()
    End Sub



#End Region
End Class
