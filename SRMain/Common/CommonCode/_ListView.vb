
Public Module _ListView

    Private Sub error_DebugPrint(ByVal routineName As String, ByVal errorDesc As String)
        _Debug.PrintError_(String.Format("_ListView.{0}(): {1}", routineName, errorDesc))
    End Sub

    Public Function Add_lvItem(ByVal lvItem As ListViewItem, ByVal lvControl As ListView) As Boolean
        Add_lvItem = False
        Try
            lvControl.Items.Add(lvItem)
            Add_lvItem = True
        Catch ex As Exception : error_DebugPrint("Add_lvItem", ex.Message)
        End Try
    End Function
    Public Function Insert_lvItem(ByVal lvItem As ListViewItem, ByVal lvControl As ListView, ByVal lvControlIndex As Integer) As Boolean
        Insert_lvItem = False
        Try
            If -1 < lvControlIndex And lvControlIndex <= lvControl.Items.Count Then
                lvControl.Items.Insert(lvControlIndex, lvItem)
                Insert_lvItem = True
            End If
        Catch ex As Exception : error_DebugPrint("Insert_lvItem", ex.Message)
        End Try
    End Function
    Public Function Remove_lvItem(ByVal lvControl As ListView, ByVal lvItemIndex As Integer) As Boolean
        Remove_lvItem = False
        Try
            If lvItemIndex < lvControl.Items.Count Then
                lvControl.Items(lvItemIndex).Remove()
                Remove_lvItem = True
            End If
        Catch ex As Exception : error_DebugPrint("Remove_lvItem", ex.Message)
        End Try
    End Function
    Public Function Remove_SelectedItem(ByVal lvControl As ListView) As Boolean
        Remove_SelectedItem = False
        Try
            If lvControl.Items.Count > 0 Then
                If lvControl.SelectedItems.Count > 0 Then
                    lvControl.SelectedItems(0).Remove()
                    Remove_SelectedItem = True
                End If
            End If
        Catch ex As Exception : error_DebugPrint("Remove_SelectedItem", ex.Message)
        End Try
    End Function
    Public Function Remove_AllItems(ByVal lvControl As ListView) As Boolean
        Remove_AllItems = False
        Try
            lvControl.Items.Clear()
            Remove_AllItems = (0 = lvControl.Items.Count)
        Catch ex As Exception : error_DebugPrint("Remove_AllItems", ex.Message)
        End Try
    End Function
    Public Function Clear(ByVal lvControl As ListView) As Boolean
        Clear = False
        Try
            lvControl.Items.Clear()
            Clear = (0 = lvControl.Items.Count)
        Catch ex As Exception : error_DebugPrint("Clear", ex.Message)
        End Try
    End Function

    Public Function IsExist_Item(ByVal lview As ListView, ByVal itemName As String, ByRef itemIndex As Integer) As Boolean
        IsExist_Item = False
        Try
            itemIndex = -1 ' assume.
            For i As Integer = 0 To lview.Items.Count - 1
                If itemName = lview.Items(i).Text Then
                    itemIndex = i
                End If
            Next
            IsExist_Item = (Not itemIndex = -1)
        Catch ex As Exception : error_DebugPrint("IsExist_Item", ex.Message)
        End Try
    End Function
End Module
