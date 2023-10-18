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
                float y = doc.PageSize.Height - 60;
                float x = 40;

                pdfContent.BeginText();
                pdfContent.SetFontAndSize(baseFont, 16);
                pdfContent.SetColorFill(textColor);
                pdfContent.ShowTextAligned(Element.ALIGN_LEFT, $"{supply.Id} ({supply.CreatedAt})", x, y, 0);
                pdfContent.EndText();

                y -= 20;
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
                    pdfContent.ShowTextAligned(Element.ALIGN_LEFT, "Группы", x, y, 0);
                    pdfContent.EndText();
                    y -= 30;

                    // Loop through group orders
                    foreach (var group in groupOrders.Groups)
                    {
                        if (y < 120)
                        {
                            // Если осталось мало места на текущей странице, добавляем новую страницу
                            doc.NewPage();
                            y = doc.PageSize.Height - 60;
                        }
                        pdfContent.BeginText();
                        pdfContent.SetFontAndSize(baseFont, 12);
                        pdfContent.ShowTextAligned(Element.ALIGN_LEFT, $"({group.Value.Count} шт.) {group.Key}", 40, y, 0);
                        pdfContent.EndText();
                        y -= 20;
                        x = 60;

                        // Loop through orders in the group
                        foreach (var order in group.Value)
                        {
                            if (y < 60)
                            {
                                // Если осталось мало места на текущей странице, добавляем новую страницу
                                doc.NewPage();
                                y = doc.PageSize.Height - 60;
                            }
                            pdfContent.BeginText();
                            pdfContent.SetFontAndSize(baseFont, 12);
                            pdfContent.ShowTextAligned(Element.ALIGN_LEFT, "☐", x - 10, y, 0);
                            pdfContent.ShowTextAligned(Element.ALIGN_LEFT, $"{order.Stickers.PartA} {order.Stickers.PartB}", x, y, 0);
                            x += 100;
                            float pz = doc.PageSize.Width;
                            if(pz - x < 100) { x = 60; y -= 20; } else { }
                            pdfContent.EndText();
                        }
                        global_done += group.Value.Count;
                        
                        pdfContent.BeginText();
                        pdfContent.SetFontAndSize(baseFont, 12);
                        pdfContent.ShowTextAligned(Element.ALIGN_LEFT, $"({global_done} / {totalCount})", 40, y-15, 0);
                        pdfContent.EndText();

                        pdfContent.SetLineWidth(1f);
                        float x1 =100f;
                        float x2 = doc.PageSize.Width;
                        pdfContent.MoveTo(x1, y - 15);
                        pdfContent.LineTo(x2, y - 15);
                        pdfContent.Stroke();


                        y -= 40;
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
                iTextSharp.text.Font smallFont = new iTextSharp.text.Font(baseFont, 2f, iTextSharp.text.Font.NORMAL);
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
                        BarHeight = 20f,
                        X = 2f,
                        N = 25f
                    };

                    Image imageBr = barcode.CreateImageWithBarcode(pdfContent, textColor, textColor);
                
                    imageBr.SetAbsolutePosition(3, 5);
                    Image br = Image.GetInstance(imageBr);
                    br.ScaleAbsoluteWidth(45f);
                    br.ScaleAbsoluteHeight(20f);
                    doc.Add(br);

                    List<string> pArtList = new List<string>();
                    pArtList.Add(order.Article);

                    AppSettings settings = new AppSettings();
                    string headerText = $"{order.Article}";
                    string headerArticul = $"({order.NmId})";
                    string headerName = $"{ProductName}";
                    string BottomText = $"{settings.GetCompanyName()} ({settings.GetCompanyId()})";

                    Phrase aboveText = new Phrase(headerText, smallFontBold);
                    Phrase belowText = new Phrase(BottomText, smallFont);
                    Phrase productName = new Phrase(headerName, smallFont);
                    Phrase wbArticel = new Phrase(headerArticul, smallFontBold);

                    ColumnText.ShowTextAligned(writer.DirectContent, Element.ALIGN_LEFT, aboveText, 2f, 35f, 0);
                    ColumnText.ShowTextAligned(writer.DirectContent, Element.ALIGN_LEFT, wbArticel, 2f, 30f, 0);
                    ColumnText.ShowTextAligned(writer.DirectContent, Element.ALIGN_LEFT, productName, 2f, 26f, 0);
                    ColumnText.ShowTextAligned(writer.DirectContent, Element.ALIGN_LEFT, belowText, 2f, 2f, 0);
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