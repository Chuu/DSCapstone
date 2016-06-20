using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;

namespace DataSet2Vec
{
    class DataSet2Vec
    {
        static Dictionary<string, int> uniqueStrings = new Dictionary<string, int>();
        static Dictionary<string, int> stringToIndex = new Dictionary<string, int>();
        
        static void Main(string[] args)
        {
            var filename = ConfigurationManager.AppSettings["dataset"];
            var outputDirectory = ConfigurationManager.AppSettings["outputFolder"];
            var fixLineEnding = new Func<string, string>(str =>
            {
                if (!new List<char>() { '.', '!', '?' }.Any(x => { return str[str.Length - 1] == x; }))
                {
                    return str + ".";
                }
                return str;
            });

            var stringPreprocessor = fixLineEnding;


            if(filename == null)
            {
                Console.WriteLine("Cannot find setting \"dataset\" in appconfig");
                return;
            }

            if (!File.Exists(filename))
            {
                Console.WriteLine("Cannot find file: " + filename);
                return;
            }

            if (!Directory.Exists(outputDirectory))
            {
                Console.WriteLine("Cannot find output directory: " + outputDirectory);
                return;
            }

            using (var reader = new System.IO.StreamReader(filename))
            {
                string line;
                while((line = reader.ReadLine()) != null)
                {
                    ProcessLine(stringPreprocessor(line));
                }
            }
            
            //output vector
            using (var writer = new System.IO.StreamWriter(outputDirectory + Path.DirectorySeparatorChar + "wordVector.csv"))
            {
                var sortedUniqueStrings = uniqueStrings.ToArray();
                Array.Sort(sortedUniqueStrings, (x, y) => { return x.Value.CompareTo(y.Value); });

                writer.WriteLine("index" + '\u0001' + "string" + '\u0001' + "count");
                for (int i = 0; i < sortedUniqueStrings.Length; ++i)
                {
                    writer.WriteLine(i + '\u0001' + sortedUniqueStrings[i].Key + '\u0001' + sortedUniqueStrings[i].Value);
                    stringToIndex[sortedUniqueStrings[i].Key] = i;
                }
            }

            //create one giant vector to train
            using (var reader = new System.IO.StreamReader(filename))
            {
                using (var writer = new System.IO.StreamWriter(outputDirectory + Path.DirectorySeparatorChar + "trainingVector.data"))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] tokens = stringPreprocessor(line).Split(new char[] { ' ' });
                        foreach(var token in tokens)
                        {
                            writer.Write(stringToIndex[token] + ',');
                        }
                    }
                }
            }
        }

        static void ProcessLine(string line)
        {
            //We're going to consider anything split with a space a word.  So we're going to get
            //a lot of junk like x, -- but oh well that's the breaks
            string[] tokens = line.Split(new char[] { ' ' });

            foreach(var token in tokens)
            {
                if (uniqueStrings.ContainsKey(token))
                {
                    uniqueStrings[token] = uniqueStrings[token] + 1;
                }
                else
                {
                    uniqueStrings.Add(token, 1);
                }
            }
        }
    }
}
