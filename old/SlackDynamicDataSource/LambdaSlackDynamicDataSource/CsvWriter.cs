
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace LambdaSlackDynamicDataSource
{
    public class CsvWriter
    {
        public byte[] ConvertDataTableToCsv(DataTable dt)
        {
            var maxColumnNumber = dt.Columns.Count - 1;
            var currentRow = new List<string>(maxColumnNumber);
            var totalRowCount = dt.Rows.Count - 1;
            var currentRowNum = 0;

            var memory = new MemoryStream();

            using (var writer = new StreamWriter(memory, Encoding.ASCII))
            {
                while (currentRowNum <= totalRowCount)
                {
                    BuildRowFromDataTable(dt, currentRow, currentRowNum, maxColumnNumber);
                    WriteRecordToFile(currentRow, writer, currentRowNum, totalRowCount);
                    currentRow.Clear();
                    currentRowNum++;
                }
            }

            return memory.ToArray();
        }

        private void BuildRowFromDataTable(
            DataTable dt,
            List<string> currentRow,
            int currentRowNum,
            int maxColumnNumber)
        {
            for (int i = 0; i <= maxColumnNumber; i++)
            {
                var cell = dt.Rows[currentRowNum][i].ToString();
                if (cell == null)
                {
                    AddCellValue(string.Empty, currentRow);
                }
                else
                {
                    if (cell == null)
                    {
                        cell = string.Empty;
                    }

                    AddCellValue(cell, currentRow);
                }
            }
        }

        private void AddCellValue(string s, List<string> record)
        {
            if (s.Contains("000+0000") && s.Contains("T"))
            {
                s = s.Replace("T", " ").Replace(".000+0000", " ");
            }

            record.Add(string.Format("{0}{1}{0}", '"', s));
        }

        private void WriteRecordToFile(
            List<string> record,
            StreamWriter sw,
            int rowNumber,
            int totalRowCount)
        {
            var commaDelimitedRecord = ConvertListToDelimitedString(
                    list: record,
                    delimiter: ",",
                    insertSpaces: false,
                    qualifier: "",
                    duplicateTicksForSQL: false);

            if (rowNumber == totalRowCount)
            {
                sw.Write(commaDelimitedRecord);
            }
            else
            {
                sw.WriteLine(commaDelimitedRecord);
            }
        }

        public string ConvertListToDelimitedString(
            List<string> list,
            string delimiter = ":",
            bool insertSpaces = false,
            string qualifier = "",
            bool duplicateTicksForSQL = false)
        {
            var result = new StringBuilder();
            for (int i = 0; i < list.Count; i++)
            {
                string initialStr = duplicateTicksForSQL ? DuplicateTicksForSql(list[i]) : list[i];
                result.Append((qualifier == string.Empty)
                    ? initialStr
                    : string.Format("{1}{0}{1}", initialStr, qualifier));
                if (i < list.Count - 1)
                {
                    result.Append(delimiter);
                    if (insertSpaces)
                    {
                        result.Append(' ');
                    }
                }
            }
            return result.ToString();
        }

        private string DuplicateTicksForSql(string s)
        {
            return s.Replace("'", "''");
        }
    }
}
