using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace TaskPrint.Reports
{
    internal class StickersToPdfConverter
    {
        public void ConvertToPdf(List<byte[]> stickerImages, string pdfFileName)
        {
            Document document = new Document();

            try
            {
                string appDirectory = Directory.GetCurrentDirectory();

                // Полный путь к PDF-файлу
                string pdfFilePath = Path.Combine(appDirectory, pdfFileName);
                PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(pdfFilePath, FileMode.Create));
                document.Open();

                foreach (byte[] stickerImageBytes in stickerImages)
                {
                    document.NewPage();
                    using (MemoryStream ms = new MemoryStream(stickerImageBytes))
                    {
                        //System.Drawing.Image stickerImage = System.Drawing.Image.GetInstance(ms);
                        //stickerImage.Alignment = Element.ALIGN_CENTER;
                        //document.Add(stickerImage);
                    }
                }

                Console.WriteLine($"PDF-файл сохранен по пути: {pdfFilePath}");
            }
            catch (Exception ex)
            {
                // Обработайте исключение, если что-то пошло не так
                Console.WriteLine($"Ошибка при создании PDF: {ex.Message}");
            }
            finally
            {
                document.Close();
            }
        }
    }
}
