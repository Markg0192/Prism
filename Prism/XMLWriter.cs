using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Prism
{
    public static class XMLWriter
    {
        public static void PDFPrintSettings(string modelFolder)
        {
            string printFile = modelFolder + "\\attributes\\" + "PrismPDFOption.xml";
            if (!File.Exists(printFile))
            {
                XmlDocument doc = new XmlDocument();
                XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "utf-8", null);
                doc.AppendChild(xmlDeclaration);

                XmlElement root = doc.CreateElement("PdfPrintOptions");
                root.SetAttribute("Version", "1.3");
                doc.AppendChild(root);

                XmlElement options = doc.CreateElement("Options");
                root.AppendChild(options);

                XmlElement printTarget = doc.CreateElement("PrintTarget");
                printTarget.InnerText = "PDF";
                options.AppendChild(printTarget);

                XmlElement printerName = doc.CreateElement("PrinterName");
                printerName.InnerText = "Bluebeam PDF";
                options.AppendChild(printerName);

                XmlElement pdfAndPlotFileLocation = doc.CreateElement("PDFAndPlotFileLocation");
                pdfAndPlotFileLocation.InnerText = @".\Plotfiles";
                options.AppendChild(pdfAndPlotFileLocation);

                XmlElement embedFonts = doc.CreateElement("EmbedFonts");
                embedFonts.InnerText = "false";
                options.AppendChild(embedFonts);

                XmlElement openFolderWhenFinished = doc.CreateElement("OpenFolderWhenFinished");
                openFolderWhenFinished.InnerText = "true";
                options.AppendChild(openFolderWhenFinished);

                XmlElement openFileWhenFinished = doc.CreateElement("OpenFileWhenFinished");
                openFileWhenFinished.InnerText = "false";
                options.AppendChild(openFileWhenFinished);

                XmlElement outputToSingleFile = doc.CreateElement("OutputToSingleFile");
                outputToSingleFile.InnerText = "false";
                options.AppendChild(outputToSingleFile);

                XmlElement singlePDFFileName = doc.CreateElement("SinglePDFFileName");
                singlePDFFileName.InnerText = "Combined";
                options.AppendChild(singlePDFFileName);

                XmlElement plotFileExtension = doc.CreateElement("PlotFileExtension");
                options.AppendChild(plotFileExtension);

                XmlElement plotFilePrefix = doc.CreateElement("PlotFilePrefix");
                options.AppendChild(plotFilePrefix);

                XmlElement plotFileSuffix = doc.CreateElement("PlotFileSuffix");
                options.AppendChild(plotFileSuffix);

                XmlElement scalingMethod = doc.CreateElement("ScalingMethod");
                scalingMethod.InnerText = "Auto";
                options.AppendChild(scalingMethod);

                XmlElement scaleFactor = doc.CreateElement("ScaleFactor");
                scaleFactor.InnerText = "1";
                options.AppendChild(scaleFactor);

                XmlElement centerDrawingOnPaper = doc.CreateElement("CenterDrawingOnPaper");
                centerDrawingOnPaper.InnerText = "true";
                options.AppendChild(centerDrawingOnPaper);

                XmlElement printOnMultipleSheets = doc.CreateElement("PrintOnMultipleSheets");
                printOnMultipleSheets.InnerText = "false";
                options.AppendChild(printOnMultipleSheets);

                XmlElement multipleSheetOrder = doc.CreateElement("MultipleSheetOrder");
                multipleSheetOrder.InnerText = "LeftToRightTopToBottom";
                options.AppendChild(multipleSheetOrder);

                XmlElement paperSize = doc.CreateElement("PaperSize");
                paperSize.InnerText = "Auto";
                options.AppendChild(paperSize);

                XmlElement orientation = doc.CreateElement("Orientation");
                orientation.InnerText = "Auto";
                options.AppendChild(orientation);

                XmlElement colorMode = doc.CreateElement("ColorMode");
                colorMode.InnerText = "BlackAndWhite";
                options.AppendChild(colorMode);

                XmlElement numberOfCopies = doc.CreateElement("NumberOfCopies");
                numberOfCopies.InnerText = "1";
                options.AppendChild(numberOfCopies);

                // create Collate element
                XmlElement collate = doc.CreateElement("Collate");
                collate.InnerText = "false";
                options.AppendChild(collate);

                // create IncludeRevision element
                XmlElement includeRevision = doc.CreateElement("IncludeRevision");
                includeRevision.InnerText = "true";
                options.AppendChild(includeRevision);

                // create LineThicknesses element
                XmlElement lineThicknesses = doc.CreateElement("LineThicknesses");
                options.AppendChild(lineThicknesses);
                XmlElement lineThickness1 = doc.CreateElement("LineThickness");
                lineThickness1.SetAttribute("Color", "151");
                lineThickness1.SetAttribute("Pen", "0");
                lineThicknesses.AppendChild(lineThickness1);

                XmlElement lineThickness2 = doc.CreateElement("LineThickness");
                lineThickness2.SetAttribute("Color", "150");
                lineThickness2.SetAttribute("Pen", "0");
                lineThicknesses.AppendChild(lineThickness2);

                XmlElement lineThickness3 = doc.CreateElement("LineThickness");
                lineThickness3.SetAttribute("Color", "0");
                lineThickness3.SetAttribute("Pen", "0");
                lineThicknesses.AppendChild(lineThickness3);

                XmlElement lineThickness4 = doc.CreateElement("LineThickness");
                lineThickness4.SetAttribute("Color", "152");
                lineThickness4.SetAttribute("Pen", "1");
                lineThicknesses.AppendChild(lineThickness4);

                XmlElement lineThickness5 = doc.CreateElement("LineThickness");
                lineThickness5.SetAttribute("Color", "153");
                lineThickness5.SetAttribute("Pen", "0");
                lineThicknesses.AppendChild(lineThickness5);

                XmlElement lineThickness6 = doc.CreateElement("LineThickness");
                lineThickness6.SetAttribute("Color", "160");
                lineThickness6.SetAttribute("Pen", "0");
                lineThicknesses.AppendChild(lineThickness6);

                XmlElement lineThickness7 = doc.CreateElement("LineThickness");
                lineThickness7.SetAttribute("Color", "161");
                lineThickness7.SetAttribute("Pen", "2");
                lineThicknesses.AppendChild(lineThickness7);

                XmlElement lineThickness8 = doc.CreateElement("LineThickness");
                lineThickness8.SetAttribute("Color", "162");
                lineThickness8.SetAttribute("Pen", "4");
                lineThicknesses.AppendChild(lineThickness8);

                XmlElement lineThickness9 = doc.CreateElement("LineThickness");
                lineThickness9.SetAttribute("Color", "163");
                lineThickness9.SetAttribute("Pen", "3");
                lineThicknesses.AppendChild(lineThickness9);

                XmlElement lineThickness10 = doc.CreateElement("LineThickness");
                lineThickness10.SetAttribute("Color", "164");
                lineThickness10.SetAttribute("Pen", "1");
                lineThicknesses.AppendChild(lineThickness10);

                XmlElement lineThickness11 = doc.CreateElement("LineThickness");
                lineThickness11.SetAttribute("Color", "165");
                lineThickness11.SetAttribute("Pen", "3");
                lineThicknesses.AppendChild(lineThickness11);

                XmlElement lineThickness12 = doc.CreateElement("LineThickness");
                lineThickness12.SetAttribute("Color", "154");
                lineThickness12.SetAttribute("Pen", "0");
                lineThicknesses.AppendChild(lineThickness12);

                XmlElement lineThickness13 = doc.CreateElement("LineThickness");
                lineThickness13.SetAttribute("Color", "155");
                lineThickness13.SetAttribute("Pen", "2");
                lineThicknesses.AppendChild(lineThickness13);

                XmlElement lineThickness14 = doc.CreateElement("LineThickness");
                lineThickness14.SetAttribute("Color", "156");
                lineThickness14.SetAttribute("Pen", "4");
                lineThicknesses.AppendChild(lineThickness14);

                XmlElement lineThickness15 = doc.CreateElement("LineThickness");
                lineThickness15.SetAttribute("Color", "157");
                lineThickness15.SetAttribute("Pen", "3");
                lineThicknesses.AppendChild(lineThickness15);

                XmlElement lineThickness16 = doc.CreateElement("LineThickness");
                lineThickness16.SetAttribute("Color", "158");
                lineThickness16.SetAttribute("Pen", "1");
                lineThicknesses.AppendChild(lineThickness16);

                XmlElement lineThickness17 = doc.CreateElement("LineThickness");
                lineThickness17.SetAttribute("Color", "159");
                lineThickness17.SetAttribute("Pen", "2");
                lineThicknesses.AppendChild(lineThickness17);

                XmlElement lineThickness18 = doc.CreateElement("LineThickness");
                lineThickness18.SetAttribute("Color", "130");
                lineThickness18.SetAttribute("Pen", "0");
                lineThicknesses.AppendChild(lineThickness18);

                XmlElement lineThickness19 = doc.CreateElement("LineThickness");
                lineThickness19.SetAttribute("Color", "131");
                lineThickness19.SetAttribute("Pen", "0");
                lineThicknesses.AppendChild(lineThickness19);

                XmlElement lineThickness20 = doc.CreateElement("LineThickness");
                lineThickness20.SetAttribute("Color", "132");
                lineThickness20.SetAttribute("Pen", "0");
                lineThicknesses.AppendChild(lineThickness20);

                XmlElement lineThickness21 = doc.CreateElement("LineThickness");
                lineThickness21.SetAttribute("Color", "133");
                lineThickness21.SetAttribute("Pen", "0");
                lineThicknesses.AppendChild(lineThickness21);

                XmlElement lineThickness22 = doc.CreateElement("LineThickness");
                lineThickness22.SetAttribute("Color", "9");
                lineThickness22.SetAttribute("Pen", "1");
                lineThicknesses.AppendChild(lineThickness22);

                XmlElement lineThickness29 = doc.CreateElement("LineThickness");
                lineThickness29.SetAttribute("Color", "15");
                lineThickness29.SetAttribute("Pen", "1");
                lineThicknesses.AppendChild(lineThickness29);

                // create PlotColors element
                XmlElement plotColors = doc.CreateElement("PlotColors");
                options.AppendChild(plotColors);

                // create PlotColor elements
                XmlElement plotColor = doc.CreateElement("PlotColor");
                plotColor.SetAttribute("Color", "151");
                plotColor.SetAttribute("PlotColor", "000000");
                plotColors.AppendChild(plotColor);

                XmlElement plotColor2 = doc.CreateElement("PlotColor");
                plotColor2.SetAttribute("Color", "150");
                plotColor2.SetAttribute("PlotColor", "FFFFFF");
                plotColors.AppendChild(plotColor2);

                XmlElement plotColor1 = doc.CreateElement("PlotColor");
                plotColor1.SetAttribute("Color", "0");
                plotColor1.SetAttribute("PlotColor", "000000");
                plotColors.AppendChild(plotColor1);

                XmlElement plotColor30 = doc.CreateElement("PlotColor");
                plotColor30.SetAttribute("Color", "152");
                plotColor30.SetAttribute("PlotColor", "E7E7E7");
                plotColors.AppendChild(plotColor30);

                XmlElement plotColor3 = doc.CreateElement("PlotColor");
                plotColor3.SetAttribute("Color", "153");
                plotColor3.SetAttribute("PlotColor", "000000");
                plotColors.AppendChild(plotColor3);

                XmlElement plotColor4 = doc.CreateElement("PlotColor");
                plotColor4.SetAttribute("Color", "160");
                plotColor4.SetAttribute("PlotColor", "FF0000");
                plotColors.AppendChild(plotColor4);

                XmlElement plotColor5 = doc.CreateElement("PlotColor");
                plotColor5.SetAttribute("Color", "161");
                plotColor5.SetAttribute("PlotColor", "54EC54");
                plotColors.AppendChild(plotColor5);

                XmlElement plotColor6 = doc.CreateElement("PlotColor");
                plotColor6.SetAttribute("Color", "162");
                plotColor6.SetAttribute("PlotColor", "0000FF");
                plotColors.AppendChild(plotColor6);

                XmlElement plotColor7 = doc.CreateElement("PlotColor");
                plotColor7.SetAttribute("Color", "163");
                plotColor7.SetAttribute("PlotColor", "00BBE0");
                plotColors.AppendChild(plotColor7);

                XmlElement plotColor8 = doc.CreateElement("PlotColor");
                plotColor8.SetAttribute("Color", "164");
                plotColor8.SetAttribute("PlotColor", "7F7F00");
                plotColors.AppendChild(plotColor8);

                XmlElement plotColor9 = doc.CreateElement("PlotColor");
                plotColor9.SetAttribute("Color", "165");
                plotColor9.SetAttribute("PlotColor", "C400CD");
                plotColors.AppendChild(plotColor9);

                XmlElement plotColor10 = doc.CreateElement("PlotColor");
                plotColor10.SetAttribute("Color", "154");
                plotColor10.SetAttribute("PlotColor", "804040");
                plotColors.AppendChild(plotColor10);

                XmlElement plotColor11 = doc.CreateElement("PlotColor");
                plotColor11.SetAttribute("Color", "155");
                plotColor11.SetAttribute("PlotColor", "00A000");
                plotColors.AppendChild(plotColor11);

                XmlElement plotColor12 = doc.CreateElement("PlotColor");
                plotColor12.SetAttribute("Color", "156");
                plotColor12.SetAttribute("PlotColor", "333399");
                plotColors.AppendChild(plotColor12);

                XmlElement plotColor13 = doc.CreateElement("PlotColor");
                plotColor13.SetAttribute("Color", "157");
                plotColor13.SetAttribute("PlotColor", "008080");
                plotColors.AppendChild(plotColor13);

                XmlElement plotColor14 = doc.CreateElement("PlotColor");
                plotColor14.SetAttribute("Color", "158");
                plotColor14.SetAttribute("PlotColor", "FF9933");
                plotColors.AppendChild(plotColor14);

                XmlElement plotColor15 = doc.CreateElement("PlotColor");
                plotColor15.SetAttribute("Color", "159");
                plotColor15.SetAttribute("PlotColor", "706B70");
                plotColors.AppendChild(plotColor15);

                XmlElement plotColor16 = doc.CreateElement("PlotColor");
                plotColor16.SetAttribute("Color", "130");
                plotColor16.SetAttribute("PlotColor", "4C4C4C");
                plotColors.AppendChild(plotColor16);

                XmlElement plotColor17 = doc.CreateElement("PlotColor");
                plotColor17.SetAttribute("Color", "131");
                plotColor17.SetAttribute("PlotColor", "7F7F7F");
                plotColors.AppendChild(plotColor17);

                XmlElement plotColor18 = doc.CreateElement("PlotColor");
                plotColor18.SetAttribute("Color", "132");
                plotColor18.SetAttribute("PlotColor", "B2B2B2");
                plotColors.AppendChild(plotColor18);

                XmlElement plotColor19 = doc.CreateElement("PlotColor");
                plotColor19.SetAttribute("Color", "133");
                plotColor19.SetAttribute("PlotColor", "E5E5E5");
                plotColors.AppendChild(plotColor19);

                XmlElement plotColor20 = doc.CreateElement("PlotColor");
                plotColor20.SetAttribute("Color", "9");
                plotColor20.SetAttribute("PlotColor", "D8D8D8");
                plotColors.AppendChild(plotColor20);

                XmlElement plotColor29 = doc.CreateElement("PlotColor");
                plotColor29.SetAttribute("Color", "15");
                plotColor29.SetAttribute("PlotColor", "7F7F7F");
                plotColors.AppendChild(plotColor29);

                // create PreviewMinimumPenWidth element
                XmlElement previewMinimumPenWidth = doc.CreateElement("PreviewMinimumPenWidth");
                previewMinimumPenWidth.InnerText = "0.5";
                root.AppendChild(previewMinimumPenWidth);

                // Save the XML document to a file
                doc.Save(printFile);
            }
        }
    }
}
