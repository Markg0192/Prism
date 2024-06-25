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

namespace Prism
{
    public static  class QrCodeGenerator
    {
        private const string ServerUrl = @"https://devtest.severfield.com:44348/?fileName=1991%5CIFC%5CFB100.ifc";
        private const string ImageSavePath = "C:\\Users\\mark.gibson\\OneDrive - Severfield plc\\Desktop\\Desktop\\Pdf\\IFCs\\QRCodeImage2.png";
        private const string modelFolderPath = @"C:\TeklaStructuresModels2023\Sandbox\QR Code Generator";

        public static void ApplyQrCode(List<PrismPart> selectedParts, PrismProjectData data, string ifcPath)
        {
            foreach (PrismPart part in selectedParts)
            {
                CreateQrCode(part.Part.GetPartMark(), data, ifcPath);
            }

            GetDrawingsAndInsertQrCodes();
        }

        private static string CreateUrlPathFromPart(string partMark, PrismProjectData data)
        {
            string projectNumber = data.ProjNumber.Substring(1, 4); //substring removes the C from the start
            return $@"https://devtest.severfield.com:44348/?fileName={projectNumber}%5CIFC%5C{partMark}.ifc";
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

        private static void GenerateAndSaveQrCode(string url, string filePath)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            qrCode.GetGraphic(20).Save(filePath, ImageFormat.Png);
        }

        private static List<Drawing> GetDrawingList()
        {
            List<Drawing> drawings = new List<Drawing>();
            IEnumerable<int> drawingNos = Tekla.Structures.DrawingInternal.Operation.GetDrawingsBySelectedParts(true, true);
            foreach (var no in drawingNos)
            {
                var id = new Identifier(no);
                drawings.Add(Tekla.Structures.DrawingInternal.Operation.GetDrawing(id));
            }
            return drawings;
        }

        private static (Point, Size) GetImageProperties(Drawing drawing)
        {
            // Retrieve the width and height of the drawing's sheet
            double width = drawing.GetSheet().Width;
            double height = drawing.GetSheet().Height;

            // Initialize placeholders for insertion point and image size
            Point insertionPoint = new Point(100, 100, 0); // Default
            Size imageSize = new Size(50, 50);        // Default

            // Define conditions for each drawing size
            if (width == 1152 && height == 821) // A0
            {
                insertionPoint = new Point(859, 5, 0);
                imageSize = new Size(50, 50);
            }
            else if (width == 804 && height == 557) // A1
            {
                insertionPoint = new Point(511, 5, 0);
                imageSize = new Size(50, 50);
            }
            else if (width == 584 && height == 410) // A2
            {
                insertionPoint = new Point(291, 5, 0);
                imageSize = new Size(50, 50);
            }
            else if (width == 410 && height == 287) // A3
            {
                insertionPoint = new Point(243, 55, 0);
                imageSize = new Size(33, 33);
            }

            // Return both the insertion point and size as a tuple
            return (insertionPoint, imageSize);
        }

        private static void InsertCode(Drawing drawing)
        {
            // Define the image path
            string imagePath = "C:\\Users\\mark.gibson\\OneDrive - Severfield plc\\Desktop\\Desktop\\Pdf\\IFCs\\QRCodeImage2.png";

            // Get the image size and insertion point based on drawing
            (Point insertionPoint, Size imageSize) = GetImageProperties(drawing);

            // Create an image object
            Image qrImage = new Image(drawing.GetSheet(), insertionPoint, imageSize, imagePath);
            qrImage.Attributes.Scaling = EmbeddedObjectScalingOptions.ScaleToFit;
            // Insert the image into the drawing
            if (qrImage.Insert())
                drawing.CommitChanges();
        }

        public static void GetDrawingsAndInsertQrCodes()
        {
            DrawingHandler drawingHandler = new DrawingHandler();

            if (drawingHandler.GetConnectionStatus())
            {
                //Drawing drawing = drawingHandler.GetActiveDrawing();

                foreach (Drawing drawing in GetDrawingList())
                {
                    var drawings = drawingHandler.GetDrawingSelector();

                    if (drawing != null)
                    {
                        InsertCode(drawing);

                       /* var sheet = drawing.GetSheet();
                        var objects = sheet.GetAllObjects();

                        foreach (var item in objects)
                        {
                            if (item is Image image)
                            {
                            }
                        }*/
                    }

                    else
                    {
                        Console.WriteLine("No active drawing found!");
                    }
                }
            }
        }
    }
}