Module ShortcutKeyHandlers

    Public Sub KeyDown(sender As Object, e As KeyEventArgs, M As SHIPRITE.CommonWindow)
        Debug.Print(e.Key.ToString() & " Is down")
        Select Case e.Key
            Case Key.F5
                If e.KeyboardDevice IsNot Nothing AndAlso e.KeyboardDevice.Modifiers = ModifierKeys.Control Then  'leftCtrlIsHeld Or rightCtrlIsHeld Then
                    ' Launch Package Valet
                    Debug.Print("Launch Package Valet")
                    If _MailboxPackage.Open_PackageProcessingCenter(gShipriteDB, gMailboxDB, gReportsDB, gCurrentUser) Then
                        Dim win As New PackageValet(M)
                        win.ShowDialog(M)
                    End If
                Else
                    ' Launch DropOff Manager
                    Debug.Print("Launch Drop Off Manager")
                    Call _DropOff.Open_DropOffManager(M, gCurrentUser, Nothing)
                End If
        End Select
    End Sub

End Module
