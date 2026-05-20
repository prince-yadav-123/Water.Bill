namespace Water.Bill.ConsumerPortal.ViewModels;

public class ConsumerBillViewModel
{
    public ConsumerBillConsumerViewModel? Consumer { get; set; }
    public ConsumerBillDetailViewModel? Bill { get; set; }
    public string? Message { get; set; }
}

public class ConsumerBillPaymentViewModel : ConsumerBillViewModel
{
    public int Step { get; set; } = 1;
    public string AmountOption { get; set; } = "Full";
    public double? PartialAmount { get; set; }
    public string PaymentMethod { get; set; } = "UPI";
    public string? PaymentIdentifier { get; set; }
    public bool SaveMethod { get; set; } = true;
    public bool SetupAutoPay { get; set; }
    public double ConvenienceFee { get; set; }
    public bool IsGatewayReady { get; set; }

    public double SelectedAmount
    {
        get
        {
            var billAmount = Bill?.TotalPayableAmount ?? 0;
            if (!AmountOption.Equals("Partial", StringComparison.OrdinalIgnoreCase))
                return billAmount;

            var partial = PartialAmount.GetValueOrDefault();
            return partial > 0 && partial < billAmount ? partial : billAmount;
        }
    }

    public double FinalPayableAmount => SelectedAmount + ConvenienceFee;
}

public class ConsumerBillConsumerViewModel
{
    public string ConsumerNo { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? MobileNo { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Sector { get; set; }
    public string? Block { get; set; }
    public string? FlatNo { get; set; }
    public string? PropertyNo { get; set; }
    public string? ConnectionType { get; set; }
    public string? Category { get; set; }
    public int? PipeSize { get; set; }
    public string? WaterConsumption { get; set; }
}

public class ConsumerBillDetailViewModel
{
    public string BillNo { get; set; } = string.Empty;
    public string? ChallanNo { get; set; }
    public DateTime? BillDate { get; set; }
    public DateTime? BillDateFrom { get; set; }
    public DateTime? BillDateTo { get; set; }
    public DateTime? DueDate { get; set; }
    public double MinimumRate { get; set; }
    public double CurrentAmount { get; set; }
    public double RebateAmount { get; set; }
    public double CessAmount { get; set; }
    public double ArrearAmount { get; set; }
    public double ArrearInterest { get; set; }
    public double LastBillExtra { get; set; }
    public double AdvanceAmount { get; set; }
    public double TotalPayableAmount { get; set; }
    public double AfterDueDateAmount { get; set; }
    public double LastPaidAmount { get; set; }
    public double EstimatedWaterCharges { get; set; }
    public double SewerageCharges { get; set; }
    public double MeterRent { get; set; }
    public DateTime? PaidDate { get; set; }
    public string PaymentStatus { get; set; } = "Pending";
    public string? BillPeriod { get; set; }
    public string? Division { get; set; }
    public int? PrintStatus { get; set; }
    public bool IsPaid => PaymentStatus.Equals("Paid", StringComparison.OrdinalIgnoreCase);
}
