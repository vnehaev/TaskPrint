using System.Drawing;
using ZXing;
using ZXing.QrCode;

namespace TaskPrint.Services
{
    class QrCodeGenerator
    {
        private BarcodeWriter qrCodeWriter;

        public QrCodeGenerator()
        {
            qrCodeWriter = new BarcodeWriter();
            qrCodeWriter.Format = BarcodeFormat.QR_CODE;
        }

        public Bitmap GenerateQrCode(string qrCodeText, int width, int height)
        {
            // Create a QR code image with the specified dimensions
            qrCodeWriter.Options = new QrCodeEncodingOptions
            {
                Width = width,
                Height = height,
                DisableECI = true,
                CharacterSet = "UTF-8",
            };

            return qrCodeWriter.Write(qrCodeText);
        }


    }
}
