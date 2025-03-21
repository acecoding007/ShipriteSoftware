Public Class MailMaster
    Inherits CommonWindow

    Dim ReturnReceiptTracking As String

    Private Class DetailListItem
        Public Property Header As String
        Public Property PicturePath As String
        Public Property Desc As String
        Public Property Tag As String
        Public Property Sell As Double
        Public Property Cost As Double
        Public Property Service As String
        Public Property OrderNo As Integer
    End Class

    Private Class Additional_Service
        Public Property ID As Integer
        Public Property Header As String
        Public Property PicturePath As String
        Public Property Sell As Double
        Public Property Cost As Double
        Public Property SellField As String
        Public Property CostField As String
        Public Property Visible As Visibility
    End Class

    Private Class TotalPostage
        Public Property Cost As Double
        Public Property Sell As Double
        Public Property Service As String
        Public Property ShippingCost As Double
        Public Property ShippingSell As Double
    End Class

    Public Class TotalLineItem
        Public Property SKU As String
        Public Property Desc As String
        Public Property Sell As Double
        Public Property Cost As Double
        Public Property Qty As Double
        Public Property ExtPrice As Double
        Public Property Service As String
    End Class

    Dim PostCard_OptionList As List(Of DetailListItem)
    Dim Letter_OptionList As List(Of DetailListItem)
    Dim Flat_OptionList As List(Of DetailListItem)
    Dim Package_OptionList As List(Of DetailListItem)
    Dim Service_Btn_DOM_List As List(Of DetailListItem)
    Dim Service_Btn_INTL_List As List(Of DetailListItem)
    Dim Additional_Services_List As List(Of Additional_Service)
    Dim TotalLine_list As List(Of TotalLineItem)

    Dim LetterImagee As String = "resources/MailMasterIcons/envelope.png"
    Dim StampImage As String = "resources/MailMasterIcons/stamp.png"
    Dim parcelImage As String = "resources/MailMasterIcons/package.png"
    Dim PostCardImage As String = "resources/MailMasterIcons/postcard.png"

    Dim Total As TotalPostage
    Dim DYMO_Setup As _EndiciaSetup


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

    Private Sub MailMaster_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded

        Country_ComboBox.Items.Add("United States")
        For Each ctry As _CountryDB In gCountry
            Country_ComboBox.Items.Add(ctry)
        Next
        Country_ComboBox.SelectedIndex = 0

        PostCard_OptionList = New List(Of DetailListItem)
        Letter_OptionList = New List(Of DetailListItem)
        Flat_OptionList = New List(Of DetailListItem)
        Package_OptionList = New List(Of DetailListItem)
        Additional_Services_List = New List(Of Additional_Service)
        Service_Btn_DOM_List = New List(Of DetailListItem)
        Service_Btn_INTL_List = New List(Of DetailListItem)
        TotalLine_list = New List(Of TotalLineItem)
        Total = New TotalPostage

        '--------------------------

        'Hide Custom Postage Options
        DisplayCustomPostageOptions(Visibility.Hidden)
        Insurance_Lbl.Visibility = Visibility.Hidden
        Insurance_TxtBx.Visibility = Visibility.Hidden


        Load_PostCardOptions()
        Load_LetterOptions()
        Load_FlatOptions()
        Load_PackageOptions()

        Load_FlatRate_Options()
        Load_AdditionalServices()

        Load_Service_Buttons()


        SetDimensionsVisibility(Visibility.Hidden)
        ExistingPostageAmnt_TxtBox.Visibility = Visibility.Hidden
        ExistingPostage_Label.Visibility = Visibility.Hidden

        NetStamps_Serial_TxtBx.Text = GetPolicyData(gReportsDB, "DYMO_ActivationCode", "")
        set_SRSetupDYMO(DYMO_Setup)


    End Sub

#Region "Load"

    Private Sub Load_Service_Buttons()
        Dim item As DetailListItem
        Dim current_Segment As String
        Dim Service As String

        Buffer = IO_GetSegmentSet(gShipriteDB, "SELECT SERVICE, DESCRIPTION From Master WHERE Carrier='USPS' and Disabled=0")

        Do Until Buffer = ""
            current_Segment = GetNextSegmentFromSet(Buffer)
            Service = ExtractElementFromSegment("SERVICE", current_Segment)



            If Service = "FirstClass" Or Service = "USPS-PRI" Or Service = "USPS-EXPR" Or Service = "USPS-MEDIA" Then
                'Domestic Services
                item = New DetailListItem
                item.Header = ExtractElementFromSegment("DESCRIPTION", current_Segment).Replace("USPS", "")
                item.Service = ExtractElementFromSegment("SERVICE", current_Segment)
                item.Sell = 0

                Select Case Service
                    Case "FirstClass"
                        item.OrderNo = 0
                    Case "USPS-PRI"
                        item.OrderNo = 1
                    Case "USPS-EXPR"
                        item.OrderNo = 2
                    Case "USPS-MEDIA"
                        item.OrderNo = 3

                End Select

                Service_Btn_DOM_List.Add(item)


            ElseIf Service = "USPS-INTL-FCMI" Then
                'International Services
                item = New DetailListItem
                item.Header = ExtractElementFromSegment("DESCRIPTION", current_Segment)
                item.Service = ExtractElementFromSegment("SERVICE", current_Segment)
                item.Sell = 0
                Service_Btn_INTL_List.Add(item)

            End If
        Loop


        Service_Btn_DOM_List = Service_Btn_DOM_List.OrderBy(Function(x As DetailListItem) x.OrderNo).ToList
        Services_LB.ItemsSource = Service_Btn_DOM_List

    End Sub

    Private Sub Load_PackageOptions()

        Dim item As DetailListItem = New DetailListItem
        item.Header = "Parcel"
        item.PicturePath = parcelImage
        item.Desc = "Small Parcel"
        item.Sell = 0
        Package_OptionList.Add(item)

        item = New DetailListItem
        item.Header = "Large Parcel"
        item.PicturePath = parcelImage
        item.Desc = "Any side over 12"""
        item.Sell = 0
        Package_OptionList.Add(item)

    End Sub

    Private Sub Load_FlatOptions()
        Dim item As DetailListItem = New DetailListItem
        item.Header = "Flat"
        item.PicturePath = "resources/MailMasterIcons/flat.png"
        item.Desc = "Regular Flat"
        item.Sell = 0
        Flat_OptionList.Add(item)

    End Sub

    Private Sub Load_FlatRate_Options()

        Dim current_Segment As String
        Dim buffer As String
        Dim item As DetailListItem


        buffer = IO_GetSegmentSet(gPackagingDB, "SELECT PackagingItems.Disabled, CarrierPackagingFlatRateValues.CarrierID, Carriers.CarrierName, CarrierPackagingFlatRateValues.ServiceTypeID, CarrierServiceTypes.ServiceTypeName, CarrierPackagingFlatRateValues.SettingID, PackagingItems.SettingName, CarrierPackagingFlatRateValues.BaseCost, CarrierPackagingFlatRateValues.SellPrice, PackagingItems.SettingDesc, PackagingItems.Length, PackagingItems.Height, PackagingItems.Width, PackagingItems.MaxLBs
FROM PackagingItems INNER JOIN (Carriers INNER JOIN (CarrierServiceTypes INNER JOIN CarrierPackagingFlatRateValues ON CarrierServiceTypes.ServiceTypeID = CarrierPackagingFlatRateValues.ServiceTypeID) ON Carriers.CarrierID = CarrierPackagingFlatRateValues.CarrierID) ON PackagingItems.SettingID = CarrierPackagingFlatRateValues.SettingID
WHERE ((Carriers.Disabled)=False) and (CarrierServiceTypes.ServiceTypeName = 'Domestic')
ORDER BY PackagingItems.Disabled DESC , CarrierServiceTypes.OrderNo, PackagingItems.SettingOrderNo")

        Do Until buffer = ""
            current_Segment = GetNextSegmentFromSet(buffer)

            item = New DetailListItem
            item.Header = ExtractElementFromSegment("SettingName", current_Segment)
            item.Header = item.Header.Substring(10)


            item.Sell = ExtractElementFromSegment("SellPrice", current_Segment, "0")
            item.Cost = ExtractElementFromSegment("BaseCost", current_Segment, "0")

            If item.Header.Contains("Env") Then
                item.PicturePath = "resources/MailMasterIcons/flatrateEnv.png"
                item.Desc = "FlatRate Env"



                If item.Header.Contains("Exp") Then
                    item.Service = "USPS-EXPR"
                Else
                    item.Service = "USPS-PRI"
                End If

                Flat_OptionList.Add(item)
            Else
                item.PicturePath = "resources/MailMasterIcons/FlatRateBox.png"
                item.Desc = "FlatRate Package"
                item.Service = "USPS-PRI"
                Package_OptionList.Add(item)
            End If

        Loop

        Flat_OptionList = Flat_OptionList.OrderBy(Function(value As DetailListItem) value.Sell).ToList
        Package_OptionList = Package_OptionList.OrderBy(Function(value As DetailListItem) value.Sell).ToList

    End Sub

    Private Sub Load_AdditionalServices()


        Dim item As Additional_Service = New Additional_Service
        item.ID = 1
        item.Header = "Certified Mail"
        item.PicturePath = "resources/MailMasterIcons/USPS_Certified.jpg"
        item.CostField = "ACTSATPU2"
        item.SellField = "SATPU2"
        Additional_Services_List.Add(item)

        item = New Additional_Service
        item.ID = 2
        item.Header = "Return Receipt"
        item.PicturePath = "resources/MailMasterIcons/USPS_ReturnReceipt.jpg"
        item.CostField = "ACTSAT2"
        item.SellField = "SAT2"
        Additional_Services_List.Add(item)

        item = New Additional_Service
        item.ID = 3
        item.Header = "Signature Confirm."
        item.PicturePath = "resources/MailMasterIcons/USPS_SigConfirm.png"
        item.CostField = "ACTDELSIG"
        item.SellField = "ACK-S"
        Additional_Services_List.Add(item)

        item = New Additional_Service
        item.ID = 4
        item.Header = "USPS Insurance"
        item.PicturePath = "resources/MailMasterIcons/USPS_InsuredMail.jpg"
        Additional_Services_List.Add(item)


        Additional_Services_LB.ItemsSource = Additional_Services_List
    End Sub

    Private Sub Load_LetterOptions()

        Dim item As DetailListItem = New DetailListItem
        item.Header = "Letter"
        item.PicturePath = LetterImagee
        item.Desc = "Regular Letter"
        item.Sell = 0

        Letter_OptionList.Add(item)

        item = New DetailListItem
        item.Header = "Letter Non-Machinable"
        item.PicturePath = LetterImagee
        item.Desc = "Odd Shape, Rigid, Square"
        item.Sell = 0

        Letter_OptionList.Add(item)


        Load_FirstClass_Stamp_Pricing()

    End Sub

    Private Sub Load_FirstClass_Stamp_Pricing()

        Dim item As DetailListItem = New DetailListItem
        Dim current_segment As String
        Dim Buffer As String = ""
        Dim BufferRetail As String = ""
        Dim RetailSegment As String
        Dim costField As String = "COST-Letter"

        If _EndiciaWeb.EndiciaWeb_IsEnabled Then
            'DYMO NetStamps Enabled
            costField = "COMM-Letter"
        End If


        Buffer = IO_GetSegmentSet(gUSMailDB_Services, "Select [WEIGHT], [" & costField & "] From FirstClass WHERE [WEIGHT] < 4")
        BufferRetail = IO_GetSegmentSet(gPackagingDB, "Select [WEIGHT], [Retail-Letter] From FirstClassRetail WHERE [WEIGHT] < 4")

        Do Until Buffer = ""
            current_segment = GetNextSegmentFromSet(Buffer)
            RetailSegment = GetNextSegmentFromSet(BufferRetail)

            item = New DetailListItem
            item.Header = "Letter " & ExtractElementFromSegment("WEIGHT", current_segment, "0") & "oz"
            item.PicturePath = StampImage
            item.Desc = ExtractElementFromSegment("WEIGHT", current_segment, "0") & "oz Stamp First Class"
            item.Sell = ExtractElementFromSegment("Retail-Letter", RetailSegment, "0")
            item.Cost = ExtractElementFromSegment(costField, current_segment, "0")
            item.Service = "FirstClass"
            Letter_OptionList.Add(item)
        Loop
    End Sub

    Private Sub Load_PostCardOptions()
        Dim current_segment As String
        Dim RetailSegment As String
        Dim item As DetailListItem = New DetailListItem

        current_segment = GetNextSegmentFromSet(IO_GetSegmentSet(gUSMailDB_Services, "Select [COST-Postcard], [COST-Postcard-Intl] From FirstClass WHERE [WEIGHT] = 1"))
        RetailSegment = GetNextSegmentFromSet(IO_GetSegmentSet(gPackagingDB, "Select [RETAIL-Postcard],  [RETAIL-Postcard-Intl] From FirstClassRetail WHERE [WEIGHT] = 1"))

        item.Header = "PostCard"
        item.PicturePath = PostCardImage
        item.Desc = "Regular PostCard"
        item.Cost = ExtractElementFromSegment("COST-Postcard", current_segment, "0")
        item.Sell = ExtractElementFromSegment("RETAIL-Postcard", RetailSegment, "0")
        item.Service = "FirstClass"
        PostCard_OptionList.Add(item)

        item = New DetailListItem
        item.Header = "International PostCard"
        item.PicturePath = PostCardImage
        item.Desc = "International PostCard"
        item.Cost = ExtractElementFromSegment("COST-Postcard-Intl", current_segment, "0")
        item.Sell = ExtractElementFromSegment("RETAIL-Postcard-Intl", RetailSegment, "0")
        item.Service = "USPS-INTL-FCMI"
        PostCard_OptionList.Add(item)

    End Sub

#End Region


#Region "DisplayFunctions"


    Private Sub Type_ListBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles Type_ListBox.SelectionChanged

        Select Case Type_ListBox.SelectedIndex
            Case 0 ' Postcard
                DetailOptions_ListBox.ItemsSource = PostCard_OptionList
                DetailOptions_ListBox.Items.Refresh()

            Case 1 'Letter
                DetailOptions_ListBox.ItemsSource = Letter_OptionList
                DetailOptions_ListBox.Items.Refresh()

            Case 2 'Flat
                DetailOptions_ListBox.ItemsSource = Flat_OptionList
                DetailOptions_ListBox.Items.Refresh()

            Case 3 'Package
                DetailOptions_ListBox.ItemsSource = Package_OptionList
                DetailOptions_ListBox.Items.Refresh()

        End Select

        DisplayCustomPostageOptions(Visibility.Hidden)
        AdditionalServices_Grid.Visibility = Visibility.Visible
        DisplayWeightZipCountry(Visibility.Visible, Visibility.Visible, Visibility.Visible)

        ExistingPostageAmnt_TxtBox.Text = "$0.00"
        ExistingPostage_CheckBox.IsChecked = False
        Tracking_TxtBx.Text = ""

        CustomPostageCost.Text = ""
        CustomPostageRetail.Text = ""

        DetailOptions_ListBox.SelectedIndex = 0
    End Sub



    Private Sub DetailOptions_ListBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles DetailOptions_ListBox.SelectionChanged

        If DetailOptions_ListBox.SelectedIndex = -1 Then
            Exit Sub
        End If

        Dim item As DetailListItem = New DetailListItem
        item = DetailOptions_ListBox.SelectedItem

        If item.Header = "Large Parcel" Then
            CustomPostage_Border.Visibility = Visibility.Hidden
            SetDimensionsVisibility(Visibility.Visible)
            L_TxtBox.Focus()
        Else
            SetDimensionsVisibility(Visibility.Hidden)
            CustomPostage_Border.Visibility = Visibility.Visible
        End If


        If Not IsNothing(item.Sell) And item.Sell <> 0 Then
            WeightZip_Border.Visibility = Visibility.Hidden
            ShippingServices_Border.Visibility = Visibility.Hidden
            'Clear_ShippingChoices()
            Set_Service(item.Service)


        Else
            WeightZip_Border.Visibility = Visibility.Visible
            ShippingServices_Border.Visibility = Visibility.Visible
            Set_Service("")
            GetShippingRates()
        End If

        CalculateTotal()

    End Sub

    Private Sub Clear_ShippingChoices()
        Lbs_TxtBox.Text = 0
        Oz_TxtBox.Text = 0
        ZipCode_TxtBox.Text = ""
        CityState_TxtBox.Text = ""
        For Each item As DetailListItem In Service_Btn_DOM_List
            item.Cost = 0
            item.Sell = 0
        Next
        Services_LB.Items.Refresh()
        Services_LB.UnselectAll()

    End Sub

    Private Sub SetDimensionsVisibility(ByRef display As Visibility)
        L_Label.Visibility = display
        W_Label.Visibility = display
        H_Label.Visibility = display

        L_TxtBox.Visibility = display
        W_TxtBox.Visibility = display
        H_TxtBox.Visibility = display

        Dim_Label.Visibility = display

        If display = Visibility.Visible Then
            CustomPostage_Btn.Visibility = Visibility.Hidden
        Else
            CustomPostage_Btn.Visibility = Visibility.Visible
            L_TxtBox.Text = ""
            W_TxtBox.Text = ""
            H_TxtBox.Text = ""
        End If
    End Sub

    Private Sub NumbersTextBoxes_GotFocus(sender As Object, e As RoutedEventArgs) Handles L_TxtBox.GotFocus, W_TxtBox.GotFocus, H_TxtBox.GotFocus, Qty_TxtBox.GotFocus, ExistingPostageAmnt_TxtBox.GotFocus, CustomPostageCost.GotFocus, CustomPostageRetail.GotFocus
        sender.text = ""
    End Sub

    Private Sub ExistingPostage_CheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles ExistingPostage_CheckBox.Checked
        ExistingPostageAmnt_TxtBox.Visibility = Visibility.Visible
        ExistingPostage_Label.Visibility = Visibility.Visible
        ExistingPostageAmnt_TxtBox.Text = ""
        ExistingPostageAmnt_TxtBox.Focus()
    End Sub

    Private Sub ExistingPostage_CheckBox_UnChecked(sender As Object, e As RoutedEventArgs) Handles ExistingPostage_CheckBox.Unchecked
        ExistingPostageAmnt_TxtBox.Text = ""
        ExistingPostageAmnt_TxtBox.Visibility = Visibility.Hidden
        ExistingPostage_Label.Visibility = Visibility.Hidden
    End Sub

    Private Sub Qty_TxtBox_LostFocus(sender As Object, e As RoutedEventArgs) Handles Qty_TxtBox.LostFocus
        If Qty_TxtBox.Text = "" Then Qty_TxtBox.Text = "1"
    End Sub

    Private Sub CustomPostage_Btn_Click(sender As Object, e As RoutedEventArgs) Handles CustomPostage_Btn.Click
        If SelectCustomPotage_Btn.IsVisible Then
            DisplayCustomPostageOptions(Visibility.Hidden)
            DisplayWeightZipCountry(Visibility.Visible, Visibility.Visible, Visibility.Visible)
            AdditionalServices_Grid.Visibility = Visibility.Visible
        Else
            'Display CustomPostage Options
            Type_ListBox.UnselectAll()
            DetailOptions_ListBox.ItemsSource = Nothing
            DetailOptions_ListBox.Items.Refresh()

            DisplayCustomPostageOptions(Visibility.Visible)
            DisplayWeightZipCountry(Visibility.Hidden, Visibility.Hidden, Visibility.Visible)
            AdditionalServices_Grid.Visibility = Visibility.Hidden
            CustomPostageCost.Focus()

        End If
    End Sub

    Private Sub DisplayCustomPostageOptions(display As Visibility)
        SelectCustomPotage_Btn.Visibility = display
        CustomPostageCost.Visibility = display
        CustomPostageRetail.Visibility = display
        CustomRetail_Label.Visibility = display

    End Sub

    Private Sub DisplayWeightZipCountry(ShowWeight As Visibility, ShowZipCity As Visibility, ShowCountry As Visibility)
        Lbs_Label.Visibility = ShowWeight
        Lbs_TxtBox.Visibility = ShowWeight
        Oz_Label.Visibility = ShowWeight
        Oz_TxtBox.Visibility = ShowWeight


        ZipCode_TxtBox.Visibility = ShowZipCity
        ZipLabel.Visibility = ShowZipCity
        CityState_Label.Visibility = ShowZipCity
        CityState_TxtBox.Visibility = ShowZipCity

        Country_ComboBox.Visibility = ShowCountry
        Country_Label.Visibility = ShowCountry

    End Sub

    Private Sub SelectCustomPotage_Btn_Click(sender As Object, e As RoutedEventArgs) Handles SelectCustomPotage_Btn.Click
        If CustomPostageRetail.Text <> "" Then
            Dim cost As Double = Val(CustomPostageCost.Text)
            Dim sell As Double = Val(CustomPostageRetail.Text)



            For Each item As DetailListItem In Service_Btn_DOM_List
                item.Cost = cost
                item.Sell = sell
            Next

            Services_LB.ItemsSource = Service_Btn_DOM_List
            Services_LB.Items.Refresh()

        End If


    End Sub

#End Region


#Region "Additional Services"
    Private Sub Load_AdditionalService_Pricing()
        If Total.Service <> "" Then

            Dim current_segment As String = GetNextSegmentFromSet(IO_GetSegmentSet(gShipriteDB, "SELECT * from Master WHERE [SERVICE]='" & Total.Service & "'"))

            For Each item As Additional_Service In Additional_Services_List
                If item.ID <> 4 Then
                    item.Cost = ExtractElementFromSegment(item.CostField, current_segment, "0")
                    item.Sell = ExtractElementFromSegment(item.SellField, current_segment, "0")
                End If
            Next

        Else
            For Each item As Additional_Service In Additional_Services_List
                item.Cost = 0
                item.Sell = 0
            Next
        End If
        Display_AdditionalService_Rules()

        Additional_Services_LB.Items.Refresh()

    End Sub

    Private Sub Display_AdditionalService_Rules()


        If Additional_Services_LB.SelectedIndex = -1 Then
            'Initial display of available services
            For Each item As Additional_Service In Additional_Services_LB.Items
                Select Case item.ID
                    Case 1 'Certified
                        If Total.Service = "USPS-EXPR" Or Total.Service = "USPS-MEDIA" Or Total.Service = "USPS-INTL-FCMI" Then
                            item.Visible = Visibility.Hidden
                        Else
                            item.Visible = Visibility.Visible
                        End If


                    Case 2 'Return Receipt
                        If Total.Service = "USPS-EXPR" Or Total.Service = "USPS-INTL-FCMI" Then
                            item.Visible = Visibility.Visible
                        Else
                            item.Visible = Visibility.Hidden
                        End If


                    Case 3 'Signature Confirmation
                        If Total.Service = "USPS-EXPR" Or (Total.Service = "FirstClass" And Type_ListBox.SelectedIndex <> 3) Or Total.Service = "USPS-INTL-FCMI" Then
                            item.Visible = Visibility.Hidden
                        Else
                            item.Visible = Visibility.Visible
                        End If

                End Select
            Next

        End If



        If Total.Service <> "USPS-EXPR" And Total.Service <> "USPS-INTL-FCMI" Then
            Dim certified_Selected As Boolean = False
            Dim Sign_Confirm_Selected As Boolean = False

            For Each item As Additional_Service In Additional_Services_LB.SelectedItems
                If item.ID = 1 Then
                    certified_Selected = True
                ElseIf item.ID = 3 Then
                    Sign_Confirm_Selected = True
                End If
            Next

            If certified_Selected = True Then
                Additional_Services_LB.Items.Item(1).Visible = Visibility.Visible 'rtrn rcpt
                Additional_Services_LB.Items.Item(2).Visible = Visibility.Hidden 'sig confirm
            Else
                Additional_Services_LB.Items.Item(1).Visible = Visibility.Hidden
                Additional_Services_LB.Items.Item(2).Visible = Visibility.Visible
            End If

            If Sign_Confirm_Selected = True Then
                Additional_Services_LB.Items.Item(0).Visible = Visibility.Hidden 'certified
            Else
                Additional_Services_LB.Items.Item(0).Visible = Visibility.Visible
            End If
        End If



        Additional_Services_LB.Items.Refresh()
    End Sub

    Private Sub Additional_Services_LB_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles Additional_Services_LB.SelectionChanged
        Display_AdditionalService_Rules()
        Check_insuranceOption()

        If Additional_Services_LB.SelectedIndex = -1 Then
            CalculateTotal()
            Exit Sub
        End If

        'if multiple items selected, get the last selected item
        Dim current_item As Additional_Service
        current_item = Additional_Services_LB.SelectedItems(Additional_Services_LB.SelectedItems.Count - 1)

        If current_item.Visible = Visibility.Hidden Then
            CalculateTotal()
            Exit Sub
        End If


        'Return Receipt Tracking
        If current_item.ID = 2 Then
            ReturnReceiptTracking = InputBox("Scan or enter Return Receipt Tracking Number!", "Return Receipt", "")

        Else
            Dim item As Additional_Service = Additional_Services_LB.Items.Item(2)
            If Not Additional_Services_LB.SelectedItems.Contains(item) Then
                ReturnReceiptTracking = ""
            End If
        End If

        Additional_Services_LB.Items.Refresh()
        CalculateTotal()

    End Sub

    Private Sub Check_insuranceOption()
        Dim item As Additional_Service = Additional_Services_LB.Items.Item(3)

        If Additional_Services_LB.SelectedItems.Contains(item) Then
            Insurance_Lbl.Visibility = Visibility.Visible
            Insurance_TxtBx.Visibility = Visibility.Visible
            Insurance_TxtBx.Focus()
        Else
            Insurance_Lbl.Visibility = Visibility.Hidden
            Insurance_TxtBx.Text = ""
            Insurance_TxtBx.Visibility = Visibility.Hidden
            item.Cost = 0
            item.Sell = 0
        End If


    End Sub

    Private Sub Insurance_TxtBx_LostFocus(sender As Object, e As RoutedEventArgs) Handles Insurance_TxtBx.LostFocus
        Calculate_Insurance_Charge()
    End Sub

    Private Sub Insurance_TxtBx_KeyUp(sender As Object, e As KeyEventArgs) Handles Insurance_TxtBx.KeyUp
        If (e.Key = Key.Return) Then
            Calculate_Insurance_Charge()
        End If
    End Sub

    Private Sub Calculate_Insurance_Charge()
        If Insurance_TxtBx.Text = "" Or Insurance_TxtBx.Text = "0" Then Exit Sub
        Dim current_segment As String

        'Domestic Insurance
        If Total.Service = "USPS-EXPR" Then
            current_segment = GetNextSegmentFromSet(IO_GetSegmentSet(gUSMailDB_Services, "Select [EXP_BaseCost], [EXP_SellPrice] From Insurance WHERE " & Insurance_TxtBx.Text & ">=CoverFrom AND " & Insurance_TxtBx.Text & "<=CoverUpTo"))
        Else
            current_segment = GetNextSegmentFromSet(IO_GetSegmentSet(gUSMailDB_Services, "Select [BaseCost], [SellPrice] From Insurance WHERE " & Insurance_TxtBx.Text & ">=CoverFrom AND " & Insurance_TxtBx.Text & "<=CoverUpTo"))
        End If


        If current_segment = "" Then
            MsgBox("Could Not apply insurance option, please check the insured amount!", vbCritical)
            Exit Sub
        End If

        If Total.Service = "USPS-EXPR" Then
            Additional_Services_List.Find(Function(value As Additional_Service) value.ID = 4).Cost = ExtractElementFromSegment("EXP_BaseCost", current_segment, "0")
            Additional_Services_List.Find(Function(value As Additional_Service) value.ID = 4).Sell = ExtractElementFromSegment("EXP_SellPrice", current_segment, "0")
        Else
            Additional_Services_List.Find(Function(value As Additional_Service) value.ID = 4).Cost = ExtractElementFromSegment("BaseCost", current_segment, "0")
            Additional_Services_List.Find(Function(value As Additional_Service) value.ID = 4).Sell = ExtractElementFromSegment("SellPrice", current_segment, "0")
        End If

        Additional_Services_LB.Items.Refresh()
        CalculateTotal()
    End Sub

#End Region

    Private Sub GetShippingRates()
        Dim zone As String
        Dim weight As Double
        Dim FCM_PackMarkup As Double

        If IsNothing(Service_Btn_DOM_List) Then Exit Sub

        Services_LB.UnselectAll()

        If Country_ComboBox.SelectedItem.ToString <> "United States" Then
            GetInternationalRates()
            Exit Sub
        End If

        For Each item As DetailListItem In Service_Btn_DOM_List

            'reset cost and sell
            item.Cost = 0
            item.Sell = 0

            zone = ShipManager.GetShippingZone("USPS", ZipCode_TxtBox.Text, item.Service)


            If item.Service = "FirstClass" Then
                If Type_ListBox.SelectedIndex <> -1 And CalculateWeight(True) <= 13 Then

                    If Type_ListBox.SelectedIndex <> 3 Then '1st class Package service has been eliminated

                        If Type_ListBox.SelectedIndex = 1 Then
                            zone = "Letter"
                        ElseIf Type_ListBox.SelectedIndex = 2 Then
                            zone = "Flat"
                        ElseIf Type_ListBox.SelectedIndex = 3 Then
                            zone = "Pack-" & zone
                        End If

                        If zone <> "Pack-" Then
                            Dim costField As String = "COST-"
                            If zone = "Letter" And _EndiciaWeb.EndiciaWeb_IsEnabled Then
                                costField = "COMM-"
                            End If
                            item.Cost = Val(ExtractElementFromSegment(costField & zone, IO_GetSegmentSet(gUSMailDB_Services, "Select [" & costField & zone & "] From FirstClass WHERE WEIGHT=" & CalculateWeight(True))))

                            If zone = "Letter" Or zone = "Flat" Then
                                item.Sell = Val(ExtractElementFromSegment("RETAIL-" & zone, IO_GetSegmentSet(gPackagingDB, "Select [RETAIL-" & zone & "] From FirstClassRetail WHERE WEIGHT=" & CalculateWeight(True))))
                            Else
                                FCM_PackMarkup = ExtractElementFromSegment("Percent", IO_GetSegmentSet(gShipriteDB, "Select [Percent] From Master Where SERVICE='FirstClass'"), "0")
                                item.Sell = item.Cost * (1 + (FCM_PackMarkup / 100))
                                item.Sell = Round(item.Sell, 2)
                            End If

                            If Type_ListBox.SelectedIndex = 1 And DetailOptions_ListBox.SelectedIndex = 1 Then
                                'Non Machineable Letter option selected
                                item.Cost = Round(item.Cost + ExtractElementFromSegment("ACTAH", IO_GetSegmentSet(gShipriteDB, "Select [ACTAH] From Master Where SERVICE='FirstClass'"), "0"), 2)
                                item.Sell = Round(item.Sell + ExtractElementFromSegment("AH", IO_GetSegmentSet(gShipriteDB, "Select [AH] From Master Where SERVICE='FirstClass'"), "0"), 2)

                            End If
                        Else

                            item.Cost = 0
                            item.Sell = 0
                        End If

                    End If
                End If

            ElseIf item.Service = "USPS-MEDIA" Then
                'Media Mail is only available for Parcels
                If Type_ListBox.SelectedIndex = 3 Then
                    zone = "ZONE1"
                    weight = CalculateWeight(False)
                    If weight < 1 And weight > 0 Then
                        weight = 1
                    End If

                    item.Cost = ShipManager.GetShippingCost(item.Service, zone, weight, Date.Today)
                    If item.Cost <> 0 Then
                        item.Sell = ShipManager.GetShippingSellingPrice(gMaster(Find_Master_Index(item.Service)), item.Cost, False, weight, zone, False)
                    End If

                End If


            Else

                If Type_ListBox.SelectedIndex = 3 And DetailOptions_ListBox.SelectedIndex = 1 Then
                    ' Large Package selected, calculate DIM Weight

                    If L_TxtBox.Text <> "" And L_TxtBox.Text <> "0" And W_TxtBox.Text <> "" And W_TxtBox.Text <> "0" And H_TxtBox.Text <> "" And H_TxtBox.Text <> "0" Then

                        Dim DimWeight As Double = ShipManager.Calculate_DimWeight("USPS", item.Service, False, CalculateWeight(False), L_TxtBox.Text, W_TxtBox.Text, H_TxtBox.Text)

                        If DimWeight > CalculateWeight(False) Then
                            item.Cost = ShipManager.GetShippingCost(item.Service, zone, DimWeight, Date.Today)
                        Else
                            item.Cost = ShipManager.GetShippingCost(item.Service, zone, CalculateWeight(False), Date.Today)
                        End If

                    End If
                Else

                    item.Cost = ShipManager.GetShippingCost(item.Service, zone, CalculateWeight(False), Date.Today)

                End If

                If item.Cost <> 0 Then
                    If Type_ListBox.SelectedIndex = 1 Or Type_ListBox.SelectedIndex = 2 Then
                        'Use Letter Markup
                        item.Sell = ShipManager.GetShippingSellingPrice(gMaster(Find_Master_Index(item.Service)), item.Cost, False, CalculateWeight(False), zone, True)
                    Else
                        'Use Level 1,2,3 Markup
                        item.Sell = ShipManager.GetShippingSellingPrice(gMaster(Find_Master_Index(item.Service)), item.Cost, False, CalculateWeight(False), zone, False)
                    End If
                End If

            End If



        Next

        'sorts service buttons by price
        'Service_Btn_DOM_List = Service_Btn_DOM_List.OrderBy(Function(item As DetailListItem) item.Sell = 0).ThenBy(Function(x As DetailListItem) x.Sell).ToList

        Services_LB.ItemsSource = Service_Btn_DOM_List
        Services_LB.Items.Refresh()

    End Sub

    Private Sub GetInternationalRates()
        Dim zone As String

        For Each item As DetailListItem In Service_Btn_INTL_List

            'reset cost and sell
            item.Cost = 0
            item.Sell = 0

            zone = ShipManager.GetShippingZone_USMail_International(item.Service, Country_ComboBox.SelectedItem.ToString)

            If Type_ListBox.SelectedIndex = 1 Then
                'Letter
                item.Cost = Val(ExtractElementFromSegment("Cost", IO_GetSegmentSet(gUSMailDB_Services, "Select min(" & zone & ") as Cost From [USPS-INTL-FCMI_Letter] WHERE OZS >= " & CalculateWeight(True))))
                item.Sell = ShipManager.GetShippingSellingPrice(gMaster(Find_Master_Index(item.Service)), item.Cost, False, CalculateWeight(False), zone, False)

            ElseIf Type_ListBox.SelectedIndex = 2 Then
                'Flat
                item.Cost = Val(ExtractElementFromSegment("Cost", IO_GetSegmentSet(gUSMailDB_Services, "Select min(" & zone & ") as Cost From [USPS-INTL-FCMI_Flats] WHERE OZS >= " & CalculateWeight(True))))
                item.Sell = ShipManager.GetShippingSellingPrice(gMaster(Find_Master_Index(item.Service)), item.Cost, False, CalculateWeight(False), zone, True)
            ElseIf Type_ListBox.SelectedIndex = 3 Then
                'Package
                item.Cost = 0
                item.Sell = 0
            End If

        Next

        Services_LB.ItemsSource = Service_Btn_INTL_List
        Services_LB.Items.Refresh()
    End Sub


    Private Function CalculateWeight(ByVal returnOunces As Boolean) As Double
        Dim ounces As Double

        If returnOunces = True Then
            'Return ozs
            ounces = Pounds2Ounces(Val(Lbs_TxtBox.Text), 2) + Val(Oz_TxtBox.Text)

            If ounces > 3 And ounces <= 3.5 Then
                Return 3.5
            Else
                Return Math.Ceiling(ounces)
            End If

        Else
            'return lbs
            Dim pounds As Double
            pounds = Val(Lbs_TxtBox.Text) + (Val(Oz_TxtBox.Text) / 16)

            If pounds <= 0.5 And pounds > 0 Then
                Return 0.5
            Else
                Return Math.Ceiling(pounds)
            End If

        End If



    End Function

    Private Sub Set_Service(ByRef service As String)
        Total.Service = service
        Service_Txt.Text = service
        Additional_Services_LB.UnselectAll()
        Load_AdditionalService_Pricing()
    End Sub



    Private Sub Services_LB_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles Services_LB.SelectionChanged
        If Services_LB.SelectedIndex = -1 Then Exit Sub
        Dim item As DetailListItem = Services_LB.SelectedItem
        Set_Service(item.Service)
        CalculateTotal()
    End Sub

    Private Sub Lbs_TxtBox_LostFocus(sender As Object, e As RoutedEventArgs) Handles Lbs_TxtBox.LostFocus, Oz_TxtBox.LostFocus, ZipCode_TxtBox.LostFocus
        Dim SegmentSet As String

        If sender.Name = ZipCode_TxtBox.Name Then
            If ZipCode_TxtBox.Text <> "" Then
                SegmentSet = IO_GetSegmentSet(gZipCodeDB, "SELECT City, ST FROM ZipCodes WHERE Zip = '" & ZipCode_TxtBox.Text & "'")
                CityState_TxtBox.Text = ExtractElementFromSegment("City", SegmentSet) & ", " & ExtractElementFromSegment("ST", SegmentSet)
            Else
                CityState_TxtBox.Text = ""
            End If
        End If

        GetShippingRates()
    End Sub

    Private Sub Dims_LostFocus(sender As Object, e As RoutedEventArgs) Handles L_TxtBox.LostFocus, W_TxtBox.LostFocus, H_TxtBox.LostFocus
        If L_TxtBox.Text = "" Or L_TxtBox.Text = "0" Or W_TxtBox.Text = "" Or W_TxtBox.Text = "0" Or H_TxtBox.Text = "" Or H_TxtBox.Text = "0" Then
            Exit Sub
        End If

        GetShippingRates()

    End Sub

    Private Sub CalculateTotal()
        If CustomPostageRetail.Text = "" Then
            If Type_ListBox.SelectedIndex = -1 Then Exit Sub
            If DetailOptions_ListBox.SelectedIndex = -1 Then Exit Sub
        End If

        Dim item As DetailListItem = New DetailListItem

        Total.Sell = 0
        Total.Cost = 0

        item = DetailOptions_ListBox.SelectedItem


        If Not IsNothing(item) AndAlso item.Sell <> 0 Then
            'Get set price from DetailOptions Listbox

            Total.Sell = item.Sell
            Total.Cost = item.Cost

        ElseIf Services_LB.SelectedIndex <> -1 Then
            'Get Price from Service Buttons
            item = Services_LB.SelectedItem
            Total.Sell = Val(item.Sell)
            Total.Cost = item.Cost
        End If


        'Save totals for shipping service only, without accessorials.
        Total.ShippingCost = Total.Cost
        Total.ShippingSell = Total.Sell

        'Add additional services:
        For Each AddService As Additional_Service In Additional_Services_LB.SelectedItems
            If AddService.Visible = Visibility.Visible Then
                Total.Sell = Total.Sell + AddService.Sell
                Total.Cost = Total.Cost + AddService.Cost
            End If
        Next

        Total_TxtBx.Text = FormatCurrency(Total.Sell)
        Cost_Txt.Text = FormatCurrency(Total.Cost)


    End Sub

    Private Sub PrintPostage_Btn_Click(sender As Object, e As RoutedEventArgs) Handles PrintPostage_Btn.Click

        'If GetPolicyData(gShipriteDB, "Endicia_AccountID2", "") <> "" Then
        If _EndiciaWeb.EndiciaWeb_IsEnabled Then
            'DYMO NetStamps Enabled

            If Total.Service = "" Or Total.Sell = 0 Then
                Exit Sub
            End If

            Dim stampRequest As _EndiciaStamps = New _EndiciaStamps
            Dim response As baseWebResponse_Shipment = New baseWebResponse_Shipment
            Dim Pack As New baseWebResponse_Package
            response.Packages.Add(Pack)

            With stampRequest
                .Quantity = Qty_TxtBox.Text
                .MailClass = Total.Service
                .WeightOz = CalculateWeight(True)
                .UseUserRate = True
                .UserRate = Total.Cost
                .ToCountryCode = "US"
                .ShipDate = DateTime.Now
                .TestPrint = TestPostage_CheckBox.IsChecked

                If ExistingPostage_CheckBox.IsChecked And Val(ExistingPostageAmnt_TxtBox.Text) > 0 Then
                    'subtract existing postage from printed postage amount
                    .UserRate = .UserRate - Val(ExistingPostageAmnt_TxtBox.Text)
                End If


                Select Case Type_ListBox.SelectedIndex
                    Case 0
                        .MailpieceShape = "Card"
                    Case 1
                        .MailpieceShape = "Letter"
                    Case 2
                        .MailpieceShape = "Flat"
                    Case 3
                        .MailpieceShape = "Parcel"
                    Case Else
                        .MailpieceShape = "Parcel"
                End Select

            End With

            If stampRequest.MailClass = "USPS-INTL-FCMI" Then
                If stampRequest.MailpieceShape = "Card" OrElse stampRequest.MailpieceShape = "Letter" Then
                    stampRequest.MailClass = "USPS-INTL-FCMI_Letter"
                ElseIf stampRequest.MailpieceShape = "Flat" Then
                    stampRequest.MailClass = "USPS-INTL-FCMI_Flats"
                End If
            End If

            Request_Stamp(DYMO_Setup, stampRequest, response)

            If response.ShipmentAlerts.Count > 0 Then
                Dim AlertMessage As String = ""

                For Each alert As String In response.ShipmentAlerts
                    AlertMessage = AlertMessage & alert & vbCrLf
                Next
                MsgBox(AlertMessage, vbOKOnly + vbExclamation, "Endicia NetStamps Server")

            Else
                'request successful
                Add_Total_Line_Item()
                Clear_ShippingChoices()
            End If

        Else

            'DYMO NetStamps not setup
            NoPrint_Btn_Click(Nothing, Nothing)
        End If



    End Sub


    Private Sub NoPrint_Btn_Click(sender As Object, e As RoutedEventArgs) Handles NoPrint_Btn.Click
        If Total.Service = "" Or Total.Sell = 0 Then
            Exit Sub
        End If

        Add_Total_Line_Item()

        Clear_ShippingChoices()
    End Sub

    Private Sub Add_Total_Line_Item()
        Dim item As TotalLineItem = New TotalLineItem
        Dim ExistingPostage As Double = Val(ExistingPostageAmnt_TxtBox.Text)


        '-- add Shipping charge--------------------------
        With item
            .SKU = Total.Service
            .Desc = ExtractElementFromSegment("DESCRIPTION", gMaster(Find_Master_Index(Total.Service)).Segment, "")
            .Cost = Total.ShippingCost
            .Sell = Total.ShippingSell
            .Qty = Qty_TxtBox.Text
            .ExtPrice = Math.Round(item.Sell * item.Qty, 2)
            .Service = Total.Service
        End With

        If DetailOptions_ListBox.SelectedIndex <> -1 Then
            Dim x As DetailListItem = DetailOptions_ListBox.SelectedItem

            If x.Desc.Contains("FlatR") Then
                If x.Header <> "" Then item.Desc = item.Desc & " - FlatRate" & x.Header

            ElseIf x.Desc.Contains("oz") Then
                If x.Header <> "" Then item.Desc = item.Desc & " - " & x.Header

            ElseIf item.Service = "FirstClass" Then
                item.Desc = item.Desc & " - " & x.Header & " " & CalculateWeight(True) & "oz"

            Else
                item.Desc = item.Desc & " - " & x.Header & " " & CalculateWeight(False) & "lbs"

            End If
        End If

        TotalLine_list.Add(item)


        '-- add line items for additional services---------------------------
        For Each AddService As Additional_Service In Additional_Services_LB.SelectedItems
            If AddService.Visible = Visibility.Visible Then
                item = New TotalLineItem

                With item
                    .SKU = Total.Service
                    .Desc = AddService.Header
                    .Cost = AddService.Cost
                    .Sell = AddService.Sell
                    .Qty = Qty_TxtBox.Text
                    .ExtPrice = Math.Round(item.Sell * item.Qty, 2)
                    .Service = Total.Service
                End With

                TotalLine_list.Add(item)

                If AddService.ID = 2 And ReturnReceiptTracking <> "" Then
                    item = New TotalLineItem

                    With item
                        .SKU = "NOTE"
                        .Desc = "Return Receipt# " & ReturnReceiptTracking
                        .Cost = 0
                        .Sell = 0
                        .Qty = 1
                        .ExtPrice = 0
                    End With

                    TotalLine_list.Add(item)
                End If
            End If
        Next


        '----add discount for existing postage--------------------------------
        If ExistingPostage > 0 Then
            item = New TotalLineItem

            With item
                .SKU = Total.Service
                .Desc = "Existing Postage"
                .Cost = ExistingPostage * -1
                .Sell = ExistingPostage * -1
                .Qty = Qty_TxtBox.Text
                .ExtPrice = Math.Round(item.Sell * item.Qty, 2)
                .Service = Total.Service
            End With

            TotalLine_list.Add(item)
        End If


        '---- Add Tracking Number -----------------------------------
        If Tracking_TxtBx.Text <> "" Then
            item = New TotalLineItem

            With item
                .SKU = "NOTE"
                .Desc = "Tracking# " & Tracking_TxtBx.Text
                .Cost = 0
                .Sell = 0
                .Qty = 1
                .ExtPrice = 0
            End With

            TotalLine_list.Add(item)
        End If




        TotalLine_LV.ItemsSource = TotalLine_list
        TotalLine_LV.Items.Refresh()


        Type_ListBox_SelectionChanged(Nothing, Nothing)


    End Sub

    Private Sub SaveButton_Click(sender As Object, e As RoutedEventArgs) Handles SaveButton.Click

        Dim posM As POSManager = CommonWindowStack.windowList.Find(Function(x As CommonWindow) x.Name = "POS_Window")

        If Not IsNothing(posM) Then
            For Each item As TotalLineItem In TotalLine_list

                Dim line As New POSLine

                With line
                    .SKU = item.SKU
                    .Description = item.Desc
                    .Department = ExtractElementFromSegment("POSDept", gMaster(Find_Master_Index(item.Service)).Segment, "")
                    .UnitPrice = item.Sell
                    .Quantity = item.Qty
                    .ExtPrice = item.ExtPrice
                    .LTotal = item.ExtPrice
                    .COGS = item.Cost
                    .isPriceOverride = True 'needed so that line edit in POS won't look for price in inventory when editing line.

                End With
                POSLines.Add(line)

            Next
        End If

        Me.Close()

    End Sub

    Private Sub Edit_NetStamps_Serial_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Edit_NetStamps_Serial_Btn.Click
        Dim result As String
        result = InputBox("Enter New Serial Number:", "Update NetStamps Serial Number")
        If result <> "" Then
            UpdatePolicy(gReportsDB, "DYMO_ActivationCode", result)
            NetStamps_Serial_TxtBx.Text = result
        End If


    End Sub

    Private Sub ExistingPostageAmnt_TxtBox_LostFocus(sender As Object, e As RoutedEventArgs) Handles ExistingPostageAmnt_TxtBox.LostFocus
        CalculateTotal()
    End Sub

    Private Sub Country_ComboBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles Country_ComboBox.SelectionChanged
        GetShippingRates()
    End Sub
End Class
