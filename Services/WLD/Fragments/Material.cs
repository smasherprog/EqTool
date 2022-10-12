using System.Collections.Generic;

namespace EQTool.Services.WLD.Fragments
{
    public enum ShaderType
    {
        Diffuse = 0,
        Transparent25 = 1,
        Transparent50 = 2,
        Transparent75 = 3,
        TransparentAdditive = 4,
        TransparentAdditiveUnlit = 5,
        TransparentMasked = 6,
        DiffuseSkydome = 7,
        TransparentSkydome = 8,
        TransparentAdditiveUnlitSkydome = 9,
        Invisible = 10,
        Boundary = 11,
    }
    public enum MaterialType
    {
        // Used for boundaries that are not rendered. TextInfoReference can be null or have reference.
        Boundary = 0x0,
        // Standard diffuse shader
        Diffuse = 0x01,
        // Diffuse variant
        Diffuse2 = 0x02,
        // Transparent with 0.5 blend strength
        Transparent50 = 0x05,
        // Transparent with 0.25 blend strength
        Transparent25 = 0x09,
        // Transparent with 0.75 blend strength
        Transparent75 = 0x0A,
        // Non solid surfaces that shouldn't really be masked
        TransparentMaskedPassable = 0x07,
        TransparentAdditiveUnlit = 0x0B,
        TransparentMasked = 0x13,
        Diffuse3 = 0x14,
        Diffuse4 = 0x15,
        TransparentAdditive = 0x17,
        Diffuse5 = 0x19,
        InvisibleUnknown = 0x53,
        Diffuse6 = 0x553,
        CompleteUnknown = 0x1A, // TODO: Analyze this
        Diffuse7 = 0x12,
        Diffuse8 = 0x31,
        InvisibleUnknown2 = 0x4B,
        DiffuseSkydome = 0x0D, // Need to confirm
        TransparentSkydome = 0x0F, // Need to confirm
        TransparentAdditiveUnlitSkydome = 0x10,
        InvisibleUnknown3 = 0x03,
    }

    public class Material : WldFragment
    {
        /// <summary>
        /// The BitmapInfoReference that this material uses
        /// </summary>
        public BitmapInfoReference BitmapInfoReference { get; private set; }

        /// <summary>
        /// The shader type that this material uses when rendering
        /// </summary>
        public ShaderType ShaderType { get; set; }

        public float Brightness { get; set; }
        public float ScaledAmbient { get; set; }

        /// <summary>
        /// If a material has not been handled, we still need to find the corresponding material list
        /// Used for alternate character skins
        /// </summary>
        public bool IsHandled { get; set; }

        public override void Initialize(int index, int size, byte[] data,
            List<WldFragment> fragments,
            Dictionary<int, string> stringHash, bool isNewWldFormat)
        {
            base.Initialize(index, size, data, fragments, stringHash, isNewWldFormat);
            Name = stringHash[-Reader.ReadInt32()];
            _ = Reader.ReadInt32();
            var parameters = Reader.ReadInt32();

            // Unsure what this color is used for
            // Referred to as the RGB pen
            _ = Reader.ReadByte();
            _ = Reader.ReadByte();
            _ = Reader.ReadByte();
            _ = Reader.ReadByte();

            Brightness = Reader.ReadSingle();
            ScaledAmbient = Reader.ReadSingle();

            var fragmentReference = Reader.ReadInt32();

            if (fragmentReference != 0)
            {
                BitmapInfoReference = fragments[fragmentReference - 1] as BitmapInfoReference;
            }

            // Thanks to PixelBound for figuring this out
            var materialType = (MaterialType)(parameters & ~0x80000000);

            switch (materialType)
            {
                case MaterialType.Boundary:
                    ShaderType = ShaderType.Boundary;
                    break;
                case MaterialType.InvisibleUnknown:
                case MaterialType.InvisibleUnknown2:
                case MaterialType.InvisibleUnknown3:
                    ShaderType = ShaderType.Invisible;
                    break;
                case MaterialType.Diffuse:
                case MaterialType.Diffuse3:
                case MaterialType.Diffuse4:
                case MaterialType.Diffuse6:
                case MaterialType.Diffuse7:
                case MaterialType.Diffuse8:
                case MaterialType.Diffuse2:
                case MaterialType.CompleteUnknown:
                case MaterialType.TransparentMaskedPassable:
                    ShaderType = ShaderType.Diffuse;
                    break;
                case MaterialType.Transparent25:
                    ShaderType = ShaderType.Transparent25;
                    break;
                case MaterialType.Transparent50:
                    ShaderType = ShaderType.Transparent50;
                    break;
                case MaterialType.Transparent75:
                    ShaderType = ShaderType.Transparent75;
                    break;
                case MaterialType.TransparentAdditive:
                    ShaderType = ShaderType.TransparentAdditive;
                    break;
                case MaterialType.TransparentAdditiveUnlit:
                    ShaderType = ShaderType.TransparentAdditiveUnlit;
                    break;
                case MaterialType.TransparentMasked:
                case MaterialType.Diffuse5:
                    ShaderType = ShaderType.TransparentMasked;
                    break;
                case MaterialType.DiffuseSkydome:
                    ShaderType = ShaderType.DiffuseSkydome;
                    break;
                case MaterialType.TransparentSkydome:
                    ShaderType = ShaderType.TransparentSkydome;
                    break;
                case MaterialType.TransparentAdditiveUnlitSkydome:
                    ShaderType = ShaderType.TransparentAdditiveUnlitSkydome;
                    break;
                default:
                    ShaderType = BitmapInfoReference == null ? ShaderType.Invisible : ShaderType.Diffuse;
                    break;
            }
        }

        public List<string> GetAllBitmapNames(bool includeExtension = false)
        {
            var bitmapNames = new List<string>();

            if (BitmapInfoReference == null)
            {
                return bitmapNames;
            }

            foreach (var bitmapName in BitmapInfoReference.BitmapInfo.BitmapNames)
            {
                var filename = bitmapName.Filename;

                if (!includeExtension)
                {
                    filename = filename.Substring(0, filename.Length - 4);
                }

                bitmapNames.Add(filename);
            }

            return bitmapNames;
        }

        /// <summary>
        /// Returns the first bitmap name this material uses
        /// </summary>
        /// <returns></returns>
        public string GetFirstBitmapNameWithoutExtension()
        {
            return BitmapInfoReference?.BitmapInfo?.BitmapNames == null || BitmapInfoReference.BitmapInfo.BitmapNames.Count == 0
                ? string.Empty
                : BitmapInfoReference.BitmapInfo.BitmapNames[0].GetFilenameWithoutExtension();
        }

        public string GetFirstBitmapExportFilename()
        {
            return BitmapInfoReference?.BitmapInfo?.BitmapNames == null || BitmapInfoReference.BitmapInfo.BitmapNames.Count == 0
                ? string.Empty
                : BitmapInfoReference.BitmapInfo.BitmapNames[0].GetExportFilename();
        }

        public string GetFullMaterialName()
        {
            return MaterialList.GetMaterialPrefix(ShaderType) +
                    FragmentNameCleaner.CleanName(this);
        }

        public void SetBitmapName(int index, string newName)
        {
            if (BitmapInfoReference == null)
            {
                return;
            }

            BitmapInfoReference.BitmapInfo.BitmapNames[index].Filename = newName + ".bmp";
        }
    }
}
