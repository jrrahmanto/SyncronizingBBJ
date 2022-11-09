using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncronizingBBJ
{
    class MemberInfo
    {
        public int TFMemberInfoId { get; set; }
        public DateTime BusinessDate { get; set; }
        public int Sequence { get; set; }
        public string AccountStatus { get; set; }
        public string FixParticipant { get; set; }
        public string ParticipantCode { get; set; }
        public string InvestorCode { get; set; }
        public string AccountType { get; set; }
        public string GroupProduct { get; set; }
        public string Region { get; set; }
        public string Special { get; set; }
        public string CustomerName { get; set; }
        public string Email { get; set; }
        public string Status { get; set; }
        public string SuretyBondAccount { get; set; }
    }
}
