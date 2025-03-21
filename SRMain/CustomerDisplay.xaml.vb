Public Class CustomerDisplay
    'Inherits CommonWindow

    Private Counter As Integer
    Dim ImageList As List(Of Ad_Image)
    Dim Timer1 As System.Windows.Threading.DispatcherTimer

    Public Sub New()

        MyBase.New()

        ' This call is required by the designer.
        InitializeComponent()

    End Sub


    Private Sub CustomerDisplay_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Dim senderwindow = TryCast(sender, Window)
        senderwindow.WindowState = WindowState.Maximized

        If Not System.IO.Directory.Exists(gDBpath & "\Ads") Then
            System.IO.Directory.CreateDirectory(gDBpath & "\Ads")
            System.IO.Directory.CreateDirectory(gDBpath & "\Ads\Logo")
            System.IO.Directory.CreateDirectory(gDBpath & "\Ads\POS")
        End If

        Load_Ad_Images()
        Counter = 0

        'makes tab headers not visible in run time. 
        For Each currentTab As TabItem In DisplayTabControl.Items
            currentTab.Visibility = Visibility.Collapsed
        Next

        ImageSlideShow()
    End Sub

    Public Sub ChangeTab(index As Integer)
        DisplayTabControl.SelectedIndex = index

        If Not IsNothing(Timer1) Then
            Timer1.Stop()
        End If


        If index = 2 Then

            'Shipping
            Dim Files = IO.Directory.GetFiles(gDBpath & "\Ads\Logo\", "*.*", IO.SearchOption.TopDirectoryOnly)

            If Files.Count > 0 Then
                Store_Logo.ImageSource = Setup_General.GetBitMapImage(Files(0))
            Else
                Store_Logo_Border.Visibility = Visibility.Hidden
            End If

            If GetPolicyData(gShipriteDB, "CustomerDisplay_Hide_SHIP", "False") Then
                SHIP_Border.Visibility = Visibility.Hidden
            End If

        ElseIf index = 1 Then
            'POS
            Dim Files = IO.Directory.GetFiles(gDBpath & "\Ads\POS\", "*.*", IO.SearchOption.TopDirectoryOnly)

            If Files.Count > 0 Then
                POS_Ad_Image.Source = Setup_General.GetBitMapImage(Files(0))
            Else
                POS_Ad_Image.Visibility = Visibility.Hidden
            End If


        ElseIf index = 0 Then

            ImageSlideShow()
        End If
    End Sub

    Private Sub ImageSlideShow()
        If ImageList.Count = 0 Then
            Exit Sub

        ElseIf ImageList.Count = 1 Then
            SlideShowImage.Source = ImageList(Counter).BitImage
            Exit Sub
        End If


        Timer1 = New System.Windows.Threading.DispatcherTimer
        Timer1.Interval = New TimeSpan(0, 0, 3)
        AddHandler Timer1.Tick, Sub(o As Object, arg As EventArgs) Timer1_Tick()
        Timer1.Start()
    End Sub


    Private Sub Timer1_Tick()
        SlideShowImage.Source = ImageList(Counter).BitImage
        updateCounter()
    End Sub

    Private Sub updateCounter()
        If Counter = ImageList.Count - 1 Then
            Counter = 0
        Else
            Counter += 1
        End If
    End Sub

    Private Sub Load_Ad_Images()
        Dim Image As Ad_Image
        Dim FileList As List(Of String)

        FileList = System.IO.Directory.GetFiles(gDBpath & "\Ads").ToList

        ImageList = New List(Of Ad_Image)

        For Each PicturePath As String In FileList
            If PicturePath.Contains(".jpg") Or PicturePath.Contains(".jpeg") Or PicturePath.Contains(".png") Then
                Image = New Ad_Image
                Image.ImageName = Get_FileName(PicturePath)
                Image.ImagePath = PicturePath
                Image.BitImage = Setup_General.GetBitMapImage(PicturePath)

                ImageList.Add(Image)
            End If
        Next
    End Sub

    Public Sub UpdateShippingRates(ByRef DisplayCarrierList As List(Of Carrier), FromAddress As String, ToAddress As String)

        Shipper_TxtBx.Text = FromAddress
        Consignee_TxtBx.Text = ToAddress

        If GetPolicyData(gShipriteDB, "CustomerDisplay_Hide_SHIP", "False") = False Then
            ShippingPanel_IC.ItemsSource = DisplayCarrierList
        End If
    End Sub

    Public Sub ClearShipScreen()
        ShippingPanel_IC.ItemsSource = Nothing
        ShippingPanel_IC.Items.Refresh()

        Shipper_TxtBx.Text = ""
        Consignee_TxtBx.Text = ""

    End Sub

    Public Sub UpdatePOS(ByRef POSLines As ObjectModel.ObservableCollection(Of POSLine))
        Receipt_LB.ItemsSource = POSLines
        Receipt_LB.Items.Refresh()
        PoleDisplay_Total.Text = FormatCurrency(gGrandTotal)
    End Sub

    Public Sub ClearPOS()
        Receipt_Border.Visibility = Visibility.Visible
        PoleDisplay_Total.Text = FormatCurrency("0")
        Receipt_LB.ItemsSource = Nothing
        Receipt_LB.Items.Refresh()
        TotalHeader.Text = "Total Sale"
    End Sub

    Public Sub DisplayChangeDue()
        Receipt_Border.Visibility = Visibility.Hidden
        PoleDisplay_Total.Text = FormatCurrency(gChangeDue)
        TotalHeader.Text = "Change Due"
    End Sub
End Class
