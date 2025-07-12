using System.Threading.Tasks;
using SharedResources;
using Pulumi;

class Program
{
    static Task<int> Main() => Deployment.RunAsync<SharedResources.Stack>();
}