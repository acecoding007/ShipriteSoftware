Imports System.Data

Public Module _MailboxPackage
    Public Const MBOX As String = "Mailbox Holder"
    Public Const FEDEX_HAL As String = "FedEx Hold At Location"
    Public Const UPS_AP As String = "UPS Access Point"
    Public Const HOLD As String = "HOLD for Non-Mailbox"

    Public StoreOwner As New _baseContact
    Public Clerk As String
    Public ReceiptFontName As String
    Public pathForSig As String
    Public IsFedExWebEnabled As Boolean
    Public IsFedExHALEnabled As Boolean
    Public SignatureFolder As String

    Public Function Open_PackageProcessingCenter(ByVal path2ShipriteDb As String, ByVal path2MailboxPackagesDb As String, ByVal path2ReportsDb As String, ByVal clerkid As String) As Boolean
        Try
            'ShipRiteDb.path2db = path2ShipriteDb
            'ReportsDb.path2db = path2ReportsDbS
            _MailboxPackagesDB.path2db = path2MailboxPackagesDb
            pathForSig = _Files.Get_DirName(_MailboxPackagesDB.path2db)
            SignatureFolder = String.Format("{0}\Signatures", pathForSig)
            Clerk = clerkid
            ReceiptFontName = String.Empty ' assume.
            'Call ReportsDb.Get_ReceiptPrinterFontName(_MailboxPackage.ReceiptFontName)
            '_MailboxPackage.IsFedExWebEnabled = FedEx_Data2XML.Test_Load_TestCredentials() ' test only
            IsFedExWebEnabled = FedEx_Data2XML.Load_Credentials_FromDatabase(gDBpath)
            IsFedExHALEnabled = _MailboxPackage.IsFedExWebEnabled And (Not String.IsNullOrEmpty(FedEx_Data2XML.OriginLocationId) And Not String.IsNullOrEmpty(FedEx_Data2XML.ApplicationId))
            '
            If ShipRiteDb.Setup_GetAddress_StoreOwner(StoreOwner) Then
                Return True
            Else
                _MsgBox.ErrorMessage("Could not read DefaultShipFrom id value to retrieve Store Owner address object!", "Failed to get Store Owner address!", "Mailbox Package Processing Center!")
                Return False
            End If
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Public Sub Print_PackageNotice(ByVal package As MailboxPackageObject)
        '
        If 0 < My.Settings.PackageValet_JPNReceiptCopies Then
            Dim directprint As New PrinterClass(GetPolicyData(gReportsDB, "InvoicePrinter"), System.Windows.Forms.Application.StartupPath)
            '
            Dim receipt As New System.Text.StringBuilder
            For i As Integer = 1 To My.Settings.PackageValet_JPNReceiptCopies

                With directprint
                    .FontName = _MailboxPackage.ReceiptFontName

                    'Printing Title
                    .NormalFont()
                    .Bold = True
                    .GotoSixth(2)
                    .WriteLine(StoreOwner.CompanyName)
                    .GotoSixth(2)
                    .WriteLine(StoreOwner.Addr1)
                    .GotoSixth(2)
                    .WriteLine(StoreOwner.CityStateZip)
                    .GotoSixth(2)
                    .WriteLine(StoreOwner.Tel)
                    .FeedPaper(1)

                    .Bold = False

                    .WriteLine("_____________________________________________")
                    .FeedPaper(1)
                    .BigFont()
                    .Bold = True
                    .GotoSixth(1)
                    .WriteLine(String.Format("PACKAGE NOTICE", package.MailboxNo.ToString))
                    .Bold = False
                    .NormalFont()

                    .WriteLine("Please bring this notice to the counter")
                    .WriteLine("to pick up below shipment!")
                    .WriteLine("_____________________________________________")
                    .FeedPaper(1)

                    .BigFont()
                    .WriteLine(String.Format("BOX  #{0}", package.MailboxNo.ToString))
                    .NormalFont()

                    .FeedPaper(1)
                    .WriteLine(String.Format("TRA Last4:  {0}", _Controls.Right(package.TrackingNo, 4)))
                    .FeedPaper(1)
                    .WriteLine(String.Format("Name:  {0}", package.MailboxName))
                    .FeedPaper(0.5)
                    .WriteLine(String.Format("Carrier:  {0}", package.CarrierName))
                    .FeedPaper(0.5)
                    .WriteLine(String.Format("TRA:  {0}", package.TrackingNo))

                    ' Bar code:
                    .FontName = "CODE3OF9X1"
                    .FontSize = 36
                    .Bold = False
                    ''ol#1.1.86(1/14)... 'Print Notice' slip barcode is not scannable.
                    .WriteLine(String.Format("*{0}*", package.TrackingNo), -5)

                    .FontName = _MailboxPackage.ReceiptFontName
                    .NormalFont()
                    .FeedPaper(1)

                    .WriteChars(String.Format("Received:  {0}", package.ReceivedDate.Date.ToString("d")))
                    .WriteLine(String.Format("   Clerk:  {0}", _MailboxPackage.Clerk))
                    .FeedPaper(1)
                    .WriteLine("Picked up By:")
                    .FeedPaper(1)
                    .WriteLine("X____________________________________")
                    .FeedPaper(1)
                    If String.Empty = package.PickedupBy Then
                        .WriteLine("Picked up Date:")
                        .FeedPaper(1)
                    Else
                        .WriteLine(String.Format("    {0}", package.PickedupBy))
                        .WriteLine(String.Format("Picked up Date:  {0}", package.PickedupDate))
                        .FeedPaper(1)
                    End If

                    ''ol#1.2.03(6/3)... Notes and Location fields are added to Print Notice body.
                    If Not String.IsNullOrEmpty(package.Notes) Then
                        .WriteChars(String.Format("Notes:  {0}", package.Notes))
                        .FeedPaper(1)
                    End If
                    If Not String.IsNullOrEmpty(package.Location) Then
                        .WriteChars(String.Format("Location:  {0}", package.Location))
                        .FeedPaper(1)
                    End If

                    .CutPaper() ' Can be used with real printer to cut the paper.

                    'Ending the session
                    .EndDoc()
                End With
            Next i
        End If

    End Sub

    Public Sub Print_PackageNotices(ByVal lvListView As ListView, ByVal package As MailboxPackageObject)
        '
        'If 0 < My.Settings.JobPrintNotice_ReceiptPrinter_Copies Then
        Dim directprint As New PrinterClass(GetPolicyData(gReportsDB, "InvoicePrinter"), System.Windows.Forms.Application.StartupPath)
        '
        'For i As Integer = 1 To My.Settings.JobPrintNotice_ReceiptPrinter_Copies
        With directprint
            .FontName = _MailboxPackage.ReceiptFontName

            'Printing Title
            .NormalFont()
            .Bold = True
            .GotoSixth(2)
            .WriteLine(StoreOwner.CompanyName)
            .GotoSixth(2)
            .WriteLine(StoreOwner.Addr1)
            .GotoSixth(2)
            .WriteLine(StoreOwner.CityStateZip)
            .GotoSixth(2)
            .WriteLine(StoreOwner.Tel)
            .FeedPaper(2)

            .Bold = False
            .WriteLine(String.Format("BOX  #{0}", package.MailboxNo.ToString))
            .WriteLine(String.Format("Name:  {0}", package.MailboxName))
            .FeedPaper(3)

            For Each pkg As MailboxPackageObjectObservable In lvListView.SelectedItems
                .WriteLine(String.Format("Carrier: {0}", pkg.CarrierName))
                ''ol#1.2.22(10/29)... Tracking# moved to its own row to always fit on the receipt.
                .WriteLine(String.Format("TRA: {0}", pkg.TrackingNo))
                .WriteChars(String.Format("Received Date:  {0}", pkg.ReceivedDate))
                .FeedPaper(2)
            Next

            .FeedPaper(2)

            .WriteChars("Picked up By:")
            .FeedPaper(2)
            .WriteLine("X____________________________________")
            .GotoSixth(2)
            .WriteChars(package.PickedupBy)

            .FeedPaper(2)
            .WriteChars(String.Format("Picked up Date: {0}", package.PickedupDate.ToString))
            .FeedPaper(1)

            .CutPaper() ' Can be used with real printer to cut the paper.

            'Ending the session
            .EndDoc()

        End With
        'Next i
        '
        'End If

    End Sub

    Public Sub Print_PackageNotice_Label(ByVal package As MailboxPackageObject)
        '
        If 0 < My.Settings.PackageValet_JPNLabelCopies Then
            Dim lbl As New System.Text.StringBuilder
            '
            ''AP(05/30/2017) - Clear image buffer before/after printing Check In 'Print Notice' and 'Print Label' labels.
            ' Clear bitmap buffer (ZPL):
            RawPrinterHelper.SendStringToPrinter(GetPolicyData(gReportsDB, "LabelPrinter"), String.Format("{0}^XA^MCY^XZ", Environment.NewLine))
            '
            ' Header:
            ''AP(05/30/2017) - Clear image buffer before/after printing Check In 'Print Notice' and 'Print Label' labels.
            lbl.Append(String.Format("{0}N{0}Q1320,24{0}R5,8{0}ZT{0}S3{0}D15{0}", Environment.NewLine))
            'lbl.Append(String.Format("N{0}Q1320,24{0}R5,8{0}ZT{0}S3{0}D15{0}", Environment.NewLine))
            '
            ' Company:
            lbl.Append(String.Format("A55,40,0,3,1,2,N,{1}{2}{1}{0}", Environment.NewLine, Chr(34), StoreOwner.CompanyName))
            lbl.Append(String.Format("A55,90,0,3,1,2,N,{1}{2}{1}{0}", Environment.NewLine, Chr(34), StoreOwner.Addr1))
            lbl.Append(String.Format("A55,140,0,3,1,2,N,{1}{2}{1}{0}", Environment.NewLine, Chr(34), StoreOwner.City))
            lbl.Append(String.Format("A55,190,0,3,1,2,N,{1}{2}{1}{0}", Environment.NewLine, Chr(34), StoreOwner.Tel))
            '
            lbl.Append(String.Format("A440,50,0,4,2,2,N,{1}Box: {2}{1}{0}", Environment.NewLine, Chr(34), package.MailboxNo))
            lbl.Append(String.Format("A440,150,0,3,1,2,N,{1}TRA Last4: {2}{1}{0}", Environment.NewLine, Chr(34), _Controls.Right(package.TrackingNo, 4)))

            lbl.AppendLine("LO55,250,800,5")
            lbl.Append(String.Format("A200,280,0,4,2,3,N,{1}{2}{1}{0}", Environment.NewLine, Chr(34), "PACKAGE NOTICE"))
            lbl.Append(String.Format("A100,400,0,2,1,2,N,{1}{2}{1}{0}", Environment.NewLine, Chr(34), "Please Bring Notice to counter to pick up below shipment!"))
            lbl.AppendLine("LO55,480,800,5")


            lbl.Append(String.Format("A100,500,0,3,1,2,N,{1}Name: {2}{1}{0}", Environment.NewLine, Chr(34), package.MailboxName))
            lbl.Append(String.Format("A100,560,0,3,1,2,N,{1}Carrier: {2}{1}{0}", Environment.NewLine, Chr(34), package.CarrierName))
            '
            ' Tracking #:
            lbl.Append(String.Format("A100,620,0,3,1,2,N,{1}TRA: {2}{1}{0}", Environment.NewLine, Chr(34), package.TrackingNo))
            lbl.Append(String.Format("B100,660,0,1,3,3,150,N,{1}{2}{1}{0}", Environment.NewLine, Chr(34), package.TrackingNo))
            '
            ' Date Package Received: 
            ''ol#1.2.01(5/26)... Received date on Notices always has 12:00:00am time.
            lbl.Append(String.Format("A100,840,0,3,1,2,N,{1}Received: {2}{1}{0}", Environment.NewLine, Chr(34), Date.Today.ToString("d")))
            lbl.Append(String.Format("A100,900,0,3,1,2,N,{1}Clerk: {2}{1}{0}", Environment.NewLine, Chr(34), _MailboxPackage.Clerk))
            '
            If Not String.Empty = package.PickedupBy Then
                ' Pickedup By:
                lbl.Append(String.Format("A100,950,0,3,1,2,N,{1}Picked up By: {2}{1}{0}", Environment.NewLine, Chr(34), String.Empty))
                lbl.AppendLine("LO290,990,400,3")
                lbl.Append(String.Format("A290,990,0,3,1,2,N,{1}{2}{1}{0}", Environment.NewLine, Chr(34), package.PickedupBy))
                '
                ' Pickedup Date:
                lbl.Append(String.Format("A100,1040,0,3,1,2,N,{1}Date: {2}{1}{0}", Environment.NewLine, Chr(34), package.PickedupDate))
                '
            Else
                ' Pickedup By:
                lbl.Append(String.Format("A100,950,0,3,1,2,N,{1}Picked up By: {2}{1}{0}", Environment.NewLine, Chr(34), String.Empty))
                lbl.AppendLine("LO290,990,400,3")
                '
                ' Pickedup Date:
                lbl.Append(String.Format("A100,1040,0,3,1,2,N,{1}Date: {2}{1}{0}", Environment.NewLine, Chr(34), String.Empty))
                lbl.AppendLine("LO190,1070,500,3")
                '
            End If
            '
            ''ol#1.2.03(6/3)... Notes and Location fields are added to Print Notice body.
            If Not String.IsNullOrEmpty(package.Notes) And Not String.IsNullOrEmpty(package.Location) Then
                lbl.Append(String.Format("A100,1090,0,3,1,2,N,{1}Notes: {2}{1}{0}", Environment.NewLine, Chr(34), package.Notes))
                lbl.Append(String.Format("A100,1140,0,3,1,2,N,{1}Location: {2}{1}{0}", Environment.NewLine, Chr(34), package.Location))
            ElseIf Not String.IsNullOrEmpty(package.Notes) Then
                lbl.Append(String.Format("A100,1110,0,3,1,2,N,{1}Notes: {2}{1}{0}", Environment.NewLine, Chr(34), package.Notes))
            ElseIf Not String.IsNullOrEmpty(package.Location) Then
                lbl.Append(String.Format("A100,1110,0,3,1,2,N,{1}Location: {2}{1}{0}", Environment.NewLine, Chr(34), package.Location))
            End If
            '
            ' Final Line:
            lbl.Append(String.Format("P1{0}N", Environment.NewLine))
            '
            ' Print:
            For i As Integer = 1 To My.Settings.PackageValet_JPNLabelCopies
                RawPrinterHelper.SendStringToPrinter(GetPolicyData(gReportsDB, "LabelPrinter"), lbl.ToString)
            Next i
            '
            ''AP(05/30/2017) - Clear image buffer before/after printing Check In 'Print Notice' and 'Print Label' labels.
            ' Clear bitmap buffer (ZPL):
            RawPrinterHelper.SendStringToPrinter(GetPolicyData(gReportsDB, "LabelPrinter"), String.Format("{0}^XA^MCY^XZ", Environment.NewLine))
            '
        End If
        '
    End Sub
    Public Sub Print_PackageLabel_v1(ByVal package As MailboxPackageObject)
        'Removed isExpired because it is not being used in the function below anymore

        If 0 < My.Settings.PackageValet_JPLLabelCopies Then
            Dim lbl As New System.Text.StringBuilder
            '
            ''AP(05/30/2017) - Clear image buffer before/after printing Check In 'Print Notice' and 'Print Label' labels.
            ' Clear bitmap buffer (ZPL):
            RawPrinterHelper.SendStringToPrinter(GetPolicyData(gReportsDB, "LabelPrinter"), String.Format("{0}^XA^MCY^XZ", Environment.NewLine))
            '
            ' Header:
            ''AP(05/30/2017) - Clear image buffer before/after printing Check In 'Print Notice' and 'Print Label' labels.
            lbl.Append(String.Format("{0}N{0}Q1320,24{0}R5,8{0}ZT{0}S3{0}D15{0}", Environment.NewLine))
            'lbl.Append(String.Format("N{0}Q1320,24{0}R5,8{0}ZT{0}S3{0}D15{0}", Environment.NewLine))
            '
            ''ol#1.2.53(5/25)... Check In 'Print Notice' and 'Print Label' labels sometimes are overlapping each other if printed at the same time.
            '' Last 4 Tracking #:
            'lbl.Append(String.Format("A50,40,0,5,1,2,N,{1}{2}{1}{0}", Environment.NewLine, Chr(34), _Controls.Right(package.TrackingNo, 4)))
            ''
            'lbl.Append(String.Format("A410,90,0,4,2,2,N,{1}Box: {2}{1}{0}", Environment.NewLine, Chr(34), package.MailboxNo))

            lbl.Append(String.Format("A55,20,0,3,1,2,N,{1}{2}{1}{0}", Environment.NewLine, Chr(34), "Mailbox:"))
            lbl.Append(String.Format("A55,120,0,3,5,9,N,{1}{2}{1}{0}", Environment.NewLine, Chr(34), package.MailboxNo))

            lbl.Append(String.Format("A500,20,0,3,1,2,N,{1}{2}{1}{0}", Environment.NewLine, Chr(34), "Tracking Last4"))
            lbl.Append(String.Format("A500,120,0,3,4,6,N,{1}{2}{1}{0}", Environment.NewLine, Chr(34), _Controls.Right(package.TrackingNo, 4)))

            lbl.Append(String.Format("LO55,350,275,10{0}", Environment.NewLine))
            lbl.Append(String.Format("A375,340,0,2,1,2,N,{1}{2}{1}{0}", Environment.NewLine, Chr(34), "FOLD HERE"))
            lbl.Append(String.Format("LO510,350,275,10{0}", Environment.NewLine))


            lbl.Append(String.Format("A55,440,0,3,1,2,N,{1}{2}{1}{0}", Environment.NewLine, Chr(34), "Mailbox:"))
            lbl.Append(String.Format("A55,520,0,3,5,9,N,{1}{2}{1}{0}", Environment.NewLine, Chr(34), package.MailboxNo))

            lbl.Append(String.Format("A500,440,0,3,1,2,N,{1}{2}{1}{0}", Environment.NewLine, Chr(34), "Tracking Last4"))
            lbl.Append(String.Format("A500,520,0,3,4,6,N,{1}{2}{1}{0}", Environment.NewLine, Chr(34), _Controls.Right(package.TrackingNo, 4)))



            lbl.Append(String.Format("A55,710,0,3,2,4,N,{1}{2}{1}{0}", Environment.NewLine, Chr(34), package.MailboxName))
            lbl.Append(String.Format("A100,840,0,3,1,2,N,{1}Carrier: {2}{1}{0}", Environment.NewLine, Chr(34), package.CarrierName))           '
            ' Tracking #:
            lbl.Append(String.Format("A100,880,0,3,1,2,N,{1}TRA: {2}{1}{0}", Environment.NewLine, Chr(34), package.TrackingNo))
            lbl.Append(String.Format("B100,920,0,1,3,3,150,N,{1}{2}{1}{0}", Environment.NewLine, Chr(34), package.TrackingNo))
            '
            ' Date Package Received:
            lbl.Append(String.Format("A100,1100,0,3,1,2,N,{1}{2}{1}{0}", Environment.NewLine, Chr(34), Now()))
            '
            ''ol#1.2.22(10/29)... If Mailbox is expired during package check in then Expiration Note will be printed on the package label.
            ''If isExpired Then ''ol#1.2.23(11/3)... PMB Expired notice prints for every Mailbox on every Package Label.
            ''    lbl.Append(String.Format("A60,900,0,4,1,2,N,{1}{2}{1}{0}", Environment.NewLine, Chr(34), "!!! PMB EXPIRED, PLEASE RENEW AT PICKUP !!!"))
            ''End If
            ''ol#1.2.03(6/3)... Notes and Location fields are added to Print Notice body.
            If Not String.IsNullOrEmpty(package.Notes) And Not String.IsNullOrEmpty(package.Location) Then
                lbl.Append(String.Format("A100,1150,0,3,1,2,N,{1}Notes: {2}{1}{0}", Environment.NewLine, Chr(34), package.Notes))
                lbl.Append(String.Format("A100,1190,0,3,1,2,N,{1}Location: {2}{1}{0}", Environment.NewLine, Chr(34), package.Location))
            ElseIf Not String.IsNullOrEmpty(package.Notes) Then
                lbl.Append(String.Format("A100,1170,0,3,1,2,N,{1}Notes: {2}{1}{0}", Environment.NewLine, Chr(34), package.Notes))
            ElseIf Not String.IsNullOrEmpty(package.Location) Then
                lbl.Append(String.Format("A100,1170,0,3,1,2,N,{1}Location: {2}{1}{0}", Environment.NewLine, Chr(34), package.Location))
            End If

            ' Final Line:
            lbl.Append(String.Format("P1{0}N", Environment.NewLine))
            '
            ' Print:
            For i As Integer = 1 To My.Settings.PackageValet_JPLLabelCopies
                RawPrinterHelper.SendStringToPrinter(GetPolicyData(gReportsDB, "LabelPrinter"), lbl.ToString)
            Next i
            '
            ''AP(05/30/2017) - Clear image buffer before/after printing Check In 'Print Notice' and 'Print Label' labels.
            ' Clear bitmap buffer (ZPL):
            RawPrinterHelper.SendStringToPrinter(GetPolicyData(gReportsDB, "LabelPrinter"), String.Format("{0}^XA^MCY^XZ", Environment.NewLine))
            '
        End If
        '
    End Sub
    Public Sub Print_SignatureSheet_Report(ByRef lvListView As ListView)
        If My.Settings.PackageValet_JSSLaserCopies > 0 Then
            Dim dTable As DataTable = DatabaseFunctions.GetEmptyDataTable(_MailboxPackagesDB.path2db, "Mailbox_Packages")
            Dim repobject As New ShipRiteReports._ReportObject
            repobject.ReportName = "PackagesPickup_SignatureSheet.rpt"
            repobject.IsPreviewReport = False

            repobject.ReportParameters.Add(StoreOwner.CompanyName)
            repobject.ReportParameters.Add(StoreOwner.Addr1)
            repobject.ReportParameters.Add(StoreOwner.CityStateZip)

            repobject.NumberOfCopies = My.Settings.PackageValet_JSSLaserCopies

            For Each pkg As MailboxPackageObjectObservable In lvListView.Items
                Dim drow As DataRow = dTable.NewRow
                drow("MailboxNo") = pkg.MailboxNo
                drow("MailboxName") = pkg.MailboxName
                drow("carriername") = pkg.CarrierName
                drow("trackingno") = pkg.TrackingNo
                dTable.Rows.Add(drow)
            Next pkg

            If dTable.Rows.Count > 0 Then
                repobject.DatabaseTables.Add(dTable)
                ShipRiteReports.Execute_DataTable(repobject)
            End If

            dTable.Dispose()
        End If
    End Sub
    Public Sub Print_PICK_Ticket_Report(ByRef lvListView As ListView)

        'Rewrite once reports module Is in place
        Dim dtable = DatabaseFunctions.GetEmptyDataTable(_MailboxPackagesDB.path2db, "Mailbox_Packages")
        If dtable IsNot Nothing Then
            Dim repobject As New ShipRiteReports._ReportObject
            repobject.ReportName = "PICK_LocationTicket.rpt"
            repobject.IsPreviewReport = False

            repobject.ReportParameters.Add(StoreOwner.CompanyName)
            repobject.ReportParameters.Add(StoreOwner.Addr1)
            repobject.ReportParameters.Add(StoreOwner.CityStateZip)

            repobject.NumberOfCopies = 1

            'For Each lvItem As ListViewItem In lvListView.CheckedItems
            '    Dim obj As _ListItemWithObject = lvItem.Tag
            '    Dim drow As DataRow = dtable.NewRow
            '    drow("MailboxNo") = obj.ItemIndex
            '    drow("MailboxName") = obj.ItemText
            '    drow("CarrierName") = lvItem.Text
            '    drow("TrackingNo") = lvItem.SubItems(1).Text
            '    'drow("ReceivedDate") = lvItem.SubItems(2).Text
            '    drow("Location") = lvItem.SubItems(3).Text
            '    drow("CheckInNotes") = lvItem.SubItems(4).Text
            '    dtable.Rows.Add(drow)
            'Next lvItem

            Dim drow As DataRow
            For Each pkg As MailboxPackageObjectObservable In lvListView.SelectedItems
                drow = dtable.NewRow
                drow("MailboxNo") = pkg.MailboxNo
                drow("MailboxName") = pkg.MailboxName
                drow("CarrierName") = pkg.CarrierName
                drow("TrackingNo") = pkg.TrackingNo
                drow("Location") = pkg.Location
                drow("CheckInNotes") = pkg.CheckInNotes
                dtable.Rows.Add(drow)
            Next

            If dtable.Rows.Count > 0 Then
                repobject.DatabaseTables.Add(dtable)
                ShipRiteReports.Execute_DataTable(repobject)
            End If
        End If
        dtable.Dispose()
        'Rewrite once reports module Is in place

    End Sub
End Module

Public Class MailboxPackageObject
    Public MailboxNo As Long
    Public MailboxName As String
    Public CarrierName As String
    Public TrackingNo As String
    Public ReceivedDate As Date
    Public PickedupBy As String
    Public PickedupDate As Date
    ''ol#1.2.03(6/3)... Notes and Location fields are added to Print Notice body.
    Public Notes As String
    Public Location As String
    ''ol#1.2.41(7/11)... 'Hold for Pickup' will be integrated with Mailbox 'Package Check In/Out'.
    Public BarCodeScan As String
    Public CustomerID As Long
    Public PackageClass As String
    Public Email As String
    Public SMS As String
    Public CellCarrier As String
    Public SignatureFile As String
    Public IsGround As Boolean
    Public ID As String
    Public Overrides Function ToString() As String
        Return MailboxName
    End Function
    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub
End Class
