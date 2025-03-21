Imports System.Management
Imports System.Management.Instrumentation

Public Class ProgramInfo

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

    Private Sub ProgramInfo_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Try
            Dim HDSize As Double
            Dim HDFree As Double


            '------ SYSTEM INFORMATION-------------------------

            OSName_Label.Content = My.Computer.Info.OSFullName
            OSVersion_Label.Content = My.Computer.Info.OSVersion
            Is64Bit_Label.Content = Environment.Is64BitOperatingSystem
            PCName_Label.Content = Environment.MachineName
            UserName_Label.Content = Environment.UserName

            Processor_Label.Content = My.Computer.Registry.GetValue("HKEY_LOCAL_MACHINE\HARDWARE\DESCRIPTION\SYSTEM\CentralProcessor\0", "ProcessorNameString", Nothing)


            HDSize = Convert.ToDouble(My.Computer.FileSystem.Drives.Item(0).TotalSize)
            HDSize = HDSize / 1024 / 1024 / 1024 'converts size from Bytes to Gigabytes.
            HDSize_Label.Content = Math.Round(HDSize, 2) & " GB"


            HDFree = Convert.ToDouble(My.Computer.FileSystem.Drives.Item(0).TotalFreeSpace)
            HDFree = HDFree / 1024 / 1024 / 1024 'converts size from Bytes to Gigabytes.
            HD_FreeSpace_Label.Content = Math.Round(HDFree, 2) & " GB"

            RAM_Label.Content = Math.Round((Convert.ToDouble(My.Computer.Info.TotalPhysicalMemory)) / 1024 / 1024 / 1024, 2) & " GB"



            '--------SOFTWARE INFORMATION-----------------------
            ProgramVersion_Label.Content = FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly.Location).FileVersion
            ProgramLocation_Label.Content = System.Reflection.Assembly.GetExecutingAssembly().Location
            ProgramCreation_Label.Content = System.IO.File.GetLastWriteTime(System.Reflection.Assembly.GetExecutingAssembly.Location)

            DatabasePath_Label.Content = gShipriteDB

            DataPath_Label.Content = gDBpath
            ReportPath_Label.Content = gRptPath
            ApplicationPath_Label.Content = gAppPath

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub
End Class
