Imports System.Threading
Imports System.Windows.Threading
Imports SHIPRITE.ShipRiteReports

Public Class TimeClock
    Inherits CommonWindow
    Private isAdmin As Boolean
    Private timer As System.Timers.Timer = New System.Timers.Timer(1000)

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

    Private Sub TimeClock_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Try
            CheckUser()

            AddHandler timer.Elapsed, New System.Timers.ElapsedEventHandler(AddressOf timer_Elapsed)
            timer.Enabled = True

            secondHand.Angle = DateTime.Now.Second * 6
            minuteHand.Angle = DateTime.Now.Minute * 6
            hourHand.Angle = (DateTime.Now.Hour * 30) + (DateTime.Now.Minute * 0.5)

            Date_TxtBx.Text = Now.ToString("D")
            Time_TxtBx.Text = Now.ToString("t")

            TimeIn_AMPM_CB.Items.Add("AM")
            TimeIn_AMPM_CB.Items.Add("PM")
            TimeIn_AMPM_CB.SelectedIndex = 0

            TimeOut_AMPM_CB.Items.Add("AM")
            TimeOut_AMPM_CB.Items.Add("PM")
            TimeOut_AMPM_CB.SelectedIndex = 1

            Report_StartDate.SelectedDate = Today
            Report_EndDate.SelectedDate = Today

            Load_Timeclock_LV()
            Check_ClockIN_OR_ClockOUT()

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try

    End Sub

    Private Sub Check_ClockIN_OR_ClockOUT()
        Try
            Dim SQL As String = "SELECT DateIn, TimeIn, DateOut From TimeClock WHERE UserName='" & UserName_TxtBx.Text & "' AND [DateOut] IS NULL"
            Dim segment As String = IO_GetSegmentSet(gShipriteDB, SQL)

            If segment = "" Then
                'No empty DateOut present, user is clocking In
                Clock_InOut_Btn.Content = "CLOCK IN"

                ClockOut_Border.Visibility = Visibility.Hidden

            Else
                'Empty DateOut present, user is clocking out
                Clock_InOut_Btn.Content = "CLOCK OUT"
                ClockOut_Border.Visibility = Visibility.Visible

                Dim DateTimeIn As DateTime
                Dim DateIn As DateTime = ExtractElementFromSegment("DateIn", segment)
                Dim TimeIn As DateTime = ExtractElementFromSegment("TimeIn", segment)

                DateTimeIn = DateIn.Add(TimeIn.TimeOfDay)

                ClockOut_DateTime_TxtBx.Content = DateTimeIn
                ClockOut_HoursWorked_TxtBx.Content = CalculateHoursWorked(DateTimeIn, DateTime.Now) & " hrs."

            End If

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Function CalculateHoursWorked(DateTimeIn As DateTime, DateTimeOUT As DateTime) As Double
        Try

            Dim duration = DateTimeOUT - DateTimeIn

            Return Round(duration.TotalHours, 2)

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
        Return 0.0
    End Function

    Private Sub CheckUser()
        Try
            'Check if user has Admin rights

            Dim buf As String
            Dim current_segment As String
            UserName_TxtBx.Text = gCurrentUser
            isAdmin = Check_Current_User_Permission("Setup_Users", True)


            If isAdmin Then
                Admin_Border.Visibility = Visibility.Visible
                AdminClerk_Border.Visibility = Visibility.Visible
                PrintAll_ChkBx.Visibility = Visibility.Visible

                buf = IO_GetSegmentSet(gShipriteDB, "SELECT DisplayName From Users")

                Do Until buf = ""
                    current_segment = GetNextSegmentFromSet(buf)
                    Clerk_CB.Items.Add(ExtractElementFromSegment("DisplayName", current_segment, ""))
                    Clerk_CB.SelectedItem = gCurrentUser
                Loop


            Else
                Admin_Border.Visibility = Visibility.Hidden
                AdminClerk_Border.Visibility = Visibility.Hidden
                PrintAll_ChkBx.Visibility = Visibility.Hidden

            End If


        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub


    Private Sub Load_Timeclock_LV()
        Try
            Dim SQL As String = "SELECT * From TimeClock WHERE UserName='" & UserName_TxtBx.Text & "' AND [DateIn] >= Now()-" & DisplayDays_TxtBx.Text & " ORDER BY DateIn DESC, TimeIn DESC"

            BindingOperations.ClearAllBindings(TimeClock_LV) ' clear binding on ListView
            TimeClock_LV.DataContext = Nothing ' remove any rows already in ListView

            Dim DT As New System.Data.DataTable ' datatable to use to populate ListView

            Dim currentGridView As GridView = TimeClock_LV.View 'Set currentgridview to be the view setup in XML.

            ' add same column names to datatable columns
            DT.Columns.Add("ID")
            DT.Columns.Add("UserName")
            DT.Columns.Add("DateIn", GetType(Date))
            DT.Columns.Add("TimeIn", GetType(Date))
            DT.Columns.Add("DateOut", GetType(Date))
            DT.Columns.Add("TimeOut", GetType(Date))
            DT.Columns.Add("Hours", GetType(Double))
            DT.Columns.Add("Notes")



            ' return the # of rows added to ListView
            IO_LoadListView(TimeClock_LV, DT, gShipriteDB, SQL, currentGridView.Columns.Count)

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub timer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs)

        Me.Dispatcher.Invoke(DispatcherPriority.Normal, CType((Sub()
                                                                   secondHand.Angle = DateTime.Now.Second * 6
                                                                   minuteHand.Angle = DateTime.Now.Minute * 6
                                                                   hourHand.Angle = (DateTime.Now.Hour * 30) + (DateTime.Now.Minute * 0.5)

                                                                   Date_TxtBx.Text = Now.ToString("D")
                                                                   Time_TxtBx.Text = Now.ToString("t")
                                                               End Sub), Action))



    End Sub

    Private Sub NumericTxtBox_PreviewTextInput(sender As Object, e As TextCompositionEventArgs) Handles DisplayDays_TxtBx.PreviewTextInput, TimeIn_Hrs_CB.PreviewTextInput, TimeIn_Min_CB.PreviewTextInput, TimeOut_Hrs_CB.PreviewTextInput, TimeOut_Min_CB.PreviewTextInput
        Try
            Dim allowedchars As String = "0123456789"
            If allowedchars.IndexOf(CChar(e.Text)) = -1 Then e.Handled = True

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Refresh_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Refresh_Btn.Click
        Load_Timeclock_LV()
    End Sub

    Private Sub DisplayDays_TxtBx_LostFocus(sender As Object, e As RoutedEventArgs) Handles DisplayDays_TxtBx.LostFocus
        Try
            If DisplayDays_TxtBx.Text = "" Then DisplayDays_TxtBx.Text = "0"

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub TimeClock_LV_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles TimeClock_LV.SelectionChanged
        Try
            If TimeClock_LV.SelectedIndex = -1 Then Exit Sub
            If isAdmin = False Then Exit Sub

            DateIn_DP.SelectedDate = TimeClock_LV.SelectedItem.item("DateIn")
            Dim TimeIn As DateTime = TimeClock_LV.SelectedItem.item("TimeIn")

            '------------
            TimeIn_Hrs_CB.Text = CInt(TimeIn.ToString("hh"))
            TimeIn_Min_CB.Text = TimeIn.ToString("mm")


            TimeIn_AMPM_CB.SelectedItem = TimeIn.ToString("tt", System.Globalization.CultureInfo.InvariantCulture)
            Notes_TxtBx.Text = TimeClock_LV.SelectedItem.item("Notes")

            If Not IsDBNull(TimeClock_LV.SelectedItem.item("DateOut")) Then
                DateOut_DP.SelectedDate = TimeClock_LV.SelectedItem.item("DateOut")

                Dim TimeOut As DateTime = TimeClock_LV.SelectedItem.item("TimeOut")
                TimeOut_Hrs_CB.Text = TimeOut.ToString("hh")
                TimeOut_Min_CB.Text = TimeOut.ToString("mm")
                TimeOut_AMPM_CB.SelectedItem = TimeOut.ToString("tt", System.Globalization.CultureInfo.InvariantCulture)
            Else
                DateOut_DP.SelectedDate = Nothing
                TimeOut_Hrs_CB.Text = ""
                TimeOut_Min_CB.Text = ""

            End If

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Clock_InOut_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Clock_InOut_Btn.Click
        Try
            If ClockOut_Border.Visibility = Visibility.Visible Then
                'CLOCK OUT
                Dim segment As String = IO_GetSegmentSet(gShipriteDB, "SELECT ID, DateIn, TimeIn From TimeClock WHERE UserName='" & UserName_TxtBx.Text & "' AND [DateOut] IS NULL")
                If segment = "" Then
                    MsgBox("Error! Cannot Clock Out!", vbExclamation)
                    Me.Close()
                    Exit Sub
                End If

                Dim ID As Integer = ExtractElementFromSegment("ID", segment)
                Dim DateIn As DateTime = ExtractElementFromSegment("DateIn", segment)
                Dim TimeIn As DateTime = ExtractElementFromSegment("TimeIn", segment)

                Dim DateTimeIn As DateTime = DateIn.Add(TimeIn.TimeOfDay)

                Dim SQL As String = "UPDATE TimeClock set DateOut=#" & Now.ToString("d") & "#, TimeOut=#" & Now.ToString("t") & "#, Hours=" & CalculateHoursWorked(DateTimeIn, DateTime.Now) & " WHERE ID=" & ID
                IO_UpdateSQLProcessor(gShipriteDB, SQL)


            Else
                'CLOCK IN
                Dim SQL As String = "INSERT INTO TimeClock (UserName, DateIn, TimeIn) VALUES ('" & UserName_TxtBx.Text & "',  #" & Now.ToString("d") & "#, #" & Now.ToString("t") & "#)"
                IO_UpdateSQLProcessor(gShipriteDB, SQL)

            End If

            Me.Close()

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Clerk_CB_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles Clerk_CB.SelectionChanged
        Try
            UserName_TxtBx.Text = Clerk_CB.SelectedItem
            Load_Timeclock_LV()
            Check_ClockIN_OR_ClockOUT()

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub DateIn_DP_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs) Handles DateIn_DP.SelectedDateChanged

        If IsNothing(DateOut_DP.SelectedDate) Then
            DateOut_DP.SelectedDate = DateIn_DP.SelectedDate
        End If
    End Sub



#Region "Add, Delete, Edit Buttons"

    Private Sub Delete_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Delete_Btn.Click
        Try
            If TimeClock_LV.SelectedIndex = -1 Then Exit Sub
            If MsgBox("Are you sure you want to delete the selected line entry? ", vbQuestion + vbYesNo) = MsgBoxResult.No Then Exit Sub

            IO_UpdateSQLProcessor(gShipriteDB, "Delete * From TimeClock Where ID=" & TimeClock_LV.SelectedItem.item("ID"))


            Clear_Admin_Options()
            Load_Timeclock_LV()

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub SaveChanges_Btn_Click(sender As Object, e As RoutedEventArgs) Handles SaveChanges_Btn.Click
        Try
            If TimeClock_LV.SelectedIndex = -1 Then Exit Sub
            Dim Time As String
            Dim SQL As String


            'Save Date and Time IN
            Time = TimeIn_Hrs_CB.Text & ":" & TimeIn_Min_CB.Text & " " & TimeIn_AMPM_CB.Text
            SQL = "UPDATE TimeClock set DateIn=#" & DateIn_DP.SelectedDate & "#, TimeIn=#" & Time & "#, Notes='" & Notes_TxtBx.Text & "' Where ID=" & TimeClock_LV.SelectedItem.item("ID")
            IO_UpdateSQLProcessor(gShipriteDB, SQL)


            'Save Date and Time OUT
            If Not IsNothing(DateOut_DP.SelectedDate) And TimeOut_Hrs_CB.Text <> "" And TimeOut_Min_CB.Text <> "" Then

                Dim DT_In As DateTime = Get_DT_In()
                Dim DT_Out As DateTime = Get_DT_Out()


                SQL = "UPDATE TimeClock set DateOut=#" & DateOut_DP.SelectedDate & "#, TimeOut=#" & DT_Out.ToString("t") & "#, Hours=" & CalculateHoursWorked(DT_In, DT_Out) & " Where ID=" & TimeClock_LV.SelectedItem.item("ID")
                IO_UpdateSQLProcessor(gShipriteDB, SQL)

            End If

            Clear_Admin_Options()
            Load_Timeclock_LV()


        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub


    Private Sub AddNew_Btn_Click(sender As Object, e As RoutedEventArgs) Handles AddNew_Btn.Click
        Try

            If IsNothing(DateIn_DP.SelectedDate) Or TimeIn_Hrs_CB.Text = "" Or TimeIn_Min_CB.Text = "" Or TimeIn_AMPM_CB.Text = "" Then
                MsgBox("Cannot add new time period. Some fields were left blank!", vbExclamation)
                Exit Sub
            End If

            Dim SQL As String
            Dim DT_In As DateTime = Get_DT_In()
            Dim DT_Out As DateTime = Get_DT_Out()


            If IsNothing(DateOut_DP.SelectedDate) Or TimeOut_Hrs_CB.Text = "" Or TimeOut_Min_CB.Text = "" Then
                'ONLY CLOCK IN IS ADDED
                SQL = "INSERT INTO TimeClock (UserName, DateIn, TimeIn, Notes) VALUES ('" & UserName_TxtBx.Text & "',  #" & DT_In.ToString("d") & "#, #" & DT_In.ToString("t") & "#, '" & Notes_TxtBx.Text & "')"
                IO_UpdateSQLProcessor(gShipriteDB, SQL)

            Else
                'CLOCK IN AND OUT
                SQL = "INSERT INTO TimeClock (UserName, DateIn, TimeIn, DateOut, TimeOut, Hours, Notes) VALUES ('" & UserName_TxtBx.Text & "',  #" & DT_In.ToString("d") & "#, #" & DT_In.ToString("t") & "#, #" & DT_Out.ToString("d") & "#, #" & DT_Out.ToString("t") & "#, " & CalculateHoursWorked(DT_In, DT_Out) & ", '" & Notes_TxtBx.Text & "')"
                IO_UpdateSQLProcessor(gShipriteDB, SQL)

            End If

            Clear_Admin_Options()
            Load_Timeclock_LV()

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Function Get_DT_In() As DateTime
        Dim DT_In As DateTime
        Dim Time As String

        DT_In = DateIn_DP.SelectedDate
        Time = TimeIn_Hrs_CB.Text & ":" & TimeIn_Min_CB.Text & " " & TimeIn_AMPM_CB.Text
        DT_In = DT_In.Add(Convert.ToDateTime(Time).TimeOfDay)

        Return DT_In

    End Function

    Private Function Get_DT_Out() As DateTime

        Dim DT_Out As DateTime
        Dim Time As String

        DT_Out = DateOut_DP.SelectedDate
        Time = TimeOut_Hrs_CB.Text & ":" & TimeOut_Min_CB.Text & " " & TimeOut_AMPM_CB.Text
        DT_Out = DT_Out.Add(Convert.ToDateTime(Time).TimeOfDay)

        Return DT_Out

    End Function

    Private Sub Clear_Admin_Options()
        Try
            DateIn_DP.SelectedDate = Nothing
            DateOut_DP.SelectedDate = Nothing

            TimeIn_Hrs_CB.Text = ""
            TimeIn_Min_CB.Text = ""
            TimeIn_AMPM_CB.SelectedIndex = -1

            TimeOut_Hrs_CB.Text = ""
            TimeOut_Min_CB.Text = ""
            TimeOut_AMPM_CB.SelectedIndex = -1

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub PrintReport_Btn_Click(sender As Object, e As RoutedEventArgs) Handles PrintReport_Btn.Click
        Try
            Cursor = Cursors.Wait
            Dim report As New _ReportObject()
            report.ReportName = "TimeClock.rpt"

            If PrintAll_ChkBx.IsChecked Then
                report.ReportFormula = "{TimeClock.DateIn} >=#" & Report_StartDate.Text & "# AND {TimeClock.DateOut} <= #" & Report_EndDate.Text & "#"
            Else
                report.ReportFormula = "{TimeClock.DateIn} >=#" & Report_StartDate.Text & "# AND {TimeClock.DateOut} <= #" & Report_EndDate.Text & "# AND {TimeClock.UserName} LIKE '" & UserName_TxtBx.Text & "'"
            End If

            Dim reportPrev As New ReportPreview(report)
            Cursor = Cursors.Arrow
            reportPrev.ShowDialog()

        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to report [Time Clock]...")
        Finally : Cursor = Cursors.Arrow
        End Try
    End Sub



#End Region


End Class
