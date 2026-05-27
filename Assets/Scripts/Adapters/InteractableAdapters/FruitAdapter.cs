using Unity.Serialization.Json;

public class FruitAdapter : IJsonAdapter<Fruit>
{
    public void Serialize(in JsonSerializationContext<Fruit> context, Fruit value)
    {
        using var os = context.Writer.WriteObjectScope();
        context.Writer.WriteKey("Coord");
        context.SerializeValue(value.coord);
        context.Writer.WriteKeyValue("weight", value.weight);
        context.Writer.WriteKeyValue("dweight", value.weightValue);
        context.Writer.WriteKeyValue("dweb", value.webValue);
        context.Writer.WriteKeyValue("needed", value.needed);
    }

    public Fruit Deserialize(in JsonDeserializationContext<Fruit> context)
    {
        return new Fruit(
            context.DeserializeValue<Coord>(context.SerializedValue["Coord"]),
            context.DeserializeValue<int>(context.SerializedValue["weight"]),
            context.DeserializeValue<int>(context.SerializedValue["dweight"]),
            context.DeserializeValue<int>(context.SerializedValue["dweb"]),
            context.DeserializeValue<bool>(context.SerializedValue["needed"])
        );
    }
}
