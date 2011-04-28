﻿using System;
using System.IO;
using System.Security.Cryptography;
using log4net;

namespace UBCopy
{
    class BufferedCopy
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(BufferedCopy));
        private static readonly bool IsDebugEnabled = Log.IsDebugEnabled;

        const FileOptions FileFlagNoBuffering = (FileOptions)0x20000000;

        public static int SyncCopyFileUnbuffered(string inputfile, string outputfile, int buffersize, out byte[] readhash)
        {
            var md5 = MD5.Create();

            if (IsDebugEnabled)
            {
                Log.Debug("Starting SyncCopyFileUnbuffered");
            }

            var buffer = new byte[buffersize];
            FileStream infile;
            FileStream outfile;
            Log.DebugFormat("attempting to lock file {0}", inputfile);
            try
            {
                infile = new FileStream(inputfile,
                                            FileMode.Open, FileAccess.Read, FileShare.None, buffersize,
                                            FileFlagNoBuffering | FileOptions.SequentialScan);

            }
            catch (Exception)
            {
                Log.Debug(inputfile);
                throw;
            }

            try
            {
                outfile = new FileStream(outputfile, FileMode.Create, FileAccess.Write,
                                             FileShare.None, buffersize, FileOptions.WriteThrough);

            }
            catch (Exception)
            {
                Log.Debug(outputfile);
                throw;
            }

            outfile.SetLength(infile.Length);
            try
            {
                int bytesRead;
                while ((bytesRead = infile.Read(buffer, 0, buffer.Length)) != 0)
                {
                    outfile.Write(buffer, 0, bytesRead);
                    if (UBCopySetup.Checksumfiles)
                        md5.TransformBlock(buffer, 0, bytesRead, buffer, 0);

                }
                // For last block:
                if (UBCopySetup.Checksumfiles)
                {
                    md5.TransformFinalBlock(buffer, 0, bytesRead);
                    readhash = md5.Hash;
                }
                else
                {
                    readhash = new byte[1];
                }

            }
            catch (Exception e)
            {
                if (IsDebugEnabled)
                {
                    Log.Debug("Exeption on file copy abort and delete partially copied output.");
                    Log.Debug(e);
                }
                if (File.Exists(outputfile))
                    File.Delete(outputfile);
                readhash = new byte[1];
                return 0;
            }
            finally
            {
                outfile.Close();
                outfile.Dispose();
                infile.Close();
                infile.Dispose();
            }
            Log.InfoFormat("Unbuffered Sync File {0} Done", inputfile);
            if (IsDebugEnabled)
            {
                Log.Debug("Exit SyncCopyFileUnbuffered");
            }

            return 0;
        }
    }
}