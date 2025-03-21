Imports Microsoft.VisualBasic
Imports System.Data
Imports CrystalDecisions.CrystalReports.Engine
Imports CrystalDecisions.Shared
Imports System.Drawing.Printing

'NOTE: Current data sources pointing to SRPro .dsn files.
'TODO: Update data sources for all reports to point to SRN .dsn files in C:\ShipriteNext\Reports\DataSources\ folder.
Namespace ShipRiteReports

    Public Class ReportViewer



    End Class

    Public Class _ReportObject
        Public ReportName As String
        Public ReportFormula As String
        Public ReportParameters As New Collection
        Public SubReports As New Collection
        Public ReportDataSource As String
        Public DatabaseTables As New Collection ' list of db tables' names or select statements  used in a report
        Public DatabasePath As String
        Public PrinterName As String
        Public IsPreviewReport As Boolean
        Public NumberOfCopies As Short
        Public ReportSaveAsPath As String
        Public EmailAddress As String
        Public EmailCustomerName As String
        Public GlobalDBPath As String
        Public IsSaveAsOnly As Boolean
    End Class

    Public Module _RemoteReport
        Public Sub Execute(ByVal reportPath As String, ByVal reportFormula As String)
            Dim rdoc As New ReportDocument
            rdoc.Load(reportPath)
            If Not reportFormula = String.Empty Then
                rdoc.RecordSelectionFormula = reportFormula
            End If
            Dim rViewer As New ReportViewer()
            rViewer.CRViewer.ViewerCore.ReportSource = rdoc
            rViewer.ShowDialog()
            rdoc.Dispose()
        End Sub
        Public Sub Execute(ByVal reportPath As String, ByVal dTable As DataTable)
            Dim rdoc As New ReportDocument
            rdoc.Load(reportPath)
            rdoc.SetDataSource(dTable)
            Dim rViewer As New ReportViewer()
            rViewer.CRViewer.ViewerCore.ReportSource = rdoc
            rViewer.ShowDialog()
            rdoc.Dispose()
        End Sub
    End Module

    Public Module _LocalReport
        Public Sub InitSAPobjects()
            ' This report is only for Initializing SAP objects on start-up of ShipRite 
            ' to speedup the opening of the actual reports.
            Dim reportsource As New ReportDocument
            reportsource = New ShipRiteReports.InitCRobjectsOnly
            Dim rViewer As New ReportViewer()
            rViewer.CRViewer.ViewerCore.ReportSource = reportsource
            reportsource.Dispose()

            '' Unnecessary in SRN?
            '' write current Setup_Gateway2ShipRite project version
            'Call Setup_VersionNumber.Version.Write_ShipRiteNet_version()
        End Sub

        Private Function IsReportSourceFound(ByVal reportName As String, ByRef reportSource As ReportDocument, ByRef printLayout As PrintLayoutSettings) As Boolean
            IsReportSourceFound = True ' assume
            Select Case reportName

                'Case "x.rpt" : reportsource = New ShipRiteReports.x

                ' AR:


                Case "AlphaList.rpt" : reportSource = New ShipRiteReports.AlphaList
                Case "Vault.rpt" : reportSource = New ShipRiteReports.Vault
                ' Drop Off
                Case "DropOff.rpt" : reportSource = New ShipRiteReports.DropOff
                Case "DropOff_byCustomer.rpt" : reportSource = New ShipRiteReports.DropOff_byCustomer
                Case "DropOff_Manifest.rpt" : reportSource = New ShipRiteReports.DropOff_Manifest
                Case "DropOff_Compensation.rpt" : reportSource = New ShipRiteReports.DropOff_Compensation
                ' HotPursuit
                Case "AddressListing.rpt" : reportSource = New ShipRiteReports.AddressListing
                Case "AddressListingLevel.rpt" : reportSource = New ShipRiteReports.AddressListingLevel
                Case "ContactSheet.rpt" : reportSource = New ShipRiteReports.ContactSheet
                Case "Contacts.rpt" : reportSource = New ShipRiteReports.Contacts
                ' Inventory
                Case "Inventory.rpt" : reportSource = New ShipRiteReports.Inventory

                Case "Inventory_OutStock.rpt" : reportSource = New ShipRiteReports.Inventory_OutStock
                Case "InventoryValuation.rpt" : reportSource = New ShipRiteReports.InventoryValuation
                Case "TicklerList.rpt" : reportSource = New ShipRiteReports.TicklerList
                Case "PurchaseOrder.rpt" : reportSource = New ShipRiteReports.PurchaseOrder
                Case "SerialNumbersUnassigned.rpt" : reportSource = New ShipRiteReports.SerialNumbersUnassigned
                ' Inventory from Reports folder
                ''TODO: Update data source. Currently pointing to some random Address.mdb database?
                Case "AddressLabels_DYMO1.1-8x3.1-2.rpt" : reportSource = New ShipRiteReports.AddressLabels_DYMO11_8x31_2
                Case "InventoryLabels.rpt" : reportSource = New ShipRiteReports.InventoryLabels



                Case "InventoryLabels_byDept.rpt" : reportSource = New ShipRiteReports.InventoryLabels_byDept
                Case "InventoryLabels_DYMO1.1-8x3.1-2.rpt" : reportSource = New ShipRiteReports.InventoryLabels_DYMO11_8x31_2
                ' MailBox:
                Case "PS1583.rpt" : reportSource = New ShipRiteReports.PS1583
                Case "MBContract.rpt" : reportSource = New ShipRiteReports.MBContract
                Case "MBXAlphaListing.rpt" : reportSource = New ShipRiteReports.MBXAlphaListing
                Case "MailboxListing.rpt" : reportSource = New ShipRiteReports.MailboxListing
                Case "PostOffice.rpt" : reportSource = New ShipRiteReports.PostOffice
                Case "PostOffice_CancelledMBoxes.rpt" : reportSource = New ShipRiteReports.PostOffice_CancelledMBoxes
                Case "MBXNotice.rpt" : reportSource = New ShipRiteReports.MBXNotice
                ' Mailbox Packages:
                Case "PackagesOnHand_NotSigned.rpt" : reportSource = New ShipRiteReports.PackagesOnHand_NotSigned
                Case "PackagesOnHand_Signed.rpt" : reportSource = New ShipRiteReports.PackagesOnHand_Signed
                Case "PackagesHistory.rpt" : reportSource = New ShipRiteReports.PackagesHistory
                Case "PackagesPickup_SignatureSheet.rpt" : reportSource = New ShipRiteReports.PackagesPickup_SignatureSheet
                Case "PICK_LocationTicket.rpt" : reportSource = New ShipRiteReports.PICK_LocationTicket
                Case "ProofOfPickup_SignatureSheet.rpt" : reportSource = New ShipRiteReports.ProofOfPickup_SignatureSheet
                ' Hold for Pickup:
                Case "Packages_OnHand.rpt" : reportSource = New ShipRiteReports.Packages_OnHand
                Case "Packages_CheckOut.rpt" : reportSource = New ShipRiteReports.Packages_CheckOut
                Case "PackagesHistory_ByCustomer.rpt" : reportSource = New ShipRiteReports.PackagesHistory_ByCustomer
                Case "PackagesHistory_ByCarrier.rpt" : reportSource = New ShipRiteReports.PackagesHistory_ByCarrier
                ' POS Manager:
                Case "ZReportShort_2Column.rpt" : reportSource = New ShipRiteReports.ZReportShort_2Column
                Case "ZReportShort.rpt" : reportSource = New ShipRiteReports.ZReportShort
                Case "ZReport.rpt" : reportSource = New ShipRiteReports.Zreport
                Case "Zreport_PostNet.rpt" : reportSource = New ShipRiteReports.Zreport_PostNet
                Case "ZreportDatewise.rpt" : reportSource = New ShipRiteReports.ZreportDatewise
                Case "Invoice_T1_T2_T3.rpt" : reportSource = New ShipRiteReports.Invoice_T1_T2_T3
                Case "Invoice.rpt" : reportSource = New ShipRiteReports.Invoice
                Case "Product.rpt" : reportSource = New ShipRiteReports.Product
                Case "ProdAcct.rpt" : reportSource = New ShipRiteReports.ProdAcct
                Case "ProdClrk.rpt" : reportSource = New ShipRiteReports.ProdClrk
                Case "Sales.rpt" : reportSource = New ShipRiteReports.Sales
                Case "SaleAcct.rpt" : reportSource = New ShipRiteReports.SaleAcct
                Case "SaleClrk.rpt" : reportSource = New ShipRiteReports.SaleClrk
                Case "SalesTax.rpt" : reportSource = New ShipRiteReports.SalesTax
                Case "SalesTax_Canada.rpt" : reportSource = New ShipRiteReports.SalesTax_Canada
                Case "SalesTax_Accts.rpt" : reportSource = New ShipRiteReports.SalesTax_Accts
                Case "Aging.rpt" : reportSource = New ShipRiteReports.Aging
                Case "Void.rpt" : reportSource = New ShipRiteReports.Void
                Case "Hourly.rpt" : reportSource = New ShipRiteReports.Hourly
                Case "HourlyByDay.rpt" : reportSource = New ShipRiteReports.HourlyByDay
                Case "SalesInquiry.rpt" : reportSource = New ShipRiteReports.SalesInquiry
                Case "StatementOld.rpt" : reportSource = New ShipRiteReports.StatementOld
                Case "Statement.rpt" : reportSource = New ShipRiteReports.Statement
                Case "StatementSaleDetails.rpt" : reportSource = New ShipRiteReports.StatementSaleDetails
                Case "StatementNoDetails.rpt" : reportSource = New ShipRiteReports.StatementNoDetails
                Case "Quote.rpt" : reportSource = New ShipRiteReports.Quote
                Case "Quote_T1_T2_T3.rpt" : reportSource = New ShipRiteReports.Quote_T1_T2_T3
                Case "LayAwayStore.rpt" : reportSource = New ShipRiteReports.LayAwayStore
                Case "LayAwayInvoice.rpt" : reportSource = New ShipRiteReports.LayAwayInvoice
                Case "LayAwayShipper.rpt" : reportSource = New ShipRiteReports.LayAwayShipper
                Case "ShipperCopy.rpt" : reportSource = New ShipRiteReports.ShipperCopy
                Case "DepartmentChargeBack.rpt" : reportSource = New ShipRiteReports.DepartmentChargeback
                Case "GeneralJournal.rpt" : reportSource = New ShipRiteReports.GeneralJournal
                Case "InvoiceNumberInventory.rpt" : reportSource = New ShipRiteReports.InvoiceNumberInventory
                ' Receipt Slips:
                Case "DrawerOpen.rpt" : reportSource = New ShipRiteReports.DrawerOpen : printLayout.Centered = False
                Case "DrawerClose.rpt" : reportSource = New ShipRiteReports.DrawerClose : printLayout.Centered = False
                Case "DepositSlip.rpt" : reportSource = New ShipRiteReports.DepositSlip : printLayout.Centered = False
                Case "PaidOut.rpt" : reportSource = New ShipRiteReports.PaidOut : printLayout.Centered = False
                Case "RefundSlip.rpt" : reportSource = New ShipRiteReports.RefundSlip : printLayout.Centered = False
                ' Shipping
                Case "ExportDocument.rpt" : reportSource = New ShipRiteReports.ExportDocument
                Case "FedEx_IntConditionsOfContract.rpt" : reportSource = New ShipRiteReports.FedEx_IntConditionsOfContract
                Case "ShippingReceipt.rpt" : reportSource = New ShipRiteReports.ShippingReceipt
                Case "FedEx_EOD_Manifest.rpt" : reportSource = New ShipRiteReports.FedEx_EOD_Manifest
                Case "UPS_EOD_Manifest.rpt" : reportSource = New ShipRiteReports.UPS_EOD_Manifest
                Case "DHL_EOD_Manifest.rpt" : reportSource = New ShipRiteReports.DHL_EOD_Manifest
                Case "Corder_Original.rpt" : reportSource = New ShipRiteReports.Corder_Original
                Case "Corder_summary.rpt" : reportSource = New ShipRiteReports.Corder_summary


                Case "Corder.rpt" : reportSource = New ShipRiteReports.Corder
                Case "Zorder.rpt" : reportSource = New ShipRiteReports.Zorder
                Case "Invorder.rpt" : reportSource = New ShipRiteReports.Invorder
                Case "Insurance.rpt" : reportSource = New ShipRiteReports.Insurance
                Case "DSIeodReport.rpt" : reportSource = New ShipRiteReports.DSIeodReport
                Case "FEDEXBatch.rpt" : reportSource = New ShipRiteReports.FEDEXBatch
                Case "FedExSRdisclaimer.rpt" : reportSource = New ShipRiteReports.FedExSRdisclaimer
                Case "SpeeDeeEODManifest.rpt" : reportSource = New ShipRiteReports.SpeeDeeEODManifest
                ' Price Charts
                Case "FedExi1st.rpt" : reportSource = New ShipRiteReports.FedExi1st
                Case "FedExIntlChart.rpt" : reportSource = New ShipRiteReports.FedExIntlChart
                Case "PriceChart.rpt" : reportSource = New ShipRiteReports.PriceChart
                Case "UPSIntl.rpt" : reportSource = New ShipRiteReports.UPSIntl
                Case "UPSIntl2.rpt" : reportSource = New ShipRiteReports.UPSIntl2
                Case "USPSiExp.rpt" : reportSource = New ShipRiteReports.USPSiExp
                Case "USPSIntl.rpt" : reportSource = New ShipRiteReports.USPSIntl
                Case "USPSpp.rpt" : reportSource = New ShipRiteReports.USPSpp
                ' Users:
                Case "TimeClock.rpt" : reportSource = New ShipRiteReports.TimeClock

                Case "Letter.rpt" : reportSource = New ShipRiteReports.Letter
                Case "Avery_5160.rpt" : reportSource = New ShipRiteReports.Avery_5160
                Case "DropOff_FASC_Compensation.rpt" : reportSource = New ShipRiteReports.DropOff_FASC_Compensation
                Case "DropOff_UPS_Compensation.rpt" : reportSource = New ShipRiteReports.DropOff_UPS_Compensation
                Case "InvoiceSearch.rpt" : reportSource = New ShipRiteReports.InvoiceSearch
                Case "Mailboxes_Expired.rpt" : reportSource = New ShipRiteReports.Mailboxes_Expired






                Case Else
                    MsgBox("Add this report to IsReportSourceFound() function", MsgBoxStyle.Critical)
                    IsReportSourceFound = False
            End Select
        End Function
        Private Function IsValid_ReportPrinterName(ByVal printerName As String) As Boolean
            IsValid_ReportPrinterName = False ' assume
            For i = 0 To PrinterSettings.InstalledPrinters.Count - 1
                If printerName = PrinterSettings.InstalledPrinters.Item(i) Then
                    IsValid_ReportPrinterName = True
                    Exit For
                End If
            Next i
        End Function

        Private Function Set_ReportParameters(ByVal rParams As Object, ByRef reportSource As ReportDocument) As Boolean
            If rParams IsNot Nothing Then
                For i As Integer = 1 To rParams.Count
                    reportSource.SetParameterValue(i - 1, rParams(i))
                Next i
            End If
            Set_ReportParameters = True
        End Function
        Private Function Set_SubReportFormulas(ByVal subReports As Object, ByRef reportSource As ReportDocument) As Boolean
            If subReports IsNot Nothing Then
                For Each repdoc As ReportDocument In reportSource.Subreports
                    If _Collection.IsItemExist(subReports, repdoc.Name) Then
                        repdoc.RecordSelectionFormula = subReports(repdoc.Name)
                    End If
                Next repdoc
            End If
            Set_SubReportFormulas = True
        End Function

        Private Function Get_ReportDataSource(ByVal reportObj As Object, ByRef dSet As DataSet) As Boolean
            If reportObj.DatabaseTables IsNot Nothing Then
                For i As Integer = 1 To reportObj.DatabaseTables.Count
                    Dim dtablename As String = _Controls.Extract_TableName_FromSQLStatement(reportObj.DatabaseTables(i))
                    Dim sql2exe As String = String.Empty
                    If String.Empty = dtablename Then
                        dtablename = reportObj.DatabaseTables(i)
                        sql2exe = "Select * From " & dtablename
                    Else
                        sql2exe = reportObj.DatabaseTables(i) ' select statement was passed through
                    End If
                    If Not Create_DataSet_ShipRiteReports(dtablename, sql2exe, dSet) Then
                        Return False
                    End If
                Next i
            End If
            Get_ReportDataSource = True
        End Function

        Public Sub Execute_DataTable(ByVal reportObj As _ReportObject)
            Dim reportsource As New ReportDocument
            Dim prter As New PrinterSettings
            Dim playout As New PrintLayoutSettings
            ''
            prter.Copies = Val(reportObj.NumberOfCopies)
            'ShipRiteDb.path2db = reportobj.DatabasePath ' overwite default database path if needed.
            If IsValid_ReportPrinterName(reportObj.PrinterName) Then
                ' cannot reset from default to report printer yet...
                prter.PrinterName = reportObj.PrinterName
                reportsource.PrintOptions.PrinterName = reportObj.PrinterName
            End If
            If IsReportSourceFound(reportObj.ReportName, reportsource, playout) Then
                Dim dtable As DataTable = reportObj.DatabaseTables.Item(1) ' always only one data-table
                reportsource.SetDataSource(dtable) ' error from vb6 !!!
                If Set_ReportParameters(reportObj.ReportParameters, reportsource) Then
                    If Not reportObj.ReportFormula = String.Empty Then
                        reportsource.RecordSelectionFormula = reportObj.ReportFormula
                    End If
                    Dim rViewer As New ReportViewer()
                    rViewer.CRViewer.ViewerCore.ReportSource = reportsource
                    If reportObj.IsPreviewReport Then
                        rViewer.ShowDialog()
                    Else
                        Dim page As New Drawing.Printing.PageSettings
                        reportsource.PrintToPrinter(prter, page, False)
                    End If
                End If
                dtable.Dispose()
            End If
            prter = Nothing
            reportsource.Dispose()
        End Sub
        Public Sub Execute_DataSet(ByVal reportObj As _ReportObject)
            Dim reportsource As New ReportDocument
            Dim prter As New PrinterSettings
            Dim playout As New PrintLayoutSettings
            ''
            prter.Copies = Val(reportObj.NumberOfCopies)
            'ShipRiteDb.path2db = reportobj.DatabasePath ' overwite default database path if needed.
            If IsValid_ReportPrinterName(reportObj.PrinterName) Then
                ' cannot reset from default to report printer yet...
                prter.PrinterName = reportObj.PrinterName
                reportsource.PrintOptions.PrinterName = reportObj.PrinterName
            End If
            If IsReportSourceFound(reportObj.ReportName, reportsource, playout) Then
                Dim dset As New DataSet
                If Get_ReportDataSource(reportObj, dset) Then
                    reportsource.SetDataSource(dset) ' error from vb6 !!!
                    If Set_ReportParameters(reportObj.ReportParameters, reportsource) Then
                        If Not reportObj.ReportFormula = String.Empty Then
                            reportsource.RecordSelectionFormula = reportObj.ReportFormula
                        End If
                        Dim rViewer As New ReportViewer()
                        rViewer.CRViewer.ViewerCore.ReportSource = reportsource
                        If reportObj.IsPreviewReport Then
                            rViewer.ShowDialog()
                        Else
                            Dim page As New Drawing.Printing.PageSettings
                            reportsource.PrintToPrinter(prter, page, False)
                        End If
                    End If
                End If
                dset.Dispose()
            End If
            prter = Nothing
            reportsource.Dispose()
        End Sub
        Public Sub Execute_ODBC(ByVal reportObj As _ReportObject)
            Dim reportsource As New ReportDocument
            Dim prter As New PrinterSettings
            Dim playout As New PrintLayoutSettings ' doesn't really work
            Dim originalDefaultPrinterName As String = _Printers.Get_DefaultPrinter()
            ''
            prter.Copies = Val(reportObj.NumberOfCopies)
            If IsValid_ReportPrinterName(reportObj.PrinterName) Then
                prter.PrinterName = reportObj.PrinterName
                reportsource.PrintOptions.PrinterName = reportObj.PrinterName
                _Debug.Print_(originalDefaultPrinterName, "->", prter.PrinterName)
                If Not originalDefaultPrinterName = prter.PrinterName Then
                    ' set as a default printer to re-set back to original one later.
                    _Printers.Set_DefaultPrinter(prter.PrinterName)
                    _Debug.Print_(prter.PrinterName, "IsDefaultPrinter: " & prter.IsDefaultPrinter)
                End If
            End If
            If IsReportSourceFound(reportObj.ReportName, reportsource, playout) Then
                If Not reportObj.ReportFormula = String.Empty Then
                    reportsource.RecordSelectionFormula = reportObj.ReportFormula
                End If
                If Not Set_ReportParameters(reportObj.ReportParameters, reportsource) Then
                    ' do something...
                End If
                If Not Set_SubReportFormulas(reportObj.SubReports, reportsource) Then
                    ' do something...
                End If
                If reportObj.IsPreviewReport Then
                    Dim rViewer As New ReportViewer()
                    rViewer.CRViewer.ViewerCore.ReportSource = reportsource
                    rViewer.ShowDialog()


                Else
                    Dim page As New Drawing.Printing.PageSettings
                    reportsource.PrintToPrinter(prter, page, False)
                End If
            End If

            If Not String.IsNullOrEmpty(reportObj.ReportSaveAsPath) AndAlso Not String.IsNullOrEmpty(reportObj.EmailAddress) Then
                reportsource.ExportToDisk(ExportFormatType.PortableDocFormat, reportObj.ReportSaveAsPath)
                Dim EmailPackages As New Collection
                If Create_EmailPackageObjects(reportObj, EmailPackages) Then
                    ' private now = gShipriteDb
                    'ShipRiteDb.path2db = reportObj.DatabasePath
                    Call _EmailSetup.Send_NotificationEmail(EmailPackages, _EmailSetup.file_YourReceiptAttached, reportObj.GlobalDBPath)
                End If
            End If
            '
            If Not originalDefaultPrinterName = prter.PrinterName Then
                ' set default back to original default printer.
                prter.PrinterName = originalDefaultPrinterName
                _Printers.Set_DefaultPrinter(prter.PrinterName)
                _Debug.Print_(prter.PrinterName, "IsDefaultPrinter: " & prter.IsDefaultPrinter)
            End If
            '
            'prter = Nothing
            'reportsource.Close()
        End Sub
        Public Sub Execute_ODBC_ToHTML(ByVal reportObj As _ReportObject)
            Dim reportsource As New ReportDocument
            Dim playout As New PrintLayoutSettings ' doesn't really work
            ''
            If IsReportSourceFound(reportObj.ReportName, reportsource, playout) Then
                If Not reportObj.ReportFormula = String.Empty Then
                    reportsource.RecordSelectionFormula = reportObj.ReportFormula
                End If
                If Not Set_ReportParameters(reportObj.ReportParameters, reportsource) Then
                    ' do something...
                End If
                If Not Set_SubReportFormulas(reportObj.SubReports, reportsource) Then
                    ' do something...
                End If
                reportsource.ExportToDisk(ExportFormatType.HTML40, reportObj.ReportSaveAsPath)
            End If
            '
            reportsource.Dispose()
        End Sub
        Public Sub Execute_ODBC_ToPDF(ByVal reportObj As _ReportObject)
            Dim reportsource As New CrystalDecisions.CrystalReports.Engine.ReportDocument
            Dim playout As New CrystalDecisions.Shared.PrintLayoutSettings ' doesn't really work
            ''
            If IsReportSourceFound(reportObj.ReportName, reportsource, playout) Then
                If Not reportObj.ReportFormula = String.Empty Then
                    reportsource.RecordSelectionFormula = reportObj.ReportFormula
                End If
                If Not Set_ReportParameters(reportObj.ReportParameters, reportsource) Then
                    ' do something...
                End If
                If Not Set_SubReportFormulas(reportObj.SubReports, reportsource) Then
                    ' do something...
                End If
                If Not String.IsNullOrEmpty(reportObj.ReportSaveAsPath) Then
                    reportsource.ExportToDisk(ExportFormatType.PortableDocFormat, reportObj.ReportSaveAsPath)
                    If Not String.IsNullOrEmpty(reportObj.EmailAddress) Then
                        Dim EmailPackages As New Collection
                        If Create_EmailPackageObjects(reportObj, EmailPackages) Then
                            ' private now = gShipriteDb
                            'ShipRiteDb.path2db = reportObj.DatabasePath
                            Call _EmailSetup.Send_NotificationEmail(EmailPackages, _EmailSetup.file_YourReceiptAttached, reportObj.GlobalDBPath)
                        End If
                    End If
                End If
                '
                reportsource.Dispose()
            End If
        End Sub

        Public Function Execute_LocalReport(ByVal reportObj As _ReportObject)
            Try
                If "ODBC" = reportObj.ReportDataSource Then
                    If reportObj.IsSaveAsOnly Then
                        _LocalReport.Execute_ODBC_ToPDF(reportObj)
                    Else
                        _LocalReport.Execute_ODBC(reportObj)
                    End If
                Else
                    _LocalReport.Execute_DataSet(reportObj)
                End If
                Return True
            Catch ex As Exception : _MsgBox.ErrorMessage(ex.Message, "Failed to Execute Local Report...", "ShipRiteReports")
                Return False
            End Try
        End Function

        Private Function Create_EmailPackageObjects(ByVal reportObj As _ReportObject, ByRef emailPackages As Collection) As Boolean
            Dim epack As New _EmailPackage
            epack.Carrier = String.Empty
            epack.TrackingNo = String.Empty
            epack.CustomerName = reportObj.EmailCustomerName
            epack.EmailTo = reportObj.EmailAddress
            emailPackages.Add(epack)
            Create_EmailPackageObjects = (0 < emailPackages.Count)
        End Function
    End Module

    Public Module ReportsODBC
        Public Sub ShipRiteReports_SetODBC()
            '
            Call reports_SetODBC(gDBpath, "Finance", ".mdb")
            Call reports_SetODBC(gDBpath, "Logging", ".mdb")
            Call reports_SetODBC(gDBpath, "PriceChart", ".mdb")
            Call reports_SetODBC(gDBpath, "ShipRite_MailboxPackages", ".mdb")
            Call reports_SetODBC(gDBpath, "ShipRite_DropOffPackages", ".mdb")
            Call reports_SetODBC(gDBpath, "ShipriteNext", ".accdb")
            '
        End Sub
        Private Function reports_SetODBC(ByVal dir2Db As String, ByVal sourceName As String, ByVal sourceExt As String) As Boolean
            Dim dsnFile As String = "c:\ShipriteNext\Reports\DataSources\" & sourceName & ".dsn"
            Dim path2Db As String = (dir2Db & "\" & sourceName & sourceExt) '".mdb")
            reports_SetODBC = False
            '
            ' if not found, then create
            If Not _Files.IsFileExist(dsnfile, False) Then
                reports_CreateODBC(dir2Db, path2Db, dsnFile)
                reports_SetODBC = True
            Else
                '
                If Not dir2Db = _Files.Read_IniValue(dsnFile, "ODBC", "DefaultDir") Then
                    Call _Files.Write_IniValue(dsnFile, "ODBC", "DefaultDir", dir2Db)
                End If
                '
                If Not path2Db = _Files.Read_IniValue(dsnFile, "ODBC", "DBQ") Then
                    Call _Files.Write_IniValue(dsnFile, "ODBC", "DBQ", path2Db)
                End If
                '
                reports_SetODBC = True
                '
            End If
        End Function
        Private Function reports_CreateODBC(ByVal dir2Db As String, ByVal path2Db As String, ByVal dsnFile As String) As Boolean
            Dim dsnKey As String = "ODBC"
            Call _Files.Write_IniValue(dsnFile, dsnKey, "DBQ", path2Db)
            Call _Files.Write_IniValue(dsnFile, dsnKey, "DefaultDir", dir2Db)
            Call _Files.Write_IniValue(dsnFile, dsnKey, "DriverId", "25")
            Call _Files.Write_IniValue(dsnFile, dsnKey, "FIL", "MS Access")
            Call _Files.Write_IniValue(dsnFile, dsnKey, "MaxBufferSize", "2048")
            Call _Files.Write_IniValue(dsnFile, dsnKey, "MaxScanRows", "8")
            Call _Files.Write_IniValue(dsnFile, dsnKey, "PageTimeout", "5")
            Call _Files.Write_IniValue(dsnFile, dsnKey, "SafeTransactions", "0")
            Call _Files.Write_IniValue(dsnFile, dsnKey, "Threads", "3")
            Call _Files.Write_IniValue(dsnFile, dsnKey, "UserCommitSync", "Yes")
            Call _Files.Write_IniValue(dsnFile, dsnKey, "UID", "admin")
            Call _Files.Write_IniValue(dsnFile, dsnKey, "DRIVER", "Microsoft Access Driver (*.mdb, *.accdb)")
            '
            Return True
        End Function
    End Module

    Public Module ReportsTest

        Private Sub TestReport()
            Try
                Dim rep As New _ReportObject
                rep.DatabasePath = "c:\shiprite\shiprite.mdb" '"c:\network\pn101\data\shiprite.mdb" '"C:\Users\Public\Documents\My Projects\ShipRite\ShipRite Customers\VW103\ShipRite.mdb"
                rep.PrinterName = "KONICA MINOLTA PS Color Laser Class Driver" '"KONICA MINOLTA C353 Series PS(P)" '"\\mvcc-print\IT154_HP4200" '"CITIZEN CT-S2000" '"PR-TB4" '
                rep.ReportDataSource = "ODBC"
                rep.ReportFormula = ""
                rep.ReportParameters.Add(11.22)
                rep.SubReports.Add("sub report formula", "sub report name")
                rep.NumberOfCopies = 1
                rep.IsPreviewReport = True

                rep.ReportName = "DepartmentChargeback.rpt" '"Invoice_T1_T2_T3.rpt" ' "Invoice.rpt" '"PostOffice.rpt" '"SalesTax.rpt" ' "Hourly.rpt" '"DrawerClose.rpt" '

                '' Main report Formula:
                rep.ReportFormula = ""
                rep.ReportFormula = "{AR.AcctNum} <> 'CASH'" '"SELECT * FROM AR WHERE AcctNum <> 'CASH' ORDER BY AcctName"
                'rep.ReportFormula = "{Transactions.InvNum}='33829'"
                'rep.ReportFormula = "{Transactions.SKU} <> 'NOTE' And {Transactions.Status} = 'Sold' And {Transactions.Date} >= Date (2015, 03, 06) and {Transactions.Date} <= Date (2015, 03, 06)" '"{Manifest.PACKAGEID}='OL14449'"
                'rep.ReportFormula = "{Transactions.Date} >= Date(2016,12,16) AND {Transactions.Date} <= Date(2016,12,16) AND {Transactions.Qty} <> 0 AND {Transactions.ExtPrice} <> 0 AND {Transactions.Status}='Sold'"
                'rep.ReportFormula = "{Transactions.InvNum}='16612'"

                '' Subreports:
                '' rep.SubReports.Add("sub report formula", "sub report name")
                'rep.SubReports.Add("{Payments.Date} >= Date(2013,06,23) AND {Payments.Date} <= Date(2013,06,27) AND {Payments.Status}='Ok' AND {Payments.Type}='Check'", "Checks")
                'rep.SubReports.Add("{Payments.Date} >= Date(2013,06,23) AND {Payments.Date} <= Date(2013,06,27) AND {Payments.Status}='Ok' AND {Payments.Type}='Charge'", "Charges")
                'rep.SubReports.Add("{Transactions.Date} >= Date(2013,06,23) AND {Transactions.Date} <= Date(2013,06,27) AND {Transactions.Status}='Sold' AND {Transactions.SKU}='POA'", "POA")
                'rep.SubReports.Add("", "ROA")
                'rep.SubReports.Add("{Payments.Date} >= Date(2013,06,23) AND {Payments.Date} <= Date(2013,06,27) AND {Payments.Status}='Ok' AND {Payments.OtherText} <> 'Sales Refunds' AND {Payments.Payment} > 0.00", "Refunds")

                '' Parameters:
                'rep.ReportParameters.Add(11.22) ' for MBXNotice.rpt

                '' DataSet only:
                'rep.DatabaseTables.Add("Contacts")
                'rep.DatabaseTables.Add("Select * From MailBox Where MailboxNumber=113")
                'rep.DatabaseTables.Add("Setup")
                'rep.DatabaseTables.Add("Select * From MBXNamesList Where MBX=113")

                If rep.ReportName = "Invoice.rpt" Or rep.ReportName = "Invoice_T1_T2_T3.rpt" Then
                    rep.IsSaveAsOnly = True ' for Invoice.rpt it will overwrite rep.IsPreviewReport value
                    ''AP(06/18/2018) - Updated file name of emailed invoice attachment from "Receipt.pdf" to "Invoice.pdf".
                    rep.ReportSaveAsPath = "c:\network\pn101\data\Receipts\Invoice.pdf" '"C:\ShipRite\Receipts\Invoice.pdf"
                    rep.EmailAddress = "" '"oleg@shipritesoftware.com"
                    rep.GlobalDBPath = "c:\network\pn101\data" '"C:\ShipRite"
                    rep.DatabasePath = "c:\network\pn101\data\shiprite.mdb" '"C:\ShipRite\Shiprite.mdb"
                End If
                _LocalReport.Execute_LocalReport(rep)

            Catch ex As Exception : MsgBox(ex.Message)

            End Try
        End Sub
    End Module

End Namespace