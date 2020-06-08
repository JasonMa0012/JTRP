using UnityEngine;

namespace TextureMaker
{
    public interface IToolsModule
    {
        void Draw();
        void Reset();
        Texture2D GetTexture();
    }
}
