Imports System.Data

Module SegmentFunctions

    Public Function RemoveBlankElementsFromSegment(ByVal Segment As String) As String

        Dim eName As String
        Dim eValue As String
        Dim NewSegment As String

        NewSegment = ""
        eName = ""
        eValue = ""
        Do Until Segment = ""

            Segment = ExtractNextElementFromSegment(eName, eValue, Segment)
            If Not eValue = "" And Not eValue = "0" And Not eValue = "False" Then

                NewSegment = AddElementToSegment(NewSegment, eName, eValue)

            End If

        Loop
        Return NewSegment

    End Function

    Function RemoveElementFromSegment(RemoveName, Segment) As String

        Dim eName As String
        Dim eValue As String
        Dim NewSegment As String

        NewSegment = ""
        eName = ""
        eValue = ""
        Do Until Segment = ""

            Segment = ExtractNextElementFromSegment(eName, eValue, Segment)
            If Not eName = RemoveName Then

                NewSegment = AddElementToSegment(NewSegment, eName, eValue)

            End If

        Loop
        Return NewSegment

    End Function

    Function ChangeElementValueFromSegment(CName, CValue, Segment) As String

        Dim eName As String
        Dim eValue As String
        Dim NewSegment As String

        NewSegment = ""
        eName = ""
        eValue = ""
        Do Until Segment = ""

            Segment = ExtractNextElementFromSegment(eName, eValue, Segment)
            If Not eName = CName Then

                NewSegment = AddElementToSegment(NewSegment, eName, eValue)
            Else
                NewSegment = AddElementToSegment(NewSegment, eName, CValue)

            End If

        Loop


        Return NewSegment

    End Function

    Public Function GetNextSegmentFromSet(ByRef buf As String) As String

        ' Use this function  to extract the next packed segment encapsulated in <SET> and </SET>

        Dim sbuf As String
        Dim iloc As Integer

        sbuf = ""
        iloc = InStr(1, buf, "</SET>")
        If Not iloc = 0 Then

            sbuf = Strings.Mid(buf, 6, iloc - 6)
            buf = Strings.Mid(buf, iloc + 8)

        Else

            buf = ""

        End If
        buf = Trim$(buf)
        Return sbuf

    End Function

    Public Function ExtractElementFromSegment(ByVal eName As String, ByVal Segment As String, Optional ByVal defaultValue As String = "") As String

        Dim iloc As Int32
        Dim iloc2 As Int32
        Dim buf As String = ""
        Dim sloc As Int32
        Dim eloc As Int32
        Dim EndChar As Integer
        Dim ComplexSegment As String

        If Segment IsNot Nothing AndAlso Not String.IsNullOrEmpty(Segment) Then

            ComplexSegment = ""
            iloc = InStr(1, Segment, Chr(171) & "@")
            If Not iloc = 0 Then

                ComplexSegment = Strings.Mid$(Segment, iloc)
                Segment = Strings.Mid$(Segment, 1, iloc - 1)

            End If
            EndChar = 187
            buf = ""
            iloc = InStr(1, Segment.ToUpper, Chr(171) & eName.ToUpper & " ")
            If Not iloc = 0 Then

                iloc2 = InStr(iloc, Segment, Chr(EndChar))
                sloc = InStr(iloc, Segment, " ") + 1
                eloc = iloc2 - sloc
                buf = Strings.Mid$(Segment, sloc, eloc)

            Else

                iloc = InStr(1, ComplexSegment, (Chr(171) & ("@" & eName)))
                If Not iloc = 0 Then

                    iloc2 = InStr(iloc, ComplexSegment, ("@" & Chr(EndChar)))
                    If Not iloc2 = 0 Then

                        sloc = InStr(iloc, ComplexSegment, " ") + 1
                        eloc = iloc2 - sloc
                        buf = Strings.Mid$(ComplexSegment, sloc, eloc)

                    End If

                End If

            End If

        End If
        '
        ' ol: added Default value parameter if returned one is null
        If String.IsNullOrEmpty(buf) Then
            buf = defaultValue
        End If
        '
        Return buf

    End Function

    Public Function AddElementToSegment(Segment As String, eName As String, eValue As String) As String

        Dim iloc As Int32
        Dim iloc2 As Int32
        Dim StoringSegment As Boolean
        Dim ComplexSegment As String

        iloc = InStr(1, eValue, Chr(171))
        If Not iloc = 0 Then

            StoringSegment = True

        Else

            StoringSegment = False

        End If
        iloc = InStr(1, Segment, Chr(171) & "$")
        If Not iloc = 0 Then

            ComplexSegment = Strings.Mid$(Segment, iloc)
            Segment = Strings.Mid$(Segment, 1, iloc - 1)

        Else

            ComplexSegment = ""

        End If
        If StoringSegment = False Then

            iloc = InStr(1, Segment, Chr(171) & eName & " ")
            If Not iloc = 0 Then

                iloc2 = InStr(iloc, Segment, Chr(187))
                If iloc = 1 Then

                    Segment = Chr(171) & eName & " " & eValue & Chr(187) & Strings.Mid$(Segment, iloc2 + 1)

                Else

                    Segment = Strings.Mid$(Segment, 1, iloc - 1) & Chr(171) & eName & " " & eValue & Chr(187) & Strings.Mid$(Segment, iloc2 + 1)

                End If

            Else

                Segment = Segment & (Chr(171) & eName & " " & eValue & Chr(187))

            End If

        Else

            iloc = InStr(1, ComplexSegment, Chr(171) & "@" & eName & " ")
            If Not iloc = 0 Then

                iloc2 = InStr(iloc, ComplexSegment, "@" & Chr(187))
                If iloc = 1 Then

                    ComplexSegment = Chr(171) & "@" & eName & " " & eValue & "@" & Chr(187) & Strings.Mid$(ComplexSegment, iloc2 + 2)

                Else

                    ComplexSegment = Strings.Mid$(ComplexSegment, 1, iloc - 1) & (Chr(171) & "@" & eName & " " & eValue & "@" & Chr(187)) & Strings.Mid$(ComplexSegment, iloc2 + 2)

                End If

            Else

                ComplexSegment = ComplexSegment & (Chr(171) & "@" & eName & " " & eValue & "@" & Chr(187))

            End If

        End If
        AddElementToSegment = Segment & ComplexSegment

    End Function


    Public Function MergeSegment(SegmentFrom As String, SegmentTo As String) As String

        Dim eName As String
        Dim eValue As String
        Dim Segment As String
        Dim SegmentNew As String

        SegmentNew = SegmentFrom
        Segment = SegmentTo
        eName = ""
        eValue = ""
        Do Until Segment = ""

            Segment = ExtractNextElementFromSegment(eName, eValue, Segment)
            If Not eValue = "" Then

                SegmentNew = AddElementToSegment(SegmentNew, eName, eValue)

            End If

        Loop
        Return SegmentNew

    End Function

    Public Function ExtractNextElementFromSegment(ByRef eName As String, ByRef eValue As String, Segment As String) As String

        Dim iloc As Integer

        iloc = InStr(1, Segment, " ")
        If iloc = 0 Then

            eName = ""
            eValue = ""
            Return ""
            Exit Function

        End If
        eName = Strings.Mid$(Segment, 2, iloc - 2)
        Segment = Strings.Mid$(Segment, iloc + 1)
        If Strings.Mid$(eName, 1, 1) = "@" Then

            eName = Strings.Mid$(eName, 2)
            iloc = InStr(1, Segment, ("@" & Chr(187)))
            eValue = Strings.Mid$(Segment, 1, iloc - 1)
            Segment = Strings.Mid$(Segment, iloc + 2)

        Else

            iloc = InStr(1, Segment, Chr(187))
            eValue = Strings.Mid$(Segment, 1, iloc - 1)
            Segment = Strings.Mid$(Segment, iloc + 1)

        End If
        Return Trim$(Segment)

    End Function

    Public Function IsElementInSegment(ByVal eName As String, ByVal Segment As String) As Boolean

        Return _Controls.Contains(Segment, Chr(171) & eName & " ")

    End Function


    Public Function LoadSegmentInToListView(ByRef LV As ListView, ByVal DT As DataTable, ByVal InSegment As String, ColumnCT As Integer) As Integer

        Dim MyItems As String()

        'LV.Items.Clear()
        ReDim MyItems(ColumnCT)
        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        Dim ct As Integer = 0
        Dim i As Integer
        Dim buf As String = ""
        Dim Segment As String = ""
        Dim SegmentSet As String = ""
        Dim eName As String = ""
        Dim eValue As String = ""
        Dim dRow As DataRow

        SegmentSet = InSegment

        Do Until SegmentSet = ""

            Segment = GetNextSegmentFromSet(SegmentSet)
            i = 0
            dRow = DT.NewRow
            Do Until Segment = ""

                Segment = ExtractNextElementFromSegment(eName, eValue, Segment)
                MyItems(i) = eValue.ToString

                'TODO: Fix
                'dRow(i) = MyItems(i)
                If dRow.Table.Columns.Item(i).DataType = GetType(String) Then
                    dRow(i) = MyItems(i)
                ElseIf dRow.Table.Columns.Item(i).DataType = GetType(Date) Then
                    If MyItems(i) <> "" Then
                        dRow(i) = MyItems(i)
                    End If
                ElseIf dRow.Table.Columns.Item(i).DataType = GetType(Boolean) Then
                    Boolean.TryParse(MyItems(i), dRow(i))
                Else
                    dRow(i) = Val(MyItems(i))
                End If

                i += 1

            Loop

            DT.Rows.Add(dRow)
            ct += 1

        Loop

        LV.DataContext = DT
        LV.SetBinding(ListView.ItemsSourceProperty, New Binding)

        Return ct

    End Function


End Module
