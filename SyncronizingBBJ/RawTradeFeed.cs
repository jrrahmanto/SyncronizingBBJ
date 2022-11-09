using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncronizingBBJ
{
    class RawTradeFeed
    {
        public int ExchangeId { get; set; }
        public int TradeFeedID { get; set; }
        public DateTime BusinessDate { get; set; }
        public DateTime TradeTime { get; set; }
        public DateTime TradeReceivedTime { get; set; }
        public int TradeTimeOffset { get; set; }
        public string TradeOptType { get; set; }
        public string ProductCode { get; set; }
        public int MonthContract { get; set; }
        public decimal Price { get; set; }
        public decimal Qty { get; set; }
        public string ExchangeRef { get; set; }
        public string SellerCMCode { get; set; }
        public string SellerEMCode { get; set; }
        public string SellerInvCode { get; set; }
        public string BuyerCMCode { get; set; }
        public string BuyerEMCode { get; set; }
        public string BuyerInvCode { get; set; }
        public int TradeStrikePrice { get; set; }
        public string ContraIndicator { get; set; }
        public string SellerInvGiveUpCode { get; set; }
        public int SellerGiveUpComm { get; set; }
        public string SellerRef { get; set; }
        public string SellerTrdType { get; set; }
        public string SellerCompTradeType { get; set; }
        public int SellerTotalLeg { get; set; }
        public string BuyerTrdType { get; set; }
        public string BuyerInvGiveUpCode { get; set; }
        public int BuyerGiveUpComm { get; set; }
        public string BuyerCompTradeType { get; set; }
        public string BuyerRef { get; set; }
        public int TradeVersion { get; set; }
        public int BuyTotLeg { get; set; }
        public string CreatedBy { get; set; }
        public string ShippingInstructionUrl { get; set; }
        public string ShippingInstructionFlag { get; set; }
        public DateTime ShippingInstructionUpdate { get; set; }
        public string ShippingInstructionFtp { get; set; }
    }
}
