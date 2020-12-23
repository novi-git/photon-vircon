using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceHub.Conference
{

    [RequireComponent( typeof( Toggle ) )]
    public class CustomizationTabToggle : MonoBehaviour
    {
        public GameObject Tab;
        public CustomizationArtCategory.CategoryType Category;

        private void Awake()
        {
            var toggle = GetComponent<Toggle>();
            toggle.onValueChanged.AddListener( OnToggle );
            toggle.group = GetComponentInParent<ToggleGroup>();
        }

        private void Start()
        {
            var assets = CustomizationData.Instance.GetAllAssetsOfCategoryType( Category );
            if ( assets == null || assets.Count == 0)
            {
                OnToggle( false );
                gameObject.SetActive( false );
            }
        }

        void OnToggle( bool value )
        {
            Tab.SetActive( value );
        }
    }
}
