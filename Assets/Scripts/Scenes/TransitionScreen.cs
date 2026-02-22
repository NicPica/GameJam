using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TransitionScreen : MonoBehaviour
{
    [Header("UI References")]
    public Image blackPanel;
    public Text titleText;
    public Text loreText;
    public Canvas transitionCanvas;

    [Header("Input Settings")]
    public GameObject continuePrompt;
    public KeyCode[] continueKeys = { KeyCode.Space, KeyCode.Return, KeyCode.E };

    [Header("Default Settings")]
    public float defaultFadeSpeed = 2.5f;
    public float defaultDisplayTime = 3f;

    private bool isTransitioning = false;
    private CanvasGroup canvasGroup;
    public static TransitionScreen Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (transitionCanvas != null) transitionCanvas.sortingOrder = 999;
        if (continuePrompt != null) continuePrompt.SetActive(false);

        // IMPORTANTE: No usamos SetActive(false) aquí para que el objeto siempre pueda recibir llamadas
        InitialState();
    }

    void InitialState()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        HideLore();
    }

    public void TransitionToScene(string sceneName, LoreData loreData = null)
    {
        // Si por alguna razón el objeto se desactivó, lo forzamos a activarse
        if (!gameObject.activeInHierarchy) gameObject.SetActive(true);

        if (isTransitioning) return;
        StartCoroutine(TransitionSequence(sceneName, loreData));
    }

    public void QuickTransition(string sceneName)
    {
        if (!gameObject.activeInHierarchy) gameObject.SetActive(true);

        if (isTransitioning) return;
        StartCoroutine(TransitionSequence(sceneName, null));
    }

    IEnumerator TransitionSequence(string sceneName, LoreData loreData)
    {
        isTransitioning = true;
        canvasGroup.blocksRaycasts = true; // Bloqueamos clics durante la transición

        // 1. Fade a negro
        yield return StartCoroutine(FadeCanvas(1f, GetFadeSpeed(loreData)));

        // 2. Lore e Input
        if (loreData != null && !string.IsNullOrEmpty(loreData.loreText))
        {
            float originalTimeScale = Time.timeScale;
            Time.timeScale = 0f;

            ShowLore(loreData);
            if (continuePrompt != null) continuePrompt.SetActive(true);

            yield return null; // Esperar un frame para evitar que el clic que activó esto también lo cierre

            bool inputPressed = false;
            while (!inputPressed)
            {
                // Usamos GetKeyDown para detectar el pulso de la tecla
                foreach (KeyCode key in continueKeys)
                {
                    if (Input.GetKeyDown(key)) { inputPressed = true; break; }
                }
                yield return null;
            }

            if (continuePrompt != null) continuePrompt.SetActive(false);
            HideLore();
            Time.timeScale = originalTimeScale;
        }

        // 3. Carga de escena
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone) yield return null;

        // 4. Pequeña espera y Fade out
        yield return new WaitForSecondsRealtime(0.3f);
        yield return StartCoroutine(FadeCanvas(0f, GetFadeSpeed(loreData)));

        isTransitioning = false;
        canvasGroup.blocksRaycasts = false; // Liberamos los clics
    }

    IEnumerator FadeCanvas(float targetAlpha, float speed)
    {
        while (!Mathf.Approximately(canvasGroup.alpha, targetAlpha))
        {
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, speed * Time.unscaledDeltaTime);
            yield return null;
        }
        canvasGroup.alpha = targetAlpha;
    }

    void ShowLore(LoreData loreData)
    {
        if (titleText != null) { titleText.text = loreData.title; titleText.gameObject.SetActive(true); }
        if (loreText != null) { loreText.text = loreData.loreText; loreText.gameObject.SetActive(true); }
    }

    void HideLore()
    {
        if (titleText != null) titleText.gameObject.SetActive(false);
        if (loreText != null) loreText.gameObject.SetActive(false);
    }

    float GetFadeSpeed(LoreData loreData) => loreData != null ? loreData.fadeSpeed : defaultFadeSpeed;
}