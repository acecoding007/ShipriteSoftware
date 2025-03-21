Imports System.Data
Imports System.Data.OleDb

Public Module _MailboxPackagesDB
#Region "Common/Private Functions"
    'Private dset As New DataSet
    Private dadapter As New OleDbDataAdapter
    Private m_path2db As String

    Public Property path2db() As String
        Get
            Return m_path2db
        End Get
        Set(ByVal value As String)
            m_path2db = value
        End Set
    End Property

    Private Function get_dreader(ByVal sql2exe As String, ByRef dreader As OleDbDataReader, Optional ByVal isRead As Boolean = False) As Boolean
        get_dreader = False ' assume.
        Dim errorDesc As String = String.Empty
        If _Connection.GetDataReader(_Connection.Jet_OLEDB, path2db, sql2exe, dreader, errorDesc, String.Empty) Then
            If isRead Then
                get_dreader = dreader.Read
            Else
                get_dreader = dreader.HasRows
            End If
        Else
            Throw New ArgumentException(errorDesc)
        End If
    End Function

    Private Function get_dreader_onevalue(ByVal sql2exe As String, ByRef onevalue As String) As Boolean
        get_dreader_onevalue = False
        Dim dreader As OleDb.OleDbDataReader = Nothing
        onevalue = String.Empty ' assume.
        If get_dreader(sql2exe, dreader, True) Then
            onevalue = _Convert.Null2DefaultValue(dreader(0), "")
            get_dreader_onevalue = True
        End If
        _Connection.CloseDataReader(dreader)
    End Function

    Private Function get_dreader_onevalue(ByVal sql2exe As String, ByRef onevalue As Long) As Boolean
        get_dreader_onevalue = False
        Dim dreader As OleDb.OleDbDataReader = Nothing
        onevalue = -1 ' assume.
        If get_dreader(sql2exe, dreader, True) Then
            onevalue = _Convert.Null2DefaultValue(dreader(0), 0)
            get_dreader_onevalue = True
        End If
        _Connection.CloseDataReader(dreader)
    End Function

    Private Function is_exists(ByVal sql2exe As String, Optional ByRef retvalue As Object = Nothing) As Boolean
        Dim dreader As OleDbDataReader = Nothing
        Dim errorDesc As String = String.Empty
        retvalue = Nothing ' assume.
        If _Connection.GetDataReader(_Connection.Jet_OLEDB, path2db, sql2exe, dreader, errorDesc, String.Empty) Then
            If dreader.Read Then
                If Not IsDBNull(dreader(0)) Then
                    retvalue = dreader(0)
                End If
            End If
        Else
            Throw New ArgumentException(errorDesc)
        End If
        is_exists = (Not retvalue Is Nothing)
    End Function

    Public Sub Close_dreader(ByRef dreader As OleDb.OleDbDataReader)
        If dreader IsNot Nothing Then
            If Not dreader.IsClosed Then
                dreader.Close()
            End If
            dreader = Nothing
        End If
    End Sub
#End Region

    Public Function Create_DataSet_ShipRiteReports(ByVal dtablename As String, ByVal sql2exe As String, ByRef dset As DataSet) As Boolean
        Create_DataSet_ShipRiteReports = False
        Dim dreader As OleDb.OleDbDataReader = Nothing
        If get_dreader(sql2exe, dreader) Then
            Create_DataSet_ShipRiteReports = _DataSet.Build_DataSet(dtablename, dreader, dset)
        End If
        dreader = Nothing
    End Function
    Public Function Read_Packages_By_Name(ByVal name As String, ByRef packageList As String) As Boolean
        Dim SQL As String = "SELECT * " &
                            "FROM Mailbox_Packages " &
                            "WHERE MailboxName LIKE '%" & name & "%' AND PickedupBy IS Null"
        packageList = IO_GetSegmentSet(gMailboxDB, SQL)
        Return packageList.Length > 0
    End Function
    Public Function Read_Packages_By_Number(ByVal box As Integer, ByRef packageList As String) As Boolean
        Dim SQL As String = "SELECT * " &
                            "FROM Mailbox_Packages " &
                            "WHERE MailboxNo = " & box & " AND PickedupBy IS Null"
        packageList = IO_GetSegmentSet(gMailboxDB, SQL)
        Return packageList.Length > 0
    End Function
    Public Function Read_Packages(ByRef packagesList As String) As Boolean
        Read_Packages = False ' assume.

        Dim sql2exe As String = "Select * " &
                                "From Mailbox_Packages " &
                                "Where PickedupBy Is Null" 'read all on hand

        packagesList = IO_GetSegmentSet(gMailboxDB, sql2exe)

        If Not packagesList = "" Then
            Read_Packages = True
        End If

    End Function
    Public Function Read_Packages(ByVal mailbox As String, ByRef packagesList As String) As Boolean
        Read_Packages = False ' assume.
        Dim sql2exe As String = ""

        If String.IsNullOrEmpty(mailbox) Then
            sql2exe = "Select * " &
                                    "From Mailbox_Packages " &
                                    "Where PickedupBy Is Null And MailboxNo > 0"
        Else
            sql2exe = "Select * " &
                                    "From Mailbox_Packages " &
                                    "Where PickedupBy Is Null And MailboxNo = " & mailbox
        End If

        packagesList = IO_GetSegmentSet(gMailboxDB, sql2exe)

        If Not packagesList = "" Then
            Read_Packages = True
        End If
    End Function
    Public Function Read_Packages(ByVal mailbox As String, ByVal mailboxname As String, ByRef packagesList As String) As Boolean
        Read_Packages = False ' assume.
        If 0 = mailbox.Length Then
            Return Read_Packages(packagesList) 'read all on hand
        Else
            Dim sql2exe As String = "Select * " &
                                    "From Mailbox_Packages " &
                                    "Where PickedupBy Is Null And MailboxName = '" & mailboxname & "' And MailboxNo = " & mailbox
            packagesList = IO_GetSegmentSet(gMailboxDB, sql2exe)

            If packagesList = "" Then
                'if no exact match found, then search for partial match
                sql2exe = "Select * " &
                          "From Mailbox_Packages " &
                          "Where PickedupBy Is Null And MailboxName LIKE '%" & mailboxname & "%'"
                packagesList = IO_GetSegmentSet(gMailboxDB, sql2exe)
            End If
        End If

        If Not packagesList = "" Then
            Read_Packages = True
        End If

    End Function

     Public Function Read_Packages(carrierName As String, packClass As String, ByRef packagesList As String, Optional fromDate As Date? = Nothing, Optional toDate As Date? = Nothing) As Boolean
        Read_Packages = False ' assume.
        Dim dateStr As String = String.Empty
        If fromDate IsNot Nothing Then
            dateStr = "And ReceivedDate >= #" & fromDate.Value.ToString("d") & "# "
        End If
        If toDate IsNot Nothing Then
            toDate = CDate(toDate).AddDays(1) 'Need to add a date since querried field is a DateTime field which by default adds a time of midnight to the date.
            dateStr &= "And ReceivedDate <= #" & toDate.Value.ToString("d") & "# "
        End If
        Dim sql2exe As String = "Select PackageID, CarrierName, IsGround, TrackingNo, ReceivedDate, PackageClass " &
                                "From Mailbox_Packages " &
                                "Where CarrierName='" & carrierName & "' " &
                                "And PackageClass='" & packClass & "' " &
                                dateStr &
                                "Order by PackageID"
        packagesList = IO_GetSegmentSet(gMailboxDB, sql2exe)
        If Not packagesList = "" Then
            Read_Packages = True
        End If
    End Function
    
    Public Function Read_Packages_ByClass(ByVal packclass As String, ByRef packagesList As String) As Boolean
        Read_Packages_ByClass = False ' assume.
        If String.IsNullOrEmpty(packclass) Then
            Return Read_Packages(packagesList) 'read all on hand
        Else
            Dim sql2exe As String = "Select * " &
                                    "From Mailbox_Packages " &
                                    "Where PickedupBy Is Null And PackageClass = '" & packclass & "'"
            packagesList = IO_GetSegmentSet(gMailboxDB, sql2exe)
        End If

        If Not packagesList = "" Then
            Read_Packages_ByClass = True
        End If
    End Function

    Public Function Read_Package(ByVal trackingno As String, ByRef packagesList As String) As Boolean
        Read_Package = False ' assume.
        Dim sql2exe As String = "Select * " &
                                "From Mailbox_Packages " &
                                "Where TrackingNo = '" & trackingno & "'"
        packagesList = IO_GetSegmentSet(gMailboxDB, sql2exe)

        If Not packagesList = "" Then
            Read_Package = True
        End If
    End Function
    Public Function Read_Package_ByBarcodeScan(ByVal barcode As String, ByRef packagesList As String) As Boolean
        Read_Package_ByBarcodeScan = False ' assume.
        Dim sql2exe As String = "Select * " &
                                "From Mailbox_Packages " &
                                "Where BarCodeScan = '" & barcode & "'"

        packagesList = IO_GetSegmentSet(gMailboxDB, sql2exe)

        If Not packagesList = "" Then
            Read_Package_ByBarcodeScan = True
        End If
    End Function

    Public Function Read_MailboxNames(ByVal mailbox As String, ByRef mailBoxList As String) As Boolean
        Dim sql2exe As String = "Select Distinct MailboxName " &
                                "From Mailbox_Packages " &
                                "Where PickedupBy Is Null And MailboxNo = " & mailbox & " " &
                                "Order by MailboxName"
        mailBoxList = IO_GetSegmentSet(gMailboxDB, sql2exe)

        If Not mailBoxList = "" Then
            Read_MailboxNames = True
        Else
            Read_MailboxNames = False
        End If
    End Function
    Public Function Read_MailboxNames_ByDateRange(ByVal mailbox As String, ByVal fromDate As Date, ByVal toDate As Date, ByRef mailBoxList As String) As Boolean
        Dim sql2exe As String = "Select Distinct MailboxName " &
                                "From Mailbox_Packages " &
                                "Where ReceivedDate Between #" & fromDate.ToString("d") & "# And #" & toDate.ToString("d") & "# And MailboxNo = " & mailbox & " " &
                                "Order by MailboxName"
        mailBoxList = IO_GetSegmentSet(gMailboxDB, sql2exe)

        If Not mailBoxList = "" Then
            Read_MailboxNames_ByDateRange = True
        Else
            Read_MailboxNames_ByDateRange = False
        End If
    End Function
    Public Function Read_MailboxNames_All(ByVal mailbox As String, ByRef mailBoxList As String) As Boolean
        Dim sql2exe As String = "Select Distinct MailboxName " &
                                "From Mailbox_Packages " &
                                "Where MailboxNo = " & mailbox & " " &
                                "Order by MailboxName"
        mailBoxList = IO_GetSegmentSet(gMailboxDB, sql2exe)

        If Not mailBoxList = "" Then
            Read_MailboxNames_All = True
        Else
            Read_MailboxNames_All = False
        End If
    End Function
    Public Function Read_MailboxNames_All(ByRef mailBoxList As String) As Boolean
        Dim sql2exe As String = "Select Distinct MailboxName " &
                                "From Mailbox_Packages " &
                                "Order by MailboxName"
        mailBoxList = IO_GetSegmentSet(gMailboxDB, sql2exe)

        Read_MailboxNames_All = mailBoxList.Length > 0
    End Function
    Public Function Read_PickupNames_All(ByRef pickupList As String) As Boolean
        Dim sql2exe As String = "Select Distinct PickedupBy " &
                                "From Mailbox_Packages " &
                                "Order by PickedupBy"
        pickupList = IO_GetSegmentSet(gMailboxDB, sql2exe)

        If Not pickupList = "" Then
            Read_PickupNames_All = True
        Else
            Read_PickupNames_All = False
        End If

    End Function

    Public Function Get_MailboxName(ByVal trackingno As String) As String
        Get_MailboxName = String.Empty ' assume.
        Dim sql2exe As String = "Select MailboxName From Mailbox_Packages Where TrackingNo = '" & trackingno & "'"

        Dim buf As String = ""
        Dim mailboxName As String = ""

        buf = IO_GetSegmentSet(gMailboxDB, sql2exe)
        mailboxName = ExtractElementFromSegment("MailboxName", buf)

        If Not mailboxName = "" Then
            Get_MailboxName = mailboxName
        End If
    End Function

    Public Function IsExist_TrackingNo(ByVal trackingno As String) As Boolean
        IsExist_TrackingNo = True ' assume.
        Dim sql2exe As String = "Select PackageID From Mailbox_Packages Where TrackingNo = '" & trackingno & "'"


        If IO_GetSegmentSet(gMailboxDB, sql2exe) = "" Then
            Return False
        Else
            Return True
        End If

    End Function

    Public Function Mailbox_GetExpirationDate(ByVal mailboxno As Long, ByRef expdate As String, ByRef isRented As Boolean) As Boolean
        Dim sql2exe As String = "Select [EndDate], [Rented] From Mailbox Where [MailboxNumber] = " & mailboxno
        Try
            Dim buf As String = ""
            Dim rentStatus As String = ""

            buf = IO_GetSegmentSet(gShipriteDB, sql2exe)

            If Not buf = "" Then
                'Found Mailbox
                rentStatus = ExtractElementFromSegment("Rented", buf)

                expdate = ExtractElementFromSegment("EndDate", buf)

                If Not rentStatus = "" Then
                    isRented = CType(rentStatus, Boolean)
                End If

                Return True
            Else
                'Mailbox Not Found
                isRented = False

                buf = IO_GetSegmentSet(gShipriteDB, "SELECT SizeDesc From MailboxSize WHERE StartingNumber <= " & mailboxno & " AND EndingNumber >= " & mailboxno)
                If buf = "" Then
                    'Mailbox doesn't exist
                    Return False
                Else
                    'Mailbox exists but not rented.
                    Return True
                End If

            End If

        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Public Function Mailbox_GetNameList(ByVal mailboxno As Long, ByRef mailBoxList As String) As Boolean
        Mailbox_GetNameList = False ' assume.
        ''ol#1.2.23(11/3)... Eliminated empty names in Name drop-down box.
        Dim sql2exe As String = "SELECT MBXNamesList.Name as MboxName, Contacts.EMail as MboxEmail, Contacts.CellPhone, Contacts.CellDomain " &
                                "FROM MBXNamesList INNER JOIN Contacts ON MBXNamesList.CID = Contacts.ID " &
                                "WHERE MBXNamesList.MBX = " & mailboxno.ToString & " And MBXNamesList.Name <> '' " &
                                "Union " &
                                "SELECT Contacts.Name as MboxName, Contacts.EMail as MboxEmail, Contacts.CellPhone, Contacts.CellDomain " &
                                "FROM MBXNamesList INNER JOIN Contacts ON MBXNamesList.CID = Contacts.ID " &
                                "WHERE MBXNamesList.MBX = " & mailboxno.ToString & " And MBXNamesList.Name = '' " &
                                "Order by 1"

        mailBoxList = IO_GetSegmentSet(gShipriteDB, sql2exe)

        If Not mailBoxList = "" Then
            Mailbox_GetNameList = True
        End If

    End Function

    Public Function Mailbox_GetName(ByVal mailboxno As Long) As String
        Mailbox_GetName = String.Empty ' assume.
        Dim mailboxname As String = String.Empty
        Dim sql2exe As String = "Select [Name] From Mailbox Where [MailboxNumber] = " & mailboxno.ToString

        Dim buf As String = ""

        buf = IO_GetSegmentSet(gShipriteDB, sql2exe)
        mailboxname = ExtractElementFromSegment("Name", buf)

        If Not mailboxname = "" Then
            Mailbox_GetName = mailboxname
        End If

    End Function

    Public Function Mailbox_GetNumber(ByVal name As String) As Long
        Dim mbox As Long = 0

        Dim sql As String = "Select [MailboxNumber] From Mailbox Where Name LIKE """ & name & "%"""
        Dim segment = IO_GetSegmentSet(gShipriteDB, sql)
        segment = SegmentFunctions.GetNextSegmentFromSet(segment)
        If SegmentFunctions.IsElementInSegment("MailboxNumber", segment) Then
            mbox = Convert.ToInt64(ExtractElementFromSegment("MailboxNumber", segment))
        End If

        Return mbox
    End Function
End Module
