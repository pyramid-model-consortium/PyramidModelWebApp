using System;
using Newtonsoft.Json;

namespace Pyramid.Models
{
    /// <summary>
    /// This class defines metadata tags to be used by the AspireTraining class.
    /// </summary>
    public class AspireTrainingMetadata
    {
        public int AspireTrainingPK { get; set; }
        [JsonProperty("eventAttendeeId")]
        public Nullable<int> EventAttendeeID { get; set; }
        [JsonProperty("courseId")]
        public Nullable<int> CourseID { get; set; }
        [JsonProperty("courseTitle")]
        public string CourseTitle { get; set; }
        [JsonProperty("eventId")]
        public Nullable<int> EventID { get; set; }
        [JsonProperty("eventTitle")]
        public string EventTitle { get; set; }
        [JsonProperty("eventStartDate")]
        public Nullable<System.DateTime> EventStartDate { get; set; }
        [JsonProperty("eventCompletionDate")]
        public Nullable<System.DateTime> EventCompletionDate { get; set; }
        [JsonProperty("aspireId")]
        public Nullable<int> AspireID { get; set; }
        [JsonProperty("fullName")]
        public string FullName { get; set; }
        [JsonProperty("attended")]
        public Nullable<bool> Attended { get; set; }
        [JsonProperty("tpotReliability")]
        public Nullable<bool> TPOTReliability { get; set; }
        [JsonProperty("tpotReliabilityExpirationDate")]
        public Nullable<System.DateTime> TPOTReliabilityExpirationDate { get; set; }
        [JsonProperty("tpitosReliability")]
        public Nullable<bool> TPITOSReliability { get; set; }
        [JsonProperty("tpitosReliabilityExpirationDate")]
        public Nullable<System.DateTime> TPITOSReliabilityExpirationDate { get; set; }
    }
}