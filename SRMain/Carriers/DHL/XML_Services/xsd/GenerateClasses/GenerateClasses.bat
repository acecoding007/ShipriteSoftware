@echo off

rem Commands against GenerateClasses schemas

"c:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\xsd.exe" /p:.\DCT_Request-GenerateClasses.xsd /o:..\..
"c:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\xsd.exe" /p:.\DCT_Response-GenerateClasses.xsd /o:..\..
"c:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\xsd.exe" /p:.\Err_Response-GenerateClasses.xsd /o:..\..
"c:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\xsd.exe" /p:.\Ship_Request-GenerateClasses.xsd /o:..\..
"c:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\xsd.exe" /p:.\Ship_Err_Response-GenerateClasses.xsd /o:..\..
"c:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\xsd.exe" /p:.\Ship_Response-GenerateClasses.xsd /o:..\..

rem pause