Imports System.Data
Public Class ShippingHoliday
    Public Property ID As Long
    Public Property ApplicableDate As Date
    Public Property AppliesTo As String
    Public Property HolidayName As String
    Public Property Status As String
    Public Property Carrier_List As List(Of String)
End Class
Public Class ShippingSetup
    Inherits CommonWindow

    Private Display_CarrierList As List(Of Carrier)
    Private Holiday_List As List(Of ShippingHoliday)


#Region "Form"
    Public Sub New()

        MyBase.New()

        ' This call is required by the designer.
        InitializeComponent()

        ShipSetup_ListBox.SelectedIndex = 0

        'makes tab headers not visible in run time. 
        For Each currentTab As TabItem In ShipSetup_TabControl.Items
            currentTab.Visibility = Visibility.Collapsed
        Next
    End Sub

    Public Sub New(ByVal callingWindow As Window, Optional ByRef TabNo As Integer = 0)

        MyBase.New(callingWindow)

        ' This call is required by the designer.
        InitializeComponent()
        ShipSetup_ListBox.SelectedIndex = TabNo

        'makes tab headers not visible in run time. 
        For Each currentTab As TabItem In ShipSetup_TabControl.Items
            currentTab.Visibility = Visibility.Collapsed
        Next
    End Sub
    Private Sub ShippingSetup_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded

        If GetPolicyData(gShipriteDB, "EnableShipsurance") = "True" Then

            EnableShipsurance.IsChecked = True

        End If
        If GetPolicyData(gShipriteDB, "EnableShipAndInsure") = "True" Then

            EnableShipAndInsure.IsChecked = True

        End If

    End Sub

#End Region

    Private Sub ShipOptions_ListBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles ShipSetup_ListBox.SelectionChanged
        ShipSetup_TabControl.SelectedIndex = ShipSetup_ListBox.SelectedIndex
        Select Case ShipSetup_TabControl.SelectedIndex
            Case 0
                'Call Load_Carrier_Panels()
                Domestic_RdioBtn.IsChecked = True

            Case 1 'Shipping Holidays
                Load_ShippingHolidays()

            Case 2 ' Third Party Insurance
                Call DSI_Load_Settings()
                Me.ThirdPartyInsuranceName_TextBox.Text = General.GetPolicyData(gShipriteDB, "ThirdPartyAddress")
                Me.ShipandInsure_UserID_TxtBx.Text = General.GetPolicyData(gShipriteDB, "ShipAndInsureUserID")
                Me.ShipandInsure_Password_PWBx.Password = General.GetPolicyData(gShipriteDB, "ShipAndInsurePassword")

            Case 3 'General Shipping Options
                Load_General_Options()


            Case 4 ' Pack Master
                Call PackMasterSetup_Load()

        End Select
    End Sub
    Private Sub SaveButton_Click(sender As Object, e As RoutedEventArgs) Handles SaveButton.Click

        Try
            If MsgBox("Do you want to save changes to " & (CType(ShipSetup_ListBox.SelectedValue, ListBoxItem)).Content.ToString() & "?", vbQuestion + vbYesNo) = MsgBoxResult.Yes Then

                Select Case ShipSetup_TabControl.SelectedIndex

                    Case 0 'Shipping Panels Setup
                        Save_Carrier_Panels()

                    Case 1 'Shipping Holidays
                        Save_ShippingHolidays()

                    Case 2 ' Third Party Insurance

                        If EnableShipsurance.IsChecked Then
                            DSI.gDSIis3rdPartyInsurance = DSI_Save_Settings()
                        End If

                        gThirdPartyInsurance = (DSI.gDSIis3rdPartyInsurance Or Not String.IsNullOrEmpty(Me.ThirdPartyInsuranceName_TextBox.Text))

                        Call General.UpdatePolicy(gShipriteDB, "ThirdPartyInsurance", gThirdPartyInsurance)
                        Call General.UpdatePolicy(gShipriteDB, "ThirdPartyAddress", Me.ThirdPartyInsuranceName_TextBox.Text)
                        Call General.UpdatePolicy(gShipriteDB, "ShipAndInsureUserID", Me.ShipandInsure_UserID_TxtBx.Text)
                        Call General.UpdatePolicy(gShipriteDB, "ShipAndInsurePassword", Me.ShipandInsure_Password_PWBx.Password)

                    Case 3 'General Shipping Options
                        Save_General_Options()


                    Case 4 ' Pack Master

                        Call PackMaster_Save_Click()

                End Select

            End If
        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to save settings...")
        End Try

    End Sub

#Region "General Shipping Options"
    Private Sub Load_General_Options()
        Auto_TinT_CheckBx.IsChecked = GetPolicyData(gShipriteDB, "Enable_Auto_TimeInTransit")
        PricingMatrix_CheckBx.IsChecked = GetPolicyData(gShipriteDB, "Enable_Pricing_Matrix")
        ForcePOSShipping_CheckBx.IsChecked = GetPolicyData(gShipriteDB, "ForcePOSShipping")

        FROM_L1.Text = GetPolicyData(gShipriteDB, "Level1L")
        FROM_L2.Text = GetPolicyData(gShipriteDB, "Level2L")
        FROM_L3.Text = GetPolicyData(gShipriteDB, "Level3L")

        TO_L1.Text = GetPolicyData(gShipriteDB, "Level1H")
        TO_L2.Text = GetPolicyData(gShipriteDB, "Level2H")
        TO_L3.Text = GetPolicyData(gShipriteDB, "Level3H")

        AddressVerification_Cmb.SelectedIndex = GetPolicyData(gShipriteDB, "Address_Verification_Service", "0")

        Rounding_Cmb.Text = GetPolicyData(gShipriteDB, "Rounding", "")

    End Sub

    Private Sub Save_General_Options()
        UpdatePolicy(gShipriteDB, "Enable_Auto_TimeInTransit", Auto_TinT_CheckBx.IsChecked)
        UpdatePolicy(gShipriteDB, "Enable_Pricing_Matrix", PricingMatrix_CheckBx.IsChecked)
        UpdatePolicy(gShipriteDB, "Address_Verification_Service", AddressVerification_Cmb.SelectedIndex)
        UpdatePolicy(gShipriteDB, "ForcePOSShipping", ForcePOSShipping_CheckBx.IsChecked)

        If TO_L1.Text <> "" Or TO_L2.Text <> "" Then
            UpdatePolicy(gShipriteDB, "Level1L", FROM_L1.Text)
            UpdatePolicy(gShipriteDB, "Level2L", FROM_L2.Text)
            UpdatePolicy(gShipriteDB, "Level3L", FROM_L3.Text)

            UpdatePolicy(gShipriteDB, "Level1H", TO_L1.Text)
            UpdatePolicy(gShipriteDB, "Level2H", TO_L2.Text)
            UpdatePolicy(gShipriteDB, "Level3H", TO_L3.Text)
        Else
            MsgBox("Could Not save Level Pricing setup, some fields were left empty.")
        End If

        UpdatePolicy(gShipriteDB, "Rounding", Rounding_Cmb.Text)

        MsgBox("Changes to General Shipping Options Saved Successfully!", vbInformation)
    End Sub

    Private Sub TO_L_LostFocus(sender As Object, e As RoutedEventArgs) Handles TO_L1.LostFocus, TO_L2.LostFocus
        If TO_L1.Text = "" Or TO_L2.Text = "" Then Exit Sub

        FROM_L1.Text = "0.00"
        FROM_L2.Text = CDbl(TO_L1.Text) + 0.01
        FROM_L3.Text = CDbl(TO_L2.Text) + 0.01
        TO_L3.Text = "99999.99"
    End Sub

#End Region


#Region "ShippingHolidays"
    Private Sub Load_ShippingHolidays()
        Dim buf As String
        Dim current_segment As String
        Dim current_holiday As ShippingHoliday

        Holiday_List = New List(Of ShippingHoliday)


        buf = IO_GetSegmentSet(gShipriteDB, "SELECT * From Holiday WHERE Year([NormalDate]) >= Year(NOW())")

        Do Until buf = ""
            current_segment = GetNextSegmentFromSet(buf)

            current_holiday = New ShippingHoliday
            current_holiday.HolidayName = ExtractElementFromSegment("Description", current_segment, "")
            current_holiday.ApplicableDate = CDate(ExtractElementFromSegment("NormalDate", current_segment, ""))
            current_holiday.AppliesTo = ExtractElementFromSegment("AppliesTo", current_segment, "")
            current_holiday.ID = ExtractElementFromSegment("ID", current_segment, "")
            current_holiday.Carrier_List = Load_Carriers_into_List()

            Holiday_List.Add(current_holiday)
        Loop

        Holiday_List = Holiday_List.OrderBy(Function(value) value.ApplicableDate).ToList
        ShippingHolidays_LV.ItemsSource = Holiday_List


        AppliesTo_TxtBx.ItemsSource = Load_Carriers_into_List()
    End Sub

    Private Sub Save_ShippingHolidays()
        Dim SQL As String
        Try

            For Each item As ShippingHoliday In Holiday_List
                If item.HolidayName <> "" Then

                    If item.Status = "Added" Then
                        SQL = "INSERT INTO Holiday (NormalDate, AppliesTo, Description) VALUES ('" & item.ApplicableDate.ToShortDateString & "', '" & item.AppliesTo & "', '" & item.HolidayName & "')"
                        IO_UpdateSQLProcessor(gShipriteDB, SQL)

                    ElseIf item.Status = "Deleted" Then
                        SQL = "DELETE * FROM Holiday WHERE [ID]=" & item.ID
                        IO_UpdateSQLProcessor(gShipriteDB, SQL)
                    ElseIf item.Status = "Edited" Then
                        SQL = "UPDATE Holiday SET [NormalDate]=#" & item.ApplicableDate.ToShortDateString & "#, [AppliesTo]='" & item.AppliesTo & "', [Description]='" & item.HolidayName & "' WHERE [ID]=" & item.ID
                        IO_UpdateSQLProcessor(gShipriteDB, SQL)

                    End If

                End If
            Next

            MsgBox("Changes to Shipping Holidays Saved Successfully!", vbInformation)
            Load_ShippingHolidays()

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Function Load_Carriers_into_List() As List(Of String)
        Dim list As New List(Of String)

        list.Add("All Carriers")

        For Each item As Carrier In gCarrierList
            list.Add(UCase(item.CarrierName))
        Next

        Return list
    End Function

    Protected Sub SelectCurrentItem(ByVal sender As Object, ByVal e As KeyboardFocusChangedEventArgs)
        'When clicking inside a textbox, select the listiew item that the textbox belongs to.
        Dim item As ListViewItem = CType(sender, ListViewItem)
        item.IsSelected = True
    End Sub

    Private Sub HolidayName_TextBox_TextChanged()
        If ShippingHolidays_LV.SelectedIndex = -1 Then Exit Sub

        Dim item As ShippingHoliday = ShippingHolidays_LV.SelectedItem

        If item.Status <> "Added" Then
            item.Status = "Edited"
        End If

    End Sub

    Private Sub AddNewHoliday_Btn_Click(sender As Object, e As RoutedEventArgs) Handles AddNewHoliday_Btn.Click
        If HolidayName_TxtBx.Text = "" Or Not IsDate(HolidayDate_TxtBx.SelectedDate) Then
            MsgBox("Invalid entry, please try again!", vbExclamation)
            Exit Sub
        End If

        Dim item As ShippingHoliday = New ShippingHoliday

        item.Status = "Added"
        item.HolidayName = HolidayName_TxtBx.Text
        item.ApplicableDate = HolidayDate_TxtBx.SelectedDate
        item.AppliesTo = AppliesTo_TxtBx.SelectedItem
        item.Carrier_List = Load_Carriers_into_List()

        Holiday_List.Add(item)
        ShippingHolidays_LV.Items.Refresh()
        ShippingHolidays_LV.ScrollIntoView(item)

    End Sub

    Private Sub DeleteHoliday_Btn_Click(sender As Object, e As RoutedEventArgs) Handles DeleteHoliday_Btn.Click
        If ShippingHolidays_LV.SelectedIndex = -1 Then
            MsgBox("Please select a department first", MsgBoxStyle.OkOnly + MsgBoxStyle.Exclamation, "Cannot delete Department")
            Exit Sub
        End If

        Dim item As ShippingHoliday = ShippingHolidays_LV.SelectedItem

        If item.Status = "Added" Then
            Holiday_List.Remove(item)
        Else
            item.Status = "Deleted"
        End If


        ShippingHolidays_LV.Items.Refresh()
    End Sub



#End Region

#Region "Shipping Panels"
    Private Sub Save_Carrier_Panels()
        Try
            Dim checked_option As String = ""

            For Each CR As Carrier In Display_CarrierList
                If Domestic_RdioBtn.IsChecked Then
                    IO_UpdateSQLProcessor(gShipriteDB, "Update Master set Panel_Row=" & Display_CarrierList.IndexOf(CR) & ", Domestic_Status=" & CR.Status_Current & " WHERE Carrier='" & CR.CarrierName & "'")
                    checked_option = "Domestic"

                ElseIf International_RdioBtn.IsChecked Then
                    IO_UpdateSQLProcessor(gShipriteDB, "Update Master set Panel_Row_Intl=" & Display_CarrierList.IndexOf(CR) & ", Intl_Status=" & CR.Status_Current & " WHERE Carrier='" & CR.CarrierName & "'")
                    checked_option = "International"

                ElseIf Canada_RdioBtn.IsChecked Then
                    IO_UpdateSQLProcessor(gShipriteDB, "Update Master set Panel_Row_Canada=" & Display_CarrierList.IndexOf(CR) & ", Canada_Status=" & CR.Status_Current & " WHERE Carrier='" & CR.CarrierName & "'")
                    checked_option = "Canadian"

                ElseIf Freight_RdioBtn.IsChecked Then
                    IO_UpdateSQLProcessor(gShipriteDB, "Update Master set Panel_Row_Freight=" & Display_CarrierList.IndexOf(CR) & ", Freight_Status=" & CR.Status_Current & " WHERE Carrier='" & CR.CarrierName & "'")
                    checked_option = "Freight"
                End If


                For Each svc In CR.ServiceList
                    If svc.service <> "" Then
                        If Canada_RdioBtn.IsChecked Then
                            IO_UpdateSQLProcessor(gShipriteDB, "Update Master set Panel_Column_Canada=" & CR.ServiceList.IndexOf(svc) & " WHERE Service='" & svc.service & "'")
                        Else
                            IO_UpdateSQLProcessor(gShipriteDB, "Update Master set Panel_Column=" & CR.ServiceList.IndexOf(svc) & " WHERE Service='" & svc.service & "'")
                        End If
                    End If
                Next
            Next


            MsgBox("Changes to " & checked_option & " Panel saved successfully!", vbInformation)

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub


    Private Sub ShippingPanel_RdioBtn_Checked(sender As Object, e As RoutedEventArgs) Handles Domestic_RdioBtn.Checked, Canada_RdioBtn.Checked, Freight_RdioBtn.Checked, International_RdioBtn.Checked
        If Freight_RdioBtn.IsChecked Then
            Load_Shipping_Panel("Freight")

        ElseIf International_RdioBtn.IsChecked Then
            Load_Shipping_Panel("Intl")

        ElseIf Canada_RdioBtn.IsChecked Then
            Load_Shipping_Panel("Canada")

        ElseIf Domestic_RdioBtn.IsChecked Then
            Load_Shipping_Panel("Domestic")
        End If

        For Each item As Carrier In gCarrierList
            For Each svc In item.ServiceList
                Definitions_Shipping.Set_Ship_Button_Color(svc)
            Next
        Next

        'if carrier doesn't have any services, then don't show it
        Display_CarrierList = gCarrierList.Where(Function(x As Carrier) x.ServiceList.Count > 0).ToList
        Carrier_IC.ItemsSource = Display_CarrierList
    End Sub


    Private Sub Left_Btn_Click(sender As Object, e As RoutedEventArgs)
        Try
            Dim SelectedService As ShippingChoiceDefinition = sender.tag
            Dim index As Integer
            Dim isFound As Boolean = False

            For Each item As Carrier In Display_CarrierList
                For Each S As ShippingChoiceDefinition In item.ServiceList
                    If S.Service = SelectedService.Service Then

                        index = item.ServiceList.IndexOf(S)
                        If index > 0 Then
                            item.ServiceList.Remove(S)
                            item.ServiceList.Insert(index - 1, SelectedService)
                            isFound = True
                            Exit For
                        End If
                    End If
                Next

                If isFound = True Then Exit For
            Next

            Carrier_IC.Items.Refresh()

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Right_Btn_Click(sender As Object, e As RoutedEventArgs)
        Try
            Dim SelectedService As ShippingChoiceDefinition = sender.tag
            Dim index As Integer
            Dim isFound As Boolean = False

            For Each item As Carrier In Display_CarrierList
                For Each S As ShippingChoiceDefinition In item.ServiceList
                    If S.Service = SelectedService.Service Then

                        index = item.ServiceList.IndexOf(S)
                        If index < 6 Then
                            item.ServiceList.Remove(S)
                            item.ServiceList.Insert(index + 1, SelectedService)
                            isFound = True
                            Exit For
                        End If
                    End If
                Next
                If isFound = True Then Exit For
            Next

            Carrier_IC.Items.Refresh()

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Up_Btn_Click(sender As Object, e As RoutedEventArgs)
        Try
            Dim Selected_Carrier As Carrier = sender.tag
            Dim index As Integer

            For Each item As Carrier In Display_CarrierList
                If Selected_Carrier.CarrierName = item.CarrierName Then
                    index = Display_CarrierList.IndexOf(item)

                    If index > 0 Then
                        Display_CarrierList.Remove(item)
                        Display_CarrierList.Insert(index - 1, Selected_Carrier)
                        Exit For
                    End If

                End If
            Next

            Carrier_IC.Items.Refresh()
        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Down_Btn_Click(sender As Object, e As RoutedEventArgs)
        Try
            Dim Selected_Carrier As Carrier = sender.tag
            Dim index As Integer

            For Each item As Carrier In Display_CarrierList
                If Selected_Carrier.CarrierName = item.CarrierName Then
                    index = Display_CarrierList.IndexOf(item)

                    If index < Display_CarrierList.Count - 1 Then
                        Display_CarrierList.Remove(item)
                        Display_CarrierList.Insert(index + 1, Selected_Carrier)
                        Exit For
                    End If

                End If
            Next

            Carrier_IC.Items.Refresh()

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub




#End Region


#Region "Third Party Insurance"

    Private Function DSI_Save_Settings() As Boolean

        DSI_Save_Settings = False

        '761 tc - Added the code below to allow the end sure to leave the account # blank.
        If Not (Me.DSI_Policy_ID_TextBox.Text = "" And Me.DSI_Email_Address_TextBox.Text = "") Then

            DSI_Save_Settings = DSI_Verify()

        End If

        If (DSI_Save_Settings = False) And Not (DSI_Policy_ID_TextBox.Text = "" And DSI_Email_Address_TextBox.Text = "") Then

            Exit Function

        End If

        Call General.UpdatePolicy(gShipriteDB, "DSI_PolicyID", DSI_Policy_ID_TextBox.Text)
        Call General.UpdatePolicy(gShipriteDB, "DSI_Email", DSI_Email_Address_TextBox.Text)
        Call General.UpdatePolicy(gShipriteDB, "DSISigThreshold", DSI_Signature_Threshold_TextBox.Text)
        ' To Do:
        ' "Member of DSI Premiere Program" check box was added to the DSI Insurance Setup screen.
        'Call General.UpdatePolicy(gShipriteDB, "DSI_PremiereProgramMember", Me.DSI_PremiereProgramMember_CheckBox.IsChecked)

        MsgBox(DSI_NewName & " settings were saved successfully!", vbInformation)

        'Unload Me

    End Function

    Private Function DSI_Verify() As Boolean
        DSI_Verify = False

        Try

            Dim PostData As String = String.Empty
            Dim url As String = String.Empty
            Dim Headers As String = String.Empty

            Dim vtData As String = String.Empty

            ' To Do:
            'lblDescription.Caption = "Verifying account with " & DSI.DSI_NewName & "."

            If _Debug.IsINHOUSE Then
                url = "https://sandbox.dsiins.com/api.net/dsi_Validation.aspx" '' test url
            Else
                url = "https://www.dsiins.com/api.net/dsi_Validation.aspx" ''dsiinsurance
            End If

            Headers = "Content-Type: application/x-www-form-urlencoded" & vbCrLf

            PostData = "extPersonSourceId=6"
            PostData = PostData & "&sourceUsername=shiprite"
            PostData = PostData & "&sourcePassword=mdrit7"
            PostData = PostData & "&extPolicyId=" & DSI_Policy_ID_TextBox.Text
            PostData = PostData & "&personEmail=" & DSI_Email_Address_TextBox.Text
            PostData = PostData & "&validationType=validPolicy"

            'MsgBox (PostData)

            'DSI_Ver_Com.RequestTimeout = 60
            'DSI_Ver_Com.Protocol = icHTTPS
            'DSI_Ver_Com.Execute url & "?" & PostData, "POST", "", Headers

            'vtData = DSI_Ver_Com.GetChunk(1024, icString)

            If Not _XML.Send_HttpWebRequest(url & "?" & PostData, vtData) Then

                MsgBox("No response from server. " & vtData, vbExclamation, DSI.DSI_NewName)
                Exit Function

            End If

            'MsgBox vtData

            Dim SplitResponse() As String = vtData.Split(",")

            If SplitResponse(0) = "1" Then
                MsgBox("Your " & DSI.DSI_NewName & " account has been Verified.", vbInformation)
                DSI_Verify = True
            Else
                MsgBox(SplitResponse(1), vbExclamation, DSI.DSI_NewName)
                DSI_Verify = False
            End If

            ' To Do:
            'lblDescription.Caption = vbNullString

        Catch ex As Exception : MsgBox(ex.Message, vbExclamation, "Error")

        End Try

    End Function

    Private Sub DSI_Load_Settings()

        DSI_Policy_ID_TextBox.Text = General.GetPolicyData(gShipriteDB, "DSI_PolicyID")
        If String.IsNullOrEmpty(DSI_Policy_ID_TextBox.Text) Then
            DSI_Policy_ID_TextBox.Text = "Contact " & DSI.DSI_NewName
        End If

        DSI_Email_Address_TextBox.Text = General.GetPolicyData(gShipriteDB, "DSI_Email")
        If String.IsNullOrEmpty(DSI_Email_Address_TextBox.Text) Then
            DSI_Email_Address_TextBox.Text = "Contact " & DSI.DSI_NewName
        End If

        DSI_Signature_Threshold_TextBox.Text = General.GetPolicyData(gShipriteDB, "DSISigThreshold")
        If 0 = Val(DSI_Email_Address_TextBox.Text) Then
            DSI_Signature_Threshold_TextBox.Text = "1000"
        End If

        ' To Do:
        ' "Member of DSI Premiere Program" check box was added to the DSI Insurance Setup screen.
        'Me.DSI_PremiereProgramMember_CheckBox.IsChecked = General.GetPolicyData(gShipriteDB, "DSI_PremiereProgramMember")


    End Sub

#End Region

#Region "Pack Master"

    Private Sub PackMasterSetup_Load()
        Try
            Call load_Setup1()
            Call load_Setup2()
            Call cear_Tab_Fragility()
            Call load_Fragility()
            Call dtlPackMaterials_AddValues()

            PM_Default_Border.Visibility = Visibility.Visible
            PM_Modified_Border.Visibility = Visibility.Hidden

        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to load Pack Master Setup...")
        End Try
    End Sub

    Private Sub txtFillSKU1_GotFocus(sender As Object, e As RoutedEventArgs) Handles txtFillSKU1.GotFocus, txtFillSKU2.GotFocus, txtFillSKU3.GotFocus, txtFillSKU4.GotFocus, txtFillSKU5.GotFocus, txtWrapSKU1.GotFocus, txtWrapSKU2.GotFocus, txtWrapSKU3.GotFocus, txtWrapSKU4.GotFocus, txtWrapSKU5.GotFocus, txtLaborSKU1.GotFocus, txtLaborSKU2.GotFocus, txtLaborSKU3.GotFocus, txtLaborSKU4.GotFocus, txtLaborSKU5.GotFocus

        Try
            InventoryLIstViewPopup.IsOpen = False
            '
            Dim obj As TextBox = CType(sender, TextBox)
            Me.grbFragility.Tag = obj ' hold the text box that was clicked
            find_cmbRegDesc_byINDEX(obj.Name, obj)
            InventoryLIstViewPopup.PlacementTarget = sender
            InventoryLIstViewPopup.IsOpen = True
        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to load Fill SKU...")
        End Try

    End Sub

#Region "Load Setup"
    Private Function load_Setup1() As Boolean
        Me.txtDoubleBoxing.Text = General.GetPolicyData(gShipriteDB, "DoubleBoxThreshold", "0")
        Return True
    End Function
    Private Function load_Setup2() As Boolean
        ' regular tab:
        Me.txtDefaultLabor.Text = General.GetPolicyData(gShipriteDB, "defaultLabor", "0")
        Me.txtFillCushion.Text = General.GetPolicyData(gShipriteDB, "defaultFill", "0")
        Me.txtPieceCharge.Text = General.GetPolicyData(gShipriteDB, "defaultPieceCharge", "0")

        ' Modified tab:
        Me.txtLabor_AddTop.Text = General.GetPolicyData(gShipriteDB, _ReusedField.fldLabor_AddTop, "0")
        Me.txtLabor_BuildUp.Text = General.GetPolicyData(gShipriteDB, _ReusedField.fldLabor_BuildUp, "0")
        Me.txtLabor_CutDown.Text = General.GetPolicyData(gShipriteDB, _ReusedField.fldLabor_CutDown, "0")
        Me.txtLabor_Telescope.Text = General.GetPolicyData(gShipriteDB, _ReusedField.fldLabor_Telescope, "0")

        Return True
    End Function
#End Region

#Region "Save Setup"
    Private Function save_Setup1() As Boolean
        Return 0 < General.UpdatePolicy(gShipriteDB, "DoubleBoxThreshold", Me.txtDoubleBoxing.Text)
    End Function
    Private Function save_Setup2() As Boolean
        General.UpdatePolicy(gShipriteDB, "defaultLabor", Me.txtDefaultLabor.Text)
        General.UpdatePolicy(gShipriteDB, "defaultFill", Me.txtFillCushion.Text)
        General.UpdatePolicy(gShipriteDB, "defaultPieceCharge", Me.txtPieceCharge.Text)
        General.UpdatePolicy(gShipriteDB, _ReusedField.fldLabor_AddTop, Me.txtLabor_AddTop.Text)
        General.UpdatePolicy(gShipriteDB, _ReusedField.fldLabor_BuildUp, Me.txtLabor_BuildUp.Text)
        General.UpdatePolicy(gShipriteDB, _ReusedField.fldLabor_CutDown, Me.txtLabor_CutDown.Text)
        General.UpdatePolicy(gShipriteDB, _ReusedField.fldLabor_Telescope, Me.txtLabor_Telescope.Text)
        Return True
    End Function
    Private Function save_FragilityItem(ByVal itemname As String) As Boolean
        Dim sql2cmd As New sqlUpdate
        Dim sql2exe As String = String.Empty
        If "FillUnit" = itemname Then
            Call sql2cmd.Qry_UPDATE("Fragile_L1", Me.txtFillUnit1.Text, sql2cmd.TXT_, True, False, "PackMasterFragility", "ItemName='" & itemname & "'")
            Call sql2cmd.Qry_UPDATE("Fragile_L2", Me.txtFillUnit2.Text, sql2cmd.TXT_)
            Call sql2cmd.Qry_UPDATE("Fragile_L3", Me.txtFillUnit3.Text, sql2cmd.TXT_)
            Call sql2cmd.Qry_UPDATE("Fragile_L4", Me.txtFillUnit4.Text, sql2cmd.TXT_)
            sql2exe = sql2cmd.Qry_UPDATE("Fragile_L5", Me.txtFillUnit5.Text, sql2cmd.TXT_, False, True)
        ElseIf "FillSKU" = itemname Then
            Call sql2cmd.Qry_UPDATE("Fragile_L1", Me.txtFillSKU1.Text, sql2cmd.TXT_, True, False, "PackMasterFragility", "ItemName='" & itemname & "'")
            Call sql2cmd.Qry_UPDATE("Fragile_L2", Me.txtFillSKU2.Text, sql2cmd.TXT_)
            Call sql2cmd.Qry_UPDATE("Fragile_L3", Me.txtFillSKU3.Text, sql2cmd.TXT_)
            Call sql2cmd.Qry_UPDATE("Fragile_L4", Me.txtFillSKU4.Text, sql2cmd.TXT_)
            sql2exe = sql2cmd.Qry_UPDATE("Fragile_L5", Me.txtFillSKU5.Text, sql2cmd.TXT_, False, True)
        ElseIf "WrapUnit" = itemname Then
            Call sql2cmd.Qry_UPDATE("Fragile_L1", Me.txtWrapUnit1.Text, sql2cmd.TXT_, True, False, "PackMasterFragility", "ItemName='" & itemname & "'")
            Call sql2cmd.Qry_UPDATE("Fragile_L2", Me.txtWrapUnit2.Text, sql2cmd.TXT_)
            Call sql2cmd.Qry_UPDATE("Fragile_L3", Me.txtWrapUnit3.Text, sql2cmd.TXT_)
            Call sql2cmd.Qry_UPDATE("Fragile_L4", Me.txtWrapUnit4.Text, sql2cmd.TXT_)
            sql2exe = sql2cmd.Qry_UPDATE("Fragile_L5", Me.txtWrapUnit5.Text, sql2cmd.TXT_, False, True)
        ElseIf "WrapSKU" = itemname Then
            Call sql2cmd.Qry_UPDATE("Fragile_L1", Me.txtWrapSKU1.Text, sql2cmd.TXT_, True, False, "PackMasterFragility", "ItemName='" & itemname & "'")
            Call sql2cmd.Qry_UPDATE("Fragile_L2", Me.txtWrapSKU2.Text, sql2cmd.TXT_)
            Call sql2cmd.Qry_UPDATE("Fragile_L3", Me.txtWrapSKU3.Text, sql2cmd.TXT_)
            Call sql2cmd.Qry_UPDATE("Fragile_L4", Me.txtWrapSKU4.Text, sql2cmd.TXT_)
            sql2exe = sql2cmd.Qry_UPDATE("Fragile_L5", Me.txtWrapSKU5.Text, sql2cmd.TXT_, False, True)
        ElseIf "LaborUnit" = itemname Then
            Call sql2cmd.Qry_UPDATE("Fragile_L1", Me.txtLaborUnit1.Text, sql2cmd.TXT_, True, False, "PackMasterFragility", "ItemName='" & itemname & "'")
            Call sql2cmd.Qry_UPDATE("Fragile_L2", Me.txtLaborUnit2.Text, sql2cmd.TXT_)
            Call sql2cmd.Qry_UPDATE("Fragile_L3", Me.txtLaborUnit3.Text, sql2cmd.TXT_)
            Call sql2cmd.Qry_UPDATE("Fragile_L4", Me.txtLaborUnit4.Text, sql2cmd.TXT_)
            sql2exe = sql2cmd.Qry_UPDATE("Fragile_L5", Me.txtLaborUnit5.Text, sql2cmd.TXT_, False, True)
        ElseIf "LaborSKU" = itemname Then
            Call sql2cmd.Qry_UPDATE("Fragile_L1", Me.txtLaborSKU1.Text, sql2cmd.TXT_, True, False, "PackMasterFragility", "ItemName='" & itemname & "'")
            Call sql2cmd.Qry_UPDATE("Fragile_L2", Me.txtLaborSKU2.Text, sql2cmd.TXT_)
            Call sql2cmd.Qry_UPDATE("Fragile_L3", Me.txtLaborSKU3.Text, sql2cmd.TXT_)
            Call sql2cmd.Qry_UPDATE("Fragile_L4", Me.txtLaborSKU4.Text, sql2cmd.TXT_)
            sql2exe = sql2cmd.Qry_UPDATE("Fragile_L5", Me.txtLaborSKU5.Text, sql2cmd.TXT_, False, True)
        End If
        save_FragilityItem = (0 < DatabaseFunctions.IO_UpdateSQLProcessor(gShipriteDB, sql2exe))
    End Function
#End Region

#Region "Buttons"
    'Private Sub cmdExit_Click(sender As Object, e As System.EventArgs) Handles BackButton.Click
    '    Try
    '        Me.Close()
    '    Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to unload Pack Master Setup...")
    '    End Try
    'End Sub
    Private Sub PackMaster_Save_Click()
        Try
            If save_Setup1() Then
                If save_Setup2() Then
                    If save_FragilityItem("FillUnit") Then
                        If save_FragilityItem("FillSKU") Then
                            If save_FragilityItem("WrapUnit") Then
                                If save_FragilityItem("WrapSKU") Then
                                    If save_FragilityItem("LaborUnit") Then
                                        If save_FragilityItem("LaborSKU") Then
                                            _MsgBox.SavedSuccessfully("Pack Master Settings")
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    End If
                End If
            End If
        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to save Pack Master Setup settings...")
        End Try
    End Sub
#End Region

#Region "Tab Fragility"
    Public Shared Function dtlPackMaterials_AddValues() As Boolean
        dtlPackMaterials_AddValues = False
        If ShipRiteDb.Load_DataSet_Inventory_PackMaterials(ShipRiteDb.tblPackMaterials) Then
            If ShipRiteDb.Get_DataTable(ShipRiteDb.tblPackMaterials, dtlPackMaterials) Then
                '' Use the ADO Filter property and the Clone method. This allows you to find the correct bookmark in the clone without affecting the rows that are visible in the recordset.
                dtlPackMaterials_Filter = dtlPackMaterials.Clone
                dtlPackMaterials_AddValues = True
            End If
        End If
        ''
    End Function

    Private Sub cear_Tab_Fragility()
        Me.txtFillSKU1.Text = String.Empty
        Me.txtFillSKU2.Text = String.Empty
        Me.txtFillSKU3.Text = String.Empty
        Me.txtFillSKU4.Text = String.Empty
        Me.txtFillSKU5.Text = String.Empty

        Me.txtFillUnit1.Text = String.Empty
        Me.txtFillUnit2.Text = String.Empty
        Me.txtFillUnit3.Text = String.Empty
        Me.txtFillUnit4.Text = String.Empty
        Me.txtFillUnit5.Text = String.Empty

        Me.txtWrapSKU1.Text = String.Empty
        Me.txtWrapSKU2.Text = String.Empty
        Me.txtWrapSKU3.Text = String.Empty
        Me.txtWrapSKU4.Text = String.Empty
        Me.txtWrapSKU5.Text = String.Empty

        Me.txtWrapUnit1.Text = String.Empty
        Me.txtWrapUnit2.Text = String.Empty
        Me.txtWrapUnit3.Text = String.Empty
        Me.txtWrapUnit4.Text = String.Empty
        Me.txtWrapUnit5.Text = String.Empty

        Me.txtLaborSKU1.Text = String.Empty
        Me.txtLaborSKU2.Text = String.Empty
        Me.txtLaborSKU3.Text = String.Empty
        Me.txtLaborSKU4.Text = String.Empty
        Me.txtLaborSKU5.Text = String.Empty

        Me.txtLaborUnit1.Text = String.Empty
        Me.txtLaborUnit2.Text = String.Empty
        Me.txtLaborUnit3.Text = String.Empty
        Me.txtLaborUnit4.Text = String.Empty
        Me.txtLaborUnit5.Text = String.Empty
    End Sub

    Private Function load_Fragility() As Boolean
        load_Fragility = True ' assume.
        Dim dreader As String = String.Empty
        Dim segment As String = String.Empty
        Dim itemName As String = String.Empty
        '
        If ShipRiteDb.PackMasterSetup_GetFragility(dreader) Then
            Do Until String.IsNullOrEmpty(dreader)
                segment = SegmentFunctions.GetNextSegmentFromSet(dreader)
                '
                If "FillUnit" = SegmentFunctions.ExtractElementFromSegment("ItemName", segment) Then
                    Me.txtFillUnit1.Text = SegmentFunctions.ExtractElementFromSegment("Fragile_L1", segment) '_Convert.Null2DefaultValue(dreader("Fragile_L1"))
                    Me.txtFillUnit2.Text = SegmentFunctions.ExtractElementFromSegment("Fragile_L2", segment) '_Convert.Null2DefaultValue(dreader("Fragile_L2"))
                    Me.txtFillUnit3.Text = SegmentFunctions.ExtractElementFromSegment("Fragile_L3", segment) '_Convert.Null2DefaultValue(dreader("Fragile_L3"))
                    Me.txtFillUnit4.Text = SegmentFunctions.ExtractElementFromSegment("Fragile_L4", segment) '_Convert.Null2DefaultValue(dreader("Fragile_L4"))
                    Me.txtFillUnit5.Text = SegmentFunctions.ExtractElementFromSegment("Fragile_L5", segment) '_Convert.Null2DefaultValue(dreader("Fragile_L5"))
                ElseIf "FillSKU" = SegmentFunctions.ExtractElementFromSegment("ItemName", segment) Then
                    Me.txtFillSKU1.Text = SegmentFunctions.ExtractElementFromSegment("Fragile_L1", segment) '_Convert.Null2DefaultValue(dreader("Fragile_L1"))
                    Me.txtFillSKU2.Text = SegmentFunctions.ExtractElementFromSegment("Fragile_L2", segment) '_Convert.Null2DefaultValue(dreader("Fragile_L2"))
                    Me.txtFillSKU3.Text = SegmentFunctions.ExtractElementFromSegment("Fragile_L3", segment) '_Convert.Null2DefaultValue(dreader("Fragile_L3"))
                    Me.txtFillSKU4.Text = SegmentFunctions.ExtractElementFromSegment("Fragile_L4", segment) '_Convert.Null2DefaultValue(dreader("Fragile_L4"))
                    Me.txtFillSKU5.Text = SegmentFunctions.ExtractElementFromSegment("Fragile_L5", segment) '_Convert.Null2DefaultValue(dreader("Fragile_L5"))
                ElseIf "WrapUnit" = SegmentFunctions.ExtractElementFromSegment("ItemName", segment) Then
                    Me.txtWrapUnit1.Text = SegmentFunctions.ExtractElementFromSegment("Fragile_L1", segment) '_Convert.Null2DefaultValue(dreader("Fragile_L1"))
                    Me.txtWrapUnit2.Text = SegmentFunctions.ExtractElementFromSegment("Fragile_L2", segment) '_Convert.Null2DefaultValue(dreader("Fragile_L2"))
                    Me.txtWrapUnit3.Text = SegmentFunctions.ExtractElementFromSegment("Fragile_L3", segment) '_Convert.Null2DefaultValue(dreader("Fragile_L3"))
                    Me.txtWrapUnit4.Text = SegmentFunctions.ExtractElementFromSegment("Fragile_L4", segment) '_Convert.Null2DefaultValue(dreader("Fragile_L4"))
                    Me.txtWrapUnit5.Text = SegmentFunctions.ExtractElementFromSegment("Fragile_L5", segment) '_Convert.Null2DefaultValue(dreader("Fragile_L5"))
                ElseIf "WrapSKU" = SegmentFunctions.ExtractElementFromSegment("ItemName", segment) Then
                    Me.txtWrapSKU1.Text = SegmentFunctions.ExtractElementFromSegment("Fragile_L1", segment) '_Convert.Null2DefaultValue(dreader("Fragile_L1"))
                    Me.txtWrapSKU2.Text = SegmentFunctions.ExtractElementFromSegment("Fragile_L2", segment) '_Convert.Null2DefaultValue(dreader("Fragile_L2"))
                    Me.txtWrapSKU3.Text = SegmentFunctions.ExtractElementFromSegment("Fragile_L3", segment) '_Convert.Null2DefaultValue(dreader("Fragile_L3"))
                    Me.txtWrapSKU4.Text = SegmentFunctions.ExtractElementFromSegment("Fragile_L4", segment) '_Convert.Null2DefaultValue(dreader("Fragile_L4"))
                    Me.txtWrapSKU5.Text = SegmentFunctions.ExtractElementFromSegment("Fragile_L5", segment) '_Convert.Null2DefaultValue(dreader("Fragile_L5"))
                ElseIf "LaborUnit" = SegmentFunctions.ExtractElementFromSegment("ItemName", segment) Then
                    Me.txtLaborUnit1.Text = SegmentFunctions.ExtractElementFromSegment("Fragile_L1", segment) '_Convert.Null2DefaultValue(dreader("Fragile_L1"))
                    Me.txtLaborUnit2.Text = SegmentFunctions.ExtractElementFromSegment("Fragile_L2", segment) '_Convert.Null2DefaultValue(dreader("Fragile_L2"))
                    Me.txtLaborUnit3.Text = SegmentFunctions.ExtractElementFromSegment("Fragile_L3", segment) '_Convert.Null2DefaultValue(dreader("Fragile_L3"))
                    Me.txtLaborUnit4.Text = SegmentFunctions.ExtractElementFromSegment("Fragile_L4", segment) '_Convert.Null2DefaultValue(dreader("Fragile_L4"))
                    Me.txtLaborUnit5.Text = SegmentFunctions.ExtractElementFromSegment("Fragile_L5", segment) '_Convert.Null2DefaultValue(dreader("Fragile_L5"))
                ElseIf "LaborSKU" = SegmentFunctions.ExtractElementFromSegment("ItemName", segment) Then
                    Me.txtLaborSKU1.Text = SegmentFunctions.ExtractElementFromSegment("Fragile_L1", segment) '_Convert.Null2DefaultValue(dreader("Fragile_L1"))
                    Me.txtLaborSKU2.Text = SegmentFunctions.ExtractElementFromSegment("Fragile_L2", segment) '_Convert.Null2DefaultValue(dreader("Fragile_L2"))
                    Me.txtLaborSKU3.Text = SegmentFunctions.ExtractElementFromSegment("Fragile_L3", segment) '_Convert.Null2DefaultValue(dreader("Fragile_L3"))
                    Me.txtLaborSKU4.Text = SegmentFunctions.ExtractElementFromSegment("Fragile_L4", segment) '_Convert.Null2DefaultValue(dreader("Fragile_L4"))
                    Me.txtLaborSKU5.Text = SegmentFunctions.ExtractElementFromSegment("Fragile_L5", segment) '_Convert.Null2DefaultValue(dreader("Fragile_L5"))
                End If
            Loop
        End If
    End Function
    Private Function load_ComboDropDown(ByVal drows() As DataRow, ByVal txtbox As TextBox) As Boolean
        load_ComboDropDown = False
        Me.lwRegDesc.Tag = txtbox ' keep combo object which produced the list
        Me.lwRegDesc.DataContext = Nothing
        Me.lwRegDesc.ItemsSource = Nothing
        '
        Dim dtable As New DataTable
        dtable.Columns.Add("SKU", GetType(String))
        dtable.Columns.Add("Desc", GetType(String))
        dtable.Columns.Add("Weight", GetType(Double))
        dtable.Columns.Add("Sell", GetType(Double))
        dtable.Columns.Add("Quantity", GetType(Double))
        '
        For i As Short = 0 To drows.Length - 1
            Dim drow As DataRow = drows(i)
            Dim nrow As DataRow = dtable.NewRow()
            nrow("SKU") = _Convert.Null2DefaultValue(drow("SKU"))
            nrow("Desc") = _Convert.Null2DefaultValue(drow("Desc"))
            nrow("Weight") = _Convert.Null2DefaultValue(drow("Weight"), 0)
            nrow("Sell") = _Convert.Null2DefaultValue(drow("Sell"), 0)
            nrow("Quantity") = _Convert.Null2DefaultValue(drow("Quantity"), 0)
            dtable.Rows.Add(nrow)
        Next i
        '
        Me.lwRegDesc.DataContext = dtable
        Me.lwRegDesc.SetBinding(ListView.ItemsSourceProperty, New Binding)
        '
        If (Me.lwRegDesc.Items.Count > 0) Then
            Me.InventoryLIstViewPopup.PlacementTarget = txtbox
            Me.InventoryLIstViewPopup.Placement = Primitives.PlacementMode.Bottom
            load_ComboDropDown = True
        End If
    End Function

    Private Function find_cmbRegDesc_byINDEX(ByVal ctrlname As String, ByRef ctrl As TextBox) As Boolean
        Dim drows() As DataRow = Nothing
        '
        find_cmbRegDesc_byINDEX = True ' assume.
        If _Controls.Contains(ctrlname, "WrapSKU") Then
            If _DataSet.Filter_DataTable(dtlPackMaterials, "MaterialsClass='Wrap'", drows) Then
                load_ComboDropDown(drows, ctrl)
            End If
            InventoryViewLabel.Content = "Select Wrap SKU"
        ElseIf _Controls.Contains(ctrlname, "FillSKU") Then
            If _DataSet.Filter_DataTable(dtlPackMaterials, "MaterialsClass='Filler'", drows) Then
                load_ComboDropDown(drows, ctrl)
            End If
            InventoryViewLabel.Content = "Select Filler SKU"
        ElseIf _Controls.Contains(ctrlname, "LaborSKU") Then
            If _DataSet.Filter_DataTable(dtlPackMaterials, "MaterialsClass='Labor'", drows) Then
                load_ComboDropDown(drows, ctrl)
            End If
            InventoryViewLabel.Content = "Select Labor SKU"
        ElseIf _Controls.Contains(ctrlname, "OtherSKU") Then
            If _DataSet.Filter_DataTable(dtlPackMaterials, "MaterialsClass<>''", drows) Then
                load_ComboDropDown(drows, ctrl)
            End If
        Else
            find_cmbRegDesc_byINDEX = False
        End If
    End Function

    Private Function find_txtSKU_byUnit(ByVal txtUnit As TextBox, ByRef txtSKU As TextBox) As Boolean
        find_txtSKU_byUnit = True ' assume.
        If _Controls.Contains(txtUnit.Name, "Fill") Then
            If "1" = _Controls.Right(txtUnit.Name, 1) Then
                txtSKU = Me.txtFillSKU1
            ElseIf "2" = _Controls.Right(txtUnit.Name, 1) Then
                txtSKU = Me.txtFillSKU2
            ElseIf "3" = _Controls.Right(txtUnit.Name, 1) Then
                txtSKU = Me.txtFillSKU3
            ElseIf "4" = _Controls.Right(txtUnit.Name, 1) Then
                txtSKU = Me.txtFillSKU4
            ElseIf "5" = _Controls.Right(txtUnit.Name, 1) Then
                txtSKU = Me.txtFillSKU5
            Else
                find_txtSKU_byUnit = False
            End If
        ElseIf _Controls.Contains(txtUnit.Name, "Wrap") Then
            If "1" = _Controls.Right(txtUnit.Name, 1) Then
                txtSKU = Me.txtWrapSKU1
            ElseIf "2" = _Controls.Right(txtUnit.Name, 1) Then
                txtSKU = Me.txtWrapSKU2
            ElseIf "3" = _Controls.Right(txtUnit.Name, 1) Then
                txtSKU = Me.txtWrapSKU3
            ElseIf "4" = _Controls.Right(txtUnit.Name, 1) Then
                txtSKU = Me.txtWrapSKU4
            ElseIf "5" = _Controls.Right(txtUnit.Name, 1) Then
                txtSKU = Me.txtWrapSKU5
            Else
                find_txtSKU_byUnit = False
            End If
        ElseIf _Controls.Contains(txtUnit.Name, "Labor") Then
            If "1" = _Controls.Right(txtUnit.Name, 1) Then
                txtSKU = Me.txtLaborSKU1
            ElseIf "2" = _Controls.Right(txtUnit.Name, 1) Then
                txtSKU = Me.txtLaborSKU2
            ElseIf "3" = _Controls.Right(txtUnit.Name, 1) Then
                txtSKU = Me.txtLaborSKU3
            ElseIf "4" = _Controls.Right(txtUnit.Name, 1) Then
                txtSKU = Me.txtLaborSKU4
            ElseIf "5" = _Controls.Right(txtUnit.Name, 1) Then
                txtSKU = Me.txtLaborSKU5
            Else
                find_txtSKU_byUnit = False
            End If
        Else
            find_txtSKU_byUnit = False
        End If
    End Function
    Private Function find_txtUnit_bySKU(ByVal txtSKU As TextBox, ByRef txtUnit As TextBox) As Boolean
        find_txtUnit_bySKU = True ' assume.
        If _Controls.Contains(txtSKU.Name, "Fill") Then
            If "1" = _Controls.Right(txtSKU.Name, 1) Then
                txtUnit = Me.txtFillUnit1
            ElseIf "2" = _Controls.Right(txtSKU.Name, 1) Then
                txtUnit = Me.txtFillUnit2
            ElseIf "3" = _Controls.Right(txtSKU.Name, 1) Then
                txtUnit = Me.txtFillUnit3
            ElseIf "4" = _Controls.Right(txtSKU.Name, 1) Then
                txtUnit = Me.txtFillUnit4
            ElseIf "5" = _Controls.Right(txtSKU.Name, 1) Then
                txtUnit = Me.txtFillUnit5
            Else
                find_txtUnit_bySKU = False
            End If
        ElseIf _Controls.Contains(txtSKU.Name, "Wrap") Then
            If "1" = _Controls.Right(txtSKU.Name, 1) Then
                txtUnit = Me.txtWrapUnit1
            ElseIf "2" = _Controls.Right(txtSKU.Name, 1) Then
                txtUnit = Me.txtWrapUnit2
            ElseIf "3" = _Controls.Right(txtSKU.Name, 1) Then
                txtUnit = Me.txtWrapUnit3
            ElseIf "4" = _Controls.Right(txtSKU.Name, 1) Then
                txtUnit = Me.txtWrapUnit4
            ElseIf "5" = _Controls.Right(txtSKU.Name, 1) Then
                txtUnit = Me.txtWrapUnit5
            Else
                find_txtUnit_bySKU = False
            End If
        ElseIf _Controls.Contains(txtSKU.Name, "Labor") Then
            If "1" = _Controls.Right(txtSKU.Name, 1) Then
                txtUnit = Me.txtLaborUnit1
            ElseIf "2" = _Controls.Right(txtSKU.Name, 1) Then
                txtUnit = Me.txtLaborUnit2
            ElseIf "3" = _Controls.Right(txtSKU.Name, 1) Then
                txtUnit = Me.txtLaborUnit3
            ElseIf "4" = _Controls.Right(txtSKU.Name, 1) Then
                txtUnit = Me.txtLaborUnit4
            ElseIf "5" = _Controls.Right(txtSKU.Name, 1) Then
                txtUnit = Me.txtLaborUnit5
            Else
                find_txtUnit_bySKU = False
            End If
        Else
            find_txtUnit_bySKU = False
        End If
    End Function

    Private Sub txtUnit_LostFocus(sender As Object, e As RoutedEventArgs) Handles txtFillSKU1.LostFocus, txtFillSKU2.LostFocus, txtFillSKU3.LostFocus, txtFillSKU4.LostFocus, txtFillSKU5.LostFocus, txtWrapSKU1.LostFocus, txtWrapSKU2.LostFocus, txtWrapSKU3.LostFocus, txtWrapSKU4.LostFocus, txtWrapSKU5.LostFocus, txtLaborSKU1.LostFocus, txtLaborSKU2.LostFocus, txtLaborSKU3.LostFocus, txtLaborSKU4.LostFocus, txtLaborSKU5.LostFocus
        Try
            Dim txtUnit As TextBox = CType(sender, TextBox)
            If 0 = Val(txtUnit.Text) Then
                Dim txtSKU As TextBox = Nothing
                If find_txtSKU_byUnit(txtUnit, txtSKU) Then
                    txtUnit.Text = String.Empty
                    txtSKU.Text = String.Empty
                End If
            ElseIf 0 < Val(txtUnit.Text) Then
                Dim txtSKU As TextBox = Nothing
                If find_txtSKU_byUnit(txtUnit, txtSKU) Then
                    If String.Empty = txtSKU.Text Then
                        Me.grbFragility.Tag = txtSKU ' hold the text box that was clicked
                        find_cmbRegDesc_byINDEX(txtSKU.Name, txtSKU)
                    End If
                End If
            End If
        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to validate SKU...")
        End Try
    End Sub

    Private Sub lwRegDesc_Leave(sender As Object, e As System.EventArgs) Handles lwRegDesc.LostFocus
        InventoryLIstViewPopup.IsOpen = False
        If Not Me.grbFragility.Tag Is Nothing Then
            Dim txt As TextBox = Me.grbFragility.Tag
            Dim txtUnit As TextBox = Nothing
            If find_txtUnit_bySKU(txt, txtUnit) Then
                txtUnit.Select(0, txtUnit.Text.Length)
            End If
        End If
    End Sub
    Private Sub lwRegDesc_DoubleClick(sender As Object, e As System.EventArgs) Handles lwRegDesc.MouseDoubleClick
        Try
            If Not Me.lwRegDesc.SelectedItems(0) Is Nothing Then
                If Not Me.grbFragility.Tag Is Nothing Then
                    Dim txt As TextBox = Me.grbFragility.Tag
                    Dim listItem As DataRowView = Me.lwRegDesc.SelectedItems(0)
                    txt.Text = listItem(0)
                    InventoryLIstViewPopup.IsOpen = False
                    Dim txtUnit As TextBox = Nothing
                    If find_txtUnit_bySKU(txt, txtUnit) Then
                        If 0 = Val(txtUnit.Text) Then
                            txtUnit.Select(0, txtUnit.Text.Length)
                        End If
                    End If
                End If
            End If
        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to select SKU...")
        Finally : InventoryLIstViewPopup.IsOpen = False
        End Try
    End Sub
    Private Sub lwRegDesc_KeyDown(sender As Object, e As KeyEventArgs) Handles lwRegDesc.KeyDown
        Try
            If e.Key = Key.Enter Then
                If Not Me.lwRegDesc.SelectedItems(0) Is Nothing Then
                    If Not Me.grbFragility.Tag Is Nothing Then
                        Dim txt As TextBox = Me.grbFragility.Tag
                        Dim listItem As DataRowView = Me.lwRegDesc.SelectedItems(0)
                        txt.Text = listItem(0)
                        InventoryLIstViewPopup.IsOpen = False
                        Dim txtUnit As TextBox = Nothing
                        If find_txtUnit_bySKU(txt, txtUnit) Then
                            If 0 = Val(txtUnit.Text) Then
                                txtUnit.Select(0, txtUnit.Text.Length)
                            End If
                        End If
                    End If
                End If
            End If
        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to select SKU...")
        End Try
    End Sub

    Private Sub Close_InventoryPopup_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Close_InventoryPopup_Btn.Click
        InventoryLIstViewPopup.IsOpen = False
    End Sub


    Private Sub EnableShipsurance_Checked(sender As Object, e As RoutedEventArgs) Handles EnableShipsurance.Checked

        gThirdPartyInsurance = True
        Call General.UpdatePolicy(gShipriteDB, "EnableShipsurance", "True")
        Call General.UpdatePolicy(gShipriteDB, "ThirdPartyInsurance", "True")

        If EnableShipAndInsure.IsChecked = True Then

            Call General.UpdatePolicy(gShipriteDB, "EnableShipAndInsure", "False")
            EnableShipAndInsure.IsChecked = False

        End If

    End Sub

    Private Sub EnableShipsurance_UnChecked(sender As Object, e As RoutedEventArgs) Handles EnableShipsurance.Unchecked

        Call General.UpdatePolicy(gShipriteDB, "EnableShipsurance", "False")
        If EnableShipAndInsure.IsChecked = False Then

            Call General.UpdatePolicy(gShipriteDB, "ThirdPartyInsurance", "False")
            gThirdPartyInsurance = False

        End If

    End Sub

    Private Sub EnableShipAndInsure_Checked(sender As Object, e As RoutedEventArgs) Handles EnableShipAndInsure.Checked

        gThirdPartyInsurance = True
        Call General.UpdatePolicy(gShipriteDB, "EnableShipAndInsure", "True")
        Call General.UpdatePolicy(gShipriteDB, "ThirdPartyInsurance", "True")
        If EnableShipsurance.IsChecked = True Then

            Call General.UpdatePolicy(gShipriteDB, "EnableShipsurance", "False")
            EnableShipsurance.IsChecked = False

        End If

    End Sub

    Private Sub EnableShipAndInsure_UnChecked(sender As Object, e As RoutedEventArgs) Handles EnableShipAndInsure.Unchecked

        Call General.UpdatePolicy(gShipriteDB, "EnableShipAndInsure", "False")
        If EnableShipsurance.IsChecked = False Then

            Call General.UpdatePolicy(gShipriteDB, "ThirdPartyInsurance", "False")
            gThirdPartyInsurance = False

        End If

    End Sub

#End Region

#End Region


End Class
