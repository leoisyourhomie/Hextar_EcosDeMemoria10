using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using static CurrencyUtils;
using System.Linq;

public class MyOrder : MonoBehaviour
{
    public ProductInstance productTemplate;

    public GameObject infoBeforeSubmitPanel;
    public GameObject inputDataTemplate;
    public Button orderNowButton;
    public Button submitButton;

    public TMP_Text totalToPayText;

    double totalToPay = 0;

    public TMP_Text numberOfProductsInCartText;

    //The GameObject is the list of ordered products for "My Order" page
    static List<Product> productsInCart = new List<Product>();

    private TMP_InputField[] inputFieldsOfInfoBeforeSubmit;

    static MyOrder instance;

    public static MyOrder Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<MyOrder>(true);
            return instance;
        }
    }

    void Awake()
    {
        productTemplate.gameObject.SetActive(false);

        //Set the input fields of info before submit
        inputFieldsOfInfoBeforeSubmit = new TMP_InputField[MenuManager.Instance.settings.additional_info_before_checkout.Length];

        //Instantiate the input data template based on the number of additional_info_before_checkout
        for (int i = 0; i < MenuManager.Instance.settings.additional_info_before_checkout.Length; i++)
        {
            GameObject inputDataTemplateObj = Instantiate(inputDataTemplate, inputDataTemplate.transform.parent);
            inputDataTemplateObj.SetActive(true);
            inputDataTemplateObj.transform.Find("InputTitle").GetComponent<TMP_Text>().text = MenuManager.Instance.settings.additional_info_before_checkout[i];
            inputDataTemplateObj.transform.Find("InputField/Text Area/Placeholder").GetComponent<TMP_Text>().text = MenuManager.Instance.settings.additional_info_before_checkout[i];
            inputDataTemplateObj.transform.Find("InputField").GetComponent<TMP_InputField>().text = "";
            inputFieldsOfInfoBeforeSubmit[i] = inputDataTemplateObj.transform.Find("InputField").GetComponent<TMP_InputField>();
        }

        orderNowButton.onClick.AddListener(OrderNow);

        inputDataTemplate.SetActive(false);
        //Move the Submit button to the bottom of the screen
        submitButton.transform.SetAsLastSibling();
    }

    void OnEnable()
    {
        RefreshCart();
    }

    void RefreshCart()
    {
        //Destroy all the products in the cart, except the productTemplate
        foreach (Transform child in productTemplate.transform.parent)
        {
            if (child != productTemplate.transform)
            {
                Destroy(child.gameObject);
            }
        }

        //disable the productTemplate
        productTemplate.gameObject.SetActive(false);

        foreach (Product product in productsInCart)
        {
            GameObject productObj = Instantiate(productTemplate.gameObject, productTemplate.transform.parent);

            ProductInstance productInstance = productObj.GetComponent<ProductInstance>();
            productInstance.product = product;
            productInstance.pageType = ProductInstance.PageType.MyOrder;
            productObj.SetActive(true); //We need to set the SetActive after assign the product to the productInstance because we have some checks in the OnEnable
        }
    }

    public void AddProductToCart(Product product)
    {
        if (productsInCart.Contains(product))
        {
            product.quantity++;
        }
        else
        {
            product.quantity = 1;
            productsInCart.Add(product);
        }

        product.onQuantityChanged?.Invoke();

        UpdateTotalToPayText();
        numberOfProductsInCartText.text = productsInCart.Sum(p => p.quantity).ToString();
    }

    public void RemoveProductFromCart(Product product)
    {
        if (productsInCart.Contains(product))
        {
            product.quantity--;
            product.onQuantityChanged?.Invoke();

            if (product.quantity <= 0)
            {
                productsInCart.Remove(product);
            }
        }

        UpdateTotalToPayText();
        numberOfProductsInCartText.text = productsInCart.Sum(p => p.quantity).ToString();
    }



    public void OrderNow()
    {
        if (MenuManager.Instance.settings.additional_info_before_checkout.Length > 0)
        {
            infoBeforeSubmitPanel.SetActive(true);
            submitButton.onClick.RemoveAllListeners();
            submitButton.onClick.AddListener(SubmitOrder);
        }
        else
        {
            SubmitOrder();
        }
    }

    public void UpdateTotalToPayText()
    {
        //Recalculate the total to pay
        totalToPay = 0;
        bool anyProductHasDecimals = false;
        foreach (Product product in productsInCart)
        {
            totalToPay += FromPriceToDouble(product.price) * product.quantity;
            if (HasDecimals(product.price))
            {
                anyProductHasDecimals = true;
            }
        }

        //If any product has decimals, round the total to pay to 2 decimals
        if (anyProductHasDecimals)
        {
            totalToPay = Math.Round(totalToPay, 2);
        }

        if (productsInCart.Count > 0)
        {
            totalToPayText.text = "TOTAL: " + GetTextBeforePrice(productsInCart.First().price.ToString()) + totalToPay.ToString() + GetTextAfterPrice(productsInCart.First().price.ToString());
        }
        else
        {
            totalToPayText.text = "TOTAL: 0";
        }
    }



    public void SubmitOrder()
    {
        string message = MenuManager.Instance.settings.pre_resume_text_wpp + "%0a%0a";

        if (MenuManager.Instance.settings.include_prices_in_resume || MenuManager.Instance.settings.include_total_to_pay_in_resume)
        {
            foreach (Product product in productsInCart)
            {
                if (MenuManager.Instance.settings.include_prices_in_resume)
                {
                    message += "• " + product.name + ": " + product.price + (product.quantity > 1 ? " (x" + product.quantity + ")" : "") + "%0a";
                }
                else
                {
                    message += "• " + product.name + (product.quantity > 1 ? " (x" + product.quantity + ")" : "") + "%0a";
                }
            }
        }

        if (MenuManager.Instance.settings.include_total_to_pay_in_resume)
        {
            if (productsInCart.Count > 0)
            {
                message += "%0aTotal: " + GetTextBeforePrice(productsInCart.First().price.ToString()) + totalToPay.ToString() + GetTextAfterPrice(productsInCart.First().price.ToString());
                message += "%0a";
            }
        }

        for (int i = 0; i < MenuManager.Instance.settings.additional_info_before_checkout.Length; i++)
        {
            message += "%0a" + MenuManager.Instance.settings.additional_info_before_checkout[i] + ": " + inputFieldsOfInfoBeforeSubmit[i].text + "%0a";
        }

        message += "%0a" + MenuManager.Instance.settings.post_resume_text_wpp;

        //Open the whatsapp with the message
        Application.OpenURL("https://wa.me/" + MenuManager.Instance.settings.whatsapp_number + "?text=" + message);

        if (MenuManager.Instance.settings.additional_info_before_checkout.Length > 0)
        {
            infoBeforeSubmitPanel.SetActive(false);
        }
    }
}
