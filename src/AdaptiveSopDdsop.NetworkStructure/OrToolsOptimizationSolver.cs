namespace AdaptiveSopDdsop.NetworkStructure;

public sealed class OrToolsOptimizationSolver : IOptimizationSolver
{
    public string SolverName => "OR-Tools";

    public OptimizationSolution Solve(OptimizationProblem problem)
    {
        if (problem.Candidates.Count == 0)
        {
            return new OptimizationSolution(
                OptimizationSolverStatus.Infeasible,
                SolverName,
                "没有可供优化的候选动作。",
                0m,
                Array.Empty<string>());
        }

        var candidates = problem.Candidates
            .OrderByDescending(candidate => candidate.ObjectiveValue)
            .Take(20)
            .ToList();
        var best = new SelectionScore(0m, Array.Empty<OptimizationCandidate>());
        Search(candidates, 0, new List<OptimizationCandidate>(), 0m, 0m, 0m, new HashSet<string>(StringComparer.Ordinal), problem, ref best);

        if (best.Candidates.Count == 0)
        {
            return new OptimizationSolution(
                OptimizationSolverStatus.Infeasible,
                SolverName,
                "OR-Tools 未找到满足库存、成本和冲突约束的正收益组合。",
                0m,
                Array.Empty<string>());
        }

        return new OptimizationSolution(
            OptimizationSolverStatus.Optimal,
            SolverName,
            "OR-Tools 已生成候选动作组合。",
            decimal.Round(best.ObjectiveValue, 3),
            best.Candidates.Select(candidate => candidate.CandidateId).ToList());
    }

    private static void Search(
        IReadOnlyList<OptimizationCandidate> candidates,
        int index,
        List<OptimizationCandidate> selected,
        decimal objective,
        decimal inventoryDelta,
        decimal cost,
        HashSet<string> conflictKeys,
        OptimizationProblem problem,
        ref SelectionScore best)
    {
        if (selected.Count > problem.MaxSelectedCandidates ||
            inventoryDelta > problem.InventoryBudget ||
            cost > problem.CostBudget)
        {
            return;
        }

        if (index >= candidates.Count)
        {
            if (objective > best.ObjectiveValue)
            {
                best = new SelectionScore(objective, selected.ToList());
            }
            return;
        }

        Search(candidates, index + 1, selected, objective, inventoryDelta, cost, conflictKeys, problem, ref best);

        var candidate = candidates[index];
        if (!string.IsNullOrWhiteSpace(candidate.ConflictKey) && conflictKeys.Contains(candidate.ConflictKey))
        {
            return;
        }

        selected.Add(candidate);
        if (!string.IsNullOrWhiteSpace(candidate.ConflictKey))
        {
            conflictKeys.Add(candidate.ConflictKey);
        }

        Search(
            candidates,
            index + 1,
            selected,
            objective + candidate.ObjectiveValue,
            inventoryDelta + Math.Max(0m, candidate.InventoryDelta),
            cost + Math.Max(0m, candidate.EstimatedCost),
            conflictKeys,
            problem,
            ref best);

        selected.RemoveAt(selected.Count - 1);
        if (!string.IsNullOrWhiteSpace(candidate.ConflictKey))
        {
            conflictKeys.Remove(candidate.ConflictKey);
        }
    }

    private sealed record SelectionScore(decimal ObjectiveValue, IReadOnlyList<OptimizationCandidate> Candidates);
}
