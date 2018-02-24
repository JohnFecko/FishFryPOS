using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketPOS
{
    partial class TicketDetail
    {
        public static string GetTitlesCSV()
        {
            return "OrderGuid,ItemName,ItemPrice" + Environment.NewLine;
        }

        public string ToCSV()
        {
            var results = "";
            results += this.OrderGuid + ",";
            results += this.ItemName + ",";
            results += this.ItemPrice + Environment.NewLine;

            return results;
        }
    }
}
