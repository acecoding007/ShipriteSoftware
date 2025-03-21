Imports System.ComponentModel
Imports System.Runtime.CompilerServices
Imports System.Windows.Controls.Primitives
Imports System.Windows.Media

Public Class CommonWindow
    Inherits Window
    Implements INotifyPropertyChanged

    Private _winListPointer As Integer = -1

    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
    Public Sub NotifyPropertyChanged(<CallerMemberName> Optional propertyName As String = "")
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
    End Sub

    Public Property winListPointer As Integer
        Get
            Return _winListPointer
        End Get
        Set(value As Integer)
            If value < 0 Then
                value = 0
            End If
            _winListPointer = value
        End Set
    End Property

    Public Sub New()

        MyBase.New()

        Try

            CommonWindowStack.PushWindowList(Me)
            winListPointer = CommonWindowStack.windowListPointer

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try

    End Sub

    Public Sub New(ByVal callingWindow As CommonWindow)

        MyBase.New()

        Try

            Me.Owner = callingWindow
            callingWindow.ShowInTaskbar = False  'prevents the calling windows from being accessible


            Me.WindowState = callingWindow.WindowState
            Me.Height = callingWindow.Height 'ActualHeight
            Me.Width = callingWindow.Width 'ActualWidth
            Me.Left = callingWindow.Left
            Me.Top = callingWindow.Top
            Me.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight
            Me.MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth

            CommonWindowStack.PushWindowList(Me)
            winListPointer = CommonWindowStack.windowListPointer

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try

    End Sub

    ' Using this procedure results in wanting to exit the program after clicking <Back> when called from a window opened via ShowDialog.
    ' Windows opened via ShowDialog can't be hidden using .Hide().
    ' Window(1) opened via ShowDialog > Window(2) opened via Show which calls Window(1).Hide() > Window(1) exits our of Modal pause and continues execution > this results in the WindowStack losing Window(1) > this results in GoBack looking to exit the program since there are no longer previous windows in the WindowStack.
    ' Hide this Sub from the rest of the program for now.
    Private Overloads Sub Show(ByVal callingWindow As CommonWindow)

        Try
            Dim secsTimeout As Double = 5
            Dim endTime As Long = DateAdd(DateInterval.Second, secsTimeout, Date.Now).Ticks

            MyBase.Show()
            Do Until Me.IsVisible Or Date.Now.Ticks > endTime ' wait until window is actually shown before hiding callingWindow
                System.Windows.Forms.Application.DoEvents()
            Loop
            callingWindow.Hide()

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try

    End Sub

    Public Overloads Sub ShowDialog(ByVal callingWindow As CommonWindow)

        Try

            Me.WindowStartupLocation = WindowStartupLocation.Manual
            callingWindow.WindowState = WindowState.Minimized
            callingWindow.ShowInTaskbar = False
            'Dim holdOpacity As Double = callingWindow.Opacity
            'callingWindow.Opacity = 0.5
            'Me.Topmost = True
            Me.ShowDialog()
            'callingWindow.Opacity = holdOpacity
            callingWindow.ShowInTaskbar = True
            CommonWindowStack.WindowSwitch(False, False,, Me)
            CommonWindowStack.windowListPointer = callingWindow.winListPointer

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try

    End Sub

    Private Sub Window_Closed(sender As Object, e As EventArgs) Handles Me.Closed

        Try

            If Me.winListPointer > 0 Then
                CommonWindowStack.RemoveWindow(Me.winListPointer)
            End If

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try

    End Sub

    Public Sub Window_Initialized(sender As Object, e As EventArgs)

    End Sub

    Public Sub Window_Loaded(sender As Object, e As RoutedEventArgs)

    End Sub

    Public Sub Grid_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs)
        Try

            Me.DragMove()

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub

    Public Sub MinimizeButton_Click(sender As Object, e As RoutedEventArgs)
        Try

            Me.WindowState = WindowState.Minimized

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub

    Public Sub MaximizeButton_Click(sender As Object, e As RoutedEventArgs)
        Try

            If WindowState = WindowState.Normal Then

                WindowState = WindowState.Maximized

            ElseIf WindowState = WindowState.Maximized Then

                WindowState = WindowState.Normal

                Me.Width = MinWidth
                Me.Height = MinHeight

            End If

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub

    Public Sub CloseButton_Click(sender As Object, e As RoutedEventArgs)

        Try
            ' play sound


            CheckAutoBackup()

            ' CloseButton should allow user to close SR at any time.
            If MessageBox.Show("Are you sure you want to exit?", "SHIPRITE", Forms.MessageBoxButtons.OKCancel, Forms.MessageBoxIcon.Information) = System.Windows.Forms.DialogResult.OK Then

                My.Settings.Window_Height = Me.Height
                My.Settings.Window_Width = Me.Width
                If Me.WindowState = WindowState.Maximized Then
                    My.Settings.Window_IsMaximized = True
                Else
                    My.Settings.Window_IsMaximized = False
                End If

                My.Settings.Save()

                Application.Current.Shutdown()
            End If

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try

    End Sub

    Public Sub HomeButton_Click(sender As Object, e As RoutedEventArgs)
        Try



            'ShowMainWindow(Me)
            'Dim win As New MainWindow
            'win.Show(Me)
            Dim Index As Integer = CommonWindowStack.windowList.FindIndex(Function(x As CommonWindow) x.Name = "Main_Window")
            CommonWindowStack.windowList(Index).Show()

            Me.Close()

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub

    Public Sub RefreshButton_Click(sender As Object, e As RoutedEventArgs)
        Try
            ' play sound


            ' start new instance at same spot in windowArray
            CommonWindowStack.WindowListPointerBack()
            Dim myWindow As CommonWindow = System.Windows.Window.GetWindow(sender) ' gets called Window
            Dim win As CommonWindow = Activator.CreateInstance(myWindow.GetType) ' creates new initialization of called Window

            win.ShowDialog()

            ' close current
            Me.Close()

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub

    Public Sub BackButton_Click(sender As Object, e As RoutedEventArgs)
        Try


            If CommonWindowStack.WindowListPointerBack Then
                CommonWindowStack.WindowSwitch(False)
            Else
                If CommonWindowStack.windowListPointer = 0 Then
                    CheckAutoBackup()

                    If MessageBox.Show("Are you sure you want to exit?", "SHIPRITE", Forms.MessageBoxButtons.OKCancel, Forms.MessageBoxIcon.Information) = System.Windows.Forms.DialogResult.OK Then
                        Application.Current.Shutdown()
                    End If
                End If
            End If

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub

    Public Sub ForwardButton_Click(sender As Object, e As RoutedEventArgs)

        Try


            If CommonWindowStack.WindowListPointerForward() Then
                CommonWindowStack.WindowSwitch(True)
            End If

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try

    End Sub

    Private Sub CommonWindow_ContentRendered(sender As Object, e As EventArgs) Handles Me.ContentRendered

        Me.Activate()

    End Sub

    ''' <summary>
    ''' TextBox control select all text function for MouseDoubleClick, GotKeyboardFocus events used in conjunction with TextBox_SelectivelyIgnoreMouseButton() function.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Public Sub TextBox_SelectAllText(sender As Object, e As RoutedEventArgs)
        Dim tb As TextBox = TryCast(sender, TextBox)

        If tb IsNot Nothing Then
            tb.SelectAll()
        End If
    End Sub

    ''' <summary>
    ''' TextBox control select all text function for PreviewMouseLeftButtonDown event used in conjunction with TextBox_SelectAllText() function.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Public Sub TextBox_SelectivelyIgnoreMouseButton(sender As Object, e As MouseButtonEventArgs)
        Dim tb As TextBox = TryCast(sender, TextBox)

        If tb IsNot Nothing Then
            If Not tb.IsKeyboardFocusWithin Then
                e.Handled = True
                tb.Focus()
            End If
        End If
    End Sub

    Private Sub CheckAutoBackup()
        If GetPolicyData(gReportsDB, "BackupAutomatic", "false") Then
            If CDate(GetPolicyData(gReportsDB, "Backup_LastDate", "1/1/1900")).ToShortDateString <> DateTime.Now.ToShortDateString Then
                If MsgBox("Daily backup was not done today." & vbCrLf & "Would you like to backup your software now?", vbYesNo + vbQuestion, "SHIPRITE Daily Auto Backup") = MsgBoxResult.Yes Then
                    Dim win As New Backup(Me, True)
                    win.ShowDialog(Me)
                End If
            End If
        End If
    End Sub
End Class

Public Class CommonWindowStack

    Public Shared windowList As New List(Of CommonWindow)
    Public Shared windowListPointer As Integer = -1

    Public Shared Sub PushWindowList(CurrentWindow As CommonWindow)
        Try

            If windowList.Count - 1 > windowListPointer Then
                ClearWindowsAtPointer(False)
            End If
            windowList.Add(CurrentWindow) ' add at end
            windowListPointer = windowList.Count - 1 ' current pointer is at end

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub

    Public Shared Sub PopWindowList()
        Try

            windowList.RemoveAt(windowList.Count - 1) ' remove at end
            windowListPointer = windowList.Count - 1

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try

    End Sub

    Public Shared Sub RemoveWindow(ByVal index As Integer)

        Try

            If index >= 0 And index <= windowList.Count - 1 Then ' within bounds
                windowList.RemoveAt(index)
                For i As Integer = index To windowList.Count - 1
                    windowList(i).winListPointer -= 1
                Next i
                If index = windowListPointer Then
                    windowListPointer -= 1
                End If
            End If

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try

    End Sub

    Public Shared Sub ClearWindowsAtPointer(Optional ByVal isIncludeCurrent As Boolean = True)
        Try

            If windowList.Count - 1 >= windowListPointer Then
                Dim start As Integer = windowListPointer
                If isIncludeCurrent Then
                    start = windowListPointer
                Else
                    start = windowListPointer + 1
                End If
                If start = 0 Then
                    start += 1
                End If
                windowList(start).Close() ' will remove start and any after from list

            End If

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try

    End Sub

    Public Shared Sub ClearWindowList()
        Try

            windowList.Clear()
            windowListPointer = 0
            windowList(windowListPointer) = Application.Current.MainWindow

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub

    Public Shared Function WindowListPointerBack() As Boolean
        Try
            windowListPointer -= 1
            If windowListPointer < 0 Then
                windowListPointer = 0
                Return False
            End If

        Catch ex As Exception

            MessageBox.Show(Err.Description)
            Return False

        End Try

        Return True

    End Function

    Public Shared Function WindowListPointerForward(Optional ByVal isExpandArray As Boolean = False) As Boolean
        Try

            windowListPointer += 1
            If windowList.Count > 0 Then
                If windowListPointer > windowList.Count - 1 Then
                    windowListPointer = windowList.Count - 1
                    Return False
                End If
            End If

        Catch ex As Exception

            MessageBox.Show(Err.Description)
            Return False

        End Try

        Return True

    End Function

    Public Shared Sub WindowSwitch(ByVal isForward As Boolean, Optional ByVal isSwitchVisibility As Boolean = True, Optional ByVal isClosePrevious As Boolean = False, Optional ByVal previousWindow As CommonWindow = Nothing)
        Try

            Dim prevWindowArrayPointer As Integer = -1
            Dim isPrevPointerValid As Boolean = False

            If isForward Then
                isClosePrevious = False ' can't close previous if forward as ownership will cause current to close too
                If windowListPointer > 0 Then
                    prevWindowArrayPointer = windowListPointer - 1
                End If
            Else ' isBackward
                If windowListPointer < windowList.Count - 1 Then
                    prevWindowArrayPointer = windowListPointer + 1
                End If
            End If

            If windowListPointer >= 0 And windowListPointer <= windowList.Count - 1 Then ' valid pointer
                If prevWindowArrayPointer > -1 Then
                    If isClosePrevious Then
                        ClearWindowsAtPointer(False)
                    Else
                        If prevWindowArrayPointer >= 0 And prevWindowArrayPointer <= windowList.Count - 1 Then ' valid pointer
                            isPrevPointerValid = True

                            windowList(windowListPointer).Width = windowList(prevWindowArrayPointer).Width 'ActualWidth
                            windowList(windowListPointer).Height = windowList(prevWindowArrayPointer).Height 'ActualHeight
                            windowList(windowListPointer).Left = windowList(prevWindowArrayPointer).Left
                            windowList(windowListPointer).Top = windowList(prevWindowArrayPointer).Top
                            windowList(windowListPointer).WindowState = windowList(prevWindowArrayPointer).WindowState
                        End If
                    End If
                ElseIf previousWindow IsNot Nothing Then
                    windowList(windowListPointer).WindowState = previousWindow.WindowState
                    windowList(windowListPointer).Width = previousWindow.Width 'ActualWidth
                    windowList(windowListPointer).Height = previousWindow.Height 'ActualHeight
                    windowList(windowListPointer).Left = previousWindow.Left
                    windowList(windowListPointer).Top = previousWindow.Top
                End If

                If isSwitchVisibility Then
                    If Not windowList(windowListPointer).IsVisible Then
                        windowList(windowListPointer).Show()
                    End If
                    If isPrevPointerValid Then
                        Dim secsTimeout As Double = 5
                        Dim endTime As Long = DateAdd(DateInterval.Second, secsTimeout, Date.Now).Ticks

                        Do Until windowList(windowListPointer).IsVisible Or Date.Now.Ticks > endTime ' wait until window is actually shown before hiding callingWindow
                            System.Windows.Forms.Application.DoEvents()
                        Loop
                        windowList(prevWindowArrayPointer).Hide()
                    End If
                End If
            End If

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try

    End Sub

    Public Shared Sub ShowMainWindow(ByVal callingWindow As CommonWindow)

        Try

            If windowList.Count > 0 Then
                If callingWindow IsNot Nothing Then
                    windowList(0).Width = callingWindow.Width
                    windowList(0).Height = callingWindow.Height
                    windowList(0).Left = callingWindow.Left
                    windowList(0).Top = callingWindow.Top
                    windowList(0).WindowState = callingWindow.WindowState
                End If
                windowList(0).Show()

                If windowListPointer > 0 And windowListPointer <= windowList.Count - 1 Then
                    windowList(windowListPointer).Hide()
                End If

            End If
            windowListPointer = 0 ' set to main window

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try

    End Sub

End Class

Public Class CommonListView
    Inherits ListView

    Protected Overrides Sub OnMouseDoubleClick(e As MouseButtonEventArgs)
        If Not IsDoubleClick_OnListViewScrollbar(e) Then
            MyBase.OnMouseDoubleClick(e)
        End If
    End Sub

    Private Function IsDoubleClick_OnListViewScrollbar(e As MouseButtonEventArgs) As Boolean
        Dim oSrc As DependencyObject = CType(e.OriginalSource, DependencyObject)
        Dim pSrc As DependencyObject = Media.VisualTreeHelper.GetParent(oSrc)
        Dim oSrcType As Type = oSrc.GetType()
        Dim pSrcType As Type = pSrc.GetType()
        ''
        If oSrcType = GetType(Border) Then
            If pSrcType = GetType(RepeatButton) Then
                Return True
            End If
        ElseIf oSrcType = GetType(Rectangle) Then
            If pSrcType = GetType(RepeatButton) OrElse pSrcType = GetType(Thumb) Then
                Return True
            End If
        ElseIf oSrcType = GetType(Path) Then
            If pSrcType = GetType(ContentPresenter) Then
                Return True
            End If
        ElseIf oSrcType = GetType(ScrollViewer) Then ' empty space underneath
            Return True
        End If
        ''
        Return False
    End Function
End Class

Public Class NumberTextBox
    Inherits TextBox

    Protected Overrides Sub OnPreviewTextInput(e As TextCompositionEventArgs)
        If isInputNumber(e) Then
            MyBase.OnPreviewTextInput(e)
        Else
            e.Handled = True
        End If

    End Sub

    Private Function isInputNumber(e As TextCompositionEventArgs)
        Dim allowedchars As String = "0123456789.-"
        If allowedchars.IndexOf(CChar(e.Text)) > -1 Then
            Return True
        Else
            Return False
        End If
    End Function

End Class

