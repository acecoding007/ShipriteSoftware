Imports System.Data
Imports System.Data.OleDb

Public Module _Connection

    'Private WithEvents cn As OleDbConnection
    'Private Sub cn_StateChange(ByVal sender As Object, ByVal e As System.Data.StateChangeEventArgs) Handles cn.StateChange
    '    ''
    '    '' Show the status of the connection.
    '    If Not 0 = (e.CurrentState And ConnectionState.Open) Then
    '        _Debug.PrintError_("The connection has been opened")
    '    ElseIf e.CurrentState = ConnectionState.Closed Then
    '        _Debug.PrintError_("The connection has been closed")
    '    End If
    '    ''
    'End Sub
    Private poolConnections As New List(Of OleDb.OleDbConnection) ' keeps one instance of each of different connections
    Public Const Jet_OLEDB As Byte = 1
    Public Const Oracle_OLEDB As Byte = 2
    Public Const Jet_OLEDB_CSV As Byte = 3
    Public Const Sql_OLEDB As Byte = 4

    Private m_UserID_Oracle As String
    Private m_UserPA_Oracle As String
    Private m_UserID_Sql As String
    Private m_UserPA_Sql As String
    Public Property UserID_Oracle() As String
        Get
            Return m_UserID_Oracle
        End Get
        Set(ByVal value As String)
            m_UserID_Oracle = value
        End Set
    End Property
    Public Property UserPA_Oracle() As String
        Get
            Return m_UserPA_Oracle
        End Get
        Set(ByVal value As String)
            m_UserPA_Oracle = value
        End Set
    End Property
    Public Property UserID_Sql() As String
        Get
            Return m_UserID_Sql
        End Get
        Set(ByVal value As String)
            m_UserID_Sql = value
        End Set
    End Property
    Public Property UserPA_Sql() As String
        Get
            Return m_UserPA_Sql
        End Get
        Set(ByVal value As String)
            m_UserPA_Sql = value
        End Set
    End Property

    Private Function getConnectionString(ByVal whatType As Byte, ByVal path2db As String, Optional isJust2Compare As Boolean = False) As String
        ''
        getConnectionString = String.Empty
        Select Case whatType
            ' we only pass CSV as the folder.  The csv file import is in the query which follows
            Case _Connection.Jet_OLEDB_CSV : getConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" & path2db & ";Extended Properties=""Text;HDR=No;FMT=Delimited;"""
            Case _Connection.Jet_OLEDB : getConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" & path2db & ";"
                ' Production Db:
                'Case _Connection.Oracle_OLEDB : getConnectionString = ("Provider=OraOLEDB.Oracle.1;Password=" & UserPA_Oracle & ";Persist Security Info=True;User ID=" & UserID_Oracle & ";Data Source=PROD.WORLD")
            Case _Connection.Oracle_OLEDB
                If isJust2Compare Then ' Oracle connection object will hide the password
                    getConnectionString = ("Provider=OraOLEDB.Oracle;User ID=" & UserID_Oracle & ";Data Source=PROD.WORLD;OLEDB.NET=true")
                Else
                    getConnectionString = ("Provider=OraOLEDB.Oracle;Password=" & UserPA_Oracle & ";User ID=" & UserID_Oracle & ";Data Source=PROD.WORLD;OLEDB.NET=true")
                End If
                ' Test Db:
                'Case _Connection.Oracle_OLEDB : getConnectionString = "Provider=OraOLEDB.Oracle.1;Password=golfgreen;Persist Security Info=True;User ID=odonchuk;Data Source=TEST.WORLD"
                ' Original production LCTrackDb:
            Case _Connection.Sql_OLEDB : getConnectionString = "Provider=SQLOLEDB.1;Password=" & UserPA_Sql & ";Persist Security Info=True;User ID=" & UserID_Sql & ";Initial Catalog=LCTrack;Data Source=DBSERVER\MVCC_SqlServer"
                ' TEST LCTrackDB_new:
                'Case _Connection.Sql_OLEDB : getConnectionString = "Provider=SQLOLEDB.1;Password=" & UserPA_Sql & ";Persist Security Info=True;User ID=" & UserID_Sql & ";Initial Catalog=LCTrack_new;Data Source=DBSERVER\MVCC_SqlServer"
        End Select
        ''
    End Function
    Private Function pool_IsExistsConnection(ByVal whatType As Byte, ByVal path2db As String, ByRef cn As OleDb.OleDbConnection) As Boolean
        pool_IsExistsConnection = False
        Try
            For i% = 0 To poolConnections.Count - 1
                '_Debug.Print_(getConnectionString(whatType, path2db, True))
                '_Debug.Print_(poolConnections.Item(i%).ConnectionString)
                If 0 = String.Compare(getConnectionString(whatType, path2db, True), poolConnections.Item(i%).ConnectionString) Then
                    cn = poolConnections.Item(i%) ' return found connection object
                    pool_IsExistsConnection = True
                    Exit For
                End If
            Next i%
        Catch ex As Exception
            _Debug.PrintError_("_Connection.pool_IsExistsConnection(" & whatType.ToString & ", " & path2db & "): " & ex.Message)
        End Try
    End Function
    Private Function pool_OpenConnection(ByVal whatType As Byte, ByVal path2db As String, ByRef cn As OleDbConnection, ByRef errorDesc As String) As Boolean
        pool_OpenConnection = False
        Try
            If Not cn.State = ConnectionState.Open Then
                cn = New OleDbConnection(getConnectionString(whatType, path2db))
                '' Open the connection to database.
                cn.Open()
            End If
            ''
            pool_OpenConnection = (cn.State = ConnectionState.Open)
            ''
        Catch ex As Exception
            _Debug.PrintError_("_Connection.pool_OpenConnection(" & whatType.ToString & ", " & path2db & "): " & ex.Message)
            errorDesc = ex.Message
            CloseConnection(cn)
            ''
        End Try
    End Function
    Private Function pool_AddConnection(ByVal whatType As Byte, ByVal path2db As String, ByVal cn As OleDb.OleDbConnection) As Boolean
        pool_AddConnection = False
        Try
            poolConnections.Add(cn)
            pool_AddConnection = (poolConnections.Item(poolConnections.Count - 1).ConnectionString = getConnectionString(whatType, path2db, True))
        Catch ex As Exception
            _Debug.PrintError_("_Connection.pool_AddConnection(" & whatType.ToString & ", " & path2db & "): " & ex.Message)
        End Try
    End Function

    Public Function OpenConnection(ByVal whatType As Byte, ByVal path2db As String, ByRef cn As OleDbConnection, ByRef errorDesc As String, Optional ByRef errorStack As String = "") As Boolean
        OpenConnection = False
        Try
            If Not pool_IsExistsConnection(whatType, path2db, cn) Then
                If pool_OpenConnection(whatType, path2db, cn, errorDesc) Then
                    If Not pool_AddConnection(whatType, path2db, cn) Then
                        _Debug.Print_("Connection string doesn't match...")
                        _Debug.Stop_()
                        ' do something...
                    End If
                End If
            End If
            ''
            OpenConnection = (cn.State = ConnectionState.Open)
            ''
        Catch ex As Exception
            ''
            _Debug.PrintError_("_Connection.OpenConnection(" & whatType.ToString & ", " & path2db & "): " & ex.Message)
            errorDesc = ex.Message
            errorStack = ex.StackTrace
            CloseConnection(cn)
            ''
        End Try
        ''
    End Function

    Public Function CloseAllConnections() As Boolean
        CloseAllConnections = False
        Try
            For i% = poolConnections.Count - 1 To 0 Step -1
                _Connection.CloseConnection(poolConnections.Item(i%))
                poolConnections.Remove(poolConnections.Item(i%))
            Next i%
            CloseAllConnections = True
        Catch ex As Exception
            _Debug.PrintError_("_Connection.Pool_CloseAllConnections(): " & ex.Message)
        End Try
    End Function
    Public Function CloseConnection(ByVal cn As OleDbConnection) As Boolean
        CloseConnection = False
        Try
            '' Close the connection only if it was opened.
            If Not 0 = (cn.State And ConnectionState.Open) Then
                '' (It doesn’t throw an exception even if the Open method failed.)
                cn.Close()
            End If
            ''
        Finally : cn.Dispose()
        End Try
        ''
    End Function
    Public Function CloseDataReader(ByRef dreader As OleDbDataReader) As Boolean
        CloseDataReader = False
        Try
            ' Close the DataReader.
            If Not dreader Is Nothing Then
                If Not dreader.IsClosed Then
                    dreader.Close()
                End If
                dreader = Nothing
            End If
        Catch ex As Exception
            _Debug.PrintError_("_Connection.CloseDataReader(): " & ex.Message)
        End Try
    End Function

    Public Function ExecuteCommand(ByVal whatType As Byte, ByVal path2db As String, ByVal sql2exe As String, ByRef rowsAffected As Integer, ByRef errorDesc As String, ByRef errorStack As String) As Boolean
        ExecuteCommand = False
        Dim cn As New OleDbConnection
        If _Connection.OpenConnection(whatType, path2db, cn, errorDesc, errorStack) Then
            ''
            If whatType = Oracle_OLEDB Then
                sql2exe.Replace("[", "")
                sql2exe.Replace("]", "")
            End If
            Dim cmd As New OleDbCommand(sql2exe, cn)
            cmd.CommandTimeout = 10 ' A 10-second timeout
            Try
                '' Run the query; get the number of affected records.
                rowsAffected = cmd.ExecuteNonQuery()
                ExecuteCommand = True
                '_Debug.PrintError_("ExecuteCommand(): records affected " & rowsAffected.ToString)
                ''
            Catch ex As Exception
                ''
                errorDesc = ex.Message
                errorStack = ex.StackTrace
                ''
                Dim sbuilder As New System.Text.StringBuilder
                sbuilder.Append("_Connection.ExecuteCommand(): ")
                sbuilder.AppendLine(ex.Message)
                sbuilder.AppendLine(sql2exe)
                _Debug.PrintError_(sbuilder.ToString)
                sbuilder = Nothing
                ''
            Finally : cmd.Dispose()
            End Try
            ''
        End If
    End Function

    Public Function GetDataReader(ByVal whatType As Byte, ByVal path2db As String, ByVal sql2exe As String, ByRef dreader As OleDbDataReader, ByRef errorDesc As String, ByRef errorStack As String) As Boolean
        GetDataReader = False
        Dim cn As New OleDbConnection
        If _Connection.OpenConnection(whatType, path2db, cn, errorDesc, errorStack) Then
            ''
            Dim cmd As New OleDbCommand(sql2exe, cn)
            cmd.CommandTimeout = 10 ' A 10-second timeout
            Try
                '' Run the query; get the DataReader object.
                '' CloseConnection argument ensures that the connection is closed automatically when the caller closes the DataReader:
                dreader = cmd.ExecuteReader() ' connection is added to the Pool and needs to be opened.
                'dreader = cmd.ExecuteReader(CommandBehavior.CloseConnection)
                GetDataReader = (Not dreader Is Nothing)
                ''
            Catch ex As Exception
                ''
                errorDesc = ex.Message
                errorStack = ex.StackTrace
                Dim sbuilder As New System.Text.StringBuilder
                sbuilder.Append("OpenDataReader(): ")
                sbuilder.AppendLine(ex.Message)
                sbuilder.AppendLine(sql2exe)
                _Debug.PrintError_(sbuilder.ToString)
                _Connection.CloseDataReader(dreader)
                sbuilder = Nothing
                ''
            Finally : cmd.Dispose()
            End Try
            ''
        End If
        ''
    End Function
    Public Function GetScalarValue(ByVal whatType As Byte, ByVal path2db As String, ByVal sql2exe As String, ByRef retScalar As String, ByRef errorDesc As String, ByRef errorStack As String) As Boolean
        GetScalarValue = False
        Dim cn As New OleDbConnection
        If _Connection.OpenConnection(whatType, path2db, cn, errorDesc, errorStack) Then
            ''
            Dim cmd As New OleDbCommand(sql2exe, cn)
            cmd.CommandTimeout = 10 ' A 10-second timeout
            Try
                '' Executes the query, and returns the first column of the first row in the result set returned by the query. 
                '' Additional columns or rows are ignored.
                '' Use the ExecuteScalar method to retrieve a single value (for example, an aggregate value) from a database. 
                '' This requires less code than using the ExecuteReader method, and then performing the operations that you need to generate the single value using the data returned by a SqlDataReader.
                retScalar = cmd.ExecuteScalar().ToString
                GetScalarValue = True
                ''
            Catch ex As Exception
                ''
                _Debug.PrintError_("GetScalarValue(): " & ex.Message)
                errorDesc = ex.Message
                errorStack = ex.StackTrace
                ''
            Finally : cmd.Dispose()
            End Try
            ''
        End If
        ''
    End Function
    Public Function GetDataAdapter(ByVal whatType As Byte, ByVal path2csv As String, ByRef dadapter As OleDbDataAdapter, ByRef errorDesc As String, ByRef errorStack As String) As Boolean
        GetDataAdapter = False
        Try
            Dim filename As String = path2csv.Substring(path2csv.LastIndexOf("\") + 1)
            Dim path2dir As String = path2csv.Substring(0, path2csv.Length - (path2csv.Length - path2csv.LastIndexOf("\")))
            Dim cn As New OleDbConnection(getConnectionString(whatType, path2dir))
            dadapter = New OleDb.OleDbDataAdapter("Select * from [" & filename & "]", cn)
            GetDataAdapter = (dadapter IsNot Nothing)
            ''
        Catch ex As Exception
            ''
            errorDesc = ex.Message
            errorStack = ex.StackTrace
            _Debug.PrintError_("GetDataAdapter(): " & errorDesc)
            dadapter.Dispose()
            ''
            'Finally : cmd.Dispose()
        End Try
        ''
    End Function
    Public Function GetConnection(ByVal whatType As Byte, ByVal path2db As String, ByRef cn As OleDb.OleDbConnection) As Boolean
        GetConnection = pool_IsExistsConnection(whatType, path2db, cn)
    End Function

End Module
