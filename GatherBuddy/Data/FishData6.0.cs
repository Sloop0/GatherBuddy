using GatherBuddy.Enums;
using GatherBuddy.Managers;
using GatherBuddyA.Enums;
using BiteType = GatherBuddy.Enums.BiteType;
using HookSet = GatherBuddy.Enums.HookSet;
using Patch = GatherBuddy.Enums.Patch;
using Snagging = GatherBuddy.Enums.Snagging;

namespace GatherBuddy.Data
{
    public static partial class FishData
    {
        // @formatter:off
        private static void ApplyEndwalker(this FishManager fish)
        {
            fish.Apply     (35604, Patch.Endwalker) // Giant Aetherlouse
                .Bait      (fish, 36588)
                .Tug       (BiteType.Weak)
                .HookType  (HookSet.Precise);
            fish.Apply     (35605, Patch.Endwalker) // Garjana Wrasse
                .Bait      (fish, 36592)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (35606, Patch.Endwalker) // Garlean Clam
                .Bait      (fish, 36589)
                .Tug       (BiteType.Weak)
                .HookType  (HookSet.Precise);
            fish.Apply     (35607, Patch.Endwalker) // Smaragdos
                .Bait      (fish, 36590)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36385, Patch.Endwalker) // Pecten
                .Tug       (BiteType.Weak)
                .Snag      (Snagging.Required)
                .HookType  (HookSet.Precise);
            fish.Apply     (36386, Patch.Endwalker) // Northern Herring
                .Bait      (fish, 36592)
                .Tug       (BiteType.Weak)
                .HookType  (HookSet.Precise);
            fish.Apply     (36387, Patch.Endwalker) // Dog-faced Puffer
                .Bait      (fish, 36592)
                .Tug       (BiteType.Weak)
                .HookType  (HookSet.Precise);
            fish.Apply     (36388, Patch.Endwalker) // Cobalt Chromis
                .Bait      (fish, 36592)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36389, Patch.Endwalker) // Guitarfish
                .Bait      (fish, 36592)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36390, Patch.Endwalker) // Astacus
                .Bait      (fish, 36588)
                .Tug       (BiteType.Weak)
                .HookType  (HookSet.Precise);
            fish.Apply     (36392, Patch.Endwalker) // Peacock Bass
                .Bait      (fish, 36588)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36393, Patch.Endwalker) // Academician
                .Bait      (fish, 36588)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36394, Patch.Endwalker) // Swordspine Snook
                .Bait      (fish, 36588)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36395, Patch.Endwalker) // Ponderer
                .Bait      (fish, 36588)
                .Tug       (BiteType.Weak)
                .HookType  (HookSet.Precise);
            fish.Apply     (36396, Patch.Endwalker) // Tidal Dahlia
                .Bait      (fish, 36588)
                .Tug       (BiteType.Weak)
                .HookType  (HookSet.Precise);
            fish.Apply     (36397, Patch.Endwalker) // Butterfly Fry
                .Bait      (fish, 36588)
                .Tug       (BiteType.Weak)
                .HookType  (HookSet.Precise);
            fish.Apply     (36398, Patch.Endwalker) // Xenocypris
                .Bait      (fish, 36588)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36399, Patch.Endwalker) // Topminnow
                .Bait      (fish, 36588)
                .Tug       (BiteType.Weak)
                .HookType  (HookSet.Precise);
            fish.Apply     (36400, Patch.Endwalker) // Tessera
                .Bait      (fish, 36588)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36402, Patch.Endwalker) // Fat Snook
                .Bait      (fish, 36588)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36403, Patch.Endwalker) // Prochilodus Luminosus
                .Bait      (fish, 36588)
                .Tug       (BiteType.Weak)
                .HookType  (HookSet.Precise);
            fish.Apply     (36404, Patch.Endwalker) // Mesonauta
                .Bait      (fish, 36588)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36405, Patch.Endwalker) // Greengill Salmon
                .Bait      (fish, 36588)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36407, Patch.Endwalker) // Raiamas
                .Bait      (fish, 36588)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36408, Patch.Endwalker) // Red Bowfin
                .Bait      (fish, 36590)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36409, Patch.Endwalker) // Macrobrachium Lar
                .Bait      (fish, 36588)
                .Tug       (BiteType.Weak)
                .HookType  (HookSet.Precise);
            fish.Apply     (36410, Patch.Endwalker) // Blowgun
                .Bait      (fish, 36588)
                .Tug       (BiteType.Weak)
                .HookType  (HookSet.Precise);
            fish.Apply     (36411, Patch.Endwalker) // Darksteel Knifefish
                .Bait      (fish, 36588)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36412, Patch.Endwalker) // Astacus Aetherius
                .Bait      (fish, 36589)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36414, Patch.Endwalker) // Labyrinthos Tilapia
                .Bait      (fish, 36589, 36412)
                .Tug       (BiteType.Strong)
                .Uptime    (480, 960)
                .Weather   (2)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36415, Patch.Endwalker) // Trunkblessed
                .Bait      (fish, 36588)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36417, Patch.Endwalker) // Seema Duta
                .Bait      (fish, 36588)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36418, Patch.Endwalker) // Longear Sunfish
                .Bait      (fish, 36588)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36419, Patch.Endwalker) // Silver Characin
                .Bait      (fish, 36588)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36420, Patch.Endwalker) // Thavnairian Goby
                .Bait      (fish, 36592)
                .Tug       (BiteType.Weak)
                .HookType  (HookSet.Precise);
            fish.Apply     (36421, Patch.Endwalker) // Qeyiq Sole
                .Bait      (fish, 36592)
                .Tug       (BiteType.Weak)
                .HookType  (HookSet.Precise);
            fish.Apply     (36422, Patch.Endwalker) // Gwl Crab
                .Bait      (fish, 36592)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36423, Patch.Endwalker) // Pantherscale Grouper
                .Bait      (fish, 36592)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36425, Patch.Endwalker) // Fate's Design
                .Bait      (fish, 36592)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36426, Patch.Endwalker) // Shadowdart Sardine
                .Bait      (fish, 36592)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36427, Patch.Endwalker) // Paksa Fish
                .Bait      (fish, 36592)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36430, Patch.Endwalker) // Golden Barramundi
                .Bait      (fish, 36592)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36431, Patch.Endwalker) // Kadjaya's Castaway
                .Bait      (fish, 36588)
                .Tug       (BiteType.Weak)
                .HookType  (HookSet.Precise);
            fish.Apply     (36432, Patch.Endwalker) // Marid Frog
                .Bait      (fish, 36588)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36434, Patch.Endwalker) // Bluegill
                .Bait      (fish, 36589)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36435, Patch.Endwalker) // Bronze Pipira
                .Bait      (fish, 36588)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36436, Patch.Endwalker) // Green Swordtail
                .Bait      (fish, 36588)
                .Tug       (BiteType.Weak)
                .HookType  (HookSet.Precise);
            fish.Apply     (36438, Patch.Endwalker) // Ksirapayin
                .Bait      (fish, 36589)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36439, Patch.Endwalker) // Wakeful Watcher
                .Bait      (fish, 36589)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36440, Patch.Endwalker) // Red Drum
                .Bait      (fish, 36589)
                .Tug       (BiteType.Strong)
                .Uptime    (960, 1440)
                .Transition(8, 3)
                .Weather   (2)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36441, Patch.Endwalker) // Forgeflame
                .Bait      (fish, 36589)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36442, Patch.Endwalker) // Bicuda
                .Bait      (fish, 36589)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36443, Patch.Endwalker) // Radzbalik
                .Bait      (fish, 36589)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36444, Patch.Endwalker) // Half-moon Betta
                .Bait      (fish, 36589)
                .Tug       (BiteType.Weak)
                .HookType  (HookSet.Precise);
            fish.Apply     (36446, Patch.Endwalker) // Banana Eel
                .Bait      (fish, 36589)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36447, Patch.Endwalker) // Handy Hamsa
                .Bait      (fish, 36589)
                .Tug       (BiteType.Weak)
                .HookType  (HookSet.Precise);
            fish.Apply     (36448, Patch.Endwalker) // Flowerhorn
                .Bait      (fish, 36589)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36449, Patch.Endwalker) // Thavnairian Caiman
                .Bait      (fish, 36589)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36450, Patch.Endwalker) // Fiery Goby
                .Bait      (fish, 36592)
                .Tug       (BiteType.Weak)
                .HookType  (HookSet.Precise);
            fish.Apply     (36451, Patch.Endwalker) // Puff-paya
                .Bait      (fish, 36593)
                .Tug       (BiteType.Weak)
                .HookType  (HookSet.Precise);
            fish.Apply     (36452, Patch.Endwalker) // Narunnairian Octopus
                .Bait      (fish, 36592)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36453, Patch.Endwalker) // Roosterfish
                .Bait      (fish, 36593)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36454, Patch.Endwalker) // Basilosaurus
                .Bait      (fish, 36593, 36451)
                .Tug       (BiteType.Legendary)
                .Weather   (2)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36456, Patch.Endwalker) // Eblan Trout
                .Bait      (fish, 36588)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36457, Patch.Endwalker) // Animulus
                .Bait      (fish, 36588)
                .Tug       (BiteType.Weak)
                .HookType  (HookSet.Precise);
            fish.Apply     (36458, Patch.Endwalker) // Cerule Core
                .Bait      (fish, 36588)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36459, Patch.Endwalker) // Icepike
                .Bait      (fish, 36588)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36460, Patch.Endwalker) // Dark Crown
                .Bait      (fish, 36588)
                .Tug       (BiteType.Weak)
                .HookType  (HookSet.Precise);
            fish.Apply     (36461, Patch.Endwalker) // Imperial Pleco
                .Bait      (fish, 36588)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36462, Patch.Endwalker) // Bluetail
                .Bait      (fish, 36588)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36463, Patch.Endwalker) // Star-blue Guppy
                .Bait      (fish, 36588)
                .Tug       (BiteType.Weak)
                .HookType  (HookSet.Precise);
            fish.Apply     (36465, Patch.Endwalker) // Lunar Cichlid
                .Bait      (fish, 36588)
                .Tug       (BiteType.Weak)
                .HookType  (HookSet.Precise);
            fish.Apply     (36466, Patch.Endwalker) // Teareye
                .Bait      (fish, 36588)
                .Tug       (BiteType.Weak)
                .HookType  (HookSet.Precise);
            fish.Apply     (36467, Patch.Endwalker) // Replipirarucu
                .Bait      (fish, 36588)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36468, Patch.Endwalker) // Feverfish
                .Bait      (fish, 36594)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36470, Patch.Endwalker) // Calicia
                .Bait      (fish, 36594)
                .Tug       (BiteType.Weak)
                .HookType  (HookSet.Precise);
            fish.Apply     (36471, Patch.Endwalker) // Protomyke #987
                .Bait      (fish, 36594)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36472, Patch.Endwalker) // Lunar Deathworm
                .Bait      (fish, 36594, 36470)
                .Tug       (BiteType.Legendary)
                .Weather   (2)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36473, Patch.Endwalker) // Fleeting Brand
                .Bait      (fish, 36594)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36475, Patch.Endwalker) // Regotoise
                .Bait      (fish, 36594)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36476, Patch.Endwalker) // Isle Skipper
                .Bait      (fish, 36588)
                .Tug       (BiteType.Weak)
                .HookType  (HookSet.Precise);
            fish.Apply     (36477, Patch.Endwalker) // Iribainion
                .Bait      (fish, 36588)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36478, Patch.Endwalker) // Albino Loach
                .Bait      (fish, 36588)
                .Tug       (BiteType.Weak)
                .HookType  (HookSet.Precise);
            fish.Apply     (36479, Patch.Endwalker) // Golden Shiner
                .Bait      (fish, 36588)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36480, Patch.Endwalker) // Mangar
                .Bait      (fish, 36588, 36478)
                .Tug       (BiteType.Strong)
                .Weather   (3)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36481, Patch.Endwalker) // Dermogenys
                .Bait      (fish, 36588)
                .Tug       (BiteType.Weak)
                .HookType  (HookSet.Precise);
            fish.Apply     (36484, Patch.Endwalker) // Antheia
                .Bait      (fish, 36588)
                .Tug       (BiteType.Weak)
                .HookType  (HookSet.Precise);
            fish.Apply     (36485, Patch.Endwalker) // Colossoma
                .Bait      (fish, 36588)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36487, Patch.Endwalker) // Superstring
                .Bait      (fish, 36594)
                .Tug       (BiteType.Weak)
                .HookType  (HookSet.Precise);
            fish.Apply     (36488, Patch.Endwalker) // Star Eater
                .Bait      (fish, 36594)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36489, Patch.Endwalker) // Vacuum Shrimp
                .Bait      (fish, 36594)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36491, Patch.Endwalker) // Cosmic Noise
                .Bait      (fish, 36594)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36492, Patch.Endwalker) // Glassfish
                .Bait      (fish, 36594)
                .Tug       (BiteType.Weak)
                .HookType  (HookSet.Precise);
            fish.Apply     (36494, Patch.Endwalker) // Foun Myhk
                .Bait      (fish, 36594)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36495, Patch.Endwalker) // Dragonscale
                .Bait      (fish, 36594)
                .Tug       (BiteType.Weak)
                .HookType  (HookSet.Precise);
            fish.Apply     (36496, Patch.Endwalker) // Ypupîara
                .Bait      (fish, 36594)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36497, Patch.Endwalker) // Eehs Forhnesh
                .Bait      (fish, 36594)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36499, Patch.Endwalker) // Katoptron
                .Bait      (fish, 36588)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36501, Patch.Endwalker) // Comet Tail
                .Bait      (fish, 36589)
                .Tug       (BiteType.Weak)
                .HookType  (HookSet.Precise);
            fish.Apply     (36502, Patch.Endwalker) // Aoide
                .Bait      (fish, 36589)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36503, Patch.Endwalker) // Protoflesh
                .Bait      (fish, 36588)
                .Tug       (BiteType.Weak)
                .HookType  (HookSet.Precise);
            fish.Apply     (36505, Patch.Endwalker) // Wandering Starscale
                .Bait      (fish, 36589)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36506, Patch.Endwalker) // Wormhole Worm
                .Bait      (fish, 36596)
                .Tug       (BiteType.Weak)
                .HookType  (HookSet.Precise);
            fish.Apply     (36507, Patch.Endwalker) // Unidentified Flying Biomass II
                .Bait      (fish, 36596)
                .Tug       (BiteType.Weak)
                .HookType  (HookSet.Precise);
            fish.Apply     (36508, Patch.Endwalker) // Triaina
                .Bait      (fish, 36596)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36509, Patch.Endwalker) // Sophos Deka-okto
                .Bait      (fish, 36596)
                .Tug       (BiteType.Weak)
                .HookType  (HookSet.Precise);
            fish.Apply     (36510, Patch.Endwalker) // Class Twenty-four
                .Bait      (fish, 36596)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36511, Patch.Endwalker) // Terrifyingway
                .Bait      (fish, 36596)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36512, Patch.Endwalker) // Alien Mertone
                .Bait      (fish, 36596)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36513, Patch.Endwalker) // Monster Carrot
                .Bait      (fish, 36597)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36514, Patch.Endwalker) // Argonaut
                .Bait      (fish, 36596)
                .Tug       (BiteType.Weak)
                .HookType  (HookSet.Precise);
            fish.Apply     (36515, Patch.Endwalker) // Echinos
                .Bait      (fish, 36596)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36516, Patch.Endwalker) // Space Bishop
                .Bait      (fish, 36596)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36517, Patch.Endwalker) // Alyketos
                .Bait      (fish, 36597)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36518, Patch.Endwalker) // Horizon Event
                .Bait      (fish, 36596)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36519, Patch.Endwalker) // E.B.E.-9318
                .Bait      (fish, 36596)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36520, Patch.Endwalker) // Unbegotten
                .Bait      (fish, 36596, 36518)
                .Tug       (BiteType.Strong)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36521, Patch.Endwalker) // Phallaina 
                .Bait      (fish, 36596, 36518)
                .Tug       (BiteType.Legendary)
                .Uptime    (1320, 240)
                .HookType  (HookSet.Powerful);
            fish.Apply     (36522, Patch.Endwalker) // Thavnairian Cucumber
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36523, Patch.Endwalker) // Spiny King Crab
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36524, Patch.Endwalker) // Thavnairian Eel
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36525, Patch.Endwalker) // Gilled Topknot
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36526, Patch.Endwalker) // Purusa Fish
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36527, Patch.Endwalker) // Giantsgall Jaw
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36528, Patch.Endwalker) // Akyaali Sardine
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36529, Patch.Endwalker) // Spicy Pickle
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36530, Patch.Endwalker) // Mayavahana
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36531, Patch.Endwalker) // Hedonfish
                .Gig       (GigHead.Normal)
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36532, Patch.Endwalker) // Satrap Trapfish
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36533, Patch.Endwalker) // Blue Marlin
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36534, Patch.Endwalker) // Satrap's Whisper
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36535, Patch.Endwalker) // Tebqeyiq Smelt
                .Gig       (GigHead.Small)
                .Snag      (Snagging.None)
                .Predators (fish, (36531, 10), (36547, 3), (36546, 2))
                .HookType  (HookSet.None);
            fish.Apply     (36536, Patch.Endwalker) // Shallows Cod
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36537, Patch.Endwalker) // Meyhane Reveler
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36538, Patch.Endwalker) // Daemir's Alloy
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36539, Patch.Endwalker) // Rasa Fish
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36540, Patch.Endwalker) // Agama's Palm
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36541, Patch.Endwalker) // Rummy-nosed Tetra
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36542, Patch.Endwalker) // Monksblade
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36543, Patch.Endwalker) // Atamra Cichlid
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36544, Patch.Endwalker) // Root of Maya
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36545, Patch.Endwalker) // Floral Snakehead
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36546, Patch.Endwalker) // Xiphactinus
                .Gig       (GigHead.Large)
                .Snag      (Snagging.None)
                .Predators (fish, (36531, 10))
                .HookType  (HookSet.None);
            fish.Apply     (36547, Patch.Endwalker) // Dusky Shark
                .Gig       (GigHead.Large)
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36548, Patch.Endwalker) // Coffer Shell
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36549, Patch.Endwalker) // Onihige
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36550, Patch.Endwalker) // Onokoro Carp
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36551, Patch.Endwalker) // Ruby-spotted Crab
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36552, Patch.Endwalker) // Marrow-eater
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36553, Patch.Endwalker) // Cloudy Catshark
                .Gig       (GigHead.Large)
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36554, Patch.Endwalker) // Red Gurnard
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36555, Patch.Endwalker) // Dream Pickle
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36556, Patch.Endwalker) // Ruby Haddock
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36557, Patch.Endwalker) // Crown Fish
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36558, Patch.Endwalker) // Sword of Isari 
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36559, Patch.Endwalker) // Blue Shark
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36560, Patch.Endwalker) // Barb of Exile
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36561, Patch.Endwalker) // Smooth Lumpfish
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36562, Patch.Endwalker) // Hells' Cap
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36563, Patch.Endwalker) // Keeled Fugu
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36564, Patch.Endwalker) // Eastern Seerfish
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36565, Patch.Endwalker) // False Fusilier
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36566, Patch.Endwalker) // Skipping Stone
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36567, Patch.Endwalker) // Red-spotted Blenny
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36568, Patch.Endwalker) // Othardian Wrasse
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36569, Patch.Endwalker) // Grey Mullet
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36570, Patch.Endwalker) // Prayer Cushion
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36571, Patch.Endwalker) // Deepbody Boarfish
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36572, Patch.Endwalker) // Jointed Razorfish
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36573, Patch.Endwalker) // Pipefish
                .Gig       (GigHead.Small)
                .Snag      (Snagging.None)
                .Predators (fish, (36553, 10))
                .HookType  (HookSet.None);
            fish.Apply     (36574, Patch.Endwalker) // Righteye Flounder
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36575, Patch.Endwalker) // Mini Yasha
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36576, Patch.Endwalker) // Sawshark
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36577, Patch.Endwalker) // Othardian Lumpsucker
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36578, Patch.Endwalker) // Shogun's Kabuto
                .Gig       (GigHead.Large)
                .Snag      (Snagging.None)
                .Predators (fish, (36553, 10))
                .HookType  (HookSet.None);
            fish.Apply     (36579, Patch.Endwalker) // Bluefin Trevally
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36580, Patch.Endwalker) // Kitefin Shark
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36581, Patch.Endwalker) // Uzumaki
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36582, Patch.Endwalker) // Natron Puffer
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36583, Patch.Endwalker) // Diamond Dagger
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36584, Patch.Endwalker) // Queenly Fan
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36585, Patch.Endwalker) // Pale Panther
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36586, Patch.Endwalker) // Saltsquid
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
            fish.Apply     (36587, Patch.Endwalker) // Platinum Hammerhead
                .Snag      (Snagging.None)
                .HookType  (HookSet.None);
        }
        // @formatter:on
    }
}
