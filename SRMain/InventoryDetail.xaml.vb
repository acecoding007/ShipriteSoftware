Public Class InventoryDetail
    Inherits CommonWindow
    Public inventory_list As List(Of InventoryItem)
    Public Last_Column_Sorted As String
    Public Last_Sort_Ascending As Boolean
    Public Current_SKU As String
    Public sel_index As Integer

    Public List_UI_Controls As List(Of Object)

    Public Class VendorContact
        Public Property ID As String
        Public Property Name As String
    End Class

    Public Sub New()

        MyBase.New()

        ' This call is required by the designer.
        InitializeComponent()

    End Sub

    Public Sub New(ByVal callingWindow As Window, Optional ByRef current_list As List(Of InventoryItem) = Nothing, Optional ByVal Selected_Index As Integer = 0, Optional SKU As String = "")

        MyBase.New(callingWindow)

        ' This call is required by the designer.
        InitializeComponent()
        sel_index = Selected_Index

        inventory_list = current_list
        LV_InventoryDetail.ItemsSource = inventory_list

        TxtBx_SKU.Text = SKU


    End Sub

    Private Sub InventoryDetail_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded

        If (gIsProgramSecurityEnabled Or gIsPOSSecurityEnabled) AndAlso Not Check_Current_User_Permission("Inventory") Then
            Me.Close()
        End If

        List_UI_Controls = New List(Of Object)

        Get_ChildControls_Of_Grid(Grid1, List_UI_Controls)
        Get_ChildControls_Of_Grid(Grid2, List_UI_Controls)
        Get_ChildControls_Of_Grid(Grid3, List_UI_Controls)
        Get_ChildControls_Of_Grid(Grid4, List_UI_Controls)
        Get_ChildControls_Of_Grid(Grid5, List_UI_Controls)
        Get_ChildControls_Of_Grid(Grid6, List_UI_Controls)
        Get_ChildControls_Of_Grid(TaxGrid, List_UI_Controls)
        Get_ChildControls_Of_Grid(VendorGrid, List_UI_Controls)

        Load_Department_ComboBox()
        Load_VendorList()

        LV_InventoryDetail.SelectedIndex = sel_index
        LV_InventoryDetail.ScrollIntoView(LV_InventoryDetail.SelectedItem)

        Current_SKU = TxtBx_SKU.Text

        Load_Inventory_Item()

    End Sub

    Private Sub GridViewColumnHeader_Click(sender As GridViewColumnHeader, e As RoutedEventArgs)
        If IsNothing(inventory_list) Then Exit Sub

        If sender.Content = "SKU" Then 'order list by SKU
            If Last_Column_Sorted = sender.Content And Last_Sort_Ascending Then
                inventory_list = inventory_list.OrderByDescending(Function(value As InventoryItem) value.SKU).ToList()
                Last_Sort_Ascending = False
            Else
                inventory_list = inventory_list.OrderBy(Function(value As InventoryItem) value.SKU).ToList()
                Last_Sort_Ascending = True
            End If

        Else 'order list by Description
            If Last_Column_Sorted = sender.Content And Last_Sort_Ascending Then
                inventory_list = inventory_list.OrderByDescending(Function(value As InventoryItem) value.Desc).ToList()
                Last_Sort_Ascending = False
            Else
                inventory_list = inventory_list.OrderBy(Function(value As InventoryItem) value.Desc).ToList()
                Last_Sort_Ascending = True
            End If
        End If

        Last_Column_Sorted = sender.Content

        LV_InventoryDetail.ItemsSource = inventory_list
        LV_InventoryDetail.Items.Refresh()
    End Sub

    Private Sub Load_Inventory_Item()
        If Current_SKU = "" Then
            Exit Sub
        End If

        Dim SegmentSet As String = ""
        Dim current_segment As String = ""
        Dim VendorID As String

        current_segment = GetNextSegmentFromSet(IO_GetSegmentSet(gShipriteDB, "SELECT * From Inventory WHERE SKU='" & Current_SKU & "'"))

        VendorID = ExtractElementFromSegment("Vendor", current_segment, "")
        Vendor_CmbBx.SelectedIndex = -1
        Display_Vendor_InDropDown(VendorID)

        Display_DBData_To_UI(List_UI_Controls, current_segment)

        Check_Number_Fields()

    End Sub

    Private Sub InventoryDetail_ListView_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles LV_InventoryDetail.SelectionChanged
        Dim item As InventoryItem = New InventoryItem

        If LV_InventoryDetail.SelectedIndex = -1 Then
            Exit Sub
        End If

        item = LV_InventoryDetail.SelectedItem
        Current_SKU = item.SKU

        Load_Inventory_Item()

        If ChkBx_PackMaster.IsChecked = True Then
            Change_PackMasterItems_Visibility(Visibility.Visible)
        Else
            Change_PackMasterItems_Visibility(Visibility.Hidden)
        End If

    End Sub

    Private Sub Load_VendorList()
        Dim VendorList As List(Of VendorContact) = New List(Of VendorContact)
        Dim contact As VendorContact
        Dim current_segment As String
        Dim buf As String

        Dim newVendor As VendorContact = New VendorContact
        newVendor.Name = "<<Add New>>"
        newVendor.ID = 0

        VendorList.Add(newVendor)

        buf = IO_GetSegmentSet(gShipriteDB, "SELECT ID, Name From Contacts where Class='Vendor'")

        Do Until buf = ""
            contact = New VendorContact
            current_segment = GetNextSegmentFromSet(buf)
            contact.ID = ExtractElementFromSegment("ID", current_segment, "")
            contact.Name = ExtractElementFromSegment("Name", current_segment, "")
            VendorList.Add(contact)

        Loop

        VendorList = VendorList.OrderBy(Function(x) x.Name).ToList

        Vendor_CmbBx.ItemsSource = VendorList
        Vendor_CmbBx.Items.Refresh()
    End Sub

    Private Sub Vendor_CmbBx_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles Vendor_CmbBx.SelectionChanged
        If Vendor_CmbBx.SelectedIndex = -1 Then
            VendorDetail_TxtBx.Text = ""
            Exit Sub

        ElseIf Vendor_CmbBx.SelectedIndex = 0 Then
            'Add New Vendor
            gContactManagerSegment = ""
            gAutoExitFromContacts = True
            Dim win As New ContactManager(Me)
            win.ShowDialog(Me)


            If Not gContactManagerSegment = "" Then
                Dim VendorID = ExtractElementFromSegment("ID", gContactManagerSegment)

                If IO_UpdateSQLProcessor(gShipriteDB, "Update Contacts set Class='Vendor' where ID=" & VendorID) Then
                    MsgBox(ExtractElementFromSegment("Name", gContactManagerSegment) & " added to Vendor list!", vbInformation)
                    Load_VendorList()

                    Display_Vendor_InDropDown(VendorID)

                End If
            End If
        End If

        VendorDetail_TxtBx.Text = CreateDisplayBlock(IO_GetSegmentSet(gShipriteDB, "SELECT Name, Addr1, City, State, Zip, PHone, Email From Contacts where ID=" & Vendor_CmbBx.SelectedItem.ID), True)
    End Sub

    Private Sub Display_Vendor_InDropDown(ByRef CID As String)
        If CID IsNot "" Then
            For Each item As VendorContact In Vendor_CmbBx.Items
                If item.ID = CID Then
                    Vendor_CmbBx.SelectedItem = item
                    Exit For
                End If
            Next
        End If
    End Sub

    Private Sub Vendor_Clear_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Vendor_Clear_Btn.Click
        Vendor_CmbBx.SelectedIndex = -1
    End Sub

    Private Sub Load_Department_ComboBox()
        Dim SegmentSet As String = ""
        Dim current_segment As String = ""
        Dim buf As String = ""
        Dim fieldName As String = ""
        Dim fieldValue As String = ""

        buf = IO_GetSegmentSet(gShipriteDB, "SELECT Department From Departments")

        Do Until buf = ""
            current_segment = GetNextSegmentFromSet(buf)
            current_segment = ExtractNextElementFromSegment(fieldName, fieldValue, current_segment)

            If fieldValue Is Nothing Then fieldValue = ""
            ComboBx_Department.Items.Add(fieldValue)
        Loop

    End Sub

    Private Sub ChkBx_PackMaster_Checked(sender As Object, e As RoutedEventArgs) Handles ChkBx_PackMaster.Checked
        Change_PackMasterItems_Visibility(Visibility.Visible)
    End Sub

    Private Sub ChkBx_PackMaster_Unchecked(sender As Object, e As RoutedEventArgs) Handles ChkBx_PackMaster.Unchecked
        Change_PackMasterItems_Visibility(Visibility.Hidden)
    End Sub

    Private Sub Change_PackMasterItems_Visibility(ByVal IsVisible As Visibility)
        ComboBx_PackMasterClass.Visibility = IsVisible
        Lbl_PackMaster_Desc.Visibility = IsVisible
        Lbl_Class.Visibility = IsVisible
        Lbl_Height.Visibility = IsVisible
        Lbl_Length.Visibility = IsVisible
        Lbl_Width.Visibility = IsVisible
        Lbl_Weight.Visibility = IsVisible
        TxtBx_PackMaster_Length.Visibility = IsVisible
        TxtBx_PackMaster_Width.Visibility = IsVisible
        TxtBx_PackMaster_Height.Visibility = IsVisible
        TxtBx_PackMaster_Weight.Visibility = IsVisible
        ChkBx_PackMaster_DefaultClass.Visibility = IsVisible

    End Sub

    Private Sub Add_Button_Click(sender As Object, e As RoutedEventArgs) Handles Add_Button.Click
        Dim SQL As String

        If Not Check_Input_Data() Then Exit Sub

        Check_Number_Fields()

        SQL = "INSERT INTO Inventory ("
        For Each x As Object In List_UI_Controls
            SQL = SQL & "[" & x.tag & "],"
        Next

        SQL = SQL.TrimEnd(",")

        SQL = SQL & ") VALUES ("

        For Each x As Object In List_UI_Controls
            If x.GetType = GetType(CheckBox) Then
                SQL = SQL & x.isChecked & ","
            Else
                SQL = SQL & "'" & x.text & "',"
            End If
        Next
        SQL = SQL.TrimEnd(",")
        SQL = SQL & ")"



        If IO_UpdateSQLProcessor(gShipriteDB, SQL) = 0 Then
            Exit Sub
        End If

        MsgBox("SKU: " & vbCrLf & TxtBx_SKU.Text & " - " & TxtBx_Desc.Text & " saved succesfully!", MsgBoxStyle.Information, "Add New Inventory Item")

        inventory_list.Add(Create_InventoryItem())
        LV_InventoryDetail.Items.Refresh()

    End Sub

    Private Sub SaveButton_Click(sender As Object, e As RoutedEventArgs) Handles SaveButton.Click
        Dim SQL As String
        Dim Index As Integer = 0

        If Not Check_Input_Data() Then Exit Sub

        Check_Number_Fields()

        If String.IsNullOrEmpty(Current_SKU) Then
            MsgBox("Cannot Save. No SKU selected", MsgBoxStyle.Exclamation, "Save Changes")
            Exit Sub
        End If

        SQL = "Update Inventory set"

        For Each x As Object In List_UI_Controls
            If x.GetType = GetType(CheckBox) Then
                SQL = SQL & " [" & x.tag & "]=" & x.isChecked & ","
            Else
                SQL = SQL & " [" & x.tag & "]='" & x.text & "',"
            End If
        Next

        If Vendor_CmbBx.SelectedIndex <> -1 Then
            SQL = SQL & " [Vendor]='" & Vendor_CmbBx.SelectedItem.ID & "'"
        Else
            SQL = SQL & " [Vendor]=''"
            'SQL = SQL.TrimEnd(",")
        End If

        SQL = SQL & " WHERE [SKU]='" & Current_SKU & "'"

        If IO_UpdateSQLProcessor(gShipriteDB, SQL) = -1 Then
            Exit Sub
        End If

        MsgBox("Changes to SKU: " & Current_SKU & " saved successfuly!", MsgBoxStyle.Information, "Save Changes")



        If Not IsNothing(inventory_list) Then
            Index = inventory_list.FindIndex(Function(x As InventoryItem) x.SKU = Current_SKU)
            inventory_list.Item(Index).Desc = TxtBx_Desc.Text
            inventory_list.Item(Index).Department = ComboBx_Department.Text
            inventory_list.Item(Index).Cost = TxtBx_Cost.Text
            inventory_list.Item(Index).Sell = TxtBx_Sell.Text
            inventory_list.Item(Index).Quantity = TxtBx_Quantity.Text
            inventory_list.Item(Index).WarningQty = TxtBx_WarningQty.Text
            inventory_list.Item(Index).SKU = TxtBx_SKU.Text
            inventory_list.Item(Index).Zero = ChkBx_CountInventory.IsChecked
            inventory_list.Item(Index).Active = ChkBx_Active.IsChecked

            LV_InventoryDetail.Items.Refresh()
        End If

        Tickler.Check_Tickler_Inventory_Flag(Current_SKU)


    End Sub

    Private Sub Remove_Button_Click(sender As Object, e As RoutedEventArgs) Handles Remove_Button.Click
        Dim SQL As String
        Check_Number_Fields()

        If LV_InventoryDetail.SelectedIndex = -1 Then
            MsgBox("Cannot Delete Inventory Item. No Item Selected!", MsgBoxStyle.Critical + vbOKOnly)
            Exit Sub
        End If
        If MsgBox("Do you want to delete the following SKU: " & vbCrLf & Current_SKU, MsgBoxStyle.YesNo + MsgBoxStyle.Question, "Delete Inventory Item") = MsgBoxResult.No Then
            Exit Sub
        End If


        SQL = "DELETE From Inventory Where SKU= '" & Current_SKU & "'"

        If IO_UpdateSQLProcessor(gShipriteDB, SQL) = 0 Then
            MsgBox("Could Not Delete SKU: " & vbCrLf & Current_SKU, MsgBoxStyle.Exclamation, "Error!")
            Exit Sub
        End If

        'Delete Items from the list/view
        inventory_list.Remove(inventory_list.Find((Function(x As InventoryItem) x.SKU = Current_SKU)))
        LV_InventoryDetail.Items.Refresh()

        MsgBox("Following SKU was deleted successfully: " & vbCrLf & Current_SKU, MsgBoxStyle.Information, "Delete Inventory Items")
        Clear_Screen()
        Current_SKU = ""
    End Sub

    Private Sub Clear_Screen()
        For Each x As Object In List_UI_Controls
            If x.GetType = GetType(CheckBox) Then
                x.isChecked = False
            Else
                x.text = ""
            End If
        Next

    End Sub

    Private Sub CalculateMarkup() Handles TxtBx_Cost.TextChanged, TxtBx_Sell.TextChanged
        Dim c As Double
        Dim s As Double

        If IsNumeric(TxtBx_Cost.Text) And IsNumeric(TxtBx_Sell.Text) Then
            c = CDbl(TxtBx_Cost.Text)
            s = CDbl(TxtBx_Sell.Text)

            TxtBx_Markup.Text = ((s - c) / c).ToString("#0.##%")

        End If

    End Sub

    Private Sub TxtBx_Markup_LostFocus(sender As Object, e As RoutedEventArgs) Handles TxtBx_Markup.LostFocus
        Dim c As Double
        Dim m As Double

        If IsNumeric(TxtBx_Cost.Text) And IsNumeric(TxtBx_Markup.Text) Then
            c = CDbl(TxtBx_Cost.Text)
            m = CDbl(TxtBx_Markup.Text) / 100

            TxtBx_Sell.Text = FormatCurrency((c * m + c).ToString())
        End If

    End Sub

    Private Sub TxtBx_Cost_LostFocus(sender As Object, e As RoutedEventArgs) Handles TxtBx_Cost.LostFocus, TxtBx_Sell.LostFocus
        Dim Current_TxtBox = DirectCast(sender, TextBox)

        If Current_TxtBox.Text <> "" Then
            Current_TxtBox.Text = FormatCurrency(Current_TxtBox.Text, 4)
        Else
            Current_TxtBox.Text = FormatCurrency(0)
        End If
    End Sub

    Private Sub Check_Number_Fields()
        'eliminates empty strings from all textboxes that are linked to number fields in the database
        If TxtBx_Cost.Text = "" Then TxtBx_Cost.Text = FormatCurrency(0) Else TxtBx_Cost.Text = FormatCurrency(TxtBx_Cost.Text, 4)
        If TxtBx_Sell.Text = "" Then TxtBx_Sell.Text = FormatCurrency(0) Else TxtBx_Sell.Text = FormatCurrency(TxtBx_Sell.Text, 4)
        If TxtBx_MSRP.Text = "" Then TxtBx_MSRP.Text = FormatCurrency(0) Else TxtBx_MSRP.Text = FormatCurrency(TxtBx_MSRP.Text)


        If TxtBx_WarningQty.Text = "" Then TxtBx_WarningQty.Text = "0"
        If TxtBx_Quantity.Text = "" Then TxtBx_Quantity.Text = "0"
        If TxtBx_PackMaster_Length.Text = "" Then TxtBx_PackMaster_Length.Text = "0"
        If TxtBx_PackMaster_Width.Text = "" Then TxtBx_PackMaster_Width.Text = "0"
        If TxtBx_PackMaster_Height.Text = "" Then TxtBx_PackMaster_Height.Text = "0"
        If TxtBx_PackMaster_Weight.Text = "" Then TxtBx_PackMaster_Weight.Text = "0"
        If Tax1_TxtBx.Text = "" Then Tax1_TxtBx.Text = "0"
        If Tax2_TxtBx.Text = "" Then Tax2_TxtBx.Text = "0"
        If Tax3_TxtBx.Text = "" Then Tax3_TxtBx.Text = "0"

        Dim List_LevelPricing As List(Of Object) = New List(Of Object)
        Get_ChildControls_Of_Grid(Grid6, List_LevelPricing)

        For Each x As Object In List_LevelPricing
            If x.GetType = GetType(TextBox) Then
                If x.text = "" Then x.text = "0"
            End If
        Next

    End Sub

    Private Function Check_Input_Data() As Boolean
        If TxtBx_SKU.Text = "" Or TxtBx_Desc.Text = "" Or ComboBx_Department.Text = "" Then
            MsgBox("Error! SKU, Descriptions, and Department field cannot be empty!", MsgBoxStyle.Information, "ShipRite Inventory")
            Return False
        End If
        Return True
    End Function

    Private Function Create_InventoryItem() As InventoryItem
        Dim item As InventoryItem = New InventoryItem

        item.SKU = TxtBx_SKU.Text
        item.Desc = TxtBx_Desc.Text

        Return item

    End Function

    Private Sub ComboBx_PackMasterClass_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles ComboBx_PackMasterClass.SelectionChanged
        If ComboBx_PackMasterClass.SelectedValue = Nothing Then
            Exit Sub
        End If

        Select Case ComboBx_PackMasterClass.SelectedValue.ToString()
            Case "Boxes"
                Lbl_PackMaster_Desc.Text = "Please enter the Lenght, Width, Height, and Weight of the empty Box."
                TxtBx_PackMaster_Length.Visibility = Visibility.Visible
                TxtBx_PackMaster_Height.Visibility = Visibility.Visible
                TxtBx_PackMaster_Width.Visibility = Visibility.Visible
            Case "Wrap"
                Lbl_PackMaster_Desc.Text = "Weight and Pricing have to be set for 1 square foot of wrapping."
                TxtBx_PackMaster_Length.Visibility = Visibility.Hidden
                TxtBx_PackMaster_Height.Visibility = Visibility.Hidden
                TxtBx_PackMaster_Width.Visibility = Visibility.Hidden
            Case "Filler"
                Lbl_PackMaster_Desc.Text = "Weight and Pricing have to be set for 1 cubic foot of fill material."
                TxtBx_PackMaster_Length.Visibility = Visibility.Hidden
                TxtBx_PackMaster_Height.Visibility = Visibility.Hidden
                TxtBx_PackMaster_Width.Visibility = Visibility.Hidden
            Case "Labor"
                Lbl_PackMaster_Desc.Text = "Please set the pricing for 1 hour of labor."
                TxtBx_PackMaster_Length.Visibility = Visibility.Hidden
                TxtBx_PackMaster_Height.Visibility = Visibility.Hidden
                TxtBx_PackMaster_Width.Visibility = Visibility.Hidden

        End Select
    End Sub

    Private Sub POSSetup_Btn_Click(sender As Object, e As RoutedEventArgs) Handles POSSetup_Btn.Click

        Dim win As New POSSetup(Me, 1)
        win.ShowDialog(Me)


    End Sub

End Class
