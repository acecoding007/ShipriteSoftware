Public Class Shipsurance
    Inherits CommonWindow

    Public Sub New()

        MyBase.New()

        ' This call is required by the designer.
        InitializeComponent()

    End Sub

    Public Sub New(ByVal callingWindow As Window)

        MyBase.New(callingWindow)

        ' This call is required by the designer.
        InitializeComponent()

    End Sub


    Private Sub Shipsurance_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Try
            '
            Call load_Countries()
            Call load_CarrierID()
            Call load_PackageType()
            Call load_CommodityCotegory()
            Call load_CommodityType()
            '
        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to load the form...")
        End Try
    End Sub

    Private Sub load_Countries()
        '
        For Each ctry As _CountryDB In gCountry
            Me.cmb_Country.Items.Add(ctry)
            If ctry.CountryName = "United States" Then
                Me.cmb_Country.SelectedItem = ctry
            End If
        Next
        '
    End Sub
    Private Sub load_CarrierID()
        '
        Dim carriers As String = "USPS,FedEx,UPS,DHL,Canada Post,BAX Global,Purolator,Greyhound GPX,Austrialian Post,Common Carrier,ICS Courier,Royal Mail,EMS,TNT Express,French Postal System,Lone Star Overnight,Spee-Dee Delivery,Golden State Overnight,SAIA,Overnight Transportation,CTI,Roadway,Freight Highway,Yellow Freight,American Freight Companies,Oak Harbor,Pitt Ohio,Koch Logistics,Old Dominion,Estes Express,R+L Carriers,Eastern Connection,Overnight Express,California Overnight"
        Dim split_carriers() = carriers.Split(",")
        For i As Integer = 0 To split_carriers.Count - 1
            Me.cmb_CarrierID.Items.Add(split_carriers(i))
        Next
        '
    End Sub
    Private Sub load_PackageType()
        '
        cmb_PkgType.Items.Add("Box")
        cmb_PkgType.Items.Add("Container")
        cmb_PkgType.Items.Add("Crate")
        cmb_PkgType.Items.Add("Pallet")
        cmb_PkgType.Items.Add("Other")
        '
    End Sub
    Private Sub load_CommodityCotegory()

        cmb_ComCat.Items.Add("General Merchandise")
        cmb_ComCat.Items.Add("Machinery")
        cmb_ComCat.Items.Add("Household Goods")
        cmb_ComCat.Items.Add("Fragile Goods/Glass")
        cmb_ComCat.Items.Add("Computers/Electronics")
        cmb_ComCat.Items.Add("Fine Art")
        cmb_ComCat.Items.Add("Motorcycles/Automobile")
        cmb_ComCat.Items.Add("Precision Instruments")
        cmb_ComCat.Items.Add("Musical Instruments")
        cmb_ComCat.Items.Add("Chemicals/Hazardous Materials")
        cmb_ComCat.Items.Add("Non Perishable Foods")
        cmb_ComCat.Items.Add("Bottled Beverages")
        cmb_ComCat.Items.Add("Frozen Foods (Non Meats)")
        cmb_ComCat.Items.Add("Frozen Meats")

    End Sub
    Private Sub load_CommodityType()
        '
        cmb_ComType.Items.Add("New Goods")
        cmb_ComType.Items.Add("Used Goods")
        cmb_ComType.Items.Add("Reconditioned Goods")
        '
    End Sub

    Private Sub Parcel_Checked(sender As Object, e As RoutedEventArgs) Handles Parcel.Checked
        '
        txt_CarrierSvc.Items.Clear()
        txt_CarrierSvc.Text = ""
        txt_CarrierSvc.Items.Add("Domestic - Ground")
        txt_CarrierSvc.Items.Add("Domestic - Express")
        txt_CarrierSvc.Items.Add("International - Ground")
        txt_CarrierSvc.Items.Add("International - Express")
        txt_dsiRate100.Text = "0"
        '
    End Sub
    Private Sub Cargo_Checked(sender As Object, e As RoutedEventArgs) Handles Cargo.Checked
        '
        txt_CarrierSvc.Items.Clear()
        txt_CarrierSvc.Text = ""
        txt_CarrierSvc.Items.Add("Domestic - Air Freight")
        txt_CarrierSvc.Items.Add("Domestic - LTL/Motor Freight")
        txt_CarrierSvc.Items.Add("Domestic - Van Line")
        txt_CarrierSvc.Items.Add("International - Air Freight")
        txt_CarrierSvc.Items.Add("International - Ocean Freight")
        txt_dsiRate100.Text = "0"
        '
    End Sub

    Private Sub txt_CarrierSvc_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles txt_CarrierSvc.SelectionChanged
        '
        Select Case txt_CarrierSvc.Text
            Case "Domestic - Air Freight" : txt_dsiRate100.Text = "0.5"
            Case "Domestic - LTL/Motor Freight" : txt_dsiRate100.Text = "0.9"
            Case "Domestic - Van Line" : txt_dsiRate100.Text = "0.9"
            Case "International - Air Freight" : txt_dsiRate100.Text = "0.9"
            Case "International - Ocean Freight" : txt_dsiRate100.Text = "1.5"
            Case Else : txt_dsiRate100.Text = "0.25"
        End Select
        '
    End Sub

    Private Sub SaveButton_Click(sender As Object, e As RoutedEventArgs) Handles SaveButton.Click
        Try
            '
            Call Go_Online_DSI()
            '
        Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to upload the package...")
        End Try
    End Sub

    Private Function Go_Online_DSI() As Boolean

        Dim url As String = String.Empty
        Dim PostData As String = String.Empty
        Dim Headers As String = String.Empty
        '
        Go_Online_DSI = False

        ' Don't print labels for Shiprite Demo version.
        If _Debug.IsINHOUSE Then
            url = "https://sandbox.dsiins.com/api.net/dsi_recordShipment.aspx" '' test url
        Else
            url = "https://www.dsiins.com/api.net/dsi_recordShipment.aspx"
        End If

        Headers = "Content-Type: text" & vbCrLf

        Dim extPolicyID As String = General.GetPolicyData(gShipriteDB, "DSI_PolicyID")
        Dim personCompany As String = Replace(General.GetPolicyData(gShipriteDB, "Name"), "&", "And")
        Dim personFirstName As String = General.GetPolicyData(gShipriteDB, "FName")
        Dim personLastName As String = General.GetPolicyData(gShipriteDB, "LName")
        Dim personPhone As String = General.GetPolicyData(gShipriteDB, "Phone1")
        Dim personFax As String = General.GetPolicyData(gShipriteDB, "Phone2")
        Dim personEmail As String = General.GetPolicyData(gShipriteDB, "DSI_Email")

        PostData = "extPersonSourceId=6"
        PostData = PostData & "&sourceUsername=shiprite"
        PostData = PostData & "&sourcePassword=mdrit7"
        PostData = PostData & "&extPolicyID=" & extPolicyID
        PostData = PostData & "&personSourceIdentifier="
        PostData = PostData & "&personCompany=" & personCompany
        PostData = PostData & "&personFirstName=" & personFirstName
        PostData = PostData & "&personLirstName=" & personLastName
        PostData = PostData & "&personPhone=" & personPhone
        PostData = PostData & "&personFax=" & personFax
        PostData = PostData & "&personEmail=" & personEmail
        '
        If Parcel.IsChecked = True Then
            PostData = PostData & "&extShipmentTypeId=1"
        Else
            PostData = PostData & "&extShipmentTypeId=2"
        End If
        '
        If String.IsNullOrEmpty(cmb_CarrierID.Text) AndAlso cmb_CarrierID.SelectedIndex > -1 Then
            MsgBox("Field Carrier ID not set." & vbCrLf & "Please select a value before continuing", vbCritical + vbDefaultButton1 + vbOKOnly)
            Return False
        Else
            PostData = PostData & "&extCarrierID=" & CStr(cmb_CarrierID.SelectedIndex)
        End If
        '
        If txt_CarrierSvc.Text = "" Then
            PostData = PostData & "&carrierServiceName=Default"
        Else
            PostData = PostData & "&carrierServiceName=" & txt_CarrierSvc.Text
        End If
        '
        If txt_TrkNum.Text = "" Then
            MsgBox("Tracking field not set." & vbCrLf & "Please select a value to identify the shipment before continuing", vbCritical + vbDefaultButton1 + vbOKOnly)
            Return False
        Else
            PostData = PostData & "&referenceNumber=" & txt_TrkNum.Text
            PostData = PostData & "&trackingNumber=" & txt_TrkNum.Text
        End If
        '
        If cur_DecValue.Text = "$0.00" Then
            MsgBox("Field Declared Value not set." & vbCrLf & "Please enter a value before continuing", vbCritical + vbDefaultButton1 + vbOKOnly)
            Return False
        Else
            PostData = PostData & "&declaredValue=" & cur_DecValue.Text
        End If
        '
        'Time to fix the date
        PostData = PostData & "&transactionDate=" & String.Format("{0:MM/dd/yyyy}", DateTime.Today)
        PostData = PostData & "&shipmentDate=" & String.Format("{0:MM/dd/yyyy}", DateTime.Today)
        PostData = PostData & "&arrivalDate="
        '
        Select Case cmb_ComCat.Text

            Case "General Merchandise"
                PostData = PostData & "&extCommodityCategoryId=1"
            Case "Machinery"
                PostData = PostData & "&extCommodityCategoryId=2"
            Case "Household Goods"
                PostData = PostData & "&extCommodityCategoryId=3"
            Case "Fragile Goods/Glass"
                PostData = PostData & "&extCommodityCategoryId=4"
            Case "Computers/Electronics"
                PostData = PostData & "&extCommodityCategoryId=5"
            Case "Fine Art"
                PostData = PostData & "&extCommodityCategoryId=6"
            Case "Motorcycles/Automobile"
                PostData = PostData & "&extCommodityCategoryId=7"
            Case "Precision Instruments"
                PostData = PostData & "&extCommodityCategoryId=8"
            Case "Musical Instruments"
                PostData = PostData & "&extCommodityCategoryId=9"
            Case "Chemicals/Hazardous Materials"
                PostData = PostData & "&extCommodityCategoryId=10"
            Case "Non Perishable Foods"
                PostData = PostData & "&extCommodityCategoryId=11"
            Case "Bottled Beverages"
                PostData = PostData & "&extCommodityCategoryId=12"
            Case "Frozen Foods (Non Meats)"
                PostData = PostData & "&extCommodityCategoryId=13"
            Case "Frozen Meats"
                PostData = PostData & "&extCommodityCategoryId=14"
            Case Else
                MsgBox("Field Commodity Catagory not set." & vbCrLf & "Please select a value before continuing", vbCritical + vbDefaultButton1 + vbOKOnly)
                Return False

        End Select
        '
        Select Case cmb_ComType.Text

            Case "New Goods"
                PostData = PostData & "&extCommodityTypeId=1"
            Case "Used Goods"
                PostData = PostData & "&extCommodityTypeId=2"
            Case "Reconditioned Goods"
                PostData = PostData & "&extCommodityTypeId=3"
            Case Else
                MsgBox("Field Commodity Type not set." & vbCrLf & "Please select a value before continuing", vbCritical + vbDefaultButton1 + vbOKOnly)
                Return False

        End Select
        '
        Select Case cmb_PkgType.Text

            Case "Box"
                PostData = PostData & "&extPackageTypeId=1"
            Case "Container"
                PostData = PostData & "&extPackageTypeId=2"
            Case "Crate"
                PostData = PostData & "&extPackageTypeId=3"
            Case "Pallet"
                PostData = PostData & "&extPackageTypeId=4"
            Case "Other"
                PostData = PostData & "&extPackageTypeId=5"
            Case Else
                MsgBox("Field Package Type not set." & vbCrLf & "Please select a value before continuing", vbCritical + vbDefaultButton1 + vbOKOnly)
                Return False

        End Select
        '
        If int_PkgCnt.Text = "0" Then
            PostData = PostData & "&packageCount=1"
        Else
            PostData = PostData & "&packageCount=" & int_PkgCnt.Text
        End If
        '
        If Y_Glass.IsChecked = True Then
            PostData = PostData & "&containsGlass=1"
        Else
            PostData = PostData & "&containsGlass=0"
        End If
        '
        If txt_PkgCont.Text = "" Then
            PostData = PostData & "&packageDescription=Default Package"
        Else
            PostData = PostData & "&packageDescription=" & txt_PkgCont.Text
        End If
        '
        If _Contact.ShipperContact IsNot Nothing Then
            PostData = PostData & "&departureAddress1=" & _Contact.ShipperContact.Addr1
            PostData = PostData & "&departureAddress2=" & _Contact.ShipperContact.Addr2
            PostData = PostData & "&departureCity=" & _Contact.ShipperContact.City
            PostData = PostData & "&departureState=" & _Contact.ShipperContact.State
            PostData = PostData & "&departurePostalCode=" & _Contact.ShipperContact.Zip
            PostData = PostData & "&departureCountry=" & _Contact.ShipperContact.Country
        End If
        '
        If txt_Address1.Text <> "" Then
            PostData = PostData & "&destinationAddress1=" & txt_Address1.Text
        Else
            MsgBox("Field Package Type not set." & vbCrLf & "Please select a value before continuing", vbCritical + vbDefaultButton1 + vbOKOnly)
            Return False
        End If
        '
        PostData = PostData & "&destinationAddress2=" & txt_Address2.Text
        '
        If txt_City.Text <> "" Then
            PostData = PostData & "&destinationCity=" & txt_City.Text
        Else
            MsgBox("Destination City not set." & vbCrLf & "Please select a value before continuing", vbCritical + vbDefaultButton1 + vbOKOnly)
            Return False
        End If
        '
        If txt_State.Text <> "" Then
            PostData = PostData & "&destinationState=" & txt_State.Text
            '11.30.05 #764 tc - Added a check for the state so that the state is not required for <> United States
        ElseIf cmb_Country.Text = "United States" Then
            MsgBox("Destination State not set." & vbCrLf & "Please select a value before continuing", vbCritical + vbDefaultButton1 + vbOKOnly)
            Return False
        End If
        '
        If txt_Zip.Text <> "" Then
            PostData = PostData & "&destinationPostalCode=" & txt_Zip.Text
        Else
            MsgBox("Destination Postal Code not set." & vbCrLf & "Please select a value before continuing", vbCritical + vbDefaultButton1 + vbOKOnly)
            Return False
        End If
        '
        If cmb_Country.Text <> "" Then
            PostData = PostData & "&destinationCountry=" & cmb_Country.Text
        Else
            PostData = PostData & "&destinationCountry=United States"
        End If
        '
        PostData = PostData & "&insuredCompany="
        PostData = PostData & "&insuredFirstName="
        PostData = PostData & "&insuredLastName="
        PostData = PostData & "&insuredPhone="
        PostData = PostData & "&insuredFax="
        PostData = PostData & "&insuredAddressStreet1="
        PostData = PostData & "&insuredAddressStreet2="
        PostData = PostData & "&insuredaddressCity="
        PostData = PostData & "&insuredAaddressState="
        PostData = PostData & "&insuredAddressPostalCode="
        PostData = PostData & "&insuredAddressCountry="
        '
        ' "Member of DSI Premiere Program" check box was added to the DSI Insurance Setup screen.
        If DSI_PremiereProgramMember Then
            PostData = PostData & "&preDeductFlag=1"
        Else
            PostData = PostData & "&preDeductFlag=0"
        End If
        '
        PostData = PostData & "&dsiRatePer100=" & txt_dsiRate100.Text
        '
        'Inet1.RequestTimeout = 60
        'Inet1.Protocol = icHTTPS
        'Inet1.Execute url & "?" & PostData, "POST", "", Headers

        Dim vtData As String = String.Empty
        If Not _XML.Send_HttpWebRequest(url & "?" & PostData, vtData) Then

            MsgBox("No response from server. " & vtData, vbExclamation, "Cannot connect to " & DSI_NewName & " Server!")
            Exit Function

        End If
        '
        Dim SplitResponse() As String = vtData.Split(",")
        '
        If SplitResponse(0) = "1" Then
            '
            MsgBox(DSI.DSI_NewName & " upload successful." & vbCrLf & DSI.DSI_NewName & " Upload Number=" & SplitResponse(2) & vbCrLf & "Please make note of this number.")
            _Debug.Print_(DSI.DSI_NewName & " Confirmation Number = " & SplitResponse(2))

            txt_Address1.Text = ""
            txt_Address2.Text = ""
            txt_City.Text = ""
            txt_State.Text = ""
            txt_Zip.Text = ""
            cmb_Country.Text = ""

            cur_DecValue.Text = "$0.00"
            txt_PkgCont.Text = ""
            int_PkgCnt.Text = "0"
            cmb_CarrierID.Text = ""
            txt_CarrierSvc.Text = ""
            txt_dsiRate100.Text = "0"
            cmb_PkgType.Text = ""
            txt_TrkNum.Text = ""
            cmb_ComCat.Text = ""
            cmb_ComType.Text = ""
            Parcel.IsChecked = True
            N_Glass.IsChecked = True
            '
            ' To Do:
            ' Call PrintShipNum(SplitResponse(2))

        Else

            MsgBox("An Error has occured." & vbCrLf & "Error Message=" & SplitResponse(1) & vbCrLf & "Please contact " & DSI.DSI_NewName & ".", vbCritical + vbDefaultButton1 + vbOKOnly, DSI.DSI_NewName & " Error")

        End If
        '
    End Function


End Class
