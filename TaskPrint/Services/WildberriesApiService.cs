using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using TaskPrint.Models.Wildberries;

namespace TaskPrint.Services
{
    class WildberriesApiService
    {
        private readonly HttpClient _httpClient;
        public WildberriesApiService(Company currentCompany)
        {

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {currentCompany.ApiKey}");
            _httpClient.BaseAddress = new Uri("https://suppliers-api.wildberries.ru");
        }

        public async Task<List<Order>> GetNewOrdersAsync()
        {
            try
            {
                string requestUri = $"/api/v3/orders/new";
                HttpResponseMessage response = await _httpClient.GetAsync(requestUri);

                if (response.IsSuccessStatusCode)
                {
                    string responseData = await response.Content.ReadAsStringAsync();
                    ApiResponseOrders apiResponse = JsonConvert.DeserializeObject<ApiResponseOrders>(responseData);
                    return apiResponse.Orders;
                }
                else
                {
                    throw new Exception($"Ошибка при запросе данных: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Произошла ошибка при выполнении запроса: {ex.Message}");
            }
        }

        public async Task<List<Supply>> GetSuppliesAsync(int limit = 1000, int next = 0)
        {
            try
            {
                string requestUri = $"/api/v3/supplies?limit={limit}&next={next}";
                HttpResponseMessage response = await _httpClient.GetAsync(requestUri);

                if (response.IsSuccessStatusCode)
                {
                    string responseData = await response.Content.ReadAsStringAsync();
                    ApiResponseSupplies apiResponse = JsonConvert.DeserializeObject<ApiResponseSupplies>(responseData);
                    return apiResponse.Supplies;
                }
                else
                {
                    throw new Exception($"Ошибка при запросе данных: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Произошла ошибка при выполнении запроса: {ex.Message}");
            }

        }

        public async Task<List<Order>> GetOrdersByDateAsync(long selectedDate, int limit = 1000, int next = 0)
        {
            DateTime selectedDateTime = DateTimeOffset.FromUnixTimeSeconds(selectedDate).DateTime;
            DateTime dateFrom = selectedDateTime.Date;
            DateTime dateTo = selectedDateTime.Date.AddDays(1).AddSeconds(-1);
            long dateFromTimestamp = new DateTimeOffset(dateFrom).ToUnixTimeSeconds();
            long dateToTimestamp = new DateTimeOffset(dateTo).ToUnixTimeSeconds();
            try
            {
                string requestUri = $"orders?limit={limit}&next={next}&dateFrom={dateFromTimestamp}&dateTo={dateToTimestamp}";
                HttpResponseMessage response = await _httpClient.GetAsync(requestUri);


                if (response.IsSuccessStatusCode)
                {
                    string responseData = await response.Content.ReadAsStringAsync();
                    ApiResponseOrders apiResponse = JsonConvert.DeserializeObject<ApiResponseOrders>(responseData);
                    return apiResponse.Orders;
                }
                else
                {
                    throw new Exception($"Ошибка при запросе данных: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Произошла ошибка при выполнении запроса: {ex.Message}");
            }

        }

        public async Task<List<Order>> GetSupplyOrdersAsync(string supplyId)
        {
            try
            {
                string requestUri = $"/api/v3/supplies/{supplyId}/orders";
                HttpResponseMessage response = await _httpClient.GetAsync(requestUri);

                if (response.IsSuccessStatusCode)
                {
                    string responseData = await response.Content.ReadAsStringAsync();
                    ApiResponseOrders apiResponse = JsonConvert.DeserializeObject<ApiResponseOrders>(responseData);
                    return apiResponse.Orders;
                }
                else
                {
                    throw new Exception($"Ошибка при запросе данных: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Произошла ошибка при выполнении запроса: {ex.Message}");
            }
        }

        public async Task<OrderStikers> GetOrderStickerAsync(long orderId, string fileFormat = "png")
        {
            string requestUri = $"/api/v3/orders/stickers?type={fileFormat}&width=58&height=40";
            SrickerRequest stickerRequest = new SrickerRequest
            {
                Orders = new List<long> { orderId }
            };
            string jsonData = JsonConvert.SerializeObject(stickerRequest);

            try
            {

                HttpResponseMessage response = await _httpClient.PostAsync(requestUri, new StringContent(jsonData, Encoding.UTF8, "application/json"));

                if (response.IsSuccessStatusCode)
                {
                    string responseData = await response.Content.ReadAsStringAsync();
                    ApiResponseStickers apiResponse = JsonConvert.DeserializeObject<ApiResponseStickers>(responseData);
                    return apiResponse.Stickers[0];
                }
                else
                {
                    throw new Exception($"Ошибка при запросе данных: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Произошла ошибка при выполнении запроса: {ex.Message}");
            }
        }

        public async Task<List<Characteristic>> GetProductInfo(List<string> articles)
        {
            string requestUri = $"content/v1/cards/filter";

            ProductRequest productRequest = new ProductRequest { VendorCodes = articles, AllowedCategoriesOnly = false };
            string jsonData = JsonConvert.SerializeObject(productRequest);

            try
            {

                HttpResponseMessage response = await _httpClient.PostAsync(requestUri, new StringContent(jsonData, Encoding.UTF8, "application/json"));

                if (response.IsSuccessStatusCode)
                {
                    string responseData = await response.Content.ReadAsStringAsync();
                    ApiResponseProducts apiResponse = JsonConvert.DeserializeObject<ApiResponseProducts>(responseData);
                    return apiResponse.Data[0].Characteristics;
                }
                else
                {
                    throw new Exception($"Ошибка при запросе данных: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Произошла ошибка при выполнении запроса: {ex.Message}");
            }

        }
    }
}
