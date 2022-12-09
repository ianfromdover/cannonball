// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Niantic.ARDK.AR;
using Niantic.ARDK.AR.ARSessionEventArgs;
using Niantic.ARDK.AR.Awareness;
using Niantic.ARDK.AR.Mesh;
using Niantic.ARDK.Extensions.Meshing;
using Niantic.ARDK.Extensions;
using Niantic.ARDK.Utilities.Logging;
using Niantic.ARDK.VirtualStudio.AR.Mock;
using Niantic.ARDK.Utilities.Input.Legacy;

namespace Niantic.LightshipHub.Templates 
{
	public class MeshGardenController : MonoBehaviour 
	{
		[HideInInspector]
		public Camera Camera;
		[HideInInspector]
		public GameObject GroundObjectsHolder, WallsObjectsHolder;
		
		[Range(0.0f, 1.0f)]
		public float TapRange = 0.5f;

		[Range(1, 20)]
		public float Density = 5;

		private List<GameObject> groundPrefabs = new List<GameObject>();
		private List<GameObject> wallPrefabs = new List<GameObject>();
		private IARSession _session;


		private void Awake() 
		{
			foreach (Transform child in GroundObjectsHolder.transform) 
			{
				groundPrefabs.Add(child.gameObject);
			}
			foreach (Transform child in WallsObjectsHolder.transform) 
			{
				wallPrefabs.Add(child.gameObject);
			}
			GroundObjectsHolder.SetActive(false);
			WallsObjectsHolder.SetActive(false);
		}

		void Start()
    {
      ARSessionFactory.SessionInitialized += OnSessionInitialized;
    }

    private void OnSessionInitialized(AnyARSessionInitializedArgs args)
    {
      ARSessionFactory.SessionInitialized -= OnSessionInitialized;
      _session = args.Session;
    }

		void Update() 
		{
			if (PlatformAgnosticInput.touchCount <= 0) { return; }

			var touch = PlatformAgnosticInput.GetTouch(0);
			if (touch.phase == TouchPhase.Began) 
			{
				var currentFrame = _session.CurrentFrame;
				if (currentFrame == null) return;

				if (Camera == null) return;

				StartCoroutine(GrowElements(touch.position));
			}
		}

		IEnumerator GrowElements(Vector2 position) 
		{
			float variation = 200 * TapRange;
			for (int i = 0; i < Density; i++) 
				{
					createElements(position + new Vector2(UnityEngine.Random.Range(-variation,variation),UnityEngine.Random.Range(-variation,variation)));
					yield return new WaitForSeconds(.1f);
				}
		}

		void createElements(Vector2 position)
		{
			var worldRay = Camera.ScreenPointToRay(position);
			RaycastHit hit;

			if (Physics.Raycast(worldRay, out hit, 1000f)) 
			{
				if (hit.transform.gameObject.name.Contains("MeshCollider") || 
					hit.transform.gameObject.name.Contains("Interior_"))
				{
					// Closer to 0: vertical plane
					// Closer to 1 or -1: horizontal plane
					bool wall = Math.Abs(hit.normal.y) < 0.4;
					GameObject prefab = wall
						? wallPrefabs[UnityEngine.Random.Range(0, wallPrefabs.Count)]
						: groundPrefabs[UnityEngine.Random.Range(0, groundPrefabs.Count)];

					var cursor = prefab.transform.Find("cursor");
					if (cursor != null) cursor.gameObject.SetActive(false);

					GameObject obj = Instantiate(prefab, this.transform);
					obj.transform.position = hit.point;
					if (obj.GetComponent<ObjectAnimation>()) obj.GetComponent<ObjectAnimation>().Scale();
					Vector3 plane = Vector3.ProjectOnPlane(Vector3.forward + Vector3.right, hit.normal);
					Quaternion rotation = Quaternion.LookRotation(plane, hit.normal);
					obj.transform.rotation = rotation;
					obj.transform.Rotate(0.0f, UnityEngine.Random.Range(0.0f, 360.0f), 0.0f, Space.Self);
				}
			}
		}

	}
}
