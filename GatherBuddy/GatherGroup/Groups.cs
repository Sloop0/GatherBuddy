using System.Collections.Generic;
using GatherBuddy.Classes;

namespace GatherBuddy.GatherGroup;

public static class GroupData
{
    private static void Add(IDictionary<string, TimedGroup> dict, string name, string desc,
        params (GatheringNode? node, string? desc)[] nodes)
        => dict.Add(name, new TimedGroup(name, desc, nodes));

    public static Dictionary<string, TimedGroup> CreateGroups()
    {
        var dict = new Dictionary<string, TimedGroup>();

        var           nodes = GatherBuddy.GameData.GatheringNodes;
        Add(dict, "80***", "Contains exarchic crafting nodes."
            ,     (nodes.TryGetValue(758, out var n) ? n : null, null) // Hard Water
            ,     (nodes.TryGetValue(759, out n) ? n : null, null)               // Solstice Stone
            ,     (nodes.TryGetValue(760, out n) ? n : null, null)               // Dolomite
            ,     (nodes.TryGetValue(761, out n) ? n : null, null)               // Wattle Petribark
            ,     (nodes.TryGetValue(762, out n) ? n : null, null)               // Silver Beech Log
            ,     (nodes.TryGetValue(763, out n) ? n : null, null)               // Raindrop Cotton Boll               
        );

        Add(dict, "80**", "Contains neo-ishgardian / aesthete crafting nodes."
            ,     (nodes.TryGetValue(681, out n) ? n : null, null) // Brashgold
            ,     (nodes.TryGetValue(682, out n) ? n : null, null) // Purpure
            ,     (nodes.TryGetValue(683, out n) ? n : null, null) // Merbau
            ,     (nodes.TryGetValue(684, out n) ? n : null, null) // Tender Dill
            ,     (nodes.TryGetValue(713, out n) ? n : null, null) // Ashen Alumen
            ,     (nodes.TryGetValue(714, out n) ? n : null, null) // Duskblooms
        );

        Add(dict, "levinsand", "Contains Shadowbringers aethersand reduction nodes."
            ,     (nodes.TryGetValue(622, out n) ? n : null, null)
            ,     (nodes.TryGetValue(624, out n) ? n : null, null)
            ,     (nodes.TryGetValue(626, out n) ? n : null, null)
            ,     (nodes.TryGetValue(597, out n) ? n : null, null)
            ,     (nodes.TryGetValue(599, out n) ? n : null, null)
            ,     (nodes.TryGetValue(601, out n) ? n : null, null)
        );

        Add(dict, "dusksand", "Contains Stormblood aethersand reduction nodes."
            ,     (nodes.TryGetValue(515, out n) ? n : null, null)
            ,     (nodes.TryGetValue(518, out n) ? n : null, null)
            ,     (nodes.TryGetValue(520, out n) ? n : null, null)
            ,     (nodes.TryGetValue(494, out n) ? n : null, null)
            ,     (nodes.TryGetValue(496, out n) ? n : null, null)
            ,     (nodes.TryGetValue(492, out n) ? n : null, null)
        );

        Add(dict, "80ws", "Contains Shadowbringers white scrip collectibles."
            ,     (nodes.TryGetValue(781, out n) ? n : null, "Rarefied Manasilver Sand")    // 6
            ,     (nodes.TryGetValue(777, out n) ? n : null, "Rarefied Urunday Log")        // 0
            ,     (nodes.TryGetValue(775, out n) ? n : null, "Rarefied Amber Cloves")       // 2
            ,     (nodes.TryGetValue(776, out n) ? n : null, "Rarefied Coral")              // 4
            ,     (nodes.TryGetValue(334, out n) ? n : null, "Rarefied Raw Onyx")           // 8
            ,     (nodes.TryGetValue(767, out n) ? n : null, "Rarefied Gyr Abanian Alumen") // 10
        );

        Add(dict, "80ys", "Contains Shadowbringers yellow scrip collectibles."
            ,     (nodes.TryGetValue(784, out n) ? n : null, "Rarefied Bright Flax")       // 0
            ,     (nodes.TryGetValue(766, out n) ? n : null, "Rarefied Reef Rock")         // 2
            ,     (nodes.TryGetValue(330, out n) ? n : null, "Rarefied Raw Petalite")      // 4
            ,     (nodes.TryGetValue(332, out n) ? n : null, "Rarefied Raw Lazurite")      // 6
            ,     (nodes.TryGetValue(334, out n) ? n : null, "Rarefied Sea Salt")          // 8
            ,     (nodes.TryGetValue(773, out n) ? n : null, "Rarefied Miracle Apple Log") // 10
        );

        Add(dict, "80ysmin", "Contains Shadowbringers yellow scrip miner collectibles."
            ,     (nodes.TryGetValue(780, out n) ? n : null, "Rarefied Bluespirit Ore") // 0, 10
            ,     (nodes.TryGetValue(766, out n) ? n : null, "Rarefied Reef Rock")      // 2
            ,     (nodes.TryGetValue(330, out n) ? n : null, "Rarefied Raw Petalite")   // 4
            ,     (nodes.TryGetValue(332, out n) ? n : null, "Rarefied Raw Lazurite")   // 6
            ,     (nodes.TryGetValue(334, out n) ? n : null, "Rarefied Sea Salt")       // 8
        );

        Add(dict, "80ysbot", "Contains Shadowbringers yellow scrip botanist collectibles."
            ,     (nodes.TryGetValue(784, out n) ? n : null, "Rarefied Bright Flax")       // 0, 6
            ,     (nodes.TryGetValue(775, out n) ? n : null, "Rarefied Sandteak Log")      // 2
            ,     (nodes.TryGetValue(776, out n) ? n : null, "Rarefied Kelp")              // 4
            ,     (nodes.TryGetValue(774, out n) ? n : null, "Rarefied White Oak Log")     // 8
            ,     (nodes.TryGetValue(773, out n) ? n : null, "Rarefied Miracle Apple Log") // 10
        );

        Add(dict, "70ys", "Contains Stormblood yellow scrip collectibles."
            ,     (nodes.TryGetValue(772, out n) ? n : null, "Rarefied Pine Log")          // 0
            ,     (nodes.TryGetValue(770, out n) ? n : null, "Rarefied Larch Log")         // 2
            ,     (nodes.TryGetValue(771, out n) ? n : null, "Rarefied Shiitake Mushroom") // 4
            ,     (nodes.TryGetValue(328, out n) ? n : null, "Rarefied Silvergrace Ore")   // 6
            ,     (nodes.TryGetValue(310, out n) ? n : null, "Rarefied Raw Kyanite")       // 8
            ,     (nodes.TryGetValue(312, out n) ? n : null, "Rarefied Raw Star Spinel")   // 10
        );

        Add(dict, "70ysmin", "Contains Stormblood yellow scrip miner collectibles."
            ,     (nodes.TryGetValue(779, out n) ? n : null, "Rarefied Gyr Abanian Mineral Water") // 0, 2, 4
            ,     (nodes.TryGetValue(328, out n) ? n : null, "Rarefied Silvergrace Ore")           // 6
            ,     (nodes.TryGetValue(310, out n) ? n : null, "Rarefied Raw Kyanite")               // 8
            ,     (nodes.TryGetValue(312, out n) ? n : null, "Rarefied Raw Star Spinel")           // 10
        );

        Add(dict, "70ysbot", "Contains Stormblood yellow scrip botanist collectibles."
            ,     (nodes.TryGetValue(783, out n) ? n : null, "Rarefied Bloodhemp")         // 6, 8, 10
            ,     (nodes.TryGetValue(772, out n) ? n : null, "Rarefied Pine Log")          // 0
            ,     (nodes.TryGetValue(770, out n) ? n : null, "Rarefied Larch Log")         // 2
            ,     (nodes.TryGetValue(771, out n) ? n : null, "Rarefied Shiitake Mushroom") // 4
        );

        Add(dict, "60ys", "Contains Heavensward yellow scrip collectibles."
            ,     (nodes.TryGetValue(778, out n) ? n : null, "Rarefied Mythrite Sand")     // 6, 8
            ,     (nodes.TryGetValue(769, out n) ? n : null, "Rarefied Dark Chestnut")     // 0
            ,     (nodes.TryGetValue(308, out n) ? n : null, "Rarefied Aurum Regis Sand")  // 2
            ,     (nodes.TryGetValue(306, out n) ? n : null, "Rarefied Limonite")          // 4
            ,     (nodes.TryGetValue(768, out n) ? n : null, "Rarefied Dark Chestnut Log") // 10
        );

        Add(dict, "60ysmin", "Contains Heavensward yellow scrip miner collectibles."
            ,     (nodes.TryGetValue(778, out n) ? n : null, "Rarefied Mythrite Sand")    // 0, 6, 8, 10
            ,     (nodes.TryGetValue(308, out n) ? n : null, "Rarefied Aurum Regis Sand") // 2
            ,     (nodes.TryGetValue(306, out n) ? n : null, "Rarefied Limonite")         // 4
        );

        Add(dict, "60ysbot", "Contains Heavensward yellow scrip botanist collectibles."
            ,     (nodes.TryGetValue(782, out n) ? n : null, "Rarefied Rainbow Cotton Boll") // 2, 4, 6, 8
            ,     (nodes.TryGetValue(769, out n) ? n : null, "Rarefied Dark Chestnut")       // 0
            ,     (nodes.TryGetValue(768, out n) ? n : null, "Rarefied Dark Chestnut Log")   // 10
        );

        return dict;
    }
}
