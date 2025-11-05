using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine; // 假设在Unity环境中使用Debug.LogError

namespace HitFeedback
{
    public class Config
    {
        public KeyCode hotKey = KeyCode.F8;
        public Dictionary<string, float> probability = new Dictionary<string, float>();

        /// <summary>
        /// 当伤害具有这些特性之一时，将播放音频反馈。
        /// </summary>
        public HashSet<DamageFeature> audioDamageFeatures = new HashSet<DamageFeature>();

        public void LoadConfig(string filename)
        {
            if (!File.Exists(filename))
            {
                Debug.LogError($"Config file not found: {filename}");
                return;
            }

            probability.Clear();
            audioDamageFeatures.Clear(); // 清空旧的音频伤害特性数据
            try
            {
                using (var sr = new StreamReader(filename))
                {
                    string line;
                    var lineNumber = 0;
                    string currentSection = ""; // 用于解析节
                    while ((line = sr.ReadLine()) != null)
                    {
                        lineNumber++;
                        line = line.Trim();
                        // 忽略空行和注释行
                        if (string.IsNullOrEmpty(line) || line.StartsWith(";") || line.StartsWith("#"))
                        {
                            continue;
                        }

                        // 处理节标题，例如 [General] 或 [AudioFeatures]
                        if (line.StartsWith("[") && line.EndsWith("]"))
                        {
                            currentSection = line.Substring(1, line.Length - 2).Trim();
                            continue; // 跳过节标题行
                        }

                        // 查找等号
                        var separatorIndex = line.IndexOf('=');
                        if (separatorIndex == -1)
                        {
                            Debug.LogWarning(
                                $"Skipping malformed line in config file '{filename}' at line {lineNumber}: No '=' found. Line: '{line}'");
                            continue;
                        }

                        var key = line.Substring(0, separatorIndex).Trim();
                        var valueStr = line.Substring(separatorIndex + 1).Trim();
                        if (currentSection.Equals("General", StringComparison.OrdinalIgnoreCase))
                        {
                            // 解析 hotKey
                            if (key.Equals("hotKey", StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    hotKey = (KeyCode)Enum.Parse(typeof(KeyCode), valueStr, true);
                                }
                                catch (ArgumentException)
                                {
                                    Debug.LogError(
                                        $"Invalid KeyCode '{valueStr}' in config file '{filename}' at line {lineNumber}. Using default F8.");
                                    hotKey = KeyCode.F8;
                                }
                            }
                        }
                        else if (currentSection.Equals("Probabilities", StringComparison.OrdinalIgnoreCase))
                        {
                            // 解析 probability 字典项
                            if (float.TryParse(valueStr, NumberStyles.Float, CultureInfo.InvariantCulture,
                                    out var probValue))
                            {
                                probability[key] = probValue;
                            }
                            else
                            {
                                Debug.LogWarning(
                                    $"Invalid float value '{valueStr}' for key '{key}' in config file '{filename}' at line {lineNumber}. Skipping entry.");
                            }
                        }
                        else if (currentSection.Equals("AudioFeatures", StringComparison.OrdinalIgnoreCase))
                        {
                            // 解析 audioDamageFeatures 集合项
                            // 键是 'feature' (或者可以随意定义，只要值是我们关心的)
                            // 值是 ExtendedDamageFeature 的枚举名称
                            if (key.Equals("feature", StringComparison.OrdinalIgnoreCase)) // 假设所有特征都用同一个键"feature"
                            {
                                foreach (var featureName in valueStr.Split(new char[] { ',', '|' },
                                             StringSplitOptions.RemoveEmptyEntries))
                                {
                                    try
                                    {
                                        // 确保 ExtendedDamageFeature 是 [Flags] 枚举
                                        var feature = (DamageFeature)Enum.Parse(typeof(DamageFeature),
                                            featureName.Trim(), true);
                                        audioDamageFeatures.Add(feature);
                                    }
                                    catch (ArgumentException)
                                    {
                                        Debug.LogWarning(
                                            $"Invalid ExtendedDamageFeature '{featureName.Trim()}' in config file '{filename}' at line {lineNumber}. Skipping entry.");
                                    }
                                }
                            }
                            else
                            {
                                try
                                {
                                    var feature =
                                        (DamageFeature)Enum.Parse(typeof(DamageFeature), key, true);
                                    if (bool.TryParse(valueStr, out bool includeFeature) && includeFeature)
                                    {
                                        audioDamageFeatures.Add(feature);
                                    }
                                    // 如果是 false，则不添加到集合，或者可以从集合中移除（如果默认是都包含）
                                }
                                catch (ArgumentException)
                                {
                                    Debug.LogWarning(
                                        $"Invalid ExtendedDamageFeature key '{key}' in config file '{filename}' at line {lineNumber}. Skipping entry.");
                                }
                            }
                        }
                        // 如果存在其他节，可以在这里添加 else if 处理
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error reading config file '{filename}': {ex.Message}");
            }
        }

        /// <summary>
        /// 将当前配置存储到指定INI文件。
        /// </summary>
        /// <param name="filename">要保存的INI文件的路径。</param>
        public void SaveConfig(string filename)
        {
            try
            {
                var directory = Path.GetDirectoryName(filename);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using (var sw = new StreamWriter(filename))
                {
                    sw.WriteLine("; HitFeedback Configuration File");
                    sw.WriteLine("; Generated by HitFeedback.Config class");
                    sw.WriteLine();
                    sw.WriteLine("[General]");
                    sw.WriteLine($"hotKey = {hotKey.ToString()}");
                    sw.WriteLine();
                    if (probability.Count > 0)
                    {
                        sw.WriteLine("[Probabilities]");
                        foreach (var kvp in probability)
                        {
                            sw.WriteLine($"{kvp.Key} = {kvp.Value.ToString(CultureInfo.InvariantCulture)}");
                        }
                    }
                    else
                    {
                        sw.WriteLine("; No probabilities currently configured.");
                    }

                    sw.WriteLine();
                    if (audioDamageFeatures.Count > 0)
                    {
                        sw.WriteLine("[AudioFeatures]");
                        // 假设每个特性一行，值为 true
                        foreach (var feature in audioDamageFeatures)
                        {
                            sw.WriteLine($"{feature.ToString()} = True");
                        }
                        // 或者如果你想用一个键存储所有特性（用逗号或竖线分隔）
                        // sw.WriteLine($"features = {string.Join(",", audioDamageFeatures.Select(f => f.ToString()))}");
                    }
                    else
                    {
                        sw.WriteLine("; No audio damage features currently configured.");
                    }

                    sw.WriteLine();
                }

                Debug.Log($"Config saved to: {filename}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error saving config file '{filename}': {ex.Message}");
            }
        }
        /// <summary>
        /// 根据DamageInfo的特性判断是否应该播放音频反馈。
        /// 如果DamageInfo的任何一个特性在audioDamageFeatures集合中，则返回true。
        /// </summary>
        /// <param name="damageInfo">要检查的DamageInfo对象。</param>
        /// <returns>如果应该播放音频反馈，则为true；否则为false。</returns>
        public bool ShouldPlayAudioFeedback(DamageInfo damageInfo)
        {
            if (audioDamageFeatures.Count == 0)
            {
                return false;
            }
            // 将DamageInfo的各种布尔属性/条件转换为DamageFeature组合
            DamageFeature currentDamageFeatures = DamageFeature.Undefined;
            if (damageInfo.damageType == DamageTypes.normal)
            {
                currentDamageFeatures |= DamageFeature.NormalDamage;
            }
            else if (damageInfo.damageType == DamageTypes.realDamage)
            {
                currentDamageFeatures |= DamageFeature.RealDamage;
            }
            
            if (damageInfo.isFromBuffOrEffect)
            {
                currentDamageFeatures |= DamageFeature.BuffOrEffectDamage;
            }
            if (damageInfo.ignoreArmor)
            {
                currentDamageFeatures |= DamageFeature.ArmorIgnoringDamage;
            }
            if (damageInfo.crit > 0) // crit > 0 表示是暴击
            {
                currentDamageFeatures |= DamageFeature.CriticalDamage;
            }
            if (damageInfo.armorPiercing > 0)
            {
                currentDamageFeatures |= DamageFeature.ArmorPiercingDamage;
            }
            if (damageInfo.isExplosion)
            {
                currentDamageFeatures |= DamageFeature.ExplosionDamage;
            }
            if (damageInfo.armorBreak > 0)
            {
                currentDamageFeatures |= DamageFeature.ArmorBreakingDamage;
            }
            if (damageInfo.elementFactors != null && damageInfo.elementFactors.Count > 0)
            {
                currentDamageFeatures |= DamageFeature.ElementalDamage;
            }
            if (damageInfo.buffChance > 0 || damageInfo.buff != null)
            {
                currentDamageFeatures |= DamageFeature.OnHitBuffApply;
            }
            if (damageInfo.bleedChance > 0)
            {
                currentDamageFeatures |= DamageFeature.OnHitBleed;
            }
            
            foreach (var configuredFeature in audioDamageFeatures)
            {
                if (configuredFeature == DamageFeature.Undefined)
                {
                    continue;
                }
                if ((currentDamageFeatures & configuredFeature) == configuredFeature)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
