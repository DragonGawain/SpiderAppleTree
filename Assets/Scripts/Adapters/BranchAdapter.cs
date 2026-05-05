using System;
using Unity.Serialization.Json;
using UnityEngine;

public class BranchAdapter : IJsonAdapter<Branch>
{
    public void Serialize(in JsonSerializationContext<Branch> context, Branch value)
    {
        using var os = context.Writer.WriteObjectScope();
        context.Writer.WriteKey("Coord");
        context.SerializeValue(value.coord);
        context.Writer.WriteKeyValue("length", value.length);
        context.Writer.WriteKeyValue("direction", (int)value.direction);
        context.Writer.WriteKeyValue("supportLevel", value.supportLevel);
    }

    public Branch Deserialize(in JsonDeserializationContext<Branch> context)
    {
        return new Branch(
            context.DeserializeValue<Coord>(context.SerializedValue["Coord"]),
            context.DeserializeValue<int>(context.SerializedValue["length"]),
            context.DeserializeValue<int>(context.SerializedValue["direction"]),
            context.DeserializeValue<int>(context.SerializedValue["supportLevel"])
        );
    }
}
