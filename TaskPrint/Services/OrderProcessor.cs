using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskPrint.Models.Wildberries;

namespace TaskPrint.Services
{
    internal class OrderProcessor
    {
        public async Task<GroupOrderResult> GetGroupedOrders(List<Order> unsortedOrders, AppSettings appSettings)
        {
            Dictionary<string, List<Order>> groupedByName = new Dictionary<string, List<Order>>();
            Company company = appSettings.GetSelectedCompany();
            string apiKey = company.ApiKey;

            WildberriesApiService service = new WildberriesApiService();

            List<long> ordersIds = unsortedOrders.Select(order => order.Id).ToList();

            foreach (var order in unsortedOrders)
            {
                OrderStikers stickers = new OrderStikers();

                stickers = await service.GetOrderStickerAsync(order.Id);

                order.Stickers = stickers;

                string name = order.Article;

                ordersIds.Add(order.Id);

                if (!groupedByName.ContainsKey(name))
                {
                    groupedByName[name] = new List<Order>();
                }
                groupedByName[name].Add(order);
            }

            foreach (var orders in groupedByName.Values.Where(orders => orders.Count > 1))
            {
                unsortedOrders.RemoveAll(order => orders.Contains(order));
            }

            groupedByName = groupedByName.Where(kv => kv.Value.Count > 1)
                                     .ToDictionary(kv => kv.Key, kv => kv.Value);
            var orderedGroups = groupedByName.OrderByDescending(kv => kv.Value.Count)
                                                     .ToDictionary(kv => kv.Key, kv => kv.Value);

            List<Order> allOrders = new List<Order>();
            foreach (var group in orderedGroups)
            {
                foreach (Order order in group.Value)
                {
                    allOrders.Add(order);
                }

            }
            allOrders.AddRange(unsortedOrders);

            return new GroupOrderResult
            {
                Groups = orderedGroups,
                Other = unsortedOrders,
                All = allOrders
            };

        }
    }
}
