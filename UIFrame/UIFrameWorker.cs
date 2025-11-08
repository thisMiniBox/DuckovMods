using System;
using System.Collections.Generic;
using TMPro;
using UIFrameAPI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.LowLevel;

namespace UIFrame
{
    public class UIFrameWorker : UIFrameAPIComponent
    {
        public GameOriginMainMenuUI gameOriginMainMenuUI = new GameOriginMainMenuUI();
        public Dictionary<string, Canvas> canvasDic = new Dictionary<string, Canvas>();


        // private void Awake()
        // {
        //     gameOriginMainMenuUI.Initialize();
        // }
        //
        // private void OnDestroy()
        // {
        //     gameOriginMainMenuUI.Cleanup();
        // }
        

        public override bool SetTitleImage(Sprite sprite)
        {
            return gameOriginMainMenuUI.SetTitle(sprite);
        }


        // public override TMP_FontAsset CreateFontAsset(string fontFilePath)
        // {
        //     var font = Font.CreateDynamicFontFromOSFont(fontFilePath, 24);
        //     var tmpFont = TMP_FontAsset.CreateFontAsset(
        //         font,
        //         samplingPointSize: 72, // 采样点大小，影响字体质量
        //         atlasPadding: 4, // 图集内字符间距
        //         renderMode: GlyphRenderMode.SDFAA, // 推荐使用 SDF 抗锯齿模式
        //         atlasWidth: 1024, // 图集宽度 (2的幂)
        //         atlasHeight: 1024, // 图集高度 (2的幂)
        //         atlasPopulationMode: AtlasPopulationMode.Dynamic, // 动态填充
        //         enableMultiAtlasSupport: true // 启用多图集支持
        //     );
        //     return tmpFont;
        // }

        // public override bool SetFont(TMP_FontAsset font)
        // {
        //     return gameOriginMainMenuUI.SetFont(font);
        // }

        public override Texture2D? LoadTexture(string imageFilePath)
        {
            return Utilities.ImageLoader.LoadImageFromFile(imageFilePath);
        }
    }
}