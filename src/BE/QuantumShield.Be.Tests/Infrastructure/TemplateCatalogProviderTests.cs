using Microsoft.Extensions.Hosting;
using NSubstitute;
using QuantumShield.Be.Infrastructure.Templates;

namespace QuantumShield.Be.Tests.Infrastructure;

public sealed class TemplateCatalogProviderTests
{
    [Fact]
    public async Task LoadCatalogAsync_ShouldReadLocalTemplates()
    {
        var hostEnvironment = Substitute.For<IHostEnvironment>();
        hostEnvironment.ContentRootPath.Returns(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "QuantumShield.Be.Api"));
        var provider = new FileSystemTemplateCatalogProvider(hostEnvironment);

        var templates = await provider.LoadCatalogAsync(CancellationToken.None);

        Assert.NotEmpty(templates);
        Assert.Contains(templates, static template => template.ControlId == "1.1.1");
        Assert.Contains(templates.SelectMany(static template => template.Checks), static check => check.Method == "manual");
    }
}
