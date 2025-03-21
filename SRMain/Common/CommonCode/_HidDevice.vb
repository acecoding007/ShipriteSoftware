Imports System.Runtime.InteropServices
Imports System.Threading
Imports System.Runtime.CompilerServices
Imports System.Text

Namespace HidLibrary
    Friend NotInheritable Class NativeMethods
        Friend Const FILE_FLAG_OVERLAPPED As Integer = &H40000000
        Friend Const FILE_SHARE_READ As Short = &H1
        Friend Const FILE_SHARE_WRITE As Short = &H2
        Friend Const GENERIC_READ As UInteger = &H80000000UI
        Friend Const GENERIC_WRITE As UInteger = &H40000000
        Friend Const ACCESS_NONE As Integer = 0
        Friend Const INVALID_HANDLE_VALUE As Integer = -1
        Friend Const OPEN_EXISTING As Short = 3
        Friend Const WAIT_TIMEOUT As Integer = &H102
        Friend Const WAIT_OBJECT_0 As UInteger = 0
        Friend Const WAIT_FAILED As UInteger = &HFFFFFFFFUI

        Friend Const WAIT_INFINITE As Integer = -1

        <StructLayout(Runtime.InteropServices.LayoutKind.Sequential)>
        Friend Structure OVERLAPPED
            Public Internal As Integer
            Public InternalHigh As Integer
            Public Offset As Integer
            Public OffsetHigh As Integer
            Public hEvent As Integer
        End Structure

        <StructLayout(Runtime.InteropServices.LayoutKind.Sequential)>
        Friend Structure SECURITY_ATTRIBUTES
            Public nLength As Integer
            Public lpSecurityDescriptor As IntPtr
            Public bInheritHandle As Boolean
        End Structure

        <DllImport("kernel32.dll", SetLastError:=True, ExactSpelling:=True, CharSet:=CharSet.Auto)>
        Friend Shared Function CancelIo(ByVal hFile As IntPtr) As Boolean
        End Function

        <DllImport("kernel32.dll", SetLastError:=True, ExactSpelling:=True, CharSet:=CharSet.Auto)>
        Friend Shared Function CancelIoEx(ByVal hFile As IntPtr, ByVal lpOverlapped As IntPtr) As Boolean
        End Function

        <DllImport("kernel32.dll", SetLastError:=True, ExactSpelling:=True, CharSet:=CharSet.Auto)>
        Friend Shared Function CloseHandle(ByVal hObject As IntPtr) As Boolean
        End Function

        <DllImport("kernel32.dll", SetLastError:=True, ExactSpelling:=True, CharSet:=CharSet.Auto)>
        Friend Shared Function CancelSynchronousIo(ByVal hObject As IntPtr) As Boolean
        End Function

        <DllImport("kernel32.dll", CharSet:=CharSet.Auto)>
        Friend Shared Function CreateEvent(ByRef securityAttributes As SECURITY_ATTRIBUTES, ByVal bManualReset As Integer, ByVal bInitialState As Integer, ByVal lpName As String) As IntPtr
        End Function

        <DllImport("kernel32.dll", CharSet:=CharSet.Auto, SetLastError:=True)>
        Friend Shared Function CreateFile(ByVal lpFileName As String, ByVal dwDesiredAccess As UInteger, ByVal dwShareMode As Integer, ByRef lpSecurityAttributes As SECURITY_ATTRIBUTES, ByVal dwCreationDisposition As Integer, ByVal dwFlagsAndAttributes As Integer, ByVal hTemplateFile As Integer) As IntPtr
        End Function

        <DllImport("kernel32.dll", SetLastError:=True)>
        Friend Shared Function ReadFile(ByVal hFile As IntPtr, ByVal lpBuffer As IntPtr, ByVal nNumberOfBytesToRead As UInteger, <Out> ByRef lpNumberOfBytesRead As UInteger, <[In]> ByRef lpOverlapped As System.Threading.NativeOverlapped) As Boolean
        End Function

        <DllImport("kernel32.dll")>
        Friend Shared Function WaitForSingleObject(ByVal hHandle As IntPtr, ByVal dwMilliseconds As Integer) As UInteger
        End Function

        <DllImport("kernel32.dll", SetLastError:=True)>
        Friend Shared Function GetOverlappedResult(ByVal hFile As IntPtr, <[In]> ByRef lpOverlapped As System.Threading.NativeOverlapped, <Out> ByRef lpNumberOfBytesTransferred As UInteger, ByVal bWait As Boolean) As Boolean
        End Function

        <DllImport("kernel32.dll")>
        Friend Shared Function WriteFile(ByVal hFile As IntPtr, ByVal lpBuffer As Byte(), ByVal nNumberOfBytesToWrite As UInteger, <Out> ByRef lpNumberOfBytesWritten As UInteger, <[In]> ByRef lpOverlapped As System.Threading.NativeOverlapped) As Boolean
        End Function

        Friend Const DBT_DEVICEARRIVAL As Integer = &H8000
        Friend Const DBT_DEVICEREMOVECOMPLETE As Integer = &H8004
        Friend Const DBT_DEVTYP_DEVICEINTERFACE As Integer = 5
        Friend Const DBT_DEVTYP_HANDLE As Integer = 6
        Friend Const DEVICE_NOTIFY_ALL_INTERFACE_CLASSES As Integer = 4
        Friend Const DEVICE_NOTIFY_SERVICE_HANDLE As Integer = 1
        Friend Const DEVICE_NOTIFY_WINDOW_HANDLE As Integer = 0
        Friend Const WM_DEVICECHANGE As Integer = &H219
        Friend Const DIGCF_PRESENT As Short = &H2
        Friend Const DIGCF_DEVICEINTERFACE As Short = &H10
        Friend Const DIGCF_ALLCLASSES As Integer = &H4
        Friend Const MAX_DEV_LEN As Integer = 1000
        Friend Const SPDRP_ADDRESS As Integer = &H1C
        Friend Const SPDRP_BUSNUMBER As Integer = &H15
        Friend Const SPDRP_BUSTYPEGUID As Integer = &H13
        Friend Const SPDRP_CAPABILITIES As Integer = &HF
        Friend Const SPDRP_CHARACTERISTICS As Integer = &H1B
        Friend Const SPDRP_CLASS As Integer = 7
        Friend Const SPDRP_CLASSGUID As Integer = 8
        Friend Const SPDRP_COMPATIBLEIDS As Integer = 2
        Friend Const SPDRP_CONFIGFLAGS As Integer = &HA
        Friend Const SPDRP_DEVICE_POWER_DATA As Integer = &H1E
        Friend Const SPDRP_DEVICEDESC As Integer = 0
        Friend Const SPDRP_DEVTYPE As Integer = &H19
        Friend Const SPDRP_DRIVER As Integer = 9
        Friend Const SPDRP_ENUMERATOR_NAME As Integer = &H16
        Friend Const SPDRP_EXCLUSIVE As Integer = &H1A
        Friend Const SPDRP_FRIENDLYNAME As Integer = &HC
        Friend Const SPDRP_HARDWAREID As Integer = 1
        Friend Const SPDRP_LEGACYBUSTYPE As Integer = &H14
        Friend Const SPDRP_LOCATION_INFORMATION As Integer = &HD
        Friend Const SPDRP_LOWERFILTERS As Integer = &H12
        Friend Const SPDRP_MFG As Integer = &HB
        Friend Const SPDRP_PHYSICAL_DEVICE_OBJECT_NAME As Integer = &HE
        Friend Const SPDRP_REMOVAL_POLICY As Integer = &H1F
        Friend Const SPDRP_REMOVAL_POLICY_HW_DEFAULT As Integer = &H20
        Friend Const SPDRP_REMOVAL_POLICY_OVERRIDE As Integer = &H21
        Friend Const SPDRP_SECURITY As Integer = &H17
        Friend Const SPDRP_SECURITY_SDS As Integer = &H18
        Friend Const SPDRP_SERVICE As Integer = 4
        Friend Const SPDRP_UI_NUMBER As Integer = &H10
        Friend Const SPDRP_UI_NUMBER_DESC_FORMAT As Integer = &H1D
        Friend Const SPDRP_UPPERFILTERS As Integer = &H11

        <StructLayout(LayoutKind.Sequential)>
        Friend Class DEV_BROADCAST_DEVICEINTERFACE
            Friend dbcc_size As Integer
            Friend dbcc_devicetype As Integer
            Friend dbcc_reserved As Integer
            Friend dbcc_classguid As Guid
            Friend dbcc_name As Short
        End Class

        <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)>
        Friend Class DEV_BROADCAST_DEVICEINTERFACE_1
            Friend dbcc_size As Integer
            Friend dbcc_devicetype As Integer
            Friend dbcc_reserved As Integer
            <MarshalAs(UnmanagedType.ByValArray, ArraySubType:=UnmanagedType.U1, SizeConst:=16)>
            Friend dbcc_classguid As Byte()
            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=255)>
            Friend dbcc_name As Char()
        End Class

        <StructLayout(LayoutKind.Sequential)>
        Friend Class DEV_BROADCAST_HANDLE
            Friend dbch_size As Integer
            Friend dbch_devicetype As Integer
            Friend dbch_reserved As Integer
            Friend dbch_handle As Integer
            Friend dbch_hdevnotify As Integer
        End Class

        <StructLayout(LayoutKind.Sequential)>
        Friend Class DEV_BROADCAST_HDR
            Friend dbch_size As Integer
            Friend dbch_devicetype As Integer
            Friend dbch_reserved As Integer
        End Class

        <StructLayout(LayoutKind.Sequential)>
        Friend Structure SP_DEVICE_INTERFACE_DATA
            Friend cbSize As Integer
            Friend InterfaceClassGuid As System.Guid
            Friend Flags As Integer
            Friend Reserved As IntPtr
        End Structure

        <StructLayout(LayoutKind.Sequential)>
        Friend Structure SP_DEVINFO_DATA
            Friend cbSize As Integer
            Friend ClassGuid As Guid
            Friend DevInst As Integer
            Friend Reserved As IntPtr
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Auto)>
        Friend Structure SP_DEVICE_INTERFACE_DETAIL_DATA
            Friend Size As Integer
            <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=256)>
            Friend DevicePath As String
        End Structure

        <StructLayout(LayoutKind.Sequential)>
        Friend Structure DEVPROPKEY
            Public fmtid As Guid
            Public pid As ULong
        End Structure

        Friend Shared DEVPKEY_Device_BusReportedDeviceDesc As DEVPROPKEY = New DEVPROPKEY With {
            .fmtid = New Guid(&H540B947E, &H8B40, &H45BC, &HA8, &HA2, &H6A, &HB, &H89, &H4C, &HBD, &HA2),
            .pid = 4
        }
        <DllImport("setupapi.dll", EntryPoint:="SetupDiGetDeviceRegistryProperty")>
        Public Shared Function SetupDiGetDeviceRegistryProperty(ByVal deviceInfoSet As IntPtr, ByRef deviceInfoData As SP_DEVINFO_DATA, ByVal propertyVal As Integer, ByRef propertyRegDataType As Integer, ByVal propertyBuffer As Byte(), ByVal propertyBufferSize As Integer, ByRef requiredSize As Integer) As Boolean
        End Function

        <DllImport("setupapi.dll", EntryPoint:="SetupDiGetDevicePropertyW", SetLastError:=True)>
        Public Shared Function SetupDiGetDeviceProperty(ByVal deviceInfo As IntPtr, ByRef deviceInfoData As SP_DEVINFO_DATA, ByRef propkey As DEVPROPKEY, ByRef propertyDataType As ULong, ByVal propertyBuffer As Byte(), ByVal propertyBufferSize As Integer, ByRef requiredSize As Integer, ByVal flags As UInteger) As Boolean
        End Function

        <DllImport("setupapi.dll")>
        Friend Shared Function SetupDiEnumDeviceInfo(ByVal deviceInfoSet As IntPtr, ByVal memberIndex As Integer, ByRef deviceInfoData As SP_DEVINFO_DATA) As Boolean
        End Function

        <DllImport("user32.dll", CharSet:=CharSet.Auto)>
        Friend Shared Function RegisterDeviceNotification(ByVal hRecipient As IntPtr, ByVal notificationFilter As IntPtr, ByVal flags As Int32) As IntPtr
        End Function

        <DllImport("setupapi.dll")>
        Friend Shared Function SetupDiCreateDeviceInfoList(ByRef classGuid As Guid, ByVal hwndParent As Integer) As Integer
        End Function

        <DllImport("setupapi.dll")>
        Friend Shared Function SetupDiDestroyDeviceInfoList(ByVal deviceInfoSet As IntPtr) As Integer
        End Function

        <DllImport("setupapi.dll")>
        Friend Shared Function SetupDiEnumDeviceInterfaces(ByVal deviceInfoSet As IntPtr, ByRef deviceInfoData As SP_DEVINFO_DATA, ByRef interfaceClassGuid As Guid, ByVal memberIndex As Integer, ByRef deviceInterfaceData As SP_DEVICE_INTERFACE_DATA) As Boolean
        End Function

        <DllImport("setupapi.dll", CharSet:=CharSet.Auto)>
        Friend Shared Function SetupDiGetClassDevs(ByRef classGuid As System.Guid, ByVal enumerator As String, ByVal hwndParent As Integer, ByVal flags As Integer) As IntPtr
        End Function

        <DllImport("setupapi.dll", CharSet:=CharSet.Auto, EntryPoint:="SetupDiGetDeviceInterfaceDetail")>
        Friend Shared Function SetupDiGetDeviceInterfaceDetailBuffer(ByVal deviceInfoSet As IntPtr, ByRef deviceInterfaceData As SP_DEVICE_INTERFACE_DATA, ByVal deviceInterfaceDetailData As IntPtr, ByVal deviceInterfaceDetailDataSize As Integer, ByRef requiredSize As Integer, ByVal deviceInfoData As IntPtr) As Boolean
        End Function

        <DllImport("setupapi.dll", CharSet:=CharSet.Auto)>
        Friend Shared Function SetupDiGetDeviceInterfaceDetail(ByVal deviceInfoSet As IntPtr, ByRef deviceInterfaceData As SP_DEVICE_INTERFACE_DATA, ByRef deviceInterfaceDetailData As SP_DEVICE_INTERFACE_DETAIL_DATA, ByVal deviceInterfaceDetailDataSize As Integer, ByRef requiredSize As Integer, ByVal deviceInfoData As IntPtr) As Boolean
        End Function

        <DllImport("user32.dll")>
        Friend Shared Function UnregisterDeviceNotification(ByVal handle As IntPtr) As Boolean
        End Function

        Friend Const HIDP_INPUT As Short = 0
        Friend Const HIDP_OUTPUT As Short = 1
        Friend Const HIDP_FEATURE As Short = 2

        <StructLayout(LayoutKind.Sequential)>
        Friend Structure HIDD_ATTRIBUTES
            Friend Size As Integer
            Friend VendorID As UShort
            Friend ProductID As UShort
            Friend VersionNumber As Short
        End Structure

        <StructLayout(LayoutKind.Sequential)>
        Friend Structure HIDP_CAPS
            Friend Usage As Short
            Friend UsagePage As Short
            Friend InputReportByteLength As Short
            Friend OutputReportByteLength As Short
            Friend FeatureReportByteLength As Short
            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=17)>
            Friend Reserved As Short()
            Friend NumberLinkCollectionNodes As Short
            Friend NumberInputButtonCaps As Short
            Friend NumberInputValueCaps As Short
            Friend NumberInputDataIndices As Short
            Friend NumberOutputButtonCaps As Short
            Friend NumberOutputValueCaps As Short
            Friend NumberOutputDataIndices As Short
            Friend NumberFeatureButtonCaps As Short
            Friend NumberFeatureValueCaps As Short
            Friend NumberFeatureDataIndices As Short
        End Structure

        <StructLayout(LayoutKind.Sequential)>
        Friend Structure HIDP_VALUE_CAPS
            Friend UsagePage As Short
            Friend ReportID As Byte
            Friend IsAlias As Integer
            Friend BitField As Short
            Friend LinkCollection As Short
            Friend LinkUsage As Short
            Friend LinkUsagePage As Short
            Friend IsRange As Integer
            Friend IsStringRange As Integer
            Friend IsDesignatorRange As Integer
            Friend IsAbsolute As Integer
            Friend HasNull As Integer
            Friend Reserved As Byte
            Friend BitSize As Short
            Friend ReportCount As Short
            Friend Reserved2 As Short
            Friend Reserved3 As Short
            Friend Reserved4 As Short
            Friend Reserved5 As Short
            Friend Reserved6 As Short
            Friend LogicalMin As Integer
            Friend LogicalMax As Integer
            Friend PhysicalMin As Integer
            Friend PhysicalMax As Integer
            Friend UsageMin As Short
            Friend UsageMax As Short
            Friend StringMin As Short
            Friend StringMax As Short
            Friend DesignatorMin As Short
            Friend DesignatorMax As Short
            Friend DataIndexMin As Short
            Friend DataIndexMax As Short
        End Structure

        <DllImport("hid.dll")>
        Friend Shared Function HidD_FlushQueue(ByVal hidDeviceObject As IntPtr) As Boolean
        End Function

        <DllImport("hid.dll")>
        Friend Shared Function HidD_GetAttributes(ByVal hidDeviceObject As IntPtr, ByRef attributes As HIDD_ATTRIBUTES) As Boolean
        End Function

        <DllImport("hid.dll")>
        Friend Shared Function HidD_GetFeature(ByVal hidDeviceObject As IntPtr, ByVal lpReportBuffer As Byte(), ByVal reportBufferLength As Integer) As Boolean
        End Function

        <DllImport("hid.dll")>
        Friend Shared Function HidD_GetInputReport(ByVal hidDeviceObject As IntPtr, ByVal lpReportBuffer As Byte(), ByVal reportBufferLength As Integer) As Boolean
        End Function

        <DllImport("hid.dll")>
        Friend Shared Sub HidD_GetHidGuid(ByRef hidGuid As Guid)
        End Sub

        <DllImport("hid.dll")>
        Friend Shared Function HidD_GetNumInputBuffers(ByVal hidDeviceObject As IntPtr, ByRef numberBuffers As Integer) As Boolean
        End Function

        <DllImport("hid.dll")>
        Friend Shared Function HidD_GetPreparsedData(ByVal hidDeviceObject As IntPtr, ByRef preparsedData As IntPtr) As Boolean
        End Function

        <DllImport("hid.dll")>
        Friend Shared Function HidD_FreePreparsedData(ByVal preparsedData As IntPtr) As Boolean
        End Function

        <DllImport("hid.dll")>
        Friend Shared Function HidD_SetFeature(ByVal hidDeviceObject As IntPtr, ByVal lpReportBuffer As Byte(), ByVal reportBufferLength As Integer) As Boolean
        End Function

        <DllImport("hid.dll")>
        Friend Shared Function HidD_SetNumInputBuffers(ByVal hidDeviceObject As IntPtr, ByVal numberBuffers As Integer) As Boolean
        End Function

        <DllImport("hid.dll")>
        Friend Shared Function HidD_SetOutputReport(ByVal hidDeviceObject As IntPtr, ByVal lpReportBuffer As Byte(), ByVal reportBufferLength As Integer) As Boolean
        End Function

        <DllImport("hid.dll")>
        Friend Shared Function HidP_GetCaps(ByVal preparsedData As IntPtr, ByRef capabilities As HIDP_CAPS) As Integer
        End Function

        <DllImport("hid.dll")>
        Friend Shared Function HidP_GetValueCaps(ByVal reportType As Short, ByRef valueCaps As Byte, ByRef valueCapsLength As Short, ByVal preparsedData As IntPtr) As Integer
        End Function

        <DllImport("hid.dll", CharSet:=CharSet.Unicode)>
        Friend Shared Function HidD_GetProductString(ByVal hidDeviceObject As IntPtr, ByRef lpReportBuffer As Byte, ByVal ReportBufferLength As Integer) As Boolean
        End Function

        <DllImport("hid.dll", CharSet:=CharSet.Unicode)>
        Friend Shared Function HidD_GetManufacturerString(ByVal hidDeviceObject As IntPtr, ByRef lpReportBuffer As Byte, ByVal ReportBufferLength As Integer) As Boolean
        End Function

        <DllImport("hid.dll", CharSet:=CharSet.Unicode)>
        Friend Shared Function HidD_GetSerialNumberString(ByVal hidDeviceObject As IntPtr, ByRef lpReportBuffer As Byte, ByVal reportBufferLength As Integer) As Boolean
        End Function

    End Class

    Public Interface IHidEnumerator
        Function IsConnected(ByVal devicePath As String) As Boolean
        Function GetDevice(ByVal devicePath As String) As IHidDevice
        Function Enumerate() As IEnumerable(Of IHidDevice)
        Function Enumerate(ByVal devicePath As String) As IEnumerable(Of IHidDevice)
        Function Enumerate(ByVal vendorId As Integer, ParamArray productIds As Integer()) As IEnumerable(Of IHidDevice)
        Function Enumerate(ByVal vendorId As Integer) As IEnumerable(Of IHidDevice)
    End Interface

    Public Class HidEnumerator
        Implements IHidEnumerator

        Public Function IsConnected(ByVal devicePath As String) As Boolean Implements IHidEnumerator.IsConnected
            Return HidDevices.IsConnected(devicePath)
        End Function

        Public Function GetDevice(ByVal devicePath As String) As IHidDevice Implements IHidEnumerator.GetDevice
            Return TryCast(HidDevices.GetDevice(devicePath), IHidDevice)
        End Function

        Public Function Enumerate() As IEnumerable(Of IHidDevice) Implements IHidEnumerator.Enumerate
            Return HidDevices.Enumerate().[Select](Function(d) TryCast(d, IHidDevice))
        End Function

        Public Function Enumerate(ByVal devicePath As String) As IEnumerable(Of IHidDevice) Implements IHidEnumerator.Enumerate
            Return HidDevices.Enumerate(devicePath).[Select](Function(d) TryCast(d, IHidDevice))
        End Function

        Public Function Enumerate(ByVal vendorId As Integer, ParamArray productIds As Integer()) As IEnumerable(Of IHidDevice) Implements IHidEnumerator.Enumerate
            Return HidDevices.Enumerate(vendorId, productIds).[Select](Function(d) TryCast(d, IHidDevice))
        End Function

        Public Function Enumerate(ByVal vendorId As Integer) As IEnumerable(Of IHidDevice) Implements IHidEnumerator.Enumerate
            Return HidDevices.Enumerate(vendorId).[Select](Function(d) TryCast(d, IHidDevice))
        End Function
    End Class

    Public Delegate Sub InsertedEventHandler()
    Public Delegate Sub RemovedEventHandler()

    Public Enum DeviceMode
        NonOverlapped = 0
        Overlapped = 1
    End Enum

    <Flags>
    Public Enum ShareMode
        Exclusive = 0
        ShareRead = NativeMethods.FILE_SHARE_READ
        ShareWrite = NativeMethods.FILE_SHARE_WRITE
    End Enum

    Public Delegate Sub ReadCallback(ByVal data As HidDeviceData)
    Public Delegate Sub ReadReportCallback(ByVal report As HidReport)
    Public Delegate Sub WriteCallback(ByVal success As Boolean)

    Public Interface IHidDevice
        Inherits IDisposable

        Event Inserted As InsertedEventHandler
        Event Removed As RemovedEventHandler
        ReadOnly Property Handle As IntPtr
        ReadOnly Property IsOpen As Boolean
        ReadOnly Property IsConnected As Boolean
        ReadOnly Property Description As String
        ReadOnly Property Capabilities As HidDeviceCapabilities
        ReadOnly Property Attributes As HidDeviceAttributes
        ReadOnly Property DevicePath As String
        Property MonitorDeviceEvents As Boolean
        Sub OpenDevice()
        Sub OpenDevice(ByVal readMode As DeviceMode, ByVal writeMode As DeviceMode, ByVal shareMode As ShareMode)
        Sub CloseDevice()
        Function Read() As HidDeviceData
        Sub Read(ByVal callback As ReadCallback)
        Sub Read(ByVal callback As ReadCallback, ByVal timeout As Integer)
        Function ReadAsync(ByVal Optional timeout As Integer = 0) As Task(Of HidDeviceData)
        Function Read(ByVal timeout As Integer) As HidDeviceData
        Sub ReadReport(ByVal callback As ReadReportCallback)
        Sub ReadReport(ByVal callback As ReadReportCallback, ByVal timeout As Integer)
        Function ReadReportAsync(ByVal Optional timeout As Integer = 0) As Task(Of HidReport)
        Function ReadReport(ByVal timeout As Integer) As HidReport
        Function ReadReport() As HidReport
        Function ReadFeatureData(<Out> ByRef data As Byte(), ByVal Optional reportId As Byte = 0) As Boolean
        Function ReadProduct(<Out> ByRef data As Byte()) As Boolean
        Function ReadManufacturer(<Out> ByRef data As Byte()) As Boolean
        Function ReadSerialNumber(<Out> ByRef data As Byte()) As Boolean
        Sub Write(ByVal data As Byte(), ByVal callback As WriteCallback)
        Function Write(ByVal data As Byte()) As Boolean
        Function Write(ByVal data As Byte(), ByVal timeout As Integer) As Boolean
        Sub Write(ByVal data As Byte(), ByVal callback As WriteCallback, ByVal timeout As Integer)
        Function WriteAsync(ByVal data As Byte(), ByVal Optional timeout As Integer = 0) As Task(Of Boolean)
        Sub WriteReport(ByVal report As HidReport, ByVal callback As WriteCallback)
        Function WriteReport(ByVal report As HidReport) As Boolean
        Function WriteReport(ByVal report As HidReport, ByVal timeout As Integer) As Boolean
        Sub WriteReport(ByVal report As HidReport, ByVal callback As WriteCallback, ByVal timeout As Integer)
        Function WriteReportAsync(ByVal report As HidReport, ByVal Optional timeout As Integer = 0) As Task(Of Boolean)
        Function CreateReport() As HidReport
        Function WriteFeatureData(ByVal data As Byte()) As Boolean
    End Interface

    Public Class HidReport
        Private _reportId As Byte
        Private _data As Byte() = New Byte() {}
        Private ReadOnly _status As HidDeviceData.ReadStatus

        Public Sub New(ByVal reportSize As Integer)
            Array.Resize(_data, reportSize - 1) ' Array.Resize(_data, reportSize - 1)
        End Sub

        Public Sub New(ByVal reportSize As Integer, ByVal deviceData As HidDeviceData)
            _status = deviceData.Status
            Array.Resize(_data, reportSize - 1) ' Array.Resize(_data, reportSize - 1)

            If (deviceData.Data IsNot Nothing) Then

                If deviceData.Data.Length > 0 Then
                    _reportId = deviceData.Data(0)
                    Exists = True

                    If deviceData.Data.Length > 1 Then ' If deviceData.Data.Length > 1 Then
                        Dim dataLength = reportSize - 1 ' Dim dataLength = reportSize - 1
                        If deviceData.Data.Length < reportSize - 1 Then dataLength = deviceData.Data.Length ' If deviceData.Data.Length < reportSize - 1 Then dataLength = deviceData.Data.Length
                        Array.Copy(deviceData.Data, 1, _data, 0, dataLength)
                    End If
                Else
                    Exists = False
                End If
            Else
                Exists = False
            End If
        End Sub

        Public Property Exists As Boolean

        Public ReadOnly Property ReadStatus As HidDeviceData.ReadStatus
            Get
                Return _status
            End Get
        End Property

        Public Property ReportId As Byte
            Get
                Return _reportId
            End Get
            Set(ByVal value As Byte)
                _reportId = value
                Exists = True
            End Set
        End Property

        Public Property Data As Byte()
            Get
                Return _data
            End Get
            Set(ByVal value As Byte())
                _data = value
                Exists = True
            End Set
        End Property

        Public Function GetBytes() As Byte()
            Dim data As Byte() = Nothing
            Array.Resize(data, _data.Length + 1)
            data(0) = _reportId
            Array.Copy(_data, 0, data, 1, _data.Length)
            Return data
        End Function
    End Class

    Public Class HidFastReadEnumerator
        Implements IHidEnumerator

        Public Function IsConnected(ByVal devicePath As String) As Boolean Implements IHidEnumerator.IsConnected
            Return HidDevices.IsConnected(devicePath)
        End Function

        Public Function GetDevice(ByVal devicePath As String) As IHidDevice Implements IHidEnumerator.GetDevice
            Return TryCast(Enumerate(devicePath).FirstOrDefault(), IHidDevice)
        End Function

        Public Function Enumerate() As IEnumerable(Of IHidDevice) Implements IHidEnumerator.Enumerate
            Return HidDevices.EnumerateDevices().[Select](Function(d) TryCast(New HidFastReadDevice(d.Path, d.Description), IHidDevice))
        End Function

        Public Function Enumerate(ByVal devicePath As String) As IEnumerable(Of IHidDevice) Implements IHidEnumerator.Enumerate
            Return HidDevices.EnumerateDevices().Where(Function(x) x.Path = devicePath).[Select](Function(d) TryCast(New HidFastReadDevice(d.Path, d.Description), IHidDevice))
        End Function

        Public Function Enumerate(ByVal vendorId As Integer, ParamArray productIds As Integer()) As IEnumerable(Of IHidDevice) Implements IHidEnumerator.Enumerate
            Return HidDevices.EnumerateDevices().[Select](Function(d) New HidFastReadDevice(d.Path, d.Description)).Where(Function(f) f.Attributes.VendorId = vendorId AndAlso productIds.Contains(f.Attributes.ProductId)).[Select](Function(d) TryCast(d, IHidDevice))
        End Function

        Public Function Enumerate(ByVal vendorId As Integer) As IEnumerable(Of IHidDevice) Implements IHidEnumerator.Enumerate
            Return HidDevices.EnumerateDevices().[Select](Function(d) New HidFastReadDevice(d.Path, d.Description)).Where(Function(f) f.Attributes.VendorId = vendorId).[Select](Function(d) TryCast(d, IHidDevice))
        End Function
    End Class

    Public Class HidFastReadDevice
        Inherits HidDevice

        Friend Sub New(ByVal devicePath As String, ByVal Optional description As String = Nothing)
            MyBase.New(devicePath, description)
        End Sub

        Public Function FastRead() As HidDeviceData
            Return FastRead(0)
        End Function

        Public Function FastRead(ByVal timeout As Integer) As HidDeviceData
            Try
                Return ReadData(timeout)
            Catch
                Return New HidDeviceData(HidDeviceData.ReadStatus.ReadError)
            End Try
        End Function

        Public Sub FastRead(ByVal callback As ReadCallback)
            FastRead(callback, 0)
        End Sub

        Public Sub FastRead(ByVal callback As ReadCallback, ByVal timeout As Integer)
            Dim readDelegate = New ReadDelegate(AddressOf FastRead)
            Dim asyncState = New HidAsyncState(readDelegate, callback)
            readDelegate.BeginInvoke(timeout, New AsyncCallback(AddressOf EndRead), asyncState)
        End Sub

        Public Async Function FastReadAsync(ByVal Optional timeout As Integer = 0) As Task(Of HidDeviceData)
            Dim readDelegate = New ReadDelegate(AddressOf FastRead)
            Return Await Task(Of HidDeviceData).Factory.FromAsync(AddressOf readDelegate.BeginInvoke, AddressOf readDelegate.EndInvoke, timeout, Nothing)
        End Function

        Public Function FastReadReport() As HidReport
            Return FastReadReport(0)
        End Function

        Public Function FastReadReport(ByVal timeout As Integer) As HidReport
            Return New HidReport(Capabilities.InputReportByteLength, FastRead(timeout))
        End Function

        Public Sub FastReadReport(ByVal callback As ReadReportCallback)
            FastReadReport(callback, 0)
        End Sub

        Public Sub FastReadReport(ByVal callback As ReadReportCallback, ByVal timeout As Integer)
            Dim readReportDelegate = New ReadReportDelegate(AddressOf FastReadReport)
            Dim asyncState = New HidAsyncState(readReportDelegate, callback)
            readReportDelegate.BeginInvoke(timeout, New AsyncCallback(AddressOf EndReadReport), asyncState)
        End Sub

        Public Async Function FastReadReportAsync(ByVal Optional timeout As Integer = 0) As Task(Of HidReport)
            Dim readReportDelegate = New ReadReportDelegate(AddressOf FastReadReport)
            Return Await Task(Of HidReport).Factory.FromAsync(AddressOf readReportDelegate.BeginInvoke, AddressOf readReportDelegate.EndInvoke, timeout, Nothing)
        End Function
    End Class

    Public Class HidDevices
        Private Shared _hidClassGuid As Guid = Guid.Empty

        Public Shared Function IsConnected(ByVal devicePath As String) As Boolean
            Return EnumerateDevices().Any(Function(x) x.Path = devicePath)
        End Function

        Public Shared Function GetDevice(ByVal devicePath As String) As HidDevice
            Return Enumerate(devicePath).FirstOrDefault()
        End Function

        Public Shared Function Enumerate() As IEnumerable(Of HidDevice)
            Return EnumerateDevices().[Select](Function(x) New HidDevice(x.Path, x.Description))
        End Function

        Public Shared Function Enumerate(ByVal devicePath As String) As IEnumerable(Of HidDevice)
            Return EnumerateDevices().Where(Function(x) x.Path = devicePath).[Select](Function(x) New HidDevice(x.Path, x.Description))
        End Function

        Public Shared Function Enumerate(ByVal vendorId As Integer, ParamArray productIds As Integer()) As IEnumerable(Of HidDevice)
            Return EnumerateDevices().[Select](Function(x) New HidDevice(x.Path, x.Description)).Where(Function(x) x.Attributes.VendorId = vendorId AndAlso productIds.Contains(x.Attributes.ProductId))
        End Function

        Public Shared Function Enumerate(ByVal vendorId As Integer, ByVal productId As Integer, ByVal UsagePage As UShort) As IEnumerable(Of HidDevice)
            Return EnumerateDevices().[Select](Function(x) New HidDevice(x.Path, x.Description)).Where(Function(x) x.Attributes.VendorId = vendorId AndAlso productId = CUShort(x.Attributes.ProductId) AndAlso CUShort(x.Capabilities.UsagePage) = UsagePage)
        End Function

        Public Shared Function Enumerate(ByVal vendorId As Integer) As IEnumerable(Of HidDevice)
            Return EnumerateDevices().[Select](Function(x) New HidDevice(x.Path, x.Description)).Where(Function(x) x.Attributes.VendorId = vendorId)
        End Function

        Friend Class DeviceInfo
            Public Property Path As String
            Public Property Description As String
        End Class

        Friend Shared Function EnumerateDevices() As IEnumerable(Of DeviceInfo)
            Dim devices = New List(Of DeviceInfo)()
            Dim hidClass = HidClassGuid
            Dim deviceInfoSet = NativeMethods.SetupDiGetClassDevs(hidClass, Nothing, 0, NativeMethods.DIGCF_PRESENT Or NativeMethods.DIGCF_DEVICEINTERFACE)

            If deviceInfoSet.ToInt64() <> NativeMethods.INVALID_HANDLE_VALUE Then
                Dim deviceInfoData = CreateDeviceInfoData()
                Dim deviceIndex = 0

                While NativeMethods.SetupDiEnumDeviceInfo(deviceInfoSet, deviceIndex, deviceInfoData)
                    deviceIndex += 1
                    Dim deviceInterfaceData = New NativeMethods.SP_DEVICE_INTERFACE_DATA()
                    deviceInterfaceData.cbSize = Marshal.SizeOf(deviceInterfaceData)
                    Dim deviceInterfaceIndex = 0

                    While NativeMethods.SetupDiEnumDeviceInterfaces(deviceInfoSet, deviceInfoData, hidClass, deviceInterfaceIndex, deviceInterfaceData)
                        deviceInterfaceIndex += 1
                        Dim devicePath = GetDevicePath(deviceInfoSet, deviceInterfaceData)
                        Dim description = If(GetBusReportedDeviceDescription(deviceInfoSet, deviceInfoData), GetDeviceDescription(deviceInfoSet, deviceInfoData))
                        devices.Add(New DeviceInfo With {
                            .Path = devicePath,
                            .Description = description
                        })
                    End While
                End While

                NativeMethods.SetupDiDestroyDeviceInfoList(deviceInfoSet)
            End If

            Return devices
        End Function

        Private Shared Function CreateDeviceInfoData() As NativeMethods.SP_DEVINFO_DATA
            Dim deviceInfoData = New NativeMethods.SP_DEVINFO_DATA()
            deviceInfoData.cbSize = Marshal.SizeOf(deviceInfoData)
            deviceInfoData.DevInst = 0
            deviceInfoData.ClassGuid = Guid.Empty
            deviceInfoData.Reserved = IntPtr.Zero
            Return deviceInfoData
        End Function

        Private Shared Function GetDevicePath(ByVal deviceInfoSet As IntPtr, ByVal deviceInterfaceData As NativeMethods.SP_DEVICE_INTERFACE_DATA) As String
            Dim bufferSize = 0
            Dim interfaceDetail = New NativeMethods.SP_DEVICE_INTERFACE_DETAIL_DATA With {
                .Size = If(IntPtr.Size = 4, 4 + Marshal.SystemDefaultCharSize, 8)
            }
            NativeMethods.SetupDiGetDeviceInterfaceDetailBuffer(deviceInfoSet, deviceInterfaceData, IntPtr.Zero, 0, bufferSize, IntPtr.Zero)
            Return If(NativeMethods.SetupDiGetDeviceInterfaceDetail(deviceInfoSet, deviceInterfaceData, interfaceDetail, bufferSize, bufferSize, IntPtr.Zero), interfaceDetail.DevicePath, Nothing)
        End Function

        Private Shared ReadOnly Property HidClassGuid As Guid
            Get
                If _hidClassGuid.Equals(Guid.Empty) Then NativeMethods.HidD_GetHidGuid(_hidClassGuid)
                Return _hidClassGuid
            End Get
        End Property

        Private Shared Function GetDeviceDescription(ByVal deviceInfoSet As IntPtr, ByRef devinfoData As NativeMethods.SP_DEVINFO_DATA) As String
            Dim descriptionBuffer = New Byte(1023) {}
            Dim requiredSize = 0
            Dim type = 0
            NativeMethods.SetupDiGetDeviceRegistryProperty(deviceInfoSet, devinfoData, NativeMethods.SPDRP_DEVICEDESC, type, descriptionBuffer, descriptionBuffer.Length, requiredSize)
            Return descriptionBuffer.ToUTF8String()
        End Function

        Private Shared Function GetBusReportedDeviceDescription(ByVal deviceInfoSet As IntPtr, ByRef devinfoData As NativeMethods.SP_DEVINFO_DATA) As String
            Dim descriptionBuffer = New Byte(1023) {}

            If Environment.OSVersion.Version.Major > 5 Then
                Dim propertyType As ULong = 0
                Dim requiredSize = 0
                Dim _continue = NativeMethods.SetupDiGetDeviceProperty(deviceInfoSet, devinfoData, NativeMethods.DEVPKEY_Device_BusReportedDeviceDesc, propertyType, descriptionBuffer, descriptionBuffer.Length, requiredSize, 0)
                If _continue Then Return descriptionBuffer.ToUTF16String()
            End If

            Return Nothing
        End Function
    End Class

    Friend Class HidDeviceEventMonitor
        Public Event Inserted As InsertedEventHandler
        Public Event Removed As RemovedEventHandler
        Public Delegate Sub InsertedEventHandler()
        Public Delegate Sub RemovedEventHandler()
        Private ReadOnly _device As HidDevice
        Private _wasConnected As Boolean

        Public Sub New(ByVal device As HidDevice)
            _device = device
        End Sub

        Public Sub Init()
            Dim eventMonitor = New Action(AddressOf DeviceEventMonitor)
            eventMonitor.BeginInvoke(AddressOf DisposeDeviceEventMonitor, eventMonitor)
        End Sub

        Private Sub DeviceEventMonitor()
            Dim isConnected = _device.IsConnected

            If isConnected <> _wasConnected Then

                If isConnected AndAlso InsertedEvent IsNot Nothing Then
                    RaiseEvent Inserted()
                ElseIf Not isConnected AndAlso Removedevent IsNot Nothing Then
                    RaiseEvent Removed()
                End If

                _wasConnected = isConnected
            End If

            Thread.Sleep(500)
            If _device.MonitorDeviceEvents Then Init()
        End Sub

        Private Shared Sub DisposeDeviceEventMonitor(ByVal ar As IAsyncResult)
            CType(ar.AsyncState, Action).EndInvoke(ar)
        End Sub
    End Class

    Public Class HidDeviceData
        Public Enum ReadStatus
            Success = 0
            WaitTimedOut = 1
            WaitFail = 2
            NoDataRead = 3
            ReadError = 4
            NotConnected = 5
        End Enum

        Public Sub New(ByVal status As ReadStatus)
            Me.Data = New Byte() {}
            Me.Status = status
        End Sub

        Public Sub New(ByVal data As Byte(), ByVal status As ReadStatus)
            Me.Data = data
            Me.Status = status
        End Sub

        Public Property Data As Byte()
        Public Property Status As ReadStatus
    End Class

    Public Class HidDeviceCapabilities
        Friend Sub New(ByVal capabilities As NativeMethods.HIDP_CAPS)
            Usage = capabilities.Usage
            UsagePage = capabilities.UsagePage
            InputReportByteLength = capabilities.InputReportByteLength
            OutputReportByteLength = capabilities.OutputReportByteLength
            FeatureReportByteLength = capabilities.FeatureReportByteLength
            Reserved = capabilities.Reserved
            NumberLinkCollectionNodes = capabilities.NumberLinkCollectionNodes
            NumberInputButtonCaps = capabilities.NumberInputButtonCaps
            NumberInputValueCaps = capabilities.NumberInputValueCaps
            NumberInputDataIndices = capabilities.NumberInputDataIndices
            NumberOutputButtonCaps = capabilities.NumberOutputButtonCaps
            NumberOutputValueCaps = capabilities.NumberOutputValueCaps
            NumberOutputDataIndices = capabilities.NumberOutputDataIndices
            NumberFeatureButtonCaps = capabilities.NumberFeatureButtonCaps
            NumberFeatureValueCaps = capabilities.NumberFeatureValueCaps
            NumberFeatureDataIndices = capabilities.NumberFeatureDataIndices
        End Sub

        Public Property Usage As Short
        Public Property UsagePage As Short
        Public Property InputReportByteLength As Short
        Public Property OutputReportByteLength As Short
        Public Property FeatureReportByteLength As Short
        Public Property Reserved As Short()
        Public Property NumberLinkCollectionNodes As Short
        Public Property NumberInputButtonCaps As Short
        Public Property NumberInputValueCaps As Short
        Public Property NumberInputDataIndices As Short
        Public Property NumberOutputButtonCaps As Short
        Public Property NumberOutputValueCaps As Short
        Public Property NumberOutputDataIndices As Short
        Public Property NumberFeatureButtonCaps As Short
        Public Property NumberFeatureValueCaps As Short
        Public Property NumberFeatureDataIndices As Short
    End Class

    Public Class HidDeviceAttributes
        Friend Sub New(ByVal attributes As NativeMethods.HIDD_ATTRIBUTES)
            VendorId = attributes.VendorID
            ProductId = attributes.ProductID
            Version = attributes.VersionNumber
            VendorHexId = "0x" & attributes.VendorID.ToString("X4")
            ProductHexId = "0x" & attributes.ProductID.ToString("X4")
        End Sub

        Public Property VendorId As Integer
        Public Property ProductId As Integer
        Public Property Version As Integer
        Public Property VendorHexId As String
        Public Property ProductHexId As String
    End Class

    Public Class HidDevice
        Implements IHidDevice

        Public Event Inserted As InsertedEventHandler Implements IHidDevice.Inserted
        Public Event Removed As RemovedEventHandler Implements IHidDevice.Removed
        Private ReadOnly _description As String
        Private ReadOnly _devicePath As String
        Private ReadOnly _deviceAttributes As HidDeviceAttributes
        Private ReadOnly _deviceCapabilities As HidDeviceCapabilities
        Private _deviceReadMode As DeviceMode = DeviceMode.NonOverlapped
        Private _deviceWriteMode As DeviceMode = DeviceMode.NonOverlapped
        Private _deviceShareMode As ShareMode = ShareMode.ShareRead Or ShareMode.ShareWrite
        Private ReadOnly _deviceEventMonitor As HidDeviceEventMonitor
        Private _monitorDeviceEvents As Boolean
        Protected Delegate Function ReadDelegate(ByVal timeout As Integer) As HidDeviceData
        Protected Delegate Function ReadReportDelegate(ByVal timeout As Integer) As HidReport
        Private Delegate Function WriteDelegate(ByVal data As Byte(), ByVal timeout As Integer) As Boolean
        Private Delegate Function WriteReportDelegate(ByVal report As HidReport, ByVal timeout As Integer) As Boolean

        Friend Sub New(ByVal devicePath As String, ByVal Optional description As String = Nothing)
            _deviceEventMonitor = New HidDeviceEventMonitor(Me)
            AddHandler _deviceEventMonitor.Inserted, AddressOf DeviceEventMonitorInserted
            AddHandler _deviceEventMonitor.Removed, AddressOf DeviceEventMonitorRemoved
            _devicePath = devicePath
            _description = description

            Try
                Dim hidHandle = OpenDeviceIO(_devicePath, NativeMethods.ACCESS_NONE)
                _deviceAttributes = GetDeviceAttributes(hidHandle)
                _deviceCapabilities = GetDeviceCapabilities(hidHandle)
                CloseDeviceIO(hidHandle)
            Catch exception As Exception
                Throw New Exception(String.Format("Error querying HID device '{0}'.", devicePath), exception)
            End Try
        End Sub

        Public Property Handle As IntPtr Implements IHidDevice.Handle
        Public Property IsOpen As Boolean Implements IHidDevice.IsOpen

        Public ReadOnly Property IsConnected As Boolean Implements IHidDevice.IsConnected
            Get
                Return HidDevices.IsConnected(_devicePath)
            End Get
        End Property

        Public ReadOnly Property Description As String Implements IHidDevice.Description
            Get
                Return _description
            End Get
        End Property

        Public ReadOnly Property Capabilities As HidDeviceCapabilities Implements IHidDevice.Capabilities
            Get
                Return _deviceCapabilities
            End Get
        End Property

        Public ReadOnly Property Attributes As HidDeviceAttributes Implements IHidDevice.Attributes
            Get
                Return _deviceAttributes
            End Get
        End Property

        Public ReadOnly Property DevicePath As String Implements IHidDevice.DevicePath
            Get
                Return _devicePath
            End Get
        End Property

        Public Property MonitorDeviceEvents As Boolean Implements IHidDevice.MonitorDeviceEvents
            Get
                Return _monitorDeviceEvents
            End Get
            Set(ByVal value As Boolean)
                If value And _monitorDeviceEvents = False Then _deviceEventMonitor.Init()
                _monitorDeviceEvents = value
            End Set
        End Property

        Public Overrides Function ToString() As String
            Return String.Format("VendorID={0}, ProductID={1}, Version={2}, DevicePath={3}", _deviceAttributes.VendorHexId, _deviceAttributes.ProductHexId, _deviceAttributes.Version, _devicePath)
        End Function

        Public Sub OpenDevice() Implements IHidDevice.OpenDevice
            OpenDevice(DeviceMode.NonOverlapped, DeviceMode.NonOverlapped, ShareMode.ShareRead Or ShareMode.ShareWrite)
        End Sub

        Public Sub OpenDevice(ByVal readMode As DeviceMode, ByVal writeMode As DeviceMode, ByVal shareMode As ShareMode) Implements IHidDevice.OpenDevice
            If IsOpen Then Return
            _deviceReadMode = readMode
            _deviceWriteMode = writeMode
            _deviceShareMode = shareMode

            Try
                Handle = OpenDeviceIO(_devicePath, readMode, NativeMethods.GENERIC_READ Or NativeMethods.GENERIC_WRITE, shareMode)
            Catch exception As Exception
                IsOpen = False
                Throw New Exception("Error opening HID device.", exception)
            End Try

            IsOpen = Handle.ToInt32() <> NativeMethods.INVALID_HANDLE_VALUE
        End Sub

        Public Sub CloseDevice() Implements IHidDevice.CloseDevice
            If Not IsOpen Then Return
            CloseDeviceIO(Handle)
            IsOpen = False
        End Sub

        Public Function Read() As HidDeviceData Implements IHidDevice.Read
            Return Read(0)
        End Function

        Public Function Read(ByVal timeout As Integer) As HidDeviceData Implements IHidDevice.Read
            If IsConnected Then
                If IsOpen = False Then OpenDevice(_deviceReadMode, _deviceWriteMode, _deviceShareMode)

                Try
                    Return ReadData(timeout)
                Catch
                    Return New HidDeviceData(HidDeviceData.ReadStatus.ReadError)
                End Try
            End If

            Return New HidDeviceData(HidDeviceData.ReadStatus.NotConnected)
        End Function

        Public Sub Read(ByVal callback As ReadCallback) Implements IHidDevice.Read
            Read(callback, 0)
        End Sub

        Public Sub Read(ByVal callback As ReadCallback, ByVal timeout As Integer) Implements IHidDevice.Read
            Dim readDelegate = New ReadDelegate(AddressOf Read)
            Dim asyncState = New HidAsyncState(readDelegate, callback)
            readDelegate.BeginInvoke(timeout, New System.AsyncCallback(AddressOf EndRead), asyncState)
        End Sub

        Public Async Function ReadAsync(ByVal Optional timeout As Integer = 0) As Task(Of HidDeviceData) Implements IHidDevice.ReadAsync
            Dim readDelegate = New ReadDelegate(AddressOf Read)
            Return Await Task(Of HidDeviceData).Factory.FromAsync(AddressOf readDelegate.BeginInvoke, AddressOf readDelegate.EndInvoke, timeout, Nothing)
        End Function

        Public Function ReadReport() As HidReport Implements IHidDevice.ReadReport
            Return ReadReport(0)
        End Function

        Public Function ReadReport(ByVal timeout As Integer) As HidReport Implements IHidDevice.ReadReport
            Return New HidReport(Capabilities.InputReportByteLength, Read(timeout))
        End Function

        Public Sub ReadReport(ByVal callback As ReadReportCallback) Implements IHidDevice.ReadReport
            ReadReport(callback, 0)
        End Sub

        Public Sub ReadReport(ByVal callback As ReadReportCallback, ByVal timeout As Integer) Implements IHidDevice.ReadReport
            Dim readReportDelegate = New ReadReportDelegate(AddressOf ReadReport)
            Dim asyncState = New HidAsyncState(readReportDelegate, callback)
            readReportDelegate.BeginInvoke(timeout, New AsyncCallback(AddressOf EndReadReport), asyncState)
        End Sub

        Public Async Function ReadReportAsync(ByVal Optional timeout As Integer = 0) As Task(Of HidReport) Implements IHidDevice.ReadReportAsync
            Dim readReportDelegate = New ReadReportDelegate(AddressOf ReadReport)
            Return Await Task(Of HidReport).Factory.FromAsync(AddressOf readReportDelegate.BeginInvoke, AddressOf readReportDelegate.EndInvoke, timeout, Nothing)
        End Function

        Public Function ReadReportSync(ByVal reportId As Byte) As HidReport
            Dim cmdBuffer As Byte() = New Byte(Capabilities.InputReportByteLength - 1) {}
            cmdBuffer(0) = reportId
            Dim bSuccess As Boolean = NativeMethods.HidD_GetInputReport(Handle, cmdBuffer, cmdBuffer.Length)
            Dim deviceData As HidDeviceData = New HidDeviceData(cmdBuffer, If(bSuccess, HidDeviceData.ReadStatus.Success, HidDeviceData.ReadStatus.NoDataRead))
            Return New HidReport(Capabilities.InputReportByteLength, deviceData)
        End Function

        Public Function ReadFeatureData(<Out> ByRef data As Byte(), ByVal Optional reportId As Byte = 0) As Boolean Implements IHidDevice.ReadFeatureData
            If _deviceCapabilities.FeatureReportByteLength <= 0 Then
                data = New Byte(-1) {}
                Return False
            End If

            data = New Byte(_deviceCapabilities.FeatureReportByteLength - 1) {}
            Dim buffer = CreateFeatureOutputBuffer()
            buffer(0) = reportId
            Dim hidHandle As IntPtr = IntPtr.Zero
            Dim success As Boolean = False

            Try

                If IsOpen Then
                    hidHandle = Handle
                Else
                    hidHandle = OpenDeviceIO(_devicePath, NativeMethods.ACCESS_NONE)
                End If

                success = NativeMethods.HidD_GetFeature(hidHandle, buffer, buffer.Length)

                If success Then
                    Array.Copy(buffer, 0, data, 0, Math.Min(data.Length, _deviceCapabilities.FeatureReportByteLength))
                End If

            Catch exception As Exception
                Throw New Exception(String.Format("Error accessing HID device '{0}'.", _devicePath), exception)
            Finally
                If hidHandle <> IntPtr.Zero AndAlso hidHandle <> Handle Then CloseDeviceIO(hidHandle)
            End Try

            Return success
        End Function

        Public Function ReadProduct(<Out> ByRef data As Byte()) As Boolean Implements IHidDevice.ReadProduct
            data = New Byte(253) {}
            Dim hidHandle As IntPtr = IntPtr.Zero
            Dim success As Boolean = False

            Try

                If IsOpen Then
                    hidHandle = Handle
                Else
                    hidHandle = OpenDeviceIO(_devicePath, NativeMethods.ACCESS_NONE)
                End If

                success = NativeMethods.HidD_GetProductString(hidHandle, data(0), data.Length)
            Catch exception As Exception
                Throw New Exception(String.Format("Error accessing HID device '{0}'.", _devicePath), exception)
            Finally
                If hidHandle <> IntPtr.Zero AndAlso hidHandle <> Handle Then CloseDeviceIO(hidHandle)
            End Try

            Return success
        End Function

        Public Function ReadManufacturer(<Out> ByRef data As Byte()) As Boolean Implements IHidDevice.ReadManufacturer
            data = New Byte(253) {}
            Dim hidHandle As IntPtr = IntPtr.Zero
            Dim success As Boolean = False

            Try

                If IsOpen Then
                    hidHandle = Handle
                Else
                    hidHandle = OpenDeviceIO(_devicePath, NativeMethods.ACCESS_NONE)
                End If

                success = NativeMethods.HidD_GetManufacturerString(hidHandle, data(0), data.Length)
            Catch exception As Exception
                Throw New Exception(String.Format("Error accessing HID device '{0}'.", _devicePath), exception)
            Finally
                If hidHandle <> IntPtr.Zero AndAlso hidHandle <> Handle Then CloseDeviceIO(hidHandle)
            End Try

            Return success
        End Function

        Public Function ReadSerialNumber(<Out> ByRef data As Byte()) As Boolean Implements IHidDevice.ReadSerialNumber
            data = New Byte(253) {}
            Dim hidHandle As IntPtr = IntPtr.Zero
            Dim success As Boolean = False

            Try

                If IsOpen Then
                    hidHandle = Handle
                Else
                    hidHandle = OpenDeviceIO(_devicePath, NativeMethods.ACCESS_NONE)
                End If

                success = NativeMethods.HidD_GetSerialNumberString(hidHandle, data(0), data.Length)
            Catch exception As Exception
                Throw New Exception(String.Format("Error accessing HID device '{0}'.", _devicePath), exception)
            Finally
                If hidHandle <> IntPtr.Zero AndAlso hidHandle <> Handle Then CloseDeviceIO(hidHandle)
            End Try

            Return success
        End Function

        Public Function Write(ByVal data As Byte()) As Boolean Implements IHidDevice.Write
            Return Write(data, 0)
        End Function

        Public Function Write(ByVal data As Byte(), ByVal timeout As Integer) As Boolean Implements IHidDevice.Write
            If IsConnected Then
                If IsOpen = False Then OpenDevice(_deviceReadMode, _deviceWriteMode, _deviceShareMode)

                Try
                    Return WriteData(data, timeout)
                Catch
                    Return False
                End Try
            End If

            Return False
        End Function

        Public Sub Write(ByVal data As Byte(), ByVal callback As WriteCallback) Implements IHidDevice.Write
            Write(data, callback, 0)
        End Sub

        Public Sub Write(ByVal data As Byte(), ByVal callback As WriteCallback, ByVal timeout As Integer) Implements IHidDevice.Write
            Dim writeDelegate = New WriteDelegate(AddressOf Write)
            Dim asyncState = New HidAsyncState(writeDelegate, callback)
            writeDelegate.BeginInvoke(data, timeout, New System.AsyncCallback(AddressOf EndWrite), asyncState)
        End Sub

        Public Async Function WriteAsync(ByVal data As Byte(), ByVal Optional timeout As Integer = 0) As Task(Of Boolean) Implements IHidDevice.WriteAsync
            Dim writeDelegate = New WriteDelegate(AddressOf Write)
            Return Await Task(Of Boolean).Factory.FromAsync(AddressOf writeDelegate.BeginInvoke, AddressOf writeDelegate.EndInvoke, data, timeout, Nothing)
        End Function

        Public Function WriteReport(ByVal report As HidReport) As Boolean Implements IHidDevice.WriteReport
            Return WriteReport(report, 0)
        End Function

        Public Function WriteReport(ByVal report As HidReport, ByVal timeout As Integer) As Boolean Implements IHidDevice.WriteReport
            Return Write(report.GetBytes(), timeout)
        End Function

        Public Sub WriteReport(ByVal report As HidReport, ByVal callback As WriteCallback) Implements IHidDevice.WriteReport
            WriteReport(report, callback, 0)
        End Sub

        Public Sub WriteReport(ByVal report As HidReport, ByVal callback As WriteCallback, ByVal timeout As Integer) Implements IHidDevice.WriteReport
            Dim writeReportDelegate = New WriteReportDelegate(AddressOf WriteReport)
            Dim asyncState = New HidAsyncState(writeReportDelegate, callback)
            writeReportDelegate.BeginInvoke(report, timeout, New System.AsyncCallback(AddressOf EndWriteReport), asyncState)
        End Sub

        Public Function WriteReportSync(ByVal report As HidReport) As Boolean
            If report IsNot Nothing Then
                Dim buffer As Byte() = report.GetBytes()
                Return (NativeMethods.HidD_SetOutputReport(Handle, buffer, buffer.Length))
            Else
                Throw New ArgumentException("The output report is null, it must be allocated before you call this method", "report")
            End If
        End Function

        Public Async Function WriteReportAsync(ByVal report As HidReport, ByVal Optional timeout As Integer = 0) As Task(Of Boolean) Implements IHidDevice.WriteReportAsync
            Dim writeReportDelegate = New WriteReportDelegate(AddressOf WriteReport)
            Return Await Task(Of Boolean).Factory.FromAsync(AddressOf writeReportDelegate.BeginInvoke, AddressOf writeReportDelegate.EndInvoke, report, timeout, Nothing)
        End Function

        Public Function CreateReport() As HidReport Implements IHidDevice.CreateReport
            Return New HidReport(Capabilities.OutputReportByteLength)
        End Function

        Public Function WriteFeatureData(ByVal data As Byte()) As Boolean Implements IHidDevice.WriteFeatureData
            If _deviceCapabilities.FeatureReportByteLength <= 0 Then Return False
            Dim buffer = CreateFeatureOutputBuffer()
            Array.Copy(data, 0, buffer, 0, Math.Min(data.Length, _deviceCapabilities.FeatureReportByteLength))
            Dim hidHandle As IntPtr = IntPtr.Zero
            Dim success As Boolean = False

            Try

                If IsOpen Then
                    hidHandle = Handle
                Else
                    hidHandle = OpenDeviceIO(_devicePath, NativeMethods.ACCESS_NONE)
                End If

                success = NativeMethods.HidD_SetFeature(hidHandle, buffer, buffer.Length)
            Catch exception As Exception
                Throw New Exception(String.Format("Error accessing HID device '{0}'.", _devicePath), exception)
            Finally
                If hidHandle <> IntPtr.Zero AndAlso hidHandle <> Handle Then CloseDeviceIO(hidHandle)
            End Try

            Return success
        End Function

        Protected Shared Sub EndRead(ByVal ar As IAsyncResult)
            Dim hidAsyncState = CType(ar.AsyncState, HidAsyncState)
            Dim callerDelegate = CType(hidAsyncState.CallerDelegate, ReadDelegate)
            Dim callbackDelegate = CType(hidAsyncState.CallbackDelegate, ReadCallback)
            Dim data = callerDelegate.EndInvoke(ar)
            If (callbackDelegate IsNot Nothing) Then callbackDelegate.Invoke(data)
        End Sub

        Protected Shared Sub EndReadReport(ByVal ar As IAsyncResult)
            Dim hidAsyncState = CType(ar.AsyncState, HidAsyncState)
            Dim callerDelegate = CType(hidAsyncState.CallerDelegate, ReadReportDelegate)
            Dim callbackDelegate = CType(hidAsyncState.CallbackDelegate, ReadReportCallback)
            Dim report = callerDelegate.EndInvoke(ar)
            If (callbackDelegate IsNot Nothing) Then callbackDelegate.Invoke(report)
        End Sub

        Private Shared Sub EndWrite(ByVal ar As IAsyncResult)
            Dim hidAsyncState = CType(ar.AsyncState, HidAsyncState)
            Dim callerDelegate = CType(hidAsyncState.CallerDelegate, WriteDelegate)
            Dim callbackDelegate = CType(hidAsyncState.CallbackDelegate, WriteCallback)
            Dim result = callerDelegate.EndInvoke(ar)
            If (callbackDelegate IsNot Nothing) Then callbackDelegate.Invoke(result)
        End Sub

        Private Shared Sub EndWriteReport(ByVal ar As IAsyncResult)
            Dim hidAsyncState = CType(ar.AsyncState, HidAsyncState)
            Dim callerDelegate = CType(hidAsyncState.CallerDelegate, WriteReportDelegate)
            Dim callbackDelegate = CType(hidAsyncState.CallbackDelegate, WriteCallback)
            Dim result = callerDelegate.EndInvoke(ar)
            If (callbackDelegate IsNot Nothing) Then callbackDelegate.Invoke(result)
        End Sub

        Private Function CreateInputBuffer() As Byte()
            Return CreateBuffer(Capabilities.InputReportByteLength - 1)
        End Function

        Private Function CreateOutputBuffer() As Byte()
            Return CreateBuffer(Capabilities.OutputReportByteLength - 1)
        End Function

        Private Function CreateFeatureOutputBuffer() As Byte()
            Return CreateBuffer(Capabilities.FeatureReportByteLength - 1)
        End Function

        Private Shared Function CreateBuffer(ByVal length As Integer) As Byte()
            Dim buffer As Byte() = Nothing
            Array.Resize(buffer, length + 1)
            Return buffer
        End Function

        Private Shared Function GetDeviceAttributes(ByVal hidHandle As IntPtr) As HidDeviceAttributes
            Dim deviceAttributes As NativeMethods.HIDD_ATTRIBUTES = Nothing
            deviceAttributes.Size = Marshal.SizeOf(deviceAttributes)
            NativeMethods.HidD_GetAttributes(hidHandle, deviceAttributes)
            Return New HidDeviceAttributes(deviceAttributes)
        End Function

        Private Shared Function GetDeviceCapabilities(ByVal hidHandle As IntPtr) As HidDeviceCapabilities
            Dim capabilities As NativeMethods.HIDP_CAPS = Nothing
            Dim preparsedDataPointer As IntPtr = Nothing

            If NativeMethods.HidD_GetPreparsedData(hidHandle, preparsedDataPointer) Then
                NativeMethods.HidP_GetCaps(preparsedDataPointer, capabilities)
                NativeMethods.HidD_FreePreparsedData(preparsedDataPointer)
            End If

            Return New HidDeviceCapabilities(capabilities)
        End Function

        Private Function WriteData(ByVal data As Byte(), ByVal timeout As Integer) As Boolean
            If _deviceCapabilities.OutputReportByteLength <= 0 Then Return False
            Dim buffer = CreateOutputBuffer()
            Dim bytesWritten As UInteger = 0
            Array.Copy(data, 0, buffer, 0, Math.Min(data.Length, _deviceCapabilities.OutputReportByteLength))

            If _deviceWriteMode = DeviceMode.Overlapped Then
                Dim security = New NativeMethods.SECURITY_ATTRIBUTES()
                Dim overlapped = New NativeOverlapped()
                Dim overlapTimeout = If(timeout <= 0, NativeMethods.WAIT_INFINITE, timeout)
                security.lpSecurityDescriptor = IntPtr.Zero
                security.bInheritHandle = True
                security.nLength = Marshal.SizeOf(security)
                overlapped.OffsetLow = 0
                overlapped.OffsetHigh = 0
                overlapped.EventHandle = NativeMethods.CreateEvent(security, Convert.ToInt32(False), Convert.ToInt32(True), "")

                Try
                    NativeMethods.WriteFile(Handle, buffer, CUInt(buffer.Length), bytesWritten, overlapped)
                Catch
                    Return False
                End Try

                Dim result = NativeMethods.WaitForSingleObject(overlapped.EventHandle, overlapTimeout)

                Select Case result
                    Case NativeMethods.WAIT_OBJECT_0
                        Return True
                    Case NativeMethods.WAIT_TIMEOUT
                        Return False
                    Case NativeMethods.WAIT_FAILED
                        Return False
                    Case Else
                        Return False
                End Select
            Else

                Try
                    Dim overlapped = New NativeOverlapped()
                    Return NativeMethods.WriteFile(Handle, buffer, CUInt(buffer.Length), bytesWritten, overlapped)
                Catch
                    Return False
                End Try
            End If
        End Function

        Protected Function ReadData(ByVal timeout As Integer) As HidDeviceData
            Dim buffer = New Byte() {}
            Dim status = HidDeviceData.ReadStatus.NoDataRead
            Dim nonManagedBuffer As IntPtr

            If _deviceCapabilities.InputReportByteLength > 0 Then
                Dim bytesRead As UInteger = 0
                buffer = CreateInputBuffer()
                nonManagedBuffer = Marshal.AllocHGlobal(buffer.Length)

                If _deviceReadMode = DeviceMode.Overlapped Then
                    Dim security = New NativeMethods.SECURITY_ATTRIBUTES()
                    Dim overlapped = New NativeOverlapped()
                    Dim overlapTimeout = If(timeout <= 0, NativeMethods.WAIT_INFINITE, timeout)
                    security.lpSecurityDescriptor = IntPtr.Zero
                    security.bInheritHandle = True
                    security.nLength = Marshal.SizeOf(security)
                    overlapped.OffsetLow = 0
                    overlapped.OffsetHigh = 0
                    overlapped.EventHandle = NativeMethods.CreateEvent(security, Convert.ToInt32(False), Convert.ToInt32(True), String.Empty)

                    Try
                        Dim success = NativeMethods.ReadFile(Handle, nonManagedBuffer, CUInt(buffer.Length), bytesRead, overlapped)

                        If success Then
                            status = HidDeviceData.ReadStatus.Success
                        Else
                            Dim result = NativeMethods.WaitForSingleObject(overlapped.EventHandle, overlapTimeout)

                            Select Case result
                                Case NativeMethods.WAIT_OBJECT_0
                                    status = HidDeviceData.ReadStatus.Success
                                    NativeMethods.GetOverlappedResult(Handle, overlapped, bytesRead, False)
                                Case NativeMethods.WAIT_TIMEOUT
                                    status = HidDeviceData.ReadStatus.WaitTimedOut
                                    buffer = New Byte() {}
                                Case NativeMethods.WAIT_FAILED
                                    status = HidDeviceData.ReadStatus.WaitFail
                                    buffer = New Byte() {}
                                Case Else
                                    status = HidDeviceData.ReadStatus.NoDataRead
                                    buffer = New Byte() {}
                            End Select
                        End If

                        Marshal.Copy(nonManagedBuffer, buffer, 0, CInt(bytesRead))
                    Catch
                        status = HidDeviceData.ReadStatus.ReadError
                    Finally
                        CloseDeviceIO(overlapped.EventHandle)
                        Marshal.FreeHGlobal(nonManagedBuffer)
                    End Try
                Else

                    Try
                        Dim overlapped = New NativeOverlapped()
                        NativeMethods.ReadFile(Handle, nonManagedBuffer, CUInt(buffer.Length), bytesRead, overlapped)
                        status = HidDeviceData.ReadStatus.Success
                        Marshal.Copy(nonManagedBuffer, buffer, 0, CInt(bytesRead))
                    Catch
                        status = HidDeviceData.ReadStatus.ReadError
                    Finally
                        Marshal.FreeHGlobal(nonManagedBuffer)
                    End Try
                End If
            End If

            Return New HidDeviceData(buffer, status)
        End Function

        Private Shared Function OpenDeviceIO(ByVal devicePath As String, ByVal deviceAccess As UInteger) As IntPtr
            Return OpenDeviceIO(devicePath, DeviceMode.NonOverlapped, deviceAccess, ShareMode.ShareRead Or ShareMode.ShareWrite)
        End Function

        Private Shared Function OpenDeviceIO(ByVal devicePath As String, ByVal deviceMode As DeviceMode, ByVal deviceAccess As UInteger, ByVal shareMode As ShareMode) As IntPtr
            Dim security = New NativeMethods.SECURITY_ATTRIBUTES()
            Dim flags = 0
            If deviceMode = DeviceMode.Overlapped Then flags = NativeMethods.FILE_FLAG_OVERLAPPED
            security.lpSecurityDescriptor = IntPtr.Zero
            security.bInheritHandle = True
            security.nLength = Marshal.SizeOf(security)
            Return NativeMethods.CreateFile(devicePath, deviceAccess, CInt(shareMode), security, NativeMethods.OPEN_EXISTING, flags, 0)
        End Function

        Private Shared Sub CloseDeviceIO(ByVal handle As IntPtr)
            If Environment.OSVersion.Version.Major > 5 Then
                NativeMethods.CancelIoEx(handle, IntPtr.Zero)
            End If

            NativeMethods.CloseHandle(handle)
        End Sub

        Private Sub DeviceEventMonitorInserted()
            If Not IsOpen Then OpenDevice(_deviceReadMode, _deviceWriteMode, _deviceShareMode)
            RaiseEvent Inserted()
        End Sub

        Private Sub DeviceEventMonitorRemoved()
            If IsOpen Then CloseDevice()
            RaiseEvent Removed()
        End Sub

        Public Sub Dispose() Implements IHidDevice.Dispose
            If MonitorDeviceEvents Then MonitorDeviceEvents = False
            If IsOpen Then CloseDevice()
        End Sub
    End Class

    Public Class HidAsyncState
        Private ReadOnly _callerDelegate As Object
        Private ReadOnly _callbackDelegate As Object

        Public Sub New(ByVal callerDelegate As Object, ByVal callbackDelegate As Object)
            _callerDelegate = callerDelegate
            _callbackDelegate = callbackDelegate
        End Sub

        Public ReadOnly Property CallerDelegate As Object
            Get
                Return _callerDelegate
            End Get
        End Property

        Public ReadOnly Property CallbackDelegate As Object
            Get
                Return _callbackDelegate
            End Get
        End Property
    End Class

    Module Extensions
        <Extension()>
        Function ToUTF8String(ByVal buffer As Byte()) As String
            Dim value = Encoding.UTF8.GetString(buffer)
            Return value.Remove(value.IndexOf(ChrW(0)))
        End Function

        <Extension()>
        Function ToUTF16String(ByVal buffer As Byte()) As String
            Dim value = Encoding.Unicode.GetString(buffer)
            Return value.Remove(value.IndexOf(ChrW(0)))
        End Function
    End Module
End Namespace