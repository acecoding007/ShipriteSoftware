Imports System.Windows.Forms
Imports System.Drawing

Public Module _DataGridView

    Public Const colType_Label As String = "Label"
    Public Const colType_Text As String = "Text"
    Public Const colType_TextReadOnly As String = "TextReadOnly"
    Public Const colType_DollarText As String = "DollarText"
    Public Const colType_PercentText As String = "PercentText"
    Public Const colType_CheckBox As String = "CheckBox"
    Public Const colType_ComboBox As String = "ComboBox"
    Public Const colType_Button As String = "Button"

    Public Function GetType_ButtonCell() As String
        GetType_ButtonCell = "System.Windows.Forms.DataGridViewButtonCell" ''= DGV.Item(e.ColumnIndex, e.RowIndex).GetType.ToString
    End Function

    Public Function SetUp_DataGridView(ByRef dgv As DataGridView) As Boolean
        ''
        SetUp_DataGridView = True '' assume.
        ''
        Try
            ' Virtual mode is turned on so that the
            ' unbound DataGridViewCheckBoxColumn will
            ' keep its state when the bound columns are
            ' sorted.
            dgv.VirtualMode = False
            dgv.AutoSize = False
            ''DataGridView1.DataSource = _
            ''Populate("SELECT * FROM Employees")
            ''DataGridView1.TopLeftHeaderCell.Value = "Employees"
            dgv.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders
            'dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells
            dgv.AllowUserToAddRows = False
            dgv.AllowUserToDeleteRows = False
        Catch ex As Exception : _Debug.Print_("_DataGridView.SetUp_DataGridView(): " & ex.Message) : SetUp_DataGridView = False
        End Try
        ''
    End Function

    Public Function Add_ButtonColumn(ByRef dgv As DataGridView, ByVal columnTitle As String) As Boolean
        ''
        Add_ButtonColumn = False
        Try
            Dim column As New DataGridViewButtonColumn()
            With column
                .HeaderText = columnTitle
                .Text = columnTitle
                ''.UseColumnTextForButtonValue = true
                .AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells
                .FlatStyle = FlatStyle.Standard
                .CellTemplate.Style.BackColor = Color.Honeydew
                .CellTemplate.Style.Font = New System.Drawing.Font("Arial Narrow", 12)
                .HeaderCell.Style.Font = New System.Drawing.Font("Arial Narrow", 10)
                ''.DisplayIndex = 0
            End With
            ''
            Dim beforeAddCount As Integer = dgv.ColumnCount
            dgv.Columns.Add(column)
            Add_ButtonColumn = (beforeAddCount < dgv.ColumnCount)
            ''
        Catch ex As Exception : _Debug.Print_("_DataGridView.Add_ButtonColumn(): " & ex.Message)
        End Try
        ''
    End Function
    Public Function Add_LinkColumn(ByRef dgv As DataGridView, ByVal columnTitle As String) As Boolean
        ''
        Add_LinkColumn = False
        Try
            Dim links As New DataGridViewLinkColumn()
            With links
                .HeaderText = columnTitle
                .DataPropertyName = columnTitle
                .ActiveLinkColor = Color.White
                .LinkBehavior = LinkBehavior.SystemDefault
                .LinkColor = Color.Blue
                .TrackVisitedState = True
                .VisitedLinkColor = Color.YellowGreen
            End With

            dgv.Columns.Add(links)
            Add_LinkColumn = True
        Catch ex As Exception : _Debug.Print_("_DataGridView.Add_LinkColumn(): " & ex.Message)
        End Try
        ''
    End Function
    Public Function Add_ComboBoxColumn(ByRef dgv As DataGridView, ByVal columnTitle As String) As Boolean
        ''
        Add_ComboBoxColumn = False
        Try
            Dim column As New DataGridViewComboBoxColumn()
            With column
                .AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                .DataPropertyName = columnTitle
                .HeaderText = columnTitle
                .Name = columnTitle
                '.DropDownWidth = 160
                '.Width = 90
                .MaxDropDownItems = 6
                .FlatStyle = FlatStyle.Flat
                .CellTemplate.Style.ForeColor = Color.Maroon
                .CellTemplate.Style.Font = New System.Drawing.Font("Arial Narrow", 12)
                .HeaderCell.Style.Font = New System.Drawing.Font("Arial Narrow", 10)
            End With
            ''
            Dim beforeAddCount As Integer = dgv.ColumnCount
            dgv.Columns.Add(column)
            Add_ComboBoxColumn = (beforeAddCount < dgv.ColumnCount)
            ''
        Catch ex As Exception : _Debug.Print_("_DataGridView.Add_ComboBoxColumn(): " & ex.Message)
        End Try
        ''
    End Function
    Public Function Add_CheckBoxColumn(ByRef dgv As DataGridView, ByVal columnTitle As String) As Boolean
        ''
        Add_CheckBoxColumn = False
        Try
            Dim column As New DataGridViewCheckBoxColumn()
            With column
                .HeaderText = columnTitle
                .Name = columnTitle
                .AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells
                .FlatStyle = FlatStyle.Standard
                .CellTemplate = New DataGridViewCheckBoxCell()
                .CellTemplate.Style.BackColor = Color.Beige
                .CellTemplate.Style.Font = New System.Drawing.Font("Arial Narrow", 12)
                .HeaderCell.Style.Font = New System.Drawing.Font("Arial Narrow", 10)
            End With
            ''
            Dim beforeAddCount As Integer = dgv.ColumnCount
            dgv.Columns.Add(column)
            Add_CheckBoxColumn = (beforeAddCount < dgv.ColumnCount)
            ''
        Catch ex As Exception : _Debug.Print_("_DataGridView.Add_CheckBoxColumn(): " & ex.Message)
        End Try
        ''
    End Function
    Public Function Add_LabelColumn(ByRef dgv As DataGridView, ByVal columnTitle As String) As Boolean
        ''
        Add_LabelColumn = False
        Try
            dgv.AllowUserToResizeColumns = True
            Dim column As New DataGridViewTextBoxColumn
            With column
                .HeaderText = columnTitle
                .Name = columnTitle
                .AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                .ReadOnly = True
                .CellTemplate = New DataGridViewTextBoxCell
                .CellTemplate.Style.BackColor = Color.Beige
                .CellTemplate.Style.Font = New System.Drawing.Font("Arial Narrow", 12)
                .HeaderCell.Style.Font = New System.Drawing.Font("Arial Narrow", 10)
            End With
            ''
            Dim beforeAddCount As Integer = dgv.ColumnCount
            dgv.Columns.Add(column)
            Add_LabelColumn = (beforeAddCount < dgv.ColumnCount)
            ''
        Catch ex As Exception : _Debug.Print_("_DataGridView.Add_LabelColumn(): " & ex.Message)
        End Try
        ''
    End Function
    Public Function Add_TextColumn(ByRef dgv As DataGridView, ByVal columnTitle As String) As Boolean
        ''
        Add_TextColumn = False
        Try
            Dim column As New DataGridViewTextBoxColumn()
            With column
                .HeaderText = columnTitle
                .Name = columnTitle
                .AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells
                .ReadOnly = False
                .CellTemplate = New DataGridViewTextBoxCell
                .CellTemplate.Style.Alignment = DataGridViewContentAlignment.MiddleLeft
                ''.CellTemplate.Style.BackColor = Color.Beige
                .CellTemplate.Style.ForeColor = Color.Maroon
                .CellTemplate.Style.Font = New System.Drawing.Font("Arial Narrow", 12)
                .HeaderCell.Style.Font = New System.Drawing.Font("Arial Narrow", 10)
            End With
            ''
            Dim beforeAddCount As Integer = dgv.ColumnCount
            dgv.Columns.Add(column)
            Add_TextColumn = (beforeAddCount < dgv.ColumnCount)
            ''
        Catch ex As Exception : _Debug.Print_("_DataGridView.Add_TextColumn(): " & ex.Message)
        End Try
        ''
    End Function
    Public Function Add_TextColumn_AsReadOnly(ByRef dgv As DataGridView, ByVal columnTitle As String) As Boolean
        ''
        Add_TextColumn_AsReadOnly = False
        Try
            Dim column As New DataGridViewTextBoxColumn
            With column
                .HeaderText = columnTitle
                .Name = columnTitle
                .AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                .ReadOnly = True
                .CellTemplate = New DataGridViewTextBoxCell
                .CellTemplate.Style.Alignment = DataGridViewContentAlignment.MiddleLeft
                ''.CellTemplate.Style.BackColor = Color.Beige
                .CellTemplate.Style.ForeColor = Color.Teal
                .CellTemplate.Style.Font = New System.Drawing.Font("Arial Narrow", 12)
                .HeaderCell.Style.Font = New System.Drawing.Font("Arial Narrow", 10)
            End With
            ''
            Dim beforeAddCount As Integer = dgv.ColumnCount
            dgv.Columns.Add(column)
            Add_TextColumn_AsReadOnly = (beforeAddCount < dgv.ColumnCount)
            ''
        Catch ex As Exception : _Debug.Print_("_DataGridView.Add_TextColumn_AsReadOnly(): " & ex.Message)
        End Try
        ''
    End Function
    Public Function Add_DollarTextColumn(ByRef dgv As DataGridView, ByVal columnTitle As String) As Boolean
        ''
        Add_DollarTextColumn = False
        Try
            Dim column As New DataGridViewTextBoxColumn
            With column
                .HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight
                .HeaderText = columnTitle
                .Name = columnTitle
                .AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells
                .ReadOnly = False
                .CellTemplate = New DataGridViewTextBoxCell
                .CellTemplate.Style.Alignment = DataGridViewContentAlignment.MiddleRight
                ''.CellTemplate.Style.BackColor = Color.Beige
                .CellTemplate.Style.ForeColor = Color.DarkGreen
                .CellTemplate.Style.Format = "$#,##0.00"
                .CellTemplate.Style.Font = New System.Drawing.Font("Arial Narrow", 12)
                .HeaderCell.Style.Font = New System.Drawing.Font("Arial Narrow", 10)
            End With
            ''
            Dim beforeAddCount As Integer = dgv.ColumnCount
            dgv.Columns.Add(column)
            Add_DollarTextColumn = (beforeAddCount < dgv.ColumnCount)
            ''
        Catch ex As Exception : _Debug.Print_("_DataGridView.Add_DollarTextColumn(): " & ex.Message)
        End Try
        ''
    End Function
    Public Function Add_PercentTextColumn(ByRef dgv As DataGridView, ByVal columnTitle As String) As Boolean
        ''
        Add_PercentTextColumn = False
        Try
            Dim column As New DataGridViewTextBoxColumn
            With column
                .HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter
                .HeaderText = columnTitle
                .Name = columnTitle
                .AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells
                .ReadOnly = False
                .CellTemplate = New DataGridViewTextBoxCell
                .CellTemplate.Style.Alignment = DataGridViewContentAlignment.MiddleCenter
                .CellTemplate.Style.BackColor = Color.Beige
                .CellTemplate.Style.ForeColor = Color.BlueViolet
                .CellTemplate.Style.Format = "0%"
                .CellTemplate.Style.Font = New System.Drawing.Font("Arial Narrow", 12)
                .HeaderCell.Style.Font = New System.Drawing.Font("Arial Narrow", 10)
            End With
            ''
            Dim beforeAddCount As Integer = dgv.ColumnCount
            dgv.Columns.Add(column)
            Add_PercentTextColumn = (beforeAddCount < dgv.ColumnCount)
            ''
        Catch ex As Exception : _Debug.Print_("_DataGridView.Add_DollarTextColumn(): " & ex.Message)
        End Try
        ''
    End Function
    Public Function Add_Columns(ByRef dgv As DataGridView, ByVal ParamArray columnTypeCommaTitle() As String) As Boolean
        ''
        ''  example: 3 columns to be added
        ''  "Label", "Setting Name", "DollarText", "Base Cost", "DollarText", "Sell Price"
        Add_Columns = False
        Try
            For i As Integer = columnTypeCommaTitle.GetLowerBound(0) To columnTypeCommaTitle.GetUpperBound(0)
                If Not (i + 1) > columnTypeCommaTitle.GetUpperBound(0) Then
                    Select Case columnTypeCommaTitle(i)
                        Case colType_Label : Add_Columns = _DataGridView.Add_LabelColumn(dgv, columnTypeCommaTitle(i + 1))
                        Case colType_Text : Add_Columns = _DataGridView.Add_TextColumn(dgv, columnTypeCommaTitle(i + 1))
                        Case colType_TextReadOnly : Add_Columns = _DataGridView.Add_TextColumn_AsReadOnly(dgv, columnTypeCommaTitle(i + 1))
                        Case colType_DollarText : Add_Columns = _DataGridView.Add_DollarTextColumn(dgv, columnTypeCommaTitle(i + 1))
                        Case colType_PercentText : Add_Columns = _DataGridView.Add_PercentTextColumn(dgv, columnTypeCommaTitle(i + 1))
                        Case colType_CheckBox : Add_Columns = _DataGridView.Add_CheckBoxColumn(dgv, columnTypeCommaTitle(i + 1))
                        Case colType_ComboBox : Add_Columns = _DataGridView.Add_ComboBoxColumn(dgv, columnTypeCommaTitle(i + 1))
                        Case colType_Button : Add_Columns = _DataGridView.Add_ButtonColumn(dgv, columnTypeCommaTitle(i + 1))
                    End Select
                End If
            Next i
            ''
        Catch ex As Exception : _Debug.Print_("_DataGridView.Add_Columns(): " & ex.Message)
        End Try
        ''
    End Function

    Public Function Add_Row(ByVal dgv As DataGridView, ByVal ParamArray values() As Object) As Boolean
        ''
        Add_Row = False
        Try
            Dim beforeAddCount As Integer = dgv.RowCount
            dgv.Rows.Add(values)
            Add_Row = (beforeAddCount < dgv.RowCount)
            ''
        Catch ex As Exception : _Debug.Print_("_DataGridView.Add_Row(): " & ex.Message)
        End Try
        ''
    End Function
    Public Function Edit_Row(ByVal dgvrow As DataGridViewRow, ByVal ParamArray values() As Object) As Boolean
        ''
        Edit_Row = False
        Try
            dgvrow.SetValues(values)
            Edit_Row = True
            ''
        Catch ex As Exception : _Debug.Print_("_DataGridView.Edit_Row(): " & ex.Message)
        End Try
        ''
    End Function

    Public Function Set_RowCell_AsReadOnly(ByVal dgv As DataGridView, ByVal rowIndex As Integer, ByVal cellIndex As Integer) As Boolean
        Set_RowCell_AsReadOnly = False
        Try
            If Not rowIndex > dgv.RowCount - 1 Then '' row index is zero-based
                If Not cellIndex > dgv.Rows.Item(rowIndex).Cells.Count - 1 Then '' cell index is zero-based
                    dgv.Rows.Item(rowIndex).Cells(cellIndex).ReadOnly = True
                    dgv.Rows.Item(rowIndex).Cells(cellIndex).Style.ForeColor = Color.Teal
                    'dgv.Rows.Item(rowIndex).Cells(cellIndex).Style.Font = New System.Drawing.Font("Arial Narrow", 12)
                    Set_RowCell_AsReadOnly = (dgv.Rows.Item(rowIndex).Cells(cellIndex).ReadOnly = True)
                End If
            End If
            ''
        Catch ex As Exception : _Debug.Print_("_DataGridView.Set_RowCell_AsReadOnly(): " & ex.Message)
        End Try
        ''
    End Function
    Public Function Set_RowCell_AsComboBox(ByRef dgv As DataGridView, ByVal columnIndex As Integer, ByVal rowIndex As Integer, ByRef comboCell As DataGridViewComboBoxCell) As Boolean
        Set_RowCell_AsComboBox = False
        Try
            With comboCell
                .MaxDropDownItems = 6
                .FlatStyle = FlatStyle.Flat
                .Style.ForeColor = Color.Maroon
            End With
            ''
            dgv(columnIndex, rowIndex) = comboCell
            Set_RowCell_AsComboBox = (dgv(columnIndex, rowIndex).Equals(comboCell))
            ''
        Catch ex As Exception : _Debug.Print_("_DataGridView.Set_RowCell_AsComboBox(): " & ex.Message)
        End Try
        ''
    End Function
    Public Function Set_RowCell_AsButton(ByRef dgv As DataGridView, ByVal columnIndex As Integer, ByVal rowIndex As Integer, ByRef buttonCell As DataGridViewButtonCell) As Boolean
        Set_RowCell_AsButton = False
        Try
            With buttonCell
                .FlatStyle = FlatStyle.Standard
                .Style.ForeColor = Color.Maroon
                .Style.BackColor = Color.Honeydew
            End With
            ''
            dgv(columnIndex, rowIndex) = buttonCell
            Set_RowCell_AsButton = (dgv(columnIndex, rowIndex).Equals(buttonCell))
            ''
        Catch ex As Exception : _Debug.Print_("_DataGridView.Set_RowCell_AsButton(): " & ex.Message)
        End Try
        ''
    End Function
    Public Function Set_RowCell_AsDollarText(ByRef dgv As DataGridView, ByVal columnIndex As Integer, ByVal rowIndex As Integer, ByRef dollarCell As DataGridViewTextBoxCell) As Boolean
        Set_RowCell_AsDollarText = False
        Try
            With dollarCell
                .ReadOnly = False
                .Style.Alignment = DataGridViewContentAlignment.MiddleRight
                .Style.ForeColor = Color.DarkGreen
                .Style.Format = "$0.00"
            End With
            ''
            dgv(columnIndex, rowIndex) = dollarCell
            Set_RowCell_AsDollarText = (dgv(columnIndex, rowIndex).Equals(dollarCell))
            ''
        Catch ex As Exception : _Debug.Print_("_DataGridView.Set_RowCell_AsDollarText(): " & ex.Message)
        End Try
        ''
    End Function
    Public Function Clear_Contents(ByRef dgv As DataGridView) As Boolean
        Clear_Contents = False
        Try
            dgv.Columns.Clear()
            Clear_Contents = (0 = dgv.RowCount)
            ''
        Catch ex As Exception : _Debug.Print_("_DataGridView.Clear_Contents(): " & ex.Message)
        End Try
        ''
    End Function

End Module
