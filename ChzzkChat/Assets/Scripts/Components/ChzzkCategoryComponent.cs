using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ChzzkCategoryComponent : ChzzkComponentBase
{
    [SerializeField] private InputField categoryInputField;
    [SerializeField] private Slider querySizeSlider;
    [SerializeField] private TMP_Text querySizeText;
    
    /// <summary>
    /// 방송은 개별 게임 카테고리 또는 종합 게임, 데모 게임, 고전 게임, 스포츠, 축구, 야구, talk, ASMR, 음악/노래, 그림/아트, 운동/건강, 과학/기술, 시사/경제, 먹방/쿡방, 뷰티, 여행/캠페인 카테고리로 분류될 수 있습니다.
    /// </summary>
    private string categoryName = "";
    private int size = 20;

    private void Start()
    {
        categoryInputField.onValueChanged.AddListener(HandleCategoryChanged);
        querySizeSlider.onValueChanged.AddListener(HandleQuerySizeChanged);
        querySizeSlider.value = size;
        categoryName = categoryInputField.text;
    }

    private void HandleCategoryChanged(string categoryName)
    {
        this.categoryName = categoryName;
    }
    private void HandleQuerySizeChanged(float size)
    {
        this.size = Mathf.FloorToInt(size);
        querySizeText.SetText(size.ToString());
    }
    
    public override EAPICategory GetAPICategory()
    {
        return EAPICategory.Category;
    }
    
    public override void DoAction(string action, string parameter)
    {
        base.DoAction(action, parameter);

        if (action == "GET")
        {
            StartCoroutine(GetCategoryInfo());
        }
    }
    
    IEnumerator GetCategoryInfo()
    {
        string url = $"{ChzzkController.BaseURL}/open/v1/categories/search?query={categoryName}&size={size}";
        Debug.Log($"[Category] Requset URL : {url}");
        using (UnityWebRequest request = UnityWebRequest.Get($"{url}"))
        {
            request.SetRequestHeader("Client-Id", cachedChzzkController.ClientId);
            request.SetRequestHeader("Client-Secret", cachedChzzkController.ClientSecret);
            request.SetRequestHeader("Content-Type", ChzzkController.ContentType);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[Category] Response: " + request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("[Category] Error: " + request.error);
            }
        }
    }
}
