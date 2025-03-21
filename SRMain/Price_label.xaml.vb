

Public Class Price_label
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

    Private Sub Price_label_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded

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

        Load_Inventory_Items()
        'Current_Inventory_List = Inventory_List
        Display_Inventory_Listing(g)

        Last_Column_Sorted = "SKU"
        Last_Sort_Ascending = True


    End Sub

    Private Sub ShowHide_LV_Columns()



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

            Inventory_List.Clear()
            'from temporary listview, load data into the inventory list.
            For Each row As System.Data.DataRowView In TempData_LV.Items
                current_item = New InventoryItem

                current_item.SKU = row.Item(0)
                current_item.Desc = row.Item(1)
                current_item.Department = row.Item(2)


                If row.Item(3) = "" Then row.Item(3) = 0
                current_item.Sell = row.Item(3)

                If row.Item(4) = "" Then row.Item(4) = 0
                current_item.Quantity = 0

                If row.Item(5) = "" Then row.Item(5) = 0
                current_item.MSRP = row.Item(5)

                current_item.Department_List = Department_List_FromDB
                current_item.PackMaterials_Class_List = PackItemClasses_List

                current_item.OriginalSKU = current_item.SKU

                Inventory_List.Add(current_item)

            Next

            'set inventory list as source for the main display listview
            Inventory_ListView.ItemsSource = Inventory_List

            Department_List_FromInventory = Inventory_List.GroupBy(Function(x) x.Department).Select(Function(x) x.First).ToList
            Department_List_FromInventory = Department_List_FromInventory.OrderBy(Function(x) x.Department).ToList

            Departments_ListBox.ItemsSource = Department_List_FromInventory

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try

    End Sub

    Public Sub Load_Temp_ListView()
        Try
            Dim SQL As String = "SELECT SKU, Desc, Department, Sell, Quantity, MSRP  From Inventory"
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
            DT.Columns.Add("Sell")
            DT.Columns.Add("Quantity")
            DT.Columns.Add("MSRP")


            TempData_LV.View = searchGrid

            IO_LoadListView(TempData_LV, DT, gShipriteDB, SQL, 6)

            TempData_LV.ItemsSource = DT.DefaultView
        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Inventory_Changed()
        If Not IsNothing(Inventory_ListView.SelectedItem) Then
            Inventory_ListView.SelectedItem.Status = "Edited"
        End If
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






    Private Sub GridViewColumnHeader_Click(sender As Object, e As RoutedEventArgs)

        Display_Inventory_Listing(sender)

    End Sub

    Private Sub Save_temp()
        Try
            Dim SQL As String
            Dim count As Integer = 0
            For Each item As InventoryItem In Inventory_List
                For i As Integer = 1 To item.Quantity
                    SQL = "INSERT INTO Inventory ([SKU], [Desc], [Quantity], [Department], [Sell], [MSRP]) VALUES ('" & item.SKU & "', '" & item.Desc & "', " & item.Quantity & ",'" & item.Department & "','" & item.Sell & "','" & item.MSRP & "')"
                    IO_UpdateSQLProcessor(gReportWriter, SQL)

                Next

            Next





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
        Dim group_dept As Boolean
        Dim MRSP As Boolean

        Dim SQL As String
        SQL = "DELETE FROM Inventory"
        IO_UpdateSQLProcessor(gReportWriter, SQL)

        group_dept = group_department.IsChecked

        MRSP = include_msrp.IsChecked

        Save_temp()



        Try

            Me.Cursor = Cursors.Wait()

            Dim report As New ShipRiteReports._ReportObject()

            report.ReportParameters.Add(MRSP)
            If group_dept = "True" Then

                report.ReportName = "InventoryLabels_byDept.rpt"

            Else
                report.ReportName = "InventoryLabels.rpt"
            End If



            Dim reportPrev As New ReportPreview(report)
            Cursor = Cursors.Arrow
            reportPrev.ShowDialog()

        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to report [Inventory Listing]...")
        Finally : Cursor = Cursors.Arrow
        End Try


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
    Private Sub Departments_ListBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles Departments_ListBox.SelectionChanged
        Display_Inventory_Listing()
    End Sub



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
        Me.Close()

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

    Private Sub RadioButton_Checked(sender As Object, e As RoutedEventArgs)

    End Sub

    Private Sub RadioButton_Checked_1(sender As Object, e As RoutedEventArgs)

    End Sub

    Private Sub qty_all_TextChanged() Handles qty_all.TextChanged

        ' Convert the entered text to an integer value
        Dim enteredValue As Integer
        enteredValue = 1

        If Not String.IsNullOrWhiteSpace(qty_all.Text) Then

            If Integer.TryParse(qty_all.Text, enteredValue) Then
                For Each item As InventoryItem In Inventory_List
                    item.Quantity = enteredValue

                Next
            End If
            Inventory_ListView.Items.Refresh()
        End If


    End Sub


#End Region
End Class

