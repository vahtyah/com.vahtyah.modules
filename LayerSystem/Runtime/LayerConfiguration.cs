using System;
using UnityEngine;

namespace VahTyah
{
    [Serializable]
    public class LayerConfiguration
    {
        [SerializeField] public Layer[] layers;

        public LayerConfiguration()
        {
            layers = Array.Empty<Layer>();
        }

        public LayerConfiguration(int layerCount)
        {
            layers = new Layer[layerCount];
            for (int i = 0; i < layerCount; i++)
            {
                layers[i] = new Layer();
            }
        }

        public void AddLayer(Layer layer)
        {
            Layer[] newLayers = new Layer[layers.Length + 1];
            for (int i = 0; i < layers.Length; i++)
            {
                newLayers[i] = layers[i];
            }

            newLayers[layers.Length] = layer;
            layers = newLayers;
        }

        public Layer GetLayerByType(LayerType type)
        {
            foreach (var layer in layers)
            {
                if (layer.type == type)
                {
                    return layer;
                }
            }

            return null;
        }

        public static LayerConfiguration CreateSimpleBackground(Color color)
        {
            LayerConfiguration config = new LayerConfiguration(1);
            config.layers[0] = Layer.CreateSolidColor(color);
            return config;
        }

        public static LayerConfiguration CreateBackgroundWithBorder(Color backgroundColor, Color borderColor,
            float borderWidth = 1f, float borderRadius = 0f)
        {
            LayerConfiguration config = new LayerConfiguration(2);

            config.layers[0] = Layer.CreateRoundedRect(backgroundColor, borderRadius);

            config.layers[1] = Layer.CreateBorder(borderColor, borderWidth, borderRadius);

            return config;
        }

        public static LayerConfiguration CreateCardStyle(Color cardColor, Color shadowColor, float cornerRadius = 4f)
        {
            LayerConfiguration config = new LayerConfiguration(2)
            {
                layers =
                {
                    [0] = Layer.CreateRoundedRect(shadowColor, cornerRadius),
                    [0] =
                    {
                        padding = new Padding(0, 2, 2, 0)
                    },
                    [1] = Layer.CreateRoundedRect(cardColor, cornerRadius)
                }
            };

            return config;
        }
    }

    [Serializable]
    public class Layer
    {
        [SerializeField] public bool enabled = true;
        [SerializeField] public LayerType type = LayerType.SolidColor;
        [SerializeField] public Color color = Color.white;
        [SerializeField] public Color gradientEndColor = Color.black;
        [SerializeField] public GradientDirection gradientDirection = GradientDirection.Vertical;
        [SerializeField] public Padding padding = new Padding();
        [SerializeField] public Vector4 borderWidth = Vector4.zero;
        [SerializeField] public Vector4 borderRadius = Vector4.zero;

        public Layer()
        {
        }

        public static Layer CreateSolidColor(Color color, Padding padding = null)
        {
            Layer layer = new Layer();
            layer.type = LayerType.SolidColor;
            layer.color = color;
            layer.padding = padding ?? new Padding();
            return layer;
        }

        public static Layer CreateBorder(Color color, float borderWidth = 1f, float borderRadius = 0f,
            Padding padding = null)
        {
            Layer layer = new Layer();
            layer.type = LayerType.Border;
            layer.color = color;
            layer.borderWidth = Vector4.one * borderWidth;
            layer.borderRadius = Vector4.one * borderRadius;
            layer.padding = padding ?? new Padding();
            return layer;
        }

        public static Layer CreateRoundedRect(Color color, float borderRadius = 4f, Padding padding = null)
        {
            Layer layer = new Layer();
            layer.type = LayerType.RoundedRect;
            layer.color = color;
            layer.borderWidth = Vector4.one; // Sẽ được nhân 100 khi vẽ
            layer.borderRadius = Vector4.one * borderRadius;
            layer.padding = padding ?? new Padding();
            return layer;
        }

        public static Layer CreateGradient(Color startColor, Color endColor,
            GradientDirection direction = GradientDirection.Vertical, Padding padding = null)
        {
            Layer layer = new Layer();
            layer.type = LayerType.Gradient;
            layer.color = startColor;
            layer.gradientEndColor = endColor;
            layer.gradientDirection = direction;
            layer.padding = padding ?? new Padding();
            return layer;
        }
    }

    [Serializable]
    public class Padding
    {
        [SerializeField] public float left;
        [SerializeField] public float right;
        [SerializeField] public float top;
        [SerializeField] public float bottom;

        public Padding()
        {
            left = right = top = bottom = 0;
        }

        public Padding(float all)
        {
            left = right = top = bottom = all;
        }

        public Padding(float left, float right, float top, float bottom)
        {
            this.left = left;
            this.right = right;
            this.top = top;
            this.bottom = bottom;
        }

        public Padding(float horizontal, float vertical)
        {
            left = right = horizontal;
            top = bottom = vertical;
        }
    }

    [Serializable]
    public enum LayerType
    {
        SolidColor,
        Border,
        RoundedRect,
        Gradient
    }

    [Serializable]
    public enum GradientDirection
    {
        Horizontal,
        Vertical
    }
}