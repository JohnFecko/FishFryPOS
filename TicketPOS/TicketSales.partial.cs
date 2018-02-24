using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketPOS
{
    partial class TicketSale
    {
        public static string GetTitlesCSV()
        {
            return "OrderNumber,Name,EventDate,OrderGuid,IsFulfilled,IsToGo,IsWalkUp,IsValid,CreateDate" + Environment.NewLine;
        }

        public string ToCSV()
        {
            var results = "";
            results += this.OrderNumber + ",";
            results += this.Name + ",";
            results += this.EventDate + ",";
            results += this.OrderGuid + ",";
            results += this.IsFulfilled + ",";
            results += this.IsToGo + ",";
            results += this.IsWalkUp + ",";
            results += this.IsValid + ",";
            results += this.CreateDate + Environment.NewLine;

            return results;
        }
    }
}
