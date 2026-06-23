#define DEBUG
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

public class Session_ME : ISession
{
	public class Sender
	{
		public List<Message> sendingMessage;

		public Sender()
		{
			sendingMessage = new List<Message>();
		}

		public void AddMessage(Message message)
		{
			sendingMessage.Add(message);
		}

		public void run()
		{
			while (connected)
			{
				try
				{
					if (getKeyComplete)
					{
						while (sendingMessage.Count > 0)
						{
							Message i = sendingMessage[0];
							doSendMessage(i);
							sendingMessage.RemoveAt(0);
						}
					}
					try
					{
						Thread.Sleep(5);
					}
					catch (Exception ex)
					{
						Cout.LogError(ex.ToString());
					}
				}
				catch (Exception)
				{
					Res.outz("error send message! ");
				}
			}
		}
	}

	private class MessageCollector
	{
		public void run()
		{
			try
			{
				while (connected)
				{
					Message message = readMessage();
					if (message == null)
					{
						break;
					}
					try
					{
						if (message.command == -27)
						{
							getKey(message);
						}
						else
						{
							onRecieveMsg(message);
						}
					}
					catch (Exception)
					{
						Cout.println("LOI NHAN  MESS THU 1");
					}
					try
					{
						Thread.Sleep(5);
					}
					catch (Exception)
					{
						Cout.println("LOI NHAN  MESS THU 2");
					}
				}
			}
			catch (Exception ex3)
			{
				Debug.WriteLine("error read message!");
				Debug.WriteLine(ex3.Message.ToString());
			}
			if (!connected)
			{
				return;
			}
			if (messageHandler != null)
			{
				if (currentTimeMillis() - timeConnected > 500)
				{
					messageHandler.onDisconnected(isMainSession);
				}
				else
				{
					messageHandler.onConnectionFail(isMainSession);
				}
			}
			if (sc != null)
			{
				cleanNetwork();
			}
		}

		private void getKey(Message message)
		{
			try
			{
				sbyte b = message.reader().readSByte();
				key = new sbyte[b];
				for (int i = 0; i < b; i++)
				{
					key[i] = message.reader().readSByte();
				}
				for (int j = 0; j < key.Length - 1; j++)
				{
					key[j + 1] ^= key[j];
				}
				getKeyComplete = true;
				GameMidlet.IP2 = message.reader().readUTF();
				GameMidlet.PORT2 = message.reader().readInt();
				GameMidlet.isConnect2 = ((message.reader().readByte() != 0) ? true : false);
				if (isMainSession && GameMidlet.isConnect2)
				{
					GameCanvas.connect2();
				}
			}
			catch (Exception)
			{
			}
		}

		private Message readMessage2(sbyte cmd)
		{
			int num = readKey(dis.ReadSByte()) + 128;
			int num2 = readKey(dis.ReadSByte()) + 128;
			int num3 = readKey(dis.ReadSByte()) + 128;
			int num4 = (num3 * 256 + num2) * 256 + num;
			sbyte[] array = new sbyte[num4];
			int num5 = 0;
			byte[] src = dis.ReadBytes(num4);
			Buffer.BlockCopy(src, 0, array, 0, num4);
			recvByteCount += 5 + num4;
			int num6 = recvByteCount + sendByteCount;
			strRecvByteCount = num6 / 1024 + "." + num6 % 1024 / 102 + "Kb";
			if (getKeyComplete)
			{
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = readKey(array[i]);
				}
			}
			return new Message(cmd, array);
		}

		private Message readMessage()
		{
			try
			{
				sbyte b = dis.ReadSByte();
				if (getKeyComplete)
				{
					b = readKey(b);
				}
				if (b == -32 || b == -66 || b == 11 || b == -67 || b == -74 || b == -87 || b == 66 || b == 12)
				{
					return readMessage2(b);
				}
				int num;
				if (getKeyComplete)
				{
					sbyte b2 = dis.ReadSByte();
					sbyte b3 = dis.ReadSByte();
					num = ((readKey(b2) & 0xFF) << 8) | (readKey(b3) & 0xFF);
				}
				else
				{
					sbyte b4 = dis.ReadSByte();
					sbyte b5 = dis.ReadSByte();
					num = ((b4 & 0xFF) << 8) | (b5 & 0xFF); // BUG FIX: đổi 0xFF00 → (b4 & 0xFF) << 8
				}
				sbyte[] array = new sbyte[num];
				int num2 = 0;
				int num3 = 0;
				byte[] src = dis.ReadBytes(num);
				Buffer.BlockCopy(src, 0, array, 0, num);
				recvByteCount += 5 + num;
				int num4 = recvByteCount + sendByteCount;
				strRecvByteCount = num4 / 1024 + "." + num4 % 1024 / 102 + "Kb";
				if (getKeyComplete)
				{
					for (int i = 0; i < array.Length; i++)
					{
						array[i] = readKey(array[i]);
					}
				}
				return new Message(b, array);
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.StackTrace.ToString());
			}
			return null;
		}
	}

	protected static Session_ME instance = new Session_ME();

	private static Stream dataStream;

	private static BinaryReader dis;

	private static BinaryWriter dos;

	public static IMessageHandler messageHandler;

	public static bool isMainSession = true;

	private static TcpClient sc;

	public static bool connected;

	public static bool connecting;

	private static Sender sender = new Sender();

	public static Thread initThread;

	public static Thread collectorThread;

	public static Thread sendThread;

	public static int sendByteCount;

	public static int recvByteCount;

	private static bool getKeyComplete;

	public static sbyte[] key = null;

	private static int curR;  // BUG FIX: đổi từ sbyte → int, tránh tràn số khi key.Length > 127

	private static int curW;  // BUG FIX: đổi từ sbyte → int

	private static int timeConnected;

	private long lastTimeConn;

	public static string strRecvByteCount = string.Empty;

	public static bool isCancel;

	private string host;

	private int port;

	private long timeWaitConnect;

	public static int count;

	public static MyVector recieveMsg = new MyVector();

	// ─── Proxy config (per-account, set via Panel) ───────────────────────────
	/// <summary>0 = HTTP, 1 = SOCKS5. Rỗng = không dùng proxy.</summary>
	public static bool HasProxyConfigured = false;
	public static string ProxyHost = "";
	public static int ProxyPort = 0;
	/// <summary>0 = HTTP, 1 = SOCKS5</summary>
	public static int ProxyType = 0;
	public static string ProxyUsername = "";
	public static string ProxyPassword = "";

	public Session_ME()
	{
		Debug.WriteLine("init Session_ME");
	}

	public void clearSendingMessage()
	{
		sender.sendingMessage.Clear();
	}

	public static Session_ME gI()
	{
		if (instance == null)
		{
			instance = new Session_ME();
		}
		return instance;
	}

	public bool isConnected()
	{
		return connected && sc != null && dis != null;
	}

	public void setHandler(IMessageHandler msgHandler)
	{
		messageHandler = msgHandler;
	}

	public void connect(string host, int port)
	{
		if (connected || connecting)
		{
			Debug.WriteLine(">>>return connect ...!" + connected + "  ::  " + connecting);
			return;
		}
		if (mSystem.currentTimeMillis() < timeWaitConnect)
		{
			Debug.WriteLine(">>>>chặn việc nó kết nối 2 3 lần liên tục");
			return;
		}
		timeWaitConnect = mSystem.currentTimeMillis() + 50;
		if (isMainSession)
		{
			ServerListScreen.testConnect = -1;
		}
		this.host = host;
		this.port = port;
		getKeyComplete = false;
		close();
		Debug.WriteLine("connecting...!");
		Debug.WriteLine("host: " + host);
		Debug.WriteLine("port: " + port);
		initThread = new Thread(NetworkInit);
		initThread.Start();
	}

	private void NetworkInit()
	{
		isCancel = false;
		connecting = true;
		Thread.CurrentThread.Priority = ThreadPriority.Highest;
		connected = true;
		try
		{
			doConnect(host, port);
			messageHandler.onConnectOK(isMainSession);
		}
		catch (Exception)
		{
			if (messageHandler != null)
			{
				close();
				messageHandler.onConnectionFail(isMainSession);
			}
		}
	}

	public void doConnect(string host, int port)
	{
		// Nếu có proxy thì kết nối qua proxy, ngược lại kết nối thẳng
		if (HasProxyConfigured)
		{
			if (string.IsNullOrWhiteSpace(ProxyHost) || ProxyPort <= 0)
			{
				throw new Exception("Lỗi proxy hoặc sai định dạng. Dừng kết nối để bảo vệ IP thật.");
			}
			if (ProxyType == 1)
			{
				sc = ConnectViaSocks5Pub(ProxyHost, ProxyPort, host, port, ProxyUsername, ProxyPassword);
				dataStream = sc.GetStream();  // SOCKS5: stream lấy từ TcpClient
			}
			else
			{
				dataStream = ConnectViaHttpProxyPub(ProxyHost, ProxyPort, host, port, ProxyUsername, ProxyPassword, false, out sc);
				// HTTP proxy: dataStream đã là tunnel stream, KHÔNG gọi lại sc.GetStream()
			}
		}
		else
		{
			sc = new TcpClient();
			sc.Connect(host, port);
			dataStream = sc.GetStream();  // Kết nối thẳng: mới lấy stream
		}
		// BUG FIX: đã xóa dòng "dataStream = sc.GetStream();" ở đây
		// vì nó ghi đè stream proxy đã thiết lập đúng ở trên
		dis = new BinaryReader(dataStream, new UTF8Encoding());
		dos = new BinaryWriter(dataStream, new UTF8Encoding());
		sendThread = new Thread(sender.run);
		sendThread.Start();
		MessageCollector @object = new MessageCollector();
		collectorThread = new Thread(@object.run);
		collectorThread.Start();
		timeConnected = currentTimeMillis();
		connecting = false;
		doSendMessage(new Message(-27));
		key = null;
	}

	private static byte[] ReadExact(Stream stream, int count)
	{
		byte[] buffer = new byte[count];
		int totalRead = 0;
		while (totalRead < count)
		{
			int read = stream.Read(buffer, totalRead, count - totalRead);
			if (read == 0) throw new EndOfStreamException("Connection closed while reading from proxy");
			totalRead += read;
		}
		return buffer;
	}

	/// <summary>Kết nối qua HTTP CONNECT proxy tunnel.</summary>
	public static Stream ConnectViaHttpProxyPub(string pHost, int pPort, string targetHost, int targetPort, string pUser, string pPass, bool isHttps, out TcpClient client)
	{
		client = new TcpClient();
		client.Connect(pHost, pPort);
		Stream stream = client.GetStream();
		if (isHttps)
		{
			SslStream sslStream = new SslStream(stream, leaveInnerStreamOpen: false, (object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors errors) => true);
			sslStream.AuthenticateAsClient(pHost);
			stream = sslStream;
		}
		string text = $"CONNECT {targetHost}:{targetPort} HTTP/1.1\r\nHost: {targetHost}:{targetPort}\r\n";
		if (!string.IsNullOrEmpty(pUser) && !string.IsNullOrEmpty(pPass))
		{
			string text2 = Convert.ToBase64String(Encoding.UTF8.GetBytes(pUser + ":" + pPass));
			text = text + "Proxy-Authorization: Basic " + text2 + "\r\n";
		}
		text += "\r\n";
		byte[] bytes = Encoding.UTF8.GetBytes(text);
		stream.Write(bytes, 0, bytes.Length);
		stream.Flush();
		
		List<byte> responseBytes = new List<byte>();
		int b;
		while ((b = stream.ReadByte()) != -1)
		{
			responseBytes.Add((byte)b);
			if (responseBytes.Count >= 4)
			{
				int c = responseBytes.Count;
				if (responseBytes[c - 4] == '\r' && responseBytes[c - 3] == '\n' && responseBytes[c - 2] == '\r' && responseBytes[c - 1] == '\n')
				{
					break;
				}
			}
		}
		
		string @string = Encoding.UTF8.GetString(responseBytes.ToArray());
		if (!@string.StartsWith("HTTP/") || !@string.Contains("200"))
		{
			Debug.WriteLine("Lỗi kết nối proxy HTTP" + (isHttps ? "S" : "") + ": " + @string);
			throw new Exception("Kết nối proxy HTTP" + (isHttps ? "S" : "") + " thất bại: " + @string);
		}
		Debug.WriteLine("Đã thiết lập đường hầm proxy HTTP" + (isHttps ? "S" : "") + ".");
		return stream;
	}

	/// <summary>Kết nối qua SOCKS5 proxy tunnel.</summary>
	public static TcpClient ConnectViaSocks5Pub(string pHost, int pPort, string targetHost, int targetPort, string pUser = null, string pPass = null)
	{
		TcpClient tcpClient = new TcpClient();
		tcpClient.Connect(pHost, pPort);
		try
		{
			NetworkStream stream = tcpClient.GetStream();
			// Greeting: v5, support NO_AUTH (0) and USER_PASS (2)
			stream.Write(new byte[] { 5, 2, 0, 2 }, 0, 4);
			stream.Flush();
			byte[] array2 = ReadExact(stream, 2);
			if (array2[0] != 5)
			{
				throw new Exception("Phản hồi SOCKS5 không hợp lệ");
			}
			if (array2[1] == 2 && !string.IsNullOrEmpty(pUser))
			{
				pPass ??= "";
				byte[] array3 = new byte[3 + pUser.Length + pPass.Length];
				array3[0] = 1;
				array3[1] = (byte)pUser.Length;
				Array.Copy(Encoding.UTF8.GetBytes(pUser), 0, array3, 2, pUser.Length);
				array3[2 + pUser.Length] = (byte)pPass.Length;
				Array.Copy(Encoding.UTF8.GetBytes(pPass), 0, array3, 3 + pUser.Length, pPass.Length);
				stream.Write(array3, 0, array3.Length);
				stream.Flush();
				byte[] array4 = ReadExact(stream, 2);
				if (array4[1] != 0)
				{
					throw new Exception("Xác thực SOCKS5 thất bại");
				}
			}
			else if (array2[1] != 0)
			{
				throw new Exception("Proxy SOCKS5 yêu cầu phương thức xác thực không hỗ trợ hoặc sai cấu hình");
			}
			List<byte> list = new List<byte> { 5, 1, 0 };
			if (System.Net.IPAddress.TryParse(targetHost, out var _))
			{
				list.Add(1);
				byte[] addressBytes = System.Net.IPAddress.Parse(targetHost).GetAddressBytes();
				list.AddRange(addressBytes);
			}
			else
			{
				list.Add(3);
				list.Add((byte)targetHost.Length);
				list.AddRange(Encoding.UTF8.GetBytes(targetHost));
			}
			list.Add((byte)(targetPort >> 8));
			list.Add((byte)(targetPort & 0xFF));
			stream.Write(list.ToArray(), 0, list.Count);
			stream.Flush();
			
			// Read the response header (VER, REP, RSV, ATYP)
			byte[] resHeader = ReadExact(stream, 4);
			if (resHeader[1] != 0)
			{
				throw new Exception("Kết nối SOCKS5 thất bại: " + resHeader[1]);
			}
			
			int atyp = resHeader[3];
			if (atyp == 1) // IPv4
			{
				ReadExact(stream, 4 + 2);
			}
			else if (atyp == 3) // Domain
			{
				byte[] lenBody = ReadExact(stream, 1);
				ReadExact(stream, lenBody[0] + 2);
			}
			else if (atyp == 4) // IPv6
			{
				ReadExact(stream, 16 + 2);
			}
			else
			{
				throw new Exception("Loại địa chỉ SOCKS5 không hỗ trợ: " + atyp);
			}
			return tcpClient;
		}
		catch (Exception ex)
		{
			Debug.WriteLine("Kết nối SOCKS5 thất bại: " + ex.Message);
			tcpClient.Close();
			throw;
		}
	}


	public void sendMessage(Message message)
	{
		count++;
		Res.outz("SEND MSG: " + message.command);
		sender.AddMessage(message);
	}

	private static void doSendMessage(Message m)
	{
		sbyte[] data = m.getData();
		try
		{
			if (getKeyComplete)
			{
				sbyte value = writeKey(m.command);
				dos.Write(value);
			}
			else
			{
				dos.Write(m.command);
			}
			if (data != null)
			{
				int num = data.Length;
				if (getKeyComplete)
				{
					int num2 = writeKey((sbyte)(num >> 8));
					dos.Write((sbyte)num2);
					int num3 = writeKey((sbyte)(num & 0xFF));
					dos.Write((sbyte)num3);
				}
				else
				{
					dos.Write((ushort)num);
				}
				if (getKeyComplete)
				{
					for (int i = 0; i < data.Length; i++)
					{
						sbyte value2 = writeKey(data[i]);
						dos.Write(value2);
					}
				}
				sendByteCount += 5 + data.Length;
			}
			else
			{
				if (getKeyComplete)
				{
					int num4 = 0;
					int num5 = writeKey((sbyte)(num4 >> 8));
					dos.Write((sbyte)num5);
					int num6 = writeKey((sbyte)(num4 & 0xFF));
					dos.Write((sbyte)num6);
				}
				else
				{
					dos.Write((ushort)0);
				}
				sendByteCount += 5;
			}
			dos.Flush();
		}
		catch (Exception ex)
		{
			Debug.WriteLine(ex.StackTrace);
			dos.Flush();
		}
	}

	public static sbyte readKey(sbyte b)
	{
		sbyte[] array = key;
		sbyte result = (sbyte)((array[curR++] & 0xFF) ^ (b & 0xFF));
		if (curR >= key.Length)
		{
			curR %= key.Length;  // BUG FIX: bỏ cast (sbyte), curR giờ là int
		}
		return result;
	}

	public static sbyte writeKey(sbyte b)
	{
		sbyte[] array = key;
		sbyte result = (sbyte)((array[curW++] & 0xFF) ^ (b & 0xFF));
		if (curW >= key.Length)
		{
			curW %= key.Length;  // BUG FIX: bỏ cast (sbyte), curW giờ là int
		}
		return result;
	}

	public static void onRecieveMsg(Message msg)
	{
		if (Thread.CurrentThread.Name == Main.mainThreadName)
		{
			messageHandler.onMessage(msg);
		}
		else
		{
			recieveMsg.addElement(msg);
		}
	}

	public static void update()
	{
		while (recieveMsg.size() > 0)
		{
			Message message = (Message)recieveMsg.elementAt(0);
			if (Controller.isStopReadMessage)
			{
				break;
			}
			if (message == null)
			{
				recieveMsg.removeElementAt(0);
				break;
			}
			messageHandler.onMessage(message);
			recieveMsg.removeElementAt(0);
		}
	}

	public void close()
	{
		cleanNetwork();
	}

	private static void cleanNetwork()
	{
		key = null;
		curR = 0;
		curW = 0;
		Debug.WriteLine(">>>cleanNetwork ...!");
		try
		{
			connected = false;
			connecting = false;
			if (sc != null)
			{
				sc.Close();
				sc = null;
			}
			if (dataStream != null)
			{
				dataStream.Close();
				dataStream = null;
			}
			if (dos != null)
			{
				dos.Close();
				dos = null;
			}
			if (dis != null)
			{
				dis.Close();
				dis = null;
			}
			if (Thread.CurrentThread.Name == Main.mainThreadName)
			{
				if (sendThread != null)
				{
					sendThread.Abort();
				}
				sendThread = null;
				if (initThread != null)
				{
					initThread.Abort();
				}
				initThread = null;
				if (collectorThread != null)
				{
					collectorThread.Abort();
				}
				collectorThread = null;
			}
			else
			{
				sendThread = null;
				initThread = null;
				collectorThread = null;
			}
			if (isMainSession)
			{
				ServerListScreen.testConnect = 0;
			}
			Controller.isGet_CLIENT_INFO = false;
		}
		catch (Exception)
		{
		}
	}

	public static int currentTimeMillis()
	{
		return Environment.TickCount;
	}

	public static byte convertSbyteToByte(sbyte var)
	{
		if (var > 0)
		{
			return (byte)var;
		}
		return (byte)(var + 256);
	}

	public static byte[] convertSbyteToByte(sbyte[] var)
	{
		byte[] array = new byte[var.Length];
		for (int i = 0; i < var.Length; i++)
		{
			if (var[i] > 0)
			{
				array[i] = (byte)var[i];
			}
			else
			{
				array[i] = (byte)(var[i] + 256);
			}
		}
		return array;
	}

	public bool isCompareIPConnect()
	{
		return true;
	}
}