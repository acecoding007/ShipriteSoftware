Imports System.Reflection

Public Class UserLogIn

    Public Shared isAllowed As Boolean
    Dim Permission As String

    Public Sub New()

        MyBase.New()

        ' This call is required by the designer.

        Me.Height = My.Computer.Screen.Bounds.Size.Height * 0.75
        Me.Width = Me.Height * 0.5273
        Me.WindowStartupLocation = WindowStartupLocation.CenterScreen
        'Me.Topmost = True

        InitializeComponent()

    End Sub

    Public Sub New(ByVal callingWindow As Window, DB_PermissionField As String)
        Try


            Me.Height = callingWindow.Height * 0.75
            Me.Width = Me.Height * 0.5273
            Me.Owner = callingWindow
            Me.WindowStartupLocation = WindowStartupLocation.CenterScreen


            ' This call is required by the designer.
            InitializeComponent()

            Permission = DB_PermissionField

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub SetScreenSize()

    End Sub

    Private Sub UserLogIn_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Try

            If Not AreUsersSetup() Then
                'No users exist
                MsgBox("No Users Setup! Please go to Setup > User Setup!", vbOKOnly + vbInformation, gProgramName)
                isAllowed = True
                Me.Close()
                Exit Sub
            End If

            If Not CheckSetupPermission() Then
                'No user exists that has amdin rights
                MsgBox("User Permissions are not Setup. Please go to Setup > User Setup and enter in permissions for all Users!", vbOKOnly + vbInformation, gProgramName)
                gIsSetupSecurityEnabled = False
                isAllowed = False
                Me.Close()
                Exit Sub
            End If

            'UserLogIn_Window.Topmost = True
            PassCode_TxtBox.Focus()
            SetSelection(PassCode_TxtBox, PassCode_TxtBox.Password.Length, 0)

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

#Region "KeypadEntry"
    Private Sub Keypad_Cancel_Click(sender As Object, e As RoutedEventArgs) Handles Keypad_Cancel.Click
        Try
            isAllowed = False
            Me.Close()

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Keypad_ENTER_Click(sender As Object, e As RoutedEventArgs) Handles Keypad_ENTER.Click
        CheckPermission()
    End Sub

    Private Sub Keypad_7_Click(sender As Object, e As RoutedEventArgs) Handles Keypad_1.Click, Keypad_2.Click, Keypad_3.Click, Keypad_4.Click, Keypad_5.Click, Keypad_6.Click, Keypad_7.Click, Keypad_8.Click, Keypad_9.Click, Keypad_0.Click
        Try
            PassCode_TxtBox.Password = PassCode_TxtBox.Password & sender.content

            SetSelection(PassCode_TxtBox, PassCode_TxtBox.Password.Length, 0)
            PassCode_TxtBox.Focus()

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Keypad_BackSpace_Click(sender As Object, e As RoutedEventArgs) Handles Keypad_BackSpace.Click
        Try
            If PassCode_TxtBox.Password.Length <> 0 Then
                PassCode_TxtBox.Password = PassCode_TxtBox.Password.Substring(0, PassCode_TxtBox.Password.Length - 1)
            End If

            SetSelection(PassCode_TxtBox, PassCode_TxtBox.Password.Length, 0)
            PassCode_TxtBox.Focus()

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub PassCode_TxtBox_KeyDown(sender As Object, e As KeyEventArgs) Handles PassCode_TxtBox.KeyDown
        Try
            If e.Key = Key.[Return] Then
                CheckPermission()
            End If

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub SetSelection(ByVal passwordBox As PasswordBox, ByVal start As Integer, ByVal length As Integer)
        Try
            'Function to set focus and cursor at end of the password textbox
            passwordBox.[GetType]().GetMethod("Select", BindingFlags.Instance Or BindingFlags.NonPublic).Invoke(passwordBox, New Object() {start, length})

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub
#End Region


    Private Sub CheckPermission()
        Try
            Dim User_Segment As String

            User_Segment = GetNextSegmentFromSet(IO_GetSegmentSet(gShipriteDB, "SELECT * From Users Where [Password]='" & PassCode_TxtBox.Password & "'"))

            If User_Segment = "" Then
                isAllowed = False
                MsgBox("User not Found!", vbOKOnly + vbInformation, "Access Denied!")
                PassCode_TxtBox.Password = ""
                SetSelection(PassCode_TxtBox, PassCode_TxtBox.Password.Length, 0)
                PassCode_TxtBox.Focus()
                Exit Sub
            End If


            'No specific permission requested, check if user Exists. Used on Program Startup.
            If Permission = "" And User_Segment <> "" Then
                gCurrentUser = ExtractElementFromSegment("DisplayName", User_Segment)
                isAllowed = True
                Me.Close()
                Exit Sub
            End If


            'Check permissions to see if user is allwed access 
            If ExtractElementFromSegment(Permission, User_Segment) = True Then
                isAllowed = True
                gCurrentUser = ExtractElementFromSegment("DisplayName", User_Segment)
                Me.Close()
            Else
                isAllowed = False
                MsgBox("User " & ExtractElementFromSegment("DisplayName", User_Segment) & " does NOT have permission to access this feature!", vbOKOnly + vbInformation, "Access Denied!")
                PassCode_TxtBox.Password = ""
                SetSelection(PassCode_TxtBox, PassCode_TxtBox.Password.Length, 0)
                PassCode_TxtBox.Focus()
            End If

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Function AreUsersSetup() As Boolean
        Try
            If IO_GetSegmentSet(gShipriteDB, "SELECT DisplayName From Users") = "" Then
                Return False
            Else
                Return True
            End If

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
        Return False
    End Function

    Public Function CheckSetupPermission() As Boolean
        Try
            'check if at least one user has access to User Setup!
            If IO_GetSegmentSet(gShipriteDB, "SELECT DisplayName From Users WHERE Setup_Users=True and SETUP=True") = "" Then
                Return False
            Else
                Return True
            End If


        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
        Return False
    End Function
End Class
