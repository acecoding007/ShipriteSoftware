Imports SHIPRITE.ShipRiteReports
Public Class AdditionalName_Item
    Public Property DisplayName As String
    Public Property CID As String
    Public Property Email As String
    Public Property CellPhone As String
    Public Property CellCarrier As String
    Public Property isSelected As Boolean
    Public Property MBXNamesList_Segment As String
End Class

Public Class MailboxManager
    Inherits CommonWindow

    Public Mailbox_Panels_List As List(Of Mbx_Panel)
    Public Mailbox_List As List(Of Mailbox) 'full list of all mailboxes
    Public DatabaseID As Long
    Public AdditionalNames_List As List(Of AdditionalName_Item)
    Public currentPrice As Double
    Public currentPrice_RentalOnly As Double
    Public POS_Call As String
    Public RenterSegment As String
    Public StopEventsfromLoading As Boolean
    Public MBXNo_CallfromWindow As String

    Public CustomPricing As String 'placeholder if selecting custom pricing for a new mailbox



    Public Class Mailbox_History_Item
        Public Property TransactionDate As String
        Public Property Description As String
        Public Property Customer As String
        Public Property Charge As String
        Public Property Clerk As String
    End Class



    Public Sub New()

        MyBase.New()

        ' This call is required by the designer.
        InitializeComponent()

    End Sub

    Public Sub New(ByVal callingWindow As Window, Optional ByVal POS_input As String = "", Optional MbxNo As String = "")

        MyBase.New(callingWindow)

        ' This call is required by the designer.
        InitializeComponent()
        POS_Call = POS_input
        MBXNo_CallfromWindow = MbxNo

    End Sub

#Region "Load Procedures"

    Private Sub MailboxManager_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Try

            StopEventsfromLoading = True
            Load_MBX_Panels_FromDB()
            Load_Mailboxes_into_List()
            RenterSegment = ""
            Pricing_Expander.IsExpanded = True

            'set default selections on startup
            Additional_Names_LV.ItemsSource = AdditionalNames_List

            MBX_Display_ListBox.SelectedIndex = 0
            SortBy_ListBox.SelectedIndex = 0
            FreeMonths_ComboBox.ItemsSource = New Integer() {"0", "1", "2", "3", "4", "5", "6", "12"}
            FWD_Day_ComboBox.ItemsSource = New String() {"Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"}
            Form1583_Border.Visibility = Visibility.Hidden
            CustomMonth_Btn.Content = GetPolicyData(gShipriteDB, "MBX_CustomMonthsNo") & " Months"


            Load_Fee_and_Deposit_Pricing(KeyDeposit_CheckBox, "KeyDepositSKU")
            Load_Fee_and_Deposit_Pricing(OtherFee_CheckBox, "OtherDepositSKU")
            Load_Fee_and_Deposit_Pricing(AdminFee_CheckBox, "AdminFeeSKU")
            Load_Fee_and_Deposit_Pricing(LateFee_CheckBox, "LateFeeSKU")

            If GetPolicyData(gShipriteDB, "MBX_CustomMonthsNo") = "N/A" Then
                CustomMonth_Btn.Visibility = Visibility.Hidden
                CustomMonthPrice_TxtBlock.Visibility = Visibility.Hidden
            End If


            If POS_Call <> "" Then

                If Not String.IsNullOrEmpty(gCustomerSegment) Then
                    RenterSegment = gCustomerSegment
                End If

                If POS_Call = "MBX" Then 'New Rental called from POS
                    MBX_Display_ListBox.SelectedIndex = 3 'show only available mailboxes
                    Customer_TxtBox.Text = CreateDisplayBlock(RenterSegment, True)

                    AdditionalNames_List = New List(Of AdditionalName_Item)
                    Load_Main_Renter_Into_AdditionalNameList()
                End If

                If POS_Call = "MBXR" Then
                    DisplayMailboxes()
                End If
            End If

            StopEventsfromLoading = False


            If MBXNo_CallfromWindow <> "" Then
                'Open mailbox number passed by calling window
                Search_TxtBox.Text = MBXNo_CallfromWindow
                DisplayMailboxes()

                Dim Current_Mailbox As Mailbox

                Current_Mailbox = Mailbox_List(Mailbox_List.FindIndex(Function(value As Mailbox) value.Number = CInt(MBXNo_CallfromWindow)))
                AdditionalNames_List = New List(Of AdditionalName_Item)

                MailboxNo_Label.Content = Current_Mailbox.Number
                MailboxSize_Label.Content = Current_Mailbox.Panel


                Display_Detail_Mailbox(Current_Mailbox.Number)


            End If

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Loading Mailbox Module")
        End Try
    End Sub


    Private Sub Load_Fee_and_Deposit_Pricing(ByRef SKU_CheckBox As Controls.CheckBox, ByVal SKU_PolicyField As String)
        Try
            'Displays pricing of the Fees/Deposits in the ToolTip of the respective checkboxes.

            Dim SQL As String
            Dim SegmentSet As String
            Dim current_segment As String


            SQL = "Select Sell from Inventory WHERE SKU='" & GetPolicyData(gShipriteDB, SKU_PolicyField) & "'"
            SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
            current_segment = GetNextSegmentFromSet(SegmentSet)
            If current_segment <> "" Then
                SKU_CheckBox.ToolTip = FormatCurrency(ExtractElementFromSegment("Sell", current_segment), , , TriState.False)
            Else
                SKU_CheckBox.ToolTip = "0"
            End If

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Loading Fee and Deposit Pricing from Inventory")
        End Try
    End Sub

    'Loads all Mailbox Panels from database into List
    Private Sub Load_MBX_Panels_FromDB()

        Try
            Mailbox_Panels_List = New List(Of Mbx_Panel)
            Mailbox_Panels_List = MailboxSetup.Load_Panels_From_DB()


            MBX_Panels_ListBox.ItemsSource = Mailbox_Panels_List



        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Loading Mailbox Panels")
        End Try

    End Sub

    'Loads individual mailbox data into list that is displayed.
    Private Sub Load_Mailboxes_into_List()
        Try

            Dim Index As Integer
            Dim Current_MBX_Number As Integer
            Dim Current_Mailbox As New Mailbox
            Mailbox_List = New List(Of Mailbox)

            Dim SQL As String = "SELECT [MailboxNumber], [EndDate], [Size], [Name], [CID] From Mailbox"
            Dim SegmentSet As String = IO_GetSegmentSet(gShipriteDB, SQL)
            Dim current_segment As String
            Dim fieldName As String = ""
            Dim fieldValue As String = ""


            'Prime list of mailboxes
            For Each current_panel As Mbx_Panel In Mailbox_Panels_List
                For Current_MBX_Number = current_panel.Starting_No To current_panel.Ending_No
                    Current_Mailbox.Number = Current_MBX_Number
                    Current_Mailbox.Panel = current_panel.Description
                    Current_Mailbox.DisplayColor = current_panel.DisplayColor
                    Current_Mailbox.DisplayTextColor = current_panel.DisplayTextColor
                    Current_Mailbox.Name = ""
                    Mailbox_List.Add(Current_Mailbox)
                    Current_Mailbox = New Mailbox
                Next
            Next


            'Load Rented Mailboxes
            Do Until SegmentSet = ""
                current_segment = GetNextSegmentFromSet(SegmentSet)

                Current_MBX_Number = ExtractElementFromSegment("MailboxNumber", current_segment)
                Index = Mailbox_List.FindIndex(Function(x As Mailbox) x.Number = Current_MBX_Number)

                Do Until current_segment = ""
                    current_segment = ExtractNextElementFromSegment(fieldName, fieldValue, current_segment)

                    If fieldValue <> "" And Index <> -1 Then
                        Select Case fieldName

                            Case "EndDate"
                                Mailbox_List.Item(Index).ExpirationDate = fieldValue
                            Case "Name"
                                Mailbox_List.Item(Index).Name = fieldValue
                            Case "CID"
                                Mailbox_List.Item(Index).ContactID = fieldValue
                            Case "Size"
                                Mailbox_List.Item(Index).Panel = fieldValue

                        End Select
                    End If
                Loop
            Loop

            Mailboxes_Control.ItemsSource = Mailbox_List
            Mailboxes_Control.Items.Refresh()

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Loading Individual Mailboxes.")
        End Try

    End Sub

#End Region

    'Displays and sorts individual mailboxes using selected criteria
    Private Sub DisplayMailboxes() Handles MBX_Panels_ListBox.SelectionChanged, SortBy_ListBox.SelectionChanged, MBX_Display_ListBox.SelectionChanged
        Try
            Dim current_Mailbox_List As List(Of Mailbox)
            current_Mailbox_List = New List(Of Mailbox)

            If POS_Call <> "MBXR" Then

                'show selected panels
                For Each panel As Mbx_Panel In MBX_Panels_ListBox.SelectedItems
                    current_Mailbox_List.AddRange(Mailbox_List.FindAll(Function(value As Mailbox) value.Panel = panel.Description))
                Next

                'if no panel is selected, display all panels
                If current_Mailbox_List.Count = 0 Then
                    current_Mailbox_List = Mailbox_List
                Else
                    All_Panels_Btn.Background = New System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White)
                End If


                If Search_TxtBox.Text <> "" And Search_TxtBox.Text <> "Box# or Name" Then
                    If IsNumeric(Search_TxtBox.Text) Then
                        current_Mailbox_List = current_Mailbox_List.FindAll(Function(value As Mailbox) value.Number = CInt(Trim(Search_TxtBox.Text))).ToList()
                    Else
                        current_Mailbox_List = current_Mailbox_List.FindAll(Function(value) value.Name.ToUpper.Contains(Trim(Search_TxtBox.Text.ToUpper)) = True)
                    End If
                End If

                'show selected display option
                If MBX_Display_ListBox.SelectedIndex = 1 Then
                    current_Mailbox_List = (current_Mailbox_List.FindAll(Function(value As Mailbox) value.ExpirationDate <= Today And value.Name <> ""))
                ElseIf MBX_Display_ListBox.SelectedIndex = 2 Then 'show rented boxes
                    current_Mailbox_List = (current_Mailbox_List.FindAll(Function(value As Mailbox) value.Name <> ""))

                ElseIf MBX_Display_ListBox.SelectedIndex = 3 Then 'show available boxes
                    current_Mailbox_List = (current_Mailbox_List.FindAll(Function(value As Mailbox) value.Name = ""))
                End If


                'sort by the selected method
                If SortBy_ListBox.SelectedIndex = 1 Then ' Sort by Name
                    current_Mailbox_List = current_Mailbox_List.OrderBy(Function(value As Mailbox) value.Name = "").ThenBy(Function(x) x.Name).ToList
                ElseIf SortBy_ListBox.SelectedIndex = 0 Then 'Sort by Number
                    current_Mailbox_List = current_Mailbox_List.OrderBy(Function(value As Mailbox) value.Number).ToList()
                ElseIf SortBy_ListBox.SelectedIndex = 2 Then 'Sort by Expiration Date
                    current_Mailbox_List = current_Mailbox_List.OrderBy(Function(value As Mailbox) value.Name = "").ThenBy(Function(x) x.ExpirationDate).ToList
                ElseIf SortBy_ListBox.SelectedIndex = 3 Then 'sort by Panel Name
                    current_Mailbox_List = current_Mailbox_List.OrderBy(Function(value As Mailbox) value.Panel).ToList()
                End If

            Else
                'RENEWAL OPTION selected in POS, only show boxes linked to customer selected in POS
                current_Mailbox_List = Mailbox_List.FindAll(Function(value As Mailbox) value.ContactID.ToString = ExtractElementFromSegment("ID", RenterSegment))
                If current_Mailbox_List.Count = 1 Then
                    Display_Detail_Mailbox(current_Mailbox_List(0).Number)
                End If
            End If

            Mailboxes_Control.ItemsSource = current_Mailbox_List
            Mailboxes_Control.Items.Refresh()


        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Displaying Mailbox Selection")
        End Try
    End Sub


    Private Sub Mailbox_Button_Click(sender As Object, e As RoutedEventArgs)
        Try

            Dim CurrentButton As Button = TryCast(sender, Button)
            Dim Current_Mailbox As Mailbox
            ClearScreen()

            Current_Mailbox = Mailbox_List(Mailbox_List.FindIndex(Function(value As Mailbox) value.Number = CurrentButton.Tag()))
            AdditionalNames_List = New List(Of AdditionalName_Item)
            Load_Main_Renter_Into_AdditionalNameList()

            MailboxNo_Label.Content = Current_Mailbox.Number
            MailboxSize_Label.Content = Current_Mailbox.Panel

            If String.IsNullOrEmpty(Current_Mailbox.Name) Then
                'NEW MAILBOX
                StartDate.SelectedDate = Today
                GetMailboxPricing()

            Else
                'EXISTING RENTED BOX
                Display_Detail_Mailbox(Current_Mailbox.Number)
            End If

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Displaying Mailbox Info")
        End Try

    End Sub

    Private Sub SetMailboxLabel_Size(MbxNo As Integer)
        Select Case MbxNo
            Case >= 10000
                MailboxNo_Label.FontSize = 28
            Case >= 1000
                MailboxNo_Label.FontSize = 36
            Case >= 100
                MailboxNo_Label.FontSize = 48
            Case Else
                MailboxNo_Label.FontSize = 55
        End Select
    End Sub

    Private Sub Display_Detail_Mailbox(Mbx_No As Integer)
        Try
            Dim current_segment As String = ""
            Dim CID As Integer
            Dim FWD_CID As String = ""
            Dim SQL As String = "SELECT * FROM Mailbox WHERE MailboxNumber = " & Mbx_No
            Dim buf As String = IO_GetSegmentSet(gShipriteDB, SQL)

            StopEventsfromLoading = True

            If buf = "" Then
                Exit Sub
            End If

            current_segment = GetNextSegmentFromSet(buf)

            CID = ExtractElementFromSegment("CID", current_segment)

            SQL = "SELECT * FROM Contacts WHERE ID = " & CID
            RenterSegment = IO_GetSegmentSet(gShipriteDB, SQL)
            Customer_TxtBox.Text = CreateDisplayBlock(RenterSegment, True)


            DatabaseID = ExtractElementFromSegment("ID", current_segment)
            MailboxSize_Label.Content = ExtractElementFromSegment("Size", current_segment)
            MailboxNo_Label.Content = Mbx_No



            Select Case ExtractElementFromSegment("Business", current_segment)
                Case -1 'Business/Commercial Rates
                    RateType_ComboBox.SelectedIndex = 1
                Case 0 'Residential Rates
                    RateType_ComboBox.SelectedIndex = 0
                Case 1 'Other Rates
                    RateType_ComboBox.SelectedIndex = 2
                Case 2 'Custom Rates
                    RateType_ComboBox.SelectedIndex = 3
            End Select



            ExpireEndOfMonth_CheckBox.IsChecked = ExtractElementFromSegment(ExpireEndOfMonth_CheckBox.Tag, current_segment)
            KeyDeposit_CheckBox.IsChecked = ExtractElementFromSegment(KeyDeposit_CheckBox.Tag, current_segment)
            OtherFee_CheckBox.IsChecked = ExtractElementFromSegment(OtherFee_CheckBox.Tag, current_segment)

            FreeMonths_ComboBox.SelectedValue = CInt(ExtractElementFromSegment(FreeMonths_ComboBox.Tag, current_segment))

            If POS_Call <> "MBXR" Then
                Select Case CInt(ExtractElementFromSegment("NumberOfMonths", current_segment))
                    Case 1
                        OneMonth_Btn.IsChecked = True
                    Case 3
                        ThreeMonth_Btn.IsChecked = True
                    Case 6
                        SixMonth_Btn.IsChecked = True
                    Case 12
                        TvelweMonth_Btn.IsChecked = True
                    Case Else
                        CustomMonth_Btn.IsChecked = True
                        CustomMonth_Btn.Content = ExtractElementFromSegment("NumberOfMonths", current_segment) & " Months"
                End Select


                Price_Label.Content = FormatCurrency(ExtractElementFromSegment(Price_Label.Tag, current_segment))

                StartDate.SelectedDate = ExtractElementFromSegment(StartDate.Tag, current_segment)
                EndDate.SelectedDate = ExtractElementFromSegment(EndDate.Tag, current_segment)

            Else
                'Renewal - start date should be the end of the rented period
                StartDate.SelectedDate = ExtractElementFromSegment(EndDate.Tag, current_segment)

            End If



            'ADDITIONAL NAMES-------------
            Load_Additional_Names(Mbx_No)

            'MAIL FORWARDING--------------------
            FWD_CID = ExtractElementFromSegment("FCID", RenterSegment, "")

            If FWD_CID <> "" Then
                FWD_Address_TxtBox.Text = CreateDisplayBlock(IO_GetSegmentSet(gShipriteDB, "SELECT * FROM Contacts WHERE ID = " & FWD_CID), True)
                FWD_Address_TxtBox.Tag = FWD_CID
            End If

            FWD_DepositAmount_TxtBox.Text = ExtractElementFromSegment(FWD_DepositAmount_TxtBox.Tag, current_segment)
            FWD_Day_ComboBox.SelectedValue = ExtractElementFromSegment(FWD_Day_ComboBox.Tag, current_segment)
            FWD_Notes_TxtBox.Text = ExtractElementFromSegment(FWD_Notes_TxtBox.Tag, current_segment)

            Select Case ExtractElementFromSegment("ForwardDepositPeriod", current_segment)
                Case 0
                    FWD_Never_RadioBtn.IsChecked = True
                Case 1
                    FWD_Daily_RadioBtn.IsChecked = True
                Case 2
                    FWD_Weekly_RadioBtn.IsChecked = True
                Case 3
                    FWD_BiWeekly_RadioBtn.IsChecked = True
                Case 4
                    FWD_Monthly_RadioBtn.IsChecked = True
            End Select


            'MAILBOX HISTORY---------------------
            Load_Mailbox_History_Tab(Mbx_No)


            '-- PS1583 -  2 forms of ID and -----
            Load_PS1583_Items(CID)

            '--- PS1583 - Type of Business ---
            PlaceOfRegistration_TextBox.Text = ExtractElementFromSegment(PlaceOfRegistration_TextBox.Tag, current_segment, "")
            TypeOfBusiness_TextBox.Text = ExtractElementFromSegment(TypeOfBusiness_TextBox.Tag, current_segment, "")
            IsBusiness_ChkBx.IsChecked = ExtractElementFromSegment(IsBusiness_ChkBx.Tag, current_segment, "False")

            GetMailboxPricing()

            SetMailboxLabel_Size(Mbx_No)

            StopEventsfromLoading = False

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Displaying Mailbox Details")
        End Try
    End Sub



    Private Sub Load_Mailbox_History_Tab(Mbx_No As Integer)
        Try
            Dim SQL As String = "SELECT * FROM MBXHistory WHERE MBX = " & Mbx_No
            Dim buf As String = IO_GetSegmentSet(gShipriteDB, SQL)
            Dim current_segment As String
            Dim currentItem As Mailbox_History_Item
            Dim history_list As List(Of Mailbox_History_Item) = New List(Of Mailbox_History_Item)

            If buf = "" Then
                Exit Sub
            End If

            Do Until buf = ""
                current_segment = GetNextSegmentFromSet(buf)
                currentItem = New Mailbox_History_Item

                currentItem.TransactionDate = ExtractElementFromSegment("Date", current_segment)
                currentItem.Description = ExtractElementFromSegment("Desc", current_segment)
                currentItem.Customer = ExtractElementFromSegment("AccountName", current_segment)
                currentItem.Charge = FormatCurrency(ExtractElementFromSegment("Charge", current_segment))
                currentItem.Clerk = ExtractElementFromSegment("Clerk", current_segment)

                history_list.Add(currentItem)
            Loop

            MBX_History_ListView.ItemsSource = history_list
            MBX_History_ListView.Items.Refresh()

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Displaying Mailbox History")
        End Try
    End Sub

    Function IsRenewal() As Boolean
        Try
            Dim SegmentSet As String

            SegmentSet = IO_GetSegmentSet(gShipriteDB, "Select CID From Mailbox WHERE MailboxNumber=" & MailboxNo_Label.Content)

            If SegmentSet = "" Then
                'non existant box, New Rental
                Return False
            End If

            If ExtractElementFromSegment("CID", GetNextSegmentFromSet(SegmentSet)) = ExtractElementFromSegment("ID", RenterSegment) Then
                'original contact and current contact are same, Renwal
                Return True
            Else
                'current contact is different from original contact linked to mailbox, New Rental
                Return False
            End If

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error")
        End Try
        Return False
    End Function


    Private Sub Rent_Button_Click(sender As Object, e As RoutedEventArgs) Handles Rent_Button.Click
        Try
            Dim ret As String
            Dim segment As String = ""

            If Customer_TxtBox.Text = "" Or RenterSegment = "" Then
                MsgBox("Cannot Rent Mailbox, Please enter a customer first!", vbExclamation + vbOKOnly, "Error!")
                Exit Sub
            End If

            If MailboxNo_Label.Content.ToString = "" Then
                MsgBox("Cannot Rent Mailbox, Please select a mailbox first!", vbExclamation + vbOKOnly, "Error!")
                Exit Sub
            End If

            If StartDate.Text = "" Or EndDate.Text = "" Then
                MsgBox("Cannot Rent Mailbox, Start and End Date cannot blank!", vbExclamation + vbOKOnly, "Error!")
                Exit Sub
            End If

            Update_MBXNamesList(MailboxNo_Label.Content)

            segment = Create_Update_Segment()

            'Create Entry in MBXHistory Table
            If IsRenewal() Then
                MBXHistoryEntry("Renewal")
            Else
                MBXHistoryEntry("Rental")
            End If

            'check forwarding address
            If FWD_Address_TxtBox.Tag <> "" Then
                IO_UpdateSQLProcessor(gShipriteDB, "Update Contacts set [FCID]='" & FWD_Address_TxtBox.Tag & "' WHERE ID=" & ExtractElementFromSegment("ID", RenterSegment))
            End If

            If IO_GetSegmentSet(gShipriteDB, "Select * From Mailbox WHERE MailboxNumber=" & MailboxNo_Label.Content) = "" Then
                'Create NEW Mailbox Entry in database
                ret = IO_UpdateSQLProcessor(gShipriteDB, MakeInsertSQLFromSchema("Mailbox", segment, gMailboxTableSchema, True))
            Else
                'Update Existing Mailbox in database
                segment = AddElementToSegment(segment, "ID", DatabaseID)
                ret = IO_UpdateSQLProcessor(gShipriteDB, MakeUpdateSQLFromSchema("Mailbox", segment, gMailboxTableSchema, False, True))
            End If

            If ret = "1" Then


                If PrintAgreement_CheckBox.IsChecked Then
                    Print_Agreement(MailboxNo_Label.Content)
                End If

                If Print1583_CheckBox.IsChecked Then
                    Print_1583Form(MailboxNo_Label.Content)
                End If

                Mailbox_List.Item(Mailbox_List.FindIndex(Function(value As Mailbox) value.Number = MailboxNo_Label.Content)).Name = Customer_TxtBox.Text.Substring(0, Customer_TxtBox.Text.IndexOf(Environment.NewLine))

                MsgBox("Mailbox # " & MailboxNo_Label.Content & " rented to " & Customer_TxtBox.Text & " !", vbInformation + vbOKOnly, "Mailbox Rental")


                If POS_Call <> "" Then
                    'close Mailbox Manager and return to POS
                    CreatePOSEntry(segment)
                    Me.Close()

                Else

                    DisplayMailboxes()
                    If MBX_Display_ListBox.SelectedIndex() = 3 Then
                        MBX_Display_ListBox.SelectedIndex() = 2
                    End If
                    ClearScreen()

                End If
            End If

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Renting Mailbox")
        End Try
    End Sub



    Private Sub Print_Agreement(MbxNo As Integer)
        Try
            Cursor = Cursors.Wait
            Dim report As New _ReportObject()
            report.ReportName = "MBContract.rpt"

            'report.ReportFormula = "{MBXHistory.Date} > #" & Today.AddMonths(-6).ToShortDateString & "# and {MBXHistory.Desc}='Cancel Mailbox'"
            report.ReportFormula = "{Mailbox.MailboxNumber} = " & MbxNo

            Dim reportPrev As New ReportPreview(report)
            Cursor = Cursors.Arrow
            reportPrev.ShowDialog()

        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to report [Cancelled Mailboxes]...")
        Finally : Cursor = Cursors.Arrow
        End Try
    End Sub

    Private Sub CreatePOSEntry(ByRef segment As String)
        Dim SegmentInventory As String

        Try

            AddPosLineToSet(0, "MBX", "«Desc Mailbox Rental»«Department MAILBOX»", Trim(Replace(currentPrice_RentalOnly, "$", "")))
            NoteLineToPOS("Mailbox# " & ExtractElementFromSegment("MailboxNumber", segment) & " - " & ExtractElementFromSegment("NumberOfMonths", segment) & " Month Rental")
            NoteLineToPOS("Expiration Date: " & ExtractElementFromSegment("EndDate", segment))


            If KeyDeposit_CheckBox.IsChecked = True Then
                SegmentInventory = IO_GetSegmentSet(gShipriteDB, "Select * from Inventory WHERE SKU='" & GetPolicyData(gShipriteDB, "KeyDepositSKU") & "'")
                AddPosLineToSet(0, GetPolicyData(gShipriteDB, "KeyDepositSKU"), SegmentInventory)

            End If

            If LateFee_CheckBox.IsChecked = True Then
                SegmentInventory = IO_GetSegmentSet(gShipriteDB, "Select * from Inventory WHERE SKU='" & GetPolicyData(gShipriteDB, "LateFeeSKU") & "'")
                AddPosLineToSet(0, GetPolicyData(gShipriteDB, "LateFeeSKU"), SegmentInventory)

            End If

            If AdminFee_CheckBox.IsChecked = True Then
                SegmentInventory = IO_GetSegmentSet(gShipriteDB, "Select * from Inventory WHERE SKU='" & GetPolicyData(gShipriteDB, "AdminFeeSKU") & "'")
                AddPosLineToSet(0, GetPolicyData(gShipriteDB, "AdminFeeSKU"), SegmentInventory)

            End If

            If OtherFee_CheckBox.IsChecked = True Then
                SegmentInventory = IO_GetSegmentSet(gShipriteDB, "Select * from Inventory WHERE SKU='" & GetPolicyData(gShipriteDB, "OtherDepositSKU") & "'")
                AddPosLineToSet(0, GetPolicyData(gShipriteDB, "OtherDepositSKU"), SegmentInventory)

            End If


        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Creating Mailbox POS Entry.")
        End Try
    End Sub

    Private Function Create_Update_Segment() As String
        Try
            Dim AdditionalNames As String = ""
            Dim Segment As String = ""

            Segment = AddElementToSegment(Segment, MailboxNo_Label.Tag, MailboxNo_Label.Content)
            Segment = AddElementToSegment(Segment, MailboxSize_Label.Tag, MailboxSize_Label.Content)
            Segment = AddElementToSegment(Segment, "Rented", "True")

            Select Case RateType_ComboBox.SelectedIndex
                Case 0 'Residential Rates 
                    Segment = AddElementToSegment(Segment, RateType_ComboBox.Tag, "0")
                Case 1 'Business/Commercial Rates
                    Segment = AddElementToSegment(Segment, RateType_ComboBox.Tag, "-1")
                Case 2 'Other Rates
                    Segment = AddElementToSegment(Segment, RateType_ComboBox.Tag, "1")
                Case 3 'Custom Rates
                    Segment = AddElementToSegment(Segment, RateType_ComboBox.Tag, "2")

                    If IsRenewal() = False Then
                        Segment = AddElementToSegment(Segment, "CustomRates", CustomPricing)
                    End If
            End Select

            Segment = AddElementToSegment(Segment, StartDate.Tag, StartDate.Text)
            Segment = AddElementToSegment(Segment, EndDate.Tag, EndDate.Text)


            Segment = AddElementToSegment(Segment, "CustFirst", ExtractElementFromSegment("FName", RenterSegment))
            Segment = AddElementToSegment(Segment, "CustLast", ExtractElementFromSegment("LName", RenterSegment))
            Segment = AddElementToSegment(Segment, "Name", ExtractElementFromSegment("Name", RenterSegment))
            Segment = AddElementToSegment(Segment, "CID", ExtractElementFromSegment("ID", RenterSegment))

            If OneMonth_Btn.IsChecked = True Then
                Segment = AddElementToSegment(Segment, "NumberOfMonths", "1")
            ElseIf ThreeMonth_Btn.IsChecked = True Then
                Segment = AddElementToSegment(Segment, "NumberOfMonths", "3")
            ElseIf SixMonth_Btn.IsChecked = True Then
                Segment = AddElementToSegment(Segment, "NumberOfMonths", "6")
            ElseIf TvelweMonth_Btn.IsChecked = True Then
                Segment = AddElementToSegment(Segment, "NumberOfMonths", "12")
            Else
                Segment = AddElementToSegment(Segment, "NumberOfMonths", GetPolicyData(gShipriteDB, "MBX_CustomMonthsNo"))
            End If

            Segment = AddElementToSegment(Segment, FreeMonths_ComboBox.Tag, FreeMonths_ComboBox.Text)
            Segment = AddElementToSegment(Segment, KeyDeposit_CheckBox.Tag, KeyDeposit_CheckBox.IsChecked)
            Segment = AddElementToSegment(Segment, OtherFee_CheckBox.Tag, OtherFee_CheckBox.IsChecked)
            Segment = AddElementToSegment(Segment, ExpireEndOfMonth_CheckBox.Tag, ExpireEndOfMonth_CheckBox.IsChecked)
            Segment = AddElementToSegment(Segment, Price_Label.Tag, CDbl(Price_Label.Content))


            '1583 Items
            Segment = AddElementToSegment(Segment, TypeOfBusiness_TextBox.Tag, TypeOfBusiness_TextBox.Text)
            Segment = AddElementToSegment(Segment, IsBusiness_ChkBx.Tag, IsBusiness_ChkBx.IsChecked)
            Segment = AddElementToSegment(Segment, PlaceOfRegistration_TextBox.Tag, PlaceOfRegistration_TextBox.Text)


            Segment = AddElementToSegment(Segment, ID_1_TextBox.Tag, ID_1_TextBox.Text)
            Segment = AddElementToSegment(Segment, ID_1_IssuingEntity_TextBox.Tag, ID_1_IssuingEntity_TextBox.Text)
            Segment = AddElementToSegment(Segment, ID_1_ExpDate_TextBox.Tag, ID_1_ExpDate_TextBox.Text)
            Segment = AddElementToSegment(Segment, ID_1_Type_CmbBx.Tag, ID_1_Type_CmbBx.SelectedIndex)

            'Segment = AddElementToSegment(Segment, ID_2_Textbox.Tag, ID_2_Textbox.Text)
            Segment = AddElementToSegment(Segment, ID_2_Type_CmbBx.Tag, ID_2_Type_CmbBx.SelectedIndex)



            If FWD_DepositAmount_TxtBox.Text = "" Then FWD_DepositAmount_TxtBox.Text = "0"
            Segment = AddElementToSegment(Segment, FWD_DepositAmount_TxtBox.Tag, Trim(Replace(FWD_DepositAmount_TxtBox.Text, "$", "")))
            Segment = AddElementToSegment(Segment, FWD_Day_ComboBox.Tag, FWD_Day_ComboBox.Text)
            Segment = AddElementToSegment(Segment, FWD_Notes_TxtBox.Tag, FWD_Notes_TxtBox.Text)


            If FWD_Never_RadioBtn.IsChecked = True Then
                Segment = AddElementToSegment(Segment, "ForwardDepositPeriod", "0")
            ElseIf FWD_Daily_RadioBtn.IsChecked = True Then
                Segment = AddElementToSegment(Segment, "ForwardDepositPeriod", "1")
            ElseIf FWD_Weekly_RadioBtn.IsChecked = True Then
                Segment = AddElementToSegment(Segment, "ForwardDepositPeriod", "2")
            ElseIf FWD_BiWeekly_RadioBtn.IsChecked = True Then
                Segment = AddElementToSegment(Segment, "ForwardDepositPeriod", "3")
            ElseIf FWD_Monthly_RadioBtn.IsChecked = True Then
                Segment = AddElementToSegment(Segment, "ForwardDepositPeriod", "4")
            End If


            'RESET Notice tags
            Segment = AddElementToSegment(Segment, "RenewalSent", "False")
            Segment = AddElementToSegment(Segment, "ExpiredSent", "False")
            Segment = AddElementToSegment(Segment, "CanceledSent", "False")


            For Each item As AdditionalName_Item In AdditionalNames_List
                If item.CID <> ExtractElementFromSegment("ID", RenterSegment) Then
                    AdditionalNames = AdditionalNames & item.DisplayName & "    "
                End If
            Next
            AdditionalNames = Trim(AdditionalNames)
            Segment = AddElementToSegment(Segment, "AddlNamesList", AdditionalNames)


            Return Segment

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Creating Mailbox Database Entry")
        End Try
        Return ""
    End Function


    Private Sub Month_Button_Selected(sender As Object, e As RoutedEventArgs) Handles OneMonth_Btn.Checked, ThreeMonth_Btn.Checked, SixMonth_Btn.Checked, TvelweMonth_Btn.Checked, CustomMonth_Btn.Checked
        Try
            If StartDate.SelectedDate Is Nothing Or StopEventsfromLoading Then
                Exit Sub
            End If

            Dim dat As Date = StartDate.SelectedDate
            GetMailboxPricing()

            If OneMonth_Btn.IsChecked Then
                EndDate.SelectedDate = dat.AddMonths(1)
            ElseIf ThreeMonth_Btn.IsChecked Then
                EndDate.SelectedDate = dat.AddMonths(3)

            ElseIf SixMonth_Btn.IsChecked Then
                EndDate.SelectedDate = dat.AddMonths(6)

            ElseIf TvelweMonth_Btn.IsChecked Then
                EndDate.SelectedDate = dat.AddMonths(12)

            Else
                If GetPolicyData(gShipriteDB, "MBX_CustomMonthsNo") <> "N/A" Then
                    EndDate.SelectedDate = dat.AddMonths(GetPolicyData(gShipriteDB, "MBX_CustomMonthsNo"))
                End If
            End If

            If Not IsRenewal() Then

                If GetPolicyData(gShipriteDB, "AlwaysKeyDeposits") Then
                    KeyDeposit_CheckBox.IsChecked = True
                End If

                If GetPolicyData(gShipriteDB, "MBX_Always_AdminFee") Then
                    AdminFee_CheckBox.IsChecked = True
                End If

                If GetPolicyData(gShipriteDB, "MBX_Always_OtherFee") Then
                    OtherFee_CheckBox.IsChecked = True
                End If

            End If

            Calculate_Current_Price()

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error calculating rental period.")
        End Try
    End Sub


    Private Sub Calculate_Current_Price()
        Try
            Dim DailyCharge As Double
            Dim AdditionalDays As Double

            'Set main rentail period price
            If OneMonth_Btn.IsChecked Then
                currentPrice = CDbl(Replace(OneMonth_Btn.ToolTip, "$", ""))

            ElseIf ThreeMonth_Btn.IsChecked Then
                currentPrice = CDbl(Replace(ThreeMonth_Btn.ToolTip, "$", ""))

            ElseIf SixMonth_Btn.IsChecked Then
                currentPrice = CDbl(Replace(SixMonth_Btn.ToolTip, "$", ""))

            ElseIf TvelweMonth_Btn.IsChecked Then
                currentPrice = CDbl(Replace(TvelweMonth_Btn.ToolTip, "$", ""))

            ElseIf CustomMonth_Btn.IsChecked Then
                currentPrice = CDbl(Replace(CustomMonth_Btn.ToolTip, "$", ""))
            End If

            'add cost for addtional days added by the "Expire at end of month" option
            If ExpireEndOfMonth_CheckBox.IsChecked = True And Not IsNothing(StartDate.SelectedDate) And Not IsNothing(EndDate.SelectedDate) Then

                DailyCharge = Calculate_Charge_Per_Day()
                AdditionalDays = Calculate_EndOfMonth_Expiration()

                If AdditionalDays > 0 Then
                    currentPrice = currentPrice + (AdditionalDays * DailyCharge)

                End If

            End If

            currentPrice_RentalOnly = currentPrice 'set price for only rental period without any fees


            'Add pricing for fees and deposits
            If KeyDeposit_CheckBox.IsChecked = True Then
                currentPrice = currentPrice + CDbl(Replace(KeyDeposit_CheckBox.ToolTip, "$", ""))
            End If

            If LateFee_CheckBox.IsChecked = True Then
                currentPrice = currentPrice + CDbl(Replace(LateFee_CheckBox.ToolTip, "$", ""))
            End If

            If AdminFee_CheckBox.IsChecked = True Then
                currentPrice = currentPrice + CDbl(Replace(AdminFee_CheckBox.ToolTip, "$", ""))
            End If

            If OtherFee_CheckBox.IsChecked = True Then
                currentPrice = currentPrice + CDbl(Replace(OtherFee_CheckBox.ToolTip, "$", ""))
            End If



            Price_Label.Content = FormatCurrency(currentPrice)

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error calculating rental pricing.")
        End Try
    End Sub

    Private Function Calculate_Charge_Per_Day() As Double
        Try

            Dim startD As Date = StartDate.SelectedDate
            Dim endD As Date = EndDate.SelectedDate
            Dim RentDays As Integer = endD.Subtract(startD).Days

            Return Math.Round(currentPrice / RentDays, 2)

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error calculating daily charge.")
        End Try
        Return 0.0
    End Function

    Private Function Calculate_EndOfMonth_Expiration() As Integer
        Try
            'updates expiration date and returns number of days added on to expiration date
            Dim AdditionalDays As Integer

            Dim D As Date = EndDate.SelectedDate ' original date
            Dim EndOfMonthDate As Date = New Date(D.Year, D.Month, Date.DaysInMonth(D.Year, D.Month)) 'end of month date

            EndDate.SelectedDate = EndOfMonthDate

            AdditionalDays = EndOfMonthDate.Subtract(D).Days

            Return AdditionalDays

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Calculating Expiration Date.")
        End Try
        Return 0
    End Function

    Private Sub GetMailboxPricing()
        Try
            Dim Current_Segment As String

            If MailboxSize_Label Is Nothing Then
                Exit Sub
            End If

            If String.IsNullOrEmpty(MailboxSize_Label.Content) Then
                Exit Sub
            End If

            Dim SegmentSet As String = IO_GetSegmentSet(gShipriteDB, "SELECT * From MailBoxSize WHERE SizeDesc = '" & MailboxSize_Label.Content & "'")

            If SegmentSet = "" Then
                Exit Sub
            End If

            Current_Segment = GetNextSegmentFromSet(SegmentSet)

            If RateType_ComboBox.SelectedIndex = 0 Then
                'Residential
                SetMailboxPricing("", Current_Segment)
            ElseIf RateType_ComboBox.SelectedIndex = 1 Then
                'Commercial
                SetMailboxPricing("Business", Current_Segment)
            ElseIf RateType_ComboBox.SelectedIndex = 2 Then
                'Other
                SetMailboxPricing("Other", Current_Segment)
            Else
                'Custom Rates
                Apply_Custom_Rates()
            End If

            Calculate_Current_Price()

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Obtaining Mailbox Pricing.")
        End Try
    End Sub

    Private Sub SetMailboxPricing(type As String, current_segment As String)

        Dim buf As String = ""
        Dim NoMonths As Integer
        Try

            OneMonthPrice_TxtBlock.Text = FormatCurrency(ExtractElementFromSegment(type & "1month", current_segment))
            ThreeMonthPrice_TxtBlock.Text = FormatCurrency(Val(ExtractElementFromSegment(type & "3month", current_segment)) / 3)
            SixMonthPrice_TxtBlock.Text = FormatCurrency(Val(ExtractElementFromSegment(type & "6month", current_segment)) / 6)
            TvelweMonthPrice_TxtBlock.Text = FormatCurrency(Val(ExtractElementFromSegment(type & "12month", current_segment)) / 12)
            buf = GetPolicyData(gShipriteDB, "MBX_CustomMonthsNo")
            NoMonths = Val(buf)
            If Not NoMonths = 0 Then

                CustomMonthPrice_TxtBlock.Text = FormatCurrency(Val(ExtractElementFromSegment(type & "CustomMonth", current_segment)) / NoMonths)

            End If

            OneMonth_Btn.ToolTip = FormatCurrency(Val(ExtractElementFromSegment(type & "1month", current_segment)))
            ThreeMonth_Btn.ToolTip = FormatCurrency(Val(ExtractElementFromSegment(type & "3month", current_segment)))
            SixMonth_Btn.ToolTip = FormatCurrency(Val(ExtractElementFromSegment(type & "6month", current_segment)))
            TvelweMonth_Btn.ToolTip = FormatCurrency(Val(ExtractElementFromSegment(type & "12month", current_segment)))
            CustomMonth_Btn.ToolTip = FormatCurrency(Val(ExtractElementFromSegment(type & "CustomMonth", current_segment)))

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error setting Mailbox Pricing.")
        End Try
    End Sub

    Private Sub CancelMailbox_Button_Click(sender As Object, e As RoutedEventArgs) Handles CancelMailbox_Button.Click
        Try
            Dim mbxNo As Integer = MailboxNo_Label.Content

            If MailboxNo_Label.Content.ToString = "" Then
                MsgBox("Cannot Cancel Mailbox, Please select a mailbox first!", vbExclamation + vbOKOnly, "Error!")
                Exit Sub
            End If

            If MsgBox("Are you sure you want to Cancel Mailbox # " & mbxNo & " ?", MsgBoxStyle.Question + vbYesNo, "Delete/Cancel Mailbox") = MsgBoxResult.No Then
                Exit Sub
            End If

            If MBXHistoryEntry("Cancel Mailbox") Then
                IO_UpdateSQLProcessor(gShipriteDB, "DELETE * FROM Mailbox WHERE MailboxNumber = " & mbxNo)

                Dim segmentSet As String =
                    IO_GetSegmentSet(gShipriteDB,
                                     "SELECT CID, COUNT(CID) AS NUM FROM MBXNamesList WHERE CID IN " &
                                     "(SELECT CID FROM MBXNamesList WHERE MBX=" & mbxNo & ") " &
                                     "GROUP BY CID")
                Dim segment As String
                Dim num As Integer
                Dim CID As Integer
                Dim mbxLessList As String = ""
                While segmentSet.Length > 0
                    segment = GetNextSegmentFromSet(segmentSet)
                    num = Val(ExtractElementFromSegment("NUM", segment, "1"))
                    CID = Val(ExtractElementFromSegment("CID", segment))
                    If num = 1 Then
                        mbxLessList &= ", " & CID
                    End If
                End While
                If mbxLessList.Length > 0 Then
                    IO_UpdateSQLProcessor(gShipriteDB,
                                          "UPDATE Contacts SET MBX=False WHERE ID IN (" &
                                          mbxLessList.Substring(2) & ")"
                                          )
                End If


                IO_UpdateSQLProcessor(gShipriteDB, "Delete * From MbxNamesList WHERE MBX=" & mbxNo)

                MsgBox("Mailbox #" & mbxNo & " cancelled successfully!", vbInformation + vbOKOnly, "Cancel Mailbox")

                Dim mboxButton = Mailbox_List.FindIndex(Function(x As Mailbox) x.Number = mbxNo)
                Mailbox_List.Item(mboxButton).ExpirationDate = Date.Parse("1/1/0001")
                Mailbox_List.Item(mboxButton).Name = ""
                Mailbox_List.Item(mboxButton).ContactID = Nothing
                Mailboxes_Control.Items.Refresh()

            End If

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Cancelling Mailbox.")
        End Try
    End Sub

    Private Function MBXHistoryEntry(ByVal Desc As String) As Boolean
        Try
            Dim segment As String = ""

            segment = AddElementToSegment(segment, "Date", DateTime.Now)
            segment = AddElementToSegment(segment, "Desc", Desc)
            segment = AddElementToSegment(segment, "MBX", MailboxNo_Label.Content)
            segment = AddElementToSegment(segment, "CID", ExtractElementFromSegment("ID", RenterSegment))
            segment = AddElementToSegment(segment, "Clerk", gCurrentUser)

            segment = AddElementToSegment(segment, "AccountNumber", ExtractElementFromSegment("AR", RenterSegment))
            segment = AddElementToSegment(segment, "AccountName", ExtractElementFromSegment("Name", RenterSegment))

            If Desc = "Cancel Mailbox" Then
                segment = AddElementToSegment(segment, "Charge", "0")

            Else
                Price_Label.Content = Replace(Price_Label.Content, ",", "")
                segment = AddElementToSegment(segment, "Charge", Replace(Price_Label.Content, "$", ""))

            End If


            If IO_UpdateSQLProcessor(gShipriteDB, MakeInsertSQLFromSchema("MBXHistory", segment, gMBXHistoryTableSchema, True)) Then
                'entry created in MBXHistory Table
                Return True
            Else
                Return False
            End If

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Saving Mailbox History")
        End Try
        Return False
    End Function


    Private Sub Customer_TxtBox_KeyDown(sender As Object, e As KeyEventArgs) Handles Customer_TxtBox.KeyDown
        Try

            If e.Key = Key.Return Then
                OpenContactManager()
            End If
        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error searching Names")
        End Try
    End Sub

    Private Sub Customer_TxtBox_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles Customer_TxtBox.MouseDoubleClick
        OpenContactManager()
    End Sub

    Private Sub OpenContactManager()
        Dim RenterCID As Long

        gContactManagerSegment = ""
        gAutoExitFromContacts = True
        RenterCID = ExtractElementFromSegment("ID", RenterSegment, "0")

        Dim win As New ContactManager(Me, RenterCID, Customer_TxtBox.Text)
        win.ShowDialog(Me)

        If Not gContactManagerSegment = "" Then

            RenterSegment = gContactManagerSegment
            Customer_TxtBox.Text = CreateDisplayBlock(RenterSegment, True)

            'Check additionalnames list for old renter and remove
            If RenterCID <> 0 Then AdditionalNames_List.RemoveAll(Function(x) x.CID = RenterCID)


            'Add newly selected renter
            Load_Main_Renter_Into_AdditionalNameList()
        End If

    End Sub

    Private Sub Load_Main_Renter_Into_AdditionalNameList()
        If RenterSegment = "" Then Exit Sub

        Dim AddName = New AdditionalName_Item
        AddName.DisplayName = ExtractElementFromSegment("Name", RenterSegment)
        AddName.CID = ExtractElementFromSegment("ID", RenterSegment)
        AdditionalNames_List.Add(AddName)

        PS1583_Names_CmbBx.ItemsSource = AdditionalNames_List
        PS1583_Names_CmbBx.Items.Refresh()

        If AdditionalNames_List.Count > 0 Then
            PS1583_Names_CmbBx.SelectedIndex = 0
        End If
    End Sub


#Region "Additional Names"

    Private Sub Load_Additional_Names(ByVal mbx_no As Integer)
        Dim current_segment As String
        Dim AddName As AdditionalName_Item
        Dim buf As String = IO_GetSegmentSet(gShipriteDB, "SELECT * FROM MbxNamesList WHERE MBX=" & mbx_no)
        AdditionalNames_List = New List(Of AdditionalName_Item)

        Try
            If buf = "" Then
                Additional_Names_LV.Items.Refresh()
                Exit Sub
            End If

            Do Until buf = ""
                AddName = New AdditionalName_Item
                current_segment = GetNextSegmentFromSet(buf)

                AddName.MBXNamesList_Segment = current_segment 'segment contains the PS1583 related items
                AddName.DisplayName = ExtractElementFromSegment("Name", current_segment)
                AddName.CID = ExtractElementFromSegment("CID", current_segment)

                AdditionalNames_List.Add(AddName)
            Loop

            Additional_Names_LV.ItemsSource = AdditionalNames_List
            Additional_Names_LV.Items.Refresh()

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Loading Additional Names")
        End Try
    End Sub

    Private Sub Add_AdditionalName_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Add_AdditionalName_Btn.Click
        Try

            Dim addname As AdditionalName_Item = New AdditionalName_Item
            gResult = ""
            gAutoExitFromContacts = True

            Dim win As New ContactManager(Me)
            win.ShowDialog(Me)

            If Not gContactManagerSegment = "" Then

                Dim addlNameSegment As String = gContactManagerSegment
                addname.CID = ExtractElementFromSegment("ID", addlNameSegment)
                addname.DisplayName = ExtractElementFromSegment("Name", addlNameSegment)
                addname.MBXNamesList_Segment = addlNameSegment
                AdditionalNames_List.Add(addname)

                If Not Convert.ToBoolean(ExtractElementFromSegment("Residential", addlNameSegment)) Then
                    'if Business, also add First and Last Name
                    Dim FName As String = ExtractElementFromSegment("FName", addlNameSegment)
                    Dim LName As String = ExtractElementFromSegment("LName", addlNameSegment)

                    If ExtractElementFromSegment("Name", addlNameSegment, "") = LName & ", " & FName Then
                        'Do Nothing - it will create a dupilcate "LName, FName" entry

                    ElseIf FName <> "" And LName <> "" Then
                        addname = New AdditionalName_Item
                        addname.DisplayName = LName & ", " & FName
                        addname.CID = ExtractElementFromSegment("ID", addlNameSegment)
                        addname.MBXNamesList_Segment = addlNameSegment
                        AdditionalNames_List.Add(addname)

                    ElseIf FName = "" And LName <> "" Then
                        addname = New AdditionalName_Item
                        addname.DisplayName = LName
                        addname.CID = ExtractElementFromSegment("ID", addlNameSegment)
                        addname.MBXNamesList_Segment = addlNameSegment
                        AdditionalNames_List.Add(addname)

                    ElseIf FName <> "" And LName = "" Then
                        addname = New AdditionalName_Item
                        addname.DisplayName = FName
                        addname.CID = ExtractElementFromSegment("ID", addlNameSegment)
                        addname.MBXNamesList_Segment = addlNameSegment
                        AdditionalNames_List.Add(addname)

                    End If

                End If

                Additional_Names_LV.ItemsSource = AdditionalNames_List

                Additional_Names_LV.Items.Refresh()

                PS1583_Names_CmbBx.Items.Refresh()

            End If

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Adding Additional Name")
        End Try
    End Sub

    Private Sub Update_MBXNamesList(ByVal mbx_no As Integer)
        Try
            Dim SQL As String
            Dim addname As AdditionalName_Item
            Dim ContactIDs As String = ""

            'Clear out Mailbox flag from Contacts table
            If AdditionalNames_List.Count > 0 Then
                For Each item As AdditionalName_Item In AdditionalNames_List
                    ContactIDs = ContactIDs & ", " & item.CID
                Next
                ContactIDs = ContactIDs.Substring(2)
            End If

            If ContactIDs.Length > 0 Then
                Dim notInAddNames As String = IO_GetSegmentSet(gShipriteDB,
                                                               "SELECT CID, COUNT(CID) AS NUM FROM (SELECT CID FROM MBXNamesList WHERE CID NOT IN (" &
                                                               ContactIDs & ") AND CID IN (SELECT CID FROM MBXNamesList WHERE MBX=" &
                                                               mbx_no & ")) GROUP BY CID"
                                                               )
                Dim segment As String
                Dim trim As Boolean = False
                While notInAddNames.Length > 0
                    segment = GetNextSegmentFromSet(notInAddNames)
                    If Val(ExtractElementFromSegment("NUM", segment)) <= 1 Then
                        ContactIDs &=", " & ExtractElementFromSegment("CID", segment)
                        trim = True
                    End If
                End While
                If trim Then ContactIDs = ContactIDs.Substring(2)
            Else

                Dim notInAddNames As String = IO_GetSegmentSet(gShipriteDB,
                                                               "SELECT CID, COUNT(CID) AS NUM FROM (SELECT CID FROM MBXNamesList WHERE " &
                                                               "CID IN (SELECT CID FROM MBXNamesList WHERE MBX=" &
                                                               mbx_no & ")) GROUP BY CID"
                                                               )
                Dim segment As String
                Dim trim As Boolean = False
                While notInAddNames.Length > 0
                    segment = GetNextSegmentFromSet(notInAddNames)
                    If Val(ExtractElementFromSegment("NUM", segment)) <= 1 Then
                        ContactIDs &= ", " & ExtractElementFromSegment("CID", segment)
                        trim = True
                    End If
                End While
                If trim Then ContactIDs = ContactIDs.Substring(2)
            End If


            If ContactIDs <> "" Then
                IO_UpdateSQLProcessor(gShipriteDB, "update contacts set MBX=False WHERE ID IN (" & ContactIDs & ")")
            End If

            'Clear out existing names from MBXNamesList
            IO_UpdateSQLProcessor(gShipriteDB, "Delete * From MbxNamesList WHERE MBX = " & mbx_no)



            'if main renter's Contact ID is not in list of names, add it in
            If AdditionalNames_List.FindIndex(Function(x) x.CID = ExtractElementFromSegment("ID", RenterSegment)) = -1 Then

                addname = New AdditionalName_Item
                addname.CID = ExtractElementFromSegment("ID", RenterSegment)
                addname.DisplayName = ExtractElementFromSegment("Name", RenterSegment)
                AdditionalNames_List.Insert(0, addname)


                If Not Convert.ToBoolean(ExtractElementFromSegment("Residential", RenterSegment)) Then
                    'if Business, separately add First and Last Name

                    If ExtractElementFromSegment("FName", RenterSegment) <> "" And ExtractElementFromSegment("LName", RenterSegment) <> "" Then
                        addname = New AdditionalName_Item
                        addname.DisplayName = ExtractElementFromSegment("LName", RenterSegment) & ", " & ExtractElementFromSegment("FName", RenterSegment)
                        addname.CID = ExtractElementFromSegment("ID", RenterSegment)
                        AdditionalNames_List.Insert(1, addname)

                    ElseIf ExtractElementFromSegment("FName", RenterSegment) = "" And ExtractElementFromSegment("LName", RenterSegment) <> "" Then
                        addname = New AdditionalName_Item
                        addname.DisplayName = ExtractElementFromSegment("LName", RenterSegment)
                        addname.CID = ExtractElementFromSegment("ID", RenterSegment)
                        AdditionalNames_List.Insert(1, addname)

                    ElseIf ExtractElementFromSegment("FName", RenterSegment) <> "" And ExtractElementFromSegment("LName", RenterSegment) = "" Then
                        addname = New AdditionalName_Item
                        addname.DisplayName = ExtractElementFromSegment("FName", RenterSegment)
                        addname.CID = ExtractElementFromSegment("ID", RenterSegment)
                        AdditionalNames_List.Insert(1, addname)

                    End If
                End If
            End If



            'Save List of Names to Database
            For Each item In AdditionalNames_List
                SQL = "INSERT INTO MBXNamesList ([MBX], [CID], [ALPHA], [Name], [FormOfID1], [ID1_Type], [ID1_IssuingEntity], [ID1_ExpDate], [FormOfID2], [ID2_Type]) 
VALUES (" & mbx_no & ", " & item.CID & ", '" & item.DisplayName.Substring(0, 1) & "', '" & item.DisplayName.Replace("'", "''") & "', '" &
ExtractElementFromSegment("FormOfID1", item.MBXNamesList_Segment, "") & "', '" &
ExtractElementFromSegment("ID1_Type", item.MBXNamesList_Segment, "") & "', '" &
ExtractElementFromSegment("ID1_IssuingEntity", item.MBXNamesList_Segment, "") & "', '" &
ExtractElementFromSegment("ID1_ExpDate", item.MBXNamesList_Segment, "") & "', '" &
ExtractElementFromSegment("FormOfID2", item.MBXNamesList_Segment, "") & "', '" &
ExtractElementFromSegment("ID2_Type", item.MBXNamesList_Segment, "") & "')"



                IO_UpdateSQLProcessor(gShipriteDB, SQL)

                'update contacts table
                IO_UpdateSQLProcessor(gShipriteDB, "Update Contacts set MBX=True Where ID=" & item.CID)
            Next

            Additional_Names_LV.Items.Refresh()

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error updating MBXNamesList Table.")
        End Try
    End Sub


    Private Sub Delete_AdditionalName_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Delete_AdditionalName_Btn.Click
        Try
            If Additional_Names_LV.SelectedIndex <> -1 Then
                AdditionalNames_List.RemoveAt(Additional_Names_LV.SelectedIndex)
                Additional_Names_LV.Items.Refresh()
            End If

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error deleting Additional Name")
        End Try
    End Sub

#End Region



#Region "1583 form"
    Private Sub PS1583_Names_CmbBx_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles PS1583_Names_CmbBx.SelectionChanged
        Dim index As Integer = PS1583_Names_CmbBx.SelectedIndex

        If index = -1 Then Exit Sub

        Load_PS1583_Items(AdditionalNames_List(index).CID)
    End Sub

    Private Sub Load_PS1583_Items(ByVal CID As String)
        Try
            Dim segment As String
            Dim index As Integer
            PS1583_Names_CmbBx.ItemsSource = AdditionalNames_List
            PS1583_Names_CmbBx.Items.Refresh()

            'make sure that the main contact tied to the mailbox is selected by default
            index = AdditionalNames_List.FindIndex(Function(x) x.CID = CID)
            If index = -1 Then Exit Sub

            PS1583_Names_CmbBx.SelectedIndex = index
            segment = AdditionalNames_List(index).MBXNamesList_Segment

            ID_1_TextBox.Text = ExtractElementFromSegment(ID_1_TextBox.Tag, segment)
            ID_1_Type_CmbBx.SelectedIndex = ExtractElementFromSegment(ID_1_Type_CmbBx.Tag, segment, "-1")
            ID_1_IssuingEntity_TextBox.Text = ExtractElementFromSegment(ID_1_IssuingEntity_TextBox.Tag, segment)
            ID_1_ExpDate_TextBox.Text = ExtractElementFromSegment(ID_1_ExpDate_TextBox.Tag, segment)
            ID_2_Type_CmbBx.SelectedIndex = ExtractElementFromSegment(ID_2_Type_CmbBx.Tag, segment, "-1")


        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Displaying PS1583 details")
        End Try
    End Sub

    Private Sub Update_1583_fields()
        Dim index As Integer = PS1583_Names_CmbBx.SelectedIndex
        Dim segment As String

        If index = -1 OrElse AdditionalNames_List.Count = 0 Then Exit Sub

        segment = AdditionalNames_List(index).MBXNamesList_Segment

        segment = AddElementToSegment(segment, "FormOfID1", ID_1_TextBox.Text)
        segment = AddElementToSegment(segment, "ID1_Type", ID_1_Type_CmbBx.SelectedIndex)
        segment = AddElementToSegment(segment, "ID1_IssuingEntity", ID_1_IssuingEntity_TextBox.Text)
        segment = AddElementToSegment(segment, "ID1_ExpDate", ID_1_ExpDate_TextBox.Text)
        'segment = AddElementToSegment(segment, "FormOfID2", "")
        segment = AddElementToSegment(segment, "ID2_Type", ID_2_Type_CmbBx.SelectedIndex)

        AdditionalNames_List(index).MBXNamesList_Segment = segment

        Debug.Print(AdditionalNames_List.ToString)
    End Sub

    Private Sub Print_1583Form(MbxNo As Integer)
        Try
            Cursor = Cursors.Wait
            Dim report As New _ReportObject()
            report.ReportName = "PS1583.rpt"

            report.ReportFormula = "{Mailbox.MailboxNumber} = " & MbxNo

            If AdditionalNames_List.Count > 0 Then
                report.ReportFormula &= " and ("

                For Each item In AdditionalNames_List
                    report.ReportFormula &= " {Contacts.ID} = " & item.CID & " or "

                Next

                report.ReportFormula = report.ReportFormula.Substring(0, report.ReportFormula.Length - 3)
                report.ReportFormula &= ")"
            End If


            Dim reportPrev As New ReportPreview(report)
            Cursor = Cursors.Arrow
            reportPrev.ShowDialog()

        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to print report [PS1583]...")
        Finally : Cursor = Cursors.Arrow
        End Try
    End Sub
#End Region

#Region "Maintnance"


    Private Sub ClearScreen()
        Try
            If POS_Call = "" Then
                Customer_TxtBox.Text = ""
                RenterSegment = ""
            End If
            MailboxNo_Label.Content = ""
            Save_Popup.IsOpen = False
            PrintAgreement_CheckBox.IsChecked = False
            Print1583_CheckBox.IsChecked = False
            TypeOfBusiness_TextBox.Text = ""
            IsBusiness_ChkBx.IsChecked = False
            PlaceOfRegistration_TextBox.Text = ""
            ID_1_TextBox.Text = ""
            ID_1_Type_CmbBx.SelectedIndex = -1
            ID_1_IssuingEntity_TextBox.Text = ""
            ID_1_ExpDate_TextBox.Text = ""

            ID_2_Type_CmbBx.SelectedIndex = -1
            'ID_2_Textbox.Text = ""

            OneMonth_Btn.IsChecked = False
            ThreeMonth_Btn.IsChecked = False
            SixMonth_Btn.IsChecked = False
            TvelweMonth_Btn.IsChecked = False
            CustomMonth_Btn.IsChecked = False


            EndDate.SelectedDate = Nothing
            Price_Label.Content = ""
            Search_TxtBox.Text = "Box# or Name"
            FreeMonths_ComboBox.SelectedIndex = 0


            If AdditionalNames_List IsNot Nothing Then
                AdditionalNames_List.Clear()
                Additional_Names_LV.Items.Refresh()
            End If

            FWD_Address_TxtBox.Text = ""
            FWD_Address_TxtBox.Tag = ""
            FWD_DepositAmount_TxtBox.Text = ""
            FWD_Never_RadioBtn.IsChecked = True
            FWD_Day_ComboBox.Text = ""
            FWD_Notes_TxtBox.Text = ""

            MBX_History_ListView.ItemsSource = Nothing
            MBX_History_ListView.Items.Clear()


        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error clearing Mailbox screen.")
        End Try
    End Sub


    Private Sub PrintButton_Click(sender As Object, e As RoutedEventArgs) Handles PrintButton.Click

        If Print_Popup.IsOpen = True Then
            Print_Popup.IsOpen = False
        Else
            Print_Popup.IsOpen = True
        End If

    End Sub

    Private Sub Fees_and_Deposits_Checked(sender As Object, e As RoutedEventArgs) Handles KeyDeposit_CheckBox.Checked, LateFee_CheckBox.Checked, AdminFee_CheckBox.Checked, OtherFee_CheckBox.Checked, KeyDeposit_CheckBox.Unchecked, LateFee_CheckBox.Unchecked, AdminFee_CheckBox.Unchecked, OtherFee_CheckBox.Unchecked
        'Calculate_Current_Price()
        Month_Button_Selected(Nothing, Nothing)
    End Sub

    Private Sub Expander_Expanded(sender As Object, e As RoutedEventArgs) Handles Pricing_Expander.Expanded, AdditionalNames_Expander.Expanded, Forwarding_Expander.Expanded, History_Expander.Expanded
        Try
            Dim CurrentExpander = DirectCast(sender, Expander)

            If CurrentExpander.Name = "Pricing_Expander" Then
                AdditionalNames_Expander.IsExpanded = False
                Forwarding_Expander.IsExpanded = False
                History_Expander.IsExpanded = False

                rowOne.Height = New GridLength(0.9, GridUnitType.Star)
                rowTwo.Height = New GridLength(0.1, GridUnitType.Star)
                rowThree.Height = New GridLength(0.1, GridUnitType.Star)
                rowFour.Height = New GridLength(0.1, GridUnitType.Star)

            ElseIf CurrentExpander.Name = "AdditionalNames_Expander" Then
                Pricing_Expander.IsExpanded = False
                Forwarding_Expander.IsExpanded = False
                History_Expander.IsExpanded = False

                rowOne.Height = New GridLength(0.1, GridUnitType.Star)
                rowTwo.Height = New GridLength(0.9, GridUnitType.Star)
                rowThree.Height = New GridLength(0.1, GridUnitType.Star)
                rowFour.Height = New GridLength(0.1, GridUnitType.Star)

            ElseIf CurrentExpander.Name = "Forwarding_Expander" Then
                Pricing_Expander.IsExpanded = False
                AdditionalNames_Expander.IsExpanded = False
                History_Expander.IsExpanded = False

                rowOne.Height = New GridLength(0.1, GridUnitType.Star)
                rowTwo.Height = New GridLength(0.1, GridUnitType.Star)
                rowThree.Height = New GridLength(0.9, GridUnitType.Star)
                rowFour.Height = New GridLength(0.1, GridUnitType.Star)

            Else
                Pricing_Expander.IsExpanded = False
                AdditionalNames_Expander.IsExpanded = False
                Forwarding_Expander.IsExpanded = False

                rowOne.Height = New GridLength(0.1, GridUnitType.Star)
                rowTwo.Height = New GridLength(0.1, GridUnitType.Star)
                rowThree.Height = New GridLength(0.1, GridUnitType.Star)
                rowFour.Height = New GridLength(0.9, GridUnitType.Star)

            End If

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Displaying selection.")
        End Try
    End Sub

    Private Sub Expander_Collapsed(sender As Object, e As RoutedEventArgs) Handles Pricing_Expander.Collapsed, AdditionalNames_Expander.Collapsed, Forwarding_Expander.Collapsed, History_Expander.Collapsed
        Try
            rowOne.Height = New GridLength(0.1, GridUnitType.Star)
            rowTwo.Height = New GridLength(0.1, GridUnitType.Star)
            rowThree.Height = New GridLength(0.1, GridUnitType.Star)
            rowFour.Height = New GridLength(0.1, GridUnitType.Star)

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Displaying selection.")
        End Try
    End Sub

    Private Sub Search_TxtBox_LostFocus(sender As Object, e As RoutedEventArgs) Handles Search_TxtBox.LostFocus
        Try
            If Search_TxtBox.Text = "" Then
                Search_TxtBox.Text = "Box# or Name"
            End If

            DisplayMailboxes()
        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error.")
        End Try
    End Sub

    Private Sub Search_TxtBox_KeyDown(sender As Object, e As KeyEventArgs) Handles Search_TxtBox.KeyDown
        Try

            If e.Key = Key.Enter Then
                'DisplayMailboxes()
                Mailboxes_Control.Focus()
            End If

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error.")
        End Try
    End Sub

    Private Sub Search_TxtBox_GotFocus(sender As Object, e As RoutedEventArgs) Handles Search_TxtBox.GotFocus
        Try
            Search_TxtBox.Text = ""

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error.")
        End Try
    End Sub

    Private Sub TextBox_GotFocus(sender As Object, e As RoutedEventArgs)
        Try
            Dim CurrentPrice_TxtBox = DirectCast(sender, TextBox)

            If CurrentPrice_TxtBox.Text = "$0.00" Then
                CurrentPrice_TxtBox.Text = ""
            End If

            If CurrentPrice_TxtBox.Text.Contains("$") Then
                CurrentPrice_TxtBox.Text = CurrentPrice_TxtBox.Text.Remove(0, 1)
            End If

            CurrentPrice_TxtBox.SelectAll()

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error.")
        End Try
    End Sub

    Private Sub TextBox_LostFocus(sender As Object, e As RoutedEventArgs)
        Try
            Dim CurrentPrice_TxtBox = DirectCast(sender, TextBox)

            If CurrentPrice_TxtBox.Text <> "" Then
                CurrentPrice_TxtBox.Text = FormatCurrency(CurrentPrice_TxtBox.Text)
            Else
                CurrentPrice_TxtBox.Text = FormatCurrency(0)
            End If

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error.")
        End Try
    End Sub

    Private Sub Print1583_CheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles Print1583_CheckBox.Checked
        Try
            Form1583_Border.Visibility = Visibility.Visible

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error.")
        End Try
    End Sub

    Private Sub Print1583_CheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles Print1583_CheckBox.Unchecked
        Try
            Form1583_Border.Visibility = Visibility.Hidden

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error.")
        End Try
    End Sub

    Private Sub SaveButton_Click(sender As Object, e As RoutedEventArgs) Handles SaveButton.Click
        Try

            If Save_Popup.IsOpen = True Then
                Save_Popup.IsOpen = False
            Else
                Save_Popup.IsOpen = True
            End If

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error.")
        End Try
    End Sub

    Private Sub RateType_ComboBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles RateType_ComboBox.SelectionChanged
        Try
            If RateType_ComboBox.SelectedIndex = 3 Then
                'Custom Rates
                Dim Prices As String = Get_CustomRates()
                If Prices = "" Then
                    'No Rates Setup
                    If gIsProgramSecurityEnabled Or gIsPOSSecurityEnabled Then
                        If Not Check_Current_User_Permission("Setup_Mailbox") Then
                            MsgBox("You do NOT have permission to access this feature!", vbExclamation)
                            RateType_ComboBox.SelectedIndex = 0
                            Exit Sub
                        End If
                    End If

                    CustomRates_Popup.IsOpen = True


                Else
                    'Custom Rates already setup
                    GetMailboxPricing()
                End If

            Else
                GetMailboxPricing()
            End If

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error changing Rate Type")
        End Try

    End Sub

    Private Sub FreeMonths_ComboBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles FreeMonths_ComboBox.SelectionChanged
        Try
            If EndDate.SelectedDate Is Nothing Then
                Exit Sub
            End If

            Dim dat As Date = EndDate.SelectedDate

            EndDate.SelectedDate = dat.AddMonths(FreeMonths_ComboBox.SelectedValue)

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error selecting Free Months option.")
        End Try
    End Sub

    Private Sub ExpireEndOfMonth_CheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles ExpireEndOfMonth_CheckBox.Checked
        If IsNothing(EndDate.SelectedDate) Then
            Exit Sub
        End If

        Month_Button_Selected(Nothing, Nothing)

    End Sub

    Private Sub ExpireEndOfMonth_CheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles ExpireEndOfMonth_CheckBox.Unchecked
        Month_Button_Selected(Nothing, Nothing)
    End Sub

    Private Sub All_Panels_Btn_Click(sender As Object, e As RoutedEventArgs) Handles All_Panels_Btn.Click
        Try
            MBX_Panels_ListBox.UnselectAll()
            All_Panels_Btn.Background = New System.Windows.Media.SolidColorBrush(FindResource(SystemColors.GradientActiveCaptionColorKey))

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error selecting Panels.")
        End Try
    End Sub

#End Region

#Region "Custom Rates"
    Private Sub MBX_Prices_GotFocus(sender As Object, e As RoutedEventArgs) Handles CustomPrice_One_Month_TxtBox.GotFocus, CustomPrice_Three_Month_TxtBox.GotFocus, CustomPrice_Six_Month_TxtBox.GotFocus, CustomPrice_Twelve_Month_TxtBox.GotFocus
        Try
            Dim CurrentPrice_TxtBox = DirectCast(sender, System.Windows.Controls.TextBox)

            If CurrentPrice_TxtBox.Text = "$0.00" Then
                CurrentPrice_TxtBox.Text = ""
            End If

            If CurrentPrice_TxtBox.Text.Contains("$") Then
                CurrentPrice_TxtBox.Text = CurrentPrice_TxtBox.Text.Replace("$", "")
            End If

            CurrentPrice_TxtBox.SelectAll()

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error - Mailbox Custom Rates")
        End Try
    End Sub

    Private Sub MBX_Prices_LostFocus(sender As Object, e As RoutedEventArgs) Handles CustomPrice_One_Month_TxtBox.LostFocus, CustomPrice_Three_Month_TxtBox.LostFocus, CustomPrice_Six_Month_TxtBox.LostFocus, CustomPrice_Twelve_Month_TxtBox.LostFocus
        Try
            Dim CurrentPrice_TxtBox = DirectCast(sender, System.Windows.Controls.TextBox)

            If CurrentPrice_TxtBox.Text <> "" And IsNumeric(CurrentPrice_TxtBox.Text) Then
                CurrentPrice_TxtBox.Text = FormatCurrency(CurrentPrice_TxtBox.Text,,, TriState.False)
            Else
                CurrentPrice_TxtBox.Text = FormatCurrency(0)
            End If

            _3Month.Text = FormatCurrency(CDbl(CustomPrice_Three_Month_TxtBox.Text) / 3, 2)

            _6Month.Text = FormatCurrency(CDbl(CustomPrice_Six_Month_TxtBox.Text) / 6, 2)

            _12Month.Text = FormatCurrency(CDbl(CustomPrice_Twelve_Month_TxtBox.Text) / 12, 2)

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error - Mailbox Custom Rates")
        End Try
    End Sub

    Private Sub CustomRates_Save_Btn_Click(sender As Object, e As RoutedEventArgs) Handles CustomRates_Save_Btn.Click
        Try
            Dim SQL As String
            Dim ret As Integer
            Dim SavePricing As String = CDbl(CustomPrice_One_Month_TxtBox.Text) & " " & CDbl(CustomPrice_Three_Month_TxtBox.Text) & " " & CDbl(CustomPrice_Six_Month_TxtBox.Text) & " " & CDbl(CustomPrice_Twelve_Month_TxtBox.Text)


            SQL = "Update Mailbox Set CustomRates='" & SavePricing & "' Where MailboxNumber= " & MailboxNo_Label.Content
            ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

            If ret = 0 Then
                'new mailbox, could not save pricing to non-existant box
                CustomPricing = SavePricing
            End If

            CustomRates_Popup.IsOpen = False
            Apply_Custom_Rates()
            Calculate_Current_Price()

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error - Saving Mailbox Custom Rates")
        End Try
    End Sub

    Private Sub CustomRates_Cancel_Btn_Click(sender As Object, e As RoutedEventArgs) Handles CustomRates_Cancel_Btn.Click
        CustomRates_Popup.IsOpen = False
    End Sub

    Private Sub CustomRates_Delete_Btn_Click(sender As Object, e As RoutedEventArgs) Handles CustomRates_Delete_Btn.Click
        Try
            Dim SQL As String = "Update Mailbox Set CustomRates='' Where MailboxNumber= " & MailboxNo_Label.Content

            IO_UpdateSQLProcessor(gShipriteDB, SQL)

            CustomRates_Popup.IsOpen = False

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error - Deleting Mailbox Custom Rates")
        End Try
    End Sub

    Private Function Get_CustomRates() As String
        Try

            If IsRenewal() Then
                Dim Prices As String = ""
                Dim SQL As String = "Select CustomRates from Mailbox Where MailboxNumber= " & MailboxNo_Label.Content

                Prices = ExtractElementFromSegment("CustomRates", IO_GetSegmentSet(gShipriteDB, SQL))


                Return Prices
            Else
                Return CustomPricing
            End If

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Obtaining Mailbox Custom Rates")
        End Try
    End Function

    Private Sub Apply_Custom_Rates()
        Try
            Dim Pricing As String = Get_CustomRates()

            If Pricing = "" Then Exit Sub

            Dim Prices As List(Of String) = New List(Of String)
            Prices = Pricing.Split(" ").ToList

            OneMonthPrice_TxtBlock.Text = FormatCurrency(Prices(0))
            ThreeMonthPrice_TxtBlock.Text = FormatCurrency(Val(Prices(1)) / 3)
            SixMonthPrice_TxtBlock.Text = FormatCurrency(Val(Prices(2)) / 6)
            TvelweMonthPrice_TxtBlock.Text = FormatCurrency(Val(Prices(3)) / 12)


            OneMonth_Btn.ToolTip = FormatCurrency(Val(Prices(0)))
            ThreeMonth_Btn.ToolTip = FormatCurrency(Val(Prices(1)))
            SixMonth_Btn.ToolTip = FormatCurrency(Val(Prices(2)))
            TvelweMonth_Btn.ToolTip = FormatCurrency(Val(Prices(3)))

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Applying Mailbox Custom Rates")
        End Try
    End Sub

    Private Sub Edit_CustomRates_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Edit_CustomRates_Btn.Click
        If gIsProgramSecurityEnabled Or gIsPOSSecurityEnabled Then
            If Not Check_Current_User_Permission("Setup_Mailbox") Then
                Exit Sub
            End If
        End If

        CustomRates_Popup.IsOpen = True
    End Sub


#End Region

#Region "Notifications"
    Private Sub Notifications_Button_Click(sender As Object, e As RoutedEventArgs) Handles Notifications_Button.Click

        If Notification_Popup.IsOpen = True Then
            Notification_Popup.IsOpen = False
        Else
            Notification_Popup.IsOpen = True
        End If

    End Sub

    Private Sub DailyNotice_Btn_Click(sender As Object, e As RoutedEventArgs) Handles DailyNotice_Btn.Click
        Try


            Notification_Popup.IsOpen = False
            Dim win As New MailboxNotifications(Me)
            win.ShowDialog(Me)


        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error setting Daily Notice options")
        End Try
    End Sub


#End Region

#Region "Print Notices"
    Private Sub PrintAgreement_Btn_Click(sender As Object, e As RoutedEventArgs) Handles PrintAgreement_Btn.Click
        If MailboxNo_Label.Content.ToString <> "" Then
            Print_Agreement(MailboxNo_Label.Content)
        End If
    End Sub

    Private Sub Print_PS1583_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Print_PS1583_Btn.Click
        If MailboxNo_Label.Content.ToString <> "" Then
            Print_1583Form(MailboxNo_Label.Content)
        End If

    End Sub

    Private Sub Print_Expiration_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Print_Expiration_Btn.Click
        If MailboxNo_Label.Content.ToString <> "" Then
            MailboxNotice.Print_Notice(CInt(MailboxNo_Label.Content), MailboxNoticeType.Expiration)
        End If
    End Sub

    Private Sub Print_Renewal_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Print_Renewal_Btn.Click
        If MailboxNo_Label.Content.ToString <> "" Then
            MailboxNotice.Print_Notice(CInt(MailboxNo_Label.Content), MailboxNoticeType.Renewal)
        End If
    End Sub

    Private Sub Print_Cancellation_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Print_Cancellation_Btn.Click
        If MailboxNo_Label.Content.ToString <> "" Then
            MailboxNotice.Print_Notice(CInt(MailboxNo_Label.Content), MailboxNoticeType.Cancellation)
        End If
    End Sub

#End Region

#Region "Email Notices"
    Private Sub Email_Renewal_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Email_Renewal_Btn.Click
        EmailNotice(MailboxNoticeType.Renewal)
    End Sub

    Private Sub Email_Expiration_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Email_Expiration_Btn.Click
        EmailNotice(MailboxNoticeType.Expiration)
    End Sub

    Private Sub Email_Cancellation_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Email_Cancellation_Btn.Click
        EmailNotice(MailboxNoticeType.Cancellation)
    End Sub

    Private Sub EmailNotice(NoticeType As MailboxNoticeType)
        Try
            Dim MbxNo As String = MailboxNo_Label.Content.ToString()

            If MbxNo <> "" Then
                Dim mbx As Mailbox = Mailbox_List.Find(Function(x As Mailbox) x.Number = CInt(MbxNo))
                If Not MailboxNotice.Email_Notice(mbx, NoticeType) Then
                    If gResult = "NoEmail" Then
                        MsgBox("Mailbox# " & mbx.Number & vbCrLf & mbx.Name & vbCrLf & vbCrLf & NoticeType.ToString & " Notice could not be emailed!" & vbCrLf & "Please ensure that renter has an email address entered!", vbExclamation, "Cannot Send Email")
                    Else
                        MsgBox("Mailbox# " & mbx.Number & vbCrLf & mbx.Name & vbCrLf & vbCrLf & NoticeType.ToString & " Notice could not be emailed!", vbExclamation, "Cannot Send Email")
                    End If
                    gResult = ""
                End If
            End If

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Emailing Notice")
        End Try
    End Sub


    Private Sub Renewal_Bulk(sender As Object, e As RoutedEventArgs) Handles Renewal_BulkEmail.Click
        Dim Renewal_List As List(Of Mailbox)

        Renewal_List = Load_Mbx_List("SELECT MailboxNumber, Name, CID, EndDate, Business, CustomRates FROM Mailbox WHERE #" & Today.Date & "# >= [EndDate] + " & GetPolicyData(gShipriteDB, "MBXDaysRenewal") & " AND [RenewalSent] = False AND [Rented] = True ORDER BY Mailbox.MailboxNumber")
        MailboxNotice.Email_Notices(Renewal_List, MailboxNoticeType.Renewal)
    End Sub

    Private Sub Expiration_Bulk(sender As Object, e As RoutedEventArgs) Handles Expiration_BulkEmail.Click
        Dim Expiration_List As List(Of Mailbox)

        Expiration_List = Load_Mbx_List("SELECT MailboxNumber, Name, CID, EndDate, Business, CustomRates FROM Mailbox WHERE #" & Today.Date & "# >= [EndDate] + " & GetPolicyData(gShipriteDB, "MBXDaysExpire") & " AND [ExpiredSent] = False AND [Rented] = True ORDER BY Mailbox.MailboxNumber")
        MailboxNotice.Email_Notices(Expiration_List, MailboxNoticeType.Expiration)
    End Sub

    Private Sub Cancel_Bulk(sender As Object, e As RoutedEventArgs) Handles Cancel_BulkEmail.Click
        Dim Cancellation_List As List(Of Mailbox)

        Cancellation_List = Load_Mbx_List("SELECT MailboxNumber, Name, CID, EndDate, Business, CustomRates FROM Mailbox WHERE #" & Today.Date & "# >= [EndDate] + " & GetPolicyData(gShipriteDB, "MBXDaysCancel") & " AND [CanceledSent] = False AND [Rented] = True ORDER BY Mailbox.MailboxNumber")
        MailboxNotice.Email_Notices(Cancellation_List, MailboxNoticeType.Cancellation)
    End Sub

#End Region

#Region "ShortcutKeyHandler"
    Private Sub Window_KeyDown(sender As Object, e As KeyEventArgs)
        ShortcutKeyHandlers.KeyDown(sender, e, Me)
    End Sub

    Private Sub Forwarding_Address_Button_Click(sender As Object, e As RoutedEventArgs) Handles Forwarding_Address_Button.Click
        gAutoExitFromContacts = True
        Dim win As New ContactManager(Me)
        win.ShowDialog(Me)

        If Not gContactManagerSegment = "" Then
            FWD_Address_TxtBox.Text = CreateDisplayBlock(gContactManagerSegment, False)
            FWD_Address_TxtBox.Tag = ExtractElementFromSegment("ID", gContactManagerSegment)
        End If
    End Sub

    Private Sub IsBusiness_ChkBx_Checked(sender As Object, e As RoutedEventArgs) Handles IsBusiness_ChkBx.Checked, IsBusiness_ChkBx.Unchecked
        If IsBusiness_ChkBx.IsChecked Then
            TypeOfBusiness_TextBox.Visibility = Visibility.Visible
            TypeOfBusiness_Lbl.Visibility = Visibility.Visible

            PlaceOfRegistration_TextBox.Visibility = Visibility.Visible
            PlaceOfRegistration_Lbl.Visibility = Visibility.Visible
        Else
            TypeOfBusiness_TextBox.Visibility = Visibility.Hidden
            TypeOfBusiness_Lbl.Visibility = Visibility.Hidden

            PlaceOfRegistration_TextBox.Visibility = Visibility.Hidden
            PlaceOfRegistration_Lbl.Visibility = Visibility.Hidden

        End If
    End Sub



    Private Function Load_Mbx_List(ByRef SQL As String) As List(Of Mailbox)
        Try
            Dim Buffer As String
            Dim current_segment As String
            Dim Notice As Mailbox
            Dim NoticeList As List(Of Mailbox) = New List(Of Mailbox)

            Buffer = IO_GetSegmentSet(gShipriteDB, SQL)
            Debug.Print(SQL)

            Do Until Buffer = ""
                current_segment = GetNextSegmentFromSet(Buffer)

                Notice = New Mailbox(current_segment)

                NoticeList.Add(Notice)
            Loop

            Return NoticeList

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Saving Mailbox History")
            Return Nothing
        End Try
    End Function

#End Region

End Class