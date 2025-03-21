Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Data
Imports System.Windows.Documents
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Media.Imaging
Imports System.Windows.Navigation
Imports System.Windows.Shapes
Imports Topaz.SigPlusNET
Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Data
Imports System.Windows.Media.Brushes


Public Class SigPlusPad

    Private Sub SigPlusPad_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Try
            SigPlusNET1.SetTabletState(1)
            SigPlusNET1.SetImageXSize(800) '50
            SigPlusNET1.SetImageYSize(480) ' 150

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Failed to load Signature Pad")
        End Try
    End Sub

    Private Sub cmdOk_Click(sender As Object, e As RoutedEventArgs) Handles cmdOk.Click
        Try
            SigPlusNET1.SetJustifyMode(5)

            Dim myimage As System.Drawing.Image
            myimage = SigPlusNET1.GetSigImage()
            myimage.Save(Signature_FileName, Signature_ImageType)

            SigPlusNET1.SetJustifyMode(0)

            _SigPlusPad.Signature_IsSaved = _Files.IsFileExist(Signature_FileName, False)
            _SigPlusPad.Signature_FileName = Signature_FileName

            Dim converter As New System.Drawing.ImageConverter
            _SigPlusPad.Signature_ByteStream = converter.ConvertTo(myimage, GetType(Byte()))

            Me.Close()

        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Failed to save the Signature...")
        End Try
    End Sub

    Private Sub cmdClear_Click(sender As Object, e As RoutedEventArgs) Handles cmdClear.Click
        Try
            _SigPlusPad.Signature_IsSaved = False
            SigPlusNET1.ClearTablet()
        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Failed to clear the Signature...")
        End Try
    End Sub

    Private Sub cmdExit_Click(sender As Object, e As RoutedEventArgs) Handles cmdExit.Click
        Try
            _SigPlusPad.Signature_IsSaved = False
            Me.Close()
        Catch ex As Exception
            _MsgBox.ErrorMessage(ex, "Failed to exit...")
        End Try
    End Sub
End Class
