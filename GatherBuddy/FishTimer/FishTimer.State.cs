using System.Diagnostics;
using Dalamud.Logging;
using GatherBuddy.Caching;
using GatherBuddy.Classes;
using GatherBuddy.Structs;

namespace GatherBuddy.FishTimer;

public partial class FishTimer
{
    public const uint SnaggingEffectId  = 761;
    public const uint ChumEffectId      = 763;
    public const uint IntuitionEffectId = 568;
    public const uint FishEyesEffectId  = 762;

    private readonly Stopwatch     _start = new();
    private          FishingSpot?  _currentSpot;
    private          Bait          _currentBait = Bait.Unknown;
    private          ExtendedFish? _lastFish;
    private          bool          _snagging;
    private          bool          _chum;
    private          bool          _intuition;
    private          bool          _fishEyes;
    private          bool          _catchHandled = true;

    private static Bait GetCurrentBait(uint id)
    {
        if (GatherBuddy.GameData.Bait.TryGetValue(id, out var bait))
            return bait;

        PluginLog.Error("Item with id {Id} is not a known type of bait.", id);
        return Bait.Unknown;
    }

    private void CheckBuffs()
    {
        _snagging  = false;
        _chum      = false;
        _intuition = false;

        if (Dalamud.ClientState.LocalPlayer?.StatusList == null)
            return;

        foreach (var buff in Dalamud.ClientState.LocalPlayer.StatusList)
        {
            switch (buff.StatusId)
            {
                case SnaggingEffectId:
                    _snagging = true;
                    break;
                case ChumEffectId:
                    _chum = true;
                    break;
                case IntuitionEffectId:
                    _intuition = true;
                    break;
                case FishEyesEffectId:
                    _fishEyes = true;
                    break;
            }
        }
    }

    private void OnBeganFishing(FishingSpot? spot)
    {
        _currentSpot = spot;
        _currentBait = GetCurrentBait(_bait.Current);
        CheckBuffs();
        _currentFishList = SortedFish();
        PluginLog.Verbose("Began fishing at {FishingSpot} using {Bait} {Snagging} and {Chum}.",
            _currentSpot?.Name ?? "Undiscovered Fishing Hole", _currentBait.Name
            , _snagging ? "with Snagging" : "without Snagging"
            , _chum ? "with Chum" : "without Chum");
        _start.Restart();
        _catchHandled = false;
    }

    private void OnBite()
    {
        _start.Stop();
        PluginLog.Verbose("Fish bit at {FishingSpot} after {Milliseconds} using {Bait} {Snagging} and {Chum}.",
            _currentSpot?.Name ?? "Undiscovered Fishing Hole", _start.ElapsedMilliseconds
            , _currentBait.Name, _snagging ? "with Snagging" : "without Snagging", _chum ? "with Chum" : "without Chum");
    }

    private void OnIdentification(FishingSpot spot)
    {
        _currentSpot = spot;
        PluginLog.Verbose("Identified previously unknown fishing spot as {FishingSpot}.", _currentSpot.Name);
    }

    private void OnCatch(Fish fish)
    {
        _lastFish = _fish.Fish[fish.ItemId];

    if (_lastFish.Records.Update(_currentBait, (ushort)_start.ElapsedMilliseconds, _snagging, _chum))
        {
            _fish.SaveRecords();
            _currentFishList = SortedFish();
        }

        PluginLog.Verbose("Caught {Fish} at {FishingSpot} after {Milliseconds} using {Bait} {Snagging} and {Chum}.",
            _lastFish.Name, _currentSpot?.Name ?? "Unknown", _start.ElapsedMilliseconds,
            _currentBait.Name, _snagging ? "with Snagging" : "without Snagging",
            _chum ? "with Chum" : "without Chum");
        _catchHandled = true;
    }

    private void OnMooch()
    {
        _currentBait = new Bait(_lastFish!.Fish.ItemData);
        CheckBuffs();
        _currentFishList = SortedFish();
        PluginLog.Verbose("Mooching with {Fish} at {FishingSpot} {Snagging} and {Chum}.", _lastFish!.Name,
            _currentSpot!.Name, _snagging ? "with Snagging" : "without Snagging", _chum ? "with Chum" : "without Chum");
        _start.Restart();
        _catchHandled = false;
    }
}
