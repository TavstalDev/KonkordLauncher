using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Tavstal.KonkordLauncher.Core.Models;

public class IgnoreReadOnlyContractResolver : DefaultContractResolver
{
    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        JsonProperty property = base.CreateProperty(member, memberSerialization);

        if (member is PropertyInfo propInfo)
        {
            if (!propInfo.CanWrite)
            {
                property.ShouldSerialize = _ => false;
                property.Writable = false;
            }
        }
        else if (member is FieldInfo fieldInfo)
        {
            if (fieldInfo.IsInitOnly)
            {
                property.ShouldSerialize = _ => false;
                property.Writable = false;
            }
        }

        return property;
    }
}