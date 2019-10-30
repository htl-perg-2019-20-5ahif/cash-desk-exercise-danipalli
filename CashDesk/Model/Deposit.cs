using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace CashDesk.Model
{
    public class Deposit : IDeposit
    {
        public int DepositId { get; set; }

        [Required]
        public Membership Membership { get; set; }

        [NotMapped]
        IMembership IDeposit.Membership => Membership;
        
        [Required]
        [Range(0, (double)decimal.MaxValue)]
        public decimal Amount { get; set; }
    }
}
