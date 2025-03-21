Imports System.Windows.Forms

Public Module _TreeView

    Public Function Add_Root(ByVal tv As System.Windows.Forms.TreeView, ByVal tvText As String, ByRef xRoot As System.Windows.Forms.TreeNode) As Boolean
        Add_Root = False
        Try
            If Not 0 = tvText.Length Then
                xRoot = tv.Nodes.Add(tvText, tvText)
                Add_Root = (Not xRoot Is Nothing)
            End If
            ''
        Catch ex As Exception : _Debug.PrintError_("Add_Root(): " & ex.Message)
        End Try
        ''
    End Function

    Public Function Add_NodeChild(ByVal xNode As System.Windows.Forms.TreeNode, ByVal tvText As String, ByRef xChild As System.Windows.Forms.TreeNode) As Boolean
        Add_NodeChild = False
        Try
            If Not 0 = tvText.Length Then
                _Debug.Print_(xNode.FullPath & "\" & tvText)
                xChild = xNode.Nodes.Add(xNode.FullPath & "\" & tvText, tvText)
                Add_NodeChild = (Not xChild Is Nothing)
            End If
            ''
        Catch ex As Exception : _Debug.PrintError_("Add_NodeChild(): " & ex.Message)
        End Try
        ''
    End Function

    Public Function Add_Node(ByVal xRoot As System.Windows.Forms.TreeNode, ByVal tvText As String, ByRef xNode As System.Windows.Forms.TreeNode) As Boolean
        Add_Node = False
        Try
            If Not 0 = tvText.Length Then
                'If "DHL" = tvText Then Stop
                If Not IsNodeExist(xRoot, xRoot.FullPath & "\" & tvText, xNode) Then
                    xNode = xRoot.Nodes.Add(xRoot.FullPath & "\" & tvText, tvText)
                End If
            End If
            ''
            Add_Node = (Not xNode Is Nothing)
            ''
        Catch ex As Exception : _Debug.PrintError_("Add_Node(): " & ex.Message)
        End Try
        ''
    End Function

    ''Public Function IsNodeExist(ByVal xParent As Windows.Forms.TreeNode, ByVal xKey As String, ByRef xNodes() As Windows.Forms.TreeNode) As Boolean
    Public Function IsNodeExist(ByVal xParent As System.Windows.Forms.TreeNode, ByVal xKey As String, ByRef xNode As System.Windows.Forms.TreeNode) As Boolean
        IsNodeExist = False
        Try
            Dim xNodes() As System.Windows.Forms.TreeNode = xParent.Nodes.Find(xKey, True) '' array of nodes after Find()
            IsNodeExist = (Not 0 = xNodes.Length)
            If IsNodeExist Then
                xNode = xNodes(0) '' first found node
            End If
            ''
        Catch ex As Exception : _Debug.PrintError_("IsNodeExist(): " & ex.Message)
        End Try
        ''
    End Function

    Public Function IsNodeSelected(ByVal tv As TreeView) As Boolean
        IsNodeSelected = False
        Try
            If tv.Visible Then
                If 0 < tv.Nodes.Count Then
                    IsNodeSelected = tv.SelectedNode.IsSelected
                End If
            End If
            ''
        Catch ex As Exception : _Debug.PrintError_("IsNodeSelected(): " & ex.Message)
        End Try
        ''
    End Function

    Public Function Remove_AllNodes(ByRef tv As System.Windows.Forms.TreeView) As Boolean
        Remove_AllNodes = False
        Try
            tv.Nodes.Clear()
            Remove_AllNodes = (0 = tv.Nodes.Count)
            ''
        Catch ex As Exception : _Debug.PrintError_("Remove_AllNodes(): " & ex.Message)
        End Try
        ''
    End Function

End Module
