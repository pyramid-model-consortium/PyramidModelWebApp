namespace Pyramid.Models
{
    public partial class CodeLCLResponse
    {
        /// <summary>
        /// This contains the groups
        /// </summary>
        public static class ResponseGroups
        {
            public const string DOMAIN_1 = "Domain1";
            public const string DOMAIN_2 = "Domain2";
            public const string DOMAIN_3 = "Domain3";
            public const string DOMAIN_4 = "Domain4";
            public const string DOMAIN_5 = "Domain5";
            public const string DOMAIN_6 = "Domain6";
            public const string IDENTIFIED_PROGRAM_BARRIERS = "IdentifiedProgramBarriers";
            public const string IDENTIFIED_PROGRAM_STRENGTHS = "IdentifiedProgramStrengths";
            public const string SITE_RESOURCES = "SiteResources";
            public const string TOPICS_DISCUSSED = "TopicsDiscussed";
            public const string TRAININGS_COVERED = "TrainingsCovered";
            public const string TIMELY_PROGRESSION = "TimelyProgressionLikelihood";
            public const string GOAL_COMPLETION = "GoalCompletionLikelihood";
        }

        public static class GoalCompletionLikelihoodPKs
        {
            public const int NOT_APPLICABLE_NO_ACTION_PLAN = 86;
        }

        public static class ProgramStrengthsPKs
        {
            public const int LEADERSHIP_TEAM_ESTABLISHED = 46;
        }

        public static class OtherSpecifyPKs
        {
            public const int DOMAIN_2_SPECIFY = 13;
            public const int SITE_RESOURCES_SPECIFY = 61;
            public const int TOPICS_DISCUSSED_SPECIFY = 70;
            public const int TRAININGS_COVERED_SPECIFY = 78;
        }

        public static class NoOtherItemsPKs
        {
            public const int DOMAIN_1_NONE_PROVIDED = 8;
            public const int DOMAIN_2_NONE_PROVIDED = 14;
            public const int DOMAIN_3_NONE_PROVIDED = 19;
            public const int DOMAIN_4_NONE_PROVIDED = 24;
            public const int DOMAIN_5_NONE_PROVIDED = 27;
            public const int DOMAIN_6_NONE_PROVIDED = 34;
            public const int PROGRAM_BARRIERS_NONE = 45;
            public const int PROGRAM_STRENGTHS_NONE = 56;
            public const int SITE_RESOURCES_NONE_NO_PLAN = 62;
            public const int SITE_RESOURCES_NONE_EFFORTS_UNDERWAY = 63;
            public const int TOPICS_DISCUSSED_NONE = 71;
            public const int TRAININGS_COVERED_NONE = 87;
        }
    }
}