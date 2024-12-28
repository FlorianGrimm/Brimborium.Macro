using Microsoft.Extensions.DependencyInjection;

namespace Brimborium.Macro;
public partial class JupiterUtility {
    public class Loading {
        private ServiceProvider? _ServiceProvider;

        internal Loading(ServiceProvider serviceProvider) {
            this._ServiceProvider = serviceProvider;
        }

        public ServiceProvider ServiceProvider => _ServiceProvider ?? throw new System.ObjectDisposedException("");

        public async Task<JupiterUtility> OpenSolutionAsync(
            string solutionFilePath
            ) {
            var solutionService = this.ServiceProvider.GetRequiredService<SolutionService>();
            var solution = await solutionService.OpenSolutionAsync(solutionFilePath, default, default);
            return new JupiterUtility(this.ServiceProvider, solutionService, solution);
        }
    }
}
