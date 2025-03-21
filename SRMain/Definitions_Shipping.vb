Public Module Definitions_Shipping

    Public gCallingSKU As String
    Public gConsigneeSegment As String
    Public gShipperSegment As String
    Public gPackagingDB As String
    Public gFlatRatesDB As String
    Public gUPS_Discount_Segment As String
    Public gFedEx_Discount_Segment As String
    Public gFedEx_OneRate_Tables As List(Of String)
    Public gPricingMatrix As List(Of PricingMatrixItem)


    <System.Serializable()>
    Public Class Matrix_Zone
        Public Property Zone As String
        Public Property isSelected As Boolean
    End Class


    Public Class PricingMatrixItem
        Public Property ID As Integer
        Public Property Service As String
        Public Property Carrier As String
        Public Property WeightStart As String
        Public Property WeightEnd As String
        Public Property Zone As String 'Zone field from DB Table
        Public Property Markup As String
        Public Property Status As String
        Public Property ZoneList As List(Of Matrix_Zone) 'List of ALL available Zones for that service
        Public Property WeightList As List(Of String)

    End Class

    Public Class Carrier
        Public Property CarrierID As Integer
        Public Property CarrierName As String
        Public Property CarrierImage As String
        Public Property ServiceList As List(Of ShippingChoiceDefinition)
        Public Property ServiceList_Domestic As List(Of ShippingChoiceDefinition)
        Public Property ServiceList_International As List(Of ShippingChoiceDefinition)
        Public Property ServiceList_Canada As List(Of ShippingChoiceDefinition)
        Public Property ServiceList_Freight As List(Of ShippingChoiceDefinition)
        Public Property Panel_Domestic As Integer
        Public Property Panel_Intl As Integer
        Public Property Panel_Canada As Integer
        Public Property Panel_Freight As Integer

        Public Property Status_Domestic As Integer
        Public Property Status_Intl As Integer
        Public Property Status_Canada As Integer
        Public Property Status_Freight As Integer

        Public Property Status_Current As Integer

        Public Property Packaging_List As List(Of PackagingItem)
        Public Property Selected_Pack_Item As PackagingItem
    End Class

    <System.Serializable()>
    Public Class PackagingItem
        Public Property Disabled As String
        Public Property SettingID As String
        Public Property SettingName As String
        Public Property SettingDesc As String
        Public Property Length As String
        Public Property Height As String
        Public Property Width As String
        Public Property MaxLBs As String
    End Class

    Public Class USPS_FlatRateItem
        Public Property SettingName As String
        Public Property ServiceTypeID As Integer
        Public Property BaseCost As Double
        Public Property BaseRetail As Double
        Public Property PackagingDB_SettingID As Integer
    End Class

    <System.Serializable()>
    Public Class ShippingChoiceDefinition

        Public Property Weight As Double
        Public Property DIM_Weight As Double
        Public Property Billable_Weight As Double
        Public Property IsBillableWeight_changed As Boolean
        Public Property ZipCode As String
        Public Property ShipTo_State As String
        Public Property ShipTo_Country As String
        Public Property Carrier As String
        Public Property Service As String
        Public Property AirOrExpress As Boolean
        Public Property ZoneTable As String
        Public Property Zone As String
        Public Property Length As Double
        Public Property Width As Double
        Public Property Height As Double
        Public Property Packaging As PackagingItem
        Public Property BaseCost As Double
        Public Property DiscountCost As Double
        Public Property Sell As Double
        Public Property TotalBaseCost As Double
        Public Property TotalSell As Double
        Public Property TotalDiscountCost As Double
        Public Property Profit As Double
        Public Property DeliveryDate As Date
        Public ReadOnly Property DeliveryDateStr As String
            Get
                If IsNothing(DeliveryDate) Or DeliveryDate.Year = 1 Then
                    Return ""
                ElseIf DeliveryDate.Hour = 0 AndAlso DeliveryDate.Minute = 0 Then
                    Return DeliveryDate.ToString("MMM d")
                Else
                    Return DeliveryDate.ToString("MMM d" & vbCrLf & "h:mm tt")
                End If
            End Get
        End Property
        Public Property Segment As String
        Public Property SurchargesList As List(Of ShippingSurcharge)
        Public Property IsInternational As Boolean
        Public Property IsResidential As Boolean
        Public Property IsLetter As Boolean
        Public Property IsFlatRate As Boolean
        Public Property DeclaredValue As Double
        Public Property isThirdPartyDecVal As Boolean
        Public Property IsDAS As Long
        Public Property IsFedExHomeDelivery As Boolean
        Public Property ServiceName As String
        Public Property Column As Integer
        Public Property Column_Canada As Integer
        Public Property IsButtonVisible As Visibility
        Public Property BackgroundColor As String
        Public Property ForegroundColor As String

        Public Sub New()
            SurchargesList = New List(Of ShippingSurcharge)
            Packaging = New PackagingItem()
            BackgroundColor = Media.Color.FromRgb(176, 196, 222).ToString() ' "#b0c4de"
            ForegroundColor = Media.Color.FromRgb(0, 0, 0).ToString() ' "#000000" 'black
        End Sub

    End Class


    Public gShippingChoices(28) As ShippingChoiceDefinition
    Public gShipCT As Integer
    Public gSelectedShipmentChoice As ShippingChoiceDefinition
    Public gPackItemList As List(Of POSLine)

    Public Structure MasterShippingTable

        Dim PrimaryKey As Long
        Dim ServiceTable As String
        Dim ZoneTable As String
        Dim Carrier As String
        Dim Segment As String
        Dim International As Boolean
        Dim Level1 As Double
        Dim Level2 As Double
        Dim Level3 As Double
        Dim LevelR As Double
        Dim LetterPercentage As Double
        Dim LetterFee As Double
        Dim PickupDate As Date

    End Structure
    Public gMaster(50) As MasterShippingTable
    Public gMCT As Integer

    Public Structure ZoneRecord

        Dim Lo As Long
        Dim Hi As Long
        Dim LoAlpha As String
        Dim HiAlpha As String
        Dim Zone As String
        Dim Country As String
        Dim Segment As String

    End Structure

    Public Structure ZoneTable

        Dim ZoneName As String
        Dim ZoneCount As Integer
        Dim International As Boolean
        Dim Zones() As ZoneRecord
        Dim dpPath As String
        Dim UseDirectDBAccess As Boolean

    End Structure

    Public gZct As Integer
    Public gZoneTables(100) As ZoneTable

    Public Structure RatesByZone

        Dim Zones() As Double

    End Structure

    Public Structure ServiceTable

        Dim ServiceName As String
        Dim MasterIndex As Integer
        Dim International As Boolean
        Dim LBS As Integer
        Dim RecordCount As Integer
        Dim NonStandard As Boolean
        Dim Rates() As RatesByZone
        Dim ColumnNames() As String
        Dim cCT As Integer
        Dim dpPath As String
        Dim UseDirectDBAccess As Boolean

    End Structure
    Public gSVCct As Integer
    Public gServiceTables(300) As ServiceTable

    Public Structure SortBlock

        Dim Index As Integer
        Dim TextSort As String
        Dim NumberSort As Double
        Dim DateSort As Date

    End Structure
    Public gSortButtons(28) As SortBlock
    Public gSortCT As Integer

    Public Structure ProfitRangeBlock

        Dim Level As Integer
        Dim LO As Double
        Dim HI As Double

    End Structure
    Public gProfitRange(3) As ProfitRangeBlock
    Public gPackageShipped As Boolean

    Public Class Domestic_Zone_ZipRange
        Public LoZip As String
        Public HiZip As String
        Private m_zone As String
        Public Property Zone As String
            Get
                Return m_zone
            End Get
            Set(value As String)
                value = value.ToUpper
                If Not value.StartsWith("ZONE") Then
                    value = "ZONE" & value
                End If
                m_zone = value
            End Set
        End Property

        Sub New()
            LoZip = ""
            HiZip = ""
            Zone = ""
        End Sub
        Sub New(ByVal lZip As String, ByVal hZip As String, ByVal zZone As String)
            LoZip = lZip
            HiZip = hZip
            Zone = zZone
        End Sub
    End Class

    Public Class Domestic_Zone
        Public ZoneName As String
        Public ZoneBase As String
        Public Zones As List(Of Domestic_Zone_ZipRange)

        Sub New()
            ZoneName = ""
            ZoneBase = ""
            Zones = New List(Of Domestic_Zone_ZipRange)
        End Sub

        Sub New(ByVal zName As String)
            ZoneName = zName
            ZoneBase = ""
            Zones = New List(Of Domestic_Zone_ZipRange)
        End Sub

        Sub New(ByVal zName As String, ByVal zBase As String)
            ZoneName = zName
            ZoneBase = zBase
            Zones = New List(Of Domestic_Zone_ZipRange)
        End Sub
    End Class

    Public Class Domestic_Zones
        Inherits List(Of Domestic_Zone)

    End Class

    Public Function Find_Master_Index(ByVal Service As String) As Short
        'Returns the index of the desired service from gMaster
        For index As Integer = 0 To 50
            If gMaster(index).ServiceTable = Service Then
                Return index
            End If
        Next
        Return -1

    End Function


    'used for Show Package details screen
    Public Class ShippingDetails

        Public Property Service As String
        Public Property Sell As String
        Public Property Discount As String
        Public Property Cost As String

    End Class

    Public Function isServiceDomestic(ByRef SVC As String) As Boolean
        If SVC = "SPEEDEE-GND" Or 'SpeeDee
          SVC = "1DAYEAM" Or'----- UPS
          SVC = "1DAY" Or
          SVC = "1DAYSVR" Or
          SVC = "2DAYAM" Or
          SVC = "2DAY" Or
          SVC = "3DAYSEL" Or
          SVC = "COM-GND" Or
          SVC = "FEDEX-1ST" Or'---FedEx
          SVC = "FEDEX-PRI" Or
          SVC = "FEDEX-STD" Or
          SVC = "FEDEX-2DY-AM" Or
          SVC = "FEDEX-2DY" Or
          SVC = "FEDEX-SVR" Or
          SVC = "FEDEX-GND" Or
          SVC = "USPS-EXPR" Or'---USPS
          SVC = "USPS-PRI" Or
          SVC = "USPS-PRI_CubicRate" Or
          SVC = "USPS-PS" Or
          SVC = "USPS-RG" Or
          SVC = "USPS-PRT-MTR" Or
          SVC = "USPS-MEDIA" Or
          SVC = "FirstClass" Or
          SVC = "USPS-GND-ADV" Or
          SVC = "" Then

            Return True
        Else
            Return False
        End If
    End Function

    Public Function isServiceCanadian(ByRef SVC As String) As Boolean
        If SVC = "DHL-INT" Or 'DHL
           SVC = "DHL-INT-DOC" Or
           SVC = "CAN-STD" Or 'UPS
           SVC = "CAN-XPED" Or
           SVC = "CAN-XPRES" Or
           SVC = "CAN-XSVR" Or
           SVC = "FEDEX-CAN" Or 'FedEx
           SVC = "FEDEX-INTP" Or
           SVC = "FEDEX-INTE" Or
           SVC = "FEDEX-INT-1ST" Or
           SVC = "USPS-INTL-EMI" Or 'USPS
           SVC = "USPS-INTL-FCMI" Or
           SVC = "USPS-INTL-GXG" Or
           SVC = "USPS-INTL-PMI" Then

            Return True
        Else
            Return False
        End If
    End Function

    Public Function isServiceFreight(ByRef SVC As String) As Boolean
        If SVC = "FEDEX-FR1" Or
           SVC = "FEDEX-FR2" Or
           SVC = "FEDEX-FR3" Or
           SVC = "FEDEX-FRP" Or
           SVC = "FEDEX-FRE" Then

            Return True
        Else
            Return False
        End If
    End Function

    Public Function isServiceInternational(ByRef SVC As String) As Boolean
        If SVC = "DHL-INT" Or 'DHL------------------
          SVC = "DHL-INT-DOC" Or
          SVC = "FEDEX-INTP" Or 'FedEx--------------
          SVC = "FEDEX-INTE" Or
          SVC = "FEDEX-INT-1ST" Or
          SVC = "FEDEX-CAN" Or
          SVC = "USPS-INTL-EMI" Or 'USPS------------
          SVC = "USPS-INTL-FCMI" Or
          SVC = "USPS-INTL-GXG" Or
          SVC = "USPS-INTL-PMI" Or
          SVC = "WWXSVR" Or 'UPS ------------------
          SVC = "WWXPRES" Or
          SVC = "WWXPED" Or
          SVC = "WWEconomy" Or
          SVC = "CAN-STD" Or
          SVC = "CAN-XPED" Or
          SVC = "CAN-XPRES" Or
          SVC = "CAN-XSVR" Then

            Return True
        Else

            Return False
        End If
    End Function

    Public Sub Load_Shipping_Panel(ByVal type As String)
        Try
            'Order carriers depending on Type
            'Set services to display depending on type

            Select Case type
                Case "Domestic"
                    gCarrierList = gCarrierList.OrderBy(Function(x As Carrier) x.Panel_Domestic).ToList
                    For Each CR As Carrier In gCarrierList
                        CR.ServiceList = DeepCopy(CR.ServiceList_Domestic)
                        CR.Status_Current = CR.Status_Domestic
                    Next

                Case "Puerto Rico"
                    'use international carrier order
                    gCarrierList = gCarrierList.OrderBy(Function(x As Carrier) x.Panel_Intl).ToList

                    For Each CR As Carrier In gCarrierList
                        If CR.CarrierName = "FedEx" Or CR.CarrierName = "DHL" Then
                            'use international services for UPS and USPS
                            CR.ServiceList = DeepCopy(CR.ServiceList_International)
                            CR.Status_Current = CR.Status_Intl
                        Else
                            'use domestic services for UPS and USPS
                            CR.ServiceList = DeepCopy(CR.ServiceList_Domestic)
                            CR.Status_Current = CR.Status_Intl
                        End If
                    Next


                Case "Intl"
                    gCarrierList = gCarrierList.OrderBy(Function(x As Carrier) x.Panel_Intl).ToList
                    For Each CR As Carrier In gCarrierList
                        CR.ServiceList = DeepCopy(CR.ServiceList_International)
                        CR.Status_Current = CR.Status_Intl
                    Next

                Case "Canada"
                    gCarrierList = gCarrierList.OrderBy(Function(x As Carrier) x.Panel_Canada).ToList
                    For Each CR As Carrier In gCarrierList
                        CR.ServiceList = DeepCopy(CR.ServiceList_Canada)
                        CR.Status_Current = CR.Status_Canada
                    Next

                Case "Freight"
                    gCarrierList = gCarrierList.OrderBy(Function(x As Carrier) x.Panel_Freight).ToList
                    For Each CR As Carrier In gCarrierList
                        CR.ServiceList = DeepCopy(CR.ServiceList_Freight)
                        CR.Status_Current = CR.Status_Freight
                    Next

            End Select

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Public Function DeepCopy(ByVal ObjectToCopy As Object) As Object

        Using mem As New IO.MemoryStream

            Dim bf As New Runtime.Serialization.Formatters.Binary.BinaryFormatter
            bf.Serialize(mem, ObjectToCopy)

            mem.Seek(0, IO.SeekOrigin.Begin)

            Return bf.Deserialize(mem)

        End Using

    End Function

    Public Function OrderServices(ByVal ServiceList As List(Of ShippingChoiceDefinition), Optional isCanada As Boolean = False) As List(Of ShippingChoiceDefinition)
        Try
            Dim searchindex As Integer
            Dim searchresultindex As Integer
            Dim SortedList As List(Of ShippingChoiceDefinition) = New List(Of ShippingChoiceDefinition)

            If ServiceList.Count = 0 Then
                Return ServiceList
            End If

            For i As Integer = 0 To 6
                searchindex = i

                If isCanada Then
                    searchresultindex = ServiceList.FindIndex(Function(x As ShippingChoiceDefinition) x.Column_Canada = searchindex)
                Else
                    searchresultindex = ServiceList.FindIndex(Function(x As ShippingChoiceDefinition) x.Column = searchindex)
                End If


                If searchresultindex = -1 Then
                    'create empty button/place holder
                    Dim svc As ShippingChoiceDefinition = New ShippingChoiceDefinition
                    svc.IsButtonVisible = Visibility.Hidden
                    SortedList.Add(svc)

                Else
                    SortedList.Add(ServiceList(searchresultindex))
                End If

            Next

            Return SortedList

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Function

    Public Sub Load_FlatRate_Packaging(ByRef Packaging_list As List(Of PackagingItem), ByRef SQL As String)
        Try
            Dim current_Segment As String
            Dim buffer As String
            Dim item As PackagingItem

            buffer = IO_GetSegmentSet(gPackagingDB, SQL)

            Do Until buffer = ""
                current_Segment = GetNextSegmentFromSet(buffer)

                item = New PackagingItem
                item.Disabled = ExtractElementFromSegment("Disabled", current_Segment)

                item.SettingID = ExtractElementFromSegment("SettingID", current_Segment)
                item.SettingName = ExtractElementFromSegment("SettingName", current_Segment)

                item.SettingDesc = ExtractElementFromSegment("SettingDesc", current_Segment)
                item.Length = ExtractElementFromSegment("Length", current_Segment)
                item.Height = ExtractElementFromSegment("Height", current_Segment)
                item.Width = ExtractElementFromSegment("Width", current_Segment)
                item.MaxLBs = ExtractElementFromSegment("MaxLBs", current_Segment)

                Packaging_list.Add(item)

            Loop

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub


    Public Sub Set_Ship_Button_Color(ByRef svc As ShippingChoiceDefinition)

        Select Case svc.Carrier
            Case "FedEx"
                svc.BackgroundColor = Media.Color.FromRgb(106, 71, 145).ToString() ' "#6a4791"
                svc.ForegroundColor = Media.Color.FromRgb(255, 255, 255).ToString() ' "#ffffff" 'white
            Case "UPS"
                svc.BackgroundColor = Media.Color.FromRgb(100, 65, 23).ToString() ' "#644117"
                svc.ForegroundColor = Media.Color.FromRgb(255, 255, 255).ToString() ' "#ffffff" 'white
            Case "DHL"
                svc.BackgroundColor = Media.Color.FromRgb(255, 204, 0).ToString() ' "#ffcc00"
                svc.ForegroundColor = Media.Color.FromRgb(0, 0, 0).ToString() ' "#000000" 'black
            Case "USPS"
                svc.BackgroundColor = Media.Color.FromRgb(0, 75, 135).ToString() ' "#004b87"
                svc.ForegroundColor = Media.Color.FromRgb(255, 255, 255).ToString() ' "#ffffff" 'white
            Case "SPEE-DEE"
                svc.BackgroundColor = Media.Color.FromRgb(128, 128, 128).ToString() ' "#004b87"
                svc.ForegroundColor = Media.Color.FromRgb(255, 255, 255).ToString() ' "#ffffff" 'white
            Case Else
                svc.BackgroundColor = Media.Color.FromRgb(176, 196, 222).ToString() ' "#b0c4de"
                svc.ForegroundColor = Media.Color.FromRgb(0, 0, 0).ToString() ' "#000000" 'black
        End Select

    End Sub


End Module
