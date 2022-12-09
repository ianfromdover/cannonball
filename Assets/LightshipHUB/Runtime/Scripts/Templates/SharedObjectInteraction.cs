// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using UnityEngine;
using UnityEngine.Events;

namespace Niantic.LightshipHub.Templates 
{
	public class SharedObjectInteraction : MonoBehaviour 
	{	
		[Serializable]
    public class AREvent : UnityEvent {}
		public AREvent OnTap = new AREvent();
		public AREvent OnDistance = new AREvent();

		internal void AnimateObjectTap() 
		{
			OnTap.Invoke();
		}

		internal void AnimateObjectDistance() 
		{
			OnDistance.Invoke();
		}
	}
}
