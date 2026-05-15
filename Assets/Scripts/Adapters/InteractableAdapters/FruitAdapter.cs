using Unity.Serialization.Json;

public class FruitAdapter : IJsonAdapter<Fruit>
{
    public void Serialize(in JsonSerializationContext<Fruit> context, Fruit value)
    {
        using var os = context.Writer.WriteObjectScope();
        context.Writer.WriteKey("Coord");
        context.SerializeValue(value.coord);
        context.Writer.WriteKeyValue("fruitType", (int)value.fruitType);
    }

    public Fruit Deserialize(in JsonDeserializationContext<Fruit> context)
    {
        return new Fruit(
            context.DeserializeValue<Coord>(context.SerializedValue["Coord"]),
            context.DeserializeValue<int>(context.SerializedValue["fruitType"])
        );
    }
}
