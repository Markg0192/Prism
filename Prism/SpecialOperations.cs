using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Prism.Enums;
using Model = Tekla.Structures.Model.Model;
using System.Windows.Forms;
using Application = Microsoft.Office.Interop.Excel.Application;
using System.IO;
using System.Diagnostics;
using Task = System.Threading.Tasks.Task;
using Tekla.Structures.Drawing;
using Tekla.Structures.Model.Operations;
using Tekla.Structures;

using iTextSharp;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Rectangle = iTextSharp.text.Rectangle;
using Font = iTextSharp.text.Font;
using Microsoft.Office.Interop.Outlook;
using System.Security.AccessControl;

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
                    File.Copy(subFile, subFile + "copy", false);
                }
            }

            foreach (string subFile in Directory.GetFiles(folderPath))
            {
                if (subFile.EndsWith("copy"))
                {
                    string directoryPath = Path.GetDirectoryName(subFile);
                    string newFilePath = Path.Combine(directoryPath, subFile.Replace("copy", ""));
                    if (File.Exists(newFilePath))
                    {
                        File.Delete(newFilePath);
                    }
                    File.Move(subFile, newFilePath);
                    File.Open(subFile, FileMode.Create);

                }
            }

            foreach (string subFile in Directory.GetFiles(folderPath))
            {
                if (subFile.EndsWith(".xsr"))
                {
                    // Create the output PDF file

                    string input = subFile;
                    string output = subFile.Replace("xsr", "pdf");

                    //Read the Data from Input File

                    StreamReader rdr = new StreamReader(input);

                    //Create a New instance on Document Class

                    //Rectangle A4Page = new Rectangle(425, 610);
                    double factor = 1.25;
                    double pageWidth = 425 * factor;
                    double pageLength = 610 * factor;
                    Rectangle A4Page = new Rectangle((int)pageWidth, (int)pageLength);
                    Document doc = new Document(A4Page, 5, 5, 5, 5);

                    //Create a New instance of PDFWriter Class for Output File

                    FileStream fs = new FileStream(subFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                    PdfWriter.GetInstance(doc, fs);
                    //new FileStream(output, FileMode.Create)
                    //Open the Document

                    doc.Open();

                    //Add the content of Text File to PDF File
                    BaseFont bf = BaseFont.CreateFont("c:\\windows\\fonts\\lucon.ttf", BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                    Font font = new Font(bf, (float)10, 0);
                    Paragraph para = new Paragraph(rdr.ReadToEnd(), font);
                    para.SetLeading((float)0.92, (float)0.92);

                    doc.Add(para);

                    //Close the Document

                    doc.Close();

                    //Open the Converted PDF File

                    System.Diagnostics.Process.Start(output);
                }
            }
        
        }

        public static void ConvertTextToPdf(string textFilePath, string pdfFilePath)
        {
            // Create a new PDF document
            Document document = new Document();

            // Create a PDF writer that writes to the specified file path
            PdfWriter.GetInstance(document, new FileStream(pdfFilePath, FileMode.Create));

            // Open the document
            document.Open();

            // Read the text file and add its content to the document
            using (StreamReader reader = new StreamReader(textFilePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    document.Add(new Paragraph(line));
                }
            }

            // Close the document
            document.Close();
        }

        public static void WriteToExcel(string cpuSpeed)
        {
            Application excel = new Application();
            Workbook workbook = excel.Workbooks.Add(Type.Missing);

            // Create a new Excel worksheet
            Worksheet worksheet = null;
            worksheet = (Worksheet)workbook.Sheets["Sheet1"];
            worksheet = (Worksheet)workbook.ActiveSheet;

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