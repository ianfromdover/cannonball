// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using UnityEngine;

using Niantic.ARDK.Networking;
using Niantic.ARDK.Networking.HLAPI.Data;
using Niantic.ARDK.Networking.HLAPI.Object.Unity;

namespace Niantic.LightshipHub.SampleProjects
{
  [RequireComponent(typeof(AuthBehaviour))]
  public class PlayerAvatarBehaviour: NetworkedBehaviour
  {
    [SerializeField]
    private GameObject _orangePlayer, _blueplayer;

    void Awake()
    {
      _orangePlayer.SetActive(false);
      _blueplayer.SetActive(false);
    }

    protected override void SetupSession
    (
      out Action initializer,
      out int order
    )
    {
      initializer = () =>
      {
        var auth = GetComponent<AuthBehaviour>();

        new UnreliableBroadcastTransformPacker
        (
          "netTransform",
          transform,
          auth.AuthorityToObserverDescriptor(TransportType.UnreliableUnordered),
          TransformPiece.Position,
          Owner.Group
        );
      };

      order = 0;
    }

    public void SetPlayerColor(string player)
    {
      _orangePlayer.SetActive(player.Equals("host"));
      _blueplayer.SetActive(player.Equals("peer"));
    }
    
  }
}
