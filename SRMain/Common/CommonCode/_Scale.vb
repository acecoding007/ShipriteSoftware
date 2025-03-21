Imports System.IO.Ports
Imports SHIPRITE.HidLibrary

'TODO: Simplify Scale object(s)

Namespace Scale

    Public Enum ScaleType
        Serial
        USB
    End Enum

    Public Class ScaleModels
        Public Const NONE As String = "(none)"
        Public Const DetectoAS350D As String = SerialScale.ScaleModels.DetectoAS350D ' serial
        Public Const FairbanksSCB2453Series As String = SerialScale.ScaleModels.FairbanksSCB2453Series  ' serial
        Public Const FairbanksUltegraBenchScale As String = SerialScale.ScaleModels.FairbanksUltegraBenchScale ' serial
        Public Const FairbanksUltegraBenchScaleUSB As String = UsbScale.ScaleModels.FairbanksUltegraBenchScaleUSB
        Public Const ToledoPS60 As String = SerialScale.ScaleModels.ToledoPS60 ' serial
        Public Const ToledoPS60USB As String = UsbScale.ScaleModels.ToledoPS60USB
        Public Const ToledoPS6L As String = SerialScale.ScaleModels.ToledoPS6L ' serial
        Public Const ToledoPS6LUSB As String = UsbScale.ScaleModels.ToledoPS6LUSB
        Public Const Toledo8213BenchScale As String = SerialScale.ScaleModels.Toledo8213BenchScale ' serial
        Public Const Toledo8213BenchScale2 As String = SerialScale.ScaleModels.Toledo8213BenchScale2 ' serial
        Public Const NCI7720_7620PostalScale As String = UsbScale.ScaleModels.NCI7720_7620PostalScale

        Public Shared ReadOnly Property List As List(Of String)
            Get
                Return New List(Of String)(SerialScale.ScaleModels.List.Concat(UsbScale.ScaleModels.List))
            End Get
        End Property
    End Class

    Public Class ScaleDbFields
        Public Const fldScaleModel As String = "Scale"
        Public Const fldScalePort As String = "ScalePort"
        Public Const fldScaleSpeed As String = "ScaleSpeed"
        Public Const fldScaleParity As String = "ScaleParity"
        Public Const fldScaleDataBits As String = "ScaleDataBits"
        Public Const fldScaleStopBits As String = "ScaleStopBits"
        Public Const fldScaleWeightLimit As String = "ScaleWeightLimit"
    End Class

    Public Class BaseScale
        Public Property Type As ScaleType
        Private Property m_Model As String
        Public Property Model As String
            Get
                Return m_Model
            End Get
            Set(value As String)
                'If value.Trim.ToLower = "(none)" Or value.Trim.ToLower = "none" Then
                '    value = ""
                'End If
                m_Model = value
            End Set
        End Property
        Public Property Serial_Port As String
        Public Property Serial_Speed As String
        Public ReadOnly Property Serial_Speed_Com As Integer
            Get
                Return SerialScale.ScaleSpeed.ConvertToInt(Serial_Speed)
            End Get
        End Property
        Public Property Serial_Parity As String
        Public ReadOnly Property Serial_Parity_Com As Parity
            Get
                Select Case Serial_Parity
                    Case SerialScale.ScaleParity.E
                        Return Parity.Even
                    Case SerialScale.ScaleParity.O
                        Return Parity.Odd
                    Case SerialScale.ScaleParity.N
                        Return Parity.None
                    Case Else
                        Return Parity.None
                End Select
            End Get
        End Property
        Public Property Serial_DataBits As String
        Public ReadOnly Property Serial_DataBits_Com As Integer
            Get
                Return SerialScale.ScaleDataBit.ConvertToInt(Serial_DataBits)
            End Get
        End Property
        Public Property Serial_StopBits As String
        Public ReadOnly Property Serial_StopBits_Com As StopBits
            Get
                Return SerialScale.ScaleStopBit.ConvertToStopBits(Serial_StopBits)
            End Get
        End Property
        Public Property WeightLimit As String
        Public Property ScaleSettings As String
        Public Property StopScale As Boolean
        Public Property IsWeightKeyed As Boolean
        Public Property IsError As Boolean
        'Public Property ScaleSwitch As Boolean
        Public ReadOnly Property Model_IsValid As Boolean
            Get
                If Model IsNot Nothing AndAlso Model.Length > 0 AndAlso Model <> ScaleModels.NONE Then
                    Return ScaleModels.List.Exists(Function(x) x = Model) ' check if scale model exists in our list or not
                End If
                Return False
            End Get
        End Property
        Public ReadOnly Property Serial_PortSettings_IsValid As Boolean
            Get
                Return Model IsNot Nothing AndAlso Model.Length > 0 AndAlso
                Serial_Speed IsNot Nothing AndAlso Serial_Speed.Length > 0 AndAlso
                Serial_Port IsNot Nothing AndAlso Serial_Port.Length > 0 AndAlso
                Serial_Parity IsNot Nothing AndAlso Serial_Parity.Length > 0 AndAlso
                Serial_DataBits IsNot Nothing AndAlso Serial_DataBits.Length > 0 AndAlso
                Serial_StopBits IsNot Nothing AndAlso Serial_StopBits.Length > 0
            End Get
        End Property
        Private Property m_ScaleDisplay As String
        Public Property ScaleDisplay As String
            Get
                If IsNumeric(m_ScaleDisplay) Then
                    If 2402.5 < m_ScaleDisplay Then
                        Return New String("-", 13)
                    Else
                        ScaleDisplay = _Convert.Weight_Oz2LbOz(Val(m_ScaleDisplay))
                    End If
                Else
                    Return m_ScaleDisplay
                End If
            End Get
            Set(value As String)
                m_ScaleDisplay = value
            End Set
        End Property

        Public Sub New(Optional scaleName As String = "")

            If scaleName Is Nothing Then scaleName = ""
            Load_DefaultScaleSettings(scaleName)
            StopScale = False ' enabled by default
            IsWeightKeyed = False ' default
            IsError = False ' default

        End Sub

        Public Sub New(bScale As BaseScale)

            If bScale IsNot Nothing Then
                Model = bScale.Model
                Type = bScale.Type
                Serial_Port = bScale.Serial_Port
                Serial_Speed = bScale.Serial_Speed
                Serial_Parity = bScale.Serial_Parity
                Serial_DataBits = bScale.Serial_DataBits
                Serial_StopBits = bScale.Serial_StopBits
                WeightLimit = bScale.WeightLimit
            Else
                Load_DefaultScaleSettings("")
            End If
            StopScale = False ' enabled by default
            IsWeightKeyed = False ' default
            IsError = False ' default

        End Sub

        Public NotOverridable Overrides Function Equals(obj As Object) As Boolean

            Dim sObj As BaseScale = TryCast(obj, BaseScale)
            If sObj IsNot Nothing Then
                Return Model = sObj.Model AndAlso Type = sObj.Type AndAlso Serial_Port = sObj.Serial_Port AndAlso
                Serial_Speed = sObj.Serial_Speed AndAlso Serial_Parity = sObj.Serial_Parity AndAlso Serial_DataBits = sObj.Serial_DataBits AndAlso
                Serial_StopBits = sObj.Serial_StopBits AndAlso WeightLimit = sObj.WeightLimit
            End If
            Return False

        End Function

        Public Shared Operator =(obj1 As BaseScale, obj2 As BaseScale)
            Return obj1.Equals(obj2)
        End Operator

        Public Shared Operator <>(obj1 As BaseScale, obj2 As BaseScale)
            Return Not obj1.Equals(obj2)
        End Operator

        Public Sub Load_ScaleFromPolicy(Optional isSetDefaultWeight As Boolean = True)

            Model = GetPolicyData(gReportsDB, ScaleDbFields.fldScaleModel)
            Type = Get_ScaleType(Model)
            Serial_Port = GetPolicyData(gReportsDB, ScaleDbFields.fldScalePort)
            Serial_Speed = GetPolicyData(gReportsDB, ScaleDbFields.fldScaleSpeed)
            Serial_Parity = GetPolicyData(gReportsDB, ScaleDbFields.fldScaleParity)
            Serial_DataBits = GetPolicyData(gReportsDB, ScaleDbFields.fldScaleDataBits)
            Serial_StopBits = GetPolicyData(gReportsDB, ScaleDbFields.fldScaleStopBits)
            WeightLimit = GetPolicyData(gReportsDB, ScaleDbFields.fldScaleWeightLimit)
            If isSetDefaultWeight AndAlso String.IsNullOrWhiteSpace(WeightLimit) Then
                WeightLimit = "150"
            End If

        End Sub

        Public Function Save_Scale() As Boolean

            Dim ret As Integer = 0

            ret += UpdatePolicy(gReportsDB, ScaleDbFields.fldScaleModel, Model)
            ret += UpdatePolicy(gReportsDB, ScaleDbFields.fldScalePort, Serial_Port)
            ret += UpdatePolicy(gReportsDB, ScaleDbFields.fldScaleSpeed, Serial_Speed)
            ret += UpdatePolicy(gReportsDB, ScaleDbFields.fldScaleParity, Serial_Parity)
            ret += UpdatePolicy(gReportsDB, ScaleDbFields.fldScaleDataBits, Serial_DataBits)
            ret += UpdatePolicy(gReportsDB, ScaleDbFields.fldScaleStopBits, Serial_StopBits)
            ret += UpdatePolicy(gReportsDB, ScaleDbFields.fldScaleWeightLimit, WeightLimit)
            Return ret = 7

        End Function

        Private Function Get_ScaleType(Optional scaleModel As String = "") As ScaleType

            If scaleModel IsNot Nothing AndAlso scaleModel.ToUpper.EndsWith("USB") Then
                Return ScaleType.USB
            Else
                Return ScaleType.Serial
            End If

        End Function

        Public Sub Load_DefaultScaleSettings(scaleName As String)

            Model = scaleName
            Type = Get_ScaleType(scaleName)
            Serial_Port = "COM1"
            WeightLimit = "150"
            Select Case scaleName
                Case ScaleModels.DetectoAS350D
                    Serial_Speed = SerialScale.ScaleSpeed.NineThousandSixHundred
                    Serial_Parity = SerialScale.ScaleParity.E
                    Serial_DataBits = SerialScale.ScaleDataBit.Eight
                    Serial_StopBits = SerialScale.ScaleStopBit.One

                Case ScaleModels.FairbanksSCB2453Series, ScaleModels.Toledo8213BenchScale2, ScaleModels.NCI7720_7620PostalScale
                    Serial_Speed = SerialScale.ScaleSpeed.NineThousandSixHundred
                    Serial_Parity = SerialScale.ScaleParity.E
                    Serial_DataBits = SerialScale.ScaleDataBit.Seven
                    Serial_StopBits = SerialScale.ScaleStopBit.Two

                Case ScaleModels.FairbanksUltegraBenchScale
                    Serial_Speed = SerialScale.ScaleSpeed.NineThousandSixHundred
                    Serial_Parity = SerialScale.ScaleParity.O
                    Serial_DataBits = SerialScale.ScaleDataBit.Seven
                    Serial_StopBits = SerialScale.ScaleStopBit.Two

                Case ScaleModels.FairbanksUltegraBenchScaleUSB, ScaleModels.ToledoPS60USB, ScaleModels.ToledoPS6LUSB
                    Serial_Port = ""
                    Serial_Speed = ""
                    Serial_Parity = ""
                    Serial_DataBits = ""
                    Serial_StopBits = ""

                Case ScaleModels.ToledoPS60, ScaleModels.ToledoPS6L, ScaleModels.Toledo8213BenchScale
                    Serial_Port = "COM1"
                    Serial_Speed = SerialScale.ScaleSpeed.NineThousandSixHundred
                    Serial_Parity = SerialScale.ScaleParity.E
                    Serial_DataBits = SerialScale.ScaleDataBit.Seven
                    Serial_StopBits = SerialScale.ScaleStopBit.One

                Case Else
                    Model = "(none)" ' ""
                    Type = ScaleType.Serial
                    Serial_Port = ""
                    Serial_Speed = ""
                    Serial_Parity = ""
                    Serial_DataBits = ""
                    Serial_StopBits = ""
                    WeightLimit = ""

            End Select

        End Sub

        Public Function Get_Weight(Optional ByRef errDesc As String = "") As String

            Dim lb As Double
            Dim oz As Double
            Dim wt As Double
            Dim iloc As Integer
            Dim ret As String = ""

            errDesc = ""
            'StopScale = False
            Get_Weight = ""

            Try
                Select Case Model

                    Case ScaleModels.ToledoPS60USB, ScaleModels.ToledoPS6LUSB
                        Dim usbScale As New UsbScale.ScaleReader With {
                            .VendorId = Convert.ToInt32("0x0EB8", 16),
                            .ProductId = Convert.ToInt32("0xF000", 16)
                        }
                        If usbScale.Read_ScaleWeight(ret, errDesc) Then
                            ScaleDisplay = ret ' ounces
                            Get_Weight = Convert.ToString(_Convert.Weight_Oz2Lb(ret))
                        End If

                    Case ScaleModels.FairbanksUltegraBenchScaleUSB
                        Dim usbScale As New UsbScale.ScaleReader With {
                            .VendorId = Convert.ToInt32("0x0B67", 16),
                            .ProductId = Convert.ToInt32("0x555E", 16)
                        }
                        If usbScale.Read_ScaleWeight(ret, errDesc) Then
                            ScaleDisplay = ret ' ounces
                            Get_Weight = Convert.ToString(_Convert.Weight_Oz2Lb(ret))
                        End If

                    Case ScaleModels.DetectoAS350D
                        If Serial_Get_ScaleResponse("~", Chr(3), ret, errDesc) Then
                            If Len(ret) > 2 Then
                                If ret.Substring(0, 1) = Chr(2) Then
                                    ret = ret.Substring(1) ' Strings.Mid(ret, 2)
                                    iloc = ret.Length ' Strings.Len(ret)
                                    ret = ret.Substring(0, iloc - 1) ' Strings.Mid(ret, 1, iloc - 1)
                                    ret = ret.Trim ' Trim(ret)
                                    iloc = ret.IndexOf("LB", 0) ' Strings.InStr(1, ret, "LB")
                                    lb = Val(ret.Substring(0, iloc - 1)) ' Val(Strings.Mid(ret, 1, iloc - 1))
                                    ret = ret.Substring(iloc + 3) ' Strings.Mid(ret, iloc + 3)
                                    iloc = ret.IndexOf("OZ", 0)  ' Strings.InStr(1, ret, "OZ")
                                    oz = Val(ret.Substring(0, iloc - 1)) ' Val(Strings.Mid(ret, 1, iloc - 1))
                                    Get_Weight = lb + (oz / 16)
                                    ScaleDisplay = lb + (oz / 16)
                                End If
                            End If
                        End If

                    Case ScaleModels.ToledoPS6L
                        If Serial_Get_ScaleResponse("L", Chr(13), ret, errDesc) Then
                            iloc = Strings.InStr(1, ret, Chr(2))
                            If iloc > 0 And Strings.Len(ret) > 2 Then
                                ret = Strings.Mid(ret, 2, Strings.Len(ret) - 1)
                            End If
                            If Not (ret = "" And Not StopScale) Then
                                If Strings.InStr(1, ret, "?") = 0 Then
                                    Dim nPounds As Integer
                                    Dim dOunces As Double
                                    Dim fZeros As Boolean
                                    Dim nOzLoc As Integer
                                    Dim nLbLoc As Integer
                                    Dim Total As Double

                                    fZeros = True
                                    While fZeros = True
                                        If ret = "" Then
                                            Throw New Exception("ToledoPS6L - Scale data blank unexpectedly.")
                                        End If
                                        If Strings.Left(ret, 1) = "0" Then ' Controls_.Left_(ret, 1) = "0" Then
                                            ret = Strings.Right(ret, Len(ret) - 1) ' Controls_.Right_(ret, Len(ret) - 1) 'lb08.1oz
                                        Else
                                            fZeros = False
                                        End If
                                        If Strings.Left(ret, 1) = "." Then 'Controls_.Left_(ret, 1) = "." Then
                                            fZeros = False
                                        End If
                                    End While

                                    nOzLoc = Strings.InStr(1, ret, "oz")
                                    nLbLoc = Strings.InStr(1, ret, "lb")
                                    nPounds = Val(Strings.Left(ret, nLbLoc - 1)) 'Val(Controls_.Left_(ret, nLbLoc - 1))
                                    dOunces = Val(Strings.Right(Strings.Left(ret, nOzLoc - 1), 4)) 'Val(Controls_.Right_(Controls_.Left_(ret, nOzLoc - 1), 4)) ''ol#7.76(7/7).

                                    dOunces /= 16
                                    dOunces = Format$(dOunces, "0.00")
                                    Total = nPounds + dOunces
                                    Get_Weight = Total
                                    ScaleDisplay = Total
                                End If
                            End If
                        End If

                    Case ScaleModels.ToledoPS60
                        If Serial_Get_ScaleResponse("L", Chr(13), ret, errDesc) Then
                            iloc = Strings.InStr(1, ret, Chr(2))
                            If iloc > 0 Then
                                ret = Strings.Mid(ret, 2, Len(ret) - 1)
                            End If
                            If Not (ret = "" And Not StopScale) Then
                                If Strings.InStr(1, ret, "?") = 0 Then
                                    Get_Weight = Format$(Val(ret), "0.00") ' lbs
                                End If
                            End If
                            ScaleDisplay = _Convert.Pounds2Ounces(Val(ret)) ' ounces
                        End If

                    Case ScaleModels.FairbanksSCB2453Series
                        If Serial_Get_ScaleResponse("W" & Chr(13), Chr(13), ret, errDesc) Then
                            If Strings.Len(ret) > 2 Then
                                ret = Strings.Mid(ret, 2)
                                ret = Strings.Trim(ret)
                                iloc = Strings.InStr(1, ret, "?")
                                If iloc > 0 Then
                                    ret = Strings.Mid(ret, 1, iloc - 1)
                                End If
                                If Val(ret) > 0 Then
                                    Get_Weight = Val(ret)
                                    ScaleDisplay = Val(ret)
                                End If
                            End If
                        End If

                    Case ScaleModels.FairbanksUltegraBenchScale
                        Dim scaleArr() As String

                        If Serial_Get_ScaleResponse(Chr(13), "", ret, errDesc) Then
                            ScaleDisplay = ret
                            If ret <> "" Then
                                scaleArr = Split(ret, ".")
                                If UBound(scaleArr) > 0 Then
                                    lb = Val(scaleArr(0))
                                    oz = Val(scaleArr(1))
                                    'oz = Round((oz * 16) / 100, 1)
                                    Get_Weight = Convert.ToString(lb) & "." & Convert.ToString(oz) & " lb"
                                End If
                            End If
                        End If

                    Case ScaleModels.Toledo8213BenchScale, ScaleModels.Toledo8213BenchScale2
                        Dim isGotWeight As Boolean = False
                        If Model = ScaleModels.Toledo8213BenchScale Then
                            isGotWeight = Serial_Get_ScaleResponse("W", Chr(13), ret, errDesc)
                        Else
                            isGotWeight = Serial_Get_ScaleResponse("W" & Chr(13), Chr(13), ret, errDesc)
                        End If
                        If isGotWeight Then
                            iloc = Strings.InStr(1, ret, Chr(2))
                            If iloc > 0 Then
                                ret = Strings.Mid(ret, 2, Strings.Len(ret) - 1)
                            End If
                            If Not (ret = "" And Not StopScale) Then
                                If Strings.InStr(1, ret, "?") = 0 Then
                                    Get_Weight = Strings.Format(Val(ret), "0.00")
                                    ScaleDisplay = ret
                                End If
                            End If
                        End If

                    Case ScaleModels.NCI7720_7620PostalScale
                        If Serial_Get_ScaleResponse("W" & Chr(13), Chr(3), ret, errDesc) Then
                            iloc = Strings.InStr(1, ret, Chr(3))
                            If iloc > 0 And Strings.Len(ret) > 2 Then
                                iloc = Strings.InStr(1, ret, Chr(13))
                                If iloc > 0 Then
                                    ret = Strings.Mid(ret, 1, iloc - 1)
                                    ret = Replace(ret, Chr(32), "") ''space (SBCS)
                                    ret = Replace(ret, Chr(10), "") ''linefeed
                                Else
                                    ret = Strings.Trim(Strings.Mid(ret, 2))
                                End If
                                lb = 0#
                                oz = 0#
                                iloc = Strings.InStr(1, ret, "lb")
                                If iloc > 0 Then
                                    lb = Val(Strings.Mid(ret, 1, iloc - 1))
                                    ret = Strings.Mid(ret, iloc + 2)
                                End If
                                iloc = Strings.InStr(1, ret, "oz")
                                If iloc > 0 Then
                                    oz = Val(Strings.Mid(ret, 1, iloc - 1))
                                End If
                                wt = lb + (oz / 16)
                            Else
                                wt = 0
                            End If
                            Get_Weight = wt
                            ScaleDisplay = wt
                        End If

                    Case Else
                        StopScale = True ' disable

                End Select

                If String.IsNullOrEmpty(errDesc) Then ' no other errors
                    If Val(Get_Weight) > Val(WeightLimit) Then
                        Throw New Exception("Over Weight Limit" & vbCrLf & "Weight '" & Get_Weight & "' is over Weight Limit '" & WeightLimit & "'")
                    End If
                End If

            Catch ex As Exception
                errDesc = ex.Message
                Debug.Write(errDesc)
                Return ""
            End Try

        End Function

        Public Function Serial_Get_ScaleResponse(attnCmd As String, endString As String, ByRef retScaleResp As String, Optional ByRef errDesc As String = "") As Boolean
            Dim ok2go As Boolean
            Dim errNum As Long = 0

            retScaleResp = ""
            errDesc = ""

            Try

                ok2go = Serial_Read_ComPort(attnCmd, endString, retScaleResp, errDesc)

            Catch ex As Exception
                If Not 0 = Err.Number Then
                    errNum = Err.Number
                    errDesc = Err.Description
                End If
            End Try

            Serial_Get_ScaleResponse = ok2go And 0 = errNum And (Not 0 = retScaleResp.Length)

        End Function

        Private Function Serial_Read_ComPort(attnCmd As String, endString As String, ByRef retScaleResp As String, Optional ByRef errDesc As String = "") As Boolean

            Dim com As SerialPort = Nothing
            Dim timeOut As Double
            retScaleResp = "" ' reset
            errDesc = "" ' reset
            Serial_Read_ComPort = False

            If Serial_PortSettings_IsValid() Then ' valid scale settings entered
                If My.Computer.Ports.SerialPortNames.IndexOf(Serial_Port) >= 0 Then ' port found

                    Try
                        ' open port
                        com = New SerialPort(Serial_Port, Serial_Speed_Com, Serial_Parity_Com, Serial_DataBits_Com, Serial_StopBits_Com)
                        com.Open()

                        If com.IsOpen Then
                            com.ReadTimeout = 5000 ' 5 s
                            com.Write(attnCmd) ' send attention command to port

                            timeOut = Timer + 10 ' 10 s

                            ' wait for data to come back from port
                            Do
                                Dim incoming As String = com.ReadExisting()
                                If incoming Is Nothing Then
                                    Exit Do
                                Else
                                    retScaleResp &= incoming
                                End If

                                Forms.Application.DoEvents()
                                If retScaleResp.Contains(endString) Then
                                    Serial_Read_ComPort = True
                                    Exit Do
                                ElseIf timeOut < Timer Then
                                    errDesc = "Serial Port read timed out without valid data."
                                    Exit Do
                                End If
                            Loop Until StopScale ' Or timeOut < Timer
                        End If

                    Catch ex As TimeoutException
                        errDesc = "Serial Port read timed out without reponse."
                    Catch ex As Exception
                        errDesc = "Serial Port communication error: " & Err.Description
                    Finally
                        If com IsNot Nothing Then com.Close()
                    End Try

                Else
                    errDesc = "Serial COM port (" & Serial_Port & ") not found on system."
                End If
            Else
                errDesc = "Serial COM port configuration settings invalid."
            End If

            If Not Serial_Read_ComPort Then retScaleResp = ""

        End Function

    End Class

    Namespace SerialScale

        Public Class ScaleModels
            Public Const DetectoAS350D As String = "Detecto AS-350D" ' serial
            Public Const FairbanksSCB2453Series As String = "Fairbanks SCB-2453 Series" ' serial
            Public Const FairbanksUltegraBenchScale As String = "Fairbanks Ultegra Bench Scale" ' serial
            Public Const ToledoPS60 As String = "Toledo PS-60" ' serial
            Public Const ToledoPS6L As String = "Toledo PS-6L" ' serial
            Public Const Toledo8213BenchScale As String = "Toledo 8213 Bench Scale" ' serial
            Public Const Toledo8213BenchScale2 As String = "Toledo 8213 Bench Scale - 2" ' serial

            Public Shared ReadOnly Property List As List(Of String)
                Get
                    Return New List(Of String) From {
                        DetectoAS350D,
                        FairbanksSCB2453Series,
                        FairbanksUltegraBenchScale,
                        ToledoPS60,
                        ToledoPS6L,
                        Toledo8213BenchScale,
                        Toledo8213BenchScale2
                    }
                End Get
            End Property
        End Class

        Public Class ScaleSpeed
            Public Const SixHundred As String = "600"
            Public Const OneThousandTwoHundred As String = "1200"
            Public Const TwoThousandFourHundred As String = "2400"
            Public Const FourThousandEightHundred As String = "4800"
            Public Const NineThousandSixHundred As String = "9600"
            Public Const NineteenThousandTwoHundred As String = "19200"
            Public Shared ReadOnly Property List As List(Of String)
                Get
                    Return New List(Of String) From {
                        SixHundred,
                        OneThousandTwoHundred,
                        TwoThousandFourHundred,
                        FourThousandEightHundred,
                        NineThousandSixHundred,
                        NineteenThousandTwoHundred
                    }
                End Get
            End Property

            Public Shared Function ConvertToInt(sSpeed As String) As Integer
                If IsNumeric(sSpeed) Then
                    Try
                        Dim s As Integer = CInt(sSpeed)
                        Return s
                    Catch ex As Exception
                    End Try
                End If
                Return 0
            End Function
        End Class

        Public Class ScaleParity
            Public Const E As String = "E"
            Public Const O As String = "O"
            Public Const N As String = "N"
            Public Shared ReadOnly Property List As List(Of String)
                Get
                    Return New List(Of String) From {
                        E,
                        O,
                        N
                    }
                End Get
            End Property

            Public Shared Function ConvertToParity(sParity As String) As Parity
                Select Case sParity
                    Case ScaleParity.E
                        Return Parity.Even
                    Case ScaleParity.O
                        Return Parity.Odd
                    Case ScaleParity.N
                        Return Parity.None
                    Case Else
                        Return Parity.None
                End Select
            End Function
        End Class

        Public Class ScaleDataBit
            Public Const Seven As String = "7"
            Public Const Eight As String = "8"

            Public Shared ReadOnly Property List As List(Of String)
                Get
                    Return New List(Of String) From {
                        Seven,
                        Eight
                    }
                End Get
            End Property

            Public Shared Function ConvertToInt(sDataBits As String) As Integer
                If IsNumeric(sDataBits) Then
                    Try
                        Dim d As Integer = CInt(sDataBits)
                        Return d
                    Catch ex As Exception
                    End Try
                End If
                Return 0
            End Function
        End Class

        Public Class ScaleStopBit
            Public Const One As String = "1"
            Public Const Two As String = "2"

            Public Shared ReadOnly Property List As List(Of String)
                Get
                    Return New List(Of String) From {
                        One,
                        Two
                    }
                End Get
            End Property

            Public Shared Function ConvertToStopBits(sStopBits As String) As StopBits
                Select Case sStopBits
                    Case ScaleStopBit.One
                        Return StopBits.One
                    Case ScaleStopBit.Two
                        Return StopBits.Two
                    Case Else
                        Return StopBits.None
                End Select
            End Function
        End Class

    End Namespace

    Namespace UsbScale

        Public Class ScaleModels
            Public Const FairbanksUltegraBenchScaleUSB As String = "Fairbanks Ultegra Bench Scale USB"
            Public Const ToledoPS60USB As String = "Toledo PS-60 USB"
            Public Const ToledoPS6LUSB As String = "Toledo PS-6L USB"
            Public Const NCI7720_7620PostalScale As String = "NCI 7720/7620 Postal Scale"

            Public Shared ReadOnly Property List As List(Of String)
                Get
                    Return New List(Of String) From {
                        FairbanksUltegraBenchScaleUSB,
                        ToledoPS60USB,
                        ToledoPS6LUSB,
                        NCI7720_7620PostalScale
                    }
                End Get
            End Property
        End Class

        Public Class ScaleData

            Public ReadOnly Property DeviceData As Byte()
            Public ReadOnly Property ReportId As UShort

            Public Enum ScaleStatus
                Fault
                StableAtZero
                InMotion
                WeightStable
                UnderZero
                OverWeight
                RequiresCalibration
                RequiresRezeroing
                RequiresGEO
                Unknown
            End Enum
            Private _Status As ScaleStatus
            Public ReadOnly Property Status As ScaleStatus
                Get
                    Return _Status
                End Get
            End Property

            Public Enum WeightUnit
                UnitMilligram
                UnitGram
                UnitKilogram
                UnitCarates
                UnitTaels
                UnitGrains
                UnitPennyweights
                UnitMetricTon
                UnitAvoirTon
                UnitTroyOunce
                UnitOunce
                UnitPound
                UnitUnknown
            End Enum
            Private _Unit As WeightUnit
            Public ReadOnly Property Unit As WeightUnit
                Get
                    Return _Unit
                End Get
            End Property

            Private _Scaling As SByte
            Private _WeightLsb As UShort
            Private _WeightMsb As UShort
            Private _Weight As Double
            Public ReadOnly Property Weight As Double
                Get
                    Return _Weight
                End Get
            End Property
            Public ReadOnly Property Weight_Ounces As Double
                Get
                    Select Case _Unit
                        Case WeightUnit.UnitOunce
                            'good
                            Return _Weight
                        Case WeightUnit.UnitPound
                            Return _Convert.Pounds2Ounces(_Weight)
                        Case Else
                            Return _Weight
                    End Select
                End Get
            End Property

            Public Property IsError As Boolean
            Private _ErrorMessage As String = String.Empty
            Public ReadOnly Property ErrorMessage As String
                Get
                    Return _ErrorMessage
                End Get
            End Property

            Public Sub New(usb_data As Byte())
                DeviceData = GetDevData(usb_data)
            End Sub

            Private Function Convert_Byte_To_ScaleStatus(b As Byte) As ScaleStatus
                Select Case b
                    Case &H1 : Return ScaleStatus.Fault
                    Case &H2 : Return ScaleStatus.StableAtZero
                    Case &H3 : Return ScaleStatus.InMotion
                    Case &H4 : Return ScaleStatus.WeightStable
                    Case &H5 : Return ScaleStatus.UnderZero
                    Case &H6 : Return ScaleStatus.OverWeight
                    Case &H7 : Return ScaleStatus.RequiresCalibration
                    Case &H8 : Return ScaleStatus.RequiresRezeroing
                    Case &H9 : Return ScaleStatus.RequiresGEO
                    Case Else : Return ScaleStatus.Unknown
                End Select
            End Function
            Private Function Convert_Byte_To_WeightUnit(b As Byte) As WeightUnit
                Select Case b
                    Case &H1 : Return WeightUnit.UnitMilligram
                    Case &H2 : Return WeightUnit.UnitGram
                    Case &H3 : Return WeightUnit.UnitKilogram
                    Case &H4 : Return WeightUnit.UnitCarates
                    Case &H5 : Return WeightUnit.UnitTaels
                    Case &H6 : Return WeightUnit.UnitGrains
                    Case &H7 : Return WeightUnit.UnitPennyweights
                    Case &H8 : Return WeightUnit.UnitMetricTon
                    Case &H9 : Return WeightUnit.UnitAvoirTon
                    Case &HA : Return WeightUnit.UnitTroyOunce
                    Case &HB : Return WeightUnit.UnitOunce
                    Case &HC : Return WeightUnit.UnitPound
                    Case Else : Return WeightUnit.UnitUnknown
                End Select
            End Function

            Private Function Convert_Byte_To_ScalingSByte(b As Byte) As SByte
                Return IIf(b < 128, b, b - 256)
            End Function

            Private Function Get_WeightValue() As Double
                'Return CType((_weightMsb * 256 + _weightLsb) * Math.Pow(10, _scaling), Double)
                Return CType(BitConverter.ToInt16(New Byte() {_WeightLsb, _WeightMsb}, 0) * Math.Pow(10, _Scaling), Double)
            End Function

            Private Function GetDevData(data As Byte()) As Byte()

                If data.Length = 5 Then
                    Array.Resize(data, data.Length + 1)
                    Array.Copy(data, 0, data, 1, 5)
                    data(0) = 0
                End If
                If data IsNot Nothing AndAlso data.Length = 6 Then

                    Try
                        _ReportId = data(0)
                        _Status = Convert_Byte_To_ScaleStatus(data(1))
                        _Unit = Convert_Byte_To_WeightUnit(data(2))
                        _Scaling = Convert_Byte_To_ScalingSByte(data(3))
                        _WeightLsb = data(4)
                        _WeightMsb = data(5)
                        _Weight = Get_WeightValue()

                        Return data
                    Catch ex As Exception
                        _ErrorMessage = "Error reading/converting data: " & Err.Description
                    End Try
                Else
                    _ErrorMessage = "Data length is invalid."
                End If

                IsError = True
                Return Nothing
            End Function
        End Class

        Public Class ScaleReader

            Public Property VendorId As Integer ' Hex values
            Public Property ProductId As Integer ' Hex values

            Public Event DeviceAttached As AttachedEventHandler
            Public Delegate Sub AttachedEventHandler()
            Public Event DeviceRemoved As RemovedEventHandler
            Public Delegate Sub RemovedEventHandler()

            Private ScaleDevice As HidDevice
            Private _IsError As Boolean
            Private _Scale_Data As ScaleData
            Public ReadOnly Property Scale_Data As ScaleData
                Get
                    Return _Scale_Data
                End Get
            End Property

            Public ReadOnly Property IsConnected As Boolean
                Get
                    If ScaleDevice IsNot Nothing Then
                        Return ScaleDevice.IsConnected
                    End If
                    Return False
                End Get
            End Property

            Private Function Connect(Optional ByRef errDesc As String = "") As Boolean
                Dim device As HidDevice = HidDevices.Enumerate(VendorId, ProductId).FirstOrDefault()
                If device IsNot Nothing Then
                    If Connect(device) Then
                        Return True
                    Else
                        errDesc = "Failed to connect to HID device."
                    End If
                Else
                    errDesc = "Failed to locate HID device."
                End If
                Return False
            End Function

            Private Function Connect(device As HidDevice) As Boolean
                ScaleDevice = device
                Dim waitTries As Integer = 0
                ScaleDevice.OpenDevice()

                While IsConnected AndAlso waitTries < 10
                    Threading.Thread.Sleep(50)
                    waitTries += 1
                End While

                Return IsConnected
            End Function

            Private Sub Disconnect()
                If IsConnected Then
                    ScaleDevice.CloseDevice()
                    ScaleDevice.Dispose()
                End If
            End Sub

            Public Sub DeviceMonitoring_Start()
                If Connect() Then
                    AddHandler ScaleDevice.Inserted, AddressOf DeviceAttachedHandler
                    AddHandler ScaleDevice.Removed, AddressOf DeviceRemovedHandler
                    ScaleDevice.MonitorDeviceEvents = True
                End If
                Disconnect()
            End Sub

            Public Sub DeviceMonitoring_Stop()
                If IsConnected Then
                    RemoveHandler ScaleDevice.Inserted, AddressOf DeviceAttachedHandler
                    RemoveHandler ScaleDevice.Removed, AddressOf DeviceRemovedHandler
                End If
                ScaleDevice.MonitorDeviceEvents = False
            End Sub

            Public Function Read_ScaleWeight(ByRef retWeight As String, Optional ByRef errDesc As String = "") As Boolean

                Try
                    retWeight = ""
                    errDesc = ""
                    ReadDevice(errDesc)
                    If Not _IsError Then
                        retWeight = Scale_Data.Weight_Ounces.ToString
                    End If

                Catch ex As Exception
                    errDesc = Err.Description
                End Try

                Return (Not 0 = retWeight.Length) And (0 = errDesc.Length)

            End Function

            Public Sub ReadDevice(Optional ByRef errDesc As String = "")

                errDesc = ""
                Try
                    If Connect(errDesc) Then
                        Dim scaleData As HidDeviceData = ScaleDevice.Read(5)
                        If IsReadDeviceSuccess(scaleData.Status, errDesc) Then
                            OnDeviceData(scaleData)

                            If Scale_Data IsNot Nothing AndAlso Scale_Data.IsError Then
                                errDesc = Scale_Data.ErrorMessage
                            End If
                        End If
                    End If
                Catch ex As Exception
                    errDesc = Err.Description
                Finally
                    Disconnect()
                End Try

                _IsError = Not errDesc.Length = 0

            End Sub

            Private Function IsReadDeviceSuccess(sData As HidDeviceData.ReadStatus, ByRef errDesc As String) As Boolean
                errDesc = ""
                Select Case sData
                    Case HidDeviceData.ReadStatus.Success
                        Return True
                    Case HidDeviceData.ReadStatus.NoDataRead
                        errDesc = "No data read from device."
                    Case HidDeviceData.ReadStatus.NotConnected
                        errDesc = "Device not connected."
                    Case HidDeviceData.ReadStatus.ReadError
                        errDesc = "Device read error encountered."
                    Case HidDeviceData.ReadStatus.WaitFail
                        errDesc = "Device wait failed."
                    Case HidDeviceData.ReadStatus.WaitTimedOut
                        errDesc = "Device time-out interval elapsed."
                    Case Else
                        errDesc = "Unknown error occurred."
                End Select
                Return False
            End Function

            Public Sub ReadDeviceAsync(Optional timeout As Integer = 0, Optional ByRef errDesc As String = "")

                errDesc = ""
                Try
                    If Connect(errDesc) Then
                        If timeout < 0 Then timeout = 0
                        ScaleDevice.Read(AddressOf OnDeviceData, timeout)
                    End If
                Catch ex As Exception
                    errDesc = Err.Description
                Finally
                    Disconnect()
                End Try

            End Sub

            Private Sub OnDeviceData(deviceData As HidDeviceData)

                If Not ScaleDevice.IsConnected Then Return
                If deviceData Is Nothing Then Return

                _Scale_Data = New ScaleData(deviceData.Data)

            End Sub

            Private Sub DeviceAttachedHandler()
                Debug.WriteLine("Device attached.")
                RaiseEvent DeviceAttached()
            End Sub

            Private Sub DeviceRemovedHandler()
                Debug.WriteLine("Device removed.")
                RaiseEvent DeviceRemoved()
            End Sub

        End Class

    End Namespace

End Namespace
