using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace MedExpert.Excel
{
    public class ExcelParser
    {
        private static readonly Regex CellNameSplitter = new Regex("([A-Z]+)(\\d+)");
        
        public Dictionary<int, Dictionary<string, Tuple<string, string>>> Parse(Stream stream)
        {
            var result = new Dictionary<int, Dictionary<string, Tuple<string, string>>>();
            
            using (var doc = SpreadsheetDocument.Open(stream, false))
            {
                try
                {

                    var workbookPart = doc.WorkbookPart;
                    var sstPart = workbookPart.GetPartsOfType<SharedStringTablePart>().First();
                    var sst = sstPart.SharedStringTable;

                    var worksheetPart = workbookPart.WorksheetParts.First();
                    var sheet = worksheetPart.Worksheet;

                    var rows = sheet.Descendants<Row>();

                    foreach (var row in rows)
                    {
                        foreach (var cell in row.Elements<Cell>())
                        {
                            string cellText;
                            if (cell.DataType != null && cell.DataType == CellValues.SharedString)
                            {
                                var ssid = int.Parse(cell.CellValue.Text);
                                cellText = sst.ChildElements[ssid].InnerText;
                            }
                            else
                            {
                                cellText = cell.CellValue?.Text;
                            }

                            var rowNumber = (int) row.RowIndex.Value;
                            if (!result.ContainsKey(rowNumber))
                            {
                                result.Add(rowNumber, new Dictionary<string, Tuple<string, string>>());
                            }

                            var columnName = cell.CellReference.Value.Replace(rowNumber.ToString(), "");
                            result[rowNumber].Add(columnName, new Tuple<string, string>(cellText, null));
                        }
                    }

                    var commentsPart = worksheetPart.WorksheetCommentsPart;
                    var comments = commentsPart?.Comments.CommentList;

                    if (comments != null)
                    {
                        foreach (Comment comment in comments)
                        {
                            var cellName = comment.Reference.Value;

                            var groups = CellNameSplitter.Match(cellName).Groups;
                            var columnName = groups[1].Value;
                            var rowNumber = int.Parse(groups[2].Value);

                            var cellComment = comment.InnerText;

                            if (!result.ContainsKey(rowNumber))
                            {
                                result[rowNumber] = new Dictionary<string, Tuple<string, string>>();
                            }

                            if (!result[rowNumber].ContainsKey(columnName))
                            {
                                result[rowNumber][columnName] = new Tuple<string, string>(null, null);
                            }

                            var cellText = result[rowNumber][columnName].Item1;
                            result[rowNumber][columnName] = new Tuple<string, string>(cellText, cellComment);
                        }
                    }
                }
                catch (Exception e)
                {
                    
                }
            }

            return result;
        }
    }
}