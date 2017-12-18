using System.ComponentModel.DataAnnotations.Schema;

namespace Toolroom.DataHelper
{
    /// <summary>
    /// All Properties with this attribute are mapped in a xml column (see XmlEntity) instead of a separate column
    /// </summary>
    public class XmlMappedAttribute : NotMappedAttribute
    {
    }
}