Public Class InventoryManager
    Inherits CommonWindow

    Public Inventory_List As List(Of InventoryItem)
    Public Current_Inventory_List As List(Of InventoryItem)

    Public Department_List_FromInventory As List(Of InventoryItem)
    Public Department_List_FromDB As List(Of String)
    Public PackItemClasses_List As List(Of String)

    Public Last_Column_Sorted As String
    Public Last_Sort_Ascending As Boolean



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

    Private Sub InventoryManager_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded

        If (gIsProgramSecurityEnabled Or gIsPOSSecurityEnabled) AndAlso Not Check_Current_User_Permission("Inventory") Then
            ' MsgBox("You do not have the right to access this feature!", vbExclamation)
            Me.Close()
        End If

        Inventory_List = New List(Of InventoryItem)
        Department_List_FromInventory = New List(Of InventoryItem)
        Department_List_FromDB = New List(Of String)
        PackItemClasses_List = New List(Of String)

        Dim g As GridViewColumnHeader = New GridViewColumnHeader
        g.Content = "SKU"

        Load_PackMaterialsList()
        Load_Department_ComboBox()
        Load_Inventory_Items()
        'Current_Inventory_List = Inventory_List
        Display_Inventory_Listing(g)
        ShowHide_LV_Columns()
        Last_Column_Sorted = "SKU"
        Last_Sort_Ascending = True


    End Sub

    Private Sub ShowHide_LV_Columns()
        If Packmaster_ChkBx.IsChecked Then
            'show packmaster columns
            Department_GVColumn.Width = 0
            Cost_GVColumn.Width = 0
            Sell_GVColumn.Width = 0
            Count_GVColumn.Width = 0
            Qty_GVColumn.Width = 0
            ReOrder_GVColumn.Width = 0
            Active_GVColumn.Width = 0

            PackItem_GVColumn.Width = 60
            PackClass_GVColumn.Width = 80
            PackWeight_GVColumn.Width = 55
            PackLength_GVColumn.Width = 42
            PackWidth_GVColumn.Width = 42
            PackHeight_GVColumn.Width = 42
        Else
            'hide packmaster columns, show regular inventory columns
            Department_GVColumn.Width = 97
            Cost_GVColumn.Width = 65
            Sell_GVColumn.Width = 65
            Count_GVColumn.Width = 35
            Qty_GVColumn.Width = 37
            ReOrder_GVColumn.Width = 45
            Active_GVColumn.Width = 40

            PackItem_GVColumn.Width = 0
            PackClass_GVColumn.Width = 0
            PackWeight_GVColumn.Width = 0
            PackLength_GVColumn.Width = 0
            PackWidth_GVColumn.Width = 0
            PackHeight_GVColumn.Width = 0
        End If
    End Sub

    Private Sub Load_PackMaterialsList()
        PackItemClasses_List.Add("Boxes")
        PackItemClasses_List.Add("Wrap")
        PackItemClasses_List.Add("Filler")
        PackItemClasses_List.Add("Labor")
        PackItemClasses_List.Add("")
    End Sub

    Private Sub Load_Inventory_Items()
        Try

            Dim current_item As InventoryItem

            'Load data into temporary hidden listview. 
            Load_Temp_ListView()

            'from temporary listview, load data into the inventory list.
            For Each row As System.Data.DataRowView In TempData_LV.Items
                current_item = New InventoryItem

                current_item.SKU = row.Item(0)
                current_item.Desc = row.Item(1)
                current_item.Department = row.Item(2)

                If row.Item(3) = "" Then row.Item(3) = 0
                current_item.Cost = row.Item(3)

                If row.Item(4) = "" Then row.Item(4) = 0
                current_item.Sell = row.Item(4)

                If row.Item(5) = "" Then row.Item(5) = 0
                current_item.Quantity = row.Item(5)

                If row.Item(6) = "" Then row.Item(6) = 0
                current_item.WarningQty = row.Item(6)

                current_item.Zero = row.Item(7)
                current_item.Active = row.Item(8)


                'PackMaster
                If row.Item(9) = "" Then row.Item(9) = False
                current_item.PackagingMaterials = row.Item(9)

                    If row.Item(10) = "" Then row.Item(10) = ""
                    current_item.MaterialsClass = row.Item(10)

                    If row.Item(11) = "" Then row.Item(11) = 0
                    current_item.Weight = row.Item(11)

                    If row.Item(12) = "" Then row.Item(12) = 0
                    current_item.L = row.Item(12)

                    If row.Item(13) = "" Then row.Item(13) = 0
                    current_item.W = row.Item(13)

                    If row.Item(14) = "" Then row.Item(14) = 0
                    current_item.H = row.Item(14)

                current_item.Department_List = Department_List_FromDB.OrderBy(Function(x) x).ToList
                current_item.PackMaterials_Class_List = PackItemClasses_List

                current_item.OriginalSKU = current_item.SKU

                Inventory_List.Add(current_item)

            Next

            'set inventory list as source for the main display listview
            Inventory_ListView.ItemsSource = Inventory_List.OrderBy(Function(value As InventoryItem) value.SKU).ToList()

            Department_List_FromInventory = Inventory_List.GroupBy(Function(x) x.Department).Select(Function(x) x.First).ToList
            Department_List_FromInventory = Department_List_FromInventory.OrderBy(Function(x) x.Department).ToList

            Departments_ListBox.ItemsSource = Department_List_FromInventory

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try

    End Sub

    Public Sub Load_Temp_ListView()
        Try
            Dim SQL As String = "SELECT SKU, Desc, Department, Cost, Sell, Quantity, WarningQty, Zero, Active, 
PackagingMaterials, MaterialsClass, Weight, L, W, H  From Inventory"
            TempData_LV.DataContext = Nothing
            TempData_LV.View = New GridView
            TempData_LV.ItemsSource = Nothing
            TempData_LV.Items.Clear()

            Dim DT As System.Data.DataTable = New System.Data.DataTable

            Dim searchGrid As GridView = TempData_LV.View

            searchGrid.AllowsColumnReorder = False

            DT.Columns.Add("SKU")
            DT.Columns.Add("Desc")
            DT.Columns.Add("Department")
            DT.Columns.Add("Cost")
            DT.Columns.Add("Sell")
            DT.Columns.Add("Quantity")
            DT.Columns.Add("WarningQty")
            DT.Columns.Add("Zero")
            DT.Columns.Add("Active")

            DT.Columns.Add("PackagingMaterials")
            DT.Columns.Add("MaterialsClass")
            DT.Columns.Add("Weight")
            DT.Columns.Add("L")
            DT.Columns.Add("W")
            DT.Columns.Add("H")

            TempData_LV.View = searchGrid

            IO_LoadListView(TempData_LV, DT, gShipriteDB, SQL, 15)

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Inventory_Changed()
        If Not IsNothing(Inventory_ListView.SelectedItem) Then
            Inventory_ListView.SelectedItem.Status = "Edited"
        End If
    End Sub

    Public Sub Load_Department_ComboBox()
        Try
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
                Department_List_FromDB.Add(fieldValue)
            Loop

            Department_ComboBox.ItemsSource = Department_List_FromDB


        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Display_Inventory_Listing(Optional ByRef OrderByColumn As GridViewColumnHeader = Nothing)
        Try
            Current_Inventory_List = New List(Of InventoryItem)


            'show selected departments
            For Each x As InventoryItem In Departments_ListBox.SelectedItems
                Current_Inventory_List.AddRange(Inventory_List.FindAll(Function(value As InventoryItem) value.Department = x.Department))
            Next

            'if no departments selected, show all
            If Current_Inventory_List.Count = 0 Then
                Current_Inventory_List = Inventory_List
            End If

            If Active_ComboBox.SelectedIndex = 1 Then
                Current_Inventory_List = Current_Inventory_List.FindAll(Function(value As InventoryItem) value.Active = True)
            ElseIf Active_ComboBox.SelectedIndex = 2 Then
                Current_Inventory_List = Current_Inventory_List.FindAll(Function(value As InventoryItem) value.Active = False)
            End If

            'Search by SKU
            If SearchSKU_TxtBox.Text <> "" And SearchSKU_TxtBox.Text <> "SKU" Then
                Current_Inventory_List = Current_Inventory_List.FindAll(Function(value As InventoryItem) value.SKU.ToUpper.Contains(Trim(SearchSKU_TxtBox.Text.ToUpper)) = True)
            End If

            'Search by Description
            If SearchDesc_TxtBox.Text <> "" And SearchDesc_TxtBox.Text <> "Description" Then
                Current_Inventory_List = Current_Inventory_List.FindAll(Function(value As InventoryItem) value.Desc.ToUpper.Contains(SearchDesc_TxtBox.Text.ToUpper) = True)
            End If


            'Sort by the clicked ColumnHeader
            If Not IsNothing(OrderByColumn) Then

                Select Case OrderByColumn.Content
                    Case "SKU"
                        If Last_Column_Sorted = OrderByColumn.Content And Last_Sort_Ascending Then
                            Current_Inventory_List = Current_Inventory_List.OrderByDescending(Function(value As InventoryItem) value.SKU).ToList()
                            Last_Sort_Ascending = False
                        Else
                            Current_Inventory_List = Current_Inventory_List.OrderBy(Function(value As InventoryItem) value.SKU).ToList()
                            Last_Sort_Ascending = True
                        End If

                    Case "Description"
                        If Last_Column_Sorted = OrderByColumn.Content And Last_Sort_Ascending Then
                            Current_Inventory_List = Current_Inventory_List.OrderByDescending(Function(value As InventoryItem) value.Desc).ToList()
                            Last_Sort_Ascending = False
                        Else
                            Current_Inventory_List = Current_Inventory_List.OrderBy(Function(value As InventoryItem) value.Desc).ToList()
                            Last_Sort_Ascending = True
                        End If

                    Case "Department"
                        If Last_Column_Sorted = OrderByColumn.Content And Last_Sort_Ascending Then
                            Current_Inventory_List = Current_Inventory_List.OrderByDescending(Function(value As InventoryItem) value.Department).ToList()
                            Last_Sort_Ascending = False
                        Else
                            Current_Inventory_List = Current_Inventory_List.OrderBy(Function(value As InventoryItem) value.Department).ToList()
                            Last_Sort_Ascending = True
                        End If

                    Case "Cost"
                        If Last_Column_Sorted = OrderByColumn.Content And Last_Sort_Ascending Then
                            Current_Inventory_List = Current_Inventory_List.OrderByDescending(Function(value As InventoryItem) value.Cost).ToList()
                            Last_Sort_Ascending = False
                        Else
                            Current_Inventory_List = Current_Inventory_List.OrderBy(Function(value As InventoryItem) value.Cost).ToList()
                            Last_Sort_Ascending = True
                        End If

                    Case "Sell"
                        If Last_Column_Sorted = OrderByColumn.Content And Last_Sort_Ascending Then
                            Current_Inventory_List = Current_Inventory_List.OrderByDescending(Function(value As InventoryItem) value.Sell).ToList()
                            Last_Sort_Ascending = False
                        Else
                            Current_Inventory_List = Current_Inventory_List.OrderBy(Function(value As InventoryItem) value.Sell).ToList()
                            Last_Sort_Ascending = True
                        End If

                    Case "Count"
                        If Last_Column_Sorted = OrderByColumn.Content And Last_Sort_Ascending Then
                            Current_Inventory_List = Current_Inventory_List.OrderByDescending(Function(value As InventoryItem) value.Zero).ToList()
                            Last_Sort_Ascending = False
                        Else
                            Current_Inventory_List = Current_Inventory_List.OrderBy(Function(value As InventoryItem) value.Zero).ToList()
                            Last_Sort_Ascending = True
                        End If

                    Case "QTY"
                        If Last_Column_Sorted = OrderByColumn.Content And Last_Sort_Ascending Then
                            Current_Inventory_List = Current_Inventory_List.OrderByDescending(Function(value As InventoryItem) value.Quantity).ToList()
                            Last_Sort_Ascending = False
                        Else
                            Current_Inventory_List = Current_Inventory_List.OrderBy(Function(value As InventoryItem) value.Quantity).ToList()
                            Last_Sort_Ascending = True
                        End If

                    Case "ReOrder"
                        If Last_Column_Sorted = OrderByColumn.Content And Last_Sort_Ascending Then
                            Current_Inventory_List = Current_Inventory_List.OrderByDescending(Function(value As InventoryItem) value.WarningQty).ToList()
                            Last_Sort_Ascending = False
                        Else
                            Current_Inventory_List = Current_Inventory_List.OrderBy(Function(value As InventoryItem) value.WarningQty).ToList()
                            Last_Sort_Ascending = True
                        End If

                    Case "Active"
                        If Last_Column_Sorted = OrderByColumn.Content And Last_Sort_Ascending Then
                            Current_Inventory_List = Current_Inventory_List.OrderByDescending(Function(value As InventoryItem) value.Active).ToList()
                            Last_Sort_Ascending = False
                        Else
                            Current_Inventory_List = Current_Inventory_List.OrderBy(Function(value As InventoryItem) value.Active).ToList()
                            Last_Sort_Ascending = True
                        End If

                    Case "Pack Item"
                        If Last_Column_Sorted = OrderByColumn.Content And Last_Sort_Ascending Then
                            Current_Inventory_List = Current_Inventory_List.OrderByDescending(Function(value As InventoryItem) value.PackagingMaterials).ToList()
                            Last_Sort_Ascending = False
                        Else
                            Current_Inventory_List = Current_Inventory_List.OrderBy(Function(value As InventoryItem) value.PackagingMaterials).ToList()
                            Last_Sort_Ascending = True
                        End If

                    Case "Pack Class"
                        If Last_Column_Sorted = OrderByColumn.Content And Last_Sort_Ascending Then
                            Current_Inventory_List = Current_Inventory_List.OrderByDescending(Function(value As InventoryItem) value.MaterialsClass).ToList()
                            Last_Sort_Ascending = False
                        Else
                            Current_Inventory_List = Current_Inventory_List.OrderBy(Function(value As InventoryItem) value.MaterialsClass).ToList()
                            Last_Sort_Ascending = True
                        End If

                    Case "Weight"
                        If Last_Column_Sorted = OrderByColumn.Content And Last_Sort_Ascending Then
                            Current_Inventory_List = Current_Inventory_List.OrderByDescending(Function(value As InventoryItem) value.Weight).ToList()
                            Last_Sort_Ascending = False
                        Else
                            Current_Inventory_List = Current_Inventory_List.OrderBy(Function(value As InventoryItem) value.Weight).ToList()
                            Last_Sort_Ascending = True
                        End If

                    Case "Length"
                        If Last_Column_Sorted = OrderByColumn.Content And Last_Sort_Ascending Then
                            Current_Inventory_List = Current_Inventory_List.OrderByDescending(Function(value As InventoryItem) value.L).ToList()
                            Last_Sort_Ascending = False
                        Else
                            Current_Inventory_List = Current_Inventory_List.OrderBy(Function(value As InventoryItem) value.L).ToList()
                            Last_Sort_Ascending = True
                        End If

                    Case "Width"
                        If Last_Column_Sorted = OrderByColumn.Content And Last_Sort_Ascending Then
                            Current_Inventory_List = Current_Inventory_List.OrderByDescending(Function(value As InventoryItem) value.W).ToList()
                            Last_Sort_Ascending = False
                        Else
                            Current_Inventory_List = Current_Inventory_List.OrderBy(Function(value As InventoryItem) value.W).ToList()
                            Last_Sort_Ascending = True
                        End If

                    Case "Height"
                        If Last_Column_Sorted = OrderByColumn.Content And Last_Sort_Ascending Then
                            Current_Inventory_List = Current_Inventory_List.OrderByDescending(Function(value As InventoryItem) value.H).ToList()
                            Last_Sort_Ascending = False
                        Else
                            Current_Inventory_List = Current_Inventory_List.OrderBy(Function(value As InventoryItem) value.H).ToList()
                            Last_Sort_Ascending = True
                        End If

                End Select

                Last_Column_Sorted = OrderByColumn.Content

            Else
                'Default Sort by SKU
                If Not IsNothing(Inventory_List) Then
                    Current_Inventory_List = Current_Inventory_List.OrderBy(Function(value As InventoryItem) value.SKU).ToList()
                    Last_Sort_Ascending = True
                    Last_Column_Sorted = "SKU"
                End If
            End If


            Inventory_ListView.ItemsSource = Current_Inventory_List
            Inventory_ListView.Items.Refresh()


        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub



    Private Sub Departments_ListBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles Departments_ListBox.SelectionChanged
        Display_Inventory_Listing()
    End Sub



    Private Sub Search_TxtBox_LostFocus(sender As Object, e As RoutedEventArgs) Handles SearchSKU_TxtBox.LostFocus, SearchDesc_TxtBox.LostFocus
        Try
            If sender.Text = "" Then
                If sender.name = "SearchSKU_TxtBox" Then
                    sender.Text = "SKU"
                Else
                    sender.Text = "Description"
                End If
            End If

            Display_Inventory_Listing()
        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Search_TxtBox_KeyDown(sender As Object, e As KeyEventArgs) Handles SearchSKU_TxtBox.KeyDown, SearchDesc_TxtBox.KeyDown
        Try

            If e.Key = Key.Enter Then
                Display_Inventory_Listing()
            End If

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Search_TxtBox_GotFocus(sender As Object, e As RoutedEventArgs) Handles SearchSKU_TxtBox.GotFocus, SearchDesc_TxtBox.GotFocus
        Try
            sender.Text = ""

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub OpenInventoryDetail_Button_Click(sender As Object, e As RoutedEventArgs) Handles OpenInventoryDetail_Button.Click

         

        Dim win As New InventoryDetail(Me, Current_Inventory_List, Inventory_ListView.SelectedIndex)
        win.ShowDialog(Me)

        Inventory_ListView.Items.Refresh()

    End Sub

    Private Sub Inventory_ListView_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles Inventory_ListView.MouseDoubleClick
        OpenInventoryDetail_Button_Click(sender, Nothing)
    End Sub

    Private Sub GridViewColumnHeader_Click(sender As Object, e As RoutedEventArgs)

        Display_Inventory_Listing(sender)

    End Sub

    Private Sub Active_ComboBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles Active_ComboBox.SelectionChanged
        Display_Inventory_Listing()
    End Sub

    Private Sub RemoveItem_Button_Click(sender As Object, e As RoutedEventArgs) Handles RemoveItem_Button.Click
        Try

            If Delete_GVColumn.Width = 0 Then
                'make delete column visible
                MsgBox("Check items to delete, then press the DELETE button again!", vbInformation)
                Delete_GVColumn.Width = 45
                Exit Sub
            End If


            Dim SQL As String
            Dim selected_Items As List(Of InventoryItem) = New List(Of InventoryItem)
            Dim SKUs_to_delete As String = ""



            For Each item As InventoryItem In Current_Inventory_List
                If item.Delete = True Then
                    SKUs_to_delete = SKUs_to_delete & item.SKU & " - " & item.Desc & vbCrLf
                    selected_Items.Add(item)
                End If
            Next

            If selected_Items.Count = 0 Then
                MsgBox("Cannot Delete Inventory Items. No Item Selected for Deletion!", MsgBoxStyle.Critical + vbOKOnly)
                Delete_GVColumn.Width = 0
                Exit Sub
            End If

            If MsgBox("Do you want to delete the following SKU/s: " & vbCrLf & SKUs_to_delete, MsgBoxStyle.YesNo + MsgBoxStyle.Question, "Delete Inventory Item") = MsgBoxResult.No Then
                Exit Sub
            End If

            If selected_Items.Count = 1 Then
                'Delete 1 SKU
                SQL = "DELETE From Inventory Where SKU= '" & selected_Items(0).SKU & "'"

                If IO_UpdateSQLProcessor(gShipriteDB, SQL) = 0 Then
                    Exit Sub
                End If

            Else
                'Delete Multiple SKU's
                SQL = "DELETE From Inventory Where"
                For Each item As InventoryItem In selected_Items
                    SQL = SQL & " SKU='" & item.SKU & "' OR"
                Next
                SQL = SQL.Substring(0, SQL.Length - 3)


                If IO_UpdateSQLProcessor(gShipriteDB, SQL) = 0 Then
                    Exit Sub
                End If

            End If

            'Delete Items from the list/view
            For Each item As InventoryItem In selected_Items
                Inventory_List.Remove(item)
            Next

            Display_Inventory_Listing()

            MsgBox("Following SKU/s were deleted successfully: " & vbCrLf & SKUs_to_delete, MsgBoxStyle.Information, "Delete Inventory Items")
            Delete_GVColumn.Width = 0

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub SaveButton_Click(sender As Object, e As RoutedEventArgs) Handles SaveButton.Click
        Try
            Dim SQL As String
            Dim count As Integer = 0

            For Each item As InventoryItem In Inventory_List
                If item.SKU <> "" Then

                    If item.Status = "Edited" Then
                        SQL = "Update Inventory set [Desc]= '" & item.Desc &
                            "' , [Department]='" & item.Department &
                            "', [Cost]=" & item.Cost &
                            ", [Sell]=" & item.Sell &
                            ", [Zero]=" & item.Zero &
                            ", [Quantity]=" & item.Quantity &
                            ", [WarningQty]=" & item.WarningQty &
                            ", [Active]=" & item.Active &
                            ", [PackagingMaterials]=" & item.PackagingMaterials &
                            ", [MaterialsClass]='" & item.MaterialsClass & "'" &
                            ", [Weight]=" & item.Weight &
                            ", [L]=" & item.L &
                            ", [W]=" & item.W &
                            ", [H]=" & item.H &
                            ", [SKU]='" & item.SKU & "'" &
                            " WHERE [SKU]='" & item.OriginalSKU & "'"

                        IO_UpdateSQLProcessor(gShipriteDB, SQL)

                        Tickler.Check_Tickler_Inventory_Flag(item.SKU)

                        item.Status = ""
                        count = count + 1
                    End If


                End If
            Next


            If count > 0 Then
                MsgBox("Inventory changes were saved succesfully!", MsgBoxStyle.Information)
            Else
                MsgBox("No changes to save!", MsgBoxStyle.Exclamation)
            End If


        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Protected Sub SelectCurrentItem(ByVal sender As Object, ByVal e As KeyboardFocusChangedEventArgs)
        'When clicking inside a textbox, select the listiew item that the textbox belongs to.
        Dim item As ListViewItem = CType(sender, ListViewItem)

        item.IsSelected = True
    End Sub

    Private Sub PrintButton_Click(sender As Object, e As RoutedEventArgs) Handles PrintButton.Click
         

        Dim win As New ReportsManager(Me, 4)
        win.ShowDialog(Me)
    End Sub
    Private Sub Packmaster_ChkBx_Checked(sender As Object, e As RoutedEventArgs) Handles Packmaster_ChkBx.Checked, Packmaster_ChkBx.Unchecked
        ShowHide_LV_Columns()

        If Packmaster_ChkBx.IsChecked Then
            'when checking packmaster checkbox, display packaging materials on top
            If Not IsNothing(Inventory_List) Then
                Current_Inventory_List = Current_Inventory_List.OrderByDescending(Function(value As InventoryItem) value.PackagingMaterials).ThenBy(Function(value As InventoryItem) value.MaterialsClass).ToList()
                Last_Sort_Ascending = True
                Last_Column_Sorted = "SKU"

                Inventory_ListView.ItemsSource = Current_Inventory_List
                Inventory_ListView.Items.Refresh()
            End If
        End If
    End Sub



#Region "Add New Inventory Item"

    Private Function Check_Input_data() As Boolean
        Try
            If Cost_TxtBox.Text = "" Then Cost_TxtBox.Text = "$ 0.00"
            If Sell_TxtBox.Text = "" Then Sell_TxtBox.Text = "$ 0.00"
            If Qty_TxtBox.Text = "" Then Qty_TxtBox.Text = "0"
            If ReOrder_TxtBox.Text = "" Then ReOrder_TxtBox.Text = "0"

            If SKU_TxtBox.Text = "" Or Desc_TxtBox.Text = "" Or Department_ComboBox.Text = "" Then
                MsgBox("Error! SKU, Descriptions, and Department field cannot be empty!", MsgBoxStyle.Information, "ShipRite Inventory")
                Return False
            End If

            Return True

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
        Return False
    End Function



    Private Sub CalculateMarkup() Handles Cost_TxtBox.TextChanged, Sell_TxtBox.TextChanged
        Try
            Dim c As Double
            Dim s As Double


            If IsNumeric(Cost_TxtBox.Text) And IsNumeric(Sell_TxtBox.Text) Then
                c = CDbl(Cost_TxtBox.Text)
                s = CDbl(Sell_TxtBox.Text)

                Markup_TxtBox.Text = ((s - c) / c).ToString("#0.##%")

            End If

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Function Check_if_SKU_already_In_Inventory(ByRef SKU As String) As Boolean
        If IO_GetSegmentSet(gShipriteDB, "Select SKU From Inventory where SKU='" & SKU & "'") = "" Then
            Return False
        Else
            Return True
        End If

    End Function

    Private Function Clear_AddItem_Screen()
        SKU_TxtBox.Text = ""
        Desc_TxtBox.Text = ""
        'Department_ComboBox.Text = ""
        Cost_TxtBox.Text = ""
        Sell_TxtBox.Text = ""
        'Count_CheckBox.IsChecked = false
        Qty_TxtBox.Text = ""
        ReOrder_TxtBox.Text = ""
        'Active_CheckBox.IsChecked
    End Function

    Private Sub Add_New_Item_to_DB_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Add_New_Item_to_DB_Btn.Click
        Dim SQL As String

        If Not Check_Input_data() Then Exit Sub

        If Check_if_SKU_already_In_Inventory(SKU_TxtBox.Text) Then
            MsgBox("SKU already exists in inventory", vbExclamation)
            Exit Sub
        End If

        Dim new_item As InventoryItem = New InventoryItem
        new_item = Load_Item_Data()

        SQL = "INSERT INTO Inventory ([SKU], [Desc], [Department], [Cost], [Sell], [Zero], [Quantity], [WarningQty], [Active]) VALUES ("
        SQL = SQL & "'" & new_item.SKU & "', '" & new_item.Desc & "', '" & new_item.Department & "', '" & new_item.Cost & "', '" & new_item.Sell & "', " & new_item.Zero & ", '" & new_item.Quantity & "', '" & new_item.WarningQty & "', " & new_item.Active & ")"
        Debug.Print(SQL)

        'update database
        If IO_UpdateSQLProcessor(gShipriteDB, SQL) = 0 Then
            Exit Sub
        End If

        'update inventory view
        Inventory_List.Add(new_item)
        Display_Inventory_Listing()

        MsgBox("SKU: " & vbCrLf & new_item.SKU & " - " & new_item.Desc & " saved succesfully!", MsgBoxStyle.Information, "Add New Inventory Item")
        Inventory_ListView.ScrollIntoView(new_item)

        Clear_AddItem_Screen()
    End Sub

    Private Function Load_Item_Data() As InventoryItem
        Try
            Dim current_item As InventoryItem = New InventoryItem With {
            .SKU = SKU_TxtBox.Text,
            .Desc = Desc_TxtBox.Text,
            .Department = Department_ComboBox.Text,
            .Cost = Cost_TxtBox.Text,
            .Sell = Sell_TxtBox.Text,
            .Zero = Count_CheckBox.IsChecked,
            .Quantity = Qty_TxtBox.Text,
            .WarningQty = ReOrder_TxtBox.Text,
            .Active = Active_CheckBox.IsChecked
        }

            Return current_item

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
        Return Nothing
    End Function



    Private Sub AddItem_Button_Click(sender As Object, e As RoutedEventArgs) Handles AddItem_Button.Click
        Try

            Add_New_Item_Popup.IsOpen = True

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Close_Popup_Click(sender As Object, e As RoutedEventArgs) Handles Close_Popup.Click
        Add_New_Item_Popup.IsOpen = False
    End Sub
    Private Sub Markup_TxtBox_LostFocus(sender As Object, e As RoutedEventArgs) Handles Markup_TxtBox.LostFocus
        Try
            Dim c As Double
            Dim m As Double

            If IsNumeric(Cost_TxtBox.Text) And IsNumeric(Markup_TxtBox.Text) Then
                c = CDbl(Cost_TxtBox.Text)
                m = CDbl(Markup_TxtBox.Text) / 100

                Sell_TxtBox.Text = FormatCurrency((c * m + c).ToString())


            End If

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Cost_TxtBox_LostFocus(sender As Object, e As RoutedEventArgs) Handles Cost_TxtBox.LostFocus, Sell_TxtBox.LostFocus
        Try
            Dim Current_TxtBox = DirectCast(sender, TextBox)

            If Current_TxtBox.Text <> "" And IsNumeric(Current_TxtBox.Text) Then
                Current_TxtBox.Text = FormatCurrency(Current_TxtBox.Text)
            Else
                Current_TxtBox.Text = FormatCurrency(0)
            End If

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub


    Private Sub CostSell_TxtBox_GotFocus(sender As Object, e As RoutedEventArgs) Handles Cost_TxtBox.GotFocus, Sell_TxtBox.GotFocus, Markup_TxtBox.GotFocus
        Dim Current_TxtBox = DirectCast(sender, TextBox)

        Current_TxtBox.Text = Current_TxtBox.Text.Replace("$", "")
        Current_TxtBox.Text = Current_TxtBox.Text.Replace("%", "")
        Current_TxtBox.Text = Current_TxtBox.Text.Replace("NaN", "")
    End Sub

#End Region

#Region "Navigation Buttons"
    Private Sub BackButton_ClickInventory(sender As Object, e As RoutedEventArgs)
        If Check_If_Inventory_Edited_and_CloseScreen() Then
            BackButton_Click(sender, e)
        End If

    End Sub

    Private Sub ForwardButton_ClickInventory(sender As Object, e As RoutedEventArgs)
        If Check_If_Inventory_Edited_and_CloseScreen() Then
            ForwardButton_Click(sender, e)
        End If

    End Sub

    Private Sub RefreshButton_ClickInventory(sender As Object, e As RoutedEventArgs)
        If Check_If_Inventory_Edited_and_CloseScreen() Then
            RefreshButton_Click(sender, e)
        End If

    End Sub

    Private Sub HomeButton_ClickInventory(sender As Object, e As RoutedEventArgs)
        If Check_If_Inventory_Edited_and_CloseScreen() Then
            HomeButton_Click(sender, e)
        End If

    End Sub

    Private Sub CloseButton_ClickInventory(sender As Object, e As RoutedEventArgs)
        If Check_If_Inventory_Edited_and_CloseScreen() Then
            CloseButton_Click(sender, e)
        End If

    End Sub



    Private Function Check_If_Inventory_Edited_and_CloseScreen() As Boolean
        Dim IsEdited As Boolean = False

        For Each item As InventoryItem In Inventory_List

            If item.Status = "Edited" Then
                IsEdited = True
                Exit For
            End If

        Next

        If IsEdited = False Then
            'Exit Inventory, not edits made
            Return True
        End If

        Dim answer As MsgBoxResult = MsgBox("Your changes to Inventory have not been saved! Would you like to save changes now?", vbQuestion + vbYesNoCancel)

        Select Case answer
            Case vbYes
                'Exit inventory with saving
                SaveButton_Click(Nothing, Nothing)
                Return True
            Case vbNo
                'Exit Inventory without saving
                Return True

            Case vbCancel
                'Don't exit inventory
                Return False
        End Select

        Return True

    End Function



#End Region

End Class
