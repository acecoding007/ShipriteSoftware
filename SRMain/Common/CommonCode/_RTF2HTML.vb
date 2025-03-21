'Version 3.03
'Copyright Brady Hegberg 2000
'  I'm not licensing this software but I'd appreciate it if
'  you'd to consider it to be under an lgpl sort of license

'More information can be found at
'http://www2.bitstream.net/~bradyh/downloads/rtf2htmlrm.html

'Converts Rich Text encoded text to HTML format
'if you find some text that this function doesn't
'convert properly please email the coded text to
'bradyh@bitstream.net

'Thanks to various people for assistance including
'   Anthony DiMauro for work on 3.0x font support
'   ...and many others

'sample use: Me.Text1.Text = Convert.rtf2html3(Me.RichTextBox1.TextRTF)

Imports System.Runtime.InteropServices
Imports System.Text
'In .NET Types are changed to Structures

Namespace RTF2HTML
    Module _RTF2HTML

        Private Structure CodeList
            Dim Code As String
            Dim Status As String               'P=Pending;A=Active;G=Paragraph;D=Dead;K=Killed
            '"Dead" means the code is active but will be killed at next text
            '"Pending" means it's waiting for text - if the code is canceled before text appears it will be killed
            '"Active" means there is text using the code at this moment
            '"Paragraph" means that the code stays active until the next paragraph: "/pard" or "/pntext"
        End Structure

        Private strCurPhrase As String
        Private strHTML As String
        Private Codes() As CodeList
        Private CodesBeg() As CodeList      'beginning codes
        Private NextCodes() As String
        Private NextCodesBeg() As String    'beginning codes for next text
        Private CodesTmp() As String           'temp stack for copying
        Private CodesTmpBeg() As String        'temp stack for copying beg

        Private strCR As String             'string to use for CRs - blank if +CR not chosen in options
        Private strBeforeText As String
        Private strBeforeText2 As String
        Private strBeforeText3 As String
        Private gPlain As Boolean              'true if all codes shouls be popped before next text
        Private gWBPlain As Boolean            'plain will be true after next text
        Private strColorTable() As String      'table of colors
        Private lColors As Long                '# of colors
        Private strEOL As String               'string to include before <br>
        Private strBOL As String               'string to include after <br>
        Private lSkipWords As Long             'number od words to skip from current
        Private gBOL As Boolean                'a <br> was inserted but no non-whitespace text has been inserted
        Private gPar As Boolean                'true if paragraph was reached since last text
        Private lBrLev As Long                 'bracket level when finding matching brackets
        Private strSecTmp As String            'temporary section buffer
        Private gIgnorePard As Boolean         'should pard end list items or not?

        Private strFontTable() As String       'table of fonts
        Private lFonts As Long                 '# of fonts
        Private strFont As String
        Private strTable As String
        Private strFace As String              'current font face for setting up fontstring
        Private strFontColor As String         'current font color for setting up fontstring
        Private strFontSize As String          'current font size for setting up fontstring
        Private lFontSize As Long
        Private iDefFontSize As Integer        'default font size
        Private gUseFontFace As Boolean        'use different fonts or always use default font

        Private gDebug As Boolean           'for debugging
        Private gStep As Boolean            'for debugging

        Private Sub ClearCodes()
            ReDim Codes(0)
            ReDim CodesBeg(0)
            ClearNext()
        End Sub
        Private Sub ClearNext(Optional ByRef strExcept As String = "")

            If Len(strExcept) > 0 Then
                If InNext(strExcept) Then
                    While NextCodes(1) <> strExcept
                        ShiftNext()
                        ShiftNextBeg()
                    End While
                    Exit Sub
                End If
            End If

            ReDim NextCodes(0)
            ReDim NextCodesBeg(0)

        End Sub
        Private Sub ClearFont()
            strFont = ""
            strTable = ""
            strFontColor = ""
            strFace = ""
            strFontSize = ""
            lFontSize = 0
        End Sub
        Private Sub Codes2NextTill(ByRef strCode As String)
            Dim l As Long

            For l = 1 To UBound(Codes)
                If Codes(l).Code = strCode Then Exit For
                If Codes(l).Status <> "K" And Codes(l).Status <> "D" Then
                    If Not InNext(strCode) Then
                        UnShiftNext(Codes(l).Code)
                        UnShiftNextBeg(CodesBeg(l).Code)
                    End If
                End If
            Next l
        End Sub
        Private Sub GetColorTable(ByRef strSecTmp As String, ByRef strColorTable() As String)
            'get color table data and fill in strColorTable array
            Dim lColors As Long
            Dim lBOS As Integer
            Dim lEOS As Integer
            Dim strTmp As String
            'Dim numTmp As long

            lBOS = InStr(1, strSecTmp, "\colortbl")
            'ReDim strColorTable(0)
            lColors = 1
            If lBOS <> 0 Then
                lBOS = InStr(lBOS, strSecTmp, ";")
                lEOS = InStr(lBOS, strSecTmp, ";}")
                If lEOS <> 0 Then
                    lBOS = InStr(lBOS, strSecTmp, "\red")
                    While ((lBOS <= lEOS) And (lBOS <> 0))
                        ReDim Preserve strColorTable(lColors)
                        strTmp = Mid(strSecTmp, lBOS + 4, 1) & IIf(IsNumeric(Mid(strSecTmp, lBOS + 5, 1)), Mid(strSecTmp, lBOS + 5, 1), "") & IIf(IsNumeric(Mid(strSecTmp, lBOS + 6, 1)), Mid(strSecTmp, lBOS + 6, 1), "")
                        If IsNumeric(strTmp) Then
                            strTmp = Trim(Hex(Val(strTmp)))
                        End If
                        If Len(strTmp) = 1 Then strTmp = "0" & strTmp
                        strColorTable(lColors) = strColorTable(lColors) & strTmp
                        lBOS = InStr(lBOS, strSecTmp, "\green")
                        strTmp = Trim(Hex(Val(Mid(strSecTmp, lBOS + 6, 1) & IIf(IsNumeric(Mid(strSecTmp, lBOS + 7, 1)), Mid(strSecTmp, lBOS + 7, 1), "") & IIf(IsNumeric(Mid(strSecTmp, lBOS + 8, 1)), Mid(strSecTmp, lBOS + 8, 1), ""))))
                        If Len(strTmp) = 1 Then strTmp = "0" & strTmp
                        strColorTable(lColors) = strColorTable(lColors) & strTmp
                        lBOS = InStr(lBOS, strSecTmp, "\blue")
                        strTmp = Trim(Hex(Val(Mid(strSecTmp, lBOS + 5, 1) & IIf(IsNumeric(Mid(strSecTmp, lBOS + 6, 1)), Mid(strSecTmp, lBOS + 6, 1), "") & IIf(IsNumeric(Mid(strSecTmp, lBOS + 7, 1)), Mid(strSecTmp, lBOS + 7, 1), ""))))
                        If Len(strTmp) = 1 Then strTmp = "0" & strTmp
                        strColorTable(lColors) = strColorTable(lColors) & strTmp
                        lBOS = InStr(lBOS, strSecTmp, "\red")
                        lColors = lColors + 1
                    End While
                End If
            End If
        End Sub
        Private Sub GetFontTable(ByRef strSecTmp As String, ByRef strFontTable() As String)
            'get font table data and fill in strFontTable array
            Dim lFonts As Long
            Dim lBOS As Integer
            Dim lEOS As Integer
            Dim strTmp As String
            Dim lLvl As Long
            Dim strNextChar As String

            lBOS = InStr(1, strSecTmp, "\fonttbl")
            ReDim strFontTable(0)
            lFonts = 0
            If lBOS <> 0 Then
                lEOS = InStr(lBOS, strSecTmp, ";}}")
                If lEOS <> 0 Then
                    lBOS = InStr(lBOS, strSecTmp, "\f0")
                    While ((lBOS <= lEOS) And (lBOS <> 0))
                        ReDim Preserve strFontTable(lFonts)
                        strNextChar = Mid(strSecTmp, lBOS, 1)
                        While (((strNextChar <> " ") And (lBOS <= lEOS)) Or (lLvl > 0))
                            lBOS = lBOS + 1
                            If strNextChar = "{" Then
                                lLvl = lLvl + 1
                                strNextChar = Mid(strSecTmp, lBOS, 1)
                            ElseIf strNextChar = "}" Then
                                lLvl = lLvl - 1
                                If lLvl = 0 Then
                                    strNextChar = " "
                                    lBOS = lBOS - 1
                                Else
                                    strNextChar = Mid(strSecTmp, lBOS, 1)
                                End If
                            Else
                                strNextChar = Mid(strSecTmp, lBOS, 1)
                            End If
                        End While
                        lBOS = lBOS + 1
                        strTmp = Mid(strSecTmp, lBOS, InStr(lBOS, strSecTmp, ";") - lBOS)
                        strFontTable(lFonts) = strFontTable(lFonts) & strTmp
                        lBOS = InStr(lBOS, strSecTmp, "\f" & (lFonts + 1))
                        lFonts = lFonts + 1
                    End While
                End If
            End If
        End Sub

        Private Function InNext(ByRef strTmp) As Boolean
            Dim gTmp As Boolean
            Dim l As Long

            l = 1
            gTmp = False
            While l <= UBound(NextCodes) And Not gTmp
                If NextCodes(l) = strTmp Then gTmp = True
                l = l + 1
            End While
            InNext = gTmp
        End Function
        Private Function InNextBeg(ByRef strTmp) As Boolean
            Dim gTmp As Boolean
            Dim l As Long

            l = 1
            gTmp = False
            While l <= UBound(NextCodesBeg) And Not gTmp
                If NextCodesBeg(l) = strTmp Then gTmp = True
                l = l + 1
            End While
            InNextBeg = gTmp
        End Function
        Private Function InCodes(ByRef strTmp As String, Optional ByRef gActiveOnly As Boolean = False) As Boolean
            Dim gTmp As Boolean
            Dim l As Long

            l = 1
            gTmp = False
            While l <= UBound(Codes) And Not gTmp
                If gActiveOnly Then
                    If Codes(l).Code = strTmp And (Codes(l).Status = "A" Or Codes(l).Status = "G") Then gTmp = True
                Else
                    If Codes(l).Code = strTmp Then gTmp = True
                End If
                l = l + 1
            End While
            InCodes = gTmp
        End Function
        Private Function InCodesBeg(ByRef strTmp As String) As Boolean
            Dim gTmp As Boolean
            Dim l As Long

            l = 1
            gTmp = False
            While l <= UBound(CodesBeg) And Not gTmp
                If CodesBeg(l).Code = strTmp Then gTmp = True
                l = l + 1
            End While
            InCodesBeg = gTmp
        End Function

        Private Function NabNextLine(ByRef strRTF As String) As String
            Dim l As Long

            l = InStr(1, strRTF, vbCrLf)
            If l = 0 Then l = Len(strRTF)
            NabNextLine = TrimAll(Left(strRTF, l))
            If l = Len(strRTF) Then
                strRTF = ""
            Else
                strRTF = TrimAll(Mid(strRTF, l))
            End If
        End Function
        Private Function NabNextWord(ByRef strLine As String) As String
            Dim l As Integer
            Dim lvl As Integer
            Dim gEndofWord As Boolean
            Dim gInCommand As Boolean    'current word is command instead of plain word

            gInCommand = False
            l = 0
            lvl = 0
            'strLine = TrimifCmd(strLine)
            If Left(strLine, 1) = "}" Then
                strLine = Mid(strLine, 2)
                NabNextWord = "}"
                Exit Function
            End If
            If Left(strLine, 1) = "{" Then
                strLine = Mid(strLine, 2)
                NabNextWord = "{"
                Exit Function
            End If
            If Left(strLine, 2) = "\'" Then
                NabNextWord = Left(strLine, 4)
                strLine = Mid(strLine, 5)
                Exit Function
            End If
            If Left(strLine, 2) = "\\" Or Left(strLine, 2) = "\{" Or Left(strLine, 2) = "\}" Then
                NabNextWord = Left(strLine, 2)
                strLine = Mid(strLine, 3)
                Exit Function
            End If
            While Not gEndofWord
                l = l + 1
                If l >= Len(strLine) Then
                    If l = Len(strLine) Then l = l + 1
                    gEndofWord = True
                ElseIf InStr(1, "\{}", Mid(strLine, l, 1)) Then
                    If l = 1 And Mid(strLine, l, 1) = "\" Then gInCommand = True
                    '            If Mid(strLine, l + 1, 1) <> "\" And l > 1 And lvl = 0 Then    'avoid...what?
                    If l > 1 And lvl = 0 Then
                        gEndofWord = True
                    End If
                ElseIf Mid(strLine, l, 1) = " " And lvl = 0 And gInCommand Then
                    gEndofWord = True
                End If
            End While

            If l = 0 Then l = Len(strLine)
            NabNextWord = Left(strLine, l - 1)
            While Len(NabNextWord) > 0 And InStr(1, "{}", Right(NabNextWord, 1)) And l > 0
                NabNextWord = Left(NabNextWord, Len(NabNextWord) - 1)
                l = l - 1
            End While

            strLine = Mid(strLine, l)
            If Left(strLine, 1) = " " Then strLine = Mid(strLine, 2)

        End Function
        Private Function NabSection(ByRef strRTF As String, ByRef lPos As Long) As String
            'grab section surrounding lPos, strip section out of strRTF and return it
            Dim lBOS As Long         'beginning of section
            Dim lEOS As Long         'ending of section
            Dim strChar As String
            Dim lLev As Long         'level of brackets/parens
            Dim lRTFLen As Long

            lRTFLen = Len(strRTF)

            lBOS = lPos
            strChar = Mid(strRTF, lBOS, 1)
            lLev = 1
            While lLev > 0
                lBOS = lBOS - 1
                If lBOS <= 0 Then
                    lLev = lLev - 1
                Else
                    strChar = Mid(strRTF, lBOS, 1)
                    If strChar = "}" Then
                        lLev = lLev + 1
                    ElseIf strChar = "{" Then
                        lLev = lLev - 1
                    End If
                End If
            End While
            lBOS = lBOS - 1
            If lBOS < 1 Then lBOS = 1

            lEOS = lPos
            strChar = Mid(strRTF, lEOS, 1)
            lLev = 1
            While lLev > 0
                lEOS = lEOS + 1
                If lEOS >= lRTFLen Then
                    lLev = lLev - 1
                Else
                    strChar = Mid(strRTF, lEOS, 1)
                    If strChar = "{" Then
                        lLev = lLev + 1
                    ElseIf strChar = "}" Then
                        lLev = lLev - 1
                    End If
                End If
            End While
            lEOS = lEOS + 1
            If lEOS > lRTFLen Then lEOS = lRTFLen
            NabSection = Mid(strRTF, lBOS + 1, lEOS - lBOS - 1)
            strRTF = Mid(strRTF, 1, lBOS) & Mid(strRTF, lEOS)
            strRTF = rtf2html_replace(strRTF, vbCrLf & vbCrLf, vbCrLf)
        End Function

        Private Sub Next2Codes()
            'move codes from pending ("next") stack to front of current stack
            Dim lNumCodes As Long
            Dim lNumNext As Long
            Dim l As Long

            If UBound(NextCodes) > 0 Then
                If InNext("</li>") Then
                    For l = UBound(NextCodes) To 1 Step -1
                        If NextCodes(l) = "</li>" And l > 1 Then
                            NextCodes(l) = NextCodes(l - 1)
                            NextCodesBeg(l) = NextCodesBeg(l - 1)
                            NextCodes(l - 1) = "</li>"
                            NextCodesBeg(l - 1) = "<li>"
                        End If
                    Next l
                End If

                lNumCodes = UBound(Codes)
                lNumNext = UBound(NextCodes)
                ReDim Preserve Codes(lNumCodes + lNumNext)
                ReDim Preserve CodesBeg(lNumCodes + lNumNext)
                For l = UBound(Codes) To 1 Step -1
                    If l > lNumNext Then
                        Codes(l) = Codes(l - lNumNext)
                        CodesBeg(l) = CodesBeg(l - lNumNext)
                    Else
                        Codes(l).Code = NextCodes(lNumNext - l + 1)
                        CodesBeg(l).Code = NextCodesBeg(lNumNext - l + 1)
                        Select Case Codes(l).Code
                            Case "</td></tr></table>", "</li>"
                                Codes(l).Status = "PG"
                                CodesBeg(l).Status = "PG"
                            Case Else
                                Codes(l).Status = "P"
                                CodesBeg(l).Status = "P"
                        End Select
                    End If
                Next l
                ReDim NextCodes(0)
                ReDim NextCodesBeg(0)
            End If
        End Sub
        Private Sub Codes2Next()
            'move codes from "current" stack to pending ("next") stack
            Dim lNumCodes As Long
            Dim l As Long

            If UBound(Codes) > 0 Then
                lNumCodes = UBound(NextCodes)
                ReDim Preserve NextCodes(lNumCodes + UBound(Codes))
                ReDim Preserve NextCodesBeg(lNumCodes + UBound(Codes))
                For l = 1 To UBound(Codes)
                    NextCodes(lNumCodes + l) = Codes(l).Code
                    NextCodesBeg(lNumCodes + l) = CodesBeg(l).Code
                Next l
                ReDim Codes(0)
                ReDim CodesBeg(0)
            End If
        End Sub

        Private Function ParseFont(ByRef strColor As String, ByRef strSize As String, ByRef strFace As String) As String
            Dim strTmpFont As String

            If strColor & strSize & strFace = "" Then
                strTmpFont = ""
            Else
                strTmpFont = "<font"
                If strFace <> "" Then
                    strTmpFont = strTmpFont & " face=""" & strFace & """"
                End If
                If strColor <> "" Then
                    strTmpFont = strTmpFont & " color=""" & strColor & """"
                End If
                If strSize <> "" And Val(strSize) <> iDefFontSize Then
                    strTmpFont = strTmpFont & " size=" & strSize
                End If
                strTmpFont = strTmpFont & ">"
            End If
            ParseFont = strTmpFont
        End Function

        Private Sub PopCode()
            If UBound(Codes) > 0 Then
                ''PopCode = Codes(UBound(Codes)).Code
                ReDim Preserve Codes(UBound(Codes) - 1)
            End If
        End Sub

        Private Function ProcessAfterTextCodes() As String
            Dim strTmp As String
            Dim l As Long
            Dim lLastKilled As Long
            Dim lRetVal As Long

            'check for/handle font change
            If strFont <> GetLastFont() Then
                KillCode("</font>")
                If Len(strFont) > 0 Then
                    lRetVal = ReplaceInNextBeg("</font>", strFont)
                    If lRetVal = 0 Then
                        PushNext("</font>")
                        PushNextBeg(strFont)
                    End If
                End If
            Else
                If Not InNext("</li>") Then ReviveCode("</font>")
            End If

            'now handle everything killed and move codes farther in to next
            '    ie: \b B\i B \u B\i0 B \u0\b0 => <b>B<i>B<u>B</u>B</i><u>B</u></b>
            strTmp = ""
            If UBound(Codes) > 0 Then
                lLastKilled = 0
                For l = UBound(Codes) To 1 Step -1
                    If Codes(l).Status = "K" Then
                        lLastKilled = l
                        Exit For
                    End If
                Next l
                If lLastKilled > 0 Then
                    For l = 1 To lLastKilled
                        strTmp = strTmp & Codes(l).Code
                        If Codes(l).Code = "</li>" Then strTmp = strTmp & strCR
                    Next l
                    For l = lLastKilled To 1 Step -1
                        If Codes(l).Status <> "D" And Codes(l).Status <> "K" Then
                            If Not InNext(Codes(l).Code) Then
                                PushNext(Codes(l).Code)
                                PushNextBeg(CodesBeg(l).Code)
                            End If
                            Codes(l).Status = "K"
                            CodesBeg(l).Status = "K"
                        End If
                    Next l
                End If
            End If
            ProcessAfterTextCodes = strTmp
        End Function

        Private Function GetActiveCodes() As String
            Dim strTmp As String
            Dim l As Long

            strTmp = ""
            If UBound(Codes) > 0 Then
                For l = 1 To UBound(Codes)
                    strTmp = strTmp & Codes(l).Code
                Next l
            End If
            GetActiveCodes = strTmp
        End Function

        Private Function GetLastFont() As String
            Dim strTmp As String
            Dim l As Long

            strTmp = ""
            If UBound(Codes) > 0 Then
                For l = UBound(Codes) To 1 Step -1
                    If Codes(l).Code = "</font>" Then
                        strTmp = CodesBeg(l).Code
                        Exit For
                    End If
                Next l
            End If
            GetLastFont = strTmp
        End Function

        Private Sub SetPendingCodesActive()
            Dim strTmp As String
            Dim l As Long

            strTmp = ""
            If UBound(Codes) > 0 Then
                For l = 1 To UBound(Codes)
                    If Codes(l).Status = "P" Then
                        Codes(l).Status = "A"
                        CodesBeg(l).Status = "A"
                    ElseIf Codes(l).Status = "PG" Then
                        Codes(l).Status = "G"
                        CodesBeg(l).Status = "G"
                    End If
                Next l
            End If
        End Sub

        Private Function KillCode(ByRef strCode As String, Optional ByRef strExcept As String = "") As Long
            KillCode = 0
            'mark all codes of type strCode as killed
            '    except where status = strExcept
            '    if strCode = "*" then mark all killed
            Dim strTmp As String
            Dim l As Long

            strTmp = ""
            If UBound(Codes) > 0 Then
                If Left(strExcept, 1) = "<" Then    'strExcept is either a code or a status
                    For l = 1 To UBound(Codes)
                        If (Codes(l).Code = strCode Or strCode = "*") And Codes(l).Code <> strExcept Then
                            Codes(l).Status = "K"
                            CodesBeg(l).Status = "K"
                        End If
                        If strCode = "*" And Codes(l).Code = strExcept Then Exit For
                    Next l
                Else
                    For l = 1 To UBound(Codes)
                        If (Codes(l).Code = strCode Or strCode = "*") And Codes(l).Status <> strExcept Then
                            Codes(l).Status = "K"
                            CodesBeg(l).Status = "K"
                        End If
                    Next l
                End If
            End If
        End Function

        Private Function GetAllCodesTill(ByRef strTill As String) As String
            'get all codes except strTill
            Dim strTmp As String
            Dim l As Long

            strTmp = ""
            If UBound(Codes) > 0 Then
                For l = UBound(Codes) To 1 Step -1
                    If Codes(l).Code = strTill Then
                        Exit For
                    Else
                        If Not InNextBeg(CodesBeg(l).Code) And Codes(l).Status <> "D" Then
                            strTmp = strTmp & Codes(l).Code
                            Codes(l).Status = "K"
                            CodesBeg(l).Status = "K"
                        End If
                    End If
                Next l
            End If
            GetAllCodesTill = strTmp
        End Function
        Private Function GetAllCodesBeg() As String
            Dim strTmp As String
            Dim l As Long

            strTmp = ""
            If UBound(CodesBeg) > 0 Then
                For l = UBound(CodesBeg) To 1 Step -1
                    If CodesBeg(l).Status = "P" Then
                        strTmp = strTmp & CodesBeg(l).Code
                        CodesBeg(l).Status = "A"
                        Codes(l).Status = "A"
                    ElseIf CodesBeg(l).Status = "PG" Then
                        strTmp = strTmp & CodesBeg(l).Code
                        CodesBeg(l).Status = "G"
                        Codes(l).Status = "G"
                    End If
                Next l
            End If
            GetAllCodesBeg = strTmp
        End Function
        Private Function GetAllCodesBegTill(ByRef strTill As String) As String
            'get all codes except strTill - stop if strTill reached
            '"<table"
            Dim strTmp As String
            Dim l As Long

            strTmp = ""
            If UBound(CodesBeg) > 0 Then
                For l = 1 To UBound(CodesBeg)
                    If Codes(l).Code = strTill Then
                        Exit For
                    Else
                        If CodesBeg(l).Status = "P" Then
                            strTmp = strTmp & CodesBeg(l).Code
                            Codes(l).Status = "A"
                            CodesBeg(l).Status = "A"
                        ElseIf CodesBeg(l).Status = "PG" Then
                            strTmp = strTmp & CodesBeg(l).Code
                            Codes(l).Status = "G"
                            CodesBeg(l).Status = "G"
                        End If
                    End If
                Next l
            End If
            GetAllCodesBegTill = strTmp
        End Function

        Private Sub ShiftNext()
            'get 1st code off list and shorten list
            Dim l As Long

            If UBound(NextCodes) > 0 Then
                ''ShiftNext = NextCodes(1)
                For l = 1 To UBound(NextCodes) - 1
                    NextCodes(l) = NextCodes(l + 1)
                Next l
                ReDim Preserve NextCodes(UBound(NextCodes) - 1)
            End If
        End Sub

        Private Sub ShiftNextBeg()
            'get 1st code off list and shorten list
            Dim l As Long

            If UBound(NextCodesBeg) > 0 Then
                ''ShiftNextBeg = NextCodesBeg(1)
                For l = 1 To UBound(NextCodesBeg) - 1
                    NextCodesBeg(l) = NextCodesBeg(l + 1)
                Next l
                ReDim Preserve NextCodesBeg(UBound(NextCodesBeg) - 1)
            End If
        End Sub

        Private Sub ProcessWord(ByRef strWord As String)
            Dim strTmp As String
            Dim l As Long
            Dim lRetVal As Long

            Dim strTableAlign As String    'current table alignment for setting up tablestring
            Dim strTableWidth As String    'current table width for setting up tablestring

            If lSkipWords > 0 Then
                lSkipWords = lSkipWords - 1
                Exit Sub
            End If
            If (Left(strWord, 1) = "\" Or Left(strWord, 1) = "{" Or Left(strWord, 1) = "}") _
               And (Left(strWord, 2) <> "\\" And Left(strWord, 2) <> "\{" And Left(strWord, 2) <> "\}") Then
                strWord = Trim(strWord)
                Select Case Left(strWord, 2)
                    Case "}"
                        If lBrLev = 0 Then
                            lRetVal = KillCode("*", "G")
                            ClearNext("</li>")
                            ClearFont()
                        End If
                    Case "\'"    'special characters
                        strTmp = HTMLCode(Mid(strWord, 3))
                        If Left(strTmp, 6) = "<rtf>:" Then
                            strSecTmp = Mid(strTmp, 7) & " " & strSecTmp
                        Else
                            strSecTmp = strTmp & strSecTmp
                        End If
                    Case "\b"    'bold
                        If strWord = "\b" Then
                            If InCodes("</b>", True) Then
                                '                    Codes2NextTill ("</b>")
                            Else
                                PushNext("</b>")
                                PushNextBeg("<b>")
                            End If
                        ElseIf strWord = "\bullet" Then
                            'If Not (Codes(UBound(Codes)).Code = "</li>" And Codes(UBound(Codes)).Status = "A") Then
                            PushNext("</li>")
                            PushNextBeg("<li>")
                            'End If
                        ElseIf strWord = "\b0" Then    'bold off
                            If InCodes("</b>") Then
                                Codes2NextTill("</b>")
                                KillCode("</b>")
                            End If
                            If InNext("</b>") Then
                                RemoveFromNext("</b>")
                            End If
                        End If
                    Case "\c"
                        If strWord = "\cf0" Then    'color font off
                            strFontColor = ""
                            strFont = ParseFont(strFontColor, strFontSize, strFace)
                        ElseIf Left(strWord, 3) = "\cf" And IsNumeric(Mid(strWord, 4)) Then  'color font
                            'get color code
                            l = Val(Mid(strWord, 4))
                            If l <= UBound(strColorTable) And l > 0 Then
                                strFontColor = "#" & strColorTable(l)
                            End If

                            'insert color
                            If strFontColor <> "#" Then
                                strFont = ParseFont(strFontColor, strFontSize, strFace)
                                If InNext("</font>") Then
                                    ReplaceInNextBeg("</font>", strFont)
                                ElseIf InCodes("</font>") Then
                                    PushNext("</font>")
                                    PushNextBeg(strFont)
                                    Codes2NextTill("</font>")
                                    KillCode("</font>")
                                Else
                                    PushNext("</font>")
                                    PushNextBeg(strFont)
                                End If
                            End If
                        End If
                    Case "\f"
                        If Left(strWord, 3) = "\fs" And IsNumeric(Mid(strWord, 4)) Then  'font size
                            l = Val(Mid(strWord, 4))
                            lFontSize = Int((l / 7) - 0)    'calc to convert RTF to HTML sizes
                            If lFontSize > 8 Then lFontSize = 8
                            If lFontSize < 1 Then lFontSize = 1
                            strFontSize = Trim(lFontSize)
                            If Val(strFontSize) = iDefFontSize Then strFontSize = ""
                            'insert size
                            strFont = ParseFont(strFontColor, strFontSize, strFace)
                        ElseIf Left(strWord, 2) = "\f" And IsNumeric(Mid(strWord, 3)) And gUseFontFace Then  'font type
                            strFace = strFontTable(Val(Mid(strWord, 3)))
                            strFont = ParseFont(strFontColor, strFontSize, strFace)
                        End If
                    Case "\i"
                        If strWord = "\i" Then 'italics
                            If InCodes("</i>", True) Then
                                '                    Codes2NextTill ("</i>")
                            Else
                                PushNext("</i>")
                                PushNextBeg("<i>")
                            End If
                        ElseIf strWord = "\i0" Then 'italics off
                            If InCodes("</i>") Then
                                Codes2NextTill("</i>")
                                KillCode("</i>")
                            End If
                            If InNext("</i>") Then
                                RemoveFromNext("</i>")
                            End If
                        End If
                    Case "\l"
                    'If strWord = "\listname" Then
                    '    lSkipWords = 1
                    'End If
                    Case "\n"
                        If strWord = "\nosupersub" Then    'superscript/subscript off
                            If InCodes("</sub>", True) Then
                                Codes2NextTill("</sub>")
                                KillCode("</sub>")
                            End If
                            If InNext("</sub>") Then
                                RemoveFromNext("</sub>")
                            End If
                            If InCodes("</sup>", True) Then
                                Codes2NextTill("</sup>")
                                KillCode("</sup>")
                            End If
                            If InNext("</sup>") Then
                                RemoveFromNext("</sup>")
                            End If
                        End If
                    Case "\p"
                        If strWord = "\par" Then
                            If Not (InCodes("</ul>") Or InCodes("</li>")) Then
                                strBeforeText2 = strBeforeText2 & strEOL & "<br>" & strCR
                            Else
                                lRetVal = KillCode("</li>")
                                RemoveFromNext("</li>")
                            End If
                            gBOL = True
                            gPar = True
                            'If InCodes("</ul>") Then
                            '    PushNext ("</li>")
                            '    PushNextBeg ("<li>")
                            'End If
                        ElseIf strWord = "\pard" Then
                            For l = 1 To UBound(CodesBeg)
                                If Codes(l).Status = "G" Or Codes(l).Status = "PG" Then
                                    Codes(l).Status = "K"
                                    CodesBeg(l).Status = "K"
                                End If
                            Next l
                            If Not gIgnorePard Then
                                If InCodes("</li>") Then
                                    lRetVal = KillCode("</li>")
                                    RemoveFromNext("</li>")
                                End If
                            End If
                            gPar = True
                        ElseIf strWord = "\plain" Then
                            lRetVal = KillCode("*", "G")
                            ClearFont()
                        ElseIf strWord = "\pnlvlblt" Then 'bulleted list
                            If Not InNext("</li>") Then
                                PushNext("</li>")
                                PushNextBeg("<li>")
                            End If
                            'PushNext ("</ul>")
                            'PushNextBeg ("<ul>")
                        ElseIf strWord = "\pntxta" Then 'numbered list?
                            lSkipWords = 1
                        ElseIf strWord = "\pntxtb" Then 'numbered list?
                            lSkipWords = 1
                        ElseIf strWord = "\pntext" Then 'bullet
                            If Not InNext("</li>") Then
                                PushNext("</li>")
                                PushNextBeg("<li>")
                                Codes2NextTill("</table>")
                                KillCode("*")
                            End If
                        End If
                    Case "\q"
                        If strWord = "\qc" Then    'centered
                            strTableAlign = "center"
                            strTableWidth = "100%"
                            If InNext("</td></tr></table>") Then
                                '?
                            Else
                                strTable = "<table width=" & strTableWidth & "><tr><td align=""" & strTableAlign & """>"
                            End If
                            If InNext("</td></tr></table>") Then
                                ReplaceInNextBeg("</td></tr></table>", strTable)
                            ElseIf InCodes("</td></tr></table>") Then
                                PushNext("</td></tr></table>")
                                PushNextBeg(strTable)
                                Codes2NextTill("</td></tr></table>")
                            Else
                                PushNext("</td></tr></table>")
                                PushNextBeg(strTable)
                            End If
                        ElseIf strWord = "\qr" Then    'right justified
                            strTableAlign = "right"
                            strTableWidth = "100%"
                            If InNext("</td></tr></table>") Then
                                '?
                            Else
                                strTable = "<table width=" & strTableWidth & "><tr><td align=""" & strTableAlign & """>"
                            End If
                            If InNext("</td></tr></table>") Then
                                ReplaceInNextBeg("</td></tr></table>", strTable)
                            ElseIf InCodes("</td></tr></table>") Then
                                PushNext("</td></tr></table>")
                                PushNextBeg(strTable)
                                Codes2NextTill("</td></tr></table>")
                            Else
                                PushNext("</td></tr></table>")
                                PushNextBeg(strTable)
                            End If
                        End If
                    Case "\s"
                        If strWord = "\strike" Then    'strike text
                            If Codes(UBound(Codes)).Code <> "</s>" Or (Codes(UBound(Codes)).Code = "</s>" And CodesBeg(UBound(Codes)).Code = "") Then
                                PushNext("</s>")
                                PushNextBeg("<s>")
                            End If
                        ElseIf strWord = "\strike0" Then    'strike off
                            If InCodes("</s>") Then
                                Codes2NextTill("</s>")
                                KillCode("</s>")
                            End If
                            If InNext("</s>") Then
                                RemoveFromNext("</s>")
                            End If
                        ElseIf strWord = "\super" Then    'superscript
                            If Codes(UBound(Codes)).Code <> "</sup>" Or (Codes(UBound(Codes)).Code = "</sup>" And CodesBeg(UBound(Codes)).Code = "") Then
                                PushNext("</sup>")
                                PushNextBeg("<sup>")
                            End If
                        ElseIf strWord = "\sub" Then    'subscript
                            If Codes(UBound(Codes)).Code <> "</sub>" Or (Codes(UBound(Codes)).Code = "</sub>" And CodesBeg(UBound(Codes)).Code = "") Then
                                PushNext("</sub>")
                                PushNextBeg("<sub>")
                            End If
                        End If

                    'If strWord = "\snext0" Then    'style
                    '    lSkipWords = 1
                    'End If
                    Case "\t"
                        If strWord = "\tab" Then    'tab
                            strSecTmp = vbTab & strSecTmp
                        End If
                    Case "\u"
                        If strWord = "\ul" Then    'underline
                            If InCodes("</u>", True) Then
                                '                    Codes2NextTill ("</u>")
                            Else
                                PushNext("</u>")
                                PushNextBeg("<u>")
                            End If
                        ElseIf strWord = "\ulnone" Then    'stop underline
                            If InCodes("</u>") Then
                                Codes2NextTill("</u>")
                                KillCode("</u>")
                            End If
                            If InNext("</u>") Then
                                RemoveFromNext("</u>")
                            End If
                        End If
                End Select
            Else
                If Len(strWord) > 0 Then
                    If strWord = "\\" Or strWord = "\{" Or strWord = "\}" Then strWord = Right(strWord, 1)
                    If Trim(strWord) = "" Then
                        If gBOL Then strWord = rtf2html_replace(strWord, " ", "&nbsp;")
                        strCurPhrase = strCurPhrase & strBeforeText3 & strWord
                    Else
                        'regular text
                        If gPar Then
                            strBeforeText = strBeforeText & ProcessAfterTextCodes()
                            Next2Codes()
                            strBeforeText3 = GetAllCodesBeg()
                            gPar = False
                        Else
                            strBeforeText = strBeforeText & ProcessAfterTextCodes()
                            Next2Codes()
                            strBeforeText3 = GetAllCodesBegTill("</td></tr></table>")
                        End If
                        RemoveBlanks()

                        strCurPhrase = strCurPhrase & strBeforeText
                        strBeforeText = ""
                        strCurPhrase = strCurPhrase & strBeforeText2
                        strBeforeText2 = ""
                        strCurPhrase = strCurPhrase & strBeforeText3 & strWord
                        strBeforeText3 = ""
                        gBOL = False
                    End If
                End If
            End If
        End Sub

        Private Sub PushNext(ByRef strCode As String)
            If Len(strCode) > 0 Then
                ReDim Preserve NextCodes(UBound(NextCodes) + 1)
                NextCodes(UBound(NextCodes)) = strCode
            End If
        End Sub

        Private Sub UnShiftNext(ByRef strCode As String)
            'stick strCode on front of list and move everything over to make room
            Dim l As Long

            If Len(strCode) > 0 Then
                ReDim Preserve NextCodes(UBound(NextCodes) + 1)
                If UBound(NextCodes) > 1 Then
                    For l = UBound(NextCodes) To 1 Step -1
                        NextCodes(l) = NextCodes(l - 1)
                    Next l
                End If
                NextCodes(1) = strCode
            End If
        End Sub

        Private Sub UnShiftNextBeg(ByRef strCode As String)
            Dim l As Long

            If Len(strCode) > 0 Then
                ReDim Preserve NextCodesBeg(UBound(NextCodesBeg) + 1)
                If UBound(NextCodesBeg) > 1 Then
                    For l = UBound(NextCodesBeg) To 1 Step -1
                        NextCodesBeg(l) = NextCodesBeg(l - 1)
                    Next l
                End If
                NextCodesBeg(1) = strCode
            End If
        End Sub

        Private Sub PushNextBeg(ByRef strCode As String)
            ReDim Preserve NextCodesBeg(UBound(NextCodesBeg) + 1)
            NextCodesBeg(UBound(NextCodesBeg)) = strCode
        End Sub

        Private Sub RemoveBlanks()
            Dim l As Long
            Dim lOffSet As Long

            l = 1
            lOffSet = 0
            While l <= UBound(CodesBeg) And l + lOffSet <= UBound(CodesBeg)
                If CodesBeg(l).Status = "K" Or CodesBeg(l).Status = "" Then     'And Not (Codes(l) = "</font>" And Len(strFont) > 0) Then
                    lOffSet = lOffSet + 1
                Else
                    l = l + 1
                End If
                If l + lOffSet <= UBound(CodesBeg) Then
                    Codes(l) = Codes(l + lOffSet)
                    CodesBeg(l) = CodesBeg(l + lOffSet)
                End If
            End While
            If lOffSet > 0 Then
                ReDim Preserve Codes(UBound(Codes) - lOffSet)
                ReDim Preserve CodesBeg(UBound(CodesBeg) - lOffSet)
            End If
        End Sub

        Private Sub RemoveFromNext(ByRef strRem As String)
            Dim l As Long
            Dim m As Long

            If UBound(NextCodes) < 1 Then Exit Sub
            l = 1
            While l < UBound(NextCodes)
                If NextCodes(l) = strRem Then
                    For m = l To UBound(NextCodes) - 1
                        NextCodes(m) = NextCodes(m + 1)
                        NextCodesBeg(m) = NextCodesBeg(m + 1)
                    Next m
                    l = m
                Else
                    l = l + 1
                End If
            End While
            ReDim Preserve NextCodes(UBound(NextCodes) - 1)
            ReDim Preserve NextCodesBeg(UBound(NextCodesBeg) - 1)
        End Sub

        Private Function rtf2html_replace(ByRef strIn As String, ByRef strRepl As String, ByRef strWith As String) As String
            'replace all instances of strRepl in strIn with strWith
            Dim i As Integer

            If ((Len(strRepl) = 0) Or (Len(strIn) = 0)) Then
                rtf2html_replace = strIn
                Exit Function
            End If
            i = InStr(1, strIn, strRepl)
            While i <> 0
                strIn = Left(strIn, i - 1) & strWith & Mid(strIn, i + Len(strRepl))
                i = InStr(i + Len(strWith), strIn, strRepl)
            End While
            rtf2html_replace = strIn
        End Function

        Private Sub ReviveCode(ByRef strCode As String)
            Dim l As Long

            For l = 1 To UBound(Codes)
                If Codes(l).Code = strCode Then
                    Codes(l).Status = "A"
                    CodesBeg(l).Status = "A"
                End If
            Next l
        End Sub

        Private Function ReplaceInNextBeg(ByRef strCode As String, ByRef strWith As String) As Long
            Dim l As Long
            Dim lCount As Long    'number of codes replaced

            lCount = 0
            For l = 1 To UBound(NextCodes)
                If NextCodes(l) = strCode Then
                    NextCodesBeg(l) = strWith
                    lCount = lCount + 1
                End If
            Next l
            ReplaceInNextBeg = lCount
        End Function

        Private Sub ReplaceInCodesBeg(ByRef strCode As String, ByRef strWith As String)
            Dim l As Long

            l = 1
            While l <= UBound(Codes) And Codes(l).Code <> strCode
                l = l + 1
            End While
            If Codes(l).Code = strCode Then
                If CodesBeg(l).Code <> strWith Then
                    CodesBeg(l).Code = strWith
                    Codes(l).Status = "P"
                    CodesBeg(l).Status = "P"
                Else
                    Codes(l).Status = "P"
                    CodesBeg(l).Status = "P"
                End If
            End If
        End Sub

        Private Function IsDig(ByRef strChar As String) As Boolean
            If Len(strChar) = 0 Then
                IsDig = False
            Else
                IsDig = InStr(1, "1234567890", strChar)
            End If
        End Function

        Private Function GetCodes(ByRef strWordTmp As String) As String
            Dim strTmp As String
            Dim l As Long

            strTmp = "CurWord: "
            If Len(strWordTmp) > 20 Then
                strTmp = strTmp & Left(strWordTmp, 20) & "..."
            Else
                strTmp = strTmp & strWordTmp
            End If
            strTmp = strTmp & vbCrLf & vbCrLf & "BegCodes: "
            For l = 1 To UBound(CodesBeg)
                strTmp = strTmp & CodesBeg(l).Code & " (" & CodesBeg(l).Status & "), "
            Next l
            strTmp = strTmp & vbCrLf & "Codes: "
            For l = 1 To UBound(Codes)
                strTmp = strTmp & Codes(l).Code & " (" & Codes(l).Status & "), "
            Next l
            strTmp = strTmp & vbCrLf & vbCrLf & "NextBegCodes: "
            For l = 1 To UBound(NextCodesBeg)
                strTmp = strTmp & NextCodesBeg(l) & ", "
            Next l
            strTmp = strTmp & vbCrLf & "NextCodes: "
            For l = 1 To UBound(NextCodes)
                strTmp = strTmp & NextCodes(l) & ", "
            Next l
            strTmp = strTmp & vbCrLf & vbCrLf & "Font String: " & strFont
            strTmp = strTmp & vbCrLf & vbCrLf & "Before Text: " & strBeforeText2
            GetCodes = strTmp
        End Function

        Private Function TrimAll(ByRef strTmp As String) As String
            Dim l As Long

            strTmp = strTmp.Trim
            l = Len(strTmp) + 1
            While l <> Len(strTmp)
                l = Len(strTmp)
                If Right(strTmp, 1) = vbCrLf Then strTmp = Left(strTmp, Len(strTmp) - 1)
                If Left(strTmp, 1) = vbCrLf Then strTmp = Right(strTmp, Len(strTmp) - 1)
                If Right(strTmp, 1) = vbCr Then strTmp = Left(strTmp, Len(strTmp) - 1)
                If Left(strTmp, 1) = vbCr Then strTmp = Right(strTmp, Len(strTmp) - 1)
                If Right(strTmp, 1) = vbLf Then strTmp = Left(strTmp, Len(strTmp) - 1)
                If Left(strTmp, 1) = vbLf Then strTmp = Right(strTmp, Len(strTmp) - 1)
            End While
            TrimAll = strTmp
        End Function

        Private Function HTMLCode(ByRef strRTFCode As String) As String
            'given rtf code return html code
            Select Case strRTFCode
                Case "00"
                    HTMLCode = "&nbsp;"
                Case "a9"
                    HTMLCode = "&copy;"
                Case "b4"
                    HTMLCode = "&acute;"
                Case "ab"
                    HTMLCode = "&laquo;"
                Case "bb"
                    HTMLCode = "&raquo;"
                Case "a1"
                    HTMLCode = "&iexcl;"
                Case "bf"
                    HTMLCode = "&iquest;"
                Case "c0"
                    HTMLCode = "&Agrave;"
                Case "e0"
                    HTMLCode = "&agrave;"
                Case "c1"
                    HTMLCode = "&Aacute;"
                Case "e1"
                    HTMLCode = "&aacute;"    'á
                Case "c2"
                    HTMLCode = "&Acirc;"
                Case "e2"
                    HTMLCode = "&acirc;"
                Case "c3"
                    HTMLCode = "&Atilde;"
                Case "e3"
                    HTMLCode = "&atilde;"
                Case "c4"
                    HTMLCode = "&Auml;"
                Case "e4", "99"
                    HTMLCode = "<rtf>:\super TM\nosupersub"
                Case "c5"
                    HTMLCode = "&Aring;"
                Case "e5"
                    HTMLCode = "&aring;"
                Case "c6"
                    HTMLCode = "&AElig;"
                Case "e6"
                    HTMLCode = "&aelig;"
                Case "c7"
                    HTMLCode = "&Ccedil;"
                Case "e7"
                    HTMLCode = "&ccedil;"
                Case "d0"
                    HTMLCode = "&ETH;"
                Case "f0"
                    HTMLCode = "&eth;"
                Case "c8"
                    HTMLCode = "&Egrave;"
                Case "e8"
                    HTMLCode = "&egrave;"
                Case "c9"
                    HTMLCode = "&Eacute;"
                Case "e9"
                    HTMLCode = "&eacute;"
                Case "ca"
                    HTMLCode = "&Ecirc;"
                Case "ea"
                    HTMLCode = "&ecirc;"
                Case "cb"
                    HTMLCode = "&Euml;"
                Case "eb"
                    HTMLCode = "&euml;"
                Case "cc"
                    HTMLCode = "&Igrave;"
                Case "ec"
                    HTMLCode = "&igrave;"
                Case "cd"
                    HTMLCode = "&Iacute;"
                Case "ed"
                    HTMLCode = "&iacute;"    'í
                Case "ce"
                    HTMLCode = "&Icirc;"
                Case "ee"
                    HTMLCode = "&icirc;"
                Case "cf"
                    HTMLCode = "&Iuml;"
                Case "ef"
                    HTMLCode = "&iuml;"
                Case "d1"
                    HTMLCode = "&Ntilde;"
                Case "f1"
                    HTMLCode = "&ntilde;"
                Case "d2"
                    HTMLCode = "&Ograve;"
                Case "f2"
                    HTMLCode = "&ograve;"
                Case "d3"
                    HTMLCode = "&Oacute;"
                Case "f3"
                    HTMLCode = "&oacute;"
                Case "d4"
                    HTMLCode = "&Ocirc;"
                Case "f4"
                    HTMLCode = "&ocirc;"
                Case "d5"
                    HTMLCode = "&Otilde;"
                Case "f5"
                    HTMLCode = "&otilde;"
                Case "d6"
                    HTMLCode = "&Ouml;"
                Case "f6"
                    HTMLCode = "&ouml;"
                Case "d8"
                    HTMLCode = "&Oslash;"
                Case "f8"
                    HTMLCode = "&oslash;"
                Case "d9"
                    HTMLCode = "&Ugrave;"
                Case "f9"
                    HTMLCode = "&ugrave;"
                Case "da"
                    HTMLCode = "&Uacute;"
                Case "fa"
                    HTMLCode = "&uacute;"
                Case "db"
                    HTMLCode = "&Ucirc;"
                Case "fb"
                    HTMLCode = "&ucirc;"
                Case "dc"
                    HTMLCode = "&Uuml;"
                Case "fc"
                    HTMLCode = "&uuml;"
                Case "dd"
                    HTMLCode = "&Yacute;"
                Case "fd"
                    HTMLCode = "&yacute;"
                Case "ff"
                    HTMLCode = "&yuml;"
                Case "de"
                    HTMLCode = "&THORN;"
                Case "fe"
                    HTMLCode = "&thorn;"
                Case "df"
                    HTMLCode = "&szlig;"
                Case "a7"
                    HTMLCode = "&sect;"
                Case "b6"
                    HTMLCode = "&para;"
                Case "b5"
                    HTMLCode = "&micro;"
                Case "a6"
                    HTMLCode = "&brvbar;"
                Case "b1"
                    HTMLCode = "&plusmn;"
                Case "b7"
                    HTMLCode = "&middot;"
                Case "a8"
                    HTMLCode = "&uml;"
                Case "b8"
                    HTMLCode = "&cedil;"
                Case "aa"
                    HTMLCode = "&ordf;"
                Case "ba"
                    HTMLCode = "&ordm;"
                Case "ac"
                    HTMLCode = "&not;"
                Case "ad"
                    HTMLCode = "&shy;"
                Case "af"
                    HTMLCode = "&macr;"
                Case "b0"
                    HTMLCode = "&deg;"
                Case "b9"
                    HTMLCode = "&sup1;"
                Case "b2"
                    HTMLCode = "&sup2;"
                Case "b3"
                    HTMLCode = "&sup3;"
                Case "bc"
                    HTMLCode = "&frac14;"
                Case "bd"
                    HTMLCode = "&frac12;"
                Case "be"
                    HTMLCode = "&frac34;"
                Case "d7"
                    HTMLCode = "&times;"
                Case "f7"
                    HTMLCode = "&divide;"
                Case "a2"
                    HTMLCode = "&cent;"
                Case "a3"
                    HTMLCode = "&pound;"
                Case "a4"
                    HTMLCode = "&curren;"
                Case "a5"
                    HTMLCode = "&yen;"
                Case "85"
                    HTMLCode = "..."
                Case "9e"
                    HTMLCode = "ž"    'ž
                Case "9a"
                    HTMLCode = "š"    'š
                Case Else
                    HTMLCode = ""
            End Select
        End Function

        Private Function TrimifCmd(ByRef strTmp As String) As String
            Dim l As Long

            l = 1
            While Mid(strTmp, l, 1) = " "
                l = l + 1
            End While
            If Mid(strTmp, l, 1) = "\" Or Mid(strTmp, l, 1) = "{" Then
                strTmp = Trim(strTmp)
            Else
                If Left(strTmp, 1) = " " Then strTmp = Mid(strTmp, 2)
                strTmp = RTrim(strTmp)
            End If
            TrimifCmd = strTmp
        End Function

        Public Function Convert_(ByVal strRTF As String, ByRef strHTML As String, Optional ByRef strOptions As String = "") As Boolean
            'Options:
            '+H              add an HTML header and footer
            '+G              add a generator Metatag
            '+T="MyTitle"    add a title (only works if +H is used)
            '+CR             add a carraige return after all <br>s
            '+I              keep html codes intact
            '+F=X            default font size (blanks out any changes to this size - saves on space)
            '-FF             ignore font faces

            Dim strRTFTmp As String
            Dim l As Long
            Dim lBOS As Integer              'beginning of section
            Dim strTmp As String
            Dim gHTML As Boolean             'true if html codes should be left intact
            Dim strWordTmp As String         'temporary word buffer
            Dim strEndText As String         'ending text
            Dim strLastWord As String        'previous "word"
            Dim tmp As String = String.Empty

            Convert_ = False
            ClearCodes()
            strHTML = ""
            gPlain = False
            gBOL = True
            gPar = False
            strCurPhrase = ""
            strWordTmp = ""
            strEndText = ""

            '_Debug.Print_(strRTF)

            Try

                'setup +CR option
                If InStr(1, strOptions, "+CR") <> 0 Then strCR = vbCrLf Else strCR = ""
                'setup +HTML option
                If InStr(1, strOptions, "+I") <> 0 Then gHTML = True Else gHTML = False
                'setup default font size option
                If InStr(1, strOptions, "+F=") <> 0 Then
                    l = InStr(1, strOptions, "+F=") + 3
                    strTmp = Mid(strOptions, l, 1)
                    iDefFontSize = 0
                    While IsDig(strTmp)
                        iDefFontSize = iDefFontSize * 10 + Val(strTmp)
                        l = l + 1
                        strTmp = Mid(strOptions, l, 1)
                    End While
                End If
                'setup to use different fonts or not
                If InStr(1, strOptions, "-FF") <> 0 Then gUseFontFace = False Else gUseFontFace = True

                strRTFTmp = TrimAll(strRTF)

                If Left(strRTFTmp, 1) = "{" And Right(strRTFTmp, 1) = "}" Then strRTFTmp = Mid(strRTFTmp, 2, Len(strRTFTmp) - 2)
                '_Debug.Print_(strRTFTmp)

                'setup list (bullets) status
                If InStr(1, strRTFTmp, "\list\") <> 0 Then
                    'I'm not sure if this is in any way correct but it seems to work for me
                    'sometimes \pard ends a list item sometimes it doesn't
                    gIgnorePard = True
                Else
                    gIgnorePard = False
                End If

                'setup color table
                lBOS = InStr(1, strRTFTmp, "\colortbl")
                If lBOS > 0 Then
                    strSecTmp = NabSection(strRTFTmp, lBOS)
                    GetColorTable(strSecTmp, strColorTable)
                End If

                'setup font table
                lBOS = InStr(1, strRTFTmp, "\fonttbl")
                If lBOS > 0 Then
                    strSecTmp = NabSection(strRTFTmp, lBOS)
                    GetFontTable(strSecTmp, strFontTable)
                End If

                'setup stylesheets
                lBOS = InStr(1, strRTFTmp, "\stylesheet")
                If lBOS > 0 Then
                    strSecTmp = NabSection(strRTFTmp, lBOS)
                    'ignore stylesheets for now
                End If

                'setup info
                lBOS = InStr(1, strRTFTmp, "\info")
                If lBOS > 0 Then
                    strSecTmp = NabSection(strRTFTmp, lBOS)
                    'ignore info for now
                End If

                'list table
                lBOS = InStr(1, strRTFTmp, "\listtable")
                If lBOS > 0 Then
                    strSecTmp = NabSection(strRTFTmp, lBOS)
                    'ignore info for now
                End If

                'list override table
                lBOS = InStr(1, strRTFTmp, "\listoverridetable")
                If lBOS > 0 Then
                    strSecTmp = NabSection(strRTFTmp, lBOS)
                    'ignore info for now
                End If

                lBrLev = 0
                strLastWord = ""
                While Len(strRTFTmp) > 0
                    strSecTmp = NabNextLine(strRTFTmp)
                    While Len(strSecTmp) > 0
                        strLastWord = strWordTmp
                        strWordTmp = NabNextWord(strSecTmp)
                        If lBrLev > 0 Then
                            If strWordTmp = "{" Then
                                lBrLev = lBrLev + 1
                            ElseIf strWordTmp = "}" Then
                                lBrLev = lBrLev - 1
                            End If
                            strWordTmp = ""
                        ElseIf strWordTmp = "\*" Or strWordTmp = "\pict" Then
                            'skip \pnlvlbt stuff
                            lBrLev = 1
                            strWordTmp = ""
                        ElseIf strWordTmp = "\pntext" Then
                            'get bullet codes but skip rest for now
                            lBrLev = 1
                        End If
                        If Len(strWordTmp) > 0 Then
                            'If gDebug Then ShowCodes (strWordTmp)  'for debugging only
                            If Len(strWordTmp) > 0 Then ProcessWord(strWordTmp)
                        End If
                    End While
                End While

                'get any remaining codes in stack
                strEndText = strEndText & GetActiveCodes()
                strBeforeText2 = rtf2html_replace(strBeforeText2, "<br>", "")
                strBeforeText2 = rtf2html_replace(strBeforeText2, vbCrLf, "")
                strCurPhrase = strCurPhrase & strBeforeText & strBeforeText2 & strEndText
                strBeforeText = ""
                strBeforeText2 = ""
                strBeforeText3 = ""
                strHTML = strHTML & strCurPhrase
                strCurPhrase = ""
                ClearFont()

                Convert_ = True

                '_Debug.Print_(strHTML)
                '_Debug.Print_(strRTF)
                If hyperlink2List(strRTF, Hyperlinks) Then
                    For i As Short = 0 To Hyperlinks.Count - 1
                        '    _Debug.Print_(i.ToString & ". " & Hyperlinks(i).TXT_Hyperlink)
                        '    _Debug.Print_(i.ToString & ". " & Hyperlinks(i).URL_Hyperlink)
                        '    _Debug.Print_(i.ToString & ". " & Hyperlinks(i).RTF_Hyperlink)
                        '    _Debug.Print_(i.ToString & ". " & Hyperlinks(i).HTML_Hyperlink)
                        ' replace RTF to HTML hyperlink in the html body:
                        strHTML = strHTML.Replace(Hyperlinks(i).RTF_Hyperlink, Hyperlinks(i).HTML_Hyperlink)
                        '_Debug.Print_(strHTML)
                    Next
                End If

            Catch ex As Exception
                'MsgBox(ex.ToString, MsgBoxStyle.Critical)
                _Debug.PrintError_(ex.ToString)
            End Try

        End Function
        Public Hyperlinks As New List(Of Hyperlink)
        Public Class Hyperlink
            Private m_rtfHyperlink As String
            Private m_htmlHyperlink As String
            Private m_txtHyperlink As String
            Private m_urlHyperlink As String

            Public Property RTF_Hyperlink() As String
                Get
                    Return m_rtfHyperlink
                End Get
                Set(ByVal value As String)
                    m_rtfHyperlink = value
                End Set
            End Property
            Public Property HTML_Hyperlink() As String
                Get
                    Return m_htmlHyperlink
                End Get
                Set(ByVal value As String)
                    m_htmlHyperlink = value
                End Set
            End Property
            Public Property TXT_Hyperlink() As String
                Get
                    Return m_txtHyperlink
                End Get
                Set(ByVal value As String)
                    m_txtHyperlink = value
                End Set
            End Property
            Public Property URL_Hyperlink() As String
                Get
                    Return m_urlHyperlink
                End Get
                Set(ByVal value As String)
                    m_urlHyperlink = value
                End Set
            End Property

        End Class
        Private Function hyperlink2HTML(ByVal rtf As String, ByRef pos2Start As Integer, ByRef lstHyperLink As Hyperlink) As Boolean
            hyperlink2HTML = False ' assume.
            Dim pos As Integer = 0
            Dim ul As Integer = 0 ' open and closing \ul and \ulnone underline rtf tags
            Dim ulnone As Integer = 0
            Dim posOpenBracket As Integer = 0
            Dim posCloseBracket As Integer = 0
            Dim posStartText As Integer = 0
            Dim posEndText As Integer = 0
            ''
            ' In RTF text look for open and closing \ul ... \ulnone underline rtf tags
            ul = rtf.IndexOf("\ul", pos2Start)
            If ul > pos2Start Then
                ulnone = rtf.IndexOf("\ulnone", ul)
                If ulnone > ul Then
                    '_Debug.Print_(rtf.Substring(ul, ulnone - ul))
                    ' If found, check if there is a hyperlink: 
                    ' look for open and close bracket < ... > If found Then we have hyperlink.
                    posOpenBracket = rtf.IndexOf("<", ul)
                    If posOpenBracket > ul Then
                        posCloseBracket = rtf.IndexOf(">", posOpenBracket)
                        If posCloseBracket > posOpenBracket Then
                            '_Debug.Print_(rtf.Substring(posOpenBracket, (posCloseBracket - posOpenBracket) + 1))
                            ' Find the next space, after \ul: 
                            ' position# + 1 will be the beginning of the Text
                            posStartText = rtf.IndexOf(" ", ul) + 1
                            If posStartText > ul And posStartText < posOpenBracket Then
                                ' Find the opening < bracket of the hyperlink: 
                                ' position# - 1 will be the end of the Text
                                posEndText = posOpenBracket - 1
                                ' Store the plain Text: 
                                lstHyperLink.TXT_Hyperlink = rtf.Substring(posStartText, posEndText - posStartText)
                                '_Debug.Print_(lstHyperLink.TXT_Hyperlink)
                                If posEndText > posStartText Then
                                    lstHyperLink.RTF_Hyperlink = rtf.Substring(posStartText, (posCloseBracket - posStartText) + 1)
                                    '_Debug.Print_(lstHyperLink.RTF_Hyperlink)
                                    If lstHyperLink.RTF_Hyperlink.Length > 0 Then
                                        ' Cut the hyperlink without <> brackets
                                        lstHyperLink.HTML_Hyperlink = rtf.Substring(posOpenBracket + 1, (posCloseBracket - posOpenBracket) - 1)
                                        '_Debug.Print_(lstHyperLink.HTML_Hyperlink)
                                        ' Store the URL with <> brackets:
                                        lstHyperLink.URL_Hyperlink = rtf.Substring(posOpenBracket, (posCloseBracket - posOpenBracket) + 1)
                                        '_Debug.Print_(lstHyperLink.URL_Hyperlink)
                                        If lstHyperLink.HTML_Hyperlink.Length > 0 Then
                                            ' Add <a href=" ... "> tags around the cut hyperlink to make a completed hyperlink tag
                                            lstHyperLink.HTML_Hyperlink = String.Format("<a href={0}{1}{2}>", Chr(34), lstHyperLink.HTML_Hyperlink, Chr(34))
                                            '_Debug.Print_(lstHyperLink.HTML_Hyperlink)
                                            ' Extract the Text and append it to the HTML hyperlink with </a> closeing hyperlink tag
                                            lstHyperLink.HTML_Hyperlink = String.Format("{0}{1}</a>", lstHyperLink.HTML_Hyperlink, rtf.Substring(posStartText, posEndText - posStartText))
                                            '_Debug.Print_(lstHyperLink.HTML_Hyperlink)
                                            pos2Start = ulnone + "\ulnone".Length
                                            Return True
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    End If
                End If
            End If

            pos2Start = rtf.Length ' if reached this line then no more hyperlinks left to process

        End Function
        Private Function hyperlink2List(ByVal rtf As String, ByRef hyperlinks As List(Of Hyperlink)) As Boolean
            Dim lstHyperLink As _RTF2HTML.Hyperlink
            Dim pos2Start As Integer = 1
            Do
                lstHyperLink = New _RTF2HTML.Hyperlink
                If _RTF2HTML.hyperlink2HTML(rtf, pos2Start, lstHyperLink) Then
                    hyperlinks.Add(lstHyperLink)
                End If
            Loop While pos2Start < rtf.Length
            hyperlink2List = (hyperlinks.Count > 0)
        End Function

        Private Function Mid(ByVal str As String, ByVal startIndex As Integer) As String
            ''
            startIndex = startIndex - 1
            Mid = String.Empty '' assume.
            If Not startIndex > str.Length Then
                Mid = str.Substring(startIndex)
            End If
            ''
        End Function
        Private Function Mid(ByVal str As String, ByVal startIndex As Integer, ByVal length As Integer) As String
            ''
            startIndex = startIndex - 1
            Mid = String.Empty '' assume.
            If Not startIndex > str.Length AndAlso Not (startIndex + length) > str.Length Then
                Mid = str.Substring(startIndex, length)
            End If
            ''
        End Function
        Public Function Left(ByVal str As String, ByVal length As Integer) As String
            ''
            Left = String.Empty '' assume.
            If Not length > str.Length Then
                Left = str.Substring(0, length)
            End If
            ''
        End Function
        Public Function Right(ByVal str As String, ByVal length As Integer) As String
            ''
            Right = String.Empty '' assume.
            If Not length > str.Length Then
                Right = str.Substring(str.Length - length, length)
            End If
            ''
        End Function

    End Module
End Namespace



