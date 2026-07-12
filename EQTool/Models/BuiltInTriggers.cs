using System;
using System.Collections.Generic;

namespace EQTool.Models
{
    public static class BuiltInTriggers
    {
        public const string CategoryName = "Built In";

        public static List<Trigger> All()
        {
            var list = new List<Trigger>
            {
                CreateEnraged(),
                CreateLevitateFading(),
                CreateInvisFading(),
                CreateFailedFeign(),
                CreateGroupInvite(),
                CreateNpcGating(),
                CreateCharmBreak(),
                CreateDeathTouch(),
                CreateTellsYou(),
                CreateExpGained(),
                CreateResist(),
                CreateAvatarOfWarLockout(),
                CreateVPHoskarResto(),
                CreateDragonRoar(),
                CreateSpellWornOff(),
                CreateEnteredZone(),

                // Spell/combat feedback triggers ported from the old hardcoded trigger list.
                // These only show overlay text (no audio) but keep their original audio text so
                // the user can switch audio on if they want it.
                BuildLegacy("builtin:spell-interrupted", "Spell Interrupted", "^Your spell is interrupted.", "Spell Interrupted", "Interrupted", "Combat"),
                BuildLegacy("builtin:spell-fizzle", "Spell Fizzle", "^Your spell fizzles!", "Spell Fizzles", "Fizzle", "Combat"),
                BuildLegacy("builtin:backstabber", "Backstabber", "^{backstabber} backstabs {target} for {damage} points of damage.", "{backstabber} backstabs {target} for {damage}", "Backstabber", "Combat"),
                BuildLegacy("builtin:corpse-need-consent", "Corpse Need Consent", "^You do not have consent to summon that corpse", "Need Consent", "Need Consent", "Utility"),
                BuildLegacy("builtin:corpse-out-of-range", "Corpse Out of Range", "^The corpse is too far away to summon", "Corpse OOR", "Corpse out of range", "Utility"),
                BuildLegacy("builtin:select-a-target", "Select a Target", "^(You must first select a target for this spell)|(You must first click on the being you wish to attack)", "Select a target", "Select a target", "Utility"),
                BuildLegacy("builtin:insufficient-mana", "Insufficient Mana", "^Insufficient Mana to cast this spell!", "OOM", "out of mana", "Combat"),
                BuildLegacy("builtin:target-out-of-range", "Target Out of Range", "^Your target is out of range", "Target out of range", "Out of range", "Combat"),
                BuildLegacy("builtin:spell-did-not-take-hold", "Spell Did Not Take Hold", "^Your spell did not take hold", "Spell did not take hold", "Spell did not take hold", "Combat"),
                BuildLegacy("builtin:must-be-standing", "Must be standing to cast", "^(You must be standing)|(You are too distracted to cast a spell now)", "Stand up!", "stand up", "Combat"),
                BuildLegacy("builtin:dispelled", "Dispelled", "^You feel a bit dispelled", "You have been dispelled", "dispelled", "Combat"),
                BuildLegacy("builtin:regen-faded", "Regen Faded", "^You have stopped regenerating", "===== Regen faded =====", "re-gen faded", "Utility"),
                BuildLegacy("builtin:cant-see-target", "Can't See Target", "^You can't see your target", "Can't see target", "Can't see target", "Combat"),
                BuildLegacy("builtin:sense-heading", "Sense Heading", "^You think you are heading {direction}", "Direction = {direction}", "{direction}", "Utility"),
                BuildLegacy("builtin:sense-heading-failed", "Sense Heading Failed", "^You have no idea what direction you are facing", "No idea", "no idea", "Utility")
            };
            list.AddRange(EncounterAoeTriggers());
            return list;
        }

        // create trigger exp grinding
        //  - triggers from "You gain experience" or "You gain party experience"
        //  - uses {counter} as part of output, to help user keep track of spawns
        //  - allow user to send a tell to ".exp" to immediately clear all timers of this type
        public static Trigger CreateExpGained()
        {
            return new Trigger
            {
                IsBuiltIn = true,
                BuiltInId = "builtin:exp-gained-0640",
                BuiltInFolder = "Utility",
                TriggerEnabled = false,
                TriggerId = Guid.NewGuid(),
                TriggerName = "Exp Timer",
                // real regex (not the simplified {name} form) so the target stays a single word
                SearchText = @"^You gain (party )?experience",
                UseRegex = true,
                Category = CategoryName,
                Comments = "Note: Sending a tell to \".exp\", i.e. \"/t .exp\" will immediately terminate all timers of this type!\n" +
                            "BB fishers = 6:40 and 22:00, " +
                            "Chardok = 18:00, " +
                            "COM = 22:00, " +
                            "Crystal Caverns = 14:45, " +
                            "Droga = 20:30, " +
                            "Grobb = 24:00, " +
                            "Hole = 21:30, " +
                            "HS = 20:30, " +
                            "Kael = 28:00, " +
                            "Kedge = 22:00, " +
                            "Lower Guk = 28:00, " +
                            "MM = 23 min, " +
                            "North Felwithe guards = 24:00, " +
                            "Oasis specs = 16:30, " +
                            "OOT specs/sisters = 6:00, " +
                            "Paw = 22:00, " +
                            "Perma = 22:00, " +
                            "Seb Lair = 27:00, " +
                            "Skyfire = 13:00, " +
                            "Skyshrine = 30:00, " +
                            "TD = 12:00, " +
                            "TT = 6:40, " +
                            "Wars Woods brutes = 6:40, " +
                            "WK guards = 6:00, " +
                            "WL = 14:30, ",

                Basic = new TriggerOutput
                {
                    DisplayTextEnabled = false,
                    AudioType = TriggerAudioType.None,
                    TtsText = ""
                },
                Timer = new TriggerTimer
                {
                    TimerType = TimerType.CountDown,
                    TimerName = "-- Exp Timer [{counter}] (.exp)",
                    Minutes = 6,
                    Seconds = 40,
                    RestartBehavior = TimerRestartBehavior.StartNewTimer,
                    EndEarlyTexts = new System.Collections.ObjectModel.ObservableCollection<EndEarlyEntry>
                    {
                        new EndEarlyEntry
                        {
                            SearchText = @"^\.exp",
                            UseRegex = true,
                        },
                    }
                },
                TimerEnding = new TriggerTimerEnding
                {
                    Enabled = true,
                    Seconds = 30,
                    Output = new TriggerOutput
                    {
                        DisplayTextEnabled = true,
                        DisplayText = "30 second warning",
                        AudioType = TriggerAudioType.TextToSpeech,
                        TtsText = "30 second warning"
                    }
                },
                TimerEnded = new TriggerTimerEnded
                {
                    Enabled = true,
                    Output = new TriggerOutput
                    {
                        DisplayTextEnabled = true,
                        DisplayText = "Pop [{counter}]",
                        AudioType = TriggerAudioType.TextToSpeech,
                        TtsText = "Pop"
                    }
                },
                Counter = new TriggerCounter
                {
                    ResetEnabled = true,
                    Minutes = 30,
                }
            };
        }

        // "Fright says, 'Targetname'" / "Dread says, 'Targetname'" -> 45 second death touch timer.
        // Fright and Dread announce their target by name (a single word) just before death touching it.
        // Ported from the old DeathTouchHandler: shows a 45 second countdown named after the target.
        public static Trigger CreateDeathTouch()
        {
            return new Trigger
            {
                IsBuiltIn = true,
                BuiltInId = "builtin:death-touch",
                BuiltInFolder = "Encounters",
                TriggerEnabled = false,
                TriggerId = Guid.NewGuid(),
                TriggerName = "Death Touch (Fright/Dread)",
                // real regex (not the simplified {name} form) so the target stays a single word
                SearchText = @"^(?<npc>Fright|Dread) says,? '(?<target>[^' ]+)'",
                UseRegex = true,
                Category = CategoryName,
                Zone = "fear",
                Basic = new TriggerOutput
                {
                    DisplayTextEnabled = true,
                    DisplayText = "Death Touch: {target}",
                    DisplayTextColor = "White",
                    AudioType = TriggerAudioType.TextToSpeech,
                    TtsText = "Death touch on {target}"
                },
                Timer = new TriggerTimer
                {
                    TimerType = TimerType.CountDown,
                    TimerName = "--DT-- '{target}'",
                    Seconds = 45,
                    RestartBehavior = TimerRestartBehavior.RestartTimer,
                    BarColor = "Red"
                }
            };
        }

        public static Trigger CreateResist()
        {
            return new Trigger
            {
                IsBuiltIn = true,
                BuiltInId = "builtin:resist",
                BuiltInFolder = "Combat",
                TriggerEnabled = false,
                TriggerId = Guid.NewGuid(),
                TriggerName = "Resist",
                SearchText = @"^(?:You resist the|Your target resisted the) (?<spell>.+) spell[!.]",
                UseRegex = true,
                Category = CategoryName,
                Basic = new TriggerOutput
                {
                    DisplayTextEnabled = true,
                    DisplayText = "Resisted: {spell}",
                    DisplayTextColor = "Red",
                    AudioType = TriggerAudioType.TextToSpeech,
                    TtsText = "Resisted"
                }
            };
        }

        public const string AvatarOfWarBuiltInId = "builtin:aow-lockout";
        public static Trigger CreateAvatarOfWarLockout()
        {
            return new Trigger
            {
                IsBuiltIn = true,
                BuiltInId = AvatarOfWarBuiltInId,
                BuiltInFolder = "Encounters/Kael",
                TriggerEnabled = false,
                TriggerId = Guid.NewGuid(),
                TriggerName = "Avatar of War Lockout",
                SearchText = "The Avatar of War shouts 'Who dares defile my temple?! Come forth and face me!'",
                UseRegex = false,
                Category = CategoryName,
                Zone = "kael",
                Timer = new TriggerTimer
                {
                    TimerType = TimerType.CountDown,
                    TimerName = "The Avatar of War Lockout",
                    Minutes = 20,
                    RestartBehavior = TimerRestartBehavior.RestartTimer,
                    BarColor = "Magenta",
                    IconName = "Spirit of Wolf"
                }
            };
        }

        // Hoskar (VP) casts Diseased Cloud, after which you have ~8 seconds to land Word of
        // Restoration. Fires on the Diseased Cloud cast (on you or others) or a resist of it,
        // but only while in Veeshan's Peak. Shows an 8s "Word Of Resto" countdown, then alerts
        // "Resto Now" when it ends. Ported from the old DiseasedCloudHandler. Lives under the
        // Built In > Encounters > VP folder, and is auto-enabled for users on startup
        // (see EQToolSettingsLoad.EnableVPHoskarRestoTriggerIfMissing).
        public const string VPHoskarRestoBuiltInId = "builtin:vp-hoskar-resto";
        public static Trigger CreateVPHoskarResto()
        {
            return new Trigger
            {
                IsBuiltIn = true,
                BuiltInId = VPHoskarRestoBuiltInId,
                BuiltInFolder = "Encounters/VP",
                TriggerEnabled = false,
                TriggerId = Guid.NewGuid(),
                TriggerName = "Hoskar Resto",
                // Diseased Cloud cast on you ("Your body begins to rot.") or others
                // ("<name>'s body begins to rot."), or a resist of it.
                SearchText = @"(body begins to rot\.|You resist the Diseased Cloud spell!)",
                UseRegex = true,
                Category = CategoryName,
                Zone = "veeshan",
                Timer = new TriggerTimer
                {
                    TimerType = TimerType.CountDown,
                    TimerName = "Word Of Resto",
                    Seconds = 8,
                    RestartBehavior = TimerRestartBehavior.RestartTimer,
                    BarColor = "Orange",
                    IconName = "Diseased Cloud",
                    ShowInOverlay = true
                },
                TimerEnded = new TriggerTimerEnded
                {
                    Enabled = true,
                    Output = new TriggerOutput
                    {
                        DisplayTextEnabled = true,
                        DisplayText = "Resto Now",
                        DisplayTextColor = "Gold",
                        AudioType = TriggerAudioType.TextToSpeech,
                        TtsText = "Resto Now"
                    }
                }
            };
        }

        // Zlandicar (Dragon Necropolis) casts Dragon Roar -> 36s fear timer. Fires on the fear
        // message ("You flee in terror.") or a resist of it, only while in necropolis. Ported from
        // the old ZlandicarHandler. Lives in the Built In root (not the DN folder) since Dragon Roar
        // is a general dragon mechanic. Auto-enabled for users on startup
        // (see EQToolSettingsLoad.EnableDragonRoarTriggerIfMissing).
        public const string DragonRoarBuiltInId = "builtin:dragon-roar";
        public static Trigger CreateDragonRoar()
        {
            return new Trigger
            {
                IsBuiltIn = true,
                BuiltInId = DragonRoarBuiltInId,
                BuiltInFolder = "Encounters",
                TriggerEnabled = false,
                TriggerId = Guid.NewGuid(),
                TriggerName = "Dragon Roar",
                SearchText = @"(You flee in terror\.|You resist the Dragon Roar spell!)",
                UseRegex = true,
                Category = CategoryName,
                Timer = new TriggerTimer
                {
                    TimerType = TimerType.CountDown,
                    TimerName = "Dragon Roar",
                    Seconds = 36,
                    RestartBehavior = TimerRestartBehavior.RestartTimer,
                    BarColor = "Orange",
                    IconName = "Dragon Roar",
                    ShowInOverlay = true
                }
            };
        }

        // "Your <spell> spell has worn off." -> "<spell> faded" alert. Ported from the alert
        // portion of SpellWornOffOtherHandler (which still handles removing the faded spell from
        // the timer window). Auto-enabled for users on startup
        // (see EQToolSettingsLoad.EnableSpellWornOffTriggerIfMissing).
        public const string SpellWornOffBuiltInId = "builtin:spell-worn-off";
        public static Trigger CreateSpellWornOff()
        {
            return Build(SpellWornOffBuiltInId, "Spell Worn Off", @"^Your (?<spell>[\w ]+) spell has worn off\.", true, "{spell} faded", "{spell} faded", "Combat");
        }

        // "You have entered <zone>." -> "You zoned into <zone>" alert. Ported from the alert
        // portion of YouZonedHandler (which still tracks the player's current zone). The negative
        // lookahead skips the non-zone "You have entered an Arena (PvP) area." / "...an area where..."
        // messages so only real zone changes alert.
        public const string EnteredZoneBuiltInId = "builtin:entered-zone";
        public static Trigger CreateEnteredZone()
        {
            return Build(EnteredZoneBuiltInId, "Entered Zone", @"^You have entered (?!an Arena|an area)(?<zone>.+)\.", true, "You zoned into {zone}", "You zoned into {zone}", "Utility");
        }

        // "Your charm spell has worn off." -> charm break alert.
        public static Trigger CreateCharmBreak()
        {
            return Build("builtin:charm-break", "Charm Break", "Your charm spell has worn off.", false, "Charm Break", "Charm Break", "Combat");
        }

        // "<name> tells you, '...'" -> tell alert. The sender must be a single word: player names
        // never contain spaces, so multi-word senders ("Peron ThreadSpinner tells you, ...") are
        // merchants/NPCs and are excluded. Real regex (not the simplified {name} form, which also
        // matches spaces), anchored so the engine can't skip to the last word of an NPC's name.
        public static Trigger CreateTellsYou()
        {
            return Build("builtin:tells-you", "Tells You", @"^(?<name>[\w`]+)( tells you, '| -> {c}: )(?!I'll give you|Attacking |Welcome to my bank|Come back soon)", true, "{name} sent a tell", "{name} sent a tell", "Utility");
        }
        public static Trigger CreateEnraged()
        {
            var trigger = Build("builtin:enraged", "Enraged", "{npc} has become ENRAGED.", true, "{npc} ENRAGED", "{npc} is enraged");
            trigger.BuiltInFolder = "Encounters";
            return trigger;
        }

        // "You feel as if you are about to fall." -> "Levitate Fading".
        public static Trigger CreateLevitateFading()
        {
            return Build("builtin:levitate-fading", "Levitate Fading", "You feel as if you are about to fall.", false, "Levitate Fading", "Levitate Fading", "Utility");
        }

        // "You feel yourself starting to appear." -> "Invisability Fading.".
        public static Trigger CreateInvisFading()
        {
            return Build("builtin:invis-fading", "Invis Fading", "You feel yourself starting to appear.", false, "Invisability Fading.", "Invisability Fading.", "Utility");
        }

        // "<name> has fallen to the ground." -> failed feign death alert.
        public static Trigger CreateFailedFeign()
        {
            return Build("builtin:failed-feign", "Failed Feign", "{c} has fallen to the ground.", true, "{c} Feign Failed Death!", "{c} Failed Feign Death", "Combat");
        }

        // "<name> invites you to join a group." -> group invite alert.
        public static Trigger CreateGroupInvite()
        {
            return Build("builtin:group-invite", "Group Invite", "{name} invites you to join a group.", true, "{name} Invites you to a group", "{name} Invites you to a group", "Utility");
        }

        // "<npc> begins to cast the gate spell." -> npc gating alert.
        public static Trigger CreateNpcGating()
        {
            return Build("builtin:npc-gating", "NPC Gating", "{npc} begins to cast the gate spell.", true, "{npc} begins to Gate", "{npc} begins to Gate", "Combat");
        }

        // A display-only built-in: shows overlay text and never speaks. The audio text is still
        // stored (with AudioType None) so the user can flip on TTS later without retyping it.
        // Encounter AOE warnings sourced from Zones.cs's NPCThatAOE lists. Each is a 12s overlay
        // countdown that fires on the spell's cast (on you / on others) or its resist, restricted to
        // the listed zone. Dragon Roar is intentionally NOT here (it has its own single trigger).
        // To add another, append a row below; it is auto-registered in All() and seeded disabled
        // on startup like any other non-top-level built-in (see EQToolSettingsLoad).
        private sealed class AoeSpec
        {
            public string Spell;   // canonical spell name (used for the icon, and the BuiltInId when Id is null)
            public string Name;    // trigger + timer display name (zone-suffixed where it repeats)
            public string Zone;    // zone short name the trigger is restricted to
            public string Folder;  // BuiltInFolder it lives under
            public string Search;  // regex: (cast on you | cast on other | You resist the X spell!)
            public string Id = null;            // explicit BuiltInId; generated from zone+spell when null
            public string BarColor = "Orange";
            public int Minutes = 0;             // timer duration
            public int Seconds = 12;
            public bool ShowInOverlay = true;   // also mirror the countdown in the overlay window
            public bool AlertText = false;      // also show/say the spell name on cast (Basic output)
        }

        private static readonly AoeSpec[] EncounterAoes =
        {
            new AoeSpec { Spell="Stun Breath",            Name="Stun Breath",            Zone="necropolis",    Folder="Encounters/DN",  Id="builtin:dn-stun-breath",            BarColor="Gold",       Search=@"(Your eardrums rupture\.|staggers with intense pain\.|You resist the Stun Breath spell!)" },
            new AoeSpec { Spell="Cloud of Silence",       Name="Cloud of Silence",       Zone="growthplane",   Folder="Encounters/POG", Id="builtin:pog-cloud-of-silence",      Minutes=3, Seconds=0, ShowInOverlay=false, AlertText=true, Search=@"(You are in a cloud of silence\.|is surrounded by a cloud of silence\.)" },
            new AoeSpec { Spell="Rain of Molten Lava",    Name="Rain of Molten Lava",    Zone="templeveeshan", Folder="Encounters/TOV", Id="builtin:tov-rain-of-molten-lava",   Search=@"(Lava sears your skin\.|skin is seared by lava\.|You resist the Rain of Molten Lava spell!)" },
            new AoeSpec { Spell="Wave of Heat",           Name="Wave of Heat",           Zone="templeveeshan", Folder="Encounters/TOV", Id="builtin:tov-wave-of-heat",          Search=@"(A blast of heat sears your skin\.|skin sears\.|You resist the Wave of Heat spell!)" },
            new AoeSpec { Spell="Wave of Cold",           Name="Wave of Cold",           Zone="templeveeshan", Folder="Encounters/TOV", Id="builtin:tov-wave-of-cold",          BarColor="DeepSkyBlue", Search=@"(A blast of cold freezes your skin\.|skin freezes\.|You resist the Wave of Cold spell!)" },
            new AoeSpec { Spell="Frost Breath",           Name="Frost Breath - TOV",     Zone="templeveeshan", Folder="Encounters/TOV", Id="builtin:tov-frost-breath",          BarColor="DeepSkyBlue",  Search=@"(body freezes as the frost hits you\.|body freezes as the frost hits them\.|You resist the Frost Breath spell!)" },
            new AoeSpec { Spell="Frost Breath",           Name="Frost Breath - Perma",   Zone="permafrost",    Folder="Encounters/VOX", Id="builtin:vox-frost-breath",          BarColor="DeepSkyBlue",  Search=@"(body freezes as the frost hits you\.|body freezes as the frost hits them\.|You resist the Frost Breath spell!)" },
            new AoeSpec { Spell="Silver Breath",          Name="Silver Breath - TOV",    Zone="templeveeshan", Folder="Encounters/TOV", Id="builtin:tov-silver-breath",         BarColor="Orange",      Search=@"(You feel your skin freeze\.|skin freezes\.|You resist the Silver Breath spell!)" },
            new AoeSpec { Spell="Silver Breath",          Name="Silver Breath - WW",     Zone="westwastes",    Folder="Encounters/WW",  Id="builtin:ww-silver-breath",          BarColor="Orange",      Search=@"(You feel your skin freeze\.|skin freezes\.|You resist the Silver Breath spell!)" },
            new AoeSpec { Spell="Freezing Breath",        Name="Freezing Breath",        Zone="dreadlands",    Folder="Encounters/Dreadlands",     Search=@"(An icy cold shoots through your body|is slowed by the freezing blast\.|You resist the Freezing Breath spell!)" },
            new AoeSpec { Spell="Ceticious Cloud",        Name="Ceticious Cloud",        Zone="emeraldjungle", Folder="Encounters/Emerald Jungle", Search=@"(Your legs lock in pain as you choke on the noxious poison\.|doubles over in pain as the noxious poison|You resist the Ceticious Cloud spell!)" },
            new AoeSpec { Spell="Silver Breath",          Name="Silver Breath - POG",    Zone="growthplane",   Folder="Encounters/POG",            Search=@"(You feel your skin freeze\.|skin freezes\.|You resist the Silver Breath spell!)" },
            new AoeSpec { Spell="Blinding Fear",          Name="Blinding Fear",          Zone="sebilis",       Folder="Encounters/Sebilis",        Search=@"(You freeze in terror\.|You resist the Blinding Fear spell!)" },
            new AoeSpec { Spell="Poison Breath",          Name="Poison Breath",          Zone="sebilis",       Folder="Encounters/Sebilis",        Search=@"(A green mist seeps in to your skin\.|You resist the Poison Breath spell!)" },
            new AoeSpec { Spell="Immolating Breath",      Name="Immolating Breath",      Zone="skyfire",       Folder="Encounters/Skyfire",        Search=@"(Your flesh is seared from your bones\.|flesh is seared\.|You resist the Immolating Breath spell!)" },
            new AoeSpec { Spell="Mind Cloud",             Name="Mind Cloud",             Zone="skyshrine",     Folder="Encounters/Skyshrine",      Search=@"(A cloud of pain covers you\.|is covered by a cloud of pain\.|You resist the Mind Cloud spell!)" , Minutes=2, Seconds=0 },
            new AoeSpec { Spell="Ice Breath",             Name="Ice Breath",             Zone="skyshrine",     Folder="Encounters/Skyshrine",      Search=@"(Shards of magical ice rend you\.|cut by shards of magical ice\.|You resist the Ice Breath spell!)", Minutes=2, Seconds=0 },
            new AoeSpec { Spell="Lava Breath",            Name="Lava Breath - Sol B",    Zone="soldungb",      Folder="Encounters/Sol B",          Search=@"(Your body combusts as the lava hits you\.|body combusts as the lava hits them\.|You resist the Lava Breath spell!)" },
            new AoeSpec { Spell="Lava Breath",            Name="Lava Breath - TOV",      Zone="templeveeshan", Folder="Encounters/TOV",            Search=@"(Your body combusts as the lava hits you\.|body combusts as the lava hits them\.|You resist the Lava Breath spell!)" },
            new AoeSpec { Spell="Cloud of Disempowerment",Name="Cloud of Disempowerment",Zone="templeveeshan", Folder="Encounters/TOV",            Search=@"(You feel your skin freeze\.|skin freezes\.|You resist the Cloud of Disempowerment spell!)" },
            new AoeSpec { Spell="Electric Blast",         Name="Electric Blast",         Zone="templeveeshan", Folder="Encounters/TOV",            Search=@"(Your body is electrified as lightning strikes you\.|body is electrified as the lightning strikes\.|You resist the Electric Blast spell!)" },
            new AoeSpec { Spell="Cloud of Fear",          Name="Cloud of Fear",          Zone="templeveeshan", Folder="Encounters/TOV",            Search=@"(Your mind is wracked by fear\.|looks very afraid\.|You resist the Cloud of Fear spell!)" },
            new AoeSpec { Spell="Bellowing Winds",        Name="Bellowing Winds",        Zone="templeveeshan", Folder="Encounters/TOV",            Search=@"(You begin to spin\.|begins to spin\.|You resist the Bellowing Winds spell!)" },
            new AoeSpec { Spell="Tsunami",                Name="Tsunami",                Zone="templeveeshan", Folder="Encounters/TOV",            Search=@"(A tsunami crushes you\.|is crushed by a wall of water\.|You resist the Tsunami spell!)" },
            new AoeSpec { Spell="Wave of Flame",          Name="Wave of Flame",          Zone="templeveeshan", Folder="Encounters/TOV",            Search=@"(You feel your skin burn\.|skin burns\.|You resist the Wave of Flame spell!)" },
            new AoeSpec { Spell="Scream of Chaos",        Name="Scream of Chaos",        Zone="templeveeshan", Folder="Encounters/TOV",            Search=@"(You experience chaotic weightlessness\.|rises chaotically into the air\.|You resist the Scream of Chaos spell!)" },
            new AoeSpec { Spell="Chaos Breath",           Name="Chaos Breath - TOV",     Zone="templeveeshan", Folder="Encounters/TOV",            Search=@"(You experience chaotic weightlessness\.|rises chaotically into the air\.|You resist the Chaos Breath spell!)" },
            new AoeSpec { Spell="Ancient Breath",         Name="Ancient Breath",         Zone="templeveeshan", Folder="Encounters/TOV",            Search=@"(Your life force drains away\.|You resist the Ancient Breath spell!)" },
            new AoeSpec { Spell="Diseased Cloud",         Name="Diseased Cloud - TOV",   Zone="templeveeshan", Folder="Encounters/TOV",            Search=@"(body begins to rot\.|You resist the Diseased Cloud spell!)" },
            new AoeSpec { Spell="Mesmerizing Breath",     Name="Mesmerizing Breath",     Zone="veeshan",       Folder="Encounters/VP",             Search=@"(You are in a cloud of silence\.|is surrounded by a cloud of silence\.|You resist the Mesmerizing Breath spell!)" },
            new AoeSpec { Spell="Chaos Breath",           Name="Chaos Breath - VP",      Zone="veeshan",       Folder="Encounters/VP",             Search=@"(You experience chaotic weightlessness\.|rises chaotically into the air\.|You resist the Chaos Breath spell!)" },
            new AoeSpec { Spell="Stun Breath",            Name="Stun Breath - VP",       Zone="veeshan",       Folder="Encounters/VP",             Search=@"(Your eardrums rupture\.|staggers with intense pain\.|You resist the Stun Breath spell!)" },
            new AoeSpec { Spell="Stream of Acid",         Name="Stream of Acid",         Zone="veeshan",       Folder="Encounters/VP",             Search=@"(Your body burns as the acid hits you\.|body burns as the acid hits them\.|You resist the Stream of Acid spell!)" },
            new AoeSpec { Spell="Lightning Breath",       Name="Lightning Breath",       Zone="veeshan",       Folder="Encounters/VP",             Search=@"(Your body is electrified as the lightning strikes you\.|body is electrified as the lightning strikes\.|You resist the Lightning Breath spell!)" },
        };

        // Fresh trigger instances for every encounter AOE in the table. Used by All() (library) and
        // by the startup auto-enable seeding.
        public static IEnumerable<Trigger> EncounterAoeTriggers()
        {
            foreach (var s in EncounterAoes)
            {
                yield return BuildAoeTrigger(s);
            }
        }

        private static Trigger BuildAoeTrigger(AoeSpec s)
        {
            var builtInId = s.Id ?? ("builtin:aoe:" + s.Zone + ":" + s.Spell).Replace(' ', '-').ToLowerInvariant();
            var trigger = new Trigger
            {
                IsBuiltIn = true,
                BuiltInId = builtInId,
                BuiltInFolder = s.Folder,
                TriggerEnabled = false,
                TriggerId = Guid.NewGuid(),
                TriggerName = s.Name,
                SearchText = s.Search,
                UseRegex = true,
                Category = CategoryName,
                Zone = s.Zone,
                Timer = new TriggerTimer
                {
                    TimerType = TimerType.CountDown,
                    TimerName = s.Name,
                    Minutes = s.Minutes,
                    Seconds = s.Seconds,
                    RestartBehavior = TimerRestartBehavior.RestartTimer,
                    BarColor = s.BarColor,
                    IconName = s.Spell,
                    ShowInOverlay = s.ShowInOverlay
                }
            };
            if (s.AlertText)
            {
                trigger.Basic = new TriggerOutput
                {
                    DisplayTextEnabled = true,
                    DisplayText = s.Name,
                    DisplayTextColor = "Red",
                    AudioType = TriggerAudioType.TextToSpeech,
                    TtsText = s.Name
                };
            }
            return trigger;
        }

        private static Trigger BuildLegacy(string builtInId, string name, string searchText, string displayText, string audioText, string folder = null)
        {
            return new Trigger
            {
                IsBuiltIn = true,
                BuiltInId = builtInId,
                BuiltInFolder = folder,
                TriggerEnabled = false,
                TriggerId = Guid.NewGuid(),
                TriggerName = name,
                SearchText = searchText,
                UseRegex = true,
                Category = CategoryName,
                Basic = new TriggerOutput
                {
                    DisplayTextEnabled = true,
                    DisplayText = displayText,
                    DisplayTextColor = "Red",
                    AudioType = TriggerAudioType.None,
                    TtsText = audioText
                }
            };
        }

        private static Trigger Build(string builtInId, string name, string searchText, bool useRegex, string displayText, string ttsText, string folder = null)
        {
            return new Trigger
            {
                IsBuiltIn = true,
                BuiltInId = builtInId,
                BuiltInFolder = folder,
                TriggerEnabled = false,
                TriggerId = Guid.NewGuid(),
                TriggerName = name,
                SearchText = searchText,
                UseRegex = useRegex,
                Category = CategoryName,
                Basic = new TriggerOutput
                {
                    DisplayTextEnabled = true,
                    DisplayText = displayText,
                    DisplayTextColor = "Red",
                    AudioType = TriggerAudioType.TextToSpeech,
                    TtsText = ttsText
                }
            };
        }
    }
}
