﻿using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace ModernWpf
{
    internal static class MergedDictionariesHelper
    {
        //public static void Add(this Collection<ResourceDictionary> mergedDictionaries, Uri source, bool useCache = true)
        //{
        //    var dictionary = GetDictionary(source, useCache);
        //    if (!mergedDictionaries.Contains(dictionary))
        //    {
        //        mergedDictionaries.Add(dictionary);
        //    }
        //}

        //public static void AddFirst(this Collection<ResourceDictionary> mergedDictionaries, Uri source, bool useCache = true)
        //{
        //    mergedDictionaries.Insert(0, source, useCache);
        //}

        //public static void Insert(this Collection<ResourceDictionary> mergedDictionaries, int index, Uri source, bool useCache = true)
        //{
        //    var dictionary = GetDictionary(source, useCache);
        //    if (!mergedDictionaries.Contains(dictionary))
        //    {
        //        mergedDictionaries.Insert(index, dictionary);
        //    }
        //}

        //public static void Remove(this Collection<ResourceDictionary> mergedDictionaries, Uri source)
        //{
        //    bool removed = false;

        //    if (ResourceDictionaryCache.TryGetDictionary(source, out ResourceDictionary dictionary))
        //    {
        //        removed = mergedDictionaries.Remove(dictionary);
        //    }

        //    if (!removed)
        //    {
        //        for (int i = mergedDictionaries.Count - 1; i >= 0; i--)
        //        {
        //            dictionary = mergedDictionaries[i];
        //            if (dictionary != null && dictionary.Source == source)
        //            {
        //                mergedDictionaries.RemoveAt(i);
        //            }
        //        }
        //    }
        //}

        public static void AddIfNotNull(this Collection<ResourceDictionary> mergedDictionaries, ResourceDictionary item)
        {
            if (item != null)
            {
                mergedDictionaries.Add(item);
            }
        }

        public static void RemoveIfNotNull(this Collection<ResourceDictionary> mergedDictionaries, ResourceDictionary item)
        {
            if (item != null)
            {
                mergedDictionaries.Remove(item);
            }
        }

        public static void InsertOrReplace(this Collection<ResourceDictionary> mergedDictionaries, int index, ResourceDictionary item)
        {
            if (mergedDictionaries.Count > index)
            {
                mergedDictionaries[0] = item;
            }
            else
            {
                mergedDictionaries.Insert(index, item);
            }
        }

        //private static ResourceDictionary GetDictionary(Uri source, bool useCache)
        //{
        //    if (useCache)
        //    {
        //        return ResourceDictionaryCache.GetOrCreateDictionary(source);
        //    }
        //    else
        //    {
        //        return new ResourceDictionary { Source = source };
        //    }
        //}
    }
}
