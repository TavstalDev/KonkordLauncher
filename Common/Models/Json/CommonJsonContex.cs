using System.Text.Json.Serialization;
using Tavstal.KonkordLauncher.Common.Models.Config;
using Tavstal.KonkordLauncher.Common.Models.Java;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Models.Accounts;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.User;

namespace Tavstal.KonkordLauncher.Common.Models.Json;

[JsonSerializable(typeof(CoreConfig))]
[JsonSerializable(typeof(AccountData))]
[JsonSerializable(typeof(Account))]
[JsonSerializable(typeof(List<Account>))]
[JsonSerializable(typeof(AccountSkin))]
[JsonSerializable(typeof(List<AccountSkin>))]
[JsonSerializable(typeof(MojangProfile))]
[JsonSerializable(typeof(Skin))]
[JsonSerializable(typeof(List<Skin>))]
[JsonSerializable(typeof(Cape))]
[JsonSerializable(typeof(List<Cape>))]
[JsonSerializable(typeof(Instance))]
[JsonSerializable(typeof(List<Instance>))]
[JsonSerializable(typeof(InstanceResource))]
[JsonSerializable(typeof(List<InstanceResource>))]
[JsonSerializable(typeof(PatchNote))]
[JsonSerializable(typeof(List<PatchNote>))]
[JsonSerializable(typeof(MetaCache.MetaCache))]
[JsonSerializable(typeof(List<MetaCache.MetaCache>))]
[JsonSerializable(typeof(MetaCache.MetaCache[]))]
[JsonSerializable(typeof(JavaMirrorConfig))]
[JsonSourceGenerationOptions(WriteIndented = true, IgnoreReadOnlyProperties = true, IgnoreReadOnlyFields = true,
    Converters = [typeof(JsonStringEnumConverter<EAccountType>)])]
public partial class CommonJsonContex : JsonSerializerContext
{
    
}