Imports System.Data
Imports System.Data.Common
Imports System.Data.OleDb
Imports System.IO
Imports System.Text.RegularExpressions

Public Module _DataAdapter

    Private Sub error_DebugPrint(ByVal routineName As String, ByVal errorDesc As String)
        _Debug.PrintError_(String.Format("_DataAdapter.{0}(): {1}", routineName, errorDesc))
    End Sub

    Public Function Fill_DataTable(ByVal whatType As Byte, ByVal path2db As String, ByVal dtableName As String, ByVal sql2exe As String, ByRef dset As DataSet, ByRef dadapter As OleDbDataAdapter, Optional ByRef errorDesc As String = "") As Boolean
        Fill_DataTable = False
        ''Dim cn As New OleDbConnection
        Try
            ''
            ''If _Connection.OpenConnection(whatType, path2db, cn, errorDesc) Then
            ''    dadapter.SelectCommand = New OleDbCommand(sql2exe, cn)
            ''    Fill_DataTable = (0 < dadapter.Fill(dset, dtableName))
            ''End If
            '
            Dim ConnectionString As String = MakeConnectionString(path2db, "")
            Using connection As New OleDbConnection(ConnectionString)

                connection.ConnectionString = ConnectionString
                connection.Open()
                dadapter.SelectCommand = New OleDbCommand(sql2exe, connection)
                Fill_DataTable = (0 < dadapter.Fill(dset, dtableName))
                connection.Close()

            End Using

            ''
        Catch ex As Exception : errorDesc = ex.Message : _Debug.PrintError_("Fill_DataTable(): " & ex.Message)
        End Try
        ''
    End Function
    Public Function Map_DataTables(ByVal dbaseTableName As String, ByVal dsetTableName As String, ByRef dadapter As OleDbDataAdapter) As Boolean
        Map_DataTables = False
        Try
            dadapter.TableMappings.Add(dbaseTableName, dsetTableName)
            Map_DataTables = True
        Catch ex As Exception : _Debug.PrintError_("Map_DataTables(): " & ex.Message)
        End Try
        ''
    End Function
    Public Function Update_Database_EditRow(ByVal dadapter As OleDbDataAdapter, ByVal dset As DataSet, ByRef stillHasChanges As Boolean) As Boolean
        ''
        Try
            ''
            ''dadapter.ContinueUpdateOnError = True
            Update_Database_EditRow = True ' assume.
            ''
            For i As Integer = 0 To dset.Tables.Count - 1
                Dim dtable As DataTable = dset.Tables(i).GetChanges(DataRowState.Modified)
                '' GetChanges returns Nothing if no row is modified.
                If Not (dtable Is Nothing) AndAlso dtable.Rows.Count > 0 Then
                    ''
                    _Debug.Print_("Updating: " & dtable.TableName)
                    dadapter.Update(dset, dtable.TableName)
                    ''
                End If
            Next i
            ''
            stillHasChanges = dset.HasChanges
            If stillHasChanges Then
                _Debug.Print_("After Editing rows, there are still some changes remain...")
            End If
            ''
        Catch ex As Exception : error_DebugPrint("Update_Database_EditRow", ex.Message) : Update_Database_EditRow = False
        End Try
    End Function
    Public Function Update_Database_InsertRow(ByVal dadapter As OleDbDataAdapter, ByVal dset As DataSet, ByRef stillHasChanges As Boolean) As Boolean
        ''
        Try
            ''
            ''dadapter.ContinueUpdateOnError = True
            Update_Database_InsertRow = True ' assume.
            ''
            For i As Integer = 0 To dset.Tables.Count - 1
                Dim dtable As DataTable = dset.Tables(i).GetChanges(DataRowState.Added)
                '' GetChanges returns Nothing if no row is modified.
                If Not (dtable Is Nothing) AndAlso dtable.Rows.Count > 0 Then
                    '
                    _Debug.Print_("Inserting: " & dtable.TableName)
                    '
                    'Dim sqlcbuilder As New OleDbCommandBuilder(dadapter)
                    'dadapter.InsertCommand = sqlcbuilder.GetInsertCommand
                    '
                    dadapter.Update(dset, dtable.TableName)
                    '
                End If
            Next i
            ''
            stillHasChanges = dset.HasChanges
            If stillHasChanges Then
                _Debug.Print_("After Inserting rows, there are still some changes remain...")
            End If
            ''
        Catch ex As Exception : error_DebugPrint("Update_Database_InsertRow", ex.Message) : Update_Database_InsertRow = False
        End Try
    End Function

End Module
