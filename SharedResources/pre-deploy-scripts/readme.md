# ESC Environment Setup Script

Simple PowerShell script to create and configure Pulumi ESC environments from a config file.

## Prerequisites

- Pulumi CLI installed
- ESC CLI installed (`pulumi shell script`)
- PowerShell

## How to Use

1. **Create your config file** (e.g., `client-env-configs.txt`):
   ```
   PulumiOrganization=YourOrg
   PulumiProject=YourProject
   PulumiEnv=prod    
   SharedEnv=Client-App-Shared
   ClientName=YourClient
   Location=EastUS
   Prefix=Prod
   Environment=Production
   DomainName=app.yourclient.com
   Tags.Environment=Production
   Tags.Owner=DevTeam
   Tags.Client=YourClient
   Tags.Project=YourProject
   Tags.ManagedBy=Pulumi
   PlanSku.Capacity=1
   PlanSku.Family=B
   PlanSku.Name=B1
   PlanSku.Size=B1
   PlanSku.Tier=Basic
   ```

2. **Run the script**:
   ```powershell
   .\setup-esc-environment.ps1
   ```

3. **Enter config file path** when prompted:
   ```
   📂 Enter the path to client-env-configs file: ./client-env-configs.txt
   ```

## What It Does

- Reads your config file
- Creates ESC environment: `{PulumiOrganization}/{PulumiProject}/{PulumiEnv}`
- Sets all configuration values from your file
- Maps everything to proper Pulumi config keys

## Example Output

✅ Creates: `YourOrg/YourProject/prod`

## Next Steps

1. View your environment: `esc env open YourOrg/YourProject/prod`
2. Import it in your Pulumi stack YAML
3. Run `pulumi up`

That's it! 🚀