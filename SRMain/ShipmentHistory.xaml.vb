Imports System.ComponentModel

Public Class ShipmentHistory
    Inherits CommonWindow
    Public isLoading As Boolean
    Dim Last_Column_Sorted As String
    Dim Last_Sort_Ascending As Boolean
    Dim SID As String
    Dim CID As String
    Dim PID As String
    Dim HideCosts As Boolean

    Private Class ManifestItem
        Property Name As String
        Property Value As String
    End Class

    Private Class ManifestCharge
        Property Name As String
        Property Cost As Double
        Property Sell As Double
        Property HideCost As Boolean
    End Class


    Public Sub New()

        MyBase.New()

        ' This call is required by the designer.
        InitializeComponent()

    End Sub

    Public Sub New(ByVal callingWindow As Window, Optional ByVal ShipperID As String = "", Optional ByVal ConsgineeID As String = "", Optional ByVal PacakgeID As String = "")

        MyBase.New(callingWindow)

        ' This call is required by the designer.
        InitializeComponent()

        SID = ShipperID
        CID = ConsgineeID
        PID = PacakgeID
    End Sub

    Private Sub ShipmentHistory_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Try
            isLoading = True

            If gIsProgramSecurityEnabled AndAlso Not Check_Current_User_Permission("View_Shipping_Costs", True) Then
                HideCosts = True
            Else
                HideCosts = False
            End If

            Load_Carriers()
            Load_ShipmentStatusOptions()
            ShipmentStatus_CmbBox.SelectedIndex = 0

            If SID <> "" Or CID <> "" Then
                'if displaying history for specific shipper/consignee, then show packages going back 1 year.
                StartDate.SelectedDate = Today.AddDays(-365)

                If SID <> "" Then
                    ShipmentHeader_Lbl.Content = "Shipments from " & ExtractElementFromSegment("Name", IO_GetSegmentSet(gShipriteDB, "Select Name from Contacts where ID=" & SID), "")
                ElseIf CID <> "" Then
                    ShipmentHeader_Lbl.Content = "Shipments to " & ExtractElementFromSegment("Name", IO_GetSegmentSet(gShipriteDB, "Select Name from Contacts where ID=" & CID), "")
                End If


            Else
                StartDate.SelectedDate = Today.AddDays(-90)
            End If

            EndDate.SelectedDate = Today.AddDays(1)
            Detail_Border.Visibility = Visibility.Hidden
            ManifestButton.Visibility = Visibility.Hidden
            ReprintCommercialInvoice_Button.Visibility = Visibility.Hidden
            Edit_TxtBx.Visibility = Visibility.Hidden

            isLoading = False
            Values_LV.Height = 173

            DisplayShipmentListing()

            If PID <> "" And Shipment_LV.Items.Count > 0 Then
                'Show details for specific shipment.
                Shipment_LV.SelectedIndex = 0
                ShowDetailScreen()
            End If

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Load_ShipmentStatusOptions()
        Try
            Dim SegmentSet As String = ""
            Dim current_segment As String = ""
            Dim buf As String = ""
            Dim fieldName As String = ""
            Dim fieldValue As String = ""
            Dim StatusList As List(Of String) = New List(Of String)

            StatusList.Add("ALL")

            buf = IO_GetSegmentSet(gShipriteDB, "SELECT DISTINCT [Exported] from Manifest")

            Do Until buf = ""
                current_segment = GetNextSegmentFromSet(buf)
                current_segment = ExtractNextElementFromSegment(fieldName, fieldValue, current_segment)

                If fieldValue Is Nothing Then fieldValue = ""

                StatusList.Add(fieldValue)
            Loop

            ShipmentStatus_CmbBox.ItemsSource = StatusList

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Load_Carriers()
        Try

            Dim SegmentSet As String = ""
            Dim current_segment As String = ""
            Dim buf As String = ""
            Dim fieldName As String = ""
            Dim fieldValue As String = ""
            Dim CarrierList As List(Of Carrier) = New List(Of Carrier)
            Dim current_Carrier As Carrier


            buf = IO_GetSegmentSet(gShipriteDB, "SELECT DISTINCT Carrier from Manifest")

            Do Until buf = ""
                current_segment = GetNextSegmentFromSet(buf)
                current_segment = ExtractNextElementFromSegment(fieldName, fieldValue, current_segment)

                If fieldValue Is Nothing Then fieldValue = ""
                current_Carrier = New Carrier
                current_Carrier.CarrierName = fieldValue
                current_Carrier.CarrierImage = "Resources/" & fieldValue & "_Logo.png"

                CarrierList.Add(current_Carrier)
            Loop

            Carrier_ListBox.ItemsSource = CarrierList

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub DisplayShipmentListing()
        Try

            If isLoading Then
                Exit Sub
            End If

            Dim SQL As String = ""
            Generate_SQL(SQL)

            BindingOperations.ClearAllBindings(Shipment_LV) ' clear data rows
            Shipment_LV.DataContext = Nothing
            Shipment_LV.View = New GridView

            Shipment_LV.Items.Clear()

            Dim DT As System.Data.DataTable = New System.Data.DataTable

            Dim searchGrid As GridView = New GridView
            Dim searchCol As GridViewColumn
            searchGrid.AllowsColumnReorder = False


            searchCol = New GridViewColumn
            searchCol.DisplayMemberBinding = New Binding("Date")
            searchCol.DisplayMemberBinding.StringFormat = "MM/dd/yyyy"
            searchCol.Header = "Date"
            searchCol.Width = 73
            searchGrid.Columns.Add(searchCol)
            DT.Columns.Add("Date", GetType(Date))


            searchCol = New GridViewColumn
            searchCol.DisplayMemberBinding = New Binding("P1")
            searchCol.Header = "Service"
            searchCol.Width = 75
            searchGrid.Columns.Add(searchCol)
            DT.Columns.Add("P1")


            searchCol = New GridViewColumn
            searchCol.DisplayMemberBinding = New Binding("ShipToName")
            searchCol.Header = "Consignee"
            searchCol.Width = 155
            searchGrid.Columns.Add(searchCol)
            DT.Columns.Add("ShipToName")

            searchCol = New GridViewColumn
            searchCol.DisplayMemberBinding = New Binding("PackageID")
            searchCol.Header = "PackageID"
            searchCol.Width = 60
            searchGrid.Columns.Add(searchCol)
            DT.Columns.Add("PackageID")

            searchCol = New GridViewColumn
            searchCol.DisplayMemberBinding = New Binding("Tracking#")
            searchCol.Header = "Tracking#"
            searchCol.Width = 160
            searchGrid.Columns.Add(searchCol)
            DT.Columns.Add("Tracking#")

            searchCol = New GridViewColumn
            searchCol.DisplayMemberBinding = New Binding("LBS")
            searchCol.Header = "LBS"
            searchCol.Width = 30
            searchGrid.Columns.Add(searchCol)
            DT.Columns.Add("LBS", GetType(Long))

            searchCol = New GridViewColumn
            searchCol.DisplayMemberBinding = New Binding("Z1")
            searchCol.Header = "Zone"
            searchCol.Width = 55
            searchGrid.Columns.Add(searchCol)
            DT.Columns.Add("Z1")

            'Hides shipment costs if user doesn't have permission to view it.
            If HideCosts = False Then
                searchCol = New GridViewColumn
                searchCol.DisplayMemberBinding = New Binding("CostT1")
                searchCol.DisplayMemberBinding.StringFormat = "$0.00"
                searchCol.Header = "Cost"
                searchCol.Width = 53
                searchGrid.Columns.Add(searchCol)
            End If
            DT.Columns.Add("CostT1", GetType(Double))

            searchCol = New GridViewColumn
            searchCol.DisplayMemberBinding = New Binding("T1")
            searchCol.DisplayMemberBinding.StringFormat = "$0.00"
            searchCol.Header = "Sell"
            searchCol.Width = 53
            searchGrid.Columns.Add(searchCol)
            DT.Columns.Add("T1", GetType(Double))


            Shipment_LV.View = searchGrid

            IO_LoadListView(Shipment_LV, DT, gShipriteDB, SQL, 9)

            Last_Column_Sorted = "Date"
            Last_Sort_Ascending = False

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub ColumnHeader_Click(sender As Object, e As RoutedEventArgs)
        'Sorts ListView by clicked Column Header

        Sort_LV_byColumn(Shipment_LV, TryCast(e.OriginalSource, GridViewColumnHeader), Last_Sort_Ascending, Last_Column_Sorted)


    End Sub

    Private Sub Generate_SQL(ByRef SQL)
        Try

            'SQL = "SELECT [Date], P1, ShipToName, PackageID, [Tracking#], LBS, Z1, Format([CostT1], '$ 0.00'), Format([T1], '$ 0.00') FROM Manifest WHERE [Date] >= #" & StartDate.SelectedDate & "# and [Date] <= #" & EndDate.SelectedDate & "#"
            SQL = "SELECT [Date], P1, ShipToName, PackageID, [Tracking#], LBS, Z1, [CostT1], [T1] FROM Manifest WHERE "

            If PID = "" Then

                If Manifest_CmbBox.SelectedIndex <> -1 Then
                    Dim ManifestNo_String As String = Manifest_CmbBox.SelectedItem
                    SQL = SQL & "PICKUPNUMBER='" & ManifestNo_String.Substring(0, ManifestNo_String.IndexOf("-") - 1) & "'"
                    SQL = SQL & " And [Carrier]='" & ManifestNo_String.Substring(ManifestNo_String.IndexOf("-") + 2) & "'"

                Else
                    SQL = SQL & "[Date] >= #" & StartDate.SelectedDate & "# And [Date] <= #" & EndDate.SelectedDate & "#"

                    If Carrier_ListBox.SelectedIndex <> -1 Then
                        SQL = SQL & " And [Carrier]='" & Carrier_ListBox.SelectedItem.CarrierName & "'"
                    End If

                    If Service_ListBox.SelectedIndex <> -1 Then
                        SQL = SQL & " and [P1]='" & Service_ListBox.SelectedItem & "'"
                    End If

                    If ShipmentStatus_CmbBox.SelectedIndex = 0 Then
                        'ALL selected
                        SQL = SQL & " and not [Exported]='Deleted'"
                    Else
                        SQL = SQL & " and [Exported]='" & ShipmentStatus_CmbBox.SelectedItem & "'"
                    End If

                    If SID <> "" Then
                        SQL = SQL & " and [SID]=" & SID
                    End If

                    If CID <> "" Then
                        SQL = SQL & " and [CID]=" & CID
                    End If

                    If Search_TxtBox.Text <> "Tracking# / PackageID / Name" And Search_TxtBox.Text <> "" Then
                        SQL = SQL & " and ([Tracking#] Like '%" & Search_TxtBox.Text & "%' OR [PACKAGEID] Like '%" & Search_TxtBox.Text & "%' OR [ShipToName] Like '%" & Search_TxtBox.Text & "%')"
                    End If

                End If

            Else
                'Display only specific Package ID
                SQL = SQL & "PACKAGEID='" & PID & "'"

            End If

            SQL = SQL & " ORDER BY [Date] DESC, [Time] DESC"

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Display_Shipment_Details()
        Dim current_segment As String = ""
        Dim d As String


        current_segment = GetNextSegmentFromSet(IO_GetSegmentSet(gShipriteDB, "SELECT * From Manifest Where PackageID = '" & Shipment_LV.SelectedItem(3) & "'"))

        If current_segment = "" Then Exit Sub

        Service_TxtBox.Text = ExtractElementFromSegment("ServiceName", current_segment)
        PackageID_TxtBox.Text = ExtractElementFromSegment("PACKAGEID", current_segment)
        TrackingNo_TxtBox.Text = ExtractElementFromSegment("TRACKING#", current_segment)

        Country_TxtBox.Text = ExtractElementFromSegment("Country", current_segment)
        Weight_TxtBox.Text = ExtractElementFromSegment("LBS", current_segment)

        From_TxtBox.Text = DisplayAddress(ExtractElementFromSegment("SID", current_segment))
        To_TxtBox.Text = DisplayAddress(ExtractElementFromSegment("CID", current_segment))

        d = ExtractElementFromSegment("Date", current_segment)
        DateTime_TxtBox.Text = d.Substring(0, d.IndexOf(" ")) & "   "

        d = ExtractElementFromSegment("Time", current_segment)
        DateTime_TxtBox.Text = DateTime_TxtBox.Text & d.Substring(d.IndexOf(" "))


        DisplayManifestValues(current_segment)

        DisplayManifestCharges(current_segment)

    End Sub

    Private Sub DisplayManifestCharges(ByRef segment As String)
        Dim Chargelist As List(Of ManifestCharge) = New List(Of ManifestCharge)


        AddChargeToList(Chargelist, "Shipping", "costCH1", "CH1", segment)

        AddChargeToList(Chargelist, "Residential Surcharge", "costRES", "chgRES", segment)

        If ExtractElementFromSegment("Carrier", segment) = "DHL" Then
            'UPS field was re-used for DHL Demand surcharges
            AddChargeToList(Chargelist, "DHL Demand Surcharge", "costUPSEarlyAMSurcharge", "UPSEarlyAMSurcharge", segment)
        Else
            AddChargeToList(Chargelist, "UPS Early AM Surcharge", "costUPSEarlyAMSurcharge", "UPSEarlyAMSurcharge", segment)
        End If


        AddChargeToList(Chargelist, "Declared Value - $", "costINS1", "INS1", segment)

        AddChargeToList(Chargelist, "Decl. Value THIRDPARTY - $", "costTHIRDINS1", "THIRDINS1", segment)

        AddChargeToList(Chargelist, "COD", "costCOD1", "COD1", segment)

        AddChargeToList(Chargelist, "HAZMAT", "costHAZMAT1", "HAZMAT1", segment)

        AddChargeToList(Chargelist, "Signature Confirmation", "costACKSIG1", "ACKSIG1", segment)

        AddChargeToList(Chargelist, "Delivery Confirmation", "costACK1", "ACK1", segment)

        AddChargeToList(Chargelist, "Additional Handling", "costAH1", "AH1", segment)

        AddChargeToList(Chargelist, "Saturday Delivery", "costSAT", "ACTSAT", segment)

        AddChargeToList(Chargelist, "Satruday Pickup", "costSATPU", "ACTSATPU", segment)

        AddChargeToList(Chargelist, "Fuel Surcharge", "costFuel", "Fuel", segment)

        AddChargeToList(Chargelist, "TAX", "TaxCostValue", "TaxChargeValue", segment)

        AddChargeToList(Chargelist, "Delivery Area Surcharge", "costDAS1", "DAS1", segment)

        AddChargeToList(Chargelist, "Large Package Fee", "costAHPlus", "AHPlus", segment)


        'FedEx specific charges
        If ExtractElementFromSegment("Carrier", segment) = "FedEx" Then

            If IsSegment_NOT_EmptyorZero("Fx_SigType", segment) Then
                AddChargeToList(Chargelist, SignatureType2Desc(ExtractElementFromSegment("Fx_SigType", segment)), "costFedEXHDSignature", "FedEXHDSignature", segment)
            End If

            AddChargeToList(Chargelist, "Date Certain Home Delivery", "costFedEXHDCertain", "FedEXHDCertain", segment)
            AddChargeToList(Chargelist, "Evening Home Delivery", "costFedEXHDEvening", "FedEXHDEvening", segment)
            AddChargeToList(Chargelist, "Appointment Home Delivery", "costFedEXHDAppt", "FedEXHDAppt", segment)
            AddChargeToList(Chargelist, "Dry Ice", "LabPackCost", "LabPackCharge", segment)
            AddChargeToList(Chargelist, "Clearance Entree Fee", "costCALLTAG1", "costCALLTAG1", segment)
        End If

        'DHL Specific
        If ExtractElementFromSegment("Carrier", segment) = "DHL" Then
            AddChargeToList(Chargelist, "DHL Elevated Risk", "costDHLElevatedRisk", "DHLElevatedRisk", segment)
            AddChargeToList(Chargelist, "DHL Restricted Destination", "costDHLRestrictedDest", "DHLRestrictedDest", segment)
            AddChargeToList(Chargelist, "DHL Exporter Validation", "costDHLExporterValidation", "DHLExporterValidation", segment)
        End If


        'USPS Specific
        If ExtractElementFromSegment("Carrier", segment) = "USPS" Then
            AddChargeToList(Chargelist, "Certified Mail", "costCertifiedMail", "CertifiedMail", segment)
            AddChargeToList(Chargelist, "ReturnReceipt", "costReturnReceipt", "ReturnReceipt", segment)

            AddChargeToList(Chargelist, "Nonstandard Fee - Length", "LabPackCost", "LabPackCharge", segment)
            AddChargeToList(Chargelist, "Nonstandard Fee - Volume", "costADDRC1", "ADDRC1", segment)
        End If

        AddChargeToList(Chargelist, "Round Option", "", "RoundOptionSell", segment)
        AddChargeToList(Chargelist, "TOTAL SHIPPING CHARGES", "costT1", "T1", segment)



        'Packing and Materials
        If IsSegment_NOT_EmptyorZero("PACK1", segment) Then
            AddChargeToList(Chargelist, "Packing & Materials", "costPACK", "PACK1", segment)

            Dim charge As ManifestCharge = New ManifestCharge
            charge.Name = "TOTAL with PACKING"
            charge.Cost = CDbl(ExtractElementFromSegment("costPACK", segment)) + CDbl(ExtractElementFromSegment("costT1", segment))
            charge.Sell = CDbl(ExtractElementFromSegment("PACK1", segment)) + CDbl(ExtractElementFromSegment("T1", segment))
            Chargelist.Add(charge)
        End If

        Charges_LV.ItemsSource = Chargelist

    End Sub

    Public Function SignatureType2Desc(nType As Integer) As String

        Select Case nType
            Case 2 : SignatureType2Desc = "Indirect Signature"
            Case 3 : SignatureType2Desc = "Direct Signature"
            Case 4 : SignatureType2Desc = "Adult Signature"
            Case Else : SignatureType2Desc = "No Signature Required"
        End Select

    End Function

    Private Sub AddChargeToList(ByRef Chargelist As List(Of ManifestCharge), ByRef ChargeName As String, ByRef ChargeCostField As String, ByRef ChargeSellField As String, ByRef Segment As String)
        If IsSegment_NOT_EmptyorZero(ChargeSellField, Segment) Then
            Dim charge As ManifestCharge = New ManifestCharge
            charge.Name = ChargeName
            If ChargeCostField <> "" Then
                charge.Cost = ExtractElementFromSegment(ChargeCostField, Segment)
            Else
                charge.Cost = 0
            End If

            charge.Sell = ExtractElementFromSegment(ChargeSellField, Segment)
            charge.HideCost = HideCosts
            Chargelist.Add(charge)

            'if Declared Value field, then add Decl. Value AMOUNT to the description
            If ChargeSellField = "INS1" Or ChargeSellField = "THIRDINS1" Then
                charge.Name = charge.Name & ExtractElementFromSegment("DECVAL", Segment)

                'If third party Insurance, add which one to the description.
                If ExtractElementFromSegment("ShipandInsure", Segment, "0") > 0 Then
                    charge.Name = charge.Name & vbCrLf & " - Insured with ShipandInsure"
                ElseIf ExtractElementFromSegment("DSI_ShipmentID", Segment, "") <> "" Then
                    charge.Name = charge.Name & vbCrLf & " - Insured with Shipsurance"
                End If
            End If

            'Signature Confirmation and Adult Signature Confirmation use same fields to store cost and sell pricing.
            If ChargeSellField = "ACKSIG1" And ExtractElementFromSegment("AdultSignature", Segment) = True Then
                charge.Name = "Adult Signature"
            End If

        End If
    End Sub

    Private Function IsSegment_NOT_EmptyorZero(ByRef fieldName As String, ByRef Segment As String)

        If ExtractElementFromSegment(fieldName, Segment) = "" Or ExtractElementFromSegment(fieldName, Segment) = "0" Then
            Return False
        Else
            Return True
        End If

    End Function

    Private Sub DisplayManifestValues(ByRef segment As String)
        Dim Itemlist As List(Of ManifestItem) = New List(Of ManifestItem)
        Dim item As ManifestItem

        item = New ManifestItem
        item.Name = "Invoice#"
        item.Value = ExtractElementFromSegment("InvoiceNumber", segment)
        Itemlist.Add(item)

        item = New ManifestItem
        item.Name = "Packaging"
        item.Value = ExtractElementFromSegment("Packaging", segment)
        Itemlist.Add(item)

        item = New ManifestItem
        item.Name = "Length"
        item.Value = ExtractElementFromSegment("LENGTH", segment)
        Itemlist.Add(item)

        item = New ManifestItem
        item.Name = "Width"
        item.Value = ExtractElementFromSegment("WIDTH", segment)
        Itemlist.Add(item)

        item = New ManifestItem
        item.Name = "Height"
        item.Value = ExtractElementFromSegment("HEIGHT", segment)
        Itemlist.Add(item)

        item = New ManifestItem
        item.Name = "WEIGHT"
        item.Value = ExtractElementFromSegment("LBS", segment)
        Itemlist.Add(item)

        item = New ManifestItem
        item.Name = "WEIGHT - Scale Reading"
        item.Value = ExtractElementFromSegment("ScaleReading", segment)
        Itemlist.Add(item)

        item = New ManifestItem
        item.Name = "WEIGHT - Dimensional Weight"
        item.Value = ExtractElementFromSegment("DIMWEIGHT", segment)
        Itemlist.Add(item)

        'item = New ManifestItem
        'item.Name = "WEIGHT - Dim. Weight Using"
        'item.Value = ExtractElementFromSegment("USEDIMWEIGHT", segment)
        'Itemlist.Add(item)

        item = New ManifestItem
        item.Name = "WEIGHT - Billable"
        item.Value = ExtractElementFromSegment("BillableWeight", segment)
        Itemlist.Add(item)

        item = New ManifestItem
        item.Name = "SHIPMENT - STATUS"
        item.Value = ExtractElementFromSegment("Exported", segment)
        Itemlist.Add(item)

        item = New ManifestItem
        item.Name = "SHIPMENT - Manfiest ID"
        item.Value = ExtractElementFromSegment("PICKUPNUMBER", segment)
        Itemlist.Add(item)

        item = New ManifestItem
        item.Name = "Invoice Number"
        item.Value = ExtractElementFromSegment("InvoiceNumber", segment)
        Itemlist.Add(item)

        item = New ManifestItem
        item.Name = "Sales Clerk"
        item.Value = ExtractElementFromSegment("SalesClerk", segment)
        Itemlist.Add(item)

        item = New ManifestItem
        item.Name = "DrawerID"
        item.Value = ExtractElementFromSegment("DrawerID", segment)
        Itemlist.Add(item)

        item = New ManifestItem
        item.Name = "ERROR"
        item.Value = ExtractElementFromSegment("ERROR", segment)
        Itemlist.Add(item)

        Values_LV.ItemsSource = Itemlist


    End Sub

    Private Function DisplayAddress(ByVal ContactID As String) As String
        Dim Contact_Segment As String
        Dim address As String = ""

        If ContactID = "" Then Return ""

        Contact_Segment = GetNextSegmentFromSet(IO_GetSegmentSet(gShipriteDB, "SELECT * From [Contacts] Where [ID]=" & ContactID))

        address = ExtractElementFromSegment("Name", Contact_Segment) & vbCrLf

        If ExtractElementFromSegment("FName", Contact_Segment) <> "" Or ExtractElementFromSegment("LName", Contact_Segment) <> "" Then
            If ExtractElementFromSegment("Name", Contact_Segment) <> (ExtractElementFromSegment("LName", Contact_Segment) & ", " & ExtractElementFromSegment("FName", Contact_Segment)) Then
                address = address & ExtractElementFromSegment("LName", Contact_Segment) & ", " & ExtractElementFromSegment("FName", Contact_Segment) & vbCrLf
            End If
        End If

        address = address & ExtractElementFromSegment("Addr1", Contact_Segment) & vbCrLf

        If ExtractElementFromSegment("Addr2", Contact_Segment) <> "" Then
            address = address & ExtractElementFromSegment("Addr2", Contact_Segment) & vbCrLf
        End If

        address = address & ExtractElementFromSegment("City", Contact_Segment) & " ," & ExtractElementFromSegment("State", Contact_Segment) & " " & ExtractElementFromSegment("Zip", Contact_Segment) & vbCrLf

        address = address & ExtractElementFromSegment("Country", Contact_Segment) & vbCrLf
        address = address & ExtractElementFromSegment("Phone", Contact_Segment)

        Return address

    End Function

    Private Sub Carrier_ListBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles Carrier_ListBox.SelectionChanged
        Try

            DisplayServiceOptions()
            DisplayShipmentListing()
            Manifest_CmbBox.SelectedIndex = -1

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub DisplayServiceOptions()
        Try
            Dim SegmentSet As String = ""
            Dim current_segment As String = ""
            Dim buf As String = ""
            Dim fieldName As String = ""
            Dim fieldValue As String = ""

            If Carrier_ListBox.SelectedIndex <> -1 Then
                'Display List of Services for selected Carrier
                Dim ServiceList As List(Of String) = New List(Of String)

                buf = IO_GetSegmentSet(gShipriteDB, "SELECT DISTINCT [P1] from Manifest WHERE [Carrier]='" & Carrier_ListBox.SelectedItem.CarrierName & "' and [Date] >= #" & StartDate.SelectedDate & "# and [Date] <= #" & EndDate.SelectedDate & "#")

                Do Until buf = ""
                    current_segment = GetNextSegmentFromSet(buf)
                    current_segment = ExtractNextElementFromSegment(fieldName, fieldValue, current_segment)

                    If fieldValue Is Nothing Then fieldValue = ""

                    ServiceList.Add(fieldValue)
                Loop

                Service_ListBox.ItemsSource = ServiceList
                Service_ListBox.Items.Refresh()

                All_Carriers_Btn.Background = New System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White)
                All_Services_Btn.Background = New System.Windows.Media.SolidColorBrush(FindResource(SystemColors.GradientActiveCaptionColorKey))

            End If

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub All_Carriers_Btn_Click(sender As Object, e As RoutedEventArgs) Handles All_Carriers_Btn.Click
        Try
            Carrier_ListBox.UnselectAll()
            All_Carriers_Btn.Background = New System.Windows.Media.SolidColorBrush(FindResource(SystemColors.GradientActiveCaptionColorKey))
            All_Services_Btn.Background = New System.Windows.Media.SolidColorBrush(FindResource(SystemColors.GradientActiveCaptionColorKey))
            Service_ListBox.ItemsSource = Nothing

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Manifest_CmbBox_DropDownOpened(sender As Object, e As EventArgs) Handles Manifest_CmbBox.DropDownOpened
        Try

            Dim SegmentSet As String = ""
            Dim current_segment As String = ""
            Dim buf As String = ""
            Dim fieldName As String = ""
            Dim fieldValue As String = ""
            Dim Manifest_No_List As List(Of String) = New List(Of String)
            Dim SQL As String


            If Not IsNothing(StartDate.SelectedDate) And Not IsNothing(EndDate.SelectedDate) Then
                SQL = "SELECT DISTINCT PICKUPNUMBER, Carrier from Manifest WHERE [PICKUPNUMBER]<>'' and [Date] >= #" & StartDate.SelectedDate & "# and [Date] <= #" & EndDate.SelectedDate & "#"

                If Carrier_ListBox.SelectedIndex <> -1 Then
                    SQL = SQL & " and Carrier='" & Carrier_ListBox.SelectedItem.CarrierName & "'"
                End If

                buf = IO_GetSegmentSet(gShipriteDB, SQL)

                Do Until buf = ""
                    current_segment = GetNextSegmentFromSet(buf)
                    Manifest_No_List.Add(ExtractElementFromSegment("PICKUPNUMBER", current_segment) & " - " & ExtractElementFromSegment("Carrier", current_segment))

                Loop
                Manifest_CmbBox.ItemsSource = Manifest_No_List
            Else
                MsgBox("Please Select a Start and End Date First!", vbOKOnly & vbExclamation, "Cannot Display Manifest List")


            End If

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub StartDate_LostFocus(sender As Object, e As RoutedEventArgs) Handles StartDate.LostFocus
        Try
            If IsNothing(StartDate.SelectedDate) Then StartDate.SelectedDate = Today.AddDays(-30)
            DisplayServiceOptions()
            DisplayShipmentListing()

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub EndDate_LostFocus(sender As Object, e As RoutedEventArgs) Handles EndDate.LostFocus
        Try
            If IsNothing(EndDate.SelectedDate) Then EndDate.SelectedDate = Today.AddDays(1)
            DisplayServiceOptions()
            DisplayShipmentListing()

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Service_ListBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles Service_ListBox.SelectionChanged
        Try
            If Service_ListBox.SelectedIndex <> -1 Then
                All_Services_Btn.Background = New System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White)
            End If

            DisplayShipmentListing()

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub ShipmentStatus_CmbBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles ShipmentStatus_CmbBox.SelectionChanged
        DisplayShipmentListing()
    End Sub

    Private Sub Search_TxtBox_GotFocus(sender As Object, e As RoutedEventArgs) Handles Search_TxtBox.GotFocus
        Search_TxtBox.Text = ""
    End Sub

    Private Sub Search_TxtBox_LostFocus(sender As Object, e As RoutedEventArgs) Handles Search_TxtBox.LostFocus
        If Search_TxtBox.Text = "" Then Search_TxtBox.Text = "Tracking# / PackageID / Name"
        DisplayShipmentListing()
    End Sub

    Private Sub Search_TxtBox_KeyDown(sender As Object, e As KeyEventArgs) Handles Search_TxtBox.KeyDown
        Try

            If e.Key = Key.Enter Then
                Shipment_LV.Focus()
            End If

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub All_Services_Btn_Click(sender As Object, e As RoutedEventArgs) Handles All_Services_Btn.Click
        Try
            All_Services_Btn.Background = New System.Windows.Media.SolidColorBrush(FindResource(SystemColors.GradientActiveCaptionColorKey))
            Service_ListBox.UnselectAll()

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub PrintButton_Click(sender As Object, e As RoutedEventArgs) Handles PrintButton.Click
        Try


            Dim win As New ReportsManager(Me, 3)
            win.ShowDialog(Me)

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub TrackPackage_Btn_Click(sender As Object, e As RoutedEventArgs) Handles TrackPackage_Btn.Click
        ' Check if a row is selected first
        If Shipment_LV.SelectedIndex >= 0 Then
            Try
                Dim current_row As System.Data.DataRowView = Shipment_LV.SelectedItem

                Dim current_segment As String
                Dim Carrier As String


                current_segment = GetNextSegmentFromSet(IO_GetSegmentSet(gShipriteDB, "SELECT Carrier from Manifest WHERE [PackageID]='" & current_row.Item("PackageID") & "'"))
                Carrier = SegmentFunctions.ExtractElementFromSegment("Carrier", current_segment)


                TRACK_Package(Carrier, current_row.Item("Tracking#"))

            Catch ex As Exception
                MessageBox.Show(Err.Description)
            End Try
        End If
    End Sub

    Public Shared Sub TRACK_Package(Carrier As String, TNum As String)
        Try

            Select Case Carrier
                Case "FedEx"
                    Process.Start("https://www.fedex.com/fedextrack/?trknbr=" & TNum)
                Case "UPS"
                    Process.Start("wwwapps.ups.com/etracking/tracking.cgi?tracknum=" & TNum)
                Case "DHL"
                    Process.Start("https://www.dhl.com/us-en/home/tracking.html?tracking-id=" & TNum)
                Case "USPS"
                    Process.Start("https://tools.usps.com/go/TrackConfirmAction_input?strOrigTrackNum=" & TNum)
            End Select

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Shipment_LV_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles Shipment_LV.MouseDoubleClick
        ShowDetailScreen()
    End Sub

    Private Sub ShowDetailScreen()

        If Detail_Border.Visibility = Visibility.Visible Then

            'Hide Package Details
            Detail_Border.Visibility = Visibility.Hidden
            Shipment_Border.Height = 431
            Shipment_LV.IsEnabled = True

            SearchBorder.IsEnabled = True
            DateBorder.IsEnabled = True
            CarrierBorder.IsEnabled = True
            ServiceBorder.IsEnabled = True
            StatusBorder.IsEnabled = True

        Else
            'Show Package Details
            If Shipment_LV.SelectedIndex = -1 Then
                Exit Sub
            End If

            Shipment_Border.Height = 65
            Shipment_LV.ScrollIntoView(Shipment_LV.SelectedItem)
            Detail_Border.Visibility = Visibility.Visible

            Shipment_LV.IsEnabled = False
            Display_Shipment_Details()

            SearchBorder.IsEnabled = False
            DateBorder.IsEnabled = False
            CarrierBorder.IsEnabled = False
            ServiceBorder.IsEnabled = False
            StatusBorder.IsEnabled = False

            DetailShipmentStatus_CmbBx.SelectedIndex = -1
            Values_LV.Height = 173

        End If


    End Sub

    Private Sub Values_LV_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles Values_LV.SelectionChanged
        If Values_LV.SelectedIndex = -1 Then Exit Sub

        Dim ItemList As List(Of ManifestItem) = Values_LV.ItemsSource
        Dim Status As String = ItemList.Find(Function(x) x.Name = "SHIPMENT - STATUS").Value

        If Values_LV.SelectedItem.Name = "SHIPMENT - STATUS" Then
            Values_LV.Height = 145
            DetailShipmentStatus_CmbBx.SelectedValue = Values_LV.SelectedItem.Value
            Edit_TxtBx.Visibility = Visibility.Hidden
            DetailShipmentStatus_CmbBx.Visibility = Visibility.Visible

        ElseIf (Status = "Pending") AndAlso (Values_LV.SelectedItem.Name = "Length" Or Values_LV.SelectedItem.Name = "Width" Or Values_LV.SelectedItem.Name = "Height" Or Values_LV.SelectedItem.Name = "WEIGHT") Then
            Values_LV.Height = 145
            Edit_TxtBx.Visibility = Visibility.Visible
            Edit_TxtBx.Text = Values_LV.SelectedItem.Value
            DetailShipmentStatus_CmbBx.Visibility = Visibility.Hidden
        Else
            Values_LV.Height = 173
        End If
    End Sub

    Private Sub Update_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Update_Btn.Click


        Select Case Values_LV.SelectedItem.Name
            Case "SHIPMENT - STATUS"
                If DetailShipmentStatus_CmbBx.SelectedIndex = -1 Then Exit Sub

                If vbYes = MsgBox("Are you sure you want to change the status of shipment " & Service_TxtBox.Text & " - " & TrackingNo_TxtBox.Text & " to " & DetailShipmentStatus_CmbBx.Text & "?", vbYesNo + vbQuestion, "Updating Shipment Status") Then
                    Dim SQL As String = "Update Manifest set [Exported]='" & DetailShipmentStatus_CmbBx.Text & "' WHERE [PackageID]='" & PackageID_TxtBox.Text & "'"
                    If 1 = IO_UpdateSQLProcessor(gShipriteDB, SQL) Then
                        MsgBox("Shipment status updated successfully!", vbInformation)
                        Values_LV.SelectedItem.Value = DetailShipmentStatus_CmbBx.Text
                        Values_LV.Items.Refresh()
                    End If
                End If

            Case "Length"
                UpdateField("Length", "LENGTH")
            Case "Width"
                UpdateField("Width", "WIDTH")
            Case "Height"
                UpdateField("Height", "HEIGHT")
            Case "WEIGHT"
                UpdateField("LBS", "WEIGHT")

        End Select

    End Sub

    Private Sub UpdateField(FieldName As String, Header As String)

        If Not IsNumeric(Edit_TxtBx.Text) Then Exit Sub

        If vbYes = MsgBox("Are you sure you want to change the " & Header & " value?", vbYesNo + vbQuestion, "Updating Shipment Manifest") Then
            Dim SQL As String = "Update Manifest set [" & FieldName & "]=" & Edit_TxtBx.Text & " WHERE [PackageID]='" & PackageID_TxtBox.Text & "'"
            If 1 = IO_UpdateSQLProcessor(gShipriteDB, SQL) Then
                MsgBox("Shipment " & Header & " updated successfully!", vbInformation)
                Values_LV.SelectedItem.Value = Edit_TxtBx.Text
                Values_LV.Items.Refresh()

                If Header = "Weight" Then
                    Weight_TxtBox.Text = Edit_TxtBx.Text
                End If
            End If

        End If

    End Sub

    Private Sub ShowPackageDetails_Button_Click(sender As Object, e As RoutedEventArgs) Handles ShowPackageDetails_Button.Click
        ' Check if a row is selected first
        If Shipment_LV.SelectedIndex >= 0 Then
            ShowDetailScreen()
        End If
    End Sub

    Private Sub CloseDetailScreen_Btn_Click(sender As Object, e As RoutedEventArgs) Handles CloseDetailScreen_Btn.Click
        ShowDetailScreen()
    End Sub

    Private Sub DeleteSelectedShipment()
        ' Check if a row is selected first
        If Shipment_LV.SelectedIndex >= 0 Then
            Dim current_row As System.Data.DataRowView = Shipment_LV.SelectedItem
            If Void_Shipment(current_row.Item("PackageID")) Then
                current_row.Delete()
                If Detail_Border.Visibility = Visibility.Visible Then
                    ' Details screen is open... close it
                    ShowDetailScreen()
                End If
            End If
        End If
        ' Else: no item is selected
    End Sub

    Private Sub DeleteShipment_Button_Click(sender As Object, e As RoutedEventArgs) Handles DeleteShipment_Button.Click
        DeleteSelectedShipment()
    End Sub

    Public Shared Function Void_Shipment(ByVal PackageID As String, Optional SkipConfirmation As Boolean = False) As Boolean
        Try

            If (gIsProgramSecurityEnabled Or gIsPOSSecurityEnabled) AndAlso Not Check_Current_User_Permission("Void_Shipment", True) Then
                MsgBox("User " & gCurrentUser & " does not have the permission to void shipments!", vbInformation)
                Return False

            End If

            If vbNo = MsgBox("Are you sure you want to Void the selected shipment?" & vbCrLf & vbCrLf & "A void request will be sent to the carrier!", vbYesNo + vbQuestion, "Void Shipment") And Not SkipConfirmation Then
                Return False
            End If

            Dim ok2delete As Boolean = False
            Dim SegmentSet As String = IO_GetSegmentSet(gShipriteDB, "SELECT [Tracking#], ShipAndInsure from Manifest WHERE [PackageID]='" & PackageID & "'")
            Dim TrackingNum As String = ExtractElementFromSegment("Tracking#", SegmentSet)

                '
                Dim current_segment As String = GetNextSegmentFromSet(IO_GetSegmentSet(gShipriteDB, "SELECT Carrier, Exported from Manifest WHERE [PackageID]='" & PackageID & "'"))
            Dim Carrier As String = SegmentFunctions.ExtractElementFromSegment("Carrier", current_segment)

            If "USPS" = Carrier And _EndiciaWeb.EndiciaWeb_IsEnabled And Not "" = TrackingNum Then
                '
                Dim ok2refund As Boolean
                Dim exportStatus As String = SegmentFunctions.ExtractElementFromSegment("Exported", current_segment)
                ok2refund = (EOD.PickupWaitingStatus = exportStatus Or EOD.ExportedStatus = exportStatus)
                If EOD.PickupScheduledStatus = exportStatus Then
                    ok2refund = (vbYes = MsgBox("This package is Scheduled for Pickup!" & vbCr & vbCr & "Are you sure you want to Delete it?", vbYesNo + vbQuestion))
                End If
                '
                If ok2refund Then
                    ok2refund = DSI.Void_PackageCoverage(PackageID)
                End If

                If ok2refund Then
                    '
                    Dim Pack As baseWebResponse_Package
                    Dim packs As New baseWebResponse_Shipment
                    Pack = New baseWebResponse_Package
                    ' USPS FirstClass flat letter can be refunded if the length of its tracking# will be 25 chars long, truncating the last 4 chars.
                    Pack.TrackingNo = _Controls.Left(TrackingNum, 25)
                    packs.Packages.Add(Pack)
                    If _EndiciaWeb.Request_PackageRefund(packs) Then
                        _Debug.Print_(Pack.LabelImage, Pack.LabelCustomsImage)
                        ' Is Refund Approved:
                        ' If a shipment was already refunded through Endicia web-site then you still can delete it from Shipment History.
                        If "YES" = Pack.LabelImage Or _Controls.Contains(Pack.LabelCustomsImage, "REFUNDED_ALREADY") Then
                            ok2delete = True
                        End If
                    End If
                    Pack = Nothing
                    packs = Nothing
                    '
                Else
                    ok2delete = False
                End If


            ElseIf "FedEx" = Carrier Then
                If _FedExWeb.objFedEx_Regular_Setup Is Nothing Then
                    _FedExWeb.objFedEx_Regular_Setup = New FedEx_Setup(False)
                End If

                _FedExWeb.objFedEx_Setup = _FedExWeb.objFedEx_Regular_Setup



                If GetPolicyData(gShipriteDB, "FedExREST_Enabled", "False") Then

                    If FXR_VoidShipment(TrackingNum) Then
                        ok2delete = True
                    Else
                        ok2delete = False
                    End If

                Else
                    Dim objResponse As baseWebResponse_Shipment = New baseWebResponse_Shipment

                    If _FedExWeb.Delete_Package(_FedExWeb.objFedEx_Regular_Setup, PackageID, TrackingNum, objResponse) Then

                        If objResponse.ShipmentAlerts.Count = 0 Then
                            ok2delete = True
                        Else
                            ok2delete = False
                            _MsgBox.ErrorMessage(vbCrLf & "FedEx Error: " & objResponse.ShipmentAlerts(0))
                        End If
                    End If

                End If




            ElseIf "UPS" = Carrier Then
                If _UPSWeb.Process_VoidAPackage(_UPSWeb.objUPS_Setup, PackageID, TrackingNum) Then
                    ok2delete = True
                Else
                    ok2delete = False
                End If


                '
            Else
                '
                If DSI.Void_PackageCoverage(PackageID) Then
                    ok2delete = True
                Else
                    ok2delete = _MsgBox.QuestionMessage("Delete Anyway?")
                End If
                '
            End If

            If ok2delete = False Then
                If vbYes = MsgBox("Shipment could not be voided, would you like to delete it anyway?", vbQuestion + vbYesNo) Then
                    ok2delete = True
                Else
                    Return False
                End If
            End If

            '
            If ok2delete Then

                If Not Val(ExtractElementFromSegment("ShipAndInsure", SegmentSet)) = 0 Then

                    Call Go_Online_ShipAndInsure(PackageID, True)
                    '
                End If
                '
                Dim sql2cmd As New sqlUpdate
                Dim sql2exe As String = sql2cmd.Qry_UPDATE("Exported", EOD.DeletedStatus, sql2cmd.TXT_, True, True, "Manifest", "PACKAGEID = '" & PackageID & "'")
                If Not -1 = IO_UpdateSQLProcessor(gShipriteDB, sql2exe) Then
                    _MsgBox.DeletedSuccessfully("Shipment - " & TrackingNum)
                    Return True
                Else
                    _MsgBox.ErrorMessage("Failed to delete package...")
                    Return False
                End If
                '
            Else
                _MsgBox.ErrorMessage("Failed to delete package...")
                Return False
            End If

        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to delete package...")
            Return False
        End Try

    End Function

    Private Sub ReprintLabel_Button_Click(sender As Object, e As RoutedEventArgs) Handles ReprintLabel_Button.Click
        If Shipment_LV.SelectedIndex = -1 Then Exit Sub

        Dim current_row As System.Data.DataRowView = Shipment_LV.SelectedItem
        Reprint_ShippingLabel(current_row.Item("PackageID"))


    End Sub

    Public Shared Sub Reprint_ShippingLabel(ByVal PackageID As String)

        Dim Carrier As String = ExtractElementFromSegment("Carrier", IO_GetSegmentSet(gShipriteDB, "SELECT [Carrier] from Manifest WHERE [PackageID]='" & PackageID & "'"))

        Select Case Carrier
            Case "FedEx"
                _FedExWeb.print_LabelFromFile(_FedExWeb.objFedEx_Regular_Setup.LabelImageType, PackageID, _FedExWeb.objFedEx_Regular_Setup.Path_SaveDocXML)

            Case "UPS"
                _UPSWeb.Print_Label(PackageID)
            Case "USPS"
                _EndiciaWeb.Print_Label(PackageID)

            Case "DHL"
                _Dhl_XML.Print_Label(PackageID)

        End Select


    End Sub

    Private Sub Manifest_CmbBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles Manifest_CmbBox.SelectionChanged
        If Manifest_CmbBox.SelectedIndex = -1 Then Exit Sub
        ManifestButton.Visibility = Visibility.Visible
        DisplayShipmentListing()
    End Sub

    Private Sub Shipment_LV_KeyUp(sender As Object, e As KeyEventArgs) Handles Shipment_LV.KeyUp
        ' Handle pressing "DEL"
        If e.Key = Key.Delete Or e.Key = Key.Back Then
            If Shipment_LV.SelectedIndex >= 0 Then
                DeleteSelectedShipment()
            End If
        End If
    End Sub

    Private Sub Shipment_LV_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles Shipment_LV.SelectionChanged
        If Shipment_LV.SelectedIndex = -1 Then Exit Sub

        If isServiceInternational(Shipment_LV.SelectedItem(1)) Then
            ReprintCommercialInvoice_Button.Visibility = Visibility.Visible
        Else
            ReprintCommercialInvoice_Button.Visibility = Visibility.Hidden
        End If
    End Sub

    Private Sub ReprintCommercialInvoice_Button_Click(sender As Object, e As RoutedEventArgs) Handles ReprintCommercialInvoice_Button.Click
        Cursor = Cursors.Wait
        ReportsManager.PrintCommercialInvoice(Shipment_LV.SelectedItem(3))
        Cursor = Cursors.Arrow
    End Sub

    Private Sub ManifestButton_Click(sender As Object, e As RoutedEventArgs) Handles ManifestButton.Click
        If Manifest_CmbBox.SelectedIndex = -1 Then
            MsgBox("Please select a Manifest# first!", vbExclamation, "No Manifest Selected.")
            Exit Sub
        End If

        Dim pickupNumber As String = Manifest_CmbBox.SelectedItem
        pickupNumber = pickupNumber.Substring(0, pickupNumber.IndexOf("-") - 1)

        EOD_Manifest.Print_Manifest(Carrier_ListBox.SelectedItem.CarrierName, pickupNumber)

    End Sub


End Class
