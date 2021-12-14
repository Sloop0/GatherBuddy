using System.Collections.Generic;
using System.Linq;
using Dalamud;

namespace GatherBuddyA.Classes;

public partial class GatheringNode
{
    public List<Gatherable> Items { get; init; }

    // Print all items separated by '|' or the given separator.
    public string PrintItems(string separator = "|", ClientLanguage lang = ClientLanguage.English)
        => string.Join(separator, Items.Select(it => it.Name[lang]));

    // Node contains any of the given items (in english names).
    public bool HasItems(params Gatherable[] it)
        => it.Length == 0 || Items.Any(it.Contains);

    public bool AddItem(Gatherable item)
    {
        if (Items.Contains(item))
            return item.NodeList.Add(this);

        Items.Add(item);
        item.NodeList.Add(this);
        return true;
    }
}
