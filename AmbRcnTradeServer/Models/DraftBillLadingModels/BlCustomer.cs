using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Models.DraftBillLadingModels
{
    public class BlCustomer
    {
        public ExcelCellData CompanyName { get; set; }
        public ExcelCellData Address1 { get; set; }
        public ExcelCellData Address2 { get; set; }
        public ExcelCellData Address3 { get; set; }
        public ExcelCellData Address4 { get; set; }
        public ExcelCellData Address5 { get; set; }
    }

    public class ExcelCellData
    {
        public ExcelCellData(string key, string value)
        {
            Key = key;
            Value = value;
        }
        public string Key { get; set;}
        public string Value { get; set; }

        public override string ToString()
        {
            return $"Key: {Key}, Value: {Value}";
        }
    }

    public class BillLadingCustomers
    {
        public BlCustomer Shipper { get; set; }
        public BlCustomer ForwardingAgent { get; set; }
        public BlCustomer Consignee { get; set; }
        public BlCustomer NotifyParty1 { get; set; }
        public BlCustomer NotifyParty2 { get; set; }
        
        public BlCustomer DestinationAgent { get; set; }
    }
}