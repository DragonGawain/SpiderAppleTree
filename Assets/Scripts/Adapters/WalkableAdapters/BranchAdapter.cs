using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Serialization.Json;

public class BranchAdapter : IJsonAdapter<Branch>
{
    public void Serialize(in JsonSerializationContext<Branch> context, Branch value)
    {
        using var os = context.Writer.WriteObjectScope();
        context.Writer.WriteKey("Coord");
        context.SerializeValue(value.coord);
        context.Writer.WriteKeyValue("length", value.length);
        context.Writer.WriteKeyValue("direction", (int)value.direction);

        IEnumerable<IGrouping<Type, ISupport>> supports = value.supportsList.GroupBy(
            s => s.GetType()
        );

        foreach (var group in supports)
        {
            if (group.Key == typeof(TrunkSupport))
            {
                context.Writer.WriteKey("trunkSupports");
                using (context.Writer.WriteArrayScope())
                {
                    foreach (TrunkSupport ts in group)
                        context.SerializeValue(new TrunkSupportDTO(ts));
                }
            }
            // else if (group.Key == typeof(WebReinforcement))
            // {
            //     context.Writer.WriteKey("webReinforcements");
            //     using (context.Writer.WriteArrayScope())
            //     {
            //         foreach (WebReinforcement wr in group)
            //             context.SerializeValue(new WebReinforcementDTO(wr));
            //     }
            // }
            // else if (group.Key == typeof(HangingWebString))
            // {
            //     context.Writer.WriteKey("hangingWebStrings");
            //     using (context.Writer.WriteArrayScope())
            //     {
            //         foreach (HangingWebString hws in group)
            //             context.SerializeValue(new HangingWebStringDTO(hws));
            //     }
            // }
        }
    }

    public Branch Deserialize(in JsonDeserializationContext<Branch> context)
    {
        Branch b =
            new(
                context.DeserializeValue<Coord>(context.SerializedValue["Coord"]),
                context.DeserializeValue<int>(context.SerializedValue["length"]),
                context.DeserializeValue<int>(context.SerializedValue["direction"]),
                false
            );

        List<TrunkSupportDTO> trunkSupportDTOs = context.DeserializeValue<List<TrunkSupportDTO>>(
            context.SerializedValue["trunkSupports"]
        );
        // List<WebReinforcementDTO> WebReinforcementDTOs = context.DeserializeValue<List<WebReinforcementDTO>>(
        //     context.SerializedValue["webReinforcements"]
        // );
        // List<HangingWebStringDTO> HangingWebStringDTOs = context.DeserializeValue<List<HangingWebStringDTO>>(
        //     context.SerializedValue["hangingWebStrings"]
        // );
        foreach (TrunkSupportDTO tsDTO in trunkSupportDTOs)
            new TrunkSupport(b, tsDTO);

        // foreach (WebReinforcementDTO wrDTO in WebReinforcementDTOs)
        //     new WebReinforcement(b, wrDTO);

        // foreach (HangingWebStringDTO hwsDTO in HangingWebStringDTOs)
        //     new HangingWebString(b, hwsDTO);

        return b;
    }
}
