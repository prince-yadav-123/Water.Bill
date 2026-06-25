using Water.Bill.Application.Models.Excel;

namespace Water.Bill.Application.Interfaces;

public interface IExcelExportService
{
    byte[] Export<T>(ExcelExportRequest<T> request);
}
