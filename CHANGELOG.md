# SRN Changelog

## Unreleased (yyyy-mm-dd)

### Enhancements

*Drop Offs*
- [SRN-1113] Combine separate compensation reports into single Carrier Compensation Report with carrier drop down selection.
- [SRN-1113] Add Summary to bottom of carrier compensation reports.

*POS*
- [SRN-1183] Added option to print store logo on receipt.
- [SRN-1194] Added Clerk name to printed POS receipt.

*Reports*
- [SRN-137] Inventory: Add Inventory Out of Stock Report
- [SRN-1086] Mailbox: Add Expired Mailboxes report that prints a list of all past due mailboxes.

### Fixes

*AR*
- [SRN-1189] If account has unexpected pricing level saved then conversion from string to integer error occurs.
- [SRN-1226] The balance needs to be updated after a line is deleted from the Ledger screen.

*Build Install*
- Update default Report_Writer database with MailBox table.
- Update default ShipriteNext database with FragilityLevel field in Contents table.
- [SRN-1209] Add Holiday.txt file v1 with 2024-2027 holidays to data path.
- Update default ShipriteNext.accdb, Update.accdb database files with latest schema changes.
- [SRN-1218] Update default ShipriteNext.accdb database file with 2025 shipping surcharges.
- [SRN-1113] Update default Drop Offs database with USPS record in DropOff_Compensation table.

*Drop Offs*
- [SRN-809] Cannot print Production Report by Customer.

*MailMaster*
- [SRN-1215] Fixed: Item COGS are not returned to POS

*PackMaster*
- [SRN-361] PackMaster should not do packing calculations until all 3 dimensions are entered.
- [SRN-363] PackMaster: Save Button in Top right not functional.
- [SRN-364] PackMaster: CLEAR ALL button not functional.
- [SRN-366] PackMaster: The Reviewer tab never gets shown/accessed.
- [SRN-367] When processing PackMaster through SHIP, there is no indicator that a packjob is added to the shipment.
- [SRN-370] PackMaster: Double Boxing not working properly.
- [SRN-371] PackMaster: Dim Weight and Length+Girth are not calculated.
- [SRN-372] PackMaster: If entering a dimension that is too large for an existing box, error displays 5 times one after another.
- [SRN-649] PackMaster: Listing of pre-saved packjobs not functional. (pull up, add, edit, delete).
- [SRN-967] PackMaster: After a packjob is added to shipment, it should update the shipping screen.
- [SRN-985] PackMaster: Reviewer option should function the same as in SRPro.
- Separated the Packaging Profile Options into it's own area.

*POS*
- [SRN-1196] Fixed: When reprinting receipt using the "Cash Out" button, the total on the receipt shows 0.
- [SRN-1186] Fixed: Hold Invoice COGS value is lost when sale completed.
- [SRN-1207] Fixed: Voiding Quote Invoice fails with message that the Invoice was included in a previous closing
- [SRN-1217] Added Social Media URL's to POS Receipt
- Fixed word wrap functionality when printing receipt memo, signature, and shipping disclaimer

*POS Payments*
- [SRN-1222] Invoices with negative balances should be not listed in Payment screen to be paid.

*Reports*
- [SRN-1190] Mailbox: Post Office Quarterly Report doesn't include all mailbox names.
- [SRN-1052] Update AR Statements processing to fix data issues.
- [SRN-1185] Shipping: Shipping Reports should not include deleted shipments.
- [SRN-1043] Shipping: Fix Shipping Reports formatting issues.
- [SRN-1044] Production Reports: Customer count is always 0.
- [SRN-1044] Production Report by Dept: Fix SR Version displayed.
- [SRN-1224] Consolidated Z-Report: Date Range in Header shows current date instead of selected date range.
- [SRN-1216] Reports should default to print to the selected printer in Setup, not to the windows default printer.
- [SRN-1013] Z-Report: Other Forms of Payment section not populating Other Text and Sales Rep fields.

*Shipping*
- [SRN-1204] FedEx REST: FedEx services line disappears in Shipping screen when "Letter" packaging selected and TinT is run
- [SRN-1179] Fixed: Third Party Insurance uses regular declared value pricing instead of dedicated third party insurance pricing.
- [SRN-1195] Fixed: DHL: Contact Address Line 3 isn't used in ship request
- [SRN-1208] Fixed: Commercial Invoice will not print 3rd address line.
- [SRN-1213] Update Endicia to print label if valid label returned without tracking.
- [SRN-1214] DHL: Update DHL web requests to round decimal values to 3 decimal places to prevent DHL server fractionDigits errors.

*Shipping Markups*
- [SRN-1176] Pricing Matrix Should not allow descending weight ranges.

*Startup*
- [SRN-1209] Update Holiday.txt processing at startup.

## 1.0.80 (2025-01-17) - [Instructions](https://support.shipritesoftware.com/ShipRite_Updates/SRN/SRN_Update_1.0.80-2025_USPS_Rates.pdf)

### Enhancements

*Shipping*
- [SRN-1142] USPS: Updated USPS rates effective 01/19/2025.
- [SRN-1168] Ship screen should allow fractions of an inch to be entered. Dimension rounding needs to be the nearest whole inch.

### Fixes

*Drop Offs*
- [SRN-1172] Fixed: For some Reports the last date of selected range is omitted from report.

*Package Valet*
- [SRN-1065] CheckOut: Added checkbox column to CheckOut screen so that selected packages are more visible.
- [SRN-1161] CheckIn: Editing a already scanned shipment in CheckIn screen will not update the mailbox number and name.

*POS*
- [SRN-1129] Fixed: After invoice recovered from Hold, any added line items throw off balance on invoice.
- [SRN-1126] Fixed: Newly created POS Memo buttons add wrong memo desc to POS.

*Shipping*
- Fix issue where FedEx Time In Transit request fails and FedEx services are cleared when declared value entered.
- [SRN-1169] PR Origin: Fix UPS Ship request error "ShipperNumber must be the same as the shipments Shipper's country".

*Shipping Markups*
- [SRN-1171] Fixed: In Pricing Matrix selecting "Letter" in the Weight End field will cause an error in SHIP screen

*Startup*
- [SRN-714] If registration key is expired, prompt user to enter in a new key manually at startup.
- Rate Updates: Fix issue where USPS rate upd files not processed.

## 1.0.79 (2024-12-24)

### Enhancements

*Shipping*
- Added support for Puerto Rico origin.
- UPS: Updated UPS Intl EAS/RAS location listing effective 12/23/2024.

### Fixes

*Build Install*
- [SRN-1135] Fix issue where DHL rates effective 01/01/2025 missing from install.

## 1.0.78 (2024-12-20) - [Instructions](https://support.shipritesoftware.com/ShipRite_Updates/SRN/SRN_Update_1.0.78-2025_Rates.pdf)

### Enhancements

*Shipping*
- Added capability to load new rate and zone files on the dates that they are due.
- [SRN-1132] UPS: Updated UPS rates effective 12/23/2024.
- [SRN-1154] UPS: All shipments subject to Additional Handling - Dimensions Surcharge now have a 40lb minimum weight.
- [SRN-1133] UPS: Updated UPS Additional Handling surcharge increase if domestic package based on weight effective 12/23/2024.
- [SRN-1134] UPS: Updated UPS Large Package surcharge increase if residential package effective 12/23/2024.
- [SRN-1136] FedEx: Updated FedEx rates effective 01/06/2025.
- [SRN-1155] FedEx: Updated FedEx DAS zip codes effective 01/13/2025.
- [SRN-1156] FedEx: Updated FedEx One Rate rates effective 01/20/2025.
- [SRN-1152] FedEx: Dry Ice is added to Fuel Surcharge Calculation for International Shipments.
- [SRN-1153] FedEx: All shipments subject to Additional Handling - Dimensions Surcharge now have a 40lb minimum weight.
- [SRN-1137] FedEx: Updated FedEx Additional Handling surcharge increase if package based on weight effective 01/06/2025.
- [SRN-1138] FedEx: Updated FedEx Oversize Package surcharge increase if Ground Home Delivery package effective 01/06/2025.
- [SRN-1131] USPS: Updated USPS National Zone Chart Matrix with zip code listing effective 01/01/2025.
- [SRN-1139] SpeeDee: Updated SpeeDee DAS zip codes effective 01/06/2025.
- [SRN-1135] Updated DHL rates effective 01/01/2025.
- DHL: Updated DHL Elevated Risk countries list.

### Fixes

*Mailbox*
- [SRN-1151] Once a mailbox is renewed, the "Sent Notice" tags need to be cleared.

*Shipping*
- [SRN-1148] Fixed: If "Always Charge Retail Rates" is enabled and dim weight is over 150, then Sell cost of 0 is returned for UPS and FedEx services
- [SRN-1160] DHL Elevated Risk, Restricted Destination surcharges aren't implemented when shipping.

*Shipping Setup*
- Added "Select All" options for accessorial charges to make the Global Update easier.

## 1.0.77 (2024-12-09)

### Enhancements

*AR*
- [SRN-761] Added emailing of AR statements.

*POS*
- [SRN-486] Added email 8.5x11 Invoice functionality.

*Shipping*
- Updated UPS DAS zip codes effective 10/21/2024.

### Fixes

*AR*
- [SRN-1118] Fix issue where "Send Statements", "Auto Pay Balance", and "Finance Charges" checkbox settings aren't saved.

*Build Install*
- Update default Report_Writer database with Addr2 field in ContactSalesData table.

*Letter Master*
- [SRN-1075] Fix issues with Letter Master functionality and Avery label printing.

*Mailbox*
- [SRN-1128] Error "Index was out of range" occurs when Additional Names List empty and editing 1583 Form ID fields.

*Package Valet*
- Update FedEx HAL web requests to use TLSv1.2 security protocol.
- [SRN-1146] Check In: Fix issue where FedEx HAL web requests fail when response includes "HOLD_SATURDAY" value in "BarcodeHandlingType" field.
- Fix FedEx HAL Publish Delivery Event request to include converted PNG image data in signature detail.

*POS*
- [SRN-1090] Invoice Search print option should include only invoices for selected customer.

*Reports*
- [SRN-1052] Fix issue where Account Adjustment records without invoice numbers are deleted when processing AR Statement.

*Shipping*
- [SRN-1127] Don't send UPS Time In Transit request if UPS not setup and authorized.

*Shipping Setup*
- [SRN-1127] UPS Carrier Setup: Clear UPS Authorization info when UPS Account Number is cleared.

*Support Utilities*
- [SRN-1052] Update "Normalize Invoice Balances" utility to add invoice numbers to Account Adjustment records without them.
- [SRN-1052] Add "Clear Cash Balances" utility to clear balances on all CASH account invoices.
- [SRN-1052] Add "Recover Adjustments" utility to re-add Account Adjustment records from old shiprite database.

## 1.0.76 (2024-11-20)

### Enhancements

*Drop Offs*
- [SRN-1095] Added option to put initial focus on either customer name or tracking number.
- [SRN-846] Customer info should transfer to Drop Off Manager when opened from POS.

*Package Valet*
- [SRN-891] Added Package Count display to both Check In and Check Out screens.

*POS*
- [SRN-1109] Added option to reprint 8.5x11 invoice for completed sale.

*Shipping Setup*
- [SRN-945] Merged Carrier Setup and Shipping Markups screens into combined Shipping Setup screen.

### Fixes

*Build Install*
- Remove ReportsSRN.exe from install app path.
- Remove SRNSQLProcessor.exe from install app path.

*Drop Offs*
- [SRN-847] Set focus to Tracking# field after looking up customer in Contact Manager.
- [SRN-1053] Process and Save button should have a check if there are no packages scanned in.
- Fixed: FASC compensation report sometimes will not display
- Fixed: Totals on Drop Off compensation report not adding up correctly
- [SRN-927] Packing Fee should be formatted as dollar amount after entered

*EOD Manifest*
- [SRN-1097] Printed Manifest needs to differentiate between FedEx Air and FedEx Ground.

*Package Valet*
[SRN-815] Letters within a scanned tracking number should always be capitalized.

*POS* 
[SRN-1067] Fixed: Invoice History: Invoice Search does not work.
[SRN-801] Fixed: Open/Close:  Drawer is not opening when selecting "Close Drawer" procedure.
- A sale of $0.00 total should not be allowed to be put on Hold.
- Update Invoice Lookup search to return matched invoice when ENTER key pressed.
- [SRN-1103] Fixed: When using the Recover Previous Package option, POS total does not get updated.
- [SRN-1104] Fixed: Option to print 2 receipts for account transactions not functional.
- Fixed: Customer Search by phone number won't work at the first attempt after a drawer opening.

*POS Open/Close*
- [SRN-1102] Fix issue where Z Report Cash Over and Short value is 0 when Closing Drawer with overage or shortage.

*POS Setup*
- [SRN-618] In Sales Tax Setup, when editing Tax1, Tax2, or Tax3 fields and pressing tab, it will revert back to the original percentage.
- [SRN-1008] Fixed: Sales Tax Setup allows too long of a name to be entered. That causes an error in AR screen when selecting that Tax County.

*Reports*
- Fixed: "Value cannot be null" error when printing Production Report by AR Account.
- Fixed: Account Aging Report column aligment.
- [SRN-989] Fix AR Aging report to load correct data into report.
- Update AR Statement print procedure to run based on the selected Radio Button options.
- [SRN-814] Convert AR Statements VB6 report to .Net report.

*Search Window*
- Only show From/To Date selection for Invoice Lookup.

*Shipment History*
- [SRN-1016] Send ShipandInsure Void web request when deleting a package with insurance applied. 

*Shipping*
- [SRN-1089] FedEx REST API disabled by default until ready for release.
- [SRN-1099] Fixed: Round Option is not applied when surcharges are added in the Process Shipment screen.
- [SRN-945] Fixed: APO/FPO/DPO shipping not functional.
- [SRN-1105] Fix issue where UPS ship web request doesn't send if old registration fields are missing data.
- USPS First Class Intl service not displaying actual discounted cost.
- [SRN-1108] USPS First Class Intl Flats service cost always 1 oz price.
- [SRN-1100] Fixed: FedEx services are not visible if a TimeInTransit request is sent with any signature option enabled.

*Shipping - Print Label*
- [SRN-1107] Fixed: In print label screen, unchecking "certified mail" or "return receipt" will not remove those options.

*Support Utilities*
- Add "Export to CSV" file path textbox for exporting data to .csv file.
- Update "Process SQL" utility to export results to "Export to CSV" file path if specified.
- Update "Normalize Invoice Balances" utility to prompt user for account number to process on single AR account.
- [SRN-1052] Update "Normalize Invoice Balances" utility to fix issues balancing AR account invoices to fix AR Statements data issues.
- [SRN-1052] Add "Set Balances" utility to update Balance field for all Invoices in Payments table.
- [SRN-1052] Update "Normalize Invoice Balances" utility to run "Set Balances" utility after normalizing invoices.

## 1.0.75 (2024-10-25)

### Enhancements

*POS*
- [SRN-1061] Add Print option to the Invoice Search window to print the current view.

*POS Payments*
- [SRN-1071] Added ability to delete payments from payment screen.
- [SRN-880] Added $2 button to Quick Cash entry options.

*Reports*
- [SRN-1061] Add Invoice Search report.

*Search Window*
- Add From/To Date selection.

*Shipment History*
- [SRN-1085] Allow user to change weight and dimensions on Pending shipments.

*Shipping*
- [SRN-359] Integrated FedEx REST API.

*Time Clock*
- [SRN-812] Added option to print timesheets for all employees that had hours.

### Fixes

*Build Install*
- Update SRPro Convert database with 10.24.801 added field.

*Carrier Setup*
- Fix "conversion from string to boolean" errors in some cases when selecting carriers.

*Drop Offs*
- [SRN-1068] Checkboxes for date selection should be removed from the Reports popup.

*EOD Manifest*
- [SRN-886] Package listing should be reloaded after returning from package details.

*Inventory*
- [SRN-1079] Allow the entry of negative sell amounts.

*Mailbox*
- [SRN-1083] Added dynamic mailbox number display sizing to be able to display 5 digit mailbox numbers.

*Package Valet*
- [SRN-853] CheckIn: Set focus to Tracking field after looking up customer in Contact Manager.
- [SRN-852] CheckIn: Set MBX # field as starting cursor/focus field.
- [SRN-1077] Check Out: Search By Tracking Number returns string to integer conversion error if format unrecognized.
- [SRN-505] Package Inventory and Check Out lists need to be loaded when tab is selected, not when form is loaded.

*POS*
- When voiding invoices, the time of the void needs to be recorded.
- [SRN-1069] A refund should be logged in the VOID table.
- Fixed: Error when updating inventory quantity if quantity in inventory for a SKU is blank.
- [SRN-1066] POS: When voiding or refunding a sale, the time of the void should be recorded. Not just the date.

*POS Open/Close*
- [SRN-1076] POS OpenClose Drawer Open boolean field in database is duplicated.

*POS Payments*
- [SRN-1063] Pressing Complete Sale should process a Cash Payment entry just like pressing <ENTER> would.

*Shipping*
- [SRN-1070] Updated UPS web services ship request to ignore "Invalid Date" warning alert in response.
- ShipAndInsure server should not be connected to if declared value is 0.
- [SRN-1011] USPS First Class Markups should not be overwritten after an update.
- [SRN-1084] Endicia: Fix issue where USPS Intl GIF image label doesn't print if destination is Global Processing Facility.

## 1.0.74 (2024-10-04)

### Enhancements

*Shipping*
- Updated USPS rates effective 10/06/2024 to 01/19/2025.

*Shipping Setup*
- [SRN-1031] Added option to round up shipment pricing.

### Fixes

*Build Install*
- [SRN-1041] Change auto updater downloads location to CommonAppDataFolder to fix "access is denied" error when attempting program updates check in some cases.
- [SRN-1039] Remove QBO .upd files with PostNet COA and Depts from install data path.

*Contacts*
- [SRN-1049] Fixed: Editing a Consignee causes Residential status to always be set to False 

*Inventory*
- [SRN-1035] After adding a new inventory item, the fields should be cleared out.

*POS*
- [SRN-1054] The void sale button should have different caption for voiding Hold or Quote Invoices.
- [SRN-1033] Fixed: Cannot void Quote invoices.
- [SRN-1056] Fixed: Declined credit cards can record blank payment records.

*Shipment History*
- [SRN-1040] Fix issue where Print Commercial Invoice button doesn't show for FedEx Ground to Canada (FEDEX-CAN) service.
- [SRN-1029] Display if shipment was insured with ShipAndInsure or Shipsurance.

*Shipping*
- [SRN-1046] Fix issue where USPS Priority Flat Rate packages apply Cubic pricing erroneously.
- [SRN-1030] Fixed: Shipping PickupDate saved in db can be old date which can cause error with Shipsurance upload
- If Declared Value is zero, don't check for third party insurance.

*Shipping Markups*
- [SRN-1047] Fix USPS Priority Flat Rates to display USPS Commercial rates in Cost field instead of discontinued Endicia Prefered rates.

*Shipping Setup*
- [SRN-1036] When saving Third Party Insurance settings, Shipsurance entries should only be verified if Shipsurance is set to "Enabled".

## 1.0.73 (2024-09-10)

### Enhancements

*Carrier Setup*
- [SRN-690] Add UPS REST API Authorization procedure to log into UPS account in default web browser.

*Mailbox*
- [SRN-607] Add Email Bulk Notices functionality.

*Shipping*
- [SRN-690] Update UPS Web Services APIs to Steppingstone Production Endpoints with OAuth Authentication.
- [SRN-1017] Added DHL Demand surcharges effective 09/15/2024 to 12/31/2025
- [SRN-954] Added support for Spee-Dee in Carrier Setup.

*Tickler*
- [SRN-607] Add Email Notices functionality.
- [SRN-608] Add Individual Email Notice functionality.

### Fixes

*Build Install*
- Add SpeeDee_DAS.txt file to DAS folder in data path of install.

*Carrier Setup*
- [SRN-1028] Save entered account number when UPS account successfully authorized/registered.

*Contacts*
- [SRN-1010] Fixed: Class field in Contacts table is not being populated.
- Endicia verification supports only domestic addresses.

*Email Setup*
- [SRN-931] Added option to view email password.

*Mailbox*
- Fixed: Syntax error when saving a mailbox with a cost over $1000.00.
- Fixed: Error may occur when selecting "Expire End Of Month" option.

*Package Valet*
- [SRN-1006] Fixed: Signatures from Topaz signature pad are not being saved.

*POS*
- [SRN-1002] Fixed: Invoice number can be duplicated on two separate invoices.
- [SRN-1015] Fixed: Cannot refund a Mailbox transaction.
- [SRN-1019] Fixed: When editing quantity on receipt, the COGS are not updated.
- If weight display is disabled in POS Receipt Setup, it should not print on receipt.

*POS Refund*
- [SRN-1002] Fixed: Invoice number can be duplicated on 2 different sales.

*Shipment History*
- [SRN-1009] Shipment History: Packages should be ordered by time, not just date. 

*Shipping*
- [SRN-1012] Disabled SRPRO pricing. All Endicia users should be on Commercial Base costs.
- [SRN-1014] DHL: Shipper address should use the Store address.
- [SRN-1022] Shipping FedEx to Canada won't show pricing of letters if the postal code are not capitalized. 
- [SRN-1025] FedEx zone lookup to Canada cannot find the zone for some specific zip codes.
- [SRN-1023] UPS zone lookup to Canada cannot find the zone for some specific zip codes.
- [SRN-1024] UPS International price lookup will not show a price for weights not specifically listed in the chart.
- [SRN-1018] FedEx, UPS Demand (Peak) surcharges aren't included in shipping calculations
- [SRN-1027] Priority Mail International customs label copy can print previous shipment's address.
- Only show "ShipAndInsure is configured ONLY for 'US' shipments" message when 3rd party insurance is ON.

## 1.0.72 (2024-08-06)

### Enhancements

*AR*
- [SRN-348] Added "Print Invoice" button functionality in "Ledger" and "Ledger By Invoice" screens.

*Package Valet*
- [SRN-811] When Checking Out package, the "Picked Up By" drop down box should display additional mailbox names.

*POS*
- Add processing of Kiosk barcode to load Kiosk contact, package info and open Shipping screen.
- [SRN-792] Added Ctrl+N / NoSale option to POS to open the drawer.
- [SRN-924] Reduced empty lines between sections at bottom of receipt

*Shipping*
- On form load, process Kiosk contact, package info to be saved in database and loaded in Shipping screen.
- [SRN-981] Add ShipandInsure third party insurance web request when processing shipment.
- Add Shipsurance third party insurance web request when processing shipment.

*Shipping Setup*
- [SRN-981] Third Party Insurance: Add loading and saving of ShipandInsure settings.
- Third Party Insurance: Add checkboxes to enable/disable ShipandInsure and Shipsurance.

### Fixes

*Drop Offs*
- UPS Commercial invoice errors should not be reported to ShipRite.

*Letter Master*
- [SRN-1000] Fix error due to "ContactSalesData" table missing from Report_Writer database.

*MailMaster*
- [SRN-995] Postage Quantity field should allow 3 digits instead of 2

*Package Valet*
- [SRN-993] POD Report by Carrier doesn't work for FedEx
- [SRN-994] Fixed: Package History by Cust report includes all customers when date range selected

*POS*
- [SRN-992] The "Change Price" button should be applied only once, and not on subsequent items.
- Fixed: Printed Quote has "Quote" caption cut off.
- Fix conversion error if POS search returns 0 customer matches

*Reports*
- [SRN-1001] Fix issue where Sales Tax, Production by Department, Departmental Chargeback reports point to old SR data sources.
- [SRN-998] Update Z-Report Collection Summary to fix issues with values displayed for Charge, Changes In AR, Cash Over/Short, and Cash Paid Out.

*Shipping*
- [SRN-897] Removed unused Dim Weight field shown in top-right of screen
- [SRN-968] Updated Display to show information labels when AH, OVS, or DAS is applied.

*Shipping - Print Label*
- [SRN-791] USPS Shipping needs to have an option to print the customers address as the shipper address on the label.

*Startup*
- [SRN-984] Remove "Note" transactions records older than a year no longer needed to reduce database size weekly.
- [SRN-779] After converting from old shiprite, repair and compact the new database.

*Support Utilities*
- [SRN-988] Update Normalize Invoice Balances utility to fix issue where duplicate statements are created for the same accounts.

## 1.0.71 (2024-07-15)

### Enhancements

*Shipping*
- Updated USPS rates effective 07/14/2024.

### Fixes

*Build Install*
- Update default ShipriteNext.accdb, Update.accdb database files with latest schema changes.

*Reports*
- [SRN-894] Fix issue where Shipping reports aren't showing a Cost value.

*Shipping*
- [SRN-898] Endicia USPS: Don't return First Class Flat Envelope ID# which isn't valid tracking number.

*Startup*
- [SRN-991] Fix text field data types in Setup table to fix errors when running EOD Manifest reports.

## 1.0.70 (2024-07-03)

### Enhancements

*Drop Offs*
- [SRN-726] Add FedEx FASC Compensation report.
- [SRN-726] Add UPS Compensation report.

*Letter Master*
- [SRN-12] Letter Master: Add functionality.

*Reports*
- [SRN-974] Add Inventory Price Labels interface and printing.
- POS: Add functionality to "Cash Paid Out" report.
- POS: Update "Houly Sales Ticket" report.
- POS: Update "Hourly Analysis for a Week" report.
- Shipping: Add "By Carrier Summary" report.
- Inventory: Add "Inventory Label" report and Price Labels screen/reporting.
- [SRN-975] Other: Add "Customer Address Listing" report interface and printing.

*Shipping*
- [SRN-952] Added support for customized rates.
- [SRN-954] Added SpeeDee price lookup.
- Updated Endicia discounted USPS rates effective 07/01/2024.

### Fixes

*Build Install*
- Update default Drop Off Receipt Disclaimer text to fix spelling errors.
- Update default Report_Writer database.

*Carrier Setup*
- [SRN-976] Fix UPS Registration authentication errors due to request not using entered info on screen.

*Contacts*
- [SRN-904] When duplicating contact don't return to previous page, stay in contact manager.
- Fixed: Sometimes the Name field can be populated with the ContactID instead of the customer name.
- [SRN-843] Search type settings should be saved locally.
- Don't display same mailbox number twice in mailbox field

*Drop Offs*
- [SRN-925] Fix issue where compensation values not saved with packages after opening Setup Options popup.
- Connection errors when uploading UPS Drop Offs should not be reported to SR.

*Mailbox*
- [SRN-799] Separate PS1583 forms need to print for each additional name tied to a mailbox.
- [SRN-965] Fix syntax error when saving if names contain apostrophe.
- [SRN-963] Fixed: Print 1583 entry fields tab order is incorrect
- [SRN-964] Fixed issue where a commercial contact can show twice in additional names field.

*Mailbox Notifications*
- [SRN-935] The "SMS" option should be sticky.
- [SRN-928] After a notification is sent, the list of additional names should be cleared.

*Mailbox Setup*
- [SRN-958] Fixed: The screen lags after pressing the Save button.
- [SRN-959] Fixed: Can't add new lines to MBX Contract
- [SRN-960] Fixed: The apostrophes in the MBX Contract are doubled when saving

*MailMaster*
- [SRN-939] Adding postage with entered weight doesn't include "Letter" or "Flat" description.

*POS*
- [SRN-932] POS Receipt header needs to print 2nd address line of the store address.
- [SRN-933] Fixed: DrawerID display always shows 01 in POS.
- Fix "Extra ) in query expression" error when selecting Invoice History when Customer is selected in POS.
- [SRN-962] SKU Input quick Search screen doesn't process new searches after opened.
- Fixed: Level Pricing not working if editing a line item.
- [SRN-930] Printed Shipping Disclaimer text should auto wrap text.
- [SRN-936] Fixed: Multi-line Memos are not printed neatly on POS Receipt.
- [SRN-957] Fixed: When re-sending email from Recovered Invoice menu, not notification displays if email went out.
- [SRN-969] Fixed: Shipment consignee FirstName, LastName not always included on receipt
- [SRN-982] Fixed: POS Buttons - "POS Discount" doesn't apply discount

*POS Button Maker*
- [SRN-951] Fixed: POS Button Groups drop down includes old "Standard" POS Style Groups
- [SRN-953] PosButtons: Renaming a Group should re-associate Skus with the new Group name.

*POS Open/Close*
- Fixed: Conversion from string " " to type 'Integer' is not valid error.

*POS Setup*
- [SRN-929] Receipt Options: Allow the ENTER key when editing the text in the Receipt Signature field.

*Reports*
- [SRN-943] Consolidated Z report: Selection of CloseID's needs to allow multi select without holding Ctrl or Shift buttons.
- [SRN-814] Convert VB6 reports to .Net reports (AR Statements still in progress).

*Shipping*
- [SRN-850] Fixed "Disable Service" option in "Shipping Markups and Settings"
- [SRN-946] Updated DHL Carrier Setup Discount tiers
- [SRN-950] Fixed: FedEx Ground and Canada Ground don't show pricing if dimensional weight is over 150lb.
- [SRN-942] Fixed: Shipment Published Cost is saved to History instead of Discounted Cost
- FedEx Home Delivery maximum is 150 lbs instead of 70 lbs
- Auto Time in Transit should make sure that weight and dimensions are entered before sending request.
- Custom Rate path should not be case sensitive.
- [SRN-929] FedEx ONE Rate pricing should not include fuel, residential and delivery area surcharges
- [SRN-978] Fixed: If Retail Rates are enabled, FedEx Ground shipments with DIM weight over 150lb show retail shipping price of $0.00
- [SRN-979] Fixed: If Retail Rates are enabled, FedEx ONE rate still uses regular LEVEL1,2,3 markups.
- [SRN-977] Fixed: Missing UPS zone file causes other carrier's zones not to get loaded.

*Tickler*
- [SRN-941] Tickler notification should not show for future tickler items.

## 1.0.69 (2024-05-14)

### Enhancements

*Shipping*
- Updated FedEx One Rate rates effective 04/15/2024.

### Fixes

*Build Install*
- [SRN-862] Update setup installer to never overwrite ShipriteNext.ini file.

*Carrier Setup*
- [SRN-854] Fix issue where carrier icons are mapped to the wrong carrier settings panels.

*Contacts*
- Reset cell carrier selection when looking up new contact.
- Clear selected contact data when screen cleared with Refresh button.
- [SRN-901] Empty Country Selection causes "Object variable not set" error.

*Drop Offs*
- [SRN-919] Customer Lookup can open with "Success" in the Name field.
- [SRN-920] Customer Name previously looked up isn't cleared from screen.
- [SRN-865] Selecting a carrier manually should uncheck auto detect.
- [SRN-911] Remove "No Email Sent" popup message when selecting "No" to sending email.

*Email Setup*
- [SRN-866] Update Save Email button to be enabled if only Email Notification Subject is changed.
- [SRN-867] Fix syntax error when saving Email Notification Subject containing an apostrophe.

*EOD*
- [SRN-885] Add ability to open package details by double-clicking package in list

*Inventory*
- [SRN-906] POS Departments in Inventory drop down should be listed alphabetically.
- Weight per unit textbox needs to be expanded to fit larger digits.

*Mailbox Setup*
- [SRN-837] Mailbox Setup monthly should be recalculated when changing panel.

*MailMaster*
- [SRN-735] Fix issue where Letter weight isn't rounded up.
- [SRN-890] Fix USPS Endicia First Class Intl Letter/Flat "Invalid MailpieceShape" error.
- [SRN-921] 1st class package has been eliminated, but it's still trying to calculate rates and causing errors.
- [SRN-887] When adding postage with entered weight include weight in line item description.

*PackMaster*
- [SRN-830] Fix "duplicate values in the index" error when opening PackMaster.

*POS*
- [SRN-893] Completing sale can result in payment record with negative balance.
- [SRN-884] Fix issue where shipment cost isn't saved with transaction line item.
- [SRN-900] Phone quick search doesn't find valid phone number if extra spaces included.
- [SRN-910] Inventory PackMaster Items on transaction are saved without Department value.
- [SRN-912] Inventory items pulled up from Hold/Quote are saved without Department value.
- [SRN-878] Address Update should work for customer when Recovered Invoice is pulled up.
- When displaying a recovered invoice, the reference ID and Approval Number will now be displayed for a credit card sale.
- [SRN-917] Line Edit Description of MailMaster and Mailbox items saves with "0.00" sell price.

*POS Open/Close*
- [SRN-870] Drawer Open/Close Receipt Slip not using set font size in printer settings.
- [SRN-869] Update closing drawer count to fix issue where drawer not showing "in balance" when it should.

*POS Payments*
- [SRN-868] Email Receipt: Fix ArgumentNullException error when selecting both Email Receipt and No Receipt options.

*POS Refund*
- [SRN-806] Update CC refund procedure to send API request directly instead of through Genius.
- [SRN-860] Fix issue where CC Vault sale can't be refunded.

*Reports*
- [SRN-827] Updated Mailbox Reports to not show comma for mailbox numbers that have more then 4 digits.
- [SRN-894] Fix issue where Shipping reports aren't showing a Cost value.
- [SRN-879] Fix issue where Production Reports still print when Cancel is selected in the Print Preview.

*Setup*
- [SRN-861] Enable Customer Display should be a local setting for that computer, not a global one.

*Shipping*
- [SRN-824] Fixed FedEx "FTR Exemption" error when shipping to certain international locations.
- [SRN-896] PackageCount and ShippingVolume fields in contacts table should be updated after every shipment.
- [SRN-845] Shipping screen needs to display Addr1/Addr2 lines in the Ship From/To boxes
- [SRN-849] If delivery date is not available it should not be displayed at all.
- When clearing/reseting ship screen, the country needs to be defaulted back to "United States".
- When selecting a international address, the country dropdown should make the selection by object, not by index.
- [SRN-922] Endicia USPS Priority Express label includes "Signature Required" when not requested.

*Shipping Markups*
- [SRN-838] Update First Class Mail setup to show Endicia Letter cost when Endicia enabled.

*Startup*
- [SRN-855] Fix issue where UPS Access Point ID lost with conversion from old SR.

*Support Utilities*
- [SRN-350] Update Normalize Invoice Balances utility to show current account number being processed.
- [SRN-350] Update Normalize Invoice Balances utility to round value to 2 decimal places.

## 1.0.68 (2024-04-23)

### Enhancements

*Shipping*
- Updated DHL Elevated Risk countries list.
- Updated UPS DAS zip codes effective 04/08/2024.
- Updated FedEx DAS zip codes effective 04/15/2024.
- Added future date shipping
- [SRN-819] Added Saturday Pickup option to SHIP screen.

### Fixes

*Carrier Setup*
- [SRN-817] Fix issue where only carriers of completed shipments show in carrier setup.

*Debug - Error Tracking*
- Include error message with stack trace in Error file and email.

*Drop Offs*
- [SRN-825] Fixed: FedEx Air and Ground Manifests always print blank

*POS*
- [SRN-818] Fixed: Duplicate values in database error can occur when completing sale
- [SRN-821] Discount option for AR users needs to carry over to POS.

*Shipping*
- [SRN-822] FedEx Freight should not have dimensional restrictions like regular shipping.
- [SRN-823] Fixed: Inside Delivery and Inside Pickup for Freight not functional
- [SRN-820] Same As Last option in SHIP screen should not pull up address from another workstation.
- Added 3rd address line

*Shipping Markups*
- [SRN-816] Fixed "No Value given for one or more required paramaters" error when saving accessorial charges.

## 1.0.67 (2024-04-05)

### Enhancements

*AR*
- [SRN-355] Add AR CC Vault setup options functionality.

*Main Window*
- Added Package Valet/Drop Off shortcut handlers to Main screen.

*Package Valet*
- [SRN-805] Updated printed notices and labels.

*POS*
- [SRN-798] Added option to email receipt for a completed sale.

*POS Payments*
- [SRN-355] Add AR CC Vault "Use Card on File" functionality.
- [SRN-807] Add CC auth, reference codes to receipt and database.

### Fixes

*Build Install*
- Update MS Access Db Engine prereq URL to our support site.
- Update SmartSwiperExpress.exe application to v9.0.1930 with processing changes.
- Update Report_Writer database, Zreport report files.

*Mailbox Setup*
- [SRN-789] Fixed: In Mailbox Setup, Panel description textbox will not allow letters to be entered.
- [SRN-790] Fixed: If a customer has more than 10 mailbox panels an error in Mailbox Setup can cause those not to be displayed.

*PackMaster*
- [SRN-803] When pressing SELECT, packmaster should select that fragility level, instead of the level selected by radio button.

*POS*
- [SRN-802] Fixed: User LogIn can be circumvented by closing the log in window from the windows Task Bar. 

*POS Payments*
- [SRN-797] In Payment screen, applying a ROA payment needs a check to verify that payment has been applied before completing sale. 

*POS Refund*
- [SRN-795] Fixed: SmartSwiper won't refund credit card due to the original sale invoice number being blank in request.
- [SRN-796] A refund should not attempt to process through SmartSwiper if SmartSwiper is disabled.

*Reports*
- [SRN-787] Update production report procedures to increase speed of compiling report data.

*Setup*
- [SRN-788] Fixed: A missing "Ads" folder crashes SRN when opening Customer Display Setup.

## 1.0.66 (2024-03-18)

### Enhancements

*Contacts*
- [SRN-751] Changed "Add New Contact" icon to make it more obvious.

*POS*
- [SRN-729] Added option to recover previous packages in POS.
- [SRN-771] Added ability to track shipments from POS.

### Fixes

*Build Install*
- Update default ShipriteNext.accdb database file with latest schema changes.
- Update default ShipriteNext.accdb database file with latest Shipping Holiday dates.
- Update default ShipriteNext.accdb database file to fix USPS-GND-ADV panel position.

*Contacts*
- [SRN-775] Fixed: Entering some zips will pull up international city/state combinations. (Example zip: 95990)
- Increased max digits for phone number from 12 to 20 to accomodate international phone numbers.
- [SRN-778] Address Autocomplete should only work if 4 or more characters are entered into the address field.

*Forms*
- [SRN-782] Removed Click Sounds when opening screens.

*Mailbox*
- Start Field should be editable.

*POS*
- [SRN-767] POS: When creating a Drop Off Manager button in button panel, it won't open Drop Off Manager.

*Reports*
- [SRN-787] Fix Production Reports processing slowness by excluding unnecessary "NOTE" transaction lines.

*Shipping*
- [SRN-764] Fixed: In some occasions UPS international pricing is not displayed due to button panel positioning conflict.
- [SRN-768] Fixed: Letter pricing not working.
- [SRN-769] Fixed: Carrier Packaging will show ground pricing if dimensions are entered.
- [SRN-728] Fix issue where Commercial Invoice doesn't print Shipper/Consignee info in some cases.
- [SRN-776] Packmaster Packing charges need to be cleared out for next shipment
- [SRN-785] Update PackageID values used to fix issue where shipments can reuse existing PackageID and fail.
- Fixed: Shipping to Puerto Rico not showing rates for FedEx, UPS, and USPS.

*Shipping Markups*
- [SRN-766] Fixed: Global Update in Shipping Markups won't work for RETAIL rate markup.
- [SRN-777] When saving markups, there should only be one confirmation message.

*Shipping Setup*
- Shipping Holidays should be sorted oldest to newest.
  
*Startup*
- [SRN-760] Add special update to create indexes in Payments, Transactions, Contacts, AR, Manifest, Mailbox tables in shipritenext database.
- Add special update to create SKU field index in Inventory table and add Shipping SKUs to Inventory.

*Support Utilities*
- [SRN-350] Add Normalize Invoice Balances utility to balance AR account invoices.

## 1.0.65 (2024-02-26)

### Enhancements

*AR*
- [SRN-350] Add AR statement printing.

### Fixes

*Build Install*
- Update setup installer to always overwrite Report_Writer.mdb application file.
- [SRN-762] Update setup installer to detect and attempt to close running applications to be updated.
- Update SmartSwiperExpress.exe application to v9.0.1928 with processing changes.

*Forms*
- [SRN-733] Fix issue where quickly clicking the scroll bar of a List results in a double-click event on the selected item in the list.

*Reports*
- [SRN-707] Fix VB6 reports to show store info in header instead of "SmartTouch Software".

## 1.0.64 (2024-02-15)

### Enhancements

*AR*
- [SRN-347] Added invoice adjustment option to AR Ledger screen.

*Build Install*
- Upgrade setup installer to Advanced Installer.
- [SRN-643] Add Advanced Installer auto updater to check for program updates.

*Contacts*
- [SRN-744] Quick Search drop down should display the mailbox number for all names associated with a mailbox.

*MailMaster*
- [SRN-407] Added First Class International service for Letters and Flats.

*Main Window*
- [SRN-643] Add options under top menu item File > Program Updates to configure and check for program updates.
- [SRN-643] Add check for program updates at startup after main window loaded.

*POS*
- [SRN-737] Added option to print the shipping disclaimer on a 2nd receipt.

*Shipping*
- [SRN-736] Added 2nd link for harmonized code lookup in customs entry screen.
- [SRN-740] In SHIP screen pressing F8 should open the "List all ShipTo Addresses for Shipper" option.
- Updated FedEx Demand (Peak) surcharges effective 02/05/2024.

*Support Utilities*
- Added Process Update.accdb procedure to copy data from current database to blank default database to fix issues such as missing indexes.

### Fixes

*Build Install*
- [SRN-718] Move MS Access Database Engine to installer prerequisite to fix issue where it isn't automatically installed in some cases.
- Update default ShipriteNext.accdb database file with 2024 USPS surcharge rates.
- [SRN-742] Update setup installer to always overwrite standardized application files.
- Fix issue where table indexes dropped when converting from old shiprite to new shipritenext database.
- Update default ShipriteNext.accdb database file with latest schema changes.

*Forms*
- [SRN-733] Fix issue where quickly clicking the scroll bar of a List results in a double-click event on the selected item in the list.

*MailMaster*
- [SRN-734] Fix issue where Letter Stamps are using wrong discounted cost if Endicia enabled.
- [SRN-735] Fix issue where Letter weight isn't rounded up.

*Main Window*
- Fix version shown in main screen title bar.

*POS*
- [SRN-732] Fixed: Receipt Options for Shipment Info printing on receipt not functioning.
- [SRN-748] Fixed: ShipRite can randomly shut down once a second shipment is added to POS receipt.
- [SRN-749] Invoice Lookup should display "CreditCard" instead of "Charge" in the Type column.
- [SRN-752] Fixed: When a shipment is recorded in POS, no POS department is assigned.

*POS Payments*
- [SRN-741] When selecting CC payment, the focus should be on the credit card entry fields.

*Reports*
- [SRN-754] When selecting the Statement option from the AR screen, the reports screen will show blank.
- [SRN-747] Fixed: Datewise Z report query.
- [SRN-753] When a report is being generated, a hourglass should display to let the user know that it's being worked on.

*Shipment History*
- [SRN-713] Invoice#, Sales Clerk, and DrawerID need to be populated for shipment.
- [SRN-758] Fixed: Cannot Reprint UPS labels

*Shipping*
- [SRN-745] After processing an international USPS shipment, subsequent USPS shipments get a Invalid MailClass error
- [SRN-755] FedEx 10kg and 25kg need to use their package specific pricing.

*Startup*
- Fix issue where error occurs if old shiprite.ini file doesn't exist when checking to create Policy tables.

*User Setup*
- [SRN-689] When selecting a "master" checkbox, the sub-checkboxes should automatically be all checked.

## 1.0.63 (2024-01-24)

### Enhancements

*Build Install*
- [SRN-723] Update default Reports.accdb with Policy table.

*Shipping*
- Updated USPS rates effective 01/21/2024.
- Updated Endicia ShipRite Preferred discounted rates effective 01/21/2024.
- Updated FedEx Peak surcharges effective 01/15/2024.
- Updated UPS Peak surcharges effective 01/14/2024.
- [SRN-699] Added USPS Non-Standard Fees.

*Startup*
- [SRN-723] Convert local settings in Reports.accdb from Setup to Policy table.

### Fixes

*Build Install*
- [SRN-691] The FedEx option "Print 8.5x11 Shipping Labels" in Carrier Setup should be unselected by default after install.
- Update default ShipriteNext.accdb database file to clear data and with 2024 surcharges.
- [SRN-720] Update default Reports.accdb with missing Scale Weight Limit field.

*Contacts*
- [SRN-688] Contact Manager: Google Address Lookup should only be done for ShipTo addresses.
- [SRN-722] International phone numbers should not be formatted.
- [SRN-687] Fixed: Cannot enter a company name with all Caps.

*Customer Display*
- [SRN-727] A missing "Ads" folder throws an error when customer display is enabled.

*Drop Offs*
- [SRN-730] Tabing out of the name field should put focus on the tracking number field

*Email Setup*
- [SRN-711] Fix issue where email settings not saving to Policy if field doesn't exist.

*Main Window*
- [SRN-706] Update store name in main screen title bar after Registration Key imported.

*Package Valet*
- [SRN-725] Fixed: "Syntax Error" when opening package valet if the CustomerID field has a NULL value.

*POS*
- [SRN-709] POS Invoice History Number of days to show should default to 90 instead of 1.
- [SRN-715] Total sale amount can be wrong when pulling sale from Invoice Lookup or Hold due to Sales Tax rounding.
- [SRN-491] Fixed: Cannot Delete an invoice from Hold.
- [SRN-693] Customer Name not printed on receipt.
- [SRN-704] Fixed error when ringing up an item, but no default Tax County is setup.
- [SRN-721] Fixed: Sales can be processed if the drawer is not Opened.
- [SRN-731] Fixed: Applying discount on a shipment in POS, calculates the price wrongly.

*POS Button Maker*
- [SRN-716] POS Buttons: Search Inventory can error if there are items with Sell = NULL

*POS Open/Close*
- [SRN-719] Open Drawer screen should not display that "Drawer is in Balance."

*POS Setup*
- [SRN-705] Error occurs when adding Tax County without all fields entered.

*Reports*
- [SRN-680] Fixed: Reports date range not applied sometimes.
- [SRN-708] Fix issue where Period date range in Footer of Sales Journals and Production Reports doesn't show selected dates of report.

*Shipment History*
- [SRN-703] Fixed URL for tracking shipments for DHL and USPS.

*Shipping*
- [SRN-686] Fixed: Endicia Shipping requests are not including Certified Mail or Return Receipt Tags.
- [SRN-694] Fixed: FedEx shipping a 0.15 lb package rounds down weight to 0 causing error.
- [SRN-696] Fixed: in Master table, UPS canadian services are displaying domestic accessorial revenue items.
- [SRN-698] Fixed: When editing the ShipFrom address, once the contact manager is closed, it will load the ShipTo address into the From section.
- [SRN-695] Added UPS and FedEx domestic Remote Area Surcharge.
- [SRN-700] Remove USPS Retail Ground and Parcel Select services if present.
- [SRN-697] Fixed: DHL DOC service name does not fit onto the shipping button.
- [SRN-724] Fixed: FedEx One Rate errors with "Invalid Special Service Type".
- [SRN-701] USPS Flat Rate Costs are not updated and need to be synced.

*Shipping - Print Label*
- [SRN-712] Endicia: Added the ability to specify amount of postage to be purchased, instead of defaulting to $10.00.

## 1.0.62 (2024-01-08)

### Enhancements

*Shipping*
- [SRN-668] Added option for "ForcePOSShipping" to Ship Setup and check when opening Shipping screen from Main window.

### Fixes

*Contacts*
- [SRN-678] Entering zipcode not autofilling city, state fields.
- [SRN-679] Enable Address Autocomplete setting doesn't save.
- [SRN-684] Fixed: When doing any search in customer lookup, it's not possible to enter a new customer anymore.

*POS*
- [SRN-669] User should have a choice to print open/close slip.

*POS Button Maker*
- [SRN-676] Fixed: Blank quantity not saved in POS Button Maker

*POS Payments*
- [SRN-681] Credit Card Payment popup should prefill the customer name in "name on card".
- [SRN-686] Fixed: Payments: Pressing Credit Card multiple times causes multiple CC lines on receipt and Change Due

*Shipping*
- [SRN-673] Fixed: USPS Endicia International Flat shipment errors with "Invalid mailpiece shape (Flat)" error.
- [SRN-674] Fixed: FedEx Ground FASC discounts don't show for packages over 25 lbs.
- [SRN-675] Fixed: UPS Ground ASO discounts don't show for packages over 25 lbs.
- [SRN-666] When selecting a residential address, the "Residential" button should be selected.
- [SRN-683] Fixed: When selecting ShipLetter in POS, it does not preselect the "Letter" option in Ship screen.

## 1.0.61 (2023-12-27)

### Enhancements

*Carrier Setup*
- Updated FASC Discount Tier option with new tiers '1 ($0-$40,999.99)', '2 ($41,000-)' in FedEx Carrier Setup.

*Mailbox*
- [SRN-651] Added Email field to Mailbox Listing and Alpha Listing reports.
- Fixed error when reprinting Mailbox Contract

*Main Window*
- [SRN-655] Add store name to main screen and POS.

*POS*
- [SRN-655] Add store name to main screen and POS.

*POS Open/Close*
- [SRN-658] When opening or closing screen is displayed, focus should be on the Penny count textbox
- [SRN-659] When entering in coin/bill counts, the ENTER key should move focus to the next field (identical to the TAB key.)
- [SRN-663] OTHER sales should be listed when closing the drawer.

*Shipment History*
- [SRN-132] Reprint Manifest function added.

*Shipping*
- Updated FedEx FASC discounts for Independent locations effective 11/16/2023.
- Updated UPS rates effective 12/26/2023.
- Updated UPS DAS zip codes effective 12/26/2023.
- Updated UPS Additional Handling surcharge increase if domestic package based on weight effective 12/26/2023.
- Updated UPS Large Package surcharge increase if residential package effective 12/26/2023.
- Updated UPS Additional Handling surcharge to apply if Intl package weighing 55 lb. or more effective 12/26/2023.
- Updated DHL rates effective 01/01/2024.
- Updated DHL Elevated Risk countries list.
- Updated FedEx rates effective 01/01/2024.
- Updated FedEx DAS zip codes effective 01/01/2024.
- Updated FedEx Additional Handling surcharge increase if package based on weight effective 01/01/2024.
- Updated FedEx Oversize Package surcharge increase if Ground Home Delivery package effective 01/01/2024.
- Updated SpeeDee rates effective 01/01/2024.

### Fixes

*Build Install*
- [SRN-542] Update install settings for shared VB6 .dll files to prevent issues with SRPro.

*Mailbox*
- Fix misspelling of "Nondriver" in Print USPS 1583 Form Photo ID, Address ID drop down selections.

*POS*
- [SRN-593] In the Hold lookup screen, the Balance Due should not be $0.00.
- In Hold/Quote Lookup screen, when selecting a customer it should only display the Hold/Quote invoices.
- [SRN-647] Invoices that have been closed out, should not be allowed to be voided.
- [SRN-646] After selecting a POS line item, the Change Price button should open the line editor for the first POS line.
- [SRN-639] Quick Cash buttons should have the ability to be clicked on multiple times and the total should be added up.
- [SRN-660] When reprinting an invoice using the Cash Out button, the POS should be cleared and a new sale started once invoice reprinted.
- [SRN-661] Log In screen should always be on top of the SRN application only, not on top of other programs and windows.
- [SRN-662] Receipt print and cash drawer opening should be BEFORE the Change Due screen is displayed.
- [SRN-664] Changing quantity on a non-taxable item in POS should not make it taxable.
- [SRN-664] Fixed: Inventory option for "Set Item to Non-Taxable" not functional.
- [SRN-667] If "No Discounting" is selected in inventory, then then no discount should be applied for that item.

*Reports*
- [SRN-656] Report Manager should check user permission before opening.

*Shipment History*
- [SRN-652] When not able to void a shipment, a pop up message should ask to delete it anyway.

*Shipping - Print Label*
- [SRN-590] In print label screen, inside pickup and delivery should only be displayed for Freight shipments.

*Startup*
- Fix Registration .key file processing issue where registration phone, fax fields not updated locally which can cause "Key is Invalid" error.
- Updated FedEx, UPS domestic zone file caching to fix issue where some zip code ranges not cached and don't show rates.

## 1.0.60 (2023-11-10)

### Enhancements

*EOD Manifest*
- [SRN-256] Printing of manifests added.

*Mailbox*
- [SRN-613] Added new USPS PS1583 Form.

*Main Window*
- Added program feature access checks based on Registration Key access codes.

*Startup*
- Added Registration Key check and .key file processing.

### Fixes

*Backup*
- [SRN-632] Zip file created by the backup utility is corrupted and cannot be read.

*Build Install*
- [SRN-645] Move default Data Path from C:\Network\ShipriteNext\Data to C:\ShipriteNext\Data.
- [SRN-645] Move global data files in App Path to Data Path.
- Update VB6 .exe files with code signing certificate.

*Mailbox*
- [SRN-644] Mailboxes converted from old SR, whose mailbox numbers are not part of any existing mailbox size/group, should be omitted during loading to prevent errors. 
- [SRN-640] Need to be able to double click on the customer textbox to open contact manager.

*Main Window*
- [SRN-635] Updated Twitter Logo to X

*PackMaster*
- [SRN-373] Need to hide tabs for reviewer and fragility levels.
- [SRN-362] Fixed Setup Button in top menu.
- [SRN-365] Fixed: Packmaster is adding packing info to receipt regardless if shipment was processed or not.

*POS*
- [SRN-123] Fixed POS Quote printing.

*Scale*
- [SRN-650] Fix issue where Mettler Toledo USB scales return "Failed to locate HID device" error.

*Shipping*
- [SRN-641] If country other than US is selected, then allow letters to be entered into the ZipCode field.

*Startup*
- [SRN-636] Check SR database and add missing shipping services to DB before conversion.

## 1.0.59 (2023-10-26)

### Enhancements

*Carrier Setup*
- [SRN-630] Added an option for Endicia to print either EPL, ZPL, or PDF labels.

### Fixes

*Build Install*
- Updated to set ShipritePackaging.mdb as permanent file to prevent file missing after update.

*Carrier Setup*
- Updated UPS Registration request to process with provided credentials and access license retrieved.

*Contacts*
- [SRN-628] When entering an international address, the software should not do US city/state lookup once the zip code is entered.

*Customer Display*
- [SRN-637] When returning from shipping to POS screen, the customer display should return to POS and not remain on the SHIP.

*Inventory*
- [SRN-633] When adding a new inventory item, the confirmation popup message is hidden behind the popup.

*Mailbox*
- [SRN-631] When renting a mailbox through POS, fixed "Cannot Rent Mailbox, Please enter a customer first" error.

*POS*
- [SRN-634] Line Editor Popup should be closed when completing or cancelling a sale.
- [SRN-629] Invoice Lookup: When entering a 6 digit number into the "Days to Show" field a crash occurs and Invoice lookup cannot be accessed anymore.

## 1.0.58 (2023-10-20)

### Enhancements

*Build Install*
- Updated FedEx Demand (Peak) surcharges effective 10/02/2023-01/14/2024.
- Updated UPS Demand (Peak) surcharges effective 10/01/2023-01/13/2024.
- Updated UPS ASO incentives for Intl services effective 10/16/2023.

*Drop Offs*
- Updated UPS Drop Off API credentials effective 10/14/2023.
- Updated DHL Drop Off API request URL.

*Mailbox*
- [SRN-605] Print Individual Notices for a Mailbox.
- [SRN-606] Print Bulk Notices for Mailboxes.

*POS*
- Added ROA payment button in the AR popup menu.

### Fixes

*AR*
- Updated Ledger By Invoice List column headers "InvAmt" to "Charges" and "InvPayM" to "Credits".

*Build Install*
- Updated default ShipriteNext.accdb database file with current SpecialUpdatesVersion.
- Added missing prerequisite for SAP Crystal Reports Runtime Engine for .NET Framework to Release configuration.

*Mailbox*
- [SRN-614] Mailbox Rental Agreement doesn't print the contract correctly.
- The Mailbox Notices button has the wrong tooltip displayed.
- Fixed problem where a Box could be rented out without selecting a customer.
- Selecting the mailbox customer should be done through the Contact Manager.
- Mailbox Notices screen changed from a popup to it's own window.

*POS*
- Added Apply Credits to Account functionality.
- Updated "Cash Out" button to reprint receipt for Recovered Invoice with invoice balance of 0.
- Updated to fix issues with the Change Quantity functionality.
- [SRN-611] Added Email Receipt functionality.
- [SRN-615] Receipt Signature needs to wrap text automatically to next line.
- [SRN-622] SalesTax should not be calculated from the Policy table, which can have a wrong tax amount.
- [SRN-623] Bulk Payment balance does not take into account Account Credits from other invoices
- [SRN-624] Bulk Payment should be accesseible when pulling up and account pressing "Cash Out" in POS
- [SRN-482] In Invoice Lookup, the refund invoice totals is off.
- [SRN-481] Invoice Lookup needs to display if a sale was a cash, check, or credit card sale; or a refund.

*POS Open/Close*
- [SRN-399] Clicking the Total column should add one roll of coins. Clicking the denomination button should add quantity of 1.
- [SRN-620] A blank entry into a coin or bill field can cause a conversion error.
- [SRN-621] The coin and bill textboxes should only allow numbers to be entered.

*POS Payments*
- Updated to fix issue where enabling the "Remote Control" setting in SmartSwiperExpress causes SRPro SmartSwiper to stop working.
- Updated Apply Credit popup to load invoices with credit balances only.
- When a customer has an email entered, the Email option in the payment screen should automatically be selected.
- [SRN-603] Removed Home button from Payment screen. User should go back to POS and cancel sale there.
- Added buttons for emailing customer review and emailing 8.5x11 invoice.

*Reports*
- Added separate Acct Name and Number fields and searching.
- Added Acct Statement Begin and End Date selection.

*Scale*
- Use default weight limit of 150 lb. when weight limit not set.

*Scale Setup*
- Load saved selected scale settings instead of default settings.

*Shipping*
- [SRN-626] Customs screen not cleared out once a shipment is completed.
- [SRN-627] Commercial Invoice is displaying all customs items from previous shipments.

*Shipping - Print Label*
- [SRN-589] F5 should open the drop off manager in the print label screen.

*SmartSwiper*
- Updated SmartSwiperExpress.exe application to v9.0.1926 with processing changes.

*Startup*
- Updated FedEx, UPS domestic zone file caching to fix issue where some zip code ranges not cached and don't show rates.

## 1.0.57 (2023-09-27)

### Fixes

*Common Window*
- [SRN-587] Fixed issue where SRN creates multiple instances of itself on the taskbar.

*Inventory*
- [SRN-600] When quickly clicking on the inventory scroll bar, the program should not register a double click event and open the detail screen.

*Mailbox*
- The Mailbox Notices button has the wrong tooltip displayed.

*POS*
- [SRN-585] When pressing the Refresh button to clear a Recovered Invoice, don't display pop up asking to clear pending invoice.
- Fixed issue where changing the quantity on the receipt won't update the price.
- [SRN-597] Fixed issue where SRN cannot reprint a receipt that has a balance due.
- [SRN-595] Memo lines need to be automatically wrapped, so that they don't print off the receipt.

*POS Payments*
- In POS payments screen, added display list of payments when "Apply Credit" is pressed.
- [SRN-596] Payment screen should not let you complete a cash/non-AR sale without full payment.

*Shipment History*
- [SRN-594] Fixed link for tracking FedEx shipments.

*Shipping - Print Label*
- [SRN-588] In Print Label screen, the shipment total should be updated when making accessorial selection changes.

*Shipping Markups*
- [SRN-598] First Class Markup tab shows a "No value given for one or more required parameters" error.

## 1.0.56 (2023-09-20)

### Fixes

*AR*
- [SRN-586] A clerk without AR access rights should not be able to create an AR account through the POS.

*Mailbox*
- [SRN-580] Forwarding address was not able to be selected.

*POS*
- [SRN-585] When clearing a Recovered Invoice from POS, the pop up asking if you are sure to clear a pending invoice should not display.
- [SRN-584] If a AR account is pulled up, The "Complete Sale" button should display the option to Charge Sale to Account.
- [SRN-583] Mailbox rental/renwal details in POS should not be able to be edited in the line editor.
- [SRN-582] In the Hold lookup screen, the Sale total should not be always $0.00.

*Shipping*
- [SRN-579] Show Package Details screen should be positioned so that the addresses, weight and dimensions are still visible.

*Startup*
- Fixed issue with Tickler and inventory items with apostrophes during DB conversion from old SR.

## 1.0.55 (2023-09-13)

### Enhancements

*Build Install*
- Updated setup installer and application file with code signing certificate.

*Customer Display* 
- Added customer display for POS and SHIP screens. Advertisement slideshow for other screens.

*POS*
- Added processing of Kiosk barcode to open Shipping screen.

*Setup*
- Added customer display setup screen.

*Shipping*
- Added Ground Advantage Cubic Pricing.
- Added Ground Advantage dimensional Weight.
- Added Ground Advantage Endicia ShipRite Preferred pricing.
- Added Priority Mail Cubic Pricing.
- Added functionality for Priority Mail Cubic as separate service button.

### Fixes

*Drop Offs*
- Updated Tracking Number field to convert entered text to all uppercase when pressing ENTER to add new package.

*Package Valet*
- Check In: Updated to set focus to Tracking Number field after adding package.

*POS Open/Close*
- Updated Close Drawer procedure to round cash short value to 2 decimal places before checking if drawer in balance.

*Shipping*
- [SRN-578] FedEx Home Delivery should not be using the residential surcharge fee for regular ground.

*Startup*
- When converting users from old SR, users with full access rights will get those automatically in SRN.

## 1.0.54 (2023-08-22)

### Enhancements

*Build Install*
- Updated all service tables to 2023 rates.

*Shipping*
- Added USPS Ground Advantage Service.

## 1.0.53 (2023-08-11)

### Enhancements

*POS*
- Added ability to print 8.5x11 invoice.
- [SRN-575] Button Maker: SKU Button Type > Implemented "Search Inventory" button.

*Reports*
- Added Mailbox Alpha Listing Report.
- Added Mailbox Listing Report.
- Added Post Office Quarterly Report.
- Added Cancelled Mailboxes Report.
- Added PS1583 Report.
- Added Mailbox Contract Report.
- Added Shipping Reports (Carrier, Invoice, Zone, and Insurance).
- Added Inventory Listing Report.
- Added Inventory Valuation Report.
- Added AR Alpha Listing Report.
- Added AR Aging Report.
- Added AR Vault Report.
- [SRN-173] Added Commercial Invoice printing to SHIP and to Shipment History screen.
- [SRN-173] Added ability to print unlimited number of customs declaration items on the commercial invoice.
- [SRN-572] Added POS Void/Cancel Report.

*Time Clock*
- Added ability to print time sheets.

### Fixes

*DHL*
- Updated DHL XML Services requests to use TLSv1.2 security protocol as expected.

*Drop Offs*
- Fixed Drop Off Manifest printing.

*Email*
- Fixed bug where SMTP encrypted connection setting is being reversed.

*MailMaster*
- MailMaster should not display a price if the cost of a service is $0.00.
- Expanded Service Name field so that longer service names (like "Priority Mail Express") are not cut off.
- Fixed bug where a service that is not avaible, is still displayed as an option from a previous rate calculation.

*POS*
- [SRN-566] If POS Security is enabled, then the OpenDrawer screen should force the user to sign in before opening.
- [SRN-571] When creating new POS group button, the "Existing Group" drop down does not load list of existing button groups.
- Fixed issue where a POS Button group would not display if it didn't contain any sub-buttons.

*POS Payments*
- Fixed issue where customer email does not carry over to POS payment screen.

*POS Setup*
- Shipping Disclaimer textbox needs to allow Carriage Return character.

*Shipping*
- Fixed bug where USPS Ground Advantage would cause USPS shipping not to function.

*Shipping Setup*
- [SRN-568] Shipping Holidays should only display upcoming holidays, not ones from years back.
- [SRN-569] Shipping Holidays should save when changing date or carriers.

## 1.0.52 (2023-06-23)

### Enhancements

*Debug - Error Tracking*
- Added Error tracking, logging, and reporting.

*Drop Offs*
- Updated UPS Drop Off API credentials effective 06/14/2023.

*POS*
- [SRN-311] Function added to print receipt slips when opening/closing drawer.
- Added option to change number of days to display in Invoice Lookup.

*Shipping*
- [SRN-195] Added harmonized codes lookup option link to the customs form.

*Shipping Setup*
- Added option to Shipping Setup to chose between Endicia and FedEx as default Address Verification service.

*Utilities*
- Added Rate Charts option to view shipping rate charts.

### Fixes

*AR*
- [SRN-548] When deleting Payment Line item in Ledger, the Balance for Invoice needs to be updated.

*Contacts*
- [SRN-544] Clearing contact screen does not clear the search result listbox.
- [SRN-545] Cell carrier selection does not clear when address input is cleared with the circular icon.
- [SRN-546] Cell carrier selection cannot be blanked out/unselected.
- [SRN-547] Notepad customer notes do not get cleared out when screen is cleared.
- [SRN-516] Address Verification should automatically verify with default Service. 
- Submit button should be disabled while hot search drop down screen is active to prevent incomplete name entries from being saved accidently.
- Remove Address Autocomplete Timeout setting from Options popup - use static timeout of 1 second.

*Mailbox*
- [SRN-520] SMS and Email checkbox selection in "You got Mail" scren needs to remain even after the software is restarted.
- [SRN-565] Fixed issue where custom Rates can only be applied to existing rented boxes, not to new rentals.

*Main Window*
- [SRN-530] When ShipRite opens, instead of being automatically maximized, the screen size should be the same as when it was closed.
- [SRN-89] When maximizing screen, the taskbar should not be covered. 

*POS*
- Fixed receipt formatting where a Unit Price of $1000.00 or more caused a crash.
- Fixed Sales Tax formatting where a total sales tax over $1000.00 caused a crash.
- Fixed problem where a price edited in Line Editor can revert back to original price. 
- Unit Price on receipt needs to display 4 digits after decimal.
- [SRN-543] Fixed POS payment problem where after applying a partial payment to an invoice, you cannot go back and apply more payments.
- Fixed Index out of Range error when doing a refund on AR account.
- When viewing a completed invoice, the receipt quantity should not be editable.
- [SRN-549] Apply Discount button should be sticky and apply to the rest of the sale.
- Fixed wrong total problem when processing a generic refund.
- Fixed issue where refund will not record the type of refund (cash, cc, check,...).
- Fixed issue where Drawer Opening screen would not display.
- [SRN-481] Invoice Lookup needs to display if a sale was a cash, check, or credit card sale; or a refund.
- Fixed Invoice Lookup search.
- Fixed CloseID selection to eliminate possibility of duplicate IDs.
- Added printing of Z report when closing Drawer.

*POS Payments*
- Fixed "Conversion from string "" to type 'Integer' is not valid" error when accessing the payment screen for AR account.
- When canceling out of the input box for "Other" payment, payment should not be completed.
- [SRN-552] When selecting a payment button, it's color should change while the corresponding popup is open, to reinforce to the user which payment he is currently entering.
- [SRN-550] When paying cash for an AR invoice, no change back is displayed.

*Reports*
- Updated Endpoints for Package Valet and Drop Off Manager reports.
- Fixed Package Valet check out report to show check out packages.
- Fixed "Value cannot be null. Parameter name: window" error when printing a Proof Of Delivery Signature sheet report.
- Added CloseID selection for Re-Printing of Z report by CloseID.
- Added ability to print Z report with selected Close IDs.
- Updated Z-Report data processing procedure to clear cached Payments, Transactions records.
- Updated ReportsSRN.exe application to v2.0.0 with Z-Report processing changes.
- Updated Z-Report report file.

*Scale Setup*
- Fixed error if field was missing for Scale Weight Limit.

*Shipping*
- [SRN-538] Shipments should not omit customs items that have a value of $0.00.
- [SRN-539] If phone number is missing, the popup for entering the number should not only use that phone number for that shipment. The number should be saved to the database.
- Added harmonized code to FedEx shipment request.
- Added harmonized code to USPS/Endicia shipment request.
- Fixed issue where ShipTo Contact info doesn't get automatically reset for a new shipment.
- Fixed shipping panel problems when user has SpeeDee services in database.
- USPS: Replaced outdated RSA Rates option with SRPRO rates option.
- [SRN-553] Removed "USA" from country list as not to cause confusion.
- [SRN-555] When using "Ship Letter" button in POS to open SHIP screen, it won't return automatically to POS after shipment.
- [SRN-558] ShipTo address won't print on the POS receipt even if that option is selected in Setup.
- [SRN-561] Fixed "Object Reference" error when pressing Time In Transit button without a ShipTo address pulled up.
- [SRN-563] FedEx/UPS 2Day AM services should not be available to residential destinations.
- [SRN-559] Fixed Hot Search option in both Contact Manager and SHIP where it errors out if the name has an apostrophe in it.
- [SRN-557] If no Declared Value was entered, then it should not be displayed on receipt
- [SRN-560] Added Latest Zone based Additional Handling and Large Package Fee rules and pricing.
- [SRN-562] Added a city/zipcode lookup in SHIP screen.
- When changing Consignee address, the shipping rates should be recalculated.

*Shipping - Print Label*
- [SRN-564] When pressing back button in Print Label screen, it will add the previous shipment to the receipt again.

*Tickler*
- [SRN-422] When opening Tickler make sure to login with User or use current user.
- [SRN-423] Tickler should open on program openinig automatically if there are ToDo items.
- [SRN-421] Tickler Due Date Time should be formatted as time.

## 1.0.51 (2023-02-27)

### Enhancements

*Drop Offs*
- [SRN-529] Added running count of how many shipments are scanned in.

*Mailbox*
- [SRN-520] Added new screen for "You got Mail" Daily Notice.

*MailMaster*
- [SRN-514] Certified and Return receipt have separate tracking numbers. Both need to be able to be scanned in.

*POS Manager*
- Sale Quantites can now be edited right on the receipt view.

*POS Payments*
- [SRN-7] Bulk Account Payments added to payment screen.

*Ship Manager*
- [SRN-522] Added option to set default Ship From to SHIP screen.

### Fixes

*Contacts*
- [SRN-517] After verifiying address with Endicia and choosing the verified address, the company, first, and last name fields should not be cleared.
- [SRN-518] After typing in a company name and pressing TAB, cursor should move the first name field, not the address field.
- [SRN-523] When selecting a country other then USA, the contact manager should allow more then 2 characters into the State/Province field.
- [SRN-528] Added option to search customers by mailbox number and display all contacts associated with that mailbox.

*Inventory*
- [SRN-509] In main inventory screen, changing the SKU should save the changes.
- [SRN-510] In inventory Detail screen, changing SKU of existing item causes error "Index was out of range" when saving.
- [SRN-512] Changes in Detail screen need to transfer over to the main Inventory screen.

*POS Manager*
- POS SKU Search optimized to show results of SKU and Description matches in a new popup window.
- SKU Search results can be sorted by clicking on the column header.
- [SRN-511] Line Editor should preselect tax county assigned to the receipt line item instead of the default tax county.
- [SRN-508] Line Editor should take into account Level Pricing.
- [SRN-535] POS Needs to check if the Department of rung in SKU is taxable before charging tax.
- [SRN-521] A phone search in POS without dashes, should match a phone number with dashes.

*POS Payments*
- Updated "Exact Cash" button image.
- Updated Fast Cash buttons to eliminate border overlap with image.
- [SRN-479] POS should not let you complete a sale for a customer without applying any payment.

*Setup*
- Setup items updated in policy table should not require a program restart to take effect.

*Shipment History*
- [SRN-515] In detail screen, need to be able to manually change status of Shipment.

*Zip Code Editor*
- [SRN-524] Added option to search by partial zip code. This should make it easier to update multiple zipcodes at the same time.

## 1.0.50 (2022-11-25)

### Enhancements

*Drop Offs*
- [SRN-73] Updated UPS Drop Off web requests to production endpoint.

*Email*
- [SRN-490] Increased field size for email addresses to allow multiple emails to be entered.

*Package Valet*
- [SRN-504] Package Inventory: Add option to search by customer name.

*POS Manager*
- [SRN-418] Added ability to view POS Receipt in COGS view.
- [SRN-485] Voiding a Sale needs to have a screen to approve the void and enter in a reason.
- [SRN-493] Voiding a sale should leave a record in the Void table.
- [SRN-492] After voiding sale, screen needs to be cleared and new sale should be started.
- [SRN-483] Corrected Refund Receipt.
- Added Reprint Receipt option to "Recovered Invoice" menu. 

*POS Payments*
- Added Other payment functionality.
- Multi payments are now listed in a listbox for easier viewing.
- When opening payment screen for AR user, all outstanding invoices will be listed.

*Shipping Setup*
- Added option to setup ShipandInsure insurance.

### Fixes

*Drop Offs*
- Updated carrier selection to fix issue where carrier name not correctly identified when Auto Detect disabled.
- Updated open form calls to use same method to fix issue where variables don't get set when opening form in some cases.
- [SRN-73] Updated UPS Drop Off web requests to parse error response to display to user and save to response file.

*Mailbox*
- [SRN-506] Change the terminology to PERSONAL and BUSINESS instead of RESIDENTIAL and COMMERCIAL.

*Package Valet*
- [SRN-495] Highlight Mailbox# when focus on Mbx field to make it easier to type in new number without needing to backspace out original.
- [SRN-496] Package Valet date of expired mailbox not displayed.
- [SRN-497] After "Process and Save" is clicked, the mailbox and customer information should be cleared.
- [SRN-498] Package Valet Tracking number field doesn't fit long 34 character scans.
- [SRN-499] When exiting the screen, a warning should show up if there are shipments in the queue and the user did not press "Process and Save".
- [SRN-501] Check Out: Customer name search should look for partial matches if no exact match found.
- [SRN-502] Check Out: After processing the "Customer Refused" option, the "Picked Up By" and "Notes" fields are not cleared out.
- [SRN-503] Package Inventory, Check Out: Added option to sort list by clicking on column header.

*POS Manager*
- [SRN-489] COGS need to be recorded in Transactions table.
- [SRN-488] Open the SHIP screen from POS and exit out without shipping a package, error will display.
- [SRN-478] When putting an invoice on Hold/Quote the Invoice Notes are not being saved.
- [SRN-469] When invoice pulled from Hold/Quote, cannot add any more items to receipt.
- Fix: When invoice pulled from Hold/Quote cannot edit or delete line items on receipt.
- Fix: when invoice pulled from Hold/Quote cannot select/change customer.
- [SRN-480] When completing a Hold/Quote sale, the original quote entry remains.

*POS Payments*
- [SRN-428] Pay by Check, name not pre-filled.
- Rearranged layout of screen to be more intuitive and easier to use.
- When processing Check transaction, the check details need to be saved to the database.
- When processing Credit Card transaction, the CC details need to be saved to the database.
- Added credit card type combobox to the the Credit Card payment popup menu.
- Payment entry should automatically add a decimal point.
- Credit Card processing through SmartSwiper will ensure that there is a "SRN" user within SmartSwiper to be used for processing.
- If multiple credit cards are used, the receipt needs to print the details for all cards.
- The "Remaining Balance" display should only be displayed once a partial payment has been made.

*POS Refund*
- [SRN-484] Refund screen should not have the Transaction ID visible or editable.
- Refund screen needs the ability set quantity refunded, not only the entire line total.
- Refunds can only be completed by an authorized user.
- Refund screen needs to display the correct user, drawerID, and prefill the refund total into the selected refund type textbox.

*Startup*
- Error when "Checking for Pricing Matrix" caused the loading process to terminate too early.
- Before renaming fields, check if the fields already exists.

## 1.0.49 (2022-09-20)

### Enhancements

*Drop Offs*
- [SRN-73] Added UPS Drop Off Scan Event web request to upload each UPS drop off package when processing and saving packages (test server).
- [SRN-73] Added UPS Commercial Invoice web request to check if commercial invoice required when adding UPS package (test server).

*Mailbox*
- Added option to customize rental rates for specific mailbox.

*User Setup*
- Added Mailbox Setup option to User Setup.

### Fixes

*Mailbox*
- Mailbox pricing needs to be reset if rate type is changed from drop down box.

*POS Manager*
- Updated Recover Invoice to change "Cash Out" button caption to "Reprint Receipt" if recovered invoice balance equals 0.
- Updated Process Transaction to check if receipt is empty before processing.
- Updated Process Transaction to only print recovered invoice if balance equals 0.

## 1.0.48 (2022-09-07)

### Enhancements

*Build Install*
- Updated all service tables to 2022 rates.

*Carrier Setup*
- Added option to enter UPS RPDO Access ID.

*Main Window*
- Added display to see the logged in user

*Shipping*
- Added Pricing Matrix to Ship price calculations.
- Added Letter Markup to Ship price calculations.
- Matrix: letter percentage/fee reset to 0 if not default.
- [SRN-477] Shipments to PO Boxes can only be processed with USPS.

*User Setup*
- Added permission options throughout the program

### Fixes

*POS Manager*
- [SRN-459] When deleting a MailBox Rental entry in POS, the attached notes with the rental period and expiration date need to be also deleted.

*Shipping*
- Fixed Pricing Matrix to handle dimensional weights over 150 lb.

*Shipping Markups* 
- Pricing Matrix should be sorted by zones selected, then by start weight.

## 1.0.47 (2022-07-29)

### Enhancements

*Shipping*
- Updated DHL XML Services from v10.0.4 to v10.0.6.

*Shipping Markups*
- Adjusted Pricing Matrix to make multiple zones selectable.
- Added Flat Fee letter markup to Pricing Matrix.

*Statistics*
- Added Statistics for POS Sales by Department and By SKU.
- Added PieChart to Statistics screen.

### Fixes

*Build Install*
- Updated Report Writer database.
- Updated Hourly Sales, Hourly Sales Analysis, Sales Inquiry, Sales Tax Report, Sales Tax By Accounts, Z-Report reports.
- Updated Reports exe application.

*Inventory*
- Double clicking on a item needs to open the items detail screen.

*POS Manager*
- Recovered Invoice pop up would not display the payment info in the listview.
- After saving an Invoice Note, the Options popup should close.
- Invoice Notes need to be cleared for a new sale.

*Reports*
- Updated reports to use Reports exe application with Report Writer database.
- Updated report processing procedure for Production By Department, Production By AR Account, Production By Sales Clerk, Departmental Chargeback, Sales Journal By Date / Invoice, Sales Journal By Customer Account, Sales Journal By Sales Clerk, Consolidated Z Report - By Date reports.
- Added functionality for Sales Inquiries, Sales Tax Report, Sales Tax Report for AR Accounts, Hourly Sales Ticket reports.

*Ship Manager*
- Fixed Auto Time in Transit error when weight is over 150lb.
- Fixed Pricing for Air Freight shipments.
- COD option hidden, since it's rarely used.

## 1.0.46 (2022-05-13)

### Enhancements

*Reports*
- Added printer selection and print preview option.

*Statistics*
- Added Statistics screen.

*Utilities*
- Added Statistics option to Utilities.

### Fixes

*Mailbox*
- [SRN-468] Mailbox number with 4+ digits should not get cut off.

*Mailbox Setup*
- [SRN-467] When exiting Mailbox Setup, check for unsaved changes and remind user to save those.

*POS Manager*
- [SRN-460] When renting a Mailbox in POS, there is only one total amount brought back to POS. The fees such as Admin Fee, Other Fee, Late Fee, Key Deposit, should all be listed as separate SKU line items on the receipt.

## 1.0.45 (2022-03-29)

### Enhancements

*Inventory Detail*
- [SRN-402] Added Vendor selection to Inventory Detail screen

*Mailbox Setup*
- [SRN-466] Mailbox Setup pricing is now displaying both total and per month rates.

*POS Manager*
- Added option to change Line Item description to POS Line Editor.

*Printer and Peripherals Setup*
- [SRN-28] Added Scale settings to Other Peripherals - Scale setup option.

*Ship Manager*
- [SRN-28] Added Scale integration to Shipping screen.

*Shipping*
- [SRN-454] Upgraded DHL XML Services to v10.

*Shipping Markups*
- [SRN-390] Reorganized Pricing Matrix markups entry.

### Fixes

*AR*
- LedgerByInvoice: Added Invoice Notes column.
- History View: Separated records by Invoice and fixed formatting
- [SRN-352] Miscellaneous Setup: Added POS Price Level drop down selection
- [SRN-351] Miscellaneous Setup: Added Tax County Drop Down selection
- [SRN-353] Cannot add new users. Contacts screen will come up, but clicking Save/Select does not return user to AR screen
- [SRN-354] Cannot Edit user. Contact Manger opens up with the selected user NOT loaded.
- [SRN-357] Updated popup Message when pressing SAVE.
- [SRN-356] Account History view needs to include sales tax in Total column.
- [SRN-96] Fixed logic for calculating account aging.
- [SRN-376] If AR screen is accessed outside of POS, then cannot open an invoice in POS from ledger screen.

*Build Install*
- Updated default Pricing.mdb to Pricing.accdb in build data path.
- Updated USMail_Services.accdb to re-add missing Postcard fields in FirstClass table.
- Added Pricing.accdb to install data path.
- Added SKU.lst to install app path.

*Carrier Setup*
- [SRN-445] Changed "Endicia Account Number" to "Endicia.com User Name".
- [SRN-446] UPS ASO's are now all same discount level.

*Contacts*
- [SRN-417] Added confirmation prompt to Create Account button.
- [SRN-426] Updated to increase size of Save/Select button.

*Inventory Detail*
- Removed AR Ledger Note. It should not be tied to the SKU item.

*Mailbox*
- [SRN-438] Updated mailbox rent procedure to check additional names list when renting mailbox to prevent index error.
- [SRN-439] Updated mailbox cancel procedure to clear the mailbox button after canceling.
- [SRN-440] Updated mailbox cancel procedure to check for users with multiple mailboxes when updating Contacts mbx flag.
- [SRN-441] Updated mailbox additional name update procedure to check for users with multiple mailboxes when updating Contacts mbx flag.

*Mailbox Setup*
- Fee amount should be cleared out if no SKU is entered.
- Added check for non-existant SKU's
- Mailbox fee amounts need to be saved to the corresponding SKU in inventory.

*Package Valet - Check Out*
- [SRN-433] Added Name field to package listing.
- [SRN-434] Updated package listing to sort by name.
- [SRN-437] Updated to list all packages by default.

*Package Valet - Inventory*
- [SRN-436] Added Name field to package listing.
- [SRN-436] Increased width of tracking number field in package listing.
- [SRN-436] Updated package listing to sort by oldest to newest.

*POS Manager*
- [SRN-448] When right clicking on SKU shortcut button, 'Edit Inventory' option was not programmed.
- [SRN-107] Updated SKU search contact lookup to show search for partial matches.
- [SRN-95] Partial SKU search.
- [SRN-409] Customer drop down doesn't close after leaving POS.
- [SRN-127] Lookup customer by phone number.
- [SRN-54] Added functionality to adding Memo to receipt.
- [SRN-340] Updated to fix issue where prices show blank on receipt if selling price is 0.
- [SRN-124] Updated to show POS Line editor when selecting Change Price button and receipt line selected.
- [SRN-415] Updated "Customer Editor" button label to "Customer Lookup".
- [SRN-455] Added functionality to the on-screen ENTER keypad button.
- [SRN-444] Updated Pole Display box to fix issue where Sales Tax amount can get cut off.
- [SRN-122] Updated to not print a receipt when putting invoice on Hold.
- [SRN-123] Added functionality to Quote button (Quote report still needs to be finished).
- [SRN-397] Updated Line Editor to add option to change Tax County.
- [SRN-431] Added confirmation prompt to contact drop down AR Create button.
- Updated print receipt procedure to add "(R)" after "Invoice#" when printing Recovered Invoice.
- [SRN-35] Add MEMO button to receipt programed.
- [SRN-450] Customized NOTE buttons in ButtonMaker were not functional.
- [SRN-452] POS Note option in Inventory was not functional.
- Changed Receipt ListBox to use WPF Data Binding
- Removed Address from Receipt View header to save space in POS display.
- When Clicking "AR - Create Edit/Account", don't prompt user to create new account if the selected customer already has an account.
- Updated to load receipt options when window first loaded.
- Updated print receipt procedure to include shipping disclaimer when shipping service in receipt.
- [SRN-453] POS Popup Message setup in Inventory Detail screen does not display when SKU is rung in POS.
- [SRN-457] Linked SKU option in inventory is now functional in POS.
- [SRN-458] POS needs to check Barcode field in inventory when SKU is entered.
- [SRN-462] Cash Paid Out receipt needs to print entered reason/purpose.
- [SRN-464] When pulling up Invoice from History, it should not be changeable.
- [SRN-463] When Looking up Invoice from Invoice History, added ability to reprint receipt.
- [SRN-461] When AR customer is pulled up, check if that AR account has a specific Tax County selected, and pull up that tax county into POS.
- [SRN-456] Inventory Level Pricing programmed in.

*POS Button Maker*
- [SRN-449] Programmed Quantity Option in POS Button Maker.

*Ship Manager*
- Updated to save package to database with Letter indicator when Letter/Envelope packaging selected.

*Shipping Setup*
- Fixed issue where 3 Markup Levels were not being pulled up and saved.

*Startup*
- Updated to run import of email templates after shiprite database conversion.
- Updated to reset special updates version if shiprite database converted.
- Added special update to add field to Transactions table to mark shipment lines.

## 1.0.44 (2021-09-20)

### Enhancements

*Build Install*
- Updated all service tables to 2021 rates.
- Added SRNSQLProcessor.exe to install app path.

*Main Window*
- Added program version number to top of main screen.
- Added red notification counter to tickler button to notify user of new ticklers.

*POS Manager*
- Added tickler icon with red notification counter to top of window to notify user of new ticklers.

*Setup*
- [SRN-11] Added Zip Code Editor.

*Ship Manager*
- [SRN-342] Added FedEx/UPS Time In Transit functionality.
- [SRN-412] Added FedEx/UPS auto Time In Transit requests when enabled.

*Shipping*
- Updated FedEx Web Services version (22 -> 26). 

*Shipping Markups*
- [SRN-390] Updated to show Pricing Matrix for non-PostNet users if setting enabled in Shipping Setup.

*Shipping Setup*
- [SRN-390] Added option for non-PostNet users to choose between level markups and pricing matrix.
- Added option to set markup levels.
- Added option to set Auto Time In Transit option on/off.

### Fixes

*Build Install*
- Updated default ShipriteNext.accdb database file to clear SpecialUpdatesVersion value in Policy table.

*Contacts*
- [SRN-404] Updated to fix issue where contact is duplicated when re-opening Contact Manager in Shipping screen and clicking Select button.

*Inventory*
- [SRN-79] Allow up to 4 decimal points for Inventory pricing.

*MailMaster*
- [SRN-393] After unselecting the Large Package option, the dimensions should be cleared out.
- [SRN-407] Updated to include international countries in country drop down selection.

*POS Manager*
- Updated to only change Clerk label if User Login successful.
- [SRN-48] Updated to fix issue where discount amount doesn't reset after using Apply Discount button and selecting item.

*POS Payments*
- Updated to add functionality to the "00" keypad button.

*Shipping - Print Label*
- [SRN-395] Updated to fix issue where signature selection in Ship Manager is reset in Print Label screen.

*Startup*
- Updated 06/05/2019 special update to add InvNum and Note fields to InvoiceNotes table.

*User Login*
- Updated to return not allowed status when User Permissions need to be setup.

## 1.0.43 (2021-08-23)

### Enhancements

*AR*
- [SRN-349] Added Account# search functionality.
- [SRN-346] Added Ledger Delete Line button functionality.
- [SRN-347] Started Ledger Adjustment button functionality with adding input checks before performing adjustment.

*EOD Manifest*
- Added upload of pending shipments.

*Ship Manager*
- [SRN-334] Added functionality for Offline Batch Label (still need to add printing of label).
- [SRN-159] Carrier Packaging Selection: To make clear that selected packaging overrides manually entered dims, the dims of the packaging is now displayed under the carier logo.
- [SRN-344] Added FedEx One Rate selection, pricing, discounts, and webservice call.
- Moved 3rd Party Insurance button to combine with the 3rd Party Insurance On/Off switch.
- Dimensions need to be rounded up to the full inch before shipment is submitted.

*Shipping - Print Label*
- [SRN-334] Added functionality for Offline Batch Label (still need to add printing of label).

### Fixes

*AR*
- Selecting "Edit User" without a user selected causing error.
- [SRN-374] Fixed issue where entering number in Account Name field and pressing ENTER causes InvalidCastException error.

*Build Install*
- Updated installer to explicitly override install actions to prevent InstallState file not found error.  

*Carrier Setup*
- [SRN-337] Endicia Password should be masked.

*Contacts*
- [SRN-378] Updated to auto select Residential address type when Name entered as "Last Name, First Name".
- [SRN-379] Updated to auto select Commercial address type when Name entered as Company Name.
- [SRN-381] Removed confirmation message when contact added successfully.
- [SRN-380] Updated to fix issue where pressing TAB from Address Line 2 changes focus to Address Line 3 when not visible instead of Zip Code.
- Updated Country selection combo box to fix issue where Address Line 3 doesn't show after selecting intl country first time.

*Drop Offs*
- Updated Add Package procedure to clear Tracking Number text box if found in packages list.
- Updated Packaging Fee text box to format number as-is instead of dividing by 100.
- Updated Process and Save procedure to set focus to Tracking Number text box after processing packages.
- [SRN-377] Updated window to set focus in Customer Name field when loading.
- [SRN-383] Updated Auto Detect setting to be persistent after leaving window like Print Receipt, Email Receipt settings.
- [SRN-382] Updated to show confirmation message when trying to leave form with entered data.
- [SRN-385] Separated FedEx Express and FedEx Ground carrier options to allow user to set option when adding package with Auto Detect disabled.

*EOD Manifest*
- Updated EOD Manifest header icon image.

*Main Window*
- Replaced Acct Receivables button with EOD Manifest.

*PackMaster*
- [SRN-341] Updated pack item set array size (20 -> 50) to fix 'Index was outside the bounds of the array' error when entering data.
- [SRN-341] Updated Fragility Calculator to check materials array indexes to fix 'Index was outside the bounds of the array' errors when entering data.
- [SRN-371] Updated to calculate and show Packaging Length+Girth value.
- [SRN-370] Updated calculate charges function to fix issue where Double Box in Fragility Levels is always set to True after Fragility Calculator.
- [SRN-369] Updated to populate the Outer Box by default instead of the Inner Box (which should be populated when item is double boxed).
- Updated Packaging Outer Box label to "Shipping Box".
- Updated Packaging Outer/Inner L/W/H, Dim Weight, and L+G textboxes to be blank instead of 0 by default.

*POS Manager*
- [SRN-126] POS Line Edit: Pressing ENTER key in any field should process Save button.
- [SRN-126] POS Line Edit: Fixed issue where discount not applied to line item.
- [SRN-126] POS Line Edit: Updated Sell Price to be formatted with 2 decimal places.
- POS Line Edit: Added user entry checks.
- POS Line Edit: Fixed textboxes tab order.
- POS: If customer has mailbox flag on, but no actual mailbox. "Conversion from string to type Date is not valid" error when pulling up user.
- [SRN-71] Added prompt to confirm canceling a sale when leaving the Window.
- [SRN-46] Added functionality to delete line items from receipt.
- Updated Line Editor to fix issue where receipt not updated after updating values.
- [SRN-55] Updated to fix issue where User Login doesn't have focus when opening.
- Updated Line Editor to recognize selecting shipping lines.
- Added ability to remove shipment items from receipt.

*POS Open/Close*
- [SRN-399] Clicking on the first column buttons (penny, dime, quarter,...) should add a roll of coins. Clicking on the 3rd column Total buttons should add 1 coin/bill.

*Shipment History*
- Updated shipment list to only allow one selection at a time for now.
- Updated to check if a shipment is selected before processing buttons to fix index out of bound errors.
- Updated to allow pressing Backspace or Delete on the keyboard to delete selected shipments.
- Updated to close the shipment detail view after deleting shipment.

*Shipping*
- [SRN-394] Carrier Packaging selection needs to be passed to the carrier. Carrier Packaging should not be ignored and submitted as "Other".
- [SRN-24] Endicia International GIF image should print on report printer.

*User Setup*
- [SRN-360] When no user is selected, the save and delete button should not be visible.

*Utilities*
- Updated EOD Manifest icon image.

## 1.0.42 (2021-07-08)

### Enhancements

*Build Install*
- Updated fields.upd to add fields to GiftRegistry table.

*POS Manager*
- Added Gift Card processing.

*POS Payments*
- Added New Gift Card popup to enter gift card payment.
- Added Gift Card processing.

*Startup*
- Updated database update function to create GiftRegistry table.

### Fixes

*Contacts*
- [SRN-206] Fixed issue where "contact updated" message shows when just selecting contact without making changes.

*POS Manager*
- Updated Gift Card popup to remove Find Gift Card button.
- Updated Gift Card popup cards listing columns.
- Updated New Gift Card popup controls placement.
- Updated Cash Paid Out popup save procedure to print cash paid out receipt.
- Updated add line to receipt procedure to limit SKU length.

*POS Payments*
- Updated "Change Due" label to "Balance Due".

## 1.0.41 (2021-07-02)

### Enhancements

*Build Install*
- Updated default ShipriteNext.accdb database file to add default email templates.
- [SRN-5] Updated ShipriteOnlineQB.exe in install app path.
- Updated default Finance.mdb in install data path.
- Added AccountingDepartments_CA.upd to install data path.
- Added AccountingDepartments_US.upd to install data path.
- Added QBC_COA_CA.upd to install data path.
- Added QBC_COA_US.upd to install data path.
- Added QBC_DEPT_CA.upd to install data path.
- Added QBC_DEPT_US.upd to install data path.

*Carrier Setup*
- [SRN-114] Added option to print 8.5x11 labels for FedEx. 

*Contacts*
- [SRN-92] Added Address Autocomplete.
- [SRN-187] Added Address Autocomplete options to enable/disable and set timeout period.
- [SRN-165] Added Endicia Address Verification.
- [SRN-164] Added Zipcode to City lookup.
- [SRN-242] Added search by phone number to Company Name entry field.

*Drop Offs*
- [SRN-219] Added setup option to set number of copies of receipt to print.
- [SRN-129] Added "Lookup" button next to customer name entry for customer lookup.
- [SRN-130] Added search by phone number to customer name field.
- [SRN-251] Added setup option to edit drop off disclaimer.

*EOD Manifest*
- Added functionality to buttons for printing manifest, voiding shipment, viewing shipment history.
- Added Endicia SCAN Form.
- Added functionality for re-printing of labels.

*Mailbox*
- [SRN-142] Added user login when Program Security is enabled.
- [SRN-240] Added shortcut keys to open Drop Off Manager (F5) and Package Valet (Ctrl+F5).

*Main Window*
- [SRN-328] Added User Log In for Tickler if program security is enabled. Log person who opened an Action Item and who closed it.

*Package Valet - Package Inventory*
- Added functionality for Package Inventory.
- [SRN-217] Added mbx number column to package listing.
- [SRN-228] Added tooltip to package listing In column.

*Package Valet - Package Check Out*
- Added functionality for Package Check Out.

*Package Valet - Package Reports*
- Added functionality for Package Reports (still working on updating reports to pull data from SRN databases).
- Added package classes to Select Package Class combobox for Packages On Hand report.
- Added Load Names buttons for By Customer reports.
- Updated From/To dates to default to current date.
- [SRN-229] Updated date range label for clarity.

*POS Manager*
- Added Gift Card popup.
- Added popup to enter a new Gift Card.
- [SRN-139] If customer has a mailbox, display box number and expiration date in POS.
- [SRN-239] Added shortcut keys to open Drop Off Manager (F5) and Package Valet (Ctrl+F5).

*POS Setup*
- Added option to set DrawerID under General POS Options.

*POS Payments*
- [SRN-52] Fixed Keypad Entry in Payment screen

*Reports*
- Added print preview popup.

*Ship Manager*
- Added multi-piece shipment processing.
- [SRN-231] Added shortcut keys to open Drop Off Manager (F5) and Package Valet (Ctrl+F5).

*Shipment History*
- Added functionality for re-printing of labels.
- Fixed error when voiding FedEx label if Setup object was not set.

*Shipping - Print Label*
- [SRN-335] Added 'Manual Label' Button

*Startup*
- When converting old shiprite copy Shiprite_DropOffPackages.mdb, Shiprite_MailboxPackages.mdb files from old shiprite if found.
- When converting old shiprite copy DropOff_Disclaimer.txt files from old shiprite if found.
- When converting old shiprite copy email templates from old shiprite.

*Utilities*
- [SRN-5] Updated QuickBooks button ShipriteOnlineQB.exe process arguments.
- Updated QuickBooks button ShipriteOnlineQB.exe process to handle new arguments.

### Fixes

*Build Install*
- Updated application icon image to fix desktop shortcut not displaying icon after install.
- Updated default ShipriteNext.accdb database file to remove unused tables.
- Updated default ShipriteNext.accdb database file to fix issue where Manifest table looking for Counter field when opened in MS Access.
- Updated ShipriteOnlineQB.exe to v1.0.1 in install app path.
- Updated Setup project files locations to reference WindowsVolume variable instead of "C:\" string.
- Updated Setup Installer Title and Product Name.
- Added Drop Off Email template files to install app path.
- Added Drop Off Email template file for packaging fee to install app path.
- Added Test Email template file to install app path.
- Added VB6 dll files required for QuickBooksOnline.exe to install app path.

*Carrier Setup*
- Removed option for separate NetStamps Endicia account.
- [SRN-163] FedEx Account Number should be pulled from textbox instead of database.

*Contacts*
- Removed Esri Address Verification.
- Updated textboxes opacity when autofilling info.
- Improved textbox text color.
- [SRN-156] Updated FirstName, LastName textboxes to clear placeholder text when clicking on them.
- [SRN-153] Updated to actually save entered Email.
- [SRN-165] Updated FedEx Address Verification to work.
- [SRN-155] Updated save/select procedure to check for changes before trying to save contact.
- [SRN-154] Updated to autofill Company Name field with "LastName, FirstName" when appropriate.
- [SRN-245] Fixed issue where Save/Select button doesn't save contact if data blank.
- [SRN-236] Updated to clear notepad notes when different contact selected.
- [SRN-235] Fixed issue where Address Verification crashes if original address fields blank.
- [SRN-237] Updated popup message when saving duplicate contact.
- [SRN-261] Fixed Address Verification issue where address placeholder text pulled into verification popup.
- [SRN-234] Updated Marketing Tools popup with ability to close it by double-clicking the list.
- [SRN-262] Fixed issues where saving duplicate contact would only save changed fields and drop all other data.
- [SRN-233] Updated to clear window and search list when contact deleted.
- [SRN-206] Fixed issue where "contact updated" message would show when just selecting contact without making changes.
- Updated to check if postal code db file exists before trying to access it.
- Removed contact profile image option.

*Customs*
- Added function to check user input.
- Customs Info should not be cleared each time the customs window is opened.

*Drop Offs*
- Updated Drop Off Email templates paths to reference application path variable.
- [SRN-191] Updated Reports popup to default dates to current date.
- Updated window to move Tracking Number and Select Carrier above other package info.
- [SRN-247] Fixed issue where email not loaded with pulled up contact.
- [SRN-249] Fixed email receipt to include packages and disclaimer text.
- [SRN-250] Fixed print receipt to include disclaimer text.
- [SRN-260] Fixed Delete Selected button to remove selected lines instead of those with Ground checkbox seleted.
- [SRN-270] Updated email receipt to include packaging fee value.
- [SRN-267] Updated print receipt copies to show the same date.
- [SRN-269] Updated email receipt to fix date format.
- [SRN-268] Updated print receipt to format packaging fee value as currency.
- [SRN-273] Fixed issue where duplicate tracking number not deleted when prompted.
- [SRN-275] Fixed data saved to DropOff_Packages database.
- [SRN-276] Removed DHL Ground from Drop Off Compensation Setup.
- [SRN-277] Fixed drop off compensation setup issue where previous package compensations not updated when saving.
- [SRN-278] Fixed add package issue where FedEx tracking numbers return error when Auto Detect enabled.

*Email*
- Updated email sending to sanitize loaded email settings from database.
- [SRN-248] Fixed issue where send email returns error trying to set SMTP port.
- Fixed some issues with emailing.

*Email Setup*
- Updated to sanitize email settings when communicating with database.
- Updated Save button to fix save message popup handling.
- [SRN-246] Updated save settings procedure so unsaved changes in email template message shows when necessary.
- [SRN-244] Added functionality to Send Test Email button.
- Added check for valid numeric SMTP port.
- Updated to fix issue with loading saved SMTP port.
- Fixed Email Template Editor.

*EOD Manifest*
- Fixed Display of Shipments by Carrier.
- Updated pickup date selection to only allow selecting date from today and forward.

*General*
- [SRN-178] Removed message box shown when a new database policy entry created.

*Mailbox*
- [SRN-332] When renting or cancelling a mailbox, the Contacts.MBX field needs to be updated.
- [SRN-143] Updated to lock Start/End dates from being edited.
- [SRN-141] Updated to diplay clerk in mbx history.
- [SRN-300] Updated to fix adding additional names to mailbox.

*Package Valet - Package Check In*
- Updated UI elements.
- Updated to clear mbx expire date for mbx number 0.
- Updated to default packages to selected.
- [SRN-170] Added email functionality using email templates.
- [SRN-171] Added SMS functionality using sms templates.
- [SRN-169] Updated customer lookup to populate mbx info for mbx customer or clear info for non-mbx customer.
- [SRN-204] Removed success message box when packages processes and saved.
- [SRN-76] Updated starting cursor field to customer name.
- [SRN-208] Updated Print Notice to increase box number size.
- [SRN-225] Added carrier option for Amazon.
- [SRN-226] Updated to save check box selections.
- [SRN-284] Updated mbx expire date to remove time value and fix label being cut off.
- [SRN-285] Updated Print Notice to fix formatting.
- [SRN-286] Add Printer options for user to print Print Notice on Receipt or Label or Both.
- [SRN-287] Fixed Delete Selected button to delete all selected entries.
- [SRN-292] Updated mbx number to default to 0.
- [SRN-75] Fixed issue where mbx name list would append all mbx names looked up.
- [SRN-283] Updated package list Date Received column to remove time value.

*POS Manager*
- Updated process transaction to print receipt copies based on number of copies set in POS Setup.
- Updated print receipt to include shipping disclaimer text when enabled.
- Updated print receipt to include receipt signature text.
- Updated print receipt to set font style to bold and use default font name and size values.

*POS Open/Close*
- [SRN-310] Updated cash count textboxes to select all text on mouse/keyboard focus for easier user entry.
- [SRN-308] Updated Close Drawer to load and display saved open date/time in Drawer Opened field.
- Disabled calculated totals textbox fields from user input.

*POS Payments*
- [SRN-52] Fixed Keypad Entry in Payment screen

*POS Setup*
- Updated database field names to load/save Receipt Signature text, Shipping Disclaimer text, and Enable Shipping Disclaimer checkbox.
- Updated to hide Marketing Tools and QuickBooks Online Integration Setup options for now.

*Reports - Mailbox Reports*
- [SRN-312] Updated 1583 report to show "Personal Use Only" under field 9 based on the status of the mailbox instead of the renter contact.

*Ship Manager*
- Updated shipment processing to include enabled Shipment Info note lines to POS receipt.
- Fixed Endicia international error due to customs.
- [SRN-323] Fixed issue where selecting country name not found in USPS zones causes E_FAIL database exception error when querying USPS FCMI rates.
- Fixed issue where selecting country name with apostrophe causes E_FAIL database exception error when querying USPS zones.
- [SRN-110] Updated shipment processing to disable USPS Retail Ground to unavailable zones 1-4.
- [SRN-111] Updated shipment processing to enable Saturday Delivery button when applicable.
- [SRN-301] Updated shipment processing to use EOD pickup date instead of today's date.
- [SRN-100] Updated service selection to show customs form for US territories except when USPS service.
- [SRN-326] Updated country selection to fix lag caused by lost focus event trying to process shipping rates too many times.
- [SRN-158] Updated to hide FedEx/UPS Ground options when selecting Express/Air packaging.
- [SRN-338] DHL: Updated XML web services request to return and print 4x6 label image instead of 4x8.

*Shipment History*
- Updated Delete procedure to add prompt to confirm deletion.
- Fixed Endicia refunds
- [SRN-131] Added WebService calls to void shipments for FedEx and UPS.

*Shipping*
- [SRN-302] Updated Endicia ship request to save successful package status as "Pickup Waiting" instead of "Exported".
- [SRN-322] Updated UPS ship request to use billable weight for packages and actual weight for letters.
- [SRN-45] Updated UPS web services to production endpoint to prevent possible warning messages.

*Shipping - Print Label*
- [SRN-321] Updated window to lock necessary fields from editing.

*Shipping Setup - PackMaster Setup*
- Labor should look up "Labor" field in database, not "Difficulty".

*Signature Pad*
- [SRN-279] Fixed issue where signature window not mirroring signature device.

*Startup*
- Added special update to copy Receipt Signature, Shipping Disclaimer, and Enable Shipping Disclaimer values to new database fields.
- Added special update to add CCEndBlock Memo field to Payments table.
- [SRN-183] Removed message box shown when a new database table created.
- [SRN-179] Updated startup special update to check if Manifest primary key exists before dropping/adding.
- [SRN-180] Updated startup special update to check if Manifest indexes/fields exist before dropping/adding.
- [SRN-181] Updated startup special update to check if Holiday indexes/fields exist before dropping.
- Fixed possible string to boolean type conversion errors when loading security settings.

*Tickler*
- [SRN-329] Action Item details need to be cleared out if that item is no longer selected.

## 1.0.40 (2021-03-05)

### Enhancements

*Build Install*
- Added default Pricing.mdb, ShipritePackaging.mdb files to install.
- Updated default ShipriteNext.accdb database file as post-conversion and to correct some data.
- Added default Finance.mdb file to install.
- Added ShipriteOnlineQB.exe to install app path.

*Carrier Setup*
- Added functionality to Endicia NetStamps Setup.

*Drop Offs*
- Added functionality.

*Email Setup*
- Added email templates for the different notification types.

*MailMaster*
- Added NetStamps integration

*Package Valet*
- Added functionality for Check In packages.

*Startup*
- When converting old shiprite copy Pricing.mdb, ShipritePackaging.mdb files from old shiprite if found.
- When converting old shiprite copy Finance.mdb file from old shiprite if found.

*Utilities*
- Added QuickBooks Online to QuickBooks button - run via ShipriteOnlineQB.exe from app path.

### Fixes

*Build Install*
- Added missing logos for DHL and FedEx.

*Contacts*
- Added more options to Cell Carrier listing.
- Fixed Commercial, Residential checkboxes to set opacity values with numbers instead of strings.
- Updated saving contact to add Cell Carrier and Phone values to be saved.
- Updated saving contact to save profile image.

*Drop Offs*
- Removed SMS options for later possible implementation.

*Email Setup*
- Updated to load and save settings.

*POS Manager*
- Updated to fix Refund Popup Quantity and Change Quantity functions to parse value from Quantity label.

*Startup*
- Updated to fix to Master Shipping Table loading to convert necessary fields from string to numbers to prevent type conversion errors.
- Added special update to add index to qCloseID field in Payments and Transactions tables.

## 1.0.39 (2021-01-06)

### Enhancements

*Build Install*
- Added SmartSwiperExpress.exe to install SmartSwiper path.

*Carrier Setup*
- Added options to "Markup From Discount" for FedEx, DHL, UPS
- Added options to "ALWAYS Charge Retail" for FedEx, DHL, UPS

*Inventory*
- When checking packmaster checkbox, packaging items will be displayed on top.

*POS Refund*
- Updated Credit Card refund processing to run through SmartSwiper Express.

*Shipment History*
- Invoice number now displayed in detail screen.
- ShipTo and ShipFrom address now shows phone number in detail screen.

*Ship Manager*
- Double clicking in From/To textbox will open Contact Manager
- Customs screen will  distribute weight evenly by default.
- Retail price is now calculated from either discounted cost or published rate, depending if "Always Markup from Discount" option selected in Carrier Setup.
- If "Always Charge Retail" option is turned on in setup, use carrier Retail rates plus LEVEL R markup for selling price.

*Shipping Markups*
- Pricing Matrix should only show for PostNet stores
- Added Level R markup. LEVEL 1,2,3 markups hidden if always charge retail is enabled.

*Utilities*
- Added Backup Utility

### Fixes

*Contacts*
- Fixed: When leaving any address field empty, the change was not being saved to the contact.

*POS Manager*
- Updated Recover Invoice to load payments data.
- Updated Payments save procedure to only save new payments to database.
- Updated Print Receipt procedure for when "Change Due" or "Credits" lines are shown.
- Updated Print Receipt procedure to include saved CC info from payment if available.
- Updated Refund popup to set Quantity data type as double instead of integer.

*POS Payments*
- Updated payment buttons to only process if balance due.
- Fixed credit card button to get next invoice number if 0.
- Updated credit card processing via SmartSwiper Express to fix issues.
- Updated credit card processing via SmartSwiper Express to show message if cancelled by user.
- Updated credit card processing via SmartSwiper Express to save returned CC info after success.

*Ship Manager*
- When selecting carrier packaging, we still need to set the 'isLetter' property in order to get proper discounts.

## 1.0.38 (2020-10-06)

### Enhancements

*Build Install*
- Updated application icon to current logo box image.

*Setup*
- Added Coupon Setup to POS Setup.

*POS Manager*
- Added popup for Refunds.
- Added/Updated functions for processing refunds.
- Updated refund processing to calculate and set adjustment/refund amounts and set original invoice status to returned.
- Updated selecting POS button to use entered numeric value less than 1000 in SKU input as quantity for selected SKU button (enter quantity value in SKU input then select SKU POS button and that quantity value is applied to the SKU added to the transaction).

*POS Refund*
- Added Continue button and Posting Date textbox.
- Added/Updated functions to processing refunds.

*Startup*
- Added startup check to see if ShipriteNext.accdb database exists.
- When converting shiprite.mdb to shipritenext.accdb, then the UPS zone CSV file should also be copied over.

### Fixes

*General*
- Updated current user permission check to always return True if user is "ADMIN".

*Main Window*
- Added user permission check for POSManager when clicking POS button.

*POS Manager*
- Updated Invoice Lookup button to skip loading invoice notes for new sale invoice.
- Added Void Sale button click handler which sets status of invoice to 'VOIDED' in Transactions and Payments tables.
- Recover invoice listview should format currrency and right allign it.
- Updated Refund listview to change column "Refund Qty" to "ID".
- Updated Refund popup listview to remove column "Refund Amount".
- Updated Refund popup to add "Quick Refund" button.
- Updated Sale Options popup to change button "Quick Refund" to "Refund".
- Updated Refund popup to only open if non-refunded invoice loaded for refund.

*POS Refund*
- Removed Save button from top bar.
- Updated window to load customer data for "CASH" customers also.
- Updated processing to fix assigning check and credit card refund info.

*Shipping*
- Fixed Additional Handling charge for Weight
- Fixed: RETAIL Price for Fuel Surcharge calculation should use the retail pricing of the accessorial charges. (it was using base/published rates).
- Fixed: COST Price for Fuel Surcharge was not taking shipping rates into calculation.

*POS Open/Close*
- Added Checks, credit cards, other, cash, and paid out listviews to closing screen

*Tickler*
- Edited tickler layout to make more intuitive.
- Fixed problem with not being able to select customer in Tickler

*Inventory*
- Added descriptions to Note options in Inventory Detail screen

## 1.0.37 (2020-07-23)

### Enhancements

*Build Install*
- Updated application icon to current image.
- Added icon to be displayed in Add/Remove Programs.

*POS Setup - QuickBooks Online Setup*
- Added screens for QB Online and QB Online Setup.

*QuickBooks Online*
- Added screens for QB Online and QB Online Setup.

### Fixes

*Mailbox*
- When clicking "renew" option in POS, the starting date of the new rental period should be the end date of the previous.

*Inventory*
- Fixed "object variable or with block variable not set" error when scrolling or clicking into the Desc search field.
- Inventory Details: Fixed Tax Setup button to open the POS Department tab in the POS Setup.

*Shipping Setup - PackMaster Setup*
- SKU selection dropdown would error out and not display SKU options.

## 1.0.36 (2020-07-14)

### Enhancements

*Build Install*
- Updated SAP Crystal Reports (13.0.26 -> 13.0.27) - no changes in referenced assembly versions.

*Contacts*
- Updated window Loaded event to parse passed data for Company Name, First Name, Last Name.

*Drop Offs*
- Linked Contact manager.

*Master Shipping Table*
- Added Global Update to Markups in MST

*Package Valet*
- Linked Contact manager.

*POS Manager*
- Updated SKU input to parse text for contact name in format "LName, FName" and search contacts.

*Search Window*
- Added functionality to Add button to pass search text to Contact Manager for adding contact. 

*Ship Manager*
- Added USPS Regional Rate Box Pricing
- Added USPS Domestic Flat Rate pricing
- Added USPS International Flat Rate Pricing
- Added Dimension/Size Limits for each carrier

*Utilities*
- Added SUPPORT UTILITIES option to Utilities
- Moved "Create Update.Accdb" code to Support Utilities.
- Moved "SQL Processor" code to Support Utilities
- Added individual DB Record view to SQL processor.
- Added Backup screen to utilities (not designed yet)

### Fixes

*AR*
- Updated Ledger By Invoice list query to skip 'Change' payment records.
- Updated Ledger By Invoice and History ListView controls.
- Updated Ledger By Invoice list with calculated invoice payment, invoice balance, and running balance.

*Inventory*
- Fixed Inventory Detail button ToolTip

*PackMaster*
- Fixes to PackMaster processing.
- Updated pack item set array size (10 -> 20).

*POS Manager*
- Updates receipt quantity string format with leading zero for decimal numbers.
- Updated "Customer Lookup" button to "Customer Editor".

*POS Setup*
- Updates save process to update SalesTax policy value for default county when saving tax counties.

*Ship Manager*
- Closing Shipmaster should also close ShowPackageDetails popup
- When selecting carrier packaging, software will use the dimensions of that packaging instead of the user entered ones.
- Fixed issue where service layout and services don't get reset when rates are calculated.
- Fixed slowness when calculating rates. ASO/FASC/PN Discounts are now cached at startup.
- Excluded some tables and databases from caching at startup to speed up caching process.
- Shipmanager will now calculate and show pricing for shipments where DIM Weight exceeds 150lb.

## 1.0.35 (2020-06-02)

### Enhancements

*Ship Manager*
- Added USPS Commercial Base discount rates
- Added USPS RSA discount rates
- Added "Edit Service Markups and Settings" option when right clicking on service button
- Added Setup Options button

### Fixes

*PackMaster*
- Fixes empty string to double conversion error when transferring from Ship Manager.

## 1.0.34 (2020-06-01)

### Fixes

*General*
- When querying setup policy, perform case-insensitive field name comparison to fix duplicate index error.

*PackMaster*
- Updates PackMaster processing to handle packaging weight and fix array issues.

*Ship Manager*
- Updates PackMaster Popup to hide Item Roster button.
- Updates PackMaster Popup Accept button to process entered values.

## 1.0.33 (2020-05-29)

### Enhancements

*Build Install*
- Updated all service tables to 2020 rates.

*Ship Manager*
- Added First Class Mail International Service
- Added Discount structure for DHL, FedEx, UPS.

### Fixes

*Carrier Setup*
- Updated with the latest discount levels for all carriers.
- Tied all carrier options to database.
- Fixes error shown when loading FedEx Discount level and no level saved.

*Contacts*
- Allows more than 5 character zip code for non-US countries.

*Forms*
- Fixes certain Windows that unexpectedly ask to close application when selecting Back button.
- Fixes Setup > Shipping Setup - PackMaster Setup > Back button which closes current Window without bringing previous Window back up.

*PackMaster*
- Fixes and updates to PackMaster processing.
- Fixes to PackMaster and transferring data to POS lines.

*POS Manager*
- Fixes to PackMaster and transferring data to POS receipt when processing "SHIP1".

*Ship Manager*
- Added blank item to carrier packaging selection to allow un selecting of carrier packaging.
- Fixed Priority Mail Intl pricing to Canada
- Differentiation between Retail and Daily/Standard list rates for FedEx and UPS.
- Show package detail screen now shows discount percentage.
- Customs can be processed without Harmonized Code.
- FirstClass Flat Retail pricing should be read from USMail_Services.mdb.
- Intl: Show Print Shipping Label window after saving Customs form when selecting service.
- FedEx Date Certain Home Delivery option: Fixes issue where selected date isn't supplied in web services request causing error.

*Shipping Markups*
- Fixed FirstClass package markups save location.

## 1.0.32 (2020-05-14)

### Enhancements

*Inventory Manager*
- Made inventory listview editable with textboxes, checkboxes, and drop downs.
- Moved Add new inventory item to a separate popup.
- Created new logo for "view inventory detail".
- Moved TempDate_LV column definitions from code to XML.
- Finished Inventory.

*PackMaster*
- When clicking the SETUP button in Packmaster, it will now open the Packmaster tab instead of the generic Shipping Options tab.
- Updated weight and value TextBox controls.
- Updated to load values from POS into window controls.
- Updated Fragility level labels with reference names.
- Updated Fragility level 3 radio button to checked by default.
- Complete re-write of PackMaster processing.

*POS Manager*
- Open Packmaster window when SKU input is "PACKMASTER" and add lines to receipt.

*POS Setup*
- Programmed Credit Card Setup. Linked credentials and IP address to SmartSwiper.mdb and SmartSwiper reports.mdb.

*Setup Manager*
- Created PackMaster setup button in SETUP.

*Ship Manager*
- Updated to load package attributes before opening Packmaster window.
- Changed "parcel shipping" logo to UPS Logo in Ship1.
- New ShipManager carrier panels and buttons.
- Added USPS International pricing.
- Added UPS Canada Pricing.
- Ship1 will hide/disable carriers as selected in setup.
- Added carrier packaging option to separate dropdowns for each carrier.
- Added "Letter" selection and pricing in Ship1
- If a "Hidden" carrier is selected to be shown, it will remain shown until the shippnig screen is either closed or cleared.
- Added First Class Mail to Ship1.

*Shipping Setup*
- Added Shipping Setup. User can set order of carriers and services.
- Added Setup screen for Shipping Holidays.

*Shipping Setup - PackMaster Setup*
- Added close Button in the inventory drop down popup.
- Removed, eliminated DEFAULT FILL CUSHION and DEFAULT LABOR MINUTES from Packmaster setup.
- Updated PackMaster Setup to change Default Labor and Fill Cushion labels.

*Startup*
- Added capability to read in Holiday.txt file

*User Setup*
- Added option for Creating AR accounts.

### Fixes

*Build Install*
- Updated log4net.dll referenced by crystal reports to point to x86 assembly to fix build issues and removed log4net.dll via package.
- Updated UPS_Zones.accdb.

*Database*
- Fixed ignore lock files when loading database tables collection.

*Contacts*
- Fixed Google Maps verification.

*Inventory Manager*
- Added check for duplicates when adding in new item.

*Main Window*
- Updated Window Activated event to load default tax info.

*POS Manager*
- Fixed Invoice Notes in POSManager
- Fixed Customer Notes in POSManager

*Ship Manager*
- Fixed Object Variable error when DefaultShipFrom is empty.  ShipperContact needs to be the StoreOwner, not DefaultShipFrom.
- Updated linking to PackMaster to load settings before opening PackMaster popup.
- Fixed 1,2,3 Day freight pricing.
- Clear Screen shouldn't clear Shipper.
- Fixed where accessorial togglebuttons drop shadow remained after being unchecked.
- Fixed billable weight should always be rounded up, never down.

*Shipping Setup - PackMaster Setup*
- Inventory items would not display if Setup was not accessed from Packmaster directly. Added check to initialize datatable in case it's empty.
- Default setup values kept being re-loaded on mouse enter event, this caused changes to be reset.
- Saving "DoubleBoxThreshold" to policy would throw error that it doesn't exist. Field name is case sensitive.

*Startup*
- Fixed Names for all shipping services to match the official trademarked names of the carriers.

*User Login*
- Fixed sizing of User LogIn screen.

## 1.0.31 (2020-03-09)

## 1.0.30 (2020-01-21)

## 1.0.29 (2019-12-05)

## 1.0.28 (2019-12-04)

## 1.0.27 (2019-11-21)

## 1.0.26 (2019-11-19)

## 1.0.25 (2019-11-08)

## 1.0.24 (2019-10-17)

## 1.0.23 (2019-10-16)

## 1.0.22 (2019-09-25)

## 1.0.21 (2019-08-21)

## 1.0.20 (2019-08-20)

## 1.0.19 (2019-07-19)

## 1.0.18 (2019-07-18)

## 1.0.17 (2019-07-17)

## 1.0.16 (2019-06-17)

## 1.0.15 (2019-06-05)

## 1.0.14 (2019-05-15)

## 1.0.13 (2019-05-10)

## 1.0.12 (2019-04-26)

## 1.0.11 (2019-04-12)

## 1.0.10 (2019-03-26)

## 1.0.9 (2019-03-20)

## 1.0.8 (2019-03-20)

## 1.0.7 (2019-02-20)

## 1.0.6 (2019-02-15)

## 1.0.5 (2019-02-04)

## 1.0.4 (2019-01-18)

## 1.0.3 (2019-01-11)

## 1.0.2 (2018-10-18)

## 1.0.1 (2018-10-09)
