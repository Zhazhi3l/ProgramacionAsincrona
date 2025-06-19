using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Diagnostics;

namespace AsynchronousExamples
{
    public class Account
    {
        public string Name { get; set; }
        public decimal Balance { get; set; }
    }

    public class Pet
    {
        public long id { get; set; }
        public string name { get; set; }
        public Category category { get; set; }
        public List<string> photoUrls { get; set; }
        public List<Tag> tags { get; set; }
        public string status { get; set; }
    }

    public class Category
    {
        public long id { get; set; }
        public string name { get; set; }
    }

    public class Tag
    {
        public long id { get; set; }
        public string name { get; set; }
    }

    public class CustomException : Exception
    {
        public CustomException(string message) : base(message) { }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            // Descomenta el método que quieras ejecutar:

            // await Example_ReadFileAsync();
            // await Example_FileIOAsync();
            // await Example_HttpClientAsync();
            // Example_ConcurrentBagExample();
            // await Example_TaskWhenAllAsync();
            // Example_TraverseTreeParallelForEach();
            // Example_HandleThree();
            // Example_HandleFour();
        }

        // 1. Leer un archivo de texto asíncronamente
        public static async Task Example_ReadFileAsync()
        {
            string filePath = "example.txt";
            string content = await ReadFileAsync(filePath);
            Console.WriteLine(content);
        }

        public static async Task<string> ReadFileAsync(string filePath)
        {
            using var reader = new StreamReader(filePath);
            return await reader.ReadToEndAsync();
        }

        // 2. Serializar/Deserializar un objeto a JSON y guardarlo en un archivo
        public static async Task Example_FileIOAsync()
        {
            string dir = Path.Combine(Environment.CurrentDirectory, "TempDir");
            Directory.CreateDirectory(dir);

            string filePath = Path.Combine(dir, "account.json");
            var account = new Account { Name = "Elize Harmsen", Balance = 1000.00m };

            await SaveAccountDataAsync(filePath, account);
            var loaded = await LoadAccountDataAsync(filePath);
            Console.WriteLine($"Name: {loaded.Name}, Balance: {loaded.Balance}");
        }

        public static async Task SaveAccountDataAsync(string filePath, Account account)
        {
            string json = JsonSerializer.Serialize(account);
            await File.WriteAllTextAsync(filePath, json);
        }

        public static async Task<Account> LoadAccountDataAsync(string filePath)
        {
            string json = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<Account>(json);
        }

        // 3. Acceder a una API REST asíncronamente
        public static async Task Example_HttpClientAsync()
        {
            using var client = new HttpClient();
            try
            {
                string url = "https://petstore.swagger.io/v2/pet/findByStatus?status=available";
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string body = await response.Content.ReadAsStringAsync();
                var pets = JsonSerializer.Deserialize<List<Pet>>(body);

                foreach (var pet in pets)
                {
                    if (pet.id.ToString().Length > 4)
                        Console.WriteLine($"Pet ID: {pet.id}, Name: {pet.name}");
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }
        }

        // 4. Parallel.For con ConcurrentBag
        public static void Example_ConcurrentBagExample()
        {
            var bag = new ConcurrentBag<int>();
            Parallel.For(0, 100, i =>
            {
                Task.Delay(100).Wait();
                bag.Add(i);
            });
            Console.WriteLine($"Processed {bag.Count} items in parallel.");
        }

        // 5. Task.WhenAll para ejecutar múltiples tareas
        public static async Task Example_TaskWhenAllAsync()
        {
            var urls = new List<string>
            {
                "https://example.com",
                "https://example.org",
                "https://example.net"
            };

            var tasks = new List<Task<string>>();
            foreach (var url in urls)
                tasks.Add(FetchDataAsync(url));

            var results = await Task.WhenAll(tasks);
            foreach (var r in results)
                Console.WriteLine(r);
        }

        static async Task<string> FetchDataAsync(string url)
        {
            using var client = new HttpClient();
            return await client.GetStringAsync(url);
        }

        // 6. Recorrer archivos en paralelo con control de errores
        public static void Example_TraverseTreeParallelForEach()
        {
            try
            {
                TraverseTreeParallelForEach(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), f => { _ = File.ReadAllBytes(f); Console.WriteLine(f); });
            }
            catch (ArgumentException)
            {
                Console.WriteLine("El directorio no existe.");
            }

            void TraverseTreeParallelForEach(string root, Action<string> action)
            {
                int fileCount = 0;
                var sw = Stopwatch.StartNew();
                int procCount = Environment.ProcessorCount;
                var dirs = new Stack<string>();
                if (!Directory.Exists(root)) throw new ArgumentException(nameof(root));
                dirs.Push(root);

                while (dirs.Count > 0)
                {
                    var current = dirs.Pop();
                    string[] subDirs = Array.Empty<string>(), files = Array.Empty<string>();
                    try { subDirs = Directory.GetDirectories(current); } catch { continue; }
                    try { files = Directory.GetFiles(current); } catch { continue; }

                    try
                    {
                        if (files.Length < procCount)
                        {
                            foreach (var file in files) { action(file); fileCount++; }
                        }
                        else
                        {
                            Parallel.ForEach(files, () => 0,
                                (file, state, local) => { action(file); return local + 1; },
                                final => Interlocked.Add(ref fileCount, final));
                        }
                    }
                    catch (AggregateException ae)
                    {
                        ae.Handle(ex => { Console.WriteLine(ex.Message); return true; });
                    }

                    foreach (var d in subDirs) dirs.Push(d);
                }

                Console.WriteLine($"Processed {fileCount} files in {sw.ElapsedMilliseconds} ms");
            }
        }

        // 7. Manejo de excepciones con Task.Wait y AggregateException
        public static void Example_HandleThree()
        {
            var task = Task.Run(() => throw new CustomException("This exception is expected!"));
            try
            {
                task.Wait();
            }
            catch (AggregateException ae)
            {
                foreach (var ex in ae.InnerExceptions)
                {
                    if (ex is CustomException ce)
                        Console.WriteLine(ce.Message);
                    else
                        throw ex;
                }
            }
        }

        public static void Example_HandleFour()
        {
            var task = Task.Run(() => throw new CustomException("This exception is expected!"));
            try
            {
                task.Wait();
            }
            catch (AggregateException ae)
            {
                ae.Handle(ex =>
                {
                    if (ex is CustomException ce)
                    {
                        Console.WriteLine(ce.Message);
                        return true;
                    }
                    return false;
                });
            }
        }
    }
}