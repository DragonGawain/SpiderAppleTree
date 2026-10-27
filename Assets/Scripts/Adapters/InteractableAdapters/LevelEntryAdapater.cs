using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Serialization.Json;
using UnityEngine;

public class LevelEntryAdapater : IJsonAdapter<LevelEntry>
{
    public void Serialize(in JsonSerializationContext<LevelEntry> context, LevelEntry value)
    {
        using var os = context.Writer.WriteObjectScope();
        context.Writer.WriteKey("Coord");
        context.SerializeValue(value.coord);
        context.Writer.WriteKeyValue("levelID", value.levelID);
    }

    public LevelEntry Deserialize(in JsonDeserializationContext<LevelEntry> context)
    {
        Debug.Log("Generating level entry object(s)...");
        return new LevelEntry(
            context.DeserializeValue<Coord>(context.SerializedValue["Coord"]),
            context.DeserializeValue<int>(context.SerializedValue["levelID"])
        );
    }
}
