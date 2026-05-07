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
    // -> Update uhh, something something what if I want an undo functionality?
    // And what about saving partial solutions? I thought partial solutions were supposed to be saveable...


    // We serialize levels in the following scenarios:
    // 1. Creating a new (or editing an existing) level
    // 2. Upon a level being solved
    // 3. Partial solution ???
    public void Serialize(in JsonSerializationContext<Level> context, Level value)
    {
        using var os = context.Writer.WriteObjectScope();
        context.Writer.WriteKeyValue("levelID", value.GetLevelID());

        IEnumerable<IGrouping<Type, ILevelElement>> elements = value
            .GetLevelElements()
            .GroupBy(w => w.GetType());

        foreach (var group in elements)
        {
            if (group.Key == typeof(Branch))
            {
                context.Writer.WriteKey("branches");
                using (context.Writer.WriteArrayScope())
                {
                    foreach (Branch b in group)
                        context.SerializeValue(b);
                }
            }
            else if (group.Key == typeof(Trunk))
            {
                context.Writer.WriteKey("trunks");
                using (context.Writer.WriteArrayScope())
                {
                    foreach (Trunk t in group)
                        context.SerializeValue(t);
                }
            }
            // case WebString:
            //     break;
            // case WebSupport:
            //     break;
            else
            {
                Debug.Log(
                    "<color=red>Unknown type of walkable supplied when serializing walkables! Supplied walkable: "
                        + group.Key
                        + "</color>"
                );
            }
        }

        context.Writer.WriteKey("spawn");
        context.SerializeValue(value.GetSpawnPoint());
        context.Writer.WriteKey("goal");
        context.SerializeValue(value.GetGoalPoint());
        context.Writer.WriteKeyValue("initWebCount", value.GetInitWebCount());
        context.Writer.WriteKeyValue("initWeight", value.GetInitWeight());
        context.Writer.WriteKeyValue("initLenght", value.GetInitLenght());
    }

    public Level Deserialize(in JsonDeserializationContext<Level> context)
    {
        // EXTREMELY IMPORTANT that trunks get loaded BEFORE branches
        // See Branch.cs for more details.

        LevelManager
            .GetActiveLevel()
            .InitializeData(
                context.DeserializeValue<Coord>(context.SerializedValue["spawn"]),
                context.DeserializeValue<Coord>(context.SerializedValue["goal"]),
                context.DeserializeValue<int>(context.SerializedValue["initWebCount"]),
                context.DeserializeValue<int>(context.SerializedValue["initWeight"]),
                context.DeserializeValue<int>(context.SerializedValue["initLenght"])
            );
        context.DeserializeValue<List<Trunk>>(context.SerializedValue["trunks"]);
        context.DeserializeValue<List<Branch>>(context.SerializedValue["branches"]);

        return null;
    }
}
