# Protecting your secrets

In this talk we'll discuss some strategies for protecting your secrets.

## Setup

Being by deploying the following resources

1. Resource Group
    - RG-ProtectYourSecrets
1. Log Analytics Workspace
    - LA-ProtectYourSecrets
1. Application 
    - AI-ProtectYourSecrets
1. Azure App Service (F1 - Free tier)
    - ASP-ProtectYourSecrets
    - WebSite-ProtectYourSecrets
1. Azure Storage Account (Standard_LRS)
    - protectsecrets...
    - images container
    - two images named `fawns.jpg` and `chicago.jpg` in the folder (these are seeded in the starter app in the database)
1. Azure SQL Server (Basic Tier - $5/mo)
    - db: dbMVCProtectingSecretsDemo
    - server: dotnetprotectsecretsdbserver
    - secretweb_user 
    - AzureSQL#123! 
1. Azure Key Vault
    - vault: kv-protectsecrets
    - make sure to use the access policies
1. Azure App Configuration
    - appconfig: ProtectYourSecrets-SharedConfig

## Get the starter app

Get the starter app and move it to your own repo

1. Download the files or clone this repo
1. Make sure the application can be deployed to Azure
1. Configure CI/CD so that changes will push to Azure 
1. Deploy to Azure

## First Problem (1) - Database

The database won't work because it isn't migrated.

1. Developer adds the connection string to the appsettings.json file
1. Developer runs `update-database` from local machine to migrate Azure
1. Developer also adds the connection string to the App Service
    - DefaultConnection
    - Server=tcp:yourdbconnection.database.windows.net,1433;Initial Catalog=yourDB;Persist Security Info=False;User ID=yourdbuser;Password=your-pwd;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
1. Developer doesn't push yet but continues to work on the next step and forgets to undo the change in app settings...

## Next Problem (2) - Protect the SAS token and images

Make the storage account private, and blobs private, then create a SAS Token.  Leverage the SAS token from config

1. Put it in appsettings.json (it's already in the code)
1. Make sure to add as a configuration setting at Azure
1. commited to GitHub
1. leaking into logs

## First Solution for 1 & 2 - Utilize Local Secrets

Use the local secrets or app.settings.development.json file to store the connection string.  Make sure to add the development to the .gitignore file if using that option.

1. Add local secrets
1. Move secrets to the local secrets file

## Move Secrets to KeyVault (3) 

Developers should not need to know secrets.  Move the sensitive information into a Key Vault

1. Create the Key Vault (if not exists)
1. Add the secrets to the Key Vault
1. Leverage the Key Vault in the application config
1. Update code to read from KeyVault

## Need to share the secrets

Some secrets and settings need to be shared across multiple applications.  Azure App Configuration is a great way to do this.

1. Move the KeyVault Connection to the Azure App Configuration
1. Leverage Azure App Configuration for the App Settings as the endpoint url
1. Add data reader permission to the app configuration for the app service
1. Ensure app config can read from keyvault as can the app service

## Log Leaks

Take a look at the log leaks.  Yes, this is contrived, but how can you go about logging to make sure that users aren't leaking PII into logs?

See: [https://zimmergren.net/redacting-sensitive-information-application-insights/](https://zimmergren.net/redacting-sensitive-information-application-insights/)  

```c#
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System.Text.RegularExpressions;

namespace ApplicationInsights.RedactSensitiveInformation
{
    /// <summary>
    /// Redacts standardized sensitive information from the trace messages.
    /// </summary>
    internal class SensitivityRedactionTelemetryInitializer : ITelemetryInitializer
    {
        public void Initialize(ITelemetry t)
        {
            var traceTelemetry = t as TraceTelemetry;
            if (traceTelemetry != null)
            {
                // Use Regex to replace any e-mail address with a replacement string.
                traceTelemetry.Message = Regex.Replace(traceTelemetry.Message, @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*", "[PII REDACTED]");
                
                // If we don't remove this CustomDimension, the telemetry message will still contain the PII in the "OriginalFormat" property.
                traceTelemetry.Properties.Remove("OriginalFormat");
            }
        }
    }
}
```

See Also [Zimmergren/cloud-code-samples](https://github.com/Zimmergren/cloud-code-samples/tree/main/ApplicationInsights.RedactSensitiveInformation/ApplicationInsights.RedactSensitiveInformation?ref=zimmergren.net&WT.mc_id=tozimmergren&utm_campaign=zimmergren&utm_medium=blog&utm_source=zimmergren)  

## Final Guard - GitGaurdian (and/or other tools)

Secret leaks are bad. GitGuardian does a great job of letting you know almost immediately if you leak one.

1. Get GitGuardian [https://www.gitguardian.com/]([https://www.gitguardian.com/])
1. Add GitGuardian to your repo
    - Settings
        - Integrations
            - GitHub Apps
                - Configure Git Guardian

## Additional Resources

Other information can be found in these resources

- [Utilize-secrets-in-your-c-mvc-projects](https://training.majorguidancesolutions.com/blog/c-advent-2022-utilize-secrets-in-your-c-mvc-projects)
- [https://zimmergren.net/redacting-sensitive-information-application-insights/](https://zimmergren.net/redacting-sensitive-information-application-insights/)  

