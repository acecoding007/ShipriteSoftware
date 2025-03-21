Imports System.Windows.Forms
Public Class POS_ButtonMaker
    Inherits CommonWindow

    Dim Current_Button As System.Windows.Controls.Button
    Dim ID As Long
    Dim Current_Group As String
    Private isButtonSaved As Boolean
    Dim Last_Column_Sorted As String
    Dim Last_Sort_Ascending As Boolean

    Public ReadOnly Property IsPosButtonSaved As Boolean
        Get
            Return isButtonSaved
        End Get
    End Property

    Public Sub New()

        MyBase.New()

        ' This call is required by the designer.
        InitializeComponent()

    End Sub

    Public Sub New(ByVal callingWindow As Window, Optional ByRef Btn As System.Windows.Controls.Button = Nothing, Optional ByVal grp As String = "")

        MyBase.New(callingWindow)

        ' This call is required by the designer.
        InitializeComponent()
        Current_Button = Btn
        Current_Group = grp

    End Sub

    Private Sub Display_Button_Details(header_Visible As Visibility,
                                       header_text As String,
                                       SKU_Text_Visible As Visibility,
                                       SKU_Text As String,
                                       SKUBox_Text_Visible As Visibility,
                                       SKUBox_Text As String,
                                       SearchButton_Visible As Visibility,
                                       Desc_Text_Visible As Visibility,
                                       Desc_Text As String,
                                       QTY_Visible As Visibility,
                                       QTY_Text As String,
                                       Group_Visible As Visibility,
                                       Group_text As String,
                                       Button_Caption As String)

        Try

            '--- SKU and Description objects-------------------------

            Instructions_Label.Visibility = header_Visible
            Instructions_Label.Content = header_text

            SKUHeader_Label.Visibility = SKU_Text_Visible
            SKUHeader_Label.Content = SKU_Text

            SKU_Label.Visibility = SKUBox_Text_Visible
            SKU_Label.Content = SKUBox_Text


            If SKUBox_Text_Visible = Visibility.Visible Then  'Hidden Entry textbox to enter in a new Group Panel
                GroupName_TxtBox.Visibility = Visibility.Hidden
            Else
                GroupName_TxtBox.Visibility = Visibility.Visible
            End If


            Search_Button.Visibility = SearchButton_Visible

            SKUDescHeader_Label.Visibility = Desc_Text_Visible
            SKUDescHeader_Label.Content = Desc_Text
            SKUDesc_Label.Visibility = Desc_Text_Visible

            '--- Quantity Objects--------------------------------------
            Quantity_Label.Visibility = QTY_Visible
            Quantity_TextBox.Visibility = QTY_Visible
            Quantity_Desc_Label.Visibility = QTY_Visible

            Quantity_Desc_Label.Content = QTY_Text



            '----Drop Down/Group Objects-------------------------------------------------------
            Group_Label.Visibility = Group_Visible
            Group_ComboBox.Visibility = Group_Visible
            Group_Label.Content = Group_text



            ButtonCaption_TextBox.Text = Button_Caption


        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try

    End Sub

    Private Sub SortBy_ListBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles ButtonType_ListBox.SelectionChanged

        Try

            Select Case ButtonType_ListBox.SelectedIndex

                Case 0
                    Description_Label.Text = "SKU" + vbCrLf + "Create a Button to ring up a Single SKU item from inventory."

                    Display_Button_Details(Visibility.Visible, "Click Search Button and Select Item SKU From Inventory", Visibility.Visible, "SKU", Visibility.Visible, "", Visibility.Visible, Visibility.Visible, "Description", Visibility.Visible, "Enter Default Order Quantity." + vbCrLf + "Enter '?' to ask for Quantity at time of sale." + vbCrLf + "Blank entry defaults to Quantity of 1.", Visibility.Hidden, "", "")


                Case 1
                    Description_Label.Text = "GROUP BUTTON" + vbCrLf + "Create a new panel/group of 20 blank buttons."

                    Display_Button_Details(Visibility.Visible, "Enter in a new Group Name or select Existing Group!", Visibility.Visible, "Group Name", Visibility.Hidden, "", Visibility.Hidden, Visibility.Hidden, "", Visibility.Hidden, "", Visibility.Visible, "Select Existing Group!", "")


                Case 2
                    Description_Label.Text = "GO BACK" + vbCrLf + "Create a button that will return to and display the Main button panel. Have one GO BACK button in each button group that is created."
                    Display_Button_Details(Visibility.Hidden, "", Visibility.Visible, "SKU", Visibility.Visible, "Main", Visibility.Hidden, Visibility.Hidden, "", Visibility.Hidden, "", Visibility.Hidden, "", "Go Back")


                Case 3
                    Description_Label.Text = "SHIP ONE" + vbCrLf + "Ship One button will open the Shipping screen with 'Other' packaging preselected."
                    Display_Button_Details(Visibility.Hidden, "", Visibility.Visible, "SKU", Visibility.Visible, "Ship1", Visibility.Hidden, Visibility.Hidden, "", Visibility.Hidden, "", Visibility.Hidden, "", "Ship One")

                Case 4
                    Description_Label.Text = "SHIP MULTIPLE" + vbCrLf + "Ship Multi button will open the Shipping screen. After a package is processed the program will not return to POS, instead it will remain in the Ship screen to process the next package! Use this option when processing multiple pacakges for the same customer."
                    Display_Button_Details(Visibility.Hidden, "", Visibility.Visible, "SKU", Visibility.Visible, "Shipm", Visibility.Hidden, Visibility.Hidden, "", Visibility.Hidden, "", Visibility.Hidden, "", "Ship Multi")

                Case 5
                    Description_Label.Text = "SHIP LETTER" + vbCrLf + "Ship Letter button will open the Shipping screen with 'Letter' packaging preselected. Use this button when processing an express envelope for any carrier."
                    Display_Button_Details(Visibility.Hidden, "", Visibility.Visible, "SKU", Visibility.Visible, "Shipl", Visibility.Hidden, Visibility.Hidden, "", Visibility.Hidden, "", Visibility.Hidden, "", "Ship Letter")

                Case 6
                    Description_Label.Text = "SHIP OTHER PACKAGING" + vbCrLf + "Ship Other Packaging will open the Shipping screen with a packaging of your choice prselected."
                    Display_Button_Details(Visibility.Hidden, "", Visibility.Visible, "SKU", Visibility.Visible, "ShipP", Visibility.Hidden, Visibility.Hidden, "", Visibility.Hidden, "", Visibility.Visible, "Select Type of Packing!", "Ship")

                Case 7
                    Description_Label.Text = "" + vbCrLf + ""
                    Display_Button_Details(Visibility.Hidden, "", Visibility.Hidden, "SKU", Visibility.Hidden, "", Visibility.Hidden, Visibility.Hidden, "", Visibility.Hidden, "", Visibility.Hidden, "", "")

                Case 8
                    Description_Label.Text = "MALIBOX RENTAL" + vbCrLf + "Opens the Mailbox Rental screen to rent a new Mailbox."
                    Display_Button_Details(Visibility.Hidden, "", Visibility.Visible, "SKU", Visibility.Visible, "MBX", Visibility.Hidden, Visibility.Hidden, "", Visibility.Hidden, "", Visibility.Hidden, "", "Mailbox Rental")

                Case 9
                    Description_Label.Text = "MAILBOX RENEWAL" + vbCrLf + "Opens the Mailbox Rental screen to renew an existing Mailbox."
                    Display_Button_Details(Visibility.Hidden, "", Visibility.Visible, "SKU", Visibility.Visible, "MBXR", Visibility.Hidden, Visibility.Hidden, "", Visibility.Hidden, "", Visibility.Hidden, "", "Mailbox Renewal")

                Case 10
                    Description_Label.Text = "MAILBOX MAINTNANCE" + vbCrLf + "Opens the Mailbox Rental screen to view and edit Mailboxes."
                    Display_Button_Details(Visibility.Hidden, "", Visibility.Visible, "SKU", Visibility.Visible, "MBXM", Visibility.Hidden, Visibility.Hidden, "", Visibility.Hidden, "", Visibility.Hidden, "", "Mailbox Maintnance")

                Case 11
                    Description_Label.Text = "" + vbCrLf + ""
                    Display_Button_Details(Visibility.Hidden, "", Visibility.Hidden, "SKU", Visibility.Hidden, "", Visibility.Hidden, Visibility.Hidden, "", Visibility.Hidden, "", Visibility.Hidden, "", "")

                Case 12
                    Description_Label.Text = "MAIL MASTER" + vbCrLf + "Opens the Mail Master screen to process US Mail and print DYMO Stamps."
                    Display_Button_Details(Visibility.Hidden, "", Visibility.Visible, "SKU", Visibility.Visible, "MailMaster", Visibility.Hidden, Visibility.Hidden, "", Visibility.Hidden, "", Visibility.Hidden, "", "Mail Master")

                Case 13
                    Description_Label.Text = "1ST CLASS MAIL" + vbCrLf + "Quick button to ring up weight specific 1st class Mail letter pricing."
                    Display_Button_Details(Visibility.Hidden, "", Visibility.Visible, "SKU", Visibility.Visible, "", Visibility.Hidden, Visibility.Hidden, "", Visibility.Hidden, "", Visibility.Visible, "Select Weight!", "1st Class Mail")

                Case 14
                    Description_Label.Text = "PACKMASTER" + vbCrLf + "Opens the PackMaster screen which allows for accurate pricing of pack jobs. PackMaster will pick the correct box, wrapping, filler, and labor for each pack job and add to the current sale."
                    Display_Button_Details(Visibility.Hidden, "", Visibility.Visible, "SKU", Visibility.Visible, "PackMaster", Visibility.Hidden, Visibility.Hidden, "", Visibility.Hidden, "", Visibility.Hidden, "", "PackMaster")

                Case 15
                    Description_Label.Text = "DROP OFF MANAGER" + vbCrLf + "Opens the Drop Off Manager which is used to log prepaid packages that are dropped off. Keeps track of all drop offs, prints a receipt, and prints a manifest at end of day"
                    Display_Button_Details(Visibility.Hidden, "", Visibility.Visible, "SKU", Visibility.Visible, "DOM", Visibility.Hidden, Visibility.Hidden, "", Visibility.Hidden, "", Visibility.Hidden, "", "Drop Off Manager")

                Case 16
                    Description_Label.Text = "" + vbCrLf + ""
                    Display_Button_Details(Visibility.Hidden, "", Visibility.Hidden, "SKU", Visibility.Hidden, "", Visibility.Hidden, Visibility.Hidden, "", Visibility.Hidden, "", Visibility.Hidden, "", "")

                Case 17
                    Description_Label.Text = "POS DISCOUNT" + vbCrLf + "Quick Button to apply a specific percentage discount to the running sale."
                    Display_Button_Details(Visibility.Hidden, "", Visibility.Visible, "SKU", Visibility.Visible, "%DISC%", Visibility.Hidden, Visibility.Hidden, "", Visibility.Visible, "Enter Discount Percentage!", Visibility.Hidden, "", "Discount")

                Case 18
                    Description_Label.Text = "SALES TAX CHANGE" + vbCrLf + "Quick Button to change the Sales Tax for the current sale to a specific percentage."
                    Display_Button_Details(Visibility.Hidden, "", Visibility.Visible, "SKU", Visibility.Visible, "%TAX%", Visibility.Hidden, Visibility.Hidden, "", Visibility.Visible, "Enter Tax Percentage!", Visibility.Hidden, "", "Change Sales Tax")

                Case 19
                    Description_Label.Text = "POS MEMO" + vbCrLf + "Add a predetermined memo/note to the current receipt."
                    Display_Button_Details(Visibility.Visible, "Enter in Memo message below!", Visibility.Visible, "Memo", Visibility.Hidden, "", Visibility.Hidden, Visibility.Hidden, "", Visibility.Hidden, "", Visibility.Hidden, "", "POS Memo")

                Case 20
                    Description_Label.Text = "" + vbCrLf + ""
                    Display_Button_Details(Visibility.Hidden, "", Visibility.Hidden, "SKU", Visibility.Hidden, "", Visibility.Hidden, Visibility.Hidden, "", Visibility.Hidden, "", Visibility.Hidden, "", "")

            End Select

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try

    End Sub

    Private Sub TextBox_PreviewTextInput(sender As Object, e As TextCompositionEventArgs)
        Try
            Dim allowedchars As String = "0123456789.?"
            If allowedchars.IndexOf(CChar(e.Text)) = -1 Then e.Handled = True

        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub

    Private Sub CustomButtonColor_Button_Click(sender As Object, e As RoutedEventArgs) Handles CustomButtonColor_Button.Click
        Dim cdialog As New ColorDialog()
        Dim mybrush As System.Windows.Media.Brush
        Dim existingBrush As System.Windows.Media.SolidColorBrush

        Try

            cdialog.FullOpen = True
            cdialog.AnyColor = True
            cdialog.ShowHelp = True

            'color dialog should open with the current color selected
            existingBrush = CustomButtonColor_Button.Background
            cdialog.Color = System.Drawing.Color.FromArgb(existingBrush.Color.R, existingBrush.Color.G, existingBrush.Color.B)


            'open color selection dialog
            If (cdialog.ShowDialog() = System.Windows.Forms.DialogResult.OK) Then
                mybrush = New System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(cdialog.Color.R, cdialog.Color.G, cdialog.Color.B))
                CustomButtonColor_Button.Background() = mybrush
                Preview_Button.Background() = mybrush
            End If


        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try

    End Sub

    Private Sub Color_Button_Click(sender As Object, e As RoutedEventArgs)
        Try

            Preview_Button.Background = sender.background


        Catch ex As Exception

            MessageBox.Show(Err.Description)

        End Try
    End Sub

    Private Sub ButtonCaption_TextBox_TextChanged(sender As Object, e As TextChangedEventArgs) Handles ButtonCaption_TextBox.TextChanged
        Try
            Preview_Button.Content = ButtonCaption_TextBox.Text


        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Button_TextColor_Click(sender As Object, e As RoutedEventArgs) Handles BT24.Click, BT25.Click
        Preview_Button.Foreground = sender.background
    End Sub

    Private Sub Custom_Text_Button_Click(sender As Object, e As RoutedEventArgs) Handles Custom_Text_Button.Click
        Try
            Dim cdialog As New ColorDialog()
            Dim mybrush As System.Windows.Media.Brush
            Dim existingBrush As System.Windows.Media.SolidColorBrush


            cdialog.FullOpen = True
            cdialog.AnyColor = True
            cdialog.ShowHelp = True

            'color dialog should open with the current color selected
            existingBrush = Custom_Text_Button.Background
            cdialog.Color = System.Drawing.Color.FromArgb(existingBrush.Color.R, existingBrush.Color.G, existingBrush.Color.B)


            'open color selection dialog
            If (cdialog.ShowDialog() = System.Windows.Forms.DialogResult.OK) Then
                mybrush = New System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(cdialog.Color.R, cdialog.Color.G, cdialog.Color.B))
                Custom_Text_Button.Background() = mybrush
                Preview_Button.Foreground() = mybrush
            End If

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub POS_ButtonMaker_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Try
            Dim SQL As String
            Dim SegmentSet As String
            Dim Segment As String
            Dim Buf As String

            If Current_Button IsNot Nothing Then
                Check_Incoming_Button()
            End If
            gPosButtonsTableSchema = IO_GetFieldsCollection(gShipriteDB, "PosButtons", "", True, False, True)
            isButtonSaved = False


            SQL = "SELECT SKU FROM POSButtons WHERE Type='Group'"
            SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
            Do Until SegmentSet = ""

                Segment = GetNextSegmentFromSet(SegmentSet)
                Buf = ExtractElementFromSegment("SKU", Segment)
                If Not Buf = "" Then
                    Group_ComboBox.Items.Add(Buf)
                End If

            Loop

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try

    End Sub

    Private Sub Check_Incoming_Button()
        Dim Type As String = ""
        Dim SKU As String = ""
        Dim Desc As String = ""
        Dim Qty As String = ""

        Try

            ID = Val(ExtractElementFromSegment("ID", Current_Button.Tag))
            Type = ExtractElementFromSegment("Type", Current_Button.Tag)
            SKU = ExtractElementFromSegment("SKU", Current_Button.Tag)
            Desc = ExtractElementFromSegment("Desc", Current_Button.Tag)
            Qty = ExtractElementFromSegment("Qty", Current_Button.Tag)

            If UCase(Type) = "GROUP" Then
                ButtonType_ListBox.SelectedIndex = 1
                GroupName_TxtBox.Text = SKU
            Else
                If SKU.Contains("CustomNote") Then
                    ButtonType_ListBox.SelectedIndex = 19
                Else
                    ButtonType_ListBox.SelectedIndex = 0
                End If

                SKU_Label.Content = SKU
            End If

            If SKU.Contains("CustomNote") Then
                GroupName_TxtBox.Text = Desc
            Else
                SKUDesc_Label.Content = Desc
            End If

            Quantity_TextBox.Text = Qty
            Preview_Button.Background = Current_Button.Background
            Preview_Button.Foreground = Current_Button.Foreground
            ButtonCaption_TextBox.Text = Current_Button.Content

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try

    End Sub

    Private Sub SaveButton_Click(sender As Object, e As RoutedEventArgs) Handles SaveButton.Click

        Dim Segment As String = ""
        Dim SQL As String = ""
        Dim ret As Long = 0
        Dim isUpdate As Boolean = (0 <> ID)

        If isUpdate Then
            Segment = AddElementToSegment(Segment, "ID", ID)
        End If

        If ButtonType_ListBox.SelectedIndex = 1 Then
            Segment = AddElementToSegment(Segment, "Type", "Group")
            If Group_ComboBox.Text = "" Then
                Segment = AddElementToSegment(Segment, "SKU", GroupName_TxtBox.Text)
            Else
                Segment = AddElementToSegment(Segment, "SKU", Group_ComboBox.Text)
            End If

        Else
            Segment = AddElementToSegment(Segment, "Type", "SKU")
            If ButtonType_ListBox.SelectedIndex = 19 Then
                'POS Note/Memo
                Segment = AddElementToSegment(Segment, "SKU", "CustomNote" & ID)
            Else
                Segment = AddElementToSegment(Segment, "SKU", SKU_Label.Content)
            End If

        End If

        If ButtonType_ListBox.SelectedIndex = 19 Then
            'POS Note/Memo
            Segment = AddElementToSegment(Segment, "Desc", GroupName_TxtBox.Text)
        Else
            Segment = AddElementToSegment(Segment, "Desc", SKUDesc_Label.Content)
        End If

        Segment = AddElementToSegment(Segment, "ButtonDesc", ButtonCaption_TextBox.Text)
        Segment = AddElementToSegment(Segment, "BN", Current_Button.Name.Substring(2))
        Segment = AddElementToSegment(Segment, "Group", Current_Group)
        Segment = AddElementToSegment(Segment, "Qty", Quantity_TextBox.Text)

        Dim lngForeColor As Long = POSManager.Brush_to_LongColor(Preview_Button.Foreground)
        Dim lngBackColor As Long = POSManager.Brush_to_LongColor(Preview_Button.Background)
        Segment = AddElementToSegment(Segment, "ForeColor", lngForeColor.ToString)
        Segment = AddElementToSegment(Segment, "BackColor", lngBackColor.ToString)

        If isUpdate Then
            SQL = MakeUpdateSQLFromSchema("PosButtons", Segment, gPosButtonsTableSchema,, True)
        Else
            SQL = MakeInsertSQLFromSchema("PosButtons", Segment, gPosButtonsTableSchema, True)
        End If

        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
        If ret > 0 Then
            isButtonSaved = True

            If isUpdate And ButtonType_ListBox.SelectedIndex = 1 Then
                'updating group name, need to update buttons associated with that group.
                Dim OldName As String = ExtractElementFromSegment("SKU", Current_Button.Tag)
                Dim NewName As String = ExtractElementFromSegment("SKU", Segment)
                SQL = "Update POSButtons set [Group]='" & NewName & "' WHERE [Group]='" & OldName & "'"

                IO_UpdateSQLProcessor(gShipriteDB, SQL)
            End If

            If ButtonType_ListBox.SelectedIndex = 19 And ID = 0 Then
                'NEW POS Note/Memo button, assign ID to SKU
                SQL = "UPDATE POSButtons set SKU='CustomNote' & [ID] WHERE SKU='CustomNote0'"
                IO_UpdateSQLProcessor(gShipriteDB, SQL)
            End If


            If isUpdate Then
                MessageBox.Show("POS Button Updated Successfully!", "POS Button Maker", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Else
                MessageBox.Show("POS Button Saved Successfully!", "POS Button Maker", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If

            Me.Close()
        End If

    End Sub

#Region "Inventory Search"
    Private Sub Search_Button_Click(sender As Object, e As RoutedEventArgs) Handles Search_Button.Click
        SKUSearch_Popup.IsOpen = True
        SKU_Search("")
        InventorySearch_TxtBx.Focus()
    End Sub

    Private Sub SKU_Search(SKU As String)
        Try
            Dim Segment As String = ""
            Dim SQL As String
            Dim SegmentSet As String
            Dim SearchList As List(Of SKUSearchItem)
            Dim item As SKUSearchItem

            If SKU = "" Then
                SQL = "SELECT SKU, Desc, Sell FROM Inventory"
            Else
                SQL = "SELECT SKU, Desc, Sell FROM Inventory WHERE SKU LIKE '" & SKU & "%' OR Desc LIKE '%" & SKU & "%'"
            End If

            SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)

            If SegmentSet <> "" Then
                SearchList = New List(Of SKUSearchItem)

                Do Until SegmentSet = ""
                    item = New SKUSearchItem
                    Segment = GetNextSegmentFromSet(SegmentSet)

                    item.SKU = ExtractElementFromSegment("SKU", Segment)
                    item.Description = ExtractElementFromSegment("Desc", Segment)
                    item.Price = ExtractElementFromSegment("Sell", Segment, "0")

                    SearchList.Add(item)
                Loop

                'SKUSearch_LV.Focus()
                SKUSearch_LV.ItemsSource = SearchList
                SKUSearch_LV.Items.Refresh()
                SKUSearch_LV.SelectedIndex = 0
            End If

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error in POS SKU Search.")
        End Try
    End Sub


    Private Sub ColumnHeader_Click(sender As Object, e As RoutedEventArgs)
        Try
            'Sorts ListView by clicked Column Header
            Sort_LV_byColumn(sender, TryCast(e.OriginalSource, GridViewColumnHeader), Last_Sort_Ascending, Last_Column_Sorted)

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error sorting Column Header.")
        End Try
    End Sub

    Private Sub InventorySearch_TxtBx_TextChanged(sender As Object, e As TextChangedEventArgs) Handles InventorySearch_TxtBx.TextChanged
        SKU_Search(InventorySearch_TxtBx.Text)
    End Sub

    Private Sub SKUSearch_Cancel_Btn_Click(sender As Object, e As RoutedEventArgs) Handles SKUSearch_Cancel_Btn.Click
        SKUSearch_Popup.IsOpen = False
    End Sub

    Private Sub SKUSearch_Select_Btn_Click(sender As Object, e As RoutedEventArgs) Handles SKUSearch_Select_Btn.Click
        SKUSearch_SelectSKU()

    End Sub

    Private Sub SKUSearch_LV_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles SKUSearch_LV.MouseDoubleClick
        SKUSearch_SelectSKU()
    End Sub

    Private Sub SKUSearch_LV_KeyDown(sender As Object, e As Input.KeyEventArgs) Handles SKUSearch_LV.KeyDown

        If e.Key = Key.Return Then
            SKUSearch_SelectSKU()
        End If
    End Sub

    Private Sub SKUSearch_SelectSKU()
        If SKUSearch_LV.SelectedIndex = -1 Then Exit Sub

        SKU_Label.Content = SKUSearch_LV.SelectedItem.SKU
        SKUDesc_Label.Content = SKUSearch_LV.SelectedItem.Description
        ButtonCaption_TextBox.Text = SKUSearch_LV.SelectedItem.Description
        SKUSearch_Popup.IsOpen = False
    End Sub

#End Region
End Class
