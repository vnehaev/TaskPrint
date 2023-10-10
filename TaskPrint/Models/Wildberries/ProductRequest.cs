using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskPrint.Models.Wildberries
{
    internal class ProductRequest
    {
        public List<string> VendorCodes { get; set; }
        public bool AllowedCategoriesOnly { get; set; }
    }
}
