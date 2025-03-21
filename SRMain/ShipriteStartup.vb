Imports System.IO
Imports wgssSTU
Imports Microsoft.Win32
Imports Microsoft.Office.Interop.Access.Dao

Module ShipriteStartup

    Public Function ProcessIndexValidation() As Integer

        Dim ret As Integer
        Dim SQL As String

        'Payments Table

        SQL = "CREATE UNIQUE INDEX IDX_ID ON Payments(ID)"
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL,, True)

        SQL = "CREATE INDEX IDX_ID ON Payments(ID)"
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL,, True)

        SQL = "CREATE INDEX IDX_NumericInvoiceNumber ON Payments(NumericInvoiceNumber)"
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL,, True)

        SQL = "CREATE INDEX IDX_InvNum ON Payments(InvNum)"
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL,, True)

        SQL = "CREATE INDEX IDX_AcctNum ON Payments(AcctNum)"
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL,, True)

        SQL = "CREATE INDEX IDX_AcctName ON Payments(AcctName)"
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL,, True)

        SQL = "CREATE INDEX IDX_Date ON Payments([Date])"
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL,, True)

        SQL = "CREATE INDEX IDX_Type ON Payments([Type])"
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL,, True)

        SQL = "CREATE INDEX IDX_Status ON Payments([Status])"
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL,, True)

        SQL = "CREATE INDEX IDX_DrawerID ON Payments(DrawerID)"
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL,, True)

        SQL = "CREATE INDEX IDX_CloseID ON Payments(CloseID)"
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL,, True)

        SQL = "CREATE INDEX IDX_qCloseID ON Payments(qCloseID)"
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL,, True)

        SQL = "CREATE INDEX IDX_SoldTo ON Payments(SoldTo)"
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL,, True)


        'Transactions Table

        SQL = "CREATE UNIQUE INDEX IDX_ID ON Transactions(ID)"
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL,, True)

        SQL = "CREATE INDEX IDX_ID ON Transactions(ID)"
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL,, True)

        SQL = "CREATE INDEX IDX_NumericInvoiceNumber ON Transactions(NumericInvoiceNumber)"
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL,, True)

        SQL = "CREATE INDEX IDX_InvNum ON Transactions(InvNum)"
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL,, True)

        SQL = "CREATE INDEX IDX_AcctNum ON Transactions(AcctNum)"
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL,, True)

        SQL = "CREATE INDEX IDX_AcctName ON Transactions(AcctName)"
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL,, True)

        SQL = "CREATE INDEX IDX_Date ON Transactions([Date])"
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL,, True)

        SQL = "CREATE INDEX IDX_Status ON Transactions([Status])"
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL,, True)

        SQL = "CREATE INDEX IDX_DrawerID ON Transactions(DrawerID)"
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL,, True)

        SQL = "CREATE INDEX IDX_CloseID ON Transactions(CloseID)"
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL,, True)

        SQL = "CREATE INDEX IDX_qCloseID ON Transactions(qCloseID)"
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL,, True)

        SQL = "CREATE INDEX IDX_SoldTo ON Transactions(SoldTo)"
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL,, True)

        SQL = "CREATE INDEX IDX_SKU ON Transactions(SKU)"
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL,, True)

        SQL = "CREATE INDEX IDX_ModelNumber ON Transactions(ModelNumber)"
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL,, True)

        SQL = "CREATE INDEX IDX_MailboxNumber ON Transactions(MailboxNumber)"
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL,, True)

        ' Contacts

        SQL = "CREATE UNIQUE INDEX IDX_ID ON Contacts(ID)"
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL,, True)

        SQL = "CREATE INDEX IDX_Name ON Contacts([Name])"
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL,, True)

        SQL = "CREATE INDEX IDX_LName ON Contacts([LName])"
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL,, True)

        SQL = "CREATE INDEX IDX_LName ON Contacts([FName])"
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL,, True)

        SQL = "CREATE INDEX IDX_LName ON Contacts([FName])"
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL,, True)

        SQL = "CREATE INDEX IDX_Phone ON Contacts([Phone])"
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL,, True)


        ' AR

        SQL = "CREATE UNIQUE INDEX IDX_ID ON Contacts(ID)"
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL,, True)

        SQL = "CREATE IDX_AcctNum ON Contacts([Phone])"
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL,, True)

        SQL = "CREATE IDX_AcctName ON Contacts([AcctName])"
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL,, True)

        SQL = "CREATE IDX_AcctNum ON Contacts([AcctNum])"
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL,, True)

        ' Manifest

        SQL = "CREATE UNIQUE INDEX IDX_ID ON Manifest(ID)"
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL,, True)

        SQL = "CREATE IDX_Date ON Contacts([Date])"
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL,, True)

        SQL = "CREATE IDX_TrackingNumber ON Contacts([Tracking#])"
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL,, True)

        SQL = "CREATE IDX_PackageID ON Contacts([PackageID])"
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL,, True)

        SQL = "CREATE IDX_CID ON Contacts([CID])"
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL,, True)

        SQL = "CREATE IDX_SID ON Contacts([SID])"
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL,, True)

        SQL = "CREATE IDX_PickupDate ON Contacts([PickupDate])"
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL,, True)

        ' Mailbox

        SQL = "CREATE UNIQUE INDEX IDX_ID ON MailBox(ID)"
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL,, True)

        SQL = "CREATE IDX_MailboxNumber ON MailBox([MailboxNumber])"
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL,, True)

        SQL = "CREATE IDX_StartDate ON MailBox([StartDate])"
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL,, True)

        SQL = "CREATE IDX_EndDate ON MailBox([EndDate])"
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL,, True)

        SQL = "CREATE IDX_Size ON MailBox([Size])"
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL,, True)

        SQL = "CREATE IDX_CID ON MailBox([CID])"
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL,, True)

        Return 0

    End Function

    Public Function CacheProfitRanges() As Integer

        gProfitRange(0).LO = Val(GetPolicyData(gShipriteDB, "Level1L")) ' get values of strings to set to Double variables
        gProfitRange(0).HI = Val(GetPolicyData(gShipriteDB, "Level1H"))
        gProfitRange(1).LO = Val(GetPolicyData(gShipriteDB, "Level2L"))
        gProfitRange(1).HI = Val(GetPolicyData(gShipriteDB, "Level2H"))
        gProfitRange(2).LO = Val(GetPolicyData(gShipriteDB, "Level3L"))
        gProfitRange(2).HI = Val(GetPolicyData(gShipriteDB, "Level3H"))

        Return 0

    End Function

    Public Sub Cache_FASC_ASO_PN_DiscountLevels()

        If _IDs.IsIt_PostNetStore Then
            gFedEx_Discount_Segment = IO_GetSegmentSet(gFedExRetailServicesDB, "SELECT * From [DiscountsFASC-Corporate]")
            gUPS_Discount_Segment = IO_GetSegmentSet(gUPSRetailServicesDB, "SELECT * From [PN_Incentive]")
        Else
            gFedEx_Discount_Segment = IO_GetSegmentSet(gFedExRetailServicesDB, "SELECT * From DiscountsFASC")
            gUPS_Discount_Segment = IO_GetSegmentSet(gUPSRetailServicesDB, "SELECT * From ASO_Incentive")
        End If

    End Sub

    Public Sub Cache_PricingMatrix()
        Try
            Dim SegmentSet As String
            Dim Segment As String
            Dim PM_Item As PricingMatrixItem

            SegmentSet = IO_GetSegmentSet(gPricingMatrixDB, "SELECT * From [PricingMatrix]")
            gPricingMatrix = New List(Of PricingMatrixItem)

            Do Until SegmentSet = ""
                Segment = GetNextSegmentFromSet(SegmentSet)
                PM_Item = New PricingMatrixItem

                PM_Item.ID = ExtractElementFromSegment("ID", Segment)
                PM_Item.Carrier = ExtractElementFromSegment("Carrier", Segment)
                PM_Item.Service = ExtractElementFromSegment("Service", Segment)
                PM_Item.WeightStart = ExtractElementFromSegment("Weight_Start", Segment)
                PM_Item.WeightEnd = ExtractElementFromSegment("Weight_End", Segment)
                PM_Item.Markup = ExtractElementFromSegment("Markup", Segment)
                PM_Item.Zone = ExtractElementFromSegment("Zone", Segment)

                gPricingMatrix.Add(PM_Item)

            Loop

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Caching Pricing Matrix")
        End Try
    End Sub

    Public Function CacheZoneTables() As Integer
        Try
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

            SpeeDee.CheckSpeeDee_Zones()

            iloc = 0
            dbPath = ""
            ZoneName = ""
            SQL = ""
            Segment = ""
            SegmentSet = ""
            TableCollection = ""
            TName = ""
            gZct = 0
            ZoneName = Dir(gZoneTablesPath & "\*.accdb")
            Do Until ZoneName = ""

                If File.Exists(gZoneTablesPath & "\custom_rates\" & ZoneName) Then
                    dbPath = gZoneTablesPath & "\custom_rates\" & ZoneName
                Else
                    dbPath = gZoneTablesPath & "\" & ZoneName
                End If

                If File.Exists(dbPath) Then

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
                        Tally = Val(ExtractElementFromSegment("Tally", SegmentSet))
                        ReDim gZoneTables(gZct).Zones(Tally)
                        gZoneTables(gZct).ZoneName = TName
                        gZoneTables(gZct).ZoneCount = Tally

                        If Not InStr(1, UCase(TName), "INT") = 0 Then

                            gZoneTables(gZct).International = True

                        Else

                            gZoneTables(gZct).International = False

                        End If
                        If Tally > 720 Then '300

                            gZoneTables(gZct).dpPath = dbPath    ' Do not CACHE Large TABLES Such as the EAS or USPS-INTL-PMI-CANADA Use SQL to lookup
                            gZoneTables(gZct).UseDirectDBAccess = True

                        Else

                            gZoneTables(gZct).UseDirectDBAccess = False

                            SQL = "Select * FROM [" & TName & "] ORDER BY ID"  ' ID field added to each zone table to make sure Access does not optimize the reading of records.  Must be read in order.
                            SegmentSet = IO_GetSegmentSet(dbPath, SQL)
                            ct = 0
                            Do Until SegmentSet = ""

                                Segment = GetNextSegmentFromSet(SegmentSet)
                                If IsNumeric(ExtractElementFromSegment("LOZIP", Segment)) Then

                                    gZoneTables(gZct).Zones(ct).Lo = Val(ExtractElementFromSegment("LOZIP", Segment))
                                    gZoneTables(gZct).Zones(ct).Hi = Val(ExtractElementFromSegment("HIZIP", Segment))

                                Else

                                    gZoneTables(gZct).Zones(ct).LoAlpha = ExtractElementFromSegment("LOZIP", Segment)
                                    gZoneTables(gZct).Zones(ct).HiAlpha = ExtractElementFromSegment("HIZIP", Segment)

                                End If
                                gZoneTables(gZct).Zones(ct).Zone = ExtractElementFromSegment("ZONE", Segment)
                                gZoneTables(gZct).Zones(ct).Country = ExtractElementFromSegment("COUNTRY", Segment)
                                gZoneTables(gZct).Zones(ct).Segment = Segment
                                ct += 1

                            Loop

                        End If
                        gZct += 1

                    Loop

                End If
                ZoneName = Dir()

            Loop


            ' Cache USPS Domestic Zone Matrix
            _USPS.USPS_IsZoneMatrixLoaded = USPS_DomesticZoneChart_Load(gStoreZip)


            If Not _IDs.IsIt_HawaiiShipper Then
                'Zone charts for Hawaii shippers are in the UPS_Zones.accdb and FedEx_Zones.accdb

                Dim fedexDomesticZones As Domestic_Zones = Cache_FedEx_DomesticZones(gStoreZip)
                Copy_DomesticZones_To_gZoneTable(fedexDomesticZones)

                If Not _IDs.IsIt_PuertoRicoShipper Then
                    Dim upsDomesticZones As Domestic_Zones = Cache_UPS_DomesticZones(gStoreZip)
                    Copy_DomesticZones_To_gZoneTable(upsDomesticZones)
                End If


            End If

            Return 0

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Caching Zone Tables")
        End Try

        Return 0

    End Function

    Public Function CacheServiceTables() As Integer
        '
        Return DatabaseFunctions.IO_CacheServiceTables()
        '
    End Function

    Public Function CreateTableIfNecessary(TName As String) As Integer
        Try
            Dim SQL As String
            Dim Segment As String
            Dim ret As Integer

            Segment = IO_GetTableCollection(gShipriteDB, TName, "")
            If Segment = "" Then

                Try

                    If InStr(1, gShipriteDB, "$") = 0 Then

                        SQL = "CREATE TABLE " & TName & " (ID LONG)"

                    Else

                        SQL = "CREATE TABLE " & TName & " (ID BIGINT)"

                    End If
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                    SQL = "CREATE UNIQUE INDEX IDNumber ON " & TName & " (ID)"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                    ' Commented out for SRN-183
                    'MsgBox("ATTENTION" & vbCrLf & vbCrLf & TName & " Table CREATED.", vbInformation, gProgramName)

                Catch ex As Exception

                    _MsgBox.ErrorMessage(ex, "Error Creating Table")

                End Try

            End If
        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Creating Table")
        End Try

        Return 0

    End Function

    Private Sub Check_Holiday_Txt_File()
        Try
            Dim SQL As String
            Dim FilePath As String = gDBpath & "\Holiday.txt"
            Dim ApplicableDate As Date
            Dim Carriers As String
            Dim HolidayName As String
            Dim isProcessed As Boolean = False
            Dim fileVersion As String = ""
            Dim fileVersionDbField As String = "HolidayFileVersion"

            If Not File.Exists(FilePath) Then
                Exit Sub
            End If

            Using MyReader As New Microsoft.VisualBasic.
                      FileIO.TextFieldParser(FilePath)
                MyReader.TextFieldType = FileIO.FieldType.Delimited
                MyReader.SetDelimiters(",")
                Dim currentRow As String()
                Dim isFirstLine As Boolean = True
                While Not MyReader.EndOfData
                    Try
                        currentRow = MyReader.ReadFields()

                        If isFirstLine Then
                            ' get first line
                            fileVersion = currentRow(1)

                            Dim savedFileVersion As String = GetPolicyData(gShipriteDB, fileVersionDbField, "")

                            If String.IsNullOrWhiteSpace(savedFileVersion) Then
                                SQL = "DELETE FROM Holiday"
                                IO_UpdateSQLProcessor(gShipriteDB, SQL)
                            End If

                            ' check version
                            If fileVersion > savedFileVersion Then
                                ' new version, process
                            Else
                                ' existing version, don't process
                                Exit While
                            End If

                            isFirstLine = False
                        Else
                            isProcessed = True ' assume if reached here
                            ApplicableDate = currentRow(1)
                            Carriers = currentRow(2)
                            HolidayName = currentRow(3)

                            SQL = "INSERT INTO Holiday (NormalDate, AppliesTo, Description) VALUES ('" & ApplicableDate & "', '" & Carriers & "', '" & Replace(HolidayName, "'", "''") & "')"
                            IO_UpdateSQLProcessor(gShipriteDB, SQL)
                        End If

                    Catch ex As Microsoft.VisualBasic.FileIO.MalformedLineException
                        MsgBox("Line " & ex.Message & "is not valid and will be skipped.")
                    End Try
                End While
            End Using

            If isProcessed Then
                UpdatePolicy(gShipriteDB, fileVersionDbField, fileVersion)
            End If
            My.Computer.FileSystem.DeleteFile(FilePath)

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Checking Holiday.txt file")
        End Try

    End Sub

    Public Function CheckForDatabaseUpdate() As Integer

        Dim buf As String
        Dim FieldsVersion As Date
        Dim iloc As Integer
        Dim ret As Integer

        ' Add New Tables Before Processing Fields.UPD
        ' Works in conjunction with the fields.upd.  Create Tables here.  Then process the DB Updates.  Just add the CreateTableIfNecessary function when adding tables.
        ret = CreateTableIfNecessary("InvoiceNotes")
        ret = CreateTableIfNecessary("GiftRegistry")

        Check_Holiday_Txt_File()
        ShipRiteReports.ReportsODBC.ShipRiteReports_SetODBC()

        buf = Dir(gDBpath & "\fields.upd")
        If Not buf = "" Then

            buf = GetPolicyData(gShipriteDB, "FieldsVersion")
            If Not buf = "" Then

                FieldsVersion = buf

            Else

                FieldsVersion = "01/01/1901"

            End If
            buf = FileDateTime(gDBpath & "\fields.upd")
            iloc = InStr(1, buf, " ")
            buf = Mid(buf, 0, iloc - 1)
            If Convert.ToDateTime(buf) > FieldsVersion Then

                ret = ret + DataBase_Update(gDBpath & "\fields.upd")
                ret = ret + UpdatePolicy(gShipriteDB, "FieldsVersion", buf)

            End If

        End If

        buf = Dir(gAppPath & "\rfields.upd")
        If Not buf = "" Then

            buf = GetPolicyData(gReportsDB, "FieldsVersion")
            If Not buf = "" Then

                FieldsVersion = buf

            Else

                FieldsVersion = "01/01/1901"

            End If
            buf = FileDateTime(gAppPath & "\rfields.upd")
            iloc = InStr(1, buf, " ")
            buf = Mid(buf, 1, iloc - 1)
            If Convert.ToDateTime(buf) > FieldsVersion Then

                ret = ret + DataBase_Update(gAppPath & "\rfields.upd", False, True)
                ret = ret + UpdatePolicy(gReportsDB, "FieldsVersion", buf)

                On Error Resume Next
                FileCopy(gAppPath & "\rfields.upd", gDBpath & "\rfields.upd")
                On Error GoTo 0

            End If

        End If

        If CleanOldTransactionData() Then
            Compact_RepairDB()
        End If

        Return 0


    End Function

    Public Function CleanOldTransactionData() As Boolean
        ' Below is the code for deleting old note transactions every week
        Dim buf As String
        Dim ret As Integer
        Dim LastClearDate As Date
        Dim SQL As String
        Dim Today As Date = Date.Today
        Dim WeekBefore As Date = Today.AddDays(-7)
        Dim Old As Date = Today.AddYears(-1)

        buf = GetPolicyData(gShipriteDB, "LastClearDate")
        If Not buf = "" Then
            LastClearDate = buf
        Else
            LastClearDate = New Date(1, 1, 1)
        End If

        If LastClearDate <= WeekBefore Then

            SQL = "DELETE FROM Transactions WHERE SKU = 'NOTE' AND [Date] < #" & Old.ToString("MM/dd/yyyy") & "#"
            ret = IO_UpdateSQLProcessor(gShipriteDB, SQL,, True)
            UpdatePolicy(gShipriteDB, "LastClearDate", Today.ToString("MM/dd/yyyy"))
            Return ret > 0

        End If

        Return False

    End Function

    Public Function Compact_RepairDB() As Boolean
        ' Below is the code for compacting/repairing the database and making a backup DB (ShipriteNext_backup.accdb) in case there are errors
        Dim DBE As New DBEngine()
        Dim src As String = gShipriteDB
        Dim dest As String = gDBpath & "\ShipriteNext_compact.accdb"
        Dim src2 As String = gDBpath & "\ShipriteNext_backup.accdb"

        Try

            If _Files.IsFileExist(src, True) Then

                If _Files.CopyFile_ToNewFolder(src, src2, True) Then

                    DBE.CompactDatabase(src, dest)

                    ' compact success - new file created
                    If _Files.IsFileExist(dest, True) Then
                        _Files.Delete_File(src, False)
                        Return _Files.MoveFile_ToNewFolder(dest, src, True)
                    End If

                End If

            End If

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Failed to Compact Database:" & Environment.NewLine & src)
        End Try

        Return False

    End Function

    Private Sub Check_If_DB_Present()
        If Not IsFileExist(gShipriteDB, False) Then
            MsgBox("Cannot Find ShipRite Database: " & gShipriteDB, vbExclamation + vbOKOnly)
            If InStr(1, gShipriteDB, "$") = 0 Then

                Environment.Exit(0)

            End If

        End If
    End Sub

    Private Sub Copy_UPS_CSVZoneFile_From_Old_ShipRite()
        Try
            Dim OldShipRite_DBPath As String = GetWinINI("ShipRite9", "c:\Windows\Shiprite.ini", "c:\Shiprite", "DataPath")
            Dim StoreZip As String = ExtractElementFromSegment("Zip", IO_GetSegmentSet(OldShipRite_DBPath & "\shiprite.mdb", "SELECT Zip From Setup"))

            Dim CSV_FileName = StoreZip.Substring(0, 3).PadLeft(3, "0") & ".csv"

            Dim Old_CSV_File As String = Path.Combine(OldShipRite_DBPath, CSV_FileName)
            Dim New_CSV_File As String = Path.Combine(gZoneTablesPath, "UPS", CSV_FileName) 'gAppPath & "\ZoneTables\UPS\" & CSV_FileName

            If File.Exists(Old_CSV_File) Then
                File.Copy(Old_CSV_File, New_CSV_File, True)
            End If

        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub

    Private Sub Copy_MDB_From_Old_ShipRite(ByVal MDB_FileName As String)
        Try
            Dim OldShipRite_DBPath As String = GetWinINI("ShipRite9", "c:\Windows\Shiprite.ini", "c:\Shiprite", "DataPath")
            Dim MDB_Ext As String = ".mdb"
            If Not String.IsNullOrWhiteSpace(MDB_FileName) Then
                If _Files.Get_FileExtension(MDB_FileName) <> MDB_Ext Then
                    MDB_FileName &= MDB_Ext
                End If
            End If

            Dim Old_MDB_File As String = Path.Combine(OldShipRite_DBPath, MDB_FileName)
            Dim New_MDB_File As String = Path.Combine(gDBpath, MDB_FileName)

            If File.Exists(Old_MDB_File) Then
                File.Copy(Old_MDB_File, New_MDB_File, True)
            End If

        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub

    Private Sub Copy_DropOffDisclaimertxt_From_Old_ShipRite()
        Try
            Dim OldShipRite_DBPath As String = GetWinINI("ShipRite9", "c:\Windows\Shiprite.ini", "c:\Shiprite", "DataPath")
            Dim TXT_FileName = "DropOff_Disclaimer.txt"

            Dim Old_TXT_File As String = Path.Combine(OldShipRite_DBPath, TXT_FileName)
            Dim New_TXT_File As String = Path.Combine(gTemplatesPath, TXT_FileName)

            If File.Exists(Old_TXT_File) Then
                File.Copy(Old_TXT_File, New_TXT_File, True)
            End If

        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub
    Public Sub Import_RTF_Templates(notifDBPath As String)
        _EmailSetup.set_Paths(notifDBPath)
        ' New Mail in mbox email
        Dim segment = String.Empty
        Dim subject As String
        Dim content As String

        ' Mail in mailbox (Email)
        EmailNotificationsDb.Read_Notification(_EmailSetup.file_YouHaveALetterInMbox, segment)
        subject = ExtractElementFromSegment("EmailSubject", segment, "You have mail in your Mailbox")
        subject = subject.Replace("'", "''")
        content = _Convert.StringToBase64(File.ReadAllText(_EmailSetup.NotificationsFolder & _EmailSetup.file_YouHaveALetterInMbox))

        UpdatePolicy(gShipriteDB, "Notify_Email-NewMailinMailboxSubject", subject)
        UpdatePolicy(gShipriteDB, "Notify_Email-NewMailinMailboxContent", content)

        ' Mail in mailbox (SMS)
        EmailNotificationsDb.Read_Notification(_EmailSetup.file_YouHaveALetterInMbox_SMS, segment)
        subject = ExtractElementFromSegment("EmailSubject", segment, "Check your mailbox")
        subject = subject.Replace("'", "''")
        content = _Convert.StringToBase64(File.ReadAllText(_EmailSetup.NotificationsFolder & _EmailSetup.file_YouHaveALetterInMbox_SMS))

        UpdatePolicy(gShipriteDB, "Notify_SMS-NewMailinMailboxSubject", subject)
        UpdatePolicy(gShipriteDB, "Notify_SMS-NewMailinMailboxContent", content)

        ' Package in mailbox (email)
        EmailNotificationsDb.Read_Notification(_EmailSetup.file_YouHaveAPackageInMbox, segment)
        subject = ExtractElementFromSegment("EmailSubject", segment, "You have a package in your Mailbox")
        subject = subject.Replace("'", "''")
        content = _Convert.StringToBase64(File.ReadAllText(_EmailSetup.NotificationsFolder & _EmailSetup.file_YouHaveAPackageInMbox))

        UpdatePolicy(gShipriteDB, "Notify_Email-NewPackageinMailboxSubject", subject)
        UpdatePolicy(gShipriteDB, "Notify_Email-NewPackageinMailboxContent", content)

        ' Package in mailbox (SMS)
        EmailNotificationsDb.Read_Notification(_EmailSetup.file_YouHaveAPackageInMbox_SMS, segment)
        subject = ExtractElementFromSegment("EmailSubject", segment, "You have a package in your Mailbox")
        subject = subject.Replace("'", "''")
        content = _Convert.StringToBase64(File.ReadAllText(_EmailSetup.NotificationsFolder & _EmailSetup.file_YouHaveAPackageInMbox_SMS))

        UpdatePolicy(gShipriteDB, "Notify_SMS-NewPackageinMailboxSubject", subject)
        UpdatePolicy(gShipriteDB, "Notify_SMS-NewPackageinMailboxContent", content)

        ' POS receipt
        EmailNotificationsDb.Read_Notification(_EmailSetup.file_YourReceiptAttached, segment)
        subject = ExtractElementFromSegment("EmailSubject", segment, "Your receipt is attached")
        subject = subject.Replace("'", "''")
        content = _Convert.StringToBase64(File.ReadAllText(_EmailSetup.NotificationsFolder & _EmailSetup.file_YourReceiptAttached))

        UpdatePolicy(gShipriteDB, "Notify_Email-POSReceiptSubject", subject)
        UpdatePolicy(gShipriteDB, "Notify_Email-POSReceiptContent", content)


        ' AR Statment
        EmailNotificationsDb.Read_Notification(_EmailSetup.file_YourARStatementAttached, segment)
        subject = ExtractElementFromSegment("EmailSubject", segment, "Your Statement is Attached")
        subject = subject.Replace("'", "''")
        content = _Convert.StringToBase64(File.ReadAllText(_EmailSetup.NotificationsFolder & _EmailSetup.file_YourARStatementAttached))

        UpdatePolicy(gShipriteDB, "Notify_Email-ARStatementSubject", subject)
        UpdatePolicy(gShipriteDB, "Notify_Email-ARStatementContent", content)


        ' Mailbox Statment
        EmailNotificationsDb.Read_Notification(_EmailSetup.file_YourMBXStatementAttached, segment)
        subject = ExtractElementFromSegment("EmailSubject", segment, "Your Mailbox Statezment is attached")
        subject = subject.Replace("'", "''")
        content = _Convert.StringToBase64(File.ReadAllText(_EmailSetup.NotificationsFolder & _EmailSetup.file_YourMBXStatementAttached))

        UpdatePolicy(gShipriteDB, "Notify_Email-MailboxStatementSubject", subject)
        UpdatePolicy(gShipriteDB, "Notify_Email-MailboxStatementContent", content)

    End Sub

    Public Function LoadGlobalValues(ByRef Status As Label, Optional ByRef cWindow As Window = Nothing) As Integer
        Try
            Dim buf As String
            Dim oldDataPath As String = ""

            Status.Content = "Loading Globals..."

            gIniPath = "c:\windows\ShipriteNext.ini"
            gIniDefaultDirectory = "C:\ShipriteNext"
            gIniShipriteIndicator = "ShipriteNext"
            gDBpath = GetWinINI(gIniShipriteIndicator, gIniPath, gIniDefaultDirectory, "DataPath")
            gAppPath = GetWinINI(gIniShipriteIndicator, gIniPath, gIniDefaultDirectory, "ApplicationPath")
            gRptPath = GetWinINI(gIniShipriteIndicator, gIniPath, gIniDefaultDirectory, "ReportPath")
            gTransactionLog = GetWinINI(gIniShipriteIndicator, gIniPath, gIniDefaultDirectory, "TransactionLogPath")

            If Right(gDBpath, 1) = "\" Then

                gDBpath = Strings.Mid(gDBpath, 1, Len(gDBpath) - 1)

            End If
            If Right(gAppPath, 1) = "\" Then

                gAppPath = Strings.Mid(gAppPath, 1, Len(gAppPath) - 1)

            End If
            If Right(gRptPath, 1) = "\" Then

                gRptPath = Strings.Mid(gRptPath, 1, Len(gRptPath) - 1)

            End If

            gDASPath = gDBpath & "\DAS"
            gServiceTablesPath = gDBpath & "\ServiceTables"
            gTemplatesPath = gDBpath & "\Templates"
            gZoneTablesPath = gDBpath & "\ZoneTables"
            gReportWriter = gAppPath & "\Report_Writer.MDB"
            '
            If _Files.IsFileExist(gDBpath & "\zipcodes.mdb", False) Then
                gZipCodeDB = gDBpath & "\zipcodes.mdb"
            Else
                gZipCodeDB = gDBpath & "\zipcodes.accdb"
            End If
            If _Files.IsFileExist(gDBpath & "\country.mdb", False) Then
                gCountryDB = gDBpath & "\country.mdb"
            Else
                gCountryDB = gDBpath & "\country.accdb"
            End If
            Call Shipping.Load_CountryDB()
            '
            gShipriteDB = gDBpath + "\ShipriteNext.accdb"
            If Not String.IsNullOrEmpty(GetWinINI(gIniShipriteIndicator, gIniPath, gIniDefaultDirectory, "ShipriteDb")) Then
                gShipriteDB = GetWinINI(gIniShipriteIndicator, gIniPath, gIniDefaultDirectory, "ShipriteDb")
            End If
            _Debug.Print_("ShipriteDb = " & gShipriteDB)
            '

            gPricingMatrixDB = gDBpath & "\Pricing.accdb"

            If _Files.IsFileExist(gDBpath & "\ShipritePackaging.mdb", False) Then
                gPackagingDB = gDBpath & "\ShipritePackaging.mdb"
            Else
                gPackagingDB = gDBpath & "\ShipritePackaging.accdb"
            End If

            gFlatRatesDB = gServiceTablesPath & "\FlatRates.accdb"

            If _Files.IsFileExist(gDBpath & "\Shiprite_DropOffPackages.mdb", False) Then
                gDropOffDB = gDBpath & "\Shiprite_DropOffPackages.mdb"
            Else
                gDropOffDB = gDBpath & "\Shiprite_DropOffPackages.accdb"
            End If

            If _Files.IsFileExist(gDBpath & "\Shiprite_MailboxPackages.mdb", False) Then
                gMailboxDB = gDBpath & "\Shiprite_MailboxPackages.mdb"
            Else
                gMailboxDB = gDBpath & "\Shiprite_MailboxPackages.accdb"
            End If


            '
            gDC = "#"
            gReportsDB = gAppPath + "\Reports.accdb"
            gCS = DatabaseFunctions.GetConnectionStrings(gIniPath)
            For i = 0 To gCS - 1

                buf = gConnectionStrings(i).Name
                Select Case buf

                    Case "Shiprite"

                        gShipriteDB = i & "$Shiprite"
                        gDC = "'"

                    Case "Finance"

                        gFinanceDB = i & "$Finance"

                    Case "Quickbooks"

                        gQBdb = i & "$Quickbooks"

                End Select

            Next i
            ConvertShipriteMDBtoACCDB(cWindow)

            Check_If_DB_Present()

            MakePolicyTable(Status)
            If gConversionProcessHasRun = True Then

                oldDataPath = GetWinINI("ShipRite9", "c:\Windows\Shiprite.ini", "c:\Shiprite", "DataPath")
                Import_RTF_Templates(oldDataPath)

                'Delete Standard POS buttons
                IO_UpdateSQLProcessor(gShipriteDB, "DELETE FROM PosButtons WHERE [BN] < 43")


            End If
            gContactsTableSchema = IO_GetFieldsCollection(gShipriteDB, "Contacts", "", True, False, True)
            gARTableSchema = IO_GetFieldsCollection(gShipriteDB, "AR", "", True, False, True)
            gStatementsSchema = IO_GetFieldsCollection(gReportWriter, "Statements", "", True, False, True)
            gOpenCloseSchema = IO_GetFieldsCollection(gShipriteDB, "OpenClose", "", True, False, True)
            gMailboxTableSchema = IO_GetFieldsCollection(gShipriteDB, "Mailbox", "", True, False, True)
            gMBXHistoryTableSchema = IO_GetFieldsCollection(gShipriteDB, "MBXHistory", "", True, False, True)
            gTicklerSchema = IO_GetFieldsCollection(gShipriteDB, "Tickler", "", True, False, True)
            gDropOffSchema = IO_GetFieldsCollection(gDropOffDB, "DropOff_Packages", "", True, False, True)
            gMailBoxSchema = IO_GetFieldsCollection(gMailboxDB, "Mailbox_Packages", "", True, False, True)
            gInventorySchema = IO_GetFieldsCollection(gShipriteDB, "Inventory", "", True, False, True)
            ARAgingSchema = IO_GetFieldsCollection(gShipriteDB, "ARAging", "", True, False, True)
            PaymentsSchema = IO_GetFieldsCollection(gShipriteDB, "Payments", "", True, False, True)

            '
            gStoreName = GetPolicyData(gShipriteDB, "Name")
            gStoreZip = GetPolicyData(gShipriteDB, "Zip")
            gDrawerID = GetPolicyData(gReportsDB, "DrawerID")
            gCurrentUser = ""
            _StoreOwner.StoreOwner = _StoreOwner.Load_StoreOwnerContact()
            _Contact.ShipperContact = _StoreOwner.StoreOwner
            '

            ' the following 3 are booleans, added the False defaults to prevent "string to boolean" conversion error
            gIsProgramSecurityEnabled = GetPolicyData(gShipriteDB, "SecurityEnabled", False)
            gIsPOSSecurityEnabled = GetPolicyData(gShipriteDB, "POSSecurity", False)
            gIsSetupSecurityEnabled = GetPolicyData(gShipriteDB, "EnableSetupSecurity", False)

            gIsCustomerDisplayEnabled = GetPolicyData(gReportsDB, "Enable_CustomerDisplay", False)


            gFedExServicesDB = gServiceTablesPath & "\FEDEX_Services.accdb"
            gFedExRetailServicesDB = gServiceTablesPath & "\FEDEX_Retail_Services.accdb"
            gFedExZoneDB = gZoneTablesPath & "\FEDEX_Zones.accdb"

            gUPSServicesDB = gServiceTablesPath & "\UPS_Services.accdb"
            gUPSRetailServicesDB = gServiceTablesPath & "\UPS_Retail_Rates.accdb"
            gUPSZoneDB = gZoneTablesPath & "\UPS_Zones.accdb"

            gDHLServicesDB = gServiceTablesPath & "\DHL_Services.accdb"
            gDHLZoneDB = gZoneTablesPath & "\DHL_Zones.accdb"

            gUSMailDB_Services = gServiceTablesPath & "\USMail_Services.accdb"
            gUSMailDB_Zones = gZoneTablesPath & "\USMail_Zones.accdb"

            gSpeeDeeServicesDB = gServiceTablesPath & "\SpeeDee_Services.accdb"
            gSpeeDeeZoneDB = gZoneTablesPath & "\SpeeDee_Zones.accdb"

            CheckCustomRates()

            Check_RateUpdates()


            gdbSchema_Payments = IO_GetFieldsCollection(gShipriteDB, "Payments", "", True, False, True)
            gdbSchema_Transactions = IO_GetFieldsCollection(gShipriteDB, "Transactions", "", True, False, True)


            If IsFileExist("C:\windows\smartswiper.ini", False) Then
                gSmartSwiperDB = GetWinINI("SmartSwiper", "C:\windows\SmartSwiper.ini", "C:\SmartSwiper\Data", "DataPath") + "\SmartSwiper.mdb"
                gSmartSwiperReportsDB = GetWinINI("SmartSwiper", "C:\windows\SmartSwiper.ini", "C:\SmartSwiper", "ApplicationPath") + "\Reports.mdb"
            End If

            Dim FilePaths_Update As New Files_UpdatePaths
            FilePaths_Update.TransferFiles_FromOld2NewLocation()

            Return 0

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Loading Global Values")
        End Try

        Return 0

    End Function

    Private Function CheckCustomRates()
        Dim CustomRatesPath = gServiceTablesPath & "\Custom_Rates\"
        Dim CustomZonesPath = gZoneTablesPath & "\Custom_Rates\"
        Dim buf As String

        buf = CustomRatesPath & Path.GetFileName(gFedExServicesDB)
        If IsFileExist(buf, False) Then
            gFedExServicesDB = buf
        End If

        buf = CustomRatesPath & Path.GetFileName(gFedExRetailServicesDB)
        If IsFileExist(buf, False) Then
            gFedExRetailServicesDB = buf
        End If

        buf = CustomRatesPath & Path.GetFileName(gUPSServicesDB)
        If IsFileExist(buf, False) Then
            gUPSServicesDB = buf
        End If

        buf = CustomRatesPath & Path.GetFileName(gUPSRetailServicesDB)
        If IsFileExist(buf, False) Then
            gUPSRetailServicesDB = buf
        End If

        buf = CustomRatesPath & Path.GetFileName(gDHLServicesDB)
        If IsFileExist(buf, False) Then
            gDHLServicesDB = buf
        End If

        buf = CustomRatesPath & Path.GetFileName(gUSMailDB_Services)
        If IsFileExist(buf, False) Then
            gUSMailDB_Services = buf
        End If



        buf = CustomZonesPath & Path.GetFileName(gFedExZoneDB)
        If IsFileExist(buf, False) Then
            gFedExZoneDB = buf
        End If

        buf = CustomZonesPath & Path.GetFileName(gUPSZoneDB)
        If IsFileExist(buf, False) Then
            gUPSZoneDB = buf
        End If

        buf = CustomZonesPath & Path.GetFileName(gDHLZoneDB)
        If IsFileExist(buf, False) Then
            gDHLZoneDB = buf
        End If

        buf = CustomZonesPath & Path.GetFileName(gUSMailDB_Zones)
        If IsFileExist(buf, False) Then
            gUSMailDB_Zones = buf
        End If
        Return 0

    End Function

    Private Sub Check_RateUpdates()
        Dim successMessages As String = ""
        Dim errMessages As String = ""
        Dim isUpdateAttempted As Boolean = False

        RateUpdates_Check(True, isUpdateAttempted, successMessages, errMessages) 'Check Services
        RateUpdates_Check(False, isUpdateAttempted, successMessages, errMessages) 'Check Zones

        If isUpdateAttempted Then
            If errMessages = "" Then
                'success
                successMessages = "Rate update(s) applied: " & vbCrLf & successMessages
                MsgBox("Shipping Rate Updates Found and Applied:" & vbCrLf & vbCrLf & successMessages, vbInformation, gProgramName)
            Else
                'failure somewhere
                errMessages = "Rate update(s) failed: " & vbCrLf & errMessages
                If successMessages = "" Then
                    MsgBox("Shipping Rate Updates Found and Failed to Apply:" & vbCrLf & vbCrLf & errMessages, vbExclamation, gProgramName)
                Else
                    MsgBox("Shipping Rate Updates Applied with Errors:" & vbCrLf & vbCrLf & successMessages & vbCrLf & vbCrLf & errMessages, vbExclamation, gProgramName)
                End If
            End If
        End If


    End Sub


    Public Sub RateUpdates_Check(isServices As Boolean, ByRef isUpdateAttempted As Boolean, Optional ByRef successMsgs As String = "", Optional ByRef errMsgs As String = "")
        ''AP(12/15/2016) - Apply rate updates on specific future date.
        ' Rate update file variables
        Dim updKeyword As String : updKeyword = "SRRateUpdate"
        Dim updExt As String : updExt = ".upd"
        Dim updSep As String : updSep = "_-_"
        Dim updSearchPath As String
        Dim updPath As String
        Dim updName As String
        Dim updFiles() As String
        ' Rate DB file variables
        Dim dbPath As String : dbPath = IIf(Not Right$(gDBpath, 1) = "\", gDBpath & "\", gDBpath)

        Dim dbExt As String : dbExt = ".accdb"
        Dim DBName As String
        ' Date to take effect
        Dim rateDate As String
        Dim carrierName As String
        Dim iloc As Long
        Dim buf As String
        Dim BigBuf As String
        Dim i As Integer
        Dim errMsg As String



        If isServices Then
            dbPath = dbPath & "ServiceTables\"
        Else
            dbPath = dbPath & "ZoneTables\"
        End If

        i = 0
        ReDim updFiles(20)

        updSearchPath = dbPath & "*" & updKeyword & "*" & updExt ' get all matching files with the identifier and the extension
        updName = Dir$(updSearchPath)

        If Not updName = "" Then

            Do Until updName = ""

                updFiles(i) = updName
                updName = Dir$()
                i = i + 1

            Loop

            ReDim Preserve updFiles(IIf(i > 0, i - 1, i))

            Call QuickSort(updFiles, LBound(updFiles), UBound(updFiles))

            For i = 0 To UBound(updFiles)

                DBName = ""
                rateDate = ""
                errMsg = ""
                updName = updFiles(i)
                'dbPath = IIf(Not Right$(gDBpath, 1) = "\", gDBpath & "\", gDBpath)
                updPath = dbPath
                carrierName = ""
                ' remove extension
                BigBuf = StrReverse(Replace(StrReverse(updName), StrReverse(updExt), ""))

                Do Until BigBuf = ""
                    iloc = InStr(1, BigBuf, updSep)
                    If Not iloc = 0 Then
                        buf = BigBuf.Substring(0, iloc - 1)
                        BigBuf = Mid$(BigBuf, iloc + Len(updSep) - 1)
                    Else
                        buf = BigBuf
                        BigBuf = ""
                    End If

                    iloc = InStr(1, buf, "_")

                    If Not iloc = 0 Then
                        carrierName = UCase(buf.Substring(0, iloc - 1))
                        If Trim(UCase(carrierName)) = "USMAIL" Then
                            carrierName = "USPS"
                        End If
                    ElseIf Trim(UCase(buf)) = "FLATRATES" Then
                        carrierName = "USPS"
                    End If

                    If UCase(buf) = UCase(updKeyword) Then
                        'KeyWord - do nothing

                    ElseIf IsNumeric(buf) Or IsDate(buf) Then
                        'Date
                        buf = buf.Replace("-", "/")
                        If IsDate(buf) Then
                            rateDate = buf
                        Else
                            rateDate = "INVALID"
                        End If
                    ElseIf Not Dir$(dbPath & buf & dbExt) = "" Then
                        DBName = buf & dbExt
                    ElseIf Not Dir$(dbPath & carrierName & "\" & buf & dbExt) = "" Then
                        DBName = buf & dbExt
                        dbPath = dbPath & carrierName & "\"
                    End If

                Loop

                If Not DBName = "" Then
                    If Not rateDate = "" And Not UCase(rateDate) = "INVALID" Then
                        ' db and rate eff supplied
                        If Today.Date >= CDate(rateDate) Then
                            isUpdateAttempted = True
                            If RateUpdates_Apply(dbPath & DBName, updPath & updName, carrierName, errMsg) Then
                                successMsgs = successMsgs & carrierName & " (" & DBName & ") - " & rateDate & vbCrLf 'add to status messages
                            Else
                                errMsgs = errMsgs & carrierName & " (" & DBName & ") - " & rateDate & " - " & errMsg & vbCrLf 'if false, add to errMsgs
                            End If
                        End If
                    ElseIf UCase(rateDate) = "INVALID" Then
                        ' invalid date supplied in filename, do nothing
                    Else
                        ' db supplied, eff date not supplied
                        ' Apply rate update immediately with prompt?
                    End If
                End If

            Next

        End If

    End Sub

    Public Function RateUpdates_Apply(ByVal dbPath As String, ByVal updPath As String, ByVal carrierName As String, Optional ByRef errMsg As String = "") As Boolean
        ''AP(12/15/2016) - Apply rate updates on specific future date.
        Dim buf As String
        Dim DBName As String
        Dim updName As String
        errMsg = ""

        DBName = Dir$(dbPath)
        updName = Dir$(updPath)

        If Not DBName = "" And Not updName = "" Then
            errMsg = "Failed to save rate update file as database."
            If _Files.CopyFile_ToNewFolder(updPath, dbPath, False) Then
                errMsg = "Failed to delete rate update file."
                If _Files.Delete_File(updPath, False) Then
                    errMsg = ""
                End If
            End If
        Else
            If DBName = "" And updName = "" Then
                errMsg = dbPath & " and " & updPath & " don't exist."
            ElseIf DBName = "" Then
                errMsg = dbPath & " doesn't exist."
            ElseIf updName = "" Then
                errMsg = updPath & " doesn't exist."
            End If
        End If
        RateUpdates_Apply = (errMsg = "")
    End Function


    Public Sub QuickSort(ByRef vArray As Object, inLow As Long, inHi As Long)
        ''AP(12/15/2016) - QuickSort function added to sort arrays.
        Dim pivot As Object
        Dim tmpSwap As Object
        Dim tmpLow As Long
        Dim tmpHi As Long

        tmpLow = inLow
        tmpHi = inHi

        pivot = vArray((inLow + inHi) \ 2)

        Do While tmpLow <= tmpHi

            Do While UCase(vArray(tmpLow)) < UCase(pivot) And tmpLow < inHi
                tmpLow = tmpLow + 1
            Loop

            Do While UCase(pivot) < UCase(vArray(tmpHi)) And tmpHi > inLow
                tmpHi = tmpHi - 1
            Loop

            If tmpLow <= tmpHi Then
                tmpSwap = vArray(tmpLow)
                vArray(tmpLow) = vArray(tmpHi)
                vArray(tmpHi) = tmpSwap
                tmpLow = tmpLow + 1
                tmpHi = tmpHi - 1
            End If
        Loop

        If inLow < tmpHi Then QuickSort(vArray, inLow, tmpHi)
        If tmpLow < inHi Then QuickSort(vArray, tmpLow, inHi)

    End Sub

    Public Sub Check_Missing_MasterTable_Services()
        'Older SR users, might have missing the services in the Master table that were added over the years by utilities.
        'Check if those services are missing and add if necessary

        If Not IsService_Exist_Master("FEDEX-2DY-AM", gShipriteDB) Then
            add_Service2Master("FedEx 2Day® A.M.", "FEDEX-2DY-AM", "FedEx", "FEDEX-2DY", gShipriteDB)
        End If

        If Not IsService_Exist_Master("DHL-INT-DOC", gShipriteDB) Then
            add_Service2Master("DHL Express Worldwide Documents", "DHL-INT-DOC", "DHL", "DHL-INT", gShipriteDB)
        End If

        If Not IsService_Exist_Master("CAN-XSVR", gShipriteDB) Then
            add_Service2Master("UPS Worldwide Saver®", "CAN-XSVR", "UPS", "CAN-XPRES", gShipriteDB)
            IO_UpdateSQLProcessor(gShipriteDB, "Update Master Set [ZONE-TBL]='CAN-XSVR' WHERE [SERVICE]='CAN-XSVR'")
        End If

        If Not IsService_Exist_Master("WWXSVR", gShipriteDB) Then
            add_Service2Master("UPS Worldwide Saver®", "WWXSVR", "UPS", "WWXPRES", gShipriteDB)
            IO_UpdateSQLProcessor(gShipriteDB, "Update Master Set [ZONE-TBL]='WWXSVR' WHERE [SERVICE]='WWXSVR'")
        End If

    End Sub

    Public Sub MakePolicyTable(ByRef Status As Label)
        Try
            Dim SQL As String = ""
            Dim ret As Long = 0
            Dim oldShipritePath As String = ""
            Dim oldReportsPath As String = ""
            Dim buf As String = ""
            Dim setup1 As String = ""
            Dim setup2 As String = ""
            Dim eName As String = ""
            Dim eValue As String = ""
            Dim ID As Long = 1
            Dim holdStatus As String = Status.Content

            If File.Exists("c:\Windows\Shiprite.ini") Then
                oldShipritePath = GetWinINI("ShipRite9", "c:\Windows\Shiprite.ini", "c:\Shiprite", "DataPath") & "\shiprite.mdb"
                oldReportsPath = GetWinINI("ShipRite9", "c:\Windows\Shiprite.ini", "c:\Shiprite", "ApplicationPath") & "\reports.mdb"
            End If

            buf = IO_GetTableCollection(gShipriteDB, "Policy")
            If buf = "" Then

                ID = 1 ' reset
                Status.Content = "Creating Global Policy Table..."

                SQL = "CREATE TABLE Policy (ID LONG, ElementName CHAR(64), ElementValue TEXT)"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                If ret = -1 Then
                    End
                End If
                SQL = "CREATE UNIQUE INDEX ID ON Policy (ID)"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                SQL = "CREATE UNIQUE INDEX ElementName ON Policy (ElementName) WITH PRIMARY"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                Status.Content = "Converting Global Setup Tables to Policy Table..."

                SQL = "SELECT * FROM SETUP WHERE ID = 1"
                buf = IO_GetSegmentSet(gShipriteDB, SQL)
                If buf = "" AndAlso Not String.IsNullOrWhiteSpace(oldShipritePath) Then
                    ' if nothing in current db, then try old shiprite db
                    buf = IO_GetSegmentSet(oldShipritePath, SQL)
                End If
                setup1 = GetNextSegmentFromSet(buf)

                SQL = "SELECT * FROM SETUP2 WHERE ID = 1"
                buf = IO_GetSegmentSet(gShipriteDB, SQL)
                If buf = "" AndAlso Not String.IsNullOrWhiteSpace(oldShipritePath) Then
                    ' if nothing in current db, then try old shiprite db
                    buf = IO_GetSegmentSet(oldShipritePath, SQL)
                End If
                setup2 = GetNextSegmentFromSet(buf)
                buf = setup1 & setup2

                Do Until buf = ""

                    buf = ExtractNextElementFromSegment(eName, eValue, buf)
                    eValue = FlushOut(eValue, "'", "~")
                    eValue = FlushOut(eValue, "~", "''")

                    If Not eName = "ID" And Not eName = "AIRBORNERoutingFileDate" Then

                        Status.Content = "Convert Global Policy Field: " & eName

                        ' After conversion will require converting Setup tables to Policy table.
                        ' Reset the Special Updates value so all SRN special updates are run.
                        If eName = "SpecialUpdatesVersion" Then
                            eValue = ""
                        End If

                        SQL = "INSERT INTO Policy (ID, ElementName, ElementValue) VALUES (" & ID & ", '" & Trim(eName) & "', '" & eValue & "')"
                        ret += IO_UpdateSQLProcessor(gShipriteDB, SQL)
                        ID += 1

                    End If
                    System.Windows.Forms.Application.DoEvents()

                Loop

                Status.Content = "Removing Global Setup Tables..."

                SQL = "DROP Table "

                buf = IO_GetTableCollection(gShipriteDB, "Setup", "")
                If Not buf = "" Then
                    SQL &= "Setup"
                End If
                buf = IO_GetTableCollection(gShipriteDB, "Setup2", "")
                If Not buf = "" Then
                    If Not SQL = "DROP Table " Then
                        SQL &= ","
                    End If
                    SQL &= "Setup2"
                End If
                If Not SQL = "DROP Table " Then
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                End If

            End If

            buf = IO_GetTableCollection(gReportsDB, "Policy")
            If buf = "" Then

                ID = 1 ' reset
                Status.Content = "Creating Local Policy Table..."

                SQL = "CREATE TABLE Policy (ID LONG, ElementName CHAR(64), ElementValue TEXT)"
                ret = IO_UpdateSQLProcessor(gReportsDB, SQL)
                If ret = -1 Then
                    End
                End If
                SQL = "CREATE UNIQUE INDEX ID ON Policy (ID)"
                ret = IO_UpdateSQLProcessor(gReportsDB, SQL)
                SQL = "CREATE UNIQUE INDEX ElementName ON Policy (ElementName) WITH PRIMARY"
                ret = IO_UpdateSQLProcessor(gReportsDB, SQL)

                Status.Content = "Converting Local Setup Tables to Policy Table..."

                SQL = "SELECT * FROM SETUP WHERE ID = 1"
                buf = IO_GetSegmentSet(gReportsDB, SQL)
                If buf = "" AndAlso Not String.IsNullOrWhiteSpace(oldReportsPath) Then
                    ' if nothing in current db, then try old shiprite db
                    buf = IO_GetSegmentSet(oldReportsPath, SQL)
                End If
                buf = GetNextSegmentFromSet(buf)

                Do Until buf = ""

                    buf = ExtractNextElementFromSegment(eName, eValue, buf)
                    eValue = FlushOut(eValue, "'", "~")
                    eValue = FlushOut(eValue, "~", "''")

                    If Not eName = "ID" Then

                        Status.Content = "Convert Local Policy Field: " & eName

                        SQL = "INSERT INTO Policy (ID, ElementName, ElementValue) VALUES (" & ID & ", '" & Trim(eName) & "', '" & eValue & "')"
                        ret += IO_UpdateSQLProcessor(gReportsDB, SQL)
                        ID += 1

                    End If
                    System.Windows.Forms.Application.DoEvents()

                Loop

                Status.Content = "Removing Local Setup Tables..."

                SQL = "DROP Table "

                buf = IO_GetTableCollection(gReportsDB, "Setup", "")
                If Not buf = "" Then
                    SQL &= "Setup"
                End If
                If Not SQL = "DROP Table " Then
                    ret = IO_UpdateSQLProcessor(gReportsDB, SQL)
                End If

            End If

            Status.Content = holdStatus

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Making Policy Table")
        End Try

    End Sub

    Public Function Load_MasterShippingTable() As Integer
        Try

            Load_Carrier_Panels()

            Dim SQL As String
            Dim Segment As String
            Dim SegmentSet As String

            Segment = ""
            SegmentSet = ""
            gMCT = 0
            SQL = "SELECT * FROM Master ORDER BY Carrier, SERVICE"
            SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
            Do Until SegmentSet = ""

                Segment = GetNextSegmentFromSet(SegmentSet)
                gMaster(gMCT).PrimaryKey = Val(ExtractElementFromSegment("ID", Segment))
                gMaster(gMCT).ZoneTable = ExtractElementFromSegment("ZONE-TBL", Segment)
                gMaster(gMCT).ServiceTable = ExtractElementFromSegment("SERVICE", Segment)
                gMaster(gMCT).Carrier = ExtractElementFromSegment("Carrier", Segment)
                gMaster(gMCT).International = ExtractElementFromSegment("International", Segment)
                gMaster(gMCT).Level1 = Val(ExtractElementFromSegment("LEVEL1", Segment))
                gMaster(gMCT).Level2 = Val(ExtractElementFromSegment("LEVEL2", Segment))
                gMaster(gMCT).Level3 = Val(ExtractElementFromSegment("LEVEL3", Segment))
                gMaster(gMCT).LevelR = Val(ExtractElementFromSegment("RETAIL", Segment))
                gMaster(gMCT).LetterFee = Val(ExtractElementFromSegment("LetterFee", Segment, 0))
                gMaster(gMCT).LetterPercentage = Val(ExtractElementFromSegment("LetterPercent", Segment, 0))
                If ExtractElementFromSegment("NextPickupDate", Segment, "") <> "" Then
                    gMaster(gMCT).PickupDate = ExtractElementFromSegment("NextPickupDate", Segment)
                Else
                    gMaster(gMCT).PickupDate = Nothing
                End If

                gMaster(gMCT).Segment = Segment
                gMCT = gMCT + 1

            Loop
            Return gMCT

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Loading Master Shipping Table")
        End Try

        Return 0

    End Function

    Public Sub Load_Carrier_Panels()
        Try

            Dim SegmentSet As String = ""
            Dim current_segment As String = ""
            Dim buf As String = ""
            Dim buf2 As String = ""
            Dim SQL As String
            Dim current_Carrier As Carrier

            Dim Current_Service As ShippingChoiceDefinition
            Dim ServiceSegment As String

            gCarrierList = New List(Of Carrier)
            buf = IO_GetSegmentSet(gShipriteDB, "SELECT DISTINCT Carrier, Panel_Row, Panel_Row_Canada, Panel_Row_Intl, Panel_Row_Freight, Domestic_Status, Intl_Status, Canada_Status, Freight_Status  from Master")

            'Loop through each carrier
            Do Until buf = ""
                current_segment = GetNextSegmentFromSet(buf)

                current_Carrier = New Carrier
                current_Carrier.CarrierName = ExtractElementFromSegment("Carrier", current_segment, "")

                current_Carrier.CarrierImage = "Resources/" & current_Carrier.CarrierName & "_Logo.png"

                current_Carrier.Panel_Domestic = Val(ExtractElementFromSegment("Panel_Row", current_segment, "0"))
                current_Carrier.Panel_Intl = Val(ExtractElementFromSegment("Panel_Row_Intl", current_segment, "0"))
                current_Carrier.Panel_Canada = Val(ExtractElementFromSegment("Panel_Row_Canada", current_segment, "0"))
                current_Carrier.Panel_Freight = Val(ExtractElementFromSegment("Panel_Row_Freight", current_segment, "0"))

                current_Carrier.Status_Domestic = Val(ExtractElementFromSegment("Domestic_Status", current_segment, "0"))
                current_Carrier.Status_Intl = Val(ExtractElementFromSegment("Intl_Status", current_segment, "0"))
                current_Carrier.Status_Canada = Val(ExtractElementFromSegment("Canada_Status", current_segment, "0"))
                current_Carrier.Status_Freight = Val(ExtractElementFromSegment("Freight_Status", current_segment, "0"))

                current_Carrier.ServiceList = New List(Of ShippingChoiceDefinition)
                current_Carrier.ServiceList_Domestic = New List(Of ShippingChoiceDefinition)
                current_Carrier.ServiceList_International = New List(Of ShippingChoiceDefinition)
                current_Carrier.ServiceList_Canada = New List(Of ShippingChoiceDefinition)
                current_Carrier.ServiceList_Freight = New List(Of ShippingChoiceDefinition)

                'Load in Services for carrier into list---------------------
                buf2 = IO_GetSegmentSet(gShipriteDB, "SELECT * from Master WHERE Carrier='" & current_Carrier.CarrierName & "'")
                Do Until buf2 = ""
                    Current_Service = New ShippingChoiceDefinition
                    ServiceSegment = GetNextSegmentFromSet(buf2)

                    Current_Service.Segment = ServiceSegment
                    Current_Service.Service = ExtractElementFromSegment("Service", ServiceSegment, "")
                    Current_Service.ServiceName = ExtractElementFromSegment("Description", ServiceSegment, "")
                    Current_Service.Column = Val(ExtractElementFromSegment("Panel_Column", ServiceSegment, "0"))
                    Current_Service.Column_Canada = Val(ExtractElementFromSegment("Panel_Column_Canada", ServiceSegment, "0"))
                    Current_Service.ZoneTable = ExtractElementFromSegment("ZONE-TBL", ServiceSegment, "")
                    Current_Service.Carrier = ExtractElementFromSegment("Carrier", ServiceSegment, "")
                    Current_Service.AirOrExpress = ExtractElementFromSegment("AIR", ServiceSegment)

                    Current_Service.IsButtonVisible = Visibility.Visible

                    'separate services for domestic, canada, international, freight
                    If FedEx_Freight.IsFreightLTLService(Current_Service.Service) Or FedEx_Freight.IsFreight_123Day_Service(Current_Service.Service) Then
                        current_Carrier.ServiceList_Freight.Add(Current_Service)
                    End If

                    If isServiceInternational(Current_Service.Service) Then
                        If Current_Service.Carrier = "UPS" Then
                            'make sure UPS canadian services are excluded from international button panel
                            If Not isServiceCanadian(Current_Service.Service) Then
                                current_Carrier.ServiceList_International.Add(Current_Service)
                            End If

                        Else
                            current_Carrier.ServiceList_International.Add(Current_Service)
                        End If
                    End If

                    If isServiceCanadian(Current_Service.Service) Then
                        current_Carrier.ServiceList_Canada.Add(Current_Service)
                    End If

                    If isServiceDomestic(Current_Service.Service) Then
                        current_Carrier.ServiceList_Domestic.Add(Current_Service)
                    End If

                Loop
                '------------------------------------------------------------
                current_Carrier.ServiceList_Domestic = OrderServices(current_Carrier.ServiceList_Domestic)
                current_Carrier.ServiceList_International = OrderServices(current_Carrier.ServiceList_International)
                current_Carrier.ServiceList_Canada = OrderServices(current_Carrier.ServiceList_Canada, True)
                current_Carrier.ServiceList_Freight = OrderServices(current_Carrier.ServiceList_Freight)


                'Add List of all packaging items for carrier
                Dim Packaging_List As List(Of PackagingItem) = New List(Of PackagingItem)

                '-add blank item at beginning of list
                Dim item As PackagingItem = New PackagingItem
                item.SettingID = 0
                item.SettingName = ""
                item.SettingDesc = ""
                Packaging_List.Add(item)
                '-------------------------

                If current_Carrier.CarrierName = "USPS" Then
                    SQL = "SELECT * From PackagingItems WHERE SettingName LIKE '%FlatR%'"
                    Load_FlatRate_Packaging(Packaging_List, SQL)
                End If


                SQL = "SELECT PackagingItems.Disabled, CarrierPackagingValues.CarrierID, Carriers.CarrierName, CarrierPackagingValues.SettingID, PackagingItems.SettingName, PackagingItems.Length, PackagingItems.Height, PackagingItems.Width, PackagingItems.MaxLBs, PackagingItems.SettingDesc
FROM PackagingItems INNER JOIN (Carriers INNER JOIN CarrierPackagingValues ON Carriers.CarrierID = CarrierPackagingValues.CarrierID) ON PackagingItems.SettingID = CarrierPackagingValues.SettingID
WHERE (Carriers.CarrierName='" & current_Carrier.CarrierName & "' AND PackagingItems.Disabled=FALSE)
ORDER BY CarrierPackagingValues.CarrierID, PackagingItems.SettingOrderNo"

                Load_FlatRate_Packaging(Packaging_List, SQL)
                current_Carrier.Packaging_List = Packaging_List

                gCarrierList.Add(current_Carrier)


            Loop

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Loading Carrier Panels")
        End Try
    End Sub

    Private Sub Check_FreightServices()
        ''AP(11/01/2018) - Updated startup to create FedEx Express & LTL Freight services and panel if don't exist.
        If IsFreight_Added_ToExistingPanel("FedEx Freight 1DAY", "FedEx 1Day® Freight", "FEDEX-FR1", "FedEx", "FEDEX-FR1", gShipriteDB) Then
            If IsFreight_Added_ToExistingPanel("FedEx Freight 2DAY", "FedEx 2Day® Freight", "FEDEX-FR2", "FedEx", "FEDEX-FR1", gShipriteDB) Then
                If IsFreight_Added_ToExistingPanel("FedEx Freight 3DAY", "FedEx 3Day® Freight", "FEDEX-FR3", "FedEx", "FEDEX-FR1", gShipriteDB) Then
                    ''ol#18.01(5/17)... FedEx Freight Box services were added.
                    If IsFreight_Added_ToExistingPanel("FedEx Freight Priority", "FedEx Freight® Priority", "FEDEX-FRP", "FedEx", "FEDEX-FR1", gShipriteDB) Then
                        ''ol#18.04(10/2)... Create LTL Freight from scratch if FR1 doesn't exist.
                        If IsFreight_Added_ToExistingPanel("FedEx Freight Economy", "FedEx Freight® Economy", "FEDEX-FRE", "FedEx", "FEDEX-FRP", gShipriteDB) Then

                        End If
                    End If
                End If
            End If
        End If
    End Sub

    Public Sub Load_ShipRite_CarrierSetup()
        Try
            '
            ''Global variables below loaded earlier with other Global variables
            '_StoreOwner.StoreOwner = _StoreOwner.Load_StoreOwnerContact()
            '_Contact.ShipperContact = _StoreOwner.StoreOwner
            '_Contact.Load_ContactFromDb(_StoreOwner.StoreOwner.ContactID, _Contact.ShipperContact)

            If _IDs.IsIt_CanadaShipper Then
                _IDs.CurrencyType = "CAD"
                _IDs.IsMetricSystem = True
            Else
                _IDs.CurrencyType = "USD"
                _IDs.IsMetricSystem = False
            End If
            '
            _EndiciaWeb.objEndiciaCredentials = New _EndiciaSetup
            _UPSWeb.objUPS_Setup = New UPSSetupData
            _Dhl_XML.objDHL_Setup = New DHL_Setup
            FedExCERT.IsFedExTestAccount = False
            _FedExWeb.IsEnabled_OneRate = False
            _FedExWeb.objFedEx_Regular_Setup = New FedEx_Setup(False)
            _FedExWeb.objFedEx_Freight_Setup = New FedEx_Setup(True)
            '
            _UPSWeb.IsEmail_UPS_ShipNotification = IIf(GetPolicyData(gShipriteDB, "Disable_UPS_EmailShipNotifications", "True") = "False", True, False)
            FedEx.IsEmail_FedEx_ShipNotification = IIf(GetPolicyData(gShipriteDB, "Disable_FedEx_EmailShipNotifications", "True") = "False", True, False)
            '
            _Files.Create_Folder(String.Format("{0}\UPS\InOut", gDBpath), False)
            _Files.Create_Folder(String.Format("{0}\FedEx\InOut", gDBpath), False)
            _Files.Create_Folder(String.Format("{0}\DHL\InOut", gDBpath), False)
            _Files.Create_Folder(String.Format("{0}\Endicia\InOut", gDBpath), False)
            '
            '
            If GetPolicyData(gShipriteDB, "FedExREST_Enabled", "False") Then Load_FedExREST_Setup()

            gFedExReturnsSETUP = New FedExRETURNS_SETUP
            '
            ' Third Party Insurance:
            Call Load_3dParty_Insurance()

            Call Update_Setup_Table()

            Cache_FASC_ASO_PN_DiscountLevels()
            '
            Check_FlatRate_Pricing()

            Cache_Peak_Surcharges()

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Loading Carrier Setup")
        End Try
    End Sub

    Private Sub Cache_Peak_Surcharges()
        Load_Peak_Surcharges(gUPSPeakSurcharges, "UPS")
        Load_Peak_Surcharges(gFedExPeakSurcharges, "FedEx")
        Load_Peak_Surcharges(gDHLPeakSurcharges, "DHL")
    End Sub

    Private Sub Load_Peak_Surcharges(ByRef surcharge_list As List(Of Peak_Surcharge), carrier As String)
        Try
            Dim Buffer As String = ""
            Dim current_segment As String
            Dim surcharge As Peak_Surcharge
            surcharge_list = New List(Of Peak_Surcharge)

            If carrier = "UPS" Then
                Buffer = IO_GetSegmentSet(gUPSServicesDB, "Select * From Holiday_Charges")

            ElseIf carrier = "FedEx" Then
                Buffer = IO_GetSegmentSet(gFedExServicesDB, "Select * From Holiday_Charges")

            ElseIf carrier = "DHL" Then
                Buffer = IO_GetSegmentSet(gDHLServicesDB, "Select * From Temp_Surcharges")
            End If

            Do Until Buffer = ""
                current_segment = GetNextSegmentFromSet(Buffer)
                surcharge = New Peak_Surcharge

                surcharge.Surcharge = ExtractElementFromSegment("Surcharge", current_segment, "0")
                surcharge.Service = ExtractElementFromSegment("Service", current_segment)
                surcharge.Cost = CDbl(ExtractElementFromSegment("Cost", current_segment, "0"))
                surcharge.Retail = CDbl(ExtractElementFromSegment("Retail", current_segment, "0"))
                surcharge.DateFrom = ExtractElementFromSegment("DateFrom", current_segment)
                surcharge.DateTo = ExtractElementFromSegment("DateTo", current_segment)

                surcharge_list.Add(surcharge)
            Loop


        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Load_FedExREST_Setup()
        gFedExSETUP = New FedExREST_SETUP

        gFedExSETUP.OAuthToken = GetPolicyData(gShipriteDB, "FedExREST_OAuth_Token", "")
        gFedExSETUP.OAuthExpires = GetPolicyData(gShipriteDB, "FedExREST_OAuth_Expires", "1/1/0001")

        gFedExSETUP.Customer_Key = GetPolicyData(gShipriteDB, "FedExREST_CustomerKey")
        gFedExSETUP.Customer_SecretKey = GetPolicyData(gShipriteDB, "FedExREST_CustomerSecret")

        gFedExSETUP.CustomerName = GetPolicyData(gShipriteDB, "Name")
        gFedExSETUP.AccountNumber = GetPolicyData(gShipriteDB, "FedExAccountNumber")


        Dim address As New FXR_address
        With address
            .city = GetPolicyData(gShipriteDB, "City")
            .postalCode = GetPolicyData(gShipriteDB, "Zip")
            .streetLines.Add(GetPolicyData(gShipriteDB, "Addr1"))

            If GetPolicyData(gShipriteDB, "Addr2", "") <> "" Then
                .streetLines.Add(GetPolicyData(gShipriteDB, "Addr2"))
            End If

            .stateOrProvinceCode = GetPolicyData(gShipriteDB, "State")
            .countryCode = "US"
        End With

        gFedExSETUP.CustomerAddress = address

    End Sub

#Region "Third Party Insurance"

    Public Sub Load_3dParty_Insurance()
        '
        ' Shiprite could have other than DSI as 3rd Party Insurer, but only one name at a time.
        gThirdPartyInsurance = IIf(GetPolicyData(gShipriteDB, "ThirdPartyInsurance", "False") = "True", True, False)
        If gThirdPartyInsurance Then
            '
            gDSIis3rdPartyInsurance = IIf(GetPolicyData(gShipriteDB, "DSI_PolicyID") <> "", True, False)
            ' Loading DSI Signature Threshold
            gDSISig = Val(GetPolicyData(gShipriteDB, "DSISigThreshold", "0"))
            If gDSISig = "" Then
                gDSISig = 1000
            End If
            '
            If gDSIis3rdPartyInsurance Then
                If DSI_Excluded_CountryList.Count = 0 Then
                    '
                    ' DSI excluded country list added to block insuring International shipments with DSI that are in this list.
                    Dim filestring As String = String.Empty
                    Dim split_filestring() As String = Nothing
                    Dim countryName As String = String.Empty
                    If _Files.ReadFile_ToEnd(gDBpath & "\DSI_ExcludedCountries.txt", False, filestring) Then
                        split_filestring = filestring.Split(",")
                        For i As Integer = 0 To split_filestring.Count - 1
                            countryName = split_filestring(i).Trim
                            DSI_Excluded_CountryList.Add(countryName, countryName)
                        Next
                    End If
                    '
                    ' test only
                    'DSI_IsCountryInExcludedList("Brazil")

                    ' "Member of DSI Premiere Program" check box was added to the DSI Insurance Setup screen.
                    DSI_PremiereProgramMember = IIf(GetPolicyData(gShipriteDB, "DSI_PremiereProgramMember", "False") = "True", True, False)
                    '
                End If
            End If
            '
        End If
        '
    End Sub
    Public Function DSI_IsCountryInExcludedList(ByVal countryName As String) As Boolean
        DSI_IsCountryInExcludedList = False
        If DSI_Excluded_CountryList.Count > 0 Then
            Return DSI_Excluded_CountryList.TryGetValue(countryName, Nothing)
        End If
    End Function
    Public Function IsOn_gThirdPartyInsurance(ByVal sCountryName As String, Optional sCarrierName As String = "", Optional sServiceABBR As String = "", Optional nDecValue As Long = 0, Optional retReason2Decline As String = "") As Boolean
        retReason2Decline = "" '' assume.
        IsOn_gThirdPartyInsurance = gThirdPartyInsurance

        If nDecValue = 0 Then
            IsOn_gThirdPartyInsurance = False
            Exit Function
        End If

        If IsOn_gThirdPartyInsurance Then

            If Not gShip.Country = "US" And GetPolicyData(gShipriteDB, "EnableShipAndInsure") = "True" Then

                IsOn_gThirdPartyInsurance = False
                Exit Function

            End If

            '
            ' Shiprite could have other than DSI as 3rd Party Insurer, but only one name at a time.
            If GetPolicyData(gShipriteDB, "EnableShipsurance") = "True" Then
                '
                ' DSI excluded country list added to block insuring International shipments with DSI that are in this list.
                If Not 0 = Len(sCountryName) Then IsOn_gThirdPartyInsurance = Not DSI_IsCountryInExcludedList(sCountryName)
                '
                If IsOn_gThirdPartyInsurance Then
                    '
                    ' DSI will insure USPS services only up to $1000.00, apply USPS insurance instead of disabling the services.
                    If "USPS" = sCarrierName Then
                        '
                        ' All USPS International shipments insured with Shipsurance must be shipped via Global Express Mail.
                        If Not gShip.Domestic And Not sServiceABBR = _USPS.Intl_GlobalExpressGuaranteed Then
                            retReason2Decline = DSI_NewName & " will insure only shipments via Global Express Guaranteed mail!" & vbCr &
                                            "Declared Value for this shipment will be submitted to the carrier instead..."
                            IsOn_gThirdPartyInsurance = False
                        Else
                            IsOn_gThirdPartyInsurance = Not (nDecValue > 1000)
                            If Not IsOn_gThirdPartyInsurance Then
                                retReason2Decline = DSI_NewName & " will insure USPS services only up to $1000.00 !" & vbCr &
                                                "Declared Value for this shipment will be submitted to the carrier instead..."
                            End If
                        End If
                        '
                    End If
                    '
                End If
                '
            End If
            If GetPolicyData(gShipriteDB, "EnableShipAndInsure") = "True" Then

                Dim CarrierCode As String = ShipandInsure_GetCarrierID(gSelectedShipmentChoice.Service)
                Dim UID As String = GetPolicyData(gShipriteDB, "ShipAndInsureUserID")
                Dim passwd As String = GetPolicyData(gShipriteDB, "ShipAndInsurePassword")
                Dim FromZip = GetPolicyData(gShipriteDB, "Zip")
                Dim ToZip = gSelectedShipmentChoice.ZipCode
                ShipandInsure_IsTest = False
                Dim t As String = ShipandInsure_GetShipmentCost(UID, "Shiprite", passwd, "Gary Ford", "10293", "1Z1111111111111112", CarrierCode, nDecValue, FromZip, ToZip, gShip.Country)
                Dim amt As Double

                ShipandInsure_IsTest = False
                amt = Val(t)

                If amt = 0 Then

                    IsOn_gThirdPartyInsurance = False

                Else

                    gShip.ShipAndInsureCost = Val(t)
                    IsOn_gThirdPartyInsurance = True
                    gShip.ShipAndInsureCost = amt

                End If
                '
            End If

        End If
        '------------------------------------------------------------
    End Function


#End Region

    Public Function Special_Updates()
        Dim SpecialUpdatesVersion As Date

        Try
            Dim buf As String

            Dim FuelUpdatesVersion As Date
            Dim ret As Long
            Dim SQL As String
            Dim Segment As String
            Dim SegmentSet As String
            Dim MaxID As Long

            SQL = ""
            Segment = ""
            SegmentSet = ""

            buf = GetPolicyData(gShipriteDB, "SpecialUpdatesVersion")
            buf = ReformatDate(buf)  ' if date not formated correctly, blank is returned
            If Not buf = "" Then

                SpecialUpdatesVersion = buf

            Else

                SpecialUpdatesVersion = "01/01/1901"
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", SpecialUpdatesVersion)

            End If

            buf = GetPolicyData(gShipriteDB, "FuelUpdatesVersion")
            buf = ReformatDate(buf)  ' if date not formated correctly, blank is returned
            If Not buf = "" Then

                FuelUpdatesVersion = buf

            Else

                FuelUpdatesVersion = "01/01/1901"
                ret = UpdatePolicy(gShipriteDB, "FuelUpdatesVersion", FuelUpdatesVersion)

            End If
            If gConversionProcessHasRun = True Then

                SpecialUpdatesVersion = "04/09/2018"  ' Making sure the date is less than f

            End If
            If CDate(SpecialUpdatesVersion) < #4/10/2018# Then                               '   Create ID Column in Each table that does not have one

                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "04/10/2018")

                If IO_GetFieldsCollection(gShipriteDB, "Master", "ID", False, False, False) = "" Then
                    SQL = "alter Table Master add ID int identity(1, 1)"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                End If

                If IO_GetFieldsCollection(gShipriteDB, "ar", "ID", False, False, False) = "" Then
                    SQL = "alter Table ar add ID int identity(1, 1)"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                End If

                If IO_GetFieldsCollection(gShipriteDB, "araging", "ID", False, False, False) = "" Then
                    SQL = "alter Table araging add ID int identity(1, 1)"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                End If

                If IO_GetFieldsCollection(gShipriteDB, "countytaxes", "ID", False, False, False) = "" Then
                    SQL = "alter Table countytaxes add ID int identity(1, 1)"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                End If

                If IO_GetFieldsCollection(gShipriteDB, "departments", "ID", False, False, False) = "" Then
                    SQL = "alter Table departments add ID int identity(1, 1)"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                End If

                If IO_GetFieldsCollection(gShipriteDB, "dropoffs", "ID", False, False, False) = "" Then
                    SQL = "alter Table dropoffs add ID int identity(1, 1)"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                End If

                If IO_GetFieldsCollection(gShipriteDB, "employees", "ID", False, False, False) = "" Then
                    SQL = "alter Table employees add ID int identity(1, 1)"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                End If

                If IO_GetFieldsCollection(gShipriteDB, "inventory", "ID", False, False, False) = "" Then
                    SQL = "alter Table inventory add ID int identity(1, 1)"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                End If

                If IO_GetFieldsCollection(gShipriteDB, "mailboxsize", "ID", False, False, False) = "" Then
                    SQL = "alter Table mailboxsize add ID int identity(1, 1)"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                End If

                If IO_GetFieldsCollection(gShipriteDB, "packaging", "ID", False, False, False) = "" Then
                    SQL = "alter Table packaging add ID int identity(1, 1)"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                End If

                If IO_GetFieldsCollection(gShipriteDB, "panel", "ID", False, False, False) = "" Then
                    SQL = "alter Table panel add ID int identity(1, 1)"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                End If

                If IO_GetFieldsCollection(gShipriteDB, "zreport", "ID", False, False, False) = "" Then
                    SQL = "alter Table zreport add ID int identity(1, 1)"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                End If

            End If
            If CDate(SpecialUpdatesVersion) < #04/23/2018# Then                               '   Fix Master Shipping Table

                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "04/23/2018")
                SQL = "DELETE * FROM Master WHERE PosDept = 'AIRBORNE'"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                If IO_GetFieldsCollection(gShipriteDB, "Master", "International", False, False, False) = "" Then
                    SQL = "ALTER Table Master ADD International YESNO DEFAULT FALSE"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                End If
                SQL = "UPDATE Master SET International = True WHERE SERVICE IN ('WWXPED', 'WWXPRES', 'WWXSVR', 'CAN-STD', 'CAN-XPEP', 'CAN-XPRES', 'CAN-XSVR', 'DHL-INT', 'DHL-INT-DOC', 'FEDEX-CAN', 'FEDEX-INT-1ST', 'FEDEX-INTP', 'FEDEX-INTE', 'USPS-INTL-EMI', 'USPS-INTL-FCMI', 'USPS-INTL-GXG', 'USPS-INTL-PMI')"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

            End If

            If CDate(SpecialUpdatesVersion) < #06/12/2018# Then
                '------ update USERS table ---------
                Dim UsersWithFullPermissions As String
                Dim User As String
                SQL = "Select USERID From Users where Perm_Ship=True and Perm_ViewCosts=True and  Perm_Suspend=True and  Perm_NameTbls=True and  Perm_OthTbls=True and  Perm_PickupRec=True and  Perm_IncRpts=True and  Perm_Tracking=True and  Perm_Utilities=True and  Perm_Setup=True and  Perm_Users=True and  Perm_POS=True and  Perm_Online=True and  Perm_Contacts=True and  Perm_Void=True and  Perm_Reports=True and  Perm_POSManager=True and  Perm_Discount=True and  Button_Panel=True"
                UsersWithFullPermissions = IO_GetSegmentSet(gShipriteDB, SQL)


                SQL = "ALTER TABLE Users ADD DisplayName text, FirstName text, LastName text, Add1 text, Add2 text, City text, State text, Zip text, Email text, POS yesno, AccountsReceivable yesno, Inventory yesno, POSManager yesno, Edit_POS_Buttons yesno, POS_Discounts yesno, POS_VoidSale yesno, POS_Refunds yesno, POS_DeleteLine yesno, SHIPPING yesno, View_Shipping_Costs yesno, EOD_Manifest yesno, Void_Shipment yesno, SETUP yesno, Setup_Users yesno, Setup_Carriers yesno, REPORTS yesno, Reports_IncomeProduction yesno"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                SQL = "UPDATE Users Set DisplayName = UserID, FirstName=FullName"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                SQL = "ALTER TABLE Users DROP COLUMN UserID, FullName, Perm_Ship, Perm_ViewCosts, Perm_Suspend, Perm_NameTbls, Perm_OthTbls, Perm_PickupRec, Perm_IncRpts, Perm_Tracking, Perm_Utilities, Perm_Setup, Perm_Users, Perm_POS, Perm_Online, Perm_Contacts, Perm_Void, Perm_Reports, Perm_POSManager, Perm_Shipper, Perm_Discount, Button_Panel, POS_Line_Delete"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                SQL = "ALTER TABLE Users ADD COLUMN AR_CreateAccounts yesno"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                SQL = "ALTER Table Users ADD COLUMN Setup_Mailbox yesno"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)


                Do Until UsersWithFullPermissions = ""
                    User = GetNextSegmentFromSet(UsersWithFullPermissions)
                    SQL = "Update Users SET POS=True, AccountsReceivable=True, AR_CreateAccounts=True, Inventory=True, POSManager=True, Edit_POS_Buttons=True, POS_Discounts=True, POS_VoidSale=True, POS_Refunds=True, POS_DeleteLine=True, SHIPPING=True, View_Shipping_Costs=True, EOD_Manifest=True, Void_Shipment=True, SETUP=True, Setup_Users=True, Setup_Carriers=True, Setup_Mailbox=True, REPORTS=True, Reports_IncomeProduction=True WHERE DisplayName='”
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL & ExtractElementFromSegment("USERID", User, "") & "'")

                Loop

                '---------------------------------

                'Fix Inventory Table---------------
                SQL = "ALTER TABLE Inventory ADD InternalNotes text"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                SQL = "UPDATE Inventory set MaterialsClass = 'Labor' WHERE MaterialsClass = 'Difficulty'"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                '-----------------------------------


                'Add columns to MailboxSize table---------
                SQL = "ALTER TABLE MailBoxSize ADD Other1month number, Other3month number, Other6month number, Other12month number, CustomMonth number, BusinessCustomMonth number, OtherCustomMonth number, ButtonColorR number, ButtonColorG number, ButtonColorB number, TextButtonColor text"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)


                'updating pricing from monthly to total price.
                SQL = "UPDATE MailBoxSize SET [3month]= [3month] * 3, [6month]= [6month] * 6, [12month] = [12month] * 12, [Business3month]= [Business3month] * 3, [Business6month]= [Business6month] * 6, [Business12month] = [Business12month] * 12"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)




                SQL = "DELETE FROM Mailbox WHERE RENTED=false"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)


                SQL = "ALTER Table Mailbox ALTER COLUMN Business INTEGER"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)



                SQL = "SELECT MAX(ID) as MaxID FROM Policy"
                SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
                MaxID = Val(ExtractElementFromSegment("MaxID", SegmentSet)) + 1

                SQL = "SELECT ID FROM Policy WHERE ElementName = '" & _ReusedField.fldLabor_BuildUp & "'"
                SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
                If SegmentSet = "" Then
                    SQL = "INSERT INTO Policy (ID, ElementName) VALUES (" & MaxID & ", '" & _ReusedField.fldLabor_BuildUp & "')"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                    MaxID += 1
                End If

                SQL = "SELECT ID FROM Policy WHERE ElementName = '" & _ReusedField.fldLabor_CutDown & "'"
                SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
                If SegmentSet = "" Then
                    SQL = "INSERT INTO Policy (ID, ElementName) VALUES (" & MaxID & ", '" & _ReusedField.fldLabor_CutDown & "')"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                    MaxID += 1
                End If

                SQL = "SELECT ID FROM Policy WHERE ElementName = '" & _ReusedField.fldLabor_Telescope & "'"
                SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
                If SegmentSet = "" Then
                    SQL = "INSERT INTO Policy (ID, ElementName) VALUES (" & MaxID & ", '" & _ReusedField.fldLabor_Telescope & "')"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                    MaxID += 1
                End If

                SQL = "SELECT ID FROM Policy WHERE ElementName = '" & _ReusedField.fldLabor_AddTop & "'"
                SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
                If SegmentSet = "" Then
                    SQL = "INSERT INTO Policy (ID, ElementName) VALUES (" & MaxID & ", '" & _ReusedField.fldLabor_AddTop & "')"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                    MaxID += 1
                End If

                '-----------------------------------------------------------------------------------------------------------------

                SQL = "SELECT ID FROM Policy WHERE ElementName = 'MBX_Always_LateFee'"
                SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
                If SegmentSet = "" Then
                    SQL = "INSERT INTO Policy (ID, ElementName, ElementValue) VALUES (" & MaxID & ", 'MBX_Always_LateFee', 'False')"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                    MaxID += 1
                End If


                SQL = "SELECT ID FROM Policy WHERE ElementName = 'MBX_Always_AdminFee'"
                SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
                If SegmentSet = "" Then
                    SQL = "INSERT INTO Policy (ID, ElementName, ElementValue) VALUES (" & MaxID & ", 'MBX_Always_AdminFee', 'False')"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                    MaxID += 1
                End If


                SQL = "SELECT ID FROM Policy WHERE ElementName = 'MBX_Always_OtherFee'"
                SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
                If SegmentSet = "" Then
                    SQL = "INSERT INTO Policy (ID, ElementName, ElementValue) VALUES (" & MaxID & ", 'MBX_Always_OtherFee', 'False')"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                    MaxID += 1
                End If


                SQL = "SELECT ID FROM Policy WHERE ElementName = 'MBX_DefaultForwardAmount'"
                SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
                If SegmentSet = "" Then
                    SQL = "INSERT INTO Policy (ID, ElementName, ElementValue) VALUES (" & MaxID & ", 'MBX_DefaultForwardAmount', '0')"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                    MaxID += 1
                End If

                SQL = "SELECT ID FROM Policy WHERE ElementName = 'MBX_CustomMonthsNo'"
                SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
                If SegmentSet = "" Then
                    SQL = "INSERT INTO Policy (ID, ElementName, ElementValue) VALUES (" & MaxID & ", 'MBX_CustomMonthsNo', '0')"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                    MaxID += 1
                End If
                '--------------------------------------------

                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "06/12/2018")

            End If

            If CDate(SpecialUpdatesVersion) < #10/03/2018# Then
                'Update Shipping related tables to remove oudated carrier names
                SQL = "UPDATE Manifest SET [Carrier]='FedEx' WHERE [Carrier]='Federal Express'"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                SQL = "UPDATE Manifest SET [Carrier]='DHL' WHERE [Carrier]='Airborne'"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                SQL = "UPDATE Master SET [Carrier]='DHL' WHERE [Carrier]='AIRBORNE'"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                SQL = "UPDATE Master SET [Type]='DHL' WHERE [Type]='AIRBORNE  '"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                SQL = "UPDATE Master SET [Carrier]='FedEx' WHERE [Carrier]='Federal Express'"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "10/03/2018")

            End If
            If CDate(SpecialUpdatesVersion) < #11/12/2018# Then
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "11/12/2018")
                SQL = "ALTER TABLE Master ADD ACTRESIDENTIAL number"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                SQL = "UPDATE Master SET ACTRESIDENTIAL = ABResCost, ResidentialSurcharge = ABResCharge"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                SQL = "ALTER TABLE Master DROP COLUMN ABResCost"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                SQL = "ALTER TABLE Master DROP COLUMN ABResCharge"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

            End If
            If CDate(SpecialUpdatesVersion) < #11/26/2018# Then

                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "11/26/2018")
                Dim isManifestIdExist As Boolean = False
                Dim ManifestSegmentSet As String = IO_GetTableIndexes(gShipriteDB, "Manifest")

                Do Until ManifestSegmentSet = ""
                    Segment = GetNextSegmentFromSet(ManifestSegmentSet)

                    Select Case ExtractElementFromSegment("IndexName", Segment)
                        Case "PrimaryKey"
                            SQL = "DROP INDEX PrimaryKey ON Manifest"
                            ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                        Case "Dept"
                            SQL = "DROP INDEX Dept ON Manifest"
                            ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                        Case "ManifestID"
                            isManifestIdExist = True
                    End Select
                Loop

                If IO_GetFieldsCollection(gShipriteDB, "Manifest", "ID", False, False, False) = "" Then
                    SQL = "ALTER TABLE Manifest ADD ID LONG"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                End If

                If Not IO_GetFieldsCollection(gShipriteDB, "Manifest", "Counter", False, False, False) = "" Then
                    SQL = "UPDATE Manifest SET ID = COUNTER"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                    SQL = "ALTER TABLE Manifest DROP [Counter]"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                End If

                If Not isManifestIdExist Then
                    ' not found - create
                    SQL = "CREATE UNIQUE INDEX ManifestID ON Manifest (ID) WITH Primary"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                End If

                If Not IO_GetFieldsCollection(gShipriteDB, "Manifest", "Dept", False, False, False) = "" Then
                    SQL = "ALTER TABLE Manifest DROP [Dept]"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                End If

            End If

            'PackageID needs to be String - current packageid includes drawerid which can include letters and numbers.
            'If CDate(SpecialUpdatesVersion) < #01/02/2019# Then

            '    ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "01/02/2019")
            '    If InStr(1, gShipriteDB, "$") = 0 Then

            '        SQL = "ALTER TABLE Manifest ALTER COLUMN PACKAGEID LONG"

            '    Else

            '        SQL = "ALTER TABLE Manifest ALTER COLUMN PACKAGEID BIGINT"

            '    End If
            '    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

            'End If

            If CDate(SpecialUpdatesVersion) < #01/14/2019# Then
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "01/14/2019")
                Rename_Field("Master", "APBase", "DasExtCost", "Number")
                Rename_Field("Master", "APRateUpTo", "DasExtCharge", "Number")
                Rename_Field("Master", "APRateUpToCost", "DasExtCommCost", "Number")
                Rename_Field("Master", "APRateUpToCharge", "DasExtCommCharge", "Number")
                Rename_Field("Master", "ABCANFreeUpToAP", "DasIntHiCost", "Number")
                Rename_Field("Master", "ABCANRateUpToAP", "DasIntHiCharge", "Number")
                Rename_Field("Master", "ABCANEachAdditionalCostAP", "DasHiCost", "Number")
                Rename_Field("Master", "ABCANEachAdditionalChargeAP", "DasHiCharge", "Number")
                Rename_Field("Master", "ACTAPINC", "DasAkCost", "Number")
                Rename_Field("Master", "ABCANEachAdditionalAP", "DasAkCharge", "Number")
                Rename_Field("Master", "ABCANFreeUpToDV", "DasHomeDelCost", "Number")
                Rename_Field("Master", "ABCANRateUpToDV", "DasHomeDelCharge", "Number")
                Rename_Field("Master", "ABCANUpToCostDV", "DasExtHomeDelCost", "Number")
                Rename_Field("Master", "ABCANUpToChargeDV", "DasExtHomeDelCharge", "Number")
                Rename_Field("Master", "aABCodMinimum", "ResHomeCost", "Number")
                Rename_Field("Master", "ABCodMinimum", "ResHomeCharge", "Number")
            End If


            If CDate(SpecialUpdatesVersion) < #01/17/2019# Then
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "01/17/2019")
                Rename_Field("Master", "ABSATRED", "OVS2_Cost", "Number")
                Rename_Field("Master", "ABSATPURED", "OVS2_Charge", "Number")
                Rename_Field("Master", "ABSATBLACK", "OVS3_Cost", "Number")
                Rename_Field("Master", "ABSATPUBLACK", "OVS3_Charge", "Number")
                Rename_Field("Master", "ABCANUpToCostAP", "OVS4_Cost", "Number")
                Rename_Field("Master", "ABCANUpToChargeAP", "OVS4_Charge", "Number")
                Rename_Field("Master", "ABBlackResCost", "OVS5_Cost", "Number")
                Rename_Field("Master", "ABBlackResCharge", "OVS5_Charge", "Number")
                Rename_Field("Master", "ACTAP", "OVS6_Cost", "Number")
                Rename_Field("Master", "AP", "OVS6_Charge", "Number")
            End If

            If CDate(SpecialUpdatesVersion) < #02/08/2019# Then
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "02/08/2019")

                SQL = "CREATE TABLE CustomsItems (ID AUTOINCREMENT PRIMARY KEY, PackageID TEXT NOT NULL,Quantity NUMBER, Description TEXT, Weight NUMBER, ItemValue NUMBER, OriginCountry TEXT)"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                SQL = "CREATE INDEX indxPackageID ON CustomsItems (PackageID)"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                CopyCustomsFromManifest()

                SQL = "ALTER TABLE Manifest DROP COLUMN CustomsDESC, CustomsDESC2, CustomsDESC3, CustomsDESC4, CustomsDESC5, 
CustomsVALUE, CustomsVALUE2, CustomsVALUE3, CustomsVALUE4, CustomsVALUE5, 
CustomsQty, CustomsQty2, CustomsQty3, CustomsQty4, CustomsQty5,
CustomsWeight, CustomsWeight2, CustomsWeight3, CustomsWeight4, CustomsWeight5,
CustomsCountryOfOrigin, CustomsCountryOfOrigin2, CustomsCountryOfOrigin3, CustomsCountryOfOrigin4, CustomsCountryOfOrigin5"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
            End If

            If CDate(SpecialUpdatesVersion) < #02/25/2019# Then
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "02/25/2019")

                SQL = "ALTER TABLE Departments DROP COLUMN DeptHead, DeptPhone, GlobalMarkUp"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

            End If


            If CDate(SpecialUpdatesVersion) < #03/14/2019# Then
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "03/14/2019")

                SQL = "SELECT MAX(ID) as MaxID FROM Policy"
                SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
                MaxID = Val(ExtractElementFromSegment("MaxID", SegmentSet)) + 1



                SQL = "SELECT ID FROM Policy WHERE ElementName = 'PrintTotalCash'"
                SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
                If SegmentSet = "" Then
                    SQL = "INSERT INTO Policy (ID, ElementName, ElementValue) VALUES (" & MaxID & ", 'PrintTotalCash', '1')"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                    MaxID += 1
                End If



                SQL = "SELECT ID FROM Policy WHERE ElementName = 'PrintTotalCheck'"
                SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
                If SegmentSet = "" Then
                    SQL = "INSERT INTO Policy (ID, ElementName, ElementValue) VALUES (" & MaxID & ", 'PrintTotalCheck', '1')"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                    MaxID += 1
                End If



                SQL = "SELECT ID FROM Policy WHERE ElementName = 'PrintTotalOther'"
                SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
                If SegmentSet = "" Then
                    SQL = "INSERT INTO Policy (ID, ElementName, ElementValue) VALUES (" & MaxID & ", 'PrintTotalOther', '1')"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                    MaxID += 1
                End If

                SQL = "SELECT ID FROM Policy WHERE ElementName = 'ReceiptLink1'"
                SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
                If SegmentSet = "" Then
                    SQL = "INSERT INTO Policy (ID, ElementName, ElementValue) VALUES (" & MaxID & ", 'ReceiptLink1', '')"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                    MaxID += 1
                End If

                SQL = "SELECT ID FROM Policy WHERE ElementName = 'ReceiptLink2'"
                SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
                If SegmentSet = "" Then
                    SQL = "INSERT INTO Policy (ID, ElementName, ElementValue) VALUES (" & MaxID & ", 'ReceiptLink2', '')"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                    MaxID += 1
                End If

                SQL = "SELECT ID FROM Policy WHERE ElementName = 'ReceiptLink3'"
                SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
                If SegmentSet = "" Then
                    SQL = "INSERT INTO Policy (ID, ElementName, ElementValue) VALUES (" & MaxID & ", 'ReceiptLink3', '')"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                    MaxID += 1
                End If
            End If

            If CDate(SpecialUpdatesVersion) < #03/26/2019# Then
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "03/26/2019")

                SQL = "ALTER TABLE CustomsItems ADD HarmonizedCode text"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

            End If

            If CDate(SpecialUpdatesVersion) < #03/29/2019# Then
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "03/29/2019")

                'Change Service SKU's for Retail Ground and Parcel Select
                SQL = "UPDATE Master Set DESCRIPTION='Parcel Select' WHERE SERVICE='USPS-4TH-NOSC'"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                SQL = "UPDATE Master Set DESCRIPTION='Retail Ground' WHERE SERVICE='USPS-4TH'"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                SQL = "UPDATE Master Set SERVICE='USPS-PS' WHERE SERVICE='USPS-4TH-NOSC'"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                SQL = "UPDATE Master Set SERVICE='USPS-RG' WHERE SERVICE='USPS-4TH'"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)



                ' Change service code in pricing matrix
                SQL = "UPDATE PricingMatrix Set Service='USPS-RG' WHERE Service='USPS-4TH'"
                ret = IO_UpdateSQLProcessor(gPricingMatrixDB, SQL)

                SQL = "UPDATE PricingMatrix Set Service='USPS-PS' WHERE Service='USPS-4TH-NOSC'"
                ret = IO_UpdateSQLProcessor(gPricingMatrixDB, SQL)


                'Change Service code in Manifest Table
                SQL = "UPDATE Manifest Set P1='USPS-PS' WHERE P1='USPS-4TH-NOSC'"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                SQL = "UPDATE Manifest Set P1='USPS-RG' WHERE P1='USPS-4TH'"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)



                'Add new field to inventory for Non_taxable status
                SQL = "ALTER TABLE Inventory ADD COLUMN Non_Taxable BIT DEFAULT 0"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
            End If

            If CDate(SpecialUpdatesVersion) < #04/15/2019# Then
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "04/15/2019")

                SQL = "ALTER TABLE TimeClock ADD COLUMN Notes Text(30)"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

            End If

            If CDate(SpecialUpdatesVersion) < #04/25/2019# Then
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "04/25/2019")

                SQL = "ALTER TABLE Tickler ADD COLUMN SKU Text(50)"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
            End If


            If CDate(SpecialUpdatesVersion) < #05/08/2019# Then
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "05/08/2019")

                SQL = "ALTER TABLE Tickler ADD COLUMN Notes Text(250)"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                UpdateTicklerTable()

            End If

            If CDate(SpecialUpdatesVersion) < #05/15/2019# Then

                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "05/15/2019")

                SQL = "Update Tickler Set Priority='Urgent' WHERE Priority='Emergency'"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                SQL = "Update Tickler Set Priority='Low' WHERE Priority='No Action'"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                SQL = "ALTER TABLE Tickler ADD COLUMN Repeat Text(50)"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                SQL = "ALTER TABLE Tickler ADD COLUMN RepeatPeriod Text(50)"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

            End If

            If CDate(SpecialUpdatesVersion) < #05/16/2019# Then
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "05/16/2019")

                SQL = "ALTER TABLE Tickler ADD COLUMN Repeat_LastCreated DATE"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

            End If

            If CDate(SpecialUpdatesVersion) < #06/05/2019# Then
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "06/05/2019")

                If IO_GetFieldsCollection(gShipriteDB, "InvoiceNotes", "InvNum", False, False, False) = "" Then
                    SQL = "ALTER TABLE InvoiceNotes ADD InvNum VARCHAR(24)"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                End If

                If IO_GetFieldsCollection(gShipriteDB, "InvoiceNotes", "Note", False, False, False) = "" Then
                    SQL = "ALTER TABLE InvoiceNotes ADD [Note] LONGTEXT"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                End If


                SQL = "CREATE INDEX IDX_InvNum ON InvoiceNotes (InvNum)"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

            End If

            If CDate(SpecialUpdatesVersion) < #07/12/2019# Then
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "07/12/2019")

                SQL = "ALTER TABLE Contacts ADD COLUMN Addr3 Text(128)"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

            End If

            If CDate(SpecialUpdatesVersion) < #08/13/2019# Then
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "08/13/2019")

                SQL = "ALTER TABLE Payments ADD COLUMN SaleAmount DOUBLE"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                SQL = "ALTER TABLE Payments ADD COLUMN Balance DOUBLE"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                SQL = "ALTER TABLE OpenClose ADD COLUMN DrawerIsOpen YESNO"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                SQL = "SELECT MAX(ID) AS MaxID From Policy"
                buf = IO_GetSegmentSet(gShipriteDB, SQL)
                ret = Val(ExtractElementFromSegment("MaxID", buf))
                ret = ret + 1

                SQL = "INSERT INTO Policy (ID, ElementName, ElementValue) VALUES (" & ret.ToString & ", 'EnableDrawerOpenClose', 'True')"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                ret += 1
                SQL = "INSERT INTO Policy (ID, ElementName, ElementValue) VALUES (" & ret.ToString & ", 'DrawerIsOpen', 'False')"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL, , True)

                SQL = "Update OpenClose set DrawerIsOpen=DrawerOpen"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL, , True)

            End If

            If CDate(SpecialUpdatesVersion) < #08/20/2019# Then

                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "08/20/2019")
                SQL = "UPDATE Inventory SET Department = 'RETAIL' WHERE ISNULL(Department) OR Department = ''"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

            End If

            If CDate(SpecialUpdatesVersion) < #10/17/2019# Then ' TODO: Remove before release? Unnecessary for conversion - should already be a string.

                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "10/17/2019")
                If InStr(1, gShipriteDB, "$") = 0 Then ' MS Access

                    SQL = "ALTER TABLE Manifest ALTER COLUMN PACKAGEID VARCHAR(20)" ' current srpro field size is 20

                Else ' SQL Server

                    SQL = "ALTER TABLE Manifest ALTER COLUMN PACKAGEID VARCHAR(20)"

                End If
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

            End If

            If CDate(SpecialUpdatesVersion) < #2/3/2020# Then
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "2/3/2020")
                SQL = "ALTER TABLE Manifest ADD CertifiedMail number"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                SQL = "ALTER TABLE Manifest ADD costCertifiedMail number"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                SQL = "ALTER TABLE Manifest ADD ReturnReceipt number"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                SQL = "ALTER TABLE Manifest ADD costReturnReceipt number"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

            End If

            If CDate(SpecialUpdatesVersion) < #2/10/2020# Then
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "2/10/2020")

                SQL = "ALTER TABLE Manifest DROP column CgnID"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                SQL = "ALTER TABLE Manifest DROP column ShpID"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

            End If

            If CDate(SpecialUpdatesVersion) < #4/10/2020# Then
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "4/10/2020")

                SQL = "ALTER TABLE Master ADD COLUMN Panel_Row number"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                SQL = "ALTER TABLE Master ADD COLUMN Panel_Row_Canada number"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                SQL = "ALTER TABLE Master ADD COLUMN Panel_Row_Intl number"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                SQL = "ALTER TABLE Master ADD COLUMN Panel_Row_Freight number"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                SQL = "ALTER TABLE Master ADD COLUMN Panel_Column number"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                SQL = "ALTER TABLE Master ADD COLUMN Panel_Column_Canada number"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                SQL = "ALTER TABLE Master ADD COLUMN Domestic_Status number"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                SQL = "ALTER TABLE Master ADD COLUMN Canada_Status number"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                SQL = "ALTER TABLE Master ADD COLUMN Intl_Status number"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                SQL = "ALTER TABLE Master ADD COLUMN Freight_Status number"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)



                'Delete old and unused DHL Services
                SQL = "DELETE FROM Master WHERE SERVICE='@SDS' or SERVICE='Ten30' or SERVICE='NAS' or SERVICE='SDS' or SERVICE='@GDS' or SERVICE='EXP' or SERVICE='GDS' or SERVICE='SPEEDY' or SERVICE='USPS-IBOK'"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                Check_Missing_MasterTable_Services()
                Check_FreightServices()
                Assign_Service_Names()
                Assign_Default_ShipPanel_ButtonPositions()

            End If

            If CDate(SpecialUpdatesVersion) < #5/7/2020# Then
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "5/7/2020")
                SegmentSet = IO_GetTableIndexes(gShipriteDB, "Holiday")

                Do Until SegmentSet = ""
                    Segment = GetNextSegmentFromSet(SegmentSet)

                    Select Case ExtractElementFromSegment("IndexName", Segment)
                        Case "JulianDate"
                            SQL = "DROP INDEX JulianDate ON Holiday"
                            ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                    End Select
                Loop

                If Not IO_GetFieldsCollection(gShipriteDB, "Holiday", "JulianDate", False, False, False) = "" Then
                    SQL = "ALTER TABLE Holiday DROP COLUMN JulianDate"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                End If

            End If


            If CDate(SpecialUpdatesVersion) < #5/18/2020# Then
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "5/18/2020")

                SQL = "Update Policy set ElementName='Enable_RetailFedExLevel' Where Trim(ElementName)='Enable_FedEx_FASC'"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
            End If

            If CDate(SpecialUpdatesVersion) < #5/20/2020# Then
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "5/20/2020")

                SQL = "Update Policy set ElementName='Enable_USPS_ApprovedShipper' Where Trim(ElementName)='EndAfterKiosk'"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
            End If

            If CDate(SpecialUpdatesVersion) < #7/29/2020# Then
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "7/29/2020")

                SQL = "Delete * From Inventory Where Department='COUPONS'"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                SQL = "ALTER TABLE Inventory ADD COLUMN Coupon_TypeOf Text"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                SQL = "INSERT INTO Inventory(SKU, [Desc], Department, Coupon_AppliesTo, Coupon_Savings, Coupon_StartDate, Coupon_EndDate, Coupon_Limit, Coupon_TypeOf, Active) 
                        SELECT SKU, Description, 'COUPONS', AffectedInventory, SavingsMethod, StartDate, EndDate, Limit, TypeOfCoupon, Activated FROM Coupons"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)


                SQL = "DROP Table Coupons"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                FixCouponList()

            End If
            If CDate(SpecialUpdatesVersion) < #9/29/2020# Then
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "9/29/2020")
                SQL = "ALTER TABLE Transactions ADD COLUMN Returned YESNO"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

            End If

            If CDate(SpecialUpdatesVersion) < #10/15/2020# Then
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "10/15/2020")

                SQL = "Update Policy Set ElementName='IsDHLMarkupDiscount' WHERE ElementName='HideEstCharges'"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                SQL = "Update Policy Set ElementName='AlwaysChargeDhlRetail' WHERE ElementName='EnableKioskExitSecurity'"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
            End If

            If CDate(SpecialUpdatesVersion) < #10/27/2020# Then
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "10/27/2020")

                ret = UpdatePolicy(gReportsDB, "BackupPath1", gAppPath & "\Backup")

                _Files.Create_Folder(gAppPath & "\Backup", True)

            End If
            If CDate(SpecialUpdatesVersion) < #02/10/2021# Then

                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "02/10/2021")

                Dim isqCloseidIndexExist As Boolean = False

                SegmentSet = IO_GetTableIndexes(gShipriteDB, "Transactions")
                Do Until SegmentSet = ""
                    Segment = GetNextSegmentFromSet(SegmentSet)

                    If ExtractElementFromSegment("IndexName", Segment) = "IDX_qCloseID" Then
                        isqCloseidIndexExist = True
                        Exit Do
                    End If
                Loop
                If Not isqCloseidIndexExist Then
                    SQL = "CREATE INDEX IDX_qCloseID ON Transactions (qCloseID)"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                End If

                isqCloseidIndexExist = False
                SegmentSet = IO_GetTableIndexes(gShipriteDB, "Payments")
                Do Until SegmentSet = ""
                    Segment = GetNextSegmentFromSet(SegmentSet)

                    If ExtractElementFromSegment("IndexName", Segment) = "IDX_qCloseID" Then
                        isqCloseidIndexExist = True
                        Exit Do
                    End If
                Loop
                If Not isqCloseidIndexExist Then
                    SQL = "CREATE INDEX IDX_qCloseID ON Payments (qCloseID)"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                End If

            End If
            'If CDate(SpecialUpdatesVersion) < #3/2/2021# Then
            '    ' Profile Image Support

            '    If IO_GetFieldsCollection(gShipriteDB, "Contacts", "ProfileImage", False, False, True) = "" Then
            '        SQL = "ALTER Table Contacts ADD COLUMN ProfileImage OLEOBJECT"
            '        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
            '    End If

            '    ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "3/2/2021")

            'End If
            If CDate(SpecialUpdatesVersion) < #4/1/2021# Then

                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "4/1/2021")

                buf = GetPolicyData(gShipriteDB, "CouponText")
                ret = UpdatePolicy(gShipriteDB, "ReceiptSignatureText", buf)
                ret = UpdatePolicy(gShipriteDB, "CouponText", "")

                buf = GetPolicyData(gShipriteDB, "InvoiceStatement")
                ret = UpdatePolicy(gShipriteDB, "ShippingDisclaimer", buf)
                ret = UpdatePolicy(gShipriteDB, "InvoiceStatement", "")

                buf = GetPolicyData(gShipriteDB, "PaperlessShipping")
                ret = UpdatePolicy(gShipriteDB, "EnableShippingDisclaimer", buf)
                ret = UpdatePolicy(gShipriteDB, "PaperlessShipping", "")

            End If
            If CDate(SpecialUpdatesVersion) < #4/6/2021# Then

                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "4/6/2021")
                buf = IO_GetFieldsCollection(gShipriteDB, "Payments", "CCEndBlock", False, False, True)
                If buf = "" Then

                    SQL = "ALTER TABLE Payments ADD CCEndBlock MEMO"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                End If
            End If
            If CDate(SpecialUpdatesVersion) < #5/21/2021# Then
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "5/21/2021")
                buf = IO_GetFieldsCollection(gShipriteDB, "MBXHistory", "Clerk", False, False, True)
                If buf = "" Then
                    SQL = "ALTER TABLE MBXHistory ADD Clerk TEXT(255)"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                End If
            End If
            If CDate(SpecialUpdatesVersion) < #6/1/2021# Then
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "6/1/2021")
                buf = IO_GetFieldsCollection(gShipriteDB, "Tickler", "OpenedBy", False, False, True)
                If buf = "" Then
                    SQL = "ALTER TABLE Tickler ADD OpenedBy TEXT(255)"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                End If
            End If
            If CDate(SpecialUpdatesVersion) < #8/12/2021# Then
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "8/12/2021")

                buf = GetPolicyData(gShipriteDB, "MessagePlus120")
                If Not String.IsNullOrWhiteSpace(buf) Then
                    If buf.ToLower.Contains("vito") And buf.ToLower.Contains("leg") And buf.ToLower.Contains("breaker") Then
                        ret = UpdatePolicy(gShipriteDB, "MessagePlus120", "We are referring your account to collections.  Please remit immediately.")
                    End If
                End If
            End If
            If CDate(SpecialUpdatesVersion) < #08/31/2021# Then

                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "08/31/2021")

                If IO_GetFieldsCollection(gShipriteDB, "Tickler", "Repeat", False, False, True) = "" Then

                    SQL = "ALTER TABLE Tickler ADD Repeat TEXT(55)"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                    SQL = "ALTER TABLE Tickler ADD RepeatPeriod TEXT(55)"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                    SQL = "ALTER TABLE Tickler ADD Repeat_LastCreated DATETIME"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                End If

            End If
            If CDate(SpecialUpdatesVersion) < #9/9/2021# Then
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "9/9/2021")
                UpdatePolicy(gShipriteDB, "Enable_Pricing_Matrix", "False")
                UpdatePolicy(gShipriteDB, "Enable_Auto_TimeInTransit", "False")
            End If

            If CDate(SpecialUpdatesVersion) < #05/23/2022# Then
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "05/23/2022")


                If IO_GetFieldsCollection(gShipriteDB, "ZReport", "ChargeCards", False, False, True) = "" Then
                    SQL = "ALTER Table ZReport ADD COLUMN ChargeCards DOUBLE"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                End If

                If IO_GetFieldsCollection(gShipriteDB, "ZReport", "Other", False, False, True) = "" Then
                    SQL = "ALTER Table ZReport ADD COLUMN Other DOUBLE"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                End If


            End If

            If CDate(SpecialUpdatesVersion) < #09/07/2022# Then
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "09/07/2022")
                SQL = "ALTER Table Mailbox ADD COLUMN CustomRates Text"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

            End If


            If CDate(SpecialUpdatesVersion) < #09/20/2022# Then
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "09/20/2022")
                SQL = "ALTER Table Transactions ADD COLUMN UnitCost Number"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                SQL = "ALTER Table Contacts ALTER COLUMN Email Text(255)"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

            End If

            If CDate(SpecialUpdatesVersion) < #10/25/2022# Then
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "10/25/2022")

                SQL = "ALTER Table Transactions ADD COLUMN ReturnedQty Number Default 0"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                If Not IO_GetFieldsCollection(gShipriteDB, "Transactions", "Returned", False, False, False) = "" Then
                    SQL = "ALTER TABLE Transactions DROP [Returned]"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                End If

            End If

            If CDate(SpecialUpdatesVersion) < #02/06/2023# Then
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "02/06/2023")

                SQL = "ALTER Table Contacts ALTER COLUMN State Text(35)"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

            End If

            If CDate(SpecialUpdatesVersion) < #03/16/2023# Then
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "03/16/2023")

                SQL = "Update Payments set Payment=0 where ISNULL(Payment) and Type='Sale'"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

            End If

            If CDate(SpecialUpdatesVersion) < #04/07/2023# Then
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "04/07/2023")

                SQL = "Update Policy set ElementName='ENABLE_USPS_SRPRO_Rate' WHERE ElementName='Enable_USPSRSARate'"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

            End If

            If CDate(SpecialUpdatesVersion) < #4/12/2023# Then
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "04/12/2023")

                UpdatePolicy(gShipriteDB, "Address_Verification_Service", "0")

            End If

            If CDate(SpecialUpdatesVersion) < #06/01/2023# Then
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "06/01/2023")

                Rename_Field("Master", "ABCANEachAdditionalCostDV", "OVS7_Cost", "Number")
                Rename_Field("Master", "ABCANEachAdditionalChargeDV", "OVS7_Charge", "Number")

                Rename_Field("Master", "ABCANEachAdditionalDV", "OVS8_Cost", "Number")
                Rename_Field("Master", "ABLightDASCharge", "OVS8_Charge", "Number")
            End If

            'If CDate(SpecialUpdatesVersion) < #06/27/2023# Then
            '    ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "06/27/2023")

            '    SQL = "CREATE TABLE Setup (ID AUTOINCREMENT PRIMARY KEY, Name VarChar(255), FName VarChar(255), LName VarChar(255), Addr1 VarChar(255), Addr2 VarChar(255), City VarChar(255), State VarChar(255), Zip VarChar(255), Phone1 VarChar(255))"
            '    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

            '    SQL = "INSERT INTO SETUP (Name, FName, LName, Addr1, Addr2, City, State, Zip, Phone1) VALUES(" &
            '        "'" & GetPolicyData(gShipriteDB, "Name", "") & "', " &
            '        "'" & GetPolicyData(gShipriteDB, "FName", "") & "', " &
            '        "'" & GetPolicyData(gShipriteDB, "LName", "") & "', " &
            '        "'" & GetPolicyData(gShipriteDB, "Addr1", "") & "', " &
            '        "'" & GetPolicyData(gShipriteDB, "Addr2", "") & "', " &
            '        "'" & GetPolicyData(gShipriteDB, "City", "") & "', " &
            '        "'" & GetPolicyData(gShipriteDB, "State", "") & "', " &
            '        "'" & GetPolicyData(gShipriteDB, "Zip", "") & "', " &
            '        "'" & GetPolicyData(gShipriteDB, "Phone1", "") & "')"

            '    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
            'End If

            If CDate(SpecialUpdatesVersion) < #8/21/2023# Then

                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "8/21/2023")

                If Not IsService_Exist_Master("USPS-GND-ADV", gShipriteDB) Then
                    If Not add_Service2Master("Ground Advantage®", "USPS-GND-ADV", "USPS", "USPS-4TH", gShipriteDB) Then
                        add_Service2Master("Ground Advantage®", "USPS-GND-ADV", "USPS", "USPS-PRI", gShipriteDB)
                        AssignPosition("USPS-GND-ADV", 6)
                    End If
                End If
            End If

            If CDate(SpecialUpdatesVersion) < #9/1/2023# Then

                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "9/1/2023")

                If Not System.IO.Directory.Exists(gDBpath & "\Ads") Then
                    System.IO.Directory.CreateDirectory(gDBpath & "\Ads")
                    System.IO.Directory.CreateDirectory(gDBpath & "\Ads\Logo")
                    System.IO.Directory.CreateDirectory(gDBpath & "\Ads\POS")
                End If

            End If

            If CDate(SpecialUpdatesVersion) < #10/20/2023# Then
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "10/20/2023")

                SQL = "ALTER TABLE CustomsItems ALTER COLUMN PACKAGEID VARCHAR(20)"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

            End If

            If CDate(SpecialUpdatesVersion) < #11/01/2023# Then
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "11/01/2023")

                If IO_GetFieldsCollection(gShipriteDB, "MailBox", "ID1_IssuingEntity", False, False, False) = "" Then
                    SQL = "ALTER Table Mailbox ADD COLUMN ID1_IssuingEntity VarChar(255)"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                End If

                If IO_GetFieldsCollection(gShipriteDB, "Mailbox", "ID1_ExpDate", False, False, False) = "" Then
                    SQL = "ALTER Table Mailbox ADD COLUMN ID1_ExpDate VarChar(255)"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                End If

                If IO_GetFieldsCollection(gShipriteDB, "Mailbox", "ID1_Type", False, False, False) = "" Then
                    SQL = "ALTER Table Mailbox ADD COLUMN ID1_Type VarChar(255)"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                End If

                If IO_GetFieldsCollection(gShipriteDB, "Mailbox", "ID2_Type", False, False, False) = "" Then
                    SQL = "ALTER Table Mailbox ADD COLUMN ID2_Type VarChar(255)"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                End If

                If IO_GetFieldsCollection(gShipriteDB, "Mailbox", "PlaceOfRegistration", False, False, False) = "" Then
                    SQL = "ALTER Table Mailbox ADD COLUMN PlaceOfRegistration VarChar(255)"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                End If

                If IO_GetFieldsCollection(gShipriteDB, "Mailbox", "isBusiness", False, False, False) = "" Then
                    SQL = "ALTER Table Mailbox ADD COLUMN isBusiness YesNo"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                End If

            End If

            If CDate(SpecialUpdatesVersion) < #11/13/2023# Then
                Update_FedEx_FASC_Tiers_Ind_20231116()
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "11/13/2023")
            End If


            If CDate(SpecialUpdatesVersion) < #1/11/2024# Then
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "1/11/2024")

                If IsService_Exist_Master("USPS-PS", gShipriteDB) Then
                    SQL = "Delete From Master WHERE SERVICE='USPS-PS'"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                End If

                If IsService_Exist_Master("USPS-RG", gShipriteDB) Then
                    SQL = "Delete From Master WHERE SERVICE='USPS-RG'"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                End If

                If IsService_Exist_Master("DHL-INT-DOC", gShipriteDB) Then
                    SQL = "Update Master Set Description='DHL Express WW Document' WHERE SERVICE='DHL-INT-DOC'"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                End If

            End If

            If CDate(SpecialUpdatesVersion) < #1/23/2024# Then
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "1/23/2024")

                If IO_GetFieldsCollection(gPackagingDB, "CarrierPackagingFlatRateValues", "BaseRetail", False, False, False) = "" Then
                    SQL = "ALTER Table CarrierPackagingFlatRateValues ADD COLUMN BaseRetail Number"
                    ret = IO_UpdateSQLProcessor(gPackagingDB, SQL)
                End If
            End If

            If CDate(SpecialUpdatesVersion) < #1/31/2024# Then
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "1/31/2024")

                UpdatePolicy(gShipriteDB, "ShippingDisclaimer_2ndReceipt", "True")
            End If


            If CDate(SpecialUpdatesVersion) < #2/9/2024# Then
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "2/9/2024")

                SQL = "ALTER TABLE Manifest ALTER COLUMN Packaging VARCHAR(35)"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
            End If

            If CDate(SpecialUpdatesVersion) < #2/16/2024# Then

                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "2/16/2024")
                If IO_GetFieldsCollection(gShipriteDB, "ar", "DateOfLastCalculation", False, False, False) = "" Then
                    SQL = "alter Table ar add DateOfLastCalculation DATE"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                End If

            End If
            If CDate(SpecialUpdatesVersion) < #2/29/2024# Then

                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "2/29/2024")
                ret = ProcessIndexValidation()

            End If

            If CDate(SpecialUpdatesVersion) < #3/5/2024# Then

                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "3/5/2024")
                buf = IO_GetFieldsCollection(gShipriteDB, "Transactions", "PackageID", False, False, True)
                If buf = "" Then

                    SQL = "ALTER TABLE Transactions ADD PackageID VarChar(50)"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                End If
            End If
            If CDate(SpecialUpdatesVersion) < #3/8/2024# Then

                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "3/8/2024")

                Dim PIDrecordset As RecordSetDefinition
                Dim RecordCT As Long
                Dim i As Long
                Dim ID As String
                Dim PID As String
                Dim LastPID As String
                Dim NewPID As String
                Dim SoldTo As String

                SQL = "SELECT ID, PackageID, CID FROM Manifest ORDER BY PackageID"
                RecordCT = IO_GetSegmentSetInToStructure(gShipriteDB, SQL, PIDrecordset)
                LastPID = "X"
                For i = 0 To RecordCT - 1

                    ID = PIDrecordset.RecordSet(i).Field(0).FValue
                    PID = PIDrecordset.RecordSet(i).Field(1).FValue
                    SoldTo = PIDrecordset.RecordSet(i).Field(2).FValue
                    If Not PID = LastPID Then

                        LastPID = PIDrecordset.RecordSet(i).Field(1).FValue

                    Else

                        NewPID = GetPackageID()
                        SQL = "UPDATE Manifest SET PackageID = '" & NewPID & "' WHERE ID = " & ID
                        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                        SQL = "UPDATE Transactions SET PackageID = '" & NewPID & "' WHERE PackageID = '" & PID & "' AND SoldTo = " & SoldTo
                        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                    End If

                Next i

            End If
            If CDate(SpecialUpdatesVersion) < #3/18/2024# Then

                Dim ID As Long

                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "3/18/2024")

                SQL = "CREATE UNIQUE INDEX IDX_SKU ON Inventory (SKU)"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL, "", True)

                SQL = "SELECT Max(ID) AS MaxID FROM Inventory"
                SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
                ID = Val(ExtractElementFromSegment("MaxID", SegmentSet)) + 1
                SQL = "SELECT Service, Description, PosDept FROM Master"
                SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)

                Do Until SegmentSet = ""

                    Segment = GetNextSegmentFromSet(SegmentSet)
                    SQL = "INSERT INTO Inventory (ID, SKU, [DESC], Department) VALUES (" & ID.ToString & ", '" &
                        ExtractElementFromSegment("Service", Segment) & "', '" &
                        ExtractElementFromSegment("Description", Segment) & "', '" &
                        ExtractElementFromSegment("PosDept", Segment) & "')"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL, "", True)

                Loop

            End If

            If CDate(SpecialUpdatesVersion) < #4/13/2024# Then
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "4/13/2024")

                If IO_GetFieldsCollection(gShipriteDB, "Master", "ACTAP", False, False, False) = "" Then
                    'Fields were renamed by accident, need to re-add them back in.
                    SQL = "alter Table Master add ACTAP Number"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                    SQL = "alter Table Master add AP Number"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                    SQL = "Update Master Set ACTAP=14.25, AP=14.25 WHERE Carrier='FedEx' or Carrier='UPS'"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                End If
            End If

            If CDate(SpecialUpdatesVersion) < #5/2/2024# Then
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "5/2/2024")

                Dim oldUpsApValue As String = GetPolicyData(gShipriteDB, "ATTPIDMM", "")
                If Not String.IsNullOrWhiteSpace(oldUpsApValue) Then
                    ' old UPS AP field exists and has data
                    Dim newUpsApValue As String = GetPolicyData(gShipriteDB, "UPS_AccessID", "")
                    If String.IsNullOrWhiteSpace(newUpsApValue) Then
                        ' new UPS AP field exist and doesn't have data
                        ' copy data from old to new
                        ret = UpdatePolicy(gShipriteDB, "UPS_AccessID", oldUpsApValue)
                    End If

                End If
            End If

            If CDate(SpecialUpdatesVersion) < #5/9/2024# Then
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "5/9/2024")

                SQL = "UPDATE Transactions, Inventory SET Transactions.Dept = Inventory.Department WHERE Transactions.SKU <> 'NOTE' AND Transactions.SKU <> 'MEMO' AND (ISNULL(Transactions.Dept) OR Transactions.Dept = '') AND Transactions.SKU = Inventory.SKU"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
            End If

            If CDate(SpecialUpdatesVersion) < #5/23/2024# Then
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "5/23/2024")

                If IO_GetFieldsCollection(gShipriteDB, "MBXNamesList", "FormOfID1", False, False, False) = "" Then
                    'PS1583 items need to be moved to MBXNamesList tables so that we can print individual PS1583's for each name on the mailbox

                    'create new columns
                    SQL = "Alter Table MBXNamesList ADD FormOfID1 VarChar(255)"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                    SQL = "Alter Table MBXNamesList ADD ID1_Type VarChar(255)"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                    SQL = "Alter Table MBXNamesList ADD ID1_IssuingEntity VarChar(255)"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                    SQL = "Alter Table MBXNamesList ADD ID1_ExpDate VarChar(255)"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                    SQL = "Alter Table MBXNamesList ADD FormOfID2 VarChar(255)"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                    SQL = "Alter Table MBXNamesList ADD ID2_Type VarChar(255)"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                    'MOVE existing data over
                    SQL = "UPDATE MBXNamesList INNER JOIN MailBox ON mbxnameslist.CID = MailBox.CID SET
MBXNamesList.FormOfID1 = MailBox.FormOfID1,
MBXNamesList.ID1_Type = MailBox.ID1_Type,
MBXNamesList.ID1_IssuingEntity = MailBox.ID1_IssuingEntity,
MBXNamesList.ID1_ExpDate = MailBox.ID1_ExpDate,
MBXNamesList.FormOfID2 = MailBox.FormOfID2,
MBXNamesList.ID2_Type = MailBox.ID2_Type
WHERE MBXNamesList.cid=MailBox.cid"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)


                    'DELTE old columns
                    SQL = "Alter Table Mailbox Drop COLUMN FormOfID1"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                    SQL = "Alter Table Mailbox DROP COLUMN ID1_Type"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                    SQL = "Alter Table Mailbox DROP COLUMN ID1_IssuingEntity"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                    SQL = "Alter Table Mailbox DROP COLUMN  ID1_ExpDate"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                    SQL = "Alter Table Mailbox DROP COLUMN FormOfID2"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                    SQL = "Alter Table Mailbox DROP COLUMN ID2_Type"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                End If
            End If

            If CDate(SpecialUpdatesVersion) < #6/5/2024# Then
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "6/5/2024")

                Dim DHL_Disc As String = GetPolicyData(gShipriteDB, "DHL_INTL_RATETABLE", "")
                DHL_Disc = UCase(DHL_Disc)

                If DHL_Disc = "60%" Then
                    UpdatePolicy(gShipriteDB, "DHL_INTL_RATETABLE", "TIER1")
                ElseIf DHL_Disc = "63%" Then
                    UpdatePolicy(gShipriteDB, "DHL_INTL_RATETABLE", "TIER2")
                ElseIf DHL_Disc = "TIER3" Then
                    UpdatePolicy(gShipriteDB, "DHL_INTL_RATETABLE", "TIER2")
                End If

            End If

            If CDate(SpecialUpdatesVersion) < #6/14/2024# Then
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "6/14/2024")
                Dim SpeeDeeAcctNum As String = ""

                SQL = "select [Shipper#] From Master Where SERVICE='SPEEDEE-GND'"
                SpeeDeeAcctNum = ExtractElementFromSegment("Shipper#", IO_GetSegmentSet(gShipriteDB, SQL), "")

                If SpeeDeeAcctNum <> "" Then
                    UpdatePolicy(gShipriteDB, "SpeeDeeAccountNumber", SpeeDeeAcctNum)
                End If

            End If

            If CDate(SpecialUpdatesVersion) < #07/09/2024# Then
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "07/09/2024")

                ' reapply changes from Special Update 06/27/2023 with correct field types

                buf = IO_GetTableCollection(gShipriteDB, "Setup")
                If Not buf = "" Then
                    SQL = "DROP TABLE Setup"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                End If

                SQL = "CREATE TABLE Setup (ID AUTOINCREMENT PRIMARY KEY, Name VarChar(255), FName VarChar(255), LName VarChar(255), Addr1 VarChar(255), Addr2 VarChar(255), City VarChar(255), State VarChar(255), Zip VarChar(255), Phone1 VarChar(255))"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                SQL = "INSERT INTO SETUP (Name, FName, LName, Addr1, Addr2, City, State, Zip, Phone1) VALUES(" &
                    "'" & GetPolicyData(gShipriteDB, "Name", "") & "', " &
                    "'" & GetPolicyData(gShipriteDB, "FName", "") & "', " &
                    "'" & GetPolicyData(gShipriteDB, "LName", "") & "', " &
                    "'" & GetPolicyData(gShipriteDB, "Addr1", "") & "', " &
                    "'" & GetPolicyData(gShipriteDB, "Addr2", "") & "', " &
                    "'" & GetPolicyData(gShipriteDB, "City", "") & "', " &
                    "'" & GetPolicyData(gShipriteDB, "State", "") & "', " &
                    "'" & GetPolicyData(gShipriteDB, "Zip", "") & "', " &
                    "'" & GetPolicyData(gShipriteDB, "Phone1", "") & "')"

                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
            End If

            If CDate(SpecialUpdatesVersion) < #07/10/2024# Then
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "07/10/2024")

                If Not IO_GetFieldsCollection(gShipriteDB, "Transactions", "ShippingProduct", False, False, False) = "" Then
                    SQL = "ALTER TABLE Transactions DROP [ShippingProduct]"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                End If
            End If

            If CDate(SpecialUpdatesVersion) < #7/29/2024# Then

                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "7/29/2024")

                If Not IO_GetFieldsCollection(gShipriteDB, "Manifest", "ControlNumber", False, False, False) = "" Then
                    SQL = "ALTER Table Manifest DROP Column ControlNumber"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                End If

                If IO_GetFieldsCollection(gShipriteDB, "Manifest", "ShipAndInsure", False, False, False) = "" Then
                    SQL = "ALTER Table Manifest ADD Column ShipAndInsure DOUBLE"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                End If

            End If

            If CDate(SpecialUpdatesVersion) < #08/13/2024# Then
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "08/13/2024")

                '[SRN-1010] Populate blank class fields in Contacts table
                SQL = "Update Contacts INNER JOIN Manifest ON Contacts.ID = Manifest.CID SET Contacts.Class='Consignee' Where isnull(Contacts.Class)"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

                SQL = "Update Contacts SET Contacts.Class='Shipper' Where isnull(Contacts.Class)"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

            End If

            If CDate(SpecialUpdatesVersion) < #8/14/2024# Then

                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "8/14/2024")

                If IO_GetFieldsCollection(gShipriteDB, "SETUP", "NextInvoiceNumber", False, False, False) = "" Then

                    SQL = "ALTER Table SETUP ADD Column NextInvoiceNumber INTEGER"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)


                    SQL = "SELECT MAX(NumericInvoiceNumber) as MAXID FROM Payments"
                    SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
                    Dim InvNum As Integer = Val(ExtractElementFromSegment("MAXID", SegmentSet, "100"))

                    SQL = "UPDATE Setup SET NextInvoiceNumber = " & InvNum + 1
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                End If
            End If

            If CDate(SpecialUpdatesVersion) < #08/16/2024# Then
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "08/16/2024")
                ' field added previously in 09/07/2022 special update but as Long Text - this should only be used if absolutely necessary
                ' update to Short Text
                SQL = "ALTER Table Mailbox ALTER COLUMN CustomRates Text(255)"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
            End If

            If CDate(SpecialUpdatesVersion) < #10/1/2024# Then
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "10/1/2024")

                SQL = "ALTER TABLE Manifest ADD RoundOptionSell number"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
            End If

            If CDate(SpecialUpdatesVersion) < #10/8/2024# Then
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "10/8/2024")

                SQL = "ALTER TABLE Manifest DROP COLUMN ShipandInsurePosted"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
            End If

            If CDate(SpecialUpdatesVersion) < #10/14/2024# Then
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "10/14/2024")

                ' UpdatePolicy(gShipriteDB, "FedExREST_Enabled", "True")


                ' MsgBox("FedEx Account Registration Necessary!" & vbCrLf & vbCrLf &
                '"ShipRiteNext has been updated to use FedEx' latest REST API integration. This change requires you to re-register your FedEx account in order to be able to process shipments!" &
                'vbCrLf & vbCrLf & "Please go SETUP > CARRIER SETUP > FEDEX" & vbCrLf & vbCrLf & "Make sure your FedEx account number is correctly entered and press the REGISTER button." & vbCrLf &
                '"Follow the prompts on the screen to register your account." & vbCrLf & vbCrLf & vbCrLf & "You will not be able to process FedEx shipments until this task is completed!!!", vbExclamation)

                SQL = "CREATE Table FirstClassRetail (Weight Float CONSTRAINT MyFieldConstraint PRIMARY KEY, [RETAIL-Postcard] Float, [RETAIL-Postcard-Intl] Float,   [RETAIL-Letter] Float, [RETAIL-Flat] Float)"
                ret = IO_UpdateSQLProcessor(gPackagingDB, SQL)

                copyFirstClassRetailPricing()
            End If

            If CDate(SpecialUpdatesVersion) < #10/25/2024# Then
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "10/25/2024")
                SQL = "Alter Table OpenClose DROP COLUMN DrawerOpen"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL, , True)
            End If


            If CDate(SpecialUpdatesVersion) < #10/30/2024# Then
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "10/30/2024")

                UpdatePolicy(gShipriteDB, "FedExREST_Enabled", "False")
            End If

            If CDate(SpecialUpdatesVersion) < #02/04/2025# Then
                ret = UpdatePolicy(gShipriteDB, "SpecialUpdatesVersion", "02/04/2025")

                SQL = "ALTER TABLE Contents ADD COLUMN FragilityLevel INT DEFAULT 1"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
            End If

        Catch ex As Exception

            _MsgBox.ErrorMessage(ex, "Error running Special Update " & SpecialUpdatesVersion)

        End Try
        Return 0

    End Function

    Private Sub copyFirstClassRetailPricing()
        Try
            Dim RetailSet As String
            Dim segment As String
            Dim SQL As String = ""

            RetailSet = IO_GetSegmentSet(gUSMailDB_Services, "Select WEIGHT, [RETAIL-Postcard], [RETAIL-Postcard-Intl], [RETAIL-Letter], [RETAIL-Flat] FROM FirstClass")

            Do Until RetailSet = ""
                segment = GetNextSegmentFromSet(RetailSet)

                SQL = "INSERT INTO FirstClassRetail (WEIGHT, [RETAIL-Postcard], [RETAIL-Postcard-Intl], [RETAIL-Letter], [RETAIL-Flat]) " &
                "VALUES (" & ExtractElementFromSegment("WEIGHT", segment, "") & ", " &
                ExtractElementFromSegment("RETAIL-Postcard", segment, "") & ", " &
                ExtractElementFromSegment("RETAIL-Postcard-Intl", segment, "") & ", " &
                ExtractElementFromSegment("RETAIL-Letter", segment, "") & ", " &
                ExtractElementFromSegment("RETAIL-Flat", segment, "") & ")"

                IO_UpdateSQLProcessor(gPackagingDB, SQL)
            Loop

        Catch ex As Exception

            _MsgBox.ErrorMessage(ex, "Error copying First Class Retail pricing!")

        End Try
    End Sub

    Private Sub FixCouponList()
        Dim SQL As String = "Select SKU, Coupon_AppliesTo FROM Inventory WHERE Department='COUPONS'"
        Dim buffer As String = IO_GetSegmentSet(gShipriteDB, SQL)

        Dim segment As String
        Dim cpn As String
        Dim SKU As String

        Do Until buffer = ""
            segment = GetNextSegmentFromSet(buffer)
            cpn = ExtractElementFromSegment("Coupon_AppliesTo", segment, "")
            SKU = ExtractElementFromSegment("SKU", segment, "")

            cpn = cpn.Replace(" ", ",")

            SQL = "Update Inventory Set Coupon_AppliesTo='" & cpn & "' WHERE SKU='" & SKU & "'"
            IO_UpdateSQLProcessor(gShipriteDB, SQL)
        Loop

    End Sub

    Private Sub Assign_Service_Names()
        Dim SQL As String = "Select Service FROM Master"
        Dim buffer As String = IO_GetSegmentSet(gShipriteDB, SQL)
        Dim segment As String
        Dim svc As String
        Dim SvcName As String = ""


        Do Until buffer = ""
            segment = GetNextSegmentFromSet(buffer)
            svc = ExtractElementFromSegment("Service", segment, "")

            Select Case svc
                Case "COM-GND"
                    SvcName = "UPS® Ground"
                Case "1DAYEAM"
                    SvcName = "UPS Next Day Air® Early"
                Case "1DAY"
                    SvcName = "UPS Next Day Air®"
                Case "1DAYSVR"
                    SvcName = "UPS Next Day Air Saver®"
                Case "2DAYAM"
                    SvcName = "UPS 2nd Day Air® A.M."
                Case "2DAY"
                    SvcName = "UPS 2nd Day Air®"
                Case "3DAYSEL"
                    SvcName = "UPS 3 Day Select®"
                Case "CAN-XPRES"
                    SvcName = "UPS Worldwide Express®"
                Case "CAN-XSVR"
                    SvcName = "UPS Worldwide Saver®"
                Case "CAN-XPED"
                    SvcName = "UPS Worldwide Expedited®"
                Case "CAN-STD"
                    SvcName = "UPS® Standard"
                Case "WWXPRES"
                    SvcName = "UPS Worldwide Express®"
                Case "WWXSVR"
                    SvcName = "UPS Worldwide Saver®"
                Case "WWXPED"
                    SvcName = "UPS Worldwide Expedited®"

                    '------------------------------------------------------
                Case "FEDEX-GND"
                    SvcName = "FedEx Ground®"
                Case "FEDEX-PRI"
                    SvcName = "FedEx Priority Overnight®"
                Case "FEDEX-STD"
                    SvcName = "FedEx Standard Overnight®"
                Case "FEDEX-2DY-AM"
                    SvcName = "FedEx 2Day® A.M."
                Case "FEDEX-2DY"
                    SvcName = "FedEx 2Day®"
                Case "FEDEX-SVR"
                    SvcName = "FedEx Express Saver®"
                Case "FEDEX-1ST"
                    SvcName = "FedEx First Overnight®"

                Case "FEDEX-INT-1ST"
                    SvcName = "FedEx International First®"
                Case "FEDEX-INTP"
                    SvcName = "FedEx International Priority®"
                Case "FEDEX-INTE"
                    SvcName = "FedEx International Economy®"
                Case "FEDEX-CAN"
                    SvcName = "FedEx International Ground®"

                Case "FEDEX-FR1"
                    SvcName = "FedEx 1Day® Freight"
                Case "FEDEX-FR2"
                    SvcName = "FedEx 2Day® Freight"
                Case "FEDEX-FR3"
                    SvcName = "FedEx 3Day® Freight"
                Case "FEDEX-FRP"
                    SvcName = "FedEx Freight® Priority"
                Case "FEDEX-FRE"
                    SvcName = "FedEx Freight® Economy"
                    '------------------------------------------------------

                Case "DHL-INT"
                    SvcName = "DHL Express Worldwide"
                Case "DHL-INT-DOC"
                    SvcName = "DHL Express Worldwide Documents"


                Case "USPS-EXPR"
                    SvcName = "Priority Mail Express®"
                Case "USPS-PRI"
                    SvcName = "Priority Mail®"
                Case "USPS-PS"
                    SvcName = "Parcel Select®"
                Case "USPS-RG"
                    SvcName = "Retail Ground®"
                Case "USPS-MEDIA"
                    SvcName = "Media Mail®"
                Case "USPS-PRT-MTR"
                    SvcName = "Bound Printed Matter®"
                Case "FirstClass"
                    SvcName = "First-Class Mail®"
                Case "USPS-PRI_CubicRate"
                    SvcName = "Priority Mail® Cubic"
                Case "USPS-GND-ADV"
                    SvcName = "Ground Advantage®"

                    'USPS International
                Case "USPS-INTL-GXG"
                    SvcName = "Global Express Guaranteed®"
                Case "USPS-INTL-EMI"
                    SvcName = "Priority Mail Express International®"
                Case "USPS-INTL-PMI"
                    SvcName = "Priority Mail International®"
                Case "USPS-INTL-FCMI"
                    SvcName = "First-Class Mail International®"


                Case "SPEEDEE-GND"
                    SvcName = "Spee-Dee Ground"
            End Select

            IO_UpdateSQLProcessor(gShipriteDB, "Update Master set Description='" & SvcName & "' WHERE SERVICE='" & svc & "'")

        Loop

    End Sub

    Private Sub Assign_Default_ShipPanel_ButtonPositions()

        IO_UpdateSQLProcessor(gShipriteDB, "Update Master set Panel_Row=0, Panel_Row_Canada=1, Panel_Row_Intl=1, Panel_Row_Freight=0 where Type='FEDEX'")
        IO_UpdateSQLProcessor(gShipriteDB, "Update Master set Panel_Row=1, Panel_Row_Canada=2, Panel_Row_Intl=2, Panel_Row_Freight=1  where Type='UPS'")
        IO_UpdateSQLProcessor(gShipriteDB, "Update Master set Panel_Row=2, Panel_Row_Canada=0, Panel_Row_Intl=0, Panel_Row_Freight=2  where Type='DHL'")
        IO_UpdateSQLProcessor(gShipriteDB, "Update Master set Panel_Row=3, Panel_Row_Canada=3, Panel_Row_Intl=3, Panel_Row_Freight=3  where Type='USPS'")

        IO_UpdateSQLProcessor(gShipriteDB, "Update Master set Domestic_Status=0, Canada_Status=0, Intl_Status=0, Freight_Status=0")

        'DHL
        AssignPosition("DHL-INT", 0, 0)
        AssignPosition("DHL-INT-DOC", 1, 1)

        'FedEx Domestic
        AssignPosition("FEDEX-GND", 0)
        AssignPosition("FEDEX-PRI", 1)
        AssignPosition("FEDEX-STD", 2)
        AssignPosition("FEDEX-2DY-AM", 3)
        AssignPosition("FEDEX-2DY", 4)
        AssignPosition("FEDEX-SVR", 5)
        AssignPosition("FEDEX-1ST", 6)

        'FedEx International
        AssignPosition("FEDEX-INT-1ST", 0, 0)
        AssignPosition("FEDEX-INTP", 1, 1)
        AssignPosition("FEDEX-INTE", 2, 2)
        AssignPosition("FEDEX-CAN", 3, 3)


        'FedEx Freight
        AssignPosition("FEDEX-FR1", 0)
        AssignPosition("FEDEX-FR2", 1)
        AssignPosition("FEDEX-FR3", 2)
        AssignPosition("FEDEX-FRP", 3)
        AssignPosition("FEDEX-FRE", 4)

        'UPS Domestic
        AssignPosition("COM-GND", 0)
        AssignPosition("1DAY", 1)
        AssignPosition("1DAYSVR", 2)
        AssignPosition("2DAYAM", 3)
        AssignPosition("2DAY", 4)
        AssignPosition("3DAYSEL", 5)
        AssignPosition("1DAYEAM", 6)

        'UPS Canada
        AssignPosition("CAN-XPRES", 0, 0)
        AssignPosition("CAN-XSVR", 1, 1)
        AssignPosition("CAN-XPED", 2, 2)
        AssignPosition("CAN-STD", 3, 3)

        'UPS International
        AssignPosition("WWXPRES", 0, 0)
        AssignPosition("WWXSVR", 1, 1)
        AssignPosition("WWXPED", 2, 2)


        'USPS Domestic
        AssignPosition("USPS-EXPR", 1)
        AssignPosition("USPS-PRI", 2)
        AssignPosition("USPS-PS", 7)
        AssignPosition("USPS-RG", 8)
        AssignPosition("USPS-GND-ADV", 0)
        AssignPosition("USPS-MEDIA", 3)
        AssignPosition("USPS-PRT-MTR", 4)
        AssignPosition("FirstClass", 5)
        AssignPosition("USPS-PRI_CubicRate", 6)

        'USPS International
        AssignPosition("USPS-INTL-GXG", 0, 0)
        AssignPosition("USPS-INTL-EMI", 1, 1)
        AssignPosition("USPS-INTL-PMI", 2, 2)
        AssignPosition("USPS-INTL-FCMI", 3, 3)


        If SpeeDee.isSpeeDee_Exists Then
            'SpeeDee Exists
            IO_UpdateSQLProcessor(gShipriteDB, "Update Master set Panel_Row=4, Panel_Row_Canada=4 where Type='SPEE-DEE'")
            AssignPosition("SPEEDEE-GND", 0, 0)
        End If

    End Sub

    Private Sub AssignPosition(SVC As String, Column As Integer, Optional ColumnCanada As Integer = Nothing)
        'check if service exists in Master
        If IO_GetSegmentSet(gShipriteDB, "Select Service from Master WHERE Service='" & SVC & "'") <> "" Then

            If Not IsNothing(ColumnCanada) Then
                IO_UpdateSQLProcessor(gShipriteDB, "Update Master set Panel_Column=" & Column & ", Panel_Column_Canada=" & ColumnCanada & " where SERVICE='" & SVC & "'")
            Else
                IO_UpdateSQLProcessor(gShipriteDB, "Update Master set Panel_Column=" & Column & " where SERVICE='" & SVC & "'")
            End If
        End If

    End Sub

    Private Sub UpdateTicklerTable()
        Try
            Dim SQL As String = "Select ID, Details FROM Tickler WHERE Category='Inventory Low'"
            Dim buffer As String = IO_GetSegmentSet(gShipriteDB, SQL)
            Dim segment As String

            Dim ID As Integer
            Dim Details As String
            Dim SKU As String
            Dim NewDetails As String

            Do Until buffer = ""
                segment = GetNextSegmentFromSet(buffer)

                ID = ExtractElementFromSegment("ID", segment)
                Details = ExtractElementFromSegment("Details", segment)


                Dim i As Integer = Details.IndexOf("(")

                If i <> -1 Then
                    SKU = Details.Substring(i + 1, Details.IndexOf(")", i + 1) - i - 1)
                    NewDetails = Details.Substring(0, i - 1)

                    SQL = "UPDATE Tickler SET Details='" & NewDetails.Replace("'", "''") & "', SKU='" & SKU & "' WHERE ID=" & ID
                    IO_UpdateSQLProcessor(gShipriteDB, SQL)
                End If
            Loop

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Updating Tickler")
        End Try
    End Sub

    Private Sub CopyCustomsFromManifest()

        Dim manifest_segment As String
        Dim ManifestRecordSet As RecordSetDefinition
        Dim Ret As Long
        Dim i As Long

        Ret = IO_GetSegmentSetInToStructure(gShipriteDB, "SELECT PACKAGEID, CustomsDESC, CustomsDESC2, CustomsDESC3, CustomsDESC4, CustomsDESC5, 
                                                CustomsVALUE, CustomsVALUE2, CustomsVALUE3, CustomsVALUE4, CustomsVALUE5,
                                                CustomsQty, CustomsQty2, CustomsQty3, CustomsQty4, CustomsQty5,
                                                CustomsWeight, CustomsWeight2, CustomsWeight3, CustomsWeight4, CustomsWeight5,
                                                CustomsCountryOfOrigin, CustomsCountryOfOrigin2, CustomsCountryOfOrigin3, CustomsCountryOfOrigin4, CustomsCountryOfOrigin5
                                                from Manifest WHERE CustomsDESC <> ''", ManifestRecordSet)

        For i = 0 To ManifestRecordSet.RecordCount - 1

            manifest_segment = MakeSegmentFromRecord(ManifestRecordSet, i)
            'get first CustomsValue from Record
            GetCustomsLineFromManifest("", manifest_segment)

            'if existant, get secondary Customs Values from Record
            If Replace(ExtractElementFromSegment("CustomsDESC2", manifest_segment), "'", "''") <> "" Then
                GetCustomsLineFromManifest("2", manifest_segment)

                If Replace(ExtractElementFromSegment("CustomsDESC3", manifest_segment), "'", "''") <> "" Then
                    GetCustomsLineFromManifest("3", manifest_segment)

                    If Replace(ExtractElementFromSegment("CustomsDESC4", manifest_segment), "'", "''") <> "" Then
                        GetCustomsLineFromManifest("4", manifest_segment)

                        If Replace(ExtractElementFromSegment("CustomsDESC5", manifest_segment), "'", "''") <> "" Then
                            GetCustomsLineFromManifest("5", manifest_segment)
                        End If
                    End If
                End If
            End If

        Next i

    End Sub

    Private Sub GetCustomsLineFromManifest(ByVal itemNo As String, ByVal manifest_segment As String)

        Dim ret As Integer
        Dim sql As String

        sql = "INSERT INTO CustomsItems (PackageID, Description, Quantity, Weight, ItemValue, OriginCountry) VALUES ('" &
                ExtractElementFromSegment("PACKAGEID", manifest_segment) & "', '" &
                Replace(ExtractElementFromSegment("CustomsDESC" & itemNo, manifest_segment), "'", "''") & "', "


        If ExtractElementFromSegment("CustomsQty" & itemNo, manifest_segment) = "" Then
            sql &= "0, "
        Else
            sql = sql & ExtractElementFromSegment("CustomsQty" & itemNo, manifest_segment) & ", "
        End If


        If ExtractElementFromSegment("CustomsWeight" & itemNo, manifest_segment) = "" Then
            sql &= "0, "
        Else
            sql = sql & ExtractElementFromSegment("CustomsWeight" & itemNo, manifest_segment) & ", "
        End If


        If ExtractElementFromSegment("CustomsVALUE" & itemNo, manifest_segment) = "" Then
            sql &= "0, "
        Else
            sql &= ExtractElementFromSegment("CustomsVALUE" & itemNo, manifest_segment) & ", "
        End If

        sql &= "'" & ExtractElementFromSegment("CustomsCountryOfOrigin" & itemNo, manifest_segment) & "')"

        ret = IO_UpdateSQLProcessor(gShipriteDB, sql)

    End Sub

    Public Sub Rename_Field(Table As String, OldFieldName As String, NewFieldName As String, NewFieldType As String)
        Try
            Dim SQL As String
            Dim ret As String
            ret = IO_GetFieldsCollection(gShipriteDB, Table, NewFieldName, False, False, False)

            If ret <> "" Then
                'field already exists
                Exit Sub
            End If

            'Create mew Field
            SQL = "ALTER TABLE " & Table & " ADD " & NewFieldName & " " & NewFieldType
            IO_UpdateSQLProcessor(gShipriteDB, SQL)

            'Copy Data Over
            SQL = "UPDATE " & Table & " SET " & NewFieldName & " = " & OldFieldName
            IO_UpdateSQLProcessor(gShipriteDB, SQL)

            'Delete old field
            SQL = "ALTER TABLE " & Table & " DROP " & OldFieldName
            IO_UpdateSQLProcessor(gShipriteDB, SQL)

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error renaming field")
        End Try
    End Sub

    Public Sub ConvertShipriteMDBtoACCDB(ByRef cWindow As Window)
        Try
            Dim ConvertFileName As String
            Dim ans As Integer
            Dim OldShipriteDB As String = ""
            Dim ConvertDB As String = gDBpath & "\Convert.accdb"
            Dim oldDataPath As String

            ConvertFileName = Dir(ConvertDB)
            If Not ConvertFileName = "" Then

                If File.Exists("c:\Windows\Shiprite.ini") Then

                    oldDataPath = GetWinINI("ShipRite9", "c:\Windows\Shiprite.ini", "c:\Shiprite", "DataPath")
                    OldShipriteDB = oldDataPath & "\shiprite.mdb"
                    If File.Exists(OldShipriteDB) Then

                        ans = MsgBox("ATTENTION...Previous Version Of Shiprite Found." & vbCrLf & vbCrLf & "CONVERT NOW", MessageBoxButton.YesNoCancel)
                        If ans = vbYes Then

                            gConversionProcessHasRun = True
                            Copy_UPS_CSVZoneFile_From_Old_ShipRite()
                            Copy_MDB_From_Old_ShipRite("Pricing")
                            Copy_MDB_From_Old_ShipRite("ShipritePackaging")
                            Copy_MDB_From_Old_ShipRite("Finance")
                            Copy_MDB_From_Old_ShipRite("Shiprite_DropOffPackages")
                            Copy_MDB_From_Old_ShipRite("Shiprite_MailboxPackages")
                            Copy_DropOffDisclaimertxt_From_Old_ShipRite()

                            ConvertUtility_Run(OldShipriteDB, ConvertDB, "ShipriteNext", cWindow)

                        ElseIf ans = vbNo Then

                            Try
                                File.Delete(ConvertDB)
                            Catch ex As Exception
                            End Try

                        End If

                    End If

                End If

            End If

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Converting Database")
        End Try

    End Sub

    Private Function Cache_UPS_DomesticZones(ByVal oZipCode As String) As Domestic_Zones

        Dim oZipCode3 As String = oZipCode.Substring(0, 3).PadLeft(3, "0")
        Dim fileName As String = oZipCode3 & ".csv"
        Dim fileDir As String = gZoneTablesPath & "\UPS\"
        Dim filePath As String = fileDir & fileName
        Dim lineBuf As String = ""
        Dim dZones As Domestic_Zones = Nothing

        Try

            If Not File.Exists(filePath) Then
                If File.Exists("c:\Windows\Shiprite.ini") Then
                    Dim oldFilePath As String = GetWinINI("ShipRite9", "c:\Windows\Shiprite.ini", "c:\Shiprite", "DataPath") & "\" & fileName
                    If File.Exists(oldFilePath) Then
                        Directory.CreateDirectory(fileDir)
                        If Directory.Exists(fileDir) Then
                            File.Copy(oldFilePath, filePath)
                        End If
                    End If
                End If
            End If

            If File.Exists(filePath) Then
                Using fileStream As StreamReader = File.OpenText(filePath)
                    Dim stopit As Boolean = False
                    Dim badZoneFile As Boolean = False
                    Do While fileStream.Peek >= 0 ' Do Until stopit = True
                        lineBuf = fileStream.ReadLine
                        If lineBuf.Contains("Ground,3 Day Select,") Then
                            If fileStream.EndOfStream() Then
                                Dim msgBuf As String = "UPS Zone file (" & filePath & ") format error..." & vbCrLf & vbCrLf
                                msgBuf &= "Please contact the Shiprite Support team."
                                MessageBox.Show(msgBuf, gProgramName, MessageBoxButton.OK, MessageBoxImage.Exclamation)
                                Return Nothing
                            End If
                            Try
                                lineBuf = fileStream.ReadLine
                            Catch ex As Exception
                                stopit = True
                                badZoneFile = True
                            End Try
                            Exit Do
                        End If
                    Loop
                    If badZoneFile = True Then
                        Dim msgBuf As String = "ATTENTION...The UPS Zone file (" & filePath & ") is corrupt." & vbCrLf & vbCrLf
                        msgBuf &= "Please contact the Shiprite Support team."
                        MessageBox.Show(msgBuf, gProgramName, MessageBoxButton.OK, MessageBoxImage.Exclamation)
                        stopit = True
                        Return Nothing
                    End If
                    Dim ZLast As Integer = 1
                    Do Until stopit = True Or Not fileStream.Peek >= 0 ' do while stopit = false and filestream.peek >= 0
                        lineBuf = fileStream.ReadLine
                        ' Input #1, Range, GND, DAY3, DAY2, DAYAM2, DAYSVR1, DAY1
                        Dim lineArr As String() = lineBuf.Split(",")
                        Dim range As String = ""
                        Dim gnd As String = ""
                        Dim day3 As String = ""
                        Dim day2 As String = ""
                        Dim dayam2 As String = ""
                        Dim daysvr1 As String = ""
                        Dim day1 As String = ""
                        Try
                            range = lineArr(0)
                            gnd = lineArr(1)
                            day3 = lineArr(2)
                            day2 = lineArr(3)
                            dayam2 = lineArr(4)
                            daysvr1 = lineArr(5)
                            day1 = lineArr(6)
                        Catch ex As Exception
                        End Try
                        If range.Length > 0 Then
                            If dZones Is Nothing Then
                                dZones = New Domestic_Zones
                                dZones.Add(New Domestic_Zone("GROUND", 0.ToString)) ' 0
                                dZones.Add(New Domestic_Zone("3DAYSEL", 300.ToString)) ' 1
                                dZones.Add(New Domestic_Zone("2DAY", 200.ToString)) ' 2
                                dZones.Add(New Domestic_Zone("2DAYAM", 200.ToString)) ' 3
                                dZones.Add(New Domestic_Zone("1DAYSVR", 100.ToString)) ' 4
                                dZones.Add(New Domestic_Zone("1DAY", 100.ToString)) ' 5
                                dZones.Add(New Domestic_Zone("1DAYEAM", 100.ToString)) ' 6
                            End If
                            If stopit = False Then
                                Dim loZip As String = ""
                                Dim hiZip As String = ""
                                Dim iloc As Integer = range.IndexOf("-")
                                If iloc >= 0 Then
                                    loZip = range.Substring(0, iloc).PadLeft(3, "0").PadRight(5, "0")
                                    hiZip = range.Substring(iloc + 1).PadLeft(3, "0").PadRight(5, "9")
                                Else
                                    loZip = range.PadLeft(3, "0").PadRight(5, "0")
                                    hiZip = range.PadLeft(3, "0").PadRight(5, "9")
                                End If

                                dZones(0).Zones.Add(New Domestic_Zone_ZipRange(loZip, hiZip, Val(gnd).ToString))
                                dZones(1).Zones.Add(New Domestic_Zone_ZipRange(loZip, hiZip, Val(day3).ToString))
                                dZones(2).Zones.Add(New Domestic_Zone_ZipRange(loZip, hiZip, Val(day2).ToString))
                                dZones(3).Zones.Add(New Domestic_Zone_ZipRange(loZip, hiZip, Val(dayam2).ToString))
                                dZones(4).Zones.Add(New Domestic_Zone_ZipRange(loZip, hiZip, Val(daysvr1).ToString))
                                dZones(5).Zones.Add(New Domestic_Zone_ZipRange(loZip, hiZip, Val(day1).ToString))
                                dZones(6).Zones.Add(New Domestic_Zone_ZipRange(loZip, hiZip, Val(day1).ToString))

                                ZLast = Val(hiZip.TrimEnd("9")) + 1

                                If ZLast > 999 Then
                                    range = ""
                                End If
                            End If
                        End If
                        If range.Length = 0 Then
                            lineBuf = ""
                            ' find start of Hawaii zip codes
                            Do Until lineBuf.IndexOf("[2] For Hawaii") >= 0
                                If fileStream.EndOfStream Then
                                    Exit Do
                                End If
                                lineBuf = fileStream.ReadLine
                            Loop

                            Dim hiZones As String() = {"", "", ""} ' 0 = Ground, 1 = Next Day Air, 2 = 2nd Day Air

                            Dim splitHiZone As String() = lineBuf.Split("Zone")
                            For i As Integer = 1 To splitHiZone.Length - 1
                                For j As Integer = 0 To splitHiZone(i).Length - 1
                                    Dim buf As String = splitHiZone(i)(j)
                                    If hiZones.GetUpperBound(0) >= i - 1 Then
                                        If IsNumeric(buf) Then
                                            hiZones(i - 1) &= buf
                                        ElseIf Not hiZones(i - 1).Length = 0 Then
                                            Exit For
                                        End If
                                    End If
                                Next
                            Next

                            Dim splitLine As String()
                            ' find first hawaii line that is numeric
                            Do While Not fileStream.EndOfStream
                                lineBuf = fileStream.ReadLine
                                splitLine = lineBuf.Split(",")
                                If splitLine.GetUpperBound(0) >= 0 Then
                                    If IsNumeric(splitLine(0)) Then
                                        Exit Do
                                    End If
                                End If
                            Loop
                            ' read first hawaii
                            Dim hiBuf As String = ""
                            Do While Not fileStream.EndOfStream
                                splitLine = lineBuf.Split(",")
                                If splitLine.GetUpperBound(0) >= 0 Then
                                    If IsNumeric(splitLine(0)) Then
                                        If Not hiBuf.Length = 0 Then hiBuf &= ","
                                        hiBuf &= lineBuf.Trim
                                    Else
                                        Exit Do
                                    End If
                                Else
                                    Exit Do
                                End If
                                lineBuf = fileStream.ReadLine
                            Loop

                            splitHiZone = hiBuf.Split({","}, StringSplitOptions.RemoveEmptyEntries)
                            For i As Integer = 0 To splitHiZone.Length - 1
                                dZones(0).Zones.Add(New Domestic_Zone_ZipRange(splitHiZone(i), splitHiZone(i), hiZones(0)))
                                dZones(5).Zones.Add(New Domestic_Zone_ZipRange(splitHiZone(i), splitHiZone(i), hiZones(1)))
                                dZones(6).Zones.Add(New Domestic_Zone_ZipRange(splitHiZone(i), splitHiZone(i), hiZones(1)))
                                dZones(2).Zones.Add(New Domestic_Zone_ZipRange(splitHiZone(i), splitHiZone(i), hiZones(2)))
                            Next

                            hiZones = {"", "", ""}
                            splitHiZone = lineBuf.Split("Zone")
                            For i As Integer = 1 To splitHiZone.Length - 1
                                For j As Integer = 0 To splitHiZone(i).Length - 1
                                    Dim buf As String = splitHiZone(i)(j)
                                    If hiZones.GetUpperBound(0) >= i - 1 Then
                                        If IsNumeric(buf) Then
                                            hiZones(i - 1) &= buf
                                        ElseIf Not hiZones(i - 1).Length = 0 Then
                                            Exit For
                                        End If
                                    End If
                                Next
                            Next

                            ' read second hawaii
                            hiBuf = ""
                            Do While Not fileStream.EndOfStream
                                lineBuf = fileStream.ReadLine
                                splitLine = lineBuf.Split(",")
                                If splitLine.GetUpperBound(0) >= 0 Then
                                    If IsNumeric(splitLine(0)) Then
                                        If Not hiBuf.Length = 0 Then hiBuf &= ","
                                        hiBuf &= lineBuf.Trim
                                    Else
                                        Exit Do
                                    End If
                                Else
                                    Exit Do
                                End If
                            Loop

                            splitHiZone = hiBuf.Split({","}, StringSplitOptions.RemoveEmptyEntries)
                            For i As Integer = 0 To splitHiZone.Length - 1
                                dZones(0).Zones.Add(New Domestic_Zone_ZipRange(splitHiZone(i), splitHiZone(i), hiZones(0)))
                                dZones(5).Zones.Add(New Domestic_Zone_ZipRange(splitHiZone(i), splitHiZone(i), hiZones(1)))
                                dZones(6).Zones.Add(New Domestic_Zone_ZipRange(splitHiZone(i), splitHiZone(i), hiZones(1)))
                                dZones(2).Zones.Add(New Domestic_Zone_ZipRange(splitHiZone(i), splitHiZone(i), hiZones(2)))
                            Next

                            ' find alaska
                            Do Until lineBuf.IndexOf("[3] For Alaska") >= 0
                                If fileStream.EndOfStream Then
                                    Exit Do
                                End If
                                lineBuf = fileStream.ReadLine()
                            Loop

                            Dim akZones As String() = {"", "", ""} ' 0 = Ground, 1 = Next Day Air, 2 = 2nd Day Air

                            Dim splitAkZone As String() = lineBuf.Split("Zone")
                            For i As Integer = 1 To splitAkZone.Length - 1
                                For j As Integer = 0 To splitAkZone(i).Length - 1
                                    Dim buf As String = splitAkZone(i)(j)
                                    If akZones.GetUpperBound(0) >= i - 1 Then
                                        If IsNumeric(buf) Then
                                            akZones(i - 1) &= buf
                                        ElseIf Not akZones(i - 1).Length = 0 Then
                                            Exit For
                                        End If
                                    End If
                                Next
                            Next

                            ' find first alaska line that is numeric
                            Do While Not fileStream.EndOfStream
                                lineBuf = fileStream.ReadLine
                                splitLine = lineBuf.Split(",")
                                If splitLine.GetUpperBound(0) >= 0 Then
                                    If IsNumeric(splitLine(0)) Then
                                        Exit Do
                                    End If
                                End If
                            Loop
                            ' read first alaska
                            Dim akBuf As String = ""
                            Do While Not fileStream.EndOfStream
                                splitLine = lineBuf.Split(",")
                                If splitLine.GetUpperBound(0) >= 0 Then
                                    If IsNumeric(splitLine(0)) Then
                                        If Not akBuf.Length = 0 Then akBuf &= ","
                                        akBuf &= lineBuf.Trim
                                    Else
                                        Exit Do
                                    End If
                                Else
                                    Exit Do
                                End If
                                lineBuf = fileStream.ReadLine
                            Loop

                            splitAkZone = akBuf.Split({","}, StringSplitOptions.RemoveEmptyEntries)
                            For i As Integer = 0 To splitAkZone.Length - 1
                                dZones(0).Zones.Add(New Domestic_Zone_ZipRange(splitAkZone(i), splitAkZone(i), akZones(0)))
                                dZones(5).Zones.Add(New Domestic_Zone_ZipRange(splitAkZone(i), splitAkZone(i), akZones(1)))
                                dZones(6).Zones.Add(New Domestic_Zone_ZipRange(splitAkZone(i), splitAkZone(i), akZones(1)))
                                dZones(2).Zones.Add(New Domestic_Zone_ZipRange(splitAkZone(i), splitAkZone(i), akZones(2)))
                            Next

                            akZones = {"", "", ""}
                            splitAkZone = lineBuf.Split("Zone")
                            For i As Integer = 1 To splitAkZone.Length - 1
                                For j As Integer = 0 To splitAkZone(i).Length - 1
                                    Dim buf As String = splitAkZone(i)(j)
                                    If akZones.GetUpperBound(0) >= i - 1 Then
                                        If IsNumeric(buf) Then
                                            akZones(i - 1) &= buf
                                        ElseIf Not akZones(i - 1).Length = 0 Then
                                            Exit For
                                        End If
                                    End If
                                Next
                            Next

                            ' read second alaska
                            akBuf = ""
                            Do While Not fileStream.EndOfStream
                                lineBuf = fileStream.ReadLine
                                splitLine = lineBuf.Split(",")
                                If splitLine.GetUpperBound(0) >= 0 Then
                                    If IsNumeric(splitLine(0)) Then
                                        If Not akBuf.Length = 0 Then akBuf &= ","
                                        akBuf &= lineBuf.Trim
                                    Else
                                        Exit Do
                                    End If
                                Else
                                    Exit Do
                                End If
                            Loop

                            splitAkZone = akBuf.Split({","}, StringSplitOptions.RemoveEmptyEntries)
                            For i As Integer = 0 To splitAkZone.Length - 1
                                dZones(0).Zones.Add(New Domestic_Zone_ZipRange(splitAkZone(i), splitAkZone(i), akZones(0)))
                                dZones(5).Zones.Add(New Domestic_Zone_ZipRange(splitAkZone(i), splitAkZone(i), akZones(1)))
                                dZones(6).Zones.Add(New Domestic_Zone_ZipRange(splitAkZone(i), splitAkZone(i), akZones(1)))
                                dZones(2).Zones.Add(New Domestic_Zone_ZipRange(splitAkZone(i), splitAkZone(i), akZones(2)))
                            Next

                            stopit = True
                        End If

                    Loop

                End Using

                Return dZones
            Else
                Dim msgBuf As String = "Failed to locate UPS Zone lookup file (" & filePath & ")." & vbCrLf & vbCrLf
                msgBuf &= "Please contact the Shiprite Support team to send you the " & fileName & " file to enable your location's UPS Domestic zone lookup."
                MessageBox.Show(msgBuf, gProgramName, MessageBoxButton.OK, MessageBoxImage.Exclamation)
            End If

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error loading UPS Zone file.")
        End Try

        Return Nothing

    End Function

    Private Function Cache_FedEx_DomesticZones(ByVal oZipCode As String) As Domestic_Zones
        ' FederalExpress.FEDEX_CACHE_ZONES()
        Dim oZipCode3 As String = oZipCode.Substring(0, 3).PadLeft(3, "0")
        Dim fileName As String = "FedEXZN.txt"
        Dim fileDir As String = gZoneTablesPath & "\FedEx\"
        Dim filePath As String = fileDir & fileName
        Dim lineBuf As String = ""
        Dim dZones As Domestic_Zones = Nothing

        Try

            If Not File.Exists(filePath) Then
                If File.Exists("c:\Windows\Shiprite.ini") Then
                    Dim oldFilePath As String = GetWinINI("ShipRite9", "c:\Windows\Shiprite.ini", "c:\Shiprite", "DataPath") & "\" & fileName
                    If File.Exists(oldFilePath) Then
                        Directory.CreateDirectory(fileDir)
                        If Directory.Exists(fileDir) Then
                            File.Copy(oldFilePath, filePath)
                        End If
                    End If
                End If
            End If

            If File.Exists(filePath) Then
                Using fileStream As StreamReader = File.OpenText(filePath)
                    Dim oFileZip As String = ""

                    ' find starting line
                    Do Until Val(oFileZip) = Val(oZipCode3) Or fileStream.EndOfStream
                        lineBuf = fileStream.ReadLine
                        oFileZip = lineBuf.Substring(0, 5).Trim
                    Loop

                    Do Until Not Val(oFileZip) = Val(oZipCode3)
                        Dim loZip As String = lineBuf.Substring(5, 5).Trim.PadLeft(3, "0").PadRight(5, "0")
                        Dim hiZip As String = lineBuf.Substring(10, 5).Trim.PadLeft(3, "0").PadRight(5, "9")
                        Dim zoneC As String = Val(lineBuf.Substring(15, lineBuf.Length - 15).Trim).ToString

                        If dZones Is Nothing Then
                            dZones = New Domestic_Zones
                            dZones.Add(New Domestic_Zone("FEDEX-GND")) ' 0
                            dZones.Add(New Domestic_Zone("FEDEX48")) ' 1
                            dZones.Add(New Domestic_Zone("FEDEX-GND-AK_HI")) ' 2
                        End If

                        dZones(0).Zones.Add(New Domestic_Zone_ZipRange(loZip, hiZip, Val(zoneC).ToString))
                        dZones(1).Zones.Add(New Domestic_Zone_ZipRange(loZip, hiZip, Val(zoneC).ToString))
                        dZones(2).Zones.Add(New Domestic_Zone_ZipRange(loZip, hiZip, Val(zoneC).ToString))

                        lineBuf = fileStream.ReadLine
                        oFileZip = lineBuf.Substring(0, 5).Trim
                    Loop
                End Using

                Return dZones
            Else
                Dim msgBuf As String = "Failed to locate FedEx Zone lookup file (" & filePath & ")." & vbCrLf & vbCrLf
                msgBuf &= "Please contact the Shiprite Support team to send you the " & fileName & " file to enable your location's FedEx Domestic zone lookup."
                MessageBox.Show(msgBuf, gProgramName, MessageBoxButton.OK, MessageBoxImage.Exclamation)
            End If

        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try

        Return Nothing

    End Function

    Private Sub Copy_DomesticZones_To_gZoneTable(ByVal dZones As Domestic_Zones) ' List(Of Domestic_Zone))

        Try

            If dZones IsNot Nothing Then
                For i As Integer = 0 To dZones.Count - 1
                    Dim zName As String = dZones(i).ZoneName
                    Dim j As Integer = 0
                    For j = 0 To gZct - 1
                        If gZoneTables(j).ZoneName = zName Then
                            Exit For
                        End If
                    Next
                    ReDim gZoneTables(j).Zones(dZones(i).Zones.Count - 1)
                    If j = gZct Then
                        gZoneTables(j).ZoneName = dZones(i).ZoneName
                        gZct += 1 ' increment 
                    End If
                    gZoneTables(j).ZoneCount = dZones(i).Zones.Count - 1

                    gZoneTables(j).International = False
                    gZoneTables(j).UseDirectDBAccess = False
                    For k As Integer = 0 To dZones(i).Zones.Count - 1
                        gZoneTables(j).Zones(k).LoAlpha = dZones(i).Zones(k).LoZip
                        gZoneTables(j).Zones(k).HiAlpha = dZones(i).Zones(k).LoZip
                        gZoneTables(j).Zones(k).Lo = Val(dZones(i).Zones(k).LoZip)
                        gZoneTables(j).Zones(k).Hi = Val(dZones(i).Zones(k).HiZip)
                        gZoneTables(j).Zones(k).Zone = dZones(i).Zones(k).Zone
                        gZoneTables(j).Zones(k).Country = ""
                        gZoneTables(j).Zones(k).Segment = ""
                    Next
                Next
            End If

        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try

    End Sub

    Private Sub Update_Setup_Table()
        'Setup table is used for crystal reports
        Try
            Dim SQL = "Update SETUP Set Name='" & GetPolicyData(gShipriteDB, "Name", "") & "', " &
                 "FName='" & GetPolicyData(gShipriteDB, "FName", "") & "', " &
                 "LName='" & GetPolicyData(gShipriteDB, "LName", "") & "', " &
                 "Addr1='" & GetPolicyData(gShipriteDB, "Addr1", "") & "', " &
                 "Addr2='" & GetPolicyData(gShipriteDB, "Addr2", "") & "', " &
                 "City='" & GetPolicyData(gShipriteDB, "City", "") & "', " &
                 "State='" & GetPolicyData(gShipriteDB, "State", "") & "', " &
                 "Zip='" & GetPolicyData(gShipriteDB, "Zip", "") & "', " &
                 "Phone1='" & GetPolicyData(gShipriteDB, "Phone1", "") & "'"

            IO_UpdateSQLProcessor(gShipriteDB, SQL)

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Loading Setup Table")
        End Try
    End Sub

    Public Sub ConvertUtility_Run(fromDbPath As String, toDbPath As String, toDbName As String, Optional ByRef cWindow As Window = Nothing)
        Try
            Dim toDbFileName As String = Dir(toDbPath)

            If cWindow IsNot Nothing Then
                cWindow.Visibility = Visibility.Hidden
            End If

            Dim process As New Process()
            Dim startinfo As New ProcessStartInfo
            startinfo.Arguments = fromDbPath & " " & toDbPath & " " & toDbName '" ShipriteNext"
            startinfo.FileName = gAppPath & "\ConvertToAccDB.exe"
            process.StartInfo = startinfo
            process.Start()

            Do Until toDbFileName = "" Or process.HasExited

                toDbFileName = Dir(toDbPath)
                Threading.Thread.Sleep(500)
                Forms.Application.DoEvents()

            Loop

            If cWindow IsNot Nothing Then
                cWindow.Visibility = Visibility.Visible
            End If

            ' Compact or repair after the conversion is completed
            Compact_RepairDB()

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Running Convert Utility")
        End Try
    End Sub

    Private Class Files_UpdatePaths
        Private Const OldDataPath As String = "C:\Network\ShipriteNext\Data"
        Private Const NewDataPath As String = "C:\ShipriteNext\Data"
        Private Const AppPath As String = "C:\ShipriteNext"
        Private Const Registry_SRN_Install_KeyName As String = "Software\ShipriteNext\Install"
        Private Const Registry_SRN_Install_IsFirstTime_KeyValueName As String = "IsFilePathsUpdated"
        Private ReadOnly Property Registry_IsFirstTime As Boolean
            Get
                Try
                    Dim test As Object = Nothing
                    Dim testKey As RegistryKey = My.Computer.Registry.LocalMachine.OpenSubKey(Registry_SRN_Install_KeyName)
                    If testKey IsNot Nothing Then
                        test = testKey.GetValue(Registry_SRN_Install_IsFirstTime_KeyValueName)
                        If test IsNot Nothing Then
                            If Not String.IsNullOrWhiteSpace(test.ToString()) Then
                                Return Not (test.ToString() = "1") ' found, and contains "1" - so not first time
                            End If
                        End If
                    End If
                Catch ex As Exception
                End Try
                Return True
            End Get
        End Property

        Private Function Registry_Update_IsFirstTime() As Boolean
            Try
                Dim newKey As RegistryKey = My.Computer.Registry.LocalMachine.CreateSubKey(Registry_SRN_Install_KeyName)
                newKey.SetValue(Registry_SRN_Install_IsFirstTime_KeyValueName, "1")
                Return True
            Catch ex As Exception
            End Try
            Return False
        End Function

        Public Sub TransferFiles_FromOld2NewLocation()
            Try
                If Registry_IsFirstTime Then
                    ' OldDataPath -> NewDataPath
                    If _Files.IsFolderExist(OldDataPath, False) Then
                        _Files.MoveFile_ToNewFolder(OldDataPath, NewDataPath, "Finance.mdb", False)
                        _Files.MoveFile_ToNewFolder(OldDataPath, NewDataPath, "Pricing.accdb", False)
                        _Files.MoveFile_ToNewFolder(OldDataPath, NewDataPath, "Shiprite_DropOffPackages.mdb", False)
                        _Files.MoveFile_ToNewFolder(OldDataPath, NewDataPath, "Shiprite_MailboxPackages.mdb", False)
                        _Files.MoveFile_ToNewFolder(OldDataPath, NewDataPath, "ShipriteNext.accdb", False)
                        _Files.MoveFile_ToNewFolder(OldDataPath, NewDataPath, "ShipritePackaging.mdb", False)
                        _Files.Move_Folder(OldDataPath & "\Endicia\InOut", NewDataPath & "\Endicia\InOut", False)
                        _Files.Move_Folder(OldDataPath & "\FedEx\InOut", NewDataPath & "\FedEx\InOut", False)
                        _Files.Move_Folder(OldDataPath & "\UPS\InOut", NewDataPath & "\UPS\InOut", False)
                        '
                        _Files.Delete_Folder(OldDataPath & "\Endicia\InOut", False)
                        _Files.Delete_Folder(OldDataPath & "\Endicia", False)
                        _Files.Delete_Folder(OldDataPath & "\FedEx\InOut", False)
                        _Files.Delete_Folder(OldDataPath & "\FedEx", False)
                        _Files.Delete_Folder(OldDataPath & "\UPS\InOut", False)
                        _Files.Delete_Folder(OldDataPath & "\UPS", False)
                        _Files.Delete_Folder(OldDataPath, False)
                        _Files.Delete_Folder(OldDataPath & "\..", False) ' C:\Network\ShipriteNext
                        _Files.Delete_Folder(OldDataPath & "\..\..", False) ' C:\Network
                    End If
                    _Files.Move_Folder(AppPath & "\ZoneTables\UPS", NewDataPath & "\ZoneTables\UPS", False)
                    _Files.Delete_Folder(AppPath & "\ZoneTables\UPS", False)
                    _Files.Delete_Folder(AppPath & "\ZoneTables", False)
                    If _Files.IsFolderExist(AppPath & "\Templates", False) Then
                        _Files.MoveFile_ToNewFolder(AppPath & "\Templates\DropOff_Disclaimer.txt", NewDataPath & "\Templates\DropOff_Disclaimer.txt", False)
                        _Files.Delete_Folder(AppPath & "\Templates", False)
                    End If
                    _Files.MoveFile_ToNewFolder(AppPath & "\zipcodes.mdb", NewDataPath & "\zipcodes.mdb", False)
                    '
                    Registry_Update_IsFirstTime()
                End If
            Catch ex As Exception
                _MsgBox.ErrorMessage(ex, "Error Transferring Files from Old to New Paths")
            End Try
        End Sub
    End Class

End Module