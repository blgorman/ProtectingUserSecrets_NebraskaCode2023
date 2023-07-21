using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System.Text.RegularExpressions;

namespace MVCProtectingSecrets.Initializers
{
    public class LogSanitizerInsightsInitializer : ITelemetryInitializer
    {
        public static string SanitizeString(string msg)
        {
            var regexEmail = @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";
            var replacedEmail = "[emailaddress]";
            //https://github.blog/changelog/2021-10-18-secret-scanning-no-longer-supports-azure-sql-connection-strings-in-private-repos/
            var regexSQLConnectionString = @"(?i)[a-z][a-z0-9-]+\.database(?:\.secure)?\.(?:(?:windows|usgovcloudapi)\.net|chinacloudapi\.cn|cloudapi\.de)";
            var cnstrReplaced = "[redacted-server]";

            var regexInitialCatalog = @"Initial Catalog=[a-zA-Z0-9-]*";
            var catReplace = "Intiial Catalog=[redacted-db]";

            var regexUserID = @"User ID=[a-zA-Z0-9-]*";
            var userIDReplace = "User ID=[redacted-user]";
            var regexPassword = @"Password=[a-zA-Z0-9-!@#$%^&*()_]*";
            var passwordReplace = "Password=[redacted-password]";

            var regexSASToken = @"sig=[a-zA-Z0-9%]*";
            var sasReplaced = "[sastoken]";

            //sanitize log message:
            msg = Regex.Replace(msg, regexEmail, replacedEmail);
            msg = Regex.Replace(msg, regexSQLConnectionString, cnstrReplaced);
            msg = Regex.Replace(msg, regexInitialCatalog, catReplace);
            msg = Regex.Replace(msg, regexUserID, userIDReplace);
            msg = Regex.Replace(msg, regexPassword, passwordReplace);
            msg = Regex.Replace(msg, regexSASToken, sasReplaced);

            return msg;
        }

        public void Initialize(ITelemetry telemetry)
        {
            var traceTelemetry = telemetry as TraceTelemetry;

            if (traceTelemetry != null)
            {
                traceTelemetry.Message = LogSanitizerInsightsInitializer.SanitizeString(traceTelemetry.Message);
                // If we don't remove this CustomDimension, the telemetry message will still contain the PII in the "OriginalFormat" property.
                traceTelemetry.Properties.Remove("OriginalFormat");
            }
        }
    }
}
