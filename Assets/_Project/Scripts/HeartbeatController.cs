using UnityEngine;

public class HeartbeatController : MonoBehaviour
{
    [Header("��������� ����� �������")]
    [Tooltip("�������� ����� �������. ���� �� ��������, ������ AudioSource � ����� �������.")]
    public AudioSource breathingAudio;
    [Tooltip("��������� ����������� �������")]
    public AudioClip normalClip;
    [Tooltip("��������� ������������ ������� (��� ������� �������)")]
    public AudioClip slowedClip;
    [Tooltip("���������� ��������� ��������������� ����� �������")]
    public float normalVolume = 1f;
    [Tooltip("����� ����������� ����� (�� ����������)")]
    public float normalPitch = 1f;

    [Header("��������� ������ (Camera Bobbing)")]
    [Tooltip("������������ ���� ����� (������ ������) � �������� ��� ���������� �������� (1)")]
    public float inhaleDuration = 2f;
    [Tooltip("������������ ���� ������ (��������� ������) � �������� ��� ���������� �������� (1)")]
    public float exhaleDuration = 2f;
    [Tooltip("���� �������� ������ �� ������� ������� (����������� ��������� ��������)")]
    public float bobAmount = 0.2f;

    // ������� ������� ������ � ����������� ������� ����� ��� ��������
    private Vector3 initialPos;
    private Vector3 bobLower;
    private Vector3 bobUpper;

    // ������� ��� ������� (��� ������)
    private float timer = 0f;
    private bool isInhaling = true;

    // ���� ��� ������������ ��������� ������� (��� ������������ �����)
    private bool isSpaceState = false;

    void Start()
    {
        if (breathingAudio == null)
            breathingAudio = GetComponent<AudioSource>();

        if (breathingAudio != null)
        {
            breathingAudio.loop = true;
            breathingAudio.volume = normalVolume;
            breathingAudio.pitch = normalPitch; // tone �� ����������
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

    // ��� ��������� ������� ��������������� ���������� ���� � ��������� ���������������
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
            isSpaceState = false; // ���������� ��������� �������
        }
    }

    // ��� ���������� ������� ������������� ���� � ������� AudioSource
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
        // �������� ��������� �������
        bool spacePressed = Input.GetKey(KeyCode.Space);

        // ��� ��������� ��������� ������� ����������� ���������
        if (spacePressed && !isSpaceState)
        {
            // ���� ������ ����� � ������ �� ������� slowedClip, ����������� ���
            if (slowedClip != null && breathingAudio != null && breathingAudio.clip != slowedClip)
            {
                breathingAudio.clip = slowedClip;
                breathingAudio.Play();
            }
            isSpaceState = true;
        }
        else if (!spacePressed && isSpaceState)
        {
            // ���� ������ �������, ���������� ���������� ����
            if (normalClip != null && breathingAudio != null && breathingAudio.clip != normalClip)
            {
                breathingAudio.clip = normalClip;
                breathingAudio.Play();
            }
            isSpaceState = false;
        }

        // ���������� ����������� �������� ��� ������: ��� ������� ������� �������� ���������� 0.25,
        // ��� ��������, ��� ������������ ��� ������������� � 1 / 0.25 = 4 ����.
        float speedFactor = spacePressed ? 0.25f : 1f;
        float currentInhaleDuration = inhaleDuration / speedFactor;
        float currentExhaleDuration = exhaleDuration / speedFactor;

        // ������-�������, ����������� �������
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
