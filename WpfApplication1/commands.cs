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
            outputHandler("User ID: " + axKHOA.GetLoginInfo("USER_ID"));
            outputHandler("User name: " + axKHOA.GetLoginInfo("USER_NAME"));
            outputHandler("Accounts: " + axKHOA.GetLoginInfo("ACCNO"));
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


            //TODO: start dealing using KNN algorithm results
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
                /*
                outputHandler("순위:     " + axKHOA.CommGetData(e.sTrCode, "", e.sRQName, i, "순위").Trim());
                outputHandler("종목코드: " + axKHOA.CommGetData(e.sTrCode, "", e.sRQName, i, "종목코드").Trim());
                outputHandler("종목명:   " + axKHOA.CommGetData(e.sTrCode, "", e.sRQName, i, "종목명").Trim());
                outputHandler("현재가:   " + axKHOA.CommGetData(e.sTrCode, "", e.sRQName, i, "현재가").Trim());
                outputHandler("전일대비: " + axKHOA.CommGetData(e.sTrCode, "", e.sRQName, i, "전일대비").Trim());
                outputHandler("거래량:   " + axKHOA.CommGetData(e.sTrCode, "", e.sRQName, i, "거래량").Trim());
                outputHandler("순매수량: " + axKHOA.CommGetData(e.sTrCode, "", e.sRQName, i, "순매수량").Trim());
                */
                
            }
        }

        //Variables
        public static class gloVar
        { 
            public static int nCount { get; set; }
            public static string[,,] foreignTop = new string[nCount, 7, 15];
        };


        //Error Handlers
        public void errorCommRqData(int nRet)
        {
            // TODO: add error codes and stuff , can't find the error code numbers right now
        }
        private void outputHandler(string request)
        {
            dc.ConsoleOutput.Add(request);
        }
    }
}


//TODO: add command history or log something like that
// fix the userinfo thing. It doesn't get the flag 'login state'
