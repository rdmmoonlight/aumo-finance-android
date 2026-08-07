using System.Threading.Tasks;
using AumoFinance.Services.Reports;

namespace AumoFinance.Services;

public class AccountingService : BaseApiService
{
    private readonly AdjustingJournalService _adjustingJournalService;
    private readonly IncomeStatementService _incomeStatementService;
    private readonly RetainedEarningsService _retainedEarningsService;
    private readonly StatementOfFinancialPositionService _sofpService;
    private readonly TrialBalanceService _trialBalanceService;
    private readonly WorksheetService _worksheetService;
    private readonly GeneralLedgerService _generalLedgerService;
    private readonly PostClosingTrialBalanceService _postClosingTbService; // jika diperlukan

    public AccountingService(
        AdjustingJournalService adjustingJournalService,
        IncomeStatementService incomeStatementService,
        RetainedEarningsService retainedEarningsService,
        StatementOfFinancialPositionService sofpService,
        TrialBalanceService trialBalanceService,
        WorksheetService worksheetService,
        GeneralLedgerService generalLedgerService)
    {
        _adjustingJournalService = adjustingJournalService;
        _incomeStatementService = incomeStatementService;
        _retainedEarningsService = retainedEarningsService;
        _sofpService = sofpService;
        _trialBalanceService = trialBalanceService;
        _worksheetService = worksheetService;
        _generalLedgerService = generalLedgerService;
    }

    // Proxy methods untuk menjaga kompatibilitas page report lama
    public async Task<(AdjustingJournalReportApiResponse?, string?)> GetAdjustingJournalReportAsync()
        => await _adjustingJournalService.GetAdjustingJournalReportAsync();

    public async Task<(IncomeStatementReportApiResponse?, string?)> GetIncomeStatementReportAsync()
        => await _incomeStatementService.GetIncomeStatementReportAsync();

    public async Task<(RetainedEarningsReportApiResponse?, string?)> GetRetainedEarningsReportAsync()
        => await _retainedEarningsService.GetRetainedEarningsReportAsync();

    public async Task<(StatementOfFinancialPositionReportApiResponse?, string?)> GetStatementOfFinancialPositionReportAsync()
        => await _sofpService.GetStatementOfFinancialPositionReportAsync();

    public async Task<(TrialBalanceReportApiResponse?, string?)> GetTrialBalanceReportAsync(string type = "unadjusted")
        => await _trialBalanceService.GetTrialBalanceReportAsync(type);

    public async Task<(WorksheetReportApiResponse?, string?)> GetWorksheetReportAsync()
        => await _worksheetService.GetWorksheetReportAsync();
}
