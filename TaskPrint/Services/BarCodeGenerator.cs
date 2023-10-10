using System.Drawing;
using ZXing;

namespace TaskPrint.Services
{
    class BarCodeGenerator
    {
        private BarcodeWriter barcodeWriter;

        public BarCodeGenerator()
        {
            barcodeWriter = new BarcodeWriter();
            barcodeWriter.Format = BarcodeFormat.EAN_13;
        }

        public Bitmap GenerateBarcode(string barcodeText, int width, int height)
        {
            barcodeWriter.Options = new ZXing.Common.EncodingOptions
            {
                Width = width,
                Height = height,
                Margin = 0,
                
            };

            return barcodeWriter.Write(barcodeText);
        }
    }
}
