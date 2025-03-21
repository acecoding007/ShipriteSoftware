
Public Module FedEx

    Public Property CarrierName() As String
        Get
            Return "FEDEX"
        End Get
        Set(ByVal value As String)
        End Set
    End Property

    Public Property Ground() As String
        Get
            Return "FEDEX-GND"
        End Get
        Set(ByVal value As String)
        End Set
    End Property
    Public Property FirstOvernight() As String
        Get
            Return "FEDEX-1ST"
        End Get
        Set(ByVal value As String)
        End Set
    End Property
    Public Property SecondDay() As String
        Get
            Return "FEDEX-2DY"
        End Get
        Set(ByVal value As String)
        End Set
    End Property
    Public Property SecondDayAM() As String
        Get
            Return "FEDEX-2DY-AM"
        End Get
        Set(ByVal value As String)
        End Set
    End Property
    Public Property Priority() As String
        Get
            Return "FEDEX-PRI"
        End Get
        Set(ByVal value As String)
        End Set
    End Property
    Public Property Standard() As String
        Get
            Return "FEDEX-STD"
        End Get
        Set(ByVal value As String)
        End Set
    End Property
    Public Property Saver() As String
        Get
            Return "FEDEX-SVR"
        End Get
        Set(ByVal value As String)
        End Set
    End Property
    Public Property CanadaGround() As String
        Get
            Return "FEDEX-CAN"
        End Get
        Set(ByVal value As String)
        End Set
    End Property
    Public Property Intl_First() As String
        Get
            Return "FEDEX-INT-1ST"
        End Get
        Set(ByVal value As String)
        End Set
    End Property
    Public Property Intl_Priority() As String
        Get
            Return "FEDEX-INTP"
        End Get
        Set(ByVal value As String)
        End Set
    End Property
    Public Property Intl_Economy() As String
        Get
            Return "FEDEX-INTE"
        End Get
        Set(ByVal value As String)
        End Set
    End Property
    Public Property Freight_1Day() As String
        Get
            Return "FEDEX-FR1"
        End Get
        Set(ByVal value As String)
        End Set
    End Property
    Public Property Freight_2Day() As String
        Get
            Return "FEDEX-FR2"
        End Get
        Set(ByVal value As String)
        End Set
    End Property
    Public Property Freight_3Day() As String
        Get
            Return "FEDEX-FR3"
        End Get
        Set(ByVal value As String)
        End Set
    End Property

    Public ReadOnly Property IsWebServicesReady() As Boolean
        Get
            IsWebServicesReady = False ' assume
            If IsNumeric(_FedExWeb.objFedEx_Regular_Setup.Client_AccountNumber) Then
                If Not 0 = Val(_FedExWeb.objFedEx_Regular_Setup.Client_AccountNumber) Then
                    If Not 0 = Len(_FedExWeb.objFedEx_Regular_Setup.Web_UserCredential_Key) And Not 0 = Len(_FedExWeb.objFedEx_Regular_Setup.Web_UserCredential_Pass) Then
                        IsWebServicesReady = (Not 0 = Len(_FedExWeb.objFedEx_Regular_Setup.Client_MeterNumber))
                    End If
                End If
            End If
        End Get
    End Property
    Public ReadOnly Property NoCommInvoiceNeeded_FilePath() As String
        Get
            Return gDBpath & "\FedEx\NoCommInvoiceNeeded.txt"
        End Get
    End Property

    Public IsEmail_FedEx_ShipNotification As Boolean
    'Public IsEnabled_OneRate As Boolean

    Public Function IsGroundHomeDelivery(ByVal ServiceABBR As String) As Boolean
        IsGroundHomeDelivery = False
        If "FEDEX-GND" = ServiceABBR Then
            IsGroundHomeDelivery = gShip.Residential 'And (Not gShip.actualWeight > 70) 'home delivery is available up to 150lb
        End If
    End Function

    Public Function NoCommInvoiceNeeded_AddCountryToFile(ByVal addCountryName As String) As Boolean
        NoCommInvoiceNeeded_AddCountryToFile = False ' assume
        Try
            Return _Files.WriteFile_ByOneString(addCountryName, FedEx.NoCommInvoiceNeeded_FilePath)
        Catch ex As Exception : _Debug.Print_(ex.Message)
        End Try
    End Function

    Public Function FedEx_GetNumericZone(ServiceABBR As String, ZoneStr As String) As Integer
        ''
        Dim ZoneNum As Integer : ZoneNum = 0
        ''
        Try
            If isServiceDomestic(ServiceABBR) Then
                ''
                'FEDEX-1ST, FEDEX-2DY, FEDEX-2DY-AM, FEDEX-PRI, FEDEX-STD, FEDEX-SVR, FEDEX-GND
                ' 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, PR
                ' 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16
                ' 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16
                ' 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, HI, PR
                ' 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16
                ' 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, PR
                ' 2, 3, 4, 5, 6, 7, 8, 9, 10, 14 (Intra HI), 17 (To AK), 92 (From HI), 96 (From HI), 22 (Intra AK), 23 (From AK), 25 (From AK)
                ZoneStr = Trim(UCase(ZoneStr))
                ZoneStr = Replace(ZoneStr, "ZONE", "")
                ''
                If Len(ZoneStr) > 0 Then
                    ' convert non numbers to numbers to be used by AH/OVS Select Case
                    Select Case ZoneStr
                        Case 22, 14, "HI" : ZoneNum = 3
                        Case "PR" : ZoneNum = 7
                        Case Else : ZoneNum = Val(ZoneStr)
                    End Select
                End If
                ''
            End If
        Catch ex As Exception
            _Debug.PrintError_(ex.Message)
        End Try
        ''
        FedEx_GetNumericZone = ZoneNum
        ''
    End Function

End Module

Public Module FedEx_Freight

    Public LTL_Freight As _baseFreight

    Public ReadOnly Property IsWebServicesReady_FreightBox() As Boolean
        Get
            IsWebServicesReady_FreightBox = False ' assume
            If IsNumeric(_FedExWeb.objFedEx_Freight_Setup.Client_AccountNumber) Then
                If Not 0 = Val(_FedExWeb.objFedEx_Freight_Setup.Client_AccountNumber) Then
                    If Not 0 = Len(_FedExWeb.objFedEx_Freight_Setup.Web_UserCredential_Key) And Not 0 = Len(_FedExWeb.objFedEx_Freight_Setup.Web_UserCredential_Pass) Then
                        IsWebServicesReady_FreightBox = (Not 0 = Len(_FedExWeb.objFedEx_Freight_Setup.Client_MeterNumber))
                    End If
                End If
            End If
        End Get
    End Property

    Public Function IsFreightBoxPackaging(ByVal sPackaging As String) As Boolean
        IsFreightBoxPackaging = False
        If _Controls.Contains(sPackaging, "Freight") And _Controls.Contains(sPackaging, "Box") Then
            IsFreightBoxPackaging = True
        End If
    End Function
    Public Function IsFreightLTLService(ByVal ServiceABBR As String) As Boolean
        IsFreightLTLService = False
        If ServiceABBR = "FEDEX-FRP" Or ServiceABBR = "FEDEX-FRE" Then
            IsFreightLTLService = True
        End If
    End Function
    Public Function IsFreight_123Day_Service(ByVal ServiceABBR As String) As Boolean
        IsFreight_123Day_Service = False
        If ServiceABBR = "FEDEX-FR1" Or ServiceABBR = "FEDEX-FR2" Or ServiceABBR = "FEDEX-FR3" Then
            IsFreight_123Day_Service = True
        End If
    End Function

    Public Function Create_FreightItemsObject(ByRef objShipment As _baseShipment) As Boolean
        Create_FreightItemsObject = False
        If FedEx_Freight.IsFreightLTLService(objShipment.CarrierService.ServiceABBR) Then
            'Debug_.Stop_ToDo "Create Freight Object..."
            Dim objPackage As _baseShipmentPackage
            Dim objFreight As _baseFreight
            objPackage = objShipment.Packages(0)
            Dim i As Int16 = 0
            If LTL_Freight IsNot Nothing AndAlso LTL_Freight.FreightFormItems IsNot Nothing Then
                For Each objFreightItem As FreightFormItem In LTL_Freight.FreightFormItems
                    '
                    If Not String.IsNullOrEmpty(objFreightItem.Description) Then
                        '
                        objFreight = New _baseFreight
                        If i > 0 Then
                            objPackage = New _baseShipmentPackage
                        End If
                        With objFreight
                            .LTL_Freight_Class = objFreightItem.PackageClass
                            .LTL_Freight_Description = objFreightItem.Description
                            .LTL_Freight_Packaging = objFreightItem.PackagingType
                            .LTL_Freight_TotalHandlingUnits = objFreightItem.HandlingUnits
                            .TotalShipmentPieces = objFreightItem.PiecesNo
                        End With
                        objPackage.Freight = objFreight
                        '
                        objPackage.Dim_Height = gShip.Height
                        objPackage.Dim_Length = gShip.Length
                        objPackage.Dim_Width = gShip.Width
                        objPackage.PackagingType = gShip.PackagingType
                        objPackage.Weight_LBs = objFreightItem.Weight
                        objPackage.DeclaredValue = objFreightItem.InsuredValue
                        '
                        If i > 0 Then
                            objShipment.Packages.Add(objPackage)
                        End If
                        '
                    End If
                    '
                    i += 1
                Next
            End If
        End If
    End Function

    Public Function TotalInsuredValue() As Double
        Dim tot As Double
        ''
        For Each FreightItem As FreightFormItem In LTL_Freight.FreightFormItems
            tot = tot + FreightItem.InsuredValue
        Next
        ''
        TotalInsuredValue = tot
    End Function
    Public Function TotalFreightWeight() As Double
        Dim tot As Double
        ''
        For Each FreightItem As FreightFormItem In LTL_Freight.FreightFormItems
            tot = tot + FreightItem.Weight
        Next
        ''
        TotalFreightWeight = tot
    End Function
    Public Function ServiceDesc(ByVal servABBR As String) As String
        If servABBR = "FEDEX-FRE" Then
            ServiceDesc = "FedEx Freight® Economy"
        ElseIf servABBR = "FEDEX-FRP" Then
            ServiceDesc = "FedEx Freight® Priority"
        Else
            Return String.Empty
        End If
    End Function

    Public Function Get_ChargeAndSurcharge(ByVal zipcode As String, ByVal fServiceType As String, ByRef fCharge As Double, ByRef fSurcharge As Double) As Boolean
        Dim sql2exe As String
        ''
        fCharge = 0 '' assume.
        fSurcharge = 0
        ''
        If LTL_Freight IsNot Nothing AndAlso LTL_Freight.FreightFormItems IsNot Nothing Then
            If LTL_Freight.FreightFormItems.Count > 0 Then
                Dim FreightItem As FreightFormItem = LTL_Freight.FreightFormItems(0)
                sql2exe = "SELECT Charge, Surcharge FROM Freight_Rates WHERE RateDate = #" & DateAndTime.Today & "# And Service = '" & fServiceType & "' And [Zip]='" & zipcode & "' And " &
                      "[Weight]=" & gShip.actualWeight & " And [DimL]=" & gShip.Length & " And [DimH]=" & gShip.Height & " And [DimW]=" & gShip.Width & " And " &
                      "[DecVal]=" & Val(gShip.DecVal) & " And [Packaging]='" & FreightItem.PackagingType.ToUpper & "' And [Class]='" & FreightItem.PackageClass.ToUpper & "'"
                _Debug.Print_(sql2exe)
                Dim SegmentSet As String = DatabaseFunctions.IO_GetSegmentSet(gFedExServicesDB, sql2exe)
                If Not String.IsNullOrEmpty(SegmentSet) Then
                    fCharge = Val(ExtractElementFromSegment("Charge", SegmentSet))
                    fSurcharge = Val(ExtractElementFromSegment("Surcharge", SegmentSet))
                End If
            End If
            ''
        End If
        ''
        Return Not 0 = fCharge
    End Function
    Public Function Get_FlatBoxCharge(ByVal fServiceType As String, ByVal fZone As String, ByRef fCharge As Double) As Boolean
        Dim sql2exe As String
        ''
        fCharge = 0 '' assume.
        sql2exe = "SELECT [" & fZone & "] FROM [FEDEX-FRBOX] WHERE [Service] = '" & fServiceType & "'"
        _Debug.Print_(sql2exe)
        Dim SegmentSet As String = DatabaseFunctions.IO_GetSegmentSet(gFedExServicesDB, sql2exe)
        If Not String.IsNullOrEmpty(SegmentSet) Then
            fCharge = Val(ExtractElementFromSegment(fZone, SegmentSet))
        End If
        ''
        Return (Not 0 = fCharge)
    End Function

    Public Function get_FreightWeightBrake(ByVal nWeight As Double, ByRef retLBS As Long) As Boolean
        '' There 4 breaks for Freight weight in pounds:
        get_FreightWeightBrake = True '' assume we have the weight.
        Select Case nWeight
            Case 151 To 499 : retLBS = 1
            Case 500 To 999 : retLBS = 2
            Case 1000 To 1999 : retLBS = 3
            Case Is >= 2000 : retLBS = 4
            Case Else : get_FreightWeightBrake = False
        End Select
    End Function
    Private Function get_FreightCharge(path2db As String, frieghtService As String, zoneNo As String, nWeight As Double, retCharge As Double, Optional errMsg As String = "") As Boolean
        get_FreightCharge = False ' assume.
        retCharge = 0 ' assume.
        '
        Dim lbsBreak As Long
        Dim sql2exe As String = String.Empty
        Dim zoneCharge As Double
        Dim minCharge As Double
        '
        ' There 4 breaks for Freight weight in pounds:
        If get_FreightWeightBrake(nWeight, lbsBreak) Then
            ''
            sql2exe = "Select LBS, " & zoneNo & " From [" & frieghtService & "] Where LBS = " & CStr(lbsBreak)
            _Debug.Print_(sql2exe)
            Dim rs As String = DatabaseFunctions.IO_GetSegmentSet(gShipriteDB, sql2exe)
            If Not String.IsNullOrEmpty(rs) Then
                '
                zoneCharge = Val(ExtractElementFromSegment(zoneNo, rs))
                retCharge = _Convert.Round_Double2Decimals(zoneCharge * nWeight, 2)
                '
                ' Compare to the minimum charge
                sql2exe = "Select LBS, " & zoneNo & " From [" & frieghtService & "] Where LBS = 5" '' minimum charge weight break
                Dim rsMin As String = DatabaseFunctions.IO_GetSegmentSet(gShipriteDB, sql2exe)
                If Not String.IsNullOrEmpty(rsMin) Then
                    '
                    minCharge = Val(ExtractElementFromSegment(zoneNo, rsMin))
                    If retCharge < minCharge Then
                        '
                        retCharge = minCharge
                        '
                    End If
                    '
                End If
            Else
                errMsg = "No FedEx Frieght to this area-zone."
                '
            End If
            '
        Else
            '
            errMsg = "FedEx Freight Pricing begins at 151 lbs."
            '
        End If
        '
        get_FreightCharge = (Not retCharge = 0)
    End Function
End Module

