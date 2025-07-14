using System.Threading.Tasks;
using Pulumi;
using App1.stack;

return await Deployment.RunAsync<App1Stack>();
