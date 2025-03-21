Imports System
Imports System.IO
Imports System.Net
Imports System.Net.Sockets

Module General
    Public FillQty_Packmaster As Double
    Public WrapQty_Packmaster As Double
    Public LaborQty_Packmaster As Double
    Public BoxQty_Packmaster As Double
    Public DoubleBoxQty_Packmaster As Double
    Public OtherQty_Packmaster As Double

    Public Function XML_GetNode(XMLbuf As String, NodeName As String) As String

        Dim Name As String = "<" & NodeName & ">"
        Dim NLen As Integer = Len(Name)
        Dim iloc As Integer = InStr(1, XMLbuf, Name)

        Dim eName As String = "</" & NodeName & ">"
        Dim eloc As Integer = InStr(1, XMLbuf, eName)

        If iloc = 0 Or eloc = 0 Then

            Return ""
            Exit Function

        End If
        Return Mid(XMLbuf, iloc + NLen - 1, eloc - (iloc + NLen))

    End Function

    Public Function GetPackageID() As String

        Dim date1 As DateTime = "01/01/2015"
        Dim date2 As DateTime = Now
        Dim ticks As Long = DateDiff(DateInterval.Second, date1, date2)

        Return Hex(ticks)

    End Function
    Public Function Update_ReportWriter_Setup() As Integer

        Dim SQL As String
        Dim Segment As String
        Dim ret As Integer

        SQL = "SELECT * FROM Setup WHERE ID = 1"
        Segment = IO_GetSegmentSet(gReportWriter, SQL)
        If Not Segment = "" Then

            SQL = "UPDATE Setup SET Name = '" & GetPolicyData(gShipriteDB, "Name") & "', "
            SQL += "Addr1 = '" & GetPolicyData(gShipriteDB, "Addr1") & "', "
            SQL += "City = '" & GetPolicyData(gShipriteDB, "City") & "', "
            SQL += "State = '" & GetPolicyData(gShipriteDB, "State") & "', "
            SQL += "Zip = '" & GetPolicyData(gShipriteDB, "Zip") & "', "
            SQL += "Phone1 = '" & GetPolicyData(gShipriteDB, "Phone1") & "' WHERE ID = 1"

        Else

            SQL = "INSERT INTO Setup (ID, Name, Addr1, City, State, Zip, Phone1) VALUES "
            SQL += "("
            SQL += "1,"
            SQL += "'" & GetPolicyData(gShipriteDB, "Name") & "',"
            SQL += "'" & GetPolicyData(gShipriteDB, "Addr1") & "',"
            SQL += "'" & GetPolicyData(gShipriteDB, "City") & "',"
            SQL += "'" & GetPolicyData(gShipriteDB, "State") & "',"
            SQL += "'" & GetPolicyData(gShipriteDB, "Zip") & "',"
            SQL += "'" & GetPolicyData(gShipriteDB, "Phone1") & "'"
            SQL += ")"

        End If
        ret = IO_UpdateSQLProcessor(gReportWriter, SQL)
        Return 0

    End Function

    Public Function GetNextCounter(DataPath As String, eName As String, TName As String) As Long

        Dim Counter As Long
        Dim SQL As String
        Dim SegmentSet As String

        SQL = "SELECT MAX(" & eName & ") AS MaxID FROM " & TName
        SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
        Counter = Val(ExtractElementFromSegment("MaxID", SegmentSet))
        If Counter = 0 Then

            Counter = 1000

        End If
        Return Counter + 1

    End Function

    Public Function Update_Invoice_Balance(InvNum As String) As Double
        Try
            Dim SQL As String
            Dim Segment As String
            Dim SaleAmount As Double
            Dim PaidAmount As Double
            Dim Balance As Double = 0

            SQL = "SELECT Charge, Payment, Type FROM Payments WHERE InvNum = '" & InvNum & "' AND ([Type] = 'Sale' or [Type]='ADJUST' or [Type]='Refund')"
            Segment = IO_GetSegmentSet(gShipriteDB, SQL)
            If ExtractElementFromSegment("Type", Segment) = "Refund" Then
                'Refund
                SaleAmount = Val(ExtractElementFromSegment("Payment", Segment))
                SaleAmount = SaleAmount * -1
            Else
                'Regular Sale
                SaleAmount = Val(ExtractElementFromSegment("Charge", Segment))
            End If
            SQL = "SELECT SUM(Charge) AS Charged, SUM(Payment) AS Paid FROM Payments WHERE InvNum = '" & InvNum & "' AND NOT [Type] = 'Sale' AND NOT [Type] = 'Refund'"
            Segment = IO_GetSegmentSet(gShipriteDB, SQL)
            PaidAmount = Val(ExtractElementFromSegment("Paid", Segment)) - Val(ExtractElementFromSegment("Charged", Segment))

            Balance = Round(SaleAmount - PaidAmount, 2)

            If Not Balance = 0 Then
                SQL = "UPDATE PAYMENTS Set SaleAmount = " & SaleAmount & ", Balance = " & Balance & " WHERE  InvNum = '" & InvNum & "'"

            Else
                SQL = "UPDATE PAYMENTS Set SaleAmount = " & SaleAmount & ", Balance = NULL WHERE  InvNum = '" & InvNum & "'"

            End If
            IO_UpdateSQLProcessor(gShipriteDB, SQL)

            Return Balance

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error Updating Balance.")
        End Try
        Return 0

    End Function

    Public Function ClearReportWriterTables() As Integer

        Dim SQL As String
        Dim ret As Long

        SQL = "DELETE * FROM Contacts"
        ret = IO_UpdateSQLProcessor(gReportWriter, SQL)
        SQL = "DELETE * FROM Payments"
        ret = IO_UpdateSQLProcessor(gReportWriter, SQL)
        SQL = "DELETE * FROM Transactions"
        ret = IO_UpdateSQLProcessor(gReportWriter, SQL)
        SQL = "DELETE * FROM AR"
        ret = IO_UpdateSQLProcessor(gReportWriter, SQL)
        SQL = "DELETE * FROM Inventory"
        ret = IO_UpdateSQLProcessor(gReportWriter, SQL)
        SQL = "DELETE * FROM ARAging"
        ret = IO_UpdateSQLProcessor(gReportWriter, SQL)
        SQL = "DELETE * FROM Cash"
        ret = IO_UpdateSQLProcessor(gReportWriter, SQL)
        SQL = "DELETE * FROM [Check]"
        ret = IO_UpdateSQLProcessor(gReportWriter, SQL)
        SQL = "DELETE * FROM Charge"
        ret = IO_UpdateSQLProcessor(gReportWriter, SQL)
        SQL = "DELETE * FROM Other"
        ret = IO_UpdateSQLProcessor(gReportWriter, SQL)
        SQL = "DELETE * FROM Accounts"
        ret = IO_UpdateSQLProcessor(gReportWriter, SQL)
        SQL = "DELETE * FROM Refund"
        ret = IO_UpdateSQLProcessor(gReportWriter, SQL)
        Return 0

    End Function

    Public Function GetReceiptOptions() As String

        Dim Segment As String
        Dim ShippingReceiptOptions As String
        Dim count As Int16 = 1

        Segment = ""
        Segment = AddElementToSegment(Segment, "PrintTotalCash", GetPolicyData(gShipriteDB, "PrintTotalCash"))
        Segment = AddElementToSegment(Segment, "PrintTotalCC", GetPolicyData(gShipriteDB, "PrintTotalCC"))
        Segment = AddElementToSegment(Segment, "PrintTotalCheck", GetPolicyData(gShipriteDB, "PrintTotalCheck"))
        Segment = AddElementToSegment(Segment, "PrintTotalOther", GetPolicyData(gShipriteDB, "PrintTotalOther"))
        Segment = AddElementToSegment(Segment, "PrintTotalAccount", GetPolicyData(gShipriteDB, "PrintTotalAccount"))
        ShippingReceiptOptions = GetPolicyData(gShipriteDB, "ReceiptOnOffOptions", "11111")
        For Each c As Char In ShippingReceiptOptions

            Select Case count
                Case 1
                    Segment = AddElementToSegment(Segment, "ConsigneeName", "True")
                Case 2
                    Segment = AddElementToSegment(Segment, "ConsigneeStreet", "True")
                Case 3
                    Segment = AddElementToSegment(Segment, "ConsigneeCSZ", "True")
                Case 4
                    Segment = AddElementToSegment(Segment, "Dimensions", "True")
                Case 5
                    Segment = AddElementToSegment(Segment, "ChargeableWeight", "True")

            End Select
            count += 1

        Next
        Segment = AddElementToSegment(Segment, "ReceiptSignatureText", GetPolicyData(gShipriteDB, "ReceiptSignatureText"))
        Segment = AddElementToSegment(Segment, "EnableShippingDisclaimer", GetPolicyData(gShipriteDB, "EnableShippingDisclaimer"))
        Segment = AddElementToSegment(Segment, "ShippingDisclaimer", GetPolicyData(gShipriteDB, "ShippingDisclaimer"))
        Segment = AddElementToSegment(Segment, "ShippingDisclaimer_2ndReceipt", GetPolicyData(gShipriteDB, "ShippingDisclaimer_2ndReceipt"))

        Return Segment

    End Function

    Public Function OpenUserLogin(callingWindow As Window, DB_PermissionField As String, Optional IsGlobal_Enabled As Boolean = True) As Boolean
        If IsGlobal_Enabled = False Then
            Return True

        Else
            Dim wind As New UserLogIn(callingWindow, DB_PermissionField)
            wind.ShowDialog()

            If UserLogIn.isAllowed = False Then
                Return False
            Else
                Return True
            End If
        End If

    End Function


    'Checks permission of gCurrentUser without opening the log in window. Use if user is already logged in. 
    Public Function Check_Current_User_Permission(DB_PermissionField As String, Optional supressWarning As Boolean = False) As Boolean
        Try

            If gCurrentUser = "" Then
                Dim win As New UserLogIn()
                win.ShowDialog()
            End If

            Dim User_Segment As String

            User_Segment = GetNextSegmentFromSet(IO_GetSegmentSet(gShipriteDB, "SELECT * From Users Where [DisplayName]='" & gCurrentUser & "'"))

            If User_Segment = "" Then
                MsgBox("User not Found!", vbOKOnly + vbInformation, "Access Denied!")
                Return False
            End If


            If ExtractElementFromSegment(DB_PermissionField, User_Segment) = True Then
                Return True
            Else
                If supressWarning = False Then
                    MsgBox("User " & ExtractElementFromSegment("DisplayName", User_Segment) & " does NOT have permission to access this feature!", vbOKOnly + vbInformation, "Access Denied!")
                End If

                Return False
            End If

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
        Return False
    End Function

    Structure PackItem

        Dim L() As String
        Dim InventorySegment() As String

    End Structure

    Structure Items

        Dim ItemName As String
        Dim SKU As PackItem
        Dim Units As PackItem
        Dim ShipSegment() As String
        Dim BasePackagingWeight() As Double
        Dim PackagingWeight() As Double
        Dim DefaultPieceCharge As Double   '  I put this value here so that it would be initialized if setup was used.  Only index 0 will be used.  Which is not used for a category
        Dim DefaultDoubleBoxLabor As Double
        Dim DefaultDoubleBoxFill As Double
        Dim DoubleBoxThreshold As Double

    End Structure

    Public gItemSet As Items()
    Public gItemSetIntialized As Boolean
    Public gFCT As Integer

    Public Function AddPOSLine(i As Integer, j As Integer, PClass As String) As Integer
        'Used for PackMaster
        Dim sell As Double = 0
        If gItemSet(j).Units.L(i) = 0 Then

            Return 0

        Else
            Dim LineItem As New POSLine
            LineItem.ID = 0                       ' New POS Line 0 = new sale, non 0 is recovered invoice do update
            LineItem.SKU = gItemSet(j).SKU.L(i)
            LineItem.ModelNumber = gItemSet(j).SKU.L(i)
            LineItem.Department = ExtractElementFromSegment("Department", gItemSet(j).SKU.InventorySegment(i))
            LineItem.Description = ExtractElementFromSegment("Desc", gItemSet(j).SKU.InventorySegment(i))
            LineItem.UnitPrice = Val(ExtractElementFromSegment("Sell", gItemSet(j).SKU.InventorySegment(i)))

            Select Case PClass
                Case "Labor"
                    LineItem.Quantity = LaborQty_Packmaster / 60
                Case "Fill"
                    LineItem.Quantity = FillQty_Packmaster
                Case "Wrap"
                    LineItem.Quantity = WrapQty_Packmaster
                Case "Box"
                    LineItem.Quantity = BoxQty_Packmaster
                Case "DoubleBox"
                    LineItem.Quantity = DoubleBoxQty_Packmaster
            End Select


            LineItem.ExtPrice = LineItem.UnitPrice * LineItem.Quantity
            LineItem.STax = LineItem.ExtPrice * (gPOSHeader.TaxRate / 100)
            LineItem.LTotal = Round(LineItem.ExtPrice + LineItem.STax, 4)
            LineItem.TaxCounty = gPOSHeader.TaxCounty
            LineItem.TRate = gPOSHeader.TaxRate
            LineItem.BrandName = gStoreName
            LineItem.Category = "PACKAGING"
            LineItem.AcctName = ExtractElementFromSegment("Name", gCustomerSegment)
            LineItem.AcctNum = ExtractElementFromSegment("AR", gCustomerSegment)
            LineItem.SoldToID = Val(ExtractElementFromSegment("ID", gCustomerSegment))
            LineItem.ShipToID = Val(ExtractElementFromSegment("ID", gShipToCustomerSegment))
            LineItem.PackMaster = True

            If isOpen_ShipNew = True Then
                'packmaster opened from SHIP, don't add to POS until shipment is completed.
                If IsNothing(gPackItemList) Then
                    gPackItemList = New List(Of POSLine)
                End If

                gPackItemList.Add(LineItem)
            Else
                'packmaster opened from POS
                POSLines.Add(LineItem)
            End If


        End If
        Return 0

    End Function

    Public Function PostPackagingToPOS(i) As Integer

        Dim j As Integer = 0
        Dim DoubleBox As String = ""
        Dim POSSegment As String = ""
        Dim ret As Integer = 0
        gPackItemList = Nothing

        ' Start with double box if present
        j = GetIndexOfMaterials("ShipSegment")
        DoubleBox = ExtractElementFromSegment("DoubleBox", gItemSet(j).ShipSegment(i))
        If DoubleBox = "True" Then

            j = GetIndexOfMaterials("DoubleBox")
            ret = AddPOSLine(i, j, "DoubleBox")

        End If

        ' Inner Box

        j = GetIndexOfMaterials("Box")
        ret = AddPOSLine(i, j, "Box")

        ' Fill

        j = GetIndexOfMaterials("Fill")
        ret = AddPOSLine(i, j, "Fill")

        ' Wrap

        j = GetIndexOfMaterials("Wrap")
        ret = AddPOSLine(i, j, "Wrap")

        ' labor

        j = GetIndexOfMaterials("Labor")
        ret = AddPOSLine(i, j, "Labor")

        Return 0

    End Function

    Public Function GetIndexOfMaterials(NameOfMaterial As String) As Integer
        Dim J As Integer

        For J = 1 To gFCT

            If gItemSet(J).ItemName = NameOfMaterial Then

                Exit For

            End If

        Next J
        If J > gFCT Then

            J = -1

        End If
        Return J

    End Function

    Public Function Packmaster_PostPackageToPOS(PIndex As Integer) As Integer

        Dim LineItem As New POSLine
        LineItem.ID = 0                       ' New POS Line 0 = new sale, non 0 is recovered invoice do update
        LineItem.SKU = gSelectedShipmentChoice.Service
        LineItem.ModelNumber = LineItem.SKU
        LineItem.Description = ExtractElementFromSegment("DESCRIPTION", gSelectedShipmentChoice.Segment)
        LineItem.UnitPrice = 0
        LineItem.Quantity = 1
        LineItem.ExtPrice = LineItem.UnitPrice * LineItem.Quantity
        LineItem.STax = 0
        LineItem.LTotal = LineItem.ExtPrice + LineItem.STax
        LineItem.TaxCounty = gPOSHeader.TaxCounty
        LineItem.TRate = gPOSHeader.TaxRate
        LineItem.BrandName = gSelectedShipmentChoice.Carrier
        LineItem.Category = "SHIPPING"
        LineItem.AcctName = ExtractElementFromSegment("Name", gCustomerSegment)
        LineItem.AcctNum = ExtractElementFromSegment("AR", gCustomerSegment)
        LineItem.SoldToID = Val(ExtractElementFromSegment("ID", gCustomerSegment))
        LineItem.ShipToID = Val(ExtractElementFromSegment("ID", gShipToCustomerSegment))
        POSLines.Add(LineItem)

        Return 0

    End Function

    '    Public Const LVM_FIRST = &H1000
    '    Public Const LVM_GETCOUNTPERPAGE As Long = (LVM_FIRST + 40)
    '    Public Declare Function SendMessage Lib "user32" Alias "SendMessageA" (ByVal hwnd As IntPtr, ByVal wMsg As Long, ByVal wParam As Long, ByVal lParam As Long) As Long

    Public Function GetInventorySegment(SKU As String, Category As String, ilevel As Integer) As String

        Dim SQL As String = ""
        Dim Segment As String = ""
        Dim SegmentSet As String = ""
        Dim i As Integer = 0

        For i = 1 To 6

            If gItemSet(i).ItemName = Category Then

                Segment = gItemSet(i).SKU.InventorySegment(ilevel)
                Exit For

            End If

        Next i
        If Segment = "" Then

            SQL = "SELECT * FROM Inventory WHERE SKU = '" & SKU & "'"
            SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
            Segment = GetNextSegmentFromSet(SegmentSet)

        End If
        Return Segment

    End Function

    Public Function FragilityCalculator(FLevel As String, ShipSegment As String) As String
        Dim SQL As String = ""
        Dim Segment As String = ""
        Dim SegmentSet As String = ""
        Dim SegmentValid As String = ""
        Dim Category As String = ""
        Dim iFLevel As Integer = 0
        Dim i As Integer = 0
        Dim j As Integer = 0
        Dim PackagingWeight As Double = 0
        Dim buf As String = ""
        Select Case FLevel

            Case "NOT FRAGILE"

                iFLevel = 1

            Case "NOT TOO FRAGILE"

                iFLevel = 2

            Case "FRAGILE"

                iFLevel = 3

            Case "VERY FRAGILE"

                iFLevel = 4

            Case "EXTREME FRAGILE"

                iFLevel = 5

        End Select
        If gItemSetIntialized = False Then ' Run this once.  Reduces the need to read the database for every shipment.  Only initialized when program restarted or Setup entered.
            ReDim gItemSet(20)
            gItemSet(0).DoubleBoxThreshold = Val(GetPolicyData(gShipriteDB, "DoubleBoxThreshold", "0"))
            gItemSet(0).DefaultPieceCharge = Val(GetPolicyData(gShipriteDB, "defaultPieceCharge", "0"))
            gItemSet(0).DefaultDoubleBoxLabor = Val(GetPolicyData(gShipriteDB, "defaultLabor", "0"))
            gItemSet(0).DefaultDoubleBoxFill = Val(GetPolicyData(gShipriteDB, "defaultFill", "0"))
            For i = 1 To 20

                ReDim gItemSet(i).SKU.L(5)
                ReDim gItemSet(i).SKU.InventorySegment(5)
                ReDim gItemSet(i).Units.L(5)
                ReDim gItemSet(i).BasePackagingWeight(5)
                ReDim gItemSet(i).PackagingWeight(5)

            Next
            gFCT = 1
            SQL = "SELECT * FROM PackMasterFragility ORDER BY ItemName, ID"
            SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)

            Do Until SegmentSet = ""

                ' The SKU is always first

                Segment = GetNextSegmentFromSet(SegmentSet)
                Category = ExtractElementFromSegment("ItemName", Segment)
                If Category = "DecValueUnit" Then

                    GoTo SkipThisOne

                End If
                Category = FlushOut(Category, "SKU", "")
                Category = FlushOut(Category, "Unit", "")
                gItemSet(gFCT).ItemName = Category

                For i = 1 To 5

                    gItemSet(gFCT).SKU.L(i) = ExtractElementFromSegment("Fragile_L" & i.ToString, Segment)
                    gItemSet(gFCT).SKU.InventorySegment(i) = GetInventorySegment(gItemSet(gFCT).SKU.L(i), Category, i)
                    gItemSet(gFCT).BasePackagingWeight(i) = Val(ExtractElementFromSegment("Weight", gItemSet(gFCT).SKU.InventorySegment(i)))

                Next

                ' Units are second

                Segment = GetNextSegmentFromSet(SegmentSet)

                For i = 1 To 5


                    gItemSet(gFCT).Units.L(i) = Val(ExtractElementFromSegment("Fragile_L" & i.ToString, Segment))

                Next

                gFCT += 1

SkipThisOne:

            Loop
            ' This adds boxes and double boxing to the table

            gItemSet(gFCT).ItemName = "Box"
            For i = 1 To 5

                gItemSet(gFCT).Units.L(i) = 1

            Next i
            gFCT += 1
            gItemSet(gFCT).ItemName = "DoubleBox"
            For i = 1 To 5

                gItemSet(gFCT).Units.L(i) = 1

            Next i
            gItemSetIntialized = True
            gFCT += 1
            gItemSet(gFCT).ItemName = "ShipSegment"
            ReDim gItemSet(gFCT).ShipSegment(5)
            For i = 1 To 5

                gItemSet(gFCT).ShipSegment(i) = ""

            Next
            gFCT += 1

        End If

        gItemSet(gFCT).ItemName = "PackagingWeight"

        Dim LaborSKU As String = ""
        Dim LaborUnit As Double = 0
        Dim FillSKU As String = ""
        Dim FillUnit As Double = 0
        Dim WrapSKU As String = ""
        Dim WrapUnit As Double = 0
        Dim OtherSKU As String = ""
        Dim OtherUnit As Double = 0
        Dim L As Integer = 0
        Dim W As Integer = 0
        Dim H As Integer = 0
        Dim DecVal As Integer = 0
        Dim DefaultFill As Integer = 0
        Dim retLL As Integer = 0
        Dim retWW As Integer = 0
        Dim retHH As Integer = 0

        For i = 1 To 4

            Select Case gItemSet(i).ItemName

                Case "Fill"
                    FillSKU = gItemSet(i).SKU.L(iFLevel)
                    FillUnit = Val(gItemSet(i).Units.L(iFLevel))

                Case "Labor"

                    LaborSKU = gItemSet(i).SKU.L(iFLevel)
                    LaborUnit = Val(gItemSet(i).Units.L(iFLevel))

                Case "Wrap"

                    WrapSKU = gItemSet(i).SKU.L(iFLevel)
                    WrapUnit = Val(gItemSet(i).Units.L(iFLevel))

                Case "Other"

                    OtherSKU = gItemSet(i).SKU.L(iFLevel)
                    OtherUnit = Val(gItemSet(i).Units.L(iFLevel))

            End Select

        Next

        ' Initialize weight
        PackagingWeight = 0

        ' Set double boxing if necessary

        DecVal = Val(ExtractElementFromSegment("DecVal", ShipSegment))


        If DecVal >= gItemSet(0).DoubleBoxThreshold Then

            ShipSegment = AddElementToSegment(ShipSegment, "DoubleBox", "True")


        Else
            If ExtractElementFromSegment("DoubleBox", ShipSegment) = "True" Then
                ShipSegment = AddElementToSegment(ShipSegment, "DoubleBox", "True")
            Else
                ShipSegment = AddElementToSegment(ShipSegment, "DoubleBox", "False")

            End If


        End If

        ' Add Fill to dims



        L = Val(ExtractElementFromSegment("L", ShipSegment)) + FillUnit

        W = Val(ExtractElementFromSegment("W", ShipSegment)) + FillUnit
        H = Val(ExtractElementFromSegment("H", ShipSegment)) + FillUnit

        ' Pick a box 
        Dim MaterialsClass As String = "Boxes"

        SQL = "SELECT SKU, L, W, H, Sell, Desc, Weight FROM Inventory WHERE PackagingMaterials = True AND ([Zero] = False OR ([Zero] = True AND [Quantity] > 0)) AND MaterialsClass = """ & MaterialsClass & """ ORDER BY (L * W * H)"

        SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)

        Do Until SegmentSet = ""
            Segment = GetNextSegmentFromSet(SegmentSet)
            retLL = ExtractElementFromSegment("L", Segment)
            retWW = ExtractElementFromSegment("W", Segment)
            retHH = ExtractElementFromSegment("H", Segment)
            If L <= retLL AndAlso W <= retWW AndAlso H <= retHH Then
                ' found
                Exit Do
            ElseIf L <= retLL AndAlso W <= retHH AndAlso H <= retWW Then
                ' found
                Segment = ChangeElementValueFromSegment("W", retHH, Segment)
                Segment = ChangeElementValueFromSegment("H", retWW, Segment)
                Exit Do
            ElseIf L <= retWW AndAlso W <= retHH AndAlso H <= retLL Then
                ' found

                Segment = ChangeElementValueFromSegment("L", retWW, Segment)
                Segment = ChangeElementValueFromSegment("W", retHH, Segment)
                Segment = ChangeElementValueFromSegment("H", retLL, Segment)

                Exit Do
            ElseIf L <= retWW AndAlso W <= retLL AndAlso H <= retHH Then
                ' found

                Segment = ChangeElementValueFromSegment("L", retWW, Segment)
                Segment = ChangeElementValueFromSegment("W", retLL, Segment)

                Exit Do
            ElseIf L <= retHH AndAlso W <= retLL AndAlso H <= retWW Then
                ' found

                Segment = ChangeElementValueFromSegment("L", retHH, Segment)
                Segment = ChangeElementValueFromSegment("W", retLL, Segment)
                Segment = ChangeElementValueFromSegment("H", retWW, Segment)

                Exit Do
            ElseIf L <= retHH AndAlso W <= retWW AndAlso H <= retLL Then
                ' found

                Segment = ChangeElementValueFromSegment("L", retHH, Segment)
                Segment = ChangeElementValueFromSegment("W", retWW, Segment)
                Segment = ChangeElementValueFromSegment("H", retLL, Segment)

                Exit Do
            End If
        Loop
        If Segment = "" Then

            ShipSegment = AddElementToSegment(ShipSegment, "ERRORMSG", "Cannot find box to fit these dimensions")
            Return ShipSegment
            Exit Function

        End If

        j = GetIndexOfMaterials("Box")
        If Not j = -1 Then

            gItemSet(j).SKU.InventorySegment(iFLevel) = Segment

            gItemSet(j).SKU.L(iFLevel) = ExtractElementFromSegment("SKU", gItemSet(j).SKU.InventorySegment(iFLevel))
            PackagingWeight = PackagingWeight + Val(ExtractElementFromSegment("Weight", gItemSet(j).SKU.InventorySegment(iFLevel)))

            ' Pick a Double Box (start with inside box dimensions)

            DefaultFill = Val(GetPolicyData(gShipriteDB, "DefaultFill", "0"))
            L = Val(ExtractElementFromSegment("L", gItemSet(j).SKU.InventorySegment(iFLevel))) + DefaultFill
            W = Val(ExtractElementFromSegment("W", gItemSet(j).SKU.InventorySegment(iFLevel))) + DefaultFill
            H = Val(ExtractElementFromSegment("H", gItemSet(j).SKU.InventorySegment(iFLevel))) + DefaultFill

        End If
        j = GetIndexOfMaterials("DoubleBox")

        If Not j = -1 Then

            buf = ExtractElementFromSegment("DoubleBox", ShipSegment)

            If buf = "True" Then

                SQL = "SELECT SKU, L, W, H, Sell, Desc, Weight FROM Inventory WHERE PackagingMaterials = True AND ([Zero] = False OR ([Zero] = True AND [Quantity] > 0)) ORDER BY (L * W * H)"

                SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)

                Do Until SegmentSet = ""
                    Segment = GetNextSegmentFromSet(SegmentSet)
                    retLL = ExtractElementFromSegment("L", Segment)
                    retWW = ExtractElementFromSegment("W", Segment)
                    retHH = ExtractElementFromSegment("H", Segment)
                    If L <= retLL AndAlso W <= retWW AndAlso H <= retHH Then
                        ' found
                        Exit Do
                    ElseIf L <= retLL AndAlso W <= retHH AndAlso H <= retWW Then
                        ' found
                        Segment = ChangeElementValueFromSegment("W", retHH, Segment)
                        Segment = ChangeElementValueFromSegment("H", retWW, Segment)
                        Exit Do
                    ElseIf L <= retWW AndAlso W <= retHH AndAlso H <= retLL Then
                        ' found

                        Segment = ChangeElementValueFromSegment("L", retWW, Segment)
                        Segment = ChangeElementValueFromSegment("W", retHH, Segment)
                        Segment = ChangeElementValueFromSegment("H", retLL, Segment)

                        Exit Do
                    ElseIf L <= retWW AndAlso W <= retLL AndAlso H <= retHH Then
                        ' found

                        Segment = ChangeElementValueFromSegment("L", retWW, Segment)
                        Segment = ChangeElementValueFromSegment("W", retLL, Segment)

                        Exit Do
                    ElseIf L <= retHH AndAlso W <= retLL AndAlso H <= retWW Then
                        ' found

                        Segment = ChangeElementValueFromSegment("L", retHH, Segment)
                        Segment = ChangeElementValueFromSegment("W", retLL, Segment)
                        Segment = ChangeElementValueFromSegment("H", retWW, Segment)

                        Exit Do
                    ElseIf L <= retHH AndAlso W <= retWW AndAlso H <= retLL Then
                        ' found

                        Segment = ChangeElementValueFromSegment("L", retHH, Segment)
                        Segment = ChangeElementValueFromSegment("W", retWW, Segment)
                        Segment = ChangeElementValueFromSegment("H", retLL, Segment)

                        Exit Do
                    End If
                Loop
                If Segment = "" Then

                    ShipSegment = AddElementToSegment(ShipSegment, "ERRORMSG", "Cannot find doublebox to fit these dimensions (" & L & " x " & W & " x " & H & ")")
                    Return ShipSegment
                    Exit Function

                End If

                gItemSet(j).SKU.InventorySegment(iFLevel) = Segment
                gItemSet(j).SKU.L(iFLevel) = ExtractElementFromSegment("SKU", gItemSet(j).SKU.InventorySegment(iFLevel))
                PackagingWeight = PackagingWeight + Val(ExtractElementFromSegment("Weight", gItemSet(j).SKU.InventorySegment(iFLevel)))



            Else

                gItemSet(j).SKU.InventorySegment(iFLevel) = ""

            End If

        End If
        j = GetIndexOfMaterials("ShipSegment")
        If Not j = -1 Then

            gItemSet(j).ShipSegment(iFLevel) = ShipSegment

        End If
        j = GetIndexOfMaterials("PackagingWeight")
        If Not j = -1 Then

            gItemSet(j).PackagingWeight(iFLevel) = PackagingWeight

        End If
        Return ShipSegment

    End Function

    Public Function MakeCustomerAccountFromContact(CustomerSegment As String, Optional HaveARNumber As String = "") As String

        Dim ret As Long = 0
        Dim Tally As Long = 0
        Dim ID As Long = 0
        Dim SQL As String = ""
        Dim Segment As String = ""
        Dim SegmentSet As String = ""

        If HaveARNumber = "" Then

            Tally = DateDiff("S", "1/1/2018", Now())
            gResult = gDrawerID & Format$(Tally + 1, "0000000000")

        Else
            gResult = HaveARNumber

        End If

        gCustomerSegment = AddElementToSegment(gCustomerSegment, "AR", gResult)
        ID = Val(ExtractElementFromSegment("ID", gCustomerSegment))
        SQL = "UPDATE Contacts SET AR = '" & gResult & "' WHERE ID = " & ID
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
        SegmentSet = IO_GetSegmentSet(gShipriteDB, "SELECT MAX(ID) AS MaxID FROM AR")
        ID = Val(ExtractElementFromSegment("MaxID", SegmentSet))
        ID = ID + 1
        Segment = ""
        Segment = AddElementToSegment(Segment, "ID", ID)
        Segment = AddElementToSegment(Segment, "AcctNum", gResult)
        Segment = AddElementToSegment(Segment, "AcctName", ExtractElementFromSegment("Name", gCustomerSegment))
        Segment = AddElementToSegment(Segment, "FName", ExtractElementFromSegment("FName", gCustomerSegment))
        Segment = AddElementToSegment(Segment, "LName", ExtractElementFromSegment("LName", gCustomerSegment))
        Segment = AddElementToSegment(Segment, "Addr1", ExtractElementFromSegment("Addr1", gCustomerSegment))
        Segment = AddElementToSegment(Segment, "City", ExtractElementFromSegment("City", gCustomerSegment))
        Segment = AddElementToSegment(Segment, "State", ExtractElementFromSegment("State", gCustomerSegment))
        Segment = AddElementToSegment(Segment, "ZipCode", ExtractElementFromSegment("Zip", gCustomerSegment))
        Segment = AddElementToSegment(Segment, "Phone", ExtractElementFromSegment("Phone", gCustomerSegment))
        Segment = AddElementToSegment(Segment, "TaxStatus", "Taxable")
        Segment = AddElementToSegment(Segment, "SName1", ExtractElementFromSegment("Name", gCustomerSegment))
        Segment = AddElementToSegment(Segment, "SAddr1", ExtractElementFromSegment("Addr1", gCustomerSegment))
        Segment = AddElementToSegment(Segment, "SCity", ExtractElementFromSegment("City", gCustomerSegment))
        Segment = AddElementToSegment(Segment, "SST", ExtractElementFromSegment("State", gCustomerSegment))
        Segment = AddElementToSegment(Segment, "SZip", ExtractElementFromSegment("Zip", gCustomerSegment))
        Segment = AddElementToSegment(Segment, "Country", ExtractElementFromSegment("Country", gCustomerSegment))

        SQL = MakeInsertSQLFromSchema("AR", Segment, gARTableSchema, True)
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
        Return gResult

    End Function
    Public Function Account_Aging(DataPath As String, ANum As String, ByRef BALANCE As Double, ByRef Current As Double, ByRef Plus30 As Double, ByRef Plus60 As Double, ByRef Plus90 As Double, ByRef Plus120 As Double, Optional ForceReCalc As Boolean = False) As Integer
        '  one
        Dim DateC As Date
        Dim Date30 As Date
        Dim Date60 As Date
        Dim Date90 As Date
        Dim Date120 As Date
        Dim Over120 As Double
        Dim SQL As String
        Dim Segment As String
        Dim SegmentSet As String
        Dim buf As String = ""
        Dim ret As Integer
        Dim InvNum As Long
        Dim InvBal As Double
        Dim InvDate As String
        Dim TDate As Date
        Dim Credits As Double
        Dim Charged As Double
        Dim Paid As Double

        If ForceReCalc = False Then

            If ExtractElementFromSegment("AcctNum", gAR) = ANum Then

                SegmentSet = gAR

            Else

                SQL = "SELECT Balance, [Current], Plus30, Plus60, Plus90, Plus120 FROM AR WHERE AcctNum = '" & ANum & "'"
                SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)

            End If

        End If
        BALANCE = 0
        Current = 0
        Plus30 = 0
        Plus60 = 0
        Plus90 = 0
        Plus120 = 0
        If ANum = "" Then

            Return 1
            Exit Function

        End If
        DateC = Date.Today.AddDays(-30)
        Date30 = Date.Today.AddDays(-60)
        Date60 = Date.Today.AddDays(-90)
        Date90 = Date.Today.AddDays(-120)
        Date120 = Date.Today.AddDays(-150)

        SQL = "SELECT SUM(Charge) as Charged, SUM(Payment) AS Paid FROM Payments WHERE AcctNum = '" & ANum & "' AND Status = 'Ok'"
        SegmentSet = IO_GetSegmentSet(DataPath, SQL)

        Charged = Val(Round(Val(ExtractElementFromSegment("Charged", SegmentSet)), 2))
        Paid = Round(Val(ExtractElementFromSegment("Paid", SegmentSet)), 2)
        BALANCE = Charged - Paid

        If Not BALANCE = 0 Then

            SQL = "SELECT InvNum, FIRST([Date]) AS TDate, SUM(Charge) as Charged, SUM(Payment) AS Paid FROM Payments WHERE AcctNum = '" & ANum & "' AND Status = 'Ok' GROUP BY InvNum"
            SegmentSet = IO_GetSegmentSet(DataPath, SQL)
            Credits = 0
            Do Until SegmentSet = ""

                Segment = GetNextSegmentFromSet(SegmentSet)
                InvNum = Val(ExtractElementFromSegment("InvNum", Segment))
                Charged = Round(Val(ExtractElementFromSegment("Charged", Segment)), 2)
                Paid = Val(Round(Val(ExtractElementFromSegment("Paid", Segment)), 2))
                InvBal = Charged - Paid
                InvDate = ExtractElementFromSegment("TDate", Segment)
                TDate = InvDate

                Select Case TDate

                    Case Is >= DateC

                        Current = Current + InvBal

                    Case Is >= Date30

                        Plus30 = Plus30 + InvBal

                    Case Is >= Date60

                        Plus60 = Plus60 + InvBal

                    Case Is >= Date90

                        Plus90 = Plus90 + InvBal

                    Case Else

                        Plus120 = Plus120 + InvBal

                End Select
            Loop

        End If
        Current = Current + Credits
        BALANCE = Round(BALANCE, 2)
        Current = Round(Current, 2)
        Plus30 = Round(Plus30, 2)
        Plus60 = Round(Plus60, 2)
        Plus90 = Round(Plus90, 2)
        Plus120 = Round(Plus120, 2)
        ret = PostAccountAging(ANum, BALANCE, Current, Plus30, Plus60, Plus90, Plus120, Over120)
        Return 0

    End Function

    Public Sub AddPosLineToSet(ID As Long, SKU As String, Segment As String, Optional oPrice As Double = -1234567.89, Optional oQuantity As Double = 1, Optional OverrideDiscount As Double = 0)
        Try
            If SKU = "" Then Exit Sub

            Dim LineItem As New POSLine
            LineItem.ID = ID                       ' New POS Line 0 = new sale, non 0 is recovered invoice do update
            LineItem.SKU = SKU
            LineItem.Department = ExtractElementFromSegment("Department", Segment)
            LineItem.ModelNumber = ExtractElementFromSegment("ModelNumber", Segment)
            LineItem.Description = ExtractElementFromSegment("Desc", Segment)

            If oPrice = -1234567.89 Then
                LineItem.UnitPrice = ExtractElementFromSegment("Sell", Segment, "0")
            Else
                LineItem.UnitPrice = oPrice
                LineItem.isPriceOverride = True
            End If

            LineItem.Quantity = oQuantity
            LineItem.Discount = OverrideDiscount

            If LineItem.Discount = 0 Then
                LineItem.ExtPrice = LineItem.UnitPrice * LineItem.Quantity
            Else
                'LineItem.ExtPrice = (LineItem.UnitPrice * LineItem.Quantity) * (1 - (LineItem.Discount / 100))
                CalculateDiscountPrice(LineItem)
            End If

            'LineItem.STax = (gPOSHeader.TaxRate / 100) * LineItem.ExtPrice
            LineItem.STax = Calculate_SalesTax(LineItem.SKU, LineItem.Department, LineItem.ExtPrice, gPOSHeader.TaxRate)
            LineItem.LTotal = LineItem.ExtPrice + LineItem.STax
            LineItem.TaxCounty = gPOSHeader.TaxCounty
            LineItem.TRate = gPOSHeader.TaxRate
            LineItem.BrandName = ExtractElementFromSegment("BrandName", Segment)
            LineItem.Category = ExtractElementFromSegment("Category", Segment)
            LineItem.SoldToID = Val(ExtractElementFromSegment("ID", gCustomerSegment))
            LineItem.ShipToID = Val(ExtractElementFromSegment("ID", gShipToCustomerSegment))
            LineItem.COGS = Val(ExtractElementFromSegment("Cost", Segment)) * LineItem.Quantity
            LineItem.UnitCost = Val(ExtractElementFromSegment("Cost", Segment))

            POSLines.Add(LineItem)


        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try

    End Sub

    Public Sub Write_Shipment_To_POS(Optional P_ID As String = "", Optional PackCharge As String = "", Optional PackWeight As String = "")

        'P_ID is passed when recovering previous packages. Otherwise gCompletedPackageStack is used for currently processed shipments.

        Dim buf As String
        Dim SegmentSet As String
        Dim Segment As String
        Dim SQL As String
        Dim ret As String
        Dim PackageID As String
        Dim Department As String

        Dim SID As String
        Dim CID As String
        Dim SHIPPER_Segment As String
        Dim CONSIGNEE_Segment As String

        Dim Name As String
        Dim FName As String
        Dim LName As String


        If P_ID = "" Then
            SQL = "SELECT * FROM Manifest WHERE PackageID in (" & gCompletedPackageStack & ") ORDER BY ID"
        Else
            SQL = "SELECT * FROM Manifest WHERE PackageID ='" & P_ID & "' ORDER BY ID"
        End If


        SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)

        '
        Do Until SegmentSet = ""
            Segment = GetNextSegmentFromSet(SegmentSet)


            SID = ExtractElementFromSegment("SID", Segment)
            CID = ExtractElementFromSegment("CID", Segment)
            SHIPPER_Segment = IO_GetSegmentSet(gShipriteDB, "SELECT ID, Name, AR From Contacts WHERE ID=" & SID)
            CONSIGNEE_Segment = IO_GetSegmentSet(gShipriteDB, "SELECT ID, Name, FName, LName, Addr1, Addr2, City, State, Zip From Contacts WHERE ID=" & CID)


            PackageID = ExtractElementFromSegment("PACKAGEID", Segment)
            Department = ExtractElementFromSegment("POSDept", IO_GetSegmentSet(gShipriteDB, "Select POSDept From Master where SERVICE='" & ExtractElementFromSegment("P1", Segment) & "'"), "")

            Dim LineItem As New POSLine()
            LineItem.ID = 0                       ' New POS Line 0 = new sale, non 0 is recovered invoice do update
            LineItem.SKU = ExtractElementFromSegment("P1", Segment)
            LineItem.Department = Department
            LineItem.ModelNumber = ExtractElementFromSegment("P1", Segment) 'LineItem.SKU
            LineItem.Description = ExtractElementFromSegment("ServiceName", Segment)
            LineItem.UnitPrice = Val(ExtractElementFromSegment("T1", Segment))
            LineItem.UnitCost = Val(ExtractElementFromSegment("costT1", Segment))
            LineItem.Quantity = 1
            LineItem.COGS = LineItem.UnitCost * LineItem.Quantity
            LineItem.ExtPrice = LineItem.UnitPrice * LineItem.Quantity
            LineItem.STax = 0
            LineItem.LTotal = LineItem.ExtPrice + LineItem.STax
            LineItem.TaxCounty = gPOSHeader.TaxCounty
            LineItem.TRate = gPOSHeader.TaxRate
            LineItem.BrandName = ExtractElementFromSegment("Carrier", Segment)
            LineItem.Category = "SHIPPING"
            LineItem.AcctName = ExtractElementFromSegment("Name", SHIPPER_Segment)
            LineItem.AcctNum = ExtractElementFromSegment("AR", SHIPPER_Segment)
            LineItem.SoldToID = Val(ExtractElementFromSegment("ID", SHIPPER_Segment))
            LineItem.ShipToID = Val(ExtractElementFromSegment("ID", CONSIGNEE_Segment))
            LineItem.PackageID = PackageID
            POSLines.Add(LineItem)


            Dim ShippingReceiptOptions = GetPolicyData(gShipriteDB, "ReceiptOnOffOptions", "11111")

            'name
            If ShippingReceiptOptions(0) = "1" Then
                Name = ExtractElementFromSegment("Name", CONSIGNEE_Segment, "")
                FName = ExtractElementFromSegment("FName", CONSIGNEE_Segment, "")
                LName = ExtractElementFromSegment("LName", CONSIGNEE_Segment, "")

                If Name = LName & ", " & FName Or (FName = "" And LName = "") Then
                    ret = NoteLineToPOS("..." & Chr(9) & Name, False, PackageID)
                Else
                    ret = NoteLineToPOS("..." & Chr(9) & Name, False, PackageID)

                    If FName = "" Then
                        ret = NoteLineToPOS("..." & Chr(9) & LName, False, PackageID)
                    ElseIf LName = "" Then
                        ret = NoteLineToPOS("..." & Chr(9) & FName, False, PackageID)
                    Else
                        ret = NoteLineToPOS("..." & Chr(9) & LName & ", " & FName, False, PackageID)
                    End If

                End If

            End If

            'street address
            If ShippingReceiptOptions(1) = "1" Then
                ret = NoteLineToPOS("..." & Chr(9) & ExtractElementFromSegment("Addr1", CONSIGNEE_Segment), False, PackageID)

                If ExtractElementFromSegment("Addr2", CONSIGNEE_Segment, "") <> "" Then
                    ret = NoteLineToPOS("..." & Chr(9) & ExtractElementFromSegment("Addr2", CONSIGNEE_Segment), False, PackageID)
                End If
            End If

            'city, state, zip
            If ShippingReceiptOptions(2) = "1" Then
                ret = NoteLineToPOS("..." & Chr(9) & ExtractElementFromSegment("City", CONSIGNEE_Segment) & ", " & ExtractElementFromSegment("State", CONSIGNEE_Segment) & "  " & ExtractElementFromSegment("Zip", CONSIGNEE_Segment), False, PackageID)
            End If

            ret = NoteLineToPOS("...Tracking:" & Chr(9) & ExtractElementFromSegment("TRACKING#", Segment), False, PackageID)
            ret = NoteLineToPOS("...Pack ID:" & Chr(9) & ExtractElementFromSegment("PACKAGEID", Segment), False, PackageID)

            'weight
            If ShippingReceiptOptions(4) = "1" Then
                ret = NoteLineToPOS("...Weight:" & Chr(9) & ExtractElementFromSegment("LBS", Segment) & " lbs", False, PackageID)
            End If


            'dimensions
            If ShippingReceiptOptions(3) = "1" Then
                ret = NoteLineToPOS("...Dims:" & Chr(9) & ExtractElementFromSegment("LENGTH", Segment) & " x " & ExtractElementFromSegment("WIDTH", Segment) & " x " & ExtractElementFromSegment("HEIGHT", Segment), False, PackageID)
            End If

            'weight
            If ShippingReceiptOptions(4) = "1" Then
                ret = NoteLineToPOS("...Dim WT:" & Chr(9) & ExtractElementFromSegment("DIMWEIGHT", Segment) & " lbs", False, PackageID)
            End If


            If ExtractElementFromSegment("DECVAL", Segment, "") <> "" Then
                ret = NoteLineToPOS("...Decl. Value:" & Chr(9) & FormatCurrency(ExtractElementFromSegment("DECVAL", Segment)), False, PackageID)
            End If

            If ExtractElementFromSegment("CONTENTS", Segment, "") <> "" Then
                ret = NoteLineToPOS("...Contents:" & Chr(9) & ExtractElementFromSegment("CONTENTS", Segment), False, PackageID)
            End If

            If Not PackCharge = "" Then
                buf = "...Packaging:" & Chr(9) & ValFix(PackCharge).ToString("$ 0.00") & "  " & PackWeight & " lbs"
                ret = NoteLineToPOS(buf)
            End If

        Loop

        If P_ID = "" Then
            If Not IsNothing(gPackItemList) Then
                For Each line In gPackItemList
                    POSLines.Add(line)
                Next
            End If
        End If

    End Sub

    Public Sub CalculateDiscountPrice(ByRef lineitem As POSLine)
        If Not isItem_NoDiscount(lineitem) Then
            lineitem.ExtPrice = lineitem.UnitPrice * lineitem.Quantity * (1 - (lineitem.Discount / 100))
        Else
            lineitem.ExtPrice = lineitem.UnitPrice * lineitem.Quantity
            lineitem.Discount = 0
        End If
    End Sub

    Public Function isItem_NoDiscount(lineitem As POSLine) As Boolean
        Dim SegmentSet = IO_GetSegmentSet(gShipriteDB, "SELECT NoDiscounting FROM Inventory WHERE SKU='" & lineitem.SKU & "'")
        If (ExtractElementFromSegment("NoDiscounting", SegmentSet, "False")) = True Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Function Calculate_SalesTax(SKU As String, Department As String, ExtPrice As Double, TaxRate As Double) As Double
        If isInventoryItemSet_NonTaxalbe(SKU) Then
            Return 0
        End If

        If isDepartmentTaxable(Department) Then
            Return (TaxRate / 100) * ExtPrice
        Else
            Return 0
        End If
    End Function

    Private Function isInventoryItemSet_NonTaxalbe(SKU As String) As Boolean
        Dim SQL = "SELECT Non_Taxable FROM Inventory WHERE SKU = '" & SKU & "'"
        Dim Segment = IO_GetSegmentSet(gShipriteDB, SQL, "")

        If ExtractElementFromSegment("Non_Taxable", Segment, "False") = True Then
            Return True
        Else
            Return False
        End If

    End Function

    Public Function isDepartmentTaxable(Department As String) As Boolean
        Dim SQL = "SELECT Taxable FROM Departments WHERE Department = '" & Department & "'"
        Dim Segment = IO_GetSegmentSet(gShipriteDB, SQL, "")

        If Segment = "" Then
            Return False 'Department not in Department table
        ElseIf ExtractElementFromSegment("Taxable", Segment, "") = False Then
            Return False
        Else
            Return True
        End If

    End Function

    Public Function NoteLineToPOS(Description As String, Optional isMemo As Boolean = False, Optional PackageID As String = "") As Integer
        Dim LineItem As New POSLine
        LineItem.ID = 0 ' Weight

        If isMemo Then
            LineItem.SKU = "MEMO"
            LineItem.ModelNumber = "MEMO"
            LineItem.BrandName = "MEMO"
            LineItem.Category = "MEMO"
        Else
            LineItem.SKU = "NOTE"
            LineItem.ModelNumber = "NOTE"
            LineItem.BrandName = "NOTE"
            LineItem.Category = "NOTE"
        End If

        LineItem.Description = Description
        LineItem.UnitPrice = 0
        LineItem.Quantity = 0
        LineItem.ExtPrice = 0
        LineItem.STax = 0
        LineItem.LTotal = 0
        LineItem.TaxCounty = gPOSHeader.TaxCounty
        LineItem.TRate = gPOSHeader.TaxRate

        LineItem.AcctName = ExtractElementFromSegment("Name", gCustomerSegment)
        LineItem.AcctNum = ExtractElementFromSegment("AR", gCustomerSegment)
        LineItem.SoldToID = Val(ExtractElementFromSegment("ID", gCustomerSegment))
        LineItem.ShipToID = Val(ExtractElementFromSegment("ID", gShipToCustomerSegment))

        If PackageID <> "" Then
            LineItem.PackageID = PackageID
        End If
        POSLines.Add(LineItem)

        Return 0

    End Function
    Function PostAccountAging(ANum As String, Bal As Double, Cur As Double, P30 As Double, P60 As Double, P90 As Double, P120 As Double, O120 As Double) As Integer

        Dim ret As Integer
        Dim SQL As String
        Dim Segment As String
        Dim ID As Long

        SQL = "SELECT ID FROM AR WHERE AcctNum = '" & ANum & "'"
        Segment = IO_GetSegmentSet(gShipriteDB, SQL)
        If Not Segment = "" Then

            ID = Val(ExtractElementFromSegment("ID", Segment))
            Segment = ""
            Segment = AddElementToSegment(Segment, "ID", ID)
            Segment = AddElementToSegment(Segment, "AcctNum", ANum)
            Segment = AddElementToSegment(Segment, "Balance", Bal)
            Segment = AddElementToSegment(Segment, "Current", Cur)
            Segment = AddElementToSegment(Segment, "Plus30", P30)
            Segment = AddElementToSegment(Segment, "Plus60", P60)
            Segment = AddElementToSegment(Segment, "Plus90", P90)
            Segment = AddElementToSegment(Segment, "Plus120", P120)
            Segment = AddElementToSegment(Segment, "Over120", O120)
            Segment = AddElementToSegment(Segment, "DateOfLastCalculation", Format$(Date.Today, "MM/dd/yyyy"))
            SQL = MakeUpdateSQLFromSchema("AR", Segment, gARTableSchema)
            ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

        Else

            ret = 0

        End If
        PostAccountAging = ret

    End Function

    Public Function LRC_Check(ByVal cData As String) As String

        Dim LRC As Long
        Dim i As Integer

        Return "A"
        Exit Function

        On Error GoTo Ooops
        LRC = 0
        For i = 1 To Len(cData)

            LRC = LRC Xor Asc(Strings.Mid(cData, i, 1))

        Next i%

Ooops:

        On Error GoTo 0
        Return Chr(LRC)

    End Function

    Public Function CheckPortConnection(PortNo As Long) As Integer

        Dim Client As TcpClient = Nothing

        Try

            Client = New TcpClient("Localhost", PortNo)
            Return 0

        Catch ex As SocketException

            Return 7

        Finally

            If Not Client Is Nothing Then

                Client.Close()

            End If

        End Try

    End Function


    Public Function GetNextInvoiceNumber() As String
        Try
            Dim SQL As String
            Dim SegmentSet As String
            Dim InvNum As Long

            'SQL = ""
            'SegmentSet = ""
            'InvNum = 0

            'SQL = "SELECT MAX(NumericInvoiceNumber) as MAXID FROM Payments"
            'SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
            'InvNum = Val(ExtractElementFromSegment("MAXID", SegmentSet))
            'If (InvNum = 0) Then
            '    InvNum = 10000
            'End If
            'Return InvNum + 1



            Dim ret As Integer

            'Do Until Not InvNum = 0

            SQL = "UPDATE Setup SET NextInvoiceNumber = NextInvoiceNumber + 1"
            ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

            SQL = "SELECT NextInvoiceNumber FROM SETUP"
            SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
            InvNum = Val(ExtractElementFromSegment("NextInvoiceNumber", SegmentSet))

            If InvNum < 100 Then
                InvNum = 101

                SQL = "UPDATE Setup SET NextInvoiceNumber = 101"
                ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
            End If



            'SQL = "UPDATE InvoiceNumbers SET [Date] = #" & Today.ToString("MM/dd/yyyy") & "#, SalesRep = '" & SalesRep & "', Status = '" & Status & "', Location = '" & Location & "' WHERE ID = " & InvNum & " AND Status = 'NEW'"
            'ret = IO_UpdateSQLProcessor(gLoggingDB, SQL)
            'If ret = 0 Then

            '    ''AP(09/12/2016){DRN = 1022} - If Logging.mdb has run out of empty trailing records, then add a buffer of 1000 empty trailing records.
            '    SQL = "SELECT ID FROM InvoiceNumbers WHERE ID = " & InvNum
            '    SegmentSet = IO_GetSegmentSet(gLoggingDB, SQL)
            '    If Not SegmentSet = "" Then

            '        InvNum = 0

            '    Else

            '        Hi = InvNum + 1000 ' buffer of 10,000 causes too long of a slow down.
            '        For Lo = InvNum To Hi

            '            SQL = "INSERT INTO InvoiceNumbers ([ID], [Status]) VALUES (" & Lo & ", 'NEW')"
            '            ret = IO_UpdateSQLProcessor(gLoggingDB, SQL)

            '        Next

            '    End If
            '    'InvNum = 0

            'End If

            'Loop
            Return InvNum

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Error getting next invoice number.")
        End Try
    End Function

    Public Function GetSocketOption(lSocket As Long, lLevel As Long, lOption As Long) As Long

        Dim lResult As Long       ' Result of API call.
        Dim lBuffer As Long       ' Buffer to get value into.
        Dim lBufferLen As Long    ' len of buffer.
        Dim linger As LINGER_STRUCT

        ' Linger requires a structure so we will get that option differently.

        If (lOption <> SO_LINGER) And (lOption <> SO_DONTLINGER) Then

            lBufferLen = Len(lBuffer)
            lResult = getsockopt(lSocket, lLevel, lOption, lBuffer, lBufferLen)

        Else

            lBufferLen = Len(linger)
            lResult = getsockopt(lSocket, lLevel, lOption, linger, lBufferLen)
            lBuffer = linger.l_onoff

        End If

        If (lResult = SOCKET_ERROR) Then

            GetSocketOption = Err.LastDllError

        Else

            GetSocketOption = lBuffer

        End If
    End Function

    Public Function ReplaceCharacters(ByVal buf As String, ByRef SearchC As String, ByRef ReplaceC As String) As String

        Dim iloc As Integer
        Dim ct As Long

        iloc = InStr(1, buf, SearchC)
        Do Until iloc = 0 Or ct = 100

            If Not iloc = 1 Then

                buf = Strings.Mid(buf, 1, iloc - 1) & ReplaceC & Strings.Mid(buf, iloc + Len(SearchC))

            Else

                buf = ReplaceC & Strings.Mid(buf, iloc + Len(SearchC))

            End If
            iloc = InStr(1, buf, SearchC)
            '        ct = ct + 1
            '        doevents

        Loop
        Return buf

    End Function

    Public Function GetWinINI(ProgramIndicator As String, IniPath As String, DefaultDirectory As String, GetData As String) As String

        Dim inbuf As String
        Dim InField As String
        Dim indata As String
        Dim iloc As Integer
        Dim MsgBuf As String
        Dim found As Integer
        Dim ifound As Integer

        If Not File.Exists(IniPath) Then

            MsgBuf = "Unable to locate file in 'GetWinINI' using the following arguments..." & vbCrLf & ProgramIndicator & vbCrLf & IniPath & vbCrLf & DefaultDirectory & vbCrLf & GetData
            MsgBox(MsgBuf, vbCritical, gProgramName)
            Return DefaultDirectory

        End If

        found = 0
        ifound = 1
        GetWinINI = ""

        Using sr As StreamReader = File.OpenText(IniPath)

            Do While sr.Peek() >= 0

                inbuf = sr.ReadLine()
                If Not Strings.Mid(inbuf, 1, 1) = ";" And Not Strings.Mid(inbuf, 1, 1) = "#" And Not Trim(inbuf) = ";" Then

                    If found = 1 And Trim(inbuf) = "" Then

                        Exit Do

                    End If
                    If InStr(1, inbuf, ProgramIndicator) > 0 Then

                        found = 1

                    End If
                    If found = 1 And InStr(1, inbuf, GetData) > 0 Then

                        iloc = InStr(1, inbuf, "=")
                        InField = Strings.Mid(inbuf, 1, iloc - 1)
                        indata = Strings.Mid(inbuf, iloc + 1)
                        If InField = GetData Then

                            GetWinINI = indata
                            ifound = 1
                            Exit Do

                        End If

                    End If

                End If

            Loop
            sr.Close()

        End Using

    End Function

    Public Function CreateDisplayBlock(Segment As String, IncludeEmail As Boolean) As String

        Dim buf As String
        Dim eMail As String

        eMail = ""
        buf = ""
        buf = ExtractElementFromSegment("Name", Segment) & vbCrLf
        buf = buf & ExtractElementFromSegment("Addr1", Segment) & vbCrLf

        If ExtractElementFromSegment("Addr2", Segment, "") <> "" Then
            buf = buf & ExtractElementFromSegment("Addr2", Segment) & vbCrLf

            If ExtractElementFromSegment("Addr3", Segment, "") <> "" Then
                buf = buf & ExtractElementFromSegment("Addr3", Segment) & vbCrLf
            End If

        End If

        buf = buf & ExtractElementFromSegment("City", Segment) & ", " & ExtractElementFromSegment("State", Segment) & "   " & ExtractElementFromSegment("Zip", Segment) & vbCrLf
        buf = buf & ExtractElementFromSegment("Phone", Segment) & "   " & ExtractElementFromSegment("Fax", Segment)
        If IncludeEmail = True Then

            eMail = ExtractElementFromSegment("EMail", Segment)
            If eMail = "" Then

                eMail = "No EMail"

            End If
            buf = buf & vbCrLf & eMail

        End If
        Return buf

    End Function

    Public Function FlushOut(ByVal buf As String, ByRef SearchC As String, ByRef ReplaceC As String) As String

        Dim iloc As Integer
        Dim CT As Long

        iloc = InStr(1, buf, SearchC)
        Do Until iloc = 0 Or CT = 100

            If Not iloc = 1 Then

                buf = Strings.Mid$(buf, 1, iloc - 1) & ReplaceC & Strings.Mid$(buf, iloc + Len(SearchC))

            Else

                buf = ReplaceC & Strings.Mid$(buf, iloc + Len(SearchC))

            End If
            iloc = InStr(1, buf, SearchC)

        Loop
        Return buf

    End Function

    Public Function GetNextIDNumber(dbpath As String, Table As String) As Long

        Dim ID As Long
        Dim SQL As String
        Dim SegmentSet As String

        SQL = "SELECT MAX(ID) AS MaxID FROM " & Table
        SegmentSet = IO_GetSegmentSet(dbpath, SQL)
        ID = Val(ExtractElementFromSegment("MaxID", SegmentSet))
        If ID = 0 Then

            ID = 1000

        End If
        ID = ID + 1
        Return ID

    End Function

    ''' <summary>
    ''' Tries to determine what kind of data is in InputBuf
    ''' </summary>
    ''' <param name="InputBuf"></param>
    ''' <returns> Returns an integer. Last, First = 0, Phone = 1, Address = 2, Invoice# = 3</returns>
    Public Function GetInputType(InputBuf As String) As Integer

        Dim iloc As Integer
        Dim buf As String
        Dim ID As Double

        ' Last, First = 0
        ' Phone = 1
        ' Address = 2
        ' Invoice# = 3

        On Error Resume Next
        ID = Val(InputBuf)
        On Error GoTo 0
        buf = ID
        If buf = InputBuf And Not Len(buf) >= 7 Then              '  Check to see if this is a Service Ticket number or Invoice number

            Return 3
            Exit Function

        End If
        iloc = InStr(1, InputBuf, ", ")
        If Not iloc = 0 Then                '  LastName, FirstName

            Return 0
            Exit Function

        End If

        iloc = InStr(1, InputBuf, "-")
        buf = FlushOut(InputBuf, "-", "")
        If IsNumeric(buf) = True And Len(buf) >= 7 And Len(buf) <= 10 Then               '  Phone Number

            Return 1
            Exit Function

        End If

        iloc = InStr(1, InputBuf, " ")
        If Not iloc = 0 Then

            buf = Strings.Mid$(buf, 1, iloc - 1)
            If Val(buf) > 0 Or buf = "%" Then

                Return 2
                Exit Function

            End If

        End If
        If Not InStr(1, InputBuf, "@") = 0 Then

            Return 3
            Exit Function

        End If

    End Function

    Public Function ReformatDate(DT As String) As String

        Dim iloc As Integer
        Dim buf As String
        Dim NewDate As String
        Dim TheDate As Date
        Dim BadDateSwitch As Boolean

        If InStr(1, DT, "AM") > 0 Or InStr(1, DT, "PM") > 0 Then

            Return "01/01/1991"
            Exit Function

        End If
        If Trim$(DT) = "" Then

            Return ""
            Exit Function

        End If
        If Len(DT) <= 2 And IsNumeric(DT) = True Then

            buf = Now.ToString("MM/dd/yyyy")
            buf = Strings.Mid(buf, 1, 3) & Val(DT).ToString("00") & Strings.Mid(buf, 6)
            DT = buf

        End If
        If InStr(1, DT, "/") > 0 Or InStr(1, DT, "-") > 0 Then

            On Error Resume Next
            TheDate = DT
            On Error GoTo 0
            DT = TheDate.ToString("MM/dd/yyyy")

        End If

        buf = FlushOut(DT, "/", "")
        buf = FlushOut(buf, "-", "")
        buf = FlushOut(buf, "_", "")
        buf = FlushOut(buf, ".", "")

        iloc = Len(buf)
        Select Case iloc

            Case 6, 8

                NewDate = Strings.Mid(buf, 1, 2) & "/" & Strings.Mid(buf, 3, 2) & "/" & Strings.Mid(buf, 5)

            Case Else

                MsgBox("ATTENTION...Date Reformat" & vbCrLf & vbCrLf & "Unexpected date format...retry" & vbCrLf & vbCrLf &
            "Try mmddyy, mmddyyyy, mm/dd/yy, or mm/dd/yyyy", vbInformation, gProgramName)
                NewDate = ""

        End Select
        BadDateSwitch = False
        If Not NewDate = "" Then

            On Error GoTo BadDate
            TheDate = NewDate
            On Error GoTo 0
            If BadDateSwitch = False Then

                NewDate = Format$(TheDate, "MM/dd/yyyy")

            Else

                NewDate = ""

            End If

        End If
        Return NewDate
        Exit Function

BadDate:

        BadDateSwitch = True
        MsgBox("ATTENTION...Date Reformat" & vbCrLf & vbCrLf & "Unexpected date format...retry" & vbCrLf & vbCrLf &
    "Try mmddyy, mmddyyyy, mm/dd/yy, or mm/dd/yyyy", vbInformation, gProgramName)
        NewDate = ""
        Resume Next

    End Function

    Public Function ReformatPhone(dbPath As String, PH As String) As String

        Dim word As String
        Dim word2 As String
        Dim word3 As String
        Dim ReturnValue As String
        Dim i As Integer

        ReturnValue = ""
        i = InStr(1, PH, "(")
        If Not i = 0 Then

            PH = FlushOut(PH, "(", "")
            PH = Trim$(FlushOut(PH, ")", ""))

        End If
        PH = Trim$(FlushOut(PH, " ", ""))
        PH = Trim$(FlushOut(PH, "-", ""))

        If Not IsNumeric(PH) = True Then

            Return ""
            Exit Function

        End If
        word = PH
        i = Len(word)
        word2 = "-"
        If i = 10 Then

            word = Strings.Mid$(word, 1, 3) & word2 & Strings.Mid$(word, 4, 3) & word2 & Strings.Mid$(word, 7)
            ReturnValue = word

        End If
        If i = 7 Then

            word = Strings.Mid$(word, 1, 3) & word2 & Strings.Mid$(word, 4)
            word3 = GetPolicyData(dbPath, "DefaultAreaCode")
            If Not word3 = "" Then

                word = word3 & word2 & word

            End If
            ReturnValue = word

        End If
        If i = 12 Then

            ReturnValue = PH

        End If
        If i = 3 Then

            PH = ""
            ReturnValue = ""

        End If
        Return ReturnValue

    End Function

    Public Function ValFix(buf As String) As Double

        Dim amt As Double
        buf = FlushOut(buf, "$", "")
        buf = FlushOut(buf, ",", "")
        amt = Val(buf)
        Return amt

    End Function
    Public Function UpdateRunTimePolicy(pIndex As Integer, ByRef eName As String, ByRef eData As String) As Integer

        gPolicy(pIndex).buf = AddElementToSegment(gPolicy(pIndex).buf, eName, eData)
        UpdateRunTimePolicy = 1

    End Function

    Public Function GetRunTimePolicy(pIndex As Integer, eName As String) As String

        GetRunTimePolicy = ExtractElementFromSegment(eName, gPolicy(pIndex).buf)

    End Function

    Public Function GetIPAddress() As String

        Dim myHost As String = Dns.GetHostName
        Dim ipEntry As IPHostEntry = Dns.GetHostEntry(myHost)
        Dim ip As String = ""

        For Each tmpIpAddress As IPAddress In ipEntry.AddressList
            If tmpIpAddress.AddressFamily = Sockets.AddressFamily.InterNetwork Then
                Dim ipAddress As String = tmpIpAddress.ToString
                ip = ipAddress
                Exit For
            End If
        Next

        If ip = "" Then
            Throw New Exception("No 10. IP found!")
        End If

        Return ip

    End Function

    Private Sub LoadPolicyData(dbPath As String, ByRef policySegment As String, Optional isReload As Boolean = False)

        Dim SQL As String
        Dim SegmentSet As String
        Dim Segment As String
        Dim buf As String
        Dim eValue As String

        If isReload Then policySegment = ""

        If policySegment = "" Then

            SQL = "SELECT * FROM Policy"
            SegmentSet = IO_GetSegmentSet(dbPath, SQL)
            Do Until SegmentSet = ""

                Segment = GetNextSegmentFromSet(SegmentSet)
                buf = Trim(ExtractElementFromSegment("ElementName", Segment))
                eValue = Trim(ExtractElementFromSegment("ElementValue", Segment))
                policySegment = AddElementToSegment(policySegment, buf, eValue)

            Loop

        End If
    End Sub

    Public Function GetPolicyData(dbPath As String, eName As String, Optional defaultValue As String = "") As String

        Dim SQL As String = "SELECT * FROM Setup WHERE ID = 1" ' default
        Dim buf As String = ""

        Select Case dbPath
            Case gShipriteDB
                LoadPolicyData(dbPath, gShipritePolicy) ' load if gShipritePolicy empty

                ' don't automatically add blank element to Policy table if not found in gShipritePolicy variable
                If gShipritePolicy.Length > 0 AndAlso Not IsNothing(defaultValue) AndAlso defaultValue.Length > 0 AndAlso Not IsElementInSegment(eName, gShipritePolicy) Then 'InStr(1, gShipritePolicy, eName, CompareMethod.Text) = 0 Then

                    ' instr(eName) = 0 -> Insert
                    UpdatePolicy(dbPath, eName, defaultValue)

                End If
                buf = ExtractElementFromSegment(eName, gShipritePolicy)

            Case gQBdb
                If gQBSetupPolicy = "" Then
                    gQBSetupPolicy = IO_GetSegmentSet(dbPath, SQL)
                End If
                buf = ExtractElementFromSegment(eName, gQBSetupPolicy)

            Case gReportsDB
                LoadPolicyData(dbPath, gRSetupPolicy) ' load if gRSetupPolicy empty

                ' don't automatically add blank element to Policy table if not found in gRSetupPolicy variable
                If gRSetupPolicy.Length > 0 AndAlso Not IsNothing(defaultValue) AndAlso defaultValue.Length > 0 AndAlso Not IsElementInSegment(eName, gRSetupPolicy) Then

                    ' instr(eName) = 0 -> Insert
                    UpdatePolicy(dbPath, eName, defaultValue)

                End If
                buf = ExtractElementFromSegment(eName, gRSetupPolicy)

            Case gSmartSwiperDB
                If gSwiperSetupPolicy = "" Then
                    gSwiperSetupPolicy = IO_GetSegmentSet(dbPath, SQL)
                End If
                buf = ExtractElementFromSegment(eName, gSwiperSetupPolicy)

        End Select

        ' ol: added default value
        If String.IsNullOrEmpty(buf) Then
            buf = defaultValue
        End If

        Return buf

    End Function

    Function Round(ByVal amt As Double, ByVal pos As Integer, Optional ByVal roundUp As Boolean = False) As Double

        Dim hold As Double
        Dim frac As Double
        Dim adder As Double

        Try

            hold = amt * (10 ^ pos) ' shift decimal place forward
            frac = hold - CLng(hold) ' values after POS
            hold = CLng(hold) ' values before POS
            adder = 0
            If roundUp Then
                If frac > 0 Then
                    adder = 1
                End If
            Else
                If frac >= 0.5 Then
                    adder = 1
                End If
            End If
            hold = hold + adder ' round
            hold = hold / (10 ^ pos) ' shift decimal place back
            Return hold

        Catch ex As Exception

            Return 0

        End Try

    End Function

    Public Function UpdatePolicy(dbPath As String, eName As String, eData As Object) As Integer

        Dim hold As String
        Dim SQL As String
        Dim RowsAffected As Long
        Dim FType As String
        Dim Quotes As Boolean
        Dim iloc As Long
        Dim isDate As Boolean
        Dim buf As String = ""
        Dim ID As Long

        If dbPath = gShipriteDB Then

            LoadPolicyData(dbPath, gShipritePolicy) ' load if gShipritePolicy empty

            If Not IsElementInSegment(eName, gShipritePolicy) Then 'ExtractElementFromSegment(eName, gShipritePolicy) = "" Then 'InStr(1, gShipritePolicy, eName) = 0 Then
                ' Insert

                gShipritePolicy = AddElementToSegment(gShipritePolicy, eName, eData)

                SQL = "SELECT MAX(ID) AS MaxID FROM Policy"
                buf = IO_GetSegmentSet(dbPath, SQL)
                ID = Val(ExtractElementFromSegment("MaxID", buf)) + 1

                SQL = "INSERT INTO Policy (ID, ElementName, ElementValue) VALUES (" & ID & ", '" & eName & "', '" & eData & "')"
                RowsAffected = IO_UpdateSQLProcessor(dbPath, SQL)

            Else
                ' Update

                gShipritePolicy = RemoveElementFromSegment(eName, gShipritePolicy)
                gShipritePolicy = AddElementToSegment(gShipritePolicy, eName, eData)

                SQL = "UPDATE Policy SET ElementValue = '" & eData & "' WHERE ElementName = '" & eName & "'"
                RowsAffected = IO_UpdateSQLProcessor(dbPath, SQL)

            End If

        ElseIf dbPath = gReportsDB Then

            LoadPolicyData(dbPath, gRSetupPolicy) ' load if gRSetupPolicy empty

            If Not IsElementInSegment(eName, gRSetupPolicy) Then
                ' Insert

                gRSetupPolicy = AddElementToSegment(gRSetupPolicy, eName, eData)

                SQL = "SELECT MAX(ID) AS MaxID FROM Policy"
                buf = IO_GetSegmentSet(dbPath, SQL)
                ID = Val(ExtractElementFromSegment("MaxID", buf)) + 1

                SQL = "INSERT INTO Policy (ID, ElementName, ElementValue) VALUES (" & ID & ", '" & eName & "', '" & eData & "')"
                RowsAffected = IO_UpdateSQLProcessor(dbPath, SQL)

            Else
                ' Update

                gRSetupPolicy = RemoveElementFromSegment(eName, gRSetupPolicy)
                gRSetupPolicy = AddElementToSegment(gRSetupPolicy, eName, eData)

                SQL = "UPDATE Policy SET ElementValue = '" & eData & "' WHERE ElementName = '" & eName & "'"
                RowsAffected = IO_UpdateSQLProcessor(dbPath, SQL)

            End If

        Else

            hold = IO_GetFieldsCollection(dbPath, "Setup", eName, True, False, True)
            If hold = "" Then
                _MsgBox.ErrorMessage("Policy field not found!" & Environment.NewLine & Environment.NewLine &
                                     "Database: " & dbPath & Environment.NewLine &
                                     "Table: Setup" & Environment.NewLine &
                                     "Field: " & eName, "Failed to update Policy field...", "Update Policy")
            End If
            hold = ExtractNextElementFromSegment(eName, buf, hold)
            FType = ""
            iloc = InStr(1, eName, ".")
            If iloc > 0 Then
                FType = Strings.Mid(eName, iloc + 1)
                eName = Strings.Mid(eName, 1, iloc - 1)
            End If

            isDate = False
            Select Case FType
                Case 130            ' Text
                    Quotes = True
                Case 0, 2, 4, 5, 6, 3, 11              ' Single, Double, Long, Boolean ''Case "LONG", "INTEGER", "BOOLEAN", "DOUBLE"
                    Quotes = False
                Case 7              ' Date
                    Quotes = False
                    isDate = True
                Case Else
                    Quotes = True
            End Select
            If Quotes = True Then
                If InStr(1, UCase(eName), "DATE") And eData.ToString.Trim = "" Then
                    SQL = "UPDATE [Setup] SET [" & eName & "] = NULL WHERE ID = 1"
                ElseIf isDate Then
                    SQL = "UPDATE [Setup] SET [" & eName & "] = #" & eData & "# WHERE ID = 1"
                Else
                    SQL = "UPDATE [Setup] SET [" & eName & "] = '" & eData & "' WHERE ID = 1"
                End If
            Else
                If eData.ToString.Trim = "" Then
                    SQL = "UPDATE [Setup] SET [" & eName & "] = NULL WHERE ID = 1"
                Else
                    SQL = "UPDATE [Setup] SET [" & eName & "] = " & eData & " WHERE ID = 1"
                End If
            End If

            RowsAffected = IO_UpdateSQLProcessor(dbPath, SQL)


        End If

        '' clear to reload
        Select Case dbPath
            Case gShipriteDB : gSetupPolicy = ""
            Case gReportsDB : gRSetupPolicy = ""
            Case gSmartSwiperDB : gSwiperSetupPolicy = ""
            Case gSecurityDB : gSSetupPolicy = ""
            Case gSalonDB : gSalonPolicy = ""
            Case gQBdb : gQBSetupPolicy = ""
        End Select

        Return RowsAffected

    End Function

    Public Function MergeContact(FromIDs As String, ToIDs As String, TableSchema As String) As String

        Dim SQL As String
        Dim ret As Long
        Dim MessageBuf As String
        Dim SegmentFrom As String
        Dim SegmentTo As String
        Dim buf As String
        Dim FromID As Long
        Dim ToID As Long

        FromID = Val(FromIDs)
        ToIDs = Val(ToIDs)

        MessageBuf = ""

        ' Transactions

        ret = 0
        SQL = "UPDATE Transactions SET SoldTo = " & ToID & " WHERE SoldTo = " & FromID
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
        SQL = "UPDATE Transactions SET ShipTo = " & ToID & " WHERE ShipTo = " & FromID
        ret = ret + IO_UpdateSQLProcessor(gShipriteDB, SQL)
        MessageBuf = MessageBuf & "Transactions......." & ret & vbCrLf

        ' Payments

        ret = 0
        SQL = "UPDATE Payments SET SoldTo = " & ToID & " WHERE SoldTo = " & FromID
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
        SQL = "UPDATE Payments SET ShipTo = " & ToID & " WHERE ShipTo = " & FromID
        ret = ret + IO_UpdateSQLProcessor(gShipriteDB, SQL)
        MessageBuf = MessageBuf & "Payments......." & ret & vbCrLf


        ' POSHold

        ret = 0
        SQL = "UPDATE POSHold SET SoldTo = " & ToID & " WHERE SoldTo = " & FromID
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
        SQL = "UPDATE POSHold SET ShipTo = " & ToID & " WHERE ShipTo = " & FromID
        ret = ret + IO_UpdateSQLProcessor(gShipriteDB, SQL)
        MessageBuf = MessageBuf & "POS Hold......." & ret & vbCrLf

        ' Manifest


        ret = 0
        'SQL = "UPDATE Manifest SET ShpID = '" & ToID & "' WHERE ShpID = '" & FromID & "'"
        'ret = ret + IO_UpdateSQLProcessor(gShipriteDB, SQL)
        'SQL = "UPDATE Manifest SET CgnID = '" & ToID & "' WHERE CgnID = '" & FromID & "'"
        'ret = ret + IO_UpdateSQLProcessor(gShipriteDB, SQL)
        SQL = "UPDATE Manifest SET SID = " & ToID & " WHERE SID = " & FromID
        ret = ret + IO_UpdateSQLProcessor(gShipriteDB, SQL)
        SQL = "UPDATE Manifest SET CID = " & ToID & " WHERE CID = " & FromID
        ret = ret + IO_UpdateSQLProcessor(gShipriteDB, SQL)
        MessageBuf = MessageBuf & "Manifest......." & ret & vbCrLf

        ' Contact History

        ret = 0
        SQL = "UPDATE ContactHistory SET CID = " & ToID & " WHERE CID = " & FromID
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
        MessageBuf = MessageBuf & "Contact History......." & ret & vbCrLf

        ' Contact Notes

        ret = 0
        SQL = "UPDATE ContactNotes SET CID = " & ToID & " WHERE CID = " & FromID
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
        MessageBuf = MessageBuf & "Contact Notes......." & ret & vbCrLf

        ' Mailbox

        ret = 0
        SQL = "UPDATE Mailbox SET CID = " & ToID & " WHERE CID = " & FromID
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
        MessageBuf = MessageBuf & "Mailbox......." & ret & vbCrLf

        ' MBX History

        ret = 0
        SQL = "UPDATE MBXHistory SET CID = " & ToID & " WHERE CID = " & FromID
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
        MessageBuf = MessageBuf & "Mailbox History......." & ret & vbCrLf

        ' MBX Names List

        ret = 0
        SQL = "UPDATE MBXNamesList SET CID = " & ToID & " WHERE CID = " & FromID
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
        MessageBuf = MessageBuf & "Mailbox Names List......." & ret & vbCrLf

        ' Quotes

        ret = 0
        SQL = "UPDATE Quotes SET SoldTo = '" & ToID & "' WHERE SoldTo = '" & FromID & "'"
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
        MessageBuf = MessageBuf & "Quotes......." & ret & vbCrLf

        ' Support

        ret = 0
        SQL = "UPDATE Support SET CID = " & ToID & " WHERE CID = " & FromID
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
        MessageBuf = MessageBuf & "Support......." & ret & vbCrLf

        ' Tickler

        ret = 0
        SQL = "UPDATE Tickler SET CID = " & ToID & " WHERE CID = " & FromID
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
        MessageBuf = MessageBuf & "Support......." & ret & vbCrLf

        ' Merge Records

        SQL = "SELECT * FROM Contacts WHERE ID = " & FromID
        buf = IO_GetSegmentSet(gShipriteDB, SQL)
        SegmentFrom = GetNextSegmentFromSet(buf)

        SQL = "SELECT * FROM Contacts WHERE ID = " & ToID
        buf = IO_GetSegmentSet(gShipriteDB, SQL)
        SegmentTo = GetNextSegmentFromSet(buf)
        SegmentTo = MergeSegment(SegmentFrom, SegmentTo)

        SQL = MakeUpdateSQLFromSchema("Contacts", SegmentTo, gContactsTableSchema)
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)

        ' Contacts

        ret = 0
        SQL = "DELETE * FROM Contacts WHERE ID = " & FromID
        ret = IO_UpdateSQLProcessor(gShipriteDB, SQL)
        MessageBuf = MessageBuf & "Deleted Old Contact [" & FromID & "]......." & ret

        ' Return Message Buffer

        MergeContact = MessageBuf

    End Function

    Public Function GoGirth(L As Integer, W As Integer, H As Integer) As Integer

        Dim Girth As Integer
        Dim Longest, Side1, Side2 As Integer

        Longest = L
        If W > Longest Then

            Side1 = Longest
            Longest = W

        Else

            Side1 = W

        End If
        If H > Longest Then

            Side2 = Longest
            Longest = H

        Else

            Side2 = H

        End If
        Girth = Longest + (2 * Side1) + (2 * Side2)
        Return Girth

    End Function

End Module
