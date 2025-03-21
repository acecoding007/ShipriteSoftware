Imports System.IO
Imports System.Windows.Forms


Public Class UtilitiesManager
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

    Private Sub ContactsButton_Click(sender As Object, e As RoutedEventArgs) Handles ContactsButton.Click
        Try

            Dim win As New ContactManager(Me)
            win.ShowDialog(Me)

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub

    Private Sub DropOffManagerButton_Click(sender As Object, e As RoutedEventArgs) Handles DropOffManagerButton.Click
        Try

            Call _DropOff.Open_DropOffManager(Me, gCurrentUser, Nothing)

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub

    Private Sub InventoryButton_Click(sender As Object, e As RoutedEventArgs) Handles InventoryButton.Click
        Try

            Dim win As New InventoryManager(Me)
            win.ShowDialog(Me)

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub

    Private Sub MailboxManagerButton_Click(sender As Object, e As RoutedEventArgs) Handles MailboxManagerButton.Click
        Try

            Dim win As New MailboxManager(Me)
            win.ShowDialog(Me)

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub

    Private Sub PackageValetButton_Click(sender As Object, e As RoutedEventArgs) Handles PackageValetButton.Click
        Try

            Dim win As New PackageValet(Me)
            win.ShowDialog(Me)

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub

    Private Sub PackMasterButton_Click(sender As Object, e As RoutedEventArgs) Handles PackMasterButton.Click
        Try

            Dim win As New Packmaster(Me)
            win.ShowDialog(Me)

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub

    Private Sub ProgramInfoButton_Click(sender As Object, e As RoutedEventArgs) Handles ProgramInfoButton.Click
        Try

            Dim win As New ProgramInfo(Me)
            win.ShowDialog(Me)

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub

    Private Sub ReportsButton_Click(sender As Object, e As RoutedEventArgs) Handles ReportsButton.Click
        Try

            Dim win As New ReportsManager(Me)
            win.ShowDialog(Me)

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub

    Private Sub ShipmentHistoryButton_Click(sender As Object, e As RoutedEventArgs) Handles ShipmentHistoryButton.Click
        Try

            Dim win As New ShipmentHistory(Me)
            win.ShowDialog(Me)

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub

    Private Sub ThemesButton_Click(sender As Object, e As RoutedEventArgs) Handles ThemesButton.Click
        Try

            Dim win As New Themes(Me)
            win.ShowDialog(Me)

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub

    Private Sub TimeClockButton_Click(sender As Object, e As RoutedEventArgs) Handles TimeClockButton.Click
        Try

            Dim win As New TimeClock(Me)
            win.ShowDialog(Me)

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub

    Private Sub AccountsButton_Click(sender As Object, e As RoutedEventArgs) Handles AccountsButton.Click
        Try

            Dim win As New AccountManager(Me)
            win.ShowDialog(Me)

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub

    Private Sub MailMasterButton_Click(sender As Object, e As RoutedEventArgs) Handles MailMasterButton.Click
        Try

            Dim win As New MailMaster(Me)
            win.ShowDialog(Me)

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub

    Private Sub TicklerButton_Click(sender As Object, e As RoutedEventArgs) Handles TicklerButton.Click
        Try

            Dim win As New Tickler(Me)
            win.ShowDialog(Me)

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub

    Private Sub Manifest_EOD_Button_Click(sender As Object, e As RoutedEventArgs) Handles Manifest_EOD_Button.Click
        Try

            Dim win As New EOD_Manifest(Me)
            win.ShowDialog(Me)

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub

    Private Sub ShipsuranceButton_Click(sender As Object, e As RoutedEventArgs) Handles ShipsuranceButton.Click
        Try

            Dim win As New Shipsurance(Me)
            win.ShowDialog(Me)

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub LetterMasterButton_Click(sender As Object, e As RoutedEventArgs) Handles LetterMasterButton.Click
        Try

            Dim win As New LetterMaster(Me)
            win.ShowDialog(Me)

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub BackupButton_Click(sender As Object, e As RoutedEventArgs) Handles Backup_Button.Click
        Try

            Dim win As New Backup(Me)
            win.ShowDialog(Me)

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Support_Utilities_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Support_Utilities_Btn.Click
        Try

            Dim win As New SupportUtilities(Me)
            win.ShowDialog(Me)

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub QuickBooksButton_Click(sender As Object, e As RoutedEventArgs) Handles QuickBooksButton.Click
        Try

            If IsProcessRunning("ShipriteOnlineQB.exe") = False Then

                Dim p As New ProcessStartInfo
                p.FileName = gAppPath & "\ShipriteOnlineQB.exe"
                p.WorkingDirectory = System.IO.Path.GetDirectoryName(p.FileName)

                Dim appStoreClass As String = IIf(_IDs.IsIt_PostNetStore(), "PostNet", "Legacy")
                Dim appStoreCountry As String = IIf(_IDs.IsIt_CanadaShipper(), "CA", "US")

                ' AppName StoreClass StoreCountry FinancePath DataPath AppPath
                p.Arguments = String.Format("{0} {1} {2} {3} {4} {5}", "ShipriteNext", appStoreClass, appStoreCountry, gDBpath & "\Finance.mdb", gDBpath, gAppPath) ' "ShipriteNext Postnet " & gDBpath & "\Finance.mdb " & gDBpath & " " & gAppPath

                p.WindowStyle = ProcessWindowStyle.Normal
                Process.Start(p)

            End If

            'Dim win As New QuickBooks_Online(Me)
            'win.ShowDialog(Me)

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub StatisticsButton_Click(sender As Object, e As RoutedEventArgs) Handles StatisticsButton.Click
        Try

            Dim win As New Statistics(Me)
            win.ShowDialog(Me)

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub RateChartsButton_Click(sender As Object, e As RoutedEventArgs) Handles RateChartsButton.Click
        Try

            Dim win As New RateCharts(Me)
            win.ShowDialog(Me)

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub
End Class
