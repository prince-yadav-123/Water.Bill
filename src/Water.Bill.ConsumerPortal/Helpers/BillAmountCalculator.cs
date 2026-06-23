using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.ConsumerPortal.Helpers;

internal static class BillAmountCalculator
{
    public static double ResolveCurrentPayableAmount(JalPrintBillMaster? bill)
    {
        if (bill is null)
            return 0;

        var baseAmount = bill.TotalBillAmt ?? bill.DueAmt ?? 0;
        var arrearInterest = bill.ArearInt ?? 0;
        var billPercentage = bill.BillPercentage ?? 0;

        if (baseAmount <= 0)
            return 0;

        var interestRebate = Math.Ceiling((arrearInterest * billPercentage) / 100d);
        var payable = baseAmount - interestRebate;

        return payable > 0 ? payable : 0;
    }

    public static double ResolvePaidAmount(JalPrintBillMaster? bill)
        => bill?.LastPaidAmt ?? bill?.PaidAmt ?? 0;
}
