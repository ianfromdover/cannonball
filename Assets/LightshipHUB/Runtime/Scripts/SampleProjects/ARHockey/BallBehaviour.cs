// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Niantic.ARDK.Networking;
using Niantic.ARDK.Networking.HLAPI.Data;
using Niantic.ARDK.Networking.HLAPI.Object;
using Niantic.ARDK.Networking.HLAPI.Object.Unity;

namespace Niantic.LightshipHub.SampleProjects
{
  /// <summary>
  /// Class that handles the ball's behaviour
  /// Only the host can affect the ball's properties, all other players must listen
  /// </summary>
  [RequireComponent(typeof(AuthBehaviour))]
  public class BallBehaviour: NetworkedBehaviour
  {
    internal ARHockeyController Controller = null;

    private Vector3 _pos;

    // Left and right boundaries of the field, in meters
    private float _lrBound = 0.72f;

    // Forward and backwards boundaries of the field, in meters
    private float _fbBound = 1.25f;

    // Initial velocity, in meters per second
    private float _initialVelocity = 0.6f;
    private Vector3 _velocity;

    // Cache the floor level, so the ball is reset properly
    private Vector3 _initialPosition;

    // Flags for whether the game has started and if the local player is the host
    private bool _gameStart;
    private bool _isHost;

    private List<float> randomInitialVelocity = new List<float>() { -0.5f, 0.5f, -0.8f, 0.8f };

    //Ball light
    private Light _ballLight;
    private Color _orange = new Color(0.9254902f, 0.4881605f, 0);
    private Color _blue = new Color(0, 0.74738f, 0.9245283f);

    //Ball SFX
    private AudioSource _ballAudioSource;
    [SerializeField]
    private AudioClip _playerHit, _wallBounce;


    // Store the start location of the ball
    private void Start()
    {
      _initialPosition = transform.position;
      _ballLight = GetComponentInChildren<Light>();
      _ballAudioSource = GetComponent<AudioSource>();
    }

    // Set up the initial conditions
    internal void GameStart(bool isHost)
    {
      _isHost = isHost;
      _gameStart = true;
      _initialPosition = transform.position;

      if (!_isHost)
        return;

      int rand1 = UnityEngine.Random.Range(0, randomInitialVelocity.Count);
      int rand2 = UnityEngine.Random.Range(0, randomInitialVelocity.Count);
      _velocity = new Vector3(randomInitialVelocity[rand1], 0 , randomInitialVelocity[rand2]);
    }

    internal void GameOver()
    {
      _gameStart = false;
      _velocity = new Vector3(0, 0, 0);
    }

    // Signal that the ball has been hit, with a unit vector representing the new direction
    internal void Hit(Vector3 direction)
    {
      if (!_gameStart || !_isHost)
        return;

      _velocity = direction * _initialVelocity;
      _initialVelocity *= 1.1f;
    }

    // Perform movement, send position to non-host player
    private void Update()
    {
      if (!_gameStart || !_isHost)
        return;

      _pos = gameObject.transform.position;

      _pos.x += _velocity.x * Time.deltaTime;
      _pos.z += _velocity.z * Time.deltaTime;

      transform.position = _pos;

      if (_pos.x > _initialPosition.x + _lrBound)
        _velocity.x = -_initialVelocity;
      else if (_pos.x < _initialPosition.x - _lrBound)
        _velocity.x = _initialVelocity;

      if (_pos.z > _initialPosition.z + _fbBound)
        _velocity.z = -_initialVelocity;
      else if (_pos.z < _initialPosition.z - _fbBound)
        _velocity.z = _initialVelocity;
    }

    // Signal to host that a goal has been scored
    private void OnTriggerEnter(Collider other)
    {
      if (other.gameObject.name == "OrangePlayer")
      {
        _ballLight.color = _orange;
        _ballAudioSource.PlayOneShot(_playerHit, 0.6f);
      }
      else if (other.gameObject.name == "BluePlayer")
      {
        _ballLight.color = _blue;
        _ballAudioSource.PlayOneShot(_playerHit, 0.6f);
      }
      else if (other.gameObject.name == "Walls")
        _ballAudioSource.PlayOneShot(_wallBounce, 1f);
      else 
      {
        if (!_gameStart || !_isHost) return;

        _velocity = new Vector3(0, 0, 0);
        StartCoroutine(ResetBall());
        gameObject.transform.position = _initialPosition;

        switch (other.gameObject.tag)
        {
          case "RedGoal":
            Controller.GoalScored("red");
            break;

          case "BlueGoal":
            Controller.GoalScored("blue");
            break;
        }
      }
    }
  

    IEnumerator ResetBall() 
    {
      yield return new WaitForSeconds(1.5f);

      _initialVelocity = 0.6f;
      int rand1 = UnityEngine.Random.Range(0, randomInitialVelocity.Count);
      int rand2 = UnityEngine.Random.Range(0, randomInitialVelocity.Count);
      _velocity = new Vector3(randomInitialVelocity[rand1], 0 , randomInitialVelocity[rand2]);
    }

    protected override void SetupSession(out Action initializer, out int order)
    {
      initializer = () =>
      {
        var auth = Owner.Auth;
        var descriptor = auth.AuthorityToObserverDescriptor(TransportType.UnreliableUnordered);

        new UnreliableBroadcastTransformPacker
        (
          "netTransform",
          transform,
          descriptor,
          TransformPiece.Position,
          Owner.Group
        );
      };

      order = 0;
    }
  }
}
