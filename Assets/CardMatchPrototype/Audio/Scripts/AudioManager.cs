using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource bgmAudioSource;

    [Header("Audio clips")]
    [SerializeField] private AudioClip triesToMatchSound;
    [SerializeField] private AudioClip matchSound;
    [SerializeField] private AudioClip mismatchSound;
    [SerializeField] private AudioClip winSound;


    [SerializeField] private CardMatchUI cardMatchUI;

    private void Start()
    {
        cardMatchUI.onMatchMade.AddListener(PlayMatchSound);
        cardMatchUI.onMatchFailed.AddListener(PlayMismatchSound);
        cardMatchUI.onTryingMatch.AddListener(PlayTriesToMatchSound);
        cardMatchUI.onWinEvent.AddListener(WinSound);
    }


    private void PlayMatchSound()
    {
        audioSource.PlayOneShot(matchSound);
    }

    private void PlayMismatchSound()
    {
        audioSource.PlayOneShot(mismatchSound);
    }

    private void PlayTriesToMatchSound()
    {
        audioSource.PlayOneShot(triesToMatchSound);
    }
    private void WinSound()
    {
        audioSource.PlayOneShot(winSound);
    }
}