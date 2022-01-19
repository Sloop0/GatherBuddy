using System;
using GatherBuddy.Classes;
using GatherBuddy.Plugin;

namespace GatherBuddy.FishTimer.Parser;

public partial class FishingParser : IDisposable
{
    public event Action<FishingSpot?>? BeganFishing;
    public event Action?               BeganMooching;
    public event Action?               SomethingBit;
    public event Action<Fish>?         CaughtFish;
    public event Action<FishingSpot>?  IdentifiedSpot;

    public FishingParser()
    {
        FishingSpotNames         =  SetupFishingSpotNames();
        Dalamud.Chat.ChatMessage += OnMessageDelegate;
    }

    public void Dispose()
        => Dalamud.Chat.ChatMessage -= OnMessageDelegate;
}
