using System;
using System.IO;
using UnityEngine;
using WheelGame.Core;

namespace WheelGame.Infrastructure
{
    public sealed class JsonFileProgressStore : IProgressStore
    {
        private readonly string _path;

        public JsonFileProgressStore(string path)
        {
            _path = path;
        }

        public bool TryLoad(out ProgressData data)
        {
            data = null;

            if (!File.Exists(_path))
            {
                return false;
            }

            try
            {
                data = JsonUtility.FromJson<ProgressData>(File.ReadAllText(_path));
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Could not read progress from {_path}: {exception.Message}");
                return false;
            }

            return data != null;
        }

        public void Save(ProgressData data)
        {
            try
            {
                var temporaryPath = _path + ".tmp";
                File.WriteAllText(temporaryPath, JsonUtility.ToJson(data));

                if (File.Exists(_path))
                {
                    File.Delete(_path);
                }

                File.Move(temporaryPath, _path);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Could not save progress to {_path}: {exception.Message}");
            }
        }
    }
}
