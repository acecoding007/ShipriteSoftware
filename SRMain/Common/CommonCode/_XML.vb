Imports System.xml
Imports System.IO
Imports System.Net

Public Module _XML

    Private m_TagPrefix As String '' add 'v1:' to tag name, like <v1:TagName>

    Private Sub error_DebugPint(ByVal routineName As String, ByVal errorDesc As String, Optional ByVal nodeName As String = "")
        _Debug.Print_(String.Format("_XML.{0}({1}): {2}", routineName, nodeName, errorDesc))
    End Sub

    Public Property TagPrefix() As String
        Get
            Return m_TagPrefix
        End Get
        Set(ByVal value As String)
            m_TagPrefix = value
        End Set
    End Property
    Private Function add_TagPrefix(ByVal tagName As String) As String
        add_TagPrefix = tagName '' assume.
        Try
            If Not tagName.Contains(_XML.TagPrefix) Then
                add_TagPrefix = String.Format("{0}{1}", _XML.TagPrefix, tagName)
            End If
        Catch ex As Exception : error_DebugPint("add_TagPrefix", Err.Description, tagName)
        End Try
    End Function


    Public Function Open_XMLDocument(ByRef retxDoc As XmlDocument) As Boolean
        '------------------------------------------------------------'Oleg - Date: July 13, 2007
        On Error GoTo Ooops
        ''
        'Create a new document object
        retxDoc = New XmlDocument
        retxDoc.PreserveWhitespace = False
        ''
Ooops:  Open_XMLDocument = (0 = Err.Number) : If Not 0 = Err.Number Then error_DebugPint("Open_XMLDocument", Err.Description)
        '------------------------------------------------------------
    End Function

    Public Function Create_VersionHeader(ByRef xdoc As Xml.XmlDocument, ByVal versionNo As String, Optional ByVal encodingCode As String = "") As Boolean
        '------------------------------------------------------------'Oleg - Date: July 13, 2007
        Dim xVer As XmlProcessingInstruction
        On Error GoTo Ooops
        If Not 0 = Len(encodingCode) Then encodingCode = " encoding=" & Chr(34) & encodingCode & Chr(34)
        xVer = xdoc.CreateProcessingInstruction("xml", "version=" & Chr(34) & versionNo & Chr(34) & encodingCode)
        xdoc.AppendChild(xVer)
Ooops:  Create_VersionHeader = (0 = Err.Number) : If Not 0 = Err.Number Then error_DebugPint("Create_VersionHeader", Err.Description)
        '------------------------------------------------------------
    End Function
    Public Function Create_RootElement(ByRef xdoc As Xml.XmlDocument, ByVal xRootName As String, ByRef retxRoot As Xml.XmlElement) As Boolean
        '------------------------------------------------------------'Oleg - Date: July 13, 2007
        On Error GoTo Ooops
        retxRoot = xdoc.CreateElement(xRootName)
Ooops:  Create_RootElement = (0 = Err.Number) : If Not 0 = Err.Number Then error_DebugPint("Create_RootElement", Err.Description, xRootName)
        '------------------------------------------------------------
    End Function
    Public Function Close_XMLDocument(ByRef xdoc As Xml.XmlDocument, ByRef xRoot As Xml.XmlElement) As Boolean
        '------------------------------------------------------------'Oleg - Date: July 13, 2007
        On Error GoTo Ooops
        xdoc.AppendChild(xRoot)
Ooops:  Close_XMLDocument = (0 = Err.Number) : If Not 0 = Err.Number Then error_DebugPint("Close_XMLDocument", Err.Description)
        '------------------------------------------------------------
    End Function
    Public Function Create_NextElement(ByRef xdoc As Xml.XmlDocument, ByVal elemName As String, ByRef retxElem As Xml.XmlElement, ByRef xRoot As Xml.XmlElement) As Boolean
        '------------------------------------------------------------'Oleg - Date: July 13, 2007
        Create_NextElement = True '' assume.
        Try
            retxElem = xdoc.CreateElement(elemName)
            xRoot.AppendChild(retxElem)
            ''
        Catch ex As Exception : error_DebugPint("Create_NextElement", Err.Description, elemName) : Create_NextElement = False
        End Try
        '------------------------------------------------------------
    End Function
    Public Function Create_ChildNode(ByRef xdoc As Xml.XmlDocument, ByVal elemName As String, ByRef retxElem As Xml.XmlElement, ByRef xNode As Xml.XmlNode, ByRef retxChildNode As Xml.XmlNode) As Boolean
        '------------------------------------------------------------'Oleg - Date: July 13, 2007
        On Error GoTo Ooops
        retxElem = xdoc.CreateElement(elemName)
        retxChildNode = xNode.AppendChild(retxElem)
Ooops:  Create_ChildNode = (0 = Err.Number) : If Not 0 = Err.Number Then error_DebugPint("Create_ChildNode", Err.Description, elemName)
        '------------------------------------------------------------
    End Function
    Public Function ChildNode_AssignValue(ByVal xNode As Xml.XmlNode, ByVal nodeValue As String) As Boolean
        '------------------------------------------------------------'Oleg - Date: July 13, 2007
        On Error GoTo Ooops
        If Not 0 = Len(nodeValue) Then '' if no value then skips adding and returns True
            xNode.Value = nodeValue
        End If
Ooops:  ChildNode_AssignValue = (0 = Err.Number) : If Not 0 = Err.Number Then error_DebugPint("ChildNode_AssignValue", Err.Description, xNode.Name & "=" & nodeValue)
        '------------------------------------------------------------
    End Function
    Public Function Create_NextChildNode(ByRef xdoc As Xml.XmlDocument, ByVal childName As String, ByRef xParent As Xml.XmlElement, ByRef xElem As Xml.XmlElement, ByRef retxNode As Xml.XmlNode) As Boolean
        '------------------------------------------------------------'Oleg - Date: July 13, 2007
        On Error GoTo Ooops
        xElem = xdoc.CreateElement(childName)
        retxNode = xParent.AppendChild(xElem)
Ooops:  Create_NextChildNode = (0 = Err.Number) : If Not 0 = Err.Number Then error_DebugPint("Create_NextChildNode", Err.Description, childName)
        '------------------------------------------------------------
    End Function
    Public Function Create_NextChildNode_WithValue(ByRef xdoc As Xml.XmlDocument, ByVal childName As String, ByVal childValue As String, ByRef xParent As Xml.XmlElement, ByRef xElem As Xml.XmlElement) As Boolean
        '------------------------------------------------------------'Oleg - Date: July 13, 2007
        Dim xNode As Xml.XmlNode = Nothing
        On Error GoTo Ooops
        If Not 0 = Len(childValue) Then  '' if no value then skips adding and returns True
            If _XML.Create_NextChildNode(xdoc, childName, xParent, xElem, xNode) Then
                Create_NextChildNode_WithValue = _XML.ChildNode_AssignValue(xNode, childValue)
            End If
        End If
Ooops:  If Not 0 = Err.Number Then error_DebugPint("Create_NextChildNode_WithValue", Err.Description, childName & "(" & childValue & ")")
        '------------------------------------------------------------
    End Function
    Public Function GetAllValues_ByNodeName(ByVal xdoc As Xml.XmlDocument, ByVal nodeName As String) As String
        '------------------------------------------------------------'Oleg - Date: July 24, 2007
        Dim i%
        On Error GoTo Ooops
        ''Debug_.Print_ xdoc.getElementsByTagName("Charge").LENGTH
        GetAllValues_ByNodeName = String.Empty '' assume.
        For i% = 0 To xdoc.GetElementsByTagName(nodeName).Count - 1
            If i% = 0 Then
                GetAllValues_ByNodeName = xdoc.GetElementsByTagName(nodeName).Item(i%).Value
            Else
                GetAllValues_ByNodeName = GetAllValues_ByNodeName & vbCr & xdoc.GetElementsByTagName(nodeName).Item(i%).Value
            End If
        Next i%
Ooops:  If Not 0 = Err.Number Then Debug.Print("GetAllValues_ByNodeName(): " & Err.Description)
        '------------------------------------------------------------
    End Function
    Public Function Dispose_XMLDocument(ByRef xdoc As Xml.XmlDocument) As Boolean
        '------------------------------------------------------------'Oleg - Date: July 13, 2007
        On Error GoTo Ooops
        xdoc = Nothing
Ooops:  Dispose_XMLDocument = (0 = Err.Number) : If Not 0 = Err.Number Then error_DebugPint("Dispose_XMLDocument", Err.Description)
        '------------------------------------------------------------
    End Function


    Public Function Node_CreateChild(ByRef xdoc As Xml.XmlDocument, ByVal childName As String, ByVal xParent As Xml.XmlElement, ByRef xChild As Xml.XmlNode) As Boolean
        Node_CreateChild = False
        Try
            Dim xElem As Xml.XmlElement = xdoc.CreateElement(childName)
            xChild = xParent.AppendChild(xElem)
            Node_CreateChild = String.Equals(xChild.LocalName, childName)
        Catch ex As Exception : error_DebugPint("Node_CreateChild", Err.Description, childName)
        End Try
        ''
    End Function
    Public Function Node_AssignValue(ByVal xNode As Xml.XmlNode, ByVal nodeValue As String) As Boolean
        Node_AssignValue = False
        Try
            xNode.InnerText = nodeValue
            Node_AssignValue = String.Equals(xNode.InnerText, nodeValue)
        Catch ex As Exception : error_DebugPint("Node_AssignValue", Err.Description, nodeValue)
        End Try
        ''
    End Function
    Public Function Node_AssignValue(ByVal nodePath As String, ByVal nodeValue As String, ByRef xdoc As Xml.XmlDocument) As Boolean
        Node_AssignValue = False
        Dim xNode As XmlNode = xdoc.SelectSingleNode(nodePath) '("/CryTools/FarCry")
        If xNode IsNot Nothing Then
            _Debug.Print_(xNode.Name, "=", nodeValue)
            If Not 0 = nodeValue.Length Then
                xNode.InnerText = nodeValue
            End If
            Node_AssignValue = True
        End If
    End Function

    Public Function Element_CreateChild(ByRef xdoc As Xml.XmlDocument, ByVal childName As String, ByVal xParent As Xml.XmlElement, ByRef xChild As Xml.XmlElement) As Boolean
        Element_CreateChild = False
        Try
            Dim xElem As Xml.XmlElement = xdoc.CreateElement(childName)
            xChild = CType(xParent.AppendChild(xElem), XmlElement)
            Element_CreateChild = String.Equals(xChild.LocalName, childName)
        Catch ex As Exception : error_DebugPint("Element_CreateChild", ex.Message, childName)
        End Try
        ''
    End Function
    Public Function Element_CreateChild_WithValue(ByRef xdoc As Xml.XmlDocument, ByVal childName As String, ByVal childValue As String, ByVal xParent As Xml.XmlElement, ByRef xChild As Xml.XmlElement) As Boolean
        Element_CreateChild_WithValue = False
        Try
            Element_CreateChild_WithValue = (0 = childValue.Length)
            If Not Element_CreateChild_WithValue Then '' if no value then skips adding and returns True
                If _XML.Element_CreateChild(xdoc, childName, xParent, xChild) Then
                    Element_CreateChild_WithValue = _XML.Element_AssignValue(xChild, childValue)
                End If
            End If
        Catch ex As Exception : error_DebugPint("Element_CreateChild_WithValue", ex.Message, String.Format("{0}={1}", childName, childValue))
        End Try
        ''
    End Function
    Public Function Element_AssignValue(ByVal xElem As Xml.XmlElement, ByVal elemValue As String) As Boolean
        Element_AssignValue = False
        Try
            xElem.InnerText = elemValue
            Element_AssignValue = String.Equals(xElem.InnerText, elemValue)
        Catch ex As Exception : error_DebugPint("Element_AssignValue", ex.Message, elemValue)
        End Try
        ''
    End Function
    Public Function Element_AddAttribute(ByRef xElem As Xml.XmlElement, ByVal xAttrName As String, ByVal xAttrValue As String) As Boolean
        Element_AddAttribute = False
        Try
            If Not 0 = xAttrValue.Length Then
                xElem.SetAttribute(xAttrName, xAttrValue)
                Element_AddAttribute = _Controls.IsEqual(xAttrValue, _XML.Element_GetAttribute(xElem, xAttrName))
            End If
        Catch ex As Exception : error_DebugPint("1 Element_AddAttribute", ex.Message, xAttrValue)
        End Try
        ''
    End Function
    Public Function Element_AddAttribute(ByRef xElem As Xml.XmlElement, ByVal xAttrName As String, ByVal xAttrValue As String, ByVal xAttrURL As String) As Boolean
        Element_AddAttribute = False
        Try
            If Not 0 = xAttrValue.Length Then
                xElem.SetAttribute(xAttrName, xAttrURL, xAttrValue)
                Element_AddAttribute = _Controls.IsEqual(xAttrValue, _XML.Element_GetAttribute(xElem, xAttrName))
            End If
        Catch ex As Exception : error_DebugPint("2 Element_AddAttribute", ex.Message, xAttrValue)
        End Try
        ''
    End Function
    Public Function Element_GetAttribute(ByRef xElem As Xml.XmlElement, ByVal xAttrName As String, ByRef xAttrValue As String) As Boolean
        Element_GetAttribute = False
        Try
            xAttrValue = String.Empty '' assume.
            If Not 0 = xAttrName.Length Then
                xAttrValue = xElem.GetAttribute(xAttrName)
            End If
            Element_GetAttribute = Not 0 = xAttrValue.Length
        Catch ex As Exception : error_DebugPint("Element_GetAttribute", ex.Message, xAttrValue)
        End Try
        ''
    End Function
    Public Function Element_GetAttribute(ByRef xElem As Xml.XmlElement, ByVal xAttrName As String) As String
        ''
        Element_GetAttribute = False '' assume.
        Try
            Element_GetAttribute = xElem.GetAttribute(xAttrName)
        Catch ex As Exception : error_DebugPint("Element_GetAttribute", ex.Message, xAttrName)
        End Try
        ''
    End Function

    Public Function NodeReader_CreateObject(ByVal xdoc As Xml.XmlDocument, ByRef nreader As Xml.XmlNodeReader) As Boolean
        NodeReader_CreateObject = False
        Try
            nreader = Nothing '' assume.
            nreader = New Xml.XmlNodeReader(xdoc)
            NodeReader_CreateObject = Not nreader Is Nothing
        Catch ex As Exception : error_DebugPint("NodeReader_CreateObject", ex.Message, String.Empty)
        End Try
        ''
    End Function
    Public Function NodeReader_DisposeObject(ByRef nreader As Xml.XmlNodeReader) As Boolean
        NodeReader_DisposeObject = False
        Try
            nreader = Nothing
            NodeReader_DisposeObject = nreader Is Nothing
        Catch ex As Exception : error_DebugPint("NodeReader_DisposeObject", ex.Message, String.Empty)
        End Try
        ''
    End Function
    Public Function NodeReader_GetNodeNameWithPrefix(ByVal xdoc As Xml.XmlDocument, ByVal nodeName As String, ByRef nodeNameWithPrefix As String) As Boolean
        NodeReader_GetNodeNameWithPrefix = False
        nodeNameWithPrefix = String.Empty '' assume.
        Try
            'nodeName = add_TagPrefix(nodeName)
            Using nreader As New Xml.XmlNodeReader(xdoc)
                Do While nreader.Read
                    If _Controls.Contains(nreader.Name, nodeName, False) Then
                        nodeNameWithPrefix = nreader.Name
                        Exit Do
                    End If
                Loop
            End Using
            ''
            NodeReader_GetNodeNameWithPrefix = (Not 0 = nodeNameWithPrefix.Length)
            ''
        Catch ex As Exception : error_DebugPint("NodeReader_GetNodeNameWithPrefix", ex.Message, nodeName)
        End Try
    End Function
    Public Function NodeReader_GetValueByNodeName(ByVal xdoc As Xml.XmlDocument, ByVal nodeName As String, ByRef nodeText As String) As Boolean
        NodeReader_GetValueByNodeName = False
        nodeText = String.Empty '' assume.
        Try
            'nodeName = add_TagPrefix(nodeName)
            Using nreader As New Xml.XmlNodeReader(xdoc)
                Do While Not nreader.EOF
                    If nreader.ReadToFollowing(nodeName) Then
                        nodeText = nreader.ReadString
                        Exit Do
                    End If
                Loop
            End Using
            ''
            NodeReader_GetValueByNodeName = (Not 0 = nodeText.Length)
            ''
        Catch ex As Exception : error_DebugPint("1 NodeReader_GetValueByNodeName", ex.Message, nodeName)
        End Try
    End Function
    Public Function NodeReader_GetValueByNodeName(ByVal nreader As Xml.XmlNodeReader, ByVal nodeName As String, ByRef nodeText As String) As Boolean
        NodeReader_GetValueByNodeName = False
        nodeText = String.Empty '' assume.
        Try
            If Not nreader Is Nothing Then
                Do While Not nreader.EOF
                    If nreader.ReadToFollowing(nodeName) Then
                        nodeText = nreader.ReadString
                        Exit Do
                    End If
                Loop
            End If
            ''
            NodeReader_GetValueByNodeName = (Not 0 = nodeText.Length)
            ''
        Catch ex As Exception : error_DebugPint("2 NodeReader_GetValueByNodeName", ex.Message, nodeName)
        End Try
    End Function
    Public Function NodeReader_GetValuesByNodeName(ByVal xdoc As Xml.XmlDocument, ByVal nodeName As String, ByRef nodesText As String) As Boolean
        NodeReader_GetValuesByNodeName = False
        nodesText = String.Empty '' assume.
        Try
            'nodeName = add_TagPrefix(nodeName)
            Using nreader As New Xml.XmlNodeReader(xdoc)
                Do While Not nreader.EOF
                    If nreader.ReadToFollowing(nodeName) Then
                        If 0 = nodesText.Length Then
                            nodesText = nreader.ReadString
                        Else
                            nodesText = String.Format("{0}\r\n{1}", nodesText, nreader.ReadString)
                        End If
                        Exit Do
                    End If
                Loop
            End Using
            ''
            NodeReader_GetValuesByNodeName = (Not 0 = nodesText.Length)
            ''
        Catch ex As Exception : error_DebugPint("NodeReader_GetValuesByNodeName", ex.Message, nodeName)
        End Try
    End Function

    Public Function Send_WebRequest(ByVal stringrequest As String, ByVal webServerUri As String, ByRef responseFromServer As String) As Boolean
        Send_WebRequest = False
        ' Create a request using a URL that can receive a post. 
        Dim request As WebRequest = WebRequest.Create(webServerUri)
        ' Set the Method property of the request to POST.
        request.Method = "POST"
        ' Create POST data and convert it to a byte array.
        Dim byteArray As Byte() = Text.Encoding.UTF8.GetBytes(stringrequest)
        ' Set the ContentType property of the WebRequest.
        request.ContentType = "application/x-www-form-urlencoded"
        ' Set the ContentLength property of the WebRequest.
        request.ContentLength = byteArray.Length
        ' Get the request stream.
        Dim dataStream As Stream = request.GetRequestStream()
        ' Write the data to the request stream.
        dataStream.Write(byteArray, 0, byteArray.Length)
        ' Close the Stream object.
        dataStream.Close()
        ' Get the response.
        Dim response As WebResponse = request.GetResponse()
        ' Display the status.
        Console.WriteLine(CType(response, HttpWebResponse).StatusDescription)
        ' Get the stream containing content returned by the server.
        dataStream = response.GetResponseStream()
        ' Open the stream using a StreamReader for easy access.
        Dim reader As New StreamReader(dataStream)
        ' Read the content.
        responseFromServer = reader.ReadToEnd()
        ' Clean up the streams.
        reader.Close()
        dataStream.Close()
        response.Close()
    End Function

    Public Function Send_HttpWebRequest(ByVal xdoc As Xml.XmlDocument, ByVal webServerUri As String, ByRef response As String) As Boolean
        ''
        Dim wrequest As Net.HttpWebRequest = Nothing
        Dim wresponse As Net.WebResponse = Nothing
        Dim swriter As IO.StreamWriter = Nothing
        Dim sreader As IO.StreamReader = Nothing
        ''
        Send_HttpWebRequest = True '' assume.
        response = String.Empty '' assume.
        ''
        Try
            wrequest = Net.HttpWebRequest.Create(webServerUri)
            wrequest.Timeout = 10000
            wrequest.Method = "POST"
            wrequest.ContentType = "application/x-www-form-urlencoded"
            'wrequest.ContentLength = xdoc.InnerXml.Length '' sometimes errors: 'The request was aborted: The request was canceled.'
            wrequest.KeepAlive = False

            '' set the stream to the WebRequest's request stream
            swriter = New IO.StreamWriter(wrequest.GetRequestStream)
            swriter.Write(xdoc.InnerXml)
            swriter.Close()
            swriter.Dispose()

            '' pass the response from WebRequest to a WebResponse.
            wresponse = wrequest.GetResponse()
            sreader = New IO.StreamReader(wresponse.GetResponseStream())
            wrequest = Nothing
            wresponse = Nothing

            response = sreader.ReadToEnd
            sreader.Close()
            sreader.Dispose()
            ''
        Catch ex As Exception : error_DebugPint("Send_HttpWebRequest", ex.Message, webServerUri) : response = ex.Message : Send_HttpWebRequest = False
        End Try
        ''
    End Function
    Public Function Send_HttpWebRequest(ByVal webServerUri_WithStringRequest As String, ByRef response As String) As Boolean
        ''
        Dim wrequest As Net.HttpWebRequest = Nothing
        Dim wresponse As Net.WebResponse = Nothing
        Dim sreader As IO.StreamReader = Nothing
        ''
        Send_HttpWebRequest = True '' assume.
        response = String.Empty '' assume.
        ''
        System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12

        Try
            wrequest = Net.HttpWebRequest.Create(webServerUri_WithStringRequest)
            wrequest.Timeout = 10000
            wrequest.Method = "GET"

            '' pass the response from WebRequest to a WebResponse.
            wresponse = wrequest.GetResponse()
            sreader = New IO.StreamReader(wresponse.GetResponseStream())
            wrequest = Nothing
            wresponse = Nothing

            response = sreader.ReadToEnd
            sreader.Close()
            sreader.Dispose()
            ''
        Catch ex As Exception : error_DebugPint("Send_HttpWebRequest", ex.Message, webServerUri_WithStringRequest) : response = ex.Message : Send_HttpWebRequest = False
        End Try
        ''
    End Function

    Function Get_PublicIPAddress() As String
        Get_PublicIPAddress = String.Empty ' assume.
        Dim php As String = "http://support.shipritesoftware.com/SR_Utilities/getMyIP.php"
        Try
            Dim ip As New WebClient
            Return ip.DownloadString(php)
        Catch ex As Exception : error_DebugPint("Get_PublicIPAddress", ex.Message, php)
        End Try
    End Function

End Module
