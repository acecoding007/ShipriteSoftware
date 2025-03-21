Public Class RateCharts
    Inherits CommonWindow

    Dim ServiceList As List(Of String)
    Public Sub New()

        MyBase.New()

        ' This call is required by the designer.
        InitializeComponent()

    End Sub

    Public Sub New(ByVal callingWindow As Window)

        MyBase.New(callingWindow)

        ' This call is required by the designer.
        InitializeComponent()
        Load_Carriers()

    End Sub

    Private Sub Load_Carriers()
        Try

            Dim SegmentSet As String = ""
            Dim current_segment As String = ""
            Dim buf As String = ""
            Dim fieldName As String = ""
            Dim fieldValue As String = ""
            Dim CarrierList As List(Of Carrier) = New List(Of Carrier)
            Dim current_Carrier As Carrier


            buf = IO_GetSegmentSet(gShipriteDB, "SELECT DISTINCT Carrier from Master")

            Do Until buf = ""
                current_segment = GetNextSegmentFromSet(buf)
                current_segment = ExtractNextElementFromSegment(fieldName, fieldValue, current_segment)

                If fieldValue Is Nothing Then fieldValue = ""
                current_Carrier = New Carrier
                current_Carrier.CarrierName = fieldValue
                current_Carrier.CarrierImage = "Resources/" & fieldValue & "_Logo.png"

                CarrierList.Add(current_Carrier)
            Loop

            Carrier_ListBox.ItemsSource = CarrierList

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub


    Private Sub DisplayServiceOptions() Handles Carrier_ListBox.SelectionChanged
        'Display List of Services for selected Carrier
        Try
            Dim SegmentSet As String = ""
            Dim current_segment As String = ""
            Dim buf As String = ""
            Dim fieldName As String = ""
            Dim fieldValue As String = ""

            ServiceList = New List(Of String)
            If Carrier_ListBox.SelectedIndex <> -1 Then

                If Carrier_ListBox.SelectedItem.CarrierName = "FedEx" Or Carrier_ListBox.SelectedItem.CarrierName = "UPS" Then
                    RetailRate_ChkBx.Visibility = Visibility.Visible
                Else
                    RetailRate_ChkBx.Visibility = Visibility.Hidden
                End If

                buf = IO_GetSegmentSet(gShipriteDB, "SELECT DISTINCT [Service] from Master WHERE [Carrier]='" & Carrier_ListBox.SelectedItem.CarrierName & "'")

                Do Until buf = ""
                    current_segment = GetNextSegmentFromSet(buf)
                    current_segment = ExtractNextElementFromSegment(fieldName, fieldValue, current_segment)

                    If fieldValue Is Nothing Then fieldValue = ""

                    ServiceList.Add(fieldValue)
                Loop

                Service_ListBox.ItemsSource = ServiceList
                Service_ListBox.Items.Refresh()


            End If

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Load_Chart_LV(TableName As String)
        Dim DB As String
        Dim Fields As String
        Dim FieldsList As List(Of String)
        Dim SQL As String
        Dim Count As Integer = 0
        Dim searchCol As GridViewColumn
        Dim Bind As Binding

        Try
            BindingOperations.ClearAllBindings(Chart_LV)
            Chart_LV.DataContext = Nothing
            Chart_LV.View = New GridView

            Chart_LV.Items.Clear()

            Dim DT As System.Data.DataTable = New System.Data.DataTable
            Dim searchGrid As GridView = Chart_LV.View
            searchGrid.AllowsColumnReorder = False

            DB = GetDBName()

            If DB <> "" Then
                Fields = IO_GetFieldsCollection(DB, TableName, "", False, False, True)

                If Fields = "" Then
                    Exit Sub
                End If
            Else
                Exit Sub
            End If

            Fields = Fields.Remove(0, 1)
            Fields = Fields.Replace("  »", "")
            FieldsList = Fields.Split("«").ToList

            For Each Field As String In FieldsList
                searchCol = New GridViewColumn
                Bind = New Binding(Field)
                Bind.StringFormat = "N2"
                searchCol.DisplayMemberBinding = Bind
                searchCol.Header = Field
                searchCol.Width = 60
                searchGrid.Columns.Add(searchCol)

                DT.Columns.Add(Field)
                Count = Count + 1
            Next

            Chart_LV.View = searchGrid

            SQL = "Select * FROM [" & TableName & "]"

            IO_LoadListView(Chart_LV, DT, DB, SQL, Count)

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try

    End Sub

    Private Function GetDBName() As String
        Try
            Select Case Carrier_ListBox.SelectedItem.CarrierName
                Case "FedEx"
                    If RetailRate_ChkBx.IsChecked Then
                        Return gFedExRetailServicesDB
                    Else
                        Return gFedExServicesDB
                    End If

                Case "UPS"
                    If RetailRate_ChkBx.IsChecked Then
                        Return gUPSRetailServicesDB
                    Else
                        Return gUPSServicesDB
                    End If

                Case "USPS"
                    Return gUSMailDB_Services

                Case "DHL"
                    Return gDHLServicesDB

                Case Else
                    Return ""

            End Select


        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try

    End Function

    Private Sub Service_ListBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles Service_ListBox.SelectionChanged
        If Service_ListBox.SelectedIndex <> -1 Then
            Load_Chart_LV(Service_ListBox.SelectedItem)
        End If
    End Sub

    Private Sub RetailRate_ChkBx_Checked(sender As Object, e As RoutedEventArgs) Handles RetailRate_ChkBx.Checked, RetailRate_ChkBx.Unchecked
        If Service_ListBox.SelectedIndex <> -1 Then
            Load_Chart_LV(Service_ListBox.SelectedItem)
        End If

    End Sub
End Class
