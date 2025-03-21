Public Class QuickBooks_Online
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

    Private Sub SetupButton_Click(sender As Object, e As RoutedEventArgs) Handles SetupButton.Click

        Dim win As New POSSetup(Me, 7)
        win.ShowDialog(Me)
    End Sub
End Class
