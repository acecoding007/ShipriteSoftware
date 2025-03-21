Imports System.Drawing
Imports LiveCharts
Imports LiveCharts.Wpf
Public Class Statistics
    Inherits CommonWindow

    Public Property SeriesCollection As SeriesCollection
    Public Property Labels As String()
    Public Property Formatter As Func(Of Double, String)


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

    Private Sub Statistics_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Load_Carriers()
        Load_Year_List()

        Load_Departments()

        PieChart.Visibility = Visibility.Hidden
        ColumnChart.Visibility = Visibility.Hidden
    End Sub

    Private Sub Statistcs_TC_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles Statistcs_TC.SelectionChanged
        PieChart.Visibility = Visibility.Hidden
        ColumnChart.Visibility = Visibility.Hidden
    End Sub

#Region "Shipping"
    Private Sub Load_Carriers()
        Try
            Dim SegmentSet As String = ""
            Dim current_segment As String = ""
            Dim buf As String = ""
            Dim fieldName As String = ""
            Dim fieldValue As String = ""
            Dim CarrierList As List(Of String) = New List(Of String)

            buf = IO_GetSegmentSet(gShipriteDB, "SELECT DISTINCT Carrier from Manifest")

            Do Until buf = ""
                current_segment = GetNextSegmentFromSet(buf)
                current_segment = ExtractNextElementFromSegment(fieldName, fieldValue, current_segment)

                If fieldValue Is Nothing Then fieldValue = ""

                CarrierList.Add(fieldValue)
            Loop

            Carrier_LB.ItemsSource = CarrierList

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub DisplayServiceOptions()
        Try
            Dim SegmentSet As String = ""
            Dim current_segment As String = ""
            Dim buf As String = ""
            Dim fieldName As String = ""
            Dim fieldValue As String = ""

            If Carrier_LB.SelectedIndex <> -1 Then
                'Display List of Services for selected Carrier
                Dim ServiceList As List(Of String) = New List(Of String)
                ServiceList.Add("ALL")

                buf = IO_GetSegmentSet(gShipriteDB, "SELECT DISTINCT [P1] from Manifest WHERE [Carrier]='" & Carrier_LB.SelectedItem & "'")

                Do Until buf = ""
                    current_segment = GetNextSegmentFromSet(buf)
                    current_segment = ExtractNextElementFromSegment(fieldName, fieldValue, current_segment)

                    If fieldValue Is Nothing Then fieldValue = ""

                    ServiceList.Add(fieldValue)
                Loop

                Services_LB.ItemsSource = ServiceList
                Services_LB.Items.Refresh()

            End If

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Load_Year_List()
        Dim Years As ArrayList = New ArrayList

        For y As Integer = 0 To 9
            Years.Add(Now.Year - y)
        Next

        Year_LB.ItemsSource = Years
        Year_LB.SelectedIndex = 0

        Year_Sales_LB.ItemsSource = Years
        Year_Sales_LB.SelectedIndex = 0
    End Sub

    Private Sub Carrier_LB_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles Carrier_LB.SelectionChanged
        DisplayServiceOptions()
    End Sub

    'Private Sub Carrier_PackageCount_ByYear()
    '    Dim SQL As String
    '    Dim SegmentSet As String
    '    Dim Segment As String
    '    Dim year As String


    '    SQL = "SELECT YEAR([Date]) AS ShipmentYear, Carrier, COUNT(Carrier) AS ShipmentCount FROM Manifest WHERE YEAR([Date]) > 2015 GROUP BY Carrier, YEAR([Date]) ORDER BY YEAR([Date]), Carrier"
    '    SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)

    '    SeriesCollection = New SeriesCollection

    '    Do Until SegmentSet = ""
    '        Segment = GetNextSegmentFromSet(SegmentSet)
    '        year = ExtractElementFromSegment("ShipmentYear", Segment, "0")
    '        Dim CV As ChartValues(Of Double) = New ChartValues(Of Double)

    '        Do While year = ExtractElementFromSegment("ShipmentYear", Segment, "0")
    '            CV.Add(ExtractElementFromSegment("ShipmentCount", Segment, "0"))
    '            Segment = GetNextSegmentFromSet(SegmentSet)
    '        Loop



    '        SeriesCollection.Add(New ColumnSeries With {
    '        .Title = year,
    '        .Values = CV,
    '        .MaxColumnWidth = 20,
    '        .ColumnPadding = 0})

    '    Loop


    '    'SeriesCollection.Add(New ColumnSeries With {
    '    '.Title = "2016",
    '    ' .Values = New ChartValues(Of Double) From {11, 56, 42, 48}
    '    ' })

    '    'SeriesCollection(1).Values.Add(48.0R)
    '    Labels = {"DHL", "FedEx", "UPS", "USPS"}
    '    'Formatter = Function(value) value.ToString("N")
    '    DataContext = Me
    'End Sub

    Private Sub CreateGraph_Btn_Click(sender As Object, e As RoutedEventArgs) Handles CreateGraph_Btn.Click
        Dim SQL As String
        If Services_LB.SelectedIndex = 0 Or Services_LB.SelectedIndex = -1 Then
            'All Services - By Carrier
            SQL = "SELECT Count(P1) As SCount, Month([Date]) AS SMonth FROM Manifest WHERE [Carrier]='" & Carrier_LB.SelectedItem & "' AND Year([Date])=" & Year_LB.SelectedItem & " AND [Exported]<>'Deleted' GROUP BY Month([Date])"
            Monthly(SQL, Carrier_LB.SelectedItem)
        Else
            SQL = "SELECT Count(P1) As SCount, Month([Date]) AS SMonth FROM Manifest WHERE [P1]='" & Services_LB.SelectedItem & "' AND Year([Date])=" & Year_LB.SelectedItem & " AND [Exported]<>'Deleted' GROUP BY Month([Date])"
            Monthly(SQL, Services_LB.SelectedItem)

        End If

        PieChart.Visibility = Visibility.Hidden
        ColumnChart.Visibility = Visibility.Visible

    End Sub


    Private Sub Monthly(SQL As String, Label As String)

        Dim SegmentSet As String
        Dim Segment As String
        Dim month As String
        Dim Months As ArrayList = New ArrayList

        For m As Integer = 1 To 12
            Months.Add(MonthName(New DateTime(1, m, 1).Month.ToString))
        Next

        DataContext = Nothing

        SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)

        If SegmentSet <> "" Then

            SeriesCollection = New SeriesCollection

            Do Until SegmentSet = ""
                Segment = GetNextSegmentFromSet(SegmentSet)
                month = ExtractElementFromSegment("SMonth", Segment, "0")
                Dim CV As ChartValues(Of Double) = New ChartValues(Of Double)
                CV.Add(ExtractElementFromSegment("SCount", Segment, "0"))


                SeriesCollection.Add(New ColumnSeries With {
                .Title = Months(month - 1),
                .Values = CV,
                .DataLabels = True,
                .MaxColumnWidth = 25,
                .ColumnPadding = 5})
            Loop

            Labels = {Label}
            DataContext = Me


        Else
            MsgBox("No Data to Display!", vbInformation)
        End If
    End Sub

    Private Sub Create_CarrierPieChart_Click(sender As Object, e As RoutedEventArgs) Handles Create_CarrierPieChart.Click
        Dim SQL As String
        Dim SegmentSet As String
        Dim Segment As String
        Dim PS As PieSeries
        Dim CV As ChartValues(Of Integer)

        DataContext = Nothing
        PieChart.Series.Clear()

        SQL = "SELECT Carrier, COUNT(Carrier) AS ShipmentCount FROM Manifest WHERE YEAR([Date]) = " & Year_LB.SelectedItem & " GROUP BY Carrier "
        SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)

        If SegmentSet <> "" Then

            Do Until SegmentSet = ""

                Segment = GetNextSegmentFromSet(SegmentSet)
                CV = New ChartValues(Of Integer)
                CV.Add(ExtractElementFromSegment("ShipmentCount", Segment, "0"))

                PS = New PieSeries
                PS.Title = ExtractElementFromSegment("Carrier", Segment, "")
                PS.Values = CV
                PS.DataLabels = True
                PieChart.Series.Add(PS)
            Loop


        End If


        DataContext = Me

        PieChart.Visibility = Visibility.Visible
        ColumnChart.Visibility = Visibility.Hidden

    End Sub

#End Region

#Region "POS"
    Private Sub Load_Departments()
        Try
            Dim buf As String
            Dim current_segment As String
            Dim Department_List = New List(Of String)


            buf = IO_GetSegmentSet(gShipriteDB, "SELECT Department From Departments")

            Do Until buf = ""
                current_segment = GetNextSegmentFromSet(buf)
                Department_List.Add(ExtractElementFromSegment("Department", current_segment, ""))
            Loop

            Department_LB.ItemsSource = Department_List

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Load_SKUs()
        Try
            Dim buf As String
            Dim current_segment As String
            Dim SKU_List = New List(Of String)


            buf = IO_GetSegmentSet(gShipriteDB, "SELECT distinct SKU From Transactions where Dept='" & Department_LB.SelectedItem & "'")

            Do Until buf = ""
                current_segment = GetNextSegmentFromSet(buf)
                SKU_List.Add(ExtractElementFromSegment("SKU", current_segment, ""))
            Loop

            SKU_LB.ItemsSource = SKU_List

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Department_LB_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles Department_LB.SelectionChanged
        If Department_LB.SelectedIndex <> -1 Then
            Load_SKUs()
        End If
    End Sub

    Private Sub CreateGraphSales_Btn_Click(sender As Object, e As RoutedEventArgs) Handles CreateGraphSales_Btn.Click
        Dim SQL As String

        If SKU_LB.SelectedIndex = 0 Or SKU_LB.SelectedIndex = -1 Then
            'run graph by department
            SQL = "SELECT Sum(ExtPrice) AS SCount, Month([Date]) As SMonth FROM transactions WHERE Dept='" & Department_LB.SelectedItem & "' and Year([Date])=" & Year_Sales_LB.SelectedItem & " and Status<>'Deleted' GROUP BY Month([Date])"
        Else
            'run graph by SKU
            SQL = "SELECT Sum(ExtPrice) AS SCount, Month([Date]) As SMonth FROM transactions WHERE SKU='" & SKU_LB.SelectedItem & "' and Year([Date])=" & Year_Sales_LB.SelectedItem & " and Status<>'Deleted' GROUP BY Month([Date])"
        End If

        POSMonthly(SQL, Department_LB.SelectedItem)


        PieChart.Visibility = Visibility.Hidden
        ColumnChart.Visibility = Visibility.Visible

    End Sub

    Private Sub POSMonthly(SQL As String, Label As String)

        Dim SegmentSet As String
        Dim Segment As String
        Dim month As String
        Dim Months As ArrayList = New ArrayList

        For m As Integer = 1 To 12
            Months.Add(MonthName(New DateTime(1, m, 1).Month.ToString))
        Next

        DataContext = Nothing

        SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)

        If SegmentSet <> "" Then

            SeriesCollection = New SeriesCollection

            Do Until SegmentSet = ""
                Segment = GetNextSegmentFromSet(SegmentSet)
                month = ExtractElementFromSegment("SMonth", Segment, "0")
                Dim CV As ChartValues(Of Double) = New ChartValues(Of Double)
                CV.Add(FormatCurrency(ExtractElementFromSegment("SCount", Segment, "0")))


                SeriesCollection.Add(New ColumnSeries With {
                .Title = Months(month - 1),
                .Values = CV,
                .DataLabels = True,
                .LabelPoint = Function(point) point.Y.ToString("c"),
                .MaxColumnWidth = 25,
                .ColumnPadding = 5})
            Loop

            Labels = {Label}
            DataContext = Me

        Else
            MsgBox("No Data to Display!", vbInformation)
        End If
    End Sub

    Private Sub Create_DepartmentPieChart_Click(sender As Object, e As RoutedEventArgs) Handles Create_DepartmentPieChart.Click
        Dim SQL As String
        Dim SegmentSet As String
        Dim Segment As String
        Dim PS As PieSeries
        Dim CV As ChartValues(Of Integer)

        DataContext = Nothing
        PieChart.Series.Clear()

        SQL = "SELECT Sum(ExtPrice) AS Total, Dept FROM transactions WHERE  Year([Date])=" & Year_LB.SelectedItem & " and Status<>'Deleted' GROUP BY Dept"

        SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)

        If SegmentSet <> "" Then

            Do Until SegmentSet = ""

                Segment = GetNextSegmentFromSet(SegmentSet)
                If (ExtractElementFromSegment("Total", Segment, "0")) >= 0 Then
                    CV = New ChartValues(Of Integer)
                    CV.Add(ExtractElementFromSegment("Total", Segment, "0"))

                    PS = New PieSeries
                    PS.Title = ExtractElementFromSegment("Dept", Segment, "")
                    PS.Values = CV
                    PS.DataLabels = True
                    PS.LabelPoint = Function(point) point.Y.ToString("c")
                    PieChart.Series.Add(PS)
                End If
            Loop


        End If


        DataContext = Me

        PieChart.Visibility = Visibility.Visible
        ColumnChart.Visibility = Visibility.Hidden
    End Sub






#End Region
End Class
