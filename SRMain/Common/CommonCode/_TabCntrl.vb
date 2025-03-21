Imports System.Windows.Forms

Public Module _TabCntrl

    Public Function Add_Page(ByRef tab As TabControl, ByVal pageName As String, ByVal pageKey As String, ByVal pageIndex As Integer) As Boolean
        ''
        Try
            Add_Page = True '' assume.
            Dim pageCount As Integer = tab.TabCount
            ''
            If Not _TabCntrl.IsExist_PageName(tab, pageName, pageKey, pageIndex) Then
                ''
                If Not pageIndex < tab.TabCount Then
                    tab.TabPages.Add(pageKey, pageName)
                ElseIf pageIndex > 0 Then
                    tab.TabPages.Insert(pageIndex - 1, pageKey, pageName) '' index is zero based
                End If
                ''
            End If
            ''
        Catch ex As Exception : _Debug.PrintError_("_TabCntrl.Add_Page(): " & ex.Message) : Add_Page = False
        End Try
        ''
    End Function

    Public Function IsExist_PageName(ByVal tab As TabControl, ByVal pageName As String) As Boolean
        ''
        IsExist_PageName = False
        Try
            For Each page As TabPage In tab.TabPages
                ''
                IsExist_PageName = (pageName = page.Text)
                If IsExist_PageName Then
                    Exit For
                End If
                ''
            Next page
            ''
        Catch ex As Exception : _Debug.PrintError_("_TabCntrl.IsExist_PageName(2): " & ex.Message)
        End Try
        ''
    End Function
    Public Function IsExist_PageName(ByVal tab As TabControl, ByVal pageName As String, ByVal pageKey As String) As Boolean
        ''
        IsExist_PageName = False
        Try
            For Each page As TabPage In tab.TabPages
                ''
                IsExist_PageName = (pageName = page.Text) And (pageKey = page.Name)
                If IsExist_PageName Then
                    Exit For
                End If
                ''
            Next page
            ''
        Catch ex As Exception : _Debug.PrintError_("_TabCntrl.IsExist_PageName(3): " & ex.Message)
        End Try
        ''
    End Function
    Public Function IsExist_PageName(ByVal tab As TabControl, ByVal pageName As String, ByVal pageKey As String, ByVal pageIndex As Integer) As Boolean
        ''
        IsExist_PageName = False
        Try
            If Not pageIndex > tab.TabPages.Count - 1 Then
                IsExist_PageName = (pageName = tab.TabPages(pageIndex).Text) And (pageKey = tab.TabPages(pageIndex).Name)
            End If
            ''
        Catch ex As Exception : _Debug.PrintError_("IsExist_PageName(4): " & ex.Message)
        End Try
        ''
    End Function

    Public Function Remove_RangeOfPages(ByRef tab As TabControl, ByVal startIndex As Integer, ByVal endIndex As Integer) As Boolean
        ''
        Remove_RangeOfPages = False
        Try
            Dim tabsCount As Integer = tab.TabPages.Count
            ''
            For i As Integer = endIndex To startIndex Step -1
                Dim page As TabPage = tab.TabPages(i)
                tab.TabPages.Remove(page)
            Next i
            ''
            Remove_RangeOfPages = (tab.TabPages.Count = (tabsCount - (endIndex + 1 - startIndex)))
            ''
        Catch ex As Exception : _Debug.PrintError_("Remove_RangeOfPages(): " & ex.Message)
        End Try
        ''
    End Function
    Public Function Remove_AllPages(ByRef tab As TabControl) As Boolean
        ''
        Remove_AllPages = False
        Try
            tab.TabPages.Clear()
            Remove_AllPages = (0 = tab.TabCount)
            ''
        Catch ex As Exception : _Debug.PrintError_("_TabCntrl.Remove_AllPages(): " & ex.Message)
        End Try
        ''
    End Function

End Module
