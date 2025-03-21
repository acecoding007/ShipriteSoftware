Imports wgssSTU

Module Shipping_Discounts

    Public Sub Check_Discount_Rules(ByRef shipment As ShippingChoiceDefinition)
        Select Case shipment.Carrier
            Case "DHL"
                Calculate_DHL_Discount(shipment)

            Case "FedEx"
                Calculate_FedEx_Discount(shipment)

            Case "UPS"
                Calculate_UPS_Discount(shipment)

            Case "USPS"
                Calculate_USPS_Discount(shipment)

            Case "SPEE-DEE"
                Calculate_SPEEDEE_Discount(shipment)
        End Select
    End Sub

    Public Function Get_FlatRate_Discount() As String
        If _EndiciaWeb.EndiciaWeb_IsEnabled Then
            'Endicia Enabled
            Return "BaseCostCommercial"
        Else
            'No Discount
            Return "BaseCostRetail"
        End If
    End Function

    Private Sub Calculate_SPEEDEE_Discount(ByRef shipment As ShippingChoiceDefinition)
        shipment.DiscountCost = shipment.BaseCost
    End Sub

    Private Sub Calculate_USPS_Discount(ByRef shipment As ShippingChoiceDefinition)
        Dim cost As Double
        Dim SRPRO_Cost As Double
        Dim cubeTier As Double
        Dim packaging As String
        Dim cubicrate As Double
        Dim isFlatRate As Boolean

        If shipment.Packaging Is Nothing Then
            packaging = ""
        Else
            packaging = shipment.Packaging.SettingName
        End If



        If _EndiciaWeb.EndiciaWeb_IsEnabled Or _IDs.IsIt_USPS_ApprovedShipper Or _IDs.IsIt_USPS_SRPRO_Rate Then
            'If Endicia Enabled, or ASO turned on, or SRPRO Rate turned On, turn on Commerical base pricing


            'check if carrier packaging item is selected.
            If packaging <> "" Then

                'Check if Regional Rate Box selected
                If shipment.Packaging.SettingName.Contains("Regnl") Then
                    cost = shipment.BaseCost
                ElseIf shipment.Packaging.SettingName.Contains("FlatR") Then
                    cost = ShipManager.Get_USPS_FlatRate_CostRetail(shipment, "BaseCost")
                    isFlatRate = True
                End If

            Else
                'Regular Packaging, check Commercial base discount
                'cost = ShipManager.GetShippingCost(ShipManager.Get_ServiceTable(shipment) & "_Commercial", shipment.Zone, shipment.Billable_Weight, shipment.DeliveryDate)

                If shipment.Service = "FirstClass" Or shipment.Service = "USPS-INTL-FCMI" Then
                    cost = ShipManager.Get_FirstClass_ShippingCost(shipment.Zone, Pounds2Ounces(shipment.Billable_Weight, 2), IIf(shipment.IsLetter, 1, 0), shipment.Service, True)
                Else
                    cost = ShipManager.GetShippingCost(shipment.Service & "_Commercial", shipment.Zone, shipment.Billable_Weight, shipment.DeliveryDate)
                End If
            End If



            If cost = 0 Then
                shipment.DiscountCost = shipment.BaseCost
            Else
                shipment.DiscountCost = cost
            End If


            'SRPRO rates discontinued by Endicia 8/12/2024
            'Check SRPRO cost
            'If _IDs.IsIt_USPS_SRPRO_Rate Then
            '    SRPRO_Cost = ShipManager.GetShippingCost(ShipManager.Get_ServiceTable(shipment) & "_SR", shipment.Zone, shipment.Billable_Weight, shipment.DeliveryDate)

            '    If SRPRO_Cost <> 0 Then
            '        shipment.DiscountCost = SRPRO_Cost
            '    End If
            'End If


        Else
            shipment.DiscountCost = shipment.BaseCost
        End If


        'check for cubic rate discounts
        If Not isFlatRate AndAlso ((shipment.Service = "USPS-GND-ADV" And GetPolicyData(gShipriteDB, "Enable_USPSCubicRate_ParcelSelect", "False")) Or (shipment.Service = "USPS-PRI" And GetPolicyData(gShipriteDB, "Enable_USPSCubicRate", "False"))) Then
            If IsEligible_CubicRates(shipment.Service, shipment.Weight, shipment.Length, shipment.Width, shipment.Height, packaging, cubeTier) Then
                Dim SRTable As String = ""

                'SRPRO rates discontinued by Endicia 8/12/2024
                'If _IDs.IsIt_USPS_SRPRO_Rate Then
                '    SRTable = "_SR"
                'End If

                cubicrate = ShipManager.GetShippingCost(ShipManager.Get_ServiceTable(shipment) & "_CubicRate" & SRTable, shipment.Zone, cubeTier, shipment.DeliveryDate, ShipManager.Get_ServiceDB_Path(shipment.Carrier))

                If cubicrate < shipment.DiscountCost Then
                    shipment.DiscountCost = cubicrate
                End If
            End If
        End If

    End Sub

    Public Function IsEligible_CubicRates(ByVal ServiceABBR As String, ByVal actWeight As Double, ByVal actLength As Double, ByVal actWidth As Double, ByVal actHeight As Double, ByVal actPackType As String, ByRef cubTier As Double) As Boolean
        ''ol#16.10(10/28)... USPS Cubic rates were added.
        Dim cubFeet As Double
        cubTier = 0 ' assume.
        If Not actPackType.Contains("Tube") Then ' Cubic-priced mailpieces may not be rolls or tubes.
            ''ol#16.10(1/18)... 'Soft Pak' was added to USPS Cubic rate logic
            If Not actWeight > 20 Then ' can not exceed 20 pounds
                If Not actLength > 18 Then ' longest dimension must not exceed 18 inches.
                    '
                    If actPackType.Contains("Soft") Or actPackType.Contains("Padded") Then
                        '
                        ''AP(04/05/2018) - USPS Cubic Rate pricing should be calculated using dimensions rounded down to the nearest 0.25 inch.
                        'cubFeet = actLength + actWidth
                        cubFeet = Round_DoubleToNearestMultiple(actLength, 0.25) + Round_DoubleToNearestMultiple(actWidth, 0.25)
                        If Not cubFeet > 36 Then ' The maximum total of length plus width cannot exceed 36 inches.
                            '
                            If ServiceABBR = "USPS-GND-ADV" Then
                                Select Case cubFeet
                                    Case Is <= 16 : cubTier = 0.1
                                    Case Is <= 21 : cubTier = 0.2
                                    Case Is <= 24 : cubTier = 0.3
                                    Case Is <= 26 : cubTier = 0.4
                                    Case Is <= 28 : cubTier = 0.5
                                    Case Is <= 30 : cubTier = 0.6
                                    Case Is <= 32 : cubTier = 0.7
                                    Case Is <= 34 : cubTier = 0.8
                                    Case Is <= 35 : cubTier = 0.9
                                    Case Else : cubTier = 1
                                End Select
                                Return True
                            Else ' Priority
                                Select Case cubFeet
                                    Case Is <= 21 : cubTier = 0.1
                                    Case Is <= 27 : cubTier = 0.2
                                    Case Is <= 31 : cubTier = 0.3
                                    Case Is <= 34 : cubTier = 0.4
                                    Case Else : cubTier = 0.5
                                End Select
                                Return True
                            End If
                            '
                        End If
                        '
                    Else
                        '
                        ''AP(04/05/2018) - USPS Cubic Rate pricing should be calculated using dimensions rounded down to the nearest 0.25 inch.
                        'If Shipping.Calculate_CubicDIM(actLength, actHeight, actWidth, 0, cubFeet) Then
                        If Calculate_CubicDIM(Round_DoubleToNearestMultiple(actLength, 0.25), Round_DoubleToNearestMultiple(actHeight, 0.25), Round_DoubleToNearestMultiple(actWidth, 0.25), 0, cubFeet) Then
                            If ServiceABBR = "USPS-GND-ADV" Then
                                If Not cubFeet > 1 Then  ' measure .50 cubic foot or less
                                    '
                                    Select Case cubFeet
                                        Case Is <= 0.1 : cubTier = 0.1
                                        Case Is <= 0.2 : cubTier = 0.2
                                        Case Is <= 0.3 : cubTier = 0.3
                                        Case Is <= 0.4 : cubTier = 0.4
                                        Case Is <= 0.5 : cubTier = 0.5
                                        Case Is <= 0.6 : cubTier = 0.6
                                        Case Is <= 0.7 : cubTier = 0.7
                                        Case Is <= 0.8 : cubTier = 0.8
                                        Case Is <= 0.9 : cubTier = 0.9
                                        Case Else : cubTier = 1
                                    End Select
                                    Return True
                                    '
                                End If
                            Else ' Priority
                                If Not cubFeet > 0.5 Then  ' measure .50 cubic foot or less
                                    '
                                    Select Case cubFeet
                                        Case Is <= 0.1 : cubTier = 0.1
                                        Case Is <= 0.2 : cubTier = 0.2
                                        Case Is <= 0.3 : cubTier = 0.3
                                        Case Is <= 0.4 : cubTier = 0.4
                                        Case Else : cubTier = 0.5
                                    End Select
                                    Return True
                                    '
                                End If
                            End If
                        End If
                        '
                    End If
                    '
                End If
            End If
        End If

        Return False
    End Function

    Public Function Round_DoubleToNearestMultiple(ByVal value2Round As Double, ByVal multiplier As Double, Optional ByVal isRoundUp As Boolean = False) As Double
        ''AP(04/05/2018) - Function added to round a value to a nearest multiple (i.e. Round a number to the nearest 0.25).

        Dim nearestValue As Double

        Round_DoubleToNearestMultiple = value2Round '' assume.
        nearestValue = CInt(value2Round / multiplier) * multiplier

        If isRoundUp Then
            If nearestValue >= value2Round Then
                Round_DoubleToNearestMultiple = nearestValue
            Else
                Round_DoubleToNearestMultiple = nearestValue + multiplier
            End If
        Else
            If nearestValue <= value2Round Then
                Round_DoubleToNearestMultiple = nearestValue
            Else
                Round_DoubleToNearestMultiple = nearestValue - multiplier
            End If
        End If

        Return Round_DoubleToNearestMultiple

    End Function

    Public Function Calculate_CubicDIM(nLength As Double, nHeight As Double, nWidth As Double, ByRef retCubicInches As Double, ByRef retCubicFeet As Double) As Boolean
        '------------------------------------------------------------'Oleg - Date: February 1, 2007
        ''AP(04/05/2018) - USPS Cubic Rate pricing should be calculated using dimensions rounded down to the nearest 0.25 inch.
        Dim nLxH As Double
        Dim nLxHxW As Double
        ''
        On Error GoTo Ooops
        ''
        retCubicInches = 0  '' assume
        retCubicFeet = 0    '' assume
        ''
        '' VB6 doesn't like to multiply all 3 dimentions at once - gives Overflow.
        nLxH = nLength * nHeight
        nLxHxW = nLxH * nWidth
        ''
        If nLxHxW > 0 Then
            retCubicInches = nLxHxW
            retCubicFeet = Round(retCubicInches / 1728, 3)
            'retCubicFeet = Convert.CubicInches2CubicFeet(retCubicInches, 3)
        End If
        ''
Ooops:
        ''
        Return (0 = Err.Number)
        '------------------------------------------------------------
    End Function

    Private Sub Calculate_UPS_Discount(ByRef shipment As ShippingChoiceDefinition)
        Dim Disc_Percentage As Double = 0
        Dim ServiceField As String = ""
        Dim Segment As String
        Dim buf As String = ""

        If _IDs.IsIt_PostNetStore Or _IDs.IsIt_UPS_ASO Then
            buf = gUPS_Discount_Segment
        End If


        If buf <> "" Then
            'Dim Level As String = GetPolicyData(gShipriteDB, "UPSLevel", "LEVEL_1")
            'All UPS ASO's are now same LEVEL 1.
            Dim Level As String = "LEVEL_1"

            If shipment.Service <> "COM-GND" And (shipment.ShipTo_State = "AK" Or shipment.ShipTo_State = "HI" Or shipment.ShipTo_State = "PR") Then
                ServiceField = shipment.Service & "_AK_HI_PR'"
            Else
                ServiceField = shipment.Service
            End If

            'check if there are special Letter discounts
            If shipment.IsLetter And (shipment.ShipTo_State <> "AK" Or shipment.ShipTo_State <> "HI" Or shipment.ShipTo_State <> "PR") Then
                If InStr("@" & ServiceField, buf) > -1 Then
                    ServiceField = "@" & ServiceField
                End If
            End If

            'check ground discounts
            If shipment.Service = "COM-GND" Then
                If shipment.Billable_Weight >= 31 Then
                    ServiceField = Replace(ServiceField, "COM-GND", "COM-GND_31LB")
                ElseIf shipment.Billable_Weight >= 21 Then
                    ServiceField = Replace(ServiceField, "COM-GND", "COM-GND_21LB")
                ElseIf shipment.Billable_Weight >= 11 Then
                    ServiceField = Replace(ServiceField, "COM-GND", "COM-GND_11LB")
                End If

                If shipment.IsResidential = False Then
                    ServiceField = Replace(ServiceField, "COM-GND", "COM-GND-COM")
                End If
            End If


            Do Until buf = ""
                Segment = GetNextSegmentFromSet(buf)

                If ExtractElementFromSegment("TABLE_NAME", Segment, "") = ServiceField Then
                    Disc_Percentage = Replace(ExtractElementFromSegment(Level, Segment, "0%"), "%", "")
                    Exit Do
                End If
            Loop


            Disc_Percentage = Disc_Percentage / 100

            shipment.DiscountCost = Round(shipment.BaseCost - (shipment.BaseCost * Disc_Percentage), 2)

        Else
            shipment.DiscountCost = shipment.BaseCost
        End If
    End Sub

    Private Sub Calculate_FedEx_Discount(ByRef shipment As ShippingChoiceDefinition)
        Dim Disc_Percentage As Double = 0
        Dim ServiceField As String = ""
        Dim Segment As String
        Dim buf As String = ""
        Dim Level As String



        If _IDs.IsIt_PostNetStore Or _IDs.IsIt_FedEx_FASC Then
            buf = gFedEx_Discount_Segment
        End If

        If buf <> "" Then

            If _IDs.IsIt_PostNetStore Then
                Level = "LEVEL_1"
            Else
                Update_FedEx_FASC_Tiers_Ind_20231116()
                ''
                Level = "LEVEL_" & Strings.Left(GetPolicyData(gShipriteDB, "RetailFedExLevel", ""), 1)
            End If

            If shipment.IsFlatRate Then
                ServiceField = shipment.Service & "-OneRate"
            ElseIf shipment.Service = "FEDEX-GND" And (shipment.ShipTo_State = "AK" Or shipment.ShipTo_State = "HI") Then
                ServiceField = shipment.Service & "_AK_HI"
            ElseIf shipment.ShipTo_State = "PR" Then
                ServiceField = shipment.Service & "_PR"
            Else
                ServiceField = shipment.Service
            End If

            If shipment.Service = "FEDEX-GND" And shipment.ShipTo_State <> "AK" And shipment.ShipTo_State <> "HI" And shipment.ShipTo_State <> "PR" Then
                If shipment.Billable_Weight >= 21 Then
                    ServiceField = Replace(ServiceField, "FEDEX-GND", "FEDEX-GND_21LB")
                End If
            End If

            If shipment.IsFedExHomeDelivery Then
                ServiceField = Replace(ServiceField, "FEDEX-GND", "FEDEX-GND-HD")
            End If

            Do Until buf = ""
                Segment = GetNextSegmentFromSet(buf)

                If ExtractElementFromSegment("TABLE_NAME", Segment, "") = ServiceField Then
                    Disc_Percentage = Replace(ExtractElementFromSegment(Level, Segment, "0%"), "%", "")
                    Exit Do
                End If
            Loop

            Disc_Percentage = Disc_Percentage / 100
            shipment.DiscountCost = Round(shipment.BaseCost - (shipment.BaseCost * Disc_Percentage), 2)

        Else
            shipment.DiscountCost = shipment.BaseCost
        End If

    End Sub

    Public Sub Update_FedEx_FASC_Tiers_Ind_20231116()
        If Not _IDs.IsIt_PostNetStore() AndAlso _IDs.IsIt_FedEx_FASC() Then
            Dim testLevel As String = Trim(GetPolicyData(gShipriteDB, "RetailFedExLevel", ""))
            If Not String.IsNullOrWhiteSpace(testLevel) Then
                If Not (testLevel = "1 ($0-$40,999.99)" Or testLevel = "2 ($41,000-)") Then
                    Select Case testLevel
                        Case "1 ($0-$11,999)", "2 ($12,000-$34,999)" : testLevel = "1 ($0-$40,999.99)"
                        Case "3 ($35,000-$64,999)", "4 ($65,000-$139,999)", "5 ($140,000-)" : testLevel = "2 ($41,000-)"
                        Case Else : testLevel = "1 ($0-$40,999.99)"
                    End Select
                    UpdatePolicy(gShipriteDB, "RetailFedExLevel", testLevel)
                End If
            End If
        End If
    End Sub


    Private Sub Calculate_DHL_Discount(ByRef shipment As ShippingChoiceDefinition)
        Dim Table As String
        Dim DB_Disc As String = GetPolicyData(gShipriteDB, "DHL_INTL_RATETABLE", "")

        Select Case UCase(DB_Disc)
            Case "TIER1"
                Table = "Tier1"
            Case "TIER2"
                Table = "Tier2"
            Case "TOPTIER"
                Table = "TopTier"
            Case Else
                'No Discount
                shipment.DiscountCost = shipment.BaseCost
                Exit Sub

        End Select

        shipment.DiscountCost = ShipManager.GetShippingCost(shipment.Service & "-" & Table, shipment.Zone, shipment.Billable_Weight, shipment.DeliveryDate, ShipManager.Get_ServiceDB_Path(shipment.Carrier))

    End Sub

End Module
