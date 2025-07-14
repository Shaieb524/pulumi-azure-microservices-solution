# Post-Deployment Scripts Guide

This directory contains automated scripts to update Pulumi ESC (Environment, Secrets, and Configuration) with values from your deployed infrastructure.

## Overview

After deploying your SharedResources infrastructure, these scripts will:
- Extract deployment outputs (like connection strings, URLs, etc.)
- Create or update Pulumi ESC environments
- Store configuration values for other projects to consume

## Prerequisites

1. **Pulumi CLI** installed and authenticated
2. **ESC CLI** installed and authenticated
3. **Azure CLI** installed and authenticated
4. **SharedResources infrastructure** successfully deployed
5. **Configuration file** `after-deploy-configs.txt` properly configured

## Configuration File

Ensure `after-deploy-configs.txt` exists with the following format:
PulumiOrganization=YourOrg
PulumiProject=YourProject
PulumiEnv=your-env
## Step-by-Step Execution Guide

Follow these steps in order after deploying your SharedResources infrastructure:

### Step 1: Update API Management URLs
Extract API Management service URLs and configuration.
.\update-apim-esc.ps1
**What it does**:
- Lists available Pulumi stacks and prompts for the stack name to use
- Gets APIM Gateway URL, Portal URL, and service name from the selected stack
- Creates ESC environment if it doesn't exist
- Stores APIM configuration under `ApiManagement:*`

**ESC Output**:
- `ApiManagement:GatewayUrl`
- `ApiManagement:PortalUrl`
- `ApiManagement:ServiceName`

### Step 2: Update SQL Connection Strings
Create SQL Server connection strings for all databases.
.\update-sql-esc.ps1
**What it does**:
- Lists available Pulumi stacks and prompts for the stack name to use
- Gets SQL Server FQDN and name from stack outputs
- Prompts for SQL admin password (or reads from config)
- Creates connection strings for all configured databases
- Stores them as secrets in ESC

**ESC Output**:
- `ConnectionStrings:YourDatabase1` (secret)
- `ConnectionStrings:YourDatabase2` (secret)
- `ConnectionStrings:YourDatabase3` (secret)
- ... (all configured databases)

### Step 3: Update Docker Settings
Extract Container Registry credentials and store them in ESC.
.\update-docker-esc.ps1
**What it does**:
- Lists available Pulumi stacks and prompts for the stack name to use
- Gets Container Registry login server, username, and password from the selected stack
- Creates ESC environment if it doesn't exist
- Stores Docker settings under `DockerSettings:*`

**ESC Output**:
- `DockerSettings:DockerRegistryUrl`
- `DockerSettings:DockerRegistryUserName`
- `DockerSettings:DockerRegistryPassword` (secret)

### Step 4: Move Container Registry Repositories
Migrate container repositories from another registry.
.\move-acr-repos.ps1 -sourceRegistry "old-registry-name" -targetRegistry "new-registry-name"
**What it does**:
- Copies all repositories and tags from source ACR to target ACR
- Uses Azure CLI `az acr import` commands
- Provides detailed progress and summary

**Usage Options**:
- **Dry run**: `.\move-acr-repos.ps1 -sourceRegistry "source" -targetRegistry "target" -whatIf:$true`
- **Actual move**: `.\move-acr-repos.ps1 -sourceRegistry "source" -targetRegistry "target" -whatIf:$false`

**Note**: Get the target registry name from Step 3 output or run:# First, list available stacks
pulumi stack ls

# Then get the output from your chosen stack
pulumi stack output ContainerRegistryUsername --stack your-stack-name
### Step 5: Run Database Migration Scripts
Execute SQL migration scripts to set up database schema and initial data.
.\db-migrations\run-migrations.ps1
**What it does**:
- Connects to SQL databases using connection strings from Step 2
- Runs SQL migration scripts in the correct order
- Creates database schema, tables, indexes, and initial data
- Provides execution progress and error handling

**Prerequisites**:
- Step 2 (SQL connection strings) must be completed first
- Migration scripts should be in the `db-migrations/` folder
- SQL Server must be accessible and databases created

**Usage Options**:
- **Run all migrations**: `.\db-migrations\run-migrations.ps1`
- **Run specific database**: `.\db-migrations\run-migrations.ps1 -database "YourDatabase1"`
- **Dry run**: `.\db-migrations\run-migrations.ps1 -whatIf:$true`

**Migration Script Structure**:db-migrations/
├── 001_initial_schema.sql
├── 002_create_tables.sql
├── 003_create_indexes.sql
├── 004_seed_data.sql
└── run-migrations.ps1
## How the Scripts Work

1. **Read Configuration**: All scripts read from `after-deploy-configs.txt`
2. **Navigate to Correct Directory**: Changes to SharedResources directory for Pulumi commands
3. **Select Pulumi Stack**: Interactively prompts for stack name with a default suggestion
4. **Create ESC Environment**: Creates `YourOrg/YourProject/your-env` if it doesn't exist
5. **Extract Values**: Gets outputs from the selected Pulumi stack
6. **Update ESC**: Stores values in the ESC environment for other projects to use

## Consuming Configuration in Other Projects

### In C# Projects:var config = new Config("SharedResources");

// Docker settings
var dockerUrl = config.Require("DockerSettings:DockerRegistryUrl");
var dockerUser = config.Require("DockerSettings:DockerRegistryUserName");
var dockerPass = config.RequireSecret("DockerSettings:DockerRegistryPassword");

// SQL connection strings
var dbConnection = config.RequireSecret("ConnectionStrings:YourDatabase");

// APIM URLs
var apimGateway = config.Require("ApiManagement:GatewayUrl");
### In Pulumi Stack YAML:
Add this to your `Pulumi.{stack}.yaml`:
environment:
  - YourOrg/YourProject/your-env
## Troubleshooting

### Common Issues:

1. **"No Pulumi.yaml project file found"**
   - Scripts automatically navigate to SharedResources directory
   - Ensure you're running from the post-deploy-scripts directory

2. **"Stack not found"**
   - When prompted, enter a valid stack name from the displayed list
   - If you're unsure, just press Enter to use the default stack name

3. **"ESC environment not found" (404 errors)**
   - Scripts automatically create ESC environments
   - Ensure you have proper ESC permissions

4. **"Configuration file not found"**
   - Ensure `after-deploy-configs.txt` exists in post-deploy-scripts directory
   - Check file format and content

### Getting Help:

1. **List available stacks**:cd ..\
   pulumi stack ls
2. **Check stack outputs**:# First, list available stacks
pulumi stack ls

# Then view outputs for your chosen stack
pulumi stack output --stack your-stack-name
3. **View ESC environment**:esc env open YourOrg/YourProject/your-env
## Execution Order

Run these scripts in the numbered order above after infrastructure deployment. Each script is independent and can be run multiple times safely.

**Quick execution** (run all steps):.\update-apim-esc.ps1
.\update-sql-esc.ps1  
.\update-docker-esc.ps1
.\move-acr-repos.ps1 -sourceRegistry "source" -targetRegistry "target" -whatIf:$false
.\db-migrations\run-migrations.ps1
## Notes

- All scripts are idempotent (safe to run multiple times)
- Sensitive values (passwords, connection strings) are stored as secrets in ESC
- Scripts automatically handle ESC environment creation
- Configuration is centralized in `after-deploy-configs.txt`
- All scripts now interactively prompt for stack name with reasonable defaults
