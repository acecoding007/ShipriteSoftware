Option Explicit On
'Option Strict On

Imports System.Data
Imports System.Data.SqlClient
Imports System.Data.OleDb
Imports System.IO
Imports System.ComponentModel

Module RecordSetFunctions

    Public Structure FieldDefinition

        Dim FName As String
        Dim FValue As String
        Dim FType As String

    End Structure

    Public Structure RecordArrayDefinition

        Dim SkipRecord As Boolean
        Dim Field() As FieldDefinition

    End Structure

    Public Structure RecordSetDefinition

        Dim RecordSet() As RecordArrayDefinition
        Dim CurrentRecord As Long
        Dim RecordCount As Long
        Dim FieldCT As Integer
        Dim LoopCT As Integer
        Dim ReDimSize As Long

    End Structure

    Public Function io_DumpRecordsetToLocalTable(dbPath As String, TRecordSet As RecordSetDefinition, TName As String, Optional IgnoreTestForID As Boolean = False) As Long

        Dim rct As Long

        Dim ConnectionString As String
        Dim oledbAdapter As New OleDbDataAdapter
        Dim SQL As String

        ConnectionString = MakeConnectionString(dbPath, "")
        Dim connection As OleDbConnection = New OleDbConnection(ConnectionString)

        connection.Open()

        Dim stop_watch_1 As New Stopwatch
        Dim getDate As Date = Date.Now
        rct = 0
        For i As Integer = 0 To TRecordSet.RecordCount - 1

            If TRecordSet.RecordSet(i).SkipRecord = False Then

                SQL = MakeInsertSQLFromRecordSet(TName, i, TRecordSet, False, IgnoreTestForID, False)

                oledbAdapter.InsertCommand = New OleDbCommand(SQL, connection)
                oledbAdapter.InsertCommand.ExecuteNonQuery()
                rct += 1

            End If

        Next

        Return rct
    End Function

    Public Function MakeInsertSQLFromRecordSet(TName As String, RecordID As Long, SQLRecordSet As RecordSetDefinition, UsingCloud As Boolean, Optional IgnoreTestForID As Boolean = False, Optional isIncludeBlankElements As Boolean = False) As String

        Dim SQL As String
        Dim eName As String
        Dim eValue As String
        Dim eNames As String = ""
        Dim eValues As String = ""
        Dim eType As String
        Dim IDfnum As Integer
        Dim i As Integer

        For i = 0 To SQLRecordSet.FieldCT - 1

            If SQLRecordSet.RecordSet(RecordID).Field(i).FName = "ID" Then

                IDfnum = i
                Exit For

            End If

        Next
        If i = SQLRecordSet.FieldCT And IgnoreTestForID = False Then

            MsgBox("ATTENTION...MakeInsertSQLFromSchema" & vbCrLf & vbCrLf & "Table [" & TName & "] Does Not Have a Primary Key...ID", vbCritical, gProgramName)
            Return ""
            Exit Function

        End If
        SQL = ""
        For i = 0 To SQLRecordSet.FieldCT - 1

            eName = SQLRecordSet.RecordSet(RecordID).Field(i).FName
            eValue = SQLRecordSet.RecordSet(RecordID).Field(i).FValue
            eType = SQLRecordSet.RecordSet(RecordID).Field(i).FType

            If eValue = "" And Not isIncludeBlankElements Then

                GoTo SkipIT

            End If

            If Not InStr(1, eValue, "'") = 0 Then

                eValue = FlushOut(eValue, "'", "~")
                eValue = FlushOut(eValue, "~", "''")

            End If

            If Not eNames = "" Then

                eNames &= ", "
                eValues &= ", "

            End If
            eNames &= "[" & eName & "]"
            Select Case eType

                Case "String"

                    eValues &= "'" & eValue & "'"

                Case "Int32", "Int64", "Single", "Double"

                    If IsNumeric(eValue) And Not InStr(1, eValue, ",") = 0 Then

                        eValue = FlushOut(eValue, ",", "")

                    End If
                    If eValue = "" Then

                        eValue = "0"

                    End If
                    eValues &= eValue

                Case "Boolean"

                    If UsingCloud = True And (eValue = "True" Or eValue = "False") Then

                        eValues &= "'" & eValue & "'"

                    Else

                        eValues &= eValue

                    End If

                Case "DateTime"

                    If UsingCloud = False Then

                        eValues &= "#" & eValue & "#"

                    Else

                        eValues &= "'" & eValue & "'"

                    End If

            End Select

SkipIT:

        Next i
        SQL = "INSERT INTO " & TName & " (" & eNames & ") VALUES (" & eValues & ")"
        Return SQL

    End Function

    Public Function IO_GetSegmentSetInToStructure(dbPath As String, SQL As String, ByRef MyRecordSet As RecordSetDefinition, Optional Password As String = "", Optional AddAdditionalFields As Integer = 0, Optional CallFromLocation As String = "", Optional AppendRecordSet As Boolean = False, Optional RecordSelection As String = "") As Long

        Dim ct As Integer
        Dim i As Integer
        Dim ConnectionString As String
        Dim buf As String
        Dim index As Integer

        buf = ""

        ' Allocate Structure Space
        If AppendRecordSet = True Then

            MyRecordSet.LoopCT = 0
            MyRecordSet.ReDimSize += 100
            ReDim Preserve MyRecordSet.RecordSet(MyRecordSet.ReDimSize)

        Else

            MyRecordSet.RecordCount = 0
            MyRecordSet.LoopCT = 0
            MyRecordSet.ReDimSize = 100
            ReDim MyRecordSet.RecordSet(MyRecordSet.ReDimSize)
            MyRecordSet.CurrentRecord = 0

        End If
        If InStr(dbPath, "$") = 0 Then

            Dim connectString = MakeConnectionString(dbPath, Password)
            Dim cn As OleDbConnection = New OleDbConnection(connectString)

            Try

                cn.Open()

            Catch ex As Exception

                MsgBox("ATTENTION...GetSegmentSet" & vbCrLf & vbCrLf & ex.Message, MsgBoxStyle.Exclamation)
                IO_GetSegmentSetInToStructure = ""
                Exit Function

            End Try
            Try

                Dim selectString As String = SQL
                Dim cmd As OleDbCommand = New OleDbCommand(selectString, cn)
                Dim reader As OleDbDataReader = cmd.ExecuteReader()

                ct = 0
                buf = ""
                MyRecordSet.FieldCT = reader.FieldCount
                While (reader.Read())

                    ' Add more space if necessary

                    If MyRecordSet.LoopCT = 100 Then

                        MyRecordSet.LoopCT = 0
                        MyRecordSet.ReDimSize = MyRecordSet.ReDimSize + 100
                        ReDim Preserve MyRecordSet.RecordSet(MyRecordSet.ReDimSize)

                    End If

                    ReDim MyRecordSet.RecordSet(MyRecordSet.RecordCount).Field(reader.FieldCount + AddAdditionalFields)

                    For i = 0 To reader.FieldCount - 1

                        MyRecordSet.RecordSet(MyRecordSet.RecordCount).Field(ct).FName = reader.GetName(i).ToString
                        MyRecordSet.RecordSet(MyRecordSet.RecordCount).Field(ct).FValue = Trim(reader(i).ToString())
                        MyRecordSet.RecordSet(MyRecordSet.RecordCount).Field(ct).FType = FlushOut(reader.GetFieldType(i).ToString, "System.", "")
                        ct += 1

                    Next

                    ct = 0
                    MyRecordSet.RecordCount += 1
                    MyRecordSet.LoopCT += 1

                End While

                reader.Close()
                cn.Close()

            Catch ex As Exception

                MsgBox(ex.Message)

            End Try

        Else

            If Strings.Mid(SQL, 1, 2) = "$$" Then

                If Not InStr(SQL, "GetBlankSegment") = 0 Then

                    i = InStr(SQL, "/")
                    SQL = Strings.Mid(SQL, i + 1)
                    buf = IO_GetFieldsCollection(dbPath, SQL, "", False, False, True)
                    Return buf
                    Exit Function

                End If

            End If

            '       Fixing SQL Compatibility Issues

            SQL = FlushOut(SQL, "#", "'")
            SQL = FlushOut(SQL, "chr(", "char(")

            SQL = FlushOut(SQL, "FIRST", "~")
            SQL = FlushOut(SQL, "~", "MAX")

            SQL = FlushOut(SQL, "True", "~")
            SQL = FlushOut(SQL, "true", "~")
            SQL = FlushOut(SQL, "~", "'True'")

            SQL = FlushOut(SQL, "False", "~")
            SQL = FlushOut(SQL, "false", "~")
            SQL = FlushOut(SQL, "~", "'False'")

            If Not dbPath = "AZURE" Then

                i = InStr(dbPath, "$")
                index = CInt(Strings.Mid(dbPath, 1, i - 1))
                ConnectionString = gConnectionStrings(index).ConnectionString

            Else

                ConnectionString = gAzureConnectionString

            End If

            Dim sqlConnection As New SqlConnection(ConnectionString)
            Dim cmd As New SqlCommand
            Dim reader As SqlDataReader

            cmd.CommandText = SQL
            cmd.CommandType = CommandType.Text
            cmd.Connection = sqlConnection

            sqlConnection.Open()

            buf = ""
            Try

                reader = cmd.ExecuteReader()

            Catch ex As Exception

                MsgBox(Err.Number & " - " & Err.Description)
                Return ""
                Exit Function

            End Try

            MyRecordSet.FieldCT = reader.FieldCount
            While (reader.Read())

                ' Add more space if necessary

                If MyRecordSet.LoopCT = 100 Then

                    MyRecordSet.LoopCT = 0
                    MyRecordSet.ReDimSize = MyRecordSet.ReDimSize + 100
                    ReDim Preserve MyRecordSet.RecordSet(MyRecordSet.ReDimSize)

                End If

                ReDim MyRecordSet.RecordSet(MyRecordSet.RecordCount).Field(reader.FieldCount + AddAdditionalFields)

                For i = 0 To reader.FieldCount - 1

                    MyRecordSet.RecordSet(MyRecordSet.RecordCount).Field(ct).FName = reader.GetName(i).ToString
                    MyRecordSet.RecordSet(MyRecordSet.RecordCount).Field(ct).FValue = Trim(reader(i).ToString())
                    ct += 1

                Next

                ct = 0
                MyRecordSet.RecordCount += 1
                MyRecordSet.LoopCT += 1

            End While

            reader.Close()

            sqlConnection.Close()

        End If

        IO_GetSegmentSetInToStructure = MyRecordSet.RecordCount

    End Function

    Public Function GetFieldNumber(MyRecordSet As RecordSetDefinition, FName As String) As Integer

        Dim i As Integer

        For i = 0 To MyRecordSet.FieldCT - 1

            If MyRecordSet.RecordSet(0).Field(i).FName = FName Then

                Exit For

            End If

        Next i
        If i < MyRecordSet.FieldCT Then

            GetFieldNumber = i

        Else

            GetFieldNumber = -1

        End If

    End Function

    Public Function GetRecordID(MyRecordSet As RecordSetDefinition, FName As String, SearchValue As String) As Long

        Dim i As Long
        Dim fnum As Integer

        If MyRecordSet.RecordCount = 0 Then

            GetRecordID = -1
            Exit Function

        End If
        For i = 0 To MyRecordSet.FieldCT - 1

            If MyRecordSet.RecordSet(0).Field(i).FName = FName Then

                Exit For

            End If

        Next i
        If i < MyRecordSet.FieldCT Then

            fnum = i

        Else

            MsgBox("ATTENTION...GetRecordID" & vbCrLf & vbCrLf & FName & " - NOT FOUND", vbCritical, gProgramName)
            GetRecordID = -2  ' There's a programming error.  Stop the Process
            Exit Function

        End If
        For i = 0 To MyRecordSet.RecordCount - 1

            If MyRecordSet.RecordSet(i).Field(fnum).FValue = SearchValue Then

                Exit For

            End If

        Next i
        If i < MyRecordSet.RecordCount Then

            GetRecordID = i

        Else

            GetRecordID = -1

        End If

    End Function

    Public Function AddSegmentToRecordSet(MyRecordSet As RecordSetDefinition, Segment As String) As Long

        Dim i As Integer
        Dim RecordID As Long
        Dim NextID As Long

        RecordID = MyRecordSet.RecordCount
        MyRecordSet.RecordCount = MyRecordSet.RecordCount + 1
        MyRecordSet.ReDimSize = MyRecordSet.ReDimSize + 1
        ReDim Preserve MyRecordSet.RecordSet(UBound(MyRecordSet.RecordSet) + 1)
        ReDim Preserve MyRecordSet.RecordSet(RecordID).Field(MyRecordSet.FieldCT)
        NextID = RecordID - 1
        If NextID < 0 Then

            NextID = 0

        End If
        For i = 0 To MyRecordSet.FieldCT - 1

            MyRecordSet.RecordSet(RecordID).Field(i).FName = MyRecordSet.RecordSet(NextID).Field(i).FName
            MyRecordSet.RecordSet(RecordID).Field(i).FValue = ExtractElementFromSegment(MyRecordSet.RecordSet(RecordID).Field(i).FName, Segment)

        Next i
        AddSegmentToRecordSet = RecordID

    End Function

    Public Function PutFieldDataIntoRecord(MyRecordSet As RecordSetDefinition, FName As String, RecordID As Long, FValue As String) As Integer

        Dim i As Integer

        For i = 0 To MyRecordSet.FieldCT - 1

            If MyRecordSet.RecordSet(RecordID).Field(i).FName = FName Then

                Exit For

            End If

        Next i
        If i < MyRecordSet.FieldCT Then

            MyRecordSet.RecordSet(RecordID).Field(i).FValue = FValue
            PutFieldDataIntoRecord = 0

        Else

            MsgBox("ATTENTION...GetFieldDataFromRecord" & vbCrLf & vbCrLf & FName & " - NOT FOUND", vbCritical, gProgramName)
            PutFieldDataIntoRecord = 1

        End If


    End Function


    Public Function GetFieldDataFromRecord(MyRecordSet As RecordSetDefinition, FName As String, RecordID As Long, Optional Silent As Boolean = False) As String

        Dim i As Integer

        If RecordID < 0 And Silent = False Then

            MsgBox("ATTENTION...GetFieldDataFromRecord" & vbCrLf & vbCrLf & FName & " - Missing Record. Report this immediately.", vbCritical, gProgramName)
            GetFieldDataFromRecord = ""
            Exit Function

        ElseIf RecordID < 0 And Silent = True Then

            GetFieldDataFromRecord = "MISSING RECORD"
            Exit Function

        End If
        If MyRecordSet.RecordCount = 0 Then

            GetFieldDataFromRecord = ""
            Exit Function

        End If
        For i = 0 To MyRecordSet.FieldCT - 1

            If MyRecordSet.RecordSet(RecordID).Field(i).FName = FName Then

                Exit For

            End If

        Next i
        If i < MyRecordSet.FieldCT Then

            GetFieldDataFromRecord = MyRecordSet.RecordSet(RecordID).Field(i).FValue

        Else

            If Silent = False Then

                MsgBox("ATTENTION...GetFieldDataFromRecord" & vbCrLf & vbCrLf & FName & " - NOT FOUND", vbCritical, gProgramName)
                GetFieldDataFromRecord = ""

            Else

                GetFieldDataFromRecord = "NOT FOUND"

            End If

        End If

    End Function

    Function NEW_ReloadRecordIntoRecordSet(SQL As String, UpdateRecordSet As RecordSetDefinition, RecordID As Long) As Integer

        Dim SegmentSet As String
        Dim j As Integer

        SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
        For j = 0 To UpdateRecordSet.FieldCT - 1

            UpdateRecordSet.RecordSet(RecordID).Field(j).FValue = ExtractElementFromSegment(UpdateRecordSet.RecordSet(RecordID).Field(j).FName, SegmentSet)

        Next j
        NEW_ReloadRecordIntoRecordSet = 0

    End Function

    Function ExtractSegmentFromRecordSet(MySET As RecordSetDefinition, RecordID As Long) As String

        Dim Segment As String
        Dim i As Integer

        If RecordID = -1 Then

            ExtractSegmentFromRecordSet = ""
            Exit Function

        End If
        Segment = ""
        For i = 0 To MySET.FieldCT - 1

            Segment = AddElementToSegment(Segment, MySET.RecordSet(RecordID).Field(i).FName, MySET.RecordSet(RecordID).Field(i).FValue)

        Next i
        ExtractSegmentFromRecordSet = Segment

    End Function

    Public Function IncrementalReadIntoStructure(dbPath As String, TName As String, Fields As String, ByRef TheRecordSet As RecordSetDefinition, BeginningID As Long, EndingID As Long, OrderBy As String) As Long

        Dim SQL As String
        Dim SegmentSet As String
        Dim ret As Long
        Dim i As Long
        Dim iBegin As Long
        Dim iEnd As Long
        Dim iStep As Long
        Dim iStep2 As Long
        Dim AppendIt As Boolean

        ret = 0
        If EndingID = 0 Then

            SQL = "SELECT MAX(ID) AS MaxID FROM " & TName
            SegmentSet = IO_GetSegmentSet(dbPath, SQL)
            EndingID = Val(ExtractElementFromSegment("MaxID", SegmentSet))

        End If
        iBegin = BeginningID
        iEnd = EndingID
        iStep = 50000
        For i = iBegin To iEnd Step iStep

            If i = iBegin Then

                AppendIt = False

            Else

                AppendIt = True

            End If
            iStep2 = iStep
            If i + iStep2 > iEnd Then

                iStep2 = iEnd - i

            End If

        Next i
        MsgBox(TheRecordSet.RecordCount)
        Return 0

    End Function

    Public Function MakeSegmentFromRecord(MySET As RecordSetDefinition, iRecord As Long, Optional NoBlanks As Boolean = False) As String

        Dim Segment As String
        Dim i As Integer
        Dim SkipIt As Boolean

        Segment = ""
        For i = 0 To MySET.FieldCT - 1

            If NoBlanks = True And MySET.RecordSet(iRecord).Field(i).FValue = "" Then

                SkipIt = True

            Else

                SkipIt = False

            End If

            If SkipIt = False Then

                Segment = AddElementToSegment(Segment, MySET.RecordSet(iRecord).Field(i).FName, MySET.RecordSet(iRecord).Field(i).FValue)

            End If

        Next i
        MakeSegmentFromRecord = Segment

    End Function

    Public Function DumpRecordSetToFile(MyRecordSet As RecordSetDefinition, FPath As String, Delimeter As String) As Long

        Dim i As Long
        Dim j As Integer
        Dim buf As String = ""

        If MyRecordSet.RecordCount = 0 Then

            Return 0
            Exit Function

        End If
        Dim FILE_NAME As String = FPath

        If System.IO.File.Exists(FILE_NAME) = False Then

            File.Create(FPath).Dispose()

        End If

        Dim objWriter As New System.IO.StreamWriter(FILE_NAME)

        For j = 0 To MyRecordSet.FieldCT - 1

            If Not buf = "" Then

                buf &= Delimeter

            End If
            buf &= MyRecordSet.RecordSet(i).Field(j).FName

        Next j
        buf &= vbCrLf
        objWriter.Write(buf)
        For i = 0 To MyRecordSet.RecordCount - 1

            buf = ""
            For j = 0 To MyRecordSet.FieldCT - 1

                If Not buf = "" Then

                    buf &= Delimeter

                End If
                buf &= MyRecordSet.RecordSet(i).Field(j).FValue

            Next j
            buf &= vbCrLf
            objWriter.Write(buf)

        Next i
        objWriter.Close()
        DumpRecordSetToFile = i

    End Function

End Module
