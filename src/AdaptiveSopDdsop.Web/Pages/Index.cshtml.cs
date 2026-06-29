using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace AdaptiveSopDdsop.Web.Pages;

public class IndexModel : PageModel
{
    private const string DefaultNetworkStructureProductUrl = "http://127.0.0.1:5296/network-structure";

    private readonly IConfiguration _configuration;

    public IndexModel(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string NetworkStructureProductUrl { get; private set; } = DefaultNetworkStructureProductUrl;

    public void OnGet()
    {
        NetworkStructureProductUrl =
            _configuration["NetworkStructure:ProductUrl"] ?? DefaultNetworkStructureProductUrl;
    }
}
