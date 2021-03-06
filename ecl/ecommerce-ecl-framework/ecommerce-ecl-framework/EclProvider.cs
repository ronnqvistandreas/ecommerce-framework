﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using Tridion.ExternalContentLibrary.V2;

namespace SDL.ECommerce.Ecl
{
    /// <summary>
    /// Base class for E-Commerce ECL providers.
    /// Each concrete implementation need to specify the following attribute:
    /// [AddIn("<ECL PROVIDER NAME>", Version = "<VERSION>")]
    /// </summary>
    public abstract class EclProvider : IContentLibrary
    {
        public static readonly XNamespace EcommerceEclNs = "http://sdl.com/ecl/ecommerce"; 
        private static readonly string IconBasePath = Path.Combine(AddInFolder, "Themes");

        internal static string MountPointId { get; private set; }
        public static IHostServices HostServices { get; private set; }
        public static ProductCatalog ProductCatalog { get; private set; }
        private static IDictionary<int, Category> rootCategoryMap = new Dictionary<int, Category>();

        /// <summary>
        /// Get root category
        /// </summary>
        internal static Category GetRootCategory(int publicationId) {

            Category rootCategory;
            rootCategoryMap.TryGetValue(publicationId, out rootCategory);
            if (rootCategory == null )
            {
                //lock ( EclProvider.EcommerceEclNs ) LOCK IS NOT NEEDED HERE, RIGHT?
                //{
                    rootCategory = ProductCatalog.GetAllCategories(publicationId);
                    rootCategoryMap.Add(publicationId, rootCategory);
                //}
            }
            return rootCategory;

        }

        internal static string AddInFolder
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        /// <summary>
        /// Get a specific category by its identity
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        internal static Category GetCategory(string categoryId, int publicationId)
        {
            return FindCategoryById(GetRootCategory(publicationId), categoryId);
        }

        /// <summary>
        /// Get all available catagories in a flat list
        /// </summary>
        /// <returns></returns>
        internal static List<Category> GetAllCategories(int publicationId)
        {
            var allCategories = new List<Category>();
            GetCategories(GetRootCategory(publicationId), allCategories);
            /*
            allCategories.Sort(delegate(Category x, Category y)
            {
                return x.Title.CompareTo(y.Title);
            });
            */
            return allCategories;
        }

        private static void GetCategories(Category category, List<Category> categories)
        {
            foreach (var subCategory in category.Categories)
            {
                categories.Add(subCategory);
                GetCategories(subCategory, categories);
            }
        }

        internal static List<string> GetAllCategoryIds(int publicationId)
        {
            List<string> allCategoryIds = new List<string>();
            getCategoryIds(GetRootCategory(publicationId), allCategoryIds);
            allCategoryIds.Sort();
            return allCategoryIds;
        }

        private static void getCategoryIds(Category category, List<string> categoryIds)
        {
            foreach ( var subCategory in category.Categories )
            {
                categoryIds.Add(subCategory.CategoryId);
                getCategoryIds(subCategory, categoryIds);
            }
        }

        private static Category FindCategoryById(Category category, string categoryId)
        {
            foreach ( var subCategory in category.Categories )
            {
                if (subCategory.CategoryId.Equals(categoryId)) return subCategory;
                else
                {
                    var cat = FindCategoryById(subCategory, categoryId);
                    if (cat != null) return cat;
                }
            }
            return null;
        }

        internal static byte[] GetIconImage(string iconIdentifier, int iconSize)
        {
            int actualSize;
            // get icon directly from default theme folder
            return HostServices.GetIcon(IconBasePath, "_Default", iconIdentifier, iconSize, out actualSize);
        }

        /// <summary>
        /// Initialize the ECL provider
        /// </summary>
        /// <param name="mountPointId"></param>
        /// <param name="configurationXmlElement"></param>
        /// <param name="hostServices"></param>
        public void Initialize(string mountPointId, string configurationXmlElement, IHostServices hostServices)
        {
            MountPointId = mountPointId;
            HostServices = hostServices;

            // read ExtenalContentLibrary.xml for this mountpoint
            XElement config = XElement.Parse(configurationXmlElement);

            // Initialize the product catalog with config
            //
            ProductCatalog = this.CreateProductCatalog(config);
        }

        /// <summary>
        /// Create a new instance of the product catalog. Needs to be implemented by concrete subclass.
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        protected abstract ProductCatalog CreateProductCatalog(XElement configuration);

        public abstract IContentLibraryContext CreateContext(IEclSession tridionUser);

        public IList<IDisplayType> DisplayTypes
        {
            get
            {
                return new List<IDisplayType>
                {
                    //HostServices.CreateDisplayType("type", "Catalog Type", EclItemTypes.Folder),
                    HostServices.CreateDisplayType("category", "Product Category", EclItemTypes.Folder), 
                    HostServices.CreateDisplayType("category", "Product Category", EclItemTypes.File), 
                    HostServices.CreateDisplayType("product", "Product", EclItemTypes.File)
                };
            }
        }

        public byte[] GetIconImage(string theme, string iconIdentifier, int iconSize)
        {
            return GetIconImage(iconIdentifier, iconSize);
        }

        public void Dispose()
        {
        }
    }


   
}
