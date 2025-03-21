
Imports System.Windows.Media.Animation

Public Class SplashScreen

    Private WithEvents dblAnimation As DoubleAnimation
    Private isAnimationCompleted As Boolean
    Private SplashScreen_StartupTask As Task

    Private Sub SplashScreen_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded

        'SplashScreen_Window.Topmost = True
        Application.Current.MainWindow.Visibility = Visibility.Hidden
        System.Windows.Forms.Application.DoEvents()
        SplashScreen_StartupTask = Nothing

    End Sub

    Private Sub SplashScreen_ContentRendered(sender As Object, e As EventArgs) Handles Me.ContentRendered

        Try

            Startup_Procedures()

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try

    End Sub

    Private Sub SplashScreen_ProgressBar_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles SplashScreen_ProgressBar.ValueChanged
        Try
            If SplashScreen_ProgressBar.Value = 100 Then
                Application.Current.MainWindow.Visibility = Visibility.Visible
                Me.Close()
            End If
        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub

    Private Sub ProgressBar_AnimationStart(ByRef theProgressBar As ProgressBar, ByVal duration_secs As Double, ByVal animation_toValue As Double)

        Try

            isAnimationCompleted = False
            Dim duration As Duration = New Duration(TimeSpan.FromSeconds(duration_secs))
            dblAnimation = New DoubleAnimation(animation_toValue, duration)
            theProgressBar.BeginAnimation(ProgressBar.ValueProperty, dblAnimation)

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try

    End Sub

    Private Sub ProgressBar_AnimationWaitForCompleted(ByVal secsTimeout As Double)

        Try

            Dim endTime As Long = DateAdd(DateInterval.Second, secsTimeout, Date.Now).Ticks

            Do Until isAnimationCompleted Or Date.Now.Ticks > endTime
                System.Windows.Forms.Application.DoEvents()
            Loop

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try

    End Sub

    Private Sub ProgressBar_AnimationCompleted(sender As Object, e As EventArgs) Handles dblAnimation.Completed

        Try

            isAnimationCompleted = True

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try

    End Sub

    Private Sub Startup_Procedures()

        Try

            Run_StartupProcedure("", Sub() ShipriteStartup.LoadGlobalValues(Loading_Label, Me), 0.5, 10)
            Run_StartupProcedure("Checking for Database Update...", Sub() ShipriteStartup.CheckForDatabaseUpdate(), 0.5, 20) ' Check for full database schema replacement
            Run_StartupProcedure("Processing Special Updates...", Sub() ShipriteStartup.Special_Updates(), 1, 30) ' One Time Use Utilities
            Run_StartupProcedure("Loading Carrier Setup...", Sub() ShipriteStartup.Load_ShipRite_CarrierSetup(), 0.5, 40)
            Run_StartupProcedureAsTask("Loading Master Shipping Table...", Sub() ShipriteStartup.Load_MasterShippingTable(), 1, 50)
            If GetPolicyData(gShipriteDB, "Enable_Pricing_Matrix", "False") Then Run_StartupProcedure("Loading Pricing Matrix...", Sub() ShipriteStartup.Cache_PricingMatrix(), 0.5, 50)
            Run_StartupProcedureAsTask("Caching Zone Tables...", Sub() ShipriteStartup.CacheZoneTables(), 1.5, 60)
            Run_StartupProcedureAsTask("Caching Service Tables...", Sub() ShipriteStartup.CacheServiceTables(), 1.5, 70)
            Run_StartupProcedure("Caching Profit Ranges...", Sub() ShipriteStartup.CacheProfitRanges(), 0.5, 80)
            Run_StartupProcedure("Checking Tickler Notices...", Sub() Tickler.Check_Tickler_Notices(), 0.5, 90)
            Run_StartupProcedure("Syncing With The Cloud...", Nothing, 1, 100)

        Catch ex As Exception
            MsgBox(Err.Description & vbCrLf & vbCrLf & "STARTUP FAILED...terminating")
            End

        End Try

    End Sub

    Private Sub Run_StartupProcedure(ByVal label_LoadingContent As String, ByVal action As Action, ByVal duration_secs As Double, ByVal animation_toValue As Double, Optional ByVal secs_Timeout As Double = 30)

        Try

            ProgressBar_AnimationStart(SplashScreen_ProgressBar, duration_secs, animation_toValue)
            Loading_Label.Content = label_LoadingContent
            If action IsNot Nothing Then
                action()
            End If
            ProgressBar_AnimationWaitForCompleted(secs_Timeout)

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try

    End Sub

    Private Sub Run_StartupProcedureAsTask(ByVal label_LoadingContent As String, ByVal action As Action, ByVal duration_secs As Double, ByVal animation_toValue As Double, Optional ByVal secs_Timeout As Double = 30)

        Try

            If action Is Nothing Then
                Run_StartupProcedure(label_LoadingContent, action, duration_secs, animation_toValue, secs_Timeout)
            Else
                Run_StartupProcedure(label_LoadingContent, Sub() Run_Task(action), duration_secs, animation_toValue, secs_Timeout)
            End If

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try

    End Sub

    Private Sub Run_Task(ByVal action As Action, Optional ByVal isStartTask As Boolean = False)

        Try

            If SplashScreen_StartupTask Is Nothing Or isStartTask Then
                SplashScreen_StartupTask = New Task(action)
                SplashScreen_StartupTask.Start()
            Else
                Dim new_action As Action(Of Task) = Sub() action()
                SplashScreen_StartupTask.ContinueWith(new_action)
            End If

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try

    End Sub

End Class
