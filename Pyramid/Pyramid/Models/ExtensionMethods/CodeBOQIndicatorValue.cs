namespace Pyramid.Models
{
    public partial class CodeBOQIndicatorValue
    {
        /// <summary>
        /// This contains the integer values for the BOQ 2.0
        /// </summary>
        public enum BOQIndicatorValues
        {
            NOT_IN_PLACE = 0,
            PARTIALLY_IN_PLACE = 1,
            IN_PLACE = 2
        }

        /// <summary>
        /// This contains the integer values for the BOQ FCC
        /// </summary>
        public enum BOQFCCIndicatorValues
        {
            NOT_IN_PLACE = 0,
            PARTIALLY_IN_PLACE = 1,
            IN_PLACE = 2,
            NA = 99
        }

        /// <summary>
        /// This contains the integer values for the BOQ FCC V2
        /// </summary>
        public enum BOQFCCV2IndicatorValues
        {
            NOT_IN_PLACE = 0,
            PARTIALLY_IN_PLACE = 1,
            IN_PLACE = 2
        }

        /// <summary>
        /// This contains the integer values for the CWLT BOQ
        /// </summary>
        public enum BOQCWLTIndicatorValues
        {
            NOT_IN_PLACE = 0,
            NEEDS_IMPROVEMENT = 1,
            IN_PLACE = 2,
            NA = 99
        }

        /// <summary>
        /// This contains the integer values for the SLT BOQ
        /// </summary>
        public enum BOQSLTIndicatorValues
        {
            NOT_IN_PLACE = 0,
            EMERGING_NEEDS_IMPROVEMENT = 1,
            IN_PLACE = 2
        }
    }
}