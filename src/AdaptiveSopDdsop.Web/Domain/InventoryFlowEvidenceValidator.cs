namespace AdaptiveSopDdsop.Web.Domain;

internal static class InventoryFlowEvidenceValidator
{
    public static IReadOnlyDictionary<(string Sku, int Week), InventoryFlowPoint> BuildCompletePointMap(
        string caseId,
        InventoryFlowProjectionResult? inventoryFlow,
        IReadOnlyList<(string Sku, int Week)> expectedKeys)
    {
        if (inventoryFlow is not { Status: "Complete", Summary: not null, Points: not null } ||
            !string.Equals(inventoryFlow.CaseId, caseId, StringComparison.Ordinal) ||
            expectedKeys.Count == 0 ||
            expectedKeys.Distinct().Count() != expectedKeys.Count)
        {
            return new Dictionary<(string Sku, int Week), InventoryFlowPoint>();
        }

        var pointGroups = inventoryFlow.Points
            .GroupBy(item => (item.Sku, item.Week))
            .ToList();
        if (pointGroups.Any(group => group.Count() != 1))
        {
            return new Dictionary<(string Sku, int Week), InventoryFlowPoint>();
        }

        var actualKeys = pointGroups.Select(group => group.Key).ToHashSet();
        if (!actualKeys.SetEquals(expectedKeys))
        {
            return new Dictionary<(string Sku, int Week), InventoryFlowPoint>();
        }

        return pointGroups.ToDictionary(group => group.Key, group => group.Single());
    }

    public static bool IsComplete(
        string caseId,
        InventoryFlowProjectionResult? inventoryFlow,
        IReadOnlyList<(string Sku, int Week)> expectedKeys) =>
        BuildCompletePointMap(caseId, inventoryFlow, expectedKeys).Count == expectedKeys.Count &&
        expectedKeys.Count > 0;
}
