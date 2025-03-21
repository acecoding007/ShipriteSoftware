Public Class Print_Shipping_Label
    Inherits CommonWindow

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

    Public Sub New(ByVal callingWindow As Window, ByVal isShipperSelected As Boolean, ByVal isConsigneeSelected As Boolean)

        MyBase.New(callingWindow)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        If Not (isShipperSelected And isConsigneeSelected) Then
            ShipPackage_Button.Opacity = "0.5" 'ShipPackage_Button.Background.Opacity = "0.5"
            ShipPackage_Button.IsEnabled = False
        End If
        '

    End Sub

    Private Sub Update_Description(SenderName As String)
        'Display description of selected option.

        Select Case SenderName


            Case FedEx_NonStandardCont_CheckBox.Name
                DescriptionHeader_Label.Content = "FEDEX NON-STANDARD CONTAINER"
                Description_TextBlock.Text = ""


            Case FedEx_DryIce_CheckBox.Name
                DescriptionHeader_Label.Content = "FEDEX DRY ICE"
                Description_TextBlock.Text = "Check this option to inform FedEx that the shipment contains Dry Ice. Dry ice is considered a Dangerous Goods material. For more information about dry ice, call 1.800.GoFedEx 1.800.463.3339 and press 81 to reach the FedEx Dangerous Goods / Hazardous Materials Hotline."

            Case FedEx_HoldAtLocation_CheckBox.Name
                DescriptionHeader_Label.Content = "HOLD AT FEDEX LOCATION"
                Description_TextBlock.Text = "If recipient will not be availble to receive shipment, use this option to deliver it to a pre-determined FedEx location close by for pickup."


            Case Fedex_IndirectSig_RadioBtn.Name
                DescriptionHeader_Label.Content = "FEDEX INDIRECT SIGNATURE"
                Description_TextBlock.Text = "FedEx will obtain signature in one of three ways:
  1. From someone at the delivery address; or
  2. From a neighbor, building manager, or other person at a neighboring address; or
  3. The receipient can sign a FedEx door tag authorizing release of package without anyone present"

            Case FedEx_DirectSig_RadioBtn.Name
                DescriptionHeader_Label.Content = "FEDEX DIRECT SIGNATURE"
                Description_TextBlock.Text = "FedEx will obtain a signature from someone at the delivery address. If no one is at the address, FedEx will reattempt delivery. Direct Signature Required overrides any recipient release that may be on file for deliveries to nonresidential addresses."

            Case FedEx_AdultSig_RadioBtn.Name
                DescriptionHeader_Label.Content = "FEDEX ADULT SIGNATURE"
                Description_TextBlock.Text = "FedEx will obtain a signature from someone at least 21 years old (government issued photo ID required) at the delivery address. If no one is at the address, 
FedEx will reattempt delivery. Adult Signature Required  overrides any recipient release that may be on file for deliveries to nonresidential addresses."

            Case FedEx_DateCertain_RadioBtn.Name
                DescriptionHeader_Label.Content = "FEDEX DATE CERTAIN HOME DELIVERY"
                Description_TextBlock.Text = "Use this option if your recipient wants to specify a certain date of delivery. 
This can be Tuesday through Saturday, excluding holidays. Date cannot be before the standard transit time, and must be no later then 2 weeks after the standard transit time."

            Case FedEx_Evening_RadioBtn.Name
                DescriptionHeader_Label.Content = "FEDEX EVENINIG HOME DELIVERY"
                Description_TextBlock.Text = "Use this option if delivery must be done in person and the recipient wants to
specify delivery between 5pm and 8pm on the scheduled delivery day."

            Case FedEx_Appointment_RadioBtn.Name
                DescriptionHeader_Label.Content = "FEDEX APPOINTMENT HOME DELIVERY"
                Description_TextBlock.Text = "Use this option If recipient needs to arrange specific date and time for delivery.
FedEx will contact the recipient by phone in advance to schedule delivery time."

'-----------------------------------------------------------------------------------------------------------------------------------------------------------------------

            Case UPS_DeliveryConfirm_RadioBtn.Name
                DescriptionHeader_Label.Content = "UPS DELIVERY CONFIRMATION"
                Description_TextBlock.Text = "UPS will mail you a confirmation of delivery without a signature. Note: Similar information is available when you track your package online."

            Case UPS_SigRequired_RadioBtn.Name
                DescriptionHeader_Label.Content = "UPS SIGNATURE CONFIRMATION"
                Description_TextBlock.Text = "UPS will obtain the recipient's signature or other electronic acknowledgement of receipt from the recipient when this option is selected and provide you with a printed copy. You may also view the recipient's signature or electronic acknowledgement of receipt online."

            Case UPS_AdultSig_RadioBtn.Name
                DescriptionHeader_Label.Content = "UPS ADULT SIGNATURE REQUIRED"
                Description_TextBlock.Text = "UPS will obtain the adult recipient's signature and provide you with a printed copy. Adult recipients must be at least 21. You may also view the adult recipient's signature online."

'-----------------------------------------------------------------------------------------------------------------------------------------------------------------------
            Case USPS_CertifiedMail_CheckBox.Name
                DescriptionHeader_Label.Content = "USPS CERTIFIED MAIL"
                Description_TextBlock.Text = "Prove you sent it. See when it was delivered or that a delivery attempt was made, and get the signature of the person who accepts the mailing when combined with Return Receipt."

            Case USPS_ReturnReceipt_CheckBox.Name
                DescriptionHeader_Label.Content = "USPS RETURN RECEIPT"
                Description_TextBlock.Text = "Get an electronic or hardcopy delivery record showing the recipient’s signature."

            Case USPS_Signature_RadioBtn.Name
                DescriptionHeader_Label.Content = "USPS SIGNATURE CONFIRMATION"
                Description_TextBlock.Text = "Find out information about the date and time an item was delivered, or when a delivery attempt was made. Add security by requiring a signature. A delivery record is kept by USPS and available electronically or by email, upon request."

            Case USPS_AdultSig_RadioBtn.Name
                DescriptionHeader_Label.Content = "USPS ADULT SIGNATURE REQUIRED"
                Description_TextBlock.Text = "This service requires the signature of an adult—someone 21 years of age or older—at the recipient’s address. You’ll get delivery information, as well as the recipient’s signature and name. "


            Case Else
                DescriptionHeader_Label.Content = ""
                Description_TextBlock.Text = ""
        End Select

    End Sub

    Private Sub FedEx_DateCertain_RadioBtn_Checked(sender As Object, e As RoutedEventArgs) Handles FedEx_DateCertain_RadioBtn.Checked
        FedEx_DateCertain_DatePicker.Visibility = Visibility.Visible
    End Sub

    Private Sub FedEx_DateCertain_RadioBtn_Unchecked(sender As Object, e As RoutedEventArgs) Handles FedEx_DateCertain_RadioBtn.Unchecked
        FedEx_DateCertain_DatePicker.Visibility = Visibility.Hidden
    End Sub

    Private Sub PrintButton_Click(sender As Object, e As RoutedEventArgs) Handles PrintButton.Click
        If Print_Popup.IsOpen = True Then
            Print_Popup.IsOpen = False
        Else
            Print_Popup.IsOpen = True
        End If
    End Sub

    Private Sub Print_Shipping_Label_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded

        Try
            'makes tab headers not visible in run time. 
            For Each currentTab As TabItem In Carrier_TabControl.Items
                currentTab.Visibility = Visibility.Collapsed
            Next

            StoreAddress_ChkBx.Visibility = Visibility.Hidden
            gPackageShipped = False
            If gSelectedShipmentChoice.Service.Length > 0 And gShip IsNot Nothing Then

                From_TxtBox.Text = General.CreateDisplayBlock(gShipperSegment, True)
                To_TxtBox.Text = General.CreateDisplayBlock(gConsigneeSegment, True)
                Weight_TxtBox.Text = gShip.actualWeight.ToString
                DeclaredValue_TxtBox.Text = gShip.DecVal.ToString()
                L_TxtBox.Text = gSelectedShipmentChoice.Length
                W_TxtBox.Text = gSelectedShipmentChoice.Width
                H_TxtBox.Text = gSelectedShipmentChoice.Height
                TotalPrice_TxtBox.Text = FormatCurrency(gSelectedShipmentChoice.TotalSell)

                Call Display_Carrier_Options()
                Call ShowPackageDetails()

            End If

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try

    End Sub

    Private Sub Display_Carrier_Options()

        Dim SegmentSet As String = GetNextSegmentFromSet(IO_GetSegmentSet(gShipriteDB, "SELECT * from Master WHERE [SERVICE]='" & gSelectedShipmentChoice.Service & "'"))
        '
        Select Case gSelectedShipmentChoice.Carrier
            '
            Case "FedEx"
                '
                If "FEDEX-GND" = gSelectedShipmentChoice.Service Then
                    '
                    Me.FedEx_Appointment_TextBox.Text = Val(ExtractElementFromSegment("FedEXHDAppt", SegmentSet)).ToString("$ 0.00")
                    Me.FedEx_Evening_TextBox.Text = Val(ExtractElementFromSegment("FedEXHDEvening", SegmentSet)).ToString("$ 0.00")
                    Me.FedEx_DateCertain_TextBox.Text = Val(ExtractElementFromSegment("FedEXHDCertain", SegmentSet)).ToString("$ 0.00")
                    '
                    FedEx_HomeDelivery_Border.Visibility = Visibility.Visible
                Else
                    FedEx_HomeDelivery_Border.Visibility = Visibility.Collapsed
                End If

                Me.Fedex_IndirectSig_TextBox.Text = Val(ExtractElementFromSegment("ISigChg", SegmentSet)).ToString("$ 0.00")
                Me.FedEx_DirectSig_TextBox.Text = Val(ExtractElementFromSegment("ACK", SegmentSet)).ToString("$ 0.00")
                Me.FedEx_AdultSig_TextBox.Text = Val(ExtractElementFromSegment("ACK-S", SegmentSet)).ToString("$ 0.00")

                If GetPolicyData(gShipriteDB, "Disable_FedEx_EmailShipNotifications", "True") Then
                    FedEx_OtherEmail_TxtBx.Visibility = Visibility.Hidden
                    FedEx_OtherEmail_txt.Visibility = Visibility.Hidden
                End If

                If Not isServiceFreight(gSelectedShipmentChoice.Service) Then
                    FedEx_InsideDelivery_CheckBox.Visibility = Visibility.Hidden
                    FedEx_InsidePickup_CheckBox.Visibility = Visibility.Hidden
                End If

                Dim sig = False
                For Each item As ShippingSurcharge In gSelectedShipmentChoice.SurchargesList
                    Select Case item.ID
                        Case ShippingSurcharge.IDs_FedEx.Sig_Ind
                            Fedex_IndirectSig_RadioBtn.IsChecked = True
                            sig = True
                            Exit For

                        Case ShippingSurcharge.IDs_FedEx.Sig_Dir
                            FedEx_DirectSig_RadioBtn.IsChecked = True
                            sig = True
                            Exit For

                        Case ShippingSurcharge.IDs_FedEx.Sig_Adult
                            FedEx_AdultSig_RadioBtn.IsChecked = True
                            sig = True
                            Exit For
                    End Select
                Next
                'If Not sig Then  'If NoSig is checked, then a NO_SIGNATURE tag needs to be sent. 
                'FedEx_NoSig_RadioBtn.IsChecked = True
                ' End If

                Carrier_TabControl.SelectedIndex = 0


            Case "UPS"
                '
                Me.UPS_DeliveryConfirm_TextBox.Text = Val(ExtractElementFromSegment("ACK", SegmentSet)).ToString("$ 0.00")
                Me.UPS_SigRequired_TextBox.Text = Val(ExtractElementFromSegment("ACK-S", SegmentSet)).ToString("$ 0.00")
                Me.UPS_AdultSig_TextBox.Text = Val(ExtractElementFromSegment("DELSIGADULT", SegmentSet)).ToString("$ 0.00")
                '
                Dim sig = False
                For Each item As ShippingSurcharge In gSelectedShipmentChoice.SurchargesList
                    Select Case item.ID
                        Case ShippingSurcharge.IDs_UPS.Sig_DelConf
                            UPS_DeliveryConfirm_RadioBtn.IsChecked = True
                            sig = True
                            Exit For

                        Case ShippingSurcharge.IDs_UPS.Sig_Req
                            UPS_SigRequired_RadioBtn.IsChecked = True
                            sig = True
                            Exit For

                        Case ShippingSurcharge.IDs_UPS.Sig_Adult
                            UPS_AdultSig_RadioBtn.IsChecked = True
                            sig = True
                            Exit For

                    End Select
                Next
                If Not sig Then
                    UPS_NoSig_RadioBtn.IsChecked = True
                End If
                '
                Carrier_TabControl.SelectedIndex = 1                '


            Case "USPS"
                '
                Me.USPS_Signature_TextBox.Text = Val(ExtractElementFromSegment("ACK-S", SegmentSet)).ToString("$ 0.00")
                Me.USPS_AdultSig_TextBox.Text = Val(ExtractElementFromSegment("DELSIGADULT", SegmentSet)).ToString("$ 0.00")
                Me.CertifiedMail_TextBox.Text = Val(ExtractElementFromSegment("SATPU2", SegmentSet)).ToString("$ 0.00")
                Me.ReturnReceipt_TextBox.Text = Val(ExtractElementFromSegment("SAT2", SegmentSet)).ToString("$ 0.00")

                StoreAddress_ChkBx.Visibility = Visibility.Visible
                StoreAddress_ChkBx.IsChecked = My.Settings.Use_Store_From_Address
                gShip.Use_Store_Address = StoreAddress_ChkBx.IsChecked


                    '
                    Dim sig = False
                For Each item As ShippingSurcharge In gSelectedShipmentChoice.SurchargesList
                    Select Case item.ID
                        Case ShippingSurcharge.IDs_USPS.Sig_Conf
                            USPS_Signature_RadioBtn.IsChecked = True
                            sig = True
                            Exit For

                        Case ShippingSurcharge.IDs_USPS.Sig_Adult
                            USPS_AdultSig_RadioBtn.IsChecked = True
                            sig = True
                            Exit For
                    End Select
                Next
                If Not sig Then
                    USPS_NoSig_RadioBtn.IsChecked = True
                End If

                If _USPS.IsAvailable_CertifiedMail(gSelectedShipmentChoice.Service) Then
                    USPS_CertifiedMail_CheckBox.IsEnabled = True
                Else
                    USPS_CertifiedMail_CheckBox.IsEnabled = False
                End If

                If _USPS.IsAvailable_ReturnReceipt(gSelectedShipmentChoice.Service) Then
                    USPS_ReturnReceipt_CheckBox.IsEnabled = True
                Else
                    USPS_ReturnReceipt_CheckBox.IsEnabled = False
                End If

                Carrier_TabControl.SelectedIndex = 2

            Case "SPEE-DEE"
                Carrier_TabControl.SelectedIndex = 4

        End Select


    End Sub

    Private Sub ShipPackage_Button_Click(sender As Object, e As RoutedEventArgs) Handles ShipPackage_Button.Click
        If FedEx_DateCertain_RadioBtn.IsChecked Then
            If FedEx_DateCertain_DatePicker.Text <> "" Then
                gShip.HOMEFedEXDeliveryDate = Me.FedEx_DateCertain_DatePicker.SelectedDate
            Else
                MsgBox("FedEx Date Certain delivery is selected, but no delivery date is specified. Please select desired delivery date!")
                Exit Sub
            End If
        End If
        gShip.FedEx_EmailNotification_Email = FedEx_OtherEmail_TxtBx.Text
        gPackageShipped = True
        Me.Close()

    End Sub

    Private Sub AncillaryCharges(sender As Object, e As RoutedEventArgs) Handles Fedex_IndirectSig_RadioBtn.Checked, FedEx_DirectSig_RadioBtn.Checked, FedEx_AdultSig_RadioBtn.Checked, FedEx_NoSig_RadioBtn.Checked, FedEx_NonStandardCont_CheckBox.Click,
                                                                                 FedEx_DryIce_CheckBox.Click, FedEx_HoldAtLocation_CheckBox.Checked, FedEx_Appointment_RadioBtn.Checked, FedEx_Evening_RadioBtn.Checked, FedEx_NoPremium_RadioBtn.Checked,
                                                                                 FedEx_DateCertain_RadioBtn.Checked, FedEx_InsideDelivery_CheckBox.Click, FedEx_InsidePickup_CheckBox.Click,
                                                                                 UPS_DeliveryConfirm_RadioBtn.Checked, UPS_SigRequired_RadioBtn.Checked, UPS_AdultSig_RadioBtn.Checked, UPS_NoSig_RadioBtn.Checked,
                                                                                 USPS_Signature_RadioBtn.Checked, USPS_AdultSig_RadioBtn.Checked, USPS_NoSig_RadioBtn.Checked, USPS_CertifiedMail_CheckBox.Click, USPS_ReturnReceipt_CheckBox.Click
        Try

            If sender.name = "FedEx_HoldAtLocation_CheckBox" Then
                FedEx_Load_HAL_ListOfLocations()
            End If

            Update_Description(sender.name)

            'changing signature options in print label screen, will also change the selection instantly in the shipping screen
            Dim ShipMngr_Win As Window = CommonWindowStack.windowList.Find(Function(x As CommonWindow) x.Name = "ShipManager_Window")

            If gSelectedShipmentChoice.Carrier = "FedEx" Then
                ShipMngr_Win.FindName("DelConf_Btn").isChecked = Fedex_IndirectSig_RadioBtn.IsChecked
                ShipMngr_Win.FindName("SigConfirm_Btn").isChecked = FedEx_DirectSig_RadioBtn.IsChecked
                ShipMngr_Win.FindName("AdultSig_Btn").isChecked = FedEx_AdultSig_RadioBtn.IsChecked

                If FedEx_NoSig_RadioBtn.IsChecked Then
                    gShip.SignatureType = 0
                Else
                    gShip.SignatureType = -1
                End If

                If Not FedEx_DateCertain_RadioBtn.IsChecked Then
                    gShip.HOMEFedEXDeliveryDate = Nothing
                    FedEx_DateCertain_DatePicker.Text = ""
                    FedEx_DateCertain_DatePicker.IsDropDownOpen = False
                ElseIf IsNothing(Me.FedEx_DateCertain_DatePicker.SelectedDate) Then
                    MsgBox("Please select desired delivery date!", vbOKOnly + vbExclamation)
                    FedEx_DateCertain_DatePicker.IsDropDownOpen = True
                End If

                gShip.NonStandardContainer = FedEx_NonStandardCont_CheckBox.IsChecked

                If FedEx_DryIce_CheckBox.IsChecked Then gShip.DryIceValue = Val(Me.FedEx_DryIce_TextBox.Text)

                gShip.InsideDelivery = FedEx_InsideDelivery_CheckBox.IsChecked
                gShip.InsidePickup = FedEx_InsidePickup_CheckBox.IsChecked


            ElseIf gSelectedShipmentChoice.Carrier = "UPS" Then

                ShipMngr_Win.FindName("DelConf_Btn").isChecked = UPS_DeliveryConfirm_RadioBtn.IsChecked
                ShipMngr_Win.FindName("SigConfirm_Btn").isChecked = UPS_SigRequired_RadioBtn.IsChecked
                ShipMngr_Win.FindName("AdultSig_Btn").isChecked = UPS_AdultSig_RadioBtn.IsChecked



            ElseIf gSelectedShipmentChoice.Carrier = "USPS" Then

                ShipMngr_Win.FindName("SigConfirm_Btn").isChecked = USPS_Signature_RadioBtn.IsChecked
                ShipMngr_Win.FindName("AdultSig_Btn").isChecked = USPS_AdultSig_RadioBtn.IsChecked

                'turn off any signatures if certified mail is selected.
                If USPS_CertifiedMail_CheckBox.IsChecked = True Then
                    If USPS_AdultSig_RadioBtn.IsChecked = True Or USPS_Signature_RadioBtn.IsChecked = True Then
                        USPS_NoSig_RadioBtn.IsChecked = True
                    End If
                End If

                gShip.IsCertifiedMail = USPS_CertifiedMail_CheckBox.IsChecked
                gShip.IsReturnReceipt = USPS_ReturnReceipt_CheckBox.IsChecked

            End If


            Check_Surcharge_Rules(gSelectedShipmentChoice)

            ShipManager.Calculate_Total(gSelectedShipmentChoice)
            ShipManager.Check_Rounding_Option(gSelectedShipmentChoice)

            TotalPrice_TxtBox.Text = FormatCurrency(gSelectedShipmentChoice.TotalSell)

            Call ShowPackageDetails()

        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to add surcharge...")
        End Try
    End Sub


    Private Sub ShowPackageDetails()
        ShipManager.Load_ShowPackageDetails_LV(ShowPackageDetails_LV, gSelectedShipmentChoice)
    End Sub


    Private Sub Endicia_PrintTestLabel_Button_Checked(sender As Object, e As RoutedEventArgs) Handles Endicia_PrintTestLabel_Button.Checked, Endicia_PrintTestLabel_Button.Unchecked
        gShip.TestShipment = sender.IsChecked
    End Sub

    Private Function Endicia_GetAccountStatus() As Boolean
        Endicia_GetAccountStatus = False
        '
        If _EndiciaWeb.EndiciaWeb_IsEnabled Then
            '
            Dim response As New Endicia_LabelService.AccountStatusResponse
            If _EndiciaWeb.Request_AccountStatus(response) Then
                If response.CertifiedIntermediary IsNot Nothing Then
                    Me.Endicia_CurrentBalance_TextBox.Content = response.CertifiedIntermediary.PostageBalance.ToString("C")
                End If
                '
                Return True
            End If
        End If
        '
    End Function
    Private Sub Endicia_BuyPostage_Button_Click(sender As Object, e As RoutedEventArgs) Handles Endicia_BuyPostage_Button.Click
        Try
            Me.Endicia_BuyPostage_Button.IsEnabled = False
            '
            If Endicia_GetAccountStatus() Then
                If MessageBoxResult.Yes = MessageBox.Show("Your current balance: " & Me.Endicia_CurrentBalance_TextBox.Content & vbCr_ & vbCr_ & "Do you want to buy more postage?", EndiciaLavelServer, MessageBoxButton.YesNo, MessageBoxImage.Question) Then
                    '
                    Dim request As New Endicia_LabelService.RecreditRequest
                    Dim response As New Endicia_LabelService.RecreditRequestResponse
                    '
                    Dim amount2add As Double = My.Settings.Endicia_LastPostageAmountAdded

                    Dim Input As String = InputBox("Please enter amount of postage to purchase.", "Purchase Postage", amount2add)
                    If Not IsNumeric(Input) Then
                        MsgBox("Invalid Entry. Make sure to enter a valid number!", vbExclamation)
                        Exit Sub
                    Else
                        amount2add = CDbl(Input)
                    End If


                    If 0 = amount2add Then amount2add = 10 '$ minimum you can buy
                    request.RecreditAmount = amount2add
                    If _EndiciaWeb.Request_Recredit(request, response) Then
                        If response.CertifiedIntermediary IsNot Nothing Then
                            Me.Endicia_CurrentBalance_TextBox.Content = response.CertifiedIntermediary.PostageBalance.ToString("C")
                            My.Settings.Endicia_LastPostageAmountAdded = amount2add
                        End If
                        'MessageBox.Show("Your account was recredited Successfully!", EndiciaLavelServer, MessageBoxButton.OK, MessageBoxImage.Information)
                    End If
                    '
                End If
            End If
            '
        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to add money to your account...")
        Finally : Me.Endicia_BuyPostage_Button.IsEnabled = True
        End Try
    End Sub

    Private Sub ManualLabel_Button_Click(sender As Object, e As RoutedEventArgs) Handles ManualLabel_Button.Click

        If ManualLabel_TxtBx.Text <> "" Then
            gPackageShipped = True
            ShipManager.PrintLabelScreen_Return = "ManualLabel_" & ManualLabel_TxtBx.Text
            Me.Close()
        Else
            MsgBox("Please enter in tracking number!", vbOKOnly + vbExclamation)
            ManualLabel_TxtBx.Focus()
        End If


    End Sub

    Private Sub BatchLabel_Button_Click(sender As Object, e As RoutedEventArgs) Handles BatchLabel_Button.Click
        gPackageShipped = True
        ShipManager.PrintLabelScreen_Return = "Batch_Label"
        Me.Close()
    End Sub

    Private Sub Window_KeyDown(sender As Object, e As KeyEventArgs)
        ShortcutKeyHandlers.KeyDown(sender, e, Me)
    End Sub

    Private Sub FedEx_DryIce_TextBox_LostFocus(sender As Object, e As RoutedEventArgs) Handles FedEx_DryIce_TextBox.LostFocus
        If FedEx_DryIce_CheckBox.IsChecked Then gShip.DryIceValue = Val(Me.FedEx_DryIce_TextBox.Text)
    End Sub

    Private Sub CloseHALPopup_Btn_Click(sender As Object, e As RoutedEventArgs) Handles CloseHALPopup_Btn.Click
        HAL_Popup.IsOpen = False
        FedEx_HoldAtLocation_CheckBox.IsChecked = False
    End Sub

    Private Sub FedEx_Load_HAL_ListOfLocations()
        If GetPolicyData(gShipriteDB, "FedExREST_Enabled", "False") = False Then Exit Sub

        Me.Cursor = Cursors.Wait
        Dim HAL_List As List(Of FXR_LocationDetailList) = FedEx_REST.FXR_GetListOf_HAL_Locations(_Contact.ShipToContact)
        Me.Cursor = Cursors.Arrow

        If Not IsNothing(HAL_List) Then
            HAL_LB.ItemsSource = HAL_List
            HAL_LB.Items.Refresh()
            HAL_Popup.IsOpen = True
        Else
            FedEx_HoldAtLocation_CheckBox.IsChecked = False
        End If

    End Sub

    Private Sub HAL_SelectAddress_Btn_Click(sender As Object, e As RoutedEventArgs) Handles HAL_SelectAddress_Btn.Click
        HAL_SelectAddress()
    End Sub

    Private Sub HAL_SelectAddress()
        If HAL_LB.SelectedIndex = -1 Then Exit Sub

        gShip.HoldAtLocationID = HAL_LB.SelectedItem.locationId
        HAL_Popup.IsOpen = False
    End Sub

    Private Sub StoreAddress_ChkBx_Checked(sender As Object, e As RoutedEventArgs) Handles StoreAddress_ChkBx.Checked, StoreAddress_ChkBx.Unchecked
        My.Settings.Use_Store_From_Address = StoreAddress_ChkBx.IsChecked

        Dim storeowner As _baseContact = New _baseContact
        Setup_GetAddress_StoreOwner(storeowner)

        If StoreAddress_ChkBx.IsChecked Then
            From_TxtBox.Text = storeowner.CompanyName & vbCrLf & storeowner.Addr1 & vbCrLf & storeowner.Addr2 & vbCrLf & storeowner.CityStateZip & vbCrLf & storeowner.Tel
        Else
            From_TxtBox.Text = General.CreateDisplayBlock(gShipperSegment, True)
        End If

        gShip.Use_Store_Address = StoreAddress_ChkBx.IsChecked
    End Sub
End Class
