using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TicketPOS.Properties;
using Brushes = System.Windows.Media.Brushes;
using FontFamily = System.Windows.Media.FontFamily;
using Point = System.Windows.Point;
using PrintDialog = System.Windows.Controls.PrintDialog;
using Size = System.Windows.Size;

namespace TicketPOS
{
    public class PrinterLogic
    {
        PrintTicket prntkt;
        Thickness marginPage = new Thickness(96);
        private object _printerCopies;
        private int _leftMargin = 25;
        private int _notesCharacters = 15;


        public void PrintPreSaleReceipt(List<SaleItem> items, string name, DateTime date, string orderNumber, string notes, string footer)
        {
            PrintDayOfTicket(items, name, date, orderNumber, "", false, false, Settings.Default.ReceiptPrinterName, true, notes, footer);
        }

        public void PrintEventDaySaleReceipt(List<SaleItem> items, string name, DateTime date, string orderNumber, string tableNumber, bool isToGo, string notes, string footer)
        {
            PrintDayOfTicket(items, name, date, orderNumber, tableNumber, isToGo, false, Settings.Default.ReceiptPrinterName, false, notes, footer);
        }

        public void PrintKitchenReceipt(List<SaleItem> items, string name, DateTime date, string orderNumber, string tableNumber, bool isToGo, bool isWalkUp, string notes)
        {
            if (items.All(x => x.Name.Contains("Raffle") || x.Name.Contains("Drink")))
            {
                return;
            }

            PrintDayOfTicket(items, name, date, orderNumber, tableNumber, isToGo, isWalkUp, Settings.Default.KitchenPrinterName, false, notes);
        }

        private void PrintDayOfTicket(List<SaleItem> items, string name, DateTime date, string orderNumber,
            string tableNumber, bool isToGo, bool isWalkUp, string printerName, bool isPrintQrCode, string notes)
        {
            PrintDayOfTicket(items, name, date, orderNumber, tableNumber, isToGo, isWalkUp, printerName, isPrintQrCode, notes, "");
        }

        private void PrintDayOfTicket(List<SaleItem> items, string name, DateTime date, string orderNumber,
            string tableNumber, bool isToGo, bool isWalkUp, string printerName, bool isPrintQrCode, string notes, string footer)
        {
            try
            {
                var dlg = new PrintDialog();
                if (!string.IsNullOrEmpty(printerName))
                {
                    try
                    {
                        dlg.PrintQueue = new PrintQueue(new PrintServer(), printerName);
                    }
                    catch
                    {
                    }
                }
                dlg.PrintTicket.CopyCount = 1;

                if (dlg.ShowDialog().GetValueOrDefault())
                {
                    // Create DrawingVisual and open DrawingContext.
                    DrawingVisual vis = new DrawingVisual();
                    DrawingContext dc = vis.RenderOpen();

                    var receiptText = "Most Precious Blood" + Environment.NewLine + "Catholic Church" + Environment.NewLine + Environment.NewLine;
                    receiptText += "Lenten Fish Fry" + Environment.NewLine + Environment.NewLine;
                    receiptText += "Presented by the" + Environment.NewLine + "Men's Fellowship" + Environment.NewLine;
                    receiptText += Environment.NewLine;
                    receiptText += "Order Number: " + Environment.NewLine + orderNumber + Environment.NewLine + Environment.NewLine;
                    receiptText += "Transaction Date: " + Environment.NewLine;
                    receiptText += DateTime.Now.ToString("MM/dd/yy HH:mm:ss") + Environment.NewLine + Environment.NewLine;
                    receiptText += "Name: " + Environment.NewLine + name.ToUpper() + Environment.NewLine + Environment.NewLine;
                    receiptText += "Seating Time: " + Environment.NewLine + date.ToString("MM/dd/yy HH:mm") + Environment.NewLine + Environment.NewLine;

                    var itemList = items.OrderByDescending(x => x.Price).ThenBy(x => x.Name).Select(x => x.Name).Distinct();
                    double runningTotal = 0;
                    foreach (var item in itemList)
                    {
                        var count = items.Count(x => x.Name == item);
                        var price = items.First(x => x.Name == item).Price;
                        var total = count * price;
                        runningTotal += total;
                        //Full Fried (5 x $5).......... $25  
                        receiptText += string.Format("{0} ({1} x ${2})....... ${3}{4}", item, count, price, total, Environment.NewLine);
                    }
                    receiptText += Environment.NewLine + Environment.NewLine + "Total Sale: $" + runningTotal;

                    if (!string.IsNullOrEmpty(notes))
                    {
                        receiptText += Environment.NewLine + Environment.NewLine + "Notes:" + Environment.NewLine;
                        if (!ShouldBreakUpNotes(notes))
                        {
                            receiptText += notes + Environment.NewLine;
                        }
                        else
                        {
                            //while (notes.Length >= _notesCharacters)
                            //{
                                //var line = "";
                                //while (line.Length < _notesCharacters && (notes.Contains(" ") || notes.Contains(Environment.NewLine)))
                                //{
                                //    var index = 0;
                                //    var delimeter = " ";
                                //    if (notes.IndexOf(" ") > 0)
                                //    {
                                //        index = notes.IndexOf(" ");
                                //    }
                                //    if (notes.IndexOf(Environment.NewLine) > 0 &&
                                //        notes.IndexOf(Environment.NewLine) < index)
                                //    {
                                //        index = notes.IndexOf(Environment.NewLine);
                                //        delimeter = Environment.NewLine;
                                //    }
                                //    if (line.Length > 0)
                                //    {
                                //        line += delimeter;
                                //    }
                                //    if (index == 0)
                                //    {
                                //        index = _notesCharacters;
                                //    }

                                //    line += notes.Substring(0, index).Trim();
                                //    notes = notes.Substring(index).Trim();
                                //    if (delimeter == Environment.NewLine)
                                //    {
                                //        break;
                                //    }
                                //}
                                receiptText += notes.Replace(" ", Environment.NewLine) + Environment.NewLine;
                            //}
                        }
                        receiptText += Environment.NewLine;
                        receiptText += Environment.NewLine;
                        receiptText += "." + Environment.NewLine;
                        receiptText += Environment.NewLine;
                    }

                    double top = 25;

                    if (!string.IsNullOrEmpty(tableNumber))
                    {
                        FormattedText formattedTextTableNumber = new FormattedText(
                            tableNumber + Environment.NewLine,
                            CultureInfo.CurrentCulture,
                            FlowDirection.LeftToRight,
                            new Typeface(new FontFamily("GenericMonospace"),
                                FontStyles.Normal, FontWeights.Bold,
                                FontStretches.Normal),
                            36, Brushes.Black);
                        Size sizeTableNumberText = new Size(formattedTextTableNumber.Width,
                            formattedTextTableNumber.Height);
                        dc.DrawText(formattedTextTableNumber, new Point(_leftMargin, top));
                        top += sizeTableNumberText.Height;
                    }

                    FormattedText formtxt = new FormattedText(
                        receiptText,
                        CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        new Typeface(new FontFamily("GenericMonospace"),
                                     FontStyles.Normal, FontWeights.Bold,
                                     FontStretches.Normal),
                        12, Brushes.Black);

                    Size sizeText = new Size(formtxt.Width, formtxt.Height);
                    dc.DrawText(formtxt, new Point(_leftMargin, top));
                    top += sizeText.Height;

                    if (isPrintQrCode)
                    {
                        var image = GetCode(orderNumber);

                        BitmapImage imgSrc = new BitmapImage();
                        imgSrc.BeginInit();
                        imgSrc.StreamSource = new MemoryStream(ImageToByte(image));
                        imgSrc.EndInit();

                        dc.DrawImage(imgSrc, new Rect(new Point(_leftMargin, top), new Point(_leftMargin + 200, 225 + top)));
                        top += 225;
                    }

                    if (isToGo)
                    {
                        FormattedText formattedTextToGo = new FormattedText(
                            Environment.NewLine + "TAKE OUT" + Environment.NewLine,
                            CultureInfo.CurrentCulture,
                            FlowDirection.LeftToRight,
                            new Typeface(new FontFamily("GenericMonospace"),
                                FontStyles.Normal, FontWeights.Bold,
                                FontStretches.Normal),
                            36, Brushes.Black);
                        dc.DrawText(formattedTextToGo, new Point(_leftMargin, top));
                        Size sizeTextToGo = new Size(formattedTextToGo.Width, formattedTextToGo.Height);
                        top += sizeTextToGo.Height;
                    }

                    if (isWalkUp)
                    {
                        FormattedText formattedTextWalkUp = new FormattedText(
                            Environment.NewLine + "WALK UP" + Environment.NewLine + Environment.NewLine,
                            CultureInfo.CurrentCulture,
                            FlowDirection.LeftToRight,
                            new Typeface(new FontFamily("GenericMonospace"),
                                FontStyles.Normal, FontWeights.Bold,
                                FontStretches.Normal),
                            36, Brushes.Black);
                        dc.DrawText(formattedTextWalkUp, new Point(_leftMargin, top));


                    }
                    if (!string.IsNullOrEmpty(footer))
                    {
                        FormattedText footerText = new FormattedText(
                            footer,
                            CultureInfo.CurrentCulture,
                            FlowDirection.LeftToRight,
                            new Typeface(new FontFamily("GenericMonospace"),
                                FontStyles.Normal, FontWeights.Bold,
                                FontStretches.Normal),
                            12, Brushes.Black);

                        dc.DrawText(footerText, new Point(_leftMargin, top));
                    }
                    dc.Close();

                    switch (dlg.PrintTicket.PageOrientation)
                    {
                        case PageOrientation.Landscape:
                            vis.Transform =
                                new RotateTransform(-90, dlg.PrintableAreaWidth / 2,
                                                         dlg.PrintableAreaWidth / 2);
                            break;

                        case PageOrientation.ReversePortrait:
                            vis.Transform =
                                new RotateTransform(180, dlg.PrintableAreaWidth / 2,
                                                         dlg.PrintableAreaHeight / 2);
                            break;

                        case PageOrientation.ReverseLandscape:
                            vis.Transform =
                                new RotateTransform(90, dlg.PrintableAreaHeight / 2,
                                                        dlg.PrintableAreaHeight / 2);
                            break;
                    }

                    // Finally, print the page.
                    dlg.PrintVisual(vis, "BB Copy");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        public void PrintSalesReport(string receiptText)
        {
            try
            {
                var dlg = new PrintDialog();
                dlg.PrintTicket.CopyCount = 1;
                if (dlg.ShowDialog().GetValueOrDefault())
                {
                    // Create DrawingVisual and open DrawingContext.
                    DrawingVisual vis = new DrawingVisual();
                    DrawingContext dc = vis.RenderOpen();

                    FormattedText formtxt = new FormattedText(
                        receiptText,
                        CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        new Typeface(new FontFamily("GenericMonospace"),
                                     FontStyles.Normal, FontWeights.Bold,
                                     FontStretches.Normal),
                        12, Brushes.Black);

                    Size sizeText = new Size(formtxt.Width, formtxt.Height);

                    dc.DrawText(formtxt, new Point(_leftMargin, 25));
                    dc.Close();

                    switch (dlg.PrintTicket.PageOrientation)
                    {
                        case PageOrientation.Landscape:
                            vis.Transform =
                                new RotateTransform(-90, dlg.PrintableAreaWidth / 2,
                                                         dlg.PrintableAreaWidth / 2);
                            break;

                        case PageOrientation.ReversePortrait:
                            vis.Transform =
                                new RotateTransform(180, dlg.PrintableAreaWidth / 2,
                                                         dlg.PrintableAreaHeight / 2);
                            break;

                        case PageOrientation.ReverseLandscape:
                            vis.Transform =
                                new RotateTransform(90, dlg.PrintableAreaHeight / 2,
                                                        dlg.PrintableAreaHeight / 2);
                            break;
                    }

                    // Finally, print the page.
                    dlg.PrintVisual(vis, "BB Copy");
                }
            }
            catch (Exception ex)
            {

            }
        }


        public Image GetCode(string text)
        {
            var qr = new Gma.QrCodeNet.Encoding.QrEncoder();
            var image = qr.GetQrCodeImage(text, 600, 600);
            return new Bitmap(image, 200, 200);
        }

        private static byte[] ImageToByte(Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }

        private bool ShouldBreakUpNotes(string notes)
        {
            try
            {
                if (notes.Length <= _notesCharacters)
                {
                    return false;
                }

                if (notes.Contains(Environment.NewLine))
                {
                    var chunks = notes.Split(Environment.NewLine.ToCharArray());
                    if (chunks.All(x => x.Length <= _notesCharacters))
                    {
                        return false;
                    }
                }
                
            }
            catch (Exception ex)
            {

            }
            return true;
        }
    }
}
