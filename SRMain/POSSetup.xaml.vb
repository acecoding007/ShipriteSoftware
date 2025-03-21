Imports System.ComponentModel
Imports System.Runtime.CompilerServices
Imports System.Windows.Media
Imports Microsoft.Win32
Public Class POSDepartment
    Public Property DepartmentName As String
    Public Property IsTaxable As Boolean
    Public Property ID As Long
    Public Property Status As String
End Class

Public Class TaxCounty
    Public Property State As String
    Public Property County As String
    Public Property TaxRate As Double
    Public Property T1 As Double
    Public Property T2 As Double
    Public Property T3 As Double
    Public Property Status As String
    Public Property ID As Integer
    Public Property DefaultCounty As Boolean
End Class

Public Class QB_DepartmentMapping_Item
    Public Property Department As String
    Public Property QB_Account As String
    Public Property QB_AccountList As List(Of String)

End Class



Public Class POSSetup
    Inherits CommonWindow

    Private Class Coupon
        Public Property SKU As String
        Public Property Desc As String
        Public Property Active As Boolean
    End Class



    Dim Department_List As List(Of POSDepartment)
    Dim TaxCounty_List As List(Of TaxCounty)
    Dim Coupon_List As List(Of Coupon)
    Dim Last_Column_Sorted As String
    Dim Last_Sort_Ascending As Boolean
    Dim Logo_Image As Ad_Image

    Public Sub New()

        MyBase.New()

        ' This call is required by the designer.
        InitializeComponent()

    End Sub

    Public Sub New(ByVal callingWindow As Window, Optional ByRef TabNo As Integer = 0)

        MyBase.New(callingWindow)

        ' This call is required by the designer.
        InitializeComponent()

        POSOptions_ListBox.SelectedIndex = TabNo

    End Sub

    Private Sub POSSetup_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded

        'makes tab headers not visible in run time. 
        For Each currentTab As TabItem In POSSetup_TabControl.Items
            currentTab.Visibility = Visibility.Collapsed
        Next


    End Sub

    Private Sub POSOptions_ListBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles POSOptions_ListBox.SelectionChanged
        POSSetup_TabControl.SelectedIndex = POSOptions_ListBox.SelectedIndex

        Select Case POSOptions_ListBox.SelectedIndex
            Case 0
                DeleteButton.Visibility = Visibility.Hidden
                Load_CreditCardSetup()
            Case 1
                DeleteButton.Visibility = Visibility.Hidden
                Load_POSDepartmentList()
            Case 2
                DeleteButton.Visibility = Visibility.Hidden
                Load_TaxCounties()
            Case 3
                DeleteButton.Visibility = Visibility.Hidden
                Load_AR_Notices()
            Case 4
                DeleteButton.Visibility = Visibility.Hidden
                Load_Receipt_Options()
            Case 6
                DeleteButton.Visibility = Visibility.Hidden
                Load_GeneralPOS_Options()
            Case 7
                DeleteButton.Visibility = Visibility.Hidden
                Load_QB_Options()
            Case 8
                DeleteButton.Visibility = Visibility.Visible
                Load_Coupons()
        End Select
    End Sub

    Private Sub SaveButton_Click(sender As Object, e As RoutedEventArgs) Handles SaveButton.Click

        If MsgBox("Do you want to save changes to " & (CType(POSOptions_ListBox.SelectedValue, ListBoxItem)).Content.ToString() & "?", vbQuestion + vbYesNo) = MsgBoxResult.Yes Then

            Select Case POSOptions_ListBox.SelectedIndex

                Case 0
                    Save_CreditCardSetup()
                Case 1
                    Save_POSDepartments()
                Case 2
                    Save_TaxCounties()
                Case 3
                    Save_AR_Notices()
                Case 4
                    Save_Receipt_Options()
                Case 6
                    Save_GeneralPOS_Options()
                Case 8
                    Save_Coupons()
            End Select

        End If
    End Sub

#Region "Coupons"
    Private Sub Load_Coupons()
        Coupon_List = New List(Of Coupon)

        Dim buf As String
        Dim current_segment As String
        Dim CPN As Coupon


        buf = IO_GetSegmentSet(gShipriteDB, "SELECT SKU, Desc, Active From Inventory WHERE Department='COUPONS'")

        Do Until buf = ""
            current_segment = GetNextSegmentFromSet(buf)

            CPN = New Coupon
            CPN.SKU = ExtractElementFromSegment("SKU", current_segment, "")
            CPN.Desc = ExtractElementFromSegment("Desc", current_segment, "False")
            CPN.Active = ExtractElementFromSegment("Active", current_segment, "False")
            Coupon_List.Add(CPN)
        Loop


        Coupons_LV.ItemsSource = Coupon_List

    End Sub

    Private Sub Coupons_LV_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles Coupons_LV.SelectionChanged
        Dim buf As String
        If Coupons_LV.SelectedIndex = -1 Then Exit Sub

        buf = IO_GetSegmentSet(gShipriteDB, "SELECT * From Inventory WHERE SKU='" & Coupons_LV.SelectedItem.SKU & "'")

        CPN_SKU_TxtBx.Text = ExtractElementFromSegment("SKU", buf, "")
        CPN_Desc_TxtBx.Text = ExtractElementFromSegment("Desc", buf, "")
        CPN_Qty_TxtBx.Text = ExtractElementFromSegment("Coupon_Limit", buf, "0")
        CPN_StartDate.SelectedDate = ExtractElementFromSegment("Coupon_StartDate", buf)
        CPN_EndDate.SelectedDate = ExtractElementFromSegment("Coupon_EndDate", buf)
        CPN_AffectedSKUs_TxtBx.Text = ExtractElementFromSegment("Coupon_AppliesTo", buf, "")
        CPN_Template_TxtBx.Text = ExtractElementFromSegment("Coupon_Savings", buf, "")

        If ExtractElementFromSegment("Active", buf, "") Then
            CPN_ActiveON_RadioBtn.IsChecked = True
        Else
            CPN_ActiveOFF_RadioBtn.IsChecked = False
        End If

        Select Case ExtractElementFromSegment("Coupon_TypeOf", buf, "")
            Case "NAT"
                CPN_National_RadioBtn.IsChecked = True
            Case "LOC"
                CPN_Store_RadioBtn.IsChecked = True
            Case "COM"
                CPN_Competitor_RadioBtn.IsChecked = True
        End Select


    End Sub

    Private Sub DeletButton_Click(sender As Object, e As RoutedEventArgs) Handles DeleteButton.Click
        If CPN_SKU_TxtBx.Text = "" Then
            MsgBox("No Coupon Selected!")
            Exit Sub
        End If

        If MsgBoxResult.Yes = MsgBox("Are you sure you want to delete Coupon: '" & CPN_Desc_TxtBx.Text & "' ?", MsgBoxStyle.YesNo + MsgBoxStyle.Question) Then
            IO_UpdateSQLProcessor(gShipriteDB, "Delete From Inventory where SKU='" & CPN_SKU_TxtBx.Text & "'")

            Coupon_List.Remove(Coupons_LV.SelectedItem)
            Coupons_LV.Items.Refresh()

            MsgBox("Coupon Deleted Successfully!", vbOKOnly + vbInformation)
        End If

    End Sub


    Private Sub Save_Coupons()
        If CPN_SKU_TxtBx.Text = "" Or CPN_Desc_TxtBx.Text = "" Then
            MsgBox("SKU and Description fields cannot be empty!")
            Exit Sub
        End If

        Dim SQL As String
        Dim ret As Integer

        If Coupon_List.FindIndex(Function(x As Coupon) x.SKU = CPN_SKU_TxtBx.Text) = -1 Then
            'add new item
            SQL = "INSERT INTO Inventory (SKU, [Desc], Department, Coupon_Limit, Coupon_StartDate, Coupon_EndDate, Coupon_AppliesTo, Coupon_Savings, Active, Coupon_TypeOf) VALUES ("
            SQL = SQL & "'" & CPN_SKU_TxtBx.Text & "', "
            SQL = SQL & "'" & CPN_Desc_TxtBx.Text & "', "
            SQL = SQL & "'COUPONS', "
            SQL = SQL & CPN_Qty_TxtBx.Text & ", "
            SQL = SQL & "#" & CPN_StartDate.SelectedDate & "#, "
            SQL = SQL & "#" & CPN_EndDate.SelectedDate & "#, "
            SQL = SQL & "'" & CPN_AffectedSKUs_TxtBx.Text & "', "
            SQL = SQL & "'" & CPN_Template_TxtBx.Text & "', "

            If CPN_ActiveON_RadioBtn.IsChecked Then
                SQL = SQL & "True, "
            Else
                SQL = SQL & "False, "
            End If

            If CPN_National_RadioBtn.IsChecked Then
                SQL = SQL & "'NAT')"

            ElseIf CPN_Store_RadioBtn.IsChecked Then
                SQL = SQL & "'LOC')"

            ElseIf CPN_Competitor_RadioBtn.IsChecked Then
                SQL = SQL & "'COM')"
            End If

            ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

            If ret <> -1 Then
                MsgBox("New Coupon Added Successfully", vbOKOnly + vbInformation)
            End If

        Else
            'save changes to existing SKU
            SQL = "UPDATE Inventory Set "
            SQL = SQL & "[Desc]='" & CPN_Desc_TxtBx.Text & "', "
            SQL = SQL & "[Department]='COUPONS', "
            SQL = SQL & "[Coupon_Limit]=" & CPN_Qty_TxtBx.Text & ", "
            SQL = SQL & "[Coupon_StartDate]=#" & CPN_StartDate.SelectedDate & "#, "
            SQL = SQL & "[Coupon_EndDate]=#" & CPN_EndDate.SelectedDate & "#, "
            SQL = SQL & "[Coupon_AppliesTo]='" & CPN_AffectedSKUs_TxtBx.Text & "', "
            SQL = SQL & "[Coupon_Savings]='" & CPN_Template_TxtBx.Text & "', "

            If CPN_ActiveON_RadioBtn.IsChecked Then
                SQL = SQL & "[Active]=True, "
            Else
                SQL = SQL & "[Active]=False, "
            End If

            If CPN_National_RadioBtn.IsChecked Then
                SQL = SQL & "[Coupon_TypeOf]='NAT' "

            ElseIf CPN_Store_RadioBtn.IsChecked Then
                SQL = SQL & "[Coupon_TypeOf]='LOC' "

            ElseIf CPN_Competitor_RadioBtn.IsChecked Then
                SQL = SQL & "[Coupon_TypeOf]='COM' "
            End If

            SQL = SQL & "WHERE SKU='" & CPN_SKU_TxtBx.Text & "'"

            ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

            If ret <> -1 Then
                MsgBox("Changes to Coupon Saved Successfully!", vbOKOnly + vbInformation)
            End If
        End If

            Load_Coupons()
    End Sub

    Private Sub ColumnHeader_Click(sender As Object, e As RoutedEventArgs)
        'Sorts ListView by clicked Column Header
        'Dim columnHeader As GridViewColumnHeader = TryCast(e.OriginalSource, GridViewColumnHeader)
        Sort_LV_byColumn(Coupons_LV, TryCast(e.OriginalSource, GridViewColumnHeader), Last_Sort_Ascending, Last_Column_Sorted)

    End Sub


#End Region

#Region "QuickBooks Online"
    Private Sub Load_QB_Options()
        Dim QB_Mapping_List As List(Of QB_DepartmentMapping_Item) = New List(Of QB_DepartmentMapping_Item)
        Dim current_department As QB_DepartmentMapping_Item
        Dim QB_Master_Account_List As List(Of String) = Load_QB_Accounts()

        Dim buf As String
        Dim current_segment As String

        Add_DefaultAccount("Cash", QB_Mapping_List, QB_Master_Account_List)
        Add_DefaultAccount("Cash Over And Short", QB_Mapping_List, QB_Master_Account_List)
        Add_DefaultAccount("Check", QB_Mapping_List, QB_Master_Account_List)
        Add_DefaultAccount("Charge", QB_Mapping_List, QB_Master_Account_List)
        Add_DefaultAccount("Other", QB_Mapping_List, QB_Master_Account_List)
        Add_DefaultAccount("GiftCard", QB_Mapping_List, QB_Master_Account_List)
        Add_DefaultAccount("AccountsReceivable", QB_Mapping_List, QB_Master_Account_List)
        Add_DefaultAccount("Sales", QB_Mapping_List, QB_Master_Account_List)
        Add_DefaultAccount("Sales Returns & Allowances", QB_Mapping_List, QB_Master_Account_List)
        Add_DefaultAccount("Sales Tax Payable", QB_Mapping_List, QB_Master_Account_List)
        Add_DefaultAccount("Deposit On Sales", QB_Mapping_List, QB_Master_Account_List)
        Add_DefaultAccount("Miscellaneous Expenses", QB_Mapping_List, QB_Master_Account_List)



        buf = IO_GetSegmentSet(gShipriteDB, "SELECT Department From Departments")

        Do Until buf = ""
            current_segment = GetNextSegmentFromSet(buf)

            current_department = New QB_DepartmentMapping_Item
            current_department.Department = ExtractElementFromSegment("Department", current_segment, "")
            current_department.QB_AccountList = QB_Master_Account_List


            '---- TEST - load set QB Departments-----------------
            current_department.QB_Account = "1002 Test Account"
            '----------------------------------------------------



            QB_Mapping_List.Add(current_department)
        Loop

        QB_DepartmentMapping_LV.ItemsSource = QB_Mapping_List


    End Sub

    Private Sub Add_DefaultAccount(deptName As String, ByRef QB_List As List(Of QB_DepartmentMapping_Item), ByRef QB_Master_Account_List As List(Of String))
        Dim item As QB_DepartmentMapping_Item = New QB_DepartmentMapping_Item

        item.Department = deptName
        item.QB_AccountList = QB_Master_Account_List

        QB_List.Add(item)

    End Sub


    Private Function Load_QB_Accounts() As List(Of String)

        Dim QB_Account_List As List(Of String) = New List(Of String)

        QB_Account_List.Add("1001 Test Account")
        QB_Account_List.Add("1002 Test Account")
        QB_Account_List.Add("1003 Test Account")

        Return QB_Account_List
    End Function

#End Region

#Region "CreditCard Setup"
    Private Sub Load_CreditCardSetup()
        Try
            SmartSwiper_Enable_ChkBx.IsChecked = GetPolicyData(gShipriteDB, "MerchantWare")

            If Not IsFileExist("C:\windows\smartswiper.ini", False) Then
                SS_Credentials_Border.Visibility = Visibility.Hidden
                SS_Keyed_Border.Visibility = Visibility.Hidden
                SS_Genius_Border.Visibility = Visibility.Hidden

                SS_NotInstalled_Label.Visibility = Visibility.Visible
                Exit Sub
            End If

            SS_Name_TxtBx.Text = GetPolicyData(gSmartSwiperDB, "MW_DBA/Name")
            SS_SiteID_TxtBx.Text = GetPolicyData(gSmartSwiperDB, "MW_SiteID")
            SS_Key_TxtBx.Text = GetPolicyData(gSmartSwiperDB, "MW_Key")

            SS_Keyed_Name_TxtBx.Text = GetPolicyData(gSmartSwiperDB, "KEYED_MW_DBA/Name")
            SS_Keyed_SiteID_TxtBx.Text = GetPolicyData(gSmartSwiperDB, "KEYED_MW_SiteID")
            SS_Keyed_Key_TxtBx.Text = GetPolicyData(gSmartSwiperDB, "KEYED_MW_Key")

            SS_Enable_Genius_ChkBx.IsChecked = GetPolicyData(gSmartSwiperDB, "GeniusIsEnabled")

            SS_Genius_IP_TxtBx.Text = ExtractElementFromSegment("GENIUS_IP", IO_GetSegmentSet(gSmartSwiperReportsDB, "Select GENIUS_IP FROM SETUP"), "")

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Save_CreditCardSetup()
        Try
            UpdatePolicy(gShipriteDB, "MerchantWare", SmartSwiper_Enable_ChkBx.IsChecked)

            If IsFileExist("C:\windows\smartswiper.ini", False) Then
                UpdatePolicy(gSmartSwiperDB, "MW_DBA/Name", SS_Name_TxtBx.Text)
                UpdatePolicy(gSmartSwiperDB, "MW_SiteID", SS_SiteID_TxtBx.Text)
                UpdatePolicy(gSmartSwiperDB, "MW_Key", SS_Key_TxtBx.Text)

                UpdatePolicy(gSmartSwiperDB, "KEYED_MW_DBA/Name", SS_Keyed_Name_TxtBx.Text)
                UpdatePolicy(gSmartSwiperDB, "KEYED_MW_SiteID", SS_Keyed_SiteID_TxtBx.Text)
                UpdatePolicy(gSmartSwiperDB, "KEYED_MW_Key", SS_Keyed_Key_TxtBx.Text)

                UpdatePolicy(gSmartSwiperDB, "GeniusIsEnabled", SS_Enable_Genius_ChkBx.IsChecked)

                IO_UpdateSQLProcessor(gSmartSwiperReportsDB, "Update Setup set GENIUS_IP='" & SS_Genius_IP_TxtBx.Text & "'")
                Exit Sub
            End If

            MsgBox("Credit card setup options saved successfully!", vbInformation)

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

#End Region

#Region "POS Departments"

    Private Sub Save_POSDepartments()
        Try
            Dim SQL As String

            For Each item As POSDepartment In Department_List
                If item.DepartmentName <> "" Then

                    If item.Status = "Added" Then
                        SQL = "INSERT INTO Departments (Department, Taxable) VALUES ('" & item.DepartmentName & "', " & item.IsTaxable & ")"
                        IO_UpdateSQLProcessor(gShipriteDB, SQL)

                    ElseIf item.Status = "Deleted" Then
                        SQL = "DELETE * FROM Departments WHERE [ID]=" & item.ID
                        IO_UpdateSQLProcessor(gShipriteDB, SQL)
                    ElseIf item.Status = "Edited" Then
                        SQL = "UPDATE Departments SET [Department]='" & item.DepartmentName & "', [Taxable]=" & item.IsTaxable & " WHERE [ID]=" & item.ID
                        IO_UpdateSQLProcessor(gShipriteDB, SQL)

                    End If

                End If
            Next

            MsgBox("POS Departments Saved Successfully!", vbInformation)
            Load_POSDepartmentList()

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Load_POSDepartmentList()
        Dim buf As String
        Dim current_segment As String
        Dim current_department As POSDepartment
        Department_List = New List(Of POSDepartment)


        buf = IO_GetSegmentSet(gShipriteDB, "SELECT * From Departments")

        Do Until buf = ""
            current_segment = GetNextSegmentFromSet(buf)

            current_department = New POSDepartment
            current_department.DepartmentName = ExtractElementFromSegment("Department", current_segment, "")
            current_department.IsTaxable = ExtractElementFromSegment("Taxable", current_segment, "False")
            current_department.ID = ExtractElementFromSegment("ID", current_segment, "False")
            Department_List.Add(current_department)
        Loop


        Department_List = Department_List.OrderBy(Function(value As POSDepartment) value.DepartmentName).ToList
        Departments_LV.ItemsSource = Department_List

    End Sub


    Private Sub DeleteDept_Btn_Click(sender As Object, e As RoutedEventArgs) Handles DeleteDept_Btn.Click
        If Departments_LV.SelectedIndex = -1 Then
            MsgBox("Please select a department first", MsgBoxStyle.OkOnly + MsgBoxStyle.Exclamation, "Cannot delete Department")
            Exit Sub
        End If

        Dim item As POSDepartment = Departments_LV.SelectedItem

        If item.Status = "Added" Then
            Department_List.Remove(item)
        Else
            item.Status = "Deleted"
        End If


        Departments_LV.Items.Refresh()


    End Sub

    Private Sub AddDept_Btn_Click(sender As Object, e As RoutedEventArgs) Handles AddDept_Btn.Click
        If DepartmentName_TextBox.Text = "" Then
            MsgBox("Please enter a department name", MsgBoxStyle.OkOnly + MsgBoxStyle.Exclamation, "Department is blank")
            Exit Sub
        End If

        Dim current_department As POSDepartment = New POSDepartment
        current_department.DepartmentName = DepartmentName_TextBox.Text
        current_department.IsTaxable = True
        current_department.Status = "Added"

        Department_List.Add(current_department)
        Departments_LV.Items.Refresh()
        Departments_LV.ScrollIntoView(current_department)
    End Sub



    Private Sub POSDepartment_TextBox_TextChanged()
        If Departments_LV.SelectedIndex = -1 Then Exit Sub

        Dim item As POSDepartment = Departments_LV.SelectedItem

        If item.Status <> "Added" Then
            item.Status = "Edited"
        End If

    End Sub


#End Region


#Region "Tax Counties"

    Private Sub Save_TaxCounties()
        Try
            Dim SQL As String

            For Each item As TaxCounty In TaxCounty_List
                If item.County <> "" Then

                    item.TaxRate = Val(item.T1) + Val(item.T2) + Val(item.T3)

                    If item.Status = "Added" Then
                        SQL = "INSERT INTO CountyTaxes (County, State, TaxRate, T1, T2, T3) VALUES ('" & item.County & "', '" & item.State & "', " & item.TaxRate & ", " & Val(item.T1) & ", " & Val(item.T2) & ", " & Val(item.T3) & ")"
                        IO_UpdateSQLProcessor(gShipriteDB, SQL)

                    ElseIf item.Status = "Deleted" Then
                        SQL = "DELETE * FROM CountyTaxes WHERE [ID]=" & item.ID
                        IO_UpdateSQLProcessor(gShipriteDB, SQL)
                    ElseIf item.Status = "Edited" Then
                        SQL = "UPDATE CountyTaxes SET [County]='" & item.County & "', [State]='" & item.State & "', [TaxRate]=" & item.TaxRate & ", [T1]=" & Val(item.T1) & ", [T2]=" & Val(item.T2) & ", [T3]=" & Val(item.T3) & " WHERE [ID]=" & item.ID
                        IO_UpdateSQLProcessor(gShipriteDB, SQL)

                    End If

                    If item.DefaultCounty = True Then

                        UpdatePolicy(gShipriteDB, "DefaultCounty", item.County)

                    End If

                End If
            Next

            MsgBox("POS Sales Tax Setup Saved Successfully!", vbInformation)
            Load_TaxCounties()

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Add_TaxCounty_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Add_TaxCounty_Btn.Click
        If TotalTax_TxtBx.Text = "" Or County_TxtBx.Text = "" Or State_TxtBx.Text = "" Then
            MsgBox("Some fields were left empty. Please make sure that the County, State, and TotalTax fields are not blank!", MsgBoxStyle.OkOnly + MsgBoxStyle.Exclamation)
            Exit Sub
        End If

        Dim current_county As TaxCounty = New TaxCounty
        current_county.County = County_TxtBx.Text
        current_county.State = State_TxtBx.Text
        current_county.TaxRate = Val(TotalTax_TxtBx.Text)
        current_county.T1 = Val(T1_TxtBx.Text)
        current_county.T2 = Val(T2_TxtBx.Text)
        current_county.T3 = Val(T3_TxtBx.Text)
        current_county.Status = "Added"


        TaxCounty_List.Add(current_county)
        TaxCounties_LV.Items.Refresh()

    End Sub

    Private Sub Load_TaxCounties()
        Dim buf As String
        Dim current_segment As String
        Dim DefaultCounty As String
        Dim current_county As TaxCounty
        TaxCounty_List = New List(Of TaxCounty)

        DefaultCounty = GetPolicyData(gShipriteDB, "DefaultCounty", "")

        buf = IO_GetSegmentSet(gShipriteDB, "SELECT * From CountyTaxes")

        Do Until buf = ""
            current_segment = GetNextSegmentFromSet(buf)

            current_county = New TaxCounty

            current_county.County = ExtractElementFromSegment("County", current_segment, "")
            current_county.State = ExtractElementFromSegment("State", current_segment, "")
            current_county.TaxRate = ExtractElementFromSegment("TaxRate", current_segment, "0")
            current_county.T1 = ExtractElementFromSegment("T1", current_segment, "0")
            current_county.T2 = ExtractElementFromSegment("T2", current_segment, "0")
            current_county.T3 = ExtractElementFromSegment("T3", current_segment, "0")
            current_county.ID = ExtractElementFromSegment("ID", current_segment, "0")

            If DefaultCounty = current_county.County Then
                current_county.DefaultCounty = True
            Else
                current_county.DefaultCounty = False
            End If

            TaxCounty_List.Add(current_county)
        Loop

        TaxCounty_List = TaxCounty_List.OrderBy(Function(value As TaxCounty) value.County).ToList

        TaxCounties_LV.ItemsSource = TaxCounty_List
    End Sub

    Private Sub TaxCounties_TextBox_TextChanged()
        If TaxCounties_LV.SelectedIndex = -1 Then Exit Sub

        Dim item As TaxCounty = TaxCounties_LV.SelectedItem

        If item.Status <> "Added" Then
            item.Status = "Edited"
        End If

    End Sub

    Private Sub DeleteCounty_Btn_Click(sender As Object, e As RoutedEventArgs) Handles DeleteCounty_Btn.Click
        If TaxCounties_LV.SelectedIndex = -1 Then
            MsgBox("Please select a tax county first", MsgBoxStyle.OkOnly + MsgBoxStyle.Exclamation, "Cannot delete Tax County")
            Exit Sub
        End If

        Dim item As TaxCounty = TaxCounties_LV.SelectedItem

        If item.Status = "Added" Then
            TaxCounty_List.Remove(item)
        Else
            item.Status = "Deleted"
        End If

        TaxCounties_LV.Items.Refresh()


    End Sub

    Private Sub TaxBoxes_LostFocus(sender As Object, e As RoutedEventArgs) Handles T1_TxtBx.LostFocus, T2_TxtBx.LostFocus, T3_TxtBx.LostFocus
        TotalTax_TxtBx.Text = Val(T1_TxtBx.Text) + Val(T2_TxtBx.Text) + Val(T3_TxtBx.Text)
    End Sub


    Private Sub TaxBoxes_Listview_LostFocus()
        If TaxCounties_LV.SelectedIndex = -1 Then Exit Sub

        Dim item As TaxCounty = TaxCounties_LV.SelectedItem

        item.TaxRate = Val(item.T1) + Val(item.T2) + Val(item.T3)

        TaxCounties_LV.Items.Refresh()

    End Sub

    Private Sub Set_Default_CheckBox()
        'when the "Default" checkbox is checked, uncheck it for all other counties in the list.

        For Each item As TaxCounty In TaxCounty_List

            If item.County = TaxCounties_LV.SelectedItem.county Then
                item.DefaultCounty = True
            Else
                item.DefaultCounty = False
            End If

        Next

        TaxCounties_LV.Items.Refresh()
    End Sub

#End Region

#Region "AR Options"
    Private Sub Load_AR_Notices()
        Current_TxtBx.Text = GetPolicyData(gShipriteDB, "MessageCurrent", "")
        Plus30_TxtBx.Text = GetPolicyData(gShipriteDB, "MessagePlus30", "")
        Plus60_TxtBx.Text = GetPolicyData(gShipriteDB, "MessagePlus60", "")
        Plus90_TxtBx.Text = GetPolicyData(gShipriteDB, "MessagePlus90", "")
        Plus120_TxtBx.Text = GetPolicyData(gShipriteDB, "MessagePlus120", "")
    End Sub

    Private Sub Save_AR_Notices()
        UpdatePolicy(gShipriteDB, "MessageCurrent", Current_TxtBx.Text)
        UpdatePolicy(gShipriteDB, "MessagePlus30", Plus30_TxtBx.Text)
        UpdatePolicy(gShipriteDB, "MessagePlus60", Plus60_TxtBx.Text)
        UpdatePolicy(gShipriteDB, "MessagePlus90", Plus90_TxtBx.Text)
        UpdatePolicy(gShipriteDB, "MessagePlus120", Plus120_TxtBx.Text)

        MsgBox("Accounts Receivable Options Saved Successfully!", vbInformation)
    End Sub

#End Region

#Region "Receipt Options"
    Private Sub Load_Receipt_Options()

        Cash_CB.Text = GetPolicyData(gShipriteDB, "PrintTotalCash", "1")
        Credit_CB.Text = GetPolicyData(gShipriteDB, "PrintTotalCC", "1")
        Check_CB.Text = GetPolicyData(gShipriteDB, "PrintTotalCheck", "1")
        Other_CB.Text = GetPolicyData(gShipriteDB, "PrintTotalOther", "1")
        AR_CB.Text = GetPolicyData(gShipriteDB, "PrintTotalAccount", "1")



        ReceiptSignature_TxtBox.Text = GetPolicyData(gShipriteDB, "ReceiptSignatureText", "")
        ShippingDisclaimer_TxtBox.Text = GetPolicyData(gShipriteDB, "ShippingDisclaimer", "")
        ShippingDisclaimer_CheckBox.IsChecked = GetPolicyData(gShipriteDB, "EnableShippingDisclaimer", "True")
        Disclaimer2ndReceipt_CheckBox.IsChecked = GetPolicyData(gShipriteDB, "ShippingDisclaimer_2ndReceipt", "True")

        FacebookLink_TxtBox.Text = GetPolicyData(gShipriteDB, "ReceiptLink1", "")
        TwitterLink_TxtBox.Text = GetPolicyData(gShipriteDB, "ReceiptLink2", "")
        SurveyLink_TxtBox.Text = GetPolicyData(gShipriteDB, "ReceiptLink3", "")


        Dim ShippingReceiptOptions As String
        Dim count As Int16 = 1
        ShippingReceiptOptions = GetPolicyData(gShipriteDB, "ReceiptOnOffOptions", "11111")
        If ShippingReceiptOptions = "" Then ShippingReceiptOptions = "11111"

        For Each c As Char In ShippingReceiptOptions
            Select Case count
                Case 1
                    Receipt_Name_ChckBx.IsChecked = Val(c)
                Case 2
                    Receipt_Street_ChckBx.IsChecked = Val(c)
                Case 3
                    Receipt_CityStateZip_ChckBx.IsChecked = Val(c)
                Case 4
                    Receipt_Dimensions_ChckBx.IsChecked = Val(c)
                Case 5
                    Receipt_Weight_ChckBx.IsChecked = Val(c)

            End Select
            count = count + 1
        Next

        Load_Logo()
    End Sub

    Private Sub Save_Receipt_Options()

        UpdatePolicy(gShipriteDB, "PrintTotalCash", Cash_CB.Text)
        UpdatePolicy(gShipriteDB, "PrintTotalCC", Credit_CB.Text)
        UpdatePolicy(gShipriteDB, "PrintTotalCheck", Check_CB.Text)
        UpdatePolicy(gShipriteDB, "PrintTotalOther", Other_CB.Text)
        UpdatePolicy(gShipriteDB, "PrintTotalAccount", AR_CB.Text)


        UpdatePolicy(gShipriteDB, "ReceiptSignatureText", ReceiptSignature_TxtBox.Text)
        UpdatePolicy(gShipriteDB, "ShippingDisclaimer", ShippingDisclaimer_TxtBox.Text)
        UpdatePolicy(gShipriteDB, "EnableShippingDisclaimer", ShippingDisclaimer_CheckBox.IsChecked)
        UpdatePolicy(gShipriteDB, "ShippingDisclaimer_2ndReceipt", Disclaimer2ndReceipt_CheckBox.IsChecked)

        UpdatePolicy(gShipriteDB, "ReceiptLink1", FacebookLink_TxtBox.Text)
        UpdatePolicy(gShipriteDB, "ReceiptLink2", TwitterLink_TxtBox.Text)
        UpdatePolicy(gShipriteDB, "ReceiptLink3", SurveyLink_TxtBox.Text)

        Dim Opt As String
        Opt = Convert.ToInt32(Receipt_Name_ChckBx.IsChecked)
        Opt = Opt & Convert.ToInt32(Receipt_Street_ChckBx.IsChecked)
        Opt = Opt & Convert.ToInt32(Receipt_CityStateZip_ChckBx.IsChecked)
        Opt = Opt & Convert.ToInt32(Receipt_Dimensions_ChckBx.IsChecked)
        Opt = Opt & Convert.ToInt32(Receipt_Weight_ChckBx.IsChecked)

        UpdatePolicy(gShipriteDB, "ReceiptOnOffOptions", Opt)

        Setup_General.SaveImage(Logo_Image, gDBpath & "\Ads\ReceiptLogo\", LogoReceipt_Img)

        MsgBox("Receipt Options Saved Successfully!", vbInformation)

    End Sub

    Private Sub Logo_Browse_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Logo_Browse_Btn.Click
        Dim op As OpenFileDialog = New OpenFileDialog()
        op.Title = "Select a picture"
        op.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" & "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" & "Portable Network Graphic (*.png)|*.png"

        If op.ShowDialog() = True Then
            Logo_Image = New Ad_Image

            Logo_Image.ImageName = Get_FileName(op.FileName)
            Logo_Image.ImagePath = op.FileName
            Logo_Image.BitImage = Setup_General.GetBitMapImage(op.FileName)

            LogoReceipt_Img.Source = Logo_Image.BitImage
        End If
    End Sub

    Private Sub Logo_Delete_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Logo_Delete_Btn.Click
        If vbYes = MsgBox("Are you sure you want to remove the selected Logo?", vbQuestion + vbYesNo, "Remove Image") Then
            Logo_Image = Nothing
            LogoReceipt_Img.Source = Nothing

        End If
    End Sub

    Private Sub Load_Logo()
        Create_Folder(gDBpath & "\Ads\ReceiptLogo", False)
        Dim FileList = System.IO.Directory.GetFiles(gDBpath & "\Ads\ReceiptLogo").ToList

        If FileList.Count <> 0 Then
            LogoReceipt_Img.Source = Setup_General.GetBitMapImage(FileList(0))
        End If
    End Sub


#End Region

#Region "General POS Options"
    Private Sub Load_GeneralPOS_Options()
        DrawerID_TxtBox.Text = GetPolicyData(gReportsDB, "DrawerID", "")
    End Sub

    Private Sub Save_GeneralPOS_Options()
        UpdatePolicy(gReportsDB, "DrawerID", DrawerID_TxtBox.Text)
    End Sub

#End Region

    Protected Sub SelectCurrentItem(ByVal sender As Object, ByVal e As KeyboardFocusChangedEventArgs)
        'When clicking inside a textbox, select the listiew item that the textbox belongs to.
        Dim item As ListViewItem = CType(sender, ListViewItem)
        item.IsSelected = True
    End Sub

    Private Sub NumericTxtBox_PreviewTextInput(sender As Object, e As TextCompositionEventArgs) Handles TotalTax_TxtBx.PreviewTextInput, T1_TxtBx.PreviewTextInput, T2_TxtBx.PreviewTextInput, T3_TxtBx.PreviewTextInput

        Try
            Dim allowedchars As String = "0123456789.-"
            If allowedchars.IndexOf(CChar(e.Text)) = -1 Then e.Handled = True

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub CPN_Template_CmbBx_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles CPN_Template_CmbBx.SelectionChanged
        CPN_Template_TxtBx.Text = CPN_Template_CmbBx.SelectedValue.ToString
    End Sub


End Class
