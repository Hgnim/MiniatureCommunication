using Android;
using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.Hardware;
using Android.Nfc;
using Android.OS;
using Android.Runtime;
using Android.Service.Controls;
using Android.Text;
using Android.Text.Method;
using Android.Text.Style;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using AndroidX.Core.Text;
using AndroidX.Core.Widget;
using Google.Android.Material.Snackbar;
using Java.IO;
using Java.Lang;
using Java.Util.Logging;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Resources;
using System.Threading;
using Xamarin.Essentials;
using static Android.Bluetooth.BluetoothClass;
using static System.Net.Mime.MediaTypeNames;
using File = System.IO.File;
using Path = System.IO.Path;
using Thread = System.Threading.Thread;

namespace MiniatureCommunication_Client_
{
    [Activity(Label = "@string/app_name", Theme = "@style/Theme.MaterialComponents.NoActionBar.Bridge", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {

        /// <summary>
        /// 2023.4.18
        /// 当前已更新内容：
        /// 
        /// 当前正在更新的内容：
        /// 
        /// 
        /// 当前正在尝试的内容：
        /// 所有UI控件搞逐字输出
        /// </summary>
        readonly string versionStr = "2.12.12.20240405_beta";//版本号信息在此填写,开头不加"V"

        System.Threading.Thread tcpListenThread; System.Threading.Thread tcpConnectThread; System.Threading.Thread tcpConnectTimeThread; System.Threading.Thread stayConnectThread;
        Thread timeChatOutputThread;
        System.Threading.Thread ConnectCooldownThread;Thread RequestCooldownThread;

        string myLocalip = "127.0.0.1";
        TcpClient tcpClient;
        BinaryReader reader; BinaryWriter writer;
        NetworkStream networkStream;

        //bool verifyPass = false;//判断是否被服务端通过验证
        string lastIpText = null, lastPortText = null;//用来检测ip和端口是否变化
        string lastUserNameText=null, lastPasswordText = null;bool lastAccountCheck = false;//用来检测用户名和密码是否变化

        bool oldChatLock = false;//禁止旧信息请求

        /// <summary>
        /// 文本框中的提示文本是否存在，提示文本将在用户点击一次后消失
        ///(0:privateSendEditText;)
        /// </summary>
        bool[] firstClick = new bool[1];
        string TimeMDHMS
        {
            get
            {
                return DateTime.Now.ToString("yy/MM/dd HH:mm:ss");
            }
        }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            RelativeLayout connectionServerRelativeLayout = FindViewById<RelativeLayout>(Resource.Id.connectionServerRelativeLayout);
            RelativeLayout chatRelativeLayout = FindViewById<RelativeLayout>(Resource.Id.chatRelativeLayout);
            RelativeLayout activateAccountRelativeLayout = FindViewById<RelativeLayout>(Resource.Id.activateAccountRelativeLayout);
            RelativeLayout chatRecordsRelativeLayout = FindViewById<RelativeLayout>(Resource.Id.chatRecordsRelativeLayout);
            Button connectionButton = FindViewById<Button>(Resource.Id.connectionButton);
            EditText ipEditText = FindViewById<EditText>(Resource.Id.ipEditText);
            EditText portEditText = FindViewById<EditText>(Resource.Id.portEditText);
            TextView stateTextView = FindViewById<TextView>(Resource.Id.stateTextView);
            TextView messageBox = FindViewById<TextView>(Resource.Id.messageBox);
            Button sendChatButton = FindViewById<Button>(Resource.Id.sendChatButton);
            TextView sendChatBox = FindViewById<TextView>(Resource.Id.sendChatBox);
            TextView guanyu = FindViewById<TextView>(Resource.Id.guanyu);
            TextView textView3=FindViewById<TextView>(Resource.Id.textView3);
            TextView textView4=FindViewById<TextView>(Resource.Id.textView4);
            EditText accountUserNameEditText=FindViewById<EditText>(Resource.Id.accountUserNameEditText);
            EditText accountPasswordEditText = FindViewById<EditText>(Resource.Id.accountPasswordEditText);
            AndroidX.AppCompat.Widget.AppCompatCheckBox accountCheck = FindViewById<AndroidX.AppCompat.Widget.AppCompatCheckBox>(Resource.Id.accountCheck);
            TextView activateAccountTextView = FindViewById<TextView>(Resource.Id.activateAccountTextView);
            TextView chatRecordsTextView = FindViewById<TextView>(Resource.Id.chatRecordsTextView);
            EditText activateAccountRelativeLayout_ActivateAccountIdEditText = FindViewById<EditText>(Resource.Id.activateAccountRelativeLayout_ActivateAccountIdEditText);
            EditText activateAccountRelativeLayout_ActivateAccountUserNameEditText = FindViewById<EditText>(Resource.Id.activateAccountRelativeLayout_ActivateAccountUserNameEditText);
            EditText activateAccountRelativeLayout_ActivateAccountPasswordEditText = FindViewById<EditText>(Resource.Id.activateAccountRelativeLayout_ActivateAccountPasswordEditText);
            EditText activateAccountRelativeLayout_ActivateAccountPassword2EditText = FindViewById<EditText>(Resource.Id.activateAccountRelativeLayout_ActivateAccountPassword2EditText);
            Button activateAccountRelativeLayout_ActivateAccountButton = FindViewById<Button>(Resource.Id.activateAccountRelativeLayout_ActivateAccountButton);
            TextView activateAccountRelativeLayout_stateTextView = FindViewById<TextView>(Resource.Id.activateAccountRelativeLayout_stateTextView);
            Button clientNumButton = FindViewById<Button>(Resource.Id.clientNumButton);
            Button oldChatButton=FindViewById<Button>(Resource.Id.oldChatButton);
            EditText privateSendEditText = FindViewById<EditText>(Resource.Id.privateSendEditText);

            Button chatRecords_exitButton = FindViewById<Button>(Resource.Id.chatRecords_exitButton);
            Button chatRecords_clearButton = FindViewById<Button>(Resource.Id.chatRecords_clearButton);
            TextView chatRecords_messageBox = FindViewById<TextView>(Resource.Id.chatRecords_messageBox);          

            messageBox.MovementMethod = new Android.Text.Method.ScrollingMovementMethod();

            //ipEditText.Text = "223.247.140.116";//暂时
            //portEditText.Text = "46674";

            connectionButton.Click += (object sender, EventArgs e) => {
                ConnectStart(false);
            };
            activateAccountRelativeLayout_ActivateAccountButton.Click += (object sender, EventArgs e) =>
            {
                //if (activateAccountRelativeLayout_ActivateAccountIdEditText.Text != "" && activateAccountRelativeLayout_ActivateAccountUserNameEditText.Text != "" && activateAccountRelativeLayout_ActivateAccountPasswordEditText.Text != "" && 
                //activateAccountRelativeLayout_ActivateAccountPasswordEditText.Text==activateAccountRelativeLayout_ActivateAccountPassword2EditText.Text)
                //{
                   //                }
                //else 
                if(activateAccountRelativeLayout_ActivateAccountIdEditText.Text == "" || activateAccountRelativeLayout_ActivateAccountUserNameEditText.Text == "" || activateAccountRelativeLayout_ActivateAccountPasswordEditText.Text == "")
                {
                    activateAccountRelativeLayout_stateTextView.Text = "激活ID或用户名或密码不能为空！";
                }else if(activateAccountRelativeLayout_ActivateAccountUserNameEditText.Text.IndexOf("@")!=-1 ||    
                activateAccountRelativeLayout_ActivateAccountUserNameEditText.Text.IndexOf("|")!=-1 ||   
                activateAccountRelativeLayout_ActivateAccountUserNameEditText.Text.IndexOf(",")!=-1||   
                activateAccountRelativeLayout_ActivateAccountUserNameEditText.Text.IndexOf("[")!=-1 ||  
                activateAccountRelativeLayout_ActivateAccountUserNameEditText.Text.IndexOf("]")!=-1 ||
                activateAccountRelativeLayout_ActivateAccountUserNameEditText.Text.IndexOf("(")!=-1||   
                activateAccountRelativeLayout_ActivateAccountUserNameEditText.Text.IndexOf(")")!=-1)
                {
                    activateAccountRelativeLayout_stateTextView.Text = "用户名内不能包含非法字符！";
                }
                else if (activateAccountRelativeLayout_ActivateAccountPasswordEditText.Text != activateAccountRelativeLayout_ActivateAccountPassword2EditText.Text)
                {
                    activateAccountRelativeLayout_stateTextView.Text = "两次输入的密码不相同";
                }else//所有检查完成且没有发现违法行为，则继续执行注册账户
                {
 connectionServerRelativeLayout.Visibility = ViewStates.Visible;
                    activateAccountRelativeLayout.Visibility = ViewStates.Gone;
                    ConnectStart(true);
                }
            };

            sendChatButton.Click += (object sender, EventArgs e) => { 
                if(sendChatBox.Text!="")
                {
                        WriterVoid("ChatSend");
                    
                }
                else
                {
                    Toast.MakeText(Android.App.Application.Context, "发送信息不能为空！", ToastLength.Long).Show();
                }
            };
            accountCheck.CheckedChange += (object sender, CompoundButton.CheckedChangeEventArgs e) =>
            {
                if (accountCheck.Checked == true)
                {
                    textView3.Enabled = true;
                    textView4.Enabled=true;
                    accountUserNameEditText.Enabled=true;
                    accountPasswordEditText.Enabled=true;
                }else if(accountCheck.Checked == false) { 
                textView3.Enabled=false;
                    textView4.Enabled=false;
                    accountUserNameEditText.Enabled=false;
                    accountPasswordEditText.Enabled=false;
                }
            };
            activateAccountTextView.Click += (object sender, EventArgs e) =>
            {
                if (connectionServerRelativeLayout.Visibility == ViewStates.Visible)
                {
                    activateAccountRelativeLayout_stateTextView.Text = "";
                    activateAccountRelativeLayout.Visibility = ViewStates.Visible;                   
                    connectionServerRelativeLayout.Visibility = ViewStates.Invisible;
                }
            };
            chatRecordsTextView.Click += (object sender, EventArgs e) =>
            {
                if (connectionServerRelativeLayout.Visibility == ViewStates.Visible)
                {
                    ScrollView chatRecords_scrollViewMessageBox = FindViewById<ScrollView>(Resource.Id.chatRecords_scrollViewMessageBox);

                    chatRecordsRelativeLayout.Visibility = ViewStates.Visible;
                    connectionServerRelativeLayout.Visibility = ViewStates.Invisible;

                    try
                    {
                        var dataFile = Path.Combine();
                        dataFile = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "ChatRecords.dat");
                        using var reader = new StreamReader(dataFile, true);               
                        chatRecords_messageBox.Text = reader.ReadToEnd();
                        reader.Close();
                    }
                    catch
                    {                    }
                    chatRecords_messageBox.LayoutChange += (object sender, View.LayoutChangeEventArgs e) =>
                    {
                        chatRecords_scrollViewMessageBox.FullScroll(FocusSearchDirection.Down);
                        chatRecords_scrollViewMessageBox.Focusable = false;
                        chatRecords_messageBox.Focusable = false;
                    };                   
                }
            };


            //messageBox.AfterTextChanged += (object sender, Android.Text.AfterTextChangedEventArgs e) => { AutoScroll(); };
            messageBox.LayoutChange +=(object sender, View.LayoutChangeEventArgs e) => { AutoScroll(); };
            sendChatBox.LayoutChange += (object sender, View.LayoutChangeEventArgs e) => { AutoScroll(); };
            clientNumButton.Click += (object sender, EventArgs e) =>
            {
                if (RequestCooldownThread == null || RequestCooldownThread.IsAlive == false)
                {
                    RequestCooldownThread?.Abort();
                    RequestCooldownThread = new Thread(RequestCooldown)
                    {
                        Name = "请求冷却线程"
                    };
                    RequestCooldownThread.Start();
                WriterVoid("clientNum");
                }
                else
                {
                    Toast.MakeText(Android.App.Application.Context, "短时间内请求次数过多,请稍后再试！", ToastLength.Long).Show();
                }
               
            };
            oldChatButton.Click += (object sender, EventArgs e) =>
            {
                WriterVoid("oldChat");
                //Toast.MakeText(Android.App.Application.Context, "此功能当前正在开发，暂时无法使用！", ToastLength.Long).Show();
            };
            privateSendEditText.FocusChange += (object sender, View.FocusChangeEventArgs e) =>
            {
                if (firstClick[0] == false)
                {
                    firstClick[0] = true;
                    privateSendEditText.SetTextColor(new Android.Graphics.Color(255, 255, 255));
                    privateSendEditText.Text = null;
                }
            };

            chatRecords_exitButton.Click += (object sender, EventArgs e)=>
            {
                if (chatRecordsRelativeLayout.Visibility == ViewStates.Visible)
                {
                    connectionServerRelativeLayout.Visibility = ViewStates.Visible;
                    chatRecordsRelativeLayout.Visibility = ViewStates.Gone;
                }
            };
            chatRecords_clearButton.Click += (object sender, EventArgs e) =>
            {
                logFileWriter?.Close();
                logFileWriter = File.CreateText(Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "ChatRecords.dat"));
                logFileWriter.Write("");
                logFileWriter.Close();
                chatRecords_messageBox.Text = "";
            };

            guanyu.Click += (object sender, EventArgs e) =>
            {
                var msgbox = new Android.App.AlertDialog.Builder(this);
                msgbox.SetTitle("关于");
                msgbox.SetMessage(
                    "程序名：微型通信(客户端)\r\n" +
                    "版本：V" + versionStr + "\r\n" +
					"Copyright (C) 2023-2024 Hagnimik, All rights reserved.");
                msgbox.SetPositiveButton("确定", delegate
                {
                });
                msgbox.Show();
            };


            //扫描文件

            int errorNum = 0;
            try
            {
                var dataFile = Path.Combine();
                dataFile = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "Data.dat");
                using var reader = new StreamReader(dataFile, true);

                errorNum = 1;
                lastIpText = reader.ReadLine();
                lastPortText = reader.ReadLine();
                ipEditText.Text = lastIpText;
                portEditText.Text = lastPortText;
                errorNum = 2;
                string lacStr = reader.ReadLine();
                if (lacStr == "true")
                {
                    lastAccountCheck = true;
                }
                else
                {
                    lastAccountCheck = false;
                }
                lastUserNameText = reader.ReadLine();
                lastPasswordText = reader.ReadLine();
                accountCheck.Checked = lastAccountCheck;
                accountUserNameEditText.Text = lastUserNameText;
                accountPasswordEditText.Text = lastPasswordText;
                reader.Close();
            }
            catch 
                {
                switch (errorNum)
                {
                    case 1:
                        ipEditText.Text = "";
                        portEditText.Text = "";
                        break;
                    case 2:
                        accountCheck.Checked = false;
                        accountUserNameEditText.Text = "";
                        accountPasswordEditText.Text = "";
                        break;
                }
            }





            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.Internet)!= Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.Internet }, 1);
            }
        }


        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {

            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

     
        }

        bool manualDisconnectionBool=false;//判断是否为手动断开连接
        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            RelativeLayout connectionServerRelativeLayout = FindViewById<RelativeLayout>(Resource.Id.connectionServerRelativeLayout);
            RelativeLayout chatRelativeLayout = FindViewById<RelativeLayout>(Resource.Id.chatRelativeLayout);
            RelativeLayout activateAccountRelativeLayout = FindViewById<RelativeLayout>(Resource.Id.activateAccountRelativeLayout);
            RelativeLayout chatRecordsRelativeLayout = FindViewById<RelativeLayout>(Resource.Id.chatRecordsRelativeLayout);

            if (keyCode == Keycode.Back && chatRelativeLayout.Visibility==ViewStates.Visible)
            {
                manualDisconnectionBool=true;
                DisconnectVoid("Disconnection");
                return true;
            }else if(keyCode == Keycode.Back && activateAccountRelativeLayout.Visibility == ViewStates.Visible)
            {
                connectionServerRelativeLayout.Visibility = ViewStates.Visible;
             activateAccountRelativeLayout.Visibility= ViewStates.Gone;
                return true;
            }else if(keyCode==Keycode.Back && chatRecordsRelativeLayout.Visibility == ViewStates.Visible)
            {
                connectionServerRelativeLayout.Visibility = ViewStates.Visible;
                chatRecordsRelativeLayout.Visibility = ViewStates.Gone;
                return true;
            }
            return base.OnKeyDown(keyCode, e);
        }


   
        void AutoScroll()//滚动条随文本增加滚动
        {
            ScrollView scrollViewMessageBox = FindViewById<ScrollView>(Resource.Id.scrollViewMessageBox);
            TextView messageBox = FindViewById<TextView>(Resource.Id.messageBox);

            if (nextOutputStrOldChat.Count != 0)
            {
                scrollViewMessageBox.FullScroll(FocusSearchDirection.Up);
            }
            else
            {
                scrollViewMessageBox.FullScroll(FocusSearchDirection.Down);
            }
            scrollViewMessageBox.Focusable = false;
            messageBox.Focusable = false;
        }
        bool activateAccountConnectBool=false;//判断是否为注册账户而连接服务器
        void ConnectStart(bool activateAccount)//activateAccount表示是否为注册账号而连接服务器
        {
            EditText ipEditText = FindViewById<EditText>(Resource.Id.ipEditText);
            EditText portEditText = FindViewById<EditText>(Resource.Id.portEditText);
            TextView messageBox = FindViewById<TextView>(Resource.Id.messageBox);

            TextView stateTextView = FindViewById<TextView>(Resource.Id.stateTextView);
            Button connectionButton = FindViewById<Button>(Resource.Id.connectionButton);
            EditText accountUserNameEditText = FindViewById<EditText>(Resource.Id.accountUserNameEditText);
            EditText accountPasswordEditText = FindViewById<EditText>(Resource.Id.accountPasswordEditText);
            AndroidX.AppCompat.Widget.AppCompatCheckBox accountCheck = FindViewById<AndroidX.AppCompat.Widget.AppCompatCheckBox>(Resource.Id.accountCheck);

            TextView chatRecordsTextView = FindViewById<TextView>(Resource.Id.chatRecordsTextView);
            TextView activateAccountTextView = FindViewById<TextView>(Resource.Id.activateAccountTextView);
            EditText activateAccountRelativeLayout_ActivateAccountIdEditText = FindViewById<EditText>(Resource.Id.activateAccountRelativeLayout_ActivateAccountIdEditText);
            EditText activateAccountRelativeLayout_ActivateAccountUserNameEditText = FindViewById<EditText>(Resource.Id.activateAccountRelativeLayout_ActivateAccountUserNameEditText);
            EditText activateAccountRelativeLayout_ActivateAccountPasswordEditText = FindViewById<EditText>(Resource.Id.activateAccountRelativeLayout_ActivateAccountPasswordEditText);

            if (ConnectCooldownThread==null || ConnectCooldownThread.IsAlive == false)
            {
                ConnectCooldownThread?.Abort();
                ConnectCooldownThread = new System.Threading.Thread(ConnectCooldown)
                {
                    Name="连接能却线程"
                };
                ConnectCooldownThread.Start();
                if (ipEditText.Text != "" && portEditText.Text != "")
                {
                    connectionButton.Enabled = false;
                    activateAccountTextView.Enabled = false;
                chatRecordsTextView.Enabled = false;
                    ipEditText.Enabled = false;
                    portEditText.Enabled = false;

                    tcpListenThread?.Abort();
                    tcpConnectThread?.Abort();
                    tcpConnectTimeThread?.Abort();

                    activateAccountConnectBool = activateAccount;

                    colorSpanText = new SpannableStringBuilder();
                    messageBox.Text = null;
                  

                    tcpConnectTimeThread = new System.Threading.Thread(TCPConnectTime);
                    tcpConnectTimeThread.Start();

                    //如果数据更改则保存文件
                    if (lastIpText != ipEditText.Text || lastPortText != portEditText.Text ||
                    lastAccountCheck != accountCheck.Checked || lastUserNameText != accountUserNameEditText.Text || lastPasswordText != accountPasswordEditText.Text)
                    {
                        try
                        {
                            var dataFile = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "Data.dat");
                            using var writer = File.CreateText(dataFile);
                            writer.WriteLine(ipEditText.Text);
                            writer.WriteLine(portEditText.Text);
                            if (accountCheck.Checked == true)
                            {
                                writer.WriteLine("true");
                            }
                            else
                            {
                                writer.WriteLine("false");
                            }
                            writer.WriteLine(accountUserNameEditText.Text);
                            writer.WriteLine(accountPasswordEditText.Text);
                            writer.Close();
                        }
                        catch { }

                        lastIpText = ipEditText.Text;
                        lastPortText = portEditText.Text;
                        lastAccountCheck = accountCheck.Checked;
                        lastUserNameText = accountUserNameEditText.Text;
                        lastPasswordText = accountPasswordEditText.Text;
                    }
                }
                else
                {
                    if (ipEditText.Text == "" && portEditText.Text != "")
                    {
                        stateTextView.Text = "请填入服务器地址";
                    }
                    else if (portEditText.Text == "" && ipEditText.Text != "")
                    {
                        stateTextView.Text = "请填入端口";
                    }
                    else
                    {
                        stateTextView.Text = "请填入服务器地址和端口";
                    }
                }
            }
            else
            {
                Toast.MakeText(Android.App.Application.Context, "短时间内连接次数过多,请稍后再试！", ToastLength.Long).Show();
            }
        }

        //RunOnUiThread(() =>{        });
        bool tcpConnectOutTimeBool=false;
        void TCPConnectTime()//连接计时块
        {
                tcpConnectThread = new System.Threading.Thread(TCPConnect);
                tcpConnectThread.Start();
                System.Threading.Thread.Sleep(6444);//计时，超出该时间后判断为连接超时
                tcpConnectOutTimeBool = true;
                DisconnectVoid("ConnectOutTime");           
        }
        void ConnectCooldown()//连接冷却线程
        {
            System.Threading.Thread.Sleep(3000);
            ConnectCooldownThread?.Abort();
        }
        void RequestCooldown()//请求冷却线程
        {
            Thread.Sleep(3500);
            RequestCooldownThread?.Abort();
        }
        void TCPConnect()//连接服务器块
        {
            RelativeLayout connectionServerRelativeLayout = FindViewById<RelativeLayout>(Resource.Id.connectionServerRelativeLayout);
            RelativeLayout chatRelativeLayout = FindViewById<RelativeLayout>(Resource.Id.chatRelativeLayout);
            Button connectionButton = FindViewById<Button>(Resource.Id.connectionButton);
            EditText ipEditText = FindViewById<EditText>(Resource.Id.ipEditText);
            EditText portEditText = FindViewById<EditText>(Resource.Id.portEditText);
            TextView stateTextView = FindViewById<TextView>(Resource.Id.stateTextView);
            TextView messageBox = FindViewById<TextView>(Resource.Id.messageBox);
            Button sendChatButton = FindViewById<Button>(Resource.Id.sendChatButton);
            TextView sendChatBox = FindViewById<TextView>(Resource.Id.sendChatBox);
            TextView chatRecordsTextView = FindViewById<TextView>(Resource.Id.chatRecordsTextView);
            TextView activateAccountTextView = FindViewById<TextView>(Resource.Id.activateAccountTextView);
            EditText activateAccountRelativeLayout_ActivateAccountIdEditText = FindViewById<EditText>(Resource.Id.activateAccountRelativeLayout_ActivateAccountIdEditText);
            EditText activateAccountRelativeLayout_ActivateAccountUserNameEditText = FindViewById<EditText>(Resource.Id.activateAccountRelativeLayout_ActivateAccountUserNameEditText);
            EditText activateAccountRelativeLayout_ActivateAccountPasswordEditText = FindViewById<EditText>(Resource.Id.activateAccountRelativeLayout_ActivateAccountPasswordEditText);
            AndroidX.AppCompat.Widget.AppCompatCheckBox accountCheck = FindViewById<AndroidX.AppCompat.Widget.AppCompatCheckBox>(Resource.Id.accountCheck);
            EditText accountUserNameEditText = FindViewById<EditText>(Resource.Id.accountUserNameEditText);
            EditText accountPasswordEditText = FindViewById<EditText>(Resource.Id.accountPasswordEditText);

            tcpConnectOutTimeBool = false;//重置此变量的值，以保证重新连接后正常使用
            switch (activateAccountConnectBool)
            {
                case true:
                    RunOnUiThread(() => { stateTextView.Text = "正在注册账号"; });
                    break;
                case false:
                    RunOnUiThread(() => { stateTextView.Text = "正在连接服务器"; });
                    break;
            }
            string[] classOutput=new string[2];//输出解密相关信息
            try
            {
                #region IP地址解密块
                string decryptedIP = null;
             decryptedIP = ipEditText.Text;
                if (decryptedIP.Length > 15)//如果字符串大于15，就说明是加密形式的IP地址,加密方法为二代XHM加密法
                {
                    _2ndGenerationEncryptionMethodsFromHgnim emfHg2g = new _2ndGenerationEncryptionMethodsFromHgnim();                   
                     classOutput = emfHg2g.DecryptInoutPut(new string[1] { decryptedIP }, "@*%M_C$&#", "text"); 
                    //加密密钥为"@#$MC&*%"，这个密钥需要与服务器相匹配，所以更改密钥后要保持服务器版本一致
                    if (classOutput[0] == "output")
                    {
                        decryptedIP = classOutput[1];
                    }
                    else if (classOutput[0]== "PwWrong")//如果密钥不一致，就说明服务器与客户端版本不一致
                    {
                        decryptedIP = "-PasswordError-";
                        throw new System.Exception();//手动抛出异常                   
                    }
                    else
                    {
                        decryptedIP = "error";
                    }                                     
                }
                #endregion

                tcpClient = new TcpClient();                
                tcpClient.Connect(decryptedIP, int.Parse(portEditText.Text));
                tcpConnectTimeThread?.Abort();
                if (tcpClient != null)
                {
                    if (activateAccountConnectBool == false)
                    {
                        RunOnUiThread(() =>
                        {
                            stateTextView.Text = "正在验证……";                           
                        });
                    }

                    networkStream = tcpClient.GetStream();
                    reader = new BinaryReader(networkStream);
                    writer = new BinaryWriter(networkStream);
                    IPAddress[] iPs = Dns.GetHostAddresses(Dns.GetHostName());
                    foreach (IPAddress iPAddress in iPs)
                    {
                        if (!iPAddress.IsIPv6SiteLocal)
                        {
                            myLocalip = iPAddress.ToString();
                        }
                        else
                        {
                            try { myLocalip = iPAddress.ToString(); }
                            catch { myLocalip = "IPv6"; }
                        }
                    }

                    switch (activateAccountConnectBool)
                    {
                        case true:
                            writer.Write(EncryptStr(true,"clientIP-activateAccount|" + myLocalip));
                            writer.Write(EncryptStr(true,"version|" + versionStr));
                            writer.Write(EncryptStr(true,"activateAccount-ID|" + activateAccountRelativeLayout_ActivateAccountIdEditText.Text));
                            writer.Write(EncryptStr(true,"activateAccount-UserName|" + activateAccountRelativeLayout_ActivateAccountUserNameEditText.Text));
                            writer.Write(EncryptStr(true,"activateAccount-Password|" +  activateAccountRelativeLayout_ActivateAccountPasswordEditText.Text));
                            break;
                        case false:
                            switch (accountCheck.Checked)
                            {
                                case true:
                                    writer.Write(EncryptStr(true,"clientIP|" + myLocalip));
                                    writer.Write(EncryptStr(true,"version|" + versionStr));
                                    writer.Write(EncryptStr(true,"AccountVerify-UserName|" + accountUserNameEditText.Text));
                                    writer.Write(EncryptStr(true,"AccountVerify-Password|"+accountPasswordEditText.Text));
                                    break;
                                case false:
                                    writer.Write(EncryptStr(true,"clientIP|" + myLocalip));
                                    writer.Write(EncryptStr(true,"version|" + versionStr));
                                    writer.Write(EncryptStr(true,"AccountVerify-TF|" + "false"));
                                    break;
                            }                        
                            break;
                    }

                    nextOutputStr = new System.Collections.ArrayList();
                    timeChatOutputThread?.Abort();
                    timeChatOutputThread = new Thread(TimeChatOutput)
                    {
                        Name = "逐字输出线程"
                    };
                    timeChatOutputThread.Start();

                    tcpListenThread?.Abort();
                    tcpListenThread = new System.Threading.Thread(TCPListen)
                    {
                        Name="信息侦听线程"
                    };
                    tcpListenThread.Start();
                    stayConnectThread?.Abort();
                    stayConnectThread = new System.Threading.Thread(StayConnect)
                    {
                        Name="保持连接线程"
                    };
                    stayConnectThread.Start();

         

                     //messageBox.Append("[" + TimeMDHMS + "][系统信息]" + "服务器连接成功！\r\n");
                     nextOutputStr.Add("[" + TimeMDHMS + "][系统信息]" + "服务器连接成功！");
                }
                else
                {
                    tcpConnectTimeThread?.Abort();
                    RunOnUiThread(() =>
                    {
                        stateTextView.Text = "服务器连接失败！";
                        connectionButton.Enabled = true;
                        activateAccountTextView.Enabled = true;
                        chatRecordsTextView.Enabled = true;
                        ipEditText.Enabled = true;
                        portEditText.Enabled = true;
                    });
                    tcpConnectThread?.Abort();
                }
            }
            catch
            {
                if (tcpConnectOutTimeBool == false)  //线程被中途终止后，会发生错误，如果为相应终止，将不执行错误块
                {
                    tcpConnectTimeThread?.Abort();
                    RunOnUiThread(() =>
                    {
                        if (classOutput[0] == "PwWrong"){ stateTextView.Text = "版本不匹配！"; }
                        else{stateTextView.Text = "服务器连接失败！";}
                        connectionButton.Enabled = true;
                        activateAccountTextView.Enabled =true; 
                        chatRecordsTextView.Enabled = true;
                       ipEditText.Enabled = true;
                        portEditText.Enabled = true;
                    });
                    tcpConnectThread?.Abort();
                }
            }

        }

        void TCPListen()//信息侦听
        {
            manualDisconnectionBool = false;//重置此变量的值，以保证重新连接后正常使用
            bool serverCloseBool=false;//判断是否已经接收到服务端发来的关闭信息
            string strReader, strReaderId, strReaderMessage = null;

            TextView messageBox = FindViewById<TextView>(Resource.Id.messageBox);
            RelativeLayout connectionServerRelativeLayout = FindViewById<RelativeLayout>(Resource.Id.connectionServerRelativeLayout);
            RelativeLayout chatRelativeLayout = FindViewById<RelativeLayout>(Resource.Id.chatRelativeLayout);
            AndroidX.AppCompat.Widget.AppCompatCheckBox accountCheck = FindViewById<AndroidX.AppCompat.Widget.AppCompatCheckBox>(Resource.Id.accountCheck);
            EditText accountUserNameEditText = FindViewById<EditText>(Resource.Id.accountUserNameEditText);

            while (true)
            {
                try
                {
                    strReader =EncryptStr(false, reader.ReadString());
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

                        switch (strReaderId)
                        {
                            case "serverChat":
                                    //messageBox.Append("[" + TimeMDHMS + "][Server]" + strReaderMessage + "\r\n");
                                    nextOutputStr.Add("[" + TimeMDHMS + "][Server]" + strReaderMessage);
                                break;
                            case "command":
                                switch (strReaderMessage)
                                {
                                    case "ServerClose":
                                        serverCloseBool = true;
                                        DisconnectVoid("ServerClose");
                                        break;
                                    case "versionNotPass":
                                        serverCloseBool = true;
                                        DisconnectVoid("versionNotPass");
                                        break;
                                    case "full":
                                        serverCloseBool = true;
                                        DisconnectVoid("full");
                                        break;
                                    case "kick":
                                        serverCloseBool = true;
                                        DisconnectVoid("kick");
                                        break;
                                    case "kickBan":
                                        serverCloseBool = true;
                                        DisconnectVoid("kickBan");
                                        break;
                                    case "passSucceed":
                                        if (activateAccountConnectBool == false)
                                        {
                                            RunOnUiThread(() =>
                                            {
                                                chatRelativeLayout.Visibility = Android.Views.ViewStates.Visible;
                                                connectionServerRelativeLayout.Visibility = Android.Views.ViewStates.Invisible;
                                            });
                                            if (accountCheck.Checked == true)
                                                {
                                                   //messageBox.Append("[" + TimeMDHMS + "][系统信息]" + "信息验证成功！当前账户：" + accountUserNameEditText.Text + "\r\n");
                                                    nextOutputStr.Add("[" + TimeMDHMS + "][系统信息]" + "信息验证成功！当前账户：" + accountUserNameEditText.Text);
                                                }
                                                else
                                                {
                                                    //messageBox.Append("[" + TimeMDHMS + "][系统信息]" + "信息验证成功！当前未使用账户" + "\r\n");
                                                    nextOutputStr.Add("[" + TimeMDHMS + "][系统信息]" + "信息验证成功！当前未使用账户");
                                                }                                         
                                        }                                                                   
                                        break;
                                    case "StayConnectFalse":
                                        serverCloseBool = true;
                                        DisconnectVoid("StayConnectFalse");
                                        break;
                                    case "activateAccount-succeed":
                                        serverCloseBool = true;
                                        DisconnectVoid("activateAccount-succeed");
                                        break;
                                    case "activateAccount-IDnot":
                                        serverCloseBool = true;
                                        DisconnectVoid("activateAccount-IDnot");
                                        break;
                                    case "activateAccount-IDStartTrue":
                                        serverCloseBool = true;
                                        DisconnectVoid("activateAccount-IDStartTrue");
                                        break;
                                    case "activateAccount-UserNameDuplication":
                                            serverCloseBool=true;
                                        DisconnectVoid("activateAccount-UserNameDuplication");
                                        break;
                                    case "accountVerify-null":
                                        serverCloseBool = true;
                                        DisconnectVoid("accountVerify-null");
                                            break;
                                    case "accountVerify-passwordError":
                                        serverCloseBool = true;
                                        DisconnectVoid("accountVerify-passwordError");
                                            break;
                                    case "accountVerify-false":
                                        serverCloseBool = true;
                                        DisconnectVoid("accountVerify-false");
                                        break;
                                    case "accountVerify-ban":
                                        serverCloseBool = true;
                                        DisconnectVoid("accountVerify-ban");
                                        break;
                                }
                                break;
                            case "clientChat":
                            case "PrivateClientChat":
                            case "PrivateClientChatError":
                                //messageBox.Append("[" + TimeMDHMS + "]" + strReaderMessage + "\r\n");
                                nextOutputStr.Add("[" + TimeMDHMS + "]" + strReaderMessage);                   
                                break;

                            case "clientConnect":
                    
                                    //messageBox.Append("[" + TimeMDHMS + "][系统信息]客户端" + strReaderMessage + "已连接"+"\r\n");
                                    nextOutputStr.Add("[" + TimeMDHMS + "][系统信息]客户端" + strReaderMessage + "已连接");
                       
                                break;
                            case "clientDis":
                                nextOutputStr.Add("[" + TimeMDHMS + "][系统信息]客户端" + strReaderMessage + "已断开连接");
                                break;
                            case "clientKick":
                                nextOutputStr.Add("[" + TimeMDHMS + "][系统信息]服务器已将客户端" + strReaderMessage + "断开连接");
                                break;
                            case "clientKickBan":
                                nextOutputStr.Add("[" + TimeMDHMS + "][系统信息]服务器已将客户端" + strReaderMessage + "禁封！");
                                break;
                            case "clientStayConnectFalse":
                                nextOutputStr.Add("[" + TimeMDHMS + "][系统信息]客户端" + strReaderMessage + "连接超时！");
                                break;
                            case "clientNum":
                                nextOutputStr.Add("[" + TimeMDHMS + "][系统信息]" + strReaderMessage);
                                break;
                            case "oldChat":
                                if (strReaderMessage == "")
                                {
                                    RunOnUiThread(() => { Toast.MakeText(Android.App.Application.Context, "没有可请求的旧信息！", ToastLength.Long).Show(); });                              
                                    oldChatLock = true;
                                }                                
                                else
                                {
                                    for (int i = 0; i < strReaderMessage.Split("\r\n").Length; i++)
                                    {
                                        nextOutputStrOldChat.Add(strReaderMessage.Split("\r\n")[i]);
                                    }
                                    for (int i = strReaderMessage.Split("\r\n").Length - 1; i >= 0; i--)
                                    {
                                        logFileWriter.WriteLine("[旧信息]" + strReaderMessage.Split("\r\n")[i]);
                                    }
                                }                                                                   
                                break;
                        }
                    }
                }
                catch(System.Exception ex) 
                {
                    Log.Error("接受信息块出错", ex.ToString());
                    if (serverCloseBool == false && manualDisconnectionBool==false)//如果值为true，则为接收到服务端发来的信息后，线程会发生错误，但不再执行断开连接块了
                    {
                        DisconnectVoid("UnexpectedDisconnection");
                    }             
                }
                System.Threading.Thread.Sleep(1);
            }
        }
        void WriterVoid(string id)//信息发出
        {
            //TextView messageBox = FindViewById<TextView>(Resource.Id.messageBox);
            TextView sendChatBox = FindViewById<TextView>(Resource.Id.sendChatBox);
            AndroidX.AppCompat.Widget.AppCompatCheckBox accountCheck = FindViewById<AndroidX.AppCompat.Widget.AppCompatCheckBox>(Resource.Id.accountCheck);
            EditText accountUserNameEditText = FindViewById<EditText>(Resource.Id.accountUserNameEditText);
            EditText privateSendEditText = FindViewById<EditText>(Resource.Id.privateSendEditText);

            if (writer != null)
            {
                try
                {
                    switch (id)
                    {
                        case "ChatSend":
                            if (privateSendEditText.Text != "" && firstClick[0] == true)
                            {
                                if (accountCheck.Checked == true)
                                {
                                    if (privateSendEditText.Text.IndexOf("|") != -1 ||
     privateSendEditText.Text.IndexOf(",") != -1 ||
     privateSendEditText.Text.IndexOf("[") != -1 ||
     privateSendEditText.Text.IndexOf("]") != -1 ||
     privateSendEditText.Text.IndexOf("(") != -1 ||
     privateSendEditText.Text.IndexOf(")") != -1)
                                    {
                                        nextOutputStr.Add("[" + TimeMDHMS + "][系统信息]私信用户名指定框中不能包含非法字符！");
                                        break;
                                    }
                                   else if (privateSendEditText.Text.IndexOf("@") == -1)
                                    {
                                        nextOutputStr.Add("[" + TimeMDHMS + "][系统信息]私信用户名格式错误！");
                                        break;
                                    }
                                    else
                                    {
                                        writer.Write(EncryptStr(true,"chat" + privateSendEditText.Text + "|" + sendChatBox.Text));                                     
                                    }
                                }
                                else
                                {
                                    nextOutputStr.Add("[" + TimeMDHMS + "][系统信息]当前未登录账户，无法使用私信功能！");
                                }
                            }
                            else
                            {
                                writer.Write(EncryptStr(true,"chat|" + sendChatBox.Text));
                                if (accountCheck.Checked == true)
                                {
                                    //messageBox.Append("[" + TimeMDHMS + "][" + accountUserNameEditText.Text + "]" + sendChatBox.Text + "\r\n");
                                    nextOutputStr.Add("[" + TimeMDHMS + "][" + accountUserNameEditText.Text + "]" + sendChatBox.Text);
                                }
                                else
                                {
                                    //messageBox.Append("[" + TimeMDHMS + "][我][" + myLocalip + "]" + sendChatBox.Text + "\r\n");
                                    nextOutputStr.Add("[" + TimeMDHMS + "][我][" + myLocalip + "]" + sendChatBox.Text);
                                }                         
                            }
                            sendChatBox.Text = null;
                            break;
                        case "Disconnection":
                            writer.Write(EncryptStr(true,"command|Disconnection"));
                            break;
                        case "StayConnect":
                            writer.Write(EncryptStr(true,"command|StayConnect"));
                            break;
                        case "clientNum"://请求客户端数量信息
                            writer.Write(EncryptStr(true,"command|clientNum"));
                            break;
                        //此功能正在尝试开发
                        case "oldChat"://请求旧信息

                            //由服务端计算上一次连接时间
                            //if (lastConnectServerTime == null)
                            //{
                            //    string str, lastStr = null;
                            //    var dataFile = Path.Combine();
                            //    dataFile = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "ChatRecords.dat");
                            //    using var reader = new StreamReader(dataFile, true);
                            //    while (true)
                            //    {
                            //        str = reader.ReadLine();
                            //        if (str == null)
                            //        {
                            //            lastConnectServerTime = lastStr.Split("]")[0].Substring(1, lastStr.Split("]")[0].Length - 1);
                            //            break;
                            //        }
                            //        else
                            //        {
                            //            lastStr = str;
                            //        }
                            //    }
                            //    reader.Close();
                            //}

                            if (nextOutputStrOldChat.Count != 0)
                            {
                                RunOnUiThread(() => { Toast.MakeText(Android.App.Application.Context, "旧信息请求过快！", ToastLength.Long).Show(); });
                            }
                            else if (oldChatLock == false)
                            {
                                writer.Write(EncryptStr(true,"command|oldChat"));
                            }                           
                            else
                            {
                                Toast.MakeText(Android.App.Application.Context, "没有可请求的旧信息！", ToastLength.Long).Show();
                            }                       
                            break;
                    }
                }
                catch 
                {
                    switch (id)
                    {
                        case "ChatSend":
                            if (accountCheck.Checked == true)
                            {
                                //messageBox.Append("[" + TimeMDHMS + "][" + accountUserNameEditText.Text + "]" + "[发送失败]" + sendChatBox.Text + "\r\n");
                                nextOutputStr.Add("[" + TimeMDHMS + "][" + accountUserNameEditText.Text + "]" + "[发送失败]" + sendChatBox.Text);
                            }
                            else
                            {
                                //messageBox.Append("[" + TimeMDHMS + "][我][" + myLocalip + "]" + "[发送失败]" + sendChatBox.Text + "\r\n");
                                nextOutputStr.Add("[" + TimeMDHMS + "][我][" + myLocalip + "]" + "[发送失败]" + sendChatBox.Text);
                            }                        
                            sendChatBox.Text = null;
                            break;
                    }
                }             
            }
            else
            {
                switch (id)
                {
                    case "ChatSend":
                        if (accountCheck.Checked == true)
                        {
                            //messageBox.Append("[" + TimeMDHMS + "][" + accountUserNameEditText.Text + "]" + "[发送失败]" + sendChatBox.Text + "\r\n");
                            nextOutputStr.Add("[" + TimeMDHMS + "][" + accountUserNameEditText.Text + "]" + "[发送失败]" + sendChatBox.Text);
                        }
                        else
                        {
                            //messageBox.Append("[" + TimeMDHMS + "][我][" + myLocalip + "]" + "[发送失败]" + sendChatBox.Text + "\r\n");
                            nextOutputStr.Add("[" + TimeMDHMS + "][我][" + myLocalip + "]" + "[发送失败]" + sendChatBox.Text);
                        }
                        sendChatBox.Text = null;
                        break;
                }
            }
        }
        void DisconnectVoid(string id)//断开连接块
        {
            RelativeLayout connectionServerRelativeLayout = FindViewById<RelativeLayout>(Resource.Id.connectionServerRelativeLayout);
            RelativeLayout chatRelativeLayout = FindViewById<RelativeLayout>(Resource.Id.chatRelativeLayout);
            TextView stateTextView = FindViewById<TextView>(Resource.Id.stateTextView);
            TextView messageBox = FindViewById<TextView>(Resource.Id.messageBox);
            EditText ipEditText = FindViewById<EditText>(Resource.Id.ipEditText);
            EditText portEditText = FindViewById<EditText>(Resource.Id.portEditText);
            Button connectionButton = FindViewById<Button>(Resource.Id.connectionButton);
            TextView chatRecordsTextView = FindViewById<TextView>(Resource.Id.chatRecordsTextView);
            TextView activateAccountTextView = FindViewById<TextView>(Resource.Id.activateAccountTextView);
            EditText activateAccountRelativeLayout_ActivateAccountIdEditText = FindViewById<EditText>(Resource.Id.activateAccountRelativeLayout_ActivateAccountIdEditText);
            EditText activateAccountRelativeLayout_ActivateAccountUserNameEditText = FindViewById<EditText>(Resource.Id.activateAccountRelativeLayout_ActivateAccountUserNameEditText);
            EditText activateAccountRelativeLayout_ActivateAccountPasswordEditText = FindViewById<EditText>(Resource.Id.activateAccountRelativeLayout_ActivateAccountPasswordEditText);
            EditText activateAccountRelativeLayout_ActivateAccountPassword2EditText = FindViewById<EditText>(Resource.Id.activateAccountRelativeLayout_ActivateAccountPassword2EditText);

            switch (id)
            {
                case "Disconnection":
                    WriterVoid("Disconnection");
                    break;
            }


            tcpClient?.Close();
            networkStream?.Close();
            reader?.Close();
            writer?.Close();
            oldChatLock = false;
            nextOutputStr = new System.Collections.ArrayList();
            nextOutputStrOldChat = new System.Collections.ArrayList();
            colorSpanText = new SpannableStringBuilder();
            //lastConnectServerTime = null;


            switch (id)
            {
                case "UnexpectedDisconnection":                                   
                    RunOnUiThread(() => {
                        stateTextView.Text = "失去连接！";
                    });                
                    break;
                case "ServerClose":                                
                    RunOnUiThread(() => {
                        stateTextView.Text = "服务器已关闭！";
                    });                                  
                    break;
                case "Disconnection":                              
                    RunOnUiThread(() => {
                        stateTextView.Text = "已断开连接！"; });                    
                    break;
                case "ConnectOutTime":                
                    RunOnUiThread(() => {
                        stateTextView.Text = "连接超时！服务器连接失败！";
                    });
                    break;
                case "versionNotPass":
                    RunOnUiThread(() => {
                        stateTextView.Text = "版本不匹配！";
                    });
                    break;
                case "full":
                    RunOnUiThread(() => {
                        stateTextView.Text = "服务器已满！";
                    });
                    break;
                case "kick":
                    RunOnUiThread(() => {
                        stateTextView.Text = "你已被服务器断开连接！";
                    });
                    break;
                case "kickBan":
                    RunOnUiThread(() => {
                        stateTextView.Text = "你已被服务器禁封！";
                    });
                    break;
                case "StayConnectFalse":
                    RunOnUiThread(() => {
                        stateTextView.Text = "无法与服务器保持连接！连接超时！";
                    });
                    break;
                case "activateAccount-succeed":
                    RunOnUiThread(() => {
                        activateAccountRelativeLayout_ActivateAccountIdEditText.Text = "";
                        activateAccountRelativeLayout_ActivateAccountPasswordEditText.Text = "";
                        activateAccountRelativeLayout_ActivateAccountPassword2EditText.Text = "";
                        activateAccountRelativeLayout_ActivateAccountUserNameEditText.Text = "";//放在注册成功后清除

                        stateTextView.Text = "账号注册成功！";
                    });
                    break;
                case "activateAccount-IDnot":
                    RunOnUiThread(() => {
                        stateTextView.Text = "账号注册失败！激活ID不存在！";
                    });
                    break;
                case "activateAccount-IDStartTrue":
                    RunOnUiThread(() => {
                        stateTextView.Text = "账号注册失败！激活ID已被激活！";
                    });
                    break;
                case "activateAccount-UserNameDuplication":
                    RunOnUiThread(() => {
                        stateTextView.Text = "账号注册失败！用户名已存在！";
                    });
                    break;
                case "accountVerify-null":
                    RunOnUiThread(() => {
                        stateTextView.Text = "验证失败！账户不存在！";
                    });
                    break;
                case "accountVerify-passwordError":
                    RunOnUiThread(() => {
                        stateTextView.Text = "验证失败！账户密码错误！";
                    });
                    break;
                case "accountVerify-false":
                    RunOnUiThread(() => {
                        stateTextView.Text = "验证失败！请登录账户！";
                    });
                    break;
                case "accountVerify-ban":
                    RunOnUiThread(() => {
                        stateTextView.Text = "验证失败！当前账户已被禁封！";
                    });
                    break;
            }
            RunOnUiThread(() => {
                messageBox.Text = null;
                connectionServerRelativeLayout.Visibility = Android.Views.ViewStates.Visible;
                chatRelativeLayout.Visibility = Android.Views.ViewStates.Gone;

                connectionButton.Enabled = true;
                activateAccountTextView.Enabled = true;
                chatRecordsTextView.Enabled = true;
               ipEditText.Enabled = true;
                portEditText.Enabled = true;
            });

            if (timeChatOutputThread!=null && timeChatOutputThread.IsAlive == true)
            {
                timeChatOutputThread.Abort();
                timeChatOutputThread = null;

                logFileWriter.WriteLine("[" + TimeMDHMS + "]" +stateTextView.Text);
                logFileWriter.Close();
            }
            if (tcpListenThread != null && tcpListenThread.IsAlive == true)
            {              
                    tcpListenThread.Abort();
                    tcpListenThread = null;
            }
            if(tcpConnectThread != null && tcpConnectThread.IsAlive==true) 
            { 
                tcpConnectThread.Abort();
                tcpConnectThread = null;
            }
            if(tcpConnectTimeThread != null && tcpConnectTimeThread.IsAlive == true)
            {
                tcpConnectTimeThread.Abort();
                tcpConnectTimeThread = null;
            }
        }

        void StayConnect()//与服务端保持连接线程
        {
            while (true)
            {
                WriterVoid("StayConnect");
                System.Threading.Thread.Sleep(5000);
            }
        }


        //逐字输出块
        StreamWriter logFileWriter;
        System.Collections.ArrayList nextOutputStr = new System.Collections.ArrayList();
        System.Collections.ArrayList nextOutputStrOldChat = new System.Collections.ArrayList();
        int[] nextOutputIndex = new int[2];

        SpannableStringBuilder colorSpanText = new SpannableStringBuilder();
        void TimeChatOutput()
        {
            TextView messageBox = FindViewById<TextView>(Resource.Id.messageBox);
            
            logFileWriter?.Close();
            logFileWriter = File.AppendText(Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "ChatRecords.dat"));
            logFileWriter.AutoFlush = true;
            if(File.ReadAllText(Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "ChatRecords.dat")) != "")
            {
                logFileWriter.WriteLine("\r\n");
            }
            while (true)
            {
                if (nextOutputStr.Count != 0 || nextOutputStrOldChat.Count !=0)
                {
                    if (nextOutputStr.Count != 0)
                    {
                        for (nextOutputIndex[0] = 0; nextOutputIndex[0] < nextOutputStr.Count; nextOutputIndex[0]++)
                        {
                            if (nextOutputStr[nextOutputIndex[0]] != null)
                            {
                                string sendStr = nextOutputStr[nextOutputIndex[0]].ToString();//sendStrObj.ToString();
                                int ctCs = 0;
                                logFileWriter.WriteLine(sendStr);
                                //try
                                //{
                                int colorNum = -1;
                                //if (sendStr.Split("]")[0]=="[旧信息")
                                //{
                                //    if (sendStr.Split("]")[3].IndexOf("私信至") != -1)
                                //    {
                                //        colorNum = 1;
                                //    }else if(sendStr.Split("]")[2].IndexOf("[") != -1)
                                //    {
                                //        colorNum = 2;
                                //    }
                                //}
                                //else 
                                if(sendStr.Split("]")[1] == "[系统信息")
                                {
                                    colorNum = 3;
                                }
                                else if(sendStr.Split("]")[2] == "[发送失败")
                                {
                                    colorNum = 4;
                                }
                                else if (sendStr.Split("]")[2].IndexOf("私信至") != -1)
                                {
                                    colorNum = 5;
                                }
                                else if (sendStr.Split("]")[1] == "[Server")
                                {
                                    colorNum = 6;
                                }
                                else if(sendStr.Split("]")[1].IndexOf("[") != -1)
                                {
                                    colorNum = 7;
                                }
                                SpannableStringBuilder span = null;
                                Color spanColor =Color.White;
                                while (true)
                                {

                                    if (ctCs < sendStr.Length)
                                    {
                                        RunOnUiThread(() =>
                                        {
                                            if (ctCs < sendStr.Length)
                                            {
                                                string[] splitStr = sendStr.Split("]");
                                                switch (colorNum)
                                                {
                                                    //case 1:
                                                    //    if (ctCs <= splitStr[0].Length)//[旧信息]
                                                    //    {
                                                    //        spanColor = Color.Gray;
                                                    //    }else if (ctCs <= splitStr[1].Length)//时间
                                                    //    {
                                                    //        spanColor = Color.Orange;
                                                    //    }else if (ctCs <= splitStr[2].Length)//用户名
                                                    //    {
                                                    //        spanColor = Color.MediumTurquoise;
                                                    //    }else//文本
                                                    //    {
                                                    //        spanColor = Color.White;
                                                    //    }
                                                    //    break;
                                                    case 3://系统信息样式
                                                        if (ctCs <= splitStr[0].Length)//时间
                                                        {
                                                            spanColor = Color.Orange;
                                                        }else if(ctCs<= splitStr[1].Length+ splitStr[0].Length+1)//[系统信息]
                                                        {
                                                            spanColor = Color.Blue;
                                                        }else//文本
                                                        {
                                                            spanColor = Color.White;
                                                        }
                                                        break;
                                                    case 4://私信失败样式
                                                        if (ctCs <= splitStr[0].Length)//时间
                                                        {
                                                            spanColor = Color.Orange;
                                                        }
                                                        else if (ctCs <= splitStr[1].Length + splitStr[0].Length+1)//用户名
                                                     {
                                                         spanColor = Color.MediumTurquoise;
                                                     }
                                                        else if(ctCs<= splitStr[2].Length+ splitStr[1].Length + splitStr[0].Length+2)//[发送失败]
                                                        {
                                                            spanColor = Color.OrangeRed;
                                                        }
                                                        else if(ctCs <= splitStr[3].Length + splitStr[2].Length + splitStr[1].Length + splitStr[0].Length+3)//[私信至
                                                        {
                                                            spanColor = Color.SkyBlue;
                                                        }
                                                        else//文本
                                                                {
                                                            spanColor = Color.White;
                                                        }
                                                        break;
                                                    case 5://私信至
                                                        if (ctCs <= splitStr[0].Length)//时间
                                                        {
                                                            spanColor = Color.Orange;
                                                        }
                                                        else if (ctCs <= splitStr[1].Length + splitStr[0].Length+1)//用户名
                                                        {
                                                            spanColor = Color.MediumTurquoise;
                                                        }
                                                        else if (ctCs <= splitStr[2].Length + splitStr[1].Length + splitStr[0].Length+2)//[私信至
                                                        {
                                                            spanColor = Color.SkyBlue;
                                                        }
                                                        else//文本
                                                        {
                                                            spanColor = Color.White;
                                                        }
                                                        break;
                                                    case 6://服务器发送的信息
                                                        if (ctCs <= splitStr[0].Length)//时间
                                                        {
                                                            spanColor = Color.Orange;
                                                        }
                                                        else if (ctCs <= splitStr[1].Length + splitStr[0].Length+1)//server
                                                        {
                                                            spanColor = Color.CornflowerBlue;
                                                        }
                                                        else//文本
                                                        {
                                                            spanColor = Color.White;
                                                        }
                                                        break;
                                                    case 7://用户正常信息
                                                        if (ctCs <= splitStr[0].Length)//时间
                                                        {
                                                            spanColor = Color.Orange;
                                                        }
                                                        else if (ctCs <= splitStr[1].Length + splitStr[0].Length+1)//用户名
                                                        {
                                                            spanColor = Color.MediumTurquoise;
                                                        }
                                                        else//文本
                                                        {
                                                            spanColor = Color.White;
                                                        }
                                                        break;
                                                    default:
                                                        spanColor = Color.Silver;
                                                        break;
                                                }
                                                //messageBox.Append(sendStr.Substring(ctCs, 1));
                                                span = new SpannableStringBuilder(sendStr.Substring(ctCs, 1));
                                                span.SetSpan(new ForegroundColorSpan(spanColor), 0,1, SpanTypes.ExclusiveExclusive);                                                
                                                messageBox.Append(span);
                                                colorSpanText.Append(span);
                                            }
                                            ctCs++;
                                        });
                                    }
                                    else
                                    {
                                        RunOnUiThread(() =>
                                        {
                                            messageBox.Append("\r\n");
                                            span = new SpannableStringBuilder("\r\n");
                                            colorSpanText.Append(span);
                                        });
                                        nextOutputStr[nextOutputIndex[0]] = null;
                                        nextOutputIndex[0] = 0;
                                        break;
                                    }

                                    Thread.Sleep(10);
                                }
                                //}
                                //catch
                                //{
                                //    if (tryBool == false)
                                //    {
                                //        messageBox.Append(sendStr.Substring(ctCs, sendStr.Length - ctCs - 1) + "\r\n");
                                //    }
                                //}
                            }
                        }
                        nextOutputIndex[0] = 0;
                        nextOutputStr = new System.Collections.ArrayList();
                    }
                   else if(nextOutputStrOldChat.Count != 0)
                    {
                        for (nextOutputIndex[1] = 0; nextOutputIndex[1] < nextOutputStrOldChat.Count-1; nextOutputIndex[1]++)
                        {
                            if (nextOutputStrOldChat[nextOutputIndex[1]]!= null)
                            {
                                string sendStr = nextOutputStrOldChat[nextOutputIndex[1]].ToString();//sendStrObj.ToString();
                                int ctCs = sendStr.Length-1;

                                int colorNum = -1;
                                //if (sendStr.Split("]")[0]=="[旧信息")
                                //{
                                //    if (sendStr.Split("]")[3].IndexOf("私信至") != -1)
                                //    {
                                //        colorNum = 1;
                                //    }else if(sendStr.Split("]")[2].IndexOf("[") != -1)
                                //    {
                                //        colorNum = 2;
                                //    }
                                //}
                                //else 
                                if (sendStr.Split("]")[1] == "[系统信息")
                                {
                                    colorNum = 3;
                                }
                                else if (sendStr.Split("]")[2] == "[发送失败")
                                {
                                    colorNum = 4;
                                }
                                else if (sendStr.Split("]")[2].IndexOf("私信至") != -1)
                                {
                                    colorNum = 5;
                                }
                                else if (sendStr.Split("]")[1] == "[Server")
                                {
                                    colorNum = 6;
                                }
                                else if (sendStr.Split("]")[1].IndexOf("[") != -1)
                                {
                                    colorNum = 7;
                                }
                                SpannableStringBuilder span = null;
                                Color spanColor = Color.White;
                                while (true)
                                {
                                    if (ctCs >= 0)
                                    {
                                        RunOnUiThread(() =>
                                        {
                                            if (ctCs == sendStr.Length - 1)
                                            {
                                                span = new SpannableStringBuilder("\r\n");
                                                //SpannableStringBuilder tempSpan = colorSpanText;
                                                //colorSpanText = span;
                                                colorSpanText.Insert(0, span);
                                                //colorSpanText.Append(tempSpan);
                                                messageBox.Text = null;
                                                messageBox.Append(colorSpanText, -1, 0);

                                                //messageBox.Text = "\r\n" + messageBox.Text;
                                            }
                                            if (ctCs >= 0)
                                            {
                                                string[] splitStr = sendStr.Split("]");
                                                switch (colorNum)
                                                {
                                                    //case 1:
                                                    //    if (ctCs <= splitStr[0].Length)//[旧信息]
                                                    //    {
                                                    //        spanColor = Color.Gray;
                                                    //    }else if (ctCs <= splitStr[1].Length)//时间
                                                    //    {
                                                    //        spanColor = Color.Orange;
                                                    //    }else if (ctCs <= splitStr[2].Length)//用户名
                                                    //    {
                                                    //        spanColor = Color.MediumTurquoise;
                                                    //    }else//文本
                                                    //    {
                                                    //        spanColor = Color.White;
                                                    //    }
                                                    //    break;
                                                    case 3://系统信息样式
                                                        if (ctCs <= splitStr[0].Length)//时间
                                                        {
                                                            spanColor = Color.Orange;
                                                        }
                                                        else if (ctCs <= splitStr[1].Length + splitStr[0].Length + 1)//[系统信息]
                                                        {
                                                            spanColor = Color.Blue;
                                                        }
                                                        else//文本
                                                        {
                                                            spanColor = Color.White;
                                                        }
                                                        break;
                                                    case 4://私信失败样式
                                                        if (ctCs <= splitStr[0].Length)//时间
                                                        {
                                                            spanColor = Color.Orange;
                                                        }
                                                        else if (ctCs <= splitStr[1].Length + splitStr[0].Length + 1)//用户名
                                                        {
                                                            spanColor = Color.MediumTurquoise;
                                                        }
                                                        else if (ctCs <= splitStr[2].Length + splitStr[1].Length + splitStr[0].Length + 2)//[发送失败]
                                                        {
                                                            spanColor = Color.OrangeRed;
                                                        }
                                                        else if (ctCs <= splitStr[3].Length + splitStr[2].Length + splitStr[1].Length + splitStr[0].Length + 3)//[私信至
                                                        {
                                                            spanColor = Color.SkyBlue;
                                                        }
                                                        else//文本
                                                        {
                                                            spanColor = Color.White;
                                                        }
                                                        break;
                                                    case 5://私信至
                                                        if (ctCs <= splitStr[0].Length)//时间
                                                        {
                                                            spanColor = Color.Orange;
                                                        }
                                                        else if (ctCs <= splitStr[1].Length + splitStr[0].Length + 1)//用户名
                                                        {
                                                            spanColor = Color.MediumTurquoise;
                                                        }
                                                        else if (ctCs <= splitStr[2].Length + splitStr[1].Length + splitStr[0].Length + 2)//[私信至
                                                        {
                                                            spanColor = Color.SkyBlue;
                                                        }
                                                        else//文本
                                                        {
                                                            spanColor = Color.White;
                                                        }
                                                        break;
                                                    case 6://服务器发送的信息
                                                        if (ctCs <= splitStr[0].Length)//时间
                                                        {
                                                            spanColor = Color.Orange;
                                                        }
                                                        else if (ctCs <= splitStr[1].Length + splitStr[0].Length + 1)//server
                                                        {
                                                            spanColor = Color.CornflowerBlue;
                                                        }
                                                        else//文本
                                                        {
                                                            spanColor = Color.White;
                                                        }
                                                        break;
                                                    case 7://用户正常信息
                                                        if (ctCs <= splitStr[0].Length)//时间
                                                        {
                                                            spanColor = Color.Orange;
                                                        }
                                                        else if (ctCs <= splitStr[1].Length + splitStr[0].Length + 1)//用户名
                                                        {
                                                            spanColor = Color.MediumTurquoise;
                                                        }
                                                        else//文本
                                                        {
                                                            spanColor = Color.White;
                                                        }
                                                        break;
                                                    default:
                                                        spanColor = Color.Silver;
                                                        break;
                                                }

                                                //messageBox.Text=sendStr.Substring(ctCs,1)+messageBox.Text;

                                                span = new SpannableStringBuilder(sendStr.Substring(ctCs, 1));                                          
                                                span.SetSpan(new ForegroundColorSpan(spanColor), 0, 1, SpanTypes.ExclusiveExclusive);
                                                // tempStr.Text = messageBox.Text;
                                                //SpannableStringBuilder tempSpan = colorSpanText;
                                                //colorSpanText = span;
                                                colorSpanText.Insert(0, span);
                                                //colorSpanText.Append(tempSpan);
                                                messageBox.Text = null;
                                                messageBox.Append(colorSpanText, -1,0);                                             
                                            }
                                            ctCs--;
                                        });
                                    }
                                    else
                                    {                                       
                                        nextOutputStrOldChat[nextOutputIndex[1]] = null;
                                        nextOutputIndex[1] = 0;
                                        break;
                                    }

                                    Thread.Sleep(10);
                                }
                            }
                        }
                        nextOutputIndex[1] = 0;
                        nextOutputStrOldChat=new System.Collections.ArrayList();
                    }
                }
                else
                {
                    Thread.Sleep(3);
                }
            }
        }




		/// <summary>
		/// 加密网络传输的字符串
		/// </summary>
		/// <param name="enc">是否为加密</param>
		/// <param name="input">输入字符串</param>
		/// <returns>加密或解密后的文本</returns>
		public static string EncryptStr(bool enc, string input)
		{
            try { 
			string[] output;
			_2ndGenerationEncryptionMethodsFromHgnim emfHg2g = new _2ndGenerationEncryptionMethodsFromHgnim();
            //加密密钥为"$#*M&C*@%"
            if (enc)
            {
                output = emfHg2g.EncryptInoutPut(new string[] { input }, "$#*M&C*@%", "text");
            }
            else
            {
                output = emfHg2g.DecryptInoutPut(new string[]{input}, "$#*M&C*@%", "text");
			}
			return output[1];
		}
			catch { return "!!----*error*----!!"; }
		}
		//正在尝试的新功能，UI文本逐字输出
		//string[,] uiNextOutputStr = new string[1,5];//需要更改UI文字时将需要更改的UI文字赋值到此数组
		///// <summary>
		///// uiNextOutputStr控件编号分配
		///// 
		///// connectionServerRelativeLayout布局中可变文字的控件：
		///// 0,0 ipEditText
		///// 0,1 portEditText
		///// 0,2 accountUserNameEditText
		///// 0,3 accountPasswordEditText
		///// 0,4 stateTextView
		///// 
		///// </summary>
		///// 
		//string startUiTimeChat =null;//用此变量启动UI逐字输出
		//bool stopUiTimeChat = false;//是否终止逐字输出
		//void UiTimeChatOutput ()//UI中的文字逐字输出
		//        {
		//            //RelativeLayout connectionServerRelativeLayout = FindViewById<RelativeLayout>(Resource.Id.connectionServerRelativeLayout);
		//            //RelativeLayout chatRelativeLayout = FindViewById<RelativeLayout>(Resource.Id.chatRelativeLayout);
		//            //RelativeLayout activateAccountRelativeLayout = FindViewById<RelativeLayout>(Resource.Id.activateAccountRelativeLayout);
		//            //RelativeLayout chatRecordsRelativeLayout = FindViewById<RelativeLayout>(Resource.Id.chatRecordsRelativeLayout);        
		//            //TextView stateTextView = FindViewById<TextView>(Resource.Id.stateTextView);
		//            //TextView messageBox = FindViewById<TextView>(Resource.Id.messageBox);
		//            //Button sendChatButton = FindViewById<Button>(Resource.Id.sendChatButton);
		//            //TextView sendChatBox = FindViewById<TextView>(Resource.Id.sendChatBox);    
		//            //EditText activateAccountRelativeLayout_ActivateAccountIdEditText = FindViewById<EditText>(Resource.Id.activateAccountRelativeLayout_ActivateAccountIdEditText);
		//            //EditText activateAccountRelativeLayout_ActivateAccountUserNameEditText = FindViewById<EditText>(Resource.Id.activateAccountRelativeLayout_ActivateAccountUserNameEditText);
		//            //EditText activateAccountRelativeLayout_ActivateAccountPasswordEditText = FindViewById<EditText>(Resource.Id.activateAccountRelativeLayout_ActivateAccountPasswordEditText);
		//            //EditText activateAccountRelativeLayout_ActivateAccountPassword2EditText = FindViewById<EditText>(Resource.Id.activateAccountRelativeLayout_ActivateAccountPassword2EditText);
		//            //Button activateAccountRelativeLayout_ActivateAccountButton = FindViewById<Button>(Resource.Id.activateAccountRelativeLayout_ActivateAccountButton);
		//            //TextView activateAccountRelativeLayout_stateTextView = FindViewById<TextView>(Resource.Id.activateAccountRelativeLayout_stateTextView);
		//            //Button clientNumButton = FindViewById<Button>(Resource.Id.clientNumButton);
		//            //Button chatRecords_exitButton = FindViewById<Button>(Resource.Id.chatRecords_exitButton);
		//            //Button chatRecords_clearButton = FindViewById<Button>(Resource.Id.chatRecords_clearButton);
		//            //TextView chatRecords_messageBox = FindViewById<TextView>(Resource.Id.chatRecords_messageBox);
		//            while (true)
		//            {
		//                if (startUiTimeChat != null)
		//                {
		//                    //string str =this.GetString(Resource.String.yonghuming);//Resource.String.yonghuming
		//                    int[] ctCs=null;
		//                    string[] Str=null;
		//                    int uiNextOutputStr_1;//表示布局编号
		//                    int uiNextOutputStr_2; //表示该布局内可变文本的数量
		//                    switch (startUiTimeChat)
		//                    {
		//                        case "connectionServerRelativeLayout":
		//                            uiNextOutputStr_1 = 0;//编号0
		//                            uiNextOutputStr_2 = 5;//有5个
		//repeat:;
		//                            if (stopUiTimeChat)
		//                            {
		//                                for (int i = 0; i < uiNextOutputStr_2; i++)
		//                                {
		//                                    uiNextOutputStr[uiNextOutputStr_1, i] = null;
		//                                }
		//                                goto stop; 
		//                            }
		//                            for (int i = 0; i < uiNextOutputStr_2; i++)
		//                            {
		//                                if (uiNextOutputStr[uiNextOutputStr_1, i] == null)
		//                                {
		//                                    Thread.Sleep(1);
		//                                    goto repeat;
		//                                }
		//                            }

		//                            Str = new string[]
		//                            {
		//                                GetString(Resource.String.fuwuqiip),
		//                                uiNextOutputStr[0,0],
		//                                GetString(Resource.String.duankou),
		//                                uiNextOutputStr[0,1],
		//                                GetString(Resource.String.lianjiefuwuqi),
		//                                GetString(Resource.String.shiyongzhanghudenglu),
		//                                GetString(Resource.String.yonghuming),
		//                                uiNextOutputStr[0,2],
		//                                GetString(Resource.String.mima),
		//                                uiNextOutputStr[0,3],
		//                                uiNextOutputStr[0,4],
		//                                GetString(Resource.String.guanyu),
		//                                GetString(Resource.String.xiaoxijilu),
		//                                GetString(Resource.String.zhucezhanghu)
		//                            };
		//                            ctCs = new int[Str.Length];

		//                            for(int i=0;i < uiNextOutputStr_2;i++)//使用完变量后清除，以便下次使用
		//                            {
		//                                uiNextOutputStr[uiNextOutputStr_1, i] = null;
		//                            }

		//                            break;
		//                    }


		//                    TextView textView1 = FindViewById<TextView>(Resource.Id.textView1);
		//                    EditText ipEditText = FindViewById<EditText>(Resource.Id.ipEditText);
		//                    TextView textView2 = FindViewById<TextView>(Resource.Id.textView2);
		//                    EditText portEditText = FindViewById<EditText>(Resource.Id.portEditText);
		//                    Button connectionButton = FindViewById<Button>(Resource.Id.connectionButton);
		//                    AndroidX.AppCompat.Widget.AppCompatCheckBox accountCheck = FindViewById<AndroidX.AppCompat.Widget.AppCompatCheckBox>(Resource.Id.accountCheck);
		//                    TextView textView3 = FindViewById<TextView>(Resource.Id.textView3);
		//                    EditText accountUserNameEditText = FindViewById<EditText>(Resource.Id.accountUserNameEditText);
		//                    TextView textView4 = FindViewById<TextView>(Resource.Id.textView4);
		//                    EditText accountPasswordEditText = FindViewById<EditText>(Resource.Id.accountPasswordEditText);
		//                    TextView guanyu = FindViewById<TextView>(Resource.Id.guanyu);
		//                    TextView chatRecordsTextView = FindViewById<TextView>(Resource.Id.chatRecordsTextView);
		//                    TextView activateAccountTextView = FindViewById<TextView>(Resource.Id.activateAccountTextView);

		//                    while (true)
		//                    {
		//                        bool allTrue = true;

		//                        switch (startUiTimeChat)
		//                        {
		//                            case "connectionServerRelativeLayout":
		//                                RunOnUiThread(() =>
		//                                {
		//                                    if (ctCs[0] != -1) { textView1.Text += Str[0].Substring(ctCs[0], 1); }
		//                                    if (ctCs[1] != -1) { ipEditText.Text += Str[1].Substring(ctCs[1], 1); }
		//                                    if (ctCs[2] != -1) { textView2.Text += Str[2].Substring(ctCs[2], 1); }
		//                                    if (ctCs[3] != -1) { portEditText.Text += Str[3].Substring(ctCs[3], 1); }
		//                                    if (ctCs[4] != -1) { connectionButton.Text += Str[4].Substring(ctCs[4], 1); }
		//                                    if (ctCs[5] != -1) { accountCheck.Text += Str[5].Substring(ctCs[5], 1); }
		//                                    if (ctCs[6] != -1) { textView3.Text += Str[6].Substring(ctCs[6], 1); }
		//                                    if (ctCs[7] != -1) { accountUserNameEditText.Text += Str[7].Substring(ctCs[7], 1); }
		//                                    if (ctCs[8] != -1) { textView4.Text += Str[8].Substring(ctCs[8], 1); }
		//                                    if (ctCs[9] != -1) { accountPasswordEditText.Text += Str[9].Substring(ctCs[9], 1); }
		//                                    if (ctCs[10] != -1) { guanyu.Text += Str[10].Substring(ctCs[10], 1); }
		//                                    if (ctCs[11] != -1) { chatRecordsTextView.Text += Str[11].Substring(ctCs[11], 1); }
		//                                    if (ctCs[12] != -1) { activateAccountTextView.Text += Str[12].Substring(ctCs[12], 1); }                                   
		//                                });
		//                                break;
		//                        }
		//                        for (int i = 0; i < ctCs.Length; i++)
		//                        {
		//                            ctCs[i]++;
		//                            if (ctCs[i] >= Str.Length)
		//                            {
		//                                ctCs[i] = -1;
		//                            }
		//                            if (ctCs[i] != -1)
		//                            {
		//                                allTrue = false;
		//                            }
		//                        }
		//                        if (allTrue == true)
		//                        {                           
		//                            break;
		//                        }
		//                        if (stopUiTimeChat) { goto stop; }
		//                    }
		//stop:;
		//                    startUiTimeChat = null;
		//                    stopUiTimeChat = false;
		//                }
		//                else
		//                {
		//                    Thread.Sleep(4);
		//                }
		//            }
		//        }

	}
}