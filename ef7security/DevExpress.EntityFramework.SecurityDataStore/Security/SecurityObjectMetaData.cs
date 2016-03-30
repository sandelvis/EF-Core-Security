﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DevExpress.EntityFramework.SecurityDataStore.Security {
    public class SecurityObjectMetaData {
        private object realObject;
        private object securityObject;
        public object RealObject {
            get {
                return realObject;
            }
            set {
                realObject = value;
                var invalidObjects = securityObjectRepository.resource.Where(p => p.realObject == realObject && p != this).ToList();
                foreach(var invalidObject in invalidObjects) {
                    securityObjectRepository.resource.Remove(invalidObject);
                    if(invalidObject.securityObject != null) {
                        InternalEntityEntry securityEntity = securityStateManager.Entries.FirstOrDefault(p => p.Entity == invalidObject.securityObject);
                        securityStateManager.StopTracking(securityEntity);
                    }
                }
            }
        }
        public object SecurityObject {
            get {
                return securityObject;
            }
            set {
                securityObject = value;
                var invalidObjects = securityObjectRepository.resource.Where(p => p.securityObject == securityObject && p != this).ToList();
                foreach(var invalidObject in invalidObjects) {
                    securityObjectRepository.resource.Remove(invalidObject);
                }
            }
        }
        public List<string> DenyProperties { get; set; }
            = new List<string>();
        public List<string> DenyNavigationProperties { get; set; }
            = new List<string>();
        public Dictionary<string, List<object>> DenyObjectsInListProperty { get; set; }
            = new Dictionary<string, List<object>>();
        public Dictionary<string, List<SecurityObjectMetaData>> ModifyObjectsInListProperty { get; set; }
            = new Dictionary<string, List<SecurityObjectMetaData>>();
        private Dictionary<string, object> defaultValueDictionary = new Dictionary<string, object>();
        private SecurityObjectRepository securityObjectRepository;
        public Dictionary<string, object> originalValueSecurityObjectDictionary = new Dictionary<string, object>();
        private SecurityDbContext securityDbContext;
        private IStateManager securityStateManager;

        public bool IsGrantedByReadProperty(string propertyName) {
            bool result = true;
            result = !DenyProperties.Any(p => p == propertyName);
            if(result) {
                result = !DenyNavigationProperties.Any(p => p == propertyName);
            }
            return result;
        }
        public object GetDefaultValueForProperty(string propertyName) {
            return defaultValueDictionary[propertyName];
        }
        public bool NeedModify() {
            bool result = false;
            result = DenyProperties.Count > 0;
            if(result == false) {
                result = DenyNavigationProperties.Count > 0;
            }
            if(!result) {
                foreach(string propertyName in DenyObjectsInListProperty.Keys) {
                    List<object> denyObjectInList = DenyObjectsInListProperty[propertyName];
                    if(denyObjectInList.Count > 0) {
                        result = true;
                        break;
                    }
                }
            }
            if(!result) {
                foreach(string propertyName in ModifyObjectsInListProperty.Keys) {
                    List<SecurityObjectMetaData> modifyObjectInList = ModifyObjectsInListProperty[propertyName];
                    if(modifyObjectInList.Count > 0) {
                        result = true;
                        break;
                    }
                }
            }
            return result;
        }

        public bool IsPropertyDeny(string propertyName) {
            bool result = true;
            result = DenyProperties.Contains(propertyName);
            if(!result) {
                result = DenyNavigationProperties.Contains(propertyName);
            }
            return result;
        }
        public object CreateRealObject() {
            Type targetType = SecurityObject.GetType();
            RealObject = Activator.CreateInstance(SecurityObject.GetType());
            IEntityType entityType = securityDbContext.Model.FindEntityType(targetType);
            IEnumerable<PropertyInfo> properiesInfo = targetType.GetRuntimeProperties();      
            IEnumerable<INavigation> navigations = entityType.GetNavigations();
            foreach(PropertyInfo propertyInfo in properiesInfo) {
                object defaultValue = propertyInfo.GetValue(RealObject);
                defaultValueDictionary[propertyInfo.Name] = defaultValue;
                if(navigations.Any(p => p.Name == propertyInfo.Name)) {
                    INavigation navigation = navigations.First(p => p.Name == propertyInfo.Name);
                    if(navigation.IsCollection()) {
                        IClrCollectionAccessor collectionAccessor = navigation.GetCollectionAccessor();
                        IEnumerable objectRealListProperty = (IEnumerable)propertyInfo.GetValue(RealObject);
                        IEnumerable objectSecurityListProperty = (IEnumerable)propertyInfo.GetValue(SecurityObject);
                        if(objectSecurityListProperty != null && objectRealListProperty != null) {
                            foreach(object objInList in objectSecurityListProperty) {
                                SecurityObjectMetaData securityObjectMetaDataObj = securityObjectRepository.GetSecurityObjectMetaData(objInList);
                                if(securityObjectMetaDataObj == null) {
                                    securityObjectMetaDataObj = new SecurityObjectMetaData(securityObjectRepository, securityDbContext);
                                    securityObjectRepository.RegisterObjects(securityObjectMetaDataObj);
                                    securityObjectMetaDataObj.SecurityObject = objInList;
                                    securityObjectMetaDataObj.CreateRealObject();
                                }
                                collectionAccessor.Add(RealObject, securityObjectMetaDataObj.RealObject);
                            }
                        }
                    }
                    else {
                        object realValue = propertyInfo.GetValue(SecurityObject);
                        if(!Equals(realValue, null)) {
                            SecurityObjectMetaData securityObjectMetaDataObj = securityObjectRepository.GetSecurityObjectMetaData(realValue);
                            if(securityObjectMetaDataObj == null) {
                                securityObjectMetaDataObj = new SecurityObjectMetaData(securityObjectRepository, securityDbContext);
                                securityObjectRepository.RegisterObjects(securityObjectMetaDataObj);
                                securityObjectMetaDataObj.SecurityObject = realValue;

                                securityObjectMetaDataObj.CreateRealObject();
                            }
                            propertyInfo.SetValue(RealObject, securityObjectMetaDataObj.RealObject);
                        }
                    }
                }
                else {
                    object securityValue = propertyInfo.GetValue(SecurityObject);
                    propertyInfo.SetValue(RealObject, securityValue);
                }
            }
            return RealObject;
        }
        public object CreateSecurityObject() {
            Type targetType = RealObject.GetType();
            SecurityObject = Activator.CreateInstance(RealObject.GetType());
            IEntityType entityType = securityDbContext.Model.FindEntityType(targetType);
            IEnumerable<PropertyInfo> properiesInfo = targetType.GetRuntimeProperties();
            IEnumerable<INavigation> navigations = entityType.GetNavigations();
            foreach(PropertyInfo propertyInfo in properiesInfo) {
                object defaultValue = propertyInfo.GetValue(SecurityObject);
                defaultValueDictionary[propertyInfo.Name] = defaultValue;
                if(IsPropertyDeny(propertyInfo.Name)) {
                   if(navigations.Any(p=>p.Name == propertyInfo.Name)) {
                        INavigation navigation = navigations.First(p => p.Name == propertyInfo.Name);
                        if(navigation.IsCollection()) {                           
                            propertyInfo.SetValue(SecurityObject, null);
                        }
                    }
                    continue;
                }
                if(navigations.Any(p => p.Name == propertyInfo.Name)) {
                    INavigation navigation = navigations.First(p => p.Name == propertyInfo.Name);
                    if(navigation.IsCollection()) {
                        IClrCollectionAccessor collectionAccessor = navigation.GetCollectionAccessor();
                        IEnumerable objectRealListProperty = (IEnumerable)propertyInfo.GetValue(RealObject);
                        IEnumerable objectSecurityListProperty = (IEnumerable)propertyInfo.GetValue(SecurityObject);
                        List<object> denyObject;
                        DenyObjectsInListProperty.TryGetValue(propertyInfo.Name, out denyObject);
                        foreach(object objInList in objectRealListProperty) {
                            if(denyObject != null && denyObject.Contains(objInList)) {
                                continue;
                            }
                            object objectToAdding;
                            SecurityObjectMetaData ModifyObjectInListMetaInfo = securityObjectRepository.GetSecurityObjectMetaData(objInList);
                            if(ModifyObjectInListMetaInfo != null) {
                                if(ModifyObjectInListMetaInfo.SecurityObject != null) {
                                    objectToAdding = ModifyObjectInListMetaInfo.SecurityObject;
                                }
                                else {
                                    objectToAdding = ModifyObjectInListMetaInfo.CreateSecurityObject();
                                }
                            }
                            else {
                                throw new Exception();
                            }
                            collectionAccessor.Add(SecurityObject, objectToAdding);
                        }
                    }
                    else {
                        object realValue = propertyInfo.GetValue(RealObject);
                        SecurityObjectMetaData securityObjectMetaDataObj = securityObjectRepository.GetSecurityObjectMetaData(realValue);
                        if(securityObjectMetaDataObj != null && realValue!= null) {
                            if(securityObjectMetaDataObj.SecurityObject == null) {
                                securityObjectMetaDataObj.SecurityObject = securityObjectMetaDataObj.CreateSecurityObject();
                            }
                            propertyInfo.SetValue(SecurityObject, securityObjectMetaDataObj.SecurityObject);
                        }
                        else {
                            propertyInfo.SetValue(SecurityObject, realValue);
                        }
                    }
                }
                else {
                    object realValue = propertyInfo.GetValue(RealObject);
                    propertyInfo.SetValue(SecurityObject, realValue);
                }
            }
            foreach(PropertyInfo propertyInfo in properiesInfo) {
                object originalValue = propertyInfo.GetValue(SecurityObject);
                originalValueSecurityObjectDictionary.Add(propertyInfo.Name, originalValue);
            }
            return SecurityObject;
        }
        public SecurityObjectMetaData(SecurityObjectRepository securityObjectRepository, DbContext dbContext) {
            this.securityObjectRepository = securityObjectRepository;
            this.securityDbContext = (SecurityDbContext)dbContext;
            securityStateManager = dbContext.GetService<IStateManager>();
        }
    }
}
