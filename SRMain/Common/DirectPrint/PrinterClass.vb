Imports System.Drawing

Public Class PrinterClass
    Private doc As Printing.PrintDocument
    Private currentPosition As Point
    Private font As New Font("15 cpi", 9.5F)
    Private fontStyle As FontStyle
    Private settings As Printing.PrinterSettings
    Private toPrint As New Queue(Of Func(Of Object, Printing.PrintPageEventArgs, PrintFlowControl))

    Private _Path As String
    Public _Align As TextAlignment = TextAlignment.Default
    Private bIsDebug As Boolean = True
    Public Enum TextAlignment As Byte
        [Default] = 0
        Left
        Center
        Right
    End Enum

    Private Enum PrintFlowControl As Byte
        [Default] = 0
        EndPage
        EndDoc
        NextCommand
    End Enum
#Region "Constructors"
    'Public Sub New(ByVal AppPath As String)
    '    SetPrinterName("CITIZEN CT-S2000", AppPath)
    'End Sub
    Public Sub New(ByVal strPrinterName As String, ByVal AppPath As String)
        Try
            doc = New Printing.PrintDocument()
            currentPosition = New Point(0, 0)
            settings = New Printing.PrinterSettings()

            SetPrinterName(strPrinterName, AppPath)
            doc.PrinterSettings = settings
        Catch ex As Exception
            Throw New System.Exception(strPrinterName & " printer was not found.")
        End Try
    End Sub
    Private Sub SetPrinterName(ByVal PrinterName As String, ByVal AppPath As String)
        Dim printerNameFound = False

        For Each printer In Printing.PrinterSettings.InstalledPrinters
            If printer = PrinterName Then
                printerNameFound = True
                settings.PrinterName = printer
                Exit For
            End If
        Next
        If Not printerNameFound Then
            Throw New System.Exception(PrinterName & " was not found in \'Printers\'")
            settings.PrinterName = Nothing
        Else
            Me.Path = AppPath
            'If bIsDebug Then
            '    p.PrintAction = Printing.PrintAction.PrintToPreview
            'End If
        End If
    End Sub
#End Region
#Region "Images"
    Public Property Path() As String
        Get
            Return _Path
        End Get
        Set(ByVal value As String)
            _Path = value
        End Set
    End Property
    Public Sub PrintLogo()
        toPrint.Enqueue(
            Function(sender As Object, ev As Printing.PrintPageEventArgs) As PrintFlowControl
                Me.PrintImage(_Path & "\Logo.bmp")
                Return PrintFlowControl.NextCommand
            End Function
        )
    End Sub
    Private Sub PrintImage(ByVal FileName As String)
        toPrint.Enqueue(
            Function(sender As Object, ev As Printing.PrintPageEventArgs) As PrintFlowControl
                Dim pic As Image
                pic = Image.FromFile(FileName)
                ev.Graphics.DrawImage(pic, currentPosition)
                currentPosition.Y += pic.Height
                Return PrintFlowControl.NextCommand
            End Function
        )
    End Sub
#End Region
#Region "Font"
    Public Property FontName() As String
        Get
            Return font.Name
        End Get
        Set(ByVal value As String)
            toPrint.Enqueue(
                Function(sender As Object, ev As Printing.PrintPageEventArgs) As PrintFlowControl
                    font = New Font(value, font.SizeInPoints, fontStyle)
                    Return PrintFlowControl.NextCommand
                End Function
            )
        End Set
    End Property

    Public Property FontSize() As Single
        Get
            Return font.SizeInPoints
        End Get
        Set(ByVal value As Single)
            toPrint.Enqueue(
                Function(sender As Object, ev As Printing.PrintPageEventArgs) As PrintFlowControl
                    font = New Font(font.FontFamily, value, fontStyle)
                    Return PrintFlowControl.NextCommand
                End Function
            )
        End Set
    End Property
    Public Property Bold() As Boolean
        Get
            Return font.Bold
        End Get
        Set(ByVal value As Boolean)
            toPrint.Enqueue(
                Function(sender As Object, ev As Printing.PrintPageEventArgs) As PrintFlowControl
                    If value Then
                        fontStyle = (fontStyle Or FontStyle.Bold)
                    Else
                        fontStyle = (fontStyle And Not FontStyle.Bold)
                    End If
                    font = New Font(font.FontFamily, font.SizeInPoints, fontStyle)
                    'font.Bold = value
                    Return PrintFlowControl.NextCommand
                End Function
            )
        End Set
    End Property
    Public Function DrawLine(col As Color)
        toPrint.Enqueue(
            Function(sender As Object, ev As Printing.PrintPageEventArgs) As PrintFlowControl
                Dim p As Pen = New Pen(col, 2)
                ev.Graphics.DrawLine(p, currentPosition, New Point(ev.PageBounds.Width, currentPosition.Y))
                currentPosition.Y += 20
                Return False
            End Function
        )
    End Function
    Public Sub NormalFont()
        Me.FontSize = 9.5F
    End Sub
    Public Sub BigFont()
        Me.FontSize = 15.0F
    End Sub
    Public Sub SmallFont()
        Me.FontSize = 7.0F
    End Sub

    Public Sub SetFont(Optional ByVal FontSize As Single = 9.5F, Optional ByVal FontName As String = "15 cpi", Optional ByVal BoldType As Boolean = False)
        Me.FontSize = FontSize
        Me.FontName = FontName ' FontA1x1
        Me.Bold = BoldType
    End Sub
#End Region
#Region "Control"
    Public Sub FeedPaper(Optional ByVal nlines As Integer = 3)
        toPrint.Enqueue(
            Function(sender As Object, ev As Printing.PrintPageEventArgs) As PrintFlowControl
                currentPosition.X = 0
                currentPosition.Y += nlines * ev.Graphics.MeasureString(" ", font).Height
                Return PrintFlowControl.NextCommand
            End Function
        )
    End Sub

    Public Sub GotoCol(Optional ByVal ColNumber As Integer = 0)
        toPrint.Enqueue(
            Function(sender As Object, ev As Printing.PrintPageEventArgs) As PrintFlowControl
                Dim ColWidth As Single = ev.PageSettings.PaperSize.Width / 44
                currentPosition.X = ColWidth * ColNumber
                Return PrintFlowControl.NextCommand
            End Function
        )
    End Sub
    Public Sub GotoSixth(Optional ByVal nSixth As Integer = 1)
        toPrint.Enqueue(
            Function(sender As Object, ev As Printing.PrintPageEventArgs) As PrintFlowControl
                Dim OneSixth As Single = ev.PageSettings.PaperSize.Width / 6
                currentPosition.X = OneSixth * (nSixth - 1)
                Return PrintFlowControl.NextCommand
            End Function
        )
    End Sub


    Public Sub UnderlineOn()
        toPrint.Enqueue(
            Function(sender As Object, ev As Printing.PrintPageEventArgs) As PrintFlowControl
                fontStyle = (fontStyle Or FontStyle.Underline)
                font = New Font(font, fontStyle)
                Return PrintFlowControl.NextCommand
            End Function
        )
    End Sub
    Public Sub UnderlineOff()
        toPrint.Enqueue(
            Function(sender As Object, ev As Printing.PrintPageEventArgs) As PrintFlowControl
                fontStyle = (fontStyle And Not FontStyle.Underline)
                font = New Font(font, fontStyle)
                Return PrintFlowControl.NextCommand
            End Function
        )
    End Sub
    Public Sub WriteLine(ByVal Text As String, Optional ByVal xOffset As Integer = 0)
        toPrint.Enqueue(
            Function(subject As Object, ev As Printing.PrintPageEventArgs) As PrintFlowControl
                Dim textWidth As Single = ev.Graphics.MeasureString(Text, font).Width
                Select Case _Align
                    Case TextAlignment.Default
                        'do nothing
                    Case TextAlignment.Left
                        currentPosition.X = 0
                    Case TextAlignment.Center
                        currentPosition.X = (ev.PageSettings.PaperSize.Width - textWidth) / 2
                    Case TextAlignment.Right
                        currentPosition.X = (ev.PageSettings.PaperSize.Width - textWidth)
                End Select

                currentPosition.X += xOffset
                'settings.Print(Text)
                ev.Graphics.DrawString(Text, font, Brushes.Black, currentPosition)
                currentPosition.Y += ev.Graphics.MeasureString(Text, font).ToSize.Height
                currentPosition.X = 0
                'currentPosition.Y += ev.Graphics.MeasureString(Text, font).Height
                Return PrintFlowControl.NextCommand
            End Function
        )
    End Sub
    Public Sub WriteChars(ByVal Text As String, Optional ByVal xOffset As Integer = 0)
        toPrint.Enqueue(
            Function(subject As Object, ev As Printing.PrintPageEventArgs) As PrintFlowControl
                Dim sTextWidth As Single = ev.Graphics.MeasureString(Text, font).Width
                Select Case _Align
                    Case TextAlignment.Default
                        'do nothing
                    Case TextAlignment.Left
                        currentPosition.X = 0
                    Case TextAlignment.Center
                        currentPosition.X = (ev.PageSettings.PaperSize.Width - sTextWidth) / 2
                    Case TextAlignment.Right
                        currentPosition.X = (ev.PageSettings.PaperSize.Width - sTextWidth)
                End Select

                currentPosition.X += xOffset
                'settings.Print(Text)
                ev.Graphics.DrawString(Text, font, Brushes.Black, New PointF(currentPosition.X, currentPosition.Y))
                currentPosition += New Point(ev.Graphics.MeasureString(Text, font).ToSize.Width, 0)
                'currentPosition.Y += ev.Graphics.MeasureString(Text, font).Height
                Return Nothing
            End Function
        )
    End Sub
    Public Sub CutPaper()
        toPrint.Enqueue(
            Function(sender As Object, ev As Printing.PrintPageEventArgs) As PrintFlowControl
                ' used to call Printer.NewPage()
                Return PrintFlowControl.EndPage
            End Function
        )
    End Sub
    Public Sub EndDoc()
        toPrint.Enqueue(
            Function(sender As Object, ev As Printing.PrintPageEventArgs) As PrintFlowControl
                Return PrintFlowControl.EndDoc
            End Function
        )
        AddHandler doc.PrintPage, AddressOf Print
        doc.Print()
    End Sub
#End Region
    Private Sub Print(sender As Object, ev As Printing.PrintPageEventArgs)
        Dim runCommand As Boolean = (toPrint.Count > 0)
        While runCommand
            Dim action As Func(Of Object, Printing.PrintPageEventArgs, PrintFlowControl) = toPrint.Dequeue()
            Dim actionResult As PrintFlowControl = action(sender, ev)
            Select Case actionResult
                Case PrintFlowControl.EndDoc
                    runCommand = False

                Case PrintFlowControl.EndPage
                    runCommand = ev.HasMorePages

                Case PrintFlowControl.NextCommand
                    runCommand = True
            End Select
            runCommand = (runCommand And toPrint.Count > 0)
        End While
    End Sub
End Class
