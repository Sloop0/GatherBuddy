using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using Dalamud;

namespace GatherBuddy.FishTimer.Parser;

public partial class FishingParser
{
    private readonly struct Regexes
    {
        public Regex  Cast           { get; private init; }
        public string Undiscovered   { get; private init; }
        public Regex  AreaDiscovered { get; private init; }
        public string Bite           { get; private init; }
        public Regex  Mooch          { get; private init; }
        public Regex  Catch          { get; private init; }
        public Regex  NoCatchFull    { get; private init; }

        public static Regexes FromLanguage(ClientLanguage lang)
        {
            return lang switch
            {
                ClientLanguage.English  => English.Value,
                ClientLanguage.German   => German.Value,
                ClientLanguage.French   => French.Value,
                ClientLanguage.Japanese => Japanese.Value,
                _                       => throw new InvalidEnumArgumentException(),
            };
        }

        // @formatter:off
        private static readonly Lazy<Regexes> English = new( () => new Regexes
        {
            Cast           = new Regex(@"(?:You cast your|.*? casts (?:her|his)) line (?:on|in|at) (?<FishingSpot>.+)\.", RegexOptions.Compiled),
            AreaDiscovered = new Regex(@".*?(on|at) (?<FishingSpot>.+) is added to your fishing log\.",                   RegexOptions.Compiled),
            Mooch          = new Regex(@"line with the fish still hooked.",                                               RegexOptions.Compiled),
            Catch          = new Regex(@".*land.*measuring",                                                              RegexOptions.Compiled),
            NoCatchFull    = new Regex(@"You cannot carry any more",                                                      RegexOptions.Compiled),
            Undiscovered   = "undiscovered fishing hole",
            Bite           = "Something bites!",
        });

        private static readonly Lazy<Regexes> German = new(() => new Regexes
        {
            Cast           = new Regex(@"Du hast mit dem Fischen (?<FishingSpotWithArticle>.+) begonnen\.(?<FishingSpot>invalid)?",                RegexOptions.Compiled),
            AreaDiscovered = new Regex(@"Die neue Angelstelle (?<FishingSpot>.*) wurde in deinem Fischer-Notizbuch vermerkt\.",                    RegexOptions.Compiled),
            Mooch          = new Regex(@"Du hast die Leine mit",                                                                                   RegexOptions.Compiled),
            Catch          = new Regex(@"Du (?:hast eine?n? | ziehst \d+ ).+?(?:\(\d|mit ein)",                                                    RegexOptions.Compiled),
            NoCatchFull    = new Regex(@"Du hast .+ geangelt, musst deinen Fang aber wieder freilassen, weil du nicht mehr davon besitzen kannst", RegexOptions.Compiled),
            Undiscovered = "unerforschten Angelplatz",
            Bite         = "Etwas hat angebissen!",
        });

        private static readonly Lazy<Regexes> French = new(() => new Regexes
        {
            Cast           = new Regex(@"Vous commencez à pêcher\.\s*Point de pêche: (?<FishingSpot>.+)\.",                RegexOptions.Compiled),
            AreaDiscovered = new Regex(@"Vous notez le banc de poissons “(?<FishingSpot>.+)” dans votre carnet\.",         RegexOptions.Compiled),
            Mooch          = new Regex(@"Vous essayez de pêcher au vif avec",                                              RegexOptions.Compiled),
            Catch          = new Regex(@"Vous avez pêché (?:un |une )?.+?de \d",                                           RegexOptions.Compiled),
            NoCatchFull    = new Regex(@"Vous avez pêché .+, mais ne pouvez en posséder davantage et l'avez donc relâché", RegexOptions.Compiled),
            Undiscovered   = "Zone de pêche inconnue",
            Bite           = "Vous avez une touche!",
        });

        private static readonly Lazy<Regexes> Japanese = new(() => new Regexes
        {
            Cast           = new Regex(@".+\u306f(?<FishingSpot>.+)で釣りを開始した。",                 RegexOptions.Compiled),
            AreaDiscovered = new Regex(@"釣り手帳に新しい釣り場「(?<FishingSpot>.+)」の情報を記録した！",   RegexOptions.Compiled),
            Mooch          = new Regex(@"は釣り上げた.+を慎重に投げ込み、泳がせ釣りを試みた。",              RegexOptions.Compiled),
            Catch          = new Regex(@".+?（\d+\.\dイルム）を釣り上げた。",                            RegexOptions.Compiled),
            NoCatchFull    = new Regex(@".+を釣り上げたが、これ以上持てないためリリースした。",              RegexOptions.Compiled),
            Undiscovered   = "未知の釣り場",
            Bite           = "魚をフッキングした！",
        });
        // @formatter:on
    }
}
