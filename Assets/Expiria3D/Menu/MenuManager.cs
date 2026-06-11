using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System;
using System.Linq;
using static CurrencyUtils;
using System.Globalization;

public class MenuManager : MonoBehaviour
{
    //Settings
    public Image headerBackground;
    public Image logo;
    public TMP_Dropdown languageDropdown;
    public TMP_Text mainMenuButtonText;
    public TMP_Text seeInArButtonText;
    public TMP_Text cartButtonText;
    public TMP_Text addButtonText;

    //Category
    public Image categoryButton;
    public TMP_Text categoryButtonText;

    private const string SUPABASE_URL = "https://tpwwnbjhlnyoyeyrplkb.supabase.co/rest/v1/expiria_order_manager_settings";
    private const string API_KEY = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InRwd3duYmpobG55b3lleXJwbGtiIiwicm9sZSI6ImFub24iLCJpYXQiOjE3MDE3NDk3MDgsImV4cCI6MjAxNzMyNTcwOH0.Mfr93JX7mtjsGgThB2rj_yHZByxWu0x79K4MpsVK9YQ";

    [Serializable]
    public class Category
    {
        public string id;
        public string name;
        public Translation[] translations;
        public Product[] expiria_order_manager_products; // Make sure this matches the relation name in your database
    }

    public class Translation
    {
        public int lang_index;
        public string column_name;
        public string translation;
    }

    [Serializable]
    public class Settings
    {
        public string project_id;
        public string[] languages;
        public string txt_main_menu_btn;
        public string txt_see_in_ar_btn;
        public string txt_cart_btn;
        public string txt_add_btn;
        public string default_language_name;
        public string language_on_start;
        public string whatsapp_number;
        public bool include_prices_in_resume;
        public bool include_total_to_pay_in_resume;
        public string pre_resume_text_wpp;
        public string post_resume_text_wpp;
        public string[] additional_info_before_checkout;
        public string app_logo_url;
        public string header_background_url;
        public string header_color;
        public Translation[] translations;
        public string categories_background_color;
        public string categories_text_color;
        public Category[] expiria_order_manager_categories; // Make sure this matches the relation name in your database
    }

    [Serializable]
    public class SettingsList
    {
        public Settings[] items;
    }

    public Settings settings;
    public ProductInstance productTemplate;
    static MenuManager instance;

    public static MenuManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<MenuManager>(true);
            return instance;
        }
    }

    void Start()
    {
        // GameObject.Find("Title").GetComponent<TMP_Text>().text = URLParameters.location_pathname();
        LoadSettings("b2926422-0768-4cec-80f8-b62ff6f5962c");

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 30;
    }

    // Function to load settings using a project_id
    public void LoadSettings(string projectId)
    {
        StartCoroutine(GetSettingsFromSupabase(projectId));
    }

    private IEnumerator GetSettingsFromSupabase(string projectId)
    {
        // Build URL with select parameter and project_id filter
        string settingsColumns = "project_id,languages,txt_main_menu_btn,txt_see_in_ar_btn,txt_cart_btn,txt_add_btn,default_language_name,language_on_start,whatsapp_number,include_prices_in_resume,include_total_to_pay_in_resume,pre_resume_text_wpp,post_resume_text_wpp,additional_info_before_checkout,app_logo_url,header_background_url,header_color,translations,categories_background_color,categories_text_color,expiria_order_manager_categories(id,name,translations,expiria_order_manager_products(id,name,icon_url,description,price,translations,ar_view_url,ar_view_type,ar_scale,ar_position,ar_rotation,category_id)))";

        string url = SUPABASE_URL + "?select=" + UnityWebRequest.EscapeURL(settingsColumns) + "&project_id=eq." + UnityWebRequest.EscapeURL(projectId);

        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("apikey", API_KEY);
        request.SetRequestHeader("Authorization", "Bearer " + API_KEY);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string jsonResult = request.downloadHandler.text;
            // Wrap the JSON for parsing
            SettingsList settingsList = WrapJson(jsonResult);

            settings = settingsList.items[0];

            languageDropdown.options.Clear();
            languageDropdown.options.Add(new TMP_Dropdown.OptionData(settings.default_language_name));
            foreach (string language in settings.languages)
            {
                languageDropdown.options.Add(new TMP_Dropdown.OptionData(language));
            }
            //Find the index from dropdown options
            languageDropdown.value = Array.IndexOf(languageDropdown.options.Select(option => option.text).ToArray(), settings.language_on_start);

            if (languageDropdown.value == -1)
            {
                languageDropdown.value = 0;
            }

            mainMenuButtonText.text = settings.txt_main_menu_btn;
            seeInArButtonText.text = settings.txt_see_in_ar_btn;
            cartButtonText.text = settings.txt_cart_btn;
            addButtonText.text = settings.txt_add_btn;

            StartCoroutine(SetImage(logo, settings.app_logo_url));
            StartCoroutine(SetImage(headerBackground, settings.header_background_url));
            SetColor(headerBackground, settings.header_color);
            SetColor(headerBackground.transform.parent.GetComponent<Image>(), settings.header_color);
            SetColor(categoryButton, settings.categories_background_color);
            SetColor(categoryButtonText, settings.categories_text_color);

            productTemplate.gameObject.SetActive(false);
            categoryButton.gameObject.SetActive(false);
            // Output the categories data
            if (settings.expiria_order_manager_categories != null)
            {
                foreach (Category category in settings.expiria_order_manager_categories)
                {
                    GameObject catButton = Instantiate(categoryButton.gameObject, categoryButton.transform.parent);
                    catButton.GetComponentInChildren<TMP_Text>().text = category.name;
                    catButton.SetActive(true);

                    // Output the products data
                    if (category.expiria_order_manager_products != null)
                    {
                        LoadProductsInCategory(category);
                    }
                }
            }

        }
        else
        {
            string errorContent = request.downloadHandler.text;
            Debug.LogError("Error while getting settings from Supabase:" +
                "\nResponse code: " + request.responseCode +
                "\nError: " + request.error +
                "\nResponse text: " + errorContent);
        }
    }

    void LoadProductsInCategory(Category category)
    {
        foreach (Product product in category.expiria_order_manager_products)
        {
            GameObject productObj = Instantiate(productTemplate.gameObject, productTemplate.transform.parent);

            ProductInstance productInstance = productObj.GetComponent<ProductInstance>();
            productInstance.product = product;
            productInstance.pageType = ProductInstance.PageType.MainMenu;
            productObj.SetActive(true); //We need to set the SetActive after assign the product to the productInstance because we have some checks in the OnEnable
        }
    }


    private SettingsList WrapJson(string json)
    {
        json = "{ \"items\": " + json + "}";
        return JsonUtility.FromJson<SettingsList>(json);
    }

    void SetColor(Image target, string color)
    {
        string[] colors = color.Split(',');
        if (colors.Length == 4)
        {
            Color32 color32 = new Color32(byte.Parse(colors[0]), byte.Parse(colors[1]), byte.Parse(colors[2]), byte.Parse(colors[3]));
            target.color = color32;
        }
        else
        {
            target.color = Color.black;
        }
    }

    void SetColor(TMP_Text target, string color)
    {
        string[] colors = color.Split(',');

        if (colors.Length == 4)
        {
            Color32 color32 = new Color32(byte.Parse(colors[0]), byte.Parse(colors[1]), byte.Parse(colors[2]), byte.Parse(colors[3]));
            target.color = color32;
        }
        else
        {
            target.color = Color.black;
        }
    }

    public IEnumerator SetImage(Image target, string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            target.sprite = Resources.Load<Sprite>("Default");
            yield break;
        }

        UnityWebRequest solicitud = UnityWebRequestTexture.GetTexture(url);
        yield return solicitud.SendWebRequest();

        if (solicitud.result == UnityWebRequest.Result.Success)
        {
            Texture2D textura = DownloadHandlerTexture.GetContent(solicitud);
            Sprite sprite = Sprite.Create(textura, new Rect(0, 0, textura.width, textura.height), new Vector2(0.5f, 0.5f));
            target.sprite = sprite;
        }
        //if error code is 404, set the image to the default image
        else if (solicitud.responseCode == 404)
        {
            target.sprite = Resources.Load<Sprite>("Default");
        }
        else
        {
            Debug.LogError("Error al descargar la imagen: " + solicitud.error + " - " + url);
        }
    }
}
