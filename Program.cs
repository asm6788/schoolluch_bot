using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputMessageContents;
using Telegram.Bot.Types.ReplyMarkups;

namespace schoolluch_bot
{
    public enum 학교종류
    {
        에러,
        초,
        중,
        고
    }
    public class 학교정보
    {
        // string 지역;
        public 학교종류 초중고;
        public string 학교이름;
        public string 학교코드;
        // string 학생수및성비;
        public string 학교주소;
        //   string 교원수및성비;
        //  string 전화;
        // string 팩스;
        // string 설립구분;
        // string 설립유형;
        public string 홈페이지;

        public 학교정보(string 홈페이지, string 학교주소, string 학교코드, string 학교이름, 학교종류 초중고)
        {
            this.홈페이지 = 홈페이지;
            this.학교주소 = 학교주소;
            this.학교코드 = 학교코드;
            this.학교이름 = 학교이름;
            this.초중고 = 초중고;
        }
    }

    public class 학교정보처리
    {
        public static List<학교정보> 학교정보파싱(string 검색학교)
        {
            HttpWebRequest wReq;
            Stream PostDataStream;
            Stream respPostStream;
            StreamReader readerPost;
            HttpWebResponse wResp;
            StringBuilder postParams = new StringBuilder();
            List<학교정보> 학교정보들 = new List<학교정보>();
            string 학교이름 = "";
            string 학교코드 = "";
            string 학교주소 = "";
            string 학교홈페이지 = "";
            학교종류 초중고 = 학교종류.에러;
            //SEARCH_GS_HANGMOK_CD=&SEARCH_GS_HANGMOK_NM=&SEARCH_SCHUL_NM=%BF%F9%B0%E8%C1%DF&SEARCH_GS_BURYU_CD=&SEARCH_KEYWORD=%BF%F9%B0%E8%C1%DF
            //보낼 데이터 추
            postParams.Append("SEARCH_GS_HANGMOK_CD=");
            postParams.Append("&SEARCH_GS_HANGMOK_NM=");
            postParams.Append("&SEARCH_SCHUL_NM=" + HttpUtility.UrlEncode(검색학교, Encoding.GetEncoding("euc-kr")));
            postParams.Append("&SEARCH_GS_BURYU_CD=");
            postParams.Append("&SEARCH_KEYWORD=" + HttpUtility.UrlEncode(검색학교, Encoding.GetEncoding("euc-kr")));

            //Encoding 정의 및 보낼 데이터 정보를 Byte배열로 변환(String -> Byte[])
            Encoding encoding = Encoding.UTF8;
            byte[] result = encoding.GetBytes(postParams.ToString());
            //<p class="School_Division">
            //보낼 곳과 데이터 보낼 방식 정의
            wReq = (HttpWebRequest)WebRequest.Create("http://www.schoolinfo.go.kr/ei/ss/Pneiss_f01_l0.do");
            wReq.Method = "POST";
            wReq.ContentType = "application/x-www-form-urlencoded";
            wReq.ContentLength = result.Length;

            string temp;
            //데이터 전송
            PostDataStream = wReq.GetRequestStream();
            PostDataStream.Write(result, 0, result.Length);
            PostDataStream.Close();
            wResp = (HttpWebResponse)wReq.GetResponse();
            respPostStream = wResp.GetResponseStream();
            readerPost = new StreamReader(respPostStream, Encoding.Default);
            String resultPost = readerPost.ReadToEnd();
            //     Console.WriteLine(resultPost);
            while (true)
            {
                resultPost = resultPost.Remove(0, resultPost.IndexOf("School_Name")).Remove(0, 76);
                temp = resultPost;
                학교이름 = resultPost = resultPost.Remove(resultPost.IndexOf("<"), resultPost.Length - resultPost.IndexOf("<"));
                if (!isContainHangul(학교이름))
                {
                    break;
                }
                resultPost = temp;
                resultPost = resultPost.Remove(0, resultPost.IndexOf("School_Division"));
                resultPost = resultPost.Remove(0, 45);
                resultPost = resultPost.Remove(0, resultPost.IndexOf("mapD_Class"));
                resultPost = resultPost.Remove(0, 16);
                temp = resultPost;
                resultPost = resultPost.Remove(resultPost.IndexOf("</span>"), resultPost.Length - resultPost.IndexOf("</span>"));
                if (resultPost == "초")
                {
                    초중고 = 학교종류.초;
                }
                else if (resultPost == "중")
                {
                    초중고 = 학교종류.중;
                }
                else if (resultPost == "고")
                {
                    초중고 = 학교종류.고;
                }
                resultPost = temp;
                resultPost = resultPost.Remove(0, resultPost.IndexOf("searchSchul")).Remove(0, 12);
                temp = resultPost;
                resultPost = resultPost.Remove(resultPost.IndexOf(")"), resultPost.Length - resultPost.IndexOf(")")).Replace("'", "");
                학교코드 = resultPost;
                resultPost = temp;
                resultPost = resultPost.Remove(0, resultPost.IndexOf("학교주소")).Remove(0, 11);
                temp = resultPost;
                resultPost = resultPost.Remove(resultPost.IndexOf("</li>"), resultPost.Length - resultPost.IndexOf("</li>"));
                학교주소 = resultPost;
                resultPost = temp;
                //  Console.WriteLine(resultPost);
                resultPost = resultPost.Remove(0, resultPost.IndexOf("홈페이지")).Remove(0, 38);
                temp = resultPost;
                resultPost = resultPost.Remove(resultPost.IndexOf("target"), resultPost.Length - resultPost.IndexOf("target"));
                resultPost = resultPost.Remove(resultPost.Length - 2, 1);
                학교홈페이지 = resultPost;
                resultPost = temp;
                학교정보들.Add(new 학교정보(학교홈페이지, 학교주소, 학교코드, 학교이름, 초중고));
            }
            return 학교정보들;
        }

        public static bool isContainHangul(string s)
        {

            char[] charArr = s.ToCharArray();
            foreach (char c in charArr)
            {
                if (char.GetUnicodeCategory(c) == System.Globalization.UnicodeCategory.OtherLetter)
                {
                    return true;
                }

            }
            return false;
        }

    }

    public class 급식
    {
        public int 날짜 = 0;
        public string 급식메뉴 = "";


        public 급식(int 날짜, string 급식메뉴)
        {
            this.날짜 = 날짜;
            this.급식메뉴 = 급식메뉴;
        }
        public 급식()
        {

        }
    }
    [Serializable]
    public class 급식구독자
    {
        public long ID = 0;
        public 받은급식정보 급식정정보들 = null;
        public 급식구독자(long ID, 받은급식정보 급식정정보들)
        {
            this.급식정정보들 = 급식정정보들;
            this.ID = ID;
        }
        public 급식구독자()
        {

        }
    }
    [Serializable]
    public class 받은급식정보
    {
        public enum 어느날의급식
        {
            None,
            오늘의급식,
            내일의급식,
            특정날의급식
        }
        public Program.학교종류 학교종류 = Program.학교종류.None;
        public Program.관할지역 관활지역 = Program.관할지역.None;
        public int 월 = 0;
        public int 일 = 0;
        public string 학교코드 = "";
        public 어느날의급식 어느날 = 어느날의급식.None;
        public bool 구독신청 = false;
        public 받은급식정보(Program.학교종류 학교종류, Program.관할지역 관활지역, string 학교코드, 어느날의급식 어느날,bool 구독신청)
        {
            this.학교종류 = 학교종류;
            this.관활지역 = 관활지역;
            this.학교코드 = 학교코드;
            this.어느날 = 어느날;
            this.구독신청 = 구독신청;
        }
      
        public 받은급식정보(Program.학교종류 학교종류, Program.관할지역 관활지역, string 학교코드)
        {
            this.학교종류 = 학교종류;
            this.관활지역 = 관활지역;
            this.학교코드 = 학교코드;
            
        }
        public 받은급식정보(Program.학교종류 학교종류, Program.관할지역 관활지역)
        {
            this.학교종류 = 학교종류;
            this.관활지역 = 관활지역;
        }
        public 받은급식정보(Program.학교종류 학교종류)
        {
            this.학교종류 = 학교종류;
        }
        public 받은급식정보()
        {
            
        }
    }


    public class Program
    {
        public static bool CheckNumber(string letter)
        {
            bool IsCheck = true;

            Regex numRegex = new Regex(@"[0-9]");
            Boolean ismatch = numRegex.IsMatch(letter);

            if (!ismatch)
            {
                IsCheck = false;
            }

            return IsCheck;
        }
            private static readonly TelegramBotClient Bot = new TelegramBotClient("API");
        static Dictionary<long, 받은급식정보> 급식저장 = new Dictionary<long, 받은급식정보>();
        static List<급식구독자> 구독자정보 = new List<급식구독자>();
        static Dictionary<long, bool> 학교코드입력단계 = new Dictionary<long, bool>();
        static Dictionary<long, List<학교정보>> 학교검색결과 = new Dictionary<long, List<학교정보>>();

        static List<급식> 급식불러오기(int Years,int Month,string ID, 관할지역 지역, 학교종류 종류)
        {
            string ResultOfstring = "0";
            using (WebClient client = new WebClient())
            {
                string 어디교육청 = "";
                switch (지역)
                {
                    case 관할지역.서울특별시:
                        어디교육청 = "stu.sen.go.kr";
                        break;
                    case 관할지역.인천광역시:
                        어디교육청 = "stu.ice.go.kr";
                        break;
                    case 관할지역.부산광역시:
                        어디교육청 = "stu.pen.go.kr";
                        break;
                    case 관할지역.광주광역시:
                        어디교육청 = "stu.gen.go.kr";
                        break;
                    case 관할지역.대전광역시:
                        어디교육청 = "stu.dje.go.kr";
                        break;
                    case 관할지역.대구광역시:
                        어디교육청 = "stu.dge.go.kr";
                        break;
                    case 관할지역.세종특별자치시:
                        어디교육청 = "stu.sje.go.kr";
                        break;
                    case 관할지역.울산광역시:
                        어디교육청 = "stu.use.go.kr";
                        break;
                    case 관할지역.경기도:
                        어디교육청 = "stu.goe.go.kr";
                        break;
                    case 관할지역.강원도:
                        어디교육청 = "stu.kwe.go.kr";
                        break;
                    case 관할지역.충청북도:
                        어디교육청 = "stu.cbe.go.kr";
                        break;
                    case 관할지역.충청남도:
                        어디교육청 = "stu.cne.go.kr";
                        break;
                    case 관할지역.경상북도:
                        어디교육청 = "stu.gbe.go.kr";
                        break;
                    case 관할지역.경상남도:
                        어디교육청 = "stu.gne.go.kr";
                        break;
                    case 관할지역.전라북도:
                        어디교육청 = "stu.jbe.go.kr";
                        break;
                    case 관할지역.전라남도:
                        어디교육청 = "stu.jne.go.kr";
                        break;
                    case 관할지역.제주도:
                        어디교육청 = "stu.jje.go.kr";
                        break;
                }
                string[] 배열 = null;
                client.Encoding = Encoding.UTF8;
                if(Month.ToString().Length == 1)
                {
                    ResultOfstring = "0" + Month;
                }
                else
                {
                    ResultOfstring = Month.ToString();
                }
                string htmlCode = client.DownloadString("http://"+어디교육청+"/sts_sci_md00_001.do?schulCode="+ID+"&schulCrseScCode="+Convert.ToInt32(종류)+"&schulKndScCode=0"+ Convert.ToInt32(종류) + "&ay="+Years+"&mm="+ ResultOfstring + "&");
                htmlCode = htmlCode.Remove(0, htmlCode.IndexOf("tbody"));
                //  Console.WriteLine(htmlCode);
                htmlCode = htmlCode.Remove(htmlCode.IndexOf("/tbody"));
                htmlCode = htmlCode.Replace("\t", "");
                htmlCode = htmlCode.Replace("\r\n", "");
                htmlCode = htmlCode.Replace("<td><div>", ":");
                // htmlCode = htmlCode.Replace("<br />", "");
                htmlCode = htmlCode.Replace("</div></td>", "");
                htmlCode = htmlCode.Replace(@"<td class=""last""><div>", "");
                htmlCode = htmlCode.Replace("t", "");
                htmlCode = htmlCode.Replace("ody", "");
                int 날짜 = 0;
                List<급식> 내용 = new List<급식>();
                배열 = htmlCode.Split("<br />".ToCharArray()).Where(x => !string.IsNullOrEmpty(x)).ToArray();
                for (int i = 1; i < 배열.Length; i++)
                {
                    if (배열[i - 1].LastIndexOf(":") != -1)
                    {
                        if (배열[i - 1].Remove(0, 배열[i - 1].LastIndexOf(":") + 1) != "")
                        {
                            날짜 = Convert.ToInt32(배열[i - 1].Remove(0, 배열[i - 1].LastIndexOf(":") + 1));
                            배열[i - 1] = 배열[i - 1].Remove(배열[i - 1].LastIndexOf(":"));
                        }
                        내용.Add(new 급식(날짜, 배열[i - 1]));
                    }
                    else
                    {
                        내용.Add(new 급식(날짜, 배열[i - 1]));
                    }
                }
                List<급식> 결과 = new List<급식>();
                급식 temp = new 급식();
                string temp2 = "";
                for (int i = 0; i < 내용.Count; i++)
                {
                    if (i == 0)
                    {
                        temp = 내용[i];
                    }
                    if (내용[i].날짜 == temp.날짜)
                    {
                        temp2 = temp2 + "\r\n" + 내용[i].급식메뉴;
                    }
                    else
                    {
                        if (내용[i].급식메뉴.Length > 0 && temp2.Length - 1 >0 && temp2.Length - 2 > 0)
                        {
                            if (내용[i].급식메뉴[0] != ':')
                            {
                                temp2 = temp2 + "\r\n" + 내용[i].급식메뉴;
                            }
                            if (CheckNumber(temp2[temp2.Length - 1].ToString()) && CheckNumber(temp2[temp2.Length - 2].ToString()))
                            {
                                temp2 = temp2.Remove(temp2.Length - 2, 2);
                            }
                            else if (CheckNumber(temp2[temp2.Length - 1].ToString()))
                            {
                                temp2 = temp2.Remove(temp2.Length - 1, 1);
                            }
                        }
                        결과.Add(new 급식(temp.날짜, temp2));
                        temp2 = "";

                        temp = 내용[i];
                    }
                }

                return 결과;
            }
        }
        static System.Timers.Timer timer = null;
        static void Main(string[] args)
        {
            System.Timers.Timer 시간제한 = new System.Timers.Timer(10000);
            시간제한.AutoReset = false;
            시간제한.Elapsed += 시간제한_Elapsed;
            Console.WriteLine("10초안에 선택하십시요.(기본값:정상작동)");
            시간제한.Start();
            Console.WriteLine("1.초기화");
            Console.WriteLine("2.정상작동");
            string temp = Console.ReadLine();
            if (temp == "1")
            {
                Console.WriteLine("초기화중");
                Bot.OnCallbackQuery += BotOnCallbackQueryReceived;
                Bot.OnMessage += Reset;
                Bot.OnInlineResultChosen += BotOnChosenInlineResultReceived;
                Bot.OnReceiveError += BotOnReceiveError;
                var me = Bot.GetMeAsync().Result;
                Console.ReadLine();
            }
            else if(temp == "2")
            {
                시간제한.Stop();
                시작();
            }
        }

        private static void 시간제한_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            시작();
        }
        public static void 시작()
        {
            Console.WriteLine("정상작동 시작");
            Bot.OnCallbackQuery += BotOnCallbackQueryReceived;
            Bot.OnMessage += BotOnMessageReceived;
            Bot.OnInlineResultChosen += BotOnChosenInlineResultReceived;
            Bot.OnReceiveError += BotOnReceiveError;

            var me = Bot.GetMeAsync().Result;
            timer = new System.Timers.Timer(TimeSpan.FromHours(24 - DateTime.Now.Hour).TotalMilliseconds);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
            List<급식구독자> p = new List<급식구독자>();
            BinaryFormatter binFmt = new BinaryFormatter();
            if (System.IO.File.Exists("구독자"))
            {
                using (FileStream rdr = new FileStream("구독자", FileMode.Open))
                {
                    p = (List<급식구독자>)binFmt.Deserialize(rdr);
                    rdr.Dispose();
                }
            }
            구독자정보 = p;     
            Console.Title = me.Username;
            Bot.StartReceiving();
            Console.WriteLine("엔터 로 종료합니다");
            Console.ReadLine();
            Console.WriteLine("봇종료");
            Bot.StopReceiving();
            using (FileStream fs = new FileStream("구독자", FileMode.Create))
            {
                binFmt.Serialize(fs, 구독자정보);
            }
        }
        private static void Reset(object sender, MessageEventArgs e)
        {
            
        }

        static async void 어디구역(long id)
        {
            var keyboard = new ReplyKeyboardMarkup(new[]
                {
                       new [] // first row
                    {
                        new KeyboardButton("서울"),
                        new KeyboardButton("인천"),
                        new KeyboardButton("부산"),
                        new KeyboardButton("세종"),
                        new KeyboardButton("경기"),
                        new KeyboardButton("충북"),
                        new KeyboardButton("경북"),
                        new KeyboardButton("전북")
                    },
                    new [] // last row
                    {
                        new KeyboardButton("광주"),
                        new KeyboardButton("대전"),
                        new KeyboardButton("대구"),
                        new KeyboardButton("울산"),
                        new KeyboardButton("강원"),
                        new KeyboardButton("충남"),
                        new KeyboardButton("경남"),
                        new KeyboardButton("전남"),
                        new KeyboardButton("제주")
                    }
                });
            await Bot.SendTextMessageAsync(id, "Choose",
                     replyMarkup: keyboard);
        }
        private static void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Console.WriteLine(e.SignalTime+"때 구독자급식전송을 시작했음");
            BinaryFormatter binFmt = new BinaryFormatter();
            using (FileStream fs = new FileStream("구독자", FileMode.Create))
            {
                binFmt.Serialize(fs, 구독자정보);
                fs.Dispose();
            }
            bool 찾음 = false;
            Console.WriteLine("급식구독자 급식전송중");
            List<급식> 급식메뉴다 = new List<급식>();
            for (int i = 0; i < 구독자정보.Count; i++)
            {
                급식메뉴다 = 급식불러오기(DateTime.Now.Year, DateTime.Now.Month, Convert.ToString(구독자정보[i].급식정정보들.학교코드), 구독자정보[i].급식정정보들.관활지역, 구독자정보[i].급식정정보들.학교종류);
                for (int ii = 0; ii < 급식메뉴다.Count; ii++)
                {
                    if (급식메뉴다[ii].날짜 == DateTime.Now.Day)
                    {
                        Bot.SendTextMessageAsync(구독자정보[i].ID, 급식메뉴다[ii].급식메뉴);
                        찾음 = true;
                        break;
                    }
                }
                if (!찾음)
                {
                    Bot.SendTextMessageAsync(구독자정보[i].ID, "급식이 없습니다");
                }
            }
            Console.WriteLine("완료");
            timer.Interval = TimeSpan.FromHours(24).TotalMilliseconds;
        }

        private static void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            
        }

        private static void BotOnChosenInlineResultReceived(object sender, ChosenInlineResultEventArgs chosenInlineResultEventArgs)
        {
            Console.WriteLine($"Received choosen inline result: {chosenInlineResultEventArgs.ChosenInlineResult.ResultId}");
        }
        public enum 학교종류
        {
            None,
            병설유치원,
            초등학교,
            중학교,
            고등학교
        }
        public enum 관할지역
        {
            None,
            서울특별시,
            인천광역시,
            부산광역시,
            광주광역시,
            대전광역시,
            대구광역시,
            세종특별자치시,
            울산광역시,
            경기도,
            강원도,
            충청북도,
            충청남도,
            경상북도,
            경상남도,
            전라북도,
            전라남도,
            제주도
        }
        public enum 시간
        {
            None,
            아침,
            점심,
            저녁
        }

        static async void 지역(string Message, long ID)
        {
            var keyboard = new ReplyKeyboardMarkup(new[]
                {
                       new [] // first row
                    {
                        new KeyboardButton("서울특별시"),
                        new KeyboardButton("인청광역시"),
                    },
                    new [] // last row
                    {
                        new KeyboardButton("부산광역시"),
                        new KeyboardButton("대전광역시"),
                    }
                  });
            await Bot.SendTextMessageAsync(ID, "Choose", replyMarkup: keyboard);
        }

        static async void 도움말(long ID)
        {
            var usage = "밥먹자! /start로 시작합니다 사전에 학교 코드를 알아합니다(NEIS 코드)\r\n/취소으로 취소할수있습니다";

            await Bot.SendTextMessageAsync(ID, usage,
                replyMarkup: new ReplyKeyboardRemove());
        }

        private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
          
            var message = messageEventArgs.Message;
            if (message == null || message.Type != MessageType.TextMessage) return;

            if (message.Text.StartsWith("/취소"))
            {
                급식저장.Remove(message.Chat.Id);
                학교코드입력단계.Remove(message.Chat.Id);
                학교검색결과.Remove(message.Chat.Id);
                await Bot.SendTextMessageAsync(message.Chat.Id, "취소되였습니다", replyMarkup: new ReplyKeyboardRemove());
            }
            else if (message.Text.ToLower().StartsWith("/start")) // send custom keyboard
            {
                var keyboard = new ReplyKeyboardMarkup(new[]
                {
                    new [] // first row
                    {
                        new KeyboardButton("/오늘의급식"),
                        new KeyboardButton("/내일의급식"),
                          new KeyboardButton("/구독신청")
                    },
                    new [] // last row
                    {
                        new KeyboardButton("/특정한날의급식"),
                        new KeyboardButton("/구독취소"),
                        new KeyboardButton("/학교코드검색")
                    }
                });
                await Bot.SendTextMessageAsync(message.Chat.Id, "Choose",
                    replyMarkup: keyboard);
            }
            else if (message.Text.StartsWith("/오늘의급식"))
            {
                var keyboard = new ReplyKeyboardMarkup(new[]
                {
                       new [] // first row
                    {
                        new KeyboardButton("병설유치원"),
                        new KeyboardButton("초등학교"),
                    },
                    new [] // last row
                    {
                        new KeyboardButton("중학교"),
                        new KeyboardButton("고등학교"),
                    }
                  });

                급식저장[message.Chat.Id] = new 받은급식정보(학교종류.None, 관할지역.None, "", 받은급식정보.어느날의급식.오늘의급식, false);
                await Bot.SendTextMessageAsync(message.Chat.Id, "Choose",
                          replyMarkup: keyboard);
            }
            else if (message.Text.StartsWith("/내일의급식"))
            {
                var keyboard = new ReplyKeyboardMarkup(new[]
                {
                       new [] // first row
                    {
                        new KeyboardButton("병설유치원"),
                        new KeyboardButton("초등학교"),
                    },
                    new [] // last row
                    {
                        new KeyboardButton("중학교"),
                        new KeyboardButton("고등학교"),
                    }
                  });

                급식저장[message.Chat.Id] = new 받은급식정보(학교종류.None, 관할지역.None, "", 받은급식정보.어느날의급식.내일의급식, false);
                await Bot.SendTextMessageAsync(message.Chat.Id, "Choose",
                          replyMarkup: keyboard);
            }
            else if (message.Text.StartsWith("/특정한날의급식"))
            {
                var keyboard = new ReplyKeyboardMarkup(new[]
                {
                       new [] // first row
                    {
                        new KeyboardButton("병설유치원"),
                        new KeyboardButton("초등학교"),
                    },
                    new [] // last row
                    {
                        new KeyboardButton("중학교"),
                        new KeyboardButton("고등학교"),
                    }
                  });

                급식저장[message.Chat.Id] = new 받은급식정보(학교종류.None, 관할지역.None, "", 받은급식정보.어느날의급식.특정날의급식, false);
                await Bot.SendTextMessageAsync(message.Chat.Id, "Choose",
                          replyMarkup: keyboard);
            }
            else if (message.Text.StartsWith("/구독신청"))
            {
                bool 찾음 = false;
                for (int i = 0; i < 구독자정보.Count; i++)
                {
                    if (구독자정보[i].ID == message.Chat.Id)
                    {
                        찾음 = true;
                        await Bot.SendTextMessageAsync(message.Chat.Id, "이미 등록되셨습니다");
                    }
                }
                if (!찾음)
                {
                    var keyboard = new ReplyKeyboardMarkup(new[]
                 {
                       new [] // first row
                    {
                        new KeyboardButton("병설유치원"),
                        new KeyboardButton("초등학교"),
                    },
                    new [] // last row
                    {
                        new KeyboardButton("중학교"),
                        new KeyboardButton("고등학교"),
                    }
                  });

                    급식저장[message.Chat.Id] = new 받은급식정보(학교종류.None, 관할지역.None, "", 받은급식정보.어느날의급식.특정날의급식, true);
                    await Bot.SendTextMessageAsync(message.Chat.Id, "Choose",
                              replyMarkup: keyboard);
                }
            }
            else if (message.Text.StartsWith("/구독취소"))
            {
                bool 찾음 = false;
                for (int i = 0; i < 구독자정보.Count; i++)
                {
                    if (구독자정보[i].ID == message.Chat.Id)
                    {
                        찾음 = true;
                        구독자정보.RemoveAt(i);
                        await Bot.SendTextMessageAsync(message.Chat.Id, "처리완료");
                    }
                }

                if (!찾음)
                {
                    await Bot.SendTextMessageAsync(message.Chat.Id, "등록되있지않습니다");
                }

            }
            else if (message.Text.StartsWith("/학교코드검색"))
            {
                학교코드입력단계[message.Chat.Id] = true;
                await Bot.SendTextMessageAsync(message.Chat.Id, "검색할 학교를 입력해주세요.", replyMarkup: new ReplyKeyboardRemove());
            }
            else if (학교코드입력단계.ContainsKey(message.Chat.Id))
            {
                string 조합 = "";
                int i = 0;
                await Bot.SendTextMessageAsync(message.Chat.Id, "처리중....");
                학교검색결과.Add(message.Chat.Id,학교정보처리.학교정보파싱(message.Text));
                if (학교검색결과[message.Chat.Id].Count != 0)
                {
                    foreach (학교정보 aed in 학교검색결과[message.Chat.Id])
                    {
                        i++;
                        var fields =
                            typeof(학교정보).GetFields(BindingFlags.Public | BindingFlags.NonPublic |
                                                   BindingFlags.Instance);
                        var names = Array.ConvertAll(fields, field => field.Name);
                        조합 += i + "\r\n";
                        foreach (string 필드명 in names)
                        {
                            FieldInfo fld = typeof(학교정보).GetField(필드명);
                            조합 += 필드명 + " : " + fld.GetValue(aed) + "\r\n";
                        }
                        조합 += "-------------------------";
                        await Bot.SendTextMessageAsync(message.Chat.Id, 조합);
                        조합 = "";
                    }
                }
                else
                {
                    await Bot.SendTextMessageAsync(message.Chat.Id, "학교를 찾을수없습니다");
                }
                학교코드입력단계.Remove(message.Chat.Id);
                학교검색결과.Remove(message.Chat.Id);
            }
            else if (!급식저장.ContainsKey(message.Chat.Id))
            {
                도움말(message.Chat.Id);
            }
            else if (급식저장[message.Chat.Id].학교종류 == 학교종류.None && 급식저장[message.Chat.Id].관활지역 == 관할지역.None && 급식저장[message.Chat.Id].학교코드 == "")
            {
                switch (message.Text)
                {
                    case "병설유치원":
                        급식저장[message.Chat.Id] = new 받은급식정보(학교종류.병설유치원, 관할지역.None, "", 급식저장[message.Chat.Id].어느날, 급식저장[message.Chat.Id].구독신청);
                        어디구역(message.Chat.Id);
                        break;
                    case "초등학교":
                        급식저장[message.Chat.Id] = new 받은급식정보(학교종류.초등학교, 관할지역.None, "", 급식저장[message.Chat.Id].어느날, 급식저장[message.Chat.Id].구독신청);
                        어디구역(message.Chat.Id);
                        break;
                    case "중학교":
                        급식저장[message.Chat.Id] = new 받은급식정보(학교종류.중학교, 관할지역.None, "", 급식저장[message.Chat.Id].어느날, 급식저장[message.Chat.Id].구독신청);
                        어디구역(message.Chat.Id);
                        break;
                    case "고등학교":
                        급식저장[message.Chat.Id] = new 받은급식정보(학교종류.고등학교, 관할지역.None, "", 급식저장[message.Chat.Id].어느날, 급식저장[message.Chat.Id].구독신청);
                        어디구역(message.Chat.Id);
                        break;
                    default:
                        급식저장[message.Chat.Id] = new 받은급식정보(학교종류.None, 관할지역.None, "", 급식저장[message.Chat.Id].어느날, 급식저장[message.Chat.Id].구독신청);
                        await Bot.SendTextMessageAsync(message.Chat.Id, "올바르지 않은 입력입니다");
                        break;
                }
                
            }
            else if (급식저장[message.Chat.Id].학교종류 != 학교종류.None && 급식저장[message.Chat.Id].관활지역 == 관할지역.None && 급식저장[message.Chat.Id].학교코드 == "")
            {
                switch (message.Text)
                {
                    case "서울":
                        급식저장[message.Chat.Id] = new 받은급식정보(급식저장[message.Chat.Id].학교종류, 관할지역.서울특별시, "", 급식저장[message.Chat.Id].어느날, 급식저장[message.Chat.Id].구독신청);
                        await Bot.SendTextMessageAsync(message.Chat.Id, "학교코드를 입력하십시요:", replyMarkup: new ReplyKeyboardRemove());
                        break;
                    case "인천":
                        급식저장[message.Chat.Id] = new 받은급식정보(급식저장[message.Chat.Id].학교종류, 관할지역.인천광역시, "", 급식저장[message.Chat.Id].어느날, 급식저장[message.Chat.Id].구독신청);
                        await Bot.SendTextMessageAsync(message.Chat.Id, "학교코드를 입력하십시요:", replyMarkup: new ReplyKeyboardRemove());
                        break;
                    case "부산":
                        급식저장[message.Chat.Id] = new 받은급식정보(급식저장[message.Chat.Id].학교종류, 관할지역.부산광역시, "", 급식저장[message.Chat.Id].어느날, 급식저장[message.Chat.Id].구독신청);
                        await Bot.SendTextMessageAsync(message.Chat.Id, "학교코드를 입력하십시요:", replyMarkup: new ReplyKeyboardRemove());
                        break;
                    case "세종":
                        급식저장[message.Chat.Id] = new 받은급식정보(급식저장[message.Chat.Id].학교종류, 관할지역.세종특별자치시, "", 급식저장[message.Chat.Id].어느날, 급식저장[message.Chat.Id].구독신청);
                        await Bot.SendTextMessageAsync(message.Chat.Id, "학교코드를 입력하십시요:", replyMarkup: new ReplyKeyboardRemove());
                        break;
                    case "경기":
                        급식저장[message.Chat.Id] = new 받은급식정보(급식저장[message.Chat.Id].학교종류, 관할지역.경기도, "", 급식저장[message.Chat.Id].어느날, 급식저장[message.Chat.Id].구독신청);
                        await Bot.SendTextMessageAsync(message.Chat.Id, "학교코드를 입력하십시요:", replyMarkup: new ReplyKeyboardRemove());
                        break;
                    case "충북":
                        급식저장[message.Chat.Id] = new 받은급식정보(급식저장[message.Chat.Id].학교종류, 관할지역.충청북도, "", 급식저장[message.Chat.Id].어느날, 급식저장[message.Chat.Id].구독신청);
                        await Bot.SendTextMessageAsync(message.Chat.Id, "학교코드를 입력하십시요:", replyMarkup: new ReplyKeyboardRemove());
                        break;
                    case "경북":
                        급식저장[message.Chat.Id] = new 받은급식정보(급식저장[message.Chat.Id].학교종류, 관할지역.경상북도, "", 급식저장[message.Chat.Id].어느날, 급식저장[message.Chat.Id].구독신청);
                        await Bot.SendTextMessageAsync(message.Chat.Id, "학교코드를 입력하십시요:", replyMarkup: new ReplyKeyboardRemove());
                        break;
                    case "전북":
                        급식저장[message.Chat.Id] = new 받은급식정보(급식저장[message.Chat.Id].학교종류, 관할지역.전라북도, "", 급식저장[message.Chat.Id].어느날, 급식저장[message.Chat.Id].구독신청);
                        await Bot.SendTextMessageAsync(message.Chat.Id, "학교코드를 입력하십시요:", replyMarkup: new ReplyKeyboardRemove());
                        break;
                    case "광주":
                        급식저장[message.Chat.Id] = new 받은급식정보(급식저장[message.Chat.Id].학교종류, 관할지역.광주광역시, "", 급식저장[message.Chat.Id].어느날, 급식저장[message.Chat.Id].구독신청);
                        await Bot.SendTextMessageAsync(message.Chat.Id, "학교코드를 입력하십시요:", replyMarkup: new ReplyKeyboardRemove());
                        break;
                    case "대전":
                        급식저장[message.Chat.Id] = new 받은급식정보(급식저장[message.Chat.Id].학교종류, 관할지역.대전광역시, "", 급식저장[message.Chat.Id].어느날, 급식저장[message.Chat.Id].구독신청);
                        await Bot.SendTextMessageAsync(message.Chat.Id, "학교코드를 입력하십시요:", replyMarkup: new ReplyKeyboardRemove());
                        break;
                    case "대구":
                        급식저장[message.Chat.Id] = new 받은급식정보(급식저장[message.Chat.Id].학교종류, 관할지역.대구광역시, "", 급식저장[message.Chat.Id].어느날, 급식저장[message.Chat.Id].구독신청);
                        await Bot.SendTextMessageAsync(message.Chat.Id, "학교코드를 입력하십시요:", replyMarkup: new ReplyKeyboardRemove());
                        break;
                    case "울산":
                        급식저장[message.Chat.Id] = new 받은급식정보(급식저장[message.Chat.Id].학교종류, 관할지역.울산광역시, "", 급식저장[message.Chat.Id].어느날, 급식저장[message.Chat.Id].구독신청);
                        await Bot.SendTextMessageAsync(message.Chat.Id, "학교코드를 입력하십시요:", replyMarkup: new ReplyKeyboardRemove());
                        break;
                    case "강원":
                        급식저장[message.Chat.Id] = new 받은급식정보(급식저장[message.Chat.Id].학교종류, 관할지역.강원도, "", 급식저장[message.Chat.Id].어느날, 급식저장[message.Chat.Id].구독신청);
                        await Bot.SendTextMessageAsync(message.Chat.Id, "학교코드를 입력하십시요:", replyMarkup: new ReplyKeyboardRemove());
                        break;
                    case "충남":
                        급식저장[message.Chat.Id] = new 받은급식정보(급식저장[message.Chat.Id].학교종류, 관할지역.충청남도, "", 급식저장[message.Chat.Id].어느날, 급식저장[message.Chat.Id].구독신청);
                        await Bot.SendTextMessageAsync(message.Chat.Id, "학교코드를 입력하십시요:", replyMarkup: new ReplyKeyboardRemove());
                        break;
                    case "경남":
                        급식저장[message.Chat.Id] = new 받은급식정보(급식저장[message.Chat.Id].학교종류, 관할지역.경상남도, "", 급식저장[message.Chat.Id].어느날, 급식저장[message.Chat.Id].구독신청);
                        await Bot.SendTextMessageAsync(message.Chat.Id, "학교코드를 입력하십시요:", replyMarkup: new ReplyKeyboardRemove());
                        break;
                    case "전남":
                        급식저장[message.Chat.Id] = new 받은급식정보(급식저장[message.Chat.Id].학교종류, 관할지역.전라남도, "", 급식저장[message.Chat.Id].어느날, 급식저장[message.Chat.Id].구독신청);
                        await Bot.SendTextMessageAsync(message.Chat.Id, "학교코드를 입력하십시요:", replyMarkup: new ReplyKeyboardRemove());
                        break;
                    case "제주":
                        급식저장[message.Chat.Id] = new 받은급식정보(급식저장[message.Chat.Id].학교종류, 관할지역.제주도, "", 급식저장[message.Chat.Id].어느날, 급식저장[message.Chat.Id].구독신청);
                        await Bot.SendTextMessageAsync(message.Chat.Id, "학교코드를 입력하십시요:", replyMarkup: new ReplyKeyboardRemove());
                        break;
                    default:
                        급식저장[message.Chat.Id] = new 받은급식정보(급식저장[message.Chat.Id].학교종류, 관할지역.None, "", 급식저장[message.Chat.Id].어느날, 급식저장[message.Chat.Id].구독신청);
                        await Bot.SendTextMessageAsync(message.Chat.Id, "올바르지 않은 입력입니다");
                        break;
                }
              
            }
            else if (급식저장[message.Chat.Id].학교종류 != 학교종류.None && 급식저장[message.Chat.Id].관활지역 != 관할지역.None && 급식저장[message.Chat.Id].학교코드 == "")
            {
                if (Regex.IsMatch(message.Text, @"[A-Z]\d{9}"))
                {
                    급식저장[message.Chat.Id].학교코드 = message.Text;
                    await Bot.SendTextMessageAsync(message.Chat.Id, "완료");
                    if (급식저장[message.Chat.Id].어느날 == 받은급식정보.어느날의급식.오늘의급식)
                    {
                        bool 찾음 = false;
                        List<급식> 급식메뉴다 = 급식불러오기(DateTime.Now.Year, DateTime.Now.Month, 급식저장[message.Chat.Id].학교코드, 급식저장[message.Chat.Id].관활지역, 급식저장[message.Chat.Id].학교종류);
                        for (int i = 0; i < 급식메뉴다.Count; i++)
                        {
                            if (급식메뉴다[i].날짜 == DateTime.Now.Day)
                            {
                                await Bot.SendTextMessageAsync(message.Chat.Id, 급식메뉴다[i].급식메뉴);
                                찾음 = true;
                                break;
                            }
                        }
                        if (!찾음)
                        {
                            await Bot.SendTextMessageAsync(message.Chat.Id, "해당 급식이 없습니다");
                        }
                        급식저장.Remove(message.Chat.Id);
                    }
                    else if (급식저장[message.Chat.Id].구독신청)
                    {
                        구독자정보.Add(new 급식구독자(message.Chat.Id, 급식저장[message.Chat.Id]));
                       await Bot.SendTextMessageAsync(message.Chat.Id, "구독신청되였습니다");
                    }
                    else if (급식저장[message.Chat.Id].어느날 == 받은급식정보.어느날의급식.내일의급식)
                    {
                        DateTime 내일 = DateTime.Now.AddDays(1);
                        bool 찾음 = false;
                        List<급식> 급식메뉴다 = 급식불러오기(내일.Year, 내일.Month, 급식저장[message.Chat.Id].학교코드, 급식저장[message.Chat.Id].관활지역, 급식저장[message.Chat.Id].학교종류);
                        for (int i = 0; i < 급식메뉴다.Count; i++)
                        {
                            if (급식메뉴다[i].날짜 == 내일.Day)
                            {
                                await Bot.SendTextMessageAsync(message.Chat.Id, 급식메뉴다[i].급식메뉴);
                                찾음 = true;
                                break;
                            }
                        }
                        if (!찾음)
                        {
                            await Bot.SendTextMessageAsync(message.Chat.Id, "해당 급식이 없습니다");
                        }
                        급식저장.Remove(message.Chat.Id);
                    }
                    else if (급식저장[message.Chat.Id].어느날 == 받은급식정보.어느날의급식.특정날의급식)
                    {
                        var keyboard = new ReplyKeyboardMarkup(new[]
                        {
                       new [] // first row
                    {
                        new KeyboardButton("1월"),
                        new KeyboardButton("2월"),
                        new KeyboardButton("3월"),
                        new KeyboardButton("4월"),
                        new KeyboardButton("5월"),
                        new KeyboardButton("6월"),
                    },
                    new [] // last row
                    {
                        new KeyboardButton("7월"),
                        new KeyboardButton("8월"),
                        new KeyboardButton("9월"),
                        new KeyboardButton("10월"),
                        new KeyboardButton("11월"),
                        new KeyboardButton("12월")
                    }
                        });
                        await Bot.SendTextMessageAsync(message.Chat.Id, "Choose",
                                 replyMarkup: keyboard);
                    }
                }
                else
                {
                    await Bot.SendTextMessageAsync(message.Chat.Id, "학교코드를 입력하십시요:");
                }
            }
            else if (급식저장[message.Chat.Id].학교종류 != 학교종류.None && 급식저장[message.Chat.Id].관활지역 != 관할지역.None && 급식저장[message.Chat.Id].학교코드 != "" && 급식저장[message.Chat.Id].어느날 == 받은급식정보.어느날의급식.특정날의급식)
            {
                bool 찾음 = false;
                List<급식> 급식메뉴다 = null;
                if (급식저장[message.Chat.Id].월 == 0)
                {
                    switch (message.Text)
                    {
                        case "1월":
                            급식저장[message.Chat.Id].월 = 1;
                            await Bot.SendTextMessageAsync(message.Chat.Id, "몇일?(그냥 숫자만 던져주세요)", replyMarkup: new ReplyKeyboardRemove());
                            break;
                        case "2월":
                            급식저장[message.Chat.Id].월 = 2;
                            await Bot.SendTextMessageAsync(message.Chat.Id, "몇일?(그냥 숫자만 던져주세요)", replyMarkup: new ReplyKeyboardRemove());
                            break;
                        case "3월":
                            급식저장[message.Chat.Id].월 = 3;
                            await Bot.SendTextMessageAsync(message.Chat.Id, "몇일?(그냥 숫자만 던져주세요)", replyMarkup: new ReplyKeyboardRemove());
                            break;
                        case "4월":
                            급식저장[message.Chat.Id].월 = 4;
                            await Bot.SendTextMessageAsync(message.Chat.Id, "몇일?(그냥 숫자만 던져주세요)", replyMarkup: new ReplyKeyboardRemove());
                            break;
                        case "5월":
                            급식저장[message.Chat.Id].월 = 5;
                            await Bot.SendTextMessageAsync(message.Chat.Id, "몇일?(그냥 숫자만 던져주세요)", replyMarkup: new ReplyKeyboardRemove());
                            break;
                        case "6월":
                            급식저장[message.Chat.Id].월 = 6;
                            await Bot.SendTextMessageAsync(message.Chat.Id, "몇일?(그냥 숫자만 던져주세요)", replyMarkup: new ReplyKeyboardRemove());
                            break;
                        case "7월":
                            급식저장[message.Chat.Id].월 = 7;
                            await Bot.SendTextMessageAsync(message.Chat.Id, "몇일?(그냥 숫자만 던져주세요)", replyMarkup: new ReplyKeyboardRemove());
                            break;
                        case "8월":
                            급식저장[message.Chat.Id].월 = 8;
                            await Bot.SendTextMessageAsync(message.Chat.Id, "몇일?(그냥 숫자만 던져주세요)", replyMarkup: new ReplyKeyboardRemove());
                            break;
                        case "9월":
                            급식저장[message.Chat.Id].월 = 9;
                            await Bot.SendTextMessageAsync(message.Chat.Id, "몇일?(그냥 숫자만 던져주세요)", replyMarkup: new ReplyKeyboardRemove());
                            break;
                        case "10월":
                            급식저장[message.Chat.Id].월 = 10;
                            await Bot.SendTextMessageAsync(message.Chat.Id, "몇일?(그냥 숫자만 던져주세요)", replyMarkup: new ReplyKeyboardRemove());
                            break;
                        case "11월":
                            급식저장[message.Chat.Id].월 = 11;
                            await Bot.SendTextMessageAsync(message.Chat.Id, "몇일?(그냥 숫자만 던져주세요)", replyMarkup: new ReplyKeyboardRemove());
                            break;
                        case "12월":
                            급식저장[message.Chat.Id].월 = 12;
                            await Bot.SendTextMessageAsync(message.Chat.Id, "몇일?(그냥 숫자만 던져주세요)", replyMarkup: new ReplyKeyboardRemove());
                            break;
                        default:
                            급식저장[message.Chat.Id].월 = 0;
                            await Bot.SendTextMessageAsync(message.Chat.Id, "올바르지 않은 입력입니다");
                            break;
                    }
                }
                else if (급식저장[message.Chat.Id].일 == 0)
                {
                    if (int.TryParse(message.Text, out 급식저장[message.Chat.Id].일))
                    {
                        급식메뉴다 = 급식불러오기(DateTime.Now.Year, 급식저장[message.Chat.Id].월, 급식저장[message.Chat.Id].학교코드, 급식저장[message.Chat.Id].관활지역, 급식저장[message.Chat.Id].학교종류);
                        for (int i = 0; i < 급식메뉴다.Count; i++)
                        {
                            if (급식메뉴다[i].날짜 == 급식저장[message.Chat.Id].일)
                            {
                                await Bot.SendTextMessageAsync(message.Chat.Id, 급식메뉴다[i].급식메뉴);
                                찾음 = true;
                                break;
                            }
                        }
                        if (!찾음)
                        {
                            await Bot.SendTextMessageAsync(message.Chat.Id, "해당 급식이 없습니다");
                        }

                        급식저장.Remove(message.Chat.Id);
                    }
                    else
                    {
                        await Bot.SendTextMessageAsync(message.Chat.Id, "몇일?(그냥 숫자만 던져주세요)");
                    }
                }
                else
                {
                    도움말(message.Chat.Id);
                }
            }
        }


     



        private static async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {
                await Bot.AnswerCallbackQueryAsync(callbackQueryEventArgs.CallbackQuery.Id,
                    $"Received {callbackQueryEventArgs.CallbackQuery.Data}");
        
        }

    }
}
