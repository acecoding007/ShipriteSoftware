Imports System.Data.OleDb

Public Module _Convert

    Private Sub error_DebugPrint(ByVal routineName As String, ByVal errorDesc As String)
        _Debug.PrintError_(String.Format("_Convert.{0}(): {1}", routineName, errorDesc))
    End Sub


    Public Function Null2DefaultValue(ByVal fld As Object, Optional ByVal defaultValue As Object = "") As Object
        Null2DefaultValue = defaultValue '' assume.
        If Not IsDBNull(fld) Then
            Null2DefaultValue = fld
        End If
    End Function

    Public Function Minutes2DecimalHours(ByVal minutes As Long) As Double
        Minutes2DecimalHours = 0
        Try ' format like 0.0 ... 1.0; 1.25; 1.5; 1.75; 2.0; etc.
            Dim remainder As Long = (minutes Mod 60)
            Dim hours As Double = ((minutes - remainder) / 60)
            Dim quaters As Double = 0
            Select Case remainder ' rounding
                Case 8 To 22 : quaters = 0.25
                Case 23 To 37 : quaters = 0.5
                Case 38 To 52 : quaters = 0.75
                Case 53 To 59 : quaters = 1
                Case Else : quaters = 0
            End Select
            Minutes2DecimalHours = hours + quaters
        Catch ex As Exception : error_DebugPrint("Minutes2DecimalHours", ex.Message)
        End Try
    End Function
    Public Function Minutes2HoursMinutes(ByVal minutes As Long) As String
        Minutes2HoursMinutes = String.Format("{0} min", minutes) ' assume.
        Try
            Dim hrs As Long = minutes \ 60
            Dim min As Long = minutes - (hrs * 60)
            If hrs > 0 Then
                Minutes2HoursMinutes = String.Format("{0} hr {1} min", hrs, min)
            Else
                Minutes2HoursMinutes = String.Format("{0} min", min)
            End If
        Catch ex As Exception : error_DebugPrint("Minutes2HoursMinutes", ex.Message)
        End Try
    End Function
    Public Function Date2String(ByVal date2convert As Date) As String
        Date2String = "01/01/0001" ' assume.
        Try
            Date2String = String.Format("{0:MM/dd/yyyy}", date2convert)
            If Date2String = "01/01/0001" Then
                Date2String = String.Empty
            End If
        Catch ex As Exception : error_DebugPrint("Date2String", ex.Message)
        End Try
    End Function
    Public Function Date2OracleDateTimeString(ByVal date2convert As Date) As String
        Date2OracleDateTimeString = "01/01/0001" ' assume.
        Try
            Date2OracleDateTimeString = String.Format("{0:yyyyMMdd HH:mm:ss}", date2convert)
            If Date2OracleDateTimeString = "01/01/0001" Then
                Date2OracleDateTimeString = String.Empty
            End If
        Catch ex As Exception : error_DebugPrint("Date2OracleDateTimeString", ex.Message)
        End Try
    End Function

    Public Function String2Date(ByVal text As String) As Date
        String2Date = #12:00:00 AM#
        Try
            String2Date = Convert.ToDateTime(text.Replace("-", "/").Trim)
        Catch ex As Exception : error_DebugPrint("String2Date", ex.Message)
        End Try
    End Function
    Public Function String2DateTime(ByVal str2date As String, ByVal str2time As String) As Date
        String2DateTime = #12:00:00 AM#
        Try
            String2DateTime = Convert.ToDateTime(str2date & " " & str2time)
        Catch ex As Exception : error_DebugPrint("String2DateTime", ex.Message)
        End Try
    End Function
    Public Function String2ProperCase(ByVal text As String) As String
        String2ProperCase = text ' assume.
        Try
            String2ProperCase = Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text)
        Catch ex As Exception : error_DebugPrint("String2ProperCase", ex.Message)
        End Try
    End Function
    Public Function String2Long(ByVal text As String) As Long
        String2Long = 0 ' assume.
        Try
            String2Long = Convert.ToInt64(text)
        Catch ex As Exception : error_DebugPrint("String2Long", ex.Message)
        End Try
    End Function
    Public Function String2Boolean_YesNo(ByVal yesno As String) As Boolean
        If "yes" = yesno.ToLower Then
            String2Boolean_YesNo = True
        Else
            String2Boolean_YesNo = False
        End If
    End Function

    Public Function LastFirstName2FirstLastName(ByVal name As String) As String
        LastFirstName2FirstLastName = name ' assume.
        Try
            Dim splitname() As String
            splitname = name.Split(",")
            If 1 < splitname.Length Then
                LastFirstName2FirstLastName = String.Format("{0} {1}", splitname(1).Trim, splitname(0).Trim)
            End If
        Catch ex As Exception : error_DebugPrint("LastFirstName2FirstLastName", ex.Message)
        End Try
    End Function

    Public Function Base64String2Byte(ByVal text As String) As Byte()
        Base64String2Byte = Nothing ' assume.
        Try
            Base64String2Byte = Convert.FromBase64String(text)
        Catch ex As Exception : error_DebugPrint("Base64String2Byte", ex.Message)
        End Try
    End Function
    Public Function Base64CharArray2Byte(ByVal array() As Char) As Byte()
        Base64CharArray2Byte = Nothing ' assume.
        Try
            Base64CharArray2Byte = Convert.FromBase64CharArray(array, 0, array.Length)
        Catch ex As Exception : error_DebugPrint("Base64CharArray2Byte", ex.Message)
        End Try
    End Function

    Public Function Base64String2Bitmap(ByVal text As String) As Drawing.Bitmap
        Dim bytes As Byte() = Nothing
        Dim imageBitmap As Drawing.Bitmap = Nothing
        Try
            bytes = Convert.FromBase64String(text)
            Using memStream As New IO.MemoryStream(bytes)
                imageBitmap = TryCast(Drawing.Image.FromStream(memStream), Drawing.Bitmap)
            End Using
            Return imageBitmap
        Catch ex As Exception : error_DebugPrint("Base64String2Bitmap", ex.Message)
            Return Nothing
        End Try
    End Function

    Public Function Boolean2Integer(bValue As Boolean, Optional TrueValueIs As Integer = 1) As Integer
        Boolean2Integer = 0
        If bValue Then Boolean2Integer = TrueValueIs
    End Function
    Public Function Boolean2String_YesNo(bValue As Boolean) As String
        If bValue Then
            Boolean2String_YesNo = "Yes"
        Else
            Boolean2String_YesNo = "No"
        End If
    End Function
    Public Function Boolean2TrueFalse(bValue As Boolean) As String
        If bValue Then
            Return "True"
        Else
            Return "False"
        End If
    End Function
    Public Function Boolean2OnOff(bValue As Boolean) As String
        If bValue Then
            Return "On"
        Else
            Return "Off"
        End If
    End Function

    Public Function Integer2Boolean(ByVal v As Integer) As Boolean
        Integer2Boolean = (Not 0 = v)
    End Function
    Public Function Long2Boolean(ByVal v As Long) As Boolean
        Long2Boolean = (Not 0 = v)
    End Function

    Public Function Double2Currency(ByVal price As Double) As String
        Double2Currency = "$0.00" ' assume
        Try
            Double2Currency = String.Format("{0:c}", price)
        Catch ex As Exception : error_DebugPrint("Double2Currency", ex.Message)
        End Try
    End Function
    Public Function Currency2Double(ByVal price As String) As Double
        Currency2Double = 0
        Try
            Currency2Double = Val(price.Replace("$", "").Trim)
        Catch ex As Exception : error_DebugPrint("Currency2Double", ex.Message)
        End Try
    End Function

    Public Function Round_Double2Decimals(ByVal curNumber As Double, ByVal decimalsNumber As Integer) As Double
        Round_Double2Decimals = 0
        Try
            Select Case decimalsNumber
                Case 0 : Round_Double2Decimals = Val(Format(curNumber, "0"))
                Case 1 : Round_Double2Decimals = Val(Format(curNumber, "0.0"))
                Case 2 : Round_Double2Decimals = Val(Format(curNumber, "0.00"))
                Case 3 : Round_Double2Decimals = Val(Format(curNumber, "0.000"))
                Case 4 : Round_Double2Decimals = Val(Format(curNumber, "0.0000"))
                Case Else : Round_Double2Decimals = curNumber
            End Select
        Catch ex As Exception : error_DebugPrint("Round_Double2Decimals", ex.Message)
        End Try
    End Function

    Public Function Weight_Oz2Lb(ByVal weightOz As Double) As Double
        Dim ozs As Double
        Dim lbs As Double

        Weight_Oz2Lb = 0
        Try
            ozs = weightOz Mod 16
            lbs = (weightOz - ozs) / 16
            Return lbs + (ozs / 16)
        Catch ex As Exception : error_DebugPrint("Weight_Oz2Lb", ex.Message)
        End Try

    End Function
    Public Function Weight_Oz2LbOz(ByVal weightOz As Double) As String
        Dim ozs As Double
        Dim lbs As Double

        Weight_Oz2LbOz = ""
        Try
            lbs = Int(weightOz / 16)
            ozs = weightOz - (lbs * 16)
            Return lbs & " lb " & Strings.Format(ozs, "0.0") & " oz"
        Catch ex As Exception : error_DebugPrint("Weight_Oz2LbOz", ex.Message)
        End Try

    End Function
    Public Function Pounds2Ounces(ByVal pounds As Double, Optional rounddecimals As Integer = 2) As Double
        Pounds2Ounces = 0
        Try
            Return Math.Round(pounds * 16, rounddecimals)
        Catch ex As Exception : error_DebugPrint("Pounds2Ounces", ex.Message)
        End Try
    End Function

    Public Function Kg2Lb(ByVal kg As Double, Optional decimals As Long = 2) As Double
        Kg2Lb = 0
        Try ''ol#9.157(3/21)... Metric or Imperial switch was added to have KG or LB and CM or IN.
            Kg2Lb = Round(kg * 2.20462, decimals)
        Catch ex As Exception : error_DebugPrint("Convert.Kg2Lb", ex.Message)
        End Try
    End Function
    Public Function Cm2IN(ByVal cm As Double, Optional decimals As Long = 2) As Double
        Cm2IN = 0
        Try ''ol#9.157(3/21)... Metric or Imperial switch was added to have KG or LB and CM or IN.
            Cm2IN = Round(cm * 0.3937008, decimals)
        Catch ex As Exception : error_DebugPrint("Convert.Cm2IN", ex.Message)
        End Try
    End Function
    Public Function Lb2Kg(ByVal lb As Double, Optional decimals As Long = 2) As Double
        Lb2Kg = 0
        Try ''ol#9.157(3/21)... Metric or Imperial switch was added to have KG or LB and CM or IN.
            Lb2Kg = Round(lb * 0.4535924, decimals)
        Catch ex As Exception : error_DebugPrint("Convert.Lb2Kg", ex.Message)
        End Try
    End Function
    Public Function IN2Cm(ByVal Inch As Double, Optional decimals As Long = 2) As Double
        IN2Cm = 0
        Try ''ol#9.157(3/21)... Metric or Imperial switch was added to have KG or LB and CM or IN.
            IN2Cm = Round(Inch * 2.54, decimals)
        Catch ex As Exception : error_DebugPrint("Convert.IN2Cm", ex.Message)
        End Try
    End Function
    Public Function ZipCode2FiveDigits(ByVal zipCode As String, ByVal isUSA As Boolean) As String
        If zipCode.Length > 5 And isUSA Then
            Return Val(Left(zipCode, 5)).ToString
        ElseIf zipCode.Length = 5 And isUSA Then
            Return Val(zipCode).ToString
        Else
            Return zipCode
        End If
    End Function
    Public Function ZipCode2FiveDigits(ByVal zipCode As Long, ByVal isUSA As Boolean) As Long
        If zipCode.ToString.Length > 5 And isUSA Then
            Return Val(Left(zipCode, 5))
        ElseIf zipCode.tostring.Length = 5 And isUSA Then
            Return Val(zipCode)
        Else
            Return zipCode
        End If
    End Function

    Public Function StringToBase64(input As String) As String
        Dim output As String = Convert.ToBase64String(System.Text.Encoding.Unicode.GetBytes(input))
        Return output
    End Function

    Public Function Base64ToString(input As String) As String
        Dim output As String = input ' default
        Try
            output = System.Text.Encoding.Unicode.GetString(Convert.FromBase64String(input))
        Catch ex As Exception
        End Try
        Return output
    End Function
End Module

