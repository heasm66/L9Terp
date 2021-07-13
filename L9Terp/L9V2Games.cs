using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace L9Terp
{
    class L9V2Games
    {
        public static L9Header Scan(byte[] infile)
        {
            L9Header header = new L9Header
            {
                offset = -1
            };

            //Search for AGAIN - Start of dictdata
            int dictdata = -1;
            for (int i = 0; i < infile.Length-5; i++) 
            {
                if ((infile[i] & 0x7f)==0x41)
                {
                    if ((infile[i+1] & 0x7f) == 0x47 && (infile[i + 2] & 0x7f) == 0x41 && (infile[i + 3] & 0x7f) == 0x49 && (infile[i + 4] & 0x7f) == 0x4E) 
                    {
                        dictdata = i;
                        Console.WriteLine("Found dictdata V2 at 0x{0:x4}", i);
                        break;
                    }
                }
            }


            //Search for header-reference to dictdata
            for (int i = 0; i < dictdata; i++)
            {
                byte bigbyte = (byte)((dictdata-i+6) / 256);
                byte littlebyte = (byte)((dictdata-i+6) - bigbyte * 256);
                if (infile[i+1] == bigbyte && infile[i] == littlebyte)
                {
                    Console.WriteLine("Found potential reference to dictdata at 0x{0:x4}", i-6);
                        
                    //Check absdatablock (always 0x0020?)
                    if (infile[i - 1] == 0x00 && infile[i-2] == 0x20)
                    {
                        header.offset = i - 6;
                        header.dictdata = dictdata;
                        header.absdatablock = header.offset + 0x0020;
                        header.startmd = header.offset + infile[i-5]*256+ infile[i - 6];
                        header.startabbrev = header.offset + infile[i - 3] * 256 + infile[i - 4];
                        header.acodeptr = header.offset + infile[i +21] * 256 + infile[i +20];
                        header.gamename = "Version 2 game";
                        header.enddictdata = header.startmd;
                        header.endmd = header.startabbrev;
                        header.L9MsgType = DetermineMsgType(header, infile);
                        header.L9Version = 2;
                        return header;
                    }
                }
            }

            return header;
        }

        private static int DetermineMsgType(L9Header header,byte[] infile)
        {
            //Crude way. Count EOS chars. If there's too few, assume type 2
            int eosCount = 0;
            for (int i=0; i<256; i++) if (infile[header.startabbrev+i] == 0x01) eosCount++;

            if (eosCount < 10) return 2;        
            return 1;
        }

        public static List<string> UnpackAbbreviations(L9Header header, byte[] infile)
        {
            if (header.L9MsgType == 1) return L9V1Games.UnpackAbbreviations(header, infile);

            int i = 0;
            string abbr = "";
            int abbrNumber = 1;
            List<string> abbrList = new List<string>(new string[] { });
            abbrList.Add(""); // Numbering start at 1
            do
            {
                //abbrev actually start 1 byte back from header info
                byte currentByte = infile[header.startabbrev + i - 1];
                int length = 0;
                do
                {
                    length += currentByte;
                    if (currentByte == 0)
                    {
                        length += 255;
                        i++;
                        currentByte = infile[header.startmd + i];
                    }
                    else break;


                } while (true);

                for (int k=1;k<length;k++)
                {
                    i++;
                    currentByte = infile[header.startabbrev + i-1];

                    if (currentByte < 3) break; //eos? break anyway

                    Boolean insertAbbr = false;
                    if (currentByte > 0x5d) insertAbbr = true;

                    if (insertAbbr)
                    {
                        currentByte -= 0x5d;
                        if (currentByte < abbrNumber)
                        {
                            abbr = string.Concat(abbr, abbrList[currentByte]);
                        }
                        else
                        {
                            abbr = string.Concat(abbr, "[A", currentByte.ToString("000#"), "]");
                        }
                    }
                    else
                    {
                        currentByte += 0x1d;
                        abbr = string.Concat(abbr, L9V1Games.ByteToChar(currentByte));
                    }

                }

                abbrList.Add(abbr);
                for (int j = 0; j < abbrList.Count; j++)
                {
                    abbrList[j] = abbrList[j].Replace(string.Concat("[A", abbrNumber.ToString("000#"), "]"), abbr);
                }
                abbr = "";
                abbrNumber++;
 
                i++;
            } while (abbrNumber <= 162);

            header.endabbrev = header.startabbrev + i - 1;
            return abbrList;
        }

        public static List<string> UnpackMessages(L9Header header, List<string> abbrList, byte[] infile)
        {
            if (header.L9MsgType == 1) return L9V1Games.UnpackMessages(header, abbrList, infile);

            int i = 0;
            string msg = "";
            int msgNumber = 1;
            List<string> msgList = new List<string>(new string[] { });
            msgList.Add(""); // Message numbering start at 1 
            do
            {
                byte currentByte = infile[header.startmd + i];
                int length = 0;
                do
                {
                    length += currentByte;
                    if (currentByte == 0)
                    {
                        length += 255;
                        i++;
                        currentByte = infile[header.startmd + i];
                    }
                    else break;

                } while (true);

                for (int k = 1; k < length; k++)
                {
                    i++;
                    currentByte = infile[header.startmd + i];

                    if (currentByte < 3) break; //eos? break anyway

                    Boolean insertAbbrev = false;
                    if (currentByte > 0x5d) insertAbbrev = true;

                    if (insertAbbrev)
                    {
                        currentByte -= 0x5d;
                        msg = string.Concat(msg, abbrList[currentByte]);
                    }
                    else
                    {
                        currentByte += 0x1d;
                        msg = string.Concat(msg, L9V1Games.ByteToChar(currentByte));
                    }

                }

                msgList.Add(msg);
                msg = "";
                msgNumber++;

                i++;
            } while (i < (header.endmd - header.startmd));

            return msgList;
        }

    }
}
