using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncronizingBBJ
{
    class FinancialInfo
    {
        public DateTime businessDate { get; set; }
        public string codeType { get; set; }
        public int sequence { get; set; }
        public DateTime financialInfoTime { get; set; }
        public string participantCode { get; set; }
        public string accountCode { get; set; }
        public string bondSerialNumber { get; set; }
        public string specialProductCode { get; set; }
        public string currency { get; set; }
        public decimal balance { get; set; }
        public DateTime bondHaircutExpiredDate { get; set; }

        public string brand { get; set; }
    }
}
