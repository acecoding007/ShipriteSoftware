Module Security
    Public gConnect As String
    Public gAccessCodes As String

    Dim EncryptedBuffer As String

    Public Function CRC16A(Data() As Byte) As Long

        Dim Temp As Long
        Dim CRC As Long

        CRC = 0
        For i = 0 To UBound(Data) - 1

            Temp = Data(i) Xor ShiftRight(CRC, 8)
            Temp = Temp Xor ShiftRight(Temp, 4)
            Temp = Temp Xor ShiftRight(Temp, 2)
            Temp = Temp Xor ShiftRight(Temp, 1)
            CRC = ShiftLeft(CRC, 8) Xor ShiftLeft(Temp, 15) Xor ShiftLeft(Temp, 2) Xor Temp

        Next i
        CRC16A = CRC

    End Function

    Public Function CheckRegistration(RegString As String, RegCode As String, Password As String, ByRef ExpireDate As Date, ByRef SupportDate As Date, ByRef AccessCode As String) As Integer

        Dim CodeLength As Integer
        Dim CodeString As String
        Dim ExpireString As String
        Dim SupportString As String
        Dim CodeBuf As String
        Dim RegNum As String
        Dim EDate As String
        Dim SDate As String
        Dim LongEDate As Long
        Dim LongSDate As Long
        Dim TDate As Date
        Dim BigString As String
        Dim tstRegistrationNumber As String
        Dim NoSupportWarning As Boolean
        Dim fnum As Integer
        Dim ret As Integer
        Dim SQL As String
        Dim SegmentSet As String
        Dim ans As Integer = 0
        Dim Passwordtot As Long
        Dim Tot As Long = 0
        Dim BaseDate As Date = "12/31/1899"
        Dim buf As String

        If gDemoExpired = True Then

            MsgBox("Your Registration Key Has Expired.  Call support for help!", vbCritical)
            gRegistrationExpired = True
            AccessCode = ""
            Return 1

        End If
        RegNum = LTrim$(RTrim$(RegCode))

        If Len(RegNum) < 6 Then
            gRegistrationExpired = True
            MsgBox("Your Registration Key Has Expired.  Call support for help!", vbCritical)
            Return 1
        End If

        On Error Resume Next

        CodeBuf = RegCode
        CodeLength = (Asc((Right(CodeBuf, 1))) - Asc("A"))
        CodeBuf = Mid$(CodeBuf, 0, (Len(CodeBuf) - 1))
        CodeString = Right(CodeBuf, CodeLength)
        CodeBuf = Mid$(CodeBuf, 0, (Len(CodeBuf) - CodeLength))
        SupportString = Right(CodeBuf, 4)
        CodeBuf = Mid$(CodeBuf, 0, (Len(CodeBuf) - 4))
        ExpireString = Right(CodeBuf, 4)
        CodeBuf = Mid$(CodeBuf, 0, (Len(CodeBuf) - 4))
        On Error GoTo 0

        LongEDate = CInt("&h" & ExpireString) - 1
        LongSDate = CInt("&h" & SupportString) - 1

        AccessCode = CodeString
        ExpireDate = BaseDate.AddDays(LongEDate)
        SupportDate = BaseDate.AddDays(LongSDate)
        EDate = ExpireDate.ToString("MM-dd-yyyy")
        SDate = SupportDate.ToString("MM-dd-yyyy")

        Passwordtot = 0
        Password = Trim(Password)
        For j = 0 To Len(Password) - 1

            Passwordtot += Asc(Mid$(Password, j, 1))

        Next j
        Tot = 0
        BigString = (LTrim$(RTrim$(RegString)) & (EDate & (SDate & CodeString)))
        For j = 0 To Len(BigString) - 1

            buf = Mid$(BigString, j, 1)
            Tot += ((Asc(buf) * Passwordtot))

        Next j
        buf = Tot.ToString("X")
        tstRegistrationNumber = (buf & (ExpireString & (SupportString & (CodeString & (Chr(Asc("A") + CodeLength))))))
        If Not tstRegistrationNumber = RegCode Then

            MsgBox("Your Registration Key is Invalid.  Call support for help!", vbCritical)
            gRegistrationExpired = True
            AccessCode = ""
            Return 1

        End If
        TDate = Today
        If TDate >= ExpireDate Then

            MsgBox("Your Registration Key Has Expired.  Call support for help!", vbCritical)
            gRegistrationExpired = True
            AccessCode = ""
            Return 1

        End If
        gRegistrationExpired = False
        NoSupportWarning = False
        If ExpireDate <= Today.AddDays(30) Then

            Tot = (ExpireDate.Date - Today.Date).TotalDays
            MsgBox("ATTENTION...Your registration key expires in " & Tot.ToString & " days." & vbCr & "Call support for help.", vbCritical, gProgramName)
            NoSupportWarning = True

        End If
        If NoSupportWarning = False Then

            If SupportDate <= TDate.AddDays(30) Then

                If TDate >= SupportDate Then

                    MsgBox("ATTENTION...Your Software Support has expired.  Call support to renew immediately.", vbCritical, gProgramName)
                    gSupportExpired = True

                Else

                    Tot = (SupportDate.Date - Today.Date).TotalDays
                    MsgBox("ATTENTION...Your Software Support expires in " & Tot.ToString & " days." & vbCr & "Call support to renew.", vbCritical, gProgramName)

                End If

            End If

        End If
        Return 0

    End Function

    Function MakeRegistration(RegString As String, Expire As String, SPExpire As String, Password As String, Options As String) As String

        Dim ExpireDate As Long
        Dim SupportDate As Long
        Dim tot As Long
        Dim BigString As String
        Dim PasswordTot As Long

        PasswordTot = 0
        Password = LTrim$(RTrim$(Password))
        For j = 1 To Len(Password)

            PasswordTot += Asc(Mid$(Password, j, 1))

        Next j
        BigString = LTrim$(RTrim$(RegString)) & LTrim$(RTrim$(Expire)) & LTrim$(RTrim$(SPExpire)) & Options
        ExpireDate = Convert.ToDateTime(Expire).Ticks
        SupportDate = Convert.ToDateTime(SPExpire).Ticks

        tot = 0

        For j = 1 To Len(BigString)

            tot += (Asc(Mid$(BigString, j, 1)) * PasswordTot)

        Next j
        Return Hex$(tot) & Hex$(ExpireDate) & Hex$(SupportDate) & Options & Chr(Asc("A") + Len(Options))

    End Function

    Public Function NEW_MakeRegistrationString(dbPath As String) As String

        Dim RBuf As String
        Dim ret As String
        Dim ExpireDate As Date
        Dim SupportDate As Date
        Dim RegCode As String
        Dim SQL As String
        Dim iloc As Integer

        RBuf = GetPolicyData(dbPath, "Name")
        If RBuf = "" And gSetupPolicy = "" Then

            SQL = "SELECT * FROM Setup WHERE ID = 1"
            gSetupPolicy = IO_GetSegmentSet(dbPath, SQL, "", True)
            RBuf = GetPolicyData(dbPath, "Name")

        End If
        RBuf += GetPolicyData(dbPath, "FName")
        RBuf += GetPolicyData(dbPath, "LName")
        RBuf += GetPolicyData(dbPath, "Addr1")
        RBuf += GetPolicyData(dbPath, "Addr2")
        RBuf += GetPolicyData(dbPath, "City")
        RBuf += GetPolicyData(dbPath, "State")
        RBuf += GetPolicyData(dbPath, "Zip")
        RBuf += GetPolicyData(dbPath, "Phone1")
        RBuf += GetPolicyData(dbPath, "Phone2")
        If RBuf = "" Then

            ret = UpdatePolicy(dbPath, "Name", "SmartTouch POS Demo Company")
            ret = UpdatePolicy(dbPath, "FName", "Gary")
            ret = UpdatePolicy(dbPath, "LName", "Ford")
            ret = UpdatePolicy(dbPath, "Addr1", "1312 Genesee Street")
            ret = UpdatePolicy(dbPath, "Addr2", "")
            ret = UpdatePolicy(dbPath, "City", "Utica, NY")
            ret = UpdatePolicy(dbPath, "State", "NY")
            ret = UpdatePolicy(dbPath, "Zip", "13403")
            ret = UpdatePolicy(dbPath, "Phone1", "315-733-6191")
            ret = UpdatePolicy(dbPath, "Phone2", "315-733-6194")
            RBuf = GetPolicyData(dbPath, "Name")
            RBuf += GetPolicyData(dbPath, "FName")
            RBuf += GetPolicyData(dbPath, "LName")
            RBuf += GetPolicyData(dbPath, "Addr1")
            RBuf += GetPolicyData(dbPath, "Addr2")
            RBuf += GetPolicyData(dbPath, "City")
            RBuf += GetPolicyData(dbPath, "State")
            RBuf += GetPolicyData(dbPath, "Zip")
            RBuf += GetPolicyData(dbPath, "Phone1")
            RBuf += GetPolicyData(dbPath, "Phone2")
            ExpireDate = Format$(Today.Ticks + 45, "mm/dd/yyyy")
            SupportDate = Format$(Today.Ticks + 45, "mm/dd/yyyy")
            RegCode = MakeRegistration(RBuf, Format$(ExpireDate, "mm-dd-yyyy"), Format$(SupportDate, "mm-dd-yyyy"), "gunny", "ABCDEFGHIJKLMNOPQRSTUVWXYZ")
            iloc = CheckRegistration(RBuf, RegCode, "gunny", ExpireDate, SupportDate, "ABCDEFGHIJKLMNOPQRSTUVWXYZ")
            If iloc = 0 Then

                ret = UpdatePolicy(dbPath, "RegistrationNumber", RegCode)

            End If
            MsgBox("ATTENTION...Created Default Registration Key" & vbCrLf & vbCrLf & "Expiration Date:  " & Format$(ExpireDate, "mm/dd/yyyy"), vbInformation)

        End If
        Return RBuf

    End Function

    Function ValidateAccess(UserID As String, vClass As String, Perm_Name As String, Perm_Msg As String, Optional NoPrint As Boolean = False) As Integer

        Dim SQL As String
        Dim msghead As String
        Dim msgtail As String
        Dim Passcode As Integer = 0
        Dim FailCode As Integer = 1

        Dim AccessDeniedMsg As String

        If String.IsNullOrWhiteSpace(vClass) Then
            Return Passcode
        End If

        If vClass = "FULL" And Not gIsProgramSecurityEnabled Then ' check "FULL" but full security setting disabled

            Return Passcode

        End If
        If vClass = "POS" And Not gIsPOSSecurityEnabled Then ' check "POS" but pos security setting disabled

            Return Passcode

        End If
        ''
        If Len(vClass) = 1 Then ' check registration access - separate from other checks

            If InStr(1, gAccessCodes, vClass) = 0 Then

                MsgBox("ATTENTION...You have not purchased access to this Program feature!!!" & vbCrLf & vbCrLf & Perm_Msg & vbCrLf & vbCrLf & "Contact Support to Purchase this feature.", vbCritical, gProgramName)
                Return FailCode

            Else
                Return Passcode
            End If

        End If
        ''
        ' check user permission otherwise
        If Not String.IsNullOrWhiteSpace(UserID) Then
            If gUserSegment = "" Then

                SQL = "SELECT * FROM Users WHERE [DisplayName] = '" & UserID & "'"
                gUserSegment = IO_GetSegmentSet(gShipriteDB, SQL)

            End If
            If Not Perm_Name = "" And Not ExtractElementFromSegment(Perm_Name, gUserSegment) = "True" Then

                If NoPrint = False And Not Perm_Msg = "" Then

                    msghead = "User - " & GetRunTimePolicy(gGLOBALpolicy, "CurrentUser") & "...Access Denied!!!" & Chr(10) & Chr(10)
                    msgtail = "You do not have Access Rights here.  See your System Administrator!"
                    AccessDeniedMsg = msghead & Perm_Msg & Chr(10) & Chr(10) & msgtail
                    MsgBox(AccessDeniedMsg, vbCritical, gProgramName)

                End If
                Return FailCode

            End If
        End If

        Return Passcode

    End Function

    Function ValidateRegistration(Code As String, RegErrorMessage As String) As Integer

        If InStr(1, gAccessCodes, Code) = 0 Then

            MsgBox("ATTENTION...Registration Code Failure" & vbCrLf & vbCrLf & RegErrorMessage)
            Return 1

        Else

            Return 0

        End If

    End Function

    Public Function ShiftRight(ByVal lngNumber As Long, ByVal intNumBits As Integer) As Long

        Return lngNumber \ 2 ^ intNumBits 'note the integer division op

    End Function

    Public Function ShiftLeft(ByVal lngNumber As Long, ByVal intNumBits As Integer) As Long

        Dim TruncateLeftMostBit As Long
        Dim i As Integer

        TruncateLeftMostBit = 65535 '61439
        For i = 1 To intNumBits

            lngNumber = lngNumber * 2
            lngNumber = lngNumber And TruncateLeftMostBit

        Next i
        Return lngNumber

    End Function
End Module
