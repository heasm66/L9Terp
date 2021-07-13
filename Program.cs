using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace L9Terp
{
    class Program
    {

        static void Main(string[] args)
        {

            /* Open file */
            byte[] infile;
            string fileName = "games\\snapshots\\Emerald Island v2.vsf";
            if (args.Length > 0) fileName = args[0];
            if (System.IO.File.Exists(fileName))
            {
                infile = System.IO.File.ReadAllBytes(fileName);
            }
            else
            {
                Console.WriteLine("File not found!");
                return;
            }

            /* Scanning for game */
            Console.WriteLine("Scanning file...");
            L9Header header = L9V1Games.Scan(infile);
            if (header.offset < 0) header=L9V2Games.Scan(infile);
            if (header.offset>-1)
            {
                Console.WriteLine("Indentified version {0} game: {1}",header.L9Version, header.gamename);
                Console.WriteLine("  Offset      : 0x{0:x4}", header.offset);
                Console.WriteLine("  startmd     : 0x{0:x4}", header.startmd);
                Console.WriteLine("  dictdata    : 0x{0:x4}", header.dictdata);
                Console.WriteLine("  absdatablock: 0x{0:x4}", header.absdatablock);
                Console.WriteLine("  acodeptr    : 0x{0:x4}", header.acodeptr);
                Console.WriteLine("  startabbrev : 0x{0:x4}", header.startabbrev);
                Console.WriteLine("  msgtype     : 0x{0:x4}", header.L9MsgType);
            }
            else
            {
                Console.WriteLine("No game identified in file!");
                return;
            }

            /* Unpack dictionary */
            Console.WriteLine("******************************");
            Console.WriteLine("UNPACK DICTIONARY");
            Console.WriteLine("******************************");
            int i = 0;
            string dictword = "";
            do
            {
                byte currentbyte = infile[header.dictdata + i];
                Boolean eos = false;
                if (currentbyte > 127) eos = true;
                currentbyte = (byte)(currentbyte & 0x7f);
                string currentchar = System.Text.Encoding.ASCII.GetString(new[] { currentbyte });
                if (!IsValidDictionaryChar(currentchar)) break; // Invalid dictionary character, dictionary is finished
                dictword = string.Concat(dictword,currentchar);
                
                if (eos)
                {
                    i++;
                    byte wordvalue = infile[header.dictdata +i];
                    Console.WriteLine("{0:00#}: {1}",wordvalue, dictword);
                    dictword = "";
                }
                i++;
            } while (i < (header.enddictdata - header.dictdata));


            /* Unpack abbreviations, always 162 */
            Console.WriteLine("******************************");
            Console.WriteLine("UNPACK ABBREVIATIONS");
            Console.WriteLine("******************************");
            List<string> abbrevList = new List<string>();
            if (header.L9Version == 1) abbrevList = L9V1Games.UnpackAbbreviations(header, infile);
            if (header.L9Version == 2) abbrevList = L9V2Games.UnpackAbbreviations(header, infile);
            for (i = 0; i < abbrevList.Count; i++) Console.WriteLine(string.Concat(i.ToString("A00#"), ": \"", abbrevList[i], "\""));

            /* Unpack messages */
            Console.WriteLine("******************************");
            Console.WriteLine("UNPACK MESSAGES");
            Console.WriteLine("******************************");
            List<string> msgList = new List<string>();
            if (header.L9Version == 1) msgList = L9V1Games.UnpackMessages(header, abbrevList, infile);
            if (header.L9Version == 2) msgList = L9V2Games.UnpackMessages(header, abbrevList, infile);
            int msgSize = 0;
            for (i = 0; i < msgList.Count; i++)
            {
                msgSize += msgList[i].Length;
                Console.WriteLine(string.Concat(i.ToString("M000#"), ": \"", msgList[i], "\""));
            }

            int memorySize = header.endmd - header.startmd;
            Console.WriteLine("\n\n\nTotal size messages        : 0x{0:x4}", msgSize);
            Console.WriteLine("Packed size in memory      : 0x{0:x4} ({1:N2}%)", memorySize, (100 * (double) memorySize / msgSize));
            Console.WriteLine("   which of, pentad        : {0:N2}% (pp)", 100 - (100 * (double) 5 / 8));
            Console.WriteLine("             abbreviations :  {0:N2}% (pp)", 100 * ((double)5 / 8 - (double) memorySize / msgSize));


            Console.ReadKey();
        }

        private static Boolean IsValidDictionaryChar(string str)
        {
            if ("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ?-'/!.,".Contains(str)) return true;
            return false;
        }

    }
}

