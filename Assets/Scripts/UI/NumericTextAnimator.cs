using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// G�re l'animation des valeurs num�riques dans les composants TextMeshPro.
/// Cette classe est impl�ment�e comme un singleton pour une utilisation facile dans tout le projet.
/// </summary>
public class NumericTextAnimator : MonoBehaviour
{
    private static NumericTextAnimator _instance;

    /// <summary>
    /// Instance singleton de NumericTextAnimator.
    /// </summary>
    public static NumericTextAnimator Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("NumericTextAnimator");
                _instance = go.AddComponent<NumericTextAnimator>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    /// <summary>
    /// Dictionnaire stockant les files d'attente d'animations pour chaque TextMeshPro.
    /// </summary>
    private Dictionary<TextMeshProUGUI, Queue<AnimationInfo>> animationQueues = new Dictionary<TextMeshProUGUI, Queue<AnimationInfo>>();

    /// <summary>
    /// Classe interne repr�sentant les informations d'une animation.
    /// </summary>
    private class AnimationInfo
    {
        /// <summary>
        /// La valeur cible de l'animation.
        /// </summary>
        public int TargetValue;

        /// <summary>
        /// La dur�e de l'animation en secondes.
        /// </summary>
        public float Duration;

        public string Suffix;

        /// <summary>
        /// Constructeur pour AnimationInfo.
        /// </summary>
        /// <param name="targetValue">La valeur cible de l'animation.</param>
        /// <param name="duration">La dur�e de l'animation en secondes.</param>
        /// <param name="suffix">Le suffixe � ajouter � la fin de la valeur. Par d�faut une cha�ne vide.</param>
        public AnimationInfo(int targetValue, float duration, string suffix = "")
        {
            TargetValue = targetValue;
            Duration = duration;
            Suffix = suffix;
        }
    }

    /// <summary>
    /// Ajoute une nouvelle animation � la file d'attente pour un TextMeshPro sp�cifique.
    /// Si aucune animation n'est en cours, d�marre imm�diatement l'animation.
    /// </summary>
    /// <param name="textMesh">Le composant TextMeshPro � animer.</param>
    /// <param name="newValue">La nouvelle valeur � atteindre.</param>
    /// <param name="duration">La dur�e de l'animation en secondes. Par d�faut 1 seconde.</param>
    public void AnimateTextTo(TextMeshProUGUI textMesh, int newValue, float duration = 1f, string suffix = "")
    {
        if (!animationQueues.ContainsKey(textMesh))
        {
            animationQueues[textMesh] = new Queue<AnimationInfo>();
        }

        animationQueues[textMesh].Enqueue(new AnimationInfo(newValue, duration, suffix));

        if (animationQueues[textMesh].Count == 1)
        {
            StartCoroutine(AnimateTextCoroutine(textMesh));
        }
    }

    /// <summary>
    /// Coroutine qui g�re l'animation d'un TextMeshPro sp�cifique.
    /// Cette coroutine continue � s'ex�cuter tant qu'il y a des animations dans la file d'attente.
    /// </summary>
    /// <param name="textMesh">Le composant TextMeshPro � animer.</param>
    private IEnumerator AnimateTextCoroutine(TextMeshProUGUI textMesh)
    {
        while (animationQueues[textMesh].Count > 0)
        {
            AnimationInfo info = animationQueues[textMesh].Peek();
            int startValue = int.Parse(textMesh.text.Split(' ')[0]);  // Prend en compte le cas o� il y a d�j� un suffixe
            float elapsedTime = 0f;

            while (elapsedTime < info.Duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / info.Duration;
                int currentValue = Mathf.RoundToInt(Mathf.Lerp(startValue, info.TargetValue, t));
                textMesh.text = currentValue.ToString() + (string.IsNullOrEmpty(info.Suffix) ? "" : " " + info.Suffix);
                yield return null;
            }

            textMesh.text = info.TargetValue.ToString() + (string.IsNullOrEmpty(info.Suffix) ? "" : " " + info.Suffix);
            animationQueues[textMesh].Dequeue();
        }
    }
}