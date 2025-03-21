Public Class SetupManager
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



    Private Sub EmailSetupButton_Click(sender As Object, e As RoutedEventArgs) Handles EmailSetupButton.Click
        Try

            Dim win As New EmailSetup(Me)
            win.ShowDialog(Me)

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub

    Private Sub MailboxSetupButton_Click(sender As Object, e As RoutedEventArgs) Handles MailboxSetupButton.Click
        Try

            If gIsSetupSecurityEnabled Then
                If Not Check_Current_User_Permission("Setup_Mailbox") Then
                    Exit Sub
                End If
            End If

            Dim win As New MailboxSetup(Me)
            win.ShowDialog(Me)

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub

    Private Sub PrinterSetupButton_Click(sender As Object, e As RoutedEventArgs) Handles PrinterSetupButton.Click
        Try

            Dim win As New PrinterSetup(Me)
            win.ShowDialog(Me)

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub

    Private Sub SecuritySetupButton_Click(sender As Object, e As RoutedEventArgs) Handles SecuritySetupButton.Click
        Try

            If gIsSetupSecurityEnabled Then
                If Not Check_Current_User_Permission("Setup_Users") Then
                    Exit Sub
                End If
            End If


            Dim win As New Setup_General(Me, 1)
            win.ShowDialog(Me)



        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub

    Private Sub UserSetupButton_Click(sender As Object, e As RoutedEventArgs) Handles UserSetupButton.Click
        Try

            If gIsSetupSecurityEnabled Then
                If Not Check_Current_User_Permission("Setup_Users") Then
                    Exit Sub
                End If
            End If


            Dim win As New UserSetup(Me)
            win.ShowDialog(Me)

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub

    Private Sub POSSetupButton_Click(sender As Object, e As RoutedEventArgs) Handles POSSetupButton.Click
        Try

            Dim win As New POSSetup(Me)
            win.ShowDialog(Me)

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub

    Private Sub ShippingMarkupsSetupButton_Click(sender As Object, e As RoutedEventArgs) Handles ShippingMarkupsSetupButton.Click
        Try

            If gIsSetupSecurityEnabled Then
                If Not Check_Current_User_Permission("Setup_Carriers") Then
                    Exit Sub
                End If
            End If


            Dim win As New ShippingMarkups(Me)
            win.ShowDialog(Me)

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub

    Private Sub ShipSetupButton_Click(sender As Object, e As RoutedEventArgs) Handles ShipOptionsButton.Click
        Try

            Dim win As New ShippingSetup(Me)
            win.ShowDialog(Me)

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub

    Private Sub ProgramRegistrationButton_Click(sender As Object, e As RoutedEventArgs) Handles ProgramRegistrationButton.Click
        Try

            Dim win As New Setup_General(Me, 0)
            win.ShowDialog(Me)

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub

    Private Sub PackMasterSetup_Click(sender As Object, e As RoutedEventArgs) Handles PackMasterSetup.Click
        Try
            Dim win As New ShippingSetup(Me, 4)
            win.ShowDialog(Me)

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub ZipCode_Editor_Button_Click(sender As Object, e As RoutedEventArgs) Handles ZipCode_Editor_Button.Click
        Try
            Dim win As New Setup_General(Me, 2)
            win.ShowDialog(Me)

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub CustomerDisplayButton_Click(sender As Object, e As RoutedEventArgs) Handles CustomerDisplayButton.Click
        Try
            Dim win As New Setup_General(Me, 3)
            win.ShowDialog(Me)

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub
End Class
