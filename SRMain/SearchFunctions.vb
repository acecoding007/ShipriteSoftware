Imports System.Windows.Forms

Module SearchFunctions

    Public Function SearchList(CallingWindow As Window, SearchData As String, SearchType As String, SearchField As String, SearchTitle As String, SQL As String, SearchSeed As String, Optional MultiSelect As Boolean = False, Optional UseDataBaseSchema As Boolean = False) As String

        Dim MultiBuf As String
        Dim ret As Long
        Dim First As String = ""
        Dim Second As String = ""
        Dim Third As String = ""

        gPolicy(gSEARCH).buf = ""

        If MultiSelect = True Then

            MultiBuf = "True"

        Else

            MultiBuf = "False"

        End If
        gPolicy(gSEARCH).buf = ""
        ret = UpdateRunTimePolicy(gSEARCH, "SEARCHFILE", SearchType)
        ret = UpdateRunTimePolicy(gSEARCH, "SEARCHSEED", SearchSeed)
        ret = UpdateRunTimePolicy(gSEARCH, "SEARCHFIELD", SearchField)
        ret = UpdateRunTimePolicy(gSEARCH, "SEARCHTEXT", SearchData)
        ret = UpdateRunTimePolicy(gSEARCH, "SEARCHTITLE", SearchTitle)
        ret = UpdateRunTimePolicy(gSEARCH, "MULTISELECT", MultiBuf)
        If UseDataBaseSchema = True Then

            ret = UpdateRunTimePolicy(gSEARCH, "USEDATABASESCHEMA", "True")

        End If
        ret = UpdateRunTimePolicy(gSEARCH, "SEARCHSQL", SQL)
        If GetRunTimePolicy(gGLOBALpolicy, "SearchingInProgress") = "True" Then

            Return ""
            Exit Function

        End If
        ret = UpdateRunTimePolicy(gGLOBALpolicy, "SearchingInProgress", "True")
        Try

            Dim win As New SearchUniversal(CallingWindow)
            win.ShowDialog(CallingWindow)

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
        ret = UpdateRunTimePolicy(gGLOBALpolicy, "SearchingInProgress", "False")
        First = GetRunTimePolicy(gSEARCH, "FIRSTRESULT")
        Second = GetRunTimePolicy(gSEARCH, "SECONDRESULT")
        Third = GetRunTimePolicy(gSEARCH, "THIRDRESULT")
        Return First

    End Function

End Module
