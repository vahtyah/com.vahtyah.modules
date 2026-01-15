using System.Collections.Generic;
using UnityEngine;
namespace VahTyah
{
    // [CreateAssetMenu(fileName = "StylesDatabase", menuName = "Custom List/List Styles Database", order = 1)]
    public class ListStylesDatabase : ScriptableObject
    {
        [SerializeField, BoxGroup("Components", "Components") ] private List<CustomListStyle> styles = new List<CustomListStyle>();

        public int StylesCount => styles.Count;

        public CustomListStyle GetStyle(int index)
        {
            if (styles == null || styles.Count == 0)
            {
                Debug.LogWarning("Styles database is empty.  Creating default style.");
                AddDefaultStyle();
            }

            return styles[Mathf.Clamp(index, 0, styles.Count - 1)];
        }

        public CustomListStyle GetStyleByName(string styleName)
        {
            foreach (var style in styles)
            {
                if (style.name == styleName)
                    return style;
            }

            Debug.LogWarning($"Style '{styleName}' not found.  Returning first style.");
            return GetStyle(0);
        }

        public List<string> GetStyleNames()
        {
            List<string> names = new List<string>();
            foreach (var style in styles)
            {
                names.Add(style.name);
            }
            return names;
        }

        [Button]
        public void AddDefaultStyle()
        {
            CustomListStyle style = new CustomListStyle();
            style.SetDefaultStyleValues();
            style.name = $"Style {styles.Count + 1}";
            styles.Add(style);
        }

        public void AddDarkStyle()
        {
            CustomListStyle style = new CustomListStyle();
            style. SetDefaultStyleValues();
            style.name = "Dark";
            styles.Add(style);
        }

        public void AddLightStyle()
        {
            CustomListStyle style = new CustomListStyle();
            style.SetDefaultStyleValues();
            style. name = "Light";
            
            // Override colors for light theme
            style.header. textColor = new Color(0.1f, 0.1f, 0.1f, 1f);
            style. header.backgroundConfig.layers[0].color = new Color(0.75f, 0.75f, 0.75f, 1f);
            
            style.element.textColor = new Color(0.1f, 0.1f, 0.1f, 1f);
            style.element. selectedBackgroundConfig.layers[0].color = new Color(0.3f, 0.5f, 0.8f, 1f);
            
            style.globalBackground.backgroundConfig.layers[0].color = new Color(0.85f, 0.85f, 0.85f, 1f);
            style.globalBackground.backgroundConfig.layers[1]. color = new Color(0.6f, 0.6f, 0.6f, 1f);
            
            style. footerButtons.backgroundConfig.layers[0].color = new Color(0.85f, 0.85f, 0.85f, 1f);
            style.footerButtons.backgroundConfig.layers[1].color = new Color(0.6f, 0.6f, 0.6f, 1f);
            
            style.pagination.backgroundConfig.layers[0].color = new Color(0.75f, 0.75f, 0.75f, 1f);
            
            styles.Add(style);
        }

        public void AddBlueStyle()
        {
            CustomListStyle style = new CustomListStyle();
            style.SetDefaultStyleValues();
            style.name = "Blue";
            
            // Blue theme colors
            style.header. textColor = new Color(0.8f, 0.9f, 1f, 1f);
            style.header. backgroundConfig.layers[0].color = new Color(0.2f, 0.3f, 0.45f, 1f);
            
            style.element.textColor = new Color(0.9f, 0.95f, 1f, 1f);
            style.element. selectedBackgroundConfig.layers[0].color = new Color(0.3f, 0.5f, 0.8f, 1f);
            
            style. globalBackground.backgroundConfig.layers[0].color = new Color(0.15f, 0.2f, 0.3f, 1f);
            style.globalBackground.backgroundConfig. layers[1].color = new Color(0.1f, 0.15f, 0.25f, 1f);
            
            style.footerButtons. backgroundConfig.layers[0].color = new Color(0.15f, 0.2f, 0.3f, 1f);
            style.footerButtons.backgroundConfig.layers[1]. color = new Color(0.1f, 0.15f, 0.25f, 1f);
            
            style. pagination.backgroundConfig.layers[0].color = new Color(0.2f, 0.3f, 0.45f, 1f);
            
            styles. Add(style);
        }

        public void AddGreenStyle()
        {
            CustomListStyle style = new CustomListStyle();
            style.SetDefaultStyleValues();
            style.name = "Green";
            
            // Green/Matrix theme colors
            style.header. textColor = new Color(0.3f, 1f, 0.3f, 1f);
            style.header.backgroundConfig.layers[0].color = new Color(0.1f, 0.2f, 0.1f, 1f);
            
            style.element.textColor = new Color(0.4f, 1f, 0.4f, 1f);
            style. element.selectedBackgroundConfig.layers[0].color = new Color(0.2f, 0.6f, 0.2f, 1f);
            
            style.globalBackground.backgroundConfig. layers[0].color = new Color(0.05f, 0.15f, 0.05f, 1f);
            style.globalBackground.backgroundConfig. layers[1].color = new Color(0.1f, 0.3f, 0.1f, 1f);
            
            style.footerButtons.backgroundConfig.layers[0].color = new Color(0.05f, 0.15f, 0.05f, 1f);
            style.footerButtons.backgroundConfig.layers[1].color = new Color(0.1f, 0.3f, 0.1f, 1f);
            
            style.pagination. backgroundConfig.layers[0].color = new Color(0.1f, 0.2f, 0.1f, 1f);
            
            styles. Add(style);
        }

        public void AddPurpleStyle()
        {
            CustomListStyle style = new CustomListStyle();
            style.SetDefaultStyleValues();
            style.name = "Purple";
            
            // Purple/Cyberpunk theme colors
            style. header.textColor = new Color(1f, 0.7f, 1f, 1f);
            style.header.backgroundConfig.layers[0].color = new Color(0.3f, 0.15f, 0.45f, 1f);
            
            style.element.textColor = new Color(1f, 0.8f, 1f, 1f);
            style.element. selectedBackgroundConfig.layers[0].color = new Color(0.6f, 0.3f, 0.9f, 1f);
            
            style.globalBackground.backgroundConfig.layers[0].color = new Color(0.2f, 0.1f, 0.3f, 1f);
            style.globalBackground.backgroundConfig.layers[1]. color = new Color(0.4f, 0.2f, 0.6f, 1f);
            
            style.footerButtons. backgroundConfig.layers[0].color = new Color(0.2f, 0.1f, 0.3f, 1f);
            style.footerButtons.backgroundConfig.layers[1]. color = new Color(0.4f, 0.2f, 0.6f, 1f);
            
            style. pagination.backgroundConfig.layers[0].color = new Color(0.3f, 0.15f, 0.45f, 1f);
            
            styles.Add(style);
        }

        public void ClearAllStyles()
        {
            styles.Clear();
        }
    }
}