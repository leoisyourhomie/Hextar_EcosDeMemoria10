namespace Expiria3DSpace
{
    using UnityEngine;
    using System;
    using System.Collections.Generic;

    public class Pandora : UnityEngine.ScriptableObject
    {
        [Serializable]
        public class PandoraCategory
        {
            public string name;
            public Sprite icon;
            public List<PandoraObjectOfCategory> objects = new List<PandoraObjectOfCategory>() { new PandoraObjectOfCategory(null) };
            public List<PandoraSubcategory> subCategories = new List<PandoraSubcategory>();

            public PandoraCategory(string _name)
            {
                this.name = _name;
            }
        }

        [Serializable]
        public class PandoraSubcategory
        {
            public string name;
            public List<PandoraObjectOfCategory> objects = new List<PandoraObjectOfCategory>() { new PandoraObjectOfCategory(null) };
            public Sprite icon;

            public PandoraSubcategory(string _name)
            {
                this.name = _name;
            }
        }

        [Serializable]
        public class PandoraObjectOfCategory
        {
            public enum Type { Prefab, Obj3D, Fbx3D, Sound, Texture }
            public Type type = Type.Prefab;
            public UnityEngine.Object obj;

            public Texture2D assetPreview = null;
            public bool isUI = false;
            public bool isFavorite = false;

            public PandoraObjectOfCategory(UnityEngine.Object obj)
            {
                this.obj = obj;
            }
        }

        [Serializable]
        public class CloudStockCategory
        {
            public string id = "";
            public string name_esp = "";
            public string name_eng = "";
        }

        [Serializable]
        public class CloudStockSubCategory
        {
            public string id = "";
            public string parent_category_id = "";
            public string name_esp = "";
            public string name_eng = "";
        }

        [Serializable]
        public class CloudStockAsset
        {
            public string id = "";
            public string name_eng = "";
            public string name_esp = "";
            public string tags_eng = "";
            public string tags_esp = "";
            public string preview_images = "";
            public string animation_names = "";
            public string asset_names_to_add_in_collections = "";
            public string author = "";
            public string license = "";
            public string asset_details = "";

            private string[] imgURLsTemp = new string[] { };
            public string[] imageURLs
            {
                get
                {
                    if (imgURLsTemp != null && imgURLsTemp.Length > 0)
                        return imgURLsTemp;

                    imgURLsTemp = preview_images.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);

                    texturePreviews = new Texture2D[imgURLsTemp.Length];

                    for (int i = 0; i < imgURLsTemp.Length; i++)
                    {
                        imgURLsTemp[i] = "https://firebasestorage.googleapis.com/v0/b/ugame-studio-pandora/o/jpg_minis_8a_MNk-1l81%2F" + imgURLsTemp[i] + "?alt=media";
                    }

                    return imgURLsTemp;
                }
            }

            public bool isLoadingPreviews = false;

            public Texture2D[] texturePreviews = new Texture2D[] { };

            public void ClearTexturesToClearMemory()
            {
                texturePreviews = new Texture2D[] { };
                isLoadingPreviews = false;
                imgURLsTemp = new string[] { };
            }
        }

        [Serializable]
        public class LikedAssetsInStock
        {
            public string asset_id;

            public LikedAssetsInStock(string id)
            {
                asset_id = id;
            }
        }

        public List<PandoraCategory> categories = new List<PandoraCategory>() { new PandoraCategory("New category") };

        public Dictionary<string, Texture2D> cacheMaterialTextures = new Dictionary<string, Texture2D>();

        [HideInInspector]
        public CloudStockCategory[] cloudStockCategories = new CloudStockCategory[] { };
        [HideInInspector]
        public CloudStockSubCategory[] cloudStockSubCategories = new CloudStockSubCategory[] { };
        [HideInInspector]
        public LikedAssetsInStock[] likedAssetsInStock = new LikedAssetsInStock[] { };


        public CloudStockAsset[] cloudAssetsInView = new CloudStockAsset[] { };

        public Transform parent;
        public int boxSize = 95;
        public int materialBoxSize = 60;

        public bool showParent = false;
        public bool showNames = true;

        static Pandora reference;
        public static Pandora instance
        {
            get
            {
                if (reference == null)
                {
                    reference = (Pandora)Resources.Load("Data/Pandora", typeof(Pandora));

                    return reference;
                }
                else
                    return reference;
            }
        }
    }
}
