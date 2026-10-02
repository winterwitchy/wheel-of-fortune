using UnityEngine;
using UnityEngine.UI;

namespace WheelGame.Views
{
    public static class ButtonLookup
    {
        public static Button FindInChildren(Component root, string buttonName)
        {
            foreach (var button in root.GetComponentsInChildren<Button>(true))
            {
                if (button.name == buttonName)
                    return button;
            }

            Debug.LogWarning($"{root.name}: button '{buttonName}' not found.", root);
            return null;
        }
    }
}