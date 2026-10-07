// SPDX-License-Identifier: MIT
using System.Xml.Linq;
using Xunit;

namespace Auditarium.Infrastructure.Security.Tests;

public sealed class ArchitectureHardeningTests
{
    [Fact]
    public void Production_project_dependencies_follow_the_allowed_directions()
    {
        var root = FindRepositoryRoot();
        var allowed = new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            ["Core/Auditarium.Common/Auditarium.Common.csproj"] = [],
            ["Core/Auditarium.Models/Auditarium.Models.csproj"] = [],
            ["Core/Auditarium.Bll/Auditarium.Bll.csproj"] =
                ["Core/Auditarium.Common/Auditarium.Common.csproj", "Core/Auditarium.Models/Auditarium.Models.csproj"],
            ["Persistence/Auditarium.Dal/Auditarium.Dal.csproj"] =
                ["Core/Auditarium.Bll/Auditarium.Bll.csproj", "Core/Auditarium.Models/Auditarium.Models.csproj"],
            ["Persistence/Auditarium.Fal/Auditarium.Fal.csproj"] = ["Core/Auditarium.Bll/Auditarium.Bll.csproj"],
            ["Infrastructure/Auditarium.Infrastructure.Ldap/Auditarium.Infrastructure.Ldap.csproj"] = ["Core/Auditarium.Bll/Auditarium.Bll.csproj"],
            ["Infrastructure/Auditarium.Infrastructure.Security/Auditarium.Infrastructure.Security.csproj"] = ["Core/Auditarium.Bll/Auditarium.Bll.csproj"],
            ["Persistence/Auditarium.Dal.PostgreSql.Migrations/Auditarium.Dal.PostgreSql.Migrations.csproj"] = ["Persistence/Auditarium.Dal/Auditarium.Dal.csproj"],
            ["Persistence/Auditarium.Dal.SqlServer.Migrations/Auditarium.Dal.SqlServer.Migrations.csproj"] = ["Persistence/Auditarium.Dal/Auditarium.Dal.csproj"]
        };

        foreach (var (project, expectedReferences) in allowed)
        {
            var projectPath = Path.Combine(root, project);
            var projectDirectory = Path.GetDirectoryName(projectPath)!;
            var references = XDocument.Load(projectPath).Descendants("ProjectReference")
                .Select(reference => Path.GetFullPath(Path.Combine(projectDirectory, reference.Attribute("Include")!.Value)))
                .Select(reference => Path.GetRelativePath(root, reference).Replace('\\', '/'))
                .Order(StringComparer.Ordinal)
                .ToArray();

            Assert.Equal(expectedReferences.Order(StringComparer.Ordinal), references);
        }
    }

    [Fact]
    public void Presentation_code_has_no_direct_dal_or_fal_shortcuts()
    {
        var root = FindRepositoryRoot();
        var forbidden = new[] { "Auditarium.Dal", "Auditarium.Fal", "IAuditariumDbContext", "AuditariumDbContext", "IFileStorage" };
        var presentationFiles = Directory.EnumerateFiles(Path.Combine(root, "UI"), "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            .Where(path => !path.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            .Where(path => !string.Equals(Path.GetFileName(path), "Program.cs", StringComparison.Ordinal));

        foreach (var path in presentationFiles)
        {
            var source = File.ReadAllText(path);
            Assert.DoesNotContain(forbidden, token => source.Contains(token, StringComparison.Ordinal));
            if (path.Contains(Path.DirectorySeparatorChar + "Auditarium.Web" + Path.DirectorySeparatorChar, StringComparison.Ordinal))
                Assert.DoesNotContain("/api/v1", source, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void Demo_data_tool_is_an_independent_console_project()
    {
        var root = FindRepositoryRoot();
        var demoDataProject = Path.Combine(root, "Tools", "Auditarium.DemoData", "Auditarium.DemoData.csproj");
        var project = XDocument.Load(demoDataProject);
        var projectReferences = project.Descendants("ProjectReference").ToArray();

        Assert.Equal("Exe", project.Descendants("OutputType").Single().Value);
        Assert.Empty(projectReferences);
        Assert.Contains("Tools\\Auditarium.DemoData\\Auditarium.DemoData.csproj", File.ReadAllText(Path.Combine(root, "Auditarium.sln")), StringComparison.Ordinal);

        foreach (var productProject in new[]
                 {
                     "UI/Auditarium.Web/Auditarium.Web.csproj",
                     "UI/Auditarium.Api/Auditarium.Api.csproj"
                 })
        {
            var references = File.ReadAllText(Path.Combine(root, productProject));
            Assert.DoesNotContain("Auditarium.DemoData", references, StringComparison.Ordinal);
        }

        var program = File.ReadAllText(Path.Combine(root, "Tools", "Auditarium.DemoData", "Program.cs"));
        Assert.DoesNotContain("WebApplication", program, StringComparison.Ordinal);
        Assert.DoesNotContain("Map", program, StringComparison.Ordinal);
        Assert.DoesNotContain("AddHostedService", program, StringComparison.Ordinal);
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
            if (File.Exists(Path.Combine(directory.FullName, "Auditarium.sln"))) return directory.FullName;
        throw new DirectoryNotFoundException("Could not locate the Auditarium repository root.");
    }
}
