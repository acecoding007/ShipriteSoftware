Imports System.Drawing.Printing
Imports SHIPRITE.ShipRiteReports

Public Class ReportPreview
    Inherits CommonWindow
    ReadOnly report As _ReportObject

    Private _status As State = State.Waiting
    Public ReadOnly Property Status As State
        Get
            Return _status
        End Get
    End Property


    Public Enum State
        Waiting
        Preview
        Print
        Cancel
    End Enum

    ''' <summary>
    ''' Constructor for report previewer. The Report Viewer handles printing of the reports, 
    ''' you still have to prepare the report. The Previewer sets the isPreview field properly.
    ''' The printer is also set.
    ''' </summary>
    ''' <param name="report"></param>
    Public Sub New(ByRef report As _ReportObject)
        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Me.report = report
    End Sub

    Private Overloads Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        ' Populate the combobox
        cmbPrinterName.ItemsSource = PrinterSettings.InstalledPrinters

        If IsValid_PrinterName(GetPolicyData(gReportsDB, "ReportPrinter", "")) Then
            cmbPrinterName.SelectedItem = GetPolicyData(gReportsDB, "ReportPrinter", "")
        Else
            cmbPrinterName.SelectedItem = _Printers.Get_DefaultPrinter
        End If


        report.PrinterName = cmbPrinterName.Text
    End Sub

    Private Sub btnPreview_Click(sender As Object, e As RoutedEventArgs) Handles btnPreview.Click
        _status = State.Preview
        report.IsPreviewReport = True
        Cursor = Cursors.Wait
        If report.ReportName <> "" Then
            If report.DatabaseTables.Count > 0 Then
                ShipRiteReports.Execute_DataTable(report)
            Else
                ShipRiteReports.Execute_ODBC(report)

            End If
        End If

        Cursor = Cursors.Arrow
        Me.Close()
    End Sub
    Private Sub btnPrint_Click(sender As Object, e As RoutedEventArgs) Handles btnPrint.Click
        _status = State.Print
        report.IsPreviewReport = False
        Cursor = Cursors.Wait
        If report.ReportName <> "" Then
            If report.DatabaseTables.Count > 0 Then
                ShipRiteReports.Execute_DataTable(report)
            Else
                ShipRiteReports.Execute_ODBC(report)
            End If
        End If
        Cursor = Cursors.Arrow
        Me.Close()
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As RoutedEventArgs) Handles btnCancel.Click
        _status = State.Cancel
        Me.Close()
    End Sub

    Private Sub cmbPrinterName_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cmbPrinterName.SelectionChanged
        report.PrinterName = cmbPrinterName.Text
    End Sub
End Class
