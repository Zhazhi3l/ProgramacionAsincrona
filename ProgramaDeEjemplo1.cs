using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgramacionAsincrona
{
    public class ProgramaDeEjemplo1
    {
        public static async Task Main1()
        {
            string filePath = "example.txt";
            string content = await ReadFileAsync(filePath);
            Console.WriteLine(content);
        }

        public static async Task<string> ReadFileAsync(string filePath)
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                string content = await reader.ReadToEndAsync();
                return content;
            }
        }
    }

    public class Account
    {
        public string Name { get; set; }
        public decimal Balance { get; set; }
    }
}
