using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;

using System.Reflection;
using System.Runtime.Loader;

namespace Brimborium.Macro;

public partial class JupiterUtility {
    public static Boot Create() => new Boot();

    internal JupiterUtility(ServiceProvider serviceProvider, SolutionService solutionService, Solution solution) {
        this.ServiceProvider = serviceProvider;
        this.SolutionService = solutionService;
        this.Solution = solution;
    }

    public ServiceProvider ServiceProvider { get; }
    public SolutionService SolutionService { get; }
    public Solution Solution { get; }
}
