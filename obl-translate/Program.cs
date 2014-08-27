using Google.API.Translate;
using Google.Apis.Services;
using Google.Apis.Translate.v2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace obl_translate
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach(var file in Directory.GetFiles("../../../data/contents", "*.txt"))
            {
                string text = System.IO.File.ReadAllText(file);

                var translated = "";

                TranslateClient client = new TranslateClient("http://go.ru");
                translated = client.Translate(text, "ru", "en");//Language.ChineseSimplified, Language.English);
                Console.WriteLine(translated);
                /*
                var service = new TranslateService(new BaseClientService.Initializer
                {
                    ApplicationName = "Translator",
                    ApiKey = "AIzaSyDMIWBKKaPqzCDczTwaMJWXtJx76PICTeA"
                });

                var listRequest = service.Translations.List(new Google.Apis.Util.Repeatable<string>(new[] { text }), "en");

                listRequest.Source = "ru";
                var response = listRequest.Execute();                

                if (translated != null && translated.Length != 0)
                {
                    var translatedText = response.Translations.FirstOrDefault().TranslatedText;

                    System.IO.File.WriteAllText("../../../data/contents-en/" + file, translatedText);
                }
                 */
            }
        }
    }
}
