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
            .GroupBy(e => e.GetType());

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
            else if (group.Key == typeof(Fruit))
            {
                context.Writer.WriteKey("fruits");
                using (context.Writer.WriteArrayScope())
                {
                    foreach (Fruit f in group)
                        context.SerializeValue(f);
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

        InitialLevelDataContainer ldc = value.GetInitialLevelDataContainer();
        context.Writer.WriteKey("spawn");
        context.SerializeValue(ldc.spawnPoint);
        context.Writer.WriteKey("goal");
        context.SerializeValue(ldc.goalPoint);
        context.Writer.WriteKeyValue("initWebCount", ldc.initWebCount);
        context.Writer.WriteKeyValue("initWeight", ldc.initWeight);
        context.Writer.WriteKeyValue("initLength", ldc.initLength);
        context.Writer.WriteKeyValue("trunkSupport", ldc.baseTrunkSupport);
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
                context.DeserializeValue<int>(context.SerializedValue["initLength"]),
                context.DeserializeValue<int>(context.SerializedValue["trunkSupport"])
            );
        context.DeserializeValue<List<Trunk>>(context.SerializedValue["trunks"]);
        context.DeserializeValue<List<Branch>>(context.SerializedValue["branches"]);
        context.DeserializeValue<List<Fruit>>(context.SerializedValue["fruits"]);

        return null;
    }
}
