using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace L9Terp
{
    class L9V1GameData
    {
        public string name;
        public int acodeptr;
        public int dictdata;
        public byte[] acodestart_footprint;
        public int absdatablock;
        public int startmd;
        public int startabbrev;
        public int startfile;
        public int enddictdata;
        public int endmd;
    }

    class L9V1Games
    {
        public List<L9V1GameData> gamesList;

        public L9V1Games()
        {
            // Build a list of version 1 games with relative addresses from 
            // the A-code start (defined by the footprint).
            gamesList = new List<L9V1GameData>();

            L9V1GameData game = new L9V1GameData
            {
                name = "Colossal Cave Adventure",
                acodestart_footprint = new byte[] { 0x20, 0x04, 0x00, 0x49, 0x00, 0x06, 0x05, 0x48, 0x01, 0x01, 0x48, 0x02, 0x02, 0x48, 0xff, 0x03 },
                acodeptr = 0x0000,
                dictdata = -0x0760,
                absdatablock = -0x03b0,
                startmd = 0x0f80,
                startabbrev = 0x57d7
            };
            game.startfile = game.dictdata;
            game.enddictdata = game.absdatablock;
            game.endmd = game.startabbrev;
            gamesList.Add(game);

            game = new L9V1GameData
            {
                name = "Adventure Quest",
                acodestart_footprint = new byte[] { 0x00, 0x06, 0x00, 0x00, 0x46, 0x00, 0x06, 0x05, 0x48, 0x01, 0x01, 0x48, 0x02, 0x02, 0x48, 0x03 },
                acodeptr = 0x0000,
                dictdata = -0x04c8,
                absdatablock = -0x0800,
                startmd = 0x1000,
                startabbrev = 0x49d1
            };
            game.startfile = game.absdatablock;
            game.enddictdata = game.acodeptr;
            game.endmd = game.startabbrev;
            gamesList.Add(game);

            game = new L9V1GameData
            {
                name = "Dungeon Adventure",
                acodestart_footprint = new byte[] { 0x00, 0x06, 0x00, 0x00, 0x44, 0x01, 0x06, 0x05, 0x48, 0x01, 0x01, 0x48, 0x02, 0x02, 0x48, 0x03 },
                acodeptr = 0x0000,
                dictdata = -0x0740,
                absdatablock = -0x0a20,
                startmd = 0x16bf,
                startabbrev = 0x58cc
            };
            game.startfile = game.absdatablock;
            game.enddictdata = game.acodeptr;
            game.endmd = game.startabbrev;
            gamesList.Add(game);

            game = new L9V1GameData
            {
                name = "Lords of Time",
                acodestart_footprint = new byte[] { 0x00, 0x06, 0x00, 0x00, 0x65, 0x01, 0x45, 0xa0, 0x08, 0x0f, 0x01, 0x01, 0x48, 0x00, 0x02, 0x48 },
                acodeptr = 0x0000,
                dictdata = -0x4a00,
                absdatablock = -0x4120,
                startmd = -0x3b9d,
                startabbrev = -0x0215
            };
            game.startfile = game.dictdata;
            game.enddictdata = game.absdatablock;
            game.endmd = game.startabbrev;
            gamesList.Add(game);

            game = new L9V1GameData
            {
                name = "Snowball",
                acodestart_footprint = new byte[] { 0x00, 0x06, 0x00, 0x00, 0xd4, 0x01, 0x45, 0xa0, 0x48, 0x00, 0x01, 0x48, 0x00, 0x02, 0x48, 0x00 },
                acodeptr = 0x0000,
                dictdata = -0x0a10,
                absdatablock = 0x0300,
                startmd = 0x1930,
                startabbrev = 0x5547
            };
            game.startfile = game.dictdata;
            game.enddictdata = game.acodeptr;
            game.endmd = game.startabbrev;
            gamesList.Add(game);
        }

        public static L9Header Scan(byte[] infile)
        {
            L9V1Games gamesV1 = new L9V1Games();

            Boolean match = false;

            L9Header header = new L9Header
            {
                offset = -1
            };

            for (int i = 0; i < infile.Length; i++)
            {
                for (int game = 0; game < gamesV1.gamesList.Count; game++)
                {
                    if (infile[i] == gamesV1.gamesList[game].acodestart_footprint[0])
                    {
                        for (int j = 1; j < gamesV1.gamesList[game].acodestart_footprint.Length; j++)
                        {
                            if ((i+j)<infile.Length)
                            {
                                if (infile[i + j] != gamesV1.gamesList[game].acodestart_footprint[j]) break;
                                if (j == gamesV1.gamesList[game].acodestart_footprint.Length - 1)
                                {
                                    header.offset = i + gamesV1.gamesList[game].startfile;
                                    header.dictdata = i + gamesV1.gamesList[game].dictdata;
                                    header.absdatablock = i + gamesV1.gamesList[game].absdatablock;
                                    header.startmd = i + gamesV1.gamesList[game].startmd;
                                    header.startabbrev = i + gamesV1.gamesList[game].startabbrev;
                                    header.acodeptr = i + gamesV1.gamesList[game].acodeptr;
                                    header.gamename = gamesV1.gamesList[game].name;
                                    header.enddictdata = i + gamesV1.gamesList[game].enddictdata;
                                    header.endmd = i + gamesV1.gamesList[game].endmd;
                                    header.L9MsgType = 1;    //Always 1 for version 1
                                    header.L9Version = 1;

                                    match = true;
                                }
                            }
                        }
                        if (match) break;
                    }
                    if (match) break;
                }
                if (match) break;
            }

            return header;
        }

        public static List<string> UnpackAbbreviations(L9Header header,byte[] infile)
        {
            int i = 0;
            string abbr = "";
            int abbrNumber = 0;
            List<string> abbrList = new List<string>(new string[] { });
            do
            {
                byte currentByte;
                if (infile.Length <= (header.startabbrev + i))
                {
                    currentByte = 1;
                }
                else
                {
                    currentByte = infile[header.startabbrev + i];

                }
                Boolean eos = false;
                Boolean insertAbbr = false;
                if (currentByte < 3) eos = true;
                if (currentByte > 0x5d) insertAbbr = true;

                if (eos)
                {
                    abbrList.Add(abbr);
                    for (int j = 0; j < abbrList.Count; j++)
                    {
                        abbrList[j] = abbrList[j].Replace(string.Concat("[A", abbrNumber.ToString("000#"), "]"), abbr);
                    }
                    abbr = "";
                    abbrNumber++;
                }
                else
                {
                    if (insertAbbr)
                    {
                        currentByte -= 0x5e;
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
                        abbr = string.Concat(abbr, ByteToChar(currentByte));
                    }
                }

                i++;
            } while (abbrNumber < 162);

            header.endabbrev =header.startabbrev + i - 1;
            return abbrList;
        }

        public static List<string> UnpackMessages(L9Header header, List<string> abbrList, byte[] infile)
        {
            int i = 0;
            int maxAbbr = 0;
            string msg = "";
            int msgNumber = 0;
            List<string> msgList = new List<string>(new string[] { });
            do
            {
                byte currentByte = infile[header.startmd + i];
                Boolean eos = false;
                Boolean insertAbbr = false;
                if (currentByte < 3) eos = true;
                if (currentByte > 0x5d) insertAbbr = true;

                if (eos)
                {
                    msgList.Add(msg);
                    msg = "";
                    msgNumber++;
                }
                else
                {
                    if (insertAbbr)
                    {
                        currentByte -= 0x5e;
                        msg = string.Concat(msg, abbrList[currentByte]);
                        if (currentByte > maxAbbr) maxAbbr = currentByte;

                    }
                    else
                    {
                        currentByte += 0x1d;
                        msg = string.Concat(msg, L9V1Games.ByteToChar(currentByte));
                    }
                }

                i++;
            } while (i < (header.endmd - header.startmd));

            return msgList;
        }

        public static string ByteToChar(byte charByte)
        {
            if (charByte == 0x0d) charByte = 0x25;
            string currentstr = System.Text.Encoding.ASCII.GetString(new[] { charByte });
            currentstr = currentstr.Replace("%", "\n");
            currentstr = currentstr.Replace("_", " ");
            return currentstr;
        }

    }
}
