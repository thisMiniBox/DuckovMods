using TMPro;
using UnityEngine;

namespace UIFrameAPI
{
    public abstract class UIFrameAPIComponent:MonoBehaviour
    {
        public abstract bool CreateCanvas(string name);
        
        //设置游戏主菜单的原版标题
        public abstract bool SetTitleImage(Sprite sprite);
        
        //创建一个TMP字体
        public abstract TMP_FontAsset CreateFontAsset(string fontFilePath);
        
        //设置游戏字体
        public abstract bool SetFont(TMP_FontAsset font);
        
        public abstract Texture2D? LoadTexture(string imageFilePath);
    }
}