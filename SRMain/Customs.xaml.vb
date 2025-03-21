Public Class Customs

    Public Class CustomsItem
        Public Property Description As String
        Public Property Weight As Double
        Public Property Value As Double
        Public Property OriginCountry As String
        Public Property Qty As Double
        Public Property HarmonizedCode As String
    End Class

    Public Shared CustomsList As List(Of CustomsItem)
    Public Shared Customs_Contents_Type As String

    Private Carrier As String

    Public Sub New(ByRef callingWindow As Window, ByVal Ship_carrier As String)

        Me.Width = callingWindow.Width * 0.7
        Me.Height = callingWindow.Height * 0.7
        Me.WindowStartupLocation = WindowStartupLocation.CenterScreen

        ' This call is required by the designer.
        InitializeComponent()
        Carrier = Ship_carrier

    End Sub

    Private Sub Close_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Close_Btn.Click
        CustomsList = Nothing
        Customs_Contents_Type = Nothing
        Me.Close()
    End Sub

    Private Sub Customs_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Try
            If IsNothing(CustomsList) Then
                CustomsList = New List(Of CustomsItem)
                AddNewCustomsItemToList()
            End If

            Customs_LV.ItemsSource = CustomsList
            If gShip.PackagingType = "Letter" Then
                ContentsType_CmbBox.SelectedIndex = 1
            End If

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub AddNewCustomsItemToList()
        Try
            Dim item As CustomsItem = New CustomsItem

            item.Description = ""
            item.OriginCountry = "United States"
            item.Qty = 1

            CustomsList.Add(item)

            Customs_LV.Items.Refresh()
            Customs_LV.SelectedItem = item

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub ListViewItem_LostFocus(sender As Object, e As RoutedEventArgs)
        Try
            'creates a new empty row in listview
            If Customs_LV.SelectedIndex <> Customs_LV.Items.Count - 1 Then Exit Sub

            Dim item As CustomsItem = Customs_LV.SelectedItem

            If item.Description <> "" And item.Weight <> 0 Then
                AddNewCustomsItemToList()
            End If
            Calculate_Totals()

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub SelectCurrentItem(ByVal sender As Object, ByVal e As KeyboardFocusChangedEventArgs)
        Try

            'selects current row in listview when a textbox is focused on
            Dim item As ListViewItem = CType(sender, ListViewItem)
            item.IsSelected = True

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub TxtBox_GotKeyboardFocus(sender As Object, e As KeyboardFocusChangedEventArgs)
        Try

            'when focusing on a textbox, select all existing text in it.
            Dim currentTextBox As TextBox = TryCast(sender, TextBox)
            If currentTextBox.Name = "Weight_TxtBox" And DistributeWeight_CheckBox.IsChecked Then
                distributeWeight()
            End If
            currentTextBox.SelectAll()

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub FocusTextBoxOnLoad(sender As Object, e As RoutedEventArgs)
        Try
            'sets focus to the Description TextBox every time a new line item is created.
            sender.focus()

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Delete_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Delete_Btn.Click
        Try

            If CustomsList.Count = 1 Then Exit Sub

            CustomsList.Remove(Customs_LV.SelectedItem)
            Customs_LV.Items.Refresh()

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Clear_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Clear_Btn.Click
        Try
            CustomsList.Clear()
            AddNewCustomsItemToList()
            Customs_LV.Items.Refresh()
            Calculate_Totals()

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Calculate_Totals()
        Try
            Dim sumValue As Double = 0
            Dim sumWeight As Double = 0

            For Each item As CustomsItem In CustomsList
                sumValue += item.Value
                sumWeight += item.Weight
            Next

            ValueTotal_TxtBox.Text = FormatCurrency(sumValue)
            WeightTotal_TxtBox.Text = sumWeight & " lb"

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub DistributeWeight_CheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles DistributeWeight_CheckBox.Checked
        distributeWeight()

    End Sub

    Private Sub distributeWeight()
        Try
            If IsNothing(CustomsList) OrElse CustomsList.Count = 0 Then Exit Sub

            Dim dist_weight As Double

            If CustomsList.Last.Description = "" Then
                dist_weight = gShip.actualWeight / (CustomsList.Count - 1)
            Else
                dist_weight = gShip.actualWeight / (CustomsList.Count)
            End If


            For Each item As CustomsItem In CustomsList
                If item.Description <> "" Then
                    item.Weight = Math.Floor(dist_weight * 100) / 100
                End If
            Next

            Customs_LV.Items.Refresh()

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Function Check_Input() As Boolean

        If CustomsList.Count = 0 Then
            MsgBox("No customs items entered!", vbOKOnly + vbExclamation)
            Return False
        End If

        If CustomsList.FindIndex(Function(x As CustomsItem) x.Description = "") <> -1 Then
            MsgBox("Description field cannot be blank!", vbOKOnly + vbExclamation)
            Return False
        End If

        If CustomsList.FindIndex(Function(x As CustomsItem) x.Weight = 0) <> -1 Then
            MsgBox("Each item needs to have a weight!", vbOKOnly + vbExclamation)
            Return False
        End If

        If CustomsList.FindIndex(Function(x As CustomsItem) x.OriginCountry = "") <> -1 Then
            MsgBox("Each item needs to have country of origin entered!", vbOKOnly + vbExclamation)
            Return False
        End If

        If Carrier = "USPS" And CustomsList.FindIndex(Function(x As CustomsItem) x.Value = 0) <> -1 Then
            'USPS/Endicia does not allow the customs value of any one item to be 0
            MsgBox("One or more customs line items have a value of $0.00." & vbCrLf & "For USPS shipments each line item has to have a value!", vbOKOnly + vbExclamation, "Customs Value Cannot Be $0.00")
            Return False
        End If

        If Carrier = "FedEx" And ContentsType_CmbBox.SelectedIndex <> 1 And CDbl(ValueTotal_TxtBox.Text) < 1 Then
            'If not shipping documents, then Fedex does not allow a value of less then $1.00
            MsgBox("Total customs value has to be at least $1.00.", vbOKOnly + vbExclamation)
            Return False
        End If

        Return True

    End Function

    Private Sub Save_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Save_Btn.Click
        Try

            If CustomsList.Count > 1 And CustomsList.Last.Description = "" And CustomsList.Last.Weight = 0 Then
                CustomsList.Remove(CustomsList.Last)
            End If
            If Check_Input() Then


                Customs_Contents_Type = ContentsType_CmbBox.Text
                gPackageShipped = True
                Me.Close()

            End If

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub HarmonizedCodeLookup_Btn_Click(sender As Object, e As RoutedEventArgs) Handles HarmonizedCodeLookup_Btn.Click
        Dim webAddress As String = "https://www.goglobalpost.com/hs-code-lookup-tool/"
        Process.Start(webAddress)
    End Sub

    Private Sub HarmonizedCodeLookup_Btn2_Click(sender As Object, e As RoutedEventArgs) Handles HarmonizedCodeLookup_Btn2.Click
        Dim webAddress As String = "https://uscensus.prod.3ceonline.com/"
        Process.Start(webAddress)
    End Sub
End Class
