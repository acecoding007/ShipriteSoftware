Imports System.Data
Imports System.Windows.Media

Imports System.Data.SqlClient
Imports System.Data.OleDb
Imports System.IO
Imports System.ComponentModel


Public Class Packmaster
    Private m_DoubleBoxThreshold As Double
    Public Const fldLabor_BuildUp As String = "ABHOMEINVLO"
    Public Const fldLabor_CutDown As String = "ABHOMEINVHI"
    Public Const fldLabor_Telescope As String = "ABTPID7"
    Public Const fldLabor_AddTop As String = "ABHOMEMMINVLO"
    Public global_fragility_index As Single = 1
    Private m_Labor_RegularTab As Single
    Private lwRegDesc_index As Single
    Private m_DefaultLabor As Single
    Private m_DefaultFill As Single
    Private m_Labor_BuildUp As Single
    Private m_Labor_CutDown As Single
    Private m_Labor_AddTop As Single
    Private m_Labor_Telescope As Single
    Private total_price As Double
    Private m_itemVol As Integer ' itemVol = itemL * itemW * itemH




#Region "Form"

    Private isFormLoaded As Boolean
    Private NeedsRecalculation As Boolean = False
    Private CalculationInProgress As Boolean = False

    Private Function CreatePOSSegment(NameOfMaterial As String, i As Integer) As String

        Dim Segment As String = ""
        Dim J As Integer
        Dim Cost As Double = 0
        Dim Price As Double = 0

        J = GetIndexOfMaterials(NameOfMaterial)
        Segment = ""
        If gItemSet(J).SKU.InventorySegment(i) = "" Then
            Return ""
        End If

        Segment = AddElementToSegment(Segment, "SKU", ExtractElementFromSegment("SKU", gItemSet(J).SKU.InventorySegment(i)))

        Segment = AddElementToSegment(Segment, "Desc", ExtractElementFromSegment("Desc", gItemSet(J).SKU.InventorySegment(i)))

        Select Case NameOfMaterial
            Case "Box"
                Segment = AddElementToSegment(Segment, "Qty", Val(txtRegQty1.Text))

            Case "DoubleBox"
                Segment = AddElementToSegment(Segment, "Qty", Val(txtRegQty0.Text))
            Case "Fill"
                Segment = AddElementToSegment(Segment, "Qty", Val(lblQtyFill.Content))
            Case "Wrap"
                Segment = AddElementToSegment(Segment, "Qty", Val(lblQtyWrap.Content))
            Case "Labor"
                Segment = AddElementToSegment(Segment, "Qty", Val(txtRegQty4.Text))
            Case "Other"
                Segment = AddElementToSegment(Segment, "Qty", Val(txtRegQty5.Text))


        End Select
        Cost = Val(ExtractElementFromSegment("Sell", gItemSet(J).SKU.InventorySegment(i)))

        If NameOfMaterial = "Labor" Then
            Price = Cost * (Val(txtRegQty4.Text) / 60)

        Else
            Price = ExtractElementFromSegment("Qty", Segment) * Cost

        End If

        total_price = total_price + Price

        Segment = AddElementToSegment(Segment, "UnitPrice", Cost.ToString)
        Segment = AddElementToSegment(Segment, "ExtPrice", Price.ToString)


        Return Segment

    End Function



    Private Function DisplayInvoiceDetail(PIndex As Integer) As Integer

        Dim SKU As String = ""
        Dim Description As String = ""
        Dim amt As Double = 0
        Dim RowCT As Integer = 0
        Dim i As Integer = 0
        Dim j As Integer = 0
        Dim Charge As Double = 0
        Dim Payment As Double = 0
        Dim Segment As String = ""
        Dim SegmentSet As String = ""
        Dim ExtPrice As Double = 0

        total_price = 0

        BindingOperations.ClearAllBindings(Reviewer_ListView) ' clear binding on ListView
        Reviewer_ListView.DataContext = Nothing ' remove any rows already in ListView

        Dim DT As New System.Data.DataTable ' datatable to use to populate ListView

        Dim currentGridView As GridView = Reviewer_ListView.View ' variable to reference current GridView in Users_ListView to set up columns.

        'add same column names to datatable columns
        DT.Columns.Add("SKU")
        DT.Columns.Add("Desc")
        DT.Columns.Add("Qty", GetType(Double))
        DT.Columns.Add("UnitPrice", GetType(Double))
        DT.Columns.Add("ExtPrice", GetType(Double))

        i = PIndex


        Segment = CreatePOSSegment("Box", i)
        If Not Segment = "" Then

            SegmentSet = SegmentSet & "<SET>" & Segment & "</SET>" & vbCrLf

        End If
        Segment = CreatePOSSegment("DoubleBox", i)
        If Not Segment = "" Then

            SegmentSet = SegmentSet & "<SET>" & Segment & "</SET>" & vbCrLf

        End If
        Segment = CreatePOSSegment("Fill", i)
        If Not Segment = "" Then

            SegmentSet = SegmentSet & "<SET>" & Segment & "</SET>" & vbCrLf

        End If
        Segment = CreatePOSSegment("Wrap", i)
        If Not Segment = "" Then

            SegmentSet = SegmentSet & "<SET>" & Segment & "</SET>" & vbCrLf

        End If
        Segment = CreatePOSSegment("Labor", i)
        If Not Segment = "" Then

            SegmentSet = SegmentSet & "<SET>" & Segment & "</SET>" & vbCrLf

        End If

        Segment = CreatePOSSegment("Other", i)
        If Not Segment = "" Then

            SegmentSet = SegmentSet & "<SET>" & Segment & "</SET>" & vbCrLf

        End If

        txtPackPrice.Text = Format(total_price, "$ 0.00")

        RowCT = LoadSegmentInToListView(Reviewer_ListView, DT, SegmentSet, currentGridView.Columns.Count)

        Return RowCT

    End Function

    Private Function sum_Regular_Price_n_Weight(ByRef retCost As Double, ByRef retSell As Double, ByRef retWeight As Double) As Boolean
        Dim i As Integer
        Dim taxblSell As Double
        Dim taxedSell As Double
        Dim tottaxSell As Double
        'Dim tnode As TreeNode = Nothing
        ''
        retCost = 0 '' assume.
        retSell = 0 '' assume.
        retWeight = 0 '' assume.
        For i = 0 To 5
            _Debug.Print_(m_RegPackItems(i).SKU, m_RegPackItems(i).ExtPrice)
            retCost = retCost + _Convert.Round_Double2Decimals(m_RegPackItems(i).ExtCost, 2)
            retSell = retSell + _Convert.Round_Double2Decimals(m_RegPackItems(i).ExtPrice, 2)
            retWeight = retWeight + _Convert.Round_Double2Decimals(m_RegPackItems(i).Weight, 1)

            ''ol#9.57(10/6)... Department of a packing item should be checked against tax requirements.
            If PackMasterII.IsTAXable(m_RegPackItems(i).Dept) Then
                taxblSell = taxblSell + m_RegPackItems(i).ExtPrice

            End If ''ol#9.57(10/6).
        Next i



        ''ol#9.57(10/6)... Department of a packing item should be checked against tax requirements.
        If taxblSell > 0 Then
            If PackMasterII.Calc_TAX(taxblSell, taxedSell) Then
                tottaxSell = retSell - taxblSell ' we need leave only non-taxable charge.
                tottaxSell = tottaxSell + taxedSell ' add subtracted taxble charge but with tax this time
            End If
        End If ''ol#9.57(10/6).
        ''ol#9.57(10/6)... Department of a packing item should be checked against tax requirements.
        If tottaxSell > 0 Then
            tottaxSell = tottaxSell + (Val(Me.txtRegPiecesNo.Text) * m_DefaultPieceCharge)
            'Call tvNode_AddTotal_Tax(UBound(m_RegPackItems) + 1, retCost, tottaxSell, retWeight, tnode)
            'Me.txtPackPrice.Text = Format(tottaxSell, "0.00")
        Else
            retSell = retSell + (Val(Me.txtRegPiecesNo.Text) * m_DefaultPieceCharge)
            'Call tvNode_AddTotal(UBound(m_RegPackItems) + 1, retCost, retSell, retWeight, tnode)
            ' Me.txtPackPrice.Text = Format(retSell, "0.00")
        End If ''ol#9.57(10/6).
        Me.txtPackPrice.Tag = Format(retCost, "0.00") ''ol#9.55(9/22)... "Apply" button will transfer currently displayed pack-job to POS/ShipMaster without saving it.
        'Me.txtPackWeight.Text = Format(retWeight + Val(Me.txtObjWeight.Text), "0.0")
        sum_Regular_Price_n_Weight = (retCost > 0 Or retSell > 0 Or retWeight > 0)
        ''
    End Function

    Public Sub cmdClearAll_click(sender As Object, e As RoutedEventArgs) Handles cmdClearAll.Click
        Call load_PackMaster()

        Call SetWarningColor()
    End Sub

    Private Function DisplaySelectedPackage() As Integer

        Dim i As Integer
        Dim j As Integer
        Dim Weight As Double = 0
        Dim ret As Integer

        For i = 1 To 5
            Dim Selection As Object = Me.FindName("optFragile" & i.ToString)
            If Selection.ischecked = True Then
                Exit For

            End If

        Next

        global_fragility_index = i
        If chkPackDoubleBox.IsChecked Then
            j = GetIndexOfMaterials("Box")
            Dim BoxSKU As Object = Me.FindName("cmbRegDesc" & "1")

            BoxSKU.text = gItemSet(j).SKU.L(i)

            Dim BoxQty As Object = Me.FindName("txtRegQty" & "1")
            BoxQty.text = "1"
            txtPackInnerL.Text = Val(ExtractElementFromSegment("L", gItemSet(j).SKU.InventorySegment(i)))
            txtPackInnerW.Text = Val(ExtractElementFromSegment("W", gItemSet(j).SKU.InventorySegment(i)))
            txtPackInnerH.Text = Val(ExtractElementFromSegment("H", gItemSet(j).SKU.InventorySegment(i)))

            j = GetIndexOfMaterials("DoubleBox")
            Dim DBoxSKU As Object = Me.FindName("cmbRegDesc" & "0")
            DBoxSKU.text = gItemSet(j).SKU.L(i)
            Dim DBoxQty As Object = Me.FindName("txtRegQty" & "0")
            DBoxQty.text = "1"
            txtPackOuterL.Text = Val(ExtractElementFromSegment("L", gItemSet(j).SKU.InventorySegment(i)))
            txtPackOuterW.Text = Val(ExtractElementFromSegment("W", gItemSet(j).SKU.InventorySegment(i)))
            txtPackOuterH.Text = Val(ExtractElementFromSegment("H", gItemSet(j).SKU.InventorySegment(i)))

        Else
            j = GetIndexOfMaterials("Box")
            Dim BoxSKU As Object = Me.FindName("cmbRegDesc" & "1")

            BoxSKU.text = gItemSet(j).SKU.L(i)

            Dim BoxQty As Object = Me.FindName("txtRegQty" & "1")
            BoxQty.text = "1"
            txtPackOuterL.Text = Val(ExtractElementFromSegment("L", gItemSet(j).SKU.InventorySegment(i)))
            txtPackOuterW.Text = Val(ExtractElementFromSegment("W", gItemSet(j).SKU.InventorySegment(i)))
            txtPackOuterH.Text = Val(ExtractElementFromSegment("H", gItemSet(j).SKU.InventorySegment(i)))


            txtPackInnerL.Text = "0"
            txtPackInnerW.Text = "0"
            txtPackInnerH.Text = "0"

        End If


        j = GetIndexOfMaterials("PackagingWeight")
        Weight = Val(gItemSet(j).PackagingWeight(i))


        j = GetIndexOfMaterials("Wrap")
        Dim WrapSKU As Object = Me.FindName("cmbRegDesc" & "2")
        WrapSKU.text = gItemSet(j).SKU.L(i)
        Dim WrapQty As Object = Me.FindName("txtRegQty" & "2")
        WrapQty.text = gItemSet(j).Units.L(i)

        Weight += gItemSet(j).BasePackagingWeight(i)

        j = GetIndexOfMaterials("Fill")
        Dim FillSKU As Object = Me.FindName("cmbRegDesc" & "3")
        FillSKU.text = gItemSet(j).SKU.L(i)
        Dim FillQty As Object = Me.FindName("txtRegQty" & "3")
        FillQty.text = gItemSet(j).Units.L(i)

        Weight += gItemSet(j).BasePackagingWeight(i)

        j = GetIndexOfMaterials("Labor")
        Dim LaborSKU As Object = Me.FindName("cmbRegDesc" & "4")
        LaborSKU.text = gItemSet(j).SKU.L(i)
        Dim LaborQty As Object = Me.FindName("txtRegQty" & "4")
        LaborQty.text = gItemSet(j).Units.L(i)

        Weight += Val(txtObjWeight.Text)

        txtPackWeight.Text = Format(Weight, "0.00")
        Dim PriceLabel As Object = Me.FindName("lblPrice" & i.ToString)
        txtPackPrice.Text = PriceLabel.content


        txtPackL_G.Text = GoGirth(Val(txtPackOuterL.Text), Val(txtPackOuterW.Text), Val(txtPackOuterH.Text)).ToString

        'ret = DisplayInvoiceDetail(i)

        Return 0

    End Function

    Private Sub cmbRegDesc_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles cmbRegDesc0.SelectionChanged, cmbRegDesc1.SelectionChanged, cmbRegDesc2.SelectionChanged, cmbRegDesc3.SelectionChanged, cmbRegDesc4.SelectionChanged, cmbRegDesc5.SelectionChanged
        Try
            Dim cmbRegDesc As ComboBox = CType(sender, ComboBox)
            Dim Index As Integer = Val(_Controls.Right(cmbRegDesc.Name, 1))
            cmbRegDesc_Click(Index)
            SetWarningColor()
        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to select SKU...")
        End Try
    End Sub

    Private Sub cmbRegDesc_Click(ByVal Index As Integer)

        Dim retLL As Integer
        Dim retWW As Integer
        Dim retHH As Integer
        Dim drows() As DataRow = Nothing
        Dim J As Integer
        Dim SQL As String = ""
        Dim Segment As String = ""
        Dim SegmentSet As String = ""

        Dim cmbRegDesc As ComboBox = Nothing : find_cmbRegDesc_byINDEX(Index, cmbRegDesc)
        Dim txtRegQty As TextBox = Nothing : find_txtRegQty_byINDEX(Index, txtRegQty)
        Dim cmdRegQtyPlus As Button = Nothing : find_cmdRegQtyPlus_byINDEX(Index, cmdRegQtyPlus)
        Dim cmdRegQtyMinus As Button = Nothing : find_cmdRegQtyMinus_byINDEX(Index, cmdRegQtyMinus)
        ''
        ''ol#9.55(9/21)... To avoid infinite clicks in combo selects we need to know if the combo was already clicked.
        If Not 0 = Len(cmbRegDesc.Tag) Then
            Exit Sub
        Else
            cmbRegDesc.Tag = "Already Clicked"
        End If ''ol#9.55(9/21).
        ''
        If -1 = cmbRegDesc.SelectedIndex Then
            txtRegQty.Text = "0"
            cmdRegQtyPlus.IsEnabled = False
            cmdRegQtyMinus.IsEnabled = False
            txtRegQty.IsEnabled = False
        ElseIf -1 < cmbRegDesc.SelectedIndex And 0 = Val(txtRegQty.Text) Then
            txtRegQty.Text = "1"
            cmdRegQtyPlus.IsEnabled = True
            cmdRegQtyMinus.IsEnabled = True
            txtRegQty.IsEnabled = True
        ElseIf 0 < Val(txtRegQty.Text) Then
            cmdRegQtyPlus.IsEnabled = True
            cmdRegQtyMinus.IsEnabled = True
            txtRegQty.IsEnabled = True
        End If


        ''
        If -1 < cmbRegDesc.SelectedIndex Then
            Select Case Index

                Case iOUTER ' outer box
                    Call find_LWH_bySKU(iOUTER, cmbRegDesc.Text, retLL, retWW, retHH)
                    Me.txtPackOuterL.Text = CStr(retLL)
                    Me.txtPackOuterW.Text = CStr(retWW)
                    Me.txtPackOuterH.Text = CStr(retHH)
                    'Me.txtPackDimW.Text = CStr(Shipping.Calc_DimWeight(retLL, retWW, retHH, m_IsInternational))
                    'Me.txtPackL_G.Text = CStr(calc_LengthGirth(retLL, retWW, retHH))
                    Call change_Price_n_Weight(i_FILL) ' filler uses Outer Volume
                    SQL = "SELECT SKU, L, W, H, Sell, Desc, Weight FROM Inventory WHERE SKU='" & cmbRegDesc.Text & "'"
                    SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
                    J = GetIndexOfMaterials("DoubleBox")
                    gItemSet(J).SKU.InventorySegment(global_fragility_index) = SegmentSet

                    DisplayInvoiceDetail(global_fragility_index)



                Case iINNER ' inner box
                    Call find_LWH_bySKU(iINNER, cmbRegDesc.Text, retLL, retWW, retHH)
                    If chkPackDoubleBox.IsChecked Then
                        Me.txtPackInnerL.Text = CStr(retLL)
                        Me.txtPackInnerW.Text = CStr(retWW)
                        Me.txtPackInnerH.Text = CStr(retHH)
                    Else

                        Me.txtPackOuterL.Text = CStr(retLL)
                        Me.txtPackOuterW.Text = CStr(retWW)
                        Me.txtPackOuterH.Text = CStr(retHH)

                    End If



                    SQL = "SELECT SKU, L, W, H, Sell, Desc, Weight FROM Inventory WHERE SKU='" & cmbRegDesc.Text & "'"
                    SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
                    J = GetIndexOfMaterials("Box")
                    gItemSet(J).SKU.InventorySegment(global_fragility_index) = SegmentSet


                    DisplayInvoiceDetail(global_fragility_index)

                    Call change_Price_n_Weight(i_FILL) ' filler uses Outer Volume
                Case iOTHER
                    SQL = "SELECT SKU, L, W, H, Sell, Desc, Weight FROM Inventory WHERE SKU='" & cmbRegDesc.Text & "'"
                    SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
                    J = GetIndexOfMaterials("Other")
                    gItemSet(J).SKU.InventorySegment(global_fragility_index) = SegmentSet


                    DisplayInvoiceDetail(global_fragility_index)

                Case iWRAP
                    SQL = "SELECT SKU, L, W, H, Sell, Desc, Weight FROM Inventory WHERE SKU='" & cmbRegDesc.Text & "'"
                    SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
                    J = GetIndexOfMaterials("Wrap")
                    gItemSet(J).SKU.InventorySegment(global_fragility_index) = SegmentSet


                    DisplayInvoiceDetail(global_fragility_index)
                Case i_FILL
                    SQL = "SELECT SKU, L, W, H, Sell, Desc, Weight FROM Inventory WHERE SKU='" & cmbRegDesc.Text & "'"
                    SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
                    J = GetIndexOfMaterials("Fill")
                    gItemSet(J).SKU.InventorySegment(global_fragility_index) = SegmentSet


                    DisplayInvoiceDetail(global_fragility_index)
                Case iLABOR
                    SQL = "SELECT SKU, L, W, H, Sell, Desc, Weight FROM Inventory WHERE SKU='" & cmbRegDesc.Text & "'"
                    SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
                    J = GetIndexOfMaterials("Labor")
                    gItemSet(J).SKU.InventorySegment(global_fragility_index) = SegmentSet


                    DisplayInvoiceDetail(global_fragility_index)

            End Select
        End If
        ''
        'change_Price_n_Weight_For2345()
        'Call change_Price_n_Weight(Index)

        ''
        cmbRegDesc.Tag = String.Empty ''ol#9.55(9/21)... To avoid infinite clicks in combo selects we need to know if the combo was already clicked.
    End Sub
    Private Function CalculateCharges() As Integer


        Dim Segment As String = ""
        Dim ReturnSegment As String = ""
        Dim DoubleBox As String = ""
        Dim L As Integer = 0
        Dim W As Integer = 0
        Dim H As Integer = 0
        Dim ErrorMessage As String = ""
        Dim J As Integer
        Dim buf As String = ""
        Dim Fill As Double = 0
        Dim Wrap As Double = 0
        Dim DBoxPrice As Double = 0
        Dim PackagingWeight As Double = 0

        Dim FillQty As Double
        Dim WrapQty As Double
        Dim iDesc As String = String.Empty
        Dim iMatClass As String = String.Empty
        Dim iDept As String = String.Empty
        Dim itemL As Single : itemL = Val(Me.txtObjL.Text)
        Dim itemW As Single : itemW = Val(Me.txtObjW.Text)
        Dim itemH As Single : itemH = Val(Me.txtObjH.Text)

        Dim itemVol As Single : itemVol = itemL * itemW * itemH
        Dim innerVol As Integer : innerVol = Val(Me.txtPackInnerL.Text) * Val(Me.txtPackInnerW.Text) * Val(Me.txtPackInnerH.Text)
        ''ol#9.51(8/27)... Calculate Wrap volume (QTY=1 is 0.5'') to include in Filler calculation.
        Dim wrapHight As Single = 0.5
        Dim wrapLWH As Single
        Dim wrapL As Single
        Dim wrapW As Single
        Dim wrapH As Single
        Dim wrapVol As Single
        ''ol#1.2.27(12/10)... Fill should be calculated based on the object dimensions if user clicks +/- buttons
        ''ol#1.2.26(12/1).


        CalculationInProgress = True

        Segment = AddElementToSegment(Segment, "Weight", txtObjWeight.Text)
        Segment = AddElementToSegment(Segment, "L", txtObjL.Text)
        Segment = AddElementToSegment(Segment, "W", txtObjW.Text)
        Segment = AddElementToSegment(Segment, "H", txtObjH.Text)
        Segment = AddElementToSegment(Segment, "DecVal", txtObjValue.Text)
        Segment = AddElementToSegment(Segment, "NoOfPcs", txtRegPiecesNo.Text)
        If chkPackDoubleBox.IsChecked Then
            Segment = AddElementToSegment(Segment, "DoubleBox", True)

        End If

        For i = 1 To 5
            Dim D As Object = Me.FindName("PackLevel" & i.ToString)
            ReturnSegment = FragilityCalculator(D.content, Segment)
            ErrorMessage = ExtractElementFromSegment("ERRORMSG", ReturnSegment)

            DoubleBox = ExtractElementFromSegment("DoubleBox", ReturnSegment)

            buf = ExtractElementFromSegment("DoubleBox", ReturnSegment)
            Dim DBox As Object = Me.FindName("chkFragile" & i.ToString)
            If buf = "True" Then
                J = GetIndexOfMaterials("DoubleBox")
                DBox.ischecked = True

            Else
                J = GetIndexOfMaterials("Box")
                DBox.ischecked = False

            End If
            L = Val(ExtractElementFromSegment("L", gItemSet(J).SKU.InventorySegment(i)))
            W = Val(ExtractElementFromSegment("W", gItemSet(J).SKU.InventorySegment(i)))
            H = Val(ExtractElementFromSegment("H", gItemSet(J).SKU.InventorySegment(i)))
            Dim Box As Object = Me.FindName("lblFragileBox" & i.ToString)
            Box.content = L.ToString & " x " & W.ToString & " x " & H.ToString

            Dim BoxPrice As Object = Me.FindName("lblBoxPrice" & i.ToString)
            BoxPrice.content = Format(Val(ExtractElementFromSegment("Sell", gItemSet(J).SKU.InventorySegment(i))), "$ 0.00")


            J = GetIndexOfMaterials("DoubleBox")
            DBoxPrice = Val(ExtractElementFromSegment("Sell", gItemSet(J).SKU.InventorySegment(i))) * gItemSet(J).Units.L(i)
            BoxPrice.content = Format(ValFix(BoxPrice.content) + DBoxPrice, "$ 0.00")
            Dim Materials As Object = Me.FindName("lblMaterialsPrice" & i.ToString)

            J = GetIndexOfMaterials("PackagingWeight")
            PackagingWeight = Val(gItemSet(J).PackagingWeight(i))

            J = GetIndexOfMaterials("Wrap")
            wrapLWH = Val(gItemSet(J).Units.L(i)) * wrapHight
            wrapL = itemL + wrapLWH
            wrapW = itemW + wrapLWH
            wrapH = itemH + wrapLWH
            wrapVol = wrapL * wrapW * wrapH

            WrapQty = _Convert.Round_Double2Decimals((((2 * (itemL * itemW)) + (2 * (itemW * itemH)) + (2 * (itemL * itemH))) / 144) * gItemSet(J).Units.L(i), 1) ''mm#9.83(3/15).
            Wrap = Val(ExtractElementFromSegment("Sell", gItemSet(J).SKU.InventorySegment(i))) * WrapQty




            J = GetIndexOfMaterials("Fill")
            Dim fillHight As Single = Val(gItemSet(J).Units.L(i))
            Dim fillVol As Single = (wrapL + fillHight) * (wrapW + fillHight) * (wrapH + fillHight)

            Dim outerVol As Integer = L * W * H
            If 1 = gItemSet(J).Units.L(i) Then

                If 0 < wrapLWH Then
                    FillQty = _Convert.Round_Double2Decimals((outerVol - wrapVol) / 1728, 1)
                Else
                    FillQty = _Convert.Round_Double2Decimals((outerVol - itemVol) / 1728, 1)

                End If
            Else
                If 0 < wrapLWH Then
                    FillQty = _Convert.Round_Double2Decimals((outerVol - wrapVol + fillVol) / 1728, 1)
                Else
                    FillQty = _Convert.Round_Double2Decimals((outerVol - wrapVol + fillVol) / 1728, 1)

                End If
            End If
            Fill = Val(ExtractElementFromSegment("Sell", gItemSet(J).SKU.InventorySegment(i))) * FillQty
            PackagingWeight = PackagingWeight + gItemSet(J).BasePackagingWeight(i)
            Materials.content = Format$(Wrap + Fill, "$ 0.00")
            PackagingWeight = PackagingWeight + gItemSet(J).BasePackagingWeight(i)

            J = GetIndexOfMaterials("Labor")

            Dim Labor As Object = Me.FindName("lblLaborPrice" & i.ToString)
            Labor.content = Format(Val(ExtractElementFromSegment("Sell", gItemSet(J).SKU.InventorySegment(i))) * (gItemSet(J).Units.L(i) / 60), "$ 0.00")


            Dim TPrice As Object = Me.FindName("lblPrice" & i.ToString)
            TPrice.content = Format(ValFix(Materials.content) + ValFix(BoxPrice.content) + ValFix(Labor.content), "$ 0.00")

            Dim lblWeightAct As Object = Me.FindName("lblWeightAct" & i.ToString)
            lblWeightAct.content = Format(Val(PackagingWeight), "0.00 lbs")
        Next
        If txtPackOuterL.Text <> "0" And txtPackOuterW.Text <> "0" And txtPackOuterH.Text <> "0" Then
            Dim ret As Integer = 0
            ret = DisplaySelectedPackage()
            Call SetWarningColor()
        End If



        CalculationInProgress = False
        Return 0



    End Function

    Private Function SetWarningColor() As Integer

        If 0 < Val(Me.txtPackOuterL.Text) * Val(Me.txtPackOuterW.Text) * Val(Me.txtPackOuterH.Text) Then
            If Val(Me.txtPackOuterL.Text) < Val(Me.txtObjL.Text) + 2 Then
                Me.txtPackOuterL.Foreground = New SolidColorBrush(Colors.Red)
            ElseIf Val(Me.txtPackOuterW.Text) < Val(Me.txtObjW.Text) + 2 Then
                Me.txtPackOuterW.Foreground = New SolidColorBrush(Colors.Red)
            ElseIf Val(Me.txtPackOuterH.Text) < Val(Me.txtObjH.Text) + 2 Then
                Me.txtPackOuterH.Foreground = New SolidColorBrush(Colors.Red)
            Else
                Me.txtPackOuterL.Foreground = New SolidColorBrush(Colors.Black)
                Me.txtPackOuterW.Foreground = New SolidColorBrush(Colors.Black)
                Me.txtPackOuterH.Foreground = New SolidColorBrush(Colors.Black)

            End If
        Else
            Me.txtPackOuterL.Foreground = New SolidColorBrush(Colors.Black)
            Me.txtPackOuterW.Foreground = New SolidColorBrush(Colors.Black)
            Me.txtPackOuterH.Foreground = New SolidColorBrush(Colors.Black)

        End If
    End Function
    Private Sub close_dtable(ByRef dtable As DataTable)
        Try
            If dtable IsNot Nothing Then
                dtable.Clear()
                dtable = Nothing
            End If
        Catch ex As Exception
        End Try
    End Sub

    Public Sub New()

        MyBase.New()
        isFormLoaded = False

        ' This call is required by the designer.
        InitializeComponent()

        isFormLoaded = True
    End Sub
    Public Sub New(ByVal callingWindow As Window)

        MyBase.New(callingWindow)
        isFormLoaded = False

        ' This call is required by the designer.
        InitializeComponent()
        load_Values()
        Call load_PackMaster()
        Me.txtObjWeight.Focus()
        isFormLoaded = True
        load_Setup1()


    End Sub
    Protected Overrides Sub Finalize()
        close_dtable(dtlPackContents)
        close_dtable(dtlPackMaterials)
        close_dtable(dtlPackMaterials_Filter)
        MyBase.Finalize()
    End Sub








    Private Function filter_FragileChoices(ByVal level As String, ByVal itemname As String, ByRef retval As String) As Boolean
        filter_FragileChoices = False
        Dim drows() As DataRow = Nothing
        If _DataSet.Filter_DataTable(dtlPackFragile, String.Format("ItemName='{0}'", itemname), drows) Then
            retval = _Convert.Null2DefaultValue(drows(0)("Fragile_L" & level))
            Return True
        End If
    End Function
    Private Function calc_FragileLevel(ByVal level As Integer) As FragilityObject
        calc_FragileLevel = Nothing
        If level >= 1 And level <= 5 Then
            For Index As Integer = 0 To 5
                Call clear_m_RegPackItem(Index, False)
            Next Index
            Dim obj As New FragilityObject
            With obj
                .Level = level
                Dim fill As String = String.Empty
                Dim wrap As String = String.Empty
                Dim labor As String = String.Empty
                If filter_FragileChoices(.Level.ToString, "FillUnit", fill) Then
                    .FillUnit = Val(fill)
                    If filter_FragileChoices(.Level.ToString, "WrapUnit", wrap) Then
                        .WrapUnit = Val(wrap)
                        Call filter_FragileChoices(.Level.ToString, "LaborUnit", labor)
                        .LaborUnit = Val(labor)
                        Call filter_FragileChoices(.Level.ToString, "FillSKU", .FillSKU)
                        Call filter_FragileChoices(.Level.ToString, "WrapSKU", .WrapSKU)
                        Call filter_FragileChoices(.Level.ToString, "LaborSKU", .LaborSKU)
                        '
                        .ObjectL = Val(Me.txtObjL.Text) + ((.WrapUnit * 0.5) + .FillUnit) ' 0.5 inches is the height of Wrap
                        .ObjectW = Val(Me.txtObjW.Text) + ((.WrapUnit * 0.5) + .FillUnit)
                        .ObjectH = Val(Me.txtObjH.Text) + ((.WrapUnit * 0.5) + .FillUnit)
                    End If
                End If
            End With
            calc_FragileLevel = obj
        End If
    End Function

#End Region

#Region "Load"

    Private Function load_Values() As Boolean
        If load_Combos(False) Then
            If load_SetupData() Then
                If load_Contents() Then
                    ''ol#1.2.27(12/10)... Don't require Content list, it could be empty.
                    ''  load_Values = True
                End If
                Return True
            End If
        End If
    End Function
    Private Function load_ComboDropDown(ByVal Index As Integer) As Boolean
        lwRegDesc.ItemsSource = Nothing
        lwRegDesc_index = Index
        Dim Cmblist = New List(Of InventoryItem)
        Cmblist.Clear()
        Dim MaterialsClass As String = String.Empty

        ' Set the MaterialsClass based on Index
        Select Case Index
            Case 0, 1, 5
                MaterialsClass = "Boxes"
            Case 2
                MaterialsClass = "Wrap"
            Case 3
                MaterialsClass = "Filler"
            Case 4
                MaterialsClass = "Labor"
            Case Else
                Return False ' Return false if index is out of range
        End Select



        Dim SQL As String = "SELECT SKU, Desc, Weight, Sell, Quantity FROM Inventory WHERE PackagingMaterials = True AND ([Zero] = False OR ([Zero] = True AND [Quantity] > 0)) AND MaterialsClass = """ & MaterialsClass & """"
        Dim SegmentSet As String = IO_GetSegmentSet(gShipriteDB, SQL)

        Do Until SegmentSet = ""
            Dim listCmb As New InventoryItem
            Dim Segment As String = GetNextSegmentFromSet(SegmentSet)

            listCmb.SKU = ExtractElementFromSegment("SKU", Segment)

            listCmb.Desc = ExtractElementFromSegment("Desc", Segment)
            listCmb.Weight = ExtractElementFromSegment("Weight", Segment)
            listCmb.Sell = ExtractElementFromSegment("Sell", Segment)
            listCmb.Quantity = ExtractElementFromSegment("Quantity", Segment)

            Cmblist.Add(listCmb)
        Loop


        lwRegDesc.ItemsSource = Cmblist

        If lwRegDesc.Items.Count > 0 Then
            lwRegDesc.Visibility = Visibility.Visible
        Else
            lwRegDesc.Visibility = Visibility.Collapsed

        End If

        If Me.lwRegDesc.IsVisible Then

            load_ComboDropDown = True
        End If
    End Function
    Private Function load_Contents() As Boolean
        If ShipRiteDb.Load_DataSet_Contents(ShipRiteDb.tblPackContents) Then
            If ShipRiteDb.Get_DataTable(ShipRiteDb.tblPackContents, dtlPackContents) Then
                load_Contents = load_ComboContents(dtlPackContents.Rows)
            End If
        End If
    End Function

    Private Function load_SetupData() As Boolean
        Dim procName As String : procName = "Private Function load_SetupData() As Boolean"
        Dim decValue As Integer : decValue = Val(Me.txtObjValue.Text)
        ''
        'Me.chkRevReceiptView.CheckState = My.Settings.Enable_ReceiptView
        'Me.chkModSuggestBox.CheckState = My.Settings.Suggest_BoxSelections
        If load_Setup1() Then
            chkPackDoubleBox.ToolTip = "Auto-Triggers at $" & m_DoubleBoxThreshold.ToString() & " of Declared Value"
        End If
        If load_Setup2() Then
            Me.txtRegMinFill.Text = m_DefaultFill.ToString()
            txtRegMinFill.ToolTip = "Amount of loose fill in inches, between the object and the side of the box"

            txtRegQty4.ToolTip = "Unit of labor billed per package, by " & m_DefaultLabor.ToString() & " times the default charge per item"

            If 0 < m_DefaultLabor And 0 < Me.cmbRegDesc4.Items.Count And Not 0 = Len(Me.cmbRegDesc1.Text) Then
                Me.cmbRegDesc4.SelectedIndex = 0
            End If
            If 0 < Val(Me.txtRegMinFill.Text) And 0 < Me.cmbRegDesc3.Items.Count And Not 0 = Len(Me.cmbRegDesc1.Text) Then
                Me.cmbRegDesc3.SelectedIndex = 0
            End If
        End If

        ''
        ''ol#1.2.38(5/19)... "New/Add/Edit/Delete" these are not intuitive labels so balloon pop-up descriptions was added when mouse hovers the buttons.
        cmdSaveAsNew.ToolTip = "Creates New 'Contents' pack job with values as they displayed on this screen and adds it to the list of other jobs."
        cmdSave.ToolTip = "Saves changes for already selected 'Contents' pack job with values as they displayed on this screen."
        cmdDelete.ToolTip = "Deletes already selected 'Contents' pack job from the list."

        ''
        If load_SetupFragile() Then
            If 0 < dtlPackFragile.Rows.Count Then
                ' Show Fragile Panel
            End If
        End If
        ''
        ''ol#1.2.26(12/3)... Setup data loads twice on form load.
        ''  load_SetupData = (load_Setup1() And load_Setup2())
        Return True
    End Function

    Private Function load_Setup1() As Boolean


        m_DoubleBoxThreshold = Val(GetPolicyData(gShipriteDB, "DoubleBoxThreshold", "0"))
        If m_DoubleBoxThreshold = "0" Then
            load_Setup1 = False
        Else
            load_Setup1 = True
        End If
    End Function
    Private Function load_Setup2() As Boolean

        ' Regular tab:

        m_DefaultPieceCharge = _Convert.Null2DefaultValue(GetPolicyData(gShipriteDB, "defaultPieceCharge"), 0)

            m_DefaultLabor = _Convert.Null2DefaultValue(GetPolicyData(gShipriteDB, "defaultLabor"), 0)

            m_DefaultFill = _Convert.Null2DefaultValue(GetPolicyData(gShipriteDB, "defaultFill"), 0)

            ''m_DefaultLabor = _Convert.Null2DefaultValue(dreader("defaultLabor"), 0)
            ''m_DefaultFill = _Convert.Null2DefaultValue(dreader("defaultFill"), 0)
            ''m_FragEasy = _Convert.Null2DefaultValue(dreader("fragEasy"), 0)
            ''m_FragMedium = _Convert.Null2DefaultValue(dreader("fragMedium"), 0)
            ''m_FragHard = _Convert.Null2DefaultValue(dreader("fragHard"), 0)
            ''ol#9.124(2/21)... 'Fragile' radio-buttons labor-unit-values should be round to the whole number without decimals.
            ''m_FragEasy2 = System.Math.Round(m_FragEasy + (m_FragMedium - m_FragEasy) / 2, 0)
            ''m_FragMedium2 = System.Math.Round(m_FragMedium + (m_FragHard - m_FragMedium) / 2, 0)
            ''m_ThresEasy = _Convert.Null2DefaultValue(dreader("threseasy"), 0)
            ''m_ThresMedium = _Convert.Null2DefaultValue(dreader("thresmedium"), 0)
            ''m_ThresHard = _Convert.Null2DefaultValue(dreader("thresHard"), 0)
            ''m_ThresEasy2 = System.Math.Round(m_ThresEasy + (m_ThresMedium - m_ThresEasy) / 2, 0)
            ''m_ThresMedium2 = System.Math.Round(m_ThresMedium + (m_ThresHard - m_ThresMedium) / 2, 0)
            ' Modified tab:
            m_Labor_AddTop = _Convert.Null2DefaultValue(GetPolicyData(gShipriteDB, fldLabor_AddTop), 0)
            m_Labor_BuildUp = _Convert.Null2DefaultValue(GetPolicyData(gShipriteDB, fldLabor_BuildUp), 0)
            m_Labor_CutDown = _Convert.Null2DefaultValue(GetPolicyData(gShipriteDB, fldLabor_CutDown), 0)
            m_Labor_Telescope = _Convert.Null2DefaultValue(GetPolicyData(gShipriteDB, fldLabor_Telescope), 0)


    End Function



    Private Function load_PackMaster() As Boolean

        Dim i As Integer
        Dim ret As Integer

        load_PackMaster = False
        '
        'isOpen_ShipNew = False ' opened from ShipMaster or POS 
        Dim gShip As New gShip_Class : gShip.ContentsID = 0
        '
        '
        'm_IsInternational = False
        m_Labor_RegularTab = m_DefaultLabor ' assume.

        If isOpen_ShipNew = True Then

            Me.txtObjL.Text = ExtractElementFromSegment("Length", gShipmentParameters)
            Me.txtObjW.Text = ExtractElementFromSegment("Width", gShipmentParameters)
            Me.txtObjH.Text = ExtractElementFromSegment("Height", gShipmentParameters)
            Me.txtObjWeight.Text = ExtractElementFromSegment("Weight", gShipmentParameters)
            Me.txtObjValue.Text = ExtractElementFromSegment("DeclaredValue", gShipmentParameters)
            Me.txtObjDesc.Text = ExtractElementFromSegment("Contents", gShipmentParameters)
            optFragile3.IsChecked = True

            Dim PackJob_Contents As String = ExtractElementFromSegment("Contents", gShipmentParameters)
            cmbContentsKeyword.SelectedIndex = cmbContentsKeyword.Items.IndexOf(PackJob_Contents)
            cmbContentsKeyword.Text = PackJob_Contents


        Else
        ' Handle the case where gShipmentParameters is null, maybe show an error or default value
        Me.txtObjL.Text = "0"
        Me.txtObjW.Text = "0"
        Me.txtObjH.Text = "0"
        Me.txtObjDesc.Text = "0"
        Me.txtObjValue.Text = "0"
        Me.txtObjWeight.Text = "0.0"
        End If


        'Me.txtObjL.Text = "0"
        'Me.txtObjW.Text = "0"
        'Me.txtObjH.Text = "0"



        'Me.txtObjDesc.Text = "0"
        'Me.txtObjValue.Text = "0"
        'Me.txtObjWeight.Text = "0.0"
        Me.txtPackWeight.Text = "0.0"
        Me.txtPackOuterL.Text = "0"
        Me.txtPackOuterW.Text = "0"
        Me.txtPackOuterH.Text = "0"
        Me.txtPackDimW.Text = "0"
        Me.txtPackL_G.Text = "0"
        Me.txtPackPrice.Text = "0.0"


        For i = 1 To 5

            Dim D As Object = Me.FindName("lblMaterialsPrice" & i.ToString)
            D.content = "$ 0.00"

        Next
        For i = 1 To 5

            Dim D As Object = Me.FindName("lblLaborPrice" & i.ToString)
            D.content = "$ 0.00"

        Next
        For i = 1 To 5

            Dim D As Object = Me.FindName("lblBoxPrice" & i.ToString)
            D.content = "$ 0.00"

        Next
        For i = 1 To 5

            Dim D As Object = Me.FindName("lblPrice" & i.ToString)
            D.content = "$ 0.00"

        Next
        For i = 1 To 5

            Dim D As Object = Me.FindName("lblFragileBox" & i.ToString)
            D.content = "L x W x H"

        Next

        'Set double boxing option to hidden for now because it's not functional.
        'chkFragile1.Visibility = Visibility.Hidden
        'chkFragile2.Visibility = Visibility.Hidden
        'chkFragile3.Visibility = Visibility.Hidden
        'chkFragile4.Visibility = Visibility.Hidden
        'chkFragile5.Visibility = Visibility.Hidden
        'DoubleBox_Grid.Visibility = Visibility.Hidden

        PackingObjects_Grid.Visibility = Visibility.Hidden

        'Hide Tab that shows reviewer
        For Each currentTab As TabItem In PackMaster_TabControl.Items
            currentTab.Visibility = Visibility.Collapsed
        Next

        If isOpen_ShipNew = True Then
            ret = CalculateCharges()
            ret = DisplaySelectedPackage()
        End If


        load_PackMaster = True

    End Function

    Private Sub TextBox_GotFocus(sender As Object, e As RoutedEventArgs)
        Dim txtBox As TextBox = CType(sender, TextBox)
        txtBox.SelectAll()
    End Sub


    Private Function workshop_Save(ByVal CID As Integer) As Boolean
        Dim i As Integer
        '
        Call workshop_Delete(CID) ' delete if exist
        For i = 0 To UBound(m_RegPackItems) 'cmbRegDesc.count - 1
            Dim sql2cmd As New sqlINSERT
            Call sql2cmd.Qry_INSERT("ContentID", CStr(CID), sql2cmd.TXT_, True, False, "PackMaster")
            Call sql2cmd.Qry_INSERT("SKU", m_RegPackItems(i).SKU, sql2cmd.TXT_)
            Call sql2cmd.Qry_INSERT("Description", m_RegPackItems(i).Desc, sql2cmd.TXT_)
            Call sql2cmd.Qry_INSERT("Qty", CStr(m_RegPackItems(i).Qty), sql2cmd.NUM_)
            Call sql2cmd.Qry_INSERT("SlidePosition", CStr(m_RegPackItems(i).SlidePosition), sql2cmd.NUM_)
            Call sql2cmd.Qry_INSERT("UnitCost", CStr(m_RegPackItems(i).UnitCost), sql2cmd.NUM_)
            Call sql2cmd.Qry_INSERT("UnitPrice", CStr(m_RegPackItems(i).UnitPrice), sql2cmd.NUM_)
            Call sql2cmd.Qry_INSERT("ExtCost", CStr(m_RegPackItems(i).ExtCost), sql2cmd.NUM_)
            Call sql2cmd.Qry_INSERT("ExtPrice", CStr(m_RegPackItems(i).ExtPrice), sql2cmd.NUM_)
            Call sql2cmd.Qry_INSERT("Weight", CStr(m_RegPackItems(i).Weight), sql2cmd.NUM_)
            Call sql2cmd.Qry_INSERT("Summarize", CStr(m_RegPackItems(i).Summary), sql2cmd.NUM_)
            Dim sql2exe As String = sql2cmd.Qry_INSERT("Department", CStr(m_RegPackItems(i).Dept), sql2cmd.TXT_, False, True)
            workshop_Save = ShipRiteDb.execute_cmd(sql2exe)
            sql2cmd = Nothing
        Next i
    End Function

    Private Function workshop_Delete(ByVal CID As Long) As Boolean
        workshop_Delete = ShipRiteDb.execute_cmd("Delete * From PackMaster Where [ContentID]='" & CStr(CID) & "'")
    End Function

    Private Function workshop_SavePIECECHARGE(ByVal CID As Integer) As Boolean
        Dim sql2cmd As New sqlINSERT
        Call sql2cmd.Qry_INSERT("ContentID", CStr(CID), sql2cmd.TXT_, True, False, "PackMaster")
        Call sql2cmd.Qry_INSERT("SKU", "PIECECHARGE", sql2cmd.TXT_)
        Call sql2cmd.Qry_INSERT("Description", "AMOUNT CHARGED PER PIECE", sql2cmd.TXT_)
        Call sql2cmd.Qry_INSERT("Qty", CStr(Val(Me.txtRegPiecesNo.Text)), sql2cmd.NUM_)
        Call sql2cmd.Qry_INSERT("UnitPrice", CStr(m_DefaultPieceCharge), sql2cmd.NUM_)
        Call sql2cmd.Qry_INSERT("ExtPrice", CStr(_Convert.Round_Double2Decimals(Val(Me.txtRegPiecesNo.Text) * m_DefaultPieceCharge, 2)), sql2cmd.NUM_)
        Call sql2cmd.Qry_INSERT("Summarize", CStr(False), sql2cmd.NUM_)
        Call sql2cmd.Qry_INSERT("SlidePosition", CStr(Val(Me.txtRegMinFill.Text)), sql2cmd.NUM_) ' Min Fill Cushion
        Dim sql2exe As String = sql2cmd.Qry_INSERT("Department", "", sql2cmd.TXT_, False, True)
        workshop_SavePIECECHARGE = ShipRiteDb.execute_cmd(sql2exe)
        sql2cmd = Nothing
    End Function


    Private Function load_SetupFragile() As Boolean
        load_SetupFragile = False ' assume.
        If ShipRiteDb.Load_DataSet_Fragility() Then
            If ShipRiteDb.Get_DataTable(ShipRiteDb.tblPackFragile, dtlPackFragile) Then
                '' Use the ADO Filter property and the Clone method. This allows you to find the correct bookmark in the clone without affecting the rows that are visible in the recordset.
                load_SetupFragile = True
            End If
        End If
    End Function

    Private Function load_Combos(ByVal isLoadSKUs As Boolean) As Boolean
        Dim loadSKU As String = String.Empty
        Dim drows() As DataRow = Nothing
        ''
        load_Combos = dtlPackMaterials_AddValues()
        If load_Combos Then
            '
            ' Regular Tab:
            If _DataSet.Filter_DataTable(dtlPackMaterials, "MaterialsClass='Boxes' And [L]>0 And [W]>0 And [H]>0", drows) Then
                If isLoadSKUs Then loadSKU = cmbRegDesc0.Text
                Call load_ComboSKU(cmbRegDesc0, loadSKU, drows)
            End If
            If _DataSet.Filter_DataTable(dtlPackMaterials, "MaterialsClass='Boxes' And [L]>0 And [W]>0 And [H]>0", drows) Then
                If isLoadSKUs Then loadSKU = cmbRegDesc1.Text
                Call load_ComboSKU(cmbRegDesc1, loadSKU, drows)
            End If
            If _DataSet.Filter_DataTable(dtlPackMaterials, "MaterialsClass='Difficulty'", drows) Then
                If isLoadSKUs Then loadSKU = cmbRegDesc4.Text
                Call load_ComboSKU(cmbRegDesc4, loadSKU, drows)
            End If
            If _DataSet.Filter_DataTable(dtlPackMaterials, "MaterialsClass='Filler'", drows) Then
                If isLoadSKUs Then loadSKU = cmbRegDesc3.Text
                Call load_ComboSKU(cmbRegDesc3, loadSKU, drows)
            End If
            If _DataSet.Filter_DataTable(dtlPackMaterials, "MaterialsClass='Wrap'", drows) Then
                If isLoadSKUs Then loadSKU = cmbRegDesc2.Text
                Call load_ComboSKU(cmbRegDesc2, loadSKU, drows)
            End If
            If _DataSet.Filter_DataTable(dtlPackMaterials, "MaterialsClass<>''", drows) Then
                If isLoadSKUs Then loadSKU = cmbRegDesc5.Text
                Call load_ComboSKU(cmbRegDesc5, loadSKU, drows)
            End If
            '
            ' Modified Tab:
            If _DataSet.Filter_DataTable(dtlPackMaterials, "MaterialsClass='Boxes' And [L]>0 And [W]>0 And [H]>0", drows) Then
                If isLoadSKUs Then loadSKU = cmbModSelectBox.Text
                Call load_ComboSKU(cmbModSelectBox, loadSKU, drows)
            End If
            If _DataSet.Filter_DataTable(dtlPackMaterials, "MaterialsClass='Cardboard' And [L]>0 And [W]>0", drows) Then
                If isLoadSKUs Then loadSKU = cmbModCardboard.Text
                Call load_ComboSKU(cmbModCardboard, loadSKU, drows)
            End If
        End If
        ''
        drows = Nothing
    End Function
    Private Function load_ComboContentsDropDown() As Boolean
        Dim Cmblist = New List(Of PackagingData)
        Cmblist.Clear()

        For i As Short = 0 To Me.cmbContentsKeyword.Items.Count - 1
            Dim lstCmb As New _ListItemWithObject
            lstCmb = Me.cmbContentsKeyword.Items(i)
            Dim drowcoll As DataRow = lstCmb.ItemObject
            Dim listCmb As New PackagingData

            listCmb.CID = _Convert.Null2DefaultValue(drowcoll("CID"))

            listCmb.Contents = _Convert.Null2DefaultValue(drowcoll("Contents"))
            listCmb.Description = _Convert.Null2DefaultValue(drowcoll("Description"))
            listCmb.Weight = _Convert.Null2DefaultValue(drowcoll("Weight"), 0)
            listCmb.PackagingCost = _Convert.Null2DefaultValue(drowcoll("PackagingCost"), 0)
            listCmb.PackagingCharge = _Convert.Null2DefaultValue(drowcoll("PackagingCharge"), 0)
            listCmb.L = _Convert.Null2DefaultValue(drowcoll("L"), 0)
            listCmb.W = _Convert.Null2DefaultValue(drowcoll("W"), 0)
            listCmb.H = _Convert.Null2DefaultValue(drowcoll("H"), 0)
            Cmblist.Add(listCmb)


        Next i
        '
        lwContentsDropDown.ItemsSource = Cmblist

        '
        If Me.lwContentsDropDown.SelectedItems.Count > 0 Then
            Me.lwContentsDropDown.SelectedItems(0).Focused = True
        End If
        load_ComboContentsDropDown = True

    End Function

    Private Sub lwContentsDropDown_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles lwContentsDropDown.MouseDoubleClick
        Try
            ' Check if an item is selected in the ListView
            If Me.lwContentsDropDown.SelectedItem IsNot Nothing Then
                ' Hide the popup
                PackJobListViewPopup.IsOpen = False

                ' Use SelectedIndex if you want to match the ListView item index with the ComboBox
                Me.cmbContentsKeyword.SelectedIndex = Me.lwContentsDropDown.SelectedIndex
            Else
                ' Optional: Handle the case where no item is selected
                _MsgBox.InformationMessage("No item is selected.")
            End If
        Catch ex As Exception
            ' Handle any errors gracefully and log them
            _MsgBox.ErrorMessage(ex, "Failed to select Contents details...")
        End Try
    End Sub


    Private Sub lwContentsDropDown_KeyDown(sender As Object, e As System.Windows.Input.KeyEventArgs) Handles lwContentsDropDown.KeyDown
        Try
            If e.Key = Key.Enter Then
                If Me.lwContentsDropDown.SelectedItems.Count > 0 Then
                    Me.PackJobListViewPopup.Visibility = Visibility.Collapsed ' Hide the popup
                    Me.cmbContentsKeyword.SelectedIndex = Me.lwContentsDropDown.SelectedItems(0).Index
                End If
            End If
        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Failed to select Contents details...")
        End Try
    End Sub

    Private Sub cmbContentsKeyword_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles cmbContentsKeyword.SelectionChanged
        Try

            Dim retCost As Double
            Dim retSell As Double
            Dim retWeight As Double
            Dim CID As Long
            ''
            If Not -1 = Me.cmbContentsKeyword.SelectedIndex Then
                'DontCalc = True
                Dim lstCmb As New _ListItemWithObject
                lstCmb = Me.cmbContentsKeyword.SelectedItem
                If Not lstCmb Is Nothing Then
                    CID = lstCmb.ItemID
                    Me.txtObjWeight.Tag = Me.txtObjWeight.Text ' preserve the weight
                    'Call clear_Form(True)
                    Me.txtObjWeight.Text = Me.txtObjWeight.Tag ' preserve the weight
                    Me.txtRegMinFill.Text = "0"
                    If load_WorkshopDetails(CID) Then
                        Dim drow As DataRow = lstCmb.ItemObject
                        Me.txtObjL.Text = _Convert.Null2DefaultValue(drow("L"), "0")
                        Me.txtObjW.Text = _Convert.Null2DefaultValue(drow("W"), "0")
                        Me.txtObjH.Text = _Convert.Null2DefaultValue(drow("H"), "0")
                        Dim cmb_index As Integer = _Convert.Null2DefaultValue(drow("FragilityLevel"), "1")
                        Me.txtObjValue.Text = _Convert.Null2DefaultValue(drow("DeclaredValue"), "3")
                        If isOpen_ShipNew = True Then
                            Me.txtObjWeight.Text = Format(gShip.actualWeight, "0.0")
                            If Not 0 = gShip.DecVal Then Me.txtObjValue.Text = CStr(gShip.DecVal) ''ol#9.124(2/17).
                        End If
                        Me.txtObjDesc.Text = _Convert.Null2DefaultValue(drow("Description"), "")
                        'change_ObjectLHW(cmb_index) ''ol#9.58(10/13)... If "Object #" has increased then allow the user to enter another Object dimensions and sum them together as one Object.

                        change_Price_n_Weight_For2345() ' some workshop items depend on Object dims
                        If sum_Regular_Price_n_Weight(retCost, retSell, retWeight) Then
                        End If
                    End If
                    'End If
                End If
                lstCmb = Nothing
            End If
        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to select the Contents...")
            'Finally : DontCalc = False
        End Try
    End Sub


    Private Function load_ComboSKU(ByRef cmbRegDesc As ComboBox, ByVal loadSKU As String, ByVal drows() As DataRow) As Boolean
        Dim lstIndex As Integer
        ''
        cmbRegDesc.Items.Clear()
        'For i As Integer = 0 To drows.Length - 1
        'Dim lstCmb As New _ComboboxItemWithObject()
        'lstCmb.ItemText = _Convert.Null2DefaultValue(drows(i)("SKU"))
        'lstCmb.ItemObject = drows(i)
        'cmbRegDesc.Items.Add(lstCmb)
        'Next i
        Call _ComboBox.AddAll_ItemObject(cmbRegDesc, "SKU", drows)
        ''
        load_ComboSKU = (0 < cmbRegDesc.Items.Count)
        If Not 0 = Len(loadSKU) Then
            If search_ComboSKU(loadSKU, cmbRegDesc, lstIndex) Then
                cmbRegDesc.SelectedIndex = lstIndex
            End If
        End If
        ''
    End Function
    Private Function load_ComboContents(ByVal drowcoll As DataRowCollection) As Boolean
        Dim i As Integer
        ''
        cmbContentsKeyword.Items.Clear()
        If drowcoll.Count > 0 Then
            For i = 0 To drowcoll.Count - 1
                Dim lstCmb As New _ListItemWithObject()
                lstCmb.ItemID = _Convert.Null2DefaultValue(drowcoll(i)("CID"), 0)
                lstCmb.ItemText = _Convert.Null2DefaultValue(drowcoll(i)("Contents"))
                lstCmb.ItemObject = drowcoll(i)
                cmbContentsKeyword.Items.Add(lstCmb)
            Next i
        End If
        load_ComboContents = (0 < cmbContentsKeyword.Items.Count)
        ''
    End Function

#End Region
    Private Function change_ObjectLHW(ByVal cmb_index As Integer) As Boolean
        ''ol#1.2.05(6/15)... If user manually changes Dims then force 'Find Best Fit' algorithm again.
        ''ol#9.55(9/16)... If "Object" dims were changed while Inner/Outer box(es) were already selected then don't change them unless they don't fit.

        ''
        CalculateCharges()

        Dim Selection As Object = Me.FindName("cmdFragile" & cmb_index)
        CType(Selection, Button).RaiseEvent(New RoutedEventArgs(Button.ClickEvent))


    End Function
    Private Function load_WorkshopDetails(ByVal contID As Long) As Boolean
        Dim segmentset As String = Nothing
        Dim dreader As String
        Dim itemSKU As String
        Dim lstIndex As Integer
        Dim cmbIndex As Integer
        Dim fld As Integer
        ''
        load_WorkshopDetails = ShipRiteDb.PackMaster_GetPackageDetails(contID, segmentset)
        If load_WorkshopDetails Then

            Do Until segmentset = ""

                dreader = GetNextSegmentFromSet(segmentset)
                itemSKU = ExtractElementFromSegment("SKU", dreader)

                Dim result As String = ExtractElementFromSegment("Summarize", dreader)
                Me.chkPrintDetails.IsChecked = (result = "True")

                If "PIECECHARGE" = itemSKU Then
                    Me.txtRegPiecesNo.Text = _Convert.Null2DefaultValue(ExtractElementFromSegment("Qty", dreader), "1")
                    Call object_SetPieceNoChange() ''ol#9.58(10/13)... If "Object #" has increased then allow the user to enter another Object dimensions and sum them together as one Object.
                    Me.txtRegMinFill.Text = _Convert.Null2DefaultValue(ExtractElementFromSegment("SlidePosition", dreader), "1")

                ElseIf Not 0 = Len(itemSKU) Then
                    ' try to find it in the Workshop invenotry
                    For cmbIndex = 0 To 5
                        Dim cmbRegDesc As ComboBox = Nothing : find_cmbRegDesc_byINDEX(cmbIndex, cmbRegDesc)
                        Dim txtRegQty As TextBox = Nothing : find_txtRegQty_byINDEX(cmbIndex, txtRegQty)

                        If cmbIndex = fld Then ' only empty ones allowed
                            If search_ComboSKU(itemSKU, cmbRegDesc, lstIndex) Then
                                If iLABOR = cmbIndex Then '"PAK_LABOR"
                                    Me.txtRegQty4.Text = ExtractElementFromSegment("Qty", dreader)
                                Else
                                    txtRegQty.Text = ExtractElementFromSegment("SlidePosition", dreader)
                                End If
                                m_RegPackItems(cmbIndex).Qty_SetbyUser = ExtractElementFromSegment("Qty", dreader) ''ol#9.123(1/23)... Reviewer 'Qty' column values are allowed to be a adjusted resulting in recalculation of 'Ext Price'.
                                cmbRegDesc.SelectedIndex = lstIndex
                                Exit For
                            End If
                        End If
                    Next cmbIndex

                End If
                fld += 1
            Loop
        End If

    End Function
#Region "Contents"
    Private Function contents_Select(ByVal CID As Long) As Boolean
        Dim cmbListIndex As Integer
        contents_Select = _ComboBox.IsExist_ItemObject(Me.cmbContentsKeyword, CID, cmbListIndex)
        If contents_Select Then
            Me.cmbContentsKeyword.SelectedIndex = cmbListIndex
        End If
    End Function
    Private Function contents_Save(ByVal isNew As Boolean) As Boolean
        Dim retCost As Double
        Dim retSell As Double
        Dim retWeight As Double
        Dim tempCID As String = String.Empty
        Dim retCID As Long

        If sum_Regular_Price_n_Weight(retCost, retSell, retWeight) Then
            Dim lstCmb As _ListItemWithObject = Me.cmbContentsKeyword.SelectedItem
            If Not lstCmb Is Nothing And Not isNew Then
                If Not 0 = lstCmb.ItemID Then
                    ' Edit:
                    retCID = Val(lstCmb.ItemID)

                    If contents_Edit(retCost, retSell, retWeight, retCID) Then
                        If workshop_Save(retCID) Then
                            contents_Save = workshop_SavePIECECHARGE(retCID)
                        End If
                    End If
                End If
            ElseIf isNew Then
                ' Add New:
                If contents_AddNew(retCost, retSell, retWeight, tempCID) Then
                    If contents_GetCID(tempCID, retCID) Then

                        If workshop_Save(retCID) Then

                            Me.cmbContentsKeyword.Tag = CStr(retCID) ''ol#9.55(9/20)... "Apply" button will transfer a currently displayed pack-job to POS/ShipMaster without assigning it a name (LASTJOB).
                            contents_Save = workshop_SavePIECECHARGE(retCID)
                        End If
                    End If
                    End If
            End If
        End If
    End Function
    Private Function contents_Delete(ByVal CID As Long) As Boolean
        contents_Delete = ShipRiteDb.execute_cmd("Delete * From Contents Where [CID]=" & CStr(CID))
    End Function
    Private Function contents_GetCID(ByVal tempCID As String, ByRef CID As Long) As Boolean
        contents_GetCID = ShipRiteDb.Contents_GetCID(tempCID, CID)
    End Function
    Private Function contents_AddNew(ByVal iCost As Double, ByVal iSell As Double, ByVal iWeight As Double, ByRef tempCID As String) As Boolean
        Dim sql2cmd As New sqlINSERT
        Dim sql2exe As String
        tempCID = CStr(Now) ' temp id
        Call sql2cmd.Qry_INSERT("Contents", Me.cmbContentsKeyword.Text, sql2cmd.TXT_, True, False, "Contents")
        Call sql2cmd.Qry_INSERT("Description", Me.txtObjDesc.Text, sql2cmd.TXT_)
        Call sql2cmd.Qry_INSERT("L", Me.txtObjL.Text, sql2cmd.TXT_)
        Call sql2cmd.Qry_INSERT("W", Me.txtObjW.Text, sql2cmd.TXT_)
        Call sql2cmd.Qry_INSERT("H", Me.txtObjH.Text, sql2cmd.TXT_)
        Call sql2cmd.Qry_INSERT("Weight", CStr(iWeight), sql2cmd.NUM_)
        Call sql2cmd.Qry_INSERT("DeclaredValue", Me.txtObjValue.Text, sql2cmd.NUM_)
        Call sql2cmd.Qry_INSERT("PackagingCost", CStr(iCost), sql2cmd.NUM_)
        Call sql2cmd.Qry_INSERT("PackagingCharge", CStr(iSell), sql2cmd.NUM_)
        Call sql2cmd.Qry_INSERT("outerL", Me.txtPackOuterL.Text, sql2cmd.NUM_)
        Call sql2cmd.Qry_INSERT("outerW", Me.txtPackOuterW.Text, sql2cmd.NUM_)
        Call sql2cmd.Qry_INSERT("outerH", Me.txtPackOuterH.Text, sql2cmd.NUM_)
        Call sql2cmd.Qry_INSERT("Category", tempCID, sql2cmd.TXT_)
        Call sql2cmd.Qry_INSERT("FragilityLevel", global_fragility_index, sql2cmd.NUM_)

        sql2exe = sql2cmd.Qry_INSERT("POSContents", Me.txtObjDesc.Text, sql2cmd.TXT_, False, True)

        contents_AddNew = ShipRiteDb.execute_cmd(sql2exe)
        sql2cmd = Nothing
    End Function
    Private Function contents_Edit(ByVal iCost As Double, ByVal iSell As Double, ByVal iWeight As Double, ByVal CID As Integer) As Boolean
        Dim sql2exe As String = String.Empty
        Dim sql2cmd As New sqlUpdate
        Call sql2cmd.Qry_UPDATE("Contents", Me.cmbContentsKeyword.Text, sql2cmd.TXT_, True, False, "Contents", "CID=" & CStr(CID))
        Call sql2cmd.Qry_UPDATE("Description", Me.txtObjDesc.Text, sql2cmd.TXT_)
        Call sql2cmd.Qry_UPDATE("L", Me.txtObjL.Text, sql2cmd.TXT_)
        Call sql2cmd.Qry_UPDATE("W", Me.txtObjW.Text, sql2cmd.TXT_)
        Call sql2cmd.Qry_UPDATE("H", Me.txtObjH.Text, sql2cmd.TXT_)
        Call sql2cmd.Qry_UPDATE("Weight", CStr(iWeight), sql2cmd.NUM_)
        Call sql2cmd.Qry_UPDATE("DeclaredValue", Me.txtObjValue.Text, sql2cmd.NUM_)
        Call sql2cmd.Qry_UPDATE("PackagingCost", CStr(iCost), sql2cmd.NUM_)
        Call sql2cmd.Qry_UPDATE("PackagingCharge", CStr(iSell), sql2cmd.NUM_)
        Call sql2cmd.Qry_UPDATE("outerL", Me.txtPackOuterL.Text, sql2cmd.NUM_)
        Call sql2cmd.Qry_UPDATE("outerW", Me.txtPackOuterW.Text, sql2cmd.NUM_)
        Call sql2cmd.Qry_UPDATE("outerH", Me.txtPackOuterH.Text, sql2cmd.NUM_)
        Call sql2cmd.Qry_UPDATE("Category", "", sql2cmd.TXT_)
        sql2exe = sql2cmd.Qry_UPDATE("POSContents", Me.txtObjDesc.Text, sql2cmd.TXT_, False, True)
        contents_Edit = ShipRiteDb.execute_cmd(sql2exe)
        sql2cmd = Nothing
    End Function
#End Region

    Private Function dtlPackMaterials_AddValues() As Boolean
        If ShipRiteDb.Load_DataSet_Inventory_PackMaterials(ShipRiteDb.tblPackMaterials) Then
            If ShipRiteDb.Get_DataTable(ShipRiteDb.tblPackMaterials, dtlPackMaterials) Then
                '' Use the ADO Filter property and the Clone method. This allows you to find the correct bookmark in the clone without affecting the rows that are visible in the recordset.
                dtlPackMaterials_Filter = dtlPackMaterials.Clone
                dtlPackMaterials_AddValues = True
            End If
        End If
        ''
    End Function

    Private Function search_ComboSKU(ByVal itemSKU As String, ByRef cmbRegDesc As ComboBox, ByRef lstIndex As Integer) As Boolean
        lstIndex = -1 '' assume.
        For i As Integer = 0 To cmbRegDesc.Items.Count - 1
            If itemSKU = cmbRegDesc.Items(i).ToString Then '' SKU search
                lstIndex = i
                Exit For
            End If
        Next i
        ''
        search_ComboSKU = (Not -1 = lstIndex)
    End Function

    Private Sub ChkPackDoubleBox_Click(sender As Object, e As RoutedEventArgs) Handles chkPackDoubleBox.Click

        Dim ret As Integer
        For i = 1 To 5
            gItemSet(2).Units.L(i) += gItemSet(0).DefaultDoubleBoxLabor

        Next

        ret = CalculateCharges()
        change_DoubleBoxStatus()
        change_Price_n_Weight_For2345()

        SetWarningColor()


    End Sub

    Private Sub TxtObjL_KeyDown(sender As Object, e As KeyEventArgs) Handles txtObjL.KeyDown, txtObjW.KeyDown, txtObjH.KeyDown, txtObjValue.KeyDown

        NeedsRecalculation = True

    End Sub

    Private Sub TxtObjL_LostFocus(sender As Object, e As RoutedEventArgs) Handles txtObjL.LostFocus, txtObjW.LostFocus, txtObjH.LostFocus, txtObjValue.LostFocus

        Dim ret As Long = 0
        If CalculationInProgress = True Then

            Exit Sub

        End If
        If txtObjL.Text <> "0" And txtObjW.Text <> "0" And txtObjH.Text <> "0" Then
            ret = CalculateCharges()

        End If


        NeedsRecalculation = False

    End Sub

    Private Sub txtObjValue_TextChanged(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles txtObjValue.TextChanged
        If Not IsNumeric(Me.txtObjValue.Text) Then
            Me.txtObjValue.Text = "0"
        Else
            Call check_DoubleBoxingThreshold() ''ol#9.124(2/17)... if setup 'Double Boxing Threshold' is less or equal to the Declared value the Object should be double boxed.
            'Call change_FragileValues(Val(Me.txtObjValue.Text))
            Call change_Price_n_Weight(iLABOR) ''ol#9.124(2/20)... 'Fragile Labor Units' (if any) are always added to the 'Default Labor Units'.
        End If
    End Sub

    Private Sub check_DoubleBoxingThreshold()
        ''ol#9.124(2/17)... if setup 'Double Boxing Threshold' is less or equal to the Declared value the Object should be double boxed.
        If Not 0 = m_DoubleBoxThreshold Then
            If m_DoubleBoxThreshold <= Val(Me.txtObjValue.Text) Then
                chkPackDoubleBox.IsChecked = True
                ChkPackDoubleBox_Click(chkPackDoubleBox, Nothing)


            End If

        End If
    End Sub

    Private Sub OptFragile1_Click(sender As Object, e As RoutedEventArgs) Handles optFragile1.Click, optFragile2.Click, optFragile3.Click, optFragile4.Click, optFragile5.Click
        Dim ret As Integer = 0

        Dim j = GetIndexOfMaterials("Box")

        load_Combos(False)
        If txtObjL.Text <> "0" And txtObjW.Text <> "0" And txtObjH.Text <> "0" Then

            ret = DisplaySelectedPackage()
        Else
            Return
        End If


        SetWarningColor()

    End Sub

    Private Sub CmdFragile1_Click(sender As Object, e As RoutedEventArgs) Handles cmdFragile1.Click, cmdFragile2.Click, cmdFragile3.Click, cmdFragile4.Click, cmdFragile5.Click

        Dim ret As Integer = 0
        Dim cmd As Button = CType(sender, Button)
        Dim level As Integer = Val(_Controls.Right(cmd.Name, 1))
        Dim SELECT_ButtonIndex As String = sender.name.ToString.Last



        Dim Selection As Object = Me.FindName("optFragile" & SELECT_ButtonIndex)

        Selection.ischecked = True
        OptFragile1_Click(Selection, Nothing)
        If txtObjL.Text <> "0" And txtObjW.Text <> "0" And txtObjH.Text <> "0" Then
            change_DoubleBoxStatus()
            dtlPackMaterials_AddValues()

            change_Price_n_Weight_For2345()
            DisplayInvoiceDetail(global_fragility_index)
        End If
        PackMaster_TabControl.SelectedIndex = 1


    End Sub

    Private Sub cmdPOSApply_Click(sender As Object, e As RoutedEventArgs) Handles cmdPOSApply.Click
        Dim ret As Integer = 0
        FillQty_Packmaster = Val(lblQtyFill.Content)
        WrapQty_Packmaster = Val(lblQtyWrap.Content)
        LaborQty_Packmaster = Val(txtRegQty4.Text)
        BoxQty_Packmaster = Val(txtRegQty1.Text)
        DoubleBoxQty_Packmaster = Val(txtRegQty0.Text)
        OtherQty_Packmaster = Val(txtRegQty5.Text)

        ret = PostPackagingToPOS(global_fragility_index)
        If isOpen_ShipNew Then
            gShipmentParameters = ""

            gShipmentParameters = AddElementToSegment(gShipmentParameters, "Length", txtPackOuterL.Text)
            gShipmentParameters = AddElementToSegment(gShipmentParameters, "Width", txtPackOuterW.Text)
            gShipmentParameters = AddElementToSegment(gShipmentParameters, "Height", txtPackOuterH.Text)

            gShipmentParameters = AddElementToSegment(gShipmentParameters, "Weight", txtPackWeight.Text)
            gShipmentParameters = AddElementToSegment(gShipmentParameters, "Charge", txtPackPrice.Text)
            gShipmentParameters = AddElementToSegment(gShipmentParameters, "PackL_G", txtPackL_G.Text)

            gShipmentParameters = AddElementToSegment(gShipmentParameters, "Contents", txtObjDesc.Text)
            isOpen_PackMaster = True
            Dim win As New ShipManager(Me)
            win.ShowDialog(Me)

            isOpen_PackMaster = False
        Else
            BackButton_Click(sender, e)

        End If


    End Sub

    Private Sub cmdOnOffFragile_Click(sender As Object, e As RoutedEventArgs) Handles cmdOnOffFragile.Click
        PackMaster_TabControl.SelectedIndex = 0
    End Sub

    Private Sub cmdSetup_Click(sender As Object, e As RoutedEventArgs) Handles cmdSetup.Click
        Try
            Dim win As New ShippingSetup(Me, 4)
            win.ShowDialog(Me)

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub cmdContentsDropDown_Click(sender As System.Object, e As System.EventArgs) Handles cmdContentsDropDown.Click
        Try
            PackJobListViewPopup.IsOpen = True
            Me.load_ComboContentsDropDown()
        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to select Contents...")
        End Try
    End Sub




    Private Sub cmdSaveAsNew_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles cmdSaveAsNew.Click
        Try
            If 0 = Len(cmbContentsKeyword.Text) Then
                cmbContentsKeyword.Text = "MISC " & txtObjL.Text & "x" & txtObjW.Text & "x" & txtObjH.Text
                If chkPackDoubleBox.IsChecked Then
                    cmbContentsKeyword.Text = cmbContentsKeyword.Text & "dbl" ' custom name to avoid typing
                End If
            End If
            If contents_Save(True) Then
                Call load_Contents()
                MsgBox("New Packaging Contents was added successfully!", MsgBoxStyle.Information)
            Else
            End If
        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to add new Contents...")
        End Try
    End Sub

    Private Sub cmdSave_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles cmdSave.Click
        Try
            If Me.cmbContentsKeyword.SelectedItem IsNot Nothing Then
                If MsgBoxResult.Yes = MsgBox("Save Changes to the selected Contents pack job?", MsgBoxStyle.YesNo + MsgBoxStyle.Question, "Save Changes?") Then
                    If 0 = Len(Me.cmbContentsKeyword.Text) Then
                        Me.cmbContentsKeyword.Text = "MISC " & Me.txtObjL.Text & "x" & Me.txtObjW.Text & "x" & Me.txtObjH.Text
                        If Me.chkPackDoubleBox.IsChecked Then
                            Me.cmbContentsKeyword.Text = Me.cmbContentsKeyword.Text & "dbl" ' custom name to avoid typing
                        End If
                    End If
                    If contents_Save(False) Then
                        Call load_Contents()
                        MsgBox("Selected Packaging Contents was edited successfully!", MsgBoxStyle.Information, Me.Title)
                    End If
                End If
            Else
                MsgBox("Select a Pack Job from the 'Contents Keyword' drop-down box to proceed!", MsgBoxStyle.Exclamation, Me.Title)
            End If
        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to edit the Contents...")
        End Try
    End Sub
    Private Sub cmdDelete_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles cmdDelete.Click
        Try
            If Me.cmbContentsKeyword.SelectedItem IsNot Nothing Then
                If MsgBoxResult.Yes = MsgBox("Delete selected Contents pack job?", MsgBoxStyle.YesNo + MsgBoxStyle.Question, "Delete?") Then
                    Dim lstCmb As _ListItemWithObject = Me.cmbContentsKeyword.SelectedItem
                    If Not 0 = lstCmb.ItemID Then
                        If contents_Delete(lstCmb.ItemID) Then
                            If workshop_Delete(lstCmb.ItemID) Then
                                load_PackMaster()
                                clear_Form()
                                PackMaster_TabControl.SelectedIndex = 0
                                MsgBox("Contents of the package was deleted successfully!", MsgBoxStyle.Information, Me.Title)
                            End If
                        End If
                    End If
                End If
            Else
                MsgBox("Select a Pack Job from the 'Contents Keyword' drop-down box to proceed!", MsgBoxStyle.Exclamation, Me.Title)
            End If
        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to delete...")
        End Try
    End Sub

    Private Sub cmdComboDropDown_Click(sender As System.Object, e As System.EventArgs) Handles cmdComboDropDown0.Click, cmdComboDropDown1.Click, cmdComboDropDown2.Click, cmdComboDropDown3.Click, cmdComboDropDown4.Click, cmdComboDropDown5.Click
        Try
            Dim Index As Integer = Val(_Controls.Right(sender.Name, 1))

            Dim cmbRegDesc As ComboBox = Nothing : find_cmbRegDesc_byINDEX(Index, cmbRegDesc)

            Me.load_ComboDropDown(Index)
            InventoryLIstViewPopup.IsOpen = True


        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to select SKU...")
        End Try
    End Sub

    Private Sub lwRegDesc_ListView_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles lwRegDesc.SelectionChanged

        Dim cmbRegDesc As ComboBox = Nothing : find_cmbRegDesc_byINDEX(lwRegDesc_index, cmbRegDesc)
        If lwRegDesc.SelectedItems.Count > 0 Then
            cmbRegDesc.Text = lwRegDesc.SelectedItem.SKU
            InventoryLIstViewPopup.IsOpen = False

        End If



    End Sub


    Private Function change_Price_n_Weight_For2345() As Boolean
        ''ol#1.2.28(12/14)... Don't calculate left side until user selects Fragility Level.
        If -1 = Me.cmbRegDesc4.SelectedIndex And 0 < m_DefaultLabor And 0 < Me.cmbRegDesc4.Items.Count And Not 0 = Len(Me.cmbRegDesc1.Text) Then
            'Me.cmbRegDesc4.SelectedIndex = 0
        Else
            change_Price_n_Weight_For2345 = change_Price_n_Weight(iLABOR) ' labor
        End If
        If -1 = Me.cmbRegDesc3.SelectedIndex And 0 < Val(Me.txtRegMinFill.Text) And 0 < Me.cmbRegDesc3.Items.Count And Not 0 = Len(Me.cmbRegDesc1.Text) Then
            'Me.cmbRegDesc3.SelectedIndex = 0
        Else
            change_Price_n_Weight_For2345 = change_Price_n_Weight(i_FILL) ' filler uses Object Volume
        End If
        If -1 = Me.cmbRegDesc2.SelectedIndex And 0 < Val(Me.txtRegMinFill.Text) And 0 < Me.cmbRegDesc2.Items.Count And Not 0 = Len(Me.cmbRegDesc1.Text) Then
            'Me.cmbRegDesc2.SelectedIndex = 0
        Else
            change_Price_n_Weight_For2345 = change_Price_n_Weight(iWRAP) ' wrap uses Object Volume
        End If
        If 0 > Me.cmbRegDesc2.SelectedIndex Then
            change_Price_n_Weight_For2345 = change_Price_n_Weight(iOTHER) ' could be filler or wrap
        End If

        If chkPackDoubleBox.IsChecked Then
            change_Price_n_Weight_For2345 = change_Price_n_Weight(iINNER)
            change_Price_n_Weight_For2345 = change_Price_n_Weight(iOUTER)

        Else change_Price_n_Weight_For2345 = change_Price_n_Weight(iINNER)

        End If
    End Function

    Private Function change_Price_n_Weight(ByVal Index As Short) As Boolean
        Dim retCost As Double
        Dim retSell As Double
        Dim retWeight As Double
        ''
        If calc_Regular_Price_n_Weight(Index) Then
            If sum_Regular_Price_n_Weight(retCost, retSell, retWeight) Then
                change_Price_n_Weight = True
            End If
        End If
        ''
    End Function

    Private Function calc_Regular_Price_n_Weight(ByVal Index As Short) As Boolean

        Dim iCost As Double
        Dim iSell As Double
        Dim iWeight As Double
        Dim iDesc As String = String.Empty
        Dim iMatClass As String = String.Empty
        Dim iDept As String = String.Empty
        Dim itemL As Single : itemL = Val(Me.txtObjL.Text)
        Dim itemW As Single : itemW = Val(Me.txtObjW.Text)
        Dim itemH As Single : itemH = Val(Me.txtObjH.Text)
        Dim outerL As Integer : outerL = Val(Me.txtPackOuterL.Text)
        Dim outerW As Integer : outerW = Val(Me.txtPackOuterW.Text)
        Dim outerH As Integer : outerH = Val(Me.txtPackOuterH.Text)
        Dim outerVol As Integer : outerVol = outerL * outerW * outerH
        Dim itemVol As Single : itemVol = itemL * itemW * itemH
        Dim innerVol As Integer : innerVol = Val(Me.txtPackInnerL.Text) * Val(Me.txtPackInnerW.Text) * Val(Me.txtPackInnerH.Text)
        ''ol#9.51(8/27)... Calculate Wrap volume (QTY=1 is 0.5'') to include in Filler calculation.
        Dim wrapHight As Single = 0.5
        Dim wrapLWH As Single : wrapLWH = Val(Me.txtRegQty2.Text) * wrapHight
        Dim wrapL As Single : wrapL = itemL + wrapLWH
        Dim wrapW As Single : wrapW = itemW + wrapLWH
        Dim wrapH As Single : wrapH = itemH + wrapLWH
        Dim wrapVol As Single : wrapVol = wrapL * wrapW * wrapH ''ol#9.51(8/27).
        ''ol#1.2.27(12/10)... Fill should be calculated based on the object dimensions if user clicks +/- buttons
        Dim fillHight As Single = Val(Me.txtRegQty3.Text)
        Dim fillVol As Single = (wrapL + fillHight) * (wrapW + fillHight) * (wrapH + fillHight) ''ol#1.2.26(12/1).

        Dim parentX As TreeViewItem = Nothing
        Dim childX As TreeViewItem = Nothing
        Dim qtyFormula As String = String.Empty
        Dim cstFormula As String = String.Empty
        Dim whtFormula As String = String.Empty
        Dim qtyMeasure As String = String.Empty
        Dim laborMin As Double
        ''
        Dim cmbRegDesc As ComboBox = Nothing : find_cmbRegDesc_byINDEX(Index, cmbRegDesc)
        Dim txtRegQty As TextBox = Nothing : find_txtRegQty_byINDEX(Index, txtRegQty)
        ''A
        calc_Regular_Price_n_Weight = True '' assume.
        ''
        If 0 < Val(txtRegQty.Text) Then
            calc_Regular_Price_n_Weight = find_Price_n_Weight(cmbRegDesc.Text, iCost, iSell, iWeight, iDesc, iMatClass, iDept)
            If calc_Regular_Price_n_Weight Then
                ''ol#9.55(9/23)... We need an indicator for PackMasterII items to indicate if the item was added to the Preview list.
                If m_RegPackItems(Index).IsDisplayed Then ' check only if item was already displayed
                    calc_Regular_Price_n_Weight = (Not Val(txtRegQty.Text) = m_RegPackItems(Index).Qty) Or (Not m_RegPackItems(Index).SKU = cmbRegDesc.Text) Or "Difficulty" = iMatClass Or "Filler" = iMatClass
                    calc_Regular_Price_n_Weight = (Not itemVol = m_itemVol) Or calc_Regular_Price_n_Weight
                End If
                If calc_Regular_Price_n_Weight Then
                    '
                    m_RegPackItems(Index).SKU = cmbRegDesc.Text
                    m_RegPackItems(Index).Dept = iDept ''ol#9.57(10/6)... Department of a packing item should be checked against tax requirements.
                    m_RegPackItems(Index).Desc = iDesc
                    m_RegPackItems(Index).UnitCost = iCost
                    m_RegPackItems(Index).UnitPrice = iSell
                    m_RegPackItems(Index).SlidePosition = Val(txtRegQty.Text)
                    m_RegPackItems(Index).Summary = _Convert.Integer2Boolean(chkPrintDetails.IsChecked)
                    ''ol#9.55(9/23)... We need an indicator for PackMasterII items to indicate if the item was added to the Preview list.
                    m_RegPackItems(Index).IsDisplayed = tvNode_AddParent(Index, parentX) ''ol#9.55(9/23).
                    qtyMeasure = "QTY"
                    '
                    '"MaterialsClass='Difficulty' Or MaterialsClass='Filler' Or MaterialsClass='Wrap'
                    Select Case iMatClass
                        Case "Labor"
                            ''ol#9.123(1/23)... Reviewer 'Qty' column values are allowed to be a adjusted resulting in recalculation of 'Ext Price'.
                            ''                      m_RegPackItems(Index).Qty = Val(txtRegQty.Text)
                            If 0 < m_RegPackItems(Index).Qty_SetbyUser Then
                                m_RegPackItems(Index).Qty = m_RegPackItems(Index).Qty_SetbyUser
                            Else
                                m_RegPackItems(Index).Qty = Val(txtRegQty.Text)
                            End If ''ol#9.123(1/23).

                            ''ol#1.2.27(12/10)... Let user to decrease Labor SKU below the default value if user clicks +/- buttons.
                            ' '' compare to Default Labor units:
                            '' ''ol#9.124(2/20)... 'Fragile Labor Units' (if any) are always added to the 'Default Labor Units'.
                            ''If m_RegPackItems(Index).SlidePosition < m_DefaultLabor Then
                            ''    qtyFormula = "DefaultLabor(" & CStr(m_DefaultLabor) & ")"
                            ''    m_RegPackItems(Index).SlidePosition = m_DefaultLabor
                            ''    m_RegPackItems(Index).Qty = m_DefaultLabor
                            ''    txtRegQty.Text = CStr(m_DefaultLabor)
                            ''    m_Labor_RegularTab = m_DefaultLabor
                            ''End If

                            ' compare to Fragile Labor units:
                            ''ol#1.1.89(2/5)... Default Labor minutes is being added twice to Fregility minutes.
                            ''If m_RegPackItems(Index).SlidePosition <= (get_FragileLaborUnits() + m_DefaultLabor) Then
                            ''    m_RegPackItems(Index).SlidePosition = get_FragileLaborUnits() + m_DefaultLabor
                            'If m_RegPackItems(Index).SlidePosition <= get_FragileLaborUnits() Then
                            '    m_RegPackItems(Index).SlidePosition = get_FragileLaborUnits()  ''ol#1.1.88(2/5).
                            '    qtyFormula = "FragileLabor(" & CStr(m_RegPackItems(Index).SlidePosition) & ")"
                            '    m_RegPackItems(Index).Qty = m_RegPackItems(Index).SlidePosition
                            '    txtRegQty.Text = CStr(m_RegPackItems(Index).SlidePosition)
                            '    m_Labor_RegularTab = m_RegPackItems(Index).SlidePosition
                            'End If

                            ''ol#9.55(9/17)... "Labor SKU" will be incrementing by QTY of 1 = 1 min.
                            laborMin = m_RegPackItems(Index).SlidePosition * 1
                            qtyFormula = "QTY(" & CStr(m_RegPackItems(Index).SlidePosition) & ") * 1 min ***Increments are by 1 min of hourly rate"
                            cstFormula = "UnitPrice(" & Format(m_RegPackItems(Index).UnitPrice, "0.00") & ") / 60" ''ol#9.54(9/2).
                            m_RegPackItems(Index).UnitCost = _Convert.Round_Double2Decimals(m_RegPackItems(Index).UnitCost / 60, 2) ' Increments are by 1 minutes of hourly rate
                            m_RegPackItems(Index).UnitPrice = _Convert.Round_Double2Decimals(m_RegPackItems(Index).UnitPrice / 60, 2) ' Increments are by 1 minutes of hourly rate
                            ''ol#9.55(9/17).
                            lblQtyLabor.Content = (CStr(laborMin) & " min.") ''ol#9.53(8/30)... Labels with value measures were added to Workshop items to give better visual.
                            If m_RegPackItems(Index).IsDisplayed Then
                                qtyMeasure = "LaborUnits"
                                ''ol#9.54(9/2)... [Labor Min] time calculation logic shown in Calculation Details Reviewer.
                                ''ol#9.54(9/2)Call tvNode_AddChild(Index, "Labor Units", qtyFormula, m_RegPackItems(Index).Qty, parentX, childX)

                                ' Call the tvNode_AddChild function for "Labor Min"
                                tvNode_AddChild(Index, "Labor Min", qtyFormula, laborMin, parentX, childX)

                                ' Call the tvNode_AddChild function for "Unit Price"
                                tvNode_AddChild(Index, "Unit Price", cstFormula, m_RegPackItems(Index).UnitPrice, parentX, childX)

                            End If
                        Case "Filler"
                            ''ol#9.123(1/23)... Reviewer 'Qty' column values are allowed to be a adjusted resulting in recalculation of 'Ext Price'.
                            If 0 < m_RegPackItems(Index).Qty_SetbyUser Then
                                m_RegPackItems(Index).Qty = m_RegPackItems(Index).Qty_SetbyUser
                            Else
                                ''ol#1.2.27(12/10)... Fill should be calculated based on the object dimensions if user clicks +/- buttons
                                ''ol#1.2.26(12/1)... 'Auto Fill SKU' check box added to enable/disable change the box size based on user adjusted Fill volume.
                                ''If Me.chkAutoFill.Checked Then
                                ''    If 0 < wrapLWH Then
                                ''        ''ol#1.2.25(11/24)... Fill cub. ft. was not calculated properly if there was Wrap involved.
                                ''        ''  ''ol#1.1.76(10/8)... 'Fill SKU' have +/- buttons and will be unlimited.
                                ''        ''  m_RegPackItems(Index).Qty = _Convert.Round_Double2Decimals((outerVol - wrapVol) / 1728 * m_RegPackItems(Index).SlidePosition, 1)
                                ''        m_RegPackItems(Index).Qty = _Convert.Round_Double2Decimals((outerVol - wrapVol) / 1728, 1)
                                ''    Else
                                ''        m_RegPackItems(Index).Qty = _Convert.Round_Double2Decimals((outerVol - itemVol) / 1728, 1)
                                ''    End If
                                ''Else
                                ''    If 0 < wrapLWH Then
                                ''        m_RegPackItems(Index).Qty = _Convert.Round_Double2Decimals((outerVol - wrapVol + fillVol) / 1728, 1)
                                ''    Else
                                ''        m_RegPackItems(Index).Qty = _Convert.Round_Double2Decimals((outerVol - itemVol + fillVol) / 1728, 1)
                                ''    End If
                                ''End If
                                If 1 = Val(Me.txtRegQty3.Text) Then

                                    If 0 < wrapLWH Then
                                        m_RegPackItems(Index).Qty = _Convert.Round_Double2Decimals((outerVol - wrapVol) / 1728, 1)
                                    Else
                                        m_RegPackItems(Index).Qty = _Convert.Round_Double2Decimals((outerVol - itemVol) / 1728, 1)

                                    End If
                                Else
                                    If 0 < wrapLWH Then
                                        m_RegPackItems(Index).Qty = _Convert.Round_Double2Decimals((outerVol - wrapVol + fillVol) / 1728, 1)

                                    Else
                                        m_RegPackItems(Index).Qty = _Convert.Round_Double2Decimals((outerVol - wrapVol + fillVol) / 1728, 1)

                                    End If
                                End If ''ol#1.2.27(12/10).

                            End If ''ol#9.123(1/23).
                            If Not 0 <= m_RegPackItems(Index).Qty Then
                                Exit Function ''ol#9.83(3/15)... Stop calculate 'Fill' if the value is less then zero.
                            Else
                                lblQtyFill.Content = (CStr(m_RegPackItems(Index).Qty) & " cu.ft.") ''ol#9.53(8/30)... Labels with value measures were added to Workshop items to give better visual.
                                If m_RegPackItems(Index).IsDisplayed Then
                                    qtyMeasure = "CubicFeet"
                                    If 0 < wrapLWH Then
                                        Call tvNode_AddChild(Index, "Wrapped Volume", "WrapL(" & CStr(wrapL) & ") * WrapW(" & CStr(wrapW) & ") * WrapH(" & CStr(wrapH) & ") ***QTY=1 is " & CStr(wrapHight) & "''", wrapVol, parentX, childX)
                                        Call tvNode_AddChild(Index, "Outer  Volume", "OuterL(" & CStr(outerL) & ") * OuterW(" & CStr(outerW) & ") * OuterH(" & CStr(outerH) & ")", outerVol, parentX, childX)
                                        ''ol#9.55(9/21)... [Fill Volume] node item added to the detailed calculation Reviewer.
                                        Call tvNode_AddChild(Index, "Fill  Volume", "(OuterVolume(" & CStr(outerVol) & ") - WrappedVolume(" & CStr(wrapVol) & "))", outerVol - wrapVol, parentX, childX)
                                        Call tvNode_AddChild(Index, "Cubic  Feet", "FillVolume(" & CStr(outerVol - wrapVol) & ") / 1728", m_RegPackItems(Index).Qty, parentX, childX) ''ol#9.55(9/21).
                                    Else
                                        Call tvNode_AddChild(Index, "Object Volume", "itemL(" & CStr(itemL) & ") * itemW(" & CStr(itemW) & ") * itemH(" & CStr(itemH) & ")", itemVol, parentX, childX)
                                        Call tvNode_AddChild(Index, "Outer  Volume", "OuterL(" & CStr(outerL) & ") * OuterW(" & CStr(outerW) & ") * OuterH(" & CStr(outerH) & ")", outerVol, parentX, childX)
                                        ''ol#9.55(9/21)... [Fill Volume] node item added to the detailed calculation Reviewer.
                                        Call tvNode_AddChild(Index, "Fill  Volume", "(OuterVolume(" & CStr(outerVol) & ") - itemVolume(" & CStr(itemVol) & "))", outerVol - itemVol, parentX, childX)
                                        Call tvNode_AddChild(Index, "Cubic  Feet", "FillVolume(" & CStr(outerVol - itemVol) & ") / 1728", m_RegPackItems(Index).Qty, parentX, childX) ''ol#9.55(9/21).
                                    End If
                                End If ''ol#9.51(8/27).
                            End If
                        Case "Wrap"
                            ''ol#9.123(1/23)... Reviewer 'Qty' column values are allowed to be a adjusted resulting in recalculation of 'Ext Price'.
                            ''                      ''mm#9.83(3/15)... 'Wrap' calculation formula has been adjusted from (4*(L x W))+(2*(W x H))/144 to ((2*(L x W))+(2*(W x H))+(2*(L x H)))/144
                            ''                      ''mm#9.83(3/15)m_RegPackItems(Index).Qty = _Convert.Round_Double2Decimals((((4 * (itemL * itemW)) + (2 * (itemW * itemH))) / 144) * m_RegPackItems(Index).SlidePosition, 1)
                            ''                      m_RegPackItems(Index).Qty = _Convert.Round_Double2Decimals((((2 * (itemL * itemW)) + (2 * (itemW * itemH)) + (2 * (itemL * itemH))) / 144) * m_RegPackItems(Index).SlidePosition, 1) ''mm#9.83(3/15).
                            If 0 < m_RegPackItems(Index).Qty_SetbyUser Then
                                m_RegPackItems(Index).Qty = m_RegPackItems(Index).Qty_SetbyUser
                            Else
                                m_RegPackItems(Index).Qty = _Convert.Round_Double2Decimals((((2 * (itemL * itemW)) + (2 * (itemW * itemH)) + (2 * (itemL * itemH))) / 144) * m_RegPackItems(Index).SlidePosition, 1) ''mm#9.83(3/15).
                            End If ''ol#9.123(1/23).
                            lblQtyWrap.Content = (CStr(m_RegPackItems(Index).Qty) & " sq.ft.") ''ol#9.53(8/30)... Labels with value measures were added to Workshop items to give better visual.
                            If m_RegPackItems(Index).IsDisplayed Then
                                qtyMeasure = "SquareFeet"
                                ''mm#9.83(3/15)... 'Wrap' calculation formula has been adjusted from (4*(L x W))+(2*(W x H))/144 to ((2*(L x W))+(2*(W x H))+(2*(L x H)))/144
                                ''mm#9.83(3/15)Call tvNode_AddChild(Index, "Square Feet", "(((4 * (itemL(" & CStr(itemL) & ") * itemW(" & CStr(itemW) & "))) + (2 * (itemW(" & CStr(itemW) & ") * itemH(" & CStr(itemH) & ")))) / 144) * QTY(" & CStr(m_RegPackItems(Index).SlidePosition) & ")", m_RegPackItems(Index).Qty, parentX, childX)
                                Call tvNode_AddChild(Index, "Square Feet", "(((2 * (itemL(" & CStr(itemL) & ") * itemW(" & CStr(itemW) & "))) + (2 * (itemW(" & CStr(itemW) & ") * itemH(" & CStr(itemH) & "))) + (2 * (itemL(" & CStr(itemL) & ") * itemH(" & CStr(itemH) & ")))) / 144) * QTY(" & CStr(m_RegPackItems(Index).SlidePosition) & ")", m_RegPackItems(Index).Qty, parentX, childX) ''mm#9.83(3/15).
                            End If
                        Case Else ' Outer/Inner Box SKU
                            ''ol#9.123(1/23)... Reviewer 'Qty' column values are allowed to be a adjusted resulting in recalculation of 'Ext Price'.
                            ''              m_RegPackItems(Index).Qty = m_RegPackItems(Index).SlidePosition
                            If 0 < m_RegPackItems(Index).Qty_SetbyUser Then
                                m_RegPackItems(Index).Qty = m_RegPackItems(Index).Qty_SetbyUser
                            Else
                                m_RegPackItems(Index).Qty = m_RegPackItems(Index).SlidePosition
                            End If ''ol#9.123(1/23).
                            If Index = iINNER Then
                                If chkPackDoubleBox.IsChecked Then
                                    lblRegVolume1.Content = (_Convert.Round_Double2Decimals(innerVol / 1728, 1) & " cu.ft.")
                                Else
                                    lblRegVolume1.Content = (_Convert.Round_Double2Decimals(outerVol / 1728, 1) & " cu.ft.")

                                End If
                            ElseIf Index = iOUTER Then
                                lblRegVolume0.Content = (_Convert.Round_Double2Decimals(outerVol / 1728, 1) & " cu.ft.")
                            End If
                    End Select
                    '
                    cstFormula = "UnitPrice(" & m_RegPackItems(Index).UnitPrice & ") * " & qtyMeasure & "(" & m_RegPackItems(Index).Qty & ")"
                    m_RegPackItems(Index).ExtCost = _Convert.Round_Double2Decimals(m_RegPackItems(Index).UnitCost * m_RegPackItems(Index).Qty, 2)
                    m_RegPackItems(Index).ExtPrice = _Convert.Round_Double2Decimals(m_RegPackItems(Index).UnitPrice * m_RegPackItems(Index).Qty, 2)
                    If "Difficulty" = iMatClass Then
                        m_RegPackItems(Index).Weight = 0
                    Else
                        whtFormula = "Weight(" & CStr(iWeight) & ") * " & qtyMeasure & "(" & m_RegPackItems(Index).Qty & ")"
                        m_RegPackItems(Index).Weight = _Convert.Round_Double2Decimals(iWeight * m_RegPackItems(Index).Qty, 1)
                        If m_RegPackItems(Index).IsDisplayed Then
                            Call tvNode_AddChild(Index, "Pack Weight", whtFormula, m_RegPackItems(Index).Weight, parentX, childX)
                        End If
                    End If
                    '
                    If m_RegPackItems(Index).IsDisplayed Then
                        Call tvNode_AddChild(Index, "Ext Price", cstFormula, m_RegPackItems(Index).ExtPrice, parentX, childX)
                    End If
                    m_RegPackItems(Index).IsDisplayed = tvNode_EditParent(Index, parentX)
                    '
                End If
            End If
            '
        Else
            '
            Call clear_m_RegPackItem(Index)
            Call tvNode_RemoveParent(Index)
            '
        End If
        ''
    End Function

#Region "Find"

    Private Function find_Price_n_Weight(ByVal itemSKU As String, ByRef retCost As Double, ByRef retSell As Double, ByRef retWeight As Double, ByRef retDesc As String, ByRef retMatClass As String, ByRef retDept As String) As Boolean
        Dim drows() As DataRow = Nothing
        retCost = 0 '' assume.
        retSell = 0 '' assume.
        retWeight = 0 '' assume.
        retDesc = "" '' assume.
        retMatClass = "" '' assume.
        retDept = "" '' assume.
        If Not 0 = Len(itemSKU) Then
            If _DataSet.Filter_DataTable(dtlPackMaterials, "SKU='" & itemSKU & "'", drows) Then
                retCost = _Convert.Null2DefaultValue(drows(0)("Cost"), 0)
                retSell = _Convert.Null2DefaultValue(drows(0)("Sell"), 0)
                retWeight = _Convert.Round_Double2Decimals(_Convert.Null2DefaultValue(drows(0)("Weight"), 0), 1)
                retDesc = _Convert.Null2DefaultValue(drows(0)("Desc"), "")
                retMatClass = _Convert.Null2DefaultValue(drows(0)("MaterialsClass"), "Boxes")
                retDept = _Convert.Null2DefaultValue(drows(0)("Department"), "") ''ol#9.57(10/6)... Department of a packing item should be checked against tax requirements.
                find_Price_n_Weight = True
            End If
        End If
        drows = Nothing
    End Function

    Private Function find_LWH_bySKU(ByVal Index As Integer, ByVal itemSKU As String, ByRef boxLL As Integer, ByRef boxWW As Integer, ByRef boxHH As Integer) As Boolean
        Dim drows() As DataRow = Nothing
        Dim retLL As Integer = 0
        Dim retWW As Integer = 0
        Dim retHH As Integer = 0
        boxLL = 0 '' assume.
        boxWW = 0 '' assume.
        boxHH = 0 '' assume.
        If Not 0 = Len(itemSKU) Then
            If _DataSet.Filter_DataTable(dtlPackMaterials, "SKU='" & itemSKU & "'", drows) Then
                retLL = _Convert.Null2DefaultValue(drows(0)("L"), 0)
                retWW = _Convert.Null2DefaultValue(drows(0)("W"), 0)
                retHH = _Convert.Null2DefaultValue(drows(0)("H"), 0)
                boxLL = retLL
                boxWW = retWW
                boxHH = retHH
                If Index = iINNER Then
                    If Val(Me.txtObjL.Text) < retLL AndAlso Val(Me.txtObjW.Text) < retWW AndAlso Val(Me.txtObjH.Text) < retHH Then
                        ' found
                    ElseIf Val(Me.txtObjL.Text) < retLL AndAlso Val(Me.txtObjW.Text) < retHH AndAlso Val(Me.txtObjH.Text) < retWW Then
                        ' found
                        boxWW = retHH
                        boxHH = retWW
                    ElseIf Val(Me.txtObjL.Text) < retWW AndAlso Val(Me.txtObjW.Text) < retHH AndAlso Val(Me.txtObjH.Text) < retLL Then
                        ' found
                        boxLL = retWW
                        boxWW = retHH
                        boxHH = retLL
                    ElseIf Val(Me.txtObjL.Text) < retWW AndAlso Val(Me.txtObjW.Text) < retLL AndAlso Val(Me.txtObjH.Text) < retHH Then
                        ' found
                        boxLL = retWW
                        boxWW = retLL
                    ElseIf Val(Me.txtObjL.Text) < retHH AndAlso Val(Me.txtObjW.Text) < retLL AndAlso Val(Me.txtObjH.Text) < retWW Then
                        ' found
                        boxLL = retHH
                        boxWW = retLL
                        boxHH = retWW
                    ElseIf Val(Me.txtObjL.Text) < retHH AndAlso Val(Me.txtObjW.Text) < retWW AndAlso Val(Me.txtObjH.Text) < retLL Then
                        ' found
                        boxLL = retHH
                        boxWW = retWW
                        boxHH = retLL
                    End If
                ElseIf Index = iOUTER Then
                    If Val(Me.txtPackInnerL.Text) < retLL AndAlso Val(Me.txtPackInnerW.Text) < retWW AndAlso Val(Me.txtPackInnerH.Text) < retHH Then
                        ' found
                    ElseIf Val(Me.txtObjL.Text) < retLL AndAlso Val(Me.txtPackInnerW.Text) < retHH AndAlso Val(Me.txtPackInnerH.Text) < retWW Then
                        ' found
                        boxWW = retHH
                        boxHH = retWW
                    ElseIf Val(Me.txtPackInnerL.Text) < retWW AndAlso Val(Me.txtPackInnerW.Text) < retHH AndAlso Val(Me.txtPackInnerH.Text) < retLL Then
                        ' found
                        boxLL = retWW
                        boxWW = retHH
                        boxHH = retLL
                    ElseIf Val(Me.txtPackInnerL.Text) < retWW AndAlso Val(Me.txtPackInnerW.Text) < retLL AndAlso Val(Me.txtPackInnerH.Text) < retHH Then
                        ' found
                        boxLL = retWW
                        boxWW = retLL
                    ElseIf Val(Me.txtPackInnerL.Text) < retHH AndAlso Val(Me.txtPackInnerW.Text) < retLL AndAlso Val(Me.txtPackInnerH.Text) < retWW Then
                        ' found
                        boxLL = retHH
                        boxWW = retLL
                        boxHH = retWW
                    ElseIf Val(Me.txtPackInnerL.Text) < retHH AndAlso Val(Me.txtPackInnerW.Text) < retWW AndAlso Val(Me.txtPackInnerH.Text) < retLL Then
                        ' found
                        boxLL = retHH
                        boxWW = retWW
                        boxHH = retLL
                    End If
                End If
                find_LWH_bySKU = True
            End If
        End If
        drows = Nothing
    End Function

    Private Function find_cmbRegDesc_byINDEX(ByVal iINDEX As Integer, ByRef ctrl As ComboBox) As Boolean
        find_cmbRegDesc_byINDEX = True ' assume.
        Select Case iINDEX
            Case PackMasterII.iINNER : ctrl = Me.cmbRegDesc1
            Case PackMasterII.iOUTER : ctrl = Me.cmbRegDesc0
            Case PackMasterII.iWRAP : ctrl = Me.cmbRegDesc2
            Case PackMasterII.i_FILL : ctrl = Me.cmbRegDesc3
            Case PackMasterII.iLABOR : ctrl = Me.cmbRegDesc4
            Case PackMasterII.iOTHER : ctrl = Me.cmbRegDesc5
            Case Else : find_cmbRegDesc_byINDEX = False
        End Select
    End Function

    Private Function find_txtRegQty_byINDEX(ByVal iINDEX As Integer, ByRef ctrl As TextBox) As Boolean
        find_txtRegQty_byINDEX = True ' assume.
        Select Case iINDEX
            Case PackMasterII.iINNER : ctrl = Me.txtRegQty1
            Case PackMasterII.iOUTER : ctrl = Me.txtRegQty0
            Case PackMasterII.iWRAP : ctrl = Me.txtRegQty2
            Case PackMasterII.i_FILL : ctrl = Me.txtRegQty3
            Case PackMasterII.iLABOR : ctrl = Me.txtRegQty4
            Case PackMasterII.iOTHER : ctrl = Me.txtRegQty5
            Case Else : find_txtRegQty_byINDEX = False
        End Select
    End Function

    Private Function find_cmdRegQtyPlus_byINDEX(ByVal iINDEX As Integer, ByRef ctrl As Button) As Boolean
        find_cmdRegQtyPlus_byINDEX = True ' assume.
        Select Case iINDEX
            Case PackMasterII.iINNER : ctrl = Me.cmdRegQtyPlus1
            Case PackMasterII.iOUTER : ctrl = Me.cmdRegQtyPlus0
            Case PackMasterII.iWRAP : ctrl = Me.cmdRegQtyPlus2
            Case PackMasterII.i_FILL : ctrl = Me.cmdRegQtyPlus3
            Case PackMasterII.iLABOR : ctrl = Me.cmdRegQtyPlus4
            Case PackMasterII.iOTHER : ctrl = Me.cmdRegQtyPlus5
            Case Else : find_cmdRegQtyPlus_byINDEX = False
        End Select
    End Function

    Private Function find_cmdRegQtyMinus_byINDEX(ByVal iINDEX As Integer, ByRef ctrl As Button) As Boolean
        find_cmdRegQtyMinus_byINDEX = True ' assume.
        Select Case iINDEX
            Case PackMasterII.iINNER : ctrl = Me.cmdRegQtyMinus1
            Case PackMasterII.iOUTER : ctrl = Me.cmdRegQtyMinus0
            Case PackMasterII.iWRAP : ctrl = Me.cmdRegQtyMinus2
            Case PackMasterII.i_FILL : ctrl = Me.cmdRegQtyMinus3
            Case PackMasterII.iLABOR : ctrl = Me.cmdRegQtyMinus4
            Case PackMasterII.iOTHER : ctrl = Me.cmdRegQtyMinus5
            Case Else : find_cmdRegQtyMinus_byINDEX = False
        End Select
    End Function
    Private Function find_lblReg_byINDEX(ByVal iINDEX As Integer, ByRef ctrl As Label) As Boolean
        find_lblReg_byINDEX = True ' assume.
        Select Case iINDEX
            Case PackMasterII.iINNER : ctrl = Me.lblReg1
            Case PackMasterII.iOUTER : ctrl = Me.lblReg0
            Case PackMasterII.iWRAP : ctrl = Me.lblReg2
            Case PackMasterII.i_FILL : ctrl = Me.lblReg3
            Case PackMasterII.iLABOR : ctrl = Me.lblReg4
            Case PackMasterII.iOTHER : ctrl = Me.lblReg5
            Case Else : find_lblReg_byINDEX = False
        End Select
    End Function
    Private Function find_lblRegVolume_byINDEX(ByVal iINDEX As Integer, ByRef ctrl As Label) As Boolean
        find_lblRegVolume_byINDEX = True ' assume.
        Select Case iINDEX
            Case PackMasterII.iINNER : ctrl = Me.lblRegVolume1
            Case PackMasterII.iOUTER : ctrl = Me.lblRegVolume0
            Case Else : find_lblRegVolume_byINDEX = False
        End Select
    End Function
    Private Function find_cmdComboDropDown_byINDEX(ByVal iINDEX As Integer, ByRef ctrl As Button) As Boolean
        find_cmdComboDropDown_byINDEX = True ' assume.
        Select Case iINDEX
            Case PackMasterII.iINNER : ctrl = Me.cmdComboDropDown1
            Case PackMasterII.iOUTER : ctrl = Me.cmdComboDropDown0
            Case PackMasterII.iWRAP : ctrl = Me.cmdComboDropDown2
            Case PackMasterII.i_FILL : ctrl = Me.cmdComboDropDown3
            Case PackMasterII.iLABOR : ctrl = Me.cmdComboDropDown4
            Case PackMasterII.iOTHER : ctrl = Me.cmdComboDropDown5
            Case Else : find_cmdComboDropDown_byINDEX = False
        End Select


    End Function

    Private Sub change_DoubleBoxStatus()
        Dim i1 As Integer
        Dim i2 As Integer
        If chkPackDoubleBox.IsChecked Then
            lblPackOuterBox.Content = "Outer Box: L x W x H"
            lblReg1.Content = "Inner Box SKU"
            lblPackInnerBox.Content = "Inner Box: L x W x H"
            i1 = iOUTER
            i2 = iINNER
        Else
            lblPackOuterBox.Content = "Single Box: L x W x H"
            lblReg1.Content = "Single Box SKU"
            lblPackInnerBox.Content = ""
            i1 = iINNER
            i2 = iOUTER

        End If
        ''
        'Call change_DoubleBoxStatus_inWorkShop(i1, i2)
        ''
        txtPackInnerL.IsEnabled = chkPackDoubleBox.IsChecked
        txtPackInnerW.IsEnabled = chkPackDoubleBox.IsChecked
        txtPackInnerH.IsEnabled = chkPackDoubleBox.IsChecked
        If chkPackDoubleBox.IsChecked Then
            lblReg0.Visibility = Visibility.Visible
        Else
            lblReg0.Visibility = Visibility.Hidden

        End If

        cmbRegDesc0.Visibility = lblReg0.Visibility
        cmdRegQtyPlus0.Visibility = lblReg0.Visibility
        cmdRegQtyMinus0.Visibility = lblReg0.Visibility
        lblRegVolume0.Visibility = lblReg0.Visibility
        txtRegQty0.Visibility = lblReg0.Visibility
        cmdComboDropDown0.Visibility = lblReg0.Visibility


    End Sub
    Private Sub change_DoubleBoxStatus_inWorkShop(ByVal Index1 As Integer, ByVal Index2 As Integer)
        Dim cmbRegDesc As ComboBox = Nothing : find_cmbRegDesc_byINDEX(Index1, cmbRegDesc)
        Dim txtRegQty As TextBox = Nothing : find_txtRegQty_byINDEX(Index1, txtRegQty)
        Dim cmdRegQtyPlus As Button = Nothing : find_cmdRegQtyPlus_byINDEX(Index1, cmdRegQtyPlus)
        Dim cmdRegQtyMinus As Button = Nothing : find_cmdRegQtyMinus_byINDEX(Index1, cmdRegQtyMinus)
        Dim lblReg As Label = Nothing : find_lblReg_byINDEX(Index1, lblReg)
        Dim lblRegVolume As Label = Nothing : find_lblRegVolume_byINDEX(Index1, lblRegVolume)
        Dim cmdComboDropDown As Button = Nothing : find_cmdComboDropDown_byINDEX(Index1, cmdComboDropDown)
        ' jumps up:
        lblReg.Margin = New Thickness(17, 58, 0, 0)
        cmbRegDesc.Margin = New Thickness(43, 58, 0, 0)
        cmdRegQtyPlus.Margin = New Thickness(276, 58, 0, 0)
        cmdRegQtyMinus.Margin = New Thickness(309, 58, 0, 0)
        lblRegVolume.Margin = New Thickness(198, 58, 0, 0)
        cmdRegQtyMinus.Margin = New Thickness(309, 58, 0, 0)
        txtRegQty.Margin = New Thickness(361, 58, 0, 0)
        cmdComboDropDown.Margin = New Thickness(175, 58, 0, 0)



        ''
        find_cmbRegDesc_byINDEX(Index2, cmbRegDesc)
        find_txtRegQty_byINDEX(Index2, txtRegQty)
        find_cmdRegQtyPlus_byINDEX(Index2, cmdRegQtyPlus)
        find_cmdRegQtyMinus_byINDEX(Index2, cmdRegQtyMinus)
        find_lblReg_byINDEX(Index2, lblReg)
        find_lblRegVolume_byINDEX(Index2, lblRegVolume)
        find_cmdComboDropDown_byINDEX(Index2, cmdComboDropDown)
        ' jumps down:

        lblReg.Margin = New Thickness(17, 84, 0, 0)
        cmbRegDesc.Margin = New Thickness(43, 84, 0, 0)
        cmdRegQtyPlus.Margin = New Thickness(276, 84, 0, 0)
        cmdRegQtyMinus.Margin = New Thickness(309, 84, 0, 0)
        lblRegVolume.Margin = New Thickness(198, 84, 0, 0)
        cmdRegQtyMinus.Margin = New Thickness(309, 84, 0, 0)
        txtRegQty.Margin = New Thickness(361, 84, 0, 0)
        cmdComboDropDown.Margin = New Thickness(175, 84, 0, 0)


    End Sub
#End Region

#Region "REVIEWER"


    Private Sub chkRevReceiptView_Checked(sender As Object, e As RoutedEventArgs) Handles chkRevReceiptView.Checked
        TV.Visibility = Visibility.Collapsed
        Reviewer_ListView.Visibility = Visibility.Visible
    End Sub

    Private Sub chkRevReceiptView_Unchecked(sender As Object, e As RoutedEventArgs) Handles chkRevReceiptView.Unchecked
        TV.Visibility = Visibility.Visible
        Reviewer_ListView.Visibility = Visibility.Collapsed
    End Sub
    Private Sub chkRevReceiptView_Click(sender As Object, e As System.EventArgs) Handles chkRevReceiptView.Click
        Try
            My.Settings.Enable_ReceiptView = chkRevReceiptView.IsChecked
            My.Settings.Save()
        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to switch between Reviewers...")
        End Try
    End Sub

#Region "TreeView"

    Private Function tvNode_EditParent(ByVal Index As Short, ByRef parentX As TreeViewItem) As Boolean
        If isExist_TVnode(Index, parentX) Then
            parentX.Header = $"{Format(m_RegPackItems(Index).Weight, "0.00 lb")} | {Format(m_RegPackItems(Index).ExtPrice, "$ 0.00")} - [{m_RegPackItems(Index).SKU}] - {m_RegPackItems(Index).Desc}"
        End If
        Return True
    End Function

    Private Function tvNode_AddChild(ByVal Index As Short, ByVal whatCalc As String, ByVal sFormula As String, ByVal nValue As Double, ByVal parentX As TreeViewItem, ByRef childX As TreeViewItem) As Boolean
        If isExist_TVnode(Index, parentX) Then
            Dim newNode As New TreeViewItem With {
                .Header = $"[{whatCalc}] {CStr(nValue)} = {Replace(sFormula, "item", "Object")}"
            }
            parentX.Items.Add(newNode)
            childX = newNode
        End If
        Return True
    End Function

    Private Function tvNode_AddParent(ByVal Index As Short, ByRef parentX As TreeViewItem) As Boolean
        If tvNode_RemoveParent(Index) Then
            parentX = New TreeViewItem With {
                .Header = $"{Format(m_RegPackItems(Index).Weight, "0.00 lb")} | {Format(m_RegPackItems(Index).ExtPrice, "$ 0.00")} - [{m_RegPackItems(Index).SKU}] - {m_RegPackItems(Index).Desc}",
                .Name = "key" & CStr(Index)
            }
            TV.Items.Add(parentX)
        End If
        Return True
    End Function

    Private Function tvNode_RemoveParent(ByVal Index As Short) As Boolean
        Dim parentX As TreeViewItem = Nothing
        If isExist_TVnode(Index, parentX) Then
            TV.Items.Remove(parentX)
        End If
        Return True
    End Function

    Private Function isExist_TVnode(ByVal Index As Short, ByRef nodeX As TreeViewItem) As Boolean
        For Each item As Object In TV.Items
            Dim currentItem As TreeViewItem = TryCast(item, TreeViewItem)
            If currentItem IsNot Nothing AndAlso currentItem.Name = "key" & CStr(Index) Then
                nodeX = currentItem
                Return True
            End If
        Next
        Return False
    End Function

#End Region

#End Region

#Region "Buttons"
    Private Sub cmdRegQtyMinus_Click(sender As System.Object, e As System.EventArgs) Handles cmdRegQtyMinus0.Click, cmdRegQtyMinus1.Click, cmdRegQtyMinus2.Click, cmdRegQtyMinus3.Click, cmdRegQtyMinus4.Click, cmdRegQtyMinus5.Click
        Try
            Dim cmdRegQtyMinus As Button = CType(sender, Button)
            Dim Index As Integer = Val(_Controls.Right(cmdRegQtyMinus.Name, 1))
            Call cmdRegQtyMinus_Click(Index)
            DisplayInvoiceDetail(global_fragility_index)
        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to minus the Quantity...")
        End Try
    End Sub
    Private Sub cmdRegQtyMinus_Click(ByVal Index As Integer)
        Dim cmbRegDesc As ComboBox = Nothing : find_cmbRegDesc_byINDEX(Index, cmbRegDesc)
        Dim txtRegQty As TextBox = Nothing : find_txtRegQty_byINDEX(Index, txtRegQty)
        Dim cmdRegQtyPlus As Button = Nothing : find_cmdRegQtyPlus_byINDEX(Index, cmdRegQtyPlus)
        Dim cmdRegQtyMinus As Button = Nothing : find_cmdRegQtyMinus_byINDEX(Index, cmdRegQtyMinus)
        ''
        txtRegQty.IsEnabled = (Not 0 = Len(cmbRegDesc.Text))
        cmdRegQtyPlus.IsEnabled = txtRegQty.IsEnabled
        cmdRegQtyMinus.IsEnabled = txtRegQty.IsEnabled
        m_RegPackItems(Index).Qty_SetbyUser = 0 ''ol#9.123(1/23)... Reviewer 'Qty' column values are allowed to be a adjusted resulting in recalculation of 'Ext Price'.
        If cmdRegQtyMinus.IsEnabled Then
            If Not 0 >= Val(txtRegQty.Text) Then
                txtRegQty.Text = CStr(Val(txtRegQty.Text) - 1)
            End If
            If Index = iLABOR Then ' labor
                'Me.cmdRegQtyMinus4.IsEnabled = (get_FragileLaborUnits() < Val(Me.txtRegQty4.Text))
                m_Labor_RegularTab = m_Labor_RegularTab - 1
            ElseIf Index = iWRAP Then
                'Me.txtRegMinFill.Text = Val(Me.txtRegMinFill.Text) - 0.5 ''ol#1.2.25(11/24)... Increasing Wrap sq.ft. over the selected box dimensions should jump to the next box size.
                'Call change_Price_n_Weight(iWRAP) ''ol#1.1.76(10/8).
            ElseIf Index = i_FILL Then
                ''ol#1.2.26(12/1)... 'Auto Fill SKU' check box added to enable/disable change the box size based on user adjusted Fill volume.
                If Me.chkAutoFill.IsChecked Then
                    Me.txtRegMinFill.Text = Val(Me.txtRegMinFill.Text) - 1 ''ol#1.2.25(11/24)... Increasing Fill cub.ft. over the selected box dimensions should jump to the next box size.
                End If
                'Call change_Price_n_Weight(iFILL) ''ol#1.1.76(10/8)... 'Fill SKU' have +/- buttons and will be unlimited.
            ElseIf Index = iINNER And 0 = Val(Me.txtRegQty0.Text) Then  ' inner box
                chkPackDoubleBox.IsChecked = False
            End If
            If 0 <= Val(txtRegQty.Text) Then
                Call change_Price_n_Weight(Index)
            End If
        End If
        ''
    End Sub
    Private Sub cmdRegQtyPlus_Click(sender As System.Object, e As System.EventArgs) Handles cmdRegQtyPlus0.Click, cmdRegQtyPlus1.Click, cmdRegQtyPlus2.Click, cmdRegQtyPlus3.Click, cmdRegQtyPlus4.Click, cmdRegQtyPlus5.Click
        Try
            Dim cmdRegQtyPlus As Button = CType(sender, Button)
            Dim Index As Integer = Val(_Controls.Right(cmdRegQtyPlus.Name, 1))
            Call cmdRegQtyPlus_Click(Index)
            DisplayInvoiceDetail(global_fragility_index)
        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to plus the Quantity...")
        End Try
    End Sub
    Private Sub cmdRegQtyPlus_Click(ByVal Index As Integer)
        Dim cmbRegDesc As ComboBox = Nothing : find_cmbRegDesc_byINDEX(Index, cmbRegDesc)
        Dim txtRegQty As TextBox = Nothing : find_txtRegQty_byINDEX(Index, txtRegQty)
        Dim cmdRegQtyPlus As Button = Nothing : find_cmdRegQtyPlus_byINDEX(Index, cmdRegQtyPlus)
        Dim cmdRegQtyMinus As Button = Nothing : find_cmdRegQtyMinus_byINDEX(Index, cmdRegQtyMinus)
        ''
        txtRegQty.IsEnabled = (Not 0 = Len(cmbRegDesc.Text))
        cmdRegQtyPlus.IsEnabled = txtRegQty.IsEnabled
        cmdRegQtyMinus.IsEnabled = txtRegQty.IsEnabled
        m_RegPackItems(Index).Qty_SetbyUser = 0 ''ol#9.123(1/23)... Reviewer 'Qty' column values are allowed to be a adjusted resulting in recalculation of 'Ext Price'.
        If cmdRegQtyPlus.IsEnabled Then
            txtRegQty.Text = CStr(Val(txtRegQty.Text) + 1)
            'Call change_Price_n_Weight(Index)
            If Index = iLABOR Then ' labor
                'Me.cmdRegQtyMinus4.IsEnabled = (get_FragileLaborUnits() < Val(Me.txtRegQty4.Text))
                m_Labor_RegularTab = m_Labor_RegularTab + 1
            ElseIf Index = iWRAP Then
                'Me.txtRegMinFill.Text = Val(Me.txtRegMinFill.Text) + 0.5 ''ol#1.2.25(11/24)... Increasing Wrap sq.ft. over the selected box dimensions should jump to the next box size.
                'Call change_Price_n_Weight(iWRAP) ''ol#1.1.76(10/8).
            ElseIf Index = i_FILL Then
                ''ol#1.2.26(12/1)... 'Auto Fill SKU' check box added to enable/disable change the box size based on user adjusted Fill volume.
                If Me.chkAutoFill.IsChecked Then
                    Me.txtRegMinFill.Text = Val(Me.txtRegMinFill.Text) + 1 ''ol#1.2.25(11/24)... Increasing Fill cub.ft. over the selected box dimensions should jump to the next box size.
                End If
                'Call change_Price_n_Weight(iFILL) ''ol#1.1.76(10/8)... 'Fill SKU' have +/- buttons and will be unlimited.
            End If
            Call change_Price_n_Weight(Index)
        End If
    End Sub

    Private Sub object_SetPieceNoChange()
        Me.txtRegPiecesNo.Tag = Me.txtRegPiecesNo.Text
    End Sub


#End Region


#Region "Clear"


    Private Sub clear_m_RegPackItem(ByVal Index As Short, Optional clear_Combos As Boolean = True)
        m_RegPackItems(Index).IsDisplayed = False ''ol#9.55(9/23)... We need an indicator for PackMasterII items to indicate if the item was added to the Preview list.
        m_RegPackItems(Index).SKU = ""
        m_RegPackItems(Index).Desc = ""
        m_RegPackItems(Index).Dept = ""
        m_RegPackItems(Index).Qty = 0
        m_RegPackItems(Index).SlidePosition = 0
        m_RegPackItems(Index).ExtCost = 0
        m_RegPackItems(Index).ExtPrice = 0
        m_RegPackItems(Index).Summary = False
        m_RegPackItems(Index).UnitCost = 0
        m_RegPackItems(Index).UnitPrice = 0
        m_RegPackItems(Index).Weight = 0
        If Index < 6 And clear_Combos Then
            Dim cmbRegDesc As ComboBox = Nothing
            If find_cmbRegDesc_byINDEX(Index, cmbRegDesc) Then
                cmbRegDesc.SelectedIndex = -1
            End If
            cmbRegDesc = Nothing
        End If
    End Sub

    Private Sub clear_Form(Optional ByRef isSkipContent As Boolean = False)
        Call clear_FormControls()
        Call clear_Reviewers()
        Call clear_m_RegPack_Qty_SetbyUser() ''ol#9.123(1/23)... Reviewer 'Qty' column values are allowed to be a adjusted resulting in recalculation of 'Ext Price'.
        'Call clear_ModifiedTab(True)
        Call clear_FragileChoices()


        'Call load_Combos
        Call load_SetupData()
        If Not isSkipContent Then
            Me.cmbContentsKeyword.Items.Clear()
            Me.cmbContentsKeyword.Text = String.Empty
            Call load_Contents()
        End If
        m_Labor_RegularTab = m_DefaultLabor
    End Sub

    Private Sub clear_FragileChoices()
        Me.lblPrice1.Content = "0.00"
        Me.lblPrice2.Content = "0.00"
        Me.lblPrice3.Content = "0.00"
        Me.lblPrice4.Content = "0.00"
        Me.lblPrice5.Content = "0.00"
        Me.lblWeightAct1.Content = "0 lbs (act)"
        Me.lblWeightAct2.Content = "0 lbs (act)"
        Me.lblWeightAct3.Content = "0 lbs (act)"
        Me.lblWeightAct4.Content = "0 lbs (act)"
        Me.lblWeightAct5.Content = "0 lbs (act)"

        Me.lblFragileBox1.Content = "0 x 0 x 0"
        Me.lblFragileBox2.Content = "0 x 0 x 0"
        Me.lblFragileBox3.Content = "0 x 0 x 0"
        Me.lblFragileBox4.Content = "0 x 0 x 0"
        Me.lblFragileBox5.Content = "0 x 0 x 0"
        Me.lblMaterialsPrice1.Content = "0.00"
        Me.lblMaterialsPrice2.Content = "0.00"
        Me.lblMaterialsPrice3.Content = "0.00"
        Me.lblMaterialsPrice4.Content = "0.00"
        Me.lblMaterialsPrice5.Content = "0.00"
        Me.lblLaborPrice1.Content = "0.00"
        Me.lblLaborPrice2.Content = "0.00"
        Me.lblLaborPrice3.Content = "0.00"
        Me.lblLaborPrice4.Content = "0.00"
        Me.lblLaborPrice5.Content = "0.00"
        Me.lblBoxPrice1.Content = "0.00"
        Me.lblBoxPrice2.Content = "0.00"
        Me.lblBoxPrice3.Content = "0.00"
        Me.lblBoxPrice4.Content = "0.00"
        Me.lblBoxPrice5.Content = "0.00"
    End Sub

    Private Sub clear_ModifiedTab(ByVal clearCheckedModifications As Boolean)
        Me.cmbModSelectBox.SelectedIndex = -1
        Me.txtModExtraLabor.Text = "0"
        Me.cmbModCardboard.SelectedIndex = -1
        Me.txtModHowMuch.Text = "0"
        Me.txtModNewBoxH.Text = "0"
        'Me.lblModSelectBox_SKUDesc.Content = "Box Description"
        Me.lblModSelectedBox_L.Content = "0"
        Me.lblModSelectedBox_W.Content = "0"
        Me.lblModSelectedBox_H.Content = "0"

        Me.lblModSelectedBox_SKU.Content = String.Empty

        If clearCheckedModifications Then
            For Each lwItem As ListViewItem In Me.lwMod.Items
                'lwItem.isChecked = False
            Next
        End If
    End Sub
    Private Sub clear_m_RegPack_Qty_SetbyUser()
        Dim i As Integer ''ol#9.123(1/23)... Reviewer 'Qty' column values are allowed to be a adjusted resulting in recalculation of 'Ext Price'.
        For i = 0 To UBound(m_RegPackItems)
            m_RegPackItems(i).Qty_SetbyUser = 0
        Next i
    End Sub
    Private Sub clear_Reviewers()
        For Index As Integer = 0 To 5
            Call clear_m_RegPackItem(Index)
            clear_ReviewItem()
            'Call change_ReviewerItem(Index)
        Next
        Me.chkPrintDetails.IsChecked = System.Windows.Forms.CheckState.Checked ''ol#9.58(10/15)... "Print Details on Receipt" check option is set to as checked by default.
        TV.Items.Clear()
    End Sub
    Private Sub clear_FormControls()
        Me.chkPackDoubleBox.IsChecked = False
        Me.chkFragile1.IsChecked = False
        Me.chkFragile2.IsChecked = False
        Me.chkFragile3.IsChecked = False
        Me.chkFragile4.IsChecked = False
        Me.chkFragile5.IsChecked = True
        '_Controls.Form_Clear_TextBoxes(Me.Content)
        Me.txtObjDesc.Text = String.Empty
    End Sub

#End Region


#Region "ReceiptView"

    Private Function clear_ReviewItem()
        Reviewer_ListView.ItemsSource = Nothing
    End Function

    Private Function change_ReviewerItem(ByVal Index As Integer) As Boolean
        Dim rowNo As Integer = 0 ' row
        ''Reviewer_ListView

    End Function



#End Region




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






End Class

Public Class FragilityObject
    Public Level As Integer
    Public Level_WeightAct As Double
    Public Level_WeightDim As Double
    Public Level_LplusG As Double
    Public Level_Cost As Double
    Public Level_Price As Double

    Public IsSelected As Boolean
    Public IsDoubleBox As Boolean

    Public ObjectL As Single
    Public ObjectW As Single
    Public ObjectH As Single

    Public BoxL As Integer
    Public BoxW As Integer
    Public BoxH As Integer
    Public BoxSKU As String
    Public BoxCost As Double
    Public BoxSell As Double
    Public BoxWeight As Double

    Public BoxL_Inner As Integer
    Public BoxW_Inner As Integer
    Public BoxH_Inner As Integer
    Public BoxSKU_Inner As String
    Public BoxCost_Inner As Double
    Public BoxSell_Inner As Double
    Public BoxWeight_Inner As Double

    Public FillUnit As Double
    Public FillSKU As String
    Public FillCost As Double
    Public FillSell As Double
    Public FillWeight As Double

    Public WrapUnit As Double
    Public WrapSKU As String
    Public WrapCost As Double
    Public WrapSell As Double
    Public WrapWeight As Double

    Public LaborUnit As Double
    Public LaborSKU As String
    Public LaborCost As Double
    Public LaborSell As Double
    Public LaborWeight As Double

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub
End Class
Public Class Modification
    Public ModificationName As String
    Public SelectedBoxSKU As String
    Public HowMuchTo As Single
    Public Labor As Single
    Public CartboardSKU As String
    Public IsChecked As Boolean

    Public Overrides Function ToString() As String
        Return ModificationName
    End Function

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub

End Class
