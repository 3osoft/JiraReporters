using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;

namespace JiraToolCheckFramework.GSheets
{
   namespace FinanceSimulator.Utilities
   {
      public class GoogleSheetClient
      {
         static string[] Scopes = { SheetsService.Scope.Spreadsheets };
         static string ApplicationName = "JiraWorklogTool";
         const string ALL_COLUMNS_RANGE = "A:ZZZ";
         private const string SHEET_START_RANGE = "A:A";
         private const string GOOGLE_API_CREDENTIALS_PATH = "credentials.json";
         private const string GOOGLE_API_TOKEN_NAME = "token.json";
         private const string InterpolationTypeNumber = "Number";
         private readonly Color _redColor = new Color { Red = 1 };
         private readonly Color _greenColor = new Color { Green = 1 };

         private static SheetsService _service;
         private static SheetsService Service => _service ?? (_service = Initialize());

         public string SheetId { get; }

         public GoogleSheetClient(string sheetId)
         {
            SheetId = sheetId;
         }

         private static SheetsService Initialize()
         {
            UserCredential credential;

            using (var stream =
                new FileStream(GOOGLE_API_CREDENTIALS_PATH, FileMode.Open, FileAccess.Read))
            {
               string credPath = GOOGLE_API_TOKEN_NAME;
               credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                   GoogleClientSecrets.Load(stream).Secrets,
                   Scopes,
                   "user",
                   System.Threading.CancellationToken.None,
                   new FileDataStore(credPath, true)).Result;
               Debug.WriteLine("Credential file saved to: " + credPath);
            }
            var service = new SheetsService(new BaseClientService.Initializer()
            {
               HttpClientInitializer = credential,
               ApplicationName = ApplicationName,
            });

            return service;
         }

         public IList<IList<object>> GetSheetData(string sheetName)
         {
            string range = GetSheetAndRangeName(sheetName, ALL_COLUMNS_RANGE);
            SpreadsheetsResource.ValuesResource.GetRequest request =
                    Service.Spreadsheets.Values.Get(SheetId, range);
            ValueRange response = request.Execute();
            return response.Values;
         }

         public void WriteToSheet(string sheetName, IList<IList<object>> data)
         {
            SpreadsheetsResource.ValuesResource.AppendRequest request =
               Service.Spreadsheets.Values.Append(new ValueRange() { Values = data }, SheetId, GetSheetAndRangeName(sheetName, SHEET_START_RANGE));
            request.InsertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.OVERWRITE;
            request.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            var response = request.Execute();
         }

         public void ClearSheet(string sheetName)
         {
            ClearValuesRequest requestBody = new ClearValuesRequest();

            SpreadsheetsResource.ValuesResource.ClearRequest clearRequest = Service.Spreadsheets.Values.Clear(requestBody, SheetId, GetSheetAndRangeName(sheetName, ALL_COLUMNS_RANGE));
            clearRequest.Execute();
         }

         //TODO this metod only adds conditional formatting, there si also needed to implement delete conditional formatting method
         private void SetConditionalFormatting()
         {
            Spreadsheet spr = Service.Spreadsheets.Get(SheetId).Execute();
            Sheet sh = spr.Sheets.FirstOrDefault(s => s.Properties.Title == "attendance");
            int sheetId = (int)sh.Properties.SheetId;

            var conditionalFormatRule = new ConditionalFormatRule
            {
               GradientRule = new GradientRule
               {
                  Maxpoint = new InterpolationPoint { Color = _redColor, Type = InterpolationTypeNumber, Value = "12" },
                  Midpoint = new InterpolationPoint { Color = _greenColor, Type = InterpolationTypeNumber, Value = "8" },
                  Minpoint = new InterpolationPoint { Color = _redColor, Type = InterpolationTypeNumber, Value = "6" }
               },
               Ranges = new List<GridRange>
               {
                  new GridRange
                  {
                     SheetId = sheetId,
                     StartColumnIndex = 1,
                     StartRowIndex = 2
                  }
               }
            };

            var batchUpdateSpreadsheetRequest = new BatchUpdateSpreadsheetRequest();

            var updateCellsRequest = new Request
            {
               AddConditionalFormatRule = new AddConditionalFormatRuleRequest
               {
                  Rule = conditionalFormatRule
               },
               
            };

            batchUpdateSpreadsheetRequest.Requests = new List<Request> { updateCellsRequest};
            var bur = Service.Spreadsheets.BatchUpdate(batchUpdateSpreadsheetRequest, SheetId);
            bur.Execute();
         }

         public void DeleteAllRowsAndColumns(string sheetName)
         {

            BatchUpdateSpreadsheetRequest requestBody = new BatchUpdateSpreadsheetRequest();

            DeleteDimensionRequest deleteRowsRequest = new DeleteDimensionRequest
            {
               Range = new DimensionRange { Dimension = "ROWS", StartIndex = 0, EndIndex = 500000 }
            };

            DeleteDimensionRequest deleteColumnsRequest = new DeleteDimensionRequest
            {
               Range = new DimensionRange { Dimension = "COLUMNS", StartIndex = 0, EndIndex = 500000 }
            };
            requestBody.Requests = new List<Request>
            {
               new Request {DeleteDimension = deleteRowsRequest},
               new Request {DeleteDimension = deleteColumnsRequest}
            };

            SpreadsheetsResource.BatchUpdateRequest batchRequest =
               Service.Spreadsheets.BatchUpdate(requestBody, SheetId);

            var response = batchRequest.Execute();
         }

         private static string GetSheetAndRangeName(string sheet, string range)
         {
            return $"{sheet}!{range}";
         }
      }
   }
}