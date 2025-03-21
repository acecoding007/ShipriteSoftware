Module BarCode1
#Region "Behaviors"
    Dim unknown As Func(Of String, String()) = Function(tracking As String) {tracking, String.Empty}
    Dim complex As Func(Of String, String()) = Function(tracking As String) {tracking, complexParse(tracking)}

    Dim SpeeDee As Func(Of String, String()) = Function(tracking As String) {tracking, "SpeeDee"}
    Dim DHL As Func(Of String, String()) = Function(tracking As String) {tracking, "DHL"}

    Dim UPSGround As Func(Of String, String()) = Function(tracking As String) {tracking, "UPS Ground"}
    Dim UPSExpress As Func(Of String, String()) = Function(tracking As String) {tracking, "UPS Express"}

    Dim USPSRight22 As Func(Of String, String()) = Function(tracking As String) {_Controls.Right(tracking, 22), "USPS"}
    Dim USPS As Func(Of String, String()) = Function(tracking) {tracking, "USPS"}

    Dim FedExGround As Func(Of String, String()) = Function(tracking As String) {tracking, "FedEx Ground"}
    Dim FedExExpress As Func(Of String, String()) = Function(tracking As String) {tracking, "FedEx Express"}
    Dim FedExXpRight15 As Func(Of String, String()) = Function(tracking As String) {_Controls.Right(tracking, 15), "FedEx Express"}
    Dim FedExXpRight12 As Func(Of String, String()) = Function(tracking As String) {_Controls.Right(tracking, 12), "FedEx Express"}
    Dim FedExGroundRight15 As Func(Of String, String()) = Function(tracking As String) {_Controls.Right(tracking, 15), "FedEx Ground"}
    Dim FedExGroundRight12 As Func(Of String, String()) = Function(tracking As String) {_Controls.Right(tracking, 12), "FedEx Ground"}

#End Region
    ' First 2 digits
    Private pages() As String = {"92", "1Z", "96", "SP", "10", "90", "42", "other"}
    ' characters 8&9, 0-indexed
    Private cols() As String = {"03", "20", "22", "42", "72", "78", "90", "A8", "other"}
    ' Length of tracking number
    Private rows() As String =
                          {"10",
                          "<13",
                          "13",
                          "16",
                          "21",
                          "22",
                          "30",
                          "32",
                          "34",
                          "other"}
    Private ruleTable(,,) =
        {
                {
                    {DHL, DHL, DHL, DHL, DHL, DHL, DHL, DHL, DHL},
                    {complex, complex, complex, complex, complex, complex, complex, complex, complex},
                    {USPS, USPS, USPS, USPS, USPS, USPS, USPS, USPS, USPS},
                    {FedExXpRight12, FedExXpRight12, FedExXpRight12, FedExXpRight12, FedExXpRight12, FedExXpRight12, FedExXpRight12, FedExXpRight12, FedExXpRight12},
                    {FedExXpRight15, FedExXpRight15, FedExXpRight15, FedExXpRight15, FedExXpRight15, FedExXpRight15, FedExXpRight15, FedExXpRight15, FedExXpRight15},
                    {FedExXpRight15, FedExXpRight15, FedExXpRight15, FedExXpRight15, FedExXpRight15, FedExXpRight15, FedExXpRight15, FedExXpRight15, FedExXpRight15},
                    {USPSRight22, USPSRight22, USPSRight22, USPSRight22, USPSRight22, USPSRight22, USPSRight22, USPSRight22, USPSRight22},
                    {FedExExpress, FedExExpress, FedExExpress, FedExExpress, FedExExpress, FedExExpress, FedExExpress, FedExExpress, FedExExpress},
                    {FedExXpRight12, FedExXpRight12, FedExXpRight12, FedExXpRight12, FedExXpRight12, FedExXpRight12, FedExXpRight12, FedExXpRight12, FedExXpRight12},
                    {complex, complex, complex, complex, complex, complex, complex, complex, complex}
                },
                {
                    {UPSGround, UPSGround, UPSGround, UPSGround, UPSGround, UPSGround, UPSGround, UPSGround, UPSExpress},
                    {UPSGround, UPSGround, UPSGround, UPSGround, UPSGround, UPSGround, UPSGround, UPSGround, UPSExpress},
                    {UPSGround, UPSGround, UPSGround, UPSGround, UPSGround, UPSGround, UPSGround, UPSGround, UPSExpress},
                    {UPSGround, UPSGround, UPSGround, UPSGround, UPSGround, UPSGround, UPSGround, UPSGround, UPSExpress},
                    {UPSGround, UPSGround, UPSGround, UPSGround, UPSGround, UPSGround, UPSGround, UPSGround, UPSExpress},
                    {UPSGround, UPSGround, UPSGround, UPSGround, UPSGround, UPSGround, UPSGround, UPSGround, UPSExpress},
                    {UPSGround, UPSGround, UPSGround, UPSGround, UPSGround, UPSGround, UPSGround, UPSGround, UPSExpress},
                    {UPSGround, UPSGround, UPSGround, UPSGround, UPSGround, UPSGround, UPSGround, UPSGround, UPSExpress},
                    {UPSGround, UPSGround, UPSGround, UPSGround, UPSGround, UPSGround, UPSGround, UPSGround, UPSExpress},
                    {UPSGround, UPSGround, UPSGround, UPSGround, UPSGround, UPSGround, UPSGround, UPSGround, UPSExpress}
                },
                {
                    {DHL, DHL, DHL, DHL, DHL, DHL, DHL, DHL, DHL},
                    {complex, complex, complex, complex, complex, complex, complex, complex, complex},
                    {USPS, USPS, USPS, USPS, USPS, USPS, USPS, USPS, USPS},
                    {FedExXpRight12, FedExXpRight12, FedExXpRight12, FedExXpRight12, FedExXpRight12, FedExXpRight12, FedExXpRight12, FedExXpRight12, FedExXpRight12},
                    {complex, complex, complex, complex, complex, complex, complex, complex, complex},
                    {FedExXpRight15, FedExXpRight15, FedExXpRight15, FedExXpRight15, FedExXpRight15, FedExXpRight15, FedExXpRight15, FedExXpRight15, FedExXpRight15},
                    {USPSRight22, USPSRight22, USPSRight22, USPSRight22, USPSRight22, USPSRight22, USPSRight22, USPSRight22, USPSRight22},
                    {FedExGround, FedExGround, FedExGround, FedExGround, FedExGround, FedExGround, FedExGround, FedExGround, FedExGround},
                    {FedExGroundRight12, FedExGroundRight12, FedExGroundRight12, FedExGroundRight12, FedExGroundRight12, FedExGroundRight12, FedExGroundRight12, FedExGroundRight12, FedExGroundRight12},
                    {complex, complex, complex, complex, complex, complex, complex, complex, complex}
                },
                {
                    {SpeeDee, SpeeDee, SpeeDee, SpeeDee, SpeeDee, SpeeDee, SpeeDee, SpeeDee, SpeeDee},
                    {SpeeDee, SpeeDee, SpeeDee, SpeeDee, SpeeDee, SpeeDee, SpeeDee, SpeeDee, SpeeDee},
                    {SpeeDee, SpeeDee, SpeeDee, SpeeDee, SpeeDee, SpeeDee, SpeeDee, SpeeDee, SpeeDee},
                    {SpeeDee, SpeeDee, SpeeDee, SpeeDee, SpeeDee, SpeeDee, SpeeDee, SpeeDee, SpeeDee},
                    {SpeeDee, SpeeDee, SpeeDee, SpeeDee, SpeeDee, SpeeDee, SpeeDee, SpeeDee, SpeeDee},
                    {SpeeDee, SpeeDee, SpeeDee, SpeeDee, SpeeDee, SpeeDee, SpeeDee, SpeeDee, SpeeDee},
                    {SpeeDee, SpeeDee, SpeeDee, SpeeDee, SpeeDee, SpeeDee, SpeeDee, SpeeDee, SpeeDee},
                    {SpeeDee, SpeeDee, SpeeDee, SpeeDee, SpeeDee, SpeeDee, SpeeDee, SpeeDee, SpeeDee},
                    {SpeeDee, SpeeDee, SpeeDee, SpeeDee, SpeeDee, SpeeDee, SpeeDee, SpeeDee, SpeeDee},
                    {SpeeDee, SpeeDee, SpeeDee, SpeeDee, SpeeDee, SpeeDee, SpeeDee, SpeeDee, SpeeDee}
                },
                {
                    {DHL, DHL, DHL, DHL, DHL, DHL, DHL, DHL, DHL},
                    {complex, complex, complex, complex, complex, complex, complex, complex, complex},
                    {USPS, USPS, USPS, USPS, USPS, USPS, USPS, USPS, USPS},
                    {FedExXpRight12, FedExXpRight12, FedExXpRight12, FedExXpRight12, FedExXpRight12, FedExXpRight12, FedExXpRight12, FedExXpRight12, FedExXpRight12},
                    {complex, complex, complex, complex, complex, complex, complex, complex, complex},
                    {FedExXpRight15, FedExXpRight15, FedExXpRight15, FedExXpRight15, FedExXpRight15, FedExXpRight15, FedExXpRight15, FedExXpRight15, FedExXpRight15},
                    {USPSRight22, USPSRight22, USPSRight22, USPSRight22, USPSRight22, USPSRight22, USPSRight22, USPSRight22, USPSRight22},
                    {FedExExpress, FedExExpress, FedExExpress, FedExExpress, FedExExpress, FedExExpress, FedExExpress, FedExExpress, FedExExpress},
                    {FedExXpRight12, FedExXpRight12, FedExXpRight12, FedExXpRight12, FedExXpRight12, FedExXpRight12, FedExXpRight12, FedExXpRight12, FedExXpRight12},
                    {complex, complex, complex, complex, complex, complex, complex, complex, complex}
                },
                {
                    {DHL, DHL, DHL, DHL, DHL, DHL, DHL, DHL, DHL},
                    {complex, complex, complex, complex, complex, complex, complex, complex, complex},
                    {USPS, USPS, USPS, USPS, USPS, USPS, USPS, USPS, USPS},
                    {FedExXpRight12, FedExXpRight12, FedExXpRight12, FedExXpRight12, FedExXpRight12, FedExXpRight12, FedExXpRight12, FedExXpRight12, FedExXpRight12},
                    {FedExXpRight15, FedExXpRight15, FedExXpRight15, FedExXpRight15, FedExXpRight15, FedExXpRight15, FedExXpRight15, FedExXpRight15, FedExXpRight15},
                    {FedExXpRight15, FedExXpRight15, FedExXpRight15, FedExXpRight15, FedExXpRight15, FedExXpRight15, FedExXpRight15, FedExXpRight15, FedExXpRight15},
                    {USPSRight22, USPSRight22, USPSRight22, USPSRight22, USPSRight22, USPSRight22, USPSRight22, USPSRight22, USPSRight22},
                    {FedExExpress, FedExExpress, FedExExpress, FedExExpress, FedExExpress, FedExExpress, FedExExpress, FedExExpress, FedExExpress},
                    {FedExXpRight12, FedExXpRight12, FedExXpRight12, FedExXpRight12, FedExXpRight12, FedExXpRight12, FedExXpRight12, FedExXpRight12, FedExXpRight12},
                    {complex, complex, complex, complex, complex, complex, complex, complex, complex}
                },
                {
                    {DHL, DHL, DHL, DHL, DHL, DHL, DHL, DHL, DHL},
                    {complex, complex, complex, complex, complex, complex, complex, complex, complex},
                    {USPS, USPS, USPS, USPS, USPS, USPS, USPS, USPS, USPS},
                    {FedExXpRight12, FedExXpRight12, FedExXpRight12, FedExXpRight12, FedExXpRight12, FedExXpRight12, FedExXpRight12, FedExXpRight12, FedExXpRight12},
                    {complex, complex, complex, complex, complex, complex, complex, complex, complex},
                    {complex, complex, complex, complex, complex, complex, complex, complex, complex},
                    {USPSRight22, USPSRight22, USPSRight22, USPSRight22, USPSRight22, USPSRight22, USPSRight22, USPSRight22, USPSRight22},
                    {FedExExpress, FedExExpress, FedExExpress, FedExExpress, FedExExpress, FedExExpress, FedExExpress, FedExExpress, FedExExpress},
                    {USPS, USPS, USPS, USPS, USPS, USPS, USPS, USPS, USPS},
                    {complex, complex, complex, complex, complex, complex, complex, complex, complex}
                },
                {
                    {DHL, DHL, DHL, DHL, DHL, DHL, DHL, DHL, DHL},
                    {complex, complex, complex, complex, complex, complex, complex, complex, complex},
                    {USPS, USPS, USPS, USPS, USPS, USPS, USPS, USPS, USPS},
                    {FedExXpRight12, FedExXpRight12, FedExXpRight12, FedExXpRight12, FedExXpRight12, FedExXpRight12, FedExXpRight12, FedExXpRight12, FedExXpRight12},
                    {complex, complex, complex, complex, complex, complex, complex, complex, complex},
                    {complex, complex, complex, complex, complex, complex, complex, complex, complex},
                    {USPSRight22, USPSRight22, USPSRight22, USPSRight22, USPSRight22, USPSRight22, USPSRight22, USPSRight22, USPSRight22},
                    {FedExExpress, FedExExpress, FedExExpress, FedExExpress, FedExExpress, FedExExpress, FedExExpress, FedExExpress, FedExExpress},
                    {FedExXpRight12, FedExXpRight12, FedExXpRight12, FedExXpRight12, FedExXpRight12, FedExXpRight12, FedExXpRight12, FedExXpRight12, FedExXpRight12},
                    {complex, complex, complex, complex, complex, complex, complex, complex, complex}
                }
            }

    Public Function ShippingCo(ByRef trackingNumScanned As String) As String
        Dim left2 As String = _Controls.Left(trackingNumScanned, 2)
        Dim mid89 As String = _Controls.Mid(trackingNumScanned, 8, 2, False)
        Dim length As String = trackingNumScanned.Count

        If Array.IndexOf(pages, left2) = -1 Then
            left2 = "other"
        End If

        If Array.IndexOf(rows, length) = -1 Then
            If trackingNumScanned.Count < 13 Then
                length = "<13"
            Else
                length = "other"
            End If
        End If

        If Array.IndexOf(cols, mid89) = -1 Then
            mid89 = "other"
        End If


        Dim page = Array.IndexOf(pages, left2)
        Dim row = Array.IndexOf(rows, length)
        Dim col = Array.IndexOf(cols, mid89)

        Dim result As String() = CType(ruleTable(page, row, col), Func(Of String, String()))(trackingNumScanned)
        trackingNumScanned = result(0)
        Return result(1)
    End Function

    Private Function complexParse(ByVal trackingNum As String)
        Try
            trackingNum = trackingNum.PadRight(26, "0")
            ' Fedex?
            Dim fedexSum As Integer = 0
            ' 
            Dim fedexCheck = New Integer(12) {}
            For i = 0 To fedexCheck.Count - 1
                Select Case i Mod 3
                    Case 0
                        fedexCheck(i) = Integer.Parse(trackingNum.Chars(i + 13))
                    Case 1
                        fedexCheck(i) = Integer.Parse(trackingNum.Chars(i + 13)) * 7
                    Case 2
                        fedexCheck(i) = Integer.Parse(trackingNum.Chars(i + 13)) * 3
                End Select
                fedexSum += fedexCheck(i)
            Next
            Dim fedexMod11 = fedexSum Mod 11
            If fedexMod11 = 10 Then fedexMod11 = 0

            Dim checkNum As Integer = 0
            If trackingNum.Length >= 25 Then
                checkNum = Integer.Parse(trackingNum.Chars(25))
            End If
            If fedexMod11 = checkNum Then
                If _Controls.Left(trackingNum, 2) = "96" Then
                    Return "FedEx Ground"
                Else
                    Return "Fedex Express"
                End If
            End If
            ' If we're here, it's not fedex.

            Dim paddedReverseTracking As String = StrReverse(trackingNum.PadRight(26, "0"))

            Dim oddSum26 = 0
            For i = 1 To 25 Step 2
                oddSum26 += Integer.Parse(paddedReverseTracking.Chars(i))
            Next
            oddSum26 *= 3
            Dim evenSum26 = 0
            For i = 2 To 24 Step 2
                evenSum26 += Integer.Parse(paddedReverseTracking.Chars(i))
            Next
            Dim sum26 As Integer = evenSum26 + oddSum26

            'Get the one's place of sum26
            Dim ones26 As Integer = ((sum26 / 10) * 10) - sum26

            Dim oddSum22 = 0
            For i = 1 To 21 Step 2
                oddSum22 += Integer.Parse(paddedReverseTracking.Chars(i))
            Next
            oddSum22 *= 3
            Dim evenSum22 = 0
            For i = 2 To 20 Step 2
                evenSum22 += Integer.Parse(paddedReverseTracking.Chars(i))
            Next
            Dim sum22 As Integer = evenSum22 + oddSum22

            'Get the one's place of sum222
            Dim ones22 As Integer = (Int(-sum22 / 10) * -10) - sum22

            If ones26 = Integer.Parse(paddedReverseTracking.Chars(0)) OrElse ones22 = Integer.Parse(paddedReverseTracking.Chars(0)) Then
                Return "USPS"
            End If

            ' If we got here, this algo won't figure it out.
            Debug.Print("Complex failed.")
            Return String.Empty
        Catch ex As Exception
            Debug.Print(ex.StackTrace)
            Return String.Empty
        End Try
    End Function
End Module
