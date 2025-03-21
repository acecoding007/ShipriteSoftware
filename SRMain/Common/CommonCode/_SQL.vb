
Public Class sqlINSERT

    Private m_flds As String
    Private m_vals As String
    Private m_tblName As String
    Private m_where As String
    Private m_whatType As Byte
    ''
    Public Property TXT_() As Byte
        Get
            TXT_ = 1  '' string type
        End Get
        Set(ByVal value As Byte)
        End Set
    End Property
    Public Property DTE_() As Byte
        Get
            DTE_ = 2   '' date type
        End Get
        Set(ByVal value As Byte)
        End Set
    End Property
    Public Property NUM_() As Byte
        Get
            NUM_ = 3   '' number type
        End Get
        Set(ByVal value As Byte)
        End Set
    End Property

    Public Function Qry_INSERT(ByVal fldName As String, ByVal fldValue As String, Optional ByVal fldType As Byte = 1, Optional ByVal isQryStart As Boolean = False, Optional ByVal isQryEnd As Boolean = False, Optional ByVal tblName As String = "SORZHLP", Optional ByVal whatType As Byte = 1) As String
        ''
        Dim sql2exe As String
        Dim tmpVal As String
        ''
        Qry_INSERT = String.Empty '' assume.
        ''
        If isQryStart Then
            ''
            m_whatType = whatType
            m_tblName = tblName
            m_flds = String.Empty
            m_vals = String.Empty
            ''
        End If
        ''
        If fldType = TXT_ Then
            '' replace single quote: 
            tmpVal = "'" & _Controls.Replace(fldValue, "'", "''") & "'"
        ElseIf fldType = DTE_ Then
            Select Case m_whatType
                Case _Connection.Oracle_OLEDB : tmpVal = ("to_date('" & fldValue & "', 'MM/DD/YYYY HH24:MI:SS')")
                Case _Connection.Jet_OLEDB, _Connection.Jet_OLEDB_CSV : tmpVal = ("#" & fldValue & "#")
                Case Else : tmpVal = ("'" & fldValue & "'")
            End Select
        Else
            tmpVal = fldValue '' NUM
            '' cannot be empty for a Number: 
            If 0 = Len(tmpVal) Then tmpVal = "0"
            '
            If m_whatType = _Connection.Sql_OLEDB Then
                tmpVal = _Controls.Replace(tmpVal, "True", "1")
                tmpVal = _Controls.Replace(tmpVal, "False", "0")
            End If
            '
        End If
        ''
        If isQryEnd And Not (m_whatType = _Connection.Oracle_OLEDB) Then
            ''
            m_flds = (m_flds & "[" & fldName & "]")
            m_vals = (m_vals & tmpVal)
            ''
            If m_whatType = _Connection.Sql_OLEDB Then sql2exe = ("INSERT INTO [dbo].") Else sql2exe = "INSERT INTO "
            sql2exe = (sql2exe & "[" & m_tblName & "] (" & m_flds & ") VALUES (" & m_vals & ")")
            ''
            Qry_INSERT = sql2exe
            ''
        ElseIf isQryEnd And (m_whatType = _Connection.Oracle_OLEDB) Then
            ''
            m_flds = (m_flds & fldName)
            m_vals = (m_vals & tmpVal)
            ''
            sql2exe = "INSERT INTO "
            sql2exe = (sql2exe & m_tblName & " (" & m_flds & ") VALUES (" & m_vals & ")")
            ''
            Qry_INSERT = sql2exe
            ''
        Else
            ''
            If m_whatType = _Connection.Oracle_OLEDB Then m_flds = (m_flds & fldName & ", ") Else m_flds = (m_flds & "[" & fldName & "], ")
            m_vals = (m_vals & tmpVal & ", ")
            ''
        End If
        ''
    End Function

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub

End Class

Public Class sqlUpdate

    Private m_flds As String
    Private m_vals As String
    Private m_tblName As String
    Private m_where As String
    Private m_whatType As Byte
    ''
    Public Property TXT_() As Byte
        Get
            TXT_ = 1  '' string type
        End Get
        Set(ByVal value As Byte)
        End Set
    End Property
    Public Property DTE_() As Byte
        Get
            DTE_ = 2   '' date type
        End Get
        Set(ByVal value As Byte)
        End Set
    End Property
    Public Property NUM_() As Byte
        Get
            NUM_ = 3   '' number type
        End Get
        Set(ByVal value As Byte)
        End Set
    End Property

    Public Function Qry_UPDATE(ByVal fldName As String, ByVal fldValue As String, Optional ByVal fldType As Byte = 1, Optional ByVal isQryStart As Boolean = False, Optional ByVal isQryEnd As Boolean = False, Optional ByVal tblName As String = "UNITS", Optional ByVal qryWhere As String = "VLUID <> 0", Optional ByVal whatType As Byte = 1) As String
        ''
        Dim sql2exe As String
        Dim tmpVal As String
        ''
        Qry_UPDATE = String.Empty '' assume.
        ''
        If isQryStart Then
            ''
            m_whatType = whatType
            m_where = qryWhere
            If whatType = _Connection.Sql_OLEDB Then
                m_where = _Controls.Replace(m_where, "#", "'")
                m_where = _Controls.Replace(m_where, "True", "1")
                m_where = _Controls.Replace(m_where, "False", "0")
            End If
            m_tblName = tblName
            m_flds = String.Empty
            m_vals = String.Empty
            ''
        End If
        ''
        If fldType = TXT_ Then
            '' replace single quote: 
            tmpVal = "'" & _Controls.Replace(fldValue, "'", "''") & "'"
        ElseIf fldType = DTE_ Then
            Select Case m_whatType
                Case _Connection.Oracle_OLEDB : tmpVal = ("to_date('" & fldValue & "', 'MM/DD/YYYY HH24:MI:SS')")
                Case _Connection.Jet_OLEDB, _Connection.Jet_OLEDB_CSV : tmpVal = ("#" & fldValue & "#")
                Case Else : tmpVal = ("'" & fldValue & "'")
            End Select
        Else
            tmpVal = fldValue '' NUM
            '
            '' cannot be empty for a Number: 
            If 0 = Len(tmpVal) Then
                If m_whatType = _Connection.Oracle_OLEDB Then
                    tmpVal = "Null"
                Else
                    tmpVal = "0"
                End If
            End If
            '
            If m_whatType = _Connection.Sql_OLEDB Then
                tmpVal = _Controls.Replace(tmpVal, "True", "1")
                tmpVal = _Controls.Replace(tmpVal, "False", "0")
            End If
            '
        End If
        ''
        If isQryEnd And Not (m_whatType = _Connection.Oracle_OLEDB) Then
            ''
            m_flds = (m_flds & "[" & fldName & "] = " & tmpVal)
            ''
            If whatType = _Connection.Sql_OLEDB Then sql2exe = ("UPDATE [dbo].") Else sql2exe = "UPDATE "
            sql2exe = (sql2exe & "[" & m_tblName & "] SET " & m_flds & " WHERE " & m_where)
            ''
            Qry_UPDATE = sql2exe
            ''
        ElseIf isQryEnd And (m_whatType = _Connection.Oracle_OLEDB) Then
            ''
            m_flds = (m_flds & fldName & " = " & tmpVal)
            ''
            sql2exe = "UPDATE "
            sql2exe = (sql2exe & m_tblName & " SET " & m_flds & " WHERE " & m_where)
            ''
            Qry_UPDATE = sql2exe
            ''
        Else
            ''
            If m_whatType = _Connection.Oracle_OLEDB Then m_flds = (m_flds & fldName & " = " & tmpVal & ", ") Else m_flds = (m_flds & "[" & fldName & "] = " & tmpVal & ", ")
            ''
        End If
        ''
    End Function

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub
End Class

