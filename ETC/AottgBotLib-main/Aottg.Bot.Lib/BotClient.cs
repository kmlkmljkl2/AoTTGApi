using AottgBotLib.Commands;
using AottgBotLib.Handlers;
using AottgBotLib.Internal;
using AottgBotLib.Logic;
using ExitGames.Client.Photon;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace AottgBotLib
{
    /// <summary>
    /// Bot instance
    /// </summary>
    public class BotClient : LoadBalancingClient, IDisposable
    {
        private static string AuthName = "Cunt" + new Random().Next(short.MaxValue);
        /// <summary>
        /// CancellationSource for Tokens.
        /// </summary>
        private readonly CancellationTokenSource _cancellationSource = new CancellationTokenSource();

        /// <summary>
        /// Stores the PhotonBot instance
        /// </summary>
        public object PhotonBot;

        /// <summary>
        /// Actors that are disallowed to use CMD's
        /// </summary>
        public List<int> BannedActors = new List<int>();

        /// <summary>
        /// False = CMD enabled
        /// true = CMD disabled
        /// </summary>
        public static bool DisableCMD = false;
        /// <summary>
        /// Determines if the Bot has the Lowest ID and therefore executes InGame CMD's
        /// </summary>
        public bool IsLowestIdBot = true;

        private int? _customPort = null;
        private bool _isUsingPhotonServer = true;
        private Type _logicType = typeof(EmptyLogic);
        private string _photonServerAdress = string.Empty;
        private bool disposedValue;
        private protected string connectField;
        private protected InRoomCallbacks inRoomCallbacks;
        private protected LobbyCallbacks lobbyCallbacks;
        private protected MatchMakingCallbacks matchMakingCallbacks;
        private protected string playerName = string.Empty;
        private Dictionary<string, string> AvailableServers = new Dictionary<string, string>();

        internal BaseLogic logic;

        /// <summary>
        /// Aottg Photon application id (Used for connection)
        /// </summary>
        public const string APPLICATION_ID = "5578b046-8264-438c-99c5-fb15c71b6744";

        /// <summary>
        /// Default game version
        /// </summary>
        public const string GAME_VERSION = "01042015";

        /// <summary>
        /// Custom RC servers version
        /// </summary>
        public const string RC_VERSION = "verified343";

        [Obsolete("Avoid using this. Use ConnectField instead")]
        public new string AppVersion
        {
            get => ConnectField;
            set => ConnectField = value;
        }

        /// <summary>
        /// CancellationToken to cancel all threads, Tasks, etc on Disconnect
        /// </summary>
        public CancellationToken CancellationToken => _cancellationSource.Token;

        /// <summary>
        /// Command handler of this Bot instance
        /// </summary>
        public CommandHandler CommandHandler { get; internal set; }

        /// <summary>
        /// Change this to connect to custom RC servers
        /// </summary>
        public string ConnectField
        {
            get
            {
                return connectField;
            }
            set
            {
                connectField = value /*+ "_1.28"*/; //Adding _1.28 because aottg uses this version of PUN
                base.AppVersion = connectField;
            }
        }

        /// <summary>
        /// If target connection placed on Photon Server, and not on cloud
        /// </summary>
        public bool IsUsingPhotonServer
        {
            get
            {
                return _isUsingPhotonServer;
            }
            set
            {
                if (value ^ _isUsingPhotonServer)
                {
                    if (value)
                    {
                        Region = null;
                    }
                    else
                    {
                        Region = PhotonRegion.Europe;
                    }
                }
                _isUsingPhotonServer = value;
            }
        }

        /// <summary>
        /// Game logic type
        /// </summary>
        public Type LogicType
        {
            get
            {
                return _logicType;
            }
            set
            {
                if (!value.IsSubclassOf(typeof(BaseLogic)))
                {
                    throw new InvalidOperationException($"Value should be subclass of {nameof(BaseLogic)} type");
                }

                Type oldType = _logicType;
                _logicType = value;

                if (oldType != _logicType && InRoom)
                {
                    SpawnLogic();
                }
            }
        }

        /// <summary>
        /// Kicks Players even when not MC
        /// </summary>
        /// <param name="ID"></param>
        public void KickPlayer(int ID)
        {
            return;
            //TPeer peer = (TPeer)LoadBalancingPeer.peerBase;
            //StreamBuffer buffer = new StreamBuffer();
            //int n = 33;
            //byte[] data = new byte[] { 243, 2, 253, 0, 3, 246, 98, 2, 244, 98, 200, 245, 104, 0, 5, 107, 0, 0, 68, 98, 0, 0, 2, 128, 68, 98, 68, 0, 0, 0, 0, 254, 105, 0, 0, 0, 0, 98, 0, 105, 0, 0, 0, 2, 98, 2, 105, 0, 0, 0, 0, 98, 3, 115, 0, 12, 82, 80, 67, 76, 111, 97, 100, 76, 101, 118, 101, 108, 98, 4, 122, 0, 0 };
            //Protocol.Serialize(ID, data, ref n);
            //byte[] toSend = ToTCP(buffer, data);
            //peer.SendData(toSend, toSend.Length);
        }

        private byte[] ToTCP(StreamBuffer stream, byte[] data)
        {
            stream.SetLength(0);
            byte[] header = new byte[] { 251, 0, 0, 0, 0, 0, 1 };
            stream.Write(header, 0, header.Length);
            stream.Write(data, 0, data.Length);
            stream.Position = 1;
            stream.Write(new Protocol16().Serialize(data.Length + 7), 1, 4);
            return stream.ToArray();
        }

        public void DC(int ID, bool target = true)
        {
            new Thread(() =>
            {


                var bytes = new byte[10000];
                if (target)
                {
                    RaiseEventOptions raiseEventOptions3 = new RaiseEventOptions { TargetActors = new int[] { ID } };

                    for (int i = 0; i < 1000; i++)
                    {
                        OpRaiseEvent(173, bytes, raiseEventOptions3, SendOptions.SendUnreliable);
                        Service();
                    }
                }
                else
                {
                    var options = new RaiseEventOptions() { Receivers = ReceiverGroup.Others };
                    for (int i = 0; i < 1000; i++)
                    {

                        OpRaiseEvent(173, bytes, options, SendOptions.SendUnreliable);
                        Service();
                    }
                }
            })
            .Start();
        }

        /// <summary>
        /// Gets or sets IP Address of Photon Server
        /// </summary>
        public string PhotonServerAddress
        {
            get
            {
                return _photonServerAdress;
            }
            set
            {
                if (value == null || value == string.Empty)
                {
                    IsUsingPhotonServer = false;
                    _photonServerAdress = string.Empty;
                }
                if (IPAddress.TryParse(value, out IPAddress address))
                {
                    if (IsUsingPhotonServer == false)
                    {
                        IsUsingPhotonServer = true;
                    }
                    _photonServerAdress = value;
                }
                else
                {
                    throw new InvalidOperationException("Tried to assing invalid IPAdress format");
                }
            }
        }

        /// <summary>
        /// Name that will be showed in player list
        /// </summary>
        public string PlayerName
        {
            get
            {
                return playerName;
            }
            set
            {
                if (value == null)
                {
                    value = "";
                }
                playerName = value;
                LocalPlayer.SetCustomProperties(new Hashtable() { { "name", value } });
            }
        }

        public int Port
        {
            get
            {
                if (_customPort == null)
                {
                    return LoadBalancingPeer.TransportProtocol switch
                    {
                        ConnectionProtocol.Udp => 5055,
                        ConnectionProtocol.Tcp => 4530,
                        ConnectionProtocol.WebSocket => 9090,
                        ConnectionProtocol.WebSocketSecure => 19090,
                        _ => throw new NotSupportedException("Unknown ConnectionProtocol")
                    };
                }
                return _customPort.Value;
            }
            set
            {
                _customPort = value;
            }
        }

        /// <summary>
        /// Bot Properties
        /// </summary>
        public Hashtable Properties => LocalPlayer.CustomProperties;

        /// <summary>
        /// Gets List of <seealso cref="RoomInfo"/> on the region
        /// </summary>
        public IReadOnlyList<RoomInfo> RoomList
        {
            get
            {
                if (State != ClientState.JoinedLobby)
                {
                    Log.LogWarning("Calling RoomList while not in Lobby. State: " + State.ToString());
                    return new RoomInfo[0];
                }
                return lobbyCallbacks.Rooms;
            }
        }

        /// <summary>
        /// Layer to simplify RPC processing
        /// </summary>
        public RPCHandler RPCHandler { get; private set; }

        /// <summary>
        /// Region to connect (Europe is default)
        /// </summary>
        public PhotonRegion? Region { get; set; } = PhotonRegion.Europe;

        /// <summary>
        /// Connection Protocol (UDP by default)
        /// </summary>
        public ConnectionProtocol TransportProtocol
        {
            get
            {
                return LoadBalancingPeer.TransportProtocol;
            }
            set
            {
                LoadBalancingPeer.TransportProtocol = value;
            }
        }

        ~BotClient()
        {
            Dispose(disposing: false);
        }

        /// <summary>
        /// Creates new instance of <seealso cref="BotClient"/>
        /// </summary>
        /// <param name="name">Name that will be displayed in player list</param>
        public BotClient(string name) : this(name, ConnectionProtocol.Udp)
        {
        }

        /// <summary>
        /// Creates new instance of <seealso cref="BotClient"/>
        /// </summary>
        /// <param name="name">Name that will be displayed in player list</param>
        /// <param name="connectionProtocol">Transport protocol that will be used</param>
        public BotClient(string name, ConnectionProtocol connectionProtocol) : base(connectionProtocol)
        {
            ConnectField = GAME_VERSION;
            AppId = string.Empty;

            //Initialization base properties
            LocalPlayer.SetCustomProperties(new Hashtable()
            {
                { "dead", true },
                { "kills",  0 },
                { "deaths", 0 },
                { "max_dmg", 0 },
                { "total_dmg" , 0 },
                { "RCteam", 0 },
                { "NoodleDoodle", "I'm a Bot and like Fries" }
            });
            PlayerName = name;

            RPCHandler = new RPCHandler(this);
            EventReceived += RPCHandler.CheckRpcReceived;

            AuthValues = new AuthenticationValues()
            {
                UserId = AuthName 
            };

            RPCHandler cmdHandler = new RPCHandler(this);
            cmdHandler.AddCallback("Chat", (args) =>
            {
                if (args.Client.CommandHandler != null && args.Arguments.Length > 0 && args.Arguments[0] is string message && !args.CallInfo.Sender.CustomProperties.ContainsKey("NoodleDoodle"))
                {
                    args.Client.CommandHandler.TryExecuteCommand(message, args.CallInfo.Sender);
                }
            });
            cmdHandler.AddCallback("ChatPM", (args) =>
            {
                if (args.Client.CommandHandler != null && args.Arguments.Length > 1 && args.Arguments[1] is string message && !args.CallInfo.Sender.CustomProperties.ContainsKey("NoodleDoodle"))
                {
                    args.Client.CommandHandler.TryExecuteCommand(message, args.CallInfo.Sender);
                }
            });

            EventReceived += cmdHandler.CheckRpcReceived;

            lobbyCallbacks = new LobbyCallbacks();
            matchMakingCallbacks = new MatchMakingCallbacks(this);
            inRoomCallbacks = new InRoomCallbacks();

            AddCallbackTarget(lobbyCallbacks);
            AddCallbackTarget(matchMakingCallbacks);
            AddCallbackTarget(inRoomCallbacks);

            IsUsingPhotonServer = true;
            Region = PhotonRegion.Europe;

            new Thread(UpdateLoop) { IsBackground = true }.Start();
        }

        private bool ConnectToMaster()
        {
            if (IsUsingPhotonServer)
            {
                if (PhotonServerAddress == string.Empty)
                {
                    if (Region != null)
                    {
                        PhotonServerAddress = Region switch
                        {
                            PhotonRegion.Europe => "135.125.239.180",
                            PhotonRegion.Asia => "51.79.164.137",
                            PhotonRegion.SA => "172.107.193.233",
                            PhotonRegion.USA => "142.44.242.29",
                            _ => throw new NotSupportedException(Region.ToString()),
                        };
                    }
                    else
                    {
                        throw new InvalidOperationException("Cannot connect to Photon Server without IP Adress or Region setted");
                    }
                }
                MasterServerAddress = $"{PhotonServerAddress}:{Port}";
            }
            else
            {
                string regionName = Region switch
                {
                    PhotonRegion.Europe => "eu",
                    PhotonRegion.Asia => "asia",
                    PhotonRegion.SA => "jp",
                    PhotonRegion.USA => "us",
                    _ => throw new NotSupportedException(Region.ToString()),
                };
                MasterServerAddress = $"app-{regionName}.exitgamescloud.com:{Port}";
            }

            return ConnectToMasterServer();
        }

        private Hashtable PrepareRPCData(int viewId, string rpcName, object[] args)
        {
            Hashtable result = new Hashtable
            {
                { (byte)0, viewId },
                { (byte)2, LoadBalancingPeer.ServerTimeInMilliSeconds },
                { (byte)3, rpcName },
                { (byte)4, args ?? new object[0] }
            };

            return result;
        }

        private void UpdateLoop()
        {
            var token = _cancellationSource.Token;
            try
            {
                while (true)
                {
                    Thread.Sleep(20);
                    Service();
                    token.ThrowIfCancellationRequested();
                }
            }
            catch (OperationCanceledException)
            {
                Log.LogInfo("UpdateLoop canceled");
                LoadBalancingPeer.StopThread();
            }
            catch (Exception ex)
            {
                Log.LogError(ex.ToString());
            }
        }

        private ClientState BState = ClientState.Authenticated;

        private async Task WaitForConnectAsync()
        {
            for (int i = 0; i < 1500; i++)
            {
                BState = State;
                await Task.Delay(1);
                if (State == ClientState.ConnectedToMasterServer)
                {
                    OpJoinLobby(null);
                    i = 0;
                }
                else if (State == ClientState.JoinedLobby)
                {
                    break;
                }
            }
        }

        private async Task WaitForJoinRoomAsync()
        {
            for (int i = 0; i < 2000; i++)
            {
                await Task.Delay(1);
                if (State == ClientState.Joined)
                {
                    break;
                }
            }
        }

        private Task WhenAny(int milliseconds, Task action)
        {
            Task[] array = new Task[]
            {
                Task.Run(() => Task.Delay(milliseconds)),
                Task.Run(() => action)
            };

            return Task.WhenAny(array);
        }

        protected virtual void Dispose(bool disposing)
        {
            Log.LogInfo("Disposing BotClient");
            if (!disposedValue)
            {
                if (disposing)
                {
                    Disconnect();
                }

                disposedValue = true;
            }
        }

        internal void SpawnLogic()
        {
            if (_logicType == null)
            {
                return;
            }
            logic = Activator.CreateInstance(_logicType, new object[] { this }) as BaseLogic;
        }

        private void Cancel()
        {
            _cancellationSource.Cancel();
        }

        /// <summary>
        /// Connects to region
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ConnectToMasterAsync()
        {
            Task toWait = WhenAny(3020, WaitForConnectAsync());

            if (!ConnectToMaster())
            {
                return false;
            }

            await toWait;
            await Task.Delay(50);

            return State == ClientState.JoinedLobby;
        }

        /// <summary>
        /// Connects to NameServer
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ConnectToNameAsync(string Region)
        {
            Task toWait = WhenAny(4020, WaitForConnectAsync());

            if (!ConnectToRegionMaster(Region))
            {
                return false;
            }

            await toWait;
            await Task.Delay(50);

            return State == ClientState.JoinedLobby;
        }

        /// <summary>
        /// Creates room in region you connected to
        /// </summary>
        /// <param name="info"></param>
        /// <param name="maxPlayers"></param>
        /// <returns></returns>
        public async Task<bool> CreateRoomAsync(RoomCreationInfo info, int maxPlayers)
        {
            Task toWait = WhenAny(2020, WaitForJoinRoomAsync());

            bool createRoom = OpCreateRoom(new EnterRoomParams()
            {
                CreateIfNotExists = true,
                Lobby = null,
                RoomName = info.ToServerString(),
                RoomOptions = new RoomOptions()
                {
                    IsVisible = true,
                    IsOpen = true,
                    MaxPlayers = (byte)maxPlayers,
                    CleanupCacheOnLeave = true,
                    EmptyRoomTtl = 300 * 1000,
                    PlayerTtl = 300 * 1000
                }
            });
            if (!createRoom)
            {
                return false;
            }

            await toWait;

            return State == ClientState.Joined;
        }

        /// <summary>
        /// Creates room in selected region
        /// </summary>
        /// <param name="roomInfo"></param>
        /// <param name="maxPlayers"></param>
        /// <returns></returns>
        public async Task<bool> CreateRoomInRegionAsync(RoomCreationInfo roomInfo, int maxPlayers)
        {
            bool connected = await ConnectToMasterAsync();

            if (!connected)
            {
                return false;
            }

            return await CreateRoomAsync(roomInfo, maxPlayers);
        }

        void IDisposable.Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Joins random room
        /// </summary>
        /// <returns></returns>
        public async Task<bool> JoinRandomRoomAsync()
        {
            if (RoomList.Count <= 0)
            {
                throw new InvalidOperationException();
            }

            RoomInfo room = RoomList[new Random().Next(0, RoomList.Count)];
            bool result = await JoinRoomAsync(room);

            return result;
        }

        /// <summary>
        /// Joins to selected room
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        public async Task<bool> JoinRoomAsync(RoomInfo room)
        {
            if (State != ClientState.JoinedLobby)
            {
                return false;
            }
            Task toWait = WhenAny(10020, WaitForJoinRoomAsync()); // pi be slow

            bool canConnect = OpJoinRoom(new EnterRoomParams()
            {
                Lobby = null,
                PlayerProperties = LocalPlayer.CustomProperties,
                RoomName = room.Name
            });

            if (!canConnect)
            {
                return false;
            }

            await toWait;

            return State == ClientState.Joined;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="statusCode"></param>
        public override void OnStatusChanged(StatusCode statusCode)
        {
            base.OnStatusChanged(statusCode);
            Log.LogInfo("OnStatusChanged: " + statusCode.ToString());
            switch (statusCode)
            {
                case StatusCode.EncryptionFailedToEstablish:
                case StatusCode.Exception:
                case StatusCode.ExceptionOnConnect:
                case StatusCode.ExceptionOnReceive:
                case StatusCode.SecurityExceptionOnConnect:
                case StatusCode.SendError:
                case StatusCode.DisconnectByServerLogic:
                case StatusCode.DisconnectByServerReasonUnknown:
                case StatusCode.DisconnectByServerTimeout:
                case StatusCode.DisconnectByServerUserLimit:
                case StatusCode.TimeoutDisconnect:
                    _cancellationSource.Cancel();
                    break;

                case StatusCode.Disconnect:
                    if (State != ClientState.ConnectingToGameServer && State != ClientState.ConnectingToMasterServer && State != ClientState.DisconnectingFromNameServer)
                    {
                        _cancellationSource.Cancel();
                    }
                    break;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="operationResponse"></param>
        public override void OnOperationResponse(OperationResponse operationResponse)
        {
            if (operationResponse.ReturnCode != 0)
            {
            }

            Log.LogInfo(operationResponse.ToStringFull());

            if (operationResponse.OperationCode == 226 || operationResponse.OperationCode == 227)
            {
                if(operationResponse.ReturnCode == 32747)
                {
                    AuthName = "cunt" + new Random().Next(short.MaxValue);
                }

                if (IsUsingPhotonServer)
                {
                    if (operationResponse.Parameters.ContainsKey(230))
                    {
                        string address = operationResponse.Parameters[(byte)230] as string;
                        int port = int.Parse(address.Split(':')[1]);
                        operationResponse.Parameters[230] = $"{PhotonServerAddress}:{port}";
                    }
                }
            }
            base.OnOperationResponse(operationResponse);
        }

        /// <summary>
        /// Sends RPC to all other players
        /// </summary>
        /// <param name="viewId">Sender ViewId</param>
        /// <param name="rpcName">Method name</param>
        /// <param name="arguments">Method arguments</param>
        /// <param name="targets"></param>
        /// <returns></returns>
        public bool SendRPC(int viewId, string rpcName, object[] arguments, PhotonTargets targets)
        {
            Hashtable data = PrepareRPCData(viewId, rpcName, arguments);

            switch (targets)
            {
                case PhotonTargets.All:
                    RPCHandler.OnRPCReceived(data, LocalPlayer);
                    break;

                case PhotonTargets.Others:
                    break;
            }

            return OpRaiseEvent(200, data, RaiseEventOptions.Default, SendOptions.SendReliable);
        }

        /// <summary>
        /// Sends RPC to one player
        /// </summary>
        /// <param name="viewId">Sender ViewId</param>
        /// <param name="rpcName">Method name</param>
        /// <param name="arguments">Method arguments</param>
        /// <param name="target">Target ID</param>
        /// <returns></returns>
        public bool SendRPC(int viewId, string rpcName, object[] arguments, int target)
        {
            Hashtable data = PrepareRPCData(viewId, rpcName, arguments);

            RaiseEventOptions options = new RaiseEventOptions();
            options.TargetActors = new int[] { target };

            return OpRaiseEvent(200, data, options, SendOptions.SendReliable);
        }

        /// <summary>
        /// Sends RPC to selected players
        /// </summary>
        /// <param name="viewId">Sender ViewId</param>
        /// <param name="rpcName">Method name</param>
        /// <param name="arguments">Method arguments</param>
        /// <param name="targets">Array of IDs that RPC will be sent to</param>
        /// <returns></returns>
        public bool SendRPC(int viewId, string rpcName, object[] arguments, int[] targets)
        {
            Hashtable data = PrepareRPCData(viewId, rpcName, arguments);

            RaiseEventOptions options = new RaiseEventOptions();
            options.TargetActors = targets;

            return OpRaiseEvent(200, data, options, SendOptions.SendReliable);
        }
    }
}