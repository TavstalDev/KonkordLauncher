using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Tavstal.KonkordLauncher.Core.Models;

/// <summary>
/// A custom Newtonsoft.Json contract resolver that suppresses serialization of members
/// that cannot be written to (read-only properties and init-only fields).
/// </summary>
public class IgnoreReadOnlyContractResolver : DefaultContractResolver
{
    /// <summary>
    /// Creates a <see cref="JsonProperty"/> definition for a reflected member and applies
    /// custom serialization rules for read-only members.
    /// </summary>
    /// <param name="member">The reflected member for which a JSON contract property is being created.</param>
    /// <param name="memberSerialization">Indicates the member serialization mode used by Json.NET.</param>
    /// <returns>
    /// A configured <see cref="JsonProperty"/> instance. If the member is read-only, the property is marked
    /// as non-writable and excluded from serialization via <see cref="JsonProperty.ShouldSerialize"/>.
    /// </returns>
    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        JsonProperty property = base.CreateProperty(member, memberSerialization);

        switch (member)
        {
            case PropertyInfo propInfo:
            {
                if (!propInfo.CanWrite)
                {
                    property.ShouldSerialize = _ => false;
                    property.Writable = false;
                }

                break;
            }
            case FieldInfo fieldInfo:
            {
                if (fieldInfo.IsInitOnly)
                {
                    property.ShouldSerialize = _ => false;
                    property.Writable = false;
                }

                break;
            }
        }

        return property;
    }
}