Imports System.Reflection

Public Class POS_ChangeDue

    Public Sub New()

        MyBase.New()

        ' This call is required by the designer.

        ' Me.Height = My.Computer.Screen.Bounds.Size.Height * 0.75
        ' Me.Width = Me.Height * 0.5273
        Me.WindowStartupLocation = WindowStartupLocation.CenterScreen
        Me.Topmost = True

        InitializeComponent()
    End Sub

    Public Sub New(ByVal callingWindow As Window)
        Try
            '     Me.Height = callingWindow.Height * 0.75
            '     Me.Width = Me.Height * 0.5273
            Me.Owner = callingWindow
            Me.WindowStartupLocation = WindowStartupLocation.CenterScreen

            ' This call is required by the designer.
            InitializeComponent()

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub POS_ChangeDue_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        AmountDue_Lbl.Content = gChangeDue.ToString("$ 0.00")

        If gIsCustomerDisplayEnabled Then
            gCustomerDisplay.DisplayChangeDue()
        End If

        Continue_Btn.Focus()

    End Sub

    Private Sub Continue_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Continue_Btn.Click
        Me.Close()
    End Sub
End Class
