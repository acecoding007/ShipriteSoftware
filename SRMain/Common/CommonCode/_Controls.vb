
Public Module _Controls

    Private Sub error_DebugPrint(ByVal routineName As String, ByVal errorDesc As String)
        _Debug.PrintError_(String.Format("_Controls.{0}(): {1}", routineName, errorDesc))
    End Sub

    Public Function Mid(ByVal str As String, ByVal startIndex As Integer) As String
        ''
        Mid = str '' assume.
        Try
            If Not startIndex > str.Length Then
                Mid = str.Substring(startIndex)
            End If
            ''
        Catch ex As Exception : error_DebugPrint("Mid 4", ex.Message)
        End Try
        ''
    End Function
    Public Function Mid(ByVal str As String, ByVal startIndex As Integer, ByVal length As Integer) As String
        ''
        Mid = str '' assume.
        Try
            If Not startIndex > str.Length AndAlso Not (startIndex + length) > str.Length Then
                Mid = str.Substring(startIndex, length)
            End If
            ''
        Catch ex As Exception : error_DebugPrint("Mid 5", ex.Message)
        End Try
        ''
    End Function
    Public Function Mid(ByVal str As String, ByVal startIndex As Integer, ByVal isOneBased As Boolean) As String
        ''
        Mid = str '' assume.
        Try
            If isOneBased Then
                startIndex = startIndex - 1
            End If
            If Not startIndex > str.Length Then
                Mid = str.Substring(startIndex)
            End If
            ''
        Catch ex As Exception : error_DebugPrint("Mid 2", ex.Message)
        End Try
        ''
    End Function
    Public Function Mid(ByVal str As String, ByVal startIndex As Integer, ByVal length As Integer, ByVal isOneBased As Boolean) As String
        ''
        Mid = str '' assume.
        Try
            If isOneBased Then
                startIndex = startIndex - 1
            End If
            If Not startIndex > str.Length AndAlso Not (startIndex + length) > str.Length Then
                Mid = str.Substring(startIndex, length)
            End If
            ''
        Catch ex As Exception : error_DebugPrint("Mid 3", ex.Message)
        End Try
        ''
    End Function

    Public Function Left(ByVal str As String, ByVal length As Integer) As String
        ''
        Left = str '' assume.
        Try
            If Not length > str.Length Then
                Left = str.Substring(0, length)
            End If
            ''
        Catch ex As Exception : error_DebugPrint("Left", ex.Message)
        End Try
        ''
    End Function
    Public Function Right(ByVal str As String, ByVal length As Integer) As String
        ''
        Right = str '' assume.
        Try
            If Not length > str.Length Then
                Right = str.Substring(str.Length - length, length)
            End If
            ''
        Catch ex As Exception : error_DebugPrint("Right", ex.Message)
        End Try
        ''
    End Function
    Public Function TrimAChar(ByVal str As String, ByVal char2trim As String) As String
        ''
        TrimAChar = str '' assume.
        Try
            '_Debug.Print_(_Controls.Left(str.Trim, 1))
            '_Debug.Print_(String.Compare(char2trim, _Controls.Left(str.Trim, 1)).ToString)
            If 0 = String.Compare(char2trim, _Controls.Left(str.Trim, 1)) Then '' 0 = Match Found
                'str = _Controls.Mid(str.Trim, 1)
            End If
            '_Debug.Print_(_Controls.Right(str.Trim, 1))
            '_Debug.Print_(String.Compare(char2trim, _Controls.Right(str.Trim, 1)).ToString)
            If 0 = String.Compare(char2trim, _Controls.Right(str.Trim, 1)) Then '' 0 = Match Found
                str = _Controls.Left(str.Trim, str.Trim.Length - 1)
            End If
            ''
            TrimAChar = str.Trim
            ''
        Catch ex As Exception : error_DebugPrint("TrimAChar", ex.Message)
        End Try
        ''
    End Function
    Public Function Replace(ByVal str As String, ByVal char2replace As String, ByVal replace2char As String) As String
        ''
        Replace = str '' assume.
        If str IsNot Nothing Then
            Try
                Replace = str.Replace(char2replace, replace2char)
                ''
            Catch ex As Exception : error_DebugPrint("Replace", ex.Message)
            End Try
        End If
        ''
    End Function

    Public Function Contains(ByVal str2check As String, ByVal str2find As String, Optional ByVal caseSensitive As Boolean = False) As Boolean
        Contains = False
        Try
            If str2check IsNot Nothing Then
                If Not String.IsNullOrEmpty(str2check) Then
                    If caseSensitive Then
                        ' case sensitive.
                        Contains = str2check.Contains(str2find)
                    Else
                        ' perform check only after converting strings to lower case.
                        Contains = str2check.ToLower.Contains(str2find.ToLower)
                    End If
                End If
            End If
        Catch ex As Exception : error_DebugPrint("Contains", ex.Message)
        End Try
        ''
    End Function

    Public Sub SetFoucus(ByRef control As System.Windows.Forms.Control)
        Try
            control.Select()
        Catch ex As Exception : error_DebugPrint("SetFoucus", ex.Message)
        End Try
    End Sub
    Public Sub SetFoucus2TextBox(ByRef txt As TextBox)
        Try
            txt.Focus()
            txt.SelectionStart = txt.Text.Length
        Catch ex As Exception : error_DebugPrint("SetFoucus2TextBox", ex.Message)
        End Try
    End Sub
    Public Sub ToProperCase(ByRef txtBox As TextBox)
        Try
            Dim words As New List(Of String)
            words.AddRange(txtBox.Text.Split(" "))
            If words.Count > 0 Then
                If txtBox.SelectionStart = txtBox.Text.Length And 0 = txtBox.SelectionLength Then
                    If 0 < words.Count Then
                        Dim word As String = words.Item(words.Count - 1)
                        If 1 = word.Length Then
                            txtBox.Text = txtBox.Text.Remove(txtBox.Text.Length - 1, 1) & word.ToUpper
                            txtBox.SelectionStart = txtBox.Text.Length
                        End If
                    End If
                End If
            End If
        Catch ex As Exception : error_DebugPrint("ToProperCase", ex.Message)
        End Try
    End Sub

    Public Sub GroupBox_Clear_Buttons(ByVal grb As System.Windows.Forms.GroupBox)
        Try
            For Each cntrl As System.Windows.Forms.Control In grb.Controls
                If TypeOf cntrl Is System.Windows.Forms.Button Then
                    cntrl.Text = String.Empty
                End If
            Next cntrl
        Catch ex As Exception : error_DebugPrint("GroupBox_Clear_Buttons", ex.Message)
        End Try
    End Sub
    Public Sub Form_Clear_TextBoxes(ByVal frm As System.Windows.Controls.Grid)
        Try
            ' Recursively clear all TextBox and MaskedTextBox controls
            ClearTextBoxes(frm)
        Catch ex As Exception
            error_DebugPrint("Form_Clear_TextBoxes", ex.Message)
        End Try
    End Sub

    Private Sub ClearTextBoxes(ByVal parent As System.Windows.Controls.Panel)
        For Each cntrl As UIElement In parent.Children
            If TypeOf cntrl Is TextBox Then
                CType(cntrl, TextBox).Clear() ' Clears the TextBox

            ElseIf TypeOf cntrl Is Panel Then
                ClearTextBoxes(CType(cntrl, Panel)) ' Recursively call for child controls
            End If
        Next
    End Sub



    Public Function IsEqual(ByVal str1 As String, ByVal str2 As String) As Boolean
        IsEqual = False
        Try
            IsEqual = (0 = String.Compare(str1, str2)) '' 0 = Match 
        Catch ex As Exception : error_DebugPrint("1 IsEqual", ex.Message)
        End Try
    End Function
    Public Function IsOddNumber(ByVal n As Long) As Boolean
        IsOddNumber = False
        Try
            IsOddNumber = (n And 1) '(If Number And 1 Then) is always odd
        Catch ex As Exception : error_DebugPrint("IsOddNumber", ex.Message)
        End Try
    End Function

    Public Property vbCr_() As String
        Get
            '' vb6: ControlChars.NewLine
            Return System.Environment.NewLine
        End Get
        Set(ByVal value As String)
        End Set
    End Property

    Public Function User_GetName() As String
        User_GetName = String.Empty ' assume.
        Try
            User_GetName = Environment.UserName
        Catch ex As Exception : error_DebugPrint("User_GetName", ex.Message)
        End Try
    End Function
    Public Function Computer_GetName() As String
        Computer_GetName = String.Empty ' assume.
        Try
            Computer_GetName = Environment.GetEnvironmentVariable("COMPUTERNAME")
        Catch ex As Exception : error_DebugPrint("Computer_GetName", ex.Message)
        End Try
    End Function
    Public Function App_CountRunningInstance() As Integer
        App_CountRunningInstance = 0
        Try
            App_CountRunningInstance = UBound(Diagnostics.Process.GetProcessesByName(Diagnostics.Process.GetCurrentProcess.ProcessName))
        Catch ex As Exception : error_DebugPrint("App_CountRunningInstance", ex.Message)
        End Try
    End Function
    Public Function App_IsNoInstancesAlreadyRunning() As Boolean
        App_IsNoInstancesAlreadyRunning = False
        Try
            App_IsNoInstancesAlreadyRunning = (0 = _Controls.App_CountRunningInstance)
        Catch ex As Exception : error_DebugPrint("App_IsNoInstancesAlreadyRunning", ex.Message)
        End Try
    End Function
    Public Function App_Path() As String
        App_Path = String.Empty
        Try
            Return System.AppDomain.CurrentDomain.BaseDirectory()
        Catch ex As Exception : error_DebugPrint("App_Path", ex.Message)
        End Try
    End Function

    Public Function Extract_TableName_FromSQLStatement(ByVal sqlstatement As String) As String
        Extract_TableName_FromSQLStatement = String.Empty
        Try
            Dim splitsql() As String = sqlstatement.Trim.Split(" ")
            For i As Integer = 0 To splitsql.Length - 1
                If "FROM" = splitsql(i).ToUpper Then
                    Return splitsql(i + 1)
                End If
            Next i
        Catch ex As Exception : error_DebugPrint("Extract_TableName_FromSQLStatement", ex.Message)
        End Try
    End Function

End Module
