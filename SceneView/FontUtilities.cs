using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace SceneView
{
    public static class FontUtilities
    {
        private static readonly Dictionary<string, TMP_FontAsset> _fontCache = new Dictionary<string, TMP_FontAsset>();

        /// <summary>
        /// 根据字体名称从操作系统加载字体，并创建 TMP_FontAsset（带缓存）
        /// 非常遗憾，用不了
        /// </summary>
        /// <param name="fontName">系统字体名称，如 "Arial", "Microsoft YaHei" 等</param>
        /// <returns>对应的 TMP_FontAsset，失败则返回 null</returns>
        public static TMP_FontAsset? GetOrCreateTMPFont(string fontName)
        {
            if (string.IsNullOrWhiteSpace(fontName))
                return null;

            if (_fontCache.TryGetValue(fontName, out var cached))
                return cached;
            
            var baseFont = Font.CreateDynamicFontFromOSFont(fontName, 12); // 12 是临时大小，不影响 TMP 字体质量

            if (baseFont == null || baseFont.dynamic == false)
            {
                Debug.LogWarning($"Font '{fontName}' not found in OS.");
                return null;
            }

            // 创建 TMP 字体资源
            var tmpFont = TMP_FontAsset.CreateFontAsset(
                baseFont,
                samplingPointSize: 72,
                atlasPadding: 4,
                renderMode: GlyphRenderMode.SDFAA,
                atlasWidth: 1024,
                atlasHeight: 1024,
                atlasPopulationMode: AtlasPopulationMode.Dynamic,
                enableMultiAtlasSupport: true
            );

            if (tmpFont != null)
            {
                _fontCache[fontName] = tmpFont;
                Debug.Log($"Loaded TMP font: {fontName}");
            }
            else
            {
                Debug.LogError($"Failed to create TMP_FontAsset from font: {fontName}");
            }

            return tmpFont;
        }

        /// <summary>
        /// 清空缓存（谨慎使用，通常不需要）
        /// </summary>
        public static void ClearCache()
        {
            _fontCache.Clear();
        }
    }
}