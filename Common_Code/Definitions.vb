Module Definitions

    Public gDC As String    ' This is the Date Delimeter  # for Access, ' for SQL Server
    Public gConversionProcessHasRun As Boolean
    Public gROAinEffect As Boolean
    Public gRegistrationExpired As Boolean
    Public gSupportExpired As Boolean

    Public Structure ARBrecord

        Dim AcctNum As String
        Dim AcctName As String
        Dim InvNum As Long
        Dim FirstDate As Date
        Dim Charges As Double
        Dim Payments As Double
        Dim BALANCE As Double
        Dim PONum As String
        Dim Desc As String
        Dim SalesRep As String

    End Structure


    Public Structure ARNameRecordSet

        Dim AcctNum As String
        Dim AcctName As String

    End Structure

    Public gRecordCT As Long
    Public gSQLErrorMessage As String
    Public gCompletedPackageStack As String  ' package IDs of shipments that shipped.  Used to pass shipment data to POS
    Public gReceiptCCEndBlock As String
    Structure ServiceCenterArray

        Dim ID As Long
        Dim Col1 As String
        Dim Col2 As String
        Dim Col3 As String

    End Structure

    Public gSA(50) As ServiceCenterArray
    Public gSAct As Integer
    Public gsaTOP As Integer

    Structure ConnectionStringStruct

        Dim Name As String
        Dim ConnectionString As String

    End Structure
    Public gConnectionStrings(10) As ConnectionStringStruct
    Public gCS As Integer
    Public gAzureConnectionString As String
    Public Structure CallingBlock

        Public BannerName As String
        Public Caption As String
        Public Command As String
        Public DatabaseName As String
        Public TableName As String
        Public SQL As String
        Public DetailSQL As String
        Public DetailBanner As String
        Public Segment As String
        Public XML As String
        Public PID As String
        Public DisplayRules As String
        Public IndexFieldName As String
        Public IndexValue As String
        Public ShowAddButton As Boolean
        Public result As String
        Public ShowEditButton As Boolean
        Public DataEntryTitle As String
        Public PREProcessing As String
        Public POSTProcessing As String
        Public UseDataBaseSchema As Boolean
        Public DataBaseSchema As String

    End Structure
    Public CallingStack As CallingBlock
    Public EmptyStack As CallingBlock
    Public Structure DataModelBlock

        Public buf As String
        Public DBName As String

    End Structure

    Public gShipmentParameters As String
    Public Const gProgramName As String = "ShipRite Next"
    Public gDisableSocetServices As Boolean
    Public gDBpath As String
    Public gSSDBpath As String
    Public gAppPath As String
    Public gSSAppPath As String
    Public gShipriteDB As String
    Public gPricingMatrixDB As String
    Public gSmartSwiperDB As String
    Public gSmartSwiperReportsDB As String
    Public gAppointmentsDB As String
    Public gSocetsLocation As String
    Public gSecurityDB As String
    Public gQBdb As String
    Public gSalonDB As String
    Public gContactsTableSchema As String
    Public gOpenCloseSchema As String
    Public gARTableSchema As String
    Public gStatementsSchema As String
    Public gInventorySchema As String
    Public gCCHistorySchema As String
    Public gMailboxTableSchema As String
    Public gMBXHistoryTableSchema As String
    Public gPosButtonsTableSchema As String
    Public gTicklerSchema As String
    Public gDropOffDB As String
    Public gDropOffSchema As String
    Public gMailboxDB As String
    Public gMailBoxSchema As String

    Public gManifestSchema As String
    Public gTransactionLog As String
    Public gDentouchDB As String
    Public gFinanceDB As String
    Public gServiceDB As String
    Public gSearchResult As String
    Public gDataEntryDB As String

    Public gDMct As Integer
    Public gDataModel(25) As DataModelBlock
    Public gCopyRight As String
    Public gSetup As String
    Public gLocalSetup As String

    Public gIsProgramSecurityEnabled As Boolean
    Public gIsPOSSecurityEnabled As Boolean
    Public gIsSetupSecurityEnabled As Boolean

    ' 3rd Party Insurance
    Public gThirdPartyInsurance As Boolean

    Public gFedExServicesDB As String
    Public gFedExRetailServicesDB As String
    Public gFedExZoneDB As String
    Public gFedExSETUP As FedExREST_SETUP
    Public gFedExReturnsSETUP As FedExRETURNS_SETUP
    Public gUPSServicesDB As String
    Public gUPSRetailServicesDB As String
    Public gUPSZoneDB As String
    Public gDHLServicesDB As String
    Public gDHLZoneDB As String
    Public gUSMailDB_Services As String
    Public gUSMailDB_Zones As String
    Public gSpeeDeeServicesDB As String
    Public gSpeeDeeZoneDB As String

    Public gDASPath As String
    Public gServiceTablesPath As String
    Public gTemplatesPath As String
    Public gZoneTablesPath As String

    Public gCarrierList As List(Of Carrier)

    Public gCustomerDisplay As CustomerDisplay
    Public gIsCustomerDisplayEnabled As Boolean

    Public gUPSPeakSurcharges As List(Of Peak_Surcharge)
    Public gFedExPeakSurcharges As List(Of Peak_Surcharge)
    Public gDHLPeakSurcharges As List(Of Peak_Surcharge)

    Public Structure PolicyBlock

        Public buf As String
        Public DBName As String

    End Structure

    Public gPolicyCT As Integer
    Public gPolicy(20) As PolicyBlock
    Public gPolicySet As String

    Public gPOSpolicy As Integer
    Public gPOpolicy As Integer
    Public gQLpolicy As Integer
    Public gARPolicy As Integer
    Public gDataEntryPolicy As Integer
    Public gGLOBALpolicy As Integer
    Public gPaymentPolicy As Integer
    Public gIOCT As Integer
    Public gSEARCH As Integer
    Public gCHARTING As Integer


    Public gReportsDB As String
    Public gFeesDB As String
    Public gCurrentUser As String
    Public gCurrentUserName As String
    Public gCurrentUserSegment As String
    Public gResult As String
    Public gResult2 As String
    Public gResult3 As String
    Public gResult4 As String
    Public gResult5 As String
    Public gHVR_Result As String
    Public gProductionDB As String
    Public gLastError As String
    Public gLastSQL As String
    Public gSetupPolicy As String
    Public gShipritePolicy As String
    Public gRSetupPolicy As String
    Public gSSetupPolicy As String
    Public gQBSetupPolicy As String
    Public gSalonPolicy As String
    Public gSwiperSetupPolicy As String
    Public gCommonTableProcessorDB As String

    Public gCDH_URL As String

    Public ContactsTableSchema As String
    Public ARTableSchema As String
    Public RepetitiveSchema As String
    Public TransactionsSchema As String
    Public PaymentsSchema As String
    Public ARAgingSchema As String

    Public gProgramEdition As String
    Public gStopITNow As Boolean

    Public gCurrentCloseID As String
    Public GENIUS_DoubleCheck As Double
    Public gIamServer As Boolean
    Public gProductionCDHLenabled As Boolean
    Public gDTS As String
    Public gAR As String
    Public gDefaultTaxCounty As String
    Public gFinancePolicy As String

    Public gAzure As String
    Public gPackageSegmentSet As String
    Public gMergeSQL As String

    Public POS_CACHE As String
    Public gHome As Boolean
    Public gButtonSegment As String
    Public gReturnToScannedInventory As Boolean

    Public Structure Coupon

        Public SKU As String
        Public Description As String
        Public StartDate As String
        Public EndDate As String
        Public TypeOfCoupon As String
        Public LIMIT As Integer
        Public AffectedInventory As String

    End Structure

    Structure InventoryCounter

        Public SKU As String
        Public Dept As String
        Public Qty As Integer
        Public UnitPrice As Double

    End Structure

    Public gSQL As String

    Public gSQLquery As String
    Public gSQLdb As String
    Public gCredentialSegment As String
    Public gTransactionSegment As String
    Public gValidationSegment As String
    Public gAID As String
    Public gPINStatement As String
    Public gCenterID As String

    Public gDataBaseActivityInProgress As Boolean

    Public doev As Integer

    Public CACHE_OtherFormsOfPayment As String

    Public gClientSocetNumber As String

    Public gSalonX As Boolean

    Public BB_Enabled As Boolean
    Public BB_ServicesEnabled As Boolean
    Public BB_HostIP As String
    Public BB_HostPort As String
    Public BB_VendorNumber As String
    Public BB_TerminalNumber As String
    Public BB_SequenceNumber As String
    Public BB_TenderNumber As String
    Public BB_LoadBalance As Boolean
    Public BB_CheckBalance As Boolean

    Public LRC_Failure_Counter As Integer

    Public gSocetGate As Boolean
    Public gDefaultLockoutTimerInSeconds As Integer
    Public gTest As Boolean
    Public Buffer As String
    Public gBookingSegment As String

    Public Structure SalonMenuStack

        Public MenuName As String
        Public SegmentSet As String

    End Structure
    Public gSalonMenuStack(100) As SalonMenuStack
    Public gSalonMenuCT As Integer
    Public gGCT As Integer
    Public Structure ButtonStructure

        Public ID As Long
        Public Description As String
        Public SKU As String
        Public BUTTON_CODE As String
        Public BUTTON_GROUP As String
        Public BUTTON_TYPE As String
        Public Language1 As String
        Public Language2 As String
        Public BackColor As String
        Public ForeColor As String

    End Structure

    Public Structure GroupStructure

        Public ID As Long
        Public Description As String
        Public SKU As String
        Public BUTTON_CODE As String
        Public BUTTON_GROUP As String
        Public BUTTON_TYPE As String
        Public Language1 As String
        Public Language2 As String
        Public BackColor As String
        Public ForeColor As String
        Public bct As Integer
        Public BStack() As ButtonStructure

    End Structure

    Public GStack(25) As GroupStructure

    Public Structure StockStack

        Public SKU As String
        Public ModelNumber As String
        Public SegmentSet As String

    End Structure
    Public gStockStack(100) As StockStack
    Public gStockStackCT As Integer


    Public gNoSwiper As Boolean
    Public gColorCodes As String
    Public gListTimeStamp As String
    Public gTrainingMode As Boolean
    Public gSalonTrainingMode As Boolean
    Public gSocetsServer As String

    Public gKeyboardInput As String
    Public gSalonBookingSegment As String
    Public gMailServer As String
    Public gMailServerUser As String
    Public gMailServerPassword As String
    Public gMailServerPort As String

    Public gServerIP As String
    Public gSocetID As String
    Public gSOCET_Data As String
    Public gSOCET_DATA_ARRIVED As Boolean
    Public gSOCET_ACK_RECEIVED As Boolean

    Public gInventoryMethodISPerpetual As Boolean
    Public gAcceptPaymentsSource As String

    Public gInitializeInventory As Boolean
    Public gLogoMain As String
    Public gAppointmentSegment As String

    Public gPOSGiftCard As Boolean

    Public gInvoiceStack As String
    Public gSearchValue As String

    Public gMW_TransportKeyEndpoint As String
    Public gMW_Transport_SalesEndpoint As String
    Public gMW_Transport_ReturnEndPoint As String
    Public gMW_Transport_VoidEndPoint As String
    Public gMW_Transport_BatchEndPoint As String

    Public gWP_WorldPayEnabled As Boolean

    Public gGIFTNumber As String

    Public UseAppointmentBook As Boolean
    Public Structure HeaderBlock

        Public Status As String
        Public AcctNum As String
        Public TicketNum As String
        Public OrderDate As String
        Public Name As String
        Public FName As String
        Public LName As String
        Public Phone As String
        Public Tech As String
        Public loc As String
        Public Addr1 As String
        Public Addr2 As String
        Public AddCode As String
        Public PCAgent As String
        Public City As String
        Public State As String
        Public Zip As String
        Public Repaired As String
        Public DateProm As String
        Public DateServiced As String
        Public Brand As String
        Public Product As String
        Public ModelNumber As String
        Public SerialNumber As String
        Public BillTo As String
        Public PartsBillTo As String
        Public Estimate As String
        Public TotalParts As String
        Public TotalLabor As String
        Public InvNum As String
        Public InvoiceDate As String
        Public NARDA As String
        Public Problem As String
        Public Resolution As String

    End Structure

    Public Structure FramingProject

        Public ID As Long
        Public ProjectID As String
        Public CreatedBy As String

        Public Changed As Boolean

        Public Desc As String
        Public Category As String
        Public pLength As String
        Public pWidth As String
        Public FrameClass As String

        Public Action_Framing As Boolean
        Public Action_Printing As Boolean
        Public Action_Restoration As Boolean
        Public Action_Repair As Boolean
        Public Action_ImageDownload As Boolean
        Public Action_CanvasTransfer As Boolean
        Public Action_ShadowBox As Boolean

        Public Action_Printing_Type As String
        Public Action_Restoration_Type As String
        Public Action_Repair_Type As String
        Public Action_ImageDownload_Type As String
        Public Action_CanvasTranfer_Type As String
        Public Action_ShadowBox_Type As String

        Public ArtworkSN As String
        Public VisibleMat As String
        Public Mat_1 As String
        Public Mat_2 As String
        Public Mat_3 As String
        Public Gap_1 As String
        Public Gap_2 As String
        Public ShapesCT As String
        Public MLength As String
        Public MWidth As String
        Public MoldingLength As String
        Public MoldingWidth As String
        Public Molding_1 As String
        Public Molding_2 As String
        Public MoldingInFeet As String
        Public Fillets As String
        Public Glass As String

        Public Other_MountingHW As Boolean
        Public Other_FoamBoard As Boolean
        Public Other_FoamBoard_Type As String
        Public Other_DryMounting As Boolean
        Public Other_CanvasPrinting As Boolean
        Public Other_CanvasStretching As Boolean

        Public ProjectNotes As String

        Public Difficulty As Integer
        Public Notes As String
        Public ProjectCost As Double
        Public ProjectSell As Double

        Public AutoExit As Boolean

    End Structure

    Public FP As FramingProject
    Public FP_Blank As FramingProject


    Public gTabletStyle As String
    Public gEnableTablet As Boolean

    Public ProviderBuffer As String

    Public gWHERE_AM_I As String
    Public gWAIT_FOR_IO As Boolean
    Public gReturnCT As Integer

    Public gDefaultNewTicketStatus As String

    Public gCreditSale As Boolean

    Public Structure MenuControlBlock

        Public LoadMenu As String
        Public ReturnMenu As String
        Public ControlSegment As String
        Public Banner As String

    End Structure
    Public gTWSegment As String

    Public UtilityStack(20) As MenuControlBlock
    Public UtilityMenuCT As Integer

    Public ServiceMenuStack(20) As MenuControlBlock
    Public ServiceMenuCT As Integer

    Public MenuCurrent As String
    Public gTestID As String

    Public gTemporarilySuspendSecurity As Boolean

    Public gPOSDefaultTaxSegment As String
    Public gPOSCurrentTaxSegment As String
    Public gPOSCategoriesBuffer As String
    Public gPOSBrandsBuffer As String
    Public gPOSMenuButtons As String
    Public gFingerPrintFileName As String
    Public gServiceStatus As String
    Public gSolutionIndicator As String
    Public gServiceLocation As String
    Public gFramersDB As String
    Public gHospitalityDB As String
    Public gSalonInvoice As String
    Public gSalonCustomer As String
    Public gHIPAAdb As String
    Public gRptPath As String
    Public gReportWriter As String
    Public gSmartServerDB As String
    Public gSmartServerAppPath As String
    Public gLoggingDB As String
    Public gClaimsDB As String
    Public gDRSDB As String
    Public gPostNetDB As String
    Public gCountryDB As String
    Public gTransactionLogging As Boolean
    Public gErrorLog As String
    Public gHomeDirectory As String
    Public gAttPath As String

    Public gSwipingIsInEffect As Boolean
    Public gStatusTag As String
    Public gUsingAddWizards As String
    Public gServiceReportsBlock As String
    Public gPaymentSegmentSet As String
    Public gPaymentHeader As String
    Public gPaymentData As String
    Public gEnableTransactionLogging As Boolean
    Public gTaskSegment As String
    Public gCreditCardSegment As String
    Public gRemoteControlSegment As String

    Public gSmartSwiperDBAuthOnly As Boolean
    Public gSmartSwiperPrintOnlyFromHost As Boolean
    Public gSmartSwiperNoPrint As Boolean

    Public gDrawerID As String
    Public gHoldPolicyCT As Integer
    Public gHoldPolicyBuffers(20) As String
    Public gCurrentPrinter As String
    Public gIniPath As String
    Public gIniGroup As String
    Public gIniDefaultDirectory As String
    Public gIniShipriteIndicator As String
    Public gLoaderDatabase As String
    Public gDataEntryShowPrintButton As Boolean
    Public gDepartment As String
    Public gLocation As String
    Public gStoreZip As String
    Public gDatabaseInfo As String
    Public gApplicationName As String
    Public gDefaultAreaCode As String
    Public gServerName As String
    Public gNodeName As String
    Public gThisIsServer As Boolean
    Public gUSMMailBox As String

    Public Structure RebatesStack

        Public ModelNumber As String
        Public Amount As Double
        Public Rebate As Boolean
        Public InstantRebate As Boolean

    End Structure
    Public gRebateCT As Integer
    Public gRebateStack(100) As RebatesStack

    Public Structure WizardElementBlock

        Public Name As String
        Public AliasStr As String
        Public Display As String
        Public HelpText As String
        Public Required As Boolean
        Public DefaultStr As String
        Public Duplicates As String
        Public Results As String
        Public UseDatabase As String
        Public ADDNEW As String

    End Structure

    Public Structure WizardPageBlock

        Public eSet() As WizardElementBlock
        Public eCT As Integer
        Public hold As String

    End Structure


    Public gWIZ_ct As Integer
    Public gWIZ_PAGES(20) As WizardPageBlock
    Public gWIZ_BlankPage As WizardPageBlock
    Public gWIZ_DataSegment As String

    Public gRefundSegment As String

    Public Structure SalesTaxBlock

        Public TaxRate As Double
        Public TotalSale As Double
        Public TotalLine As Double
        Public ComputedTax As Double
        Public CalculatePosition As Integer
        Public IsNegagive As Boolean
        Public ServiceTax As Double

    End Structure
    Public TaxBlock(10) As SalesTaxBlock
    Public BlankTaxBlock As SalesTaxBlock
    Public gTaxCT As Integer

    Public Structure COUNTERS

        Public Read_Requests As Long
        Public Write_Requests As Long
        Public Rows_Affected As Long
        Public Records_Returned As Long

    End Structure
    Public gCOUNT As COUNTERS
    Public gBLANK_COUNTER As COUNTERS

    Public gUserSegment As String

    Public gPOSStyle As String
    Public gMaxNumberRows As Integer
    Public gDemoExpired As Boolean
    Public gBaseProgramVersionDate As String

    Public FTPuserid As String
    Public FTPpassword As String

    Public Structure Top10Block

        Public ModelNumber As String
        Public Description As String
        Public Tally As Long
        Public AvgPrice As Double
        Public Income As Double

    End Structure
    Public gTop10(10) As Top10Block

    Public Structure Top50Block

        Public AcctNum As String
        Public AcctName As String
        Public TotalSales As Double
        Public TotalIncome As Double
        Public InvoiceCT As Long

    End Structure

    Public gTopCT As Integer
    Public gTopBLANK As Top10Block
    Public gTopITEM As Top10Block
    Public gTopHOLD As Top10Block
    Public gStoreName As String
    Public gTitle As String

    Public Structure ServiceBlock

        Public ID As Long
        Public Header As String
        Public CID As Long
        Public SID As Long
        Public AcctNum As String
        Public Name As String
        Public CustomerNameBlock As String
        Public ShipToNameBlock As String
        Public InvoiceNumber As Long
        Public GotoInvoice As Boolean
        Public InvoiceSegmentSet As String
        Public WorkInvoiceNumber As Long
        Public AutoExit As Boolean
        Public AutoInvoice As String
        Public WarrantyID As String
        Public WarrantyEst As String
        Public InvoiceChanged As Boolean
        Public FinalizeInvoice As Boolean

    End Structure
    Public gServiceBlock As ServiceBlock
    Public gBlankServiceBlock As ServiceBlock
    Public gODBCConnectionType As String
    Public gDataEntrySegment As String
    Public gCurrentUserCommissionRate As Double
    Public gWordPadLocation As String
    Public Structure DentistBlock

        Public Name As String
        Public ID As String
        Public SSAN As String

    End Structure

    Public gProvider(100) As DentistBlock

    ' Global Charting Symbol ID's

    Public Const AMALGAM = 1
    Public Const COMPOSITE = 2
    Public Const CROWN = 3

    ' Global RunTimePolicyIndex

    Public gIndexNumber As Integer
    Public gWINCAGEDB As String
    Public gComputerAgeDB As String
    Public gZipCodeDB As String
    Public gWordPath As String
    Public gReadErrors As Long

    Public Const gSecurityPassword As String = "keisha11"

    Public Structure Accounting

        Public AccountNumber As String
        Public Description As String
        Public Amount As Double
        Public Business As String
        Public AccountType As String
        Public AccountGroup As String

    End Structure

    Public gCustomerSegment As String
    Public gAccountSegment As String
    Public gShipToCustomerSegment As String
    Public gLocalConfigurationDB As String
    Public gOSVersion As String
    Public gReportsDBDataModel As String
    Public gContactManagerSegment As String

    Public gAutoExitFromContacts As Boolean

End Module
