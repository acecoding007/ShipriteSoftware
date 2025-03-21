Imports System
Imports System.IO
Imports System.Drawing
'Imports System.Windows.Forms
Imports System.Windows.Window
Imports System.Data
Imports System.ComponentModel
Imports SHIPRITE.ShipRiteReports



Public Class SearchUniversal
    Inherits CommonWindow

    Structure ColumnDefinition

        Dim Header As String
        Dim Width As Integer
        Dim Alignment As Integer
        Dim DataField As String
        Dim Format As String
        Dim DataType As String

    End Structure
    Dim argColumnSet(20) As ColumnDefinition
    Dim argMultiSelect As Boolean
    Dim argColumnCount As Integer
    Dim argPreprocessing As String
    Dim argPostprocessing As String
    Dim argDBPath As String
    Dim argSearchText As String
    Dim argSearchName As String
    Dim argSearchField As String
    Dim argSearchTitleText As String
    Dim argSearchData As String
    Dim argSearchSeed As String
    Dim argFontSize As Integer
    Dim argSelectCT As Integer
    Dim argSelectCT2 As Integer
    Dim argSelectCT3 As Integer
    Dim argColumnNumber As Integer
    Dim argRowHeight As Integer
    Dim argDeleteFromList As Integer
    Dim argTableName As String

    Private searchDT As DataTable
    Dim Last_Column_Sorted As String
    Dim Last_Sort_Ascending As Boolean

    Public Sub New()

        MyBase.New()

        ' This call is required by the designer.
        InitializeComponent()

    End Sub

    Public Sub New(ByVal callingWindow As Window)

        MyBase.New(callingWindow)

        ' This call is required by the designer.
        InitializeComponent()

    End Sub

    Private Function ProcessSelection() As Integer

        Dim row As Integer
        Dim ret As Integer
        Dim selRow As DataRowView

        If LV.SelectedItems.Count = 0 Then

            Return 0
            Exit Function

        End If

        row = LV.SelectedIndex 'row = LV.FocusedItem.Index
        selRow = LV.SelectedItem

        If argMultiSelect = False Then

            ret = UpdateRunTimePolicy(gSEARCH, "FIRSTRESULT", selRow.Item(argSelectCT).ToString())

        End If
        If argSelectCT2 > -1 Then

            ret = UpdateRunTimePolicy(gSEARCH, "SECONDRESULT", selRow.Item(argSelectCT2).ToString())

        End If
        If argSelectCT3 > -1 Then

            ret = UpdateRunTimePolicy(gSEARCH, "THIRDRESULT", selRow.Item(argSelectCT3).ToString())

        End If
        Me.Close()
        Return 0

    End Function

    Private Sub SearchUniversal_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Try

            Dim LineBuf As String
            Dim buf As String
            Dim iloc As Integer
            Dim word As String
            Dim Keyboard As Boolean
            Dim PROPERNAME As String
            Dim ret As Long

            Dim AddList As String
            Dim CloneAddList As String

            argPreprocessing = ""
            argPostprocessing = ""
            argSearchText = ""
            PROPERNAME = False

            'LV.Left = (Me.Width - LV.Width) / 2
            'LV.Top = 136
            'LV.Height = (Me.Height - LV.Top) - 200

            argSearchName = GetRunTimePolicy(gSEARCH, "SEARCHFILE")

            If argSearchName = "" Then

                Exit Sub

            End If
            argSearchTitleText = GetRunTimePolicy(gSEARCH, "SEARCHTITLE")
            buf = gAppPath & "\LST_Files\" & argSearchName & ".lst"

            If Not File.Exists(buf) Then

                MsgBox("ATTENTION...Cannot Load Search Configuration File" & vbCrLf & vbCrLf & gAppPath & "\" & argSearchName & ".lst", vbCritical, gProgramName)

                'Me.Cursor = Cursors.Default

                Me.Close()
                Exit Sub

            End If

            argSearchData = GetRunTimePolicy(gSEARCH, "SEARCHSQL")

            argSearchSeed = GetRunTimePolicy(gSEARCH, "SEARCHSEED")
            ''SQLStatement.Content = FlushOut(argSearchData, "<<SEED>>", argSearchSeed)
            SQLStatement.Text = FlushOut(argSearchData, "<<SEED>>", argSearchSeed)
            ret = UpdateRunTimePolicy(gGLOBALpolicy, "DataBaseName", gCommonTableProcessorDB)

            Using sr As StreamReader = File.OpenText(buf)

                Do While sr.Peek() >= 0

                    LineBuf = sr.ReadLine()
                    iloc = InStr(1, LineBuf, " ")
                    If Not iloc = 0 Then

                        word = Strings.Mid$(LineBuf, 1, iloc - 1)
                        LineBuf = Trim$(Strings.Mid$(LineBuf, iloc + 1))

                    Else

                        word = LineBuf
                        LineBuf = ""

                    End If
                    Select Case word

                        Case "[COLUMNCOUNT]"

                            argColumnCount = Val(LineBuf)

                        Case "[TITLE]"

                            If argSearchTitleText = "" Then

                                WindowLabel.Content = LineBuf

                            Else

                                WindowLabel.Content = argSearchTitleText

                            End If

                        Case "[SHOWLookupPanel]"

                            If LineBuf = "False" Then

                                LookupPanel.Visibility = Visibility.Hidden

                            End If

                        Case "[LABELCAPTION]"

                            SearchLabel.Content = LineBuf

                        Case "[PREPROCESSING]"

                            argPreprocessing = LineBuf

                        Case "[SEARCHCASE]"

                            Select Case LineBuf

                                Case "UPPER"

                                    SData.Tag = "UPPER"

                                Case "LOWER"

                                    SData.Tag = "LOWER"

                                Case "PROPER"

                                    SData.Tag = "PROPER"

                            End Select

                        Case "[COUNTERFIELD]"

                            ret = UpdateRunTimePolicy(gSEARCH, "COUNTERFIELD", LineBuf)

                        Case "[POSTPROCESSING]"

                            argPostprocessing = LineBuf

                        Case "[DATABASE]"

                            Select Case LineBuf

                                Case "Shiprite"

                                    ret = UpdateRunTimePolicy(gGLOBALpolicy, "DataBaseName", gShipriteDB)
                                    argDBPath = gShipriteDB

                                Case "Transactions"

                                    ret = UpdateRunTimePolicy(gGLOBALpolicy, "DataBaseName", gTransactionLog)
                                    argDBPath = gTransactionLog

                                Case "SmartTouch", "EasySale"

                                    LineBuf = "SmartTouch"
                                    ret = UpdateRunTimePolicy(gGLOBALpolicy, "DataBaseName", gShipriteDB)
                                    argDBPath = gShipriteDB

                                Case "Reports"

                                    ret = UpdateRunTimePolicy(gGLOBALpolicy, "DataBaseName", gReportsDB)
                                    argDBPath = gReportsDB

                                Case "FeeSchedules"

                                    ret = UpdateRunTimePolicy(gGLOBALpolicy, "DataBaseName", gFeesDB)
                                    argDBPath = gFeesDB

                                Case "QuickBooks"

                                    ret = UpdateRunTimePolicy(gGLOBALpolicy, "DataBaseName", gQBdb)
                                    argDBPath = gQBdb

                                Case "Dental"

                                    ret = UpdateRunTimePolicy(gGLOBALpolicy, "DataBaseName", gDentouchDB)
                                    argDBPath = gDentouchDB

                                Case "Finance"

                                    ret = UpdateRunTimePolicy(gGLOBALpolicy, "DataBaseName", gFinanceDB)
                                    argDBPath = gFinanceDB

                                Case "Service"

                                    ret = UpdateRunTimePolicy(gGLOBALpolicy, "DataBaseName", gServiceDB)
                                    argDBPath = gServiceDB

                                Case "Security"

                                    ret = UpdateRunTimePolicy(gGLOBALpolicy, "DataBaseName", gSecurityDB)
                                    argDBPath = gSecurityDB

                            End Select
                            gCommonTableProcessorDB = argDBPath
                            ret = UpdateRunTimePolicy(gGLOBALpolicy, "DataBaseName", gCommonTableProcessorDB)

                        Case "[SEARCHFIELD1]"

                            ret = UpdateRunTimePolicy(gSEARCH, "SEARCHFIELD1", LineBuf)

                        Case "[SEARCHFIELD2]"

                            ret = UpdateRunTimePolicy(gSEARCH, "SEARCHFIELD2", LineBuf)

                        Case "[SPECIAL]"

                            If InStr(1, LineBuf, "PROPERNAME") > 0 Then

                                PROPERNAME = True

                            End If

                        Case "[SQL]"

                            argSearchData = GetRunTimePolicy(gSEARCH, "SEARCHSQL")
                            If argSearchData = "" Then

                                argSearchData = LineBuf
                                ret = UpdateRunTimePolicy(gSEARCH, "SEARCHSQL", argSearchData)

                            End If

                        Case "[FIRSTRETURNCOLUMN]"

                            argSelectCT = Val(LineBuf)

                        Case "[SECONDRETURNCOLUMN]"

                            argSelectCT2 = Val(LineBuf)

                        Case "[THIRDRETURNCOLUMN]"

                            argSelectCT3 = Val(LineBuf)

                        Case "[FONTSIZE]"

                            argFontSize = Val(LineBuf)

                        Case "[SEARCHFIELD]"

                            argSearchField = LineBuf

                        Case "[COLUMN]"

                            argColumnNumber = Val(LineBuf)

                        Case "[WIDTH]"

                            argColumnSet(argColumnNumber).Width = Val(LineBuf)

                        Case "[DATAFIELD]"

                            argColumnSet(argColumnNumber).DataField = LineBuf

                        Case "[ALIGNHEADER]"

                            argColumnSet(argColumnNumber).Alignment = LineBuf

                        Case "[HEADERTEXT]"

                            argColumnSet(argColumnNumber).Header = LineBuf

                        Case "[HIDE]"

                            argColumnSet(argColumnNumber).Width = 0

                        Case "[ROWHEIGHT]"

                            argRowHeight = Val(LineBuf)

                        Case "[DATATYPE]"
                            argColumnSet(argColumnNumber).DataType = LineBuf

                        Case "[SHOWADDBUTTON]"

                            AddButton.Visibility = Visibility.Visible
                            AddList = LineBuf

                        Case "[SHOWCLONEBUTTON]"

                            CloneButton.Visibility = Visibility.Visible
                            CloneAddList = LineBuf

                        Case "[SHOWDELETEBUTTON]"
                            If LineBuf = True Then
                                DeleteButton.Visibility = Visibility.Visible
                            Else
                                DeleteButton.Visibility = Visibility.Hidden
                            End If
                            argDeleteFromList = Val(LineBuf)

                        Case "[TABLE]"

                            argTableName = LineBuf
                            ret = UpdateRunTimePolicy(gGLOBALpolicy, "TableName", LineBuf)

                    End Select

                Loop

            End Using

            argMultiSelect = False
            gSearchResult = ""
            SData.Text = Trim$(GetRunTimePolicy(gSEARCH, "SEARCHSEED"))
            ret = UpdateRunTimePolicy(gSEARCH, "FIRSTTIME", "True")

            gDataEntryDB = argDBPath
            If Not gShipriteDB = "" Then

                gDataEntryDB = gShipriteDB

            End If
            If Not gDentouchDB = "" Then

                gDataEntryDB = gDentouchDB

            End If
            If Not gShipriteDB = "" Then

                gDataEntryDB = gShipriteDB

            End If

            Keyboard = False
            'SQLStatement.Top = LV.Top + LV.Height
            'LineCounter.Top = SQLStatement.Top
            'SQLStatement.Left = LV.Left
            'SQLStatement.Width = LV.Width - LineCounter.Width

            ' Clear LV
            LV_Clear()

            ' set up columns
            LV_AddColumns()

            Me.Cursor = Cursors.Wait
            ''ret = IO_LoadListView(LV, searchDT, gShipriteDB, SQLStatement.Content, argColumnCount)
            ret = IO_LoadListView(LV, searchDT, gShipriteDB, SQLStatement.Text, argColumnCount)
            Me.Cursor = Cursors.Arrow
            'SData.SelectionStart = Len(SData.Text)
            'SData.Select()
            SData.SelectAll()
            If LV.Items.Count >= 1 Then
                LV.SelectedIndex = 0
                LV.Focus()
            End If


            If argSearchName = "Invoices" Then
                DaysToShow_TxtBx.Text = My.Settings.POS_InvoiceHistory_DaysToShow
            Else
                DaysToShow_TxtBx.Visibility = Visibility.Hidden
                DaysToShow_Txt.Visibility = Visibility.Hidden
                PrintButton.Visibility = Visibility.Hidden
                InvoiceFrom_TxtBx.Visibility = Visibility.Hidden
                InvoiceFrom_Lbl.Visibility = Visibility.Hidden
                InvoiceTo_TxtBx.Visibility = Visibility.Hidden
                InvoiceTo_Lbl.Visibility = Visibility.Hidden
            End If

        Catch ex As Exception

            MessageBox.Show(Err.Description)
            Me.Close()

        End Try
    End Sub

    Private Sub LV_Clear()

        BindingOperations.ClearAllBindings(LV) ' clear data rows
        ''LV.ItemsSource = Nothing
        LV.DataContext = Nothing ' clear data rows by clearing connection to datatable
        LV.View = New GridView ' clear columns
        LV.Items.Clear() ' clear items just in case

    End Sub

    Private Sub LV_AddColumns()

        searchDT = New DataTable ' init

        Dim searchGrid As GridView = New GridView
        Dim searchCol As GridViewColumn
        searchGrid.AllowsColumnReorder = False

        For i = 0 To argColumnCount - 1
            searchCol = New GridViewColumn
            searchCol.DisplayMemberBinding = New Binding(argColumnSet(i).DataField)
            searchCol.Header = argColumnSet(i).Header
            searchCol.Width = argColumnSet(i).Width


            If argColumnSet(i).DataType = "Date" Then
                searchCol.DisplayMemberBinding.StringFormat = "MM/dd/yyyy"
                searchGrid.Columns.Add(searchCol)
                searchDT.Columns.Add(argColumnSet(i).DataField, GetType(Date))
            ElseIf argColumnSet(i).DataType = "Double" Then
                searchCol.DisplayMemberBinding.StringFormat = "$ 0.00"
                searchGrid.Columns.Add(searchCol)
                searchDT.Columns.Add(argColumnSet(i).DataField, GetType(Double))

            Else
                searchGrid.Columns.Add(searchCol)
                searchDT.Columns.Add(argColumnSet(i).DataField)
            End If
        Next

        LV.View = searchGrid

        If Not argFontSize = 0 Then

            LV.FontSize = argFontSize
            LV.FontStyle = FontStyles.Normal

        End If

    End Sub

    Private Sub SData_TextChanged(sender As Object, e As TextChangedEventArgs) Handles SData.TextChanged

        'LV_Clear()
        SData.Text = StrConv(SData.Text, VbStrConv.ProperCase)
        SData.SelectionStart = SData.Text.Length + 1

    End Sub

    Private Sub SData_KeyDown(sender As Object, e As KeyEventArgs) Handles SData.KeyDown

        Dim sql As String
        Dim SegmentSet As String
        Dim i As Integer
        Dim ret As Integer
        If e.Key = Key.Down Then

            If Not LV.Items.Count = 0 Then

                i = 0
                'LV.Items(i).Selected = True
                'LV.Select()
                LV.SelectedIndex = i

            End If
            Exit Sub

        End If
        If Not e.Key = Key.Enter Then

            Exit Sub

        End If
        If argSearchName = "Invoices" And Not SData.Text = "" Then

            sql = "SELECT COUNT(*) AS Tally FROM Payments WHERE InvNum = '" & SData.Text & "'"
            SegmentSet = IO_GetSegmentSet(gShipriteDB, sql)
            If Not Val(ExtractElementFromSegment("Tally", SegmentSet)) = 0 Then

                ret = UpdateRunTimePolicy(gSEARCH, "FIRSTRESULT", SData.Text)
                Me.Close()
                Exit Sub

            End If

        End If
        Refresh_LV()

    End Sub

    Private Sub Refresh_LV()

        If argSearchName = "Invoices" And SData.Text <> "" Then
            SQLStatement.Text = FlushOut(argSearchData, "<<SEED>>", " AND InvNum='" & SData.Text & "'")
        Else
            SQLStatement.Text = FlushOut(argSearchData, "<<SEED>>", SData.Text)
        End If

        ' Clear LV
        LV_Clear()

        ' set up columns
        LV_AddColumns()

        Dim ret As Long
        Me.Cursor = Cursors.Wait
        ''ret = IO_LoadListView(LV, searchDT, gShipriteDB, SQLStatement.Content, argColumnCount)
        ret = IO_LoadListView(LV, searchDT, gShipriteDB, SQLStatement.Text, argColumnCount)

        Me.Cursor = Cursors.Arrow
    End Sub

    Private Sub LV_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles LV.MouseDoubleClick

        Dim ret As Integer

        ret = ProcessSelection()

    End Sub

    Private Sub LV_KeyDown(sender As Object, e As KeyEventArgs) Handles LV.KeyDown

        Dim ret As Integer

        If Not e.Key = Key.Enter Then

            Exit Sub

        End If
        ret = ProcessSelection()

    End Sub

    Private Sub ColumnHeader_Click(sender As Object, e As RoutedEventArgs)
        Try

            Sort_LV_byColumn(LV, TryCast(e.OriginalSource, GridViewColumnHeader), Last_Sort_Ascending, Last_Column_Sorted)

        Catch ex As Exception : _MsgBox.ErrorMessage(ex.Message, "Failed to sort list items...")
        End Try
    End Sub
    Private Sub AddButton_Click(sender As Object, e As RoutedEventArgs) Handles AddButton.Click

        gResult = SData.Text
        Me.Close()

    End Sub

    Private Sub DeleteButton_Click(sender As Object, e As RoutedEventArgs) Handles DeleteButton.Click

        Dim row As Integer
        Dim ret As Integer
        Dim selRow As DataRowView
        Dim ID As Long = 0
        Dim SQL As String

        If LV.SelectedItems.Count = 0 Then

            Exit Sub

        End If
        ret = MsgBox("ATTENTION...Delete Row" & vbCrLf & vbCrLf & "CONTINUE", vbQuestion + vbYesNo, gProgramName)
        If ret = vbNo Then

            Exit Sub

        Else

            row = LV.SelectedIndex 'row = LV.FocusedItem.Index
            selRow = LV.SelectedItem
            ID = selRow.Item(argDeleteFromList).ToString()
            SQL = "DELETE * FROM " & argTableName & " WHERE ID = " & ID.ToString
            ret = IO_UpdateSQLProcessor(argDBPath, SQL)
            MsgBox(ret.ToString & " - ROWS AFFECTED")
            '           LV.SelectedItems(row).remove()


        End If

    End Sub

#Region "Invoices Number of Days to Show"
    Private Sub DaysToShow_TxtBx_LostFocus(sender As Object, e As RoutedEventArgs) Handles DaysToShow_TxtBx.LostFocus
        Save_DaysToShow()
    End Sub

    Private Sub Invoicedaterange_TxtBx_SelectedDateChanged(sender As Object, e As RoutedEventArgs) Handles InvoiceFrom_TxtBx.SelectedDateChanged, InvoiceTo_TxtBx.SelectedDateChanged
        Save_DateRangeToShow()
    End Sub

    Private Sub DaysToShow_TxtBx_KeyDown(sender As Object, e As KeyEventArgs) Handles DaysToShow_TxtBx.KeyDown
        If e.Key = Key.Enter Then
            Save_DaysToShow()
        End If
    End Sub

    Private Sub PrintButton_Click(sender As Object, e As RoutedEventArgs) Handles PrintButton.Click
        Dim insertfromDate As String
        Dim insertendDate As String
        Dim report As New _ReportObject
        Dim sql As String
        Dim RecordCT As Long = 0
        Dim TransRecordSet As RecordSetDefinition


        If InvoiceFrom_TxtBx.SelectedDate IsNot Nothing Then
            insertfromDate = InvoiceFrom_TxtBx.SelectedDate.Value.ToString("MM/dd/yyyy")
        Else
            insertfromDate = Date.Today.AddDays(-CInt(DaysToShow_TxtBx.Text)).ToString("MM/dd/yyyy")
        End If

        If InvoiceTo_TxtBx.SelectedDate IsNot Nothing Then
            insertendDate = InvoiceTo_TxtBx.SelectedDate.Value.ToString("MM/dd/yyyy")
        Else
            insertendDate = Date.Today.ToString("MM/dd/yyyy")
        End If


        Dim ret As Integer
        ret = IO_UpdateSQLProcessor(gReportWriter, "DELETE * FROM Payments")

        RecordCT = IO_GetSegmentSetInToStructure(gShipriteDB, SQLStatement.Text, TransRecordSet)


        ret = io_DumpRecordsetToLocalTable(gReportWriter, TransRecordSet, "Payments", True)

        sql = "UPDATE PrintInformation SET Dates = '" & insertfromDate & " - " & insertendDate & "'"
        ret = IO_UpdateSQLProcessor(gReportWriter, sql)

        ret = UpdatePolicy(gReportWriter, "DateRange", insertfromDate & " - " & insertendDate)


        report.ReportName = "InvoiceSearch.rpt"
        Dim reportPrev As New ReportPreview(report)
        reportPrev.ShowDialog()

    End Sub
    Private Sub Save_DaysToShow()
        Dim x As Integer
        Dim insertDate As String
        If IsNumeric(DaysToShow_TxtBx.Text) Then
            My.Settings.POS_InvoiceHistory_DaysToShow = CInt(DaysToShow_TxtBx.Text)
            My.Settings.Save()

            x = argSearchData.IndexOf("#")

            insertDate = Date.Today.AddDays(-CInt(DaysToShow_TxtBx.Text)).ToString("MM/dd/yyyy")

            argSearchData = argSearchData.Remove(x + 1, 10).Insert(x + 1, insertDate)


            Refresh_LV()
        End If


    End Sub

    Private Sub Save_DateRangeToShow()
        Dim x As Integer
        Dim insertstartDate As String
        Dim insertendDate As String
        Dim daysDifference As Integer


        x = argSearchData.IndexOf("#")
        insertstartDate = InvoiceFrom_TxtBx.SelectedDate.Value.ToString("MM/dd/yyyy")
        argSearchData = argSearchData.Remove(x + 1, 10).Insert(x + 1, insertstartDate)

        If InvoiceTo_TxtBx.SelectedDate IsNot Nothing Then
            insertendDate = InvoiceTo_TxtBx.SelectedDate.Value.ToString("MM/dd/yyyy")
        Else
            insertendDate = Date.Today.ToString("MM/dd/yyyy")
        End If

        argSearchData = argSearchData.Remove(x + 28, 10).Insert(x + 28, insertendDate)

        daysDifference = (DateTime.Parse(insertendDate) - DateTime.Parse(insertstartDate)).Days
        DaysToShow_TxtBx.Text = daysDifference.ToString()
        My.Settings.POS_InvoiceHistory_DaysToShow = Nothing
        My.Settings.Save()


        Refresh_LV()


    End Sub


#End Region

End Class

