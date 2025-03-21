Imports System.IO
Imports System.Windows.Forms


Public Module _Debug

    ' Catch ex As Exception : Throw New ArgumentException(errorDesc)

#If INHOUSE = 0 Then
    Public Const IsINHOUSE As Boolean = False
#Else
    Public Const IsINHOUSE As Boolean = True
#End If


    Public Sub Print_(ByVal ParamArray args() As String)
        ''
        Dim argLine As String = String.Empty
        For i As Integer = args.GetLowerBound(0) To args.GetUpperBound(0)
            argLine = argLine & args(i) & vbTab
        Next i
        ''
        Debug.Print(argLine.Trim)
        ''Call Debug_.Stop_    
        ''
    End Sub

    Public Sub PrintError_(ByVal ParamArray args() As String)
        ''
        Dim argLine As String = String.Empty
        For i As Integer = args.GetLowerBound(0) To args.GetUpperBound(0)
            argLine = argLine & args(i) & vbTab
        Next i
        ''
        Dim errline As String = String.Format("[{0}] {1}", System.DateTime.Now, argLine.Trim)
        _Debug.Print_(errline)
        _Debug.Print2File(argLine.Trim, String.Empty)
        ''Call Debug_.Stop_    
        ''
    End Sub
    Public Sub PrintActivity_(ByVal activity2Print As String, ByVal fileName As String)
        ''
        _Debug.Print_("[" & System.DateTime.Now.ToString & "] " & activity2Print)
        ''If debug_Print2File Then
        _Debug.Print2File(activity2Print, fileName)
        ''End If
        ''Call Debug_.Stop_    
        ''
    End Sub
    Public Sub Print2File(ByVal debugEx As Exception, ByVal fileName As String)
        ''
        Try
            Dim debug2Print As String
            debug2Print = debugEx.Message & ControlChars.NewLine & debugEx.StackTrace
            Print2File(debug2Print, fileName)
            ''
        Catch ex As Exception : Debug.Print(String.Format("_Debug.Print2File({0}): {1}", fileName, ex.Message))
        End Try
        ''
    End Sub
    Public Sub Print2File(ByVal debug2Print As String, ByVal fileName As String)
        ''
        Try
            If 0 = fileName.Length Then
                Debug.Print(System.Windows.Forms.Application.StartupPath)
                fileName = String.Format("{0}\ErrStack.log", System.Windows.Forms.Application.StartupPath)
            End If
            ''
            Debug.Print(System.Windows.Forms.Application.ProductVersion)
            'MsgBox(Application.StartupPath)
            ''
            Using swriter As New StreamWriter(fileName, True)
                ''ol#1.2.24(11/17)... Net error log will have a program version now.
                swriter.Write("[{0}] [{3}] {2}{1}{2}{2}", System.DateTime.Now, debug2Print, ControlChars.NewLine, System.Windows.Forms.Application.ProductVersion)
                swriter.Close()
            End Using
            ''
        Catch ex As Exception : Debug.Print(String.Format("_Debug.Print2File({0}): {1}", fileName, ex.Message))
        End Try
        ''
    End Sub

    Public Sub Stop_(Optional comment As String = "")
        If IsINHOUSE Then
            If comment.Length > 0 Then
                _Debug.Print_(comment)
            End If
            Stop
        End If
    End Sub

    Public Sub ReportError(ByVal ex As Exception)
        Dim Success As Boolean
        Dim report_template As EmailTemplate
        report_template = Create_ErrorNotification_Template(ex)

        Debug.Print(ex.Message & ex.StackTrace)

        _Debug.Print2File(ex, String.Empty)

        If GetPolicyData(gShipriteDB, "Notify_Email") <> "" And GetPolicyData(gShipriteDB, "Notify_Password") <> "" And GetPolicyData(gShipriteDB, "Notify_SmtpServer") <> "" Then
            If vbYes = MsgBox("Would you like to report this error to ShipRite?" & vbCrLf & vbCrLf & "Click YES to email the error log and help us improve our software.", vbYesNo + vbQuestion, "Report Error") Then
                Success = sendEmail("developers@shipritesoftware.com", report_template, False)

                If Success = True Then
                    MsgBox("Error report sent successfully!")
                End If
            End If
        End If

        'If String.IsNullOrEmpty(_Email.EmailFrom) Then
        '_Email.EmailFrom = "dogsoft@mvcc.edu"
        'End If
        '_Email.Send_Error(ex)
    End Sub

    Public Function Create_ErrorNotification_Template(ex As Exception) As EmailTemplate
        Dim template As EmailTemplate = New EmailTemplate

        template.Subject &= "Error Report from " & _StoreOwner.StoreOwner.CompanyName

        template.Content = String.Format("Version: {1}{0}{0}", Environment.NewLine, My.Application.Info.Version.ToString)
        template.Content &= String.Format("The following error has occurred: {0}{1}{0}{2}{0}{0}{0}", Environment.NewLine, ex.Message, ex.StackTrace)
        template.Content &= String.Format("{1}{0}{2}{0}{3}{0}{4}{0}", Environment.NewLine, _StoreOwner.StoreOwner.CompanyName, _StoreOwner.StoreOwner.FNameLName, _StoreOwner.StoreOwner.Address, _StoreOwner.StoreOwner.Tel)


        Return template
    End Function
    Public Sub ReportError(ByVal ex As Exception, ByVal subject As String)
        _Debug.Print_(ex.ToString)
        _Debug.Print2File(subject & System.Environment.NewLine & ex.ToString, String.Empty)
        If String.IsNullOrEmpty(_Email.EmailFrom) Then
            _Email.EmailFrom = "dogsoft@mvcc.edu"
        End If
        _Email.Send_Error(ex, subject)
    End Sub
    Public Sub ReportError(ByVal ex As Exception, ByVal subject As String, ByVal sql2exe As String)
        _Debug.Print2File(subject & System.Environment.NewLine & "[sql2exe]: " & sql2exe & System.Environment.NewLine & ex.ToString, String.Empty)
        If String.IsNullOrEmpty(_Email.EmailFrom) Then
            _Email.EmailFrom = "dogsoft@mvcc.edu"
        End If
        _Email.Send_Error(ex, subject, sql2exe)
    End Sub

End Module

