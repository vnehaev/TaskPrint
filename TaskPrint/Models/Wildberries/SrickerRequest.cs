using System.Collections.Generic;

namespace TaskPrint.Models.Wildberries
{
    class SrickerRequest
    {
        public List<long> Orders { get; set; }
        public string Type { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
