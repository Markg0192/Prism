using System;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Tekla.Structures.Drawing;
using Image = Tekla.Structures.Drawing.Image;
using Point = Tekla.Structures.Geometry3d.Point;
using Size = Tekla.Structures.Drawing.Size;
using System.Collections.Generic;
using Tekla.Structures;
using Drawing = Tekla.Structures.Drawing.Drawing;
using QRCoder;
using System.IO;
using System.Drawing;
using Prism.Properties;
using System.Threading.Tasks;
using System.Linq;
using System.Windows.Media;

namespace Prism
{
    public static class QrCodeGenerator
    {
        private const string ServerUrl = @"https://devtest.severfield.com:44348/?fileName=1991%5CIFC%5CFB100.ifc";
        //private const string ImageSavePath = "C:\\Users\\mark.gibson\\OneDrive - Severfield plc\\Desktop\\Desktop\\Pdf\\IFCs\\QRCodeImage2.png";
        //private const string modelFolderPath = @"C:\TeklaStructuresModels2023\Sandbox\QR Code Generator";

        public static void ApplyQrCode(List<PrismPart> selectedParts, PrismProjectData data, string qrCodeFolderPath)
        {
            // Create QR codes in parallel for parts where IsMainPart is true
            Parallel.ForEach(selectedParts.Where(part => part.IsMainPart), part =>
            {
                CreateQrCode(part.Part.GetPartMark(), data, qrCodeFolderPath);
            });

            GetDrawingsAndInsertQrCodes(qrCodeFolderPath, data);
        }

        private static void CreateQrCode(string partMark, PrismProjectData data, string ifcPath)
        {
            try
            {
                string urlPath = CreateUrlPathFromPart(partMark, data);
                GenerateAndSaveQrCode(urlPath, Path.Combine(ifcPath, partMark + ".png"));

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to create QR code: {ex.Message}");
            }
        }

        public static void GetDrawingsAndInsertQrCodes(string qrCodeFolderPath, PrismProjectData data)
        {
            DrawingHandler drawingHandler = new DrawingHandler();

            if (drawingHandler.GetConnectionStatus())
            {
                IEnumerable<int> drawingNos = Tekla.Structures.DrawingInternal.Operation.GetDrawingsBySelectedParts(true, true);
                foreach (var no in drawingNos)
                {
                    var id = new Identifier(no);
                    var drawing = Tekla.Structures.DrawingInternal.Operation.GetDrawing(id);
                    if (drawing is AssemblyDrawing assDrawing)
                    {
                        try
                        {
                            assDrawing.Select();
                            string sanitizedMark = assDrawing.Mark.Replace(".", "").Replace("[", "").Replace("]", "");
                            InsertCode(assDrawing, qrCodeFolderPath, sanitizedMark, data);
                        }
                        catch (Exception ex)
                        {
                            // Log the exception or handle it accordingly
                            Console.WriteLine($"Error processing drawing {id}: {ex.Message}");
                        }
                    }
                }
            }
        }

        private static Text HyperlinkText(ContainerView sheet, string inputText)
        {            
            Text myText = new Text(sheet, new Point(3.146, 220), inputText);

            myText.Attributes.Font.Height = 2.0;
            myText.Attributes.Font.Name = "Arial"; // Set font
            myText.Attributes.Font.Color = DrawingColors.Blue; // Set color
            myText.Attributes.Font.Bold = false;
            myText.Attributes.Font.Italic = false;
            myText.Attributes.Angle = 90;
            myText.Attributes.Frame.Type = FrameTypes.Line;
            myText.Attributes.Frame.Color = DrawingColors.Blue;
            myText.Attributes.PreferredPlacing = PreferredMarkPlacingTypes.PointPlacingType();
            return myText;
        }

        private static string CreateUrlPathFromPart(string partMark, PrismProjectData data)
        {
            string projectNumber = data.ProjNumber.Substring(1, 4); //substring removes the C from the start
            return $@"https://devtest.severfield.com:44348/?fileName={projectNumber}%5CIFC%5C{partMark}.ifc";
        }

        private static void GenerateAndSaveQrCode(string url, string filePath)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);

            using (Bitmap qrCodeImage = qrCode.GetGraphic(20))
            using (Bitmap logo = new Bitmap(Resources.SeverS))
            {
                int logoSize = qrCodeImage.Width / 5;
                int logoX = (qrCodeImage.Width - logoSize) / 2;
                int logoY = (qrCodeImage.Height - logoSize) / 2;

                using (Graphics graphics = Graphics.FromImage(qrCodeImage))
                {
                    graphics.DrawImage(logo, logoX, logoY, logoSize, logoSize);
                }

                qrCodeImage.Save(filePath, ImageFormat.Png);
            }
        }

        private static void InsertCode(Drawing drawing, string qrCodeFolderPath, string partMark, PrismProjectData data)
        {
            string codePath = Path.Combine(qrCodeFolderPath, partMark + ".png");
            string shortCodePath = Path.Combine(".\\Prism Packages\\QR Codes", partMark + ".png");

            if (!File.Exists(codePath)) return;

            ContainerView sheet = drawing.GetSheet();

            // Get the image size and insertion point based on drawing
            (Point insertionPoint, Size imageSize) = GetImageProperties(sheet);

            // Create an image object
            Image qrImage = new Image(sheet, insertionPoint, imageSize, shortCodePath);
            qrImage.Attributes.Scaling = EmbeddedObjectScalingOptions.ScaleToFit;
            // Insert the image into the drawing
            if (qrImage.Insert() && HyperlinkText(sheet, "Link - " + CreateUrlPathFromPart(partMark, data)).Insert())
                drawing.CommitChanges();
        }

        private static (Point, Size) GetImageProperties(ContainerView sheet)
        {
            // Retrieve the width and height of the drawing's sheet
            double width = sheet.Width;
            double height = sheet.Height;

            // Initialize placeholders for insertion point and image size
            Point insertionPoint = new Point(100, 100, 0); // Default
            Size imageSize = new Size(50, 50);        // Default

            // Define conditions for each drawing size
            if (width == 1152 && height == 821) // A0
            {
                insertionPoint = new Point(1041.5, 25.5, 0);
                imageSize = new Size(32.2495, 29.7453);
            }
            else if (width == 804 && height == 557) // A1
            {
                insertionPoint = new Point(693.5, 25.5, 0);
                imageSize = new Size(32.2495, 29.7453);
            }
            else if (width == 584 && height == 410) // A2
            {
                insertionPoint = new Point(473.5, 25.5, 0);
                imageSize = new Size(32.2495, 29.7453);
            }
            else if (width == 410 && height == 287) // A3
            {
                insertionPoint = new Point(299.5, 25.5, 0);
                imageSize = new Size(32.2495, 29.7453);
            }

            // Return both the insertion point and size as a tuple
            return (insertionPoint, imageSize);
        }
    }
}