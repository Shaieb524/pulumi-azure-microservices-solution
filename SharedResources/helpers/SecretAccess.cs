using Pulumi;

namespace SharedResources.helpers
{
    // SharedResources secret access template
    public class SecretAccess
    {
        private readonly Config _config;

        public SecretAccess(Config config)
        {
            _config = config;
        }

        // SQL Server secrets
        public Output<string> SqlAdminPassword => _config.RequireSecret("SqlAdminPassword");
    }
}