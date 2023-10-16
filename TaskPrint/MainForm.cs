using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using TaskPrint.Models.Wildberries;
using TaskPrint.Services;
using Font = System.Drawing.Font;
using Image = iTextSharp.text.Image;

namespace TaskPrint
{
    public partial class MainForm : Form
    {
        private DateTime selectedDate;
        private int currentPageIndex = 0;
        private List<Order> allOrders = new List<Order>();
        private List<Order> orders = new List<Order>();
        private GroupOrderResult groupOrders = new GroupOrderResult();

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            long todayDate = ConvertToUnixTimestamp(calendarTasks.SelectionStart);
            LoadData(todayDate);
        }

        private void CalendarTasks_DateSelected(object sender, DateRangeEventArgs e)
        {
            selectedDate = calendarTasks.SelectionStart;
            long timestamp = ConvertToUnixTimestamp(selectedDate);
            LoadData(timestamp);
        }

        private async void LoadData(long selectedDate)
        {
            try
            {
                WildberriesApiService apiService = new WildberriesApiService();

                List<Supply> responseData = await apiService.GetSuppliesAsync();

                if (responseData.Count == 0)
                {
                    dataGridView1.Rows.Clear();
                    MessageBox.Show("Нет сборочных заданий.", "Нет заявок", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    responseData = responseData.OrderByDescending(supply => supply.CreatedAt).ToList();
                    responseData = responseData.Where(suply => suply.Done == false).ToList();
                    PopulateSupplyDataGridView(responseData);

                }
            }
            catch (Exception)
            {
                MessageBox.Show("Нет данных.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<List<Order>> GetSupplyOrders(string supplyId)
        {
            List<Order> responseData = new List<Order>();

            try
            {
                WildberriesApiService apiService = new WildberriesApiService();
                responseData = await apiService.GetSupplyOrdersAsync(supplyId);

                if (responseData.Count == 0)
                {
                    dataGridView1.Rows.Clear();
                    MessageBox.Show("Нет сборочных заданий.", "Нет заявок", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    responseData = responseData.OrderByDescending(order => order.CreatedAt).ToList();
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Нет данных.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return responseData;
        }

        private void PopulateSupplyDataGridView(List<Supply> suplies)
        {
            dataGridView1.Rows.Clear();
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.CellContentClick += DataGridView1_CellContentClickAsync;

            DataGridViewTextBoxColumn createdAtColumn = new DataGridViewTextBoxColumn();
            createdAtColumn.DataPropertyName = "CreatedAt";
            createdAtColumn.HeaderText = "Created At";
            createdAtColumn.ReadOnly = true;
            dataGridView1.Columns.Add(createdAtColumn);

            DataGridViewTextBoxColumn idColumn = new DataGridViewTextBoxColumn();
            idColumn.DataPropertyName = "Id";
            idColumn.HeaderText = "ID";
            idColumn.ReadOnly = true;
            dataGridView1.Columns.Add(idColumn);

            DataGridViewTextBoxColumn nameColumn = new DataGridViewTextBoxColumn();
            nameColumn.DataPropertyName = "Name";
            nameColumn.HeaderText = "Name";
            nameColumn.ReadOnly = true;
            dataGridView1.Columns.Add(nameColumn);

            DataGridViewTextBoxColumn doneColumn = new DataGridViewTextBoxColumn();
            doneColumn.DataPropertyName = "Done";
            doneColumn.HeaderText = "Done";
            doneColumn.ReadOnly = true;
            dataGridView1.Columns.Add(doneColumn);

            DataGridViewButtonColumn printButtonColumn = new DataGridViewButtonColumn();
            printButtonColumn.HeaderText = "Печать";
            printButtonColumn.Name = "PrintButton";
            printButtonColumn.Text = "Печать";
            printButtonColumn.UseColumnTextForButtonValue = true;
            dataGridView1.Columns.Add(printButtonColumn);



            dataGridView1.DataSource = suplies;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.CellClick += (sender, e) =>
            {
                if (e.RowIndex >= 0)
                {
                    dataGridView1.Rows[e.RowIndex].Selected = true;
                }
            };



        }

        private void DataGridView1_CellContentClickAsync(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dataGridView1.Columns["PrintButton"].Index)
            {

                Supply selectedOrder = (Supply)dataGridView1.Rows[e.RowIndex].DataBoundItem;
                _ = SaveSupplyToPdfAsync(selectedOrder);
                SaveStickersToPdf(selectedOrder);
            }

        }

        private async void PrintSupply(Supply suply)
        {
            PrintDocument printDocument = new PrintDocument();
            OrderProcessor processor = new OrderProcessor();


            orders = await GetSupplyOrders(suply.Id);
            groupOrders = await processor.GetGroupedOrders(orders);

            printDocument.PrintPage += (object sender, PrintPageEventArgs e) =>
            {
                Graphics g = e.Graphics;

                Font titleFont = new Font("Arial", 16, FontStyle.Bold);
                Font headingFont = new Font("Arial", 14, FontStyle.Bold);
                Font normalFont = new Font("Arial", 12);
                SolidBrush brush = new SolidBrush(Color.Black);

                float y = 20;
                float x = 20;

                g.DrawString($"{suply.Id.ToString()}", titleFont, brush, new PointF(x, y));

                y += 40;
                int totalCount = 0;
                int global_done = 0;

                foreach (var group in groupOrders.Groups)
                {
                    totalCount += group.Value.Count;
                }
                totalCount += groupOrders.Other.Count;

                if (groupOrders.Groups != null && groupOrders.Groups.Count > 0)
                {
                    g.DrawString("Группы", headingFont, brush, 20, y);
                    y += 30;


                    foreach (var group in groupOrders.Groups)
                    {

                        g.DrawString($"({group.Value.Count} шт.) {group.Key}", normalFont, brush, 40, y);
                        y += 20;

                        foreach (var order in group.Value)
                        {
                            g.DrawString("☐", normalFont, brush, 60, y);
                            g.DrawString($"{order.Stickers.PartA} {order.Stickers.PartB}", normalFont, brush, 80, y);
                            y += 20;
                        }
                        global_done += group.Value.Count;

                        g.DrawString($"({global_done} / {totalCount})", normalFont, brush, 60, y);
                        y += 30;
                    }
                }


                if (groupOrders.Other != null && groupOrders.Other.Count > 0)
                {
                    g.DrawString("Мелочь", headingFont, brush, 20, y);
                    y += 30;

                    // Create a table header
                    g.DrawString("№", headingFont, brush, 40, y);
                    g.DrawString("Номер заказа", headingFont, brush, 100, y);
                    g.DrawString("В работе", headingFont, brush, 250, y);
                    g.DrawString("Готово", headingFont, brush, 350, y);
                    g.DrawString("Название", headingFont, brush, 450, y);
                    y += 30;


                    int index = 1;
                    foreach (var order in groupOrders.Other)
                    {
                        global_done = global_done + 1;
                        g.DrawString($"{index} ({global_done} / {totalCount})", normalFont, brush, 40, y);
                        g.DrawString($"{order.Stickers.PartA} {order.Stickers.PartB}", normalFont, brush, 130, y);
                        g.DrawString("☐", normalFont, brush, 270, y);
                        g.DrawString("☐", normalFont, brush, 370, y);
                        g.DrawString(order.Article, normalFont, brush, 450, y);
                        y += 20;
                        index++;
                    }
                }

            };

            PrintPreviewDialog printPreviewDialog = new PrintPreviewDialog();
            printPreviewDialog.Document = printDocument;

            //printPreviewDialog.ShowDialog();
            //PrintDialog printDialog = new PrintDialog();
            //printDialog.Document = printDocument;
            //printDialog.AllowSelection = true;
            //printDialog.UseEXDialog = true;
            //DialogResult result = printDialog.ShowDialog();
            //PrintStickers(groupOrders);
        }

       

 

        private async Task SaveSupplyToPdfAsync(Supply supply)
        {
            Document doc = new Document();
            OrderProcessor processor = new OrderProcessor();

            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string saveDirectory = Path.Combine(appDirectory, "Reports");
            if (!Directory.Exists(saveDirectory))
            {
                Directory.CreateDirectory(saveDirectory);
            }
            string fileName = $"{supply.Id}-list.pdf";

            string filePath = Path.Combine(saveDirectory, fileName);

            orders = await GetSupplyOrders(supply.Id);
            groupOrders = await processor.GetGroupedOrders(orders);

            string fontFileName = "arialuni.ttf";
            string fontFilePath = Path.Combine("Fonts", fontFileName);

            try
            {
                PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));
                doc.Open();

                BaseFont baseFont = BaseFont.CreateFont(fontFilePath, BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);

                iTextSharp.text.Font titleFont = new iTextSharp.text.Font(baseFont, 16, iTextSharp.text.Font.BOLD);
                iTextSharp.text.Font headingFont = new iTextSharp.text.Font(baseFont, 14, iTextSharp.text.Font.NORMAL);
                iTextSharp.text.Font normalFont = new iTextSharp.text.Font(baseFont, 12);
                BaseColor textColor = BaseColor.BLACK;

                PdfContentByte pdfContent = writer.DirectContent;
                float y = doc.PageSize.Height - 20;
                float x = 20;

                pdfContent.BeginText();
                pdfContent.SetFontAndSize(baseFont, 16);
                pdfContent.SetColorFill(textColor);
                pdfContent.ShowTextAligned(Element.ALIGN_LEFT, $"{supply.Id} ({supply.CreatedAt})", x, y, 0);
                pdfContent.EndText();

                y -= 40;
                int totalCount = 0;
                int global_done = 0;

                foreach (var group in groupOrders.Groups)
                {
                    totalCount += group.Value.Count;
                }

                totalCount += groupOrders.Other.Count;
                

                if (groupOrders.Groups != null && groupOrders.Groups.Count > 0)
                {
                    // Add "Группы" heading
                    pdfContent.BeginText();
                    pdfContent.SetFontAndSize(baseFont, 14);
                    pdfContent.ShowTextAligned(Element.ALIGN_LEFT, "Группы", 20, y, 0);
                    pdfContent.EndText();
                    y -= 30;

                    // Loop through group orders
                    foreach (var group in groupOrders.Groups)
                    {
                        pdfContent.BeginText();
                        pdfContent.SetFontAndSize(baseFont, 12);
                        pdfContent.ShowTextAligned(Element.ALIGN_LEFT, $"({group.Value.Count} шт.) {group.Key}", 40, y, 0);
                        pdfContent.EndText();
                        y -= 20;

                        // Loop through orders in the group
                        foreach (var order in group.Value)
                        {
                            if (y < 20)
                            {
                                // Если осталось мало места на текущей странице, добавляем новую страницу
                                doc.NewPage();
                                y = doc.PageSize.Height - 20;
                            }
                            pdfContent.BeginText();
                            pdfContent.SetFontAndSize(baseFont, 12);
                            pdfContent.ShowTextAligned(Element.ALIGN_LEFT, "☐", 60, y, 0);
                            pdfContent.ShowTextAligned(Element.ALIGN_LEFT, $"{order.Stickers.PartA} {order.Stickers.PartB}", 80, y, 0);
                            pdfContent.EndText();
                            y -= 20;
                        }
                        global_done += group.Value.Count;

                        pdfContent.BeginText();
                        pdfContent.SetFontAndSize(baseFont, 12);
                        pdfContent.ShowTextAligned(Element.ALIGN_LEFT, $"({global_done} / {totalCount})", 60, y, 0);
                        pdfContent.EndText();
                        y -= 30;
                    }
                }
                int index = 1;
                foreach (var order in groupOrders.Other)
                {
                    if (y < 20)
                    {
                        // Если осталось мало места на текущей странице, добавляем новую страницу
                        doc.NewPage();
                        y = doc.PageSize.Height - 20;
                    }
                    global_done = global_done + 1;

                    pdfContent.BeginText();
                    pdfContent.SetFontAndSize(baseFont, 12);
                    pdfContent.ShowTextAligned(Element.ALIGN_LEFT, $"{index} ({global_done} / {totalCount})", 40, y, 0);
                    pdfContent.ShowTextAligned(Element.ALIGN_LEFT, $"{order.Stickers.PartA} {order.Stickers.PartB}", 130, y, 0);
                    pdfContent.ShowTextAligned(Element.ALIGN_LEFT, "☐", 270, y, 0);
                    pdfContent.ShowTextAligned(Element.ALIGN_LEFT, "☐", 370, y, 0);
                    pdfContent.ShowTextAligned(Element.ALIGN_LEFT, order.Article, 450, y, 0);
                    pdfContent.EndText();
                    y -= 20;
                    index++;
                }
                doc.Close();

                Process.Start("chrome.exe", filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void SaveStickersToPdf(Supply supply)
        {
            iTextSharp.text.Rectangle customPageSize = new iTextSharp.text.Rectangle(50f, 40f);
            Document doc = new Document(customPageSize);
            OrderProcessor processor = new OrderProcessor();
            WildberriesApiService apiService = new WildberriesApiService();

            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string saveDirectory = Path.Combine(appDirectory, "Reports");
            if (!Directory.Exists(saveDirectory))
            {
                Directory.CreateDirectory(saveDirectory);
            }
            string fileName = $"{supply.Id}-stickers.pdf";

            string filePath = Path.Combine(saveDirectory, fileName);

            orders = await GetSupplyOrders(supply.Id);
            groupOrders = await processor.GetGroupedOrders(orders);

            string fontFileName = "arialuni.ttf";
            string fontFilePath = Path.Combine("Fonts", fontFileName);

            try
            {
                PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));
                BaseFont baseFont = BaseFont.CreateFont(fontFilePath, BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
                iTextSharp.text.Font titleFont = new iTextSharp.text.Font(baseFont, 16, iTextSharp.text.Font.BOLD);
                iTextSharp.text.Font headingFont = new iTextSharp.text.Font(baseFont, 14, iTextSharp.text.Font.NORMAL);
                iTextSharp.text.Font normalFont = new iTextSharp.text.Font(baseFont, 12);
                iTextSharp.text.Font smallFont = new iTextSharp.text.Font(baseFont, 4f, iTextSharp.text.Font.NORMAL);
                iTextSharp.text.Font smallFontBold = new iTextSharp.text.Font(baseFont, 4f, iTextSharp.text.Font.BOLD);
                BaseColor textColor = BaseColor.BLACK;
                BaseColor barColor = BaseColor.WHITE;
                doc.Open();
                PdfContentByte pdfContent = writer.DirectContent;



                foreach (Order order in groupOrders.All) {
                    List<string> ProductCodes = new List<string>();
                    ProductCodes.Add(order.Article);

                    List<Characteristic> productCaracteristics = await apiService.GetProductInfo(ProductCodes);
                    string ProductName = null;
                    foreach (Characteristic characteristic in productCaracteristics) { 
                        if(characteristic.ProductName!= null) { ProductName = characteristic.ProductName; break; }
                    }
                    
                    doc.NewPage();
                    byte[] imageBytes = Convert.FromBase64String(order.Stickers.File);
                    Image image = Image.GetInstance(imageBytes);
                    image.SetAbsolutePosition(0, 4);
                    image.ScaleToFit(50, 40);
                    doc.Add(image);

                    doc.NewPage();

                    BarcodeEAN barcode = new BarcodeEAN
                    {
                        CodeType = Barcode.EAN13,
                        ChecksumText = true,
                        GenerateChecksum = false,
                        StartStopText = true,
                        Code = order.Skus[0],
                        BarHeight = 14f,
                        X = 2f,
                        N = 25f
                    };

                    Image imageBr = barcode.CreateImageWithBarcode(pdfContent, textColor, textColor);
                
                    imageBr.SetAbsolutePosition(5, 10);
                    Image br = Image.GetInstance(imageBr);
                    br.ScaleAbsoluteWidth(40f);
                    br.ScaleAbsoluteHeight(20f);
                    doc.Add(br);

                    List<string> pArtList = new List<string>();
                    pArtList.Add(order.Article);
                  
                    string headerText = $"{order.Article}";
                    string headerArticul = $"({order.NmId})";
                    string headerName = $"{ProductName}";

                    AppSettings settings = new AppSettings();

                    string BottomText = $"{settings.GetCompanyName()} ({settings.GetCompanyId()})";

                    Phrase aboveText = new Phrase(headerText, smallFontBold);
                    ColumnText.ShowTextAligned(writer.DirectContent, Element.ALIGN_MIDDLE, aboveText, 2f, 33f, 0);
                    Phrase belowText = new Phrase(BottomText, smallFont);
                    ColumnText.ShowTextAligned(writer.DirectContent, Element.ALIGN_LEFT, belowText, 3f, 15f, 0);
                }
                

                doc.Close();
                Process.Start("chrome.exe", filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private long ConvertToUnixTimestamp(DateTime dateTime)
        {
            return ((DateTimeOffset)dateTime).ToUnixTimeSeconds();
        }
    }

}