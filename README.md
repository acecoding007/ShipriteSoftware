# ShipRite Next

## Development Prerequisites

### Microsoft Visual Studio 2019 (>= Community)
- Main development environment.
- Using this version for best results with latest Crystal Reports versions. CR can have compatibility issues with older VS now.

#### Installers
- *Install: https://learn.microsoft.com/en-us/visualstudio/releases/2019/system-requirements > Download > Click "Download Community 2019".* 
  - Note: If have subscription for professional or higher version, then you may found a download available in your Visual Studio Subscriptions: https://my.visualstudio.com/ > log in with MS credentials > click Downloads tab > search for visual studio 2019 professional or higher version to download.

#### Extensions

##### Optional
- Advanced Installer for Visual Studio 2019 (**Requires Paid License**)
  - VS 2019 > Extensions > Manage Extensions > Click "Online" in left menu > Search for "Advanced Installer for Visual Studio 2019" > Select it and click "Download" > Follow steps to install.
    - *License: Professional (developers group email)*
	- *Install: https://www.advancedinstaller.com/download.html*


### Microsoft Office Access Database Engine 2010 (32-bit)
- To communicate with MS Access databases (.mdb, .accdb).
- Using this version to prevent issues when SRPro users are converted to SRN and they need to possibly use both programs at once.
  - Tried newer versions and this caused SRPro to show errors when trying to communicate with databases.

#### Installers
- *Install: .\\_Build_Files\\ShipriteNext\\AccessDatabaseEngine.exe*
  - Official online installer not available any more (current lowest version found is 2010).
  
#### To Do
- **Look into updating this while still being able to work with SRPro if possible.**


### SAP Crystal Reports for Visual Studio (13.0.27)
- To view, create, edit crystal reports in VS 2019.
- Need this version installed. Previous versions don't work with current Visual Studio versions.
  - At end of install asked to install 64-bit runtime, confirm this choice.
  - Also download and install 32-bit runtime.

#### Installers
- *Downloads: https://origin.softwaredownloads.sap.com/public/site/index.html*
  1. Under "Software Product" select "SAP Crystal Reports, version for Visual Studio" and click "Go" button to search.
  2. Find entries in the list with "CR for Visual Studio SP27".
     - *Install: "CR for Visual Studio SP27 install package"*
	   - Note: Includes install for VS, Click Once, and 64-bit Runtime.
     - *64-bit Runtime: "CR for Visual Studio SP27 CR Runtime 64-bit MSI"*
     - *32-bit Runtime: "CR for Visual Studio SP27 CR Runtime 32-bit MSI"*
     - *Visual Studio Click Once: "CR for Visual Studio SP27 Click Once"*

#### Note
- SRPro versions < 10.20.5 were using CR version 13.0.22. So both SRN and SRPro would have issues depending on which CR version was installed.
  - e.g., If Crystal Reports version 13.0.27 was installed, error message would show when starting SRPro due to mismatching CR assemblies.
- This adds log4net.dll as dependency in Setup Installer.


### Install/Runtime Files
- Files used by the project can be found in the repo in the .\\_Build_Files directory. 
- *Install: Copy .\\_Build_Files\\\* to C: drive.*
