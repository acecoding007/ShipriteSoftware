Imports Microsoft.VisualBasic

Public Module _Format

    Public Function DollarText(ByVal value As String) As String
        ''
        DollarText = "$0.00" '' assume.
        Try
            If Not 0 = value.Length Then
                Dim tmpNo As Double = Val(value.Replace("$", ""))
                DollarText = Format(tmpNo, "$#,##0.00")
            End If
        Catch ex As Exception : _Debug.Print_("_Format.DollarText(): " & ex.Message)
        End Try
        ''
    End Function

    Public Function PercentText(ByVal value As String) As String
        ''
        PercentText = "0%" '' assume.
        Try
            If Not 0 = value.Length Then
                Dim tmpNo As Double = Val(value.Replace("%", ""))
                PercentText = Format(tmpNo / 100, "#,##0%") '' % format multiplies value by 100 in the System
            End If
        Catch ex As Exception : _Debug.Print_("_Format.PercentText(): " & ex.Message)
        End Try
        ''
    End Function

End Module
