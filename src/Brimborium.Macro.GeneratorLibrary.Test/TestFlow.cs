using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brimborium.Macro;
public class TestFlow {
    [Fact]
    public async Task Test1Enumator() {
        var solution = SourceSolution.GetSample1();

        var hsDocuments = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var dictListProjectByDocument = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var project in solution.GetProjects()) {
            // project.ProjectId
            foreach (var document in project.ListDocument) {
                hsDocuments.Add(document);
                if (!dictListProjectByDocument.TryGetValue(document, out var hsProject)) {
                    hsProject = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    dictListProjectByDocument.Add(document, hsProject);
                }
                hsProject.Add(project.ProjectId);
            }
        }
        await Verify(new {
            hsDocuments = hsDocuments.Order().ToList(),
            dictListProjectByDocument = dictListProjectByDocument.Select(
                kv => new {
                    Document = kv.Key,
                    Projects = kv.Value.Order().ToList()
                }).ToList()
        });
    }

    [Fact]
    public void Test2() {

    }
    [Fact]
    public void Test3() {
        var solution = SourceSolution.GetSample1();
        var projects = solution.GetProjects().ToList();

        FlowSource<SourceProject> flowSourceProjects = new FlowSource<SourceProject>();
        flowSourceProjects.AddProcess((SourceProject project) => {

        });

        foreach (var project in projects) {
            flowSourceProjects.Next(project);
        }

    }

    internal class SourceSolution {
        public SourceSolution() {
        }
        public List<SourceProject> ListProjects { get; } = new List<SourceProject>();

        public IEnumerable<SourceProject> GetProjects() {
            return this.ListProjects.AsReadOnly();
        }

        public static SourceSolution GetSample1() {
            var result = new SourceSolution();
            result.ListProjects.Add(new SourceProject("1", ["a", "b", "f"]));
            result.ListProjects.Add(new SourceProject("2", ["d", "e", "f"]));
            return result;
        }
    }
    internal class SourceProject {
        public SourceProject(string projectId, List<string> listDocument) {
            this.ProjectId = projectId;
            this.ListDocument = listDocument;
        }
        public string ProjectId { get; }
        public List<string> ListDocument { get; }
    }
}
public class FlowContext {
    private static int _Counter = 0;
    private readonly int _Id = System.Threading.Interlocked.Increment(ref _Counter);

    public FlowContext() {
    }

    public int Id => this._Id;
}
public class FlowSource<T> {
    private ImmutableList<IFlowProcess<T>> _ListFlowProcess = ImmutableList<IFlowProcess<T>>.Empty;
    public FlowSource() {
    }

    public void Next(T item) {
        var listFlowProcess = this._ListFlowProcess;
        foreach (var process in this._ListFlowProcess) { 
            process.Next(item);
        }
    }

    public void AddProcess(IFlowProcess<T> process) {
        _ListFlowProcess = _ListFlowProcess.Add(process);
    }

    public void AddProcess(Action<T> process) {
        _ListFlowProcess = _ListFlowProcess.Add(new FlowProcessSync<T>(process));
    }
}
public interface IFlowProcess<T> {
    Task Next(T item);
}
public class FlowProcessSync<T> : IFlowProcess<T> {
    private readonly Action<T> _Action;

    public FlowProcessSync(Action<T> action) {
        this._Action = action;
    }

    public Task Next(T item) {
        this._Action(item);
        return Task.CompletedTask;
    }
}
public class FlowProcessAsync<T> : IFlowProcess<T> {
    private readonly Func<T, Task> _Action;

    public FlowProcessAsync(
        Func<T, Task> action
        ) {
        this._Action = action;
    }

    public async Task Next(T item) {
        await this._Action(item);
    }
}
