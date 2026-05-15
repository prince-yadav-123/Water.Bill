using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class ErpPaymentResponse
{
    public string? StatusCode { get; set; }

    public string? StatusMessage { get; set; }

    public string? VoucherNumber { get; set; }

    public string? Of1 { get; set; }

    public string? Of2 { get; set; }

    public string? Of3 { get; set; }

    public string? ChallanNo { get; set; }

    public string? BankTransactionNo { get; set; }

    public DateTime? CreatedOn { get; set; }
}
