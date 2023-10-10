using System.Collections.Generic;

namespace TaskPrint.Models.Wildberries
{
    class GroupOrderResult
    {
        public Dictionary<string, List<Order>> Groups { get; set; }
        public List<Order> Other { get; set; }
        public List<Order> All { get; set; }
        public string CompanyName {  get; set; }
        public string CompanyId { get; set;}
    }
}
