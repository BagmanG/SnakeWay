using UnityEngine;
using YG.Utils.Pay;
using YG;
using UnityEngine.UI;

public class WinterBiomeAds : MonoBehaviour
{
    [SerializeField] private string id = "WinterBiomePurchased";
    [SerializeField] private Text title;
    [SerializeField] private Text price;
    private void Start() => UpdateEntries(YG2.PurchaseByID(id));
    private void OnEnable() => UpdateEntries(YG2.PurchaseByID(id));
    public void UpdateEntries(Purchase data)
    {
        if (data == null)
        {
            Debug.LogError($"No product with ID found: {id}");
            return;
        }

        title.text = data.title;
        price.text = data.price;
    }
}
