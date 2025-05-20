using System.Collections.Generic;
using System.Diagnostics;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using SimpleJSON;
using UnityEngine;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Linq;

namespace BaseMod
{
    public static class WsClient
    {
        private const string ApiVersion = "1";
        private static ClientWebSocket _ws;
        private static readonly Uri ServerUri = new Uri($"ws://127.0.0.1:60536/{ApiVersion}");//这里填写自己的手机ip地址
        private static readonly ArraySegment<byte> Buffer = new ArraySegment<byte>(new byte[4096]);

        public static async void Send(string json)
        {
            try
            {
                if (_ws?.State != WebSocketState.Open)
                {
                    _ws = new ClientWebSocket();
                    await _ws.ConnectAsync(ServerUri, CancellationToken.None);
                }

                var data = Encoding.UTF8.GetBytes(json);
                await _ws.SendAsync(
                    new ArraySegment<byte>(data),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine($"WebSocket Error: {e.Message}");
                _ws?.Dispose();
                _ws = null;
            }
        }
    }

    [BepInPlugin(Guid, Name, Version)]
    public class Main : BasePlugin
    {
        public const string Guid = Author + "." + Name;
        public const string Name = "DG-LAB MOD";
        public const string Version = "0.0.1";
        public const string Author = "Tintagedfish";
        private static float CalculateIntensity(float damage)
        {
            damage = Mathf.Max(damage, 0f);
            float a = 0.574f;
            float b = 2.876f;
            float intensity = a * damage + b * Mathf.Log(damage + 1);
            return Mathf.Min(intensity, 40f);
        }

        private static float CalculateDuration(float damage)
        {
            damage = Mathf.Max(damage, 0f);
            float duration = (1.5f / Mathf.Log(51f)) * Mathf.Log(damage + 1);
            return Mathf.Min(duration, 1.5f);
        }
        private static string ExtractDigits(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            return new string(input.Where(char.IsDigit).ToArray());
        }

        private static bool IsValidFiveDigitFormat(string digits)//默认万分位为1，千分位与百分位组成的数字在1~12的外观ID是罪人的ID
        {
            if (digits.Length != 5) return false;
            if (digits[0] != '1') return false;
            if (!int.TryParse(digits.Substring(1, 2), out int num) || num < 1 || num > 12)
                return false;
            return true;
        }

        private static readonly HashSet<string> WhiteList = new HashSet<string>//白名单，可以让除普通罪人外观之外的任意ID外观受伤后触发通信
        {
            "1234",
            "99999",
            "11234"
        };


        public override void Load()
        {
            // 打印日志信息
            Log.LogInfo("Hello LimbusConpanay!!!This is DG-LAB MOD!!!");
            Log.LogWarning("This is Warning!!!From DG-LAB MOD.");
            Harmony.CreateAndPatchAll(typeof(Main));
        }
        [HarmonyPostfix, HarmonyPatch(typeof(BattleEffectManager), "OnDamageEffect_Base")]
        public static void BattleEffectManager_OnDamageEffect_Base(BattleEffectManager __instance, float damage, ATK_BEHAVIOUR attackType, string appearanceId, HashSet<UNIT_KEYWORD> keyword)
        {
            float intensity = CalculateIntensity(damage);
            float duration = CalculateDuration(damage);
            int ticks = Mathf.FloorToInt(duration * 10f);
            ticks = Mathf.Clamp(ticks, 1, 10); // 持续时间范围0.1~1.0秒

            List<object> patternUnits = new List<object>();
            switch (attackType)
            {
                case ATK_BEHAVIOUR.SLASH: // 斩击类型
                    patternUnits.Add(new { pattern_intensity = 30, frequency = 80 });
                    patternUnits.Add(new { pattern_intensity = 60, frequency = 100 });
                    patternUnits.Add(new { pattern_intensity = 100, frequency = 120 });
                    break;

                case ATK_BEHAVIOUR.PENETRATE: // 穿刺类型
                    patternUnits.Add(new { pattern_intensity = 100, frequency = 150 });
                    patternUnits.Add(new { pattern_intensity = 40, frequency = 80 });
                    break;

                case ATK_BEHAVIOUR.HIT: // 打击类型
                default:
                    for (int i = 0; i < 3; i++)
                    {
                        patternUnits.Add(new
                        {
                            pattern_intensity = 80 + i * 10,
                            frequency = 100 - i * 20
                        });
                    }
                    break;
            }

            // 构造JSON消息
            var jsonBuilder = new System.Text.StringBuilder();
            jsonBuilder.Append("{");
            jsonBuilder.Append("\"cmd\":\"set_pattern\",");

            // 构建pattern_units数组
            jsonBuilder.Append("\"pattern_units\":[");
            for (int i = 0; i < patternUnits.Count; i++)
            {
                dynamic unit = patternUnits[i]; // 使用动态类型访问匿名对象
                jsonBuilder.Append("{");
                jsonBuilder.Append($"\"pattern_intensity\":{unit.pattern_intensity},");
                jsonBuilder.Append($"\"frequency\":{unit.frequency}");
                jsonBuilder.Append("}");
                if (i != patternUnits.Count - 1) jsonBuilder.Append(",");
            }
            jsonBuilder.Append("],");

            // 添加其他字段
            jsonBuilder.Append($"\"intensity\":{Mathf.RoundToInt(intensity * 1.0f)},");
            jsonBuilder.Append($"\"ticks\":{ticks}");
            jsonBuilder.Append("}");

            // 发送JSON数据
            string digits = ExtractDigits(appearanceId);
            if (WhiteList.Contains(digits) || IsValidFiveDigitFormat(digits))
            {
                 WsClient.Send(jsonBuilder.ToString()); // 发送波形
                 // 调试输出
                 string log = $"Damage: {damage} | Intensity: {intensity:F1} | Duration: {duration:F1}s";
                 System.Diagnostics.Debug.WriteLine(log);
            }
            // 调试输出
            string logMessage = $"dealDamage {damage} to {appearanceId}####attackType:{attackType}";
            System.Diagnostics.Debug.WriteLine($"####{logMessage}####");
        }
    }
}