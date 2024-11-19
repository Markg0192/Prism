using Prism.Properties;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tekla.Structures;
using Tekla.Structures.Drawing;
using Tekla.Structures.DrawingInternal;
using Drawing = Tekla.Structures.Drawing.Drawing;
using Image = Tekla.Structures.Drawing.Image;
using Point = Tekla.Structures.Geometry3d.Point;
using Size = Tekla.Structures.Drawing.Size;

namespace Prism
{
	public static class QrCodeGenerator
	{
		private const string ServerUrl = @"https://devtest.severfield.com:44348/?fileName=1991%5CIFC%5CFB100.ifc";
		//private const string ImageSavePath = "C:\\Users\\mark.gibson\\OneDrive - Severfield plc\\Desktop\\Desktop\\Pdf\\IFCs\\QRCodeImage2.png";
		//private const string modelFolderPath = @"C:\TeklaStructuresModels2023\Sandbox\QR Code Generator";

		public static async Task ApplyQrCode(SelectedObjects selectedObjects, PrismProjectData data, string qrCodeFolderPath, DrawingManager drawingManager, ToolStrip ts, ToolStripStatusLabel tssl)
		{
			DrawingHandler drawingHandler = new DrawingHandler();
			// Create QR codes in parallel for parts where IsMainPart is true
			List<PrismPart> distinctParts = GetDistinctByPartMark(selectedObjects.GetMainParts());

			Parallel.ForEach(distinctParts, part =>
			{
				CreateQrCode(part.PartMark + "-" + part.DrawingRevision, data, qrCodeFolderPath);
			});

			//	GetDrawingsAndInsertQrCodes(drawingHandler, drawingManager, qrCodeFolderPath, data, ts, tssl);
			await GetDrawingsAndInsertQrCodes(qrCodeFolderPath, data, ts, tssl, drawingManager.GetDrawingByType("A").Count);
		}

		private static List<PrismPart> GetDistinctByPartMark(List<PrismPart> prismParts)
		{
			return prismParts
				.GroupBy(part => part.PartMark)
				.Select(group => group.First())
				.ToList();
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

		public static async Task GetDrawingsAndInsertQrCodes(string qrCodeFolderPath, PrismProjectData data, ToolStrip toolStrip, ToolStripStatusLabel statusLabel, int numberOfDrawings)
		{
			DrawingHandler drawingHandler = new DrawingHandler();

			int currentFileCount = 0;

			if (drawingHandler.GetConnectionStatus())
			{
				IEnumerable<int> drawingNos = Operation.GetDrawingsBySelectedParts(true, true);

				// Run the heavy work on a background thread
				await Task.Run(() =>
				{
					foreach (var no in drawingNos)
					{
						var id = new Identifier(no);
						var drawing = Operation.GetDrawing(id);
						if (drawing is AssemblyDrawing assDrawing)
						{
							try
							{
								// Update the status label on the UI thread
								toolStrip.Invoke(new System.Action(() =>
								{
									statusLabel.Text = $"Applying QR code: {currentFileCount} of {numberOfDrawings}";
								}));

								assDrawing.Select();
								string sanitizedMark = assDrawing.Mark.Replace(".", "").Replace("[", "").Replace("]", "");
								InsertCode(assDrawing, qrCodeFolderPath, sanitizedMark, data);
								currentFileCount++;
							}
							catch (Exception ex)
							{
								// Log the exception or handle it accordingly
								Console.WriteLine($"Error processing drawing {id}: {ex.Message}");
							}
						}
					}
				});

				// Final status update
				toolStrip.Invoke(new System.Action(() =>
				{
					statusLabel.Text = "QR code insertion complete.";
				}));
			}
		}

		private static Text.TextAttributes TextAttributes()
		{
			Text.TextAttributes myText = new Text.TextAttributes();
			myText.Font.Height = 2.0;
			myText.Font.Name = "Arial"; // Set font
			myText.Font.Color = DrawingColors.Blue; // Set color
			myText.Font.Bold = false;
			myText.Font.Italic = false;
			myText.Angle = 90;
			myText.Frame.Type = FrameTypes.Line;
			myText.Frame.Color = DrawingColors.Blue;
			myText.PreferredPlacing = PreferredMarkPlacingTypes.PointPlacingType();
			return myText;
		}

		private static string CreateUrlPathFromPart(string partMark, PrismProjectData data)
		{
			string projectNumber = data.ProjNumber.Substring(1, 4); //substring removes the C from the start
			return $@"https://devtest.severfield.com:44355/?fileName={projectNumber}%5CIFC%5C{partMark}.ifc";
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

		/*	private static void InsertCode(Drawing drawing, string qrCodeFolderPath, string partMark, PrismProjectData data)
			{
				string codePath = Path.Combine(qrCodeFolderPath, partMark + "-0" + ".png");
				string shortCodePath = Path.Combine(".\\Prism Packages\\QR Codes", partMark + "-0.png");

				if (!File.Exists(codePath)) return;

				ContainerView sheet = drawing.GetSheet();

				// Get the image size and insertion point based on drawing
				(Point insertionPoint, Size imageSize) = GetImageProperties(sheet);

				// Create an image object
				Image qrImage = new Image(sheet, insertionPoint, imageSize, shortCodePath);
				qrImage.Attributes.Scaling = EmbeddedObjectScalingOptions.ScaleToFit;

				// Insert the image into the drawing
			//	if (qrImage.Insert() && HyperlinkText(sheet, "Link - " + CreateUrlPathFromPart(partMark, data)).Insert())
					drawing.CommitChanges();
			}

				private static void InsertCode(PrismDrawing pDrawing, string qrCodeFolderPath, PrismProjectData data, List<AssemblyDrawing> drawings)
				{
					string codePath = Path.Combine(qrCodeFolderPath, pDrawing.DrawingNumber + "-" + pDrawing.DrawingRevision + ".png");
					string shortCodePath = Path.Combine(".\\Prism Packages\\QR Codes", pDrawing.DrawingNumber + "-" + pDrawing.DrawingRevision + ".png");

					if (!File.Exists(codePath)) return;

					Drawing drawing = drawings.FirstOrDefault(d => d.GetIdentifier().ID == pDrawing.DrawingId);

					ContainerView sheet = drawing.GetSheet();

					// Get the image size and insertion point based on drawing
					(Point insertionPoint, Size imageSize) = GetImageProperties(sheet);

					// Create an image object
					Image qrImage = new Image(sheet, insertionPoint, imageSize, shortCodePath);
					qrImage.Attributes.Scaling = EmbeddedObjectScalingOptions.ScaleToFit;
					// Insert the image into the drawing
					if (qrImage.Insert() &&	HyperlinkText(sheet, "Link - " + CreateUrlPathFromPart(pDrawing.DrawingNumber + "-" + pDrawing.DrawingRevision, data)).Insert())
					{ drawing.CommitChanges(); }
				}*/

		private static void InsertCode(Drawing drawing, string qrCodeFolderPath, string partMark, PrismProjectData data)
		{
			var stopwatch = new System.Diagnostics.Stopwatch();

			stopwatch.Restart();
			string codePath = Path.Combine(qrCodeFolderPath, partMark + "-0" + ".png");
			Console.WriteLine($"[InsertCode] Path combine for codePath took: {stopwatch.ElapsedMilliseconds} ms");

			stopwatch.Restart();
			string shortCodePath = Path.Combine(".\\Prism Packages\\QR Codes", partMark + "-0.png");
			Console.WriteLine($"[InsertCode] Path combine for shortCodePath took: {stopwatch.ElapsedMilliseconds} ms");

			stopwatch.Restart();
			if (!File.Exists(codePath))
			{
				Console.WriteLine($"[InsertCode] File.Exists check took: {stopwatch.ElapsedMilliseconds} ms");
				return;
			}

			stopwatch.Restart();
			ContainerView sheet = drawing.GetSheet();
			Console.WriteLine($"[InsertCode] GetSheet() call took: {stopwatch.ElapsedMilliseconds} ms");

			stopwatch.Restart();
			// Get the image size and insertion point based on drawing
			bool knownSize = false;
			(Point insertionPoint, Size imageSize) = GetImageProperties(sheet, out knownSize);
			Console.WriteLine($"[InsertCode] GetImageProperties() call took: {stopwatch.ElapsedMilliseconds} ms");

			stopwatch.Restart();
			// Create an image object
			Image qrImage = new Image(sheet, insertionPoint, imageSize, shortCodePath);
			Console.WriteLine($"[InsertCode] Image object creation took: {stopwatch.ElapsedMilliseconds} ms");

			stopwatch.Restart();
			qrImage.Attributes.Scaling = EmbeddedObjectScalingOptions.ScaleToFit;
			Console.WriteLine($"[InsertCode] Set image scaling took: {stopwatch.ElapsedMilliseconds} ms");

			stopwatch.Restart();

			// Insert the image into the drawing
			bool imageInserted = qrImage.Insert();
			Console.WriteLine($"[InsertCode] QR image insertion took: {stopwatch.ElapsedMilliseconds} ms");

			/*string url = CreateUrlPathFromPart(partMark, data);

			stopwatch.Restart();
			//bool hyperlinkInserted = HyperlinkText(sheet, url, textAttributes).Insert();
			Console.WriteLine($"[InsertCode] Hyperlink insertion took: {stopwatch.ElapsedMilliseconds} ms");*/

			stopwatch.Restart();
			if (imageInserted)// && hyperlinkInserted)
			{
				drawing.CommitChanges();
				Console.WriteLine($"[InsertCode] CommitChanges() call took: {stopwatch.ElapsedMilliseconds} ms");
			}

			Console.WriteLine($"[InsertCode] Method execution completed.");
		}
		
		private static (Point, Size) GetImageProperties(ContainerView sheet, out bool knownSize)
		{
			// Retrieve the width and height of the drawing's sheet
			double width = sheet.Width;
			double height = sheet.Height;
			knownSize = false;

			DrawingObjectEnumerator imageEnumerator = sheet.GetObjects(new Type[] { typeof(Image) });

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

			else if (width == 1350 && height == 821) // A3
			{
				insertionPoint = new Point(1239.5, 25.5, 0);
				imageSize = new Size(32.2495, 29.7453);
			}
			else if (width == 1500 && height == 821) // A3
			{
				insertionPoint = new Point(1389.5, 25.5, 0);
				imageSize = new Size(32.2495, 29.7453);
			}
			else if (width == 1800 && height == 821) // A3
			{
				insertionPoint = new Point(1689.5, 25.5, 0);
				imageSize = new Size(32.2495, 29.7453);
			}
			else if (width == 2100 && height == 821) // A3
			{
				insertionPoint = new Point(1989.5, 25.5, 0);
				imageSize = new Size(32.2495, 29.7453);
			}
			else if (width == 2500 && height == 821) // A3
			{				
				insertionPoint = new Point(2389.5, 25.5, 0);
				imageSize = new Size(32.2495, 29.7453);
			}
			// Return both the insertion point and size as a tuple
			return (insertionPoint, imageSize);
		}
	}
}