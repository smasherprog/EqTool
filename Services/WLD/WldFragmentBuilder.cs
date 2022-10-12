using EQTool.Services.WLD.Fragments;
using System;
using System.Collections.Generic;

namespace EQTool.Services.WLD
{

    public static class WldFragmentBuilder
    {
        public static Dictionary<int, Func<WldFragment>> Fragments = new Dictionary<int, Func<WldFragment>>
        {
            // Materials
            {0x03, () => new BitmapName()},
            {0x04, () => new BitmapInfo()},
            {0x05, () => new BitmapInfoReference()},
            {0x30, () => new Material()},
            {0x31, () => new MaterialList()},

            // BSP Tree
            {0x21, () => new UnusedWldFragment()},
            {0x22, () => new UnusedWldFragment()},
            {0x29, () => new UnusedWldFragment()},

            // Meshes
            {0x36, () => new Mesh()},
            {0x37, () => new UnusedWldFragment()},
            {0x2F, () => new UnusedWldFragment()},
            {0x2D, () => new UnusedWldFragment()},
            {0x2C, () => new LegacyMesh()},

            // Animation
            {0x10, () => new UnusedWldFragment()},
            {0x11, () => new UnusedWldFragment()},
            {0x12, () => new UnusedWldFragment()},
            {0x13, () => new UnusedWldFragment()},
            {0x14, () => new UnusedWldFragment()},

            // Lights
            {0x1B, () => new UnusedWldFragment()},
            {0x1C, () => new UnusedWldFragment()},
            {0x28, () => new UnusedWldFragment()},
            {0x2A, () => new UnusedWldFragment()},
            {0x35, () => new UnusedWldFragment()},

            // Vertex colors
            {0x32, () => new UnusedWldFragment()},
            {0x33, () => new UnusedWldFragment()},

            // Particle Cloud
            {0x26, () => new UnusedWldFragment()},
            {0x27, () => new UnusedWldFragment()},
            {0x34, () => new UnusedWldFragment()},

            // General
            {0x15, () => new UnusedWldFragment()},

            // Not used/unknown
            {0x08, () => new UnusedWldFragment()},
            {0x09, () => new UnusedWldFragment()},
            {0x16, () => new UnusedWldFragment()},
            {0x17, () => new UnusedWldFragment()},
            {0x18, () => new UnusedWldFragment()},
            {0x06, () => new UnusedWldFragment()},
            {0x07, () => new UnusedWldFragment()},
        };
    }
}
