// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using Niantic.ARDK.AR;
using Niantic.ARDK.AR.ARSessionEventArgs;
using Niantic.ARDK.AR.HitTest;
using Niantic.ARDK.AR.Networking;
using Niantic.ARDK.AR.Networking.ARNetworkingEventArgs;
using Niantic.ARDK.Extensions;
using Niantic.ARDK.Networking;
using Niantic.ARDK.Networking.HLAPI;
using Niantic.ARDK.Networking.HLAPI.Authority;
using Niantic.ARDK.Networking.HLAPI.Data;
using Niantic.ARDK.Networking.HLAPI.Object;
using Niantic.ARDK.Networking.HLAPI.Object.Unity;
using Niantic.ARDK.Networking.HLAPI.Routing;
using Niantic.ARDK.Networking.MultipeerNetworkingEventArgs;
using Niantic.ARDK.Utilities;
using Niantic.ARDK.Utilities.Input.Legacy;

namespace Niantic.LightshipHub.SampleProjects
{
  /// <summary>
  /// Controls the game logic and creation of objects
  /// </summary>
  public class ARHockeyController:
    MonoBehaviour
  {
    /// Prefabs to be instantiated when the game starts
    [SerializeField]
    private NetworkedUnityObject playingFieldPrefab = null;

    [SerializeField]
    private NetworkedUnityObject ballPrefab = null;

    [SerializeField]
    private NetworkedUnityObject playerPrefab = null;

    /// Reference to the game buttons
    [SerializeField]
    private Button startGame = null;

    [SerializeField]
    private Button joinButton = null;

    [SerializeField]
    private Button createButton = null;

    [SerializeField]
    private Button backButton = null;

    [SerializeField]
    private Text countdown;

    [SerializeField]
    private FeaturePreloadManager preloadManager = null;

    /// Reference to AR Camera, used for hit test
    [SerializeField]
    private Camera _camera = null;

    /// References to game objects after instantiation
    private GameObject _ball;

    private GameObject _player;
    private GameObject _playingField;

    [SerializeField]
    private GameObject _secondPlayerUIState;

    [SerializeField]
    private InputField _sessionID;

    // GameScreens
    public GameObject HomeScreen, CreateGameScreen, JoinGameScreen, PeerStateScreen, GameScreen, GameOverScreen;

    /// The score
    public Text redScore, blueScore;
    public Text PeerStateText;

    public GameObject SyncStateHelper;

    // Session ID to share
    public Text SessionCode;

    public Text WaitingForPlayers;

    // Win - Lose Screen components
    public Text WinLoseText, WaitingText;
    public GameObject RestartButton;

    /// HLAPI Networking objects
    private IHlapiSession _manager;

    private IAuthorityReplicator _auth;
    private MessageStreamReplicator<Vector3> _hitStreamReplicator;

    private INetworkedField<string> _redScoreText, _blueScoreText;
    private int _redScore;
    private int _blueScore;
    private INetworkedField<Vector3> _fieldPosition;
    private INetworkedField<byte> _gameStarted;

    /// Cache your location every frame
    private Vector3 _location;

    /// Some fields to provide a lockout upon hitting the ball, in case the hit message is not
    /// processed in a single frame
    private bool _recentlyHit = false;

    private int _hitLockout = 0;

    private IARNetworking _arNetworking;
    private BallBehaviour _ballBehaviour;

    private bool _isHost;
    private IPeer _self;

    private bool _gameStart;
    private bool _synced;
    private int winnerScore = 3;

    private void Start()
    {
      HomeScreen.SetActive(true);
      CreateGameScreen.SetActive(false);
      JoinGameScreen.SetActive(false);
      PeerStateScreen.SetActive(false);
      GameScreen.SetActive(false);
      GameOverScreen.SetActive(false);
      backButton.gameObject.SetActive(false);
      countdown.gameObject.SetActive(false);

      _secondPlayerUIState.SetActive(false);
      startGame.interactable = false;
      WaitingForPlayers.text = "Waiting for players to join...";

      SyncStateHelper.SetActive(true);
      PeerStateText.gameObject.SetActive(false);

      ARNetworkingFactory.ARNetworkingInitialized += OnAnyARNetworkingSessionInitialized;

      if (preloadManager.AreAllFeaturesDownloaded())
        OnPreloadFinished(true);
      else
        preloadManager.ProgressUpdated += PreloadProgressUpdated;
    }

    private void PreloadProgressUpdated(FeaturePreloadManager.PreloadProgressUpdatedArgs args)
    {
      if (args.PreloadAttemptFinished)
      {
        preloadManager.ProgressUpdated -= PreloadProgressUpdated;
        OnPreloadFinished(args.FailedPreloads.Count == 0);
      }
    }

    public void CreateGame() 
    {
      _sessionID.text = GenerateSessionID();
      SessionCode.text = _sessionID.text;
    }

    public string GenerateSessionID() 
    {
      string builder = "";

      for (int i = 0; i < 6; ++i)
      {
        int r = UnityEngine.Random.Range(0, 26);
        builder += (char)('A' + r);
      }

      return builder;
    }

    private void OnPreloadFinished(bool success)
    {
      if (success)
      {
        joinButton.interactable = true;
        createButton.interactable = true;
      }
      else
        Debug.LogError("Failed to download resources needed to run AR Multiplayer");
    }

    // When all players are ready, create the game. Only the host will have the option to call this
    public void StartGame()
    {
      startGame.interactable = false;
      _gameStarted.Value = Convert.ToByte(true);
    }

    // This function will be call for both players
    private void StartGameForAll()
    {
      PeerStateScreen.SetActive(false);

      _blueScore = _redScore = 0;

      var players = FindObjectsOfType<PlayerAvatarBehaviour>();
      foreach (var player in players)
      {
        if (player.gameObject != _player) player.SetPlayerColor(_isHost ? "peer" : "host");
      }
      
      _ball = FindObjectOfType<BallBehaviour>().gameObject;
      var _playingFieldBehaviour = FindObjectOfType<PlayingFieldBehaviour>();
      _playingField = _playingFieldBehaviour.gameObject;
      _playingFieldBehaviour.SwitchFieldArrows(_isHost);

      // Set the score text for all players
      redScore.text = "00";
      blueScore.text = "00";
      _playingFieldBehaviour.ShowScores();
      _playingFieldBehaviour.UpdateFieldScore(redScore.text, blueScore.text);
      _playingFieldBehaviour.UpdateFieldScore(redScore.text, blueScore.text);
      
      GameOverScreen.SetActive(false);
      GameScreen.SetActive(true);
      
      countdown.gameObject.SetActive(true);
      SFXController.Instance.PlayStartGameSound();
      StartCoroutine(StartCountdown());
    }

    IEnumerator GameOverForAll()
    {
      yield return new WaitForSeconds(1.5f);

      WinLoseText.text = _redScore > _blueScore ? "HOST" : "GUEST";
      SFXController.Instance.PlayGameOverSound();
      RestartButton.SetActive(_isHost);
      WaitingText.gameObject.SetActive(!_isHost);

      GameOverScreen.SetActive(true);
    }

    IEnumerator StartCountdown() 
    {
      countdown.text = "3";
      yield return new WaitForSeconds(1.0f);
      countdown.text = "2";
      yield return new WaitForSeconds(1.0f);
      countdown.text = "1";
      yield return new WaitForSeconds(1.0f);
      countdown.text = "PLAY";
      yield return new WaitForSeconds(1.0f);
      countdown.gameObject.SetActive(false);
      if (_isHost) _ballBehaviour.GameStart(_isHost);
    }

    // Instantiate game objects
    private void InstantiateObjects(Vector3 position)
    {
      if (_playingField != null && _isHost)
      {
        Debug.Log("Relocating the playing field!");
        _fieldPosition.Value = new Optional<Vector3>(position);
        _player.transform.position = position + new Vector3(0, 0, -1.1f);
        _playingField.transform.position = position;
        _ball.transform.position = position;

        return;
      }

      Debug.Log("Instantiating the playing field!");

      var startingFieldOffset = new Vector3(0, -0.5f, 1.9f);

      // Both players want to spawn an avatar that they are the Authority of
      var startingOffset =
        _isHost ? new Vector3(0, 0, -1.1f) : new Vector3(0, 0, 1.1f);

      _player =
        playerPrefab.NetworkSpawn
        (
          _arNetworking.Networking,
          position + startingFieldOffset + startingOffset,
          Quaternion.identity,
          Role.Authority
        )
        .gameObject;

        _player.GetComponent<PlayerAvatarBehaviour>().SetPlayerColor( _isHost ? "host" : "peer" );

      // Only the host should spawn the remaining objects
      if (!_isHost)
        return;

      // Instantiate the playing field at floor level
      _playingField =
        playingFieldPrefab.NetworkSpawn
        (
          _arNetworking.Networking,
          position + startingFieldOffset,
          Quaternion.identity
        )
        .gameObject;

      _playingField.GetComponent<PlayingFieldBehaviour>().SwitchFieldArrows(_isHost);

      // Spawn the ball and set up references
      _ballBehaviour = ballPrefab.NetworkSpawn
        (
          _arNetworking.Networking,
          position + startingFieldOffset,
          Quaternion.identity
        )
        .DefaultBehaviour as BallBehaviour;
      _ball = _ballBehaviour.gameObject;

      _ballBehaviour.Controller = this;
    }

    // Reset the ball when a goal is scored, increase score for player that scored
    // Only the host should call this method
    internal void GoalScored(string color)
    {
      // color param is the color of the goal that the ball went into
      // we score points by getting the ball in our opponent's goal
      if (color == "red")
      {
        Debug.Log
        (
          "Point scored for team blue. " +
          "Setting score via HLAPI. Only host will receive this log entry."
        );

        _blueScore += 1;
        _blueScoreText.Value = string.Format(_blueScore.ToString("00"));
      }
      else
      {
        Debug.Log
        (
          "Point scored for team red. " +
          "Setting score via HLAPI. Only host will receive this log entry."
        );

        _redScore += 1;
        _redScoreText.Value = string.Format(_redScore.ToString("00"));
      }

      if (_blueScore == winnerScore || _redScore == winnerScore) 
      {
        _ballBehaviour.GameOver();
        _gameStarted.Value = Convert.ToByte(false);
      }
    }

    // Every frame, detect if you have hit the ball
    // If so, either bounce the ball (if host) or tell host to bounce the ball
    private void Update()
    {
      if (_manager != null)
        _manager.SendQueuedData();

      if (_synced && !_gameStart && _isHost)
      {
        if (PlatformAgnosticInput.touchCount <= 0 || EventSystem.current.currentSelectedGameObject != null)
          return;

        var touch = PlatformAgnosticInput.GetTouch(0);

        if (touch.phase == TouchPhase.Began)
        {
          var distance =
            Vector2.Distance
            (
              touch.position,
              new Vector2(startGame.transform.position.x, startGame.transform.position.y)
            );

          if (distance <= 80)
            return;

          FindFieldLocation(touch);
        }
      }

      if (!_gameStart)
        return;

      if (_recentlyHit)
      {
        _hitLockout += 1;

        if (_hitLockout >= 15)
        {
          _recentlyHit = false;
          _hitLockout = 0;
        }
      }

      var distance2 = Vector3.Distance(_player.transform.position, _ball.transform.position);
      if (distance2 > .12 || _recentlyHit)
        return;

      var bounceDirection = _ball.transform.position - _player.transform.position;
      bounceDirection = Vector3.Normalize(bounceDirection);
      _recentlyHit = true;

      if (_isHost)
        _ballBehaviour.Hit(bounceDirection);
      else
        _hitStreamReplicator.SendMessage(bounceDirection, _auth.PeerOfRole(Role.Authority));
    }

    private void FindFieldLocation(Touch touch)
    {
      var currentFrame = _arNetworking.ARSession.CurrentFrame;

      if (currentFrame == null)
        return;

      var results =
        currentFrame.HitTest
        (
          _camera.pixelWidth,
          _camera.pixelHeight,
          touch.position,
          ARHitTestResultType.EstimatedHorizontalPlane
        );

      if (results.Count <= 0)
      {
        Debug.Log("Unable to place the field at the chosen location. Can't find a valid surface");

        return;
      }

      // Get the closest result
      var result = results[0];

      var hitPosition = result.WorldTransform.ToPosition();

      InstantiateObjects(hitPosition);
    }

    // Every updated frame, get our location from the frame data and move the local player's avatar
    private void OnFrameUpdated(FrameUpdatedArgs args)
    {
      _location = MatrixUtils.PositionFromMatrix(args.Frame.Camera.Transform);

      if (_player == null || _playingField == null)
        return;

      var playerPos = _player.transform.position;
      var multiplier = 2.0f;
# if UNITY_EDITOR
      multiplier = 0.7f;
#endif
      playerPos.x = _location.x * multiplier;

      var distance = Math.Abs(playerPos.x - _playingField.transform.position.x);

      if (distance <= 0.7)
        _player.transform.position = playerPos;
      
    }

    private void OnPeerStateReceived(PeerStateReceivedArgs args)
    {
      if (_self.Identifier != args.Peer.Identifier)
      {
        if (args.State == PeerState.Stable)
        {
          _synced = true;

          if (_isHost)
          {
            WaitingForPlayers.text = "Ready to start!";
            _secondPlayerUIState.SetActive(true);
            startGame.interactable = true;
            InstantiateObjects(_location);
          }
          else
          {
            InstantiateObjects(_arNetworking.LatestPeerPoses[args.Peer].ToPosition());
          }
        }
        return;
      }
      else
      {
        if (args.State == PeerState.Stable)
        {
          if (_isHost)
          {
            PeerStateScreen.SetActive(false);
            CreateGameScreen.SetActive(true);
          }
          else 
          {
            SyncStateHelper.SetActive(false);
            PeerStateText.text = "Waiting for host to start the game...";
            PeerStateText.gameObject.SetActive(true);
          }
          return;
        }
      }

      Debug.Log("We reached state " + args.State.ToString());
    }

    private void OnDidConnect(ConnectedArgs connectedArgs)
    {
      _isHost = connectedArgs.IsHost;
      _self = connectedArgs.Self;

      _manager = new HlapiSession(19244);

      var group = _manager.CreateAndRegisterGroup(new NetworkId(4321));
      _auth = new GreedyAuthorityReplicator("pongHLAPIAuth", group);

      _auth.TryClaimRole(_isHost ? Role.Authority : Role.Observer, () => {}, () => {});

      var authToObserverDescriptor =
        _auth.AuthorityToObserverDescriptor(TransportType.ReliableUnordered);

      _fieldPosition =
        new NetworkedField<Vector3>("fieldReplicator", authToObserverDescriptor, group);

      _fieldPosition.ValueChangedIfReceiver += OnFieldPositionDidChange;

      _redScoreText = new NetworkedField<string>("redScoreText", authToObserverDescriptor, group);
      _redScoreText.ValueChanged += OnRedScoreDidChange;

      _blueScoreText = new NetworkedField<string>("blueScoreText", authToObserverDescriptor, group);
      _blueScoreText.ValueChanged += OnBlueScoreDidChange;

      _gameStarted = new NetworkedField<byte>("gameStarted", authToObserverDescriptor, group);

      _gameStarted.ValueChanged +=
        value =>
        {
          _gameStart = Convert.ToBoolean(value.Value.Value);

          if (_gameStart)
          {
            StartGameForAll();
          }
          else 
          {
            StartCoroutine(GameOverForAll());
          }
        };
#pragma warning disable 0618
      _hitStreamReplicator =
        new MessageStreamReplicator<Vector3>
        (
          "hitMessageStream",
          _arNetworking.Networking.AnyToAnyDescriptor(TransportType.ReliableOrdered),
          group
        );
#pragma warning restore 0618
      _hitStreamReplicator.MessageReceived +=
        (args) =>
        {
          Debug.Log("Ball was hit");

          if (_auth.LocalRole != Role.Authority)
            return;

          _ballBehaviour.Hit(args.Message);
        };
    }

    private void OnFieldPositionDidChange(NetworkedFieldValueChangedArgs<Vector3> args)
    {
      var value = args.Value;
      if (!value.HasValue)
        return;

      var offsetPos = value.Value + new Vector3(0, 0, 1.1f);
      _player.transform.position = offsetPos;
    }

    private void OnRedScoreDidChange(NetworkedFieldValueChangedArgs<string> args)
    {
      if (!_isHost) _redScore++;
      redScore.text = args.Value.GetOrDefault();
      if (_playingField != null) _playingField.GetComponent<PlayingFieldBehaviour>().UpdateFieldScore(redScore.text, blueScore.text);
      SFXController.Instance.PlayGoalSound();
    }

    private void OnBlueScoreDidChange(NetworkedFieldValueChangedArgs<string> args)
    {
      if (!_isHost) _blueScore++;
      blueScore.text = args.Value.GetOrDefault();
      if (_playingField != null) _playingField.GetComponent<PlayingFieldBehaviour>().UpdateFieldScore(redScore.text, blueScore.text);
      SFXController.Instance.PlayGoalSound();
    }

    private void OnAnyARNetworkingSessionInitialized(AnyARNetworkingInitializedArgs args)
    {
      _arNetworking = args.ARNetworking;
      _arNetworking.PeerStateReceived += OnPeerStateReceived;

      _arNetworking.ARSession.FrameUpdated += OnFrameUpdated;
      _arNetworking.Networking.Connected += OnDidConnect;
    }

    private void OnDestroy()
    {
      ARNetworkingFactory.ARNetworkingInitialized -= OnAnyARNetworkingSessionInitialized;

      if (_arNetworking != null)
      {
        _arNetworking.PeerStateReceived -= OnPeerStateReceived;
        _arNetworking.ARSession.FrameUpdated -= OnFrameUpdated;
        _arNetworking.Networking.Connected -= OnDidConnect;
      }
    }
  }
}
