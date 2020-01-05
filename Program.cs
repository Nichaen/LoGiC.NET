using System;
using System.IO;
using dnlib.DotNet;
using dnlib.DotNet.Writer;
using LoGiC.NET.Protections;
using SharpConfigParser;
using LoGiC.NET.Utils;
using System.Reflection;


namespace LoGiC.NET
{
    class Program
    {
        public static ModuleDefMD Module { get; set; }

        public static string FileExtension { get; set; }

        public static bool DontRename { get; set; }
        public static bool ForceWinForms { get; set; }
        static string spoof;

        static void Main(string[] args)
        {
            //Console.WriteLine("Drag & drop your file : ");
            //string path = Console.ReadLine();

            Console.WriteLine("Preparing obfuscation...");
            Parser p = new Parser() { ConfigFile = "config.txt" };
            ForceWinForms = bool.Parse(p.Read("ForceWinFormsCompatibility").ReadResponse().ReplaceSpaces());
            DontRename = bool.Parse(p.Read("DontRename").ReadResponse().ReplaceSpaces());

            foreach (var hi in args)
            {
                spoof = hi;
            }
            
            Module = ModuleDefMD.Load(spoof);
            FileExtension = Path.GetExtension(spoof);

            Console.WriteLine("Renaming...");
            Renamer.Execute();

            Console.WriteLine("Adding junk methods...");
            JunkMethods.Execute();

            Console.WriteLine("Adding proxy calls...");
            ProxyAdder.Execute();

            Console.WriteLine("Encoding ints...");
            IntEncoding.Execute();

            Console.WriteLine("Encrypting strings...");
            StringEncryption.Execute();

            Console.WriteLine("Watermarking...");
            Watermark.AddAttribute();

            Console.WriteLine("Saving file...");
            ModuleWriterOptions opts = new ModuleWriterOptions(Module) { Logger = DummyLogger.NoThrowInstance };
            
            var loadasm = Assembly.LoadFrom(spoof);

            var asmanme = loadasm.ManifestModule.Name;

            var path = spoof.Replace(asmanme, @"\Obfuscated\") + asmanme;

            Directory.CreateDirectory(spoof.Replace(asmanme, @"\Obfuscated"));//Create

            Module.Write(path, opts);

            Console.WriteLine("Done! Press any key to exit...");
            Console.ReadKey();
        }
    }
}
