using System.Collections;
using UnityEngine;

namespace Niantic.LightshipHub.SampleProjects
{
  public class SFXController : MonoBehaviour
  {
    public static SFXController Instance;
    private AudioSource sFX, ambience;
    [SerializeField]
    private AudioClip[] audioClips;
         
    private void Awake()
    {
      if (Instance != null)
      {
        Destroy(gameObject);
        return;
      }
      Instance = this;

      sFX = gameObject.AddComponent<AudioSource>();
      ambience = SetAmbienceSource();
    }
    private AudioSource SetAmbienceSource()
    {
      ambience = gameObject.AddComponent<AudioSource>();
      ambience.clip = audioClips[5];
      ambience.loop = true;
      ambience.volume = 0.9f;
      ambience.spatialBlend = 1;
      ambience.pitch = 3;
      ambience.reverbZoneMix = 1.1f;
      ambience.playOnAwake = false;
      return ambience;
    }

    public void PlayStartGameSound()
    {
      StartCoroutine(PlayCountDownWhistle());
      sFX.clip = audioClips[0];
      PlayOnce();
    }
    private IEnumerator PlayCountDownWhistle()
    {
      yield return new WaitForSeconds(3.0f);
      sFX.clip = audioClips[3];
      sFX.Play();
      ambience.Play();
    }

    public void PlayGoalSound()
    {
      sFX.clip = audioClips[1];
      PlayOnce();
      sFX.PlayOneShot(audioClips[2], 0.3f);
      PlaySingleWhisle();
    }

    private void PlaySingleWhisle()
    {
      sFX.PlayOneShot(audioClips[4], 0.45f);
    }

    public void PlayGameOverSound()
    {
      sFX.clip = audioClips[1];
      PlayOnce();
    }

    private void PlayOnce()
    {
      sFX.PlayOneShot(sFX.clip);
    }
  }
}