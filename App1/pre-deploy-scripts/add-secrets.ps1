# Pre-Deploy Script for App1 Infrastructure
# This script adds necessary secrets to the Pulumi configuration

Write-Host "Adding secrets to App1 Infrastructure configuration..."

# Add secrets for App1 infrastructure
# NOTE: Replace these with actual secret values from your secure store

# API Keys
pulumi config set App1Infrastructure:ApiKey "your-api-key-here" --secret

# Storage Keys
pulumi config set App1Infrastructure:PrimaryBlobAccountKey "your-primary-blob-key-here" --secret
pulumi config set App1Infrastructure:SecondaryBlobAccountKey "your-secondary-blob-key-here" --secret
pulumi config set App1Infrastructure:App1BlobAccountKey "your-app1-blob-key-here" --secret

# Function Keys
pulumi config set App1Infrastructure:App1FnStorageKey "your-app1-fn-storage-key-here" --secret

# Event Grid Keys
pulumi config set App1Infrastructure:App1EventsTopicKey "your-app1-events-topic-key-here" --secret

# SendGrid Key
pulumi config set App1Infrastructure:SendGridKey "your-sendgrid-key-here" --secret

# Additional blob keys
pulumi config set App1Infrastructure:PrimaryBlobKey "your-primary-blob-key-here" --secret
pulumi config set App1Infrastructure:SecondaryBlobKey "your-secondary-blob-key-here" --secret
pulumi config set App1Infrastructure:App1BlobKey "your-app1-blob-key-here" --secret

Write-Host "Secrets added successfully!"
Write-Host "Please update the secret values with actual values from your secure store."
