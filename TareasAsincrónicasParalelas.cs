using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgramacionAsincrona
{
    internal class TareasAsincrónicasParalelas
    {
        public static void HandleFour()
        {
            var task = Task.Run(
                () => throw new CustomException("This exception is expected!"));

            try
            {
                task.Wait();
            }
            catch (AggregateException ae)
            {
                ae.Handle(ex =>
                {
                    // Handle the custom exception.
                    if (ex is CustomException)
                    {
                        Console.WriteLine(ex.Message);
                        return true;
                    }
                    // Rethrow any other exception.
                    return false;
                });
            }
        }
        public static partial class Program
        {
            public static void Main()
            {
                HandleThree();
            }

            public static void HandleThree()
            {
                var task = Task.Run(
                    () => throw new CustomException("This exception is expected!"));

                try
                {
                    task.Wait();
                }
                catch (AggregateException ae)
                {
                    foreach (var ex in ae.InnerExceptions)
                    {
                        // Handle the custom exception.
                        if (ex is CustomException)
                        {
                            Console.WriteLine(ex.Message);
                        }
                        // Rethrow any other exception.
                        else
                        {
                            throw ex;
                        }
                    }
                }
            }
        }

        // Define the CustomException class
        public class CustomException : Exception
        {
            public CustomException(string message) : base(message) { }
        }

    }
}
