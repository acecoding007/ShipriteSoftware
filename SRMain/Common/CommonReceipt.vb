
Public Class _basePOS_Receipt
    '
    Public Property InvoiceNumb As Long
    Public Property InvoiceDate As Date
    Public Property AccountName As String
    Public Property AccountNumb As String
    Public Property Clerk As String
    '
    Public Property Items As New List(Of _basePOS_ReceiptItem)
    '
    Public Property PaymentType As String
    Public Property CustomerPaid As Double
    Public Property IsCash As Boolean
    Public Property IsCheck As Boolean
    Public Property IsCredit As Boolean
    Public Property IsRefund As Boolean
    Public Property IsOther As Boolean
    Public Property CheckOrCCard_Numb As String
    Public Property CheckOrCCard_Name As String
    '
    Public Property IsPrintDisclaimer As Boolean
    Public Property Disclaimer As String
    Public Property Coupons As String
    '
    Public ReadOnly Property SalesTax1 As Double
        Get ' Canada receipts will have HST, PST, GST sales tax break-down.
            SalesTax1 = 0 ' assume
            For Each item As _basePOS_ReceiptItem In Items
                SalesTax1 += item.ItemSalesTax1
            Next
        End Get
    End Property
    Public ReadOnly Property SalesTax2 As Double
        Get ' Canada receipts will have HST, PST, GST sales tax break-down.
            SalesTax2 = 0 ' assume
            For Each item As _basePOS_ReceiptItem In Items
                SalesTax2 += item.ItemSalesTax2
            Next
        End Get
    End Property
    Public ReadOnly Property SalesTax3 As Double
        Get ' Canada receipts will have HST, PST, GST sales tax break-down.
            SalesTax3 = 0 ' assume
            For Each item As _basePOS_ReceiptItem In Items
                SalesTax3 += item.ItemSalesTax3
            Next
        End Get
    End Property
    '
    Public ReadOnly Property SubTotal As Double
        Get
            SubTotal = 0 ' assume
            For Each item As _basePOS_ReceiptItem In Items
                SubTotal += item.ItemExtPrice
            Next
        End Get
    End Property
    Public ReadOnly Property SalesTax As Double
        Get
            SalesTax = 0 ' assume
            For Each item As _basePOS_ReceiptItem In Items
                SalesTax += item.ItemSalesTax
            Next
        End Get
    End Property
    Public ReadOnly Property ServiceTax As Double
        Get
            ServiceTax = 0 ' assume
            For Each item As _basePOS_ReceiptItem In Items
                ServiceTax += item.ItemServiceTax
            Next
        End Get
    End Property
    Public ReadOnly Property Total As Double
        Get
            Total = SubTotal + SalesTax + ServiceTax
        End Get
    End Property
    '
    Public Sub New()
        IsPrintDisclaimer = False
        Disclaimer = String.Empty
        Coupons = String.Empty
    End Sub
    Public Sub New(ByVal invoiceNumber As Long)
        InvoiceNumb = invoiceNumber
        IsPrintDisclaimer = False
        Disclaimer = String.Empty
        Coupons = String.Empty
    End Sub
End Class

Public Class _basePOS_ReceiptItem
    Public Property ItemDesc As String
    Public Property ItemSKU As String
    Public Property ItemPrice As Double
    Public Property ItemQty As Double
    Public Property ItemExtPrice As Double
    Public Property ItemIsTaxable As Boolean
    Public Property ItemSalesTax As Double
    Public Property ItemServiceTax As Double
    ' Canada receipts will have HST, PST, GST sales tax break-down.
    Public Property ItemSalesTax1 As Double
    Public Property ItemSalesTax2 As Double
    Public Property ItemSalesTax3 As Double
End Class
