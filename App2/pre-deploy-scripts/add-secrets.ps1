# Pre-Deploy Script for App2 Infrastructure
# This script adds necessary secrets to the Pulumi configuration

Write-Host "Adding secrets to App2 Infrastructure configuration..."

# Add secrets for App2 infrastructure
# NOTE: Replace these with actual secret values from your secure store

# API Keys
pulumi config set App2Infrastructure:ApiKey "your-api-key-here" --secret

# Storage Keys
pulumi config set App2Infrastructure:PrimaryBlobAccountKey "your-primary-blob-key-here" --secret
pulumi config set App2Infrastructure:SecondaryBlobAccountKey "your-secondary-blob-key-here" --secret
pulumi config set App2Infrastructure:App2BlobAccountKey "your-app2-blob-key-here" --secret

# Function Keys
pulumi config set App2Infrastructure:App2FnStorageKey "your-app2-fn-storage-key-here" --secret

# Event Grid Keys
pulumi config set App2Infrastructure:App2EventsTopicKey "your-app2-events-topic-key-here" --secret

# SendGrid Key
pulumi config set App2Infrastructure:SendGridKey "your-sendgrid-key-here" --secret

# Additional blob keys
pulumi config set App2Infrastructure:PrimaryBlobKey "your-primary-blob-key-here" --secret
pulumi config set App2Infrastructure:SecondaryBlobKey "your-secondary-blob-key-here" --secret
pulumi config set App2Infrastructure:App2BlobKey "your-app2-blob-key-here" --secret

Write-Host "Secrets added successfully!"
Write-Host "Please update the secret values with actual values from your secure store."
