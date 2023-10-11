#-----------------------------------------------------------------------
#Please read the README.txt file before uploading or modifying this file
#-----------------------------------------------------------------------

#Local testing
$TrainingUrl = "http://localhost:64567/api/ASPIRE/ImportTrainings";
$ReliabilityUrl = "http://localhost:64567/api/ASPIRE/ImportReliabilityRecords";

#The id and secret are in the appSettings.config file (DO NOT COMMIT THEM TO SOURCE CONTROL!)
$PIDSId = "";
$PIDSSecret = "";

#Get the current date now so that it won't change between setting the start and end dates
$CurrentDate = (Get-Date);

#Split the last 6 months into two calls to ensure that it runs properly and before timeout
#This first call should use a start date of 6 months in the past and an end date of 3 months in the past
$FirstCallBody = @{
    id = $PIDSId;
    secret = $PIDSSecret;
    startDate = $CurrentDate.AddMonths(-6);
    endDate = $CurrentDate.AddMonths(-3);
}

#This second call should use a start date of 3 months in the past and an end date of today
$SecondCallBody = @{
    id = $PIDSId;
    secret = $PIDSSecret;
    startDate = $CurrentDate.AddMonths(-3);
    endDate = $CurrentDate;
}

#Set the security protocol to use TLS 1.2 (prevents connection errors)
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12;

#Call the API to import the trainings
#First call
Write-Output ("{0}: {1}" -f "First training call", (Get-Date));
$FirstTrainingCallResponse = Invoke-RestMethod -Method 'Post' -Uri $TrainingUrl -Body $FirstCallBody;
Write-Output $FirstTrainingCallResponse;

#Separate the responses
Write-Output "`n------------------------------`n";

#Second call
Write-Output ("{0}: {1}" -f "Second training call", (Get-Date));
$SecondTrainingCallResponse = Invoke-RestMethod -Method 'Post' -Uri $TrainingUrl -Body $SecondCallBody;
Write-Output $SecondTrainingCallResponse;

#Separate the responses
Write-Output "`n------------------------------`n";

#Call the API to import the reliability records
#First call
Write-Output ("{0}: {1}" -f "First reliability call", (Get-Date));
$FirstReliabilityCallResponse = Invoke-RestMethod -Method 'Post' -Uri $ReliabilityUrl -Body $FirstCallBody;
Write-Output $FirstReliabilityCallResponse;

#Separate the responses
Write-Output "`n------------------------------`n";

#Second call
Write-Output ("{0}: {1}" -f "Second reliability call", (Get-Date));
$SecondReliabilityCallResponse = Invoke-RestMethod -Method 'Post' -Uri $ReliabilityUrl -Body $SecondCallBody;
Write-Output $SecondReliabilityCallResponse;