using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brimborium.Macro;

public partial class JupiterUtlity {
    public class Boot {
        private static AssemblyResolver? _AssemblyResolver;
        private ServiceCollection? _ServiceDescriptors;

        public Boot() {
            var assemblyResolver = _AssemblyResolver ??= AssemblyResolver.Create(this.GetType().Assembly);

            var serviceDescriptors = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            serviceDescriptors.AddMacro();
            if (assemblyResolver is { }) {
                serviceDescriptors.AddSingleton<AssemblyResolver>(assemblyResolver);
            }
            this._ServiceDescriptors = serviceDescriptors;
        }

        public ServiceCollection ServiceDescriptors => this._ServiceDescriptors ?? throw new System.ObjectDisposedException("");

        public Boot ConfigureServices(Action<ServiceCollection> configure) {
            configure(this.ServiceDescriptors);
            return this;
        }

        public Loading Build() {
            var serviceProvider = this.ServiceDescriptors.BuildServiceProvider();
            var result = new Loading(serviceProvider);
            this._ServiceDescriptors = default!;
            return result;
        }
    }
}
