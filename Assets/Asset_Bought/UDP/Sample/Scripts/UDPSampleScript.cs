using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UDP;

/// <summary>
/// Scripts for the SampleScene.
/// </summary>
public class UDPSampleScript : MonoBehaviour
{
    // Two products shown in the dropdown of sample scene.
    public string Product1;
    public string Product2;

    private static bool m_ConsumeOnPurchase;
    private static bool m_ConsumeOnQuery;

    private Dropdown m_Dropdown;
    private List<Dropdown.OptionData> m_Options;
    private static Text m_TextField;
    private static bool m_Initialized;

    PurchaseListener m_PurchaseListener;
    InitListener m_InitListener;
    LicenseCheckListener m_LicenseCheckListener;


    void Start()
    {
        #region Basic Information Initialization

        m_PurchaseListener = new PurchaseListener();
        m_InitListener = new InitListener();
        m_LicenseCheckListener = new LicenseCheckListener();

        #endregion

        #region Text Field Initialization

        GameObject gameObject = GameObject.Find("Information");
        m_TextField = gameObject.GetComponent<Text>();
        m_TextField.text = "Please Click Init to Start";

        #endregion

        #region DropDown Initialization

        gameObject = GameObject.Find("Dropdown");

        m_Dropdown = gameObject.GetComponent<Dropdown>();
        m_Dropdown.ClearOptions();
        m_Dropdown.options.Add(new Dropdown.OptionData(Product1));
        m_Dropdown.options.Add(new Dropdown.OptionData(Product2));
        m_Dropdown.RefreshShownValue();

        #endregion

        InitUI();

        StoreService.LicenseCheck(m_LicenseCheckListener);
    }

    private static void Show(string message, bool append = false)
    {
        m_TextField.text = append ? string.Format("{0}\n{1}", m_TextField.text, message) : message;
    }

    void InitUI()
    {
        #region Button Initialization

        /*
         * Initialzie the SDK. StoreService.Initialize() function will read GameSettings.asset
         * file to initialize the SDK. However, GameSettings.asset only supports Unity whose
         * version is higher than 5.6.1 (inlcuded). If developers are using older Unity, they
         * should get these information from the UDP portal and fill the AppInfo manually.
         *
         * AppInfo appInfo = new AppInfo();
         * appInfo.AppSlug = "app slug from the portal";
         * appInfo.ClientId = "client id from the portal";
         * appInfo.ClientKey = "client key from the portal";
         * appInfo.RSAPublicKey = "rsa public key from the portal";
         * StoreService.Initialize(initListener, appInfo);
         *
         * In addition, developers using lower version Unity have to save the clientId to
         * Assets/Plugins/Android/assets/GameSettings.prop manually.
         *
         * GameSettings.prop only contains a single line of clientID, for example:
         * DXVCZFVPxp8S1xkliHwYww
         */
        GetButton("InitButton").onClick.AddListener(() =>
        {
            m_Initialized = false;
            Debug.Log("Init button is clicked.");
            Show("Initializing");
            StoreService.Initialize(m_InitListener);
        });

        /*
         * Purchase a product.
         *
         * The purchase flow of UDP is:
         * 1) For consumable products, e.g. 100 coins: purchase -> consume -> deliver
         * 2) For un-consumable products, e.g. removing ads: purchase -> deliver
         *
         * Through this way, if a purchase is paid but for some reason (e.g. crash of
         * the game) the player doesn't get the product, the product can be restored
         * by StoreService.QueryInventory(). Thus, make sure StoreService.QueryInventory()
         * is called after the Initialization is successful.
         */
        GetButton("BuyButton").onClick.AddListener(() =>
        {
            if (!m_Initialized)
            {
                Show("Please Initialize first");
                return;
            }

            string prodcutId = m_Dropdown.options[m_Dropdown.value].text;
            Debug.Log("Buy button is clicked.");
            Show("Buying Product: " + prodcutId);
            m_ConsumeOnPurchase = false;
            Debug.Log(m_Dropdown.options[m_Dropdown.value].text + " will be bought");
            StoreService.Purchase(prodcutId, "payload", m_PurchaseListener);
        });

        /*
         * Purchase a product and consume it (for consumable product).
         * The API is the same but m_ConsumeOnPurchase will be set to true
         * and the product will be consumed in OnPurchase().
         */
        GetButton("BuyConsumeButton").onClick.AddListener(() =>
        {
            if (!m_Initialized)
            {
                Show("Please Initialize first");
                return;
            }

            string prodcutId = m_Dropdown.options[m_Dropdown.value].text;
            Show("Buying Product: " + prodcutId);
            Debug.Log("Buy&Consume button is clicked.");
            m_ConsumeOnPurchase = true;

            StoreService.Purchase(prodcutId, "payload2", m_PurchaseListener);
        });

        List<string> productIds = new List<string> {Product1, Product2};

        /*
         * Query the Inventory. This function should be called right after the
         * initialization succeeds.
         *
         * StoreService.QueryInventory() will return two things:
         * 1) Un-consumed purchase.
         * 2) Queried products information.
         */
        GetButton("QueryButton").onClick.AddListener(() =>
        {
            if (!m_Initialized)
            {
                Show("Please Initialize first");
                return;
            }

            m_ConsumeOnQuery = false;
            Debug.Log("Query button is clicked.");
            Show("Querying Inventory");
            StoreService.QueryInventory(productIds, m_PurchaseListener);
        });

        /*
         * Query the inventory and consume the unconsumed purchase.
         */
        GetButton("QueryConsumeButton").onClick.AddListener(() =>
        {
            if (!m_Initialized)
            {
                Show("Please Initialize first");
                return;
            }

            m_ConsumeOnQuery = true;
            Show("Querying Inventory");
            Debug.Log("QueryConsume button is clicked.");
            StoreService.QueryInventory(productIds, m_PurchaseListener);
        });

        #endregion
    }

    private Button GetButton(string buttonName)
    {
        GameObject obj = GameObject.Find(buttonName);
        if (obj != null)
        {
            return obj.GetComponent<Button>();
        }

        return null;
    }

    /// <summary>
    /// Init Listener
    /// </summary>
    public class InitListener : IInitListener
    {
        public void OnInitialized(UserInfo userInfo)
        {
            Debug.Log("[Game]On Initialized suceeded");
            Show("Initialize succeeded");
            m_Initialized = true;
        }

        public void OnInitializeFailed(string message)
        {
            Debug.Log("[Game]OnInitializeFailed: " + message);
            Show("Initialize Failed: " + message);
        }
    }

    public class LicenseCheckListener : ILicensingListener
    {
        public void allow(LicensingCode code, string message)
        {
            //LicensingCode enum:
            //RETRY, LICENSED, NOT_LICENSED, STORE_NOT_SUPPORT
            Debug.Log("license check passed: " + code + " message: " + message);
            Show(message);  //some meaningful message
        }
        public void dontAllow(LicensingCode code, string message)
        {
            //LicensingCode enum:
            //RETRY, LICENSED, NOT_LICENSED, STORE_NOT_SUPPORT
            Debug.Log("license check failed: " + code + " message: " + message);
            Show(message);  //some meaningful message
        }
        public void applicationError(LicensingErrorCode code, string message)   {
            //LicensingErrorCode enum:
            //ERROR_INVALID_PACKAGE_NAME, ERROR_NON_MATCHING_UID, ERROR_NOT_MARKET_MANAGED, ERROR_CHECK_IN_PROGRESS, ERROR_INVALID_PUBLIC_KEY, ERROR_MISSING_PERMISSION
            Debug.Log("license check error: " + code + " message: " + message);
            Show(message);  //some meaningful message
        }
    }

    /// <summary>
    /// Purchase Listener.
    /// </summary>
    public class PurchaseListener : IPurchaseListener
    {
        public void OnPurchase(PurchaseInfo purchaseInfo)
        {
            string message = string.Format(
                "[Game] Purchase Succeeded, productId: {0}, cpOrderId: {1}, developerPayload: {2}, storeJson: {3}",
                purchaseInfo.ProductId, purchaseInfo.GameOrderId, purchaseInfo.DeveloperPayload,
                purchaseInfo.StorePurchaseJsonString);

            Debug.Log(message);
            Show(message);

            /*
             * If the product is consumable, consume it and deliver the product in OnPurchaseConsume().
             * Otherwise, deliver the product here.
             */

            if (m_ConsumeOnPurchase)
            {
                Debug.Log("Consuming");
                StoreService.ConsumePurchase(purchaseInfo, this);
            }
        }

        public void OnPurchaseFailed(string message, PurchaseInfo purchaseInfo)
        {
            Debug.Log("Purchase Failed: " + message);
            Show("Purchase Failed: " + message);
        }

        public void OnPurchaseRepeated(string productCode)
        {
            throw new System.NotImplementedException();
        }

        public void OnPurchaseConsume(PurchaseInfo purchaseInfo)
        {
            Show("Consume success for " + purchaseInfo.ProductId, true);
            Debug.Log("Consume success: " + purchaseInfo.ProductId);
        }

        public void OnPurchaseConsumeFailed(string message, PurchaseInfo purchaseInfo)
        {
            Debug.Log("Consume Failed: " + message);
            Show("Consume Failed: " + message);
        }

        public void OnQueryInventory(Inventory inventory)
        {
            Debug.Log("OnQueryInventory");
            Debug.Log("[Game] Product List: ");
            string message = "Product List: \n";


            foreach (KeyValuePair<string, ProductInfo> productInfo in inventory.GetProductDictionary())
            {
                Debug.Log("[Game] Returned product: " + productInfo.Key + " " + productInfo.Value.ProductId);
                message += string.Format("{0}:\n" +
                                         "\tTitle: {1}\n" +
                                         "\tDescription: {2}\n" +
                                         "\tConsumable: {3}\n" +
                                         "\tPrice: {4}\n" +
                                         "\tCurrency: {5}\n" +
                                         "\tPriceAmountMicros: {6}\n" +
                                         "\tItemType: {7}\n",
                    productInfo.Key,
                    productInfo.Value.Title,
                    productInfo.Value.Description,
                    productInfo.Value.Consumable,
                    productInfo.Value.Price,
                    productInfo.Value.Currency,
                    productInfo.Value.PriceAmountMicros,
                    productInfo.Value.ItemType
                );
            }

            message += "\nPurchase List: \n";

            foreach (KeyValuePair<string, PurchaseInfo> purchaseInfo in inventory.GetPurchaseDictionary())
            {
                Debug.Log("[Game] Returned purchase: " + purchaseInfo.Key);
                message += string.Format("{0}\n", purchaseInfo.Value.ProductId);
            }

            Show(message);

            if (m_ConsumeOnQuery)
            {
                StoreService.ConsumePurchase(inventory.GetPurchaseList(), this);
            }
        }

        public void OnQueryInventoryFailed(string message)
        {
            Debug.Log("OnQueryInventory Failed: " + message);
            Show("QueryInventory Failed: " + message);
        }
    }
}