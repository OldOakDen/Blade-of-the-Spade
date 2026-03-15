using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;

public class HintTextBox : MonoBehaviour
{
    public TextMeshProUGUI textMeshPro;
    public LocalizeStringEvent localizeStringEvent;
    private Coroutine displayCoroutine;
    public float intervalBetweenTexts = 3.0f; // interval mezi jednotlivými øetìzci
    public float wordDelay = 0.25f; // interval mezi jednotlivými slovy
    public float sentencePause = 1.0f; // pauza mezi vìtami
    public float cyclePause = 5.0f; // pauza po dokonèení cyklu

    void Awake()
    {
        if (textMeshPro == null)
        {
            textMeshPro = GetComponent<TextMeshProUGUI>();
        }

        if (localizeStringEvent == null)
        {
            localizeStringEvent = GetComponent<LocalizeStringEvent>();
        }
    }

    public void DisplayLocalizedText(string tableName, params string[] textKeys)
    {
        if (displayCoroutine != null)
        {
            StopCoroutine(displayCoroutine);
        }

        displayCoroutine = StartCoroutine(DisplayTextCoroutine(tableName, intervalBetweenTexts, wordDelay, textKeys));
    }

    private IEnumerator DisplayTextCoroutine(string tableName, float intervalBetweenTexts, float wordDelay, string[] textKeys)
    {
        while (true)
        {
            foreach (string key in textKeys)
            {
                localizeStringEvent.StringReference.TableReference = tableName;
                localizeStringEvent.StringReference.TableEntryReference = key;

                yield return localizeStringEvent.StringReference.GetLocalizedStringAsync().Task;

                string localizedText = localizeStringEvent.StringReference.GetLocalizedString();
                yield return StartCoroutine(TypeText(localizedText, wordDelay));

                yield return new WaitForSeconds(intervalBetweenTexts);

                textMeshPro.text = ""; // clear text
                yield return new WaitForSeconds(sentencePause);
            }

            yield return new WaitForSeconds(cyclePause);
        }
    }

    private IEnumerator TypeText(string text, float wordDelay)
    {
        string[] words = text.Split(' ');
        textMeshPro.text = "";

        foreach (string word in words)
        {
            textMeshPro.text += word + " ";
            yield return new WaitForSeconds(wordDelay);
        }
    }
}
