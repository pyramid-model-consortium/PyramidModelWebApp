using System.ComponentModel.DataAnnotations;

namespace Pyramid.Models
{
    /// <summary>
    /// This assigns the metadata tags from the metadata class 
    /// to the AspireTraining class properties.
    /// </summary>
    [MetadataType(typeof(AspireTrainingMetadata))]
    public partial class AspireTraining
    {

    }
}