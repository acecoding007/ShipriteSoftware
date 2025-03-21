Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Drawing.Printing
Imports System.Runtime.CompilerServices

Public Class PrinterSetup
    Inherits CommonWindow

    Public Selected_Printer_List As List(Of String) 'Keeps track of the selected printers for each printer type

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

    Private Sub PrinterSetup_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded

        Dim buf As String

        Load_Printer_List()

        'Prime list of selected printers
        Selected_Printer_List = New List(Of String)
        For count As Integer = 0 To PrinterType_List.Items.Count - 1
            Dim dbValue As String = ""
            Dim lvItem As ListBoxItem = TryCast(PrinterType_List.Items.Item(count), ListBoxItem)
            If lvItem IsNot Nothing Then
                dbValue = GetPolicyData(gReportsDB, lvItem.Tag.ToString)
            End If
            If dbValue = "" Or dbValue.ToLower = "(none)" Then
                Selected_Printer_List.Add("")
            Else
                If Printer_List.Items.IndexOf(dbValue) >= 0 Then
                    ' found
                    Selected_Printer_List.Add(dbValue)
                Else
                    ' not found
                    Selected_Printer_List.Add("")
                End If
            End If
        Next

        PrinterType_List.SelectedIndex = 0
        Receipt_Settings_Border.Visibility = Visibility.Hidden

        buf = GetPolicyData(gReportsDB, Receipt_Fonts_ComboBox.Tag)
        For Each cItem As ComboBoxItem In Receipt_Fonts_ComboBox.Items
            If cItem.Content.ToString = buf Then
                Receipt_Fonts_ComboBox.Text = buf
                Exit For
            End If
        Next

        buf = GetPolicyData(gReportsDB, Receipt_FontSize_ComboBox.Tag)
        For Each cItem As ComboBoxItem In Receipt_FontSize_ComboBox.Items
            If cItem.Content.ToString = buf Then
                Receipt_FontSize_ComboBox.Text = buf
                Exit For
            End If
        Next

        buf = GetPolicyData(gReportsDB, Receipt_CodeToOpenDrawer_TextBox.Tag)
        Receipt_CodeToOpenDrawer_TextBox.Text = buf

        For Each currentTab As TabItem In Setup_Tab.Items
            currentTab.Visibility = Visibility.Collapsed
        Next

        Scale_LoadSavedScale()

    End Sub

    Private Sub Load_Printer_List()
        Dim pkInstalledPrinters As String

        Printer_List.Items.Add("(none)")

        ' Find all printers installed
        For Each pkInstalledPrinters In
            PrinterSettings.InstalledPrinters
            Printer_List.Items.Add(pkInstalledPrinters)
            Printer_List.Items.Refresh()
        Next pkInstalledPrinters

    End Sub


    Private Sub DevicesAndPrinters_Button_Click(sender As Object, e As RoutedEventArgs) Handles DevicesAndPrinters_Button.Click
        'Opens "Devices and Printers" in Windows Control Panel
        Dim startInfo As New ProcessStartInfo("control.exe")
        startInfo.WindowStyle = ProcessWindowStyle.Minimized
        startInfo.Arguments = "printers"
        Process.Start(startInfo)
    End Sub



    Private Sub PrinterType_List_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles PrinterType_List.SelectionChanged

        If PrinterType_List.SelectedIndex = -1 Then
            Exit Sub
        End If

        If PrinterType_List.SelectedIndex = 1 Then
            'Receipt Printer selected, Display additional receipt settings.
            Receipt_Settings_Border.Visibility = Visibility.Visible
        Else
            Receipt_Settings_Border.Visibility = Visibility.Hidden
        End If


        If Selected_Printer_List.Item(PrinterType_List.SelectedIndex) = "" Then
            Printer_List.UnselectAll()
        Else
            Printer_List.SelectedItem = Selected_Printer_List.Item(PrinterType_List.SelectedIndex)
        End If

    End Sub


    Private Sub Printer_List_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles Printer_List.SelectionChanged
        If Printer_List.SelectedItem <> Nothing Then
            Selected_Printer_List.Item(PrinterType_List.SelectedIndex) = Printer_List.SelectedItem
        End If
    End Sub

    Private Sub Peripheral_List_GotFocus(sender As Object, e As RoutedEventArgs) Handles Peripheral_List.GotFocus
        PrinterType_List.UnselectAll()
    End Sub

    Private Sub PrinterType_List_GotFocus(sender As Object, e As RoutedEventArgs) Handles PrinterType_List.GotFocus
        'display printer tab
        Setup_Tab.SelectedIndex = 0

        Peripheral_List.UnselectAll()
    End Sub

    Private Sub Peripheral_List_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles Peripheral_List.SelectionChanged
        If Peripheral_List.SelectedIndex = -1 Then
            Exit Sub
        End If

        Setup_Tab.SelectedIndex = Peripheral_List.SelectedIndex + 1

    End Sub

    Private Sub SaveButton_Click(sender As Object, e As RoutedEventArgs) Handles SaveButton.Click

        Try

            Dim ret As Integer = 0

            For count As Integer = 0 To PrinterType_List.Items.Count - 1
                Dim dbValue As String = ""
                Dim lvItem As ListBoxItem = TryCast(PrinterType_List.Items.Item(count), ListBoxItem)
                dbValue = Selected_Printer_List(count).ToString
                If dbValue = "" Then
                    dbValue = "(none)"
                End If
                If lvItem IsNot Nothing Then
                    ret = UpdatePolicy(gReportsDB, lvItem.Tag.ToString, dbValue)
                End If
            Next

            ret = UpdatePolicy(gReportsDB, Receipt_Fonts_ComboBox.Tag, Receipt_Fonts_ComboBox.Text)
            ret = UpdatePolicy(gReportsDB, Receipt_FontSize_ComboBox.Tag, Receipt_FontSize_ComboBox.Text)
            ret = UpdatePolicy(gReportsDB, Receipt_CodeToOpenDrawer_TextBox.Tag, Receipt_CodeToOpenDrawer_TextBox.Text)

            If Scale_Selected IsNot Nothing AndAlso Scale_Selected.Save_Scale() Then
                Scale_Saved = New Scale.BaseScale(Scale_Selected)
            End If

            MessageBox.Show("Settings saved successfully!", "Printer Setup", MessageBoxButton.OK, MessageBoxImage.Information)

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try

    End Sub

    Private Sub PrintTestPage_Button_Click(sender As Object, e As RoutedEventArgs) Handles PrintTestPage_Button.Click

        Try

            Dim pSettings As PrintHelper = Nothing
            Dim sText As String = vbCrLf & vbCrLf & "TEST PRINT PAGE - Line 1"
            sText &= vbCrLf & vbCrLf & "TEST PRINT PAGE - Line 2"
            sText &= vbCrLf & vbCrLf & "TEST PRINT PAGE - Line 3"
            sText &= vbCrLf & vbCrLf & "TEST PRINT PAGE - Line 4"
            Dim sSelectedPrinter As String = ""

            If PrinterType_List.SelectedIndex > -1 Then
                sSelectedPrinter = Selected_Printer_List.Item(PrinterType_List.SelectedIndex)
                If sSelectedPrinter.ToLower.Trim = "(none)" Then
                    sSelectedPrinter = ""
                End If
            End If

            If sSelectedPrinter.Trim.Length > 0 Then
                If PrinterType_List.SelectedIndex = 1 Then ' receipt options
                    pSettings = New PrintHelper
                    If Receipt_Fonts_ComboBox.Text.Length > 0 Then
                        pSettings.PrintFontFamilyName = Receipt_Fonts_ComboBox.Text
                    End If
                    If Receipt_FontSize_ComboBox.Text.Length > 0 Then
                        pSettings.PrintFontSize = Receipt_FontSize_ComboBox.Text
                    End If
                End If
                _PrintReceipt.Print_FromText(sText, sSelectedPrinter, pSettings)
            End If

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try

    End Sub

#Region "Scale Settings"

    ' Scale Models object collection
    Public ReadOnly Property Scale_ModelList As ObservableCollection(Of Scale.BaseScale)
        Get
            'Return _ScaleModelList
            Dim modelList As New ObservableCollection(Of Scale.BaseScale)
            modelList.Add(New Scale.BaseScale(""))
            For Each s As String In Scale.ScaleModels.List
                modelList.Add(New Scale.BaseScale(s))
            Next
            Return New ObservableCollection(Of Scale.BaseScale)(modelList.OrderBy(Function(x) x.Model))
        End Get
    End Property

    ' Scale object saved in the database
    Private Property Scale_Saved As Scale.BaseScale

    ' Scale object currently selected in form
    Private _Scale_Selected As Scale.BaseScale
    Public Property Scale_Selected As Scale.BaseScale
        Get
            Return _Scale_Selected
        End Get
        Set(value As Scale.BaseScale)
            _Scale_Selected = value
            NotifyPropertyChanged()
        End Set
    End Property

    Public Class Scale_Setting
        Implements INotifyPropertyChanged

        Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
        Public Sub NotifyPropertyChanged(<CallerMemberName> Optional propertyName As String = "")
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
        End Sub

        Public ReadOnly Property List As ObservableCollection(Of String)
        Private _Visibility As Visibility
        Public Property Visibility As Visibility
            Get
                Return _Visibility
            End Get
            Set(value As Visibility)
                _Visibility = value
                NotifyPropertyChanged()
            End Set
        End Property

        Public Sub New()
            List = Nothing
        End Sub
        Public Sub New(settingList As IEnumerable(Of String))
            List = New ObservableCollection(Of String)(settingList)
        End Sub
    End Class

    Public Property Scale_Settings As New Scale_Setting()
    Public Property ScaleSetting_Ports As New Scale_Setting(My.Computer.Ports.SerialPortNames)
    Public Property ScaleSetting_Speed As New Scale_Setting(Scale.SerialScale.ScaleSpeed.List)
    Public Property ScaleSetting_Parity As New Scale_Setting(Scale.SerialScale.ScaleParity.List)
    Public Property ScaleSetting_DataBits As New Scale_Setting(Scale.SerialScale.ScaleDataBit.List)
    Public Property ScaleSetting_StopBits As New Scale_Setting(Scale.SerialScale.ScaleStopBit.List)
    Public Property ScaleSetting_Weight As New Scale_Setting()

    Private Sub Scale_LoadSavedScale()

        Scale_Saved = New Scale.BaseScale()
        Scale_Saved.Load_ScaleFromPolicy(False)

        If Scale_Saved.Model_IsValid Then
            Scale_Selected = Scale_ModelList.First(Function(x) x.Model = Scale_Saved.Model) ' selected scale obj loaded with default settings from list - selected in list in window
            Scale_Selected = New Scale.BaseScale(Scale_Saved) ' selected scale obj updated with settings from saved scale obj - updated in settings in window
        Else
            Scale_Saved = Nothing
            Scale_Selected = Scale_ModelList.Item(0) ' none
        End If

    End Sub

    Private Sub Scale_SetSettingsVisibility()

        Scale_Settings.Visibility = Visibility.Visible

        If Scale_Selected.Type = Scale.ScaleType.Serial Then
            ScaleSetting_Ports.Visibility = Visibility.Visible
            ScaleSetting_Speed.Visibility = Visibility.Visible
            ScaleSetting_Parity.Visibility = Visibility.Visible
            ScaleSetting_DataBits.Visibility = Visibility.Visible
            ScaleSetting_StopBits.Visibility = Visibility.Visible
        Else
            ScaleSetting_Ports.Visibility = Visibility.Collapsed
            ScaleSetting_Speed.Visibility = Visibility.Collapsed
            ScaleSetting_Parity.Visibility = Visibility.Collapsed
            ScaleSetting_DataBits.Visibility = Visibility.Collapsed
            ScaleSetting_StopBits.Visibility = Visibility.Collapsed
        End If
        ScaleSetting_Weight.Visibility = Visibility.Visible

    End Sub

    Private Sub Scale_Selected_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)

        If Scale_Selected Is Nothing Then
            ' shouldn't be nothing - set to (none)
            Scale_Selected = Scale_ModelList.Item(0) ' none
        End If

        If Scale_Selected.Model = Scale.ScaleModels.NONE Then
            Scale_Settings.Visibility = Visibility.Hidden
        Else
            Scale_SetSettingsVisibility()
        End If

    End Sub

    Private Sub Scale_Test_Click(sender As Object, e As RoutedEventArgs)

        If Scale_Selected Is Nothing Then
            Exit Sub
        ElseIf Scale_Selected.Model = Scale.ScaleModels.NONE Then
            MessageBox.Show("No Scale selected to test. Please select a scale.", "Scale Test", MessageBoxButton.OK, MessageBoxImage.Warning)
        End If

        Dim errDesc As String = ""
        Dim wt As String = Scale_Selected.Get_Weight(errDesc) 'Selected_Scale.Get_Weight(errDesc)
        MessageBox.Show("Scale Reading: " & wt & vbCrLf &
                        "Scale Display: " & Scale_Selected.ScaleDisplay & 'Selected_Scale.ScaleDisplay &
                        IIf(errDesc.Length > 0, vbCrLf & vbCrLf & "Scale Errors: " & errDesc, ""),
                        "Scale Test", MessageBoxButton.OK, MessageBoxImage.Information)

    End Sub


#End Region

End Class
