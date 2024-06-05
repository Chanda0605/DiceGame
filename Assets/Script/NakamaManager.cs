using UnityEngine;
using Nakama;
using System;
using System.Threading.Tasks;

public class NakamaManager : MonoBehaviour
{
    private const string Scheme = "http";
    private const string Host = "127.0.0.1";
    private const int Port = 7350;
    private const string ServerKey = "defaultkey";

    private IClient _client;
    private ISocket _socket;
    private ISession _session;
    private IMatch _match;

    public static NakamaManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeNakama();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private async void InitializeNakama()
    {
        _client = new Client(ServerKey, Host, Port, false.ToString());

        _socket = _client.NewSocket();

        _session = await _client.AuthenticateCustomAsync(Guid.NewGuid().ToString());
        await _socket.ConnectAsync(_session, true);
        _socket.ReceivedMatchState += OnReceivedMatchState;

        _match = await _socket.CreateMatchAsync();
        Debug.Log("Match created with ID: " + _match.Id);
    }

    public async Task SendMatchState(int opCode, byte[] data)
    {
        if (_match != null)
        {
            await _socket.SendMatchStateAsync(_match.Id, opCode, data);
        }
    }

    private void OnReceivedMatchState(IMatchState matchState)
    {
        DiceGameManager.Instance.OnMatchStateReceived(matchState.State);
    }
}
