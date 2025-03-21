Imports Microsoft.Win32

Public Class Ad_Image
    Public Property ImagePath
    Public Property ImageName
    Public Property BitImage As BitmapImage

End Class

Public Class Setup_General
    Inherits CommonWindow

    Public Class Zip
        Public Property ID As Long
        Public Property Zipcode As String
        Public Property City As String
        Public Property State As String
        Public Property AreaCode As String
        Public Property Status As String
    End Class



    Dim ImageList As List(Of Ad_Image)
    Dim Logo_Image As Ad_Image
    Dim POS_Ad_Image As Ad_Image
    Dim ZipCode_List As List(Of Zip)

    Public Sub New()

        MyBase.New()

        ' This call is required by the designer.
        InitializeComponent()

    End Sub


    Public Sub New(ByVal callingWindow As Window, ByVal TabNo As Integer)

        MyBase.New(callingWindow)

        ' This call is required by the designer.
        InitializeComponent()

        General_TabControl.SelectedIndex = TabNo

    End Sub


    Private Sub Setup_General_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded

        Select Case General_TabControl.SelectedIndex
            Case 0
                Load_ProgramRegistration()

            Case 1
                Load_SecuritySetup()

            Case 2
                Load_ZipCode_Editor()

            Case 3
                Load_CustomerDisplay()
        End Select

        For Each currentTab As TabItem In General_TabControl.Items
            currentTab.Visibility = Visibility.Collapsed
        Next
    End Sub

    Private Sub SaveButton_Click(sender As Object, e As RoutedEventArgs) Handles SaveButton.Click
        Select Case General_TabControl.SelectedIndex
            Case 0
                Save_ProgramRegistration()

            Case 1
                Save_SecuritySetup()

            Case 2
                Save_ZipCode_List()

            Case 3
                Save_CustomerDisplay()

        End Select
    End Sub

#Region "Zip Code Editor"
    Private Sub Load_ZipCode_Editor()
        Header_Image.Source = New BitmapImage(New Uri("Resources/ZipCode.png", UriKind.Relative))
        Header_Lbl.Content = "ZIP CODE SETUP"

        Search_TxtBox.Focus()
    End Sub

    Private Sub Load_ZipCodeList(zip As String)
        Dim buf As String
        Dim current_segment As String
        Dim item As Zip
        ZipCode_List = New List(Of Zip)

        If zip = "" Then Exit Sub
        If zip.Length < 3 Then
            MsgBox("To search for matching zip codes, please enter at least the first 3 digits of the zip code.", vbExclamation)
            Exit Sub
        End If

        buf = IO_GetSegmentSet(gZipCodeDB, "SELECT * From ZipCodes Where Zip LIKE '" & zip & "%'")

        If buf <> "" Then
            Do Until buf = ""
                current_segment = GetNextSegmentFromSet(buf)

                item = New Zip
                item.ID = ExtractElementFromSegment("ID", current_segment, "False")
                item.Zipcode = ExtractElementFromSegment("Zip", current_segment, "False")
                item.City = ExtractElementFromSegment("City", current_segment, "")
                item.State = ExtractElementFromSegment("ST", current_segment, "")
                item.AreaCode = ExtractElementFromSegment("A/C", current_segment, "False")

                ZipCode_List.Add(item)
            Loop

        Else

            If zip.Length = 5 Then
                If MsgBox("Cannot Find Zip:" & zip & " Do you want to add it?", MsgBoxStyle.YesNo + MsgBoxStyle.Exclamation, "Zip Code does not exist") = vbYes Then
                    AddNew_Blank_Zip()
                End If
            End If

        End If



        ZipCode_List = ZipCode_List.OrderBy(Function(value As Zip) value.City).ToList
        ZipCode_LV.ItemsSource = ZipCode_List
    End Sub

    Private Sub Save_ZipCode_List()
        Try
            Dim SQL As String

            For Each item As Zip In ZipCode_List
                If item.Zipcode <> "" Then


                    If item.Status = "Added" Then
                        SQL = "INSERT INTO ZipCodes (City, ST, Zip, [A/C]) VALUES ('" & item.City & "', '" & item.State & "', '" & item.Zipcode & "', '" & item.AreaCode & "')"
                        IO_UpdateSQLProcessor(gZipCodeDB, SQL)

                    ElseIf item.Status = "Deleted" Then
                        SQL = "DELETE * FROM ZipCodes WHERE [ID]=" & item.ID
                        IO_UpdateSQLProcessor(gZipCodeDB, SQL)
                    ElseIf item.Status = "Edited" Then
                        SQL = "UPDATE ZipCodes SET [City]='" & item.City & "', [ST]='" & item.State & "', [Zip]='" & item.Zipcode & "', [A/C]='" & item.AreaCode & "' WHERE [ID]=" & item.ID
                        IO_UpdateSQLProcessor(gZipCodeDB, SQL)

                    End If

                End If
            Next

            MsgBox("Zip Code Changes Saved!", vbInformation)
            Load_ZipCodeList(Search_TxtBox.Text)

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Search_TxtBox_KeyDown(sender As Object, e As KeyEventArgs) Handles Search_TxtBox.KeyDown
        If e.Key = Key.Return Then
            Load_ZipCodeList(Search_TxtBox.Text)
        End If
    End Sub

    Protected Sub SelectCurrentItem(ByVal sender As Object, ByVal e As KeyboardFocusChangedEventArgs)
        'When clicking inside a textbox, select the listiew item that the textbox belongs to.
        Dim item As ListViewItem = CType(sender, ListViewItem)
        item.IsSelected = True
    End Sub

    Private Sub ZipEntry_TextBox_TextChanged()
        If ZipCode_LV.SelectedIndex = -1 Then Exit Sub

        Dim item As Zip = ZipCode_LV.SelectedItem

        If item.Status <> "Added" Then
            item.Status = "Edited"
        End If

    End Sub

    Private Sub Zip_Delete_Btn_Click()
        If ZipCode_LV.SelectedIndex = -1 Then
            MsgBox("No Line Entry Selected", MsgBoxStyle.OkOnly + MsgBoxStyle.Exclamation, "Cannot delete Zip")
            Exit Sub
        End If

        Dim item As Zip = ZipCode_LV.SelectedItem

        If item.Status = "Added" Then
            ZipCode_List.Remove(item)
        Else
            item.Status = "Deleted"
        End If

        ZipCode_LV.Items.Refresh()

    End Sub

    Private Sub AddNew_Blank_Zip()
        If Search_TxtBox.Text = "" Then
            MsgBox("Please enter a zip code into the search box first!", vbExclamation + vbOKOnly)
            Exit Sub
        End If

        Dim item As Zip = New Zip
        item.Zipcode = Search_TxtBox.Text
        item.Status = "Added"
        ZipCode_List.Add(item)
        ZipCode_LV.Items.Refresh()

    End Sub

    Private Sub Zip_AddNew_btn_Click(sender As Object, e As RoutedEventArgs) Handles Zip_AddNew_btn.Click

        AddNew_Blank_Zip()
    End Sub
#End Region

#Region "SecuritySetup"
    Private Sub Load_SecuritySetup()
        Try
            Header_Image.Source = New BitmapImage(New Uri("Resources/Security_Light.png", UriKind.Relative))
            Header_Lbl.Content = "SECURITY SETUP"

            ProgramSecurity_CheckBox.IsChecked = gIsProgramSecurityEnabled
            POSSecurity_CheckBox.IsChecked = gIsPOSSecurityEnabled
            SetupSecurity_CheckBox.IsChecked = gIsSetupSecurityEnabled

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Private Sub Save_SecuritySetup()
        Try
            UpdatePolicy(gShipriteDB, "SecurityEnabled", ProgramSecurity_CheckBox.IsChecked)
            UpdatePolicy(gShipriteDB, "POSSecurity", POSSecurity_CheckBox.IsChecked)
            UpdatePolicy(gShipriteDB, "EnableSetupSecurity", SetupSecurity_CheckBox.IsChecked)

            MsgBox("Changes Saved Successfully! Restart ShipRite for changes to take effect!", vbOKOnly + vbInformation, gProgramName)

        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

#End Region

#Region "Program Registration"
    Private Sub Load_ProgramRegistration()
        Header_Image.Source = New BitmapImage(New Uri("Resources/SoftwareRegistration.png", UriKind.Relative))
        Header_Lbl.Content = "PROGRAM REGISTRATION"

        CompanyName_TxtBox.Text = GetPolicyData(gShipriteDB, "Name", "")
        FirstName_TxtBox.Text = GetPolicyData(gShipriteDB, "FName", "")
        LastName_TxtBox.Text = GetPolicyData(gShipriteDB, "LName", "")
        Add1_TxtBox.Text = GetPolicyData(gShipriteDB, "Addr1", "")
        Add2_TxtBox.Text = GetPolicyData(gShipriteDB, "Addr2", "")
        City_TxtBox.Text = GetPolicyData(gShipriteDB, "City", "")
        State_TxtBox.Text = GetPolicyData(gShipriteDB, "State", "")
        Zip_TxtBox.Text = GetPolicyData(gShipriteDB, "Zip", "")
        Phone_TxtBox.Text = GetPolicyData(gShipriteDB, "Phone1", "")
        Fax_TxtBox.Text = GetPolicyData(gShipriteDB, "Phone2", "")
        Email_TxtBox.Text = GetPolicyData(gShipriteDB, "Email", "")
        RegKey_TxtBox.Text = GetPolicyData(gShipriteDB, "RegistrationNumber", "")
    End Sub

    Private Sub Save_ProgramRegistration()
        UpdatePolicy(gShipriteDB, "Email", Email_TxtBox.Text)
        UpdatePolicy(gShipriteDB, "RegistrationNumber", RegKey_TxtBox.Text)
        MsgBox("Changes Saved Successfully!", vbInformation)
    End Sub




#End Region

#Region "Customer Display Setup"
    Private Sub Load_CustomerDisplay()
        Header_Image.Source = New BitmapImage(New Uri("Resources/DualDisplay.png", UriKind.Relative))
        Header_Lbl.Content = "CUSTOMER DISPLAY SETUP"

        Enable_Display_ChkBx.IsChecked = gIsCustomerDisplayEnabled
        HideShip_ChkBx.IsChecked = GetPolicyData(gShipriteDB, "CustomerDisplay_Hide_SHIP", "False")

        Load_Ad_Images()
        Load_POS_Ad()
        Load_Logo()
    End Sub

    Public Shared Function GetBitMapImage(Path) As BitmapImage
        Dim BitMap As New BitmapImage
        BitMap.BeginInit()
        BitMap.CacheOption = BitmapCacheOption.OnLoad
        BitMap.UriSource = New Uri(Path)
        BitMap.EndInit()

        Return BitMap
    End Function


    Private Sub Save_CustomerDisplay()

        Try
            Dim Path As String
            Dim FileList As List(Of String)
            Dim index As Integer

            UpdatePolicy(gReportsDB, "Enable_CustomerDisplay", Enable_Display_ChkBx.IsChecked)
            UpdatePolicy(gShipriteDB, "CustomerDisplay_Hide_SHIP", HideShip_ChkBx.IsChecked)

            For Each item As Ad_Image In ImageList
                Path = gDBpath & "\Ads\" & item.ImageName

                If Not System.IO.File.Exists(Path) Then
                    'newly selected picture needs to be copied to the SRN Ads folder
                    FileSystem.FileCopy(item.ImagePath, Path)
                    item.ImagePath = Path
                End If
            Next


            FileList = System.IO.Directory.GetFiles(gDBpath & "\Ads").ToList
            For Each PicturePath As String In FileList
                index = ImageList.FindIndex(Function(x) x.ImagePath = PicturePath)
                If index = -1 Then
                    'picture removed by user from the list, delete the file.
                    My.Computer.FileSystem.DeleteFile(PicturePath)
                End If
            Next


            SaveImage(Logo_Image, gDBpath & "\Ads\Logo\", Logo_Img)
            SaveImage(POS_Ad_Image, gDBpath & "\Ads\POS\", Logo_Img)


            MsgBox("Changes Saved Successfully!", vbInformation)


        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Sub

    Public Shared Sub SaveImage(img As Ad_Image, Path As String, ImgSource As Image)
        If Not IsNothing(img) Then
            'delete old Logo
            For Each deleteFile In IO.Directory.GetFiles(Path, "*.*", IO.SearchOption.TopDirectoryOnly)
                IO.File.Delete(deleteFile)
            Next

            'save new logo
            FileSystem.FileCopy(img.ImagePath, Path & img.ImageName)
        Else

            'Logo removed / non-existant
            For Each deleteFile In IO.Directory.GetFiles(Path, "*.*", IO.SearchOption.TopDirectoryOnly)
                If IsNothing(ImgSource.Source) Then
                    IO.File.Delete(deleteFile)
                End If
            Next
        End If
    End Sub

    Private Sub Load_Ad_Images()

        Dim Image As Ad_Image
        Dim FileList As List(Of String)

        If Not System.IO.Directory.Exists(gDBpath & "\Ads") Then
            System.IO.Directory.CreateDirectory(gDBpath & "\Ads")
            System.IO.Directory.CreateDirectory(gDBpath & "\Ads\Logo")
            System.IO.Directory.CreateDirectory(gDBpath & "\Ads\POS")
        End If


        FileList = System.IO.Directory.GetFiles(gDBpath & "\Ads").ToList

        ImageList = New List(Of Ad_Image)

        For Each PicturePath As String In FileList
            If PicturePath.Contains(".jpg") Or PicturePath.Contains(".jpeg") Or PicturePath.Contains(".png") Then
                Image = New Ad_Image
                Image.ImageName = Get_FileName(PicturePath)
                Image.ImagePath = PicturePath
                Image.BitImage = GetBitMapImage(PicturePath)

                ImageList.Add(Image)
            End If
        Next

        Images_LB.ItemsSource = ImageList
        Images_LB.Items.Refresh()

    End Sub

    Private Sub DeleteImage_Btn_Click(sender As Object, e As RoutedEventArgs) Handles DeleteImage_Btn.Click

        If Images_LB.SelectedIndex = -1 Then
            MsgBox("No Image selected. Please select a image first!", vbExclamation)
            Exit Sub
        End If

        If vbYes = MsgBox("Are you sure you want to remove the selected image?", vbQuestion + vbYesNo, "Delete Image") Then
            ImageList.Remove(Images_LB.SelectedItem)
            Images_LB.Items.Refresh()
        End If
    End Sub

    Private Sub AddImage_Btn_Click(sender As Object, e As RoutedEventArgs) Handles AddImage_Btn.Click
        Dim Image As Ad_Image
        Dim op As OpenFileDialog = New OpenFileDialog()
        op.Title = "Select a picture"
        op.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" & "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" & "Portable Network Graphic (*.png)|*.png"
        op.Multiselect = True

        If op.ShowDialog() = True Then
            For Each imagePath As String In op.FileNames
                Image = New Ad_Image
                Image.ImageName = Get_FileName(imagePath)
                Image.ImagePath = imagePath
                Image.BitImage = GetBitMapImage(imagePath)

                ImageList.Add(Image)
            Next

        End If

        Images_LB.Items.Refresh()
    End Sub

    '----------- LOGO ------------------

    Private Sub Select_Logo_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Select_Logo_Btn.Click

        Dim op As OpenFileDialog = New OpenFileDialog()
        op.Title = "Select a picture"
        op.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" & "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" & "Portable Network Graphic (*.png)|*.png"

        If op.ShowDialog() = True Then
            Logo_Image = New Ad_Image

            Logo_Image.ImageName = Get_FileName(op.FileName)
            Logo_Image.ImagePath = op.FileName
            Logo_Image.BitImage = GetBitMapImage(op.FileName)

            Logo_Img.Source = Logo_Image.BitImage
        End If
    End Sub

    Private Sub Delete_Logo_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Delete_Logo_Btn.Click
        If vbYes = MsgBox("Are you sure you want to remove the selected Logo?", vbQuestion + vbYesNo, "Remove Image") Then
            Logo_Image = Nothing
            Logo_Img.Source = Nothing

        End If
    End Sub

    Private Sub Load_Logo()
        Dim FileList = System.IO.Directory.GetFiles(gDBpath & "\Ads\Logo").ToList

        If FileList.Count <> 0 Then
            Logo_Img.Source = GetBitMapImage(FileList(0))
        End If
    End Sub




    '---------------- POS Ad Image --------------------------------------

    Private Sub Load_POS_Ad()
        Dim FileList = System.IO.Directory.GetFiles(gDBpath & "\Ads\POS").ToList

        If FileList.Count <> 0 Then
            POS_Ad_Img.Source = GetBitMapImage(FileList(0))
        End If
    End Sub

    Private Sub Select_POS_Ad_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Select_POS_Ad_Btn.Click

        Dim op As OpenFileDialog = New OpenFileDialog()
        op.Title = "Select a picture"
        op.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" & "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" & "Portable Network Graphic (*.png)|*.png"

        If op.ShowDialog() = True Then
            POS_Ad_Image = New Ad_Image

            POS_Ad_Image.ImageName = Get_FileName(op.FileName)
            POS_Ad_Image.ImagePath = op.FileName
            POS_Ad_Image.BitImage = GetBitMapImage(op.FileName)

            POS_Ad_Img.Source = POS_Ad_Image.BitImage
        End If
    End Sub

    Private Sub Delete_POS_Ad_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Delete_POS_Ad_Btn.Click
        If vbYes = MsgBox("Are you sure you want to remove the selected Image", vbQuestion + vbYesNo, "Remove Image") Then
            POS_Ad_Image = Nothing
            POS_Ad_Img.Source = Nothing

        End If
    End Sub

#End Region
End Class


