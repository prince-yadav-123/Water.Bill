using System.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Water.Bill.API.Filters;
using Water.Bill.API.Models.Dashboard;
using Water.Bill.Core.Common;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;
using Water.Bill.Infrastructure.Services;

namespace Water.Bill.API.Controllers.Mvc;

[Authorize(AuthenticationSchemes = AppConstants.CookieScheme)]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _db;

    public DashboardController(ApplicationDbContext db)
    {
        _db = db;
    }

    [RequirePermission("Dashboard.view")]
    public async Task<IActionResult> Index(decimal? defaulterThreshold, CancellationToken ct)
    {
        ViewData["Title"] = "Dashboard";
        ViewData["ActiveMenu"] = "Dashboard";

        var userId = ResolveUserId();
        var roleId = ResolveRoleId();
        var username = ResolveUsername();
        var user = userId > 0
            ? await _db.Appusers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId && !x.IsDeleted, ct)
            : null;
        var roleName = await _db.Approles
            .AsNoTracking()
            .Where(x => x.Id == roleId && !x.IsDeleted)
            .Select(x => x.Name)
            .FirstOrDefaultAsync(ct)
            ?? User.FindFirstValue(ClaimTypes.Role)
            ?? AppConstants.Roles.Staff;
        var isAdminView = roleName.Contains("admin", StringComparison.OrdinalIgnoreCase);
        var departmentId = userId > 0
            ? await _db.Appusers
                .AsNoTracking()
                .Where(x => x.Id == userId && !x.IsDeleted)
                .Select(x => x.DeptId)
                .FirstOrDefaultAsync(ct)
            : null;

        var vm = isAdminView
            ? await BuildAdminDashboardAsync(user?.FullName ?? username, roleName, defaulterThreshold, ct)
            : await BuildStaffDashboardAsync(userId, roleId, username, user?.FullName ?? username, roleName, departmentId, ct);

        return View(vm);
    }

    private async Task<DashboardIndexViewModel> BuildAdminDashboardAsync(string userName, string roleName, decimal? defaulterThreshold, CancellationToken ct)
    {
        var monthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var pendingTaskQuery = PendingWorkflowTasksQuery();
        var hasSupportQueries = await TableExistsAsync("consumersupportqueries", ct);

        var totalConsumers = await _db.ConsumerDetailsMasters.AsNoTracking().CountAsync(ct);
        var activeConsumers = await _db.ConsumerDetailsMasters.AsNoTracking().CountAsync(x => x.Status == 1, ct);

        var newConnections = await _db.NewConnectionApplications.AsNoTracking().CountAsync(x => !x.IsDeleted, ct);
        var finalConsumersCreated = await _db.NewConnectionApplications.AsNoTracking()
            .CountAsync(x => !x.IsDeleted && x.ApplicationStatus == "FinalConsumerCreated", ct);

        var pendingApprovals = await pendingTaskQuery.CountAsync(ct);

        var totalChallans = await _db.Challans.AsNoTracking().CountAsync(ct);
        var paidChallans = await _db.Challans.AsNoTracking()
            .CountAsync(x => x.PayDate.HasValue || (x.PaidAmt != null && x.PaidAmt > 0) || x.Status == "Paid", ct);
        var pendingChallans = await _db.Challans.AsNoTracking().CountAsync(x =>
            !x.PayDate.HasValue && (x.PaidAmt == null || x.PaidAmt <= 0) && x.Status != "Paid" &&
            (x.Status == null || x.Status == "" || x.Status == "Generated" || x.Status == "PendingPayment"), ct);

        var collectionThisMonth = await _db.ChallanPaymentHistories.AsNoTracking()
            .Where(x => !x.IsDeleted && x.PaymentDate >= monthStart)
            .SumAsync(x => (double?)x.Amount, ct) ?? 0d;

        var openQueries = hasSupportQueries
            ? await _db.ConsumerSupportQueries.AsNoTracking()
                .CountAsync(x => !x.IsDeleted && (x.Status == "Open" || x.Status == "InProgress"), ct)
            : 0;

        var summaryCards = new List<DashboardStatCardViewModel>
        {
            MakeCard("Total Consumers", totalConsumers.ToString("N0"), string.Empty,
                tone: "primary", url: Url.Action("Index", "ConsumerMasterMaintenance"), icon: "bi-people-fill"),

            MakeCard("Active Consumers", activeConsumers.ToString("N0"), string.Empty,
                tone: "success", url: Url.Action("Index", "ConsumerMasterMaintenance"), icon: "bi-person-check-fill"),

            MakeCard("New Connections", newConnections.ToString("N0"), string.Empty,
                tone: "info", url: "/Approvals?tab=All&applicationType=NewConnection", icon: "bi-plug-fill"),

            MakeCard("Consumers Created", finalConsumersCreated.ToString("N0"), string.Empty,
                tone: "success", url: "/Approvals?tab=All&applicationType=NewConnection", icon: "bi-patch-check-fill"),

            MakeCard("Pending Approvals", pendingApprovals.ToString("N0"), string.Empty,
                tone: "warning", url: "/Approvals?tab=Pending", icon: "bi-hourglass-split"),

            MakeCard("Pending Challans", pendingChallans.ToString("N0"), string.Empty,
                tone: "warning", url: "/ChallanManagement?status=PendingPayment", icon: "bi-receipt"),

            MakeCard("Collected This Month", $"Rs. {collectionThisMonth:N0}", string.Empty,
                tone: "success", url: Url.Action("PaymentHistory", "ChallanManagement"), icon: "bi-currency-rupee"),

            MakeCard("Open Queries", openQueries.ToString("N0"), string.Empty,
                tone: "danger", url: "/ConsumerQueryManagement?status=Open", icon: "bi-chat-dots-fill")
        };

        var applicationStatusChart = await BuildApplicationStatusChartAsync(ct);
        var challanStatusChart = await BuildChallanStatusChartAsync(paidChallans, pendingChallans, totalChallans - paidChallans - pendingChallans, ct);
        var divisionConsumerChart = await BuildDivisionConsumerChartAsync(ct);
        var paymentCollectionChart = await BuildPaymentCollectionByTypeChartAsync(ct);
        var defaulterWidget = await BuildDefaulterWidgetAsync(totalConsumers, defaulterThreshold, ct);
        var workloadChart = await BuildDepartmentWorkloadChartAsync(pendingTaskQuery, ct);
        var trendChart = await BuildAdminTrendChartAsync(ct);
        var serviceDeskPanel = await BuildAdminServiceDeskPanelAsync(hasSupportQueries, ct);
        var recentApplications = await BuildRecentApplicationsAsync(ct);
        var pendingApprovalsRows = await BuildPendingApprovalRowsAsync(pendingTaskQuery, ct);
        var recentChallans = await BuildRecentChallansAsync(_db.Challans.AsNoTracking(), ct);
        var recentServiceDesk = await BuildRecentServiceDeskAsync(hasSupportQueries, userId: null, ct);
        var recentActivity = await BuildRecentActivityAsync(_db.Auditlogs.AsNoTracking(), ct);

        return new DashboardIndexViewModel
        {
            UserName = userName,
            RoleName = roleName,
            IsAdminView = true,
            SummaryCards = summaryCards,
            PrimaryStatusChart = applicationStatusChart,
            ChallanStatusChart = challanStatusChart,
            DivisionConsumerChart = divisionConsumerChart,
            PaymentCollectionChart = paymentCollectionChart,
            DefaulterWidget = defaulterWidget,
            SecondaryBarChart = workloadChart,
            TrendChart = trendChart,
            ServiceDeskPanel = serviceDeskPanel,
            RecentApplications = recentApplications,
            PendingApprovals = pendingApprovalsRows,
            RecentChallans = recentChallans,
            RecentServiceDeskItems = recentServiceDesk,
            RecentActivities = recentActivity,
            QuickLinks = new List<DashboardQuickLinkViewModel>
            {
                MakeLink("Consumers", Url.Action("Index", "ConsumerMasterMaintenance"), "Master records"),
                MakeLink("Approvals", "/Approvals?tab=Pending", "Workflow queue"),
                MakeLink("Challans", Url.Action("Index", "ChallanManagement"), "Demand and collection"),
                MakeLink("Consumer Queries", Url.Action("Index", "ConsumerQueryManagement"), "Support queue"),
                MakeLink("Reports & MIS", Url.Action("Index", "ReportsMis"), "Operational summaries")
            }
        };
    }

    private async Task<DashboardIndexViewModel> BuildStaffDashboardAsync(
        int userId,
        int roleId,
        string username,
        string userName,
        string roleName,
        int? departmentId,
        CancellationToken ct)
    {
        var assignedPendingQuery = ApplyWorkflowAssignmentFilter(PendingWorkflowTasksQuery(), userId, roleId, departmentId);
        var hasSupportQueries = await TableExistsAsync("consumersupportqueries", ct);
        var allAssignedQuery = ApplyWorkflowAssignmentFilter(_db.ApplicationWorkflowTasks
            .Include(x => x.Stage)
            .Include(x => x.WorkflowInstance)
            .AsNoTracking()
            .Where(x => !x.IsDeleted && !x.WorkflowInstance.IsDeleted), userId, roleId, departmentId);

        var myPendingApprovals = await assignedPendingQuery.CountAsync(ct);
        var today = DateTime.Today;
        var overdueApprovals = (await assignedPendingQuery
                .Select(x => new
                {
                    x.AssignedOn,
                    x.Stage.SlaDays
                })
                .ToListAsync(ct))
            .Count(x => x.SlaDays.HasValue
                && x.SlaDays.Value > 0
                && x.AssignedOn.Date.AddDays(x.SlaDays.Value) < today);
        var myChallansQuery = _db.Challans.AsNoTracking().Where(x =>
            x.Userid == username || x.Userid == userId.ToString());
        var myChallanCount = await myChallansQuery.CountAsync(ct);
        var myOpenQueries = hasSupportQueries
            ? await _db.ConsumerSupportQueries.AsNoTracking()
                .CountAsync(x => !x.IsDeleted && x.AssignedToUserId == userId && (x.Status == "Open" || x.Status == "InProgress"), ct)
            : 0;

        var summaryCards = new List<DashboardStatCardViewModel>
        {
            MakeCard("My Pending Approvals", myPendingApprovals.ToString("N0"), string.Empty,
                tone: "warning", url: "/Approvals?tab=Pending", icon: "bi-hourglass-split"),

            MakeCard("Overdue Approvals", overdueApprovals.ToString("N0"), string.Empty,
                tone: "danger", url: "/Approvals?tab=Pending", icon: "bi-exclamation-triangle-fill"),

            MakeCard("My Challans", myChallanCount.ToString("N0"), string.Empty,
                tone: "info", url: Url.Action("Index", "ChallanManagement"), icon: "bi-receipt"),

            MakeCard("My Open Queries", myOpenQueries.ToString("N0"), string.Empty,
                tone: "danger", url: Url.Action("Index", "ConsumerQueryManagement"), icon: "bi-chat-dots-fill")
        };

        var workloadChart = await BuildStaffWorkloadChartAsync(assignedPendingQuery, ct);
        var recentChallans = await BuildRecentChallansAsync(myChallansQuery, ct);
        var recentServiceDesk = await BuildRecentServiceDeskAsync(hasSupportQueries, userId, ct);
        var recentActivity = await BuildRecentActivityAsync(_db.Auditlogs.AsNoTracking().Where(x => x.UserId == userId), ct);
        var assignedRows = await BuildPendingApprovalRowsAsync(allAssignedQuery.OrderByDescending(x => x.AssignedOn).Take(8), ct);

        return new DashboardIndexViewModel
        {
            UserName = userName,
            RoleName = roleName,
            IsAdminView = false,
            SummaryCards = summaryCards,
            SecondaryBarChart = workloadChart,
            PendingApprovals = assignedRows,
            RecentChallans = recentChallans,
            RecentServiceDeskItems = recentServiceDesk,
            RecentActivities = recentActivity,
            QuickLinks = Array.Empty<DashboardQuickLinkViewModel>()
        };
    }

    private static Task<DashboardDonutChartViewModel> BuildChallanStatusChartAsync(int paid, int pending, int generated, CancellationToken ct)
    {
        var items = new List<DashboardChartSliceViewModel>
        {
            new() { Label = "Paid", Count = paid, Color = "#16a34a" },
            new() { Label = "Pending Payment", Count = pending, Color = "#f59e0b" },
            new() { Label = "Generated / Due", Count = Math.Max(0, generated), Color = "#2563eb" }
        }.Where(x => x.Count > 0).ToList();

        return Task.FromResult(new DashboardDonutChartViewModel
        {
            Title = "Challan Payment Status",
            Caption = "Payment position across all raised challans",
            Items = items
        });
    }

    private async Task<DashboardDonutChartViewModel> BuildApplicationStatusChartAsync(CancellationToken ct)
    {
        var raw = await _db.NewConnectionApplications.AsNoTracking()
            .Where(x => !x.IsDeleted)
            .GroupBy(x => x.ApplicationStatus)
            .Select(x => new { Status = x.Key, Count = x.Count() })
            .ToListAsync(ct);
        var counts = raw.ToDictionary(x => x.Status ?? string.Empty, x => x.Count, StringComparer.OrdinalIgnoreCase);

        return new DashboardDonutChartViewModel
        {
            Title = "Application Status",
            Caption = "New connection lifecycle across all applications",
            Items = new List<DashboardChartSliceViewModel>
            {
                MakeSlice("Draft", counts, "#94a3b8"),
                MakeSlice("PendingPayment", counts, "#f59e0b"),
                MakeSlice("Submitted", counts, "#2563eb"),
                MakeSlice("UnderReview", counts, "#0ea5e9"),
                MakeSlice("Approved", counts, "#16a34a"),
                MakeSlice("Rejected", counts, "#dc2626"),
                MakeSlice("FinalConsumerCreated", counts, "#7c3aed")
            }.Where(x => x.Count > 0).ToList()
        };
    }

    private async Task<DashboardDistributionChartViewModel> BuildDivisionConsumerChartAsync(CancellationToken ct)
    {
        var rows = await _db.ConsumerDetailsMasters.AsNoTracking()
            .GroupBy(x => x.DevType)
            .Select(x => new { x.Key, Count = x.Count() })
            .ToListAsync(ct);

        var total = rows.Sum(x => x.Count);
        var items = rows
            .Select((x, index) =>
            {
                var division = AppConstants.Divisions.Find(x.Key);
                var label = division?.Name;
                if (string.IsNullOrWhiteSpace(label) || string.Equals(label, AppConstants.Divisions.AllDivision.Name, StringComparison.OrdinalIgnoreCase))
                    label = "Others";

                return new { Label = label, x.Count, Color = BarPalette[index % BarPalette.Length] };
            })
            .GroupBy(x => x.Label)
            .Select((group, index) => new DashboardDistributionSliceViewModel
            {
                Label = group.Key,
                Count = group.Sum(x => x.Count),
                Percentage = total == 0 ? 0 : Math.Round(group.Sum(x => x.Count) * 100m / total, 1),
                Color = group.FirstOrDefault()?.Color ?? BarPalette[index % BarPalette.Length]
            })
            .OrderByDescending(x => x.Count)
            .ToList();

        NormalizePercentages(items.Select(x => (Action<decimal>)(pct => x.Percentage = pct)).ToList(), items.Select(x => (decimal)x.Count).ToList());

        return new DashboardDistributionChartViewModel
        {
            Title = "Division-wise Consumers",
            Caption = "Consumer count and percentage share by division",
            Items = items
        };
    }

    private async Task<DashboardAmountDistributionChartViewModel> BuildPaymentCollectionByTypeChartAsync(CancellationToken ct)
    {
        var onlineRows = await _db.JalnoidaBankpayMasters.AsNoTracking()
            .Where(x => x.Payamount.HasValue
                && (
                    x.Paymentstatus == "SUCCESS" || x.Paymentstatus == "SUC000" || x.Paymentstatus == "Y" || x.Paymentstatus == "S" || x.Paymentstatus == "1"
                    || x.Status == "SUCCESS" || x.Status == "SUC000" || x.Status == "Y" || x.Status == "S" || x.Status == "1"))
            .Select(x => x.Payamount!.Value)
            .ToListAsync(ct);

        var offlineRows = await _db.ChallanPaymentHistories.AsNoTracking()
            .Where(x => !x.IsDeleted)
            .Select(x => x.Amount)
            .ToListAsync(ct);

        var onlineAmount = ToDecimal(onlineRows.Sum());
        var offlineAmount = ToDecimal(offlineRows.Sum());
        var items = new List<DashboardAmountDistributionSliceViewModel>
        {
            new()
            {
                Label = "Online Collection",
                Amount = onlineAmount,
                Color = "#16a34a"
            },
            new()
            {
                Label = "Offline Collection",
                Amount = offlineAmount,
                Color = "#2563eb"
            }
        };

        NormalizePercentages(items.Select(x => (Action<decimal>)(pct => x.Percentage = pct)).ToList(), items.Select(x => x.Amount).ToList());

        return new DashboardAmountDistributionChartViewModel
        {
            Title = "Payment Collection (Offline/Online)",
            Caption = "Online and offline collection share based on received amount",
            Items = items
        };
    }

    private async Task<DashboardDefaulterWidgetViewModel> BuildDefaulterWidgetAsync(int totalConsumers, decimal? defaulterThreshold, CancellationToken ct)
    {
        var threshold = defaulterThreshold.HasValue && defaulterThreshold.Value > 0
            ? defaulterThreshold.Value
            : AppConstants.DefaulterDueThreshold;
        var billRows = await _db.JalPrintBillMasters.AsNoTracking()
            .Where(x => x.ConsNo != null)
            .Select(x => new
            {
                x.ConsNo,
                Total = x.TotalBillAmt ?? x.DueAmt ?? x.MinTotalAmt ?? 0d,
                x.PaidAmt,
                x.PaidDate,
                x.PaidStatus
            })
            .ToListAsync(ct);

        var defaulters = billRows
            .GroupBy(x => x.ConsNo!)
            .Select(group => new
            {
                ConsumerNo = group.Key,
                Outstanding = group.Sum(x =>
                {
                    var total = ToDecimal(x.Total);
                    var paid = x.PaidDate.HasValue || string.Equals(x.PaidStatus, "Y", StringComparison.OrdinalIgnoreCase)
                        ? (x.PaidAmt.HasValue ? ToDecimal(x.PaidAmt.Value) : total)
                        : ToDecimal(x.PaidAmt ?? 0d);
                    return Math.Max(0m, total - paid);
                })
            })
            .Where(x => x.Outstanding >= threshold)
            .ToList();

        return new DashboardDefaulterWidgetViewModel
        {
            Title = "Defaulters",
            Caption = "Consumers with outstanding dues at or above the configured threshold",
            TotalConsumers = totalConsumers,
            ConsumerCount = defaulters.Count,
            NonDefaulterCount = Math.Max(0, totalConsumers - defaulters.Count),
            TotalOutstandingAmount = defaulters.Sum(x => x.Outstanding),
            ThresholdAmount = threshold,
            ConfiguredThresholdAmount = AppConstants.DefaulterDueThreshold,
            Url = "/ReportsMis?reportType=Dues"
        };
    }

    private async Task<DashboardBarChartViewModel> BuildDepartmentWorkloadChartAsync(IQueryable<ApplicationWorkflowTask> pendingTaskQuery, CancellationToken ct)
    {
        var rows = await pendingTaskQuery
            .GroupBy(x => new { x.AssignedDepartmentId, DepartmentName = x.Stage.Department != null ? x.Stage.Department.DeptName : null })
            .Select(x => new
            {
                Name = x.Key.DepartmentName ?? (x.Key.AssignedDepartmentId.HasValue ? $"Department {x.Key.AssignedDepartmentId.Value}" : "Direct Assignment"),
                Count = x.Count()
            })
            .OrderByDescending(x => x.Count)
            .Take(6)
            .ToListAsync(ct);

        return new DashboardBarChartViewModel
        {
            Title = "Pending Workload by Department",
            Caption = "Current active workflow queue",
            Items = rows.Select((x, index) => new DashboardBarItemViewModel
            {
                Label = x.Name,
                Value = x.Count,
                Color = BarPalette[index % BarPalette.Length],
                Url = "/Approvals?tab=Pending"
            }).ToList()
        };
    }

    private async Task<DashboardBarChartViewModel> BuildStaffWorkloadChartAsync(IQueryable<ApplicationWorkflowTask> assignedPendingQuery, CancellationToken ct)
    {
        // Retained for potential future use; currently not shown in staff dashboard
        var rows = await assignedPendingQuery
            .GroupBy(x => x.WorkflowInstance.ApplicationType)
            .Select(x => new { Label = x.Key, Count = x.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync(ct);

        return new DashboardBarChartViewModel
        {
            Title = "My Pending Works",
            Caption = "Only items currently assigned to you",
            Items = rows.Select((x, index) => new DashboardBarItemViewModel
            {
                Label = FriendlyApplicationType(x.Label),
                Value = x.Count,
                Color = BarPalette[index % BarPalette.Length],
                Url = "/Approvals?tab=Pending"
            }).ToList()
        };
    }

    private async Task<DashboardTrendChartViewModel> BuildAdminTrendChartAsync(CancellationToken ct)
    {
        var months = LastSixMonths();
        var rangeStart = months.First().Start;

        var receivedRows = await _db.NewConnectionApplications.AsNoTracking()
            .Where(x => !x.IsDeleted && x.CreatedOn >= rangeStart)
            .GroupBy(x => new { x.CreatedOn.Year, x.CreatedOn.Month })
            .Select(x => new { x.Key.Year, x.Key.Month, Count = x.Count() })
            .ToListAsync(ct);

        var approvedRows = await _db.NewConnectionApplications.AsNoTracking()
            .Where(x => !x.IsDeleted
                && x.ApprovedOn.HasValue
                && x.ApprovedOn.Value >= rangeStart
                && (x.ApplicationStatus == "Approved" || x.ApplicationStatus == "FinalConsumerCreated"))
            .GroupBy(x => new { x.ApprovedOn!.Value.Year, x.ApprovedOn!.Value.Month })
            .Select(x => new { x.Key.Year, x.Key.Month, Count = x.Count() })
            .ToListAsync(ct);

        return new DashboardTrendChartViewModel
        {
            Title = "New Applications Received vs Approved",
            Caption = "Month-wise comparison for the last 6 months",
            PrimaryLabel = "Received",
            SecondaryLabel = "Approved",
            Points = months.Select(month => new DashboardTrendPointViewModel
            {
                Label = month.Label,
                PrimaryValue = receivedRows.FirstOrDefault(x => x.Year == month.Year && x.Month == month.Month)?.Count ?? 0,
                SecondaryValue = approvedRows.FirstOrDefault(x => x.Year == month.Year && x.Month == month.Month)?.Count ?? 0
            }).ToList()
        };
    }

    private async Task<DashboardStatusPanelViewModel> BuildAdminServiceDeskPanelAsync(bool hasSupportQueries, CancellationToken ct)
    {
        var queryMap = hasSupportQueries
            ? await _db.ConsumerSupportQueries.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .GroupBy(x => x.Status)
                .Select(x => new { Status = x.Key, Count = x.Count() })
                .ToDictionaryAsync(x => x.Status ?? string.Empty, x => x.Count, StringComparer.OrdinalIgnoreCase, ct)
            : new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        return new DashboardStatusPanelViewModel
        {
            Title = "Query Status Snapshot",
            Caption = "Consumer support queue at a glance",
            Groups = new List<DashboardStatusGroupViewModel>
            {
                new DashboardStatusGroupViewModel
                {
                    Label = "Consumer Queries",
                    Items = new List<DashboardStatusItemViewModel>
                    {
                        MakeStatus("Open", queryMap, "danger", "/ConsumerQueryManagement?status=Open"),
                        MakeStatus("InProgress", queryMap, "warning", "/ConsumerQueryManagement?status=InProgress"),
                        MakeStatus("Resolved", queryMap, "success", "/ConsumerQueryManagement?status=Resolved"),
                        MakeStatus("Closed", queryMap, "secondary", "/ConsumerQueryManagement?status=Closed")
                    }
                }
            }
        };
    }

    private async Task<List<DashboardRecentApplicationViewModel>> BuildRecentApplicationsAsync(CancellationToken ct)
    {
        var newConnections = await _db.NewConnectionApplications.AsNoTracking()
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.CreatedOn)
            .Take(5)
            .Select(x => new DashboardRecentApplicationViewModel
            {
                Type = "New Connection",
                ApplicationNo = x.ApplicationNo,
                ApplicantName = x.ApplicantName,
                Status = x.ApplicationStatus,
                Property = $"{x.Sector} / {x.Block} / {x.FlatNo}",
                CreatedOn = x.SubmittedOn ?? x.CreatedOn,
                Url = $"/Approvals?tab=All&applicationType=NewConnection&search={x.ApplicationNo}"
            })
            .ToListAsync(ct);

        var ndcApps = await _db.ConsumerApplyNdcs.AsNoTracking()
            .OrderByDescending(x => x.CreatedOn)
            .Take(3)
            .Select(x => new DashboardRecentApplicationViewModel
            {
                Type = "NDC",
                ApplicationNo = x.ApplicationNo,
                ApplicantName = x.ConsName ?? "-",
                Status = x.CurrentStatus ?? x.FinalStatus ?? x.Status ?? "-",
                Property = $"{x.Sector} / {x.Block} / {x.PlotNo}",
                CreatedOn = x.CreatedOn,
                Url = $"/Approvals?tab=All&applicationType={WorkflowService.ApplicationTypeNdc}&search={x.ApplicationNo}"
            })
            .ToListAsync(ct);

        var serviceRequests = await _db.MasterApplicationDetails.AsNoTracking()
            .Where(x => x.AppType == "TRN" || x.AppType == "CTC")
            .OrderByDescending(x => x.EnterDate)
            .Take(3)
            .Select(x => new DashboardRecentApplicationViewModel
            {
                Type = x.AppType == "TRN" ? "Name Transfer" : "Connection Change",
                ApplicationNo = x.ApplicationId,
                ApplicantName = x.ConName ?? "-",
                Status = x.ApplicationStatus ?? "-",
                Property = $"{x.SectorVill} / {x.Block} / {x.PlotNo}",
                CreatedOn = x.EnterDate.HasValue ? x.EnterDate.Value.ToDateTime(TimeOnly.MinValue) : null,
                Url = $"/Approvals?tab=All&search={x.ApplicationId}"
            })
            .ToListAsync(ct);

        return newConnections
            .Concat(ndcApps)
            .Concat(serviceRequests)
            .OrderByDescending(x => x.CreatedOn)
            .Take(8)
            .ToList();
    }

    private async Task<List<DashboardPendingApprovalViewModel>> BuildPendingApprovalRowsAsync(IQueryable<ApplicationWorkflowTask> query, CancellationToken ct)
    {
        var rows = await query
            .OrderByDescending(x => x.AssignedOn)
            .Take(8)
            .ToListAsync(ct);

        if (rows.Count == 0)
            return new List<DashboardPendingApprovalViewModel>();

        var appIds = rows.Where(x => x.WorkflowInstance.ApplicationType == "NewConnection").Select(x => x.ApplicationId).Distinct().ToList();
        var ndcIds = rows.Where(x => x.WorkflowInstance.ApplicationType == WorkflowService.ApplicationTypeNdc).Select(x => (int)x.ApplicationId).Distinct().ToList();
        var legacyNos = rows.Where(x => IsLegacyConsumerChange(x.WorkflowInstance.ApplicationType)).Select(x => x.ApplicationNo).Distinct().ToList();
        var userIds = rows.Where(x => x.AssignedUserId.HasValue).Select(x => x.AssignedUserId!.Value).Distinct().ToList();
        var roleIds = rows.Where(x => x.AssignedRoleId.HasValue).Select(x => x.AssignedRoleId!.Value).Distinct().ToList();

        var apps = await _db.NewConnectionApplications.AsNoTracking()
            .Where(x => appIds.Contains(x.Id) && !x.IsDeleted)
            .ToDictionaryAsync(x => x.Id, ct);
        var ndcs = await _db.ConsumerApplyNdcs.AsNoTracking()
            .Where(x => ndcIds.Contains(x.AutoId))
            .ToDictionaryAsync(x => x.AutoId, ct);
        var legacy = await _db.MasterApplicationDetails.AsNoTracking()
            .Where(x => legacyNos.Contains(x.ApplicationId))
            .ToDictionaryAsync(x => x.ApplicationId, ct);
        var users = await _db.Appusers.AsNoTracking()
            .Where(x => userIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.FullName, ct);
        var roles = await _db.Approles.AsNoTracking()
            .Where(x => roleIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Name, ct);

        var today = DateTime.Today;
        return rows.Select(x =>
        {
            apps.TryGetValue(x.ApplicationId, out var app);
            ndcs.TryGetValue((int)x.ApplicationId, out var ndc);
            legacy.TryGetValue(x.ApplicationNo, out var legacyApp);

            return new DashboardPendingApprovalViewModel
            {
                TaskId = x.Id,
                ApplicationType = FriendlyApplicationType(x.WorkflowInstance.ApplicationType),
                ApplicationNo = x.ApplicationNo,
                ApplicantName = app?.ApplicantName ?? ndc?.ConsName ?? legacyApp?.ConName ?? "-",
                StageName = x.Stage.StageName,
                Status = x.Status,
                AssignedOn = x.AssignedOn,
                DaysSinceAssigned = (int)(today - x.AssignedOn.Date).TotalDays,
                AssignedTo = ResolveAssignedTo(x, users, roles),
                Url = $"/Approvals/Details/{x.Id}"
            };
        }).ToList();
    }

    private async Task<List<DashboardRecentChallanViewModel>> BuildRecentChallansAsync(IQueryable<Challan> query, CancellationToken ct)
    {
        var rows = await query
            .OrderByDescending(x => x.Id)
            .Take(8)
            .ToListAsync(ct);

        return rows.Select(x => new DashboardRecentChallanViewModel
        {
            ChallanId = x.Id,
            ChallanNo = ResolveChallanNo(x),
            ConsumerNo = x.ConsNo ?? "-",
            Purpose = ResolveChallanPurpose(x),
            Amount = Convert.ToDecimal(x.BillAmt ?? x.Noc ?? x.TFee ?? x.ConnCharge ?? x.Disconnection ?? x.Reconnection ?? 0d),
            Status = ResolveChallanStatus(x),
            GeneratedOn = x.EntryDate,
            Url = $"/ChallanManagement/Details/{x.Id}"
        }).ToList();
    }

    private async Task<List<DashboardServiceDeskItemViewModel>> BuildRecentServiceDeskAsync(
        bool hasSupportQueries,
        int? userId,
        CancellationToken ct)
    {
        if (!hasSupportQueries)
            return new List<DashboardServiceDeskItemViewModel>();

        return await ApplyServiceDeskAssignment(_db.ConsumerSupportQueries.AsNoTracking().Where(x => !x.IsDeleted), userId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(8)
            .Select(x => new DashboardServiceDeskItemViewModel
            {
                Type = "Query",
                ReferenceNo = x.QueryNo,
                ConsumerName = x.ConsumerName,
                Subject = x.Subject,
                Status = x.Status,
                CreatedOn = x.CreatedAt,
                Url = $"/ConsumerQueryManagement/Details/{x.Id}"
            })
            .ToListAsync(ct);
    }

    private static async Task<List<DashboardActivityItemViewModel>> BuildRecentActivityAsync(IQueryable<Auditlog> query, CancellationToken ct)
    {
        var rows = await query
            .OrderByDescending(x => x.Timestamp)
            .Take(8)
            .Select(x => new DashboardActivityItemViewModel
            {
                Action = AuditActionLabel(x.Action),
                Module = x.Module ?? "System",
                Username = x.Username,
                Success = x.Success,
                OccurredOn = x.Timestamp,
                Details = x.Details
            })
            .ToListAsync(ct);

        return rows;
    }

    private static IQueryable<ConsumerSupportQuery> ApplyServiceDeskAssignment(IQueryable<ConsumerSupportQuery> query, int? userId)
        => userId.HasValue ? query.Where(x => x.AssignedToUserId == userId.Value) : query;

    private IQueryable<ApplicationWorkflowTask> PendingWorkflowTasksQuery()
        => _db.ApplicationWorkflowTasks
            .Include(x => x.Stage)
                .ThenInclude(x => x.Department)
            .Include(x => x.WorkflowInstance)
            .AsNoTracking()
            .Where(x => !x.IsDeleted
                && x.IsActive
                && x.Status == "Pending"
                && x.WorkflowInstance.CurrentStageId == x.StageId
                && x.WorkflowInstance.IsActive
                && !x.WorkflowInstance.IsDeleted);

    // ── IMPORTANT: This filter MUST match ApprovalsController.ApplyWorkflowAssignmentFilter exactly.
    // If they differ the dashboard count and the Approvals list will not match.
    // Rule: if AssignedUserId is set, ONLY that specific user can see the task (role/dept are ignored).
    private static IQueryable<ApplicationWorkflowTask> ApplyWorkflowAssignmentFilter(
        IQueryable<ApplicationWorkflowTask> query,
        int userId,
        int roleId,
        int? departmentId)
        => query.Where(x =>
            // 1. Task assigned to this specific user
            (x.AssignedUserId.HasValue && x.AssignedUserId == userId)
            // 2. Task assigned by dept+role (no specific user)
            || (!x.AssignedUserId.HasValue
                && x.AssignedDepartmentId.HasValue
                && x.AssignedRoleId.HasValue
                && x.AssignedRoleId == roleId
                && departmentId.HasValue
                && x.AssignedDepartmentId == departmentId.Value)
            // 3. Task assigned by dept only (no specific user, no role)
            || (!x.AssignedUserId.HasValue
                && x.AssignedDepartmentId.HasValue
                && !x.AssignedRoleId.HasValue
                && departmentId.HasValue
                && x.AssignedDepartmentId == departmentId.Value)
            // 4. Task assigned by role only (no specific user, no dept)
            || (!x.AssignedUserId.HasValue
                && !x.AssignedDepartmentId.HasValue
                && x.AssignedRoleId.HasValue
                && x.AssignedRoleId == roleId));

    private static DashboardStatCardViewModel MakeCard(
        string label, string value, string caption,
        string? subValue = null, string tone = "primary", string? url = null, string? icon = null)
        => new()
        {
            Label = label,
            Value = value,
            Caption = caption,
            SubValue = subValue,
            Tone = tone,
            Url = url,
            Icon = icon
        };

    private static DashboardQuickLinkViewModel MakeLink(string label, string? url, string caption)
        => new()
        {
            Label = label,
            Url = url ?? "#",
            Caption = caption
        };

    private static DashboardChartSliceViewModel MakeSlice(string status, IReadOnlyDictionary<string, int> rows, string color)
    {
        var count = rows.TryGetValue(status, out var resolved) ? resolved : 0;
        return new DashboardChartSliceViewModel
        {
            Label = FriendlyStatus(status),
            Count = count,
            Color = color
        };
    }

    private static DashboardStatusItemViewModel MakeStatus(string status, IReadOnlyDictionary<string, int> rows, string tone, string url)
    {
        var count = rows.TryGetValue(status, out var resolved) ? resolved : 0;
        return new DashboardStatusItemViewModel
        {
            Label = FriendlyStatus(status),
            Value = count,
            Tone = tone,
            Url = url
        };
    }

    private static void NormalizePercentages(IReadOnlyList<Action<decimal>> setters, IReadOnlyList<decimal> bases)
    {
        if (setters.Count == 0 || bases.Count == 0 || setters.Count != bases.Count)
            return;

        var total = bases.Sum();
        if (total <= 0)
        {
            foreach (var set in setters)
                set(0m);
            return;
        }

        var percentages = bases.Select(x => Math.Round(x * 100m / total, 1)).ToList();
        var diff = 100m - percentages.Sum();
        percentages[^1] += diff;

        for (var i = 0; i < setters.Count; i++)
            setters[i](percentages[i]);
    }

    private static decimal ToDecimal(double value) => Convert.ToDecimal(value);

    private static string ResolveAssignedTo(ApplicationWorkflowTask task, IReadOnlyDictionary<int, string> users, IReadOnlyDictionary<int, string> roles)
    {
        if (task.AssignedUserId.HasValue && users.TryGetValue(task.AssignedUserId.Value, out var userName))
            return userName;
        if (task.AssignedRoleId.HasValue && roles.TryGetValue(task.AssignedRoleId.Value, out var roleName))
            return roleName;
        if (task.AssignedDepartmentId.HasValue)
            return "Department Queue";
        return "Direct Queue";
    }

    private static string FriendlyStatus(string? status)
        => status switch
        {
            "PendingPayment" => "Pending Payment",
            "UnderReview" => "Under Review",
            "FinalConsumerCreated" => "Final Consumer Created",
            "InProgress" => "In Progress",
            _ => status ?? "-"
        };

    private static string FriendlyApplicationType(string? type)
        => type switch
        {
            WorkflowService.ApplicationTypeNdc => "NDC",
            WorkflowService.ApplicationTypeNameTransfer => "Name Transfer",
            WorkflowService.ApplicationTypeConnectionChange => "Connection Change",
            "NewConnection" => "New Connection",
            _ => type ?? "-"
        };

    private static string ResolveChallanNo(Challan x)
        => x.ReceiptId1 ?? x.ReceiptId ?? x.RecpNo ?? $"CH-{x.Id}";

    private static string ResolveChallanPurpose(Challan x)
    {
        if ((x.BillAmt ?? 0d) > 0) return "Bill Due";
        if ((x.Noc ?? 0d) > 0) return "NDC Fee";
        if ((x.TFee ?? 0d) > 0) return "Transfer Fee";
        if ((x.ConnCharge ?? 0d) > 0) return "New Connection";
        if ((x.Disconnection ?? 0d) > 0) return "Disconnection";
        if ((x.Reconnection ?? 0d) > 0) return "Reconnection";
        return "Service Charge";
    }

    private static string ResolveChallanStatus(Challan x)
    {
        if (x.PayDate.HasValue || (x.PaidAmt ?? 0d) > 0 || string.Equals(x.Status, "Paid", StringComparison.OrdinalIgnoreCase))
            return "Paid";
        if (!string.IsNullOrWhiteSpace(x.Status))
            return x.Status;
        return "Generated";
    }

    private static string AuditActionLabel(int action)
        => action switch
        {
            1 => "Create",
            2 => "Update",
            3 => "Delete",
            4 => "View",
            5 => "Login Success",
            6 => "Login Failed",
            7 => "Logout",
            8 => "Permission Changed",
            _ => $"Action {action}"
        };

    private static bool IsLegacyConsumerChange(string? applicationType)
        => applicationType == WorkflowService.ApplicationTypeNameTransfer
           || applicationType == WorkflowService.ApplicationTypeConnectionChange;

    private async Task<bool> TableExistsAsync(string tableName, CancellationToken ct)
    {
        var connection = _db.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;
        if (shouldClose)
            await connection.OpenAsync(ct);

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = @"
SELECT COUNT(*)
FROM sys.tables t
INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
WHERE LOWER(t.name) = LOWER(@tableName);";

            var parameter = command.CreateParameter();
            parameter.ParameterName = "@tableName";
            parameter.Value = tableName;
            command.Parameters.Add(parameter);

            var result = await command.ExecuteScalarAsync(ct);
            return Convert.ToInt32(result) > 0;
        }
        finally
        {
            if (shouldClose)
                await connection.CloseAsync();
        }
    }

    private int ResolveUserId()
        => int.TryParse(User.FindFirstValue(AppConstants.Claims.UserId) ?? User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)
            ? userId
            : 0;

    private int ResolveRoleId()
        => int.TryParse(User.FindFirstValue("RoleId"), out var roleId) ? roleId : 0;

    private string ResolveUsername()
        => User.FindFirstValue(AppConstants.Claims.Username)
           ?? User.Identity?.Name
           ?? "Authority User";

    private static List<(DateTime Start, int Year, int Month, string Label)> LastSixMonths()
    {
        var month = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-5);
        var result = new List<(DateTime, int, int, string)>();
        for (var i = 0; i < 6; i++)
        {
            var current = month.AddMonths(i);
            result.Add((current, current.Year, current.Month, current.ToString("MMM yyyy")));
        }

        return result;
    }

    private static readonly string[] BarPalette =
    {
        "#2563eb",
        "#0f766e",
        "#7c3aed",
        "#ea580c",
        "#db2777",
        "#16a34a"
    };
}
