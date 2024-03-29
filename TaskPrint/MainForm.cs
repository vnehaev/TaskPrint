﻿using iTextSharp.text;
using iTextSharp.text.pdf;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private List<Order> orders = new List<Order>();
        private GroupOrderResult groupOrders = new GroupOrderResult();
        private AppSettings _appSettings = new AppSettings();

        public MainForm(AppSettings appSettings)
        {
            _appSettings = appSettings;
            InitializeComponent();
            CompanyName.Text = _appSettings.GetSelectedCompany().Name;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            if(_appSettings.GetAllCompanies().Count == 1)
            {
                CompanyName.Text = _appSettings.GetAllCompanies()[0].Name;
                companiesList.Visible = false;
            }
            else
            {
                companiesList.Visible = true;
                companiesList.DataSource = _appSettings.GetAllCompanies();
                companiesList.DisplayMember = "Name";
                companiesList.ValueMember = "Id";
                companiesList.SelectedIndexChanged += (s, ev) =>
                {
                    Company selectedCompany = (Company)companiesList.SelectedItem;
                    _appSettings.SetSelectedCompany(selectedCompany);
                    CompanyName.Text = selectedCompany.Name;
                };
            }   
            //LoadData();
        }


        private async void LoadData()
        {
            dataGridView1.DataSource = null;
            groupOrders = new GroupOrderResult();
            orders = new List<Order>();
            List<Supply> responseData = new List<Supply>();
            long next = 0;
            int limit = 1000;
            ApiResponseSupplies apiResponse = new ApiResponseSupplies();
            Company selectedCompany = _appSettings.GetSelectedCompany();
            CompanyName.Name = selectedCompany.Name;
            

            try
            {
                WildberriesApiService apiService = new WildberriesApiService(selectedCompany);

                apiResponse = await apiService.GetSuppliesAsync();
                responseData = apiResponse.Supplies;
                next = apiResponse.Next;
                if(next != 0)
                {
                    while (next != 0)
                    {
                        apiResponse = await apiService.GetSuppliesAsync(limit, (long)next);
                        responseData.AddRange(apiResponse.Supplies);
                        next = apiResponse.Next;
                    }
                }

                if (responseData.Count == 0)
                {
                    MessageBox.Show("Невозможно получить задания.", "Нет заданий", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {

                    responseData = responseData.OrderByDescending(supply => supply.CreatedAt).ToList();
                    if (onlyUnDone.Checked == true)
                    {
                        responseData = responseData.Where(suply => suply.Done == false).OrderByDescending(supply => supply.CreatedAt).ToList();

                    }
                    else {
                        //response data sorted by date
                        responseData = responseData.OrderByDescending(supply => supply.CreatedAt).ToList();
                    }
                    
                    if (responseData.Count > 0) { 
                        PopulateSupplyDataGridView(responseData);
                    }
                    else {
                        MessageBox.Show("Нет сборочных заданий для сборки.", "Нет заданий", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Загрузка данных для отображения заданий. {ex.Message}", "Ждите", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

        }

        private async Task<List<Order>> GetSupplyOrders(string supplyId)
        {
            List<Order> responseData = new List<Order>();
            Company selectedCompany = _appSettings.GetSelectedCompany();

            try
            {
                WildberriesApiService apiService = new WildberriesApiService(selectedCompany);
                responseData = await apiService.GetSupplyOrdersAsync(supplyId);

                if (responseData.Count == 0)
                {
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
            dataGridView1.DataSource = null;
            //check if exist column with button
            if (dataGridView1.Columns.Contains("PrintButton") == false)
            {
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
            }



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
            Company selectedCompany = _appSettings.GetSelectedCompany();

            WildberriesApiService apiService = new WildberriesApiService(selectedCompany);

            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string saveDirectory = Path.Combine(appDirectory, "Reports");
            if (!Directory.Exists(saveDirectory))
            {
                Directory.CreateDirectory(saveDirectory);
            }
            string fileName = $"{supply.Id}-list.pdf";

            string filePath = Path.Combine(saveDirectory, fileName);

            orders = await GetSupplyOrders(supply.Id);
            groupOrders = await processor.GetGroupedOrders(orders, selectedCompany);

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
                float x = 30;

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
                    y -= 20;

                    // Loop through group orders
                    foreach (var group in groupOrders.Groups)
                    {
                        Data itemInfo = null;
                        string ProductName = null;
                        x = 30;
                        if (y < 80)
                        {
                            doc.NewPage();
                            y = doc.PageSize.Height - 60;
                        }
                        pdfContent.BeginText();
                        pdfContent.SetFontAndSize(baseFont, 12);
                        int groupCount = group.Value.Count;
                        pdfContent.ShowTextAligned(Element.ALIGN_LEFT, $"({groupCount} шт.) {group.Key}", x, y, 0);
                        pdfContent.EndText(); 
                        y -= 20;
 

                        string productPhoto = null;
                        if (showPhotoCheckbox.Checked == true) {
                            List<string> ProductCodes = new List<string>();
                            ProductCodes.Add(group.Value[0].Article);
                            itemInfo = await apiService.GetProductInfo(ProductCodes);
                            List<string> productPhotos = itemInfo.MediaFiles;
                            
                            if (productPhotos.Count > 0) { productPhoto = productPhotos[0]; }

                            List<Characteristic> productCaracteristics = itemInfo.Characteristics;


                            foreach (Characteristic characteristic in productCaracteristics)
                            {
                                if (characteristic.ProductName != null) { ProductName = characteristic.ProductName; break; }
                            }
                        }

                        
                        //add image from productPhoto url
                        if (productPhoto != null && showPhotoCheckbox.Checked == true)
                        {
                            byte[] imageBytes;

                            using (var webClient = new System.Net.WebClient())
                            {
                                imageBytes = webClient.DownloadData(productPhoto);
                            }

                            Image image = Image.GetInstance(imageBytes);
                            image.SetAbsolutePosition(x, y - 45);
                            image.ScaleToFit(60, 60);
                            doc.Add(image);
                            y -= 30;
                            x = 160;
                        }
                        else
                        {
                            y -= 10;
                            x = 40;
                        }

                        //if is set size and name add to pdf nearby image
                        if (productPhoto != null && showPhotoCheckbox.Checked == true)
                        {
                            float mainY = y + 30;
                            if (itemInfo.Sizes.Count > 0)
                            {
                                mainY = y + 50;
                                if(itemInfo.Sizes[0].TechnicalSize != "0" && itemInfo.Sizes[0].TechnicalSize != null)
                                {
                                    pdfContent.BeginText();
                                    pdfContent.SetFontAndSize(baseFont, 12);
                                    pdfContent.ShowTextAligned(Element.ALIGN_LEFT, $"{itemInfo.Sizes[0].TechnicalSize}", x, mainY, 0);
                                    pdfContent.EndText();
                                    mainY = mainY - 10;
                                }
                            }

                            if (ProductName != null)
                            {
                                pdfContent.BeginText();
                                pdfContent.SetFontAndSize(baseFont, 12);
                                pdfContent.ShowTextAligned(Element.ALIGN_LEFT, $"{ProductName}", x, mainY, 0);
                                pdfContent.EndText();
                            }
                        }

                        // Loop through orders in the group
                        foreach (Order order in group.Value)
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
                            if (productPhoto != null && showPhotoCheckbox.Checked == true)
                            {
                                if (pz - x < 100) { x = 160; y -= 20; }
                            }
                            else
                            {
                                if (pz - x < 100) { x = 60; y -= 20; }
                            }
                                

                            pdfContent.EndText();
                        }
                        global_done += group.Value.Count;
                        
                        pdfContent.BeginText();
                        pdfContent.SetFontAndSize(baseFont, 12);
                        if(groupCount <= 12 && productPhoto != null && showPhotoCheckbox.Checked == true)
                        {
                            pdfContent.ShowTextAligned(Element.ALIGN_LEFT, $"({global_done} / {totalCount})", 30, y - 35, 0);
                        }
                        else
                        {
                            pdfContent.ShowTextAligned(Element.ALIGN_LEFT, $"({global_done} / {totalCount})", 30, y - 15, 0);
                        }
                        
                        pdfContent.EndText();

                        if (groupCount <= 12 && productPhoto != null && showPhotoCheckbox.Checked == true)
                        {
                            pdfContent.SetLineWidth(1f);
                            float x1 = 0;
                            float x2 = doc.PageSize.Width;
                            pdfContent.MoveTo(x1, y - 45);
                            pdfContent.LineTo(x2, y - 45);
                            pdfContent.Stroke();
                            y -= 65;
                        }
                        else
                        {
                            pdfContent.SetLineWidth(1f);
                            float x1 = 80;
                            float x2 = doc.PageSize.Width;
                            pdfContent.MoveTo(x1, y - 15);
                            pdfContent.LineTo(x2, y - 15);
                            pdfContent.Stroke();
                            y -= 40;
                        }

                        
                    }
                }
                int index = 1;
                foreach (var order in groupOrders.Other)
                {
                    string productPhoto = null;
                    Data itemInfo = null;
                    string Name = null;
                    string Size = null;

                    if (showPhotoCheckbox.Checked == true)
                    {
                        List<string> ProductCodes = new List<string>();
                        ProductCodes.Add(order.Article);
                        itemInfo = await apiService.GetProductInfo(ProductCodes);
                        List<string> productPhotos = itemInfo.MediaFiles;
                        if (productPhotos.Count > 0) { productPhoto = productPhotos[0]; }
                        List<Characteristic> productCaracteristics = itemInfo.Characteristics;

                        foreach (Characteristic characteristic in productCaracteristics)
                        {
                            if (characteristic.ProductName != null) { Name = characteristic.ProductName; break; }
                        }
                    }



                    int pageRemeins = 40;
                    if (productPhoto != null && showPhotoCheckbox.Checked == true)
                    {
                        pageRemeins = 60;
                    }


                        if (y < pageRemeins)
                    {
                        doc.NewPage();
                        y = doc.PageSize.Height - 20;
                    }
                    global_done = global_done + 1;
                    int startPosition = 30;

                    if(productPhoto != null && showPhotoCheckbox.Checked == true)
                    {
                        byte[] imageBytes;

                        using (var webClient = new System.Net.WebClient())
                        {
                            imageBytes = webClient.DownloadData(productPhoto);
                        }

                        Image image = Image.GetInstance(imageBytes);
                        image.SetAbsolutePosition(startPosition , y - 50);
                        image.ScaleToFit(60, 60);
                        doc.Add(image);
                        startPosition = 90;
                    }

                    if (productPhoto != null && showPhotoCheckbox.Checked == true)
                    {
                        float mainY = y + 30;
                        if (itemInfo.Sizes.Count > 0)
                        {
                            mainY = y + 50;
                            
                            if (itemInfo.Sizes[0].TechnicalSize != "0" && itemInfo.Sizes[0].TechnicalSize != null)
                            {
                                Size = itemInfo.Sizes[0].TechnicalSize;
                                mainY = mainY - 10;
                            }
                        }
                    }

                    pdfContent.BeginText();
                    pdfContent.SetFontAndSize(baseFont, 12);
                    float diffY = 20;
                    // add Size if exist and Name allways to next line
                    if (Size != null)
                    {
                        pdfContent.ShowTextAligned(Element.ALIGN_LEFT, $"Размер: {Size} ", startPosition, y, 0);
                        pdfContent.ShowTextAligned(Element.ALIGN_LEFT, $"{Name}", startPosition, y - diffY, 0);
                        diffY = 40;
                    }
                    else
                    {
                        pdfContent.ShowTextAligned(Element.ALIGN_LEFT, $"{Name}", startPosition, y, 0);
                        
                    }
                    pdfContent.ShowTextAligned(Element.ALIGN_LEFT, $"{index} ({global_done} / {totalCount})", startPosition, y- diffY, 0);
                    pdfContent.ShowTextAligned(Element.ALIGN_LEFT, $"{order.Stickers.PartA} {order.Stickers.PartB}", startPosition + 75, y - diffY, 0);
                    pdfContent.ShowTextAligned(Element.ALIGN_LEFT, "☐", startPosition + 210, y - diffY, 0);
                    pdfContent.ShowTextAligned(Element.ALIGN_LEFT, "☐", startPosition + 310, y - diffY, 0);
                    pdfContent.ShowTextAligned(Element.ALIGN_LEFT, order.Article, startPosition + 390, y - diffY, 0);
                    pdfContent.EndText();
                    //add image from productPhoto url
                    if (productPhoto != null && showPhotoCheckbox.Checked == true)
                    {
                        y -= 80;
                    }
                    else
                    {
                        y -= 20;
                    }

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
            Company selectedCompany = _appSettings.GetSelectedCompany();

            WildberriesApiService apiService = new WildberriesApiService(selectedCompany);

            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string saveDirectory = Path.Combine(appDirectory, "Reports");
            if (!Directory.Exists(saveDirectory))
            {
                Directory.CreateDirectory(saveDirectory);
            }
            string fileName = $"{supply.Id}-stickers.pdf";

            string filePath = Path.Combine(saveDirectory, fileName);

            orders = await GetSupplyOrders(supply.Id);
            groupOrders = await processor.GetGroupedOrders(orders, selectedCompany);

            string fontFileName = "arialuni.ttf";
            string fontFilePath = Path.Combine("Fonts", fontFileName);
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
            try
            {
                foreach (Order order in groupOrders.All) {
                    List<string> ProductCodes = new List<string>();
                    ProductCodes.Add(order.Article);
                    Data itemInfo = await apiService.GetProductInfo(ProductCodes);

                    List<Characteristic> productCaracteristics = itemInfo.Characteristics;
                    string ProductName = null;
                    foreach (Characteristic characteristic in productCaracteristics) { 
                        if(characteristic.ProductName!= null) { ProductName = characteristic.ProductName; break; }
                    }

                    if(ProductName == null) { 
                        ProductName = order.Article; 
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
                    string BottomText = $"{selectedCompany.Name} ({selectedCompany.Id})";

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
                doc.Close();
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private long ConvertToUnixTimestamp(DateTime dateTime)
        {
            return ((DateTimeOffset)dateTime).ToUnixTimeSeconds();
        }

        private void companiesList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Company selectedCompany = (Company)companiesList.SelectedItem;
            _appSettings.SetSelectedCompany(selectedCompany);
            LoadData();
        }

        private void button1_MouseClick(object sender, MouseEventArgs e)
        {
            LoadData();
        }

        private void onlyUnDone_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            LoadData();
        }
    }

}