Imports System.Windows

Public Module _MsgBox

    Public Sub ErrorMessage(ByVal errorDesc As String, Optional ByVal addmsg As String = "", Optional ByVal msgboxTitle As String = "")
        ''
        Dim msg As String
        Dim msgTmp As String
        ''
        '' regular error message display to warn users
        msgTmp = "Process has failed !"
        If 0 = addmsg.Length Then msg = msgTmp Else msg = addmsg
        If 0 = msgboxTitle.Length Then msgboxTitle = msgTmp
        ''
        msg = msg & System.Environment.NewLine & System.Environment.NewLine & _
              "The following error has occurred:" & System.Environment.NewLine & _
              errorDesc
        ''
        MessageBox.Show(msg, msgboxTitle, MessageBoxButton.OK, MessageBoxImage.Error)
        ''
    End Sub
    Public Sub ErrorMessage(ByVal ex As Exception, ByVal addmsg As String, Optional ByVal msgboxTitle As String = "", Optional ReportError As Boolean = True)
        ''
        Call ErrorMessage(ex.Message, addmsg, msgboxTitle)
        If ReportError Then
            _Debug.ReportError(ex)
        End If

        _Debug.Print_(ex.Message)
        ''
    End Sub
    Public Sub ErrorMessageReport(ByVal ex As Exception, ByVal addmsg As String, Optional ByVal msgboxTitle As String = "")
        Call ErrorMessage(ex, addmsg, msgboxTitle)
    End Sub

    Public Sub WarningMessage(ByVal warningDesc As String, Optional ByVal addmsg As String = "", Optional ByVal msgboxTitle As String = "")
        If Not addmsg = String.Empty Then
            addmsg = (addmsg & _Controls.vbCr_ & _Controls.vbCr_)
        End If
        If msgboxTitle = String.Empty Then msgboxTitle = "Warning!"
        MessageBox.Show(addmsg & warningDesc, msgboxTitle, MessageBoxButton.OK, MessageBoxImage.Warning)
    End Sub

    Public Sub InformationMessage(ByVal infoDesc As String, Optional ByVal addmsg As String = "", Optional ByVal msgboxTitle As String = "")
        If Not addmsg = String.Empty Then
            addmsg = (addmsg & _Controls.vbCr_ & _Controls.vbCr_)
        End If
        If msgboxTitle = String.Empty Then msgboxTitle = "Note!"
        MessageBox.Show(addmsg & infoDesc, msgboxTitle, MessageBoxButton.OK, MessageBoxImage.Information)
    End Sub

    Public Function QuestionMessage(Optional ByVal questionDetails As String = "", Optional ByVal msgboxTitle As String = "", Optional ByVal addmsg As String = "") As Boolean
        QuestionMessage = False
        If Not questionDetails = String.Empty Then
            questionDetails = (questionDetails & _Controls.vbCr_ & _Controls.vbCr_)
        End If
        If msgboxTitle = String.Empty Then msgboxTitle = "Confirm!"
        If addmsg = String.Empty Then addmsg = "Are you sure?"
        If MessageBoxResult.Yes = MessageBox.Show(questionDetails & addmsg, msgboxTitle, MessageBoxButton.YesNo, MessageBoxImage.Question) Then
            Return True
        End If
    End Function

    Public Sub CallForAssistance(ByVal msg As String)
        MessageBox.Show(msg & _Controls.vbCr_ & _Controls.vbCr_ & "Please call for Assistance...", "Call for Assistance !", MessageBoxButton.OK, MessageBoxImage.Exclamation)
    End Sub
    Public Sub AddedSuccessfully(ByVal what As String)
        MessageBox.Show("Added Successfully !", what, MessageBoxButton.OK, MessageBoxImage.Information)
    End Sub
    Public Sub SavedSuccessfully(ByVal what As String)
        MessageBox.Show("Saved Successfully !", what, MessageBoxButton.OK, MessageBoxImage.Information)
    End Sub
    Public Sub DeletedSuccessfully(ByVal what As String)
        MessageBox.Show("Deleted Successfully !", what, MessageBoxButton.OK, MessageBoxImage.Information)
    End Sub

End Module
