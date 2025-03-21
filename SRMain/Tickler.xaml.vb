Public Class Tickler
    Inherits CommonWindow

    Private Class InventoryNotice
        Public Property ID As Integer
        Public Property DueDate As Date
        Public Property SKU As String
        Public Property Desc As String
        Public Property Status As String
    End Class

    Private Class ActionNotice
        Public Property ID As Integer
        Public Property DueDate As Date
        Public Property AssignedTo As String
        Public Property Customer As String
        Public Property Priority As String
        Public Property Details As String
        Public Property Status As String
    End Class

    'Private Class MailboxNotice
    '    Public Property MBX_No As Integer
    '    Public Property MBX_Name As String
    '    Public Property Exp_Date As DateTime
    'End Class


    Dim InventoryNotice_List As List(Of InventoryNotice)
    Dim ActionNotice_List As List(Of ActionNotice)
    Dim RenewalNotice_List As List(Of Mailbox)
    Dim ExpirationNotice_List As List(Of Mailbox)
    Dim CancellationNotice_List As List(Of Mailbox)

    Public Sub New()

        MyBase.New()

        ' This call is required by the designer.
        InitializeComponent()

    End Sub

    Public Sub New(ByVal callingWindow As Window, Optional CustomerSegment As String = "")

        MyBase.New(callingWindow)

        ' This call is required by the designer.
        InitializeComponent()

        If CustomerSegment <> "" Then
            ACT_Customer_Btn.Content = ExtractElementFromSegment("Name", CustomerSegment, "") & vbCrLf & ExtractElementFromSegment("FullAddress", CustomerSegment, "")
            ACT_CustID_Lbl.Text = ExtractElementFromSegment("ID", CustomerSegment, "")
            ACT_Users_CmbBx.SelectedIndex = 0
            ACT_Details_TxtBx.Focus()
        End If

    End Sub

    Private Sub Tickler_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Try
            InventoryNotice_List = New List(Of InventoryNotice)
            ActionNotice_List = New List(Of ActionNotice)

            RenewalNotice_List = New List(Of Mailbox)
            ExpirationNotice_List = New List(Of Mailbox)
            CancellationNotice_List = New List(Of Mailbox)

            LoadActionNotices()
            LoadInventoryNotices()
            LoadUserList()

            Load_Mailbox_Notices()

            For Each currentTab As TabItem In Details_TabCtrl.Items
                currentTab.Visibility = Visibility.Collapsed
            Next

            ShowHide_Action_ListItems()

            SaveButton.Visibility = Visibility.Hidden
            Remove_Button.Visibility = Visibility.Hidden

            Load_Time_ComboBoxes()

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try

    End Sub

    Public Sub Load_Time_ComboBoxes()

        For i As Integer = 0 To 12
            DueHour_Cmb.Items.Add(i.ToString("00"))
        Next

        DueHour_Cmb.SelectedIndex = 0

        For i As Integer = 0 To 55 Step 5
            DueMinute_Cmb.Items.Add(i.ToString("00"))
        Next

        DueMinute_Cmb.SelectedIndex = 0
    End Sub

    Public Shared Function Get_Open_Tickler_Count() As Integer
        Dim count As Integer = 0

        count = ExtractElementFromSegment("Tally", IO_GetSegmentSet(gShipriteDB, "Select Count(ID) As Tally FROM Tickler WHERE Status='Open' and DueDate <=#" & Today.ToShortDateString & "#"), "0")

        count = count + CheckMailBoxNoticesOnStartup()

        Return count

    End Function

    Private Sub TxtBx_PreviewTextInput(sender As Object, e As TextCompositionEventArgs) Handles Inv_QtyOnHand_TxtBx.PreviewTextInput, Inv_WarningQty_TxtBx.PreviewTextInput, Repeat_NoOfDays_TxtBx.PreviewTextInput
        Try
            Dim allowedchars As String = "0123456789"
            If allowedchars.IndexOf(CChar(e.Text)) = -1 Then e.Handled = True

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Priority_CmbBx_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles Priority_CmbBx.SelectionChanged
        ShowHide_Action_ListItems()

    End Sub

    Private Sub Status_CmbBx_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles Status_CmbBx.SelectionChanged
        ShowHide_Action_ListItems()
        ShowHide_Inventory_ListItems()
    End Sub

    Private Sub SaveButton_Click(sender As Object, e As RoutedEventArgs) Handles SaveButton.Click
        If ActionNotices_LB.SelectedIndex <> -1 Then
            Save_Action_Notice()

        ElseIf InventoryNotices_LB.SelectedIndex <> -1 Then
            Save_Inventory_Notice()

        End If

    End Sub

    Private Sub Remove_Button_Click(sender As Object, e As RoutedEventArgs) Handles Remove_Button.Click
        Try
            If MsgBox("Are you sure you want to permanently Delete the selected notice?", vbQuestion + vbYesNo) = MsgBoxResult.Yes Then

                Dim ID As String = ""

                If ActionNotices_LB.SelectedIndex <> -1 Then
                    'delete action notice
                    ID = ActionNotices_LB.SelectedItem.ID
                    ActionNotice_List.Remove(ActionNotices_LB.SelectedItem)
                    ActionNotices_LB.Items.Refresh()

                ElseIf InventoryNotices_LB.SelectedIndex <> -1 Then
                    'delete inventory notice
                    ID = InventoryNotices_LB.SelectedItem.ID
                    InventoryNotice_List.Remove(InventoryNotices_LB.SelectedItem)
                    InventoryNotices_LB.Items.Refresh()

                End If

                IO_UpdateSQLProcessor(gShipriteDB, "Delete * From Tickler WHERE ID=" & ID)

                MsgBox("Item Deleted Successfully!", vbInformation)
            End If

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub



#Region "Inventory Notices"
    Private Sub LoadInventoryNotices()
        Try
            Dim Buffer As String
            Dim current_segment As String
            Dim Notice As InventoryNotice

            Buffer = IO_GetSegmentSet(gShipriteDB, "Select ID, Details, DueDate, Status, SKU FROM Tickler WHERE Category='Inventory Low' ORDER BY DueDate DESC")

            Do Until Buffer = ""
                current_segment = GetNextSegmentFromSet(Buffer)

                Notice = New InventoryNotice
                Notice.ID = ExtractElementFromSegment("ID", current_segment)
                Notice.DueDate = ExtractElementFromSegment("DueDate", current_segment)
                Notice.Desc = ExtractElementFromSegment("Details", current_segment)
                Notice.SKU = ExtractElementFromSegment("SKU", current_segment)
                Notice.Status = ExtractElementFromSegment("Status", current_segment)
                InventoryNotice_List.Add(Notice)
            Loop

            InventoryNotices_LB.ItemsSource = InventoryNotice_List

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Save_Inventory_Notice()
        Dim Segment As String = ""
        Dim SQL As String

        If Open_RB.IsChecked = True Then
            Segment = AddElementToSegment(Segment, "Status", "Open")
        ElseIf Closed_RB.IsChecked = True Then
            Segment = AddElementToSegment(Segment, "Status", "Closed")
            Segment = AddElementToSegment(Segment, "DateCompleted", Today.ToShortDateString)
            Segment = AddElementToSegment(Segment, "TimeCompleted", Now.ToLongTimeString)
        End If

        Segment = AddElementToSegment(Segment, "Notes", Notes_TxtBx.Text)
        Segment = AddElementToSegment(Segment, "CompletedBy", CompletedBy_CmbBx.Text)


        SQL = MakeUpdateSQLFromSchema("Tickler", Segment, gTicklerSchema, True, False)
        SQL = SQL & InventoryNotices_LB.SelectedItem.ID

        IO_UpdateSQLProcessor(gShipriteDB, SQL)
        MsgBox("Changes To Selected Invenotry Notice Saved Successfully!", vbInformation)

        ShowHide_Inventory_ListItems()
    End Sub

    Private Sub InventoryNotices_LB_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles InventoryNotices_LB.SelectionChanged
        Try
            If InventoryNotices_LB.SelectedIndex = -1 Then Exit Sub
            Add_Button.Visibility = Visibility.Hidden
            SaveButton.Visibility = Visibility.Visible
            Remove_Button.Visibility = Visibility.Visible
            OpenClose_Border.Visibility = Visibility.Visible

            Dim current_segment As String


            InventoryDetails_Tab.IsSelected = True
            ActionNotices_LB.UnselectAll()
            Cancellation_LV.UnselectAll()
            Expiration_LV.UnselectAll()
            Renewal_LV.UnselectAll()

            DetailHeader_TxtBlck.Text = "DETAILS - Inventory Notice"

            current_segment = IO_GetSegmentSet(gShipriteDB, "Select DueDate, Status, CompletedBy, Notes FROM Tickler WHERE ID=" & InventoryNotices_LB.SelectedItem.ID)

            INV_Desc_Lbl.Content = InventoryNotices_LB.SelectedItem.Desc
            Inv_SKU_Lbl.Content = InventoryNotices_LB.SelectedItem.SKU

            Inv_Date_TxtBx.Text = Date.Parse(ExtractElementFromSegment("DueDate", current_segment)).ToShortDateString
            Inv_Status_TxtBx.Text = ExtractElementFromSegment("Status", current_segment)

            If ExtractElementFromSegment("Status", current_segment) = "Open" Then
                Open_RB.IsChecked = True
            Else
                Closed_RB.IsChecked = True
            End If

            CompletedBy_CmbBx.Text = ExtractElementFromSegment("CompletedBy", current_segment, "")
            Notes_TxtBx.Text = ExtractElementFromSegment("Notes", current_segment, "")

            current_segment = IO_GetSegmentSet(gShipriteDB, "Select Quantity, WarningQty FROM Inventory WHERE SKU='" & InventoryNotices_LB.SelectedItem.SKU & "'")

            Inv_QtyOnHand_TxtBx.Text = ExtractElementFromSegment("Quantity", current_segment)
            Inv_WarningQty_TxtBx.Text = ExtractElementFromSegment("WarningQty", current_segment)





        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Public Shared Sub CreateInventoryTickler(ByVal SKU As String, ByVal Desc As String)
        Try
            Dim Tickler_Schema As String = IO_GetFieldsCollection(gShipriteDB, "Tickler", "", True, False, True)
            Dim SQL As String
            Dim Segment As String = ""

            Segment = AddElementToSegment(Segment, "For", "Manager")
            Segment = AddElementToSegment(Segment, "CID", "0")
            Segment = AddElementToSegment(Segment, "DueDate", Today.ToShortDateString)
            Segment = AddElementToSegment(Segment, "EnteredBy", "UpdateInventory")
            Segment = AddElementToSegment(Segment, "Category", "Inventory Low")
            Segment = AddElementToSegment(Segment, "Status", "Open")
            Segment = AddElementToSegment(Segment, "DateEntered", Today.ToShortDateString)
            Segment = AddElementToSegment(Segment, "Details", Desc)
            Segment = AddElementToSegment(Segment, "TimeEntered", Now.ToLongTimeString)
            Segment = AddElementToSegment(Segment, "Priority", "Urgent")
            Segment = AddElementToSegment(Segment, "Mailbox", "0")
            Segment = AddElementToSegment(Segment, "SKU", SKU)


            SQL = MakeInsertSQLFromSchema("Tickler", Segment, Tickler_Schema, True)
            IO_UpdateSQLProcessor(gShipriteDB, SQL)

            SQL = "Update Inventory SET WarningSent=True WHERE SKU='" & SKU & "'"
            IO_UpdateSQLProcessor(gShipriteDB, SQL)


        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub



    Private Sub OpenInventoryDetail_Btn_Click(sender As Object, e As RoutedEventArgs) Handles OpenInventoryDetail_Btn.Click
        Try
            Dim item As InventoryItem = New InventoryItem
            Dim itemList As List(Of InventoryItem) = New List(Of InventoryItem)

            item.SKU = Inv_SKU_Lbl.Content
            item.Desc = INV_Desc_Lbl.Content

            itemList.Add(item)

            Dim win As New InventoryDetail(Me, itemList, 0)
            win.ShowDialog(Me)

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub

    Private Sub OpenInventory_Btn_Click(sender As Object, e As RoutedEventArgs) Handles OpenInventory_Btn.Click
        Try
            Dim win As New InventoryManager(Me)
            win.ShowDialog(Me)

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub



    Private Sub ShowHide_Inventory_ListItems()
        Try
            If IsNothing(InventoryNotice_List) Then Exit Sub

            Dim LBItem As ListBoxItem
            Dim Status As String = (CType(Status_CmbBx.SelectedItem, ComboBoxItem)).Content.ToString()

            For Each item As InventoryNotice In InventoryNotice_List
                LBItem = InventoryNotices_LB.ItemContainerGenerator.ContainerFromItem(item)

                If Status.ToUpper = "OPEN" Or Status.ToUpper = "CLOSED" Then
                    If item.Status.ToUpper = Status Then
                        LBItem.Visibility = Visibility.Visible
                    Else
                        LBItem.Visibility = Visibility.Collapsed
                    End If

                Else
                    LBItem.Visibility = Visibility.Visible
                End If

            Next

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub



    Private Sub UpdateQty_Btn_Click(sender As Object, e As RoutedEventArgs) Handles UpdateQty_Btn.Click
        Try
            If Inv_QtyOnHand_TxtBx.Text = "" Then Exit Sub
            If Inv_SKU_Lbl.Content = "" Then Exit Sub

            IO_UpdateSQLProcessor(gShipriteDB, "Update Inventory SET Quantity=" & Inv_QtyOnHand_TxtBx.Text & " WHERE SKU='" & Inv_SKU_Lbl.Content & "'")

            MsgBox("Quantity for " & INV_Desc_Lbl.Content & " Updated!", vbInformation)

            Check_Tickler_Inventory_Flag(Inv_SKU_Lbl.Content)

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub UpdateWarningQty_Btn_Click(sender As Object, e As RoutedEventArgs) Handles UpdateWarningQty_Btn.Click
        Try
            If Inv_WarningQty_TxtBx.Text = "" Then Exit Sub
            If Inv_SKU_Lbl.Content = "" Then Exit Sub

            IO_UpdateSQLProcessor(gShipriteDB, "Update Inventory SET WarningQty=" & Inv_WarningQty_TxtBx.Text & " WHERE SKU='" & Inv_SKU_Lbl.Content & "'")

            MsgBox("Warning Quantity for " & INV_Desc_Lbl.Content & " Updated!", vbInformation)

            Check_Tickler_Inventory_Flag(Inv_SKU_Lbl.Content)

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Public Shared Sub Check_Tickler_Inventory_Flag(ByVal SKU As String)
        Try
            If SKU = "" Then Exit Sub

            Dim segment As String = IO_GetSegmentSet(gShipriteDB, "SELECT Quantity, WarningQty, Zero, WarningSent FROM Inventory WHERE SKU='" & SKU & "'")


            If segment <> "" Then
                If ExtractElementFromSegment("Zero", segment) = "True" And ExtractElementFromSegment("WarningSent", segment) = True Then

                    If CDbl(ExtractElementFromSegment("Quantity", segment)) > CDbl(ExtractElementFromSegment("WarningQty", segment)) Then
                        IO_UpdateSQLProcessor(gShipriteDB, "Update Inventory SET WarningSent=False WHERE SKU='" & SKU & "'")
                    End If
                End If
            End If

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

#End Region

#Region "ActionItems Menu"

    Private Sub Clear_Action_Details()

        ActionDetails_Tab.IsSelected = True

        InventoryNotices_LB.UnselectAll()
        Cancellation_LV.UnselectAll()
        Expiration_LV.UnselectAll()
        Renewal_LV.UnselectAll()

        DetailHeader_TxtBlck.Text = "DETAILS - ACTION ITEM"


        ACT_Details_TxtBx.Text = ""
        ACT_Users_CmbBx.Text = ""
        ACT_DueDatePicker.SelectedDate = Today
        DueMinute_Cmb.SelectedIndex = 0
        DueHour_Cmb.SelectedIndex = 0
        DueAMPM_Cmb.SelectedIndex = 0
        ACT_Priority_CmbBx.SelectedIndex = 1 'Routine
        ACT_CustID_Lbl.Text = ""
        ACT_Customer_Btn.Content = ""


        CompletedBy_CmbBx.Text = ""
        Notes_TxtBx.Text = ""
        If gIsProgramSecurityEnabled Then
            OpenedBy_TxtBx.Text = gCurrentUser
        End If



        Open_RB.IsChecked = False
        Closed_RB.IsChecked = False
        Repeat_CmbBx.SelectedIndex = 0 'Once



    End Sub

    Private Sub Save_Action_Notice()
        Try
            Dim Segment As String = ""
            Dim SQL As String

            If IsNothing(ACT_DueDatePicker.SelectedDate) Then
                MsgBox("Please select a date first!", vbExclamation)
                Exit Sub

            ElseIf ACT_Details_TxtBx.Text = "" Then
                MsgBox("Please enter the To Do Details!", vbExclamation)
                Exit Sub

            End If


            Create_ActionNotice_Segment(Segment, False)

            SQL = MakeUpdateSQLFromSchema("Tickler", Segment, gTicklerSchema, True, False)
            SQL = SQL & ActionNotices_LB.SelectedItem.ID

            If Repeat_CmbBx.Text <> "Once" Then
                SQL = SQL.Replace("[Status] = 'Closed'", "[Status] = 'Repetitive'")
                SQL = SQL.Replace("[Status] = 'Open'", "[Status] = 'Repetitive'")
            End If

            IO_UpdateSQLProcessor(gShipriteDB, SQL)
            MsgBox("Changes To Selected Action Item Saved Successfully!", vbInformation)

            LoadActionNotices()
            ShowHide_Action_ListItems()

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Add_Button_Click(sender As Object, e As RoutedEventArgs) Handles Add_Button.Click
        If MsgBox("Are you sure you want to add a new Action Item?", vbQuestion + vbYesNo) = MsgBoxResult.Yes Then
            If IsNothing(ACT_DueDatePicker.SelectedDate) Then
                MsgBox("Please select a date first!", vbExclamation)
                Exit Sub

            ElseIf ACT_Details_TxtBx.Text = "" Then
                MsgBox("Please enter the To Do Details!", vbExclamation)
                Exit Sub

            End If

            Add_New_Action_Notice()
        End If
    End Sub

    Private Sub Add_New_Action_Notice()
        Try
            Dim Segment As String = ""
            Dim SQL As String

            Create_ActionNotice_Segment(Segment, True)


            If Repeat_CmbBx.SelectedIndex <> 0 Then
                'Repetitive Action Item

                If MsgBox("You are creating a Repetitive Series. The series will create a new Action Item " & Repeat_CmbBx.Text & "!" & vbCrLf & vbCrLf &
                       "To view, edit, or cancel the repetitive series, please select 'Repetitive' in the Display Drop Down!" & vbCrLf & vbCrLf &
                       "Are you sure you want to proceed?", vbYesNo + vbQuestion) <> MsgBoxResult.Yes Then
                    Exit Sub
                End If

                Segment = Segment.Replace("Status Open", "Status Repetitive")
                Segment = Segment.Replace("Status Closed", "Status Repetitive")
                Segment = AddElementToSegment(Segment, "Repeat", Repeat_CmbBx.Text)


                If Repeat_CmbBx.Text = "Weekly" Then
                    Segment = AddElementToSegment(Segment, "RepeatPeriod", Repeat_WeekDay_CmbBx.Text)

                ElseIf Repeat_CmbBx.Text = "Monthly" Then
                    If Repeat_NoOfDays_TxtBx.Text = "" Or Repeat_NoOfDays_TxtBx.Text = "0" Then
                        MsgBox("Please enter in the Day of the Month to repeat the Notice!", vbExclamation + vbOKOnly)
                        Exit Sub
                    End If

                    Segment = AddElementToSegment(Segment, "RepeatPeriod", Repeat_NoOfDays_TxtBx.Text)
                End If

            End If


            SQL = MakeInsertSQLFromSchema("Tickler", Segment, gTicklerSchema, True)
            IO_UpdateSQLProcessor(gShipriteDB, SQL)


            If Repeat_CmbBx.SelectedIndex <> 0 Then
                MsgBox("New Repetitive Series Added Successfully!", vbInformation)
            Else
                MsgBox("New Action Item Added Successfully!", vbInformation)
            End If


            LoadActionNotices()
            ShowHide_Action_ListItems()

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Create_ActionNotice_Segment(ByRef Segment As String, ByVal IsNewNotice As Boolean)
        Try

            Dim customer_segment As String = ""

            If ACT_Users_CmbBx.Text = "" Then
                Segment = AddElementToSegment(Segment, "For", "All Employees")
            Else
                Segment = AddElementToSegment(Segment, "For", ACT_Users_CmbBx.Text)
            End If

            If ACT_CustID_Lbl.Text = "" Then
                'no customer assigned
                Segment = AddElementToSegment(Segment, "CID", "0")
            Else
                'set customer ID and Name
                Segment = AddElementToSegment(Segment, "CID", ACT_CustID_Lbl.Text)
                customer_segment = IO_GetSegmentSet(gShipriteDB, "SELECT Name, Phone FROM Contacts WHERE ID = " & ACT_CustID_Lbl.Text)
                Segment = AddElementToSegment(Segment, "Customer", ExtractElementFromSegment("Name", customer_segment, "") & " " & ExtractElementFromSegment("Phone", customer_segment, ""))
            End If

            If IsNewNotice Then
                Segment = AddElementToSegment(Segment, "DateEntered", Today.ToShortDateString)
                Segment = AddElementToSegment(Segment, "TimeEntered", Now.ToLongTimeString)
                Segment = AddElementToSegment(Segment, "Status", "Open")
            End If

            Segment = AddElementToSegment(Segment, "DueDate", ACT_DueDatePicker.SelectedDate)
            Segment = AddElementToSegment(Segment, "TimeDue", DueHour_Cmb.Text & "." & DueMinute_Cmb.Text & "." & DueAMPM_Cmb.Text)
            Segment = AddElementToSegment(Segment, "Category", "Action Item")
            Segment = AddElementToSegment(Segment, "Details", ACT_Details_TxtBx.Text)
            Segment = AddElementToSegment(Segment, "Priority", ACT_Priority_CmbBx.Text)
            Segment = AddElementToSegment(Segment, "Mailbox", "0")
            Segment = AddElementToSegment(Segment, "StringDate", Today.ToShortDateString)

            If Open_RB.IsChecked = True Then
                Segment = AddElementToSegment(Segment, "Status", "Open")
            ElseIf Closed_RB.IsChecked = True Then
                Segment = AddElementToSegment(Segment, "Status", "Closed")
                Segment = AddElementToSegment(Segment, "DateCompleted", Today.ToShortDateString)
                Segment = AddElementToSegment(Segment, "TimeCompleted", Now.ToLongTimeString)
            End If

            Segment = AddElementToSegment(Segment, "Notes", Notes_TxtBx.Text)
            Segment = AddElementToSegment(Segment, "CompletedBy", CompletedBy_CmbBx.Text)

            If gIsProgramSecurityEnabled Then
                Segment = AddElementToSegment(Segment, "OpenendBy", gCurrentUser)
            End If

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub


    Private Sub Repeat_CmbBx_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles Repeat_CmbBx.SelectionChanged
        Try
            If IsNothing(Repeat_CmbBx) Then Exit Sub

            Select Case Repeat_CmbBx.SelectedIndex
                Case 0
                    'Once
                    Repeat_WeekDay_CmbBx.Visibility = Visibility.Hidden
                    Repeat_NoOfDays_TxtBx.Visibility = Visibility.Hidden
                    Repeat_Lbl.Visibility = Visibility.Hidden

                Case 1
                    'Daily
                    Repeat_WeekDay_CmbBx.Visibility = Visibility.Hidden
                    Repeat_NoOfDays_TxtBx.Visibility = Visibility.Hidden
                    Repeat_Lbl.Visibility = Visibility.Hidden

                Case 2
                    'Weekly
                    Repeat_WeekDay_CmbBx.Visibility = Visibility.Visible
                    Repeat_NoOfDays_TxtBx.Visibility = Visibility.Hidden
                    Repeat_Lbl.Visibility = Visibility.Visible
                    Repeat_Lbl.Text = "Day:"

                Case 3
                    'Monthly
                    Repeat_WeekDay_CmbBx.Visibility = Visibility.Hidden
                    Repeat_NoOfDays_TxtBx.Visibility = Visibility.Visible
                    Repeat_Lbl.Visibility = Visibility.Visible
                    Repeat_Lbl.Text = "DayOfMonth:"
            End Select

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub LoadUserList()
        Try
            Dim buf As String
            Dim current_segment As String
            buf = IO_GetSegmentSet(gShipriteDB, "SELECT DisplayName From Users")

            ACT_Users_CmbBx.Items.Add("All Employees")
            ACT_Users_CmbBx.Items.Add("Manager")

            Do Until buf = ""
                current_segment = GetNextSegmentFromSet(buf)
                ACT_Users_CmbBx.Items.Add(ExtractElementFromSegment("DisplayName", current_segment, ""))
                CompletedBy_CmbBx.Items.Add(ExtractElementFromSegment("DisplayName", current_segment, ""))
            Loop

            If gIsProgramSecurityEnabled Then
                CompletedBy_CmbBx.SelectedItem = gCurrentUser
                CompletedBy_CmbBx.IsEnabled = False

            End If


        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub
    Private Sub LoadActionNotices()
        Try

            Dim Buffer As String
            Dim current_segment As String
            Dim Notice As ActionNotice

            ActionNotice_List.Clear()

            Buffer = IO_GetSegmentSet(gShipriteDB, "Select ID, Details, DueDate, Customer, Priority, For, Status FROM Tickler WHERE Category='Action Item' ORDER BY DueDate DESC")

            Do Until Buffer = ""
                current_segment = GetNextSegmentFromSet(Buffer)

                Notice = New ActionNotice
                Notice.ID = ExtractElementFromSegment("ID", current_segment)
                Notice.DueDate = ExtractElementFromSegment("DueDate", current_segment)
                Notice.Details = ExtractElementFromSegment("Details", current_segment)
                Notice.AssignedTo = ExtractElementFromSegment("For", current_segment)
                Notice.Customer = ExtractElementFromSegment("Customer", current_segment)
                Notice.Customer = New IO.StringReader(Notice.Customer).ReadLine() 'Display only first line of address
                Notice.Priority = ExtractElementFromSegment("Priority", current_segment)
                Notice.Status = ExtractElementFromSegment("Status", current_segment)

                ActionNotice_List.Add(Notice)

            Loop

            ActionNotices_LB.ItemsSource = ActionNotice_List
            ActionNotices_LB.Items.Refresh()


        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub ActionNotices_LB_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles ActionNotices_LB.SelectionChanged
        Try
            If ActionNotices_LB.SelectedIndex = -1 Then Exit Sub
            Add_Button.Visibility = Visibility.Visible
            SaveButton.Visibility = Visibility.Visible
            Remove_Button.Visibility = Visibility.Visible

            OpenClose_Border.Visibility = Visibility.Visible

            Dim current_segment As String
            Dim Customer_Segment As String

            Dim TimeDueList As List(Of String)

            ActionDetails_Tab.IsSelected = True

            InventoryNotices_LB.UnselectAll()
            Cancellation_LV.UnselectAll()
            Expiration_LV.UnselectAll()
            Renewal_LV.UnselectAll()

            DetailHeader_TxtBlck.Text = "DETAILS - ACTION ITEM"

            current_segment = IO_GetSegmentSet(gShipriteDB, "Select * FROM Tickler WHERE ID=" & ActionNotices_LB.SelectedItem.ID)

            ACT_Details_TxtBx.Text = ExtractElementFromSegment("Details", current_segment, "")
            ACT_Users_CmbBx.Text = ExtractElementFromSegment("For", current_segment, "")
            ACT_DueDatePicker.SelectedDate = ExtractElementFromSegment("DueDate", current_segment, "")

            TimeDueList = ExtractElementFromSegment("TimeDue", current_segment, "").Split(".").ToList
            DueHour_Cmb.SelectedValue = TimeDueList(0)
            If DueHour_Cmb.SelectedIndex = -1 Then DueHour_Cmb.SelectedIndex = 0

            If TimeDueList.Count > 1 Then
                DueMinute_Cmb.SelectedValue = TimeDueList(1)

                If TimeDueList.Count > 2 Then
                    If TimeDueList(2) = "AM" Then
                        DueAMPM_Cmb.SelectedIndex = 0
                    Else
                        DueAMPM_Cmb.SelectedIndex = 1
                    End If
                End If
            End If



            ACT_Priority_CmbBx.Text = ExtractElementFromSegment("Priority", current_segment, "")
            ACT_CustID_Lbl.Text = ExtractElementFromSegment("CID", current_segment, "")


            If ACT_CustID_Lbl.Text <> "0" And ACT_CustID_Lbl.Text <> "" Then
                Customer_Segment = IO_GetSegmentSet(gShipriteDB, "SELECT Name, FullAddress FROM Contacts WHERE ID = " & ACT_CustID_Lbl.Text)
                ACT_Customer_Btn.Content = ExtractElementFromSegment("Name", Customer_Segment, "") & vbCrLf & ExtractElementFromSegment("FullAddress", Customer_Segment, "")
            Else
                ACT_Customer_Btn.Content = ""
            End If

            CompletedBy_CmbBx.Text = ExtractElementFromSegment("CompletedBy", current_segment, "")
            Notes_TxtBx.Text = ExtractElementFromSegment("Notes", current_segment, "")
            OpenedBy_TxtBx.Text = ExtractElementFromSegment("OpenedBy", current_segment, "")

            If ExtractElementFromSegment("Status", current_segment) = "Open" Then
                Open_RB.IsChecked = True
            Else
                Closed_RB.IsChecked = True
            End If

            Repeat_CmbBx.Text = ExtractElementFromSegment("Repeat", current_segment)
            If Repeat_CmbBx.Text = "" Then
                Repeat_CmbBx.SelectedIndex = 0

            ElseIf Repeat_CmbBx.Text = "Weekly" Then
                Repeat_WeekDay_CmbBx.Text = ExtractElementFromSegment("RepeatPeriod", current_segment)
            ElseIf Repeat_CmbBx.Text = "Monthly" Then
                Repeat_NoOfDays_TxtBx.Text = ExtractElementFromSegment("RepeatPeriod", current_segment)
            End If

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub ShowHide_Action_ListItems()
        Try
            If IsNothing(ActionNotice_List) Then Exit Sub
            Clear_Action_Details()

            Dim Priority As String = ""
            If Priority_CmbBx.SelectedIndex <> 0 Then
                Priority = (CType(Priority_CmbBx.SelectedItem, ComboBoxItem)).Content.ToString()
            End If

            Dim Status As String = ""
            If Status_CmbBx.SelectedIndex <> 0 Then
                Status = (CType(Status_CmbBx.SelectedItem, ComboBoxItem)).Content.ToString()
            End If


            ActionNotices_LB.UpdateLayout()
            Dim LBItem As ListBoxItem

            For Each item As ActionNotice In ActionNotice_List

                ActionNotices_LB.ScrollIntoView(item)
                LBItem = ActionNotices_LB.ItemContainerGenerator.ContainerFromItem(item)

                If Priority = "" And Status = "" Then
                    'PRIORITY AND STATUS  show All
                    If item.Status.ToUpper = "REPETITIVE" Then
                        LBItem.Visibility = Visibility.Collapsed
                    Else
                        LBItem.Visibility = Visibility.Visible
                    End If

                ElseIf Priority = "" And Status <> "" Then
                    'PRIORITY show All
                    If item.Status.ToUpper = Status Then
                        LBItem.Visibility = Visibility.Visible
                    Else
                        LBItem.Visibility = Visibility.Collapsed
                    End If


                ElseIf Priority <> "" And Status = "" Then
                    'STATUS Show ALL
                    If item.Priority.ToUpper = Priority Then
                        LBItem.Visibility = Visibility.Visible
                    Else
                        LBItem.Visibility = Visibility.Collapsed
                    End If


                ElseIf item.Priority.ToUpper = Priority And item.Status.ToUpper = Status Then
                    'Neither Show All
                    LBItem.Visibility = Visibility.Visible
                Else
                    LBItem.Visibility = Visibility.Collapsed
                End If


                'Hide future to do items
                If item.DueDate > Today And Status = "UPCOMING" Then
                    LBItem.Visibility = Visibility.Visible
                ElseIf Status <> "UPCOMING" And item.DueDate > Today Then
                    LBItem.Visibility = Visibility.Collapsed
                End If

            Next

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Customer_Btn_Click(sender As Object, e As RoutedEventArgs) Handles ACT_Customer_Btn.Click
        Try
            gAutoExitFromContacts = True

            If ACT_CustID_Lbl.Text <> "0" And ACT_CustID_Lbl.Text <> "" Then
                gCustomerSegment = IO_GetSegmentSet(gShipriteDB, "SELECT * FROM Contacts WHERE ID = " & ACT_CustID_Lbl.Text)
            End If


            Dim win As New ContactManager(Me)
            win.ShowDialog(Me)


            If gContactManagerSegment <> "" Then
                Dim CustomerSegment As String = gContactManagerSegment
                ACT_Customer_Btn.Content = ExtractElementFromSegment("Name", CustomerSegment, "") & vbCrLf & ExtractElementFromSegment("FullAddress", CustomerSegment, "")
                ACT_CustID_Lbl.Text = ExtractElementFromSegment("ID", CustomerSegment, "")
            End If


        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Public Shared Sub Check_Tickler_Notices()
        Try
            'checks repetitive notices on program startup

            Dim Buffer As String
            Dim current_segment As String
            Dim LastCreatedDate As Date

            Buffer = IO_GetSegmentSet(gShipriteDB, "Select ID, Repeat, RepeatPeriod, Repeat_LastCreated FROM Tickler WHERE Status='Repetitive'")

            Do Until Buffer = ""
                current_segment = GetNextSegmentFromSet(Buffer)

                If ExtractElementFromSegment("Repeat_LastCreated", current_segment, "") = "" Then
                    LastCreatedDate = DateTime.Today.AddDays(-1)
                Else
                    LastCreatedDate = ExtractElementFromSegment("Repeat_LastCreated", current_segment)
                End If


                If LastCreatedDate.Date < Today.Date Then

                    If ExtractElementFromSegment("Repeat", current_segment) = "Daily" Then
                        Create_ActionNotice_FromRepetitive(ExtractElementFromSegment("ID", current_segment))


                    ElseIf ExtractElementFromSegment("Repeat", current_segment) = "Weekly" Then
                        If ExtractElementFromSegment("RepeatPeriod", current_segment) = Today.DayOfWeek.ToString Then
                            Create_ActionNotice_FromRepetitive(ExtractElementFromSegment("ID", current_segment))
                        End If


                    ElseIf ExtractElementFromSegment("Repeat", current_segment) = "Monthly" Then
                        Dim RepeatDay As Integer = ExtractElementFromSegment("RepeatPeriod", current_segment)
                        Dim DaysInMonth As Integer = DateTime.DaysInMonth(Today.Year, Today.Month)

                        'if set day does not exist in month, set day to be last day of month.
                        If RepeatDay > DaysInMonth Then
                            RepeatDay = DaysInMonth
                        End If

                        If RepeatDay = Today.Day Then
                            Create_ActionNotice_FromRepetitive(ExtractElementFromSegment("ID", current_segment))
                        End If

                    End If

                End If
            Loop

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Public Shared Sub Create_ActionNotice_FromRepetitive(ByVal RepetitiveID As String)
        Try
            Dim Rep_Segment As String
            Dim Segment As String = ""
            Dim SQL As String

            Rep_Segment = IO_GetSegmentSet(gShipriteDB, "Select For, CID, Customer, Details, Priority FROM Tickler WHERE ID=" & RepetitiveID)


            Segment = AddElementToSegment(Segment, "For", ExtractElementFromSegment("For", Rep_Segment, ""))
            Segment = AddElementToSegment(Segment, "CID", ExtractElementFromSegment("CID", Rep_Segment, ""))
            Segment = AddElementToSegment(Segment, "Customer", ExtractElementFromSegment("Customer", Rep_Segment, ""))
            Segment = AddElementToSegment(Segment, "DueDate", Today.Date)
            Segment = AddElementToSegment(Segment, "EnteredBy", "Repetitive Item")
            Segment = AddElementToSegment(Segment, "Category", "Action Item")
            Segment = AddElementToSegment(Segment, "Status", "Open")
            Segment = AddElementToSegment(Segment, "DateEntered", Today.ToShortDateString)
            Segment = AddElementToSegment(Segment, "TimeEntered", Now.ToLongTimeString)
            Segment = AddElementToSegment(Segment, "Details", ExtractElementFromSegment("Details", Rep_Segment, ""))
            Segment = AddElementToSegment(Segment, "Priority", ExtractElementFromSegment("Priority", Rep_Segment, ""))
            Segment = AddElementToSegment(Segment, "Mailbox", "0")
            Segment = AddElementToSegment(Segment, "StringDate", Today.ToShortDateString)
            Segment = AddElementToSegment(Segment, "Status", "Open")


            SQL = MakeInsertSQLFromSchema("Tickler", Segment, gTicklerSchema, True)
            IO_UpdateSQLProcessor(gShipriteDB, SQL)

            SQL = "Update Tickler SET Repeat_LastCreated=#" & Today.Date & "# WHERE ID=" & RepetitiveID
            IO_UpdateSQLProcessor(gShipriteDB, SQL)


        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

#End Region

#Region "Mailbox Notices"

    Private Sub Load_Mailbox_Notices()
        Dim SQL As String = "'"
        Try

            If Not IsNothing(CancellationNotice_List) Then
                CancellationNotice_List.Clear()
                CancellationNotice_List = Load_Mailbox_List("SELECT MailboxNumber, Name, CID, EndDate, Business, CustomRates FROM Mailbox WHERE #" & Today.Date & "# >= [EndDate] + " & GetPolicyData(gShipriteDB, "MBXDaysCancel") & " AND [CanceledSent] = False AND [Rented] = True ORDER BY Mailbox.MailboxNumber")
                Cancellation_LV.ItemsSource = CancellationNotice_List
                Cancellation_LV.Items.Refresh()


                'if mailbox qualifies for a cancellation notice, then set the expiration and renewal notice flag to true, so that those do not get generated as well.
                If CancellationNotice_List.Count > 0 Then
                    SQL = "Update Mailbox Set [ExpiredSent]=True, [RenewalSent]=True WHERE MailboxNumber IN("

                    For Each item As Mailbox In CancellationNotice_List
                        SQL = SQL & item.Number & ","
                    Next
                    SQL = SQL.TrimEnd(CChar(","))
                    SQL = SQL & ")
"
                    IO_UpdateSQLProcessor(gShipriteDB, SQL)
                End If

            End If
            '----------------------------------------------------------


            If Not IsNothing(ExpirationNotice_List) Then
                ExpirationNotice_List.Clear()
                ExpirationNotice_List = Load_Mailbox_List("SELECT MailboxNumber, Name, CID, EndDate, Business, CustomRates FROM Mailbox WHERE #" & Today.Date & "# >= [EndDate] + " & GetPolicyData(gShipriteDB, "MBXDaysExpire") & " AND [ExpiredSent] = False AND [Rented] = True ORDER BY Mailbox.MailboxNumber")
                Expiration_LV.ItemsSource = ExpirationNotice_List
                Expiration_LV.Items.Refresh()

                If ExpirationNotice_List.Count > 0 Then
                    SQL = "Update Mailbox Set [RenewalSent]=True WHERE MailboxNumber IN("

                    For Each item As Mailbox In ExpirationNotice_List
                        SQL = SQL & item.Number & ","
                    Next
                    SQL = SQL.TrimEnd(CChar(","))
                    SQL = SQL & ")
"
                    IO_UpdateSQLProcessor(gShipriteDB, SQL)
                End If
            End If
            '-------------------------------------------------------------

            If Not IsNothing(RenewalNotice_List) Then
                RenewalNotice_List.Clear()
                RenewalNotice_List = Load_Mailbox_List("SELECT MailboxNumber, Name, CID, EndDate, Business, CustomRates FROM Mailbox WHERE #" & Today.Date & "# >= [EndDate] + " & GetPolicyData(gShipriteDB, "MBXDaysRenewal") & " AND [RenewalSent] = False AND [Rented] = True ORDER BY Mailbox.MailboxNumber")
                Renewal_LV.ItemsSource = RenewalNotice_List
                Renewal_LV.Items.Refresh()
            End If


        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Function Load_Mailbox_List(ByRef SQL As String) As List(Of Mailbox)
        Try
            Dim Buffer As String
            Dim current_segment As String
            Dim Notice As Mailbox
            Dim NoticeList As List(Of Mailbox) = New List(Of Mailbox)

            Buffer = IO_GetSegmentSet(gShipriteDB, SQL)
            Debug.Print(SQL)

            Do Until Buffer = ""
                current_segment = GetNextSegmentFromSet(Buffer)

                Notice = New Mailbox(current_segment)

                'Notice.MBX_Name = ExtractElementFromSegment("Name", current_segment, "")
                'Notice.MBX_No = ExtractElementFromSegment("MailboxNumber", current_segment, "")
                'Notice.Exp_Date = ExtractElementFromSegment("EndDate", current_segment)

                NoticeList.Add(Notice)
            Loop

            Return NoticeList

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Function

    Public Shared Function CheckMailBoxNoticesOnStartup() As Integer
        Try
            Dim count As Integer = 0

            If IO_GetSegmentSet(gShipriteDB, "SELECT MailboxNumber, Name FROM Mailbox WHERE #" & Today.Date & "# >= [EndDate] + " & GetPolicyData(gShipriteDB, "MBXDaysRenewal") & " AND [RenewalSent] = False AND [Rented] = True ORDER BY Mailbox.MailboxNumber") <> "" Then
                count = count + 1
            End If

            If IO_GetSegmentSet(gShipriteDB, "SELECT MailboxNumber, Name FROM Mailbox WHERE #" & Today.Date & "# >= [EndDate] + " & GetPolicyData(gShipriteDB, "MBXDaysExpire") & " AND [ExpiredSent] = False AND [Rented] = True ORDER BY Mailbox.MailboxNumber") <> "" Then
                count = count + 1
            End If

            If IO_GetSegmentSet(gShipriteDB, "SELECT MailboxNumber, Name FROM Mailbox WHERE #" & Today.Date & "# >= [EndDate] + " & GetPolicyData(gShipriteDB, "MBXDaysCancel") & " AND [CanceledSent] = False AND [Rented] = True ORDER BY Mailbox.MailboxNumber") <> "" Then
                count = count + 1
            End If

            Return count

        Catch ex As Exception
            MessageBox.Show(Err.Description)
            Return False
        End Try
    End Function

    Private Sub Renewal_LV_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles Renewal_LV.SelectionChanged, Expiration_LV.SelectionChanged, Cancellation_LV.SelectionChanged
        Dim LV As ListView = DirectCast(sender, ListView)
        If LV.SelectedIndex = -1 Then Exit Sub

        ActionNotices_LB.UnselectAll()
        InventoryNotices_LB.UnselectAll()

        DetailHeader_TxtBlck.Text = "DETAILS - MAILBOX NOTICE"

        MailboxDetails_Tab.IsSelected = True
        Dim item As Mailbox = LV.SelectedItem

        MBX_No_Lbl.Content = item.Number
        MBX_Name_Lbl.Content = item.Name
        MBX_Exp_Lbl.Content = item.ExpirationDate

        OpenClose_Border.Visibility = Visibility.Hidden
        Add_Button.Visibility = Visibility.Hidden
        SaveButton.Visibility = Visibility.Hidden
        Remove_Button.Visibility = Visibility.Hidden

    End Sub

    Private Sub Open_MailboxManager_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Open_MailboxManager_Btn.Click

        Dim win As New MailboxManager(Me,, MBX_No_Lbl.Content)
        win.ShowDialog(Me)
    End Sub

    Private Sub PrintNotices_Btn_Click(sender As Object, e As RoutedEventArgs) Handles PrintNotices_Btn.Click
        Print_MBX_Notices()
        Set_MBXNotices_SENT()
    End Sub

    Private Sub EmailNotices_Btn_Click(sender As Object, e As RoutedEventArgs) Handles EmailNotices_Btn.Click
        EMail_MBX_Notices()

        '..Email code will set successfully emailed mailboxes to SENT. 
        '..Boxes with failed emails or without email addresses should not be cleared.
        'Set_MBXNotices_SENT()
    End Sub

    Private Sub PrintEmailNotices_Btn_Click(sender As Object, e As RoutedEventArgs) Handles PrintEmailNotices_Btn.Click
        Print_MBX_Notices()
        EMail_MBX_Notices()
        Set_MBXNotices_SENT()
    End Sub

    Private Sub Print_MBX_Notices()

        'RENEWALS
        Dim renewalsList As List(Of Mailbox) = RenewalNotice_List.Where(Function(x) Not x.IsCustomRate).ToList()
        If renewalsList.Count > 0 Then
            MailboxNotice.Print_Notices(renewalsList, MailboxNoticeType.Renewal)
        End If

        'EXPIRATIONS
        Dim expirationsList As List(Of Mailbox) = ExpirationNotice_List.Where(Function(x) Not x.IsCustomRate).ToList()
        If expirationsList.Count > 0 Then
            MailboxNotice.Print_Notices(expirationsList, MailboxNoticeType.Expiration)
        End If

        'CANCELLATIONS
        Dim cancellationsList As List(Of Mailbox) = CancellationNotice_List.Where(Function(x) Not x.IsCustomRate).ToList()
        If cancellationsList.Count > 0 Then
            MailboxNotice.Print_Notices(cancellationsList, MailboxNoticeType.Cancellation)
        End If


        'CUSTOM RATE MAILBOXES - need to print individually

        'CUSTOM RATE - RENEWALS
        Dim crRenewalsList As List(Of Mailbox) = RenewalNotice_List.Where(Function(x) x.IsCustomRate).ToList()
        For Each item As Mailbox In crRenewalsList
            MailboxNotice.Print_Notice(item, MailboxNoticeType.Renewal)
        Next

        'CUSTOM RATE - EXPIRATIONS
        Dim crExpirationsList As List(Of Mailbox) = ExpirationNotice_List.Where(Function(x) x.IsCustomRate).ToList()
        For Each item As Mailbox In crExpirationsList
            MailboxNotice.Print_Notice(item, MailboxNoticeType.Expiration)
        Next

        'CUSTOM RATE - CANCELLATIONS
        Dim crCancellationsList As List(Of Mailbox) = CancellationNotice_List.Where(Function(x) x.IsCustomRate).ToList()
        For Each item As Mailbox In crCancellationsList
            MailboxNotice.Print_Notice(item, MailboxNoticeType.Cancellation)
        Next

    End Sub

    Private Sub EMail_MBX_Notices()

        'RENEWAL
        Dim renewalList As List(Of Mailbox) = RenewalNotice_List.Where(Function(x) Not x.IsCustomRate).ToList()
        If renewalList.Count > 0 Then
            MailboxNotice.Email_Notices(renewalList, MailboxNoticeType.Renewal)
        End If

        'EXPIRATION
        Dim expirationList As List(Of Mailbox) = ExpirationNotice_List.Where(Function(x) Not x.IsCustomRate).ToList()
        If expirationList.Count > 0 Then
            MailboxNotice.Email_Notices(expirationList, MailboxNoticeType.Expiration)
        End If

        'CANCELLATIONS
        Dim cancellationsList As List(Of Mailbox) = CancellationNotice_List.Where(Function(x) Not x.IsCustomRate).ToList()
        If cancellationsList.Count > 0 Then
            MailboxNotice.Email_Notices(cancellationsList, MailboxNoticeType.Cancellation)
        End If

        ' CUSTOM RATE MAILBOXES

        'CUSTOM RATE - RENEWALS
        Dim crRenewalsList As List(Of Mailbox) = RenewalNotice_List.Where(Function(x) x.IsCustomRate).ToList()
        For Each item As Mailbox In crRenewalsList
            MailboxNotice.Email_Notice(item, MailboxNoticeType.Renewal)
        Next

        'CUSTOM RATE - EXPIRATIONS
        Dim crExpirationsList As List(Of Mailbox) = ExpirationNotice_List.Where(Function(x) x.IsCustomRate).ToList()
        For Each item As Mailbox In crExpirationsList
            MailboxNotice.Email_Notice(item, MailboxNoticeType.Expiration)
        Next

        'CUSTOM RATE - CANCELLATIONS
        Dim crCancellationsList As List(Of Mailbox) = CancellationNotice_List.Where(Function(x) x.IsCustomRate).ToList()
        For Each item As Mailbox In crCancellationsList
            MailboxNotice.Email_Notice(item, MailboxNoticeType.Cancellation)
        Next


        Load_Mailbox_Notices()
    End Sub

    Private Sub Set_MBXNotices_SENT()
        'marks all notices as Sent
        Dim SQL As String = ""

        If CancellationNotice_List.Count > 0 Then
            SQL = "Update Mailbox Set [CanceledSent]=True WHERE MailboxNumber IN("

            For Each item As Mailbox In CancellationNotice_List
                SQL = SQL & item.Number & ","
            Next
            SQL = SQL.TrimEnd(CChar(","))
            SQL = SQL & ")
"
            IO_UpdateSQLProcessor(gShipriteDB, SQL)

            CancellationNotice_List.Clear()
            Cancellation_LV.Items.Refresh()
        End If


        If ExpirationNotice_List.Count > 0 Then
            SQL = "Update Mailbox Set [ExpiredSent]=True WHERE MailboxNumber IN("

            For Each item As Mailbox In ExpirationNotice_List
                SQL = SQL & item.Number & ","
            Next
            SQL = SQL.TrimEnd(CChar(","))
            SQL = SQL & ")
"
            IO_UpdateSQLProcessor(gShipriteDB, SQL)

            ExpirationNotice_List.Clear()
            Expiration_LV.Items.Refresh()
        End If


        If RenewalNotice_List.Count > 0 Then
            SQL = "Update Mailbox Set [RenewalSent]=True WHERE MailboxNumber IN("

            For Each item As Mailbox In RenewalNotice_List
                SQL = SQL & item.Number & ","
            Next
            SQL = SQL.TrimEnd(CChar(","))
            SQL = SQL & ")
"
            IO_UpdateSQLProcessor(gShipriteDB, SQL)

            RenewalNotice_List.Clear()
            Renewal_LV.Items.Refresh()
        End If

    End Sub



#End Region
End Class
