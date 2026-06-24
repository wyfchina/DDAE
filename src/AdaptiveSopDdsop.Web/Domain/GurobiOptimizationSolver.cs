using Gurobi;

namespace AdaptiveSopDdsop.Web.Domain;

public sealed class GurobiOptimizationSolver : IOptimizationSolver
{
    public string SolverName => "Gurobi";

    public OptimizationSolution Solve(OptimizationProblem problem)
    {
        if (problem.Candidates.Count == 0)
        {
            return new OptimizationSolution(
                OptimizationSolverStatus.Infeasible,
                "Gurobi",
                "没有可供优化的候选动作。",
                0m,
                Array.Empty<string>());
        }

        try
        {
            using var environment = new GRBEnv(true);
            environment.Set("OutputFlag", "0");
            environment.Start();

            using var model = new GRBModel(environment)
            {
                ModelName = problem.ProblemId
            };

            var variables = problem.Candidates.ToDictionary(
                candidate => candidate.CandidateId,
                candidate => model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, $"choose_{Sanitize(candidate.CandidateId)}"),
                StringComparer.Ordinal);

            var objective = new GRBLinExpr();
            foreach (var candidate in problem.Candidates)
            {
                objective.AddTerm(decimal.ToDouble(candidate.ObjectiveValue), variables[candidate.CandidateId]);
            }
            model.SetObjective(objective, GRB.MAXIMIZE);

            var selectedCount = new GRBLinExpr();
            foreach (var variable in variables.Values)
            {
                selectedCount.AddTerm(1.0, variable);
            }
            model.AddConstr(selectedCount, GRB.LESS_EQUAL, problem.MaxSelectedCandidates, "max_actions");

            if (problem.InventoryBudget >= 0m)
            {
                var inventoryBudget = new GRBLinExpr();
                foreach (var candidate in problem.Candidates)
                {
                    inventoryBudget.AddTerm(decimal.ToDouble(Math.Max(0m, candidate.InventoryDelta)), variables[candidate.CandidateId]);
                }
                model.AddConstr(inventoryBudget, GRB.LESS_EQUAL, decimal.ToDouble(problem.InventoryBudget), "inventory_budget");
            }

            if (problem.CostBudget >= 0m)
            {
                var costBudget = new GRBLinExpr();
                foreach (var candidate in problem.Candidates)
                {
                    costBudget.AddTerm(decimal.ToDouble(Math.Max(0m, candidate.EstimatedCost)), variables[candidate.CandidateId]);
                }
                model.AddConstr(costBudget, GRB.LESS_EQUAL, decimal.ToDouble(problem.CostBudget), "cost_budget");
            }

            foreach (var conflictGroup in problem.Candidates
                         .Where(candidate => !string.IsNullOrWhiteSpace(candidate.ConflictKey))
                         .GroupBy(candidate => candidate.ConflictKey, StringComparer.Ordinal)
                         .Where(group => group.Count() > 1))
            {
                var conflict = new GRBLinExpr();
                foreach (var candidate in conflictGroup)
                {
                    conflict.AddTerm(1.0, variables[candidate.CandidateId]);
                }
                model.AddConstr(conflict, GRB.LESS_EQUAL, 1.0, $"conflict_{Sanitize(conflictGroup.Key)}");
            }

            model.Optimize();

            if (model.Status == GRB.Status.OPTIMAL || model.SolCount > 0)
            {
                var selected = problem.Candidates
                    .Where(candidate => variables[candidate.CandidateId].X > 0.5)
                    .Select(candidate => candidate.CandidateId)
                    .ToList();
                var status = model.Status == GRB.Status.OPTIMAL
                    ? OptimizationSolverStatus.Optimal
                    : OptimizationSolverStatus.Feasible;
                var objectiveValue = selected.Count == 0 ? 0m : Convert.ToDecimal(model.ObjVal);

                return new OptimizationSolution(
                    status,
                    "Gurobi",
                    selected.Count == 0 ? "Gurobi 已求解，但未选择正收益候选动作。" : "Gurobi 已生成推荐动作组合。",
                    decimal.Round(objectiveValue, 3),
                    selected);
            }

            return new OptimizationSolution(
                OptimizationSolverStatus.Infeasible,
                "Gurobi",
                $"Gurobi 未找到可行解，状态码 {model.Status}。",
                0m,
                Array.Empty<string>());
        }
        catch (GRBException ex)
        {
            return new OptimizationSolution(
                OptimizationSolverStatus.Unavailable,
                "Gurobi",
                $"Gurobi 不可用：{ex.Message}",
                0m,
                Array.Empty<string>());
        }
        catch (Exception ex)
        {
            return new OptimizationSolution(
                OptimizationSolverStatus.Error,
                "Gurobi",
                $"优化求解失败：{ex.Message}",
                0m,
                Array.Empty<string>());
        }
    }

    private static string Sanitize(string value)
    {
        var chars = value.Select(ch => char.IsLetterOrDigit(ch) ? ch : '_').ToArray();
        return new string(chars);
    }
}
