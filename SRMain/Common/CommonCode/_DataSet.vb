Imports System.Data
Imports System.Data.Common
Imports System.Data.OleDb
Imports System.IO
Imports System.Text.RegularExpressions

''EXAMPLE: create DataSet Tabels from scratch:
''Dim sql2exe As String = "SELECT CarrierOnOffValues.CarrierID, CarrierSetup.CarrierName, CarrierOnOffValues.SettingID, CarrierOnOffSettings.SettingName, CarrierOnOffValues.SettingValue, SettingGroups.GroupName " & _
''"FROM SettingGroups INNER JOIN (CarrierSetup INNER JOIN (CarrierOnOffSettings INNER JOIN CarrierOnOffValues ON CarrierOnOffSettings.SettingID = CarrierOnOffValues.SettingID) ON CarrierSetup.CarrierID = CarrierOnOffValues.CarrierID) ON SettingGroups.GroupID = CarrierOnOffSettings.GroupID " & _
''"Order By CarrierSetup.CarrierID"
''If _Connection.GetDataReader(Globals.gMasterSetupDb, sql2exe, dreader, errorDesc, "") Then
''    load_DataSet_WithCarrierOnOffValues = _DataSet.Build_DataSet(tblCarrierOnOffValues, dreader, dset)
''    If load_DataSet_WithCarrierOnOffValues Then '' debug only
''        ''_Debug.Print_(dset.Tables(0).TableName, " count = " & dset.Tables(0).Rows.Count.ToString)
''        ''_DataSet.Print_DataTable(dset.Tables(tblCarrierOnOffValues))
''    End If
''End If
''If _DataAdapter.Map_DataTables("CarrierOnOffValues", tblCarrierOnOffValues, dadapter) Then

Public Module _DataSet

    Private Function create_AutoIncrementedColumn(ByVal columnName As String, ByRef retDtable As DataTable) As Boolean
        ''
        create_AutoIncrementedColumn = True '' assume.
        Try
            '' Create an identity, auto-incremented column.
            Dim dcolumn As New DataColumn(columnName, GetType(Integer))
            dcolumn.AutoIncrement = True ' Make it auto-increment.
            dcolumn.AutoIncrementSeed = 1
            dcolumn.AllowDBNull = False ' Default is True.
            dcolumn.Unique = True ' All key columns should be unique.
            retDtable.Columns.Add(dcolumn) ' Add to Columns collection.
            ' Make it the primary key. (Create the array on-the-fly.)
            retDtable.PrimaryKey = New DataColumn() {dcolumn}
            ''
        Catch ex As Exception : _Debug.PrintError_("_DataSet.create_AutoIncrementedColumn(): " & ex.Message) : create_AutoIncrementedColumn = False
        End Try
        ''
    End Function
    Public Function Create_TableColumn(ByVal drow As DataRow, ByRef retDtable As DataTable) As Boolean
        ''
        Create_TableColumn = True '' assume.
        Try
            '' Create an identity, auto-incremented column.
            Dim dcolumn As New DataColumn(CStr(drow("ColumnName")))
            dcolumn.DataType = CType(drow("DataType"), Type)
            If dcolumn.DataType.ToString = "System.String" Then
                dcolumn.MaxLength = CInt(drow("ColumnSize"))
            End If
            ''dcolumn.AllowDBNull = False ' Default is True.
            dcolumn.Unique = CBool(drow("IsUnique")) ' All key columns should be unique.
            retDtable.Columns.Add(dcolumn) ' Add to Columns collection.
            ''
        Catch ex As Exception : _Debug.PrintError_("_DataSet.Create_TableColumn(): " & ex.Message) : Create_TableColumn = False
        End Try
        ''
    End Function

    Public Function Copy_TableColumns(ByVal dreader As OleDbDataReader, ByRef retDtable As DataTable) As Boolean
        ''
        Copy_TableColumns = True '' aasume.
        Try
            Dim dtable As DataTable = dreader.GetSchemaTable()
            '' Display name, data type, size, and unique attribute for all columns.
            Dim str As String = String.Format("{0}, {1}, {2}, {3}", "ColumnName", "DataType", "ColumnSize", "IsUnique")
            _Debug.Print_(str)
            For Each drow As DataRow In dtable.Rows
                ''
                str = String.Format("{0}, {1}, {2}, {3}", drow("ColumnName"), drow("DataType"), drow("ColumnSize"), drow("IsUnique"))
                _Debug.Print_(str)
                If Not create_TableColumn(drow, retDtable) Then
                    Exit For
                End If
                ''
            Next drow
            ''
        Catch ex As Exception : _Debug.PrintError_("_DataSet.Copy_TableColumns(): " & ex.Message) : Copy_TableColumns = False
        End Try
        ''
    End Function
    Private Function copy_DataTableColumnsSchema(ByVal dtableSchema As DataTable, ByVal retDtable As DataTable) As Boolean
        ''
        copy_DataTableColumnsSchema = True '' assume.
        ''
        Try
            '' Copy name, data type, size, and unique attribute for all columns.
            For Each drow As DataRow In dtableSchema.Rows
                ''
                create_TableColumn(drow, retDtable)
                ''
            Next drow
            ''
        Catch ex As Exception : _Debug.Print_("_DataSet.copy_DataTableColumnsSchema(): " & ex.Message) : copy_DataTableColumnsSchema = False

        End Try
        ''
    End Function
    Private Function copy_DataTableRows(ByVal dreader As OleDbDataReader, ByVal dtableSchema As DataTable, ByRef retDtable As DataTable) As Boolean
        ''
        copy_DataTableRows = True '' assume.
        ''
        Try
            Do While dreader.Read
                ''
                '' Create a new row with the same schema.
                Dim drow As DataRow = retDtable.NewRow()
                '' Get column names.
                For Each drowSchema As DataRow In dtableSchema.Rows
                    '' Set all the columns.
                    drow(drowSchema("ColumnName").ToString) = dreader.Item(drowSchema("ColumnName").ToString)
                    ''Dim str As String = String.Format("{0} = {1}", drowSchema("ColumnName").ToString, drow(drowSchema("ColumnName").ToString).ToString) : _Debug.Print_(str)
                Next drowSchema
                ''
                '' Add to the Rows collection.
                retDtable.Rows.Add(drow)
                ''_Debug.Print_(drow("ColumnName"))
                ''_Debug.Print_(dtable.Rows(i)("CarrierID"))
                ''
            Loop
            ''
        Catch ex As Exception : _Debug.Print_("_DataSet.copy_DataTableRows(): " & ex.Message) : copy_DataTableRows = False
        End Try
        ''
    End Function

    Private Function get_DataTableSchema(ByVal dreader As OleDbDataReader, ByRef retDtable As DataTable) As Boolean
        ''
        get_DataTableSchema = True '' aasume.
        Try
            retDtable = dreader.GetSchemaTable()
            ''
        Catch ex As Exception : _Debug.PrintError_("_DataSet.get_DataTableSchema(): " & ex.Message) : get_DataTableSchema = False
        End Try
        ''
    End Function

    Private Sub print_DataTableColumnsSchema(ByVal dtableSchema As DataTable)
        ''
        '' Display name, data type, size, and unique attribute for all columns.
        Dim str As String = String.Format("{0}, {1}, {2}, {3}", "ColumnName", "DataType", "ColumnSize", "IsUnique")
        _Debug.Print_(str)
        For Each drow As DataRow In dtableSchema.Rows
            ''
            str = String.Format("{0}, {1}, {2}, {3}", drow("ColumnName"), drow("DataType"), drow("ColumnSize"), drow("IsUnique"))
            _Debug.Print_(str)
            ''
        Next drow
        ''
    End Sub
    Private Sub print_DataTableRows(ByVal dtable As DataTable, ByVal fieldName2Print As String)
        ''
        Try
            For i As Integer = 0 To dtable.Rows.Count - 1
                ''
                Dim drow As DataRow = dtable.Rows(i)
                ''Dim str As String = String.Format("{0}, {1}", drow("ColumnName"), drow(drow("ColumnName").ToString).ToString)
                ''_Debug.Print_(drow("ColumnName"))
                _Debug.Print_((dtable.Rows(i)(fieldName2Print)).ToString)
                ''
            Next i
            ''
        Catch ex As Exception : _Debug.Print_("_DataSet.print_DataTableRows(): " & ex.Message)
        End Try
        ''
    End Sub
    Public Sub Print_DataTable(ByVal dtable As DataTable)
        Try
            For r As Integer = 0 To dtable.Rows.Count - 1
                For c As Integer = 0 To dtable.Columns.Count - 1
                    ''
                    Dim str As String = String.Format("{0} = {1}", dtable.Columns(c).ColumnName, dtable.Rows(r)(dtable.Columns(c).ColumnName)) : _Debug.Print_(str)
                    ''
                Next c
            Next r
            ''
        Catch ex As Exception : _Debug.Print_("_DataSet.Print_DataTable(): " & ex.Message)
        End Try
    End Sub
    Public Sub Print_DataRows(ByVal drows() As DataRow, ByVal fieldName2Print As String)
        ''
        Try
            For i As Integer = 0 To drows.GetUpperBound(0)
                ''
                Dim drow As DataRow = drows(i)
                ''Dim str As String = String.Format("{0}, {1}", drow("ColumnName"), drow(drow("ColumnName").ToString).ToString)
                ''_Debug.Print_(drow("ColumnName"))
                _Debug.Print_((drows(i)(fieldName2Print)).ToString)
                ''
            Next i
            ''
        Catch ex As Exception : _Debug.Print_("_DataSet.Print_DataRows(): " & ex.Message)
        End Try
        ''
    End Sub

    Public Function Build_NewDataSet(ByVal tableName As String, ByRef retDset As DataSet) As Boolean
        ''
        '' Create a table; set its initial capacity.
        Dim dtable As New DataTable(tableName)
        dtable.MinimumCapacity = 100
        '' Add created table to DataSet.
        Call retDset.Tables.Add(dtable)
        Build_NewDataSet = (tableName = retDset.Tables(tableName).TableName)
        ''
    End Function
    Public Function Build_DataSet(ByVal tableName As String, ByVal dreader As OleDbDataReader, ByRef retDset As DataSet) As Boolean
        Build_DataSet = False
        ''
        '' Create a table; set its initial capacity.
        Dim dtable As New DataTable(tableName)
        Dim dtableSchema As New DataTable(tableName)
        dtable.MinimumCapacity = 100
        ''
        '' Get a reference to the table.
        If get_DataTableSchema(dreader, dtableSchema) Then
            If copy_DataTableColumnsSchema(dtableSchema, dtable) Then '' Copy all columns.
                If copy_DataTableRows(dreader, dtableSchema, dtable) Then '' Copy all data in rows.
                    '' Add created table to DataSet.
                    Call retDset.Tables.Add(dtable)
                    Build_DataSet = (tableName = retDset.Tables(tableName).TableName)
                    ''
                End If
            End If
            ''
        End If
        ''
    End Function
    Public Function Build_DataSet_WithEmptyTable(ByVal tableName As String, ByVal dreader As OleDbDataReader, ByRef retDset As DataSet) As Boolean
        Build_DataSet_WithEmptyTable = False
        ''
        '' Create a table; set its initial capacity.
        Dim dtable As New DataTable(tableName)
        Dim dtableSchema As New DataTable(tableName)
        dtable.MinimumCapacity = 100
        ''
        '' Get a reference to the table.
        If get_DataTableSchema(dreader, dtableSchema) Then
            If copy_DataTableColumnsSchema(dtableSchema, dtable) Then '' Copy all columns.
                '' Add created table to DataSet.
                Call retDset.Tables.Add(dtable)
                Build_DataSet_WithEmptyTable = (tableName = retDset.Tables(tableName).TableName)
            End If
            ''
        End If
        ''
    End Function
    Public Function Build_DataTable_WithoutData(ByVal dreader As OleDbDataReader, ByRef dtable As DataTable) As Boolean
        Build_DataTable_WithoutData = False
        ''
        '' Create a table; set its initial capacity.
        Dim dtableSchema As New DataTable(dtable.TableName)
        dtable.MinimumCapacity = 100
        ''
        '' Get a reference to the table.
        If get_DataTableSchema(dreader, dtableSchema) Then
            Build_DataTable_WithoutData = copy_DataTableColumnsSchema(dtableSchema, dtable) '' Copy all columns.
        End If
        ''
    End Function
    Public Function Build_DataTable_WithData(ByVal dreader As OleDbDataReader, ByRef dtable As DataTable) As Boolean
        Build_DataTable_WithData = False
        ''
        '' Create a table; set its initial capacity.
        Dim dtableSchema As New DataTable(dtable.TableName)
        dtable.MinimumCapacity = 100
        ''
        '' Get a reference to the table.
        If get_DataTableSchema(dreader, dtableSchema) Then
            If copy_DataTableColumnsSchema(dtableSchema, dtable) Then '' Copy all columns.
                Build_DataTable_WithData = copy_DataTableRows(dreader, dtableSchema, dtable) '' Copy all data in rows.
            End If
        End If
        ''
    End Function

    Public Function IsExist_DataTable(ByVal dset As DataSet, ByVal dtableName As String, Optional ByRef dtable As DataTable = Nothing) As Boolean
        IsExist_DataTable = False
        Try
            IsExist_DataTable = dset.Tables.Contains(dtableName)
            If IsExist_DataTable Then
                dtable = dset.Tables(dtableName)
            End If
        Catch ex As Exception : _Debug.Print_("_DataSet.IsDataTableExist(" & dtableName & "): " & ex.Message)
        End Try
    End Function
    Public Function IsModified(ByVal dset As DataSet) As Boolean
        IsModified = False
        ''
        Try
            '' check if any data tables were updated 
            If Not (dset Is Nothing) AndAlso dset.Tables.Count > 0 Then
                IsModified = dset.HasChanges(DataRowState.Modified)
            End If
            ''
        Catch ex As Exception : _Debug.Print_("_DataSet.IsModified(): " & ex.Message)
        End Try
        ''
    End Function

    Public Function Add_DataTable2DataSet(ByVal tableName As String, ByRef retDset As DataSet) As Boolean
        Add_DataTable2DataSet = False
        ''
        Try
            '' Create a table; set its initial capacity.
            Dim dtable As New DataTable(tableName)
            dtable.MinimumCapacity = 100
            ''
            '' Add created table to DataSet.
            Call retDset.Tables.Add(dtable)
            Add_DataTable2DataSet = (tableName = retDset.Tables(tableName).TableName)
            ''
        Catch ex As Exception : _Debug.PrintError_("_DataSet.Add_DataTable2DataSet(): " & ex.Message)
        End Try
        ''
    End Function

    Public Function Load_DataTable(ByVal whatType As Byte, ByVal dtableName As String, ByVal path2db As String, ByVal sql2exe As String, ByRef dset As DataSet, ByRef dadapter As OleDbDataAdapter, ByRef errorDesc As String, Optional ByVal is2reload As Boolean = False) As Boolean
        Load_DataTable = False
        Try
            Load_DataTable = IsExist_DataTable(dset, dtableName)
            If Load_DataTable Then ' if dtable is empty then force to read/fill it again
                If is2reload Then
                    dset.Tables(dtableName).Rows.Clear() ' we need this for ASP.Net pages, which reloads each time user click a control on the page
                End If
                Load_DataTable = (0 < dset.Tables(dtableName).Rows.Count)
            End If
            If Not Load_DataTable Then
                ''
                Load_DataTable = _DataAdapter.Fill_DataTable(whatType, path2db, dtableName, sql2exe, dset, dadapter, errorDesc)
                If Not Load_DataTable Then
                    If 0 < errorDesc.Length Then
                        _Debug.PrintError_("_DataSet.Load_DataTable.Fill_DataTable(" & dtableName & "): " & errorDesc)
                    Else
                        Load_DataTable = IsExist_DataTable(dset, dtableName) ' table loaded but there is no data in it
                    End If
                End If
                ''
            End If
            ''
        Catch ex As Exception : errorDesc = ex.Message : _Debug.PrintError_("_DataSet.Load_DataTable(" & dtableName & "): " & ex.Message)
        End Try
    End Function
    Public Function Filter_DataTable(ByVal dtable As DataTable, ByVal filter As String, ByRef retDrows() As DataRow) As Boolean
        If IsNothing(dtable) Then Return False

        Filter_DataTable = False
        Try
            retDrows = dtable.Select(filter)
            Filter_DataTable = (0 < retDrows.GetLength(0))
        Catch ex As Exception : _Debug.Print_("_DataSet.Filter_DataTable(): " & ex.Message)
        End Try
    End Function

End Module
