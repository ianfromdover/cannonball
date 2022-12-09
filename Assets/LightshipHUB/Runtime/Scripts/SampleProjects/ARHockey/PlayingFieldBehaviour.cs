// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using UnityEngine;

using Niantic.ARDK.Networking;
using Niantic.ARDK.Networking.HLAPI.Data;
using Niantic.ARDK.Networking.HLAPI.Object.Unity;

namespace Niantic.LightshipHub.SampleProjects
{
  [RequireComponent(typeof(AuthBehaviour))]
  public class PlayingFieldBehaviour: NetworkedBehaviour
  {
    [SerializeField]
    private TextMesh _redScore, _blueScore;
    
    [SerializeField]
    private MeshRenderer arrows;
    
    [SerializeField]
    private Material arrowsMat, reversedArrowsMat;


    void Start()
    {
      _redScore.gameObject.SetActive(false);
      _blueScore.gameObject.SetActive(false);
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

    public void UpdateFieldScore(string redScore, string blueScore) 
    {
      _redScore.text = "HOST " + redScore;
      _blueScore.text = "GUEST " + blueScore;
    }

    public void ShowScores() 
    {
      _redScore.gameObject.SetActive(true);
      _blueScore.gameObject.SetActive(true);
    }

    public void SwitchFieldArrows(bool isHost) 
    {
      arrows.material = isHost ? reversedArrowsMat : arrowsMat;
    }
  }
}
