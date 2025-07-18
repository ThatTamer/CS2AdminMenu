using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CS2MenuManager.API.Menu;
using System.Text.Json;

namespace AdminMenu
{
    public class AdminMenu : BasePlugin
    {
        public override string ModuleAuthor => "ThatTamer";
        public override string ModuleName => "AdminMenu";
        public override string ModuleVersion => "1.0.0";

        // Initialize config
        private AdminMenuConfig Config = new();

        // Load JSON Config
        public override void Load(bool hotReload)
        {
            LoadConfig();
        }

        // Load the Config file
        private void LoadConfig()
        {
            string configPath = Path.Combine(ModuleDirectory, "adminmenu_config.json");

            // Initial Setup
            if (!File.Exists(configPath))
            {
                string defaultJson = JsonSerializer.Serialize(Config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(configPath, defaultJson);
                Console.WriteLine($"[AdminMenu] Created default config at: {configPath}");
            }

            // Config Initilization
            try
            {
                string json = File.ReadAllText(configPath);
                AdminMenuConfig? loadedConfig = JsonSerializer.Deserialize<AdminMenuConfig>(json);
                if (loadedConfig != null)
                {
                    Config = loadedConfig;
                    Console.WriteLine("[AdminMenu] Config loaded successfully.");
                }
            }
            // Error -- Debug in console (to:do: easy debug messages.)
            catch (Exception e)
            {
                Console.WriteLine($"[AdminMenu] Failed to load config: {e.Message}");
            }
        }

        // Check if player is an admin
        private bool IsAdmin(CCSPlayerController caller) =>
            caller != null && caller.IsValid && AdminManager.PlayerInGroup(caller, "@css/serveradmin");

        // Admin Command (open admin menu)
        [ConsoleCommand("admin")]
        [ConsoleCommand("css_admin")]
        public void OnShowPlayersCommand(CCSPlayerController caller, CommandInfo info)
        {
            if (!IsAdmin(caller)) return;
            ShowAdminMenu(caller);
        }

        // Display first panel of admin menu
        private void ShowAdminMenu(CCSPlayerController caller)
        {
            var adminPanelMenu = new ScreenMenu("Admin Panel:", this);

            adminPanelMenu.AddItem("Player Management", (p, o) =>
            {
                o.PostSelectAction = CS2MenuManager.API.Enum.PostSelectAction.Reset;
                BuildPlayerManagementMenu().Display(p, 0);
            });

            adminPanelMenu.AddItem("Server Management", (p, o) =>
            {
                o.PostSelectAction = CS2MenuManager.API.Enum.PostSelectAction.Reset;
                BuildServerManagementMenu().Display(p, 0);
            });

            adminPanelMenu.Display(caller, 0);
        }

        // Create secondary menus
        private ScreenMenu BuildPlayerManagementMenu()
        {
            var playerManagement = new ScreenMenu("Player Management:", this);

            playerManagement.AddItem("Comms Management", (p, o) =>
            {
                o.PostSelectAction = CS2MenuManager.API.Enum.PostSelectAction.Reset;
                BuildAdminChatMenu().Display(p, 0);
            });
            playerManagement.AddItem("General Management", (p, o) =>
            {
                o.PostSelectAction = CS2MenuManager.API.Enum.PostSelectAction.Reset;
                BuildAdminGeneralMenu().Display(p, 0);
            });
            playerManagement.AddItem("Fun Commands", (p, o) =>
            {
                o.PostSelectAction = CS2MenuManager.API.Enum.PostSelectAction.Reset;
                BuildAdminFunMenu().Display(p, 0);
            });

            return playerManagement;
        }

        private ScreenMenu BuildServerManagementMenu()
        {
            var serverManagement = new ScreenMenu("Server Management:", this);
            serverManagement.AddItem("Map Management", (p, o) =>
            {
                o.PostSelectAction = CS2MenuManager.API.Enum.PostSelectAction.Reset;
                BuildAdminMapMenu().Display(p, 0);
            });
            return serverManagement;
        }

        // Check if valid user
        private void BuildPlayerSelectMenu(Action<string, CCSPlayerController> onSelected, CCSPlayerController caller)
        {
            var menu = new ScreenMenu("Player Selection:", this);
            foreach (var player in Utilities.GetPlayers())
            {
                if (player == null || !player.IsValid || player.IsBot) continue;
                var currentName = player.PlayerName;
                menu.AddItem(currentName, (p, o) =>
                {
                    o.PostSelectAction = CS2MenuManager.API.Enum.PostSelectAction.Reset;
                    onSelected(currentName, p);
                });
            }
            menu.Display(caller, 0);
        }

        // Select duration
        private void BuildTimeSelectMenu(Action<int, CCSPlayerController> onSelected, CCSPlayerController caller)
        {
            var menu = new ScreenMenu("Time Selection:", this);
            foreach (var duration in Config.Durations)
            {
                var currentTime = duration.Value;
                menu.AddItem(duration.Key, (p, o) =>
                {
                    o.PostSelectAction = CS2MenuManager.API.Enum.PostSelectAction.Reset;
                    onSelected(currentTime, p);
                });
            }
            menu.Display(caller, 0);
        }

        // Select reason
        private void BuildReasonSelectMenu(IEnumerable<string> reasons, Action<string, CCSPlayerController> onSelected, CCSPlayerController caller)
        {
            var menu = new ScreenMenu("Reason:", this);
            foreach (var reason in reasons)
            {
                var currentReason = reason;
                menu.AddItem(currentReason, (p, o) =>
                {
                    o.PostSelectAction = CS2MenuManager.API.Enum.PostSelectAction.Reset;
                    onSelected(currentReason, p);
                });
            }
            menu.Display(caller, 0);
        }

        // Gravity selection
        private void BuildGravitySelectMenu(Action<double, CCSPlayerController> onSelected, CCSPlayerController caller)
        {
            var menu = new ScreenMenu("Gravity Selection:", this);
            foreach (var gravity in Config.GravityValue)
            {
                var currentValue = gravity.Value;
                menu.AddItem(gravity.Key, (p, o) =>
                {
                    o.PostSelectAction = CS2MenuManager.API.Enum.PostSelectAction.Reset;
                    onSelected(currentValue, p);
                });
            }
            menu.Display(caller, 0);
        }

        // Speed selection
        private void BuildSpeedSelectMenu(Action<double, CCSPlayerController> onSelected, CCSPlayerController caller)
        {
            var menu = new ScreenMenu("Speed Selection:", this);
            foreach (var speed in Config.SpeedValue)
            {
                var currentValue = speed.Value;
                menu.AddItem(speed.Key, (p, o) =>
                {
                    o.PostSelectAction = CS2MenuManager.API.Enum.PostSelectAction.Reset;
                    onSelected(currentValue, p);
                });
            }
            menu.Display(caller, 0);
        }

        // Chat/Comms Management Menu
        private ScreenMenu BuildAdminChatMenu()
        {
            var menu = new ScreenMenu("Punishment:", this);

            void PunishMenu(string command)
            {
                menu.AddItem(command, (caller, o) =>
                {
                    o.PostSelectAction = CS2MenuManager.API.Enum.PostSelectAction.Reset;
                    BuildPlayerSelectMenu((selectedPlayer, p) =>
                    {
                        if (command.StartsWith("Un"))
                        {
                            Server.ExecuteCommand($"css_{command.ToLower()} \"{selectedPlayer}\"");
                        }
                        else
                        {
                            BuildTimeSelectMenu((selectedTime, p2) =>
                            {
                                BuildReasonSelectMenu(Config.Reasons, (selectedReason, p3) =>
                                {
                                    Server.ExecuteCommand($"css_{command.ToLower()} \"{selectedPlayer}\" {selectedTime} \"{selectedReason}\"");
                                }, p2);
                            }, p);
                        }
                    }, caller);
                });
            }

            // to:do - refactor
            PunishMenu("Mute");
            PunishMenu("Gag");
            PunishMenu("Silence");
            PunishMenu("Unmute");
            PunishMenu("Ungag");
            PunishMenu("Unsilence");

            return menu;
        }

        // General Management Menu
        private ScreenMenu BuildAdminGeneralMenu()
        {
            var menu = new ScreenMenu("Commands:", this);

            // Ban
            menu.AddItem("Ban", (caller, o) =>
            {
                o.PostSelectAction = CS2MenuManager.API.Enum.PostSelectAction.Reset;
                BuildPlayerSelectMenu((selectedPlayer, p) =>
                {
                    BuildTimeSelectMenu((selectedTime, p2) =>
                    {
                        BuildReasonSelectMenu(Config.BanReasons, (selectedReason, p3) =>
                        {
                            Server.ExecuteCommand($"css_ban \"{selectedPlayer}\" {selectedTime} \"{selectedReason}\"");
                        }, p2);
                    }, p);
                }, caller);
            });

            // CTBan
            menu.AddItem("CTBan", (caller, o) =>
            {
                o.PostSelectAction = CS2MenuManager.API.Enum.PostSelectAction.Reset;
                BuildPlayerSelectMenu((selectedPlayer, p) =>
                {
                    BuildTimeSelectMenu((selectedTime, p2) =>
                    {
                        BuildReasonSelectMenu(Config.CtReasons, (selectedReason, p3) =>
                        {
                            Server.ExecuteCommand($"css_ctban \"{selectedPlayer}\" {selectedTime} \"{selectedReason}\"");
                        }, p2);
                    }, p);
                }, caller);
            });

            // UnCTBan
            menu.AddItem("UnCTBan", (caller, o) =>
            {
                o.PostSelectAction = CS2MenuManager.API.Enum.PostSelectAction.Reset;
                BuildPlayerSelectMenu((selectedPlayer, p) =>
                {
                    Server.ExecuteCommand($"css_unctban \"{selectedPlayer}\"");
                }, caller);
            });

            // Kick
            menu.AddItem("Kick", (caller, o) =>
            {
                o.PostSelectAction = CS2MenuManager.API.Enum.PostSelectAction.Reset;
                BuildPlayerSelectMenu((selectedPlayer, p) =>
                {
                    Server.ExecuteCommand($"css_kick \"{selectedPlayer}\"");
                }, caller);
            });

            // Who
            menu.AddItem("Who", (caller, o) =>
            {
                o.PostSelectAction = CS2MenuManager.API.Enum.PostSelectAction.Reset;
                var playerSelectPanelMenu = new ScreenMenu("Select Player:", this);

                foreach (var player in Utilities.GetPlayers())
                {
                    if (player == null || !player.IsValid || player.IsBot)
                        continue;

                    var name = player.PlayerName;
                    var ip = player.IpAddress ?? "Unknown";
                    var steamId = player.SteamID;
                    var userId = player.UserId;

                    playerSelectPanelMenu.AddItem(name, (p2, opt) =>
                    {
                        opt.PostSelectAction = CS2MenuManager.API.Enum.PostSelectAction.Reset;
                        p2.PrintToConsole("[Client] --------- PLAYER INFO ---------");
                        p2.PrintToConsole($"[Client] • [#{userId}] \"{name}\" (IP Address: \"{ip}\" SteamID64: \"{steamId}\")");
                        p2.PrintToConsole("[Client] --------- END PLAYER INFO ---------");
                    });
                }
                playerSelectPanelMenu.Display(caller, 0);
            });

            // Current Players
            menu.AddItem("Current Players", (caller, o) =>
            {
                o.PostSelectAction = CS2MenuManager.API.Enum.PostSelectAction.Reset;
                caller.PrintToConsole("[Client] --------- PLAYER LIST ---------");
                foreach (var player in Utilities.GetPlayers())
                {
                    if (player == null || !player.IsValid || player.IsBot)
                        continue;

                    var name = player.PlayerName;
                    var ip = player.IpAddress ?? "Unknown";
                    var steamId = player.SteamID;
                    var userId = player.UserId;

                    caller.PrintToConsole($"[Client] • [#{userId}] \"{name}\" (IP Address: \"{ip}\" SteamID64: \"{steamId}\")");
                }
                caller.PrintToConsole("[Client] --------- END PLAYER LIST ---------");
            });

            // Reload Admins
            menu.AddItem("Reload Admins", (caller, o) =>
            {
                o.PostSelectAction = CS2MenuManager.API.Enum.PostSelectAction.Reset;
                Server.ExecuteCommand("css_reloadadmins");
            });

            // Reload Bans
            menu.AddItem("Reload Bans", (caller, o) =>
            {
                o.PostSelectAction = CS2MenuManager.API.Enum.PostSelectAction.Reset;
                Server.ExecuteCommand("css_reloadbans");
            });

            return menu;
        }

        // Fun Commands Menu
        private ScreenMenu BuildAdminFunMenu()
        {
            var menu = new ScreenMenu("Commands:", this);

            // Slap
            menu.AddItem("Slap", (caller, o) =>
            {
                o.PostSelectAction = CS2MenuManager.API.Enum.PostSelectAction.Reset;
                BuildPlayerSelectMenu((selectedPlayer, p) =>
                {
                    Server.ExecuteCommand($"css_slap \"{selectedPlayer}\"");
                }, caller);
            });

            // Freeze
            menu.AddItem("Freeze", (caller, o) =>
            {
                o.PostSelectAction = CS2MenuManager.API.Enum.PostSelectAction.Reset;
                BuildPlayerSelectMenu((selectedPlayer, p) =>
                {
                    BuildTimeSelectMenu((selectedTime, p2) =>
                    {
                        Server.ExecuteCommand($"css_freeze \"{selectedPlayer}\" \"{selectedTime}\"");
                    }, p);
                }, caller);
            });

            // UnFreeze
            menu.AddItem("UnFreeze", (caller, o) =>
            {
                o.PostSelectAction = CS2MenuManager.API.Enum.PostSelectAction.Reset;
                BuildPlayerSelectMenu((selectedPlayer, p) =>
                {
                    Server.ExecuteCommand($"css_unfreeze \"{selectedPlayer}\"");
                }, caller);
            });

            // Gravity
            menu.AddItem("Gravity", (caller, o) =>
            {
                o.PostSelectAction = CS2MenuManager.API.Enum.PostSelectAction.Reset;
                BuildPlayerSelectMenu((selectedPlayer, p) =>
                {
                    BuildGravitySelectMenu((selectedGravity, p2) =>
                    {
                        Server.ExecuteCommand($"css_gravity \"{selectedPlayer}\" \"{selectedGravity}\"");
                    }, p);
                }, caller);
            });

            // Speed
            menu.AddItem("Speed", (caller, o) =>
            {
                o.PostSelectAction = CS2MenuManager.API.Enum.PostSelectAction.Reset;
                BuildPlayerSelectMenu((selectedPlayer, p) =>
                {
                    BuildSpeedSelectMenu((selectedSpeed, p2) =>
                    {
                        Server.ExecuteCommand($"css_speed \"{selectedPlayer}\" \"{selectedSpeed}\"");
                    }, p);
                }, caller);
            });

            // Godmode
            menu.AddItem("Godmode", (caller, o) =>
            {
                o.PostSelectAction = CS2MenuManager.API.Enum.PostSelectAction.Reset;
                BuildPlayerSelectMenu((selectedPlayer, p) =>
                {
                    Server.ExecuteCommand($"css_godmode \"{selectedPlayer}\"");
                }, caller);
            });

            return menu;
        }

        // Map Management Menu
        private ScreenMenu BuildAdminMapMenu()
        {
            var menu = new ScreenMenu("Map Commands:", this);

            menu.AddItem("Change Map", (caller, o) =>
            {
                o.PostSelectAction = CS2MenuManager.API.Enum.PostSelectAction.Reset;
                var mapChangePanelMenu = new ScreenMenu("Map Select:", this);

                int count = Math.Min(Config.MapList.Count, Config.MapNameList.Count);
                for (int i = 0; i < count; i++)
                {
                    string mapName = Config.MapNameList[i];
                    string mapId = Config.MapList[i];

                    mapChangePanelMenu.AddItem($"{mapName} ({mapId})", (p, o2) =>
                    {
                        o2.PostSelectAction = CS2MenuManager.API.Enum.PostSelectAction.Reset;
                        Server.ExecuteCommand($"css_wsmap \"{mapId}\"");
                    });
                }
                mapChangePanelMenu.Display(caller, 0);
            });

            return menu;
        }
    }
}