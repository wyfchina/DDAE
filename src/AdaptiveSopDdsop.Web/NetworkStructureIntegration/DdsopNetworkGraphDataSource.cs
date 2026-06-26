using AdaptiveSopDdsop.NetworkStructure;

using AdaptiveSopDdsop.Web.Domain;

namespace AdaptiveSopDdsop.Web.NetworkStructureIntegration;

public sealed class DdsopNetworkGraphDataSource : INetworkGraphDataSource
{
    private readonly INetworkStructureDataSource _dataSource;

    public DdsopNetworkGraphDataSource(INetworkStructureDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public NetworkGraphDataSet LoadNetworkGraph(DateOnly anchorDate)
    {
        var data = _dataSource.LoadNetworkStructure(new NetworkStructureDataRequest(12, anchorDate));
        return new NetworkGraphDataSet(data.NetworkData, data.Request.AnchorDate);
    }
}
