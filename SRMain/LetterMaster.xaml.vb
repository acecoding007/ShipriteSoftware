Imports System.Text.RegularExpressions
Imports System.Data.SqlClient
Imports System.Data
Imports System.StringSplitOptions
Imports System.IO
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Media
Imports System.Drawing.Printing
Imports SHIPRITE.ShipRiteReports

Imports System.Windows.Controls.Primitives
Public Class LetterMaster
    Inherits CommonWindow

    Public Contact_List As List(Of Contact_listing)
    Public Current_Contact_List As List(Of Contact_listing)
    Private Shipper_flag As Boolean = False
    Private Consignee_flag As Boolean = False
    Private AR_flag As Boolean = False
    Private MBX_flag As Boolean = False
    Public namefrom As String
    Public nameto As String
    Public city As String
    Public state As String
    Public zip As String

    Public SalesVolumeFrom As Decimal
    Public SalesVolumeTo As Decimal

    Public ShippingVolumeFrom As Decimal
    Public ShippingVolumeTo As Decimal

    Public PackageCountFrom As Integer
    Public PackageCountTo As Integer

    Public ShippingSalesDateTo As Date
    Public ShippingSalesDateFrom As Date

    Public FirstDateFrom As Date
    Public FirstDateTo As Date

    Public LastContactDateFrom As Date
    Public LastContactDateTo As Date

    Dim TemplatePosition As Integer = 0
    Dim EmailTemplates As New List(Of EmailTemplate)()
    Dim EmailContentUnsaved As Boolean = False

    Dim Types As New List(Of String)()

    Dim Message_list_ename As String
    Dim Pre_ename As String






    Public SQL As String

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

    Private Sub LetterMaster_loaded() Handles Me.Loaded

        SQL = "SELECT Contacts.ID, Contacts.Name, Contacts.Email, Contacts.Phone, Contacts.Addr1, Contacts.Addr2, Contacts.City, Contacts.State, Contacts.Zip, SUM(Payments.Charge) As SalesVolume, Contacts.ShippingVolume, Contacts.PackageCount, Contacts.FName, Contacts.LName
        FROM (Contacts
        LEFT JOIN Manifest ON Manifest.SID = Contacts.ID )
        LEFT JOIN Payments ON Payments.SoldTo = Contacts.ID 
        WHERE 1 AND Payments.Desc = 'Sales'"

        'Display_Contact_List()

    End Sub
    Private Sub Load_Contacting_list()
        Try

            Dim current_item As Contact_listing

            Contact_List = New List(Of Contact_listing)

            Dim querycount As Integer = 0


            'Load data into temporary hidden listview. 
            Load_Temp_ContactList()
            Contact_List.Clear()
            'from temporary listview, load data into the inventory list.
            For Each row As System.Data.DataRowView In TempData_CL.Items
                querycount += 1

                current_item = New Contact_listing

                current_item.ID = row.Item(0)



                current_item.Name = row.Item(1)

                current_item.Email = row.Item(2)

                If row.Item(3) = "" Then row.Item(3) = 0
                current_item.SMS = row.Item(3)

                If row.Item(4) = "" Then row.Item(4) = 0
                current_item.Address = row.Item(4)

                If row.Item(5) = "" Then row.Item(5) = 0
                current_item.City = row.Item(5)

                If row.Item(6) = "" Then row.Item(6) = 0
                current_item.State = row.Item(6)

                If row.Item(7) = "" Then row.Item(7) = 0
                current_item.Zip = row.Item(7)

                If row.Item(8) = "" Then row.Item(8) = 0
                current_item.SalesVolume = row.Item(8)

                If row.Item(9) = "" Then row.Item(9) = 0
                current_item.ShippingVolume = row.Item(9)

                If row.Item(10) = "" Then row.Item(10) = 0
                current_item.PackageCount = row.Item(10)




                Contact_List.Add(current_item)

            Next

            Letter_master.ItemsSource = Contact_List
            query_count.Text = querycount.ToString()
            query_count2.Text = querycount.ToString()



            Letter_master.Items.Refresh()






        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try

    End Sub

    Public Sub Load_Temp_ContactList()
        Try

            TempData_CL.DataContext = Nothing
            TempData_CL.View = New GridView
            TempData_CL.ItemsSource = Nothing
            TempData_CL.Items.Clear()


            Dim DT As System.Data.DataTable = New System.Data.DataTable

            Dim searchGrid As GridView = TempData_CL.View

            searchGrid.AllowsColumnReorder = False


            DT.Columns.Add("ID")
            DT.Columns.Add("Name")
            DT.Columns.Add("Email")
            DT.Columns.Add("SMS")
            DT.Columns.Add("Address")
            DT.Columns.Add("City")
            DT.Columns.Add("State")
            DT.Columns.Add("Zip")
            DT.Columns.Add("SalesVolume")
            DT.Columns.Add("ShippingVolume")
            DT.Columns.Add("PackageCount")




            TempData_CL.View = searchGrid

            IO_LoadListView(TempData_CL, DT, gShipriteDB, SQL, 11)

            TempData_CL.ItemsSource = DT.DefaultView

        Catch ex As Exception

            MessageBox.Show(Err.Description)
        End Try
    End Sub



    Public Sub Process_Selection_Click(sender As Object, e As RoutedEventArgs) Handles Process_Selection.Click
        Dim RecordCT As Long = 0
        Dim TransRecordSet As RecordSetDefinition
        Dim ret As Integer

        salesVolumeColumn.Width = 0
        shippingVolumeColumn.Width = 0
        packageCountColumn.Width = 0

        SQL = SQL.Replace(" AND Name BETWEEN '" & namefrom & "'AND'" & nameto & "'", "")
        SQL = SQL.Replace(" AND City LIKE '%" & city & "%'", "")
        SQL = SQL.Replace(" AND State LIKE '%" & state & "%'", "")
        SQL = SQL.Replace(" AND Zip LIKE '%" & zip & "%'", "")

        namefrom = Name_From.Text
        If String.IsNullOrEmpty(namefrom) Then
            namefrom = "a"
        End If
        nameto = Name_To.Text
        If String.IsNullOrEmpty(nameto) Then
            nameto = "z"
        End If
        city = City_inc.Text
        state = State_inc.Text
        zip = Zip_inc.Text

        Date.TryParse(Shipping_Sales_Date_From.Text, ShippingSalesDateFrom)
        Date.TryParse(Shipping_Sales_Date_To.Text, ShippingSalesDateTo)


        Date.TryParse(First_Date_From.Text, FirstDateFrom)
        Date.TryParse(First_Date_To.Text, FirstDateTo)

        Date.TryParse(Last_ContactDate_From.Text, LastContactDateFrom)
        Date.TryParse(Last_ContactDate_To.Text, LastContactDateTo)





        Decimal.TryParse(Sales_Volume_From.Text, SalesVolumeFrom)
        Decimal.TryParse(Sales_Volume_To.Text, SalesVolumeTo)

        If SalesVolumeTo = 0 Then
            SalesVolumeTo = Decimal.MaxValue
        End If


        Decimal.TryParse(Shipping_Volumn_from.Text, ShippingVolumeFrom)
        Decimal.TryParse(Shipping_Volumn_to.Text, ShippingVolumeTo)

        If ShippingVolumeTo = 0 Then
            ShippingVolumeTo = Decimal.MaxValue
        End If

        Integer.TryParse(Package_Count_From.Text, PackageCountFrom)
        Integer.TryParse(Package_Count_To.Text, PackageCountTo)

        If PackageCountTo = 0 Then
            PackageCountTo = Integer.MaxValue
        End If


        SQL = "SELECT Contacts.ID, Contacts.Name, Contacts.Email, Contacts.Phone, Contacts.Addr1, Contacts.Addr2, Contacts.City, Contacts.State, Contacts.Zip, SUM(Payments.Charge) As SalesVolume, Contacts.ShippingVolume, Contacts.PackageCount, Contacts.FName, Contacts.LName
        FROM (Contacts
        LEFT JOIN Manifest ON Manifest.SID = Contacts.ID )
        LEFT JOIN Payments ON Payments.SoldTo = Contacts.ID 
        WHERE 1 AND Payments.Desc = 'Sales'"





        SQL &= " AND Name BETWEEN '" & namefrom & "'AND'" & nameto & "'"

        If Not String.IsNullOrEmpty(city) Then
            SQL &= " AND City LIKE '%" & city & "%'"
        End If

        If Not String.IsNullOrEmpty(state) Then
            SQL &= " AND State LIKE '%" & state & "%'"
        End If

        If Not String.IsNullOrEmpty(zip) Then
            SQL &= " AND Zip LIKE '%" & zip & "%'"
        End If



        If Not (ShippingVolumeFrom = 0 And ShippingVolumeTo = Decimal.MaxValue) Then
            shippingVolumeColumn.Width = 100
            SQL &= " AND ShippingVolume BETWEEN " & ShippingVolumeFrom & " AND " & ShippingVolumeTo & ""

        End If

        If Not (PackageCountFrom = 0 And PackageCountTo = Integer.MaxValue) Then
            packageCountColumn.Width = 100
            SQL &= " AND PackageCount BETWEEN " & PackageCountFrom & " AND " & PackageCountTo & ""
        End If




        If Not (ShippingSalesDateFrom = "12:00:00 AM" And ShippingSalesDateTo = "12:00:00 AM") Then
            If Not (PackageCountFrom = 0 And PackageCountTo = Integer.MaxValue And ShippingVolumeFrom = 0 And ShippingVolumeTo = Decimal.MaxValue) Then
                SQL &= " AND Manifest.Date BETWEEN #" & ShippingSalesDateFrom & "# AND #" & ShippingSalesDateTo & "#"

            End If

            If Not (SalesVolumeFrom = 0 And SalesVolumeTo = Decimal.MaxValue) Then
                SQL &= " AND Payments.Date BETWEEN #" & ShippingSalesDateFrom & "# AND #" & ShippingSalesDateTo & "#"
            End If
        End If

        If Not (FirstDateFrom = "12:00:00 AM" And FirstDateTo = "12:00:00 AM") Then
            SQL &= " AND Contacts.FirstDate BETWEEN #" & FirstDateFrom & "# AND #" & FirstDateTo & "#"
        End If

        If Not (LastContactDateFrom = "12:00:00 AM" And LastContactDateTo = "12:00:00 AM") Then
            SQL &= " AND Contacts.LastContactDate BETWEEN #" & LastContactDateFrom & "# AND #" & LastContactDateTo & "#"
        End If

        If Shipper.IsChecked Then
            SQL &= " AND Class = 'Shipper'"
        Else
            SQL = SQL.Replace(" AND Class = 'Shipper'", "")
        End If

        If Consignee.IsChecked Then
            SQL &= " AND Class = 'Consignee'"
        Else
            SQL = SQL.Replace(" AND Class = 'Consignee'", "")
        End If


        If AR.IsChecked Then
            SQL &= " AND AR <> ''"
        Else
            SQL = SQL.Replace(" AND Class = 'Consignee'", "")
        End If

        If MBX.IsChecked Then
            SQL &= " AND MBX = -1"
        Else
            SQL = SQL.Replace(" AND MBX = -1", "")
        End If


        SQL &= " GROUP BY Contacts.ID, Contacts.Name, Contacts.Email, Contacts.Phone, Contacts.Addr1, Contacts.Addr2, Contacts.City, Contacts.State, Contacts.Zip, SalesVolume, Contacts.ShippingVolume, Contacts.PackageCount, Contacts.FName, Contacts.LName"

        If Not (SalesVolumeFrom = 0 And SalesVolumeTo = Decimal.MaxValue) Then
            salesVolumeColumn.Width = 100

            SQL &= " HAVING SUM(Payments.Charge) BETWEEN " & SalesVolumeFrom & " AND " & SalesVolumeTo & ""
        End If

        ret = IO_UpdateSQLProcessor(gReportWriter, "DELETE * FROM ContactSalesData")

        RecordCT = IO_GetSegmentSetInToStructure(gShipriteDB, SQL, TransRecordSet)
        ret = io_DumpRecordsetToLocalTable(gReportWriter, TransRecordSet, "ContactSalesData")
        Load_Contacting_list()


    End Sub

    Public Sub Letter_Setup_Load(sender As Object, e As RoutedEventArgs) Handles LetterMaster_Window.Loaded
        load_email_combox()
        load_letter_combox()
        load_sms_combox()
    End Sub

    Private Sub Go_to_Process_Click(sender As Object, e As RoutedEventArgs) Handles Go_to_Process.Click

        Letter_Master_Tap.SelectedIndex = 1
    End Sub

    Private lastFocusedRichTextBox As RichTextBox = Nothing

    Private Sub Letter_Content_GotFocus(sender As Object, e As RoutedEventArgs) Handles Letter_Content.GotFocus
        lastFocusedRichTextBox = Letter_Content
    End Sub

    Private Sub Email_content_GotFocus(sender As Object, e As RoutedEventArgs) Handles Email_content.GotFocus
        lastFocusedRichTextBox = Email_content
    End Sub

    Private Sub SMS_Content_GotFocus(sender As Object, e As RoutedEventArgs) Handles SMS_Content.GotFocus
        lastFocusedRichTextBox = SMS_Content
    End Sub

    Private Sub letter_setup_param_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles letter_setup_param.SelectionChanged
        Dim setup_param As ComboBoxItem = CType(letter_setup_param.SelectedItem, ComboBoxItem)

        If setup_param IsNot Nothing AndAlso lastFocusedRichTextBox IsNot Nothing Then
            Dim selectedText As String = "%" & setup_param.Content.ToString() & "%"
            InsertTextAtCaret(lastFocusedRichTextBox, selectedText)


        End If
    End Sub

    Private Sub InsertTextAtCaret(richTextBox As RichTextBox, text As String)
        Dim caretPosition As TextPointer = richTextBox.CaretPosition
        caretPosition.InsertTextInRun(text)
    End Sub
#Region "Email Setup"

    Private Sub E_Name_changed(sender As Object, e As RoutedEventArgs) Handles E_Name.SelectionChanged
        Dim emailname As String = E_Name.SelectedValue
        Dim emailsubject As String = ""
        Dim emailcontent As String = ""

        If emailname <> "" Then
            emailsubject = GetPolicyData(gShipriteDB, "Letter_E_" & Regex.Replace(emailname, "[ ]", String.Empty) & "Subject", "")
            emailcontent = GetPolicyData(gShipriteDB, "Letter_E_" & Regex.Replace(emailname, "[ ]", String.Empty) & "Content", "")
        Else

        End If

        If Not String.IsNullOrEmpty(emailcontent) Then
            ' There is content to be had
            '  Create a temporary file
            Dim tempFile As String = My.Computer.FileSystem.GetTempFileName & Guid.NewGuid().ToString
            '  Put content of object into temporary file
            Dim writer As New System.IO.StreamWriter(tempFile)
            Dim content As String = _Convert.Base64ToString(emailcontent)
            writer.Write(content)
            writer.Close()
            '  Load temporary file to RichTextBox
            Dim tr As TextRange = New TextRange(Email_content.Document.ContentStart, Email_content.Document.ContentEnd)
            Dim stream As FileStream = New FileStream(tempFile, FileMode.Open)

            tr.Load(stream, DataFormats.Rtf)

            stream.Close()
            '  Delete the temporary file
            My.Computer.FileSystem.DeleteFile(tempFile)
        Else
            Email_content.Document.Blocks.Clear()
        End If

        Email_subject.Text = emailsubject



    End Sub

    Private Sub Email_content_TextChanged(sender As Object, e As TextChangedEventArgs) Handles Email_content.TextChanged
        ' Handle the text changed event here
        ' You can access the text using Email_content.Document property
        Dim emailcontent As String = New TextRange(Email_content.Document.ContentStart, Email_content.Document.ContentEnd).Text

        ' Do whatever you need with the email content
    End Sub

    Public Sub Add_Email_Clicked(sender As Object, e As RoutedEventArgs) Handles Add_Email.Click
        Dim emailname As String = E_Name.Text
        Dim Ename = GetPolicyData(gShipriteDB, "Letter_Ename", "")
        Dim evalues As String() = Ename.Split(";"c)

        If Not evalues.Contains(emailname) Then
            Ename = Ename & emailname & ";"
            UpdatePolicy(gShipriteDB, "Letter_Ename", Ename)
            save_letter_email(emailname)
            MsgBox("New Email Template added successfully")

        Else
            MsgBox("There is a same email template in DB")
        End If
        load_email_combox()
        E_Name.SelectedItem = emailname
    End Sub

    Public Sub Delete_Email_clicked(sender As Object, e As RoutedEventArgs) Handles Delete_Email.Click
        Dim emailname As String = E_Name.Text

        Dim Ename = GetPolicyData(gShipriteDB, "Letter_Ename", "")

        Ename = Ename.Replace(emailname & ";", String.Empty)

        UpdatePolicy(gShipriteDB, "Letter_Ename", Ename)

        Dim subject_ename As String = "Letter_E_" & Regex.Replace(emailname, "[ ]", String.Empty) & "Subject"
        Dim content_ename As String = "Letter_E_" & Regex.Replace(emailname, "[ ]", String.Empty) & "Content"


        Dim SQL1 As String = "DELETE FROM Policy WHERE ElementName ='" & subject_ename & "'"
        IO_UpdateSQLProcessor(gShipriteDB, SQL1)

        Dim SQL2 As String = "DELETE FROM Policy WHERE ElementName ='" & content_ename & "'"
        IO_UpdateSQLProcessor(gShipriteDB, SQL2)


        load_email_combox()
    End Sub

    Public Sub Save_Email_Clicked(sender As Object, e As RoutedEventArgs) Handles Save_Email.Click
        Dim emailname As String = E_Name.SelectedValue
        save_letter_email(emailname)
    End Sub

    Public Sub save_letter_email(ename)
        Dim emailsubject As String = Email_subject.Text
        Dim emailcontent As String = RichBoxToString(Email_content)

        Try
            UpdatePolicy(gShipriteDB, "Letter_E_" & Regex.Replace(ename, "[ ]", String.Empty) & "Subject", emailsubject)
            UpdatePolicy(gShipriteDB, "Letter_E_" & Regex.Replace(ename, "[ ]", String.Empty) & "Content", emailcontent)
            MsgBox("Email is saved successfully")
        Catch ex As Exception

        End Try


    End Sub

    Public Sub load_email_combox()

        E_Name.Items.Clear()

        Dim Ename = GetPolicyData(gShipriteDB, "Letter_Ename", "")
        Dim evalues As String() = Ename.Split(";"c)
        Dim nb_Email As Integer = evalues.Length - 1
        For i = 1 To nb_Email
            ' For each in Types
            E_Name.Items.Add(evalues(i))
        Next

        If E_Name.Items.Count > 0 Then
            E_Name.SelectedIndex = 0 ' Select the first item as default
        End If

    End Sub
    Public Shared Function RichBoxToString(box As Controls.RichTextBox) As String

        Dim tempFile As String = My.Computer.FileSystem.GetTempFileName & Guid.NewGuid().ToString
        '  Save email content to temporary file
        Dim tr As TextRange = New TextRange(box.Document.ContentStart, box.Document.ContentEnd)
        Dim stream As FileStream = New FileStream(tempFile, FileMode.Create)
        tr.Save(stream, DataFormats.Rtf)
        stream.Close()
        '  Read temporary file to object
        Dim content As String = File.ReadAllText(tempFile)


        content = _Convert.StringToBase64(content)



        '  Delete the temporary file
        My.Computer.FileSystem.DeleteFile(tempFile)
        Return content

    End Function
#End Region

#Region "Letter Setup"
    Public Sub Add_Letter_Clicked(sender As Object, e As RoutedEventArgs) Handles Add_Letter.Click
        Dim lettername As String = Letter_Name.Text
        Dim Letternames = GetPolicyData(gShipriteDB, "Letter_Name", "")
        Dim evalues As String() = Letternames.Split(";"c)

        If Not evalues.Contains(lettername) Then
            Letternames = Letternames & lettername & ";"
            UpdatePolicy(gShipriteDB, "Letter_Name", Letternames)
            save_letter(lettername)
            MsgBox("New Letter Template added successfully")

        Else
            MsgBox("There is a same Letter template in DB")
        End If
        load_letter_combox()
        Letter_Name.SelectedItem = lettername
    End Sub

    Public Sub Save_ex_Letter_Clicked(sender As Object, e As RoutedEventArgs) Handles Save_ex_Letter.Click
        Dim lettername As String = Letter_Name.SelectedValue
        save_letter(lettername)
    End Sub

    Public Sub Delete_Letter_clicked(sender As Object, e As RoutedEventArgs) Handles Delete_Letter.Click
        Dim lettername As String = Letter_Name.Text

        Dim Letternames = GetPolicyData(gShipriteDB, "Letter_Name", "")

        Letternames = Letternames.Replace(lettername & ";", String.Empty)

        UpdatePolicy(gShipriteDB, "Letter_Name", Letternames)


        Dim letter_content_ename As String = "Letter_" & Regex.Replace(lettername, "[ ]", String.Empty) & "Content"




        Dim SQL2 As String = "DELETE FROM Policy WHERE ElementName ='" & letter_content_ename & "'"
        IO_UpdateSQLProcessor(gShipriteDB, SQL2)


        load_letter_combox()
    End Sub

    Private Sub Letter_Name_changed(sender As Object, e As RoutedEventArgs) Handles Letter_Name.SelectionChanged

        Dim lettername As String = Letter_Name.SelectedValue

        Dim lettercontent As String = ""

        If lettername <> "" Then

            lettercontent = GetPolicyData(gShipriteDB, "Letter_" & Regex.Replace(lettername, "[ ]", String.Empty) & "Content", "")
        Else

        End If

        If Not String.IsNullOrEmpty(lettercontent) Then
            ' There is content to be had
            '  Create a temporary file
            Dim tempFile As String = My.Computer.FileSystem.GetTempFileName & Guid.NewGuid().ToString
            '  Put content of object into temporary file
            Dim writer As New System.IO.StreamWriter(tempFile)
            Dim content As String = _Convert.Base64ToString(lettercontent)
            writer.Write(content)
            writer.Close()
            '  Load temporary file to RichTextBox
            Dim tr As TextRange = New TextRange(Letter_Content.Document.ContentStart, Letter_Content.Document.ContentEnd)
            Dim stream As FileStream = New FileStream(tempFile, FileMode.Open)

            tr.Load(stream, DataFormats.Rtf)

            stream.Close()
            '  Delete the temporary file
            My.Computer.FileSystem.DeleteFile(tempFile)
        Else
            Letter_Content.Document.Blocks.Clear()
        End If




    End Sub

    Private Sub field_list_SelectionChanged(sender As Object, e As RoutedEventArgs) Handles field_list.SelectionChanged
        ' Check if an item is selected
        Dim lettercontent As String = New TextRange(Letter_Content.Document.ContentStart, Letter_Content.Document.ContentEnd).Text




        If field_list.SelectedItem IsNot Nothing Then
            ' Toggle the FontWeight between Normal and Bold
            Dim itemContainer As ComboBoxItem = CType(field_list.ItemContainerGenerator.ContainerFromItem(field_list.SelectedItem), ComboBoxItem)
            If itemContainer IsNot Nothing Then
                If itemContainer.FontWeight = FontWeights.Normal Then
                    itemContainer.FontWeight = FontWeights.Bold
                Else
                    itemContainer.FontWeight = FontWeights.Normal
                End If
            End If
        End If

        Dim selected_value As String = field_list.SelectedValue

        If lettercontent.Contains("%" & selected_value & "%") Then
            lettercontent = lettercontent.Replace("%" & selected_value & "%", "")
        Else
            lettercontent &= "%" & selected_value & "%"
        End If

        Dim range As New TextRange(Letter_Content.Document.ContentStart, Letter_Content.Document.ContentEnd)
        range.Text = lettercontent
    End Sub

    Private Sub Letter_Content_TextChanged(sender As Object, e As TextChangedEventArgs) Handles Letter_Content.TextChanged
        ' Handle the text changed event here
        ' You can access the text using Email_content.Document property
        Dim lettercontent As String = New TextRange(Letter_Content.Document.ContentStart, Letter_Content.Document.ContentEnd).Text

        ' Do whatever you need with the email content
    End Sub

    Public Sub save_letter(lettername)

        Dim lettercontent As String = RichBoxToString(Letter_Content)
        Try
            UpdatePolicy(gShipriteDB, "Letter_" & Regex.Replace(lettername, "[ ]", String.Empty) & "Content", lettercontent)
            MsgBox("Letter is saved successfully")
        Catch ex As Exception

        End Try
    End Sub

    Public Sub load_letter_combox()

        Letter_Name.Items.Clear()

        Dim Ename = GetPolicyData(gShipriteDB, "Letter_Name", "")
        Dim evalues As String() = Ename.Split(";"c)
        Dim nb_letter As Integer = evalues.Length - 1

        For i = 1 To nb_letter
            ' For each in Types
            Letter_Name.Items.Add(evalues(i))
        Next

        If Letter_Name.Items.Count > 0 Then
            Letter_Name.SelectedIndex = 0 ' Select the first item as default
        End If

    End Sub

#End Region


#Region "SMS Setup"
    Public Sub Add_SMS_Clicked(sender As Object, e As RoutedEventArgs) Handles Add_SMS.Click
        Dim smsname As String = SMS_Name.Text
        Dim SMSNames = GetPolicyData(gShipriteDB, "Letter_smsName", "")
        Dim evalues As String() = SMSNames.Split(";"c)

        If Not evalues.Contains(smsname) Then
            SMSNames = SMSNames & smsname & ";"
            UpdatePolicy(gShipriteDB, "Letter_smsName", SMSNames)
            save_sms(smsname)
            MsgBox("New SMS Template added successfully")

        Else
            MsgBox("There is a same SMS template in DB")
        End If
        load_sms_combox()
        SMS_Name.SelectedItem = smsname
    End Sub

    Public Sub Save_SMS(sender As Object, e As RoutedEventArgs) Handles Save_ex_SMS.Click
        Dim smsname As String = SMS_Name.SelectedValue
        save_sms(smsname)
    End Sub

    Public Sub Delete_SMS_clicked(sender As Object, e As RoutedEventArgs) Handles Delete_SMS.Click
        Dim smsname As String = SMS_Name.Text

        Dim Letternames = GetPolicyData(gShipriteDB, "Letter_smsName", "")

        Letternames = Letternames.Replace(smsname & ";", String.Empty)

        UpdatePolicy(gShipriteDB, "Letter_smsName", Letternames)

        Dim sms_content_ename As String = "Letter_sms_" & Regex.Replace(smsname, "[ ]", String.Empty) & "Content"




        Dim SQL2 As String = "DELETE FROM Policy WHERE ElementName ='" & sms_content_ename & "'"
        IO_UpdateSQLProcessor(gShipriteDB, SQL2)


        load_sms_combox()
    End Sub

    Private Sub SMS_Name_changed(sender As Object, e As RoutedEventArgs) Handles SMS_Name.SelectionChanged

        Dim smsname As String = SMS_Name.SelectedValue

        Dim smscontent As String = ""

        If smsname <> "" Then

            smscontent = GetPolicyData(gShipriteDB, "Letter_sms_" & Regex.Replace(smsname, "[ ]", String.Empty) & "Content", "")
        Else

        End If

        If Not String.IsNullOrEmpty(smscontent) Then
            ' There is content to be had
            '  Create a temporary file
            Dim tempFile As String = My.Computer.FileSystem.GetTempFileName & Guid.NewGuid().ToString
            '  Put content of object into temporary file
            Dim writer As New System.IO.StreamWriter(tempFile)
            Dim content As String = _Convert.Base64ToString(smscontent)
            writer.Write(content)
            writer.Close()
            '  Load temporary file to RichTextBox
            Dim tr As TextRange = New TextRange(SMS_Content.Document.ContentStart, SMS_Content.Document.ContentEnd)
            Dim stream As FileStream = New FileStream(tempFile, FileMode.Open)

            tr.Load(stream, DataFormats.Rtf)

            stream.Close()
            '  Delete the temporary file
            My.Computer.FileSystem.DeleteFile(tempFile)
        Else
            SMS_Content.Document.Blocks.Clear()
        End If





    End Sub

    Private Sub SMS_Content_TextChanged(sender As Object, e As TextChangedEventArgs) Handles SMS_Content.TextChanged
        ' Handle the text changed event here
        ' You can access the text using Email_content.Document property
        Dim smscontent As String = New TextRange(SMS_Content.Document.ContentStart, SMS_Content.Document.ContentEnd).Text

        ' Do whatever you need with the email content
    End Sub

    Public Sub save_sms(smsname)

        Dim lettercontent As String = RichBoxToString(SMS_Content)

        Try
            UpdatePolicy(gShipriteDB, "Letter_sms_" & Regex.Replace(smsname, "[ ]", String.Empty) & "Content", lettercontent)
            MsgBox("SMS is saved successfully")
        Catch ex As Exception

        End Try

    End Sub

    Public Sub load_sms_combox()

        SMS_Name.Items.Clear()

        Dim Ename = GetPolicyData(gShipriteDB, "Letter_smsName", "")
        Dim evalues As String() = Ename.Split(";"c)
        Dim nb_letter As Integer = evalues.Length - 1
        For i = 1 To nb_letter
            ' For each in Types
            SMS_Name.Items.Add(evalues(i))
        Next

        If SMS_Name.Items.Count > 0 Then
            SMS_Name.SelectedIndex = 0 ' Select the first item as default
        End If

    End Sub

#End Region

#Region "Process Letters"

    Public Sub process_print_clicked(sender As Object, e As RoutedEventArgs) Handles process_print.Click

        Send_Letter.Content = "PRINT"
        process_print.Background = New SolidColorBrush(Colors.LightSkyBlue)

        process_email.Background = New SolidColorBrush(Colors.LightGray)
        process_sms.Background = New SolidColorBrush(Colors.LightGray)
        Message_list_ename = GetPolicyData(gShipriteDB, "Letter_Name", "")
        Pre_ename = "Letter_"
        load_message_combobox()

    End Sub
    Public Sub process_email_clicked(sender As Object, e As RoutedEventArgs) Handles process_email.Click
        Send_Letter.Content = "SEND"
        process_email.Background = New SolidColorBrush(Colors.LightSkyBlue)
        process_print.Background = New SolidColorBrush(Colors.LightGray)
        process_sms.Background = New SolidColorBrush(Colors.LightGray)
        Message_list_ename = GetPolicyData(gShipriteDB, "Letter_EName", "")
        Pre_ename = "Letter_E_"
        load_message_combobox()
    End Sub
    Public Sub process_sms_clicked(sender As Object, e As RoutedEventArgs) Handles process_sms.Click
        Send_Letter.Content = "SEND"
        process_sms.Background = New SolidColorBrush(Colors.LightSkyBlue)
        process_email.Background = New SolidColorBrush(Colors.LightGray)
        process_print.Background = New SolidColorBrush(Colors.LightGray)
        Message_list_ename = GetPolicyData(gShipriteDB, "Letter_smsName", "")
        Pre_ename = "Letter_sms_"
        load_message_combobox()

    End Sub

    Public Sub load_message_combobox()
        Message_list.Items.Clear()


        Dim evalues As String() = Message_list_ename.Split(";"c)
        Dim nb_letter As Integer = evalues.Length - 1
        For i = 1 To nb_letter
            ' For each in Types
            Message_list.Items.Add(evalues(i))
        Next

        If Message_list.Items.Count > 0 Then
            Message_list.SelectedIndex = 0 ' Select the first item as default
        End If
    End Sub

    Private Sub Message_list_changed(sender As Object, e As RoutedEventArgs) Handles Message_list.SelectionChanged

        Dim messagename As String = Message_list.SelectedValue

        Dim messagecontent As String = ""

        If messagename <> "" Then

            messagecontent = GetPolicyData(gShipriteDB, Pre_ename & Regex.Replace(messagename, "[ ]", String.Empty) & "Content", "")
        Else

        End If

        If Not String.IsNullOrEmpty(messagecontent) Then
            ' There is content to be had
            '  Create a temporary file
            Dim tempFile As String = My.Computer.FileSystem.GetTempFileName & Guid.NewGuid().ToString
            '  Put content of object into temporary file
            Dim writer As New System.IO.StreamWriter(tempFile)
            Dim content As String = _Convert.Base64ToString(messagecontent)
            writer.Write(content)
            writer.Close()
            '  Load temporary file to RichTextBox
            Dim tr As TextRange = New TextRange(Message_content.Document.ContentStart, Message_content.Document.ContentEnd)
            Dim stream As FileStream = New FileStream(tempFile, FileMode.Open)

            tr.Load(stream, DataFormats.Rtf)

            stream.Close()
            '  Delete the temporary file
            My.Computer.FileSystem.DeleteFile(tempFile)
        Else
            Message_content.Document.Blocks.Clear()
        End If






    End Sub

    Private Sub Message_content_TextChanged(sender As Object, e As TextChangedEventArgs) Handles Message_content.TextChanged
        ' Handle the text changed event here
        ' You can access the text using Email_content.Document property
        Dim smscontent As String = New TextRange(Message_content.Document.ContentStart, Message_content.Document.ContentEnd).Text

        ' Do whatever you need with the email content
    End Sub
    Public Function GetRichTextBoxContent() As String
        Dim textRange As New TextRange(Message_content.Document.ContentStart, Message_content.Document.ContentEnd)
        Return textRange.Text
    End Function
    Public Sub Send_Letter_Clicked() Handles Send_Letter.Click
        If Send_Letter.Content = "PRINT" Then

            Try
                Dim textRange As New TextRange(Message_content.Document.ContentStart, Message_content.Document.ContentEnd)
                Dim messageContent As String = textRange.Text

                Dim Name_param As String = "0"
                Dim FName_param As String = "0"
                Dim Lname_param As String = "0"
                Dim Add1_param As String = "0"
                Dim Add2_param As String = "0"

                Dim City_param As String = "0"
                Dim Zip_param As String = "0"
                Dim State_param As String = "0"
                Dim Phone_param As String = "0"

                If messageContent.Contains("%Name%") Then
                    Name_param = "1"
                End If

                If messageContent.Contains("%First Name%") Then
                    FName_param = "1"
                End If
                If messageContent.Contains("%Last Name%") Then
                    Lname_param = "1"
                End If
                If messageContent.Contains("%Address1%") Then
                    Add1_param = "1"
                End If
                If messageContent.Contains("%Address2%") Then
                    Add2_param = "1"
                End If
                If messageContent.Contains("%City%") Then
                    City_param = "1"
                End If
                If messageContent.Contains("%State") Then
                    State_param = "1"
                End If
                If messageContent.Contains("%Zip%") Then
                    Zip_param = "1"
                End If


                Cursor = Cursors.Wait

                Dim report As New _ReportObject
                Dim message_name = Message_list.SelectedValue
                If message_name = "Avery 5160 Labels" Then
                    report.ReportName = "Avery_5160.rpt"

                    report.ReportParameters.Add(FName_param)
                    report.ReportParameters.Add(Lname_param)
                    report.ReportParameters.Add(Add1_param)
                    report.ReportParameters.Add(Add2_param)
                    report.ReportParameters.Add(City_param)
                    report.ReportParameters.Add(State_param)
                    report.ReportParameters.Add(Zip_param)
                    report.ReportParameters.Add(Name_param)

                Else
                    report.ReportName = "Letter.rpt"




                    Dim sql As String
                    Load_Temp_ContactList()
                    IO_UpdateSQLProcessor(gReportWriter, "DELETE FROM letters_table;")
                    Dim template_Letter As String
                    For Each row As System.Data.DataRowView In TempData_CL.Items

                        template_Letter = getLetterTemplate(messageContent, row.Item(1))

                        sql = "INSERT INTO letters_table (letter) VALUES ('" & template_Letter & "')"

                        IO_UpdateSQLProcessor(gReportWriter, sql)
                    Next
                End If


                Dim reportPrev As New ReportPreview(report)
                Cursor = Cursors.Arrow
                reportPrev.ShowDialog()

            Catch ex As Exception : _MsgBox.ErrorMessage(ex, "Failed to report [Post Office Quarterly]...")
            Finally : Cursor = Cursors.Arrow
            End Try
        Else
            Dim current_item As Contact_listing
            Dim querycount As Integer = 0
            Contact_List = New List(Of Contact_listing)
            Contact_List.Clear()
            Load_Temp_ContactList()
            Dim template_Email As EmailTemplate
            For Each row As System.Data.DataRowView In TempData_CL.Items
                querycount += 1

                current_item = New Contact_listing

                current_item.Name = row.Item(1)

                current_item.Email = row.Item(2)


                Contact_List.Add(current_item)



                template_Email = getEmailTemplate("Letter_E_Email", current_item.Name)
                sendEmail(current_item.Email, template_Email)

            Next

        End If

    End Sub


#End Region

End Class
