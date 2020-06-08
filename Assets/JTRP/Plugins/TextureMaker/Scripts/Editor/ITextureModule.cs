using UnityEngine;

namespace TextureMaker
{
    public interface ITextureModule
    {
        void Draw();
        void Reset();
        Texture2D GetTexture(Vector2Int textureSize);    
    }
}