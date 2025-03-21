
Public Module _Collection

    Private Sub error_DebugPrint(ByVal routineName As String, ByVal errorDesc As String)
        _Debug.PrintError_(String.Format("_Collection.{0}(): {1}", routineName, errorDesc))
    End Sub

#Region "VB6 Collection"

    Private Function Format_colKey(ByVal colKey As String) As String
        On Error GoTo Ooops
        Format_colKey = colKey '' assume.
        If Not 0 = Len(colKey) Then
            If IsNumeric(colKey) Then
                Format_colKey = "key" & colKey
            End If
        End If
Ooops:  If Not 0 = Err.Number Then error_DebugPrint("Collection_.format_colKey(): ", Err.Description)
    End Function

    Public Function AddItem_(ByRef col As Object, ByVal colItem As Object, ByVal colKey As String) As Boolean
        '------------------------------------------------------------'Oleg - Date: January 15, 2008
        Dim okKey As String
        ''
        On Error GoTo Ooops
        ''
        okKey = Format_colKey(colKey)
        col.Add(colItem, okKey)
        ''
Ooops:  AddItem_ = (0 = Err.Number)
        If Not 0 = Err.Number Then error_DebugPrint("Collection_.AddItem_(): ", Err.Description)
        '------------------------------------------------------------
    End Function

    Public Function RemoveItem_ByIndex(ByRef col As Object, ByVal colIndex As Integer) As Boolean
        '------------------------------------------------------------'Oleg - Date: January 15, 2008
        On Error GoTo Ooops
        ''
        If col.Count > 0 Then
            col.Remove(colIndex)
        End If
        ''
Ooops:  RemoveItem_ByIndex = (0 = Err.Number)
        If Not 0 = Err.Number Then error_DebugPrint("Collection_.RemoveItem_ByIndex(): ", Err.Description)
        '------------------------------------------------------------
    End Function

    Public Function FindItem_ByNameProperty(ByRef col As Object, ByVal colName As String, ByRef colObject As Object, ByRef colIndex As Integer) As Boolean
        '------------------------------------------------------------'Oleg - Date: January 15, 2008
        Dim i%
        ''
        On Error GoTo Ooops
        ''
        colObject = Nothing '' assume.
        colIndex = 0            '' assume.
        ''
        For i% = 1 To col.Count
            If colName = col(i%).Name Then
                colObject = col(i%)
                colIndex = i%
            End If
        Next i%
        ''
Ooops:  FindItem_ByNameProperty = (Not colObject Is Nothing)
        If Not 0 = Err.Number Then error_DebugPrint("Collection_.FindItem_ByNameProperty(): ", Err.Description)
        '------------------------------------------------------------
    End Function

    Public Function FindItem_ByKey(ByRef col As Object, ByVal colKey As String, ByRef colObject As Object) As Boolean
        '------------------------------------------------------------'Oleg - Date: January 15, 2008
        Dim i%
        Dim okKey As String
        ''
        On Error GoTo Ooops
        ''
        colObject = Nothing '' assume.
        ''
        If col.Count > 0 Then
            okKey = Format_colKey(colKey)
            colObject = col(okKey)
        End If
        ''
Ooops:  FindItem_ByKey = (Not colObject Is Nothing)
        If Not 0 = Err.Number Then error_DebugPrint("Collection_.FindItem_ByKey(): ", Err.Description)
        '------------------------------------------------------------
    End Function

    Public Function RemoveItem_ByKey(ByRef col As Object, ByVal colKey As String) As Boolean
        '------------------------------------------------------------'Oleg - Date: January 15, 2008
        Dim okKey As String
        ''
        On Error GoTo Ooops
        ''
        okKey = Format_colKey(colKey)
        col.Remove(okKey)
        ''
Ooops:  RemoveItem_ByKey = (0 = Err.Number)
        If Not 0 = Err.Number Then error_DebugPrint("Collection_.RemoveItem_ByKey(): ", Err.Description)
        '------------------------------------------------------------
    End Function

    Public Function Clear_(ByRef col As Object) As Boolean
        '------------------------------------------------------------'Oleg - Date: April 26, 2008
        Dim i%
        ''
        On Error GoTo Ooops
        ''
        For i% = col.Count To 1 Step -1
            Call col.Remove(i%)
        Next i%
        ''
Ooops:  Clear_ = (0 = col.Count)
        If Not 0 = Err.Number Then error_DebugPrint("Collection_.Clear_(): ", Err.Description)
        '------------------------------------------------------------
    End Function

    Public Function IsItemExist(ByRef col As Object, ByVal colKey As String) As Boolean
        '------------------------------------------------------------'Oleg - Date: January 15, 2008
        Dim okKey As String
        Dim tmpItem As String
        ''
        On Error GoTo Ooops
        ''
        okKey = Format_colKey(colKey)
        tmpItem = col(okKey)
        ''
Ooops:  IsItemExist = (0 = Err.Number)
        If Not 0 = Err.Number Then error_DebugPrint("Collection_.IsItemExist(): ", Err.Description)
        '------------------------------------------------------------
    End Function

    Public Function AddItem_String(ByRef col As Object, ByVal colItem As String, ByVal colKey As String) As Boolean
        Dim okKey As String
        ''
        On Error GoTo Ooops ''ol#9.62(11/1)... Collection routine was added to handle String values.
        ''
        okKey = Format_colKey(colKey)
        col.Add(colItem, okKey)
        ''
Ooops:  AddItem_String = (0 = Err.Number) : If Not 0 = Err.Number Then error_DebugPrint("Collection_.AddItem_String(): ", Err.Description)
        '------------------------------------------------------------
    End Function
    Public Function FindItemString_ByKey(ByRef col As Object, ByVal colKey As String, ByRef colItem As String) As Boolean
        Dim i%
        Dim okKey As String
        ''
        On Error GoTo Ooops ''ol#9.62(11/1)... Collection routine was added to handle String values.
        ''
        colItem = "" '' assume.
        If col.Count > 0 Then
            okKey = Format_colKey(colKey)
            colItem = col(okKey)
        End If
        ''
Ooops:  FindItemString_ByKey = (Not 0 = Len(colItem)) : If Not 0 = Err.Number Then error_DebugPrint("Collection_.FindItemString_ByKey(): ", Err.Description)
    End Function

#End Region

End Module
