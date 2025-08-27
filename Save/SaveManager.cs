using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;

namespace Swift_Blade.Save
{
    public class SaveManager : MonoSingleton<SaveManager>
    {
        private Dictionary<string, object> _jsonDictionary;

        protected override void Awake() {
            base.Awake();

            _jsonDictionary = new Dictionary<string, object>();
        }

        public void Save()
        {
            string path = $"{Application.persistentDataPath}\\Save.json";
            string data = JsonConvert.SerializeObject(_jsonDictionary);
            File.WriteAllText(path, data, Encoding.UTF8);
        }

        public void Load()
        {
            string path = $"{Application.persistentDataPath}\\Save.json";
            string jsonString;

            try
            {
                jsonString = File.ReadAllText(path);
            }
            catch
            {
                string data = JsonConvert.SerializeObject(new Dictionary<string, object>());
                File.WriteAllText(path, data, Encoding.UTF8);
                jsonString = File.ReadAllText(path);
            }

            _jsonDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
        }

        public T GetData<T>(string key) where T : class
        {
            if (_jsonDictionary.TryGetValue(key, out object obj))
                return obj as T;

            Load();
            _jsonDictionary.TryGetValue(key, out obj);

            return obj as T;
        }

        public void AddData<T>(string key, T value)
        {
            _jsonDictionary.Add(key, value);
        }

        public bool IsPlayed()
        {
            string isPlayed = GetData<string>("IsPlayed");

            if(string.IsNullOrEmpty(isPlayed))
                return false;

            return isPlayed == "1";
        }

        #if UNITY_EDITOR

        [ContextMenu("Save Test")]
        private void SaveTest()
        {
            AddData("6equal0", "1008");
            Save();
            Load();
            Debug.Log(GetData<string>("6equal0"));
        }

        #endif
    }
}
