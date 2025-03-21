Imports System.Reflection
Imports System.Windows.Forms

Class Application

    ' Application-level events, such as Startup, Exit, and DispatcherUnhandledException
    ' can be handled in this file.

    'Function : checkInstance
    'Input Parameter : None
    'Return Value : Returns the Process if the exe is already running or else returns nothing
    Public Function CheckInstance() As Process
        Try
            Dim currentProcess As Process = Process.GetCurrentProcess()
            Dim allProcesses() As Process = Process.GetProcessesByName(currentProcess.ProcessName)

            For Each process As Process In allProcesses
                If process.Id <> currentProcess.Id Then
                    If [Assembly].GetExecutingAssembly().Location = currentProcess.MainModule.FileName Then
                        Return process
                    End If
                End If
            Next

            Return Nothing
        Catch ex As Exception

            MessageBox.Show(Err.Description)
            Return Nothing

        End Try

    End Function

    Public Sub MainProcessCheck()

        Try
            Dim tempProcess As Process
            tempProcess = CheckInstance()
            If tempProcess Is Nothing Then

            Else
                MessageBox.Show("Application is already running.", "SHIPRITE",
                MessageBoxButtons.OKCancel, MessageBoxIcon.Information)
                Application.Current.Shutdown()
            End If

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try

    End Sub

    Private Sub Application_Startup(sender As Object, e As StartupEventArgs) Handles Me.Startup

        If e.Args.Length > 0 Then
            For Each argument As String In e.Args
                Select Case argument
                    Case "-r"
                        System.Threading.Thread.Sleep(5000)
                End Select
            Next
        End If

        Call MainProcessCheck()

    End Sub
End Class
