using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using SuperSalsaNOW.Core.Models;
using SuperSalsaNOW.Core.Services;

namespace SuperSalsaNOW.Core.Tests;

public class GitHubManifestLoaderTests
{
    private static readonly string s_directoryJson = /*lang=json,strict*/ """
        {"InstallRoot":"C:\\steam","GameDirectory":"Games","ModsDirectory":"Mods"}
        """;

    private static readonly string s_modsJson = /*lang=json,strict*/ """
        [{"Id":"err","Name":"ERR","Description":"x","Nexus":{"GameDomain":"eldenring","ModId":541,"FilePattern":"main"},"Strategy":0}]
        """;

    private static readonly string s_toolsJson = /*lang=json,strict*/ """
        [{"Id":"tool1","Name":"Tool","Url":"https://example.com/t.zip","Version":"1.0.0"}]
        """;

    [Fact]
    public async Task LoadManifestAsync_fetches_three_files_and_composes_ManifestRoot()
    {
        var handler = new ManifestHttpHandler(
            ("directory.json", s_directoryJson),
            ("mods.json", s_modsJson),
            ("tools.json", s_toolsJson));
        using var client = new HttpClient(handler);

        var loader = new GitHubManifestLoader(client, NullLogger<GitHubManifestLoader>.Instance);
        var root = await loader.LoadManifestAsync("https://example.com/base");

        Assert.Equal("C:\\steam", root.Directory.InstallRoot);
        Assert.Single(root.Mods);
        Assert.Equal("err", root.Mods[0].Id);
        Assert.Equal(541, root.Mods[0].Nexus.ModId);
        Assert.Equal(InstallStrategy.ErrLauncher, root.Mods[0].Strategy);
        Assert.Single(root.Tools);
        Assert.Equal("tool1", root.Tools[0].Id);
    }

    [Fact]
    public async Task LoadManifestFileAsync_throws_when_json_deserializes_to_null()
    {
        var handler = new ManifestHttpHandler(("directory.json", "null"));
        using var client = new HttpClient(handler);

        var loader = new GitHubManifestLoader(client, NullLogger<GitHubManifestLoader>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await loader.LoadManifestFileAsync<DirectoryConfig>("https://example.com/x", "directory.json"));
    }

    private sealed class ManifestHttpHandler : HttpMessageHandler
    {
        private readonly (string fileSuffix, string Body)[] _responses;

        public ManifestHttpHandler(params (string fileSuffix, string Body)[] responses)
        {
            _responses = responses;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var uri = request.RequestUri?.ToString() ?? "";
            foreach (var (suffix, body) in _responses)
            {
                if (uri.EndsWith(suffix, StringComparison.Ordinal))
                    return Task.FromResult(Response(body));
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        }

        private static HttpResponseMessage Response(string json) =>
            new(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
            };
    }
}
