Imports System.Data

Public Module _ComboBox

    Private Sub error_DebugPrint(ByVal routineName As String, ByVal errorDesc As String)
        _Debug.PrintError_(String.Format("_Controls.{0}(): {1}", routineName, errorDesc))
    End Sub

    Public Function IsExist_Item(ByVal comboText As String, ByVal combo As ComboBox, ByRef comboIndex As Integer) As Boolean
        IsExist_Item = False
        Try
            comboIndex = -1 ' assume.
            For i% = 0 To combo.Items.Count - 1
                If comboText = combo.Items(i%).ToString Then
                    comboIndex = i%
                    Exit For
                End If
            Next i%
            IsExist_Item = (Not comboIndex = -1)
        Catch ex As Exception : error_DebugPrint("IsExist_Item", ex.Message)
        End Try
    End Function
    Public Function IsExist_Item(ByVal itemID As Long, ByVal combo As ComboBox, ByRef item As ListItemWithID) As Boolean
        IsExist_Item = False
        Try
            For i% = 0 To combo.Items.Count - 1
                item = combo.Items(i%)
                IsExist_Item = (itemID = item.ItemID)
                If IsExist_Item Then
                    Exit For
                End If
            Next i%
        Catch ex As Exception : error_DebugPrint("IsExist_Item", ex.Message)
        End Try
    End Function
    Public Function IsExist_Item(ByVal itemName As String, ByVal itemID As Long, ByVal combo As ComboBox, ByRef drow As DataRow, Optional listIndex As Integer = -1) As Boolean
        IsExist_Item = False
        Try
            For i% = 0 To combo.Items.Count - 1
                drow = combo.Items(i%)
                IsExist_Item = (itemID = drow(itemName))
                If IsExist_Item Then
                    listIndex = i%
                    Exit For
                End If
            Next i%
        Catch ex As Exception : error_DebugPrint("IsExist_Item", ex.Message)
        End Try
    End Function

    Public Function AddValues(ByVal combo As ComboBox, ByVal dreader As OleDb.OleDbDataReader, Optional ByVal is2clear As Boolean = True) As Boolean
        AddValues = False
        Try
            If is2clear Then
                Call _ComboBox.Clear(combo)
            End If
            Do While dreader.Read
                Dim lst As ListItemWithID = New ListItemWithID()
                lst.ItemID = _Convert.Null2DefaultValue(dreader(0))     ' database ID
                lst.ItemText = _Convert.Null2DefaultValue(dreader(1))   ' database Text
                lst.ItemIndex = combo.Items.Count ' optional to know exact list index
                combo.Items.Add(lst)
            Loop
            AddValues = (combo.Items.Count > 0)
        Catch ex As Exception : error_DebugPrint("AddValues", ex.Message)
        End Try
    End Function
    Public Function AddGeneric_Text(ByVal combo As ComboBox, ByVal itemText As String, ByVal itemID As Long) As Boolean
        AddGeneric_Text = False
        Try
            Dim lst As ListItemWithID = New ListItemWithID()
            lst.ItemID = itemID     ' database ID
            lst.ItemText = itemText   ' database Text
            lst.ItemIndex = combo.Items.Count ' optional to know exact list index
            combo.Items.Add(lst)
            AddGeneric_Text = (combo.Items.Count > 0)
        Catch ex As Exception : error_DebugPrint("AddGeneric_Text", ex.Message)
        End Try
    End Function

    Public Function Remove_Item(ByVal itemID As Long, ByVal combo As ComboBox) As Boolean
        Try
            Dim item As New ListItemWithID
            Remove_Item = True ' assume.
            If _ComboBox.IsExist_Item(itemID, combo, item) Then
                combo.Items.Remove(item)
                combo.Text = String.Empty
            End If
        Catch ex As Exception : error_DebugPrint("Remove_Item", ex.Message) : Remove_Item = False
        End Try
    End Function

    Public Function GetSelected_ItemID(ByVal combo As ComboBox, ByRef itemID As Long) As Boolean
        GetSelected_ItemID = False
        Try
            itemID = 0 ' assume.
            If Not combo.SelectedItem Is Nothing Then
                Dim lstItem As ListItemWithID = combo.SelectedItem
                itemID = lstItem.ItemID
            End If
            GetSelected_ItemID = (Not 0 = itemID)
        Catch ex As Exception : error_DebugPrint("GetSelected_ItemID", ex.Message)
        End Try
    End Function
    Public Function GetSelected_ItemID(ByVal itemName As String, ByVal combo As ComboBox, ByRef itemID As Long) As Boolean
        GetSelected_ItemID = False
        Try
            itemID = 0 ' assume.
            If Not combo.SelectedItem Is Nothing Then
                Dim drow As DataRow = combo.SelectedItem
                itemID = drow(itemName)
            End If
            GetSelected_ItemID = (Not 0 = itemID)
        Catch ex As Exception : error_DebugPrint("GetSelected_ItemID", ex.Message)
        End Try
    End Function
    Public Function GetSelected_DataRow(ByVal combo As ComboBox, ByRef drow As DataRow) As Boolean
        GetSelected_DataRow = False
        Try
            drow = Nothing 'assume.
            If Not combo.SelectedItem Is Nothing Then
                drow = combo.SelectedItem
            End If
            GetSelected_DataRow = (Not drow Is Nothing)
        Catch ex As Exception : error_DebugPrint("GetSelected_DataRow", ex.Message)
        End Try
    End Function

    Public Sub Clear(ByVal combo As ComboBox)
        Try
            combo.Text = String.Empty
            combo.Items.Clear()
        Catch ex As Exception : error_DebugPrint("Clear", ex.Message)
        End Try
    End Sub

#Region "Combo With ItemObject"
    Public Function AddOne_ItemObject(ByVal combo As ComboBox, ByVal fieldName4ItemText As String, ByVal drow As DataRow) As Boolean
        AddOne_ItemObject = False
        Try
            If Not IsNothing(drow) Then
                Dim lstCmb As New _ListItemWithObject
                lstCmb.ItemText = _Convert.Null2DefaultValue(drow(fieldName4ItemText))
                lstCmb.ItemObject = drow
                combo.Items.Add(lstCmb)
                AddOne_ItemObject = True
            End If
        Catch ex As Exception : error_DebugPrint("AddOne_ItemObject", ex.Message)
        End Try
    End Function
    Public Function AddAll_ItemObject(ByVal combo As ComboBox, ByVal fieldName4ItemText As String, ByVal drows() As DataRow, Optional ByVal is2clear As Boolean = True) As Boolean
        AddAll_ItemObject = False
        Try
            If is2clear Then
                Call _ComboBox.Clear(combo)
            End If
            For i As Integer = 0 To drows.Length - 1
                Dim lstCmb As New _ListItemWithObject
                lstCmb.ItemText = _Convert.Null2DefaultValue(drows(i)(fieldName4ItemText))
                lstCmb.ItemObject = drows(i)
                combo.Items.Add(lstCmb)
            Next
            AddAll_ItemObject = True
        Catch ex As Exception : error_DebugPrint("AddAll_ItemObject", ex.Message)
        End Try
    End Function
    Public Function IsExist_ItemObject(ByVal combo As ComboBox, ByVal itemIndex As Integer, ByRef drow As DataRow) As Boolean
        IsExist_ItemObject = False
        Try
            drow = Nothing ' assume.
            If combo.Items.Count > itemIndex Then
                Dim lstCmb As _ListItemWithObject = combo.Items(itemIndex)
                drow = lstCmb.ItemObject
                lstCmb = Nothing
            End If
            IsExist_ItemObject = Not IsNothing(drow)
        Catch ex As Exception : error_DebugPrint("IsExist_ItemObject", ex.Message)
        End Try
    End Function
    Public Function IsExist_ItemObject(ByVal combo As ComboBox, ByVal itemText As String, ByRef drow As DataRow) As Boolean
        IsExist_ItemObject = False
        Try
            drow = Nothing ' assume.
            For i As Integer = 0 To combo.Items.Count - 1
                Dim lstCmb As _ListItemWithObject = combo.Items(i)
                If itemText = lstCmb.ItemText Then
                    drow = lstCmb.ItemObject
                    Exit For
                End If
                lstCmb = Nothing
            Next
            IsExist_ItemObject = Not IsNothing(drow)
        Catch ex As Exception : error_DebugPrint("IsExist_ItemObject", ex.Message)
        End Try
    End Function
    Public Function IsExist_ItemObject(ByVal combo As ComboBox, ByVal itemID As Long, ByRef drow As DataRow) As Boolean
        IsExist_ItemObject = False
        Try
            drow = Nothing ' assume.
            For i As Integer = 0 To combo.Items.Count - 1
                Dim lstCmb As _ListItemWithObject = combo.Items(i)
                If itemID = lstCmb.ItemID Then
                    drow = lstCmb.ItemObject
                    Exit For
                End If
                lstCmb = Nothing
            Next
            IsExist_ItemObject = Not IsNothing(drow)
        Catch ex As Exception : error_DebugPrint("IsExist_ItemObject", ex.Message)
        End Try
    End Function
    Public Function IsExist_ItemObject(ByVal combo As ComboBox, ByVal itemID As Long, ByRef itemIndex As Integer) As Boolean
        IsExist_ItemObject = False
        Try
            itemIndex = -1 ' assume.
            For i As Integer = 0 To combo.Items.Count - 1
                Dim lstCmb As _ListItemWithObject = combo.Items(i)
                If itemID = lstCmb.ItemID Then
                    ''ol#1.1.99(5/11)...  Contents combo index was not read correctly.
                    itemIndex = i 'lstCmb.ItemIndex
                    Exit For
                End If
                lstCmb = Nothing
            Next
            IsExist_ItemObject = Not (-1 = itemIndex)
        Catch ex As Exception : error_DebugPrint("IsExist_ItemObject", ex.Message)
        End Try
    End Function
#End Region
End Module
