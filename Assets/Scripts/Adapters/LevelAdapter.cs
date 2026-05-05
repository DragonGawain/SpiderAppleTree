using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Serialization.Json;
using UnityEngine;

public class LevelAdapter : IJsonAdapter<Level>
{
    // We will only save *completed solutions*. This means we need to save the following:
    // trunk locs
    // branch locs
    // web support locs
    // left over web count (?)

    public void Serialize(in JsonSerializationContext<Level> context, Level value)
    {
        using var os = context.Writer.WriteObjectScope();
        context.Writer.WriteKeyValue("levelID", value.GetLevelID());

        IEnumerable<IGrouping<Type, ILevelElement>> walkables = value
            .GetIsWalkable()
            .Values.Distinct()
            .GroupBy(w => w.GetType());

        foreach (var group in walkables)
        {
            switch ((ILevelElement)group.Key)
            {
                case Branch:
                    context.Writer.WriteKey("branches");
                    using (context.Writer.WriteArrayScope())
                    {
                        foreach (Branch b in group)
                            context.SerializeValue(b);
                    }
                    break;
                case Trunk:
                    context.Writer.WriteKey("trunks");
                    using (context.Writer.WriteArrayScope())
                    {
                        foreach (Trunk t in group)
                            context.SerializeValue(t);
                    }
                    break;
                // case WebString:
                //     break;
                // case WebSupport:
                //     break;
                default:
                    Debug.Log(
                        "<color=red>Unknown type of walkable supplied when serializing walkables!</color>"
                    );
                    break;
            }
        }
    }

    public Level Deserialize(in JsonDeserializationContext<Level> context)
    {
        context.DeserializeValue<List<Branch>>(context.SerializedValue["branches"]);
        context.DeserializeValue<List<Trunk>>(context.SerializedValue["trunks"]);

        return null;
    }
}
