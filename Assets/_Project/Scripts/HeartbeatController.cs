using UnityEngine;

public class HeartbeatController : MonoBehaviour
{
    [Header("Настройки звука дыхания")]
    [Tooltip("Источник звука дыхания. Если не назначен, возьмёт AudioSource с этого объекта.")]
    public AudioSource breathingAudio;
    [Tooltip("Аудиоклип нормального дыхания")]
    public AudioClip normalClip;
    [Tooltip("Аудиоклип замедленного дыхания (при зажатом пробеле)")]
    public AudioClip slowedClip;
    [Tooltip("Нормальная громкость воспроизведения звука дыхания")]
    public float normalVolume = 1f;
    [Tooltip("Норма тональности звука (не изменяется)")]
    public float normalPitch = 1f;

    [Header("Настройки камеры (Camera Bobbing)")]
    [Tooltip("Длительность фазы вдоха (подъём камеры) в секундах при нормальной скорости (1)")]
    public float inhaleDuration = 2f;
    [Tooltip("Длительность фазы выдоха (опускание камеры) в секундах при нормальной скорости (1)")]
    public float exhaleDuration = 2f;
    [Tooltip("Сила смещения камеры от базовой позиции (настраивает амплитуду движения)")]
    public float bobAmount = 0.2f;

    // Базовая позиция камеры и вычисленные крайние точки для боббинга
    private Vector3 initialPos;
    private Vector3 bobLower;
    private Vector3 bobUpper;

    // Тайминг фаз дыхания (для камеры)
    private float timer = 0f;
    private bool isInhaling = true;

    // Флаг для отслеживания состояния пробела (для переключения звука)
    private bool isSpaceState = false;

    void Start()
    {
        if (breathingAudio == null)
            breathingAudio = GetComponent<AudioSource>();

        if (breathingAudio != null)
        {
            breathingAudio.loop = true;
            breathingAudio.volume = normalVolume;
            breathingAudio.pitch = normalPitch; // tone не изменяется
            if (normalClip != null)
            {
                breathingAudio.clip = normalClip;
                breathingAudio.Play();
            }
        }
        initialPos = transform.localPosition;
        bobLower = initialPos - new Vector3(0, bobAmount, 0);
        bobUpper = initialPos + new Vector3(0, bobAmount, 0);
        transform.localPosition = bobLower;
    }

    // При включении скрипта восстанавливаем нормальный звук и запускаем воспроизведение
    void OnEnable()
    {
        if (breathingAudio == null)
            breathingAudio = GetComponent<AudioSource>();

        if (breathingAudio != null && normalClip != null)
        {
            breathingAudio.loop = true;
            breathingAudio.volume = normalVolume;
            breathingAudio.pitch = normalPitch;
            breathingAudio.clip = normalClip;
            breathingAudio.Play();
            isSpaceState = false; // сбрасываем состояние пробела
        }
    }

    // При отключении скрипта останавливаем звук и очищаем AudioSource
    void OnDisable()
    {
        if (breathingAudio != null)
        {
            breathingAudio.Stop();
            breathingAudio.clip = null;
        }
    }

    void Update()
    {
        // Проверка состояния пробела
        bool spacePressed = Input.GetKey(KeyCode.Space);

        // При изменении состояния пробела переключаем аудиоклип
        if (spacePressed && !isSpaceState)
        {
            // Если пробел зажат и сейчас не активен slowedClip, переключаем его
            if (slowedClip != null && breathingAudio != null && breathingAudio.clip != slowedClip)
            {
                breathingAudio.clip = slowedClip;
                breathingAudio.Play();
            }
            isSpaceState = true;
        }
        else if (!spacePressed && isSpaceState)
        {
            // Если пробел отпущен, возвращаем нормальный звук
            if (normalClip != null && breathingAudio != null && breathingAudio.clip != normalClip)
            {
                breathingAudio.clip = normalClip;
                breathingAudio.Play();
            }
            isSpaceState = false;
        }

        // Определяем коэффициент скорости для камеры: при зажатом пробеле скорость становится 0.25,
        // что означает, что длительность фаз увеличивается в 1 / 0.25 = 4 раза.
        float speedFactor = spacePressed ? 0.25f : 1f;
        float currentInhaleDuration = inhaleDuration / speedFactor;
        float currentExhaleDuration = exhaleDuration / speedFactor;

        // Камера-боббинг, имитирующий дыхание
        if (isInhaling)
        {
            timer += Time.deltaTime;
            float fraction = timer / currentInhaleDuration;
            transform.localPosition = Vector3.Lerp(bobLower, bobUpper, Mathf.Clamp01(fraction));

            if (fraction >= 1f)
            {
                isInhaling = false;
                timer = 0f;
            }
        }
        else
        {
            timer += Time.deltaTime;
            float fraction = timer / currentExhaleDuration;
            transform.localPosition = Vector3.Lerp(bobUpper, bobLower, Mathf.Clamp01(fraction));

            if (fraction >= 1f)
            {
                isInhaling = true;
                timer = 0f;
            }
        }
    }
}
