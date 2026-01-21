using Microsoft.Office.Interop.Excel;
using System;
using System.Linq;
using Application = Microsoft.Office.Interop.Excel.Application;
using System.IO;
using System.Drawing.Printing;
using System.Drawing;

namespace Prism
{
    //This class is a place to put experimental "Special operations" used for testing and remote debugging

    public static class SpecialOperations
    {
        public static void TextToPDF(string folderPath)
        {
            foreach (string subFile in Directory.GetFiles(folderPath))
            {
                if (subFile.EndsWith(".xsr"))
                {
                    VirtualPrinter(subFile);
                    File.Delete(subFile);
                }
            }
        }

        public static void VirtualPrinter(string filePath)
        {

            System.Drawing.Font font = new System.Drawing.Font("Lucida Console", 10, FontStyle.Regular);

            string printerName = "Microsoft Print to PDF"; // name of the printer

            PrintDocument printDocument = new PrintDocument();
            printDocument.PrinterSettings.PrinterName = printerName;

            printDocument.PrinterSettings.PrintToFile = true;
            printDocument.PrinterSettings.PrintFileName = Path.ChangeExtension(filePath, "pdf");
            printDocument.DefaultPageSettings.PaperSize = new PaperSize("A4", 2100, 2970);
            printDocument.DefaultPageSettings.Margins = new Margins(40,40,40,40); // 0.5 inch margins
            printDocument.DefaultPageSettings.Landscape = false; // portrait ori0entation
            printDocument.DefaultPageSettings.Color = false; // black and white output

            printDocument.DocumentName = Path.GetFileNameWithoutExtension(filePath);

            // Variables to keep track of current position in file and number of lines printed
            int linesPerPage = 74;
            int lineNumber = 0;
            int position = 0;

            printDocument.PrintPage += (sender, e) =>
            {
                // Read a portion of the file starting from the current position
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string text = reader.ReadToEnd();
                    string[] lines = text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    int linesToPrint = Math.Min(linesPerPage, lines.Length - lineNumber);
                    string portion = string.Join(Environment.NewLine, lines.Skip(lineNumber).Take(linesToPrint));
                    e.Graphics.DrawString(portion, font, Brushes.Black, e.MarginBounds);

                    // Update variables for next page
                    lineNumber += linesToPrint;
                    position += portion.Length;
                    if (lineNumber >= lines.Length)
                    {
                        e.HasMorePages = false;
                    }
                    else
                    {
                        e.HasMorePages = true;
                    }
                }
            };        

            printDocument.Print();
        }

        public static void WriteToExcel(string cpuSpeed)
        {
            Application excel = new Application();
            Workbook workbook = excel.Workbooks.Add(Type.Missing);

            // Create a new Excel worksheet
            Worksheet worksheet = workbook.Sheets["Sheet1"] as Worksheet;

            // Write data to the Excel worksheet
            worksheet.Cells[1, 1] = "Hello";
            worksheet.Cells[1, 2] = "World";

            // Save the Excel workbook
            workbook.SaveAs("C:\\TeklaStructuresModels\\2019i models\\Gemini Colled Rolled\\example.xlsx");

            // Close the Excel workbook and application
            workbook.Close();
            excel.Quit();
        }
    }
}