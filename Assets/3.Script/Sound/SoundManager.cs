using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임 전체 효과음 관리. DontDestroyOnLoad 싱글톤.
/// AudioSource 풀로 동시 재생 지원, 3D 거리 감쇠 적용.
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Sound Clips")]
    [SerializeField] private SoundEntry[] sounds = new SoundEntry[]
    {
        new SoundEntry { type = SoundType.Mining },
        new SoundEntry { type = SoundType.Deposit },
        new SoundEntry { type = SoundType.ItemAppear },
        new SoundEntry { type = SoundType.PickupItem },
        new SoundEntry { type = SoundType.Handcuff },
        new SoundEntry { type = SoundType.PayMoney },
        new SoundEntry { type = SoundType.PickupCoin },
        new SoundEntry { type = SoundType.UpgradePay },
    };

    [Header("Pool Settings")]
    [SerializeField] private int poolSize = 10;

    [Header("3D Audio Settings")]
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float maxDistance = 20f;
    [SerializeField] private AudioRolloffMode rolloffMode = AudioRolloffMode.Linear;

    private AudioSource[] _pool;
    private Dictionary<SoundType, SoundEntry> _soundMap;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        BuildPool();
        BuildMap();
    }

    public void PlaySound(SoundType type, Vector3 position)
    {
        if (!_soundMap.TryGetValue(type, out var entry) || entry.clip == null)
        {
            Debug.LogWarning($"[SoundManager] 클립 없음: {type}");
            return;
        }

        var src = GetAvailableSource();
        src.transform.position = position;
        src.clip = entry.clip;
        src.volume = entry.volume;
        src.Play();
    }

    private AudioSource GetAvailableSource()
    {
        foreach (var src in _pool)
            if (!src.isPlaying) return src;

        // 풀이 다 찼을 때 pool[0] 강탈
        _pool[0].Stop();
        return _pool[0];
    }

    private void BuildPool()
    {
        _pool = new AudioSource[poolSize];
        for (int i = 0; i < poolSize; i++)
        {
            var go = new GameObject($"SoundSource_{i}");
            go.transform.SetParent(transform);
            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.spatialBlend = 1f;
            src.rolloffMode = rolloffMode;
            src.minDistance = minDistance;
            src.maxDistance = maxDistance;
            _pool[i] = src;
        }
    }

    private void BuildMap()
    {
        _soundMap = new Dictionary<SoundType, SoundEntry>();
        foreach (var entry in sounds)
            _soundMap[entry.type] = entry;
    }
}

[Serializable]
public class SoundEntry
{
    public SoundType type;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
}
