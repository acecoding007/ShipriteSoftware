Imports System.Data

#Region "Store Owner"
Public Module _StoreOwner
    Public StoreOwner As _baseContact

    Public Function Load_StoreOwnerContact() As _baseContact
        Load_StoreOwnerContact = New _baseContact
        '
        With Load_StoreOwnerContact
            .ContactID = Val(General.GetPolicyData(gShipriteDB, "DefaultShipFrom")) ' get value of string to set to Long variable
            .CompanyName = General.GetPolicyData(gShipriteDB, "Name")
            .FName = General.GetPolicyData(gShipriteDB, "FName")
            .LName = General.GetPolicyData(gShipriteDB, "LName")
            .Addr1 = General.GetPolicyData(gShipriteDB, "Addr1")
            .Addr2 = General.GetPolicyData(gShipriteDB, "Addr2")
            .City = General.GetPolicyData(gShipriteDB, "City")
            .State = General.GetPolicyData(gShipriteDB, "State")
            .Zip = General.GetPolicyData(gShipriteDB, "Zip")
            .Province = String.Empty
            .Country = General.GetPolicyData(gShipriteDB, "Country") ' don't have country name is Setup
            If String.IsNullOrWhiteSpace(.Country) Then
                If .State = "PR" Then
                    .Country = "Puerto Rico"
                Else
                    .Country = "United States"
                End If
            End If
            .CountryCode = _Contact.Get_CountryCodeFromCountryName(.Country)
            .Tel = General.GetPolicyData(gShipriteDB, "Phone1")
            .Fax = General.GetPolicyData(gShipriteDB, "Phone2")
            .Email = General.GetPolicyData(gShipriteDB, "Email")
            .Residential = False
            '.AccountNumber = General.GetPolicyData(gShipriteDB, "")
            '.CellPhone = General.GetPolicyData(gShipriteDB, "")
            '.CellDomain = General.GetPolicyData(gShipriteDB, "")
            '.CellCarrier = General.GetPolicyData(gShipriteDB, "")
        End With
        '
    End Function

End Module

#End Region

Public Module _SignatureType
    '   0 = No Signature Specified
    '   1 = Delivery confirmation
    '   2 = Delivery with Indirect Signature (or Delivery confirmation with Signature)
    '   3 = Delivery with Direct Signature
    '   4 = Delivery with Adult Signature
    Public Const No_Signature_Required As Integer = 0
    Public Const Delivery_Confirmation As Integer = 1
    Public Const Indirect_Signature As Integer = 2
    Public Const Direct_Signature As Integer = 3
    Public Const Adult_Signature As Integer = 4
End Module
Public Module _Contact
    Public Property ShipperContact As New _baseContact
    Public Property ShipFromContact As New _baseContact
    Public Property ShipToContact As New _baseContact
    Public Property HoldAtContact As New _baseContact

    Public Function Load_ContactFromDb(ByRef ContactID As Long, ByRef objContact As _baseContact) As Boolean
        ''
        Load_ContactFromDb = False ' assume.
        ''
        Dim sql2exe As String = String.Empty
        Dim SegmentSet As String = String.Empty
        ''
        If IsNothing(objContact) Then objContact = New _baseContact

        Try
            ''
            sql2exe = "SELECT [ID], [FName], [LName], [Name], [Addr1], [Addr2], [Addr3], [City], [State], [Zip], [Phone], [Fax], [email], [Country], [Residential] FROM [Contacts] WHERE [ID] = " & CStr(ContactID)
            SegmentSet = DatabaseFunctions.IO_GetSegmentSet(gShipriteDB, sql2exe)
            If Not String.IsNullOrEmpty(SegmentSet) Then
                '
                objContact.ContactID = Val(ExtractElementFromSegment("ID", SegmentSet))
                '' PostalMate uses Chr(160) instead of Chr(32) for space character.
                objContact.FName = Replace(ExtractElementFromSegment("FName", SegmentSet), Chr(160), Chr(32))
                objContact.LName = Replace(ExtractElementFromSegment("LName", SegmentSet), Chr(160), Chr(32))
                objContact.CompanyName = Replace(ExtractElementFromSegment("Name", SegmentSet), Chr(160), Chr(32))
                objContact.Addr1 = Replace(ExtractElementFromSegment("Addr1", SegmentSet), Chr(160), Chr(32))
                objContact.Addr2 = Replace(ExtractElementFromSegment("Addr2", SegmentSet), Chr(160), Chr(32))
                objContact.Addr3 = Replace(ExtractElementFromSegment("Addr3", SegmentSet), Chr(160), Chr(32))
                objContact.City = Replace(ExtractElementFromSegment("City", SegmentSet), Chr(160), Chr(32))
                objContact.State = Replace(ExtractElementFromSegment("State", SegmentSet), Chr(160), Chr(32))
                objContact.Zip = Replace(ExtractElementFromSegment("Zip", SegmentSet), Chr(160), Chr(32))
                objContact.Tel = Replace(ExtractElementFromSegment("Phone", SegmentSet), Chr(160), Chr(32))
                objContact.Fax = Replace(ExtractElementFromSegment("Fax", SegmentSet), Chr(160), Chr(32))
                objContact.Email = Replace(ExtractElementFromSegment("email", SegmentSet), Chr(160), Chr(32))
                objContact.Residential = ExtractElementFromSegment("Residential", SegmentSet, "False")
                objContact.Province = ""
                objContact.Country = Replace(ExtractElementFromSegment("Country", SegmentSet), Chr(160), Chr(32)) ''ol#9.230(7/18).
                objContact.CountryCode = _Contact.Get_CountryCodeFromCountryName(objContact.Country)
                'objContact.AccountNumber
                '
                Return True
            End If
            ''
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to read Concact info from Contacts database table...")
        End Try
    End Function

    Public Function Update_PhoneNo(ByVal CID As Long, ByVal Phone As String)
        Dim SQL As String
        Dim FormatedPhone As String = ReformatPhone(gShipriteDB, Phone)
        If FormatedPhone <> "" Then
            Phone = FormatedPhone
        End If


        SQL = "Update Contacts set Phone=" & Phone & " WHERE ID=" & CID
        IO_UpdateSQLProcessor(gShipriteDB, SQL)
    End Function


    Public Function Get_CountryCodeFromCountryName(ByVal CountryName As String) As String
        ''
        Get_CountryCodeFromCountryName = String.Empty ' assume.
        Dim SegmentSet As String = String.Empty
        Dim sql2exe As String
        ''
        Try
            ''
            If 0 = Len(CountryName) Or "united states" = LCase(CountryName) Then
                ''
                Return "US"
                ''
            Else
                ''
                sql2exe = "SELECT [Country_Cd] FROM [Country] WHERE [Country_Name] = '" & Replace(CountryName, "'", "''") & "'"
                SegmentSet = DatabaseFunctions.IO_GetSegmentSet(gCountryDB, sql2exe)
                If Not String.IsNullOrEmpty(SegmentSet) Then
                    Return ExtractElementFromSegment("Country_Cd", SegmentSet)
                End If
                ''
            End If
            ''
        Catch ex As System.Exception : _MsgBox.ErrorMessage(ex, "Failed to read Country Code from Country database table...")
        End Try
    End Function
    Public Function ChangeShipFromAs_co_StoreAddress(ByVal isNameThenCompany As Boolean) As _baseContact
        Dim obj As New _baseContact

        '' In stead of reading shipper contact from db every time - just copy it from shipper object.
        If _Contact.Copy_ContactFromObject(_Contact.ShipperContact, obj) Then
            '
            If isNameThenCompany Then
                '
                If _Contact.ShipFromContact.CompanyName.Length = 0 Then
                    obj.FName = _Contact.ShipFromContact.FName
                    obj.LName = _Contact.ShipFromContact.LName
                Else
                    obj.FName = ""
                    obj.LName = _Contact.ShipFromContact.CompanyName
                End If
                obj.CompanyName = "c/o " & _Contact.ShipperContact.CompanyName
                '
            Else
                '
                If _Contact.ShipFromContact.CompanyName.Length = 0 Then
                    obj.CompanyName = _Contact.ShipFromContact.FNameLName
                Else
                    obj.CompanyName = _Contact.ShipFromContact.CompanyName
                End If
                obj.FName = "c/o"
                obj.LName = _Contact.ShipperContact.CompanyName
                '
            End If
            '
            obj.Email = _Contact.ShipFromContact.Email 'Leave customer original email in Return Address block
            '' ShipFrom contact is required to have a phone number, if missing then Shipper one will be assigned.
            If Not IsNothing(obj.Tel.Length) AndAlso obj.Tel.Length = 0 Then
                obj.Tel = _Contact.ShipperContact.Tel
            End If
            ChangeShipFromAs_co_StoreAddress = obj
        Else
            ChangeShipFromAs_co_StoreAddress = _Contact.ShipFromContact ' in case of error.
        End If
    End Function
    Public Function Copy_ContactFromObject(ByVal copyfrom As _baseContact, ByRef copyto As _baseContact) As Boolean
        If copyfrom Is Nothing Then
            copyfrom = New _baseContact
        End If
        With copyfrom
            copyto.ContactID = .ContactID
            copyto.FName = .FName
            copyto.LName = .LName
            copyto.CompanyName = .CompanyName
            copyto.Addr1 = .Addr1
            copyto.Addr2 = .Addr2
            copyto.City = .City
            copyto.State = .State
            copyto.Zip = .Zip
            copyto.Tel = .Tel
            copyto.Fax = .Fax
            copyto.Email = .Email
            copyto.Residential = .Residential
            copyto.Province = .Province
            copyto.Country = .Country
            copyto.CountryCode = .CountryCode
            copyto.AccountNumber = .AccountNumber
        End With
        Return True
    End Function
End Module

Public Module _IDs

    Public IsMetricSystem As Boolean   ''ol#9.141(1/3)... Metric or Imperial switch was added to have KG or LB and CM or IN.
    Public CurrencyType As String   ''ol#9.151(2/5)... CurrencyType variable was added to manipulate between CAD and USD.
    Public Function IsIt_AlaskaShipper() As Boolean
        Return ("US" = _StoreOwner.StoreOwner.CountryCode And _StoreOwner.StoreOwner.State = "AK")
    End Function
    Public Function IsIt_HawaiiShipper() As Boolean
        Return ("US" = _StoreOwner.StoreOwner.CountryCode And _StoreOwner.StoreOwner.State = "HI")
    End Function
    Public Function IsIt_PuertoRicoShipper() As Boolean
        'Return ("PR" = _StoreOwner.StoreOwner.CountryCode)
        Return ("US" = _StoreOwner.StoreOwner.CountryCode And _StoreOwner.StoreOwner.State = "PR") Or "PR" = _StoreOwner.StoreOwner.CountryCode
    End Function
    Public Function IsIt_VirginIslandShipper() As Boolean
        Return ("VI" = _StoreOwner.StoreOwner.CountryCode)
    End Function
    Public Function IsIt_GuamShipper() As Boolean
        Return ("GU" = _StoreOwner.StoreOwner.CountryCode)
    End Function
    Public Function IsIt_USAShipper() As Boolean
        Return ("US" = _StoreOwner.StoreOwner.CountryCode)
    End Function
    Public Function IsIt_CanadaShipper() As Boolean
        Return ("CA" = _StoreOwner.StoreOwner.CountryCode) Or ("Canada" = _StoreOwner.StoreOwner.Country)
    End Function
    Public Function IsIt_PostNetStore() As Boolean
        Return (_StoreOwner.StoreOwner.Name.Contains("PostNet"))
    End Function
    Public Function IsIt_FedEx_FASC() As Boolean
        Return CBool(GetPolicyData(gShipriteDB, "Enable_RetailFedExLevel", "False"))
    End Function

    Public Function IsIt_UPS_ASO() As Boolean
        Return CBool(GetPolicyData(gShipriteDB, "Enable_RetailUPSLevel", "False"))
    End Function

    Public Function IsIt_USPS_ApprovedShipper() As Boolean
        Return CBool(GetPolicyData(gShipriteDB, "Enable_USPS_ApprovedShipper", "False"))
    End Function

    Public Function IsIt_USPS_SRPRO_Rate() As Boolean
        Return CBool(GetPolicyData(gShipriteDB, "ENABLE_USPS_SRPRO_Rate", "False"))
    End Function

End Module

Public Module _ReusedField
    '' ShipRite.mdb
    ''
    '' Reused fields in Setup table:
    Public Const fldUPSWeb_UserID As String = "Panel0LMessage"
    Public Const fldUPSWeb_UserPassword As String = "Panel1LMessage"
    Public Const fldUPSWeb_UPSAccount As String = "Panel2LMessage"
    Public Const fldUPSWeb_UserLicense As String = "BankBookAccounts" ''ol#9.174(8/22)... Switching UPS Ready from Test to Production server.
    ''
    Public Const fldDryIce_Cost As String = "LabPackCost" ''ol#8.12(1/13)... FedEx DryIce charge was added.
    Public Const fldDryIce_Charge As String = "LabPackCharge" ''ol#8.12(1/13)... FedEx DryIce charge was added.
    ''
    Public Const fldOVSZone2_Cost As String = "ABSATRED"        '' Zone2 oversize cost
    Public Const fldOVSZone2_Charge As String = "ABSATPURED"    '' Zone2 oversize charge
    Public Const fldOVSZone3_Cost As String = "ABSATBLACK"      '' Zone3 oversize cost
    Public Const fldOVSZone3_Charge As String = "ABSATPUBLACK"  '' Zone3 oversize charge
    Public Const fldOVSZone4_Cost As String = "ABResCost"       '' Zone4 oversize cost
    Public Const fldOVSZone4_Charge As String = "ABResCharge"   '' Zone4 oversize charge
    Public Const fldOVSZone5_Cost As String = "ABBlackResCost"      '' Zone5 oversize cost
    Public Const fldOVSZone5_Charge As String = "ABBlackResCharge"  '' Zone5 oversize charge
    ''
    Public Const fldIsMarkups_UPS_OnDiscCost As String = "IsUPSMarkupDiscount" ''ol#9.201(2/20)... Store Owner will have an option to choose the markups to be based on the shipper discounted cost or retail price for UPS.
    Public Const fldIsMarkups_FedEx_OnDiscCost As String = "IsFedExMarkupDiscount" ''ol#9.201(2/18)... Store Owner will have an option to choose the markups to be based on the shipper discounted cost or retail price for FedEx.
    ''
    ''ol#16.05(2/5)... Endicia account info moved from ShipriteSetup_Integration.mdb to ShipRite.mdb (Setup table).
    Public Const fldPickUpLocation As String = "ABButton1"
    Public Const fldPickUpInstructions As String = "ABButton2"
    Public Const fldAccountID As String = "ABButton3" ' reused Setup table fields
    Public Const fldPassPhrase As String = "ABButton4"
    Public Const fldRequesterID As String = "ABButton5" ''ol#16.05(2/5).
    ''
    ''ol#16.05(2/12)... DHL-I account info moved from DHL-I.cfg to ShipRite.mdb (Setup table).
    Public Const fldDHL_SiteID As String = "ABRemoteID"
    Public Const fldDHL_SitePassword As String = "ABPassword"
    Public Const fldDHL_ShipperID As String = "ABOrigin"
    Public Const fldDHL_ShipperAccountNumber As String = "ABTPID3"
    Public Const fldDHL_BillingAccountNumber As String = "ABTPID4"
    Public Const fldDHL_DutyAccountNumber As String = "ABTPID5" ''ol#16.05(2/12).
    ''
    Public Const fldRegCountryCode As String = "Labels" ''ol#16.06(3/29)... 'Labels' setup field is re-used for 'CountryCode' field.

    ' Pack Master:
    Public Const fldLabor_BuildUp As String = "ABHOMEINVLO"
    Public Const fldLabor_CutDown As String = "ABHOMEINVHI"
    Public Const fldLabor_Telescope As String = "ABTPID7"
    Public Const fldLabor_AddTop As String = "ABHOMEMMINVLO"


    '' Reports.mdb
    ''
    '' Setup table:
    Public Const fldReportPrinter As String = "ReportPrinter"
    ''ol#9.187(10/18)... 'LabelPrinter2' printer field in Reports.mdb will be used for Receipt Slips reports now.
    ''ol#9.187(10/18)Public Const fldLabelPrinter2   As String = "LabelPrinter2"
    Public Const fldReceiptSlipPrinter As String = "LabelPrinter2" ''ol#9.187(10/18).
    Public Const fldInvoicePrinter As String = "InvoicePrinter"
    Public Const fldFedExPrinter As String = "FedExPrinter"
    Public Const fldFedExTPrinter As String = "FedExTPrinter"
    Public Const fldDHLLabelPrinter As String = "ABLabelPrinter"
    Public Const fldGenericLabelPrinter As String = "LabelPrinter"
    ''
    Public Const fldInvoiceDrawerCode As String = "InvoiceDrawer"
    Public Const fldInvoiceFontName As String = "InvoiceFont"
    Public Const fldInvoiceFontSize As String = "FontSize"
    Public Const fldInvoiceSkipLines As String = "SkipLinesReceipt"
    ''
    '' Reused fields in Manifest table:
    ''ol#9.122(1/4)... 'CALLTAG1' Manifest table field is reused for new 'Clearance Entry Fee' for Canada Ground shipments.
    Public Const fldClearanceEntryFeeCost As String = "costCALLTAG1"
    Public Const fldClearanceEntryFeeCharge As String = "CALLTAG1"
End Module

Public Class PackagingCharges_Class

    Public SKU As String
    Public Desc As String
    Public Qty As Double
    Public SlidePosition As Long
    Public UnitCost As Double
    Public ExtCost As Double
    Public UnitPrice As Double
    Public ExtPrice As Double
    Public Weight As Double
    Public Summary As Boolean
    Public Dept As String
    Public IsDisplayed As Boolean ''ol#9.55(9/23)... We need an indicator for PackMasterII items to indicate if the item was added to the Preview list.
    Public Qty_SetbyUser As Double ''ol#9.123(1/23)... Reviewer 'Qty' column values are allowed to be a adjusted resulting in recalculation of 'Ext Price'.
    Public Index As Integer

End Class

Public Module PackMasterII

    Public Const iINNER As Integer = 1
    Public Const iOUTER As Integer = 0
    Public Const iWRAP As Integer = 2
    Public Const i_FILL As Integer = 3
    Public Const iLABOR As Integer = 4
    Public Const iOTHER As Integer = 5
    ''
    Public Structure PackagingCharges
        Public SKU As String
        Public Desc As String
        Public Qty As Double
        Public SlidePosition As Long
        Public UnitCost As Double
        Public ExtCost As Double
        Public UnitPrice As Double
        Public ExtPrice As Double
        Public Weight As Double
        Public Summary As Boolean
        Public Dept As String
        Public IsDisplayed As Boolean ''ol#9.55(9/23)... We need an indicator for PackMasterII items to indicate if the item was added to the Preview list.
        Public Qty_SetbyUser As Double ''ol#9.123(1/23)... Reviewer 'Qty' column values are allowed to be a adjusted resulting in recalculation of 'Ext Price'.
    End Structure
    Public m_RegPackItems(5) As PackagingCharges

    Public m_DefaultPieceCharge As Double
    Public m_PiecesNumber As Integer ' = txtRegPiecesNo.Text
    Public m_PMSummary As Boolean
    Public m_PMOuterL As Long
    Public m_PMOuterW As Long
    Public m_PMOuterH As Long
    Public m_PMContentID As Long
    Public m_PMDesc As String
    Public m_PMContents As String
    Public m_PMDecVal As Double
    Public m_PMCharge As Double
    Public m_PMTax As Double
    Public m_PMCost As Double
    Public m_PMWeight As Double
    Public m_DefaultCountyName As String
    ''
    Private m_IsEnabled As Boolean ''ol#9.69(12/17)... Disable Pack Master II if 'Ship Multiple' was selected until the Pack Master queue is created.
    ''
    Public Function IsTAXable(ByVal iDept As String) As Boolean
        ''ol#1.2.29(12/17)... Don't calculate Tax in PM since it will be calculated in POS.
        Return False ' get out.
        If PackMasterII.GetTaxableStatus(iDept) Then
            If 0 = Len(PackMasterII.m_DefaultCountyName) Then
                Call PackMasterII.Get_DefaultTaxCountyName(PackMasterII.m_DefaultCountyName)
            End If
            '' TAXEXEMPT customer should be exempt from any taxes applied to a sale.
            IsTAXable = (Not UCase(PackMasterII.m_DefaultCountyName) = "TAXEXEMPT")
        End If
    End Function
    Public Function Calc_TAX(ByVal extCharge As Double, ByRef taxedExCharge As Double) As Boolean
        Dim TaxRate As Double
        ''
        taxedExCharge = 0 '' assume.
        If 0 = Len(PackMasterII.m_DefaultCountyName) Then
            Call PackMasterII.Get_DefaultTaxCountyName(PackMasterII.m_DefaultCountyName)
        End If
        Calc_TAX = PackMasterII.Get_DefaultTaxValue_ByDefaultCounty(PackMasterII.m_DefaultCountyName, TaxRate)
        If Calc_TAX Then
            PackMasterII.m_PMTax = _Convert.Round_Double2Decimals((TaxRate / 100) * extCharge, 4)
            taxedExCharge = _Convert.Round_Double2Decimals(extCharge + PackMasterII.m_PMTax, 2)
        End If
    End Function

#Region "Taxes"

    Public colTaxableDepartments As New List(Of String)

    Public Function Departments_FillOutTaxableCollection() As Boolean
        ''ToDo:
        'Dim dreader As OleDb.OleDbDataReader = Nothing
        ''
        'colTaxableDepartments.Clear() '' assume.
        'Departments_FillOutTaxableCollection = ShipRiteDb.Departments_GetAllTaxableDepartmentNames(dreader)
        'If Departments_FillOutTaxableCollection Then
        '    Do While dreader.Read
        '        colTaxableDepartments.Add(_Convert.Null2DefaultValue(dreader("Department")))
        '    Loop
        'End If
        '''
        '_Connection.CloseDataReader(dreader)
        Return False
    End Function
    Public Function GetTaxableStatus(DP As String) As Boolean
        GetTaxableStatus = False
        ''ol#7.57(5/22)... 'Taxable Departments' collection was created to avoid database errors return wrong value as not taxable (default) for actually taxable departments.
        If Not 0 = Len(DP) Then
            If Not 0 = colTaxableDepartments.Count Then
                GetTaxableStatus = colTaxableDepartments.Contains(DP)
                '_Debug.Print_(DP & " - taxable(" & GetTaxableStatus.ToString & ")")
            End If
        End If ''ol#7.57(5/22).
    End Function
    Public Function Get_DefaultTaxCountyName(ByRef countyName As String) As Boolean
        countyName = General.GetPolicyData(gShipriteDB, "DefaultCounty")
        Return True
    End Function
    Public Function Get_DefaultTaxValue_ByDefaultCounty(ByVal countyName As String, ByRef taxValue As Double) As Boolean
        ''ToDo:
        'Get_DefaultTaxValue_ByDefaultCounty = ShipRiteDb.CountyTaxes_GetDefaultTaxValue_ByDefaultCounty(countyName, taxValue)
        Return False
    End Function
#End Region

End Module

Public Class UPS

    Public Shared Function UPS_GetNumericZone(ServiceABBR As String, ZoneStr As String) As Integer
        ''
        Dim ZoneNum As Integer : ZoneNum = 0
        ''
        Try
            If isServiceDomestic(ServiceABBR) Then
                ''
                ZoneStr = Trim(UCase(ZoneStr))
                ZoneStr = Replace(ZoneStr, "ZONE", "")
                ZoneNum = Val(ZoneStr)
                ''
                If ZoneNum > 0 Then
                    ''
                    If _IDs.IsIt_AlaskaShipper Then
                        Select Case ServiceABBR
                            Case "1DAYEAM", "1DAY" : ZoneNum = ZoneNum - 20 ' 22, 24 ' 22, 24, 26
                            Case "1DAYSVR" : ZoneNum = 2 ' 20
                            Case "2DAYAM" : ZoneNum = ZoneNum - 10 ' 18
                            Case "2DAY" : ZoneNum = ZoneNum - 10 ' 14
                            Case "3DAYSEL" : ZoneNum = 0 ' not a service
                            Case "COM-GND" : ZoneNum = ZoneNum ' 2-11
                        End Select
                    ElseIf _IDs.IsIt_HawaiiShipper Then
                        Select Case ServiceABBR
                            Case "1DAYEAM", "1DAY" : ZoneNum = IIf(ZoneNum = 12, ZoneNum - 10, ZoneNum - 140) ' 142, 144 ' 142, 144, 146, 12 (101, 102)
                            Case "1DAYSVR" : ZoneNum = ZoneNum - 150 ' 152
                            Case "2DAYAM" : ZoneNum = ZoneNum - 10 ' 18 (248)
                            Case "2DAY" : ZoneNum = ZoneNum - 10 ' 14 (212-218, 224, 225), 16 (226) ' underlying zones are technically charge based on last number but no way for us to determine this zone
                            Case "3DAYSEL" : ZoneNum = 0 ' not a service
                            Case "COM-GND" : ZoneNum = ZoneNum ' 2-11
                        End Select
                    Else
                        Select Case ServiceABBR
                            Case "1DAYEAM", "1DAY" : ZoneNum = ZoneNum - 100 ' 102-108, 124 ' 102-108, 124-126
                            Case "1DAYSVR" : ZoneNum = ZoneNum - 130 ' 132-138
                            Case "2DAYAM" : ZoneNum = ZoneNum - 240 ' 242-248
                            Case "2DAY" : ZoneNum = ZoneNum - 200 ' 202-208, 224-226
                            Case "3DAYSEL" : ZoneNum = ZoneNum - 300 ' 302-308
                            Case "COM-GND" : ZoneNum = ZoneNum ' 2-8, 44-46
                        End Select
                    End If
                    ''
                End If
                ''
            End If
            ''
        Catch ex As Exception
            _Debug.PrintError_(ex.Message)
        End Try
        ''
        Return ZoneNum
        ''
    End Function

End Class

Public Module DSI

    ' 3rd Party Insurance
    Public gDSIis3rdPartyInsurance As Boolean
    Public DSI_Excluded_CountryList As New Dictionary(Of String, String)
    Public DSI_PremiereProgramMember As Boolean
    Public Const DSI_NewName As String = "Shipsurance"
    Public gDSISig As String

    Public Sub Print_DSI_EOD(Seq_Num As Long)

        ' To Do:
        'If Not Printing_.Set_SystemPrinter2PreviewDialogPrinter(True, fldReportPrinter) Then
        '    _Debug.Stop_() : GoTo Ooops
        'End If


        'Dim rep As New baseReportObject
        'rep.ReportName = "DSIeodReport.rpt"
        'rep.ReportFormula = "{manifest.DSI_Manifest#}= '" & Seq_Num & "'"
        'Call Printing_.Print_CrystalNetReport(fldReportPrinter, rep)  ''ol#9.173(8/8).

    End Sub

    Public Function Go_Online_ShipAndInsure(PackageID As String, Optional Delete As Boolean = False) As Boolean

        Try
            Dim SQL = "SELECT [Tracking#], CID, P1, DECVAL, ZIPCODE, Country FROM Manifest WHERE PACKAGEID = '" & PackageID & "'"
            Dim SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
            Dim TrackingNumber = ExtractElementFromSegment("Tracking#", SegmentSet)
            Dim CID = ExtractElementFromSegment("CID", SegmentSet)
            Dim CarrierCode As String = ShipandInsure_GetCarrierID(ExtractElementFromSegment("P1", SegmentSet))
            Dim r As String
            ShipandInsure_IsTest = False
            If Delete = False Then

                r = ShipandInsure_SaveBulkUploadItem(GetPolicyData(gShipriteDB, "ShipAndInsureUserID"), "Shiprite", GetPolicyData(gShipriteDB, "ShipAndInsurePassword"), GetPolicyData(gShipriteDB, "Name"), CID, TrackingNumber, CarrierCode, gShip.DecVal, GetPolicyData(gShipriteDB, "Zip"), gSelectedShipmentChoice.ZipCode, gShip.Country)

            Else

                r = ShipandInsure_DeleteBulkUploadItem(GetPolicyData(gShipriteDB, "ShipAndInsureUserID"), "Shiprite", GetPolicyData(gShipriteDB, "ShipAndInsurePassword"), GetPolicyData(gShipriteDB, "Name"), CID, TrackingNumber, CarrierCode, ExtractElementFromSegment("DECVAL", SegmentSet), GetPolicyData(gShipriteDB, "Zip"), ExtractElementFromSegment("ZIPCODE", SegmentSet), ExtractElementFromSegment("Country", SegmentSet))

            End If
            If Val(r) > 0 Then

                Return True

            Else

                Return False

            End If

        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to delete Insurance...")
            Return False
        End Try

    End Function

    Public Function Go_Online_DSI() As Boolean

        Dim SR_DB As String = String.Empty
        Dim setupRecords As String = String.Empty
        Dim setup2records As String = String.Empty
        Dim ManifestRecords As String = String.Empty
        Dim ContactsRecords As String = String.Empty
        Dim MasterRecords As String = String.Empty

        Dim url As String = String.Empty
        Dim PostData As String = String.Empty
        Dim Headers As String = String.Empty

        Dim Man_Num As Long
        Dim sql2exe As String = String.Empty

        Go_Online_DSI = False

        'setupRecords = SR_DB.OpenRecordset("Setup")
        'setup2Records = SR_DB.OpenRecordset("Setup2")
        sql2exe = "SELECT Manifest.* FROM Manifest WHERE " &
              "[DSI_Exported] = 'Pending' AND [DECVAL] > 0 And " &
              "([Exported] = '" & EOD.PickupWaitingStatus & "' OR [Exported] = 'Exported') " &
              "ORDER BY [PACKAGEID]"
        _Debug.Print_(sql2exe)
        ManifestRecords = DatabaseFunctions.IO_GetSegmentSet(gShipriteDB, sql2exe)

        ' Don't print labels for Shiprite Demo version.
        If _Debug.IsINHOUSE Then
            url = "https://sandbox.dsiins.com/api.net/dsi_recordShipment.aspx" '' test url
        Else
            url = "https://www.dsiins.com/api.net/dsi_recordShipment.aspx"
        End If

        Headers = "Content-Type: text" & vbCrLf

        'Set the Manifest #

        setup2records = General.GetPolicyData(gShipriteDB, "DSI_Manifest#")
        If Not String.IsNullOrEmpty(setup2records) Then
            Man_Num = setup2records
            Man_Num = Man_Num + 1
        Else
            Man_Num = 1
        End If

        Dim extPolicyID As String = General.GetPolicyData(gShipriteDB, "DSI_PolicyID")
        Dim personCompany As String = Replace(General.GetPolicyData(gShipriteDB, "Name"), "&", "And")
        Dim personFirstName As String = General.GetPolicyData(gShipriteDB, "FName")
        Dim personLastName As String = General.GetPolicyData(gShipriteDB, "LName")
        Dim personPhone As String = General.GetPolicyData(gShipriteDB, "Phone1")
        Dim personFax As String = General.GetPolicyData(gShipriteDB, "Phone2")
        Dim personEmail As String = General.GetPolicyData(gShipriteDB, "DSI_Email")

        Do Until String.IsNullOrEmpty(ManifestRecords)

            PostData = "extPersonSourceId=6"
            PostData = PostData & "&sourceUsername=shiprite"
            PostData = PostData & "&sourcePassword=mdrit7"
            PostData = PostData & "&extPolicyID=" & extPolicyID
            PostData = PostData & "&personSourceIdentifier="
            PostData = PostData & "&personCompany=" & personCompany
            PostData = PostData & "&personFirstName=" & personFirstName
            PostData = PostData & "&personLirstName=" & personLastName
            PostData = PostData & "&personPhone=" & personPhone
            PostData = PostData & "&personFax=" & personFax
            PostData = PostData & "&personEmail=" & personEmail

            Dim ManifestSegment As String = GetNextSegmentFromSet(ManifestRecords)
            Dim packageID As String = ExtractElementFromSegment("PACKAGEID", ManifestSegment)
            PostData = PostData & "&recordSourceIdentifier=" & packageID

            '_Debug.Print_(ManifestSegment)
            Dim serviceName As String = ExtractElementFromSegment("ServiceName", ManifestSegment)
            Dim SplitService() As String = serviceName.Split()
            If UBound(SplitService) < 1 Then
                '
                PostData = PostData & "&extShipmentTypeId=1"
                '
            ElseIf SplitService(1) = "Freight" Then

                PostData = PostData & "&extShipmentTypeId=2"
            Else
                PostData = PostData & "&extShipmentTypeId=1"
            End If

            Dim carrier As String = ExtractElementFromSegment("Carrier", ManifestSegment)
            If carrier = "USPS" Then
                PostData = PostData & "&extCarrierID=1"
            ElseIf carrier = "FedEx" Then
                PostData = PostData & "&extCarrierID=2"
            ElseIf carrier = "UPS" Then
                PostData = PostData & "&extCarrierID=3"
            ElseIf carrier = "DHL" Then
                PostData = PostData & "&extCarrierID=4"
            ElseIf carrier = SpeeDee.CarrierName Then
                PostData = PostData & "&extCarrierID=17"
            End If

            PostData = PostData & "&carrierServiceName=" & serviceName
            PostData = PostData & "&referenceNumber=" & packageID
            PostData = PostData & "&trackingNumber=" & ExtractElementFromSegment("TRACKING#", ManifestSegment)
            PostData = PostData & "&declaredValue=" & Val(ExtractElementFromSegment("DECVAL", ManifestSegment)).ToString("C")

            'Time to fix the date
            Dim transactionDate As String = _Convert.String2Date(ExtractElementFromSegment("Date", ManifestSegment))
            PostData = PostData & "&transactionDate=" & transactionDate

            Dim pickupDate As String = _Convert.String2Date(ExtractElementFromSegment("PICKUPDATE", ManifestSegment))
            If Not pickupDate = "12:00:00 AM" Then
                PostData = PostData & "&shipmentDate=" & pickupDate
            Else
                PostData = PostData & "&shipmentDate=" & transactionDate
            End If

            PostData = PostData & "&arrivalDate="
            PostData = PostData & "&extCommodityCatagoryId="
            PostData = PostData & "&extCommodityTypeId="
            PostData = PostData & "&extPackageTypeId="
            PostData = PostData & "&packageCount=1"
            PostData = PostData & "&containsGlass="

            'Bless you Todd. the memo field was added
            Dim Contents As String = ExtractElementFromSegment("Contents", ManifestSegment)
            If Not String.IsNullOrEmpty(Contents) Then
                PostData = PostData & "&packageDescription=" & Contents
            Else
                PostData = PostData & "&packageDescription=Default Package"
            End If

            Dim Temp_DSI As String = ExtractElementFromSegment("SID", ManifestSegment)
            ContactsRecords = DatabaseFunctions.IO_GetSegmentSet(gShipriteDB, "SELECT Contacts.* FROM Contacts WHERE [ID] =" & Temp_DSI)

            If Not String.IsNullOrEmpty(ContactsRecords) Then
                PostData = PostData & "&departureAddress1=" & ExtractElementFromSegment("Addr1", ContactsRecords)
                PostData = PostData & "&departureAddress2=" & ExtractElementFromSegment("Addr2", ContactsRecords)
                PostData = PostData & "&departureCity=" & ExtractElementFromSegment("City", ContactsRecords)
                PostData = PostData & "&departureState=" & ExtractElementFromSegment("State", ContactsRecords)
                PostData = PostData & "&departurePostalCode=" & ExtractElementFromSegment("Zip", ContactsRecords)
                PostData = PostData & "&departureCountry=" & ExtractElementFromSegment("Country", ContactsRecords)
            End If

            'ContactsRecords.Close

            Temp_DSI = ExtractElementFromSegment("CID", ManifestSegment)
            ContactsRecords = DatabaseFunctions.IO_GetSegmentSet(gShipriteDB, "SELECT Contacts.* FROM Contacts WHERE [ID] =" & Temp_DSI)

            If Not String.IsNullOrEmpty(ContactsRecords) Then
                PostData = PostData & "&destinationAddress1=" & ExtractElementFromSegment("Addr1", ContactsRecords)
                PostData = PostData & "&destinationAddress2=" & ExtractElementFromSegment("Addr2", ContactsRecords)
                PostData = PostData & "&destinationCity=" & ExtractElementFromSegment("City", ContactsRecords)
                PostData = PostData & "&destinationState=" & ExtractElementFromSegment("State", ContactsRecords)
                PostData = PostData & "&destinationPostalCode=" & ExtractElementFromSegment("Zip", ContactsRecords)
                PostData = PostData & "&destinationCountry=" & ExtractElementFromSegment("Country", ContactsRecords)
            End If

            'ContactsRecords.Close

            ' "Member of DSI Premiere Program" check box was added to the DSI Insurance Setup screen.
            If DSI_PremiereProgramMember Then
                PostData = PostData & "&preDeductFlag=1"
            Else
                PostData = PostData & "&preDeductFlag=0"
            End If

            MasterRecords = DatabaseFunctions.IO_GetSegmentSet(gShipriteDB, "Select Master.* FROM Master WHERE [SERVICE] ='" & ExtractElementFromSegment("P1", ManifestSegment) & "'")
            Dim MasterSegment As String = GetNextSegmentFromSet(MasterRecords)
            PostData = PostData & "&dsiRatePer100=" & Val(ExtractElementFromSegment("thirdACTDecVal", ManifestSegment)).ToString("C")

            'MasterRecords.Close

            'Inet1.RequestTimeout = 60
            'Inet1.Protocol = icHTTPS
            'Inet1.Execute url & "?" & PostData, "POST", "", Headers

            Dim vtData As String = String.Empty
            If Not _XML.Send_HttpWebRequest(url & "?" & PostData, vtData) Then

                MsgBox("No response from server. " & vtData, vbExclamation, "Cannot connect to " & DSI_NewName & " Server!")
                Exit Function

            End If

            Dim SplitResponse() As String = vtData.Split(",")

            If UBound(SplitResponse) > -1 Then
                '
                Dim sql2cmd As New sqlUpdate

                If SplitResponse(0) = "1" Then

                    With sql2cmd
                        .Qry_UPDATE("DSI_Exported", "Exported", .TXT_, True, False, "Manifest", "PACKAGEID = '" & packageID & "'")
                        .Qry_UPDATE("DSI_ShipmentID", SplitResponse(2), .TXT_)
                        sql2exe = .Qry_UPDATE("DSI_Manifest#", Man_Num.ToString, .TXT_, False, True)
                    End With

                    Go_Online_DSI = True

                    'ManifestRecords.Update

                    _Debug.Print_(sql2exe)
                    If -1 = IO_UpdateSQLProcessor(gShipriteDB, sql2exe) Then
                        '
                        _Debug.Print_("Error")
                        '
                    End If


                ElseIf UBound(SplitResponse) > 0 Then

                    ' ShipRite should give to users a choice to delete DSI past-due-date shipments after trying to upload them.
                    With sql2cmd
                        If vbYes = MsgBox(SplitResponse(1) & vbCr & vbCr & "Delete this entry from EOD list?", vbExclamation + vbYesNo, DSI_NewName & " Error: Contact " & DSI_NewName) Then
                            .Qry_UPDATE("ERROR", SplitResponse(1), .TXT_, True, False, "Manifest", "PACKAGEID = '" & packageID & "'")
                            sql2exe = .Qry_UPDATE("DSI_Exported", "Uninsured", .TXT_, False, True)
                        Else
                            sql2exe = .Qry_UPDATE("ERROR", SplitResponse(1), .TXT_, True, True, "Manifest", "PACKAGEID = '" & packageID & "'")
                        End If
                    End With

                    _Debug.Print_(SplitResponse(1))
                    _Debug.Print_(PostData)

                    'ManifestRecords.Update

                    _Debug.Print_(sql2exe)
                    If -1 = IO_UpdateSQLProcessor(gShipriteDB, sql2exe) Then
                        '
                        _Debug.Print_("Error")
                        '
                    End If


                Else

                    ' ShipRite should give to users a choice to delete DSI past-due-date shipments after trying to upload them.
                    If vbYes = MsgBox(SplitResponse(0) & vbCr & vbCr & "Delete this entry from EOD list?", vbExclamation + vbYesNo, DSI_NewName & " Error: Contact " & DSI_NewName) Then

                        With sql2cmd
                            sql2exe = .Qry_UPDATE("DSI_Exported", "Uninsured", .TXT_, True, True, "Manifest", "PACKAGEID = '" & packageID & "'")
                        End With

                        _Debug.Print_(SplitResponse(0))
                        _Debug.Print_(PostData)

                        'ManifestRecords.Update

                        _Debug.Print_(sql2exe)
                        If -1 = IO_UpdateSQLProcessor(gShipriteDB, sql2exe) Then
                            '
                            _Debug.Print_("Error")
                            '
                        End If

                    End If
                    Exit Do
                    ''
                End If
                ''
            End If
            '
            'ManifestRecords.MoveNext 
        Loop

        If Go_Online_DSI = True Then

            'setup2records.Edit
            General.UpdatePolicy(gShipriteDB, "DSI_Manifest#", Man_Num.ToString)
            ' To Do:
            DSI.Print_DSI_EOD(Man_Num)
            '
        End If

    End Function
    Public Function Go_Online_DSI(ByVal packageID As String) As Boolean

        Dim setup2records As String = String.Empty
        Dim ManifestRecords As String = String.Empty
        Dim ContactsRecords As String = String.Empty
        Dim MasterRecords As String = String.Empty

        Dim url As String = String.Empty
        Dim PostData As String = String.Empty

        Dim Man_Num As Long
        Dim sql2exe As String = String.Empty

        Go_Online_DSI = False

        sql2exe = "SELECT Manifest.* FROM Manifest WHERE [PACKAGEID] = '" & packageID & "' And [DSI_Exported] = 'Pending'"
        _Debug.Print_(sql2exe)
        ManifestRecords = DatabaseFunctions.IO_GetSegmentSet(gShipriteDB, sql2exe)

        ' Don't print labels for Shiprite Demo version.
        If _Debug.IsINHOUSE Then
            url = "https://sandbox.dsiins.com/api.net/dsi_recordShipment.aspx" '' test url
        Else
            url = "https://www.dsiins.com/api.net/dsi_recordShipment.aspx"
        End If

        'Set the Manifest #

        setup2records = General.GetPolicyData(gShipriteDB, "DSI_Manifest#")
        If Not String.IsNullOrEmpty(setup2records) Then
            Man_Num = setup2records
            Man_Num = Man_Num + 1
        Else
            Man_Num = 1
        End If

        Dim extPolicyID As String = General.GetPolicyData(gShipriteDB, "DSI_PolicyID")
        Dim personCompany As String = Replace(General.GetPolicyData(gShipriteDB, "Name"), "&", "And")
        Dim personFirstName As String = General.GetPolicyData(gShipriteDB, "FName")
        Dim personLastName As String = General.GetPolicyData(gShipriteDB, "LName")
        Dim personPhone As String = General.GetPolicyData(gShipriteDB, "Phone1")
        Dim personFax As String = General.GetPolicyData(gShipriteDB, "Phone2")
        Dim personEmail As String = General.GetPolicyData(gShipriteDB, "DSI_Email")

        Do Until String.IsNullOrEmpty(ManifestRecords)

            PostData = "extPersonSourceId=6"
            PostData = PostData & "&sourceUsername=shiprite"
            PostData = PostData & "&sourcePassword=mdrit7"
            PostData = PostData & "&extPolicyID=" & extPolicyID
            PostData = PostData & "&personSourceIdentifier="
            PostData = PostData & "&personCompany=" & personCompany
            PostData = PostData & "&personFirstName=" & personFirstName
            PostData = PostData & "&personLirstName=" & personLastName
            PostData = PostData & "&personPhone=" & personPhone
            PostData = PostData & "&personFax=" & personFax
            PostData = PostData & "&personEmail=" & personEmail

            Dim ManifestSegment As String = GetNextSegmentFromSet(ManifestRecords)
            PostData = PostData & "&recordSourceIdentifier=" & packageID

            _Debug.Print_(ManifestSegment)
            Dim serviceName As String = ExtractElementFromSegment("ServiceName", ManifestSegment)
            Dim SplitService() As String = serviceName.Split()
            If UBound(SplitService) < 1 Then
                '
                PostData = PostData & "&extShipmentTypeId=1"
                '
            ElseIf SplitService(1) = "Freight" Then

                PostData = PostData & "&extShipmentTypeId=2"
            Else
                PostData = PostData & "&extShipmentTypeId=1"
            End If

            Dim carrier As String = ExtractElementFromSegment("Carrier", ManifestSegment)
            If carrier = "USPS" Then
                PostData = PostData & "&extCarrierID=1"
            ElseIf carrier = "FedEx" Then
                PostData = PostData & "&extCarrierID=2"
            ElseIf carrier = "UPS" Then
                PostData = PostData & "&extCarrierID=3"
            ElseIf carrier = "DHL" Then
                PostData = PostData & "&extCarrierID=4"
            ElseIf carrier = SpeeDee.CarrierName Then
                PostData = PostData & "&extCarrierID=17"
            End If

            PostData = PostData & "&carrierServiceName=" & serviceName
            PostData = PostData & "&referenceNumber=" & packageID
            PostData = PostData & "&trackingNumber=" & ExtractElementFromSegment("TRACKING#", ManifestSegment)
            PostData = PostData & "&declaredValue=" & Val(ExtractElementFromSegment("DECVAL", ManifestSegment)).ToString("C")

            'Time to fix the date
            Dim transactionDate As String = _Convert.String2Date(ExtractElementFromSegment("Date", ManifestSegment))
            PostData = PostData & "&transactionDate=" & transactionDate

            Dim pickupDate As String = _Convert.String2Date(ExtractElementFromSegment("PICKUPDATE", ManifestSegment))
            If Not pickupDate = "12:00:00 AM" Then
                PostData = PostData & "&shipmentDate=" & pickupDate
            Else
                PostData = PostData & "&shipmentDate=" & transactionDate
            End If

            PostData = PostData & "&arrivalDate="
            PostData = PostData & "&extCommodityCatagoryId="
            PostData = PostData & "&extCommodityTypeId="
            PostData = PostData & "&extPackageTypeId="
            PostData = PostData & "&packageCount=1"
            PostData = PostData & "&containsGlass="

            'Bless you Todd. the memo field was added
            Dim Contents As String = ExtractElementFromSegment("Contents", ManifestSegment)
            If Not String.IsNullOrEmpty(Contents) Then
                PostData = PostData & "&packageDescription=" & Contents
            Else
                PostData = PostData & "&packageDescription=Default Package"
            End If

            Dim Temp_DSI As String = ExtractElementFromSegment("SID", ManifestSegment)
            ContactsRecords = DatabaseFunctions.IO_GetSegmentSet(gShipriteDB, "SELECT Contacts.* FROM Contacts WHERE [ID] =" & Temp_DSI)

            If Not String.IsNullOrEmpty(ContactsRecords) Then
                PostData = PostData & "&departureAddress1=" & ExtractElementFromSegment("Addr1", ContactsRecords)
                PostData = PostData & "&departureAddress2=" & ExtractElementFromSegment("Addr2", ContactsRecords)
                PostData = PostData & "&departureCity=" & ExtractElementFromSegment("City", ContactsRecords)
                PostData = PostData & "&departureState=" & ExtractElementFromSegment("State", ContactsRecords)
                PostData = PostData & "&departurePostalCode=" & ExtractElementFromSegment("Zip", ContactsRecords)
                PostData = PostData & "&departureCountry=" & ExtractElementFromSegment("Country", ContactsRecords)
            End If

            'ContactsRecords.Close

            Temp_DSI = ExtractElementFromSegment("CID", ManifestSegment)
            ContactsRecords = DatabaseFunctions.IO_GetSegmentSet(gShipriteDB, "SELECT Contacts.* FROM Contacts WHERE [ID] =" & Temp_DSI)

            If Not String.IsNullOrEmpty(ContactsRecords) Then
                PostData = PostData & "&destinationAddress1=" & ExtractElementFromSegment("Addr1", ContactsRecords)
                PostData = PostData & "&destinationAddress2=" & ExtractElementFromSegment("Addr2", ContactsRecords)
                PostData = PostData & "&destinationCity=" & ExtractElementFromSegment("City", ContactsRecords)
                PostData = PostData & "&destinationState=" & ExtractElementFromSegment("State", ContactsRecords)
                PostData = PostData & "&destinationPostalCode=" & ExtractElementFromSegment("Zip", ContactsRecords)
                PostData = PostData & "&destinationCountry=" & ExtractElementFromSegment("Country", ContactsRecords)
            End If

            'ContactsRecords.Close

            ' "Member of DSI Premiere Program" check box was added to the DSI Insurance Setup screen.
            If DSI_PremiereProgramMember Then
                PostData = PostData & "&preDeductFlag=1"
            Else
                PostData = PostData & "&preDeductFlag=0"
            End If

            MasterRecords = DatabaseFunctions.IO_GetSegmentSet(gShipriteDB, "Select Master.* FROM Master WHERE [SERVICE] ='" & ExtractElementFromSegment("P1", ManifestSegment) & "'")
            Dim MasterSegment As String = GetNextSegmentFromSet(MasterRecords)
            PostData = PostData & "&dsiRatePer100=" & Val(ExtractElementFromSegment("thirdACTDecVal", ManifestSegment)).ToString("C")

            'MasterRecords.Close

            'Inet1.RequestTimeout = 60
            'Inet1.Protocol = icHTTPS
            'Inet1.Execute url & "?" & PostData, "POST", "", Headers

            Dim vtData As String = String.Empty
            If Not _XML.Send_HttpWebRequest(url & "?" & PostData, vtData) Then

                MsgBox("No response from server. " & vtData, vbExclamation, "Cannot connect to " & DSI_NewName & " Server!")
                Exit Function

            End If

            Dim SplitResponse() As String = vtData.Split(",")

            If UBound(SplitResponse) > -1 Then
                '
                Dim sql2cmd As New sqlUpdate

                If SplitResponse(0) = "1" Then

                    With sql2cmd
                        .Qry_UPDATE("DSI_Exported", "Exported", .TXT_, True, False, "Manifest", "PACKAGEID = '" & packageID & "'")
                        .Qry_UPDATE("DSI_ShipmentID", SplitResponse(2), .TXT_)
                        sql2exe = .Qry_UPDATE("DSI_Manifest#", Man_Num.ToString, .TXT_, False, True)
                    End With

                    Go_Online_DSI = True

                    'ManifestRecords.Update

                    _Debug.Print_(sql2exe)
                    If -1 = IO_UpdateSQLProcessor(gShipriteDB, sql2exe) Then
                        '
                        _Debug.Print_("Error")
                        '
                    End If
                    '
                End If
                ''
            End If
            '
            'ManifestRecords.MoveNext 
        Loop

        If Go_Online_DSI = True Then

            'setup2records.Edit
            General.UpdatePolicy(gShipriteDB, "DSI_Manifest#", Man_Num.ToString)
            ' To Do:
            'DSI.Print_DSI_EOD(Man_Num)
            '
        End If

    End Function
    Public Function Void_PackageCoverage(ByVal packageID As String) As Boolean
        Void_PackageCoverage = True ' assume.
        '
        Dim extPolicyID As String = General.GetPolicyData(gShipriteDB, "DSI_PolicyID")
        Dim personEmail As String = General.GetPolicyData(gShipriteDB, "DSI_Email")
        '
        ' Don't print labels for Shiprite Demo version.
        Dim url As String = String.Empty
        If _Debug.IsINHOUSE Then
            url = "https://sandbox.dsiins.com/api.net/dsi_voidRecordShipment.aspx" '' test url
        Else
            url = "https://www.dsiins.com/api.net/dsi_voidRecordShipment.aspx"
        End If
        '
        Dim sql2exe As String = "SELECT DSI_ShipmentID FROM Manifest WHERE [PACKAGEID] = '" & packageID & "'"
        _Debug.Print_(sql2exe)
        Dim SegmentSet As String = DatabaseFunctions.IO_GetSegmentSet(gShipriteDB, sql2exe)
        Dim DSI_ShipmentID As String = ExtractElementFromSegment("DSI_ShipmentID", SegmentSet)
        '
        If Not String.IsNullOrEmpty(DSI_ShipmentID) Then
            '
            Dim PostData As String = "extPersonSourceId=6"
            PostData = PostData & "&sourceUsername=shiprite"
            PostData = PostData & "&sourcePassword=mdrit7"
            PostData = PostData & "&extPolicyID=" & extPolicyID
            PostData = PostData & "&personEmail=" & personEmail
            PostData = PostData & "&recordedShipmentId=" & DSI_ShipmentID
            PostData = PostData & "&extRSVoidReasonId=1" ' Shipment Cancelled
            PostData = PostData & "&voidDescription=Shipment Cancelled"
            '
            Dim vtData As String = String.Empty
            If Not _XML.Send_HttpWebRequest(url & "?" & PostData, vtData) Then

                MsgBox("No response from server. " & vtData, vbExclamation, "Cannot connect to " & DSI_NewName & " Server!")
                Exit Function

            End If

            Dim SplitResponse() As String = vtData.Split(",")

            If UBound(SplitResponse) > -1 Then
                '
                Dim sql2cmd As New sqlUpdate

                If SplitResponse(0) = "1" Then

                    With sql2cmd
                        sql2exe = .Qry_UPDATE("DSI_Exported", "Voided", .TXT_, True, True, "Manifest", "PACKAGEID = '" & packageID & "'")
                    End With

                    'ManifestRecords.Update

                    _Debug.Print_(sql2exe)
                    If -1 = IO_UpdateSQLProcessor(gShipriteDB, sql2exe) Then
                        '
                        _Debug.Print_("Error")
                        '
                    End If
                    '
                ElseIf SplitResponse(0) = "2" Then
                    '
                    _MsgBox.ErrorMessage(SplitResponse(1), "Failed to void package insurance...", DSI_NewName)
                    Void_PackageCoverage = False
                    '
                End If
                '
            End If
            '
        End If
        '
    End Function

End Module

Public Module Shipping

    Public gShip As New gShip_Class
    Public gCountry As List(Of _CountryDB)

    Public Function Calc_DimWeight(ByVal LL As Long, ByVal WW As Long, ByVal hh As Long, ByVal IsInternational As Boolean, Optional CarrierName As String = "") As Long
        Dim dmw As Double
        ''
        If LL > 0 And WW > 0 And hh > 0 Then
            '
            ''ol#9.69(12/15)... Beginning January 3, 2011, the divisor used to calculate dimensional weight will change.
            If Not "USPS" = CarrierName Then ''ol#9.93(4/22)... USPS still has the original dimensional weight calculation (prior January 3, 2011)
                '
                If IsInternational Then
                    dmw = (LL * WW * hh) / 139 ' 166 to 139
                Else
                    dmw = (LL * WW * hh) / 166 ' 194 to 166
                End If
                '
            Else
                '
                If IsInternational Then
                    dmw = (LL * WW * hh) / 166
                Else
                    dmw = (LL * WW * hh) / 194
                End If
                '
            End If
            '
            If dmw - Int(dmw) > 0 Then
                dmw = Int(dmw) + 1
            End If
            '
        End If
        ''
        Calc_DimWeight = Val(Format$(dmw, "0"))
        ''
    End Function
    Public Function Load_CountryDB() As Boolean
        Load_CountryDB = False
        '
        If gCountry Is Nothing Then
            gCountry = New List(Of _CountryDB)
            '
            Dim lindex As Long = 0
            Dim sql2exe As String = "Select * From Country Order by Country_Name"
            Dim SegmentSet As String = DatabaseFunctions.IO_GetSegmentSet(gCountryDB, sql2exe)
            If Not String.IsNullOrEmpty(SegmentSet) Then
                Do Until SegmentSet = String.Empty
                    '
                    Dim Segment As String = GetNextSegmentFromSet(SegmentSet)
                    '
                    Dim country As New _CountryDB
                    With country
                        .CountryName = ExtractElementFromSegment("Country_Name", Segment)
                        .CountryCode = ExtractElementFromSegment("Country_Cd", Segment)
                        .CountryCode3 = ExtractElementFromSegment("Country_Cd_3", Segment)
                        .CountryCodeNumeric = ExtractElementFromSegment("Country_Cd_Numeric", Segment)
                        .CountryCurrency = ExtractElementFromSegment("Country_Curr_Cd", Segment)
                        .ListIndex = lindex
                    End With

                    If country.CountryName <> "USA" Then
                        gCountry.Add(country)
                    End If
                    lindex += 1
                    '
                Loop
            End If
        End If
        '
        Return 0 < gCountry.Count
    End Function
    Public Function Find_CountryObject_byName(ByVal countryname As String, ByRef countryobject As _CountryDB) As Boolean
        Find_CountryObject_byName = False ' assume.
        '
        For Each cntry As _CountryDB In gCountry
            If UCase(countryname) = UCase(cntry.CountryName) Then
                countryobject = cntry
                Return True
            End If
        Next
    End Function
    Public Function Find_CountryObject_byCode(ByVal countrycode As String, ByRef countryobject As _CountryDB) As Boolean
        Find_CountryObject_byCode = False ' assume.
        '
        For Each cntry As _CountryDB In gCountry
            If UCase(countrycode) = UCase(cntry.CountryCode) Then
                countryobject = cntry
                Return True
            End If
        Next
    End Function

End Module
Public Module EOD
    Public Const PickupWaitingStatus As String = "Pickup Waiting"
    Public Const PickupScheduledStatus As String = "Pickup Scheduled"
    Public Const DeletedStatus As String = "Deleted"
    Public Const ExportedStatus As String = "Exported"
    Public Const PendingStatus As String = "Pending"
    Public Const HoldStatus As String = "Hold"
    Public Const ResubStatus As String = "Resub"
    Public Const OpenShipStatus As String = "Open Ship" ' 'FedEx Open Ship' packages will have 'Open Ship' status displayed indicating that the packages were not Confirmed/Closed for shipping.
    Public Const ConsolidatorStatus As String = "Consolidator" ' New 'Consolidator' EOD status will be assigned to Newgistics shipments.

End Module


Public Module ShipRiteDb
#Region "Common DB Functions"

    Private dset As New System.Data.DataSet
    Private dadapter As New System.Data.OleDb.OleDbDataAdapter
    Public Const tblInventory As String = "Inventory"
    Public Const tblPackMaster As String = "PackMaster"
    Public Const tblPackMaterials As String = "PackMaterials"
    Public Const tblPackContents As String = "PackContents"
    Public Const tblPackFragile As String = "PackFragile"
    Private path2db As String = gShipriteDB '"C:\ShipriteNext\Data\Shiprite.mdb" 'Temp Location

    Public Sub Close_dreader(ByRef dreader As OleDb.OleDbDataReader)
        If Not dreader.IsClosed Then
            dreader.Close()
        End If
        dreader = Nothing
    End Sub

    Public Function execute_cmd(ByVal sql2exe As String) As Boolean
        If IO_UpdateSQLProcessor(path2db, sql2exe) >= 0 Then
            Return True
        End If
        Return False
    End Function

    Public Function Setup2_GetFedExHAL_IDs(ByRef locationID As String, ByRef agentID As String) As Boolean
        ''ol#1.2.43(10/20)... FedExWeb HAL IDs will be read from Setup2.
        locationID = String.Empty ' assume.
        agentID = String.Empty ' assume.
        Dim dreader As OleDb.OleDbDataReader = Nothing
        Dim sql2exe As String = "SELECT [FedExHAL_LocationID], [FedExHAL_AgentID] FROM [Setup2] WHERE [ID] > 0"
        Setup2_GetFedExHAL_IDs = get_dreader(sql2exe, dreader, True)
        If Setup2_GetFedExHAL_IDs Then
            locationID = _Convert.Null2DefaultValue(dreader("FedExHAL_LocationID"))
            agentID = _Convert.Null2DefaultValue(dreader("FedExHAL_AgentID"))
        End If
        ShipRiteDb.Close_dreader(dreader)
    End Function

    Public Function Load_DataSet_Inventory() As Boolean
        'Dim sql2exe As String = "Select SKU, Desc, Sell, Quantity, Department From Inventory Where MaterialsClass = 'Rental'"
        Dim sql2exe As String = "Select SKU, Desc, Sell, Quantity, Department From Inventory"
        Load_DataSet_Inventory = get_dset(tblInventory, sql2exe)
    End Function
    Public Function Load_DataSet_Fragility() As Boolean
        Dim sql2exe As String = "SELECT * FROM PackMasterFragility"
        Load_DataSet_Fragility = get_dset(tblPackFragile, sql2exe)
    End Function
    Public Function Load_DataSet_Inventory_PackMaterials(ByVal dtablename As String) As Boolean
        ''ol#1.2.06(6/18)... Boxes will be available if one of the two conditions are met: 
        '' 'Zero Means Out of Stock' is checked and Quantity > 0 Or 'Zero Means Out of Stock' is un-checked (Quantity then ignored).
        ''ol#1.2.04(6/11)... Only 'Package Estimator' checked inventory items should be available.
        Dim sql2exe As String = "SELECT Inventory.*, (Inventory.[L]*Inventory.[W]*Inventory.[H]) as Volume FROM Inventory " &
            "WHERE [MaterialsClass] <> '' " &
            "And [PackagingMaterials] = True " &
            "And ([Zero]=False Or ([Zero]=True And [Quantity]>0)) " &
            "ORDER BY [DefaultInClass], (Inventory.[L]*Inventory.[W]*Inventory.[H]), [SKU]"
        Load_DataSet_Inventory_PackMaterials = get_dset(dtablename, sql2exe)
    End Function
    Public Function Load_DataSet_Contents(ByVal dtablename As String) As Boolean
        Load_DataSet_Contents = False
        Dim sql2exe As String = "SELECT Contents.* FROM Contents ORDER BY [Contents] DESC"
        Load_DataSet_Contents = get_dset(dtablename, sql2exe)
    End Function

    Public Function Get_DataTable(ByVal dtableName As String, ByRef dtable As Data.DataTable) As Boolean
        Get_DataTable = _DataSet.IsExist_DataTable(dset, dtableName, dtable)
    End Function

    Private Function get_dset(ByVal dtableName As String, ByVal sql2exe As String) As Boolean
        Dim errorDesc As String = String.Empty
        get_dset = _DataSet.Load_DataTable(_Connection.Jet_OLEDB, dtableName, gShipriteDB, sql2exe, dset, dadapter, errorDesc, True)
        If Not get_dset Then
            If 0 < errorDesc.Length Then
                Throw New ArgumentException(errorDesc)
            End If
        End If
    End Function

    Public Function Setup_GetAddress_StoreOwner(ByRef objContact As _baseContact) As Boolean
        objContact = _StoreOwner.Load_StoreOwnerContact()
        Return (objContact.CompanyName.Length > 0)
    End Function

    Public Function get_dreader(ByVal sql2exe As String, ByRef dreader As OleDb.OleDbDataReader, Optional ByVal is2read As Boolean = False) As Boolean
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
        get_dreader_onevalue = False
        Dim dreader As OleDb.OleDbDataReader = Nothing
        onevalue = String.Empty ' assume.
        If get_dreader(sql2exe, dreader, True) Then
            onevalue = _Convert.Null2DefaultValue(dreader(0), "")
            get_dreader_onevalue = True
        End If
        _Connection.CloseDataReader(dreader)
    End Function
#End Region

#Region "Contacts"
    Public Function Contacts_GetCountryName(ByVal contactID As Long) As String
        Contacts_GetCountryName = String.Empty ' assume.
        Dim sql2exe As String = "Select [Country] From Contacts Where [ID] = " & contactID.ToString
        Dim country As String = String.Empty
        If get_dreader_onevalue(sql2exe, country) Then
            Contacts_GetCountryName = country
        End If
    End Function
#End Region

#Region "PackMaster"

    Public isOpen_ShipNew As Boolean  ' opened from ShipMaster or POS 
    Public isOpen_PackMaster As Boolean  ' opened from Packmaster

    Public dtlPackMaterials As DataTable
    Public dtlPackMaterials_Filter As DataTable
    Public dtlPackContents As DataTable
    Public dtlPackFragile As DataTable

    Public Function PackMaster_GetPackageDetails(ByVal contentID As Long, ByRef dreader As String) As Boolean
        Dim sql2exe As String = "SELECT PackMaster.* FROM PackMaster WHERE [ContentID] = '" & contentID.ToString & "' Order by [ID]"
        dreader = IO_GetSegmentSet(gShipriteDB, sql2exe)
        PackMaster_GetPackageDetails = Not String.IsNullOrEmpty(dreader)
    End Function
    Public Function PackMasterSetup_GetFragility(ByRef dreader As String) As Boolean
        Dim sql2exe As String = "SELECT * FROM PackMasterFragility"
        dreader = IO_GetSegmentSet(gShipriteDB, sql2exe)
        PackMasterSetup_GetFragility = Not String.IsNullOrEmpty(dreader)
    End Function
#End Region

#Region "Contents"
    Public Function Contents_GetCID(ByVal tempCID As String, ByRef CID As Long) As Boolean
        Dim dreader As String = String.Empty
        CID = 0 ' assume.
        Dim sql2exe As String = "Select [CID] From Contents Where Category = '" & tempCID & "'"
        Dim SegmentSet As String = IO_GetSegmentSet(gShipriteDB, sql2exe)
        Dim Segment As String = GetNextSegmentFromSet(SegmentSet)
        CID = Val(ExtractElementFromSegment("CID", Segment))
        Contents_GetCID = (CID > 0)
    End Function

#End Region

#Region "Setup2"
    'Public Function Setup2_GetPackMasterDefaults(ByRef dreader As OleDb.OleDbDataReader) As Boolean
    '    Dim sql2exe As String = "SELECT " & fldLabor_BuildUp & ", " & fldLabor_CutDown & ", " & fldLabor_Telescope & ", " & fldLabor_AddTop & ", [defaultLabor], [defaultFill], [defaultPieceCharge], [fragEasy], [fragMedium], [fragHard], [threseasy], [thresmedium], [thresHard] FROM [Setup2] WHERE [ID] <> 0"
    '    Setup2_GetPackMasterDefaults = get_dreader(sql2exe, dreader, True)
    'End Function
    Public Function Setup2_GetSMTP_Settings(ByRef segment As String) As Boolean
        Dim sql2exe As String = "Select SMTPUserID, SMTPUserPassword, SMTPServerName, SMTPServerPort, SMTPServerEncrypted, SMTPEmailCopy, SMTPNotificationLog From [Setup2] Where [ID]>0"
        segment = GetNextSegmentFromSet(IO_GetSegmentSet(path2db, sql2exe))
        Setup2_GetSMTP_Settings = Not String.IsNullOrEmpty(segment)
    End Function
    'Public Function Setup2_GetReceiptOnOffOptions() As String
    '    Setup2_GetReceiptOnOffOptions = String.Empty ' assume.
    '    Dim sql2exe As String = "Select ReceiptOnOffOptions From [Setup2] Where [ID]>0"
    '    Dim val As String = String.Empty
    '    If get_dreader_onevalue(sql2exe, val) Then
    '        Return val
    '    End If
    'End Function
    'Public Function Setup2_GetUPSAP_Enabled() As Boolean
    '    Dim sql2exe As String = "SELECT [Enable_UPS_AP] FROM [Setup2] WHERE [ID] > 0"
    '    Dim val As Long
    '    If get_dreader_onevalue(sql2exe, val) Then
    '        Return _Convert.Long2Boolean(val)
    '    End If
    'End Function
    'Public Function Setup2_GetFedExHAL_Enabled() As Boolean
    '    Dim sql2exe As String = "SELECT [Enable_FedEx_HAL] FROM [Setup2] WHERE [ID] > 0"
    '    Dim val As Long
    '    If get_dreader_onevalue(sql2exe, val) Then
    '        Return _Convert.Long2Boolean(val)
    '    End If
    'End Function
    'Public Function Setup2_GetFedExHAL_IDs(ByRef locationID As String, ByRef agentID As String) As Boolean
    '    locationID = String.Empty ' assume.
    '    agentID = String.Empty ' assume.
    '    Dim dreader As OleDb.OleDbDataReader = Nothing
    '    Dim sql2exe As String = "SELECT [FedExHAL_LocationID], [FedExHAL_AgentID] FROM [Setup2] WHERE [ID] > 0"
    '    Setup2_GetFedExHAL_IDs = get_dreader(sql2exe, dreader, True)
    '    If Setup2_GetFedExHAL_IDs Then
    '        locationID = _Convert.Null2DefaultValue(dreader("FedExHAL_LocationID"))
    '        agentID = _Convert.Null2DefaultValue(dreader("FedExHAL_AgentID"))
    '    End If
    '    ShipRiteDb.Close_dreader(dreader)
    'End Function
    Public Function Setup_Get_DHL_ShipperNo() As String
        Setup_Get_DHL_ShipperNo = String.Empty ' assume.
        Dim shipperNo As String = String.Empty
        'Dim sql2exe As String = "Select [ABOrigin] From Setup Where [ID] > 0" ''AP(11/03/2016){DRN = 1110} - DHL Account# pulled from wrong field for DHL DropOff.
        'shipperNo = GetNextSegmentFromSet(IO_GetSegmentSet(path2db, sql2exe))
        shipperNo = GetPolicyData(gShipriteDB, "ABOrigin", "")
        Console.WriteLine(shipperNo)
        Return shipperNo
    End Function

    Public Function Setup_Get_UPS_AccessPointId() As String
        Dim shipperNo As String = String.Empty ' assume.
        shipperNo = GetPolicyData(gShipriteDB, "UPS_AccessID", "")
        Console.WriteLine(shipperNo)
        Return shipperNo
    End Function

#End Region

End Module

Public Module EmailNotificationsDb

    Public Property Path2db As String

    Public Function Read_Notifications(ByRef segment As String) As Boolean
        Dim sql2exe As String = "Select * From Notifications"
        segment = GetNextSegmentFromSet(IO_GetSegmentSet(Path2db, sql2exe))
        Read_Notifications = Not String.IsNullOrEmpty(segment)
    End Function
    Public Function Read_Notification(ByVal filename As String, ByRef segment As String) As Boolean
        Dim sql2exe As String = String.Format("Select * From Notifications Where [FileName]='{0}'", filename)
        segment = GetNextSegmentFromSet(IO_GetSegmentSet(Path2db, sql2exe))
        Read_Notification = Not String.IsNullOrEmpty(segment)
    End Function

End Module

#Region "Bar Code"
Public Module BarCode
    Public Function ShippingCo(ByRef trackingNumScanned As String) As String
        Dim C1 As Integer
        Dim C2 As Integer
        Dim C3 As Integer
        Dim C4 As Integer
        Dim C5 As Integer
        Dim C6 As Integer
        Dim C7 As Integer
        Dim C8 As Integer
        Dim C9 As Integer
        Dim C10 As Integer
        Dim C11 As Integer
        Dim C12 As Integer
        Dim C13 As Integer
        Dim C14 As Integer
        Dim C15 As Integer
        Dim C16 As Integer
        Dim C17 As Integer
        Dim C18 As Integer
        Dim C19 As Integer
        Dim C20 As Integer
        Dim C21 As Integer
        Dim C22 As Integer
        Dim C23 As Integer
        Dim C24 As Integer
        Dim C25 As Integer
        Dim C26 As Integer

        Dim F1 As Integer
        Dim F2 As Integer
        Dim F3 As Integer
        Dim F4 As Integer
        Dim F5 As Integer
        Dim F6 As Integer
        Dim F7 As Integer
        Dim F8 As Integer
        Dim F9 As Integer
        Dim F10 As Integer
        Dim F11 As Integer
        Dim F12 As Integer
        Dim F13 As Integer
        'Dim F14 As Integer
        'Dim F15 As Integer
        'Dim F16 As Integer
        'Dim F17 As Integer
        'Dim F18 As Integer
        'Dim F19 As Integer
        'Dim F20 As Integer
        'Dim F21 As Integer
        'Dim F22 As Integer
        'Dim F23 As Integer
        'Dim F24 As Integer
        Try
            Dim TrackingExtract As String
            Dim TrackingSum As Long
            Dim CheckDigit As Long

            ShippingCo = String.Empty ' assume.

            If _Controls.Left(trackingNumScanned, 2) = "1Z" Then
                ''ol#1.2.38(5/17)... Improved UPS tracking# Ground vs. Air recognition.
                ' TRACKING #: 1Z XXX XXX YY ZZZZ ZZZC
                ' X = Shipper 's Account # (6 Digits)
                ' Y = Service Code (2 Digits) 
                ' Z = Shipper 's Reference # (Can be set by shipper for convenience, to mirror an invoice#, etc.)
                ' C = Check Digit
                Select Case _Controls.Mid(trackingNumScanned, 9, 2, True)
                    Case "03", "20", "22", "42", "72", "78", "90", "A8"
                        Return "UPS Ground"
                    Case Else
                        Return "UPS Express"
                End Select

            ElseIf "SP" = _Controls.Left(trackingNumScanned, 2) Then
                Return "SpeeDee"
            ElseIf 10 = trackingNumScanned.Length Then
                Return "DHL"

            ElseIf trackingNumScanned.Length = 30 Then
                ''ol#1.2.68(1/16)... USPS tracking# of 30 chars in length needed to be truncated.
                trackingNumScanned = _Controls.Right(trackingNumScanned, 22)
                Return "USPS"
            ElseIf trackingNumScanned.Length = 34 And "42" = _Controls.Left(trackingNumScanned, 2) Then
                Return "USPS"

                ''ol#1.2.23(11/5)... FedEx tracking# containing 22 numbers in length is not being auto-detected correctly.
            ElseIf trackingNumScanned.Length = 22 And "96" = _Controls.Left(trackingNumScanned, 2) Then
                ''ol#1.2.38(5/17)... If FedEx number is 22 chars in length then tracking# is 15 digit long at the end.
                trackingNumScanned = _Controls.Right(trackingNumScanned, 15)
                Return "FedEx Ground"
            ElseIf trackingNumScanned.Length = 22 And "10" = _Controls.Left(trackingNumScanned, 2) Then
                ''ol#1.2.38(5/17)... If FedEx number is 22 chars in length then tracking# is 15 digit long at the end.
                trackingNumScanned = _Controls.Right(trackingNumScanned, 15)
                Return "FedEx Express"
            ElseIf trackingNumScanned.Length = 22 And "90" = _Controls.Left(trackingNumScanned, 2) Then
                ''ol#1.2.38(5/17)... If FedEx number is 22 chars in length then tracking# is 15 digit long at the end.
                trackingNumScanned = _Controls.Right(trackingNumScanned, 15)
                Return "FedEx Express"
            ElseIf trackingNumScanned.Length = 22 And "92" = _Controls.Left(trackingNumScanned, 2) Then
                ''ol#1.2.38(5/17)... If FedEx number is 22 chars in length then tracking# is 15 digit long at the end.
                trackingNumScanned = _Controls.Right(trackingNumScanned, 15)
                Return "FedEx Express"
            ElseIf trackingNumScanned.Length = 21 And "92" = _Controls.Left(trackingNumScanned, 2) Then
                ''ol#1.2.39(5/23)... If FedEx number is 21 chars in lengthand first two "92" or "90" then tracking# is 15 digit long at the end.
                trackingNumScanned = _Controls.Right(trackingNumScanned, 15)
                Return "FedEx Express"
            ElseIf trackingNumScanned.Length = 21 And "90" = _Controls.Left(trackingNumScanned, 2) Then
                ''ol#1.2.39(5/23)... If FedEx number is 21 chars in length and first two "90" or "90" then tracking# is 15 digit long at the end.
                trackingNumScanned = _Controls.Right(trackingNumScanned, 15)
                Return "FedEx Express"

            ElseIf trackingNumScanned.Length = 34 Then
                If "96" = _Controls.Left(trackingNumScanned, 2) Then
                    trackingNumScanned = _Controls.Right(trackingNumScanned, 12)
                    Return "FedEx Ground"
                Else
                    trackingNumScanned = _Controls.Right(trackingNumScanned, 12)
                    Return "FedEx Express"
                End If
            ElseIf trackingNumScanned.Length = 16 Then
                ''ol#1.2.24(11/16)... FedEx tracking# containing 16 numbers in length has last 4 digits as FormID.
                trackingNumScanned = _Controls.Left(trackingNumScanned, 12)
                Return "FedEx Express"
            End If


            If trackingNumScanned.Length < 13 Then
                GoTo FedexFigure
            End If

            Dim USPS22_ As Long

            If trackingNumScanned.Length = 13 Then GoTo USPS13

            If trackingNumScanned.Length = 30 Then GoTo USPS22

            'If trackingNumScanned.Length = 22 Then GoTo FedEx22
            If trackingNumScanned.Length = 22 Then GoTo FedexFigure

            If trackingNumScanned.Length = 32 Then GoTo FedEx32

            If trackingNumScanned.Length = 16 Then GoTo FedEx16

            If trackingNumScanned.Length = 34 Then GoTo FedexFigure


FedexFigure:
            'MsgBox TrackingNumScanned

            TrackingExtract = _Controls.Right(trackingNumScanned, 26)
            _Debug.Print_(TrackingExtract)
            'MsgBox (TrackingExtract)
            C1 = _Controls.Mid(TrackingExtract, 1, 1, True)
            C2 = _Controls.Mid(TrackingExtract, 2, 1, True)
            C3 = _Controls.Mid(TrackingExtract, 3, 1, True)
            C4 = _Controls.Mid(TrackingExtract, 4, 1, True)
            C5 = _Controls.Mid(TrackingExtract, 5, 1, True)
            C6 = _Controls.Mid(TrackingExtract, 6, 1, True)
            C7 = _Controls.Mid(TrackingExtract, 7, 1, True)
            If trackingNumScanned.Length < 8 Then
                C8 = 0
            Else
                C8 = _Controls.Mid(TrackingExtract, 8, 1, True)
            End If
            If trackingNumScanned.Length < 9 Then
                C9 = 0
            Else
                C9 = _Controls.Mid(TrackingExtract, 9, 1, True)
            End If
            If trackingNumScanned.Length < 10 Then
                C10 = 0
            Else
                C10 = _Controls.Mid(TrackingExtract, 10, 1, True)
            End If
            If trackingNumScanned.Length < 11 Then
                C11 = 0
            Else
                C11 = _Controls.Mid(TrackingExtract, 11, 1, True)
            End If
            If trackingNumScanned.Length < 12 Then
                C12 = 0
            Else
                C12 = _Controls.Mid(TrackingExtract, 12, 1, True)
            End If
            If trackingNumScanned.Length < 13 Then
                C13 = 0
            Else
                C13 = _Controls.Mid(TrackingExtract, 13, 1, True)
            End If
            If trackingNumScanned.Length < 14 Then
                C14 = 0
            Else
                C14 = _Controls.Mid(TrackingExtract, 14, 1, True)
            End If
            If trackingNumScanned.Length < 15 Then
                C15 = 0
            Else
                C15 = _Controls.Mid(TrackingExtract, 15, 1, True)
            End If
            If trackingNumScanned.Length < 16 Then
                C16 = 0
            Else
                C16 = _Controls.Mid(TrackingExtract, 16, 1, True)
            End If
            If trackingNumScanned.Length < 17 Then
                C17 = 0
            Else
                C17 = _Controls.Mid(TrackingExtract, 17, 1, True)
            End If
            If trackingNumScanned.Length < 18 Then
                C18 = 0
            Else
                C18 = _Controls.Mid(TrackingExtract, 18, 1, True)
            End If
            If trackingNumScanned.Length < 19 Then
                C19 = 0
            Else
                C19 = _Controls.Mid(TrackingExtract, 19, 1, True)
            End If
            If trackingNumScanned.Length < 20 Then
                C20 = 0
            Else
                C20 = _Controls.Mid(TrackingExtract, 20, 1, True)
            End If
            If trackingNumScanned.Length < 21 Then
                C21 = 0
            Else
                C21 = _Controls.Mid(TrackingExtract, 21, 1, True)
            End If
            If trackingNumScanned.Length < 22 Then
                C22 = 0
            Else
                C22 = _Controls.Mid(TrackingExtract, 22, 1, True)
            End If
            If trackingNumScanned.Length < 23 Then
                C23 = 0
            Else
                C23 = _Controls.Mid(TrackingExtract, 23, 1, True)
            End If
            If trackingNumScanned.Length < 24 Then
                C24 = 0
            Else
                C24 = _Controls.Mid(TrackingExtract, 24, 1, True)
            End If
            If trackingNumScanned.Length < 25 Then
                C25 = 0
            Else
                C25 = _Controls.Mid(TrackingExtract, 25, 1, True)
            End If
            If trackingNumScanned.Length < 26 Then
                C26 = 0
            Else
                C26 = _Controls.Mid(TrackingExtract, 26, 1, True)
            End If


            'USPS check number
            Dim U1 As Integer = C26
            Dim U2 As Integer = C25
            Dim U3 As Integer = C24
            Dim U4 As Integer = C23
            Dim U5 As Integer = C22
            Dim U6 As Integer = C21
            Dim U7 As Integer = C20
            Dim U8 As Integer = C19
            Dim U9 As Integer = C18
            Dim U10 As Integer = C17
            Dim U11 As Integer = C16
            Dim U12 As Integer = C15
            Dim U13 As Integer = C14
            Dim U14 As Integer = C13
            Dim U15 As Integer = C12
            Dim U16 As Integer = C11
            Dim U17 As Integer = C10
            Dim U18 As Integer = C9
            Dim U19 As Integer = C8
            Dim U20 As Integer = C7
            Dim U21 As Integer = C6
            Dim U22 As Integer = C5
            Dim U23 As Integer = C4
            Dim U24 As Integer = C3
            Dim U25 As Integer = C2
            Dim U26 As Integer = C1

            'FedEx Mod Check
            F1 = C13 * 1
            F2 = C14 * 7
            F3 = C15 * 3
            F4 = C16 * 1
            F5 = C17 * 7
            F6 = C18 * 3
            F7 = C19 * 1
            F8 = C20 * 7
            F9 = C21 * 3
            F10 = C22 * 1
            F11 = C23 * 7
            F12 = C24 * 3
            F13 = C25 * 1

            Dim USPS26checkDigit As Long
            Dim USPS22checkDigit As Long
            Dim USPSeven26 As Long
            Dim USPSodd26 As Long
            Dim USPSodd22 As Long
            Dim USPS26 As Long
            Dim USPS26rd As Long
            Dim USPS22rd As Long
            Dim USPSeven22 As Long



            'USPS Check
            USPSeven26 = (U2 + U4 + U6 + U8 + U10 + U12 + U14 + U16 + U18 + U20 + U22 + U24 + U26) * 3
            USPSodd26 = (U3 + U5 + U7 + U9 + U11 + U13 + U15 + U17 + U19 + U21 + U23 + U25)
            USPS26 = USPSeven26 + USPSodd26
            USPS26rd = (USPS26 / 10) * 10
            USPS26checkDigit = USPS26rd - USPS26
            'MsgBox "USPS26 Check Digit" & " " & USPS26CheckDigit

            USPSeven22 = (U2 + U4 + U6 + U8 + U10 + U12 + U14 + U16 + U18 + U20 + U22) * 3

            USPSodd22 = (U3 + U5 + U7 + U9 + U11 + U13 + U15 + U17 + U19 + U21)
            USPS22_ = USPSeven22 + USPSodd22
            USPS22rd = -10 * Int(-USPS22_ / 10)
            USPS22checkDigit = USPS22rd - USPS22_



            'Fedex Check
            TrackingSum = (F1 + F2 + F3 + F4 + F5 + F6 + F7 + F8 + F9 + F10 + F11 + F12 + F13)
            CheckDigit = TrackingSum Mod 11
            If CheckDigit = 10 Then CheckDigit = 0


            Dim MyNameIs As String = String.Empty

            If CheckDigit = USPS22checkDigit Then

                If MyNameIs = "Fedex34" Then GoTo Fedex
                If MyNameIs = "USPS34" Then GoTo USPS221

            End If

            If CheckDigit = USPS26checkDigit Then

                If MyNameIs = "Fedex34" Then GoTo Fedex
                If MyNameIs = "USPS34" Then GoTo USPS26

            End If

            'FedEx";"Fedex34";"Post Office";"USPS34"


            If USPS22checkDigit = U1 Then GoTo USPS221
            If CheckDigit = C26 Then GoTo Fedex
            If USPS26checkDigit = U1 Then GoTo USPS26

            'MsgBox "I am at Fedex stop"
Fedex:
            If "96" = _Controls.Left(trackingNumScanned, 2) Then

                GoTo FedExGrnd
            Else
                GoTo FedeExExpress
            End If

USPS26:
            Return "USPS"

USPSstop:
            'MsgBox "I am at USPS stop"
USPS221:
            Return "USPS"

FedEx16:
            GoTo FedeExExpress

FedEx22:

            If "96" = _Controls.Left(trackingNumScanned, 2) Then
                GoTo FedExGrnd
            Else
                GoTo FedeExExpress
            End If

FedEx32:
            If "96" = _Controls.Left(trackingNumScanned, 2) Then
                GoTo FedExGrnd
            Else
                GoTo FedeExExpress
            End If

FedExGrnd:
            ''ol#16.05(3/14)... FedEx tracking# will be trancated to 12 digits only for the length of 34 and 16
            ''If 12 < trackingNumScanned.Length Then
            ''    trackingNumScanned = _Controls.Right(trackingNumScanned, 12)
            ''End If
            Return "FedEx Ground"

FedeExExpress:
            ''ol#16.05(3/14)... FedEx tracking# will be trancated to 12 digits only for the length of 34 and 16
            ''If 12 < trackingNumScanned.Length Then
            ''    trackingNumScanned = _Controls.Right(trackingNumScanned, 12)
            ''End If
            Return "FedEx Express"

USPS22:
            Return "USPS"

USPS:
            Return "USPS"
UPS:
            Return "UPS"

USPS13:
            Return "USPS"

NumberEntered:
        Catch ex As Exception
            Return Nothing
        End Try

    End Function


End Module
#End Region


Public Module _Carrier

    Public Function IsService_Exist_Master(ServiceABBR As String, path2db As String) As Boolean
        Dim sql2exe As String
        '
        sql2exe = "Select [Service] From [Master] Where [Service] = '" & ServiceABBR & "'"
        Dim SegmentSet As String = DatabaseFunctions.IO_GetSegmentSet(gShipriteDB, sql2exe)
        Return Not String.IsNullOrEmpty(SegmentSet)
    End Function

    Public Function IsService_Exist_Panel(ServiceABBR As String, path2db As String) As Boolean
        Dim sql2exe As String
        '
        sql2exe = "Select * From [Panel] Where [SV1] = '" & ServiceABBR & "' Or [SV2] = '" & ServiceABBR & "' Or [SV3] = '" & ServiceABBR & "' Or [SV4] = '" & ServiceABBR & "' Or [SV5] = '" & ServiceABBR & "' Or [SV6] = '" & ServiceABBR & "' Or [SV7] = '" & ServiceABBR & "'"
        Dim SegmentSet As String = DatabaseFunctions.IO_GetSegmentSet(gShipriteDB, sql2exe)
        Return Not String.IsNullOrEmpty(SegmentSet)
    End Function
    Public Function IsMessage_Exist_Panel(MessageABBR As String, path2db As String) As Boolean
        ''AP(11/01/2018) - Updated startup to create FedEx Express & LTL Freight services and panel if don't exist.
        IsMessage_Exist_Panel = False ' assume.
        Dim sql2exe As String
        '
        If Len(Trim(MessageABBR)) > 0 Then
            sql2exe = "Select * From [Panel] Where [Message] = '" & MessageABBR & "'"
            Dim SegmentSet As String = DatabaseFunctions.IO_GetSegmentSet(gShipriteDB, sql2exe)
            Return Not String.IsNullOrEmpty(SegmentSet)
        End If
    End Function

    Public Function IsService_ZoneTableExist(ServiceABBR As String, path2db As String) As Boolean
        Dim sql2exe As String
        '
        sql2exe = "Select Top 1 [ZONE] From [" & ServiceABBR & "]"
        Dim SegmentSet As String = DatabaseFunctions.IO_GetSegmentSet(gShipriteDB, sql2exe)
        Return Not String.IsNullOrEmpty(SegmentSet)
    End Function
    Public Function IsService_ServiceTableExist(ServiceABBR As String, path2db As String) As Boolean
        Dim sql2exe As String
        '
        sql2exe = "Select Top 1 [LBS] From [" & ServiceABBR & "]"
        Dim SegmentSet As String = DatabaseFunctions.IO_GetSegmentSet(gShipriteDB, sql2exe)
        Return Not String.IsNullOrEmpty(SegmentSet)
    End Function

    Public Function IsService_Added(serviceMasterDesc As String, servicePanelDesc As String, ServiceABBR As String, carrierName As String, copyServiceABBR As String, path2db As String, path2zones As String, path2services As String) As Boolean
        Dim ok2master As Boolean
        Dim ok2panels As Boolean
        ''
        If isService_SafeToAdd(ServiceABBR, carrierName, path2db, path2zones, path2services) Then
            If vbOK = MsgBox("Would you like to add " & serviceMasterDesc & " '" & ServiceABBR & "' service to your Shiprite ?", vbOKCancel + vbQuestion, "New " & carrierName & " Service is Available!") Then
                ''
                ok2master = IsService_Exist_Master(ServiceABBR, path2db)
                If Not ok2master Then
                    ''
                    ok2master = add_Service2Master(serviceMasterDesc, ServiceABBR, carrierName, copyServiceABBR, path2db)
                    ''
                End If
                ''
                ok2panels = IsService_Exist_Panel(ServiceABBR, path2db)
                If Not ok2panels Then
                    ''
                    ok2panels = add_Service2NewPanel(servicePanelDesc, ServiceABBR, carrierName, copyServiceABBR, path2db)
                    ''
                End If
                ''
            End If
        End If
        ''
        IsService_Added = (ok2master And ok2panels)
    End Function
    Private Function isService_SafeToAdd(ServiceABBR As String, carrierName As String, path2db As String, path2zones As String, path2services As String) As Boolean
        isService_SafeToAdd = False ' assume.
        '
        If IsService_ZoneTableExist(ServiceABBR, path2zones) Then
            ''
            If IsService_ServiceTableExist(ServiceABBR, path2services) Then
                ''
                isService_SafeToAdd = True
                ''
            Else
                ''
                MsgBox("You need the new " & carrierName & " '" & ServiceABBR & "' service table to be added to your " & carrierName & " Services database to continue." & vbCr & vbCr &
                       "Please contact Shiprite Support to update your " & carrierName & " Services database...", vbInformation, "Cannot Add the Service")
                ''
            End If
            ''
        Else
            ''
            MsgBox("You need the new " & carrierName & " '" & ServiceABBR & "' service table to be added to your " & carrierName & " Zones database to continue." & vbCr & vbCr &
                   "Please contact Shiprite Support to update your " & carrierName & " Zones database...", vbInformation, "Cannot Add the Service")
            ''
        End If
        ''
    End Function

    Public Function add_Service2Master(serviceMasterDesc As String, ServiceABBR As String, carrierName As String, copyServiceABBR As String, path2db As String) As Boolean
        add_Service2Master = False ' assume.
        '
        Dim sql2exe As String = "Select Master.* From [Master] Where Service = '" & copyServiceABBR & "'"
        Dim SegmentSet As String = DatabaseFunctions.IO_GetSegmentSet(gShipriteDB, sql2exe)
        Dim fields As String = DatabaseFunctions.IO_GetFieldsCollection(gShipriteDB, "Master", String.Empty, True, False, True)
        '_Debug.Print_(fields)

        Dim splitSegment() As String = fields.Split(Chr(171) & Chr(187))
        Dim sql2cmd As New sqlINSERT
        If Not String.IsNullOrEmpty(SegmentSet) Then
            '
            Call sql2cmd.Qry_INSERT("CARRIER", carrierName, sql2cmd.TXT_, True, False, "Master")
            Call sql2cmd.Qry_INSERT("SERVICE", ServiceABBR, sql2cmd.TXT_)
            '
            Dim Segment As String = GetNextSegmentFromSet(SegmentSet)
            '_Debug.Print_(Segment)
            For i As Int16 = 0 To splitSegment.GetUpperBound(0)
                '
                Dim fieldname_type As String = splitSegment(i).Replace(Chr(171), "").Replace(Chr(187), "").Trim
                If Not String.IsNullOrEmpty(fieldname_type) Then
                    '
                    Dim iloc As Int16 = InStr(1, fieldname_type, ".")
                    If iloc > 0 Then
                        Dim fieldname As String = Trim(Strings.Mid(fieldname_type, 1, iloc - 1))
                        Dim fieldtype As Int16 = CInt(Trim(Strings.Mid(fieldname_type, iloc + 1)))

                        Dim fieldvalue As String = ExtractElementFromSegment(fieldname, Segment)
                        If Not String.IsNullOrEmpty(fieldvalue) Then
                            '
                            Select Case UCase(fieldname)
                                ' field values that are different:
                                Case "ID"
                                Case "CARRIER"
                                Case "SERVICE"
                                Case "DESCRIPTION"
                                Case Else
                                    ' field values that are the same.
                                    Select Case fieldtype
                                        Case 0, 2, 4, 5, 6, 3, 11 : Call sql2cmd.Qry_INSERT(fieldname, fieldvalue, sql2cmd.NUM_)
                                        Case 7 : Call sql2cmd.Qry_INSERT(fieldname, fieldvalue, sql2cmd.DTE_)
                                        Case Else : Call sql2cmd.Qry_INSERT(fieldname, fieldvalue, sql2cmd.TXT_)
                                    End Select
                                    '
                            End Select
                            '
                        End If
                    End If
                    '
                End If
            Next
            '
            sql2exe = sql2cmd.Qry_INSERT("DESCRIPTION", serviceMasterDesc, sql2cmd.TXT_, False, True)
            '_Debug.Print_(sql2exe)
            add_Service2Master = Not (-1 = IO_UpdateSQLProcessor(gShipriteDB, sql2exe))
            '
        End If
        ''
    End Function
    Private Function add_NewFedEx_Freight_Service2Master(serviceMasterDesc As String, ServiceABBR As String, carrierName As String, path2db As String) As Boolean
        Dim sql2exe As String
        Dim sql2cmd As New sqlINSERT
        ''
        ''ol#18.04(10/2)... Create LTL Freight from scratch if FR1 doesn't exist.
        Call sql2cmd.Qry_INSERT("Carrier", carrierName, sql2cmd.TXT_, True, False, "Master")
        Call sql2cmd.Qry_INSERT("Type", carrierName, sql2cmd.TXT_)
        Call sql2cmd.Qry_INSERT("SERVICE", ServiceABBR, sql2cmd.TXT_)
        Call sql2cmd.Qry_INSERT("ZONE-TBL", "FEDEX48", sql2cmd.TXT_)
        Call sql2cmd.Qry_INSERT("DESCRIPTION", serviceMasterDesc, sql2cmd.TXT_)
        Call sql2cmd.Qry_INSERT("PosDept", carrierName, sql2cmd.TXT_)
        Call sql2cmd.Qry_INSERT("SaturdayDelivery", "-1", sql2cmd.NUM_)
        Call sql2cmd.Qry_INSERT("SaturdayPickup", "-1", sql2cmd.NUM_)
        Call sql2cmd.Qry_INSERT("FreightService", "-1", sql2cmd.NUM_)
        Call sql2cmd.Qry_INSERT("ResidentialService", "0", sql2cmd.NUM_)
        sql2exe = sql2cmd.Qry_INSERT("Disabled", "0", sql2cmd.NUM_, False, True)
        Return Not (-1 = IO_UpdateSQLProcessor(gShipriteDB, sql2exe))
        ''
    End Function
    Private Function add_FedExFreight_Domestic_Service2NewPanel(servicePanelDesc As String, ServiceABBR As String, carrierName As String, path2db As String) As Boolean
        add_FedExFreight_Domestic_Service2NewPanel = False ' assume.
        '
        Dim sql2exe As String
        Dim nextPIndex As Long
        Dim sql2cmd As New sqlINSERT
        ''
        sql2exe = "Select PanelIndex From [Panel] Order By PanelIndex DESC"
        Dim SegmentSet As String = DatabaseFunctions.IO_GetSegmentSet(gShipriteDB, sql2exe)
        If Not String.IsNullOrEmpty(SegmentSet) Then
            ''
            nextPIndex = Val(ExtractElementFromSegment("PanelIndex", SegmentSet))
            nextPIndex = nextPIndex + 1 '' this is our next available panel index
            ''
            Call sql2cmd.Qry_INSERT("PanelIndex", CStr(nextPIndex), sql2cmd.NUM_, True, , "Panel")
            Call sql2cmd.Qry_INSERT("BD1", servicePanelDesc, sql2cmd.TXT_)
            Call sql2cmd.Qry_INSERT("SV1", ServiceABBR, sql2cmd.TXT_)
            Call sql2cmd.Qry_INSERT("Domestic", "1", sql2cmd.NUM_)
            Call sql2cmd.Qry_INSERT("Icon", "1", sql2cmd.NUM_)
            Call sql2cmd.Qry_INSERT("Message", "Freight", sql2cmd.TXT_)
            Call sql2cmd.Qry_INSERT("ForeColor", "16777215", sql2cmd.NUM_)
            sql2exe = sql2cmd.Qry_INSERT("BackColor", "8388608", sql2cmd.NUM_, , True)
            Return Not (-1 = IO_UpdateSQLProcessor(gShipriteDB, sql2exe))
            ''
        End If
        '
    End Function
    Private Function add_Service2NewPanel(servicePanelDesc As String, ServiceABBR As String, carrierName As String, copyServiceABBR As String, path2db As String) As Boolean
        add_Service2NewPanel = False ' assume.
        '
        Dim nextPIndex As Long
        Dim sql2exe As String = "Select PanelIndex From [Panel] Order By PanelIndex DESC"
        Dim sql2cmd As New sqlINSERT
        '
        Dim SegmentSet As String = DatabaseFunctions.IO_GetSegmentSet(gShipriteDB, sql2exe)
        If Not String.IsNullOrEmpty(SegmentSet) Then
            ''
            nextPIndex = Val(ExtractElementFromSegment("PanelIndex", SegmentSet))
            nextPIndex = nextPIndex + 1 '' this is our next available panel index
            ''
            Call sql2cmd.Qry_INSERT("PanelIndex", CStr(nextPIndex), sql2cmd.NUM_, True, , "Panel")
            Call sql2cmd.Qry_INSERT("BD1", servicePanelDesc, sql2cmd.TXT_)
            Call sql2cmd.Qry_INSERT("SV1", ServiceABBR, sql2cmd.TXT_)
            Call sql2cmd.Qry_INSERT("Domestic", "1", sql2cmd.NUM_)
            Call sql2cmd.Qry_INSERT("Icon", "4", sql2cmd.NUM_)
            Call sql2cmd.Qry_INSERT("Message", "Domestic", sql2cmd.TXT_)
            Call sql2cmd.Qry_INSERT("ForeColor", "0", sql2cmd.NUM_) '' black
            sql2exe = sql2cmd.Qry_INSERT("BackColor", CStr(Val(&H8000000F)), sql2cmd.NUM_, , True) '' grey
            Return Not (-1 = IO_UpdateSQLProcessor(gShipriteDB, sql2exe))
            ''
        End If
        '
    End Function
    Private Function add_Service2Panel(servicePanelDesc As String, ServiceABBR As String, carrierName As String, copyServiceABBR As String, path2db As String, Optional ByVal copyMessageABBR As String = "") As Boolean
        add_Service2Panel = False ' assume.
        '
        Dim i%
        Dim sql2cmd As New sqlUpdate
        Dim sql2exe As String = "Select * From [Panel] Where SV1 = '" & copyServiceABBR & "' Or SV2 = '" & copyServiceABBR & "' Or SV3 = '" & copyServiceABBR & "' Or SV4 = '" & copyServiceABBR & "' Or SV5 = '" & copyServiceABBR & "' Or SV6 = '" & copyServiceABBR & "' Or SV7 = '" & copyServiceABBR & "'"
        'AP(11/01/2018) - Updated startup to create FedEx Express & LTL Freight services and panel if don't exist.
        If Len(Trim(copyMessageABBR)) > 0 Then
            sql2exe = sql2exe & " Or [Message] = '" & copyMessageABBR & "'"
        End If
        Dim SegmentSet As String = DatabaseFunctions.IO_GetSegmentSet(gShipriteDB, sql2exe)
        If Not String.IsNullOrEmpty(SegmentSet) Then
            '
            ' AP(11/01/2018) - Updated startup to create FedEx Express & LTL Freight services and panel if don't exist.
            ' we have total of 7 buttons to check for empty one
            For i% = 1 To 7
                '
                If String.IsNullOrEmpty(ExtractElementFromSegment("SV" & CStr(i%), SegmentSet)) Then
                    '
                    Dim panelindex As Int16 = Val(ExtractElementFromSegment("PanelIndex", SegmentSet))
                    Call sql2cmd.Qry_UPDATE("BD" & CStr(i%), servicePanelDesc, sql2cmd.TXT_, True, False, "Panel", "PanelIndex = " & panelindex)
                    sql2exe = sql2cmd.Qry_UPDATE("SV" & CStr(i%), ServiceABBR, sql2cmd.TXT_, False, True)
                    add_Service2Panel = Not (-1 = IO_UpdateSQLProcessor(gShipriteDB, sql2exe)) '' added successfully.
                    Exit For
                    '
                End If
                '
            Next i%
            '
        End If
        '
    End Function

    Public Function IsFreight_Added_ToExistingPanel(serviceMasterDesc As String, servicePanelDesc As String, ServiceABBR As String, carrierName As String, copyServiceABBR As String, path2db As String) As Boolean
        Dim ok2master As Boolean

        ''
        ok2master = IsService_Exist_Master(ServiceABBR, path2db)
        If Not ok2master Then
            ''
            ''ol#18.04(10/2)... Create LTL Freight from scratch if FR1 doesn't exist.
            If IsService_Exist_Master(copyServiceABBR, path2db) Then
                ok2master = add_Service2Master(serviceMasterDesc, ServiceABBR, carrierName, copyServiceABBR, path2db)
            Else
                _Debug.Stop_("Create Freight Button from scratch...")
                ok2master = add_NewFedEx_Freight_Service2Master(serviceMasterDesc, ServiceABBR, carrierName, path2db)
            End If
            ''
        End If
        ''

        ''
        IsFreight_Added_ToExistingPanel = ok2master
    End Function

End Module

Public Class gShip_Class

    Private m_ContentsID As Long
    Private m_Contents As String
    Private m_PackagingCharge As Double
    Private m_PackagingCost As Double
    Private m_PackagingWeight As Double
    Private m_actualWeight As Double
    Private m_DecVal As Double
    Private m_Length As Long
    Private m_Width As Long
    Private m_Height As Long
    Public ShipAndInsureCost As Double
    Public ContentsDesc As String
    Public PrintSummaryOnly As Boolean
    Public Tax As Double
    Public DefaultCountyName As String
    Public DefaultPieceCharge As Double
    Public PiecesNumber As Integer
    Public PackagingType As String
    Public PackageID As String
    Public TrackingNumber As String
    Public Residential As Boolean
    Public Domestic As Boolean
    Public ServiceABBR As String
    Public Country As String
    Public SaturdayDelivery As Boolean
    Public SaturdayPickUp As Boolean
    Public NonStandardContainer As Boolean
    Public DryIceValue As Double
    Public SignatureType As Integer
    Public HOMEFedEXDeliveryDate As String
    Public HoldAtLocationID As String
    Public TestShipment As Boolean
    Public IsCertifiedMail As Boolean
    Public IsReturnReceipt As Boolean
    Public InsideDelivery As Boolean
    Public InsidePickup As Boolean
    Public FedEx_EmailNotification_Email As String
    Public Use_Store_Address As Boolean 'Used for Endicia to allow switching between store address and regular address

    Sub New()
        SignatureType = -1 ' assume no selection was made
    End Sub
    Public Property ContentsID As Long
        Get
            Return m_ContentsID
        End Get
        Set(value As Long)
            m_ContentsID = value
        End Set
    End Property
    Public Property Contents As String
        Get
            Return m_Contents
        End Get
        Set(value As String)
            m_Contents = value
        End Set
    End Property
    Public Property PackagingCharge As Double
        Get
            Return m_PackagingCharge
        End Get
        Set(value As Double)
            m_PackagingCharge = value
        End Set
    End Property
    Public Property PackagingCost As Double
        Get
            Return m_PackagingCost
        End Get
        Set(value As Double)
            m_PackagingCost = value
        End Set
    End Property
    Public Property PackagingWeight As Double
        Get
            Return m_PackagingWeight
        End Get
        Set(value As Double)
            m_PackagingWeight = value
        End Set
    End Property

    Public Property actualWeight As Double
        Get
            Return m_actualWeight
        End Get
        Set(value As Double)
            m_actualWeight = value
        End Set
    End Property
    Public Property DecVal As Double
        Get
            Return m_DecVal
        End Get
        Set(value As Double)
            m_DecVal = value
        End Set
    End Property

    Public Property Length As Long
        Get
            Return m_Length
        End Get
        Set(value As Long)
            m_Length = value
        End Set
    End Property
    Public Property Width As Long
        Get
            Return m_Width
        End Get
        Set(value As Long)
            m_Width = value
        End Set
    End Property
    Public Property Height As Long
        Get
            Return m_Height
        End Get
        Set(value As Long)
            m_Height = value
        End Set
    End Property

End Class
Public Class _CountryDB
    Public ListIndex As Integer
    Public CountryName As String
    Public CountryCode As String
    Public CountryCode3 As String
    Public CountryCurrency As String
    Public CountryCodeNumeric As String
    Public Overrides Function ToString() As String
        Return CountryName
    End Function

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub

End Class

#Region "Shipment Request"

Public Class _baseShipment
    Public Property ShipperContact As New _baseContact
    Public Property ShipFromContact As New _baseContact
    Public Property ShipToContact As New _baseContact
    Public Property HoldAtLocation As New _baseContact
    '
    Public Property CommInvoice As New _baseCommInvoice
    Public Property CarrierService As New _baseCarrierService
    Public Property Packages As New List(Of _baseShipmentPackage)
    Public Property DryIce As New _baseDryIce
    '
    Public Property RateRequestType As String
    Public Property PackageCount As Integer
    Public Property ShipmentNo As String
    Public Property Comments As String
    Public Property IsDocumentsOnly As Boolean
    '
    Public Property WebServicesResponse As New List(Of baseWebResponse_Shipment)
    '
End Class
Public Class _baseShipmentPackage
    '
    Sub New()
        If _IDs.IsMetricSystem Then
            Dim_Units = "CM"
            Weight_Units = "KG"
        Else
            Dim_Units = "IN"
            Weight_Units = "LB"
        End If
    End Sub
    Public Property PackageID As String
    Public Property Weight_LBs As Double
    Public Property Dim_Length As Double
    Public Property Dim_Height As Double
    Public Property Dim_Width As Double
    Public Property DeclaredValue As Double
    Public Property PackagingType As String
    '
    Public Property IsLetter As Boolean
    Public Property IsLargePackage As Boolean
    Public Property IsAdditionalHandling As Boolean
    Public Property DeliveryConfirmation As String
    Public Property COD As New _baseServiceSurchargeCOD
    Public Property DryIce As New _baseDryIce
    Public Property DangerousGoods As _baseDangerousGoods
    Public Property ServiceSurcharges As New List(Of _baseServiceSurcharge)
    '
    Public Property Weight_Units As String
    Public Property Currency_Type As String
    Public Property Dim_Units As String
    Public Property SequenceNo As Integer
    Public Property TrackingNo As String
    '
    Public Freight As New _baseFreight ''ol#18.01(5/17)... FedEx Freight Box services were added.
End Class
Public Class _baseCarrierService
    '
    Public Property ShipDate As Date
    Public Property DeliveryDate As Date
    Public Property DeliveryDays As Integer
    '
    Public Property CarrierName As String
    Public Property ServiceABBR As String
    Public Property ServiceZone As String
    Public Property IsAir As Boolean
    Public Property IsDomestic As Boolean
    '
    Public Property ServiceSurchargeCOD As New _baseServiceSurchargeCOD
    Public Property ServiceSurcharges As New List(Of _baseServiceSurcharge)
    '
End Class
Public Class _baseServiceSurcharge
    '
    Public Property Name As String
    Public Property ID As Long
    Public Property IDNote As String
    Public Property Description As String
    Public Property BaseCost As Double
    Public Property SellPrice As Double
    Public Property Discount As Double
    Public Property IsToShow As Boolean
    '
    Public Sub New()
        ID = 0
        Name = String.Empty
        Description = String.Empty
        SellPrice = 0
        BaseCost = 0
        Discount = 0
        IDNote = String.Empty
        IsToShow = False
    End Sub
    '
    Public Sub New(servID As Integer, servName As String, servDesc As String, servIsToShow As Boolean, Optional servBaseCost As Double = 0, Optional servSellPrice As Double = 0, Optional servDiscount As Double = 0, Optional servIDNote As String = "")
        ''
        ID = servID
        Name = servName
        Description = servDesc
        SellPrice = servSellPrice
        BaseCost = servBaseCost
        Discount = servDiscount
        IDNote = servIDNote
        IsToShow = servIsToShow
        ''
    End Sub
    '
End Class
Public Class _baseServiceSurchargeCOD
    '
    Public Property Amount As Double
    Public Property PaymentType As Integer
    Public Property ChargeType As Integer
    Public Property ReferenceID As String
    Public Property TinTypeID As String
    Public Property TinNoID As String
    Public Property AddCOD2Total As Boolean
    Public Property CurrencyType As String
    '
End Class
Public Class _baseFreight
    '
    Public Property IsPackagingListEnclosed As Boolean
    Public Property TotalShipmentPieces As Integer
    Public Property BookingConfirmationNo As String
    '
    Public LTL_Freight_Class As String
    Public LTL_Freight_Packaging As String
    Public LTL_Freight_Description As String
    Public LTL_Freight_TotalHandlingUnits As Integer
    '
    Public FreightFormItems As New List(Of FreightFormItem)
    Public FreightFormPaymentType As String
    '
    Sub New()
        LTL_Freight_Class = "CLASS 100"
        LTL_Freight_Packaging = "PALLET"
        LTL_Freight_Description = String.Empty
        LTL_Freight_TotalHandlingUnits = 0

        FreightFormPaymentType = "SENDER" ' Prepaid default
    End Sub
End Class
Public Class FreightFormItem
    Public HandlingUnits As Integer
    Public PackagingType As String
    Public PiecesNo As Integer
    Public Description As String
    Public PackageClass As String
    Public Weight As Double
    Public InsuredValue As Double
    Sub New()
        HandlingUnits = 1
        PackagingType = "PALLET"
        PiecesNo = 1
        Description = String.Empty
        PackageClass = "CLASS 100"
        Weight = 0
        InsuredValue = 0
    End Sub
End Class
Public Class _baseDryIce
    '
    Public Property Weight As Double
    Public Property WeightUnits As String
    '
End Class
Public Class _baseCommodities
    '
    Public Property Item_Code As String
    Public Property Item_CustomsValue As Double
    Public Property Item_Weight As Double
    Public Property Item_UnitPrice As Double
    Public Property Item_Quantity As Integer
    Public Property Item_Description As String
    Public Property Item_CountryOfOrigin As String
    Public Property Item_UnitsOfMeasure As String
    Public Property Item_WeightUnits As String
    '
End Class
Public Class _baseCommInvoice
    '
    Public Property InvoiceNo As String
    Public Property Comments As String
    Public Property IDTinType As String
    Public Property IDTinNo As String
    Public Property DutiesPaymentType As String
    Public Property TypeOfContents As String
    Public Property TermsOfSale As String
    Public Property FreightCharge As Double
    Public Property InsuranceCharge As Double
    Public Property TaxesOrMiscCharge As Double
    Public Property Freight As New List(Of _baseFreight)
    Public Property CustomsValue As Double
    Public Property DutiesChargeType As String
    Public Property DutiesPayorCountyCode As String
    Public Property B13AFilingOption As String
    Public Property CommoditiesList As New List(Of _baseCommodities)
    Public Property CurrencyType As String
    Public ReadOnly Property CommoditiesTotalValue() As Double
        Get
            CommoditiesTotalValue = 0 '' assume.
            Dim i As Short
            For i = 0 To CommoditiesList.Count - 1
                CommoditiesTotalValue = CommoditiesTotalValue + CommoditiesList.Item(i).Item_CustomsValue
            Next i
        End Get
    End Property
    '
End Class
Public Class _baseDangerousGoods
    '
    Public Property ID As String
    Public Property ProperShippingName As String
    Public Property ContainerType As String
    Public Property NumberOfContainers As Integer
    Public Property IsAccessible As Boolean
    Public Property Options As String
    Public Property CargoAircraftOnly As Boolean
    Public Property PackingGroup As String
    Public Property PackingInstructions As String
    Public Property HazardClass As String
    Public Property Quantity_Amount As Double
    Public Property Quantity_Units As String
    '
    Public Property Signatory_ContactName As String
    Public Property Signatory_Title As String
    Public Property Signatory_Place As String
    Public Property Signatory_EmergencyContactNumber As String
    '
End Class
Public Class _baseContact
    Public Property ContactID As Long
    Public Property CompanyName As String
    Public Property FName As String
    Public Property LName As String
    Public Property Addr1 As String
    Public Property Addr2 As String
    Public Property Addr3 As String
    Public Property City As String
    Public Property State As String
    Public Property Zip As String
    Public Property Province As String
    Public Property Country As String
    Public Property CountryCode As String
    Public Property Tel As String
    Public Property Fax As String
    Public Property Email As String
    Public Property Residential As Boolean
    Public Property AccountNumber As String
    Public Property CreatedOn As Date
    Public Property UniqueID As String
    Public Property IsConsignee As Boolean
    Public Property CellPhone As String
    Public Property CellDomain As String
    Public Property CellCarrier As String
    Public ReadOnly Property FNameLName() As String
        Get
            Dim tmpName As String = String.Format("{0} {1}", FName, LName)
            Return tmpName.Trim
        End Get
    End Property
    Public ReadOnly Property LNameFName() As String
        Get
            Dim tmpName As String = String.Format("{0}, {1}", LName, FName)
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
            Dim tmp As String = Me.Addr1
            If Me.Addr2 IsNot Nothing AndAlso Not 0 = Me.Addr2.Length Then
                tmp = String.Format("{0}{1}{2}", Me.Addr1, System.Environment.NewLine, Me.Addr2)
            End If
            Return String.Format("{0}{1}{2}, {3} {4}", tmp, System.Environment.NewLine, Me.City, Me.State, Me.Zip)
        End Get
    End Property

    Public Property CityStateZip() As String
        Get
            Return String.Format("{0}, {1} {2}", City, State, Zip)
        End Get
        Set(ByVal value As String)
        End Set
    End Property
End Class

#End Region

#Region "Shipment Response"
Public Class baseWebResponse_Shipment

    Public Property ShipmentID As String
    Public Property AdditionalInfo As String
    Public Property ServiceOptionsCharges As Double
    Public Property TransportationCharges As Double
    Public Property TotalCharges As Double
    '
    Public Property DeliveryDate As Date
    Public Property DeliveryDay As String
    '
    Public Property Packages As New List(Of baseWebResponse_Package)
    Public Property ShipmentAlerts As New List(Of String) ' of warning string messages
    '
    Public ReadOnly Property TotalWeight As Double
        Get
            TotalWeight = 0
            For i As Integer = 0 To MyClass.Packages.Count - 1
                Dim pack As baseWebResponse_Package = MyClass.Packages(i)
                TotalWeight += pack.PackageWeight
            Next
        End Get
    End Property

End Class
Public Class baseWebResponse_Package

    Public Property PackageID As String
    Public Property TrackingNo As String
    Public Property SequenceNo As Integer
    Public Property LabelImage As String
    Public Property LabelCODImage As String
    Public Property LabelCustomsImage As String

    Public Property ServiceCode As String
    Public Property PackageWeight As Integer
    Public Property Recipient As New _baseContact
    '
End Class

Public Class baseWebResponse_TinT_Services
    '
    Public Property TimeInTransitAlerts As New List(Of String) ' of warning string messages
    Public Property AvailableServices As New List(Of baseWebResponse_TinT_Service)
    '
End Class
Public Class baseWebResponse_TinT_Service
    '
    Public Property IsServiceAvailable As Boolean ' set to TRUE if returned by carrier web
    Public Property ServiceCode As String ' <ServiceType>FIRST_OVERNIGHT</ServiceType>
    Public Property ServiceDesc As String
    Public Property ArrivalDate As Date ' <CommitTimestamp>2013-06-03T08:00:00</CommitTimestamp>
    Public Property ArrivalDayOfWeek As String ' <DayOfWeek>MON</DayOfWeek>
    Public Property IsOnlyArrivalTransitTime As Boolean ' if set to TRUE then only <TransitTime> is available
    Public Property ArrivalTransitTime As String ' <TransitTime>FIVE_DAYS</TransitTime>
    '
    ''ol#1.2.69(5/8)... FedEx Freight Box services were added.
    Public TotalBaseCharge As Double
    Public TotalSurcharges As Double

End Class

#End Region


