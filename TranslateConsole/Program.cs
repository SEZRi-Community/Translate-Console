using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace TranslateConsole
{
    internal class Program
    {
        private static string googleTranslateApiUrl;

        static void Main(string[] args)
        {
            if (!LoadApiConfig())
            {
                Console.WriteLine("Error: config.json is missing or API is not configured correctly in config.json.");
                return;
            }

            while (true)
            {
                Console.Write("> ");
                string input = Console.ReadLine();

                if (input.ToLower() == "exit")
                    break;

                if (input.ToLower() == "translate help")
                {
                    ShowHelp();
                    continue;
                }

                if (input.ToLower().StartsWith("translate "))
                {
                    string[] parts = input.Split(new[] { ' ' }, 3, StringSplitOptions.RemoveEmptyEntries);

                    if (parts.Length != 3 && parts.Length != 4)
                    {
                        Console.WriteLine("Syntax error: The value entered is in the wrong format.");
                        continue;
                    }

                    string targetLang = parts[1];
                    string textToTranslate = Uri.EscapeDataString(parts[2].Trim('"'));

                    if (parts[0] == "translate" && parts[1] == "json")
                    {
                        // json コマンドの場合、parts[2] で言語コードを受け取る
                        string targetLangForJson = parts[2].Split(' ')[0];
                        string textToTranslateForJson = Uri.EscapeDataString(parts[2].Substring(targetLangForJson.Length).Trim());

                        // 翻訳開始アニメーション
                        var translateTask = TranslateTextWithAnimationAsync(textToTranslateForJson, targetLangForJson);

                        // JSON 出力
                        var jsonOutput = new JObject
                        {
                            ["before"] = Uri.UnescapeDataString(textToTranslateForJson),
                            ["after"] = translateTask.Result,
                            ["language"] = targetLangForJson
                        };

                        // JSON ファイル名の作成
                        string filePath = "output.json";
                        int fileCount = 1;
                        while (File.Exists(filePath))
                        {
                            filePath = $"output({fileCount}).json";
                            fileCount++;
                        }

                        // JSON を表示
                        Console.WriteLine("> [Before]\n>  " + Uri.UnescapeDataString(textToTranslateForJson));
                        Console.WriteLine(">  ↓\n> [After: " + targetLangForJson + "]\n> " + translateTask.Result);
                        Console.WriteLine(); // 一行空ける

                        // JSON ファイルを保存
                        string formattedJson = jsonOutput.ToString().Replace("\\\"", "\"");
                        File.WriteAllText(filePath, formattedJson);
                        Console.WriteLine($"The translation result has been saved to {filePath} .");
                    }
                    else
                    {
                        var translateTask = TranslateTextWithAnimationAsync(textToTranslate, targetLang);

                        // 結果が出るまで待つ
                        var translatedText = translateTask.Result;

                        // 出力の順序を変更: Before → After の順で表示
                        Console.WriteLine("> [Before]\n>  " + Uri.UnescapeDataString(textToTranslate));
                        Console.WriteLine(">  ↓\n> [After: " + targetLang + "]\n> " + translatedText);
                        Console.WriteLine(); // 一行空ける
                    }
                }
                else
                {
                    Console.WriteLine("Syntax error: Command must begin with 'translate'.");
                }
            }
        }

        static async Task<string> TranslateTextWithAnimationAsync(string text, string targetLang)
        {
            // アニメーションを表示しながら翻訳処理を実行
            var animationTask = ShowAnimationAsync();

            // 翻訳を非同期に実行
            string translatedText = await TranslateTextAsync(text, targetLang);

            // アニメーションを終了させる
            await animationTask;

            return translatedText;
        }

        static async Task ShowAnimationAsync()
        {
            // アニメーションのキャラクター配列
            char[] animationChars = new char[] { '|', '\\', '/', '*' };
            int index = 0;
            DateTime startTime = DateTime.Now;
            while ((DateTime.Now - startTime).TotalSeconds < 3)  // アニメーション表示時間 (例: 2秒)
            {
                Console.Write($" {animationChars[index]} \r");
                await Task.Delay(100); // 0.1秒待機
                index = (index + 1) % animationChars.Length;
            }
            Console.Write(" \r");  // アニメーションが終了したら、表示を空にする
        }

        static async Task<string> TranslateTextAsync(string text, string targetLang)
        {
            using (var client = new HttpClient())
            {
                string requestUrl = $"{googleTranslateApiUrl}?text={text}&to={targetLang}";
                HttpResponseMessage response = await client.GetAsync(requestUrl);
                string responseBody = await response.Content.ReadAsStringAsync();

                var result = JObject.Parse(responseBody);
                if ((int)result["code"] != 200)
                {
                    Console.WriteLine("Error: " + result["msg"]);
                    return "";
                }
                return result["text"].ToString();
            }
        }

        static void ShowHelp()
        {
            Console.WriteLine(" Don't know how to use it?");
            Console.WriteLine(" Please check the official Translate Console documentation:");
            Console.WriteLine(" https://github.com/SEZRi-Community/Translate-Console");
        }

        static bool LoadApiConfig()
        {
            try
            {
                if (!File.Exists("config.json"))
                {
                    Console.WriteLine("Error: config.json not found.");
                    return false;
                }

                var configText = File.ReadAllText("config.json");
                var configJson = JObject.Parse(configText);

                if (configJson["api"] == null)
                {
                    Console.WriteLine("Error: config.json does not contain API.");
                    return false;
                }

                googleTranslateApiUrl = configJson["api"].ToString();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: Failed to read internal configuration file." + ex.Message);
                return false;
            }
        }
    }
    } 
