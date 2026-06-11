using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Expiria3DSpace;
using UnityEngine;

public class ARViewPanelManager : MonoBehaviour
{
    static ARViewPanelManager instance;

    public static ARViewPanelManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<ARViewPanelManager>(true);
            return instance;
        }
    }

    public ProductInstance productInstance;

    RectTransform contentParent;

    public GameObject scanMePanelForImage;
    public GameObject worldARViewPanel;

    void Awake()
    {
        contentParent = productInstance.transform.parent.GetComponent<RectTransform>();

        if (ARManager.Instance.arProjectType == ARManager.ARProjectType.Image)
        {
            scanMePanelForImage.SetActive(true);
            worldARViewPanel.SetActive(false);
        }
        else if (ARManager.Instance.arProjectType == ARManager.ARProjectType.World)
        {
            scanMePanelForImage.SetActive(false);
            worldARViewPanel.SetActive(true);
        }
    }

    void OnEnable()
    {
        //Initialize the product in view if it is not initialized
        if (productInstance.product == null || string.IsNullOrEmpty(productInstance.product.id))
        {
            if (MenuManager.Instance.settings.expiria_order_manager_categories.Length > 0
            && MenuManager.Instance.settings.expiria_order_manager_categories[0].expiria_order_manager_products.Length > 0)
            {
                SetProductInView(MenuManager.Instance.settings.expiria_order_manager_categories[0].expiria_order_manager_products[0]);
            }
        }
    }

    public void SetProductInView(Product product)
    {
        productInstance.product = product;

        OpenARView(product.ar_view_url, ParseVector3(product.ar_position), ParseVector3(product.ar_scale), ParseVector3(product.ar_rotation));

        this.gameObject.SetActive(true);
    }

    bool startAnimToCenter = false;
    void Update()
    {
        if (startAnimToCenter)
        {
            // Usar Lerp con suavizado y deltaTime
            contentParent.anchoredPosition = Vector2.Lerp(
                contentParent.anchoredPosition,
                new Vector2(0, contentParent.anchoredPosition.y),
                5 * Time.deltaTime
            );

            if (Mathf.Abs(contentParent.anchoredPosition.x) < 10f)
            {
                contentParent.anchoredPosition = new Vector2(0, contentParent.anchoredPosition.y);
                startAnimToCenter = false;
            }
        }
        else
        {
            if (contentParent.anchoredPosition.x < -90f)
            {
                LoadPreviousProduct();
                startAnimToCenter = true;
            }
            else if (contentParent.anchoredPosition.x > 90f)
            {
                LoadNextProduct();
                startAnimToCenter = true;
            }
        }
    }

    public void LoadNextProduct()
    {
        Product currentProduct = productInstance.product;
        if (currentProduct == null)
        {
            SetProductInView(GetFirstProduct());
            return;
        }

        int currentCatIndex = -1;
        int currentProdIndex = -1;
        MenuManager.Category[] categories = MenuManager.Instance.settings.expiria_order_manager_categories;

        // Find current product's position
        for (int catIdx = 0; catIdx < categories.Length; catIdx++)
        {
            Product[] products = categories[catIdx].expiria_order_manager_products;
            if (products == null) continue;

            for (int prodIdx = 0; prodIdx < products.Length; prodIdx++)
            {
                if (products[prodIdx].id == currentProduct.id)
                {
                    currentCatIndex = catIdx;
                    currentProdIndex = prodIdx;
                    break;
                }
            }
            if (currentCatIndex != -1) break;
        }

        if (currentCatIndex == -1 || currentProdIndex == -1)
        {
            SetProductInView(GetFirstProduct());
            return;
        }

        // Try next product in current category
        Product[] currentProducts = categories[currentCatIndex].expiria_order_manager_products;
        if (currentProdIndex < currentProducts.Length - 1)
        {
            SetProductInView(currentProducts[currentProdIndex + 1]);
            return;
        }

        // Find next category with products
        for (int catIdx = currentCatIndex + 1; catIdx < categories.Length; catIdx++)
        {
            Product[] products = categories[catIdx].expiria_order_manager_products;
            if (products != null && products.Length > 0)
            {
                SetProductInView(products[0]);
                return;
            }
        }

        // Wrap around to beginning
        for (int catIdx = 0; catIdx < currentCatIndex; catIdx++)
        {
            Product[] products = categories[catIdx].expiria_order_manager_products;
            if (products != null && products.Length > 0)
            {
                SetProductInView(products[0]);
                return;
            }
        }
    }

    public void LoadPreviousProduct()
    {
        Product currentProduct = productInstance.product;
        if (currentProduct == null)
        {
            SetProductInView(GetFirstProduct());
            return;
        }

        int currentCatIndex = -1;
        int currentProdIndex = -1;
        MenuManager.Category[] categories = MenuManager.Instance.settings.expiria_order_manager_categories;

        // Find current product's position
        for (int catIdx = 0; catIdx < categories.Length; catIdx++)
        {
            Product[] products = categories[catIdx].expiria_order_manager_products;
            if (products == null) continue;

            for (int prodIdx = 0; prodIdx < products.Length; prodIdx++)
            {
                if (products[prodIdx].id == currentProduct.id)
                {
                    currentCatIndex = catIdx;
                    currentProdIndex = prodIdx;
                    break;
                }
            }
            if (currentCatIndex != -1) break;
        }

        if (currentCatIndex == -1 || currentProdIndex == -1)
        {
            SetProductInView(GetFirstProduct());
            return;
        }

        // Try previous product in current category
        if (currentProdIndex > 0)
        {
            Product[] currentProducts = categories[currentCatIndex].expiria_order_manager_products;
            SetProductInView(currentProducts[currentProdIndex - 1]);
            return;
        }

        // Find previous category with products
        for (int catIdx = currentCatIndex - 1; catIdx >= 0; catIdx--)
        {
            Product[] products = categories[catIdx].expiria_order_manager_products;
            if (products != null && products.Length > 0)
            {
                SetProductInView(products[products.Length - 1]);
                return;
            }
        }

        // Wrap around to end
        for (int catIdx = categories.Length - 1; catIdx > currentCatIndex; catIdx--)
        {
            Product[] products = categories[catIdx].expiria_order_manager_products;
            if (products != null && products.Length > 0)
            {
                SetProductInView(products[products.Length - 1]);
                return;
            }
        }
    }

    private Product GetFirstProduct()
    {
        foreach (MenuManager.Category category in MenuManager.Instance.settings.expiria_order_manager_categories)
        {
            Product[] products = category.expiria_order_manager_products;
            if (products != null && products.Length > 0)
            {
                return products[0];
            }
        }
        return null;
    }


    private Vector3 ParseVector3(string vector3String)
    {
        string[] values = vector3String.Trim().Split(',');
        return new Vector3(float.Parse(values[0], CultureInfo.InvariantCulture), float.Parse(values[1], CultureInfo.InvariantCulture), float.Parse(values[2], CultureInfo.InvariantCulture));
    }

    private void OpenARView(string modelURL, Vector3 position, Vector3 scale, Vector3 rotation)
    {
        this.gameObject.SetActive(true);
        gameObject.SetActive(false);
        LoadModelFromURL.Instance.LoadModel(modelURL, position, scale, rotation);
    }
}
