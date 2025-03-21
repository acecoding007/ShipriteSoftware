Imports SHIPRITE.ShipRiteReports

Public Class EOD_Manifest
    Inherits CommonWindow
    Dim Last_Column_Sorted As String
    Dim Last_Sort_Ascending As Boolean

    Public Sub New()

        MyBase.New()

        ' This call is required by the designer.
        InitializeComponent()

    End Sub

    Public Sub New(ByVal callingWindow As Window)

        MyBase.New(callingWindow)

        ' This call is required by the designer.
        InitializeComponent()

    End Sub

    Private Sub EOD_Manifest_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded

        If gIsProgramSecurityEnabled AndAlso Not Check_Current_User_Permission("EOD_Manifest") Then
            Me.Close()
        End If

        Last_Column_Sorted = "PACKAGEID"
        Last_Sort_Ascending = False
        Load_Carriers()
        ShippingDate_Border.Visibility = Visibility.Hidden
        AirGround_Border.Visibility = Visibility.Hidden
        Upload_Button.Visibility = Visibility.Hidden
        FedExGround_Close_Btn.Visibility = Visibility.Hidden

        ShippingDate_DP.DisplayDateStart = Date.Today

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


            buf = IO_GetSegmentSet(gShipriteDB, "SELECT DISTINCT Carrier from Master")

            Do Until buf = ""
                current_segment = GetNextSegmentFromSet(buf)
                current_segment = ExtractNextElementFromSegment(fieldName, fieldValue, current_segment)

                If fieldValue Is Nothing Then fieldValue = ""
                current_Carrier = New Carrier
                current_Carrier.CarrierName = fieldValue
                current_Carrier.CarrierImage = "Resources/" & fieldValue & "_Logo.png"

                CarrierList.Add(current_Carrier)
            Loop


            '-------- ADD CHECK IF SHIPSURANCE IS ENABLED---------------
            If gThirdPartyInsurance And DSI.gDSIis3rdPartyInsurance Then

                current_Carrier = New Carrier
                current_Carrier.CarrierName = "Shipsurance"
                current_Carrier.CarrierImage = "Resources/Shipsurance_Logo.png"

                CarrierList.Add(current_Carrier)

            End If
            '----------------------------------------------


            Carrier_ListBox.ItemsSource = CarrierList

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Load_EOD_LV(ByVal carrierName As String)
        Try
            If IsNothing(carrierName) Then Exit Sub
            Dim SQL As String = Build_EOD_SQL(carrierName)

            BindingOperations.ClearAllBindings(EOD_LV) ' clear binding on ListView
            EOD_LV.DataContext = Nothing ' remove any rows already in ListView

            Dim DT As New System.Data.DataTable ' datatable to use to populate ListView

            Dim currentGridView As GridView = EOD_LV.View ' variable to reference current GridView in Users_ListView to set up columns.

            ' add same column names to datatable columns

            DT.Columns.Add("Status")
            DT.Columns.Add("Date", GetType(Date))
            DT.Columns.Add("Time", GetType(Date))
            DT.Columns.Add("Service")
            DT.Columns.Add("ShipTo")
            DT.Columns.Add("TrackingNo")
            DT.Columns.Add("PackageID")
            DT.Columns.Add("Weight", GetType(Double))
            DT.Columns.Add("Cost", GetType(Double))
            DT.Columns.Add("Z1")
            DT.Columns.Add("DecVal")

            ' return the # of rows added to ListView
            IO_LoadListView(EOD_LV, DT, gShipriteDB, SQL, currentGridView.Columns.Count)

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Function Build_EOD_SQL(CarrierName As String) As String
        Dim SQL As String = "SELECT Exported as Status, [Date], [Time], [P1] as Service, [ShipToName] as ShipTo, [TRACKING#] as TrackingNo, [PackageID], [LBS] as Weight, [costT1] as Cost, [Z1], [DecVal] "
        Dim SQL_end As String = " AND NOT ISNULL([Exported]) AND [Exported] <> 'Exported' AND [Exported] <> 'CWT' AND [Exported] <> 'Deleted' ORDER BY [PACKAGEID]"


        If CarrierName = "Shipsurance" Then
            SQL = SQL &
          "FROM Manifest WHERE [DSI_Exported] = 'Pending' AND [DECVAL] > 0 And " &
          "([Exported] = '" & EOD.PickupWaitingStatus & "' OR [Exported] = 'Exported') " &
          "ORDER BY [PACKAGEID]"

        ElseIf CarrierName = "FedEx" Then
            Dim GroundSQL As String = " AND [P1] = 'FEDEX-GND' "
            Dim AirSQL As String = " AND [P1] <> 'FEDEX-GND' "

            If AirGround_LB.SelectedIndex = 2 Then
                'show both air and ground
                SQL = SQL & "FROM Manifest WHERE [Carrier] = '" & CarrierName & "' " & SQL_end

            ElseIf AirGround_LB.SelectedIndex = 1 Then
                'Ground Only
                SQL = SQL & "FROM Manifest WHERE [Carrier] = '" & CarrierName & "' " & GroundSQL & SQL_end

            Else
                'Air Only
                SQL = SQL & "FROM Manifest WHERE [Carrier] = '" & CarrierName & "' " & AirSQL & SQL_end
            End If

        Else

            SQL = SQL & "FROM Manifest WHERE [Carrier] = '" & CarrierName & "' " & SQL_end
        End If

        Return SQL
    End Function

    Private Sub Carrier_ListBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles Carrier_ListBox.SelectionChanged

        Refresh_View()
    End Sub

    Private Sub Refresh_View()
        Try
            Dim current_Carrier As Carrier = Carrier_ListBox.SelectedItem

            Call Load_EOD_LV(current_Carrier.CarrierName)

            If current_Carrier.CarrierName = "FedEx" Then
                AirGround_Border.Visibility = Visibility.Visible
            Else
                AirGround_Border.Visibility = Visibility.Hidden
            End If

            Call Load_Shipping_Date()

            If current_Carrier.CarrierName = DSI.DSI_NewName Then
                Upload_Button.Visibility = Visibility.Visible
            Else
                'search for pending packages to upload
                Dim DT As New System.Data.DataView
                DT = EOD_LV.ItemsSource
                DT.Sort = "Status"

                If DT.Find("Pending") = -1 Then
                    'No Pending packages found
                    Upload_Button.Visibility = Visibility.Hidden
                Else
                    Upload_Button.Visibility = Visibility.Visible
                End If
            End If

            CheckShippingDate()

        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to load packages...")
        End Try
    End Sub

    Private Sub CheckShippingDate()
        Dim segment As String
        Dim PickupDate As Date

        If Carrier_ListBox.SelectedItem.CarrierName = "FedEx" And AirGround_LB.SelectedIndex = 1 Then
            'FedEx Ground
            segment = IO_GetSegmentSet(gShipriteDB, "SELECT First(NextPickupDate) as PickupDate From Master WHERE [Carrier]='" & Carrier_ListBox.SelectedItem.CarrierName & "' and [SERVICE]='FEDEX-GND'")

        ElseIf Carrier_ListBox.SelectedItem.CarrierName = "FedEx" And AirGround_LB.SelectedIndex = 0 Then
            'FedEx Air
            segment = IO_GetSegmentSet(gShipriteDB, "SELECT First(NextPickupDate) as PickupDate  From Master WHERE [Carrier]='" & Carrier_ListBox.SelectedItem.CarrierName & "' and [SERVICE]<>'FEDEX-GND'")

        Else
            segment = IO_GetSegmentSet(gShipriteDB, "SELECT First(NextPickupDate) as PickupDate  From Master WHERE [Carrier]='" & Carrier_ListBox.SelectedItem.CarrierName & "'")
        End If



        PickupDate = ExtractElementFromSegment("PickupDate", segment, "1/1/0001")

        If PickupDate < Today.Date Then
            PickupDate = Today.Date
        End If

        ShippingDate_DP.SelectedDate = PickupDate

    End Sub


    Private Sub Upload_Button_Click(sender As Object, e As RoutedEventArgs) Handles Upload_Button.Click
        Try
            If Carrier_ListBox.SelectedIndex = -1 Then Exit Sub
            Dim current_Carrier As Carrier = Carrier_ListBox.SelectedItem
            Dim buf As String
            Dim Segment As String
            Dim PackageID As String

            If current_Carrier.CarrierName = DSI.DSI_NewName Then ' Third Party Insurance

                If Me.EOD_LV.Items.Count > 0 Then
                    Call DSI.Go_Online_DSI()
                    ' refresh the list
                    Call Load_EOD_LV(current_Carrier.CarrierName)
                End If
            Else



                buf = IO_GetSegmentSet(gShipriteDB, "SELECT PackageID From Manifest WHERE [Exported]='Pending' AND [Carrier]='" & current_Carrier.CarrierName & "'")

                Do Until buf = ""
                    Segment = GetNextSegmentFromSet(buf)
                    PackageID = ExtractElementFromSegment("PackageID", Segment)
                    Dim shipment As New _baseShipment

                    Select Case current_Carrier.CarrierName

                        Case "FedEx"
                            If _FedExWeb.Prepare_PackageFromDb(PackageID, shipment) Then
                                If _FedExWeb.Prepare_ShipmentFromDb(PackageID, shipment) Then
                                    _FedExWeb.Upload_Shipment(shipment)
                                End If
                            End If

                        Case "UPS"
                            If _UPSWeb.Prepare_PackageFromDb(PackageID, shipment) Then
                                If _UPSWeb.Prepare_ShipmentFromDb(PackageID, shipment) Then
                                    _UPSWeb.Upload_Shipment(shipment)
                                End If
                            End If

                        Case "DHL"
                            If _Dhl_XML.Prepare_PackageFromDb(PackageID, shipment) Then
                                If _Dhl_XML.Prepare_ShipmentFromDb(PackageID, shipment) Then
                                    _Dhl_XML.Upload_Shipment(shipment)
                                End If
                            End If

                        Case "USPS"
                            If _EndiciaWeb.Prepare_PackageFromDb(PackageID, shipment) Then
                                If _EndiciaWeb.Prepare_ShipmentFromDb(PackageID, shipment) Then
                                    _EndiciaWeb.Upload_Shipment(shipment)
                                End If
                            End If


                    End Select
                Loop
            End If

            Refresh_View()

        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to upload packages")
        End Try
    End Sub

    Private Sub ColumnHeader_Click(sender As Object, e As RoutedEventArgs)
        Try

            Sort_LV_byColumn(EOD_LV, TryCast(e.OriginalSource, GridViewColumnHeader), Last_Sort_Ascending, Last_Column_Sorted)

        Catch ex As Exception : _MsgBox.ErrorMessage(ex.Message, "Failed to sort list items...")
        End Try
    End Sub

    Private Sub AirGround_LB_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles AirGround_LB.SelectionChanged
        If IsNothing(Carrier_ListBox.SelectedItem) Then Exit Sub
        Dim current_Carrier As Carrier = Carrier_ListBox.SelectedItem
        Call Load_EOD_LV(current_Carrier.CarrierName)

        If current_Carrier.CarrierName = "FedEx" And AirGround_LB.SelectedIndex = 1 Then
            'FedEx Ground Selected
            FedExGround_Close_Btn.Visibility = Visibility.Visible
        Else
            FedExGround_Close_Btn.Visibility = Visibility.Hidden
        End If

        CheckShippingDate()
    End Sub

    Private Sub Load_Shipping_Date()
        Dim buf As String
        Dim SQL As String
        Dim ShipDate As Date

        Dim current_Carrier As Carrier = Carrier_ListBox.SelectedItem

        If current_Carrier.CarrierName = "FedEx" Then
            If AirGround_LB.SelectedIndex = 1 Then
                'Ground
                SQL = "SELECT NextPickUpDate FROM Master WHERE Service='FEDEX-GND'"
            Else
                'Air
                SQL = "SELECT NextPickUpDate FROM Master WHERE Service='FEDEX-PRI'"

            End If
        Else
            'All other carriers
            SQL = "SELECT First(NextPickUpDate) As NextPickupDate FROM Master WHERE Carrier='" & current_Carrier.CarrierName & "'"

        End If

        ShippingDate_Border.Visibility = Visibility.Visible
        buf = IO_GetSegmentSet(gShipriteDB, SQL)

        ShipDate = ExtractElementFromSegment("NextPickupDate", buf, Today)

        If ShipDate <= Today Then
            ShippingDate_DP.SelectedDate = Today
        Else
            ShippingDate_DP.SelectedDate = ShipDate
        End If
    End Sub

    Private Sub UpdateShippingDate_Btn_Click(sender As Object, e As RoutedEventArgs) Handles UpdateShippingDate_Btn.Click
        Dim current_Carrier As Carrier = Carrier_ListBox.SelectedItem
        Dim SQL As String = ""

        If current_Carrier.CarrierName = "FedEx" Then
            Select Case AirGround_LB.SelectedIndex
                Case 0 'Air
                    SQL = "Update Master set NextPickupDate=#" & ShippingDate_DP.SelectedDate & "# WHERE Carrier='" & current_Carrier.CarrierName & "' AND Service<>'FEDEX-GND'"

                Case 1 'Ground
                    SQL = "Update Master set NextPickupDate=#" & ShippingDate_DP.SelectedDate & "# WHERE Service='FEDEX-GND'"

                Case 2 'Air & Ground
                    SQL = "Update Master set NextPickupDate=#" & ShippingDate_DP.SelectedDate & "# WHERE Carrier='" & current_Carrier.CarrierName & "'"

            End Select
        Else
            SQL = "Update Master set NextPickupDate=#" & ShippingDate_DP.SelectedDate & "# WHERE Carrier='" & current_Carrier.CarrierName & "'"
        End If

        IO_UpdateSQLProcessor(gShipriteDB, SQL)

        MsgBox(current_Carrier.CarrierName & " shipping date updated!" & vbCrLf & vbCrLf & "Please restart ShipRite on all computers for change to take effect!", vbOKOnly + vbInformation)

    End Sub

    Private Sub VoidShipment_Button_Click(sender As Object, e As RoutedEventArgs) Handles VoidShipment_Button.Click
        If EOD_LV.SelectedIndex = -1 Then Exit Sub

        Dim current_row As System.Data.DataRowView = EOD_LV.SelectedItem
        If ShipmentHistory.Void_Shipment(current_row.Item("PackageID")) Then
            current_row.Delete()
        End If

    End Sub

    Private Sub ShowPackageDetails_Button_Click(sender As Object, e As RoutedEventArgs) Handles ShowPackageDetails_Button.Click
        Show_History_Details()
    End Sub

    Private Sub Show_History_Details()
        If EOD_LV.SelectedIndex = -1 Then Exit Sub

        Dim current_row As System.Data.DataRowView = EOD_LV.SelectedItem
        Dim PackageID As String = current_row.Item("PackageID")

        If PackageID = "" Then Exit Sub

        Dim win As New ShipmentHistory(Me,,, PackageID)
        win.ShowDialog(Me)

        Refresh_View()
    End Sub

    Private Sub Manifest_Button_Click(sender As Object, e As RoutedEventArgs) Handles Manifest_Button.Click
        If Carrier_ListBox.SelectedIndex = -1 Then Exit Sub

        Dim FedExOption As Integer = -1
        Dim current_Carrier As Carrier = Carrier_ListBox.SelectedItem
        Dim SQL As String = "Update Manifest set [Exported]='Exported', [PICKUPDATE]='" & Format(Today, "MM-dd-yyyy") & "', [PICKUPNUMBER]='" & Format(Today, "yyyyMMdd") & "'"
        Dim SQL_WHERE As String = " WHERE Carrier='" & current_Carrier.CarrierName & "' and [Exported]='Pickup Waiting'"

        If current_Carrier.CarrierName = "FedEx" Then

            FedExOption = AirGround_LB.SelectedIndex
            Select Case FedExOption
                Case 0 'Air
                    SQL_WHERE = SQL_WHERE & " and [P1]<>'FEDEX-GND'"

                Case 1 'Ground
                    SQL_WHERE = SQL_WHERE & " and [P1]='FEDEX-GND'"
            End Select

        End If

        If vbNo = MsgBox("Are you sure you want to run daily manifest for " & current_Carrier.CarrierName & "?", vbYesNo + vbQuestion) Then Exit Sub

        IO_UpdateSQLProcessor(gShipriteDB, SQL & SQL_WHERE)

        '--------------------------------------------
        'PRINT MANIFEST HERE

        If current_Carrier.CarrierName = "USPS" Then
            If _EndiciaWeb.EndiciaWeb_IsEnabled Then
                Dim response As New baseWebResponse_Shipment
                Dim package As baseWebResponse_Package = Nothing

                'Print Endicia SCAN Form
                For Each row As System.Data.DataRowView In EOD_LV.Items
                    package = New baseWebResponse_Package
                    package.TrackingNo = row.Item(5)
                    response.Packages.Add(package)
                Next

                If _EndiciaWeb.Request_SCANform(response) Then
                    MsgBox("SCAN Form Successful!  Submission ID=" & response.ShipmentID, vbInformation)
                    Process.Start(_EndiciaWeb.objEndiciaCredentials.LabelFilePath & "\SCANform_" & response.ShipmentID & ".pdf")
                    IO_UpdateSQLProcessor(gShipriteDB, SQL & SQL_WHERE)
                End If
            End If

        Else
            Print_Manifest(current_Carrier.CarrierName, Format(Today, "yyyyMMdd"), FedExOption)
        End If


        Call Load_EOD_LV(current_Carrier.CarrierName)
    End Sub

    Public Shared Sub Print_Manifest(Carrier As String, ManifestNo As String, Optional FedExOption As Integer = 0)
        Dim report As New _ReportObject()
        ' Cursor = Cursors.Wait

        report.ReportFormula = "{Manifest.PICKUPNUMBER} = '" & ManifestNo & "' AND {Manifest.Carrier} = '" & Carrier & "'"

        Select Case Carrier
            Case "FedEx"
                report.ReportName = "FedEx_EOD_Manifest.rpt"

                '0 = Air
                '1 = Ground
                'Else = Both

                If FedExOption = 0 Then
                    report.ReportFormula = report.ReportFormula & " AND {Manifest.P1} <> 'FEDEX-GND'"
                ElseIf FedExOption = 1 Then
                    report.ReportFormula = report.ReportFormula & " AND {Manifest.P1} = 'FEDEX-GND'"
                End If

            Case "UPS"
                report.ReportName = "UPS_EOD_Manifest.rpt"

            Case "DHL"
                report.ReportName = "DHL_EOD_Manifest.rpt"
        End Select

        Dim reportPrev As New ReportPreview(report)
        ' Cursor = Cursors.Arrow
        reportPrev.ShowDialog()
    End Sub

    Private Sub ReprintLabel_Button_Click(sender As Object, e As RoutedEventArgs) Handles ReprintLabel_Button.Click
        If EOD_LV.SelectedIndex = -1 Then Exit Sub

        Dim current_row As System.Data.DataRowView = EOD_LV.SelectedItem
        Dim PackageID As String = current_row.Item("PackageID")

        ShipmentHistory.Reprint_ShippingLabel(PackageID)
    End Sub

    Private Sub FedExGround_Close_Btn_Click(sender As Object, e As RoutedEventArgs) Handles FedExGround_Close_Btn.Click
        If GetPolicyData(gShipriteDB, "FedExREST_Enabled", "False") Then
            FXR_EOD_CloseGround()
        End If
    End Sub

    Private Sub EOD_LV_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles EOD_LV.MouseDoubleClick
        Show_History_Details()
    End Sub
End Class
