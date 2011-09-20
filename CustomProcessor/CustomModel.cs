using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using System.ComponentModel;
using System.IO;

namespace Custom_Processor
{

    [ContentProcessor(DisplayName = "Custom Model")]
    public class CustomModel : ModelProcessor
    {
        
        [Browsable(false)]
        public override bool GenerateTangentFrames
        {
            get { return true; }
            set { }
        }

        private string normalMapKey = "NormalMap";
        [DisplayName("Normal Map Key")]
        [Description("Nombre de la variable que contiene el normal map en el shader")]
        [DefaultValue("NormalMap")]
        public string NormalMapKey
        {
            get { return normalMapKey; }
            set { normalMapKey = value; }
        }

        private string heightMapKey = "HeightMap";
        [DisplayName("Height Map Key")]
        [Description("Nombre de la variable que contiene el height map en el shader")]
        [DefaultValue("HeightMap")]
        public string HeightMapKey
        {
            get { return heightMapKey; }
            set { heightMapKey = value; }
        }

        private string diffuseMapKey = "Texture";
        [DisplayName("Diffuse Map Key")]
        [Description("Nombre de la variable que contiene el diffuse map en el shader")]
        [DefaultValue("Texture")]
        public string DiffuseMapKey
        {
            get { return diffuseMapKey; }
            set { diffuseMapKey = value; }
        }

        private string effect = "Effects\\Textured.fx";
        [DisplayName("Effect")]
        [Description("Shader que se aplicará al modelo")]
        [DefaultValue("MyEffect.fx")]
        public string Effect
        {
            get { return effect; }
            set { effect = value; }
        }

        public override ModelContent Process(NodeContent input, ContentProcessorContext context)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            return base.Process(input, context);
        }

        static IList<string> acceptChannelNames = new string[]
        {
            VertexChannelNames.TextureCoordinate(0),
            VertexChannelNames.Normal(0),
            VertexChannelNames.Binormal(0),
            VertexChannelNames.Tangent(0),
        };

        protected override void ProcessVertexChannel(GeometryContent geometry,
            int vertexChannelIndex, ContentProcessorContext context)
        {
            String vertexChannelName =
                geometry.Vertices.Channels[vertexChannelIndex].Name;

            if (acceptChannelNames.Contains(vertexChannelName))
            {
                base.ProcessVertexChannel(geometry, vertexChannelIndex, context);
            }
            else
            {
                geometry.Vertices.Channels.Remove(vertexChannelName);
            }
        }

        protected override MaterialContent ConvertMaterial(MaterialContent material,
                                                          ContentProcessorContext context)
        {
            EffectMaterialContent Material = new EffectMaterialContent();
            Material.Effect = new ExternalReference<EffectContent>(effect);

            foreach (KeyValuePair<String, ExternalReference<TextureContent>> texture
            in material.Textures)
            {
                if ((texture.Key == "Texture"))
                {
                    Material.Textures.Add(diffuseMapKey, texture.Value);
                    Material.Textures.Add(normalMapKey, 
                        new ExternalReference<TextureContent>(texture.Value.Filename.Substring(0, texture.Value.Filename.Length - 4) + "_N.jpg"));
                    Material.Textures.Add(heightMapKey, 
                        new ExternalReference<TextureContent>(texture.Value.Filename.Substring(0, texture.Value.Filename.Length - 4) + "_H.jpg"));
                }
            }
            return context.Convert<MaterialContent, MaterialContent>(Material, typeof(MaterialProcessor).Name);
        }
    }
}