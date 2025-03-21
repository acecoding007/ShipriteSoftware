Module SocketServices

    ' Error returned by Winsock API.
    Public Const SOCKET_ERROR = -1

    ' Level number for (get/set)sockopt() to apply to socket itself.
    Public Const SOL_SOCKET = 65535      ' Options for socket level.
    Public Const IPPROTO_TCP = 6         ' Protocol Public Constant for TCP.

    ' option flags per socket
    Public Const SO_DEBUG = &H1&         ' Turn on debugging info recording
    Public Const SO_ACCEPTCONN = &H2&    ' Socket has had listen() - READ-ONLY.
    Public Const SO_REUSEADDR = &H4&     ' Allow local address reuse.
    Public Const SO_KEEPALIVE = &H8&     ' Keep connections alive.
    Public Const SO_DONTROUTE = &H10&    ' Just use interface addresses.
    Public Const SO_BROADCAST = &H20&    ' Permit sending of broadcast msgs.
    Public Const SO_USELOOPBACK = &H40&  ' Bypass hardware when possible.
    Public Const SO_LINGER = &H80&       ' Linger on close if data present.
    Public Const SO_OOBINLINE = &H100&   ' Leave received OOB data in line.

    Public Const SO_DONTLINGER = Not SO_LINGER
    Public Const SO_EXCLUSIVEADDRUSE = Not SO_REUSEADDR ' Disallow local address reuse.

    ' Additional options.
    Public Const SO_SNDBUF = &H1001&     ' Send buffer size.
    Public Const SO_RCVBUF = &H1002&     ' Receive buffer size.
    Public Const SO_ERROR = &H1007&      ' Get error status and clear.
    Public Const SO_TYPE = &H1008&       ' Get socket type - READ-ONLY.

    ' TCP Options
    Public Const TCP_NODELAY = &H1&      ' Turn off Nagel Algorithm.

    ' linger structure
    Public Structure LINGER_STRUCT
        Dim l_onoff As Integer          ' Is linger on or off?
        Dim l_linger As Integer         ' Linger timeout in seconds.
    End Structure

    ' Winsock API declares
    Public Declare Function setsockopt Lib "wsock32.dll" (ByVal s As Int32, ByVal level As Int32, ByVal optname As Int32, ByRef optval As Object, ByVal optlen As Int32) As Int32
    Public Declare Function getsockopt Lib "wsock32.dll" (ByVal s As Int32, ByVal level As Int32, ByVal optname As Int32, optval As Object, optlen As Int32) As Int32


End Module
