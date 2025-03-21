Public Class SpeeDee
    Public Const CarrierName As String = "SpeeDee"

    Public Shared Function CheckSpeeDee_Zip_Availability() As Boolean
        Try
            Dim path As String = gZoneTablesPath & "\SPEE-DEE\ZoneTables.csv"
            Dim currentRow As String()
            Dim StartZip As Integer = 99999
            Dim EndZip As Integer = 0

            If IsFileExist(path, False) Then
                Using MyReader As New FileIO.TextFieldParser(path)
                    MyReader.TextFieldType = FileIO.FieldType.Delimited
                    MyReader.SetDelimiters(",")


                    While Not MyReader.EndOfData
                        currentRow = MyReader.ReadFields()

                        If IsNumeric(currentRow(1)) Then
                            If CInt(currentRow(1)) < StartZip Then
                                StartZip = currentRow(1)
                            End If

                            If CInt(currentRow(2)) > EndZip Then
                                EndZip = currentRow(2)
                            End If
                        End If

                    End While
                End Using

                If CInt(_StoreOwner.StoreOwner.Zip) > StartZip And CInt(_StoreOwner.StoreOwner.Zip) < EndZip Then
                    Return True
                Else
                    Return False
                End If
            Else
                Return False
            End If

        Catch ex As Exception
            MessageBox.Show(Err.Description)
            Return False
        End Try
    End Function

    Public Shared Function Add_SpeeDee_Services() As Boolean
        Dim buf As String
        Dim SQL As String
        Dim PanelRow As Integer
        Try

            If isSpeeDee_Exists() Then
                MsgBox("Spee-Dee is already present in the Master table!")
                Return False
            End If

            'Find which row to place SpeeDee on the panel screen.
            buf = IO_GetSegmentSet(gShipriteDB, "SELECT max(Panel_Row) as MaxRow from Master")
            If buf = "" Then
                PanelRow = 4
            Else
                PanelRow = ExtractElementFromSegment("MaxRow", buf, "4")
                PanelRow = PanelRow + 1
            End If


            SQL = "INSERT INTO Master ([SERVICE], [ZONE-TBL], [Description], [LEVEL1], [LEVEL2], [LEVEL3], [POSDEPT], [Carrier], [TYPE], [Panel_Row], [Panel_Column])" &
                   "VALUES('SPEEDEE-GND', 'SPEEDEE-GND', 'Spee-Dee Ground', 50, 50, 50, 'SPEE-DEE', 'SPEE-DEE', 'SPEE-DEE', " & PanelRow & ", 0)"


            If IO_UpdateSQLProcessor(gShipriteDB, SQL) <> -1 Then
                MsgBox("Spee-Dee services added succesfully!" & vbCrLf & vbCrLf & "Please go to Setup > Shipping Setup and Markups." & vbCrLf &
                     "You must setup your markups and enter pricing for accessorial charges." & vbCrLf & vbCrLf & "Restart ShipRite for changes to take effect", vbInformation, "Spee-Dee")
                Return True
            Else
                MsgBox("Could not add SpeeDee service")
                Return False
            End If
        Catch ex As Exception
            MessageBox.Show(Err.Description)
        End Try
    End Function

    Public Shared Function isSpeeDee_Exists() As Boolean
        If IO_GetSegmentSet(gShipriteDB, "Select Service from Master WHERE Type='SPEE-DEE'") <> "" Then
            Return True
        Else
            Return False
        End If
    End Function


    Public Shared Sub CheckSpeeDee_Zones()
        Dim buf As String


        If Not isSpeeDee_Exists() Then
            'User doesn't use SpeeDee
            Exit Sub
        End If

        'check if zone table is blank.
        buf = IO_GetSegmentSet(gSpeeDeeZoneDB, "SELECT first(ID) as ID from [SPEEDEE-GND]")

        If ExtractElementFromSegment("ID", buf, "") <> "" Then
            'zone table is already populated, exit
            Exit Sub
        End If

        'populate zone table from csv.
        Dim path As String = gZoneTablesPath & "\SPEE-DEE\ZoneTables.csv"
        Dim currentRow As String()
        Dim StoreZip As Integer = CInt(_StoreOwner.StoreOwner.Zip)
        Dim StartZip As Integer
        Dim EndZip As Integer
        Dim ZipTable As String = ""

        If IsFileExist(path, False) Then
            Using MyReader As New FileIO.TextFieldParser(path)
                MyReader.TextFieldType = FileIO.FieldType.Delimited
                MyReader.SetDelimiters(",")


                While Not MyReader.EndOfData
                    currentRow = MyReader.ReadFields()

                    If IsNumeric(currentRow(1)) Then
                        StartZip = currentRow(1)
                        EndZip = currentRow(2)

                        If StoreZip >= StartZip And StoreZip <= EndZip Then
                            ZipTable = currentRow(3) & ".csv"
                            Exit While
                        End If

                    End If

                End While
            End Using

        End If


        If ZipTable <> "" Then
            Load_SpeeDeeZones(ZipTable)
        End If

    End Sub

    Public Shared Sub Load_SpeeDeeZones(CSVFile As String)
        Dim path As String = gZoneTablesPath & "\SPEE-DEE\" & CSVFile
        Dim currentRow As String()
        Dim Segment As String
        Dim schema As String = IO_GetFieldsCollection(gSpeeDeeZoneDB, "SPEEDEE-GND", "", True, False, True)
        Dim SQL As String


        If IsFileExist(path, False) Then
            Using MyReader As New FileIO.TextFieldParser(path)
                MyReader.TextFieldType = FileIO.FieldType.Delimited
                MyReader.SetDelimiters(",")


                While Not MyReader.EndOfData
                    currentRow = MyReader.ReadFields()

                    If IsNumeric(currentRow(1)) Then

                        Segment = ""
                        Segment = AddElementToSegment(Segment, "LOZIP", currentRow(1))
                        Segment = AddElementToSegment(Segment, "HIZIP", currentRow(2))
                        Segment = AddElementToSegment(Segment, "ZONE", currentRow(3))

                        SQL = MakeInsertSQLFromSchema("[SPEEDEE-GND]", Segment, schema, True)

                        IO_UpdateSQLProcessor(gSpeeDeeZoneDB, SQL)

                    End If

                End While
            End Using

        End If
    End Sub

End Class
