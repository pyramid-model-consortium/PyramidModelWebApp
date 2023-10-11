using System;
using System.Configuration;
using RestSharp;
using Newtonsoft.Json;
using System.Collections.Specialized;
using System.Web;
using System.Linq;
using System.Collections.Generic;
using Pyramid.Models;
using Pyramid.Code;

namespace Pyramid.Aspire
{
    /// <summary>
    /// This is the class that handles interactions with the ASPIRE API
    /// API testing spec: https://nyuat.newworldnow.com/aspire/go/v7/api/nypids/index.html
    /// API live spec: https://nyworksforchildren.org/aspire/go/v7/api/nypids/index.html
    /// </summary>
    public sealed class AspireAPI
    {
        /// <summary>
        /// This is what the bearer token API should return
        /// DO NOT CHANGE THE PROPERTY NAMES UNLESS THE API CHANGES THEM!
        /// </summary>
        public class BearerTokenResponse
        {
            [JsonProperty("access_token")]
            public string AccessToken { get; set; }
            [JsonProperty("expires_in")]
            public int? ExpiresInSeconds { get; set; }
            [JsonProperty("token_type")]
            public string TokenType { get; set; }
        }

        /// <summary>
        /// This is what the member validation API should return
        /// DO NOT CHANGE THE PROPERTY NAMES UNLESS THE API CHANGES THEM!
        /// </summary>
        public class MemberValidationResponse
        {
            [JsonProperty("accountExists")]
            public bool? IsAccountValid { get; set; }
        }

        /// <summary>
        /// This is what the eventAttendee API should return
        /// DO NOT CHANGE THE PROPERTY NAMES UNLESS THE API CHANGES THEM!
        /// </summary>
        public class EventAttendeeResponse
        {
            [JsonProperty("more")]
            public string NextPageURL { get; set; }
            [JsonProperty("data")]
            public List<Models.AspireTraining> Trainings { get; set; }
            [JsonProperty("totalCount")]
            public int? TotalTrainings { get; set; }
            [JsonProperty("totalPages")]
            public int? TotalPages { get; set; }
        }

        /// <summary>
        /// This is what the reliabilites API endpoint should return
        /// DO NOT CHANGE THE PROPERTY NAMES UNLESS THE API CHANGES THEM!
        /// </summary>
        public class ReliabilitiesResponse
        {
            [JsonProperty("more")]
            public string NextPageURL { get; set; }
            [JsonProperty("data")]
            public List<EmployeeReliabilityInfo> EmployeeReliabilityRecords { get; set; }
            [JsonProperty("totalCount")]
            public int? TotalTrainings { get; set; }
            [JsonProperty("totalPages")]
            public int? TotalPages { get; set; }
        }

        /// <summary>
        /// This is what the reliabilites API endpoint should return
        /// DO NOT CHANGE THE PROPERTY NAMES UNLESS THE API CHANGES THEM!
        /// </summary>
        public class EmployeeReliabilityInfo
        {
            [JsonProperty("aspireId")]
            public Nullable<int> AspireID { get; set; }
            [JsonProperty("fullName")]
            public string FullName { get; set; }
            [JsonProperty("emailAddress")]
            public string EmailAddress { get; set; }
            [JsonProperty("tpotReliability")]
            public List<ReliabilityRecord> TPOTReliabilityRecords { get; set; }
            [JsonProperty("tpitosReliability")]
            public List<ReliabilityRecord> TPITOSReliabilityRecords { get; set; }
        }

        /// <summary>
        /// This is what the reliabilites API endpoint should return
        /// DO NOT CHANGE THE PROPERTY NAMES UNLESS THE API CHANGES THEM!
        /// </summary>
        public class ReliabilityRecord
        {
            [JsonProperty("isActive")]
            public Nullable<bool> IsActive { get; set; }
            [JsonProperty("eligibilityDate")]
            public Nullable<System.DateTime> EligibilityDate { get; set; }
            [JsonProperty("expirationDate")]
            public Nullable<System.DateTime> ExpirationDate { get; set; }
        }

        public sealed class FunctionCalls
        {
            /// <summary>
            /// This method retrieves trainings from ASPIRE that match the passed parameters
            /// </summary>
            /// <param name="whoCalledThis">The username of the user who called this, or, if called by external API, the caller's public IP</param>
            /// <param name="aspireID">The trainee's ASPIRE ID</param>
            /// <param name="startDate">Get reliability records with an eligibility date between this date and the endDate parameter</param>
            /// <param name="endDate">Get reliability records with an eligibility date between the startDate parameter and this</param>
            /// <param name="pageSize">The number of records to include in each page of results (default: 200 & max: 200)</param>
            /// <param name="lastEventAttendeeId">Used to retrieve next pages of results.  The ASPIRE results are sorted by this value descending, so if you send this value, only aspireIds that are less than this will be included in the next page.</param>
            public static void GetTrainings(string whoCalledThis, string emailAddress, int? aspireID, int? courseID, int? eventID, DateTime? eventStartDate, DateTime? eventEndDate, int? pageSize, int? lastEventAttendeeId)
            {
                //This will be set to the number of expected iterations below
                int numExpectedIterations = 1;

                //To hold the bearer token for authorization
                BearerTokenResponse bearerToken = null;

                //To hold the bearer token expiration
                DateTime? bearerTokenExpiration = null;

                //Loop through
                for (int currentIteration = 1; currentIteration <= numExpectedIterations; currentIteration++)
                {
                    //Check to see if we need a new bearer token
                    //Get a new one if it doesn't exist or the expiration time has passed
                    if (bearerToken == null || bearerTokenExpiration.HasValue == false || bearerTokenExpiration.Value <= DateTime.Now)
                    {
                        //Get the bearer token for the request
                        bearerToken = GetBearerToken();

                        //Set the expiration datetime (25 second safety padding)
                        bearerTokenExpiration = DateTime.Now.AddSeconds((bearerToken.ExpiresInSeconds.Value - 25));
                    }

                    //Set up the URL for the API call
                    UriBuilder APICall = new UriBuilder("https://nyworksforchildren.org/aspire/go/v7/api/nypids/eventattendees");

                    //Get the parameters
                    List<string> APIParameters = new List<string>();

                    if (!string.IsNullOrWhiteSpace(emailAddress))
                    {
                        APIParameters.Add(string.Format("emailAddress={0}", HttpUtility.UrlEncode(emailAddress)));
                    }

                    if (aspireID.HasValue)
                    {
                        APIParameters.Add(string.Format("aspireId={0}", HttpUtility.UrlEncode(aspireID.Value.ToString())));
                    }

                    if (courseID.HasValue)
                    {
                        APIParameters.Add(string.Format("courseId={0}", HttpUtility.UrlEncode(courseID.Value.ToString())));
                    }

                    if (eventID.HasValue)
                    {
                        APIParameters.Add(string.Format("eventId={0}", HttpUtility.UrlEncode(eventID.Value.ToString())));
                    }

                    if (eventStartDate.HasValue)
                    {
                        APIParameters.Add(string.Format("eventStartDate={0}", HttpUtility.UrlEncode(eventStartDate.Value.ToString("MM/dd/yyyy"))));
                    }

                    if (eventEndDate.HasValue)
                    {
                        APIParameters.Add(string.Format("eventEndDate={0}", HttpUtility.UrlEncode(eventEndDate.Value.ToString("MM/dd/yyyy"))));
                    }

                    if (pageSize.HasValue)
                    {
                        APIParameters.Add(string.Format("pageSize={0}", HttpUtility.UrlEncode(pageSize.Value.ToString())));
                    }

                    if (lastEventAttendeeId.HasValue)
                    {
                        APIParameters.Add(string.Format("lastEventAttendeeId={0}", HttpUtility.UrlEncode(lastEventAttendeeId.Value.ToString())));
                    }

                    //Set the query string
                    APICall.Query = string.Join("&", APIParameters);

                    //Get the full API call
                    string fullAPICall = APICall.Uri.AbsoluteUri;

                    //Create the RestSharp client for the API call
                    var client = new RestClient(fullAPICall);
                    client.Timeout = 30000;

                    //Create the request
                    var request = new RestRequest(Method.GET);

                    //Add the header
                    request.AddHeader("Authorization", string.Format("Bearer {0}", bearerToken.AccessToken));

                    //Get the response
                    IRestResponse response = client.Execute(request);

                    //Get the results
                    var results = JsonConvert.DeserializeObject<EventAttendeeResponse>(response.Content);

                    //Make sure the result is OK
                    if (response.StatusCode == System.Net.HttpStatusCode.OK && results.TotalPages.HasValue)
                    {
                        if (currentIteration == 1)
                        {
                            //Set the number of expected iterations
                            numExpectedIterations = results.TotalPages.Value;

                            using (PyramidContext context = new PyramidContext())
                            {
                                //Create a new audit record
                                AspireAPIAudit auditRecord = new AspireAPIAudit()
                                {
                                    APICall = fullAPICall,
                                    DateCalled = DateTime.Now,
                                    CalledBy = whoCalledThis
                                };

                                //Record the call in the audit table
                                context.AspireAPIAudit.Add(auditRecord);

                                //Check to see if the date parameters are used
                                if (eventStartDate.HasValue && eventEndDate.HasValue)
                                {
                                    //Delete the trainings for this date range and, optionally, for the aspire ID
                                    context.spDeleteAspireTrainings(aspireID, whoCalledThis, eventStartDate, eventEndDate);
                                }

                                //Save the changes
                                context.SaveChanges();
                            }
                        }

                        using (PyramidContext context = new PyramidContext())
                        {
                            //Add the trainings to the database
                            context.AspireTraining.AddRange(results.Trainings);

                            //Save the changes
                            context.SaveChanges();
                        }

                        //Check to see if there are more trainings
                        if (!string.IsNullOrWhiteSpace(results.NextPageURL) && results.Trainings.Count > 1)
                        {
                            //Get the last training from the list
                            AspireTraining lastTraining = results.Trainings[results.Trainings.Count - 1];

                            //Make sure the last training exists
                            if (lastTraining != null && lastTraining.EventAttendeeID.HasValue)
                            {
                                //Set the last event attendee ID
                                lastEventAttendeeId = lastTraining.EventAttendeeID.Value;
                            }
                            else
                            {
                                //Get the admin's email address
                                string adminEmailAddress = ConfigurationManager.AppSettings["AdminEmailAddress"];

                                //Create an error message
                                string errorMessage = String.Format("Last training or eventAttendeeId was null when getting more pages of trainings from ASPIRE! " +
                                    "Parameters: emailAddress={0}, aspireID={1}, courseID={2}, eventID={3}, eventStartDate={4}, " +
                                    "eventEndDate={5}, pageSize={6}, lastEventAttendeeID={7}",
                                    emailAddress, aspireID, courseID, eventID, eventStartDate, eventEndDate, pageSize, lastEventAttendeeId);

                                //Send an email to the admin about the issue
                                Utilities.SendEmail(adminEmailAddress, "ASPIRE Error: Trainings", "Could not retrieve trainings from ASPIRE!  Please check the error log.", "Login", "/Account/Login.aspx", true);

                                //Create a null reference exception
                                NullReferenceException failedTrainingRequest = new NullReferenceException(errorMessage);

                                //Log the exception
                                Utilities.LogException(failedTrainingRequest);

                                //Throw the exception
                                throw failedTrainingRequest;
                            }
                        }
                        else
                        {
                            using (PyramidContext context = new PyramidContext())
                            {
                                //Import the trainings from the AspireTraining table into the PIDS training table
                                context.spImportAspireTrainings(aspireID);
                            }

                            //Break out of the for loop
                            break;
                        }
                    }
                    else
                    {
                        //The response was not ok, the training request failed
                        //Get the admin's email address
                        string adminEmailAddress = ConfigurationManager.AppSettings["AdminEmailAddress"];

                        //Create an error message
                        string errorMessage = String.Format("Failed request when retrieving trainings from ASPIRE! " +
                            "Parameters: emailAddress={0}, aspireID={1}, courseID={2}, eventID={3}, eventStartDate={4}, " +
                            "eventEndDate={5}, pageSize={6}, lastEventAttendeeID={7}",
                            emailAddress, aspireID, courseID, eventID, eventStartDate, eventEndDate, pageSize, lastEventAttendeeId);

                        //Send an email to the admin about the issue
                        Utilities.SendEmail(adminEmailAddress, "ASPIRE Error: Trainings", "Could not retrieve trainings from ASPIRE!  Please check the error log.", "Login", "/Account/Login.aspx", true);

                        //Create a null reference exception
                        NullReferenceException failedTrainingRequest = new NullReferenceException(errorMessage);

                        //Log the exception
                        Utilities.LogException(failedTrainingRequest);

                        //Throw the exception
                        throw failedTrainingRequest;
                    }
                }
            }

            /// <summary>
            /// This method retrieves reliability information from ASPIRE that match the passed parameters
            /// </summary>
            /// <param name="whoCalledThis">The username of the user who called this, or, if called by external API, the caller's public IP</param>
            /// <param name="aspireID">The trainee's ASPIRE ID</param>
            /// <param name="startDate">Get reliability records with an eligibility date between this date and the endDate parameter</param>
            /// <param name="endDate">Get reliability records with an eligibility date between the startDate parameter and this</param>
            /// <param name="pageSize">The number of records to include in each page of results (default: 200 & max: 200)</param>
            /// <param name="lastAspireId">Used to retrieve next pages of results.  The ASPIRE results are sorted by this value descending, so if you send this value, only aspireIds that are less than this will be included in the next page.</param>
            public static void GetReliabilityRecords(string whoCalledThis, int? aspireID, DateTime? startDate, DateTime? endDate, int? pageSize, int? lastAspireId)
            {
                //This will be set to the number of expected iterations below
                int numExpectedIterations = 1;

                //To hold the bearer token for authorization
                BearerTokenResponse bearerToken = null;

                //To hold the bearer token expiration
                DateTime? bearerTokenExpiration = null;

                //Loop through
                for(int currentIteration = 1; currentIteration <= numExpectedIterations; currentIteration++)
                {
                    //Check to see if we need a new bearer token
                    //Get a new one if it doesn't exist or the expiration time has passed
                    if(bearerToken == null || bearerTokenExpiration.HasValue == false || bearerTokenExpiration.Value <= DateTime.Now)
                    {
                        //Get the bearer token for the request
                        bearerToken = GetBearerToken();

                        //Set the expiration datetime (25 second safety padding)
                        bearerTokenExpiration = DateTime.Now.AddSeconds((bearerToken.ExpiresInSeconds.Value - 25));
                    }

                    //Set up the URL for the API call
                    UriBuilder APICall = new UriBuilder("https://nyworksforchildren.org/aspire/go/v7/api/nypids/reliabilities");

                    //Get the parameters
                    List<string> APIParameters = new List<string>();

                    if (aspireID.HasValue)
                    {
                        APIParameters.Add(string.Format("aspireId={0}", HttpUtility.UrlEncode(aspireID.Value.ToString())));
                    }

                    if (startDate.HasValue)
                    {
                        APIParameters.Add(string.Format("startDate={0}", HttpUtility.UrlEncode(startDate.Value.ToString("MM/dd/yyyy"))));
                    }

                    if (endDate.HasValue)
                    {
                        APIParameters.Add(string.Format("endDate={0}", HttpUtility.UrlEncode(endDate.Value.ToString("MM/dd/yyyy"))));
                    }

                    if (pageSize.HasValue)
                    {
                        APIParameters.Add(string.Format("pageSize={0}", HttpUtility.UrlEncode(pageSize.Value.ToString())));
                    }

                    if (lastAspireId.HasValue)
                    {
                        APIParameters.Add(string.Format("lastAspireId={0}", HttpUtility.UrlEncode(lastAspireId.Value.ToString())));
                    }

                    //Set the query string
                    APICall.Query = string.Join("&", APIParameters);

                    //Get the full API call
                    string fullAPICall = APICall.Uri.AbsoluteUri;

                    //Create the RestSharp client for the API call
                    var client = new RestClient(fullAPICall);
                    client.Timeout = 30000;

                    //Create the request
                    var request = new RestRequest(Method.GET);

                    //Add the header
                    request.AddHeader("Authorization", string.Format("Bearer {0}", bearerToken.AccessToken));

                    //Get the response
                    IRestResponse response = client.Execute(request);

                    //Get the results
                    var results = JsonConvert.DeserializeObject<ReliabilitiesResponse>(response.Content);

                    //Make sure the result is OK
                    if (response.StatusCode == System.Net.HttpStatusCode.OK && results.TotalPages.HasValue)
                    {
                        if (currentIteration == 1)
                        {
                            //Set the number of expected iterations
                            numExpectedIterations = results.TotalPages.Value;

                            using (PyramidContext context = new PyramidContext())
                            {
                                //Create a new audit record
                                AspireAPIAudit auditRecord = new AspireAPIAudit()
                                {
                                    APICall = fullAPICall,
                                    DateCalled = DateTime.Now,
                                    CalledBy = whoCalledThis
                                };

                                //Record the call in the audit table
                                context.AspireAPIAudit.Add(auditRecord);

                                //Check to see if the date parameters are used
                                if (startDate.HasValue && endDate.HasValue)
                                {
                                    //Delete the reliability records for this date range and, optionally, for the aspire ID
                                    context.spDeleteAspireReliabilityRecords(aspireID, whoCalledThis, startDate, endDate);
                                }

                                //Save the changes
                                context.SaveChanges();
                            }
                        }

                        //To hold the reliability records to save to the database
                        List<AspireReliability> recordsToSave = new List<AspireReliability>();

                        //TPITOS
                        List<EmployeeReliabilityInfo> employeesWithTPITOSReliability = results.EmployeeReliabilityRecords.Where(r => r.TPITOSReliabilityRecords.Count > 0).ToList();

                        foreach(EmployeeReliabilityInfo reliablityInfo in employeesWithTPITOSReliability)
                        {
                            //Get the tpitos reliability records
                            List<AspireReliability> validTPITOSReliabilities = reliablityInfo.TPITOSReliabilityRecords.Select(r => new AspireReliability()
                            {
                                ReliabilityType = "TPITOS",
                                AspireID = reliablityInfo.AspireID,
                                EmailAddress = reliablityInfo.EmailAddress,
                                FullName = reliablityInfo.FullName,
                                IsActive = r.IsActive,
                                EligibilityDate = r.EligibilityDate,
                                ExpirationDate = r.ExpirationDate
                            }).ToList();

                            if (validTPITOSReliabilities.Count > 0)
                            {
                                recordsToSave.AddRange(validTPITOSReliabilities);
                            }
                        }

                        //TPOT
                        List<EmployeeReliabilityInfo> employeesWithTPOTReliability = results.EmployeeReliabilityRecords.Where(r => r.TPOTReliabilityRecords.Count > 0).ToList();

                        foreach (EmployeeReliabilityInfo reliablityInfo in employeesWithTPOTReliability)
                        {
                            //Get the tpot reliability records
                            List<AspireReliability> validTPOTReliabilities = reliablityInfo.TPOTReliabilityRecords.Select(r => new AspireReliability()
                            {
                                ReliabilityType = "TPOT",
                                AspireID = reliablityInfo.AspireID,
                                EmailAddress = reliablityInfo.EmailAddress,
                                FullName = reliablityInfo.FullName,
                                IsActive = r.IsActive,
                                EligibilityDate = r.EligibilityDate,
                                ExpirationDate = r.ExpirationDate
                            }).ToList();

                            if(validTPOTReliabilities.Count > 0)
                            {
                                recordsToSave.AddRange(validTPOTReliabilities);
                            }
                        }

                        //Make sure that records exist
                        if (recordsToSave.Count > 0)
                        {
                            using (PyramidContext context = new PyramidContext())
                            {
                                //Add the reliability records to the database
                                context.AspireReliability.AddRange(recordsToSave);

                                //Save the changes
                                context.SaveChanges();
                            }
                        }

                        //Check to see if there are more reliability records
                        if (!string.IsNullOrWhiteSpace(results.NextPageURL) && results.EmployeeReliabilityRecords.Count > 1)
                        {
                            //Get the last reliability record from the list
                            EmployeeReliabilityInfo lastReliabilityRecord = results.EmployeeReliabilityRecords[results.EmployeeReliabilityRecords.Count - 1];

                            //Make sure the last reliability record exists                            
                            if (lastReliabilityRecord != null && lastReliabilityRecord.AspireID.HasValue)
                            {
                                //Set the last aspire ID
                                lastAspireId = lastReliabilityRecord.AspireID.Value;
                            }
                            else 
                            {
                                //Get the admin's email address
                                string adminEmailAddress = ConfigurationManager.AppSettings["AdminEmailAddress"];

                                //Create an error message
                                string errorMessage = String.Format("Last reliability record or aspireId was null when getting more pages of reliability records from ASPIRE! " +
                                    "Parameters: aspireID={0}, startDate={1}, eventEndDate={2}, pageSize={3}, lastAspireId={4}",
                                    aspireID, startDate, endDate, pageSize, lastAspireId);

                                //Send an email to the admin about the issue
                                Utilities.SendEmail(adminEmailAddress, "ASPIRE Error: Reliability Records", "Could not retrieve reliability records from ASPIRE!  Please check the error log.", "Login", "/Account/Login.aspx", true);

                                //Create a null reference exception
                                NullReferenceException failedReliabilityRequest = new NullReferenceException(errorMessage);

                                //Log the exception
                                Utilities.LogException(failedReliabilityRequest);

                                //Throw the exception
                                throw failedReliabilityRequest;
                            }
                        }
                        else
                        {
                            using (PyramidContext context = new PyramidContext())
                            {
                                //Import the reliability records from the AspireReliability table into the PIDS training table
                                context.spImportAspireReliabilityRecords(aspireID);
                            }

                            //Break out of the for loop
                            break;
                        }
                    }
                    else
                    {
                        //The response was not ok, the reliability records request failed
                        //Get the admin's email address
                        string adminEmailAddress = ConfigurationManager.AppSettings["AdminEmailAddress"];

                        //Create an error message
                        string errorMessage = String.Format("Failed request when retrieving reliability records from ASPIRE! " +
                            "Parameters: aspireID={0}, startDate={1}, eventEndDate={2}, pageSize={3}, lastAspireId={4}",
                                    aspireID, startDate, endDate, pageSize, lastAspireId);

                        //Send an email to the admin about the issue
                        Utilities.SendEmail(adminEmailAddress, "ASPIRE Error: Reliability Records", "Could not retrieve reliability records from ASPIRE!  Please check the error log.", "Login", "/Account/Login.aspx", true);

                        //Create a null reference exception
                        NullReferenceException failedReliabilityRequest = new NullReferenceException(errorMessage);

                        //Log the exception
                        Utilities.LogException(failedReliabilityRequest);

                        //Throw the exception
                        throw failedReliabilityRequest;
                    }
                }
            }

            /// <summary>
            /// This method validates the supplied email and ID to tell if an ASPIRE account exists
            /// with both of those fields matching.
            /// </summary>
            /// <param name="whoCalledThis">The username of the user who called this, or, if called by external API, the caller's public IP</param>
            /// <param name="aspireEmail">The ASPIRE account email address</param>
            /// <param name="aspireID">The ASPIRE account ID</param>
            /// <returns>True if the account exists, false otherwise</returns>
            public static bool IsAspireAccountValid(string whoCalledThis, string aspireEmail, int aspireID)
            {
                //Whether or not the account is valid
                bool isAccountValid = false;

                //The bearer token for the request
                BearerTokenResponse bearerToken = GetBearerToken();

                //Get the full API call
                string fullAPICall = string.Format("https://nyworksforchildren.org/aspire/go/v7/api/nypids/members?aspireId={0}&emailAddress={1}", aspireID.ToString(), HttpUtility.UrlEncode(aspireEmail));

                using (PyramidContext context = new PyramidContext())
                {
                    //Create a new audit record
                    AspireAPIAudit auditRecord = new AspireAPIAudit()
                    {
                        APICall = fullAPICall,
                        DateCalled = DateTime.Now,
                        CalledBy = whoCalledThis
                    };

                    //Record the call in the audit table
                    context.AspireAPIAudit.Add(auditRecord);

                    //Save the changes
                    context.SaveChanges();
                }

                //Create the RestSharp client
                var client = new RestClient(fullAPICall);
                client.Timeout = 30000;

                //Create the request
                var request = new RestRequest(Method.GET);

                //Add the header
                request.AddHeader("Authorization", string.Format("Bearer {0}", bearerToken.AccessToken));

                //Get the response
                IRestResponse response = client.Execute(request);

                //Get the results
                var results = JsonConvert.DeserializeObject<MemberValidationResponse>(response.Content);

                //Make sure the result is OK
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    //Make sure the results are valid
                    if (results != null && results.IsAccountValid.HasValue)
                    {
                        //Set the valid bool to the IsAccountValid bool
                        isAccountValid = results.IsAccountValid.Value;
                    }
                    else
                    {
                        //Results are invalid, set the valid bool to false
                        isAccountValid = false;
                    }
                }
                else
                {
                    //The response was not ok, the account is not valid
                    isAccountValid = false;
                }

                //Return the valid bool
                return isAccountValid;
            }

            /// <summary>
            /// This method returns the bearer token for the ASPIRE API
            /// </summary>
            /// <returns>The bearer token</returns>
            public static BearerTokenResponse GetBearerToken()
            {
                //To hold whether or not the request was successful
                bool isSuccessful = false;

                //To hold the bearer token
                BearerTokenResponse bearerToken = null;

                //Create the RestSharp client
                RestClient client = new RestClient("https://identity.newworldnow.com/connect/token");
                client.Timeout = 30000;

                //Create the request
                RestRequest request = new RestRequest(Method.POST);

                //Add the headers
                request.AddHeader("Content-Type", "application/x-www-form-urlencoded");

                //Add the body parameters
                request.AddParameter("client_id", ConfigurationManager.AppSettings["ASPIREClientID"]);
                request.AddParameter("client_secret", ConfigurationManager.AppSettings["ASPIREClientSecret"]);
                request.AddParameter("grant_type", "client_credentials");

                //Execute the request and get the response
                IRestResponse response = client.Execute(request);

                //Make sure the result is OK
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    //Get the results
                    var results = JsonConvert.DeserializeObject<BearerTokenResponse>(response.Content);

                    //Make sure the results are valid
                    if (results != null && !string.IsNullOrWhiteSpace(results.AccessToken))
                    {
                        //Get the bearer token and set the success bool
                        bearerToken = results;
                        isSuccessful = true;
                    }
                    else
                    {
                        //Request failed or token is missing
                        isSuccessful = false;
                    }
                }
                else
                {
                    //Request failed or token is missing
                    isSuccessful = false;
                }

                //Check to see if the request was successful
                if (isSuccessful)
                {
                    //Return the bearer token
                    return bearerToken;
                }
                else
                {
                    //Get the admin's email address
                    string adminEmailAddress = ConfigurationManager.AppSettings["AdminEmailAddress"];

                    //Create an error message
                    string errorMessage = "Could not retrieve the bearer token from ASPIRE!  Please investigate immediately.";

                    //Send an email to the admin about the issue
                    Utilities.SendEmail(adminEmailAddress, "ASPIRE Error: Bearer Token", "Could not retrieve the bearer token from ASPIRE!  Please investigate immediately.", "Login", "/Account/Login.aspx", true);

                    //Create a null reference exception
                    NullReferenceException bearerTokenException = new NullReferenceException(errorMessage);

                    //Log the exception
                    Utilities.LogException(bearerTokenException);

                    //Throw the exception
                    throw bearerTokenException;
                }
            }

            /// <summary>
            /// This method returns the bearer token for the UAT ASPIRE API
            /// </summary>
            /// <returns>The bearer token</returns>
            public static BearerTokenResponse GetUATBearerToken()
            {
                //To hold whether or not the request was successful
                bool isSuccessful = false;

                //To hold the bearer token response
                BearerTokenResponse bearerToken = null;

                //Create the RestSharp client
                RestClient client = new RestClient("https://identityuat.newworldnow.com/connect/token");
                client.Timeout = 30000;

                //Create the request
                RestRequest request = new RestRequest(Method.POST);

                //Add the headers
                request.AddHeader("Content-Type", "application/x-www-form-urlencoded");

                //Add the body parameters
                request.AddParameter("client_id", ConfigurationManager.AppSettings["ASPIRETestClientID"]);
                request.AddParameter("client_secret", ConfigurationManager.AppSettings["ASPIRETestClientSecret"]);
                request.AddParameter("grant_type", "client_credentials");

                //Execute the request and get the response
                IRestResponse response = client.Execute(request);

                //Make sure the result is OK
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    //Get the results
                    var results = JsonConvert.DeserializeObject<BearerTokenResponse>(response.Content);

                    //Make sure the results are valid
                    if (results != null && !string.IsNullOrWhiteSpace(results.AccessToken))
                    {
                        //Get the bearer token and set the success bool
                        bearerToken = results;
                        isSuccessful = true;
                    }
                    else
                    {
                        //Request failed or token is missing
                        isSuccessful = false;
                    }
                }
                else
                {
                    //Request failed or token is missing
                    isSuccessful = false;
                }

                //Check to see if the request was successful
                if (isSuccessful)
                {
                    //Return the bearer token
                    return bearerToken;
                }
                else
                {
                    //Get the admin's email address
                    string adminEmailAddress = ConfigurationManager.AppSettings["AdminEmailAddress"];

                    //Create an error message
                    string errorMessage = "Could not retrieve the bearer token from ASPIRE!  Please investigate immediately.";

                    //Send an email to the admin about the issue
                    Utilities.SendEmail(adminEmailAddress, "ASPIRE Error: Bearer Token", "Could not retrieve the bearer token from ASPIRE!  Please investigate immediately.", "Login", "/Account/Login.aspx", true);

                    //Create a null reference exception
                    NullReferenceException bearerTokenException = new NullReferenceException(errorMessage);

                    //Log the exception
                    Utilities.LogException(bearerTokenException);

                    //Throw the exception
                    throw bearerTokenException;
                }
            }
        }
    }
}