using System.Collections.Generic;
using UIFrameAPI;
using UnityEngine;

//改为自己的命名空间更好，这是一个画布单元，一个命名空间一个画布
namespace UIFrame
{
    //反射虽然很好用，但我认为用组件传递高效
    public static class UIFrameAPI
    {
        private static UIFrameAPIComponent? _apiComponent;
        private static bool createdCanvas = false;
        private static readonly string NameSpace = typeof(UIFrameAPI).Namespace ?? "UIFrame";

        public static Dictionary<string, Texture2D> textureCache = new Dictionary<string, Texture2D>();
        public static Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();
        
        /// <summary>
        /// 初始化API
        /// </summary>
        /// <returns></returns>
        public static bool Initialize()
        {
            if (_apiComponent!=null)
                return true;
            _apiComponent = Object.FindObjectOfType<UIFrameAPIComponent>();
            return _apiComponent;
        }
        /// <summary>
        /// 设置标题图片（游戏中的标题是图片）
        /// </summary>
        /// <param name="imageFilePath">图片路径</param>
        /// <returns></returns>
        public static bool SetGameTitle(string imageFilePath)
        {
            var texture=LoadSprite(imageFilePath);
            if(texture==null)
            {
                return false;
            }
            return _apiComponent&&_apiComponent.SetTitleImage(texture);
        }
        /// <summary>
        /// 设置标题图片（游戏中的标题是图片）
        /// </summary>
        /// <param name="sprite">贴图</param>
        /// <returns></returns>
        public static bool SetGameTitle(Sprite sprite)
        {
            return _apiComponent&&_apiComponent.SetTitleImage(sprite);
        }
        
        /// <summary>
        /// 加载图片文件为Texture2D
        /// Texture2D实际存储了图片，图片的数据会上传显卡
        /// 此函数默认加载后不保留内存备份，即不可读像素
        /// 函数会建立文件到图片的索引缓存
        /// </summary>
        /// <param name="imageFilePath"></param>
        /// <returns></returns>
        public static Texture2D? LoadImage(string imageFilePath)
        {
            if (!textureCache.ContainsKey(imageFilePath))
            {
                var texture=_apiComponent.LoadTexture(imageFilePath);
                if (texture != null)
                {
                    textureCache[imageFilePath] = texture;
                }
                else
                {
                    Debug.LogError($"加载图片：{imageFilePath}失败");
                    return null;
                }
            }
            return textureCache[imageFilePath];
        }
        /// <summary>
        /// 加载图片为Sprite
        /// 直接简单的调用LoadImage再创建一个代表此图片的Sprite
        /// Sprite只是表明了图片的处理方式，所以为了灵活性建议自己创建
        /// 此函数会缓存地址到Sprite的索引
        /// </summary>
        /// <param name="imageFilePath"></param>
        /// <returns></returns>
        public static Sprite? LoadSprite(string imageFilePath)
        {
            var texture=LoadImage(imageFilePath);
            if (texture==null)
                return null;
            if (!spriteCache.ContainsKey(imageFilePath))
            {
                spriteCache[imageFilePath] = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            }
            return spriteCache[imageFilePath];
        }

    }
}