using System;
using System.IO;
using UnityEngine;

namespace UIFrame.Utilities
{
    public class ImageLoader
    {
        /// <summary>
        /// 从指定文件路径加载图片并创建 Texture2D。
        /// 支持常用的图片格式 (如 .png, .jpg, .jpeg, .bmp, .tga)
        /// </summary>
        /// <param name="filePath">图片文件的绝对路径。</param>
        /// <param name="createNewTexture">如果为true，则创建一个新的Texture2D对象并加载图片数据。如果为false，它会尝试加载到默认的空白Texture2D对象上，但通常建议使用true。</param>
        /// <param name="linear">指定纹理是否加载为线性颜色空间 (true) 或 sRGB 颜色空间 (false)。</param>
        /// <returns>加载成功的 Texture2D 对象，如果失败则返回 null。</returns>
        public static Texture2D? LoadImageFromFile(string filePath, bool createNewTexture = true, bool linear = false)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                Debug.LogError("ImageLoader: 图片文件路径为空或无效。");
                return null;
            }
            
            if (!File.Exists(filePath))
            {
                Debug.LogError($"ImageLoader: 文件不存在于路径: {filePath}");
                return null;
            }

            Texture2D? texture = null;
            try
            {
                var fileData = File.ReadAllBytes(filePath);
                if (createNewTexture)
                {
                    texture = new Texture2D(2, 2, TextureFormat.RGBA32, false, linear);
                }
                else if (texture == null)
                {
                    Debug.LogError("ImageLoader: 未能提供现有Texture2D用于加载，且createNewTexture为false。");
                    return null;
                }

                var success = texture.LoadImage(fileData, true);

                if (!success)
                {
                    Debug.LogError($"ImageLoader: 无法加载图片数据到Texture2D。请检查文件是否为有效的图片格式或是否损坏: {filePath}");
                    UnityEngine.Object.Destroy(texture); // 销毁失败的纹理对象
                    return null;
                }
                return texture;
            }
            catch (Exception ex)
            {
                Debug.LogError($"ImageLoader: 加载图片时发生错误: {filePath} - {ex.Message}");
                if (texture != null)
                {
                    UnityEngine.Object.Destroy(texture); // 发生异常时销毁已创建的纹理
                }

                return null;
            }
        }
    }
}