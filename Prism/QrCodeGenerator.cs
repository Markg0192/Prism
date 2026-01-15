using iText.IO.Image;
using iText.Kernel.Pdf;
//using Mono.Cecil.Cil;
//using Mono.Cecil.Mdb;
using Prism.Properties;
using QRCoder;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
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
				CreateQrCode(part.PartMark, data, qrCodeFolderPath);
			});

			//	GetDrawingsAndInsertQrCodes(drawingHandler, drawingManager, qrCodeFolderPath, data, ts, tssl);
			//	await GetDrawingsAndInsertQrCodes(qrCodeFolderPath, data, ts, tssl, drawingManager.GetDrawingByType("A").Count);
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

		private static string CreateUrlPathFromPart(string partMark, PrismProjectData data)
		{
			string projectNumber = data.ProjNumber.Substring(1, 4); //substring removes the C from the start
			return $@"http://drawingscan.severfield.com/otiswebextclient/Drawings?Contract={projectNumber}&Drawing={partMark}";
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
			var stopwatch = new System.Diagnostics.Stopwatch();

			stopwatch.Restart();
			string codePath = Path.Combine(qrCodeFolderPath, partMark + ".png");
			Console.WriteLine($"[InsertCode] Path combine for codePath took: {stopwatch.ElapsedMilliseconds} ms");

			stopwatch.Restart();
			string shortCodePath = Path.Combine(".\\Prism Packages\\QR Codes", partMark + ".png");
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

		// Cache QR code image bytes to avoid reading the same file repeatedly.
		private static readonly ConcurrentDictionary<string, byte[]> qrCodeCache = new ConcurrentDictionary<string, byte[]>();

		// Use a lock for image creation to avoid thread-safety issues in iText7.
		private static readonly object imageCreationLock = new object();

		/// <summary>
		/// Processes all PDF files in the given folder concurrently,
		/// but with a limit on the degree of parallelism.
		/// </summary>
		public static async Task ProcessPdfFilesAsync(string pdfFolderPath, string qrCodeFolderPath)
		{
			var pdfFiles = Directory.GetFiles(pdfFolderPath, "*.pdf");
			int totalFiles = pdfFiles.Length;
			int processedCount = 0;

			// Limit concurrency to a reasonable number (e.g. number of processors).
			using (var semaphore = new SemaphoreSlim(Environment.ProcessorCount))
			{
				var tasks = new List<Task>();

				foreach (var pdfFile in pdfFiles)
				{
					await semaphore.WaitAsync().ConfigureAwait(false);
					tasks.Add(Task.Run(() =>
					{
						try
						{
							ProcessSinglePdfFile(pdfFile, qrCodeFolderPath);
							int currentCount = Interlocked.Increment(ref processedCount);
							Console.WriteLine($"Processed {currentCount} of {totalFiles}: {pdfFile}");
						}
						catch (Exception ex)
						{
							Console.WriteLine($"Error processing {pdfFile}: {ex.Message}");
						}
						finally
						{
							semaphore.Release();
						}
					}));
				}
				await Task.WhenAll(tasks).ConfigureAwait(false);
			}
		}

		public static void ProcessPdfFiles(string pdfFolderPath, string qrCodeFolderPath)
		{
			var pdfFiles = Directory.GetFiles(pdfFolderPath, "*.pdf");
			int totalFiles = pdfFiles.Length;
			int processedCount = 0;

			foreach (var pdfFile in pdfFiles)
			{
				try
				{
					ProcessSinglePdfFile(pdfFile, qrCodeFolderPath);
					processedCount++;
					Console.WriteLine($"Processed {processedCount} of {totalFiles}: {pdfFile}");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error processing {pdfFile}: {ex.Message}");
				}
			}
		}


		/// <summary>
		/// Processes a single PDF file: strips off any revision suffix,
		/// reads the corresponding QR code image (using caching and a lock),
		/// inserts it into the PDF using Tekla coordinates, and saves the file.
		/// </summary>
		private static void ProcessSinglePdfFile(string pdfFilePath, string qrCodeFolderPath)
		{
			// Use the PDF file name (without extension) as the part mark.
			string partMark = Path.GetFileNameWithoutExtension(pdfFilePath);
			// Strip off the revision suffix. E.g., "101-0" => "101"
			string partMarkWithoutRevision = StripRevision(partMark);
			// Construct the expected QR code image path.
			string qrCodePath = Path.Combine(qrCodeFolderPath, partMarkWithoutRevision + ".png");

			if (!File.Exists(qrCodePath))
			{
				Console.WriteLine($"QR code image not found for {partMarkWithoutRevision}. Expected at: {qrCodePath}");
				return;
			}

			// Use a temporary memory stream for processing.
			using (var ms = new MemoryStream())
			{
				using (PdfReader reader = new PdfReader(pdfFilePath))
				using (PdfWriter writer = new PdfWriter(ms))
				using (PdfDocument pdfDoc = new PdfDocument(reader, writer))
				{
					if (pdfDoc.GetNumberOfPages() < 1)
					{
						Console.WriteLine($"PDF {pdfFilePath} has no pages.");
						return;
					}

					// For this example, we insert the QR code on the first page.
					var firstPage = pdfDoc.GetPage(1);
					var pageSize = firstPage.GetPageSize();
					float pageWidth = pageSize.GetWidth();
					float pageHeight = pageSize.GetHeight();

					Console.WriteLine($"Drawing {partMark} width {pageWidth} height {pageHeight}");

					// Determine insertion point and image size using Tekla drawing logic.
					(float x, float y, float imgWidth, float imgHeight) = GetImageProperties(pageWidth, pageHeight, partMark);

					try
					{
						// Retrieve the QR code image bytes from cache or read from disk if not cached.
						byte[] imageBytes = qrCodeCache.GetOrAdd(qrCodePath, path => File.ReadAllBytes(path));

						ImageData imageData;
						// Lock image creation to ensure thread safety.
						lock (imageCreationLock)
						{
							imageData = ImageDataFactory.Create(imageBytes);
						}

						// Create the image element from the image data.
						iText.Layout.Element.Image qrImage = new iText.Layout.Element.Image(imageData);
						// Scale and position the image.
						//qrImage.ScaleToFit(imgWidth, imgHeight);
						qrImage.ScaleAbsolute(imgWidth, imgHeight);

						qrImage.SetFixedPosition(1, x, y);

						// Create a Document instance to add the image.
						iText.Layout.Document doc = new iText.Layout.Document(pdfDoc);
						try
						{
							doc.Add(qrImage);
						}
						finally
						{
							// Explicitly close the Document.
							doc.Close();
						}
					}
					catch (Exception ex)
					{
						Console.WriteLine($"Error inserting QR code into {pdfFilePath}: {ex.Message}");
					}
				}
				// Overwrite the original PDF file with the modified content.
				File.WriteAllBytes(pdfFilePath, ms.ToArray());
			}
		}

		/// <summary>
		/// Removes the revision suffix from a part mark, if present.
		/// For example, "101-0" becomes "101".
		/// </summary>
		private static string StripRevision(string partMark)
		{
			int dashIndex = partMark.LastIndexOf('-');
			if (dashIndex >= 0)
			{
				return partMark.Substring(0, dashIndex);
			}
			return partMark;
		}

		/// <summary>
		/// Returns Tekla drawing coordinates and image size based on the page dimensions.
		/// Adjust these values to match your exact requirements.
		/// </summary>
		private static (float x, float y, float imgWidth, float imgHeight) GetImageProperties(float pageWidth, float pageHeight, string partMark)
		{
			// Default placeholder values.
			float x = 100f, y = 100f, imgWidth = 50f, imgHeight = 50f;
			const float tolerance = 5f;

			if (IsA0(pageWidth, pageHeight))
			{
				// A0 drawing.
				x = 3022.7f;
				y = 88.2f;
				imgWidth = 90.2f;
				imgHeight = 83.0f;
			}
			else if (IsA1(pageWidth, pageHeight))
			{
				// A1 drawing.
				x = 2049f;
				y = 101f;
				imgWidth = 90.2f;
				imgHeight = 83.0f;
			}
			else if (IsA2(pageWidth, pageHeight))
			{
				// A2 drawing.
				x = 1357.2f;
				y = 87f; 
				imgWidth = 90.2f;
				imgHeight = 83.0f;
			}
			else if (IsA3(pageWidth, pageHeight))
			{
				// A3 drawing.
				x = 863.8f;
				y = 87.005f;
				imgWidth = 90.2f;
				imgHeight = 83.0f;
			}
			else if (IsA0x1350(pageWidth, pageHeight))
			{
				// A3 drawing.
				x = 3528.2f;
				y = 101.2f;
				imgWidth = 90.2f;
				imgHeight = 83.0f;
			}
			else if (IsA0x1500(pageWidth, pageHeight))
			{
				// A3 drawing.
				x = 3953.5f;
				y = 101.2f;
				imgWidth = 90.2f;
				imgHeight = 83.0f;
			}
			else if (IsA0x1800(pageWidth, pageHeight))
			{
				// A3 drawing.
				x = 4804f;
				y = 101.2f;
				imgWidth = 90.2f;
				imgHeight = 83.0f;
			}
			else if (IsA0x2100(pageWidth, pageHeight))
			{
				// A3 drawing.
				x = 5654.2f;
				y = 101.2f;
				imgWidth = 90.2f;
				imgHeight = 83.0f;
			}
			else if (IsA0x2500(pageWidth, pageHeight))
			{
				// A3 drawing.
				x = 6788f;
				y = 101.2f;
				imgWidth = 90.2f;
				imgHeight = 83.0f;
			}
			// Add additional conditions as needed.
			return (x, y, imgWidth, imgHeight);
		}

		private static bool IsA3(double pageWidth, double pageHeight)
		{
			double tolerance = 10;

			return Math.Abs(pageWidth - 1190) < tolerance && Math.Abs(pageHeight - 841) < tolerance;
		}

		private static bool IsA2(double pageWidth, double pageHeight)
		{
			double tolerance = 10;

			return Math.Abs(pageWidth - 1683) < tolerance && Math.Abs(pageHeight - 1190) < tolerance;
		}

		private static bool IsA1(double pageWidth, double pageHeight)
		{
			double tolerance = 10;

			return Math.Abs(pageWidth - 2383) < tolerance && Math.Abs(pageHeight - 1683) < tolerance;
		}

		private static bool IsA0(double pageWidth, double pageHeight)
		{
			double tolerance = 10;

			return Math.Abs(pageWidth - 3370) < tolerance && Math.Abs(pageHeight - 2383) < tolerance;
		}

		private static bool IsA0x1350(double pageWidth, double pageHeight)
		{
			double tolerance = 10;

			return Math.Abs(pageWidth - 3855) < tolerance && Math.Abs(pageHeight - 2383) < tolerance;
		}

		private static bool IsA0x1500(double pageWidth, double pageHeight)
		{
			double tolerance = 10;

			return Math.Abs(pageWidth - 4280) < tolerance && Math.Abs(pageHeight - 2383) < tolerance;
		}

		private static bool IsA0x1800(double pageWidth, double pageHeight)
		{
			double tolerance = 10;

			return Math.Abs(pageWidth - 5130) < tolerance && Math.Abs(pageHeight - 2383) < tolerance;
		}

		private static bool IsA0x2100(double pageWidth, double pageHeight)
		{
			double tolerance = 10;

			return Math.Abs(pageWidth - 5981) < tolerance && Math.Abs(pageHeight - 2383) < tolerance;
		}

		private static bool IsA0x2500(double pageWidth, double pageHeight)
		{
			double tolerance = 10;

			return Math.Abs(pageWidth - 7114) < tolerance && Math.Abs(pageHeight - 2383) < tolerance;
		}


		/// <summary>
		/*		/// Processes all PDF files in the given folder by inserting matching QR codes.
				/// Each PDF’s name (without extension) is assumed to match a QR code image file name (with .png extension).
				/// </summary>
				/// <param name="pdfFolderPath">Folder containing the PDF files.</param>
				/// <param name="qrCodeFolderPath">Folder containing the QR code images.</param>
				/// <param name="progress">An IProgress instance for UI updates.</param>
				/// <returns>A Task representing the asynchronous operation.</returns>
				public static async Task ProcessPdfFilesAsync(string pdfFolderPath, string qrCodeFolderPath)
				{
					// Get all PDF file paths from the folder.
					var pdfFiles = Directory.GetFiles(pdfFolderPath, "*.pdf");
					int totalFiles = pdfFiles.Length;
					int processedCount = 0;

					// Create a task for each PDF file processing.
					var tasks = pdfFiles.Select(pdfFile => Task.Run(() =>
					{
						try
						{
							ProcessSinglePdfFile(pdfFile, qrCodeFolderPath);
							System.Threading.Interlocked.Increment(ref processedCount);
						}
						catch (Exception ex)
						{
							// Log the exception so you know what’s going wrong.
							Console.WriteLine($"Error processing {pdfFile}: {ex.Message}");
							throw; // Optionally rethrow to fail the whole operation.
						}
					})).ToArray();

					// Wait until all PDF files are processed.
					await Task.WhenAll(tasks);
				}


				public static void ProcessPdfFiles(string pdfFolderPath, string qrCodeFolderPath)
				{
					// Get all PDF file paths from the folder.
					var pdfFiles = Directory.GetFiles(pdfFolderPath, "*.pdf");
					int totalFiles = pdfFiles.Length;
					int processedCount = 0;

					// Process each PDF file on the current thread, one by one.
					foreach (var pdfFile in pdfFiles)
					{
						try
						{
							ProcessSinglePdfFile(pdfFile, qrCodeFolderPath);
							int currentCount = System.Threading.Interlocked.Increment(ref processedCount);
							Console.WriteLine($"Processed {currentCount} of {totalFiles}: {pdfFile}");
						}
						catch (Exception ex)
						{
							Console.WriteLine($"Error processing {pdfFile}: {ex.Message}");
						}
					}

					Console.WriteLine("QR code insertion complete (synchronous).");
				}

				private static readonly object imageCreationLock = new object();

				private static void ProcessSinglePdfFile(string pdfFilePath, string qrCodeFolderPath)
				{
					// Use the PDF file name (without extension) as the part mark.
					string partMark = Path.GetFileNameWithoutExtension(pdfFilePath);

					// Strip off the revision suffix. E.g., "101-0" => "101"
					string partMarkWithoutRevision = StripRevision(partMark);

					// Construct the expected QR code image path.
					string qrCodePath = Path.Combine(qrCodeFolderPath, partMarkWithoutRevision + ".png");

					if (!File.Exists(qrCodePath))
					{
						Console.WriteLine($"QR code image not found for {partMarkWithoutRevision}. Expected at: {qrCodePath}");
						return;
					}

					// Use a temporary memory stream for processing.
					using (var ms = new MemoryStream())
					{
						// Open the existing PDF for reading and writing.
						using (PdfReader reader = new PdfReader(pdfFilePath))
						using (PdfWriter writer = new PdfWriter(ms))
						using (PdfDocument pdfDoc = new PdfDocument(reader, writer))
						{
							if (pdfDoc.GetNumberOfPages() < 1)
							{
								Console.WriteLine($"PDF {pdfFilePath} has no pages.");
								return;
							}

							// For this example, we insert the QR code on the first page.
							var firstPage = pdfDoc.GetPage(1);
							var pageSize = firstPage.GetPageSize();
							float pageWidth = pageSize.GetWidth();
							float pageHeight = pageSize.GetHeight();

							// Determine insertion point and image size using Tekla drawing logic.
							(float x, float y, float imgWidth, float imgHeight) = GetImageProperties(pageWidth, pageHeight);

							try
							{
								// Read the QR code image into a byte array.
								byte[] imageBytes = File.ReadAllBytes(qrCodePath);

								// Use a lock to ensure thread safety during image data creation.
								ImageData imageData;
								lock (imageCreationLock)
								{
									imageData = ImageDataFactory.Create(imageBytes);
								}

								// Create the image element from the image data.
								iText.Layout.Element.Image qrImage = new iText.Layout.Element.Image(imageData);

								// Scale the image to fit the desired dimensions.
								qrImage.ScaleToFit(imgWidth, imgHeight);

								// Set the fixed position based on the provided Tekla coordinates.
								qrImage.SetFixedPosition(1, x, y);

								// Create a Document instance to add the image.
								// Note: iText7's Document is not IDisposable so we close it explicitly.
								iText.Layout.Document doc = new iText.Layout.Document(pdfDoc);
								try
								{
									doc.Add(qrImage);
								}
								finally
								{
									doc.Close();
								}
							}
							catch (Exception ex)
							{
								Console.WriteLine($"Error inserting QR code into {pdfFilePath}: {ex.Message}");
							}
						}

						// Overwrite the original PDF file with the modified content.
						File.WriteAllBytes(pdfFilePath, ms.ToArray());
					}
				}*


		/// <summary>
		/// Removes the revision suffix from the part mark if it exists.
		/// E.g., "101-0" => "101", "ABC-123-2" => "ABC-123"
		/// </summary>
		private static string StripRevision(string partMark)
		{
			int dashIndex = partMark.LastIndexOf('-');
			if (dashIndex >= 0)
			{
				// Return everything before the last dash
				return partMark.Substring(0, dashIndex);
			}
			return partMark;
		}


		/// <summary>
		/// Determines the image insertion coordinates and size based on the PDF page dimensions.
		/// These values mirror the Tekla drawing coordinate logic.
		/// </summary>
		/// <param name="pageWidth">Width of the PDF page (in points).</param>
		/// <param name="pageHeight">Height of the PDF page (in points).</param>
		/// <returns>A tuple containing: x coordinate, y coordinate, image width, image height.</returns>
		private static (float x, float y, float imgWidth, float imgHeight) GetImageProperties(float pageWidth, float pageHeight)
		{
			// Use a small tolerance for floating-point comparisons.
			const float tolerance = 0.01f;
			// Default placeholder values.
			float x = 100f, y = 100f, imgWidth = 50f, imgHeight = 50f;

			if (Math.Abs(pageWidth - 1152f) < tolerance && Math.Abs(pageHeight - 821f) < tolerance)
			{
				// A0 drawing size
				x = 1041.5f;
				y = 25.5f;
				imgWidth = 32.2495f;
				imgHeight = 29.7453f;
			}
			else if (Math.Abs(pageWidth - 804f) < tolerance && Math.Abs(pageHeight - 557f) < tolerance)
			{
				// A1 drawing size
				x = 693.5f;
				y = 25.5f;
				imgWidth = 32.2495f;
				imgHeight = 29.7453f;
			}
			else if (Math.Abs(pageWidth - 584f) < tolerance && Math.Abs(pageHeight - 410f) < tolerance)
			{
				// A2 drawing size
				x = 473.5f;
				y = 25.5f;
				imgWidth = 32.2495f;
				imgHeight = 29.7453f;
			}
			else if (Math.Abs(pageWidth - 410f) < tolerance && Math.Abs(pageHeight - 287f) < tolerance)
			{
				// A3 drawing size
				x = 299.5f;
				y = 25.5f;
				imgWidth = 32.2495f;
				imgHeight = 29.7453f;
			}
			else if (Math.Abs(pageWidth - 1350f) < tolerance && Math.Abs(pageHeight - 821f) < tolerance)
			{
				x = 1239.5f;
				y = 25.5f;
				imgWidth = 32.2495f;
				imgHeight = 29.7453f;
			}
			else if (Math.Abs(pageWidth - 1500f) < tolerance && Math.Abs(pageHeight - 821f) < tolerance)
			{
				x = 1389.5f;
				y = 25.5f;
				imgWidth = 32.2495f;
				imgHeight = 29.7453f;
			}
			else if (Math.Abs(pageWidth - 1800f) < tolerance && Math.Abs(pageHeight - 821f) < tolerance)
			{
				x = 1689.5f;
				y = 25.5f;
				imgWidth = 32.2495f;
				imgHeight = 29.7453f;
			}
			else if (Math.Abs(pageWidth - 2100f) < tolerance && Math.Abs(pageHeight - 821f) < tolerance)
			{
				x = 1989.5f;
				y = 25.5f;
				imgWidth = 32.2495f;
				imgHeight = 29.7453f;
			}
			else if (Math.Abs(pageWidth - 2500f) < tolerance && Math.Abs(pageHeight - 821f) < tolerance)
			{
				x = 2389.5f;
				y = 25.5f;
				imgWidth = 32.2495f;
				imgHeight = 29.7453f;
			}

			return (x, y, imgWidth, imgHeight);
		}*/
	}

}
