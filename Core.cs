using MelonLoader;

[assembly: MelonInfo(typeof(SimpleCall.Core), "SimpleCall", "1.2.0", "Tiagovito", null)]
[assembly: MelonGame("TVGS", "Schedule I")]

namespace SimpleCall
{
    public class Core : MelonMod
    {
        private static string _previousSceneName = string.Empty;

        public override void OnInitializeMelon()
        {
            ModSettings.Initialize();
            LoggerInstance.Msg("SimpleCall initialized.");
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            switch (sceneName)
            {
                case "Main":
                    Views.CallDealerButtonView.Initialize();
                    break;
                case "Menu":
                    if (_previousSceneName == "Main")
                        Views.CallDealerButtonView.Terminate();
                    break;
            }
            _previousSceneName = sceneName;
        }
    }
}