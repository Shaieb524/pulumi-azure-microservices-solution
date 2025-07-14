using System.Threading.Tasks;
using Pulumi;
using App2.stack;

return await Deployment.RunAsync<App2Stack>();
