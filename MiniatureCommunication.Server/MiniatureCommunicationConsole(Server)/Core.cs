using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static MiniatureCommunicationConsole_Server_.Core;

namespace MiniatureCommunicationConsole_Server_
{

	public class Core
	{
		/// <summary>
		/// 20230429
		/// 计划更新内容：
		/// 2、服务端也有私信功能
		/// 3、语音通信功能
		/// 4、一次只能连入一个账户
		/// 
		/// 已更新内容-20230430：
		/// 如果客户端在没验证成功的情况下失去连接，服务端会给别的客户端发送断开信息，这是不允许的
		/// 如果客户端在连接后没有获取旧信息就断开，重连后无法获取之前的旧信息
		/// 客户端连接时间显示到客户端管理列表里
		/// </summary>
		public readonly static string versionStr = "2.12.17.20240405_beta";//版本号信息在此填写,开头不加"V"

		/// <summary>
		/// 存储各文件的路径信息
		/// </summary>
		public static class FilePath
		{
			public readonly static string DataDir = "./MiniatureCommunicationServerData/";
			public readonly static string LogDir = DataDir + "log/";
			public readonly static string MoreLogDir = DataDir + "moreLog/";
			public readonly static string ServerSettingFile = DataDir + "ServerSetting.xml";
			public readonly static string ClientAccountDataFile = DataDir + "ClientAccountData.xml";
		}

		/// <summary>
		/// 存储各类配置属性信息
		/// </summary>
		static public class Config
		{
			public static string OpenIP = "nothing";
			public static string OpenPort = "nothing";
			#region IP和端口设置块
			public static bool HostChecking()//检查ip或端口是否不为空
			{
				if (OpenIP == "" || OpenIP == null)
					return false;
				if (OpenPort == "" || OpenPort == null)
					return false;
				return true;
			}
			static void SetIP(string? input)
			{
				if (input != null)
				{
					OpenIP = input;
					ConsoleOutput.CO("[" + TimeMDHMS + "][控制台信息]" + "已将开放IP设置为: " + OpenIP);
				}
				else
					ConsoleOutput.CO("[" + TimeMDHMS + "][控制台信息]" + "当前开放IP为: " + OpenIP);
			}
			static void SetPort(string? input)
			{
				if (input != null)
				{
					OpenPort = input;
					ConsoleOutput.CO("[" + TimeMDHMS + "][控制台信息]" + "已将开放端口设置为: " + OpenPort);
				}
				else
					ConsoleOutput.CO("[" + TimeMDHMS + "][控制台信息]" + "当前开放端口为: " + OpenPort);

			}
			/// <summary>
			/// 设置IP和端口调用
			/// </summary>
			/// <param name="input">输入的值</param>
			/// <param name="id">调用ID:  0:'/host'命令,同时设置IP和端口; 1:'/ip'命令,设置IP; 2：'/port'命令,设置端口</param>
			public static void SetHost(string? input, int id)
			{
				try
				{
					switch (id)
					{
						case 0:
							if (input != null)
							{
								if (input.IndexOf(":") != -1)
								{
									SetIP(input.Split(":")[0]);
									SetPort(input.Split(":")[1]);
								}
								else goto error;
							}
							else
							{
								SetIP(null);
								SetPort(null);
							}
							break;
						case 1:
							SetIP(input); break;
						case 2:
							SetPort(input); break;
					}
				}
				catch { goto error; }
				DataSave();
				return;
error:;
				ConsoleOutput.CO("[" + TimeMDHMS + "][控制台信息]" + "出现错误！IP或端口设置失败");
				return;
			}
			#endregion
			static void DataSave()
			{
				#region 保存数据块
				XmlDocument xmlDoc = new XmlDocument();
				XmlNodeList xmlNL;
				XmlElement xmlEle;
				xmlDoc.Load(FilePath.ServerSettingFile);
				xmlNL = xmlDoc.SelectSingleNode("ServerSetting").ChildNodes;
				foreach (XmlNode xn in xmlNL) //循环扫描节点
				{
					xmlEle = (XmlElement)xn;
					switch (xmlEle.Name)
					{
						case "IPandPort":
							xmlEle.SetAttribute("ip", Config.OpenIP); //设置节点属性值
							xmlEle.SetAttribute("port", Config.OpenPort);
							break;
						case "AccountVerify":
							switch (Config.AccountVerify)
							{
								case true:
									xmlEle.SetAttribute("bool", "true");
									break;
								case false:
									xmlEle.SetAttribute("bool", "false");
									break;
							}
							break;
						case "moreMessage":
							switch (Config.moreMessage)
							{
								case true:
									xmlEle.SetAttribute("bool", "true");break;
								case false:
									xmlEle.SetAttribute("bool", "false");break;
							}break;
					}
				}
				xmlDoc.Save(FilePath.ServerSettingFile);
			#endregion
			}
			public static string SendChat = "";

			public static bool moreMessage = false;//是否为详细输出
			static public void SetMoreMessage(int input)
			{
				switch (input)
				{
					case -1:
						if (moreMessage)
							ConsoleOutput.CO("[" + TimeMDHMS + "][控制台信息]" + "当前已启用详细输出!");
						else
							ConsoleOutput.CO("[" + TimeMDHMS + "][控制台信息]" + "当前已禁用详细输出!");
						break;
					case 0:
						moreMessage = false;
						ConsoleOutput.CO("[" + TimeMDHMS + "][控制台信息]" + "已禁用详细输出!");
						break;
					case 1:
						moreMessage = true;
						ConsoleOutput.CO("[" + TimeMDHMS + "][控制台信息]" + "已启用详细输出!");
						break;
				}
				if (input == 0 || input == 1) DataSave();
			}
			public static bool AccountVerify;//用户验证
			static public void SetAccountVerify(int input)
			{
				switch(input)
				{
					case -1:
						if(AccountVerify)
						ConsoleOutput.CO("[" + TimeMDHMS + "][控制台信息]" + "当前已启用账户验证!");
						else
							ConsoleOutput.CO("[" + TimeMDHMS + "][控制台信息]" + "当前已禁用账户验证!");
						break;
					case 0:
						AccountVerify = false;
						ConsoleOutput.CO("[" + TimeMDHMS + "][控制台信息]" + "已禁用账户验证!");
						break;
					case 1:
						AccountVerify = true;
						ConsoleOutput.CO("[" + TimeMDHMS + "][控制台信息]" + "已启用账户验证!");
						break;
				}
				if (input == 0 || input == 1) DataSave();
			}

			public static bool CanCloseServer;
			public static bool CanOpenServer=true;
			public static bool CanSendChat;
			public static bool CanClientControl;//控制客户端?
		}

		TcpListener tcpListener;
#pragma warning disable IDE0044 // 添加只读修饰符，根本就不是只读
		Thread clientListenThread; Thread[] readerListenThread = new Thread[10]; Thread[] stayConnectThread = new Thread[10];
		bool clientListenThreadAbort;bool[] readerListenThreadAbort=new bool[10];bool[] stayConnectThreadAbort=new bool[10];

		TcpClient[] tcpClient = new TcpClient[10];
		NetworkStream[] networkStream = new NetworkStream[10];
		BinaryReader[] reader = new BinaryReader[10]; BinaryWriter[] writer = new BinaryWriter[10];

		bool[] clientNumBool = new bool[10];//每个编号是否有对应的客户端连接

		string[] clientIP = new string[10];//每个客户端的ip信息
		string[] clientHost = new string[10];//记录每个客户端的主机地址信息
		string[] clientConnectTime = new string[10];//客户端连接的时间
		string[] clientConnectLastTime = new string[10];//客户端上一次连接的时间
		int[] clientOldChatLine = Enumerable.Repeat(-3, 10).ToArray();//客户端获取旧信息最后一次的行数，下一次获取信息就直接从此行开始，初始化为-3表示空值

		bool[] clientPass = new bool[10];//验证客户端版本以及其身份，判断是否为正确客户端
		int[] clientPassNum = new int[10];//客户端身份通过数量

		string[] clientAccountID = new string[10];
		string[] clientAccountUserName = new string[10];

		bool[] stayConnectBool = new bool[10];//用来确定用户是否保持连接

		bool[] ReaderListenTryLock = new bool[10];//信息读取线程，当终止线程时，会引发错误，但时如果是手动终止，则不需要执行catch后的语句
#pragma warning restore IDE0044 // 添加只读修饰符
		int clientNumStatus = 0;//客户端个数状态，0代表无客户端；1代表有客户端；2代表客户端已满

		System.Collections.ArrayList notRunOldChatLastConnectLastTime = new System.Collections.ArrayList();//如果客户端进入服务器后没有点击请求旧信息，那么最后断开时间应保存在此变量里面，避免客户端重连后就无法获取之前的信息了
		static public string TimeMDHMS
		{
			get
			{
				return DateTime.Now.ToString("yy/MM/dd HH:mm:ss");
			}
		}



		//bool clientListenBool = false;
		void ClientListen()
		{
			clientListenThreadAbort = false;
			
			try
			{
				IPAddress iPAddress = Dns.GetHostAddresses(Config.OpenIP)[0];
				int port = int.Parse(Config.OpenPort);
				tcpListener = new TcpListener(iPAddress, port);
				tcpListener.Start();
				 
				ConsoleOutput.CO("[" + TimeMDHMS + "][系统信息]" + "服务器启动成功！\r\n");
				//}));
				 
				ConsoleOutput.CO("[" + TimeMDHMS + "][系统信息]" + "服务器IP地址：" + Config.OpenIP + " 服务器端口：" + Config.OpenPort + "\r\n");
				//}));
			}
			catch
			{
				//this.Invoke(new Action(() =>{
				Config.CanCloseServer = false;
				Config.CanSendChat = false;

				Config.CanOpenServer = true;

				Config.CanClientControl = false;
				//ipTextBox.Enabled = true;
				//portTextBox.Enabled = true;
				//}));

				 
				ConsoleOutput.CO("[" + TimeMDHMS + "][系统信息]" + "服务器启动失败！可能是IP或端口错误！\r\n");
				//}));
				goto end;
			}
			while (!clientListenThreadAbort)
			{
				if (clientNumStatus != 2)
				{
					if (!tcpListener.Pending())//判断是否有连接请求，避免造成线程堵塞
					{
						Thread.Sleep(5);
					}
					else
					{
						int clientNum = 0;
						for (clientNum = 0; clientNum < clientNumBool.Length; clientNum++)
						{
							if (clientNumBool[clientNum] == false)
							{
								clientNumBool[clientNum] = true;
								
								tcpClient[clientNum] = tcpListener.AcceptTcpClient();

								clientIP[clientNum] = ((IPEndPoint)tcpClient[clientNum].Client.RemoteEndPoint).Address.ToString(); //获取客户端ip地址
								clientHost[clientNum] = ((IPEndPoint)tcpClient[clientNum].Client.RemoteEndPoint).ToString(); //获取客户端ip和端口
								ConsoleOutput.CO("[" + TimeMDHMS + "][系统信息]" + "客户端(编号:" + clientNum + ")"+ clientHost[clientNum] +"正在连接!" + "\r\n", true);

								networkStream[clientNum] = tcpClient[clientNum].GetStream();
								reader[clientNum] = new BinaryReader(networkStream[clientNum]);
								writer[clientNum] = new BinaryWriter(networkStream[clientNum]);

								clientConnectTime[clientNum] = TimeMDHMS;
								 
								ConsoleOutput.CO("[" + clientConnectTime[clientNum] + "][系统信息]" + "客户端" + clientHost[clientNum] +"已连接，客户端编号:" + clientNum + "\r\n");
								//}));

								if (readerListenThread[clientNum] != null && readerListenThread[clientNum].IsAlive == true)
									readerListenThreadAbort[clientNum] = true;
								if (stayConnectThread[clientNum] != null && stayConnectThread[clientNum].IsAlive == true)
									stayConnectThreadAbort[clientNum] = true;
								readerListenThread[clientNum] = new Thread(ReaderListen);
								readerListenThread[clientNum].Start(clientNum);
								stayConnectThread[clientNum] = new Thread(StayConnect);
								stayConnectThread[clientNum].Start(clientNum);

								break;
							}
							//else if (clientNum == clientNumBool.Length && clientNumBool[clientNum] == true)//如果没有客户端空位
							//{                                                              
							//    //考虑增加一个临时空位，然后再将客户端断开连接
							//}
						}
						ClientNumStatusVoid();
						//clientListViewDataRefreshThread = new Thread(ClientListViewDataRefresh);
						//clientListViewDataRefreshThread.Start();
						//ClientListViewDataRefresh();
						//目前支持多客户端
					}
				}
				else if (tcpListener.Pending())
				{
					try//如果服务器已满且有挂起的连接请求，则临时创建一个位置发送服务器已满信息后释放资源
					{
						TcpClient tcpClient2 = tcpListener.AcceptTcpClient();
						NetworkStream networkStream2 = tcpClient2.GetStream();
						BinaryWriter writer2 = new BinaryWriter(networkStream2);
						writer2.Write(EncryptStr(true, "command|full"));
						tcpClient2.Close();
						networkStream2.Close();
						writer2.Close();
					}
					catch
					{
					}
				}
				else
				{ Thread.Sleep(100); }
			}
end:;
			clientListenThreadAbort= false;
		}

		void CloseServerVoid(string id)
		{
			try
			{
				WriterVoid(new string[1] { "ServerClose" }, new string[0], -1);

				if (clientListenThread != null)
				{
					if (clientListenThread.IsAlive == true)
					{
						clientListenThreadAbort=true;
						clientListenThread = null;
					}
				}
				tcpListener?.Stop();
				tcpListener = null;
				for (int clientNum = 0; clientNum < clientNumBool.Length; clientNum++)
				{
					if (clientNumBool[clientNum] == true)
					{
						if (readerListenThread[clientNum] != null)
						{
							if (readerListenThread[clientNum].IsAlive == true)
							{
								readerListenThreadAbort[clientNum]=true;
								readerListenThread[clientNum] = null;
							}
						}
						if (stayConnectThread[clientNum] != null)
						{
							if (stayConnectThread[clientNum].IsAlive == true)
							{
								stayConnectThreadAbort[clientNum]=true;
								stayConnectThread[clientNum] = null;
							}
						}

						tcpClient[clientNum]?.Close();
						networkStream[clientNum]?.Close();
						reader[clientNum]?.Close();
						writer[clientNum]?.Close();
						tcpClient[clientNum] = null;
						networkStream[clientNum] = null;
						writer[clientNum] = null;
						reader[clientNum] = null;
						clientNumBool[clientNum] = false;
						clientPass[clientNum] = false;
						clientPassNum[clientNum] = 0;
						clientIP[clientNum] = null;
						clientHost[clientNum] = null;
						clientConnectTime[clientNum] = null;
						clientConnectLastTime[clientNum] = null;
						clientOldChatLine[clientNum] = -3;
						clientAccountUserName[clientNum] = null;
						clientAccountID[clientNum] = null;

						#region 记录客户端上一次连接的时间
						XmlDocument xmlDoc = new XmlDocument();
						XmlNodeList xmlNL;
						XmlElement xmlEle;
						xmlDoc.Load(FilePath.ClientAccountDataFile);
						xmlNL = xmlDoc.SelectSingleNode("ClientAccountData").ChildNodes;
						foreach (XmlNode xn in xmlNL) //循环扫描节点
						{
							xmlEle = (XmlElement)xn;
							if (xmlEle.Name == "ClientAccount" && clientAccountID[clientNum] == xmlEle.GetAttribute("ID")) //找到名字为其的节点
							{
								xmlEle.SetAttribute("ConnectLastTime", TimeMDHMS); //设置节点属性值
								break;
							}
						}
						xmlDoc.Save(FilePath.ClientAccountDataFile);
						#endregion//注意，有两个地方写了此块代码，如果要更改，必须把另一个地方一起更改
					}
				}

				ClientNumStatusVoid();
				//ClientListViewDataRefresh();

				switch (id)
				{
					case "Button"://如果是关闭服务器按钮按下
						Config.CanCloseServer = false;
						Config.CanSendChat = false;

						Config.CanOpenServer = true;


						Config.CanClientControl = false;
						//ipTextBox.Enabled = true;
						//portTextBox.Enabled = true;

						 
						ConsoleOutput.CO("[" + TimeMDHMS + "][系统信息]" + "已关闭服务器！\r\n");
						//}));
						break;
					case "Close":
						 
						ConsoleOutput.CO("[" + TimeMDHMS + "][系统信息]" + "已退出服务端！\r\n");
						//}));

						ConsoleOutput.Log.Close();

						/*#region 保存数据块
						XmlDocument xmlDoc = new XmlDocument();
						XmlNodeList xmlNL;
						XmlElement xmlEle;
						xmlDoc.Load(FilePath.ServerSettingFile);
						if (xmlDoc.SelectSingleNode("ServerSetting").SelectSingleNode("FormControlState") != null)
						{
							xmlNL = xmlDoc.SelectSingleNode("ServerSetting").SelectSingleNode("FormControlState").ChildNodes;
							foreach (XmlNode xn in xmlNL) //循环扫描节点
							{
								xmlEle = (XmlElement)xn;
								switch (xmlEle.Name)
								{
									case "Form":
										xmlEle.SetAttribute("Left", this.Left.ToString());
										xmlEle.SetAttribute("Top", this.Top.ToString());
										xmlEle.SetAttribute("Width", this.Width.ToString());
										xmlEle.SetAttribute("Height", this.Height.ToString());
										break;
									case "splitChatAndControlPanel":
										xmlEle.SetAttribute("SplitterDistance", splitChatAndControlPanel.SplitterDistance.ToString());
										break;
									case "clientListView":
										xmlEle.SetAttribute("clientIDHeader_Width", clientIDHeader.Width.ToString());
										xmlEle.SetAttribute("clientUserNameHeader_Width", clientUserNameHeader.Width.ToString());
										xmlEle.SetAttribute("clientIPHeader_Width", clientIPHeader.Width.ToString());
										xmlEle.SetAttribute("clientNumHeader_Width", clientNumHeader.Width.ToString());
										xmlEle.SetAttribute("clientConnectTimeHeader_Width", clientConnectTimeHeader.Width.ToString());
										break;
									case "clientAccountListView":
										xmlEle.SetAttribute("accountID_Width", accountID.Width.ToString());
										xmlEle.SetAttribute("accountUserName_Width", accountUserName.Width.ToString());
										xmlEle.SetAttribute("accountState_Width", accountState.Width.ToString());
										xmlEle.SetAttribute("ConnectLastTimeHeader_Width", ConnectLastTimeHeader.Width.ToString());
										xmlEle.SetAttribute("accountActivateTime_Width", accountActivateTime.Width.ToString());
										xmlEle.SetAttribute("accountTime_Width", accountTime.Width.ToString());
										xmlEle.SetAttribute("accountPassword_Width", accountPassword.Width.ToString());
										break;
								}
							}
							xmlDoc.Save(FilePath.ServerSettingFile);
						}

						#endregion*/

						/*while (true)
						{
							if (this.IsHandleCreated)//如果服务端仍在后台运行，则强制退出服务端
							{
								try { Environment.Exit(0); } catch { try { Environment.Exit(0); } catch { System.Diagnostics.Process.GetCurrentProcess().Kill(); } }
								break;
							}
							Thread.Sleep(1);
						}*/
						break;
				}

			}
			catch { }

		}

		void ClientNumStatusVoid()
		{
			int clientNum;
			int trueNum = 0;
			int falseNum = 0;
			for (clientNum = 0; clientNum < clientNumBool.Length; clientNum++)
			{
				if (clientNumBool[clientNum] == true)
				{
					trueNum++;
				}
				else if (clientNumBool[clientNum] == false)
				{
					falseNum++;
				}
			}
			if (trueNum == clientNumBool.Length)
			{
				clientNumStatus = 2;//客户端满
			}
			else if (falseNum == clientNumBool.Length)
			{
				clientNumStatus = 0;//无客户端
			}
			else
			{
				clientNumStatus = 1;//有客户端
			}
		}

		void DisClient(string id, int clientNum)
		{
			string clientInfo;
			if (clientAccountUserName != null)
			{
				clientInfo = clientAccountUserName[clientNum];
			}
			else
			{
				clientInfo = clientIP[clientNum];
			}
			switch (id)
			{
				case "refuse":
					ConsoleOutput.CO("[" + TimeMDHMS + "][系统信息]" + "已拒绝客户端" + "(编号:" + clientNum + ")" + clientInfo + "连接！(可能是客户端与服务端版本不一致)" + "\r\n",true); 
					Thread.Sleep(500);
					break;
				case "refuse2":
					ConsoleOutput.CO("[" + TimeMDHMS + "][系统信息]" + "已拒绝客户端" + "(编号:" + clientNum + ")" + clientInfo + "连接！(客户端信息错误！)" + "\r\n",true);
					break;
				case "refuse3":
					ConsoleOutput.CO("[" + TimeMDHMS + "][系统信息]" + "已拒绝客户端" + "(编号:" + clientNum + ")" + clientInfo + "连接！(账户验证错误：账户不存在！)" + "\r\n",true);
					Thread.Sleep(500);
					break;
				case "refuse4":
					 ConsoleOutput.CO("[" + TimeMDHMS + "][系统信息]" + "已拒绝客户端" + "(编号:" + clientNum + ")" + clientInfo + "连接！(账户验证错误：账户密码错误！)" + "\r\n",true); 
					Thread.Sleep(500);
					break;
				case "refuse5":
				ConsoleOutput.CO("[" + TimeMDHMS + "][系统信息]" + "已拒绝客户端" + "(编号:" + clientNum + ")" + clientInfo + "连接！(账户验证错误：未登录账户！)" + "\r\n",true); 
					Thread.Sleep(500);
					break;
				case "refuse6":
					ConsoleOutput.CO("[" + TimeMDHMS + "][系统信息]" + "已拒绝客户端" + "(编号:" + clientNum + ")" + clientInfo + "连接！(账户验证错误：账户已被禁封！)" + "\r\n",true); 
					Thread.Sleep(500);
					break;
				case "lost":				 
					ConsoleOutput.CO("[" + TimeMDHMS + "][系统信息]" + "客户端" + clientInfo + "失去连接！" + "\r\n", false);
					ConsoleOutput.CO("[" + TimeMDHMS + "][系统信息]" + "客户端" + "(编号:" + clientNum + ")" + clientInfo + "失去连接！" + "\r\n", true);
					break;
				case "clientDis":					 
					ConsoleOutput.CO("[" + TimeMDHMS + "][系统信息]" + "客户端" + clientInfo + "已断开连接！" + "\r\n", false);
					ConsoleOutput.CO("[" + TimeMDHMS + "][系统信息]" + "客户端" + "(编号:" + clientNum + ")" + clientInfo + "已断开连接！" + "\r\n", true);
					break;
				case "clientDisByServer":
					ConsoleOutput.CO("[" + TimeMDHMS + "][系统信息]" + "已将客户端" + "(编号:" + clientNum + ")" + clientInfo + "从服务器断开连接！" + "\r\n",true);
					Thread.Sleep(500);
					break;
				case "kick":					 
					ConsoleOutput.CO("[" + TimeMDHMS + "][系统信息]" + "已将客户端" + clientInfo + "从服务器断开连接！(踢出)" + "\r\n", false);
					ConsoleOutput.CO("[" + TimeMDHMS + "][系统信息]" + "已将客户端" + "(编号:" + clientNum + ")" + clientInfo + "从服务器断开连接！(踢出)" + "\r\n", true);
					Thread.Sleep(500);
					break;
				case "kickBan":					 
					ConsoleOutput.CO("[" + TimeMDHMS + "][系统信息]" + "已将客户端" + clientInfo + "禁封！" + "\r\n", false);
					ConsoleOutput.CO("[" + TimeMDHMS + "][系统信息]" + "已将客户端" + "(编号:" + clientNum + ")" + clientInfo + "禁封！" + "\r\n", true);
					Thread.Sleep(500);
					break;
				case "StayConnectFalse":					 
					ConsoleOutput.CO("[" + TimeMDHMS + "][系统信息]" + "客户端" + clientInfo + "因连接超时而失去连接！" + "\r\n", false);
					ConsoleOutput.CO("[" + TimeMDHMS + "][系统信息]" + "客户端" + "(编号:" + clientNum + ")" + clientInfo + "因连接超时而失去连接！" + "\r\n", true);
					Thread.Sleep(500);
					break;
			}

			tcpClient[clientNum]?.Close();
			networkStream[clientNum]?.Close();
			writer[clientNum]?.Close();
			reader[clientNum]?.Close();
			tcpClient[clientNum] = null;
			networkStream[clientNum] = null;
			writer[clientNum] = null;
			reader[clientNum] = null;
			clientNumBool[clientNum] = false;
			clientPass[clientNum] = false;
			clientPassNum[clientNum] = 0;

			if (clientNumStatus != 0)
			{
				if (id == "kick")
				{
					WriterVoid(new string[1] { "clientStatus" }, new string[2] { "kick", clientInfo }, clientNum);
				}
				else if (id == "kickBan")
				{
					WriterVoid(new string[1] { "clientStatus" }, new string[2] { "kickBan", clientInfo }, clientNum);
				}
				else if (id == "StayConnectFalse")
				{
					WriterVoid(new string[1] { "clientStatus" }, new string[2] { "StayConnectFalse", clientInfo }, clientNum);
				}
				else if (/*clientPass[clientNum] == true &&*/ id != "refuse" && id != "refuse2" && id != "refuse3" && id != "refuse4" && id != "refuse5" && id != "refuse6" && id != "clientDisByServer")//发送断开连接的客户端ip地址
				{ WriterVoid(new string[1] { "clientStatus" }, new string[2] { "dis", clientInfo }, clientNum); }

			}

			#region 记录客户端上一次连接的时间
			XmlDocument xmlDoc = new XmlDocument();
			XmlNodeList xmlNL;
			XmlElement xmlEle;
			xmlDoc.Load(FilePath.ClientAccountDataFile);
			xmlNL = xmlDoc.SelectSingleNode("ClientAccountData").ChildNodes;
			foreach (XmlNode xn in xmlNL) //循环扫描节点
			{
				xmlEle = (XmlElement)xn;
				if (xmlEle.Name == "ClientAccount" && clientAccountID[clientNum] == xmlEle.GetAttribute("ID")) //找到名字为其的节点
				{
					xmlEle.SetAttribute("ConnectLastTime", TimeMDHMS); //设置节点属性值
					break;
				}
			}
			xmlDoc.Save(FilePath.ClientAccountDataFile);
			#endregion//注意，有两个地方写了此块代码，如果要更改，必须把另一个地方一起更改

			ClientNumStatusVoid();
			//ClientListViewDataRefresh();

			clientIP[clientNum] = null;
			clientHost[clientNum] = null;
			clientConnectTime[clientNum] = null;
			clientConnectLastTime[clientNum] = null;
			clientOldChatLine[clientNum] = -3;
			clientAccountUserName[clientNum] = null;
			clientAccountID[clientNum] = null;

			readerListenThreadAbort[clientNum]=true;
			stayConnectThreadAbort[clientNum]=true;
		}


		void StayConnect(object threadId)
		{
			int clientNum = (int)threadId;
			stayConnectThreadAbort[(int)threadId] = false;
			while (!stayConnectThreadAbort[(int)threadId])
			{
				for(int i = 0; i < 12; i++)//每12秒检查一次连接状态
				{
					Thread.Sleep(1000);
					if (stayConnectThreadAbort[(int)threadId]) goto end;//每一秒都判断是否该终止线程
				}
				if (stayConnectBool[clientNum] == false && !stayConnectThreadAbort[(int)threadId] && writer[clientNum] != null)
				{
					WriterVoid(new string[1] { "StayConnectFalse" }, null, clientNum);
					break;
				}
				else
				{
					stayConnectBool[clientNum] = false;
				}
			}
end:;
			stayConnectThreadAbort[(int)threadId] = false;
		}
		#region"信息接受和发送事件"        
		void WriterVoid(string[] id, string[] sendStr, int clientNum)
		{
			int clientNumInner;
			if (writer!= null && clientNumStatus != 0)
			{
				try
				{
					switch (id[0])
					{
						case "ChatSend":
							ConsoleOutput.CO("[" + TimeMDHMS + "][Server]" + Config.SendChat + "\r\n");
							for (clientNumInner = 0; clientNumInner < clientNumBool.Length; clientNumInner++)
							{
								if (clientNumBool[clientNumInner] == true)
								{
									writer[clientNumInner].Write(EncryptStr(true,"serverChat|" + Config.SendChat));
								}
							}
							//this.Invoke(new Action(() =>{
							Config.SendChat = "";
							//}));
							break;
						case "PrivateClientChat":
							switch (id[1])
							{
								case "Send":
									for (clientNumInner = 0; clientNumInner < clientNumBool.Length; clientNumInner++)
									{
										if (clientNumBool[clientNumInner] == true && clientAccountUserName[clientNumInner] == sendStr[0])
										{
											writer[clientNumInner].Write(EncryptStr(true,"PrivateClientChat|" + "[" + sendStr[0] + "][私信至" + sendStr[1] + "]" + sendStr[2]));
										}
										else
										{
											for (int i = 0; i < sendStr[1].Split(char.Parse(",")).Length; i++)
											{
												if (clientNumBool[clientNumInner] == true && clientAccountUserName[clientNumInner] == sendStr[1].Split(char.Parse(","))[i])
												{
													writer[clientNumInner].Write(EncryptStr(true,"PrivateClientChat|" + "[" + sendStr[0] + "][私信至" + sendStr[1] + "]" + sendStr[2]));
													break;
												}
											}
										}
									}
									break;
								case "NotExist":
									writer[clientNum].Write(EncryptStr(true,"PrivateClientChatError|" + "[" + sendStr[0] + "][发送失败][私信至" + sendStr[1] + "(用户" + sendStr[2] + "不存在)]" + sendStr[3]));
									break;
							}
							break;
						case "ClientChat":
							for (clientNumInner = 0; clientNumInner < clientNumBool.Length; clientNumInner++)
							{
								if (clientNumBool[clientNumInner] == true && clientNumInner != clientNum)
								{
									writer[clientNumInner].Write(EncryptStr(true,"clientChat|" + "[" + sendStr[0] + "]" + sendStr[1]));
								}
							}
							break;
						case "ServerClose":
							for (clientNumInner = 0; clientNumInner < clientNumBool.Length; clientNumInner++)
							{
								if (clientNumBool[clientNumInner] == true)
								{
									writer[clientNumInner].Write(EncryptStr(true,"command|ServerClose"));
								}
							}
							break;
						case "versionNotPass":
							writer[clientNum].Write(EncryptStr(true,"command|versionNotPass"));
							ReaderListenTryLock[clientNum] = true;
							DisClient("refuse", clientNum);
							break;
						case "clientStatus":
							switch (sendStr[0])
							{
								case "connect":
									for (clientNumInner = 0; clientNumInner < clientNumBool.Length; clientNumInner++)
									{
										if (clientNumBool[clientNumInner] == true && clientNumInner != clientNum)
										{
											writer[clientNumInner].Write(EncryptStr(true,"clientConnect|" + sendStr[1]));//发送客户端ip地址
										}
									}
									break;
								case "dis":
									for (clientNumInner = 0; clientNumInner < clientNumBool.Length; clientNumInner++)
									{
										if (clientNumBool[clientNumInner] == true && clientNumInner != clientNum)
										{
											writer[clientNumInner].Write(EncryptStr(true,"clientDis|" + sendStr[1]));//发送客户端ip地址
										}
									}
									break;
								case "kick":
									for (clientNumInner = 0; clientNumInner < clientNumBool.Length; clientNumInner++)
									{
										if (clientNumBool[clientNumInner] == true && clientNumInner != clientNum)
										{
											writer[clientNumInner].Write(EncryptStr(true,"clientKick|" + sendStr[1]));//发送客户端ip地址
										}
									}
									break;
								case "kickBan":
									for (clientNumInner = 0; clientNumInner < clientNumBool.Length; clientNumInner++)
									{
										if (clientNumBool[clientNumInner] == true && clientNumInner != clientNum)
										{
											writer[clientNumInner].Write(EncryptStr(true,"clientKickBan|" + sendStr[1]));
										}
									}
									break;
								case "StayConnectFalse":
									for (clientNumInner = 0; clientNumInner < clientNumBool.Length; clientNumInner++)
									{
										if (clientNumBool[clientNumInner] == true && clientNumInner != clientNum)
										{
											writer[clientNumInner].Write(EncryptStr(true,"clientStayConnectFalse|" + sendStr[1]));//发送客户端ip地址
										}
									}
									break;
							}
							break;
						case "activateAccount":
							switch (id[1])
							{
								case "succeed":
									writer[clientNum].Write(EncryptStr(true,"command|activateAccount-succeed"));
									break;
								case "fail":
									break;
								case "IDnot":
									writer[clientNum].Write(EncryptStr(true,"command|activateAccount-IDnot"));
									break;
								case "IDStartTrue":
									writer[clientNum].Write(EncryptStr(true,"command|activateAccount-IDStartTrue"));
									break;
								case "UserNameDuplication":
									writer[clientNum].Write(EncryptStr(true,"command|activateAccount-UserNameDuplication"));
									break;
							}
							ReaderListenTryLock[clientNum] = true;
							DisClient("clientDisByServer", clientNum);
							break;
						case "accountVerify":
							switch (id[1])
							{
								case "false":
									writer[clientNum].Write(EncryptStr(true,"command|accountVerify-false"));
									ReaderListenTryLock[clientNum] = true;
									DisClient("refuse5", clientNum);
									break;
								case "null":
									writer[clientNum].Write(EncryptStr(true,"command|accountVerify-null"));
									ReaderListenTryLock[clientNum] = true;
									DisClient("refuse3", clientNum);
									break;
								case "passwordError":
									writer[clientNum].Write(EncryptStr(true,"command|accountVerify-passwordError"));
									ReaderListenTryLock[clientNum] = true;
									DisClient("refuse4", clientNum);
									break;
								case "ban":
									writer[clientNum].Write(EncryptStr(true,"command|accountVerify-ban"));
									ReaderListenTryLock[clientNum] = true;
									DisClient("refuse6", clientNum);
									break;
							}
							break;
						case "passSucceed":
							writer[clientNum].Write(EncryptStr(true,"command|passSucceed"));
							break;
						case "kick":
							writer[clientNum].Write(EncryptStr(true,"command|kick"));
							ReaderListenTryLock[clientNum] = true;
							DisClient("kick", clientNum);
							break;
						case "kickBan":
							writer[clientNum].Write(EncryptStr(true,"command|kickBan"));
							ReaderListenTryLock[clientNum] = true;
							DisClient("kickBan", clientNum);
							break;
						case "StayConnectFalse":
							writer[clientNum].Write(EncryptStr(true,"command|StayConnectFalse"));
							ReaderListenTryLock[clientNum] = true;
							DisClient("StayConnectFalse", clientNum);
							break;
						case "clientNum":
							writer[clientNum].Write(EncryptStr(true,"clientNum|" + sendStr[0]));
							break;
						case "oldChat":
							writer[clientNum].Write(EncryptStr(true,"oldChat|" + sendStr[0]));
							break;
					}
				}
				catch { }
			}
			else
			{
				switch (id[0])
				{
					case "ChatSend":
						ConsoleOutput.CO("[" + TimeMDHMS + "][Server][当前未与任何设备连接]" + Config.SendChat + "\r\n");
						//this.Invoke(new Action(() =>{
						Config.SendChat = "";
						//}));
						break;
				}
			}

		}
		void ReaderListen(object threadId)
		{
			readerListenThreadAbort[(int)threadId] = false;

			// bool clientDisconnectionBool=false;//判断是否已经接收到客户端发来的断连信息
			string strReader, strReaderId, strReaderMessage = null;
			int clientNum = (int)threadId;
			bool errorLock = false;//提前终止线程会导致错误，但是终止线程导致的错误不需要处理
			bool clientActivateAccount = false;//判断客户端是否是为了激活账户而连接服务器
			string[] AccountMessage = new string[3];//如果客户端申请注册账户，则使用此变量；验证账户也使用此变量
			ReaderListenTryLock[clientNum] = false;

			while (!readerListenThreadAbort[(int)threadId])
			{
				try
				{
					strReader =EncryptStr(false, reader[clientNum].ReadString());
					if (strReader != null)
					{
						#region 信息处理  将命令信息和主要信息分割开
						strReaderId = strReader.Split(char.Parse("|"))[0];
						if (strReader.Split(char.Parse("|")).Length == 2)
						{
							strReaderMessage = strReader.Split(char.Parse("|"))[1];
						}
						else
						{
							for (int i = 1; i < strReader.Split(char.Parse("|")).Length; i++)
							{
								strReaderMessage += strReader.Split(char.Parse("|"))[i];
							}
						}
						#endregion

						if (clientPass[clientNum] == true)
						{
							switch (strReaderId)
							{
								case "chat":
									if (clientAccountUserName[clientNum] != null)
									{
										 
										ConsoleOutput.CO("[" + TimeMDHMS + "][客户端" + clientAccountUserName[clientNum] + "的消息]" + strReaderMessage + "\r\n", false);
										ConsoleOutput.CO("[" + TimeMDHMS + "][客户端" + "(编号:" + clientNum + ")" + clientAccountUserName[clientNum] + "的消息]" + strReaderMessage + "\r\n", true);
										//}));
										WriterVoid(new string[1] { "ClientChat" }, new string[2] { clientAccountUserName[clientNum], strReaderMessage }, clientNum);
									}
									else
									{
										 
										ConsoleOutput.CO("[" + TimeMDHMS + "][客户端" + clientIP[clientNum] + "的消息]" + strReaderMessage + "\r\n", false);
										ConsoleOutput.CO("[" + TimeMDHMS + "][客户端" + "(编号:" + clientNum + ")" + clientIP[clientNum] + "的消息]" + strReaderMessage + "\r\n", true);
										//}));
										WriterVoid(new string[1] { "ClientChat" }, new string[2] { clientIP[clientNum], strReaderMessage }, clientNum);
									}

									break;
								case "command":
									switch (strReaderMessage)
									{
										case "Disconnection":
											DisClient("clientDis", clientNum);
											break;
										case "StayConnect":
											stayConnectBool[clientNum] = true;
											break;
										case "clientNum":
											string tempStr;
											int tempClientNum = 0;
											string tempClientUserName = null;
											for (int i = 0; i < clientNumBool.Length; i++)
											{
												if (clientNumBool[i] == true)
												{
													if (i != 0) { tempClientUserName += "\r\n"; }

													tempClientNum++;
													if (clientAccountUserName[i] != null)
													{
														tempClientUserName += clientAccountUserName[i];
													}
													else
													{
														tempClientUserName += clientIP[i];
													}
												}
											}
											tempStr = "当前客户端人数：" + tempClientNum + "\r\n" + tempClientUserName;

											WriterVoid(new string[1] { "clientNum" }, new string[1] { tempStr }, clientNum);

											if (clientAccountUserName[clientNum] != null)
											{
												 
												ConsoleOutput.CO("[" + TimeMDHMS + "][系统信息]客户端" + clientAccountUserName[clientNum] + "请求查看当前客户端数量\r\n", false);
												ConsoleOutput.CO("[" + TimeMDHMS + "][系统信息]客户端" + "(编号:" + clientNum + ")" + clientAccountUserName[clientNum] + "请求查看当前客户端数量\r\n" + tempStr + "\r\n", true);
												//}));

											}
											else
											{
												 
												ConsoleOutput.CO("[" + TimeMDHMS + "][系统信息]客户端" + clientIP[clientNum] + "请求查看当前客户端数量\r\n", false);
												ConsoleOutput.CO("[" + TimeMDHMS + "][系统信息]客户端" + "(编号:" + clientNum + ")" + clientIP[clientNum] + "请求查看当前客户端数量\r\n" + tempStr + "\r\n", true);
												//}));
											}

											break;
										case "oldChat":
											if (clientAccountUserName[clientNum] != null)
											{
												for (int i = 0; i < ConsoleOutput.listMessage.Count; i++)
												{
													try
													{
														if (ConsoleOutput.listMessage[i].Split(char.Parse("]"))[0].IndexOf(clientConnectTime[clientNum]) != -1 &&
														ConsoleOutput.listMessage[i].Split(char.Parse("]"))[1].IndexOf("系统信息") != -1 &&
														ConsoleOutput.listMessage[i].Split(char.Parse("]"))[2].IndexOf("客户端已连接") != -1)
														{
															string output = null;
															int maxLine = 9;//最多扫描10行
															int i2;//找到客户端连接一行，引入新的变量，往前搜索
															if (clientOldChatLine[clientNum] != -3)//如果客户端上一次获取过旧信息，则从上次获取的行数继续获取，-3值表示没有
															{
																i2 = clientOldChatLine[clientNum];
																i = i2;
															}
															else
															{
																i2 = i;
															}

															string tempCclt = null;
															for (int nroclclt = 0; nroclclt < notRunOldChatLastConnectLastTime.Count; nroclclt += 3)
															{
																if (notRunOldChatLastConnectLastTime[nroclclt].ToString() == clientAccountUserName[clientNum])
																{
																	tempCclt = notRunOldChatLastConnectLastTime[nroclclt + 2].ToString();
																	notRunOldChatLastConnectLastTime[nroclclt + 1] = "true";
																	break;
																}
															}

															while (i - i2 <= maxLine && i2 >= 0)
															{
																try
																{
																	//Console.WriteLine(messageBox.Lines[i2].Split(char.Parse("]"))[0].Substring(1).Replace("/", "-"));
																	if (ConsoleOutput.listMessage[i2].Split(char.Parse("]"))[1].IndexOf("系统信息") == -1 &&
																	  (DateTime.Compare(
																		  Convert.ToDateTime(ConsoleOutput.listMessage[i2].Split(char.Parse("]"))[0].Substring(1).Replace("/", "-")),
																	  Convert.ToDateTime(tempCclt.Replace("/", "-"))) > 0) == true)
																	{
																		if (ConsoleOutput.listMessage[i2].Split(char.Parse("]"))[1].Substring(1, 3) == "客户端" &&
																			ConsoleOutput.listMessage[i2].Split(char.Parse("]"))[1].IndexOf("的") != -1 &&
																			ConsoleOutput.listMessage[i2].Split(char.Parse("]"))[1].IndexOf("消息") != -1)
																		{
																			if (ConsoleOutput.listMessage[i2].Split(char.Parse("]"))[1].Substring(ConsoleOutput.listMessage[i2].Split(char.Parse("]"))[1].Length - 4, 2) == "私信"
																				&& ConsoleOutput.listMessage[i2].Split(char.Parse("]"))[2] != "[私信失败")
																			{
																				string[] uName = new string[ConsoleOutput.listMessage[i2].Split(char.Parse("]"))[2].Substring(4).Split(char.Parse(",")).Length];
																				for (int un = 0; un < uName.Length; un++)
																				{
																					uName[un] = ConsoleOutput.listMessage[i2].Split(char.Parse("]"))[2].Substring(4).Split(char.Parse(","))[un];
																				}
																				for (int un = 0; un < uName.Length; un++)
																				{
																					if (uName[un] == clientAccountUserName[clientNum])
																					{
																						string tempStr2 = null;
																						//if (output != null) { tempStr2 += "\r\n"; }  //注意，此处取消额外添加回车符，因为将历史信息导入List变量后回车符也会一起进入List变量内，所以不用额外回车
																						tempStr2 += ConsoleOutput.listMessage[i2].Split(char.Parse("]"))[0] + "]" +
																								"[" + ConsoleOutput.listMessage[i2].Split(char.Parse("]"))[1].Substring(4, ConsoleOutput.listMessage[i2].Split(char.Parse("]"))[1].Length - 4 - 3 - 2) + "]" +
																								ConsoleOutput.listMessage[i2].Substring(
																									(ConsoleOutput.listMessage[i2].Split(char.Parse("]"))[0] + "]" + ConsoleOutput.listMessage[i2].Split(char.Parse("]"))[1] + "]")
																									  .Length);
																						output += tempStr2;

																						goto end;//找到与客户端相关的私信信息后，直接跳出
																					}
																				}
																				maxLine++;//如果运行到此，就说明该私信信息和此客户端无关，所以再加一条获取信息
end:;
																			}
																			else if (ConsoleOutput.listMessage[i2].Split(char.Parse("]"))[2] != "[私信失败")
																			{
																				string tempStr2 = null;
																				//if (output != null) { tempStr2 += "\r\n"; }
																				tempStr2 += ConsoleOutput.listMessage[i2].Split(char.Parse("]"))[0] + "]" +
																					  "[" + ConsoleOutput.listMessage[i2].Split(char.Parse("]"))[1].Substring(4, ConsoleOutput.listMessage[i2].Split(char.Parse("]"))[1].Length - 4 - 3) + "]" +
																					  ConsoleOutput.listMessage[i2].Substring(
																						  (ConsoleOutput.listMessage[i2].Split(char.Parse("]"))[0] + "]" + ConsoleOutput.listMessage[i2].Split(char.Parse("]"))[1] + "]")
																						  .Length);
																				output += tempStr2;//+ output;
																			}
																		}
																		else if (ConsoleOutput.listMessage[i2].Split(char.Parse("]"))[1].IndexOf("Server") != -1)
																		{
																			string tempStr2 = null;
																			//if (output != null) { tempStr2 += "\r\n"; }
																			if (ConsoleOutput.listMessage[i2].Split(char.Parse("]"))[2] != "[当前未与任何设备连接")
																			{
																				tempStr2 += ConsoleOutput.listMessage[i2].Split(char.Parse("]"))[0] + "]" +
																				   "[Server]" +
																				   ConsoleOutput.listMessage[i2].Substring((ConsoleOutput.listMessage[i2].Split(char.Parse("]"))[0] + "]" + "[Server]").Length);
																			}
																			else
																			{
																				tempStr2 += ConsoleOutput.listMessage[i2].Split(char.Parse("]"))[0] + "]" +
																			   "[Server]" +
																			   ConsoleOutput.listMessage[i2].Substring((ConsoleOutput.listMessage[i2].Split(char.Parse("]"))[0] + "]" + "[Server][当前未与任何设备连接]").Length);
																			}
																			output += tempStr2;//+ output;
																		}
																	}
																	else if ((DateTime.Compare(Convert.ToDateTime(
																		ConsoleOutput.listMessage[i2].Split(char.Parse("]"))[0].Substring(1).Replace("/", "-")),
																		Convert.ToDateTime(tempCclt.Replace("/", "-"))) > 0) == false)
																	{
																		break;
																	}
																	else
																	{
																		maxLine++;
																	}
																	i2--;
																}
																catch { i2--; maxLine++; }
															}
															//Console.WriteLine("output" + output);
															clientOldChatLine[clientNum] = i2;
															WriterVoid(new string[1] { "oldChat" }, new string[1] { output }, clientNum);

															string tempOutputStr = null;//处理输出到详细信息的旧信息
															tempOutputStr += "<输出到客户端的旧信息>" + "\r\n";
															if (output != null)
															{
																for (int j = output.Split(char.Parse("\r")).Length - 1; j >= 0; j--)
																{
																	tempOutputStr += "[输出到客户端的旧信息]" + output.Split(char.Parse("\r"))[j] + "\r\n";
																}
															}
															else
															{
																tempOutputStr += "无信息" + "\r\n";
															}
															tempOutputStr += "</输出到客户端的旧信息>" + "\r\n";
															 
															ConsoleOutput.CO("[" + TimeMDHMS + "][系统信息]客户端" + clientAccountUserName[clientNum] + "请求获取旧信息" + "\r\n", false);
															ConsoleOutput.CO("[" + TimeMDHMS + "][系统信息]客户端" + "(编号:" + clientNum + ")" + clientAccountUserName[clientNum] + "请求获取旧信息" + "\r\n" + tempOutputStr, true);
															//}));
															break;
														}
													}
													catch { }
												}
											}
											break;
									}
									break;
								#region 激活账户
								case "activateAccount-ID":
								case "activateAccount-UserName":
								case "activateAccount-Password":
									if (AccountMessage[0] == null && AccountMessage[1] == null && AccountMessage[2] == null)
									{
										 
										ConsoleOutput.CO("[" + TimeMDHMS + "][系统信息]客户端" + clientIP[clientNum] + "正在激活账户" + "\r\n", false);
										ConsoleOutput.CO("[" + TimeMDHMS + "][系统信息]客户端" + "(编号:" + clientNum + ")" + clientIP[clientNum] + "正在激活账户" + "\r\n", true);
										//}));
									}

									switch (strReaderId)
									{
										case "activateAccount-ID":
											AccountMessage[0] = strReaderMessage;
											break;
										case "activateAccount-UserName":
											AccountMessage[1] = strReaderMessage;
											break;
										case "activateAccount-Password":
											AccountMessage[2] = strReaderMessage;
											break;
									}

									if (AccountMessage[0] != null && AccountMessage[1] != null && AccountMessage[2] != null)
									{
										string output = "IDnot";//判断是否操作成功
										XmlDocument xmlDoc = new XmlDocument();
										XmlNodeList xmlNL;
										XmlElement xmlEle;
										xmlDoc.Load(FilePath.ClientAccountDataFile);
										xmlNL = xmlDoc.SelectSingleNode("ClientAccountData").ChildNodes;
										foreach (XmlNode xn in xmlNL) //循环扫描节点
										{
											xmlEle = (XmlElement)xn;

											if (xmlEle.GetAttribute("UserName") == AccountMessage[1])
											{
												output = "UserNameDuplication";
												break;
											}
											else if (xmlEle.Name == "ClientAccount" && xmlEle.GetAttribute("ID") == AccountMessage[0])
											{
												if (xmlEle.GetAttribute("State") == "false")
												{
													xmlEle.SetAttribute("UserName", AccountMessage[1]); //设置节点属性值
													xmlEle.SetAttribute("Password", AccountMessage[2]);
													xmlEle.SetAttribute("State", "true");
													xmlEle.SetAttribute("ActivateTime", System.DateTime.Now.ToString());
													output = "succeed";
													break;
												}
												else if (xmlEle.GetAttribute("State") == "true")
												{
													output = "IDStartTrue";
													break;
												}

											}
										}
										xmlDoc.Save(FilePath.ClientAccountDataFile);

										string tempPwStr = null;
										for (int i = 0; i < AccountMessage[2].Length; i++)
										{
											tempPwStr += "*";
										}
										 
										ConsoleOutput.CO("[" + TimeMDHMS + "][系统信息]客户端" + "(编号:" + clientNum + ")" + clientIP[clientNum] + "输入的被激活账户的信息: " +
										"[激活ID: " + AccountMessage[0] +
										"][用户名: " + AccountMessage[1] +
										"][密码: " + tempPwStr
										+ "]\r\n", true);
										//}));
										switch (output)
										{
											case "succeed":
												 ConsoleOutput.CO("[" + TimeMDHMS + "][系统信息]" + "客户端" + "(编号:" + clientNum + ")" + clientIP[clientNum] + "的申请，" + "为" + AccountMessage[0] + "的账户ID激活成功！" + "\r\n",true);
												//AccountListViewDataRefresh();
												WriterVoid(new string[2] { "activateAccount", "succeed" }, null, clientNum);
												break;
											case "fail":
												break;
											case "IDnot":
												 ConsoleOutput.CO("[" + TimeMDHMS + "][系统信息]" + "客户端" + "(编号:" + clientNum + ")" + clientIP[clientNum] + "的申请，" + "为" + AccountMessage[0] + "的账户ID激活失败！原因：没有找到此ID" + "\r\n",true);
												WriterVoid(new string[2] { "activateAccount", "IDnot" }, null, clientNum);
												break;
											case "IDStartTrue":
												ConsoleOutput.CO("[" + TimeMDHMS + "][系统信息]" + "客户端" + "(编号:" + clientNum + ")" + clientIP[clientNum] + "的申请，" + "为" + AccountMessage[0] + "的账户ID激活失败！原因：ID已被注册" + "\r\n",true);
												WriterVoid(new string[2] { "activateAccount", "IDStartTrue" }, null, clientNum);
												break;
											case "UserNameDuplication":
												ConsoleOutput.CO("[" + TimeMDHMS + "][系统信息]" + "客户端" + "(编号:" + clientNum + ")" + clientIP[clientNum] + "的申请，" + "为" + AccountMessage[0] + "的账户ID激活失败！原因：用户名" + AccountMessage[1] + "与现有用户名重复" + "\r\n",true);
												WriterVoid(new string[2] { "activateAccount", "UserNameDuplication" }, null, clientNum);
												break;
										}
									}
									break;
								#endregion
								default:
									if (strReaderId.IndexOf("chat") != -1)
									{
										string[] userName = new string[strReaderId.Split(char.Parse("@")).Length - 1];
										bool[] unameExists = new bool[userName.Length];//判断用户名是否存在
										for (int i = 0; i < userName.Length; i++)
										{
											userName[i] = strReaderId.Split(char.Parse("@"))[i + 1];
										}
										XmlDocument xmlDoc = new XmlDocument();
										XmlNode xmlRoot;
										xmlDoc.Load(FilePath.ClientAccountDataFile);
										xmlRoot = xmlDoc.SelectSingleNode("ClientAccountData"); //进入对应节点
										XmlNodeList xmlNL = xmlRoot.ChildNodes;
										foreach (XmlNode xn in xmlNL) //循环扫描节点
										{
											XmlElement xmlE = (XmlElement)xn;

											for (int i = 0; i < userName.Length; i++)
											{
												if (userName[i] == xmlE.GetAttribute("UserName") && xmlE.GetAttribute("State") != "false")
												{
													unameExists[i] = true;
													break;
												}
											}
										}
										string notExistUname = null;
										string allUname = null;
										for (int i = 0; i < unameExists.Length; i++)
										{
											if (allUname != null) { allUname += ","; }
											allUname += userName[i];
											if (unameExists[i] == false)
											{
												if (notExistUname != null) { notExistUname += ","; }
												notExistUname += userName[i];
											}

										}
										if (notExistUname == null)
										{
											 
											ConsoleOutput.CO("[" + TimeMDHMS + "][客户端" + clientAccountUserName[clientNum] + "的私信消息]" + "[私信至" + allUname + "]" + strReaderMessage + "\r\n", false);
											ConsoleOutput.CO("[" + TimeMDHMS + "][客户端" + "(编号:" + clientNum + ")" + clientAccountUserName[clientNum] + "的私信消息]" + "[私信至" + allUname + "]" + strReaderMessage + "\r\n", true);
											//}));
											WriterVoid(new string[2] { "PrivateClientChat", "Send" }, new string[3] { clientAccountUserName[clientNum], allUname, strReaderMessage }, clientNum);
										}
										else
										{
											 
											ConsoleOutput.CO("[" + TimeMDHMS + "][客户端" + clientAccountUserName[clientNum] + "的私信消息]" + "[私信失败][私信至" + allUname + "(用户" + notExistUname + "不存在)]" + strReaderMessage + "\r\n", false);
											ConsoleOutput.CO("[" + TimeMDHMS + "][客户端" + "(编号:" + clientNum + ")" + clientAccountUserName[clientNum] + "的私信消息]" + "[私信失败][私信至" + allUname + "(用户" + notExistUname + "不存在)]" + strReaderMessage + "\r\n", true);
											//}));
											WriterVoid(new string[2] { "PrivateClientChat", "NotExist" }, new string[4] { clientAccountUserName[clientNum], allUname, notExistUname, strReaderMessage }, clientNum);
										}

									}
									break;
							}
						}
						else
						{
							switch (strReaderId)
							{
								case "version":									 
									ConsoleOutput.CO("[" + TimeMDHMS + "][系统信息]" + "客户端(编号:" + clientNum + ")版本：" + strReaderMessage + "\r\n", true);

									if (strReaderMessage.Split(char.Parse("."))[0] + strReaderMessage.Split(char.Parse("."))[1] ==
								   versionStr.Split(char.Parse("."))[0] + versionStr.Split(char.Parse("."))[1])
									{
										clientPassNum[clientNum] += 1;
									}
									else
									{
										WriterVoid(new string[1] { "versionNotPass" }, new string[0], clientNum);//客户端版本不匹配断开连接                                          
									}
									break;
								case "clientIP-activateAccount":
								case "clientIP":								 
									//ConsoleOutput.CO("[" + TimeMDHMS + "][系统信息]" + "客户端(编号:" + clientNum + ")IP地址：" + strReaderMessage + "\r\n", true);
									//clientIP[clientNum] = strReaderMessage;
									//暂时禁用这愚蠢的客户端IP获取方式，该方式只能获取客户端的局域网IP
									clientPassNum[clientNum]++;

									if (strReaderId == "clientIP-activateAccount")
									{
										clientActivateAccount = true;
										clientPassNum[clientNum]++;//如果客户端是为了注册账户而连接，则直接跳过账户验证
									}

									//clientListViewDataRefreshThread?.Abort();
									//  clientListViewDataRefreshThread = new Thread(ClientListViewDataRefresh);
									//clientListViewDataRefreshThread.Start();
									//ClientListViewDataRefresh();
									break;
								case "AccountVerify-TF":
									if (strReaderMessage == "false")
									{
										if (Config.AccountVerify == false)
										{
											clientPassNum[clientNum]++;//如果关闭了账户验证，则直接通过账户验证块
										}
										else
										{
											WriterVoid(new string[2] { "accountVerify", "false" }, null, clientNum);//如果没有登录账户，则断开连接

										}
									}
									break;
								case "AccountVerify-UserName":
								case "AccountVerify-Password":
									switch (strReaderId)
									{
										case "AccountVerify-UserName":
											AccountMessage[0] = strReaderMessage; break;
										case "AccountVerify-Password":
											AccountMessage[1] = strReaderMessage; break;
									}
									if (AccountMessage[0] != null && AccountMessage[1] != null)
									{
										string output = "null";

										XmlDocument xmlDoc = new XmlDocument();
										XmlNode xmlRoot;
										xmlDoc.Load(FilePath.ClientAccountDataFile);
										xmlRoot = xmlDoc.SelectSingleNode("ClientAccountData"); //进入对应节点
										XmlNodeList xmlNL = xmlRoot.ChildNodes;
										foreach (XmlNode xn in xmlNL) //循环扫描节点
										{
											XmlElement xmlE = (XmlElement)xn;
											switch (xmlE.Name)
											{
												case "ClientAccount":
													if (AccountMessage[0] == xmlE.GetAttribute("UserName") && xmlE.GetAttribute("State") == "true")
													{
														if (AccountMessage[1] == xmlE.GetAttribute("Password"))
														{
															output = "true";
															AccountMessage[2] = xmlE.GetAttribute("ID");
														}
														else
														{
															output = "passwordError";
														}
														goto overFor;
													}
													else if (AccountMessage[0] == xmlE.GetAttribute("UserName") && xmlE.GetAttribute("State") == "ban")
													{
														output = "ban";
													}
													break;
											}
										}
overFor:;
										string tempPwStr = null;
										for (int i = 0; i < AccountMessage[1].Length; i++)
										{
											tempPwStr += "*";
										}
										 
										ConsoleOutput.CO("[" + TimeMDHMS + "][系统信息]" + "客户端(编号:" + clientNum + ")输入的账户信息：用户名:" + AccountMessage[0] + ";密码:" + tempPwStr + "\r\n", true);
										//}));
										switch (output)
										{
											case "true":
												clientAccountUserName[clientNum] = AccountMessage[0];
												clientAccountID[clientNum] = AccountMessage[2];
												 
												ConsoleOutput.CO("[" + TimeMDHMS + "][系统信息]" + "客户端(编号:" + clientNum + ")用户名：" + clientAccountUserName[clientNum] + "\r\n", true);
												//}));
												clientPassNum[clientNum]++;

												if (clientConnectLastTime[clientNum] == null)//给客户端上一次连接时间的变量赋值
												{
													xmlDoc = new XmlDocument();
													xmlDoc.Load(FilePath.ClientAccountDataFile);
													xmlRoot = xmlDoc.SelectSingleNode("ClientAccountData"); //进入对应节点
													xmlNL = xmlRoot.ChildNodes;
													foreach (XmlNode xn in xmlNL) //循环扫描节点
													{
														XmlElement xmlE = (XmlElement)xn;
														switch (xmlE.Name)
														{
															case "ClientAccount":
																if (clientAccountID[clientNum] == xmlE.GetAttribute("ID"))
																{
																	clientConnectLastTime[clientNum] = xmlE.GetAttribute("ConnectLastTime");
																}
																break;
														}
													}
													for (int i = 0; i < notRunOldChatLastConnectLastTime.Count; i += 3)
													{
														if (notRunOldChatLastConnectLastTime[i].ToString() == clientAccountUserName[clientNum])
														{
															if (notRunOldChatLastConnectLastTime[i + 1].ToString() == "false")
															{

															}
															else if (notRunOldChatLastConnectLastTime[i + 1].ToString() == "true")
															{
																notRunOldChatLastConnectLastTime[i + 1] = "false";
																notRunOldChatLastConnectLastTime[i + 2] = clientConnectLastTime[clientNum];
															}
															goto exitFor;
														}
													}
													notRunOldChatLastConnectLastTime.Add(clientAccountUserName[clientNum]);
													notRunOldChatLastConnectLastTime.Add("false");
													notRunOldChatLastConnectLastTime.Add(clientConnectLastTime[clientNum]);
exitFor:;
												}
												break;
											case "null":
												WriterVoid(new string[2] { "accountVerify", "null" }, null, clientNum);
												break;
											case "passwordError":
												WriterVoid(new string[2] { "accountVerify", "passwordError" }, null, clientNum);
												break;
											case "ban":
												WriterVoid(new string[2] { "accountVerify", "ban" }, null, clientNum);
												break;
										}
									}
									break;
								default:
									DisClient("refuse2", clientNum);//客户端信息错误拒绝连接
									break;
							}
							if (clientPassNum[clientNum] == 3)
							{
								clientPass[clientNum] = true;
								WriterVoid(new string[1] { "passSucceed" }, null, clientNum);
								 

								if (clientAccountUserName[clientNum] != null)
								{
									ConsoleOutput.CO("[" + TimeMDHMS + "][系统信息]" + "客户端(编号:" + clientNum + ")验证成功！(已使用账户登录)" + "\r\n", true);
									if (clientActivateAccount == false) { ConsoleOutput.CO("[" + clientConnectTime[clientNum] + "][系统信息]" + "客户端验证完毕!客户端用户名：" + clientAccountUserName[clientNum] + "\r\n"); }
								}
								else
								{
									ConsoleOutput.CO("[" + TimeMDHMS + "][系统信息]" + "客户端(编号:" + clientNum + ")验证成功！(未使用账户登录)" + "\r\n", true);
									if (clientActivateAccount == false) { ConsoleOutput.CO("[" + clientConnectTime[clientNum] + "][系统信息]" + "客户端验证完毕!客户端IP: " + clientIP[clientNum] + "\r\n"); }
								}
								//}));
								if (clientActivateAccount == false)
								{
									WriterVoid(new string[1] { "clientStatus" }, new string[2] { "connect", clientAccountUserName[clientNum] }, clientNum);//给其他客户端发送客户端连入信息
								}

								AccountMessage = null;
								AccountMessage = new string[3];
							}
						}
					}
				}
				catch (Exception ex)
				{
					//Console.WriteLine("信息读取块出错：" + ex);
					if (ReaderListenTryLock[clientNum] == false)
					{
						if (clientNumStatus != 0 && clientNumBool[clientNum] == true)
						{
							DisClient("lost", clientNum);
						}
						else if (clientNumBool[clientNum] == true && clientPass[clientNum] == false && errorLock == false)
						{
							DisClient("refuse2", clientNum);
						}
					}
					break;
				}
				Thread.Sleep(1);
			}
			readerListenThreadAbort[(int)threadId] = false;
		}
		#endregion

		public void ClientListOutput()//已连接客户端管理列表输出
		{
			int clientNum;
			/* 
				clientListView.BeginUpdate();   //数据更新，UI暂时挂起
				clientListView.Items.Clear();
			//}));*/
			string outputStr = "";
			for (clientNum = 0; clientNum < clientNumBool.Length; clientNum++)
			{
				if (clientNumBool[clientNum] == true)
				{
					//ListViewItem lvi = new ListViewItem();
					List<string> outputList = new();
					if (clientAccountID[clientNum] != null && clientAccountUserName[clientNum] != null)
					{
						outputList.Add(clientAccountID[clientNum]);
						outputList.Add(clientAccountUserName[clientNum]);
					}
					else
					{
						outputList.Add("访客");
						outputList.Add("-");
					}
					outputList.Add(clientIP[clientNum]);
					outputList.Add(clientNum.ToString());
					outputList.Add(clientConnectTime[clientNum]);
					  //clientListView.Items.Add(lvi); //}));
					for (int i = 0; i < outputList.Count; i++)
					{
						outputStr += outputList[i] + " ";
					}
					outputStr += "\r\n";
				}
				//else if (clientNumBool[clientNum] == false)
				//{

				//}
			}
			ConsoleOutput.CO(outputStr);
			  //clientListView.EndUpdate(); //}));
		}
		public void AccountListOutput()//客户端账户列表输出
		{
			//this.Invoke(new Action(() =>{
			//clientAccountListView.BeginUpdate();//数据更新，UI暂时挂起
			//clientAccountListView.Items.Clear();
			//}));

			XmlDocument xmlDoc = new XmlDocument();
			XmlNode xmlRoot;
			xmlDoc.Load(FilePath.ClientAccountDataFile);
			xmlRoot = xmlDoc.SelectSingleNode("ClientAccountData"); //进入对应节点
			XmlNodeList xmlNL = xmlRoot.ChildNodes;
			// int i=0;
			string outputStr = "";
			foreach (XmlNode xn in xmlNL) //循环扫描节点
			{
				XmlElement xmlE = (XmlElement)xn;
				switch (xmlE.Name)
				{
					case "ClientAccount":
						List<string> outputList = new List<string>();
						outputList.Add(xmlE.GetAttribute("ID"));

						outputList.Add(xmlE.GetAttribute("UserName"));

						switch (xmlE.GetAttribute("State"))
						{
							case "false":
								outputList.Add("未激活");
								break;
							case "true":
								outputList.Add("已激活");
								break;
							case "ban":
								outputList.Add("禁封");
								break;
						}

						outputList.Add(xmlE.GetAttribute("ConnectLastTime"));
						outputList.Add(xmlE.GetAttribute("ActivateTime"));
						outputList.Add(xmlE.GetAttribute("Time"));

						string pwStr = xmlE.GetAttribute("Password");
						string pwStrOut = null;
						for (int i2 = 0; i2 < pwStr.Length; i2++)
						{
							pwStrOut += "*";
						}
						outputList.Add(pwStrOut);

						//this.Invoke(new Action(() =>{
						//clientAccountListView.Items.Add(lvi);
						//}));
						for (int i = 0; i < outputList.Count; i++)
						{
							outputStr += outputList[i] + " ";
						}
						outputStr += "\r\n";
						break;
				}
			}
			//this.Invoke(new Action(() =>{
			//clientAccountListView.EndUpdate();//结束数据处理
			//}));
			ConsoleOutput.CO(outputStr);
		}


		#region"其他控件事件"
		//string ConsoleOutput.logFileName = DateTime.Now.ToString("yyyy_MM_dd HH_mm_ss")+".log";

		//int logLines = 0, logLines2 = 0;
		#region 左侧输出栏控件相关事件
		/*private void MessageBox_TextChanged(object sender, EventArgs e)
		{
			try
			{
				while (messageBox.Lines[logLines] != "" || logLines + 2 == messageBox.Lines.Length)
				{
					ConsoleOutput.logFile.WriteLine(messageBox.Lines[logLines]);
					logLines++;
				}
			}
			catch
			{
				Console.WriteLine("日志编写块出错！");
			}

		}
		private void MoreMessageBox_TextChanged(object sender, EventArgs e)
		{
			try
			{
				while (moreMessageBox.Lines[logLines2] != "" || logLines2 + 2 == messageBox.Lines.Length)
				{
					ConsoleOutput.logFile2.WriteLine(moreMessageBox.Lines[logLines2]);
					logLines2++;
				}
			}
			catch
			{
				Console.WriteLine("日志编写块出错！");
			}
		}*/

		#region 发送相关控件
		public void SendChat()//Button_Click(object sender, EventArgs e)
		{
			WriterVoid(["ChatSend"], [], -1);
		}

		/*private void SendChatBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				e.Handled = true;
				WriterVoid(new string[1] { "ChatSend" }, new string[0], -1);
			}
		}
		private void SendChatBox_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == System.Convert.ToChar(13))
			{
				e.Handled = true;
			}
		}*/
		#endregion
		#endregion

		#region 服务器控制相关控件
		public void OpenServer()//_Click(object sender, EventArgs e)
		{
			if (Config.CanOpenServer)
			{
				Config.CanOpenServer = false;
				//ipTextBox.Enabled = false;
				//portTextBox.Enabled = false;
				Config.CanCloseServer = true;
				Config.CanClientControl = true;
				Config.CanSendChat = true;
				//sendChatBox.Focus();

				clientListenThreadAbort=true;
				clientListenThread = new Thread(ClientListen);
				clientListenThread.Start();
			}
			else
				ConsoleOutput.CO("[" + TimeMDHMS + "][控制台信息]" + "错误:无法启动服务端");
		}
		public void CloseServer()//_Click(object sender, EventArgs e)
		{
			if (Config.CanCloseServer)
			CloseServerVoid("Button");
			else
				ConsoleOutput.CO("[" + TimeMDHMS + "][控制台信息]" + "错误:无法关闭服务端");
		}
		#endregion


		/*private void ControlPanelTabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (ControlPanelTabControl.SelectedTab.Name == clientAccountControl.Name)
			{
				AccountListViewDataRefresh();
			}
			else if (ControlPanelTabControl.SelectedTab.Name == clientControl.Name)
			{
				ClientListViewDataRefresh();
			}
		}*/

		#region 客户端账户相关控件
		/*#region 账户ID文本框事件
		private void InputAccountIdTextBox_Enter(object sender, EventArgs e)
		{
			if (inputAccountId == "在此输入账户的ID" && inputAccountIdTextBox.ForeColor == Color.DarkGray)
			{
				inputAccountIdTextBox.Clear();
				inputAccountIdTextBox.ForeColor = Color.Black;
			}
		}
		private void InputAccountIdTextBox_Leave(object sender, EventArgs e)
		{
			if (inputAccountId == "")
			{
				inputAccountIdTextBox.ForeColor = Color.DarkGray;
				inputAccountId = "在此输入账户的ID";
			}
		}
		#endregion*/
		public void AddAccountId(string inputAccountId)//Button_Click(object sender, EventArgs e)
		{
			if (inputAccountId != "")
			{
				bool IdExist = false;

				XmlDocument xmlDoc = new XmlDocument();
				XmlNode xmlRoot;
				xmlDoc.Load(FilePath.ClientAccountDataFile);
				xmlRoot = xmlDoc.SelectSingleNode("ClientAccountData"); //进入对应节点
				XmlNodeList xmlNL = xmlRoot.ChildNodes;
				foreach (XmlNode xn in xmlNL) //循环扫描节点
				{
					XmlElement xmlE = (XmlElement)xn;
					switch (xmlE.Name)
					{
						case "ClientAccount":
							if (xmlE.GetAttribute("ID") == inputAccountId)
							{
								IdExist = true;
								goto overFor;
							}
							break;
					}
				}
overFor:;

				if (IdExist == false)//判断要添加的ID是否与已有ID重复
				{
					xmlDoc = new XmlDocument();
					XmlElement xmlEle;

					xmlDoc.Load(FilePath.ClientAccountDataFile);
					xmlRoot = xmlDoc.SelectSingleNode("ClientAccountData");
					xmlEle = xmlDoc.CreateElement("ClientAccount");
					xmlEle.SetAttribute("ID", inputAccountId);
					xmlEle.SetAttribute("State", "false");
					xmlEle.SetAttribute("UserName", "");
					xmlEle.SetAttribute("Password", "");
					xmlEle.SetAttribute("Time", System.DateTime.Now.ToString());
					xmlEle.SetAttribute("ActivateTime", "");
					//xmlEle.InnerText = "";//内部文本（可选）
					xmlRoot.AppendChild(xmlEle);
					xmlDoc.Save(FilePath.ClientAccountDataFile);

					ConsoleOutput.CO("[" + TimeMDHMS + "][控制台信息]" + "已添加ID为" + inputAccountId + "的账户！" + "\r\n");
					//inputAccountIdTextBox.Clear();
					//InputAccountIdTextBox_Leave(sender, e);
					//AccountListViewDataRefresh();
				}
				else
				{
					ConsoleOutput.CO("[" + TimeMDHMS + "][控制台信息]" + "添加失败！为" + inputAccountId + "的ID与已有ID重复！" + "\r\n");
					//inputAccountIdTextBox.Clear();
					//InputAccountIdTextBox_Leave(sender, e);
				}
			}
			else
			{
				ConsoleOutput.CO("[" + TimeMDHMS + "][控制台信息]" + "被添加的账户ID不能为空！\r\n");
			}
		}
		public void RemoveAccountId(string inputAccountId)//Button_Click(object sender, EventArgs e)
		{
			if (inputAccountId != "")
			{
				bool IdExist = false;

				XmlDocument xmlDoc = new XmlDocument();
				XmlNode xmlRoot;
				xmlDoc.Load(FilePath.ClientAccountDataFile);
				xmlRoot = xmlDoc.SelectSingleNode("ClientAccountData"); //进入对应节点
				XmlNodeList xmlNL = xmlRoot.ChildNodes;
				foreach (XmlNode xn in xmlNL) //循环扫描节点
				{
					XmlElement xmlE = (XmlElement)xn;
					switch (xmlE.Name)
					{
						case "ClientAccount":
							if (xmlE.GetAttribute("ID") == inputAccountId)
							{
								IdExist = true;
								goto overFor;
							}
							break;
					}
				}
overFor:;

				if (IdExist == true)//判断被删除ID是否存在
				{
					//if (MessageBox.Show("确定删除ID为" + inputAccountId + "的账户吗？", this.Name, MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button2) == DialogResult.OK){
					Console.Write("确定删除ID为" + inputAccountId + "的账户吗？" + "(yes/no): ");
restart:;
					string? readInput= Console.ReadLine()?.ToLower();
					if (readInput == "yes")
					{
						xmlDoc = new XmlDocument();
						xmlDoc.Load(FilePath.ClientAccountDataFile);
						xmlRoot = xmlDoc.SelectSingleNode("ClientAccountData");
						xmlNL = xmlRoot.ChildNodes;
						foreach (XmlNode xn in xmlNL) //循环扫描节点
						{
							XmlElement xmlE = (XmlElement)xn;
							if (xmlE.GetAttribute("ID") == inputAccountId)
							{
								xmlRoot.RemoveChild(xmlE); //删除该节点
								break;
							}
						};
						xmlDoc.Save(FilePath.ClientAccountDataFile);

						ConsoleOutput.CO("[" + TimeMDHMS + "][控制台信息]" + "已删除ID为" + inputAccountId + "的账户！" + "\r\n");
						//inputAccountIdTextBox.Clear();
						//InputAccountIdTextBox_Leave(sender, e);
						//AccountListViewDataRefresh();
					}else if (readInput != "no")
					{
						Console.Write("请输入yes或no来确认操作: ");
						goto restart;
					}
				}
				else
				{
					ConsoleOutput.CO("[" + TimeMDHMS + "][控制台信息]" + "为" + inputAccountId + "的账户ID不存在！\r\n");
					//inputAccountIdTextBox.Clear();
					//InputAccountIdTextBox_Leave(sender, e);
				}
			}
			else
			{
				ConsoleOutput.CO("[" + TimeMDHMS + "][控制台信息]" + "被删除的账户ID不能为空！\r\n");
			}
		}
		public void BanAccountId(string inputAccountId)//Button_Click(object sender, EventArgs e)
		{
			if (inputAccountId != "" )
			{
				bool IdExist = false;//扫描指定账户是否存在

				XmlDocument xmlDoc = new XmlDocument();
				XmlNode xmlRoot;
				xmlDoc.Load(FilePath.ClientAccountDataFile);
				xmlRoot = xmlDoc.SelectSingleNode("ClientAccountData"); //进入对应节点
				XmlNodeList xmlNL = xmlRoot.ChildNodes;
				foreach (XmlNode xn in xmlNL) //循环扫描节点
				{
					XmlElement xmlE = (XmlElement)xn;
					switch (xmlE.Name)
					{
						case "ClientAccount":
							if (xmlE.GetAttribute("ID") == inputAccountId)
							{
								if (xmlE.GetAttribute("State") == "false")
								{
									ConsoleOutput.CO("[" + TimeMDHMS + "][控制台信息]" + "被禁封或解封的账户ID未激活！\r\n");
									goto end;
								}
								IdExist = true;
								goto overFor;
							}
							break;
					}
				}
overFor:;
				if (IdExist == true)//判断被删除ID是否存在
				{
					xmlDoc = new XmlDocument();
					XmlElement xmlEle;
					xmlDoc.Load(FilePath.ClientAccountDataFile);
					xmlNL = xmlDoc.SelectSingleNode("ClientAccountData").ChildNodes;
					foreach (XmlNode xn in xmlNL) //循环扫描节点
					{
						xmlEle = (XmlElement)xn;
						if (xmlEle.GetAttribute("ID") == inputAccountId) //找到名字为其的节点
						{
							if (xmlEle.GetAttribute("State") == "true")
							{
								xmlEle.SetAttribute("State", "ban");
								ConsoleOutput.CO("[" + TimeMDHMS + "][控制台信息]" + "已禁封ID为" + inputAccountId + "的账户！" + "\r\n");

								for (int i = 0; i < clientNumBool.Length; i++)
								{
									if (clientNumBool[i] == true && clientAccountUserName[i] == xmlEle.GetAttribute("UserName"))
									{
										WriterVoid(new string[1] { "kickBan" }, null, i);
										break;
									}
								}
							}
							else if (xmlEle.GetAttribute("State") == "ban")
							{
								xmlEle.SetAttribute("State", "true");
								ConsoleOutput.CO("[" + TimeMDHMS + "][控制台信息]" + "已解封ID为" + inputAccountId + "的账户！" + "\r\n");
							}
							break;
						}
					}
					xmlDoc.Save(FilePath.ClientAccountDataFile);

					//inputAccountIdTextBox.Clear();
					//InputAccountIdTextBox_Leave(sender, e);
					//AccountListViewDataRefresh();
				}
			}
			else
			{
				ConsoleOutput.CO("[" + TimeMDHMS + "][控制台信息]" + "被禁封或解封的账户ID不能为空！\r\n");
			}
end:;
		}
		/*private void ClientAccountListView_SelectedIndexChanged(object sender, EventArgs e)
		{
			InputAccountIdTextBox_Enter(sender, e);
			try { inputAccountId = clientAccountListView.Items[clientAccountListView.FocusedItem.Index].SubItems[0].Text; } catch { }

			AccountListViewDataRefresh();
		}*/
		#endregion

		#region 客户端管理相关控件
		/*#region 客户端编号文本框事件
		private void InputClientTextBox_Enter(object sender, EventArgs e)
		{
			if (inputClient == "在此输入客户端的编号" && inputClientTextBox.ForeColor == Color.DarkGray)
			{
				inputClientTextBox.Clear();
				inputClientTextBox.ForeColor = Color.Black;
			}
		}

		private void InputClientTextBox_Leave(object sender, EventArgs e)
		{
			if (inputClient == "")
			{
				inputClientTextBox.ForeColor = Color.DarkGray;
				inputClient = "在此输入客户端的编号";
			}
		}
		#endregion*/

		public void ClientKick(string inputClient)//Button_Click(object sender, EventArgs e)
		{
			try
			{
				if (inputClient != "")
				{
					if (clientNumBool[int.Parse(inputClient)] == true)
					{
						WriterVoid(new string[1] { "kick" }, null, int.Parse(inputClient));
						//inputClientTextBox.Clear();
					}
					else
					{
						ConsoleOutput.CO("[" + TimeMDHMS + "][控制台信息]" + "被断开连接的客户端编号不存在！\r\n");
					}
				}
				else
				{
					ConsoleOutput.CO("[" + TimeMDHMS + "][控制台信息]" + "被断开连接的客户端编号不能为空！\r\n");
				}
			}
			catch
			{
				ConsoleOutput.CO("[" + TimeMDHMS + "][控制台信息]" + "被断开连接的客户端编号错误！\r\n");
			}

		}
		/*private void ClientListView_SelectedIndexChanged(object sender, EventArgs e)
		{
			InputClientTextBox_Enter(sender, e);
			inputClient = clientListView.Items[clientListView.FocusedItem.Index].SubItems[3].Text;

			ClientListViewDataRefresh();

			//for (int i = 0; i < clientListView.Items.Count; i++)
			//{
			//    if (clientListView.Items[i].SubItems[3].Text == inputClient)
			//    {
			//        clientListView.Items[i].Selected = true;
			//        break;
			//    }
			//}
		}*/
		#endregion
		#endregion

		#region"Form事件"
		public void Core_Closed()
		{
			CloseServerVoid("Close");
			//后面不要添加代码，因为不会执行到后面，如要添加就添加到上面的定义里      
		}



		public void Core_Load()//(object sender, EventArgs e)
		{
			System.IO.Directory.CreateDirectory(FilePath.DataDir);
			System.IO.Directory.CreateDirectory(FilePath.LogDir);
			System.IO.Directory.CreateDirectory(FilePath.MoreLogDir);
			if (System.IO.File.Exists(FilePath.ServerSettingFile) == false)
			{
				XmlTextWriter xmlWriter = new XmlTextWriter(FilePath.ServerSettingFile, System.Text.Encoding.GetEncoding("utf-8")) { Formatting = System.Xml.Formatting.Indented };

				xmlWriter.WriteRaw("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");

				xmlWriter.WriteStartElement("ServerSetting");//新建节点Data

				xmlWriter.WriteStartElement("IPandPort");
				xmlWriter.WriteAttributeString("ip", Config.OpenIP);//定义属性
				xmlWriter.WriteAttributeString("port", Config.OpenPort);
				xmlWriter.WriteEndElement();

				xmlWriter.WriteStartElement("AccountVerify");
				xmlWriter.WriteAttributeString("bool", "false");
				xmlWriter.WriteEndElement();

				xmlWriter.WriteStartElement("moreMessage");
				xmlWriter.WriteAttributeString("bool", "false");
				xmlWriter.WriteEndElement();
				

				/*xmlWriter.WriteStartElement("FormControlState");
				{
					xmlWriter.WriteStartElement("Form");
					xmlWriter.WriteAttributeString("Left", this.Left.ToString());
					xmlWriter.WriteAttributeString("Top", this.Top.ToString());
					xmlWriter.WriteAttributeString("Width", this.Width.ToString());
					xmlWriter.WriteAttributeString("Height", this.Height.ToString());
					xmlWriter.WriteEndElement();

					xmlWriter.WriteStartElement("splitChatAndControlPanel");
					xmlWriter.WriteAttributeString("SplitterDistance", splitChatAndControlPanel.SplitterDistance.ToString());
					xmlWriter.WriteEndElement();

					xmlWriter.WriteStartElement("clientListView");
					xmlWriter.WriteAttributeString("clientIDHeader_Width", clientIDHeader.Width.ToString());
					xmlWriter.WriteAttributeString("clientUserNameHeader_Width", clientUserNameHeader.Width.ToString());
					xmlWriter.WriteAttributeString("clientIPHeader_Width", clientIPHeader.Width.ToString());
					xmlWriter.WriteAttributeString("clientNumHeader_Width", clientNumHeader.Width.ToString());
					xmlWriter.WriteAttributeString("clientConnectTimeHeader_Width", clientConnectTimeHeader.Width.ToString());
					xmlWriter.WriteEndElement();

					xmlWriter.WriteStartElement("clientAccountListView");
					xmlWriter.WriteAttributeString("accountID_Width", accountID.Width.ToString());
					xmlWriter.WriteAttributeString("accountUserName_Width", accountUserName.Width.ToString());
					xmlWriter.WriteAttributeString("accountState_Width", accountState.Width.ToString());
					xmlWriter.WriteAttributeString("ConnectLastTimeHeader_Width", ConnectLastTimeHeader.Width.ToString());
					xmlWriter.WriteAttributeString("accountActivateTime_Width", accountActivateTime.Width.ToString());
					xmlWriter.WriteAttributeString("accountTime_Width", accountTime.Width.ToString());
					xmlWriter.WriteAttributeString("accountPassword_Width", accountPassword.Width.ToString());
					xmlWriter.WriteEndElement();
				}
				xmlWriter.WriteEndElement();*/


				xmlWriter.WriteFullEndElement();//Data根节点结束
				xmlWriter.Close();
			}
			else
			{
				try
				{
					XmlDocument xmlDoc = new XmlDocument();
					XmlNode xmlRoot;
					xmlDoc.Load(FilePath.ServerSettingFile);
					xmlRoot = xmlDoc.SelectSingleNode("ServerSetting"); //进入对应节点
					XmlNodeList xmlNL = xmlRoot.ChildNodes;
					foreach (XmlNode xn in xmlNL) //循环扫描节点
					{
						XmlElement xmlE = (XmlElement)xn;
						switch (xmlE.Name)
						{
							case "IPandPort":
								Config.OpenIP = xmlE.GetAttribute("ip"); //获取指定属性名的对应值
								Config.OpenPort = xmlE.GetAttribute("port");
								break;
							case "AccountVerify":
								switch (xmlE.GetAttribute("bool"))
								{
									case "true":
										Config.AccountVerify = true;
										break;
									case "false":
										Config.AccountVerify = false;
										break;
								}
								break;
							case "moreMessage":
								switch(xmlE.GetAttribute("bool"))
								{
									case "true":
										Config.moreMessage = true;break;
										case "false":
										Config.moreMessage=false;break;
								}
								break;
						}
					}
				}
				catch { }
				/*try
				{
					XmlDocument xmlDoc = new XmlDocument();
					XmlNode xmlRoot;
					XmlNodeList xmlNL;
					xmlDoc.Load(FilePath.ServerSettingFile);
					xmlRoot = xmlDoc.SelectSingleNode("ServerSetting").SelectSingleNode("FormControlState");
					xmlNL = xmlRoot.ChildNodes;
					foreach (XmlNode xn in xmlNL)
					{
						XmlElement xmlE = (XmlElement)xn;
						switch (xmlE.Name)
						{
							case "Form":
								Left = int.Parse(xmlE.GetAttribute("Left"));
								Top = int.Parse(xmlE.GetAttribute("Top"));
								Width = int.Parse(xmlE.GetAttribute("Width"));
								Height = int.Parse(xmlE.GetAttribute("Height"));
								break;
							case "splitChatAndControlPanel":
								splitChatAndControlPanel.SplitterDistance = int.Parse(xmlE.GetAttribute("SplitterDistance"));
								break;
							case "clientListView":
								clientIDHeader.Width = int.Parse(xmlE.GetAttribute("clientIDHeader_Width"));
								clientUserNameHeader.Width = int.Parse(xmlE.GetAttribute("clientUserNameHeader_Width"));
								clientIPHeader.Width = int.Parse(xmlE.GetAttribute("clientIPHeader_Width"));
								clientNumHeader.Width = int.Parse(xmlE.GetAttribute("clientNumHeader_Width"));
								clientConnectTimeHeader.Width = int.Parse(xmlE.GetAttribute("clientConnectTimeHeader_Width"));
								break;
							case "clientAccountListView":
								accountID.Width = int.Parse(xmlE.GetAttribute("accountID_Width"));
								accountUserName.Width = int.Parse(xmlE.GetAttribute("accountUserName_Width"));
								accountState.Width = int.Parse(xmlE.GetAttribute("accountState_Width"));
								ConnectLastTimeHeader.Width = int.Parse(xmlE.GetAttribute("ConnectLastTimeHeader_Width"));
								accountActivateTime.Width = int.Parse(xmlE.GetAttribute("accountActivateTime_Width"));
								accountTime.Width = int.Parse(xmlE.GetAttribute("accountTime_Width"));
								accountPassword.Width = int.Parse(xmlE.GetAttribute("accountPassword_Width"));
								break;
						}
					}
				}
				catch { }*/
			}
			if (System.IO.File.Exists(FilePath.ClientAccountDataFile) == false)
			{
				XmlTextWriter xmlWriter = new XmlTextWriter(FilePath.ClientAccountDataFile, System.Text.Encoding.GetEncoding("utf-8")) { Formatting = System.Xml.Formatting.Indented };

				xmlWriter.WriteRaw("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");

				xmlWriter.WriteStartElement("ClientAccountData");
				xmlWriter.WriteFullEndElement();
				xmlWriter.Close();
			}
			//if (true)
			//{
			//    XmlTextWriter xmlWriter = new XmlTextWriter("C:\\ProgramData\\MiniatureCommunicationServer_Data\\tempClientOldChatLine.xml", System.Text.Encoding.GetEncoding("utf-8")) { Formatting = System.Xml.Formatting.Indented };

			//    xmlWriter.WriteRaw("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");

			//    xmlWriter.WriteStartElement("tempClientOldChatLine");//新建节点Data

			//    xmlWriter.WriteEndElement();

			//    xmlWriter.WriteFullEndElement();//Data根节点结束
			//    xmlWriter.Close();
			//}//临时客户端获取旧信息的历史行数

			ConsoleOutput.CO("服务端当前版本：V" + versionStr + "\r\n");

			//gy2.Text += versionStr;
		}







		#endregion




		#region 加解密块
		public static string EncryptIP(string EncryptInput)//IP加解密
		{
			string[] classOutput;
			if (EncryptInput.Length > 15)//如果字符串大于15，就说明是加密形式的IP地址,加密方法为二代XHM加密法，运行解密方法
			{
				_2ndGenerationEncryptionMethodsFromHgnim emfHg2g = new();
				classOutput = emfHg2g.DecryptInoutPut([EncryptInput], "@*%M_C$&#", "text");
				//加密密钥为"@#$MC&*%"
				if (classOutput[0] == "output")
				{
					//EncryptInput = classOutput[1];
					return classOutput[1];
				}
				else if (classOutput[0] == "PwWrong")//如果密钥不一致，就说明服务器与客户端版本不一致
				{
					//MessageBox.Show("解密错误！加密此IP的服务端版本与当前服务端版本不匹配！", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);//如果密钥不一致，说明加密时用的服务端用的密钥与当前不同，尽量不要更改加密密钥
					//EncryptInput = null;
					return "解密错误！加密此IP的服务端版本与当前服务端版本不匹配！";
				}
				else
				{
					//MessageBox.Show("解密错误！加密文本错误！", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
					//EncryptInput = null;
					return "解密错误！加密文本错误！";
				}
			}
			else//否则就是待加密的IP地址
			{
				_2ndGenerationEncryptionMethodsFromHgnim emfHg2g = new();
				classOutput = emfHg2g.EncryptInoutPut([EncryptInput], "@*%M_C$&#", "text");
				//加密密钥为"@#$MC&*%"
				if (classOutput[0] == "output")
				{
					//EncryptInput = classOutput[1];
					return classOutput[1];
				}
				else
				{
					//MessageBox.Show("加密错误！", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
					//EncryptInput = null;
					return "加密错误！";
				}
			}
		}
		/// <summary>
		/// 加密网络传输的字符串
		/// </summary>
		/// <param name="enc">是否为加密</param>
		/// <param name="input">输入字符串</param>
		/// <returns>加密或解密后的文本</returns>
		public static string EncryptStr(bool enc,string input)
		{
			try
			{
				string[] output;
				_2ndGenerationEncryptionMethodsFromHgnim emfHg2g = new();
				//加密密钥为"$#*M&C*@%"
				if (enc)
				{
					output = emfHg2g.EncryptInoutPut([input], "$#*M&C*@%", "text");
				}
				else
				{
					output = emfHg2g.DecryptInoutPut([input], "$#*M&C*@%", "text");
				}
				return output[1];
			}
			catch { return "!!----*error*----!!"; }
		}
		#endregion

		public void Info()
		{
			Copyright cp = new();
			ConsoleOutput.CO("程序名: 微型通信服务端(命令行重制版)/MiniatureCommunicationConsole(Server)" +
				         "\r\n版本: V"+versionStr+
						 "\r\n" + cp.GetC());
		}
	}
	public static class ConsoleOutput
	{
		public static List<string> listMessage = new();
		public static List<string> listMoreMessage = new();
		/// <summary>
		/// 控制台信息输出事件
		/// </summary>
		/// <param name="outputStr">输出文本</param>
		/// <param name="moremsg">是否为详细信息输出</param>
		//	/// <param name="mustIs">是否必须符合输出条件，如果不符合则只编写日志不进行输出
		//	/// ，当详细输出和正常输出不同时可以开启此项</param>
		public static void CO(string outputStr, bool moremsg)//, bool mustIs = false)
		{
			if (outputStr.Substring(outputStr.Length - 2, 2) != "\r\n")outputStr += "\r\n";
			if (moremsg)
			{
				if (Config.moreMessage)
					Console.Write(outputStr);
				Log.Write(2, [outputStr]);
				listMoreMessage.Add(outputStr);
			}
			else
			{
				if (!Config.moreMessage)
					Console.Write(outputStr);
				Log.Write(1, [outputStr]);
				listMessage.Add(outputStr);
			}
			/*else
			{
				//mustIs仅对moremsg为false的情况有效。
				//当mustIs为false时，则输出当前的信息并且将日志写入正常信息日志文件和详细信息日志文件中；
				//当mustIs为true时，如果当前不是正常输出模式，则不输出信息，但会将信息写入正常信息日志文件，不会将信息写入详细信息日志文件中；
				//当mustIs为true时，如果当前是正常输出模式，则输出信息，会将信息写入正常信息日志文件，但不会将信息写入详细详细日志文件中；
				if (!(mustIs && Config.moreMessage != false))//如果不是必须符合输出条件，则不进行输出。
															 //例如：如果当前是详细输出模式，则遇到两种不同的输出方式，则跳过正常类型的输出，但记录日志文件。
					Console.WriteLine(outputStr);
				ConsoleOutput.logFile.WriteLine(outputStr);
				if (!(mustIs))//如果不是必须符合输出条件，则将信息写入详细信息的日志文件中
					ConsoleOutput.logFile2.WriteLine(outputStr);
			}*/
		}
		/// <summary>
		/// 控制台信息输出事件，不标记后面参数时，则表示当前正常输出和详细输出的内容相同
		/// </summary>
		/// <param name="outputStr">输出文本</param>
		public static void CO(string outputStr)
		{
			try
			{
				if (outputStr.Substring(outputStr.Length - 2, 2) != "\r\n") outputStr += "\r\n";
			}
			catch { }

			Console.Write(outputStr);
			Log.Write(0, [outputStr]);
			listMessage.Add(outputStr);
			listMoreMessage.Add(outputStr);
		}

		public static class Log
		{
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
			 static System.IO.StreamWriter logFile, logFile2;
			static bool load = false;
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
			public static void Load()
			{
				if (!load)
				{
					logFile = new System.IO.StreamWriter(FilePath.LogDir + DateTime.Now.ToString("yyyy_MM_dd HH_mm_ss") + ".log"
		, true, System.Text.UTF8Encoding.Default)
					{
						AutoFlush = true
					};
					logFile2 = new StreamWriter(FilePath.MoreLogDir + DateTime.Now.ToString("yyyy_MM_dd HH_mm_ss") + ".log"
						, true, UTF8Encoding.Default)
					{
						AutoFlush = true
					};
					load=true;
				}		
			}
			public static void Write(int id, string[] args)
			{
				if (load)
				{
					switch (id)
					{
						case 0:
							logFile.Write(args[0]);
							logFile2.Write(args[0]);
							break;
						case 1:
							logFile.Write(args[0]);
							break;
						case 2:
							logFile2.Write(args[0]);
							break;
					}
				}
			}
			public static void Close()
			{
				if (load)
				{
					logFile.Close();
					logFile2.Close();
					load=false;
				}
			}
		}
	}
}
