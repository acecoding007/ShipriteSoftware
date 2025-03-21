Imports SHIPRITE.ShipRiteReports
Module StatementAndAccounting

    Public Function PostAccountAging(ANum As String, Bal As Double, Cur As Double, P30 As Double, P60 As Double, P90 As Double, P120 As Double, O120 As Double, Optional ARSegment As String = "") As Integer

        Dim ret As Integer
        Dim SQL As String
        Dim Segment As String
        Dim ID As Long
        Dim buf As String

        buf = ANum
        buf = FlushOut(buf, "'", "~")
        buf = FlushOut(buf, "~", "''")
        If ARSegment = "" Then

            SQL = "SELECT ID FROM AR WHERE AcctNum = '" & buf & "'"
            Segment = IO_GetSegmentSet(gShipriteDB, SQL)

        Else

            Segment = ARSegment

        End If
        If Not Segment = "" Then

            ID = Val(ExtractElementFromSegment("ID", Segment))
            SQL = "UPDATE AR SET Balance = " & Bal & ", [Current] = " & Cur & ", Plus30 = " & P30 & ", Plus60 = " & P60 &
            ", Plus90 = " & P90 & ", Plus120 = " & P120 & ", Over120 = " & O120 & " WHERE ID = " & ID
            ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

        Else

            ret = 0

        End If
        PostAccountAging = ret

    End Function

    Function PrintStatements() As Integer

        Dim report As New _ReportObject()
        report.ReportName = "Statement.rpt"
        Dim reportPrev As New ReportPreview(report)
        reportPrev.ShowDialog()

        Return 0

    End Function

End Module
