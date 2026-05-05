using Unity.Serialization.Json;

public class CoordAdapter : IJsonAdapter<Coord>
{
    public void Serialize(in JsonSerializationContext<Coord> context, Coord value)
    {
        using var os = context.Writer.WriteObjectScope();
        context.Writer.WriteKeyValue("x", value.x);
        context.Writer.WriteKeyValue("y", value.y);
    }

    public Coord Deserialize(in JsonDeserializationContext<Coord> context)
    {
        return new Coord(
            context.DeserializeValue<int>(context.SerializedValue["x"]),
            context.DeserializeValue<int>(context.SerializedValue["y"])
        );
    }
}
