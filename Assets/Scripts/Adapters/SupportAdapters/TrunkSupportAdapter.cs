using Unity.Serialization.Json;

public class TrunkSupportAdapter : IJsonAdapter<TrunkSupportDTO>
{
    // public void Serialize(in JsonSerializationContext<TrunkSupport> context, TrunkSupport value)
    // {
    //     using var os = context.Writer.WriteObjectScope();
    //     context.Writer.WriteKey("Coord");
    //     context.SerializeValue(value.GetCoord());
    //     context.Writer.WriteKeyValue("delta", value.GetSupportValue());
    // }

    // public TrunkSupport Deserialize(in JsonDeserializationContext<TrunkSupport> context)
    // {
    //     throw new System.NotImplementedException();
    // }
    public void Serialize(
        in JsonSerializationContext<TrunkSupportDTO> context,
        TrunkSupportDTO value
    )
    {
        using var os = context.Writer.WriteObjectScope();
        context.Writer.WriteKey("Coord");
        context.SerializeValue(value.coord);
        context.Writer.WriteKeyValue("delta", value.supportValue);
    }

    public TrunkSupportDTO Deserialize(in JsonDeserializationContext<TrunkSupportDTO> context)
    {
        return new TrunkSupportDTO(
            context.DeserializeValue<Coord>(context.SerializedValue["Coord"]),
            context.DeserializeValue<int>(context.SerializedValue["delta"])
        );
    }
}
