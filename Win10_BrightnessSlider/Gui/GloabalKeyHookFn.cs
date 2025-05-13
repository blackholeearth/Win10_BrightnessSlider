using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Linq;

//fix oncombination.
using Gma.System.MouseKeyHook.Implementation;
using Gma.System.MouseKeyHook.WinApi;
using Gma.System.MouseKeyHook;
using System.Collections;
using ep_uiauto;

namespace Win10_BrightnessSlider
{
    public static class GloabalKeyHookFn
    {
        public static CombinationData combination_dict_Updater;

        static Dictionary<Combination, Action> combination_dict = new Dictionary<Combination, Action>();
        static Combination copilot_Key = Combination.FromString("LWin+Shift+F23");
        static Combination openEverything_Key = Combination.FromString("LWin+Space");
        static Combination hideEverything_Key = Combination.FromString("LWin" );
        //static Combination lwin_Key = Combination.FromString("LWin");

        static GloabalKeyHookFn() { } 

        //remap  keys -- 
        public static void init_remapKeys(this IKeyboardMouseEvents m_GlobalHook)
        {
            //var copilot_key = Combination.TriggeredBy(Keys.LWin).With(Keys.Shift).With(Keys.F23);
            //var RANDOMKEY2 = Combination.FromString("Control+Y");

            //keycombo_dict = new Dictionary<Combination, Action>
            //{
            //    //{ copilot_key_STR, ()=>  act_copilotkey()  },
            //    //{Combination.FromString("Control+Z"), DoSomething},
            //    //{RANDOMKEY2, () => {   MessageBox.Show("PRESSED:  RANDOM KEY 2 ");} },
            //};


            //// if you call  Hook.GlobalEvents() . 2ndtime, the previous events gets deleted.
            //Hook.GlobalEvents().OnCombination(keycombo_act_list );
            combination_dict_Updater = m_GlobalHook.OnCombination_v2(combination_dict);

        }

        private static void copilotKey_Act()
        {
            Task.Run(() =>
            {
                Thread.Sleep(80);
                SendKeys.SendWait("+({F10})"); //Shift F10 - rightClickMenu
                //SendKeys.SendWait("^({ESC}R)");
                Console.WriteLine(" you pressed Win+Shift+F23");
            });
        }
        public static void copilotKey_toggle(bool Enabled)
        {
            var key = copilot_Key;
            Action act = () => copilotKey_Act();

            ToggleKey(Enabled, key, act);
        }


        /// <summary>
        /// win - space if possible
        /// </summary>
        private static void openEverything_Act()
        {
            Task.Run(() =>
            {
                Form1.Run_EverythingExe();
            });
        }
        public static void openEverything_toggle(bool Enabled)
        {
            var key = openEverything_Key;
            Action act = () => openEverything_Act();

            ToggleKey(Enabled, key, act);
        }


        public static Action on_LWinPresssed_ExtraAction;
        public static void hideEverything_toggle(bool Enabled)
        {
            ToggleKey(Enabled,
                hideEverything_Key,
                   () =>
                   {
                       Console.WriteLine("Lwin pressed - hideEverythingKey_toggle ");

                       var handle = UIAutoPinvoke.GetWindowHandle_byText("everything",(e1) => e1.className == "EVERYTHING");
                       if (handle != IntPtr.Zero)
                           UIAutoPinvoke.ShowWindow(handle, UIAutoPinvoke.SW_HIDE);

                       //---action2
                       on_LWinPresssed_ExtraAction?.Invoke();
                   });
        }
        //public static void on_LWinPresssed_toggle(bool Enabled, Action act)
        //{
        //    ToggleKey(Enabled,
        //        hideEverything_Key,
        //           () =>
        //           {
        //               act?.Invoke();
        //           });
        //}


        //if new keys added in the future
        private static void ToggleKey(bool Enabled, Combination key, Action act)
        {
            if (Enabled)
                TryAdd(key, act);
            else
                TryRemove(key);

            combination_dict_Updater?.UpdateCombinations(combination_dict);
            //update register.
            //m_GlobalHook.De_RegisterCombination();
            //m_GlobalHook.RegisterCombination();
        }
        private static void TryRemove(Combination key)
        {
            var dict = combination_dict;

            if (dict.ContainsKey(key))
                dict.Remove(key);
        }
        public static void TryAdd(Combination key, Action val)
        {
            var dict = combination_dict;

            if (!dict.ContainsKey(key))
                dict.Add(key, val);
        }





    }

    public static class CombinationV2_Extensions
    {
        /// <summary>
        /// same As  OnCombination , But you can Add,Remove,Update your Key combination.
        /// if dictionary is registered you can update  keys in the dictionary, it will automatically. respect changes.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="map"></param>
        /// <param name="reset"></param>
        /// <returns></returns>
        public static CombinationData OnCombination_v2(this IKeyboardEvents source,
                IEnumerable<KeyValuePair<Combination, Action>> map, Action reset = null)
        {
            var watchlists = map.GroupBy(k => k.Key.TriggerKey)
               .ToDictionary(g => g.Key, g => g.ToArray());

            //get reference to this. so i can update the Key_Combinations  , while App is Running.
            var OnCombinationData = new CombinationData();
            { watchlists = watchlists; }

            KeyEventHandler ehandler = (sender, e) =>
            {
                KeyValuePair<Combination, Action>[] element;
                //var found = watchlists.TryGetValue(e.KeyCode.Normalize(), out element);
                var found = OnCombinationData.watchlists.TryGetValue(e.KeyCode.Normalize(), out element);
                if (!found)
                {
                    reset?.Invoke();
                    return;
                }
                var state = KeyboardState.GetCurrent();
                var action = reset;
                var maxLength = 0;
                foreach (var current in element)
                {
                    var matches = current.Key.Chord.All(state.IsDown);
                    if (!matches) continue;
                    if (maxLength > current.Key.ChordLength) continue;
                    maxLength = current.Key.ChordLength;
                    action = current.Value;
                }
                action?.Invoke();
            };
            source.KeyDown += ehandler;

            return OnCombinationData;
        }
    }

    //fix OnCombination
    public class CombinationData
    {
        /// <summary>
        /// user should not touch this.  user Should use UpdateKeyCombinations(new_keys_here);
        /// </summary>
        //internal Dictionary<Keys, KeyValuePair<Combination, Action>[]> watchlists;

        // private set
        private Dictionary<Keys, KeyValuePair<Combination, Action>[]> _watchlists;
        public IReadOnlyDictionary<Keys, KeyValuePair<Combination, Action>[]> watchlists => _watchlists;

        /// <summary>
        /// you keyCombos will be Updated To the new map. (key_action list)
        /// </summary>
        /// <param name="map"></param>
        public void UpdateCombinations(IEnumerable<KeyValuePair<Combination, Action>> map)
        {
            _watchlists = map.GroupBy(k => k.Key.TriggerKey)
             .ToDictionary(g => g.Key, g => g.ToArray());

        }
    }

    internal static class GloabalKeyHookFromRepo
    {

        //internal static class KeysExtensions
        public static Keys Normalize(this Keys key)
        {
            if ((key & Keys.LControlKey) == Keys.LControlKey ||
                (key & Keys.RControlKey) == Keys.RControlKey) return Keys.Control;
            if ((key & Keys.LShiftKey) == Keys.LShiftKey ||
                (key & Keys.RShiftKey) == Keys.RShiftKey) return Keys.Shift;
            if ((key & Keys.LMenu) == Keys.LMenu ||
                (key & Keys.RMenu) == Keys.RMenu) return Keys.Alt;
            return key;
        }
    }

}




