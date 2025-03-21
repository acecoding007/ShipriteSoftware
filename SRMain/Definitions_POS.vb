Public Module Definitions_POS

    Public gdbSchema_Payments As String
    Public gdbSchema_Transactions As String
    Public gPaymentsCompleted As Boolean
    Public gPOS_IsPrintReceipt As Boolean
    Public gPOS_FullSheetInvoice_Email As String
    Public gPOS_IsPrintFullSheetInvoice As Boolean
    Public gPOS_EmailReceipt As Email_POSReceipt

    Public Class Email_POSReceipt
        Public Property isEmail As Boolean
        Public Property EmailAddress As String
        Public Property EmailBody As String
        Public Property EmailTemplate As EmailTemplate
    End Class

    Public Class POSLine

        Public Property ID As Long
        Public Property SKU As String
        Public Property ModelNumber As String
        Public Property Description As String
        Public Property Department As String
        Public Property Category As String
        Public Property BrandName As String
        Public Property AcctNum As String
        Public Property AcctName As String
        Public Property SoldToID As Long
        Public Property ShipToID As Long
        Public Property UnitPrice As Double
        Public Property UnitCost As Double
        Public Property Quantity As Double
        Public Property Discount As Double
        Public Property ExtPrice As Double
        Public Property TRate As Double
        Public Property STax As Double
        Public Property TaxCounty As String
        Public Property LTotal As Double
        Public Property COGS As Double
        Public Property PackageID As String
        Public Property PackMaster As Boolean
        Public Property isCOGSview As Boolean
        Public Property isPriceOverride As Boolean

    End Class

    Public POSLines As New ObjectModel.ObservableCollection(Of POSLine)

    Public gInvoiceNumber As String
    Public gSubTotal As Double
    Public gSalesTax As Double
    Public gGrandTotal As Double
    Public gChangeDue As Double

    Public Structure NewSaleHeader

        Public TaxCounty As String
        Public TaxRate As String
        Public LineCount As Integer
        Public SubTotal As Double
        Public Taxes As Double
        Public GrandTotal As Double

    End Structure
    Public gPOSHeader As NewSaleHeader
    Public gBlankPosHeader As NewSaleHeader

    Public Class PaymentDefinition

        Public Property ID As Long
        Public Property InvNum As Long
        Public Property PostDate As String
        Public Property Type As String
        Public Property Desc As String
        Public Property Charge As Double
        Public Property Payment As Double
        Public Property Check_Number As String
        Public Property Check_Name As String
        Public Property Check_NameOfBank As String
        Public Property Check_StateOfBank As String
        Public Property CC_Last4 As String
        Public Property CC_AuthorizationCode As String
        Public Property CC_ExpDate As String
        Public Property CC_TypeOfCard As String
        Public Property CC_CardName
        Public Property RecoveredPayment As Boolean
        Public Property AdjustmentInvoiceNumber As Long
        Public Property PaymentDisplay As String
        Public Property OtherText As String

    End Class
    Public Class PayMaster

        Public Property TotalPaid As Double
        Public Property Balance As Double
        Public Property PreviousPayments() As PaymentDefinition
        Public Property NewPayments As List(Of PaymentDefinition)
        Public isBulkPayment As Boolean

    End Class

    Public Class AR_BulkPaymentItem
        Public Property isPay As Boolean
        Public Property InvDate As Date
        Public Property InvoiceNo As String
        Public Property InvoiceAmount As Double
        Public Property Balance As Double
        Public Property PaymentAmt As Double

    End Class

    Public Class Refund_LineItem
        Public Property ID As String
        Public Property isRefunded As Boolean
        Public Property SKU As String
        Public Property Desc As String
        Public Property UnitPrice As Double
        Public Property Qty As Double
        Public Property LineTotal As Double
        Public Property TRate As Double
        Public Property STax As Double
        Public Property Refund_Qty As Double
        Public Property Refund_Amt As Double
        Public Property Refund_Tax As Double
        Public Property PreviouslyRefundedQty As Double
        Public Property isRefundable As Boolean
        Public Property COGS As Double
        Public Property Department As String

    End Class



    Public Class InvoiceHistoryItem
        Public Property InvDate As Date
        Public Property Memo As String
        Public Property ChargePayment As Double
        Public Property Balance As Double
    End Class

    Public gBlankPaymentMaster As PayMaster
    Public gPM As PayMaster

    Public Class SKUSearchItem
        Public Property SKU As String
        Public Property Description As String
        Public Property Price As Double
    End Class

    Public Class CreditPaymentItem
        Public Property InvoiceNo As String
        Public Property Balance As Double
    End Class

End Module
