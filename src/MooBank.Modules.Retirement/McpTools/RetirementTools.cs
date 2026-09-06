using System.ComponentModel;
using Asm.MooBank.Modules.Retirement.Models;
using ModelContextProtocol.Server;

namespace Asm.MooBank.Modules.Retirement.McpTools;

[McpServerToolType]
public class RetirementTools(IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher)
{
    [McpServerTool(Destructive = false, Idempotent = true, Name = "get-retirement-plans", ReadOnly = true, Title = "Get Retirement Plans")]
    [Description(
        "Retrieves the household's retirement plans with the assumptions they run under and the people on them. " +
        "This is the inputs, not the answer: inflation, the employer contribution rate, contributions tax, life expectancy and target income for the plan, and for each member their age, retirement age, income, salary sacrifice, fees, insurance, growth strategy and — where their strategy is Custom — the return rate they chose. " +
        "Use it to see what a projection is being run on, or to check a figure before changing it. Run the projection itself with run-retirement-projection.")]
    public ValueTask<IEnumerable<RetirementPlan>> GetPlans(CancellationToken cancellationToken = default) =>
        queryDispatcher.Dispatch(new Queries.GetPlans(), cancellationToken);

    [McpServerTool(Destructive = false, Idempotent = true, Name = "run-retirement-projection", ReadOnly = true, Title = "Run Retirement Projection")]
    [Description(
        "Projects a retirement plan forward from today: every year's opening and closing balance, contributions, investment return, costs, drawdown and Age Pension, in nominal terms and in today's dollars, plus a per-member outcome and a summary. " +
        "Nothing is saved — the plan is left exactly as it is, so this is safe to run repeatedly. " +
        "The summary carries the balance at retirement, the year the money runs out if it does, the total pension received and the most the plan could sustainably pay. " +
        "Members can be left out to see the plan for one person; excluding someone changes what the household is, so a couple projected as one is assessed at the single Age Pension rate. " +
        "The response is long — one entry per year to life expectancy — so ask for a specific plan rather than exploring with it.")]
    public ValueTask<RetirementProjection> RunProjection(
        [Description("The plan to project, from get-retirement-plans.")] Guid planId,
        [Description("Member ids to leave out of this projection, from the plan's members. Omit to project the whole household.")] IEnumerable<Guid>? excludedMemberIds = null,
        CancellationToken cancellationToken = default) =>
        commandDispatcher.Dispatch(
            new Commands.RunProjection(planId, new ProjectionOverrides { ExcludedMemberIds = excludedMemberIds ?? [] }),
            cancellationToken);
}
