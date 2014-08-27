using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace obl_alchemy
{
    class Program
    {
        static void Main(string[] args)
        {
            var api = new AlchemyAPI.AlchemyAPI();
            api.SetAPIHost("access");
            api.SetAPIKey("a68961f0b038d11cda86b0105009399b782c67ab");

            foreach (var file in Directory.GetFiles("../../../data/contents-en", "*.txt"))
            {
                var outFileName = Path.GetFileName(file);
                var outFilePath = Path.Combine("../../../data/alchemy-concepts/", outFileName);

                if (!File.Exists(outFilePath))
                {                    
                    string text = System.IO.File.ReadAllText(file);
                    if (!string.IsNullOrEmpty(text))
                    {
                        var res = api.TextGetRankedConcepts(text);
                        var doc = new XmlDocument();
                        doc.LoadXml(res);
                        var r = doc.SelectNodes("results/concepts/concept/text").Cast<XmlNode>().Select(x => x.InnerText);
                        var j = string.Join(",", r);
                        System.IO.File.WriteAllText(outFilePath, j);
                    }
                }


            }
        }
    }
}
