﻿using System;
using System.Threading.Tasks;
using NextGenSoftware.OASIS.Common;
using NextGenSoftware.OASIS.API.Core.Enums;
using NextGenSoftware.OASIS.API.Core.Events;
using NextGenSoftware.OASIS.API.Core.Helpers;
using NextGenSoftware.OASIS.API.Core.Holons;
using NextGenSoftware.OASIS.API.Core.Interfaces;
using NextGenSoftware.OASIS.API.Core.Interfaces.STAR;
using static NextGenSoftware.OASIS.API.Core.Events.EventDelegates;

namespace NextGenSoftware.OASIS.STAR.Zomes
{
    public abstract class ZomeBase : Holon, IZomeBase
    {
        //private HolonManager _holonManager = null;
        //private const string CONST_USERMESSAGE_ID_OR_PROVIDERKEY_NOTSET = "Both Id and ProviderUniqueStorageKey are null, one of these need to be set before calling this method.";
        //private Dictionary<Guid, IOmiverse> _parentOmiverse = new Dictionary<Guid, IOmiverse>();
        //private Dictionary<Guid, IDimension> _parentDimension = new Dictionary<Guid, IDimension>();
        //private Dictionary<Guid, IMultiverse> _parentMultiverse = new Dictionary<Guid, IMultiverse>();
        //private Dictionary<Guid, IUniverse> _parentUniverse = new Dictionary<Guid, IUniverse>();
        //private Dictionary<Guid, IGalaxyCluster> _parentGalaxyCluster = new Dictionary<Guid, IGalaxyCluster>();
        //private Dictionary<Guid, IGalaxy> _parentGalaxy = new Dictionary<Guid, IGalaxy>();
        //private Dictionary<Guid, ISolarSystem> _parentSolarSystem = new Dictionary<Guid, ISolarSystem>();
        //private Dictionary<Guid, IGreatGrandSuperStar> _parentGreatGrandSuperStar = new Dictionary<Guid, IGreatGrandSuperStar>();
        //private Dictionary<Guid, IGrandSuperStar> _parentGrandSuperStar = new Dictionary<Guid, IGrandSuperStar>();
        //private Dictionary<Guid, ISuperStar> _parentSuperStar = new Dictionary<Guid, ISuperStar>();
        //private Dictionary<Guid, IStar> _parentStar = new Dictionary<Guid, IStar>();
        //private Dictionary<Guid, IPlanet> _parentPlanet = new Dictionary<Guid, IPlanet>();
        //private Dictionary<Guid, IMoon> _parentMoon = new Dictionary<Guid, IMoon>();
        //private Dictionary<Guid, ICelestialSpace> _parentCelestialSpace = new Dictionary<Guid, ICelestialSpace>();
        //private Dictionary<Guid, ICelestialBody> _parentCelestialBody = new Dictionary<Guid, ICelestialBody>();
        //private Dictionary<Guid, IZome> _parentZome = new Dictionary<Guid, IZome>();
        //private Dictionary<Guid, IHolon> _parentHolon = new Dictionary<Guid, IHolon>();
        //private Dictionary<Guid, ICelestialBodyCore> _core = new Dictionary<Guid, ICelestialBodyCore>();

        //public List<IHolon> _holons = new List<IHolon>();

        //TODO: Not sure we need this? Because this is covered by the Children property on Holon. So on a Zome the Children will be it's child holons.
        //public List<IHolon> Holons
        //{
        //    get
        //    {
        //        return _holons;
        //    }
        //    set
        //    {
        //        _holons = value;
        //    }
        //}

        //public List<IHolon> Holons
        //{
        //    get
        //    {
        //        return _holons;
        //    }
        //    set
        //    {
        //        _holons = value;
        //    }
        //}

        public event ZomeLoaded OnLoaded;
        public event ZomeSaved OnSaved;
        public event ZomeError OnError;

        //public event EventDelegates.Initialized OnInitialized;
        //public event EventDelegates.HolonLoaded OnHolonLoaded;
        //public event Events.HolonLoadedGeneric<T> OnHolonLoaded2;
        // public event EventDelegates.HolonsLoaded OnHolonsLoaded;
        //public event EventDelegates.HolonSaved OnHolonSaved;
        // public event Events.HolonSaved<T> OnHolonSaved;
        //public event EventDelegates.HolonsSaved OnHolonsSaved;
        //public event EventDelegates.ZomeSaved<T> OnSaved;

        //public event EventDelegates.HolonAdded OnHolonAdded;
        //public event EventDelegates.HolonRemoved<T> OnHolonRemoved;
        //public event EventDelegates.HolonRemoved OnHolonRemoved;
        //public event Events.ZomesLoaded OnZomesLoaded;

        ////TODO: Not sure if we want to expose the HoloNETClient events at this level? They can subscribe to them through the HoloNETClient property below...
        //public delegate void Disconnected(object sender, DisconnectedEventArgs e);
        //public event Disconnected OnDisconnected;
        //public delegate void DataReceived(object sender, DataReceivedEventArgs e);
        //public event DataReceived OnDataReceived;

        public ZomeBase(Guid id) : base(id)
        {
            Init();
        }

        public ZomeBase(string providerKey, ProviderType providerType) : base(providerKey, providerType)
        {
            Init();
        }

        //public ZomeBase(Dictionary<ProviderType, string> providerKeys) : base(providerKeys)
        //{
        //    Init();
        //}

        public ZomeBase()
        {
            Init();
        }

        private void Init()
        {
            //_holonManager = HolonManager.Instance;
            //OnInitialized?.Invoke(this, new System.EventArgs());

            //OASISResult<IOASISStorageProvider> result = OASISBootLoader.OASISBootLoader.GetAndActivateDefaultProvider();

            ////TODO: Eventually want to replace all exceptions with OASISResult throughout the OASIS because then it makes sure errors are handled properly and friendly messages are shown (plus less overhead of throwing an entire stack trace!)
            //if (result.IsError)
            //{
            //    string errorMessage = string.Concat("Error calling OASISDNAManager.GetAndActivateDefaultProvider(). Error details: ", result.Message);
            //    OASISErrorHandling.HandleError(ref result, errorMessage, true, false, true);
            //    OnError?.Invoke(this, new ZomeErrorEventArgs() { Reason = errorMessage });
            //}
            //else
            //{
            //    //TODO: Mot sure if want each Zome to have its own instance of HolonManager? Have to think about possible use cases for this?
            //    _holonManager = HolonManager.Instance;
            //    //_holonManager = new HolonManager(result.Result); //TODO: Change to use static instance instead!
            //    OnInitialized?.Invoke(this, new System.EventArgs());
            //}
        }

        public virtual async Task<OASISResult<IZome>> LoadAsync(bool loadChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, bool loadChildrenFromProvider = false, int version = 0, ProviderType providerType = ProviderType.Default)
        {
            OASISResult<IZome> result = new OASISResult<IZome>();
            string errorMessage = "Error occured in ZomeBase.LoadAsync. Reason: ";

            OASISResult<IHolon> holonResult = await base.LoadAsync(loadChildren, recursive, maxChildDepth, continueOnError, loadChildrenFromProvider, version, providerType);

            if (holonResult.IsError)
            {
                OASISErrorHandling.HandleError(ref result, $"{errorMessage}Error occured calling base.LoadAsync. Reason: {holonResult.Message}");
                OnError?.Invoke(this, new ZomeErrorEventArgs() { Result = result, Reason = result.Message, Exception = result.Exception });
            }
            else
            {
                result = OASISResultHelper.CopyResultToIZome(holonResult);
                OnSaved?.Invoke(this, new ZomeSavedEventArgs() { Result = result });
            }

            return result;
        }

        public virtual OASISResult<IZome> Load(bool loadChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, bool loadChildrenFromProvider = false, int version = 0, ProviderType providerType = ProviderType.Default)
        {
            OASISResult<IZome> result = new OASISResult<IZome>();
            string errorMessage = "Error occured in ZomeBase.Load. Reason: ";

            OASISResult<IHolon> holonResult = base.Load(loadChildren, recursive, maxChildDepth, continueOnError, loadChildrenFromProvider, version, providerType);

            if (holonResult.IsError)
            {
                OASISErrorHandling.HandleError(ref result, $"{errorMessage}Error occured calling base.Load. Reason: {holonResult.Message}");
                OnError?.Invoke(this, new ZomeErrorEventArgs() { Result = result, Reason = result != null ? result.Message : null, Exception = result != null ? result.Exception : null });
            }
            else
            {
                result = OASISResultHelper.CopyResultToIZome(holonResult);
                OnSaved?.Invoke(this, new ZomeSavedEventArgs() { Result = result });
            }

            return result;
        }

        public virtual async Task<OASISResult<T>> LoadAsync<T>(bool loadChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, bool loadChildrenFromProvider = false, int version = 0, ProviderType providerType = ProviderType.Default) where T : IHolon, new()
        {
            OASISResult<T> result = new OASISResult<T>();
            string errorMessage = "Error occured in ZomeBase.LoadAsync<T>. Reason: ";

            result = await base.LoadAsync<T>(loadChildren, recursive, maxChildDepth, continueOnError, loadChildrenFromProvider, version, providerType);

            if (result.IsError)
            {
                OASISErrorHandling.HandleError(ref result, $"{errorMessage}Error occured calling base.LoadAsync<T>. Reason: {result.Message}");
                OnError?.Invoke(this, new ZomeErrorEventArgs() { Result = OASISResultHelper.CopyResultToIZome(result), Reason = result.Message, Exception = result.Exception });
            }
            else
                OnSaved?.Invoke(this, new ZomeSavedEventArgs() { Result = OASISResultHelper.CopyResultToIZome(result) });

            return result;
        }

        public virtual OASISResult<T> Load<T>(bool loadChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, bool loadChildrenFromProvider = false, int version = 0, ProviderType providerType = ProviderType.Default) where T : IHolon, new()
        {
            OASISResult<T> result = new OASISResult<T>();
            string errorMessage = "Error occured in ZomeBase.Load<T>. Reason: ";

            result = base.Load<T>(loadChildren, recursive, maxChildDepth, continueOnError, loadChildrenFromProvider, version, providerType);

            if (result.IsError)
            {
                OASISErrorHandling.HandleError(ref result, $"{errorMessage}Error occured calling base.Load<T>. Reason: {result.Message}");
                OnError?.Invoke(this, new ZomeErrorEventArgs() { Result = OASISResultHelper.CopyResultToIZome(result), Reason = result.Message, Exception = result.Exception });
            }
            else
                OnSaved?.Invoke(this, new ZomeSavedEventArgs() { Result = OASISResultHelper.CopyResultToIZome(result) });

            return result;
        }

        public new virtual async Task<OASISResult<IZome>> SaveAsync(bool saveChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, bool saveChildrenOnProvider = false, ProviderType providerType = ProviderType.Default)
        {
            OASISResult<IZome> result = new OASISResult<IZome>((IZome)this);
            string errorMessage = "Error occured in ZomeBase.SaveAsync. Reason: ";

            OASISResult<IHolon> holonResult = await base.SaveAsync(saveChildren, recursive, maxChildDepth, continueOnError, saveChildrenOnProvider, providerType);

            if (holonResult.IsError)
            {
                OASISErrorHandling.HandleError(ref result, $"{errorMessage}Error occured calling base.SaveAsync. Reason: {holonResult.Message}");
                OnError?.Invoke(this, new ZomeErrorEventArgs() { Result = result, Reason = result.Message, Exception = result.Exception });
            }
            else
            {
                result = OASISResultHelper.CopyResultToIZome(holonResult);
                OnSaved?.Invoke(this, new ZomeSavedEventArgs() { Result = result });
            }

            return result;
        }

        public new virtual OASISResult<IZome> Save(bool saveChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, bool saveChildrenOnProvider = false, ProviderType providerType = ProviderType.Default)
        {
            OASISResult<IZome> result = new OASISResult<IZome>((IZome)this);
            string errorMessage = "Error occured in ZomeBase.Save. Reason: ";

            OASISResult<IHolon> holonResult = base.Save(saveChildren, recursive, maxChildDepth, continueOnError, saveChildrenOnProvider, providerType);

            if (holonResult.IsError)
            {
                OASISErrorHandling.HandleError(ref result, $"{errorMessage}Error occured calling base.Save. Reason: {holonResult.Message}");
                OnError?.Invoke(this, new ZomeErrorEventArgs() { Result = result, Reason = result.Message, Exception = result.Exception });
            }
            else
            {
                result = OASISResultHelper.CopyResultToIZome(holonResult);
                OnSaved?.Invoke(this, new ZomeSavedEventArgs() { Result = result });
            }

            return result;
        }

        public new virtual async Task<OASISResult<T>> SaveAsync<T>(bool saveChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, bool saveChildrenOnProvider = false, ProviderType providerType = ProviderType.Default) where T : IHolon, new()
        {
            OASISResult<T> result = new OASISResult<T>();
            string errorMessage = "Error occured in ZomeBase.SaveAsync<T>. Reason: ";

            result = await base.SaveAsync<T>(saveChildren, recursive, maxChildDepth, continueOnError, saveChildrenOnProvider, providerType);

            if (result.IsError)
            {
                OASISErrorHandling.HandleError(ref result, $"{errorMessage}Error occured calling base.SaveAsync<T>. Reason: {result.Message}");
                OnError?.Invoke(this, new ZomeErrorEventArgs() { Result = OASISResultHelper.CopyResultToIZome(result), Reason = result.Message, Exception = result.Exception });
            }
            else
                OnSaved?.Invoke(this, new ZomeSavedEventArgs() { Result = OASISResultHelper.CopyResultToIZome(result) });

            return result;
        }

        public new virtual OASISResult<T> Save<T>(bool saveChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, bool saveChildrenOnProvider = false, ProviderType providerType = ProviderType.Default) where T : IHolon, new()
        {
            OASISResult<T> result = new OASISResult<T>();
            string errorMessage = "Error occured in ZomeBase.Save<T>. Reason: ";

            result = base.Save<T>(saveChildren, recursive, maxChildDepth, continueOnError, saveChildrenOnProvider, providerType);

            if (result.IsError)
            {
                OASISErrorHandling.HandleError(ref result, $"{errorMessage}Error occured calling ZomeBase.base.Save<T>. Reason: {result.Message}");
                OnError?.Invoke(this, new ZomeErrorEventArgs() { Result = OASISResultHelper.CopyResultToIZome(result), Reason = result.Message, Exception = result.Exception });
            }
            else
                OnSaved?.Invoke(this, new ZomeSavedEventArgs() { Result =  OASISResultHelper.CopyResultToIZome(result) });

            return result;
        }

        //NOT SURE IF WE NEED ZOMEBASE TO HANDLE LOADING AND SAVING HOLONS? WHEN HOLONMANAGER OR HOLONBASE CAN MANAGE THIS FOR US.... WILL SEE... ;-)
        /*
        protected virtual async Task<OASISResult<IHolon>> LoadHolonAsync(Guid id, bool loadChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, int version = 0, ProviderType providerType = ProviderType.Default)
        {
            OASISResult<IHolon> result = await _holonManager.LoadHolonAsync(id, loadChildren, recursive, maxChildDepth, continueOnError, version, providerType);

            if (result.IsError)
                OnError?.Invoke(this, new ZomeErrorEventArgs() { Reason = string.Concat("Error in LoadHolonAsync method with id ", id, ". Error Details: ", result.Message), Exception = result.Exception });

            OnHolonLoaded?.Invoke(this, new HolonLoadedEventArgs() { Result = result });
            return result;
        }

        protected virtual OASISResult<IHolon> LoadHolon(Guid id, bool loadChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, int version = 0, ProviderType providerType = ProviderType.Default)
        {
            OASISResult<IHolon> result = _holonManager.LoadHolon(id, loadChildren, recursive, maxChildDepth, continueOnError, version, providerType);

            if (result.IsError)
                OnError?.Invoke(this, new ZomeErrorEventArgs() { Reason = string.Concat("Error in LoadHolon method with id ", id, ". Error Details: ", result.Message), Exception = result.Exception });

            OnHolonLoaded?.Invoke(this, new HolonLoadedEventArgs() { Result = result });
            return result;
        }

        protected virtual async Task<OASISResult<T>> LoadHolonAsync<T>(Guid id, bool loadChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, int version = 0, ProviderType providerType = ProviderType.Default) where T : IHolon, new()
        {
            OASISResult<T> result = await _holonManager.LoadHolonAsync<T>(id, loadChildren, recursive, maxChildDepth, continueOnError, version, providerType);

            if (result.IsError)
                OnError?.Invoke(this, new ZomeErrorEventArgs() { Reason = string.Concat("Error in ZomeBase.LoadHolonAsync method with id ", id, ". Error Details: ", result.Message), Exception = result.Exception });

            OnHolonLoaded?.Invoke(this, new HolonLoadedEventArgs() { Result = OASISResultHelperForHolons.CopyResultToIHolon(result) });
            return result;
        }

        protected virtual OASISResult<T> LoadHolon<T>(Guid id, bool loadChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, int version = 0, ProviderType providerType = ProviderType.Default) where T : IHolon, new()
        {
            OASISResult<T> result = _holonManager.LoadHolon<T>(id, loadChildren, recursive, maxChildDepth, continueOnError, version, providerType);

            if (result.IsError)
                OnError?.Invoke(this, new ZomeErrorEventArgs() { Reason = string.Concat("Error in ZomeBase.LoadHolon method with id ", id, ". Error Details: ", result.Message), Exception = result.Exception });

            OnHolonLoaded?.Invoke(this, new HolonLoadedEventArgs() { Result = OASISResultHelperForHolons.CopyResultToIHolon(result) });
            return result;
        }

        protected virtual async Task<OASISResult<IHolon>> LoadHolonAsync(ProviderType providerType, string providerKey, bool loadChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, int version = 0)
        {
            OASISResult<IHolon> result = await _holonManager.LoadHolonAsync(providerKey, loadChildren, recursive, maxChildDepth, continueOnError, version, providerType);

            if (result.IsError)
                OnError?.Invoke(this, new ZomeErrorEventArgs() { Reason = string.Concat("Error in LoadHolonAsync method with providerKey ", providerKey, ". Error Details: ", result.Message), Exception = result.Exception });

            OnHolonLoaded?.Invoke(this, new HolonLoadedEventArgs() { Result = result });
            return result;
        }

        protected virtual OASISResult<IHolon> LoadHolon(ProviderType providerType, string providerKey, bool loadChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, int version = 0)
        {
            OASISResult<IHolon> result = _holonManager.LoadHolon(providerKey, loadChildren, recursive, maxChildDepth, continueOnError, version, providerType);

            if (result.IsError)
                OnError?.Invoke(this, new ZomeErrorEventArgs() { Reason = string.Concat("Error in LoadHolon method with providerKey ", providerKey, ". Error Details: ", result.Message), Exception = result.Exception });

            OnHolonLoaded?.Invoke(this, new HolonLoadedEventArgs() { Result = result });
            return result;
        }

        protected virtual async Task<OASISResult<T>> LoadHolonAsync<T>(ProviderType providerType, string providerKey, bool loadChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, int version = 0) where T : IHolon, new()
        {
            OASISResult<T> result = await _holonManager.LoadHolonAsync<T>(providerKey, loadChildren, recursive, maxChildDepth, continueOnError, version, providerType);

            if (result.IsError)
                OnError?.Invoke(this, new ZomeErrorEventArgs() { Reason = string.Concat("Error in LoadHolonAsync method with providerKey ", providerKey, ". Error Details: ", result.Message), Exception = result.Exception });

            OnHolonLoaded?.Invoke(this, new HolonLoadedEventArgs() { Result = OASISResultHelperForHolons.CopyResultToIHolon(result) });
            return result;
        }

        protected virtual OASISResult<T> LoadHolon<T>(ProviderType providerType, string providerKey, bool loadChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, int version = 0) where T : IHolon, new()
        {
            OASISResult<T> result = _holonManager.LoadHolon<T>(providerKey, loadChildren, recursive, maxChildDepth, continueOnError, version, providerType);

            if (result.IsError)
                OnError?.Invoke(this, new ZomeErrorEventArgs() { Reason = string.Concat("Error in LoadHolonAsync method with providerKey ", providerKey, ". Error Details: ", result.Message), Exception = result.Exception });

            OnHolonLoaded?.Invoke(this, new HolonLoadedEventArgs() { Result = OASISResultHelperForHolons.CopyResultToIHolon(result) });
            return result;
        }
        */


        //protected virtual async Task<OASISResult<IEnumerable<IHolon>>> LoadAllHolonsAsync(HolonType holonType = HolonType.All, bool loadChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, bool loadChildrenFromProvider = false, int version = 0, ProviderType providerType = ProviderType.Default)
        //{
        //    OASISResult<IEnumerable<IHolon>> result = await _holonManager.LoadAllHolonsAsync(holonType, loadChildren, recursive, maxChildDepth, continueOnError, loadChildrenFromProvider, version, providerType);

        //    if (result.IsError)
        //        OnError?.Invoke(this, new ZomeErrorEventArgs() { Reason = string.Concat("Error in LoadAllHolonsAsync method with holonType ", Enum.GetName(typeof(HolonType), holonType), ". Error Details: ", result.Message), Exception = result.Exception });

        //    OnHolonsLoaded?.Invoke(this, new HolonsLoadedEventArgs() { Result = result });
        //    return result;
        //}

        //protected virtual OASISResult<IEnumerable<IHolon>> LoadAllHolons(HolonType holonType = HolonType.All, bool loadChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, bool loadChildrenFromProvider = false, int version = 0, ProviderType providerType = ProviderType.Default)
        //{
        //    OASISResult<IEnumerable<IHolon>> result = _holonManager.LoadAllHolons(holonType, loadChildren, recursive, maxChildDepth, continueOnError, loadChildrenFromProvider, version, providerType);

        //    if (result.IsError)
        //        OnError?.Invoke(this, new ZomeErrorEventArgs() { Reason = string.Concat("Error in LoadAllHolons method with holonType ", Enum.GetName(typeof(HolonType), holonType), ". Error Details: ", result.Message), Exception = result.Exception });

        //    OnHolonsLoaded?.Invoke(this, new HolonsLoadedEventArgs() { Result = result });
        //    return result;
        //}

        //protected virtual async Task<OASISResult<IEnumerable<T>>> LoadAllHolonsAsync<T>(HolonType holonType = HolonType.All, bool loadChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, bool loadChildrenFromProvider = false, int version = 0, ProviderType providerType = ProviderType.Default) where T : IHolon, new()
        //{
        //    OASISResult<IEnumerable<T>> result = await _holonManager.LoadAllHolonsAsync<T>(holonType, loadChildren, recursive, maxChildDepth, continueOnError, loadChildrenFromProvider, version, providerType);

        //    if (result.IsError)
        //        OnError?.Invoke(this, new ZomeErrorEventArgs() { Reason = string.Concat("Error in LoadAllHolonsAsync method with holonType ", Enum.GetName(typeof(HolonType), holonType), ". Error Details: ", result.Message), Exception = result.Exception });
        //    else
        //        OnHolonsLoaded?.Invoke(this, new HolonsLoadedEventArgs() { Result = OASISResultHelper.CopyResult(result) }); //OASISResultHelper<IEnumerable<T>, IEnumerable<IHolon>>.CopyResult(result) });

        //    return result;
        //}

        //protected virtual OASISResult<IEnumerable<T>> LoadAllHolons<T>(HolonType holonType = HolonType.All, bool loadChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, bool loadChildrenFromProvider = false, int version = 0, ProviderType providerType = ProviderType.Default) where T : IHolon, new()
        //{
        //    OASISResult<IEnumerable<T>> result = _holonManager.LoadAllHolons<T>(holonType, loadChildren, recursive, maxChildDepth, continueOnError, loadChildrenFromProvider, version, providerType);

        //    if (result.IsError)
        //        OnError?.Invoke(this, new ZomeErrorEventArgs() { Reason = string.Concat("Error in LoadAllHolonsAsync method with holonType ", Enum.GetName(typeof(HolonType), holonType), ". Error Details: ", result.Message), Exception = result.Exception });
        //    else
        //        OnHolonsLoaded?.Invoke(this, new HolonsLoadedEventArgs() { Result = OASISResultHelper.CopyResult(result) });

        //    return result;
        //}

        //protected virtual async Task<OASISResult<IEnumerable<IHolon>>> LoadHolonsForParentAsync(Guid id, HolonType holonType = HolonType.All, bool loadChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, bool loadChildrenFromProvider = false, int version = 0, ProviderType providerType = ProviderType.Default)
        //{
        //    OASISResult<IEnumerable<IHolon>> result = await _holonManager.LoadHolonsForParentAsync(id, holonType, loadChildren, recursive, maxChildDepth, continueOnError, loadChildrenFromProvider, version, 0, providerType);

        //    if (result.IsError)
        //        OnError?.Invoke(this, new ZomeErrorEventArgs() { Reason = string.Concat("Error in LoadHolonsForParentAsync method with id ", id, " and holonType ", Enum.GetName(typeof(HolonType), holonType), ". Error Details: ", result.Message), Exception = result.Exception });
        //    else
        //        OnHolonsLoaded?.Invoke(this, new HolonsLoadedEventArgs() { Result = result });

        //    return result;
        //}

        //protected virtual OASISResult<IEnumerable<IHolon>> LoadHolonsForParent(Guid id, HolonType holonType = HolonType.All, bool loadChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, bool loadChildrenFromProvider = false, int version = 0, ProviderType providerType = ProviderType.Default)
        //{
        //    OASISResult<IEnumerable<IHolon>> result = _holonManager.LoadHolonsForParent(id, holonType, loadChildren, recursive, maxChildDepth, continueOnError, loadChildrenFromProvider, version, 0, providerType);

        //    if (result.IsError)
        //        OnError?.Invoke(this, new ZomeErrorEventArgs() { Reason = string.Concat("Error in LoadHolonsForParent method with id ", id, " and holonType ", Enum.GetName(typeof(HolonType), holonType), ". Error Details: ", result.Message), Exception = result.Exception });

        //    OnHolonsLoaded?.Invoke(this, new HolonsLoadedEventArgs() { Result = result });
        //    return result;
        //}

        //protected virtual async Task<OASISResult<IEnumerable<T>>> LoadHolonsForParentAsync<T>(Guid id, HolonType holonType = HolonType.All, bool loadChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, bool loadChildrenFromProvider = false, int version = 0, ProviderType providerType = ProviderType.Default) where T : IHolon, new()
        //{
        //    OASISResult<IEnumerable<T>> result = await _holonManager.LoadHolonsForParentAsync<T>(id, holonType, loadChildren, recursive, maxChildDepth, continueOnError, loadChildrenFromProvider, version, 0, providerType);

        //    if (result.IsError)
        //        OnError?.Invoke(this, new ZomeErrorEventArgs() { Reason = string.Concat("Error in LoadHolonsForParentAsync method with id ", id, " and holonType ", Enum.GetName(typeof(HolonType), holonType), ". Error Details: ", result.Message), Exception = result.Exception });

        //    OnHolonsLoaded?.Invoke(this, new HolonsLoadedEventArgs() { Result = OASISResultHelper.CopyResult(result) });
        //    return result;
        //}

        //protected virtual OASISResult<IEnumerable<T>> LoadHolonsForParent<T>(Guid id, HolonType holonType = HolonType.All, bool loadChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, bool loadChildrenFromProvider = false, int version = 0, ProviderType providerType = ProviderType.Default) where T : IHolon, new()
        //{
        //    OASISResult<IEnumerable<T>> result = _holonManager.LoadHolonsForParent<T>(id, holonType, loadChildren, recursive, maxChildDepth, continueOnError, loadChildrenFromProvider, version, 0, providerType);

        //    if (result.IsError)
        //        OnError?.Invoke(this, new ZomeErrorEventArgs() { Reason = string.Concat("Error in LoadHolonsForParent method with id ", id, " and holonType ", Enum.GetName(typeof(HolonType), holonType), ". Error Details: ", result.Message), Exception = result.Exception });

        //    OnHolonsLoaded?.Invoke(this, new HolonsLoadedEventArgs() { Result = OASISResultHelper.CopyResult(result) });
        //    return result;
        //}

        //protected virtual async Task<OASISResult<IEnumerable<IHolon>>> LoadHolonsForParentAsync(ProviderType providerType, string providerKey, HolonType holonType = HolonType.All, bool loadChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, bool loadChildrenFromProvider = false, int version = 0)
        //{
        //    OASISResult<IEnumerable<IHolon>> result = await _holonManager.LoadHolonsForParentAsync(providerKey, holonType, loadChildren, recursive, maxChildDepth, continueOnError, loadChildrenFromProvider, version, 0, providerType);

        //    if (result.IsError)
        //        OnError?.Invoke(this, new ZomeErrorEventArgs() { Reason = string.Concat("Error in LoadHolonsForParentAsync method with providerKey ", providerKey, " and holonType ", Enum.GetName(typeof(HolonType), holonType), ". Error Details: ", result.Message), Exception = result.Exception });

        //    OnHolonsLoaded?.Invoke(this, new HolonsLoadedEventArgs() { Result = result });
        //    return result;
        //}

        //protected virtual OASISResult<IEnumerable<IHolon>> LoadHolonsForParent(ProviderType providerType, string providerKey, HolonType holonType = HolonType.All, bool loadChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, bool loadChildrenFromProvider = false, int version = 0)
        //{
        //    OASISResult<IEnumerable<IHolon>> result = _holonManager.LoadHolonsForParent(providerKey, holonType, loadChildren, recursive, maxChildDepth, continueOnError, loadChildrenFromProvider, version, 0, providerType);

        //    if (result.IsError)
        //        OnError?.Invoke(this, new ZomeErrorEventArgs() { Reason = string.Concat("Error in LoadHolonsForParent method with providerKey ", providerKey, " and holonType ", Enum.GetName(typeof(HolonType), holonType), ". Error Details: ", result.Message), Exception = result.Exception });

        //    OnHolonsLoaded?.Invoke(this, new HolonsLoadedEventArgs() { Result = result });
        //    return result;
        //}

        //protected virtual async Task<OASISResult<IEnumerable<T>>> LoadHolonsForParentAsync<T>(ProviderType providerType, string providerKey, HolonType holonType = HolonType.All, bool loadChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, bool loadChildrenFromProvider = false, int version = 0) where T : IHolon, new()
        //{
        //    OASISResult<IEnumerable<T>> result = await _holonManager.LoadHolonsForParentAsync<T>(providerKey, holonType, loadChildren, recursive, maxChildDepth, continueOnError, loadChildrenFromProvider, version, 0, providerType);

        //    if (result.IsError)
        //        OnError?.Invoke(this, new ZomeErrorEventArgs() { Reason = string.Concat("Error in LoadHolonsForParent method with providerKey ", providerKey, " and holonType ", Enum.GetName(typeof(HolonType), holonType), ". Error Details: ", result.Message), Exception = result.Exception });

        //    OnHolonsLoaded?.Invoke(this, new HolonsLoadedEventArgs() { Result = OASISResultHelper.CopyResult(result) });
        //    return result;
        //}

        //protected virtual OASISResult<IEnumerable<T>> LoadHolonsForParent<T>(ProviderType providerType, string providerKey, HolonType holonType = HolonType.All, bool loadChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, bool loadChildrenFromProvider = false, int version = 0) where T : IHolon, new()
        //{
        //    OASISResult<IEnumerable<T>> result = _holonManager.LoadHolonsForParent<T>(providerKey, holonType, loadChildren, recursive, maxChildDepth, continueOnError, loadChildrenFromProvider, version, 0, providerType);

        //    if (result.IsError)
        //        OnError?.Invoke(this, new ZomeErrorEventArgs() { Reason = string.Concat("Error in LoadHolonsForParent method with providerKey ", providerKey, " and holonType ", Enum.GetName(typeof(HolonType), holonType), ". Error Details: ", result.Message), Exception = result.Exception });

        //    OnHolonsLoaded?.Invoke(this, new HolonsLoadedEventArgs() { Result = OASISResultHelper.CopyResult(result) });
        //    return result;
        //}

        //public virtual async Task<OASISResult<IEnumerable<IHolon>>> LoadHolonsForParentAsync(HolonType holonType = HolonType.All, bool loadChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, int version = 0, ProviderType providerType = ProviderType.Default)
        //public virtual async Task<OASISResult<IEnumerable<IHolon>>> LoadChildHolonsAsync(HolonType holonType = HolonType.All, bool loadChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, bool loadChildrenFromProvider = false, int version = 0, ProviderType providerType = ProviderType.Default)
        //{
        //    OASISResult<IEnumerable<IHolon>> result = new OASISResult<IEnumerable<IHolon>>();

        //    if (this.Id != Guid.Empty)
        //        result = await LoadHolonsForParentAsync(Id, holonType, loadChildren, recursive, maxChildDepth, continueOnError, loadChildrenFromProvider, version, providerType);

        //    else if (this.ProviderUniqueStorageKey != null && this.ProviderUniqueStorageKey.Count > 0)
        //    {
        //        OASISResult<string> providerKeyResult = GetCurrentProviderKey(providerType);

        //        if (!providerKeyResult.IsError && !string.IsNullOrEmpty(providerKeyResult.Result))
        //            result = await LoadHolonsForParentAsync(providerType, providerKeyResult.Result, holonType, loadChildren, recursive, maxChildDepth, continueOnError, loadChildrenFromProvider, version);
        //        else
        //            OASISErrorHandling.HandleError(ref result, $"Error occured in LoadHolonAsync. Reason: {providerKeyResult.Message}", providerKeyResult.DetailedMessage);
        //    }
        //    else
        //    {
        //        result.IsError = true;
        //        result.Message = CONST_USERMESSAGE_ID_OR_PROVIDERKEY_NOTSET;
        //    }

        //    return result;
        //}

        ////public virtual OASISResult<IEnumerable<IHolon>> LoadHolonsForParent(HolonType holonType = HolonType.All, bool loadChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, int version = 0, ProviderType providerType = ProviderType.Default)
        //public virtual OASISResult<IEnumerable<IHolon>> LoadChildHolons(HolonType holonType = HolonType.All, bool loadChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, bool loadChildrenFromProvider = false, int version = 0, ProviderType providerType = ProviderType.Default)
        //{
        //    OASISResult<IEnumerable<IHolon>> result = new OASISResult<IEnumerable<IHolon>>();

        //    if (this.Id != Guid.Empty)
        //        result = LoadHolonsForParent(Id, holonType, loadChildren, recursive, maxChildDepth, continueOnError, loadChildrenFromProvider, version, providerType);

        //    else if (this.ProviderUniqueStorageKey != null && this.ProviderUniqueStorageKey.Count > 0)
        //    {
        //        OASISResult<string> providerKeyResult = GetCurrentProviderKey(providerType);

        //        if (!providerKeyResult.IsError && !string.IsNullOrEmpty(providerKeyResult.Result))
        //            result = LoadHolonsForParent(providerType, providerKeyResult.Result, holonType, loadChildren, recursive, maxChildDepth, continueOnError, loadChildrenFromProvider, version);
        //        else
        //            OASISErrorHandling.HandleError(ref result, $"Error occured in LoadHolonAsync. Reason: {providerKeyResult.Message}", providerKeyResult.DetailedMessage);
        //    }
        //    else
        //    {
        //        result.IsError = true;
        //        result.Message = CONST_USERMESSAGE_ID_OR_PROVIDERKEY_NOTSET;
        //    }

        //    return result;
        //}

        //public virtual async Task<OASISResult<IEnumerable<T>>> LoadChildHolonsAsync<T>(HolonType holonType = HolonType.All, bool loadChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, bool loadChildrenFromProvider = false, int version = 0, ProviderType providerType = ProviderType.Default) where T : IHolon, new()
        //{
        //    OASISResult<IEnumerable<T>> result = new OASISResult<IEnumerable<T>>();

        //    if (this.Id != Guid.Empty)
        //        result = await LoadHolonsForParentAsync<T>(Id, holonType, loadChildren, recursive, maxChildDepth, continueOnError, loadChildrenFromProvider, version, providerType);

        //    else if (this.ProviderUniqueStorageKey != null && this.ProviderUniqueStorageKey.Count > 0)
        //    {
        //        OASISResult<string> providerKeyResult = GetCurrentProviderKey(providerType);

        //        if (!providerKeyResult.IsError && !string.IsNullOrEmpty(providerKeyResult.Result))
        //            result = await LoadHolonsForParentAsync<T>(providerType, providerKeyResult.Result, holonType, loadChildren, recursive, maxChildDepth, continueOnError, loadChildrenFromProvider, version);
        //        else
        //            OASISErrorHandling.HandleError(ref result, $"Error occured in LoadHolonAsync. Reason: {providerKeyResult.Message}", providerKeyResult.DetailedMessage);
        //    }
        //    else
        //    {
        //        result.IsError = true;
        //        result.Message = CONST_USERMESSAGE_ID_OR_PROVIDERKEY_NOTSET;
        //    }

        //    return result;
        //}

        //public virtual OASISResult<IEnumerable<T>> LoadChildHolons<T>(HolonType holonType = HolonType.All, bool loadChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, bool loadChildrenFromProvider = false, int version = 0, ProviderType providerType = ProviderType.Default) where T : IHolon, new()
        //{
        //    OASISResult<IEnumerable<T>> result = new OASISResult<IEnumerable<T>>();

        //    if (this.Id != Guid.Empty)
        //        result = LoadHolonsForParent<T>(Id, holonType, loadChildren, recursive, maxChildDepth, continueOnError, loadChildrenFromProvider, version, providerType);

        //    else if (this.ProviderUniqueStorageKey != null && this.ProviderUniqueStorageKey.Count > 0)
        //    {
        //        OASISResult<string> providerKeyResult = GetCurrentProviderKey(providerType);

        //        if (!providerKeyResult.IsError && !string.IsNullOrEmpty(providerKeyResult.Result))
        //            result = LoadHolonsForParent<T>(providerType, providerKeyResult.Result, holonType, loadChildren, recursive, maxChildDepth, continueOnError, loadChildrenFromProvider, version);
        //        else
        //            OASISErrorHandling.HandleError(ref result, $"Error occured in LoadHolonAsync. Reason: {providerKeyResult.Message}", providerKeyResult.DetailedMessage);
        //    }
        //    else
        //    {
        //        result.IsError = true;
        //        result.Message = CONST_USERMESSAGE_ID_OR_PROVIDERKEY_NOTSET;
        //    }

        //    return result;
        //}



        //protected virtual async Task<OASISResult<IHolon>> SaveHolonAsync(IHolon savingHolon, bool mapBaseHolonProperties = true, bool saveChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, bool saveChildrenOnProvider = false, ProviderType providerType = ProviderType.Default)
        //{
        //    savingHolon = RemoveCelesialBodies(savingHolon);
        //    OASISResult<IHolon> result = await _holonManager.SaveHolonAsync(savingHolon, AvatarManager.LoggedInAvatar.Id, saveChildren, recursive, maxChildDepth, continueOnError, saveChildrenOnProvider, providerType);
        //    HandleSaveHolonResult(savingHolon, "SaveHolonAsync", ref result, mapBaseHolonProperties);
        //    return result;
        //}

        //protected virtual OASISResult<IHolon> SaveHolon(IHolon savingHolon, bool mapBaseHolonProperties = true, bool saveChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, bool saveChildrenOnProvider = false, ProviderType providerType = ProviderType.Default)
        //{
        //    savingHolon = RemoveCelesialBodies(savingHolon);
        //    OASISResult<IHolon> result = _holonManager.SaveHolon(savingHolon, AvatarManager.LoggedInAvatar.Id, saveChildren, recursive, maxChildDepth, continueOnError, saveChildrenOnProvider, providerType);
        //    HandleSaveHolonResult(savingHolon, "SaveHolon", ref result, mapBaseHolonProperties);
        //    return result;
        //}

        //protected virtual async Task<OASISResult<T>> SaveHolonAsync<T>(IHolon savingHolon, bool mapBaseHolonProperties = true, bool saveChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, bool saveChildrenOnProvider = false, ProviderType providerType = ProviderType.Default) where T : IHolon, new()
        //{
        //    savingHolon = RemoveCelesialBodies(savingHolon);
        //    OASISResult<T> result = await _holonManager.SaveHolonAsync<T>(savingHolon, AvatarManager.LoggedInAvatar.Id, saveChildren, recursive, maxChildDepth, continueOnError, saveChildrenOnProvider, providerType);
        //    HandleSaveHolonResult(savingHolon, "SaveHolonAsync<T>", ref result, mapBaseHolonProperties);
        //    return result;
        //}

        //protected virtual OASISResult<T> SaveHolon<T>(IHolon savingHolon, bool mapBaseHolonProperties = true, bool saveChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, bool saveChildrenOnProvider = false, ProviderType providerType = ProviderType.Default) where T : IHolon, new()
        //{
        //    savingHolon = RemoveCelesialBodies(savingHolon);
        //    OASISResult<T> result = _holonManager.SaveHolon<T>(savingHolon, AvatarManager.LoggedInAvatar.Id, saveChildren, recursive, maxChildDepth, continueOnError, saveChildrenOnProvider, providerType);
        //    HandleSaveHolonResult(savingHolon, "SaveHolon<T>", ref result, mapBaseHolonProperties);
        //    return result;
        //}

        //protected virtual async Task<OASISResult<IEnumerable<IHolon>>> SaveHolonsAsync(IEnumerable<IHolon> savingHolons, bool mapBaseHolonProperties = true, bool saveChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, bool saveChildrenOnProvider = false, ProviderType providerType = ProviderType.Default)
        //{
        //    if (savingHolons == null)
        //        return new OASISResult<IEnumerable<IHolon>>(savingHolons) { Message = "Holons collection is null.", IsWarning = true };

        //    if (savingHolons.Count() == 0)
        //        return new OASISResult<IEnumerable<IHolon>>(savingHolons) { Message = "Holons collection is empty.", IsWarning = true };

        //    savingHolons = RemoveCelesialBodies(savingHolons);
        //    OASISResult<IEnumerable<IHolon>> result = await _holonManager.SaveHolonsAsync(savingHolons, AvatarManager.LoggedInAvatar.Id, saveChildren, recursive, maxChildDepth, continueOnError, saveChildrenOnProvider, providerType);
        //    HandleSaveHolonsResult(savingHolons, "SaveHolonsAsync", ref result, mapBaseHolonProperties);
        //    return result;
        //}

        //protected virtual OASISResult<IEnumerable<IHolon>> SaveHolons(IEnumerable<IHolon> savingHolons, bool mapBaseHolonProperties = true, bool saveChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, bool saveChildrenOnProvider = false, ProviderType providerType = ProviderType.Default)
        //{
        //    if (savingHolons == null)
        //        return new OASISResult<IEnumerable<IHolon>>(savingHolons) { Message = "Holons collection is null.", IsWarning = true };

        //    if (savingHolons.Count() == 0)
        //        return new OASISResult<IEnumerable<IHolon>>(savingHolons) { Message = "Holons collection is empty.", IsWarning = true };

        //    savingHolons = RemoveCelesialBodies(savingHolons);
        //    OASISResult<IEnumerable<IHolon>> result = _holonManager.SaveHolons(savingHolons, AvatarManager.LoggedInAvatar.Id, saveChildren, recursive, maxChildDepth, continueOnError, saveChildrenOnProvider, providerType);
        //    HandleSaveHolonsResult(savingHolons, "SaveHolonsAsync", ref result, mapBaseHolonProperties);
        //    return result;
        //}

        //protected virtual async Task<OASISResult<IEnumerable<T>>> SaveHolonsAsync<T>(IEnumerable<T> savingHolons, bool mapBaseHolonProperties = true, bool saveChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, bool saveChildrenOnProvider = false, ProviderType providerType = ProviderType.Default) where T : IHolon, new()
        //{
        //    OASISResult<IEnumerable<T>> result = new OASISResult<IEnumerable<T>>();

        //    if (savingHolons == null)
        //        return new OASISResult<IEnumerable<T>>(savingHolons) { Message = "Holons collection is null.", IsWarning = true };

        //    if (savingHolons.Count() == 0)
        //        return new OASISResult<IEnumerable<T>>(savingHolons) { Message = "Holons collection is empty.", IsWarning = true };

        //    savingHolons = RemoveCelesialBodies(savingHolons);
        //    OASISResult<IEnumerable<T>> saveHolonResult = await _holonManager.SaveHolonsAsync<T>(savingHolons, AvatarManager.LoggedInAvatar.Id, saveChildren, recursive, maxChildDepth, continueOnError, saveChildrenOnProvider, providerType);
        //    OASISResult<IEnumerable<IHolon>> holonsResult = OASISResultHelper.CopyResult(saveHolonResult);
        //    HandleSaveHolonsResult(OASISResultHelper.CopyResult(result).Result, "SaveHolonsAsync<T>", ref holonsResult);
        //    return result;
        //}

        //protected virtual OASISResult<IEnumerable<T>> SaveHolons<T>(IEnumerable<T> savingHolons, bool mapBaseHolonProperties = true, bool saveChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, bool saveChildrenOnProvider = false, ProviderType providerType = ProviderType.Default) where T : IHolon, new()
        //{
        //    OASISResult<IEnumerable<T>> result = new OASISResult<IEnumerable<T>>();

        //    if (savingHolons == null)
        //        return new OASISResult<IEnumerable<T>>(savingHolons) { Message = "Holons collection is null.", IsWarning = true };

        //    if (savingHolons.Count() == 0)
        //        return new OASISResult<IEnumerable<T>>(savingHolons) { Message = "Holons collection is empty.", IsWarning = true };

        //    savingHolons = RemoveCelesialBodies(savingHolons);
        //    OASISResult<IEnumerable<T>> saveHolonResult = _holonManager.SaveHolons(savingHolons, AvatarManager.LoggedInAvatar.Id, saveChildren, recursive, maxChildDepth, continueOnError, saveChildrenOnProvider, providerType);

        //    OASISResult<IEnumerable<IHolon>> holonsResult = OASISResultHelper.CopyResult(saveHolonResult);
        //    HandleSaveHolonsResult(OASISResultHelper.CopyResult(result).Result, "SaveHolonsAsync<T>", ref holonsResult);
        //    return result;
        //}

        //private OASISResult<string> GetCurrentProviderKey(ProviderType providerType = ProviderType.Default)
        //{
        //    OASISResult<string> result = new OASISResult<string>();

        //    if (providerType == ProviderType.Default || providerType == ProviderType.All || providerType == ProviderType.None)
        //        providerType = ProviderManager.Instance.CurrentStorageProviderType.Value;

        //    if (ProviderUniqueStorageKey.ContainsKey(providerType) && !string.IsNullOrEmpty(ProviderUniqueStorageKey[providerType]))
        //        result.Result = ProviderUniqueStorageKey[providerType];
        //    else
        //        OASISErrorHandling.HandleError(ref result, string.Concat("ProviderUniqueStorageKey not found for CurrentStorageProviderType ", Enum.GetName(typeof(ProviderType), providerType)));

        //    return result;
        //}

        //private void GetGreatGrandSuperStar(ref OASISResult<IHolon> result, OASISResult<IEnumerable<IHolon>> holonsResult)
        //{
        //    if (!holonsResult.IsError && holonsResult.Result != null)
        //    {
        //        List<IHolon> holons = (List<IHolon>)holonsResult.Result;

        //        if (holons.Count == 1)
        //            result.Result = holons[0];
        //        else
        //        {
        //            result.IsError = true;
        //            result.Message = "ERROR, there should only be one GreatGrandSuperStar!";
        //        }
        //    }
        //}

        //private void GetGreatGrandSuperStar<T>(ref OASISResult<T> result, OASISResult<IEnumerable<T>> holonsResult)
        //{
        //    if (!holonsResult.IsError && holonsResult.Result != null)
        //    {
        //        if (holonsResult.Result.Count() == 1)
        //            result.Result = holonsResult.Result.ToList()[0];
        //        else
        //        {
        //            result.IsError = true;
        //            result.Message = "ERROR, there should only be one GreatGrandSuperStar!";
        //        }
        //    }
        //}

        //private IEnumerable<IHolon> RemoveCelesialBodies(IEnumerable<IHolon> holons)
        //{
        //    List<IHolon> holonsList = holons.ToList();

        //    for (int i = 0; i < holonsList.Count(); i++)
        //        holonsList[i] = RemoveCelesialBodies(holonsList[i]);

        //    return holonsList;
        //}

        //private IEnumerable<T> RemoveCelesialBodies<T>(IEnumerable<T> holons) where T : IHolon
        //{
        //    List<T> holonsList = holons.ToList();

        //    for (int i = 0; i < holonsList.Count(); i++)
        //        holonsList[i] = (T)RemoveCelesialBodies(holonsList[i]);

        //    return holonsList;
        //}

        //private IEnumerable<IHolon> RestoreCelesialBodies(IEnumerable<IHolon> holons)
        //{
        //    List<IHolon> restoredHolons = new List<IHolon>();

        //    foreach (IHolon holon in holons)
        //        restoredHolons.Add(RestoreCelesialBodies(holon));

        //    return restoredHolons;
        //}

        //private T RemoveCelesialBodies<T>(T holon) where T : IHolon
        //{
        //    if (holon.Id == Guid.Empty)
        //    {
        //        holon.Id = Guid.NewGuid();
        //        holon.IsNewHolon = true;
        //    }

        //    ICelestialBody celestialBody = holon as ICelestialBody;

        //    if (celestialBody != null)
        //    {
        //        _core[holon.Id] = celestialBody.CelestialBodyCore;
        //        celestialBody.CelestialBodyCore = null;
        //    }

        //    _parentOmiverse[holon.Id] = holon.ParentOmniverse;
        //    _parentDimension[holon.Id] = holon.ParentDimension;
        //    _parentMultiverse[holon.Id] = holon.ParentMultiverse;
        //    _parentUniverse[holon.Id] = holon.ParentUniverse;
        //    _parentGalaxyCluster[holon.Id] = holon.ParentGalaxyCluster;
        //    _parentGalaxy[holon.Id] = holon.ParentGalaxy;
        //    _parentSolarSystem[holon.Id] = holon.ParentSolarSystem;
        //    _parentGreatGrandSuperStar[holon.Id] = holon.ParentGreatGrandSuperStar;
        //    _parentGrandSuperStar[holon.Id] = holon.ParentGrandSuperStar;
        //    _parentSuperStar[holon.Id] = holon.ParentSuperStar;
        //    _parentStar[holon.Id] = holon.ParentStar;
        //    _parentPlanet[holon.Id] = holon.ParentPlanet;
        //    _parentMoon[holon.Id] = holon.ParentMoon;
        //    _parentCelestialSpace[holon.Id] = holon.ParentCelestialSpace;
        //    _parentCelestialBody[holon.Id] = holon.ParentCelestialBody;
        //    _parentZome[holon.Id] = holon.ParentZome;
        //    _parentHolon[holon.Id] = holon.ParentHolon;

        //    holon.ParentOmniverse = null;
        //    holon.ParentDimension = null;
        //    holon.ParentMultiverse = null;
        //    holon.ParentUniverse = null;
        //    holon.ParentGalaxyCluster = null;
        //    holon.ParentGalaxy = null;
        //    holon.ParentSolarSystem = null;
        //    holon.ParentGreatGrandSuperStar = null;
        //    holon.ParentGrandSuperStar = null;
        //    holon.ParentSuperStar = null;
        //    holon.ParentStar = null;
        //    holon.ParentPlanet = null;
        //    holon.ParentMoon = null;
        //    holon.ParentCelestialBody = null;
        //    holon.ParentCelestialSpace = null;
        //    holon.ParentZome = null;
        //    holon.ParentHolon = null;

        //    return holon;
        //}

        //private IHolon RestoreCelesialBodies(IHolon originalHolon)
        //{
        //    originalHolon.IsNewHolon = false;
        //    originalHolon.ParentOmniverse = _parentOmiverse[originalHolon.Id];
        //    originalHolon.ParentDimension = _parentDimension[originalHolon.Id];
        //    originalHolon.ParentMultiverse = _parentMultiverse[originalHolon.Id];
        //    originalHolon.ParentUniverse = _parentUniverse[originalHolon.Id];
        //    originalHolon.ParentGalaxyCluster = _parentGalaxyCluster[originalHolon.Id];
        //    originalHolon.ParentGalaxy = _parentGalaxy[originalHolon.Id];
        //    originalHolon.ParentSolarSystem = _parentSolarSystem[originalHolon.Id];
        //    originalHolon.ParentGreatGrandSuperStar = _parentGreatGrandSuperStar[originalHolon.Id];
        //    originalHolon.ParentGrandSuperStar = _parentGrandSuperStar[originalHolon.Id];
        //    originalHolon.ParentSuperStar = _parentSuperStar[originalHolon.Id];
        //    originalHolon.ParentStar = _parentStar[originalHolon.Id];
        //    originalHolon.ParentPlanet = _parentPlanet[originalHolon.Id];
        //    originalHolon.ParentMoon = _parentMoon[originalHolon.Id];
        //    originalHolon.ParentCelestialSpace = _parentCelestialSpace[originalHolon.Id];
        //    originalHolon.ParentCelestialBody = _parentCelestialBody[originalHolon.Id];
        //    originalHolon.ParentZome = _parentZome[originalHolon.Id];
        //    originalHolon.ParentHolon = _parentHolon[originalHolon.Id];

        //    _parentOmiverse.Remove(originalHolon.Id);
        //    _parentDimension.Remove(originalHolon.Id);
        //    _parentMultiverse.Remove(originalHolon.Id);
        //    _parentUniverse.Remove(originalHolon.Id);
        //    _parentGalaxyCluster.Remove(originalHolon.Id);
        //    _parentGalaxy.Remove(originalHolon.Id);
        //    _parentSolarSystem.Remove(originalHolon.Id);
        //    _parentGreatGrandSuperStar.Remove(originalHolon.Id);
        //    _parentGrandSuperStar.Remove(originalHolon.Id);
        //    _parentSuperStar.Remove(originalHolon.Id);
        //    _parentStar.Remove(originalHolon.Id);
        //    _parentPlanet.Remove(originalHolon.Id);
        //    _parentMoon.Remove(originalHolon.Id);
        //    _parentCelestialSpace.Remove(originalHolon.Id);
        //    _parentCelestialBody.Remove(originalHolon.Id);
        //    _parentZome.Remove(originalHolon.Id);
        //    _parentHolon.Remove(originalHolon.Id);

        //    ICelestialBody celestialBody = originalHolon as ICelestialBody;

        //    if (celestialBody != null)
        //    {
        //        celestialBody.CelestialBodyCore = _core[originalHolon.Id];
        //        _core.Remove(originalHolon.Id);
        //        return celestialBody;
        //    }

        //    return originalHolon;
        //}

        //private void HandleSaveHolonResult<T>(IHolon savingHolon, string callingMethodName, ref OASISResult<T> result, bool mapBaseHolonProperties = true) where T : IHolon, new()
        //{
        //    if (!result.IsError && result.Result != null)
        //    {
        //        if (mapBaseHolonProperties)
        //            Mapper<IHolon, T>.MapBaseHolonProperties(savingHolon, result.Result);

        //        result.Result = (T)RestoreCelesialBodies(savingHolon);
        //        OASISResult<IHolon> holonResult = new OASISResult<IHolon>(result.Result);
        //        OASISResultHelper.CopyResult(result, holonResult);
        //        OnHolonSaved?.Invoke(this, new HolonSavedEventArgs() { Result = holonResult });
        //    }
        //    else
        //        OnError?.Invoke(this, new ZomeErrorEventArgs() { Reason = string.Concat("Error in ", callingMethodName, " method for holon with ", LoggingHelper.GetHolonInfoForLogging(savingHolon), Enum.GetName(typeof(HolonType), savingHolon.HolonType), ". Error Details: ", result.Message), Exception = result.Exception });
        //}

        //private void HandleSaveHolonResult(IHolon savingHolon, string callingMethodName, ref OASISResult<IHolon> result, bool mapBaseHolonProperties = true)
        //{
        //    if (!result.IsError && result.Result != null)
        //    {
        //        if (mapBaseHolonProperties)
        //            Mapper.MapBaseHolonProperties(savingHolon, result.Result);

        //        result.Result = RestoreCelesialBodies(savingHolon);
        //        //result.Result = RestoreCelesialBodies(result.Result);
        //        OnHolonSaved?.Invoke(this, new HolonSavedEventArgs() { Result = result });
        //    }
        //    else
        //        OnError?.Invoke(this, new ZomeErrorEventArgs() { Reason = string.Concat("Error in ", callingMethodName, " method for holon with ", LoggingHelper.GetHolonInfoForLogging(savingHolon), Enum.GetName(typeof(HolonType), savingHolon.HolonType), ". Error Details: ", result.Message), Exception = result.Exception });
        //}

        //private void HandleSaveHolonsResult(IEnumerable<IHolon> savingHolons, string callingMethodName, ref OASISResult<IEnumerable<IHolon>> result, bool mapBaseHolonProperties = true)
        //{
        //    if (!result.IsError && result.Result != null)
        //    {
        //        //TODO: FIND OUT IF THIS IS STILL NEEDED ASAP?! THANKS! ;-)
        //        if (mapBaseHolonProperties)
        //            result.Result = Mapper.MapBaseHolonProperties(savingHolons, result.Result);

        //        result.Result = RestoreCelesialBodies(savingHolons);
        //        OnHolonsSaved?.Invoke(this, new HolonsSavedEventArgs() { Result = result });
        //    }
        //    else
        //        OnError?.Invoke(this, new ZomeErrorEventArgs() { Reason = string.Concat("Error in SaveHolonsAsync method. Error Details: ", result.Message), Exception = result.Exception });
        //}
    }
}