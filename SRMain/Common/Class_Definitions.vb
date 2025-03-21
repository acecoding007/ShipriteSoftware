Imports SHIPRITE.ShipRiteReports

Public Class Shipment
    Public Property ShipFrom As New Contact
    Public Property ShipTo As New Contact
End Class


Public Class Contact
    Public Property ContactID As Long
    Public Property CompanyName As String
    Public Property FirstName As String
    Public Property LastName As String
    Public Property Address1 As String
    Public Property Address2 As String
    Public Property City As String
    Public Property State As String
    Public Property Zip As String
    Public Property Province As String
    Public Property Country As String
    Public Property CountryCode As String
    Public Property Phone As String
    Public Property Fax As String
    Public Property Email As String
    Public Property IsResidential As Boolean
    Public Property AR_AccountNumber As String
    Public Property CreatedOn As Date
    Public Property UniqueID As String
    Public Property IsConsignee As Boolean
    Public Property CellPhone As String
    Public Property CellDomain As String
    Public Property CellCarrier As String
    Public ReadOnly Property FNameLName() As String
        Get
            Dim tmpName As String = String.Format("{0} {1}", FirstName, LastName)
            Return tmpName.Trim
        End Get
    End Property
    Public ReadOnly Property LNameFName() As String
        Get
            Dim tmpName As String = String.Format("{0}, {1}", LastName, FirstName)
            Return tmpName.Trim
        End Get
    End Property
    Public ReadOnly Property Name() As String
        Get
            If Not 0 = CompanyName.Length Then
                Return CompanyName
            Else
                Return FNameLName
            End If
        End Get
    End Property
    Public ReadOnly Property Address() As String
        Get
            ' Returns formatted address in 2 or 3 lines
            Dim tmp As String = Me.Address1
            If Me.Address2 IsNot Nothing AndAlso Not 0 = Me.Address2.Length Then
                tmp = String.Format("{0}{1}{2}", Me.Address1, System.Environment.NewLine, Me.Address2)
            End If
            Return String.Format("{0}{1}{2}, {3} {4}", tmp, System.Environment.NewLine, Me.City, Me.State, Me.Zip)
        End Get
    End Property
End Class

Public Class Mbx_Panel

    Public Property Description As String
    Public Property Starting_No As Double
    Public Property Ending_No As Double

    Public Property MBX_Pricing As List(Of Double)
    Public Property DisplayColor As System.Windows.Media.SolidColorBrush
    Public Property DisplayTextColor As System.Windows.Media.SolidColorBrush

End Class

Public Enum MailboxNoticeType
    NONE
    Renewal
    Expiration
    Cancellation
End Enum

Public Class Mailbox
    Public Property Number As Integer
    Public Property Panel As String
    Public Property ContactID As Integer
    Public Property Name As String
    Public Property ExpirationDate As Date
    Public Property BusinessType As Integer
    Public Property CustomRates As String
    Public ReadOnly Property IsCustomRate As Boolean
        Get
            Return BusinessType = 2
        End Get
    End Property
    Public Property DisplayColor As System.Windows.Media.SolidColorBrush
    Public Property DisplayTextColor As System.Windows.Media.SolidColorBrush

    Public Sub New()
        'properties set to defaults
    End Sub

    Public Sub New(mbxNumber As Integer)
        'load other info
        Dim sql As String = "SELECT MailboxNumber, Name, EndDate, Business, CustomRates FROM Mailbox WHERE MailboxNumber = " & mbxNumber.ToString()
        Dim segment As String = GetNextSegmentFromSet(IO_GetSegmentSet(gShipriteDB, sql))
        Load_FromSegment(segment)
    End Sub

    Public Sub New(segment As String)
        Load_FromSegment(segment)
    End Sub

    Private Sub Load_FromSegment(segment As String)
        Name = ExtractElementFromSegment("Name", segment, "")
        Number = ExtractElementFromSegment("MailboxNumber", segment, "0")
        ContactID = ExtractElementFromSegment("CID", segment, "0")
        ExpirationDate = ExtractElementFromSegment("EndDate", segment)
        BusinessType = ExtractElementFromSegment("Business", segment, "0")
        CustomRates = ExtractElementFromSegment("CustomRates", segment, "")
    End Sub

    Public Shared Function IsDept_Taxable() As Boolean
        Dim buf = IO_GetSegmentSet(gShipriteDB, "SELECT Taxable From Departments WHERE Department='MAILBOX'")
        Return ExtractElementFromSegment("Taxable", buf, "False")
    End Function
End Class

Public Class MailboxNotice

    Private Const ReportName As String = "MBXNotice.rpt"

    Private Shared Function Get_MailboxNoticeType_Desc(type As MailboxNoticeType) As String
        Select Case type
            Case MailboxNoticeType.Renewal
                Return "RENEWAL NOTICE"
            Case MailboxNoticeType.Expiration
                Return "EXPIRATION NOTICE"
            Case MailboxNoticeType.Cancellation
                Return "CANCELLATION NOTICE"
            Case Else
                Return ""
        End Select
    End Function
    Private Shared Function Get_MailboxNoticeType_DbField(type As MailboxNoticeType) As String
        Select Case type
            Case MailboxNoticeType.Renewal
                Return "MBXRenewalMsg"
            Case MailboxNoticeType.Expiration
                Return "MBXExpireMsg"
            Case MailboxNoticeType.Cancellation
                Return "MBXCancelMsg"
            Case Else
                Return ""
        End Select
    End Function
    Private Shared Function Get_MailboxNoticeType_FileName(type As MailboxNoticeType) As String
        Select Case type
            Case MailboxNoticeType.Renewal
                Return "mbx_renewal_statement"
            Case MailboxNoticeType.Expiration
                Return "mbx_expiration_statement"
            Case MailboxNoticeType.Cancellation
                Return "mbx_cancellation_statement"
            Case Else
                Return "mbx_statement"
        End Select
    End Function

    Private Shared Function Generate_Report(ByRef report As _ReportObject, mbx As Mailbox, type As MailboxNoticeType) As Boolean
        Return Generate_Report(report, New List(Of Mailbox) From {mbx}, type)
    End Function

    Private Shared Function Generate_Report(ByRef report As _ReportObject, mbxList As List(Of Mailbox), type As MailboxNoticeType) As Boolean
        Try

            If report Is Nothing Then
                Throw New Exception("Report object is null.")
            ElseIf mbxList.Count = 0 Then
                Throw New Exception("Mailbox Number(s) missing.")
            ElseIf type = MailboxNoticeType.NONE Then
                Throw New Exception("Mailbox Notice Type not set.")
            End If

            Dim crFormula As String = ""
            Dim mbx As New Mailbox
            If mbxList.Count = 1 Then
                mbx = mbxList(0)
            End If

            For Each mbxItem In mbxList
                If crFormula <> "" Then
                    crFormula &= " OR "
                End If
                crFormula &= "{Mailbox.MailboxNumber} = " & mbxItem.Number
            Next
            report.ReportFormula = crFormula

            If Mailbox.IsDept_Taxable() Then
                report.ReportParameters.Add(ExtractElementFromSegment("TaxRate", gPOSDefaultTaxSegment, "0"))
            Else
                report.ReportParameters.Add(0)
            End If

            report.ReportParameters.Add(Get_MailboxNoticeType_Desc(type))
            report.ReportParameters.Add(GetPolicyData(gShipriteDB, Get_MailboxNoticeType_DbField(type), ""))

            'Check if mailbox has custom Pricing.
            'notice with custom pricing has to be printed individually and cannot be part of larger mailbox group.
            Dim crPriceList As New List(Of String)

            If mbx.Number <> 0 AndAlso mbx.IsCustomRate Then
                Dim cRates As String = mbx.CustomRates

                If cRates <> "" Then
                    crPriceList = cRates.Split(" ").ToList
                End If
            End If

            If crPriceList.Count = 4 Then
                report.ReportParameters.Add(crPriceList(0)) '1 month
                report.ReportParameters.Add(crPriceList(1)) '3 months
                report.ReportParameters.Add(crPriceList(2)) '6 months
                report.ReportParameters.Add(crPriceList(3)) '12 months
            Else
                report.ReportParameters.Add(0) '1 month
                report.ReportParameters.Add(0) '3 months
                report.ReportParameters.Add(0) '6 months
                report.ReportParameters.Add(0) '12 months
            End If

            Return True
        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Failed to generate Mailbox Notice report...")
        End Try
        Return False
    End Function

    Public Shared Sub Print_Notices(mbxList As List(Of Mailbox), type As MailboxNoticeType, Optional isPrintSeparately As Boolean = False)
        Dim report As New _ReportObject() With {
            .ReportName = ReportName
        }

        If isPrintSeparately Then
            ' do individually
            For Each item As Mailbox In mbxList
                Print_Notice(item, type)
            Next
        Else
            If Generate_Report(report, mbxList, type) Then
                Dim reportPrev As New ReportPreview(report)
                reportPrev.ShowDialog()
            End If
        End If
    End Sub

    Public Shared Sub Print_Notice(mbx As Mailbox, type As MailboxNoticeType)
        Print_Notices(New List(Of Mailbox) From {mbx}, type)
    End Sub
    Public Shared Sub Print_Notice(mbxNum As Integer, type As MailboxNoticeType)
        Dim mbx As New Mailbox(mbxNum)
        Print_Notice(mbx, type)
    End Sub

    Public Shared Sub Email_Notices(mbxList As List(Of Mailbox), type As MailboxNoticeType)
        For Each mbxNotice In mbxList
            Email_Notice(mbxNotice, type)
        Next
    End Sub

    Public Shared Function Email_Notice(mbx As Mailbox, type As MailboxNoticeType) As Boolean
        gResult = ""
        Dim email As String = GetEmail_ForMBXNotice(mbx.ContactID)

        If email = "" Then
            gResult = "NoEmail"
            Return False
        End If


        Dim report As New _ReportObject() With {
            .ReportName = ReportName
        }

        If Generate_Report(report, mbx, type) Then

            Dim mbxNoticeFileName As String = Get_MailboxNoticeType_FileName(type) & "-" & mbx.Number.ToString & ".pdf"
            Dim mbxNoticeDirPath As String = gAppPath & "\MBX_Statements"
            If _Files.IsFolderExist_CreateIfNot(mbxNoticeDirPath, True) Then

                _Files.Delete_FilesFromFolder(mbxNoticeDirPath, False)

                ' file path = ... \MBX_Statements\statementtype-mbxnum.pdf
                Dim mbxNoticeFilePath As String = mbxNoticeDirPath & "\" & mbxNoticeFileName

                ' save to pdf
                report.ReportSaveAsPath = mbxNoticeFilePath
                ' don't use report email fields - specifically only for attached receipt currently and won't return status
                Execute_ODBC_ToPDF(report)

                ' if file path exists, then was saved as pdf successfully
                If _Files.IsFileExist(mbxNoticeFilePath, True) Then
                    ' email addr and email customer name
                    ' send and receive response
                    Dim customer_name As String = mbx.Name
                    Dim SQL As String

                    Dim template_Email As EmailTemplate = getEmailTemplate("Notify_Email-MailboxStatement", customer_name)
                    template_Email.Content = template_Email.Content.Replace("%MailboxNoticeType%", "Mailbox " & type.ToString)

                    Dim statement_pdf As New System.Net.Mail.Attachment(mbxNoticeFilePath)

                    Dim success As Boolean
                    success = sendEmailWithAttachment(email, template_Email.Subject, template_Email.Content, statement_pdf)

                    If success Then
                        If type = MailboxNoticeType.Expiration Then
                            SQL = "UPDATE Mailbox SET [ExpiredSent] = True WHERE MailboxNumber = " & mbx.Number
                        ElseIf type = MailboxNoticeType.Cancellation Then
                            SQL = "UPDATE Mailbox SET [CanceledSent] = True WHERE MailboxNumber = " & mbx.Number
                        ElseIf type = MailboxNoticeType.Renewal Then
                            SQL = "UPDATE Mailbox SET [RenewalSent] = True WHERE MailboxNumber = " & mbx.Number
                        End If
                        IO_UpdateSQLProcessor(gShipriteDB, SQL)

                        Return True
                    End If

                End If
            End If
        End If

        Return False
    End Function

    Public Shared Function GetEmail_ForMBXNotice(CID As String) As String
        Dim Segment As String
        Dim SQL As String

        SQL = "SELECT Email FROM Contacts WHERE ID = " & CID
        Segment = IO_GetSegmentSet(gShipriteDB, SQL)

        Return ExtractElementFromSegment("Email", Segment, "")

    End Function

End Class



Public Class User
    Public Property DatabaseID As String
    Public Property DisplayName As String
    Public Property PassCode As String

    Public Property FirstName As String
    Public Property LastName As String
    Public Property Address1 As String
    Public Property Address2 As String
    Public Property City As String
    Public Property State As String
    Public Property Zip As String
    Public Property Phone As String
    Public Property Email As String
    Public Property FingerPrint As String

    Public Property Permission_List As List(Of User_Permission)
End Class

Public Class User_Permission
    Public Property DB_Field As String
    Public Property isAllowed As Boolean
End Class


'' Usage Example
' Do While dreader.Read
'   Dim lst As ListItemWithID = New ListItemWithID()
'   lst.ItemID = _Convert.Null2DefaultValue(dreader("ID"))
'   lst.ItemText = _Convert.Null2DefaultValue(dreader("Country"))
'   lst.ItemIndex = Me.cmbCountry.Items.Count ' optional to know exact list index
'   Me.cmbCountry.Items.Add(lst)
' Loop

Public Class ListItemWithID

    Public ItemText As String
    Public ItemID As Long
    Public ItemIndex As Integer

    Public Overrides Function ToString() As String
        Return ItemText
    End Function

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub
End Class

Public Class _ListItemWithObject

    Public ItemText As String
    Public ItemID As Long
    Public ItemIndex As Integer
    Public ItemObject As Object

    Public Overrides Function ToString() As String
        Return ItemText
    End Function

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub

End Class

Public Class InventoryItem
    Public Property SKU As String
    Public Property Desc As String
    Public Property Cost As Double
    Public Property Sell As Double
    Public Property MSRP As Double
    Public Property Quantity As Double
    Public Property WarningQty As Double
    Public Property Department As String
    Public Property Active As Boolean
    Public Property Zero As Boolean

    'PackMaster items
    Public Property PackagingMaterials As Boolean
    Public Property MaterialsClass As String
    Public Property Weight As String
    Public Property L As Double
    Public Property W As Double
    Public Property H As Double

    Public Property Department_List As List(Of String)
    Public Property PackMaterials_Class_List As List(Of String)

    Public Property Status As String
    Public Property Delete As Boolean = False
    Public Property OriginalSKU As String 'placeholder in case user changes the SKU.
    '
End Class

Public Class Contact_listing
    Public Property ID As String
    Public Property Name As String
    Public Property Email As String
    Public Property SMS As String
    Public Property Address As String
    Public Property State As String
    Public Property City As String

    Public Property Zip As String

    Public Property SalesVolume As String

    Public Property ShippingVolume As String
    Public Property PackageCount As String

End Class

Public Class PackagingData

    Public Property CID As String
    Public Property Contents As String
    Public Property Description As String
    Public Property Weight As Decimal
    Public Property PackagingCost As Decimal
    Public Property PackagingCharge As Decimal
    Public Property L As Decimal
    Public Property W As Decimal
    Public Property H As Decimal
End Class


