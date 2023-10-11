The Powershell script in this folder is designed to be uploaded to an Azure Web App's WebJob section as a scheduled job.

CRONTAB schedule: 0 30 20 * * SUN.  Explanation: Every Sunday at 20:30 (8:30 PM).

When uploading to the live site, ensure that both the staging and main slots have updated WebJobs, but stagger their schedule.  For example, if you set the main slot to run at 8:30 PM, set the staging to run at 10:30 PM.

The staging slot will NOT run the webjob as long as you have the WEBJOBS_STOPPED application setting set to the number 1.  DO NOT add the WEBJOBS_STOPPED setting to the main slot.

The id and secret can be found in the appSettings.config file and SHOULD NEVER be committed to source control.

Ensure that, before uploading to the WebJob section, only one CORRECT URL is uncommented and the id and secret are in the file.

After uploading, remove the id and secret from this file before saving to ensure that they are not in source control.