Imports System.Drawing
Imports System.IO

Public Module _SigPlusPad

    Public Signature_ImageType As System.Drawing.Imaging.ImageFormat
    Public Signature_FileName As String
    Public Signature_IsSaved As Boolean
    Public Signature_ByteStream() As Byte

    Public Sub SigPlusPad_ShowForm()
        '_SigPlusPad.Signature_FileName = "C:\ShipRite\Fedex\Test\Signature.tif"
        '_SigPlusPad.Signature_ImageType = System.Drawing.Imaging.ImageFormat.Tiff

        'SigPlusPad.ShowDialog()
        'SigPlusPad.Dispose()

        Dim win As New SigPlusPad()
        win.ShowDialog()


    End Sub

End Module
