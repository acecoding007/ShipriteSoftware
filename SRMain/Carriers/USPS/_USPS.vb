Imports System.IO

Module _USPS

    Private Class SettingItem
        Public Property SettingName As String
        Public Property SettingID As Integer
    End Class

    Public ReadOnly Property PriorityMail As String
        Get
            Return "USPS-PRI"
        End Get
    End Property
    Public ReadOnly Property ExpressMail As String
        Get
            Return "USPS-EXPR"
        End Get
    End Property
    Public ReadOnly Property FirstClassMail As String
        Get
            Return "FirstClass"
        End Get
    End Property
    Public ReadOnly Property StandardPost As String
        Get
            Return "USPS-4TH"
        End Get
    End Property
    Public ReadOnly Property Intl_GlobalExpressGuaranteed() As String
        Get
            Intl_GlobalExpressGuaranteed = "USPS-INTL-GXG" ''ol#4.00(4/29)... (New-USPS) new for Shiprite
        End Get
    End Property

    Public ReadOnly Property Intl_ExpressMailInternational() As String
        Get
            Intl_ExpressMailInternational = "USPS-INTL-EMI" ''ol#4.00(4/29)... (New-USPS) used to be "USPS-IEXP"
        End Get
    End Property
    Public ReadOnly Property Intl_ExpressMailInt_FlatRateZone() As String
        Get
            Intl_ExpressMailInt_FlatRateZone = "USPS-INTL-EMI-FLATR" ''ol#16.04(1/27)... New 'Service Type' (2-8 Price Groups) were added for International FlatRate packaging.
        End Get
    End Property
    Public ReadOnly Property Intl_PriorityMailInt_FlatRateZone() As String
        Get
            Intl_PriorityMailInt_FlatRateZone = "USPS-INTL-PMI-FLATR" ''ol#16.04(1/27)... New 'Service Type' (2-8 Price Groups) were added for International FlatRate packaging.
        End Get
    End Property

    Public ReadOnly Property Intl_ExpressMailInternational_MaxLbs() As String
        Get
            Intl_ExpressMailInternational_MaxLbs = "USPS-INTL-EMI_MaxLbs" ''ol#4.00(4/29)... (New-USPS) used to be "USPS-IEXP" Max Pounds allowed
        End Get
    End Property

    Public ReadOnly Property Intl_PriorityMailInternational() As String
        Get
            Intl_PriorityMailInternational = "USPS-INTL-PMI" ''ol#4.00(4/29)... (New-USPS) used to be "USPS-GPM", "USPS-PPAIR", "USPS-PPECON"
        End Get
    End Property

    Public ReadOnly Property Intl_PriorityMailInternational_MaxLbs() As String
        Get
            Intl_PriorityMailInternational_MaxLbs = "USPS-INTL-PMI_MaxLbs" ''ol#4.00(4/29)... (New-USPS) used to be "USPS-GPM", "USPS-PPAIR", "USPS-PPECON" Max Pounds allowed
        End Get
    End Property

    Public ReadOnly Property Intl_PriorityMailInternational_MaxIns() As String
        Get
            Intl_PriorityMailInternational_MaxIns = "USPS-INTL-PMI_MaxIns" ''ol#4.00(4/29)... (New-USPS) used to be "USPS-GPM", "USPS-PPAIR", "USPS-PPECON" Max Insurance allowed
        End Get
    End Property

    Public ReadOnly Property Intl_FirstClassMailInternational() As String
        Get
            Intl_FirstClassMailInternational = "USPS-INTL-FCMI" ''ol#4.00(4/29)... (New-USPS) used to be "USPS-LPAIR", "USPS-LPECON"
        End Get
    End Property

    Public ReadOnly Property Intl_FirstClassMailInternational_Letter() As String
        Get
            Intl_FirstClassMailInternational_Letter = "USPS-INTL-FCMI_Letter" ''ol#7.57(5/27)... USPS First Class Mail International service tables (USPS-INTL-FCMI_Letter and USPS-INTL-FCMI_Flats) were added for rates viewing to the Services screen.
        End Get
    End Property

    Public ReadOnly Property Intl_FirstClassMailInternational_Flats() As String
        Get
            Intl_FirstClassMailInternational_Flats = "USPS-INTL-FCMI_Flats" ''ol#7.57(5/27)... USPS First Class Mail International service tables (USPS-INTL-FCMI_Letter and USPS-INTL-FCMI_Flats) were added for rates viewing to the Services screen.
        End Get
    End Property

    Public ReadOnly Property Intl_GlobalExpressGuaranteed_Desc() As String
        Get
            Intl_GlobalExpressGuaranteed_Desc = "Global Express Guaranteed" ''ol#4.00(4/29)... (New-USPS) new for Shiprite
        End Get
    End Property

    Public ReadOnly Property Intl_GlobalExpressGuaranteed_DescShort() As String
        Get
            Intl_GlobalExpressGuaranteed_DescShort = "Global Exp Guarantd" ''ol#4.00(4/30)... (New-USPS) new for Shiprite
        End Get
    End Property

    Public ReadOnly Property Intl_ExpressMailInternational_Desc() As String
        Get
            Intl_ExpressMailInternational_Desc = "Express Mail International" ''ol#4.00(4/29)... (New-USPS) used to be "USPS-IEXP"
        End Get
    End Property

    Public ReadOnly Property Intl_ExpressMailInternational_DescShort() As String
        Get
            Intl_ExpressMailInternational_DescShort = "Express Mail Intl" ''ol#4.00(4/30)... (New-USPS) used to be "USPS-IEXP"
        End Get
    End Property

    Public ReadOnly Property Intl_PriorityMailInternational_Desc() As String
        Get
            Intl_PriorityMailInternational_Desc = "Priority Mail International" ''ol#4.00(4/29)... (New-USPS) used to be "USPS-GPM", "USPS-PPAIR", "USPS-PPECON"
        End Get
    End Property

    Public ReadOnly Property Intl_PriorityMailInternational_DescShort() As String
        Get
            Intl_PriorityMailInternational_DescShort = "Priority Mail Intl" ''ol#4.00(4/30)... (New-USPS) used to be "USPS-GPM", "USPS-PPAIR", "USPS-PPECON"
        End Get
    End Property

    Public ReadOnly Property Intl_FirstClassMailInternational_Desc() As String
        Get
            Intl_FirstClassMailInternational_Desc = "First-Class Mail International" ''ol#4.00(4/29)... (New-USPS) used to be "USPS-LPAIR", "USPS-LPECON"
        End Get
    End Property

    Public ReadOnly Property Intl_FirstClassMailInternational_DescShort() As String
        Get
            Intl_FirstClassMailInternational_DescShort = "First-Class Mail Intl" ''ol#4.00(4/30)... (New-USPS) used to be "USPS-LPAIR", "USPS-LPECON"
        End Get
    End Property

    Public Function IsAvailable_CertifiedMail(ServiceABBR As String, Optional isShowTheRule As Boolean = False) As Boolean

        IsAvailable_CertifiedMail = (_USPS.PriorityMail = ServiceABBR) Or (_USPS.FirstClassMail = ServiceABBR)

    End Function
    Public Function IsAvailable_ReturnReceipt(ServiceABBR As String, Optional isShowTheRule As Boolean = False) As Boolean

        IsAvailable_ReturnReceipt = (_USPS.PriorityMail = ServiceABBR) Or (_USPS.ExpressMail = ServiceABBR) Or (_USPS.FirstClassMail = ServiceABBR)

    End Function

#Region "USPS Domestic Zone Matrix"

    Public Property USPS_IsZoneMatrixLoaded As Boolean

    Private Enum USPS_MatrixFillerCodes
        NDC_Entry_Discount = 1
        PriorityMail_To_Military = 2
        FiveDigitException = 4
    End Enum

    Private Enum USPS_ExceptionMailTypeCodes
        PriorityMail_To_Military = 1
    End Enum

    Public Structure USPS_MatrixDestZip
        Dim DestZone As String ' 3 digit Numeric, 001 - 999
        Dim Filler As String ' 1 Numeric: *|a|e|b|1|space
    End Structure

    Public Structure USPS_MatrixEntry
        Dim OriginZip As String ' 3 digit
        Dim DestZips() As USPS_MatrixDestZip ' Destination Zip Codes
        Dim DestZipsLookup As String
    End Structure

    Public gUSPSDomesticZoneMatrix As USPS_MatrixEntry

    Public Structure USPS_Exceptions
        Dim OriginZipRangeStart As String ' 5 Numeric
        Dim OriginZipRangeEnd As String ' 5 Numeric
        Dim DestZipRangeStart As String ' 5 Numeric
        Dim DestZipRangeEnd As String ' 5 Numeric
        Dim Zone As String ' 2 Numeric
        Dim MailType As String ' Alpha/Numeric/Space '01'=Priority Mail Going to Military ZIP
        Dim Filler As String ' 6 Alpha/Numeric/Space
    End Structure

    Public gUSPSDomesticZoneExceptions() As USPS_Exceptions

    Public Function USPS_DomesticZoneChart_Load(ByVal storeZip As String) As Boolean

        Dim argStr As String = "ZipCode='" & storeZip & "'"
        Dim zoneMatrixFileName As String = "USPS_ZoneMatrix.txt"
        Dim zoneMatrixFilePath As String = gZoneTablesPath & "\" & zoneMatrixFileName
        Dim zoneExceptionFileName As String = "USPS_ZoneExceptions.txt"
        Dim zoneExceptionsFilePath As String = gZoneTablesPath & "\" & zoneExceptionFileName
        Dim errDesc As String = ""
        Dim errMsg As String = ""

        Try

            If Len(storeZip) > 0 Then
                If USPS_DomesticZoneMatrix_Load(zoneMatrixFilePath, storeZip) Then
                    If USPS_DomesticZoneExceptions_Load(zoneExceptionsFilePath, storeZip) Then
                        Return True
                    End If
                End If
            Else
                errDesc = "Origin Zip Code parameter missing."
            End If
        Catch ex As Exception
            errDesc = ex.Message
        End Try

        If Len(errDesc) > 0 Then
            errMsg = "Error loading USPS Zone Chart..." & vbCrLf & vbCrLf
            errMsg &= "Parameters: " & argStr & vbCrLf
            errMsg &= "Error Description: " & errDesc
            MessageBox.Show(errMsg, gProgramName, MessageBoxButton.OK, MessageBoxImage.Exclamation)
        End If

        Return False

    End Function

    Public Function USPS_DomesticZoneMatrix_Load(ByVal zoneMatrixFilePath As String, ByVal storeZip As String, Optional ByVal isShowDirErrMsg As Boolean = True) As Boolean

        Dim argStr As String = "FilePath='" & zoneMatrixFilePath & "', ZipCode='" & storeZip & "'"
        Dim lineBuf As String
        Dim oZip As String
        Dim dZips As String
        Dim i As Long
        Dim buf As String
        Dim storeZip3 As String
        Dim errDesc As String = ""
        Dim errMsg As String = ""

        Try
            ReDim gUSPSDomesticZoneMatrix.DestZips(999)
            storeZip3 = Strings.Mid(Trim(storeZip), 1, 3) ' get 3 digit

            If Len(storeZip3) = 3 And IsNumeric(storeZip3) Then
                If File.Exists(zoneMatrixFilePath) Then ' file exists
                    Using fileStream As StreamReader = File.OpenText(zoneMatrixFilePath)
                        ' first line is file date
                        If fileStream.Peek >= 0 Then
                            lineBuf = fileStream.ReadLine
                        End If
                        Do While fileStream.Peek >= 0
                            lineBuf = fileStream.ReadLine
                            oZip = Trim(Strings.Mid(lineBuf, 1, 3))
                            If storeZip3 = oZip Then
                                gUSPSDomesticZoneMatrix.OriginZip = oZip
                                dZips = Strings.Mid(lineBuf, 4)

                                i = 0
                                gUSPSDomesticZoneMatrix.DestZips(i).DestZone = "0"
                                gUSPSDomesticZoneMatrix.DestZips(i).Filler = ""
                                i = 1
                                Do Until dZips.Trim = "" Or i > 999
                                    buf = Strings.Mid(dZips, 1, 2)
                                    dZips = Strings.Mid(dZips, 3)
                                    gUSPSDomesticZoneMatrix.DestZips(i).DestZone = Trim(Strings.Mid(buf, 1, 1))
                                    gUSPSDomesticZoneMatrix.DestZips(i).Filler = Trim(Strings.Mid(buf, 2, 1))

                                    i = i + 1
                                Loop

                                Exit Do
                            End If
                        Loop
                    End Using

                    Return True
                ElseIf isShowDirErrMsg Then
                    errDesc = "USPS Zone Matrix File not found."
                End If
            Else
                errDesc = "Invalid Zip Code parameter."
            End If
        Catch ex As Exception
            errDesc = Err.Description
        End Try

        If Len(errDesc) > 0 Then
            errMsg &= "Error Loading USPS Zone Matrix..." & vbCrLf & vbCrLf
            errMsg &= "Parameters: " & argStr & vbCrLf
            errMsg &= "Error Description: " & errDesc
            MessageBox.Show(errMsg, gProgramName, MessageBoxButton.OK, MessageBoxImage.Exclamation)
        End If

        Return False

    End Function

    Public Function USPS_DomesticZoneExceptions_Load(ByVal zoneExceptionsFilePath As String, ByVal storeZip As String, Optional ByVal isShowDirErrMsg As Boolean = True) As Boolean

        Dim argStr As String : argStr = "FilePath='" & zoneExceptionsFilePath & "', ZipCode='" & storeZip & "'"
        Dim lineBuf As String
        Dim oZipStart As String
        Dim oZipEnd As String
        Dim dZips As String
        Dim i As Long
        Dim storeZip5 As String
        Dim errDesc As String = ""
        Dim errMsg As String = ""

        Try
            storeZip5 = Strings.Mid(Trim(storeZip), 1, 5) ' make sure 5 digit

            If Len(storeZip5) = 5 And IsNumeric(storeZip5) Then
                If File.Exists(zoneExceptionsFilePath) Then ' file exists
                    Using fileStream As StreamReader = File.OpenText(zoneExceptionsFilePath)
                        ' first line is file date
                        If fileStream.Peek >= 0 Then
                            lineBuf = fileStream.ReadLine
                        End If
                        i = 0
                        Do While fileStream.Peek >= 0
                            lineBuf = fileStream.ReadLine
                            oZipStart = Trim(Strings.Mid(lineBuf, 1, 5))
                            oZipEnd = Trim(Strings.Mid(lineBuf, 6, 5))

                            If Val(storeZip5) >= Val(oZipStart) And Val(storeZip5) <= Val(oZipEnd) Then
                                ReDim Preserve gUSPSDomesticZoneExceptions(i)
                                dZips = Strings.Mid(lineBuf, 11)

                                gUSPSDomesticZoneExceptions(i).OriginZipRangeStart = oZipStart ' Origin zip code start

                                gUSPSDomesticZoneExceptions(i).OriginZipRangeEnd = oZipEnd ' Origin zip code end

                                gUSPSDomesticZoneExceptions(i).DestZipRangeStart = Trim(Strings.Mid(dZips, 1, 5)) ' Dest zip code start
                                dZips = Strings.Mid(dZips, 5 + 1)

                                gUSPSDomesticZoneExceptions(i).DestZipRangeEnd = Trim(Strings.Mid(dZips, 1, 5)) ' Dest zip code end
                                dZips = Strings.Mid(dZips, 5 + 1)

                                gUSPSDomesticZoneExceptions(i).Zone = Trim(Strings.Mid(dZips, 1, 2)) ' Zone
                                dZips = Strings.Mid(dZips, 2 + 1)

                                gUSPSDomesticZoneExceptions(i).MailType = Trim(Strings.Mid(dZips, 1, 2)) ' Mail TYPE
                                dZips = Strings.Mid(dZips, 2 + 1)

                                gUSPSDomesticZoneExceptions(i).Filler = Trim(Strings.Mid(dZips, 1, 6)) ' Filler
                                dZips = Strings.Mid(dZips, 6 + 1)

                                i = i + 1
                            End If
                        Loop
                    End Using

                    Return True
                ElseIf isShowDirErrMsg Then
                    errDesc = "USPS Zone Exceptions File not found."
                End If
            Else
                errDesc = "Invalid Zip Code parameter."
            End If
        Catch ex As Exception
            errDesc = ex.Message
        End Try

        If Len(errDesc) > 0 Then
            errMsg &= "Error Loading USPS Zone Exceptions..." & vbCrLf & vbCrLf
            errMsg &= "Parameters: " & argStr & vbCrLf
            errMsg &= "Error Description: " & errDesc
            MessageBox.Show(errMsg, gProgramName, MessageBoxButton.OK, MessageBoxImage.Exclamation)
        End If

        Return False

    End Function

    Public Function USPS_DomesticZoneMatrix_LookupZone(ByVal destZip As String, ByRef retZone As String, ByRef retZoneFiller As String) As Boolean

        Dim dZipVal As Long
        Dim destZip3 As String

        Try
            retZone = ""
            retZoneFiller = ""
            destZip3 = Strings.Mid(Trim(destZip), 1, 3)
            dZipVal = Val(destZip3)

            If dZipVal > 0 Then
                If Len(gUSPSDomesticZoneMatrix.OriginZip) > 0 Then
                    If dZipVal <= UBound(gUSPSDomesticZoneMatrix.DestZips) Then
                        retZone = gUSPSDomesticZoneMatrix.DestZips(dZipVal).DestZone.TrimStart("0"c) 'String.Format("{0:##}", gUSPSDomesticZoneMatrix.DestZips(dZipVal).DestZone)

                        retZoneFiller = gUSPSDomesticZoneMatrix.DestZips(dZipVal).Filler
                        If Len(Trim(retZone)) > 0 Then
                            Return True
                        End If
                    End If
                End If
            End If
        Catch ex As Exception
        End Try

        Return False

    End Function

    Public Function USPS_DomesticZoneMatrix_ConvertFiller(ByVal fillerCode As String) As Integer

        Dim retFiller As USPS_MatrixFillerCodes = 0

        Try
            Select Case fillerCode

                Case "*" : retFiller = USPS_MatrixFillerCodes.NDC_Entry_Discount

                Case "1" : retFiller = USPS_MatrixFillerCodes.PriorityMail_To_Military

                Case "a" : retFiller = USPS_MatrixFillerCodes.NDC_Entry_Discount Or USPS_MatrixFillerCodes.PriorityMail_To_Military

                Case "b" : retFiller = USPS_MatrixFillerCodes.FiveDigitException Or USPS_MatrixFillerCodes.NDC_Entry_Discount

                Case "e" : retFiller = USPS_MatrixFillerCodes.FiveDigitException

            End Select
        Catch ex As Exception
        End Try

        Return retFiller

    End Function

    Public Function USPS_DomesticZoneMatrix_IsException(ByVal fillerCode As String) As Boolean

        Dim fillerVal As USPS_MatrixFillerCodes = USPS_DomesticZoneMatrix_ConvertFiller(fillerCode)

        Return (fillerVal And (USPS_MatrixFillerCodes.PriorityMail_To_Military Or USPS_MatrixFillerCodes.FiveDigitException))

    End Function

    Public Function USPS_DomesticZoneMatrix_IsPriority(ByVal fillerCode As String) As Boolean

        Dim fillerVal As USPS_MatrixFillerCodes = USPS_DomesticZoneMatrix_ConvertFiller(fillerCode)

        Return (fillerVal And (USPS_MatrixFillerCodes.PriorityMail_To_Military))

    End Function

    Public Function USPS_DomesticZoneMatrix_IsNDCEntryDiscount(ByVal fillerCode As String) As Boolean

        Dim fillerVal As USPS_MatrixFillerCodes = USPS_DomesticZoneMatrix_ConvertFiller(fillerCode)

        Return (fillerVal And (USPS_MatrixFillerCodes.NDC_Entry_Discount))

    End Function

    Public Function USPS_DomesticZoneExceptions_LookupZone(ByVal destZip As String, ByRef retZone As String, ByRef retMailType As String, ByRef retZoneFiller As String) As Boolean

        Dim dZipVal As Long
        Dim destZip5 As String
        Dim eZipStart As String
        Dim eZipStartVal As Long
        Dim eZipEnd As String
        Dim eZipEndVal As Long
        Dim i As Long

        Try
            retZone = ""
            retMailType = ""
            retZoneFiller = ""
            destZip5 = Strings.Mid(Trim(destZip), 1, 5)
            dZipVal = Val(destZip5)

            If dZipVal > 0 Then
                If LBound(gUSPSDomesticZoneExceptions) >= 0 And UBound(gUSPSDomesticZoneExceptions) >= 0 Then ' if error will jump to end of function
                    For i = LBound(gUSPSDomesticZoneExceptions) To UBound(gUSPSDomesticZoneExceptions)
                        eZipStart = gUSPSDomesticZoneExceptions(i).DestZipRangeStart
                        eZipEnd = gUSPSDomesticZoneExceptions(i).DestZipRangeEnd
                        eZipStartVal = Val(eZipStart)
                        eZipEndVal = Val(eZipEnd)

                        If dZipVal >= eZipStartVal And dZipVal <= eZipEndVal Then
                            retZone = gUSPSDomesticZoneExceptions(i).Zone.TrimStart("0"c) 'String.Format("{0:##}", gUSPSDomesticZoneExceptions(i).Zone)

                            retMailType = gUSPSDomesticZoneExceptions(i).MailType
                            retZoneFiller = gUSPSDomesticZoneExceptions(i).Filler

                            If Len(Trim(retZone)) > 0 Then
                                Return True
                            End If

                            Exit For
                        End If
                    Next
                End If
            End If
        Catch ex As Exception
        End Try

        Return False

    End Function

    Public Function USPS_DomesticZoneExceptions_ConvertMailType(ByVal mailTypeCode As String) As Integer

        Dim retMailType As USPS_ExceptionMailTypeCodes = 0

        Try
            Select Case Trim(mailTypeCode)

                Case "01", "1" : retMailType = USPS_ExceptionMailTypeCodes.PriorityMail_To_Military

            End Select

        Catch ex As Exception
        End Try

        Return retMailType

    End Function

    Public Function USPS_DomesticZoneExceptions_IsPriority(ByVal mailTypeCode As String) As Boolean
        ''AP(03/14/2018) - Processing of global domestic zone lookup text files added so individual .mzn files no longer necessary.

        Dim mailTypeVal As USPS_ExceptionMailTypeCodes = USPS_DomesticZoneExceptions_ConvertMailType(mailTypeCode)

        Return (mailTypeVal And (USPS_ExceptionMailTypeCodes.PriorityMail_To_Military))

    End Function

#End Region

    Public Sub Check_FlatRate_Pricing()
        Dim SQL As String
        Dim buf As String
        Dim segment As String


        'Load List of flat rate values from FlatRates.mdb
        Dim item As USPS_FlatRateItem
        Dim FLatR_List As List(Of USPS_FlatRateItem) = New List(Of USPS_FlatRateItem)
        Dim BaseCostColumn As String = Shipping_Discounts.Get_FlatRate_Discount

        If IsFileExist(gFlatRatesDB, True) Then
            SQL = "Select * From CarrierPackagingFlatRateValues INNER JOIN PackagingItems ON CarrierPackagingFlatRateValues.SettingID = PackagingItems.SettingID"
            buf = IO_GetSegmentSet(gFlatRatesDB, SQL)

            Do Until buf = ""
                segment = GetNextSegmentFromSet(buf)
                item = New USPS_FlatRateItem

                item.SettingName = ExtractElementFromSegment("SettingName", segment)
                item.ServiceTypeID = ExtractElementFromSegment("ServiceTypeID", segment)
                item.BaseCost = ExtractElementFromSegment(BaseCostColumn, segment, "0")
                item.BaseRetail = ExtractElementFromSegment("BaseCostRetail", segment, "0")
                FLatR_List.Add(item)
            Loop
        Else
            Exit Sub
        End If



        'SettingID's don't match up between the 2 databases.  Load SettingID's from Shipritepackaging.mdb into separate list
        Dim PackagingSetting_List As List(Of SettingItem) = New List(Of SettingItem)
        Dim SItem As SettingItem
        SQL = "Select SettingID, SettingName From PackagingItems"
        buf = IO_GetSegmentSet(gPackagingDB, SQL)

        Do Until buf = ""
            segment = GetNextSegmentFromSet(buf)
            SItem = New SettingItem

            SItem.SettingID = ExtractElementFromSegment("SettingID", segment)
            SItem.SettingName = ExtractElementFromSegment("SettingName", segment)
            PackagingSetting_List.Add(SItem)
        Loop


        'match up SettingIDs from shipritePackaging.mdb to List loaded from FlatRates.mdb
        For Each value As USPS_FlatRateItem In FLatR_List
            value.PackagingDB_SettingID = PackagingSetting_List.Find(Function(x As SettingItem) x.SettingName = value.SettingName).SettingID
        Next


        FLatR_List = FLatR_List.OrderBy(Function(x) x.PackagingDB_SettingID).ToList

        SQL = "" 'SQL for user cost pricing
        Dim RetailSQL = "" 'SQL for USPS Retail Pricing

        Dim currentValue As USPS_FlatRateItem = FLatR_List(0)
        '----Save Flat Rate Pricing
        For Each value As USPS_FlatRateItem In FLatR_List

            If currentValue.PackagingDB_SettingID = value.PackagingDB_SettingID Then
                SQL = SQL & "ServiceTypeID=" & value.ServiceTypeID & ", " & value.BaseCost & ", "
                RetailSQL = RetailSQL & "ServiceTypeID=" & value.ServiceTypeID & ", " & value.BaseRetail & ", "
            Else
                'new settingID found
                'process SQL for old setting ID
                UpdatePackagingMDB(SQL, currentValue.PackagingDB_SettingID, "BaseCost")
                UpdatePackagingMDB(RetailSQL, currentValue.PackagingDB_SettingID, "BaseRetail")

                currentValue = value
                SQL = "ServiceTypeID=" & value.ServiceTypeID & ", " & value.BaseCost & ", "
                RetailSQL = "ServiceTypeID=" & value.ServiceTypeID & ", " & value.BaseRetail & ", "

            End If
        Next

        UpdatePackagingMDB(SQL, currentValue.PackagingDB_SettingID, "BaseCost")
        UpdatePackagingMDB(RetailSQL, currentValue.PackagingDB_SettingID, "BaseRetail")

        Debug.Print(SQL)

    End Sub

    Private Sub UpdatePackagingMDB(SQL As String, settingID As Integer, Field As String)

        Dim SQLMain = "Update CarrierPackagingFlatRateValues set " & Field & " = SWITCH("

        SQL = SQL.Substring(0, SQL.Length - 2) 'remove trailing comma and space
        SQLMain = SQLMain & SQL

        SQLMain = SQLMain & ") WHERE SettingID=" & settingID

        IO_UpdateSQLProcessor(gPackagingDB, SQLMain)

    End Sub

End Module
