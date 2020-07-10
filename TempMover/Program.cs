using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace ReleaseToSupplier
{
    class Program
    {
        static void Main(string[] args)
        {
            PrintWelcomeMessage();
            ConsoleKeyInfo mainMenuSelection = Console.ReadKey();
            Console.WriteLine();
            if (mainMenuSelection.KeyChar == '0')
            {
                ProcessFiles();
                WaitForRunTime();
            }
            else
            {
                WaitForRunTime();
            }
        }

        private static void ProcessFiles()
        {
            try
            {
                Console.WriteLine("Starting Released > SMTP Copy at " + DateTime.Now);

                var fileCopyCount = 0;
                var fileNotCopiedCount = 0;
                var path = @"\\thas-app01\Catia_System\Released\"; //Move From Path
                var moveLocation = @"\\thas-sftp01\SupplierRelease$\"; //Move To Location

                var filteredFiles = Directory.EnumerateFiles(path)
                    .Where(file =>

                            (
                            file.ToLower().EndsWith("stp") ||
                            file.ToLower().EndsWith("dwg") ||
                            file.ToLower().EndsWith(".pdf")
                            )
                        &&
                        (
                            file.ToLower().Contains("vt") ||
                            file.ToLower().Contains("py") ||
                            file.ToLower().Contains("vs")
                        )
                    )
                    .ToList();

                Console.WriteLine("Filtered File Count: " + filteredFiles.Count);

                Console.WriteLine("Begin file prep...");

                var forEachCounter = 0;

                foreach (var file in filteredFiles)
                {
                    forEachCounter++;
                    if (forEachCounter % 1000 == 0)
                    {
                        Console.WriteLine(forEachCounter + " out of " + filteredFiles.Count + " files proceessed.");
                    }
                    try
                    {
                        var trimmedFileName = Path.GetFileName(file);
                        var fileExt = "";
                        var prog = trimmedFileName.Substring(0, 4);
                        var subF = "";
                        var acceptableFile = false;

                        if (file.ToLower().EndsWith(".stp"))
                        {
                            fileExt = ".stp";
                            subF = "STP";
                        }
                        else if (file.ToLower().EndsWith(").stp"))
                        {
                            fileExt = ").stp";
                            subF = "STP";
                        }
                        else if (file.ToLower().EndsWith(".dwg"))
                        {
                            fileExt = ".dwg";
                            subF = "DWG";
                        }
                        else if (file.ToLower().EndsWith(").dwg"))
                        {
                            fileExt = ").dwg";
                            subF = "DWG";
                        }
                        else if (file.ToLower().EndsWith(".pdf"))
                        {
                            fileExt = ".pdf";
                            subF = "PDF";
                        }
                        else
                        {
                            fileExt = "unknown";
                        }

                        if (prog.ToLower().Contains("vt") || prog.ToLower().Contains("py") || prog.ToLower().Contains("vs"))
                        {
                            acceptableFile = true;
                        }

                        if (fileExt != "unknown" && acceptableFile)
                        {
                            FileInfo file1 = new FileInfo(path + trimmedFileName);
                           
                            if (file1.Exists)
                            {
                                Directory.CreateDirectory(moveLocation + prog + "\\" + subF + "\\");
                                File.Copy(path + trimmedFileName, moveLocation + prog + "\\" + subF + "\\" + trimmedFileName, true);
                                fileCopyCount++;
                            }
                        }
                        else
                        {
                            fileNotCopiedCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        var errorMessage = "Move File Failed. See Error Excepetion: " + ex.Message + ".";
                        Console.WriteLine(errorMessage);
                        fileNotCopiedCount++;
                    }
                }

                SendMail("Successfully completed SMTP copy job at " + DateTime.Now + " - From Report01 task scheduler ReleasedFolderCopy. ", "Files Copied: " + fileCopyCount + ". Errors: " + fileNotCopiedCount +  ".");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Copy Job Failed Badly.." + ex.Message + ex.InnerException);
            }
            
        }

        private static void SendMail(string message, string result)
        {
            try
            {
                string from = "SMTPReleasedCopyJob@thompsonaero.com";
                string to = "sean.kelly@thompsonaero.com";

                using (MailMessage mail = new MailMessage(from, to))
                {
                    mail.Subject = message;
                    mail.Body = result;
                    mail.IsBodyHtml = true;
                    SmtpClient client = new SmtpClient("remote.thompsonaero.com");
                    client.Send(mail);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.InnerException);
            }
        }

        private static void PrintWelcomeMessage()
        {
            Console.WriteLine("");
            Console.WriteLine(" --------------------------------------------- ");
            Console.WriteLine("");
            Console.Title = "SMTP Released Folder Copy - Deployed 10/07/2020 [" + typeof(Program).Assembly.GetName().Version + "]";
            Console.WriteLine(" Welcome to the TAS Connect Health Processor Generator");
            Console.WriteLine(" Version : " + typeof(Program).Assembly.GetName().Version);
            Console.WriteLine("");
            Console.WriteLine(" --------------------------------------------- ");
            Console.WriteLine("");
            Console.WriteLine(" Here are your options...");
            Console.WriteLine("");
            Console.WriteLine(" --------------------------------------------- ");
            Console.WriteLine("");
            Console.WriteLine(" 0. Run Right Now Then Continue As Normal");
            Console.WriteLine("");
            Console.WriteLine(" 1. Run on Auto Timer (7.00AM Run)");
            Console.WriteLine("");
            Console.WriteLine(" --------------------------------------------- ");
            Console.WriteLine("");
        }
        private static void WaitForRunTime()
        {
            TimeSpan _morningTimeToRun = new TimeSpan(07, 00, 00);
            TimeSpan _eveningTimeToRun = new TimeSpan(17, 00, 00);

            while (true)
            {
                TimeSpan timeNow = DateTime.Now.TimeOfDay;

                TimeSpan mornDiff = _morningTimeToRun - DateTime.Now.TimeOfDay;
                TimeSpan noonDiff = _eveningTimeToRun - DateTime.Now.TimeOfDay;

                if (DateTime.Now.TimeOfDay < _morningTimeToRun)
                {
                    Console.WriteLine("morn diff:" + mornDiff.ToString());
                    Console.WriteLine("We will sleep until this time.");
                    System.Threading.Thread.Sleep(mornDiff.Duration());

                    ProcessFiles();
                }
                else if (DateTime.Now.TimeOfDay > _morningTimeToRun && DateTime.Now.TimeOfDay < _eveningTimeToRun)
                {
                    Console.WriteLine("evening diff:" + noonDiff.ToString());
                    Console.WriteLine("We will sleep until this time.");
                    System.Threading.Thread.Sleep(noonDiff.Duration());
                    ProcessFiles();
                }
                else
                {
                    var wait = new TimeSpan(1, 0, 0, 0) - mornDiff.Duration();
                    Console.WriteLine("next day diff:" + wait.ToString());
                    Console.WriteLine("We will sleep until this time.");
                    System.Threading.Thread.Sleep(wait);
                    ProcessFiles();
                }
            }
        }
    }
    

    class fileObject
    {
        public string fileName { get; set; }
        public string trimmedFileName { get; set; }
        public string programName { get; set; }
        public string subFolder { get; set; }
        public DateTime lastModified { get; set; }
    }
}
