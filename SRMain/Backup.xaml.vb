Imports System.Windows.Forms
Imports System.IO.Compression


Public Class Backup
    Inherits CommonWindow
    Public AutoBackup As Boolean = False

    Public Sub New()

        MyBase.New()

        ' This call is required by the designer.
        InitializeComponent()

    End Sub

    Public Sub New(ByVal callingWindow As Window, Optional AutoBckp As Boolean = False)

        MyBase.New(callingWindow)

        ' This call is required by the designer.
        InitializeComponent()

        AutoBackup = AutoBckp

    End Sub

    Private Sub BrowsePath_Btn_Click(sender As Object, e As RoutedEventArgs) Handles BrowsePath1_Btn.Click, BrowsePath2_Btn.Click
        Try
            Dim filePath As String = ""
            Dim fbd As FolderBrowserDialog = New FolderBrowserDialog

            Show_OpenFolderBrowserDialog(fbd, filePath, "Select Folder to backup to!")

            If sender.tag = 1 Then
                Path1_TxtBx.Text = fbd.SelectedPath
            Else
                Path2_TxtBx.Text = fbd.SelectedPath
            End If

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub


    Private Sub Backup_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Try
            Path1_TxtBx.Text = GetPolicyData(gReportsDB, "BackupPath1", "")
            Path2_TxtBx.Text = GetPolicyData(gReportsDB, "BackupPath2", "")
            AutoBackup_ChkBx.IsChecked = GetPolicyData(gReportsDB, "BackupAutomatic", "false")

            If AutoBackup Then
                Run_Backup()
            Else
                BackingUp_Status_Border.Visibility = Visibility.Hidden
            End If

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub


    Private Sub Run_Backup_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Run_Backup_Btn.Click
        Try
            BackingUp_Status_Border.Visibility = Visibility.Visible
            Status_TxtBx.Text = ""

            InvalidateVisual()

            Run_Backup()

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Run_Backup()
        Try
            Status_TxtBx.Text = ""


            If AutoBackup Then
                Process_Backup(GetPolicyData(gReportsDB, "BackupPath1", ""))
                Process_Backup(GetPolicyData(gReportsDB, "BackupPath2", ""))

            Else
                Process_Backup(Path1_TxtBx.Text)
                Process_Backup(Path2_TxtBx.Text)
            End If

            MsgBox("Backup Complete!", vbInformation)

            If AutoBackup Then Me.Close()

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub


    Private Sub Process_Backup(Path As String)
        Try
            If Path <> "" Then 'if blank skip
                If _Files.IsFolderExist(Path, True) Then 'check if destination folder exists


                    If AutoBackup Then
                        'Create Backup for day of week
                        Path = Path & "\" & Today.DayOfWeek.ToString()

                        If _Files.Create_Folder(Path, True) Then 'if folder doesn't exist, create it
                            Status_TxtBx.Text = " - Running Daily AutoBackup"
                            Create_Backup_Zip(Path & "\SR_Backup.zip")
                        End If

                        UpdatePolicy(gReportsDB, "Backup_LastDate", "#" & DateTime.Now & "#")

                    Else
                        'Backup button pressed, Create One time backup
                        If _Files.Create_Folder(Path, True) Then 'if folder doesn't exist, create it
                            Create_Backup_Zip(Path & "\SR_Backup_" & Strings.Replace(Today.ToShortDateString, "/", "-") & ".zip")
                        End If
                    End If


                End If
            End If

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub


    Private Sub Create_Backup_Zip(FilePath As String)
        Try

            If IsFileExist(FilePath, False) Then Delete_File(FilePath, False)

            Using zip As ZipArchive = ZipFile.Open(FilePath, ZipArchiveMode.Create)

                UpdateStatus(gShipriteDB, FilePath)
                If IsFileExist(gShipriteDB, False) Then zip.CreateEntryFromFile(gShipriteDB, gShipriteDB.Remove(0, gShipriteDB.LastIndexOf("\") + 1))

                UpdateStatus(gDropOffDB, FilePath)
                If IsFileExist(gDropOffDB, False) Then zip.CreateEntryFromFile(gDropOffDB, gDropOffDB.Remove(0, gShipriteDB.LastIndexOf("\") + 1))

                UpdateStatus(gMailboxDB, FilePath)
                If IsFileExist(gMailboxDB, False) Then zip.CreateEntryFromFile(gMailboxDB, gMailboxDB.Remove(0, gShipriteDB.LastIndexOf("\") + 1))

                UpdateStatus(gPackagingDB, FilePath)
                If IsFileExist(gPackagingDB, False) Then zip.CreateEntryFromFile(gPackagingDB, gPackagingDB.Remove(0, gShipriteDB.LastIndexOf("\") + 1))

                UpdateStatus(gPricingMatrixDB, FilePath)
                If IsFileExist(gPricingMatrixDB, False) Then zip.CreateEntryFromFile(gPricingMatrixDB, gPricingMatrixDB.Remove(0, gShipriteDB.LastIndexOf("\") + 1))

            End Using

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub UpdateStatus(FilePath As String, ZipPath As String)
        Try
            Status_TxtBx.Text = Status_TxtBx.Text & "Compressing " & FilePath.Remove(0, gShipriteDB.LastIndexOf("\") + 1) & " to " & vbCrLf & ZipPath & vbCrLf & vbCrLf

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub SaveButton_Click(sender As Object, e As RoutedEventArgs) Handles SaveButton.Click
        Try
            UpdatePolicy(gReportsDB, "BackupPath1", Path1_TxtBx.Text)
            UpdatePolicy(gReportsDB, "BackupPath2", Path2_TxtBx.Text)
            UpdatePolicy(gReportsDB, "BackupAutomatic", AutoBackup_ChkBx.IsChecked)

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub RestoreDefault_Btn_Click(sender As Object, e As RoutedEventArgs) Handles RestoreDefault_Btn.Click
        Try
            Path1_TxtBx.Text = gAppPath & "\Backup"
            AutoBackup_ChkBx.IsChecked = True

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub


End Class
