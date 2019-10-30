using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace CashDesk.Model
{
    public class DepositStatistics : IDepositStatistics
    {
        public Member Member { get; set; }
        IMember IDepositStatistics.Member => Member;

        public int Year { get; set; }

        public decimal TotalAmount { get; set; }
    }
}
