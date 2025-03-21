
Imports System.Text
Imports System.IO
Imports System.Net

Module Genius

    Public Function GENIUS_processRETURN(InvoiceNumber As String, Amount As Double) As String

        Dim SQL As String
        Dim Segment As String
        Dim SegmentSet As String
        Dim XMLBody As String
        Dim Status As String
        Dim AuthCode As String
        Dim ct As Integer
        Dim Token As String
        Dim mname As String
        Dim CardType As String
        Dim fnum As Integer
        Dim ID As String
        Dim iloc As Integer
        Dim AmountCharged As Double
        Dim ReturnAmount As Double
        Dim amt As Double
        Dim ReturnedAmount As Double
        Dim buf As String = ""
        Dim Message As String = ""
        Dim TransType As String = ""
        Dim resString As String = ""
        Dim URI As String
        Dim ret As Long = 0
        Dim OrderID As String = ""
        Dim HistoryID As String = ""
        Dim ReferenceNumber As String = ""

        ReturnAmount = Amount
        ReturnedAmount = 0
        ct = 0

        SQL = "SELECT ID, [ApprovalNum], Payment FROM Payments WHERE InvNum = '" & InvoiceNumber & "' AND [ApprovalNum] LIKE '%/%' AND [Type] = 'CHARGE' ORDER BY ID"
        SegmentSet = IO_GetSegmentSet(gShipriteDB, SQL)
        If SegmentSet = "" Then

            Message = "Reference ID NOT FOUND"
            GENIUS_processRETURN = Message
            Exit Function

        End If
        ct += 1
        Segment = GetNextSegmentFromSet(SegmentSet)
        gCredentialSegment = GENIUS_GenerateCredentialSegment()
        ID = ExtractElementFromSegment("ID", Segment)
        AmountCharged = Val(ExtractElementFromSegment("Payment", Segment))
        Token = ExtractElementFromSegment("ApprovalNum", Segment)
        iloc = InStr(1, Token, "/")
        If Not iloc = 0 Then

            Token = Mid(Token, iloc)

        End If
        If AmountCharged <= ReturnAmount Then

            amt = AmountCharged
            ReturnAmount = ReturnAmount - AmountCharged

        Else

            amt = ReturnAmount
            ReturnAmount = 0

        End If
        Net.ServicePointManager.SecurityProtocol = Net.SecurityProtocolType.Tls12
        Using webClient As New Net.WebClient()

            '    Headers

            webClient.Headers("Content-Type") = "text/xml; charset=utf-"
            webClient.Headers("SOAPAction") = "http://schemas.merchantwarehouse.com/merchantware/40/Credit/Refund "
            Dim WebHeaders As String = webClient.Headers.ToString
            ret = WriteFile_ToEnd(WebHeaders, "c:\shipritenext\XML_HEADERS.TXT")

            TransType = "REFUND"
            mname = ExtractElementFromSegment("MerchantName", gCredentialSegment)
            mname = FlushOut(mname, "~", "&amp;")

            URI = "https://ps1.merchantware.net/Merchantware/ws/RetailTransaction/v4/Credit.asmx"

            XMLBody = ""
            XMLBody = XMLBody & "<?xml version=""1.0"" encoding=""utf-8""?>" & vbCrLf
            XMLBody = XMLBody & "<soap:Envelope "
            XMLBody = XMLBody & "  xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""" & vbCrLf
            XMLBody = XMLBody & "  xmlns:xsd=""http://www.w3.org/2001/XMLSchema""" & vbCrLf
            XMLBody = XMLBody & "  xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/""" & ">" & vbCrLf
            XMLBody = XMLBody & " <soap:Body>" & vbCrLf
            XMLBody = XMLBody & "  <Refund xmlns=""http://schemas.merchantwarehouse.com/merchantware/40/Credit/"">" & vbCrLf
            XMLBody = XMLBody & "    <merchantName>" & mname & "</merchantName>" & vbCrLf
            XMLBody = XMLBody & "    <merchantSiteId>" & ExtractElementFromSegment("SiteID", gCredentialSegment) & "</merchantSiteId>" & vbCrLf
            XMLBody = XMLBody & "    <merchantKey>" & ExtractElementFromSegment("MerchantKey", gCredentialSegment) & "</merchantKey>" & vbCrLf
            XMLBody = XMLBody & "    <invoiceNumber>" & InvoiceNumber & "</invoiceNumber>" & vbCrLf
            XMLBody = XMLBody & "    <token>" & Token & "</token>" & vbCrLf
            XMLBody = XMLBody & "    <overrideAmount>" & amt & "</overrideAmount>" & vbCrLf
            XMLBody = XMLBody & "    <registerNumber></registerNumber>" & vbCrLf
            XMLBody = XMLBody & "    <merchantTransactionId></merchantTransactionId>" & vbCrLf
            XMLBody = XMLBody & "  </Refund>" & vbCrLf
            XMLBody = XMLBody & " </soap:Body>" & vbCrLf
            XMLBody = XMLBody & "</soap:Envelope>" & vbCrLf & vbCrLf

            fnum = FreeFile()
            FileOpen(fnum, "c:\SmartSwiper\XML_History.txt", OpenMode.Append)
            FileSystem.Print(fnum, "" & vbCrLf)
            FileSystem.Print(fnum, "######################################### [Request Transaction Return] #########################" & vbCrLf)
            FileSystem.Print(fnum, "" & vbCrLf)
            FileSystem.Print(fnum, Format$(Today, "MM/dd/yyyy") & " " & Format$(Now, "hh:mm:ss" & gInvoiceNumber & vbCrLf))
            FileSystem.Print(fnum, "--------XML REQUEST--------------------------------------------------------" & vbCrLf)
            FileSystem.Print(fnum, XMLBody & vbCrLf)
            FileSystem.Print(fnum, "" & vbCrLf)
            FileSystem.FileClose(fnum)

            Try

                resString = webClient.UploadString(URI, "POST", XMLBody)

            Catch ex As Exception

                _MsgBox.ErrorMessage(ex, "Error processing Credit Card Refund.")
                FileOpen(fnum, "c:\SmartSwiper\XML_History.txt", OpenMode.Append)
                FileSystem.Print(fnum, "" & vbCrLf)
                FileSystem.Print(fnum, "######################################### [Request Transaction Return] #########################" & vbCrLf)
                FileSystem.Print(fnum, "" & vbCrLf)
                FileSystem.Print(fnum, Format$(Today, "MM/dd/yyyy") & " " & Format$(Now, "hh:mm:ss" & gInvoiceNumber & vbCrLf))
                FileSystem.Print(fnum, "--------FAILED--------------------------------------------------------" & vbCrLf)
                FileSystem.Print(fnum, ex.Message & vbCrLf & "Error processing Credit Card Refund" & vbCrLf)
                FileSystem.Print(fnum, "" & vbCrLf)
                FileSystem.FileClose(fnum)
                Return False

            End Try


            FileOpen(fnum, "c:\SmartSwiper\XML_History.txt", OpenMode.Append)
            FileSystem.Print(fnum, "" & vbCrLf)
            FileSystem.Print(fnum, "######################################### [Return Response] #########################" & vbCrLf)
            FileSystem.Print(fnum, "" & vbCrLf)
            FileSystem.Print(fnum, Format$(Today, "MM/dd/yyyy") & " " & Format$(Now, "hh:mm:ss" & gInvoiceNumber & vbCrLf))
            FileSystem.Print(fnum, "--------XML REQUEST--------------------------------------------------------" & vbCrLf)
            FileSystem.Print(fnum, resString & vbCrLf)
            FileSystem.Print(fnum, "" & vbCrLf)
            FileSystem.FileClose(fnum)

        End Using

        buf = XML_GetNode(resString, "ApprovalStatus")
        If Not UCase(buf) = "APPROVED" Then

            buf = XML_GetNode(resString, "authcode")
            iloc = InStr(1, buf, ":")
            If Not iloc = 0 Then

                buf = Mid(buf, iloc + 1)
                iloc = InStr(1, buf, ":")
                If Not iloc = 0 Then

                    buf = Mid(buf, iloc + 1)

                End If

            Else

                buf = XML_GetNode(resString, "ApprovalStatus")
                If buf = "" Then

                    buf = XML_GetNode(resString, "ErrorMessage")

                End If
                If buf = "" Then

                    buf = "UNKNOWN ERROR"

                End If

            End If
            GENIUS_processRETURN = buf
            Exit Function

        Else

            buf = resString
            gCreditCardSegment = buf
            Status = XML_GetNode(resString, "ApprovalStatus")
            OrderID = XML_GetNode(resString, "InvoiceNumber")
            HistoryID = XML_GetNode(resString, "Token")
            Amount = Val(XML_GetNode(resString, "Amount"))
            MsgBox("APPROVED AMOUNT #" & ct & ": " & Format$(Amount, "$ 0.00"))
            ReturnedAmount = ReturnedAmount + Amount
            AuthCode = XML_GetNode(resString, "AuthorizationCode")

            CardType = XML_GetNode(resString, "CardType")
            Segment = ""

            Select Case Val(CardType)

                Case 1

                    CardType = "Amex"

                Case 2

                    CardType = "Discover"

                Case 3

                    CardType = "Mastercard"

                Case 4

                    CardType = "Visa"

                Case 5

                    CardType = "Debit"

                Case 6

                    CardType = "EBT"

                Case 7

                    CardType = "EGC"

                Case 8

                    CardType = "WEX"

                Case 9

                    CardType = "Voyager"

                Case 10

                    CardType = "JCB"

                Case 11

                    CardType = "CUP"

                Case Else

                    CardType = "Unknown"

            End Select
            CardType = UCase(CardType)
            ReferenceNumber = HistoryID

            ID = GetNextIDNumber(gSmartSwiperDB, "History")
            Segment = AddElementToSegment(Segment, "ID", ID)
            Segment = AddElementToSegment(Segment, "TransactionCode", "Refund")
            Segment = AddElementToSegment(Segment, "TransactionType", "KEYED")
            Segment = AddElementToSegment(Segment, "TransactionID", ReferenceNumber)
            If Not gDepartment = "" Then

                Segment = AddElementToSegment(Segment, "Department", gDepartment)

            End If
            Segment = AddElementToSegment(Segment, "Token", HistoryID)
            Segment = AddElementToSegment(Segment, "ReferenceID", HistoryID)
            Segment = AddElementToSegment(Segment, "Provider", CardType)
            Segment = AddElementToSegment(Segment, "DisplayAmount", Amount * -1)
            Segment = AddElementToSegment(Segment, "AuthorizationCode", AuthCode)
            Segment = AddElementToSegment(Segment, "AuthCode", AuthCode)
            Segment = RemoveBlankElementsFromSegment(Segment)
            Segment = AddElementToSegment(Segment, "InvNum", XML_GetNode(resString, "InvoiceNumber"))
            buf = XML_GetNode("TransactionDate", resString)
            iloc = InStr(1, buf, " ")
            Segment = AddElementToSegment(Segment, "TransactionDate", Mid(buf, 1, iloc - 1))
            Segment = AddElementToSegment(Segment, "TransactionTime", Mid(buf, iloc + 1))
            Segment = AddElementToSegment(Segment, "DrawerID", gDrawerID)
            Segment = AddElementToSegment(Segment, "UserID", gCurrentUser)

            SQL = "SELECT * FROM History WHERE TransactionCode = 'Sale' AND InvNum = " & XML_GetNode(resString, "InvoiceNumber")
            buf = IO_GetSegmentSet(gSmartSwiperDB, SQL)
            buf = GetNextSegmentFromSet(buf)
            Segment = AddElementToSegment(Segment, "AuthorizedAmount", Amount * -1)
            Segment = AddElementToSegment(Segment, "NameOnCard", ExtractElementFromSegment("NameOnCard", buf))
            Segment = AddElementToSegment(Segment, "RightFour", ExtractElementFromSegment("RightFour", buf))
            Segment = AddElementToSegment(Segment, "OriginalChargeAmount", ExtractElementFromSegment("Amount", buf))
            buf = Segment
            Segment = AddElementToSegment(Segment, "XMLResponse", resString)
            gCCHistorySchema = IO_GetFieldsCollection(gSmartSwiperDB, "History", "", True, False, True)
            SQL = MakeInsertSQLFromSchema("History", Segment, gCCHistorySchema)
            ret = IO_UpdateSQLProcessor(gSmartSwiperDB, SQL)

            GENIUS_processRETURN = buf
            gCreditCardSegment = buf

        End If
        If ReturnedAmount > 0 And ct > 1 Then

            MsgBox("Returned Total = " & Format$(ReturnedAmount, "$ 0.00"))

        End If
        Exit Function

    End Function

    Public Function GENIUS_GenerateCredentialSegment() As String

        Dim Segment As String

        Segment = ""

        ' These are real credentials

        Segment = AddElementToSegment(Segment, "MerchantName", GetPolicyData(gSmartSwiperDB, "MW_DBA/Name"))
        Segment = AddElementToSegment(Segment, "SiteID", GetPolicyData(gSmartSwiperDB, "MW_SiteID"))
        Segment = AddElementToSegment(Segment, "MerchantKey", GetPolicyData(gSmartSwiperDB, "MW_Key"))

        GENIUS_GenerateCredentialSegment = Segment

    End Function

    Public Function GENIUS_Vault_RemoveToken(Token As String) As String

        '########################################   The Sale Reference ID is the Token

        Dim XMLBody As String
        Dim ErrorMesg As String = ""
        Dim mname As String

        Dim URI As String = "https://ps1.merchantware.net/Merchantware/ws/RetailTransaction/v45/Credit.asmx"
        Dim ret As Long = 0
        Dim reqString As String = String.Empty
        Dim resString As String = String.Empty

        Net.ServicePointManager.SecurityProtocol = Net.SecurityProtocolType.Tls12
        Using webClient As New Net.WebClient()

            '    Headers

            webClient.Headers("Content-Type") = "text/xml; charset=utf-8"
            webClient.Headers("SOAPAction") = "http://schemas.merchantwarehouse.com/merchantware/v45/UnboardCard"
            Dim WebHeaders As String = webClient.Headers.ToString
            ret = WriteFile_ToEnd(WebHeaders, "c:\shipritenext\XML_HEADERS.TXT")

            '    Payload

            mname = ExtractElementFromSegment("MerchantName", gCredentialSegment)
            mname = FlushOut(mname, "&", "~")
            mname = FlushOut(mname, "~", "&amp;")

            XMLBody = ""
            XMLBody &= "<?xml version=""1.0"" encoding=""utf-8""?>" & vbCrLf
            XMLBody &= "<soap:Envelope " & vbCrLf
            XMLBody &= "      xmlns:soap=""http://www.w3.org/2003/05/soap-envelope""" & ">" & vbCrLf
            XMLBody &= " <soap:Body>" & vbCrLf
            XMLBody &= "  <UnboardCard xmlns=""http://schemas.merchantwarehouse.com/merchantware/v45/"">" & vbCrLf
            XMLBody &= "    <Credentials>" & vbCrLf
            XMLBody &= "      <MerchantName>" & mname & "</MerchantName>" & vbCrLf
            XMLBody &= "      <MerchantSiteId>" & ExtractElementFromSegment("SiteID", gCredentialSegment) & "</MerchantSiteId>" & vbCrLf
            XMLBody &= "      <MerchantKey>" & ExtractElementFromSegment("MerchantKey", gCredentialSegment) & "</MerchantKey>" & vbCrLf
            XMLBody &= "    </Credentials>" & vbCrLf
            XMLBody &= "    <Request>" & vbCrLf
            XMLBody &= "      <VaultToken>" & Token & "</VaultToken>" & vbCrLf
            XMLBody &= "    </Request>" & vbCrLf
            XMLBody &= "  </UnboardCard>" & vbCrLf
            XMLBody &= " </soap:Body>" & vbCrLf
            XMLBody &= "</soap:Envelope>" & vbCrLf & vbCrLf
            ret = WriteFile_ToEnd(XMLBody, "c:\shipritenext\XML_UnVault_Payload.TXT")

            resString = webClient.UploadString(URI, "POST", XMLBody)

        End Using
        'MsgBox(resString)
        ErrorMesg = XML_GetNode(resString, "ErrorMessage")
        If Not ErrorMesg = "" Then

            MsgBox("ATTENTION...UnVault Token Failure" & vbCrLf & vbCrLf & ErrorMesg, vbCritical, "Shiprite Next")
            Return ""

        Else

            '            MsgBox(resString)
            Return XML_GetNode(resString, "VaultToken")

        End If

    End Function

    Public Function GENIUS_Vault_BoardPreviousCard(Token As String) As String

        '########################################   The Sale Reference ID is the Token

        Dim XMLBody As String
        Dim ErrorMesg As String = ""
        Dim mname As String

        Dim URI As String = "https://ps1.merchantware.net/Merchantware/ws/RetailTransaction/v45/Credit.asmx"
        Dim ret As Long = 0
        Dim reqString As String = String.Empty
        Dim resString As String = String.Empty

        Net.ServicePointManager.SecurityProtocol = Net.SecurityProtocolType.Tls12
        Using webClient As New Net.WebClient()

            '    Headers

            webClient.Headers("Content-Type") = "text/xml; charset=utf-8"
            webClient.Headers("SOAPAction") = "http://schemas.merchantwarehouse.com/merchantware/v45/BoardCard"
            Dim WebHeaders As String = webClient.Headers.ToString
            ret = WriteFile_ToEnd(WebHeaders, "c:\shipritenext\XML_HEADERS.TXT")

            '    Payload

            mname = ExtractElementFromSegment("MerchantName", gCredentialSegment)
            mname = FlushOut(mname, "&", "~")
            mname = FlushOut(mname, "~", "&amp;")

            XMLBody = ""
            XMLBody &= "<?xml version=""1.0"" encoding=""utf-8""?>" & vbCrLf
            XMLBody &= "<soap:Envelope " & vbCrLf
            '            XMLBody &= "      xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/""" & ">" & vbCrLf
            XMLBody &= "      xmlns:soap=""http://www.w3.org/2003/05/soap-envelope""" & ">" & vbCrLf
            XMLBody &= " <soap:Body>" & vbCrLf
            XMLBody &= "  <BoardCard xmlns=""http://schemas.merchantwarehouse.com/merchantware/v45/"">" & vbCrLf
            XMLBody &= "    <Credentials>" & vbCrLf
            XMLBody &= "      <MerchantName>" & mname & "</MerchantName>" & vbCrLf
            XMLBody &= "      <MerchantSiteId>" & ExtractElementFromSegment("SiteID", gCredentialSegment) & "</MerchantSiteId>" & vbCrLf
            XMLBody &= "      <MerchantKey>" & ExtractElementFromSegment("MerchantKey", gCredentialSegment) & "</MerchantKey>" & vbCrLf
            XMLBody &= "    </Credentials>" & vbCrLf
            XMLBody &= "    <PaymentData>" & vbCrLf
            XMLBody &= "      <Source>PreviousTransaction</Source>" & vbCrLf
            XMLBody &= "      <Token>" & Token & "</Token>" & vbCrLf
            XMLBody &= "    </PaymentData>" & vbCrLf
            XMLBody &= "  </BoardCard>" & vbCrLf
            XMLBody &= " </soap:Body>" & vbCrLf
            XMLBody &= "</soap:Envelope>" & vbCrLf & vbCrLf
            ret = WriteFile_ToEnd(XMLBody, "c:\shipritenext\XML_Payload.TXT")

            resString = webClient.UploadString(URI, "POST", XMLBody)

        End Using
        ErrorMesg = XML_GetNode(resString, "ErrorMessage")
        If Not ErrorMesg = "" Then

            MsgBox("ATTENTION...Card Boarding Failure" & vbCrLf & vbCrLf & ErrorMesg, vbCritical, "Shiprite Next")
            Return ""

        Else

            'MsgBox(resString)
            Return XML_GetNode(resString, "VaultToken")

        End If

    End Function

    Public Function GENIUS_Vault_Sale(Token As String, DataSegment As String) As String

        '#############################################  Input the VAULT Token, Not Sale Token.

        '#############################################  Returns the CC segment and writes to history

        Dim ReturnSegment As String = ""
        Dim buf As String = ""

        Dim XMLBody As String
        Dim ErrorMesg As String = ""
        Dim mname As String

        Dim URI As String = "https://ps1.merchantware.net/Merchantware/ws/RetailTransaction/v45/Credit.asmx"
        Dim ret As Long = 0
        Dim reqString As String = String.Empty
        Dim resString As String = String.Empty

        Dim WriteResult As Boolean = False

        Net.ServicePointManager.SecurityProtocol = Net.SecurityProtocolType.Tls12
        Using webClient As New Net.WebClient()

            '    Headers

            webClient.Headers("Content-Type") = "text/xml; charset=utf-8"
            webClient.Headers("SOAPAction") = "http://schemas.merchantwarehouse.com/merchantware/v45/Sale"
            Dim WebHeaders As String = webClient.Headers.ToString
            ret = WriteFile_ToEnd(WebHeaders, "c:\shipritenext\XML_HEADERS.TXT")

            '    Payload

            mname = ExtractElementFromSegment("MerchantName", gCredentialSegment)
            mname = FlushOut(mname, "&", "~")
            mname = FlushOut(mname, "~", "&amp;")

            XMLBody = ""
            XMLBody &= "<?xml version=""1.0"" encoding=""utf-8""?>" & vbCrLf
            XMLBody &= "<soap:Envelope " & vbCrLf
            XMLBody &= "      xmlns:soap=""http://www.w3.org/2003/05/soap-envelope""" & ">" & vbCrLf
            XMLBody &= " <soap:Body>" & vbCrLf
            XMLBody = XMLBody & "  <Sale xmlns=""http://schemas.merchantwarehouse.com/merchantware/v45/"">" & vbCrLf
            XMLBody = XMLBody & "   <Credentials>" & vbCrLf
            XMLBody = XMLBody & "     <MerchantName>" & mname & "</MerchantName>" & vbCrLf
            XMLBody = XMLBody & "     <MerchantSiteId>" & ExtractElementFromSegment("SiteID", gCredentialSegment) & "</MerchantSiteId>" & vbCrLf
            XMLBody = XMLBody & "     <MerchantKey>" & ExtractElementFromSegment("MerchantKey", gCredentialSegment) & "</MerchantKey>" & vbCrLf
            XMLBody = XMLBody & "   </Credentials>" & vbCrLf
            XMLBody = XMLBody & "   <PaymentData>" & vbCrLf
            XMLBody = XMLBody & "     <Source>Vault</Source>" & vbCrLf
            XMLBody = XMLBody & "     <VaultToken>" & Token & "</VaultToken>" & vbCrLf
            XMLBody = XMLBody & "   </PaymentData>" & vbCrLf
            XMLBody = XMLBody & "   <Request>" & vbCrLf
            XMLBody = XMLBody & "     <Amount>" & ExtractElementFromSegment("SaleAmount", DataSegment) & "</Amount>" & vbCrLf
            XMLBody = XMLBody & "     <CashbackAmount>0.00</CashbackAmount>" & vbCrLf
            XMLBody = XMLBody & "     <SurchargeAmount>0.00</SurchargeAmount>" & vbCrLf
            XMLBody = XMLBody & "     <TaxAmount>" & ExtractElementFromSegment("SalesTax", DataSegment) & "</TaxAmount>" & vbCrLf
            XMLBody = XMLBody & "     <InvoiceNumber>" & ExtractElementFromSegment("InvoiceNumber", DataSegment) & "</InvoiceNumber>" & vbCrLf
            XMLBody = XMLBody & "     <PurchaseOrderNumber>" & ExtractElementFromSegment("PONumber", DataSegment) & "</PurchaseOrderNumber>" & vbCrLf
            XMLBody = XMLBody & "     <CustomerCode>" & ExtractElementFromSegment("CustomerID", DataSegment) & "</CustomerCode>" & vbCrLf
            XMLBody = XMLBody & "     <MerchantTransactionId>" & ExtractElementFromSegment("TransactionID", DataSegment) & "</MerchantTransactionId>" & vbCrLf
            XMLBody = XMLBody & "     <CardAcceptorTerminalId>3</CardAcceptorTerminalId>" & vbCrLf
            XMLBody = XMLBody & "     <EnablePartialAuthorization>False</EnablePartialAuthorization>" & vbCrLf
            XMLBody = XMLBody & "     <ForceDuplicate>False</ForceDuplicate>" & vbCrLf
            XMLBody = XMLBody & "   </Request>" & vbCrLf
            XMLBody = XMLBody & "  </Sale>" & vbCrLf
            XMLBody &= " </soap:Body>" & vbCrLf
            XMLBody &= "</soap:Envelope>" & vbCrLf & vbCrLf
            ret = WriteFile_ToEnd(XMLBody, "c:\shipritenext\XML_Payload_VaultSale.TXT")
            buf = ""
            buf &= vbCrLf & "######################################### [Issue Sale with CC Token from the Cyan Vault] #########################"
            buf &= vbCrLf & ""
            buf &= vbCrLf & Format$(Today, "MM/dd/yyyy") & " " & Format$(Now, "HH:mm:ss")
            buf &= vbCrLf & "--------XML REQUEST--------------------------------------------------------"
            buf &= vbCrLf & XMLBody
            buf &= vbCrLf & vbCrLf & ""
            WriteResult = WriteFile_Append(buf, "c:\SmartSwiper\XML_History.txt")


            resString = webClient.UploadString(URI, "POST", XMLBody)


            buf &= vbCrLf & ""
            buf &= vbCrLf & "######################################### [Reply Vault Sale] #########################"
            buf &= vbCrLf & ""
            buf &= vbCrLf & Format$(Today, "MM/dd/yyyy") & " " & Format$(Now, "HH:mm:ss")
            buf &= vbCrLf & "--------XML RESPONSE--------------------------------------------------------"
            buf &= vbCrLf & FlushOut(resString, "><", ">" & vbCrLf & "<")
            buf &= vbCrLf & ""
            WriteResult = WriteFile_Append(buf, "c:\SmartSwiper\XML_History.txt")

        End Using

        '        MsgBox(resString)
        ErrorMesg = XML_GetNode(resString, "ErrorMessage")
        If Not ErrorMesg = "" Then

            MsgBox("ATTENTION...Vault Sale Failure" & vbCrLf & vbCrLf & ErrorMesg, vbCritical, "Shiprite Next")
            ReturnSegment = ""

        Else

            ReturnSegment = ""
            ReturnSegment = AddElementToSegment(ReturnSegment, "Result", "SUCCESS")
            ReturnSegment = AddElementToSegment(ReturnSegment, "AuthCode", XML_GetNode(resString, "AuthorizationCode"))
            ReturnSegment = AddElementToSegment(ReturnSegment, "AuthorizedAmount", XML_GetNode(resString, "Amount"))
            ReturnSegment = AddElementToSegment(ReturnSegment, "OriginalChargeAmount", XML_GetNode(resString, "Amount"))
            ReturnSegment = AddElementToSegment(ReturnSegment, "CardNumber", XML_GetNode(resString, "CardNumber"))
            ReturnSegment = AddElementToSegment(ReturnSegment, "CardHolder", XML_GetNode(resString, "Cardholder"))
            ReturnSegment = AddElementToSegment(ReturnSegment, "ReferenceID", XML_GetNode(resString, "Token"))
            ReturnSegment = AddElementToSegment(ReturnSegment, "Provider", "VAULT")
            ReturnSegment = AddElementToSegment(ReturnSegment, "SaleNumber", ExtractElementFromSegment("InvoiceNumber", DataSegment))
            ReturnSegment = AddElementToSegment(ReturnSegment, "CardType", XML_GetNode(resString, "CardType"))
            ReturnSegment = AddElementToSegment(ReturnSegment, "TransactionType", "VAULT")
            ReturnSegment = AddElementToSegment(ReturnSegment, "TransactionCode", "Sale")
            ReturnSegment = ReturnSegment & DataSegment
            'MsgBox(ReturnSegment)

        End If
        Return ReturnSegment

    End Function

    Public Function GENIUS_ProcessVaultPayment(MoneyAmt As String, SalesTaxAmt As String, InvNum As String, PONumber As String) As String

        Dim VaultToken As String
        Dim VaultCardType As String
        Dim VaultExpirationDT As String
        Dim VaultCreditCard As String
        Dim Segment As String
        Dim buf As String
        Dim SQL As String

        SQL = "SELECT VaultToken, VaultCardType, VaultExpirationDT, VaultCreditCard, AcctNum FROM AR WHERE AcctNum = '" & ExtractElementFromSegment("AR", gCustomerSegment) & "'"
        Segment = IO_GetSegmentSet(gShipriteDB, SQL)

        VaultToken = ExtractElementFromSegment("VaultToken", Segment)
        VaultCardType = ExtractElementFromSegment("VaultCardType", Segment)
        VaultExpirationDT = ExtractElementFromSegment("VaultExpirationDT", Segment)
        VaultCreditCard = ExtractElementFromSegment("VaultCreditCard", Segment)

        gCredentialSegment = GENIUS_GenerateCredentialSegment()

        Segment = ""
        Segment = AddElementToSegment(Segment, "SaleAmount", MoneyAmt)
        Segment = AddElementToSegment(Segment, "SalesTax", SalesTaxAmt)
        Segment = AddElementToSegment(Segment, "InvoiceNumber", InvNum)
        Segment = AddElementToSegment(Segment, "PONumber", PONumber)
        Segment = AddElementToSegment(Segment, "CustomerID", ExtractElementFromSegment("AcctNum", ExtractElementFromSegment("AcctNum", Segment)))
        Segment = AddElementToSegment(Segment, "TransactionID", InvNum)

        gCreditCardSegment = GENIUS_Vault_Sale(VaultToken, Segment)
        buf = ExtractElementFromSegment("Result", gCreditCardSegment)

        If UCase(buf) = "FAILED" Then

            buf = ExtractElementFromSegment("ErrorMessage", gCreditCardSegment)
            MsgBox(buf, vbCritical, "ShipriteNext")

        Else

            If ExtractElementFromSegment("Provider", gCreditCardSegment) = "" Then
                gCreditCardSegment = AddElementToSegment(gCreditCardSegment, "Provider", GENIUS_Convert_CardTypeValue(VaultCardType, True))
            End If
            If ExtractElementFromSegment("ExpirationDate", gCreditCardSegment) = "" Then
                gCreditCardSegment = AddElementToSegment(gCreditCardSegment, "ExpirationDate", VaultExpirationDT)
            End If
            gCreditCardSegment = AddElementToSegment(gCreditCardSegment, "IsVaultSale", "True")

        End If
        Return ""

    End Function
    Public Function GENIUS_Convert_CardTypeValue(ByVal CardTypeValue As String, Optional ByVal ReturnArgOnFail As Boolean = False) As String

        Dim intCardType As Integer
        Dim buf As String

        CardTypeValue = Trim$(CardTypeValue)
        If ReturnArgOnFail Then
            buf = CardTypeValue
        Else
            buf = ""
        End If

        If IsNumeric(CardTypeValue) Then

            intCardType = Val(CardTypeValue)

            Select Case intCardType

                Case 0 : buf = "UNKNOWN"
                Case 1 : buf = "AMEX"
                Case 2 : buf = "DISCOVER"
                Case 3 : buf = "MASTERCARD"
                Case 4 : buf = "VISA"
                Case 5 : buf = "DEBIT"
                Case 6 : buf = "EBT"
                Case 7 : buf = "EGC"
                Case 8 : buf = "WEX"
                Case 9 : buf = "VOYAGER"
                Case 10 : buf = "JCB"
                Case 11 : buf = "CUP"
                Case 12 : buf = "LVLUP"
                Case Else : buf = "UNKNOWN"

            End Select

        End If

        Return buf

    End Function


End Module
