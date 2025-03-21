Imports System.IO
Imports System.Text
Imports System.Windows.Forms

Public Module _Files

    Private Sub error_DebugPrint(ByVal routineName As String, ByVal errorDesc As String)
        _Debug.PrintError_(String.Format("_Files.{0}(): {1}", routineName, errorDesc))
    End Sub

    Public Function IsFileExist(ByVal path2file As String, ByVal msg2show As Boolean) As Boolean
        ''
        Dim file As File = Nothing
        ''
        IsFileExist = file.Exists(path2file)
        If Not IsFileExist Then
            If msg2show Then
                ''
                MsgBox(System.Windows.Forms.Application.ProductName & " cannot locate the following file:" & vbCr & vbCr &
                       path2file, CType(MsgBoxStyle.OkOnly + MsgBoxStyle.Exclamation, MsgBoxStyle), "File Not Found !")
                ''
            End If
        End If
        ''
    End Function
    Public Function IsFolderExist(ByVal path2folder As String, ByVal msg2show As Boolean) As Boolean
        ''
        IsFolderExist = Directory.Exists(path2folder)
        If Not IsFolderExist Then
            If msg2show Then
                ''
                MsgBox(System.Windows.Forms.Application.ProductName & " cannot locate the following file folder:" & vbCr & vbCr &
                       path2folder, CType(MsgBoxStyle.OkOnly + MsgBoxStyle.Exclamation, MsgBoxStyle), "File Folder Not Found !")
                ''
            End If
        End If
        ''
    End Function
    Public Function IsFolderExist_CreateIfNot(ByVal path2folder As String, ByVal msg2show As Boolean) As Boolean
        ''
        IsFolderExist_CreateIfNot = Directory.Exists(path2folder)
        If Not IsFolderExist_CreateIfNot Then
            Dim folder As DirectoryInfo = Directory.CreateDirectory(path2folder)
            IsFolderExist_CreateIfNot = folder.Exists
            If Not IsFolderExist_CreateIfNot Then
                If msg2show Then
                    ''
                    MsgBox(System.Windows.Forms.Application.ProductName & " cannot locate or create the following file folder:" & vbCr & vbCr &
                           path2folder, CType(MsgBoxStyle.OkOnly + MsgBoxStyle.Exclamation, MsgBoxStyle), "File Folder Not Found !")
                    ''
                End If
            End If
        End If
        ''
    End Function
    Public Function IsFolderEmpty(ByVal path2folder As String) As Boolean
        IsFolderEmpty = False
        Try
            Dim files As IO.FileInfo() = Nothing
            IsFolderEmpty = Not _Files.Get_FilesFromFolder(path2folder, files)
            files = Nothing
        Catch ex As Exception : _Debug.PrintError_("IsFolderEmpty(): " & ex.Message)
        End Try
    End Function

    Public Function ReadFile_ToEnd(ByVal path2file As String, ByVal msg2show As Boolean, ByRef data2string As String) As Boolean
        data2string = String.Empty '' assume.
        ReadFile_ToEnd = False
        ''
        Try
            If _Files.IsFileExist(path2file, msg2show) Then
                '
                Using sreader As New StreamReader(path2file, Encoding.[Default])
                    data2string = sreader.ReadToEnd
                    sreader.Close()
                End Using
                '
                ReadFile_ToEnd = (Not 0 = data2string.Length)
                '
            End If
        Catch ex As Exception : error_DebugPrint("ReadFile_ToEnd", ex.Message)
        End Try
    End Function

    Public Function WriteFile_ByOneString(ByVal data2write As String, ByVal path2file As String, Optional ByVal isAppend As Boolean = True) As Boolean
        Try
            WriteFile_ByOneString = True ' assume.
            Using swriter As New StreamWriter(path2file, isAppend)
                swriter.Write(data2write)
                swriter.Close()
            End Using
            ''
        Catch ex As Exception : _Debug.PrintError_("WriteFile_ByOneString(): " & ex.Message, path2file) : WriteFile_ByOneString = False
        End Try
    End Function
    Public Function WriteFile_ByOneString(ByVal bytes As Byte(), ByVal path2file As String, Optional ByVal isAppend As Boolean = True) As Boolean
        WriteFile_ByOneString = False
        Try
            If Not 0 = _Files.Get_FileExtension(path2file).Length Then ' we have extension
                Dim ifilemode As FileMode = IO.FileMode.Append ' assume.
                If Not isAppend Then
                    ifilemode = IO.FileMode.Create
                End If
                Using fstream As New IO.FileStream(path2file, ifilemode)
                    fstream.Write(bytes, 0, bytes.Length)
                    fstream.Close()
                    WriteFile_ByOneString = True
                End Using
            End If
        Catch ex As Exception : error_DebugPrint("WriteFile_ByOneString()", ex.Message)
        End Try
    End Function
    Public Function WriteFile_ToEnd(ByVal dreader As System.Data.OleDb.OleDbDataReader, ByVal path2file As String) As Boolean
        WriteFile_ToEnd = False
        Try
            If Not dreader Is Nothing Then
                Using swriter As New StreamWriter(path2file, True)
                    Do While dreader.Read
                        swriter.WriteLine(dreader.GetString(0))
                    Loop
                    swriter.Close()
                    WriteFile_ToEnd = True
                End Using
            End If
            ''
        Catch ex As Exception : _Debug.PrintError_("WriteFile_ToEnd(): " & ex.Message, path2file)
        End Try
    End Function
    Public Function WriteFile_ToEnd(ByVal bytes As Byte(), ByVal path2file As String) As Boolean
        WriteFile_ToEnd = False
        Try
            If Not 0 = _Files.Get_FileExtension(path2file).Length Then ' we have extension
                Using fstream As New IO.FileStream(path2file, IO.FileMode.Create)
                    fstream.Write(bytes, 0, bytes.Length)
                    fstream.Close()
                    WriteFile_ToEnd = True
                End Using
            End If
        Catch ex As Exception : error_DebugPrint("WriteFile_ToEnd", ex.Message)
        End Try
    End Function
    Public Function WriteFile_ToEnd(ByVal data2write As String, ByVal path2file As String) As Boolean
        WriteFile_ToEnd = False
        Try
            If Not 0 = _Files.Get_FileExtension(path2file).Length Then ' we have extension
                Using swriter As New StreamWriter(path2file)
                    swriter.Write(data2write)
                    swriter.Close()
                    WriteFile_ToEnd = True
                End Using
            End If
        Catch ex As Exception : error_DebugPrint("WriteFile_ToEnd", ex.Message)
        End Try
    End Function
    Public Function WriteFile_Append(ByVal data2write As String, ByVal path2file As String) As Boolean

        Try

            My.Computer.FileSystem.WriteAllText(path2file, data2write, True)
            Return True

        Catch ex As Exception

            MsgBox("ATTENTION...Write Failure..." & path2file & vbCrLf & vbCrLf & ex.Message, vbCrLf, "Shiprite Next")
            Return False

        End Try

    End Function

    Public Function CopyFile_ToNewFolder(ByVal frompath As String, ByVal topath As String, ByVal msg2show As Boolean) As Boolean
        CopyFile_ToNewFolder = False
        Try
            CopyFile_ToNewFolder = _Files.IsFileExist(frompath, False) AndAlso _Files.IsFolderExist_CreateIfNot(_Files.Get_DirName(topath), False)
            If CopyFile_ToNewFolder Then
                Dim fileFrom As New IO.FileInfo(frompath)
                fileFrom.CopyTo(topath, True)
                fileFrom = Nothing
                CopyFile_ToNewFolder = _Files.IsFileExist(topath, False)
            End If
            If Not CopyFile_ToNewFolder And msg2show Then
                Dim sbuilder As System.Text.StringBuilder = Nothing
                sbuilder.Append("From '") : sbuilder.Append(_Files.Get_DirName(frompath))
                sbuilder.Append("' to '") : sbuilder.Append(_Files.Get_DirName(topath)) : sbuilder.Append("' file directory...")
                _MsgBox.WarningMessage(sbuilder.ToString, "Failed to copy '" & _Files.Get_FileName(frompath) & "' file:")
                sbuilder = Nothing
            End If
        Catch ex As Exception : _Debug.PrintError_("CopyFile_ToNewFolder(): " & ex.Message)
        End Try
    End Function

    Public Function MoveFile_ToNewFolder(ByVal frompath As String, ByVal topath As String, ByVal filename As String, ByVal msg2show As Boolean) As Boolean
        Dim oldPath As String = Path.Combine(frompath, filename)
        Dim newPath As String = Path.Combine(topath, filename)
        Return MoveFile_ToNewFolder(oldPath, newPath, msg2show)
    End Function

    Public Function MoveFile_ToNewFolder(ByVal frompath As String, ByVal topath As String, ByVal msg2show As Boolean) As Boolean
        MoveFile_ToNewFolder = False
        Try
            MoveFile_ToNewFolder = _Files.CopyFile_ToNewFolder(frompath, topath, False)
            If MoveFile_ToNewFolder Then
                MoveFile_ToNewFolder = _Files.Delete_File(frompath, False)
            End If
            If Not MoveFile_ToNewFolder And msg2show Then
                Dim sbuilder As System.Text.StringBuilder = Nothing
                sbuilder.Append("From '") : sbuilder.Append(_Files.Get_DirName(frompath))
                sbuilder.Append("' to '") : sbuilder.Append(_Files.Get_DirName(topath)) : sbuilder.Append("' file directory...")
                _MsgBox.WarningMessage(sbuilder.ToString, "Failed to move '" & _Files.Get_FileName(frompath) & "' file:")
                sbuilder = Nothing
            End If
        Catch ex As Exception : _Debug.PrintError_("MoveFile_ToNewFolder(): " & ex.Message)
        End Try
    End Function

    Public Function Move_Folder(ByVal frompath As String, ByVal topath As String, ByVal msg2show As Boolean) As Boolean
        Move_Folder = False
        Try
            Move_Folder = Copy_Folder(frompath, topath, False)
            If Move_Folder Then
                Move_Folder = _Files.Delete_Folder(frompath, False)
            End If
            If Not Move_Folder And msg2show Then
                Dim sbuilder As System.Text.StringBuilder = Nothing
                sbuilder.Append("From '") : sbuilder.Append(frompath)
                sbuilder.Append("' to '") : sbuilder.Append(topath) : sbuilder.Append("' directory...")
                _MsgBox.WarningMessage(sbuilder.ToString, "Failed to move folder:")
                sbuilder = Nothing
            End If
        Catch ex As Exception : _Debug.PrintError_("Move_Folder(): " & ex.Message)
        End Try
    End Function

    Public Function Copy_Folder(ByVal frompath As String, ByVal topath As String, ByVal msg2show As Boolean) As Boolean
        Copy_Folder = False
        Try
            Copy_Folder = _Files.IsFolderExist(frompath, False) AndAlso _Files.IsFolderExist_CreateIfNot(topath, False)
            If Copy_Folder Then
                Dim folderFiles As FileInfo() = Nothing
                Copy_Folder = _Files.Get_FilesFromFolder(frompath, folderFiles)
                If Copy_Folder Then
                    For Each folderFile In folderFiles
                        Copy_Folder = _Files.MoveFile_ToNewFolder(frompath & "\" & folderFile.Name, topath & "\" & folderFile.Name, False)
                    Next
                End If
            End If
            If Not Copy_Folder And msg2show Then
                Dim sbuilder As System.Text.StringBuilder = Nothing
                sbuilder.Append("From '") : sbuilder.Append(frompath)
                sbuilder.Append("' to '") : sbuilder.Append(topath) : sbuilder.Append("' directory...")
                _MsgBox.WarningMessage(sbuilder.ToString, "Failed to copy folder:")
                sbuilder = Nothing
            End If
        Catch ex As Exception : _Debug.PrintError_("Copy_Folder(): " & ex.Message)
        End Try
    End Function

    Public Function Delete_Folder(ByVal path2folder As String, ByVal msg2show As Boolean, Optional isOverride As Boolean = False) As Boolean
        Delete_Folder = False
        Try
            Delete_Folder = Not _Files.IsFolderExist(path2folder, False) ' folder doesn't exist?
            If Not Delete_Folder Then ' folder exists
                '
                If isOverride Then
                    Delete_Folder = _Files.Delete_FilesFromFolder(path2folder, False)
                Else
                    Delete_Folder = Not _Files.IsFolderEmpty(path2folder) ' folder not empty?
                End If
                If Not Delete_Folder Then ' folder empty
                    Directory.Delete(path2folder)
                    Delete_Folder = Not _Files.IsFolderExist(path2folder, False)
                    If Not Delete_Folder And msg2show Then
                        Dim sbuilder As System.Text.StringBuilder = Nothing
                        sbuilder.Append("From '") : sbuilder.Append(path2folder) : sbuilder.Append("' directory...")
                        _MsgBox.WarningMessage(sbuilder.ToString, "Failed to delete folder:")
                        sbuilder = Nothing
                    End If
                End If
                '
            End If
        Catch ex As Exception : _Debug.PrintError_("Delete_Folder(): " & ex.Message, path2folder)
        End Try
    End Function

    Public Function Delete_File(ByVal path2file As String, ByVal msg2show As Boolean) As Boolean
        Delete_File = False
        Try
            Delete_File = Not _Files.IsFileExist(path2file, False)
            If Not Delete_File Then
                '
                File.Delete(path2file)
                Delete_File = Not _Files.IsFileExist(path2file, False)
                If Not Delete_File And msg2show Then
                    Dim sbuilder As System.Text.StringBuilder = Nothing
                    sbuilder.Append("From '") : sbuilder.Append(_Files.Get_DirName(path2file)) : sbuilder.Append("' file directory...")
                    _MsgBox.WarningMessage(sbuilder.ToString, "Failed to delete '" & _Files.Get_FileName(path2file) & "' file:")
                    sbuilder = Nothing
                End If
                '
            End If
        Catch ex As Exception : _Debug.PrintError_("Delete_File(): " & ex.Message, path2file)
        End Try
    End Function
    Public Function Delete_FilesFromFolder(ByVal path2folder As String, ByVal msg2show As Boolean) As Boolean
        Delete_FilesFromFolder = False
        Try
            Dim files As IO.FileInfo() = Nothing
            If _Files.Get_FilesFromFolder(path2folder, files) Then
                For i As Integer = 0 To files.Count - 1
                    files(i).Delete()
                Next i
            End If
            Delete_FilesFromFolder = Not _Files.Get_FilesFromFolder(path2folder, files)
            If Not Delete_FilesFromFolder And msg2show Then
                Dim sbuilder As System.Text.StringBuilder = Nothing
                sbuilder.Append("From '") : sbuilder.Append(path2folder) : sbuilder.Append("' file directory...")
                _MsgBox.WarningMessage(sbuilder.ToString, "Failed to delete " & files.Count.ToString & " file(s):")
                sbuilder = Nothing
            End If
            files = Nothing
        Catch ex As Exception : _Debug.PrintError_("Delete_FilesFromFolder(): " & ex.Message, path2folder)
        End Try
    End Function

    Public Function Run_File(ByVal path2file As String, ByVal msg2show As Boolean) As Boolean
        Run_File = False
        Try
            If _Files.IsFileExist(path2file, msg2show) Then
                Process.Start(path2file)
                Run_File = True
            End If
        Catch ex As Exception : _Debug.PrintError_("Run_File(1): " & ex.Message, path2file)
            If msg2show Then MsgBox("Failed to execute file..." & ControlChars.NewLine & ControlChars.NewLine & ex.Message, MsgBoxStyle.Critical)
        End Try
    End Function
    Public Function Run_File(ByVal path2file As String, ByVal arguments As String, ByVal msg2show As Boolean) As Boolean
        Run_File = False
        Try
            If _Files.IsFileExist(path2file, msg2show) Then
                Process.Start(path2file, arguments)
                Run_File = True
            End If
        Catch ex As Exception : _Debug.PrintError_("Run_File(2): " & ex.Message, path2file, arguments)
            If msg2show Then MsgBox("Failed to execute file..." & ControlChars.NewLine & ControlChars.NewLine & ex.Message, MsgBoxStyle.Critical)
        End Try
    End Function

    Public Function Get_FileName(ByVal path2file As String) As String
        Get_FileName = String.Empty '' assume.
        Try
            Get_FileName = Path.GetFileName(path2file)
        Catch ex As Exception : _Debug.PrintError_("Get_FileName(): " & ex.Message, path2file)
        End Try
    End Function
    Public Function Get_FileExtension(ByVal path2file As String) As String
        Get_FileExtension = String.Empty '' assume.
        Try
            Get_FileExtension = Path.GetExtension(path2file)
        Catch ex As Exception : error_DebugPrint("Get_FileExtension", ex.Message)
        End Try
    End Function
    Public Function Get_DirName(ByVal path2file As String) As String
        Get_DirName = String.Empty '' assume.
        Try
            Get_DirName = Path.GetDirectoryName(path2file)
        Catch ex As Exception : _Debug.PrintError_("Get_DirName(): " & ex.Message, path2file)
        End Try
    End Function
    Public Function Get_FileSize(ByVal path2file As String) As Long
        Get_FileSize = 0
        Try
            If _Files.IsFileExist(path2file, False) Then
                Dim file As New FileInfo(path2file)
                Get_FileSize = file.Length ' in bytes
            End If
        Catch ex As Exception : _Debug.PrintError_("Get_FileSize(): " & ex.Message, path2file)
        End Try
    End Function
    Public Function Get_FilesFromFolder(ByVal path2dir As String, ByRef files As IO.FileInfo()) As Boolean
        Get_FilesFromFolder = False
        Dim dir As New IO.DirectoryInfo(path2dir)
        files = Nothing ' assume.
        If dir.Exists Then
            files = dir.GetFiles()
            ' list the names of all files in the specified directory
            ''For Each file As IO.FileInfo In files
            ''Next
            Get_FilesFromFolder = (files.Count > 0)
        End If
    End Function

    Public Function Create_Folder(ByVal path2folder As String, ByVal msg2show As Boolean) As Boolean
        Create_Folder = IsFolderExist_CreateIfNot(path2folder, msg2show)
    End Function

    Public Function Show_SaveFileDialog(ByVal dialogTitle As String, ByVal dialogFilter As String, ByRef path2file As String) As Boolean
        Show_SaveFileDialog = False
        Dim saveFileDialog1 As New SaveFileDialog()
        path2file = String.Empty '' assume.
        Try

            If Not String.IsNullOrEmpty(dialogFilter) Then
                saveFileDialog1.Title = dialogTitle
            End If
            If String.IsNullOrEmpty(dialogFilter) Then
                saveFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*"
            Else
                saveFileDialog1.Filter = dialogFilter
            End If
            saveFileDialog1.FilterIndex = 1
            saveFileDialog1.RestoreDirectory = True

            Show_SaveFileDialog = (saveFileDialog1.ShowDialog = DialogResult.OK)
            If Show_SaveFileDialog Then
                Dim myStream As Stream = saveFileDialog1.OpenFile()
                If (myStream IsNot Nothing) Then
                    ' Code to write the stream goes here.
                    myStream.Close() : myStream.Dispose()
                End If
                path2file = saveFileDialog1.FileName
            End If
        Catch ex As Exception : _Debug.PrintError_("Show_SaveFileDialog(): " & ex.ToString) : MsgBox(ex.Message, MsgBoxStyle.Critical)
        Finally : saveFileDialog1.Dispose()
        End Try
    End Function
    Public Function Show_OpenFileDialog(ByVal dialogTitle As String, ByVal dialogFilter As String, ByRef path2file As String) As Boolean
        Show_OpenFileDialog = False
        Dim openFileDialog1 As New OpenFileDialog()
        path2file = String.Empty '' assume.
        Try

            If Not String.IsNullOrEmpty(dialogFilter) Then
                openFileDialog1.Title = dialogTitle
            End If
            If String.IsNullOrEmpty(dialogFilter) Then
                openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*"
            Else
                openFileDialog1.Filter = dialogFilter
            End If
            openFileDialog1.FilterIndex = 1
            openFileDialog1.RestoreDirectory = True

            Show_OpenFileDialog = (openFileDialog1.ShowDialog = DialogResult.OK)
            If Show_OpenFileDialog Then
                Dim myStream As Stream = openFileDialog1.OpenFile()
                If (myStream IsNot Nothing) Then
                    ' Code to write the stream goes here.
                    myStream.Close() : myStream.Dispose()
                End If
                path2file = openFileDialog1.FileName
            End If
        Catch ex As Exception : _Debug.PrintError_("Show_OpenFileDialog(): " & ex.ToString) : MsgBox(ex.Message, MsgBoxStyle.Critical)
        Finally : openFileDialog1.Dispose()
        End Try
    End Function
    Public Function Show_OpenFileBrowserDialog(ByVal ofd As OpenFileDialog, ByRef path2file As String, ByVal title As String) As Boolean
        Show_OpenFileBrowserDialog = False
        Try
            ofd.Filter = "All files (*.*)|*.*" '' assume.
            Dim splitPath() As String = Split(path2file, "\")
            If 0 < splitPath.Length - 1 AndAlso _Files.IsFileExist(path2file, False) Then
                ofd.InitialDirectory = Replace(path2file, "\" & splitPath(splitPath.Length - 1), "")
                ofd.FileName = splitPath(splitPath.Length - 1)
                If _Controls.Left(ofd.FileName, ofd.FileName.Length - 4).StartsWith(".") Then
                    Dim ext As String = _Controls.Left(ofd.FileName, ofd.FileName.Length - 3)
                    ofd.Filter = "(*." & ext & ")|*." & ext & "|All files (*.*)|*.*"
                End If
            End If
            ''
            ofd.Title = title
            If ofd.ShowDialog = DialogResult.OK Then
                Show_OpenFileBrowserDialog = _Files.IsFileExist(ofd.FileName, False)
            End If
            ''
        Catch ex As Exception : _Debug.PrintError_("Show_OpenFileBrowserDialog(): " & ex.Message)
        End Try
        ''
    End Function
    Public Function Show_OpenFolderBrowserDialog(ByVal fbd As FolderBrowserDialog, ByRef path2folder As String, ByVal title As String) As Boolean
        Show_OpenFolderBrowserDialog = False
        Try
            If _Files.IsFolderExist(path2folder, False) Then
                fbd.SelectedPath = path2folder
            End If
            ''
            fbd.Description = title
            If fbd.ShowDialog = DialogResult.OK Then
                Show_OpenFolderBrowserDialog = _Files.IsFolderExist(fbd.SelectedPath, False)
                path2folder = fbd.SelectedPath
            End If
            ''
        Catch ex As Exception : _Debug.PrintError_("Show_OpenFolderBrowserDialog(): " & ex.Message)
        End Try
        ''
    End Function

    Public Function Write_IniValue(IniPath As String, PutKey As String, PutVariable As String, PutValue As String) As Boolean
        Dim Temp As String
        Dim LcaseTemp As String
        Dim ReadKey As String
        Dim ReadVariable As String
        Dim LOKEY As Integer
        Dim HIKEY As Integer
        Dim KEYLEN As Integer
        Dim Var As Integer
        Dim VARENDOFLINE As Integer
        Dim NF As Integer

        Try
AssignVariables:
            NF = FreeFile()
            ReadKey = vbCrLf & "[" & Strings.LCase(PutKey) & "]" & Chr(13)
            KEYLEN = Len(ReadKey)
            ReadVariable = Chr(10) & Strings.LCase(PutVariable) & "="

EnsureDirExists:
            _Files.Create_Folder(_Files.Get_DirName(IniPath), False)

EnsureFileExists:
            FileOpen(NF, IniPath, OpenMode.Binary)
            FileClose(NF)
            File.SetAttributes(IniPath, FileAttributes.Archive)

LoadFile:
            FileOpen(NF, IniPath, OpenMode.Input)
            Temp = FileSystem.InputString(NF, LOF(NF))
            Temp = vbCrLf & Temp & "[]"
            FileClose(NF)
            LcaseTemp = LCase(Temp)

LogicMenu:
            LOKEY = Strings.InStr(LcaseTemp, ReadKey)
            If LOKEY = 0 Then GoTo AddKey : 
            HIKEY = Strings.InStr(LOKEY + KEYLEN, LcaseTemp, "[")
            Var = Strings.InStr(LOKEY, LcaseTemp, ReadVariable)
            If Var > HIKEY Or Var < LOKEY Then GoTo AddVariable
            GoTo RenewVariable

AddKey:
            Temp = Strings.Left(Temp, Len(Temp) - 2)
            Temp = Temp & vbCrLf & vbCrLf & "[" & PutKey & "]" & vbCrLf & PutVariable & "=" & PutValue
            GoTo TrimFinalString

AddVariable:
            Temp = Strings.Left(Temp, Len(Temp) - 2)
            Temp = Strings.Left(Temp, LOKEY + KEYLEN) & PutVariable & "=" & PutValue & vbCrLf & Strings.Mid(Temp, LOKEY + KEYLEN + 1)
            GoTo TrimFinalString

RenewVariable:
            Temp = Strings.Left(Temp, Len(Temp) - 2)
            VARENDOFLINE = Strings.InStr(Var, Temp, Chr(13))
            Temp = Strings.Left(Temp, Var) & PutVariable & "=" & PutValue & Strings.Mid(Temp, VARENDOFLINE)
            GoTo TrimFinalString

TrimFinalString:
            Temp = Strings.Mid(Temp, 2)
            Do Until Strings.InStr(Temp, vbCrLf & vbCrLf & vbCrLf) = 0
                Temp = Strings.Replace(Temp, vbCrLf & vbCrLf & vbCrLf, vbCrLf & vbCrLf)
            Loop

            Do Until Strings.Right(Temp, 1) > Chr(13)
                Temp = Strings.Left(Temp, Len(Temp) - 1)
            Loop

            Do Until Strings.Left(Temp, 1) > Chr(13)
                Temp = Strings.Mid(Temp, 2)
            Loop

OutputAmendedINIFile:
            FileOpen(NF, IniPath, OpenMode.Output)
            FileSystem.PrintLine(NF, Temp)
            FileClose(NF)

            Return True
        Catch ex As Exception
            Return False
        Finally : FileClose(NF)
        End Try
    End Function
    Public Function Read_IniValue(IniPath As String, Key As String, Variable As String) As String
        Dim NF As Integer
        Dim Temp As String
        Dim LcaseTemp As String
        Dim ReadyToRead As Boolean

        Try
AssignVariables:
            NF = FreeFile()
            Read_IniValue = ""
            Key = "[" & LCase$(Key) & "]"
            Variable = LCase$(Variable)

EnsureFileExists:
            FileOpen(NF, IniPath, OpenMode.Binary)
            FileClose(NF)
            File.SetAttributes(IniPath, FileAttributes.Archive)

LoadFile:
            FileOpen(NF, IniPath, OpenMode.Input)
            While Not EOF(NF)
                Temp = FileSystem.LineInput(NF)
                LcaseTemp = LCase$(Temp)
                If InStr(LcaseTemp, "[") <> 0 Then ReadyToRead = False
                If LcaseTemp = Key Then ReadyToRead = True
                If InStr(LcaseTemp, "[") = 0 And ReadyToRead = True Then
                    If InStr(LcaseTemp, Variable & "=") = 1 Then
                        Read_IniValue = Strings.Mid(Temp, 1 + Len(Variable & "="))
                        FileClose(NF) : Exit Function
                    End If
                End If
            End While
            FileClose(NF)
        Catch ex As Exception
            Return ""
        Finally : FileClose(NF)
        End Try
    End Function

End Module
