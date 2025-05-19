using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RoamingBots.Componets
{
    public class HookObjectSatic
    {
        private static GameObject HookObject
        {
            get
            {
                if (_HookObject != null)
                    return _HookObject;

                var result = GameObject.Find("Application (Main Client)");
                if (result != null)
                    return result;

                _HookObject = new GameObject("RoamingBotsController");
                Object.DontDestroyOnLoad(result);
                return _HookObject;
            }
        }

        private static GameObject _HookObject;

        public static Component GetOrAddComponent(Type type)
        {
            return HookObject.GetOrAddComponent(type);
        }
    };
    
}
