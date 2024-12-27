using Microsoft.Extensions.DependencyInjection;

namespace Brimborium.Macro;
public partial class JupiterUtlity {
    public class Loading {
        private ServiceProvider? _ServiceProvider;

        internal Loading(ServiceProvider serviceProvider) {
            this._ServiceProvider = serviceProvider;
        }

        public ServiceProvider ServiceProvider => _ServiceProvider ?? throw new System.ObjectDisposedException("");

        public async Task<JupiterUtlity> OpenSolutionAsync(
            string solutionFilePath
            ) {
            var solutionService = this.ServiceProvider.GetRequiredService<SolutionService>();
            var solution = await solutionService.OpenSolutionAsync(solutionFilePath, default, default);
            return new JupiterUtlity(this.ServiceProvider, solutionService, solution);
        }
    }
}
