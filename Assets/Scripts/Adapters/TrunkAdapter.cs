using Unity.Serialization.Json;

public class TrunkAdapter : IJsonAdapter<Trunk>
{
    public void Serialize(in JsonSerializationContext<Trunk> context, Trunk value)
    {
        using var os = context.Writer.WriteObjectScope();
        context.Writer.WriteKey("Coord");
        context.SerializeValue(value.coord);
        context.Writer.WriteKeyValue("height", value.height);
    }

    public Trunk Deserialize(in JsonDeserializationContext<Trunk> context)
    {
        return new Trunk(
            context.DeserializeValue<Coord>(context.SerializedValue["Coord"]),
            context.DeserializeValue<int>(context.SerializedValue["height"])
        );
    }
}
