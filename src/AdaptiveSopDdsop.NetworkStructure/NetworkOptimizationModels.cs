namespace AdaptiveSopDdsop.NetworkStructure;

public enum OptimizationSolverStatus
{
    Optimal,
    Feasible,
    Infeasible,
    Unavailable,
    Error
}

public sealed record OptimizationCandidate(
    string CandidateId,
    string ActionType,
    string Target,
    string ConflictKey,
    decimal ObjectiveValue,
    decimal InventoryDelta,
    decimal EstimatedCost,
    string ConstraintNote,
    string FeasibilityStatus,
    string Explanation);

public sealed record OptimizationCandidateImpact(
    string CandidateId,
    string ActionType,
    string Target,
    decimal ServiceImpactPercent,
    decimal InventoryImpactValue,
    decimal PeakLoadImpactPercent,
    decimal SupplyGapImpact,
    int ReplenishmentOrderImpact,
    decimal EstimatedCost,
    string CostBasis,
    string ConstraintNote,
    string FeasibilityStatus);

public sealed record OptimizationProblem(
    string ProblemId,
    string ProfileId,
    int MaxSelectedCandidates,
    decimal InventoryBudget,
    decimal CostBudget,
    IReadOnlyList<OptimizationCandidate> Candidates);

public sealed record OptimizationSolution(
    OptimizationSolverStatus Status,
    string SolverName,
    string Message,
    decimal ObjectiveValue,
    IReadOnlyList<string> SelectedCandidateIds);

public interface IOptimizationSolver
{
    string SolverName { get; }

    OptimizationSolution Solve(OptimizationProblem problem);
}

public sealed record CandidateActionSelectionProfile(
    string ProfileId,
    string ProfileName,
    int MaxSelectedCandidates,
    decimal InventoryBudget,
    decimal CostBudget,
    IReadOnlyList<OptimizationCandidate> Candidates);

public sealed record CandidateActionSelectionResult(
    string ProfileId,
    string ProfileName,
    OptimizationSolverStatus Status,
    string SolverName,
    string Message,
    decimal ObjectiveValue,
    IReadOnlyList<string> SelectedCandidateIds);

public sealed record CandidateActionCombinationRequest(
    int HorizonWeeks = 12,
    string? SolverName = null,
    int CombinationCount = 3,
    int MaxActionsPerCombination = 3,
    string? TargetMode = null,
    IReadOnlyList<string>? CandidateIds = null);

public sealed class CandidateActionCombinationSelector
{
    private readonly IReadOnlyDictionary<string, IOptimizationSolver> _solvers;

    public CandidateActionCombinationSelector(IEnumerable<IOptimizationSolver> solvers)
    {
        _solvers = solvers.ToDictionary(solver => solver.SolverName, StringComparer.OrdinalIgnoreCase);
    }

    public CandidateActionSelectionResult Select(CandidateActionSelectionProfile profile, string? solverName)
    {
        var solver = ResolveSolver(solverName);
        var problem = new OptimizationProblem(
            $"CANDIDATE-COMBO-{profile.ProfileId}-{Guid.NewGuid():N}",
            profile.ProfileId,
            Math.Clamp(profile.MaxSelectedCandidates, 1, 5),
            Math.Max(0m, profile.InventoryBudget),
            Math.Max(0m, profile.CostBudget),
            profile.Candidates);
        var solution = solver.Solve(problem);

        return new CandidateActionSelectionResult(
            profile.ProfileId,
            profile.ProfileName,
            solution.Status,
            solution.SolverName,
            solution.Message,
            solution.ObjectiveValue,
            solution.SelectedCandidateIds);
    }

    private IOptimizationSolver ResolveSolver(string? solverName)
    {
        if (!string.IsNullOrWhiteSpace(solverName) && _solvers.TryGetValue(solverName, out var requested))
        {
            return requested;
        }

        if (_solvers.TryGetValue("Gurobi", out var gurobi))
        {
            return gurobi;
        }

        return _solvers.Values.First();
    }
}
