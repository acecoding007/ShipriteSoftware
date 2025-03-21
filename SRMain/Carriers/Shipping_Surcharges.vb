Public Module Shipping_Surcharges
    Dim ShipMngr_Win As ShipManager
    Dim PrintLabel_Win As Print_Shipping_Label
    Dim charge As ShippingSurcharge
    Dim DHLZoneSegment As String

    'Declared Value Fields -----------------------
    Dim FreeUpTo_Amt As Double
    Dim MinimumInsured_Amt As Double
    Dim Min_Cost As Double
    Dim Min_Sell As Double
    Dim EachAdditional_Amt As Double
    Dim DV_Cost As Double
    Dim DV_Sell As Double
    ''--------------------------------------

    Private Class Peak_Link
        Public Property SurchargeID As Integer 'Internal surchargeID
        Public Property PeakCharge_Name As String ' Surcharge Description in Service Table
    End Class

    Public Class Peak_Surcharge
        Public Property Surcharge As String
        Public Property Service As String
        Public Property Cost As Double
        Public Property Retail As Double
        Public Property DateFrom As Date
        Public Property DateTo As Date
    End Class

    <System.Serializable()>
    Public Class ShippingSurcharge
        Public Property Name As String
        Public Property ID As Integer
        Public Property BaseCost As Double
        Public Property SellPrice As Double
        Public Property DiscountCost As Double
        Public Property DBField_Cost As String
        Public Property DBField_Sell As String
        Public Property DBField_Manifest_Cost As String
        Public Property DBField_Manifest_Sell As String
        Public Property DeclaredValue_Amt As Double

        Public Enum IDs_Other As Integer
            RoundOption = 999
        End Enum

        Public Enum IDs_FedEx As Integer
            AddlHand = 1
            Resi = 2
            SatDel = 3
            Sig_Ind = 4
            Sig_Dir = 5
            Sig_Adult = 6
            DecVal = 7
            Oversize = 8
            DAS_HomeDel = 9
            DAS_Res = 10
            DAS_Comm = 11
            DAS_HomeDelExt = 12
            DAS_ResExt = 13
            DAS_CommExt = 14
            DAS_HI = 15
            DAS_AK = 16
            DAS_IntraHI = 17
            DAS_UsRem = 18
            Cod = 19
            DryIce = 20
            HomeDel_Appt = 21
            HomeDel_Eve = 22
            HomeDel_DateCert = 23
            SatPU = 24
            FuelSC = 49
        End Enum
        Public Enum IDs_UPS As Integer
            AddlHand = 51
            Resi = 52
            SatDel = 53
            Sig_DelConf = 54
            Sig_Req = 55
            Sig_Adult = 56
            DecVal = 57
            Oversize = 58
            DAS_Res = 59
            DAS_Comm = 60
            DAS_ResExt = 61
            DAS_CommExt = 62
            DAS_HI = 63
            DAS_AK = 64
            DAS_UsRem = 65
            Cod = 66
            SatPU = 67
            FuelSC = 99
        End Enum
        Public Enum IDs_USPS As Integer
            Sig_Conf = 101
            Sig_Adult = 102
            DecVal = 103
            CertMail = 104
            RetRcpt = 105
            NonStand_Len = 106
            NonStand_Vol = 107
        End Enum
        Public Enum IDs_DHL As Integer
            DecVal = 151
            DemandSC = 152
            ElevRisk = 153
            RestrDestination = 154

            FuelSC = 199
        End Enum

        Public Enum IDs_SPEEDEE As Integer
            DecVal = 201
            Resi = 202
            Sig_DelConf = 203
            Sig_Req = 204
            Sig_Adult = 205
            DAS = 206

            FuelSC = 249

        End Enum
    End Class

    Public Sub Check_Surcharge_Rules(ByRef shipment As ShippingChoiceDefinition)

        shipment.SurchargesList = New List(Of ShippingSurcharge)
        charge = New ShippingSurcharge
        Set_ShipMngr_Window_Variable()
        Set_PrintLabel_Windows_Vairable()

        Select Case shipment.Carrier
            Case "FedEx"
                Check_FedEx_LargePackageSurcharge(shipment)
                Check_FedEx_AdditionalHandling(shipment)
                Check_FedEx_ResidentialSurcharge(shipment)
                Check_FedEx_SaturdayDelivery(shipment)
                Check_FedEx_SaturdayPickup(shipment)
                Check_FedEx_SignatureType(shipment)
                Check_FedEx_Declared_Value(shipment)
                Check_FedEx_DAS(shipment)
                Check_FedEx_COD(shipment)
                Check_FedEx_DryIce(shipment)

                If shipment.Service = "FEDEX-GND" Then
                    Check_FedEx_AppointmentHomeDelivery(shipment)
                    Check_FedEx_EveningHomeDelivery(shipment)
                    Check_FedEx_DateCertainHomeDelivery(shipment)
                End If

                Check_Peak_Surcharges(shipment)

                Check_FedEx_FuelSurcharge(shipment)


            Case "UPS"
                Check_UPS_LargePackageSurcharge(shipment)
                Check_UPS_AdditionalHandling(shipment)
                Check_UPS_ResidentialSurcharge(shipment)
                Check_UPS_SatrudayDelivery(shipment)
                Check_UPS_SignatureType(shipment)
                Check_UPS_Declared_Value(shipment)
                Check_UPS_DAS(shipment)
                Check_UPS_COD(shipment)
                Check_Peak_Surcharges(shipment)

                Check_UPS_FuelSurcharge(shipment)


            Case "USPS"
                Check_USPS_SignatureType(shipment)
                Check_USPS_DeclaredValue(shipment)
                Check_USPS_Certifiedmail(shipment)
                Check_USPS_ReturnReceipt(shipment)
                Check_USPS_NonstandardFee(shipment)

            Case "DHL"
                Get_DHL_ZoneSegment(shipment.ShipTo_Country)

                Check_DHL_RestrictedDestination_ElevatedRisk(shipment)
                Check_DHL_DeclaredValue(shipment)
                Check_Peak_Surcharges(shipment)

                Check_DHL_FuelSurcharge(shipment)

            Case "SPEE-DEE"
                Check_SPEEDEE_DeclaredValue(shipment)
                Check_SpeeDee_ResidentialSurcharge(shipment)
                Check_SPEEDEE_SignatureType(shipment)
                Check_SPEEDEE_DAS(shipment)

                Check_SPEEDEE_FuelSurcharge(shipment)
        End Select

    End Sub

    Private Sub Set_ShipMngr_Window_Variable()
        ShipMngr_Win = CommonWindowStack.windowList.Find(Function(x As CommonWindow) x.Name = "ShipManager_Window")
    End Sub

    Private Sub Set_PrintLabel_Windows_Vairable()
        PrintLabel_Win = CommonWindowStack.windowList.Find(Function(x As CommonWindow) x.Name = "Print_Shipping_Label_Window")

    End Sub

    Private Function Add_SurchargeDetails(ByRef shipment As ShippingChoiceDefinition, ID As Integer, Name As String, DBCostField As String, DBSellField As String, DBManifest_Cost As String, DBManifest_Sell As String) As ShippingSurcharge
        Dim Scharge As ShippingSurcharge = New ShippingSurcharge

        Scharge.ID = ID
        Scharge.Name = Name
        Scharge.DBField_Cost = DBCostField
        Scharge.DBField_Sell = DBSellField
        Scharge.DBField_Manifest_Cost = DBManifest_Cost
        Scharge.DBField_Manifest_Sell = DBManifest_Sell

        Scharge.BaseCost = ExtractElementFromSegment(Scharge.DBField_Cost, shipment.Segment, "0")
        Scharge.SellPrice = ExtractElementFromSegment(Scharge.DBField_Sell, shipment.Segment, "0")

        Scharge.DiscountCost = Scharge.BaseCost

        Return Scharge
    End Function

    Private Function Is_Button_Selected(BtnName As String, win As String) As Boolean

        If win = "SHIP" Then
            If IsNothing(ShipMngr_Win) Then
                Set_ShipMngr_Window_Variable()
            End If

            Return ShipMngr_Win.FindName(BtnName).isChecked

        ElseIf win = "PrintLabel" Then

            If Not IsNothing(PrintLabel_Win) Then
                'PrintLabel_Win.UpdateLayout()
                Return PrintLabel_Win.FindName(BtnName).isChecked
            End If
        End If

        Return False
    End Function

    Private Sub Read_DecVal_DBFields(ByRef shipment As ShippingChoiceDefinition)
        'Dec. Val DB fields are same for all carriers.

        If shipment.isThirdPartyDecVal Then
            FreeUpTo_Amt = ExtractElementFromSegment("thirdACTDVBASE", shipment.Segment, "0")
            MinimumInsured_Amt = ExtractElementFromSegment("thirdDVRateUpTo", shipment.Segment, "0")

            Min_Cost = ExtractElementFromSegment("thirdDVRateUpToCost", shipment.Segment, "0")
            Min_Sell = ExtractElementFromSegment("thirdDVRateUpToCharge", shipment.Segment, "0")


            EachAdditional_Amt = ExtractElementFromSegment("thirdACTDVINC", shipment.Segment, "0")
            DV_Cost = ExtractElementFromSegment("thirdACTDECVAL", shipment.Segment, "0")
            DV_Sell = ExtractElementFromSegment("thirdDV", shipment.Segment, "0")



        Else
            FreeUpTo_Amt = ExtractElementFromSegment("ACTDVBASE", shipment.Segment, "0")
            MinimumInsured_Amt = ExtractElementFromSegment("DVRateUpTo", shipment.Segment, "0")

            Min_Cost = ExtractElementFromSegment("DVRateUpToCost", shipment.Segment, "0")
            Min_Sell = ExtractElementFromSegment("DVRateUpToCharge", shipment.Segment, "0")


            EachAdditional_Amt = ExtractElementFromSegment("ACTDVINC", shipment.Segment, "0")
            DV_Cost = ExtractElementFromSegment("ACTDECVAL", shipment.Segment, "0")
            DV_Sell = ExtractElementFromSegment("DV", shipment.Segment, "0")

        End If


    End Sub

    Private Function Check_If_SurchargePresent(ByRef shipment As ShippingChoiceDefinition, ID As Integer) As Boolean

        If shipment.SurchargesList.FindIndex(Function(x As ShippingSurcharge) x.ID = ID) = -1 Then
            Return False
        Else
            Return True
        End If
    End Function

    Private Sub Check_Peak_Surcharges(ByRef shipment As ShippingChoiceDefinition)
        Dim LocShipment As ShippingChoiceDefinition = shipment 'local copy needed for lambda search
        Dim PeakCharge As Peak_Surcharge
        Dim i As Integer
        Dim Surcharges_MasterList As List(Of Peak_Surcharge) = Nothing
        'creates link between the "Surcharge" field in the service table, and the ID of the surcharge in program.
        Dim Definitions_List As List(Of Peak_Link) = New List(Of Peak_Link)


        Select Case shipment.Carrier
            Case "FedEx"
                Definitions_List.Add(AddPeakListItem(ShippingSurcharge.IDs_FedEx.Oversize, "Oversize Charge"))
                Definitions_List.Add(AddPeakListItem(ShippingSurcharge.IDs_FedEx.AddlHand, "Additional Handling"))
                Surcharges_MasterList = gFedExPeakSurcharges

            Case "UPS"
                Definitions_List.Add(AddPeakListItem(ShippingSurcharge.IDs_UPS.Oversize, "Large Package"))
                Definitions_List.Add(AddPeakListItem(ShippingSurcharge.IDs_UPS.AddlHand, "Additional Handling"))
                Surcharges_MasterList = gUPSPeakSurcharges

            Case "DHL"
                Check_DHL_Demand_Surcharge(shipment)
                Exit Sub
        End Select


        For Each item As Peak_Link In Definitions_List

            'check if surcharge that qualifies for Peak is included in shipment.
            i = shipment.SurchargesList.FindIndex(Function(surcharge) surcharge.ID = item.SurchargeID)

            If i <> -1 Then

                PeakCharge = Nothing
                'Check if the peak charge applies
                PeakCharge = Surcharges_MasterList.Find(Function(x) x.Surcharge = item.PeakCharge_Name And x.Service = LocShipment.Service And x.DateFrom <= Today And x.DateTo >= Today)

                If Not IsNothing(PeakCharge) Then
                    'Add Fee on top of the regular surcharge
                    shipment.SurchargesList(i).BaseCost += PeakCharge.Cost
                    shipment.SurchargesList(i).DiscountCost += PeakCharge.Cost
                    shipment.SurchargesList(i).SellPrice += PeakCharge.Retail

                    shipment.SurchargesList(i).Name = "Peak " & shipment.SurchargesList(i).Name

                End If
            End If
        Next

    End Sub

    Private Function AddPeakListItem(ID As String, PeakName As String) As Peak_Link
        Dim item As Peak_Link = New Peak_Link

        item.SurchargeID = ID
        item.PeakCharge_Name = PeakName

        Return item

    End Function

    Private Sub Set_FuelSurcharge(ByRef shipment As ShippingChoiceDefinition, ByRef Fuel_Calc_List As List(Of Integer), Surcharge_ID As Integer)

        Dim SubTotal_Published As Double = 0
        Dim SubTotal_Cost As Double = 0
        Dim SubTotal_Retail As Double = 0
        Dim FuelPercentCost As Double
        Dim FuelPercentSell As Double

        'SubTotal is the total used for fuel surcharge cal culation.
        'add base shipping charge to subtotal
        SubTotal_Cost += shipment.DiscountCost
        SubTotal_Published += shipment.BaseCost
        SubTotal_Retail += shipment.BaseCost

        'Check which surcharges are on the Fuel Surcharge Applicable list and add them to subtotal.
        For Each item As ShippingSurcharge In shipment.SurchargesList
            If Fuel_Calc_List.Contains(item.ID) Then
                SubTotal_Cost += item.DiscountCost
                SubTotal_Published += item.BaseCost
                SubTotal_Retail += item.SellPrice
            End If
        Next

        'get fuel surcharge percentages from Master table
        FuelPercentCost = ExtractElementFromSegment("ActFuel", shipment.Segment, "0")
        FuelPercentSell = ExtractElementFromSegment("Fuel", shipment.Segment, "0")

        Dim Scharge As ShippingSurcharge = New ShippingSurcharge

        Scharge.ID = Surcharge_ID
        Scharge.Name = "Fuel Surcharge"
        Scharge.BaseCost = Round(SubTotal_Published * FuelPercentCost / 100, 2)
        Scharge.DiscountCost = Round(SubTotal_Cost * FuelPercentCost / 100, 2)
        Scharge.SellPrice = Round(SubTotal_Retail * FuelPercentSell / 100, 2)
        Scharge.DBField_Manifest_Cost = "costFuel"
        Scharge.DBField_Manifest_Sell = "Fuel"

        shipment.SurchargesList.Add(Scharge)


    End Sub

#Region "SPEE-DEE"


    Private Sub Check_SPEEDEE_DAS(ByRef shipment As ShippingChoiceDefinition)

        Dim content As String = My.Computer.FileSystem.ReadAllText(gDASPath & "/SpeeDee_DAS.txt")

        If content.Contains(shipment.ZipCode) Then
            charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_SPEEDEE.DAS, "DAS", "ACTDAS", "DAS", "costDAS1", "DAS1")
            shipment.SurchargesList.Add(charge)
            Exit Sub
        End If
    End Sub

    Private Sub Check_SPEEDEE_ResidentialSurcharge(ByRef shipment As ShippingChoiceDefinition)

        If Is_Button_Selected("Residential_Btn", "SHIP") Or shipment.IsResidential Then

            shipment.IsResidential = True

            charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_FedEx.Resi, "Residential Surcharge", "ACTRESIDENTIAL", "ResidentialSurcharge", "costRES", "chgRES")
            shipment.SurchargesList.Add(charge)

        End If

    End Sub

    Private Sub Check_SPEEDEE_DeclaredValue(shipment)
        If shipment.DeclaredValue = 0 Then Exit Sub

        Dim TotalCost As Double = 0
        Dim TotalSell As Double = 0
        Dim Description As String = ""


        Read_DecVal_DBFields(shipment)

        'Check Free Insurance Amount
        If shipment.DeclaredValue <= FreeUpTo_Amt Then
            Description = "Declared Value $" & FreeUpTo_Amt & " free"
            GoTo AddDecValCharge
        End If



        'Check Minimum Charge
        If shipment.DeclaredValue <= MinimumInsured_Amt Then
            Description = "Declared Value $" & shipment.DeclaredValue & " (Min Charge)"
            TotalCost = Min_Cost
            TotalSell = Min_Sell
            GoTo AddDecValCharge
        End If

        If EachAdditional_Amt = 0 Then
            TotalSell = Min_Sell
            TotalCost = Min_Cost
        Else
            TotalSell = (Math.Ceiling((shipment.DeclaredValue - MinimumInsured_Amt) / EachAdditional_Amt) * DV_Sell) + Min_Sell
            TotalCost = (Math.Ceiling((shipment.DeclaredValue - MinimumInsured_Amt) / EachAdditional_Amt) * DV_Cost) + Min_Cost
        End If


        Description = "Declared Value $" & shipment.DeclaredValue



AddDecValCharge:

        If shipment.isThirdPartyDecVal Then
            Description = "3rd Party " & Description
        End If

        Dim Scharge As ShippingSurcharge = New ShippingSurcharge

        Scharge.ID = ShippingSurcharge.IDs_SPEEDEE.DecVal
        Scharge.Name = Description
        Scharge.BaseCost = TotalCost
        Scharge.DiscountCost = TotalCost
        Scharge.SellPrice = TotalSell
        Scharge.DBField_Manifest_Cost = "costINS1"
        Scharge.DBField_Manifest_Sell = "INS1"
        Scharge.DeclaredValue_Amt = shipment.DeclaredValue

        shipment.SurchargesList.Add(Scharge)
    End Sub

    Private Sub Check_SPEEDEE_FuelSurcharge(shipment)
        Dim Fuel_Calc_List As List(Of Integer) = New List(Of Integer)

        Set_FuelSurcharge(shipment, Fuel_Calc_List, ShippingSurcharge.IDs_SPEEDEE.FuelSC)
    End Sub

    Private Sub Check_SPEEDEE_SignatureType(ByRef shipment As ShippingChoiceDefinition)
        If Is_Button_Selected("DelConf_Btn", "SHIP") Then

            charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_UPS.Sig_DelConf, "Delivery Confirmation", "ACTDELC", "ACK", "costACK1", "ACK1")
            shipment.SurchargesList.Add(charge)

        ElseIf Is_Button_Selected("SigConfirm_Btn", "SHIP") Then

            charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_UPS.Sig_Req, "Signature Required", "ACTDELSIG", "ACK-S", "costACKSIG1", "ACKSIG1")
            shipment.SurchargesList.Add(charge)

        ElseIf Is_Button_Selected("AdultSig_Btn", "SHIP") Then

            charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_UPS.Sig_Adult, "Adult Signature Required", "ACTDELSIGADULT", "DELSIGADULT", "costACKSIG1", "ACKSIG1")
            shipment.SurchargesList.Add(charge)
        End If

    End Sub

#End Region



#Region "FedEx"
    Public Function Check_FedEx_IsAdditionalHandling() As Boolean
        Dim shipment As New ShippingChoiceDefinition
        Check_FedEx_AdditionalHandling(shipment)
        Return shipment.SurchargesList.Count > 0
    End Function

    Private Sub Check_FedEx_AdditionalHandling(ByRef shipment As ShippingChoiceDefinition)
        Dim addCharge As Boolean = False
        Dim Type As String = ""

        'check if Oversize charge is already added
        If Check_If_SurchargePresent(shipment, ShippingSurcharge.IDs_FedEx.Oversize) Then
            Exit Sub
        End If

        'check if manually added with Button in Ship screen
        If Is_Button_Selected("AdditionalHandling_Btn", "SHIP") Then
            addCharge = True
            Type = "Packaging"
        End If


        'Check Size, Weight rules
        If (shipment.Weight > 70 And shipment.IsInternational) Or (shipment.Weight > 50 And Not shipment.IsInternational) Then
            addCharge = True
            Type = "Weight"

        ElseIf shipment.Length > 48 Or shipment.Width > 48 Or shipment.Height > 48 Then
            addCharge = True
            Type = "Dimensions"

        ElseIf (shipment.Length > 30 And shipment.Width > 30) Or (shipment.Length > 30 And shipment.Height > 30) Or (shipment.Width > 30 And shipment.Height > 30) Then
            addCharge = True
            Type = "Dimensions"

        ElseIf ShipManager.Calculate_Length_Plus_Girth(shipment.Length, shipment.Width, shipment.Height) > 105 Then
            addCharge = True
            Type = "Dimensions"

        End If

        If addCharge Then
            'Adds surcharge to shipment

            Dim weightDiff As Double = 0 ' price difference to add if weight based fee
            Dim packagingDiff As Double = 0 ' price difference to subtract if packaging based fee

            If shipment.IsInternational Then
                charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_FedEx.AddlHand, "Additional Handling - " & Type, "ACTAH", "AH", "costAH1", "AH1")
                weightDiff = 11 ' Effective 01/06/2025, dims 27 | weight 38.00 | diff 11.00
                packagingDiff = 0 ' Effective 01/06/2025, dims 27 | pkg 27 | diff 0.00
            Else
                'Domestic
                Dim zoneNoInt As Integer = FedEx.FedEx_GetNumericZone(shipment.Service, shipment.Zone)
                Select Case zoneNoInt
                    Case 2
                        charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_FedEx.AddlHand, "Additional Handling - " & Type, "OVS2_Cost", "OVS2_Charge", "costAH1", "AH1")
                        weightDiff = 15.5 ' Effective 01/06/2025, dims 28.00 | weight 43.50 | diff 15.50
                        packagingDiff = 3 ' Effective 01/06/2025, dims 28.00 | pkg 25.00 | diff 3.00
                    Case 3 To 4
                        charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_FedEx.AddlHand, "Additional Handling - " & Type, "ACTAH", "AH", "costAH1", "AH1")
                        weightDiff = 16.5 ' Effective 01/06/2025, dims 31.00 | weight 47.50 | diff 16.50
                        packagingDiff = 2 ' Effective 01/06/2025, dims 31.00| pkg 29.00 | diff 2.00
                    Case 5 To 6
                        charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_FedEx.AddlHand, "Additional Handling - " & Type, "OVS3_Cost", "OVS3_Charge", "costAH1", "AH1")
                        weightDiff = 16.5 ' Effective 01/06/2025, dims 34.00 | weight 50.50 | diff 16.50
                        packagingDiff = 3.5 ' Effective 01/06/2025, dims 34.00 | pkg 30.50 | diff 3.50
                    Case Is >= 7
                        charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_FedEx.AddlHand, "Additional Handling - " & Type, "OVS7_Cost", "OVS7_Charge", "costAH1", "AH1")
                        weightDiff = 17 ' Effective 01/06/2025 dims 38.00 | weight 55.00 | diff 17.00
                        packagingDiff = 6.5 ' Effective 01/06/2025, dims 38.00 | pkg 31.50 | diff 6.50
                End Select

                '------Remove this code after 1/6/2025--------
                If Today < #1/6/2025# Then

                    Select Case zoneNoInt
                        Case 2
                            weightDiff -= 3
                            packagingDiff -= 0.5
                        Case 3 To 4
                            weightDiff -= 3.5
                            packagingDiff -= 0.5
                        Case 5 To 6
                            weightDiff -= 3.5
                            packagingDiff -= 0.5
                        Case Is >= 7
                            weightDiff -= 3.5
                            packagingDiff -= 1.5
                    End Select
                End If
                '----------------------------------------------
            End If

            'dimension type is default
            If Type = "Weight" Then
                charge.BaseCost += weightDiff
                charge.DiscountCost += weightDiff
                charge.SellPrice += weightDiff
            ElseIf Type = "Packaging" Then
                charge.BaseCost -= packagingDiff
                charge.DiscountCost -= packagingDiff
                charge.SellPrice -= packagingDiff
            End If

            shipment.SurchargesList.Add(charge)

            If Type = "Dimensions" And Today >= #01/13/2025# Then
                'All U.S. and international packages that meet the criteria of Additional Handling Surcharge – Dimension will be subject to a 40-lb. minimum billable weight

                If shipment.Billable_Weight < 40 Then
                    shipment.Billable_Weight = 40
                    shipment.IsBillableWeight_changed = True
                End If
            End If
        End If
    End Sub



    Private Sub Check_FedEx_ResidentialSurcharge(ByRef shipment As ShippingChoiceDefinition)

        If Is_Button_Selected("Residential_Btn", "SHIP") Or shipment.IsResidential Then

            If shipment.IsFlatRate Then Exit Sub

            shipment.IsResidential = True
            If shipment.IsFedExHomeDelivery Then
                charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_FedEx.Resi, "Residential Surcharge - Home Delivery", "ResHomeCost", "ResHomeCharge", "costRES", "chgRES")
            Else
                charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_FedEx.Resi, "Residential Surcharge", "ACTRESIDENTIAL", "ResidentialSurcharge", "costRES", "chgRES")
            End If

            shipment.SurchargesList.Add(charge)
        End If

    End Sub

    Private Sub Check_FedEx_SaturdayDelivery(ByRef shipment As ShippingChoiceDefinition)
        If Is_Button_Selected("SatDelivery_Btn", "SHIP") Then

            If Not (Today.DayOfWeek = DayOfWeek.Saturday) Then

                charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_FedEx.SatDel, "FedEx Saturday Delivery", "ACTSAT", "SAT", "costSAT", "ACTSAT")
                shipment.SurchargesList.Add(charge)

            End If
        End If
    End Sub

    Private Sub Check_FedEx_SaturdayPickup(ByRef shipment As ShippingChoiceDefinition)
        If Is_Button_Selected("SatDelivery_Btn", "SHIP") Then

            If (Today.DayOfWeek = DayOfWeek.Saturday) Then

                charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_FedEx.SatPU, "FedEx Saturday Pickup", "ACTSATPU", "SATPU", "costSATPU", "ActSATPU")
                shipment.SurchargesList.Add(charge)

            End If
        End If
    End Sub

    Private Sub Check_FedEx_SignatureType(ByRef shipment As ShippingChoiceDefinition)
        If Is_Button_Selected("DelConf_Btn", "SHIP") Then

            charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_FedEx.Sig_Ind, "FedEx Indirect Signature", "ISigCost", "ISigChg", "costFedEXHDSignature", "FedEXHDSignature")
            shipment.SurchargesList.Add(charge)

        ElseIf Is_Button_Selected("SigConfirm_Btn", "SHIP") Then

            charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_FedEx.Sig_Dir, "FedEx Direct Signature", "ACTDELC", "ACK", "costFedEXHDSignature", "FedEXHDSignature")
            shipment.SurchargesList.Add(charge)

        ElseIf Is_Button_Selected("AdultSig_Btn", "SHIP") Then

            charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_FedEx.Sig_Adult, "FedEx Adult Signature", "ACTDELSIG", "ACK-S", "costFedEXHDSignature", "FedEXHDSignature")
            shipment.SurchargesList.Add(charge)
        End If

    End Sub

    Private Sub Check_FedEx_Declared_Value(ByRef shipment As ShippingChoiceDefinition)
        If shipment.DeclaredValue = 0 Then Exit Sub

        Dim TotalCost As Double = 0
        Dim TotalSell As Double = 0
        Dim Description As String = ""


        Read_DecVal_DBFields(shipment)

        'Check Free Insurance Amount
        If shipment.DeclaredValue <= FreeUpTo_Amt Then
            Description = "Declared Value $" & FreeUpTo_Amt & " free"
            GoTo AddDecValCharge
        End If



        'Domestic services and Canada Ground
        If (shipment.IsInternational = False Or shipment.Service = "FEDEX-CAN") And (Not shipment.Service.Contains("FEDEX-FR")) Then

            'Check Minimum Charge
            If shipment.DeclaredValue <= MinimumInsured_Amt Or EachAdditional_Amt = 0 Then
                Description = "Declared Value $" & shipment.DeclaredValue & " (Min Charge)"
                TotalCost = Min_Cost
                TotalSell = Min_Sell
                GoTo AddDecValCharge
            End If

            TotalSell = (Math.Ceiling((shipment.DeclaredValue - MinimumInsured_Amt) / EachAdditional_Amt) * DV_Sell) + Min_Sell
            TotalCost = (Math.Ceiling((shipment.DeclaredValue - MinimumInsured_Amt) / EachAdditional_Amt) * DV_Cost) + Min_Cost
            Description = "Declared Value $" & shipment.DeclaredValue
            GoTo AddDecValCharge

        Else
            'International Services and Freight - no minimum charge
            TotalSell = (Math.Ceiling(shipment.DeclaredValue / EachAdditional_Amt) * DV_Sell)
            TotalCost = (Math.Ceiling(shipment.DeclaredValue / EachAdditional_Amt) * DV_Cost)
            Description = "Declared Value $" & shipment.DeclaredValue

        End If



AddDecValCharge:

        If shipment.isThirdPartyDecVal Then
            Description = "3rd Party " & Description
        End If

        Dim Scharge As ShippingSurcharge = New ShippingSurcharge

        Scharge.ID = ShippingSurcharge.IDs_FedEx.DecVal
        Scharge.Name = Description
        Scharge.BaseCost = TotalCost
        Scharge.DiscountCost = TotalCost
        Scharge.SellPrice = TotalSell
        Scharge.DBField_Manifest_Cost = "costINS1"
        Scharge.DBField_Manifest_Sell = "INS1"
        Scharge.DeclaredValue_Amt = shipment.DeclaredValue

        shipment.SurchargesList.Add(Scharge)

    End Sub

    Public Function Check_FedEx_IsLargePackageSurcharge() As Boolean
        Dim shipment As New ShippingChoiceDefinition
        Check_FedEx_LargePackageSurcharge(shipment)
        Return shipment.SurchargesList.Count > 0
    End Function

    Private Sub Check_FedEx_LargePackageSurcharge(ByRef shipment As ShippingChoiceDefinition)

        'Any side longer then 96" or Length+Girth more then 130"
        If shipment.Length > 96 Or shipment.Width > 96 Or shipment.Height > 96 Or ShipManager.Calculate_Length_Plus_Girth(shipment.Length, shipment.Width, shipment.Height) > 130 Then

            If shipment.IsInternational Then
                'International
                charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_FedEx.Oversize, "Oversize Charge", "costAHPlus", "AHPlus", "costAHPlus", "AHPlus")

            Else
                'Domestic
                Dim residentialDiff As Double = 0 ' price difference to add if residential based fee
                Dim zoneNoInt As Integer = FedEx.FedEx_GetNumericZone(shipment.Service, shipment.Zone)
                Select Case zoneNoInt
                    Case 2
                        charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_FedEx.Oversize, "Oversize Charge", "OVS4_Cost", "OVS4_Charge", "costAHPlus", "AHPlus")
                        residentialDiff = 35 ' Effective 01/06/2025, comm 205.00 - res 240.00 - diff 35.00
                    Case 3 To 4
                        charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_FedEx.Oversize, "Oversize Charge", "costAHPlus", "AHPlus", "costAHPlus", "AHPlus")
                        residentialDiff = 35 ' Effective 01/06/2025, comm 225.00 - res 260.00 - diff 35.00
                    Case 5 To 6
                        charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_FedEx.Oversize, "Oversize Charge", "OVS5_Cost", "OVS5_Charge", "costAHPlus", "AHPlus")
                        residentialDiff = 50 ' Effective 01/06/2025, comm 240.00 - res 290.00 - diff 50.00
                    Case Is >= 7
                        charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_FedEx.Oversize, "Oversize Charge", "OVS8_Cost", "OVS8_Charge", "costAHPlus", "AHPlus")
                        residentialDiff = 45 ' Effective 01/06/2025, comm 260.00 - res 305.00 - diff 45.00
                End Select

                '------Remove this code after 1/6/2025--------
                If Today < #1/6/2025# Then
                    Select Case zoneNoInt
                        Case 2
                            residentialDiff -= 5
                        Case 3 To 4
                            residentialDiff -= 5
                        Case 5 To 6
                            residentialDiff -= 10
                        Case Is >= 7
                            residentialDiff -= 10
                    End Select
                End If
                '-----------------------------------------------

                If shipment.IsFedExHomeDelivery Then
                    charge.BaseCost += residentialDiff
                    charge.DiscountCost += residentialDiff
                    charge.SellPrice += residentialDiff
                End If

            End If

            shipment.SurchargesList.Add(charge)

            If shipment.Billable_Weight < 90 Then
                shipment.Billable_Weight = 90
                shipment.IsBillableWeight_changed = True
            End If
        End If

    End Sub


    Public Function Is_Zip_FedEx_DAS(Zip As String) As Integer

        ' 0 - No DAS
        ' 1 - DAS
        ' 2 - Extended DAS
        ' 3 - Hawaii DAS
        ' 4 - Alaska DAS

        Dim content As String



        'Check DAS
        content = My.Computer.FileSystem.ReadAllText(gDASPath & "/FEDEX_DAS.txt")
        If content.Contains(Zip) Then
            Return 1
        Else

            'Check Extended DAS
            content = My.Computer.FileSystem.ReadAllText(gDASPath & "/FEDEX_DAS_ContiguousUS_Extended.txt")
            If content.Contains(Zip) Then
                Return 2
            Else

                'Check Hawaii DAS
                content = My.Computer.FileSystem.ReadAllText(gDASPath & "/FedEx_DAS_Hawaii.txt")
                If content.Contains(Zip) Then
                    Return 3
                Else

                    'Check Alaska DAS
                    content = My.Computer.FileSystem.ReadAllText(gDASPath & "/FedEx_DAS_Alaska.txt")
                    If content.Contains(Zip) Then
                        Return 4
                    Else

                        'Check Remote DAS
                        content = My.Computer.FileSystem.ReadAllText(gDASPath & "/FEDEX_DAS_ContiguousUS_Remote.txt")
                        If content.Contains(Zip) Then
                            Return 5

                        End If
                    End If
                End If
            End If
        End If

        If Is_Button_Selected("DAS_Btn", "SHIP") Then
            Return 1
        Else
            Return 0
        End If


    End Function

    Private Sub Check_FedEx_DAS(ByRef shipment As ShippingChoiceDefinition)

        'Check Intra-Hawaii DAS
        If GetPolicyData(gShipriteDB, "State") = "HI" And shipment.Service = "FEDEX-GND" Then
            Dim content As String = My.Computer.FileSystem.ReadAllText(gDASPath & "/FEDEX_DAS_IntraHawaii.txt")

            If content.Contains(shipment.ZipCode) Then
                charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_FedEx.DAS_IntraHI, "DAS Intra - Hawaii", "DasIntHiCost", "DasIntHiCharge", "costDAS1", "DAS1")
                Exit Sub
            End If
        End If

        If shipment.IsFlatRate Then Exit Sub

        Select Case shipment.IsDAS
            Case 0
                ' No DAS
                Exit Sub

            Case 1
                ' DAS
                If shipment.IsResidential Then
                    If shipment.IsFedExHomeDelivery Then
                        charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_FedEx.DAS_HomeDel, "DAS Surcharge - Home Del", "DasHomeDelCost", "DasHomeDelCharge", "costDAS1", "DAS1")
                    Else
                        charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_FedEx.DAS_Res, "DAS Residential", "ACTDAS", "DAS", "costDAS1", "DAS1")
                    End If

                Else
                    charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_FedEx.DAS_Comm, "DAS Commercial", "DASCOMM", "aDASCOMM", "costDAS1", "DAS1")
                End If


            Case 2
                ' Extended DAS
                If shipment.IsResidential Then
                    If shipment.IsFedExHomeDelivery Then
                        charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_FedEx.DAS_HomeDelExt, "DAS Ext Surcharge - Home Del", "DasExtHomeDelCost", "DasExtHomeDelCharge", "costDAS1", "DAS1")
                    Else
                        charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_FedEx.DAS_ResExt, "DAS Extended Residential", "DasExtCost", "DasExtCharge", "costDAS1", "DAS1")
                    End If

                Else
                    charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_FedEx.DAS_CommExt, "DAS Extended Commercial", "DasExtCommCost", "DasExtCommCharge", "costDAS1", "DAS1")
                End If


            Case 3
                ' Hawaii DAS
                charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_FedEx.DAS_HI, "DAS Hawaii", "DasHiCost", "DasHiCharge", "costDAS1", "DAS1")


            Case 4
                ' Alaska DAS
                charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_FedEx.DAS_AK, "DAS Alaska", "DasAkCost", "DasAkCharge", "costDAS1", "DAS1")

            Case 5
                'Remote US48 
                charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_FedEx.DAS_UsRem, "DAS US Remote", "ACTAP", "AP", "costDAS1", "DAS1")

        End Select

        shipment.SurchargesList.Add(charge)

    End Sub

    Private Sub Check_FedEx_COD(ByRef shipment As ShippingChoiceDefinition)
        'check if manually added with Button
        If Is_Button_Selected("COD_Btn", "SHIP") Then
            charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_FedEx.Cod, "FedEx COD", "ACTCOD", "COD", "costCOD1", "COD1")
            shipment.SurchargesList.Add(charge)
        End If
    End Sub


    Private Sub Check_FedEx_DryIce(ByRef shipment As ShippingChoiceDefinition)
        If Is_Button_Selected("FedEx_DryIce_CheckBox", "PrintLabel") Then
            charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_FedEx.DryIce, "Dry Ice", "fldDryIce_Cost", "fldDryIce_Charge", "LabPackCost", "LabPackCharge")
            shipment.SurchargesList.Add(charge)
        End If
    End Sub

    Private Sub Check_FedEx_AppointmentHomeDelivery(ByRef shipment As ShippingChoiceDefinition)
        If Is_Button_Selected("FedEx_Appointment_RadioBtn", "PrintLabel") Then
            charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_FedEx.HomeDel_Appt, "Appointment Home Delivery", "costFedEXHDAppt", "FedEXHDAppt", "costFedEXHDAppt", "FedEXHDAppt")
            shipment.SurchargesList.Add(charge)
        End If
    End Sub
    Private Sub Check_FedEx_EveningHomeDelivery(ByRef shipment As ShippingChoiceDefinition)
        If Is_Button_Selected("FedEx_Evening_RadioBtn", "PrintLabel") Then
            charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_FedEx.HomeDel_Eve, "Evening Home Delivery", "costFedEXHDEvening", "FedEXHDEvening", "costFedEXHDEvening", "FedEXHDEvening")
            shipment.SurchargesList.Add(charge)
        End If
    End Sub
    Private Sub Check_FedEx_DateCertainHomeDelivery(ByRef shipment As ShippingChoiceDefinition)
        If Is_Button_Selected("FedEx_DateCertain_RadioBtn", "PrintLabel") Then
            charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_FedEx.HomeDel_DateCert, "Date Certain Home Delivery", "costFedEXHDCertain", "FedEXHDCertain", "costFedEXHDCertain", "FedEXHDCertain")
            shipment.SurchargesList.Add(charge)
        End If
    End Sub

    Private Sub Check_FedEx_FuelSurcharge(ByRef shipment As ShippingChoiceDefinition)

        If shipment.IsFlatRate Then Exit Sub

        'List of all surcharge ID's that need to be included in the fuel surcharge calculation
        Dim Fuel_Calc_List As List(Of Integer) = New List(Of Integer)

        With Fuel_Calc_List
            .Add(1) 'additional handling
            .Add(2) 'residential
            .Add(3) 'saturday delivery
            .AddRange(New Integer() {ShippingSurcharge.IDs_FedEx.Sig_Ind, ShippingSurcharge.IDs_FedEx.Sig_Dir, ShippingSurcharge.IDs_FedEx.Sig_Adult}) 'Signature Options
            .Add(8) 'oversize fee
            .AddRange(New Integer() {ShippingSurcharge.IDs_FedEx.DAS_HomeDel, ShippingSurcharge.IDs_FedEx.DAS_Res, ShippingSurcharge.IDs_FedEx.DAS_Comm, ShippingSurcharge.IDs_FedEx.DAS_HomeDelExt, ShippingSurcharge.IDs_FedEx.DAS_ResExt, ShippingSurcharge.IDs_FedEx.DAS_CommExt, ShippingSurcharge.IDs_FedEx.DAS_HI, ShippingSurcharge.IDs_FedEx.DAS_AK, ShippingSurcharge.IDs_FedEx.DAS_IntraHI, ShippingSurcharge.IDs_FedEx.DAS_UsRem}) 'DAS Charges
            .Add(18) 'COD
            .AddRange(New Integer() {ShippingSurcharge.IDs_FedEx.HomeDel_Appt, ShippingSurcharge.IDs_FedEx.HomeDel_Eve, ShippingSurcharge.IDs_FedEx.HomeDel_DateCert}) 'Home Delivery Premium Options
            If shipment.IsInternational Then
                .Add(20) 'Dry Ice added to calculation for international shipments, effective Jan 13th 2025
            End If
            .Add(ShippingSurcharge.IDs_FedEx.SatPU)


        End With

        Set_FuelSurcharge(shipment, Fuel_Calc_List, ShippingSurcharge.IDs_FedEx.FuelSC)

    End Sub

#End Region


#Region "UPS"
    Public Function Check_UPS_IsAdditionalHandling() As Boolean
        Dim shipment As New ShippingChoiceDefinition
        Check_UPS_AdditionalHandling(shipment)
        Return shipment.SurchargesList.Count > 0
    End Function

    Private Sub Check_UPS_AdditionalHandling(ByRef shipment As ShippingChoiceDefinition)
        Dim addCharge As Boolean = False
        Dim Type As String = ""

        'check if Oversize charge is already added
        If Check_If_SurchargePresent(shipment, ShippingSurcharge.IDs_UPS.Oversize) Then
            Exit Sub
        End If

        'check if manually added with Button
        If Is_Button_Selected("AdditionalHandling_Btn", "SHIP") Then
            addCharge = True
            Type = "Packaging"
        End If

        'Check Size, Weight rules
        ' Effective 12/26/2023, an Additional Handling fee will apply to international packages weighing 55 pounds or more.
        If (shipment.Weight > 55 And isServiceInternational(shipment.Service)) Or (shipment.Weight > 50 And Not isServiceInternational(shipment.Service)) Then
            addCharge = True
            Type = "Weight"

        ElseIf shipment.Length > 48 Or shipment.Width > 48 Or shipment.Height > 48 Then
            addCharge = True
            Type = "Dimensions"

        ElseIf (shipment.Length > 30 And shipment.Width > 30) Or (shipment.Length > 30 And shipment.Height > 30) Or (shipment.Width > 30 And shipment.Height > 30) Then
            addCharge = True
            Type = "Dimensions"

        ElseIf ShipManager.Calculate_Length_Plus_Girth(shipment.Length, shipment.Width, shipment.Height) > 105 Then
            addCharge = True
            Type = "Dimensions"

        End If

        If addCharge Then
            'Adds surcharge to shipment

            Dim weightDiff As Double = 0 ' price difference to add if weight based fee
            Dim packagingDiff As Double = 0 ' price difference to subtract if packaging based fee

            If isServiceInternational(shipment.Service) Then
                'International
                charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_UPS.AddlHand, "Additional Handling - " & Type, "ACTAH", "AH", "costAH1", "AH1")

            Else
                'Domestic
                Dim zoneNoInt As Integer = UPS.UPS_GetNumericZone(shipment.Service, shipment.Zone)
                Select Case zoneNoInt
                    Case 2
                        charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_UPS.AddlHand, "Additional Handling - " & Type, "OVS2_Cost", "OVS2_Charge", "costAH1", "AH1")
                        weightDiff = 15.5 ' Effective 12/23/2024, dims 28.00 | weight 43.50 | diff 15.50
                        packagingDiff = 3 ' Effective 12/23/2024, dims 28.00 | pkg 25.00 | diff -3.00
                    Case 3 To 4
                        charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_UPS.AddlHand, "Additional Handling - " & Type, "ACTAH", "AH", "costAH1", "AH1")
                        weightDiff = 16.5 ' Effective 12/23/2024, dims 31.00 | weight 47.50 | diff 16.50
                        packagingDiff = 2 ' Effective 12/23/2024, dims 31.00 | pkg 29.00 | diff -2.00
                    Case Is >= 5
                        charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_UPS.AddlHand, "Additional Handling - " & Type, "OVS3_Cost", "OVS3_Charge", "costAH1", "AH1")
                        weightDiff = 16.75 ' Effective 12/23/2024, dims 36.00 | weight 52.75 | diff 16.75
                        packagingDiff = 5 ' Effective 12/23/2024, dims 36.00 | pkg 31.00 | diff -5.00
                End Select
            End If

            'dimension type is default
            If Type = "Weight" Then
                charge.BaseCost += weightDiff
                charge.DiscountCost += weightDiff
                charge.SellPrice += weightDiff
            ElseIf Type = "Packaging" Then
                charge.BaseCost -= packagingDiff
                charge.DiscountCost -= packagingDiff
                charge.SellPrice -= packagingDiff
            End If

            shipment.SurchargesList.Add(charge)

            If Type = "Dimensions" Then
                'Additional Handling Packages determined by length, width or length plus girth are subject to a minimum billable weight of 40 pounds.
                'Additional Handling determined by weight or packaging/other factors will not be subject to Additional Handling minimum billable weight.
                If shipment.Billable_Weight < 40 Then
                    shipment.Billable_Weight = 40
                    shipment.IsBillableWeight_changed = True
                End If
            End If
        End If
    End Sub

    Private Sub Check_UPS_ResidentialSurcharge(ByRef shipment As ShippingChoiceDefinition)

        If Is_Button_Selected("Residential_Btn", "SHIP") Or shipment.IsResidential Then

            charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_UPS.Resi, "Residential Surcharge", "ACTRESIDENTIAL", "ResidentialSurcharge", "costRES", "chgRES")
            shipment.SurchargesList.Add(charge)
        End If

    End Sub

    Private Sub Check_UPS_SatrudayDelivery(ByRef shipment As ShippingChoiceDefinition)
        If Is_Button_Selected("SatDelivery_Btn", "SHIP") Then
            If Not (Today.DayOfWeek = DayOfWeek.Saturday) Then
                charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_UPS.SatDel, "UPS Saturday Delivery", "ACTSAT", "SAT", "costSAT", "ACTSAT")
                shipment.SurchargesList.Add(charge)
            End If
        End If
    End Sub

    Private Sub Check_UPS_SatrudayPickup(ByRef shipment As ShippingChoiceDefinition)
        If Is_Button_Selected("SatDelivery_Btn", "SHIP") Then

            If (Today.DayOfWeek = DayOfWeek.Saturday) Then

                charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_UPS.SatPU, "UPS Saturday Pickup", "ACTSATPU", "SATPU", "costSATPU", "ActSATPU")
                shipment.SurchargesList.Add(charge)

            End If
        End If
    End Sub

    Private Sub Check_UPS_SignatureType(ByRef shipment As ShippingChoiceDefinition)
        If Is_Button_Selected("DelConf_Btn", "SHIP") Then

            charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_UPS.Sig_DelConf, "UPS Delivery Confirmation", "ACTDELC", "ACK", "costACK1", "ACK1")
            shipment.SurchargesList.Add(charge)

        ElseIf Is_Button_Selected("SigConfirm_Btn", "SHIP") Then

            charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_UPS.Sig_Req, "UPS Signature Required", "ACTDELSIG", "ACK-S", "costACKSIG1", "ACKSIG1")
            shipment.SurchargesList.Add(charge)

        ElseIf Is_Button_Selected("AdultSig_Btn", "SHIP") Then

            charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_UPS.Sig_Adult, "UPS Adult Signature Required", "ACTDELSIGADULT", "DELSIGADULT", "costACKSIG1", "ACKSIG1")
            shipment.SurchargesList.Add(charge)
        End If

    End Sub

    Private Sub Check_UPS_Declared_Value(ByRef shipment As ShippingChoiceDefinition)
        If shipment.DeclaredValue = 0 Then Exit Sub

        Dim TotalCost As Double = 0
        Dim TotalSell As Double = 0
        Dim Description As String = ""


        Read_DecVal_DBFields(shipment)

        'Check Free Insurance Amount
        If shipment.DeclaredValue <= FreeUpTo_Amt Then
            Description = "Declared Value $" & FreeUpTo_Amt & " free"
            GoTo AddDecValCharge
        End If



        'Check Minimum Charge
        If shipment.DeclaredValue <= MinimumInsured_Amt Or EachAdditional_Amt = 0 Then
            Description = "Declared Value $" & shipment.DeclaredValue & " (Min Charge)"
            TotalCost = Min_Cost
            TotalSell = Min_Sell
            GoTo AddDecValCharge
        End If


        TotalSell = (Math.Ceiling((shipment.DeclaredValue - MinimumInsured_Amt) / EachAdditional_Amt) * DV_Sell) + Min_Sell
        TotalCost = (Math.Ceiling((shipment.DeclaredValue - MinimumInsured_Amt) / EachAdditional_Amt) * DV_Cost) + Min_Cost
        Description = "Declared Value $" & shipment.DeclaredValue



AddDecValCharge:

        If shipment.isThirdPartyDecVal Then
            Description = "3rd Party " & Description
        End If

        Dim Scharge As ShippingSurcharge = New ShippingSurcharge

        Scharge.ID = ShippingSurcharge.IDs_UPS.DecVal
        Scharge.Name = Description
        Scharge.BaseCost = TotalCost
        Scharge.DiscountCost = TotalCost
        Scharge.SellPrice = TotalSell
        Scharge.DBField_Manifest_Cost = "costINS1"
        Scharge.DBField_Manifest_Sell = "INS1"
        Scharge.DeclaredValue_Amt = shipment.DeclaredValue

        shipment.SurchargesList.Add(Scharge)
    End Sub

    Public Function Check_UPS_IsLargePackageSurcharge() As Boolean
        Dim shipment As New ShippingChoiceDefinition
        Check_UPS_LargePackageSurcharge(shipment)
        Return shipment.SurchargesList.Count > 0
    End Function

    Private Sub Check_UPS_LargePackageSurcharge(ByRef shipment As ShippingChoiceDefinition)
        Dim addCharge As Boolean = False

        If shipment.IsInternational = False Then
            'DOMESTIC

            'Any side longer then 96" or Length+Girth more then 130"
            If shipment.Length > 96 Or shipment.Width > 96 Or shipment.Height > 96 Or ShipManager.Calculate_Length_Plus_Girth(shipment.Length, shipment.Width, shipment.Height) > 130 Then
                addCharge = True
            End If
        Else
            'INTERNATIONAL

            'Only check if Length+Girth is more then 130"
            If ShipManager.Calculate_Length_Plus_Girth(shipment.Length, shipment.Width, shipment.Height) > 130 Then
                addCharge = True
            End If
        End If

        If addCharge Then

            If shipment.Billable_Weight < 90 Then
                shipment.Billable_Weight = 90
                shipment.IsBillableWeight_changed = True
            End If

            Dim residentialDiff As Double = 0 ' price difference to add if residential based fee

            If shipment.IsInternational Then
                'INTERNATIONAL
                charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_UPS.Oversize, "Large Package Surcharge", "costAHPlus", "AHPlus", "costAHPlus", "AHPlus")

            Else
                'DOMESTIC
                Dim zoneNoInt As Integer = UPS.UPS_GetNumericZone(shipment.Service, shipment.Zone)
                Select Case zoneNoInt
                    Case 2
                        charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_UPS.Oversize, "Oversize Charge", "OVS4_Cost", "OVS4_Charge", "costAHPlus", "AHPlus")
                        residentialDiff = 35 ' Effective 12/23/2024, comm 205.00 | res 240.00 | diff 35.00
                    Case 3 To 4
                        charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_UPS.Oversize, "Oversize Charge", "costAHPlus", "AHPlus", "costAHPlus", "AHPlus")
                        residentialDiff = 35 ' Effective 12/23/2024, comm 225.00 | res 260.00 | diff 35.00
                    Case Is >= 5
                        charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_UPS.Oversize, "Oversize Charge", "OVS5_Cost", "OVS5_Charge", "costAHPlus", "AHPlus")
                        residentialDiff = 47.5 ' Effective 12/23/2024, comm 250.00 | res 297.50 | diff 47.50
                End Select
            End If

            If shipment.IsResidential = True Then
                'Large Package surcharge to domestic residential address is higher
                charge.BaseCost += residentialDiff
                charge.DiscountCost += residentialDiff
                charge.SellPrice += residentialDiff
            End If

            shipment.SurchargesList.Add(charge)

        End If
    End Sub

    Public Function Is_Zip_UPS_DAS(zip As String) As Integer

        ' 0 - No DAS
        ' 1 - DAS
        ' 2 - Extended DAS
        ' 3 - Hawaii DAS
        ' 4 - Alaska DAS

        Dim DAS_Charge As String
        Dim SQL As String



        SQL = "Select Destination_Surcharge from EAS_Definitions WHERE CountryCode='US' and '" & zip & "' Between [LowZip] and [HighZip]"
        DAS_Charge = ExtractElementFromSegment("Destination_Surcharge", IO_GetSegmentSet(gUPSZoneDB, SQL), "")

        Select Case Trim(DAS_Charge)

            Case "Delivery Area Surcharge"
                Return 1

            Case "Delivery Area Surcharge - Extended"
                Return 2

            Case "Remote Area Surcharge"

                If zip.Substring(0, 3) = "967" Or zip.Substring(0, 3) = "968" Then
                    'Hawaii
                    Return 3
                Else
                    'US 48 Remote
                    Return 5
                End If


            Case "Remote Area Surcharge - Extended"
                'Alaska
                Return 4

        End Select

        If Is_Button_Selected("DAS_Btn", "SHIP") Then
            Return 1
        Else
            Return 0
        End If
    End Function

    Private Sub Check_UPS_DAS(ByRef shipment As ShippingChoiceDefinition)
        Select Case shipment.IsDAS
            Case 0
                'No DAS
                Exit Sub

            Case 1
                'DAS
                If shipment.IsResidential Then
                    charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_UPS.DAS_Res, "DAS Residential", "ACTDAS", "DAS", "costDAS1", "DAS1")
                Else
                    charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_UPS.DAS_Comm, "DAS Commercial", "DASCOMM", "aDASCOMM", "costDAS1", "DAS1")
                End If

            Case 2
                'Extended DAS
                If shipment.IsResidential Then
                    charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_UPS.DAS_ResExt, "DAS Extended Residential", "DasExtCost", "DasExtCharge", "costDAS1", "DAS1")
                Else
                    charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_UPS.DAS_CommExt, "DAS Extended Commercial", "DasExtCommCost", "DasExtCommCharge", "costDAS1", "DAS1")
                End If

            Case 3
                'DAS Hawai
                charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_UPS.DAS_HI, "Hawaii Remote Area Surch.", "DasHiCost", "DasHiCharge", "costDAS1", "DAS1")

            Case 4
                'DAS Alaska
                charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_UPS.DAS_AK, "Alaska Remote Area Surch.", "DasAkCost", "DasAkCharge", "costDAS1", "DAS1")

            Case 5
                'Remote US48 
                charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_UPS.DAS_UsRem, "US Remote Area Surch.", "ACTAP", "AP", "costDAS1", "DAS1")

        End Select

        shipment.SurchargesList.Add(charge)

    End Sub

    Private Sub Check_UPS_COD(ByRef shipment As ShippingChoiceDefinition)
        'check if manually added with Button
        If Is_Button_Selected("COD_Btn", "SHIP") Then
            charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_UPS.Cod, "UPS COD", "ACTCOD", "COD", "costCOD1", "COD1")
            shipment.SurchargesList.Add(charge)
        End If
    End Sub

    Private Sub Check_UPS_FuelSurcharge(ByRef shipment As ShippingChoiceDefinition)

        'List of all surcharge ID's that need to be included in the fuel surcharge calculation
        Dim Fuel_Calc_List As List(Of Integer) = New List(Of Integer)

        With Fuel_Calc_List
            .Add(ShippingSurcharge.IDs_UPS.AddlHand) 'additional handling
            .Add(ShippingSurcharge.IDs_UPS.Resi) 'residential
            .Add(ShippingSurcharge.IDs_UPS.SatDel) 'saturday delivery
            .AddRange(New Integer() {ShippingSurcharge.IDs_UPS.Sig_DelConf, ShippingSurcharge.IDs_UPS.Sig_Req, ShippingSurcharge.IDs_UPS.Sig_Adult}) 'Signature Options
            .Add(58) 'oversize fee
            .AddRange(New Integer() {ShippingSurcharge.IDs_UPS.DAS_Res, ShippingSurcharge.IDs_UPS.DAS_Comm, ShippingSurcharge.IDs_UPS.DAS_ResExt, ShippingSurcharge.IDs_UPS.DAS_CommExt, ShippingSurcharge.IDs_UPS.DAS_HI, ShippingSurcharge.IDs_UPS.DAS_AK, ShippingSurcharge.IDs_UPS.DAS_UsRem}) 'DAS Charges

            .Add(ShippingSurcharge.IDs_UPS.SatPU)

        End With


        Set_FuelSurcharge(shipment, Fuel_Calc_List, ShippingSurcharge.IDs_UPS.FuelSC)

    End Sub
#End Region


#Region "USPS"
    '101 - signature confirmation
    '102 - adult signature confirmation
    '103 - declared value
    '104 - certified mail
    '105 - return receipt
    '106 - Nonstandard fee - Length
    '107 - Nonstandard fee - volume

    Private Sub Check_USPS_NonstandardFee(ByRef shipment As ShippingChoiceDefinition)
        If isServiceDomestic(shipment.Service) And shipment.Service <> "USPS-MEDIA" And shipment.Service <> "USPS-PRT-MTR" Then

            Dim lst = New List(Of Double) From {shipment.Length, shipment.Width, shipment.Height}
            Dim longestSide = lst.Max()

            If longestSide > 30 Then
                charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_USPS.NonStand_Len, "Nonstandard Fee - Length > 30 in.", "ACTCTAG", "CTAG", "LabPackCost", "LabPackCharge")
                shipment.SurchargesList.Add(charge)
            ElseIf longestSide > 22 Then
                charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_USPS.NonStand_Len, "Nonstandard Fee - Length > 22 in.", "LabPackCost", "LabPackCharge", "LabPackCost", "LabPackCharge")
                shipment.SurchargesList.Add(charge)
            End If

            Dim cuft As Double
            Dim cuInches As Double
            Calculate_CubicDIM(shipment.Length, shipment.Height, shipment.Width, cuInches, cuft)

            If cuft > 2 Then
                charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_USPS.NonStand_Vol, "Nonstandard Fee - Volume > 2 cu.ft.", "ACTADDRCOR", "ADD-CORRECTION", "costADDRC1", "ADDRC1")
                shipment.SurchargesList.Add(charge)
            End If
        End If


    End Sub

    Private Sub Check_USPS_SignatureType(ByRef shipment As ShippingChoiceDefinition)
        If Is_Button_Selected("SigConfirm_Btn", "SHIP") Then

            charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_USPS.Sig_Conf, "USPS Signature Confirmation", "ACTDELSIG", "ACK-S", "costACKSIG1", "ACKSIG1")
            shipment.SurchargesList.Add(charge)

        ElseIf Is_Button_Selected("AdultSig_Btn", "SHIP") Then

            charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_USPS.Sig_Adult, "USPS Adult Signature Required", "ACTDELSIGADULT", "DELSIGADULT", "costACKSIG1", "ACKSIG1")
            shipment.SurchargesList.Add(charge)
        End If

    End Sub

    Private Sub Check_USPS_DeclaredValue(ByRef shipment As ShippingChoiceDefinition)
        If shipment.DeclaredValue = 0 Then Exit Sub

        Dim TotalCost As Double = 0
        Dim TotalSell As Double = 0
        Dim Description As String = ""


        Read_DecVal_DBFields(shipment)

        'Check Free Insurance Amount
        If shipment.DeclaredValue <= FreeUpTo_Amt Then
            Description = "Declared Value $" & FreeUpTo_Amt & " free"
            GoTo AddDecValCharge
        End If



        'Check Minimum Charge
        If shipment.DeclaredValue <= MinimumInsured_Amt Or EachAdditional_Amt = 0 Then
            Description = "Declared Value $" & shipment.DeclaredValue & " (Min Charge)"
            TotalCost = Min_Cost
            TotalSell = Min_Sell
            GoTo AddDecValCharge
        End If


        TotalSell = (Math.Ceiling((shipment.DeclaredValue - MinimumInsured_Amt) / EachAdditional_Amt) * DV_Sell) + Min_Sell
        TotalCost = (Math.Ceiling((shipment.DeclaredValue - MinimumInsured_Amt) / EachAdditional_Amt) * DV_Cost) + Min_Cost
        Description = "Declared Value $" & shipment.DeclaredValue



AddDecValCharge:

        If shipment.isThirdPartyDecVal Then
            Description = "3rd Party " & Description
        End If

        Dim Scharge As ShippingSurcharge = New ShippingSurcharge

        Scharge.ID = ShippingSurcharge.IDs_USPS.DecVal
        Scharge.Name = Description
        Scharge.BaseCost = TotalCost
        Scharge.DiscountCost = TotalCost
        Scharge.SellPrice = TotalSell
        Scharge.DBField_Manifest_Cost = "costINS1"
        Scharge.DBField_Manifest_Sell = "INS1"
        Scharge.DeclaredValue_Amt = shipment.DeclaredValue

        shipment.SurchargesList.Add(Scharge)

    End Sub

    Private Sub Check_USPS_Certifiedmail(shipment As ShippingChoiceDefinition)
        If Is_Button_Selected("USPS_CertifiedMail_CheckBox", "PrintLabel") Then
            charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_USPS.CertMail, "USPS Certified Mail", "ACTSATPU2", "SATPU2", "costCertifiedMail", "CertifiedMail")
            shipment.SurchargesList.Add(charge)
        End If
    End Sub

    Private Sub Check_USPS_ReturnReceipt(shipment As ShippingChoiceDefinition)
        If Is_Button_Selected("USPS_ReturnReceipt_CheckBox", "PrintLabel") Then
            charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_USPS.RetRcpt, "USPS Return Receipt", "ACTSAT2", "SAT2", "costReturnReceipt", "ReturnReceipt")
            shipment.SurchargesList.Add(charge)
        End If
    End Sub

#End Region

#Region "DHL"

    Private Sub Check_DHL_RestrictedDestination_ElevatedRisk(ByRef shipment As ShippingChoiceDefinition)
        Dim isElevatedRisk As Boolean
        Dim isRestrictedDestination As Boolean
        Dim isExporterValidation As Boolean


        isElevatedRisk = ExtractElementFromSegment("DHLElevatedRisk", DHLZoneSegment, "False")
        isRestrictedDestination = ExtractElementFromSegment("DHLRestrictedDest", DHLZoneSegment, "False")
        isExporterValidation = ExtractElementFromSegment("DHLExporterValidation", DHLZoneSegment, "False")

        If isElevatedRisk Then
            charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_DHL.ElevRisk, "Elevated Risk", "costDHLElevatedRisk", "DHLElevatedRisk", "costDHLElevatedRisk", "DHLElevatedRisk")
            shipment.SurchargesList.Add(charge)
        End If

        If isRestrictedDestination Or isExporterValidation Then
            'exporter validation is removed as separate service and now part of restricted destination.
            charge = Add_SurchargeDetails(shipment, ShippingSurcharge.IDs_DHL.RestrDestination, "Restricted Destination", "costDHLRestrictedDest", "DHLRestrictedDest", "costDHLRestrictedDest", "DHLRestrictedDest")
            shipment.SurchargesList.Add(charge)
        End If


    End Sub

    Private Sub Check_DHL_FuelSurcharge(ByRef shipment As ShippingChoiceDefinition)
        'List of all surcharge ID's that need to be included in the fuel surcharge calculation
        Dim Fuel_Calc_List As List(Of Integer) = New List(Of Integer)
        Fuel_Calc_List.Add(152) 'demand

        Set_FuelSurcharge(shipment, Fuel_Calc_List, ShippingSurcharge.IDs_DHL.FuelSC)
    End Sub

    Private Sub Check_DHL_DeclaredValue(ByRef shipment As ShippingChoiceDefinition)
        If shipment.DeclaredValue = 0 Then Exit Sub

        Dim TotalCost As Double = 0
        Dim TotalSell As Double = 0
        Dim Description As String = ""


        Read_DecVal_DBFields(shipment)

        'Check Free Insurance Amount
        If shipment.DeclaredValue <= FreeUpTo_Amt Then
            Description = "Declared Value $" & FreeUpTo_Amt & " free"
            GoTo AddDecValCharge
        End If



        'Check Minimum Charge
        If shipment.DeclaredValue <= MinimumInsured_Amt Then
            Description = "Declared Value $" & shipment.DeclaredValue & " (Min Charge)"
            TotalCost = Min_Cost
            TotalSell = Min_Sell
            GoTo AddDecValCharge
        End If

        If EachAdditional_Amt = 0 Then
            TotalSell = Min_Sell
            TotalCost = Min_Cost
        Else
            TotalSell = (Math.Ceiling((shipment.DeclaredValue - MinimumInsured_Amt) / EachAdditional_Amt) * DV_Sell) + Min_Sell
            TotalCost = (Math.Ceiling((shipment.DeclaredValue - MinimumInsured_Amt) / EachAdditional_Amt) * DV_Cost) + Min_Cost
        End If


        Description = "Declared Value $" & shipment.DeclaredValue



AddDecValCharge:

        If shipment.isThirdPartyDecVal Then
            Description = "3rd Party " & Description
        End If

        Dim Scharge As ShippingSurcharge = New ShippingSurcharge

        Scharge.ID = ShippingSurcharge.IDs_DHL.DecVal
        Scharge.Name = Description
        Scharge.BaseCost = TotalCost
        Scharge.DiscountCost = TotalCost
        Scharge.SellPrice = TotalSell
        Scharge.DBField_Manifest_Cost = "costINS1"
        Scharge.DBField_Manifest_Sell = "INS1"
        Scharge.DeclaredValue_Amt = shipment.DeclaredValue

        shipment.SurchargesList.Add(Scharge)

    End Sub

    Private Sub Check_DHL_Demand_Surcharge(ByRef shipment As ShippingChoiceDefinition)
        Dim Demand_Region As String
        Dim Peak_Charge As Peak_Surcharge = Nothing
        Dim LocShipment As ShippingChoiceDefinition = shipment
        Dim Factor As Double

        Demand_Region = ExtractElementFromSegment("Demand_Region", DHLZoneSegment, "")

        Peak_Charge = gDHLPeakSurcharges.Find(Function(x) x.Surcharge.Contains(Demand_Region) And x.Service = LocShipment.Service And x.DateFrom <= Today And x.DateTo >= Today)

        If Not IsNothing(Peak_Charge) Then

            Dim Scharge As ShippingSurcharge = New ShippingSurcharge

            Factor = Math.Ceiling(shipment.Billable_Weight)
            If Factor = 0 Then Factor = 1

            Scharge.ID = ShippingSurcharge.IDs_DHL.DemandSC
            Scharge.Name = Peak_Charge.Surcharge
            Scharge.BaseCost = Peak_Charge.Cost * Factor
            Scharge.DiscountCost = Peak_Charge.Cost * Factor
            Scharge.SellPrice = Peak_Charge.Retail * Factor
            Scharge.DBField_Manifest_Cost = "costUPSEarlyAMSurcharge"
            Scharge.DBField_Manifest_Sell = "UPSEarlyAMSurcharge"

            shipment.SurchargesList.Add(Scharge)
        End If

    End Sub

    Private Sub Get_DHL_ZoneSegment(CountryName As String)
        DHLZoneSegment = IO_GetSegmentSet(gDHLZoneDB, "Select * from [DHL-INTL] Where COUNTRY='" & CountryName & "'")

    End Sub

#End Region

End Module
