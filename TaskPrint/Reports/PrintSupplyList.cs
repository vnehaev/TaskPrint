using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TaskPrint.Models.Wildberries;
using TaskPrint.Services;

namespace TaskPrint.Reports
{
    internal class PrintSupplyList
    {
        private Supply Supply = new Supply();
        private List<Order> Orders = new List<Order>();
        private GroupOrderResult groupOrders = new GroupOrderResult();

        private PrintDocument printDocument = new PrintDocument();
        private OrderProcessor processor = new OrderProcessor();
        //private int currentPageIndex = 0;


        public PrintSupplyList(Supply supply) 
        {
            Supply = supply;
        }

        private async void GetSupplyOrders(string supplyId)
        {
            try
            {
                WildberriesApiService apiService = new WildberriesApiService();
                Orders = await apiService.GetSupplyOrdersAsync(supplyId);
            }
            catch (Exception)
            {
                MessageBox.Show("Нет данных.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
