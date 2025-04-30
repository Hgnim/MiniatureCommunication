namespace MiniatureCommunicationConsole_Server_
{
	internal class Program
	{
		//微型通信命令行重制版，从WinForm版移植过来的

		public static bool ExitSignal=false;//放出退出信号以退出程序
		static Core core = new();
		static void Main(string[] args)
		{			
			core.Core_Load();
			ConsoleOutput.Log.Load();
			Console.WriteLine("*微型通信服务端*" +"\r\n输入'/help'获取帮助");

			{
				string? input;
				List<string> command;
				do
				{
					input = Console.ReadLine();					
					if (input != null)
					{
						ConsoleOutput.Log.Write(0, ["> "+input +"\r\n"]);
						if (input.Length >= 1)
						{
							switch (input.Substring(0, 1))
							{
								case "/":
									command = [];
									for (int i = 0; i < input.Split(" ").Length; i++)
									{
										if (i == 0)
											command.Add(input.Split(" ")[i].Substring(1));
										else
											command.Add(input.Split(" ")[i]);
									}
									RunCommand(command);
									break;
								default:
									RunCommand(["say", input]);
									break;
							}
						}
						else RunCommand([""]);
					}
				} while (!ExitSignal);
			}
			ConsoleOutput.Log.Close();
			core = null!;
		}
		static void RunCommand(List<string> command)
		{
			switch (command[0])
			{
				case "help":
					ConsoleOutput.CO(     "--------" +
									  "\r\n帮助: " +
									  "\r\n输入'/'以输入命令，不使用'/'则发送信息" +
									  "\r\n命令列表: " +
									  "\r\n/help	获取当前帮助" +
									  "\r\n/version(/info)	获取程序版本信息" +
									  "\r\n/say [text]	发送一条信息" +
									  "\r\n/start	启动服务端" +
									  "\r\n/stop	停止服务端" +
									  "\r\n/exit	退出服务端" +
									  "\r\n/ip {IP}	获取或设置ip地址" +
									  "\r\n/port {PORT}	获取或设置端口号" +
									  "\r\n/host {IP:PORT}	获取或设置IP和端口" +
									  "\r\n/account [list/add/remove/ban] [Account_ID]	账户操作" +
									  "\r\n/accountverify {enable/disable}	启用或禁用账户验证功能" +
									  "\r\n/client [list/kick] [Client_ID]	客户端操作" +
									  "\r\n/more {enable/disable}	启用或禁用控制台详细输出" +
									  "\r\n/enc [IP/Enc_Text]	加密或解密IP地址" +
									  "\r\n--------\r\n");
					break;
				case "start":
					core.OpenServer();break;
				case "stop":
					core.CloseServer();break;
				case "exit":
					core.Core_Closed();
					ExitSignal = true;
					break;
				case "say":
					Core.Config.SendChat = command[1];
					core.SendChat();
					break;
				case "ip":
				case "port":
				case "host":
					{
						string? commandInput;
						if (command.Count < 2)
							commandInput = null;
						else
									commandInput = command[1];					
						switch (command[0])
						{
							case "ip":
								Core.Config.SetHost(commandInput,1);break;
							case "port":
								Core.Config.SetHost(commandInput,2);break;
								case "host":
								Core.Config.SetHost(commandInput,0); break;
						}
						break;
					}
				case "account":
					{
						if (command.Count >= 2)
						{
							if (command[1] == "list")
								core.AccountListOutput();
							else
							{
								if (command.Count >= 3)
								{
									switch (command[1])
									{
										case "add":
											core.AddAccountId(command[2]);break;
										case "remove":
											core.RemoveAccountId(command[2]); break;
										case "ban":
											core.BanAccountId(command[2]);break;
										default:
											goto error;
									}
								}
								else goto error;
							}
						}
						else goto error;
						break;
error:;
						ConsoleOutput.CO("[" + Core.TimeMDHMS + "][控制台信息]" + "命令参数有误！");
						break;
					}
				case "accountverify":
					{
						if (command.Count >= 2)
						{
							switch (command[1])
							{
								case "enable":
									Core.Config.SetAccountVerify(1);break;
								case "disable":
									Core.Config.SetAccountVerify(0);break;
								default:goto error;
							}
						}
						else 
							Core.Config.SetAccountVerify(-1);
						break;
error:;
						ConsoleOutput.CO("[" + Core.TimeMDHMS + "][控制台信息]" + "命令参数有误！");
						break;
					}
				case "client":
					{
						if (command.Count >= 2)
						{
							if (command[1] == "list")
								core.ClientListOutput();
							else
							{
								if (command.Count >= 3)
								{
									switch (command[1])
									{
										case "kick":
											core.ClientKick(command[2]); break;
										default: goto error;
									}
								}else goto error;
							}
						}
						else goto error;
						break;
error:;
						ConsoleOutput.CO("[" + Core.TimeMDHMS + "][控制台信息]" + "命令参数有误！");
						break;
					}
				case "more":
					{
						if (command.Count >= 2)
						{
							switch (command[1])
							{
								case "enable":
									Core.Config.SetMoreMessage(1); break;
								case "disable":
									Core.Config.SetMoreMessage(0); break;
								default: goto error;
							}
						}
						else
							Core.Config.SetMoreMessage(-1);
						break;
error:;
						ConsoleOutput.CO("[" + Core.TimeMDHMS + "][控制台信息]" + "命令参数有误！");
						break;
					}
				case "enc":
					{
						if (command.Count >= 2)
						{
							ConsoleOutput.CO("[" + Core.TimeMDHMS + "][控制台信息]" + Core.EncryptIP(command[1]));
						}
						else goto error;
						break;
error:;
						ConsoleOutput.CO("[" + Core.TimeMDHMS + "][控制台信息]" + "命令参数有误！");
						break;
					}
				case "version":
				case "info":
					core.Info();
					break;
				default:
					ConsoleOutput.CO("[" + Core.TimeMDHMS + "][控制台信息]" + "输入的命令有误！");
					break;
			}
		}
	}
}
