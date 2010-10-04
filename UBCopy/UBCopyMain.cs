﻿using System;
using System.Diagnostics;
using System.IO;
using NDesk.Options;

namespace UBCopy
{
    class UBCopyMain
    {
        //hold command line options
        private static string _sourcefile;
        private static string _destinationfile;
        private static bool _overwritedestination;
        //we set an inital buffer size to be on the safe side.
        private static int _buffersize = 16;
        private static bool _checksumfiles;
        private static bool _reportprogres;

        private static int Main(string[] args)
        {
            Console.WriteLine("UBCopy 1.50");
            int parseerr = ParseCommandLine(args);
            if (parseerr == 1)
            {
#if DEBUG
                Console.ReadKey();
#endif
                return 0;
            }
            try
            {
                var f = new FileInfo(_sourcefile);
                long s1 = f.Length;

                var sw = new Stopwatch();
                sw.Start();
                AsyncUnbuffCopy.AsyncCopyFileUnbuffered(_sourcefile, _destinationfile, _overwritedestination, _checksumfiles, _buffersize, _reportprogres);
                sw.Stop();

                Debug.WriteLine(sw.ElapsedMilliseconds);
                Debug.WriteLine(s1 / (float)sw.ElapsedMilliseconds / 1000.00);

                Console.WriteLine("Elapsed Seconds: {0}", sw.ElapsedMilliseconds / 1000.00);
                Console.WriteLine("Megabytes/sec  : {0}", Math.Round(s1 / (float)sw.ElapsedMilliseconds / 1000.00, 2));


                Console.WriteLine("Done.");
                Console.ReadKey();

#if DEBUG
                Console.ReadKey();
#endif
                return 1;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: File copy aborted");
                Console.WriteLine(e.Message);
                return 0;
            }

        }

        static public int ParseCommandLine(string[] args)
        {
            bool showHelp = false;

            var p = new OptionSet
                        {
                          { "s:|sourcefile:", "The file you wish to copy",
                          v => _sourcefile = v },

                          { "d:|destinationfile:", "The target file you wish to write",
                          v => _destinationfile = v},

                          { "o:|overwritedestination:", "True if you want to overwrite the destination file if it exists",
                          (bool v) => _overwritedestination = v},

                          { "c:|checksum:", "True if you want use SHA1 to verify the destination file is the same as the source file",
                          (bool v) => _checksumfiles = v},

                          { "b:|buffersize:", "size in Megabytes, maximum of 32",
                          (int v) => _buffersize = v},

                          { "r:|reportprogress:", "True give a visual indicator of the copy progress",
                          (bool v) => _reportprogres = v},

                          { "?|h|help",  "show this message and exit", 
                          v => showHelp = v != null },
                        };

            try
            {
                p.Parse(args);
            }

            catch (OptionException e)
            {
                Console.Write("UBCopy Error: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `UBCopy --help' for more information.");
                return 1;
            }

            if (args.Length == 0)
            {
                ShowHelp("Error: please specify some commands....", p);
                return 1;
            }

            if (_sourcefile == null || _destinationfile == null && !showHelp)
            {
                ShowHelp("Error: You must specify a file to copy (-s) and a file to copy to (-d).", p);
                return 1;
            }

            if (showHelp)
            {
                ShowHelp(p);
                return 1;
            }
            return 0;
        }

        static void ShowHelp(string message, OptionSet p)
        {
            Console.WriteLine(message);
            Console.WriteLine("Usage: UBCopy [OPTIONS]");
            Console.WriteLine("copy files using unbuffered IO and asyncronus buffers");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }

        static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: UBCopy [OPTIONS]");
            Console.WriteLine("copy files using unbuffered IO and asyncronus buffers");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }
    }
}
