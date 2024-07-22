using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ReviewPanelManagerUI : MonoBehaviour
{
    [SerializeField] private GameObject _reviewsListBackground;
    [SerializeField] private GameObject _reviewsListPanel;
    [SerializeField] private RatingBarStarUI _ratingBarStar;
    [SerializeField] private Button _reveiwsBtn;

    [Space(10)]
    [SerializeField] private GameObject _reviewPrefab;
    [SerializeField] private Transform _reviewListContent;

    [Space(10)]
    [SerializeField] private TextMeshProUGUI _globalReview;

    [Space(10)]
    [Header("Manager")]
    [SerializeField] private HotelRateManager _hotelRateManager;

    public UnityEvent OnReviewAdd = new UnityEvent();

    public void Start()
    {
        _hotelRateManager = FindObjectOfType<HotelRateManager>();

        _reviewsListBackground.SetActive(false);
        _reviewsListPanel.SetActive(false);

        _reveiwsBtn.onClick.AddListener(() =>
        {
            ToggleReviewsListPanel();
        });

        UpdateGlobalReviewAtStart();

        _hotelRateManager.OnReviewAdd += UpdateGlobalReview;
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            UpdateGlobalReview();
            UpdateReviewList();
        }
    }

    private void UpdateGlobalReview()
    {
        _ratingBarStar.UpdateBar(_hotelRateManager.averageCurrentRating);

        float _noteGlobale;
        _noteGlobale = Mathf.Round(_hotelRateManager.averageCurrentRating * 10) / 10;

        _globalReview.text = "Note globale "+ _noteGlobale.ToString()+ "/5";

        string reviewBtnText = "";

        if (_hotelRateManager.totalReviews == 0)
        {
            reviewBtnText = "0 avis";
        }
        else
        {
            reviewBtnText = _hotelRateManager.totalReviews + " avis";
        }
        _reveiwsBtn.GetComponentInChildren<TextMeshProUGUI>().text = reviewBtnText;

        UpdateReviewList();
        
        OnReviewAdd.Invoke();
    }

    private void ToggleReviewsListPanel()
    {
        _reviewsListBackground.SetActive(!_reviewsListBackground.activeSelf);
        _reviewsListPanel.SetActive(!_reviewsListPanel.activeSelf);
    }

    private void UpdateReviewList()
    {
        foreach ( Transform child in _reviewListContent )
        {
            Destroy( child.gameObject );
        }

        foreach (RateReviews review in _hotelRateManager.listReviews)
        {
            GameObject reviewRow = Instantiate( _reviewPrefab, _reviewListContent);

            reviewRow.GetComponent<RatingBarStarUI>().UpdateBar( review.note );
            reviewRow.GetComponentInChildren<TextMeshProUGUI>().text = review.review;
        }
    }

    private void UpdateGlobalReviewAtStart()
    {
        _ratingBarStar.UpdateBar(_hotelRateManager.averageCurrentRating);

        float _noteGlobale;
        _noteGlobale = Mathf.Round(_hotelRateManager.averageCurrentRating * 10) / 10;

        _globalReview.text = "Note globale " + _noteGlobale.ToString() + "/5";

        string reviewBtnText = "";

        if (_hotelRateManager.totalReviews == 0)
        {
            reviewBtnText = "0 avis";
        }
        else
        {
            reviewBtnText = _hotelRateManager.totalReviews + " avis";
        }
        _reveiwsBtn.GetComponentInChildren<TextMeshProUGUI>().text = reviewBtnText;

        UpdateReviewList();

    }
}
