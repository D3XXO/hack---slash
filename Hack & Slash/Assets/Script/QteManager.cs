using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

public class QteManager : MonoBehaviour
{
    public static QteManager instance;

    [Header("QTE Settings")]
    [SerializeField] private float qteDurationPerKey;
    [SerializeField] private KeyCode qteInitiateKey;
    [SerializeField] private float playerInitiateRange;
    [SerializeField] private float qteShakeDuration;
    [SerializeField] private float qteShakeMagnitude;

    [Header("Slow Motion Settings")]
    [SerializeField] private float slowMotionScale;

    [Header("UI Settings")]
    [SerializeField] Image qteKeyImage;
    [SerializeField] float uiPadding;
    [SerializeField] Sprite wKeySprite;
    [SerializeField] Sprite aKeySprite;
    [SerializeField] Sprite sKeySprite;
    [SerializeField] Sprite dKeySprite;

    private TopDownPlayer player;
    private PlayerAttack playerAttack;
    private CameraController cameraController;
    private Miniboss currentMiniboss;
    private List<Miniboss> availableQteTargets = new List<Miniboss>();
    private bool isQteActive = false;
    private List<KeyCode> qteSequence;
    private int currentQteStage = 0;
    private readonly KeyCode[] qteKeys = { KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D };

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        player = FindObjectOfType<TopDownPlayer>();
        playerAttack = FindObjectOfType<PlayerAttack>();

        cameraController = FindObjectOfType<CameraController>();

        if(qteKeyImage != null)
        {
            qteKeyImage.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (isQteActive || availableQteTargets.Count == 0) return;

        if (Input.GetKeyDown(qteInitiateKey))
        {
            Miniboss closestTarget = GetClosestQteTarget();
            if (closestTarget != null)
            {
                StartQte(closestTarget);
            }
        }
    }

    private Miniboss GetClosestQteTarget()
    {
        Miniboss closest = null;
        float minDistance = float.MaxValue;

        foreach (var target in availableQteTargets)
        {
            float distance = Vector2.Distance(player.transform.position, target.transform.position);
            if (distance < minDistance && distance <= playerInitiateRange)
            {
                minDistance = distance;
                closest = target;
            }
        }
        return closest;
    }

    public void RegisterQteTarget(Miniboss miniboss)
    {
        if (!availableQteTargets.Contains(miniboss))
        {
            availableQteTargets.Add(miniboss);
        }
    }

    public void UnregisterQteTarget(Miniboss miniboss)
    {
        if (availableQteTargets.Contains(miniboss))
        {
            availableQteTargets.Remove(miniboss);
        }
    }

    public void StartQte(Miniboss miniboss)
    {
        if (isQteActive) return;

        isQteActive = true;
        currentMiniboss = miniboss;
        currentMiniboss.InitiateQteSequence();
        UnregisterQteTarget(currentMiniboss);

        player.SetControlEnabled(false);
        playerAttack.SetAttackEnabled(false);
        Time.timeScale = slowMotionScale;

        GenerateQteSequence();
        StartNextQteStage();
    }

    private void GenerateQteSequence()
    {
        qteSequence = qteKeys.OrderBy(key => Random.value).ToList();
        currentQteStage = 0;
    }
    
    private IEnumerator QteStageCoroutine(KeyCode correctKey)
    {
        yield return null;

        float timer = 0f;
        bool inputReceived = false;

        while (timer < qteDurationPerKey && !inputReceived)
        {
            if (Input.anyKeyDown)
            {
                inputReceived = true;
                if (Input.GetKeyDown(correctKey))
                {
                    HandleStageSuccess();
                }
                else
                {
                    HandleQteFailure();
                }
            }
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        if (!inputReceived)
        {
            HandleQteFailure();
        }
    }
    
    private void StartNextQteStage()
    {
        KeyCode keyForThisStage = qteSequence[currentQteStage];
        ShowKeyPromptUI(keyForThisStage);
        StartCoroutine(QteStageCoroutine(keyForThisStage));
    }


    private void HandleStageSuccess()
    {
        if (cameraController != null)
        {
            cameraController.TriggerShake(qteShakeDuration, qteShakeMagnitude);
        }

        currentQteStage++;
        bool isFinalHit = (currentQteStage >= qteSequence.Count);

        player.PerformQteAttack(currentMiniboss.transform);

        if (isFinalHit)
        {
            StartCoroutine(DelayedMinibossDeath(0.1f));
        }
        else
        {
            StartNextQteStage();
        }
    }

    private IEnumerator DelayedMinibossDeath(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (currentMiniboss != null)
        {
            currentMiniboss.Die();
        }
        EndQte();
    }

    private void HandleQteFailure()
    {
        currentMiniboss.HandleQtePartialFailure(currentQteStage);
        EndQte();
    }



    private void EndQte()
    {
        Time.timeScale = 1f;
        if (qteKeyImage != null) qteKeyImage.gameObject.SetActive(false);
        isQteActive = false;
        player.SetControlEnabled(true);
        playerAttack.SetAttackEnabled(true);
    }
    
    private void ShowKeyPromptUI(KeyCode key)
    {
        if (qteKeyImage == null) return;

        RectTransform rect = qteKeyImage.GetComponent<RectTransform>();

        switch (key)
        {
            case KeyCode.W:
                qteKeyImage.sprite = wKeySprite;
                rect.anchorMin = new Vector2(0.5f, 1f);
                rect.anchorMax = new Vector2(0.5f, 1f);
                rect.pivot = new Vector2(0.5f, 1f);
                rect.anchoredPosition = new Vector2(0, -uiPadding);
                break;

            case KeyCode.A:
                qteKeyImage.sprite = aKeySprite;
                rect.anchorMin = new Vector2(0f, 0.5f);
                rect.anchorMax = new Vector2(0f, 0.5f);
                rect.pivot = new Vector2(0f, 0.5f);
                rect.anchoredPosition = new Vector2(uiPadding, 0);
                break;

            case KeyCode.S:
                qteKeyImage.sprite = sKeySprite;
                rect.anchorMin = new Vector2(0.5f, 0f);
                rect.anchorMax = new Vector2(0.5f, 0f);
                rect.pivot = new Vector2(0.5f, 0f);
                rect.anchoredPosition = new Vector2(0, uiPadding);
                break;

            case KeyCode.D:
                qteKeyImage.sprite = dKeySprite;
                rect.anchorMin = new Vector2(1f, 0.5f);
                rect.anchorMax = new Vector2(1f, 0.5f);
                rect.pivot = new Vector2(1f, 0.5f);
                rect.anchoredPosition = new Vector2(-uiPadding, 0);
                break;
        }

        qteKeyImage.gameObject.SetActive(true);
    }
}