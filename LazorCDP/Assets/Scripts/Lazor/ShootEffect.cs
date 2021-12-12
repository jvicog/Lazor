using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class ShootEffect : MonoBehaviour {
    private Light light;
    private AudioSource audioSource;
    [SerializeField] GameObject particles;
    [SerializeField] private CinemachineFreeLook cinemachineFreeLook;

    private void Awake() {
        light = GetComponent<Light>();
        audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable() {
        audioSource.Play();
        StartCoroutine(PlayEffect());
    }

    IEnumerator PlayEffect() {
        Noise();
        yield return new WaitForSeconds(0.1f);
        particles.SetActive(false);
        Denoise();
        light.enabled = false;
        yield return new WaitForSeconds(0.45f);
        light.enabled = true;
        particles.SetActive(true);
        gameObject.SetActive(false);
    }

    void Noise() {
        if (cinemachineFreeLook == null) return;
        cinemachineFreeLook.GetRig(0).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 0.5f;
        cinemachineFreeLook.GetRig(1).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 0.5f;
        cinemachineFreeLook.GetRig(2).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 0.5f;
        cinemachineFreeLook.GetRig(0).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = 0.5f;
        cinemachineFreeLook.GetRig(1).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = 0.5f;
        cinemachineFreeLook.GetRig(2).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = 0.5f;
    }

    void Denoise() {
        if (cinemachineFreeLook == null) return;
        cinemachineFreeLook.GetRig(0).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 0;
        cinemachineFreeLook.GetRig(1).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 0;
        cinemachineFreeLook.GetRig(2).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 0;
        cinemachineFreeLook.GetRig(0).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = 0;
        cinemachineFreeLook.GetRig(1).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = 0;
        cinemachineFreeLook.GetRig(2).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = 0;
    }
}
