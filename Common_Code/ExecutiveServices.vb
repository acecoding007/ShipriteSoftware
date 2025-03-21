Imports System
Imports System.Diagnostics
Module ExecutiveServices

    Public Function LaunchProcess(FPath As String) As Integer

        Dim proc As New System.Diagnostics.Process()

        Try

            proc = Process.Start(FPath, "")

        Catch ex As Exception

            MsgBox("ATTENTION..." & FPath & " Not Running. Contact Support.", vbCritical, gProgramName)

        End Try
        Return 0

    End Function

    Public Function IsProcessRunning(ProcessName As String) As Boolean

        Dim p() As Process

        p = Process.GetProcessesByName(ProcessName)

        Return p.Count > 0

    End Function

End Module
