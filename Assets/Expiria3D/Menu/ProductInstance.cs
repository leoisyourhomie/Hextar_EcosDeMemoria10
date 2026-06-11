using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Serializable classes to parse JSON data
[Serializable]
public class Product
{
    public string id;
    public string name;
    public string icon_url;
    public string description;
    public string price;
    public MenuManager.Translation[] translations;
    public string ar_view_url;
    public string ar_scale;
    public string ar_position;
    public string ar_rotation;
    public string ar_view_type;
    public string category_id;

    //This is not loaded from the database, it is added when the user adds the product to the cart
    public int quantity;

    public Action onQuantityChanged;
}

public class ProductInstance : MonoBehaviour
{
    public Product product;
    public Image productIcon;
    public TMP_Text productName;
    public TMP_Text productDescription;
    public TMP_Text productPrice;
    public Button buttonAddToCart;
    public Button buttonAddPlus;
    public Button buttonAddMinus;
    public Button buttonSeeInAR;
    public TMP_Text textQuantity;

    public GameObject quantityModifierPanel;

    public enum PageType
    {
        MainMenu,
        MyOrder,
        ARView
    }

    public PageType pageType = PageType.MainMenu;

    void OnEnable()
    {
        if (product != null && !string.IsNullOrEmpty(product.id))
        {
            SetInfo();

            UpdateQuantity();
            product.onQuantityChanged += OnQuantityChanged;
        }
    }

    void SetInfo()
    {
        if (productName != null)
            productName.text = product.name;

        if (productDescription != null)
            productDescription.text = product.description;

        if (productPrice != null)
            productPrice.text = product.price;

        //Remove the onClick listeners from the buttons
        if (buttonAddToCart != null)
        {
            buttonAddToCart.onClick.RemoveAllListeners();
            buttonAddToCart.onClick.AddListener(() => MyOrder.Instance.AddProductToCart(product));
        }

        if (buttonAddPlus != null)
        {
            buttonAddPlus.onClick.RemoveAllListeners();
            buttonAddPlus.onClick.AddListener(() => MyOrder.Instance.AddProductToCart(product));
        }

        if (buttonAddMinus != null)
        {
            buttonAddMinus.onClick.RemoveAllListeners();
            buttonAddMinus.onClick.AddListener(() => MyOrder.Instance.RemoveProductFromCart(product));
        }

        Sprite sprite = null;

        if (pageType != PageType.MainMenu)
        {
            //Try to get the image from the Menu, otherwise, load it.
            ProductInstance productInstance = MenuManager.Instance.productTemplate.transform.parent.GetComponentsInChildren<ProductInstance>().First(x => x.product.id == product.id);

            if (productInstance != null && productInstance.productIcon != null && productInstance.productIcon.sprite != null)
            {
                sprite = productInstance.productIcon.sprite;
            }

            if (sprite != null)
            {
                productIcon.sprite = sprite;
            }
            else
            {
                StartCoroutine(MenuManager.Instance.SetImage(productIcon, product.icon_url));
            }
        }
        else
        {
            StartCoroutine(MenuManager.Instance.SetImage(productIcon, product.icon_url));

            Button productIconButton = productIcon.GetComponent<Button>();
            if (productIconButton != null)
            {
                productIconButton.onClick.RemoveAllListeners();
                productIconButton.onClick.AddListener(() =>
                {
                    ARViewPanelManager.Instance.SetProductInView(product);
                    MenuManager.Instance.gameObject.SetActive(false);
                });
            }
        }

        if (buttonSeeInAR != null)
        {
            if (product.ar_view_url != null && product.ar_view_url != "")
            {
                buttonSeeInAR.onClick.RemoveAllListeners();
                buttonSeeInAR.onClick.AddListener(() =>
                {
                    ARViewPanelManager.Instance.SetProductInView(product);
                    MenuManager.Instance.gameObject.SetActive(false);
                });
            }
            else
            {
                buttonSeeInAR.gameObject.SetActive(false);
            }
        }


    }

    void OnDisable()
    {
        if (product != null && !string.IsNullOrEmpty(product.id))
        {
            product.onQuantityChanged -= OnQuantityChanged;
        }
    }

    void OnQuantityChanged()
    {
        UpdateQuantity();
    }

    void UpdateQuantity()
    {
        if (textQuantity != null)
            textQuantity.text = product.quantity.ToString();

        if (product.quantity > 0)
        {
            if (quantityModifierPanel != null)
                quantityModifierPanel.SetActive(true);
            if (buttonAddToCart != null)
                buttonAddToCart.gameObject.SetActive(false);
        }
        else
        {
            if (pageType != PageType.MyOrder)
            {
                if (quantityModifierPanel != null)
                    quantityModifierPanel.SetActive(false);
                if (buttonAddToCart != null)
                    buttonAddToCart.gameObject.SetActive(true);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
