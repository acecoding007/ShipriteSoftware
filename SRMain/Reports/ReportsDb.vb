Imports System.Data
Imports System.Data.OleDb
Imports CommonCode


Public Module ReportsDb

    '' Reports.mdb
    ''
    '' Setup table:
    Public Const fldReportPrinter As String = "ReportPrinter"
    Public Const fldReceiptSlipPrinter As String = "LabelPrinter2"
    Public Const fldInvoicePrinter As String = "InvoicePrinter"
    Public Const fldFedExPrinter As String = "FedExPrinter"
    Public Const fldFedExTPrinter As String = "FedExTPrinter"
    Public Const fldDHLLabelPrinter As String = "ABLabelPrinter"
    Public Const fldGenericLabelPrinter As String = "LabelPrinter"

    Private m_path2db As String
    Public Property path2db() As String
        Get
            Return m_path2db
        End Get
        Set(ByVal value As String)
            m_path2db = value
        End Set
    End Property

#Region "Private/Common"

    Private Function get_dreader(ByVal sql2exe As String, ByRef dreader As OleDb.OleDbDataReader, Optional ByVal is2read As Boolean = False) As Boolean
        Dim errorDesc As String = String.Empty
        If _Connection.GetDataReader(_Connection.Jet_OLEDB, path2db, sql2exe, dreader, errorDesc, String.Empty) Then
            get_dreader = dreader.HasRows
            If is2read Then
                get_dreader = dreader.Read
            End If
        Else
            Throw New ArgumentException(errorDesc)
        End If
    End Function
    Private Function get_dreader_onevalue(ByVal sql2exe As String, ByRef onevalue As String) As Boolean
        Dim dreader As OleDb.OleDbDataReader = Nothing
        onevalue = String.Empty ' assume.
        If get_dreader(sql2exe, dreader, True) Then
            onevalue = _Convert.Null2DefaultValue(dreader(0), "")
        End If
        _Connection.CloseDataReader(dreader)
    End Function
    Private Function get_dreader_onevalue(ByVal sql2exe As String, ByRef onevalue As Single) As Boolean
        Dim dreader As OleDb.OleDbDataReader = Nothing
        onevalue = String.Empty ' assume.
        If get_dreader(sql2exe, dreader, True) Then
            onevalue = _Convert.Null2DefaultValue(dreader(0), "")
        End If
        _Connection.CloseDataReader(dreader)
    End Function
    Private Function get_dreader_onevalue(ByVal sql2exe As String, ByRef onevalue As Long) As Boolean
        Dim dreader As OleDb.OleDbDataReader = Nothing
        If get_dreader(sql2exe, dreader, True) Then
            onevalue = _Convert.Null2DefaultValue(dreader(0), 0)
        End If
        _Connection.CloseDataReader(dreader)
    End Function

    Public Function execute_cmd(ByVal sql2exe As String) As Boolean
        Dim errorDesc As String = String.Empty
        Dim rowsAffected As Integer = 0
        If _Connection.ExecuteCommand(_Connection.Jet_OLEDB, path2db, sql2exe, rowsAffected, errorDesc, String.Empty) Then
            execute_cmd = (rowsAffected > 0)
        Else
            Throw New ArgumentException(errorDesc)
        End If
    End Function

#End Region

    Public Function Get_ReportPrinterName(ByVal fldPrinterFieldName As String, ByRef printername As String) As Boolean
        Dim sql2exe As String = "Select [" & fldPrinterFieldName & "] From [Setup]"
        Get_ReportPrinterName = get_dreader_onevalue(sql2exe, printername)
    End Function
    Public Function Get_ReceiptPrinterFontName(ByRef fontname As String) As Boolean
        Dim sql2exe As String = "Select [InvoiceFont] From [Setup]"
        Get_ReceiptPrinterFontName = get_dreader_onevalue(sql2exe, fontname)
    End Function
    Public Function Get_ReceiptPrinterFontSize(ByRef fontsize As Single) As Boolean
        Dim strsize As String = String.Empty
        Dim sql2exe As String = "Select [FontSize] From [Setup]"
        Get_ReceiptPrinterFontSize = get_dreader_onevalue(sql2exe, strsize)
        fontsize = Val(strsize)
    End Function
    Public Function Get_FedExLabelType() As String
        Get_FedExLabelType = GetPolicyData(gReportsDB, "FedExLabelType")
    End Function

End Module
