Imports SHIPRITE.ShipRiteReports
Public Class addressbook





    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        Dim namefrom As String
        Dim nameto As String
        Dim cityfrom As String
        Dim cityto As String
        Dim statefrom As String
        Dim stateto As String
        Dim zipfrom As String
        Dim zipto As String
        Dim formula As String
        ' Retrieve input values
        namefrom = name_from.Text
        nameto = name_to.Text
        cityfrom = city_from.Text
        cityto = city_to.Text
        statefrom = state_from.Text
        stateto = state_to.Text
        zipfrom = zip_from.Text
        zipto = zip_to.Text

        formula = ""

        If namefrom IsNot "" Then
            If formula.Length > 0 Then
                formula &= "And {Contacts.Name} >= '" & namefrom & "'"
            Else
                formula &= " {Contacts.Name} >= '" & namefrom & "'"
            End If
        End If


        If nameto IsNot "" Then

            If formula.Length > 0 Then
                formula &= "And  {Contacts.Name} <= '" & nameto & "'"
            Else
                formula &= "  {Contacts.Name} <= '" & nameto & "'"
            End If


        End If
        If cityfrom IsNot "" Then
            If formula.Length > 0 Then
                formula &= "And {Contacts.City} >= '" & cityfrom & "'"
            Else
                formula &= " {Contacts.City} >= '" & cityfrom & "'"
            End If
        End If

        If cityto IsNot "" Then

            If formula.Length > 0 Then
                formula &= "And  {Contacts.City} <= '" & cityto & "'"
            Else
                formula &= "  {Contacts.City} <= '" & cityto & "'"
            End If


        End If

        If statefrom IsNot "" Then
            If formula.Length > 0 Then
                formula &= "And {Contacts.State} >= '" & statefrom & "'"
            Else
                formula &= " {Contacts.State} >= '" & statefrom & "'"
            End If
        End If


        If stateto IsNot "" Then

            If formula.Length > 0 Then
                formula &= "And  {Contacts.State} <= '" & stateto & "'"
            Else
                formula &= "  {Contacts.State} <= '" & stateto & "'"
            End If
        End If

        If zipfrom IsNot "" Then
            If formula.Length > 0 Then
                formula &= "And {Contacts.Zip} >= '" & zipfrom & "'"
            Else
                formula &= " {Contacts.Zip} >= '" & zipfrom & "'"
            End If
        End If


        If zipto IsNot "" Then

            If formula.Length > 0 Then
                formula &= "And  {Contacts.Zip} <= '" & zipto & "'"
            Else
                formula &= "  {Contacts.Zip} <= '" & zipto & "'"
            End If
        End If




        Try
            Cursor = Cursors.Wait
            Dim report As New ShipRiteReports._ReportObject()
            report.ReportName = "Contacts.rpt"
            report.ReportFormula = formula


            Dim reportPrev As New ReportPreview(report)
            Cursor = Cursors.Arrow
            reportPrev.ShowDialog()

        Catch ex As Exception
            ' Display error message if reporting fails
            _MsgBox.ErrorMessage(ex, "Failed to report [Vault]...")
        Finally
            Cursor = Cursors.Arrow
        End Try
    End Sub

    Private Sub Cancel_Click(sender As Object, e As RoutedEventArgs) Handles Cancel.Click
        Me.Close()
    End Sub


End Class
