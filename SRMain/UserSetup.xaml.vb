Public Class UserSetup
    Inherits CommonWindow

    Public Current_UserList As List(Of User)
    Public CheckboxList As List(Of Object)

    Public POSList As List(Of Object)
    Public ShippingList As List(Of Object)
    Public SetupList As List(Of Object)
    Public ReportsList As List(Of Object)

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

    Private Sub UserSetup_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Try
            Current_UserList = New List(Of User)
            CheckboxList = New List(Of Object)
            POSList = New List(Of Object)
            ShippingList = New List(Of Object)
            SetupList = New List(Of Object)
            ReportsList = New List(Of Object)

            Get_ChildControls_Of_Grid(Permissions_Grid, CheckboxList)

            Get_ChildControls_Of_Grid(POS_Grid, POSList)
            Get_ChildControls_Of_Grid(Shipping_Grid, ShippingList)
            Get_ChildControls_Of_Grid(Setup_Grid, SetupList)
            Get_ChildControls_Of_Grid(Reports_Grid, ReportsList)

            CheckboxList.AddRange(POSList)
            CheckboxList.AddRange(ShippingList)
            CheckboxList.AddRange(SetupList)
            CheckboxList.AddRange(ReportsList)

            LoadUsersFromDB()
            UserSelection_ListBox.ItemsSource = Current_UserList
            SaveButton.Visibility = Visibility.Hidden
            RemoveUser_Button.Visibility = Visibility.Hidden
        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try

    End Sub

    Private Sub LoadUsersFromDB()
        Try
            Dim SegmentSet As String = ""
            Dim current_segment As String = ""
            Dim buf As String = ""
            Dim fieldName As String = ""
            Dim fieldValue As String = ""
            Dim current_user As User
            Dim current_permission As User_Permission

            buf = IO_GetSegmentSet(gShipriteDB, "SELECT * From Users")

            Do Until buf = ""
                current_segment = GetNextSegmentFromSet(buf)
                current_user = New User
                current_user.Permission_List = New List(Of User_Permission)


                Do Until current_segment = ""
                    current_segment = ExtractNextElementFromSegment(fieldName, fieldValue, current_segment)

                    Select Case fieldName
                        Case "ID"
                            current_user.DatabaseID = fieldValue
                        Case "DisplayName"
                            current_user.DisplayName = fieldValue
                        Case "Password"
                            current_user.PassCode = fieldValue
                        Case "FirstName"
                            current_user.FirstName = fieldValue
                        Case "LastName"
                            current_user.LastName = fieldValue
                        Case "Add1"
                            current_user.Address1 = fieldValue
                        Case "Add2"
                            current_user.Address2 = fieldValue
                        Case "City"
                            current_user.City = fieldValue
                        Case "State"
                            current_user.State = fieldValue
                        Case "Zip"
                            current_user.Zip = fieldValue
                        Case "Phone"
                            current_user.Phone = fieldValue
                        Case "Email"
                            current_user.Email = fieldValue
                        Case "FingerPrint"
                            current_user.FingerPrint = fieldValue

                        Case Else
                            current_permission = New User_Permission
                            current_permission.DB_Field = fieldName
                            current_permission.isAllowed = Convert.ToBoolean(fieldValue)
                            current_user.Permission_List.Add(current_permission)
                    End Select

                Loop
                Current_UserList.Add(current_user)

            Loop

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try


    End Sub



    Private Sub SaveUserPermissions(ByRef CurrentUser As User)
        Try
            Dim CurrentPermissionList As List(Of User_Permission) = New List(Of User_Permission)
            Dim CurrentPermission As User_Permission


            For Each Current_checkbox As CheckBox In CheckboxList
                CurrentPermission = New User_Permission
                CurrentPermission.DB_Field = Current_checkbox.Tag        'Tag on Checkbox corresponds to Database field name
                CurrentPermission.isAllowed = Current_checkbox.IsChecked
                CurrentPermissionList.Add(CurrentPermission)
            Next

            CurrentUser.Permission_List = CurrentPermissionList
        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub



    Private Sub Clear_Fields()
        Try
            DisplayName_TxtBox.Text = ""
            PassCode_TxtBox.Password = ""
            FirstName_TxtBox.Text = ""
            LastName_TxtBox.Text = ""
            Add1_TxtBox.Text = ""
            Add2_TxtBox.Text = ""
            City_TxtBox.Text = ""
            State_TxtBox.Text = ""
            Zip_TxtBox.Text = ""
            Phone_TxtBox.Text = ""
            Email_TxtBox.Text = ""

            For Each Current_checkbox As CheckBox In CheckboxList
                Current_checkbox.IsChecked = False
            Next

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub



    Private Sub UserSelection_ListBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles UserSelection_ListBox.SelectionChanged
        Try
            Dim current_user As User = New User
            Dim index As Integer

            If UserSelection_ListBox.SelectedIndex = -1 Then
                Exit Sub
            End If

            SaveButton.Visibility = Visibility.Visible
            RemoveUser_Button.Visibility = Visibility.Visible

            current_user = Current_UserList.Item((UserSelection_ListBox.SelectedIndex))

            DisplayName_TxtBox.Text = current_user.DisplayName
            PassCode_TxtBox.Password = current_user.PassCode
            FirstName_TxtBox.Text = current_user.FirstName
            LastName_TxtBox.Text = current_user.LastName
            Add1_TxtBox.Text = current_user.Address1
            Add2_TxtBox.Text = current_user.Address2
            City_TxtBox.Text = current_user.City
            State_TxtBox.Text = current_user.State
            Zip_TxtBox.Text = current_user.Zip
            Phone_TxtBox.Text = current_user.Phone
            Email_TxtBox.Text = current_user.Email



            For Each Current_checkbox As CheckBox In CheckboxList
                index = current_user.Permission_List.FindIndex(Function(value As User_Permission) value.DB_Field = Current_checkbox.Tag)
                If index <> -1 Then
                    Current_checkbox.IsChecked = current_user.Permission_List(index).isAllowed
                End If
            Next

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub

#Region "Add, Remove, Save User"

    Private Sub AddUser_Button_Click(sender As Object, e As RoutedEventArgs) Handles AddUser_Button.Click
        Try
            Dim SQL As String
            Dim CurrentUser As User = New User

            If DisplayName_TxtBox.Text = "" Or PassCode_TxtBox.Password = "" Then
                MsgBox("User Not Saved. DisplayName and Passcode fields cannot be empty.", MsgBoxStyle.Critical)
                Exit Sub
            End If

            'check if user exists
            If IO_GetSegmentSet(gShipriteDB, "SELECT DisplayName From Users WHERE DisplayName='" & Trim(DisplayName_TxtBox.Text) & "'") <> "" Then
                MsgBox("User already exists!", MsgBoxStyle.Critical, "Cannot Add New User")
                Exit Sub

            End If


            CurrentUser.DisplayName = Trim(DisplayName_TxtBox.Text)
            CurrentUser.PassCode = Trim(PassCode_TxtBox.Password)
            CurrentUser.FirstName = Trim(FirstName_TxtBox.Text)
            CurrentUser.LastName = Trim(LastName_TxtBox.Text)
            CurrentUser.Address1 = Trim(Add1_TxtBox.Text)
            CurrentUser.Address2 = Trim(Add2_TxtBox.Text)
            CurrentUser.City = Trim(City_TxtBox.Text)
            CurrentUser.State = Trim(State_TxtBox.Text)
            CurrentUser.Zip = Trim(Zip_TxtBox.Text)
            CurrentUser.Phone = Trim(Phone_TxtBox.Text)
            CurrentUser.Email = Trim(Email_TxtBox.Text)
            CurrentUser.FingerPrint = ""

            'Permissions
            SaveUserPermissions(CurrentUser)


            'Add user to database
            SQL = "INSERT INTO Users ([DisplayName], [Password], [FirstName], [LastName], [Add1], [Add2], [City], [State], [Zip], [Phone], [Email], [Fingerprint]"


            'adds permission field names
            For Each current_checkbox In CheckboxList
                SQL = SQL & ", [" & current_checkbox.Tag & "]"
            Next

            SQL = SQL & ") VALUES ("

            SQL = SQL & "'" & CurrentUser.DisplayName & "',"
            SQL = SQL & "'" & CurrentUser.PassCode & "',"
            SQL = SQL & "'" & CurrentUser.FirstName & "',"
            SQL = SQL & "'" & CurrentUser.LastName & "',"
            SQL = SQL & "'" & CurrentUser.Address1 & "',"
            SQL = SQL & "'" & CurrentUser.Address2 & "',"
            SQL = SQL & "'" & CurrentUser.City & "',"
            SQL = SQL & "'" & CurrentUser.State & "',"
            SQL = SQL & "'" & CurrentUser.Zip & "',"
            SQL = SQL & "'" & CurrentUser.Phone & "',"
            SQL = SQL & "'" & CurrentUser.Email & "',"
            SQL = SQL & "'" & CurrentUser.FingerPrint & "'"

            'adds permission true/false values to SQL
            For Each Current_Permission In CurrentUser.Permission_List
                SQL = SQL & ", " & Current_Permission.isAllowed & ""
            Next

            SQL = SQL & ")"

            If IO_UpdateSQLProcessor(gShipriteDB, SQL) = 0 Then
                Exit Sub
            End If

            'Add user to List
            Current_UserList.Add(CurrentUser)

            MsgBox("New User " & CurrentUser.DisplayName & " added Successfully", vbInformation + vbOKOnly)
            UserSelection_ListBox.Items.Refresh()
            Clear_Fields()



            'reset/reload user list so that new user is included with databaseID.
            Current_UserList = Nothing
            Current_UserList = New List(Of User)
            LoadUsersFromDB()
            UserSelection_ListBox.ItemsSource = Current_UserList

        Catch ex As Exception

            MessageBox.Show(Err.Description)
            Exit Sub

        End Try
    End Sub

    Private Sub SaveButton_Click(sender As Object, e As RoutedEventArgs) Handles SaveButton.Click
        Try
            Dim SQL As String
            Dim currentuser As User = New User

            If UserSelection_ListBox.SelectedIndex = -1 Then
                MsgBox("Cannot Save Changes! Please select a user first.", MsgBoxStyle.Critical + vbOKOnly)
                Exit Sub
            End If

            If DisplayName_TxtBox.Text = "" Or PassCode_TxtBox.Password = "" Then
                MsgBox("User Not Saved. DisplayName and Passcode fields cannot be empty.", MsgBoxStyle.Critical + vbOKOnly)
                Exit Sub
            End If

            currentuser = Current_UserList.Item((UserSelection_ListBox.SelectedIndex))

            '----------Save Changes to Database--------------

            currentuser.DisplayName = Trim(DisplayName_TxtBox.Text)
            currentuser.PassCode = Trim(PassCode_TxtBox.Password)
            currentuser.FirstName = Trim(FirstName_TxtBox.Text)
            currentuser.LastName = Trim(LastName_TxtBox.Text)
            currentuser.Address1 = Trim(Add1_TxtBox.Text)
            currentuser.Address2 = Trim(Add2_TxtBox.Text)
            currentuser.City = Trim(City_TxtBox.Text)
            currentuser.State = Trim(State_TxtBox.Text)
            currentuser.Zip = Trim(Zip_TxtBox.Text)
            currentuser.Phone = Trim(Phone_TxtBox.Text)
            currentuser.Email = Trim(Email_TxtBox.Text)

            SaveUserPermissions(currentuser)


            SQL = "UPDATE USERS SET "

            SQL = SQL & "[DisplayName]=" & "'" & currentuser.DisplayName & "',"
            SQL = SQL & "[Password]=" & "'" & currentuser.PassCode & "',"
            SQL = SQL & "[FirstName]=" & "'" & currentuser.FirstName & "',"
            SQL = SQL & "[LastName]=" & "'" & currentuser.LastName & "',"
            SQL = SQL & "[Add1]=" & "'" & currentuser.Address1 & "',"
            SQL = SQL & "[Add2]=" & "'" & currentuser.Address2 & "',"
            SQL = SQL & "[City]=" & "'" & currentuser.City & "',"
            SQL = SQL & "[State]=" & "'" & currentuser.State & "',"
            SQL = SQL & "[Zip]=" & "'" & currentuser.Zip & "',"
            SQL = SQL & "[Phone]=" & "'" & currentuser.Phone & "',"
            SQL = SQL & "[Email]=" & "'" & currentuser.Email & "',"
            SQL = SQL & "[FingerPrint]=" & "'" & currentuser.FingerPrint & "'"



            'adds permission true/false values to SQL
            For Each Current_Permission In CurrentUser.Permission_List
                SQL = SQL & ", [" & Current_Permission.DB_Field & "]=" & Current_Permission.isAllowed
            Next

            SQL = SQL & " WHERE [ID]=" & currentuser.DatabaseID


            If IO_UpdateSQLProcessor(gShipriteDB, SQL) = 0 Then
                Exit Sub
            End If



            MsgBox("Changes to User " & currentuser.DisplayName & " saved Successfully!", vbInformation + vbOKOnly)
            UserSelection_ListBox.Items.Refresh()
            Clear_Fields()
            UserSelection_ListBox.SelectedIndex = -1

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try


    End Sub


    Private Sub RemoveUser_Button_Click(sender As Object, e As RoutedEventArgs) Handles RemoveUser_Button.Click
        Try
            Dim sql As String

            If UserSelection_ListBox.SelectedIndex = -1 Then
                MsgBox("Cannot Delete User. No User Selected.", MsgBoxStyle.Critical + vbOKOnly)
                Exit Sub
            End If

            '-------------remove user from database here----------
            sql = "DELETE * From Users WHERE ID=" & Current_UserList(UserSelection_ListBox.SelectedIndex).DatabaseID
            If IO_UpdateSQLProcessor(gShipriteDB, sql) = 0 Then
                Exit Sub
            End If

            MsgBox("User " & Current_UserList(UserSelection_ListBox.SelectedIndex).DisplayName & " Deleted Successfully!", vbInformation + vbOKOnly)

            Current_UserList.RemoveAt(UserSelection_ListBox.SelectedIndex)
            UserSelection_ListBox.Items.Refresh()
            Clear_Fields()



        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub

#End Region

#Region "Checkboxes"

    Private Sub Child_CheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles UserSetup_CheckBox.Checked, AR_CheckBox.Checked, CreateAR_CheckBox.Checked, Inventory_CheckBox.Checked, POSManager_CheckBox.Checked,
        POSButtons_CheckBox.Checked, SaleDiscounts_CheckBox.Checked, VoidInvoice_CheckBox.Checked, RefundInvoice_CheckBox.Checked,
        DeletePOSLine_CheckBox.Checked, ViewShippingCosts_CheckBox.Checked, Manifest_CheckBox.Checked, VoidShipment_CheckBox.Checked,
        CarrierSetup_CheckBox.Checked, IncomeReports_CheckBox.Checked, MailboxSetup_CheckBox.Checked

        Dim ParentGrid As Grid = sender.Parent

        Select Case ParentGrid.Name
            Case "POS_Grid"
                POS_CheckBox.IsChecked = True

            Case "Shipping_Grid"
                Shipping_CheckBox.IsChecked = True

            Case "Setup_Grid"
                Setup_CheckBox.IsChecked = True

            Case "Reports_Grid"
                Reports_CheckBox.IsChecked = True

        End Select


    End Sub

    Private Sub PassCode_TxtBox_PreviewTextInput(sender As Object, e As TextCompositionEventArgs) Handles PassCode_TxtBox.PreviewTextInput
        Try
            Dim allowedchars As String = "0123456789"
            If allowedchars.IndexOf(CChar(e.Text)) = -1 Then e.Handled = True

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub



    Private Sub POS_CheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles POS_CheckBox.Unchecked
        Try
            For Each box As CheckBox In POSList
                box.IsChecked = False
            Next

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub POS_CheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles POS_CheckBox.Checked
        Try
            For Each box As CheckBox In POSList
                box.IsChecked = True
            Next

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub


    Private Sub Shipping_CheckBox_UnChecked(sender As Object, e As RoutedEventArgs) Handles Shipping_CheckBox.Unchecked
        Try
            For Each box As CheckBox In ShippingList
                box.IsChecked = False
            Next

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Shipping_CheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles Shipping_CheckBox.Checked
        Try
            For Each box As CheckBox In ShippingList
                box.IsChecked = True
            Next

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub


    Private Sub Setup_CheckBox_unChecked(sender As Object, e As RoutedEventArgs) Handles Setup_CheckBox.Unchecked
        Try
            For Each box As CheckBox In SetupList
                box.IsChecked = False
            Next

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Setup_CheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles Setup_CheckBox.Checked
        Try
            For Each box As CheckBox In SetupList
                box.IsChecked = True
            Next

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub


    Private Sub Reports_CheckBox_unChecked(sender As Object, e As RoutedEventArgs) Handles Reports_CheckBox.Unchecked
        Try
            IncomeReports_CheckBox.IsChecked = False

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Reports_CheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles Reports_CheckBox.Checked
        Try
            IncomeReports_CheckBox.IsChecked = True

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub



#End Region
End Class
