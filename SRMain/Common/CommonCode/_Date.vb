Imports System.DateTime
Imports System.Math
Imports System.Windows.Forms

Public Module _Date

    ' ALL the patterns: 
    ' Date2String = String.Format("{0:dd-MMM-yy}", date2convert)
    '     [0]:    MM/dd/yyyy
    '     [1]:    dddd, dd MMMM yyyy
    '     [2]:    dddd, dd MMMM yyyy HH:mm
    '     [3]:    dddd, dd MMMM yyyy hh:mm tt
    '     [4]:    dddd, dd MMMM yyyy H:mm
    '     [5]:    dddd, dd MMMM yyyy h:mm tt
    '     [6]:    dddd, dd MMMM yyyy HH:mm:ss
    '     [7]:    MM/dd/yyyy HH:mm
    '     [8]:    MM/dd/yyyy hh:mm tt
    '     [9]:    MM/dd/yyyy H:mm
    '     [10]:    MM/dd/yyyy h:mm tt
    '     [11]:    MM/dd/yyyy HH:mm:ss
    '     [12]:    MMMM dd
    '     [13]:    MMMM dd
    '     [14]:    ddd, dd MMM yyyy HH':'mm':'ss 'GMT'
    '     [16]:    yyyy'-'MM'-'dd'T'HH':'mm':'ss
    '     [17]:    HH:mm
    '     [18]:    hh:mm tt
    '     [19]:    H:mm
    '     [20]:    h:mm tt
    '     [21]:    HH:mm:ss
    '     [22]:    yyyy'-'MM'-'dd HH':'mm':'ss'Z'
    '     [23]:    dddd, dd MMMM yyyy HH:mm:ss
    '     [24]:    yyyy MMMM

    Private Sub error_DebugPrint(ByVal routineName As String, ByVal errorDesc As String)
        _Debug.PrintError_(String.Format("_Date.{0}(): {1}", routineName, errorDesc))
    End Sub

    Public Function UTC_TimeSpan(ByVal dateFrom As Date, ByRef utcDiff As System.TimeSpan) As Boolean
        UTC_TimeSpan = False
        Try
            Dim utcNow As Date = DateTime.UtcNow
            utcDiff = utcNow.Subtract(dateFrom) : UTC_TimeSpan = True
            _Debug.Print_(String.Format("[{0}] - [{1}] = [{2}]", dateFrom, utcNow, utcDiff))
            ''
        Catch ex As Exception : error_DebugPrint("UTC_TimeSpan1", ex.Message)
        End Try
        ''
    End Function
    Public Function UTC_TimeSpan(ByVal dateFrom As Date, ByVal dateTo As Date, ByRef utcDiff As System.TimeSpan) As Boolean
        UTC_TimeSpan = False
        Try
            Dim utcNow As Date = dateTo
            utcDiff = utcNow.Subtract(dateFrom) : UTC_TimeSpan = True
            _Debug.Print_(String.Format("[{0}] - [{1}] = [{2}]", dateFrom, utcNow, utcDiff))
            ''
        Catch ex As Exception : error_DebugPrint("UTC_TimeSpan2", ex.Message)
        End Try
        ''
    End Function

    Public Function Diff_(ByVal interval As String, ByVal dateFrom As Date, ByVal dateTo As Date, ByRef timeSpan As Long) As Boolean
        Try
            Diff_ = True '' assume.
            timeSpan = Math.Abs(DateAndTime.DateDiff(interval, dateTo, dateFrom))
            _Debug.Print_(String.Format("[{0}] - [{1}] = [{2}]", dateFrom, dateTo, timeSpan))
            ''
        Catch ex As Exception : error_DebugPrint("Diff_", ex.Message) : Diff_ = False
        End Try
    End Function
    Public Function Diff_(ByVal interval As DateInterval, ByVal dateFrom As Date, ByVal dateTo As Date, ByRef timeSpan As Long) As Boolean
        Try
            Diff_ = True '' assume.
            timeSpan = Math.Abs(DateAndTime.DateDiff(interval, dateTo, dateFrom))
            _Debug.Print_(String.Format("[{0}] - [{1}] = [{2}]", dateFrom, dateTo, timeSpan))
            ''
        Catch ex As Exception : error_DebugPrint("Diff_", ex.Message) : Diff_ = False
        End Try
    End Function
    Public Function Diff_Dates(ByVal dateFrom As Date, ByVal dateTo As Date, ByRef timeSpan As Long) As Boolean
        Try
            Diff_Dates = True '' assume.
            timeSpan = Math.Abs(DateAndTime.DateDiff(DateInterval.Day, dateTo, dateFrom))
            _Debug.Print_(String.Format("[{0}] - [{1}] = [{2}]", dateFrom, dateTo, timeSpan))
            ''
        Catch ex As Exception : error_DebugPrint("Diff_Dates", ex.Message) : Diff_Dates = False
        End Try
    End Function
    Public Function Diff_Dates(ByVal dateFrom As Date, ByVal dateTo As Date) As Long
        Diff_Dates = 0
        Try
            Diff_Dates = Math.Abs(DateAndTime.DateDiff(DateInterval.Day, dateTo, dateFrom))
        Catch ex As Exception : error_DebugPrint("Diff_Dates", ex.Message)
        End Try
    End Function
    Public Function Diff_Minutes(ByVal dateFrom As Date, ByVal dateTo As Date, ByRef timeSpan As Long) As Boolean
        Try
            Diff_Minutes = True '' assume.
            timeSpan = Math.Abs(DateAndTime.DateDiff(DateInterval.Minute, dateTo, dateFrom))
            _Debug.Print_(String.Format("[{0}] - [{1}] = [{2}]", dateFrom, dateTo, timeSpan))
            ''
        Catch ex As Exception : error_DebugPrint("Diff_Minutes", ex.Message) : Diff_Minutes = False
        End Try
    End Function
    Public Function Diff_Minutes(ByVal dateFrom As Date, ByVal dateTo As Date) As Long
        Diff_Minutes = 0
        Try
            Diff_Minutes = DateAndTime.DateDiff(DateInterval.Minute, dateTo, dateFrom)
            ''
        Catch ex As Exception : error_DebugPrint("Diff_Minutes", ex.Message)
        End Try
    End Function

    Public Function Add_(ByVal interval As String, ByVal number2add As Double, ByVal date2add As Date, ByRef dateNew As Date) As Boolean
        Try
            Add_ = True '' assume.
            dateNew = DateAndTime.DateAdd(interval, number2add, date2add)
            _Debug.Print_(String.Format("[{1}] + [{0}] = [{2}]", number2add, date2add, dateNew))
            ''
        Catch ex As Exception : error_DebugPrint("Add_", ex.Message) : Add_ = False
        End Try
    End Function
    Public Function Add_(ByVal interval As DateInterval, ByVal number2add As Double, ByVal date2add As Date, ByRef dateNew As Date) As Boolean
        Try
            Add_ = True '' assume.
            dateNew = DateAndTime.DateAdd(interval, number2add, date2add)
            _Debug.Print_(String.Format("[{1}] + [{0}] = [{2}]", number2add, date2add, dateNew))
            ''
        Catch ex As Exception : error_DebugPrint("Add_", ex.Message) : Add_ = False
        End Try
    End Function
    Public Function Add_Day(ByVal number2add As Double, ByVal date2add As Date, ByRef dateNew As Date) As Boolean
        Try
            Add_Day = True '' assume.
            dateNew = date2add.AddDays(number2add)
            _Debug.Print_(String.Format("[{1}] + [{0}] = [{2}]", number2add, date2add, dateNew))
            ''
        Catch ex As Exception : error_DebugPrint("Add_Day", ex.Message) : Add_Day = False
        End Try
    End Function
    Public Function Add_Minutes(ByVal number2add As Double, ByVal date2add As Date, ByRef dateNew As Date) As Boolean
        Try
            Add_Minutes = True '' assume.
            dateNew = date2add.AddMinutes(number2add)
            '_Debug.Print_(String.Format("[{1}] + [{0}] = [{2}]", number2add, date2add, dateNew))
            ''
        Catch ex As Exception : error_DebugPrint("Add_Minutes", ex.Message) : Add_Minutes = False
        End Try
    End Function
    Public Function Add_Minutes(ByVal number2add As Double, ByVal date2add As Date) As Date
        Add_Minutes = #1/1/1999#
        Try
            Add_Minutes = date2add.AddMinutes(number2add)
            '_Debug.Print_(String.Format("[{1}] + [{0}] = [{2}]", number2add, date2add, dateNew))
            ''
        Catch ex As Exception : error_DebugPrint("Add_Minutes", ex.Message)
        End Try
    End Function

    Function Time_Round(ByVal minutes As Long, ByVal rounding As Long, ByRef roundedMinutes As Long) As Boolean
        Try
            roundedMinutes = -1 '' assume.
            Dim remainMinutes As Long = minutes Mod rounding
            Dim wholeMinutes As Long = minutes - remainMinutes
            Dim percentMinutes As Double = Math.Round(remainMinutes / rounding, 1)
            If minutes < rounding Then
                ' must be minimum, which is rounding value
                roundedMinutes = rounding
            ElseIf percentMinutes >= 0.5 Then
                ' if grater/equal than half then add rounding value  
                roundedMinutes = wholeMinutes + rounding
            Else
                ' else leave the whole minutes 
                roundedMinutes = wholeMinutes
            End If
            Time_Round = (Not -1 = roundedMinutes)
            _Debug.Print_(String.Format("[{0}] round by [{1}] = [{2}]", minutes, rounding, roundedMinutes))
            ''
        Catch ex As Exception : error_DebugPrint("Time_Round", ex.Message) : Time_Round = False
        End Try
    End Function

    Public Function Format_HrsMin(ByVal date2format As Date) As String
        Format_HrsMin = date2format.ToShortTimeString '' assume.
        Try
            If 7 = Format_HrsMin.Length Then
                Format_HrsMin = String.Format("0{0}", Format_HrsMin)
            End If
        Catch ex As Exception : error_DebugPrint("Format_HrsMin", ex.Message)
        End Try
    End Function

    Public Function IsDate_(ByVal date2check As Date) As Boolean
        IsDate_ = False
        Try
            'IsDate_ = (Not "01/01/0001" = _Convert.Date2String(date2check))
            IsDate_ = (date2check.Year > 1900)
        Catch ex As Exception : error_DebugPrint("IsDate_", ex.Message)
        End Try
    End Function
    Public Function IsDate_(ByVal date2check As String) As Boolean
        IsDate_ = False
        Try
            If IsDate(date2check) Then
                IsDate_ = IsDate_(Convert.ToDateTime(date2check))
            End If
        Catch ex As Exception : error_DebugPrint("IsDate_", ex.Message)
        End Try
    End Function

    Public Function Extract_Time(ByVal d As Date) As String
        Extract_Time = "00:00:00"
        Try
            Extract_Time = d.ToString("T")
        Catch ex As Exception : error_DebugPrint("Extract_Time", ex.Message)
        End Try
    End Function
    Public Function Extract_Date(ByVal d As Date) As Date
        Extract_Date = DateTime.Today
        Try
            Extract_Date = d.ToString("d")
        Catch ex As Exception : error_DebugPrint("Extract_Date", ex.Message)
        End Try
    End Function

    Public Function Get_Date_FromToday(ByVal dayofweek As Double) As Date
        ' find date: the start of the week
        Dim startOfWeek As Date = DateTime.Today.AddDays(-DateTime.Today.DayOfWeek)
        ' now we can figure what date is passed to us:
        Dim actualDate As Date = startOfWeek.AddDays(dayofweek)
        If actualDate < DateTime.Today Then
            ' this date is from next week
            Get_Date_FromToday = actualDate.AddDays(7)
        Else
            Get_Date_FromToday = actualDate
        End If
        Try
        Catch ex As Exception : error_DebugPrint("Get_Date_FromToday", ex.Message)
        End Try
    End Function

    Public Function MaskedTextBox_ReplaceSpacesWith(ByVal mtbox As MaskedTextBox, Optional ByVal whatChar As String = "0") As String
        MaskedTextBox_ReplaceSpacesWith = mtbox.Text '' assume.
        Try
            Dim cntr As New MaskedTextBox
            cntr.Mask = mtbox.Mask
            cntr.TextMaskFormat = MaskFormat.ExcludePromptAndLiterals
            cntr.Text = mtbox.Text
            If cntr.Text.Length = 0 Then
                ' Empty
                MaskedTextBox_ReplaceSpacesWith = String.Empty
            ElseIf cntr.Text.Length = 1 Then
                ' Month's first digit could be only 0 or 1
                If Val(cntr.Text) > 1 Then
                    MaskedTextBox_ReplaceSpacesWith = cntr.Text.Insert(0, "0")
                End If
            ElseIf cntr.Text.Length = 3 Then
                ' Day's first digit could be only 0, 1, 2, or 3
                If Val(_Controls.Right(cntr.Text, 1)) > 3 Then
                    MaskedTextBox_ReplaceSpacesWith = cntr.Text.Insert(2, "0")
                End If
            ElseIf cntr.Text.Length = 2 Or cntr.Text.Length = 4 Then
                ' Month
                MaskedTextBox_ReplaceSpacesWith = cntr.Text.Replace(" ", whatChar)
            ElseIf cntr.Text.Length = 6 Then
                ' Year
                Dim year2digits As Integer = Val(_Controls.Right(cntr.Text, 2))
                If Not 19 = year2digits And Not 20 = year2digits Then
                    ' user has entered last to digits of the end of the year...
                    ' insert the Century digits:
                    If 2000 + year2digits > DateTime.Today.Year Then
                        ' last century
                        MaskedTextBox_ReplaceSpacesWith = cntr.Text.Insert(4, "19") ' 20th century
                    Else
                        ' this century
                        MaskedTextBox_ReplaceSpacesWith = cntr.Text.Insert(4, "20") ' 20th century
                    End If
                End If
            End If
            cntr.Dispose()
        Catch ex As Exception : error_DebugPrint("MaskedTextBox_ReplaceSpacesWith", ex.Message)
        End Try
    End Function

End Module
