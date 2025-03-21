Option Explicit On
'Option Strict On

Imports System.Data
Imports System.Data.SqlClient
Imports System.Data.OleDb
Imports System.IO
Imports System.ComponentModel

Module DatabaseFunctions

    Public Function DataBase_Update(filePath As String, Optional Swiper As Boolean = False, Optional DontKillFile As String = "") As Integer

        Dim SQL As String
        Dim Segment As String
        Dim SegmentSet As String
        Dim buf As String
        Dim holdbuf As String
        Dim SecBuf As String
        Dim word As String
        Dim dbPath As String
        Dim TName As String
        Dim eName As String
        Dim EType As String
        Dim eSize As String
        Dim eIOMethod As String
        Dim iloc As Integer
        Dim ProcCT As Integer
        Dim ID As Long
        Dim ForceLoad As String
        Dim FName As String
        Dim FData As String
        Dim TCT As Integer
        Dim ret As Long
        dbPath = ""

        ' NOTE:  YESNO = a boolean field.

        TCT = 0
        Dim FILE_NAME As String = filePath
        If System.IO.File.Exists(FILE_NAME) = True Then

            Dim objReader As New System.IO.StreamReader(FILE_NAME)
            Do While objReader.Peek() <> -1

                buf = objReader.ReadLine()
                holdbuf = buf           ' get copy of buf for error message if needed
                If Trim$(buf) = "" Then

                    GoTo ResumeWithError

                End If
                If Mid(buf, 0, 1) = "$" Then

                    iloc = InStr(1, buf, " ")
                    If Not iloc = 0 Then

                        word = Trim$(Mid(buf, 0, iloc - 1))
                        buf = Trim$(Mid(buf, iloc))

                    Else

                        word = buf

                    End If
                    word = UCase(word)
                    Select Case word

                        Case "$DATABASE"

                            buf = UCase(buf)
                            Select Case buf

                                Case "SHIPRITE"

                                    dbPath = gShipriteDB

                                Case "REPORTS"

                                    dbPath = gReportsDB

                                Case "QUICKBOOKS"

                                    dbPath = gQBdb

                                Case "SMARTSWIPE", "SMARTSWIPER"

                                    dbPath = gSmartSwiperDB

                                Case "ZIPCODES"

                                    dbPath = gZipCodeDB

                            End Select
                            buf = Dir(dbPath)

                            If buf = "" And InStr(1, dbPath, "$") = 0 Then

                                If dbPath = gSalonDB And gProgramEdition = "Salon Edition" Then

                                    MsgBox("ATTENTION...Database Updates" & vbCrLf & vbCrLf & "Unexpected termination of UPD File." & vbCrLf & "Database NOT Found..." & dbPath, vbCritical, gProgramName)

                                End If
                                Exit Do

                            End If
                            SecBuf = ""
                            If dbPath = gSecurityDB Then

                                SecBuf = gSecurityPassword

                            End If

                        Case "$END"

                            Exit Do

                    End Select

                Else

                    iloc = InStr(1, buf, " ")       ' Table Name
                    If iloc = 0 Then

                        GoTo BadLineFormat

                    End If
                    TName = Trim$(Mid(buf, 0, iloc - 1))
                    buf = Trim$(Mid(buf, iloc))

                    iloc = InStr(1, buf, ",")       ' Element Name
                    If iloc = 0 Then

                        GoTo BadLineFormat

                    End If
                    eName = Trim$(Mid(buf, 0, iloc - 1))
                    buf = Trim$(Mid(buf, iloc))

                    iloc = InStr(1, buf, ",")       ' Element Type
                    If iloc = 0 Then

                        GoTo BadLineFormat

                    End If
                    EType = Trim$(Mid(buf, 0, iloc - 1))
                    buf = Trim$(Mid(buf, iloc))

                    eSize = Val(buf)
                    Select Case EType

                        Case "MEMO", "TEXT"

                            EType = "TEXT"

                        Case "BOOLEAN", "YESNO"

                            If InStr(1, dbPath, "$") = 0 Then

                                EType = "YESNO"

                            Else

                                EType = "BIT"

                            End If

                        Case "LONG"

                            If Not InStr(1, dbPath, "$") = 0 Then

                                EType = "BIGINT"

                            End If

                        Case "CHAR"

                            EType = "VARCHAR(" & eSize & ") NULL"

                    End Select
                    TName = FlushOut(TName, ",", "")
                    If eName = "LedgerStartInvNum" Then

                        eName = eName

                    End If
                    If eName = "PoIndex" Then

                        eName = eName

                    End If
                    If Not TName = "NULL" Then

                        buf = IO_GetTableCollection(dbPath, TName)
                        If Not buf = "" Then

                            buf = IO_GetFieldsCollection(dbPath, TName, eName, False, False, False)

                        Else

                            MsgBox("ATTENTION...DataBase_Update" & vbCrLf & vbCrLf & "TABLE NOT FOUND..." & TName & ",,,,,,SKIPPED", vbInformation, gProgramName)
                            buf = "Ignore"
                            ret = -1

                        End If
                        If buf = "" Then

                            ret = UpdateRunTimePolicy(gGLOBALpolicy, "AlterTableRequest", "True")
                            If InStr(1, dbPath, "$") = 0 Then

                                SQL = "ALTER TABLE " & TName & " ADD [" & eName & "] " & EType & ";"

                            Else

                                Select Case EType

                                    Case "DOUBLE"

                                        SQL = "ALTER TABLE " & TName & " ADD [" & eName & "] DECIMAL (12,2);"

                                    Case Else

                                        SQL = "ALTER TABLE " & TName & " ADD [" & eName & "] " & EType & ";"

                                End Select

                            End If
                            ret = IO_UpdateSQLProcessor(dbPath, SQL)
                            ret = UpdateRunTimePolicy(gGLOBALpolicy, "AlterTableRequest", "False")

                        Else

                            ret = -1

                        End If

                    Else

                        ret = 0

                    End If
                    If dbPath = "" Then

                        ret = -1

                    End If
                    System.Windows.Forms.Application.DoEvents()

                End If

ResumeWithError:

            Loop
            objReader.Close()

        Else

            MessageBox.Show("File Does Not Exist")
            DontKillFile = "True"
            ProcCT = 0

        End If
        DataBase_Update = ProcCT
        If ProcCT > 0 Then

            MsgBox("ATTENTION..." & ProcCT & " Database Updates Processed Successfully" & vbCrLf & vbCrLf & "See Transaction Logs for Details", vbInformation, gProgramName)
            DataBase_Update = 1

        Else

            DataBase_Update = 0

        End If
        If Not DontKillFile = "True" Then

            'My.Computer.FileSystem.DeleteFile(filePath)

        End If
        Exit Function

BadLineFormat:

        MsgBox("ATTENTION...ERROR Processing Fields.UPD" & vbCrLf & vbCrLf & holdbuf, vbInformation, gProgramName)
        GoTo ResumeWithError

UpdateError:

        MsgBox("ATTENTION...DataBase_Update" & vbCrLf & vbCrLf & Err.Number & " - " & Err.Description, vbCritical, gProgramName)
        On Error GoTo 0
        DataBase_Update = 0
        Exit Function

    End Function

    Public Function IO_CacheServiceTables() As Integer

        Dim ServiceName As String
        Dim SQL As String
        Dim Segment As String
        Dim SegmentSet As String
        Dim ct As Integer
        Dim fct As Integer
        Dim TableCollection As String
        Dim dbPath As String
        Dim iloc As Integer
        Dim TName As String
        Dim Tally As Integer
        Dim buf As String
        Dim word As String
        Dim FieldCT As Integer
        Dim eName As String
        Dim eValue As String
        Dim i As Integer
        Dim Password As String
        Dim index As Integer
        gFedEx_OneRate_Tables = New List(Of String)

        eName = ""
        eValue = ""
        i = 0
        iloc = 0
        dbPath = ""
        ServiceName = ""
        SQL = ""
        Segment = ""
        SegmentSet = ""
        TableCollection = ""
        TName = ""
        Password = ""
        fct = 0
        index = 0
        gSVCct = 0


        ServiceName = Dir(gServiceTablesPath & "\*.*")
        Do Until ServiceName = ""

            If File.Exists(gServiceTablesPath & "\Custom_Rates\" & ServiceName) Then
                dbPath = gServiceTablesPath & "\Custom_Rates\" & ServiceName
            Else
                dbPath = gServiceTablesPath & "\" & ServiceName
            End If


            If File.Exists(dbPath) And ServiceName <> "FlatRates.accdb" Then

                TableCollection = IO_GetTableCollection(dbPath, "")
                Do Until TableCollection = ""

                    iloc = InStr(1, TableCollection, ",")
                    If Not iloc = 0 Then

                        TName = Trim(Mid(TableCollection, 0, iloc - 1))
                        TableCollection = Trim(Mid(TableCollection, iloc))

                    Else

                        TName = TableCollection
                        TableCollection = ""

                    End If

                    If TName.Contains("OneRate") Then
                        gFedEx_OneRate_Tables.Add(TName)
                    End If

                    SQL = "Select COUNT(*) As Tally FROM [" & TName & "]"
                    SegmentSet = IO_GetSegmentSet(dbPath, SQL)
                    Tally = CInt(Val(ExtractElementFromSegment("Tally", SegmentSet)))
                    ReDim gServiceTables(gSVCct).Rates(Tally)                                   ' Set the array size to number of lines in table
                    gServiceTables(gSVCct).ServiceName = TName


                    ct = 0
                    For i = 0 To gMCT - 1

                        If gMaster(i).ServiceTable = TName Then

                            Exit For

                        End If

                    Next
                    If Not i = gMCT Then

                        gServiceTables(gSVCct).MasterIndex = i

                    End If

                    gServiceTables(gSVCct).RecordCount = Tally

                    If Not InStr(1, UCase(TName), "-INTP") = 0 Then

                        gServiceTables(gSVCct).International = True

                    Else

                        gServiceTables(gSVCct).International = False

                    End If
                    buf = IO_GetFieldsCollection(dbPath, TName, "LBS", False, False, False)
                    iloc = InStr(1, buf, "LBS")
                    gServiceTables(gSVCct).dpPath = dbPath

                    If Tally > 300 Or iloc = 0 Or
                        TName.Contains("Discounts") Or
                        TName.Contains("OneRate") Or
                        TName.Contains("Temp_Surcharges") Or
                        TName.Contains("Holiday_Charges") Or
                        TName.Contains("INCENTIVE") Or
                        TName.Contains("CommercialPlus") Or
                        TName.Contains("Insurance") Or
                        TName.Contains("FirstClass") Or
                        TName.Contains("UPS-DAS") Or
                        TName.Contains("Regional") Then


                        ' Do not CACHE Large TABLES or Tables w/o LBS or Tables that will use direct access
                        gServiceTables(gSVCct).UseDirectDBAccess = True

                    Else

                        gServiceTables(gSVCct).UseDirectDBAccess = False
                        buf = IO_GetFieldsCollection(dbPath, TName, "", False, False, False)
                        FieldCT = 0
                        Do Until buf = ""                                       ' Count the number of columns

                            iloc = InStr(1, buf, ",")
                            If Not iloc = 0 Then

                                word = Mid(buf, 0, iloc - 1)
                                buf = Mid(buf, iloc + 1)

                            Else

                                word = buf
                                buf = ""

                            End If
                            FieldCT = FieldCT + 1

                        Loop
                        ReDim gServiceTables(gSVCct).ColumnNames(FieldCT)
                        gServiceTables(gSVCct).cCT = 0

                        '##################################################################################################################
                        SQL = "Select * FROM [" & TName & "] ORDER BY LBS"  ' LBS for normal rate tables
                        '##################################################################################################################


                        Dim connectString = MakeConnectionString(dbPath, Password)
                        Dim cn As OleDbConnection = New OleDbConnection(connectString)

                        Try

                            cn.Open()

                        Catch ex As Exception

                            MsgBox("ATTENTION...IO_CacheServiceTables" & vbCrLf & vbCrLf & ex.Message, MsgBoxStyle.Exclamation)
                            Return 0
                            Exit Function

                        End Try
                        Try

                            Dim selectString As String = SQL
                            Dim cmd As OleDbCommand = New OleDbCommand(selectString, cn)
                            Dim reader As OleDbDataReader = cmd.ExecuteReader()

                            ct = 0
                            buf = ""

                            While (reader.Read())

                                ReDim gServiceTables(gSVCct).Rates(ct).Zones(FieldCT)
                                fct = 0
                                For i = 0 To reader.FieldCount - 1

                                    gServiceTables(gSVCct).Rates(ct).Zones(fct) = Val(reader(i).ToString())
                                    fct = fct + 1
                                    If ct = 0 Then

                                        gServiceTables(gSVCct).ColumnNames(gServiceTables(gSVCct).cCT) = reader.GetName(i).ToString
                                        gServiceTables(gSVCct).cCT = gServiceTables(gSVCct).cCT + 1

                                    End If

                                Next
                                ct = ct + 1

                            End While

                            reader.Close()
                            cn.Close()

                        Catch ex As Exception

                            MsgBox(ex.Message)

                        End Try

                    End If
                    gSVCct = gSVCct + 1

                Loop

            End If
            ServiceName = Dir()

        Loop


        Return 0

    End Function
    Public Function IO_CacheZoneTables() As Integer

        Dim ZoneName As String

        Dim SQL As String
        Dim Segment As String
        Dim SegmentSet As String
        Dim ct As Integer
        Dim TableCollection As String
        Dim dbPath As String
        Dim iloc As Integer
        Dim TName As String
        Dim Tally As Integer
        Dim Password As String
        Dim Index As Integer
        Dim buf As String
        buf = ""
        Password = ""
        Index = 0

        iloc = 0
        dbPath = ""
        ZoneName = ""
        SQL = ""
        Segment = ""
        SegmentSet = ""
        TableCollection = ""
        TName = ""
        gZct = 0
        ZoneName = Dir(gZoneTablesPath & "\*.*")
        Do Until ZoneName = ""

            dbPath = gZoneTablesPath & "\" & ZoneName
            TableCollection = IO_GetTableCollection(dbPath, "")
            Do Until TableCollection = ""

                iloc = InStr(1, TableCollection, ",")
                If Not iloc = 0 Then

                    TName = Trim(Mid(TableCollection, 0, iloc - 1))
                    TableCollection = Trim(Mid(TableCollection, iloc))

                Else

                    TName = TableCollection
                    TableCollection = ""

                End If
                SQL = "Select COUNT(*) As Tally FROM [" & TName & "]"
                SegmentSet = IO_GetSegmentSet(dbPath, SQL)
                Tally = CInt(Val(ExtractElementFromSegment("Tally", SegmentSet)))
                Tally = Tally + 1
                ReDim gZoneTables(gZct).Zones(Tally)
                gZoneTables(gZct).ZoneName = TName
                gZoneTables(gZct).ZoneCount = Tally

                If Not InStr(1, UCase(TName), "-INTP") = 0 Then

                    gZoneTables(gZct).International = True

                Else

                    gZoneTables(gZct).International = False

                End If
                If Tally > 300 Then

                    gZoneTables(gZct).dpPath = dbPath    ' Do not CACHE Large TABLES Such as the EAS or USPS-INTL-PMI-CANADA Use SQL to lookup
                    gZoneTables(gZct).UseDirectDBAccess = True

                Else

                    gZoneTables(gZct).UseDirectDBAccess = False

                    '#############################################################################################
                    'SQL = "Select LOZIP, HIZIP, ZONE, COUNTRY FROM [" & TName & "] ORDER BY ID"  ' ID field added to each zone table to make sure Access does not optimize the reading of records.  Must be read in order.
                    SQL = "Select * FROM [" & TName & "] ORDER BY ID"  ' ID field added to each zone table to make sure Access does not optimize the reading of records.  Must be read in order.
                    '#############################################################################################


                    Dim connectString = MakeConnectionString(dbPath, Password)
                    Dim cn As OleDbConnection = New OleDbConnection(connectString)

                    Try

                        cn.Open()

                    Catch ex As Exception

                        MsgBox("ATTENTION...IO_CacheZoneTables" & vbCrLf & vbCrLf & ex.Message, MsgBoxStyle.Exclamation)
                        Return 0
                        Exit Function

                    End Try
                    Try

                        Dim selectString As String = SQL
                        Dim cmd As OleDbCommand = New OleDbCommand(selectString, cn)
                        Dim reader As OleDbDataReader = cmd.ExecuteReader()

                        ct = 0

                        While (reader.Read())

                            If Not gZoneTables(gZct).International = True Then

                                gZoneTables(gZct).Zones(ct).Lo = CLng(Val(reader(1).ToString()))
                                gZoneTables(gZct).Zones(ct).Hi = CLng(Val(reader(2).ToString()))

                            Else

                                gZoneTables(gZct).Zones(ct).LoAlpha = reader(1).ToString()
                                gZoneTables(gZct).Zones(ct).HiAlpha = reader(2).ToString()

                            End If
                            gZoneTables(gZct).Zones(ct).Zone = reader(3).ToString()
                            ct = ct + 1

                        End While

                        reader.Close()
                        cn.Close()

                    Catch ex As Exception

                        MsgBox(ex.Message)

                    End Try

                End If
                gZct = gZct + 1

            Loop
            ZoneName = Dir()

        Loop
        Return 0

    End Function

    Public Function GetConnectionStrings(ByVal iniFilePath As String) As Integer

        Dim inbuf As String
        inbuf = ""
        gCS = 0

        If Not File.Exists(iniFilePath) And iniFilePath <> gIniPath Then

            iniFilePath = gIniPath

        End If
        If File.Exists(iniFilePath) Then

            Using sr As StreamReader = File.OpenText(iniFilePath)

                Do While UCase(inbuf) = "[CONNECTIONSTRINGS]" Or sr.Peek() >= 0

                    inbuf = sr.ReadLine()
                    If InStr(UCase(inbuf), "CONNECT") > 0 Then

                        gCS = gCS
                        Exit Do

                    End If

                Loop
                If Not UCase(inbuf) = "[CONNECTIONSTRINGS]" Then

                    sr.Close()
                    gCS = 0

                Else

                    inbuf = sr.ReadLine()
                    Do While sr.Peek() >= 0


                        inbuf = FlushOut(inbuf, "[", "")
                        inbuf = FlushOut(inbuf, "]", "")
                        gConnectionStrings(gCS).Name = inbuf
                        inbuf = sr.ReadLine()
                        gConnectionStrings(gCS).ConnectionString = inbuf
                        inbuf = sr.ReadLine()
                        gCS = gCS + 1
                        inbuf = sr.ReadLine()

                    Loop
                    sr.Close()

                End If

            End Using

        End If

        Return gCS

    End Function

    Public Function MakeInsertSQLFromSchema(TName As String, Segment As String, DbSchema As String, Optional IgnoreTestForID As Boolean = False, Optional isIncludeBlankElements As Boolean = False) As String

        Dim schema As String
        Dim SQL As String
        Dim iloc As Integer
        Dim eName As String
        Dim eValue As String
        Dim EType As Integer
        Dim buf As String
        Dim ID As String
        Dim eNames As String
        Dim eValues As String
        Dim UsingCloud As Boolean

        If gResult = "AZURE" Then

            UsingCloud = True

        Else

            UsingCloud = False

        End If

        ID = ExtractElementFromSegment("ID", Segment)
        If ID = "" And IgnoreTestForID = False Then

            MsgBox("ATTENTION...MakeInsertSQLFromSchema" & vbCrLf & vbCrLf & "Table [" & TName & "] Does Not Have a Primary Key...ID", vbCritical, gProgramName)
            Return ""
            Exit Function

        End If
        schema = DbSchema
        SQL = ""
        If IgnoreTestForID = False Then

            eNames = "[ID]"
            eValues = ID

        Else

            eNames = ""
            eValues = ""

        End If
        Do Until schema = ""

            eName = ""
            eValue = ""
            schema = ExtractNextElementFromSegment(eName, eValue, schema)
            buf = eName
            iloc = InStr(1, buf, ".")
            eName = Trim(Strings.Mid(buf, 1, iloc - 1))
            If eName = "ID" And IgnoreTestForID = False Then

                GoTo SkipIT

            End If
            EType = CInt(Trim(Strings.Mid(buf, iloc + 1)))
            eValue = ExtractElementFromSegment(eName, Segment)
            If (eValue = "" And Not isIncludeBlankElements) Then

                GoTo SkipIT

            ElseIf eValue = "" AndAlso Not IsElementInSegment(eName, Segment) Then

                GoTo SkipIT

            End If

            If Not InStr(1, eValue, "'") = 0 Then

                eValue = FlushOut(eValue, "'", "~")
                eValue = FlushOut(eValue, "~", "''")

            End If
            If EType = 5 And eValue = "" Then

                eValue = "0"

            End If

            If (eValue = "True" Or eValue = "False") And Not EType = 11 Then

                iloc = iloc

            End If
            'If Not eValue = "" Then
            If 1 = 1 Then

                If Not eNames = "" Then

                    eNames = eNames & ", "
                    eValues = eValues & ", "

                End If
                eNames = eNames & "[" & eName & "]"
                Select Case EType

                    Case 130            ' Text

                        eValues = eValues & "'" & eValue & "'"

                    Case 0, 2, 4, 5, 6, 3, 11              ' Single, Double, Long, Boolean

                        If IsNumeric(eValue) AndAlso Not InStr(1, eValue, ",") = 0 Then

                            eValue = FlushOut(eValue, ",", "")

                        End If
                        If eValue = "" Then

                            eValue = "0"

                        End If
                        If UsingCloud = True And (eValue = "True" Or eValue = "False") Then

                            eValues = eValues & "'" & eValue & "'"

                        Else

                            If UCase(eValue) = "FALSE" Then

                                iloc = iloc

                            End If
                            eValues = eValues & eValue

                        End If

                    Case 7              ' Date

                        If UsingCloud = False Then

                            eValues = eValues & "#" & eValue & "#"

                        Else

                            eValues = eValues & "'" & eValue & "'"

                        End If

                End Select

            End If

SkipIT:

        Loop

        SQL = "INSERT INTO " & TName & " (" & eNames & ") VALUES (" & eValues & ")"
        Return SQL

    End Function

    Public Function MakeUpdateSQLFromSchema(TName As String, Segment As String, DbSchema As String, Optional SkipChecks As Boolean = False, Optional isIncludeBlankElements As Boolean = False) As String

        Dim schema As String
        Dim SQL As String
        Dim iloc As Integer
        Dim eName As String
        Dim eValue As String
        Dim EType As String
        Dim EType_Int As Integer
        Dim ID As String
        Dim AcctNum As String
        eName = ""
        eValue = ""
        EType = ""
        AcctNum = ""
        ID = ""
        If SkipChecks = False Then

            ID = ExtractElementFromSegment("ID", Segment)
            If ID = "" And Not TName = "AR" Then

                MsgBox("ATTENTION...MakeUpdateSQLFromSchema" & vbCrLf & vbCrLf & "Table [" & TName & "] Does Not Have a Primary Key...ID", vbCritical, gProgramName)
                MakeUpdateSQLFromSchema = ""
                Exit Function

            End If
            If TName = "AR" Then

                AcctNum = ExtractElementFromSegment("AcctNum", Segment)

            End If

        End If
        schema = DbSchema
        SQL = ""
        Do Until schema = ""

            schema = ExtractNextElementFromSegment(eName, eValue, schema)
            iloc = InStr(1, eName, ".")
            EType = (Trim$(Strings.Mid$(eName, iloc + 1)))
            eName = Trim$(Strings.Mid$(eName, 1, iloc - 1))
            eValue = ExtractElementFromSegment(eName, Segment)
            If Not InStr(1, eValue, "'") = 0 Then

                eValue = FlushOut(eValue, "'", "~")
                eValue = FlushOut(eValue, "~", "''")

            End If

            If (eValue = "" And Not isIncludeBlankElements) Then

                GoTo SkipIT

            ElseIf eValue = "" AndAlso Not IsElementInSegment(eName, Segment) Then

                GoTo SkipIT

            End If

            If Not eName = "ID" Then

                EType_Int = CInt(EType)
                Select Case EType_Int

                    Case 130            ' Text

                        If Not SQL = "" Then

                            SQL = SQL & ", "

                        End If
                        SQL = SQL & "[" & eName & "]" & " = '" & eValue & "'"

                    Case 0, 2, 5, 6, 3, 11              ' Double, Long, Boolean

                        If eValue <> "" Then

                            If Not SQL = "" Then
                                SQL = SQL & ", "
                            End If

                            SQL = SQL & "[" & eName & "]" & " = " & eValue
                        End If


                    Case 7              ' Date

                        If eValue <> "" Then
                            If Not SQL = "" Then
                                SQL = SQL & ", "
                            End If

                            SQL = SQL & "[" & eName & "]" & " = #" & eValue & "#"
                        End If


                    Case Else

                        MsgBox("Here")

                End Select

            End If

SkipIT:

        Loop
        If Not TName = "AR" Then

            SQL = "UPDATE " & TName & " SET " & SQL & " WHERE ID = " & ID

        Else

            SQL = "UPDATE " & TName & " SET " & SQL & " WHERE AcctNum = '" & AcctNum & "'"

        End If
        MakeUpdateSQLFromSchema = SQL

    End Function

    Public Function IO_LoadListView(ByRef LV As ListView, ByVal DT As DataTable, ByVal dbPath As String, ByVal SQL As String, ColumnCT As Integer, Optional ByVal Password As String = "") As Integer

        'Dim LVI As ListViewItem
        Dim MyItems As String()

        LV.Items.Clear()
        ReDim MyItems(ColumnCT)
        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        Dim ct As Integer
        Dim i As Integer
        Dim index As Integer
        Dim ConnectionString As String
        Dim buf As String

        If InStr(dbPath, "$") = 0 And Not dbPath = "AZURE" Then

            Dim connectString As String = MakeConnectionString(dbPath, Password)
            Dim cn As OleDbConnection = New OleDbConnection(connectString)

            Try

                cn.Open()

            Catch ex As Exception

                MsgBox("ATTENTION...GetSegmentSet" & vbCrLf & vbCrLf & ex.Message, MsgBoxStyle.Exclamation)
                Return 0
                Exit Function

            End Try

            Dim selectString As String = SQL
            Dim cmd As OleDbCommand = New OleDbCommand(selectString, cn)
            Dim reader As OleDbDataReader = cmd.ExecuteReader()
            Dim dRow As DataRow

            ct = 0
            buf = ""

            While (reader.Read())

                i = 0
                dRow = DT.NewRow
                Do Until i = reader.FieldCount Or i = ColumnCT

                    MyItems(i) = reader(i).ToString()
                    If Not InStr(MyItems(i), "'") = 0 Then

                        MyItems(i) = ReplaceCharacters(MyItems(i), "'", "~")
                        MyItems(i) = ReplaceCharacters(MyItems(i), "~", "''")

                    End If

                    If dRow.Table.Columns.Item(i).DataType = GetType(String) Then
                        dRow(i) = MyItems(i)
                    ElseIf dRow.Table.Columns.Item(i).DataType = GetType(Date) Then
                        If MyItems(i) <> "" Then
                            dRow(i) = MyItems(i)
                        End If
                    ElseIf dRow.Table.Columns.Item(i).DataType = GetType(Boolean) Then
                        Boolean.TryParse(MyItems(i), dRow(i))
                    Else
                        dRow(i) = Val(MyItems(i))
                    End If

                    i += 1

                Loop

                DT.Rows.Add(dRow)
                ct += 1

            End While
            reader.Close()
            cn.Close()

            LV.DataContext = DT
            LV.SetBinding(ListView.ItemsSourceProperty, New Binding)

        Else

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
            Dim dRow As DataRow

            cmd.CommandText = SQL
            cmd.CommandType = CommandType.Text
            cmd.Connection = sqlConnection

            sqlConnection.Open()

            ct = 0
            reader = cmd.ExecuteReader()
            While (reader.Read())

                i = 0
                dRow = DT.NewRow
                Do Until i = reader.FieldCount Or i = ColumnCT

                    MyItems(i) = reader(i).ToString()
                    If Not InStr(MyItems(i), "'") = 0 Then

                        MyItems(i) = ReplaceCharacters(MyItems(i), "'", "~")
                        MyItems(i) = ReplaceCharacters(MyItems(i), "~", "''")

                    End If

                    dRow(i) = MyItems(i)
                    i = i + 1

                Loop

                DT.Rows.Add(dRow)
                ct = ct + 1

            End While

            reader.Close()
            sqlConnection.Close()
            LV.DataContext = DT
            LV.SetBinding(ListView.ItemsSourceProperty, New Binding)

        End If
        Return ct

    End Function

    Public Sub LV_Sort(ByVal ColumnHeader As String, ByVal direction As ListSortDirection, ByRef LV As ListView)
        'sorts Listview by Column Header

        Dim grid As GridView = LV.View

        'get column from clicked column header.
        Dim column As GridViewColumn = grid.Columns.Single(Function(c) c.Header.ToString() = ColumnHeader)

        'get binding field name from column
        Dim DataField As String = (TryCast(column.DisplayMemberBinding, Binding)).Path.Path

        'Setup Sorting
        Dim dataView As ICollectionView = CollectionViewSource.GetDefaultView(LV.ItemsSource)
        dataView.SortDescriptions.Clear()
        Dim sd As SortDescription = New SortDescription(DataField, direction)
        dataView.SortDescriptions.Add(sd)
        dataView.Refresh()
    End Sub

    Public Sub Sort_LV_byColumn(ByRef LV As ListView, ByRef columnHeader As GridViewColumnHeader, ByRef Last_Sort_Ascending As Boolean, ByRef Last_Column_Sorted As String)
        If IsNothing(columnHeader) Then Exit Sub

        If columnHeader.Content = Last_Column_Sorted Then
            If Last_Sort_Ascending = True Then
                LV_Sort(columnHeader.Content, ListSortDirection.Descending, LV)
                Last_Sort_Ascending = False
            Else
                LV_Sort(columnHeader.Content, ListSortDirection.Ascending, LV)
                Last_Sort_Ascending = True
            End If
        Else

            LV_Sort(columnHeader.Content, ListSortDirection.Ascending, LV)
            Last_Column_Sorted = columnHeader.Content
            Last_Sort_Ascending = True

        End If
    End Sub

    Public Function IO_UpdateSQLProcessor(ByVal DBPath As String, ByVal SQL As String, Optional dbConnectionString As String = "", Optional Silent As Boolean = False) As Long

        Dim ret As Long
        Dim ConnectionString As String
        Dim cmd As OleDbCommand
        Dim i As Integer
        Dim index As Integer
        Dim buf As String

        If InStr(DBPath, "$") = 0 And Not DBPath = "AZURE" Then

            ConnectionString = MakeConnectionString(DBPath, "")
            Dim cn As OleDbConnection = New OleDbConnection(ConnectionString)
            cn.Open()
            Try

                cmd = New OleDbCommand(SQL, cn)
                ret = cmd.ExecuteNonQuery()

            Catch ex As Exception

                buf = ex.Message
                If InStr(1, buf, "already exists in table") = 0 And Silent = False Then

                    MsgBox(ex.Message)

                End If
                ret = -1

            End Try
            cn.Close()

        Else

            If Not DBPath = "AZURE" Then

                i = InStr(DBPath, "$")
                index = CInt(Strings.Mid(DBPath, 1, i - 1))
                ConnectionString = gConnectionStrings(index).ConnectionString

            Else

                ConnectionString = gAzureConnectionString

            End If
            Using connection As New SqlConnection(ConnectionString)

                Dim command As New SqlCommand(SQL, connection)
                Try

                    command.Connection.Open()
                    ConnectionString = dbConnectionString
                    ret = command.ExecuteNonQuery()

                Catch ex As Exception

                    If Silent = False Then

                        MsgBox(ex.Message)

                    End If
                    ret = -1

                End Try

                connection.Close()
            End Using

        End If

        Return ret

    End Function

    Public Function IO_GetTableCollection(DBName As String, TableName As String, Optional dbConnectionString As String = "") As String

        Dim buf As String
        Dim TName As String
        Dim connectionString As String
        Dim Table As New DataTable
        Dim i As Integer
        Dim index As Integer

        If InStr(DBName, "laccdb") Then
            Return ""
        End If

        If InStr(DBName, "$") = 0 And Not DBName = "AZURE" Then

            connectionString = MakeConnectionString(DBName, "")
            Using connection As New OleDbConnection(connectionString)

                connection.ConnectionString = connectionString
                connection.Open()
                Table = connection.GetSchema("Tables")
                connection.Close()

            End Using

        Else

            If Not DBName = "AZURE" Then

                i = InStr(DBName, "$")
                index = CInt(Strings.Mid(DBName, 1, i - 1))
                connectionString = gConnectionStrings(index).ConnectionString

            Else

                connectionString = gAzureConnectionString

            End If
            Using connection As New SqlConnection(connectionString)

                connection.Open()
                Table = connection.GetSchema("Tables")

            End Using

        End If
        buf = ""
        For i = 0 To Table.Rows.Count - 1

            TName = Table.Rows(i)!TABLE_NAME.ToString
            If Not TName = "database_firewall_rules" And Not Strings.Mid(TName, 1, 4) = "MSys" Then

                If Not buf = "" Then

                    buf = buf & ", "

                End If
                If TableName = "" Then

                    buf = buf & TName

                Else

                    If TableName = TName Then

                        buf = TableName
                        Exit For

                    End If

                End If

            End If

        Next i
        Return buf
        'Server = tcp : shiprite.database.windows.net, 1433;Initial Catalog=ShipriteHYB;Persist Security Info=False;User ID=GFORD;Password=K#isha11;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;     Return buf

    End Function

    Public Function IO_GetTableIndexes(DBName As String, DoOnlyTable As String, Optional dbConnectionString As String = "") As String

        Dim buf As String
        Dim connectionString As String
        Dim Table As New DataTable
        Dim i As Integer
        Dim Segment As String
        Dim SegmentSet As String
        Dim index As Integer

        If InStr(DBName, "$") = 0 And Not DBName = "AZURE" Then

            connectionString = MakeConnectionString(DBName, "")
            Using connection As New OleDbConnection(connectionString)

                connection.ConnectionString = connectionString
                connection.Open()
                Table = connection.GetSchema("Indexes")
                connection.Close()

            End Using

        Else

            If Not DBName = "AZURE" Then

                i = InStr(DBName, "$")
                index = CInt(Strings.Mid(DBName, 1, i - 1))
                connectionString = gConnectionStrings(index).ConnectionString

            Else

                connectionString = gAzureConnectionString

            End If
            Using connection As New SqlConnection(connectionString)

                connection.Open()
                Table = connection.GetSchema("IndexColumns")

            End Using

        End If
        buf = ""
        Segment = ""
        SegmentSet = ""
        For i = 0 To Table.Rows.Count - 1

            If InStr(Table(i)!Table_Name.ToString, "MSys") = 0 Then

                Segment = ""
                Segment = AddElementToSegment(Segment, "TableName", Table(i)!Table_Name.ToString)
                Segment = AddElementToSegment(Segment, "ColumnName", Table(i)!Column_Name.ToString)
                Segment = AddElementToSegment(Segment, "IndexName", Table(i)!Index_Name.ToString)
                Segment = AddElementToSegment(Segment, "IndexType", Table(i)!Type.ToString)
                Segment = AddElementToSegment(Segment, "PrimaryKey", Table(i)!PRIMARY_KEY.ToString)
                Segment = AddElementToSegment(Segment, "Unique", Table(i)!UNIQUE.ToString)
                Segment = AddElementToSegment(Segment, "Clustered", Table(i)!CLUSTERED.ToString)
                If DoOnlyTable = "" Then

                    SegmentSet = SegmentSet & "<SET>" & Segment & "</SET>" & vbCrLf

                Else

                    If DoOnlyTable = Table(i)!Table_Name.ToString Then

                        SegmentSet = SegmentSet & "<SET>" & Segment & "</SET>" & vbCrLf

                    End If
                End If

            End If

        Next i

        Return SegmentSet

    End Function

    Public Function IO_GetFieldsCollection(DBName As String, TName As String, ReturnOnlyField As String, IncludeType As Boolean, IncludeSize As Boolean, ReturnAsBlankSegment As Boolean) As String

        Dim cnn As OleDbConnection = New OleDbConnection
        Dim ConnectionString As String
        Dim cbuf As String
        Dim cbuf2 As String
        Dim iloc As Integer
        Dim stopit As Boolean
        Dim Table As New DataTable
        Dim Rest() As String = {Nothing, Nothing, TName, Nothing}
        Dim columns As String = "Columns"
        Dim fsize As Long
        Dim i As Integer
        Dim index As Integer

        If InStr(DBName, "$") = 0 And Not DBName = "AZURE" Then

            ConnectionString = MakeConnectionString(DBName, "")
            Using connection As New OleDbConnection(ConnectionString)

                connection.ConnectionString = ConnectionString
                connection.Open()
                Table = connection.GetSchema("Columns", Rest)
                connection.Close()

            End Using

        Else

            If Not DBName = "AZURE" Then

                i = InStr(DBName, "$")
                index = CInt(Strings.Mid(DBName, 1, i - 1))
                ConnectionString = gConnectionStrings(index).ConnectionString

            Else

                ConnectionString = gAzureConnectionString

            End If
            Using connection As New SqlConnection(ConnectionString)

                connection.Open()
                Table = connection.GetSchema("Columns", Rest)

            End Using

        End If

        cbuf = ""
        For Each row As DataRow In Table.Rows

            If Not cbuf = "" Then

                cbuf = cbuf & ", "

            End If
            If ReturnOnlyField = "" Then

                cbuf = cbuf & row.Item("COLUMN_NAME").ToString
                If IncludeType = True Then

                    cbuf = cbuf & "." & row.Item("DATA_TYPE").ToString

                End If
                If IncludeSize = True Then

                    fsize = CLng(Val(row.Item("CHARACTER_MAXIMUM_LENGTH").ToString))
                    cbuf = cbuf & "@" & fsize

                End If

            Else

                If row.Item("COLUMN_NAME").ToString = ReturnOnlyField Then

                    cbuf = ReturnOnlyField
                    stopit = True
                    If IncludeType = True Then

                        cbuf = cbuf & "." & row.Item("DATA_TYPE").ToString

                    End If
                    If IncludeSize = True Then

                        fsize = CLng(Val(row.Item("CHARACTER_MAXIMUM_LENGTH").ToString))
                        cbuf = cbuf & "@" & fsize

                    End If

                    Exit For

                End If

            End If

        Next
        cnn.Close()
        iloc = InStr(1, cbuf, " ID,")
        If Strings.Mid$(cbuf, 1, 3) = "ID," Then

            iloc = 0

        End If
        If Not iloc = 0 And Not iloc = 1 Then

            cbuf2 = Trim$(Strings.Mid$(cbuf, iloc + 4))
            cbuf = Trim$(Strings.Mid$(cbuf, 1, iloc - 2))
            If Not cbuf2 = "" Then

                cbuf = cbuf & ", " & cbuf2

            End If
            cbuf = "ID, " & cbuf

        End If
        If ReturnAsBlankSegment = True Then

            cbuf = Trim(cbuf)
            If Not cbuf = "" Then

                cbuf = FlushOut(cbuf, ", ", "  " & Chr(187) & Chr(171))
                cbuf = Chr(171) & cbuf & "  " & Chr(187)

            End If

        End If
        Return cbuf

    End Function

    Public Function IO_GetSegmentSet(ByVal dbPath As String, ByVal SQL As String, Optional ByVal Password As String = "", Optional dbConnectionString As String = "") As String

        Dim ct As Integer
        Dim i As Integer
        Dim ConnectionString As String
        Dim buf As String
        Dim index As Integer

        buf = ""
        If InStr(dbPath, "$") = 0 And Not dbPath = "AZURE" Then

            Dim connectString = MakeConnectionString(dbPath, Password)
            Dim cn As OleDbConnection = New OleDbConnection(connectString)

            Try

                cn.Open()

            Catch ex As Exception

                MsgBox("ATTENTION...GetSegmentSet" & vbCrLf & vbCrLf & ex.Message, MsgBoxStyle.Exclamation)
                IO_GetSegmentSet = ""
                Exit Function

            End Try
            Try

                Dim selectString As String = SQL
                Dim cmd As OleDbCommand = New OleDbCommand(selectString, cn)
                Dim reader As OleDbDataReader = cmd.ExecuteReader()

                ct = 0
                buf = ""

                While (reader.Read())

                    buf &= "<SET>"
                    For i = 0 To reader.FieldCount - 1

                        buf &= Chr(171)
                        buf &= reader.GetName(i).ToString
                        buf &= " "
                        buf &= reader(i).ToString()
                        buf &= Chr(187)

                    Next
                    buf &= "</SET>"
                    buf &= vbCrLf
                    ct += 1

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

            While (reader.Read())

                buf &= "<SET>"
                For i = 0 To reader.FieldCount - 1

                    buf &= Chr(171)
                    buf &= reader.GetName(i).ToString
                    buf &= " "
                    buf &= Trim(reader(i).ToString())
                    buf &= Chr(187)

                Next
                buf &= "</SET>"
                buf &= vbCrLf
                ct += 1

            End While

            reader.Close()

            sqlConnection.Close()

        End If

        IO_GetSegmentSet = buf.ToString

    End Function

    Public Function MakeConnectionString(Path As String, ByVal Password As String) As String

        Dim Provider As String

        If Not InStr(1, UCase(Path), "MDB") = 0 Then

            Provider = "Provider=Microsoft.Jet.OLEDB.4.0; Data Source="

        Else

            Provider = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source ="

        End If

        If Not Password = "" Then

            Provider = Provider & ";Jet OLEDB:Database password=" & Password & ";"

        End If

        Return Provider & Path

    End Function


    Public Sub Get_ChildControls_Of_Grid(ByRef Current_Grid As Grid, ByRef List_OBJ As List(Of Object))
        'Gets all Contorls on a Grid that have a tag associated with it. The controls are added into a list which can be sorted through. 

        For Each obj As Object In Current_Grid.Children
            If obj.tag <> "" Then
                List_OBJ.Add(obj)
            End If
        Next
    End Sub

    Public Sub Display_DBData_To_UI(ByRef List_OBJ As List(Of Object), ByRef current_segment As String)
        'reads in data from the segment and displays in the UI objects

        Dim fieldName As String = ""
        Dim fieldValue As String = ""
        Dim index As Integer


        Do Until current_segment = ""
            current_segment = ExtractNextElementFromSegment(fieldName, fieldValue, current_segment)
            If fieldValue Is Nothing Then fieldValue = ""

            index = List_OBJ.FindIndex(Function(x As Object) x.tag = fieldName)

            If index <> -1 Then
                If List_OBJ(index).GetType Is GetType(CheckBox) Then
                    List_OBJ(index).isChecked = fieldValue
                Else
                    List_OBJ(index).text = fieldValue
                End If
            End If
        Loop
    End Sub


    ''' <summary>
    '''     Gets an empty DataTable matching the columns in table tableName from database at dbPath
    ''' </summary>
    ''' <param name="dbPath"></param>
    ''' <param name="tableName"></param>
    ''' <param name="password"></param>
    ''' <returns></returns>
    Public Function GetEmptyDataTable(ByVal dbPath As String, tableName As String, Optional password As String = "", Optional newTableName As String = "") As DataTable
        Dim schema As New DataTable(newTableName)

        If InStr(dbPath, "$") = 0 And dbPath <> "AZURE" Then
            Dim connectionString = MakeConnectionString(dbPath, password)
            Dim connection As OleDbConnection = New OleDbConnection(connectionString)
            Try
                connection.Open()
            Catch ex As Exception
                Debug.Print("ATTENTION! Problem connecting to database to generate datatable.\n" & ex.Message & vbCrLf & ex.StackTrace, MsgBoxStyle.Exclamation)
                Return schema
            End Try

            schema = connection.GetSchema("Columns", {Nothing, Nothing, tableName, Nothing})
            connection.Close()
        Else
            Dim connectionString As String
            If dbPath <> "AZURE" Then
                Dim loc = InStr(dbPath, "$")
                Dim index = CInt(Strings.Mid(dbPath, 1, loc - 1))
                connectionString = gConnectionStrings(index).ConnectionString
            Else
                connectionString = gAzureConnectionString
            End If
            Dim connection = New SqlConnection(connectionString)
            connection.Open()
            schema = connection.GetSchema("Columns", {Nothing, Nothing, tableName, Nothing})
            connection.Close()
        End If

        Dim dTable As DataTable
        If newTableName <> "" Then
            dTable = New DataTable(newTableName)
        Else
            dTable = New DataTable(tableName)
        End If
        For Each row As DataRow In schema.Rows
            dTable.Columns.Add(row.Field(Of String)("COLUMN_NAME"))
        Next
        Return dTable
    End Function

    'Public Function UpdateBytesToDb(DBPath As String, Data As Byte(), table As String, col As String, match As String) As Boolean
    '    Dim cmd As OleDbCommand = New OleDbCommand()
    '    cmd.CommandType = CommandType.Text
    '    Dim cn As OleDbConnection = New OleDbConnection(MakeConnectionString(DBPath, ""))
    '    cn.Open()
    '    cmd.Connection = cn
    '    ' Example data:
    '    '  table  Contacts
    '    '  col    ProfileImage
    '    '  match  ID=41409
    '    cmd.CommandText = "UPDATE " & table & " SET " & col & " = @p1 WHERE " & match
    '    cmd.Parameters.AddWithValue("@p1", Data)
    '    Try
    '        cmd.ExecuteNonQuery()
    '        cn.Close()
    '        Return True
    '    Catch ex As Exception
    '        cn.Close()
    '        Debug.WriteLine(ex.ToString)
    '        Return False
    '    End Try
    'End Function
    'Public Function GetBytesFromDb(DBPath As String, table As String, col As String, match As String) As Byte()
    '    Dim cmd As OleDbCommand = New OleDbCommand()
    '    cmd.CommandType = CommandType.Text
    '    Dim cn As OleDbConnection = New OleDbConnection(MakeConnectionString(DBPath, ""))
    '    cn.Open()
    '    cmd.Connection = cn
    '    ' Example data:
    '    '  table  Contacts
    '    '  col    ProfileImage
    '    '  match  ID=41409
    '    cmd.CommandText = "SELECT " & col & " FROM " & table & " WHERE " & match
    '    Dim data As Byte() = Nothing
    '    Try
    '        data = cmd.ExecuteScalar()
    '    Catch ex As Exception
    '        Debug.WriteLine(ex.ToString)
    '    End Try
    '    cn.Close()
    '    Return data
    'End Function

End Module
