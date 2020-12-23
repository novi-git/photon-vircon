using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SpaceHub.Conference
{
    public class CustomizationCategoryButtons : MonoBehaviour
    {
        public GameObject ButtonPrefab;
        public CustomizationArtCategory.CategoryType Type;

        void Start()
        {
            CreateButtons();
        }

        void CreateButtons()
        {
            var assetList = CustomizationData.Instance.GetAllAssetsOfCategoryType( Type );
            //Debug.Log("Asset List: " + assetList.Count);
            //Debug.Log("Create Buttons: " + Type);  
          
            foreach( var asset in assetList )
            { 
                GameObject newButton = Instantiate( ButtonPrefab, transform );
                RectTransform rectTransform = newButton.GetComponent<RectTransform>();

                rectTransform.SetParent( transform );
                rectTransform.localPosition = Vector3.zero;
                rectTransform.localRotation = Quaternion.identity;
                rectTransform.localScale = Vector3.one;

                //Debug.Log("Asset: " + asset);
                newButton.GetComponent<CustomizationCategoryButton>().SetData( asset );
            }
        }
    }
}