using System;
using System.IO;
using Newtonsoft.Json;

namespace OROMDataBinToJson
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Console.Clear();

            if (args.Length <= 0)
                return;

            var fileName = args[0];
            try
            {
                var appPath = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
                if (string.IsNullOrEmpty(appPath))
                    throw new Exception("Failed on try to retrieve application executable directory");
                
                if (!Path.IsPathRooted(fileName))
                    fileName = Path.Combine(appPath, fileName);
                
                var outputFileName = Path.Combine(appPath, "output.json");

                if (args.Length == 2)
                {
                    outputFileName = args[1];
                    if (!Path.IsPathRooted(outputFileName))
                        outputFileName = Path.Combine(appPath, outputFileName);
                }
                
                try
                {
                    using (var fileStream = File.OpenRead(fileName))
                    {
                        try
                        {
                            fileStream.Lock(0, fileStream.Length);
                            var dataScheme = new DataScheme(fileStream);
                            
                            if(File.Exists(outputFileName))
                                File.Delete(outputFileName);
                            
                            File.WriteAllText(outputFileName, JsonConvert.SerializeObject(dataScheme, Formatting.Indented));
                        }
                        finally
                        {
                            fileStream.Unlock(0, fileStream.Length);
                            fileStream.Close();
                        }
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

    }
}