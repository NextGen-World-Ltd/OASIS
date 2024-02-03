﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NextGenSoftware.OASIS.API.Core.Enums;
using NextGenSoftware.OASIS.API.Core.Interfaces;
using NextGenSoftware.OASIS.API.Core.Interfaces.STAR;
using NextGenSoftware.OASIS.API.Core.Helpers;
using NextGenSoftware.OASIS.API.Core.Events;
using static NextGenSoftware.OASIS.API.Core.Events.Events;
using NextGenSoftware.OASIS.STAR.Holons;

namespace NextGenSoftware.OASIS.STAR.CelestialSpace
{
    public abstract class CelestialSpace : CelestialHolon, ICelestialSpace
    {
        public event CelestialSpaceLoaded OnCelestialSpaceLoaded;
        public event CelestialSpaceSaved OnCelestialSpaceSaved;
        public event CelestialSpaceError OnCelestialSpaceError;
        public event CelestialSpacesLoaded OnCelestialSpacesLoaded;
        public event CelestialSpacesSaved OnCelestialSpacesSaved;
        public event CelestialSpacesError OnCelestialSpacesError;
        public event CelestialBodyLoaded OnCelestialBodyLoaded;
        public event CelestialBodySaved OnCelestialBodySaved;
        public event CelestialBodyError OnCelestialBodyError;
        public event CelestialBodiesLoaded OnCelestialBodiesLoaded;
        public event CelestialBodiesSaved OnCelestialBodiesSaved;
        public event CelestialBodiesError OnCelestialBodiesError;
        public event ZomeLoaded OnZomeLoaded;
        public event ZomeSaved OnZomeSaved;
        public event ZomeError OnZomeError;
        public event ZomesLoaded OnZomesLoaded;
        public event ZomesSaved OnZomesSaved;
        public event ZomesError OnZomesError;
        public event HolonLoaded OnHolonLoaded;
        public event HolonSaved OnHolonSaved;
        public event HolonError OnHolonError;
        public event HolonsLoaded OnHolonsLoaded;
        public event HolonsSaved OnHolonsSaved;
        public event HolonsError OnHolonsError;

        public IStar NearestStar { get; set; }
        public List<ICelestialSpace> CelestialSpaces = new List<ICelestialSpace>();
        public List<ICelestialBody> CelestialBodies = new List<ICelestialBody>();

        public CelestialSpace(HolonType holonType) : base(holonType)
        {
            Initialize();
        }

        public CelestialSpace(Guid id, HolonType holonType, bool autoLoad = true) : base(id, holonType)
        {
            Initialize(autoLoad);
        }

        public CelestialSpace(string providerKey, ProviderType providerType, HolonType holonType, bool autoLoad = true) : base(providerKey, providerType, holonType)
        {
            Initialize(autoLoad);
        }

        //public CelestialSpace(Dictionary<ProviderType, string> providerKey, HolonType holonType) : base(providerKey, holonType)
        //{
        //    Initialize();
        //}

        protected void Initialize(bool autoLoad = true)
        {
            RegisterCelestialBodies(this.CelestialBodies);
            RegisterCelestialSpaces(this.CelestialSpaces);

            if (autoLoad && !IsNewHolon && (Id != Guid.Empty || (ProviderUniqueStorageKey != null && ProviderUniqueStorageKey.Keys.Count > 0)))
            {
                OASISResult<ICelestialSpace> celestialSpaceResult = Load();

                if (celestialSpaceResult != null && !celestialSpaceResult.IsError && celestialSpaceResult.Result != null)
                    base.Initialize();
            }
        }

        protected async Task InitializeAsync(bool autoLoad = true)
        {
            RegisterCelestialBodies(this.CelestialBodies);
            RegisterCelestialSpaces(this.CelestialSpaces);

            if (autoLoad && !IsNewHolon && (Id != Guid.Empty || (ProviderUniqueStorageKey != null && ProviderUniqueStorageKey.Keys.Count > 0)))
            {
                OASISResult<ICelestialSpace> celestialSpaceResult = await LoadAsync();

                if (celestialSpaceResult != null && !celestialSpaceResult.IsError && celestialSpaceResult.Result != null)
                    await base.InitializeAsync();
            }
        }

        public async Task<OASISResult<ICelestialSpace>> LoadAsync(bool loadChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, int version = 0, ProviderType providerType = ProviderType.Default)
        {
            OASISResult<ICelestialSpace> result = new OASISResult<ICelestialSpace>();
            IStar star = GetCelestialSpaceNearestStar();

            if (star == null)
            {
                OASISErrorHandling.HandleError(ref result, $"Error occured in CelestialSpace.LoadAsync method loading the {LoggingHelper.GetHolonInfoForLogging(this, "CelestialSpace")}. Could not find the nearest star for the {LoggingHelper.GetHolonInfoForLogging(this, "CelestialSpace")}.");
                OnCelestialSpaceError?.Invoke(this, new CelestialSpaceErrorEventArgs() { Reason = $"{result.Message}", Result = result });
                return result;
            }

            OASISResult<IHolon> holonResult = await star.CelestialBodyCore.LoadHolonAsync(this.Id, loadChildren, recursive, maxChildDepth, continueOnError, version, providerType);

            if ((holonResult != null && !holonResult.IsError && holonResult.Result != null)
                || ((holonResult == null || holonResult.IsError || holonResult.Result == null) && continueOnError))
            {
                if (holonResult != null && !holonResult.IsError && holonResult.Result != null)
                    Mapper.MapBaseHolonProperties(holonResult.Result, this);
                else
                {
                    // If there was an error then continueOnError must have been set to true.
                    OASISErrorHandling.HandleWarning(ref result, $"An errror occured in CelestialSpace.LoadAsync method whilst loading the {LoggingHelper.GetHolonInfoForLogging(this, "CelestialSpace")}. ContinueOnError is set to true so continuing to attempt to load the celestial bodies... Reason: {holonResult.Message}");
                    OnCelestialSpaceError?.Invoke(this, new CelestialSpaceErrorEventArgs() { Reason = $"{result.Message}", Result = result });
                }

                if (loadChildren)
                {
                    OASISResult<IEnumerable<ICelestialBody>> celestialBodiesResult = await LoadCelestialBodiesAsync(loadChildren, recursive, maxChildDepth, continueOnError, version, providerType);

                    if (!(celestialBodiesResult != null && !celestialBodiesResult.IsError && celestialBodiesResult.Result != null))
                    {
                        if (result.IsWarning)
                            OASISErrorHandling.HandleError(ref result, $"Error occured in CelestialSpace.LoadAsync method. The {LoggingHelper.GetHolonInfoForLogging(this, "CelestialSpace")} failed to load and one or more of it's celestialbodies failed to load. Reason: {celestialBodiesResult.Message}");
                        else
                            OASISErrorHandling.HandleWarning(ref result, $"Error occured in CelestialSpace.LoadAsync method. The {LoggingHelper.GetHolonInfoForLogging(this, "CelestialSpace")} loaded fine but one or more of it's celestialbodies failed to load. Reason: {celestialBodiesResult.Message}");

                        OnCelestialSpaceError?.Invoke(this, new CelestialSpaceErrorEventArgs() { Reason = $"{result.Message}", Result = result });

                        if (!continueOnError)
                        {
                            OnCelestialSpaceLoaded?.Invoke(this, new CelestialSpaceLoadedEventArgs() { Result = result });
                            return result;
                        }
                    }

                    OASISResult<IEnumerable<ICelestialSpace>> celestialSpacesResult = await LoadCelestialSpacesAsync(loadChildren, recursive, maxChildDepth, continueOnError, version, providerType);

                    if (!(celestialSpacesResult != null && !celestialSpacesResult.IsError && celestialSpacesResult.Result != null))
                    {
                        if (result.IsWarning)
                            OASISErrorHandling.HandleError(ref result, $"Error occured in CelestialSpace.LoadAsync method. The {LoggingHelper.GetHolonInfoForLogging(this, "CelestialSpace")} failed to load and one or more of it's child celestialspaces failed to load. Reason: {celestialSpacesResult.Message}");
                        else
                            OASISErrorHandling.HandleWarning(ref result, $"Error occured in CelestialSpace.LoadAsync method. The {LoggingHelper.GetHolonInfoForLogging(this, "CelestialSpace")} loaded fine but one or more of it's child celestialspaces failed to load. Reason: {celestialSpacesResult.Message}");

                        OnCelestialSpaceError?.Invoke(this, new CelestialSpaceErrorEventArgs() { Reason = $"{result.Message}", Result = result });
                    }
                }
            }

            OnCelestialSpaceLoaded?.Invoke(this, new CelestialSpaceLoadedEventArgs() { Result = result });
            return result;
        }

        public OASISResult<ICelestialSpace> Load(bool loadChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, int version = 0, ProviderType providerType = ProviderType.Default)
        {
            return LoadAsync(loadChildren, recursive, maxChildDepth, continueOnError, version, providerType).Result;
        }

        public async Task<OASISResult<T>> LoadAsync<T>(bool loadChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, int version = 0, ProviderType providerType = ProviderType.Default) where T : ICelestialSpace, new()
        {
            return OASISResultHelperForHolons<ICelestialSpace, T>.CopyResult(await LoadAsync(loadChildren, recursive, maxChildDepth, continueOnError, version, providerType));
        }

        public OASISResult<T> Load<T>(bool loadChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, int version = 0, ProviderType providerType = ProviderType.Default) where T : ICelestialSpace, new()
        {
            return OASISResultHelperForHolons<ICelestialSpace, T>.CopyResult(Load(loadChildren, recursive, maxChildDepth, continueOnError, version, providerType));
        }

        public async Task<OASISResult<IEnumerable<ICelestialBody>>> LoadCelestialBodiesAsync(bool loadChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, int version = 0, ProviderType providerType = ProviderType.Default)
        {
            //OASISResult<ICelestialSpace> result = new OASISResult<ICelestialSpace>();
            OASISResult<IEnumerable<ICelestialBody>> result = new OASISResult<IEnumerable<ICelestialBody>>(this.CelestialBodies);
            //OASISResult<ICelestialBody> celestialBodyResult = null;
            
            //TODO: Find a way to use new generic version so can use ICelestialBody instead of IHolon.
            OASISResult<ICelestialBody> celestialBodyResult = null;

            foreach (ICelestialBody celestialBody in CelestialBodies)
            {
                //TODO: Find a way to use new generic version so can use ICelestialBody instead of IHolon.
                celestialBodyResult = await celestialBody.LoadAsync(loadChildren, recursive, maxChildDepth, continueOnError, version, providerType);

                if (celestialBodyResult != null && celestialBodyResult.Result != null && !celestialBodyResult.IsError)
                    result.LoadedCount++;
                else
                {
                    result.ErrorCount++;
                    OASISErrorHandling.HandleWarning(ref celestialBodyResult, $"There was an error in CelestialSpace.LoadCelestialBodiesAsync method whilst loading the {LoggingHelper.GetHolonInfoForLogging(celestialBody, "CelestialBody")}. Reason: {celestialBodyResult.Message}", true, false, false, true, false);

                    //TODO: Think better to just raise one error (below) rather than lots for every item?
                    //OnCelestialBodiesError?.Invoke(this, new CelestialBodiesErrorEventArgs() { Reason = $"{result.Message}", Result = result });

                    if (!continueOnError)
                        break;
                }
            }

            if (result.ErrorCount > 0)
            {
                string message = $"{result.ErrorCount} Error(s) occured in CelestialSpace.LoadCelestialBodiesAsync method loading {CelestialBodies.Count} CelestialBodies in the {LoggingHelper.GetHolonInfoForLogging(this, "CelestialSpace")}. Please check the logs and InnerMessages for more info. Reason: {OASISResultHelper.BuildInnerMessageError(result.InnerMessages)}";

                if (result.LoadedCount == 0)
                    OASISErrorHandling.HandleError(ref result, message);
                else
                {
                    OASISErrorHandling.HandleWarning(ref result, message);
                    result.IsLoaded = true;
                }

                OnCelestialBodiesError?.Invoke(this, new CelestialBodiesErrorEventArgs() { Reason = $"{result.Message}", Result = result });
            }
            else
                result.IsLoaded = true;

            OnCelestialBodiesLoaded?.Invoke(this, new CelestialBodiesLoadedEventArgs() { Result = result });
            return result;
        }

        public OASISResult<IEnumerable<ICelestialBody>> LoadCelestialBodies(bool loadChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, int version = 0, ProviderType providerType = ProviderType.Default)
        {
            return LoadCelestialBodiesAsync(loadChildren, recursive, maxChildDepth, continueOnError, version).Result;
        }

        public async Task<OASISResult<IEnumerable<T>>> LoadCelestialBodiesAsync<T>(bool loadChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, int version = 0, ProviderType providerType = ProviderType.Default) where T : ICelestialBody, new()
        {
            return OASISResultHelperForHolons<ICelestialBody, T>.CopyResult(await LoadCelestialBodiesAsync(loadChildren, recursive, maxChildDepth, continueOnError, version, providerType));
        }

        public OASISResult<IEnumerable<T>> LoadCelestialBodies<T>(bool loadChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, int version = 0, ProviderType providerType = ProviderType.Default) where T : ICelestialBody, new()
        {
            return OASISResultHelperForHolons<ICelestialBody, T>.CopyResult(LoadCelestialBodies(loadChildren, recursive, maxChildDepth, continueOnError, version, providerType));
        }

        public async Task<OASISResult<IEnumerable<ICelestialSpace>>> LoadCelestialSpacesAsync(bool loadChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, int version = 0, ProviderType providerType = ProviderType.Default)
        {
            OASISResult<IEnumerable<ICelestialSpace>> result = new OASISResult<IEnumerable<ICelestialSpace>>(this.CelestialSpaces);
            OASISResult<ICelestialSpace> celestialSpaceResult = null;

            foreach (ICelestialSpace celestialSpace in CelestialSpaces)
            {
                celestialSpaceResult = await celestialSpace.LoadAsync(loadChildren, recursive, maxChildDepth, continueOnError, version, providerType);

                if (celestialSpaceResult != null && celestialSpaceResult.Result != null && !celestialSpaceResult.IsError)
                    result.LoadedCount++;
                else
                {
                    result.ErrorCount++;
                    OASISErrorHandling.HandleWarning(ref celestialSpaceResult, $"There was an error in CelestialSpace.LoadCelestialSpacesAsync method whilst loading the {LoggingHelper.GetHolonInfoForLogging(celestialSpace, "CelestialSpace")}. Reason: {celestialSpaceResult.Message}", true, false, false, true, false);

                    //TODO: Think better to just raise one error (below) rather than lots for every item?
                    //OnCelestialBodiesError?.Invoke(this, new CelestialBodiesErrorEventArgs() { Reason = $"{result.Message}", Result = result });

                    if (!continueOnError)
                        break;
                }
            }

            if (result.ErrorCount > 0)
            {
                string message = $"{result.ErrorCount} Error(s) occured in CelestialSpace.LoadCelestialSpacesAsync method loading {CelestialSpaces.Count} CelestialSpaces in the {LoggingHelper.GetHolonInfoForLogging(this, "CelestialSpace")}. Please check the logs and InnerMessages for more info. Reason: {OASISResultHelper.BuildInnerMessageError(result.InnerMessages)}";

                if (result.LoadedCount == 0)
                    OASISErrorHandling.HandleError(ref result, message);
                else
                {
                    OASISErrorHandling.HandleWarning(ref result, message);
                    result.IsLoaded = true;
                }

                OnCelestialSpacesError?.Invoke(this, new CelestialSpacesErrorEventArgs() { Reason = $"{result.Message}", Result = result });
            }
            else
                result.IsLoaded = true;

            OnCelestialSpacesLoaded?.Invoke(this, new CelestialSpacesLoadedEventArgs() { Result = result });
            return result;
        }

        public OASISResult<IEnumerable<ICelestialSpace>> LoadCelestialSpaces(bool loadChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, int version = 0, ProviderType providerType = ProviderType.Default)
        {
            return LoadCelestialSpacesAsync(loadChildren, recursive, maxChildDepth, continueOnError, version, providerType).Result;
        }

        public async Task<OASISResult<IEnumerable<T>>> LoadCelestialSpacesAsync<T>(bool loadChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, int version = 0, ProviderType providerType = ProviderType.Default) where T : ICelestialSpace, new()
        {
            return OASISResultHelperForHolons<ICelestialSpace, T>.CopyResult(await LoadCelestialSpacesAsync(loadChildren, recursive, maxChildDepth, continueOnError, version, providerType));
        }

        public OASISResult<IEnumerable<T>> LoadCelestialSpaces<T>(bool loadChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, int version = 0, ProviderType providerType = ProviderType.Default) where T : ICelestialSpace, new()
        {
            return OASISResultHelperForHolons<ICelestialSpace, T>.CopyResult(LoadCelestialSpaces(loadChildren, recursive, maxChildDepth, continueOnError, version, providerType));
        }

        public async Task<OASISResult<ICelestialSpace>> SaveAsync(bool saveChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, ProviderType providerType = ProviderType.Default)
        {
            OASISResult<ICelestialSpace> result = new OASISResult<ICelestialSpace>();
            IsSaving = true;

            if (!STAR.IsStarIgnited)
                STAR.ShowStatusMessage(Enums.StarStatusMessageType.Processing, $"Creating CelestialSpace {this.Name}...");

            IStar star = GetCelestialSpaceNearestStar();

            if (star == null)
            {
                OASISErrorHandling.HandleError(ref result, $"Error occured in CelestialSpace.SaveAsync method saving the {LoggingHelper.GetHolonInfoForLogging(this, "CelestialSpace")}. Could not find the nearest star for the {LoggingHelper.GetHolonInfoForLogging(this, "CelestialSpace")}.");
                OnCelestialSpaceError?.Invoke(this, new CelestialSpaceErrorEventArgs() { Reason = $"{result.Message}", Result = result });
                IsSaving = false;
                return result;
            }

            OASISResult<IHolon> holonResult = await star.CelestialBodyCore.SaveHolonAsync(this, true, saveChildren, recursive, maxChildDepth, continueOnError, providerType);

            if ((holonResult != null && !holonResult.IsError && holonResult.Result != null)
                || ((holonResult == null || holonResult.IsError || holonResult.Result == null) && continueOnError))
            {
                if (!(holonResult != null && !holonResult.IsError && holonResult.Result != null))
                {
                    // If there was an error then continueOnError must have been set to true.
                    OASISErrorHandling.HandleWarning(ref result, $"An errror occured in CelestialSpace.SaveAsync method saving the {LoggingHelper.GetHolonInfoForLogging(this, "CelestialSpace")}. ContinueOnError is set to true so continuing to attempt to save the celestial bodies... Reason: {holonResult.Message}");
                    OnCelestialSpaceError?.Invoke(this, new CelestialSpaceErrorEventArgs() { Reason = $"{result.Message}", Result = result });
                }
                else
                    result.Result = (ICelestialSpace)holonResult.Result;

                if (saveChildren)
                {
                    OASISResult<IEnumerable<ICelestialBody>> celestialBodiesResult = await SaveCelestialBodiesAsync(saveChildren, recursive, maxChildDepth, continueOnError, providerType);

                    if (!(celestialBodiesResult != null && !celestialBodiesResult.IsError && celestialBodiesResult.Result != null))
                    {
                        if (result.IsWarning)
                            OASISErrorHandling.HandleError(ref result, $"Error occured in CelestialSpace.SaveAsync method. The {LoggingHelper.GetHolonInfoForLogging(this, "CelestialSpace")} failed to save and one or more of it's celestialbodies failed to save. Reason: {celestialBodiesResult.Message}");
                        else
                            OASISErrorHandling.HandleWarning(ref result, $"Error occured in CelestialSpace.SaveAsync method. The {LoggingHelper.GetHolonInfoForLogging(this, "CelestialSpace")} saved fine but one or more of it's celestialbodies failed to save. Reason: {celestialBodiesResult.Message}");

                        OnCelestialSpaceError?.Invoke(this, new CelestialSpaceErrorEventArgs() { Reason = $"{result.Message}", Result = result });

                        if (!continueOnError)
                        {
                            OnCelestialSpaceSaved?.Invoke(this, new CelestialSpaceSavedEventArgs() { Result = result });
                            IsSaving = false;
                            return result;
                        }
                    }

                    OASISResult<IEnumerable<ICelestialSpace>> celestialSpacesResult = await SaveCelestialSpacesAsync(saveChildren, recursive, maxChildDepth, continueOnError, providerType);

                    if (!(celestialSpacesResult != null && !celestialSpacesResult.IsError && celestialSpacesResult.Result != null))
                    {
                        if (result.IsWarning)
                            OASISErrorHandling.HandleError(ref result, $"Error occured in CelestialSpace.SaveAsync method. The {LoggingHelper.GetHolonInfoForLogging(this, "CelestialSpace")} failed to save and one or more of it's child celestialspaces failed to save. Reason: {celestialSpacesResult.Message}");
                        else
                            OASISErrorHandling.HandleWarning(ref result, $"Error occured in CelestialSpace.SaveAsync method. The {LoggingHelper.GetHolonInfoForLogging(this, "CelestialSpace")} saved fine but one or more of it's child celestialspaces failed to save. Reason: {celestialSpacesResult.Message}");

                        OnCelestialSpaceError?.Invoke(this, new CelestialSpaceErrorEventArgs() { Reason = $"{result.Message}", Result = result });
                    }
                }
            }

            if (result.WarningCount > 0)
            {
                if (result.SavedCount == 0)
                    OASISErrorHandling.HandleError(ref result, $"There was {result.WarningCount} error(s) in CelestialSpace.SaveAsync method whilst saving the {LoggingHelper.GetHolonInfoForLogging(this, "CelestialSpace")}. All operations failed, please check the logs and InnerMessages property for more details. Inner Messages: {OASISResultHelper.BuildInnerMessageError(result.InnerMessages)}");
                else
                    OASISErrorHandling.HandleWarning(ref result, $"There was {result.WarningCount} error(s) in CelestialSpace.SaveAsync method whilst saving the {LoggingHelper.GetHolonInfoForLogging(this, "CelestialSpace")}. {result.SavedCount} operations did save correctly however. Please check the logs and InnerMessages property for more details. Inner Messages: {OASISResultHelper.BuildInnerMessageError(result.InnerMessages)}");

                OnCelestialSpaceError?.Invoke(this, new CelestialSpaceErrorEventArgs() { Result = result });

                if (!STAR.IsStarIgnited) //TODO: Not sure why this is here?! lol NEED TO DOUBLE CHECK ASAP...
                    STAR.ShowStatusMessage(Enums.StarStatusMessageType.Error, $"Error Creating CelestialSpace {this.Name}. Reason: {result.Message}");
            }
            else
            {
                result.IsSaved = true;

                if (!STAR.IsStarIgnited) //TODO: Not sure why this is here?! lol NEED TO DOUBLE CHECK ASAP...
                    STAR.ShowStatusMessage(Enums.StarStatusMessageType.Success, $"CelestialSpace {this.Name} Created.");
            }

            IsSaving = false;
            OnCelestialSpaceSaved?.Invoke(this, new CelestialSpaceSavedEventArgs() { Result = result });
            return result;
        }

        public OASISResult<ICelestialSpace> Save(bool saveChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, ProviderType providerType = ProviderType.Default)
        {
            return SaveAsync(saveChildren, recursive, maxChildDepth, continueOnError, providerType).Result;
        }

        public async Task<OASISResult<T>> SaveAsync<T>(bool saveChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, ProviderType providerType = ProviderType.Default) where T : ICelestialSpace, new()
        {
            return OASISResultHelperForHolons<ICelestialSpace, T>.CopyResult(await SaveAsync(saveChildren, recursive, maxChildDepth, continueOnError, providerType));
        }

        public OASISResult<T> Save<T>(bool saveChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, ProviderType providerType = ProviderType.Default) where T : ICelestialSpace, new()
        {
            return OASISResultHelperForHolons<ICelestialSpace, T>.CopyResult(Save(saveChildren, recursive, maxChildDepth, continueOnError, providerType));
        }

        public async Task<OASISResult<IEnumerable<ICelestialBody>>> SaveCelestialBodiesAsync(bool saveChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, ProviderType providerType = ProviderType.Default)
        {
            OASISResult<IEnumerable<ICelestialBody>> result = new OASISResult<IEnumerable<ICelestialBody>>(this.CelestialBodies);
            IsSaving = true;
            OASISResult<ICelestialBody> celestialBodyResult = null;

            //Save all CelestialBodies contained within this space.
            foreach (ICelestialBody celestialBody in CelestialBodies)
            {
                if (!celestialBody.IsSaving)
                {
                    celestialBodyResult = await celestialBody.SaveAsync(saveChildren, recursive, maxChildDepth, continueOnError, providerType);

                    if (celestialBodyResult != null && celestialBodyResult.Result != null && !celestialBodyResult.IsError)
                        result.SavedCount++;
                    else
                    {
                        //result.ErrorCount++;
                        //OASISErrorHandling.HandleWarning(ref celestialBodyResult, $"Error occured in CelestialSpace.SaveCelestialBodiesAsync method whilst saving the {LoggingHelper.GetHolonInfoForLogging(celestialBody, "CelestialBody")}. Reason: {celestialBodyResult.Message}", true, false, false, true, false);
                        OASISErrorHandling.HandleWarning(ref celestialBodyResult, $"Error occured in CelestialSpace.SaveCelestialBodiesAsync method whilst saving the {LoggingHelper.GetHolonInfoForLogging(celestialBody, "CelestialBody")}. Reason: {celestialBodyResult.Message}");


                        //TODO: Think better to just raise one error (below) rather than lots for every item?
                        //OnCelestialBodiesError?.Invoke(this, new CelestialBodiesErrorEventArgs() { Reason = $"{result.Message}", Result = result });

                        if (!continueOnError)
                            break;
                    }
                }
            }

            //if (result.ErrorCount > 0)
            if (result.WarningCount > 0)
            {
                //string message = $"{result.ErrorCount} Error(s) occured in CelestialSpace.SaveCelestialBodiesAsync method saving {CelestialBodies.Count} CelestialBodies in the {LoggingHelper.GetHolonInfoForLogging(this, "CelestialSpace")}. Please check the logs and InnerMessages for more info. Reason: {OASISResultHelper.BuildInnerMessageError(result.InnerMessages)}";
                string message = $"{result.WarningCount} Error(s) occured in CelestialSpace.SaveCelestialBodiesAsync method saving {CelestialBodies.Count} CelestialBodies in the {LoggingHelper.GetHolonInfoForLogging(this, "CelestialSpace")}. {result.SavedCount} CelestialBodies saved successfully. Please check the logs and InnerMessages for more info. Reason: {OASISResultHelper.BuildInnerMessageError(result.InnerMessages)}";

                if (result.SavedCount == 0)
                    OASISErrorHandling.HandleError(ref result, message);
                else
                {
                    OASISErrorHandling.HandleWarning(ref result, message);
                    result.IsSaved = true;
                }

                OnCelestialBodiesError?.Invoke(this, new CelestialBodiesErrorEventArgs() { Reason = $"{result.Message}", Result = result });
            }
            else
                result.IsSaved = true;

            OnCelestialBodiesSaved?.Invoke(this, new CelestialBodiesSavedEventArgs() { Result = result });
            IsSaving = false;
            return result;
        }

        public OASISResult<IEnumerable<ICelestialBody>> SaveCelestialBodies(bool saveChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, ProviderType providerType = ProviderType.Default)
        {
            return SaveCelestialBodiesAsync(saveChildren, recursive, maxChildDepth, continueOnError, providerType).Result;
        }

        public async Task<OASISResult<IEnumerable<T>>> SaveCelestialBodiesAsync<T>(bool saveChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, ProviderType providerType = ProviderType.Default) where T : ICelestialBody, new()
        {
            return OASISResultHelperForHolons<ICelestialBody, T>.CopyResult(await SaveCelestialBodiesAsync(saveChildren, recursive, maxChildDepth, continueOnError, providerType));
        }

        public OASISResult<IEnumerable<T>> SaveCelestialBodies<T>(bool saveChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, ProviderType providerType = ProviderType.Default) where T : ICelestialBody, new()
        {
            return OASISResultHelperForHolons<ICelestialBody, T>.CopyResult(SaveCelestialBodies(saveChildren, recursive, maxChildDepth, continueOnError, providerType));
        }

        public async Task<OASISResult<IEnumerable<ICelestialSpace>>> SaveCelestialSpacesAsync(bool saveChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, ProviderType providerType = ProviderType.Default)
        {
            OASISResult<IEnumerable<ICelestialSpace>> result = new OASISResult<IEnumerable<ICelestialSpace>>(this.CelestialSpaces);
            OASISResult<ICelestialSpace> celestialSpaceResult = null;
            IsSaving = true;

            //Save all CelestialSpaces contained within this space.
            foreach (ICelestialSpace celestialSpace in CelestialSpaces)
            {
                if (!celestialSpace.IsSaving)
                {
                    celestialSpaceResult = await celestialSpace.SaveAsync(saveChildren, recursive, maxChildDepth, continueOnError, providerType);

                    if (celestialSpaceResult != null && celestialSpaceResult.Result != null && !celestialSpaceResult.IsError)
                        result.SavedCount++;
                    else
                    {
                        //result.ErrorCount++;
                        //OASISErrorHandling.HandleWarning(ref celestialSpaceResult, $"Error occured in CelestialSpace.SaveCelestialSpacesAsync method whilst saving the {LoggingHelper.GetHolonInfoForLogging(celestialSpace, "CelestialSpace")}. Reason: {celestialSpaceResult.Message}", true, false, false, true, false);
                        OASISErrorHandling.HandleWarning(ref celestialSpaceResult, $"Error occured in CelestialSpace.SaveCelestialSpacesAsync method whilst saving the {LoggingHelper.GetHolonInfoForLogging(celestialSpace, "CelestialSpace")}. Reason: {celestialSpaceResult.Message}");

                        //TODO: Think better to just raise one error (below) rather than lots for every item?
                        //OnCelestialBodiesError?.Invoke(this, new CelestialBodiesErrorEventArgs() { Reason = $"{result.Message}", Result = result });

                        if (!continueOnError)
                            break;
                    }
                }
            }

            //if (result.ErrorCount > 0)
            if (result.WarningCount > 0)
            {
                //string message = $"{result.ErrorCount} Error(s) occured in CelestialSpace.SaveCelestialSpacesAsync method saving {CelestialSpaces.Count} CelestialSpaces in the {LoggingHelper.GetHolonInfoForLogging(this, "CelestialSpace")}. {result.SavedCount} CelestialSpaces saved successfully. Please check the logs and InnerMessages for more info. Reason: {OASISResultHelper.BuildInnerMessageError(result.InnerMessages)}";
                string message = $"{result.WarningCount} Error(s) occured in CelestialSpace.SaveCelestialSpacesAsync method saving {CelestialSpaces.Count} CelestialSpaces in the {LoggingHelper.GetHolonInfoForLogging(this, "CelestialSpace")}. {result.SavedCount} CelestialSpaces saved successfully. Please check the logs and InnerMessages for more info. Reason: {OASISResultHelper.BuildInnerMessageError(result.InnerMessages)}";

                if (result.SavedCount == 0)
                    OASISErrorHandling.HandleError(ref result, message);
                else
                {
                    OASISErrorHandling.HandleWarning(ref result, message);
                    result.IsSaved = true;
                }

                OnCelestialSpacesError?.Invoke(this, new CelestialSpacesErrorEventArgs() { Reason = $"{result.Message}", Result = result });
            }
            else
                result.IsSaved = true;

            OnCelestialSpacesSaved?.Invoke(this, new CelestialSpacesSavedEventArgs() { Result = result });
            IsSaving = false;
            return result;
        }

        public OASISResult<IEnumerable<ICelestialSpace>> SaveCelestialSpaces(bool saveChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, ProviderType providerType = ProviderType.Default)
        {
            return SaveCelestialSpacesAsync(saveChildren, recursive, maxChildDepth, continueOnError, providerType).Result;
        }

        public async Task<OASISResult<IEnumerable<T>>> SaveCelestialSpacesAsync<T>(bool saveChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, ProviderType providerType = ProviderType.Default) where T : ICelestialSpace, new()
        {
            return OASISResultHelperForHolons<ICelestialSpace, T>.CopyResult(await SaveCelestialSpacesAsync(saveChildren, recursive, maxChildDepth, continueOnError, providerType));
        }

        public OASISResult<IEnumerable<T>> SaveCelestialSpaces<T>(bool saveChildren = true, bool recursive = true, int maxChildDepth = 0, bool continueOnError = true, ProviderType providerType = ProviderType.Default) where T : ICelestialSpace, new()
        {
            return OASISResultHelperForHolons<ICelestialSpace, T>.CopyResult(SaveCelestialSpaces(saveChildren, recursive, maxChildDepth, continueOnError, providerType));
        }

        protected void RegisterCelestialBodies(IEnumerable<ICelestialBody> celestialBodies, bool unregisterExistingBodiesFirst = true)
        {
            if (unregisterExistingBodiesFirst)
                UnregisterAllCelestialSpaces();

            this.CelestialBodies.AddRange(celestialBodies);

            foreach (ICelestialBody celestialBody in this.CelestialBodies)
            {
                celestialBody.OnCelestialBodyLoaded += CelestialBody_OnCelestialBodyLoaded;
                celestialBody.OnCelestialBodySaved += CelestialBody_OnCelestialBodySaved;
                celestialBody.OnCelestialBodyError += CelestialBody_OnCelestialBodyError;
                celestialBody.OnHolonLoaded += CelestialBody_OnHolonLoaded;
                celestialBody.OnHolonSaved += CelestialBody_OnHolonSaved;
                celestialBody.OnHolonError += CelestialBody_OnHolonError;
                celestialBody.OnHolonsLoaded += CelestialBody_OnHolonsLoaded;
                celestialBody.OnHolonsSaved += CelestialBody_OnHolonsSaved;
                celestialBody.OnHolonsError += CelestialBody_OnHolonsError;
                celestialBody.OnZomeLoaded += CelestialBody_OnZomeLoaded;
                celestialBody.OnZomeSaved += CelestialBody_OnZomeSaved;
                celestialBody.OnZomeError += CelestialBody_OnZomeError;
                celestialBody.OnZomesLoaded += CelestialBody_OnZomesLoaded;
                celestialBody.OnZomesSaved += CelestialBody_OnZomesSaved;
                celestialBody.OnZomesError += CelestialBody_OnZomesError;
            }
        }

        protected void RegisterCelestialSpaces(IEnumerable<ICelestialSpace> celestialSpaces, bool unregisterExistingSpacesFirst = true)
        {
            if (unregisterExistingSpacesFirst)
                UnregisterAllCelestialSpaces();

            this.CelestialSpaces.AddRange(celestialSpaces);

            foreach (ICelestialSpace celestialSpace in this.CelestialSpaces)
            {
                celestialSpace.OnCelestialSpaceLoaded += CelestialSpace_OnCelestialSpaceLoaded;
                celestialSpace.OnCelestialSpaceSaved += CelestialSpace_OnCelestialSpaceSaved;
                celestialSpace.OnCelestialSpaceError += CelestialSpace_OnCelestialSpaceError;
                celestialSpace.OnCelestialSpacesLoaded += CelestialSpace_OnCelestialSpacesLoaded;
                celestialSpace.OnCelestialSpacesSaved += CelestialSpace_OnCelestialSpacesSaved;
                celestialSpace.OnCelestialSpacesError += CelestialSpace_OnCelestialSpacesError;
                celestialSpace.OnCelestialBodyLoaded += CelestialSpace_OnCelestialBodyLoaded;
                celestialSpace.OnCelestialBodySaved += CelestialSpace_OnCelestialBodySaved;
                celestialSpace.OnCelestialBodyError += CelestialSpace_OnCelestialBodyError;
                celestialSpace.OnCelestialBodiesLoaded += CelestialSpace_OnCelestialBodiesLoaded;
                celestialSpace.OnCelestialBodiesSaved += CelestialSpace_OnCelestialBodiesSaved;
                celestialSpace.OnCelestialBodiesError += CelestialSpace_OnCelestialBodiesError;
                celestialSpace.OnHolonLoaded += CelestialSpace_OnHolonLoaded;
                celestialSpace.OnHolonSaved += CelestialSpace_OnHolonSaved;
                celestialSpace.OnHolonError += CelestialSpace_OnHolonError;
                celestialSpace.OnHolonsLoaded += CelestialSpace_OnHolonsLoaded;
                celestialSpace.OnHolonsSaved += CelestialSpace_OnHolonsSaved;
                celestialSpace.OnHolonsError += CelestialSpace_OnHolonsError;
                celestialSpace.OnZomeLoaded += CelestialSpace_OnZomeLoaded;
                celestialSpace.OnZomeSaved += CelestialSpace_OnZomeSaved;
                celestialSpace.OnZomeError += CelestialSpace_OnZomeError;
                celestialSpace.OnZomesLoaded += CelestialSpace_OnZomesLoaded;
                celestialSpace.OnZomesSaved += CelestialSpace_OnZomesSaved;
                celestialSpace.OnZomesError += CelestialSpace_OnZomesError;
            }
        }

        protected void UnregisterAllCelestialBodies()
        {
            //First unsubscibe events to prevent any memory leaks.
            foreach (ICelestialBody celestialBody in this.CelestialBodies)
            {
                celestialBody.OnCelestialBodyLoaded -= CelestialBody_OnCelestialBodyLoaded;
                celestialBody.OnCelestialBodySaved -= CelestialBody_OnCelestialBodySaved;
                celestialBody.OnCelestialBodyError -= CelestialBody_OnCelestialBodyError;
                celestialBody.OnHolonLoaded -= CelestialBody_OnHolonLoaded;
                celestialBody.OnHolonSaved -= CelestialBody_OnHolonSaved;
                celestialBody.OnHolonError -= CelestialBody_OnHolonError;
                celestialBody.OnHolonsLoaded -= CelestialBody_OnHolonsLoaded;
                celestialBody.OnHolonsSaved -= CelestialBody_OnHolonsSaved;
                celestialBody.OnHolonsError -= CelestialBody_OnHolonsError;
                celestialBody.OnZomeLoaded -= CelestialBody_OnZomeLoaded;
                celestialBody.OnZomeSaved -= CelestialBody_OnZomeSaved;
                celestialBody.OnZomeError -= CelestialBody_OnZomeError;
                celestialBody.OnZomesLoaded -= CelestialBody_OnZomesLoaded;
                celestialBody.OnZomesSaved -= CelestialBody_OnZomesSaved;
                celestialBody.OnZomesError -= CelestialBody_OnZomesError;
            }

            this.CelestialBodies = new List<ICelestialBody>();
        }

        protected void UnregisterAllCelestialSpaces()
        {
            //First unsubscibe events to prevent any memory leaks.
            foreach (ICelestialSpace celestialSpace in this.CelestialSpaces)
            {
                celestialSpace.OnCelestialSpaceLoaded -= CelestialSpace_OnCelestialSpaceLoaded;
                celestialSpace.OnCelestialSpaceSaved -= CelestialSpace_OnCelestialSpaceSaved;
                celestialSpace.OnCelestialSpaceError -= CelestialSpace_OnCelestialSpaceError;
                celestialSpace.OnCelestialSpacesLoaded -= CelestialSpace_OnCelestialSpacesLoaded;
                celestialSpace.OnCelestialSpacesSaved -= CelestialSpace_OnCelestialSpacesSaved;
                celestialSpace.OnCelestialSpacesError -= CelestialSpace_OnCelestialSpacesError;
                celestialSpace.OnCelestialBodyLoaded -= CelestialSpace_OnCelestialBodyLoaded;
                celestialSpace.OnCelestialBodySaved -= CelestialSpace_OnCelestialBodySaved;
                celestialSpace.OnCelestialBodyError -= CelestialSpace_OnCelestialBodyError;
                celestialSpace.OnCelestialBodiesLoaded -= CelestialSpace_OnCelestialBodiesLoaded;
                celestialSpace.OnCelestialBodiesSaved -= CelestialSpace_OnCelestialBodiesSaved;
                celestialSpace.OnCelestialBodiesError -= CelestialSpace_OnCelestialBodiesError;
                celestialSpace.OnHolonLoaded -= CelestialSpace_OnHolonLoaded;
                celestialSpace.OnHolonSaved -= CelestialSpace_OnHolonSaved;
                celestialSpace.OnHolonError -= CelestialSpace_OnHolonError;
                celestialSpace.OnHolonsLoaded -= CelestialSpace_OnHolonsLoaded;
                celestialSpace.OnHolonsSaved -= CelestialSpace_OnHolonsSaved;
                celestialSpace.OnHolonsError -= CelestialSpace_OnHolonsError;
                celestialSpace.OnZomeLoaded -= CelestialSpace_OnZomeLoaded;
                celestialSpace.OnZomeSaved -= CelestialSpace_OnZomeSaved;
                celestialSpace.OnZomeError -= CelestialSpace_OnZomeError;
                celestialSpace.OnZomesLoaded -= CelestialSpace_OnZomesLoaded;
                celestialSpace.OnZomesSaved -= CelestialSpace_OnZomesSaved;
                celestialSpace.OnZomesError -= CelestialSpace_OnZomesError;
            }

            this.CelestialSpaces = new List<ICelestialSpace>();
        }

        private IStar GetCelestialSpaceNearestStar()
        {
            switch (this.HolonType)
            {
                case HolonType.Omniverse:
                    NearestStar = ParentGreatGrandSuperStar;
                    break;

                case HolonType.Multiverse:
                case HolonType.Universe:
                    NearestStar = ParentGrandSuperStar;
                    break;

                case HolonType.Galaxy:
                case HolonType.GalaxyCluster:
                    NearestStar = ParentSuperStar;
                    break;

                case HolonType.SolarSystem:
                    NearestStar = ParentStar;
                    break;

                default:
                    {
                        if (this.ParentStar != null)
                            NearestStar = ParentStar;

                        else if (this.ParentSuperStar != null)
                            NearestStar = ParentSuperStar;

                        else if (this.ParentGrandSuperStar != null)
                            NearestStar = ParentGrandSuperStar;

                        else if (this.ParentGreatGrandSuperStar != null)
                            NearestStar = ParentGreatGrandSuperStar;

                        //NearestStar = null;
                        break;
                    }
            }

            return NearestStar;
        }

        private void CelestialSpace_OnCelestialSpaceLoaded(object sender, CelestialSpaceLoadedEventArgs e)
        {
            OnCelestialSpaceLoaded?.Invoke(sender, e);
        }

        private void CelestialSpace_OnCelestialSpaceSaved(object sender, CelestialSpaceSavedEventArgs e)
        {
            OnCelestialSpaceSaved?.Invoke(sender, e);
        }

        private void CelestialSpace_OnCelestialSpaceError(object sender, CelestialSpaceErrorEventArgs e)
        {
            OnCelestialSpaceError?.Invoke(sender, e);
        }

        private void CelestialSpace_OnCelestialSpacesLoaded(object sender, CelestialSpacesLoadedEventArgs e)
        {
            OnCelestialSpacesLoaded?.Invoke(sender, e);
        }

        private void CelestialSpace_OnCelestialSpacesSaved(object sender, CelestialSpacesSavedEventArgs e)
        {
            OnCelestialSpacesSaved?.Invoke(sender, e);
        }

        private void CelestialSpace_OnCelestialSpacesError(object sender, CelestialSpacesErrorEventArgs e)
        {
            OnCelestialSpacesError?.Invoke(sender, e);
        }

        private void CelestialSpace_OnCelestialBodyLoaded(object sender, CelestialBodyLoadedEventArgs e)
        {
            OnCelestialBodyLoaded?.Invoke(sender, e);
        }

        private void CelestialSpace_OnCelestialBodySaved(object sender, CelestialBodySavedEventArgs e)
        {
            OnCelestialBodySaved?.Invoke(sender, e);
        }

        private void CelestialSpace_OnCelestialBodyError(object sender, CelestialBodyErrorEventArgs e)
        {
            OnCelestialBodyError?.Invoke(sender, e);
        }

        private void CelestialSpace_OnCelestialBodiesLoaded(object sender, CelestialBodiesLoadedEventArgs e)
        {
            OnCelestialBodiesLoaded?.Invoke(sender, e);
        }

        private void CelestialSpace_OnCelestialBodiesSaved(object sender, CelestialBodiesSavedEventArgs e)
        {
            OnCelestialBodiesSaved?.Invoke(sender, e);
        }

        private void CelestialSpace_OnCelestialBodiesError(object sender, CelestialBodiesErrorEventArgs e)
        {
            OnCelestialBodiesError?.Invoke(sender, e);
        }

        private void CelestialSpace_OnZomeLoaded(object sender, ZomeLoadedEventArgs e)
        {
            OnZomeLoaded?.Invoke(sender, e);
        }

        private void CelestialSpace_OnZomeSaved(object sender, ZomeSavedEventArgs e)
        {
            OnZomeSaved?.Invoke(sender, e);
        }

        private void CelestialSpace_OnZomeError(object sender, ZomeErrorEventArgs e)
        {
            OnZomeError?.Invoke(sender, e);
        }

        private void CelestialSpace_OnZomesLoaded(object sender, ZomesLoadedEventArgs e)
        {
            OnZomesLoaded?.Invoke(sender, e);
        }

        private void CelestialSpace_OnZomesSaved(object sender, ZomesSavedEventArgs e)
        {
            OnZomesSaved?.Invoke(sender, e);
        }

        private void CelestialSpace_OnZomesError(object sender, ZomesErrorEventArgs e)
        {
            OnZomesError?.Invoke(sender, e);
        }

        private void CelestialSpace_OnHolonLoaded(object sender, HolonLoadedEventArgs e)
        {
            OnHolonLoaded?.Invoke(sender, e);
        }

        private void CelestialSpace_OnHolonSaved(object sender, HolonSavedEventArgs e)
        {
            OnHolonSaved?.Invoke(sender, e);
        }

        private void CelestialSpace_OnHolonError(object sender, HolonErrorEventArgs e)
        {
            OnHolonError?.Invoke(sender, e);
        }

        private void CelestialSpace_OnHolonsLoaded(object sender, HolonsLoadedEventArgs e)
        {
            OnHolonsLoaded?.Invoke(sender, e);
        }

        private void CelestialSpace_OnHolonsSaved(object sender, HolonsSavedEventArgs e)
        {
            OnHolonsSaved?.Invoke(sender, e);
        }

        private void CelestialSpace_OnHolonsError(object sender, HolonsErrorEventArgs e)
        {
            OnHolonsError?.Invoke(sender, e);
        }

        private void CelestialBody_OnCelestialBodyLoaded(object sender, CelestialBodyLoadedEventArgs e)
        {
            OnCelestialBodyLoaded?.Invoke(sender, e);
        }

        private void CelestialBody_OnCelestialBodySaved(object sender, CelestialBodySavedEventArgs e)
        {
            OnCelestialBodySaved?.Invoke(sender, e);
        }

        private void CelestialBody_OnCelestialBodyError(object sender, CelestialBodyErrorEventArgs e)
        {
            OnCelestialBodyError?.Invoke(sender, e);
        }

        private void CelestialBody_OnZomeLoaded(object sender, ZomeLoadedEventArgs e)
        {
            OnZomeLoaded?.Invoke(sender, e);
        }

        private void CelestialBody_OnZomeSaved(object sender, ZomeSavedEventArgs e)
        {
            OnZomeSaved?.Invoke(sender, e);
        }

        private void CelestialBody_OnZomeError(object sender, ZomeErrorEventArgs e)
        {
            OnZomeError?.Invoke(sender, e);
        }

        private void CelestialBody_OnZomesLoaded(object sender, ZomesLoadedEventArgs e)
        {
            OnZomesLoaded?.Invoke(sender, e);
        }

        private void CelestialBody_OnZomesSaved(object sender, ZomesSavedEventArgs e)
        {
            OnZomesSaved?.Invoke(sender, e);
        }

        private void CelestialBody_OnZomesError(object sender, ZomesErrorEventArgs e)
        {
            OnZomesError?.Invoke(sender, e);
        }

        private void CelestialBody_OnHolonLoaded(object sender, HolonLoadedEventArgs e)
        {
            OnHolonLoaded?.Invoke(sender, e);
        }

        private void CelestialBody_OnHolonSaved(object sender, HolonSavedEventArgs e)
        {
            OnHolonSaved?.Invoke(sender, e);
        }

        private void CelestialBody_OnHolonError(object sender, HolonErrorEventArgs e)
        {
            OnHolonError?.Invoke(sender, e);
        }

        private void CelestialBody_OnHolonsLoaded(object sender, HolonsLoadedEventArgs e)
        {
            OnHolonsLoaded?.Invoke(sender, e);
        }

        private void CelestialBody_OnHolonsSaved(object sender, HolonsSavedEventArgs e)
        {
            OnHolonsSaved?.Invoke(sender, e);
        }

        private void CelestialBody_OnHolonsError(object sender, HolonsErrorEventArgs e)
        {
            OnHolonsError?.Invoke(sender, e);
        }

        /*
        private async Task<OASISResult<ICelestialSpace>> LoadSaveCelestialBodies<T>(LoadSaveCelestialBodiesEnum loadsave, bool saveChildren = true, bool continueOnError = true) where T : ICelestialSpace, new()
        {
            OASISResult<ICelestialSpace> result = new OASISResult<ICelestialSpace>();
            OASISResult<ICelestialBody> celestialBodyResult = null;

            foreach (ICelestialBody celestialBody in CelestialBodies)
            {
                if (loadsave == LoadSaveCelestialBodiesEnum.Load)
                    celestialBodyResult = await celestialBody.LoadAsync(saveChildren, continueOnError);
                else
                    celestialBodyResult = await celestialBody.SaveAsync(saveChildren, continueOnError);

                if (!(celestialBodyResult != null && celestialBodyResult.Result != null && !celestialBodyResult.IsError))
                {
                    result.ErrorCount++;
                    string message = $"There was an error whilst saving the CelestialBody {celestialBody.Name} of type {Enum.GetName(typeof(HolonType), celestialBody.HolonType)}. Reason: {celestialBodyResult.Message}";
                    result.InnerMessages.Add(message);
                    OASISErrorHandling.HandleWarning(ref celestialBodyResult, message);

                    if (!continueOnError)
                        break;
                }
                else
                    result.SavedCount++;
            }

            if (result.ErrorCount > 0)
            {
                string message = $"{result.ErrorCount} Error(s) occured saving {CelestialBodies.Count} CelestialBodies in the CelestialSpace {this.Name} of type {Enum.GetName(typeof(HolonType), this.HolonType)}. Please check the logs and InnerMessages for more info. Reason: {OASISResultHelper.BuildInnerMessageError(result.InnerMessages)}";

                if (result.SavedCount == 0)
                    OASISErrorHandling.HandleError(ref result, message);
                else
                {
                    OASISErrorHandling.HandleWarning(ref result, message);
                    result.IsSaved = true;
                }
            }
            else
                result.IsSaved = true;

            //base.OnCelestialHolonSaved?.Invoke(this, new System.EventArgs());
            return result;
        }*/

        //private void HandleResult()
        //{
        //    if (!(celestialBodyResult != null && celestialBodyResult.Result != null && !celestialBodyResult.IsError))
        //    {
        //        result.ErrorCount++;
        //        string message = $"There was an error whilst saving the CelestialBody {celestialBody.Name} of type {Enum.GetName(typeof(HolonType), celestialBody.HolonType)}. Reason: {celestialBodyResult.Message}";
        //        result.InnerMessages.Add(message);
        //        OASISErrorHandling.HandleWarning(ref celestialBodyResult, message);

        //        if (!continueOnError)
        //            break;
        //    }
        //    else
        //        result.SavedCount++;
        //}
    }
}