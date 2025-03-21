Imports System.Data
Imports System.Data.OleDb

Public Module DropOffPackagesDB

#Region "DB Functions for Dropoff"
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

    Private Function execute_cmd(ByVal sql2exe As String) As Boolean
        'Dim errorDesc As String = String.Empty
        'Dim rowsAffected As Integer = 0
        'execute_cmd = False ' assume
        'If _Connection.ExecuteCommand(_Connection.Jet_OLEDB, path2db, sql2exe, rowsAffected, errorDesc, String.Empty) Then
        '    Return True
        'Else
        '    Throw New ArgumentException(errorDesc)
        'End If
        If IO_UpdateSQLProcessor(path2db, sql2exe) >= 0 Then
            Return True
        End If
        Return False
    End Function

    Public Function Execute(ByVal sql2exe As String) As Boolean
        Execute = execute_cmd(sql2exe)
    End Function

    Private Function is_exists(ByVal sql2exe As String, Optional ByRef retvalue As Object = Nothing) As Boolean
        Dim SegmentSet As String = IO_GetSegmentSet(path2db, sql2exe)
        If Not String.IsNullOrEmpty(SegmentSet) Then
            Return True
        End If
        Return False
    End Function

    Public Function IsExist_TrackingNo(ByVal trackingno As String) As Boolean
        IsExist_TrackingNo = True ' assume.
        Dim sql2exe As String = "Select 'Y' From DropOff_Packages Where TrackingNo = '" & trackingno & "'"
        IsExist_TrackingNo = is_exists(sql2exe)
    End Function

    Public Function IsManifest_ClosedForTheDay(ByVal mdate As Date, ByVal carriername As String) As Boolean
        IsManifest_ClosedForTheDay = False ' assume.
        Dim sql2exe As String = "Select 'Y' From DropOff_Packages Where ManifestDate = #" & mdate.ToString("d") & "# And CarrierName = '" & carriername & "'"
        IsManifest_ClosedForTheDay = is_exists(sql2exe)
    End Function

    Public Function IsManifest_ClosedForTheDay(ByVal mdate As Date, ByVal carriername As String, ByVal isGround As Boolean) As Boolean
        IsManifest_ClosedForTheDay = False ' assume.
        Dim sql2exe As String = "Select 'Y' From DropOff_Packages Where ManifestDate = #" & mdate.ToString("d") & "# And CarrierName = '" & carriername & "' And IsGround = " & isGround.ToString
        IsManifest_ClosedForTheDay = is_exists(sql2exe)
    End Function

#End Region

Public Function Create_DropOffPackages_dtable(ByRef dtable As DataTable) As Boolean
        Create_DropOffPackages_dtable = False ' assume.
        Dim sql2exe As String = "Select Top 1 * From DropOff_Packages Where TrackingNo Is Not Null"
        Dim dreader As OleDb.OleDbDataReader = Nothing
        If get_dreader(sql2exe, dreader) Then
            Create_DropOffPackages_dtable = _DataSet.Build_DataTable_WithoutData(dreader, dtable)
        End If
        CloseDataReader(dreader)
    End Function

    Public Function Read_Packages(carrierName As String, ByRef packagesList As String, Optional fromDate As Date? = Nothing, Optional toDate As Date? = Nothing) As Boolean

        Read_Packages = False ' assume.
        Dim dateStr As String = String.Empty

        If fromDate IsNot Nothing Then
            dateStr = "And DropOffDate >= #" & fromDate.Value.ToString("d") & "# "
        End If
        If toDate IsNot Nothing Then
            toDate = CDate(toDate).AddDays(1)
            dateStr &= "And DropOffDate <= #" & toDate.Value.ToString("d") & "# "
        End If
        Dim sql2exe As String = "Select PackageID, CarrierName, IsGround, TrackingNo, DropOffDate " &
                                "From DropOff_Packages " &
                                "Where CarrierName='" & carrierName & "' " &
                                dateStr &
                                "Order by PackageID"

        packagesList = IO_GetSegmentSet(gDropOffDB, sql2exe)
        If Not packagesList = "" Then
            Read_Packages = True
        End If

    End Function
End Module
