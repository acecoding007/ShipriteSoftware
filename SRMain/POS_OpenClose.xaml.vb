Public Class POS_OpenClose
    Inherits CommonWindow

    Private Class ClosingPayment_ListItem
        Public Property InvNum As String
        Public Property Amount As Double
        Public Property Customer_Name As String
        Public Property Description As String
    End Class

    Private OpenSegment As String = ""
    Private CashPayment_List As List(Of ClosingPayment_ListItem)
    Private CheckPayment_list As List(Of ClosingPayment_ListItem)
    Private OtherPayment_list As List(Of ClosingPayment_ListItem)
    Private CCPayment_List As List(Of ClosingPayment_ListItem)
    Private PaidOut_List As List(Of ClosingPayment_ListItem)




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

    Private Sub POS_OpenClose_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded

        Dim SQL As String
        Dim SegmentSet As String
        Dim buf As String = ""
        Dim oCash As Double = 0
        Dim sCash As Double = 0
        Dim pCash As Double = 0
        Dim eCash As Double = 0

        If gIsPOSSecurityEnabled Then
            Dim win As New UserLogIn(Me, "POSManager")
            win.ShowDialog()

            If UserLogIn.isAllowed = False Then
                Me.Close()
            End If
        End If

        DrawerID.Text = gDrawerID
        TodaysDate.Content = Format(Today, "dddd" & vbCrLf & "MMMM dd, yyyy")
        Header.Content = gResult & " Procedure"
        If gResult = "Open" Then

            Header.Content = "OPEN DRAWER"
            CashOverAndShort.Visibility = Visibility.Hidden
            CashOverAndShort_Label.Visibility = Visibility.Hidden
            DrawerOpenTime.Visibility = Visibility.Hidden
            DrawerOpenTime_Label.Visibility = Visibility.Hidden
            PaymentListing_Border.Visibility = Visibility.Hidden
            Expected_Payments_Grid.Visibility = Visibility.Hidden
            OpeningCash.Visibility = Visibility.Hidden
            CashSales.Visibility = Visibility.Hidden
            ExpectedCash.Visibility = Visibility.Hidden
            Line2.Visibility = Visibility.Hidden

            openLbl.Visibility = Visibility.Hidden
            CashLbl.Visibility = Visibility.Hidden
            ExpCashLbl.Visibility = Visibility.Hidden
            PlusLbl.Visibility = Visibility.Hidden

        ElseIf gResult = "Close" Then

            SQL = "SELECT * FROM OpenClose WHERE DrawerIsOpen = True"
            SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
            OpenSegment = GetNextSegmentFromSet(SegmentSet)
            Header.Content = "CLOSE DRAWER"
            CashOverAndShort.Visibility = Visibility.Visible
            CashOverAndShort_Label.Visibility = Visibility.Visible
            DrawerOpenTime.Visibility = Visibility.Visible
            DrawerOpenTime_Label.Visibility = Visibility.Visible

            buf = ExtractElementFromSegment("OpenDate", OpenSegment)
            buf = Mid(buf, 0, 9)
            DrawerOpenTime.Text = buf & " " & ExtractElementFromSegment("OpenTime", OpenSegment)

        End If
        ClerkID.Text = gCurrentUser
        SQL = "SELECT * FROM OpenClose WHERE DrawerIsOpen = True and DrawerID = '" & gDrawerID & "'"
        SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
        If SegmentSet = "" Then

            Header.Content = "OPEN DRAWER"

        Else

            OpenSegment = GetNextSegmentFromSet(SegmentSet)
            oCash = Val(ExtractElementFromSegment("OpenTotal", OpenSegment))
            OpeningCash.Text = Format(oCash, "$ 0.00")


            Get_ClosingCash()
            Get_ClosingChecks()
            Get_ClosingCreditCards()
            Get_ClosingPaidOut()
            Get_ClosingOther()

            ExpectedCash.Text = FormatCurrency(oCash + CashSales.Text)

            Header.Content = "CLOSE DRAWER"

        End If
        POSManager.FireDrawer()
        CoinCt0.Focus()
    End Sub

    Private Function CalculateOverAndShort() As Double

        Dim oCash As Double = 0
        Dim sCash As Double = 0
        Dim cCash As Double = 0
        Dim eCash As Double = 0
        Dim ShortCash As Double = 0


        oCash = Math.Round(ValFix(OpeningCash.Text), 2)
        sCash = Math.Round(ValFix(CashSales.Text), 2)
        eCash = oCash + sCash
        cCash = Math.Round(ValFix(TotalCash_TxtBx.Text), 2)
        ShortCash = Math.Round(cCash - eCash, 2)
        CashOverAndShort.Text = Format(Math.Abs(ShortCash), "$ 0.00")



        If ShortCash = 0 Then
            DrawerIsInBalance_lbl.Visibility = Visibility.Visible
            CashOverAndShort_Label.Text = "Cash Over/Short"
            CashOverAndShort.Foreground = Media.Brushes.Black
        Else
            DrawerIsInBalance_lbl.Visibility = Visibility.Hidden
            If ShortCash > 0 Then
                CashOverAndShort_Label.Text = "Cash Overage"
                CashOverAndShort.Foreground = Media.Brushes.DarkRed
            Else
                CashOverAndShort_Label.Text = "Cash Short"
                CashOverAndShort.Foreground = Media.Brushes.DarkRed
            End If
        End If

        Return ShortCash

    End Function

    Private Sub Get_ClosingPaidOut()

        Dim SegmentSet As String
        Dim Segment As String
        Dim cls_item As ClosingPayment_ListItem
        Dim SQL As String = "SELECT Payment, PaidOutReason FROM Payments WHERE [DrawerID] = '" & gDrawerID & "' AND  [Type] = 'Paid-Out' AND [Status] = 'Ok' AND [DrawerStatus] = 'Open'"

        SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
        PaidOut_List = New List(Of ClosingPayment_ListItem)

        Do Until SegmentSet = ""
            Segment = GetNextSegmentFromSet(SegmentSet)
            cls_item = New ClosingPayment_ListItem

            cls_item.Amount = ExtractElementFromSegment("Payment", Segment, "0")
            cls_item.Description = ExtractElementFromSegment("PaidOutReason", Segment, "")

            PaidOut_List.Add(cls_item)

        Loop


        PaidOut_LV.ItemsSource = PaidOut_List
        PaidOut_LV.Items.Refresh()


    End Sub

    Private Sub Get_ClosingOther()
        Dim Total As Double = 0
        Dim SegmentSet As String
        Dim Segment As String
        Dim cls_item As ClosingPayment_ListItem
        Dim SQL As String = "SELECT Payment, InvNum, OtherText FROM Payments WHERE [DrawerID] = '" & gDrawerID & "' AND  [Type] = 'Other' AND [Status] = 'Ok' AND [DrawerStatus] = 'Open'"

        SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
        OtherPayment_list = New List(Of ClosingPayment_ListItem)

        Do Until SegmentSet = ""
            Segment = GetNextSegmentFromSet(SegmentSet)
            cls_item = New ClosingPayment_ListItem

            cls_item.InvNum = ExtractElementFromSegment("InvNum", Segment, "")
            cls_item.Amount = ExtractElementFromSegment("Payment", Segment, "0")
            cls_item.Description = ExtractElementFromSegment("OtherText", Segment, "")

            OtherPayment_list.Add(cls_item)

            Total += cls_item.Amount
        Loop


        Other_LV.ItemsSource = OtherPayment_list
        Other_LV.Items.Refresh()

        TotalOther.Text = FormatCurrency(Total)
    End Sub

    Private Sub Get_ClosingCreditCards()
        Dim Total As Double = 0
        Dim SegmentSet As String
        Dim Segment As String
        Dim cls_item As ClosingPayment_ListItem
        Dim SQL As String = "SELECT [Payment], InvNum, CardName, CCnum FROM Payments WHERE [DrawerID] = '" & gDrawerID & "' AND [Type] = 'Charge' AND [Status] = 'Ok' AND [Payment] <> 0 AND [DrawerStatus] = 'Open'"

        SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
        CCPayment_List = New List(Of ClosingPayment_ListItem)

        Do Until SegmentSet = ""
            Segment = GetNextSegmentFromSet(SegmentSet)
            cls_item = New ClosingPayment_ListItem

            cls_item.InvNum = ExtractElementFromSegment("InvNum", Segment, "")
            cls_item.Amount = ExtractElementFromSegment("Payment", Segment, "0")
            cls_item.Customer_Name = ExtractElementFromSegment("CardName", Segment, "")
            cls_item.Description = Right(ExtractElementFromSegment("CCNum", Segment, ""), 4)

            CCPayment_List.Add(cls_item)

            Total += cls_item.Amount
        Loop


        CreditCards_LV.ItemsSource = CCPayment_List
        CreditCards_LV.Items.Refresh()
        TotalCharges.Text = FormatCurrency(Total)
    End Sub

    Private Sub Get_ClosingChecks()
        Dim Total As Double = 0
        Dim SegmentSet As String
        Dim Segment As String
        Dim cls_item As ClosingPayment_ListItem
        Dim SQL As String = "SELECT Payment, InvNum, CheckNum, NameOnCheck FROM Payments WHERE [DrawerID] = '" & gDrawerID & "' AND [Type] = 'Check' AND [Status] = 'Ok' AND [DrawerStatus] = 'Open' "

        SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
        CheckPayment_list = New List(Of ClosingPayment_ListItem)

        Do Until SegmentSet = ""
            Segment = GetNextSegmentFromSet(SegmentSet)
            cls_item = New ClosingPayment_ListItem

            cls_item.InvNum = ExtractElementFromSegment("InvNum", Segment, "")
            cls_item.Amount = ExtractElementFromSegment("Payment", Segment, "0")
            cls_item.Customer_Name = ExtractElementFromSegment("NameOnCheck", Segment, "")
            cls_item.Description = ExtractElementFromSegment("CheckNum", Segment, "")

            CheckPayment_list.Add(cls_item)

            Total += cls_item.Amount
        Loop


        Checks_LV.ItemsSource = CheckPayment_list
        Checks_LV.Items.Refresh()

        TotalChecks.Text = FormatCurrency(Total)
    End Sub

    Private Sub Get_ClosingCash()
        Dim Total As Double = 0
        Dim SegmentSet As String
        Dim Segment As String
        Dim cls_item As ClosingPayment_ListItem
        Dim SQL As String = "SELECT sum([Payment] - [Charge]) as CashCollected, InvNum, AcctName FROM Payments WHERE [DrawerID] = '" & gDrawerID & "' AND ([Type] = 'Cash' OR [Type] = 'Change')  AND [Status] = 'Ok' AND [DrawerStatus] = 'Open' GROUP BY InvNum, AcctName"

        SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
        CashPayment_List = New List(Of ClosingPayment_ListItem)

        Do Until SegmentSet = ""
            Segment = GetNextSegmentFromSet(SegmentSet)
            cls_item = New ClosingPayment_ListItem

            cls_item.InvNum = ExtractElementFromSegment("InvNum", Segment, "")
            cls_item.Amount = ExtractElementFromSegment("CashCollected", Segment, "0")
            cls_item.Customer_Name = ExtractElementFromSegment("AcctName", Segment, "")

            If cls_item.Customer_Name = "Cash, Check, Charge" Then cls_item.Customer_Name = ""

            CashPayment_List.Add(cls_item)

            Total += cls_item.Amount
        Loop


        Cash_LV.ItemsSource = CashPayment_List
        Cash_LV.Items.Refresh()

        CashSales.Text = FormatCurrency(Total)
    End Sub



    Private Sub CoinCoin_LostFocus(sender As Object, e As KeyEventArgs) Handles CoinCt0.KeyUp, CoinCt1.KeyUp, CoinCt2.KeyUp, CoinCt3.KeyUp, CoinCt4.KeyUp, CoinCt5.KeyUp,
            BillCt0.KeyUp, BillCt1.KeyUp, BillCt2.KeyUp, BillCt3.KeyUp, BillCt4.KeyUp, BillCt5.KeyUp, BillCt6.KeyUp

        If sender.text <> "" Then
            Caclulate_Total_Cash()
        End If

        If e.Key = Key.Return Then
            'Enter key should move focus to the next textbox
            sender.MoveFocus(New TraversalRequest(FocusNavigationDirection.Next))
        End If
    End Sub

    Private Sub CoinBill_PreviewTextInput(sender As Object, e As TextCompositionEventArgs) Handles CoinCt0.PreviewTextInput, CoinCt1.PreviewTextInput, CoinCt2.PreviewTextInput, CoinCt3.PreviewTextInput, CoinCt4.PreviewTextInput, CoinCt5.PreviewTextInput,
            BillCt0.PreviewTextInput, BillCt1.PreviewTextInput, BillCt2.PreviewTextInput, BillCt3.PreviewTextInput, BillCt4.PreviewTextInput, BillCt5.PreviewTextInput, BillCt6.PreviewTextInput
        Try
            Dim allowedchars As String = "0123456789"
            If allowedchars.IndexOf(CChar(e.Text)) = -1 Then e.Handled = True

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error.")
        End Try
    End Sub

    Private Sub CoinBill_LostKeyboardFocus(sender As Object, e As KeyboardFocusChangedEventArgs) Handles CoinCt0.LostKeyboardFocus, CoinCt1.LostKeyboardFocus, CoinCt2.LostKeyboardFocus, CoinCt3.LostKeyboardFocus, CoinCt4.LostKeyboardFocus, CoinCt5.LostKeyboardFocus,
            BillCt0.LostKeyboardFocus, BillCt1.LostKeyboardFocus, BillCt2.LostKeyboardFocus, BillCt3.LostKeyboardFocus, BillCt4.LostKeyboardFocus, BillCt5.LostKeyboardFocus, BillCt6.LostKeyboardFocus
        Try
            If sender.text = "" Then sender.text = "0"
            Caclulate_Total_Cash()

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error.")
        End Try
    End Sub

    Private Sub CoinTotal_Click(sender As Object, e As RoutedEventArgs) Handles CoinTotal0.Click, CoinTotal1.Click, CoinTotal2.Click, CoinTotal3.Click, CoinTotal4.Click, CoinTotal5.Click,
            BillTotal0.Click, BillTotal1.Click, BillTotal2.Click, BillTotal3.Click, BillTotal4.Click, BillTotal5.Click, BillTotal6.Click
        'When clicking on the denomination button, add quantity of 1
        Dim name As String = sender.name
        Dim Count_TxtBx As TextBox

        If name.First() = "C" Then
            Count_TxtBx = Me.FindName("CoinCt" & name.Last())
        Else
            Count_TxtBx = Me.FindName("BillCt" & name.Last())
        End If

        Count_TxtBx.Text = CInt(Count_TxtBx.Text) + 1

        Caclulate_Total_Cash()


    End Sub


    Private Sub CoinBtn_Click(sender As Object, e As RoutedEventArgs) Handles CoinBtn0.Click, CoinBtn1.Click, CoinBtn2.Click, CoinBtn3.Click, CoinBtn4.Click, CoinBtn5.Click,
            BillBtn0.Click, BillBtn1.Click, BillBtn2.Click, BillBtn3.Click, BillBtn4.Click, BillBtn5.Click, BillBtn6.Click
        Dim name As String = sender.name
        Dim Count_TxtBx As TextBox

        If name.First() = "C" Then
            Count_TxtBx = Me.FindName("CoinCt" & name.Last())
        Else
            Count_TxtBx = Me.FindName("BillCt" & name.Last())
        End If

        Count_TxtBx.Text = CInt(Count_TxtBx.Text) + sender.Tag

        Caclulate_Total_Cash()

    End Sub

    Private Sub Caclulate_Total_Cash()
        Dim CoinTotal As Double = 0
        Dim BillTotal As Double = 0

        '-----COINS------------------------------------------------------------------
        For i = 0 To 5
            Dim Count_TxtBx As TextBox = Me.FindName("CoinCt" & i.ToString)
            Dim Total_Btn As Button = Me.FindName("CoinTotal" & i.ToString)

            If Count_TxtBx.Text = "" Or Count_TxtBx.Text = " " Then
                Count_TxtBx.Text = "0"
            End If

            Total_Btn.Content = FormatCurrency(CInt(Count_TxtBx.Text) * CDbl(Count_TxtBx.Tag))
            CoinTotal = CoinTotal + CDbl(Total_Btn.Content)
        Next
        Coin_Total_TxtBx.Text = FormatCurrency(CoinTotal)


        '----BILLS-----------------------------------------------------------------------
        For i = 0 To 6
            Dim Count_TxtBx As TextBox = Me.FindName("BillCt" & i.ToString)
            Dim Total_Btn As Button = Me.FindName("BillTotal" & i.ToString)

            If Count_TxtBx.Text = "" Then Count_TxtBx.Text = "0"

            Total_Btn.Content = FormatCurrency(CInt(Count_TxtBx.Text) * CDbl(Count_TxtBx.Tag))
            BillTotal = BillTotal + CDbl(Total_Btn.Content)
        Next
        Bill_Total_TxtBx.Text = FormatCurrency(BillTotal)


        TotalCash_TxtBx.Text = FormatCurrency(CoinTotal + BillTotal)

        If Header.Content = "CLOSE DRAWER" Then CalculateOverAndShort()
    End Sub

    Private Sub SaveButton_Click(sender As Object, e As RoutedEventArgs) Handles SaveButton.Click
        Dim SQL As String
        Dim Segment As String
        Dim SegmentSet As String
        Dim NextID As Long
        Dim ret As Long
        Dim amt As Double
        Dim CashShort As Double
        Dim ans As Integer
        Dim buf As String
        Dim NextCloseID As Long = 0

        Select Case Header.Content

            Case "OPEN DRAWER"

                SQL = "SELECT MAX(ID) AS NextID FROM OpenClose"
                SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
                NextID = Val(ExtractElementFromSegment("NextID", SegmentSet))
                NextID = NextID + 1
                Segment = ""
                Segment = AddElementToSegment(Segment, "ID", NextID)
                Segment = AddElementToSegment(Segment, "OpenDate", Format(Today, "MM/dd/yyyy"))
                Segment = AddElementToSegment(Segment, "OpenTime", Format(Now, "HH:mm:ss"))
                Segment = AddElementToSegment(Segment, "Clerk", gCurrentUser)
                Segment = AddElementToSegment(Segment, "DrawerID", gDrawerID)
                Segment = AddElementToSegment(Segment, "DrawerIsOpen", "True")

                Segment = AddElementToSegment(Segment, "oPenny", ValFix(CoinTotal0.Content))
                Segment = AddElementToSegment(Segment, "oNickel", ValFix(CoinTotal1.Content))
                Segment = AddElementToSegment(Segment, "oDime", ValFix(CoinTotal2.Content))
                Segment = AddElementToSegment(Segment, "oQuarter", ValFix(CoinTotal3.Content))
                Segment = AddElementToSegment(Segment, "oHalf", ValFix(CoinTotal4.Content))
                Segment = AddElementToSegment(Segment, "oSilverDollar", ValFix(CoinTotal5.Content))

                Segment = AddElementToSegment(Segment, "oDollar", ValFix(BillTotal0.Content))
                Segment = AddElementToSegment(Segment, "oTwin", ValFix(BillTotal1.Content))
                Segment = AddElementToSegment(Segment, "oFive", ValFix(BillTotal2.Content))
                Segment = AddElementToSegment(Segment, "oTen", ValFix(BillTotal3.Content))
                Segment = AddElementToSegment(Segment, "oTwenty", ValFix(BillTotal4.Content))
                Segment = AddElementToSegment(Segment, "oFifty", ValFix(BillTotal5.Content))
                Segment = AddElementToSegment(Segment, "oHundred", ValFix(BillTotal6.Content))

                Segment = AddElementToSegment(Segment, "oCash", ValFix(TotalCash_TxtBx.Text))
                Segment = AddElementToSegment(Segment, "oChecks", ValFix(TotalChecks.Text))
                Segment = AddElementToSegment(Segment, "oCharges", ValFix(TotalCharges.Text))
                Segment = AddElementToSegment(Segment, "oOtherTotal", ValFix(TotalOther.Text))
                Segment = AddElementToSegment(Segment, "OpenTotal", ValFix(TotalCash_TxtBx.Text))
                Segment = AddElementToSegment(Segment, "OTotalCash", ValFix(TotalCash_TxtBx.Text))

                SQL = MakeInsertSQLFromSchema("OpenClose", Segment, gOpenCloseSchema, True)
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                If ret = 1 Then

                    ret = UpdatePolicy(gShipriteDB, "DrawerIsOpen", "True")
                    gResult = "Success"
                    gResult2 = ValFix(TotalCash_TxtBx.Text)
                    MsgBox("DRAWER IS OPEN with " & Format(ValFix(TotalCash_TxtBx.Text), "$ 0.00"))
                    Print_OpenClose_Slip(Segment)

                Else

                    gResult = "Failure"
                    gResult2 = ""

                End If

            Case "CLOSE DRAWER"

                CashShort = CalculateOverAndShort()

                If Not Round(CashShort, 2) = 0 Then

                    buf = "ATTENTION...Drawer is NOT in Balance" & vbCrLf & vbCrLf
                    If CashShort < 0 Then

                        buf = buf & "Drawer is SHORT " & Format$(CashShort * -1, "$ 0.00")

                    Else

                        buf = buf & "Drawer is OVER " & Format$(CashShort, "$ 0.00")

                    End If
                    buf = buf & vbCrLf & vbCrLf & "CONTINUE?"
                    ans = MsgBox(buf, vbQuestion + vbYesNo, gProgramName)
                    If ans = vbNo Then

                        gResult = "FAILED"

                        Exit Sub

                    End If

                End If
                'NextCloseID = Val(ExtractElementFromSegment("ID", OpenSegment))
                NextCloseID = GetNextCloseID()
                If NextCloseID = 0 Then
                    Exit Sub
                End If

                Segment = OpenSegment
                Segment = RemoveBlankElementsFromSegment(Segment)
                Segment = AddElementToSegment(Segment, "CloseID", NextCloseID)
                Segment = AddElementToSegment(Segment, "CloseDate", Format$(Today, "MM/dd/yyyy"))
                Segment = AddElementToSegment(Segment, "CloseTime", Now.ToString("HH:mm:ss"))
                Segment = AddElementToSegment(Segment, "DrawerIsOpen", "False")
                Segment = AddElementToSegment(Segment, "CloseClerk", gCurrentUser)
                Segment = AddElementToSegment(Segment, "cCash", ValFix(TotalCash_TxtBx.Text))
                Segment = AddElementToSegment(Segment, "cTotalCash", ValFix(TotalCash_TxtBx.Text))
                Segment = AddElementToSegment(Segment, "cCharge", ValFix(TotalCharges.Text))
                Segment = AddElementToSegment(Segment, "cOther", ValFix(TotalOther.Text))
                'Segment = AddElementToSegment(Segment, "cOtherCT", OtherBox.ListCount)
                Segment = AddElementToSegment(Segment, "CloseTotal", ValFix(TotalCash_TxtBx.Text) + ValFix(TotalChecks.Text) + ValFix(TotalCharges.Text) + ValFix(TotalOther.Text))
                Segment = AddElementToSegment(Segment, "CashShort", CashShort)

                Segment = AddElementToSegment(Segment, "cPenny", ValFix(CoinTotal0.Content))
                Segment = AddElementToSegment(Segment, "cNickel", ValFix(CoinTotal1.Content))
                Segment = AddElementToSegment(Segment, "cDime", ValFix(CoinTotal2.Content))
                Segment = AddElementToSegment(Segment, "cQuarter", ValFix(CoinTotal3.Content))
                Segment = AddElementToSegment(Segment, "cHalf", ValFix(CoinTotal4.Content))
                Segment = AddElementToSegment(Segment, "cSilverDollar", ValFix(CoinTotal5.Content))

                Segment = AddElementToSegment(Segment, "cDollar", ValFix(BillTotal0.Content))
                Segment = AddElementToSegment(Segment, "cTwin", ValFix(BillTotal1.Content))
                Segment = AddElementToSegment(Segment, "cFive", ValFix(BillTotal2.Content))
                Segment = AddElementToSegment(Segment, "cTen", ValFix(BillTotal3.Content))
                Segment = AddElementToSegment(Segment, "cTwenty", ValFix(BillTotal4.Content))
                Segment = AddElementToSegment(Segment, "cFifty", ValFix(BillTotal5.Content))
                Segment = AddElementToSegment(Segment, "cHundred", ValFix(BillTotal6.Content))
                Segment = AddElementToSegment(Segment, "CloseExpecting", ValFix(ExpectedCash.Text))

                SQL = MakeUpdateSQLFromSchema("OpenClose", Segment, gOpenCloseSchema)
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                If ret = 1 Then

                    'gResult = "CLOSED"
                    If InStr(1, gShipriteDB, "$") = 0 Then

                        SQL = "UPDATE Transactions SET CloseID = " & NextCloseID & " WHERE DrawerID = '" & gDrawerID & "' AND (CloseID = 0 OR ISNULL(CloseID))"

                    Else

                        SQL = "UPDATE Transactions SET CloseID = " & NextCloseID & " WHERE DrawerID = '" & gDrawerID & "' AND (CloseID = 0 OR CloseID IS NULL)"

                    End If
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                    If InStr(1, gShipriteDB, "$") = 0 Then

                        SQL = "UPDATE Payments SET CloseID = " & NextCloseID & ", DrawerStatus = 'CLOSED' WHERE DrawerID = '" & gDrawerID & "' AND (CloseID = 0 OR ISNULL(CloseID))"

                    Else

                        SQL = "UPDATE Payments SET CloseID = " & NextCloseID & ", DrawerStatus = 'CLOSED' WHERE DrawerID = '" & gDrawerID & "' AND (CloseID = 0 OR CloseID IS NULL)"

                    End If

                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                    gCurrentCloseID = NextCloseID
                    ret = UpdatePolicy(gShipriteDB, "DrawerIsOpen", "False")
                    SQL = "UPDATE OpenClose SET DrawerIsOpen = False WHERE DrawerID = '" & gDrawerID & "'"
                    ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
                    gResult = ""
                    gResult = AddElementToSegment(gResult, "CloseIDs", gCurrentCloseID)

                    Print_OpenClose_Slip(Segment)
                    General.Update_ReportWriter_Setup()
                    ReportsManager.Print_ZReport(Today, Today)

                    gResult = "CLOSED"
                    Me.Close()
                    Exit Sub

                Else

                    gResult = "Failure"
                    MsgBox("ATTENTION...Close Drawer FAILED!!!" & vbCrLf & vbCrLf & "Please Investigate Immediately.", vbCritical, gProgramName)

                End If

        End Select
        Me.Close()
    End Sub


    Private Function GetNextCloseID() As Integer
        Try
            Dim CloseID As Integer
            Dim SQL = "SELECT MAX(CloseID) AS NextID FROM OpenClose"
            Dim SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)

            CloseID = ExtractElementFromSegment("NextID", SegmentSet, "1")

            Return CloseID + 1

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error obtaining CloseID!")
            Return 0
        End Try
    End Function


    Private Sub Print_OpenClose_Slip(ByRef Segment As String)
        Dim ReceiptToPrint As String = ""
        Dim LineCT As Integer
        Dim IsOpening As Boolean

        If MsgBox("Do you want to print the " & Header.Content & " Slip?", vbQuestion + vbYesNo, "Print Receipt Slip") = vbNo Then
            Exit Sub
        End If

        If Header.Content = "OPEN DRAWER" Then
            IsOpening = True
        Else
            IsOpening = False
        End If


        LineCT = POSManager.DefaultLineCT + 1

        ReceiptToPrint &= POSManager.FillData(GetPolicyData(gShipriteDB, "Name"), LineCT, "C") & vbCrLf
        ReceiptToPrint &= POSManager.FillData(GetPolicyData(gShipriteDB, "Addr1"), LineCT, "C") & vbCrLf
        ReceiptToPrint &= POSManager.FillData(GetPolicyData(gShipriteDB, "City") & ", " & GetPolicyData(gShipriteDB, "State") & "   " & GetPolicyData(gShipriteDB, "Zip"), LineCT, "C") & vbCrLf
        ReceiptToPrint &= POSManager.FillData(GetPolicyData(gShipriteDB, "Phone1"), LineCT, "C") & vbCrLf & vbCrLf

        ReceiptToPrint &= POSManager.FillData("DRAWER #" & gDrawerID, LineCT, "C") & vbCrLf

        If gCurrentUser <> "" Then
            ReceiptToPrint &= POSManager.FillData("By " & gCurrentUser, LineCT, "C") & vbCrLf & vbCrLf
        End If

        If IsOpening Then
            ReceiptToPrint &= POSManager.FillData("*** OPENING SLIP ***", LineCT, "C") & vbCrLf & vbCrLf

        Else
            ReceiptToPrint &= POSManager.FillData("*** CLOSING SLIP ***", LineCT, "C") & vbCrLf
        End If

        ReceiptToPrint &= "======================================" & vbCrLf
        ReceiptToPrint &= POSManager.FillData(Now, LineCT, "C") & vbCrLf
        ReceiptToPrint &= "======================================" & vbCrLf & vbCrLf & vbCrLf


        If IsOpening Then
            PrepareOpeningNumbers(ReceiptToPrint, LineCT, Segment)

        Else
            'CLOSING SLIP
            PrepareClosingNumbers(ReceiptToPrint, LineCT, Segment)
        End If


        Print_Slip(ReceiptToPrint)
    End Sub

    Private Sub PrepareClosingNumbers(ByRef ReceiptToPrint As String, LineCT As Integer, CloseSegment As String)
        ReceiptToPrint &= POSManager.FillData("COIN", LineCT / 4 + 4, "L") & POSManager.FillData("OPEN", LineCT / 4, "R") & POSManager.FillData("Qty", LineCT / 4 - 4, "R") & POSManager.FillData("CLOSE", LineCT / 4, "R") & vbCrLf
        ReceiptToPrint &= "--------------------------------------" & vbCrLf

        ReceiptToPrint &= PrepareClosingLine("Pennies", ExtractElementFromSegment("oPenny", OpenSegment), ExtractElementFromSegment("cPenny", CloseSegment), 0.01, LineCT) & vbCrLf
        ReceiptToPrint &= PrepareClosingLine("Nickles", ExtractElementFromSegment("oNickel", OpenSegment), ExtractElementFromSegment("cNickel", CloseSegment), 0.05, LineCT) & vbCrLf
        ReceiptToPrint &= PrepareClosingLine("Dimes", ExtractElementFromSegment("oDime", OpenSegment), ExtractElementFromSegment("cDime", CloseSegment), 0.1, LineCT) & vbCrLf
        ReceiptToPrint &= PrepareClosingLine("Quarters", ExtractElementFromSegment("oQuarter", OpenSegment), ExtractElementFromSegment("cQuarter", CloseSegment), 0.25, LineCT) & vbCrLf
        ReceiptToPrint &= PrepareClosingLine("Half Dollars", ExtractElementFromSegment("oHalf", OpenSegment), ExtractElementFromSegment("cHalf", CloseSegment), 0.5, LineCT) & vbCrLf
        ReceiptToPrint &= PrepareClosingLine("Silver Dollars", ExtractElementFromSegment("oSilverDollar", OpenSegment), ExtractElementFromSegment("cSilverDollar", CloseSegment), 1, LineCT) & vbCrLf
        ReceiptToPrint &= "--------------------------------------" & vbCrLf & vbCrLf


        ReceiptToPrint &= POSManager.FillData("CURRENCY", LineCT / 4 + 4, "L") & POSManager.FillData("OPEN", LineCT / 4, "R") & POSManager.FillData("Qty", LineCT / 4 - 4, "R") & POSManager.FillData("CLOSE", LineCT / 4, "R") & vbCrLf
        ReceiptToPrint &= "--------------------------------------" & vbCrLf

        ReceiptToPrint &= PrepareClosingLine("Dollars", ExtractElementFromSegment("oDollar", OpenSegment), ExtractElementFromSegment("cDollar", CloseSegment), 1, LineCT) & vbCrLf
        ReceiptToPrint &= PrepareClosingLine("Twins", ExtractElementFromSegment("oTwin", OpenSegment), ExtractElementFromSegment("cTwin", CloseSegment), 2, LineCT) & vbCrLf
        ReceiptToPrint &= PrepareClosingLine("Fives", ExtractElementFromSegment("oFive", OpenSegment), ExtractElementFromSegment("cFive", CloseSegment), 5, LineCT) & vbCrLf
        ReceiptToPrint &= PrepareClosingLine("Tens", ExtractElementFromSegment("oTen", OpenSegment), ExtractElementFromSegment("cTen", CloseSegment), 10, LineCT) & vbCrLf
        ReceiptToPrint &= PrepareClosingLine("Twenties", ExtractElementFromSegment("oTwenty", OpenSegment), ExtractElementFromSegment("cTwenty", CloseSegment), 20, LineCT) & vbCrLf
        ReceiptToPrint &= PrepareClosingLine("Fifties", ExtractElementFromSegment("oFifty", OpenSegment), ExtractElementFromSegment("cFifty", CloseSegment), 50, LineCT) & vbCrLf
        ReceiptToPrint &= PrepareClosingLine("Hundreds", ExtractElementFromSegment("oHundred", OpenSegment), ExtractElementFromSegment("cHundred", CloseSegment), 100, LineCT) & vbCrLf
        ReceiptToPrint &= "--------------------------------------" & vbCrLf & vbCrLf


        ReceiptToPrint &= "CLOSE TOTAL: " & vbCrLf
        ReceiptToPrint &= FormatCurrency(ExtractElementFromSegment("CloseTotal", CloseSegment)) & vbCrLf
    End Sub

    Private Function PrepareClosingLine(Name As String, OpenDollarAmount As Double, CloseDollarAmount As Double, Value As Double, LineCT As Integer) As String
        Return POSManager.FillData(Name, LineCT / 4 + 4, "L") &
             POSManager.FillData(FormatCurrency(OpenDollarAmount), LineCT / 4, "R") &
            POSManager.FillData(CloseDollarAmount / Value, LineCT / 4 - 4, "R") &
            POSManager.FillData(FormatCurrency(CloseDollarAmount), LineCT / 4, "R")

    End Function


    Private Sub PrepareOpeningNumbers(ByRef ReceiptToPrint As String, LineCT As Integer, Segment As String)
        ReceiptToPrint &= POSManager.FillData("COIN", LineCT / 3 + 4, "L") & POSManager.FillData("Qty", LineCT / 3 - 4, "R") & POSManager.FillData("OPEN", LineCT / 3, "R") & vbCrLf
        ReceiptToPrint &= "--------------------------------------" & vbCrLf


        ReceiptToPrint &= PrepareLine("Pennies", ExtractElementFromSegment("oPenny", Segment), 0.01, LineCT) & vbCrLf
        ReceiptToPrint &= PrepareLine("Nickles", ExtractElementFromSegment("oNickel", Segment), 0.05, LineCT) & vbCrLf
        ReceiptToPrint &= PrepareLine("Dimes", ExtractElementFromSegment("oDime", Segment), 0.1, LineCT) & vbCrLf
        ReceiptToPrint &= PrepareLine("Quarters", ExtractElementFromSegment("oQuarter", Segment), 0.25, LineCT) & vbCrLf
        ReceiptToPrint &= PrepareLine("Half Dollars", ExtractElementFromSegment("oHalf", Segment), 0.5, LineCT) & vbCrLf
        ReceiptToPrint &= PrepareLine("Silver Dollars", ExtractElementFromSegment("oSilverDollar", Segment), 1, LineCT) & vbCrLf
        ReceiptToPrint &= "--------------------------------------" & vbCrLf & vbCrLf


        ReceiptToPrint &= POSManager.FillData("CURRENCY", LineCT / 3 + 4, "L") & POSManager.FillData("Qty", LineCT / 3 - 4, "R") & POSManager.FillData("OPEN", LineCT / 3, "R") & vbCrLf
        ReceiptToPrint &= "--------------------------------------" & vbCrLf

        ReceiptToPrint &= PrepareLine("Dollars", ExtractElementFromSegment("oDollar", Segment), 1, LineCT) & vbCrLf
        ReceiptToPrint &= PrepareLine("Twins", ExtractElementFromSegment("oTwin", Segment), 2, LineCT) & vbCrLf
        ReceiptToPrint &= PrepareLine("Fives", ExtractElementFromSegment("oFive", Segment), 5, LineCT) & vbCrLf
        ReceiptToPrint &= PrepareLine("Tens", ExtractElementFromSegment("oTen", Segment), 10, LineCT) & vbCrLf
        ReceiptToPrint &= PrepareLine("Twenties", ExtractElementFromSegment("oTwenty", Segment), 20, LineCT) & vbCrLf
        ReceiptToPrint &= PrepareLine("Fifties", ExtractElementFromSegment("oFifty", Segment), 50, LineCT) & vbCrLf
        ReceiptToPrint &= PrepareLine("Hundreds", ExtractElementFromSegment("oHundred", Segment), 100, LineCT) & vbCrLf
        ReceiptToPrint &= "--------------------------------------" & vbCrLf & vbCrLf


        ReceiptToPrint &= "OPEN TOTAL: " & vbCrLf
        ReceiptToPrint &= FormatCurrency(ExtractElementFromSegment("OpenTotal", Segment)) & vbCrLf
    End Sub

    Private Function PrepareLine(Name As String, DollarAmount As Double, Value As Double, LineCT As Integer) As String
        Return POSManager.FillData(Name, LineCT / 3 + 4, "L") &
            POSManager.FillData(DollarAmount / Value, LineCT / 3 - 4, "R") &
            POSManager.FillData(FormatCurrency(DollarAmount), LineCT / 3, "R")

    End Function

    Private Sub Print_Slip(ByRef receiptToPrint As String)
        If receiptToPrint.Trim.Length > 0 Then

            Dim pName As String = GetPolicyData(gReportsDB, "InvoicePrinter")
            If pName = "" Then
                pName = _Printers.Get_DefaultPrinter()
            End If
            Dim pSettings As New PrintHelper
            pSettings.PrintFontFamilyName = GetPolicyData(gReportsDB, "InvoiceFont")
            pSettings.PrintFontSize = GetPolicyData(gReportsDB, "FontSize")
            pSettings.PrintJobName = "ShipRite OpenClose Slip"

            _PrintReceipt.Print_FromText(receiptToPrint, pName, pSettings)

        End If
    End Sub


End Class