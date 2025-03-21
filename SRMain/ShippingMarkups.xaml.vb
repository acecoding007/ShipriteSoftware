Imports System.Windows.Media
Imports SHIPRITE.FedEx_AddressValidationService
Imports SHIPRITE.FedEx_OpenShipService


Public Class ShippingMarkups
    Inherits CommonWindow

    Private Class INT_PriceGroup
        Public Property ServiceTypeID As String
        Public Property ServiceTypeName As String
        Public Property SettingID As String
        Public Property BaseCost As Double
        Public Property SellPrice As Double
    End Class



    Private Class FirstClass_Charge
        Public Property Weight As Double
        Public Property COST_Letter As Double
        Public Property RETAIL_Letter As Double
        Public Property COST_Flat As Double
        Public Property RETAIL_Flat As Double
    End Class

    Public Class Insurance_Charge
        Public Property Amount As Double
        Public Property Cost As Double
        Public Property Sell As Double
    End Class

    Public Class Accessorial_Charge
        Public Property Name As String
        Public Property Cost As Double
        Public Property Sell As Double
        Public Property CostField As String
        Public Property SellField As String
        Public Property Description As String
        Public Property NameBoxField As String 'Additional TextBox added to the Name column in the accessorial revenue listview. Used for Declared Value info. 
        Public Property NameBoxAmount As Double
    End Class




    Dim Packaging_list As List(Of PackagingItem)
    Dim Charge_list As List(Of FirstClass_Charge)
    Dim Surcharge_list As List(Of Peak_Surcharge)
    Dim Insurance_list As List(Of Insurance_Charge)
    Dim ServiceOptions_list As List(Of Object)
    Dim AccessorialCharge_list As List(Of Accessorial_Charge)
    Dim INT_GroupList As List(Of INT_PriceGroup)
    Dim ServiceList As List(Of String)
    Dim Matrix_list As List(Of PricingMatrixItem)
    Dim Zone_List As List(Of Matrix_Zone)
    Dim Weight_List As List(Of String)


    Dim DT As System.Data.DataTable
    Dim searchGrid As GridView

    Dim Preselected_Service As ShippingChoiceDefinition

    Public Sub New()

        MyBase.New()

        ' This call is required by the designer.
        InitializeComponent()

    End Sub

    Public Sub New(ByVal callingWindow As Window, Optional shipment As ShippingChoiceDefinition = Nothing)

        MyBase.New(callingWindow)

        ' This call is required by the designer.
        InitializeComponent()

        If Not IsNothing(shipment) Then
            Preselected_Service = shipment
        End If

    End Sub


    Private Sub ShippingMarkups_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Load_Carriers()
        HideTabItems()
        Options_TabCntrl.Visibility = Visibility.Hidden


        If Not IsNothing(Preselected_Service) Then
            Dim carrierList As List(Of Carrier) = Carrier_ListBox.ItemsSource
            Dim i As Integer

            'preselect Carrier
            For i = 0 To carrierList.Count - 1
                If carrierList(i).CarrierName = Preselected_Service.Carrier Then
                    Exit For
                End If
            Next

            Carrier_ListBox.SelectedIndex = i

            'preselect Service
            Service_ListBox.SelectedItem = Preselected_Service.Service
            Service_ListBox.ScrollIntoView(Service_ListBox.SelectedItem)

        End If

        LoadCarrierSetup()


    End Sub

    Private Sub HideTabItems()
        For Each item As TabItem In Options_TabCntrl.Items
            item.Visibility = Visibility.Collapsed
        Next


    End Sub

    Private Sub NumericTxtBox_PreviewTextInput(sender As Object, e As TextCompositionEventArgs) Handles FirstClass_MarkupPercent_TxtBox.PreviewTextInput,
        Insurance_MarkupPercent_TxtBox.PreviewTextInput,
        LTR_Markup_Fixed_TxtBox.PreviewTextInput, LTR_Markup_Percent_TxtBox.PreviewTextInput, Level1_TxtBox.PreviewTextInput, Level2_TxtBox.PreviewTextInput, Level3_TxtBox.PreviewTextInput,
        Pkg_Length.PreviewTextInput, Pkg_Width.PreviewTextInput, Pkg_Height.PreviewTextInput, Pkg_WeightLimit.PreviewTextInput, FlatR_CAN_Cost.PreviewTextInput, FlatR_CAN_Sell.PreviewTextInput,
        FlatR_Dom_Cost.PreviewTextInput, FlatR_Dom_Sell.PreviewTextInput, PC_Dom_Sell_Txt.PreviewTextInput, PC_Intl_Sell_Txt.PreviewTextInput


        Try
            Dim allowedchars As String = "0123456789.+-"
            If allowedchars.IndexOf(CChar(e.Text)) = -1 Then e.Handled = True

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub SaveButton_Click(sender As Object, e As RoutedEventArgs) Handles SaveButton.Click
        Try

            If Carrier_ListBox.SelectedIndex = -1 Then
                MsgBox("Please select a carrier/service option first!", MsgBoxStyle.Critical)
                Exit Sub
            End If

            For Each item As TabItem In Options_TabCntrl.Items
                If item.IsSelected Then

                    If MsgBox("Do you want to save changes to " & HeaderName.Content & " " & item.Header & "?", vbQuestion + vbYesNo) = MsgBoxResult.Yes Then
                        Select Case item.Name

                            Case CarrierSetup_Tab.Name
                                Save_CarrierSetup()

                            Case ServiceMarkups_Tab.Name
                                Save_Markups()

                            Case ServiceOptions_Tab.Name
                                Save_ServiceOptions()

                            Case Packaging_Tab.Name
                                Save_Packaging()

                            Case PeakCharges_Tab.Name
                                Save_PeakSurcharges()

                            Case PostalInsurance_Tab.Name
                                Save_PostalInsurance()

                            Case Accesorial_Tab.Name
                                Save_Accessorials()

                        End Select

                        Exit For
                    End If
                End If
            Next


        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Save_Markups()
        Try
            For Each item As TabItem In Markups_TabCntrl.Items
                If item.IsSelected Then
                    If item.Name = PricingMatrix_TabItem.Name Then
                        Save_PricingMatrix()

                        Save_Matrix_DefaultMarkups()

                    ElseIf item.Name = LevelMarkup_TabItem.Name Then
                        Save_LevelMarkups()

                    ElseIf item.Name = FirstClass_TabItem.Name Then
                        Save_FirstClassMarkups()
                    End If
                End If
            Next


        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Save_Matrix_DefaultMarkups()
        Dim SQL As String

        If Matrix_Level1_TxtBox.Text = "" Then Matrix_Level1_TxtBox.Text = "0"
        If Matrix_Letter_TxtBx.Text = "" Then Matrix_Letter_TxtBx.Text = "0"
        If Matrix_Letter_Fixed_TxtBx.Text = "" Then Matrix_Letter_Fixed_TxtBx.Text = "0"


        If Not IsNumeric(Matrix_Level1_TxtBox.Text) Or Not IsNumeric(Matrix_Letter_TxtBx.Text) Or Not IsNumeric(Matrix_Letter_Fixed_TxtBx.Text) Then
            MsgBox("Cannot Save Default Markups, please check!", vbExclamation)
            Exit Sub
        End If

        Try

            SQL = "Update Master set [LetterPercent]=" & Matrix_Letter_TxtBx.Text & ", [LetterFee]=" & Matrix_Letter_Fixed_TxtBx.Text & ", [LEVEL1]=" & Matrix_Level1_TxtBox.Text & " WHERE [SERVICE]='" & Service_ListBox.SelectedItem & "'"

            If IO_UpdateSQLProcessor(gShipriteDB, SQL) = 0 Then
                MsgBox("Cannot Save Default Markups, please check!", vbExclamation)
                Exit Sub
            End If


        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub


#Region "Carrier And Service selection And tab display"
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

            Check_SpeeDee_Availability(CarrierList)

            Carrier_ListBox.ItemsSource = CarrierList

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Check_SpeeDee_Availability(ByRef CarrierList As List(Of Carrier))
        Try

            If CarrierList.FindIndex(Function(x) x.CarrierName = "SPEE-DEE") = -1 And SpeeDee.CheckSpeeDee_Zip_Availability() Then
                'SpeeDee available but not setup/enabled

                Dim Current_Carrier As Carrier = New Carrier
                Current_Carrier.CarrierName = "SPEE-DEE"
                Current_Carrier.CarrierImage = "Resources/Spee-Dee_Logo.png"

                CarrierList.Add(Current_Carrier)


                Add_SpeeDee_Btn.Visibility = Visibility.Visible
                SpeeDee_AccountNumber_TxtBx.Visibility = Visibility.Hidden
                SpeeDee_AccountNumber_TxtBx.Visibility = Visibility.Hidden

            Else
                Add_SpeeDee_Btn.Visibility = Visibility.Hidden
                SpeeDee_AccountNumber_TxtBx.Visibility = Visibility.Visible
                SpeeDee_AccountNumber_TxtBx.Visibility = Visibility.Visible
            End If



        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try

    End Sub


    Private Sub DisplayServiceOptions() Handles Carrier_ListBox.SelectionChanged
        'Display List of Services for selected Carrier
        Try
            Dim SegmentSet As String = ""
            Dim current_segment As String = ""
            Dim buf As String = ""
            Dim fieldName As String = ""
            Dim fieldValue As String = ""

            If Carrier_ListBox.SelectedIndex <> -1 Then

                ServiceList = New List(Of String)

                buf = IO_GetSegmentSet(gShipriteDB, "SELECT DISTINCT [Service] from Master WHERE [Carrier]='" & Carrier_ListBox.SelectedItem.CarrierName & "'")

                Do Until buf = ""
                    current_segment = GetNextSegmentFromSet(buf)
                    current_segment = ExtractNextElementFromSegment(fieldName, fieldValue, current_segment)

                    If fieldValue Is Nothing Then fieldValue = ""

                    ServiceList.Add(fieldValue)
                Loop

                Service_ListBox.ItemsSource = ServiceList
                Service_ListBox.Items.Refresh()
                HeaderName.Content = Carrier_ListBox.SelectedItem.CarrierName

                DisplayCarrierTabItems()
                DisplayPackagingOptions()

            End If

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub DisplayCarrierTabItems()
        Try
            Options_TabCntrl.Visibility = Visibility.Visible
            HideTabItems()
            Select Case Carrier_ListBox.SelectedItem.CarrierName
                Case "FedEx"
                    Packaging_Tab.Visibility = Visibility.Visible
                    PeakCharges_Tab.Visibility = Visibility.Visible
                    CarrierSetup_Tab.Visibility = Visibility.Visible

                    Options_TabCntrl.SelectedItem = CarrierSetup_Tab
                    Load_Peak_Surcharges()

                Case "UPS"
                    Packaging_Tab.Visibility = Visibility.Visible
                    PeakCharges_Tab.Visibility = Visibility.Visible
                    CarrierSetup_Tab.Visibility = Visibility.Visible

                    Options_TabCntrl.SelectedItem = CarrierSetup_Tab
                    Load_Peak_Surcharges()

                Case "DHL"
                    Packaging_Tab.Visibility = Visibility.Visible
                    PeakCharges_Tab.Visibility = Visibility.Visible
                    CarrierSetup_Tab.Visibility = Visibility.Visible

                    Options_TabCntrl.SelectedItem = CarrierSetup_Tab
                    Load_Peak_Surcharges()

                Case "USPS"
                    Packaging_Tab.Visibility = Visibility.Visible
                    PostalInsurance_Tab.Visibility = Visibility.Visible
                    CarrierSetup_Tab.Visibility = Visibility.Visible

                    Options_TabCntrl.SelectedItem = CarrierSetup_Tab

                Case "SPEE-DEE"
                    Packaging_Tab.Visibility = Visibility.Visible
                    PeakCharges_Tab.Visibility = Visibility.Hidden
                    CarrierSetup_Tab.Visibility = Visibility.Visible

                    Options_TabCntrl.SelectedItem = CarrierSetup_Tab

            End Select
        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub DisplayServiceTabItems()
        Try
            HideTabItems()

            ServiceOptions_Tab.Visibility = Visibility.Visible
            ServiceMarkups_Tab.Visibility = Visibility.Visible
            Accesorial_Tab.Visibility = Visibility.Visible

            Options_TabCntrl.SelectedItem = ServiceMarkups_Tab

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Function GetServiceName() As String
        Try
            Return ExtractElementFromSegment("DESCRIPTION", GetNextSegmentFromSet(IO_GetSegmentSet(gShipriteDB, "SELECT [DESCRIPTION] from Master WHERE [SERVICE]='" & Service_ListBox.SelectedItem & "'")))

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
        Return ""
    End Function

    Private Sub Service_ListBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles Service_ListBox.SelectionChanged
        Try

            If Service_ListBox.SelectedIndex = -1 Then
                Exit Sub
            End If

            HeaderName.Content = GetServiceName()
            DisplayServiceTabItems()
            Load_Service_Options()
            Load_Accessorial_Charges()

            'makes tab headers not visible in run time. 
            For Each currentTab As TabItem In Markups_TabCntrl.Items
                currentTab.Visibility = Visibility.Collapsed
            Next


            If Service_ListBox.SelectedItem = "FirstClass" Then
                FirstClass_TabItem.IsSelected = True
                Load_FirstClass_Charges()
            Else


                If _IDs.IsIt_PostNetStore Or GetPolicyData(gShipriteDB, "Enable_Pricing_Matrix", "False") Then

                    Dim buf As String
                    Dim current_segment As String
                    buf = IO_GetSegmentSet(gShipriteDB, "SELECT [LEVEL1], [LetterPercent], [LetterFee] from Master WHERE [SERVICE]='" & Service_ListBox.SelectedItem & "'")
                    current_segment = GetNextSegmentFromSet(buf)

                    Matrix_Letter_TxtBx.Text = ExtractElementFromSegment("LetterPercent", current_segment, "0")
                    Matrix_Letter_Fixed_TxtBx.Text = ExtractElementFromSegment("LetterFee", current_segment, "0")
                    Matrix_Level1_TxtBox.Text = ExtractElementFromSegment("LEVEL1", current_segment, "0")


                    Display_PricingMatrix()
                Else

                    Markups_TabCntrl.SelectedItem = LevelMarkup_TabItem
                    DisplayLevelMarkups()
                End If

                If FirstClass_TabItem.IsSelected = True Then
                    PricingMatrix_TabItem.IsSelected = True
                End If

            End If

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

#End Region


    '------------------------- Markups Tab----------------
#Region "Pricing Matrix"
    Private Sub Display_PricingMatrix()
        Dim buf As String
        Dim current_segment As String
        Dim item As PricingMatrixItem

        Matrix_list = New List(Of PricingMatrixItem)

        Load_Zone_List()
        Load_Weight_List()

        buf = IO_GetSegmentSet(gPricingMatrixDB, "SELECT * From PricingMatrix Where Service='" & Service_ListBox.SelectedItem & "'")


        Do Until buf = ""
            current_segment = GetNextSegmentFromSet(buf)

            item = New PricingMatrixItem
            item.WeightStart = ExtractElementFromSegment("Weight_Start", current_segment, "")
            item.WeightEnd = ExtractElementFromSegment("Weight_End", current_segment, "")
            item.Zone = ExtractElementFromSegment("Zone", current_segment, "")
            item.ID = ExtractElementFromSegment("ID", current_segment, "")
            item.Markup = ExtractElementFromSegment("Markup", current_segment, "")
            item.ZoneList = DeepCopy(Zone_List)
            item.WeightList = Weight_List


            Select_Zones_From_Matrix(item)

            Matrix_list.Add(item)
        Loop


        Matrix_list = Matrix_list.OrderBy(Function(x) x.Zone).ThenBy(Function(x) IsNumeric(x.WeightStart)).ToList

        Matrix_Add_Blank_Line()

        PricingMatrix_LV.ItemsSource = Matrix_list
    End Sub

    Private Sub Load_Weight_List()
        Weight_List = New List(Of String)

        Weight_List.Add("ALL")
        Weight_List.Add("LETTER")
        Weight_List.Add("0")

        For y As Integer = 1 To 149
            Weight_List.Add(y.ToString)
        Next

        Weight_List.Add("150+")
    End Sub

    Private Sub Select_Zones_From_Matrix(ByRef item As PricingMatrixItem)
        Dim SelectedZones As List(Of String) = New List(Of String)
        SelectedZones = Strings.Split(item.Zone, ",").ToList

        For Each SelectedZone As String In SelectedZones
            For Each zone As Matrix_Zone In item.ZoneList
                If zone.Zone = SelectedZone Then
                    zone.isSelected = True
                    Exit For
                End If
            Next
        Next


    End Sub

    Private Sub Load_Zone_List()
        Dim Fields As String
        Dim field As String = ""
        Dim Services_DB As String = ""
        Dim matrixItem As Matrix_Zone
        Dim Zone_byNumber As List(Of Matrix_Zone) = New List(Of Matrix_Zone)
        Dim Zone_byString As List(Of Matrix_Zone) = New List(Of Matrix_Zone)

        Zone_List = New List(Of Matrix_Zone)

        Select Case Carrier_ListBox.SelectedItem.CarrierName
            Case "FedEx"
                Services_DB = gFedExServicesDB
            Case "UPS"
                Services_DB = gUPSServicesDB
            Case "DHL"
                Services_DB = gDHLServicesDB
            Case "USPS"
                Services_DB = gUSMailDB_Services
        End Select

        Fields = IO_GetFieldsCollection(Services_DB, Service_ListBox.SelectedItem, "", False, False, True)

        'Loads list of avaialble zones from rate table
        Do Until Fields = ""
            Fields = ExtractNextElementFromSegment(field, "", Fields)
            If field <> "LBS" Then
                field = Replace(field, "ZONE", "")
                matrixItem = New Matrix_Zone
                matrixItem.Zone = field

                Zone_List.Add(matrixItem)
            End If
        Loop

        'separate out zones that are Letters and Numbers to sort separately
        Zone_byString = Zone_List.Where(Function(x) Not IsNumeric(x.Zone)).ToList
        Zone_byNumber = Zone_List.Where(Function(x) IsNumeric(x.Zone)).OrderBy(Function(y) Convert.ToInt32(y.Zone)).ToList

        Zone_List = Zone_byNumber
        Zone_List.AddRange(Zone_byString)

        'Add "ALL" option to first position
        matrixItem = New Matrix_Zone
        matrixItem.Zone = "ALL"
        Zone_List.Insert(0, matrixItem)

    End Sub


    Private Sub Save_PricingMatrix()
        Try
            Dim SQL As String

            For Each item As PricingMatrixItem In Matrix_list
                item.Service = Service_ListBox.SelectedItem
                item.Carrier = Carrier_ListBox.SelectedItem.CarrierName
                item.Zone = ","

                For Each x In item.ZoneList
                    If x.isSelected Then
                        item.Zone &= x.Zone & ","
                    End If
                Next

                If item.Zone = "," Then item.Zone = "ALL"

                If item.WeightStart = "LETTER" Then item.WeightEnd = "LETTER"
                If item.WeightEnd = "LETTER" Then item.WeightStart = "LETTER"

                If item.WeightStart = "ALL" Then item.WeightEnd = "ALL"
                If item.WeightEnd = "ALL" Then item.WeightStart = "ALL"


                If item.WeightStart <> "" And item.WeightEnd <> "" And item.Markup <> "" Then
                    If item.Status = "Added" Then
                        SQL = "INSERT INTO PricingMatrix ([Service], [Carrier], [Weight_Start], [Weight_End], [Zone], [Markup]) VALUES ('" & item.Service & "', '" & item.Carrier & "', '" & item.WeightStart & "', '" & item.WeightEnd & "', '" & item.Zone & "', " & item.Markup & ")"
                        IO_UpdateSQLProcessor(gPricingMatrixDB, SQL)

                    ElseIf item.Status = "Deleted" Then
                        SQL = "DELETE * FROM PricingMatrix WHERE [ID]=" & item.ID
                        IO_UpdateSQLProcessor(gPricingMatrixDB, SQL)

                    ElseIf item.Status = "Edited" Then
                        SQL = "UPDATE PricingMatrix SET [Weight_Start]='" & item.WeightStart & "', [Weight_End]='" & item.WeightEnd & "', [Zone]='" & item.Zone & "', [Markup]=" & item.Markup & " WHERE [ID]=" & item.ID
                        IO_UpdateSQLProcessor(gPricingMatrixDB, SQL)

                    End If
                End If

            Next

            Display_PricingMatrix()

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try

    End Sub

    Private Sub Matrix_TextChanged()
        If PricingMatrix_LV.SelectedIndex = -1 Then Exit Sub

        Dim item As PricingMatrixItem = PricingMatrix_LV.SelectedItem

        If item.Status <> "Added" And item.Status <> "Deleted" Then
            item.Status = "Edited"
        End If

    End Sub


    Protected Sub SelectCurrentItem(ByVal sender As Object, ByVal e As KeyboardFocusChangedEventArgs)
        'When clicking inside a textbox, select the listiew item that the textbox belongs to.
        Dim item As ListViewItem = CType(sender, ListViewItem)
        item.IsSelected = True
    End Sub

    Private Sub ListViewItem_LostFocus(sender As Object, e As RoutedEventArgs)
        Try

            Dim item As PricingMatrixItem = PricingMatrix_LV.SelectedItem

            If item.WeightStart = "ALL" Then item.WeightEnd = "ALL"
            If item.WeightStart = "LETTER" Then item.WeightEnd = "LETTER"

            If item.WeightEnd = "ALL" Then item.WeightStart = "ALL"
            If item.WeightEnd = "LETTER" Then item.WeightStart = "LETTER"

            If item.WeightStart <> "ALL" And item.WeightStart <> "LETTER" Then
                'don't allow descending weight range
                If CInt(item.WeightStart) > CInt(item.WeightEnd) Then
                    item.WeightEnd = item.WeightStart
                End If
            End If

            If PricingMatrix_LV.SelectedIndex = PricingMatrix_LV.Items.Count - 1 Then
                'Last item on list selected
                If item.WeightStart <> "" And item.WeightEnd <> "" And item.Markup <> "" Then
                    Matrix_Add_Blank_Line()
                End If
            End If

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Matrix_Delete_Btn_Click()

        If PricingMatrix_LV.SelectedIndex = PricingMatrix_LV.Items.Count - 1 Then Exit Sub

        If PricingMatrix_LV.SelectedIndex = -1 Then
            MsgBox("No Line Entry Selected", MsgBoxStyle.OkOnly + MsgBoxStyle.Exclamation, "Cannot delete Entry")
            Exit Sub
        End If

        Dim item As PricingMatrixItem = PricingMatrix_LV.SelectedItem

        If item.Status = "Added" Then
            Matrix_list.Remove(item)
        ElseIf item.Status = "Deleted" Then
            item.Status = "Edited"
        Else
            item.Status = "Deleted"
        End If

        PricingMatrix_LV.Items.Refresh()

    End Sub


    Private Sub Matrix_Add_Blank_Line()
        Dim item As PricingMatrixItem = New PricingMatrixItem
        item.Status = "Added"
        item.Zone = "ALL"
        item.ZoneList = DeepCopy(Zone_List)
        item.WeightList = Weight_List

        Matrix_list.Add(item)
        PricingMatrix_LV.Items.Refresh()

    End Sub

    Private Sub Matrix_Letter_TxtBx_LostFocus(sender As Object, e As RoutedEventArgs) Handles Matrix_Letter_TxtBx.LostFocus
        If Matrix_Letter_TxtBx.Text <> "" And Matrix_Letter_TxtBx.Text <> "0" Then
            Matrix_Letter_Fixed_TxtBx.Text = "0"
        End If
    End Sub

    Private Sub Matrix_Letter_Fixed_TxtBx_LostFocus(sender As Object, e As RoutedEventArgs) Handles Matrix_Letter_Fixed_TxtBx.LostFocus
        If Matrix_Letter_Fixed_TxtBx.Text <> "" And Matrix_Letter_Fixed_TxtBx.Text <> "0" Then
            Matrix_Letter_TxtBx.Text = "0"
        End If
    End Sub


#End Region

#Region "Level Markups"
    Private Sub DisplayLevelMarkups()
        Try
            Dim buf As String
            Dim current_segment As String
            buf = IO_GetSegmentSet(gShipriteDB, "SELECT [LEVEL1], [LEVEL2], [LEVEL3], [LetterFee], [LetterPercent], [RETAIL] from Master WHERE [SERVICE]='" & Service_ListBox.SelectedItem & "'")
            current_segment = GetNextSegmentFromSet(buf)

            Level1_TxtBox.Text = ExtractElementFromSegment("LEVEL1", current_segment, "0")
            Level2_TxtBox.Text = ExtractElementFromSegment("LEVEL2", current_segment, "0")
            Level3_TxtBox.Text = ExtractElementFromSegment("LEVEL3", current_segment, "0")
            LevelR_TxtBox.Text = ExtractElementFromSegment("RETAIL", current_segment, "0")

            LTR_Markup_Fixed_TxtBox.Text = ExtractElementFromSegment("LetterFee", current_segment, "0")
            LTR_Markup_Percent_TxtBox.Text = ExtractElementFromSegment("LetterPercent", current_segment, "0")

            If (Carrier_ListBox.SelectedItem.CarrierName = "FedEx" And GetPolicyData(gShipriteDB, "AlwaysChargeFedExRetail")) Or (Carrier_ListBox.SelectedItem.CarrierName = "UPS" And GetPolicyData(gShipriteDB, "AlwaysChargeUPSRetail")) Then
                'Always charge retail is enabled, hide Level 1, 2, 3 markups
                LevelMarkup_Description_TxtBx.Text = "Enter percentage to markup carrier retail rates." & vbCrLf & "(Always Charge Retail Rates is enabled in Carrier Setup)"
                Level123_Grid.Visibility = Visibility.Hidden
                LevelRpercent_Lbl.Visibility = Visibility.Visible
                LevelR_TxtBox.Visibility = Visibility.Visible
                LevelR_Lbl.Visibility = Visibility.Visible

            Else
                'Display Markups Level 1, 2, and 3
                LevelMarkup_Description_TxtBx.Text = "Enter percentage markup for each level. Level Ranges are based on the $ shipping cost and can be setup in Shipping Setup."
                Level123_Grid.Visibility = Visibility.Visible
                LevelRpercent_Lbl.Visibility = Visibility.Collapsed
                LevelR_TxtBox.Visibility = Visibility.Collapsed
                LevelR_Lbl.Visibility = Visibility.Collapsed
            End If

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try

    End Sub

    Private Sub Save_LevelMarkups()
        Dim SQL As String

        If Not IsNumeric(Level1_TxtBox.Text) Or Not IsNumeric(Level2_TxtBox.Text) Or Not IsNumeric(Level3_TxtBox.Text) Or Not IsNumeric(LTR_Markup_Fixed_TxtBox.Text) Or Not IsNumeric(LTR_Markup_Percent_TxtBox.Text) Or Not IsNumeric(LevelR_TxtBox.Text) Then
            MsgBox("Cannot Save Level Markups, please check!", vbExclamation)
            Exit Sub
        End If

        Try

            SQL = "Update Master set [LetterPercent]=" & LTR_Markup_Percent_TxtBox.Text & ", [LetterFee]=" & LTR_Markup_Fixed_TxtBox.Text & ", [LEVEL1]=" & Level1_TxtBox.Text & ", [LEVEL2]=" & Level2_TxtBox.Text & ", [LEVEL3]=" & Level3_TxtBox.Text & ", [RETAIL]=" & LevelR_TxtBox.Text & " WHERE [SERVICE]='" & Service_ListBox.SelectedItem & "'"

            If IO_UpdateSQLProcessor(gShipriteDB, SQL) = 0 Then
                MsgBox("Cannot Save Level Markups, please check!", vbExclamation)
                Exit Sub

            Else
                MsgBox("Level Markups Saved Successfully!", vbInformation)
            End If


        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub LTR_Markup_Percent_TxtBox_LostFocus(sender As Object, e As RoutedEventArgs) Handles LTR_Markup_Percent_TxtBox.LostFocus
        If LTR_Markup_Percent_TxtBox.Text <> "" And LTR_Markup_Percent_TxtBox.Text <> "0" Then
            LTR_Markup_Fixed_TxtBox.Text = "0"
        End If
    End Sub

    Private Sub LTR_Markup_Fixed_TxtBox_LostFocus(sender As Object, e As RoutedEventArgs) Handles LTR_Markup_Fixed_TxtBox.LostFocus
        If LTR_Markup_Fixed_TxtBox.Text <> "" And LTR_Markup_Fixed_TxtBox.Text <> "0" Then
            LTR_Markup_Percent_TxtBox.Text = "0"
        End If
    End Sub

#End Region

#Region "First Class Markups"
    Private Sub Load_FirstClass_Charges()
        Try
            Dim Buffer As String = ""
            Dim RetailBuffer As String = ""
            Dim current_segment As String
            Dim RetailSegment As String
            Dim charge As FirstClass_Charge
            Charge_list = New List(Of FirstClass_Charge)
            Dim letterCostField As String = "Cost-Letter"

            If _EndiciaWeb.EndiciaWeb_IsEnabled Then
                letterCostField = "Comm-Letter"
            End If

            Buffer = IO_GetSegmentSet(gUSMailDB_Services, "Select [WEIGHT], [" & letterCostField & "], [Cost-Flat], [COST-Postcard], [COST-Postcard-Intl] From FirstClass")
            RetailBuffer = IO_GetSegmentSet(gPackagingDB, "Select [WEIGHT], [Retail-Letter], [Retail-Flat], [RETAIL-Postcard], [RETAIL-Postcard-Intl] From FirstClassRetail")

            Do Until Buffer = ""
                current_segment = GetNextSegmentFromSet(Buffer)
                RetailSegment = GetNextSegmentFromSet(RetailBuffer)

                If CDbl(ExtractElementFromSegment("WEIGHT", current_segment, "0")) <= 13 Then
                    charge = New FirstClass_Charge

                    charge.Weight = ExtractElementFromSegment("WEIGHT", current_segment, "0")
                    charge.COST_Letter = ExtractElementFromSegment(letterCostField, current_segment, "0")
                    charge.RETAIL_Letter = ExtractElementFromSegment("Retail-Letter", RetailSegment, "0")
                    charge.COST_Flat = ExtractElementFromSegment("Cost-Flat", current_segment, "0")
                    charge.RETAIL_Flat = ExtractElementFromSegment("Retail-Flat", RetailSegment, "0")

                    If CDbl(ExtractElementFromSegment("WEIGHT", current_segment, "0")) = 1 Then
                        'check for Postcard pricing
                        PC_Dom_Cost_Txt.Text = FormatCurrency(ExtractElementFromSegment("COST-Postcard", current_segment, "0"))
                        PC_Intl_Cost_Txt.Text = FormatCurrency(ExtractElementFromSegment("COST-Postcard-Intl", current_segment, "0"))

                        PC_Dom_Sell_Txt.Text = FormatCurrency(ExtractElementFromSegment("RETAIL-Postcard", RetailSegment, "0"))
                        PC_Intl_Sell_Txt.Text = FormatCurrency(ExtractElementFromSegment("RETAIL-Postcard-Intl", RetailSegment, "0"))
                    End If


                    Charge_list.Add(charge)
                End If
            Loop

            FirstClass_LV.ItemsSource = Charge_list

            'First Class Package service is discontinued
            ' FirstClassPackage_Markup_TxtBox.Text = ExtractElementFromSegment("LEVEL1", GetNextSegmentFromSet(IO_GetSegmentSet(gShipriteDB, "Select [LEVEL1] From Master Where SERVICE='" & Service_ListBox.SelectedItem & "'")), "0")

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub



    Private Sub Apply_PercentMarkup_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Apply_PercentMarkup_Btn.Click
        Try
            If FirstClass_MarkupPercent_TxtBox.Text = "" Then
                Exit Sub
            End If

            If IsNothing(Charge_list) Then Exit Sub

            For Each item As FirstClass_Charge In Charge_list
                item.RETAIL_Flat = item.COST_Flat + item.COST_Flat * (CDbl(FirstClass_MarkupPercent_TxtBox.Text) / 100)
            Next

            FirstClass_LV.Items.Refresh()
        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Save_FirstClassMarkups()
        Try
            Dim SQL As String
            Dim Percent As String

            For Each item As FirstClass_Charge In Charge_list

                SQL = "Update FirstClassRetail set [RETAIL-Letter]=" & item.RETAIL_Letter & ", [RETAIL-Flat]=" & item.RETAIL_Flat & " WHERE [WEIGHT]=" & item.Weight

                If IO_UpdateSQLProcessor(gPackagingDB, SQL) = -1 Then
                    Exit Sub
                End If
            Next

            'First Class Package service is discontinued
            'Percent = FirstClassPackage_Markup_TxtBox.Text
            'IO_UpdateSQLProcessor(gShipriteDB, "Update Master set [LEVEL1]=" & Percent & ", [LEVEL2]=" & Percent & ", [LEVEL3]=" & Percent & " WHERE SERVICE='" & Service_ListBox.SelectedItem & "'")

            IO_UpdateSQLProcessor(gPackagingDB, "Update FirstClassRetail set [RETAIL-Postcard]=" & (PC_Dom_Sell_Txt.Text).Replace("$", "") & ", [RETAIL-Postcard-Intl]=" & (PC_Intl_Sell_Txt.Text).Replace("$", "") & " WHERE [WEIGHT]=1")

            MsgBox(HeaderName.Content & " First Class Markups Saved Successfully!", vbInformation)


        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub


#End Region

#Region "Accessorial Charges"

    Private Sub Load_Accessorial_Charges()
        Try

            If GlobalUpdate_Popup.IsOpen = True Then
                GlobalUpdate_Popup.IsOpen = False
            End If


            Dim current_segment As String = GetNextSegmentFromSet(IO_GetSegmentSet(gShipriteDB, "SELECT * from Master WHERE [SERVICE]='" & Service_ListBox.SelectedItem & "'"))

            AccessorialCharge_list = New List(Of Accessorial_Charge)

            Select Case Carrier_ListBox.SelectedItem.CarrierName
                Case "DHL"
                    Add_Accessorial_Item(current_segment, "Residential Surcharge", "This is the surcharge for packages going to residential addresses.", "ACTRESIDENTIAL", "ResidentialSurcharge")
                    Add_Accessorial_Item(current_segment, "Saturday Delivery", "Additional Saturday Delivery Estimated Cost and Retail Cost", "ACTSAT", "SAT")
                    Add_Accessorial_Item(current_segment, "Saturday Pickup", "Additional Saturday Pickup Estimated Cost and Retail Cost", "ACTSATPU", "SATPU")
                    Add_Accessorial_Item(current_segment, "Fuel Surcharge", "The fuel surcharge percentage for Air shipments and the markup percentage.", "ActFuel", "Fuel")
                    Add_Accessorial_Item(current_segment, "Over Sized Piece", "A Large Package Fee will be applied to all domestic packages that are greater than 130 inches in length plus girth but less than the maximum of 165 inches in length plus girth.", "costAHPlus", "AHPlus")
                    Add_Accessorial_Item(current_segment, "Over Weight Piece", "Additional charge and the markup for handling oversized packages where the weight is over 150 lbs or the length plus girth is over 165 in or the length is over 108 in.", "costExcessLimit", "ExcessLimit")

                    Add_Accessorial_Item(current_segment, "DAS Surcharge - RES", "Delivery Area Surcharge Estimated Cost and Retail Cost", "ACTDAS", "DAS")
                    Add_Accessorial_Item(current_segment, "DAS Surcharge - COM", "Commercial Delivery Area Surcharge Estimated Cost and Retail Cost", "DASCOMM", "aDASCOMM")


                    Add_Accessorial_Item(current_segment, "Elevated Risk Surcharge", "Additional charge applied to packages when shipping to a destination country where DHL is operating at elevated risk.", "costDHLElevatedRisk", "DHLElevatedRisk")
                    Add_Accessorial_Item(current_segment, "Restricted Destination Surcharge", "Additional charge applied to packages when shipping to destination country that is subject to trade restrictions imposed by the UN Security Council.", "costDHLRestrictedDest", "DHLRestrictedDest")
                    'Add_Accessorial_Item(current_segment, "Exporter Validation Surcharge", "Additional charge applied to packages when shipping to a destination country that is subject to trade restrictions imposed by federal regulatory agencies such as OFAC.", "costDHLExporterValidation", "DHLExporterValidation")



                Case "UPS"
                    Add_Accessorial_Item(current_segment, "Residential Surcharge", "This is the surcharge for packages going to residential addresses.", "ACTRESIDENTIAL", "ResidentialSurcharge")
                    Add_Accessorial_Item(current_segment, "C.O.D.", "C.O.D. Estimated Cost and Retail Cost", "ACTCOD", "COD")
                    Add_Accessorial_Item(current_segment, "Indirect Signature", "Delivery Confirmation Estimated Cost and Retail Cost", "ACTDELC", "ACK")
                    Add_Accessorial_Item(current_segment, "Delivery Confirmation w Sig", "Delivery Confirmation with Signature Estimated Cost and Retail Cost", "ACTDELSIG", "ACK-S")
                    Add_Accessorial_Item(current_segment, "Adult Signature Required", "Adult Signature Required Service Cost and Charge", "ACTDELSIGADULT", "DELSIGADULT")


                    If isServiceInternational(Service_ListBox.SelectedItem) Then
                        Add_Accessorial_Item(current_segment, "Large Package Fee", "Additional charge and the markup for handling oversized packages where the weight is over 90 lbs or the length plus girth is over 130 in or the length is over 96 in.", "costAHPlus", "AHPlus")
                        Add_Accessorial_Item(current_segment, "Additional Handling", "Additional Handling Estimated Cost and Retail Cost", "ACTAH", "AH")

                    Else
                        Add_Accessorial_Item(current_segment, "Additional Handling - Zone 2", "Additional Handling Estimated Cost and Retail Cost", "OVS2_Cost", "OVS2_Charge")
                        Add_Accessorial_Item(current_segment, "Additional Handling - Zones 3-4", "Additional Handling Estimated Cost and Retail Cost", "ACTAH", "AH")
                        Add_Accessorial_Item(current_segment, "Additional Handling - Zones 5+", "Additional Handling Estimated Cost and Retail Cost", "OVS3_Cost", "OVS3_Charge")

                        Add_Accessorial_Item(current_segment, "Large Package Fee - Zone 2", "Additional charge and the markup for handling oversized packages where the weight is over 90 lbs or the length plus girth is over 130 in or the length is over 96 in.", "OVS4_Cost", "OVS4_Charge")
                        Add_Accessorial_Item(current_segment, "Large Package Fee - Zones 3-4", "Additional charge and the markup for handling oversized packages where the weight is over 90 lbs or the length plus girth is over 130 in or the length is over 96 in.", "costAHPlus", "AHPlus")
                        Add_Accessorial_Item(current_segment, "Large Package Fee - Zones 5+", "Additional charge and the markup for handling oversized packages where the weight is over 90 lbs or the length plus girth is over 130 in or the length is over 96 in.", "OVS5_Cost", "OVS5_Charge")
                    End If





                    Add_Accessorial_Item(current_segment, "Fuel Surcharge", "The fuel surcharge percentage for Air shipments and the markup percentage.", "ActFuel", "Fuel")
                    Add_Accessorial_Item(current_segment, "Saturday Delivery ", "Additional Saturday Delivery Estimated Cost and Retail Cost", "ACTSAT", "SAT")
                    Add_Accessorial_Item(current_segment, "Saturday Pickup (Sat Air Proc Fee)", "Additional Saturday Pickup Estimated Cost and Retail Cost", "ACTSATPU", "SATPU")


                    If isServiceInternational(Service_ListBox.SelectedItem) Then
                        'INTERNATIONAL
                        Add_Accessorial_Item(current_segment, "Remote Area Surcharge (RAS)", "Remote Area Surcharge Estimated Cost and Retail Cost", "ACTDAS", "DAS")
                        Add_Accessorial_Item(current_segment, "Extended Area Surcharge (EAS)", "Extended Area Surcharge Estimated Cost and Retail Cost", "DASCOMM", "aDASCOMM")
                    Else
                        'DOMESTIC
                        Add_Accessorial_Item(current_segment, "DAS Surcharge - RES", "Delivery Area Surcharge Estimated Cost and Retail Cost", "ACTDAS", "DAS")
                        Add_Accessorial_Item(current_segment, "DAS Ext Surcharge - RES", "Extended Delivery Area Surcharge Estimated Cost and Retail Cost", "DasExtCost", "DasExtCharge")
                        Add_Accessorial_Item(current_segment, "DAS Surcharge - COM", "Commercial Delivery Area Surcharge Estimated Cost and Retail Cost", "DASCOMM", "aDASCOMM")
                        Add_Accessorial_Item(current_segment, "DAS Ext Surcharge - COM", "Extended Commercial Delivery Area Surcharge Estimated Cost and Retail Cost", "DasExtCommCost", "DasExtCommCharge")
                        Add_Accessorial_Item(current_segment, "RAS Surcharge - AK", "Alaska Remote Area Surcharge Estimated Cost and Retail Cost", "DasAkCost", "DasAkCharge")
                        Add_Accessorial_Item(current_segment, "RAS Surcharge - HI", "Hawaii Remote Area Surcharge Estimated Cost and Retail Cost", "DasHiCost", "DasHiCharge")
                        Add_Accessorial_Item(current_segment, "RAS Surcharge - US48", "US48 Remote Area Surcharge Estimated Cost and Retail Cost", "ACTAP", "AP")

                    End If

                    If Service_ListBox.SelectedItem = "1DAYEAM" Then
                        'Add_Accessorial_Item(current_segment, "Early Surcharge Markup", "This is the markup percent for Early Surcharge.", "X", "Percent")
                    End If


                Case "FedEx"
                    Add_Accessorial_Item(current_segment, "Residential Surcharge", "This is the surcharge for packages going to residential addresses.", "ACTRESIDENTIAL", "ResidentialSurcharge")

                    If Service_ListBox.SelectedItem = "FEDEX-GND" Then
                        Add_Accessorial_Item(current_segment, "Residential Surcharge - Home Del", "This is the surcharge for packages going to residential addresses via FedEx Home Delivery®", "ResHomeCost", "ResHomeCharge")
                    End If
                    Add_Accessorial_Item(current_segment, "C.O.D", "C.O.D. Estimated Cost and Retail Cost", "ACTCOD", "COD")
                    Add_Accessorial_Item(current_segment, "Indirect Signature Required", "Indirect Signature Service Cost and Charge", "ISigCost", "ISigChg")
                    Add_Accessorial_Item(current_segment, "Direct Signature Required", "Direct Signature Service Cost and Charge", "ACTDELC", "ACK")
                    Add_Accessorial_Item(current_segment, "Adult Signature Required", "Adult Signature Required Service Cost and Charge", "ACTDELSIG", "ACK-S")


                    If isServiceInternational(Service_ListBox.SelectedItem) Or isServiceFreight(Service_ListBox.SelectedItem) Then
                        Add_Accessorial_Item(current_segment, "Additional Handling", "Additional Handling Estimated Cost and Retail Cost", "ACTAH", "AH")
                        Add_Accessorial_Item(current_segment, "Large Package Fee", "Additional charge and the markup for handling oversized packages where the weight is over 90 lbs or the length plus girth is over 130 in or the length is over 96 in.", "costAHPlus", "AHPlus")
                    Else
                        Add_Accessorial_Item(current_segment, "Additional Handling - Zone 2", "Additional Handling Estimated Cost and Retail Cost", "OVS2_Cost", "OVS2_Charge")
                        Add_Accessorial_Item(current_segment, "Additional Handling - Zones 3-4", "Additional Handling Estimated Cost and Retail Cost", "ACTAH", "AH")
                        Add_Accessorial_Item(current_segment, "Additional Handling - Zones 5-6", "Additional Handling Estimated Cost and Retail Cost", "OVS3_Cost", "OVS3_Charge")
                        Add_Accessorial_Item(current_segment, "Additional Handling - Zones 7+", "Additional Handling Estimated Cost and Retail Cost", "OVS7_Cost", "OVS7_Charge")

                        Add_Accessorial_Item(current_segment, "Large Package Fee - Zone 2", "Additional charge and the markup for handling oversized packages where the weight is over 90 lbs or the length plus girth is over 130 in or the length is over 96 in.", "OVS4_Cost", "OVS4_Charge")
                        Add_Accessorial_Item(current_segment, "Large Package Fee - Zones 3-4", "Additional charge and the markup for handling oversized packages where the weight is over 90 lbs or the length plus girth is over 130 in or the length is over 96 in.", "costAHPlus", "AHPlus")
                        Add_Accessorial_Item(current_segment, "Large Package Fee - Zones 5-6", "Additional charge and the markup for handling oversized packages where the weight is over 90 lbs or the length plus girth is over 130 in or the length is over 96 in.", "OVS5_Cost", "OVS5_Charge")
                        Add_Accessorial_Item(current_segment, "Large Package Fee - Zones 7+ ", "Additional charge and the markup for handling oversized packages where the weight is over 90 lbs or the length plus girth is over 130 in or the length is over 96 in.", "OVS8_Cost", "OVS8_Charge")
                    End If



                    Add_Accessorial_Item(current_segment, "Saturday Delivery", "Additional Saturday Delivery Estimated Cost and Retail Cost", "ACTSAT", "SAT")
                    Add_Accessorial_Item(current_segment, "Saturday Pickup", "Additional Saturday Pickup Estimated Cost and Retail Cost", "ACTSATPU", "SATPU")
                    Add_Accessorial_Item(current_segment, "Fuel Surcharge", "The fuel surcharge percentage for Air shipments and the markup percentage.", "ActFuel", "Fuel")


                    'DAS CHARGES--------------------
                    If isServiceInternational(Service_ListBox.SelectedItem) Then
                        'International DAS
                        Add_Accessorial_Item(current_segment, "Intl Out-of-Del-Area Surcharge - RES", "Delivery Area Surcharge Estimated Cost and Retail Cost", "ACTDAS", "DAS")
                        Add_Accessorial_Item(current_segment, "Intl Out-of-Del-Area Surcharge - COM", "Commercial Delivery Area Surcharge Estimated Cost and Retail Cost", "DASCOMM", "aDASCOMM")

                    Else
                        'Domestic DAS
                        Add_Accessorial_Item(current_segment, "DAS Surcharge - RES", "Delivery Area Surcharge Estimated Cost and Retail Cost", "ACTDAS", "DAS")
                        If Service_ListBox.SelectedItem = "FEDEX-GND" Then
                            Add_Accessorial_Item(current_segment, "DAS Surcharge - Home Del", "FedEx Home Delivery® Delivery Area Surcharge Estimated Cost and Retail Cost", "DasHomeDelCost", "DasHomeDelCharge")
                        End If

                        Add_Accessorial_Item(current_segment, "DAS Ext Surcharge - RES", "Extended Delivery Area Surcharge Estimated Cost and Retail Cost", "DasExtCost", "DasExtCharge")

                        If Service_ListBox.SelectedItem = "FEDEX-GND" Then
                            Add_Accessorial_Item(current_segment, "DAS Ext Surcharge - Home Del", "FedEx Home Delivery® Delivery Area Surcharge Estimated Cost and Retail Cost", "DasExtHomeDelCost", "DasExtHomeDelCharge")
                        End If

                        Add_Accessorial_Item(current_segment, "DAS Surcharge - COM", "Commercial Delivery Area Surcharge Estimated Cost and Retail Cost", "DASCOMM", "aDASCOMM")
                        Add_Accessorial_Item(current_segment, "DAS Ext Surcharge - COM", "Extended Commercial Delivery Area Surcharge Estimated Cost and Retail Cost", "DasExtCommCost", "DasExtCommCharge")

                        If Service_ListBox.SelectedItem = "FEDEX-GND" And GetPolicyData(gShipriteDB, "State") = "HI" Then
                            Add_Accessorial_Item(current_segment, "DAS Surcharge - Intra HI", "Intra-Hawaii Delivery Area Surcharge Estimated Cost and Retail Cost", "DasIntHiCost", "DasIntHiCharge")
                        End If
                        Add_Accessorial_Item(current_segment, "DAS Surcharge - AK", "Alaska Delivery Area Surcharge Estimated Cost and Retail Cost", "DasAkCost", "DasAkCharge")
                        Add_Accessorial_Item(current_segment, "DAS Surcharge - HI", "Hawaii Delivery Area Surcharge Estimated Cost and Retail Cost", "DasHiCost", "DasHiCharge")
                        Add_Accessorial_Item(current_segment, "DAS Surcharge - US Remote", "US48 Remote Delivery Area Surcharge Estimated Cost and Retail Cost", "ACTAP", "AP")
                    End If
                    '-----------------------------



                    If Service_ListBox.SelectedItem = "FEDEX-GND" Then
                        Add_Accessorial_Item(current_segment, "Home Delivery - Date Certain", "FedEX Home Delivery Charge for Date Certain Delivery", "costFedEXHDCertain", "FedEXHDCertain")
                        Add_Accessorial_Item(current_segment, "Home Delivery - Evening", "FedEX Home Delivery Charge for Evening delivery", "costFedEXHDEvening", "FedEXHDEvening")
                        Add_Accessorial_Item(current_segment, "Home Delivery - Appointment", "FedEX Home Delivery Charge for Delivery By Appointment", "costFedEXHDAppt", "FedEXHDAppt")
                        Add_Accessorial_Item(current_segment, "Dry Ice", "Dry Ice charge", fldDryIce_Cost, fldDryIce_Charge)
                    End If


                Case "USPS"
                    Add_Accessorial_Item(current_segment, "Certified Mail", "Prove you sent it. See when it was delivered or that a delivery attempt was made.", "ACTSATPU2", "SATPU2")
                    Add_Accessorial_Item(current_segment, "Return Receipt", "Return Receipt", "ACTSAT2", "SAT2")
                    Add_Accessorial_Item(current_segment, "USPS Tracking", "USPS Tracking / Delivery Confirmation", "ACTDELC", "ACK")
                    Add_Accessorial_Item(current_segment, "Signature Confirmation", "Find out information about the date and time an item was delivered, or when a delivery attempt was made. Add security by requiring a signature. A delivery record is kept by USPS and available electronically or by email, upon request. ", "ACTDELSIG", "ACK-S")
                    Add_Accessorial_Item(current_segment, "Adult Signature Required", "This service requires the signature of an adult—someone 21 years of age or older—at the recipient’s address. You’ll get delivery information, as well as the recipient’s signature and name. ", "ACTDELSIGADULT", "DELSIGADULT")

                    If Service_ListBox.SelectedItem = "FirstClass" Then
                        Add_Accessorial_Item(current_segment, "Non-Machineable Surcharge", "Surcharge for Letters that are rigid, odd shaped, square, or contain a rigid object.", "ACTAH", "AH")
                    Else
                        Add_Accessorial_Item(current_segment, "Special Handling - Fragile", "Get preferential handling if you’re sending unusual shipments that are fragile or for other mailable content that needs extra care.", "ACTAH", "AH")
                    End If

                    If isServiceDomestic(Service_ListBox.SelectedItem) And Service_ListBox.SelectedItem <> "USPS-MEDIA" And Service_ListBox.SelectedItem <> "USPS-PRT-MTR" Then
                        'non-standard fee
                        Add_Accessorial_Item(current_segment, "Nonstandard Fee (L > 22 in)", "Nonstandard Fee - Longest Side exceeds 22 inches, but is less then 30 inches.", "LabPackCost", "LabPackCharge")
                        Add_Accessorial_Item(current_segment, "Nonstandard Fee (L > 30 in)", "Nonstandard Fee - Longest Side exceeds 30 inches.", "ACTCTAG", "CTAG")
                        Add_Accessorial_Item(current_segment, "Nonstandard Fee (L > 2 cu ft)", "Nonstandard Fee - Volume exceeds 2 cubic feet", "ACTADDRCOR", "ADD-CORRECTION")
                    End If

                    Add_Accessorial_Item(current_segment, "USPS C.O.D", "C.O.D. Percentage Markup, This is applied to the cost to get the charge.", "", "COD")

                Case "Greyhound"
                    Add_Accessorial_Item(current_segment, "Fuel Surcharge", "The fuel surcharge percentage for Air shipments and the markup percentage.", "ActFuel", "Fuel")

                Case "SPEE-DEE"
                    Add_Accessorial_Item(current_segment, "Residential Surcharge", "Surcharge for packages going to residential addresses.", "ACTRESIDENTIAL", "ResidentialSurcharge")
                    Add_Accessorial_Item(current_segment, "C.O.D", "C.O.D. Estimated Cost and Retail Cost", "ACTCOD", "COD")
                    Add_Accessorial_Item(current_segment, "Delivery Confirmation", "", "ACTDELC", "ACK")
                    Add_Accessorial_Item(current_segment, "Signature Confirmation", "Delivery Confirmation with Signature", "ACTDELSIG", "ACK-S")
                    Add_Accessorial_Item(current_segment, "Adult Signature Required", "Adult Signature Required", "ACTDELSIGADULT", "DELSIGADULT")

                    Add_Accessorial_Item(current_segment, "Fuel Surcharge", "The fuel surcharge percentage for Air shipments and the markup percentage.", "ActFuel", "Fuel")

                    Add_Accessorial_Item(current_segment, "Unboxed Parcel Fee", "Applies to any parcel not fully encased in a shipping container made of corrugated cardboard. Exempt: Packages weighing 1-5 lbs, stackable totes, Coolers", "LabPackCost", "LabPackCharge")
                    Add_Accessorial_Item(current_segment, "DAS Surcharge", "Delivery Area Surcharge", "ACTDAS", "DAS")

                    Add_Accessorial_Item(current_segment, "Large PKG Fee - Zone 2", "Oversize Estimated Cost and Retail Cost - apply to packages measuring 130 to 170 inches", "OVS2_Cost", "OVS2_Charge")
                    Add_Accessorial_Item(current_segment, "Large PKG Fee - Zone 3", "Oversize Estimated Cost and Retail Cost - apply to packages measuring 130 to 170 inches", "OVS3_Cost", "OVS3_Charge")
                    Add_Accessorial_Item(current_segment, "Large PKG Fee - Zone 4", "Oversize Estimated Cost and Retail Cost - apply to packages measuring 130 to 170 inches", "OVS4_Cost", "OVS4_Charge")
                    Add_Accessorial_Item(current_segment, "Large PKG Fee - Zone 5", "Oversize Estimated Cost and Retail Cost - apply to packages measuring 130 to 170 inches", "OVS5_Cost", "OVS5_Charge")
                    Add_Accessorial_Item(current_segment, "Large PKG Fee - Zone 6", "Oversize Estimated Cost and Retail Cost - apply to packages measuring 130 to 170 inches", "OVS6_Cost", "OVS6_Charge")

                    'Add_Accessorial_Item(current_segment, "", "", "", "")


            End Select

            'Declared Value
            Add_Accessorial_Item(current_segment, "Decl. Value - Free Coverage Up To", "Free Insurance coverage up to amount", "", "", "ACTDVBASE")
            Add_Accessorial_Item(current_segment, "Decl. Value - Minimum Cost Up To", "Minimum Insured amount; Cost, and Sell for Minimum Insured Amount!", "DVRateUpToCost", "DVRateUpToCharge", "DVRateUpTo")
            Add_Accessorial_Item(current_segment, "Decl. Value - For Each Additional", "Each additional insured amount; Cost, and Sell for addtional Insured Amount!", "ACTDECVAL", "DV", "ACTDVINC")


            'Third Party Insurance
            Add_Accessorial_Item(current_segment, "3rd Party Ins. - Free Coverage Up To", "Third Party Insurance. Free Insurance coverage up to amount", "", "", "thirdACTDVBASE")
            Add_Accessorial_Item(current_segment, "3rd Party Ins. - Minimum Cost Up To", "Third Party Insurance. Minimum Insured amount; Cost, and Sell for Minimum Insured Amount!", "thirdDVRateUpToCost", "thirdDVRateUpToCharge", "thirdDVRateUpTo")
            Add_Accessorial_Item(current_segment, "3rd Party Ins. - For Each Additional", "Third Party Insurance. Each additional insured amount; Cost, and Sell for addtional Insured Amount!", "thirdACTDecVal", "thirdDV", "thirdACTDVINC")



            Accessorial_LV.ItemsSource = AccessorialCharge_list

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Add_Accessorial_Item(current_segment As String, Name As String, Description As String, CostDBField As String, SellDBField As String, Optional NameTextBoxField As String = "")
        Try
            Dim Accs_Item As Accessorial_Charge = New Accessorial_Charge

            Accs_Item.Name = Name
            Accs_Item.Description = Description
            Accs_Item.SellField = SellDBField
            Accs_Item.CostField = CostDBField
            Accs_Item.NameBoxField = NameTextBoxField


            Accs_Item.NameBoxAmount = Val(ExtractElementFromSegment(NameTextBoxField, current_segment))
            Accs_Item.Cost = Val(ExtractElementFromSegment(CostDBField, current_segment))
            Accs_Item.Sell = Val(ExtractElementFromSegment(SellDBField, current_segment))

            AccessorialCharge_list.Add(Accs_Item)

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Save_Accessorials()
        Try
            Dim SQL As String
            Dim ret As Integer = 0
            Dim ErrorOccured As Boolean = False

            For Each item As Accessorial_Charge In AccessorialCharge_list

                If item.NameBoxField <> "" Then
                    SQL = "Update Master set [" & item.NameBoxField & "]=" & item.NameBoxAmount & " WHERE [SERVICE]='" & Service_ListBox.SelectedItem & "'"

                    IO_UpdateSQLProcessor(gShipriteDB, SQL)
                End If

                If item.CostField <> "" Then
                    SQL = "Update Master set [" & item.CostField & "]=" & item.Cost & ", [" & item.SellField & "]=" & item.Sell & " WHERE [SERVICE]='" & Service_ListBox.SelectedItem & "'"

                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                    If ret = -1 Then
                        ErrorOccured = True
                    End If
                End If
            Next

            If ErrorOccured Then
                MsgBox(HeaderName.Content & " - One or more accessorial items did not save, please refresh screen and double check!", vbExclamation)
            Else
                MsgBox(HeaderName.Content & " Accessorial Surcharges saved successfully!", vbInformation)
            End If


        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub GlobalUpdate_Btn_Click(sender As Object, e As RoutedEventArgs) Handles GlobalUpdate_Btn.Click
        Try
            If GlobalUpdate_Popup.IsOpen = False Then

                If Accessorial_LV.SelectedItems.Count <> 0 Then
                    GlobalUpdate_Service_ListBox.ItemsSource = ServiceList
                    GlobalUpdate_Service_ListBox.Items.Refresh()

                    GlobalUpdate_Service_ListBox.SelectedItems.Add(Service_ListBox.SelectedItem)

                    GlobalUpdate_Popup.IsOpen = True

                Else
                    MsgBox("Please select Accessorial Charges first!", vbExclamation + vbOKOnly)
                End If

            Else
                GlobalUpdate_Popup.IsOpen = False
            End If

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Save_GlobalUpdate_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Save_GlobalUpdate_Btn.Click
        Try
            Dim sql As String

            For Each item As Accessorial_Charge In Accessorial_LV.SelectedItems

                If item.NameBoxField <> "" Then
                    sql = "Update Master set [" & item.NameBoxField & "]=" & item.NameBoxAmount & " WHERE"

                    For Each service As String In GlobalUpdate_Service_ListBox.SelectedItems
                        sql = sql & "[SERVICE] ='" & service & "' OR "
                    Next

                    sql = sql.Substring(0, sql.Length - 4)

                    If IO_UpdateSQLProcessor(gShipriteDB, sql) = -1 Then
                        GlobalUpdate_Popup.IsOpen = False
                    End If
                End If



                If item.CostField <> "" Then
                    sql = "Update Master set [" & item.CostField & "]=" & item.Cost & ", [" & item.SellField & "]=" & item.Sell & " WHERE"

                    For Each service As String In GlobalUpdate_Service_ListBox.SelectedItems
                        sql = sql & "[SERVICE] ='" & service & "' OR "
                    Next

                    sql = sql.Substring(0, sql.Length - 4)
                    If IO_UpdateSQLProcessor(gShipriteDB, sql) = -1 Then
                        GlobalUpdate_Popup.IsOpen = False
                    End If
                End If
            Next

            GlobalUpdate_Popup.IsOpen = False
            MsgBox("Global Update saved successfully", vbInformation)

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub SelectAllGlobal_Btn_Click(sender As Object, e As RoutedEventArgs) Handles SelectAllGlobal_Btn.Click
        GlobalUpdate_Service_ListBox.SelectAll()
    End Sub

    Private Sub Accessorial_SelectAll_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Accessorial_SelectAll_Btn.Click
        Accessorial_LV.SelectAll()
    End Sub

    Private Sub GlobalUpdate_Markups_Btn_Click(sender As Object, e As RoutedEventArgs) Handles GlobalUpdate_Markups_Btn.Click
        Try
            Dim SQL As String

            If MsgBox("Do you want to update the markups for ALL " & Carrier_ListBox.SelectedItem.CarrierName & " services?", vbQuestion + vbYesNo) = MsgBoxResult.Yes Then

                If LevelR_TxtBox.Visibility = Visibility.Visible Then
                    'LEVEL Retail Markup
                    If Not IsNumeric(LevelR_TxtBox.Text) Or Not IsNumeric(LTR_Markup_Fixed_TxtBox.Text) Or Not IsNumeric(LTR_Markup_Percent_TxtBox.Text) Then
                        MsgBox("Cannot Save Level Markups, please check!", vbExclamation)
                        Exit Sub
                    End If

                    SQL = "Update Master set [LetterPercent]=" & LTR_Markup_Percent_TxtBox.Text & ", [LetterFee]=" & LTR_Markup_Fixed_TxtBox.Text & ", [RETAIL]=" & LevelR_TxtBox.Text &
                     " WHERE [Carrier]='" & Carrier_ListBox.SelectedItem.carrierName & "'"

                    IO_UpdateSQLProcessor(gShipriteDB, SQL)


                Else
                    'LEVEL 1, 2, 3

                    If Not IsNumeric(Level1_TxtBox.Text) Or Not IsNumeric(Level2_TxtBox.Text) Or Not IsNumeric(Level3_TxtBox.Text) Or Not IsNumeric(LTR_Markup_Fixed_TxtBox.Text) Or Not IsNumeric(LTR_Markup_Percent_TxtBox.Text) Then
                        MsgBox("Cannot Save Level Markups, please check!", vbExclamation)
                        Exit Sub
                    End If


                    SQL = "Update Master set [LetterPercent]=" & LTR_Markup_Percent_TxtBox.Text & ", [LetterFee]=" & LTR_Markup_Fixed_TxtBox.Text & ", [LEVEL1]=" & Level1_TxtBox.Text &
                    ", [LEVEL2]=" & Level2_TxtBox.Text & ", [LEVEL3]=" & Level3_TxtBox.Text & " WHERE [Carrier]='" & Carrier_ListBox.SelectedItem.carrierName & "'"

                    IO_UpdateSQLProcessor(gShipriteDB, SQL)

                    MsgBox("Global Update saved successfully", vbInformation)
                End If
            End If



        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try

    End Sub


#End Region

    '------------------------- End Markups Tab----------------



#Region "Peak Surcharges"

    Private Sub Save_PeakSurcharges()
        Try
            Dim SQL As String

            For Each item As Peak_Surcharge In PeakSurcharges_LV.ItemsSource
                SQL = "Update Holiday_Charges set [Retail]=" & item.Retail & " WHERE [Surcharge]=" & "'" & item.Surcharge & "' and [Service]='" & item.Service & "'"

                If Carrier_ListBox.SelectedItem.CarrierName = "UPS" Then
                    If IO_UpdateSQLProcessor(gUPSServicesDB, SQL) = -1 Then
                        Exit Sub
                    End If

                ElseIf Carrier_ListBox.SelectedItem.CarrierName = "FedEx" Then
                    If IO_UpdateSQLProcessor(gFedExServicesDB, SQL) = -1 Then
                        Exit Sub
                    End If
                End If

            Next

            MsgBox(HeaderName.Content & " Peak Surcharges saved successfully!", vbInformation)

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub


    Private Sub Load_Peak_Surcharges()
        Try
            Select Case Carrier_ListBox.SelectedItem.CarrierName
                Case "FedEx"
                    PeakSurcharges_LV.ItemsSource = gFedExPeakSurcharges

                Case "UPS"
                    PeakSurcharges_LV.ItemsSource = gUPSPeakSurcharges

                Case "DHL"
                    PeakSurcharges_LV.ItemsSource = gDHLPeakSurcharges
            End Select


        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Apply_PeakPercentMarkup_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Apply_PeakPercentMarkup_Btn.Click
        Try
            If Peak_MarkupPercent_TxtBox.Text = "" Then
                Exit Sub
            End If

            If IsNothing(Surcharge_list) Then Exit Sub

            For Each item As Peak_Surcharge In Surcharge_list
                item.Retail = item.Cost + item.Cost * (CDbl(Peak_MarkupPercent_TxtBox.Text) / 100)
            Next

            PeakSurcharges_LV.Items.Refresh()
        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub



#End Region


#Region "USPS Insurance"

    Private Sub Save_PostalInsurance()
        Try
            Dim costField As String = ""
            Dim sellField As String = ""
            Dim SQL As String = ""
            Dim SQLBegin As String
            Dim SQLEnd As String
            Dim count As Integer = 1
            GetCostSellFields(costField, sellField)

            SQLBegin = "Update Insurance Set " & sellField & "=SWITCH("
            SQLEnd = " WHERE [CoverUpTo] IN("


            For Each item As Insurance_Charge In Insurance_list
                SQL = SQL & "CoverUpTo=" & item.Amount & ", " & item.Sell & ","
                SQLEnd = SQLEnd & item.Amount & ","
                count = count + 1

                If count Mod 14 = 0 Then  'SWITCH Statement can update a maximum of 14 rows at a time. 
                    SQL = SQL.TrimEnd(",")
                    SQL = SQL & ")"

                    SQLEnd = SQLEnd.TrimEnd(",")
                    SQLEnd = SQLEnd & ")"

                    IO_UpdateSQLProcessor(gUSMailDB_Services, SQLBegin & SQL & SQLEnd)
                    SQL = ""
                    SQLEnd = " WHERE [CoverUpTo] IN("
                End If

            Next

            If SQL IsNot "" Then
                SQL = SQL.TrimEnd(",")
                SQL = SQL & ")"

                SQLEnd = SQLEnd.TrimEnd(",")
                SQLEnd = SQLEnd & ")"

                IO_UpdateSQLProcessor(gUSMailDB_Services, SQLBegin & SQL & SQLEnd)
            End If

            MsgBox("USPS Insurance charges saved successfully!", vbInformation)


        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub


    Private Sub Insurance_ListBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles Insurance_ListBox.SelectionChanged
        Try
            Dim Buffer As String = ""
            Dim current_segment As String
            Dim costField As String = ""
            Dim sellField As String = ""

            Dim charge As Insurance_Charge
            Insurance_list = New List(Of Insurance_Charge)

            GetCostSellFields(costField, sellField)

            Dim SQL As String = "Select [CoverUpTo], " & costField & ", " & sellField & " From Insurance "
            Buffer = IO_GetSegmentSet(gUSMailDB_Services, SQL)


            Do Until Buffer = ""
                current_segment = GetNextSegmentFromSet(Buffer)
                charge = New Insurance_Charge

                charge.Amount = ExtractElementFromSegment("CoverUpTo", current_segment, "0")
                charge.Cost = CDbl(ExtractElementFromSegment(costField, current_segment, "0"))
                charge.Sell = CDbl(ExtractElementFromSegment(sellField, current_segment, "0"))

                Insurance_list.Add(charge)
            Loop

            Insurance_lv.ItemsSource = Insurance_list
        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub GetCostSellFields(ByRef CostField As String, ByRef SellField As String)
        Try
            Select Case Insurance_ListBox.SelectedIndex
                Case 0 'domestic
                    CostField = "BaseCost"
                    SellField = "SellPrice"

                Case 1 'express
                    CostField = "EXP_BaseCost"
                    SellField = "EXP_SellPrice"

                Case 2 'express international
                    CostField = "EMI_BaseCost"
                    SellField = "EMI_SellPrice"

                Case 3 'global express guaranteed
                    CostField = "GXG_BaseCost"
                    SellField = "GXG_SellPrice"

                Case 4 'priority international
                    CostField = "PMI_BaseCost"
                    SellField = "PMI_SellPrice"
            End Select

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Apply_InsurancePercentMarkup_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Apply_InsurancePercentMarkup_Btn.Click
        Try
            If Insurance_MarkupPercent_TxtBox.Text = "" Then
                Exit Sub
            End If

            If IsNothing(Insurance_list) Then Exit Sub

            For Each item As Insurance_Charge In Insurance_list
                item.Sell = item.Cost + item.Cost * (CDbl(Insurance_MarkupPercent_TxtBox.Text) / 100)
            Next

            Insurance_lv.Items.Refresh()
        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub




#End Region


#Region "Service Options"
    Private Sub Load_Service_Options()
        Try
            'To add new options to the screen, add a new checkbox or textbox with a TAG of the field name in master table. 
            Dim current_segment As String
            ServiceOptions_list = New List(Of Object)

            Get_ChildControls_Of_Grid(ServiceCheckBoxes_Grid, ServiceOptions_list)
            Get_ChildControls_Of_Grid(ServiceOptions_Grid, ServiceOptions_list)

            current_segment = GetNextSegmentFromSet(IO_GetSegmentSet(gShipriteDB, "SELECT * from Master WHERE [SERVICE]='" & Service_ListBox.SelectedItem & "'"))

            Display_DBData_To_UI(ServiceOptions_list, current_segment)

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Save_ServiceOptions()
        Try
            Dim SQL As String = "Update Master set"

            For Each x As Object In ServiceOptions_list
                If x.GetType = GetType(CheckBox) Then
                    SQL = SQL & " [" & x.tag & "]=" & x.isChecked & ","
                Else
                    SQL = SQL & " [" & x.tag & "]='" & x.text & "',"
                End If
            Next
            SQL = SQL.TrimEnd(",")
            SQL = SQL & " WHERE [SERVICE]='" & Service_ListBox.SelectedItem & "'"

            If IO_UpdateSQLProcessor(gShipriteDB, SQL) = 0 Then
                Exit Sub
            End If

            MsgBox(HeaderName.Content & " service options saved successfully!", vbInformation)

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

#End Region


#Region "Packaging and Flat Rates"

    Private Sub Save_Packaging()
        Try
            If Packaging_ListBox.SelectedIndex = -1 Then Exit Sub
            Dim item As PackagingItem = Packaging_ListBox.SelectedItem


            Dim SQL As String = "Update PackagingItems set "

            SQL = SQL & "[Length]=" & item.Length & ", "
            SQL = SQL & "[Width]=" & item.Width & ", "
            SQL = SQL & "[Height]=" & item.Height & ", "
            SQL = SQL & "[MaxLBs]=" & item.MaxLBs & ", "
            SQL = SQL & "[Disabled]=" & item.Disabled


            SQL = SQL & " WHERE [SettingID]=" & item.SettingID

            If IO_UpdateSQLProcessor(gPackagingDB, SQL) = 0 Then
                Exit Sub
            End If



            '----Save Flat Rate Pricing
            If Packaging_ListBox.SelectedItem.SettingName.Contains("FlatR") Then
                SQL = "Update CarrierPackagingFlatRateValues set SellPrice = SWITCH("
                SQL = SQL & "ServiceTypeID=1, " & FlatR_Dom_Sell.Text

                If FlatR_CAN_Cost.Text <> "" Then
                    SQL = SQL & ", ServiceTypeID=3, " & FlatR_CAN_Sell.Text
                End If

                For Each INT_GroupItem As INT_PriceGroup In INT_GroupList
                    SQL = SQL & ", ServiceTypeID=" & INT_GroupItem.ServiceTypeID & ", " & INT_GroupItem.SellPrice
                Next

                SQL = SQL & ") WHERE SettingID=" & item.SettingID

                If IO_UpdateSQLProcessor(gPackagingDB, SQL) = 0 Then
                    Exit Sub
                End If
            End If

            MsgBox("Packaging options for " & item.SettingName & " saved successfully!", vbInformation)


        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub DisplayPackagingOptions()
        Try
            Dim SQL As String
            Packaging_list = New List(Of PackagingItem)

            If Carrier_ListBox.SelectedItem.carrierName = "USPS" Then
                Load_FlatRate_Packaging(Packaging_list, "SELECT * From PackagingItems WHERE SettingName LIKE '%FlatR%'")
            End If


            SQL = "SELECT PackagingItems.Disabled, CarrierPackagingValues.CarrierID, Carriers.CarrierName, CarrierPackagingValues.SettingID, PackagingItems.SettingName, PackagingItems.Length, PackagingItems.Height, PackagingItems.Width, PackagingItems.MaxLBs, PackagingItems.SettingDesc
FROM PackagingItems INNER JOIN (Carriers INNER JOIN CarrierPackagingValues ON Carriers.CarrierID = CarrierPackagingValues.CarrierID) ON PackagingItems.SettingID = CarrierPackagingValues.SettingID
WHERE (Carriers.CarrierName='" & Carrier_ListBox.SelectedItem.carrierName & "')
ORDER BY PackagingItems.Disabled DESC , CarrierPackagingValues.CarrierID, PackagingItems.SettingOrderNo"

            Load_FlatRate_Packaging(Packaging_list, SQL)

            Packaging_ListBox.ItemsSource = Packaging_list

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub


    Private Sub Packaging_ListBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles Packaging_ListBox.SelectionChanged
        Try
            If Packaging_ListBox.SelectedIndex = -1 Then
                FlatRate_Border.Visibility = Visibility.Hidden
                Exit Sub
            End If

            Dim SelectedText As String = Packaging_ListBox.SelectedItem.SettingName

            If SelectedText.Contains("FlatR") Then
                FlatRate_Border.Visibility = Visibility.Visible
                Load_FlatRate_Pricing()
            Else
                FlatRate_Border.Visibility = Visibility.Hidden
            End If

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Load_FlatRate_Pricing()
        Try
            Dim Buffer As String
            Dim current_segment As String
            Dim INT_GroupItem As INT_PriceGroup
            INT_GroupList = New List(Of INT_PriceGroup)

            FlatR_Dom_Cost.Text = ""
            FlatR_Dom_Sell.Text = ""
            FlatR_CAN_Cost.Text = ""
            FlatR_CAN_Sell.Text = ""

            Dim SQL As String = "SELECT CarrierPackagingFlatRateValues.ServiceTypeID, CarrierServiceTypes.ServiceTypeName, CarrierPackagingFlatRateValues.SettingID, PackagingItems.SettingName, CarrierPackagingFlatRateValues.BaseCost, CarrierPackagingFlatRateValues.SellPrice, PackagingItems.SettingDesc
FROM PackagingItems INNER JOIN (Carriers INNER JOIN (CarrierServiceTypes INNER JOIN CarrierPackagingFlatRateValues ON CarrierServiceTypes.ServiceTypeID = CarrierPackagingFlatRateValues.ServiceTypeID) ON Carriers.CarrierID = CarrierPackagingFlatRateValues.CarrierID) ON PackagingItems.SettingID = CarrierPackagingFlatRateValues.SettingID
WHERE PackagingItems.SettingName='" & Packaging_ListBox.SelectedItem.SettingName & "' " &
    "ORDER BY PackagingItems.Disabled DESC , CarrierServiceTypes.OrderNo, PackagingItems.SettingOrderNo"

            Buffer = IO_GetSegmentSet(gPackagingDB, SQL)

            Do Until Buffer = ""
                current_segment = GetNextSegmentFromSet(Buffer)

                If ExtractElementFromSegment("ServiceTypeID", current_segment) = 1 Then
                    'Domestic Flat Rate pricing
                    FlatR_Dom_Cost.Text = ExtractElementFromSegment("BaseCost", current_segment, "0")
                    FlatR_Dom_Sell.Text = ExtractElementFromSegment("SellPrice", current_segment, "0")

                ElseIf ExtractElementFromSegment("ServiceTypeID", current_segment) = 3 Then
                    'CANADIAN Flat Rate Pricing
                    FlatR_CAN_Cost.Text = ExtractElementFromSegment("BaseCost", current_segment, "0")
                    FlatR_CAN_Sell.Text = ExtractElementFromSegment("SellPrice", current_segment, "0")

                ElseIf ExtractElementFromSegment("ServiceTypeID", current_segment) >= 10 Then
                    'International Price Group
                    INT_GroupItem = New INT_PriceGroup
                    INT_GroupItem.ServiceTypeID = ExtractElementFromSegment("ServiceTypeID", current_segment)
                    INT_GroupItem.ServiceTypeName = ExtractElementFromSegment("ServiceTypeName", current_segment)
                    INT_GroupItem.SettingID = ExtractElementFromSegment("SettingID", current_segment)
                    INT_GroupItem.BaseCost = ExtractElementFromSegment("BaseCost", current_segment, "0")
                    INT_GroupItem.SellPrice = ExtractElementFromSegment("SellPrice", current_segment, "0")

                    INT_GroupList.Add(INT_GroupItem)
                End If

            Loop

            FlatR_INTL_lv.ItemsSource = INT_GroupList

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub TxtBox_GotFocus(sender As Object, e As RoutedEventArgs)
        sender.selectall()
    End Sub




#End Region

#Region "Carrier Setup"
    Private Sub LoadCarrierSetup()

        UPS_LoadDiscountLevels()
        FedEx_LoadDiscountLevels()
        DHL_LoadDiscountLevels()

        'makes tab headers not visible in run time. 
        For Each currentTab As TabItem In Carrier_TabControl.Items
            currentTab.Visibility = Visibility.Collapsed
        Next

        For Each currentTab As TabItem In UPS_Auth_TabControl.Items
            currentTab.Visibility = Visibility.Collapsed
        Next

        For Each currentTab As TabItem In UPSReady_TabControl.Items
            currentTab.Visibility = Visibility.Collapsed
        Next

        For Each currentTab As TabItem In FedEx_TC.Items
            currentTab.Visibility = Visibility.Collapsed
        Next

        UPSReady_Border.Visibility = Visibility.Collapsed
        UPS_AcceptAgreement_Btn.IsEnabled = False

        FedExAgreement_Border.Visibility = Visibility.Collapsed
        FedEx_AcceptAgreement_Btn.IsEnabled = False

        Endicia_Border.Visibility = Visibility.Collapsed

        'NetStamps and Labelserver need only to have one account. Hide option for separate NetStamp Account.
        NetStampSetup_Border.Visibility = Visibility.Hidden

    End Sub

    Private Sub DHL_LoadDiscountLevels()
        DHL_RateType_ComboBox.Items.Add("No Discount")
        DHL_RateType_ComboBox.Items.Add("Tier 1")
        DHL_RateType_ComboBox.Items.Add("Tier 2")
        DHL_RateType_ComboBox.Items.Add("Top Tier")
    End Sub

    Private Sub FedEx_LoadDiscountLevels()

        FedEx_FASC_DiscountLevel_ComboBox.Items.Add("1 ($0-$40,999.99)")
        FedEx_FASC_DiscountLevel_ComboBox.Items.Add("2 ($41,000-)")

    End Sub

    Private Sub UPS_LoadDiscountLevels()

        For count As Integer = 1 To 10
            UPSDiscountLevel_ComboBox.Items.Add("LEVEL_" & count)
        Next
    End Sub


    Private Function GetSelectedCarrierName() As String
        Dim selectedCarrier As Carrier = Carrier_ListBox.SelectedItem
        Dim selectedCarrierName As String = String.Empty
        If selectedCarrier IsNot Nothing Then
            selectedCarrierName = selectedCarrier.CarrierName.ToUpper()
        End If
        Return selectedCarrierName
    End Function

    Private Function GetSelectedCarrierTab(selectedCarrierName As String) As TabItem
        Dim carrierTabList As IEnumerable(Of TabItem) = Carrier_TabControl.Items.Cast(Of TabItem)
        Dim selectedCarrierTabItem As TabItem = carrierTabList.FirstOrDefault(Function(tItem)
                                                                                  Dim tItemName As String = tItem.Name.ToUpper
                                                                                  If tItemName = "SPEEDEE_TABITEM" Then tItemName = "SPEE-DEE_TABITEM"
                                                                                  Return _Controls.Contains(tItemName, selectedCarrierName)
                                                                              End Function)
        Return selectedCarrierTabItem
    End Function

    Private Sub Carriers_ListBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles Carrier_ListBox.SelectionChanged
        '
        Try
            Dim selectedCarrierName As String = GetSelectedCarrierName()
            Dim selectedCarrierTabItem As TabItem = GetSelectedCarrierTab(selectedCarrierName)

            If selectedCarrierTabItem IsNot Nothing Then
                Carrier_TabControl.SelectedItem = selectedCarrierTabItem
            End If
            '
            Select Case selectedCarrierName
                Case "FEDEX"
                    '
                    If _FedExWeb.objFedEx_Regular_Setup Is Nothing Then
                        _FedExWeb.objFedEx_Regular_Setup = New FedEx_Setup(False)
                    End If
                    '
                    If _FedExWeb.objFedEx_Freight_Setup Is Nothing Then
                        _FedExWeb.objFedEx_Freight_Setup = New FedEx_Setup(True)
                    End If
                    '
                    Me.FedEx_AccountNumber_TextBox.Text = _FedExWeb.objFedEx_Regular_Setup.Csp_AccountNumber
                    Me.FedEx_HAL_LocationID_TextBox.Text = _FedExWeb.objFedEx_Regular_Setup.OriginLocationId
                    Me.FedEx_HAL_AgentID_TextBox.Text = _FedExWeb.objFedEx_Regular_Setup.ApplicationId
                    Me.FedEx_AccountNumber_FreightBox_TextBox.Text = _FedExWeb.objFedEx_Freight_Setup.Csp_AccountNumber

                    FedEx_FRT_LocationID_TextBox.Text = GetPolicyData(gShipriteDB, "FedExRETURN_LocationID", "")

                    isFASC_CheckBox.IsChecked = _IDs.IsIt_FedEx_FASC
                    FedEx_DisableEmailNotification_ChkBx.IsChecked = GetPolicyData(gShipriteDB, "Disable_FedEx_EmailShipNotifications", "False")
                    FedEx_MarkupDiscount_CheckBox.IsChecked = GetPolicyData(gShipriteDB, "IsFedExMarkupDiscount", "False")
                    FedEx_AlwaysChargeRetail_CheckBox.IsChecked = GetPolicyData(gShipriteDB, "AlwaysChargeFedExRetail", "False")

                    Dim FedEx_Discount As Integer = Val(Strings.Left(GetPolicyData(gShipriteDB, "RetailFedExLevel", ""), 1))
                    FedEx_FASC_DiscountLevel_ComboBox.SelectedIndex = FedEx_Discount - 1

                    If GetPolicyData(gReportsDB, "FedExLabelType") = "Laser" Then
                        FedEx_PrintLaserLabels_CheckBox.IsChecked = True
                    Else
                        FedEx_PrintLaserLabels_CheckBox.IsChecked = False
                    End If

                    '
                Case "UPS"
                    '
                    ' reload to get current values
                    _UPSWeb.objUPS_Setup = New UPSSetupData
                    '
                    Me.UPSReady_UserID_TextBox.Text = _UPSWeb.objUPS_Setup.ShipRite_Username
                    Me.UPSReady_Password_TextBox.Password = _UPSWeb.objUPS_Setup.ShipRite_Password
                    Me.UPSReady_AccountNumber_TextBox.Text = _UPSWeb.objUPS_Setup.ShipRite_ShipperNumber
                    Me.UPSREST_AccountNumber_TextBox.Text = _UPSWeb.objUPS_Setup.ShipRite_ShipperNumber

                    If UPS_Rest.Api.Authentication.AuthenticationService.IsEnabled Then
                        Me.UPS_Auth_TabControl.SelectedIndex = 1
                    Else
                        Me.UPS_Auth_TabControl.SelectedIndex = 0
                    End If

                    UPS_DisableEmailNotification_ChkBx.IsChecked = GetPolicyData(gShipriteDB, "Disable_UPS_EmailShipNotifications", "False")

                    isASO_CheckBox.IsChecked = _IDs.IsIt_UPS_ASO
                    UPSDiscountLevel_ComboBox.SelectedIndex = Strings.Right(GetPolicyData(gShipriteDB, "UPSLevel", "LEVEL_1"), 1) - 1
                    UPS_MarkupDiscount_CheckBox.IsChecked = GetPolicyData(gShipriteDB, "IsUPSMarkupDiscount", "False")
                    UPS_AlwaysChargeRetail_CheckBox.IsChecked = GetPolicyData(gShipriteDB, "AlwaysChargeUPSRetail", "False")
                    UPS_DropOff_AccessID_TxtBx.Text = GetPolicyData(gShipriteDB, "UPS_AccessID")

                '
                Case "DHL"

                    If _Dhl_XML.objDHL_Setup Is Nothing Then
                        _Dhl_XML.objDHL_Setup = New DHL_Setup
                    End If
                    '
                    Me.DHLAccountNo_TxtBox.Text = _Dhl_XML.objDHL_Setup.ShipperAccountNumber

                    Dim DHLDiscount As String = GetPolicyData(gShipriteDB, "DHL_INTL_RATETABLE", "")
                    DHLDiscount = UCase(DHLDiscount)
                    Select Case DHLDiscount
                        Case "TIER1" : DHL_RateType_ComboBox.SelectedIndex = 1
                        Case "TIER2" : DHL_RateType_ComboBox.SelectedIndex = 2
                        Case "TOPTIER" : DHL_RateType_ComboBox.SelectedIndex = 3
                        Case Else : DHL_RateType_ComboBox.SelectedIndex = 0  'n/a

                    End Select
                    DHL_MarkupDiscount_CheckBox.IsChecked = GetPolicyData(gShipriteDB, "IsDHLMarkupDiscount", "False")
                    DHL_AlwaysChargeRetail_CheckBox.IsChecked = GetPolicyData(gShipriteDB, "AlwaysChargeDhlRetail", "False")
                    '
                Case "USPS"
                    '
                    If _EndiciaWeb.objEndiciaCredentials Is Nothing Then
                        _EndiciaWeb.objEndiciaCredentials = New _EndiciaSetup
                    End If
                    '
                    Me.Endicia_AccountNo_TxtBox.Text = _EndiciaWeb.objEndiciaCredentials.AccountID
                    Me.Endicia_PassPhrase_TxtBox.Password = _EndiciaWeb.objEndiciaCredentials.PassPhrase
                    Me.Endicia_BuyPostage_TextBox.Text = My.Settings.Endicia_LastPostageAmountAdded.ToString("N")

                    USPS_CubicRates_ChkBx.IsChecked = GetPolicyData(gShipriteDB, "Enable_USPSCubicRate", "False")
                    USPS_GroundADV_CubicRates_ChkBx.IsChecked = GetPolicyData(gShipriteDB, "Enable_USPSCubicRate_ParcelSelect", "False")

                    'SRPRO rates discontinued by Endicia 8/12/2024
                    'USPS_SRPRO_Rates_ChkBx.IsChecked = _IDs.IsIt_USPS_SRPRO_Rate

                    USPS_ApprovedShipper_ChkBx.IsChecked = _IDs.IsIt_USPS_ApprovedShipper

                    USPS_LabelType_CmbBx.SelectedValue = GetPolicyData(gReportsDB, "EndiciaLabelType", "Zebra Thermal")

                    'Endicia_NetStamp_SerialNo_TxtBx.Text = GetPolicyData(gReportsDB, "DYMO_ActivationCode", "")
                    'Endicia_NetStamp_Acct_TxtBx.Text = GetPolicyData(gShipriteDB, "Endicia_AccountID2", "")
                    'Endicia_NetStamp_Passphrase_TxtBx.Text = GetPolicyData(gShipriteDB, "Endicia_PassPhrase2", "")

                    If Endicia_AccountNo_TxtBox.Text = Endicia_NetStamp_Acct_TxtBx.Text And Endicia_PassPhrase_TxtBox.Password = Endicia_NetStamp_Passphrase_TxtBx.Text Then
                        Endicia_NetStamp_ChkBx.IsChecked = True
                    End If

                Case "SPEE-DEE"
                    SpeeDee_AccountNumber_TxtBx.Text = GetPolicyData(gShipriteDB, "SpeeDeeAccountNumber", "")

            End Select
            '
        Catch ex As Exception : _MsgBox.ErrorMessage(ex.Message, "Failed to select the Carrier...")
        End Try
    End Sub


#Region "FedEx"

    Private Sub FedEx_Register_Btn_Click(sender As Object, e As RoutedEventArgs) Handles FedEx_Register_Btn.Click, FedEx_FreightBox_Register_Btn.Click

        Dim cmd As Button = CType(sender, Button)
        cmd.IsEnabled = False
        Try
            _FedExWeb.objFedEx_Setup = _FedExWeb.objFedEx_Regular_Setup
            If (Not String.IsNullOrEmpty(Me.FedEx_AccountNumber_TextBox.Text) And Not Me.FedEx_Register_Btn.IsEnabled) Or (Not String.IsNullOrEmpty(Me.FedEx_AccountNumber_FreightBox_TextBox.Text) And Not Me.FedEx_FreightBox_Register_Btn.IsEnabled) Then
                If _Files.IsFileExist(gDBpath & "\FEDEX_REG_EULA.txt", False) Or _Files.IsFileExist(gDBpath & "\FEDEX_REG_EULA.tmp", False) Then
                    '
                    If load_FedExEULA() Then
                        FedExAgreement_Border.Visibility = Visibility.Visible
                        FedEx_ReadAgreement_CheckBox.IsChecked = False
                    End If
                    '
                Else
                    _MsgBox.WarningMessage("FedEx EULA agreement file missing. Contact Support.")
                End If
            End If
        Catch ex As Exception : _MsgBox.ErrorMessage(ex.Message, "Failed to register FedEx account...")
        End Try
    End Sub
    Private Function load_FedExEULA() As Boolean
        Dim eula As String = String.Empty
        '
        If _Files.IsFileExist(gDBpath + "\FEDEX_REG_EULA.txt", False) Then

            'Open gDBpath + "\FEDEX_REG_EULA.txt" For Input As #2
            If _Files.ReadFile_ToEnd(gDBpath + "\FEDEX_REG_EULA.txt", True, eula) Then
            End If

        ElseIf _Files.IsFileExist(gDBpath + "\FEDEX_REG_EULA.tmp", False) Then

            'Name gDBpath + "\FEDEX_REG_EULA.tmp" As gDBpath + "\FEDEX_REG_EULA.txt"
            'Open gDBpath + "\FEDEX_REG_EULA.txt" For Input As #2
            If _Files.CopyFile_ToNewFolder(gDBpath + "\FEDEX_REG_EULA.tmp", gDBpath + "\FEDEX_REG_EULA.txt", True) Then
                If _Files.ReadFile_ToEnd(gDBpath + "\FEDEX_REG_EULA.txt", True, eula) Then
                End If
            End If

        End If
        '
        Me.FedEx_EULA_TextBox.Text = eula
        load_FedExEULA = Not String.IsNullOrEmpty(eula)
    End Function
    Private Sub FedEx_DeclineAgreement_Btn_Click(sender As Object, e As RoutedEventArgs) Handles FedEx_DeclineAgreement_Btn.Click
        FedExAgreement_Border.Visibility = Visibility.Collapsed
        Me.FedEx_FreightBox_Register_Btn.IsEnabled = True : Me.FedEx_Register_Btn.IsEnabled = True
    End Sub

    Private Sub FedEx_ReadAgreement_CheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles FedEx_ReadAgreement_CheckBox.Checked
        FedEx_AcceptAgreement_Btn.IsEnabled = True
    End Sub

    Private Sub FedEx_ReadAgreement_CheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles FedEx_ReadAgreement_CheckBox.Unchecked
        FedEx_AcceptAgreement_Btn.IsEnabled = False
    End Sub

    Private Sub FedEx_AcceptAgreement_Btn_Click(sender As Object, e As RoutedEventArgs) Handles FedEx_AcceptAgreement_Btn.Click
        Try

            If GetPolicyData(gShipriteDB, "FedExREST_Enabled", "False") Then
                FedEx_TC.SelectedIndex = 1
                Exit Sub
            End If

            Dim userKey As String = String.Empty
            Dim userPass As String = String.Empty
            Dim userMeterNumber As String = String.Empty
            Dim userPostalCode As String = String.Empty
            Dim reply As String = String.Empty
            '
            If _MsgBox.QuestionMessage("FedEx User Account registration procedure consists of 3 steps:" & vbCr & vbCr &
                            "1. obtain user Key and Password" & vbCr &
                            "2. obtain user Meter number" & vbCr &
                            "3. register your copy of ShipRite" & vbCr & vbCr &
                            "Would you like ShipRite to register your FedEx Account?", ShipRite) Then
                '
                If Not Me.FedEx_FreightBox_Register_Btn.IsEnabled Then
                    ' freight
                    _StoreOwner.StoreOwner.AccountNumber = Me.FedEx_AccountNumber_FreightBox_TextBox.Text
                    _FedExWeb.objFedEx_Setup = _FedExWeb.objFedEx_Freight_Setup
                Else
                    _StoreOwner.StoreOwner.AccountNumber = Me.FedEx_AccountNumber_TextBox.Text
                    _FedExWeb.objFedEx_Setup = _FedExWeb.objFedEx_Regular_Setup
                    _FedExWeb.objFedEx_Regular_Setup.Csp_AccountNumber = Me.FedEx_AccountNumber_TextBox.Text
                End If
                '
                If _FedExWeb.Process_RegisterCSPUser_Request(_StoreOwner.StoreOwner, userKey, userPass, reply) Then
                    '
                    _FedExWeb.objFedEx_Setup.Web_UserCredential_Key = userKey
                    _FedExWeb.objFedEx_Setup.Web_UserCredential_Pass = userPass
                    If Not Me.FedEx_FreightBox_Register_Btn.IsEnabled Then
                        ' freight
                        Call General.UpdatePolicy(gShipriteDB, "FedExFreightBox_UserName", userKey)
                        Call General.UpdatePolicy(gShipriteDB, "FedExFreightBox_Password", userPass)
                    Else
                        Call General.UpdatePolicy(gShipriteDB, "FedExIOPort", userKey)
                        Call General.UpdatePolicy(gShipriteDB, "FedExPassword", userPass)
                    End If
                    _MsgBox.InformationMessage("FedEx Confirmation:" & vbCr & vbCr & "User Key = " & userKey & vbCr & "User Pass = " & userPass & vbCr & vbCr & reply, _FedExWeb.WebServTitle)
                    '
                    ''ol#9.264(10/21)... For Puerto Rico shipper to register with FedEx Web successfully the country code must be 'US'.
                    If "PR" = _StoreOwner.StoreOwner.CountryCode Then
                        _StoreOwner.StoreOwner.CountryCode = "US"
                    End If
                    '
                    If _FedExWeb.Process_Subscription_Request(_StoreOwner.StoreOwner, userMeterNumber, reply) Then
                        '
                        _FedExWeb.objFedEx_Setup.Client_MeterNumber = userMeterNumber
                        If Not Me.FedEx_FreightBox_Register_Btn.IsEnabled Then
                            ' freight
                            Call General.UpdatePolicy(gShipriteDB, "FedExFreightBox_MeterNumber", userMeterNumber)
                        Else
                            Call General.UpdatePolicy(gShipriteDB, "FedExMeter", userMeterNumber)
                        End If
                        _MsgBox.InformationMessage("FedEx Confirmation:" & vbCr & vbCr & "Meter No = " & userMeterNumber & vbCr & vbCr & reply, _FedExWeb.WebServTitle)
                        userPostalCode = _StoreOwner.StoreOwner.Zip
                        '
                        If _FedExWeb.Process_VersionCapture_Request(userPostalCode, reply) Then
                            '
                            _MsgBox.InformationMessage("FedEx Confirmation:" & vbCr & vbCr & "Version Captured Successfully" & vbCr & reply, _FedExWeb.WebServTitle)
                            '
                            Call General.UpdatePolicy(gShipriteDB, "FedExOnline", "True")
                            If Not Me.FedEx_FreightBox_Register_Btn.IsEnabled Then
                                ' freight
                                Call General.UpdatePolicy(gShipriteDB, "FedExFreightBox_AccountNumber", _FedExWeb.objFedEx_Setup.Csp_AccountNumber)
                            Else
                                Call General.UpdatePolicy(gShipriteDB, "FedExAccountNumber", _FedExWeb.objFedEx_Setup.Csp_AccountNumber)
                            End If
                            '
                        Else
                            _MsgBox.WarningMessage("Failed to send FedEx the ShipRite Version Capture..." & vbCr & reply, _FedExWeb.WebServTitle)
                        End If
                        '
                    Else
                        _MsgBox.WarningMessage("Failed to obtain FedEx User Meter number..." & vbCr & reply, _FedExWeb.WebServTitle)
                    End If
                    '
                Else
                    _MsgBox.WarningMessage("Failed to obtain FedEx User Key and Password..." & vbCr & reply, _FedExWeb.WebServTitle)
                End If
                '
            End If
            '
            FedExAgreement_Border.Visibility = Visibility.Collapsed
            '
        Catch ex As Exception : _MsgBox.ErrorMessage(ex.Message, "Failed to register FedEx account...")
        Finally : Me.FedEx_FreightBox_Register_Btn.IsEnabled = True : Me.FedEx_Register_Btn.IsEnabled = True
        End Try
    End Sub

#End Region

#Region "UPS"


    Private useLicenseKey As String = String.Empty

    Private Sub UPSReady_Register_Btn_Click(sender As Object, e As RoutedEventArgs) Handles UPSReady_Register_Btn.Click
        Try
            Dim userAgreementText As String = String.Empty
            UPS_ReadAgreement_CheckBox.IsChecked = False
            If _UPSWeb.Get_LicenseAgreementText(userAgreementText) Then
                Me.UPS_UserAgreement_TextBox.Text = userAgreementText.Replace(Convert.ToChar(10), vbCr_)
                UPSReady_Border.Visibility = Visibility.Visible
                UPSReady_TabControl.SelectedIndex = 0 ' User Agreement tab
            End If
        Catch ex As Exception : _MsgBox.ErrorMessage(ex.Message, "Failed to register UPS account...")
        End Try
    End Sub

    Private Sub UPS_ReadAgreement_CheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles UPS_ReadAgreement_CheckBox.Checked
        Me.UPS_AcceptAgreement_Btn.IsEnabled = True
        Me.UPS_DeclineAgreement_Btn.IsEnabled = True
    End Sub
    Private Sub UPS_ReadAgreement_CheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles UPS_ReadAgreement_CheckBox.Unchecked
        Me.UPS_AcceptAgreement_Btn.IsEnabled = False
        Me.UPS_DeclineAgreement_Btn.IsEnabled = False
    End Sub

    Private Sub UPS_AcceptAgreement_Btn_Click(sender As Object, e As RoutedEventArgs) Handles UPS_AcceptAgreement_Btn.Click
        _UPSWeb.IsUserAgreementAccepted = Me.UPS_ReadAgreement_CheckBox.IsChecked
        If _UPSWeb.IsUserAgreementAccepted Then
            ''
            _UPSWeb.objUPS_Setup.ShipRite_Username = Me.UPSReady_UserID_TextBox.Text
            _UPSWeb.objUPS_Setup.ShipRite_Password = Me.UPSReady_Password_TextBox.Password
            _UPSWeb.objUPS_Setup.ShipRite_ShipperNumber = Me.UPSReady_AccountNumber_TextBox.Text
            _StoreOwner.StoreOwner.AccountNumber = _UPSWeb.objUPS_Setup.ShipRite_ShipperNumber
            ''
            If _UPSWeb.Agree_ToGetLicenseNumber(_StoreOwner.StoreOwner, Me.UPS_UserAgreement_TextBox.Text, useLicenseKey) Then
                _UPSWeb.objUPS_Setup.ShipRite_AccessLicenseNumber = useLicenseKey
                UPSReady_TabControl.SelectedIndex = 1 ' User Invoice tab
            End If
        End If
    End Sub
    Private Sub UPS_DeclineAgreement_Btn_Click(sender As Object, e As RoutedEventArgs) Handles UPS_DeclineAgreement_Btn.Click
        _UPSWeb.IsUserAgreementAccepted = False
        UPSReady_Border.Visibility = Visibility.Collapsed
    End Sub
    Private Sub UPS_Cancel_Btn_Click(sender As Object, e As RoutedEventArgs) Handles UPS_Cancel_Btn.Click
        UPSReady_Border.Visibility = Visibility.Collapsed
    End Sub
    Private Sub UPS_PrintAgreement_Btn_Click(sender As Object, e As RoutedEventArgs) Handles UPS_PrintAgreement_Btn.Click
        Try
            'Dim tp As New _Printer._PrintText(Me.txtAgreement.Text)
            'tp.Print()
        Catch ex As Exception : _MsgBox.ErrorMessage(ex.Message, "Failed to print UPS Agreement...")
        End Try
    End Sub

    Private Sub UPS_Finish_Btn_Click(sender As Object, e As RoutedEventArgs) Handles UPS_Finish_Btn.Click
        If _UPSWeb.Finish_Registration(_UPSWeb.objUPS_Setup, _StoreOwner.StoreOwner) Then
            MessageBox.Show("Registered Successfully!", "Success!", MessageBoxButton.OK, MessageBoxImage.Information)
        End If
        UPSReady_Border.Visibility = Visibility.Collapsed
    End Sub


#End Region

#Region "USPS"

    Private Sub Endicia_NetStamp_ChkBx_Checked(sender As Object, e As RoutedEventArgs) Handles Endicia_NetStamp_ChkBx.Checked
        Endicia_NetStamp_Acct_TxtBx.Text = Endicia_AccountNo_TxtBox.Text
        Endicia_NetStamp_Passphrase_TxtBx.Text = Endicia_PassPhrase_TxtBox.Password
    End Sub

    Private Sub Endicia_NetStamp_TxtBx_TextChanged(sender As Object, e As TextChangedEventArgs) Handles Endicia_NetStamp_Acct_TxtBx.TextChanged, Endicia_NetStamp_Passphrase_TxtBx.TextChanged
        Endicia_NetStamp_ChkBx.IsChecked = False
    End Sub

    Private Sub Endicia_Register_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Endicia_Register_Btn.Click
        Try

            UpdatePolicy(gShipriteDB, “ABButton3”, Me.Endicia_AccountNo_TxtBox.Text)
            UpdatePolicy(gShipriteDB, “ABButton4”, Me.Endicia_PassPhrase_TxtBox.Password)
            _EndiciaWeb.objEndiciaCredentials.AccountID = Me.Endicia_AccountNo_TxtBox.Text
            _EndiciaWeb.objEndiciaCredentials.PassPhrase = Me.Endicia_PassPhrase_TxtBox.Password
            '
            If Endicia_GetAccountStatus() Then
                MessageBox.Show("Your Endicia Account was Registered Successfully!", EndiciaLavelServer, MessageBoxButton.OK, MessageBoxImage.Information)


                Endicia_Border.Visibility = Visibility.Visible
                Endicia_LocationPhrase_Grid.Visibility = Visibility.Visible
                EndiciaHeader_Lbl.Content = "Endicia Label Server Options"

                If 0 = _EndiciaWeb._PickupLocations.Count Then
                    _EndiciaWeb.Load_PickupLocationList()
                End If

                If load_PickupLocations() Then
                    Me.Endicia_SetPickUpLocation_ComboBox.Text = General.GetPolicyData(gShipriteDB, _ReusedField.fldPickUpLocation)
                    Me.Endicia_SpecialInstructions_TextBox.Text = General.GetPolicyData(gShipriteDB, _ReusedField.fldPickUpInstructions)
                End If

            End If
        Catch ex As Exception : _MsgBox.ErrorMessage(ex.Message, "Failed to register Endicia account...") : Me.Endicia_Register_Btn.IsEnabled = True
        End Try
    End Sub

    Private Sub Endicia_NetStamp_Register_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Endicia_NetStamp_Register_Btn.Click

        Try

            UpdatePolicy(gReportsDB, "DYMO_ActivationCode", Endicia_NetStamp_SerialNo_TxtBx.Text)
            UpdatePolicy(gShipriteDB, "Endicia_AccountID2", Endicia_NetStamp_Acct_TxtBx.Text)
            UpdatePolicy(gShipriteDB, "Endicia_PassPhrase2", Endicia_NetStamp_Passphrase_TxtBx.Text)

            If Endicia_GetAccountStatus(True) Then
                MessageBox.Show("Your Endicia Account was Registered Successfully!", EndiciaLavelServer, MessageBoxButton.OK, MessageBoxImage.Information)
                Endicia_Border.Visibility = Visibility.Visible
                Endicia_LocationPhrase_Grid.Visibility = Visibility.Hidden
                EndiciaHeader_Lbl.Content = "NetStamp Account Options"

            End If
        Catch ex As Exception : _MsgBox.ErrorMessage(ex.Message, "Failed to register Endicia account...") : Me.Endicia_Register_Btn.IsEnabled = True
        End Try

    End Sub

    Private Function load_PickupLocations() As Boolean
        If 0 = Me.Endicia_SetPickUpLocation_ComboBox.Items.Count Then
            For i As Integer = 0 To _EndiciaWeb._PickupLocations.Count - 1
                Dim pickup As _EndiciaWeb._PickupLocation = _EndiciaWeb._PickupLocations.Item(i)
                Me.Endicia_SetPickUpLocation_ComboBox.Items.Add(pickup)
            Next i
        End If
        load_PickupLocations = (0 < Me.Endicia_SetPickUpLocation_ComboBox.Items.Count)
    End Function

    Private Function Endicia_GetAccountStatus(Optional isDYMO As Boolean = False) As Boolean
        Endicia_GetAccountStatus = False
        Dim SetupInfo As New _EndiciaSetup

        If isDYMO Then
            set_SRSetupDYMO(SetupInfo)
        Else
            SetupInfo = _EndiciaWeb.objEndiciaCredentials
        End If

        '
        If SetupInfo.IsEnabled Then
            '
            Dim response As New Endicia_LabelService.AccountStatusResponse
            If _EndiciaWeb.Request_AccountStatus(response, isDYMO) Then
                If response.CertifiedIntermediary IsNot Nothing Then
                    Me.Endicia_CurrentBalance_TextBox.Text = response.CertifiedIntermediary.PostageBalance.ToString("C")
                    Me.Endicia_TotalPostagePrinted_TextBox.Text = response.CertifiedIntermediary.AscendingBalance.ToString("C")
                    Me.Endicia_DeviceID_TextBox.Text = response.CertifiedIntermediary.DeviceID
                    If response.CertifiedIntermediary.AccountStatus = "A" Then
                        Me.Endicia_AccountStatus_TextBox.Text = "Active"
                    Else
                        Me.Endicia_AccountStatus_TextBox.Text = "Inactive"
                    End If
                End If
                '
                Return True
            End If
        End If
        '
    End Function

    Private Sub Endicia_AccountNo_TxtBox_TextChanged(sender As Object, e As TextChangedEventArgs) Handles Endicia_AccountNo_TxtBox.TextChanged
        Me.Endicia_Register_Btn.IsEnabled = True
    End Sub
    Private Sub Endicia_PassPhrase_TxtBox_PasswordChanged(sender As Object, e As RoutedEventArgs) Handles Endicia_PassPhrase_TxtBox.PasswordChanged
        Me.Endicia_Register_Btn.IsEnabled = True
    End Sub

    Private Sub Endicia_SaveLocation_Button_Click(sender As Object, e As RoutedEventArgs) Handles Endicia_SaveLocation_Button.Click
        Try
            '
            Me.Endicia_SaveLocation_Button.IsEnabled = False
            If save_PickupLocation() Then
                MessageBox.Show("New Pickup Location was changed Successfully!", ShipRite, MessageBoxButton.OK, MessageBoxImage.Information)
            End If
        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to save new Pickup Location...")
        Finally : Me.Endicia_SaveLocation_Button.IsEnabled = True
        End Try
    End Sub
    Private Function save_PickupLocation() As Boolean
        save_PickupLocation = False
        If Not String.IsNullOrEmpty(Me.Endicia_SetPickUpLocation_ComboBox.Text) Then
            UpdatePolicy(gShipriteDB, “ABButton1”, Me.Endicia_SetPickUpLocation_ComboBox.Text)
            UpdatePolicy(gShipriteDB, “ABButton2”, Me.Endicia_SpecialInstructions_TextBox.Text)
            save_PickupLocation = True
        End If
    End Function

    Private Sub Endicia_BuyPostage_Button_Click(sender As Object, e As RoutedEventArgs) Handles Endicia_BuyPostage_Button.Click
        Try
            Me.Endicia_BuyPostage_Button.IsEnabled = False
            Dim isDYMO As Boolean = False
            If Endicia_LocationPhrase_Grid.Visibility = Visibility.Hidden Then
                isDYMO = True
            End If
            '
            Dim request As New Endicia_LabelService.RecreditRequest
            Dim response As New Endicia_LabelService.RecreditRequestResponse
            '
            My.Settings.Endicia_LastPostageAmountAdded = Val(Me.Endicia_BuyPostage_TextBox.Text)
            request.RecreditAmount = My.Settings.Endicia_LastPostageAmountAdded.ToString ' get from the form.
            If _EndiciaWeb.Request_Recredit(request, response, isDYMO) Then
                If response.CertifiedIntermediary IsNot Nothing Then
                    Me.Endicia_CurrentBalance_TextBox.Text = response.CertifiedIntermediary.PostageBalance.ToString("C")
                    Me.Endicia_TotalPostagePrinted_TextBox.Text = response.CertifiedIntermediary.AscendingBalance.ToString("C")
                End If
                MessageBox.Show("Your account was recredited Successfully!", EndiciaLavelServer, MessageBoxButton.OK, MessageBoxImage.Information)
            End If
            '
        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to add money to your account...")
        Finally : Me.Endicia_BuyPostage_Button.IsEnabled = True
        End Try
    End Sub

    Private Sub Endicia_ChangePassPhrase_Button_Click(sender As Object, e As RoutedEventArgs) Handles Endicia_ChangePassPhrase_Button.Click
        Try
            Dim request As New Endicia_LabelService.ChangePassPhraseRequest
            request.CertifiedIntermediary = New Endicia_LabelService.CertifiedIntermediary
            request.CertifiedIntermediary.PassPhrase = Me.Endicia_Enter_OldPassPhrase_TextBox.Text ' get from the form.
            request.NewPassPhrase = Me.Endicia_Enter_NewPassPhrase_TextBox.Text
            '
            If _EndiciaWeb.Request_ChangePassPhrase(request) Then
                objEndiciaCredentials.PassPhrase = request.NewPassPhrase
                General.UpdatePolicy(gShipriteDB, _ReusedField.fldPassPhrase, request.NewPassPhrase)
                _MsgBox.InformationMessage("New Pass Phrase was changed Successfully!", EndiciaLavelServer)
            End If
        Catch ex As Exception : _MsgBox.ErrorMessage(ex.Message, "Failed to change Pass Phase for Your Endicia account...") : Me.Endicia_Register_Btn.IsEnabled = True
        End Try
    End Sub

#End Region

    Private Sub Save_CarrierSetup()
        Try
            Dim selectedCarrierName As String = GetSelectedCarrierName()

            Select Case selectedCarrierName
                Case "FEDEX"
                    '
                    General.UpdatePolicy(gShipriteDB, "FedExAccountNumber", Me.FedEx_AccountNumber_TextBox.Text)
                    General.UpdatePolicy(gShipriteDB, "FedExHAL_LocationID", Me.FedEx_HAL_LocationID_TextBox.Text)
                    General.UpdatePolicy(gShipriteDB, "FedExHAL_AgentID", Me.FedEx_HAL_AgentID_TextBox.Text)
                    General.UpdatePolicy(gShipriteDB, "Enable_RetailFedExLevel", isFASC_CheckBox.IsChecked)
                    General.UpdatePolicy(gShipriteDB, "RetailFedExLevel", FedEx_FASC_DiscountLevel_ComboBox.SelectedItem)
                    General.UpdatePolicy(gShipriteDB, "Disable_FedEx_EmailShipNotifications", FedEx_DisableEmailNotification_ChkBx.IsChecked)
                    General.UpdatePolicy(gShipriteDB, "IsFedExMarkupDiscount", FedEx_MarkupDiscount_CheckBox.IsChecked)
                    General.UpdatePolicy(gShipriteDB, "AlwaysChargeFedExRetail", FedEx_AlwaysChargeRetail_CheckBox.IsChecked)
                    General.UpdatePolicy(gShipriteDB, "FedExRETURN_LocationID", FedEx_FRT_LocationID_TextBox.Text)

                    ' refresh setup object
                    gFedExReturnsSETUP.LocationID = FedEx_FRT_LocationID_TextBox.Text
                    _FedExWeb.objFedEx_Regular_Setup.OriginLocationId = FedEx_HAL_LocationID_TextBox.Text
                    _FedExWeb.objFedEx_Regular_Setup.ApplicationId = FedEx_HAL_AgentID_TextBox.Text

                    If FedEx_PrintLaserLabels_CheckBox.IsChecked = True Then
                        General.UpdatePolicy(gReportsDB, "FedExLabelType", "Laser")
                    Else
                        General.UpdatePolicy(gReportsDB, "FedExLabelType", "Thermal")
                    End If

                    '
                    _MsgBox.SavedSuccessfully("FedEx Carrier Settings")
                    '
                Case "UPS"
                    '
                    UPS_SaveAuthorizationInfo()
                    General.UpdatePolicy(gShipriteDB, "Enable_RetailUPSLevel", isASO_CheckBox.IsChecked)
                    General.UpdatePolicy(gShipriteDB, "Disable_UPS_EmailShipNotifications", UPS_DisableEmailNotification_ChkBx.IsChecked)
                    General.UpdatePolicy(gShipriteDB, "UPSLevel", UPSDiscountLevel_ComboBox.SelectedItem)
                    General.UpdatePolicy(gShipriteDB, "IsUPSMarkupDiscount", UPS_MarkupDiscount_CheckBox.IsChecked)
                    General.UpdatePolicy(gShipriteDB, "AlwaysChargeUPSRetail", UPS_AlwaysChargeRetail_CheckBox.IsChecked)
                    General.UpdatePolicy(gShipriteDB, "UPS_AccessID", UPS_DropOff_AccessID_TxtBx.Text)
                    '
                    _MsgBox.SavedSuccessfully("UPS Carrier Settings")
                    '
                Case "DHL"
                    '
                    General.UpdatePolicy(gShipriteDB, _ReusedField.fldDHL_ShipperID, Me.DHLAccountNo_TxtBox.Text)
                    '
                    ' refresh setup object
                    _Dhl_XML.objDHL_Setup.ShipperAccountNumber = Me.DHLAccountNo_TxtBox.Text

                    Dim SaveDHLDiscount As String = "n/a"

                    Select Case DHL_RateType_ComboBox.SelectedIndex
                        Case 0 : SaveDHLDiscount = "n/a"
                        Case 1 : SaveDHLDiscount = "TIER1"
                        Case 2 : SaveDHLDiscount = "TIER2"
                        Case 3 : SaveDHLDiscount = "TOPTIER"

                    End Select

                    General.UpdatePolicy(gShipriteDB, "DHL_INTL_RATETABLE", SaveDHLDiscount)
                    General.UpdatePolicy(gShipriteDB, "IsDHLMarkupDiscount", DHL_MarkupDiscount_CheckBox.IsChecked)
                    General.UpdatePolicy(gShipriteDB, "AlwaysChargeDhlRetail", DHL_AlwaysChargeRetail_CheckBox.IsChecked)

                    _MsgBox.SavedSuccessfully("DHL Carrier Settings")
                    '
                Case "USPS"
                    '
                    General.UpdatePolicy(gShipriteDB, _ReusedField.fldAccountID, Me.Endicia_AccountNo_TxtBox.Text)
                    General.UpdatePolicy(gShipriteDB, _ReusedField.fldPassPhrase, Me.Endicia_PassPhrase_TxtBox.Password)
                    My.Settings.Endicia_LastPostageAmountAdded = Val(Me.Endicia_BuyPostage_TextBox.Text)
                    If Endicia_Border.Visibility = Visibility.Visible Then
                        General.UpdatePolicy(gShipriteDB, _ReusedField.fldPickUpLocation, Me.Endicia_SetPickUpLocation_ComboBox.Text)
                        General.UpdatePolicy(gShipriteDB, _ReusedField.fldPickUpInstructions, Me.Endicia_SpecialInstructions_TextBox.Text)
                    End If
                    '
                    ' refresh object
                    _EndiciaWeb.objEndiciaCredentials.AccountID = Me.Endicia_AccountNo_TxtBox.Text
                    _EndiciaWeb.objEndiciaCredentials.PassPhrase = Me.Endicia_PassPhrase_TxtBox.Password


                    UpdatePolicy(gShipriteDB, "Enable_USPSCubicRate", USPS_CubicRates_ChkBx.IsChecked)

                    'SRPRO rates discontinued by Endicia 8/12/2024
                    'UpdatePolicy(gShipriteDB, "ENABLE_USPS_SRPRO_Rate", USPS_SRPRO_Rates_ChkBx.IsChecked)

                    UpdatePolicy(gShipriteDB, "Enable_USPS_ApprovedShipper", USPS_ApprovedShipper_ChkBx.IsChecked)
                    UpdatePolicy(gShipriteDB, "Enable_USPSCubicRate_ParcelSelect", USPS_GroundADV_CubicRates_ChkBx.IsChecked)

                    UpdatePolicy(gReportsDB, "EndiciaLabelType", USPS_LabelType_CmbBx.SelectedValue.ToString)

                    'UpdatePolicy(gReportsDB, "DYMO_ActivationCode", Endicia_NetStamp_SerialNo_TxtBx.Text)
                    'UpdatePolicy(gShipriteDB, "Endicia_AccountID2", Endicia_NetStamp_Acct_TxtBx.Text)
                    'UpdatePolicy(gShipriteDB, "Endicia_PassPhrase2", Endicia_NetStamp_Passphrase_TxtBx.Text)
                    '
                    _MsgBox.SavedSuccessfully("Endicia Carrier Settings")

                Case "SPEE-DEE"
                    UpdatePolicy(gShipriteDB, "SpeeDeeAccountNumber", SpeeDee_AccountNumber_TxtBx.Text)
                    _MsgBox.SavedSuccessfully("SpeeDee Carrier Settings")
                    '
            End Select
            '
        Catch ex As Exception : _MsgBox.ErrorMessage(ex.Message, "Failed to save Carrier Settings...")
        End Try
    End Sub

#Region "FedEx REST Integration"
    Private Sub FedExREST_Register_Btn_Click(sender As Object, e As RoutedEventArgs) Handles FedExREST_Register_Btn.Click

        Dim InvoiceDate As String = Format(FedExREST_InvoiceDate_TxtBx.SelectedDate, "yyyy-MM-dd")

        gFedExSETUP.AccountNumber = FedEx_AccountNumber_TextBox.Text
        'Test Account# 700257037

        gFedExSETUP.invoiceDetail = New invoiceDetail With {
        .number = Val(FedExREST_InvoiceNo_TxtBx.Text),
        .currency = "USD",
        .[date] = InvoiceDate,
        .amount = CDbl(FedExREST_InvoiceAmt_TxtBx.Text)
        }


        If Get_OAuth_Token(True) Then
            If Get_Customer_Key() Then
                If VerifyInvoice() Then
                    UpdatePolicy(gShipriteDB, "FedExAccountNumber", gFedExSETUP.AccountNumber)
                End If
            End If
        End If
    End Sub

    Private Sub FedExREST_RegisterPIN_Btn_Click(sender As Object, e As RoutedEventArgs) Handles FedExREST_RegisterPIN_Btn.Click
        gFedExSETUP.AccountNumber = FedEx_AccountNumber_TextBox.Text
        'Test Account# 700257037

        If FedExREST_PIN_TxtBx.Text <> "" Then
            If Get_OAuth_Token(True) Then
                If Get_Customer_Key() Then
                    If Register_PIN(FedExREST_PIN_TxtBx.Text) Then
                        UpdatePolicy(gShipriteDB, "FedExAccountNumber", gFedExSETUP.AccountNumber)
                    End If
                End If
            End If

        Else
            MsgBox("Please Enter a valid PIN first")
        End If
    End Sub

    Private Sub FedExREST_RequestPin_Btn_Click(sender As Object, e As RoutedEventArgs) Handles FedExREST_RequestPin_Btn.Click
        Dim isSuccess As Boolean = False
        gFedExSETUP.AccountNumber = FedEx_AccountNumber_TextBox.Text


        If Get_OAuth_Token(True) Then
            If Get_Customer_Key() Then
                If FedExREST_SMS_Radio.IsChecked Then
                    isSuccess = Request_PIN("SMS")
                ElseIf FedExREST_Phone_Radio.IsChecked Then
                    isSuccess = Request_PIN("CALL")
                ElseIf FedExREST_Email_Radio.IsChecked Then
                    isSuccess = Request_PIN("EMAIL")
                End If

                If isSuccess Then
                    UpdatePolicy(gShipriteDB, "FedExAccountNumber", gFedExSETUP.AccountNumber)
                End If

            End If
        End If

    End Sub

    Private Sub FedExREST_RegisterTechSupport_Btn_Click(sender As Object, e As RoutedEventArgs) Handles FedExREST_RegisterTechSupport_Btn.Click
        gFedExSETUP.AccountNumber = FedEx_AccountNumber_TextBox.Text

        If Get_OAuth_Token(True) Then
            If Get_Customer_Key(True) Then
                UpdatePolicy(gShipriteDB, "FedExAccountNumber", gFedExSETUP.AccountNumber)
                MsgBox("FedEx account successfully registered!", vbInformation, "Success!")
            Else
                MsgBox("FedEx account was NOT registered. FedEx server did not return registration credentials!", vbExclamation, "Error!")
            End If
        End If
    End Sub

    Private Sub UPS_SaveAuthorizationInfo()

        If UPS_Rest.Api.Authentication.AuthenticationService.IsEnabled Then
            _UPSWeb.objUPS_Setup.ShipRite_ShipperNumber = Me.UPSREST_AccountNumber_TextBox.Text
            If String.IsNullOrWhiteSpace(_UPSWeb.objUPS_Setup.ShipRite_ShipperNumber) Then
                ' UPS Account Number cleared -> Clear saved OAuth
                UPS_Rest.Api.Authentication.AuthenticationService.ClearTokenInDatabase()
            End If
        Else
            _UPSWeb.objUPS_Setup.ShipRite_ShipperNumber = Me.UPSReady_AccountNumber_TextBox.Text
            _UPSWeb.objUPS_Setup.ShipRite_Username = Me.UPSReady_UserID_TextBox.Text
            _UPSWeb.objUPS_Setup.ShipRite_Password = Me.UPSReady_Password_TextBox.Password

            General.UpdatePolicy(gShipriteDB, _ReusedField.fldUPSWeb_UserLicense, _UPSWeb.objUPS_Setup.ShipRite_AccessLicenseNumber)
            General.UpdatePolicy(gShipriteDB, _ReusedField.fldUPSWeb_UserID, _UPSWeb.objUPS_Setup.ShipRite_Username)
            General.UpdatePolicy(gShipriteDB, _ReusedField.fldUPSWeb_UserPassword, _UPSWeb.objUPS_Setup.ShipRite_Password)
        End If
        General.UpdatePolicy(gShipriteDB, _ReusedField.fldUPSWeb_UPSAccount, _UPSWeb.objUPS_Setup.ShipRite_ShipperNumber)

    End Sub

    Private Sub UPSREST_Authorize_Btn_Click(sender As Object, e As RoutedEventArgs) Handles UPSREST_Authorize_Btn.Click

        UPSREST_Authorize()

    End Sub

    Private Async Sub UPSREST_Authorize()

        Try
            If UPSREST_Authorize_Btn.Content = "Authorize UPS Account" Then
                ' authorize
                UPSREST_Authorize_Btn.Content = "CANCEL Authorize Request"
                UPSREST_Authorize_Btn.Background = New SolidColorBrush(Colors.Crimson)
                '
                Dim success As Boolean = Await UPS_Rest.Api.Authentication.AuthenticationService.RequestAuthorizationAsync()
                If success Then
                    MessageBox.Show("UPS Authorization Processed Successfully!", "Success!", MessageBoxButton.OK, MessageBoxImage.Information)
                    UPS_SaveAuthorizationInfo()
                End If
            Else
                ' cancel
                UPS_Rest.Api.Authentication.AuthenticationService.CancelAuthorizationRequest()
            End If
        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Failed to Authorize UPS Account...")
        Finally
            If Not UPSREST_Authorize_Btn.Content = "Authorize UPS Account" Then
                UPSREST_Authorize_Btn.Content = "Authorize UPS Account"
                UPSREST_Authorize_Btn.Background = New BrushConverter().ConvertFrom("#FF6876AB")
            End If
        End Try
    End Sub

    Private Sub CarrierSetup_Unloaded(sender As Object, e As RoutedEventArgs) Handles Me.Unloaded
        UPS_Rest.Api.Authentication.AuthenticationService.CancelAuthorizationRequest()
    End Sub

    Private Sub Add_SpeeDee_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Add_SpeeDee_Btn.Click
        If vbYes = MsgBox("Do you want to add Spee-Dee service to your ShipRite program?", vbYesNo + vbQuestion) Then
            If SpeeDee.Add_SpeeDee_Services() Then
                Add_SpeeDee_Btn.Visibility = Visibility.Hidden
                SpeeDee_AccountNumber_TxtBx.Visibility = Visibility.Visible
                SpeeDee_AccountNumber_TxtBx.Visibility = Visibility.Visible
            End If
        End If
    End Sub

    Private Sub FedExREST_InvoiceNo_TxtBx_PreviewTextInput(sender As Object, e As TextCompositionEventArgs) Handles FedExREST_InvoiceNo_TxtBx.PreviewTextInput
        Dim allowedchars As String = "0123456789"
        If allowedchars.IndexOf(CChar(e.Text)) > -1 Then
            e.Handled = False
        Else
            e.Handled = True
        End If
    End Sub


#End Region
#End Region
End Class

