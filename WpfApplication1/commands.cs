using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace WpfApplication1
{
    public class commands //////////////////////List of commands///////////////////////
    {
        //fetching OpenAPI instance, console instance from MainWindow
        private AxKHOpenAPILib.AxKHOpenAPI axKHOA = MainWindow.getKHOA();
        private ConsoleContent dc = MainWindow.getDc();

        public void commandLoginComm() //Login Request communication with the hts.
        {
            long result;
            result = axKHOA.CommConnect();
            if (result != 0)
                MessageBox.Show("Login창 열림 Fail");
            outputHandler("waiting for login");
            axKHOA.OnEventConnect += new AxKHOpenAPILib._DKHOpenAPIEvents_OnEventConnectEventHandler(eventLogin); //Trigger connection event 'eventLogin'
        }

        public void commandUserInfo()
        {
            if (axKHOA.GetConnectState() == 0)
            {
                outputHandler(constants.UNCONNECTED);
                return;
            }
            outputHandler("User ID: " + axKHOA.GetLoginInfo("USER_ID").Trim());
            outputHandler("User name: " + axKHOA.GetLoginInfo("USER_NAME").Trim());
            outputHandler("Accounts: " + axKHOA.GetLoginInfo("ACCNO").Trim());
            switch (axKHOA.GetLoginInfo("KEY_BSECGB"))
            {
                case "0":
                   outputHandler("keyboard safety is ON");
                   break;
                case "1":
                    outputHandler("keyboard safety is OFF");
                    break;
                default: break;
            }
            switch (axKHOA.GetLoginInfo("FIREW_SECGB"))
            {
                case "0":
                outputHandler("firewall is not configurated");
                   break;
                case "1":
                outputHandler("firewall is ON");
                   break;
                case "2":
                outputHandler("firewall is OFF");
                   break;
                default: break;
            }
        }
        public void commandConnectionState()
        {
            switch (axKHOA.GetConnectState())
            {
                case 0:
                    outputHandler("You are NOT connected");
                    break;
                case 1:
                    outputHandler("You are CONNECTED");
                    break;
            }
        }
        public void commandStartSearch()
        {
            if (axKHOA.GetConnectState() == 0)
            {
                outputHandler(constants.UNCONNECTED);
                return;
            }

            /*
            axKHOA.SetInputValue("종목코드", dc.ConsoleInput);///step 1
            errorCommRqData(axKHOA.CommRqData("me", "OPT10001", 0, "0101"));// step 2

            this.axKHOA.OnReceiveTrData += new AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEventHandler(this.eventReceiveTrDataPlus);//step 3
            */
            axKHOA.SetInputValue("시장구분", "000");
            axKHOA.SetInputValue("매매구분", "2");
            axKHOA.SetInputValue("기간", "1");
            errorCommRqData(axKHOA.CommRqData("foreignTop", "OPT10034", 0, "0101"));
            this.axKHOA.OnReceiveTrData += new AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEventHandler(this.eventReceiveTrForeignerTop);


           // this.axKHOA.OnReceiveTrCondition += new AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrConditionEventHandler(this.eventReceiveTrCondition);

        }
        public void commandTrade()
        {
            if (axKHOA.GetConnectState() == 0)
            {
                outputHandler(constants.UNCONNECTED);
                return;
            }

            commandStartSearch();

            outputHandler("The top foreign bought product is " + gloVar.foreignTop[1, 2]);
            outputHandler("attempting to buy " + gloVar.foreignTop[1, 2]);

            /*
            // 현재가로 구매하기 위해 외인 매수 톱 종목의 현재가를 받는 과정임, 주문은 반환되는 이벤트 함수 내에서 이루어짐
            axKHOA.SetInputValue("종목코드", gloVar.foreignTop[0, 1]);
            errorCommRqData(axKHOA.CommRqData("me", "OPT10001", 0, "0101"));

            this.axKHOA.OnReceiveTrData += new AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEventHandler(this.eventReceiveForeignTopPrice);


            */
        }
        public void commandHelp()
        {
            outputHandler("login: Login into Kiwoom HTS");
            outputHandler("userinfo: display logged user info");
            outputHandler("connection: am I connected(logged)?");
            outputHandler("search: search an appropriate product automatically");
            outputHandler("trade: trade according to the search algorithm automatically");
        }

        // Event methods
        public void eventLogin(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnEventConnectEvent e)
        {

            if (e.nErrCode != 0)
            {
                outputHandler("failed to login.... please try again.");
                return;
            }

            outputHandler("login successful! Welcome " + axKHOA.GetLoginInfo("USER_NAME") + "!");
            if (axKHOA.GetConnectState() == 0)
                outputHandler("Connection state: not connected");
            else if (axKHOA.GetConnectState() == 1)
            {
                outputHandler("Connection state: connected");
                commandUserInfo();
            }
            Initialize();
            return;
        }
        /*
        private void axKHOpenAPI_OnReceiveConditionVer(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveConditionVerEvent e)
        {
            if (e.lRet == 1)
            {
                //[이벤트] 조건식 저장 성공
            }
            else
            {
                //[이벤트] 조건식 저장 실패
            }

        }
        
        private void eventReceiveTrDataPlus(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEvent e)
        {
            if (e.sRQName == "me")
            {
                outputHandler("종목코드: " + axKHOA.CommGetData(e.sTrCode, "", e.sRQName, 0, "종목코드"));
                outputHandler("종목명: " + axKHOA.CommGetData(e.sTrCode, "", e.sRQName, 0, "종목명"));
                outputHandler("시가: " + axKHOA.CommGetData(e.sTrCode, "", e.sRQName, 0, "시가"));
            }
        }// step 4 */
        private void eventReceiveTrForeignerTop(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEvent e)
        {
            gloVar.nCount = axKHOA.GetRepeatCnt(e.sTrCode, e.sRQName);

            string[,,] foreignerTop = new string[gloVar.nCount, 7, 15];

            for (int i = 0; i < gloVar.nCount; i++)
            {
                gloVar.foreignTop[i, 0] = axKHOA.CommGetData(e.sTrCode, "", e.sRQName, i, "순위").Trim(); //순위
                gloVar.foreignTop[i, 1] = axKHOA.CommGetData(e.sTrCode, "", e.sRQName, i, "종목코드").Trim(); //종목코드 
                gloVar.foreignTop[i, 2] = axKHOA.CommGetData(e.sTrCode, "", e.sRQName, i, "종목명").Trim();  //종목명
                gloVar.foreignTop[i, 3] = axKHOA.CommGetData(e.sTrCode, "", e.sRQName, i, "현재가").Trim(); //현재가
                gloVar.foreignTop[i, 4] = axKHOA.CommGetData(e.sTrCode, "", e.sRQName, i, "전일대비").Trim();//전일대비
                gloVar.foreignTop[i, 5] = axKHOA.CommGetData(e.sTrCode, "", e.sRQName, i, "거래량").Trim();
                gloVar.foreignTop[i, 6] = axKHOA.CommGetData(e.sTrCode, "", e.sRQName, i, "순매수량").Trim();//순매수량
            }

            outputHandler("Search done");
        }
        private void eventReceiveForeignTopPrice(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEvent e)
        {
            int ret = axKHOA.SendOrder("주식주문", "0101", gloVar.accountNum, 1, gloVar.foreignTop[0, 1], 10, Int32.Parse(axKHOA.CommGetData(e.sTrCode, "", e.sRQName, 0, "현재가").Trim()), "00", "");
        }
        private void eventReceiveTradeResult(object sender,AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveChejanDataEvent e)
        {
            if(e.sGubun=="0")
            {
                outputHandler("/////////////////////// ORDER RESULT ///////////////////////");
                outputHandler("/// 계좌번호: " + axKHOA.GetChejanData(9201));
                outputHandler("/// 주문번호: " + axKHOA.GetChejanData(9203));
                outputHandler("/// 종목코드: " + axKHOA.GetChejanData(9001));
                outputHandler("/// 종목명: " + axKHOA.GetChejanData(302));
                outputHandler("/// 주문수량: " + axKHOA.GetChejanData(900));
                outputHandler("/// 주문가격: " + axKHOA.GetChejanData(901));
                outputHandler("/// 주문액: " + (Int32.Parse(axKHOA.GetChejanData(900).Trim()) * Int32.Parse(axKHOA.GetChejanData(901).Trim())));
                outputHandler("////////////////////////////////////////////////////////////");
            }
        }

        //Variables
        public static class gloVar
        { 
            public static int nCount { get; set; }
            public static string[,] foreignTop = new string[nCount, 7];
            public static string accountNum;
        };


        //Error Handlers including error events
        public void errorCommRqData(int nRet)
        {
            // TODO: add error codes and stuff , can't find the error code numbers right now
        }
        private void outputHandler(string request)
        {
            dc.ConsoleOutput.Add(request);
        }
        private void OnReceiveMsg(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveMsgEvent e)
        {
            outputHandler(e.sScrNo);
            outputHandler(e.sRQName);
            outputHandler(e.sTrCode);
            outputHandler(e.sMsg);
        }

        //else
        public void Initialize() //Stuff that must be done before searching products or dealing
        {
            gloVar.accountNum = axKHOA.GetLoginInfo("ACCNO").Trim();
        }
    }
}


//TODO: add command history or log something like that
// fix the userinfo thing. It doesn't get the flag 'login state'
