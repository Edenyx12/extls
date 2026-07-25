using extls.Core;

namespace extls.Tools
{
    public class ApiKeys : Module
    {
        public ApiKeys()
        {
            name = "apikeys";
            version = "0.2a";
            commands = new[]
            {
                new HelpSlot("add", "add the API key to the config", null!, null!),
                new HelpSlot("remove", "remove the API key from the config", null!, null!),
                new HelpSlot("list", "show all keys in the config", null!, null!)
            };
        }

        public override bool Dispatch(string[] args)
        {
            if (base.Dispatch(args)) return false;

            ApiKeysConfig config = JsonService.LoadJson<ApiKeysConfig>("config", "apikeys.json");
            if (config == null)
                config = new ApiKeysConfig(new List<ApiKey>());

            switch (args[0])
            {
                case "add":    Add(ref config);    break;
                case "remove": Remove(ref config); break;
                case "list":   ListKeys(config, true);   return true;
                default:
                    Utils.InvalidOperation();
                    break;
            }

            JsonService.SaveJson<ApiKeysConfig>("config", "apikeys.json", config);
            return true;
        }

        private void Add(ref ApiKeysConfig config)
        {
            Print.Inline("Enter the Key Name > ");
            string name = Console.ReadLine()!;

            Print.Inline("Enter the API Key > ");
            string key = Console.ReadLine()!;

            if ((key == null || key.Length == 0 || key == "") ||
                (name == null || name.Length == 0 || name == ""))
            {
                Print.Warning("The key or key name was empty. Addition cancelled.");
                return;
            }

            config.apikeys.Add(new ApiKey(key, name));
        }
        private void Remove(ref ApiKeysConfig config)
        {
            ListKeys(config, false);

            Print.Inline("Enter the key number for removal (or any for exit) > ");
            if (int.TryParse(Console.ReadLine(), out int index) && 
               (index <= config.apikeys.Count - 1 && index >= 0))
                config.apikeys.RemoveAt(index);
            else { Print.Warning("Invalid number. Exiting."); return; }
        }
        private void ListKeys(ApiKeysConfig config, bool menu)
        {
            Markup.Rich($"Your [yellow]API Keys [gray]({config.apikeys.Count})[white]:\n", null!);

            if (config.apikeys.Count > 0)
            {
                for (int i = 0; i < config.apikeys.Count; i++)
                {
                    string encrypt = "";
                    for (int j = 0; j < config.apikeys[i].key.Length; j++)
                        encrypt += "*";

                    Markup.Rich($"{i}) [blue]{config.apikeys[i].keyname} [white]- [gray]{encrypt}\n", null!);
                }
            }
            else 
            { 
                Print.Warning("The list of API keys is empty."); 
                return;
            }

            if (!menu) return;

            Print.Inline("Enter the key number for show (or any for exit) > ");
            if (int.TryParse(Console.ReadLine(), out int index) &&
               (index <= config.apikeys.Count - 1 && index >= 0))
                Markup.Rich($"[blue]{config.apikeys[index].keyname}[white]: [gray]{config.apikeys[index].key}", null!);
            else { Print.Warning("Invalid number. Exiting."); return; }
        }

        private class ApiKeysConfig
        {
            public List<ApiKey> apikeys { get; set; } = new();

            public ApiKeysConfig(List<ApiKey> apikeys) => this.apikeys = apikeys;

        }
        private struct ApiKey
        {
            public string key { get; set; }
            public string keyname { get; set; }

            public ApiKey(string key, string keyname)
            {
                this.key = key;
                this.keyname = keyname;
            }
        }
    }
}
