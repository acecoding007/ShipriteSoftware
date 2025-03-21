Imports System.Windows.Forms

Public Class MailboxSetup
    Inherits CommonWindow

    Public Current_MBX_Panels As List(Of Mbx_Panel)
    Private NoticesFields_List As List(Of Object)
    Private FeesDeposits_List As List(Of Object)
    Private ContractFields_List As List(Of Object)
    Private isChanged As Boolean

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

    Private Sub MailboxSetup_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Current_MBX_Panels = New List(Of Mbx_Panel)
        Current_MBX_Panels = Load_Panels_From_DB()
        Current_MBX_Panels = Current_MBX_Panels.OrderBy(Function(x As Mbx_Panel) x.Starting_No).ToList
        Mbx_Panel_ListView.ItemsSource = Current_MBX_Panels

        Load_SetupData_FromPolicy()

        Load_SKUPricing_For_Fees(KeyDepositSKU_TxtBox, KeyDepositAmount_TxtBox)
        Load_SKUPricing_For_Fees(LateFeeSKU_TxtBox, LateFeeAmount_TxtBox)
        Load_SKUPricing_For_Fees(AdminFeeSKU_TxtBox, AdminFeeAmount_TxtBox)
        Load_SKUPricing_For_Fees(OtherFeeSKU_TxtBox, OtherFeeAmount_TxtBox)


        Custom_Months_ComboBox.ItemsSource = New String() {"N/A", "2 months", "4", "5", "7", "8", "9", "10", "11", "13", "14", "15", "16", "17", "18", "24", "36"}
        Custom_Months_ComboBox.SelectedIndex = 0
        DisplayColorButton.Background = Nothing

        For Each currentTab As TabItem In MBX_Setup_TC.Items
            currentTab.Visibility = Visibility.Collapsed
        Next

        MBX_Setup_TC.Margin = New Thickness(162, 0, 0, 0)

        MBX_Setup_LB.SelectedIndex = 0

        If Mbx_Panel_ListView.Items.Count > 0 Then
            Mbx_Panel_ListView.SelectedIndex = 0
        End If
        Update_MonthlyRates()
        isChanged = False

    End Sub

    Private Sub Load_SKUPricing_For_Fees(ByRef SKU_txtBox As Controls.TextBox, ByRef Amount_TxtBox As Controls.TextBox)
        Dim SQL As String
        Dim SegmentSet As String
        Dim current_segment As String

        If SKU_txtBox.Text <> "" Then
            SQL = "Select Sell from Inventory WHERE SKU='" & SKU_txtBox.Text & "'"
            SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
            current_segment = GetNextSegmentFromSet(SegmentSet)
            If current_segment <> "" Then
                Amount_TxtBox.Text = FormatCurrency(ExtractElementFromSegment("Sell", current_segment), , , TriState.False)
            Else
                SKU_txtBox.Text = ""
                Amount_TxtBox.Text = ""

                SKU_txtBox.Focus()

            End If

        End If

    End Sub



    Private Sub Load_SetupData_FromPolicy()
        NoticesFields_List = New List(Of Object)
        FeesDeposits_List = New List(Of Object)
        ContractFields_List = New List(Of Object)

        Get_ChildControls_Of_Grid(Notices_Grid, NoticesFields_List)
        Get_ChildControls_Of_Grid(FeesDeposits_Grid, FeesDeposits_List)
        Get_ChildControls_Of_Grid(Contract_Grid, ContractFields_List)

        LoadList(NoticesFields_List)
        LoadList(FeesDeposits_List)
        LoadList(ContractFields_List)

        Custom_Months_ComboBox.Text = GetPolicyData(gShipriteDB, Custom_Months_ComboBox.Tag)

        If CInt(RenewalDays_TxtBox.Text) > 0 Then
            Renewal_ComboBox.Text = "After"
        Else
            Renewal_ComboBox.Text = "Before"
            RenewalDays_TxtBox.Text = Math.Abs(CInt(RenewalDays_TxtBox.Text))
        End If

        If CInt(ExpirationDays_TxtBox.Text) > 0 Then
            Expiration_ComboBox.Text = "After"
        Else
            Expiration_ComboBox.Text = "Before"
            ExpirationDays_TxtBox.Text = Math.Abs(CInt(ExpirationDays_TxtBox.Text))
        End If

        If CInt(CancellationDays_TxtBox.Text) > 0 Then
            Cancellation_ComboBox.Text = "After"
        Else
            Cancellation_ComboBox.Text = "Before"
            CancellationDays_TxtBox.Text = Math.Abs(CInt(CancellationDays_TxtBox.Text))
        End If

    End Sub

    Private Sub LoadList(ByRef SetupFields_List As List(Of Object))
        For Each obj As Object In SetupFields_List

            If obj.GetType Is GetType(System.Windows.Controls.CheckBox) Then
                obj.isChecked = GetPolicyData(gShipriteDB, obj.tag, "False")
            Else
                obj.text = GetPolicyData(gShipriteDB, obj.tag)
                obj.text = obj.text.replace("''", "'")
            End If
        Next
    End Sub

    Public Shared Function Load_Panels_From_DB() As List(Of Mbx_Panel)
        Dim SegmentSet As String = ""
        Dim current_segment As String = ""
        Dim buf As String = ""
        Dim sql As String
        Dim fieldName As String = ""
        Dim fieldValue As String = ""
        Dim ColorR As Integer
        Dim ColorG As Integer
        Dim ColorB As Integer
        Dim MBX_LIST As List(Of Mbx_Panel) = New List(Of Mbx_Panel)

        Dim current_panel As Mbx_Panel
        sql = "SELECT SizeDesc, StartingNumber, EndingNumber, [1month], [3month], [6month], [12month], customMonth, Business1month, Business3month, Business6month, Business12month, " &
                               " BusinessCustomMonth, Other1month, Other3month, Other6month, Other12month, OtherCustomMonth, ButtonColorR, ButtonColorG, ButtonColorB, TextButtonColor From MailBoxSize"

        Debug.Print(sql)
        buf = IO_GetSegmentSet(gShipriteDB, sql)

        Do Until buf = ""
            current_segment = GetNextSegmentFromSet(buf)
            current_panel = New Mbx_Panel
            current_panel.MBX_Pricing = New List(Of Double)

            Do Until current_segment = ""
                current_segment = ExtractNextElementFromSegment(fieldName, fieldValue, current_segment)

                Select Case fieldName
                    Case "SizeDesc"
                        current_panel.Description = fieldValue
                    Case "StartingNumber"
                        current_panel.Starting_No = fieldValue
                    Case "EndingNumber"
                        current_panel.Ending_No = fieldValue
                    Case "ButtonColorR"
                        If fieldValue = "" Then fieldValue = -1
                        ColorR = fieldValue
                    Case "ButtonColorG"
                        If fieldValue = "" Then fieldValue = -1
                        ColorG = fieldValue
                    Case "ButtonColorB"
                        If fieldValue = "" Then fieldValue = -1
                        ColorB = fieldValue
                    Case "TextButtonColor"
                        If fieldValue = "White" Then
                            current_panel.DisplayTextColor = New System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255))
                        Else
                            current_panel.DisplayTextColor = New System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0))
                        End If

                    Case Else
                        'Add Mailbox Pricing to current panel
                        If fieldValue = "" Then fieldValue = 0
                        current_panel.MBX_Pricing.Add(fieldValue)

                End Select

            Loop

            If ColorR <> -1 Or ColorG <> -1 Or ColorB <> -1 Then
                current_panel.DisplayColor = New System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(ColorR, ColorG, ColorB))
            End If
            MBX_LIST.Add(current_panel)

        Loop

        Return MBX_LIST
    End Function

    Private Sub SaveButton_Click(sender As Object, e As RoutedEventArgs) Handles SaveButton.Click
        Dim selecteditem As ListBoxItem = MBX_Setup_LB.SelectedItem

        If MsgBox("Would you like to save changes to " & selecteditem.Content.ToString & "?", vbYesNo + vbQuestion) = vbNo Then
            Exit Sub
        End If


        Select Case MBX_Setup_LB.SelectedIndex
            Case 0
                SAVE_MailboxPanels()

            Case 1
                SaveList(NoticesFields_List)

                'If "Before" is selected, then # of days has to be saved as a negative number
                If Renewal_ComboBox.SelectedIndex = 0 Then UpdatePolicy(gShipriteDB, RenewalDays_TxtBox.Tag, CInt(RenewalDays_TxtBox.Text) * -1)
                If Cancellation_ComboBox.SelectedIndex = 0 Then UpdatePolicy(gShipriteDB, CancellationDays_TxtBox.Tag, CInt(CancellationDays_TxtBox.Text) * -1)
                If Expiration_ComboBox.SelectedIndex = 0 Then UpdatePolicy(gShipriteDB, ExpirationDays_TxtBox.Tag, CInt(ExpirationDays_TxtBox.Text) * -1)

            Case 2
                SaveList(FeesDeposits_List)

                'Save Fee Pricing to Inventory
                Update_MailboxFee_In_Inventory(KeyDepositSKU_TxtBox.Text, KeyDepositAmount_TxtBox.Text)
                Update_MailboxFee_In_Inventory(LateFeeSKU_TxtBox.Text, LateFeeAmount_TxtBox.Text)
                Update_MailboxFee_In_Inventory(AdminFeeSKU_TxtBox.Text, AdminFeeAmount_TxtBox.Text)
                Update_MailboxFee_In_Inventory(OtherFeeSKU_TxtBox.Text, OtherFeeAmount_TxtBox.Text)

            Case 3
                SaveList(ContractFields_List)

        End Select



        MsgBox("Changes to Mailbox Setup saved successfully!", MsgBoxStyle.OkOnly + MsgBoxStyle.Information, "Mailbox Setup")

        isChanged = False

    End Sub

    Private Sub SaveList(SetupFields_List As List(Of Object))
        For Each obj As Object In SetupFields_List

            If obj.GetType Is GetType(System.Windows.Controls.CheckBox) Then
                UpdatePolicy(gShipriteDB, obj.tag, obj.isChecked)
            Else
                obj.text = obj.text.Replace("'", "''")
                UpdatePolicy(gShipriteDB, obj.tag, obj.text)
                obj.text = obj.text.Replace("''", "'")
            End If
        Next
    End Sub

    Private Sub SAVE_MailboxPanels()
        Dim SQL As String
        '------Update MailboxSize Table------------

        SQL = "Delete * From MailboxSize"
        IO_UpdateSQLProcessor(gShipriteDB, SQL)

        For Each panel As Mbx_Panel In Current_MBX_Panels
            SQL = "INSERT INTO MailboxSize ([SizeDesc], [StartingNumber], [EndingNumber], [ButtonColorR], [ButtonColorG], [ButtonColorB], [TextButtonColor], [1month], [3month], [6month], [12month], [customMonth], [Business1month], [Business3month], [Business6month], [Business12month], " &
                               " [BusinessCustomMonth], [Other1month], [Other3month], [Other6month], [Other12month], [OtherCustomMonth]) VALUES  ("

            SQL = SQL & "'" & panel.Description & "', "
            SQL = SQL & panel.Starting_No & ", "
            SQL = SQL & panel.Ending_No & ", "

            If IsNothing(panel.DisplayColor) Then
                Assign_Color(panel)
            End If

            SQL = SQL & "'" & panel.DisplayColor.Color.R & "', "
            SQL = SQL & "'" & panel.DisplayColor.Color.G & "', "
            SQL = SQL & "'" & panel.DisplayColor.Color.B & "', "

            If panel.DisplayTextColor.Color.R = 255 Then
                SQL = SQL & "'White', "
            Else
                SQL = SQL & "'Black', "
            End If

            For Each x As Double In panel.MBX_Pricing
                SQL = SQL & x & ", "
            Next

            SQL = SQL.TrimEnd()
            SQL = SQL.TrimEnd(",")
            SQL = SQL & ")"

            IO_UpdateSQLProcessor(gShipriteDB, SQL)
            SQL = ""
        Next


        '-------Update Mailbox Table
        For Each panel As Mbx_Panel In Current_MBX_Panels
            SQL = "UPDATE Mailbox set [Size]='" & panel.Description & "' WHERE [MailboxNumber] >= " & panel.Starting_No & " AND [MailboxNumber] <= " & panel.Ending_No
            IO_UpdateSQLProcessor(gShipriteDB, SQL)
        Next


        UpdatePolicy(gShipriteDB, Custom_Months_ComboBox.Tag, Custom_Months_ComboBox.Text)
    End Sub

    Private Sub Update_MailboxFee_In_Inventory(SKU As String, Amount As String)
        If SKU = "" Or Amount = "" Then
            Exit Sub
        End If
        Amount = CDbl(Amount)

        IO_UpdateSQLProcessor(gShipriteDB, "Update Inventory set Sell=" & Amount & " Where SKU='" & SKU & "'")
    End Sub

    Private Sub Assign_Color(ByRef panel As Mbx_Panel)
        Dim colorExists As Boolean = False

        'Creates a List of Default Button colors in case the user does not pick a color on their own.
        Dim Default_Color_List As List(Of System.Windows.Media.SolidColorBrush) = New List(Of System.Windows.Media.SolidColorBrush)

        Default_Color_List.Add(New System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(174, 177, 178)))
        Default_Color_List.Add(New System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(145, 159, 183)))
        Default_Color_List.Add(New System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(139, 155, 74)))
        Default_Color_List.Add(New System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(196, 152, 123)))
        Default_Color_List.Add(New System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(139, 114, 165)))
        Default_Color_List.Add(New System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(157, 186, 224)))
        Default_Color_List.Add(New System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(234, 234, 119)))
        Default_Color_List.Add(New System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(163, 149, 173)))
        Default_Color_List.Add(New System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(160, 188, 188)))
        Default_Color_List.Add(New System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(234, 218, 218)))



        'checks which colors are already used to avoid the same color being used again.
        For Each color As System.Windows.Media.SolidColorBrush In Default_Color_List
            colorExists = False
            For Each x As Mbx_Panel In Current_MBX_Panels

                If Not IsNothing(x.DisplayColor) Then
                    If x.DisplayColor.Color = color.Color Then
                        colorExists = True
                        Exit For
                    End If
                End If

            Next

            If colorExists = False Then
                panel.DisplayColor = color
                Exit Sub
            Else
                panel.DisplayColor = Default_Color_List.Last
            End If

        Next

    End Sub

    Private Sub HandleExit(sender As Object, e As RoutedEventArgs)
        If isChanged Then
            If vbYes = MsgBox("Your changes to the Mailbox setup and pricing have not been saved! Would you like to save them now?", vbYesNo + vbQuestion, "Unsaved Changes") Then
                SaveButton_Click(Nothing, Nothing)
            End If
        End If

        Select Case sender.Name
            Case "HomeButton"
                HomeButton_Click(sender, e)
            Case "BackButton"
                BackButton_Click(sender, e)
            Case "CloseButton"
                CloseButton_Click(sender, e)
            Case "ForwardButton"
                ForwardButton_Click(sender, e)
        End Select

    End Sub

#Region "Panel selection - add/delete/save/colors"

    Private Sub Save_Changes_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Save_Changes_Btn.Click
        If Mbx_Panel_ListView.SelectedIndex = -1 Then
            MsgBox("No Panel is selected. Please Select a existing panel first", MsgBoxStyle.OkOnly + MsgBoxStyle.Exclamation, "Cannot Save Changes!")
            Exit Sub
        End If

        If CheckInput() = False Then
            Exit Sub
        End If

        'Check if there is a number overlap in Mailbox Number ranges
        '-----------------------------------------------------------------------------------
        Dim Low As Integer = CInt(StartingNo_TxtBox.Text)
        Dim High As Integer = CInt(EndingNo_TxtBox.Text)


        For Each x As Mbx_Panel In Current_MBX_Panels

            If Current_MBX_Panels.IndexOf(x) <> Mbx_Panel_ListView.SelectedIndex Then
                If (Low >= x.Starting_No And Low <= x.Ending_No) Or (High >= x.Starting_No And High <= x.Ending_No) Then
                    MsgBox("There is a number overlap with panel: " & x.Description, MsgBoxStyle.OkOnly + MsgBoxStyle.Exclamation, "Warning!")
                    Exit Sub
                End If
            End If
        Next

        Dim Current_Panel As New Mbx_Panel
        Current_Panel.MBX_Pricing = New List(Of Double)


        Current_Panel.Description = Trim(Panel_Desc_TxtBox.Text)
        Current_Panel.Starting_No = Trim(StartingNo_TxtBox.Text)
        Current_Panel.Ending_No = Trim(EndingNo_TxtBox.Text)
        Current_Panel.DisplayColor = DisplayColorButton.Background
        Current_Panel.DisplayTextColor = DisplayColorButton.Foreground

        'adds all pricing into Current_Panel object
        Read_MBX_Pricing_Into_List(Current_Panel)

        Current_MBX_Panels(Mbx_Panel_ListView.SelectedIndex) = Current_Panel
        Mbx_Panel_ListView.Items.Refresh()

        'MsgBox("Changes Saved Successfully!", MsgBoxStyle.OkOnly + MsgBoxStyle.Information)
        ClearFields()

        isChanged = True
    End Sub

    Private Sub AddPanel_Btn_Click(sender As Object, e As RoutedEventArgs) Handles AddPanel_Btn.Click

        If CheckInput() = False Then
            Exit Sub
        End If

        'Check if there is a number overlap in Mailbox Number ranges
        '-----------------------------------------------------------------------------------
        Dim Low As Integer = CInt(StartingNo_TxtBox.Text)
        Dim High As Integer = CInt(EndingNo_TxtBox.Text)


        For Each x As Mbx_Panel In Current_MBX_Panels
            If (Low >= x.Starting_No And Low <= x.Ending_No) Or (High >= x.Starting_No And High <= x.Ending_No) Then
                MsgBox("There is a number overlap with panel: " & x.Description, MsgBoxStyle.OkOnly + MsgBoxStyle.Exclamation, "Warning!")
                Exit Sub
            End If
        Next

        If Current_MBX_Panels.FindIndex(Function(value As Mbx_Panel) value.Description = Panel_Desc_TxtBox.Text) <> -1 Then
            MsgBox("Cannot Add Panel. Panel " + Panel_Desc_TxtBox.Text + " alread exists!", MsgBoxStyle.OkOnly + MsgBoxStyle.Exclamation, "Warning!")
            Exit Sub
        End If


        Dim Current_Panel As New Mbx_Panel
        Current_Panel.MBX_Pricing = New List(Of Double)

        Current_Panel.Description = Trim(Panel_Desc_TxtBox.Text)
        Current_Panel.Starting_No = Trim(StartingNo_TxtBox.Text)
        Current_Panel.Ending_No = Trim(EndingNo_TxtBox.Text)
        Current_Panel.DisplayColor = DisplayColorButton.Background
        Current_Panel.DisplayTextColor = DisplayColorButton.Foreground

        'adds all pricing into Current_Panel object
        Read_MBX_Pricing_Into_List(Current_Panel)

        'add current panel to master list of panels
        Current_MBX_Panels.Add(Current_Panel)

        'Update UI ListView
        Mbx_Panel_ListView.Items.Refresh()


        'reset all fields
        ClearFields()
        Mbx_Panel_ListView.UnselectAll()

        isChanged = True

    End Sub

    Private Sub Delete_Panel_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Delete_Panel_Btn.Click
        If Mbx_Panel_ListView.SelectedIndex = -1 Then
            MsgBox("Please select line item to delete first!", MsgBoxStyle.Critical + MsgBoxStyle.OkOnly, "Cannot Delete!")
            Exit Sub
        End If

        Current_MBX_Panels.RemoveAt(Mbx_Panel_ListView.SelectedIndex)
        Mbx_Panel_ListView.Items.Refresh()

        isChanged = True

    End Sub

    Private Function CheckInput() As Boolean

        If Not IsNumeric(StartingNo_TxtBox.Text) Or Not IsNumeric(EndingNo_TxtBox.Text) Then
            MsgBox("Start Or End field Is Not numeric! Please enter in Numbers only", MsgBoxStyle.OkOnly + MsgBoxStyle.Exclamation, "Warning!")
            Return False
        End If

        If Panel_Desc_TxtBox.Text = "" Or StartingNo_TxtBox.Text = "" Or EndingNo_TxtBox.Text = "" Then
            MsgBox("Description, Start, And End fields cannot be empty!", MsgBoxStyle.OkOnly + MsgBoxStyle.Exclamation, "Please fill out all fields!")
            Return False

        End If

        If CDbl(StartingNo_TxtBox.Text) > CDbl(EndingNo_TxtBox.Text) Then
            MsgBox("Starting number cannot be larger Then ending Number", MsgBoxStyle.OkOnly + MsgBoxStyle.Exclamation, "Warning!")
            Return False
        End If
        Return True

    End Function

    Private Sub Mbx_Panel_ListView_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles Mbx_Panel_ListView.SelectionChanged
        If Mbx_Panel_ListView.SelectedIndex = -1 Then
            Exit Sub
        End If

        Dim Current_Panel As Mbx_Panel
        Current_Panel = Current_MBX_Panels.Item(Mbx_Panel_ListView.SelectedIndex)

        Panel_Desc_TxtBox.Text = Current_Panel.Description
        StartingNo_TxtBox.Text = Current_Panel.Starting_No
        EndingNo_TxtBox.Text = Current_Panel.Ending_No
        DisplayColorButton.Background = Current_Panel.DisplayColor
        DisplayColorButton.Foreground = Current_Panel.DisplayTextColor

        One_Month_Res_TxtBox.Text = FormatCurrency(Current_Panel.MBX_Pricing(0))
        Three_Month_Res_TxtBox.Text = FormatCurrency(Current_Panel.MBX_Pricing(1))
        Six_Month_Res_TxtBox.Text = FormatCurrency(Current_Panel.MBX_Pricing(2))
        Twelve_Month_Res_TxtBox.Text = FormatCurrency(Current_Panel.MBX_Pricing(3))
        Custom_Res_TxtBox.Text = FormatCurrency(Current_Panel.MBX_Pricing(4))

        One_Month_Comm_TxtBox.Text = FormatCurrency(Current_Panel.MBX_Pricing(5))
        Three_Month_Comm_TxtBox.Text = FormatCurrency(Current_Panel.MBX_Pricing(6))
        Six_Month_Comm_TxtBox.Text = FormatCurrency(Current_Panel.MBX_Pricing(7))
        Twelve_Month_Comm_TxtBox.Text = FormatCurrency(Current_Panel.MBX_Pricing(8))
        Custom_Comm_TxtBox.Text = FormatCurrency(Current_Panel.MBX_Pricing(9))

        One_Month_Other_TxtBox.Text = FormatCurrency(Current_Panel.MBX_Pricing(10))
        Three_Month_Other_TxtBox.Text = FormatCurrency(Current_Panel.MBX_Pricing(11))
        Six_Month_Other_TxtBox.Text = FormatCurrency(Current_Panel.MBX_Pricing(12))
        Twelve_Month_Other_TxtBox.Text = FormatCurrency(Current_Panel.MBX_Pricing(13))
        Custom_Other_TxtBox.Text = FormatCurrency(Current_Panel.MBX_Pricing(14))

        Update_MonthlyRates()

    End Sub

    Private Sub Read_MBX_Pricing_Into_List(ByRef Current_Panel As Mbx_Panel)
        'Load all pricing into list

        Current_Panel.MBX_Pricing.Add(CDbl(One_Month_Res_TxtBox.Text))
        Current_Panel.MBX_Pricing.Add(CDbl(Three_Month_Res_TxtBox.Text))
        Current_Panel.MBX_Pricing.Add(CDbl(Six_Month_Res_TxtBox.Text))
        Current_Panel.MBX_Pricing.Add(CDbl(Twelve_Month_Res_TxtBox.Text))
        Current_Panel.MBX_Pricing.Add(CDbl(Custom_Res_TxtBox.Text))

        Current_Panel.MBX_Pricing.Add(CDbl(One_Month_Comm_TxtBox.Text))
        Current_Panel.MBX_Pricing.Add(CDbl(Three_Month_Comm_TxtBox.Text))
        Current_Panel.MBX_Pricing.Add(CDbl(Six_Month_Comm_TxtBox.Text))
        Current_Panel.MBX_Pricing.Add(CDbl(Twelve_Month_Comm_TxtBox.Text))
        Current_Panel.MBX_Pricing.Add(CDbl(Custom_Comm_TxtBox.Text))

        Current_Panel.MBX_Pricing.Add(CDbl(One_Month_Other_TxtBox.Text))
        Current_Panel.MBX_Pricing.Add(CDbl(Three_Month_Other_TxtBox.Text))
        Current_Panel.MBX_Pricing.Add(CDbl(Six_Month_Other_TxtBox.Text))
        Current_Panel.MBX_Pricing.Add(CDbl(Twelve_Month_Other_TxtBox.Text))
        Current_Panel.MBX_Pricing.Add(CDbl(Custom_Other_TxtBox.Text))

    End Sub

    Private Sub DisplayColorButton_Click(sender As Object, e As RoutedEventArgs) Handles DisplayColorButton.Click
        Dim cdialog As New ColorDialog()
        Dim mybrush As System.Windows.Media.Brush
        Dim existingBrush As System.Windows.Media.SolidColorBrush


        cdialog.FullOpen = True
        cdialog.AnyColor = True
        cdialog.ShowHelp = True

        'color dialog should open with the current color selected
        If Not IsNothing(DisplayColorButton.Background) Then
            existingBrush = DisplayColorButton.Background
            cdialog.Color = System.Drawing.Color.FromArgb(existingBrush.Color.R, existingBrush.Color.G, existingBrush.Color.B)
        End If


        'open color selection dialog
        If (cdialog.ShowDialog() = System.Windows.Forms.DialogResult.OK) Then
            mybrush = New System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(cdialog.Color.R, cdialog.Color.G, cdialog.Color.B))
            DisplayColorButton.Background() = mybrush
        End If

    End Sub

    Private Sub White_Button_Click(sender As Object, e As RoutedEventArgs) Handles White_Button.Click
        DisplayColorButton.Foreground = New System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255))

    End Sub

    Private Sub Black_Button_Click(sender As Object, e As RoutedEventArgs) Handles Black_Button.Click
        DisplayColorButton.Foreground = New System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0))
    End Sub

#End Region


#Region "Maintnance"
    Private Sub SKU_TxtBox_LostFocus(sender As Controls.TextBox, e As RoutedEventArgs) Handles KeyDepositSKU_TxtBox.LostFocus, LateFeeSKU_TxtBox.LostFocus, AdminFeeSKU_TxtBox.LostFocus, OtherFeeSKU_TxtBox.LostFocus
        If sender.Name = "KeyDepositSKU_TxtBox" Then
            If sender.Text = "" Then
                KeyDepositAmount_TxtBox.Text = ""
            Else
                Load_SKUPricing_For_Fees(sender, KeyDepositAmount_TxtBox)
            End If

        ElseIf sender.Name = "LateFeeSKU_TxtBox" Then
            If sender.Text = "" Then
                LateFeeAmount_TxtBox.Text = ""
            Else
                Load_SKUPricing_For_Fees(sender, LateFeeAmount_TxtBox)
            End If

        ElseIf sender.Name = "AdminFeeSKU_TxtBox" Then
            If sender.Text = "" Then
                AdminFeeAmount_TxtBox.Text = ""
            Else
                Load_SKUPricing_For_Fees(sender, AdminFeeAmount_TxtBox)
            End If

        Else

            If sender.Text = "" Then
                OtherFeeAmount_TxtBox.Text = ""
            Else
                Load_SKUPricing_For_Fees(sender, OtherFeeAmount_TxtBox)
            End If

        End If
    End Sub

    Private Sub Notice_Days_PreviewTextInput(sender As Object, e As TextCompositionEventArgs) Handles CancellationDays_TxtBox.PreviewTextInput, RenewalDays_TxtBox.PreviewTextInput, ExpirationDays_TxtBox.PreviewTextInput
        Dim allowedchars As String = "0123456789"
        If allowedchars.IndexOf(CChar(e.Text)) = -1 Then e.Handled = True
    End Sub

    Private Sub DepositAndFees_Amount_PreviewTextInput(sender As Object, e As TextCompositionEventArgs) Handles KeyDepositAmount_TxtBox.PreviewTextInput, ForwardingDepositAmount_TxtBox.PreviewTextInput, LateFeeAmount_TxtBox.PreviewTextInput, AdminFeeAmount_TxtBox.PreviewTextInput, OtherFeeAmount_TxtBox.PreviewTextInput
        Dim allowedchars As String = "0123456789."
        If allowedchars.IndexOf(CChar(e.Text)) = -1 Then e.Handled = True
    End Sub

    Private Sub Starting_Ending_PreviewTextInput(sender As Object, e As TextCompositionEventArgs) Handles StartingNo_TxtBox.PreviewTextInput, EndingNo_TxtBox.PreviewTextInput
        Dim allowedchars As String = "0123456789"
        If allowedchars.IndexOf(CChar(e.Text)) = -1 Then e.Handled = True
    End Sub

    Private Sub MBX_Prices_PreviewTextInput(sender As Object, e As TextCompositionEventArgs) Handles One_Month_Res_TxtBox.PreviewTextInput
        Dim allowedchars As String = "0123456789."
        If allowedchars.IndexOf(CChar(e.Text)) = -1 Then e.Handled = True
    End Sub

    Private Sub MBX_Prices_LostFocus(sender As Object, e As RoutedEventArgs) Handles One_Month_Res_TxtBox.LostFocus, Three_Month_Res_TxtBox.LostFocus, Six_Month_Res_TxtBox.LostFocus, Twelve_Month_Res_TxtBox.LostFocus, One_Month_Comm_TxtBox.LostFocus, Three_Month_Comm_TxtBox.LostFocus, Six_Month_Comm_TxtBox.LostFocus, Twelve_Month_Comm_TxtBox.LostFocus, One_Month_Other_TxtBox.LostFocus, Three_Month_Other_TxtBox.LostFocus, Six_Month_Other_TxtBox.LostFocus, Twelve_Month_Other_TxtBox.LostFocus, KeyDepositAmount_TxtBox.LostFocus, ForwardingDepositAmount_TxtBox.LostFocus, LateFeeAmount_TxtBox.LostFocus, AdminFeeAmount_TxtBox.LostFocus, OtherFeeAmount_TxtBox.LostFocus, Custom_Res_TxtBox.LostFocus, Custom_Comm_TxtBox.LostFocus, Custom_Other_TxtBox.LostFocus
        Dim CurrentPrice_TxtBox = DirectCast(sender, System.Windows.Controls.TextBox)

        If CurrentPrice_TxtBox.Text <> "" And IsNumeric(CurrentPrice_TxtBox.Text) Then
            CurrentPrice_TxtBox.Text = FormatCurrency(CurrentPrice_TxtBox.Text,,, TriState.False)
        Else
            CurrentPrice_TxtBox.Text = FormatCurrency(0)
        End If

        Update_MonthlyRates()


    End Sub

    Private Sub Update_MonthlyRates()
        Dim CustomMonth As Integer = 1

        _3Res.Text = FormatCurrency(CDbl(Three_Month_Res_TxtBox.Text) / 3, 2)
        _3Com.Text = FormatCurrency(CDbl(Three_Month_Comm_TxtBox.Text) / 3, 2)
        _3Oth.Text = FormatCurrency(CDbl(Three_Month_Other_TxtBox.Text) / 3, 2)

        _6Res.Text = FormatCurrency(CDbl(Six_Month_Res_TxtBox.Text) / 6, 2)
        _6Com.Text = FormatCurrency(CDbl(Six_Month_Comm_TxtBox.Text) / 6, 2)
        _6Oth.Text = FormatCurrency(CDbl(Six_Month_Other_TxtBox.Text) / 6, 2)

        _12Res.Text = FormatCurrency(CDbl(Twelve_Month_Res_TxtBox.Text) / 12, 2)
        _12Com.Text = FormatCurrency(CDbl(Twelve_Month_Comm_TxtBox.Text) / 12, 2)
        _12Oth.Text = FormatCurrency(CDbl(Twelve_Month_Other_TxtBox.Text) / 12, 2)


        Select Case Custom_Months_ComboBox.SelectedIndex
            Case 0
                CustomMonth = 1
            Case 1
                CustomMonth = 2
            Case Else
                CustomMonth = Custom_Months_ComboBox.SelectedItem
        End Select

        _CRes.Text = FormatCurrency(CDbl(Custom_Res_TxtBox.Text) / CustomMonth, 2)
        _CCom.Text = FormatCurrency(CDbl(Custom_Comm_TxtBox.Text) / CustomMonth, 2)
        _COth.Text = FormatCurrency(CDbl(Custom_Other_TxtBox.Text) / CustomMonth, 2)

    End Sub

    Private Sub MBX_Prices_GotFocus(sender As Object, e As RoutedEventArgs) Handles One_Month_Res_TxtBox.GotFocus, Three_Month_Res_TxtBox.GotFocus, Six_Month_Res_TxtBox.GotFocus, Twelve_Month_Res_TxtBox.GotFocus, One_Month_Comm_TxtBox.GotFocus, Three_Month_Comm_TxtBox.GotFocus, Six_Month_Comm_TxtBox.GotFocus, Twelve_Month_Comm_TxtBox.GotFocus, One_Month_Other_TxtBox.GotFocus, Three_Month_Other_TxtBox.GotFocus, Six_Month_Other_TxtBox.GotFocus, Twelve_Month_Other_TxtBox.GotFocus, KeyDepositAmount_TxtBox.GotFocus, ForwardingDepositAmount_TxtBox.GotFocus, LateFeeAmount_TxtBox.GotFocus, AdminFeeAmount_TxtBox.GotFocus, OtherFeeAmount_TxtBox.GotFocus, Custom_Res_TxtBox.GotFocus, Custom_Comm_TxtBox.GotFocus, Custom_Other_TxtBox.GotFocus
        Dim CurrentPrice_TxtBox = DirectCast(sender, System.Windows.Controls.TextBox)

        If CurrentPrice_TxtBox.Text = "$0.00" Then
            CurrentPrice_TxtBox.Text = ""
        End If

        If CurrentPrice_TxtBox.Text.Contains("$") Then
            CurrentPrice_TxtBox.Text = CurrentPrice_TxtBox.Text.Replace("$", "")
        End If

        CurrentPrice_TxtBox.SelectAll()
    End Sub

    Private Sub ClearFields()
        Panel_Desc_TxtBox.Text = ""
        StartingNo_TxtBox.Text = ""
        EndingNo_TxtBox.Text = ""

        One_Month_Res_TxtBox.Text = "$0.00"
        Three_Month_Res_TxtBox.Text = "$0.00"
        Six_Month_Res_TxtBox.Text = "$0.00"
        Twelve_Month_Res_TxtBox.Text = "$0.00"
        Custom_Res_TxtBox.Text = "$0.00"

        One_Month_Comm_TxtBox.Text = "$0.00"
        Three_Month_Comm_TxtBox.Text = "$0.00"
        Six_Month_Comm_TxtBox.Text = "$0.00"
        Twelve_Month_Comm_TxtBox.Text = "$0.00"
        Custom_Res_TxtBox.Text = "$0.00"

        One_Month_Other_TxtBox.Text = "$0.00"
        Three_Month_Other_TxtBox.Text = "$0.00"
        Six_Month_Other_TxtBox.Text = "$0.00"
        Twelve_Month_Other_TxtBox.Text = "$0.00"
        Custom_Other_TxtBox.Text = "$0.00"
        Update_MonthlyRates()

    End Sub

    Private Sub MBX_Setup_LB_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles MBX_Setup_LB.SelectionChanged
        MBX_Setup_TC.SelectedIndex = MBX_Setup_LB.SelectedIndex
    End Sub

    Private Sub Custom_Months_ComboBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles Custom_Months_ComboBox.SelectionChanged
        Update_MonthlyRates()
    End Sub

#End Region

End Class

